using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents a running (or completed) execution of a workflow template.
    /// Maps to workflow.WorkflowInstance in the SCS database.
    /// </summary>
    public class WorkflowInstance
    {
        /// <summary>Primary key.</summary>
        public int WorkflowInstanceId { get; set; }

        /// <summary>Foreign key to the WorkflowDefinition template used to create this instance.</summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Snapshot of the template version number at the time this instance was created.
        /// Preserved so that template updates do not affect in-flight instances.
        /// </summary>
        public int TemplateVersion { get; set; }

        /// <summary>AD username of the user who initiated this workflow instance.</summary>
        public string InitiatedBy { get; set; }

        /// <summary>Date and time the workflow instance was created.</summary>
        public DateTime InitiatedDate { get; set; }

        /// <summary>
        /// Foreign key to the WorkflowStep that is currently active.
        /// Null when the instance has not yet started or has reached a terminal state.
        /// </summary>
        public int? CurrentStepId { get; set; }

        /// <summary>Current lifecycle status of this workflow instance.</summary>
        public WorkflowInstanceStatus Status { get; set; }

        /// <summary>
        /// Date and time the workflow instance reached a terminal state (Completed or Rejected).
        /// Null while the instance is still in progress.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// JSON-serialised input parameters supplied when the instance was created.
        /// Schema is defined by the workflow template.
        /// </summary>
        public string InputParameters { get; set; }

        /// <summary>Soft-delete flag. True when the instance has been logically deleted.</summary>
        public bool IsDeleted { get; set; }

        /// <summary>Date and time the instance was soft-deleted. Null when not deleted.</summary>
        public DateTime? DeletedDate { get; set; }
    }
}
