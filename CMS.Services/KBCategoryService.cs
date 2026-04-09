using System;
using System.Collections.Generic;
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
