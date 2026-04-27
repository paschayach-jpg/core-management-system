using CMS.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result DTO for workflow instance
    /// </summary>
    public class WorkflowInstanceResult
    {
        /// <summary>
        /// Workflow instance ID
        /// </summary>
        public int WorkflowInstanceId { get; set; }

        /// <summary>
        /// Workflow definition ID
        /// </summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Workflow definition name
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Template version used
        /// </summary>
        public int TemplateVersion { get; set; }

        /// <summary>
        /// User who initiated the workflow
        /// </summary>
        public string InitiatedBy { get; set; }

        /// <summary>
        /// Date workflow was initiated
        /// </summary>
        public DateTime InitiatedDate { get; set; }

        /// <summary>
        /// Current workflow status
        /// </summary>
        public WorkflowInstanceStatus Status { get; set; }

        /// <summary>
        /// Current step ID (if in progress)
        /// </summary>
        public int? CurrentStepId { get; set; }

        /// <summary>
        /// Current step name (if in progress)
        /// </summary>
        public string CurrentStepName { get; set; }

        /// <summary>
        /// Date workflow was completed (if completed)
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Input parameters (JSON string)
        /// </summary>
        public string InputParameters { get; set; }

        /// <summary>
        /// Execution history of all steps
        /// </summary>
        public List<WorkflowStepInstanceResult> StepHistory { get; set; }

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int ProgressPercentage { get; set; }

        /// <summary>
        /// Description of the workflow instance
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Priority level
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// Template name (transient property)
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// Number of completed steps
        /// </summary>
        public int CompletedStepCount { get; set; }

        /// <summary>
        /// Total number of steps in workflow
        /// </summary>
        public int TotalStepCount { get; set; }
    }
}
