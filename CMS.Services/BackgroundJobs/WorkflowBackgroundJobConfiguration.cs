using CMS.Data.Interfaces;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;
using System;

namespace CMS.Services.BackgroundJobs
{
    /// <summary>
    /// Configuration class for setting up workflow background jobs
    /// </summary>
    public class WorkflowBackgroundJobConfiguration
    {
        private readonly IBackgroundJobScheduler _scheduler;
        private readonly IWorkflowStepInstanceRepository _stepInstanceRepository;
        private readonly ISLACalculator _slaCalculator;
        private readonly IWorkflowNotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        public WorkflowBackgroundJobConfiguration(
            IBackgroundJobScheduler scheduler,
            IWorkflowStepInstanceRepository stepInstanceRepository,
            ISLACalculator slaCalculator,
            IWorkflowNotificationService notificationService,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            _scheduler = scheduler;
            _stepInstanceRepository = stepInstanceRepository;
            _slaCalculator = slaCalculator;
            _notificationService = notificationService;
            _auditService = auditService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Configure and start all workflow background jobs
        /// </summary>
        public void ConfigureAndStart()
        {
            try
            {
                _loggingService.Info("Configuring workflow background jobs");

                // Configure SLA monitoring job - runs every 5 minutes
                var slaJob = new WFESLABackgroundJob(
                    _stepInstanceRepository,
                    _slaCalculator,
                    _notificationService,
                    _auditService,
                    _loggingService);

                _scheduler.ScheduleJob(
                    "WFESLAMonitoring",
                    () => slaJob.ExecuteWithErrorHandling(),
                    TimeSpan.FromMinutes(5));

                // Start all jobs
                _scheduler.StartAll();

                _loggingService.Info("Workflow background jobs configured and started");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error configuring workflow background jobs: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Stop all workflow background jobs
        /// </summary>
        public void Stop()
        {
            try
            {
                _loggingService.Info("Stopping workflow background jobs");
                _scheduler.StopAll();
                _loggingService.Info("Workflow background jobs stopped");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error stopping workflow background jobs: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Check if SLA monitoring job is running
        /// </summary>
        public bool IsSLAMonitoringRunning()
        {
            return _scheduler.IsJobRunning("WFESLAMonitoring");
        }
    }
}
