using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result DTO for workflow delegation
    /// </summary>
    public class WFDelegationResult
    {
        /// <summary>
        /// Delegation ID
        /// </summary>
        public int WFDelegationId { get; set; }

        /// <summary>
        /// Step instance ID
        /// </summary>
        public int WorkflowStepInstanceId { get; set; }

        /// <summary>
        /// User who delegated the task
        /// </summary>
        public string DelegatedFrom { get; set; }

        /// <summary>
        /// User who received the delegation
        /// </summary>
        public string DelegatedTo { get; set; }

        /// <summary>
        /// Reason for delegation
        /// </summary>
        public string DelegationReason { get; set; }

        /// <summary>
        /// Date of delegation
        /// </summary>
        public DateTime DelegationDate { get; set; }

        /// <summary>
        /// Delegation depth (how many times delegated)
        /// </summary>
        public int DelegationDepth { get; set; }

        /// <summary>
        /// Whether delegation is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Remarks
        /// </summary>
        public string Remarks { get; set; }
    }
}
