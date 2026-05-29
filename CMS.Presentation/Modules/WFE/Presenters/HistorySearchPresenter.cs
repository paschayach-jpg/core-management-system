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
    /// Presenter for Workflow History Search
    /// </summary>
    public class HistorySearchPresenter
    {
        private readonly IHistorySearchView _view;
        private readonly IWorkflowService _workflowService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;

        public HistorySearchPresenter(
            IHistorySearchView view,
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
        /// Search workflow history based on criteria
        /// </summary>
        public void SearchHistory()
        {
            try
            {
                var criteria = _view.GetSearchCriteria();

                var filter = new WorkflowInstanceFilter
                {
                    WorkflowDefinitionId = criteria.TemplateId,
                    Status = criteria.Status.HasValue ? (WorkflowInstanceStatus?)criteria.Status.Value : null,
                    FromDate = criteria.FromDate,
                    ToDate = criteria.ToDate,
                    InitiatedBy = criteria.InitiatedBy,
                    PageNumber = criteria.PageNumber,
                    PageSize = criteria.PageSize
                };

                var results = _workflowService.GetWorkflowInstances(filter, _userContext);
                _view.DisplayResults(results);

                _loggingService.Info($"History search found {results.Items.Count()} instances");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error searching history: {ex.Message}", ex);
                _view.ShowError($"Search failed: {ex.Message}");
            }
        }

        /// <summary>
        /// View details of selected instance
        /// </summary>
        public void ViewInstanceDetails(WorkflowInstanceResult instance)
        {
            try
            {
                if (instance == null)
                {
                    _view.ShowError("Please select an instance to view");
                    return;
                }

                _view.ShowDetailView(instance);
                _loggingService.Info($"Viewed details for workflow instance {instance.WorkflowInstanceId}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error viewing instance details: {ex.Message}", ex);
                _view.ShowError($"Failed to view details: {ex.Message}");
            }
        }

        /// <summary>
        /// Export search results to file
        /// </summary>
        public void ExportResults(string filePath)
        {
            try
            {
                _view.ExportResults(filePath);
                _view.ShowInfo($"Results exported successfully to {filePath}");

                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowHistoryExported",
                    TableName = "WorkflowInstance",
                    UserName = _userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Exported to {filePath}"
                });

                _loggingService.Info($"Workflow history exported to {filePath}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error exporting results: {ex.Message}", ex);
                _view.ShowError($"Export failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear search results
        /// </summary>
        public void ClearSearch()
        {
            try
            {
                _view.ClearResults();
                _loggingService.Info("Cleared search results");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error clearing search: {ex.Message}", ex);
                _view.ShowError($"Failed to clear search: {ex.Message}");
            }
        }

        /// <summary>
        /// Load available templates for dropdown
        /// </summary>
        public void LoadTemplates()
        {
            try
            {
                // Placeholder: would fetch from service
                _loggingService.Info("Templates loaded");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading templates: {ex.Message}", ex);
            }
        }

        public IWorkflowService GetWorkflowService() => _workflowService;
        public ILoggingService GetLoggingService() => _loggingService;
        public IWorkflowTemplateService GetTemplateService() => null;
    }
}
