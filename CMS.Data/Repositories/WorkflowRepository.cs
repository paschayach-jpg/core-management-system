using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Data.Models;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    /// <summary>
    /// ADO.NET implementation of IWorkflowRepository.
    /// All mutations go through stored procedures; reads use inline SQL for flexibility.
    /// </summary>
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly string _connectionString;

        public WorkflowRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        // ----------------------------------------------------------------
        // Create
        // ----------------------------------------------------------------

        public int CreateWorkflowInstance(WorkflowInstance instance)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("WFE.sp_CreateWFInstance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@WFDefinitionId", instance.WorkflowDefinitionId);
                    cmd.Parameters.AddWithValue("@TemplateVersion",      instance.TemplateVersion);
                    cmd.Parameters.AddWithValue("@InitiatedBy",          instance.InitiatedBy);
                    cmd.Parameters.AddWithValue("@InputParameters",      (object)instance.InputParameters ?? DBNull.Value);

                    var outParam = new SqlParameter("@NewWFInstanceId", SqlDbType.Int)
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

        public WorkflowInstance GetWorkflowInstance(int workflowInstanceId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WFInstanceId as WorkflowInstanceId, WFDefinitionId as WorkflowDefinitionId, TemplateVersion, " +
                    "       InitiatedBy, InitiatedDate, CurrentStepId, Status, " +
                    "       CompletedDate, InputParameters, IsDeleted, DeletedDate " +
                    "FROM   WFE.t_WFInstance " +
                    "WHERE  WFInstanceId = @WorkflowInstanceId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowInstanceId", workflowInstanceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapInstance(reader) : null;
                    }
                }
            }
        }

        public PagedResult<WorkflowInstance> GetWorkflowInstances(WorkflowInstanceFilter filter)
        {
            var items      = new List<WorkflowInstance>();
            int totalCount = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Build dynamic WHERE clause
                var where = new System.Text.StringBuilder("WHERE IsDeleted = 0");
                if (filter.WorkflowDefinitionId.HasValue)
                    where.Append(" AND WFDefinitionId = @WorkflowDefinitionId");
                if (!string.IsNullOrEmpty(filter.InitiatedBy))
                    where.Append(" AND InitiatedBy = @InitiatedBy");
                if (filter.Status.HasValue)
                    where.Append(" AND Status = @Status");
                if (filter.FromDate.HasValue)
                    where.Append(" AND InitiatedDate >= @FromDate");
                if (filter.ToDate.HasValue)
                    where.Append(" AND InitiatedDate <= @ToDate");

                int offset = (filter.PageNumber - 1) * filter.PageSize;

                string sql =
                    "SELECT COUNT(*) OVER () AS TotalCount, " +
                    "       WFInstanceId as WorkflowInstanceId, WFDefinitionId as WorkflowDefinitionId, TemplateVersion, " +
                    "       InitiatedBy, InitiatedDate, CurrentStepId, Status, " +
                    "       CompletedDate, InputParameters, IsDeleted, DeletedDate " +
                    "FROM   WFE.t_WFInstance " +
                    where +
                    " ORDER BY InitiatedDate DESC " +
                    " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    if (filter.WorkflowDefinitionId.HasValue)
                        cmd.Parameters.AddWithValue("@WorkflowDefinitionId", filter.WorkflowDefinitionId.Value);
                    if (!string.IsNullOrEmpty(filter.InitiatedBy))
                        cmd.Parameters.AddWithValue("@InitiatedBy", filter.InitiatedBy);
                    if (filter.Status.HasValue)
                        cmd.Parameters.AddWithValue("@Status", filter.Status.Value);
                    if (filter.FromDate.HasValue)
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Value);
                    if (filter.ToDate.HasValue)
                        cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Value);
                    cmd.Parameters.AddWithValue("@Offset",   offset);
                    cmd.Parameters.AddWithValue("@PageSize", filter.PageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (totalCount == 0)
                                totalCount = (int)reader["TotalCount"];
                            items.Add(MapInstance(reader));
                        }
                    }
                }
            }

            return new PagedResult<WorkflowInstance>
            {
                Items       = items,
                TotalCount  = totalCount,
                PageNumber  = filter.PageNumber,
                PageSize    = filter.PageSize
            };
        }

        public IList<WorkflowInstance> GetActiveWorkflowInstances()
        {
            var results = new List<WorkflowInstance>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WFInstanceId as WorkflowInstanceId, WFDefinitionId as WorkflowDefinitionId, TemplateVersion, " +
                    "       InitiatedBy, InitiatedDate, CurrentStepId, Status, " +
                    "       CompletedDate, InputParameters, IsDeleted, DeletedDate " +
                    "FROM   WFE.t_WFInstance " +
                    "WHERE  IsDeleted = 0 " +
                    "  AND  Status NOT IN (3, 4) " +   // exclude Completed and Rejected
                    "ORDER BY InitiatedDate ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapInstance(reader));
                    }
                }
            }
            return results;
        }

        // ----------------------------------------------------------------
        // Update
        // ----------------------------------------------------------------

        public void UpdateWorkflowInstance(WorkflowInstance instance)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE WFE.t_WFInstance " +
                    "SET    Status = @Status, " +
                    "       CurrentStepId = @CurrentStepId, " +
                    "       CompletedDate = @CompletedDate " +
                    "WHERE  WFInstanceId = @WorkflowInstanceId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@WorkflowInstanceId", SqlDbType.Int).Value = instance.WorkflowInstanceId;
                    cmd.Parameters.Add("@Status", SqlDbType.TinyInt).Value = (byte)instance.Status;
                    cmd.Parameters.Add("@CurrentStepId", SqlDbType.Int).Value = (object)instance.CurrentStepId ?? DBNull.Value;
                    cmd.Parameters.Add("@CompletedDate", SqlDbType.DateTime).Value = (object)instance.CompletedDate ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Delete (soft)
        // ----------------------------------------------------------------

        public void SoftDeleteWorkflowInstance(int workflowInstanceId, string deletedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE WFE.t_WFInstance " +
                    "SET    IsDeleted = 1, DeletedDate = GETUTCDATE() " +
                    "WHERE  WFInstanceId = @WorkflowInstanceId " +
                    "  AND  Status IN (3, 4) " +   // only terminal instances
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowInstanceId", workflowInstanceId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Mapping
        // ----------------------------------------------------------------

        private static WorkflowInstance MapInstance(SqlDataReader reader)
        {
            return new WorkflowInstance
            {
                WorkflowInstanceId   = (int)reader["WorkflowInstanceId"],
                WorkflowDefinitionId = (int)reader["WorkflowDefinitionId"],
                TemplateVersion      = (int)reader["TemplateVersion"],
                InitiatedBy          = (string)reader["InitiatedBy"],
                InitiatedDate        = (DateTime)reader["InitiatedDate"],
                CurrentStepId        = reader["CurrentStepId"] == DBNull.Value ? (int?)null : (int)reader["CurrentStepId"],
                Status               = (WorkflowInstanceStatus)(byte)reader["Status"],
                CompletedDate        = reader["CompletedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["CompletedDate"],
                InputParameters      = reader["InputParameters"] == DBNull.Value ? null : (string)reader["InputParameters"],
                IsDeleted            = (bool)reader["IsDeleted"],
                DeletedDate          = reader["DeletedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DeletedDate"]
            };
        }
    }
}
