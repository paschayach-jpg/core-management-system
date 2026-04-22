using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    public interface IKBCategoryController
    {
        string CategoryCode { get; set; }
        string CategoryName { get; set; }
        string Icon { get; set; }
        string Description { get; set; }
        int SortOrder { get; set; }
        int? ParentCategoryId { get; set; }
        int? EditingId { get; set; }
        string FormTitleText { set; }
        bool DeleteEnabled { set; }
        string DeleteButtonText { set; }
        void BindCategories(IEnumerable<KBCategory> categories);
        void BindCategoryDetails(KBCategory category);
        void BindCategoryStats(int total, int published, int draft);
        void BindArticleUsage(IEnumerable<KBArticle> articles);
        void ClearForm();
        void ShowError(string message);
        void ShowSuccess(string message);
    }
}