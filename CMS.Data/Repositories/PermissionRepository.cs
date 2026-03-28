using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly string _connectionString;

        public PermissionRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(FolderPermission permission)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_InsertFolderPermission", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderID",       permission.FolderID);
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

        public IEnumerable<FolderPermission> GetByFolder(int folderId)
        {
            var results = new List<FolderPermission>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_GetPermissionsByFolder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapPermission(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<FolderPermission> GetAllByFolder(int folderId)
        {
            var results = new List<FolderPermission>();
            System.Diagnostics.Debug.WriteLine($"[REPO] GetAllByFolder: FolderID={folderId}, ConnStr={(string.IsNullOrEmpty(_connectionString) ? "EMPTY!" : "OK")}");

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine($"[REPO] DB connection opened (State={conn.State})");
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[REPO] Calling SP: security.sp_GetAllPermissionsByFolder");
                    using (var cmd = new SqlCommand("security.sp_GetAllPermissionsByFolder", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FolderID", folderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) results.Add(MapPermission(reader));
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"[REPO] SP succeeded → {results.Count} rows");
                }
                catch (System.Data.SqlClient.SqlException sqlEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[REPO] SP FAILED ({sqlEx.Number}): {sqlEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"[REPO] Falling back to direct SQL query");

                    // SP may not exist yet — fall back to direct query
                    results.Clear();
                    const string sql =
                        @"SELECT PermissionID, FolderID, PrincipalType, PrincipalID,
                                 CanRead, CanWrite, CanDelete, CanModify, CanFullControl, IsDeny,
                                 ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
                          FROM   security.FolderPermission
                          WHERE  FolderID = @FolderID
                          ORDER  BY CASE WHEN RevokedDate IS NULL THEN 0 ELSE 1 END, GrantedDate DESC";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@FolderID", folderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) results.Add(MapPermission(reader));
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"[REPO] Direct SQL → {results.Count} rows");
                }
            }

            foreach (var r in results)
                System.Diagnostics.Debug.WriteLine($"[REPO]   Row: PermID={r.PermissionID} Principal={r.PrincipalType}#{r.PrincipalID} Read={r.CanRead} Write={r.CanWrite} Revoked={r.RevokedDate.HasValue}");

            return results;
        }

        public IEnumerable<FolderPermission> GetByPrincipal(int principalType, int principalId)
        {
            var results = new List<FolderPermission>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_GetPermissionsByPrincipal", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PrincipalType", (byte)principalType);
                    cmd.Parameters.AddWithValue("@PrincipalID",   principalId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapPermission(reader));
                    }
                }
            }
            return results;
        }

        public void Revoke(int permissionId, int revokedBy, string revokeReason)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_RevokePermission", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PermissionID", permissionId);
                    cmd.Parameters.AddWithValue("@RevokedBy",    revokedBy);
                    cmd.Parameters.AddWithValue("@RevokeReason", (object)revokeReason ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteByFolder(int folderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM security.FolderPermission WHERE FolderID = @FolderID", conn))
                {
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>Alias for GetByFolder — returns active ACL entries for the folder.</summary>
        public IEnumerable<FolderPermission> GetAclEntries(int folderId)
        {
            return GetByFolder(folderId);
        }

        private static FolderPermission MapPermission(SqlDataReader reader)
        {
            return new FolderPermission
            {
                PermissionID   = (int)reader["PermissionID"],
                FolderID       = (int)reader["FolderID"],
                PrincipalType  = (PrincipalType)(byte)reader["PrincipalType"],
                PrincipalID    = (int)reader["PrincipalID"],
                CanRead        = (bool)reader["CanRead"],
                CanWrite       = (bool)reader["CanWrite"],
                CanDelete      = (bool)reader["CanDelete"],
                CanModify      = (bool)reader["CanModify"],
                CanFullControl = (bool)reader["CanFullControl"],
                IsDeny         = (bool)reader["IsDeny"],
                ExpiryDate     = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ExpiryDate"],
                GrantedBy      = (int)reader["GrantedBy"],
                GrantedDate    = (DateTime)reader["GrantedDate"],
                RevokedBy      = reader["RevokedBy"] == DBNull.Value ? (int?)null : (int)reader["RevokedBy"],
                RevokedDate    = reader["RevokedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["RevokedDate"],
                RevokeReason   = reader["RevokeReason"] == DBNull.Value ? null : (string)reader["RevokeReason"]
            };
        }
    }
}
