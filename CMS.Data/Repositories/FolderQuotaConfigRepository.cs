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

        private const string _selectCols =
            @"SELECT q.QuotaConfigID, q.OrgUnitID, q.ParentOrgUnitID,
                     q.QuotaBytes, q.WarnThresholdPct, q.CriticalThresholdPct,
                     q.IsActive, q.CreatedBy, q.CreatedDate, q.UpdatedBy, q.UpdatedDate,
                     ISNULL(o.OrgUnitNameTH, o.OrgUnitNameEN) AS OrgUnitName,
                     CASE o.OrgUnitType WHEN 1 THEN N'Sub Committee'
                                        WHEN 2 THEN N'Division'
                                        WHEN 3 THEN N'Department' END AS OrgUnitTypeName
              FROM   dms.FolderQuotaConfig q
              INNER JOIN core.OrgUnit o ON o.OrgUnitID = q.OrgUnitID
              WHERE  q.IsActive = 1";

        public FolderQuotaConfigRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public FolderQuotaConfig GetByOrgUnit(int orgUnitId, int? parentOrgUnitId = null)
        {
            // เลือก config ที่ตรง parentOrgUnitId ก่อน ถ้าไม่มีค่อย fallback หา null
            var sql = _selectCols +
                @" AND q.OrgUnitID = @OrgUnitID
                   ORDER BY CASE WHEN q.ParentOrgUnitID = @ParentOrgUnitID THEN 0
                                 WHEN q.ParentOrgUnitID IS NULL            THEN 1
                                 ELSE 2 END";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitID",       orgUnitId);
                    cmd.Parameters.AddWithValue("@ParentOrgUnitID", (object)parentOrgUnitId ?? DBNull.Value);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? Map(r) : null;
                }
            }
        }

        public IEnumerable<FolderQuotaConfig> GetAll()
        {
            var results = new List<FolderQuotaConfig>();
            var sql = _selectCols + " ORDER BY o.OrgUnitType, o.OrgUnitNameTH";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) results.Add(Map(r));
            }
            return results;
        }

        public void Save(FolderQuotaConfig config)
        {
            const string sql =
                @"IF EXISTS (SELECT 1 FROM dms.FolderQuotaConfig
                             WHERE OrgUnitID = @OrgUnitID
                               AND (ParentOrgUnitID = @ParentOrgUnitID
                                    OR (ParentOrgUnitID IS NULL AND @ParentOrgUnitID IS NULL)))
                  BEGIN
                      UPDATE dms.FolderQuotaConfig
                      SET    QuotaBytes           = @QuotaBytes,
                             WarnThresholdPct     = @WarnThresholdPct,
                             CriticalThresholdPct = @CriticalThresholdPct,
                             UpdatedBy            = @UpdatedBy,
                             UpdatedDate          = GETDATE()
                      WHERE  OrgUnitID = @OrgUnitID
                        AND  (ParentOrgUnitID = @ParentOrgUnitID
                              OR (ParentOrgUnitID IS NULL AND @ParentOrgUnitID IS NULL))
                  END
                  ELSE
                  BEGIN
                      INSERT INTO dms.FolderQuotaConfig
                             (OrgUnitID, ParentOrgUnitID, QuotaBytes, WarnThresholdPct,
                              CriticalThresholdPct, IsActive, CreatedBy, CreatedDate)
                      VALUES (@OrgUnitID, @ParentOrgUnitID, @QuotaBytes, @WarnThresholdPct,
                              @CriticalThresholdPct, 1, @CreatedBy, GETDATE())
                  END";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitID",           config.OrgUnitID);
                    cmd.Parameters.AddWithValue("@ParentOrgUnitID",     (object)config.ParentOrgUnitID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@QuotaBytes",          config.QuotaBytes);
                    cmd.Parameters.AddWithValue("@WarnThresholdPct",    config.WarnThresholdPct);
                    cmd.Parameters.AddWithValue("@CriticalThresholdPct",config.CriticalThresholdPct);
                    cmd.Parameters.AddWithValue("@CreatedBy",           config.CreatedBy);
                    cmd.Parameters.AddWithValue("@UpdatedBy",           (object)config.UpdatedBy ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int quotaConfigId)
        {
            const string sql = "UPDATE dms.FolderQuotaConfig SET IsActive = 0 WHERE QuotaConfigID = @ID";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", quotaConfigId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static FolderQuotaConfig Map(SqlDataReader r)
        {
            return new FolderQuotaConfig
            {
                QuotaConfigID       = (int)r["QuotaConfigID"],
                OrgUnitID           = (int)r["OrgUnitID"],
                ParentOrgUnitID     = r["ParentOrgUnitID"] == DBNull.Value ? (int?)null : (int)r["ParentOrgUnitID"],
                QuotaBytes          = (long)r["QuotaBytes"],
                WarnThresholdPct    = (byte)r["WarnThresholdPct"],
                CriticalThresholdPct= (byte)r["CriticalThresholdPct"],
                IsActive            = (bool)r["IsActive"],
                CreatedBy           = (int)r["CreatedBy"],
                CreatedDate         = (DateTime)r["CreatedDate"],
                UpdatedBy           = r["UpdatedBy"]   == DBNull.Value ? (int?)null     : (int)r["UpdatedBy"],
                UpdatedDate         = r["UpdatedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["UpdatedDate"],
                OrgUnitName         = r["OrgUnitName"]      == DBNull.Value ? "" : Convert.ToString(r["OrgUnitName"]),
                OrgUnitTypeName     = r["OrgUnitTypeName"]  == DBNull.Value ? "" : Convert.ToString(r["OrgUnitTypeName"])
            };
        }
    }
}
