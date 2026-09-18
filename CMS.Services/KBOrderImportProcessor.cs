using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// ตรวจสอบและนำเข้า ประกาศ/คำสั่ง จาก Excel (layout "law-matrix-formulas": column A–L)
    /// ค่าคงที่ตามข้อกำหนด: Status=2, CategoryID=54, ArticleType=1, ConfidentialityLevel=0, CurrentVersion=1
    /// </summary>
    public class KBOrderImportProcessor : IKBOrderImportProcessorService
    {
        // ── ค่าคงที่ตามข้อกำหนด ──────────────────────────────────────────────
        private const byte FixedStatus               = 2;
        private const int  FixedCategoryId           = 54;
        private const byte FixedArticleType          = 1;
        private const int  FixedConfidentialityLevel = 0;

        /// <summary>Folder ปลายทางของไฟล์แนบ — ตรงกับที่ KBEditorDialog ใช้ ("KMS Attachments")</summary>
        private const int AttachmentFolderId = 437;

        private readonly IKBArticleRepository     _articleRepo;
        private readonly IKBArticleLinkRepository _linkRepo;
        private readonly IKBArticleService        _articleService;
        private readonly IFileService             _fileService;
        private readonly LawFileDownloader        _downloader;

        public KBOrderImportProcessor(
            IKBArticleRepository     articleRepo,
            IKBArticleLinkRepository linkRepo,
            IKBArticleService        articleService,
            IFileService             fileService,
            LawFileDownloader        downloader)
        {
            if (articleRepo    == null) throw new ArgumentNullException("articleRepo");
            if (linkRepo       == null) throw new ArgumentNullException("linkRepo");
            if (articleService == null) throw new ArgumentNullException("articleService");
            if (fileService    == null) throw new ArgumentNullException("fileService");
            if (downloader     == null) throw new ArgumentNullException("downloader");

            _articleRepo    = articleRepo;
            _linkRepo       = linkRepo;
            _articleService = articleService;
            _fileService    = fileService;
            _downloader     = downloader;
        }

        // ── 1) ค้นหากฎหมายแม่ ────────────────────────────────────────────────

        public IList<KBSectionGroup> ResolveSectionGroups(IList<KBOrderImportRow> rows)
        {
            var groups = new List<KBSectionGroup>();
            if (rows == null) return groups;

            var byKey = new Dictionary<string, KBSectionGroup>();

            foreach (var row in rows)
            {
                for (int i = 0; i < row.ParentCodes.Count; i++)
                {
                    string code = row.ParentCodes[i];
                    if (byKey.ContainsKey(code)) continue;

                    var group = new KBSectionGroup
                    {
                        SectionKey = code,
                        SectionRaw = code,
                    };

                    var match = _articleRepo.GetByCode(code);
                    if (match != null)
                    {
                        group.Candidates.Add(new KBSectionCandidate
                        {
                            ArticleID     = match.ArticleID,
                            ArticleCode   = match.ArticleCode,
                            TitleCategory = match.TitleCategory,
                            YearDisplay   = ExtractYear(match.ArticleCode),
                        });
                        // ArticleCode ตรงตัวเป๊ะ มีได้แค่ record เดียว — เลือกให้เลย ไม่ต้องถาม
                        group.SelectedArticleId = match.ArticleID;
                    }

                    byKey[code] = group;
                    groups.Add(group);
                }
            }

            return groups;
        }

        /// <summary>ดึงปี พ.ศ. จาก ArticleCode — index 7–9 แล้วเติม '2' ข้างหน้า เช่น "535" → "2535"</summary>
        public static string ExtractYear(string articleCode)
        {
            if (string.IsNullOrEmpty(articleCode) || articleCode.Length < 10)
                return "-";

            string digits = articleCode.Substring(7, 3);
            foreach (char ch in digits)
                if (!char.IsDigit(ch)) return "-";

            return "2" + digits;
        }

        // ── 2) ตรวจความถูกต้อง ───────────────────────────────────────────────

        public void ValidateRows(IList<KBOrderImportRow> rows, IList<KBSectionGroup> sectionGroups)
        {
            if (rows == null) return;

            var groupByKey = new Dictionary<string, KBSectionGroup>();
            if (sectionGroups != null)
                foreach (var g in sectionGroups) groupByKey[g.SectionKey] = g;

            // ArticleCode (column D ตัวแรก) ที่ซ้ำกันเองภายในไฟล์ — รู้ค่าได้ทันทีเพราะ Excel คำนวณมาให้แล้ว
            var duplicateCodes = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.FinalArticleCode))
                .GroupBy(r => r.FinalArticleCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var row in rows)
            {
                // แถวที่ reader ตีว่า error ไปแล้ว (เช่นแปลงวันที่ไม่ได้) ไม่ต้องตรวจซ้ำ
                if (row.Status == "Error") continue;

                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(row.FinalArticleCode))
                    errors.Add("column D (Code) ห้ามว่าง");
                else if (duplicateCodes.Contains(row.FinalArticleCode.Trim(), StringComparer.OrdinalIgnoreCase))
                    errors.Add(string.Format("ArticleCode '{0}' ซ้ำกับแถวอื่นในไฟล์", row.FinalArticleCode.Trim()));
                else if (_articleRepo.ExistsByCode(row.FinalArticleCode.Trim()))
                    errors.Add(string.Format("ArticleCode '{0}' มีอยู่ในระบบแล้ว", row.FinalArticleCode.Trim()));

                if (string.IsNullOrWhiteSpace(row.ContentHTML))
                    errors.Add("column I (บัญญัติไว้ว่า) ห้ามว่าง");

                if (string.IsNullOrWhiteSpace(row.AttachmentFileName))
                    errors.Add("column K (ชื่อไฟล์) ห้ามว่าง");

                if (row.ParentCodes.Count == 0)
                    errors.Add("column A (Code กฎหมายแม่) ห้ามว่าง");
                else
                {
                    // ต้องหาเจอครบทุกกฎหมายแม่ ไม่ครบ = error ทั้งแถว
                    var notFound = new List<string>();
                    for (int i = 0; i < row.ParentCodes.Count; i++)
                    {
                        KBSectionGroup g;
                        if (!groupByKey.TryGetValue(row.ParentCodes[i], out g) || g.Candidates.Count == 0)
                            notFound.Add(row.ParentCodes[i]);
                    }

                    if (notFound.Count > 0)
                        errors.Add("ไม่พบกฎหมายแม่ในระบบ: " + string.Join(", ", notFound.ToArray()));
                }

                if (errors.Count > 0)
                {
                    row.Status     = "Error";
                    row.StatusNote = string.Join("; ", errors.ToArray());
                }
                else
                {
                    row.Status     = "Valid";
                    row.StatusNote = string.Empty;
                }
            }
        }

        // ── 3) นำเข้า ────────────────────────────────────────────────────────

        public void InsertArticles(
            IList<KBOrderImportRow> rows,
            IList<KBSectionGroup>   sectionGroups,
            bool                    skipRowOnDownloadFailure,
            UserContext             user,
            out int                 successCount,
            out int                 errorCount,
            Action<int, int>        onProgress)
        {
            successCount = 0;
            errorCount   = 0;

            if (rows == null || rows.Count == 0) return;

            var selectedByKey = new Dictionary<string, int>();
            if (sectionGroups != null)
                foreach (var g in sectionGroups) selectedByKey[g.SectionKey] = g.SelectedArticleId;

            string createdBy = user != null
                ? (user.AdIdentity ?? string.Empty)
                : string.Empty;

            int total = rows.Count;
            int done  = 0;

            foreach (var row in rows)
            {
                try
                {
                    var notes = new List<string>();

                    // ── ไฟล์แนบ ──────────────────────────────────────────────
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

                    // ── ArticleCode = column D ตัวแรก (Excel คำนวณไว้แล้ว) ─────
                    if (string.IsNullOrWhiteSpace(row.FinalArticleCode))
                        throw new InvalidOperationException("column D (Code) ห้ามว่าง");

                    if (_articleRepo.ExistsByCode(row.FinalArticleCode))
                        throw new InvalidOperationException(string.Format(
                            "ArticleCode '{0}' มีอยู่ในระบบแล้ว", row.FinalArticleCode));

                    // ── บทความ ───────────────────────────────────────────────
                    var article = new KBArticle
                    {
                        ArticleCode          = row.FinalArticleCode,
                        Title                = row.Title,
                        TitleCategory        = row.TitleCategory,
                        ContentHTML          = ConvertPlainTextToHtml(row.ContentHTML),
                        Summary              = row.Summary,
                        PublishedDate        = row.PublishedDate,
                        EffectiveDate        = row.EffectiveDate,
                        CategoryID           = FixedCategoryId,
                        ArticleType          = FixedArticleType,
                        Status               = FixedStatus,
                        ConfidentialityLevel = FixedConfidentialityLevel,
                        CreatedBy            = createdBy,
                    };

                    int articleId = _articleRepo.Insert(article);
                    row.ImportedArticleId = articleId;

                    // ── ผูกไฟล์แนบเข้ากับบทความ ───────────────────────────────
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

                    // ── link กฎหมายแม่ทุกตัวใน column A (ถ้าตัวไหน error ให้ข้ามและแจ้ง) ─
                    for (int i = 0; i < row.ParentCodes.Count; i++)
                    {
                        string code = row.ParentCodes[i];

                        int sectionArticleId;
                        if (!selectedByKey.TryGetValue(code, out sectionArticleId) || sectionArticleId <= 0)
                        {
                            notes.Add(string.Format("ข้าม link '{0}' (ไม่พบกฎหมายแม่ในระบบ)", code));
                            continue;
                        }

                        try
                        {
                            string entityId = sectionArticleId.ToString();
                            string articleIdStr = articleId.ToString();

                            // ── ไป: คำสั่งใหม่ → มาตรา ─────────────────────────
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

                            // ── กลับ: มาตรา → คำสั่งใหม่ (reference ไป-กลับ) ──────
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
                        row.StatusNote = string.Join("; ", notes.ToArray());
                    }
                    else
                    {
                        row.Status     = "Valid";
                        row.StatusNote = string.Empty;
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    row.Status     = "Error";
                    row.StatusNote = ex.Message;
                    errorCount++;
                }

                done++;
                if (onProgress != null) onProgress(done, total);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// แปลงข้อความล้วนจาก column I (บัญญัติไว้ว่า) ให้เป็น HTML ก่อนเก็บลง t_KBArticle.ContentHTML
        /// จำเป็นเพราะทั้ง ArticleViewerForm (webContent.DocumentText) และ KBEditorDialog
        /// (webEditor.Document.Body.InnerHtml) เอาค่านี้ไป set เข้า HTML DOM ตรงๆ โดยไม่แปลงเพิ่ม —
        /// ถ้าเก็บ \n/\t แบบดิบไว้ เบราว์เซอร์จะยุบช่องว่างตาม HTML spec ทำให้บรรทัดที่ผู้ใช้ขึ้นบรรทัดใหม่
        /// ไว้ใน Excel (Alt+Enter) หายไปตอนแสดงผล จึง HTML-encode ป้องกัน markup พังก่อน แล้วค่อยแปลง
        /// \n เป็น &lt;br&gt; และ \t เป็น 4x&amp;nbsp; ให้ตรงกับ convention ของ rich-text editor เดิม
        /// (ดู NormalizeLegacyEditorHtml ใน KBEditorDialog.cs ที่ใช้ &amp;nbsp; ชุดเดียวกันแทน tab)
        /// </summary>
        private static string ConvertPlainTextToHtml(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            string normalized = raw.Replace("\r\n", "\n").Replace("\r", "\n");
            string encoded    = System.Net.WebUtility.HtmlEncode(normalized);
            encoded = encoded.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
            encoded = encoded.Replace("\n", "<br>");
            return encoded;
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
    }
}
