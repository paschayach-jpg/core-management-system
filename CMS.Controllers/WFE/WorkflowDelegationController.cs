using CMS.Services.Interfaces;
using CMS.Services.DTOs;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Models;
using System;
using System.Collections.Generic;

namespace CMS.Controllers
{
    /// <summary>
    /// Workflow delegation controller for task delegation management
    /// </summary>
    public class WorkflowDelegationController
    {
        private readonly IWorkflowDelegationService _delegationService;
        private readonly ILoggingService _loggingService;

        public WorkflowDelegationController(IWorkflowDelegationService delegationService = null, ILoggingService loggingService = null)
        {
            _delegationService = delegationService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Delegate a task to another user
        /// </summary>
        public WFDelegationResult DelegateTask(DelegateTaskRequest request, UserContext userContext)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Delegating task {request.WorkflowStepInstanceId} to {request.DelegateToUserName}");
                return _delegationService.DelegateTask(request, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error delegating task: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get delegation history for a step instance
        /// </summary>
        public List<WFDelegationResult> GetDelegationHistory(WorkflowStepInstance stepInstance)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                _loggingService?.Info($"Getting delegation history for step {stepInstance.WorkflowStepInstanceId}");
                return _delegationService.GetDelegationHistory(stepInstance.WorkflowStepInstanceId);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting delegation history: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get eligible delegates for a task
        /// </summary>
        public List<string> GetEligibleDelegates(WorkflowStepInstance stepInstance)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                _loggingService?.Info($"Getting eligible delegates for step {stepInstance.WorkflowStepInstanceId}");
                return _delegationService.GetEligibleDelegates(stepInstance);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting eligible delegates: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Validate if a delegation is allowed
        /// </summary>
        public ValidationResult ValidateDelegation(WorkflowStepInstance stepInstance, string delegateToUserName, UserContext userContext)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                if (string.IsNullOrEmpty(delegateToUserName))
                    throw new ArgumentNullException(nameof(delegateToUserName));

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Validating delegation for step {stepInstance.WorkflowStepInstanceId}");
                return _delegationService.ValidateDelegation(stepInstance, delegateToUserName, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error validating delegation: {ex.Message}", ex);
                throw;
            }
        }
    }
}
