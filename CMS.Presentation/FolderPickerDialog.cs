using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog เลือก Folder ปลายทาง — แสดง 2 panel:
    ///   ซ้าย  = TreeView hierarchy (navigate ตามโครงสร้าง)
    ///   ขวา   = ListView flat-list แสดง path เต็มทุก folder ชัดเจน
    /// เลือกได้จากทั้งสองฝั่ง
    /// </summary>
    public class FolderPickerDialog : Form
    {
        // ── Controls ──────────────────────────────────────────────────────────
        private SplitContainer _split;
        private TreeView       _treeView;
        private ListView       _listView;
        private Label          _lblHeader;
        private Label          _lblSourceInfo;
        private Label          _lblTreeHeader;
        private Label          _lblListHeader;
        private Panel          _pnlHeader;
        private Panel          _pnlSourceInfo;
        private Panel          _pnlSummary;
        private Label          _lblSummaryFolder;
        private Label          _lblSummaryDept;
        private Label          _lblSummaryLevel;
        private Label          _lblSummaryPath;
        private Panel          _pnlFooter;
        private Button         _btnOk;
        private Button         _btnCancel;
        private TextBox        _txtSearch;
        private ToolTip        _toolTip;

        // ── State ──────────────────────────────────────────────────────────────
        private Dictionary<int, Folder>   _folderMap   = new Dictionary<int, Folder>();
        private Dictionary<int, string>   _pathMap     = new Dictionary<int, string>();   // folderId → full path string
        private List<OrgUnit>             _orgUnits    = new List<OrgUnit>();              // full OrgUnit hierarchy
        private List<Folder>             _visibleList  = new List<Folder>();

        // ── Properties ────────────────────────────────────────────────────────
        public int?         SourceFolderId    { get; set; }
        public HashSet<int> ExcludedFolderIds { get; private set; }
        public int?         SelectedFolderId  { get; private set; }

        // ── Constructor ───────────────────────────────────────────────────────
        public FolderPickerDialog()
        {
            ExcludedFolderIds = new HashSet<int>();
            InitializeControls();
        }

        // ── UI Build ──────────────────────────────────────────────────────────
        private void InitializeControls()
        {
            _toolTip = new ToolTip { AutoPopDelay = 9000, ShowAlways = true };

            _pnlHeader     = new Panel();
            _lblHeader     = new Label();
            _pnlSourceInfo = new Panel();
            _lblSourceInfo = new Label();

            _split = new SplitContainer();
            _treeView  = new TreeView();
            _listView  = new ListView();
            _lblTreeHeader = new Label();
            _lblListHeader = new Label();
            _txtSearch     = new TextBox();

            _pnlSummary       = new Panel();
            _lblSummaryFolder = new Label();
            _lblSummaryDept   = new Label();
            _lblSummaryLevel  = new Label();
            _lblSummaryPath   = new Label();

            _pnlFooter = new Panel();
            _btnOk     = new Button();
            _btnCancel = new Button();

            SuspendLayout();

            // ── Header ────────────────────────────────────────────────────────
            _pnlHeader.BackColor = Color.FromArgb(31, 56, 100);
            _pnlHeader.Controls.Add(_lblHeader);
            _pnlHeader.Dock   = DockStyle.Top;
            _pnlHeader.Height = 44;

            _lblHeader.Dock      = DockStyle.Fill;
            _lblHeader.Font      = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            _lblHeader.ForeColor = Color.White;
            _lblHeader.Padding   = new Padding(12, 0, 0, 0);
            _lblHeader.Text      = "📁  เลือก Folder ปลายทาง";
            _lblHeader.TextAlign = ContentAlignment.MiddleLeft;

            // ── Source info ───────────────────────────────────────────────────
            _pnlSourceInfo.BackColor = Color.FromArgb(219, 234, 254);
            _pnlSourceInfo.Controls.Add(_lblSourceInfo);
            _pnlSourceInfo.Dock   = DockStyle.Top;
            _pnlSourceInfo.Height = 26;

            _lblSourceInfo.Dock      = DockStyle.Fill;
            _lblSourceInfo.Font      = new Font("Segoe UI", 9F);
            _lblSourceInfo.ForeColor = Color.FromArgb(30, 64, 120);
            _lblSourceInfo.Padding   = new Padding(12, 0, 0, 0);
            _lblSourceInfo.Text      = "ย้ายจาก: —";
            _lblSourceInfo.TextAlign = ContentAlignment.MiddleLeft;

            // ── SplitContainer — Panel2 ปิดไว้ ใช้ TreeView อย่างเดียว ────────
            _split.Dock             = DockStyle.Fill;
            _split.BorderStyle      = BorderStyle.None;
            _split.Panel1.Padding   = new Padding(0);
            _split.Panel2Collapsed  = true;   // ซ่อน right panel

            // Left panel: sub-header + search + TreeView
            _lblTreeHeader.Dock      = DockStyle.Top;
            _lblTreeHeader.Height    = 24;
            _lblTreeHeader.BackColor = Color.FromArgb(240, 244, 252);
            _lblTreeHeader.Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblTreeHeader.ForeColor = Color.FromArgb(60, 80, 120);
            _lblTreeHeader.Padding   = new Padding(8, 0, 0, 0);
            _lblTreeHeader.Text      = "🗂 โครงสร้างหน่วยงาน";
            _lblTreeHeader.TextAlign = ContentAlignment.MiddleLeft;

            // Search box ย้ายมาอยู่บน TreeView
            _txtSearch.Dock        = DockStyle.Top;
            _txtSearch.Font        = new Font("Segoe UI", 9F);
            _txtSearch.Height      = 24;
            _txtSearch.BorderStyle = BorderStyle.FixedSingle;
            _txtSearch.ForeColor   = Color.FromArgb(80, 80, 80);
            _txtSearch.Text        = "🔍 พิมพ์ค้นหา folder...";
            _txtSearch.GotFocus   += (s, e) => { if (_txtSearch.Text.StartsWith("🔍")) _txtSearch.Text = ""; _txtSearch.ForeColor = Color.Black; };
            _txtSearch.LostFocus  += (s, e) => { if (string.IsNullOrWhiteSpace(_txtSearch.Text)) { _txtSearch.Text = "🔍 พิมพ์ค้นหา folder..."; _txtSearch.ForeColor = Color.FromArgb(80, 80, 80); } };
            _txtSearch.TextChanged += TxtSearch_TextChanged;

            _treeView.Dock          = DockStyle.Fill;
            _treeView.Font          = new Font("Segoe UI", 9F);
            _treeView.ShowLines     = true;
            _treeView.ShowPlusMinus = true;
            _treeView.FullRowSelect = true;
            _treeView.HideSelection = false;
            _treeView.BorderStyle   = BorderStyle.None;
            _treeView.Indent        = 20;
            _treeView.ItemHeight    = 22;
            _treeView.AfterSelect  += TreeView_AfterSelect;
            _treeView.DoubleClick  += (s, e) => { if (_btnOk.Enabled) BtnOk_Click(s, e); };

            _split.Panel1.Controls.Add(_treeView);
            _split.Panel1.Controls.Add(_txtSearch);
            _split.Panel1.Controls.Add(_lblTreeHeader);

            // Right panel controls (ไม่ใช้แล้ว — collapsed)
            _split.Panel2.Controls.Add(_listView);
            _split.Panel2.Controls.Add(_lblListHeader);

            // ── Summary panel ─────────────────────────────────────────────────
            _pnlSummary.BackColor = Color.FromArgb(248, 250, 255);
            _pnlSummary.Dock      = DockStyle.Bottom;
            _pnlSummary.Height    = 80;
            _pnlSummary.Padding   = new Padding(12, 4, 12, 4);

            _lblSummaryFolder.Dock      = DockStyle.Top;
            _lblSummaryFolder.Height    = 22;
            _lblSummaryFolder.Font      = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblSummaryFolder.ForeColor = Color.FromArgb(31, 56, 100);
            _lblSummaryFolder.Text      = "กรุณาเลือก Folder ปลายทาง";

            _lblSummaryDept.Dock      = DockStyle.Top;
            _lblSummaryDept.Height    = 17;
            _lblSummaryDept.Font      = new Font("Segoe UI", 8.5F);
            _lblSummaryDept.ForeColor = Color.FromArgb(70, 70, 70);
            _lblSummaryDept.Text      = "";

            _lblSummaryLevel.Dock      = DockStyle.Top;
            _lblSummaryLevel.Height    = 17;
            _lblSummaryLevel.Font      = new Font("Segoe UI", 8.5F);
            _lblSummaryLevel.ForeColor = Color.FromArgb(70, 70, 70);
            _lblSummaryLevel.Text      = "";

            _lblSummaryPath.Dock      = DockStyle.Top;
            _lblSummaryPath.Height    = 15;
            _lblSummaryPath.Font      = new Font("Segoe UI", 8F, FontStyle.Italic);
            _lblSummaryPath.ForeColor = Color.FromArgb(130, 130, 130);
            _lblSummaryPath.Text      = "";

            var sepLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(200, 212, 235) };
            _pnlSummary.Controls.Add(_lblSummaryPath);
            _pnlSummary.Controls.Add(_lblSummaryLevel);
            _pnlSummary.Controls.Add(_lblSummaryDept);
            _pnlSummary.Controls.Add(_lblSummaryFolder);
            _pnlSummary.Controls.Add(sepLine);

            // ── Footer ────────────────────────────────────────────────────────
            _pnlFooter.BackColor = Color.FromArgb(245, 247, 250);
            _pnlFooter.Controls.Add(_btnOk);
            _pnlFooter.Controls.Add(_btnCancel);
            _pnlFooter.Dock   = DockStyle.Bottom;
            _pnlFooter.Height = 50;
            _pnlFooter.Resize += (s, e) =>
            {
                _btnCancel.Left = _pnlFooter.ClientSize.Width - _btnCancel.Width - 10;
                _btnOk.Left     = _btnCancel.Left - _btnOk.Width - 8;
                _btnOk.Top = _btnCancel.Top = (_pnlFooter.Height - _btnOk.Height) / 2;
            };

            _btnOk.BackColor = Color.FromArgb(31, 56, 100);
            _btnOk.Enabled   = false;
            _btnOk.FlatStyle = FlatStyle.Flat;
            _btnOk.FlatAppearance.BorderSize = 0;
            _btnOk.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _btnOk.ForeColor = Color.White;
            _btnOk.Location  = new Point(232, 8);
            _btnOk.Size      = new Size(130, 34);
            _btnOk.Text      = "✔  ย้ายมาที่นี่";
            _btnOk.UseVisualStyleBackColor = false;
            _btnOk.Click    += BtnOk_Click;

            _btnCancel.BackColor = Color.White;
            _btnCancel.FlatStyle = FlatStyle.Flat;
            _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            _btnCancel.Font      = new Font("Segoe UI", 9.5F);
            _btnCancel.ForeColor = Color.FromArgb(80, 80, 80);
            _btnCancel.Location  = new Point(370, 8);
            _btnCancel.Size      = new Size(90, 34);
            _btnCancel.Text      = "ยกเลิก";
            _btnCancel.UseVisualStyleBackColor = false;
            _btnCancel.Click    += BtnCancel_Click;

            // ── Form ─────────────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode       = AutoScaleMode.Font;
            CancelButton        = _btnCancel;
            ClientSize          = new Size(480, 600);
            FormBorderStyle     = FormBorderStyle.FixedDialog;
            MaximizeBox         = false;
            MinimizeBox         = false;
            StartPosition       = FormStartPosition.CenterParent;
            Text                = "เลือก Folder ปลายทาง";

            Controls.Add(_split);
            Controls.Add(_pnlSummary);
            Controls.Add(_pnlFooter);
            Controls.Add(_pnlSourceInfo);
            Controls.Add(_pnlHeader);

            ResumeLayout(false);
        }

        // ── Public API ────────────────────────────────────────────────────────
        public void LoadFolders(IEnumerable<Folder> allFolders, IEnumerable<int> excludedIds,
                                IEnumerable<OrgUnit> orgUnits = null)
        {
            if (allFolders == null) allFolders = Enumerable.Empty<Folder>();

            _orgUnits = orgUnits != null ? orgUnits.ToList() : new List<OrgUnit>();

            ExcludedFolderIds = excludedIds != null
                ? new HashSet<int>(excludedIds)
                : new HashSet<int>();

            var allList = allFolders.ToList();
            _folderMap = allList.ToDictionary(f => f.FolderID, f => f);

            // Source folder label
            if (SourceFolderId.HasValue && _folderMap.ContainsKey(SourceFolderId.Value))
                _lblSourceInfo.Text = "ย้ายจาก:  📂 " + _folderMap[SourceFolderId.Value].FolderName;

            _visibleList = allList
                .Where(f => !f.IsDeleted && !f.IsArchived && !ExcludedFolderIds.Contains(f.FolderID))
                .ToList();

            // Build path map
            _pathMap = BuildPathMap(_visibleList, _folderMap);

            // Build TreeView
            BuildTree(_visibleList);
        }

        // ── TreeView build ────────────────────────────────────────────────────
        private void BuildTree(List<Folder> visible)
        {
            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();

            // ── สร้าง folder nodes ──────────────────────────────────────────
            var folderNodeMap = new Dictionary<int, TreeNode>();
            foreach (var folder in visible)
            {
                var node = new TreeNode("📂 " + folder.FolderName)
                {
                    Tag       = folder.FolderID,
                    Name      = folder.FolderID.ToString(),
                    ForeColor = LevelColor(folder.ConfidentialityLevel),
                };
                if (folder.ConfidentialityLevel <= 2)
                    node.NodeFont = new Font("Segoe UI", 9F, FontStyle.Bold);
                folderNodeMap[folder.FolderID] = node;
            }

            // จัด parent-child ของ folder และแยก root folders ตาม OrgUnit
            var rootsByOrgUnit = new Dictionary<int, List<TreeNode>>();
            foreach (var folder in visible)
            {
                var node = folderNodeMap[folder.FolderID];
                if (folder.ParentFolderID.HasValue && folderNodeMap.ContainsKey(folder.ParentFolderID.Value))
                    folderNodeMap[folder.ParentFolderID.Value].Nodes.Add(node);
                else
                {
                    if (!rootsByOrgUnit.ContainsKey(folder.OrgUnitID))
                        rootsByOrgUnit[folder.OrgUnitID] = new List<TreeNode>();
                    rootsByOrgUnit[folder.OrgUnitID].Add(node);
                }
            }

            if (_orgUnits.Count > 0)
            {
                // ── สร้าง OrgUnit nodes พร้อม hierarchy ─────────────────────
                var ouMap     = _orgUnits.ToDictionary(ou => ou.OrgUnitId, ou => ou);
                var ouNodeMap = new Dictionary<int, TreeNode>();

                foreach (var ou in _orgUnits)
                {
                    bool isTopLevel = !ou.ParentOrgUnitID.HasValue
                                      || !ouMap.ContainsKey(ou.ParentOrgUnitID.Value);

                    var ouNode = new TreeNode("🏢 " + ou.Name)
                    {
                        Tag       = null,   // ไม่ใช่ folder — กด OK ไม่ได้
                        BackColor = isTopLevel
                                    ? Color.FromArgb(31, 56, 100)     // Node 0 — เข้ม
                                    : Color.FromArgb(219, 234, 254),  // Sub-dept — ฟ้าอ่อน
                        ForeColor = isTopLevel ? Color.White : Color.FromArgb(21, 46, 90),
                        NodeFont  = new Font("Segoe UI",
                                             isTopLevel ? 10F : 9F,
                                             FontStyle.Bold),
                    };
                    ouNodeMap[ou.OrgUnitId] = ouNode;
                }

                // วาง OrgUnit ใน hierarchy
                var rootOuNodes = new List<TreeNode>();
                foreach (var ou in _orgUnits.OrderBy(o => o.OrgUnitType).ThenBy(o => o.SortOrder))
                {
                    var ouNode = ouNodeMap[ou.OrgUnitId];
                    if (ou.ParentOrgUnitID.HasValue && ouNodeMap.ContainsKey(ou.ParentOrgUnitID.Value))
                        ouNodeMap[ou.ParentOrgUnitID.Value].Nodes.Add(ouNode);
                    else
                        rootOuNodes.Add(ouNode);
                }

                // เพิ่ม folder nodes ใต้ OrgUnit ของมัน
                foreach (var kvp in rootsByOrgUnit)
                {
                    if (ouNodeMap.ContainsKey(kvp.Key))
                        foreach (var fn in kvp.Value.OrderBy(n => n.Text))
                            ouNodeMap[kvp.Key].Nodes.Add(fn);
                    else
                        foreach (var fn in kvp.Value)
                            _treeView.Nodes.Add(fn);  // fallback
                }

                // เพิ่ม root OrgUnit nodes (Node 0)
                foreach (var n in rootOuNodes)
                    _treeView.Nodes.Add(n);

                // Expand Node 0 เท่านั้น (sub-dept ยุบไว้ให้ user เลือกเปิดเอง)
                foreach (TreeNode n in _treeView.Nodes)
                    n.Expand();
            }
            else
            {
                // Fallback: ไม่มี OrgUnit data
                foreach (var folder in visible)
                {
                    var node = folderNodeMap[folder.FolderID];
                    if (!folder.ParentFolderID.HasValue || !folderNodeMap.ContainsKey(folder.ParentFolderID.Value))
                        _treeView.Nodes.Add(node);
                }
                ExpandToLevel(_treeView.Nodes, 0, 1);
            }

            _treeView.EndUpdate();
        }

        // ── ListView build ────────────────────────────────────────────────────
        private void PopulateListView(List<Folder> folders, string filter)
        {
            _listView.BeginUpdate();
            _listView.Items.Clear();

            foreach (var folder in folders)
            {
                string path = _pathMap.ContainsKey(folder.FolderID) ? _pathMap[folder.FolderID] : folder.FolderName;

                // แยก parent path ออกจาก folder name (รวม OrgUnit name ที่ prefix อยู่แล้ว)
                string parentPath = "";
                var pathParts = path.Split(new[] { "  ›  " }, StringSplitOptions.None);
                if (pathParts.Length > 1)
                    parentPath = string.Join("  ›  ", pathParts.Take(pathParts.Length - 1));

                // ถ้าค้นหา ให้ filter
                if (!string.IsNullOrEmpty(filter)
                    && filter != "🔍 พิมพ์ค้นหา..."
                    && !path.ToLower().Contains(filter.ToLower()))
                    continue;

                var item = new ListViewItem(folder.FolderName);
                item.SubItems.Add(string.IsNullOrEmpty(parentPath) ? "(Root)" : parentPath);
                item.Tag      = folder.FolderID;
                item.ForeColor = LevelColor(folder.ConfidentialityLevel);
                if (folder.ConfidentialityLevel <= 2)
                    item.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

                _toolTip.SetToolTip(_listView, path);
                _listView.Items.Add(item);
            }

            // Auto-resize columns
            _listView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            _listView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            if (_listView.Columns[0].Width < 120) _listView.Columns[0].Width = 120;
            if (_listView.Columns[1].Width < 200) _listView.Columns[1].Width = 200;

            _listView.EndUpdate();
        }

        // ── Events ────────────────────────────────────────────────────────────
        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag == null) return;  // OrgUnit node — ไม่ใช่ folder
            var folderId = (int)e.Node.Tag;
            UpdateSummary(folderId);

            // Sync ListView selection
            foreach (ListViewItem item in _listView.Items)
            {
                if ((int)item.Tag == folderId)
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    break;
                }
            }
        }

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0) return;
            var folderId = (int)_listView.SelectedItems[0].Tag;
            UpdateSummary(folderId);

            // Sync TreeView
            if (_treeView.Nodes.Count > 0)
            {
                var node = FindTreeNode(_treeView.Nodes, folderId);
                if (node != null)
                {
                    _treeView.SelectedNode = node;
                    node.EnsureVisible();
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string q = _txtSearch.Text;
            if (q == "🔍 พิมพ์ค้นหา folder...") q = "";
            FilterTree(q);
        }

        // Filter tree — rebuild tree เฉพาะ folder ที่ตรงกับ query
        private void FilterTree(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                BuildTree(_visibleList);
            }
            else
            {
                var lower    = query.ToLower();
                var filtered = _visibleList
                    .Where(f => f.FolderName.ToLower().Contains(lower)
                             || (_pathMap.ContainsKey(f.FolderID)
                                 && _pathMap[f.FolderID].ToLower().Contains(lower)))
                    .ToList();
                BuildTree(filtered);
            }
        }

        private void UpdateSummary(int folderId)
        {
            _btnOk.Enabled = true;

            if (!_folderMap.ContainsKey(folderId)) return;

            var folder   = _folderMap[folderId];
            var fullPath = _pathMap.ContainsKey(folderId) ? _pathMap[folderId] : folder.FolderName;
            var badge    = LevelBadge(folder.ConfidentialityLevel);

            // หา parent path (ชื่อแผนก/หน่วยงาน ทั้งสาย)
            string parentPath = "";
            var parts = fullPath.Split(new[] { "  ›  " }, StringSplitOptions.None);
            if (parts.Length > 1)
                parentPath = string.Join("  ›  ", parts.Take(parts.Length - 1));

            _lblSummaryFolder.Text      = "📂  " + folder.FolderName;
            _lblSummaryFolder.ForeColor = LevelColor(folder.ConfidentialityLevel);

            _lblSummaryDept.Text = string.IsNullOrEmpty(parentPath)
                ? "🏢  อยู่ที่: (Root)"
                : "🏢  อยู่ใน:  " + parentPath;

            _lblSummaryLevel.Text      = "🔒  ระดับ:  " + badge;
            _lblSummaryLevel.ForeColor = LevelColor(folder.ConfidentialityLevel);

            _lblSummaryPath.Text = "📍  " + fullPath;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            // ดึง selection จาก ListView ก่อน ถ้าไม่มีใช้ TreeView
            int? folderId = null;
            if (_listView.SelectedItems.Count > 0)
                folderId = (int)_listView.SelectedItems[0].Tag;
            else if (_treeView.SelectedNode != null)
                folderId = (int)_treeView.SelectedNode.Tag;

            if (!folderId.HasValue)
            {
                MessageBox.Show("กรุณาเลือก Folder ปลายทางก่อน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SelectedFolderId = folderId;
            DialogResult     = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>สร้าง path map folderId → "[หน่วยงาน]  ›  parent  ›  folderName"</summary>
        private Dictionary<int, string> BuildPathMap(
            List<Folder> visible, Dictionary<int, Folder> allMap)
        {
            var result = new Dictionary<int, string>();

            foreach (var folder in visible)
            {
                var parts   = new List<string>();
                var current = folder;
                var visited = new HashSet<int>();

                while (current != null && !visited.Contains(current.FolderID))
                {
                    visited.Add(current.FolderID);
                    parts.Insert(0, current.FolderName);
                    if (current.ParentFolderID.HasValue && allMap.ContainsKey(current.ParentFolderID.Value))
                        current = allMap[current.ParentFolderID.Value];
                    else
                        break;
                }

                // Prepend OrgUnit name — ทำให้ path บอกหน่วยงานด้วย
                var orgUnit = _orgUnits.FirstOrDefault(ou => ou.OrgUnitId == folder.OrgUnitID);
                if (orgUnit != null)
                    parts.Insert(0, orgUnit.Name);

                result[folder.FolderID] = string.Join("  ›  ", parts);
            }
            return result;
        }

        private static TreeNode FindTreeNode(TreeNodeCollection nodes, int folderId)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is int && (int)node.Tag == folderId) return node;
                var found = FindTreeNode(node.Nodes, folderId);
                if (found != null) return found;
            }
            return null;
        }

        private static void ExpandToLevel(TreeNodeCollection nodes, int cur, int max)
        {
            if (cur > max) return;
            foreach (TreeNode n in nodes)
            {
                n.Expand();
                ExpandToLevel(n.Nodes, cur + 1, max);
            }
        }

        private static Color LevelColor(int level)
        {
            switch (level)
            {
                case 1: return Color.FromArgb(180, 30,  30);
                case 2: return Color.FromArgb(160, 70,   0);
                case 3: return Color.FromArgb( 25, 110, 25);
                default: return Color.FromArgb(31,  56, 100);
            }
        }

        private static string LevelBadge(int level)
        {
            switch (level)
            {
                case 1: return "1HC — High Confidential";
                case 2: return "2SC — Semi Confidential";
                case 3: return "3NC — Normal Confidential";
                case 4: return "4PC — Public";
                default: return "Level " + level;
            }
        }
    }
}
