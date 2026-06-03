using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Models;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow.Presenters
{
    /// <summary>
    /// Presenter for Workflow Monitor (Administrative dashboard)
    /// </summary>
    public class WorkflowMonitorPresenter
    {
        private readonly IWorkflowMonitorView _view;
        private readonly IWorkflowService _workflowService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;

        public WorkflowMonitorPresenter(
            IWorkflowMonitorView view,
            IWorkflowService workflowService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Load and display system-wide metrics
        /// </summary>
        public void LoadMetrics()
        {
            try
            {
                var filter = new WorkflowInstanceFilter
                {
                    PageNumber = 1,
                    PageSize = 10000
                };

                var instances = _workflowService.GetWorkflowInstances(filter, _userContext);
                var metrics = CalculateMetrics(instances.Items.ToList());
                _view.DisplayMetrics(metrics);
                _view.DisplayWorkflowInstances(instances.Items.ToList());

                _loggingService.Info("Loaded workflow monitor metrics and instances list");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading metrics: {ex.Message}", ex);
                _view.ShowError($"Failed to load metrics: {ex.Message}");
            }
        }

        /// <summary>
        /// Load and display SLA compliance chart
        /// </summary>
        public void LoadSLAChart()
        {
            try
            {
                var filter = new WorkflowInstanceFilter
                {
                    PageNumber = 1,
                    PageSize = 10000
                };

                var instances = _workflowService.GetWorkflowInstances(filter, _userContext);
                var chartData = GenerateSLAChartData(instances.Items.ToList());
                _view.DisplaySLAChart(chartData);

                _loggingService.Info("Loaded SLA compliance chart data");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading SLA chart: {ex.Message}", ex);
                _view.ShowError($"Failed to load SLA chart: {ex.Message}");
            }
        }

        /// <summary>
        /// Load and display task distribution
        /// </summary>
        public void LoadTaskDistribution()
        {
            try
            {
                var distribution = new List<TaskDistributionData>();

                try
                {
                    var instances = _workflowService.GetWorkflowInstances(new WorkflowInstanceFilter(), _userContext);
                    if (instances?.Items != null)
                    {
                        var tasksByUser = new Dictionary<string, (int assigned, int completed, int pending, double avgTime)>();

                        foreach (var instance in instances.Items)
                        {
                            var fullInstance = _workflowService.GetWorkflowInstance(instance.WorkflowInstanceId, _userContext);
                            var steps = fullInstance?.StepHistory;
                            if (steps != null)
                            {
                                foreach (var step in steps)
                                {
                                    string user = step.AssignedTo ?? "Unassigned";
                                    if (!tasksByUser.ContainsKey(user))
                                        tasksByUser[user] = (0, 0, 0, 0);

                                    var (assigned, completed, pending, avgTime) = tasksByUser[user];
                                    assigned++;

                                    int completed_count = step.CompletedDate.HasValue ? 1 : 0;
                                    completed += completed_count;
                                    pending += (1 - completed_count);

                                    double stepTime = step.TimeSpentMinutes.HasValue ? step.TimeSpentMinutes.Value / 60.0 : 0;
                                    avgTime = (avgTime + stepTime) / 2;

                                    tasksByUser[user] = (assigned, completed, pending, avgTime);
                                }
                            }
                        }

                        foreach (var kvp in tasksByUser)
                        {
                            var (assigned, completed, pending, avgTime) = kvp.Value;
                            distribution.Add(new TaskDistributionData
                            {
                                UserName = kvp.Key,
                                Role = "Officer",
                                AssignedCount = assigned,
                                CompletedCount = completed,
                                PendingCount = pending,
                                PendingTasks = pending,
                                CompletedTasks = completed,
                                AverageCompletionTimeHours = avgTime
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.Warn($"Error loading task distribution details: {ex.Message}");
                }

                _view.DisplayTaskDistribution(distribution);
                _loggingService.Info("Loaded task distribution data");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading task distribution: {ex.Message}", ex);
                _view.ShowError($"Failed to load task distribution: {ex.Message}");
            }
        }

        /// <summary>
        /// Load and display bottleneck analysis
        /// </summary>
        public void LoadBottleneckAnalysis()
        {
            try
            {
                var bottlenecks = new List<BottleneckData>();

                try
                {
                    var instances = _workflowService.GetWorkflowInstances(new WorkflowInstanceFilter(), _userContext);
                    if (instances?.Items != null)
                    {
                        var stepMetrics = new Dictionary<string, (int pending, double avgWait, double avgComplete, int total, int overdue)>();

                        foreach (var instance in instances.Items)
                        {
                            var fullInstance = _workflowService.GetWorkflowInstance(instance.WorkflowInstanceId, _userContext);
                            var steps = fullInstance?.StepHistory;
                            if (steps != null)
                            {
                                foreach (var step in steps)
                                {
                                    string key = $"{instance.WorkflowName}:{step.StepName}";
                                    if (!stepMetrics.ContainsKey(key))
                                        stepMetrics[key] = (0, 0, 0, 0, 0);

                                    var (pending, avgWait, avgComplete, total, overdue) = stepMetrics[key];
                                    total++;

                                    int isPending = step.Status == CMS.Domain.Enums.WorkflowStepInstanceStatus.Pending ? 1 : 0;
                                    pending += isPending;

                                    int isOverdue = step.SLAStatus == CMS.Domain.Enums.SLAStatus.Overdue ? 1 : 0;
                                    overdue += isOverdue;

                                    double waitTime = step.TimeSpentMinutes.HasValue ? step.TimeSpentMinutes.Value / 60.0 : 0;
                                    avgWait = (avgWait + waitTime) / 2;
                                    avgComplete = avgWait;

                                    stepMetrics[key] = (pending, avgWait, avgComplete, total, overdue);
                                }
                            }
                        }

                        foreach (var kvp in stepMetrics.OrderByDescending(x => x.Value.pending).Take(10))
                        {
                            var parts = kvp.Key.Split(':');
                            var (pending, avgWait, avgComplete, total, overdue) = kvp.Value;

                            bottlenecks.Add(new BottleneckData
                            {
                                TemplateName = parts[0],
                                StepName = parts.Length > 1 ? parts[1] : "-",
                                PendingCount = pending,
                                AverageWaitTimeHours = avgWait,
                                AverageCompletionTimeHours = avgComplete,
                                TotalInstances = total,
                                OverdueCount = overdue
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.Warn($"Error loading bottleneck details: {ex.Message}");
                }

                _view.DisplayBottleneckAnalysis(bottlenecks);
                _loggingService.Info("Loaded bottleneck analysis");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading bottleneck analysis: {ex.Message}", ex);
                _view.ShowError($"Failed to load bottleneck analysis: {ex.Message}");
            }
        }

        /// <summary>
        /// Load and display escalation statistics
        /// </summary>
        public void LoadEscalationStats()
        {
            try
            {
                var stats = new EscalationStatistics
                {
                    TotalEscalations = 0,
                    EscalationsThisWeek = 0,
                    EscalationsThisMonth = 0,
                    TopEscalatedSteps = new List<string>(),
                    EscalationsByReason = new Dictionary<string, int>(),
                    LastEscalationDate = null
                };

                try
                {
                    var instances = _workflowService.GetWorkflowInstances(new WorkflowInstanceFilter(), _userContext);
                    if (instances?.Items != null)
                    {
                        var escalationsByStep = new Dictionary<string, int>();
                        var now = DateTime.Now;
                        var weekAgo = now.AddDays(-7);
                        var monthAgo = now.AddDays(-30);

                        foreach (var instance in instances.Items)
                        {
                            var fullInstance = _workflowService.GetWorkflowInstance(instance.WorkflowInstanceId, _userContext);
                            var steps = fullInstance?.StepHistory;
                            if (steps != null)
                            {
                                foreach (var step in steps)
                                {
                                    if (step.SLAStatus == CMS.Domain.Enums.SLAStatus.Overdue)
                                    {
                                        stats.TotalEscalations++;

                                        if (step.AssignedDate >= weekAgo)
                                            stats.EscalationsThisWeek++;

                                        if (step.AssignedDate >= monthAgo)
                                            stats.EscalationsThisMonth++;

                                        if (!stats.LastEscalationDate.HasValue || step.AssignedDate > stats.LastEscalationDate)
                                            stats.LastEscalationDate = step.AssignedDate;

                                        string stepKey = $"{step.StepName}";
                                        if (!escalationsByStep.ContainsKey(stepKey))
                                            escalationsByStep[stepKey] = 0;
                                        escalationsByStep[stepKey]++;
                                    }
                                }
                            }
                        }

                        stats.TopEscalatedSteps = escalationsByStep
                            .OrderByDescending(x => x.Value)
                            .Take(5)
                            .Select(x => x.Key)
                            .ToList();

                        stats.EscalationsByReason["Overdue SLA"] = stats.TotalEscalations;
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.Warn($"Error loading escalation details: {ex.Message}");
                }

                _view.DisplayEscalationStats(stats);
                _loggingService.Info("Loaded escalation statistics");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading escalation stats: {ex.Message}", ex);
                _view.ShowError($"Failed to load escalation statistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Refresh all monitor data
        /// </summary>
        public void RefreshMonitor(bool showInfo = false)
        {
            LoadMetrics();
            LoadSLAChart();
            LoadTaskDistribution();
            LoadBottleneckAnalysis();
            LoadEscalationStats();
            if (showInfo)
                _view.ShowInfo("Monitor refreshed successfully");
        }

        /// <summary>
        /// Calculate system-wide metrics
        /// </summary>
        private WorkflowMonitorMetrics CalculateMetrics(List<WorkflowInstanceResult> instances)
        {
            // Status values: Created=1, InProgress=2, Suspended=3, Completed=4, Rejected=5
            var metrics = new WorkflowMonitorMetrics
            {
                TotalInstances = instances.Count,
                CompletedInstances = instances.Count(i => i.Status == WorkflowInstanceStatus.Completed), // Completed
                FailedInstances = instances.Count(i => i.Status == WorkflowInstanceStatus.Rejected), // Rejected
                InProgressInstances = instances.Count(i => i.Status == WorkflowInstanceStatus.InProgress), // InProgress
                SuspendedInstances = instances.Count(i => i.Status == WorkflowInstanceStatus.Suspended) // Suspended
            };

            // Calculate average duration
            var completedInstances = instances.Where(i => i.Status == WorkflowInstanceStatus.Completed && i.CompletedDate.HasValue).ToList();
            if (completedInstances.Count > 0)
            {
                var totalDuration = completedInstances.Sum(i => (i.CompletedDate.Value - i.InitiatedDate).TotalHours);
                metrics.AverageCompletionTimeHours = totalDuration / completedInstances.Count;
            }

            return metrics;
        }

        /// <summary>
        /// Generate SLA chart data
        /// </summary>
        private List<SLAChartData> GenerateSLAChartData(List<WorkflowInstanceResult> instances)
        {
            var chartData = new List<SLAChartData>();

            // Group by template
            var groupedByTemplate = instances.GroupBy(i => i.WorkflowDefinitionId);

            foreach (var group in groupedByTemplate)
            {
                var data = new SLAChartData
                {
                    TemplateName = group.FirstOrDefault()?.WorkflowName ?? $"Template {group.Key}",
                    OnTimeCount = group.SelectMany(i => i.StepHistory ?? new List<WorkflowStepInstanceResult>()).Count(s => s.SLAStatus == SLAStatus.OnTime),
                    AtRiskCount = group.SelectMany(i => i.StepHistory ?? new List<WorkflowStepInstanceResult>()).Count(s => s.SLAStatus == SLAStatus.AtRisk),
                    OverdueCount = group.SelectMany(i => i.StepHistory ?? new List<WorkflowStepInstanceResult>()).Count(s => s.SLAStatus == SLAStatus.Overdue)
                };
                var total = data.OnTimeCount + data.AtRiskCount + data.OverdueCount;
                data.CompliancePercentage = total > 0
                    ? Math.Round((data.OnTimeCount + data.AtRiskCount) * 100.0 / total, 1)
                    : 100;

                chartData.Add(data);
            }

            return chartData;
        }
    }
}
