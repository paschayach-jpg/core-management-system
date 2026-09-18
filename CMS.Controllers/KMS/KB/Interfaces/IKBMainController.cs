using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    public interface IKBMainController
    {
        string SearchKeyword    { get; }
        byte?  StatusFilter    { get; }
        byte?  TypeFilter      { get; }
        int    SearchFieldId   { get; }
        bool   IsArchivedMode  { get; set; }
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
        /// Bind ต้นไม้หมวดหมู่ทั้งชุด รวมถึง folder อ้างอิงข้ามหมวด (มาตรา ↔ คำสั่ง/หมวดภายใน)
        /// </summary>
        void BindCategoryTree(KBCategoryTreeData data);
        int PageSize { get; }
        void RenderArticles(IEnumerable<KBArticle> articles);
        void RenderPagination(int totalCount, int currentPage, int pageSize);
        void ShowLoading();
        void HideLoading();
        void ShowError(string message);
        void ShowReport(string reportText);
    }
}