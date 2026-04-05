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
                using (var cmd = new SqlCommand("core.sp_GetAllFolderAuthorityConfigs", conn))
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
                using (var cmd = new SqlCommand("core.sp_ResolveFolderAuthority", conn))
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
                using (var cmd = new SqlCommand("core.sp_InsertFolderAuthorityConfig", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AddParams(cmd, c);
                    cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(FolderAuthorityConfig c, int updatedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_UpdateFolderAuthorityConfig", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ConfigID", c.ConfigID);
                    AddParams(cmd, c);
                    cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(string configLabel, int deletedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_DeleteFolderAuthorityConfig", conn))
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

            return new FolderAuthorityConfig
            {
                ConfigID             = (int)r["ConfigID"],
                ConfigLabel          = r["ConfigLabel"]    as string ?? "",
                PositionName         = r["PositionName"]   as string,
                DepartmentCode       = r["DepartmentCode"] as string,
                DivisionCode         = r["DivisionCode"]   as string,
                ConfidentialityLevel = Convert.ToInt32(r["ConfidentialityLevel"]),
                LevelCode            = levelCode ?? LevelCodeFromInt(Convert.ToInt32(r["ConfidentialityLevel"])),
                CanView    = (bool)r["CanView"],
                CanAdd     = (bool)r["CanAdd"],
                CanEdit    = (bool)r["CanEdit"],
                CanApprove = (bool)r["CanApprove"],
                IsActive    = (bool)r["IsActive"],
                CreatedBy   = r["CreatedBy"]   == DBNull.Value ? (int?)null : (int)r["CreatedBy"],
                CreatedDate = (DateTime)r["CreatedDate"],
                UpdatedBy   = r["UpdatedBy"]   == DBNull.Value ? (int?)null : (int)r["UpdatedBy"],
                UpdatedDate = r["UpdatedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["UpdatedDate"]
            };
        }

        private static string LevelCodeFromInt(int level)
        {
            switch (level)
            {
                case 1: return "1HC";
                case 2: return "2SC";
                case 3: return "3NC";
                case 4: return "4PC";
                default: return null;
            }
        }
    }
}
