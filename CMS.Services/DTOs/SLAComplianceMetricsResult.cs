using System;
using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result DTO for SLA compliance metrics
    /// </summary>
    public class SLAComplianceMetricsResult
    {
        /// <summary>
        /// Total number of tasks evaluated
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// Number of tasks completed on time
        /// </summary>
        public int OnTimeCount { get; set; }

        /// <summary>
        /// Number of tasks completed overdue
        /// </summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// Number of tasks currently at risk
        /// </summary>
        public int AtRiskCount { get; set; }

        /// <summary>
        /// SLA compliance percentage (0-100)
        /// </summary>
        public decimal CompliancePercentage { get; set; }

        /// <summary>
        /// Average completion time in hours
        /// </summary>
        public decimal AverageCompletionHours { get; set; }

        /// <summary>
        /// Metrics by workflow template
        /// </summary>
        public List<TemplateMetrics> ByTemplate { get; set; }

        /// <summary>
        /// Metrics by user
        /// </summary>
        public List<UserMetrics> ByUser { get; set; }

        /// <summary>
        /// Date range for metrics
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// Date range for metrics
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>Weekly SLA compliance trend (ordered oldest first).</summary>
        public List<WeeklyTrendPoint> WeeklyTrend { get; set; }

        /// <summary>Cycle time distribution histogram.</summary>
        public List<CycleTimeBucket> CycleTimeDistribution { get; set; }
    }

    public class TemplateMetrics
    {
        public int WorkflowDefinitionId { get; set; }
        public string TemplateName { get; set; }
        public int TotalTasks { get; set; }
        public int OnTimeCount { get; set; }
        public int AtRiskCount { get; set; }
        public int OverdueCount { get; set; }
        public decimal CompliancePercentage { get; set; }
    }

    public class UserMetrics
    {
        public string UserName { get; set; }
        public string Department { get; set; }
        public int TotalTasks { get; set; }
        public int OnTimeCount { get; set; }
        public int OverdueCount { get; set; }
        public decimal CompliancePercentage { get; set; }
        public decimal AverageCompletionHours { get; set; }
        public double AvgOverdueDays { get; set; }
    }

    public class WeeklyTrendPoint
    {
        public string WeekLabel { get; set; }
        public double CompliancePercent { get; set; }
    }

    public class CycleTimeBucket
    {
        public string Label { get; set; }
        public int Count { get; set; }
        public string Category { get; set; } // "pass" | "near" | "over"
    }
}
