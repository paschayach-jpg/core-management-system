using CMS.Domain.Models;
using System;
using System.Collections.Generic;

namespace CMS.Services.SLA
{
    /// <summary>
    /// Optimized SLA calculator with batch processing support
    /// Supports Phase 13: Performance Optimization
    /// Requirement 7 AC1-7: SLA Management
    /// Requirement 21 AC4-5: Performance targets for SLA evaluation
    /// 
    /// Optimization Strategy:
    /// - Batch SLA evaluation for multiple steps
    /// - Caching of SLA rules
    /// - Optimized deadline calculations
    /// - Efficient escalation grouping
    /// </summary>
    public interface IOptimizedSLACalculator
    {
        /// <summary>
        /// Calculate SLA deadline for a single step instance
        /// </summary>
        DateTime CalculateSLADeadline(WorkflowStepInstance stepInstance, WFSLARule slaRule);

        /// <summary>
        /// Evaluate SLA status for a single step instance
        /// </summary>
        SLAStatus EvaluateSLAStatus(WorkflowStepInstance stepInstance);

        /// <summary>
        /// Batch evaluate SLA status for multiple step instances
        /// Optimized for background job processing
        /// </summary>
        IList<SLAEvaluationResult> BatchEvaluateSLAStatus(IList<WorkflowStepInstance> stepInstances);

        /// <summary>
        /// Check and escalate SLA for a single step instance
        /// </summary>
        bool CheckAndEscalateSLA(WorkflowStepInstance stepInstance);

        /// <summary>
        /// Batch check and escalate SLA for multiple step instances
        /// Groups escalations by action for efficient processing
        /// </summary>
        IList<SLAEscalationResult> BatchCheckAndEscalateSLA(IList<WorkflowStepInstance> stepInstances);

        /// <summary>
        /// Get SLA compliance metrics
        /// </summary>
        SLAComplianceMetrics GetSLAComplianceMetrics(DateTime fromDate, DateTime toDate);
    }

    /// <summary>
    /// Result of SLA evaluation for a single step
    /// </summary>
    public class SLAEvaluationResult
    {
        public int WorkflowStepInstanceId { get; set; }
        public SLAStatus Status { get; set; }
        public DateTime Deadline { get; set; }
        public int MinutesRemaining { get; set; }
        public bool RequiresWarning { get; set; }
        public bool RequiresEscalation { get; set; }
    }

    /// <summary>
    /// Result of SLA escalation for a single step
    /// </summary>
    public class SLAEscalationResult
    {
        public int WorkflowStepInstanceId { get; set; }
        public int WorkflowInstanceId { get; set; }
        public string AssignedTo { get; set; }
        public EscalationAction Action { get; set; }
        public string EscalationRecipient { get; set; }
        public DateTime EscalationTime { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// SLA compliance metrics
    /// </summary>
    public class SLAComplianceMetrics
    {
        public int TotalTasks { get; set; }
        public int OnTimeTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int AtRiskTasks { get; set; }
        public double CompliancePercentage { get; set; }
        public double AverageCompletionTimeMinutes { get; set; }
        public int EscalationCount { get; set; }
    }

    /// <summary>
    /// Enumeration for escalation actions
    /// </summary>
    public enum EscalationAction
    {
        NotifyManager = 1,
        IncreasePriority = 2,
        Reassign = 3,
        AutoReject = 4
    }

    /// <summary>
    /// SLA status enumeration
    /// </summary>
    public enum SLAStatus
    {
        OnTime = 1,
        AtRisk = 2,
        Overdue = 3
    }
}
