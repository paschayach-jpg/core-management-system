using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog เลือก Folder ปลายทาง — แสดง TreeView hierarchy
    /// </summary>
    public partial class FolderPickerDialog : Form
    {
        // ── State ──────────────────────────────────────────────────────────────
        private Dictionary<int, Folder> _folderMap  = new Dictionary<int, Folder>();
        private Dictionary<int, string> _pathMap    = new Dictionary<int, string>();
        private List<OrgUnit>           _orgUnits   = new List<OrgUnit>();
        private List<Folder>            _visibleList = new List<Folder>();

        // ── Properties ────────────────────────────────────────────────────────
        public int?         SourceFolderId    { get; set; }
        public HashSet<int> ExcludedFolderIds { get; private set; }
        public int?         SelectedFolderId  { get; private set; }

        // ── Constructor ───────────────────────────────────────────────────────
        // Guard flag — ป้องกัน AfterSelect fire จาก programmatic selection
        private bool _suppressSelect = false;

        public FolderPickerDialog()
        {
            ExcludedFolderIds = new HashSet<int>();
            InitializeComponent();

            // Wire dynamic events
            _pnlFooter.Resize += (s, e) =>
            {
                _btnCancel.Left = _pnlFooter.ClientSize.Width - _btnCancel.Width - 10;
                _btnOk.Left     = _btnCancel.Left - _btnOk.Width - 8;
                _btnOk.Top = _btnCancel.Top = (_pnlFooter.Height - _btnOk.Height) / 2;
            };

            _txtSearch.GotFocus  += (s, e) =>
            {
                if (_txtSearch.Text.StartsWith("🔍")) _txtSearch.Text = "";
                _txtSearch.ForeColor = Color.Black;
            };
            _txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _txtSearch.Text      = "🔍 พิมพ์ค้นหา folder...";
                    _txtSearch.ForeColor = Color.FromArgb(80, 80, 80);
                }
            };

            // ใช้ NodeMouseClick แทน AfterSelect เพื่อ fire เฉพาะเมื่อ user คลิกจริงๆ
            // AfterSelect fire ได้จากหลายสาเหตุ (expand, keyboard, programmatic) ทำให้ selection กระโดด
            _treeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                var folder = e.Node?.Tag as Folder;
                if (folder == null) return; // OrgUnit node — ไม่ทำอะไร
                _suppressSelect = true;
                try { _treeView.SelectedNode = e.Node; }
                finally { _suppressSelect = false; }
                SelectedFolderId = folder.FolderID;
                UpdateSummary(folder.FolderID);
                _btnOk.Enabled = true;
            };

            // ป้องกัน OrgUnit node (tag=null) ถูก select จาก keyboard/programmatic
            _treeView.BeforeSelect += (s, e) =>
            {
                if (_suppressSelect) return;
                if (e.Node?.Tag == null)
                    e.Cancel = true;
            };

            _treeView.DoubleClick += (s, e) => { if (_btnOk.Enabled) BtnOk_Click(s, e); };
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

            _folderMap.Clear();
            foreach (var f in allFolders)
                _folderMap[f.FolderID] = f;

            _pathMap = BuildPathMap(_folderMap);

            var visible = _folderMap.Values
                .Where(f => !ExcludedFolderIds.Contains(f.FolderID) && !f.IsDeleted && !f.IsArchived)
                .ToList();

            _visibleList = visible;

            if (SourceFolderId.HasValue && _folderMap.TryGetValue(SourceFolderId.Value, out var src))
                _lblSourceInfo.Text = string.Format("ย้ายจาก: {0}", src.FolderName);
            else
                _lblSourceInfo.Text = "ย้ายจาก: —";

            BuildTree(visible);
            PopulateListView(visible, "");
        }

        private void BuildTree(List<Folder> visible)
        {
            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();

            var visibleIds = new HashSet<int>(visible.Select(f => f.FolderID));

            if (_orgUnits.Count > 0)
            {
                // สร้าง OrgUnit hierarchy: SubCommittee → Division → Department → Folders
                // ใช้ OrgUnitType: 1=SubCommittee, 2=Division, 3=Department
                var ouById = _orgUnits.ToDictionary(u => u.OrgUnitID);

                // Build OrgUnit tree nodes recursively
                Func<OrgUnit, TreeNode> buildOuNode = null;
                buildOuNode = (ou) =>
                {
                    string icon = ou.OrgUnitType == 1 ? "🏢 "
                                : ou.OrgUnitType == 2 ? "📂 "
                                : "📋 ";
                    var ouNode = new TreeNode(icon + ou.Name)
                    {
                        Tag      = null,  // OrgUnit — not selectable
                        ForeColor = ou.OrgUnitType == 1
                            ? Color.FromArgb(31, 56, 100)
                            : ou.OrgUnitType == 2
                                ? Color.FromArgb(0, 80, 160)
                                : Color.FromArgb(60, 60, 60),
                        NodeFont = ou.OrgUnitType == 1
                            ? new Font(_treeView.Font, FontStyle.Bold)
                            : _treeView.Font
                    };

                    // Add child OrgUnits
                    var children = _orgUnits
                        .Where(c => c.ParentOrgUnitID == ou.OrgUnitID)
                        .OrderBy(c => c.Name);
                    foreach (var child in children)
                        ouNode.Nodes.Add(buildOuNode(child));

                    // Add folders belonging to this OrgUnit
                    var rootFolders = visible
                        .Where(f => f.OrgUnitID == ou.OrgUnitID
                               && (f.ParentFolderID == null || !visibleIds.Contains(f.ParentFolderID.Value)))
                        .OrderBy(f => f.FolderName);
                    foreach (var folder in rootFolders)
                    {
                        var fNode = CreateNode(folder);
                        ouNode.Nodes.Add(fNode);
                        AddChildren(fNode, folder.FolderID, visible);
                    }

                    return ouNode;
                };

                // Start from SubCommittee roots (no parent or parent not in list)
                var ouIds = new HashSet<int>(_orgUnits.Select(u => u.OrgUnitID));
                var roots = _orgUnits
                    .Where(u => !u.ParentOrgUnitID.HasValue || !ouIds.Contains(u.ParentOrgUnitID.Value))
                    .OrderBy(u => u.Name);

                foreach (var sc in roots)
                    _treeView.Nodes.Add(buildOuNode(sc));
            }
            else
            {
                // Fallback: ไม่มี OrgUnit → flat tree
                var roots = visible
                    .Where(f => f.ParentFolderID == null || !visibleIds.Contains(f.ParentFolderID.Value))
                    .OrderBy(f => f.FolderName).ToList();
                foreach (var root in roots)
                {
                    var node = CreateNode(root);
                    _treeView.Nodes.Add(node);
                    AddChildren(node, root.FolderID, visible);
                }
            }

            ExpandToLevel(_treeView.Nodes, 0, 2);
            _treeView.EndUpdate();
        }

        private void AddChildren(TreeNode parent, int parentId, List<Folder> visible)
        {
            var children = visible.Where(f => f.ParentFolderID == parentId)
                                  .OrderBy(f => f.FolderName);
            foreach (var child in children)
            {
                var node = CreateNode(child);
                parent.Nodes.Add(node);
                AddChildren(node, child.FolderID, visible);
            }
        }

        private TreeNode CreateNode(Folder f)
        {
            string badge = LevelBadge(f.ConfidentialityLevel);
            string label = string.Format("{0}  {1}", f.FolderName, badge);
            return new TreeNode(label)
            {
                Tag       = f,
                ForeColor = LevelColor(f.ConfidentialityLevel)
            };
        }

        private void PopulateListView(List<Folder> folders, string filter)
        {
            _listView.BeginUpdate();
            _listView.Items.Clear();

            var filtered = string.IsNullOrWhiteSpace(filter)
                ? folders
                : folders.Where(f => f.FolderName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                               || (_pathMap.ContainsKey(f.FolderID) && _pathMap[f.FolderID].IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
                         .ToList();

            foreach (var f in filtered.OrderBy(f => _pathMap.ContainsKey(f.FolderID) ? _pathMap[f.FolderID] : f.FolderName))
            {
                string path = _pathMap.ContainsKey(f.FolderID) ? _pathMap[f.FolderID] : f.FolderName;
                var item = new ListViewItem(f.FolderName) { Tag = f, ForeColor = LevelColor(f.ConfidentialityLevel) };
                item.SubItems.Add(path);
                item.SubItems.Add(LevelBadge(f.ConfidentialityLevel));
                _listView.Items.Add(item);
            }

            _listView.EndUpdate();
        }

        // ── Event handlers ────────────────────────────────────────────────────
        // TreeView_AfterSelect — ยังคงอยู่เพราะ Designer wire ไว้
        // แต่ logic จริงย้ายไปอยู่ใน NodeMouseClick แล้ว
        // ใช้ _suppressSelect เพื่อป้องกัน double-fire
        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_suppressSelect) return;
            // ไม่ทำอะไร — NodeMouseClick จัดการแทนแล้ว
        }

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0) return;
            var folder = _listView.SelectedItems[0].Tag as Folder;
            if (folder == null) return;
            SelectedFolderId = folder.FolderID;
            UpdateSummary(folder.FolderID);
            _btnOk.Enabled = true;

            // Sync tree selection — suppress AfterSelect to avoid double-update
            var node = FindTreeNode(_treeView.Nodes, folder.FolderID);
            if (node != null)
            {
                _suppressSelect = true;
                try
                {
                    _treeView.SelectedNode = node;
                    node.EnsureVisible();
                }
                finally
                {
                    _suppressSelect = false;
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string q = _txtSearch.Text.StartsWith("🔍") ? "" : _txtSearch.Text.Trim();
            FilterTree(q);
            PopulateListView(_visibleList, q);
        }

        private void FilterTree(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                BuildTree(_visibleList);
                return;
            }

            var matched = _visibleList
                .Where(f => f.FolderName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            BuildTree(matched);
            ExpandToLevel(_treeView.Nodes, 0, 10);
        }

        private void UpdateSummary(int folderId)
        {
            if (!_folderMap.TryGetValue(folderId, out var f)) return;

            _lblSummaryFolder.Text = string.Format("📁  {0}", f.FolderName);
            _lblSummaryLevel.Text  = string.Format("ระดับ: {0}", LevelBadge(f.ConfidentialityLevel));
            _lblSummaryPath.Text   = _pathMap.ContainsKey(folderId) ? _pathMap[folderId] : "";

            string deptName = "";
            if (_orgUnits.Count > 0)
            {
                var ou = _orgUnits.FirstOrDefault(u => u.OrgUnitID == f.OrgUnitID);
                if (ou != null) deptName = ou.Name;
            }
            _lblSummaryDept.Text = string.IsNullOrEmpty(deptName) ? "" : string.Format("หน่วยงาน: {0}", deptName);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (!SelectedFolderId.HasValue)
            {
                MessageBox.Show("กรุณาเลือก Folder ปลายทาง", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (SourceFolderId.HasValue && SelectedFolderId.Value == SourceFolderId.Value)
            {
                MessageBox.Show("ไม่สามารถย้ายไปยัง Folder เดิมได้", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private Dictionary<int, string> BuildPathMap(Dictionary<int, Folder> map)
        {
            // Build OrgUnit name lookup
            var ouNames = _orgUnits.ToDictionary(u => u.OrgUnitID, u => u.Name);

            var result = new Dictionary<int, string>();
            foreach (var f in map.Values)
            {
                var parts = new List<string>();
                var cur = f;
                while (cur != null)
                {
                    parts.Insert(0, cur.FolderName);
                    cur = cur.ParentFolderID.HasValue && map.ContainsKey(cur.ParentFolderID.Value)
                        ? map[cur.ParentFolderID.Value] : null;
                }

                // Prepend OrgUnit name if available
                if (f.OrgUnitID > 0 && ouNames.ContainsKey(f.OrgUnitID))
                    parts.Insert(0, ouNames[f.OrgUnitID]);

                result[f.FolderID] = string.Join(" › ", parts);
            }
            return result;
        }

        private static TreeNode FindTreeNode(TreeNodeCollection nodes, int folderId)
        {
            foreach (TreeNode n in nodes)
            {
                if (n.Tag is Folder f && f.FolderID == folderId) return n;
                var found = FindTreeNode(n.Nodes, folderId);
                if (found != null) return found;
            }
            return null;
        }

        private static void ExpandToLevel(TreeNodeCollection nodes, int cur, int max)
        {
            if (cur >= max) return;
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
                case 1: return Color.FromArgb(180, 0, 0);
                case 2: return Color.FromArgb(200, 80, 0);
                case 3: return Color.FromArgb(0, 80, 160);
                default: return Color.FromArgb(0, 130, 60);
            }
        }

        private static string LevelBadge(int level)
        {
            switch (level)
            {
                case 1: return "[1HC]";
                case 2: return "[2SC]";
                case 3: return "[3NC]";
                default: return "[4PC]";
            }
        }
    }
}
