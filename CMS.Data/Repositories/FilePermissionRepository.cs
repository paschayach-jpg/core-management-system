using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class FilePermissionRepository : IFilePermissionRepository
    {
        private readonly string _connectionString;

        public FilePermissionRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(FilePermission permission)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_InsertFilePermission", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileID",         permission.FileID);
                    cmd.Parameters.AddWithValue("@PrincipalType",  (byte)permission.PrincipalType);
                    cmd.Parameters.AddWithValue("@PrincipalID",    permission.PrincipalID);
                    cmd.Parameters.AddWithValue("@CanRead",        permission.CanRead);
                    cmd.Parameters.AddWithValue("@CanWrite",       permission.CanWrite);
                    cmd.Parameters.AddWithValue("@CanDelete",      permission.CanDelete);
                    cmd.Parameters.AddWithValue("@CanModify",      permission.CanModify);
                    cmd.Parameters.AddWithValue("@CanFullControl", permission.CanFullControl);
                    cmd.Parameters.AddWithValue("@IsDeny",         permission.IsDeny);
                    cmd.Parameters.AddWithValue("@ExpiryDate",     (object)permission.ExpiryDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GrantedBy",      permission.GrantedBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public IEnumerable<FilePermission> GetByFile(int fileId)
        {
            var results = new List<FilePermission>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_GetFilePermissionsByFile", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileID", fileId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapPermission(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<FilePermission> GetAllByFile(int fileId)
        {
            var results = new List<FilePermission>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_GetAllFilePermissionsByFile", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileID", fileId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapPermission(reader));
                    }
                }
            }
            return results;
        }

        public void Revoke(int filePermissionId, int revokedBy, string revokeReason)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_RevokeFilePermission", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FilePermissionID", filePermissionId);
                    cmd.Parameters.AddWithValue("@RevokedBy",        revokedBy);
                    cmd.Parameters.AddWithValue("@RevokeReason",     (object)revokeReason ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteByFile(int fileId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM security.FilePermission WHERE FileID = @FileID", conn))
                {
                    cmd.Parameters.AddWithValue("@FileID", fileId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static FilePermission MapPermission(SqlDataReader reader)
        {
            return new FilePermission
            {
                FilePermissionID = (int)reader["FilePermissionID"],
                FileID           = (int)reader["FileID"],
                PrincipalType    = (byte)reader["PrincipalType"],
                PrincipalID      = (int)reader["PrincipalID"],
                CanRead          = (bool)reader["CanRead"],
                CanWrite         = (bool)reader["CanWrite"],
                CanDelete        = (bool)reader["CanDelete"],
                CanModify        = (bool)reader["CanModify"],
                CanFullControl   = (bool)reader["CanFullControl"],
                IsDeny           = (bool)reader["IsDeny"],
                ExpiryDate       = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ExpiryDate"],
                GrantedBy        = (int)reader["GrantedBy"],
                GrantedDate      = (DateTime)reader["GrantedDate"],
                RevokedBy        = reader["RevokedBy"] == DBNull.Value ? (int?)null : (int)reader["RevokedBy"],
                RevokedDate      = reader["RevokedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["RevokedDate"],
                RevokeReason     = reader["RevokeReason"] == DBNull.Value ? null : (string)reader["RevokeReason"]
            };
        }
    }
}
