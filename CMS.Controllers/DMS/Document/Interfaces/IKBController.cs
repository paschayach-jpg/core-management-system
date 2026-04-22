using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IKBController
    {
        // Category operations
        IList<KBCategory> GetAllCategories();
        int CreateCategory(KBCategory category);
        void UpdateCategory(KBCategory category);
        void DeactivateCategory(int categoryId);

        // Article operations
        IList<KBArticle> GetAllArticles();
        IList<KBArticle> GetArticlesByCategory(int categoryId);
        IList<KBArticle> SearchArticles(string keyword, int? categoryId, byte? status, byte? articleType);
        void IncrementViewCount(int articleId);
    }
}
