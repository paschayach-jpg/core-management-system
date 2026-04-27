using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Workflow notification service
    /// Handles notifications for workflow events
    /// </summary>
    public interface IWorkflowNotificationService
    {
        /// <summary>
        /// Send notification when a task is assigned
        /// </summary>
        void NotifyTaskAssignment(WorkflowStepInstance stepInstance, UserContext userContext);

        /// <summary>
        /// Send notification when SLA deadline is approaching
        /// </summary>
        void NotifySLAWarning(WorkflowStepInstance stepInstance, UserContext userContext);

        /// <summary>
        /// Send notification when SLA escalation is triggered
        /// </summary>
        void NotifyEscalation(WorkflowStepInstance stepInstance, EscalationAction escalationAction, UserContext userContext);

        /// <summary>
        /// Send notification when workflow instance is completed
        /// </summary>
        void NotifyWorkflowCompletion(WorkflowInstance instance, UserContext userContext);

        /// <summary>
        /// Send notification when a task is delegated
        /// </summary>
        void NotifyDelegation(WFDelegation delegation, UserContext userContext);

        /// <summary>
        /// Send notification when a task is rejected
        /// </summary>
        void NotifyTaskRejection(WorkflowStepInstance stepInstance, string reason, UserContext userContext);
    }
}
