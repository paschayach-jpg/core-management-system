using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Recycle Bin form — shows soft-deleted folders AND files with Restore / Permanently Delete.
    /// </summary>
    public partial class RecycleBinForm : Form
    {
        // ── Services ────────────────────────────────────────────────────────────
        private readonly IRecycleBinController _recycleBinController;
        private readonly UserContext           _user;

        // ── State ───────────────────────────────────────────────────────────────
        private List<Folder>       _folders       = new List<Folder>();
        private List<FileDocument> _files         = new List<FileDocument>();
        private List<OrgUnit>      _orgUnits      = new List<OrgUnit>();
        private int?               _filterOrgUnit = null;

        // ────────────────────────────────────────────────────────────────────────
        public RecycleBinForm(
            IRecycleBinController recycleBinController,
            UserContext user)
        {
            if (recycleBinController == null) throw new ArgumentNullException("recycleBinController");
            if (user == null) throw new ArgumentNullException("user");

            _recycleBinController = recycleBinController;
            _user = user;

            InitializeComponent();
            // ไม่โหลด DB ที่นี่ — โหลดใน Shown event เพื่อให้ form เปิดทันที
        }

        // ── Async initial load ────────────────────────────────────────────────────

        private async void RecycleBinForm_Shown(object sender, EventArgs e)
        {
            await LoadAllAsync(rebuildFilter: true);
        }

        private async Task LoadAllAsync(bool rebuildFilter = false)
        {
            SetLoadingState(true);

            var orgUnits = rebuildFilter ? new List<OrgUnit>() : _orgUnits;
            var folders  = new List<Folder>();
            var files    = new List<FileDocument>();

            await Task.Run(() =>
            {
                if (rebuildFilter)
                {
                    try { orgUnits = _recycleBinController.GetOrgHierarchy().ToList(); }
                    catch { }
                }
                try { folders = _recycleBinController.GetDeletedFolders(_filterOrgUnit, _user).ToList(); }
                catch { }
                try { files = _recycleBinController.GetDeletedFiles(_filterOrgUnit, _user).ToList(); }
                catch { }
            });

            if (rebuildFilter) _orgUnits = orgUnits;
            _folders = folders;
            _files   = files;

            PopulateList();
            if (rebuildFilter) BuildOrgUnitFilter();
            SetLoadingState(false);
        }

        private void SetLoadingState(bool loading)
        {
            _loadingPanel.Visible = loading;
            _toolbar.Enabled = !loading;
            if (loading)
            {
                _listView.Visible   = false;
                _emptyPanel.Visible = false;
            }
        }

        // ── Data Loading ─────────────────────────────────────────────────────────

        private void BuildOrgUnitFilter()
        {
            // Show only departments that actually have deleted items — no empty entries, no duplicates
            var distinctIds = _folders
                .Select(f => f.OrgUnitID)
                .Distinct()
                .OrderBy(id => GetOrgUnitName(id))
                .ToList();

            _cmbOrgUnit.SelectedIndexChanged -= CmbOrgUnit_SelectedIndexChanged;
            _cmbOrgUnit.Items.Clear();
            _cmbOrgUnit.Items.Add(new OrgUnitItem { Id = null, Name = "— ทั้งหมด —" });

            foreach (var id in distinctIds)
                _cmbOrgUnit.Items.Add(new OrgUnitItem { Id = id, Name = GetOrgUnitName(id) });

            // Restore previous selection if still available
            int selIdx = 0;
            if (_filterOrgUnit.HasValue)
            {
                for (int i = 1; i < _cmbOrgUnit.Items.Count; i++)
                {
                    var item = _cmbOrgUnit.Items[i] as OrgUnitItem;
                    if (item != null && item.Id == _filterOrgUnit) { selIdx = i; break; }
                }
            }
            _cmbOrgUnit.SelectedIndex = selIdx;
            _cmbOrgUnit.SelectedIndexChanged += CmbOrgUnit_SelectedIndexChanged;
        }

        private void LoadData()
        {
            try
            {
                _folders = _recycleBinController.GetDeletedFolders(_filterOrgUnit, _user).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลด Folder ในถังขยะได้: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _folders = new List<Folder>();
            }

            try
            {
                _files = _recycleBinController.GetDeletedFiles(_filterOrgUnit, _user).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลด File ในถังขยะได้: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _files = new List<FileDocument>();
            }

            PopulateList();
        }

        private void PopulateList()
        {
            _listView.BeginUpdate();
            _listView.Items.Clear();

            // ── Folders ──────────────────────────────────────────────────────────
            foreach (var folder in _folders.OrderByDescending(f => f.DeletedAt ?? DateTime.MinValue))
            {
                string orgUnitName = GetOrgUnitName(folder.OrgUnitID);
                string parentPath  = folder.ParentFolderId.HasValue
                    ? "Folder #" + folder.ParentFolderId.Value
                    : "— (Root)";
                string deletedAt = folder.DeletedAt.HasValue
                    ? folder.DeletedAt.Value.ToString("dd/MM/yyyy HH:mm")
                    : "—";

                int daysRemaining = folder.DeletedAt.HasValue
                    ? Math.Max(0, 30 - (int)(DateTime.Now - folder.DeletedAt.Value).TotalDays)
                    : 30;
                string remaining = daysRemaining == 0 ? "หมดอายุ" : daysRemaining + " วัน";
                string level     = ConfidentialityBadge(folder.ConfidentialityLevel);

                var item = new ListViewItem(new[]
                {
                    "📁  " + folder.FolderName,
                    orgUnitName,
                    parentPath,
                    deletedAt,
                    remaining,
                    level
                });
                item.Tag = new RecycleBinItem { ItemType = RecycleBinItemType.Folder, Folder = folder };

                item.ForeColor = daysRemaining <= 7
                    ? Color.FromArgb(180, 60, 0)
                    : ConfidentialityColor(folder.ConfidentialityLevel);

                _listView.Items.Add(item);
            }

            // ── Files ─────────────────────────────────────────────────────────────
            foreach (var file in _files.OrderByDescending(f => f.DeletedDate ?? DateTime.MinValue))
            {
                string orgUnitName = string.IsNullOrEmpty(file.OrgUnitName)
                    ? "—"
                    : file.OrgUnitName;
                string folderName = string.IsNullOrEmpty(file.FolderName)
                    ? "Folder #" + file.FolderID
                    : file.FolderName;
                string deletedAt = file.DeletedDate.HasValue
                    ? file.DeletedDate.Value.ToString("dd/MM/yyyy HH:mm")
                    : "—";

                int daysRemaining = file.DeletedDate.HasValue
                    ? Math.Max(0, 30 - (int)(DateTime.Now - file.DeletedDate.Value).TotalDays)
                    : 30;
                string remaining = daysRemaining == 0 ? "หมดอายุ" : daysRemaining + " วัน";
                string level     = ConfidentialityBadge(file.ConfidentialityLevel);

                var item = new ListViewItem(new[]
                {
                    "📄  " + file.OriginalFileName,
                    orgUnitName,
                    folderName,
                    deletedAt,
                    remaining,
                    level
                });
                item.Tag = new RecycleBinItem { ItemType = RecycleBinItemType.File, File = file };

                item.ForeColor = daysRemaining <= 7
                    ? Color.FromArgb(180, 60, 0)
                    : ConfidentialityColor(file.ConfidentialityLevel);

                _listView.Items.Add(item);
            }

            _listView.EndUpdate();

            int totalCount  = _listView.Items.Count;
            bool isEmpty    = totalCount == 0;
            _loadingPanel.Visible = false;
            _listView.Visible     = !isEmpty;
            _emptyPanel.Visible   = isEmpty;

            _btnEmptyBin.Enabled = !isEmpty;
            UpdateActionButtons();

            _lblStatus.Text = isEmpty
                ? "ถังขยะว่างเปล่า"
                : string.Format("📁 {0} Folder  📄 {1} File ในถังขยะ", _folders.Count, _files.Count);
        }

        private string GetOrgUnitName(int orgUnitId)
        {
            var ou = _orgUnits.FirstOrDefault(o => o.OrgUnitId == orgUnitId);
            return ou != null ? ou.Name : "(หน่วยงาน #" + orgUnitId + ")";
        }

        // ── Event Handlers ───────────────────────────────────────────────────────

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            _filterOrgUnit = null;
            await LoadAllAsync(rebuildFilter: true);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
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
                ConfirmRestore(new[] { _listView.SelectedItems[0].Tag as RecycleBinItem });
        }

        private void ListView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var hit = _listView.HitTest(e.X, e.Y);
            if (hit.Item == null) return;

            // Select the right-clicked item if it's not part of the current multi-selection
            if (!hit.Item.Selected)
            {
                _listView.SelectedItems.Clear();
                hit.Item.Selected = true;
                hit.Item.Focused  = true;
            }
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

        private async void CmbOrgUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sel = _cmbOrgUnit.SelectedItem as OrgUnitItem;
            _filterOrgUnit = sel?.Id;
            await LoadAllAsync(rebuildFilter: false);
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedItems();
            if (selected.Count == 0) return;
            ConfirmRestore(selected);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedItems();
            if (selected.Count == 0) return;
            ConfirmPermanentDelete(selected);
        }

        private void BtnEmptyBin_Click(object sender, EventArgs e)
        {
            int total = _folders.Count + _files.Count;
            if (total == 0) return;

            var result = MessageBox.Show(
                string.Format("ต้องการล้างถังขยะทั้งหมด ({0} Folder, {1} File) หรือไม่?\n\n⚠️ การลบถาวรไม่สามารถกู้คืนได้",
                    _folders.Count, _files.Count),
                "ล้างถังขยะ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes) return;

            int success = 0, failed = 0;

            foreach (var folder in _folders.ToList())
            {
                try { _recycleBinController.PermanentlyDeleteFolder(folder.FolderId, _user); success++; }
                catch { failed++; }
            }

            foreach (var file in _files.ToList())
            {
                try { _recycleBinController.PermanentlyDeleteFile(file.FileID, _user); success++; }
                catch { failed++; }
            }

            string msg = string.Format("ล้างถังขยะเรียบร้อย — ลบ {0} รายการ", success);
            if (failed > 0) msg += string.Format(" (ล้มเหลว {0} รายการ)", failed);
            MessageBox.Show(msg, "เสร็จสิ้น", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadData();
        }

        // ── Context Menu ─────────────────────────────────────────────────────────

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool canManage = _user.Role == UserRole.Manager || _user.Role == UserRole.Admin;
            int  selCount  = _listView.SelectedItems.Count;
            int  total     = _folders.Count + _files.Count;

            // ลบถาวร: แสดงเฉพาะ Manager/Admin เท่านั้น
            _ctxDelete.Visible  = canManage;
            _ctxSep1.Visible    = canManage;

            _ctxRestore.Enabled   = selCount > 0;
            _ctxDelete.Enabled    = selCount > 0 && canManage;
            _ctxSelectAll.Enabled = total > 0;

            // อัปเดต label ให้แสดงจำนวนที่เลือกด้วย
            _ctxRestore.Text = selCount > 1
                ? string.Format("♻️   คืนค่า {0} รายการ", selCount)
                : "♻️   คืนค่า (Restore)";

            _ctxDelete.Text = selCount > 1
                ? string.Format("🗑️   ลบถาวร {0} รายการ", selCount)
                : "🗑️   ลบถาวร";
        }

        private void CtxRestore_Click(object sender, EventArgs e)   { BtnRestore_Click(sender, e); }
        private void CtxDelete_Click(object sender, EventArgs e)    { BtnDelete_Click(sender, e); }
        private void CtxRefresh_Click(object sender, EventArgs e)   { BtnRefresh_Click(sender, e); }

        private void CtxSelectAll_Click(object sender, EventArgs e)
        {
            _listView.BeginUpdate();
            foreach (ListViewItem item in _listView.Items)
                item.Selected = true;
            _listView.EndUpdate();
        }

        // ── Actions ──────────────────────────────────────────────────────────────

        private void ConfirmRestore(IList<RecycleBinItem> targets)
        {
            if (targets == null || targets.Count == 0) return;

            string names = targets.Count == 1
                ? string.Format("'{0}'", targets[0].DisplayName)
                : string.Format("{0} รายการที่เลือก", targets.Count);

            var result = MessageBox.Show(
                string.Format("คืนค่า {0} กลับไปยังตำแหน่งเดิมหรือไม่?", names),
                "คืนค่า", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            int success = 0, failed = 0;
            foreach (var item in targets)
            {
                try
                {
                    if (item.ItemType == RecycleBinItemType.Folder)
                        _recycleBinController.RestoreFolder(item.Folder.FolderId, null, _user);
                    else
                        _recycleBinController.RestoreFile(item.File.FileID, _user);
                    success++;
                }
                catch (InvalidOperationException ex) when (item.ItemType == RecycleBinItemType.Folder)
                {
                    var choice = MessageBox.Show(
                        string.Format("ไม่พบตำแหน่งเดิมของ '{0}'\n\n{1}\n\nต้องการคืนค่าเป็น Root Folder แทนหรือไม่?",
                            item.Folder.FolderName, ex.Message),
                        "ไม่พบตำแหน่งเดิม", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (choice == DialogResult.Yes)
                    {
                        try { _recycleBinController.RestoreFolder(item.Folder.FolderId, 0, _user); success++; }
                        catch { failed++; }
                    }
                    else failed++;
                }
                catch { failed++; }
            }

            string msg = string.Format("คืนค่า {0} รายการเรียบร้อย", success);
            if (failed > 0) msg += string.Format(" (ล้มเหลว {0} รายการ)", failed);
            MessageBox.Show(msg, "เสร็จสิ้น", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadData();
        }

        private void ConfirmPermanentDelete(IList<RecycleBinItem> targets)
        {
            if (targets == null || targets.Count == 0) return;

            string names = targets.Count == 1
                ? string.Format("'{0}'", targets[0].DisplayName)
                : string.Format("{0} รายการที่เลือก", targets.Count);

            var result = MessageBox.Show(
                string.Format("⚠️ ลบ {0} ออกจากระบบถาวรหรือไม่?\n\nการลบถาวรไม่สามารถกู้คืนได้", names),
                "ลบถาวร", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes) return;

            int success = 0, failed = 0;
            foreach (var item in targets)
            {
                try
                {
                    if (item.ItemType == RecycleBinItemType.Folder)
                        _recycleBinController.PermanentlyDeleteFolder(item.Folder.FolderId, _user);
                    else
                        _recycleBinController.PermanentlyDeleteFile(item.File.FileID, _user);
                    success++;
                }
                catch { failed++; }
            }

            string msg = string.Format("ลบถาวร {0} รายการเรียบร้อย", success);
            if (failed > 0) msg += string.Format(" (ล้มเหลว {0} รายการ)", failed);
            MessageBox.Show(msg, "เสร็จสิ้น", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadData();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void UpdateActionButtons()
        {
            bool canManage = _user.Role == UserRole.Manager || _user.Role == UserRole.Admin;
            int  selCount  = _listView.SelectedItems.Count;
            int  total     = _folders.Count + _files.Count;

            _btnRestore.Enabled  = selCount > 0;
            _btnDelete.Enabled   = selCount > 0 && canManage;
            _btnEmptyBin.Enabled = total > 0 && canManage;

            if (selCount > 0)
                _lblStatus.Text = string.Format("เลือก {0} รายการ  |  📁 {1} Folder  📄 {2} File ในถังขยะ",
                    selCount, _folders.Count, _files.Count);
            else
                _lblStatus.Text = total == 0
                    ? "ถังขยะว่างเปล่า"
                    : string.Format("📁 {0} Folder  📄 {1} File ในถังขยะ", _folders.Count, _files.Count);
        }

        private List<RecycleBinItem> GetSelectedItems()
        {
            return _listView.SelectedItems.Cast<ListViewItem>()
                .Select(i => i.Tag as RecycleBinItem)
                .Where(r => r != null)
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
                Rectangle.Inflate(e.Bounds, -5, -1), fore, flags);
        }

        // ── Inner classes ────────────────────────────────────────────────────────

        private enum RecycleBinItemType { Folder, File }

        private class RecycleBinItem
        {
            public RecycleBinItemType ItemType { get; set; }
            public Folder             Folder   { get; set; }
            public FileDocument       File     { get; set; }

            public string DisplayName
            {
                get
                {
                    return ItemType == RecycleBinItemType.Folder
                        ? (Folder != null ? Folder.FolderName : "")
                        : (File   != null ? File.OriginalFileName : "");
                }
            }
        }

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
