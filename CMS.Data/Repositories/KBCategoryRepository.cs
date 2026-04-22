using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class KBCategoryRepository : IKBCategoryRepository
    {
        private readonly string _connectionString;

        public KBCategoryRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public IEnumerable<KBCategory> GetAll()
        {
            var results = new List<KBCategory>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT c.CategoryID, c.ParentCategoryID, c.CategoryCode,
                           c.CategoryName, c.Icon, c.Description,
                           c.SortOrder, c.IsActive, c.CreatedBy, c.CreatedDate,
                           p.CategoryName AS ParentCategoryName
                    FROM   KMS.m_KBCategory c
                    LEFT JOIN KMS.m_KBCategory p ON p.CategoryID = c.ParentCategoryID
                    ORDER BY c.SortOrder, c.CategoryName";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        results.Add(Map(reader));
                }
            }
            return results;
        }

        public IEnumerable<KBCategory> GetAllActive()
        {
            var results = new List<KBCategory>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT c.CategoryID, c.ParentCategoryID, c.CategoryCode,
                           c.CategoryName, c.Icon, c.Description,
                           c.SortOrder, c.IsActive, c.CreatedBy, c.CreatedDate,
                           p.CategoryName AS ParentCategoryName
                    FROM   KMS.m_KBCategory c
                    LEFT JOIN KMS.m_KBCategory p ON p.CategoryID = c.ParentCategoryID
                    WHERE  c.IsActive = 1
                    ORDER BY c.SortOrder, c.CategoryName";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        results.Add(Map(reader));
                }
            }
            return results;
        }

        public KBCategory GetById(int categoryId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT c.CategoryID, c.ParentCategoryID, c.CategoryCode,
                           c.CategoryName, c.Icon, c.Description,
                           c.SortOrder, c.IsActive, c.CreatedBy, c.CreatedDate,
                           p.CategoryName AS ParentCategoryName
                    FROM   KMS.m_KBCategory c
                    LEFT JOIN KMS.m_KBCategory p ON p.CategoryID = c.ParentCategoryID
                    WHERE  c.CategoryID = @CategoryID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return Map(reader);
                    }
                }
            }
            return null;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public int Insert(KBCategory category)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO KMS.m_KBCategory
                        (ParentCategoryID, CategoryCode, CategoryName, Icon,
                         Description, SortOrder, IsActive, CreatedBy, CreatedDate)
                    VALUES
                        (@ParentCategoryID, @CategoryCode, @CategoryName, @Icon,
                         @Description, @SortOrder, 1, @CreatedBy, GETDATE());
                    SELECT SCOPE_IDENTITY();";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    AddParams(cmd, category);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(KBCategory category)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    UPDATE KMS.m_KBCategory SET
                        ParentCategoryID = @ParentCategoryID,
                        CategoryCode     = @CategoryCode,
                        CategoryName     = @CategoryName,
                        Icon             = @Icon,
                        Description      = @Description,
                        SortOrder        = @SortOrder
                    WHERE CategoryID = @CategoryID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", category.CategoryID);
                    AddParams(cmd, category);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SoftDelete(int categoryId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "UPDATE KMS.m_KBCategory SET IsActive = 0 WHERE CategoryID = @CategoryID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void AddParams(SqlCommand cmd, KBCategory c)
        {
            cmd.Parameters.AddWithValue("@ParentCategoryID", (object)c.ParentCategoryID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryCode",     c.CategoryCode ?? string.Empty);
            cmd.Parameters.AddWithValue("@CategoryName",     c.CategoryName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Icon",             (object)c.Icon ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description",      (object)c.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SortOrder",        c.SortOrder);
            cmd.Parameters.AddWithValue("@CreatedBy",        c.CreatedBy ?? string.Empty);
        }

        private static KBCategory Map(IDataReader r)
        {
            return new KBCategory
            {
                CategoryID         = r.GetInt32(r.GetOrdinal("CategoryID")),
                ParentCategoryID   = r.IsDBNull(r.GetOrdinal("ParentCategoryID")) ? (int?)null : r.GetInt32(r.GetOrdinal("ParentCategoryID")),
                CategoryCode       = r["CategoryCode"]     as string,
                CategoryName       = r["CategoryName"]     as string,
                Icon               = r["Icon"] as string,
                Description        = r["Description"]      as string,
                SortOrder          = r.IsDBNull(r.GetOrdinal("SortOrder")) ? 0 : r.GetInt32(r.GetOrdinal("SortOrder")),
                IsActive           = !r.IsDBNull(r.GetOrdinal("IsActive")) && r.GetBoolean(r.GetOrdinal("IsActive")),
                CreatedBy          = r["CreatedBy"]        as string,
                CreatedDate        = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate")),
                ParentCategoryName = r["ParentCategoryName"] as string,
            };
        }
    }
}
