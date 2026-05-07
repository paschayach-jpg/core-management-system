using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CMS.Services
{
    /// <summary>
    /// Orchestrates the three-phase KB import pipeline:
    /// Phase 1 — row validation (no DB writes),
    /// Phase 2 — category upsert (<c>KMS.m_KBCategory</c>),
    /// Phase 3 — article + version insert (<c>KMS.t_KBArticle</c>, <c>KMS.t_KBVersion</c>).
    /// </summary>
    public class KBImportProcessor
    {
        private readonly IKBCategoryRepository _categoryRepo;
        private readonly IKBArticleRepository  _articleRepo;
        private readonly IKBVersionRepository  _versionRepo;

        public KBImportProcessor(
            IKBCategoryRepository categoryRepo,
            IKBArticleRepository  articleRepo,
            IKBVersionRepository  versionRepo)
        {
            if (categoryRepo == null) throw new ArgumentNullException("categoryRepo");
            if (articleRepo  == null) throw new ArgumentNullException("articleRepo");
            if (versionRepo  == null) throw new ArgumentNullException("versionRepo");

            _categoryRepo = categoryRepo;
            _articleRepo  = articleRepo;
            _versionRepo  = versionRepo;
        }

        // ── Phase 1: Validation ──────────────────────────────────────────────

        /// <summary>
        /// Validates each row and sets <see cref="KBImportRow.Status"/> /
        /// <see cref="KBImportRow.StatusNote"/>.
        /// No database access is performed.
        /// </summary>
        public void ValidateRows(IList<KBImportRow> rows)
        {
            if (rows == null) throw new ArgumentNullException("rows");

            foreach (var row in rows)
            {
                ValidateSingleRow(row);
            }
        }

        // ── Phase 2: Category upsert ─────────────────────────────────────────

        /// <summary>
        /// For each row whose <see cref="KBImportRow.Status"/> is not <c>"Error"</c>,
        /// resolves or creates the parent and sub-category records in
        /// <c>KMS.m_KBCategory</c> and sets <see cref="KBImportRow.ResolvedCategoryID"/>
        /// to the sub-category's <c>CategoryID</c>.
        /// An in-memory cache prevents redundant <c>GetByCode</c> calls within the batch.
        /// </summary>
        public void ResolveCategories(IList<KBImportRow> rows, string createdBy)
        {
            if (rows == null) throw new ArgumentNullException("rows");

            var cache = new Dictionary<string, int>(StringComparer.Ordinal);
            KBCategoryCodeName result;
            foreach (var row in rows)
            {
                if (row.Status == "Error")
                    continue;

                try
                {
                    bool success = Enum.TryParse(row.ColA, out result);
                    if (!success)
                    {
                        row.Status     = "Error";
                        row.StatusNote = TruncateTo200(string.Format(
                            "ไม่พบรหัสหมวดหมู่ '{0}' ในระบบ", row.ColA));
                        continue;
                    }

                    FieldInfo field = result.GetType().GetField(result.ToString());
                    DescriptionAttribute attr = (DescriptionAttribute)Attribute.GetCustomAttribute(
                        field, typeof(DescriptionAttribute));

                    int parentCategoryId = ResolveOrInsertCategory(
                        code:      row.ColA,
                        name:      attr.Description,
                        parentId:  null,
                        createdBy: createdBy,
                        cache:     cache);

                    int subCategoryId = ResolveOrInsertCategory(
                        code:      row.ColB,
                        name:      row.ColC,
                        parentId:  parentCategoryId,
                        createdBy: createdBy,
                        cache:     cache);

                    row.ResolvedCategoryID = subCategoryId;
                }
                catch (Exception ex)
                {
                    row.Status     = "Error";
                    row.StatusNote = TruncateTo200(ex.Message);
                }
            }
        }

        // ── Phase 3: Article + version insert ───────────────────────────────

        /// <summary>
        /// Inserts a <c>KBArticle</c> and a corresponding initial <c>KBVersion</c>
        /// for each row in <paramref name="rows"/>.
        /// Each row is wrapped in its own <c>try/catch</c>; a failure on one row
        /// increments <paramref name="errorCount"/> and processing continues.
        /// </summary>
        /// <param name="onProgress">
        /// Optional callback invoked after each row with (rowsProcessed, totalRows).
        /// Use to update a progress bar in the UI.
        /// </param>
        public void InsertArticles(
            IList<KBImportRow> rows,
            string createdBy,
            out int successCount,
            out int errorCount,
            Action<int, int> onProgress)
        {
            if (rows == null) throw new ArgumentNullException("rows");

            successCount = 0;
            errorCount   = 0;
            int total    = rows.Count;

            for (int i = 0; i < total; i++)
            {
                var row = rows[i];
                try
                {
                    var article = new KBArticle
                    {
                        ArticleCode      = row.ArticleCode,
                        Title            = row.ColE,
                        ContentHTML      = row.ColT,
                        Summary          = row.Summary,
                        CategoryID       = row.ResolvedCategoryID,
                        ArticleType      = 5,
                        Status           = 1,
                        CurrentVersion   = 1,
                        PublishedDate    = row.PublishedDateParsed,
                        ExpiryDate       = row.ExpiryDateParsed,
                        ReviewDueDate    = null,
                        ViewCount        = 0,
                        LikeCount        = 0,
                        IsOfficialPolicy = false,
                        Tags             = BuildTags(row.ColC, row.ColE),
                        CreatedBy        = createdBy,
                        CreatedDate      = DateTime.Now,
                    };

                    int articleId = _articleRepo.Insert(article);

                    var version = new KBVersion
                    {
                        ArticleID         = articleId,
                        VersionNumber     = 1,
                        ContentSnapshot   = row.ColT,
                        CreatedBy         = createdBy,
                        CreatedDate       = DateTime.Now,
                    };

                    _versionRepo.Insert(version);

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

        // ── Static helpers (internal — directly testable) ────────────────────

        /// <summary>
        /// Returns <c>true</c> when <paramref name="value"/> is a sentinel placeholder
        /// (<c>"-"</c> or <c>"ไม่มีค่า"</c>, exact case-sensitive match).
        /// </summary>
        internal static bool IsSentinel(string value)
        {
            return value == "-" || value == "ไม่มีค่า";
        }

        /// <summary>
        /// Builds the <c>ArticleCode</c> by joining non-null, non-empty, non-sentinel
        /// values from columns A, B, D, F, H, J, L, N, P with NO separator (concatenated
        /// directly without spaces).
        /// </summary>
        internal static string BuildArticleCode(KBImportRow row)
        {
            if (row == null) throw new ArgumentNullException("row");

            var segments = new[]
            {
                row.ColA, row.ColB, row.ColD, row.ColF,
                row.ColH, row.ColJ, row.ColL, row.ColN, row.ColP
            };

            return string.Concat(segments
                .Where(v => !string.IsNullOrEmpty(v) && !IsSentinel(v)));
        }

        /// <summary>
        /// Builds the <c>Summary</c> by joining non-null, non-empty, non-sentinel
        /// values from columns E, G, I, K, M, O, Q with a single space.
        /// Each segment is trimmed before evaluation; empty strings (after trim),
        /// <c>"-"</c>, and <c>"ไม่มีค่า"</c> are excluded from the concatenation.
        /// </summary>
        internal static string BuildSummary(KBImportRow row)
        {
            if (row == null) throw new ArgumentNullException("row");

            var segments = new[]
            {
                row.ColE, row.ColG, row.ColI, row.ColK,
                row.ColM, row.ColO, row.ColQ
            };

            return string.Join(" ", segments
                .Select(v => v == null ? string.Empty : v.Trim())
                .Where(v => v.Length > 0 && !IsSentinel(v)));
        }

        /// <summary>
        /// Builds the <c>Tags</c> field as <c>"{colC}, {colE}"</c>.
        /// </summary>
        internal static string BuildTags(string colC, string colE)
        {
            return string.Format("{0}, {1}", colC, colE);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Validates a single row and sets its <see cref="KBImportRow.Status"/> and
        /// <see cref="KBImportRow.StatusNote"/>.
        /// </summary>
        private static void ValidateSingleRow(KBImportRow row)
        {
            var errors = new List<string>();

            // ── Required columns (null/empty check) ──────────────────────────
            // Columns: A, B, C, D, E, F, H, J, L, N, P, R, T
            CheckRequired(row.ColA, "A", errors);
            CheckRequired(row.ColB, "B", errors);
            CheckRequired(row.ColC, "C", errors);
            CheckRequired(row.ColD, "D", errors);
            CheckRequired(row.ColE, "E", errors);
            CheckRequired(row.ColF, "F", errors);
            CheckRequired(row.ColH, "H", errors);
            CheckRequired(row.ColJ, "J", errors);
            CheckRequired(row.ColL, "L", errors);
            CheckRequired(row.ColN, "N", errors);
            CheckRequired(row.ColP, "P", errors);
            CheckRequired(row.ColT, "T", errors);

            // ── Column R (PublishedDate) — required + parse check ─────────────
            if (string.IsNullOrEmpty(row.ColR))
            {
                errors.Add("คอลัมน์ R: PublishedDate จำเป็นต้องระบุ");
            }
            else if (!row.PublishedDateParsed.HasValue)
            {
                errors.Add(string.Format(
                    "คอลัมน์ R: ไม่สามารถแปลงวันที่ได้ — '{0}'", row.ColR));
            }

            // ── Column S (ExpiryDate) — optional, but if present must parse ───
            if (!string.IsNullOrEmpty(row.ColS)
                && !IsSentinel(row.ColS)
                && !row.ExpiryDateParsed.HasValue)
            {
                errors.Add(string.Format(
                    "คอลัมน์ S: ไม่สามารถแปลงวันที่ได้ — '{0}'", row.ColS));
            }

            // ── Code columns — numeric-only (excluding sentinels) ─────────────
            // Columns: B, D, F, H, J, L, N, P
            // Column A is intentionally excluded — it holds a text-based category
            // code (e.g. "LW") and must NOT be validated as numeric.
            CheckNumericCode(row.ColB, "B", errors);
            CheckNumericCode(row.ColD, "D", errors);
            CheckNumericCode(row.ColF, "F", errors);
            CheckNumericCode(row.ColH, "H", errors);
            CheckNumericCode(row.ColJ, "J", errors);
            CheckNumericCode(row.ColL, "L", errors);
            CheckNumericCode(row.ColN, "N", errors);
            CheckNumericCode(row.ColP, "P", errors);

            if (errors.Count > 0)
            {
                row.Status     = "Error";
                row.StatusNote = string.Join("; ", errors);
            }
            else
            {
                row.Status      = "Valid";
                row.StatusNote  = null;
                row.ArticleCode = BuildArticleCode(row);
                row.Summary     = BuildSummary(row);
            }
        }

        /// <summary>
        /// Adds an error message to <paramref name="errors"/> when
        /// <paramref name="value"/> is null or empty.
        /// </summary>
        private static void CheckRequired(string value, string columnLetter, List<string> errors)
        {
            if (string.IsNullOrEmpty(value))
                errors.Add(string.Format("คอลัมน์ {0}: จำเป็นต้องระบุ", columnLetter));
        }

        /// <summary>
        /// Adds an error message to <paramref name="errors"/> when
        /// <paramref name="value"/> is non-empty, non-sentinel, and contains
        /// non-numeric characters.
        /// </summary>
        private static void CheckNumericCode(string value, string columnLetter, List<string> errors)
        {
            if (string.IsNullOrEmpty(value) || IsSentinel(value))
                return;

            if (!Regex.IsMatch(value, @"^\d+$"))
            {
                errors.Add(string.Format(
                    "คอลัมน์ {0}: ค่า '{1}' ต้องเป็นตัวเลขเท่านั้น", columnLetter, value));
            }
        }

        /// <summary>
        /// Resolves a category by <paramref name="code"/> from the in-memory
        /// <paramref name="cache"/> or the database, inserting a new record when
        /// none exists.  Returns the resolved <c>CategoryID</c>.
        /// </summary>
        private int ResolveOrInsertCategory(
            string code,
            string name,
            int?   parentId,
            string createdBy,
            Dictionary<string, int> cache)
        {
            int existingId;
            if (cache.TryGetValue(code, out existingId))
                return existingId;

            KBCategory existing = _categoryRepo.GetByCode(code);
            if (existing != null)
            {
                cache[code] = existing.CategoryID;
                return existing.CategoryID;
            }

            // Not found — insert a new category record.
            var newCategory = new KBCategory
            {
                CategoryCode     = code,
                CategoryName     = name,
                Icon             = "6",
                SortOrder        = 0,
                IsActive         = true,
                CreatedBy        = createdBy,
                CreatedDate      = DateTime.Now,
                ParentCategoryID = parentId,
            };

            int newId = _categoryRepo.Insert(newCategory);
            cache[code] = newId;
            return newId;
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
