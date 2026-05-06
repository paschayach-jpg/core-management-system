using CMS.Domain.Enums;
using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result DTO for workflow step instance
    /// </summary>
    public class WorkflowStepInstanceResult
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
        /// Workflow step ID
        /// </summary>
        public int WorkflowStepId { get; set; }

        /// <summary>
        /// Step name
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// Step type
        /// </summary>
        public WorkflowStepType StepType { get; set; }

        /// <summary>
        /// User assigned to this step
        /// </summary>
        public string AssignedTo { get; set; }

        /// <summary>
        /// Date step was assigned
        /// </summary>
        public DateTime AssignedDate { get; set; }

        /// <summary>
        /// Current step status
        /// </summary>
        public WorkflowStepInstanceStatus Status { get; set; }

        /// <summary>
        /// Date step was completed
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// User who completed the step
        /// </summary>
        public string CompletedBy { get; set; }

        /// <summary>
        /// Output data from step (JSON string)
        /// </summary>
        public string OutputData { get; set; }

        /// <summary>
        /// SLA deadline for this step
        /// </summary>
        public DateTime SLADeadline { get; set; }

        /// <summary>
        /// Current SLA status
        /// </summary>
        public SLAStatus SLAStatus { get; set; }

        /// <summary>
        /// Time spent on this step (in minutes)
        /// </summary>
        public int? TimeSpentMinutes { get; set; }

        /// <summary>
        /// Remarks or comments
        /// </summary>
        public string Remarks { get; set; }
    }
}
