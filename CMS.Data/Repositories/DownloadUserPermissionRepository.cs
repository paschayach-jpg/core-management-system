using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class DownloadUserPermissionRepository : IDownloadUserPermissionRepository
    {
        private readonly string _connectionString;

        private const string SelectActive = @"
SELECT p.PermissionID, p.UserID, p.CanHC, p.CanSC, p.CanNC, p.CanPC, p.IsActive,
       p.CreatedBy, p.CreatedDate, p.UpdatedBy, p.UpdatedDate,
       u.ADUserName, u.DisplayName, u.PositionName, u.LevelCode, u.DepartmentName
FROM   workflow.DownloadUserPermission p
JOIN   EMS.UserAccount u ON u.UserID = p.UserID
WHERE  p.IsActive = 1";

        public DownloadUserPermissionRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<DownloadUserPermission> GetAll()
        {
            var result = new List<DownloadUserPermission>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(SelectActive + " ORDER BY u.DisplayName", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) result.Add(Map(r));
            }
            return result;
        }

        public DownloadUserPermission GetByUser(int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(SelectActive + " AND p.UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? Map(r) : null;
                }
            }
        }

        public void Upsert(DownloadUserPermission permission, int updatedBy)
        {
            if (permission == null) throw new ArgumentNullException("permission");

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
UPDATE workflow.DownloadUserPermission
SET    CanHC = @CanHC, CanSC = @CanSC, CanNC = @CanNC, CanPC = @CanPC,
       IsActive = 1, UpdatedBy = @By, UpdatedDate = GETDATE()
WHERE  UserID = @UserID;

IF @@ROWCOUNT = 0
    INSERT INTO workflow.DownloadUserPermission
        (UserID, CanHC, CanSC, CanNC, CanPC, IsActive, CreatedBy, CreatedDate)
    VALUES (@UserID, @CanHC, @CanSC, @CanNC, @CanPC, 1, @By, GETDATE());", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", permission.UserID);
                    cmd.Parameters.AddWithValue("@CanHC",  permission.CanHC);
                    cmd.Parameters.AddWithValue("@CanSC",  permission.CanSC);
                    cmd.Parameters.AddWithValue("@CanNC",  permission.CanNC);
                    cmd.Parameters.AddWithValue("@CanPC",  permission.CanPC);
                    cmd.Parameters.AddWithValue("@By",     updatedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Deactivate(int userId, int updatedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
UPDATE workflow.DownloadUserPermission
SET    IsActive = 0, UpdatedBy = @By, UpdatedDate = GETDATE()
WHERE  UserID = @UserID AND IsActive = 1", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@By",     updatedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static DownloadUserPermission Map(SqlDataReader r)
        {
            return new DownloadUserPermission
            {
                PermissionID   = (int)r["PermissionID"],
                UserID         = (int)r["UserID"],
                CanHC          = (bool)r["CanHC"],
                CanSC          = (bool)r["CanSC"],
                CanNC          = (bool)r["CanNC"],
                CanPC          = (bool)r["CanPC"],
                IsActive       = (bool)r["IsActive"],
                CreatedBy      = r["CreatedBy"]   == DBNull.Value ? (int?)null : (int)r["CreatedBy"],
                CreatedDate    = (DateTime)r["CreatedDate"],
                UpdatedBy      = r["UpdatedBy"]   == DBNull.Value ? (int?)null : (int)r["UpdatedBy"],
                UpdatedDate    = r["UpdatedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["UpdatedDate"],
                ADUserName     = r["ADUserName"]     as string,
                DisplayName    = r["DisplayName"]    as string,
                PositionName   = r["PositionName"]   as string,
                LevelCode      = r["LevelCode"] == DBNull.Value ? null : ((string)r["LevelCode"]).Trim(),
                DepartmentName = r["DepartmentName"] as string
            };
        }
    }
}
