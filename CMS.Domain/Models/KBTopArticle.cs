namespace CMS.Domain.Models
{
    /// <summary>
    /// Top article model for dashboard
    /// </summary>
    public class KBTopArticle
    {
        public int ArticleID { get; set; }
        public string ArticleCode { get; set; }
        public string Title { get; set; }
        public string ArticleType { get; set; }
        public string Category { get; set; }
        public int ViewCount { get; set; }
        public decimal Rating { get; set; }
    }
}
