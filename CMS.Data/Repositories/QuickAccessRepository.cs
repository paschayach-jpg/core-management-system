using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    /// <summary>
    /// ADO.NET repository สำหรับ Quick Access / Favorite Folder
    /// Requirements: 7.2, 7.4, 7.5
    /// </summary>
    public class QuickAccessRepository : IQuickAccessRepository
    {
        private readonly string _connectionString;

        public QuickAccessRepository(IConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<QuickAccessFolder> GetByUser(int userId)
        {
            var results = new List<QuickAccessFolder>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetQuickAccessByUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapQuickAccess(reader));
                    }
                }
            }
            return results;
        }

        public void Add(int userId, int folderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_AddQuickAccess", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID",   userId);
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Remove(int userId, int folderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_RemoveQuickAccess", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID",   userId);
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static QuickAccessFolder MapQuickAccess(SqlDataReader reader)
        {
            return new QuickAccessFolder
            {
                QuickAccessID = (int)reader["QuickAccessID"],
                UserID        = (int)reader["UserID"],
                FolderID      = (int)reader["FolderID"],
                AddedDate     = (DateTime)reader["AddedDate"]
            };
        }
    }
}
