using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Services.Interfaces;
using CMS.Services.Integrations;
using CMS.Infrastructure.Interfaces;
using CMS.Domain.Models;
using CMS.Services.SLA;
using CMS.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CMS.Services.BackgroundJobs
{
    /// <summary>
    /// Optimized background job for monitoring SLA deadlines with batch processing
    /// Supports Phase 13: Performance Optimization
    /// Requirement 7 AC1-7: SLA Management
    /// Requirement 21 AC4-5: Performance targets for SLA evaluation
    /// 
    /// Optimization Techniques:
    /// - Batch SLA evaluation reduces database round-trips
    /// - Batch escalation processing groups actions
    /// - Async notification delivery decouples from SLA processing
    /// - Efficient error handling and retry logic
    /// 
    /// Execution: Every 5 minutes
    /// Target Performance: Complete SLA evaluation for all active instances within 5 minutes
    /// </summary>
    public class OptimizedWFESLABackgroundJob
    {
        private readonly IWorkflowStepInstanceRepository _stepInstanceRepository;
        private readonly IOptimizedSLACalculator _slaCalculator;
        private readonly IAsyncNotificationQueue _notificationQueue;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private readonly AuditAdapter _auditAdapter;

        // Configuration
        private const int BATCH_SIZE = 100;
        private const int MAX_EXECUTION_TIME_SECONDS = 300; // 5 minutes

        public OptimizedWFESLABackgroundJob(
            IWorkflowStepInstanceRepository stepInstanceRepository,
            IOptimizedSLACalculator slaCalculator,
            IAsyncNotificationQueue notificationQueue,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            _stepInstanceRepository = stepInstanceRepository;
            _slaCalculator = slaCalculator;
            _notificationQueue = notificationQueue;
            _auditService = auditService;
            _loggingService = loggingService;
            _auditAdapter = new AuditAdapter(auditService, loggingService);
        }

        /// <summary>
        /// Execute the optimized SLA monitoring job
        /// </summary>
        public void Execute()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _loggingService.Info("OptimizedWFESLABackgroundJob started");

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
                int batchCount = 0;

                // 2. Process steps in batches
                for (int i = 0; i < pendingSteps.Count; i += BATCH_SIZE)
                {
                    // Check execution time limit
                    if (stopwatch.Elapsed.TotalSeconds > MAX_EXECUTION_TIME_SECONDS)
                    {
                        _loggingService.Warn($"SLA background job exceeded maximum execution time ({MAX_EXECUTION_TIME_SECONDS}s). Processed {i}/{pendingSteps.Count} steps.");
                        break;
                    }

                    var batch = pendingSteps.Skip(i).Take(BATCH_SIZE).ToList();
                    batchCount++;

                    try
                    {
                        // Batch evaluate SLA status
                        var evaluationResults = _slaCalculator.BatchEvaluateSLAStatus(batch);

                        // Separate results by action needed
                        var warningSteps = new List<WorkflowStepInstance>();
                        var escalationSteps = new List<WorkflowStepInstance>();
                        var statusUpdates = new List<(WorkflowStepInstance, SLAStatus)>();

                        foreach (var result in evaluationResults)
                        {
                            var step = batch.FirstOrDefault(s => s.WorkflowStepInstanceId == result.WorkflowStepInstanceId);
                            if (step == null)
                                continue;

                            // Track status changes
                            if (step.SLAStatus != result.Status)
                                statusUpdates.Add((step, result.Status));

                            // Collect steps needing warnings
                            if (result.RequiresWarning && step.SLAStatus != SLAStatus.AtRisk)
                                warningSteps.Add(step);

                            // Collect steps needing escalation
                            if (result.RequiresEscalation && step.SLAStatus != SLAStatus.Overdue)
                                escalationSteps.Add(step);
                        }

                        // 3. Send batch warnings asynchronously
                        if (warningSteps.Count > 0)
                        {
                            var notifications = warningSteps.Select(s => new NotificationMessage
                            {
                                RecipientUser = s.AssignedTo,
                                Title = "SLA Warning",
                                Message = $"Task deadline approaching",
                                Type = NotificationType.SLAWarning,
                                WorkflowStepInstanceId = s.WorkflowStepInstanceId,
                                DueDate = s.SLADeadline
                            }).ToList();

                            _notificationQueue.EnqueueNotifications(notifications);
                            warningCount += notifications.Count;
                            _loggingService.Warn($"Batch {batchCount}: {notifications.Count} SLA warnings enqueued");
                        }

                        // 4. Batch escalate overdue steps
                        if (escalationSteps.Count > 0)
                        {
                            var escalationResults = _slaCalculator.BatchCheckAndEscalateSLA(escalationSteps);
                            escalationCount += escalationResults.Count(r => r.Success);
                            errorCount += escalationResults.Count(r => !r.Success);
                            _loggingService.Warn($"Batch {batchCount}: {escalationResults.Count(r => r.Success)} escalations triggered");
                        }

                        // 5. Batch update SLA statuses
                        if (statusUpdates.Count > 0)
                        {
                            foreach (var (step, newStatus) in statusUpdates)
                            {
                                step.SLAStatus = newStatus;
                                _stepInstanceRepository.UpdateStepInstance(step);
                            }
                            _loggingService.Debug($"Batch {batchCount}: {statusUpdates.Count} SLA statuses updated");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _loggingService.Error($"Error processing batch {batchCount}: {ex.Message}", ex);
                    }
                }

                stopwatch.Stop();

                _loggingService.Info($"OptimizedWFESLABackgroundJob completed in {stopwatch.ElapsedMilliseconds}ms: " +
                    $"{warningCount} warnings, {escalationCount} escalations, {errorCount} errors, {batchCount} batches");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _loggingService.Error($"OptimizedWFESLABackgroundJob failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}", ex);
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
                _loggingService.Error($"Unhandled exception in OptimizedWFESLABackgroundJob: {ex.Message}", ex);
                // Don't rethrow - background jobs should not crash the application
            }
        }
    }
}
