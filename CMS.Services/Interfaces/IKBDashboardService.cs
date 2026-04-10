using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Services.Interfaces
{
    public interface IKBDashboardService
    {
        KBDashboardStats GetDashboardStats(UserContext user);
        IEnumerable<KBMonthlyArticle> GetMonthlyArticles(UserContext user, int months);
        IEnumerable<KBTopArticle> GetTopArticles(UserContext user, int topN);
    }
}
