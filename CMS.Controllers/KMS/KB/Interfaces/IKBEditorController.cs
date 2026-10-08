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

        /// <summary>true = พิมพ์ Code รวมได้ (เฉพาะบทความใหม่); บทความเดิมแก้ Code ไม่ได้</summary>
        bool ArticleCodeEditable { set; }

        /// <summary>true = เลือกหมวดเองได้ — false เมื่อหมวดคำนวณจาก Code (กฎหมาย/ไม่ใช่กฎหมาย) หรือเป็นบทความเดิม (ย้ายหมวดที่หน้า "ย้ายบทความ")</summary>
        bool CategoryEditable { set; }

        /// <summary>false เมื่อชื่อคำนวณจาก Code ("มาตรา N" ของกฎหมาย/พระราชกฤษฎีกา)</summary>
        bool TitleEditable { set; }

        /// <summary>ข้อความบอกผลการตรวจ Code รวม (ชนิด/ชื่อ/หมวดที่ได้ หรือเหตุที่ใช้ไม่ได้) ข้างช่อง ArticleCode</summary>
        void SetArticleCodeHint(string text, bool isError);

        // ── UI Display Properties ────────────────────────────────────────────
        string HeaderText { set; }
        string ViewCountText { set; }
        string LikeCountText { set; }
        bool PublishButtonVisible { set; }
        bool ArchiveButtonVisible { set; }
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
        void ShowError(string message);
        void ShowInfo(string message);

        // ── DataGridView Access for Article Links ────────────────────────────────
        System.Windows.Forms.DataGridView GetRelatedDataGridView();

        // ── DataGridView Access for Attachments ──────────────────────────────
        System.Windows.Forms.DataGridView GetAttachLinkedDataGridView();
    }
}
