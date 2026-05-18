using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow.Presenters
{
    /// <summary>
    /// Presenter for Workflow Template List view
    /// </summary>
    public class WorkflowTemplateListPresenter
    {
        private readonly IWorkflowTemplateListView _view;
        private readonly IWorkflowTemplateService _templateService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private int _currentPage = 1;
        private const int PageSize = 50;

        public WorkflowTemplateListPresenter(
            IWorkflowTemplateListView view,
            IWorkflowTemplateService templateService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _auditService = auditService;
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Load and display workflow templates
        /// </summary>
        public void LoadTemplates()
        {
            try
            {
                var filter = new WorkflowTemplateFilter
                {
                    PageNumber = _currentPage,
                    PageSize = PageSize
                };

                var templates = _templateService.GetTemplates(filter, _userContext);
                _view.DisplayTemplates(templates);
                _loggingService.Info($"Loaded {templates.Items.Count()} workflow templates");
            }
            catch (Exception ex)
            {
                if (IsWorkflowDefinitionMissing(ex))
                {
                    _loggingService.Warn("WorkflowTemplateList fallback to empty list because WorkflowDefinition table is not available yet.");
                    _view.DisplayTemplates(CreateEmptyPagedResult());
                    return;
                }

                _loggingService.Error($"Error loading workflow templates: {ex.Message}", ex);
                _view.ShowError($"Failed to load templates: {ex.Message}");
            }
        }

        /// <summary>
        /// Search templates by name, status, and category
        /// </summary>
        public void SearchTemplates()
        {
            try
            {
                _currentPage = 1;
                var searchText = _view.GetSearchFilter();
                var statusFilter = _view.GetStatusFilter();
                var categoryFilter = _view.GetCategoryFilter();

                var filter = new WorkflowTemplateFilter
                {
                    IsActive = statusFilter == "Active" ? true
                             : statusFilter == "Inactive" ? (bool?)false
                             : null,
                    StatusLabel = string.IsNullOrWhiteSpace(statusFilter) ? null : statusFilter,
                    SearchKeyword = searchText,
                    Category = string.IsNullOrWhiteSpace(categoryFilter) ? null : categoryFilter,
                    PageNumber = _currentPage,
                    PageSize = PageSize
                };

                var templates = _templateService.GetTemplates(filter, _userContext);
                _view.DisplayTemplates(templates);
                _loggingService.Info($"Search found {templates.Items.Count()} templates");
            }
            catch (Exception ex)
            {
                if (IsWorkflowDefinitionMissing(ex))
                {
                    _loggingService.Warn("WorkflowTemplateList search fallback to empty list because WorkflowDefinition table is not available yet.");
                    _view.DisplayTemplates(CreateEmptyPagedResult());
                    return;
                }

                _loggingService.Error($"Error searching templates: {ex.Message}", ex);
                _view.ShowError($"Search failed: {ex.Message}");
            }
        }

        private PagedResult<WorkflowTemplateResult> CreateEmptyPagedResult()
        {
            return new PagedResult<WorkflowTemplateResult>
            {
                Items = new List<WorkflowTemplateResult>(),
                TotalCount = 0,
                PageNumber = _currentPage,
                PageSize = PageSize
            };
        }

        private static bool IsWorkflowDefinitionMissing(Exception ex)
        {
            var message = ex != null ? ex.Message : string.Empty;
            if (string.IsNullOrWhiteSpace(message))
                return false;

            return message.IndexOf("invalid object name", StringComparison.OrdinalIgnoreCase) >= 0
                   && message.IndexOf("workflowdefinition", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Create new template
        /// </summary>
        public void CreateTemplate()
        {
            try
            {
                _view.ShowTemplateForm(null);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error creating template: {ex.Message}", ex);
                _view.ShowError($"Failed to create template: {ex.Message}");
            }
        }

        /// <summary>
        /// Edit selected template
        /// </summary>
        public void EditTemplate(WorkflowTemplateResult template)
        {
            try
            {
                if (template == null)
                {
                    _view.ShowError("Please select a template to edit");
                    return;
                }

                _view.ShowTemplateForm(template);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error editing template: {ex.Message}", ex);
                _view.ShowError($"Failed to edit template: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete selected template
        /// </summary>
        public void DeleteTemplate(WorkflowTemplateResult template)
        {
            try
            {
                if (template == null)
                {
                    _view.ShowError("Please select a template to delete");
                    return;
                }

                _templateService.DeleteTemplate(template.WorkflowDefinitionId, _userContext);
                _view.ShowInfo("Template deleted successfully");
                LoadTemplates();

                _auditService?.Log(new AuditEntry
                {
                    Action = "WorkflowTemplateDeletedFromUI",
                    TableName = "WorkflowDefinition",
                    RecordID = template.WorkflowDefinitionId,
                    UserName = _userContext.ADUserName,
                    AuditDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error deleting template: {ex.Message}", ex);
                _view.ShowError($"Failed to delete template: {ex.Message}");
            }
        }

        /// <summary>
        /// View template versions
        /// </summary>
        public void ViewVersions(WorkflowTemplateResult template)
        {
            try
            {
                if (template == null)
                {
                    _view.ShowError("Please select a template");
                    return;
                }

                var versions = _templateService.GetTemplateVersions(template.WorkflowDefinitionId, _userContext);
                _view.ShowVersionHistory(versions);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading template versions: {ex.Message}", ex);
                _view.ShowError($"Failed to load versions: {ex.Message}");
            }
        }

        /// <summary>
        /// Duplicate template
        /// </summary>
        public void DuplicateTemplate(WorkflowTemplateResult template)
        {
            try
            {
                if (template == null)
                {
                    _view.ShowError("Please select a template to duplicate");
                    return;
                }

                var request = new CreateTemplateRequest
                {
                    Name = $"{template.Name} (Copy)",
                    Description = template.Description,
                    IsActive = false
                };

                var newTemplate = _templateService.CreateTemplate(request, _userContext);
                _view.ShowInfo("Template duplicated successfully");
                LoadTemplates();

                _auditService?.Log(new AuditEntry
                {
                    Action = "WorkflowTemplateDuplicatedFromUI",
                    TableName = "WorkflowDefinition",
                    RecordID = newTemplate.WorkflowDefinitionId,
                    UserName = _userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Duplicated from template {template.WorkflowDefinitionId}"
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error duplicating template: {ex.Message}", ex);
                _view.ShowError($"Failed to duplicate template: {ex.Message}");
            }
        }

        /// <summary>
        /// Go to next page
        /// </summary>
        public void NextPage()
        {
            _currentPage++;
            LoadTemplates();
        }

        /// <summary>
        /// Go to previous page
        /// </summary>
        public void PreviousPage()
        {
            if (_currentPage > 1)
                _currentPage--;
            LoadTemplates();
        }
    }
}
