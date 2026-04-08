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

        public IEnumerable<KBArticleLink> GetByArticle(int articleId)
        {
            var results = new List<KBArticleLink>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT * FROM KMS.t_KBArticleLink
                    WHERE  ArticleID = @ArticleID
                    ORDER  BY CreatedDate DESC";
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

        public int Insert(KBArticleLink link)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO KMS.t_KBArticleLink (ArticleID, EntityType, EntityID, CreatedBy, CreatedDate)
                    VALUES (@ArticleID, @EntityType, @EntityID, @CreatedBy, GETDATE());
                    SELECT SCOPE_IDENTITY();";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID",  link.ArticleID);
                    cmd.Parameters.AddWithValue("@EntityType", link.EntityType);
                    cmd.Parameters.AddWithValue("@EntityID",   link.EntityID);
                    cmd.Parameters.AddWithValue("@CreatedBy",  link.CreatedBy ?? string.Empty);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Delete(int linkId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "DELETE FROM KMS.t_KBArticleLink WHERE LinkID = @LinkID", conn))
                {
                    cmd.Parameters.AddWithValue("@LinkID", linkId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static KBArticleLink Map(IDataReader r)
        {
            return new KBArticleLink
            {
                LinkID      = r.GetInt32(r.GetOrdinal("LinkID")),
                ArticleID   = r.GetInt32(r.GetOrdinal("ArticleID")),
                EntityType  = r.IsDBNull(r.GetOrdinal("EntityType")) ? (byte)0 : r.GetByte(r.GetOrdinal("EntityType")),
                EntityID    = r.GetInt32(r.GetOrdinal("EntityID")),
                CreatedBy   = r["CreatedBy"] as string,
                CreatedDate = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate")),
            };
        }
    }
}
