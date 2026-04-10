using CMS.Controllers.KMS.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Controllers.KMS.KB
{
    public class KnowledgeBasePresenter
    {
        private readonly IKnowledgeBaseView  _view;
        private readonly IKBCategoryService  _categoryService;
        private readonly IKBArticleService   _articleService;
        private readonly UserContext         _user;

        private static readonly byte?[] StatusFilterValues = { null, 1, 2, 3, 4 };

        public KnowledgeBasePresenter(
            IKnowledgeBaseView  view,
            IKBCategoryService  categoryService,
            IKBArticleService   articleService,
            UserContext         user)
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (user            == null) throw new ArgumentNullException("user");
            _view = view; _categoryService = categoryService;
            _articleService = articleService; _user = user;
        }

        public void Initialize() { LoadCategoryTree(); }

        public void LoadCategoryTree()
        {
            try
            {
                var categories = _categoryService.GetAll().ToList();

                // Build per-category article counts
                var counts = new Dictionary<int, int>();
                try
                {
                    foreach (var a in _articleService.GetAll())
                    {
                        if (!counts.ContainsKey(a.CategoryID)) counts[a.CategoryID] = 0;
                        counts[a.CategoryID]++;
                    }
                }
                catch { /* article counts are non-critical */ }

                _view.BindCategoryTree(categories, counts);
            }
            catch (Exception ex) { _view.ShowError("Load category failed: " + ex.Message); }
        }

        public void OnCategorySelected(int? categoryId, string categoryName)
        {
            _view.SelectedCategoryId = categoryId;
            _view.ArticleTitleText   = categoryId.HasValue
                ? string.Format("  {0}", categoryName)
                : "  ทุกหมวดหมู่";
            LoadArticles();
        }

        public void LoadArticles()
        {
            try
            {
                string keyword      = _view.SearchKeyword;
                byte?  statusFilter = GetStatusFilter(_view.StatusFilterIndex);
                int?   catId        = _view.SelectedCategoryId;

                IEnumerable<KBArticle> articles;
                if (string.IsNullOrEmpty(keyword) && !catId.HasValue && !statusFilter.HasValue)
                    articles = _articleService.GetAll();
                else if (string.IsNullOrEmpty(keyword) && !statusFilter.HasValue && catId.HasValue)
                    articles = _articleService.GetByCategory(catId.Value);
                else
                    articles = _articleService.Search(
                        string.IsNullOrEmpty(keyword) ? null : keyword,
                        catId, statusFilter, null);

                var list = articles.ToList();
                _view.RenderArticles(list);
                _view.ArticleCountText = string.Format("{0} บทความ", list.Count);
            }
            catch (Exception ex) { _view.ShowError("Load articles failed: " + ex.Message); }
        }

        public void OnArticleClicked(int articleId)
        {
            try { _articleService.IncrementViewCount(articleId); } catch { }
        }

        public void ShowReport()
        {
            try
            {
                var all = _articleService.GetAll().ToList();
                string report = string.Format(
                    "📊 สรุปรายงาน Knowledge Base\n\n" +
                    "บทความทั้งหมด: {0}\n" +
                    "📝 Draft:      {1}\n" +
                    "🔍 Review:     {2}\n" +
                    "✅ Published:  {3}\n" +
                    "📦 Archived:   {4}\n\n" +
                    "👁 Views ทั้งหมด: {5}\n" +
                    "👍 Likes ทั้งหมด: {6}",
                    all.Count,
                    all.Count(a => a.Status == 1),
                    all.Count(a => a.Status == 2),
                    all.Count(a => a.Status == 3),
                    all.Count(a => a.Status == 4),
                    all.Sum(a => a.ViewCount),
                    all.Sum(a => a.LikeCount));
                _view.ShowReport(report);
            }
            catch (Exception ex) { _view.ShowError("Load report failed: " + ex.Message); }
        }

        private byte? GetStatusFilter(int index)
        {
            if (index < 0 || index >= StatusFilterValues.Length) return null;
            return StatusFilterValues[index];
        }
    }
}
