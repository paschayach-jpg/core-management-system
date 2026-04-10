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
    /// หน้าตั้งค่า Category สำหรับ Knowledge Base (MOD-006)
    /// รองรับ: เพิ่ม / แก้ไข / Soft-Delete รายการใน KMS.m_KBCategory
    /// </summary>
    public partial class KBCategoryForm : Form, IKBCategoryView
    {
        private KBCategoryPresenter _presenter;
        private readonly UserContext _user;

        private List<KBCategory> _categories = new List<KBCategory>();

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
        public void AttachPresenter(KBCategoryPresenter presenter)
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

        private void LstCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (lstCategories.SelectedItems.Count == 0) return;
            var cat = lstCategories.SelectedItems[0].Tag as KBCategory;
            _presenter.OnCategorySelected(cat);
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.Initialize();
            _view_ClearForm();
        }

        private void BtnClear_Click(object sender, EventArgs e) { _view_ClearForm(); }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (!ValidateInput()) return;
            _presenter.Save();
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

        // ── IKBCategoryView ──────────────────────────────────────────────────

        public string CategoryCode
        {
            get { return txtCode.Text.Trim(); }
            set { txtCode.Text = value; }
        }

        public string CategoryName
        {
            get { return txtName.Text.Trim(); }
            set { txtName.Text = value; }
        }

        public string Icon
        {
            get { return txtIcon.Text.Trim(); }
            set { txtIcon.Text = value; }
        }

        public string Description
        {
            get { return txtDescription.Text.Trim(); }
            set { txtDescription.Text = value; }
        }

        public int SortOrder
        {
            get { return (int)numSortOrder.Value; }
            set { numSortOrder.Value = value; }
        }

        public int? ParentCategoryId
        {
            get { return GetSelectedParentId(); }
            set { SelectParentInCombo(value); }
        }

        public int? EditingId { get; set; }

        public string FormTitleText
        {
            set { lblFormTitle.Text = value; }
        }

        public bool DeleteEnabled
        {
            set { btnDelete.Enabled = value; }
        }

        public string DeleteButtonText
        {
            set { btnDelete.Text = value; }
        }

        public void BindCategories(IEnumerable<KBCategory> categories)
        {
            _categories = new List<KBCategory>(categories);
            RefreshList();
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

        // ── Private UI helpers ───────────────────────────────────────────────

        private void _view_ClearForm()
        {
            EditingId           = null;
            lblFormTitle.Text   = "เพิ่ม Category ใหม่";
            txtCode.Text        = string.Empty;
            txtName.Text        = string.Empty;
            txtIcon.Text        = string.Empty;
            txtDescription.Text = string.Empty;
            numSortOrder.Value  = 0;
            btnDelete.Enabled   = false;
            btnDelete.Text      = "🗑️ ปิดใช้งาน";
            lstCategories.SelectedItems.Clear();
            RefreshParentCombo();
            txtCode.Focus();
        }

        private void RefreshList()
        {
            lstCategories.Items.Clear();
            foreach (var c in _categories)
            {
                var item = new ListViewItem(c.CategoryCode ?? "");
                item.SubItems.Add(c.CategoryName ?? "");
                item.SubItems.Add(c.ParentCategoryName ?? "(Root)");
                item.SubItems.Add(c.SortOrder.ToString());
                item.SubItems.Add(c.IsActive ? "ใช้งาน" : "ปิด");
                item.Tag       = c;
                item.ForeColor = c.IsActive
                    ? Color.FromArgb(20, 20, 20)
                    : Color.FromArgb(160, 160, 160);
                lstCategories.Items.Add(item);
            }
            for (int i = 0; i < lstCategories.Items.Count; i++)
            {
                var lvi = lstCategories.Items[i];
                var cat = lvi.Tag as KBCategory;
                if (cat != null && !cat.IsActive)
                    lvi.BackColor = Color.FromArgb(250, 240, 240);
                else if (i % 2 == 0)
                    lvi.BackColor = Color.White;
                else
                    lvi.BackColor = Color.FromArgb(248, 250, 253);
            }
        }

        private void RefreshParentCombo()
        {
            cboParent.Items.Clear();
            cboParent.Items.Add(new ComboItem("(ไม่มี Parent — Root Category)", null));
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

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("กรุณากรอกรหัส Category", "กรุณากรอกข้อมูล",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return false;
            }
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
