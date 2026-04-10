using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Data.Interfaces
{
    public interface IKBDashboardRepository
    {
        KBDashboardStats GetDashboardStats();
        IEnumerable<KBMonthlyArticle> GetMonthlyArticles(int months);
        IEnumerable<KBTopArticle> GetTopArticles(int topN);
    }
}
