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
                var reportFilter = new CMS.Data.Interfaces.WorkflowReportFilter
                {
                    WorkflowDefinitionId = filter.WorkflowDefinitionId,
                    FromDate             = filter.FromDate,
                    ToDate               = filter.ToDate,
                    PageNumber           = 1,
                    PageSize             = 200,
                };

                // Aggregate metrics
                var agg = _reportRepository.GetSLAComplianceMetrics(reportFilter);

                // Per-template breakdown
                var templateRows = _reportRepository.GetSLAByTemplate(reportFilter);
                var byTemplate   = new System.Collections.Generic.List<TemplateMetrics>();
                foreach (var r in templateRows)
                {
                    int total = r.Total > 0 ? r.Total : 1;
                    byTemplate.Add(new TemplateMetrics
                    {
                        WorkflowDefinitionId = r.WorkflowDefinitionId,
                        TemplateName         = r.TemplateName,
                        TotalTasks           = r.Total,
                        OnTimeCount          = r.OnTime,
                        AtRiskCount          = r.AtRisk,
                        OverdueCount         = r.Overdue,
                        CompliancePercentage = (decimal)((r.OnTime + r.AtRisk) * 100.0 / total),
                    });
                }

                // Per-user (top overdue) — derived from task distribution
                var distRows = _reportRepository.GetTaskDistribution(reportFilter);
                var byUser   = new System.Collections.Generic.List<UserMetrics>();
                // Sort by overdue descending, take top 5
                var sortedDist = new System.Collections.Generic.List<CMS.Data.Interfaces.TaskDistributionRow>(distRows.Items);
                sortedDist.Sort((a, b) => b.Overdue.CompareTo(a.Overdue));
                foreach (var u in sortedDist)
                {
                    if (u.Overdue == 0) break;
                    byUser.Add(new UserMetrics
                    {
                        UserName               = u.AssignedTo,
                        Department             = string.Empty,
                        TotalTasks             = u.TotalAssigned,
                        OnTimeCount            = u.Completed - u.Overdue,
                        OverdueCount           = u.Overdue,
                        AverageCompletionHours = 0,
                        AvgOverdueDays         = u.Overdue > 0 ? 1.5 : 0, // placeholder until we have SLADeadline delta
                    });
                    if (byUser.Count >= 5) break;
                }

                // Weekly trend (last 12 weeks)
                var trendRows = _reportRepository.GetSLAWeeklyTrend(12);
                var weeklyTrend = new System.Collections.Generic.List<WeeklyTrendPoint>();
                foreach (var t in trendRows)
                    weeklyTrend.Add(new WeeklyTrendPoint { WeekLabel = t.WeekLabel, CompliancePercent = t.CompliancePercent });

                // Cycle time distribution
                var cycleRows = _reportRepository.GetCycleTimeDistribution(reportFilter);
                var cycleBuckets = new System.Collections.Generic.List<CycleTimeBucket>();
                foreach (var c in cycleRows)
                {
                    string cat = c.SortKey <= 2 ? "pass" : c.SortKey <= 4 ? "near" : "over";
                    cycleBuckets.Add(new CycleTimeBucket { Label = c.Label, Count = c.Count, Category = cat });
                }

                int totalSteps = agg.TotalSteps > 0 ? agg.TotalSteps : 1;
                return new SLAComplianceMetricsResult
                {
                    TotalTasks             = agg.TotalSteps,
                    OnTimeCount            = agg.OnTimeCount,
                    OverdueCount           = agg.OverdueCount,
                    AtRiskCount            = agg.AtRiskCount,
                    CompliancePercentage   = (decimal)(agg.OnTimePercent),
                    AverageCompletionHours = (decimal)(agg.AvgCompletionHours),
                    FromDate               = filter.FromDate ?? DateTime.Now.AddMonths(-1),
                    ToDate                 = filter.ToDate   ?? DateTime.Now,
                    ByTemplate             = byTemplate,
                    ByUser                 = byUser,
                    WeeklyTrend            = weeklyTrend,
                    CycleTimeDistribution  = cycleBuckets,
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error("Error getting SLA compliance metrics: " + ex.Message, ex);
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
