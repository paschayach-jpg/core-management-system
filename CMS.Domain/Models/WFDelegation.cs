using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Records a single task delegation event — when an assignee transfers their
    /// responsibility for a workflow step instance to another user.
    /// Maps to workflow.WFDelegation in the SCS database.
    /// </summary>
    public class WFDelegation
    {
        /// <summary>Primary key.</summary>
        public int WFDelegationId { get; set; }

        /// <summary>Foreign key to the step instance that was delegated.</summary>
        public int WorkflowStepInstanceId { get; set; }

        /// <summary>AD username of the user who delegated the task.</summary>
        public string DelegatedFrom { get; set; }

        /// <summary>AD username of the user who received the delegated task.</summary>
        public string DelegatedTo { get; set; }

        /// <summary>
        /// Free-text reason provided by the delegating user explaining why the task
        /// is being transferred.
        /// </summary>
        public string DelegationReason { get; set; }

        /// <summary>Date and time the delegation was recorded.</summary>
        public DateTime DelegationDate { get; set; }

        /// <summary>
        /// Depth of this delegation within the chain for the step instance.
        /// 1 = first delegation from the original assignee; increments with each
        /// subsequent re-delegation. Enforced against WorkflowStep.MaxDelegationDepth.
        /// </summary>
        public int DelegationDepth { get; set; }

        /// <summary>
        /// Indicates whether this delegation is currently active (i.e. the delegated
        /// user is the current assignee). Set to false when the task is completed,
        /// rejected, or re-delegated further.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>Soft-delete flag. True when this delegation record has been logically deleted.</summary>
        public bool IsDeleted { get; set; }
    }
}
