using System;

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
    }
}
