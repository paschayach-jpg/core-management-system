using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    /// <summary>
    /// View interface for KBEditor dialog (MOD-006)
    /// Redesigned from ArticleEditorDialog with complete mapping tabs support
    /// </summary>
    public interface IKBEditorController
    {
        // ── General Information Fields ───────────────────────────────────────
        string ArticleCode { get; set; }
        string Title { get; set; }
        string Summary { get; set; }
        string TitleCategory { get; set; }
        string ContentHTML { get; set; }
        string Tags { get; set; }
        int SelectedCrafLevelCode { get; set; }
        System.DateTime? PublishedDate { get; set; }
        System.DateTime? EffectiveDate { get; set; }
        System.DateTime? ExpiryDate { get; set; }
        int SelectedCategoryId { get; set; }
        byte SelectedArticleType { get; set; }
        byte SelectedStatus { get; set; }
        bool StatusEnabled { set; }

        /// <summary>
        /// Restrict the status dropdown to only the given status codes (1=Draft,
        /// 2=Review, 3=Published, 4=Archived), preserving the current selection if
        /// it remains allowed.
        /// </summary>
        void SetAllowedStatuses(IEnumerable<byte> allowedStatuses);

        // ── UI Display Properties ────────────────────────────────────────────
        string HeaderText { set; }
        string ViewCountText { set; }
        string LikeCountText { set; }
        bool PublishButtonVisible { set; }
        bool ArchiveButtonVisible { set; }
        bool PublishNowButtonVisible { set; }
        bool SaveEnabled { set; }
        string PublishButtonText { set; }
        System.Drawing.Color PublishButtonColor { set; }

        // ── Status Bar Display ───────────────────────────────────────────────
        string VersionText { set; }
        string CurVersionText { set; }
        string AuthorText { set; }
        string DateInfoText { set; }

        // ── Data Binding Methods ─────────────────────────────────────────────
        void BindCategories(IEnumerable<KBCategory> categories);

        // ── Dialog Control ───────────────────────────────────────────────────
        void CloseOk();
        void CloseCancel();
        void ShowError(string message);
        void ShowInfo(string message);

        // ── DataGridView Access for Article Links ────────────────────────────────
        System.Windows.Forms.DataGridView GetRelatedDataGridView();

        // ── DataGridView Access for Attachments ──────────────────────────────
        System.Windows.Forms.DataGridView GetAttachLinkedDataGridView();

        // ── DataGridView Access for Workflows ────────────────────────────────
        System.Windows.Forms.DataGridView GetWorkflowLinkedDataGridView();
        System.Windows.Forms.DataGridView GetWorkflowSearchDataGridView();

        // ── DataGridView Access for Programs ─────────────────────────────────
        System.Windows.Forms.DataGridView GetProgramsLinkedDataGridView();
        System.Windows.Forms.DataGridView GetProgramsSearchDataGridView();

        // ── DataGridView Access for Documents ────────────────────────────────
        System.Windows.Forms.DataGridView GetDocsLinkedDataGridView();
        System.Windows.Forms.DataGridView GetDocsSearchDataGridView();
    }
}
