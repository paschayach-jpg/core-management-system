using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Filter for querying SLA metrics
    /// </summary>
    public class SLAMetricsFilter
    {
        /// <summary>
        /// Filter by workflow definition ID
        /// </summary>
        public int? WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Filter by user name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Filter by date range start
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filter by date range end
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Include only completed tasks
        /// </summary>
        public bool OnlyCompleted { get; set; } = true;
    }
}
