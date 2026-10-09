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
        private static bool _schemaEnsured = false;
        private static readonly object _schemaLock = new object();

        private const string SelectActive = @"
SELECT c.ConfigID, c.ApprovalType, c.ScopeType, c.OrgCode, c.ApproverUserID, c.SortOrder, c.IsActive,
       c.CreatedBy, c.CreatedDate, c.UpdatedBy, c.UpdatedDate,
       u.DisplayName, u.PositionName, u.LevelCode, u.DepartmentName
FROM   workflow.DownloadApproverConfig c
JOIN   EMS.UserAccount u ON u.UserID = c.ApproverUserID
WHERE  c.IsActive = 1";

        public DownloadApproverConfigRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        private void EnsureSchema()
        {
            if (_schemaEnsured) return;
            lock (_schemaLock)
            {
                if (_schemaEnsured) return;
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(@"
IF OBJECT_ID(N'workflow.DownloadApproverConfig', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('workflow.DownloadApproverConfig', 'ApprovalType') IS NULL
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_DownloadApprover' AND parent_object_id = OBJECT_ID('workflow.DownloadApproverConfig'))
            ALTER TABLE workflow.DownloadApproverConfig DROP CONSTRAINT UQ_DownloadApprover;

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DownloadApprover_Org' AND object_id = OBJECT_ID('workflow.DownloadApproverConfig'))
            DROP INDEX IX_DownloadApprover_Org ON workflow.DownloadApproverConfig;

        ALTER TABLE workflow.DownloadApproverConfig
            ADD ApprovalType CHAR(1) NOT NULL CONSTRAINT DF_DownloadApprover_ApprovalType DEFAULT ('D');

        ALTER TABLE workflow.DownloadApproverConfig
            ADD CONSTRAINT CK_DownloadApprover_ApprovalType CHECK (ApprovalType IN ('D', 'S'));

        ALTER TABLE workflow.DownloadApproverConfig
            ADD CONSTRAINT UQ_DownloadApprover UNIQUE (ApprovalType, ScopeType, OrgCode, ApproverUserID);

        CREATE INDEX IX_DownloadApprover_Org ON workflow.DownloadApproverConfig (ApprovalType, ScopeType, OrgCode, IsActive);
    END
END", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    _schemaEnsured = true;
                }
                catch
                {
                    // If DB user lacks DDL permissions, let runtime queries execute and fallback
                }
            }
        }

        public IEnumerable<DownloadApproverConfig> GetAll(string approvalType = DownloadApproverConfig.TypeDownload)
        {
            EnsureSchema();
            string type = string.IsNullOrWhiteSpace(approvalType) ? DownloadApproverConfig.TypeDownload : approvalType.Trim();
            var result = new List<DownloadApproverConfig>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                try
                {
                    using (var cmd = new SqlCommand(SelectActive + " AND c.ApprovalType = @ApprovalType ORDER BY c.ScopeType, c.OrgCode, c.SortOrder, c.ConfigID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ApprovalType", type);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read()) result.Add(Map(r));
                    }
                }
                catch (SqlException ex) when (ex.Message.IndexOf("ApprovalType", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Fallback for old schema before migration
                    if (type == DownloadApproverConfig.TypeDownload)
                    {
                        string fallbackSql = @"
SELECT c.ConfigID, c.ScopeType, c.OrgCode, c.ApproverUserID, c.SortOrder, c.IsActive,
       c.CreatedBy, c.CreatedDate, c.UpdatedBy, c.UpdatedDate,
       u.DisplayName, u.PositionName, u.LevelCode, u.DepartmentName
FROM   workflow.DownloadApproverConfig c
JOIN   EMS.UserAccount u ON u.UserID = c.ApproverUserID
WHERE  c.IsActive = 1
ORDER BY c.ScopeType, c.OrgCode, c.SortOrder, c.ConfigID";
                        using (var cmd = new SqlCommand(fallbackSql, conn))
                        using (var r = cmd.ExecuteReader())
                            while (r.Read()) result.Add(Map(r));
                    }
                }
            }
            return result;
        }

        public IEnumerable<DownloadApproverConfig> GetByOrg(string scopeType, string orgCode, string approvalType = DownloadApproverConfig.TypeDownload)
        {
            EnsureSchema();
            var result = new List<DownloadApproverConfig>();
            if (string.IsNullOrWhiteSpace(scopeType) || string.IsNullOrWhiteSpace(orgCode))
                return result;

            string type = string.IsNullOrWhiteSpace(approvalType) ? DownloadApproverConfig.TypeDownload : approvalType.Trim();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                try
                {
                    using (var cmd = new SqlCommand(SelectActive +
                        " AND c.ApprovalType = @ApprovalType AND c.ScopeType = @ScopeType AND c.OrgCode = @OrgCode ORDER BY c.SortOrder, c.ConfigID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ApprovalType", type);
                        cmd.Parameters.AddWithValue("@ScopeType",    scopeType.Trim());
                        cmd.Parameters.AddWithValue("@OrgCode",      orgCode.Trim());
                        using (var r = cmd.ExecuteReader())
                            while (r.Read()) result.Add(Map(r));
                    }
                }
                catch (SqlException ex) when (ex.Message.IndexOf("ApprovalType", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Fallback for old schema before migration
                    if (type == DownloadApproverConfig.TypeDownload)
                    {
                        string fallbackSql = @"
SELECT c.ConfigID, c.ScopeType, c.OrgCode, c.ApproverUserID, c.SortOrder, c.IsActive,
       c.CreatedBy, c.CreatedDate, c.UpdatedBy, c.UpdatedDate,
       u.DisplayName, u.PositionName, u.LevelCode, u.DepartmentName
FROM   workflow.DownloadApproverConfig c
JOIN   EMS.UserAccount u ON u.UserID = c.ApproverUserID
WHERE  c.IsActive = 1 AND c.ScopeType = @ScopeType AND c.OrgCode = @OrgCode
ORDER BY c.SortOrder, c.ConfigID";
                        using (var cmd = new SqlCommand(fallbackSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ScopeType", scopeType.Trim());
                            cmd.Parameters.AddWithValue("@OrgCode",   orgCode.Trim());
                            using (var r = cmd.ExecuteReader())
                                while (r.Read()) result.Add(Map(r));
                        }
                    }
                }
            }
            return result;
        }

        public void ReplaceForOrg(string scopeType, string orgCode, IList<int> approverUserIds, int updatedBy, string approvalType = DownloadApproverConfig.TypeDownload)
        {
            EnsureSchema();
            if (string.IsNullOrWhiteSpace(scopeType)) throw new ArgumentNullException("scopeType");
            if (string.IsNullOrWhiteSpace(orgCode))   throw new ArgumentNullException("orgCode");
            if (approverUserIds == null)               throw new ArgumentNullException("approverUserIds");

            string type = string.IsNullOrWhiteSpace(approvalType) ? DownloadApproverConfig.TypeDownload : approvalType.Trim();
            string scope = scopeType.Trim();
            string code = orgCode.Trim();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // ปิดทุกคนของ org นี้ตาม ApprovalType ก่อน แล้วเปิด/เพิ่มเฉพาะคนในรายการใหม่ตามลำดับ
                    using (var cmd = new SqlCommand(@"
UPDATE workflow.DownloadApproverConfig
SET    IsActive = 0, UpdatedBy = @By, UpdatedDate = GETDATE()
WHERE  ApprovalType = @ApprovalType AND ScopeType = @ScopeType AND OrgCode = @OrgCode AND IsActive = 1", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@ApprovalType", type);
                        cmd.Parameters.AddWithValue("@ScopeType",    scope);
                        cmd.Parameters.AddWithValue("@OrgCode",      code);
                        cmd.Parameters.AddWithValue("@By",           updatedBy);
                        cmd.ExecuteNonQuery();
                    }

                    for (int i = 0; i < approverUserIds.Count; i++)
                    {
                        using (var cmd = new SqlCommand(@"
UPDATE workflow.DownloadApproverConfig
SET    IsActive = 1, SortOrder = @SortOrder, UpdatedBy = @By, UpdatedDate = GETDATE()
WHERE  ApprovalType = @ApprovalType AND ScopeType = @ScopeType AND OrgCode = @OrgCode AND ApproverUserID = @UserID;

IF @@ROWCOUNT = 0
    INSERT INTO workflow.DownloadApproverConfig
        (ApprovalType, ScopeType, OrgCode, ApproverUserID, SortOrder, IsActive, CreatedBy, CreatedDate)
    VALUES (@ApprovalType, @ScopeType, @OrgCode, @UserID, @SortOrder, 1, @By, GETDATE());", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ApprovalType", type);
                            cmd.Parameters.AddWithValue("@ScopeType",    scope);
                            cmd.Parameters.AddWithValue("@OrgCode",      code);
                            cmd.Parameters.AddWithValue("@UserID",       approverUserIds[i]);
                            cmd.Parameters.AddWithValue("@SortOrder",    i + 1);
                            cmd.Parameters.AddWithValue("@By",           updatedBy);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
        }

        private static bool HasColumn(SqlDataReader r, string columnName)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (string.Equals(r.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static DownloadApproverConfig Map(SqlDataReader r)
        {
            return new DownloadApproverConfig
            {
                ConfigID       = (int)r["ConfigID"],
                ApprovalType   = HasColumn(r, "ApprovalType") ? ((string)r["ApprovalType"]).Trim() : DownloadApproverConfig.TypeDownload,
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
