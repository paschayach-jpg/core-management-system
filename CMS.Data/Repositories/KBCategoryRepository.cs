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
                           c.DisplayProfileID, c.ImportTypeDigit,
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
                           c.DisplayProfileID, c.ImportTypeDigit,
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
                           c.DisplayProfileID, c.ImportTypeDigit,
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

        public KBCategory GetByCode(string categoryCode)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT c.CategoryID, c.ParentCategoryID, c.CategoryCode,
                           c.CategoryName, c.Icon, c.Description,
                           c.SortOrder, c.IsActive, c.CreatedBy, c.CreatedDate,
                           c.DisplayProfileID, c.ImportTypeDigit,
                           p.CategoryName AS ParentCategoryName
                    FROM   KMS.m_KBCategory c
                    LEFT JOIN KMS.m_KBCategory p ON p.CategoryID = c.ParentCategoryID
                    WHERE  c.CategoryCode = @CategoryCode";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryCode", categoryCode ?? string.Empty);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return Map(reader);
                    }
                }
            }
            return null;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        /// <summary>
        /// เพิ่มหมวดใหม่ คืน CategoryID ที่ได้. CategoryID เป็น IDENTITY ในตารางที่สร้างจาก 03_KMS_WFE_TMS_Schema.sql แต่ในบาง DB
        /// (เช่น CMS_DB) ไม่ใช่ IDENTITY — กรณีนั้นใช้ MAX(CategoryID)+1 ใน transaction เดียวกับ INSERT (ล็อกช่วงด้วย UPDLOCK/HOLDLOCK
        /// ผู้เพิ่มพร้อมกันจึงรอกันและไม่ได้เลขซ้ำ)
        /// </summary>
        public int Insert(KBCategory category)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    int newId;

                    if (IsCategoryIdIdentity(conn, tx))
                    {
                        const string identitySql = @"
                            INSERT INTO KMS.m_KBCategory
                                (ParentCategoryID, CategoryCode, CategoryName, Icon,
                                 Description, SortOrder, IsActive, CreatedBy, CreatedDate, DisplayProfileID, ImportTypeDigit)
                            VALUES
                                (@ParentCategoryID, @CategoryCode, @CategoryName, @Icon,
                                 @Description, @SortOrder, 1, @CreatedBy, GETDATE(), @DisplayProfileID, @ImportTypeDigit);
                            SELECT CONVERT(INT, SCOPE_IDENTITY());";
                        using (var cmd = new SqlCommand(identitySql, conn, tx))
                        {
                            AddParams(cmd, category);
                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    else
                    {
                        const string manualIdSql = @"
                            DECLARE @NewID INT;
                            SELECT @NewID = ISNULL(MAX(CategoryID), 0) + 1
                            FROM   KMS.m_KBCategory WITH (UPDLOCK, HOLDLOCK);

                            INSERT INTO KMS.m_KBCategory
                                (CategoryID, ParentCategoryID, CategoryCode, CategoryName, Icon,
                                 Description, SortOrder, IsActive, CreatedBy, CreatedDate, DisplayProfileID, ImportTypeDigit)
                            VALUES
                                (@NewID, @ParentCategoryID, @CategoryCode, @CategoryName, @Icon,
                                 @Description, @SortOrder, 1, @CreatedBy, GETDATE(), @DisplayProfileID, @ImportTypeDigit);
                            SELECT @NewID;";
                        using (var cmd = new SqlCommand(manualIdSql, conn, tx))
                        {
                            AddParams(cmd, category);
                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }

                    tx.Commit();
                    return newId;
                }
            }
        }

        /// <summary>true ถ้าคอลัมน์ KMS.m_KBCategory.CategoryID เป็น IDENTITY (DB ออกเลขให้เอง)</summary>
        private static bool IsCategoryIdIdentity(SqlConnection conn, SqlTransaction tx)
        {
            const string sql = "SELECT ISNULL(COLUMNPROPERTY(OBJECT_ID('KMS.m_KBCategory'), 'CategoryID', 'IsIdentity'), 0)";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
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
                        SortOrder        = @SortOrder,
                        DisplayProfileID = @DisplayProfileID,
                        ImportTypeDigit  = @ImportTypeDigit
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
            cmd.Parameters.AddWithValue("@DisplayProfileID",  (object)c.DisplayProfileID ?? DBNull.Value);
            cmd.Parameters.Add("@ImportTypeDigit", SqlDbType.Char, 1).Value = (object)c.ImportTypeDigit ?? DBNull.Value;
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
                DisplayProfileID   = r.IsDBNull(r.GetOrdinal("DisplayProfileID")) ? (int?)null : r.GetInt32(r.GetOrdinal("DisplayProfileID")),
                ImportTypeDigit    = r.IsDBNull(r.GetOrdinal("ImportTypeDigit")) ? null : Convert.ToString(r["ImportTypeDigit"]).Trim(),
                ParentCategoryName = r["ParentCategoryName"] as string,
            };
        }
    }
}
