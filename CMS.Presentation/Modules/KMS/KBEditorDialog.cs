// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// KBEditor Dialog — แก้ไข/เพิ่มบทความ Knowledge Base (MOD-006)
    /// Redesigned from ArticleEditorDialog based on kms_editor.html
    /// Layout: General Info, Content Editor, Mapping Tabs (Attachments, Related, Workflow, Programs, Docs, Versions)
    /// Supports: Create / Edit / Send Review / Publish / Archive + Version History
    /// </summary>
    public partial class KBEditorDialog : Form, IKBEditorView
    {
        private KBEditorPresenter _presenter;
        private readonly UserContext _user;

        public KBEditorDialog(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
        }

        /// <summary>
        /// Called by the composition root after the presenter is wired.
        /// Must be called before the dialog is shown.
        /// </summary>
        public void AttachPresenter(KBEditorPresenter presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form Events ──────────────────────────────────────────────────────

        private void KBEditorDialog_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิด Dialog",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            InitTypeCombo();
            _presenter.Initialize();
        }

        private void ChkNoExpiry_CheckedChanged(object sender, EventArgs e)
        {
            dtpExpiry.Enabled = !chkNoExpiry.Checked;
        }

        private void BtnSaveDraft_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (!ValidateInput()) return;

            try
            {
                _presenter.Save(txtChangeDesc.Text.Trim());
            }
            catch (Exception ex)
            {
                ShowError("บันทึกไม่สำเร็จ:\n" + ex.Message);
            }
        }

        private void BtnSendReview_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;

            string action = btnSendReview.Text.Contains("Review") ? "ส่งขอ Review" : "เผยแพร่";
            var confirm = MessageBox.Show(
                string.Format("ต้องการ{0}บทความ \"{1}\" ใช่หรือไม่?", action, txtTitle.Text.Trim()),
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _presenter.Publish();
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาด:\n" + ex.Message);
            }
        }

        private void BtnArchive_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;

            var confirm = MessageBox.Show(
                string.Format(
                    "ต้องการ Archive บทความ \"{0}\" ใช่หรือไม่?\n\n" +
                    "(บทความที่ Archive แล้วไม่สามารถนำกลับมาแก้ไขได้)",
                    txtTitle.Text.Trim()),
                "ยืนยัน Archive", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _presenter.Archive();
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาด:\n" + ex.Message);
            }
        }

        private void BtnPreview_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                string.Format("Preview: {0}\n\n(ฟีเจอร์ Preview จะเปิดในหน้าต่างใหม่ — Phase 2)",
                    txtTitle.Text.Trim()),
                "Preview บทความ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── IKBEditorView Implementation ─────────────────────────────────────

        public string ArticleCode
        {
            get { return txtCode.Text.Trim(); }
            set { txtCode.Text = value; }
        }

        public string Title
        {
            get { return txtTitle.Text.Trim(); }
            set { txtTitle.Text = value; }
        }

        public string Summary
        {
            get { return txtSummary.Text.Trim(); }
            set { txtSummary.Text = value; }
        }

        public string ContentHTML
        {
            get { return txtContent.Text.Trim(); }
            set { txtContent.Text = value; }
        }

        public string Tags
        {
            get { return txtTags.Text.Trim(); }
            set { txtTags.Text = value; }
        }

        public bool IsOfficialPolicy
        {
            get { return chkOfficialPolicy.Checked; }
            set { chkOfficialPolicy.Checked = value; }
        }

        public bool NoExpiry
        {
            get { return chkNoExpiry.Checked; }
            set
            {
                chkNoExpiry.Checked = value;
                dtpExpiry.Enabled   = !value;
            }
        }

        public DateTime? ExpiryDate
        {
            get { return chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value; }
            set { if (value.HasValue) dtpExpiry.Value = value.Value; }
        }

        public int SelectedCategoryId
        {
            get
            {
                var ci = cboCategory.SelectedItem as ComboItem;
                return ci != null ? ci.Value : 0;
            }
            set { SelectComboByValue(cboCategory, value); }
        }

        public byte SelectedArticleType
        {
            get
            {
                var ci = cboType.SelectedItem as ComboItem;
                return ci != null ? (byte)ci.Value : (byte)1;
            }
            set { SelectComboByValue(cboType, value); }
        }

        public byte SelectedStatus
        {
            get { return _currentStatus; }
            set
            {
                _currentStatus = value;
                SetStatusDisplay(value);
            }
        }
        private byte _currentStatus = 1;

        public string HeaderText
        {
            set { lblHeader.Text = value; }
        }

        public string ViewCountText
        {
            set { lblViewCount.Text = value; }
        }

        public string LikeCountText
        {
            set { lblLikeCount.Text = value; }
        }

        public string VersionTabText
        {
            set { tabVersions.Text = value; }
        }

        public bool PublishButtonVisible
        {
            set { btnSendReview.Visible = value; }
        }

        public bool ArchiveButtonVisible
        {
            set { btnArchive.Visible = value; }
        }

        public bool SaveEnabled
        {
            set { btnSaveDraft.Enabled = value; }
        }

        public string PublishButtonText
        {
            set { btnSendReview.Text = value; }
        }

        public Color PublishButtonColor
        {
            set { btnSendReview.BackColor = value; }
        }

        public string VersionText
        {
            set { lblVerInfo.Text = value; }
        }

        public string AuthorText
        {
            set { lblAuthorInfo.Text = value; }
        }

        public string DateInfoText
        {
            set { lblDateInfo.Text = value; }
        }

        public void BindCategories(IEnumerable<KBCategory> categories)
        {
            cboCategory.Items.Clear();
            cboCategory.Items.Add(new ComboItem("— เลือกหมวดหมู่ —", 0));

            foreach (var c in categories.OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName))
            {
                cboCategory.Items.Add(new ComboItem(
                    c.ParentCategoryID.HasValue
                        ? string.Format("  └ {0}", c.CategoryName)
                        : c.CategoryName,
                    c.CategoryID));
            }

            cboCategory.SelectedIndex = 0;
        }

        public void BindVersionHistory(IEnumerable<KBVersion> versions)
        {
            lstVersions.Items.Clear();

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
        }

        public void CloseOk()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public void CloseCancel()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Private UI Helpers ───────────────────────────────────────────────

        private void InitTypeCombo()
        {
            cboType.Items.Clear();
            cboType.Items.Add(new ComboItem("SOP (Standard Operating Procedure)", 1));
            cboType.Items.Add(new ComboItem("FAQ (คำถามที่พบบ่อย)",              2));
            cboType.Items.Add(new ComboItem("Guideline (แนวปฏิบัติ)",             3));
            cboType.Items.Add(new ComboItem("Template (แม่แบบเอกสาร)",            4));
            cboType.Items.Add(new ComboItem("Policy (นโยบาย)",                    5));
            cboType.SelectedIndex = 0;
        }

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

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowInfo("กรุณากรอกชื่อบทความ");
                txtTitle.Focus();
                return false;
            }

            var catItem = cboCategory.SelectedItem as ComboItem;
            if (catItem == null || catItem.Value == 0)
            {
                ShowInfo("กรุณาเลือกหมวดหมู่");
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

        // ── RTE Toolbar Handlers ─────────────────────────────────────────────

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

        private void TsRteH1_Click(object sender, EventArgs e)     { InsertAtLineStart("# "); }
        private void TsRteH2_Click(object sender, EventArgs e)     { InsertAtLineStart("## "); }
        private void TsRteH3_Click(object sender, EventArgs e)     { InsertAtLineStart("### "); }
        private void TsRteBullet_Click(object sender, EventArgs e) { InsertAtLineStart("- "); }
        private void TsRteNumList_Click(object sender, EventArgs e){ InsertAtLineStart("1. "); }

        private void TsRteLink_Click(object sender, EventArgs e)
        {
            string sel  = txtContent.SelectedText;
            string text = string.IsNullOrEmpty(sel) ? "ข้อความลิงก์" : sel;
            int    pos  = txtContent.SelectionStart;
            txtContent.SelectedText   = string.Format("[{0}](URL)", text);
            txtContent.SelectionStart = pos + text.Length + 3;
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

        private void InsertAtLineStart(string prefix)
        {
            int lineIndex = txtContent.GetLineFromCharIndex(txtContent.SelectionStart);
            int lineStart = txtContent.GetFirstCharIndexFromLine(lineIndex);
            txtContent.SelectionStart  = lineStart;
            txtContent.SelectionLength = 0;
            txtContent.SelectedText    = prefix;
            txtContent.Focus();
        }

        // ── Helper Classes ───────────────────────────────────────────────────

        private class ComboItem
        {
            public string Display { get; private set; }
            public int    Value   { get; private set; }

            public ComboItem(string display, int value)
            {
                Display = display;
                Value   = value;
            }

            public override string ToString()
            {
                return Display;
            }
        }
    }
}
