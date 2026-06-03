using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents a workflow template definition used to create workflow instances.
    /// Maps to workflow.WorkflowDefinition in the SCS database.
    /// </summary>
    public class WorkflowDefinition
    {
        /// <summary>Primary key.</summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>Unique name identifying this workflow template.</summary>
        public string Name { get; set; }

        /// <summary>Human-readable description of the workflow's purpose.</summary>
        public string Description { get; set; }

        /// <summary>
        /// Template version number. Incremented each time the template is published
        /// while active instances exist, preserving the snapshot used by those instances.
        /// </summary>
        public int Version { get; set; }

        /// <summary>Indicates whether this template is available for creating new instances.</summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Approval lifecycle status: Draft → PendingApproval → Active (or back to Draft on reject).
        /// Maps to WorkflowDefinition.ApprovalStatus column (tinyint).
        /// </summary>
        public WorkflowTemplateStatus ApprovalStatus { get; set; }

        /// <summary>AD username of the user who submitted this template for approval (null = not submitted yet).</summary>
        public string SubmittedBy { get; set; }

        /// <summary>Date and time the template was submitted for approval.</summary>
        public DateTime? SubmittedDate { get; set; }

        /// <summary>AD username of the approver who approved or rejected this template.</summary>
        public string ApprovedBy { get; set; }

        /// <summary>Date and time the approval decision was made.</summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>Reason for rejection (populated when ApprovalStatus = Draft after a reject).</summary>
        public string RejectionReason { get; set; }

        /// <summary>AD username of the user who created this template.</summary>
        public string CreatedBy { get; set; }

        /// <summary>Date and time the template was created.</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>AD username of the user who last modified this template.</summary>
        public string ModifiedBy { get; set; }

        /// <summary>Date and time the template was last modified.</summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>Soft-delete flag. True when the template has been logically deleted.</summary>
        public bool IsDeleted { get; set; }

        /// <summary>Date and time the template was soft-deleted. Null when not deleted.</summary>
        public DateTime? DeletedDate { get; set; }

        /// <summary>Template definition (JSON string) used by the wizard and form builder.</summary>
        public string TemplateDefinition { get; set; }

        /// <summary>Number of steps defined in this template (populated in list queries).</summary>
        public int StepCount { get; set; }

        /// <summary>Total workflow instances ever created from this template.</summary>
        public int TotalInstanceCount { get; set; }

        /// <summary>Active (non-terminal) workflow instances from this template.</summary>
        public int ActiveInstanceCount { get; set; }

        /// <summary>Category label (e.g. เอกสาร, IT, HR). Maps to Category column in s_WFDefinition.</summary>
        public string Category { get; set; }
    }
}
