using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request to complete a workflow step
    /// </summary>
    public class CompleteWorkflowStepRequest
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
        /// Output data from step completion (JSON string)
        /// </summary>
        public string OutputData { get; set; }

        /// <summary>
        /// Optional remarks or comments
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Optional attachment IDs
        /// </summary>
        public int[] AttachmentIds { get; set; }
    }
}
