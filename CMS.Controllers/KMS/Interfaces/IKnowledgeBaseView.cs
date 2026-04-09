using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Controllers.KBS.Interfaces
{
    public interface IKnowledgeBaseView
    {
        string SearchKeyword { get; }
        int StatusFilterIndex { get; }
        int? SelectedCategoryId { get; set; }
        string ArticleTitleText { set; }
        string ArticleCountText { set; }
        void BindCategoryTree(IEnumerable<KBCategory> categories);
        void RenderArticles(IEnumerable<KBArticle> articles);
        void ShowError(string message);
        void ShowReport(string reportText);
    }
}