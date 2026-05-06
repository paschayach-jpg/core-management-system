using System;
using System.Collections.Generic;
using CMS.Data.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Presenters.Interfaces
{
    /// <summary>
    /// View interface for Workflow Dashboard (My Tasks and workflow status)
    /// </summary>
    public interface IWorkflowDashboardView
    {
        /// <summary>
        /// Display user's assigned tasks
        /// </summary>
        void DisplayMyTasks(List<WorkflowTaskResult> tasks);

        /// <summary>
        /// Display workflow instances
        /// </summary>
        void DisplayWorkflowInstances(PagedResult<WorkflowInstanceResult> instances);

        /// <summary>
        /// Display SLA metrics (OnTime %, AtRisk %, Overdue %)
        /// </summary>
        void DisplaySLAMetrics(SLAComplianceMetricsResult metrics);

        /// <summary>
        /// Show error message to user
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Show info message to user
        /// </summary>
        void ShowInfo(string message);

        /// <summary>
        /// Enable/disable refresh button
        /// </summary>
        void SetRefreshEnabled(bool enabled);
    }
}
