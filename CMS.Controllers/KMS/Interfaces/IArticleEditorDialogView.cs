using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Controllers.KMS.Interfaces
{
    public interface IArticleEditorDialogView
    {
        string ArticleCode { get; set; }
        string Title { get; set; }
        string Summary { get; set; }
        string ContentHTML { get; set; }
        string Tags { get; set; }
        bool IsOfficialPolicy { get; set; }
        bool NoExpiry { get; set; }
        System.DateTime? ExpiryDate { get; set; }
        int SelectedCategoryId { get; set; }
        byte SelectedArticleType { get; set; }
        byte SelectedStatus { get; set; }
        string HeaderText { set; }
        string ViewCountText { set; }
        string LikeCountText { set; }
        string VersionTabText { set; }
        bool PublishButtonVisible { set; }
        bool ArchiveButtonVisible { set; }
        bool SaveEnabled { set; }
        string PublishButtonText { set; }
        System.Drawing.Color PublishButtonColor { set; }
        void BindCategories(IEnumerable<KBCategory> categories);
        void BindVersionHistory(IEnumerable<KBVersion> versions);
        void CloseOk();
        void CloseCancel();
        void ShowError(string message);
        void ShowInfo(string message);

        // Status-bar display labels
        string VersionText   { set; }
        string AuthorText    { set; }
        string DateInfoText  { set; }
    }
}
