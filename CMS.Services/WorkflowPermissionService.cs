using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Implements IWorkflowPermissionService: provides role-based access control for workflow operations.
    /// Supports Requirements 14.1 (Permission checks for workflow access and template modification).
    /// </summary>
    public class WorkflowPermissionService : IWorkflowPermissionService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowStepInstanceRepository _stepInstanceRepository;
        private readonly ILoggingService _loggingService;
        private readonly IAdAuthService _adAuthService;

        public WorkflowPermissionService(
            IWorkflowRepository workflowRepository,
            IWorkflowStepInstanceRepository stepInstanceRepository,
            ILoggingService loggingService,
            IAdAuthService adAuthService)
        {
            if (workflowRepository == null) throw new ArgumentNullException(nameof(workflowRepository));
            if (stepInstanceRepository == null) throw new ArgumentNullException(nameof(stepInstanceRepository));
            if (loggingService == null) throw new ArgumentNullException(nameof(loggingService));
            if (adAuthService == null) throw new ArgumentNullException(nameof(adAuthService));

            _workflowRepository = workflowRepository;
            _stepInstanceRepository = stepInstanceRepository;
            _loggingService = loggingService;
            _adAuthService = adAuthService;
        }

        /// <summary>
        /// Check if user can view a workflow instance.
        /// User can view if: initiator, assigned to a step, or has admin role.
        /// </summary>
        public bool CanViewWorkflowInstance(WorkflowInstance instance, UserContext userContext)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // Admin can view all
            if (userContext.AppRole >= 4) // Admin role
                return true;

            // Initiator can view their own
            if (instance.InitiatedBy == userContext.ADUserName)
                return true;

            // Check if user is assigned to any step in this workflow
            var stepInstances = _stepInstanceRepository.GetStepInstancesByWorkflow(instance.WorkflowInstanceId);
            if (stepInstances.Any(s => s.AssignedTo == userContext.ADUserName && !s.IsDeleted))
                return true;

            _loggingService.Warn($"User {userContext.ADUserName} denied access to view workflow instance {instance.WorkflowInstanceId}");
            return false;
        }

        /// <summary>
        /// Check if user can create instances from a template.
        /// User can create if: has appropriate role or explicit permission.
        /// </summary>
        public bool CanCreateWorkflowInstance(WorkflowDefinition template, UserContext userContext)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // Admin can create from any template
            if (userContext.AppRole >= 4) // Admin role
                return true;

            // Manager can create from templates
            if (userContext.AppRole >= 3) // Manager role
                return true;

            // Regular users can create from public templates
            // TODO: Implement template visibility/permission check
            return true;
        }

        /// <summary>
        /// Check if user can modify a workflow template.
        /// User can modify if: has admin role and no active instances exist.
        /// </summary>
        public bool CanModifyTemplate(WorkflowDefinition template, UserContext userContext)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // Only admin can modify templates
            if (userContext.AppRole < 4) // Admin role
            {
                _loggingService.Warn($"User {userContext.ADUserName} denied permission to modify template {template.WorkflowDefinitionId}");
                return false;
            }

            // Check if active instances exist
            var activeInstances = _workflowRepository.GetActiveWorkflowInstances()
                .Where(i => i.WorkflowDefinitionId == template.WorkflowDefinitionId && !i.IsDeleted)
                .ToList();

            if (activeInstances.Count > 0)
            {
                _loggingService.Warn($"Cannot modify template {template.WorkflowDefinitionId}: {activeInstances.Count} active instances exist");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if user can delete a workflow template.
        /// User can delete if: has admin role and no instances exist.
        /// </summary>
        public bool CanDeleteTemplate(WorkflowDefinition template, UserContext userContext)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // Only admin can delete templates
            if (userContext.AppRole < 4) // Admin role
            {
                _loggingService.Warn($"User {userContext.ADUserName} denied permission to delete template {template.WorkflowDefinitionId}");
                return false;
            }

            // Check if any instances exist (including completed)
            var allInstances = _workflowRepository.GetWorkflowInstances(
                new Data.Interfaces.WorkflowInstanceFilter { PageSize = 1000 })
                .Items
                .Where(i => i.WorkflowDefinitionId == template.WorkflowDefinitionId && !i.IsDeleted)
                .ToList();

            if (allInstances.Count > 0)
            {
                _loggingService.Warn($"Cannot delete template {template.WorkflowDefinitionId}: {allInstances.Count} instances exist");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if user can complete a workflow step.
        /// User can complete if: assigned to the step.
        /// </summary>
        public bool CanCompleteWorkflowStep(WorkflowStepInstance stepInstance, UserContext userContext)
        {
            if (stepInstance == null) throw new ArgumentNullException(nameof(stepInstance));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // Admin can complete any step
            if (userContext.AppRole >= 4) // Admin role
                return true;

            // User must be assigned to the step
            if (stepInstance.AssignedTo != userContext.ADUserName)
            {
                _loggingService.Warn($"User {userContext.ADUserName} denied permission to complete step {stepInstance.WorkflowStepInstanceId}");
                return false;
            }

            // Step must be pending or in progress
            if (stepInstance.Status != WorkflowStepInstanceStatus.Pending && 
                stepInstance.Status != WorkflowStepInstanceStatus.InProgress)
            {
                _loggingService.Warn($"Cannot complete step {stepInstance.WorkflowStepInstanceId}: status is {stepInstance.Status}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if user can delegate a workflow step.
        /// User can delegate if: assigned to the step and delegation is allowed.
        /// </summary>
        public bool CanDelegateWorkflowStep(WorkflowStepInstance stepInstance, UserContext userContext)
        {
            if (stepInstance == null) throw new ArgumentNullException(nameof(stepInstance));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // User must be assigned to the step
            if (stepInstance.AssignedTo != userContext.ADUserName)
            {
                _loggingService.Warn($"User {userContext.ADUserName} denied permission to delegate step {stepInstance.WorkflowStepInstanceId}");
                return false;
            }

            // Step must be pending or in progress
            if (stepInstance.Status != WorkflowStepInstanceStatus.Pending && 
                stepInstance.Status != WorkflowStepInstanceStatus.InProgress)
            {
                _loggingService.Warn($"Cannot delegate step {stepInstance.WorkflowStepInstanceId}: status is {stepInstance.Status}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if user can be assigned to a workflow step.
        /// User can be assigned if: has required role and is not at max delegation depth.
        /// </summary>
        public bool CanBeAssignedToStep(WorkflowStep step, string userName, UserContext userContext)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException(nameof(userName));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // Check if user has required role
            if (!string.IsNullOrEmpty(step.AssignedRole))
            {
                var userRoles = GetUserWorkflowRoles(userContext);
                if (!userRoles.Contains(step.AssignedRole))
                {
                    _loggingService.Warn($"User {userName} does not have required role {step.AssignedRole} for step {step.WorkflowStepId}");
                    return false;
                }
            }

            // Check if user is specifically assigned
            if (!string.IsNullOrEmpty(step.AssignedUser) && step.AssignedUser != userName)
            {
                _loggingService.Warn($"User {userName} is not the assigned user for step {step.WorkflowStepId}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if user can view workflow audit logs.
        /// User can view if: has manager or admin role.
        /// </summary>
        public bool CanViewWorkflowAuditLogs(UserContext userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // Manager (3) or Admin (4) can view audit logs
            return userContext.AppRole >= 3;
        }

        /// <summary>
        /// Check if user can export workflow data.
        /// User can export if: has admin role.
        /// </summary>
        public bool CanExportWorkflowData(UserContext userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // Only admin can export
            return userContext.AppRole >= 4;
        }

        /// <summary>
        /// Get user's workflow roles (e.g., "Approver", "Reviewer", "Admin").
        /// </summary>
        public string[] GetUserWorkflowRoles(UserContext userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            var roles = new List<string>();

            // Map app roles to workflow roles
            if (userContext.AppRole >= 4) // Admin
            {
                roles.Add("Admin");
                roles.Add("Approver");
                roles.Add("Reviewer");
            }
            else if (userContext.AppRole >= 3) // Manager
            {
                roles.Add("Manager");
                roles.Add("Approver");
                roles.Add("Reviewer");
            }
            else if (userContext.AppRole >= 2) // User
            {
                roles.Add("User");
                roles.Add("Reviewer");
            }
            else
            {
                roles.Add("Guest");
            }

            return roles.ToArray();
        }

        /// <summary>
        /// Check if user has a specific workflow role.
        /// </summary>
        public bool HasWorkflowRole(UserContext userContext, string roleName)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            var userRoles = GetUserWorkflowRoles(userContext);
            return userRoles.Contains(roleName);
        }
    }
}
