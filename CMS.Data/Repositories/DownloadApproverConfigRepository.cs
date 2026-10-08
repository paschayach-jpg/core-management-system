using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class DownloadApproverConfigRepository : IDownloadApproverConfigRepository
    {
        private readonly string _connectionString;

        private const string SelectActive = @"
SELECT c.ConfigID, c.ScopeType, c.OrgCode, c.ApproverUserID, c.SortOrder, c.IsActive,
       c.CreatedBy, c.CreatedDate, c.UpdatedBy, c.UpdatedDate,
       u.DisplayName, u.PositionName, u.LevelCode, u.DepartmentName
FROM   workflow.DownloadApproverConfig c
JOIN   EMS.UserAccount u ON u.UserID = c.ApproverUserID
WHERE  c.IsActive = 1";

        public DownloadApproverConfigRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<DownloadApproverConfig> GetAll()
        {
            var result = new List<DownloadApproverConfig>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(SelectActive + " ORDER BY c.ScopeType, c.OrgCode, c.SortOrder, c.ConfigID", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) result.Add(Map(r));
            }
            return result;
        }

        public IEnumerable<DownloadApproverConfig> GetByOrg(string scopeType, string orgCode)
        {
            var result = new List<DownloadApproverConfig>();
            if (string.IsNullOrWhiteSpace(scopeType) || string.IsNullOrWhiteSpace(orgCode))
                return result;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(SelectActive +
                    " AND c.ScopeType = @ScopeType AND c.OrgCode = @OrgCode ORDER BY c.SortOrder, c.ConfigID", conn))
                {
                    cmd.Parameters.AddWithValue("@ScopeType", scopeType);
                    cmd.Parameters.AddWithValue("@OrgCode",   orgCode.Trim());
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) result.Add(Map(r));
                }
            }
            return result;
        }

        public void ReplaceForOrg(string scopeType, string orgCode, IList<int> approverUserIds, int updatedBy)
        {
            if (string.IsNullOrWhiteSpace(scopeType)) throw new ArgumentNullException("scopeType");
            if (string.IsNullOrWhiteSpace(orgCode))   throw new ArgumentNullException("orgCode");
            if (approverUserIds == null)               throw new ArgumentNullException("approverUserIds");

            string code = orgCode.Trim();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // ปิดทุกคนของ org นี้ก่อน แล้วเปิด/เพิ่มเฉพาะคนในรายการใหม่ตามลำดับ
                    using (var cmd = new SqlCommand(@"
UPDATE workflow.DownloadApproverConfig
SET    IsActive = 0, UpdatedBy = @By, UpdatedDate = GETDATE()
WHERE  ScopeType = @ScopeType AND OrgCode = @OrgCode AND IsActive = 1", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@ScopeType", scopeType);
                        cmd.Parameters.AddWithValue("@OrgCode",   code);
                        cmd.Parameters.AddWithValue("@By",        updatedBy);
                        cmd.ExecuteNonQuery();
                    }

                    for (int i = 0; i < approverUserIds.Count; i++)
                    {
                        using (var cmd = new SqlCommand(@"
UPDATE workflow.DownloadApproverConfig
SET    IsActive = 1, SortOrder = @SortOrder, UpdatedBy = @By, UpdatedDate = GETDATE()
WHERE  ScopeType = @ScopeType AND OrgCode = @OrgCode AND ApproverUserID = @UserID;

IF @@ROWCOUNT = 0
    INSERT INTO workflow.DownloadApproverConfig
        (ScopeType, OrgCode, ApproverUserID, SortOrder, IsActive, CreatedBy, CreatedDate)
    VALUES (@ScopeType, @OrgCode, @UserID, @SortOrder, 1, @By, GETDATE());", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ScopeType", scopeType);
                            cmd.Parameters.AddWithValue("@OrgCode",   code);
                            cmd.Parameters.AddWithValue("@UserID",    approverUserIds[i]);
                            cmd.Parameters.AddWithValue("@SortOrder", i + 1);
                            cmd.Parameters.AddWithValue("@By",        updatedBy);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
        }

        private static DownloadApproverConfig Map(SqlDataReader r)
        {
            return new DownloadApproverConfig
            {
                ConfigID       = (int)r["ConfigID"],
                ScopeType      = ((string)r["ScopeType"]).Trim(),
                OrgCode        = ((string)r["OrgCode"]).Trim(),
                ApproverUserID = (int)r["ApproverUserID"],
                SortOrder      = (int)r["SortOrder"],
                IsActive       = (bool)r["IsActive"],
                CreatedBy      = r["CreatedBy"]   == DBNull.Value ? (int?)null : (int)r["CreatedBy"],
                CreatedDate    = (DateTime)r["CreatedDate"],
                UpdatedBy      = r["UpdatedBy"]   == DBNull.Value ? (int?)null : (int)r["UpdatedBy"],
                UpdatedDate    = r["UpdatedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["UpdatedDate"],
                ApproverName   = r["DisplayName"]    as string,
                PositionName   = r["PositionName"]   as string,
                LevelCode      = r["LevelCode"] == DBNull.Value ? null : ((string)r["LevelCode"]).Trim(),
                DepartmentName = r["DepartmentName"] as string
            };
        }
    }
}
