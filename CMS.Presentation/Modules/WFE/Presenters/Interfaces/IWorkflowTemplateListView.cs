using System;
using System.Collections.Generic;
using CMS.Data.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Presenters.Interfaces
{
    /// <summary>
    /// View interface for Workflow Template List
    /// </summary>
    public interface IWorkflowTemplateListView
    {
        /// <summary>
        /// Display list of templates
        /// </summary>
        void DisplayTemplates(PagedResult<WorkflowTemplateResult> templates);

        /// <summary>
        /// Show template creation/edit form
        /// </summary>
        void ShowTemplateForm(WorkflowTemplateResult template = null);

        /// <summary>
        /// Show version history for template
        /// </summary>
        void ShowVersionHistory(List<WorkflowTemplateVersionResult> versions);

        /// <summary>
        /// Enable/disable action buttons based on current selection and permissions
        /// </summary>
        void SetActionButtonsEnabled(bool canEdit, bool canDelete, bool canDuplicate);

        /// <summary>
        /// Show error message
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Show info message
        /// </summary>
        void ShowInfo(string message);

        /// <summary>
        /// Get search filter text
        /// </summary>
        string GetSearchFilter();

        /// <summary>
        /// Get status filter label (Active, Draft, Inactive, Deprecated). Null = all.
        /// </summary>
        string GetStatusFilter();

        /// <summary>
        /// Get category filter (null or empty = all categories)
        /// </summary>
        string GetCategoryFilter();
    }
}
