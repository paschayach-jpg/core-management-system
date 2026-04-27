using CMS.Domain.Enums;
using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Filter for querying workflow instances
    /// </summary>
    public class WorkflowInstanceFilter
    {
        /// <summary>
        /// Filter by workflow definition ID
        /// </summary>
        public int? WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Filter by status
        /// </summary>
        public WorkflowInstanceStatus? Status { get; set; }

        /// <summary>
        /// Filter by initiator user name
        /// </summary>
        public string InitiatedBy { get; set; }

        /// <summary>
        /// Filter by date range start
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filter by date range end
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Search keyword in workflow name or description
        /// </summary>
        public string SearchKeyword { get; set; }

        /// <summary>
        /// Page number for pagination (1-based)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size for pagination
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Sort by column name
        /// </summary>
        public string SortBy { get; set; } = "InitiatedDate";

        /// <summary>
        /// Sort direction (ASC or DESC)
        /// </summary>
        public string SortDirection { get; set; } = "DESC";
    }
}
