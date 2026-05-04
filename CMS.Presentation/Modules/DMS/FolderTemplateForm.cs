using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// จัดการ Template Folder — สร้าง/แก้ไข/ลบ Template และ Sub-Folder items
    /// ตรงกับ dialog "จัดการ Template Folder" ในรูป
    /// </summary>
    public partial class FolderTemplateForm : Form
    {
        private readonly IFolderTemplateController       _repo;
        private readonly IUserAccessOrgLookupController  _orgLookup;
        private readonly UserContext                     _user;

        private List<FolderTemplate>     _templates = new List<FolderTemplate>();
        private List<FolderTemplateItem> _items     = new List<FolderTemplateItem>();
        private List<OrgUnit>            _orgUnits  = new List<OrgUnit>();
        private FolderTemplate           _selected;

        public FolderTemplateForm(IFolderTemplateController repo, IUserAccessOrgLookupController orgLookup, UserContext user)
        {
            if (repo      == null) throw new ArgumentNullException("repo");
            if (orgLookup == null) throw new ArgumentNullException("orgLookup");
            if (user      == null) throw new ArgumentNullException("user");
            _repo      = repo;
            _orgLookup = orgLookup;
            _user      = user;
            InitializeComponent();
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private void FolderTemplateForm_Load(object sender, EventArgs e)
        {
            // ปุ่ม ยกเลิก ชิดขวาเสมอ
            _btnCancel.Left = pnlBottom.ClientSize.Width - _btnCancel.Width - 16;
            pnlBottom.Resize += (s, ev) =>
                _btnCancel.Left = pnlBottom.ClientSize.Width - _btnCancel.Width - 16;

            LoadOrgUnits();
            LoadTemplates();
        }

        // ── Custom draw for template list ─────────────────────────────────────

        private void LstTemplates_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _templates.Count) return;
            var  tmpl     = _templates[e.Index];
            bool selected = (e.State & DrawItemState.Selected) != 0;

            var bgColor  = selected ? Color.FromArgb(0, 51, 102) : Color.White;
            var altColor = selected ? Color.FromArgb(0, 51, 102) : Color.FromArgb(249, 250, 252);
            e.Graphics.FillRectangle(new SolidBrush(e.Index % 2 == 0 ? bgColor : altColor), e.Bounds);

            // accent bar ด้านซ้ายเมื่อ selected
            if (selected)
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 170, 255)),
                    new Rectangle(e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height));

            int textX = e.Bounds.X + (selected ? 12 : 10);

            e.Graphics.DrawString(
                "📋  " + tmpl.TemplateName,
                new Font("Segoe UI", 9.5f, FontStyle.Bold),
                selected ? Brushes.White : new SolidBrush(Color.FromArgb(20, 20, 20)),
                new PointF(textX, e.Bounds.Y + 5));

            // แสดงชื่อหน่วยงาน หรือ TargetType ถ้าเป็น Generic
            string subLine = !string.IsNullOrEmpty(tmpl.OrgUnitName)
                ? "🏢 " + tmpl.OrgUnitName
                : tmpl.TargetType;

            e.Graphics.DrawString(
                subLine,
                new Font("Segoe UI", 8f),
                selected ? new SolidBrush(Color.FromArgb(160, 210, 255)) : new SolidBrush(Color.FromArgb(110, 110, 110)),
                new PointF(textX + 2, e.Bounds.Y + 21));

            // bottom separator line
            if (!selected)
                e.Graphics.DrawLine(new Pen(Color.FromArgb(230, 232, 238)),
                    e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        // ── Data loading ──────────────────────────────────────────────────────

        private void LoadOrgUnits()
        {
            try
            {
                _orgUnits = _orgLookup.GetHierarchy()
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .OrderBy(u => u.Name)
                    .ToList();

                _cboOrgUnit.Items.Clear();
                // รายการแรก = Generic (ไม่ผูกกับหน่วยงานใด)
                _cboOrgUnit.Items.Add("— ทุกหน่วยงาน (Generic) —");
                foreach (var org in _orgUnits)
                    _cboOrgUnit.Items.Add(org.Name);
                _cboOrgUnit.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลดข้อมูลหน่วยงานได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadTemplates()
        {
            try
            {
                _templates = _repo.GetAll().OrderBy(t => t.TemplateName).ToList();
                _lstTemplates.Items.Clear();
                foreach (var t in _templates)
                    _lstTemplates.Items.Add(t.TemplateName);

                if (_lstTemplates.Items.Count > 0)
                    _lstTemplates.SelectedIndex = 0;
                else
                    ClearRightPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลด Template ได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LstTemplates_Changed(object sender, EventArgs e)
        {
            int idx = _lstTemplates.SelectedIndex;
            if (idx < 0 || idx >= _templates.Count) { ClearRightPanel(); return; }

            _selected         = _templates[idx];
            _txtName.Text     = _selected.TemplateName;
            int typeIdx       = _cboType.Items.IndexOf(_selected.TargetType);
            _cboType.SelectedIndex = typeIdx >= 0 ? typeIdx : 0;

            // bind OrgUnit: index 0 = Generic, index N+1 = _orgUnits[N]
            if (_selected.OrgUnitID.HasValue)
            {
                int orgIdx = _orgUnits.FindIndex(o => o.OrgUnitID == _selected.OrgUnitID.Value);
                _cboOrgUnit.SelectedIndex = orgIdx >= 0 ? orgIdx + 1 : 0;
            }
            else
            {
                _cboOrgUnit.SelectedIndex = 0; // Generic
            }

            LoadItems(_selected.TemplateID);
        }

        private void LoadItems(int templateId)
        {
            try
            {
                _items = _repo.GetItems(templateId).OrderBy(i => i.SortOrder).ToList();
                RefreshItemList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลด Sub-Folder ได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshItemList()
        {
            _lstItems.Items.Clear();
            foreach (var item in _items.Where(i => i.ParentItemID == null).OrderBy(i => i.SortOrder))
                AddItemNode(item, "");
        }

        private void AddItemNode(FolderTemplateItem item, string indent)
        {
            var lvi = new ListViewItem(indent + "📂 " + item.FolderName);
            lvi.SubItems.Add(ConfidentialityLabel(item.ConfidentialityLevel));
            lvi.SubItems.Add(item.SortOrder.ToString());
            lvi.SubItems.Add(item.InheritPermission ? "✔" : "");
            lvi.Tag = item;
            _lstItems.Items.Add(lvi);

            foreach (var child in _items.Where(i => i.ParentItemID == item.ItemID).OrderBy(i => i.SortOrder))
                AddItemNode(child, indent + "    ");
        }

        private void ClearRightPanel()
        {
            _selected              = null;
            _txtName.Text          = "";
            _cboType.SelectedIndex = 0;
            _cboOrgUnit.SelectedIndex = 0;
            _lstItems.Items.Clear();
            _items.Clear();
        }

        // ── New template ──────────────────────────────────────────────────────

        private void BtnNew_Click(object sender, EventArgs e)
        {
            string name = PromptText("สร้าง Template ใหม่", "ชื่อ Template:", "");
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                var tmpl = new FolderTemplate
                {
                    TemplateCode = "T" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    TemplateName = name.Trim(),
                    TargetType   = "Department",
                    IsActive     = true,
                    CreatedBy    = _user.UserID,
                    CreatedDate  = DateTime.Now
                };
                int newId = _repo.Insert(tmpl);
                tmpl.TemplateID = newId;

                LoadTemplates();
                int idx = _templates.FindIndex(t => t.TemplateID == newId);
                if (idx >= 0) _lstTemplates.SelectedIndex = idx;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถสร้าง Template ได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Save template ─────────────────────────────────────────────────────

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_selected == null)
            { MessageBox.Show("กรุณาเลือก Template ก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            { MessageBox.Show("กรุณาระบุชื่อ Template", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                _selected.TemplateName = _txtName.Text.Trim();
                _selected.TargetType   = _cboType.SelectedItem.ToString();
                // OrgUnit: SelectedIndex 0 = Generic (null), 1+ = _orgUnits[idx-1]
                int orgIdx = _cboOrgUnit.SelectedIndex;
                _selected.OrgUnitID    = orgIdx > 0 ? (int?)_orgUnits[orgIdx - 1].OrgUnitID : null;
                _selected.OrgUnitName  = orgIdx > 0 ? _orgUnits[orgIdx - 1].Name : null;
                _selected.IsActive     = true;
                _repo.Update(_selected);
                LoadTemplates();
                MessageBox.Show("✅ บันทึก Template เรียบร้อยแล้ว", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถบันทึกได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Delete template ───────────────────────────────────────────────────

        private void BtnDeleteTemplate_Click(object sender, EventArgs e)
        {
            if (_selected == null) return;
            if (MessageBox.Show(
                    string.Format("ลบ Template '{0}' และ Sub-Folder ทั้งหมดหรือไม่?", _selected.TemplateName),
                    "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                _repo.Delete(_selected.TemplateID);
                LoadTemplates();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถลบได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Add sub-folder ────────────────────────────────────────────────────

        private void BtnAddFolder_Click(object sender, EventArgs e)
        {
            if (_selected == null)
            { MessageBox.Show("กรุณาเลือก Template ก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            string name = PromptText("เพิ่ม Sub-Folder", "ชื่อ Folder:", "");
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                int nextOrder = _items.Count > 0 ? _items.Max(i => i.SortOrder) + 1 : 1;
                var item = new FolderTemplateItem
                {
                    TemplateID           = _selected.TemplateID,
                    ParentItemID         = null,
                    FolderName           = name.Trim(),
                    ConfidentialityLevel = 4,
                    SortOrder            = nextOrder,
                    InheritPermission    = true
                };
                int newId = _repo.InsertItem(item);
                item.ItemID = newId;
                LoadItems(_selected.TemplateID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถเพิ่ม Folder ได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Edit sub-folder ───────────────────────────────────────────────────

        private void BtnEditFolder_Click(object sender, EventArgs e)
        {
            if (_lstItems.SelectedItems.Count == 0)
            { MessageBox.Show("กรุณาเลือก Folder ที่ต้องการแก้ไข", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            var item = _lstItems.SelectedItems[0].Tag as FolderTemplateItem;
            if (item == null) return;

            string name = PromptText("แก้ไข Folder", "ชื่อ Folder:", item.FolderName);
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                item.FolderName = name.Trim();
                _repo.UpdateItem(item);
                LoadItems(_selected.TemplateID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถแก้ไขได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Delete sub-folder ─────────────────────────────────────────────────

        private void BtnDeleteFolder_Click(object sender, EventArgs e)
        {
            if (_lstItems.SelectedItems.Count == 0) return;
            var item = _lstItems.SelectedItems[0].Tag as FolderTemplateItem;
            if (item == null) return;

            if (MessageBox.Show(
                    string.Format("ลบ Folder '{0}' หรือไม่?", item.FolderName),
                    "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                _repo.DeleteItem(item.ItemID);
                LoadItems(_selected.TemplateID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถลบได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string PromptText(string title, string label, string defaultValue)
        {
            using (var dlg = new Form())
            {
                dlg.Text            = title;
                dlg.ClientSize      = new Size(360, 120);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;

                dlg.Controls.Add(new Label { Text = label, Location = new Point(12, 16), AutoSize = true, Font = new Font("Segoe UI", 9) });
                var txt = new TextBox { Location = new Point(12, 36), Size = new Size(330, 24), Font = new Font("Segoe UI", 9), Text = defaultValue };
                dlg.Controls.Add(txt);

                var btnOk     = new Button { Text = "ตกลง",   Location = new Point(160, 72), Size = new Size(80, 28), DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "ยกเลิก", Location = new Point(250, 72), Size = new Size(80, 28), DialogResult = DialogResult.Cancel };
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                return dlg.ShowDialog() == DialogResult.OK ? txt.Text : null;
            }
        }

        private static string ConfidentialityLabel(int level)
        {
            switch (level)
            {
                case 1: return "1HC - Highly Confidential";
                case 2: return "2SC - Secret";
                case 3: return "3NC - Confidential";
                case 4: return "4PC - Public";
                default: return level.ToString();
            }
        }
    }
}
