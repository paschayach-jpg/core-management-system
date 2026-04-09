using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Recycle Bin form — shows soft-deleted folders with Restore / Permanently Delete.
    /// Accessible from the Admin ribbon tab.
    /// </summary>
    public partial class RecycleBinForm : Form
    {
        // ── Services ────────────────────────────────────────────────────────────
        private readonly IFolderService       _folderService;
        private readonly IOrgStructureService _orgService;
        private readonly UserContext          _user;

        // ── State ───────────────────────────────────────────────────────────────
        private List<Folder>  _folders       = new List<Folder>();
        private List<OrgUnit> _orgUnits      = new List<OrgUnit>();
        private int?          _filterOrgUnit = null;

        // ────────────────────────────────────────────────────────────────────────
        public RecycleBinForm(
            IFolderService       folderService,
            UserContext          user,
            IOrgStructureService orgService = null)
        {
            if (folderService == null) throw new ArgumentNullException("folderService");
            if (user          == null) throw new ArgumentNullException("user");

            _folderService = folderService;
            _user          = user;
            _orgService    = orgService;

            InitializeComponent();
            LoadOrgUnits();
            LoadData();
        }

        // ── Data Loading ─────────────────────────────────────────────────────────

        private void LoadOrgUnits()
        {
            _cmbOrgUnit.Items.Clear();
            _cmbOrgUnit.Items.Add(new OrgUnitItem { Id = null, Name = "— ทั้งหมด —" });

            if (_orgService != null)
            {
                try
                {
                    _orgUnits = _orgService.GetHierarchy().ToList();
                    foreach (var ou in _orgUnits.OrderBy(o => o.Name))
                        _cmbOrgUnit.Items.Add(new OrgUnitItem { Id = ou.OrgUnitId, Name = ou.Name });
                }
                catch { /* ignore */ }
            }

            _cmbOrgUnit.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                _folders = _folderService.GetDeletedFolders(_filterOrgUnit, _user).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลดถังขยะได้: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _folders = new List<Folder>();
            }

            PopulateList();
        }

        private void PopulateList()
        {
            _listView.BeginUpdate();
            _listView.Items.Clear();

            foreach (var folder in _folders.OrderByDescending(f => f.DeletedAt ?? DateTime.MinValue))
            {
                string orgUnitName = GetOrgUnitName(folder.OrgUnitID);
                string parentPath  = folder.ParentFolderId.HasValue
                    ? "Folder #" + folder.ParentFolderId.Value
                    : "— (Root)";
                string deletedAt   = folder.DeletedAt.HasValue
                    ? folder.DeletedAt.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                    : "—";

                int daysRemaining = folder.DeletedAt.HasValue
                    ? Math.Max(0, 30 - (int)(DateTime.Now - folder.DeletedAt.Value).TotalDays)
                    : 30;
                string remaining = daysRemaining == 0 ? "หมดอายุ" : daysRemaining + " วัน";

                string level = ConfidentialityBadge(folder.ConfidentialityLevel);

                var item = new ListViewItem(new[]
                {
                    "📁  " + folder.FolderName,
                    orgUnitName,
                    parentPath,
                    deletedAt,
                    remaining,
                    level
                });
                item.Tag = folder;

                // เตือนด้วยสีเมื่อเหลือน้อยกว่า 7 วัน
                if (daysRemaining <= 7)
                    item.ForeColor = Color.FromArgb(180, 60, 0);
                else
                    item.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);

                _listView.Items.Add(item);
            }

            _listView.EndUpdate();

            bool isEmpty = _listView.Items.Count == 0;
            _listView.Visible   = !isEmpty;
            _emptyPanel.Visible = isEmpty;

            _btnEmptyBin.Enabled = !isEmpty;
            UpdateActionButtons();

            _lblStatus.Text = isEmpty
                ? "ถังขยะว่างเปล่า"
                : string.Format("📁 {0} Folder ในถังขยะ", _listView.Items.Count);
        }

        private string GetOrgUnitName(int orgUnitId)
        {
            var ou = _orgUnits.FirstOrDefault(o => o.OrgUnitId == orgUnitId);
            return ou != null ? ou.Name : "(หน่วยงาน #" + orgUnitId + ")";
        }

        // ── Event Handlers ───────────────────────────────────────────────────────

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // owner-draw handled entirely in DrawSubItem
        }

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateActionButtons();
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 1)
                ConfirmRestore(new[] { _listView.SelectedItems[0].Tag as Folder });
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var sorter = (ListViewColumnSorter)_listView.ListViewItemSorter;
            if (sorter.SortColumn == e.Column)
                sorter.Order = sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            else
            {
                sorter.SortColumn = e.Column;
                sorter.Order      = SortOrder.Ascending;
            }
            _listView.Sort();
        }

        private void CmbOrgUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sel = _cmbOrgUnit.SelectedItem as OrgUnitItem;
            _filterOrgUnit = sel?.Id;
            LoadData();
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedFolders();
            if (selected.Count == 0) return;
            ConfirmRestore(selected);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedFolders();
            if (selected.Count == 0) return;
            ConfirmPermanentDelete(selected);
        }

        private void BtnEmptyBin_Click(object sender, EventArgs e)
        {
            if (_folders.Count == 0) return;

            var result = MessageBox.Show(
                string.Format("ต้องการล้างถังขยะทั้งหมด ({0} Folder) หรือไม่?\n\n⚠️ การลบถาวรไม่สามารถกู้คืนได้", _folders.Count),
                "ล้างถังขยะ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes) return;

            int success = 0, failed = 0;
            foreach (var folder in _folders.ToList())
            {
                try
                {
                    _folderService.PermanentlyDeleteFolder(folder.FolderId, _user);
                    success++;
                }
                catch { failed++; }
            }

            string msg = string.Format("ล้างถังขยะเรียบร้อย — ลบ {0} Folder", success);
            if (failed > 0) msg += string.Format(" (ล้มเหลว {0} รายการ)", failed);
            MessageBox.Show(msg, "เสร็จสิ้น", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadData();
        }

        // ── Actions ──────────────────────────────────────────────────────────────

        private void ConfirmRestore(IList<Folder> targets)
        {
            if (targets == null || targets.Count == 0) return;

            string names = targets.Count == 1
                ? string.Format("'{0}'", targets[0].FolderName)
                : string.Format("{0} Folder ที่เลือก", targets.Count);

            var result = MessageBox.Show(
                string.Format("คืนค่า {0} กลับไปยังตำแหน่งเดิมหรือไม่?", names),
                "คืนค่า Folder", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            int success = 0, failed = 0;
            foreach (var folder in targets)
            {
                try
                {
                    _folderService.RestoreFolder(folder.FolderId, null, _user);
                    success++;
                }
                catch (InvalidOperationException ex)
                {
                    var choice = MessageBox.Show(
                        string.Format("ไม่พบตำแหน่งเดิมของ '{0}'\n\n{1}\n\nต้องการคืนค่าเป็น Root Folder แทนหรือไม่?",
                            folder.FolderName, ex.Message),
                        "ไม่พบตำแหน่งเดิม", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (choice == DialogResult.Yes)
                    {
                        try { _folderService.RestoreFolder(folder.FolderId, 0, _user); success++; }
                        catch { failed++; }
                    }
                    else failed++;
                }
                catch { failed++; }
            }

            string msg = string.Format("คืนค่า {0} Folder เรียบร้อย", success);
            if (failed > 0) msg += string.Format(" (ล้มเหลว {0} รายการ)", failed);
            MessageBox.Show(msg, "เสร็จสิ้น", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadData();
        }

        private void ConfirmPermanentDelete(IList<Folder> targets)
        {
            if (targets == null || targets.Count == 0) return;

            string names = targets.Count == 1
                ? string.Format("'{0}'", targets[0].FolderName)
                : string.Format("{0} Folder ที่เลือก", targets.Count);

            var result = MessageBox.Show(
                string.Format("⚠️ ลบ {0} ออกจากระบบถาวรหรือไม่?\n\nการลบถาวรไม่สามารถกู้คืนได้", names),
                "ลบถาวร", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes) return;

            int success = 0, failed = 0;
            foreach (var folder in targets)
            {
                try
                {
                    _folderService.PermanentlyDeleteFolder(folder.FolderId, _user);
                    success++;
                }
                catch { failed++; }
            }

            string msg = string.Format("ลบถาวร {0} Folder เรียบร้อย", success);
            if (failed > 0) msg += string.Format(" (ล้มเหลว {0} รายการ)", failed);
            MessageBox.Show(msg, "เสร็จสิ้น", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadData();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void UpdateActionButtons()
        {
            bool canManage = _user.Role == UserRole.Manager || _user.Role == UserRole.Admin;
            int  selCount  = _listView.SelectedItems.Count;

            _btnRestore.Enabled  = selCount > 0;
            _btnDelete.Enabled   = selCount > 0 && canManage;
            _btnEmptyBin.Enabled = _folders.Count > 0 && canManage;

            if (selCount > 0)
                _lblStatus.Text = string.Format("เลือก {0} รายการ  |  รวม {1} Folder ในถังขยะ",
                    selCount, _folders.Count);
            else
                _lblStatus.Text = _folders.Count == 0
                    ? "ถังขยะว่างเปล่า"
                    : string.Format("📁 {0} Folder ในถังขยะ", _folders.Count);
        }

        private List<Folder> GetSelectedFolders()
        {
            return _listView.SelectedItems.Cast<ListViewItem>()
                .Select(i => i.Tag as Folder)
                .Where(f => f != null)
                .ToList();
        }

        private static string ConfidentialityBadge(int level)
        {
            switch (level)
            {
                case 1: return "1HC";
                case 2: return "2SC";
                case 3: return "3NC";
                case 4: return "4PC";
                default: return level.ToString();
            }
        }

        private static Color ConfidentialityColor(int level)
        {
            switch (level)
            {
                case 1: return Color.FromArgb(180, 30, 30);
                case 2: return Color.FromArgb(200, 100, 0);
                case 3: return Color.FromArgb(31, 100, 31);
                case 4: return Color.FromArgb(31, 56, 100);
                default: return Color.Black;
            }
        }

        // ── Owner-draw ListView (alternating rows) ───────────────────────────────

        private void ListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            bool isSelected = e.Item.Selected;
            bool isEven     = e.ItemIndex % 2 == 0;

            Color back = isSelected
                ? Color.FromArgb(204, 228, 247)
                : isEven ? Color.White : Color.FromArgb(248, 249, 252);

            using (var br = new SolidBrush(back))
                e.Graphics.FillRectangle(br, e.Bounds);

            Color fore = isSelected ? Color.Black : e.Item.ForeColor;
            if (fore == Color.Empty) fore = Color.Black;

            var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.ListView.Font,
                Rectangle.Inflate(e.Bounds, -2, 0), fore, flags);
        }

        // ── Inner classes ────────────────────────────────────────────────────────

        private class OrgUnitItem
        {
            public int?   Id   { get; set; }
            public string Name { get; set; }
            public override string ToString() { return Name; }
        }

        private class ListViewColumnSorter : System.Collections.IComparer
        {
            public int       SortColumn { get; set; }
            public SortOrder Order      { get; set; } = SortOrder.Ascending;

            public int Compare(object x, object y)
            {
                var lx = (ListViewItem)x;
                var ly = (ListViewItem)y;
                string tx = SortColumn < lx.SubItems.Count ? lx.SubItems[SortColumn].Text : "";
                string ty = SortColumn < ly.SubItems.Count ? ly.SubItems[SortColumn].Text : "";
                int cmp = string.Compare(tx, ty, StringComparison.CurrentCultureIgnoreCase);
                return Order == SortOrder.Descending ? -cmp : cmp;
            }
        }
    }
}
