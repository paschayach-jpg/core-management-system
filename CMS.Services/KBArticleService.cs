using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Business Logic สำหรับ Knowledge Base Article (MOD-006)
    /// จัดการ Lifecycle: Draft → Review → Published → Archived
    /// บันทึก Version Snapshot ทุกครั้งที่แก้ไข + Audit Log
    /// </summary>
    public class KBArticleService : IKBArticleService
    {
        private readonly IKBArticleRepository     _articleRepo;
        private readonly IKBVersionRepository     _versionRepo;
        private readonly IKBArticleLinkRepository _linkRepo;
        private readonly IAuditService            _auditService;

        public KBArticleService(
            IKBArticleRepository     articleRepo,
            IKBVersionRepository     versionRepo,
            IKBArticleLinkRepository linkRepo,
            IAuditService            auditService)
        {
            if (articleRepo  == null) throw new ArgumentNullException("articleRepo");
            if (versionRepo  == null) throw new ArgumentNullException("versionRepo");
            if (linkRepo     == null) throw new ArgumentNullException("linkRepo");
            if (auditService == null) throw new ArgumentNullException("auditService");
            _articleRepo  = articleRepo;
            _versionRepo  = versionRepo;
            _linkRepo     = linkRepo;
            _auditService = auditService;
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public IEnumerable<KBArticle> GetAll()
        {
            return _articleRepo.GetAll();
        }

        public IEnumerable<KBArticle> GetByCategory(int categoryId)
        {
            return _articleRepo.GetByCategory(categoryId);
        }

        public IEnumerable<KBArticle> Search(string keyword, int? categoryId, byte? status, byte? articleType)
        {
            return _articleRepo.Search(keyword, categoryId, status, articleType);
        }

        public KBArticle GetById(int articleId)
        {
            var article = _articleRepo.GetById(articleId);
            return article;
        }

        public IEnumerable<KBArticle> SearchPublishedArticles(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<KBArticle>();

            // Search only published articles (Status = 3)
            // Filter by Article Code OR Title containing search text
            return _articleRepo.Search(searchText, categoryId: null, status: null, articleType: null);
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public string GenerateNextArticleCode()
        {
            int year = DateTime.Now.Year;
            int seq  = _articleRepo.GetNextCodeSequence(year);
            return string.Format("KB-{0}-{1:D3}", year, seq);
        }

        public int Create(KBArticle article)
        {
            if (article == null)      throw new ArgumentNullException("article");
            if (string.IsNullOrWhiteSpace(article.Title))
                throw new ArgumentException("กรุณากรอกชื่อบทความ");
            if (article.CategoryID <= 0)
                throw new ArgumentException("กรุณาเลือก Category");

            // Auto-generate ArticleCode if empty — รูปแบบ KB-yyyy-NNN (sequential per year)
            if (string.IsNullOrWhiteSpace(article.ArticleCode))
            {
                int year = DateTime.Now.Year;
                int seq  = _articleRepo.GetNextCodeSequence(year);
                article.ArticleCode = string.Format("KB-{0}-{1:D3}", year, seq);
            }

            // New article always starts as Draft
            article.Status = 1;
            article.CurrentVersion = 1;

            // Set ReviewDueDate = 1 year from now if not specified
            if (!article.ReviewDueDate.HasValue)
                article.ReviewDueDate = DateTime.Now.AddYears(1);

            int newId = _articleRepo.Insert(article);

            // Snapshot version 1
            _versionRepo.Insert(new KBVersion
            {
                ArticleID         = newId,
                VersionNumber     = 1,
                ContentSnapshot   = article.ContentHTML,
                ChangeDescription = "บทความใหม่",
                CreatedBy         = article.CreatedBy,
            });

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "CREATE_KB_ARTICLE",
                UserName  = article.CreatedBy ?? string.Empty,
                TableName = "KMS.t_KBArticle",
                RecordID  = newId,
                AuditDate = DateTime.Now,
                Result    = 1,
            });

            return newId;
        }

        public void Update(KBArticle article, string changeDescription)
        {
            if (article == null) throw new ArgumentNullException("article");
            if (string.IsNullOrWhiteSpace(article.Title))
                throw new ArgumentException("กรุณากรอกชื่อบทความ");

            // Snapshot new version before update
            int nextVer = _versionRepo.GetNextVersionNumber(article.ArticleID);
            _versionRepo.Insert(new KBVersion
            {
                ArticleID         = article.ArticleID,
                VersionNumber     = nextVer,
                ContentSnapshot   = article.ContentHTML,
                ChangeDescription = changeDescription ?? "แก้ไขบทความ",
                CreatedBy         = article.CreatedBy,
            });

            _articleRepo.Update(article);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "UPDATE_KB_ARTICLE",
                TableName = "KMS.t_KBArticle",
                RecordID  = article.ArticleID,
                AuditDate = DateTime.Now,
                Result    = 1,
            });
        }

        public void ChangeStatus(int articleId, byte newStatus, string actionBy)
        {
            // Business rules
            // Draft(1) → Review(2): anyone
            // Review(2) → Published(3): requires reviewer
            // Published(3) → Archived(4): admin/manager
            // Archived(4) → cannot go back (per BRD: Obsolete → Archive only, no delete)
            var current = _articleRepo.GetById(articleId);
            if (current == null)
                throw new InvalidOperationException("ไม่พบบทความที่ต้องการเปลี่ยนสถานะ");
            if (current.Status == 4)
                throw new InvalidOperationException("บทความที่ Archive แล้วไม่สามารถเปลี่ยนสถานะได้");

            _articleRepo.UpdateStatus(articleId, newStatus, actionBy);

            string actionName;
            switch (newStatus)
            {
                case 2:  actionName = "SUBMIT_REVIEW";  break;
                case 3:  actionName = "PUBLISH";        break;
                case 4:  actionName = "ARCHIVE";        break;
                default: actionName = "CHANGE_STATUS";  break;
            }

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = actionName,
                UserName  = actionBy ?? string.Empty,
                TableName = "KMS.t_KBArticle",
                RecordID  = articleId,
                AuditDate = DateTime.Now,
                Result    = 1,
            });
        }

        // ── Version History ───────────────────────────────────────────────────

        public IEnumerable<KBVersion> GetVersionHistory(int articleId)
        {
            return _versionRepo.GetByArticle(articleId);
        }

        // ── Version (latest number) ───────────────────────────────────────────

        public int GetLatestVersionNumber(int articleId)
        {
            try
            {
                var latest = _versionRepo.GetLatestByArticle(articleId);
                return latest != null ? latest.VersionNumber : 0;
            }
            catch { return 0; }
        }

        // ── Article Links ─────────────────────────────────────────────────────

        public IEnumerable<KBArticleLink> GetArticleLinks(int articleId, byte? entityType = null)
        {
            return _linkRepo.GetLinksForArticle(articleId, entityType);
        }

        public void SaveArticleLinks(int articleId, IEnumerable<KBArticleLink> links)
        {
            _linkRepo.DeleteAllForArticle(articleId);
            if (links == null) return;
            foreach (var link in links)
            {
                link.ArticleID = articleId;
                _linkRepo.Insert(link);
            }
        }

        public void AddArticleLink(KBArticleLink link)
        {
            if (link == null) throw new ArgumentNullException("link");
            _linkRepo.Insert(link);
        }

        public void RemoveArticleLink(int articleId, int entityId, byte entityType)
        {
            var link = _linkRepo.GetLinksForArticle(articleId, entityType)
                                .FirstOrDefault(l => l.EntityID == entityId);
            if (link != null)
                _linkRepo.Delete(link.LinkID);
        }

        // ── Engagement ────────────────────────────────────────────────────────

        public void IncrementViewCount(int articleId)
        {
            _articleRepo.IncrementViewCount(articleId);
        }

        public void IncrementLikeCount(int articleId)
        {
            _articleRepo.IncrementLikeCount(articleId);
        }
    }
}
