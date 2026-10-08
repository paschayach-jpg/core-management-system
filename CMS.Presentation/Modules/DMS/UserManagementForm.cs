using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Admin form for managing user accounts — create, edit role, activate/deactivate.
    /// </summary>
    public partial class UserManagementForm : Form
    {
        private readonly IUserAdminController _userAdminController;
        private readonly UserContext _currentUser;
        private readonly IAuditLogController _auditLogController;
        private readonly CMS.Services.Interfaces.ILdapUserSearchService _ldapUserSearchService;

        private List<UserAccount> _allUsers = new List<UserAccount>();
        private List<HrDivision> _divisions = new List<HrDivision>();
        private List<HrDepartment> _departments = new List<HrDepartment>();
        private List<HrPosition> _positions = new List<HrPosition>();
        private List<HrPositionLevel> _levels = new List<HrPositionLevel>();
        private List<HrPositionMapping> _positionMappings = new List<HrPositionMapping>();

        private static readonly Font boldLevelFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        private int _sortColumn = -1;
        private SortOrder _sortOrder = SortOrder.None;

        public UserManagementForm(IUserAdminController userAdminController, UserContext currentUser,
                                   CMS.Services.Interfaces.IAuditService auditService = null,
                                   CMS.Services.Interfaces.ILdapUserSearchService ldapUserSearchService = null)
        {
            _userAdminController = userAdminController;
            _currentUser = currentUser;
            _auditLogController = new AuditLogController(auditService);
            _ldapUserSearchService = ldapUserSearchService;

            InitializeComponent();
            WireEvents();
            LoadUsers();     // โหลดก่อนเพื่อให้ _allUsers ใช้เป็น fallback ใน LoadHrData
            LoadHrData();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event wiring
        // ─────────────────────────────────────────────────────────────────────

        private void WireEvents()
        {
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) ApplyFilter(); };
            tabStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            cmbFilterColumn.SelectedIndexChanged += (s, e) => ApplyFilter();
            tvOrg.AfterSelect += (s, e) => ApplyFilter();
            btnSearch.Click += (s, e) => ApplyFilter();
            btnCreate.Click += BtnCreate_Click;
            btnEdit.Click += BtnEdit_Click;
            btnToggle.Click += BtnToggle_Click;
            btnRefresh.Click += (s, e) => LoadUsers();
            lvUsers.SelectedIndexChanged += LvUsers_SelectionChanged;
            lvUsers.DoubleClick += BtnEdit_Click;
            lvUsers.ColumnClick += LvUsers_ColumnClick;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Data
        // ─────────────────────────────────────────────────────────────────────

        private void LoadHrData()
        {
            // ── Phase 1: โหลดจาก HR master tables ──────────────────────────────
            if (_userAdminController != null)
            {
                try { _divisions = _userAdminController.GetDivisions().OrderBy(d => d.DivisionCode).ToList(); }
                catch { _divisions = new List<HrDivision>(); }

                try { _departments = _userAdminController.GetDepartments().OrderBy(d => d.DepartmentCode).ToList(); }
                catch { _departments = new List<HrDepartment>(); }

                try { _positions = _userAdminController.GetPositions().OrderBy(p => p.PositionName).ToList(); }
                catch { _positions = new List<HrPosition>(); }

                try { _levels = _userAdminController.GetPositionLevels().OrderBy(l => l.LevelCode).ToList(); }
                catch { _levels = new List<HrPositionLevel>(); }

                try { _positionMappings = _userAdminController.GetPositionMappings().ToList(); }
                catch { _positionMappings = new List<HrPositionMapping>(); }
            }

            // ── Phase 2: Fallback — สร้างจาก UserAccount ถ้า HR table ยังว่าง ──
            // ใช้ข้อมูลที่ users มีอยู่แล้วเป็น dropdown choices
            if (_divisions.Count == 0 && _allUsers.Count > 0)
            {
                int seq = 1;
                _divisions = _allUsers
                    .Where(u => !string.IsNullOrWhiteSpace(u.DivisionName))
                    .GroupBy(u => u.DivisionName.Trim(), StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var sample = g.First();
                        return new HrDivision
                        {
                            DivisionID = seq++,
                            DivisionCode = sample.DivisionCode ?? "",
                            DivisionName = g.Key,
                            IsActive = true
                        };
                    })
                    .ToList();
            }

            if (_departments.Count == 0 && _allUsers.Count > 0)
            {
                int seq = 1;
                _departments = _allUsers
                    .Where(u => !string.IsNullOrWhiteSpace(u.DepartmentName))
                    .GroupBy(u => u.DepartmentName.Trim(), StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var sample = g.First();
                        // หา DivisionID จาก fallback _divisions เพื่อให้ filter ตาม ฝ่ายได้
                        int divId = 0;
                        if (!string.IsNullOrWhiteSpace(sample.DivisionName))
                        {
                            var matchedDiv = _divisions.FirstOrDefault(d =>
                                string.Equals((d.DivisionName ?? "").Trim(),
                                              sample.DivisionName.Trim(),
                                              StringComparison.OrdinalIgnoreCase));
                            divId = matchedDiv != null ? matchedDiv.DivisionID : 0;
                        }
                        return new HrDepartment
                        {
                            DepartmentID = seq++,
                            DivisionID = divId,
                            DepartmentCode = sample.DepartmentCode ?? "",
                            DepartmentName = g.Key,
                            DepartmentNameTh = g.Key,
                            IsActive = true
                        };
                    })
                    .ToList();
            }

            if (_positions.Count == 0 && _allUsers.Count > 0)
            {
                int seq = 1;
                _positions = _allUsers
                    .Where(u => !string.IsNullOrWhiteSpace(u.PositionName))
                    .GroupBy(u => u.PositionName.Trim(), StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => g.Key)
                    .Select(g => new HrPosition
                    {
                        PositionID = seq++,
                        PositionCode = "",
                        PositionName = g.Key,
                        IsActive = true
                    })
                    .ToList();
            }

            // ถ้าโหลดระดับตรงๆ ไม่ได้ แต่มีข้อมูลตำแหน่งจาก HR แล้ว
            // ให้ derive ระดับจากตำแหน่งก่อน เพื่อให้สอดคล้องกับ config ตำแหน่ง
            if (_levels.Count == 0 && _positions.Count > 0)
            {
                int seq = 1;
                _levels = _positions
                    .Where(p => !string.IsNullOrWhiteSpace(p.Level))
                    .GroupBy(p => p.Level.Trim(), StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => g.Key)
                    .Select(g => new HrPositionLevel
                    {
                        LevelID = seq++,
                        LevelCode = g.Key,
                        LevelName = g.Key,
                        IsActive = true
                    })
                    .ToList();
            }

            if (_levels.Count == 0 && _allUsers.Count > 0)
            {
                int seq = 1;
                _levels = _allUsers
                    .Where(u => !string.IsNullOrWhiteSpace(u.LevelCode))
                    .GroupBy(u => u.LevelCode.Trim(), StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => g.Key)
                    .Select(g => new HrPositionLevel
                    {
                        LevelID = seq++,
                        LevelCode = g.Key,
                        LevelName = g.Key,
                        IsActive = true
                    })
                    .ToList();
            }
        }

        private void LoadUsers()
        {
            try
            {
                _allUsers = (_userAdminController != null
                    ? _userAdminController.GetAllUsers() ?? Enumerable.Empty<UserAccount>()
                    : Enumerable.Empty<UserAccount>()).ToList();

                PopulateOrgTree();
                ApplyFilter();
                string filterLabel = tabStatus.SelectedIndex == 1 ? " (กำลังกรอง: ใช้งาน)"
                    : tabStatus.SelectedIndex == 2 ? " (กำลังกรอง: ปิดใช้งาน)" : "";
                SetStatus(string.Format("โหลดสำเร็จ: {0} ผู้ใช้{1}", _allUsers.Count, filterLabel), Color.Gray);
            }
            catch (Exception ex)
            {
                SetStatus("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        private class OrgKey
        {
            public string Division { get; set; }
            public string Department { get; set; }
            public bool IsRoot { get; set; }
            public bool IsUnspecifiedDiv { get; set; }
            public bool IsUnspecifiedDept { get; set; }
            public string BaseText { get; set; }

            public bool EqualsKey(OrgKey other)
            {
                if (other == null) return false;
                if (IsRoot) return other.IsRoot;
                if (other.IsRoot) return false;

                if (IsUnspecifiedDiv != other.IsUnspecifiedDiv) return false;
                if (!IsUnspecifiedDiv && !string.Equals(Division ?? "", other.Division ?? "", StringComparison.OrdinalIgnoreCase)) return false;

                if (Department == null && other.Department == null) return true;
                if (Department == null || other.Department == null) return false;

                if (IsUnspecifiedDept != other.IsUnspecifiedDept) return false;
                if (!IsUnspecifiedDept && !string.Equals(Department ?? "", other.Department ?? "", StringComparison.OrdinalIgnoreCase)) return false;

                return true;
            }
        }

        private void PopulateOrgTree()
        {
            OrgKey prevSelection = tvOrg.SelectedNode != null ? tvOrg.SelectedNode.Tag as OrgKey : null;
            List<OrgKey> expandedKeys = GetExpandedKeys(tvOrg.Nodes);

            tvOrg.BeginUpdate();
            tvOrg.Nodes.Clear();

            var rootKey = new OrgKey { IsRoot = true, BaseText = "ทุกฝ่าย" };
            var rootNode = new TreeNode { Tag = rootKey };
            tvOrg.Nodes.Add(rootNode);

            var divGroups = _allUsers
                .GroupBy(u => string.IsNullOrWhiteSpace(u.DivisionName) ? "" : u.DivisionName.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => string.IsNullOrEmpty(g.Key) ? 1 : 0)
                .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

            foreach (var divGroup in divGroups)
            {
                bool isUnspecDiv = string.IsNullOrEmpty(divGroup.Key);
                string divName = isUnspecDiv ? "(ไม่ระบุฝ่าย)" : divGroup.Key;

                var divKey = new OrgKey
                {
                    Division = isUnspecDiv ? null : divName,
                    IsUnspecifiedDiv = isUnspecDiv,
                    BaseText = divName
                };
                var divNode = new TreeNode { Tag = divKey };
                rootNode.Nodes.Add(divNode);

                var deptGroups = divGroup
                    .GroupBy(u => string.IsNullOrWhiteSpace(u.DepartmentName) ? "" : u.DepartmentName.Trim(), StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => string.IsNullOrEmpty(g.Key) ? 1 : 0)
                    .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

                foreach (var deptGroup in deptGroups)
                {
                    bool isUnspecDept = string.IsNullOrEmpty(deptGroup.Key);
                    string deptName = isUnspecDept ? "(ไม่ระบุแผนก)" : deptGroup.Key;

                    var deptKey = new OrgKey
                    {
                        Division = isUnspecDiv ? null : divName,
                        IsUnspecifiedDiv = isUnspecDiv,
                        Department = isUnspecDept ? null : deptName,
                        IsUnspecifiedDept = isUnspecDept,
                        BaseText = deptName
                    };
                    var deptNode = new TreeNode { Tag = deptKey };
                    divNode.Nodes.Add(deptNode);
                }
            }

            rootNode.Expand();
            RestoreExpandedNodes(tvOrg.Nodes, expandedKeys);

            TreeNode matchedNode = FindNodeByKey(tvOrg.Nodes, prevSelection);
            if (matchedNode != null)
                tvOrg.SelectedNode = matchedNode;
            else
                tvOrg.SelectedNode = rootNode;

            tvOrg.EndUpdate();
        }

        private List<OrgKey> GetExpandedKeys(TreeNodeCollection nodes)
        {
            var list = new List<OrgKey>();
            foreach (TreeNode node in nodes)
            {
                if (node.IsExpanded && node.Tag is OrgKey key && !key.IsRoot)
                {
                    list.Add(key);
                }
                if (node.Nodes.Count > 0)
                {
                    list.AddRange(GetExpandedKeys(node.Nodes));
                }
            }
            return list;
        }

        private void RestoreExpandedNodes(TreeNodeCollection nodes, List<OrgKey> expandedKeys)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is OrgKey key && expandedKeys.Any(k => k.EqualsKey(key)))
                {
                    node.Expand();
                }
                if (node.Nodes.Count > 0)
                {
                    RestoreExpandedNodes(node.Nodes, expandedKeys);
                }
            }
        }

        private TreeNode FindNodeByKey(TreeNodeCollection nodes, OrgKey targetKey)
        {
            if (targetKey == null) return null;
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is OrgKey key && key.EqualsKey(targetKey))
                {
                    return node;
                }
                if (node.Nodes.Count > 0)
                {
                    var found = FindNodeByKey(node.Nodes, targetKey);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private void ApplyFilter()
        {
            int status = tabStatus.SelectedIndex; // 0=all, 1=active, 2=inactive

            OrgKey selectedOrg = (tvOrg.SelectedNode != null && tvOrg.SelectedNode.Tag is OrgKey)
                ? (OrgKey)tvOrg.SelectedNode.Tag
                : null;

            var kwFiltered = FilterByKeyword(_allUsers).ToList();

            var orgFiltered = FilterByOrg(kwFiltered, selectedOrg).ToList();
            UpdateTabCounts(orgFiltered);

            var statusFiltered = FilterByStatus(kwFiltered, status).ToList();
            UpdateTreeCounts(statusFiltered);

            var finalList = FilterByStatus(orgFiltered, status);
            if (status == 0) finalList = finalList.OrderByDescending(u => u.IsActive);

            PopulateList(finalList.ToList());
        }

        private void UpdateTabCounts(List<UserAccount> users)
        {
            int active = users.Count(u => u.IsActive);
            tabAll.Text      = string.Format("ทั้งหมด ({0})", users.Count);
            tabActive.Text   = string.Format("✅ ใช้งาน ({0})", active);
            tabInactive.Text = string.Format("🔒 ปิดใช้งาน ({0})", users.Count - active);
        }

        private void UpdateTreeCounts(List<UserAccount> statusAndKwFiltered)
        {
            UpdateNodeCountRecursive(tvOrg.Nodes, statusAndKwFiltered);
        }

        private void UpdateNodeCountRecursive(TreeNodeCollection nodes, List<UserAccount> users)
        {
            foreach (TreeNode node in nodes)
            {
                var key = node.Tag as OrgKey;
                if (key != null)
                {
                    int count = FilterByOrg(users, key).Count();
                    node.Text = string.Format("{0} ({1})", key.BaseText, count);
                    node.ForeColor = count == 0 ? Color.Gray : Color.Black;
                }
                if (node.Nodes.Count > 0)
                {
                    UpdateNodeCountRecursive(node.Nodes, users);
                }
            }
        }

        private IEnumerable<UserAccount> FilterByKeyword(IEnumerable<UserAccount> source)
        {
            string kw = txtSearch.Text.Trim().ToLower();
            int col = cmbFilterColumn.SelectedIndex; // 0=all columns, 1=AD, 2=Display, 3=Employee, 4=Dept, 5=Div, 6=Position, 7=Role

            if (string.IsNullOrEmpty(kw)) return source;

            switch (col)
            {
                case 1: return source.Where(u => (u.ADUserName ?? "").ToLower().Contains(kw));
                case 2: return source.Where(u => (u.DisplayName ?? "").ToLower().Contains(kw));
                case 3: return source.Where(u => (u.EmployeeCode ?? "").ToLower().Contains(kw));
                case 4: return source.Where(u => (u.DepartmentName ?? "").ToLower().Contains(kw));
                case 5: return source.Where(u => (u.DivisionName ?? "").ToLower().Contains(kw));
                case 6: return source.Where(u => (u.PositionName ?? "").ToLower().Contains(kw));
                case 7: return source.Where(u => RoleName(u.AppRole).ToLower().Contains(kw));
                default:
                    return source.Where(u =>
                        (u.DisplayName ?? "").ToLower().Contains(kw) ||
                        (u.ADUserName ?? "").ToLower().Contains(kw) ||
                        (u.EmployeeCode ?? "").ToLower().Contains(kw) ||
                        (u.DepartmentName ?? "").ToLower().Contains(kw) ||
                        (u.DivisionName ?? "").ToLower().Contains(kw) ||
                        (u.PositionName ?? "").ToLower().Contains(kw) ||
                        RoleName(u.AppRole).ToLower().Contains(kw));
            }
        }

        private IEnumerable<UserAccount> FilterByStatus(IEnumerable<UserAccount> source, int status)
        {
            if (status == 1) return source.Where(u => u.IsActive);
            if (status == 2) return source.Where(u => !u.IsActive);
            return source;
        }

        private IEnumerable<UserAccount> FilterByOrg(IEnumerable<UserAccount> source, OrgKey key)
        {
            if (key == null || key.IsRoot) return source;

            if (key.IsUnspecifiedDiv)
                source = source.Where(u => string.IsNullOrWhiteSpace(u.DivisionName));
            else
                source = source.Where(u => string.Equals((u.DivisionName ?? "").Trim(), key.Division, StringComparison.OrdinalIgnoreCase));

            if (key.Department != null || key.IsUnspecifiedDept)
            {
                if (key.IsUnspecifiedDept)
                    source = source.Where(u => string.IsNullOrWhiteSpace(u.DepartmentName));
                else
                    source = source.Where(u => string.Equals((u.DepartmentName ?? "").Trim(), key.Department, StringComparison.OrdinalIgnoreCase));
            }

            return source;
        }

        private void PopulateList(List<UserAccount> users)
        {
            lvUsers.BeginUpdate();
            lvUsers.Items.Clear();
            foreach (var u in users)
            {
                var item = new ListViewItem(u.UserID.ToString());
                item.SubItems.Add(u.ADUserName ?? "");
                item.SubItems.Add(u.DisplayName ?? "");
                item.SubItems.Add(u.EmployeeCode ?? "");
                item.SubItems.Add(u.DepartmentName ?? "");
                item.SubItems.Add(u.DivisionName ?? "");
                item.SubItems.Add(u.PositionName ?? "");
                var subLevel = item.SubItems.Add(u.LevelCode ?? "");
                item.SubItems.Add(RoleName(u.AppRole));
                item.SubItems.Add(u.IsActive ? "ใช้งาน" : "ปิดใช้");
                item.SubItems.Add(u.LastLoginDate.HasValue
                    ? u.LastLoginDate.Value.ToString("dd/MM/yy HH:mm") : "—");
                item.Tag = u;
                item.ForeColor = u.IsActive ? Color.Black : Color.DimGray;

                // เน้นพื้นหลังทั้งแถวเมื่อ Level มีค่า — สื่อว่า "ระบุ Level แล้ว พร้อมใช้งาน"
                if (!string.IsNullOrWhiteSpace(u.LevelCode))
                {
                    item.BackColor = u.IsActive
                        ? Color.FromArgb(200, 235, 210)   // เขียวอ่อน = active + มี Level
                        : Color.FromArgb(225, 235, 225);  // เขียวจางลง เมื่อ inactive
                    subLevel.Font = boldLevelFont;
                }
                lvUsers.Items.Add(item);
            }
            if (_sortColumn != -1) lvUsers.Sort();   // คงลำดับ sort เดิมไว้หลัง filter/refresh ใหม่
            lvUsers.EndUpdate();

            // แสดง status พร้อมจำนวนรายการ
            if (users.Count == _allUsers.Count)
                SetStatus(string.Format("แสดงทั้งหมด {0} รายการ", users.Count), Color.Gray);
            else
                SetStatus(string.Format("แสดง {0} จาก {1} รายการ (กรองแล้ว)", users.Count, _allUsers.Count), Color.FromArgb(25, 55, 105));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event Handlers
        // ─────────────────────────────────────────────────────────────────────

        private void LvUsers_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            else
            {
                _sortColumn = e.Column;
                _sortOrder = SortOrder.Ascending;
            }

            lvUsers.ListViewItemSorter = new ListViewItemComparer(_sortColumn, _sortOrder);
            lvUsers.Sort();
        }

        // เรียงตามข้อความในคอลัมน์ที่คลิก — ลอง parse ตัวเลขก่อน (เช่น ID) ไม่ได้ค่อย fallback เรียงตัวอักษร
        private class ListViewItemComparer : System.Collections.IComparer
        {
            private readonly int _col;
            private readonly SortOrder _order;

            public ListViewItemComparer(int col, SortOrder order)
            {
                _col = col;
                _order = order;
            }

            public int Compare(object x, object y)
            {
                var itemX = (ListViewItem)x;
                var itemY = (ListViewItem)y;
                string textX = itemX.SubItems.Count > _col ? itemX.SubItems[_col].Text : "";
                string textY = itemY.SubItems.Count > _col ? itemY.SubItems[_col].Text : "";

                int result;
                double numX, numY;
                if (double.TryParse(textX, out numX) && double.TryParse(textY, out numY))
                    result = numX.CompareTo(numY);
                else
                    result = string.Compare(textX, textY, StringComparison.CurrentCultureIgnoreCase);

                return _order == SortOrder.Descending ? -result : result;
            }
        }

        private void LvUsers_SelectionChanged(object sender, EventArgs e)
        {
            int count = lvUsers.SelectedItems.Count;
            btnEdit.Enabled = count == 1;
            btnToggle.Enabled = count > 0;

            if (count == 1)
            {
                var u = lvUsers.SelectedItems[0].Tag as UserAccount;
                btnToggle.Text = (u != null && u.IsActive) ? "🔒 ปิดใช้งาน" : "🔓 เปิดใช้งาน";

                if (u != null)
                {
                    string selectionInfo = string.Format("เลือก: {0} ({1}) - {2}",
                        u.DisplayName ?? u.ADUserName,
                        u.EmployeeCode ?? "ไม่มีรหัส",
                        u.OrgDisplay ?? "ไม่ระบุหน่วยงาน");
                    SetStatus(selectionInfo, Color.FromArgb(25, 55, 105));
                }
            }
            else if (count > 1)
            {
                btnToggle.Text = "🔄 สลับสถานะ";
                SetStatus(string.Format("เลือก {0} รายการ", count), Color.FromArgb(25, 55, 105));
            }
            else
            {
                btnToggle.Text = "🔒 ปิดใช้งาน";
                SetStatus(string.Format("ทั้งหมด {0} รายการ", lvUsers.Items.Count), Color.Gray);
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            using (var dlg = BuildEditDialog(null))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                LoadUsers();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (lvUsers.SelectedItems.Count == 0) return;
            var user = lvUsers.SelectedItems[0].Tag as UserAccount;
            if (user == null) return;

            using (var dlg = BuildEditDialog(user))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                LoadUsers();
            }
        }

        private void BtnToggle_Click(object sender, EventArgs e)
        {
            var users = lvUsers.SelectedItems.Cast<ListViewItem>()
                .Select(i => i.Tag as UserAccount)
                .Where(u => u != null)
                .ToList();
            if (users.Count == 0) return;

            string confirmMsg = users.Count == 1
                ? string.Format("{0} ผู้ใช้ '{1}' ใช่หรือไม่?",
                    users[0].IsActive ? "ปิดใช้งาน" : "เปิดใช้งาน",
                    users[0].DisplayName ?? users[0].ADUserName)
                : string.Format("สลับสถานะผู้ใช้ที่เลือก {0} รายการ ใช่หรือไม่?", users.Count);

            if (MessageBox.Show(confirmMsg, "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;

            int okCount = 0;
            var errors = new List<string>();

            foreach (var user in users)
            {
                try
                {
                    bool wasActive = user.IsActive;
                    if (user.IsActive)
                        _userAdminController.DeactivateUser(user.UserID);
                    else
                    {
                        user.IsActive = true;
                        _userAdminController.UpdateUser(user);
                    }
                    _auditLogController.Log(new AuditEntry
                    {
                        Module = "UserManagement",
                        Action = wasActive ? "DEACTIVATE_USER" : "ACTIVATE_USER",
                        UserID = _currentUser.UserID,
                        TableName = "UserAccount",
                        RecordID = user.UserID,
                        NewValue = string.Format("User:{0} IsActive:{1}", user.ADUserName, !wasActive),
                        MachineName = _currentUser.MachineName,
                        IPAddress = _currentUser.IPAddress,
                        SessionID = _currentUser.SessionID,
                        Result = 1,
                        AuditDate = DateTime.Now
                    });
                    okCount++;
                }
                catch (Exception ex)
                {
                    errors.Add((user.DisplayName ?? user.ADUserName) + ": " + ex.Message);
                }
            }

            LoadUsers();

            if (errors.Count == 0)
                SetStatus(string.Format("เปลี่ยนสถานะสำเร็จ {0} รายการ", okCount), Color.DarkGreen);
            else
                SetStatus(string.Format("สำเร็จ {0} รายการ, ล้มเหลว {1} รายการ: {2}",
                    okCount, errors.Count, string.Join("; ", errors)), Color.Red);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Edit / Create dialog
        // ─────────────────────────────────────────────────────────────────────

        private Form BuildEditDialog(UserAccount user)
        {
            bool isNew = user == null;
            if (isNew) user = new UserAccount { IsActive = true, AppRole = 1 };

            // ── Palette ────────────────────────────────────────────────────
            var navyDark = Color.FromArgb(25, 55, 105);
            var navyMid = Color.FromArgb(50, 90, 150);
            var navyLight = Color.FromArgb(215, 228, 248);
            var bodyBg = Color.FromArgb(243, 246, 251);
            var footerBg = Color.FromArgb(228, 234, 245);
            var savedBg = Color.FromArgb(232, 245, 233);   // สีเขียวอ่อน = ข้อมูลที่บันทึกแล้ว
            var savedBdr = Color.FromArgb(160, 210, 170);
            var lblColor = Color.FromArgb(55, 65, 90);
            var savedColor = Color.FromArgb(30, 110, 50);
            var uiFont = new Font("Segoe UI", 10.5f);
            var boldFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            var lblFont = new Font("Segoe UI", 10f);
            var smallFont = new Font("Segoe UI", 9f);

            var dlg = new Form
            {
                Text = isNew ? "เพิ่มผู้ใช้ใหม่" : "แก้ไขข้อมูลผู้ใช้",
                Size = new Size(600, 760),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = bodyBg,
                Font = uiFont,
            };
            dlg.AutoScaleMode = AutoScaleMode.Font;

            // ── Header ──────────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = navyDark };
            var pnlIcon = new Panel { Size = new Size(36, 36), Location = new Point(14, 11), BackColor = Color.Transparent };
            pnlIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(navyMid), 0, 0, 35, 35);
                using (var f = new Font("Segoe UI", 17f))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(isNew ? "+" : "✎", f, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
            };
            var lblTitle = new Label
            {
                Text = isNew ? "เพิ่มผู้ใช้ใหม่" : "แก้ไขข้อมูลผู้ใช้",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(58, 16),
            };
            pnlHeader.Controls.Add(pnlIcon);
            pnlHeader.Controls.Add(lblTitle);

            // ── Body (scrollable) ────────────────────────────────────────────
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,   // เติมพื้นที่ระหว่าง Header และ Footer อัตโนมัติ
                BackColor = bodyBg,
                AutoScroll = true,
                Padding = new Padding(0, 12, 0, 0),
            };

            int lw = 136, fw = 360, lx = 14, tx = 158;
            int inputH = 30, rowGap = 10, rowH = inputH + rowGap;
            int y = 26;

            Action<string, int> addLbl = (text, yy) =>
                pnlBody.Controls.Add(new Label
                {
                    Text = text,
                    Left = lx,
                    Top = yy + 5,
                    Width = lw,
                    Font = lblFont,
                    ForeColor = lblColor,
                    TextAlign = ContentAlignment.MiddleRight,
                });

            Func<string, int, TextBox> addTxt = (val, yy) =>
            {
                var t = new TextBox
                {
                    Left = tx,
                    Top = yy,
                    Width = fw,
                    Height = inputH,
                    Text = val,
                    Font = uiFont,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                };
                pnlBody.Controls.Add(t);
                return t;
            };

            Func<int, ComboBox> addCmb = (yy) =>
            {
                var c = new ComboBox
                {
                    Left = tx,
                    Top = yy,
                    Width = fw,
                    Height = inputH,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = uiFont,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                };
                pnlBody.Controls.Add(c);
                return c;
            };

            // ── helper: section bar ──────────────────────────────────────────
            Action<string, int> addSectionBar = (title, yy) =>
            {
                var bar = new Panel { Left = lx, Top = yy, Width = lw + fw + 10, Height = 24, BackColor = navyLight };
                bar.Controls.Add(new Label
                {
                    Text = "  " + title,
                    Font = boldFont,
                    ForeColor = navyDark,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                });
                pnlBody.Controls.Add(bar);
            };

            // ── helper: สร้าง "กล่องข้อมูลที่บันทึกแล้ว" ────────────────────────
            // แสดงค่าที่บันทึกอยู่ในระบบ (read-only) เพื่อให้ HR เห็นสถานะปัจจุบัน
            Func<string, int, Label> addSavedValue = (val, yy) =>
            {
                bool hasValue = !string.IsNullOrWhiteSpace(val);
                var lbl = new Label
                {
                    Text = hasValue ? val : "(ยังไม่ได้ระบุ)",
                    Left = tx,
                    Top = yy,
                    Width = fw,
                    Height = 22,
                    Font = hasValue ? new Font("Segoe UI", 10f, FontStyle.Bold) : smallFont,
                    ForeColor = hasValue ? savedColor : Color.DarkGray,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 0, 0, 0),
                };
                pnlBody.Controls.Add(lbl);
                return lbl;
            };

            // ══════════════════════════════════════════════════════════════════
            // SECTION 1: ข้อมูลพื้นฐาน
            // ══════════════════════════════════════════════════════════════════
            addSectionBar("ข้อมูลพื้นฐาน", y); y += 34;

            addLbl("ชื่อผู้ใช้ AD:", y);
            TextBox txtAD;
            Button btnAdSearch = null;
            bool showAdSearch = isNew && _ldapUserSearchService != null;
            if (showAdSearch)
            {
                txtAD = new TextBox
                {
                    Left = tx,
                    Top = y,
                    Width = fw - 96,
                    Height = inputH,
                    Text = user.ADUserName ?? "",
                    Font = uiFont,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                };
                pnlBody.Controls.Add(txtAD);

                btnAdSearch = new Button
                {
                    Left = tx + fw - 92,
                    Top = y,
                    Width = 92,
                    Height = inputH,
                    Text = "🔍 ค้นหา AD",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand,
                };
                btnAdSearch.FlatAppearance.BorderSize = 0;
                pnlBody.Controls.Add(btnAdSearch);
            }
            else
            {
                txtAD = addTxt(user.ADUserName ?? "", y);
                if (!isNew) txtAD.ReadOnly = true;  // AD username ไม่ควรเปลี่ยนหลังสร้าง
            }
            y += rowH;

            addLbl("ชื่อ-นามสกุล:", y);
            var txtDisplay = addTxt(user.DisplayName ?? "", y);
            y += rowH;

            addLbl("รหัสพนักงาน:", y);
            var txtEmpCode = addTxt(user.EmployeeCode ?? "", y);
            y += rowH;

            if (showAdSearch)
            {
                btnAdSearch.Click += (s, e) =>
                {
                    using (var adSearchDlg = new CMS.Presentation.Modules.Workflow.DelegateSearchDialog(
                        null, _ldapUserSearchService, null, _currentUser.ADUserName))
                    {
                        if (adSearchDlg.ShowDialog(this) == DialogResult.OK && adSearchDlg.SelectedUser != null)
                        {
                            txtAD.Text = adSearchDlg.SelectedUser.SamAccountName;
                            if (string.IsNullOrWhiteSpace(txtDisplay.Text))
                                txtDisplay.Text = adSearchDlg.SelectedUser.DisplayName;
                            if (string.IsNullOrWhiteSpace(txtEmpCode.Text))
                                txtEmpCode.Text = adSearchDlg.SelectedUser.EmployeeId;
                        }
                    }
                };
            }

            addLbl("บทบาท:", y);
            var cmbRole = addCmb(y);
            cmbRole.Items.AddRange(new object[] { "Officer (1)", "SrOfficer (2)", "Manager (3)", "Admin (4)" });
            cmbRole.SelectedIndex = Math.Max(0, Math.Min(3, user.AppRole - 1));
            y += rowH + 10;

            // ══════════════════════════════════════════════════════════════════
            // SECTION 2: ข้อมูลองค์กร — แสดงค่าปัจจุบันก่อน แล้วค่อยให้แก้ไข
            // ══════════════════════════════════════════════════════════════════
            addSectionBar("ข้อมูลองค์กร", y); y += 34;

            // ── กล่อง "ข้อมูลที่บันทึกไว้ในระบบ" ──────────────────────────────
            if (!isNew)
            {
                bool hasOrgData = !string.IsNullOrWhiteSpace(user.DivisionName)
                               || !string.IsNullOrWhiteSpace(user.DepartmentName)
                               || !string.IsNullOrWhiteSpace(user.PositionName)
                               || !string.IsNullOrWhiteSpace(user.LevelCode);

                var pnlSaved = new Panel
                {
                    Left = lx,
                    Top = y,
                    Width = lw + fw + 10,
                    Height = hasOrgData ? 84 : 36,
                    BackColor = savedBg,
                    BorderStyle = BorderStyle.FixedSingle,
                };

                var lblSavedTitle = new Label
                {
                    Text = hasOrgData
                        ? "✓ ข้อมูลองค์กรที่บันทึกไว้ในระบบ:"
                        : "ℹ️  ยังไม่มีข้อมูลองค์กรบันทึกไว้ในระบบ",
                    Left = 6,
                    Top = 4,
                    Width = lw + fw - 4,
                    Height = 20,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = hasOrgData ? savedColor : Color.Gray,
                    AutoSize = false,
                };
                pnlSaved.Controls.Add(lblSavedTitle);

                if (hasOrgData)
                {
                    string orgLine1 = string.Format("ฝ่าย: {0}   แผนก: {1}",
                        string.IsNullOrWhiteSpace(user.DivisionName) ? "(ไม่ระบุ)" : user.DivisionName,
                        string.IsNullOrWhiteSpace(user.DepartmentName) ? "(ไม่ระบุ)" : user.DepartmentName);
                    string orgLine2 = string.Format("ตำแหน่ง: {0}   Level: {1}",
                        string.IsNullOrWhiteSpace(user.PositionName) ? "(ไม่ระบุ)" : user.PositionName,
                        string.IsNullOrWhiteSpace(user.LevelCode) ? "(ไม่ระบุ)" : user.LevelCode);

                    pnlSaved.Controls.Add(new Label
                    {
                        Text = orgLine1,
                        Left = 8,
                        Top = 26,
                        Width = lw + fw - 10,
                        Height = 20,
                        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                        ForeColor = savedColor,
                        AutoSize = false,
                    });
                    pnlSaved.Controls.Add(new Label
                    {
                        Text = orgLine2,
                        Left = 8,
                        Top = 48,
                        Width = lw + fw - 10,
                        Height = 20,
                        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                        ForeColor = savedColor,
                        AutoSize = false,
                    });
                }
                pnlBody.Controls.Add(pnlSaved);
                y += pnlSaved.Height + 10;

                // คำอธิบายสำหรับ HR
                var lblHint = new Label
                {
                    Text = "หากต้องการอัปเดตข้อมูลองค์กร เลือกค่าใหม่จาก dropdown ด้านล่าง แล้วกด \"บันทึก\"",
                    Left = lx,
                    Top = y,
                    Width = lw + fw + 10,
                    Height = 32,
                    Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                    ForeColor = Color.FromArgb(80, 80, 120),
                    AutoSize = false,
                };
                pnlBody.Controls.Add(lblHint);
                y += 36;
            }

            // ── Dropdowns แก้ไของค์กร ─────────────────────────────────────────
            addLbl("ฝ่าย:", y);
            var cmbDiv = addCmb(y);
            cmbDiv.Items.Add(new HrDivision { DivisionCode = "", DivisionName = "-- ไม่เปลี่ยน / ไม่ระบุ --" });
            foreach (var d in _divisions) cmbDiv.Items.Add(d);
            cmbDiv.DisplayMember = "DivisionName";
            cmbDiv.SelectedIndex = 0;
            SelectDivision(cmbDiv, user);

            // ถ้า pre-select ไม่เจอใน master list ให้เพิ่มค่าปัจจุบันของผู้ใช้เข้าไปชั่วคราว
            // เพื่อให้เห็นข้อมูลเดิมและแก้ไขต่อได้ (เหมือนแผนก/ตำแหน่ง)
            if (cmbDiv.SelectedIndex == 0 &&
                (!string.IsNullOrWhiteSpace(user.DivisionName) ||
                 !string.IsNullOrWhiteSpace(user.DivisionCode)))
            {
                cmbDiv.Items.Add(new HrDivision
                {
                    DivisionID = user.DivisionId.GetValueOrDefault(),
                    DivisionCode = user.DivisionCode ?? string.Empty,
                    DivisionName = !string.IsNullOrWhiteSpace(user.DivisionName) ? user.DivisionName : user.DivisionCode,
                    IsActive = true
                });
                cmbDiv.SelectedIndex = cmbDiv.Items.Count - 1;
            }
            y += rowH;

            addLbl("แผนก:", y);
            var cmbDept = addCmb(y);
            cmbDept.DisplayMember = "DisplayName";   // ใช้ DisplayName (รองรับ DepartmentNameTh)
            y += rowH;

            // autoSelect=true → pre-select user's saved dept (initial load only)
            // autoSelect=false → just filter by div, leave at placeholder (when user changes ฝ่าย)
            Action<bool> populateDept = (autoSelect) =>
            {
                cmbDept.Items.Clear();
                cmbDept.Items.Add(new HrDepartment
                {
                    DepartmentCode = "",
                    DepartmentName = "-- ไม่เปลี่ยน / ไม่ระบุ --",
                    DepartmentNameTh = "-- ไม่เปลี่ยน / ไม่ระบุ --"
                });

                var selDiv = cmbDiv.SelectedItem as HrDivision;
                var depts = (selDiv != null && selDiv.DivisionID > 0)
                    ? _departments.Where(d => d.DivisionID == selDiv.DivisionID).ToList()
                    : _departments.ToList();

                // บาง environment มีข้อมูล Department ที่ไม่ผูก DivisionID ครบ
                // ถ้ากรองด้วย DivisionID แล้วไม่เจอรายการ ให้ fallback เป็นรายการทั้งหมด
                // เพื่อไม่ให้ dropdown แผนกกลายเป็นว่าง
                if (selDiv != null && selDiv.DivisionID > 0 && depts.Count == 0)
                    depts = _departments.ToList();

                foreach (var d in depts) cmbDept.Items.Add(d);
                cmbDept.SelectedIndex = 0;

                if (autoSelect)
                {
                    SelectDepartment(cmbDept, user);

                    // ถ้า pre-select ไม่เจอใน master list ให้เพิ่มค่าปัจจุบันของผู้ใช้เข้าไปชั่วคราว
                    // เพื่อให้เห็นข้อมูลเดิมและแก้ไขต่อได้
                    if (cmbDept.SelectedIndex == 0 &&
                        (!string.IsNullOrWhiteSpace(user.DepartmentName) ||
                         !string.IsNullOrWhiteSpace(user.DepartmentCode)))
                    {
                        var currentDept = new HrDepartment
                        {
                            DepartmentID = user.DepartmentId.GetValueOrDefault(),
                            DepartmentCode = user.DepartmentCode ?? string.Empty,
                            DepartmentName = user.DepartmentName ?? string.Empty,
                            DepartmentNameTh = user.DepartmentName ?? string.Empty,
                            DivisionID = user.DivisionId.GetValueOrDefault(),
                            IsActive = true
                        };

                        cmbDept.Items.Add(currentDept);
                        cmbDept.SelectedIndex = cmbDept.Items.Count - 1;
                    }
                }
            };

            populateDept(true);   // initial load — pre-select user's saved dept
            // เมื่อ user เปลี่ยน ฝ่าย เอง → แค่ filter แผนก ไม่ override ให้เลือกเอง
            cmbDiv.SelectedIndexChanged += (s, ev) => populateDept(false);

            addLbl("ตำแหน่ง:", y);
            var cmbPos = addCmb(y);
            cmbPos.DisplayMember = "PositionName";
            y += rowH;

            // ตำแหน่งกรองตามแผนกที่เลือก (ผ่าน m_PositionMapping): ฝ่าย → แผนก → ตำแหน่ง
            // autoSelect=true → pre-select ตำแหน่งเดิมของ user (initial load only)
            // autoSelect=false → แค่กรองตามแผนก ไม่ override ให้เลือกเอง
            Action<bool> populatePos = (autoSelect) =>
            {
                cmbPos.Items.Clear();
                cmbPos.Items.Add(new HrPosition { PositionCode = "", PositionName = "-- ไม่เปลี่ยน / ไม่ระบุ --" });

                var selDept = cmbDept.SelectedItem as HrDepartment;
                List<HrPosition> posList = null;
                if (_positionMappings.Count > 0 && selDept != null && selDept.DepartmentID > 0)
                {
                    posList = _positionMappings
                        .Where(m => m.DepartmentID == selDept.DepartmentID)
                        .GroupBy(m => m.PositionID)
                        .Select(g => new HrPosition
                        {
                            PositionID = g.Key,
                            PositionCode = g.First().PositionCode,
                            PositionName = g.First().PositionName,
                            LevelCode = g.First().Level
                        })
                        .OrderBy(p => p.PositionName)
                        .ToList();
                }

                // ไม่มี mapping data หรือยังไม่เลือกแผนก → fallback แสดงตำแหน่งทั้งหมด
                if (posList == null || posList.Count == 0)
                    posList = _positions.ToList();

                foreach (var p in posList) cmbPos.Items.Add(p);
                cmbPos.SelectedIndex = 0;

                if (autoSelect)
                {
                    SelectPosition(cmbPos, user);

                    // ถ้า pre-select ไม่เจอในรายการที่กรองแล้ว ให้เพิ่มค่าปัจจุบันของผู้ใช้เข้าไปชั่วคราว
                    if (cmbPos.SelectedIndex == 0 && !string.IsNullOrWhiteSpace(user.PositionName))
                    {
                        var currentPos = new HrPosition
                        {
                            PositionCode = "",
                            PositionName = user.PositionName,
                            LevelCode = user.LevelCode
                        };
                        cmbPos.Items.Add(currentPos);
                        cmbPos.SelectedIndex = cmbPos.Items.Count - 1;
                    }
                }
            };

            populatePos(true);   // initial load — pre-select ตำแหน่งเดิมของ user
            cmbDept.SelectedIndexChanged += (s, ev) => populatePos(false);

            addLbl("Level:", y);
            var cmbLevel = addCmb(y);
            cmbLevel.Items.Add(new HrPositionLevel { LevelCode = "", LevelName = "-- ไม่เปลี่ยน / ไม่ระบุ --" });
            foreach (var lv in _levels) cmbLevel.Items.Add(lv);
            // ใช้ Format event แทน DisplayMember เพื่อให้ placeholder แสดงข้อความแทน blank
            cmbLevel.FormattingEnabled = true;
            cmbLevel.Format += (s, e) =>
            {
                var lv = e.ListItem as HrPositionLevel;
                e.Value = (lv == null || string.IsNullOrEmpty(lv.LevelCode))
                    ? "-- ไม่เปลี่ยน / ไม่ระบุ --"
                    : lv.LevelCode;
            };
            cmbLevel.SelectedIndex = 0;
            SelectLevel(cmbLevel, user);

            // เมื่อเลือกตำแหน่ง → auto-fill Level ถ้าตำแหน่งมีค่า Level กำกับไว้
            cmbPos.SelectedIndexChanged += (s, ev) =>
            {
                var pos = cmbPos.SelectedItem as HrPosition;
                if (pos != null && pos.PositionID > 0 && !string.IsNullOrEmpty(pos.Level))
                {
                    EnsureLevelOptionExists(cmbLevel, pos.Level);
                    SelectLevelByCode(cmbLevel, pos.Level);
                }
            };

            // ── Footer ──────────────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = footerBg };

            var btnOk = new Button
            {
                Text = "บันทึก",
                Size = new Size(120, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = navyDark,
                ForeColor = Color.White,
                Font = boldFont,
                DialogResult = DialogResult.OK,
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCan = new Button
            {
                Text = "ยกเลิก",
                Size = new Size(90, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 228, 245),
                ForeColor = navyDark,
                Font = boldFont,
                DialogResult = DialogResult.Cancel,
            };
            btnCan.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
            btnCan.FlatAppearance.BorderSize = 1;

            pnlFooter.Resize += (s, e) =>
            {
                btnOk.Location = new Point(pnlFooter.Width - btnOk.Width - btnCan.Width - 20, 9);
                btnCan.Location = new Point(pnlFooter.Width - btnCan.Width - 10, 9);
            };
            btnOk.Location = new Point(520 - btnOk.Width - btnCan.Width - 20, 9);
            btnCan.Location = new Point(520 - btnCan.Width - 10, 9);

            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCan;
            pnlFooter.Controls.Add(btnOk);
            pnlFooter.Controls.Add(btnCan);

            // ลำดับสำคัญ: Top/Bottom ก่อน แล้วค่อย Fill เพื่อให้ WinForms แบ่งพื้นที่ถูก
            dlg.Controls.Add(pnlHeader);   // DockStyle.Top
            dlg.Controls.Add(pnlFooter);   // DockStyle.Bottom
            dlg.Controls.Add(pnlBody);     // DockStyle.Fill (เติมส่วนที่เหลือ)

            // ── Save logic ──────────────────────────────────────────────────
            btnOk.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtAD.Text))
                {
                    MessageBox.Show("กรุณาระบุชื่อผู้ใช้ AD", "แจ้งเตือน",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dlg.DialogResult = DialogResult.None;
                    return;
                }

                user.ADUserName = txtAD.Text.Trim();
                user.DisplayName = txtDisplay.Text.Trim();
                user.EmployeeCode = txtEmpCode.Text.Trim();
                user.AppRole = cmbRole.SelectedIndex + 1;

                var selPos = cmbPos.SelectedItem as HrPosition;
                var selLevel = cmbLevel.SelectedItem as HrPositionLevel;

                // อัปเดต Position/Level เฉพาะเมื่อเลือกค่าจริง (ไม่ใช่ "-- ไม่เปลี่ยน --")
                if (selPos != null && selPos.PositionID > 0)
                    user.PositionName = selPos.PositionName;
                // ถ้าเลือก "-- ไม่เปลี่ยน --" ให้คง PositionName เดิมไว้

                if (selLevel != null && !string.IsNullOrEmpty(selLevel.LevelCode))
                    user.LevelCode = selLevel.LevelCode;

                var selDiv = cmbDiv.SelectedItem as HrDivision;
                var selDept = cmbDept.SelectedItem as HrDepartment;

                if (selDiv != null && selDiv.DivisionID > 0)
                {
                    user.DivisionId = selDiv.DivisionID;
                    user.DivisionCode = selDiv.DivisionCode;
                    user.DivisionName = selDiv.DivisionName;
                }
                // ถ้าไม่ได้เลือก div → คง DivisionName เดิม (ไม่ clear)

                if (selDept != null && selDept.DepartmentID > 0)
                {
                    user.DepartmentId = selDept.DepartmentID;
                    user.DepartmentCode = selDept.DepartmentCode;
                    // เก็บทั้ง Name และ NameTh
                    user.DepartmentName = !string.IsNullOrEmpty(selDept.DepartmentNameTh)
                        ? selDept.DepartmentNameTh
                        : selDept.DepartmentName;
                }

                try
                {
                    if (isNew) _userAdminController.CreateUser(user);
                    else _userAdminController.UpdateUser(user);

                    _auditLogController.Log(new AuditEntry
                    {
                        Module = "UserManagement",
                        Action = isNew ? "CREATE_USER" : "EDIT_USER",
                        UserID = _currentUser.UserID,
                        TableName = "UserAccount",
                        RecordID = user.UserID,
                        NewValue = string.Format("AD:{0} Role:{1} Div:{2} Dept:{3} Pos:{4} Level:{5}",
                                         user.ADUserName, user.AppRole,
                                         user.DivisionCode, user.DepartmentCode,
                                         user.PositionName, user.LevelCode),
                        MachineName = _currentUser.MachineName,
                        IPAddress = _currentUser.IPAddress,
                        SessionID = _currentUser.SessionID,
                        Result = 1,
                        AuditDate = DateTime.Now
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("บันทึกไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dlg.DialogResult = DialogResult.None;
                }
            };

            return dlg;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Org selection helpers — ใช้ fallback หลายชั้น + case-insensitive
        // ─────────────────────────────────────────────────────────────────────

        private static void SelectDivision(ComboBox cmb, UserAccount user)
        {
            // ลำดับ: DivisionId → DivisionCode → DivisionName (case-insensitive)
            for (int i = 1; i < cmb.Items.Count; i++)
            {
                var div = cmb.Items[i] as HrDivision;
                if (div == null) continue;
                if (user.DivisionId.HasValue && user.DivisionId.Value > 0 && div.DivisionID == user.DivisionId.Value)
                { cmb.SelectedIndex = i; return; }
            }
            if (!string.IsNullOrEmpty(user.DivisionCode))
            {
                for (int i = 1; i < cmb.Items.Count; i++)
                {
                    var div = cmb.Items[i] as HrDivision;
                    if (div != null && string.Equals(div.DivisionCode, user.DivisionCode, StringComparison.OrdinalIgnoreCase))
                    { cmb.SelectedIndex = i; return; }
                }
            }
            if (!string.IsNullOrEmpty(user.DivisionName))
            {
                string target = user.DivisionName.Trim();
                for (int i = 1; i < cmb.Items.Count; i++)
                {
                    var div = cmb.Items[i] as HrDivision;
                    if (div != null && string.Equals((div.DivisionName ?? "").Trim(), target, StringComparison.OrdinalIgnoreCase))
                    { cmb.SelectedIndex = i; return; }
                }
            }
        }

        private static void SelectDepartment(ComboBox cmb, UserAccount user)
        {
            // ลำดับ: DepartmentId → DepartmentCode → DisplayName/DepartmentNameTh/DepartmentName (case-insensitive)
            for (int i = 1; i < cmb.Items.Count; i++)
            {
                var dept = cmb.Items[i] as HrDepartment;
                if (dept == null) continue;
                if (user.DepartmentId.HasValue && user.DepartmentId.Value > 0 && dept.DepartmentID == user.DepartmentId.Value)
                { cmb.SelectedIndex = i; return; }
            }
            if (!string.IsNullOrEmpty(user.DepartmentCode))
            {
                for (int i = 1; i < cmb.Items.Count; i++)
                {
                    var dept = cmb.Items[i] as HrDepartment;
                    if (dept != null && string.Equals(dept.DepartmentCode, user.DepartmentCode, StringComparison.OrdinalIgnoreCase))
                    { cmb.SelectedIndex = i; return; }
                }
            }
            if (!string.IsNullOrEmpty(user.DepartmentName))
            {
                string target = user.DepartmentName.Trim();
                for (int i = 1; i < cmb.Items.Count; i++)
                {
                    var dept = cmb.Items[i] as HrDepartment;
                    if (dept == null) continue;
                    // ตรวจสอบ DisplayName, DepartmentNameTh, DepartmentName ตามลำดับ
                    if (string.Equals((dept.DisplayName ?? "").Trim(), target, StringComparison.OrdinalIgnoreCase)
                     || string.Equals((dept.DepartmentNameTh ?? "").Trim(), target, StringComparison.OrdinalIgnoreCase)
                     || string.Equals((dept.DepartmentName ?? "").Trim(), target, StringComparison.OrdinalIgnoreCase))
                    { cmb.SelectedIndex = i; return; }
                }
            }
        }

        private static void SelectPosition(ComboBox cmb, UserAccount user)
        {
            if (string.IsNullOrEmpty(user.PositionName)) return;
            string target = user.PositionName.Trim();
            for (int i = 1; i < cmb.Items.Count; i++)
            {
                var pos = cmb.Items[i] as HrPosition;
                if (pos != null && string.Equals((pos.PositionName ?? "").Trim(), target, StringComparison.OrdinalIgnoreCase))
                { cmb.SelectedIndex = i; return; }
            }
        }

        private static void SelectLevel(ComboBox cmb, UserAccount user)
        {
            if (string.IsNullOrEmpty(user.LevelCode)) return;
            SelectLevelByCode(cmb, user.LevelCode);
        }

        private static void SelectLevelByCode(ComboBox cmb, string code)
        {
            if (string.IsNullOrEmpty(code)) return;
            for (int i = 1; i < cmb.Items.Count; i++)
            {
                var lv = cmb.Items[i] as HrPositionLevel;
                if (lv != null && string.Equals(lv.LevelCode, code, StringComparison.OrdinalIgnoreCase))
                { cmb.SelectedIndex = i; return; }
            }
        }

        private static void EnsureLevelOptionExists(ComboBox cmb, string code)
        {
            if (cmb == null || string.IsNullOrWhiteSpace(code)) return;

            for (int i = 1; i < cmb.Items.Count; i++)
            {
                var lv = cmb.Items[i] as HrPositionLevel;
                if (lv != null && string.Equals(lv.LevelCode, code, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            cmb.Items.Add(new HrPositionLevel
            {
                LevelCode = code.Trim(),
                LevelName = code.Trim(),
                IsActive = true
            });
        }

        private static TextBox AddField(Form dlg, string label, int lx, int lw, int tx, int fw, ref int y, string value)
        {
            dlg.Controls.Add(new Label { Text = label, Left = lx, Top = y + 3, Width = lw, TextAlign = ContentAlignment.MiddleRight });
            var txt = new TextBox { Left = tx, Top = y, Width = fw, Text = value };
            dlg.Controls.Add(txt);
            y += 34;
            return txt;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Utility
        // ─────────────────────────────────────────────────────────────────────

        private static string RoleName(int appRole)
        {
            switch (appRole)
            {
                case 1: return "Officer";
                case 2: return "SrOfficer";
                case 3: return "Manager";
                case 4: return "Admin";
                default: return appRole.ToString();
            }
        }

        private void SetStatus(string text, Color color)
        {
            lblStatusBar.Text = text;
            lblStatusBar.ForeColor = color;
        }
    }
}
