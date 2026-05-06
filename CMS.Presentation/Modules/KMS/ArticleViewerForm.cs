// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// KMS-03 Article Viewer — แสดงรายละเอียดบทความ Knowledge Base
    /// ฝังใน pnlViewerHost ของ KBMainForm (TopLevel = false)
    /// </summary>
    public partial class ArticleViewerForm : Form, IArticleViewerController
    {
        private ArticleViewerController       _presenter;
        private Func<KBArticle, DialogResult> _openEditorFactory;
        private KBArticle                     _article;
        private IEnumerable<KBArticle>        _relatedArticles = new List<KBArticle>();

        /// <summary>The ArticleID currently displayed; used by KBMainForm history stack.</summary>
        public int CurrentArticleId { get { return _article != null ? _article.ArticleID : 0; } }

        /// <summary>Fired when the user clicks "← กลับ".</summary>
        public event EventHandler BackRequested;

        /// <summary>Fired when the user clicks a related article row.</summary>
        public event EventHandler<KBArticle> ArticleOpenRequested;

        public ArticleViewerForm()
        {
            InitializeComponent();
        }

        public void AttachPresenter(ArticleViewerController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        public void AttachEditorFactory(Func<KBArticle, DialogResult> factory)
        {
            _openEditorFactory = factory;
        }

        // ── Form events ──────────────────────────────────────────────────────

        private void ArticleViewerForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            AdjustSplitter();
            splitViewer.Resize += (s, ev) => AdjustSplitter();
            _presenter.Initialize();
            // Default tab — populate attachments immediately
            PopulateAttachTab();
        }

        // ── IArticleViewerView ───────────────────────────────────────────────

        public void PopulateArticle(KBArticle article)
        {
            if (article == null) throw new ArgumentNullException("article");
            _article = article;
            PopulateHeader();
            PopulateInfoBox();
            BeginInvoke(new Action(LoadContent));
        }

        public void BindVersionHistory(IEnumerable<KBVersion> versions)
        {
            // Kept for interface compatibility — version history not shown in this view.
        }

        public void BindRelatedArticles(IEnumerable<KBArticle> articles)
        {
            _relatedArticles = articles ?? new List<KBArticle>();
            // Refresh the Related tab if it is currently selected
            if (tabMapping.SelectedTab == tabRelated)
                PopulateRelatedTab();
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Toolbar button handlers ──────────────────────────────────────────

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_presenter == null || _openEditorFactory == null || _article == null) return;
            if (_openEditorFactory(_article) == DialogResult.OK)
                _presenter.RefreshAfterEdit();
        }

        private void BtnShare_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            Clipboard.SetText(string.Format("{0}  [{1}]", _article.Title, _article.ArticleCode));
            ShowInfo("คัดลอกข้อมูลบทความไปยัง Clipboard แล้ว");
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try { webContent.Print(); }
            catch { ShowInfo("ไม่สามารถพิมพ์ได้"); }
        }

        private void BtnExportPdf_Click(object sender, EventArgs e)
        {
            ShowInfo("ฟังก์ชัน Export PDF อยู่ระหว่างการพัฒนา");
        }

        private void BtnPin_Click(object sender, EventArgs e)
        {
            ShowInfo("ฟังก์ชัน Pin Article อยู่ระหว่างการพัฒนา");
        }

        // ── TabControl handler ───────────────────────────────────────────────

        private void TabMapping_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tab = tabMapping.SelectedTab;
            if      (tab == tabAttach)      PopulateAttachTab();
            else if (tab == tabRelated)     PopulateRelatedTab();
            else if (tab == tabWorkflow)    PopulateWorkflowTab();
            else if (tab == tabPrograms)    PopulateProgramsTab();
            else if (tab == tabDocs)        PopulateDocsTab();
            else if (tab == tabDepartments) PopulateDepartmentsTab();
        }

        // ── Workflow button handler ──────────────────────────────────────────

        private void BtnStartWorkflow_Click(object sender, EventArgs e)
        {
            ShowInfo("เปิด Workflow Module");
        }

        // ── Related grid click ───────────────────────────────────────────────

        private void DgvRelatedLinked_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var article = dgvRelatedLinked.Rows[e.RowIndex].Tag as KBArticle;
            if (article != null && ArticleOpenRequested != null)
                ArticleOpenRequested(this, article);
        }

        // ── WebBrowser handlers ──────────────────────────────────────────────

        private void webContent_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webContent.Document != null)
            {
                webContent.Document.Click -= WebContent_Document_Click;
                webContent.Document.Click += WebContent_Document_Click;
            }
        }

        private void WebContent_Document_Click(object sender, HtmlElementEventArgs e)
        {
            var element = webContent.Document.GetElementFromPoint(e.ClientMousePosition);
            if (element == null) return;
            while (element != null && element.TagName.ToLower() != "a")
                element = element.Parent;
            if (element == null || element.TagName.ToLower() != "a") return;

            string href = element.GetAttribute("href");
            if (string.IsNullOrWhiteSpace(href)) return;

            if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                href.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                e.ReturnValue = false;
                try { System.Diagnostics.Process.Start(href); }
                catch (Exception ex) { ShowError("ไม่สามารถเปิดลิงก์ได้: " + ex.Message); }
            }
        }

        // ── Tab populate methods (data binding only) ─────────────────────────

        private void PopulateAttachTab()
        {
            // Mockup — replace with real DMS data when available
            dgvAttachLinked.Rows.Clear();
            dgvAttachLinked.Rows.Add("1", "Claims_Form_2569.pdf",   "245 KB", "01/01/2569");
            dgvAttachLinked.Rows.Add("2", "UW_Criteria_Table.xlsx", "89 KB",  "01/01/2569");
            lblAttachLinked.Text = string.Format("ไฟล์ที่แนบ ({0} ไฟล์)", dgvAttachLinked.Rows.Count);
        }

        private void PopulateRelatedTab()
        {
            dgvRelatedLinked.Rows.Clear();
            var articles = _relatedArticles.ToList();
            int i = 1;
            foreach (var a in articles)
            {
                int idx = dgvRelatedLinked.Rows.Add(
                    i.ToString(),
                    a.ArticleCode ?? "-",
                    a.Title ?? "(ไม่มีชื่อ)",
                    a.ArticleTypeName ?? "-");
                dgvRelatedLinked.Rows[idx].Tag = a;
                i++;
            }
            lblRelatedLinked.Text = string.Format("บทความที่เกี่ยวข้อง ({0} รายการ)", dgvRelatedLinked.Rows.Count);
        }

        private void PopulateWorkflowTab()
        {
            // Mockup — replace with real workflow data when available
            dgvWorkflowLinked.Rows.Clear();
            dgvWorkflowLinked.Rows.Add("1", "RG01000", "การรับแจ้งอุบัติเหตุ", "Active");
            dgvWorkflowLinked.Rows.Add("2", "RG02000", "การออกสำรวจภัย",       "Active");
            lblWorkflowLinked.Text = string.Format("เส้นงานที่เกี่ยวข้อง ({0} รายการ)", dgvWorkflowLinked.Rows.Count);
        }

        private void PopulateProgramsTab()
        {
            // Mockup — replace with real program data when available
            dgvProgramsLinked.Rows.Clear();
            dgvProgramsLinked.Rows.Add("1", "PRG001", "RISC Claim",     "ฝ่ายสินไหมทดแทน");
            dgvProgramsLinked.Rows.Add("2", "PRG002", "ทะเบียนสินไหม", "ฝ่ายสินไหมทดแทน");
            dgvProgramsLinked.Rows.Add("3", "PRG003", "New E-Filing",   "IT");
            dgvProgramsLinked.Rows.Add("4", "PRG004", "BIS Claim",      "ฝ่ายสินไหมทดแทน");
            dgvProgramsLinked.Rows.Add("5", "PRG005", "TFRS17",         "แผนกบัญชี");
            lblProgramsLinked.Text = string.Format("โปรแกรมที่เกี่ยวข้อง ({0} รายการ)", dgvProgramsLinked.Rows.Count);
        }

        private void PopulateDocsTab()
        {
            // Mockup — replace with real document data when available
            dgvDocsLinked.Rows.Clear();
            dgvDocsLinked.Rows.Add("1", "FM-CLM-001 แบบฟอร์มรับแจ้งสินไหม",       "Rev.5", "3NC");
            dgvDocsLinked.Rows.Add("2", "FM-CLM-002 แบบฟอร์มประเมินความเสียหาย", "Rev.3", "3NC");
            lblDocsLinked.Text = string.Format("เอกสารที่เกี่ยวข้อง ({0} รายการ)", dgvDocsLinked.Rows.Count);
        }

        private void PopulateDepartmentsTab()
        {
            // Mockup — replace with real org data when available
            dgvDepartmentsLinked.Rows.Clear();
            dgvDepartmentsLinked.Rows.Add("1", "CLM", "ฝ่ายสินไหมทดแทน", "ฝ่าย");
            dgvDepartmentsLinked.Rows.Add("2", "UW",  "ฝ่ายรับประกันภัย", "ฝ่าย");
            lblDepartmentsLinked.Text = string.Format("ฝ่าย/แผนก ที่เกี่ยวข้อง ({0} รายการ)", dgvDepartmentsLinked.Rows.Count);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private void AdjustSplitter()
        {
            int available = splitViewer.Width
                            - splitViewer.Panel2MinSize
                            - splitViewer.SplitterWidth;
            if (available >= splitViewer.Panel1MinSize)
                splitViewer.SplitterDistance = available;
        }

        private void PopulateHeader()
        {
            lblViewerTitle.Text = "  👁️ ดูบทความ: " + (_article.Title ?? "(ไม่มีชื่อ)");
        }

        private void PopulateInfoBox()
        {
            lblTypeBadge.Text      = _article.ArticleTypeName.ToUpper();
            lblTypeBadge.BackColor = GetTypeBadgeColor(_article.ArticleType);

            lblCategoryBadge.Text = _article.CategoryName ?? "-";

            Color statusFore, statusBack;
            GetStatusColors(_article.Status, out statusFore, out statusBack);
            lblStatusBadge.ForeColor = statusFore;
            lblStatusBadge.BackColor = statusBack;
            lblStatusBadge.Text      = GetStatusText(_article.Status);

            lblOfficialBadge.Visible = _article.IsOfficialPolicy;
            if (_article.IsOfficialPolicy && !flpBadges.Controls.Contains(lblOfficialBadge))
                flpBadges.Controls.Add(lblOfficialBadge);

            lblCodeVersion.Text  = string.Format("รหัส: {0}  |  Version {1}",
                _article.ArticleCode ?? "-", _article.CurrentVersion);
            lblAuthorVal.Text    = "ผู้เขียน: "  + (_article.CreatedBy ?? "-");
            lblCategoryVal.Text  = "หมวดหมู่: "  + (_article.CategoryName ?? "-");
            lblPublishVal.Text   = _article.PublishedDate.HasValue
                ? "Publish: " + _article.PublishedDate.Value.ToString("dd MMM yyyy")
                : "Publish: -";
            lblConfidential.Text = "ระดับความลับ: -";
            lblReviewDueVal.Text = _article.ReviewDueDate.HasValue
                ? "Review Due: " + _article.ReviewDueDate.Value.ToString("dd MMM yyyy")
                : "Review Due: -";
        }

        private void LoadContent()
        {
            try
            {
                string body = _article.ContentHTML ?? string.Empty;
                string html = string.Format(
                    "<!DOCTYPE html><html><head><meta charset='utf-8'>" +
                    "<style>" +
                    "body{{font-family:'Segoe UI',Tahoma,sans-serif;font-size:13px;" +
                        "margin:12px 16px;color:#333;line-height:1.8}}" +
                    "h2{{font-size:16px;color:#1E3A5F;border-bottom:2px solid #2E74B5;" +
                        "padding-bottom:4px;margin:0 0 10px}}" +
                    "h3{{font-size:14px;color:#2E74B5;margin:10px 0 5px}}" +
                    "p{{margin-bottom:8px}}" +
                    "pre,code{{background:#f4f6f8;padding:3px 7px;border-radius:3px;" +
                        "font-family:Consolas,monospace;font-size:12px}}" +
                    "table{{border-collapse:collapse;width:100%;margin:8px 0}}" +
                    "td,th{{border:1px solid #ddd;padding:6px 10px;text-align:left}}" +
                    "th{{background:#2E74B5;color:#fff}}" +
                    "a{{color:#2E74B5}}" +
                    "</style></head><body>{0}</body></html>",
                    body);
                webContent.DocumentText = html;
            }
            catch
            {
                webContent.DocumentText =
                    "<p style='font-family:Segoe UI;color:#888'>ไม่สามารถแสดงเนื้อหาได้</p>";
            }
        }

        // ── Colour helpers ────────────────────────────────────────────────────

        private static Color GetTypeBadgeColor(byte articleType)
        {
            switch (articleType)
            {
                case 1:  return Color.FromArgb(231, 76, 60);
                case 2:  return Color.FromArgb(243, 156, 18);
                case 3:  return Color.FromArgb(142, 68, 173);
                case 4:  return Color.FromArgb(22, 160, 133);
                case 5:  return Color.FromArgb(46, 116, 181);
                default: return Color.FromArgb(100, 110, 130);
            }
        }

        private static void GetStatusColors(byte status, out Color fore, out Color back)
        {
            switch (status)
            {
                case 1:  fore = Color.FromArgb(192, 57, 43);   back = Color.FromArgb(253, 237, 236); break;
                case 2:  fore = Color.FromArgb(183, 119, 13);  back = Color.FromArgb(254, 249, 231); break;
                case 3:  fore = Color.FromArgb(30, 132, 73);   back = Color.FromArgb(213, 245, 227); break;
                case 4:  fore = Color.FromArgb(127, 140, 141); back = Color.FromArgb(238, 238, 238); break;
                default: fore = Color.Gray;                    back = Color.FromArgb(238, 238, 238); break;
            }
        }

        private static string GetStatusText(byte status)
        {
            switch (status)
            {
                case 1:  return "Draft";
                case 2:  return "รอ Review";
                case 3:  return "✅ Published";
                case 4:  return "Archived";
                default: return "Unknown";
            }
        }
    }
}
