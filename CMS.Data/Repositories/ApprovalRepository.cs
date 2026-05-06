using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly string _connectionString;

        public ApprovalRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int InsertRequest(ApprovalRequest request)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "INSERT INTO workflow.ApprovalRequest " +
                    "  (RequestType, FolderID, FileID, PermissionID, RequestedBy, " +
                    "   RequestedDate, RequestStatus, Subject, Remark, ExpiryDate) " +
                    "OUTPUT INSERTED.RequestID " +
                    "VALUES (@RequestType, @FolderID, @FileID, @PermissionID, @RequestedBy, " +
                    "        GETDATE(), 1, @Subject, @Remark, @ExpiryDate)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@RequestType",  request.RequestType);
                    cmd.Parameters.AddWithValue("@FolderID",     (object)request.FolderID     ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileID",       (object)request.FileID       ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PermissionID", (object)request.PermissionID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RequestedBy",  request.RequestedBy);
                    cmd.Parameters.AddWithValue("@Subject",      (object)request.Subject      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remark",       (object)request.Remark       ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpiryDate",   (object)request.ExpiryDate   ?? DBNull.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int InsertStep(ApprovalStep step)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "INSERT INTO workflow.ApprovalStep (RequestID, StepOrder, ApproverID, StepStatus) " +
                    "OUTPUT INSERTED.StepID " +
                    "VALUES (@RequestID, @StepOrder, @ApproverID, @StepStatus)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@RequestID",  step.RequestID);
                    cmd.Parameters.AddWithValue("@StepOrder",  step.StepOrder);
                    cmd.Parameters.AddWithValue("@ApproverID", step.ApproverID);
                    cmd.Parameters.AddWithValue("@StepStatus", (byte)step.StepStatus);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public ApprovalRequest GetRequest(int requestId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("workflow.sp_GetApprovalRequestByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RequestID", requestId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapRequest(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<ApprovalStep> GetPendingSteps(int requestId)
        {
            var results = new List<ApprovalStep>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT StepID, RequestID, StepOrder, ApproverID, StepStatus, " +
                    "       ActionDate, ActionRemark, DelegatedTo, NotifiedDate, EscalatedDate " +
                    "FROM workflow.ApprovalStep " +
                    "WHERE RequestID = @RequestID AND StepStatus IN (1, 4)", conn))  // 1=Pending 4=Delegated
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@RequestID", requestId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapStep(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<ApprovalStep> GetAllSteps(int requestId)
        {
            var results = new List<ApprovalStep>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("workflow.sp_GetAllApprovalSteps", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RequestID", requestId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapStep(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<ApprovalRequest> GetAllRequestsByFolder(int folderId)
        {
            var results = new List<ApprovalRequest>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT RequestID, RequestType, FolderID, FileID, PermissionID, RequestedBy, " +
                    "RequestedDate, RequestStatus, Subject, Remark, ExpiryDate, " +
                    "ResolvedBy, ResolvedDate, ResolveRemark " +
                    "FROM workflow.ApprovalRequest " +
                    "WHERE FolderID = @FolderID " +
                    "ORDER BY RequestedDate DESC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapRequest(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<ApprovalRequest> GetPendingRequests()
        {
            var results = new List<ApprovalRequest>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT RequestID, RequestType, FolderID, FileID, PermissionID, RequestedBy, " +
                    "RequestedDate, RequestStatus, Subject, Remark, ExpiryDate, " +
                    "ResolvedBy, ResolvedDate, ResolveRemark " +
                    "FROM workflow.ApprovalRequest " +
                    "WHERE RequestStatus = 1 " + // 1 = Pending
                    "ORDER BY RequestedDate DESC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapRequest(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<ApprovalRequest> GetExpiredPendingRequests()
        {
            var results = new List<ApprovalRequest>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("workflow.sp_GetExpiredPendingRequests", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapRequest(reader));
                    }
                }
            }
            return results;
        }

        public void UpdateStepStatus(int stepId, int stepStatus, string actionRemark)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("workflow.sp_UpdateApprovalStepStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StepID",       stepId);
                    cmd.Parameters.AddWithValue("@StepStatus",   (byte)stepStatus);
                    cmd.Parameters.AddWithValue("@ActionRemark", (object)actionRemark ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateRequestStatus(int requestId, int requestStatus, int resolvedBy, string resolveRemark)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("workflow.sp_UpdateApprovalRequestStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RequestID",     requestId);
                    cmd.Parameters.AddWithValue("@RequestStatus", (byte)requestStatus);
                    cmd.Parameters.AddWithValue("@ResolvedBy",    resolvedBy);
                    cmd.Parameters.AddWithValue("@ResolveRemark", (object)resolveRemark ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteByFolder(int folderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Delete steps first (child), then requests (parent)
                using (var cmd = new SqlCommand(
                    "DELETE s FROM workflow.ApprovalStep s " +
                    "INNER JOIN workflow.ApprovalRequest r ON s.RequestID = r.RequestID " +
                    "WHERE r.FolderID = @FolderID", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand(
                    "DELETE FROM workflow.ApprovalRequest WHERE FolderID = @FolderID", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeletePendingHCRequestsForOrgUnit(int orgUnitId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // ลบ steps ก่อน (child) แล้วค่อยลบ requests (parent)
                using (var cmd = new SqlCommand(
                    "DELETE s FROM workflow.ApprovalStep s " +
                    "INNER JOIN workflow.ApprovalRequest r ON s.RequestID = r.RequestID " +
                       "WHERE r.RequestType = 'CREATE_HC' AND r.RequestStatus = 1 " +
                       "AND r.Remark LIKE @OrgUnitPattern", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@OrgUnitPattern", "%OrgUnitId=" + orgUnitId + "%");
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand(
                    "DELETE FROM workflow.ApprovalRequest " +
                    "WHERE RequestType = 'CREATE_HC' AND RequestStatus = 1 " +
                           "AND Remark LIKE @OrgUnitPattern", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@OrgUnitPattern", "%OrgUnitId=" + orgUnitId + "%");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static ApprovalRequest MapRequest(SqlDataReader reader)
        {
            return new ApprovalRequest
            {
                RequestID     = (int)reader["RequestID"],
                RequestType   = (string)reader["RequestType"],
                FolderID      = reader["FolderID"] == DBNull.Value ? (int?)null : (int)reader["FolderID"],
                FileID        = reader["FileID"] == DBNull.Value ? (int?)null : (int)reader["FileID"],
                PermissionID  = reader["PermissionID"] == DBNull.Value ? (int?)null : (int)reader["PermissionID"],
                RequestedBy   = (int)reader["RequestedBy"],
                RequestedDate = (DateTime)reader["RequestedDate"],
                RequestStatus = (byte)reader["RequestStatus"],
                Subject       = reader["Subject"] == DBNull.Value ? null : (string)reader["Subject"],
                Remark        = reader["Remark"] == DBNull.Value ? null : (string)reader["Remark"],
                ExpiryDate    = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ExpiryDate"],
                ResolvedBy    = reader["ResolvedBy"] == DBNull.Value ? (int?)null : (int)reader["ResolvedBy"],
                ResolvedDate  = reader["ResolvedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ResolvedDate"],
                ResolveRemark = reader["ResolveRemark"] == DBNull.Value ? null : (string)reader["ResolveRemark"]
            };
        }

        private static ApprovalStep MapStep(SqlDataReader reader)
        {
            return new ApprovalStep
            {
                StepID        = (int)reader["StepID"],
                RequestID     = (int)reader["RequestID"],
                StepOrder     = (int)reader["StepOrder"],
                ApproverID    = (int)reader["ApproverID"],
                StepStatus    = (byte)reader["StepStatus"],
                ActionDate    = reader["ActionDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ActionDate"],
                ActionRemark  = reader["ActionRemark"] == DBNull.Value ? null : (string)reader["ActionRemark"],
                DelegatedTo   = reader["DelegatedTo"] == DBNull.Value ? (int?)null : (int)reader["DelegatedTo"],
                NotifiedDate  = reader["NotifiedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["NotifiedDate"],
                EscalatedDate = reader["EscalatedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["EscalatedDate"]
            };
        }
    }
}
