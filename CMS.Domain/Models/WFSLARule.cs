using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Defines the SLA configuration for a specific workflow step.
    /// Controls deadline calculation, warning thresholds, and escalation behaviour
    /// when a step instance exceeds its allowed completion time.
    /// Maps to workflow.WFSLARule in the SCS database.
    /// </summary>
    public class WFSLARule
    {
        /// <summary>Primary key.</summary>
        public int WFSLARuleId { get; set; }

        /// <summary>Foreign key to the WorkflowStep this rule applies to.</summary>
        public int WorkflowStepId { get; set; }

        /// <summary>
        /// Total SLA window in hours. Used by ISLACalculator to compute the deadline:
        /// deadline = AssignedDate + SLADurationHours.
        /// Must be a positive value.
        /// </summary>
        public int SLADurationHours { get; set; }

        /// <summary>
        /// Percentage of the SLA window at which a warning notification is sent.
        /// For example, 75 means a warning fires when 75 % of the allowed time has elapsed.
        /// Valid range: 1–99. Defaults to 75.
        /// </summary>
        public int WarningThresholdPercent { get; set; }

        /// <summary>
        /// Action to take when the SLA deadline is exceeded.
        /// Executed by WFESLABackgroundJob via ISLACalculator.CheckAndEscalateSLA.
        /// </summary>
        public EscalationAction EscalationAction { get; set; }

        /// <summary>
        /// Role name or AD username that receives the escalation.
        /// Interpretation depends on EscalationAction:
        ///   NotifyManager    — manager role or specific AD username to notify
        ///   Reassign         — role or user the task is reassigned to
        ///   IncreasePriority — not used (may be null)
        ///   AutoReject       — not used (may be null)
        /// </summary>
        public string EscalationRecipient { get; set; }

        /// <summary>
        /// Whether this SLA rule is currently active.
        /// Inactive rules are retained for historical reference but not evaluated
        /// by the background job.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>Date and time this rule was created.</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>Date and time this rule was last modified.</summary>
        public DateTime ModifiedDate { get; set; }
    }
}
