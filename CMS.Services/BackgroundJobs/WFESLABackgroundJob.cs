using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Services.Interfaces;
using CMS.Services.Integrations;
using CMS.Infrastructure.Interfaces;
using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Services.BackgroundJobs
{
    /// <summary>
    /// Background job for monitoring SLA deadlines and triggering escalations
    /// Executes every 5 minutes
    /// </summary>
    public class WFESLABackgroundJob
    {
        private readonly IWorkflowStepInstanceRepository _stepInstanceRepository;
        private readonly ISLACalculator _slaCalculator;
        private readonly IWorkflowNotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private readonly NotificationAdapter _notificationAdapter;
        private readonly AuditAdapter _auditAdapter;

        public WFESLABackgroundJob(
            IWorkflowStepInstanceRepository stepInstanceRepository,
            ISLACalculator slaCalculator,
            IWorkflowNotificationService notificationService,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            _stepInstanceRepository = stepInstanceRepository;
            _slaCalculator = slaCalculator;
            _notificationService = notificationService;
            _auditService = auditService;
            _loggingService = loggingService;
            _notificationAdapter = new NotificationAdapter(null, loggingService);
            _auditAdapter = new AuditAdapter(auditService, loggingService);
        }

        /// <summary>
        /// Execute the SLA monitoring job
        /// </summary>
        public void Execute()
        {
            try
            {
                _loggingService.Info("WFESLABackgroundJob started");

                // 1. Get all pending step instances
                var pendingSteps = _stepInstanceRepository.GetPendingStepInstances();
                _loggingService.Info($"Found {pendingSteps.Count} pending step instances to evaluate");

                if (pendingSteps.Count == 0)
                {
                    _loggingService.Info("No pending steps to evaluate");
                    return;
                }

                int warningCount = 0;
                int escalationCount = 0;
                int errorCount = 0;

                // 2. For each step, evaluate SLA status
                foreach (var step in pendingSteps)
                {
                    try
                    {
                        // Evaluate SLA status
                        var slaStatus = _slaCalculator.EvaluateSLAStatus(step);
                        var previousStatus = step.SLAStatus;

                        // 3. If at warning threshold, send notification
                        if (slaStatus == SLAStatus.AtRisk && previousStatus != SLAStatus.AtRisk)
                        {
                            try
                            {
                                _notificationService.NotifySLAWarning(step, new UserContext { ADUserName = "System" });
                                warningCount++;
                                _loggingService.Warn($"SLA warning sent for step instance {step.WorkflowStepInstanceId}");
                            }
                            catch (Exception ex)
                            {
                                _loggingService.Error($"Error sending SLA warning for step {step.WorkflowStepInstanceId}: {ex.Message}", ex);
                            }
                        }

                        // 4. If overdue, trigger escalation
                        if (slaStatus == SLAStatus.Overdue && previousStatus != SLAStatus.Overdue)
                        {
                            try
                            {
                                var escalated = _slaCalculator.CheckAndEscalateSLA(step);
                                if (escalated)
                                {
                                    escalationCount++;
                                    _auditAdapter.LogSLAEscalation(step, "SLA Exceeded", new UserContext { ADUserName = "System" });
                                    _loggingService.Warn($"SLA escalation triggered for step instance {step.WorkflowStepInstanceId}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _loggingService.Error($"Error escalating SLA for step {step.WorkflowStepInstanceId}: {ex.Message}", ex);
                            }
                        }

                        // 5. Update SLA status
                        if (step.SLAStatus != slaStatus)
                        {
                            step.SLAStatus = slaStatus;
                            _stepInstanceRepository.UpdateStepInstance(step);
                            _loggingService.Debug($"SLA status updated for step {step.WorkflowStepInstanceId}: {slaStatus}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _loggingService.Error($"Error processing step instance {step.WorkflowStepInstanceId}: {ex.Message}", ex);
                    }
                }

                _loggingService.Info($"WFESLABackgroundJob completed: {warningCount} warnings, {escalationCount} escalations, {errorCount} errors");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"WFESLABackgroundJob failed: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Execute the job with error handling and logging
        /// </summary>
        public void ExecuteWithErrorHandling()
        {
            try
            {
                Execute();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Unhandled exception in WFESLABackgroundJob: {ex.Message}", ex);
                // Don't rethrow - background jobs should not crash the application
            }
        }
    }
}
