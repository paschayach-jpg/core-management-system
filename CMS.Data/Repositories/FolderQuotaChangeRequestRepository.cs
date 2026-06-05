using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class FolderQuotaChangeRequestRepository : IFolderQuotaChangeRequestRepository
    {
        private readonly string _connectionString;

        private const string _selectCols =
            @"SELECT r.RequestID, r.OrgUnitID, r.CurrentQuotaBytes, r.RequestedQuotaBytes,
                     r.Reason, r.Status, r.RequestedBy, r.RequestedDate,
                     r.ReviewedBy, r.ReviewedDate, r.ReviewRemark,
                     ISNULL(o.OrgUnitNameTH, o.OrgUnitNameEN) AS OrgUnitName,
                     ISNULL(ua1.DisplayName, ua1.ADUserName)  AS RequestedByName,
                     ISNULL(ua2.DisplayName, ua2.ADUserName)  AS ReviewedByName
              FROM   dms.FolderQuotaChangeRequest r
              INNER JOIN EMS.OrgUnit     o   ON o.OrgUnitID     = r.OrgUnitID
              INNER JOIN EMS.UserAccount ua1 ON ua1.UserID      = r.RequestedBy
              LEFT  JOIN EMS.UserAccount ua2 ON ua2.UserID      = r.ReviewedBy";

        public FolderQuotaChangeRequestRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(FolderQuotaChangeRequest req)
        {
            const string sql =
                @"INSERT INTO dms.FolderQuotaChangeRequest
                         (OrgUnitID, CurrentQuotaBytes, RequestedQuotaBytes, Reason, Status, RequestedBy, RequestedDate)
                  VALUES (@OrgUnitID, @CurrentQuotaBytes, @RequestedQuotaBytes, @Reason, @Status, @RequestedBy, GETDATE());
                  SELECT SCOPE_IDENTITY();";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitID",           req.OrgUnitID);
                    cmd.Parameters.AddWithValue("@CurrentQuotaBytes",   req.CurrentQuotaBytes);
                    cmd.Parameters.AddWithValue("@RequestedQuotaBytes", req.RequestedQuotaBytes);
                    cmd.Parameters.AddWithValue("@Reason",              (object)req.Reason ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status",              (byte)QuotaRequestStatus.Pending);
                    cmd.Parameters.AddWithValue("@RequestedBy",         req.RequestedBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public FolderQuotaChangeRequest GetById(int requestId)
        {
            var sql = _selectCols + " WHERE r.RequestID = @RequestID";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RequestID", requestId);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? Map(r) : null;
                }
            }
        }

        public IEnumerable<FolderQuotaChangeRequest> GetByOrgUnit(int orgUnitId)
        {
            return Query(_selectCols + " WHERE r.OrgUnitID = @OrgUnitID ORDER BY r.RequestedDate DESC",
                cmd => cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId));
        }

        public IEnumerable<FolderQuotaChangeRequest> GetPending()
        {
            return Query(_selectCols + " WHERE r.Status = 1 ORDER BY r.RequestedDate",
                null);
        }

        public IEnumerable<FolderQuotaChangeRequest> GetAll()
        {
            return Query(_selectCols + " ORDER BY r.RequestedDate DESC", null);
        }

        public void UpdateStatus(int requestId, QuotaRequestStatus status, int reviewedBy, string remark)
        {
            const string sql =
                @"UPDATE dms.FolderQuotaChangeRequest
                  SET Status = @Status, ReviewedBy = @ReviewedBy,
                      ReviewedDate = GETDATE(), ReviewRemark = @Remark
                  WHERE RequestID = @RequestID";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RequestID",  requestId);
                    cmd.Parameters.AddWithValue("@Status",     (byte)status);
                    cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
                    cmd.Parameters.AddWithValue("@Remark",     (object)remark ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private IEnumerable<FolderQuotaChangeRequest> Query(string sql, Action<SqlCommand> addParams)
        {
            var results = new List<FolderQuotaChangeRequest>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (addParams != null) addParams(cmd);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) results.Add(Map(r));
                }
            }
            return results;
        }

        private static FolderQuotaChangeRequest Map(SqlDataReader r)
        {
            return new FolderQuotaChangeRequest
            {
                RequestID           = (int)r["RequestID"],
                OrgUnitID           = (int)r["OrgUnitID"],
                CurrentQuotaBytes   = (long)r["CurrentQuotaBytes"],
                RequestedQuotaBytes = (long)r["RequestedQuotaBytes"],
                Reason              = r["Reason"]        == DBNull.Value ? null  : (string)r["Reason"],
                Status              = (QuotaRequestStatus)(byte)r["Status"],
                RequestedBy         = (int)r["RequestedBy"],
                RequestedDate       = (DateTime)r["RequestedDate"],
                ReviewedBy          = r["ReviewedBy"]   == DBNull.Value ? (int?)null     : (int)r["ReviewedBy"],
                ReviewedDate        = r["ReviewedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)r["ReviewedDate"],
                ReviewRemark        = r["ReviewRemark"] == DBNull.Value ? null  : (string)r["ReviewRemark"],
                OrgUnitName         = r["OrgUnitName"]      == DBNull.Value ? "" : Convert.ToString(r["OrgUnitName"]),
                RequestedByName     = r["RequestedByName"]  == DBNull.Value ? "" : Convert.ToString(r["RequestedByName"]),
                ReviewedByName      = r["ReviewedByName"]   == DBNull.Value ? "" : Convert.ToString(r["ReviewedByName"])
            };
        }
    }
}

