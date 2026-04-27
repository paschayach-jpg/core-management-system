using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Data.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    /// <summary>
    /// ADO.NET implementation of IWFReportRepository.
    /// All queries delegate to the reporting stored procedures created in script 080.
    /// </summary>
    public class WFReportRepository : IWFReportRepository
    {
        private readonly string _connectionString;

        public WFReportRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        // ----------------------------------------------------------------
        // SLA Compliance Metrics
        // ----------------------------------------------------------------

        public SLAComplianceMetrics GetSLAComplianceMetrics(WorkflowReportFilter filter)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_WF_GetSLAComplianceMetrics", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AddFilterParameters(cmd, filter);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SLAComplianceMetrics
                            {
                                TotalSteps         = (int)reader["TotalSteps"],
                                OnTimeCount        = (int)reader["OnTimeCount"],
                                AtRiskCount        = (int)reader["AtRiskCount"],
                                OverdueCount       = (int)reader["OverdueCount"],
                                OnTimePercent      = reader["OnTimePercent"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["OnTimePercent"]),
                                AvgCompletionHours = reader["AvgCompletionHours"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["AvgCompletionHours"])
                            };
                        }
                        return new SLAComplianceMetrics();
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // Task Distribution
        // ----------------------------------------------------------------

        public PagedResult<TaskDistributionRow> GetTaskDistribution(WorkflowReportFilter filter)
        {
            var items = new List<TaskDistributionRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_WF_GetTaskDistribution", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AddFilterParameters(cmd, filter);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new TaskDistributionRow
                            {
                                AssignedTo    = (string)reader["AssignedTo"],
                                TotalAssigned = (int)reader["TotalAssigned"],
                                Completed     = (int)reader["Completed"],
                                Pending       = (int)reader["Pending"],
                                Rejected      = (int)reader["Rejected"],
                                Overdue       = (int)reader["Overdue"]
                            });
                        }
                    }
                }
            }

            // sp_WF_GetTaskDistribution returns all rows; apply paging in memory
            int total  = items.Count;
            int offset = (filter.PageNumber - 1) * filter.PageSize;
            var page   = items.GetRange(
                Math.Min(offset, total),
                Math.Min(filter.PageSize, Math.Max(0, total - offset)));

            return new PagedResult<TaskDistributionRow>
            {
                Items      = page,
                TotalCount = total,
                PageNumber = filter.PageNumber,
                PageSize   = filter.PageSize
            };
        }

        // ----------------------------------------------------------------
        // Bottleneck Analysis
        // ----------------------------------------------------------------

        public PagedResult<BottleneckRow> GetBottleneckAnalysis(WorkflowReportFilter filter)
        {
            var items = new List<BottleneckRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_WF_GetBottleneckAnalysis", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AddFilterParameters(cmd, filter);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new BottleneckRow
                            {
                                WorkflowStepId         = (int)reader["WorkflowStepId"],
                                StepName               = (string)reader["StepName"],
                                StepType               = (int)(byte)reader["StepType"],
                                SequenceNumber         = (int)reader["SequenceNumber"],
                                WorkflowDefinitionName = (string)reader["WorkflowDefinitionName"],
                                TotalExecutions        = (int)reader["TotalExecutions"],
                                CompletedCount         = (int)reader["CompletedCount"],
                                OverdueCount           = (int)reader["OverdueCount"],
                                AvgCompletionHours     = reader["AvgCompletionHours"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["AvgCompletionHours"]),
                                MaxCompletionHours     = reader["MaxCompletionHours"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["MaxCompletionHours"])
                            });
                        }
                    }
                }
            }

            int total  = items.Count;
            int offset = (filter.PageNumber - 1) * filter.PageSize;
            var page   = items.GetRange(
                Math.Min(offset, total),
                Math.Min(filter.PageSize, Math.Max(0, total - offset)));

            return new PagedResult<BottleneckRow>
            {
                Items      = page,
                TotalCount = total,
                PageNumber = filter.PageNumber,
                PageSize   = filter.PageSize
            };
        }

        // ----------------------------------------------------------------
        // Escalation Statistics
        // ----------------------------------------------------------------

        public EscalationStatistics GetEscalationStatistics(WorkflowReportFilter filter)
        {
            // Derived from SLA compliance data — count overdue steps grouped by escalation action
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT " +
                    "    COUNT(*)                                                              AS TotalEscalations, " +
                    "    SUM(CASE WHEN sla.EscalationAction = 1 THEN 1 ELSE 0 END)            AS NotifyManagerCount, " +
                    "    SUM(CASE WHEN sla.EscalationAction = 2 THEN 1 ELSE 0 END)            AS IncreasePriorityCount, " +
                    "    SUM(CASE WHEN sla.EscalationAction = 3 THEN 1 ELSE 0 END)            AS ReassignCount, " +
                    "    SUM(CASE WHEN sla.EscalationAction = 4 THEN 1 ELSE 0 END)            AS AutoRejectCount " +
                    "FROM dbo.WorkflowStepInstance wsi " +
                    "INNER JOIN dbo.WorkflowInstance wi ON wi.WorkflowInstanceId = wsi.WorkflowInstanceId " +
                    "LEFT  JOIN dbo.WFSLARule sla ON sla.WorkflowStepId = wsi.WorkflowStepId AND sla.IsActive = 1 " +
                    "WHERE wsi.SLAStatus = 3 " +   // Overdue
                    "  AND wsi.IsDeleted = 0 " +
                    "  AND wi.IsDeleted  = 0 " +
                    "  AND (@WorkflowDefinitionId IS NULL OR wi.WorkflowDefinitionId = @WorkflowDefinitionId) " +
                    "  AND (@FromDate IS NULL OR wsi.AssignedDate >= @FromDate) " +
                    "  AND (@ToDate   IS NULL OR wsi.AssignedDate <= @ToDate)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowDefinitionId",
                        filter.WorkflowDefinitionId.HasValue ? (object)filter.WorkflowDefinitionId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate",
                        filter.FromDate.HasValue ? (object)filter.FromDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate",
                        filter.ToDate.HasValue ? (object)filter.ToDate.Value : DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new EscalationStatistics
                            {
                                TotalEscalations      = (int)reader["TotalEscalations"],
                                NotifyManagerCount    = (int)reader["NotifyManagerCount"],
                                IncreasePriorityCount = (int)reader["IncreasePriorityCount"],
                                ReassignCount         = (int)reader["ReassignCount"],
                                AutoRejectCount       = (int)reader["AutoRejectCount"]
                            };
                        }
                        return new EscalationStatistics();
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        private static void AddFilterParameters(SqlCommand cmd, WorkflowReportFilter filter)
        {
            cmd.Parameters.AddWithValue("@WorkflowDefinitionId",
                filter.WorkflowDefinitionId.HasValue ? (object)filter.WorkflowDefinitionId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@FromDate",
                filter.FromDate.HasValue ? (object)filter.FromDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@ToDate",
                filter.ToDate.HasValue ? (object)filter.ToDate.Value : DBNull.Value);
        }
    }
}
