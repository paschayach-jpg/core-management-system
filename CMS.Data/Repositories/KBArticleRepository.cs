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
                    SELECT a.*, c.CategoryName,
                           STUFF((
                               SELECT '|' + ISNULL(lu._desc, l.EntityID)
                               FROM   KMS.t_KBArticleLink l
                               LEFT JOIN (
                                   SELECT SubCommitteeCode AS code, SubCommitteeNameTH AS _desc FROM [EMS].[m_SubCommittee]
                                   UNION ALL
                                   SELECT DivisionCode,    DivisionName    FROM [EMS].[m_Division]
                                   UNION ALL
                                   SELECT DepartmentCode,  DepartmentName  FROM [EMS].[m_Department]
                               ) lu ON lu.code = l.EntityID
                               WHERE  l.ArticleID = a.ArticleID AND EntityType = 6
                               ORDER BY l.CreatedDate DESC
                               FOR XML PATH(''), TYPE
                           ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS LinkedEntities
                    FROM   KMS.t_KBArticle a
                    LEFT JOIN KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                    WHERE  (a.IsDelete = 0 OR a.IsDelete IS NULL)
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
                    SELECT a.*, c.CategoryName,
                           STUFF((
                               SELECT '|' + ISNULL(lu._desc, l.EntityID)
                               FROM   KMS.t_KBArticleLink l
                               LEFT JOIN (
                                   SELECT SubCommitteeCode AS code, SubCommitteeNameTH AS _desc FROM [EMS].[m_SubCommittee]
                                   UNION ALL
                                   SELECT DivisionCode,    DivisionName    FROM [EMS].[m_Division]
                                   UNION ALL
                                   SELECT DepartmentCode,  DepartmentName  FROM [EMS].[m_Department]
                               ) lu ON lu.code = l.EntityID
                               WHERE  l.ArticleID = a.ArticleID AND EntityType = 6
                               ORDER BY l.CreatedDate DESC
                               FOR XML PATH(''), TYPE
                           ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS LinkedEntities
                    FROM   KMS.t_KBArticle a
                    LEFT JOIN KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                    WHERE  a.CategoryID = @CategoryID
                      AND  (a.IsDelete = 0 OR a.IsDelete IS NULL)
                    ORDER BY a.ArticleCode ASC";
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

        public IEnumerable<KBArticle> Search(int? categoryId, byte? status, byte? articleType)
        {
            var results = new List<KBArticle>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT a.*, c.CategoryName,
                           STUFF((
                               SELECT '|' + ISNULL(lu._desc, l.EntityID)
                               FROM   KMS.t_KBArticleLink l
                               LEFT JOIN (
                                   SELECT SubCommitteeCode AS code, SubCommitteeNameTH AS _desc FROM [EMS].[m_SubCommittee]
                                   UNION ALL
                                   SELECT DivisionCode,    DivisionName    FROM [EMS].[m_Division]
                                   UNION ALL
                                   SELECT DepartmentCode,  DepartmentName  FROM [EMS].[m_Department]
                               ) lu ON lu.code = l.EntityID
                               WHERE  l.ArticleID = a.ArticleID AND l.EntityType = 6
                               ORDER BY l.CreatedDate DESC
                               FOR XML PATH(''), TYPE
                           ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS LinkedEntities
                    FROM   KMS.t_KBArticle a
                    LEFT JOIN KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                    WHERE  (@CategoryID  IS NULL OR a.CategoryID  = @CategoryID)
                      AND  (@Status      IS NULL OR a.Status      = @Status)
                      AND  (@ArticleType IS NULL OR a.ArticleType = @ArticleType)
                      AND  (a.IsDelete = 0 OR a.IsDelete IS NULL)
                    ORDER BY a.ArticleCode ASC";
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
            //if (string.IsNullOrWhiteSpace(keyword))
            return results;

            // Filter by keyword in C# - search in Title, Summary, Tags, and extracted HTML text
            //keyword = keyword.Trim();
            //return results.Where(a =>
            //    (a.Title != null && string.Equals(a.Title, keyword, StringComparison.OrdinalIgnoreCase)) ||
            //    (a.Summary != null && string.Equals(a.Summary,keyword, StringComparison.OrdinalIgnoreCase)) ||
            //    (a.Tags != null && string.Equals(a.Tags, keyword, StringComparison.OrdinalIgnoreCase)) ||
            //    (a.ContentHTML != null && string.Equals(StripHtmlTags(a.ContentHTML), keyword, StringComparison.OrdinalIgnoreCase))
            //).ToList();
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
                        (ArticleCode, Title, ContentHTML, Summary, TitleCategory, CategoryID, ArticleType,
                         Status, CurrentVersion, PublishedDate, EffectiveDate, ExpiryDate, ReviewDueDate, IsOfficialPolicy,
                         Tags, ConfidentialityLevel, CreatedBy, CreatedDate, ViewCount, LikeCount)
                    VALUES
                        (@ArticleCode, @Title, @ContentHTML, @Summary, @TitleCategory, @CategoryID, @ArticleType,
                         @Status, 1, @PublishedDate, @EffectiveDate, @ExpiryDate, @ReviewDueDate, @IsOfficialPolicy,
                         @Tags, @ConfidentialityLevel, @CreatedBy, GETDATE(), 0, 0);
                    SELECT SCOPE_IDENTITY();";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    AddParams(cmd, article);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int InsertWithVersion(KBArticle article, KBVersion version)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int articleId;
                        const string articleSql = @"
                            INSERT INTO KMS.t_KBArticle
                                (ArticleCode, Title, ContentHTML, Summary, TitleCategory, CategoryID, ArticleType,
                                 Status, CurrentVersion, PublishedDate, EffectiveDate, ExpiryDate, ReviewDueDate, IsOfficialPolicy,
                                 Tags, ConfidentialityLevel, CreatedBy, CreatedDate, ViewCount, LikeCount)
                            VALUES
                                (@ArticleCode, @Title, @ContentHTML, @Summary, @TitleCategory, @CategoryID, @ArticleType,
                                 @Status, 1, @PublishedDate, @EffectiveDate, @ExpiryDate, @ReviewDueDate, @IsOfficialPolicy,
                                 @Tags, @ConfidentialityLevel, @CreatedBy, GETDATE(), 0, 0);
                            SELECT SCOPE_IDENTITY();";
                        using (var cmd = new SqlCommand(articleSql, conn, tx))
                        {
                            AddParams(cmd, article);
                            articleId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        const string versionSql = @"
                            INSERT INTO KMS.t_KBVersion
                                (ArticleID, VersionNumber, ContentSnapshot, ChangeDescription, CreatedBy, CreatedDate)
                            VALUES
                                (@ArticleID, @VersionNumber, @ContentSnapshot, @ChangeDescription, @CreatedBy, GETDATE());";
                        using (var cmd = new SqlCommand(versionSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ArticleID",         articleId);
                            cmd.Parameters.AddWithValue("@VersionNumber",     version.VersionNumber);
                            cmd.Parameters.AddWithValue("@ContentSnapshot",   (object)version.ContentSnapshot   ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ChangeDescription", (object)version.ChangeDescription ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy",         version.CreatedBy ?? string.Empty);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return articleId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
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
                        Title                = @Title,
                        ContentHTML          = @ContentHTML,
                        Summary              = @Summary,
                        TitleCategory        = @TitleCategory,
                        CategoryID           = @CategoryID,
                        ArticleType          = @ArticleType,
                        Status               = @Status,
                        PublishedDate        = @PublishedDate,
                        EffectiveDate        = @EffectiveDate,
                        ExpiryDate           = @ExpiryDate,
                        ReviewDueDate        = @ReviewDueDate,
                        IsOfficialPolicy     = @IsOfficialPolicy,
                        Tags                 = @Tags,
                        ConfidentialityLevel = @ConfidentialityLevel
                    WHERE ArticleID = @ArticleID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", article.ArticleID);
                    AddParams(cmd, article);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// คอลัมน์ที่ Excel import เขียนทับได้ตอน update บทความเดิม → ค่าจาก <see cref="KBArticle"/> (whitelist กัน SQL injection ผ่านชื่อคอลัมน์).
        /// ArticleCode (คีย์ที่ใช้หาบทความ) และ CategoryID (หมวดที่ผู้ใช้อาจย้ายเอง) ตั้งใจไม่อยู่ในรายการ — import ห้ามแก้
        /// </summary>
        private static readonly Dictionary<string, Func<KBArticle, object>> ImportUpdatableColumns =
            new Dictionary<string, Func<KBArticle, object>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Title",         a => (object)a.Title         ?? DBNull.Value },
            { "TitleCategory", a => (object)a.TitleCategory ?? DBNull.Value },
            { "ContentHTML",   a => (object)a.ContentHTML   ?? DBNull.Value },
            { "Summary",       a => (object)a.Summary       ?? DBNull.Value },
            { "PublishedDate", a => (object)a.PublishedDate ?? DBNull.Value },
            { "EffectiveDate", a => (object)a.EffectiveDate ?? DBNull.Value },
            { "ArticleType",   a => a.ArticleType },
            { "Tags",          a => (object)a.Tags          ?? DBNull.Value },
        };

        public void UpdateFromImport(int articleId, KBArticle values, IEnumerable<string> columns, string updatedBy, KBVersion newVersion)
        {
            if (values  == null) throw new ArgumentNullException("values");
            if (columns == null) throw new ArgumentNullException("columns");

            var sets   = new List<string>();
            var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var bound  = new List<KeyValuePair<string, object>>();
            foreach (string column in columns)
            {
                Func<KBArticle, object> getValue;
                if (column == null || !ImportUpdatableColumns.TryGetValue(column, out getValue))
                    throw new ArgumentException("คอลัมน์ '" + column + "' ไม่อยู่ในรายการที่ import เขียนทับได้", "columns");
                if (!seen.Add(column)) continue;

                sets.Add(column + " = @" + column);
                bound.Add(new KeyValuePair<string, object>("@" + column, getValue(values)));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    int? nextVersion = null;

                    if (newVersion != null)
                    {
                        const string nextVersionSql = @"
                            SELECT ISNULL(MAX(VersionNumber), 0) + 1
                            FROM   KMS.t_KBVersion WITH (UPDLOCK, HOLDLOCK)
                            WHERE  ArticleID = @ArticleID";
                        using (var cmd = new SqlCommand(nextVersionSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ArticleID", articleId);
                            nextVersion = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        const string versionSql = @"
                            INSERT INTO KMS.t_KBVersion
                                (ArticleID, VersionNumber, ContentSnapshot, ChangeDescription, CreatedBy, CreatedDate)
                            VALUES
                                (@ArticleID, @VersionNumber, @ContentSnapshot, @ChangeDescription, @CreatedBy, GETDATE());";
                        using (var cmd = new SqlCommand(versionSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ArticleID",         articleId);
                            cmd.Parameters.AddWithValue("@VersionNumber",     nextVersion.Value);
                            cmd.Parameters.AddWithValue("@ContentSnapshot",   (object)newVersion.ContentSnapshot   ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ChangeDescription", (object)newVersion.ChangeDescription ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy",         newVersion.CreatedBy ?? string.Empty);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (nextVersion.HasValue) sets.Add("CurrentVersion = @CurrentVersion");
                    sets.Add("UpdatedBy = @UpdatedBy");
                    sets.Add("UpdatedDate = GETDATE()");

                    string sql = "UPDATE KMS.t_KBArticle SET " + string.Join(", ", sets) + " WHERE ArticleID = @ArticleID";
                    using (var cmd = new SqlCommand(sql, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@ArticleID", articleId);
                        foreach (var p in bound)
                            cmd.Parameters.AddWithValue(p.Key, p.Value);
                        if (nextVersion.HasValue)
                            cmd.Parameters.AddWithValue("@CurrentVersion", nextVersion.Value);
                        cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy ?? string.Empty);

                        if (cmd.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException("ไม่พบบทความ ArticleID " + articleId + " ที่จะอัปเดต");
                    }

                    tx.Commit();
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

        // ── Excel Import Template Config support ────────────────────────────────

        private static readonly HashSet<string> NonImportableColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ArticleID", "CreatedBy", "CreatedDate", "ReviewedBy", "ReviewedDate",
            "UpdatedBy", "UpdatedDate", "ViewCount", "LikeCount", "IsDelete", "CurrentVersion",
            // Excel import (KBImportWizardForm/KBImportProcessor) sets these itself — as a fixed
            // constant (Status, ConfidentialityLevel) or computed internally (Tags, IsOfficialPolicy)
            // — and never reads a Template ExcelColumnRef mapping for them. ReviewDueDate isn't set
            // by the import flow at all. None of the 5 should be offered as a mappable target column.
            "Status", "ConfidentialityLevel", "ReviewDueDate", "IsOfficialPolicy", "Tags",
        };

        public IList<string> GetImportableTargetColumns()
        {
            var results = new List<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT COLUMN_NAME
                    FROM   INFORMATION_SCHEMA.COLUMNS
                    WHERE  TABLE_SCHEMA = 'KMS' AND TABLE_NAME = 't_KBArticle'
                    ORDER BY ORDINAL_POSITION";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader.GetString(0);
                        if (!NonImportableColumns.Contains(columnName))
                            results.Add(columnName);
                    }
                }
            }
            return results;
        }

        public bool ExistsByCode(string articleCode)
        {
            if (string.IsNullOrWhiteSpace(articleCode)) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT TOP 1 1
                    FROM   KMS.t_KBArticle
                    WHERE  ArticleCode = @ArticleCode";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleCode", articleCode);
                    return cmd.ExecuteScalar() != null;
                }
            }
        }

        public KBArticle GetByCode(string articleCode)
        {
            if (string.IsNullOrWhiteSpace(articleCode)) return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT TOP 1 ArticleID, ArticleCode, TitleCategory
                    FROM   KMS.t_KBArticle
                    WHERE  ArticleCode = @ArticleCode
                      AND  (IsDelete = 0 OR IsDelete IS NULL)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleCode", articleCode);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return new KBArticle
                            {
                                ArticleID     = r.GetInt32(r.GetOrdinal("ArticleID")),
                                ArticleCode   = r["ArticleCode"]   as string,
                                TitleCategory = r["TitleCategory"] as string,
                            };
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<KBArticle> FindByNormalizedTitleCategory(string normalizedKey)
        {
            var results = new List<KBArticle>();
            if (string.IsNullOrWhiteSpace(normalizedKey)) return results;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // ตัด space / tab / CR / LF / non-breaking space ออกจาก TitleCategory ก่อนเทียบ
                const string sql = @"
                    SELECT ArticleID, ArticleCode, TitleCategory
                    FROM   KMS.t_KBArticle
                    WHERE  (IsDelete = 0 OR IsDelete IS NULL)
                      AND  REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                               ISNULL(TitleCategory, N''),
                               N' ', N''), NCHAR(9), N''), NCHAR(10), N''), NCHAR(13), N''), NCHAR(160), N'')
                           = @Key
                    ORDER BY ArticleCode ASC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Key", normalizedKey);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            results.Add(new KBArticle
                            {
                                ArticleID     = r.GetInt32(r.GetOrdinal("ArticleID")),
                                ArticleCode   = r["ArticleCode"]   as string,
                                TitleCategory = r["TitleCategory"] as string,
                            });
                        }
                    }
                }
            }
            return results;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static bool HasColumn(IDataReader r, string name)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static void AddParams(SqlCommand cmd, KBArticle a)
        {
            cmd.Parameters.AddWithValue("@ArticleCode",      a.ArticleCode      ?? string.Empty);
            cmd.Parameters.AddWithValue("@Title",            a.Title            ?? string.Empty);
            cmd.Parameters.AddWithValue("@ContentHTML",      (object)a.ContentHTML      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Summary",          (object)a.Summary          ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TitleCategory",    (object)a.TitleCategory    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryID",       a.CategoryID);
            cmd.Parameters.AddWithValue("@ArticleType",      a.ArticleType);
            cmd.Parameters.AddWithValue("@Status",           a.Status);
            cmd.Parameters.AddWithValue("@PublishedDate",    (object)a.PublishedDate    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EffectiveDate",    (object)a.EffectiveDate    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExpiryDate",       (object)a.ExpiryDate       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReviewDueDate",    (object)a.ReviewDueDate    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsOfficialPolicy", a.IsOfficialPolicy);
            cmd.Parameters.AddWithValue("@Tags",                 (object)a.Tags             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ConfidentialityLevel", a.ConfidentialityLevel);
            cmd.Parameters.AddWithValue("@CreatedBy",            a.CreatedBy        ?? string.Empty);
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
                TitleCategory    = r["TitleCategory"] as string,
                CategoryID       = r.GetInt32(r.GetOrdinal("CategoryID")),
                ArticleType      = r.IsDBNull(r.GetOrdinal("ArticleType"))  ? (byte)0 : r.GetByte(r.GetOrdinal("ArticleType")),
                Status           = r.IsDBNull(r.GetOrdinal("Status"))       ? (byte)1 : r.GetByte(r.GetOrdinal("Status")),
                CurrentVersion   = r.IsDBNull(r.GetOrdinal("CurrentVersion")) ? 1 : r.GetInt32(r.GetOrdinal("CurrentVersion")),
                PublishedDate    = r.IsDBNull(r.GetOrdinal("PublishedDate"))  ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("PublishedDate")),
                EffectiveDate    = r.IsDBNull(r.GetOrdinal("EffectiveDate"))  ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("EffectiveDate")),
                ExpiryDate       = r.IsDBNull(r.GetOrdinal("ExpiryDate"))     ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("ExpiryDate")),
                ReviewDueDate    = r.IsDBNull(r.GetOrdinal("ReviewDueDate"))  ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("ReviewDueDate")),
                ViewCount        = r.IsDBNull(r.GetOrdinal("ViewCount"))   ? 0 : r.GetInt32(r.GetOrdinal("ViewCount")),
                LikeCount        = r.IsDBNull(r.GetOrdinal("LikeCount"))   ? 0 : r.GetInt32(r.GetOrdinal("LikeCount")),
                IsOfficialPolicy = !r.IsDBNull(r.GetOrdinal("IsOfficialPolicy")) && r.GetBoolean(r.GetOrdinal("IsOfficialPolicy")),
                Tags                 = r["Tags"]          as string,
                ConfidentialityLevel = r.IsDBNull(r.GetOrdinal("ConfidentialityLevel")) ? 0 : r.GetByte(r.GetOrdinal("ConfidentialityLevel")),
                CreatedBy            = r["CreatedBy"]     as string,
                CreatedDate          = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate")),
                ReviewedBy           = r["ReviewedBy"]    as string,
                CategoryName         = r["CategoryName"]  as string,
                LinkedEntities       = HasColumn(r, "LinkedEntities") ? r["LinkedEntities"] as string : null,
                IsDelete             = HasColumn(r, "IsDelete") && !r.IsDBNull(r.GetOrdinal("IsDelete")) && r.GetBoolean(r.GetOrdinal("IsDelete")),
            };
        }

        private const int MoveChunkSize = 1000;   // SQL Server รับ parameter ได้ราว 2100 ต่อคำสั่ง

        private static IEnumerable<List<int>> Chunk(IEnumerable<int> ids)
        {
            var distinct = (ids ?? Enumerable.Empty<int>()).Distinct().ToList();
            for (int i = 0; i < distinct.Count; i += MoveChunkSize)
                yield return distinct.GetRange(i, Math.Min(MoveChunkSize, distinct.Count - i));
        }

        /// <summary>สร้าง "@p0,@p1,..." และใส่ parameter ให้ครบ</summary>
        private static string AddInParameters(SqlCommand cmd, List<int> ids)
        {
            var names = new string[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                names[i] = "@p" + i;
                cmd.Parameters.AddWithValue(names[i], ids[i]);
            }
            return string.Join(",", names);
        }

        public IList<KBArticle> GetMoveCandidates(IEnumerable<int> categoryIds)
        {
            return QueryMoveCandidates("a.CategoryID", categoryIds);
        }

        public IList<KBArticle> GetMoveCandidatesByIds(IEnumerable<int> articleIds)
        {
            return QueryMoveCandidates("a.ArticleID", articleIds);
        }

        private IList<KBArticle> QueryMoveCandidates(string filterColumn, IEnumerable<int> ids)
        {
            var results = new List<KBArticle>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                foreach (var chunk in Chunk(ids))
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        string inList = AddInParameters(cmd, chunk);
                        cmd.CommandText = @"
                            SELECT a.ArticleID, a.ArticleCode, a.Title, a.TitleCategory, a.CategoryID,
                                   a.ArticleType, a.Status, c.CategoryName
                            FROM   KMS.t_KBArticle a
                            LEFT JOIN KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                            WHERE  " + filterColumn + @" IN (" + inList + @")
                              AND  (a.IsDelete = 0 OR a.IsDelete IS NULL)";
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                results.Add(new KBArticle
                                {
                                    ArticleID     = r.GetInt32(0),
                                    ArticleCode   = r.IsDBNull(1) ? null : r.GetString(1),
                                    Title         = r.IsDBNull(2) ? null : r.GetString(2),
                                    TitleCategory = r.IsDBNull(3) ? null : r.GetString(3),
                                    CategoryID    = r.GetInt32(4),
                                    ArticleType   = r.IsDBNull(5) ? (byte)0 : r.GetByte(5),
                                    Status        = r.IsDBNull(6) ? (byte)1 : r.GetByte(6),
                                    CategoryName  = r.IsDBNull(7) ? null : r.GetString(7),
                                });
                            }
                        }
                    }
                }
            }
            return results.OrderBy(a => a.ArticleCode, StringComparer.Ordinal).ToList();
        }

        public IList<KeyValuePair<int, int>> MoveToCategory(IEnumerable<int> articleIds, int toCategoryId, string updatedBy)
        {
            var moved = new List<KeyValuePair<int, int>>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var chunk in Chunk(articleIds))
                        {
                            var toMove = new List<KeyValuePair<int, int>>();   // (ArticleID, CategoryID เดิม)
                            using (var cmd = new SqlCommand())
                            {
                                cmd.Connection  = conn;
                                cmd.Transaction = tx;
                                string inList = AddInParameters(cmd, chunk);
                                cmd.Parameters.AddWithValue("@To", toCategoryId);
                                cmd.CommandText = @"
                                    SELECT ArticleID, CategoryID
                                    FROM   KMS.t_KBArticle WITH (UPDLOCK)
                                    WHERE  ArticleID IN (" + inList + @")
                                      AND  CategoryID <> @To
                                      AND  (IsDelete = 0 OR IsDelete IS NULL)";
                                using (var r = cmd.ExecuteReader())
                                {
                                    while (r.Read())
                                        toMove.Add(new KeyValuePair<int, int>(r.GetInt32(0), r.GetInt32(1)));
                                }
                            }
                            if (toMove.Count == 0) continue;

                            using (var cmd = new SqlCommand())
                            {
                                cmd.Connection  = conn;
                                cmd.Transaction = tx;
                                string inList = AddInParameters(cmd, toMove.Select(m => m.Key).ToList());
                                cmd.Parameters.AddWithValue("@To", toCategoryId);
                                cmd.Parameters.AddWithValue("@UpdatedBy", (object)updatedBy ?? DBNull.Value);
                                cmd.CommandText = @"
                                    UPDATE KMS.t_KBArticle
                                    SET    CategoryID = @To, UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE()
                                    WHERE  ArticleID IN (" + inList + ")";
                                cmd.ExecuteNonQuery();
                            }
                            moved.AddRange(toMove);
                        }
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
            return moved;
        }

        public void SoftDelete(int articleId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "UPDATE KMS.t_KBArticle SET IsDelete = 1 WHERE ArticleID = @ArticleID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateExpiryDate(int articleId, DateTime? expiryDate)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "UPDATE KMS.t_KBArticle SET ExpiryDate = @ExpiryDate WHERE ArticleID = @ArticleID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID",  articleId);
                    cmd.Parameters.AddWithValue("@ExpiryDate", (object)expiryDate ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
