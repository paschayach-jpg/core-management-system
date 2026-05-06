using System;
using System.Collections.Generic;
using CMS.Data.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Presenters.Interfaces
{
    /// <summary>
    /// View interface for Workflow Monitor (Administrative dashboard)
    /// </summary>
    public interface IWorkflowMonitorView
    {
        /// <summary>
        /// Display system-wide metrics
        /// </summary>
        void DisplayMetrics(WorkflowMonitorMetrics metrics);

        /// <summary>
        /// Display SLA compliance chart data
        /// </summary>
        void DisplaySLAChart(List<SLAChartData> chartData);

        /// <summary>
        /// Display task distribution by user/role
        /// </summary>
        void DisplayTaskDistribution(List<TaskDistributionData> distribution);

        /// <summary>
        /// Display bottleneck analysis
        /// </summary>
        void DisplayBottleneckAnalysis(List<BottleneckData> bottlenecks);

        /// <summary>
        /// Display escalation statistics
        /// </summary>
        void DisplayEscalationStats(EscalationStatistics stats);

        /// <summary>
        /// Show error message
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Show info message
        /// </summary>
        void ShowInfo(string message);
    }

    /// <summary>
    /// Monitor metrics data
    /// </summary>
    public class WorkflowMonitorMetrics
    {
        public int TotalInstances { get; set; }
        public int CompletedInstances { get; set; }
        public int FailedInstances { get; set; }
        public int InProgressInstances { get; set; }
        public int SuspendedInstances { get; set; }
        public double AverageDuration { get; set; }
        public double AverageCompletionTimeHours { get; set; }
    }

    /// <summary>
    /// SLA chart data point
    /// </summary>
    public class SLAChartData
    {
        public string TemplateName { get; set; }
        public int OnTimeCount { get; set; }
        public int AtRiskCount { get; set; }
        public int OverdueCount { get; set; }
        public double CompliancePercentage { get; set; }
    }

    /// <summary>
    /// Task distribution data
    /// </summary>
    public class TaskDistributionData
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public int AssignedCount { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int PendingTasks { get; set; }
        public int CompletedTasks { get; set; }
        public double AverageCompletionTimeHours { get; set; }
    }

    /// <summary>
    /// Bottleneck analysis data
    /// </summary>
    public class BottleneckData
    {
        public string TemplateName { get; set; }
        public string StepName { get; set; }
        public int PendingCount { get; set; }
        public double AverageWaitTimeHours { get; set; }
        public double AverageCompletionTimeHours { get; set; }
        public int TotalInstances { get; set; }
        public int OverdueCount { get; set; }
    }

    /// <summary>
    /// Escalation statistics
    /// </summary>
    public class EscalationStatistics
    {
        public int TotalEscalations { get; set; }
        public int EscalationsThisWeek { get; set; }
        public int EscalationsThisMonth { get; set; }
        public List<string> TopEscalatedSteps { get; set; }
        public Dictionary<string, int> EscalationsByReason { get; set; }
        public DateTime? LastEscalationDate { get; set; }
    }
}
