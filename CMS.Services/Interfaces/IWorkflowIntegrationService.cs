using CMS.Domain.Models;
using CMS.Services.DTOs;
using System;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Workflow integration service for coordinating with other modules
    /// </summary>
    public interface IWorkflowIntegrationService
    {
        // MOD-003: Approval Workflow Integration
        /// <summary>
        /// Create an approval task in the Approval Workflow module
        /// </summary>
        int CreateApprovalTask(WorkflowStepInstance stepInstance, UserContext userContext);

        /// <summary>
        /// Handle approval completion callback
        /// </summary>
        void OnApprovalCompleted(int approvalId, bool isApproved, string reason, UserContext userContext);

        // MOD-006: File Management Integration
        /// <summary>
        /// Attach files to a workflow step instance
        /// </summary>
        void AttachFilesToStep(int stepInstanceId, int[] fileIds, UserContext userContext);

        /// <summary>
        /// Get files attached to a workflow step instance
        /// </summary>
        int[] GetAttachedFiles(int stepInstanceId);

        // MOD-007: Notifications Integration
        /// <summary>
        /// Send workflow notification through notification system
        /// </summary>
        void SendWorkflowNotification(string recipientUser, string title, string message, string actionUrl, UserContext userContext);

        // MOD-009: Task Management Integration
        /// <summary>
        /// Create a task in Task Management module for a workflow step
        /// </summary>
        int CreateTaskManagementTask(WorkflowStepInstance stepInstance, UserContext userContext);

        /// <summary>
        /// Update task in Task Management module
        /// </summary>
        void UpdateTaskManagementTask(int stepInstanceId, string status, UserContext userContext);

        /// <summary>
        /// Handle task completion from Task Management module
        /// </summary>
        void OnTaskManagementTaskCompleted(int stepInstanceId, UserContext userContext);

        // MOD-012: Organizational Structure Integration
        /// <summary>
        /// Get users in an organizational unit
        /// </summary>
        string[] GetUsersInOrgUnit(string orgUnitId);

        /// <summary>
        /// Get manager of a user
        /// </summary>
        string GetUserManager(string userName);

        // MOD-013: User Management Integration
        /// <summary>
        /// Get user roles
        /// </summary>
        string[] GetUserRoles(string userName);

        /// <summary>
        /// Check if user has a specific role
        /// </summary>
        bool UserHasRole(string userName, string roleName);

        // MOD-020: Audit Logging Integration
        /// <summary>
        /// Log workflow action to audit trail
        /// </summary>
        void LogWorkflowAction(string action, string entityType, int entityId, string details, UserContext userContext);

        // MOD-021: Configuration Management Integration
        /// <summary>
        /// Get workflow configuration setting
        /// </summary>
        string GetConfigurationSetting(string key);

        /// <summary>
        /// Get default SLA hours for a step type
        /// </summary>
        int GetDefaultSLAHours(string stepType);
    }
}
