using System.Collections.Generic;
using CMS.Data.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Presenters.Interfaces
{
    /// <summary>
    /// View interface for Template Approval form.
    /// Displays workflow templates pending approval and allows approve/reject actions.
    /// </summary>
    public interface ITemplateApprovalView
    {
        /// <summary>Display the list of templates pending approval.</summary>
        void DisplayPendingTemplates(PagedResult<WorkflowTemplateResult> templates);

        /// <summary>Enable/disable action buttons based on selection.</summary>
        void SetActionButtonsEnabled(bool canApprove, bool canReject, bool canPreview);

        /// <summary>Update the pending count badge.</summary>
        void UpdatePendingCount(int count);

        /// <summary>Show error message.</summary>
        void ShowError(string message);

        /// <summary>Show info message.</summary>
        void ShowInfo(string message);

        /// <summary>Get search filter text.</summary>
        string GetSearchFilter();

        /// <summary>Get category filter (null = all).</summary>
        string GetCategoryFilter();
    }
}
