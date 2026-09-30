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
    /// หน้า config Article Display Profile สำหรับ Knowledge Base (MOD-006) — กำหนด header
    /// label/การซ่อนแสดงคอลัมน์ของ dgvArticles/ArticleCardControl ใน KBMainForm แบบ dynamic ตาม
    /// CategoryID (แทนที่ hardcode ตายตัวแบบเดิม) รองรับ: เพิ่ม/แก้ไข/Soft-Delete Profile,
    /// แก้ label/visible ของแต่ละ slot, ผูก Profile ให้กับ category ต้นสาย (หมวดย่อย inherit ต่อเอง)
    /// เปิดจากปุ่มในหัวฟอร์ม KBMainForm (ไม่ผ่านเมนู admin ใดๆ)
    /// </summary>
    public partial class KBArticleDisplayProfileConfigForm : Form, IKBArticleDisplayProfileConfigController
    {
        private KBArticleDisplayProfileConfigController _presenter;
        private readonly UserContext _user;

        private List<KBArticleDisplayProfile> _profiles = new List<KBArticleDisplayProfile>();

        public event EventHandler BackRequested;

        public KBArticleDisplayProfileConfigForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
        }

        public void AttachPresenter(KBArticleDisplayProfileConfigController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form events ──────────────────────────────────────────────────────

        private void KBArticleDisplayProfileConfigForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            SetupColumnMappingGrid();
            _presenter.Initialize();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
        }

        private void BtnNewProfile_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            dgvProfiles.ClearSelection();
            _presenter.OnNewProfile();
        }

        private void DgvProfiles_SelectionChanged(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (dgvProfiles.SelectedRows.Count == 0) return;
            var profile = dgvProfiles.SelectedRows[0].Tag as KBArticleDisplayProfile;
            if (profile == null) return;
            _presenter.OnProfileSelected(profile);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (string.IsNullOrWhiteSpace(txtProfileName.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อ Profile", "กรุณากรอกข้อมูล",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProfileName.Focus();
                return;
            }
            _presenter.Save();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (!EditingProfileId.HasValue) return;
            if (_recordIsActive)
            {
                var confirm = MessageBox.Show(
                    string.Format("ต้องการปิดใช้งาน Profile \"{0}\" ใช่หรือไม่?", ProfileName),
                    "ยืนยันการปิดใช้งาน", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm == DialogResult.Yes)
                    _presenter.Delete();
            }
            else
            {
                var confirm = MessageBox.Show(
                    string.Format("ต้องการเปิดใช้งาน Profile \"{0}\" อีกครั้งใช่หรือไม่?", ProfileName),
                    "ยืนยันการเปิดใช้งาน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                    _presenter.Activate();
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ── IKBArticleDisplayProfileConfigController ────────────────────────

        public int? EditingProfileId { get; set; }

        public string ProfileName
        {
            get { return txtProfileName.Text.Trim(); }
            set { txtProfileName.Text = value ?? string.Empty; }
        }

        public string ProfileDescription
        {
            get { return txtProfileDescription.Text.Trim(); }
            set { txtProfileDescription.Text = value ?? string.Empty; }
        }

        public bool DeleteEnabled
        {
            set { btnDelete.Enabled = value; }
        }

        private bool _recordIsActive = true;

        public bool RecordIsActive
        {
            set
            {
                _recordIsActive = value;
                btnDelete.Text = value ? "🗑️ ปิดใช้งาน" : "♻️ เปิดใช้งาน";
                btnDelete.BackColor = value
                    ? System.Drawing.Color.FromArgb(200, 50, 50)
                    : System.Drawing.Color.FromArgb(40, 150, 80);
            }
        }

        public void BindProfileList(IEnumerable<KBArticleDisplayProfile> profiles)
        {
            _profiles = new List<KBArticleDisplayProfile>(profiles);
            dgvProfiles.Rows.Clear();
            foreach (var p in _profiles)
            {
                int rowIndex = dgvProfiles.Rows.Add(p.ProfileName + (p.IsActive ? string.Empty : "  (ปิด)"));
                dgvProfiles.Rows[rowIndex].Tag = p;
                dgvProfiles.Rows[rowIndex].DefaultCellStyle.BackColor = p.IsActive
                    ? Color.FromArgb(223, 247, 227)   // เขียวอ่อน — ใช้งานอยู่
                    : Color.FromArgb(253, 223, 221);  // แดงอ่อน — ปิดใช้งานแล้ว
            }
        }

        public void BindColumnGrid(IEnumerable<KBArticleDisplayProfileColumnRow> rows)
        {
            dgvColumnMapping.Rows.Clear();
            foreach (var row in rows)
            {
                int rowIndex = dgvColumnMapping.Rows.Add(row.SlotDescription, row.DisplayLabel, row.IsVisible);
                dgvColumnMapping.Rows[rowIndex].Tag = row;
            }
        }

        public void BindCategoryAssignment(IEnumerable<KBCategory> allCategories, IEnumerable<int> assignedCategoryIds)
        {
            var assignedSet = new HashSet<int>(assignedCategoryIds);

            clbCategories.Items.Clear();
            foreach (var cat in allCategories.OrderBy(c => c.CategoryName))
            {
                int index = clbCategories.Items.Add(cat);
                clbCategories.SetItemChecked(index, assignedSet.Contains(cat.CategoryID));
            }
        }

        public IList<KBArticleDisplayProfileColumnRow> ReadColumnGridEdits()
        {
            var result = new List<KBArticleDisplayProfileColumnRow>();
            foreach (DataGridViewRow gridRow in dgvColumnMapping.Rows)
            {
                var source = gridRow.Tag as KBArticleDisplayProfileColumnRow;
                if (source == null) continue;
                result.Add(new KBArticleDisplayProfileColumnRow
                {
                    SlotKey         = source.SlotKey,
                    SlotDescription = source.SlotDescription,
                    DisplayLabel    = Convert.ToString(gridRow.Cells[colDisplayLabel.Index].Value ?? string.Empty).Trim(),
                    IsVisible       = Convert.ToBoolean(gridRow.Cells[colIsVisible.Index].Value ?? true),
                });
            }
            return result;
        }

        public IList<int> ReadAssignedCategoryIds()
        {
            var result = new List<int>();
            foreach (var item in clbCategories.CheckedItems)
            {
                var cat = item as KBCategory;
                if (cat != null) result.Add(cat.CategoryID);
            }
            return result;
        }

        public void ClearForm()
        {
            EditingProfileId = null;
            txtProfileName.Text = string.Empty;
            txtProfileDescription.Text = string.Empty;
            btnDelete.Enabled = false;
            RecordIsActive = true;
            dgvProfiles.ClearSelection();
            dgvColumnMapping.Rows.Clear();
            clbCategories.Items.Clear();
            txtProfileName.Focus();
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

        private void SetupColumnMappingGrid()
        {
            dgvColumnMapping.AutoGenerateColumns = false;
            dgvColumnMapping.AllowUserToAddRows = false;
            dgvColumnMapping.RowHeadersVisible = false;

            colDisplayLabel.ToolTipText = "ข้อความ header/caption ที่จะแสดงแทนค่า default — เว้นว่างจะใช้ default ตัวเดิม";
            colIsVisible.ToolTipText = "ยกเลิกติ๊กเพื่อซ่อนคอลัมน์/label นี้สำหรับหมวดที่ผูก Profile นี้";

            clbCategories.DisplayMember = "CategoryName";
        }
    }
}
