using System;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Workflow-specific permission checking service.
    /// Implements role-based access control (RBAC) for workflow operations.
    /// Supports Requirements 14.1 (Permission checks for workflow access and template modification)
    /// </summary>
    public interface IWorkflowPermissionService
    {
        /// <summary>
        /// Check if user can view a workflow instance.
        /// User can view if: initiator, assigned to a step, or has admin role.
        /// </summary>
        bool CanViewWorkflowInstance(WorkflowInstance instance, UserContext userContext);

        /// <summary>
        /// Check if user can create instances from a template.
        /// User can create if: has appropriate role or explicit permission.
        /// </summary>
        bool CanCreateWorkflowInstance(WorkflowDefinition template, UserContext userContext);

        /// <summary>
        /// Check if user can modify a workflow template.
        /// User can modify if: has admin role and no active instances exist.
        /// </summary>
        bool CanModifyTemplate(WorkflowDefinition template, UserContext userContext);

        /// <summary>
        /// Check if user can delete a workflow template.
        /// User can delete if: has admin role and no instances exist.
        /// </summary>
        bool CanDeleteTemplate(WorkflowDefinition template, UserContext userContext);

        /// <summary>
        /// Check if user can complete a workflow step.
        /// User can complete if: assigned to the step.
        /// </summary>
        bool CanCompleteWorkflowStep(WorkflowStepInstance stepInstance, UserContext userContext);

        /// <summary>
        /// Check if user can delegate a workflow step.
        /// User can delegate if: assigned to the step and delegation is allowed.
        /// </summary>
        bool CanDelegateWorkflowStep(WorkflowStepInstance stepInstance, UserContext userContext);

        /// <summary>
        /// Check if user can be assigned to a workflow step.
        /// User can be assigned if: has required role and is not at max delegation depth.
        /// </summary>
        bool CanBeAssignedToStep(WorkflowStep step, string userName, UserContext userContext);

        /// <summary>
        /// Check if user can view workflow audit logs.
        /// User can view if: has manager or admin role.
        /// </summary>
        bool CanViewWorkflowAuditLogs(UserContext userContext);

        /// <summary>
        /// Check if user can export workflow data.
        /// User can export if: has admin role.
        /// </summary>
        bool CanExportWorkflowData(UserContext userContext);

        /// <summary>
        /// Get user's workflow roles (e.g., "Approver", "Reviewer", "Admin").
        /// </summary>
        string[] GetUserWorkflowRoles(UserContext userContext);

        /// <summary>
        /// Check if user has a specific workflow role.
        /// </summary>
        bool HasWorkflowRole(UserContext userContext, string roleName);
    }
}
