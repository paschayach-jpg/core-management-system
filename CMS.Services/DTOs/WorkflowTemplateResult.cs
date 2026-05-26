using System;
using System.Collections.Generic;
using CMS.Domain.Enums;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result DTO for workflow template
    /// </summary>
    public class WorkflowTemplateResult
    {
        /// <summary>
        /// Template ID
        /// </summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Template name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Template description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Current version number
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Whether template is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// User who created the template
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Date template was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// User who last modified the template
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Date template was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Template definition (JSON string)
        /// </summary>
        public string TemplateDefinition { get; set; }

        /// <summary>
        /// Number of active instances using this template
        /// </summary>
        public int ActiveInstanceCount { get; set; }

        /// <summary>
        /// Total number of instances created from this template
        /// </summary>
        public int TotalInstanceCount { get; set; }

        /// <summary>
        /// Category of the workflow template (e.g., เอกสาร, IT, Knowledge, HR)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Number of steps in this workflow template
        /// </summary>
        public int StepCount { get; set; }

        /// <summary>
        /// Display name of the department/team that owns this template
        /// </summary>
        public string OwnerDepartment { get; set; }

        /// <summary>
        /// Approval lifecycle status enum value.
        /// </summary>
        public WorkflowTemplateStatus ApprovalStatus { get; set; }

        /// <summary>
        /// Explicit status string: Active, Draft, PendingApproval, Inactive, Deprecated.
        /// When not set, derived from ApprovalStatus / IsActive.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Status label derived from ApprovalStatus or Status override.
        /// </summary>
        public string StatusLabel
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Status)) return Status;
                switch (ApprovalStatus)
                {
                    case WorkflowTemplateStatus.PendingApproval: return "PendingApproval";
                    case WorkflowTemplateStatus.Active:          return "Active";
                    case WorkflowTemplateStatus.Inactive:        return "Inactive";
                    case WorkflowTemplateStatus.Deprecated:      return "Deprecated";
                    default:                                     return "Draft";
                }
            }
        }

        /// <summary>AD username of who submitted the template for approval.</summary>
        public string SubmittedBy { get; set; }

        /// <summary>Date the template was submitted for approval.</summary>
        public DateTime? SubmittedDate { get; set; }

        /// <summary>AD username of who approved/rejected.</summary>
        public string ApprovedBy { get; set; }

        /// <summary>Date the approval decision was made.</summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>Rejection reason (when returned to Draft after reject).</summary>
        public string RejectionReason { get; set; }
    }
}
