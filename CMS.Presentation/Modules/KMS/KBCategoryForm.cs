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
    /// หน้าตั้งค่า Category สำหรับ Knowledge Base (MOD-006)
    /// รองรับ: เพิ่ม / แก้ไข / Soft-Delete รายการใน KMS.m_KBCategory
    /// </summary>
    public partial class KBCategoryForm : Form
    {
        private readonly IKBCategoryService _service;
        private readonly UserContext        _user;

        private List<KBCategory> _categories = new List<KBCategory>();
        private int?             _editingId   = null;

        public KBCategoryForm(IKBCategoryService service, UserContext user)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (user    == null) throw new ArgumentNullException("user");
            _service = service;
            _user    = user;
            InitializeComponent();
        }

        private void KBCategoryForm_Load(object sender, EventArgs e) { LoadCategories(); }

        private void LoadCategories()
        {
            try
            {
                _categories = _service.GetAll().ToList();
                RefreshList();
                RefreshParentCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูล Category ไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                item.ForeColor = c.IsActive ? Color.FromArgb(20, 20, 20) : Color.FromArgb(160, 160, 160);
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
                if (_editingId.HasValue && c.CategoryID == _editingId.Value) continue;
                cboParent.Items.Add(new ComboItem(
                    string.Format("[{0}] {1}", c.CategoryCode, c.CategoryName), c.CategoryID));
            }
            cboParent.SelectedIndex = 0;
        }

        private void LstCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstCategories.SelectedItems.Count == 0) return;
            var cat = lstCategories.SelectedItems[0].Tag as KBCategory;
            if (cat == null) return;

            _editingId        = cat.CategoryID;
            lblFormTitle.Text = "แก้ไข Category";
            txtCode.Text        = cat.CategoryCode;
            txtName.Text        = cat.CategoryName;
            txtIcon.Text        = cat.Icon;
            txtDescription.Text = cat.Description;
            numSortOrder.Value  = cat.SortOrder;
            RefreshParentCombo();
            SelectParentInCombo(cat.ParentCategoryID);
            btnDelete.Enabled = cat.IsActive;
            btnDelete.Text    = cat.IsActive ? "🗑️ ปิดใช้งาน" : "⛔ ปิดแล้ว";
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

        private void BtnNew_Click(object sender, EventArgs e)   { ClearForm(); }
        private void BtnClear_Click(object sender, EventArgs e) { ClearForm(); }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!Validate_()) return;
            var parentItem = cboParent.SelectedItem as ComboItem;
            int? parentId  = parentItem != null ? parentItem.Value : null;
            try
            {
                if (!_editingId.HasValue)
                {
                    var cat = new KBCategory
                    {
                        ParentCategoryID = parentId,
                        CategoryCode     = txtCode.Text.Trim(),
                        CategoryName     = txtName.Text.Trim(),
                        Icon             = txtIcon.Text.Trim(),
                        Description      = txtDescription.Text.Trim(),
                        SortOrder        = (int)numSortOrder.Value,
                        IsActive         = true,
                        CreatedBy        = _user.AdIdentity,
                    };
                    _service.Create(cat);
                    MessageBox.Show("เพิ่ม Category สำเร็จ", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var cat = new KBCategory
                    {
                        CategoryID       = _editingId.Value,
                        ParentCategoryID = parentId,
                        CategoryCode     = txtCode.Text.Trim(),
                        CategoryName     = txtName.Text.Trim(),
                        Icon             = txtIcon.Text.Trim(),
                        Description      = txtDescription.Text.Trim(),
                        SortOrder        = (int)numSortOrder.Value,
                    };
                    _service.Update(cat);
                    MessageBox.Show("แก้ไข Category สำเร็จ", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                LoadCategories();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!_editingId.HasValue) return;
            var cat = _categories.FirstOrDefault(c => c.CategoryID == _editingId.Value);
            string name = cat != null ? cat.CategoryName : _editingId.Value.ToString();
            var confirm = MessageBox.Show(
                string.Format("ต้องการปิดใช้งาน Category \"{0}\" ใช่หรือไม่?", name),
                "ยืนยันการปิดใช้งาน", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
            try
            {
                _service.Deactivate(_editingId.Value);
                MessageBox.Show("ปิดใช้งาน Category สำเร็จ", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCategories();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ปิดใช้งานไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            _editingId          = null;
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

        private bool Validate_()
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
