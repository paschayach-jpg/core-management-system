using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class KBArticleLinkRepository : IKBArticleLinkRepository
    {
        private readonly string _connectionString;

        public KBArticleLinkRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<KBArticleLink> GetLinksForArticle(int articleId, byte? entityType = null)
        {
            var results = new List<KBArticleLink>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                string sql;
                if (!entityType.HasValue)
                {
                    sql = @"
                        SELECT LinkID, ArticleID, EntityType, EntityID, CreatedBy, CreatedDate
                        FROM   KMS.t_KBArticleLink
                        WHERE  ArticleID = @ArticleID
                        ORDER BY CreatedDate DESC";
                }
                else
                {
                    sql = @"
                        SELECT LinkID, ArticleID, EntityType, EntityID, CreatedBy, CreatedDate
                        FROM   KMS.t_KBArticleLink
                        WHERE  ArticleID = @ArticleID
                          AND  EntityType = @EntityType
                        ORDER BY CreatedDate DESC";
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    if (entityType.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@EntityType", entityType.Value);
                    }

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            results.Add(Map(r));
                        }
                    }
                }
            }
            return results;
        }

        public IEnumerable<KBArticleLink> GetAllByEntityType(byte entityType)
        {
            var results = new List<KBArticleLink>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = @"
                    SELECT LinkID, ArticleID, EntityType, EntityID, CreatedBy, CreatedDate
                    FROM   KMS.t_KBArticleLink
                    WHERE  EntityType = @EntityType";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EntityType", entityType);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            results.Add(Map(r));
                        }
                    }
                }
            }
            return results;
        }

        public int Insert(KBArticleLink link)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO KMS.t_KBArticleLink
                        (ArticleID, EntityType, EntityID, CreatedBy, CreatedDate)
                    VALUES
                        (@ArticleID, @EntityType, @EntityID, @CreatedBy, @CreatedDate);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", link.ArticleID);
                    cmd.Parameters.AddWithValue("@EntityType", link.EntityType);
                    cmd.Parameters.AddWithValue("@EntityID", link.EntityID);
                    cmd.Parameters.AddWithValue("@CreatedBy", link.CreatedBy ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CreatedDate", link.CreatedDate);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public bool Exists(int articleId, byte entityType, string entityId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = @"
                    SELECT TOP 1 1
                    FROM   KMS.t_KBArticleLink
                    WHERE  ArticleID = @ArticleID
                      AND  EntityType = @EntityType
                      AND  EntityID = @EntityID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    cmd.Parameters.AddWithValue("@EntityType", entityType);
                    cmd.Parameters.AddWithValue("@EntityID", entityId ?? string.Empty);
                    return cmd.ExecuteScalar() != null;
                }
            }
        }

        public void Delete(int linkId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = "DELETE FROM KMS.t_KBArticleLink WHERE LinkID = @LinkID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LinkID", linkId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteAllForArticle(int articleId, byte? entityType = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string sql;
                if (!entityType.HasValue)
                {
                    sql = "DELETE FROM KMS.t_KBArticleLink WHERE ArticleID = @ArticleID";
                }
                else
                {
                    sql = "DELETE FROM KMS.t_KBArticleLink WHERE ArticleID = @ArticleID AND EntityType = @EntityType";
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    if (entityType.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@EntityType", entityType.Value);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static KBArticleLink Map(IDataReader r)
        {
            return new KBArticleLink
            {
                LinkID      = r.GetInt32(r.GetOrdinal("LinkID")),
                ArticleID   = r.GetInt32(r.GetOrdinal("ArticleID")),
                EntityType  = r.GetByte(r.GetOrdinal("EntityType")),
                EntityID    = r.GetString(r.GetOrdinal("EntityID")),
                CreatedBy   = r["CreatedBy"] as string,
                CreatedDate = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate"))
            };
        }
    }
}
