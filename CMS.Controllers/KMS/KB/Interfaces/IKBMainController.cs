using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    public interface IKBMainController
    {
        string SearchKeyword  { get; }
        int    StatusFilterIndex { get; }
        int    SearchFieldId  { get; }
        int? SelectedCategoryId { get; set; }
        string ArticleTitleText { set; }
        string ArticleCountText { set; }
        void BindCategoryTree(IEnumerable<KBCategory> categories, System.Collections.Generic.IDictionary<int, int> articleCounts);
        int PageSize { get; }
        void RenderArticles(IEnumerable<KBArticle> articles);
        void RenderPagination(int totalCount, int currentPage, int pageSize);
        void ShowError(string message);
        void ShowReport(string reportText);
    }
}