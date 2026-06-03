using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request to delegate a workflow task
    /// </summary>
    public class DelegateTaskRequest
    {
        /// <summary>
        /// Workflow step instance ID to delegate
        /// </summary>
        public int WorkflowStepInstanceId { get; set; }

        /// <summary>
        /// User to delegate to (AD username)
        /// </summary>
        public string DelegateToUserName { get; set; }

        /// <summary>
        /// Reason for delegation
        /// </summary>
        public string DelegationReason { get; set; }

        /// <summary>
        /// Optional remarks
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Start date of delegation
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date of delegation
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
