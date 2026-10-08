using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class FolderAuthorityRepository : IFolderAuthorityRepository
    {
        private readonly string _connectionString;

        public FolderAuthorityRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<FolderAuthorityConfig> GetAll()
        {
            var result = new List<FolderAuthorityConfig>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_GetAllFolderAuthorityConfigs", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) result.Add(Map(r));
                }
            }
            return result;
        }

        public IEnumerable<FolderAuthorityConfig> Resolve(string positionName, string departmentCode, string divisionCode, string levelCode = null)
        {
            var result = new List<FolderAuthorityConfig>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_ResolveFolderAuthority", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LevelCode",      (object)levelCode      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PositionName",   (object)positionName   ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentCode", (object)departmentCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DivisionCode",   (object)divisionCode   ?? DBNull.Value);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) result.Add(Map(r));
                }
            }
            return result;
        }

        public int Insert(FolderAuthorityConfig c, int createdBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_InsertFolderAuthorityConfig", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AddParams(cmd, c);
                    cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                    int id = Convert.ToInt32(cmd.ExecuteScalar());
                    SetCanDownload(conn, id, c.CanDownload);
                    return id;
                }
            }
        }

        public void Update(FolderAuthorityConfig c, int updatedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_UpdateFolderAuthorityConfig", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ConfigID", c.ConfigID);
                    AddParams(cmd, c);
                    cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                    cmd.ExecuteNonQuery();
                }
                SetCanDownload(conn, c.ConfigID, c.CanDownload);
            }
        }

        /// <summary>
        /// CanDownload is written separately so the existing insert/update SPs need no change.
        /// If the column does not exist yet (download_policy.sql not applied) a false value is ignored;
        /// a true value fails loudly so the admin knows the script is missing.
        /// </summary>
        private static void SetCanDownload(SqlConnection conn, int configId, bool canDownload)
        {
            try
            {
                using (var cmd = new SqlCommand(
                    "UPDATE EMS.FolderAuthorityConfig SET CanDownload = @CanDownload WHERE ConfigID = @ConfigID", conn))
                {
                    cmd.Parameters.AddWithValue("@CanDownload", canDownload);
                    cmd.Parameters.AddWithValue("@ConfigID",    configId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                const int InvalidColumnName = 207;
                if (ex.Number != InvalidColumnName) throw;
                if (canDownload)
                    throw new InvalidOperationException(
                        "ยังไม่ได้รัน script download_policy.sql (ไม่มีคอลัมน์ CanDownload) จึงบันทึกสิทธิ์ดาวน์โหลดไม่ได้", ex);
            }
        }

        public void Delete(string configLabel, int deletedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_DeleteFolderAuthorityConfig", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ConfigLabel", configLabel ?? "");
                    cmd.Parameters.AddWithValue("@DeletedBy",   deletedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void AddParams(SqlCommand cmd, FolderAuthorityConfig c)
        {
            cmd.Parameters.AddWithValue("@ConfigLabel",          c.ConfigLabel          ?? "");
            cmd.Parameters.AddWithValue("@PositionName",         (object)c.PositionName   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DepartmentCode",       (object)c.DepartmentCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DivisionCode",         (object)c.DivisionCode   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LevelCode",            (object)c.LevelCode      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ConfidentialityLevel", (byte)c.ConfidentialityLevel);
            cmd.Parameters.AddWithValue("@CanView",    c.CanView);
            cmd.Parameters.AddWithValue("@CanAdd",     c.CanAdd);
            cmd.Parameters.AddWithValue("@CanEdit",    c.CanEdit);
            cmd.Parameters.AddWithValue("@CanApprove", c.CanApprove);
        }

        private static FolderAuthorityConfig Map(SqlDataReader r)
        {
            // LevelCode อาจยังไม่มีใน DB เก่า — ใช้ safe read
            string levelCode = null;
            try { levelCode = r["LevelCode"] as string; } catch { }

            // CanDownload อาจยังไม่มีใน DB เก่า (ยังไม่ได้รัน download_policy.sql)
            bool canDownload = false;
            try { canDownload = r["CanDownload"] != DBNull.Value && (bool)r["CanDownload"]; } catch { }

            return new FolderAuthorityConfig
            {
                ConfigID             = (int)r["ConfigID"],
                ConfigLabel          = r["ConfigLabel"]    as string ?? "",
                PositionName         = r["PositionName"]   as string,
                DepartmentCode       = r["DepartmentCode"] as string,
                DivisionCode         = r["DivisionCode"]   as string,
                ConfidentialityLevel = Convert.ToInt32(r["ConfidentialityLevel"]),
                // LevelCode is the employee job level (e.g. 3A). Do not substitute the confidentiality
                // badge ("1HC".."4PC") when it is empty: that value was saved back on edit and stopped
                // the config from ever matching a job level. Trim the nchar padding.
                LevelCode            = string.IsNullOrWhiteSpace(levelCode) ? null : levelCode.Trim(),
                CanView    = (bool)r["CanView"],
                CanAdd     = (bool)r["CanAdd"],
                CanEdit    = (bool)r["CanEdit"],
                CanApprove = (bool)r["CanApprove"],
                CanDownload = canDownload,
                IsActive    = (bool)r["IsActive"],
                CreatedBy   = r["CreatedBy"]   == DBNull.Value ? (int?)null : (int)r["CreatedBy"],
                CreatedDate = (DateTime)r["CreatedDate"],
                UpdatedBy   = r["UpdatedBy"]   == DBNull.Value ? (int?)null : (int)r["UpdatedBy"],
                UpdatedDate = r["UpdatedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["UpdatedDate"]
            };
        }
    }
}

