using System;
using System.Collections.Generic;
using CMS.Data.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Presenters.Interfaces
{
    /// <summary>
    /// View interface for Workflow History Search
    /// </summary>
    public interface IHistorySearchView
    {
        /// <summary>
        /// Display search results
        /// </summary>
        void DisplayResults(PagedResult<WorkflowInstanceResult> results);

        /// <summary>
        /// Show detail view for selected instance
        /// </summary>
        void ShowDetailView(WorkflowInstanceResult instance);

        /// <summary>
        /// Export results to file
        /// </summary>
        void ExportResults(string filePath);

        /// <summary>
        /// Show error message
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Show info message
        /// </summary>
        void ShowInfo(string message);

        /// <summary>
        /// Get search criteria
        /// </summary>
        WorkflowHistorySearchCriteria GetSearchCriteria();

        /// <summary>
        /// Clear search results
        /// </summary>
        void ClearResults();
    }

    /// <summary>
    /// Search criteria for workflow history
    /// </summary>
    public class WorkflowHistorySearchCriteria
    {
        public int? TemplateId { get; set; }
        public byte? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string InitiatedBy { get; set; }
        public string AssignedTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
