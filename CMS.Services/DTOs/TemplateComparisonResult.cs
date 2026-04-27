using System;
using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result DTO for template version comparison
    /// </summary>
    public class TemplateComparisonResult
    {
        /// <summary>
        /// Template ID
        /// </summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>
        /// First version number
        /// </summary>
        public int Version1 { get; set; }

        /// <summary>
        /// Second version number
        /// </summary>
        public int Version2 { get; set; }

        /// <summary>
        /// Steps added in version 2
        /// </summary>
        public List<string> StepsAdded { get; set; }

        /// <summary>
        /// Steps removed in version 2
        /// </summary>
        public List<string> StepsRemoved { get; set; }

        /// <summary>
        /// Steps modified in version 2
        /// </summary>
        public List<StepModification> StepsModified { get; set; }

        /// <summary>
        /// Connections added in version 2
        /// </summary>
        public List<string> ConnectionsAdded { get; set; }

        /// <summary>
        /// Connections removed in version 2
        /// </summary>
        public List<string> ConnectionsRemoved { get; set; }

        /// <summary>
        /// Whether there are any differences
        /// </summary>
        public bool HasDifferences { get; set; }
    }

    /// <summary>
    /// Details of a step modification
    /// </summary>
    public class StepModification
    {
        public string StepName { get; set; }
        public List<string> ChangedProperties { get; set; }
        public Dictionary<string, object> OldValues { get; set; }
        public Dictionary<string, object> NewValues { get; set; }
    }
}
