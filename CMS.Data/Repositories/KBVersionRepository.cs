using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class KBVersionRepository : IKBVersionRepository
    {
        private readonly string _connectionString;

        public KBVersionRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<KBVersion> GetByArticle(int articleId)
        {
            var results = new List<KBVersion>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT * FROM KMS.t_KBVersion
                    WHERE  ArticleID = @ArticleID
                    ORDER  BY VersionNumber DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read()) results.Add(Map(r));
                    }
                }
            }
            return results;
        }

        public KBVersion GetById(int versionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "SELECT * FROM KMS.t_KBVersion WHERE VersionID = @VersionID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@VersionID", versionId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read()) return Map(r);
                    }
                }
            }
            return null;
        }

        public KBVersion GetLatestByArticle(int articleId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT TOP 1 * FROM KMS.t_KBVersion
                    WHERE  ArticleID = @ArticleID
                    ORDER  BY VersionNumber DESC";
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

        public int Insert(KBVersion version)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO KMS.t_KBVersion
                        (ArticleID, VersionNumber, ContentSnapshot, ChangeDescription, CreatedBy, CreatedDate)
                    VALUES
                        (@ArticleID, @VersionNumber, @ContentSnapshot, @ChangeDescription, @CreatedBy, GETDATE());
                    SELECT SCOPE_IDENTITY();";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID",         version.ArticleID);
                    cmd.Parameters.AddWithValue("@VersionNumber",     version.VersionNumber);
                    cmd.Parameters.AddWithValue("@ContentSnapshot",   (object)version.ContentSnapshot   ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ChangeDescription", (object)version.ChangeDescription ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy",         version.CreatedBy ?? string.Empty);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int GetNextVersionNumber(int articleId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT ISNULL(MAX(VersionNumber), 0) + 1
                    FROM   KMS.t_KBVersion
                    WHERE  ArticleID = @ArticleID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private static KBVersion Map(IDataReader r)
        {
            return new KBVersion
            {
                VersionID         = r.GetInt32(r.GetOrdinal("VersionID")),
                ArticleID         = r.GetInt32(r.GetOrdinal("ArticleID")),
                VersionNumber     = r.GetInt32(r.GetOrdinal("VersionNumber")),
                ContentSnapshot   = r["ContentSnapshot"]   as string,
                ChangeDescription = r["ChangeDescription"] as string,
                CreatedBy         = r["CreatedBy"]         as string,
                CreatedDate       = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate")),
            };
        }
    }
}
