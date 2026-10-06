using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Business Logic สำหรับจัดการ Category ของ Knowledge Base (MOD-006)
    /// ครอบ IKBCategoryRepository และเพิ่ม Validation + Audit Logging
    /// </summary>
    public class KBCategoryService : IKBCategoryService
    {
        private readonly IKBCategoryRepository _repo;
        private readonly IAuditService         _auditService;

        public KBCategoryService(
            IKBCategoryRepository repo,
            IAuditService         auditService)
        {
            if (repo         == null) throw new ArgumentNullException("repo");
            if (auditService == null) throw new ArgumentNullException("auditService");
            _repo         = repo;
            _auditService = auditService;
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public IEnumerable<KBCategory> GetAll()
        {
            return _repo.GetAll();
        }

        public IEnumerable<KBCategory> GetAllActive()
        {
            return _repo.GetAllActive();
        }

        public KBCategory GetById(int categoryId)
        {
            return _repo.GetById(categoryId);
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public int Create(KBCategory category)
        {
            if (category == null)
                throw new ArgumentNullException("category");
            if (string.IsNullOrWhiteSpace(category.CategoryCode))
                throw new ArgumentException("CategoryCode ต้องไม่ว่าง");
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                throw new ArgumentException("CategoryName ต้องไม่ว่าง");

            // ป้องกัน self-reference ก่อน insert
            if (category.ParentCategoryID.HasValue && category.ParentCategoryID.Value == category.CategoryID)
                throw new InvalidOperationException("ไม่สามารถตั้ง Parent ให้ตัวเองได้");

            NormalizeAndValidateImportTypeDigit(category);

            int newId = _repo.Insert(category);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "CREATE_KB_CATEGORY",
                UserID    = 0,          // ถูก override โดย caller ถ้าต้องการ
                UserName  = category.CreatedBy ?? string.Empty,
                TableName = "KMS.m_KBCategory",
                RecordID  = newId,
                AuditDate = DateTime.Now,
                Result    = 1
            });

            return newId;
        }

        public void Update(KBCategory category)
        {
            if (category == null)
                throw new ArgumentNullException("category");
            if (string.IsNullOrWhiteSpace(category.CategoryCode))
                throw new ArgumentException("CategoryCode ต้องไม่ว่าง");
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                throw new ArgumentException("CategoryName ต้องไม่ว่าง");

            // ป้องกัน self-reference
            if (category.ParentCategoryID.HasValue && category.ParentCategoryID.Value == category.CategoryID)
                throw new InvalidOperationException("ไม่สามารถตั้ง Parent ให้ตัวเองได้");

            // ป้องกันตั้ง Parent เป็นหมวดย่อยของตัวเอง (ต้นไม้หมวดลึกกี่ชั้นก็ได้ จึงวนลูปได้)
            if (category.ParentCategoryID.HasValue &&
                KBCategoryTree.GetDescendantIds(_repo.GetAll(), category.CategoryID).Contains(category.ParentCategoryID.Value))
                throw new InvalidOperationException("ไม่สามารถตั้ง Parent เป็นหมวดย่อยของตัวเองได้");

            NormalizeAndValidateImportTypeDigit(category);

            // DisplayProfileID ไม่ได้แก้ที่หน้านี้ (ผูกจากหน้า config Profile → KBArticleDisplayProfileService.AssignProfileToCategory) — คงค่าที่เก็บไว้
            // ไม่งั้นค่าที่ caller ไม่ได้ส่งมา (null) จะเขียนทับ ทำให้ Profile ที่ผูกกับหมวดหายทุกครั้งที่แก้หมวด (อ่านสด ไม่ใช้ค่าที่หน้าจอโหลดไว้ตอนเลือกหมวด)
            var stored = _repo.GetById(category.CategoryID);
            category.DisplayProfileID = stored != null ? stored.DisplayProfileID : null;

            _repo.Update(category);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "UPDATE_KB_CATEGORY",
                UserID    = 0,
                TableName = "KMS.m_KBCategory",
                RecordID  = category.CategoryID,
                AuditDate = DateTime.Now,
                Result    = 1
            });
        }

        /// <summary>
        /// ตรวจ "รหัสประเภทเอกสาร" (<see cref="KBCategory.ImportTypeDigit"/> = ตัวที่ 25 ของ Code รวมตอน import ไฟล์ที่ไม่ใช่กฎหมาย):
        /// ว่าง = ไม่ใช่หมวดประเภทเอกสาร (เก็บเป็น null); ไม่ว่างต้องเป็นตัวเลข 1-9 ตัวเดียว, ห้ามซ้ำกับหมวดอื่น (1 รหัส = 1 ประเภท)
        /// และห้ามเป็นหมวด "กฎหมาย" หรือหมวดที่อยู่ใต้กฎหมาย (ไฟล์กฎหมายไม่ใช้ตัวที่ 25 แยกประเภท). ปรับค่าที่ผ่านแล้วให้เป็นรูปมาตรฐาน
        /// </summary>
        private void NormalizeAndValidateImportTypeDigit(KBCategory category)
        {
            if (string.IsNullOrWhiteSpace(category.ImportTypeDigit))
            {
                category.ImportTypeDigit = null;
                return;
            }

            char digit;
            if (!KBImportLayoutTypeCatalog.TryParseTypeDigit(category.ImportTypeDigit, out digit))
                throw new ArgumentException(string.Format("รหัสประเภทเอกสารต้องเป็นตัวเลข {0}-{1} ตัวเดียว",
                    KBImportLayoutTypeCatalog.MinTypeDigit, KBImportLayoutTypeCatalog.MaxTypeDigit));

            category.ImportTypeDigit = digit.ToString();

            var all = _repo.GetAll().ToList();

            var owner = all.FirstOrDefault(c => c.CategoryID != category.CategoryID && c.ImportTypeDigit != null
                                                && c.ImportTypeDigit.Trim() == category.ImportTypeDigit);
            if (owner != null)
                throw new InvalidOperationException(string.Format(
                    "รหัสประเภทเอกสาร {0} ถูกใช้แล้วโดยหมวด \"{1}\" ({2}) — 1 รหัสใช้ได้กับ 1 หมวดเท่านั้น",
                    digit, owner.CategoryName, (owner.CategoryCode ?? string.Empty).Trim()));

            if (IsLawRootOrUnder(category, all))
                throw new InvalidOperationException(
                    "หมวดกฎหมาย (LW) และหมวดที่อยู่ใต้กฎหมายตั้งรหัสประเภทเอกสารไม่ได้ — รหัสนี้ใช้กับประเภทเอกสารที่ไม่ใช่กฎหมายเท่านั้น");
        }

        /// <summary>true ถ้า <paramref name="category"/> เป็นหมวด "กฎหมาย" (LW) เอง หรืออยู่ใต้หมวดนั้น (ไล่ ParentCategoryID ขึ้นไป มีตัวกันวนลูป)</summary>
        private static bool IsLawRootOrUnder(KBCategory category, IList<KBCategory> all)
        {
            var byId = all.GroupBy(c => c.CategoryID).ToDictionary(g => g.Key, g => g.First());
            var visited = new HashSet<int>();

            KBCategory current = category;
            while (current != null)
            {
                if (string.Equals((current.CategoryCode ?? string.Empty).Trim(),
                                  KBImportLayoutTypeCatalog.LawRootCategoryCode, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!current.ParentCategoryID.HasValue || !visited.Add(current.ParentCategoryID.Value))
                    return false;

                KBCategory parent;
                current = byId.TryGetValue(current.ParentCategoryID.Value, out parent) ? parent : null;
            }
            return false;
        }

        public void Deactivate(int categoryId)
        {
            _repo.SoftDelete(categoryId);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "DEACTIVATE_KB_CATEGORY",
                UserID    = 0,
                TableName = "KMS.m_KBCategory",
                RecordID  = categoryId,
                AuditDate = DateTime.Now,
                Result    = 1
            });
        }
    }
}
