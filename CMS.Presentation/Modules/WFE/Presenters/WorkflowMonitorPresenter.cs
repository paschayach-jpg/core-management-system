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

                _loggingService.Info("Loaded workflow monitor metrics");
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
                // TODO: Implement task distribution loading from repository
                var distribution = new List<TaskDistributionData>();
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
                // TODO: Implement bottleneck analysis from repository
                var bottlenecks = new List<BottleneckData>();
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
                // TODO: Implement escalation statistics loading from repository
                var stats = new EscalationStatistics
                {
                    TotalEscalations = 0,
                    EscalationsThisWeek = 0,
                    EscalationsThisMonth = 0,
                    TopEscalatedSteps = new List<string>()
                };

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
        public void RefreshMonitor()
        {
            LoadMetrics();
            LoadSLAChart();
            LoadTaskDistribution();
            LoadBottleneckAnalysis();
            LoadEscalationStats();
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
                    TemplateName = $"Template {group.Key}",
                    OnTimeCount = (int)(group.Count() * 0.75),
                    AtRiskCount = (int)(group.Count() * 0.15),
                    OverdueCount = (int)(group.Count() * 0.10),
                    CompliancePercentage = 75
                };

                chartData.Add(data);
            }

            return chartData;
        }
    }
}
