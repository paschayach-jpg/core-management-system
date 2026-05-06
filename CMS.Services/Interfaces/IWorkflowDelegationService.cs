using CMS.Domain.Models;
using CMS.Services.DTOs;
using System.Collections.Generic;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Workflow task delegation service
    /// Handles task delegation and delegation rules
    /// </summary>
    public interface IWorkflowDelegationService
    {
        /// <summary>
        /// Delegate a task to another user
        /// </summary>
        WFDelegationResult DelegateTask(DelegateTaskRequest request, UserContext userContext);

        /// <summary>
        /// Get delegation history for a step instance
        /// </summary>
        List<WFDelegationResult> GetDelegationHistory(int stepInstanceId);

        /// <summary>
        /// Validate if a delegation is allowed
        /// </summary>
        ValidationResult ValidateDelegation(WorkflowStepInstance stepInstance, string delegateToUserName, UserContext userContext);

        /// <summary>
        /// Get eligible delegates for a task
        /// </summary>
        List<string> GetEligibleDelegates(WorkflowStepInstance stepInstance);

        /// <summary>
        /// Deactivate a delegation
        /// </summary>
        void DeactivateDelegation(int delegationId, UserContext userContext);

        /// <summary>
        /// Check if a user can delegate a task
        /// </summary>
        bool CanDelegate(WorkflowStepInstance stepInstance, string delegateFromUserName);
    }
}
