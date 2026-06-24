using CMS.Controllers.KMS.KB.Interfaces;
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
        private readonly IKBCategoryService _categoryService;
        private readonly IKBArticleService  _articleService;
        private readonly UserContext        _user;

        private int _currentPage = 1;

        public KBMainController(
            IKBMainController  view,
            IKBCategoryService categoryService,
            IKBArticleService  articleService,
            UserContext        user)
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (user            == null) throw new ArgumentNullException("user");
            _view            = view;
            _categoryService = categoryService;
            _articleService  = articleService;
            _user            = user;
        }

        public void Initialize() { LoadCategoryTree(); }

        public void LoadCategoryTree()
        {
            try
            {
                var categories       = _categoryService.GetAll().ToList();
                var counts           = new Dictionary<int, int>();
                // CategoryID → (Title → article count with that exact title)
                var titlesByCategory = new Dictionary<int, IDictionary<string, int>>();

                try
                {
                    foreach (var a in _articleService.GetAll())
                    {
                        // per-category article count
                        if (!counts.ContainsKey(a.CategoryID)) counts[a.CategoryID] = 0;
                        counts[a.CategoryID]++;

                        // per-title count within each category
                        string t = (a.Title ?? string.Empty).Trim();
                        if (string.IsNullOrEmpty(t)) continue;

                        if (!titlesByCategory.ContainsKey(a.CategoryID))
                            titlesByCategory[a.CategoryID] =
                                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                        var titleMap = titlesByCategory[a.CategoryID];
                        if (!titleMap.ContainsKey(t)) titleMap[t] = 0;
                        titleMap[t]++;
                    }
                }
                catch { /* article counts / titles are non-critical */ }

                _view.BindCategoryTree(categories, counts, titlesByCategory);
            }
            catch (Exception ex) { _view.ShowError("Load category failed: " + ex.Message); }
        }

        public void OnCategorySelected(int? categoryId, string categoryName)
        {
            _view.SelectedCategoryId   = categoryId;
            _view.SelectedArticleTitle = null;   // clear title-subcategory filter
            _view.ArticleTitleText     = categoryId.HasValue
                ? string.Format("  {0}", categoryName)
                : "  ทุกหมวดหมู่";
            LoadArticles();
        }

        /// <summary>
        /// Called when the user clicks a title leaf-node in treeCategories.
        /// Filters the article list to rows whose Title matches exactly.
        /// </summary>
        public void OnTitleSelected(int? categoryId, string categoryName, string title)
        {
            _view.SelectedCategoryId   = categoryId;
            _view.SelectedArticleTitle = title;
            _view.ArticleTitleText     = string.Format("  {0}  ›  {1}", categoryName, title);
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
                _view.ShowLoading();

                string keyword       = _view.SearchKeyword;
                int    searchFieldId = _view.SearchFieldId;
                byte?  statusFilter  = _view.StatusFilter;
                byte?  typeFilter    = _view.TypeFilter;
                int?   catId         = _view.SelectedCategoryId;
                string titleFilter   = _view.SelectedArticleTitle;
                int    pageSize      = _view.PageSize;

                if (!catId.HasValue) return;

                IEnumerable<KBArticle> articles;
                articles = _articleService.SearchByType(keyword, catId, statusFilter, typeFilter, searchFieldId);

                var list = articles.ToList();

                if (!string.IsNullOrEmpty(titleFilter))
                    list = list.Where(a => string.Equals(
                        (a.Title ?? string.Empty).Trim(),
                        titleFilter.Trim(),
                        StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var article in list)
                    ResolveLatestVersion(article);

                int totalCount = list.Count;
                int totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);
                if (_currentPage > totalPages) _currentPage = totalPages;
                if (_currentPage < 1)          _currentPage = 1;

                var pageItems = list.Skip((_currentPage - 1) * pageSize).Take(pageSize).ToList();
                pageItems.ForEach(x => x.ContentHTML = StripHtmlTags(x.ContentHTML));
                _view.RenderArticles(pageItems);
                _view.ArticleCountText = string.Format("{0} บทความ", totalCount);
                _view.RenderPagination(totalCount, _currentPage, pageSize);
            }
            catch (Exception ex) { _view.ShowError("Load articles failed: " + ex.Message); }
            finally
            {
                _view.HideLoading();
            }
        }

        private static IEnumerable<KBArticle> ApplyKeywordFilter(
            IEnumerable<KBArticle> articles, string keyword, int searchFieldId)
        {
            var _keyword = new string(keyword.Where(c => !char.IsWhiteSpace(c)).ToArray());
            switch (searchFieldId)
            {
                case 1:
                    
                    return articles.Where(a =>
                        a.Title != null &&
                        string.Equals(new string(a.Title.Where(c => !char.IsWhiteSpace(c)).ToArray()), _keyword, StringComparison.OrdinalIgnoreCase));
                case 2:
                    return articles.Where(a =>
                        (a.ContentHTML != null && new string(StripHtmlTags(a.ContentHTML).Where(c => !char.IsWhiteSpace(c)).ToArray()).IndexOf(_keyword, StringComparison.OrdinalIgnoreCase) >= 0));
                case 3:
                    return articles.Where(a =>
                        (a.Tags != null && a.Tags.IndexOf(_keyword, StringComparison.OrdinalIgnoreCase) >= 0));
                case 4:
                    return articles.Where(a =>
                        a.Summary != null &&
                        a.Summary.IndexOf(_keyword, StringComparison.OrdinalIgnoreCase) >= 0);
                default:
                    return articles;
            }
        }

        private static string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<br\s*/?>",  " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?p>",      " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?div>",    " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?li>",     " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>",    string.Empty);
            html = System.Net.WebUtility.HtmlDecode(html);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"\s+",        " ");
            return html.Trim();
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

        /// <summary>
        /// Resolves the latest VersionNumber from KMS.t_KBVersion and writes it
        /// back to article.CurrentVersion so lblMeta always shows the DB value.
        /// </summary>
        private void ResolveLatestVersion(KBArticle article)
        {
            try
            {
                int latest = _articleService.GetLatestVersionNumber(article.ArticleID);
                if (latest > 0)
                    article.CurrentVersion = latest;
            }
            catch { /* non-critical — keep whatever CurrentVersion was already set */ }
        }
    }
}
