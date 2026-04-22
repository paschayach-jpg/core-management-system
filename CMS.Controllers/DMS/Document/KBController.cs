using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class KBController : IKBController
    {
        private readonly IKBCategoryService _categoryService;
        private readonly IKBArticleService  _articleService;

        public KBController(
            IKBCategoryService categoryService = null,
            IKBArticleService  articleService  = null)
        {
            _categoryService = categoryService;
            _articleService  = articleService;
        }

        public IList<KBCategory> GetAllCategories()
        {
            if (_categoryService == null)
                return new List<KBCategory>();

            return _categoryService.GetAll().ToList();
        }

        public int CreateCategory(KBCategory category)
        {
            if (_categoryService == null || category == null)
                return 0;

            return _categoryService.Create(category);
        }

        public void UpdateCategory(KBCategory category)
        {
            if (_categoryService == null || category == null)
                return;

            _categoryService.Update(category);
        }

        public void DeactivateCategory(int categoryId)
        {
            if (_categoryService == null)
                return;

            _categoryService.Deactivate(categoryId);
        }

        public IList<KBArticle> GetAllArticles()
        {
            if (_articleService == null)
                return new List<KBArticle>();

            return _articleService.GetAll().ToList();
        }

        public IList<KBArticle> GetArticlesByCategory(int categoryId)
        {
            if (_articleService == null)
                return new List<KBArticle>();

            return _articleService.GetByCategory(categoryId).ToList();
        }

        public IList<KBArticle> SearchArticles(string keyword, int? categoryId, byte? status, byte? articleType)
        {
            if (_articleService == null)
                return new List<KBArticle>();

            return _articleService.Search(keyword, categoryId, status, articleType).ToList();
        }

        public void IncrementViewCount(int articleId)
        {
            if (_articleService == null)
                return;

            _articleService.IncrementViewCount(articleId);
        }
    }
}
