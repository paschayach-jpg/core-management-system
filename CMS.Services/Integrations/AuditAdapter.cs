using CMS.Domain.Models;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;
using System;

namespace CMS.Services.Integrations
{
    /// <summary>
    /// Adapter for integrating with Audit Logging (MOD-020)
    /// </summary>
    public class AuditAdapter
    {
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        public AuditAdapter(IAuditService auditService, ILoggingService loggingService)
        {
            _auditService = auditService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Log workflow instance creation
        /// </summary>
        public void LogWorkflowInstanceCreated(WorkflowInstance instance, UserContext userContext)
        {
            try
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance));

                // TODO: Create audit entry for workflow instance creation

                _loggingService.Info($"Workflow instance {instance.WorkflowInstanceId} creation logged");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error logging workflow instance creation: {ex.Message}", ex);
                // Don't throw - audit logging should not block workflow
            }
        }

        /// <summary>
        /// Log workflow step completion
        /// </summary>
        public void LogStepCompleted(WorkflowStepInstance stepInstance, UserContext userContext)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                // TODO: Create audit entry for step completion

                _loggingService.Info($"Workflow step {stepInstance.WorkflowStepInstanceId} completion logged");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error logging step completion: {ex.Message}", ex);
                // Don't throw
            }
        }

        /// <summary>
        /// Log workflow step rejection
        /// </summary>
        public void LogStepRejected(WorkflowStepInstance stepInstance, string reason, UserContext userContext)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                // TODO: Create audit entry for step rejection

                _loggingService.Info($"Workflow step {stepInstance.WorkflowStepInstanceId} rejection logged: {reason}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error logging step rejection: {ex.Message}", ex);
                // Don't throw
            }
        }

        /// <summary>
        /// Log task delegation
        /// </summary>
        public void LogTaskDelegated(WFDelegation delegation, UserContext userContext)
        {
            try
            {
                if (delegation == null)
                    throw new ArgumentNullException(nameof(delegation));

                // TODO: Create audit entry for task delegation

                _loggingService.Info($"Task delegation {delegation.WFDelegationId} logged");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error logging task delegation: {ex.Message}", ex);
                // Don't throw
            }
        }

        /// <summary>
        /// Log SLA escalation
        /// </summary>
        public void LogSLAEscalation(WorkflowStepInstance stepInstance, string escalationAction, UserContext userContext)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                // TODO: Create audit entry for SLA escalation

                _loggingService.Warn($"SLA escalation logged for step {stepInstance.WorkflowStepInstanceId}: {escalationAction}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error logging SLA escalation: {ex.Message}", ex);
                // Don't throw
            }
        }

        /// <summary>
        /// Log workflow instance completion
        /// </summary>
        public void LogWorkflowInstanceCompleted(WorkflowInstance instance, UserContext userContext)
        {
            try
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance));

                // TODO: Create audit entry for workflow completion

                _loggingService.Info($"Workflow instance {instance.WorkflowInstanceId} completion logged");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error logging workflow instance completion: {ex.Message}", ex);
                // Don't throw
            }
        }
    }
}
