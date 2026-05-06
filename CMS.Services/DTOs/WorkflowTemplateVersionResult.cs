using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result DTO for workflow template version
    /// </summary>
    public class WorkflowTemplateVersionResult
    {
        /// <summary>
        /// Template ID
        /// </summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Version number
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Template name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User who created this version
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Date version was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Change description
        /// </summary>
        public string ChangeDescription { get; set; }

        /// <summary>
        /// Whether this is the active version
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Number of instances using this version
        /// </summary>
        public int InstanceCount { get; set; }
    }
}
