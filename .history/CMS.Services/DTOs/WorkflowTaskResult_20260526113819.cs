using CMS.Domain.Enums;
using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result DTO for workflow task (user's task view)
    /// </summary>
    public class WorkflowTaskResult
    {
        /// <summary>
        /// Step instance ID
        /// </summary>
        public int WorkflowStepInstanceId { get; set; }

        /// <summary>
        /// Workflow instance ID
        /// </summary>
        public int WorkflowInstanceId { get; set; }

        /// <summary>
        /// Workflow name
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Step name
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// Step description
        /// </summary>
        public string StepDescription { get; set; }

        /// <summary>
        /// Task status
        /// </summary>
        public WorkflowStepInstanceStatus Status { get; set; }

        /// <summary>
        /// Date task was assigned
        /// </summary>
        public DateTime AssignedDate { get; set; }

        /// <summary>
        /// Date task was completed
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// SLA deadline
        /// </summary>
        public DateTime SLADeadline { get; set; }

        /// <summary>
        /// SLA status
        /// </summary>
        public SLAStatus SLAStatus { get; set; }

        /// <summary>
        /// Priority level
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// User who initiated the workflow
        /// </summary>
        public string InitiatedBy { get; set; }

        /// <summary>
        /// Whether task can be delegated
        /// </summary>
        public bool CanDelegate { get; set; }

        /// <summary>
        /// Current delegation depth
        /// </summary>
        public int CurrentDelegationDepth { get; set; }

        /// <summary>
        /// Maximum allowed delegation depth
        /// </summary>
        public int MaxDelegationDepth { get; set; }

        /// <summary>
        /// Time remaining until SLA deadline (in minutes)
        /// </summary>
        public int? TimeRemainingMinutes { get; set; }

        /// <summary>
        /// Workflow instance description
        /// </summary>
        public string WorkflowDescription { get; set; }
    }
}
