using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// KMS Dashboard statistics model
    /// </summary>
    public class KBDashboardStats
    {
        public int TotalArticles { get; set; }
        public int MonthlyViews { get; set; }
        public decimal AverageRating { get; set; }
        public int PublishedArticles { get; set; }
        public int DraftArticles { get; set; }
        public int ReviewArticles { get; set; }
        public int ArchivedArticles { get; set; }
        public int ExpiringArticles { get; set; }
        public int OverdueReviews { get; set; }
        public int PendingApprovals { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
