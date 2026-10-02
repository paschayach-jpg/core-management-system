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

        /// <summary>
        /// AfterSelect ไม่ fire ถ้าคลิกโหนดที่ selected อยู่แล้ว (WinForms TreeView behavior)
        /// ทำให้คลิกเมนู/หมวดหมู่ซ้ำที่ node เดิมแล้วไม่มีอะไรเกิดขึ้น — เรียก logic เดิมซ้ำเองในกรณีนี้
        /// </summary>
        private void TreeCategories_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null && treeCategories.SelectedNode == e.Node)
                TreeCategories_AfterSelect(sender, new TreeViewEventArgs(e.Node));
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
            
            // Clear the form for new entry
            _view_ClearForm();
            
            // Ensure Root is selected (index 0 = no parent)
            cboParent.SelectedIndex = 0;
            
            lblEditCardHeader.Text = "  ✏️ เพิ่มหมวดหมู่หลัก";
            txtName.Focus();
        }

        private void BtnNewSub_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            
            // Store the selected category before clearing
            var selected = treeCategories.SelectedNode;
            KBCategory selectedCat = null;
            if (selected != null)
            {
                selectedCat = selected.Tag as KBCategory;
            }
            
            // Clear the form for new entry
            _view_ClearForm();
            
            // Pre-select the previously highlighted tree node as parent
            if (selectedCat != null)
            {
                SelectParentInCombo(selectedCat.CategoryID);
            }
            else if (cboParent.Items.Count > 1)
            {
                // If no node was selected, select the first available category as parent
                cboParent.SelectedIndex = 1;
            }
            
            lblEditCardHeader.Text = "  ✏️ เพิ่มหมวดหมู่ย่อย";
            txtName.Focus();
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
        }

        private void TxtDescription_TextChanged(object sender, EventArgs e) { MarkDirty(); }

        private void TxtCategoryCode_TextChanged(object sender, EventArgs e) { MarkDirty(); }

        private void NumSortOrder_ValueChanged(object sender, EventArgs e) { MarkDirty(); }

        private void CboParent_SelectedIndexChanged(object sender, EventArgs e) { MarkDirty(); }

        private void CboImportTypeDigit_SelectedIndexChanged(object sender, EventArgs e) { MarkDirty(); }

        private void CboIcon_SelectedIndexChanged(object sender, EventArgs e)
        {
            MarkDirty();
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
        /// CategoryCode is now displayed in txtCategoryCode.
        /// </summary>
        public string CategoryCode
        {
            get { return txtCategoryCode.Text.Trim(); }
            set { txtCategoryCode.Text = value ?? string.Empty; }
        }

        public string CategoryName
        {
            get { return txtName.Text.Trim(); }
            set { txtName.Text = value; }
        }

        /// <summary>
        /// Icon stored as a byte value (1–9) matching <see cref="CMS.Domain.Enums.KBCategoryIcon"/>.
        /// The combo index is 0-based; the enum value is 1-based.
        /// </summary>
        public string Icon
        {
            get
            {
                // combo index 0 → enum value 1, etc.
                byte val = (byte)(cboIcon.SelectedIndex + 1);
                return val.ToString();
            }
            set
            {
                byte parsed;
                if (byte.TryParse(value, out parsed) && parsed >= 1 && parsed <= cboIcon.Items.Count)
                    cboIcon.SelectedIndex = parsed - 1;
                else
                    cboIcon.SelectedIndex = 0;
            }
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

        /// <summary>
        /// รหัสประเภทเอกสารที่ไม่ใช่กฎหมาย (1-9 = ตัวที่ 23 ของ Code รวมตอน import) — null = ไม่ใช่หมวดประเภทเอกสาร
        /// </summary>
        public string ImportTypeDigit
        {
            get
            {
                var item = cboImportTypeDigit.SelectedItem as DigitItem;
                return item != null ? item.Digit : null;
            }
            set { SelectImportTypeDigit(value); }
        }

        private int? _editingId;

        /// <summary>
        /// หมวดที่กำลังแก้ไข (null = เพิ่มใหม่) — เปลี่ยนแล้วรีเฟรชรายการ Parent เพื่อตัดตัวเองและหมวดย่อยของตัวเองออก
        /// (ตั้งเป็น parent แล้วต้นไม้หมวดจะวนลูป)
        /// </summary>
        public int? EditingId
        {
            get { return _editingId; }
            set
            {
                if (_editingId == value) return;
                _editingId = value;
                RefreshParentCombo();
                RefreshImportTypeCombo();
            }
        }

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
            RefreshImportTypeCombo();
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

        public void BindCategoryDetails(KBCategory category)
        {
            if (category == null) { ClearDetailPanel(); return; }

            lblInfoCreatedByVal.Text   = string.IsNullOrEmpty(category.CreatedBy) ? "—" : category.CreatedBy;
            lblInfoCreatedDateVal.Text = category.CreatedDate == default(DateTime)
                ? "—"
                : category.CreatedDate.ToString("dd/MM/yyyy HH:mm");
            lblInfoUpdatedVal.Text     = "—";   // KBCategory has no UpdatedDate field

            // Build full path: Root > ... > Category (กี่ชั้นก็ได้)
            string path = KBCategoryTree.GetPath(_categories, category.CategoryID, " > ");
            lblInfoPathVal.Text = string.IsNullOrEmpty(path) ? category.CategoryName : path;
        }

        public void BindCategoryStats(int total, int published, int draft)
        {
            lblStatTotalNum.Text     = total.ToString();
            lblStatPublishedNum.Text = published.ToString();
            lblStatDraftNum.Text     = draft.ToString();
        }

        public void BindArticleUsage(IEnumerable<KBArticle> articles)
        {
            lstArticleUsage.Items.Clear();
            foreach (var a in articles)
            {
                var item = new ListViewItem(a.ArticleCode ?? string.Empty);
                item.SubItems.Add(a.Title ?? string.Empty);
                item.SubItems.Add(a.StatusName);
                item.SubItems.Add(a.CreatedBy ?? string.Empty);
                item.Tag = a;
                lstArticleUsage.Items.Add(item);
            }
        }

        // ── Private UI helpers ────────────────────────────────────────────────

        private void _view_ClearForm()
        {
            EditingId              = null;
            lblEditCardHeader.Text = "  ✏️ เพิ่ม Category ใหม่";
            txtName.Text           = string.Empty;
            txtCategoryCode.Text   = string.Empty;
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
            
            // Refresh parent combo to show all available categories
            RefreshParentCombo();
            
            // Default to Root (no parent) for new categories
            // This will be overridden by BtnNewSub_Click if creating a subcategory
            if (cboParent.Items.Count > 0)
                cboParent.SelectedIndex = 0;

            SelectImportTypeDigit(null);
            
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

            // หมวดกี่ชั้นก็ได้ (เช่น กฎหมาย › พระราชบัญญัติประกันชีวิต › พระราชบัญญัติประกันชีวิต2535)
            var childrenOf = _categories
                .Where(c => c.ParentCategoryID.HasValue)
                .GroupBy(c => c.ParentCategoryID.Value)
                .ToDictionary(g => g.Key, g => g.OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName).ToList());

            var roots = _categories
                .Where(c => !c.ParentCategoryID.HasValue)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);

            foreach (var root in roots)
            {
                var rootNode = BuildBranch(root, search, false, childrenOf, new HashSet<int>());
                if (rootNode == null) continue;

                treeCategories.Nodes.Add(rootNode);
                rootNode.Expand();
            }

            if (search.Length > 0)
                treeCategories.ExpandAll();   // ค้นหา: เปิดทุกชั้นให้เห็นหมวดที่ตรง

            treeCategories.EndUpdate();
        }

        /// <summary>
        /// สร้างโหนดของหมวดพร้อมหมวดย่อยทุกชั้น — หมวดที่ชื่อตรงคำค้น (หรือ ancestor ตรง) แสดงทั้งกิ่ง, หมวดที่ไม่ตรงแสดงเฉพาะเมื่อมี
        /// หมวดย่อยตรง; คืน null เมื่อไม่ต้องแสดง. <paramref name="path"/> กันวนลูปเมื่อ ParentCategoryID ชี้กลับ
        /// </summary>
        private TreeNode BuildBranch(
            KBCategory cat, string search, bool ancestorMatches,
            Dictionary<int, List<KBCategory>> childrenOf, HashSet<int> path)
        {
            if (!path.Add(cat.CategoryID)) return null;

            bool showAll = ancestorMatches || search.Length == 0
                || (cat.CategoryName ?? string.Empty).ToLower().Contains(search);

            var childNodes = new List<TreeNode>();
            List<KBCategory> children;
            if (childrenOf.TryGetValue(cat.CategoryID, out children))
            {
                foreach (var child in children)
                {
                    var childNode = BuildBranch(child, search, showAll, childrenOf, path);
                    if (childNode != null) childNodes.Add(childNode);
                }
            }
            path.Remove(cat.CategoryID);

            if (!showAll && childNodes.Count == 0) return null;

            var node = BuildTreeNode(cat);
            foreach (var childNode in childNodes) node.Nodes.Add(childNode);
            return node;
        }

        private TreeNode BuildTreeNode(KBCategory cat)
        {
            string symbol = cat.IconSymbol;
            string text   = symbol + "  " + cat.CategoryName;
            if (!cat.IsActive) text += "  (ปิด)";
            var node = new TreeNode(text);
            node.Tag       = cat;
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
            // Store the currently selected parent ID before refreshing
            int? currentSelection = GetSelectedParentId();

            cboParent.Items.Clear();
            cboParent.Items.Add(new ComboItem("— ไม่มี (หมวดหมู่หลัก / Root) —", null));

            // Mirror treeCategories: tree order at any depth, indented by level — same sort order.
            // ตอนแก้ไขหมวด ตัดตัวเองและหมวดย่อยของตัวเองออก (ตั้งเป็น parent แล้วต้นไม้จะวนลูป)
            ISet<int> excluded = new HashSet<int>();
            if (EditingId.HasValue)
            {
                excluded = KBCategoryTree.GetDescendantIds(_categories, EditingId.Value);
                excluded.Add(EditingId.Value);
            }

            foreach (var item in KBCategoryTree.Flatten(_categories, true))
            {
                if (excluded.Contains(item.Category.CategoryID)) continue;

                string indent = item.Depth == 0 ? string.Empty : new string(' ', 4 * item.Depth) + "└─ ";
                cboParent.Items.Add(new ComboItem(
                    string.Format("{0}{1} {2}", indent, item.Category.IconSymbol, item.Category.CategoryName),
                    item.Category.CategoryID));
            }

            // Restore previous selection if it still exists
            if (currentSelection.HasValue)
                SelectParentInCombo(currentSelection);
            else
                cboParent.SelectedIndex = 0;
        }

        /// <summary>
        /// รายการรหัสประเภทเอกสาร: "ไม่ใช่ประเภทเอกสาร" + 1-9 — รหัสที่หมวดอื่นใช้อยู่แล้วมีชื่อหมวดนั้นกำกับ (1 รหัสใช้ได้ 1 หมวด
        /// เลือกซ้ำแล้วบันทึกไม่ผ่าน) ส่วนรหัสของหมวดที่กำลังแก้ไขอยู่ไม่กำกับ; เก็บค่าที่เลือกอยู่ไว้ตอนสร้างรายการใหม่
        /// </summary>
        private void RefreshImportTypeCombo()
        {
            string current = ImportTypeDigit;

            cboImportTypeDigit.Items.Clear();
            cboImportTypeDigit.Items.Add(new DigitItem(null, "— ไม่ใช่ประเภทเอกสาร —"));
            for (char ch = KBImportLayoutTypeCatalog.MinTypeDigit; ch <= KBImportLayoutTypeCatalog.MaxTypeDigit; ch++)
            {
                string digit = ch.ToString();
                var owner = _categories.FirstOrDefault(c => c.ImportTypeDigit == digit
                                                            && (!_editingId.HasValue || c.CategoryID != _editingId.Value));
                cboImportTypeDigit.Items.Add(new DigitItem(digit, owner == null
                    ? digit
                    : string.Format("{0} — ใช้แล้ว: {1}", digit, owner.CategoryName)));
            }

            SelectImportTypeDigit(current);
        }

        private void SelectImportTypeDigit(string digit)
        {
            if (cboImportTypeDigit.Items.Count == 0) return;

            string wanted = string.IsNullOrWhiteSpace(digit) ? null : digit.Trim();
            for (int i = 0; i < cboImportTypeDigit.Items.Count; i++)
            {
                var item = cboImportTypeDigit.Items[i] as DigitItem;
                if (item != null && item.Digit == wanted)
                {
                    cboImportTypeDigit.SelectedIndex = i;
                    return;
                }
            }
            cboImportTypeDigit.SelectedIndex = 0;
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

        private class DigitItem
        {
            public string Digit   { get; private set; }
            public string Display { get; private set; }
            public DigitItem(string digit, string display) { Digit = digit; Display = display; }
            public override string ToString() { return Display; }
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
