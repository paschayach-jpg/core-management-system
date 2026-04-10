namespace CMS.Domain.Models
{
    /// <summary>
    /// Monthly article count for dashboard chart
    /// </summary>
    public class KBMonthlyArticle
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int ArticleCount { get; set; }
    }
}
