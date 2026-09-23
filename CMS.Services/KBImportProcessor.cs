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
    /// (<c>KMS.t_KBArticle</c>, <c>KMS.t_KBVersion</c>). CategoryID is resolved from the
    /// ArticleCode's 3-letter prefix — no category upsert is needed.
    ///
    /// <b>Order</b> ("ประกาศ/คำสั่ง" rows) — Phase 1: row validation, including resolving each
    /// referenced parent law via <c>ArticleCode</c> exact match. Phase 2: attachment
    /// download + DMS upload, article insert, attachment link, and bidirectional
    /// <c>KMS.t_KBArticleLink</c> records to every resolved parent law. CategoryID is
    /// resolved from the ประเภทลูก digit ("1"=ประกาศ→55, "2"=คำสั่ง→54).
    /// </summary>
    public class KBImportProcessor : IKBImportProcessorService
    {
        private readonly IKBArticleRepository     _articleRepo;
        private readonly IKBVersionRepository     _versionRepo;
        private readonly IKBArticleLinkRepository _linkRepo;
        private readonly IKBArticleService        _articleService;
        private readonly IFileService             _fileService;
        private readonly LawFileDownloader        _downloader;

        // ── ค่าคงที่ตามข้อกำหนด ──────────────────────────────────────────────
        private const byte FixedArticleTypeLaw       = 5;   // Policy
        private const byte FixedArticleTypeOrder     = 1;
        private const byte FixedStatus               = 2;   // Review
        private const int  FixedConfidentialityLevel = 0;

        /// <summary>Folder ปลายทางของไฟล์แนบ (Order) — ตรงกับที่ KBEditorDialog ใช้ ("KMS Attachments")</summary>
        private const int AttachmentFolderId = 437;

        /// <summary>Law: ArticleCode 3 ตัวอักษรแรก → CategoryID (KMS.m_KBCategory)</summary>
        private static readonly Dictionary<string, int> PrefixCategoryMap =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "PVI", 46 },   // LawSectionCategoryId — ดู KBMainController.cs
        };

        /// <summary>Order: ColC (ประเภทลูก) → CategoryID + ป้ายแสดงผล</summary>
        private static readonly Dictionary<string, Tuple<int, string>> DocumentTypeMap =
            new Dictionary<string, Tuple<int, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "1", Tuple.Create(55, "ประกาศ") },
            { "2", Tuple.Create(54, "คำสั่ง") },
        };

        public KBImportProcessor(
            IKBArticleRepository     articleRepo,
            IKBVersionRepository     versionRepo,
            IKBArticleLinkRepository linkRepo,
            IKBArticleService        articleService,
            IFileService             fileService,
            LawFileDownloader        downloader)
        {
            if (articleRepo    == null) throw new ArgumentNullException("articleRepo");
            if (versionRepo    == null) throw new ArgumentNullException("versionRepo");
            if (linkRepo       == null) throw new ArgumentNullException("linkRepo");
            if (articleService == null) throw new ArgumentNullException("articleService");
            if (fileService    == null) throw new ArgumentNullException("fileService");
            if (downloader     == null) throw new ArgumentNullException("downloader");

            _articleRepo    = articleRepo;
            _versionRepo    = versionRepo;
            _linkRepo       = linkRepo;
            _articleService = articleService;
            _fileService    = fileService;
            _downloader     = downloader;
        }

        // ── Phase 1: Validation ──────────────────────────────────────────────

        /// <summary>
        /// Validates each row and sets <see cref="KBImportRow.Status"/> /
        /// <see cref="KBImportRow.StatusNote"/>. Read-only DB access only
        /// (<see cref="IKBArticleRepository.ExistsByCode"/>, <see cref="IKBArticleRepository.GetByCode"/>)
        /// — no writes.
        /// </summary>
        public void ValidateRows(IList<KBImportRow> rows)
        {
            if (rows == null) throw new ArgumentNullException("rows");

            var seenCodes   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parentCache = new Dictionary<string, KBArticle>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                // Rows already marked Error by the reader (unreadable row) are left as-is.
                if (row.Status == "Error")
                    continue;

                if (row.RowKind == "Order")
                    ValidateOrderRow(row, seenCodes, parentCache, _articleRepo);
                else
                    ValidateLawRow(row, seenCodes, _articleRepo);
            }
        }

        // ── Phase 2: Article insert ──────────────────────────────────────────

        /// <summary>
        /// Inserts a <c>KBArticle</c> for each row in <paramref name="rows"/> (expected to be
        /// pre-filtered to <c>Status == "Valid"</c> rows by the caller). Law rows also insert
        /// an initial <c>KBVersion</c>; Order rows also download/upload the attachment and
        /// link every resolved parent law. Each row is wrapped in its own <c>try/catch</c>;
        /// a failure on one row increments <paramref name="errorCount"/> and processing continues.
        /// </summary>
        /// <param name="onProgress">
        /// Optional callback invoked after each row with (rowsProcessed, totalRows).
        /// Use to update a progress bar in the UI.
        /// </param>
        public void InsertArticles(
            IList<KBImportRow> rows,
            UserContext user,
            bool skipRowOnDownloadFailure,
            out int successCount,
            out int errorCount,
            Action<int, int> onProgress)
        {
            if (rows == null) throw new ArgumentNullException("rows");

            successCount = 0;
            errorCount   = 0;
            int total    = rows.Count;

            string createdBy = user != null ? (user.AdIdentity ?? string.Empty) : string.Empty;

            for (int i = 0; i < total; i++)
            {
                var row = rows[i];
                try
                {
                    if (row.RowKind == "Order")
                        InsertOrderArticle(row, user, createdBy, skipRowOnDownloadFailure);
                    else
                        InsertLawArticle(row, createdBy);

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

        private void InsertLawArticle(KBImportRow row, string createdBy)
        {
            // Defensive re-check — the DB may have changed since ValidateRows ran.
            if (string.IsNullOrEmpty(row.ArticleCode))
                throw new InvalidOperationException("คอลัมน์ A (ArticleCode) ว่าง");
            if (_articleRepo.ExistsByCode(row.ArticleCode))
                throw new InvalidOperationException(string.Format(
                    "ArticleCode '{0}' มีอยู่ในระบบแล้ว", row.ArticleCode));

            var article = new KBArticle
            {
                ArticleCode           = row.ArticleCode,
                Title                 = row.Title,
                ContentHTML           = ConvertPlainTextToHtml(row.RawContent),
                Summary               = row.Summary,
                TitleCategory         = row.TitleCategory,
                CategoryID            = row.ResolvedCategoryID,
                ArticleType           = FixedArticleTypeLaw,
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

            int articleId = _articleRepo.Insert(article);
            row.ImportedArticleId = articleId;

            var version = new KBVersion
            {
                ArticleID       = articleId,
                VersionNumber   = 1,
                ContentSnapshot = article.ContentHTML,
                CreatedBy       = createdBy,
                CreatedDate     = DateTime.Now,
            };
            _versionRepo.Insert(version);

            row.Status     = "Valid";
            row.StatusNote = string.Empty;
        }

        private void InsertOrderArticle(KBImportRow row, UserContext user, string createdBy, bool skipRowOnDownloadFailure)
        {
            var notes = new List<string>();

            // ── ArticleCode ──────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(row.ArticleCode))
                throw new InvalidOperationException("คอลัมน์ B/C/D ไม่ครบ ไม่สามารถสร้าง ArticleCode ได้");
            if (_articleRepo.ExistsByCode(row.ArticleCode))
                throw new InvalidOperationException(string.Format(
                    "ArticleCode '{0}' มีอยู่ในระบบแล้ว", row.ArticleCode));

            Tuple<int, string> docType;
            if (!DocumentTypeMap.TryGetValue(row.OrderTypeDigit ?? string.Empty, out docType))
                throw new InvalidOperationException(string.Format(
                    "คอลัมน์ C (ประเภทลูก): ค่า '{0}' ต้องเป็น 1 (ประกาศ) หรือ 2 (คำสั่ง) เท่านั้น", row.OrderTypeDigit));

            // ── ไฟล์แนบ ──────────────────────────────────────────────────────
            int? fileId = null;
            var download = _downloader.Download(row.AttachmentUrl, row.AttachmentFileName);

            if (download.Success)
            {
                string uploadError;
                fileId = UploadToDms(download, user, out uploadError);
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
                    throw new InvalidOperationException(download.ErrorMessage);
                notes.Add(download.ErrorMessage);
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
                CategoryID            = docType.Item1,
                ArticleType           = FixedArticleTypeOrder,
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

            // ── link กฎหมายแม่ทุกตัวใน ParentCodes (ถ้าตัวไหน error ให้ข้ามและแจ้ง) ─
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

                    // ── ไป: ประกาศ/คำสั่งใหม่ → มาตรา ───────────────────────
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
                    }

                    // ── กลับ: มาตรา → ประกาศ/คำสั่งใหม่ (reference ไป-กลับ) ──
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
                    }
                }
                catch (Exception ex)
                {
                    notes.Add(string.Format("ข้าม link '{0}': {1}", code, ex.Message));
                }
            }

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

        // ── Static helpers (internal — directly testable) ────────────────────

        /// <summary>
        /// Converts plain text (Law column P / Order column I) into HTML before it is stored in
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

        /// <summary>
        /// Resolves a Law <c>CategoryID</c> from the first 3 characters of <paramref name="articleCode"/>
        /// via <see cref="PrefixCategoryMap"/>. Returns <c>false</c> (not found) when unmapped.
        /// </summary>
        internal static bool TryResolveLawCategoryId(string articleCode, out int categoryId)
        {
            categoryId = 0;
            if (string.IsNullOrEmpty(articleCode) || articleCode.Length < 3)
                return false;

            string prefix = articleCode.Substring(0, 3);
            return PrefixCategoryMap.TryGetValue(prefix, out categoryId);
        }

        // ── Private helpers — validation ──────────────────────────────────────

        /// <summary>
        /// Validates a single Law row and sets its <see cref="KBImportRow.Status"/> and
        /// <see cref="KBImportRow.StatusNote"/>. On success, also sets the computed
        /// <c>ArticleCode</c>/<c>Title</c>/<c>Tags</c>/<c>Summary</c>/<c>TitleCategory</c>/<c>ResolvedCategoryID</c>.
        /// </summary>
        private static void ValidateLawRow(KBImportRow row, HashSet<string> seenCodes, IKBArticleRepository articleRepo)
        {
            string raw = row.ColA;

            // ── Column A ("Code รวม") — blank ⇒ Skip ──────────────────────────
            if (string.IsNullOrEmpty(raw))
            {
                row.Status     = "Skip";
                row.StatusNote = "คอลัมน์ A (Code รวม) ว่าง — ข้ามแถวนี้";
                return;
            }

            // ── Derive the 22-digit ArticleCode from column A ─────────────────
            // A is up to 27 digits: first 22 = ArticleCode, last 5 = child-document
            // sequence ("00000" means this row is the law section itself; anything else
            // is a คำสั่ง/ประกาศ child row, imported instead from the Order-layout file).
            // A bare 22-digit value (no suffix) is treated as already being the law section.
            string code;
            if (raw.Length == 27)
            {
                string suffix = raw.Substring(22, 5);
                if (suffix != "00000")
                {
                    row.Status     = "Skip";
                    row.StatusNote = string.Format(
                        "คอลัมน์ A: '{0}' เป็นข้อมูลประกาศ/คำสั่ง (นำเข้าจากไฟล์ ประกาศ/คำสั่ง แยกต่างหาก) — ข้ามแถวนี้", raw);
                    return;
                }
                code = raw.Substring(0, 22);
            }
            else if (raw.Length == 22)
            {
                code = raw;
            }
            else
            {
                row.Status     = "Error";
                row.StatusNote = string.Format(
                    "คอลัมน์ A: ความยาวไม่ถูกต้อง (คาดหวัง 22 หรือ 27 หลัก แต่พบ {0} หลัก) — '{1}'", raw.Length, raw);
                return;
            }

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

            // ── Column C — required, numeric (source of Title/Tags) ───────────
            int sectionNumber = 0;
            if (string.IsNullOrEmpty(row.ColC))
                errors.Add("คอลัมน์ C (มาตรา): จำเป็นต้องระบุ");
            else if (!Regex.IsMatch(row.ColC, @"^\d+$") || !int.TryParse(row.ColC, out sectionNumber))
                errors.Add(string.Format("คอลัมน์ C (มาตรา): ค่า '{0}' ต้องเป็นตัวเลขเท่านั้น", row.ColC));

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

            // ── CategoryID — resolved from ArticleCode's 3-letter prefix ──────
            int categoryId;
            if (!TryResolveLawCategoryId(code, out categoryId))
                errors.Add(string.Format("ไม่พบ CategoryID สำหรับรหัสขึ้นต้น '{0}'",
                    code.Length >= 3 ? code.Substring(0, 3) : code));

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
            row.ResolvedCategoryID = categoryId;
            row.DocumentTypeLabel  = "มาตรา";
        }

        /// <summary>
        /// Validates a single Order row and sets its <see cref="KBImportRow.Status"/> and
        /// <see cref="KBImportRow.StatusNote"/>. On success, also sets the computed
        /// <c>ArticleCode</c>/<c>Title</c>/<c>Summary</c>/<c>TitleCategory</c>/<c>ResolvedCategoryID</c>/
        /// <c>ResolvedParentArticleIds</c>.
        /// </summary>
        private static void ValidateOrderRow(
            KBImportRow row, HashSet<string> seenCodes,
            Dictionary<string, KBArticle> parentCache, IKBArticleRepository articleRepo)
        {
            string finalCode = row.FinalCodeChoices.Count > 0 ? row.FinalCodeChoices[0] : null;
            row.ArticleCode  = finalCode;

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(finalCode))
            {
                errors.Add("คอลัมน์ B/C/D (Code กฎหมายแม่/ประเภทลูก/เอกสารลูก) ไม่ครบ ไม่สามารถสร้าง ArticleCode ได้");
            }
            else if (seenCodes.Contains(finalCode))
            {
                errors.Add(string.Format("ArticleCode '{0}' ซ้ำกับแถวอื่นในไฟล์", finalCode));
            }
            else if (articleRepo.ExistsByCode(finalCode))
            {
                errors.Add(string.Format("ArticleCode '{0}' มีอยู่ในระบบแล้ว", finalCode));
            }
            else
            {
                seenCodes.Add(finalCode);
            }

            Tuple<int, string> docType;
            if (!DocumentTypeMap.TryGetValue(row.OrderTypeDigit ?? string.Empty, out docType))
            {
                errors.Add(string.Format(
                    "คอลัมน์ C (ประเภทลูก): ค่า '{0}' ต้องเป็น 1 (ประกาศ) หรือ 2 (คำสั่ง) เท่านั้น", row.OrderTypeDigit));
            }
            else
            {
                row.ResolvedCategoryID = docType.Item1;
                row.DocumentTypeLabel  = docType.Item2;
            }

            if (string.IsNullOrWhiteSpace(row.RawContent))
                errors.Add("คอลัมน์ I (บัญญัติไว้ว่า) ห้ามว่าง");

            if (string.IsNullOrWhiteSpace(row.AttachmentFileName))
                errors.Add("คอลัมน์ K (ชื่อไฟล์) ห้ามว่าง");

            if (row.ParentCodes.Count == 0)
            {
                errors.Add("คอลัมน์ B (Code กฎหมายแม่) ห้ามว่าง");
            }
            else
            {
                var resolvedIds = new List<int>();
                var notFound    = new List<string>();

                foreach (var code in row.ParentCodes)
                {
                    KBArticle match;
                    if (!parentCache.TryGetValue(code, out match))
                    {
                        match = articleRepo.GetByCode(code);
                        parentCache[code] = match;
                    }

                    if (match == null) notFound.Add(code);
                    else               resolvedIds.Add(match.ArticleID);
                }

                row.ResolvedParentArticleIds = resolvedIds;
                if (notFound.Count > 0)
                    errors.Add("ไม่พบกฎหมายแม่ในระบบ: " + string.Join(", ", notFound));
            }

            if (!string.IsNullOrEmpty(row.RawPublishedDate) && !row.PublishedDateParsed.HasValue)
                errors.Add(string.Format("คอลัมน์ G (วันที่ประกาศ): ไม่สามารถแปลงวันที่ได้ — '{0}'", row.RawPublishedDate));

            if (!string.IsNullOrEmpty(row.RawEffectiveDate) && !row.EffectiveDateParsed.HasValue)
                errors.Add(string.Format("คอลัมน์ H (วันที่มีผลบังคับใช้): ไม่สามารถแปลงวันที่ได้ — '{0}'", row.RawEffectiveDate));

            if (errors.Count > 0)
            {
                row.Status     = "Error";
                row.StatusNote = string.Join("; ", errors);
                return;
            }

            row.Status        = "Valid";
            row.StatusNote    = null;
            row.Title          = row.RawTitle;
            row.Summary        = row.RawSummary;
            row.TitleCategory  = row.RawTitleCategory;
        }

        // ── Private helpers — misc ────────────────────────────────────────────

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
