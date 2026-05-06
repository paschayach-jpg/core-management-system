using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents a single execution of a workflow step within a running workflow instance.
    /// Each time a step becomes active a new WorkflowStepInstance row is created.
    /// Maps to workflow.WorkflowStepInstance in the SCS database.
    /// </summary>
    public class WorkflowStepInstance
    {
        /// <summary>Primary key.</summary>
        public int WorkflowStepInstanceId { get; set; }

        /// <summary>Foreign key to the parent WorkflowInstance.</summary>
        public int WorkflowInstanceId { get; set; }

        /// <summary>Foreign key to the WorkflowStep template definition this instance executes.</summary>
        public int WorkflowStepId { get; set; }

        /// <summary>AD username of the user currently assigned to action this step.</summary>
        public string AssignedTo { get; set; }

        /// <summary>Date and time the step was assigned to the current assignee.</summary>
        public DateTime AssignedDate { get; set; }

        /// <summary>Current lifecycle status of this step instance.</summary>
        public WorkflowStepInstanceStatus Status { get; set; }

        /// <summary>
        /// Date and time the step reached a terminal state (Completed, Rejected, or Delegated).
        /// Null while the step is still pending or in progress.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// AD username of the user who completed or rejected this step.
        /// Null while the step has not yet been actioned.
        /// </summary>
        public string CompletedBy { get; set; }

        /// <summary>
        /// JSON-serialised output data produced when the step was completed.
        /// Schema is defined by the workflow template step definition.
        /// Null while the step has not yet been completed.
        /// </summary>
        public string OutputData { get; set; }

        /// <summary>
        /// Calculated deadline by which this step must be completed to satisfy its SLA.
        /// Computed as AssignedDate + SLA duration from the associated WFSLARule.
        /// </summary>
        public DateTime SLADeadline { get; set; }

        /// <summary>
        /// Current SLA compliance status for this step instance.
        /// Updated by the WFESLABackgroundJob every 5 minutes.
        /// </summary>
        public SLAStatus SLAStatus { get; set; }

        /// <summary>
        /// Optional reference to an external entity (e.g. an approval task ID or task management task ID)
        /// created as part of processing this step.
        /// Null when no external entity has been created.
        /// </summary>
        public int? ExternalReferenceId { get; set; }

        /// <summary>
        /// Describes the type of external entity referenced by ExternalReferenceId
        /// (e.g. "ApprovalRequest", "TaskManagementTask").
        /// Null when ExternalReferenceId is null.
        /// </summary>
        public string ExternalReferenceType { get; set; }

        /// <summary>
        /// ID of the task created in the Task Management module (MOD-009) for this step.
        /// Null when no corresponding task management task has been created.
        /// </summary>
        public int? ExternalTaskId { get; set; }

        /// <summary>Soft-delete flag. True when this step instance has been logically deleted.</summary>
        public bool IsDeleted { get; set; }
    }
}
