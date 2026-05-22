using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    public interface IKBMainController
    {
        string SearchKeyword  { get; }
        byte?  StatusFilter   { get; }
        byte?  TypeFilter     { get; }
        int    SearchFieldId  { get; }
        int?   SelectedCategoryId    { get; set; }
        /// <summary>
        /// When set by the presenter, LoadArticles filters results to articles whose
        /// Title matches this value exactly (same semantics as Search-by-Title).
        /// Reset to null when the user selects a plain category node.
        /// </summary>
        string SelectedArticleTitle  { get; set; }
        string ArticleTitleText { set; }
        string ArticleCountText { set; }
        /// <summary>
        /// titlesByCategory maps CategoryID → (Title → article count).
        /// Used to render "📄 Title (N)" leaf nodes under each category.
        /// </summary>
        void BindCategoryTree(
            IEnumerable<KBCategory>                  categories,
            IDictionary<int, int>                    articleCounts,
            IDictionary<int, IDictionary<string,int>> titlesByCategory);
        int PageSize { get; }
        void RenderArticles(IEnumerable<KBArticle> articles);
        void RenderPagination(int totalCount, int currentPage, int pageSize);
        void ShowLoading();
        void HideLoading();
        void ShowError(string message);
        void ShowReport(string reportText);
    }
}