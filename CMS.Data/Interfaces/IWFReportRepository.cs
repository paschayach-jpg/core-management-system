using System;
using CMS.Data.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository interface for workflow reporting and analytics queries.
    /// All methods map to dedicated reporting stored procedures and return
    /// lightweight result models rather than full domain entities.
    /// </summary>
    public interface IWFReportRepository
    {
        /// <summary>
        /// Returns SLA compliance metrics (on-time %, average completion time, etc.)
        /// for the administrative dashboard and SLA report.
        /// </summary>
        SLAComplianceMetrics GetSLAComplianceMetrics(WorkflowReportFilter filter);

        /// <summary>
        /// Returns task distribution counts grouped by assigned user.
        /// Used by the administrative dashboard "task distribution" panel.
        /// </summary>
        PagedResult<TaskDistributionRow> GetTaskDistribution(WorkflowReportFilter filter);

        /// <summary>
        /// Returns average and maximum completion times per workflow step to identify
        /// bottlenecks.  Used by the administrative dashboard bottleneck analysis panel.
        /// </summary>
        PagedResult<BottleneckRow> GetBottleneckAnalysis(WorkflowReportFilter filter);

        /// <summary>
        /// Returns escalation statistics (count by escalation action, overdue counts).
        /// Used by the administrative dashboard escalation statistics panel.
        /// </summary>
        EscalationStatistics GetEscalationStatistics(WorkflowReportFilter filter);

        /// <summary>SLA compliance breakdown per workflow template.</summary>
        System.Collections.Generic.IList<SLATemplateMetricRow> GetSLAByTemplate(WorkflowReportFilter filter);

        /// <summary>Weekly SLA compliance trend for the last <paramref name="weeks"/> weeks.</summary>
        System.Collections.Generic.IList<SLATrendRow> GetSLAWeeklyTrend(int weeks);

        /// <summary>Cycle-time distribution histogram for completed workflow instances.</summary>
        System.Collections.Generic.IList<CycleTimeRow> GetCycleTimeDistribution(WorkflowReportFilter filter);
    }

    // ----------------------------------------------------------------
    // Lightweight result models used only by the reporting repository
    // ----------------------------------------------------------------

    /// <summary>SLA compliance summary returned by GetSLAComplianceMetrics.</summary>
    public class SLAComplianceMetrics
    {
        public int    TotalSteps        { get; set; }
        public int    OnTimeCount       { get; set; }
        public int    AtRiskCount       { get; set; }
        public int    OverdueCount      { get; set; }
        public double OnTimePercent     { get; set; }
        public double AvgCompletionHours { get; set; }
    }

    /// <summary>Per-user task distribution row returned by GetTaskDistribution.</summary>
    public class TaskDistributionRow
    {
        public string AssignedTo    { get; set; }
        public int    TotalAssigned { get; set; }
        public int    Completed     { get; set; }
        public int    Pending       { get; set; }
        public int    Rejected      { get; set; }
        public int    Overdue       { get; set; }
    }

    /// <summary>Per-step bottleneck row returned by GetBottleneckAnalysis.</summary>
    public class BottleneckRow
    {
        public int     WorkflowStepId          { get; set; }
        public string  StepName                { get; set; }
        public int     StepType                { get; set; }
        public int     SequenceNumber          { get; set; }
        public string  WorkflowDefinitionName  { get; set; }
        public int     TotalExecutions         { get; set; }
        public int     CompletedCount          { get; set; }
        public int     OverdueCount            { get; set; }
        public double? AvgCompletionHours      { get; set; }
        public double? MaxCompletionHours      { get; set; }
    }

    /// <summary>Escalation statistics returned by GetEscalationStatistics.</summary>
    public class EscalationStatistics
    {
        public int TotalEscalations      { get; set; }
        public int NotifyManagerCount    { get; set; }
        public int IncreasePriorityCount { get; set; }
        public int ReassignCount         { get; set; }
        public int AutoRejectCount       { get; set; }
    }

    public class SLATemplateMetricRow
    {
        public int    WorkflowDefinitionId { get; set; }
        public string TemplateName         { get; set; }
        public int    Total                { get; set; }
        public int    OnTime               { get; set; }
        public int    AtRisk               { get; set; }
        public int    Overdue              { get; set; }
    }

    public class SLATrendRow
    {
        public string WeekLabel          { get; set; }
        public double CompliancePercent  { get; set; }
    }

    public class CycleTimeRow
    {
        public string Label    { get; set; }
        public int    Count    { get; set; }
        public int    SortKey  { get; set; }
    }

    /// <summary>Common filter parameters shared by all reporting queries.</summary>
    public class WorkflowReportFilter
    {
        public int?     WorkflowDefinitionId { get; set; }
        public DateTime? FromDate            { get; set; }
        public DateTime? ToDate              { get; set; }
        public int      PageNumber           { get; set; }
        public int      PageSize             { get; set; }

        public WorkflowReportFilter() { PageNumber = 1; PageSize = 50; }
    }
}
