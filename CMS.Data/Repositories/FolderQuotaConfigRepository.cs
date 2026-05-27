using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class FolderQuotaConfigRepository : IFolderQuotaConfigRepository
    {
        private readonly string _connectionString;

        private const string SelectMasterQuota = @"
SELECT o.OrgUnitID AS QuotaConfigID,
       o.OrgUnitID,
       o.ParentOrgUnitID,
       CAST(ISNULL(sc.quota_size, 0) AS BIGINT) AS QuotaBytes,
       CAST(80 AS TINYINT) AS WarnThresholdPct,
       CAST(95 AS TINYINT) AS CriticalThresholdPct,
       CAST(sc.IsActive AS BIT) AS IsActive,
       CAST(0 AS INT) AS CreatedBy,
       ISNULL(sc.CreatedDate, GETDATE()) AS CreatedDate,
       CAST(NULL AS INT) AS UpdatedBy,
       CAST(NULL AS DATETIME) AS UpdatedDate,
       ISNULL(o.OrgUnitNameTH, o.OrgUnitNameEN) AS OrgUnitName,
       N'Sub Committee' AS OrgUnitTypeName
FROM EMS.m_SubCommittee sc
INNER JOIN EMS.OrgUnit o ON o.OrgUnitID = sc.OrgUnitID
WHERE sc.IsActive = 1 AND sc.IsDeleted = 0 AND o.IsDeleted = 0

UNION ALL

SELECT d.DivisionID AS QuotaConfigID,
       d.DivisionID AS OrgUnitID,
       CAST(NULL AS INT) AS ParentOrgUnitID,
       CAST(ISNULL(d.quota_size, 0) AS BIGINT) AS QuotaBytes,
       CAST(80 AS TINYINT) AS WarnThresholdPct,
       CAST(95 AS TINYINT) AS CriticalThresholdPct,
       CAST(d.IsActive AS BIT) AS IsActive,
       CAST(0 AS INT) AS CreatedBy,
       ISNULL(d.CreatedDate, GETDATE()) AS CreatedDate,
       CAST(NULL AS INT) AS UpdatedBy,
       CAST(NULL AS DATETIME) AS UpdatedDate,
       d.DivisionName AS OrgUnitName,
       N'Division' AS OrgUnitTypeName
FROM EMS.m_Division d
WHERE d.IsActive = 1

UNION ALL

SELECT dep.DepartmentID AS QuotaConfigID,
       dep.DepartmentID AS OrgUnitID,
       dep.DivisionID AS ParentOrgUnitID,
       CAST(ISNULL(dep.quota_size, 0) AS BIGINT) AS QuotaBytes,
       CAST(80 AS TINYINT) AS WarnThresholdPct,
       CAST(95 AS TINYINT) AS CriticalThresholdPct,
       CAST(dep.IsActive AS BIT) AS IsActive,
       CAST(0 AS INT) AS CreatedBy,
       ISNULL(dep.CreatedDate, GETDATE()) AS CreatedDate,
       CAST(NULL AS INT) AS UpdatedBy,
       CAST(NULL AS DATETIME) AS UpdatedDate,
       COALESCE(NULLIF(dep.DepartmentNameTh, ''), NULLIF(dep.DepartmentNameEn, ''), dep.DepartmentName) AS OrgUnitName,
       N'Department' AS OrgUnitTypeName
FROM EMS.m_Department dep
WHERE dep.IsActive = 1";

        public FolderQuotaConfigRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public FolderQuotaConfig GetByOrgUnit(int orgUnitId, int? parentOrgUnitId = null)
        {
            var sql = "SELECT TOP 1 * FROM (" + SelectMasterQuota + ") q WHERE q.OrgUnitID = @OrgUnitID";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    using (var reader = cmd.ExecuteReader())
                        return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public IEnumerable<FolderQuotaConfig> GetAll()
        {
            var results = new List<FolderQuotaConfig>();
            var sql = "SELECT * FROM (" + SelectMasterQuota + ") q ORDER BY q.OrgUnitTypeName, q.OrgUnitName";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) results.Add(Map(reader));
            }
            return results;
        }

        public void Save(FolderQuotaConfig config)
        {
            const string sql = @"
IF @OrgUnitTypeName LIKE '%Sub%'
BEGIN
    UPDATE EMS.m_SubCommittee
    SET quota_size = @QuotaBytes,
        UpdatedBy = CONVERT(NVARCHAR(100), @UpdatedBy),
        UpdatedDate = GETDATE()
    WHERE OrgUnitID = @OrgUnitID;
END
ELSE IF @OrgUnitTypeName LIKE '%Division%'
BEGIN
    UPDATE EMS.m_Division
    SET quota_size = @QuotaBytes
    WHERE DivisionID = @OrgUnitID;
END
ELSE IF @OrgUnitTypeName LIKE '%Department%'
BEGIN
    UPDATE EMS.m_Department
    SET quota_size = @QuotaBytes
    WHERE DepartmentID = @OrgUnitID;
END
ELSE
BEGIN
    RAISERROR('Unsupported OrgUnitType for quota config.', 16, 1);
END";

            ExecuteQuotaUpdate(sql, config.OrgUnitID, config.QuotaBytes, config.UpdatedBy, config.OrgUnitTypeName);
        }

        public void Delete(int quotaConfigId, string orgUnitTypeName)
        {
            const string sql = @"
IF @OrgUnitTypeName LIKE '%Sub%'
    UPDATE EMS.m_SubCommittee SET quota_size = 0 WHERE OrgUnitID = @OrgUnitID;
ELSE IF @OrgUnitTypeName LIKE '%Division%'
    UPDATE EMS.m_Division SET quota_size = 0 WHERE DivisionID = @OrgUnitID;
ELSE IF @OrgUnitTypeName LIKE '%Department%'
    UPDATE EMS.m_Department SET quota_size = 0 WHERE DepartmentID = @OrgUnitID;";

            ExecuteQuotaUpdate(sql, quotaConfigId, 0, null, orgUnitTypeName);
        }

        private void ExecuteQuotaUpdate(string sql, int orgUnitId, long quotaBytes, int? updatedBy, string orgUnitTypeName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    cmd.Parameters.AddWithValue("@QuotaBytes", quotaBytes);
                    cmd.Parameters.AddWithValue("@UpdatedBy", (object)updatedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrgUnitTypeName", (object)orgUnitTypeName ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static FolderQuotaConfig Map(SqlDataReader reader)
        {
            return new FolderQuotaConfig
            {
                QuotaConfigID = Convert.ToInt32(reader["QuotaConfigID"]),
                OrgUnitID = Convert.ToInt32(reader["OrgUnitID"]),
                ParentOrgUnitID = reader["ParentOrgUnitID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ParentOrgUnitID"]),
                QuotaBytes = Convert.ToInt64(reader["QuotaBytes"]),
                WarnThresholdPct = Convert.ToByte(reader["WarnThresholdPct"]),
                CriticalThresholdPct = Convert.ToByte(reader["CriticalThresholdPct"]),
                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                CreatedBy = Convert.ToInt32(reader["CreatedBy"]),
                CreatedDate = reader["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedDate"]),
                UpdatedBy = reader["UpdatedBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["UpdatedBy"]),
                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedDate"]),
                OrgUnitName = reader["OrgUnitName"] == DBNull.Value ? string.Empty : Convert.ToString(reader["OrgUnitName"]),
                OrgUnitTypeName = reader["OrgUnitTypeName"] == DBNull.Value ? string.Empty : Convert.ToString(reader["OrgUnitTypeName"])
            };
        }
    }
}

