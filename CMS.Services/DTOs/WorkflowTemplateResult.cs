using System;
using System.Collections.Generic;

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
        /// Explicit status: Active, Draft, Inactive, Deprecated.
        /// When not set, derived from IsActive.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Status label: Active, Draft, Inactive, or Deprecated
        /// </summary>
        public string StatusLabel
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Status)) return Status;
                return IsActive ? "Active" : "Inactive";
            }
        }
    }
}
