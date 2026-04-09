using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// Dialog สำหรับเพิ่ม/แก้ไขบทความ Knowledge Base (MOD-006)
    /// Layout ตาม kms_editor.html — 3 sections: General Info, Content Editor, Mapping Tabs
    /// รองรับ: Create / Edit / Send Review / Publish / Archive + Version History
    /// </summary>
    public partial class ArticleEditorDialog : Form
    {
        private readonly IKBCategoryService _categoryService;
        private readonly IKBArticleService  _articleService;
        private readonly UserContext        _user;
        private readonly KBArticle          _editingArticle; // null = new article

        private List<KBCategory> _categories = new List<KBCategory>();

        public ArticleEditorDialog(
            IKBCategoryService categoryService,
            IKBArticleService  articleService,
            UserContext        user,
            KBArticle          article)   // null = create new
        {
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (user            == null) throw new ArgumentNullException("user");
            _categoryService = categoryService;
            _articleService  = articleService;
            _user            = user;
            _editingArticle  = article;
            InitializeComponent();
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private void ArticleEditorDialog_Load(object sender, EventArgs e)
        {
            LoadDropdowns();

            if (_editingArticle == null)
            {
                // New article — Draft defaults
                Text = "เพิ่มบทความใหม่ — Knowledge Base | KMS";
                lblHeader.Text      = "  ✏️ เพิ่มบทความใหม่";
                lblBreadcrumb.Text  = "KMS › บทความใหม่";
                txtCode.Text        = GenerateArticleCodePreview();
                cboType.SelectedIndex = 0;
                chkNoExpiry.Checked = true;
                dtpExpiry.Enabled   = false;
                btnArchive.Visible  = false;

                SetStatusDisplay(1);
                SetStatusBarInfo(null);
            }
            else
            {
                Text = string.Format("แก้ไขบทความ: {0} — KMS", _editingArticle.Title ?? "");
                lblHeader.Text     = string.Format("  ✏️ แก้ไขบทความ: {0}", _editingArticle.Title ?? "");
                lblBreadcrumb.Text = string.Format("KMS › {0} › แก้ไขบทความ",
                    _editingArticle.CategoryName ?? "หมวดหมู่");

                PopulateFields(_editingArticle);
                LoadVersionHistory(_editingArticle.ArticleID);
                UpdateActionButtons(_editingArticle.Status);
            }
        }

        // ── Dropdowns ─────────────────────────────────────────────────────────

        private void LoadDropdowns()
        {
            try { _categories = _categoryService.GetAllActive().ToList(); }
            catch { _categories = new List<KBCategory>(); }

            cboCategory.Items.Clear();
            cboCategory.Items.Add(new ComboItem("— เลือกหมวดหมู่ —", 0));
            foreach (var c in _categories.OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName))
                cboCategory.Items.Add(new ComboItem(
                    c.ParentCategoryID.HasValue
                        ? string.Format("  └ {0}", c.CategoryName)
                        : c.CategoryName,
                    c.CategoryID));
            cboCategory.SelectedIndex = 0;

            cboType.Items.Clear();
            cboType.Items.Add(new ComboItem("SOP (Standard Operating Procedure)", 1));
            cboType.Items.Add(new ComboItem("FAQ (คำถามที่พบบ่อย)", 2));
            cboType.Items.Add(new ComboItem("Guideline (แนวปฏิบัติ)", 3));
            cboType.Items.Add(new ComboItem("Template (แม่แบบเอกสาร)", 4));
            cboType.Items.Add(new ComboItem("Policy (นโยบาย)", 5));
            cboType.SelectedIndex = 0;
        }

        // ── Populate fields from existing article ─────────────────────────────

        private void PopulateFields(KBArticle a)
        {
            txtCode.Text    = a.ArticleCode ?? string.Empty;
            txtTitle.Text   = a.Title       ?? string.Empty;
            txtSummary.Text = a.Summary     ?? string.Empty;
            txtContent.Text = a.ContentHTML ?? string.Empty;
            txtTags.Text    = a.Tags        ?? string.Empty;
            chkOfficialPolicy.Checked = a.IsOfficialPolicy;

            SelectComboByValue(cboCategory, a.CategoryID);
            SelectComboByValue(cboType, (int)a.ArticleType);

            SetStatusDisplay(a.Status);
            lblCurVersion.Text = string.Format("v{0}.0", a.CurrentVersion > 0 ? a.CurrentVersion : 1);

            // Expiry
            if (a.ExpiryDate.HasValue)
            {
                chkNoExpiry.Checked = false;
                dtpExpiry.Enabled   = true;
                dtpExpiry.Value     = a.ExpiryDate.Value;
            }
            else
            {
                chkNoExpiry.Checked = true;
                dtpExpiry.Enabled   = false;
            }

            lblViewCount.Text = string.Format("👁  ดู {0} ครั้ง", a.ViewCount);
            lblLikeCount.Text = string.Format("👍  {0} Like", a.LikeCount);

            SetStatusBarInfo(a);
        }

        // ── Status display ────────────────────────────────────────────────────

        private void SetStatusDisplay(byte status)
        {
            switch (status)
            {
                case 1:
                    lblStatusDisplay.Text      = "📝  Draft";
                    lblStatusDisplay.BackColor = Color.FromArgb(253, 237, 236);
                    lblStatusDisplay.ForeColor = Color.FromArgb(146, 43, 33);
                    lblStatusInfo.Text         = "📝 Draft";
                    break;
                case 2:
                    lblStatusDisplay.Text      = "🔍  รอ Review";
                    lblStatusDisplay.BackColor = Color.FromArgb(254, 249, 231);
                    lblStatusDisplay.ForeColor = Color.FromArgb(126, 81, 9);
                    lblStatusInfo.Text         = "🔍 รอ Review";
                    break;
                case 3:
                    lblStatusDisplay.Text      = "✅  Published";
                    lblStatusDisplay.BackColor = Color.FromArgb(213, 245, 227);
                    lblStatusDisplay.ForeColor = Color.FromArgb(30, 132, 73);
                    lblStatusInfo.Text         = "✅ Published";
                    break;
                case 4:
                    lblStatusDisplay.Text      = "📦  Archived";
                    lblStatusDisplay.BackColor = Color.FromArgb(236, 240, 241);
                    lblStatusDisplay.ForeColor = Color.FromArgb(127, 140, 141);
                    lblStatusInfo.Text         = "📦 Archived";
                    break;
                default:
                    lblStatusDisplay.Text      = "—";
                    lblStatusDisplay.BackColor = Color.FromArgb(245, 247, 250);
                    lblStatusDisplay.ForeColor = Color.FromArgb(74, 85, 104);
                    lblStatusInfo.Text         = "—";
                    break;
            }
        }

        private void SetStatusBarInfo(KBArticle a)
        {
            if (a == null)
            {
                lblVerInfo.Text    = "Version: 1.0";
                lblAuthorInfo.Text = string.Format("สร้างโดย: {0}", _user.AdIdentity);
                lblDateInfo.Text   = string.Format("วันที่: {0}", DateTime.Now.ToString("dd/MM/yyyy"));
            }
            else
            {
                lblVerInfo.Text    = string.Format("Version: {0}.0", a.CurrentVersion > 0 ? a.CurrentVersion : 1);
                lblAuthorInfo.Text = string.Format("สร้างโดย: {0}", a.CreatedBy ?? "—");
                lblDateInfo.Text   = string.Format("วันที่: {0}", a.CreatedDate.ToString("dd/MM/yyyy"));
            }
        }

        // ── Version History ───────────────────────────────────────────────────

        private void LoadVersionHistory(int articleId)
        {
            lstVersions.Items.Clear();
            try
            {
                var versions = _articleService.GetVersionHistory(articleId).ToList();
                foreach (var v in versions)
                {
                    var item = new ListViewItem(string.Format("v{0}", v.VersionNumber));
                    item.SubItems.Add(v.CreatedDate.ToString("dd/MM/yyyy HH:mm"));
                    item.SubItems.Add(v.CreatedBy ?? "-");
                    item.SubItems.Add(v.ChangeDescription ?? "-");
                    item.Tag       = v;
                    item.BackColor = v.VersionNumber % 2 == 0
                        ? Color.White
                        : Color.FromArgb(248, 250, 253);
                    lstVersions.Items.Add(item);
                }
                tabVersions.Text = string.Format("🕐 ประวัติ Version ({0})", versions.Count);
            }
            catch { /* non-critical */ }
        }

        // ── Action button state ───────────────────────────────────────────────

        private void UpdateActionButtons(byte status)
        {
            bool isArchived  = (status == 4);
            bool isPublished = (status == 3);

            btnSaveDraft.Enabled  = !isArchived;
            btnSendReview.Enabled = !isArchived && !isPublished;
            btnPreview.Enabled    = true;

            if (status == 1)   // Draft
            {
                btnSendReview.Text      = "📤 ส่งขอ Review";
                btnSendReview.BackColor = Color.FromArgb(46, 116, 181);
            }
            else if (status == 2)   // Review — reviewer can publish
            {
                btnSendReview.Text      = "✅ เผยแพร่";
                btnSendReview.BackColor = Color.FromArgb(39, 174, 96);
            }

            btnArchive.Visible = isPublished;

            if (isArchived)
            {
                btnSaveDraft.Text    = "🔒 (Archived)";
                btnSaveDraft.Enabled = false;
            }
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void ChkNoExpiry_CheckedChanged(object sender, EventArgs e)
        {
            dtpExpiry.Enabled = !chkNoExpiry.Checked;
        }

        // 💾 Save as Draft
        private void BtnSaveDraft_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            try
            {
                var article = BuildArticleFromUI();
                if (_editingArticle == null)
                {
                    _articleService.Create(article);
                    MessageBox.Show("เพิ่มบทความสำเร็จ", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    article.ArticleID = _editingArticle.ArticleID;
                    _articleService.Update(article, txtChangeDesc.Text.Trim());
                    MessageBox.Show("บันทึกบทความสำเร็จ", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 📤 Send Review / ✅ Publish
        private void BtnSendReview_Click(object sender, EventArgs e)
        {
            if (_editingArticle == null)
            {
                MessageBox.Show("กรุณาบันทึกบทความก่อนส่ง Review", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            byte currentStatus = _editingArticle.Status;
            byte targetStatus  = currentStatus == 1 ? (byte)2 : (byte)3;
            string actionText  = targetStatus == 2 ? "ส่งขอ Review" : "เผยแพร่";

            var confirm = MessageBox.Show(
                string.Format("ต้องการ{0}บทความ \"{1}\" ใช่หรือไม่?",
                    actionText, _editingArticle.Title),
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                _articleService.ChangeStatus(_editingArticle.ArticleID, targetStatus, _user.AdIdentity);
                MessageBox.Show(string.Format("{0}สำเร็จ", actionText), "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 👁️ Preview
        private void BtnPreview_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                string.Format("Preview: {0}\n\n(ฟีเจอร์ Preview จะเปิดในหน้าต่างใหม่ — Phase 2)",
                    txtTitle.Text.Trim()),
                "Preview บทความ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 📦 Archive
        private void BtnArchive_Click(object sender, EventArgs e)
        {
            if (_editingArticle == null) return;

            var confirm = MessageBox.Show(
                string.Format("ต้องการ Archive บทความ \"{0}\" ใช่หรือไม่?\n\n" +
                              "(บทความที่ Archive แล้วไม่สามารถนำกลับมาแก้ไขได้)",
                    _editingArticle.Title),
                "ยืนยัน Archive", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                _articleService.ChangeStatus(_editingArticle.ArticleID, 4, _user.AdIdentity);
                MessageBox.Show("Archive บทความสำเร็จ", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ❌ Cancel
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── RTE Toolbar handlers ──────────────────────────────────────────────

        private void TsRteBold_Click(object sender, EventArgs e)
        {
            if (txtContent.SelectionLength == 0) return;
            bool isBold = txtContent.SelectionFont != null && txtContent.SelectionFont.Bold;
            txtContent.SelectionFont = new Font(
                txtContent.SelectionFont ?? txtContent.Font,
                isBold ? FontStyle.Regular : FontStyle.Bold);
            txtContent.Focus();
        }

        private void TsRteItalic_Click(object sender, EventArgs e)
        {
            if (txtContent.SelectionLength == 0) return;
            bool isItalic = txtContent.SelectionFont != null && txtContent.SelectionFont.Italic;
            txtContent.SelectionFont = new Font(
                txtContent.SelectionFont ?? txtContent.Font,
                isItalic ? FontStyle.Regular : FontStyle.Italic);
            txtContent.Focus();
        }

        private void TsRteUnder_Click(object sender, EventArgs e)
        {
            if (txtContent.SelectionLength == 0) return;
            bool isUnder = txtContent.SelectionFont != null && txtContent.SelectionFont.Underline;
            txtContent.SelectionFont = new Font(
                txtContent.SelectionFont ?? txtContent.Font,
                isUnder ? FontStyle.Regular : FontStyle.Underline);
            txtContent.Focus();
        }

        private void TsRteH1_Click(object sender, EventArgs e) { InsertAtLineStart("# "); }
        private void TsRteH2_Click(object sender, EventArgs e) { InsertAtLineStart("## "); }
        private void TsRteH3_Click(object sender, EventArgs e) { InsertAtLineStart("### "); }
        private void TsRteBullet_Click(object sender, EventArgs e) { InsertAtLineStart("- "); }
        private void TsRteNumList_Click(object sender, EventArgs e) { InsertAtLineStart("1. "); }

        private void TsRteLink_Click(object sender, EventArgs e)
        {
            string sel  = txtContent.SelectedText;
            string text = string.IsNullOrEmpty(sel) ? "ข้อความลิงก์" : sel;
            int    pos  = txtContent.SelectionStart;
            txtContent.SelectedText    = string.Format("[{0}](URL)", text);
            txtContent.SelectionStart  = pos + text.Length + 3;
            txtContent.SelectionLength = 3;
            txtContent.Focus();
        }

        private void TsRteImage_Click(object sender, EventArgs e)
        {
            txtContent.SelectedText = "![คำอธิบายรูปภาพ](URL)";
            txtContent.Focus();
        }

        private void TsRteAttach_Click(object sender, EventArgs e)
        {
            tabMapping.SelectedTab = tabAttach;
        }

        private void TsRteUndo_Click(object sender, EventArgs e)
        {
            if (txtContent.CanUndo) txtContent.Undo();
            txtContent.Focus();
        }

        private void TsRteRedo_Click(object sender, EventArgs e)
        {
            if (txtContent.CanRedo) txtContent.Redo();
            txtContent.Focus();
        }

        private void TxtContent_TextChanged(object sender, EventArgs e)
        {
            lblRteStatus.Text = string.Format(
                "🖊️ กำลังแก้ไข  |  จำนวนอักขระ: {0}", txtContent.TextLength);
        }

        // ── Build / Validate ──────────────────────────────────────────────────

        /// <summary>
        /// เรียก service เพื่อดึง ArticleCode ถัดไป (KB-YYYY-NNN) สำหรับแสดงใน txtCode
        /// หาก DB ไม่พร้อมจะคืนค่าว่าง ซึ่ง KBArticleService.Create() จะสร้างใหม่ขณะ Save
        /// </summary>
        private string GenerateArticleCodePreview()
        {
            try
            {
                return _articleService.GenerateNextArticleCode();
            }
            catch
            {
                // ไม่สามารถติดต่อ DB ได้ — ปล่อยว่าง ให้ service สร้างขณะ Save
                return string.Empty;
            }
        }

        private KBArticle BuildArticleFromUI()
        {
            var catItem  = cboCategory.SelectedItem as ComboItem;
            var typeItem = cboType.SelectedItem     as ComboItem;

            return new KBArticle
            {
                ArticleCode      = txtCode.Text.Trim(),
                Title            = txtTitle.Text.Trim(),
                Summary          = txtSummary.Text.Trim(),
                ContentHTML      = txtContent.Text.Trim(),
                CategoryID       = catItem  != null ? catItem.Value  : 0,
                ArticleType      = typeItem != null ? (byte)typeItem.Value : (byte)1,
                Status           = 1,   // always saved as Draft
                IsOfficialPolicy = chkOfficialPolicy.Checked,
                Tags             = txtTags.Text.Trim(),
                ExpiryDate       = chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value,
                CreatedBy        = _user.AdIdentity,
            };
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อบทความ", "กรุณากรอกข้อมูล",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return false;
            }
            var catItem = cboCategory.SelectedItem as ComboItem;
            if (catItem == null || catItem.Value == 0)
            {
                MessageBox.Show("กรุณาเลือกหมวดหมู่", "กรุณากรอกข้อมูล",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCategory.Focus();
                return false;
            }
            return true;
        }

        private void SelectComboByValue(ComboBox cbo, int value)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                var ci = cbo.Items[i] as ComboItem;
                if (ci != null && ci.Value == value)
                {
                    cbo.SelectedIndex = i;
                    return;
                }
            }
        }

        private void InsertAtLineStart(string prefix)
        {
            int lineIndex = txtContent.GetLineFromCharIndex(txtContent.SelectionStart);
            int lineStart = txtContent.GetFirstCharIndexFromLine(lineIndex);
            txtContent.SelectionStart  = lineStart;
            txtContent.SelectionLength = 0;
            txtContent.SelectedText    = prefix;
            txtContent.Focus();
        }

        // ── Inner helper ──────────────────────────────────────────────────────

        private class ComboItem
        {
            public string Display { get; private set; }
            public int    Value   { get; private set; }
            public ComboItem(string display, int value) { Display = display; Value = value; }
            public override string ToString() { return Display; }
        }
    }
}
