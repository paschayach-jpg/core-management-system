using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Services.Interfaces;
using CMS.Services.DTOs;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Models;

namespace CMS.Controllers
{
    /// <summary>
    /// Workflow dashboard controller for user and admin dashboards
    /// </summary>
    public class WorkflowDashboardController
    {
        private readonly IWorkflowService _workflowService;
        private readonly ISLACalculator _slaCalculator;
        private readonly ILoggingService _loggingService;

        public WorkflowDashboardController(
            IWorkflowService workflowService = null,
            ISLACalculator slaCalculator = null,
            ILoggingService loggingService = null)
        {
            _workflowService = workflowService;
            _slaCalculator = slaCalculator;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Get user's assigned tasks
        /// </summary>
        public List<WorkflowTaskResult> GetMyTasks(UserContext userContext)
        {
            try
            {
                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Getting tasks for user {userContext.ADUserName}");
                return _workflowService.GetMyTasks(userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting my tasks: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get user's workflow instances
        /// </summary>
        public PagedResult<WorkflowInstanceResult> GetMyWorkflows(UserContext userContext)
        {
            try
            {
                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                var filter = new WorkflowInstanceFilter
                {
                    InitiatedBy = userContext.ADUserName,
                    PageSize = 20
                };

                _loggingService?.Info($"Getting workflows for user {userContext.ADUserName}");
                return _workflowService.GetWorkflowInstances(filter, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting my workflows: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get dashboard metrics for user
        /// </summary>
        public object GetDashboardMetrics(UserContext userContext)
        {
            try
            {
                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Getting dashboard metrics for user {userContext.ADUserName}");

                var tasks = _workflowService.GetMyTasks(userContext) ?? new List<WorkflowTaskResult>();
                var totalTasks = tasks.Count;
                var overdueTasks = tasks.Count(t => t.SLAStatus == SLAStatus.Overdue);
                var completedTasks = tasks.Count(t => t.Status == WorkflowStepInstanceStatus.Completed && t.CompletedDate.HasValue && t.CompletedDate.Value.Date == DateTime.Today);
                var pendingTasks = tasks.Count(t => t.Status != WorkflowStepInstanceStatus.Completed);
                var slaCompliance = totalTasks == 0 ? 100 : (int)Math.Round((double)(totalTasks - overdueTasks) * 100 / totalTasks);

                return new
                {
                    TotalTasks = totalTasks,
                    PendingTasks = pendingTasks,
                    OverdueTasks = overdueTasks,
                    CompletedTasks = completedTasks,
                    SLACompliancePercentage = slaCompliance
                };
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting dashboard metrics: {ex.Message}", ex);
                throw;
            }
        }
    }
}
