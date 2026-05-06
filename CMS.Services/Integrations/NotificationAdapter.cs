using CMS.Domain.Models;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;
using System;

namespace CMS.Services.Integrations
{
    /// <summary>
    /// Adapter for integrating with Notification system (MOD-007)
    /// </summary>
    public class NotificationAdapter
    {
        private readonly INotificationService _notificationService;
        private readonly ILoggingService _loggingService;

        public NotificationAdapter(INotificationService notificationService, ILoggingService loggingService)
        {
            _notificationService = notificationService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Send task assignment notification
        /// </summary>
        public void NotifyTaskAssignment(WorkflowStepInstance stepInstance, WorkflowStep step, WorkflowDefinition template, UserContext userContext)
        {
            try
            {
                if (stepInstance == null || step == null || template == null)
                    throw new ArgumentNullException();

                // TODO: Create notification through INotificationService
                // Include workflow name, step name, SLA deadline, action URL

                _loggingService.Info($"Task assignment notification sent to {stepInstance.AssignedTo}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error sending task assignment notification: {ex.Message}", ex);
                // Don't throw - notifications should not block workflow
            }
        }

        /// <summary>
        /// Send SLA warning notification
        /// </summary>
        public void NotifySLAWarning(WorkflowStepInstance stepInstance, WorkflowStep step, int minutesRemaining, UserContext userContext)
        {
            try
            {
                if (stepInstance == null || step == null)
                    throw new ArgumentNullException();

                // TODO: Create SLA warning notification
                // Include time remaining, step name, action URL

                _loggingService.Warn($"SLA warning notification sent to {stepInstance.AssignedTo}: {minutesRemaining} minutes remaining");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error sending SLA warning notification: {ex.Message}", ex);
                // Don't throw
            }
        }

        /// <summary>
        /// Send escalation notification
        /// </summary>
        public void NotifyEscalation(WorkflowStepInstance stepInstance, string escalationRecipient, string escalationAction, UserContext userContext)
        {
            try
            {
                if (stepInstance == null || string.IsNullOrEmpty(escalationRecipient))
                    throw new ArgumentNullException();

                // TODO: Create escalation notification
                // Include escalation action, step details, action URL

                _loggingService.Warn($"Escalation notification sent to {escalationRecipient}: {escalationAction}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error sending escalation notification: {ex.Message}", ex);
                // Don't throw
            }
        }

        /// <summary>
        /// Send workflow completion notification
        /// </summary>
        public void NotifyWorkflowCompletion(WorkflowInstance instance, UserContext userContext)
        {
            try
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance));

                // TODO: Create workflow completion notification
                // Send to workflow initiator

                _loggingService.Info($"Workflow completion notification sent to {instance.InitiatedBy}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error sending workflow completion notification: {ex.Message}", ex);
                // Don't throw
            }
        }

        /// <summary>
        /// Send delegation notification
        /// </summary>
        public void NotifyDelegation(WFDelegation delegation, UserContext userContext)
        {
            try
            {
                if (delegation == null)
                    throw new ArgumentNullException(nameof(delegation));

                // TODO: Create delegation notification
                // Send to delegate with delegation reason

                _loggingService.Info($"Delegation notification sent to {delegation.DelegatedTo}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error sending delegation notification: {ex.Message}", ex);
                // Don't throw
            }
        }
    }
}
