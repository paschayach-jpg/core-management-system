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
        private readonly IKBArticleRepository           _articleRepo;
        private readonly IKBVersionRepository           _versionRepo;
        private readonly IKBArticleLinkRepository       _linkRepo;
        private readonly IKBArticleAttachmentRepository _attachmentRepo;
        private readonly IFileDocumentRepository        _fileDocRepo;
        private readonly IAuditService                  _auditService;

        public KBArticleService(
            IKBArticleRepository           articleRepo,
            IKBVersionRepository           versionRepo,
            IKBArticleLinkRepository       linkRepo,
            IKBArticleAttachmentRepository attachmentRepo,
            IFileDocumentRepository        fileDocRepo,
            IAuditService                  auditService)
        {
            if (articleRepo    == null) throw new ArgumentNullException("articleRepo");
            if (versionRepo    == null) throw new ArgumentNullException("versionRepo");
            if (linkRepo       == null) throw new ArgumentNullException("linkRepo");
            if (attachmentRepo == null) throw new ArgumentNullException("attachmentRepo");
            if (fileDocRepo    == null) throw new ArgumentNullException("fileDocRepo");
            if (auditService   == null) throw new ArgumentNullException("auditService");
            _articleRepo    = articleRepo;
            _versionRepo    = versionRepo;
            _linkRepo       = linkRepo;
            _attachmentRepo = attachmentRepo;
            _fileDocRepo    = fileDocRepo;
            _auditService   = auditService;
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
            var results = _articleRepo.Search(categoryId, status, articleType);
            if (string.IsNullOrWhiteSpace(keyword)) return results;

            var _keyword = new string(keyword.Where(c => !char.IsWhiteSpace(c)).ToArray());

            return results.Where(a =>
                (a.Title != null && a.Title.IndexOf(_keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.Summary != null && a.Summary.IndexOf(_keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.Tags != null && a.Tags.IndexOf(_keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.TitleCategory != null && a.TitleCategory.IndexOf(_keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.ContentHTML != null && StripHtmlTags(a.ContentHTML).IndexOf(_keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();
        }
        public IEnumerable<KBArticle> SearchByType(string keyword, int? categoryId, byte? status, byte? articleType, int searchFieldId)
        {
            var results = _articleRepo.Search(categoryId, status, articleType);
            return FilterByKeyword(results, keyword, searchFieldId);
        }

        /// <summary>
        /// ใช้ criteria การค้นหาแบบเดียวกับ <see cref="SearchByType"/> กับรายการบทความที่มีอยู่แล้วในหน่วยความจำ
        /// (เช่น กลุ่มบทความที่ resolve จาก link graph ของโหนด cross-reference ในต้นไม้หมวดหมู่ ซึ่งไม่ได้มาจาก
        /// query ตาม CategoryID ตรง ๆ จึงใช้ <see cref="SearchByType"/> ค้นหาซ้ำไม่ได้)
        /// </summary>
        public IEnumerable<KBArticle> FilterByKeyword(IEnumerable<KBArticle> articles, string keyword, int searchFieldId)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return articles;

            var _keyword = new string(keyword.Where(c => !char.IsWhiteSpace(c)).ToArray());
            switch (searchFieldId)
            {
                case 1: // ทั้งหมด — ค้นทุก field
                    return articles.Where(a =>
                        MatchesTitle(a, _keyword) || MatchesContent(a, _keyword) || MatchesTags(a, _keyword) ||
                        MatchesSummary(a, _keyword) || MatchesTitleCategory(a, _keyword));
                case 2:
                    return articles.Where(a => MatchesTitle(a, _keyword));
                case 3:
                    return articles.Where(a => MatchesContent(a, _keyword));
                case 4:
                    return articles.Where(a => MatchesTags(a, _keyword));
                case 5:
                    return articles.Where(a => MatchesSummary(a, _keyword));
                case 6:
                    return articles.Where(a => MatchesTitleCategory(a, _keyword));
                default:
                    return articles;
            }
        }

        /// <summary>ค้นหาแบบ substring (ไม่สนช่องว่าง) ให้ behavior เหมือนกับ field อื่น ๆ ทั้งหมด (Content/Tags/Summary/TitleCategory)</summary>
        private static bool MatchesTitle(KBArticle a, string keyword)
        {
            if (string.IsNullOrEmpty(a.Title)) return false;
            var title = new string(a.Title.Where(c => !char.IsWhiteSpace(c)).ToArray());
            return title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool MatchesContent(KBArticle a, string keyword)
        {
            return a.ContentHTML != null &&
                new string(StripHtmlTags(a.ContentHTML).Where(c => !char.IsWhiteSpace(c)).ToArray()).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool MatchesTags(KBArticle a, string keyword)
        {
            return a.Tags != null && a.Tags.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool MatchesSummary(KBArticle a, string keyword)
        {
            if (string.IsNullOrEmpty(a.Summary)) return false;
            var summary = new string(a.Summary.Where(c => !char.IsWhiteSpace(c)).ToArray());
            return summary.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool MatchesTitleCategory(KBArticle a, string keyword)
        {
            if (string.IsNullOrEmpty(a.TitleCategory)) return false;
            var titleCategory = new string(a.TitleCategory.Where(c => !char.IsWhiteSpace(c)).ToArray());
            return titleCategory.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
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

            var results = _articleRepo.Search(categoryId: null, status: null, articleType: null);

            string keyword = new string(searchText.Where(c => !char.IsWhiteSpace(c)).ToArray()); ;
            return results.Where(a =>
                (a.Title != null && a.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.Summary != null && a.Summary.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.Tags != null && a.Tags.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.TitleCategory != null && a.TitleCategory.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.ContentHTML != null && StripHtmlTags(a.ContentHTML).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();
        }
        /// <summary>
        /// Strips HTML tags from text and returns plain text content.
        /// </summary>
        private static string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Replace common block-level tags with spaces to preserve word boundaries
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<br\s*/?>", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?p>", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?div>", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?li>", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?tr>", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?td>", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?th>", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Remove all remaining HTML tags
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>", string.Empty);

            // Decode HTML entities
            html = System.Net.WebUtility.HtmlDecode(html);

            // Clean up multiple spaces and trim
            html = System.Text.RegularExpressions.Regex.Replace(html, @"\s+", " ");
            return new string(html.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public bool ArticleCodeExists(string articleCode)
        {
            return !string.IsNullOrWhiteSpace(articleCode) && _articleRepo.ExistsByCode(articleCode);
        }

        public KBArticle GetByCode(string articleCode)
        {
            return string.IsNullOrWhiteSpace(articleCode) ? null : _articleRepo.GetByCode(articleCode);
        }

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

            // ArticleCode ซ้ำไม่ได้ — ExistsByCode นับบทความที่ถูกลบด้วย (Code เดียวกับบทความที่ลบแล้ว import ซ้ำไม่ได้ จึงห้ามสร้างทับ)
            if (_articleRepo.ExistsByCode(article.ArticleCode))
                throw new ArgumentException(string.Format("ArticleCode '{0}' มีอยู่ในระบบแล้ว", article.ArticleCode));

            // New article always starts as Draft
            article.Status = 1;
            article.CurrentVersion = 1;

            // Set ReviewDueDate = 1 year from now if not specified
            if (!article.ReviewDueDate.HasValue)
                article.ReviewDueDate = DateTime.Now.AddYears(1);

            // บทความ + Version 1 ในธุรกรรมเดียว (version ล้มเหลว = rollback บทความด้วย ไม่เหลือบทความที่ไม่มี version)
            int newId = _articleRepo.InsertWithVersion(article, new KBVersion
            {
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

        public void Update(KBArticle article, string changeDescription, string actionBy)
        {
            if (article == null) throw new ArgumentNullException("article");
            if (string.IsNullOrWhiteSpace(article.Title))
                throw new ArgumentException("กรุณากรอกชื่อบทความ");

            var current = _articleRepo.GetById(article.ArticleID);
            if (current == null)
                throw new InvalidOperationException("ไม่พบบทความที่ต้องการแก้ไข");

            // สร้าง Version Snapshot เฉพาะเมื่อเนื้อหาเปลี่ยนจริง (เทียบ ordinal เหมือน import) — บันทึกซ้ำโดยไม่แก้เนื้อหาไม่เพิ่มเลข version
            bool contentChanged = !string.Equals(current.ContentHTML ?? string.Empty, article.ContentHTML ?? string.Empty, StringComparison.Ordinal);
            KBVersion newVersion = contentChanged
                ? new KBVersion
                {
                    ArticleID         = article.ArticleID,
                    ContentSnapshot   = article.ContentHTML,
                    ChangeDescription = string.IsNullOrWhiteSpace(changeDescription) ? "แก้ไขบทความ" : changeDescription,
                    CreatedBy         = actionBy,
                }
                : null;

            _articleRepo.Update(article, newVersion, actionBy);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "UPDATE_KB_ARTICLE",
                UserName  = actionBy ?? string.Empty,
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

        // ── Article Attachments ───────────────────────────────────────────────

        public IEnumerable<KBArticleAttachment> GetArticleAttachments(int articleId)
        {
            return _attachmentRepo.GetAttachmentsForArticle(articleId);
        }

        public IEnumerable<FileDocument> GetArticleAttachmentFiles(int articleId)
        {
            var attachments = _attachmentRepo.GetAttachmentsForArticle(articleId);
            var files = new List<FileDocument>();
            foreach (var att in attachments)
            {
                var file = _fileDocRepo.GetById(att.FileID);
                if (file != null)
                {
                    files.Add(file);
                }
            }
            return files;
        }

        public void SaveArticleAttachments(int articleId, IEnumerable<int> fileIds, string actionBy)
        {
            _attachmentRepo.DeleteAllForArticle(articleId);
            if (fileIds == null) return;

            // Load existing folder/file links to check for duplicates
            var existingLinks = _linkRepo.GetLinksForArticle(articleId, 1);
            var existingFileIds = new HashSet<string>(existingLinks.Select(l => l.EntityID));

            foreach (var fileId in fileIds)
            {
                var attachment = new KBArticleAttachment
                {
                    ArticleID = articleId,
                    FileID = fileId,
                    CreatedBy = actionBy,
                    CreatedDate = DateTime.Now
                };
                int insertedId = _attachmentRepo.Insert(attachment);

                // Write audit log for attaching file
                _auditService.Log(new AuditEntry
                {
                    Module = "KnowledgeBase",
                    Action = "ATTACH_FILE",
                    UserName = actionBy ?? string.Empty,
                    TableName = "KMS.t_KBArticleAttachment",
                    RecordID = insertedId,
                    NewValue = string.Format("Attached file ID {0} to KB article ID {1}", fileId, articleId),
                    AuditDate = DateTime.Now,
                    Result = 1
                });

                // Check for duplicates in t_KBArticleLink to prevent duplicate entries
                string entityIdStr = fileId.ToString();
                if (!existingFileIds.Contains(entityIdStr))
                {
                    _linkRepo.Insert(new KBArticleLink
                    {
                        ArticleID = articleId,
                        EntityType = 1, // Folder/File
                        EntityID = entityIdStr,
                        CreatedBy = actionBy,
                        CreatedDate = DateTime.Now
                    });
                    existingFileIds.Add(entityIdStr); // Avoid duplicates within the loop itself
                }
            }
        }

        public void RemoveArticleAttachment(int articleId, int fileId)
        {
            _attachmentRepo.DeleteByArticleAndFile(articleId, fileId);
        }

        // ── Article Links ─────────────────────────────────────────────────────

        public IEnumerable<KBArticleLink> GetArticleLinks(int articleId, byte? entityType = null)
        {
            return _linkRepo.GetLinksForArticle(articleId, entityType);
        }

        public IEnumerable<KBArticleLink> GetAllLinksByType(byte entityType)
        {
            return _linkRepo.GetAllByEntityType(entityType);
        }

        public void SaveArticleLinks(int articleId, IEnumerable<KBArticleLink> links)
        {
            // editor จัดการเฉพาะ link "บทความที่เกี่ยวข้อง" (Law, 4) — Task (2) / Program (3) / ไฟล์ (1) ไม่ถูกแตะ
            // (เดิมลบ Task/Program ทุกครั้งที่บันทึกแล้วเขียนกลับไม่ได้ → link หายหมด)
            var wanted    = new List<KBArticleLink>();
            var wantedIds = new HashSet<int>();
            if (links != null)
            {
                foreach (var link in links)
                {
                    int id;
                    if (link.EntityType == 4 && int.TryParse(link.EntityID, out id) && id > 0 && wantedIds.Add(id))
                        wanted.Add(link);
                }
            }

            // ตัวที่หายไปจากรายการ → ยกเลิกทั้งสองทาง (link ที่ import สร้างไว้เป็นสองทาง การลบทางเดียวทำให้ผังกฎหมายยังเห็นอยู่)
            foreach (var existing in _linkRepo.GetLinksForArticle(articleId, 4).ToList())
            {
                int existingId;
                if (!int.TryParse(existing.EntityID, out existingId)) existingId = 0;
                if (existingId > 0 && wantedIds.Contains(existingId)) continue;

                UnlinkArticles(articleId, existingId, existing);
            }

            // ตัวที่เพิ่ม → ผูกทั้งสองทาง; ตัวที่มีอยู่แล้วไม่ถูกแตะ (คง CreatedBy/CreatedDate เดิม)
            foreach (var link in wanted)
                LinkArticles(articleId, int.Parse(link.EntityID), link.CreatedBy);
        }

        public void LinkArticles(int articleId, int otherArticleId, string createdBy)
        {
            if (articleId <= 0 || otherArticleId <= 0 || articleId == otherArticleId) return;

            EnsureLawLink(articleId, otherArticleId, createdBy);
            EnsureLawLink(otherArticleId, articleId, createdBy);
        }

        private void EnsureLawLink(int fromArticleId, int toArticleId, string createdBy)
        {
            string target = toArticleId.ToString();
            if (_linkRepo.Exists(fromArticleId, 4, target)) return;

            _linkRepo.Insert(new KBArticleLink
            {
                ArticleID   = fromArticleId,
                EntityType  = 4,
                EntityID    = target,
                CreatedBy   = createdBy,
                CreatedDate = DateTime.Now,
            });
        }

        public void UnlinkArticles(int articleId, int otherArticleId)
        {
            UnlinkArticles(articleId, otherArticleId, null);
        }

        /// <summary>ลบ link ทั้งสองทาง; <paramref name="knownLink"/> = link ที่รู้อยู่แล้ว (กรณี EntityID แปลงเป็นเลขไม่ได้ ลบเฉพาะแถวนั้น)</summary>
        private void UnlinkArticles(int articleId, int otherArticleId, KBArticleLink knownLink)
        {
            if (otherArticleId <= 0)
            {
                if (knownLink != null) _linkRepo.Delete(knownLink.LinkID);
                return;
            }

            DeleteLawLinks(articleId, otherArticleId);
            DeleteLawLinks(otherArticleId, articleId);
        }

        private void DeleteLawLinks(int fromArticleId, int toArticleId)
        {
            string target = toArticleId.ToString();
            foreach (var link in _linkRepo.GetLinksForArticle(fromArticleId, 4).Where(l => l.EntityID == target).ToList())
                _linkRepo.Delete(link.LinkID);
        }

        public void AddArticleLink(KBArticleLink link)
        {
            if (link == null) throw new ArgumentNullException("link");
            if (_linkRepo.Exists(link.ArticleID, link.EntityType, link.EntityID)) return;
            _linkRepo.Insert(link);
        }

        public void RemoveArticleLink(int articleId, int entityId, byte entityType)
        {
            var link = _linkRepo.GetLinksForArticle(articleId, entityType)
                                .FirstOrDefault(l => Convert.ToInt32(l.EntityID) == entityId);
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

        public void SoftDelete(int articleId, string actionBy)
        {
            // link สองทาง: ที่ออกจากบทความนี้ + ที่ชี้เข้าบทความนี้ (EntityID = ArticleID ของบทความนี้ ในบทความอื่น)
            _linkRepo.DeleteAllForArticle(articleId, 4);
            string idText = articleId.ToString();
            foreach (var incoming in _linkRepo.GetAllByEntityType(4).Where(l => l.EntityID == idText).ToList())
                _linkRepo.Delete(incoming.LinkID);

            _articleRepo.SoftDelete(articleId);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "DELETE_KB_ARTICLE",
                UserName  = actionBy ?? string.Empty,
                TableName = "KMS.t_KBArticle",
                RecordID  = articleId,
                AuditDate = DateTime.Now,
                Result    = 1,
            });
        }

        public void UpdateExpiryDate(int articleId, DateTime? expiryDate)
        {
            _articleRepo.UpdateExpiryDate(articleId, expiryDate);
        }
    }
}
