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
    /// รองรับ: Create/Edit/Publish/Archive + Version History
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
                // New article
                Text = "เพิ่มบทความใหม่ — Knowledge Base";
                lblHeader.Text = "  📄 เพิ่มบทความใหม่";
                txtCode.Text   = string.Format("KB-{0}-NEW", DateTime.Now.Year);
                cboStatus.SelectedIndex = 0;  // Draft
                cboType.SelectedIndex   = 0;
                chkNoExpiry.Checked     = true;
                dtpExpiry.Enabled       = false;
                btnArchive.Visible      = false;
                btnPublish.Text         = "📤 ส่ง Review";
            }
            else
            {
                // Edit existing
                Text = "แก้ไขบทความ — " + (_editingArticle.Title ?? "");
                lblHeader.Text = "  📄 แก้ไขบทความ: " + (_editingArticle.Title ?? "");
                PopulateFields(_editingArticle);
                LoadVersionHistory(_editingArticle.ArticleID);
                UpdateStatusButtons(_editingArticle.Status);
            }
        }

        // ── Dropdowns ─────────────────────────────────────────────────────────

        private void LoadDropdowns()
        {
            // Categories
            try
            {
                _categories = _categoryService.GetAllActive().ToList();
            }
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

            // Article Types
            cboType.Items.Clear();
            cboType.Items.Add(new ComboItem("SOP (Standard Operating Procedure)", 1));
            cboType.Items.Add(new ComboItem("FAQ (คำถามที่พบบ่อย)", 2));
            cboType.Items.Add(new ComboItem("Guideline (แนวปฏิบัติ)", 3));
            cboType.Items.Add(new ComboItem("Template (แม่แบบเอกสาร)", 4));
            cboType.Items.Add(new ComboItem("Policy (นโยบาย)", 5));
            cboType.SelectedIndex = 0;

            // Status
            cboStatus.Items.Clear();
            cboStatus.Items.Add(new ComboItem("📝 Draft", 1));
            cboStatus.Items.Add(new ComboItem("🔍 รอ Review", 2));
            cboStatus.Items.Add(new ComboItem("✅ Published", 3));
            cboStatus.Items.Add(new ComboItem("📦 Archived", 4));
            cboStatus.SelectedIndex = 0;
        }

        // ── Populate fields from existing article ─────────────────────────────

        private void PopulateFields(KBArticle a)
        {
            txtCode.Text   = a.ArticleCode ?? string.Empty;
            txtTitle.Text  = a.Title       ?? string.Empty;
            txtSummary.Text = a.Summary    ?? string.Empty;
            txtContent.Text = a.ContentHTML ?? string.Empty;
            txtTags.Text    = a.Tags       ?? string.Empty;
            chkOfficialPolicy.Checked = a.IsOfficialPolicy;

            // Category
            SelectComboByValue(cboCategory, a.CategoryID);
            // Type
            SelectComboByValue(cboType, (int)a.ArticleType);
            // Status
            SelectComboByValue(cboStatus, (int)a.Status);

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

            lblViewCount.Text = string.Format("👁 ดู {0} ครั้ง", a.ViewCount);
            lblLikeCount.Text = string.Format("👍 {0} Like", a.LikeCount);
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

        // ── Status button visibility ──────────────────────────────────────────

        private void UpdateStatusButtons(byte status)
        {
            bool isArchived = (status == 4);
            btnSave.Enabled    = !isArchived;
            btnPublish.Enabled = !isArchived;

            if (status == 1 || status == 2)
            {
                btnPublish.Text      = status == 1 ? "📤 ส่ง Review" : "✅ เผยแพร่";
                btnPublish.BackColor = status == 1
                    ? Color.FromArgb(0, 120, 215)
                    : Color.FromArgb(34, 139, 34);
                btnArchive.Visible = false;
            }
            else if (status == 3)
            {
                btnPublish.Visible   = false;
                btnArchive.Visible   = true;
            }
            else if (status == 4)
            {
                btnPublish.Visible  = false;
                btnArchive.Visible  = false;
                btnSave.Text        = "🔒 (Archived)";
                btnSave.Enabled     = false;
            }
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void ChkNoExpiry_CheckedChanged(object sender, EventArgs e)
        {
            dtpExpiry.Enabled = !chkNoExpiry.Checked;
        }

        private void BtnSave_Click(object sender, EventArgs e)
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

        private void BtnPublish_Click(object sender, EventArgs e)
        {
            if (_editingArticle == null)
            {
                MessageBox.Show("กรุณาบันทึกบทความก่อนเผยแพร่", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            byte currentStatus = _editingArticle.Status;
            byte targetStatus  = currentStatus == 1 ? (byte)2 : (byte)3;
            string actionText  = targetStatus == 2 ? "ส่ง Review" : "เผยแพร่";

            var confirm = MessageBox.Show(
                string.Format("ต้องการ{0}บทความ \"{1}\" ใช่หรือไม่?", actionText, _editingArticle.Title),
                "ยืนยัน",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

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

        private void BtnArchive_Click(object sender, EventArgs e)
        {
            if (_editingArticle == null) return;

            var confirm = MessageBox.Show(
                string.Format("ต้องการ Archive บทความ \"{0}\" ใช่หรือไม่?\n\n" +
                              "(บทความที่ Archive แล้วไม่สามารถนำกลับมาได้)", _editingArticle.Title),
                "ยืนยัน Archive",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

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

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

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
                Status           = 1,   // always saved as Draft initially
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

        // ── Inner helper ─────────────────────────────────────────────────────

        private class ComboItem
        {
            public string Display { get; private set; }
            public int    Value   { get; private set; }
            public ComboItem(string display, int value) { Display = display; Value = value; }
            public override string ToString() { return Display; }
        }
    }
}
