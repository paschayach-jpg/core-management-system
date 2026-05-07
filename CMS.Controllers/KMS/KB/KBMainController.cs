using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Controllers.KMS.KB
{
    public class KBMainController
    {
        private readonly IKBMainController  _view;
        private readonly IKBCategoryService  _categoryService;
        private readonly IKBArticleService   _articleService;
        private readonly IKBVersionRepository _versionRepository;
        private readonly UserContext         _user;

        private static readonly byte?[] StatusFilterValues = { null, 1, 2, 3, 4 };
        private int _currentPage = 1;

        public KBMainController(
            IKBMainController  view,
            IKBCategoryService  categoryService,
            IKBArticleService   articleService,
            IKBVersionRepository versionRepository,
            UserContext         user)
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (versionRepository == null) throw new ArgumentNullException("versionRepository");
            if (user            == null) throw new ArgumentNullException("user");
            _view = view; _categoryService = categoryService;
            _articleService = articleService; _versionRepository = versionRepository; _user = user;
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
            _currentPage = 1;
            LoadArticlesAtPage();
        }

        public void GoToPage(int page)
        {
            _currentPage = page;
            LoadArticlesAtPage();
        }

        private void LoadArticlesAtPage()
        {
            try
            {
                string keyword      = _view.SearchKeyword;
                byte?  statusFilter = GetStatusFilter(_view.StatusFilterIndex);
                int?   catId        = _view.SelectedCategoryId;
                int    pageSize     = _view.PageSize;

                if (!catId.HasValue) return;   // require a specific category selection

                IEnumerable<KBArticle> articles;
                if (string.IsNullOrEmpty(keyword) && !statusFilter.HasValue)
                    articles = _articleService.GetByCategory(catId.Value);
                else
                    articles = _articleService.Search(
                        string.IsNullOrEmpty(keyword) ? null : keyword,
                        catId, statusFilter, null);

                var list = articles.ToList();
                foreach (var article in list)
                    ResolveLatestVersion(article);

                int totalCount = list.Count;
                int totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);
                if (_currentPage > totalPages) _currentPage = totalPages;
                if (_currentPage < 1)          _currentPage = 1;

                var pageItems = list.Skip((_currentPage - 1) * pageSize).Take(pageSize).ToList();

                _view.RenderArticles(pageItems);
                _view.ArticleCountText = string.Format("{0} บทความ", totalCount);
                _view.RenderPagination(totalCount, _currentPage, pageSize);
            }
            catch (Exception ex) { _view.ShowError("Load articles failed: " + ex.Message); }
        }

        public void OnArticleClicked(int articleId)
        {
            try { _articleService.IncrementViewCount(articleId); } catch { }
        }

        public void LikeArticle(int articleId)
        {
            try { _articleService.IncrementLikeCount(articleId); } catch { }
        }

        /// <summary>
        /// Send article for review - Change status from Draft (1) to Review (2)
        /// </summary>
        public void SendArticleForReview(int articleId)
        {
            try
            {
                _articleService.ChangeStatus(articleId, 2, _user.AdIdentity);
                // Success - no need to show message, the UI will refresh
            }
            catch (Exception ex)
            {
                _view.ShowError("ส่งขอ Review ไม่สำเร็จ: " + ex.Message);
            }
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

        /// <summary>
        /// Resolves the latest VersionNumber from KMS.t_KBVersion and writes it
        /// back to article.CurrentVersion so lblMeta always shows the DB value.
        /// </summary>
        private void ResolveLatestVersion(KBArticle article)
        {
            try
            {
                var latest = _versionRepository.GetLatestByArticle(article.ArticleID);
                if (latest != null)
                    article.CurrentVersion = latest.VersionNumber;
            }
            catch { /* non-critical — keep whatever CurrentVersion was already set */ }
        }
    }
}
