using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Filter for querying workflow templates
    /// </summary>
    public class WorkflowTemplateFilter
    {
        /// <summary>
        /// Filter by active status
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Search keyword in template name or description
        /// </summary>
        public string SearchKeyword { get; set; }

        /// <summary>
        /// Filter by created date range start
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filter by created date range end
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Filter by creator user name
        /// </summary>
        public string CreatedBy { get; set; }

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
        public string SortBy { get; set; } = "CreatedDate";

        /// <summary>
        /// Sort direction (ASC or DESC)
        /// </summary>
        public string SortDirection { get; set; } = "DESC";

        /// <summary>
        /// Filter by category (e.g., เอกสาร, IT, Knowledge, HR)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Filter by status label (Active, Draft, Inactive, Deprecated). Null = all.
        /// </summary>
        public string StatusLabel { get; set; }
    }
}
