using System;
using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request to create a new workflow template
    /// </summary>
    public class CreateTemplateRequest
    {
        /// <summary>
        /// Template name (must be unique)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Template description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Template definition (JSON string containing steps and connections)
        /// </summary>
        public string TemplateDefinition { get; set; }

        /// <summary>
        /// Whether template is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Optional change description for versioning
        /// </summary>
        public string ChangeDescription { get; set; }
    }
}
