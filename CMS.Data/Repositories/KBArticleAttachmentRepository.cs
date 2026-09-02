using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class KBArticleAttachmentRepository : IKBArticleAttachmentRepository
    {
        private readonly string _connectionString;

        public KBArticleAttachmentRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<KBArticleAttachment> GetAttachmentsForArticle(int articleId)
        {
            var results = new List<KBArticleAttachment>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT AttachmentID, ArticleID, FileID, CreatedBy, CreatedDate
                    FROM   KMS.t_KBArticleAttachment
                    WHERE  ArticleID = @ArticleID
                    ORDER BY CreatedDate DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);

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

        public int Insert(KBArticleAttachment attachment)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO KMS.t_KBArticleAttachment
                        (ArticleID, FileID, CreatedBy, CreatedDate)
                    VALUES
                        (@ArticleID, @FileID, @CreatedBy, @CreatedDate);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", attachment.ArticleID);
                    cmd.Parameters.AddWithValue("@FileID", attachment.FileID);
                    cmd.Parameters.AddWithValue("@CreatedBy", attachment.CreatedBy ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CreatedDate", attachment.CreatedDate);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Delete(int attachmentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "DELETE FROM KMS.t_KBArticleAttachment WHERE AttachmentID = @AttachmentID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AttachmentID", attachmentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteAllForArticle(int articleId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "DELETE FROM KMS.t_KBArticleAttachment WHERE ArticleID = @ArticleID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteByArticleAndFile(int articleId, int fileId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "DELETE FROM KMS.t_KBArticleAttachment WHERE ArticleID = @ArticleID AND FileID = @FileID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", articleId);
                    cmd.Parameters.AddWithValue("@FileID", fileId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static KBArticleAttachment Map(IDataReader r)
        {
            return new KBArticleAttachment
            {
                AttachmentID = r.GetInt32(r.GetOrdinal("AttachmentID")),
                ArticleID    = r.GetInt32(r.GetOrdinal("ArticleID")),
                FileID       = r.GetInt32(r.GetOrdinal("FileID")),
                CreatedBy    = r["CreatedBy"] as string,
                CreatedDate  = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate"))
            };
        }
    }
}
