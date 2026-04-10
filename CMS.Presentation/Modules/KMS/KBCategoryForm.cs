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
    /// หน้าตั้งค่า Category สำหรับ Knowledge Base (MOD-006)
    /// รองรับ: เพิ่ม / แก้ไข / Soft-Delete รายการใน KMS.m_KBCategory
    /// ออกแบบตาม kms_category.html (lines 603–788)
    /// เปิดแบบฝังใน KBMainForm เหมือน ArticleViewerForm (TopLevel=false, Dock=Fill)
    /// </summary>
    public partial class KBCategoryForm : Form, IKBCategoryController
    {
        private KBCategoryController _presenter;
        private readonly UserContext _user;

        private List<KBCategory> _categories = new List<KBCategory>();

        /// <summary>
        /// Fired when the user clicks "← กลับ"; parent form handles navigation.
        /// </summary>
        public event EventHandler BackRequested;

        /// <summary>Backing store for CategoryCode (not shown in UI but required by interface).</summary>
        private string _categoryCodeValue = string.Empty;

        public KBCategoryForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
        }

        /// <summary>
        /// Called by the composition root after the presenter is wired.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachPresenter(KBCategoryController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form events ──────────────────────────────────────────────────────

        private void KBCategoryForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            _presenter.Initialize();
        }

        private void TreeCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_presenter == null) return;
            var cat = e.Node != null ? e.Node.Tag as KBCategory : null;
            if (cat == null) return;
            _presenter.OnCategorySelected(cat);
            lblEditCardHeader.Text = "  \u270F\uFE0F แก้ไขหมวดหมู่: " + cat.CategoryName;
            ClearDirty();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshTree();
        }

        private void TxtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.ForeColor == System.Drawing.Color.FromArgb(160, 160, 160))
            {
                txtSearch.Text = string.Empty;
                txtSearch.ForeColor = System.Drawing.Color.FromArgb(20, 20, 20);
            }
        }

        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.ForeColor = System.Drawing.Color.FromArgb(160, 160, 160);
                txtSearch.Text = "\uD83D\uDD0D ค้นหาหมวดหมู่...";
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
        }

        private void BtnNewRoot_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.Initialize();
            _view_ClearForm();
            cboParent.SelectedIndex = 0;   // ensure Root is selected
        }

        private void BtnNewSub_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.Initialize();
            _view_ClearForm();
            // Pre-select the currently highlighted tree node as parent
            var selected = treeCategories.SelectedNode;
            if (selected != null)
            {
                var cat = selected.Tag as KBCategory;
                if (cat != null)
                    SelectParentInCombo(cat.CategoryID);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e) { treeCategories.SelectedNode = null; _view_ClearForm(); }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (!ValidateInput()) return;
            _presenter.Save();
            ClearDirty();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (!EditingId.HasValue) return;
            var cat  = _categories.FirstOrDefault(c => c.CategoryID == EditingId.Value);
            string name = cat != null ? cat.CategoryName : EditingId.Value.ToString();
            var confirm = MessageBox.Show(
                string.Format("ต้องการปิดใช้งาน Category \"{0}\" ใช่หรือไม่?", name),
                "ยืนยันการปิดใช้งาน", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
                _presenter.Deactivate();
        }

        // ── Dirty tracking ────────────────────────────────────────────────────

        private void TxtName_TextChanged(object sender, EventArgs e)
        {
            MarkDirty();
            UpdateIconFullPreview();
        }

        private void TxtDescription_TextChanged(object sender, EventArgs e) { MarkDirty(); }

        private void NumSortOrder_ValueChanged(object sender, EventArgs e) { MarkDirty(); }

        private void CboParent_SelectedIndexChanged(object sender, EventArgs e) { MarkDirty(); }

        private void CboIcon_SelectedIndexChanged(object sender, EventArgs e)
        {
            MarkDirty();
            UpdateIconPreview();
        }

        private void MarkDirty()
        {
            lblDirtyHint.Visible = true;
        }

        private void ClearDirty()
        {
            lblDirtyHint.Visible = false;
        }

        // ── IKBCategoryController ─────────────────────────────────────────────

        /// <summary>
        /// CategoryCode is not displayed in the redesigned form but is required
        /// by IKBCategoryController. Stored internally.
        /// </summary>
        public string CategoryCode
        {
            get { return _categoryCodeValue; }
            set { _categoryCodeValue = value ?? string.Empty; }
        }

        public string CategoryName
        {
            get { return txtName.Text.Trim(); }
            set
            {
                txtName.Text = value;
                UpdateIconFullPreview();
            }
        }

        /// <summary>
        /// Icon stored as the emoji prefix of the selected combo item (e.g. "❓").
        /// On get: returns the first "word" (emoji) of the selected item.
        /// On set: finds the matching item by emoji prefix.
        /// </summary>
        public string Icon
        {
            get { return GetIconEmoji(); }
            set { SelectIconByEmoji(value); }
        }

        public string Description
        {
            get { return txtDescription.Text.Trim(); }
            set { txtDescription.Text = value; }
        }

        public int SortOrder
        {
            get { return (int)numSortOrder.Value; }
            set
            {
                if (value >= numSortOrder.Minimum && value <= numSortOrder.Maximum)
                    numSortOrder.Value = value;
            }
        }

        public int? ParentCategoryId
        {
            get { return GetSelectedParentId(); }
            set { SelectParentInCombo(value); }
        }

        public int? EditingId { get; set; }

        public string FormTitleText
        {
            set { lblEditCardHeader.Text = "  ✏️ " + value; }
        }

        public bool DeleteEnabled
        {
            set
            {
                btnDelete.Enabled = value;
                // Mirror active/inactive state in the read-only status combo
                cboStatus.SelectedIndex = value ? 0 : 1;
                // Update info-row status badge
                lblInfoStatusVal.Text = value ? "● Active" : "○ Inactive";
                lblInfoStatusVal.ForeColor = value
                    ? Color.FromArgb(30, 132, 73)
                    : Color.FromArgb(150, 150, 150);
            }
        }

        public string DeleteButtonText
        {
            set { btnDelete.Text = value; }
        }

        public void BindCategories(IEnumerable<KBCategory> categories)
        {
            _categories = new List<KBCategory>(categories);
            RefreshTree();
            RefreshParentCombo();
        }

        public void ClearForm()
        {
            _view_ClearForm();
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowSuccess(string message)
        {
            MessageBox.Show(message, "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Private UI helpers ────────────────────────────────────────────────

        private void _view_ClearForm()
        {
            EditingId              = null;
            _categoryCodeValue     = string.Empty;
            lblEditCardHeader.Text = "  ✏️ เพิ่ม Category ใหม่";
            txtName.Text           = string.Empty;
            if (cboIcon.Items.Count > 0)
                cboIcon.SelectedIndex = 0;
            txtDescription.Text    = string.Empty;
            numSortOrder.Value     = 1;
            cboStatus.SelectedIndex = 0;
            btnDelete.Enabled      = false;
            btnDelete.Text         = "🗑️ ลบหมวดหมู่";
            treeCategories.SelectedNode = null;
            ClearDirty();
            ClearDetailPanel();
            RefreshParentCombo();
            txtName.Focus();
        }

        private void ClearDetailPanel()
        {
            lblStatTotalNum.Text     = "—";
            lblStatPublishedNum.Text = "—";
            lblStatDraftNum.Text     = "—";
            lblInfoCreatedByVal.Text  = "—";
            lblInfoCreatedDateVal.Text = "—";
            lblInfoUpdatedVal.Text    = "—";
            lblInfoStatusVal.Text     = "—";
            lblInfoStatusVal.ForeColor = Color.FromArgb(120, 120, 120);
            lblInfoPathVal.Text       = "—";
            lstArticleUsage.Items.Clear();
        }

        private void RefreshTree()
        {
            string search = GetSearchText().ToLower();

            treeCategories.BeginUpdate();
            treeCategories.Nodes.Clear();

            var roots = _categories
                .Where(c => !c.ParentCategoryID.HasValue)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);

            foreach (var root in roots)
            {
                var children = _categories
                    .Where(c => c.ParentCategoryID.HasValue && c.ParentCategoryID.Value == root.CategoryID)
                    .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName)
                    .ToList();

                bool rootMatches = string.IsNullOrEmpty(search) || root.CategoryName.ToLower().Contains(search);
                bool anyChildMatches = children.Any(c => string.IsNullOrEmpty(search) || c.CategoryName.ToLower().Contains(search));

                if (!rootMatches && !anyChildMatches) continue;

                var rootNode = BuildTreeNode(root);
                foreach (var child in children)
                {
                    bool childMatches = string.IsNullOrEmpty(search) || child.CategoryName.ToLower().Contains(search);
                    if (!childMatches && !rootMatches) continue;
                    rootNode.Nodes.Add(BuildTreeNode(child));
                }
                treeCategories.Nodes.Add(rootNode);
                rootNode.Expand();
            }

            treeCategories.EndUpdate();
        }

        private TreeNode BuildTreeNode(KBCategory cat)
        {
            string icon = string.IsNullOrEmpty(cat.Icon) ? "\uD83D\uDCC2" : cat.Icon;
            string text = icon + "  " + cat.CategoryName;
            if (!cat.IsActive) text += "  (ปิด)";
            var node = new TreeNode(text);
            node.Tag      = cat;
            node.ForeColor = cat.IsActive
                ? Color.FromArgb(20, 20, 20)
                : Color.FromArgb(160, 160, 160);
            return node;
        }

        private string GetSearchText()
        {
            if (txtSearch.ForeColor == Color.FromArgb(160, 160, 160)) return string.Empty;
            return txtSearch.Text.Trim();
        }

        private void RefreshParentCombo()
        {
            cboParent.Items.Clear();
            cboParent.Items.Add(new ComboItem("— ไม่มี (หมวดหมู่หลัก / Root) —", null));
            foreach (var c in _categories.Where(x => x.IsActive))
            {
                if (EditingId.HasValue && c.CategoryID == EditingId.Value) continue;
                cboParent.Items.Add(new ComboItem(
                    string.Format("[{0}] {1}", c.CategoryCode, c.CategoryName),
                    c.CategoryID));
            }
            cboParent.SelectedIndex = 0;
        }

        private void SelectParentInCombo(int? parentId)
        {
            if (!parentId.HasValue) { cboParent.SelectedIndex = 0; return; }
            for (int i = 0; i < cboParent.Items.Count; i++)
            {
                var ci = cboParent.Items[i] as ComboItem;
                if (ci != null && ci.Value.HasValue && ci.Value.Value == parentId.Value)
                {
                    cboParent.SelectedIndex = i;
                    return;
                }
            }
            cboParent.SelectedIndex = 0;
        }

        private int? GetSelectedParentId()
        {
            var ci = cboParent.SelectedItem as ComboItem;
            return ci != null ? ci.Value : null;
        }

        /// <summary>Returns the emoji portion of the selected icon combo item.</summary>
        private string GetIconEmoji()
        {
            if (cboIcon.SelectedItem == null) return string.Empty;
            string item = cboIcon.SelectedItem.ToString();
            // Items are formatted as "❓ คำถาม" — split on first space
            int spaceIdx = item.IndexOf(' ');
            return spaceIdx > 0 ? item.Substring(0, spaceIdx) : item;
        }

        /// <summary>Selects the combo item whose emoji prefix matches <paramref name="emoji"/>.</summary>
        private void SelectIconByEmoji(string emoji)
        {
            if (string.IsNullOrEmpty(emoji)) { cboIcon.SelectedIndex = 0; return; }
            for (int i = 0; i < cboIcon.Items.Count; i++)
            {
                string item = cboIcon.Items[i].ToString();
                if (item.StartsWith(emoji))
                {
                    cboIcon.SelectedIndex = i;
                    return;
                }
            }
            cboIcon.SelectedIndex = 0;
        }

        private void UpdateIconPreview()
        {
            string emoji = GetIconEmoji();
            lblIconPreview.Text = string.IsNullOrEmpty(emoji) ? "❓" : emoji;
            UpdateIconFullPreview();
        }

        private void UpdateIconFullPreview()
        {
            string emoji = GetIconEmoji();
            string name  = txtName.Text.Trim();
            lblIconFullPreview.Text = string.Format("{0} {1}",
                string.IsNullOrEmpty(emoji) ? "❓" : emoji,
                string.IsNullOrEmpty(name)  ? "ชื่อ Category" : name);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อ Category", "กรุณากรอกข้อมูล",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }
            return true;
        }

        private class ComboItem
        {
            public string Display { get; private set; }
            public int?   Value   { get; private set; }
            public ComboItem(string display, int? value) { Display = display; Value = value; }
            public override string ToString() { return Display; }
        }
    }
}
