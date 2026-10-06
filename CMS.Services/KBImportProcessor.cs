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
using System.Text;
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
    /// ArticleCode ที่มีในระบบแล้ว (ทั้งกฎหมายและไม่ใช่กฎหมาย) ไม่ insert ซ้ำ แต่ UPDATE บทความเดิมทุก field ที่ Template/Excel ให้ค่ามา
    /// (เซลล์ว่าง/ field ที่ไม่ได้ map = คงค่าเดิม; ค่าคงที่ตอน insert เช่น Status ไม่ถูกเขียนทับ) และตั้ง UpdatedBy = 'system',
    /// UpdatedDate = เวลาที่ update — ดู <see cref="UpdateExistingLawArticle"/> / <see cref="UpdateExistingNonLawArticle"/>.
    /// ไฟล์แนบที่มีอยู่แล้วไม่ดาวน์โหลด/อัปโหลดซ้ำ.
    /// CategoryID is resolved per row from digit 23 of the ArticleCode (the document type category whose
    /// <c>KBCategory.ImportTypeDigit</c> equals it, e.g. 2 → OR — a master in m_KBCategory, not hard-coded),
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

        /// <summary>UpdatedBy ที่ตั้งตอน import อัปเดตบทความที่มีอยู่แล้ว (UpdatedDate = เวลาที่ update)</summary>
        private const string ImportUpdatedBy = "system";

        /// <summary>Folder ปลายทางของไฟล์แนบ (NonLaw) — ตรงกับที่ KBEditorDialog ใช้ ("KMS Attachments")</summary>
        /// <remarks>ใช้เมื่อผู้เรียกไม่ระบุโฟลเดอร์ปลายทางที่ผู้ใช้เลือกในหน้า import</remarks>
        private const int DefaultAttachmentFolderId = 437;

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
        /// link the parent law. Rows whose article already exists
        /// (<see cref="KBImportRow.ExistingArticleId"/> &gt; 0) are not inserted again — the existing article is UPDATED
        /// (see <see cref="UpdateExistingLawArticle"/>/<see cref="UpdateExistingNonLawArticle"/>); a NonLaw row becomes <c>Skip</c> only
        /// when the article has no attachment yet and its source file is unavailable.
        /// Each row is wrapped in its own <c>try/catch</c>;
        /// a failure on one row increments <paramref name="errorCount"/> and processing continues.
        /// </summary>
        /// <param name="resolvedArticleType">
        /// ArticleType configured on the KB Import Template selected in the Wizard — applied to
        /// every row in this batch (both Law and NonLaw layouts). Must be greater than 0.
        /// </param>
        /// <param name="attachmentFolder">
        /// NonLaw rows only: โฟลเดอร์ในเครื่องที่ผู้ใช้เลือกไว้เก็บไฟล์แนบ (หาไฟล์ตามชื่อที่ Template map เป็น Attachment — ดู <see cref="LawFileDownloader"/>)
        /// </param>
        /// <param name="dmsFolderId">
        /// NonLaw rows only: โฟลเดอร์ปลายทางใน DMS ที่ผู้ใช้เลือกเก็บไฟล์แนบ (ผ่านการตรวจสิทธิ์แล้ว); null = ใช้ค่าเริ่มต้น (<see cref="DefaultAttachmentFolderId"/>)
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
            string attachmentFolder,
            int? dmsFolderId,
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
            var files = new RowFileCache(_downloader, attachmentFolder, dmsFolderId ?? DefaultAttachmentFolderId);

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

            // บทความมีอยู่แล้ว (ตรวจพบตอน validate) → UPDATE บทความเดิม ไม่ insert ซ้ำ
            if (row.ExistingArticleId > 0)
            {
                UpdateExistingLawArticle(row, createdBy, resolvedArticleType);
                return;
            }

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

            // บทความมีอยู่แล้ว (ตรวจพบตอน validate) — ไม่ insert ซ้ำ แต่ UPDATE บทความเดิม
            if (row.ExistingArticleId > 0)
            {
                UpdateExistingNonLawArticle(row, user, createdBy, resolvedArticleType, files);
                return;
            }

            // Defensive re-check — เพิ่งมีคนสร้าง Code นี้หลัง validate (ไม่เจอตอน validate จึงยังไม่มี ExistingArticleId)
            if (_articleRepo.ExistsByCode(row.ArticleCode))
                throw new InvalidOperationException(string.Format(
                    "ArticleCode '{0}' มีอยู่ในระบบแล้ว", row.ArticleCode));

            // Defensive re-check — รูปแบบ Code รวมถูกตรวจแล้วตอน ValidateRows; ตรวจซ้ำเพราะ InsertArticles
            // อาจถูกเรียกเมื่อไรก็ได้หลังจากนั้น
            if (!KBImportLayoutTypeCatalog.IsNonLawCodeFormat(row.ArticleCode))
                throw new InvalidOperationException(string.Format(
                    "{0} — '{1}'", KBImportLayoutTypeCatalog.NonLawCodeFormatMessage, row.ArticleCode));
            if (row.ResolvedCategoryID <= 0)
                throw new InvalidOperationException("ไม่พบหมวด (CategoryID) ของแถวนี้ — ต้องผ่านการ validate ก่อน insert");

            // ── ไฟล์แนบ ──────────────────────────────────────────────────────
            int? fileId = null;
            var rowFile = files.For(row);

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
        /// กฎหมาย — บทความมีอยู่แล้ว (<see cref="KBImportRow.ExistingArticleId"/>): UPDATE ทุก field ที่ Excel/Template ให้ค่ามา
        /// (<see cref="GetUpdateColumns"/>) + UpdatedBy = 'system', UpdatedDate = ตอนนี้. ถ้า ContentHTML ต่างจากที่อยู่ในระบบ เพิ่ม KBVersion ใหม่
        /// (เหมือนหน้าแก้ไขบทความ); เหมือนกัน = ไม่เพิ่ม version. กฎหมายไม่มีไฟล์แนบ จึงไม่มีขั้นดาวน์โหลด/อัปโหลด
        /// </summary>
        private void UpdateExistingLawArticle(KBImportRow row, string createdBy, byte resolvedArticleType)
        {
            int articleId = row.ExistingArticleId;

            var columns = GetUpdateColumns(row, true);
            var patch   = BuildImportPatch(row, resolvedArticleType);

            KBVersion newVersion = null;
            if (columns.Contains("ContentHTML"))
            {
                KBArticle current = _articleRepo.GetById(articleId);
                if (current != null && !string.Equals(current.ContentHTML, patch.ContentHTML, StringComparison.Ordinal))
                {
                    newVersion = new KBVersion
                    {
                        ContentSnapshot   = patch.ContentHTML,
                        ChangeDescription = "แก้ไขจาก Excel import",
                        CreatedBy         = createdBy,
                        CreatedDate       = DateTime.Now,
                    };
                }
            }

            _articleRepo.UpdateFromImport(articleId, patch, columns, ImportUpdatedBy, newVersion);

            row.ImportedArticleId = articleId;
            row.Status            = "Valid";
            row.StatusNote        = TruncateTo200("อัปเดตแล้ว: " + string.Join(", ", columns) + (newVersion != null ? " (เพิ่ม version ใหม่)" : string.Empty));
        }

        /// <summary>
        /// ไม่ใช่กฎหมาย — บทความมีอยู่แล้ว (<see cref="KBImportRow.ExistingArticleId"/>), ตามลำดับ:
        /// 1) ไฟล์แนบ: มี record ใน t_KBArticleAttachment แล้ว → ไม่ต้องทำอะไรกับไฟล์ (ไม่ดาวน์โหลด/อัปโหลด) เติมเฉพาะ link ไฟล์
        ///    (t_KBArticleLink EntityType=1, EntityID=FileID) ที่ยังไม่มี; ยังไม่มี → ต้องโหลดไฟล์ต้นทางได้ (ไม่ได้ = "Skip" เฉพาะ ArticleCode นี้
        ///    ยังไม่ update อะไร) แล้วอัปโหลด (ครั้งเดียวต่อ Excel row) + attach + link
        /// 2) UPDATE ทุก field ที่ Excel/Template ให้ค่ามา (<see cref="GetUpdateColumns"/>) + UpdatedBy = 'system', UpdatedDate = ตอนนี้
        /// 3) link กฎหมายแม่ไป-กลับที่ยังไม่มี
        /// ไม่มี KBVersion (ตอน insert บทความที่ไม่ใช่กฎหมายก็ไม่สร้าง version)
        /// </summary>
        private void UpdateExistingNonLawArticle(KBImportRow row, UserContext user, string createdBy, byte resolvedArticleType, RowFileCache files)
        {
            int articleId = row.ExistingArticleId;
            row.ImportedArticleId = articleId;

            var added = new List<string>();   // ไฟล์แนบ/link ที่เติมให้
            var notes = new List<string>();   // ปัญหาย่อยที่ไม่ทำให้ทั้ง Code ล้มเหลว (เช่น link กฎหมายแม่)

            // ── ไฟล์แนบ ─────────────────────────────────────────────────────────────────
            var attachments = _articleService.GetArticleAttachments(articleId).ToList();
            if (attachments.Count == 0)
            {
                // ยังไม่เคยมีไฟล์ → ต้องโหลดไฟล์ต้นทาง; ไม่มีไฟล์/โหลดไม่ได้ → ข้ามเฉพาะ ArticleCode นี้
                var rowFile = files.For(row);
                if (!rowFile.Download.Success)
                {
                    row.Status     = "Skip";
                    row.StatusNote = TruncateTo200(string.Format(
                        "ArticleCode '{0}' (ArticleID {1}) มีในระบบแล้วแต่ยังไม่มีไฟล์แนบ — ข้าม: {2}",
                        row.ArticleCode, articleId, rowFile.Download.ErrorMessage));
                    return;
                }

                string uploadError;
                int? fileId = EnsureUploaded(rowFile, user, out uploadError);
                if (fileId == null)
                    throw new InvalidOperationException("อัปโหลดไฟล์แนบไม่สำเร็จ: " + uploadError);

                // ยังไม่มี record เลย → SaveArticleAttachments ปลอดภัย (DeleteAll ไม่มีอะไรให้ลบ) และทำ attachment + audit + link ให้ครบ.
                // ห้ามใช้เมื่อมี attachment อยู่แล้ว เพราะมันลบของเดิมทั้งหมดก่อน
                _articleService.SaveArticleAttachments(articleId, new List<int> { fileId.Value }, createdBy);
                row.UploadedFileId = fileId.Value;
                added.Add("ไฟล์แนบ");
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
                    added.Add(string.Format("link ไฟล์ {0} รายการ", linked));
            }

            // ── UPDATE field ──────────────────────────────────────────────────────────
            var columns = GetUpdateColumns(row, false);
            _articleRepo.UpdateFromImport(articleId, BuildImportPatch(row, resolvedArticleType), columns, ImportUpdatedBy, null);

            // ── link กฎหมายแม่ ──────────────────────────────────────────────────────────
            int parentLinks = LinkToParents(articleId, row, createdBy, notes);
            if (parentLinks > 0)
                added.Add(string.Format("link กฎหมายแม่ {0} รายการ", parentLinks));

            string summary = "อัปเดตแล้ว: " + string.Join(", ", columns);
            if (added.Count > 0)
                summary += "; เติม: " + string.Join(", ", added);

            if (notes.Count > 0)
            {
                row.Status     = "Warn";
                row.StatusNote = TruncateTo200(summary + "; " + string.Join("; ", notes));
            }
            else
            {
                row.Status     = "Valid";
                row.StatusNote = TruncateTo200(summary);
            }
        }

        /// <summary>
        /// คอลัมน์ของ t_KBArticle ที่จะเขียนทับบทความเดิมจาก row: เฉพาะ field ที่ Excel/Template ให้ค่าไม่ว่างมา (เซลล์ว่าง หรือ field ที่ Template
        /// ไม่ได้ map = คงค่าเดิม) + ArticleType เสมอ. ไม่แก้ ArticleCode และ CategoryID (ไม่ย้ายหมวดของบทความเดิม) และไม่รวมค่าคงที่
        /// ตอน insert (Status, ConfidentialityLevel, IsOfficialPolicy, CurrentVersion, Created*) — กันบทความที่ Publish ไปแล้วถูกดึงกลับเป็น Review
        /// ทุกครั้งที่ upload ซ้ำ
        /// </summary>
        private static List<string> GetUpdateColumns(KBImportRow row, bool isLaw)
        {
            var columns = new List<string>();

            if (!string.IsNullOrWhiteSpace(row.Title))         columns.Add("Title");
            if (isLaw && !string.IsNullOrWhiteSpace(row.Tags)) columns.Add("Tags");
            if (!string.IsNullOrWhiteSpace(row.TitleCategory)) columns.Add("TitleCategory");
            if (!string.IsNullOrWhiteSpace(row.RawContent))    columns.Add("ContentHTML");
            if (!string.IsNullOrWhiteSpace(row.Summary))       columns.Add("Summary");
            if (row.PublishedDateParsed.HasValue)              columns.Add("PublishedDate");
            if (row.EffectiveDateParsed.HasValue)              columns.Add("EffectiveDate");
            columns.Add("ArticleType");

            return columns;
        }

        /// <summary>ค่าที่จะเขียนทับบทความเดิม — แปลงด้วยกฎเดียวกับตอน insert (เช่น ContentHTML ผ่าน <see cref="ConvertPlainTextToHtml"/>); ใช้คู่กับ <see cref="GetUpdateColumns"/></summary>
        private static KBArticle BuildImportPatch(KBImportRow row, byte resolvedArticleType)
        {
            return new KBArticle
            {
                Title         = row.Title,
                Tags          = row.Tags,
                TitleCategory = row.TitleCategory,
                ContentHTML   = ConvertPlainTextToHtml(row.RawContent),
                Summary       = row.Summary,
                PublishedDate = row.PublishedDateParsed,
                EffectiveDate = row.EffectiveDateParsed,
                ArticleType   = resolvedArticleType,
            };
        }

        /// <summary>ข้อความ preview ของแถวที่บทความมีอยู่แล้ว: จะ update field ไหน</summary>
        private static string BuildUpdateNote(KBImportRow row, bool isLaw)
        {
            return string.Format("ArticleCode '{0}' มีในระบบแล้ว (ArticleID {1}) — จะอัปเดต: {2}",
                row.ArticleCode, row.ExistingArticleId, string.Join(", ", GetUpdateColumns(row, isLaw)));
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
        /// whitespace would collapse per the HTML whitespace spec, so each line is HTML-encoded and
        /// the whitespace is made visible: line break → <c>&lt;br&gt;</c>, <c>\t</c> → 4x <c>&amp;nbsp;</c>,
        /// leading/trailing spaces (indent) → <c>&amp;nbsp;</c>, a run of spaces → alternating space/<c>&amp;nbsp;</c>
        /// (single spaces between words stay plain so the text still wraps).
        /// </summary>
        internal static string ConvertPlainTextToHtml(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            string[] lines = raw.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            var html = new StringBuilder(raw.Length + 16);
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0) html.Append("<br>");
                html.Append(EncodeLineForHtml(lines[i]));
            }
            return html.ToString();
        }

        private static string EncodeLineForHtml(string line)
        {
            string encoded = WebUtility.HtmlEncode(line).Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");

            int start = 0;
            while (start < encoded.Length && encoded[start] == ' ') start++;
            int end = encoded.Length;
            while (end > start && encoded[end - 1] == ' ') end--;

            string core = encoded.Substring(start, end - start).Replace("  ", " &nbsp;");   // "a    b" → "a &nbsp; &nbsp;b" (4 spaces kept)

            var sb = new StringBuilder(encoded.Length + 8);
            for (int i = 0; i < start; i++) sb.Append("&nbsp;");
            sb.Append(core);
            for (int i = end; i < encoded.Length; i++) sb.Append("&nbsp;");
            return sb.ToString();
        }

        /// <summary>
        /// ข้อความที่เก็บเป็นข้อความล้วน (Title / TitleCategory / Summary — แสดงใน Label/TextBox/Grid ไม่ใช่ HTML): คงทุกอย่างตามต้นฉบับ
        /// (เว้นวรรค, indent, tab, ช่องว่างหัว-ท้าย) ปรับเฉพาะตัวขึ้นบรรทัดใหม่เป็น CRLF เหมือนที่ TextBox ของหน้าแก้ไขบทความบันทึก —
        /// Excel ใช้ LF เดี่ยว ซึ่ง TextBox แบบหลายบรรทัดจะไม่แสดงเป็นการขึ้นบรรทัด
        /// </summary>
        internal static string ToPlainText(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            return raw.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
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
            // prefix = CategoryCode ของกฎหมายฉบับนั้น (เช่น RVA — 1 Template ใช้ได้ทุกกฎหมาย หมวดหาเองตามรหัสนี้);
            // กฎหมายที่แยกหมวดตามปี (เช่น PLI2535/PLI2540 ใต้ PLI) เลือกหมวดปีจากตัวที่ 8-10 + "2" นำหน้าด้วย (ดู TryResolveLawArticle),
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
            KBCategory        lawCategory;
            string            categoryError;
            KBMissingCategory missingCategory;
            if (!resolver.TryResolveLawArticle(raw, out lawCategory, out categoryError, out missingCategory))
            {
                row.MissingCategory = missingCategory;
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

            // มีในระบบแล้ว → ไม่ใช่ Skip/Error: UPDATE บทความเดิมตอน import (ดึง ArticleID ไว้ใช้ต่อ)
            if (articleRepo.ExistsByCode(code))
            {
                KBArticle existing = articleRepo.GetByCode(code);   // null = ถูก soft-delete (ExistsByCode ไม่กรอง IsDelete)
                if (existing == null)
                {
                    row.Status     = "Error";
                    row.StatusNote = string.Format("ArticleCode '{0}' มีอยู่ในระบบแต่ถูกลบ (IsDelete) — อัปเดตข้อมูลไม่ได้", code);
                    seenCodes.Add(code);
                    return;
                }
                row.ExistingArticleId = existing.ArticleID;
            }

            seenCodes.Add(code);

            // บทความเดิม: เซลล์เนื้อหาว่าง = คงเนื้อหาเดิม จึงไม่บังคับกรอก
            bool isExisting = row.ExistingArticleId > 0;

            var errors = new List<string>();

            // ── Title source: ค่า Title ที่ Template map ไว้ (เช่น {A}) ถ้ามี ไม่งั้นใช้ Code รวม;
            // ตัวที่ 4-7 = เลขมาตรา 4 หลัก เช่น "0110" → มาตรา 110 ─────────────────
            int sectionNumber = 0;
            string titleSource = !string.IsNullOrWhiteSpace(row.RawTitle) ? row.RawTitle.Trim() : raw;
            string sectionDigits = titleSource.Length >= 7 ? titleSource.Substring(3, 4) : string.Empty;
            if (!Regex.IsMatch(sectionDigits, @"^\d{4}$") || !int.TryParse(sectionDigits, out sectionNumber) || sectionNumber <= 0)
                errors.Add(string.Format("เลขมาตราไม่ถูกต้อง: ตัวที่ 4-7 ของ '{0}' ต้องเป็นตัวเลข 4 หลักและมากกว่า 0", titleSource));

            // ── Column P — required (source of ContentHTML) ───────────────────
            if (!isExisting && string.IsNullOrWhiteSpace(row.RawContent))
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
            row.Summary            = string.IsNullOrWhiteSpace(row.RawSummary) ? null : ToPlainText(row.RawSummary);
            row.TitleCategory      = string.IsNullOrWhiteSpace(row.RawTitleCategory) ? null : ToPlainText(row.RawTitleCategory);
            row.ResolvedCategoryID = lawCategory.CategoryID;
            row.ResolvedCategoryName = lawCategory.CategoryName;
            row.DocumentTypeCode   = lawCategory.CategoryCode;
            row.DocumentTypeLabel  = "มาตรา";

            if (isExisting)
                row.StatusNote = BuildUpdateNote(row, true);
        }

        /// <summary>
        /// Validates a single NonLaw row and sets its <see cref="KBImportRow.Status"/> and
        /// <see cref="KBImportRow.StatusNote"/>. On success, also sets the computed
        /// <c>ArticleCode</c>/<c>Title</c>/<c>Summary</c>/<c>TitleCategory</c>/<c>ResolvedCategoryID</c>/
        /// <c>ResolvedParentArticleIds</c>.
        /// ArticleCode ที่มีในระบบแล้ว (<see cref="KBImportRow.ExistingArticleId"/>) ไม่ใช่ Error/Skip — จะ UPDATE บทความเดิม (ดู
        /// <see cref="UpdateExistingNonLawArticle"/>): Title/เนื้อหา/ไฟล์แนบไม่บังคับกรอก (ว่าง = คงค่าเดิม) แต่วันที่ที่กรอกมาต้องแปลงได้.
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
                mappedCode = mappedLines.FirstOrDefault(KBImportLayoutTypeCatalog.IsNonLawCodeFormat) ?? mappedLines.FirstOrDefault();
            }
            row.ArticleCode = mappedCode;

            var errors = new List<string>();

            // ── หมวดของแถวนี้ — ตรวจก่อนอย่างอื่น (ไม่ผูกกับ Template; ดูจาก ArticleCode + ตาราง m_KBCategory เท่านั้น):
            //    ตัวที่ 23 (ประเภทเอกสาร) → หมวดประเภท เช่น OR, แล้วต่อรหัสปี (ตัวที่ 8-10 + "2" นำหน้า) → หมวดปี เช่น OR2535
            //    หมวดไม่พบ = Error + row.MissingCategory บอกว่าต้องสร้างหมวดอะไร (controller สรุปแจ้งผู้ใช้ที่หน้าจอ) ──────────
            if (!string.IsNullOrWhiteSpace(mappedCode))
            {
                if (!KBImportLayoutTypeCatalog.IsNonLawCodeFormat(mappedCode))
                {
                    errors.Add(string.Format("{0} — '{1}'", KBImportLayoutTypeCatalog.NonLawCodeFormatMessage, mappedCode));
                }
                else
                {
                    KBCategory        resolvedCategory;
                    string            categoryError;
                    KBMissingCategory missingCategory;
                    if (resolver.TryResolveNonLawArticle(mappedCode, out resolvedCategory, out categoryError, out missingCategory))
                    {
                        row.ResolvedCategoryID   = resolvedCategory.CategoryID;
                        row.ResolvedCategoryName = resolvedCategory.CategoryName;
                        row.DocumentTypeCode     = resolvedCategory.CategoryCode;
                    }
                    else
                    {
                        row.MissingCategory = missingCategory;

                        // รหัสประเภทที่ตั้งไว้แล้วแต่หมวดมีปัญหา (ปิดใช้งาน ฯลฯ) ใส่รหัสหมวดกำกับ เช่น "ตัวที่ 23 = 2 (OR)"; รหัสที่ยังไม่มีหมวดใช้ไม่มีวงเล็บ
                        KBCategory typeCategory = resolver.NonLawTypes.FindByArticleCode(mappedCode);
                        string typeCodeNote = typeCategory == null ? string.Empty : " (" + (typeCategory.CategoryCode ?? string.Empty).Trim() + ")";
                        errors.Add(string.Format("Code รวม: ตัวที่ 23 = {0}{1} — {2} — '{3}'",
                            mappedCode[KBImportLayoutTypeCatalog.NonLawTypeDigitIndex], typeCodeNote, categoryError, mappedCode));
                    }

                    row.DocumentTypeLabel = resolver.NonLawTypes.GetTypeLabel(mappedCode[KBImportLayoutTypeCatalog.NonLawTypeDigitIndex]);

                    // กฎหมายแม่ = 22 ตัวแรกของ Code รวม (ใช้ resolve + สร้าง KBArticleLink ไป-กลับ) ไม่อ่านคอลัมน์ B
                    row.ParentCodes = new List<string> { mappedCode.Substring(0, 22) };
                }
            }

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

            if (!string.IsNullOrEmpty(row.RawPublishedDate) && !row.PublishedDateParsed.HasValue)
                errors.Add(string.Format("PublishedDate (วันที่ประกาศ): ไม่สามารถแปลงวันที่ได้ — '{0}'", row.RawPublishedDate));

            if (!string.IsNullOrEmpty(row.RawEffectiveDate) && !row.EffectiveDateParsed.HasValue)
                errors.Add(string.Format("EffectiveDate (วันที่มีผลบังคับใช้): ไม่สามารถแปลงวันที่ได้ — '{0}'", row.RawEffectiveDate));

            if (errors.Count > 0)
            {
                row.Status     = "Error";
                row.StatusNote = string.Join("; ", errors);
                return;
            }

            row.Title          = ToPlainText(row.RawTitle);
            row.Summary        = ToPlainText(row.RawSummary);
            row.TitleCategory  = ToPlainText(row.RawTitleCategory);

            row.Status     = "Valid";
            row.StatusNote = isExisting ? BuildUpdateNote(row, false) : null;
        }

        // ── Private helpers — misc ────────────────────────────────────────────

        /// <summary>ไฟล์แนบของ Excel row หนึ่ง: ผลโหลด + ผลอัปโหลดเข้า DMS (ทำครั้งเดียว ใช้ร่วมกันทุก ArticleCode ของ row นั้น)</summary>
        private sealed class RowFile
        {
            public int                   RowNumber;
            public LawFileDownloadResult Download;
            public bool                  UploadAttempted;
            public int                   DmsFolderId;
            public int?                  FileId;
            public string                UploadError;
        }

        /// <summary>
        /// เก็บ <see cref="RowFile"/> ของ Excel row ล่าสุด. ArticleCode ที่แตกจาก row เดียวกันอยู่ติดกัน จึงใช้ไฟล์เดียวกัน —
        /// ถ้าอัปโหลดแยกต่อ Code DMS จะตั้งชื่อซ้ำเป็น "ชื่อ (1)" ได้ไฟล์ซ้ำ; เก็บเฉพาะ row ล่าสุดเพื่อไม่ถือ byte[] ของไฟล์เก่าค้างหน่วยความจำ
        /// </summary>
        private sealed class RowFileCache
        {
            private readonly LawFileDownloader _downloader;
            private readonly string            _folder;
            private readonly int               _dmsFolderId;
            private RowFile                    _current;

            public RowFileCache(LawFileDownloader downloader, string folder, int dmsFolderId)
            {
                _downloader  = downloader;
                _folder      = folder;
                _dmsFolderId = dmsFolderId;
            }

            public RowFile For(KBImportRow row)
            {
                if (_current == null || _current.RowNumber != row.RowNumber)
                {
                    _current = new RowFile
                    {
                        RowNumber   = row.RowNumber,
                        DmsFolderId = _dmsFolderId,
                        Download    = _downloader.Download(_folder, row.AttachmentUrl, row.AttachmentFileName),
                    };
                }
                return _current;
            }
        }

        /// <summary>อัปโหลดเข้า DMS ครั้งเดียวต่อ Excel row — ครั้งถัดไปคืน FileID เดิม; คืน null พร้อม <paramref name="errorMessage"/> เมื่ออัปโหลดไม่สำเร็จ</summary>
        /// <remarks>โฟลเดอร์ปลายทางคือ <see cref="RowFile.DmsFolderId"/> ที่ผู้ใช้เลือกในหน้า import; OverwriteExisting = false (ชื่อซ้ำ → "ชื่อ copy(N).pdf")</remarks>
        private int? EnsureUploaded(RowFile file, UserContext user, out string errorMessage)
        {
            if (!file.UploadAttempted)
            {
                file.UploadAttempted = true;
                string error;
                file.FileId      = UploadToDms(file.Download, user, file.DmsFolderId, out error);
                file.UploadError = error;
            }

            errorMessage = file.UploadError;
            return file.FileId;
        }

        private int? UploadToDms(LawFileDownloadResult download, UserContext user, int dmsFolderId, out string errorMessage)
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
                    new List<FileUploadRequest> { request }, dmsFolderId, user);

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
