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
    /// Presenter for Workflow Dashboard (My Tasks and workflow status)
    /// </summary>
    public class WorkflowDashboardPresenter
    {
        private readonly IWorkflowDashboardView _view;
        private readonly IWorkflowService _workflowService;
        private readonly ISLACalculator _slaCalculator;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;

        public WorkflowDashboardPresenter(
            IWorkflowDashboardView view,
            IWorkflowService workflowService,
            ISLACalculator slaCalculator,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _slaCalculator = slaCalculator ?? throw new ArgumentNullException(nameof(slaCalculator));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Load and display user's tasks
        /// </summary>
        public void LoadMyTasks()
        {
            try
            {
                _view.SetRefreshEnabled(false);
                var tasks = _workflowService.GetMyTasks(_userContext);
                _view.DisplayMyTasks(tasks);
                _loggingService.Info($"Loaded {tasks.Count} tasks for user {_userContext.ADUserName}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading my tasks: {ex.Message}", ex);
                _view.ShowError($"Failed to load tasks: {ex.Message}");
            }
            finally
            {
                _view.SetRefreshEnabled(true);
            }
        }

        /// <summary>
        /// Load and display workflow instances
        /// </summary>
        public void LoadWorkflowInstances()
        {
            try
            {
                _view.SetRefreshEnabled(false);
                var filter = new WorkflowInstanceFilter
                {
                    Status = (WorkflowInstanceStatus?)2, // InProgress
                    PageNumber = 1,
                    PageSize = 50
                };

                var instances = _workflowService.GetWorkflowInstances(filter, _userContext);
                _view.DisplayWorkflowInstances(instances);
                _loggingService.Info($"Loaded {instances.Items.Count()} workflow instances");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading workflow instances: {ex.Message}", ex);
                _view.ShowError($"Failed to load workflow instances: {ex.Message}");
            }
            finally
            {
                _view.SetRefreshEnabled(true);
            }
        }

        /// <summary>
        /// Load and display SLA metrics
        /// </summary>
        public void LoadSLAMetrics()
        {
            try
            {
                _view.SetRefreshEnabled(false);
                var filter = new WorkflowInstanceFilter
                {
                    PageNumber = 1,
                    PageSize = 1000
                };

                var instances = _workflowService.GetWorkflowInstances(filter, _userContext);
                var metrics = CalculateSLAMetrics(instances.Items.ToList());
                _view.DisplaySLAMetrics(metrics);
                _loggingService.Info($"Calculated SLA metrics: OnTime={metrics.OnTimeCount}, AtRisk={metrics.AtRiskCount}, Overdue={metrics.OverdueCount}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading SLA metrics: {ex.Message}", ex);
                _view.ShowError($"Failed to load SLA metrics: {ex.Message}");
            }
            finally
            {
                _view.SetRefreshEnabled(true);
            }
        }

        /// <summary>
        /// Refresh all dashboard data
        /// </summary>
        public void RefreshDashboard()
        {
            LoadMyTasks();
            LoadWorkflowInstances();
            LoadSLAMetrics();
            _view.ShowInfo("Dashboard refreshed successfully");
        }

        /// <summary>
        /// Calculate SLA metrics from workflow instances
        /// </summary>
        private SLAComplianceMetricsResult CalculateSLAMetrics(List<WorkflowInstanceResult> instances)
        {
            var metrics = new SLAComplianceMetricsResult();

            if (instances.Count == 0)
                return metrics;

            // Get all step instances for the user's workflows
            var totalSteps = 0;
            var onTimeSteps = 0;
            var atRiskSteps = 0;
            var overdueSteps = 0;

            foreach (var instance in instances)
            {
                try
                {
                    var steps = _workflowService.GetWorkflowStepInstances(instance.WorkflowInstanceId, _userContext);
                    if (steps != null)
                    {
                        foreach (var step in steps)
                        {
                            totalSteps++;
                            switch (step.SLAStatus)
                            {
                                case CMS.Domain.Enums.SLAStatus.OnTime:
                                    onTimeSteps++;
                                    break;
                                case CMS.Domain.Enums.SLAStatus.AtRisk:
                                    atRiskSteps++;
                                    break;
                                case CMS.Domain.Enums.SLAStatus.Overdue:
                                    overdueSteps++;
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.Warn($"Failed to load steps for instance {instance.WorkflowInstanceId}: {ex.Message}");
                    totalSteps += instance.TotalStepCount;
                }
            }

            metrics.TotalTasks = totalSteps;
            metrics.OnTimeCount = onTimeSteps;
            metrics.AtRiskCount = atRiskSteps;
            metrics.OverdueCount = overdueSteps;
            metrics.CompliancePercentage = totalSteps > 0 ? (onTimeSteps * 100) / totalSteps : 0;

            return metrics;
        }
    }
}
