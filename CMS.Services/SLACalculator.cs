using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Services
{
    /// <summary>
    /// SLA calculation and monitoring service implementation
    /// </summary>
    public class SLACalculator : ISLACalculator
    {
        private readonly IWorkflowStepInstanceRepository _stepInstanceRepository;
        private readonly IWFReportRepository _reportRepository;
        private readonly IWorkflowNotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        public SLACalculator(
            IWorkflowStepInstanceRepository stepInstanceRepository,
            IWFReportRepository reportRepository,
            IWorkflowNotificationService notificationService,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            _stepInstanceRepository = stepInstanceRepository;
            _reportRepository = reportRepository;
            _notificationService = notificationService;
            _auditService = auditService;
            _loggingService = loggingService;
        }

        public DateTime CalculateSLADeadline(WorkflowStepInstance stepInstance, WFSLARule slaRule)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                if (slaRule == null)
                    throw new ArgumentNullException(nameof(slaRule));

                var deadline = stepInstance.AssignedDate.AddHours(slaRule.SLADurationHours);
                return deadline;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error calculating SLA deadline: {ex.Message}", ex);
                throw;
            }
        }

        public SLAStatus EvaluateSLAStatus(WorkflowStepInstance stepInstance)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                var now = DateTime.Now;

                if (stepInstance.Status == WorkflowStepInstanceStatus.Completed)
                {
                    if (stepInstance.CompletedDate <= stepInstance.SLADeadline)
                        return SLAStatus.OnTime;
                    else
                        return SLAStatus.Overdue;
                }

                if (now > stepInstance.SLADeadline)
                    return SLAStatus.Overdue;

                var timeRemaining = stepInstance.SLADeadline - now;
                var warningThreshold = GetWarningThresholdPercent(stepInstance);
                var totalTime = stepInstance.SLADeadline - stepInstance.AssignedDate;
                var timeUsed = now - stepInstance.AssignedDate;
                var percentageUsed = (timeUsed.TotalSeconds / totalTime.TotalSeconds) * 100;

                if (percentageUsed >= warningThreshold)
                    return SLAStatus.AtRisk;

                return SLAStatus.OnTime;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error evaluating SLA status: {ex.Message}", ex);
                throw;
            }
        }

        public SLAComplianceMetricsResult GetSLAComplianceMetrics(SLAMetricsFilter filter)
        {
            try
            {
                // TODO: Implement detailed metrics calculation
                return new SLAComplianceMetricsResult
                {
                    TotalTasks = 0,
                    OnTimeCount = 0,
                    OverdueCount = 0,
                    AtRiskCount = 0,
                    CompliancePercentage = 0,
                    AverageCompletionHours = 0,
                    FromDate = filter.FromDate ?? DateTime.Now.AddMonths(-1),
                    ToDate = filter.ToDate ?? DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting SLA compliance metrics: {ex.Message}", ex);
                throw;
            }
        }

        public bool CheckAndEscalateSLA(WorkflowStepInstance stepInstance)
        {
            try
            {
                var status = EvaluateSLAStatus(stepInstance);

                if (status == SLAStatus.Overdue)
                {
                    // TODO: Implement escalation logic
                    _auditService.Log(new AuditEntry
                    {
                        Action = "SLAEscalated",
                        TableName = "WorkflowStepInstance",
                        RecordID = stepInstance.WorkflowStepInstanceId,
                        UserName = "System",
                        AuditDate = DateTime.Now,
                        NewValue = "SLA deadline exceeded"
                    });

                    _loggingService.Warn($"SLA escalated for step instance {stepInstance.WorkflowStepInstanceId}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error checking and escalating SLA: {ex.Message}", ex);
                throw;
            }
        }

        public int GetWarningThresholdPercent(WorkflowStepInstance stepInstance)
        {
            // Default warning threshold is 75%
            return 75;
        }

        public TimeSpan GetTimeRemaining(WorkflowStepInstance stepInstance)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                var remaining = stepInstance.SLADeadline - DateTime.Now;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting time remaining: {ex.Message}", ex);
                throw;
            }
        }
    }
}
