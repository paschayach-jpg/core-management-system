using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Controllers.KMS.Interfaces
{
    /// <summary>
    /// View interface for KBEditor dialog (MOD-006)
    /// Redesigned from ArticleEditorDialog with complete mapping tabs support
    /// </summary>
    public interface IKBEditorView
    {
        // ── General Information Fields ───────────────────────────────────────
        string ArticleCode { get; set; }
        string Title { get; set; }
        string Summary { get; set; }
        string ContentHTML { get; set; }
        string Tags { get; set; }
        string SelectedCrafLevel { get; set; }
        int SelectedCategoryId { get; set; }
        byte SelectedArticleType { get; set; }
        byte SelectedStatus { get; set; }

        // ── UI Display Properties ────────────────────────────────────────────
        string HeaderText { set; }
        string ViewCountText { set; }
        string LikeCountText { set; }
        string VersionTabText { set; }
        bool PublishButtonVisible { set; }
        bool ArchiveButtonVisible { set; }
        bool SaveEnabled { set; }
        string PublishButtonText { set; }
        System.Drawing.Color PublishButtonColor { set; }

        // ── Status Bar Display ───────────────────────────────────────────────
        string VersionText { set; }
        string AuthorText { set; }
        string DateInfoText { set; }

        // ── Data Binding Methods ─────────────────────────────────────────────
        void BindCategories(IEnumerable<KBCategory> categories);
        void BindVersionHistory(IEnumerable<KBVersion> versions);

        // ── Dialog Control ───────────────────────────────────────────────────
        void CloseOk();
        void CloseCancel();
        void ShowError(string message);
        void ShowInfo(string message);
    }
}
