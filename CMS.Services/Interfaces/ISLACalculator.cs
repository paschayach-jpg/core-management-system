using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Services.DTOs;
using System;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// SLA calculation and monitoring service
    /// Handles SLA deadline calculation, status evaluation, and escalation
    /// </summary>
    public interface ISLACalculator
    {
        /// <summary>
        /// Calculate SLA deadline for a workflow step instance
        /// </summary>
        DateTime CalculateSLADeadline(WorkflowStepInstance stepInstance, WFSLARule slaRule);

        /// <summary>
        /// Evaluate current SLA status for a step instance
        /// </summary>
        SLAStatus EvaluateSLAStatus(WorkflowStepInstance stepInstance);

        /// <summary>
        /// Get SLA compliance metrics for a given filter
        /// </summary>
        SLAComplianceMetricsResult GetSLAComplianceMetrics(SLAMetricsFilter filter);

        /// <summary>
        /// Check SLA deadline and trigger escalation if needed
        /// Returns true if escalation was triggered
        /// </summary>
        bool CheckAndEscalateSLA(WorkflowStepInstance stepInstance);

        /// <summary>
        /// Get warning threshold percentage for a step
        /// </summary>
        int GetWarningThresholdPercent(WorkflowStepInstance stepInstance);

        /// <summary>
        /// Calculate time remaining until SLA deadline
        /// </summary>
        TimeSpan GetTimeRemaining(WorkflowStepInstance stepInstance);
    }
}
