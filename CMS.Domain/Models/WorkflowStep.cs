using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents a single step within a workflow template definition.
    /// Maps to workflow.WorkflowStep in the SCS database.
    /// </summary>
    public class WorkflowStep
    {
        /// <summary>Primary key.</summary>
        public int WorkflowStepId { get; set; }

        /// <summary>Foreign key to the parent WorkflowDefinition template.</summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>Display name for this step.</summary>
        public string StepName { get; set; }

        /// <summary>Functional type of this step (Start, Approval, Review, etc.).</summary>
        public WorkflowStepType StepType { get; set; }

        /// <summary>Human-readable description of what this step does.</summary>
        public string Description { get; set; }

        /// <summary>
        /// Role name assigned to perform this step.
        /// Null when the step is assigned to a specific user or org unit instead.
        /// </summary>
        public string AssignedRole { get; set; }

        /// <summary>
        /// AD username of the specific user assigned to perform this step.
        /// Null when the step is assigned by role or org unit.
        /// </summary>
        public string AssignedUser { get; set; }

        /// <summary>
        /// Org unit code assigned to perform this step.
        /// Null when the step is assigned by role or specific user.
        /// </summary>
        public string AssignedOrgUnit { get; set; }

        /// <summary>
        /// SLA duration in hours. Combined with SLADays to compute the total SLA window.
        /// Null when no SLA is defined for this step.
        /// </summary>
        public int? SLAHours { get; set; }

        /// <summary>
        /// SLA duration in days. Combined with SLAHours to compute the total SLA window.
        /// Null when no SLA is defined for this step.
        /// </summary>
        public int? SLADays { get; set; }

        /// <summary>
        /// Whether the assignee is permitted to delegate this step to another user.
        /// </summary>
        public bool AllowDelegation { get; set; }

        /// <summary>
        /// Maximum number of times this step may be re-delegated in a chain.
        /// Enforced by WorkflowDelegationService.
        /// </summary>
        public int MaxDelegationDepth { get; set; }

        /// <summary>
        /// Execution order of this step within the workflow template.
        /// Lower numbers execute first.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>Soft-delete flag. True when this step has been logically removed.</summary>
        public bool IsDeleted { get; set; }
    }
}
