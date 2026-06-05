using System;
using System.Collections.Generic;
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
    /// Presenter for Workflow Instance Detail view
    /// </summary>
    public class WorkflowInstanceDetailPresenter
    {
        private readonly IWorkflowInstanceDetailView _view;
        private readonly IWorkflowService _workflowService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private int _currentInstanceId;

        public WorkflowInstanceDetailPresenter(
            IWorkflowInstanceDetailView view,
            IWorkflowService workflowService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Load and display workflow instance details
        /// </summary>
        public void LoadInstanceDetails(int workflowInstanceId)
        {
            try
            {
                _currentInstanceId = workflowInstanceId;
                var instance = _workflowService.GetWorkflowInstance(workflowInstanceId, _userContext);
                _view.DisplayInstanceDetails(instance);

                // Load step history
                LoadStepHistory(workflowInstanceId);

                // Update action buttons based on instance status
                UpdateActionButtons(instance);

                _loggingService.Info($"Loaded workflow instance {workflowInstanceId} details");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading workflow instance details: {ex.Message}", ex);
                _view.ShowError($"Failed to load instance details: {ex.Message}");
            }
        }

        /// <summary>
        /// Load and display step execution history
        /// </summary>
        private void LoadStepHistory(int workflowInstanceId)
        {
            try
            {
                var fullInstance = _workflowService.GetWorkflowInstance(workflowInstanceId, _userContext);
                var history = fullInstance?.StepHistory ?? new List<WorkflowStepInstanceResult>();

                _view.DisplayStepHistory(history);
                _loggingService.Info($"Loaded step history for instance {workflowInstanceId}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading step history: {ex.Message}", ex);
                _view.ShowError($"Failed to load step history: {ex.Message}");
            }
        }

        /// <summary>
        /// Suspend the workflow instance
        /// </summary>
        public void SuspendInstance()
        {
            try
            {
                var result = _workflowService.SuspendWorkflowInstance(_currentInstanceId, _userContext);
                _view.DisplayInstanceDetails(result);
                _view.ShowInfo("Workflow instance suspended successfully");

                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowInstanceSuspendedFromUI",
                    TableName = "WorkflowInstance",
                    RecordID = _currentInstanceId,
                    UserName = _userContext.ADUserName,
                    AuditDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error suspending workflow instance: {ex.Message}", ex);
                _view.ShowError($"Failed to suspend instance: {ex.Message}");
            }
        }

        /// <summary>
        /// Resume the workflow instance
        /// </summary>
        public void ResumeInstance()
        {
            try
            {
                var result = _workflowService.ResumeWorkflowInstance(_currentInstanceId, _userContext);
                _view.DisplayInstanceDetails(result);
                _view.ShowInfo("Workflow instance resumed successfully");

                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowInstanceResumedFromUI",
                    TableName = "WorkflowInstance",
                    RecordID = _currentInstanceId,
                    UserName = _userContext.ADUserName,
                    AuditDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error resuming workflow instance: {ex.Message}", ex);
                _view.ShowError($"Failed to resume instance: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel the workflow instance
        /// </summary>
        public void CancelInstance()
        {
            try
            {
                if (_currentInstanceId <= 0)
                {
                    _view.ShowError("No workflow instance selected");
                    return;
                }

                _workflowService.SuspendWorkflowInstance(_currentInstanceId, _userContext);

                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowInstanceCancelledFromUI",
                    TableName = "WorkflowInstance",
                    RecordID = _currentInstanceId,
                    UserName = _userContext.ADUserName,
                    AuditDate = DateTime.Now
                });

                _view.ShowInfo("Workflow instance cancelled successfully");
                LoadInstanceDetails(_currentInstanceId);
                _loggingService.Info($"Cancelled workflow instance {_currentInstanceId}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error cancelling workflow instance: {ex.Message}", ex);
                _view.ShowError($"Failed to cancel instance: {ex.Message}");
            }
        }

        /// <summary>
        /// Update action buttons based on instance status
        /// </summary>
        private void UpdateActionButtons(WorkflowInstanceResult instance)
        {
            // Status values: Created=1, InProgress=2, Completed=3, Rejected=4, Suspended=5
            bool canSuspend = instance.Status == WorkflowInstanceStatus.InProgress;
            bool canResume = instance.Status == WorkflowInstanceStatus.Suspended;
            bool canCancel = instance.Status == WorkflowInstanceStatus.InProgress || instance.Status == WorkflowInstanceStatus.Suspended;

            _view.SetActionButtonsEnabled(canSuspend, canResume, canCancel);
        }
    }
}
