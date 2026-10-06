using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// ย้ายบทความ KMS ไปหมวดอื่น: เปลี่ยนเฉพาะ CategoryID (+UpdatedBy/UpdatedDate) ไม่แตะเนื้อหา/version/สถานะ.
    /// ปลายทางต้องเป็นหมวดที่มีอยู่และเปิดใช้งาน; กรณีเสี่ยง (ข้ามกฎหมาย↔ไม่ใช่กฎหมาย, หมวดแม่ที่แยกหมวดปี ฯลฯ) คืนเป็นคำเตือนให้ผู้ใช้ยืนยัน ไม่บล็อก
    /// </summary>
    public class KBArticleMoveService : IKBArticleMoveService
    {
        private readonly IKBArticleRepository  _articleRepo;
        private readonly IKBCategoryRepository _categoryRepo;
        private readonly IAuditService         _auditService;

        public KBArticleMoveService(
            IKBArticleRepository  articleRepo,
            IKBCategoryRepository categoryRepo,
            IAuditService         auditService)
        {
            if (articleRepo   == null) throw new ArgumentNullException("articleRepo");
            if (categoryRepo  == null) throw new ArgumentNullException("categoryRepo");
            if (auditService  == null) throw new ArgumentNullException("auditService");
            _articleRepo  = articleRepo;
            _categoryRepo = categoryRepo;
            _auditService = auditService;
        }

        public IList<KBArticle> GetMoveCandidates(IEnumerable<int> categoryIds, bool includeSubCategories)
        {
            var ids = new HashSet<int>(categoryIds ?? Enumerable.Empty<int>());
            if (ids.Count == 0) return new List<KBArticle>();

            if (includeSubCategories)
            {
                var all = _categoryRepo.GetAll().ToList();
                foreach (int id in ids.ToList())
                    ids.UnionWith(KBCategoryTree.GetDescendantIds(all, id));
            }
            return _articleRepo.GetMoveCandidates(ids);
        }

        public KBArticleMovePreview PreviewMove(IEnumerable<int> articleIds, int toCategoryId)
        {
            var all  = _categoryRepo.GetAll().ToList();
            var dest = RequireDestination(all, toCategoryId);
            var articles = _articleRepo.GetMoveCandidatesByIds(articleIds);

            var toMove = articles.Where(a => a.CategoryID != toCategoryId).ToList();
            var preview = new KBArticleMovePreview
            {
                ToMoveCount               = toMove.Count,
                AlreadyInDestinationCount = articles.Count - toMove.Count,
            };
            if (toMove.Count == 0) return preview;

            bool destIsLaw = KBCategoryTree.IsLawRootOrUnder(all, toCategoryId);
            int crossKind = toMove.Count(a => KBCategoryTree.IsLawRootOrUnder(all, a.CategoryID) != destIsLaw);
            if (crossKind > 0)
                preview.Warnings.Add(string.Format(
                    "{0} บทความจะย้ายข้ามระหว่างกฎหมายกับเอกสารที่ไม่ใช่กฎหมาย — ความเชื่อมโยง (cross-reference) ของบทความเหล่านี้ใน KB tree อาจหายหรือเปลี่ยนไป",
                    crossKind));

            if (string.Equals((dest.CategoryCode ?? string.Empty).Trim(),
                              KBImportLayoutTypeCatalog.LawRootCategoryCode, StringComparison.OrdinalIgnoreCase))
                preview.Warnings.Add("หมวดปลายทางคือหมวดกฎหมายหลัก (LW) — ปกติบทความกฎหมายอยู่ใต้หมวดกฎหมายแต่ละฉบับ");

            int activeChildren = all.Count(c => c.IsActive && c.ParentCategoryID == dest.CategoryID);
            if (activeChildren > 0)
                preview.Warnings.Add(string.Format(
                    "หมวดปลายทางมีหมวดย่อย {0} หมวด (เช่น หมวดแยกตามปี) — การนำเข้า Excel ครั้งต่อไปจะนำบทความใหม่ไปลงหมวดย่อย ไม่ใช่หมวดนี้",
                    activeChildren));

            int? destProfile = KBCategoryTree.GetEffectiveDisplayProfileId(all, toCategoryId);
            int diffProfile = toMove.Count(a => KBCategoryTree.GetEffectiveDisplayProfileId(all, a.CategoryID) != destProfile);
            if (diffProfile > 0)
                preview.Warnings.Add(string.Format(
                    "{0} บทความจะใช้รูปแบบการแสดงผลของหมวดปลายทาง (Display Profile ต่างจากเดิม)", diffProfile));

            return preview;
        }

        public KBArticleMoveResult Move(IEnumerable<int> articleIds, int toCategoryId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var requested = (articleIds ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (requested.Count == 0)
                throw new InvalidOperationException("ยังไม่ได้เลือกบทความที่จะย้าย");

            var all = _categoryRepo.GetAll().ToList();
            RequireDestination(all, toCategoryId);

            string updatedBy = string.IsNullOrWhiteSpace(user.ADUserName) ? "system" : user.ADUserName;
            var moved = _articleRepo.MoveToCategory(requested, toCategoryId, updatedBy);

            foreach (var m in moved)
            {
                _auditService.Log(new AuditEntry
                {
                    Module     = "KnowledgeBase",
                    Action     = "MOVE_KB_ARTICLE",
                    UserID     = user.UserID,
                    UserName   = user.ADUserName,
                    UserRole   = user.AppRole,
                    TableName  = "KMS.t_KBArticle",
                    RecordID   = m.Key,
                    OldValue   = "CategoryID=" + m.Value,
                    NewValue   = "CategoryID=" + toCategoryId,
                    Result     = 1,
                    MachineName = user.MachineName,
                    IPAddress  = user.IPAddress,
                    SessionID  = user.SessionID,
                    AuditDate  = DateTime.Now,
                });
            }

            return new KBArticleMoveResult { RequestedCount = requested.Count, MovedCount = moved.Count };
        }

        private static KBCategory RequireDestination(IList<KBCategory> all, int toCategoryId)
        {
            var dest = all.FirstOrDefault(c => c.CategoryID == toCategoryId);
            if (dest == null)
                throw new InvalidOperationException("ไม่พบหมวดปลายทาง");
            if (!dest.IsActive)
                throw new InvalidOperationException("หมวดปลายทางถูกปิดใช้งาน — เลือกหมวดอื่น");
            return dest;
        }
    }
}
