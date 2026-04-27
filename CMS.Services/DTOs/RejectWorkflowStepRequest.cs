using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request to reject a workflow step
    /// </summary>
    public class RejectWorkflowStepRequest
    {
        /// <summary>
        /// Workflow instance ID
        /// </summary>
        public int WorkflowInstanceId { get; set; }

        /// <summary>
        /// Workflow step instance ID
        /// </summary>
        public int WorkflowStepInstanceId { get; set; }

        /// <summary>
        /// Reason for rejection
        /// </summary>
        public string RejectionReason { get; set; }

        /// <summary>
        /// Optional remarks
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Target step to route to (optional, defaults to previous step)
        /// </summary>
        public int? TargetStepId { get; set; }
    }
}
