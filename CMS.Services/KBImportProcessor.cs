using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace CMS.Services
{
    /// <summary>
    /// Orchestrates the KB import pipeline for both supported Excel layouts (see
    /// <see cref="KBExcelReader"/> and <see cref="KBImportRow.RowKind"/>):
    ///
    /// <b>Law</b> ("มาตรา" rows) — Phase 1: row validation, including <c>ArticleCode</c>
    /// existence check (read-only DB access). Phase 2: article + version insert
    /// (<c>KMS.t_KBArticle</c>, <c>KMS.t_KBVersion</c>). CategoryID is resolved per row from the
    /// row's <c>ArticleCode</c> (3-letter law prefix, see <see cref="KBImportCategoryResolver"/>) so one
    /// Template serves every law; ArticleType comes from the KB Import Template selected in the Wizard
    /// (see <see cref="InsertArticles"/>) — not hardcoded here.
    ///
    /// <b>NonLaw</b> (ไฟล์ที่ไม่ใช่กฎหมาย: OR/AN/SO/RG/CI/MR rows; 1 row = 1 ArticleCode) — Phase 1: row validation, including resolving
    /// the parent law via <c>ArticleCode</c> exact match. Phase 2: attachment
    /// download + DMS upload, article insert, attachment link, and bidirectional
    /// <c>KMS.t_KBArticleLink</c> records to the parent law (first 22 chars of the ArticleCode).
    /// ArticleCode ที่มีในระบบแล้วไม่ใช่ Error: ไม่ insert ซ้ำ แต่เติมไฟล์แนบ/link ที่ตกหล่น (ดู <see cref="FillExistingNonLawArticle"/>)
    /// จึง upload Excel เดิมซ้ำได้โดยได้เฉพาะข้อมูลที่ขาด.
    /// CategoryID is resolved per row from digit 23 of the ArticleCode (1-6 → AN/OR/SO/RG/CI/MR),
    /// same as Law — one Template serves every document type; ArticleType comes from the Template.
    /// </summary>
    public class KBImportProcessor : IKBImportProcessorService
    {
        private readonly IKBArticleRepository     _articleRepo;
        private readonly IKBArticleLinkRepository _linkRepo;
        private readonly IKBCategoryRepository    _categoryRepo;
        private readonly IKBArticleService        _articleService;
        private readonly IFileService             _fileService;
        private readonly LawFileDownloader        _downloader;

        // ── ค่าคงที่ตามข้อกำหนด ──────────────────────────────────────────────
        private const byte FixedStatus               = 2;   // Review
        private const int  FixedConfidentialityLevel = 0;

        /// <summary>Folder ปลายทางของไฟล์แนบ (NonLaw) — ตรงกับที่ KBEditorDialog ใช้ ("KMS Attachments")</summary>
        private const int AttachmentFolderId = 437;

        public KBImportProcessor(
            IKBArticleRepository     articleRepo,
            IKBArticleLinkRepository linkRepo,
            IKBCategoryRepository    categoryRepo,
            IKBArticleService        articleService,
            IFileService             fileService,
            LawFileDownloader        downloader)
        {
            if (articleRepo    == null) throw new ArgumentNullException("articleRepo");
            if (linkRepo       == null) throw new ArgumentNullException("linkRepo");
            if (categoryRepo   == null) throw new ArgumentNullException("categoryRepo");
            if (articleService == null) throw new ArgumentNullException("articleService");
            if (fileService    == null) throw new ArgumentNullException("fileService");
            if (downloader     == null) throw new ArgumentNullException("downloader");

            _articleRepo    = articleRepo;
            _linkRepo       = linkRepo;
            _categoryRepo   = categoryRepo;
            _articleService = articleService;
            _fileService    = fileService;
            _downloader     = downloader;
        }

        // ── Phase 1: Validation ──────────────────────────────────────────────

        /// <summary>
        /// Validates each row and sets <see cref="KBImportRow.Status"/> /
        /// <see cref="KBImportRow.StatusNote"/>. Read-only DB access only
        /// (<see cref="IKBArticleRepository.ExistsByCode"/>, <see cref="IKBArticleRepository.GetByCode"/>, attachment/link lookups)
        /// — no writes.
        /// </summary>
        /// <remarks>
        /// CategoryID ของแต่ละแถวหาจาก ArticleCode ของแถวนั้น (<see cref="KBImportCategoryResolver"/>) — Template ไม่ผูกหมวด
        /// จึงรับได้หลายกฎหมาย/หลายประเภทในไฟล์เดียว; แถวที่หาหมวดไม่ได้เป็น Error รายแถว
        /// </remarks>
        public void ValidateRows(IList<KBImportRow> rows)
        {
            if (rows == null) throw new ArgumentNullException("rows");

            // โหลดรายการหมวดครั้งเดียวต่อการ validate หนึ่งไฟล์
            var resolver    = new KBImportCategoryResolver(_categoryRepo.GetAll());
            var seenCodes   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parentCache = new Dictionary<string, KBArticle>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                // Rows already marked Error by the reader (unreadable row) are left as-is.
                if (row.Status == "Error")
                    continue;

                switch (row.RowKind)
                {
                    case KBImportLayoutTypeCatalog.NonLaw:
                        ValidateNonLawRow(row, seenCodes, parentCache, resolver);
                        break;
                    case KBImportLayoutTypeCatalog.Law:
                        ValidateLawRow(row, seenCodes, _articleRepo, resolver);
                        break;
                    default:
                        // Unknown RowKind must not be silently treated as Law — fail just this row.
                        row.Status     = "Error";
                        row.StatusNote = string.Format("ไม่รู้จัก RowKind '{0}'", row.RowKind);
                        break;
                }
            }
        }

        // ── Phase 2: Article insert ──────────────────────────────────────────

        /// <summary>
        /// Inserts a <c>KBArticle</c> for each row in <paramref name="rows"/> (expected to be
        /// pre-filtered to <c>Status == "Valid"</c> rows by the caller). Law rows also insert
        /// an initial <c>KBVersion</c>; NonLaw rows also download/upload the attachment and
        /// link the parent law. NonLaw rows whose article already exists
        /// (<see cref="KBImportRow.ExistingArticleId"/> &gt; 0) are not inserted again — only the missing attachment/links
        /// are filled in, and the row becomes <c>Skip</c> when there is nothing to fill or its source file is unavailable.
        /// Each row is wrapped in its own <c>try/catch</c>;
        /// a failure on one row increments <paramref name="errorCount"/> and processing continues.
        /// </summary>
        /// <param name="resolvedArticleType">
        /// ArticleType configured on the KB Import Template selected in the Wizard — applied to
        /// every row in this batch (both Law and NonLaw layouts). Must be greater than 0.
        /// </param>
        /// <param name="onProgress">
        /// Optional callback invoked after each row with (rowsProcessed, totalRows).
        /// Use to update a progress bar in the UI.
        /// </param>
        public void InsertArticles(
            IList<KBImportRow> rows,
            UserContext user,
            bool skipRowOnDownloadFailure,
            byte resolvedArticleType,
            out int successCount,
            out int errorCount,
            Action<int, int> onProgress)
        {
            if (rows == null) throw new ArgumentNullException("rows");
            if (resolvedArticleType == 0)
                throw new ArgumentException("resolvedArticleType ต้องมากกว่า 0 — Template ที่เลือกยังไม่ได้ตั้งค่า ArticleType", "resolvedArticleType");

            successCount = 0;
            errorCount   = 0;
            int total    = rows.Count;

            string createdBy = user != null ? (user.AdIdentity ?? string.Empty) : string.Empty;

            // ไฟล์แนบของ Excel row ปัจจุบัน — ArticleCode จาก row เดียวกันใช้ไฟล์ที่โหลด/อัปโหลดครั้งเดียวร่วมกัน
            var files = new RowFileCache();

            for (int i = 0; i < total; i++)
            {
                var row = rows[i];
                try
                {
                    switch (row.RowKind)
                    {
                        case KBImportLayoutTypeCatalog.NonLaw:
                            InsertNonLawArticle(row, user, createdBy, skipRowOnDownloadFailure, resolvedArticleType, files);
                            break;
                        case KBImportLayoutTypeCatalog.Law:
                            InsertLawArticle(row, createdBy, resolvedArticleType);
                            break;
                        default:
                            // Unknown RowKind must not be silently treated as Law.
                            throw new InvalidOperationException(string.Format("ไม่รู้จัก RowKind '{0}'", row.RowKind));
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    row.Status     = "Error";
                    row.StatusNote = TruncateTo200(ex.Message);
                    errorCount++;
                }

                if (onProgress != null)
                    onProgress(i + 1, total);
            }
        }

        private void InsertLawArticle(KBImportRow row, string createdBy, byte resolvedArticleType)
        {
            // Defensive re-check — the DB may have changed since ValidateRows ran.
            if (string.IsNullOrEmpty(row.ArticleCode))
                throw new InvalidOperationException("คอลัมน์ A (ArticleCode) ว่าง");
            if (_articleRepo.ExistsByCode(row.ArticleCode))
                throw new InvalidOperationException(string.Format(
                    "ArticleCode '{0}' มีอยู่ในระบบแล้ว", row.ArticleCode));
            if (row.ResolvedCategoryID <= 0)
                throw new InvalidOperationException("ไม่พบหมวด (CategoryID) ของแถวนี้ — ต้องผ่านการ validate ก่อน insert");

            var article = new KBArticle
            {
                ArticleCode           = row.ArticleCode,
                Title                 = row.Title,
                ContentHTML           = ConvertPlainTextToHtml(row.RawContent),
                Summary               = row.Summary,
                TitleCategory         = row.TitleCategory,
                CategoryID            = row.ResolvedCategoryID,
                ArticleType           = resolvedArticleType,
                Status                = FixedStatus,
                CurrentVersion        = 1,
                PublishedDate         = row.PublishedDateParsed,
                EffectiveDate         = row.EffectiveDateParsed,
                IsOfficialPolicy      = false,
                Tags                  = row.Tags,
                ConfidentialityLevel  = FixedConfidentialityLevel,
                CreatedBy             = createdBy,
                CreatedDate           = DateTime.Now,
            };

            // Article + first Version inserted in one DB transaction — a Version-insert failure
            // rolls back the Article insert too, instead of leaving an orphan KBArticle with no
            // KBVersion (which previously happened and got misreported as "duplicate key").
            var version = new KBVersion
            {
                VersionNumber   = 1,
                ContentSnapshot = article.ContentHTML,
                CreatedBy       = createdBy,
                CreatedDate     = DateTime.Now,
            };
            int articleId = _articleRepo.InsertWithVersion(article, version);
            row.ImportedArticleId = articleId;

            row.Status     = "Valid";
            row.StatusNote = string.Empty;
        }

        private void InsertNonLawArticle(KBImportRow row, UserContext user, string createdBy, bool skipRowOnDownloadFailure, byte resolvedArticleType, RowFileCache files)
        {
            var notes = new List<string>();

            // ── ArticleCode ──────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(row.ArticleCode))
                throw new InvalidOperationException("Code รวม (ArticleCode ที่ Template map ไว้) ว่าง");

            // บทความมีอยู่แล้ว (ตรวจพบตอน validate) — ไม่ insert ซ้ำ เติมเฉพาะไฟล์แนบ/link ที่ตกหล่น
            if (row.ExistingArticleId > 0)
            {
                FillExistingNonLawArticle(row, user, createdBy, files);
                return;
            }

            // Defensive re-check — เพิ่งมีคนสร้าง Code นี้หลัง validate (ไม่เจอตอน validate จึงยังไม่มี ExistingArticleId)
            if (_articleRepo.ExistsByCode(row.ArticleCode))
                throw new InvalidOperationException(string.Format(
                    "ArticleCode '{0}' มีอยู่ในระบบแล้ว", row.ArticleCode));

            // Defensive re-check — รูปแบบ Code รวมถูกตรวจแล้วตอน ValidateRows; ตรวจซ้ำเพราะ InsertArticles
            // อาจถูกเรียกเมื่อไรก็ได้หลังจากนั้น
            if (!KBImportLayoutTypeCatalog.IsNonLawCode(row.ArticleCode))
                throw new InvalidOperationException(string.Format(
                    "Code รวม: ต้องยาว 27 ตัว, 5 ตัวสุดท้ายห้ามเป็น 00000 และตัวที่ 23 ต้องเป็น 1-6 — '{0}'", row.ArticleCode));
            if (row.ResolvedCategoryID <= 0)
                throw new InvalidOperationException("ไม่พบหมวด (CategoryID) ของแถวนี้ — ต้องผ่านการ validate ก่อน insert");

            // ── ไฟล์แนบ ──────────────────────────────────────────────────────
            int? fileId = null;
            var rowFile = files.For(row, _downloader);

            if (rowFile.Download.Success)
            {
                string uploadError;
                fileId = EnsureUploaded(rowFile, user, out uploadError);
                if (fileId == null)
                {
                    if (skipRowOnDownloadFailure)
                        throw new InvalidOperationException("อัปโหลดไฟล์แนบไม่สำเร็จ: " + uploadError);
                    notes.Add("อัปโหลดไฟล์แนบไม่สำเร็จ: " + uploadError);
                }
            }
            else
            {
                if (skipRowOnDownloadFailure)
                    throw new InvalidOperationException(rowFile.Download.ErrorMessage);
                notes.Add(rowFile.Download.ErrorMessage);
            }

            // ── บทความ ───────────────────────────────────────────────────────
            var article = new KBArticle
            {
                ArticleCode           = row.ArticleCode,
                Title                 = row.Title,
                TitleCategory         = row.TitleCategory,
                ContentHTML           = ConvertPlainTextToHtml(row.RawContent),
                Summary               = row.Summary,
                PublishedDate         = row.PublishedDateParsed,
                EffectiveDate         = row.EffectiveDateParsed,
                CategoryID            = row.ResolvedCategoryID,
                ArticleType           = resolvedArticleType,
                Status                = FixedStatus,
                ConfidentialityLevel  = FixedConfidentialityLevel,
                CreatedBy             = createdBy,
            };

            int articleId = _articleRepo.Insert(article);
            row.ImportedArticleId = articleId;

            // ── ผูกไฟล์แนบเข้ากับบทความ ───────────────────────────────────────
            if (fileId.HasValue)
            {
                row.UploadedFileId = fileId.Value;
                try
                {
                    _articleService.SaveArticleAttachments(
                        articleId, new List<int> { fileId.Value }, createdBy);
                }
                catch (Exception ex)
                {
                    notes.Add("ผูกไฟล์แนบไม่สำเร็จ: " + ex.Message);
                }
            }

            // ── link กฎหมายแม่ (22 ตัวแรกของ ArticleCode) ไป-กลับ — ตัวไหน error ให้ข้ามและแจ้ง ─
            LinkToParents(articleId, row, createdBy, notes);

            if (notes.Count > 0)
            {
                row.Status     = "Warn";
                row.StatusNote = TruncateTo200(string.Join("; ", notes));
            }
            else
            {
                row.Status     = "Valid";
                row.StatusNote = string.Empty;
            }
        }

        /// <summary>
        /// บทความมีอยู่แล้ว (<see cref="KBImportRow.ExistingArticleId"/>) — ไม่ insert ซ้ำ เติมเฉพาะของที่ตกหล่น ตามลำดับ:
        /// 1) ไฟล์ต้นทางต้องโหลดได้ ไม่งั้น "Skip" เฉพาะ ArticleCode นี้
        /// 2) ยังไม่มี record ใน t_KBArticleAttachment → อัปโหลด (ครั้งเดียวต่อ Excel row) แล้ว attach + link;
        ///    มีแล้ว → วน attachment ทุกตัว เติม link ไฟล์ (t_KBArticleLink EntityType=1, EntityID=FileID) ที่ยังไม่มี
        /// 3) link กฎหมายแม่ไป-กลับที่ยังไม่มี
        /// ไม่แตะเนื้อหาบทความเดิม (Title/ContentHTML/วันที่)
        /// </summary>
        private void FillExistingNonLawArticle(KBImportRow row, UserContext user, string createdBy, RowFileCache files)
        {
            int articleId = row.ExistingArticleId;
            row.ImportedArticleId = articleId;

            var filled = new List<string>();   // สิ่งที่เติมได้จริง
            var notes  = new List<string>();   // ปัญหาย่อยที่ไม่ทำให้ทั้ง Code ล้มเหลว (เช่น link กฎหมายแม่)

            // ── ไฟล์ต้นทาง: ไม่มีไฟล์/โหลดไม่สำเร็จ → ข้ามเฉพาะ ArticleCode นี้ ─────────────
            var rowFile = files.For(row, _downloader);
            if (!rowFile.Download.Success)
            {
                row.Status     = "Skip";
                row.StatusNote = TruncateTo200(string.Format(
                    "ArticleCode '{0}' (ArticleID {1}) มีในระบบแล้ว — ข้าม: {2}",
                    row.ArticleCode, articleId, rowFile.Download.ErrorMessage));
                return;
            }

            // ── ไฟล์แนบ + link ไฟล์ ──────────────────────────────────────────────────────
            var attachments = _articleService.GetArticleAttachments(articleId).ToList();
            if (attachments.Count == 0)
            {
                string uploadError;
                int? fileId = EnsureUploaded(rowFile, user, out uploadError);
                if (fileId == null)
                    throw new InvalidOperationException("อัปโหลดไฟล์แนบไม่สำเร็จ: " + uploadError);

                // ยังไม่มี record เลย → SaveArticleAttachments ปลอดภัย (DeleteAll ไม่มีอะไรให้ลบ) และทำ attachment + audit + link ให้ครบ.
                // ห้ามใช้เมื่อมี attachment อยู่แล้ว เพราะมันลบของเดิมทั้งหมดก่อน
                _articleService.SaveArticleAttachments(articleId, new List<int> { fileId.Value }, createdBy);
                row.UploadedFileId = fileId.Value;
                filled.Add("ไฟล์แนบ");
            }
            else
            {
                int linked = 0;
                foreach (var attachment in attachments)
                {
                    string entityId = attachment.FileID.ToString();
                    if (_linkRepo.Exists(articleId, (byte)KBEntityType.Folder, entityId))
                        continue;   // มี link แล้ว → ไป attachment ถัดไป

                    _linkRepo.Insert(new KBArticleLink
                    {
                        ArticleID   = articleId,
                        EntityType  = (byte)KBEntityType.Folder,   // 1 = Folder/File — ตรงกับ KBArticleService.SaveArticleAttachments
                        EntityID    = entityId,
                        CreatedBy   = createdBy,
                        CreatedDate = DateTime.Now,
                    });
                    linked++;
                }

                if (linked > 0)
                    filled.Add(string.Format("link ไฟล์ {0} รายการ", linked));
            }

            // ── link กฎหมายแม่ ──────────────────────────────────────────────────────────
            int parentLinks = LinkToParents(articleId, row, createdBy, notes);
            if (parentLinks > 0)
                filled.Add(string.Format("link กฎหมายแม่ {0} รายการ", parentLinks));

            if (filled.Count == 0 && notes.Count == 0)
            {
                row.Status     = "Skip";
                row.StatusNote = string.Format("ArticleCode '{0}' (ArticleID {1}) ครบถ้วนแล้ว — ไม่มีอะไรให้เติม", row.ArticleCode, articleId);
                return;
            }

            string summary = filled.Count > 0 ? "เติม: " + string.Join(", ", filled) : string.Empty;
            if (notes.Count > 0)
            {
                row.Status     = "Warn";
                row.StatusNote = TruncateTo200((summary.Length > 0 ? summary + "; " : string.Empty) + string.Join("; ", notes));
            }
            else
            {
                row.Status     = "Valid";
                row.StatusNote = TruncateTo200(summary);
            }
        }

        /// <summary>
        /// link กฎหมายแม่ของแถว (22 ตัวแรกของ ArticleCode) ไป-กลับ: บทความ → มาตรา และ มาตรา → บทความ (<see cref="KBEntityType.Law"/>).
        /// ตรวจ Exists ก่อน insert ทุกทิศ จึงเรียกซ้ำได้โดยไม่เกิด link ซ้ำ; ตัวไหนล้มเหลวให้ข้ามและบันทึกใน <paramref name="notes"/>
        /// </summary>
        /// <returns>จำนวน link ที่ insert จริงในครั้งนี้</returns>
        private int LinkToParents(int articleId, KBImportRow row, string createdBy, List<string> notes)
        {
            int inserted = 0;

            for (int i = 0; i < row.ParentCodes.Count; i++)
            {
                string code = row.ParentCodes[i];

                int sectionArticleId = i < row.ResolvedParentArticleIds.Count ? row.ResolvedParentArticleIds[i] : 0;
                if (sectionArticleId <= 0)
                {
                    notes.Add(string.Format("ข้าม link '{0}' (ไม่พบกฎหมายแม่ในระบบ)", code));
                    continue;
                }

                try
                {
                    string entityId      = sectionArticleId.ToString();
                    string articleIdStr  = articleId.ToString();

                    // ── ไป: ประกาศ/คำสั่ง → มาตรา ───────────────────────────
                    if (!_linkRepo.Exists(articleId, (byte)KBEntityType.Law, entityId))
                    {
                        _linkRepo.Insert(new KBArticleLink
                        {
                            ArticleID   = articleId,
                            EntityType  = (byte)KBEntityType.Law,
                            EntityID    = entityId,
                            CreatedBy   = createdBy,
                            CreatedDate = DateTime.Now,
                        });
                        inserted++;
                    }

                    // ── กลับ: มาตรา → ประกาศ/คำสั่ง (reference ไป-กลับ) ──────
                    if (!_linkRepo.Exists(sectionArticleId, (byte)KBEntityType.Law, articleIdStr))
                    {
                        _linkRepo.Insert(new KBArticleLink
                        {
                            ArticleID   = sectionArticleId,
                            EntityType  = (byte)KBEntityType.Law,
                            EntityID    = articleIdStr,
                            CreatedBy   = createdBy,
                            CreatedDate = DateTime.Now,
                        });
                        inserted++;
                    }
                }
                catch (Exception ex)
                {
                    notes.Add(string.Format("ข้าม link '{0}': {1}", code, ex.Message));
                }
            }

            return inserted;
        }

        // ── Static helpers (internal — directly testable) ────────────────────

        /// <summary>
        /// Converts plain text (Law column P / NonLaw ContentHTML) into HTML before it is stored in
        /// <c>t_KBArticle.ContentHTML</c>. Both <c>ArticleViewerForm</c> and <c>KBEditorDialog</c>
        /// set this value straight into an HTML DOM without further conversion — storing raw
        /// <c>\n</c>/<c>\t</c> would collapse per the HTML whitespace spec, so the text is
        /// HTML-encoded first and then <c>\n</c> → <c>&lt;br&gt;</c> / <c>\t</c> → 4x <c>&amp;nbsp;</c>.
        /// </summary>
        internal static string ConvertPlainTextToHtml(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            string normalized = raw.Replace("\r\n", "\n").Replace("\r", "\n");
            string encoded    = WebUtility.HtmlEncode(normalized);
            encoded = encoded.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
            encoded = encoded.Replace("\n", "<br>");
            return encoded;
        }

        // ── Private helpers — validation ──────────────────────────────────────

        /// <summary>
        /// Validates a single Law row and sets its <see cref="KBImportRow.Status"/> and
        /// <see cref="KBImportRow.StatusNote"/>. On success, also sets the computed
        /// <c>ArticleCode</c>/<c>Title</c>/<c>Tags</c>/<c>Summary</c>/<c>TitleCategory</c>/<c>ResolvedCategoryID</c>.
        /// </summary>
        private static void ValidateLawRow(KBImportRow row, HashSet<string> seenCodes, IKBArticleRepository articleRepo, KBImportCategoryResolver resolver)
        {
            string raw = row.ColA;

            // ── Column A ("Code รวม") — blank ⇒ Skip ──────────────────────────
            if (string.IsNullOrEmpty(raw))
            {
                row.Status     = "Skip";
                row.StatusNote = "คอลัมน์ A (Code รวม) ว่าง — ข้ามแถวนี้";
                return;
            }

            // ── Code รวม ต้องยาว 27 ตัว: [prefix 3][...][suffix 5 = "00000"] ───
            // prefix = CategoryCode ของกฎหมายฉบับนั้น (เช่น RVA — 1 Template ใช้ได้ทุกกฎหมาย หมวดหาเองตามรหัสนี้),
            // suffix "00000" = ตัวมาตราเอง; ArticleCode คือ 22 ตัวแรก. ผิดกติกาข้อใดก็เป็น Error
            if (raw.Length != 27)
            {
                row.Status     = "Error";
                row.StatusNote = string.Format(
                    "Code รวม: ความยาวไม่ถูกต้อง (ต้อง 27 ตัว แต่พบ {0}) — '{1}'", raw.Length, raw);
                return;
            }
            if (raw.Substring(22, 5) != "00000")
            {
                row.Status     = "Error";
                row.StatusNote = string.Format("Code รวม: 5 ตัวสุดท้ายต้องเป็น 00000 — '{0}'", raw);
                return;
            }
            KBCategory lawCategory;
            string     categoryError;
            if (!resolver.TryResolveLaw(raw.Substring(0, 3), out lawCategory, out categoryError))
            {
                row.Status     = "Error";
                row.StatusNote = string.Format("Code รวม: {0} — '{1}'", categoryError, raw);
                return;
            }
            string code = raw.Substring(0, 22);

            // ── ArticleCode already-known (exists / duplicate-in-file) ⇒ Skip ──
            if (seenCodes.Contains(code))
            {
                row.Status     = "Skip";
                row.StatusNote = string.Format("ArticleCode '{0}' ซ้ำกับแถวก่อนหน้าในไฟล์เดียวกัน — ข้ามแถวนี้", code);
                return;
            }

            if (articleRepo.ExistsByCode(code))
            {
                row.Status     = "Skip";
                row.StatusNote = string.Format("ArticleCode '{0}' มีอยู่แล้วในระบบ — ข้ามแถวนี้", code);
                seenCodes.Add(code);
                return;
            }

            seenCodes.Add(code);

            var errors = new List<string>();

            // ── Title source: ค่า Title ที่ Template map ไว้ (เช่น {A}) ถ้ามี ไม่งั้นใช้ Code รวม;
            // ตัวที่ 4-7 = เลขมาตรา 4 หลัก เช่น "0110" → มาตรา 110 ─────────────────
            int sectionNumber = 0;
            string titleSource = !string.IsNullOrWhiteSpace(row.RawTitle) ? row.RawTitle.Trim() : raw;
            string sectionDigits = titleSource.Length >= 7 ? titleSource.Substring(3, 4) : string.Empty;
            if (!Regex.IsMatch(sectionDigits, @"^\d{4}$") || !int.TryParse(sectionDigits, out sectionNumber) || sectionNumber <= 0)
                errors.Add(string.Format("เลขมาตราไม่ถูกต้อง: ตัวที่ 4-7 ของ '{0}' ต้องเป็นตัวเลข 4 หลักและมากกว่า 0", titleSource));

            // ── Column P — required (source of ContentHTML) ───────────────────
            if (string.IsNullOrWhiteSpace(row.RawContent))
                errors.Add("คอลัมน์ P (บัญญัติไว้ว่า): จำเป็นต้องระบุ");

            // ── Column N (วันที่ประกาศ) / O (วันที่มีผลบังคับใช้) — optional, but if
            // present must parse (already attempted in the reader; a non-empty raw
            // value with no parsed result means it failed to parse) ────────────
            if (!string.IsNullOrEmpty(row.RawPublishedDate) && !row.PublishedDateParsed.HasValue)
                errors.Add(string.Format("คอลัมน์ N (วันที่ประกาศ): ไม่สามารถแปลงวันที่ได้ — '{0}'", row.RawPublishedDate));

            if (!string.IsNullOrEmpty(row.RawEffectiveDate) && !row.EffectiveDateParsed.HasValue)
                errors.Add(string.Format("คอลัมน์ O (วันที่มีผลบังคับใช้): ไม่สามารถแปลงวันที่ได้ — '{0}'", row.RawEffectiveDate));

            if (errors.Count > 0)
            {
                row.Status     = "Error";
                row.StatusNote = string.Join("; ", errors);
                return;
            }

            row.Status             = "Valid";
            row.StatusNote         = null;
            row.ArticleCode        = code;
            row.Title              = "มาตรา " + sectionNumber;
            row.Tags               = row.Title;
            row.Summary            = string.IsNullOrWhiteSpace(row.RawSummary) ? null : row.RawSummary.Trim();
            row.TitleCategory      = string.IsNullOrWhiteSpace(row.RawTitleCategory) ? null : row.RawTitleCategory.Trim();
            row.ResolvedCategoryID = lawCategory.CategoryID;
            row.DocumentTypeCode   = lawCategory.CategoryCode;
            row.DocumentTypeLabel  = "มาตรา";
        }

        /// <summary>
        /// Validates a single NonLaw row and sets its <see cref="KBImportRow.Status"/> and
        /// <see cref="KBImportRow.StatusNote"/>. On success, also sets the computed
        /// <c>ArticleCode</c>/<c>Title</c>/<c>Summary</c>/<c>TitleCategory</c>/<c>ResolvedCategoryID</c>/
        /// <c>ResolvedParentArticleIds</c>.
        /// ArticleCode ที่มีในระบบแล้ว (<see cref="KBImportRow.ExistingArticleId"/>) ไม่ใช่ Error — ตรวจเฉพาะสิ่งที่ใช้เติมของที่ขาด
        /// (ดู <see cref="ClassifyExistingRow"/>); ไม่ตรวจ Title/เนื้อหา/วันที่ เพราะไม่เขียนทับบทความเดิม.
        /// </summary>
        private void ValidateNonLawRow(
            KBImportRow row, HashSet<string> seenCodes,
            Dictionary<string, KBArticle> parentCache, KBImportCategoryResolver resolver)
        {
            // ── ArticleCode = Code รวมที่ Template map ไว้ (บังคับ map เสมอ → row.ColA != null) ──
            // ถ้าเซลล์มีหลายบรรทัด เลือกบรรทัดแรกที่เป็นรูปแบบ Code ประกาศ/คำสั่งที่ถูกต้อง (ไม่มี = บรรทัดแรก เพื่อรายงาน error)
            // ประเภทเอกสาร (ป้ายแสดงผล + หมวด) อ่านจากตัวที่ 23 ของรหัสนี้ ไม่อ่านคอลัมน์ C/D
            string mappedCode = null;
            if (row.ColA != null)
            {
                var mappedLines = row.ColA.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
                mappedCode = mappedLines.FirstOrDefault(KBImportLayoutTypeCatalog.IsNonLawCode) ?? mappedLines.FirstOrDefault();
            }
            row.ArticleCode = mappedCode;

            var errors = new List<string>();

            if (row.ColA == null)
            {
                errors.Add("Template ไม่ได้ Map ArticleCode (Code รวม)");
            }
            else if (string.IsNullOrWhiteSpace(mappedCode))
            {
                errors.Add("Code รวม (ArticleCode ที่ Template map ไว้) ว่าง");
            }
            else if (seenCodes.Contains(mappedCode))
            {
                errors.Add(string.Format("ArticleCode '{0}' ซ้ำกับแถวอื่นในไฟล์", mappedCode));
            }
            else
            {
                seenCodes.Add(mappedCode);

                // มีในระบบแล้ว → ดึง ArticleID ไว้ใช้ต่อ (เติมไฟล์แนบ/link ที่ตกหล่นตอน import) ไม่ใช่ Error
                if (_articleRepo.ExistsByCode(mappedCode))
                {
                    KBArticle existing = _articleRepo.GetByCode(mappedCode);   // null = ถูก soft-delete (ExistsByCode ไม่กรอง IsDelete)
                    if (existing == null)
                        errors.Add(string.Format("ArticleCode '{0}' มีอยู่ในระบบแต่ถูกลบ (IsDelete) — เติมข้อมูลไม่ได้", mappedCode));
                    else
                        row.ExistingArticleId = existing.ArticleID;
                }
            }

            // บทความเดิม: ไม่เขียนทับ Title/เนื้อหา/วันที่ จึงไม่บังคับกรอก — ตรวจเฉพาะไฟล์แนบ/กฎหมายแม่ที่ใช้เติมของที่ขาด
            bool isExisting = row.ExistingArticleId > 0;

            // ── รูปแบบ Code รวม + ตัวที่ 23 (ประเภท) → หมวดของแถวนี้ (หาเอง ไม่ผูกกับ Template) ──────────────
            if (!string.IsNullOrWhiteSpace(mappedCode))
            {
                string expectedCode = KBImportLayoutTypeCatalog.GetNonLawCategoryCode(mappedCode);

                if (expectedCode == null)
                {
                    errors.Add(string.Format(
                        "Code รวม: ต้องยาว 27 ตัว, 5 ตัวสุดท้ายห้ามเป็น 00000 และตัวที่ 23 ต้องเป็น 1-6 — '{0}'", mappedCode));
                }
                else
                {
                    KBCategory typeCategory;
                    string     categoryError;
                    if (resolver.TryResolveNonLaw(expectedCode, out typeCategory, out categoryError))
                    {
                        row.ResolvedCategoryID = typeCategory.CategoryID;
                        row.DocumentTypeCode   = typeCategory.CategoryCode;
                    }
                    else
                    {
                        errors.Add(string.Format("Code รวม: ตัวที่ 23 = {0} ({1}) — {2} — '{3}'",
                            mappedCode[22], expectedCode, categoryError, mappedCode));
                    }

                    row.DocumentTypeLabel  = KBImportLayoutTypeCatalog.GetNonLawTypeLabel(mappedCode[22]);

                    // กฎหมายแม่ = 22 ตัวแรกของ Code รวม (ใช้ resolve + สร้าง KBArticleLink ไป-กลับ) ไม่อ่านคอลัมน์ B
                    string parentPart = mappedCode.Substring(0, 22);
                    row.ParentCodes = new List<string> { parentPart };
                }
            }

            if (!isExisting)
            {
                if (string.IsNullOrWhiteSpace(row.RawTitle))
                    errors.Add("Title (ชื่อ) ห้ามว่าง — ตรวจสอบว่า Template Map Title แล้ว");

                if (string.IsNullOrWhiteSpace(row.RawContent))
                    errors.Add("ContentHTML (บัญญัติไว้ว่า) ห้ามว่าง — ตรวจสอบว่า Template Map ContentHTML แล้ว");

                if (string.IsNullOrWhiteSpace(row.AttachmentFileName))
                    errors.Add("Attachment (ชื่อไฟล์แนบ) ห้ามว่าง — ตรวจสอบว่า Template Map Attachment แล้ว");
            }

            // ParentCodes ว่าง = Code รวมผิดรูปแบบ ซึ่งรายงาน error ไปแล้วด้านบน
            if (row.ParentCodes.Count > 0)
            {
                var resolvedIds = new List<int>();
                var notFound    = new List<string>();

                foreach (var code in row.ParentCodes)
                {
                    KBArticle match;
                    if (!parentCache.TryGetValue(code, out match))
                    {
                        match = _articleRepo.GetByCode(code);
                        parentCache[code] = match;
                    }

                    if (match == null) notFound.Add(code);
                    else               resolvedIds.Add(match.ArticleID);
                }

                row.ResolvedParentArticleIds = resolvedIds;
                // บทความเดิมที่หากฎหมายแม่ไม่พบ: ไม่ใช่ Error — แค่ไม่มี link กฎหมายแม่ให้เติม (ไฟล์แนบยังเติมได้)
                if (notFound.Count > 0 && !isExisting)
                    errors.Add("ไม่พบกฎหมายแม่ในระบบ: " + string.Join(", ", notFound));
            }

            if (!isExisting && !string.IsNullOrEmpty(row.RawPublishedDate) && !row.PublishedDateParsed.HasValue)
                errors.Add(string.Format("PublishedDate (วันที่ประกาศ): ไม่สามารถแปลงวันที่ได้ — '{0}'", row.RawPublishedDate));

            if (!isExisting && !string.IsNullOrEmpty(row.RawEffectiveDate) && !row.EffectiveDateParsed.HasValue)
                errors.Add(string.Format("EffectiveDate (วันที่มีผลบังคับใช้): ไม่สามารถแปลงวันที่ได้ — '{0}'", row.RawEffectiveDate));

            if (errors.Count > 0)
            {
                row.Status     = "Error";
                row.StatusNote = string.Join("; ", errors);
                return;
            }

            row.Title          = row.RawTitle;
            row.Summary        = row.RawSummary;
            row.TitleCategory  = row.RawTitleCategory;

            if (isExisting)
            {
                ClassifyExistingRow(row);
                return;
            }

            row.Status     = "Valid";
            row.StatusNote = null;
        }

        /// <summary>
        /// บทความมีอยู่แล้ว: ไม่ insert ซ้ำ — ดูว่าขาดอะไร (ไฟล์แนบ / link ไฟล์ / link กฎหมายแม่) เพื่อเติมตอน import.
        /// ไม่ขาดอะไร = Skip, ขาด = Valid (พร้อมเติม), ไม่มีชื่อไฟล์แนบ = Skip (ข้ามเฉพาะ Code นี้). Read-only DB access
        /// </summary>
        private void ClassifyExistingRow(KBImportRow row)
        {
            int articleId = row.ExistingArticleId;
            string known  = string.Format("ArticleCode '{0}' มีในระบบแล้ว (ArticleID {1})", row.ArticleCode, articleId);

            if (string.IsNullOrWhiteSpace(row.AttachmentFileName))
            {
                row.Status     = "Skip";
                row.StatusNote = known + " — ไม่มีชื่อไฟล์แนบ ข้ามรหัสนี้";
                return;
            }

            var missing = FindMissingParts(row, articleId);
            if (missing.Count == 0)
            {
                row.Status     = "Skip";
                row.StatusNote = known + " ครบถ้วนแล้ว — ไม่มีอะไรให้เติม";
                return;
            }

            row.Status     = "Valid";
            row.StatusNote = known + " — จะเติม: " + string.Join(", ", missing);
        }

        private List<string> FindMissingParts(KBImportRow row, int articleId)
        {
            var missing = new List<string>();

            var attachments = _articleService.GetArticleAttachments(articleId).ToList();
            if (attachments.Count == 0)
                missing.Add("ไฟล์แนบ");
            else if (attachments.Any(a => !_linkRepo.Exists(articleId, (byte)KBEntityType.Folder, a.FileID.ToString())))
                missing.Add("link ไฟล์");

            for (int i = 0; i < row.ResolvedParentArticleIds.Count; i++)
            {
                int parentId = row.ResolvedParentArticleIds[i];
                if (!_linkRepo.Exists(articleId, (byte)KBEntityType.Law, parentId.ToString()) ||
                    !_linkRepo.Exists(parentId, (byte)KBEntityType.Law, articleId.ToString()))
                {
                    missing.Add("link กฎหมายแม่");
                    break;
                }
            }

            return missing;
        }

        // ── Private helpers — misc ────────────────────────────────────────────

        /// <summary>ไฟล์แนบของ Excel row หนึ่ง: ผลโหลด + ผลอัปโหลดเข้า DMS (ทำครั้งเดียว ใช้ร่วมกันทุก ArticleCode ของ row นั้น)</summary>
        private sealed class RowFile
        {
            public int                   RowNumber;
            public LawFileDownloadResult Download;
            public bool                  UploadAttempted;
            public int?                  FileId;
            public string                UploadError;
        }

        /// <summary>
        /// เก็บ <see cref="RowFile"/> ของ Excel row ล่าสุด. ArticleCode ที่แตกจาก row เดียวกันอยู่ติดกัน จึงใช้ไฟล์เดียวกัน —
        /// ถ้าอัปโหลดแยกต่อ Code DMS จะตั้งชื่อซ้ำเป็น "ชื่อ (1)" ได้ไฟล์ซ้ำ; เก็บเฉพาะ row ล่าสุดเพื่อไม่ถือ byte[] ของไฟล์เก่าค้างหน่วยความจำ
        /// </summary>
        private sealed class RowFileCache
        {
            private RowFile _current;

            public RowFile For(KBImportRow row, LawFileDownloader downloader)
            {
                if (_current == null || _current.RowNumber != row.RowNumber)
                {
                    _current = new RowFile
                    {
                        RowNumber = row.RowNumber,
                        Download  = downloader.Download(row.AttachmentUrl, row.AttachmentFileName),
                    };
                }
                return _current;
            }
        }

        /// <summary>อัปโหลดเข้า DMS ครั้งเดียวต่อ Excel row — ครั้งถัดไปคืน FileID เดิม; คืน null พร้อม <paramref name="errorMessage"/> เมื่ออัปโหลดไม่สำเร็จ</summary>
        private int? EnsureUploaded(RowFile file, UserContext user, out string errorMessage)
        {
            if (!file.UploadAttempted)
            {
                file.UploadAttempted = true;
                string error;
                file.FileId      = UploadToDms(file.Download, user, out error);
                file.UploadError = error;
            }

            errorMessage = file.UploadError;
            return file.FileId;
        }

        private int? UploadToDms(LawFileDownloadResult download, UserContext user, out string errorMessage)
        {
            errorMessage = null;

            using (var stream = new MemoryStream(download.Content))
            {
                var request = new FileUploadRequest
                {
                    FileName          = download.FileName,
                    FileStream        = stream,
                    FileSizeBytes     = download.Content.Length,
                    MimeType          = download.MimeType,
                    OverwriteExisting = false,
                };

                var results = _fileService.UploadFiles(
                    new List<FileUploadRequest> { request }, AttachmentFolderId, user);

                var result = results == null ? null : results.FirstOrDefault();

                if (result == null)
                {
                    errorMessage = "ไม่ได้รับผลลัพธ์จากระบบจัดเก็บไฟล์";
                    return null;
                }

                if (!result.Success || !result.FileDocumentId.HasValue)
                {
                    errorMessage = result.ErrorMessage ?? "ไม่ทราบสาเหตุ";
                    return null;
                }

                return result.FileDocumentId.Value;
            }
        }

        /// <summary>
        /// Truncates <paramref name="message"/> to at most 200 characters.
        /// </summary>
        private static string TruncateTo200(string message)
        {
            if (message == null) return string.Empty;
            return message.Length <= 200 ? message : message.Substring(0, 200);
        }
    }
}
