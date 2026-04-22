using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class KBArticleRepository : IKBArticleRepository
    {
        private readonly string _connectionString;

        public KBArticleRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<KBArticle> GetAll()
        {
            var results = new List<KBArticle>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT a.*, c.CategoryName
                    FROM   KMS.t_KBArticle a
                    LEFT JOIN KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                    ORDER BY a.CreatedDate DESC";
                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) results.Add(Map(r));
                }
            }
            return results;
        }

        public IEnumerable<KBArticle> GetByCategory(int categoryId)
        {
            var results = new List<KBArticle>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT a.*, c.CategoryName
                    FROM   KMS.t_KBArticle a
                    LEFT JOIN KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                    WHERE  a.CategoryID = @CategoryID
                    ORDER BY a.CreatedDate DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read()) results.Add(Map(r));
                    }
                }
            }
            return results;
        }

        public IEnumerable<KBArticle> Search(string keyword, int? categoryId, byte? status, byte? articleType)
        {
            var results = new List<KBArticle>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT a.*, c.CategoryName
                    FROM   KMS.t_KBArticle a
                    LEFT JOIN KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                    WHERE  (@CategoryID  IS NULL OR a.CategoryID  = @CategoryID)
                      AND  (@Status      IS NULL OR a.Status      = @Status)
                      AND  (@ArticleType IS NULL OR a.ArticleType = @ArticleType)
                    ORDER BY a.CreatedDate DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID",  (object)categoryId  ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status",      (object)status      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ArticleType", (object)articleType ?? DBNull.Value);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read()) results.Add(Map(r));
                    }
                }
            }

            // If no keyword, return all results
            if (string.IsNullOrWhiteSpace(keyword))
                return results;

            // Filter by keyword in C# - search in Title, Summary, Tags, and extracted HTML text
            keyword = keyword.Trim();
            return results.Where(a =>
                (a.Title != null && a.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.Summary != null && a.Summary.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (a.Tags != null && a.Tags.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
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
            return html.Trim();
        }

        public KBArticle GetById(int articleId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT a.*, c.CategoryName
                    FROM   KMS.t_KBArticle a
                    LEFT JOIN KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                    WHERE  a.ArticleID = @ArticleID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read()) return Map(r);
                    }
                }
            }
            return null;
        }

        public int Insert(KBArticle article)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO KMS.t_KBArticle
                        (ArticleCode, Title, ContentHTML, Summary, CategoryID, ArticleType,
                         Status, CurrentVersion, ExpiryDate, ReviewDueDate, IsOfficialPolicy,
                         Tags, CreatedBy, CreatedDate, ViewCount, LikeCount)
                    VALUES
                        (@ArticleCode, @Title, @ContentHTML, @Summary, @CategoryID, @ArticleType,
                         @Status, 1, @ExpiryDate, @ReviewDueDate, @IsOfficialPolicy,
                         @Tags, @CreatedBy, GETDATE(), 0, 0);
                    SELECT SCOPE_IDENTITY();";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    AddParams(cmd, article);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(KBArticle article)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    UPDATE KMS.t_KBArticle SET
                        Title            = @Title,
                        ContentHTML      = @ContentHTML,
                        Summary          = @Summary,
                        CategoryID       = @CategoryID,
                        ArticleType      = @ArticleType,
                        Status           = @Status,
                        ExpiryDate       = @ExpiryDate,
                        ReviewDueDate    = @ReviewDueDate,
                        IsOfficialPolicy = @IsOfficialPolicy,
                        Tags             = @Tags
                    WHERE ArticleID = @ArticleID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", article.ArticleID);
                    AddParams(cmd, article);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateStatus(int articleId, byte status, string reviewedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    UPDATE KMS.t_KBArticle SET
                        Status       = @Status,
                        ReviewedBy   = @ReviewedBy,
                        PublishedDate = CASE WHEN @Status = 3 THEN GETDATE() ELSE PublishedDate END
                    WHERE ArticleID = @ArticleID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    cmd.Parameters.AddWithValue("@Status",    status);
                    cmd.Parameters.AddWithValue("@ReviewedBy", (object)reviewedBy ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void IncrementViewCount(int articleId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE KMS.t_KBArticle SET ViewCount = ViewCount + 1 WHERE ArticleID = @ArticleID", conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void IncrementLikeCount(int articleId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE KMS.t_KBArticle SET LikeCount = LikeCount + 1 WHERE ArticleID = @ArticleID", conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int GetNextCodeSequence(int year)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    DECLARE @prefix VARCHAR(12) = 'KB-' + CAST(@Year AS VARCHAR(4)) + '-'
                    SELECT ISNULL(
                        MAX(TRY_CAST(SUBSTRING(ArticleCode, LEN(@prefix) + 1, 3) AS INT)),
                        0) + 1
                    FROM KMS.t_KBArticle
                    WHERE ArticleCode LIKE @prefix + '%'";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Year", year);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void AddParams(SqlCommand cmd, KBArticle a)
        {
            cmd.Parameters.AddWithValue("@ArticleCode",      a.ArticleCode      ?? string.Empty);
            cmd.Parameters.AddWithValue("@Title",            a.Title            ?? string.Empty);
            cmd.Parameters.AddWithValue("@ContentHTML",      (object)a.ContentHTML      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Summary",          (object)a.Summary          ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryID",       a.CategoryID);
            cmd.Parameters.AddWithValue("@ArticleType",      a.ArticleType);
            cmd.Parameters.AddWithValue("@Status",           a.Status);
            cmd.Parameters.AddWithValue("@ExpiryDate",       (object)a.ExpiryDate       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReviewDueDate",    (object)a.ReviewDueDate    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsOfficialPolicy", a.IsOfficialPolicy);
            cmd.Parameters.AddWithValue("@Tags",             (object)a.Tags             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy",        a.CreatedBy        ?? string.Empty);
        }

        private static KBArticle Map(IDataReader r)
        {
            return new KBArticle
            {
                ArticleID        = r.GetInt32(r.GetOrdinal("ArticleID")),
                ArticleCode      = r["ArticleCode"]   as string,
                Title            = r["Title"]         as string,
                ContentHTML      = r["ContentHTML"]   as string,
                Summary          = r["Summary"]       as string,
                CategoryID       = r.GetInt32(r.GetOrdinal("CategoryID")),
                ArticleType      = r.IsDBNull(r.GetOrdinal("ArticleType"))  ? (byte)0 : r.GetByte(r.GetOrdinal("ArticleType")),
                Status           = r.IsDBNull(r.GetOrdinal("Status"))       ? (byte)1 : r.GetByte(r.GetOrdinal("Status")),
                CurrentVersion   = r.IsDBNull(r.GetOrdinal("CurrentVersion")) ? 1 : r.GetInt32(r.GetOrdinal("CurrentVersion")),
                PublishedDate    = r.IsDBNull(r.GetOrdinal("PublishedDate"))  ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("PublishedDate")),
                ExpiryDate       = r.IsDBNull(r.GetOrdinal("ExpiryDate"))     ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("ExpiryDate")),
                ReviewDueDate    = r.IsDBNull(r.GetOrdinal("ReviewDueDate"))  ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("ReviewDueDate")),
                ViewCount        = r.IsDBNull(r.GetOrdinal("ViewCount"))   ? 0 : r.GetInt32(r.GetOrdinal("ViewCount")),
                LikeCount        = r.IsDBNull(r.GetOrdinal("LikeCount"))   ? 0 : r.GetInt32(r.GetOrdinal("LikeCount")),
                IsOfficialPolicy = !r.IsDBNull(r.GetOrdinal("IsOfficialPolicy")) && r.GetBoolean(r.GetOrdinal("IsOfficialPolicy")),
                Tags             = r["Tags"]          as string,
                CreatedBy        = r["CreatedBy"]     as string,
                CreatedDate      = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate")),
                ReviewedBy       = r["ReviewedBy"]    as string,
                CategoryName     = r["CategoryName"]  as string,
            };
        }
    }
}
