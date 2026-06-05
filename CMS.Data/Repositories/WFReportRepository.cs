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
                    "FROM WFE.t_WFStepInstance wsi " +
                    "INNER JOIN WFE.t_WFInstance wi ON wi.WFInstanceId = wsi.WFInstanceId " +
                    "LEFT  JOIN dbo.WFSLARule sla ON sla.WorkflowStepId = wsi.WFStepId AND sla.IsActive = 1 " +
                    "WHERE wsi.SLAStatus = '3' " +   // Overdue
                    "  AND wsi.IsDeleted = 0 " +
                    "  AND wi.IsDeleted  = 0 " +
                    "  AND (@WorkflowDefinitionId IS NULL OR wi.WFDefinitionId = @WorkflowDefinitionId) " +
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
        // SLA by Template
        // ----------------------------------------------------------------

        public IList<SLATemplateMetricRow> GetSLAByTemplate(WorkflowReportFilter filter)
        {
            var results = new List<SLATemplateMetricRow>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    const string sql =
                        "SELECT wd.WFDefinitionId AS WorkflowDefinitionId, wd.WFName AS TemplateName, " +
                        "       COUNT(*) AS Total, " +
                        "       SUM(CASE WHEN wsi.SLAStatus = '1' THEN 1 ELSE 0 END) AS OnTime, " +
                        "       SUM(CASE WHEN wsi.SLAStatus = '2' THEN 1 ELSE 0 END) AS AtRisk, " +
                        "       SUM(CASE WHEN wsi.SLAStatus = '3' THEN 1 ELSE 0 END) AS Overdue " +
                        "FROM   WFE.s_WFDefinition wd " +
                        "INNER JOIN WFE.t_WFInstance wi " +
                        "       ON wi.WFDefinitionId = wd.WFDefinitionId AND wi.IsDeleted = 0 " +
                        "INNER JOIN WFE.t_WFStepInstance wsi " +
                        "       ON wsi.WFInstanceId = wi.WFInstanceId AND wsi.IsDeleted = 0 " +
                        "WHERE  wd.IsDeleted = 0 " +
                        "  AND  (@WorkflowDefinitionId IS NULL OR wd.WFDefinitionId = @WorkflowDefinitionId) " +
                        "  AND  (@FromDate IS NULL OR wsi.AssignedDate >= @FromDate) " +
                        "  AND  (@ToDate   IS NULL OR wsi.AssignedDate <= @ToDate) " +
                        "GROUP  BY wd.WFDefinitionId, wd.WFName " +
                        "ORDER  BY Total DESC";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        AddFilterParameters(cmd, filter);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new SLATemplateMetricRow
                                {
                                    WorkflowDefinitionId = (int)reader["WorkflowDefinitionId"],
                                    TemplateName         = reader["TemplateName"] == DBNull.Value ? "" : (string)reader["TemplateName"],
                                    Total                = (int)reader["Total"],
                                    OnTime               = (int)reader["OnTime"],
                                    AtRisk               = (int)reader["AtRisk"],
                                    Overdue              = (int)reader["Overdue"],
                                });
                            }
                        }
                    }
                }
            }
            catch { /* return empty list — table may not exist yet */ }
            return results;
        }

        // ----------------------------------------------------------------
        // SLA Weekly Trend
        // ----------------------------------------------------------------

        public IList<SLATrendRow> GetSLAWeeklyTrend(int weeks)
        {
            var results = new List<SLATrendRow>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    const string sql =
                        "SELECT yr, wk, " +
                        "       CAST(SUM(CASE WHEN SLAStatus = '1' THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0) AS float) AS CompliancePct " +
                        "FROM ( " +
                        "    SELECT YEAR(AssignedDate) AS yr, DATEPART(WEEK, AssignedDate) AS wk, SLAStatus " +
                        "    FROM   WFE.t_WFStepInstance " +
                        "    WHERE  IsDeleted = 0 AND AssignedDate >= DATEADD(WEEK, -@Weeks, GETUTCDATE()) " +
                        ") t " +
                        "GROUP  BY yr, wk " +
                        "ORDER  BY yr, wk";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@Weeks", weeks);
                        using (var reader = cmd.ExecuteReader())
                        {
                            int idx = 1;
                            while (reader.Read())
                            {
                                double pct = reader["CompliancePct"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CompliancePct"]);
                                results.Add(new SLATrendRow { WeekLabel = "W" + idx++, CompliancePercent = pct });
                            }
                        }
                    }
                }
            }
            catch { /* return empty list */ }
            return results;
        }

        // ----------------------------------------------------------------
        // Cycle Time Distribution
        // ----------------------------------------------------------------

        public IList<CycleTimeRow> GetCycleTimeDistribution(WorkflowReportFilter filter)
        {
            var results = new List<CycleTimeRow>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    const string sql =
                        "SELECT BucketLabel AS Label, BucketOrder AS SortKey, COUNT(*) AS Cnt " +
                        "FROM ( " +
                        "    SELECT " +
                        "        CASE " +
                        "            WHEN ct >= 0  AND ct < 1  THEN '0–1'  " +
                        "            WHEN ct >= 1  AND ct < 2  THEN '1–2'  " +
                        "            WHEN ct >= 2  AND ct < 3  THEN '2–3'  " +
                        "            WHEN ct >= 3  AND ct < 4  THEN '3–4'  " +
                        "            WHEN ct >= 4  AND ct < 5  THEN '4–5'  " +
                        "            WHEN ct >= 5  AND ct < 7  THEN '5–7'  " +
                        "            WHEN ct >= 7  AND ct < 10 THEN '7–10' " +
                        "            ELSE '10+' " +
                        "        END AS BucketLabel, " +
                        "        CASE " +
                        "            WHEN ct >= 0  AND ct < 1  THEN 0 " +
                        "            WHEN ct >= 1  AND ct < 2  THEN 1 " +
                        "            WHEN ct >= 2  AND ct < 3  THEN 2 " +
                        "            WHEN ct >= 3  AND ct < 4  THEN 3 " +
                        "            WHEN ct >= 4  AND ct < 5  THEN 4 " +
                        "            WHEN ct >= 5  AND ct < 7  THEN 5 " +
                        "            WHEN ct >= 7  AND ct < 10 THEN 6 " +
                        "            ELSE 7 " +
                        "        END AS BucketOrder " +
                        "    FROM ( " +
                        "        SELECT DATEDIFF(MINUTE, wi.InitiatedDate, wi.CompletedDate) / 1440.0 AS ct " +
                        "        FROM   WFE.t_WFInstance wi " +
                        "        WHERE  wi.IsDeleted = 0 AND wi.Status = 3 AND wi.CompletedDate IS NOT NULL " +
                        "          AND  (@FromDate IS NULL OR wi.InitiatedDate >= @FromDate) " +
                        "          AND  (@ToDate   IS NULL OR wi.InitiatedDate <= @ToDate) " +
                        "    ) t " +
                        ") b " +
                        "GROUP  BY BucketLabel, BucketOrder " +
                        "ORDER  BY BucketOrder";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.HasValue ? (object)filter.FromDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate",   filter.ToDate.HasValue   ? (object)filter.ToDate.Value   : DBNull.Value);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new CycleTimeRow
                                {
                                    Label   = (string)reader["Label"],
                                    Count   = (int)reader["Cnt"],
                                    SortKey = (int)reader["SortKey"],
                                });
                            }
                        }
                    }
                }
            }
            catch { /* return empty list */ }
            return results;
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
