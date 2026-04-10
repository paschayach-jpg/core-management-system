using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    public interface IKBDashboardController
    {
        // Dashboard Stats
        void DisplayStats(KBDashboardStats stats);
        
        // Monthly Chart
        void DisplayMonthlyChart(IEnumerable<KBMonthlyArticle> monthlyData);
        
        // Top Articles
        void DisplayTopArticles(IEnumerable<KBTopArticle> topArticles);
        
        // Alerts & Messages
        void ShowError(string message);
        void ShowInfo(string message);
        void SetStatus(string message);
    }
}
