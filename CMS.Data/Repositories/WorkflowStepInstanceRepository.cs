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
    /// <summary>
    /// ADO.NET implementation of IWorkflowStepInstanceRepository.
    /// Step instances are created via stored procedure; reads use inline SQL.
    /// </summary>
    public class WorkflowStepInstanceRepository : IWorkflowStepInstanceRepository
    {
        private readonly string _connectionString;

        public WorkflowStepInstanceRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        // ----------------------------------------------------------------
        // Create
        // ----------------------------------------------------------------

        public int CreateStepInstance(WorkflowStepInstance instance)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_WF_CreateStepInstance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@WorkflowInstanceId", instance.WorkflowInstanceId);
                    cmd.Parameters.AddWithValue("@WorkflowStepId",     instance.WorkflowStepId);
                    cmd.Parameters.AddWithValue("@AssignedTo",         instance.AssignedTo);
                    cmd.Parameters.AddWithValue("@SLADeadline",        instance.SLADeadline);

                    var outParam = new SqlParameter("@NewWorkflowStepInstanceId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outParam);
                    cmd.ExecuteNonQuery();
                    return (int)outParam.Value;
                }
            }
        }

        // ----------------------------------------------------------------
        // Read
        // ----------------------------------------------------------------

        public WorkflowStepInstance GetStepInstance(int workflowStepInstanceId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WorkflowStepInstanceId, WorkflowInstanceId, WorkflowStepId, " +
                    "       AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy, " +
                    "       OutputData, SLADeadline, SLAStatus, IsDeleted " +
                    "FROM   dbo.WorkflowStepInstance " +
                    "WHERE  WorkflowStepInstanceId = @WorkflowStepInstanceId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowStepInstanceId", workflowStepInstanceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapStepInstance(reader) : null;
                    }
                }
            }
        }

        public IList<WorkflowStepInstance> GetStepInstancesByWorkflow(int workflowInstanceId)
        {
            var results = new List<WorkflowStepInstance>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT wsi.WorkflowStepInstanceId, wsi.WorkflowInstanceId, wsi.WorkflowStepId, " +
                    "       wsi.AssignedTo, wsi.AssignedDate, wsi.Status, wsi.CompletedDate, wsi.CompletedBy, " +
                    "       wsi.OutputData, wsi.SLADeadline, wsi.SLAStatus, wsi.IsDeleted " +
                    "FROM   dbo.WorkflowStepInstance wsi " +
                    "INNER JOIN dbo.WorkflowStep ws ON ws.WorkflowStepId = wsi.WorkflowStepId " +
                    "WHERE  wsi.WorkflowInstanceId = @WorkflowInstanceId " +
                    "  AND  wsi.IsDeleted = 0 " +
                    "ORDER BY ws.SequenceNumber ASC, wsi.AssignedDate ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowInstanceId", workflowInstanceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapStepInstance(reader));
                    }
                }
            }
            return results;
        }

        public IList<WorkflowStepInstance> GetPendingStepInstances()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_WF_GetPendingSteps", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // No look-ahead filter — return all pending steps
                    cmd.Parameters.AddWithValue("@LookAheadHours", DBNull.Value);

                    var results = new List<WorkflowStepInstance>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // sp_WF_GetPendingSteps returns a superset of columns;
                            // map only the WorkflowStepInstance fields here.
                            results.Add(new WorkflowStepInstance
                            {
                                WorkflowStepInstanceId = (int)reader["WorkflowStepInstanceId"],
                                WorkflowInstanceId     = (int)reader["WorkflowInstanceId"],
                                WorkflowStepId         = (int)reader["WorkflowStepId"],
                                AssignedTo             = (string)reader["AssignedTo"],
                                AssignedDate           = (DateTime)reader["AssignedDate"],
                                Status                 = (WorkflowStepInstanceStatus)(byte)reader["Status"],
                                SLADeadline            = (DateTime)reader["SLADeadline"],
                                SLAStatus              = (SLAStatus)(byte)reader["SLAStatus"]
                            });
                        }
                    }
                    return results;
                }
            }
        }

        public IList<WorkflowStepInstance> GetStepInstancesByUser(string adUserName)
        {
            var results = new List<WorkflowStepInstance>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WorkflowStepInstanceId, WorkflowInstanceId, WorkflowStepId, " +
                    "       AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy, " +
                    "       OutputData, SLADeadline, SLAStatus, IsDeleted " +
                    "FROM   dbo.WorkflowStepInstance " +
                    "WHERE  AssignedTo = @AssignedTo " +
                    "  AND  Status IN (1, 2) " +   // Pending or InProgress
                    "  AND  IsDeleted = 0 " +
                    "ORDER BY SLADeadline ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@AssignedTo", adUserName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapStepInstance(reader));
                    }
                }
            }
            return results;
        }

        public WorkflowStepInstance GetByExternalReference(int externalReferenceId, string externalReferenceType)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // ExternalReferenceId / ExternalReferenceType are transient properties
                // stored in OutputData JSON; this query is a placeholder for when
                // a dedicated column is added in a future migration.
                // For now, scan OutputData for the reference (acceptable for low-volume callbacks).
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 " +
                    "       WorkflowStepInstanceId, WorkflowInstanceId, WorkflowStepId, " +
                    "       AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy, " +
                    "       OutputData, SLADeadline, SLAStatus, IsDeleted " +
                    "FROM   dbo.WorkflowStepInstance " +
                    "WHERE  IsDeleted = 0 " +
                    "  AND  OutputData LIKE @RefPattern", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    // Match JSON fragment: "ExternalReferenceId":123,"ExternalReferenceType":"ApprovalRequest"
                    cmd.Parameters.AddWithValue("@RefPattern",
                        "%\"ExternalReferenceId\":" + externalReferenceId +
                        "%\"ExternalReferenceType\":\"" + externalReferenceType + "\"%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapStepInstance(reader) : null;
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // Update
        // ----------------------------------------------------------------

        public void UpdateStepInstance(WorkflowStepInstance instance)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE dbo.WorkflowStepInstance " +
                    "SET    AssignedTo    = @AssignedTo, " +
                    "       Status        = @Status, " +
                    "       CompletedDate = @CompletedDate, " +
                    "       CompletedBy   = @CompletedBy, " +
                    "       OutputData    = @OutputData, " +
                    "       SLADeadline   = @SLADeadline, " +
                    "       SLAStatus     = @SLAStatus " +
                    "WHERE  WorkflowStepInstanceId = @WorkflowStepInstanceId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowStepInstanceId", instance.WorkflowStepInstanceId);
                    cmd.Parameters.AddWithValue("@AssignedTo",    instance.AssignedTo);
                    cmd.Parameters.AddWithValue("@Status",        (byte)instance.Status);
                    cmd.Parameters.AddWithValue("@CompletedDate", (object)instance.CompletedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompletedBy",   (object)instance.CompletedBy   ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OutputData",    (object)instance.OutputData    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SLADeadline",   instance.SLADeadline);
                    cmd.Parameters.AddWithValue("@SLAStatus",     (byte)instance.SLAStatus);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Delete (soft)
        // ----------------------------------------------------------------

        public void SoftDeleteStepInstance(int workflowStepInstanceId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE dbo.WorkflowStepInstance " +
                    "SET    IsDeleted = 1 " +
                    "WHERE  WorkflowStepInstanceId = @WorkflowStepInstanceId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowStepInstanceId", workflowStepInstanceId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Mapping
        // ----------------------------------------------------------------

        private static WorkflowStepInstance MapStepInstance(SqlDataReader reader)
        {
            return new WorkflowStepInstance
            {
                WorkflowStepInstanceId = (int)reader["WorkflowStepInstanceId"],
                WorkflowInstanceId     = (int)reader["WorkflowInstanceId"],
                WorkflowStepId         = (int)reader["WorkflowStepId"],
                AssignedTo             = (string)reader["AssignedTo"],
                AssignedDate           = (DateTime)reader["AssignedDate"],
                Status                 = (WorkflowStepInstanceStatus)(byte)reader["Status"],
                CompletedDate          = reader["CompletedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["CompletedDate"],
                CompletedBy            = reader["CompletedBy"] == DBNull.Value ? null : (string)reader["CompletedBy"],
                OutputData             = reader["OutputData"] == DBNull.Value ? null : (string)reader["OutputData"],
                SLADeadline            = (DateTime)reader["SLADeadline"],
                SLAStatus              = (SLAStatus)(byte)reader["SLAStatus"],
                IsDeleted              = (bool)reader["IsDeleted"]
            };
        }
    }
}
