using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Services
{
    /// <summary>
    /// Workflow task delegation service implementation
    /// </summary>
    public class WorkflowDelegationService : IWorkflowDelegationService
    {
        private readonly IDelegationRepository _delegationRepository;
        private readonly IWorkflowStepInstanceRepository _stepInstanceRepository;
        private readonly IWorkflowNotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        public WorkflowDelegationService(
            IDelegationRepository delegationRepository,
            IWorkflowStepInstanceRepository stepInstanceRepository,
            IWorkflowNotificationService notificationService,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            _delegationRepository = delegationRepository;
            _stepInstanceRepository = stepInstanceRepository;
            _notificationService = notificationService;
            _auditService = auditService;
            _loggingService = loggingService;
        }

        public WFDelegationResult DelegateTask(DelegateTaskRequest request, UserContext userContext)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                // Get step instance
                var stepInstance = _stepInstanceRepository.GetStepInstance(request.WorkflowStepInstanceId);
                if (stepInstance == null)
                    throw new Exception($"Step instance {request.WorkflowStepInstanceId} not found");

                // Validate delegation
                var validationResult = ValidateDelegation(stepInstance, request.DelegateToUserName, userContext);
                if (!validationResult.IsValid)
                    throw new Exception(string.Join(", ", validationResult.Errors));

                // Get current delegation depth
                var delegationHistory = GetDelegationHistory(request.WorkflowStepInstanceId);
                var currentDepth = delegationHistory.Count;

                // Create delegation record
                var delegation = new WFDelegation
                {
                    WorkflowStepInstanceId = request.WorkflowStepInstanceId,
                    DelegatedFrom = userContext.ADUserName,
                    DelegatedTo = request.DelegateToUserName,
                    DelegationReason = request.DelegationReason,
                    DelegationDate = DateTime.Now,
                    DelegationDepth = currentDepth + 1,
                    IsActive = true,
                    IsDeleted = false
                };

                var delegationId = _delegationRepository.CreateDelegation(delegation, delegation.DelegationDepth);
                delegation.WFDelegationId = delegationId;

                // Update step instance assignment
                stepInstance.AssignedTo = request.DelegateToUserName;
                stepInstance.Status = Domain.Enums.WorkflowStepInstanceStatus.Delegated;
                _stepInstanceRepository.UpdateStepInstance(stepInstance);

                // Send notification
                _notificationService.NotifyDelegation(delegation, userContext);

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "TaskDelegated",
                    TableName = "WFDelegation",
                    RecordID = delegationId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Task delegated from {userContext.ADUserName} to {request.DelegateToUserName}"
                });

                _loggingService.Info($"Task {request.WorkflowStepInstanceId} delegated from {userContext.ADUserName} to {request.DelegateToUserName}");

                return MapToResult(delegation);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error delegating task: {ex.Message}", ex);
                throw;
            }
        }

        public List<WFDelegationResult> GetDelegationHistory(int stepInstanceId)
        {
            try
            {
                var delegations = _delegationRepository.GetDelegationsByStepInstance(stepInstanceId);
                return delegations.Select(MapToResult).ToList();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting delegation history: {ex.Message}", ex);
                throw;
            }
        }

        public ValidationResult ValidateDelegation(WorkflowStepInstance stepInstance, string delegateToUserName, UserContext userContext)
        {
            try
            {
                var result = new ValidationResult { IsValid = true };

                if (stepInstance == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("Step instance not found");
                    return result;
                }

                if (string.IsNullOrEmpty(delegateToUserName))
                {
                    result.IsValid = false;
                    result.Errors.Add("Delegate user name is required");
                    return result;
                }

                // Check if user is assigned to the step
                if (stepInstance.AssignedTo != userContext.ADUserName)
                {
                    result.IsValid = false;
                    result.Errors.Add("You are not assigned to this step");
                    return result;
                }

                // Check if delegation is allowed
                if (!CanDelegate(stepInstance, userContext.ADUserName))
                {
                    result.IsValid = false;
                    result.Errors.Add("Delegation is not allowed for this step");
                    return result;
                }

                // Check delegation depth
                var delegationHistory = GetDelegationHistory(stepInstance.WorkflowStepInstanceId);
                if (delegationHistory.Count >= 3) // Default max depth
                {
                    result.IsValid = false;
                    result.Errors.Add("Maximum delegation depth exceeded");
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error validating delegation: {ex.Message}", ex);
                throw;
            }
        }

        public List<string> GetEligibleDelegates(WorkflowStepInstance stepInstance)
        {
            try
            {
                // TODO: Implement logic to get eligible delegates based on role
                return new List<string>();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting eligible delegates: {ex.Message}", ex);
                throw;
            }
        }

        public void DeactivateDelegation(int delegationId, UserContext userContext)
        {
            try
            {
                var delegation = _delegationRepository.GetDelegation(delegationId);
                if (delegation == null)
                    throw new Exception($"Delegation {delegationId} not found");

                delegation.IsActive = false;
                _delegationRepository.UpdateDelegation(delegation);

                _auditService.Log(new AuditEntry
                {
                    Action = "DelegationDeactivated",
                    TableName = "WFDelegation",
                    RecordID = delegationId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now
                });

                _loggingService.Info($"Delegation {delegationId} deactivated by {userContext.ADUserName}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error deactivating delegation: {ex.Message}", ex);
                throw;
            }
        }

        public bool CanDelegate(WorkflowStepInstance stepInstance, string delegateFromUserName)
        {
            try
            {
                // TODO: Implement logic to check if delegation is allowed
                // Check step configuration, user role, etc.
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error checking delegation eligibility: {ex.Message}", ex);
                throw;
            }
        }

        private WFDelegationResult MapToResult(WFDelegation delegation)
        {
            return new WFDelegationResult
            {
                WFDelegationId = delegation.WFDelegationId,
                WorkflowStepInstanceId = delegation.WorkflowStepInstanceId,
                DelegatedFrom = delegation.DelegatedFrom,
                DelegatedTo = delegation.DelegatedTo,
                DelegationReason = delegation.DelegationReason,
                DelegationDate = delegation.DelegationDate,
                DelegationDepth = delegation.DelegationDepth,
                IsActive = delegation.IsActive
            };
        }
    }
}
