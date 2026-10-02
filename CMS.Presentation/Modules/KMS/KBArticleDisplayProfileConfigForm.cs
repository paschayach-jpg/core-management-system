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
        private bool _bindingProfiles;   // กำลังสร้างรายการ Profile ใหม่ — ไม่ให้ SelectionChanged โหลดฟอร์มทับ

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
            if (_presenter == null || _bindingProfiles) return;
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

            // Rows.Clear() ทำให้ selection หายและ scroll กลับบนสุด (แถวแรกถูกเลือกเอง) — จำแถวที่กำลังแก้ไขกับตำแหน่ง scroll ไว้คืนให้หลังสร้างใหม่
            int? keepProfileId = EditingProfileId;
            int firstRow = dgvProfiles.FirstDisplayedScrollingRowIndex;

            _bindingProfiles = true;   // การเลือกแถวตอนสร้างรายการไม่ใช่การที่ผู้ใช้เลือก Profile — ห้ามโหลดฟอร์มใหม่ทับสิ่งที่กำลังแก้
            try
            {
                dgvProfiles.Rows.Clear();
                foreach (var p in _profiles)
                {
                    int rowIndex = dgvProfiles.Rows.Add(p.ProfileName + (p.IsActive ? string.Empty : "  (ปิด)"));
                    dgvProfiles.Rows[rowIndex].Tag = p;
                    dgvProfiles.Rows[rowIndex].DefaultCellStyle.BackColor = p.IsActive
                        ? Color.FromArgb(223, 247, 227)   // เขียวอ่อน — ใช้งานอยู่
                        : Color.FromArgb(253, 223, 221);  // แดงอ่อน — ปิดใช้งานแล้ว
                }

                dgvProfiles.ClearSelection();
                dgvProfiles.CurrentCell = null;

                if (firstRow >= 0 && firstRow < dgvProfiles.RowCount && dgvProfiles.DisplayedRowCount(false) > 0)
                    dgvProfiles.FirstDisplayedScrollingRowIndex = firstRow;

                if (keepProfileId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvProfiles.Rows)
                    {
                        var p = row.Tag as KBArticleDisplayProfile;
                        if (p == null || p.ProfileID != keepProfileId.Value) continue;
                        dgvProfiles.CurrentCell = row.Cells[0];   // เลื่อนให้เห็นแถวนี้ด้วยถ้าอยู่นอกจอ
                        row.Selected = true;
                        break;
                    }
                }
            }
            finally
            {
                _bindingProfiles = false;
            }
        }

        public void BindColumnGrid(IEnumerable<KBArticleDisplayProfileColumnRow> rows)
        {
            dgvColumnMapping.Rows.Clear();
            foreach (var row in rows)
            {
                int rowIndex = dgvColumnMapping.Rows.Add(row.SlotKey, row.SlotDescription, row.DisplayLabel, row.IsVisible);
                dgvColumnMapping.Rows[rowIndex].Tag = row;
            }
        }

        public void BindCategoryAssignment(IEnumerable<KBCategory> allCategories, IEnumerable<int> assignedCategoryIds)
        {
            var assignedSet = new HashSet<int>(assignedCategoryIds);

            // แสดงเป็น tree ตาม ParentCategoryID (กี่ชั้นก็ได้) — ติ๊กแต่ละโหนดแยกกัน ไม่ cascade ไปลูก/แม่
            // เพราะหมวดย่อยที่ไม่ได้ผูก Profile เองจะ inherit จากหมวดแม่อยู่แล้ว (ผูกซ้ำทุกลูกจะทำให้เสียการ inherit)
            treeCategories.BeginUpdate();
            try
            {
                treeCategories.Nodes.Clear();

                var lastNodeAtDepth = new List<TreeNode>();
                foreach (var item in KBCategoryTree.Flatten(allCategories, false))
                {
                    var node = new TreeNode(item.Category.CategoryName)
                    {
                        Tag     = item.Category,
                        Checked = assignedSet.Contains(item.Category.CategoryID),
                    };

                    if (item.Depth == 0)
                        treeCategories.Nodes.Add(node);
                    else
                        lastNodeAtDepth[item.Depth - 1].Nodes.Add(node);

                    if (lastNodeAtDepth.Count > item.Depth)
                        lastNodeAtDepth[item.Depth] = node;
                    else
                        lastNodeAtDepth.Add(node);
                }

                // เปิดมาแบบยุบทั้งหมด เห็นเฉพาะระดับบนสุด — ผู้ใช้กดขยายเอง (หมวดเยอะ ขยายทั้งหมดแล้วยาวเกินไป)
                treeCategories.CollapseAll();
                if (treeCategories.Nodes.Count > 0)
                    treeCategories.Nodes[0].EnsureVisible();
            }
            finally
            {
                treeCategories.EndUpdate();
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
                    SlotDescription = Convert.ToString(gridRow.Cells[colSlotDescription.Index].Value ?? string.Empty).Trim(),
                    DisplayLabel    = Convert.ToString(gridRow.Cells[colDisplayLabel.Index].Value ?? string.Empty).Trim(),
                    IsVisible       = Convert.ToBoolean(gridRow.Cells[colIsVisible.Index].Value ?? true),
                });
            }
            return result;
        }

        public IList<int> ReadAssignedCategoryIds()
        {
            var result = new List<int>();
            CollectCheckedCategoryIds(treeCategories.Nodes, result);
            return result;
        }

        private static void CollectCheckedCategoryIds(TreeNodeCollection nodes, List<int> result)
        {
            foreach (TreeNode node in nodes)
            {
                var cat = node.Tag as KBCategory;
                if (node.Checked && cat != null) result.Add(cat.CategoryID);
                CollectCheckedCategoryIds(node.Nodes, result);
            }
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
            treeCategories.Nodes.Clear();
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

            colSlotKey.ToolTipText = "รหัสจุดที่แสดงผล (ตายตัว แก้ไม่ได้) — ใช้ระบุว่าแถวนี้คือจุดไหน";
            colSlotDescription.ToolTipText = "คำอธิบายจุดที่แสดงผลของ Profile นี้ — แก้ไขได้ (เก็บต่อ Profile)";
            colDisplayLabel.ToolTipText = "ข้อความ header/caption ที่จะแสดงแทนค่า default — เว้นว่างจะใช้ default ตัวเดิม";
            colIsVisible.ToolTipText = "ยกเลิกติ๊กเพื่อซ่อนคอลัมน์/label นี้สำหรับหมวดที่ผูก Profile นี้";
        }
    }
}
