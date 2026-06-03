using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
                using (var cmd = new SqlCommand("WFE.sp_CreateWFStepInstance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@WorkflowInstanceId", instance.WorkflowInstanceId);
                    cmd.Parameters.AddWithValue("@WorkflowStepId", instance.WorkflowStepId);
                    cmd.Parameters.AddWithValue("@AssignedTo", instance.AssignedTo);
                    cmd.Parameters.AddWithValue("@SLADeadline", instance.SLADeadline);

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
                    "SELECT WFStepInstanceId, WFInstanceId, WFStepId, " +
                    "       AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy, " +
                    "       OutputData, SLADeadline, SLAStatus, IsDeleted " +
                    "FROM   WFE.t_WFStepInstance " +
                    "WHERE  WFStepInstanceId = @WorkflowStepInstanceId " +
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
                    "SELECT wsi.WFStepInstanceId, wsi.WFInstanceId, wsi.WFStepId, " +
                    "       wsi.AssignedTo, wsi.AssignedDate, wsi.Status, wsi.CompletedDate, wsi.CompletedBy, " +
                    "       wsi.OutputData, wsi.SLADeadline, wsi.SLAStatus, wsi.IsDeleted " +
                    "FROM   WFE.t_WFStepInstance wsi " +
                    "INNER JOIN WFE.s_WFStep ws ON ws.WFStepId = wsi.WFStepId " +
                    "WHERE  wsi.WFInstanceId = @WorkflowInstanceId " +
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
                                WorkflowInstanceId = (int)reader["WorkflowInstanceId"],
                                WorkflowStepId = (int)reader["WorkflowStepId"],
                                AssignedTo = (string)reader["AssignedTo"],
                                AssignedDate = (DateTime)reader["AssignedDate"],
                                Status = (WorkflowStepInstanceStatus)(byte)reader["Status"],
                                SLADeadline = (DateTime)reader["SLADeadline"],
                                SLAStatus = (SLAStatus)(byte)reader["SLAStatus"]
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
                    "SELECT WFStepInstanceId, WFInstanceId, WFStepId, " +
                    "       AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy, " +
                    "       OutputData, SLADeadline, SLAStatus, IsDeleted " +
                    "FROM   WFE.t_WFStepInstance " +
                    "WHERE  AssignedTo = @AssignedTo " +
                    "  AND  Status IN (1, 2, 3) " +   // Pending, InProgress or Completed
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
                    "       WFStepInstanceId, WFInstanceId, WFStepId, " +
                    "       AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy, " +
                    "       OutputData, SLADeadline, SLAStatus, IsDeleted " +
                    "FROM   WFE.t_WFStepInstance " +
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
                    "UPDATE WFE.t_WFStepInstance " +
                    "SET    AssignedTo    = @AssignedTo, " +
                    "       Status        = @Status, " +
                    "       CompletedDate = @CompletedDate, " +
                    "       CompletedBy   = @CompletedBy, " +
                    "       OutputData    = @OutputData, " +
                    "       SLADeadline   = @SLADeadline, " +
                    "       SLAStatus     = @SLAStatus " +
                    "WHERE  WFStepInstanceId = @WorkflowStepInstanceId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@WorkflowStepInstanceId", SqlDbType.Int).Value = instance.WorkflowStepInstanceId;
                    cmd.Parameters.Add("@AssignedTo", SqlDbType.NVarChar, 256).Value = (object)instance.AssignedTo ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.TinyInt).Value = (byte)instance.Status;
                    cmd.Parameters.Add("@CompletedDate", SqlDbType.DateTime).Value = (object)instance.CompletedDate ?? DBNull.Value;
                    cmd.Parameters.Add("@CompletedBy", SqlDbType.NVarChar, 256).Value = (object)instance.CompletedBy ?? DBNull.Value;
                    cmd.Parameters.Add("@OutputData", SqlDbType.NVarChar, -1).Value = (object)instance.OutputData ?? DBNull.Value;
                    cmd.Parameters.Add("@SLADeadline", SqlDbType.DateTime).Value =
                        instance.SLADeadline == DateTime.MinValue
                            ? (object)DBNull.Value
                            : instance.SLADeadline;
                    cmd.Parameters.Add("@SLAStatus", SqlDbType.TinyInt).Value = (byte)instance.SLAStatus;
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
                    "UPDATE WFE.t_WFStepInstance " +
                    "SET    IsDeleted = 1 " +
                    "WHERE  WFStepInstanceId = @WorkflowStepInstanceId " +
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
            // tolerant mapping: some DB schemas use different column names (WFStepInstanceId, WFInstanceId, DueDate, etc.)
            T GetValue<T>(string[] keys, Func<object, T> convert, T defaultValue = default(T))
            {
                foreach (var k in keys)
                {
                    try
                    {
                        var val = reader[k];
                        if (val == DBNull.Value) return defaultValue;
                        return convert(val);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        continue; // column not in result set — try next alias
                    }
                    catch (FormatException)
                    {
                        return defaultValue; // column exists but value can't be converted (wrong DB type)
                    }
                    catch (InvalidCastException)
                    {
                        return defaultValue;
                    }
                }
                return defaultValue;
            }

            return new WorkflowStepInstance
            {
                WorkflowStepInstanceId = GetValue<int>(new[] { "WorkflowStepInstanceId", "WFStepInstanceId", "WFStepInstanceID" }, v => Convert.ToInt32(v)),
                WorkflowInstanceId = GetValue<int>(new[] { "WorkflowInstanceId", "WFInstanceId", "WorkflowInstanceID" }, v => Convert.ToInt32(v)),
                WorkflowStepId = GetValue<int>(new[] { "WorkflowStepId", "WFStepId", "WorkflowStepID" }, v => Convert.ToInt32(v)),
                AssignedTo = GetValue<string>(new[] { "AssignedTo", "AssignedUser", "Assigned" }, v => v.ToString(), null),
                AssignedDate = GetValue<DateTime>(new[] { "AssignedDate", "AssignedOn", "StartedDate" }, v => Convert.ToDateTime(v), DateTime.MinValue),
                Status = GetValue<WorkflowStepInstanceStatus>(new[] { "Status" }, v => (WorkflowStepInstanceStatus)Convert.ToByte(v), WorkflowStepInstanceStatus.Pending),
                CompletedDate = GetValue<DateTime?>(new[] { "CompletedDate", "CompletedOn", "FinishedDate" }, v => (DateTime?)Convert.ToDateTime(v), null),
                CompletedBy = GetValue<string>(new[] { "CompletedBy", "CompletedByAD", "CompletedUser" }, v => v.ToString(), null),
                OutputData = GetValue<string>(new[] { "OutputData", "Output", "Payload" }, v => v.ToString(), null),
                SLADeadline = GetValue<DateTime>(new[] { "SLADeadline", "DueDate", "Deadline" }, v => Convert.ToDateTime(v), DateTime.MinValue),
                SLAStatus = GetValue<SLAStatus>(new[] { "SLAStatus", "SLAState" }, v => (SLAStatus)Convert.ToByte(v), SLAStatus.OnTime),
                IsDeleted = GetValue<bool>(new[] { "IsDeleted", "Deleted" }, v => Convert.ToBoolean(v), false)
            };
        }
    }
}
