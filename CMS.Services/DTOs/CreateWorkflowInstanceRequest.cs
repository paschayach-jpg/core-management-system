using System;
using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request to create a new workflow instance
    /// </summary>
    public class CreateWorkflowInstanceRequest
    {
        /// <summary>
        /// Workflow template ID
        /// </summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Template version to use (optional, defaults to latest active)
        /// </summary>
        public int? TemplateVersion { get; set; }

        /// <summary>
        /// Input parameters for the workflow instance (JSON string)
        /// </summary>
        public string InputParameters { get; set; }

        /// <summary>
        /// Optional description for this workflow instance
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional priority level
        /// </summary>
        public int? Priority { get; set; }
    }
}
