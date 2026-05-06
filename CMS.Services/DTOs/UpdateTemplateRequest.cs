using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request to update an existing workflow template
    /// </summary>
    public class UpdateTemplateRequest
    {
        /// <summary>
        /// Template ID to update
        /// </summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Updated template name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Updated template description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Updated template definition (JSON string)
        /// </summary>
        public string TemplateDefinition { get; set; }

        /// <summary>
        /// Whether template is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Change description for versioning
        /// </summary>
        public string ChangeDescription { get; set; }
    }
}
