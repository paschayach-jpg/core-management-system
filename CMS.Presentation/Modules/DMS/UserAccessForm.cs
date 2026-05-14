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
    /// Admin form for managing user access rights.
    /// Tab 1 (สิทธิ์ผู้ใช้): Change user role, deactivate user.
    /// Tab 2 (หน่วยงานผู้ใช้): View org hierarchy and reassign a user's org unit.
    /// Tab 3 (สิทธิ์มองเห็นหน่วยงาน): Admin-only — manage per-user OrgUnit visibility grants.
    /// </summary>
    public partial class UserAccessForm : Form
    {
        private readonly IUserAdminController _userAdminController;
        private readonly IUserAccessOrgLookupController _orgLookupController;
        private readonly UserContext _currentUser;
        private readonly IUserOrgVisibilityController _visibilityController;
        private readonly IAuditLogController _auditLogController;

        private List<UserAccount> _allUsers = new List<UserAccount>();
        private List<OrgUnit> _allOrgs = new List<OrgUnit>();

        // Tab 3 controls (created programmatically)
        private TabPage _tabVisibility;
        private ListView _lvVisibility;
        private Button _btnGrantVisibility;
        private Button _btnRevokeVisibility;
        private Label _lblVisibilityStatus;
        private Label _lblVisibilityUser;

        public UserAccessForm(
            IUserAdminController userAdminController,
            IUserAccessOrgLookupController orgLookupController,
            UserContext currentUser,
            IUserOrgVisibilityController visibilityController = null,
            IAuditLogController auditLogController = null)
        {
            _userAdminController = userAdminController;
            _orgLookupController = orgLookupController;
            _currentUser = currentUser;
            _visibilityController = visibilityController;
            _auditLogController = auditLogController;

            InitializeComponent();
            InitializeVisibilityTab();
            WireEvents();
            LoadData();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tab 3 — Visibility (created programmatically)
        // ─────────────────────────────────────────────────────────────────────

        private void InitializeVisibilityTab()
        {
            bool isAdmin = _currentUser != null && _currentUser.AppRole == 4;

            _tabVisibility = new TabPage("  สิทธิ์มองเห็นหน่วยงาน  ");
            _tabVisibility.Padding = new Padding(8);
            _tabVisibility.UseVisualStyleBackColor = true;

            // Header label showing which user's grants are being viewed
            _lblVisibilityUser = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "เลือกผู้ใช้จากแท็บ 'สิทธิ์ผู้ใช้' เพื่อดูสิทธิ์"
            };

            // ListView showing granted OrgUnits
            _lvVisibility = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = true
            };
            _lvVisibility.Columns.Add("หน่วยงาน", 280);
            _lvVisibility.Columns.Add("รหัสหน่วยงาน", 120);
            _lvVisibility.Columns.Add("ให้สิทธิ์โดย", 160);
            _lvVisibility.Columns.Add("วันที่ให้สิทธิ์", 140);

            // Bottom button panel
            var pnlVisBtns = new Panel { Dock = DockStyle.Bottom, Height = 44 };
            pnlVisBtns.Padding = new Padding(4, 8, 4, 0);

            _btnGrantVisibility = new Button
            {
                Text = "➕ เพิ่มสิทธิ์",
                Location = new Point(0, 8),
                Size = new Size(110, 28),
                Enabled = false
            };

            _btnRevokeVisibility = new Button
            {
                Text = "➖ ลบสิทธิ์",
                Location = new Point(118, 8),
                Size = new Size(110, 28),
                Enabled = false
            };

            _lblVisibilityStatus = new Label
            {
                Location = new Point(240, 14),
                Size = new Size(500, 23),
                ForeColor = Color.Gray
            };

            pnlVisBtns.Controls.Add(_btnGrantVisibility);
            pnlVisBtns.Controls.Add(_btnRevokeVisibility);
            pnlVisBtns.Controls.Add(_lblVisibilityStatus);

            _tabVisibility.Controls.Add(_lvVisibility);
            _tabVisibility.Controls.Add(pnlVisBtns);
            _tabVisibility.Controls.Add(_lblVisibilityUser);

            // Only add the tab and enable controls for Admin
            if (isAdmin)
            {
                tabControl.TabPages.Add(_tabVisibility);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event wiring (kept separate from Designer)
        // ─────────────────────────────────────────────────────────────────────

        private void WireEvents()
        {
            // Tab 1
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) FilterUsers(); };
            _cmbRoleFilter.SelectedIndexChanged += (s, e) => FilterUsers();
            btnFind.Click += (s, e) => FilterUsers();
            _lvUsers.SelectedIndexChanged += LvUsers_SelectionChanged;
            _lvUsers.DoubleClick += BtnChangeRole_Click;
            _btnChangeRole.Click += BtnChangeRole_Click;
            _btnDeactivate.Click += BtnDeactivate_Click;
            _btnAssignOrg.Click += BtnAssignOrg_Click;
            _btnRefresh.Click += (s, e) => LoadData();
            // Tab 2
            _orgTree.AfterSelect += OrgTree_AfterSelect;
            _orgTree.AfterSelect += (s, e) =>
                _btnAddUserToOrg.Enabled = e.Node != null && e.Node.Tag is OrgUnit;
            _lvOrgUsers.SelectedIndexChanged += (s, e) =>
                _btnChangeOrg.Enabled = _lvOrgUsers.SelectedItems.Count > 0;
            _btnChangeOrg.Click += BtnChangeOrg_Click;
            _btnAddUserToOrg.Click += BtnAddUserToOrg_Click;
            // Tab 3
            _lvVisibility.SelectedIndexChanged += (s, e) =>
            {
                bool hasRevocable = _lvVisibility.SelectedItems.Cast<ListViewItem>()
                    .Any(item => item.Name != "own_org" && item.Tag is OrgUnit);
                _btnRevokeVisibility.Enabled = hasRevocable;
            };
            _btnGrantVisibility.Click += BtnGrantVisibility_Click;
            _btnRevokeVisibility.Click += BtnRevokeVisibility_Click;

            tabControl.SelectedIndexChanged += (s, e) =>
            {
                if (_tabVisibility == null || tabControl.SelectedTab != _tabVisibility) return;
                var selectedUser = GetSelectedUser();
                if (selectedUser != null)
                    LoadVisibilityList(selectedUser);
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // Data Loading
        // ─────────────────────────────────────────────────────────────────────

        private void LoadData()
        {
            try
            {
                _allUsers = (_userAdminController != null
                    ? _userAdminController.GetAllUsers() ?? Enumerable.Empty<UserAccount>()
                    : Enumerable.Empty<UserAccount>()).ToList();

                _allOrgs = (_orgLookupController != null
                    ? _orgLookupController.GetHierarchy() ?? Enumerable.Empty<OrgUnit>()
                    : Enumerable.Empty<OrgUnit>()).ToList();

                FilterUsers();
                LoadOrgTree();
                SetStatus(_lblStatus,
                    string.Format("โหลดสำเร็จ: {0} ผู้ใช้", _allUsers.Count), Color.Gray);
            }
            catch (Exception ex)
            {
                SetStatus(_lblStatus, "โหลดข้อมูลไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        private void FilterUsers()
        {
            string search = _txtSearch.Text.Trim().ToLower();
            int roleIdx = _cmbRoleFilter.SelectedIndex;

            IEnumerable<UserAccount> result = _allUsers;

            if (!string.IsNullOrEmpty(search))
                result = result.Where(u =>
                    (u.DisplayName ?? "").ToLower().Contains(search) ||
                    (u.ADUserName ?? "").ToLower().Contains(search) ||
                    (u.EmployeeCode ?? "").ToLower().Contains(search));

            if (roleIdx > 0)
                result = result.Where(u => u.AppRole == roleIdx);

            PopulateUserList(result.ToList());
        }

        private void PopulateUserList(List<UserAccount> users)
        {
            _lvUsers.BeginUpdate();
            _lvUsers.Items.Clear();
            foreach (var u in users)
            {
                var item = new ListViewItem(u.DisplayName ?? u.ADUserName);
                item.SubItems.Add(u.ADUserName ?? "");
                item.SubItems.Add(u.DepartmentName ?? "");
                item.SubItems.Add(u.DivisionName ?? "");
                item.SubItems.Add(RoleName(u.AppRole));
                item.SubItems.Add(u.IsActive ? "ใช้งาน" : "ปิดใช้");
                item.Tag = u;
                item.ForeColor = u.IsActive ? Color.Black : Color.DimGray;
                _lvUsers.Items.Add(item);
            }
            _lvUsers.EndUpdate();
        }

        private void LoadOrgTree()
        {
            _orgTree.BeginUpdate();
            _orgTree.Nodes.Clear();

            List<OrgUnit> visible;
            if (_visibilityController != null)
            {
                try
                {
                    var visibleSet = new HashSet<int>(
                        _visibilityController.GetVisibleOrgUnits(_currentUser.UserID, _currentUser)
                                          .Select(o => o.OrgUnitID));
                    visible = _allOrgs.Where(o => visibleSet.Contains(o.OrgUnitID)).ToList();
                }
                catch
                {
                    visible = GetVisibleOrgUnits(_allOrgs, _currentUser);
                }
            }
            else
            {
                visible = GetVisibleOrgUnits(_allOrgs, _currentUser);
            }

            foreach (var root in visible.Where(o => !o.ParentOrgUnitID.HasValue || o.ParentOrgUnitID == 0))
                _orgTree.Nodes.Add(BuildOrgTreeNode(root, visible));

            _orgTree.ExpandAll();
            _orgTree.EndUpdate();
        }

        /// <summary>
        /// กรอง OrgUnit ที่ current user มีสิทธิ์มองเห็น:
        /// Admin(4)       → ทั้งหมด
        /// Manager(3)     → OrgUnit ของตัวเองและ sub-units ทั้งหมดใต้มัน
        /// Officer/Sr(1,2)→ เฉพาะ OrgUnit ของตัวเองเท่านั้น
        /// </summary>
        private static System.Collections.Generic.HashSet<int> GetVisibleOrgUnitIds(
            System.Collections.Generic.List<OrgUnit> all, UserContext user)
        {
            if (user == null || user.AppRole >= 4)
                return new System.Collections.Generic.HashSet<int>(all.Select(o => o.OrgUnitID));

            var result = new System.Collections.Generic.HashSet<int>();
            if (user.OrgUnitID <= 0) return result;

            result.Add(user.OrgUnitID);

            if (user.AppRole >= 3) // Manager — เพิ่ม sub-units ทั้งหมด
            {
                var queue = new System.Collections.Generic.Queue<int>();
                queue.Enqueue(user.OrgUnitID);
                while (queue.Count > 0)
                {
                    int parentId = queue.Dequeue();
                    foreach (var child in all.Where(o => o.ParentOrgUnitID == parentId))
                    {
                        if (result.Add(child.OrgUnitID))
                            queue.Enqueue(child.OrgUnitID);
                    }
                }
            }

            return result;
        }

        private static System.Collections.Generic.List<OrgUnit> GetVisibleOrgUnits(
            System.Collections.Generic.List<OrgUnit> all, UserContext user)
        {
            var ids = GetVisibleOrgUnitIds(all, user);
            return all.Where(o => ids.Contains(o.OrgUnitID)).ToList();
        }

        private TreeNode BuildOrgTreeNode(OrgUnit org, System.Collections.Generic.List<OrgUnit> visible = null)
        {
            string prefix = org.OrgUnitType == 1 ? "▶ " : org.OrgUnitType == 2 ? "  ◆ " : "    • ";
            var node = new TreeNode(prefix + (org.OrgUnitNameTH ?? org.OrgUnitCode)) { Tag = org };
            var children = (visible ?? _allOrgs).Where(o => o.ParentOrgUnitID == org.OrgUnitID);
            foreach (var child in children)
                node.Nodes.Add(BuildOrgTreeNode(child, visible));
            return node;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event Handlers
        // ─────────────────────────────────────────────────────────────────────

        private void LvUsers_SelectionChanged(object sender, EventArgs e)
        {
            bool sel = _lvUsers.SelectedItems.Count > 0;
            _btnChangeRole.Enabled = sel;
            _btnAssignOrg.Enabled = sel;
            if (sel)
            {
                var u = _lvUsers.SelectedItems[0].Tag as UserAccount;
                _btnDeactivate.Enabled = u != null && u.IsActive;
                LoadVisibilityList(u);
            }
            else
            {
                _btnDeactivate.Enabled = false;
                ClearVisibilityList();
            }
        }

        private void OrgTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var org = e.Node != null ? e.Node.Tag as OrgUnit : null;
            if (org == null) return;

            LoadUsersForOrgNode(org);
        }

        private void LoadUsersForOrgNode(OrgUnit org)
        {
            if (org == null) return;

            bool includeDescendants = org.OrgUnitType <= 0;
            HashSet<int> orgIds;
            if (includeDescendants)
            {
                orgIds = GetOrgAndDescendantIds(org.OrgUnitID);
            }
            else
            {
                orgIds = new HashSet<int> { org.OrgUnitID };
            }

            var users = _allUsers.Where(u => orgIds.Contains(u.OrgUnitID)).ToList();
            _lvOrgUsers.BeginUpdate();
            _lvOrgUsers.Items.Clear();
            foreach (var u in users)
            {
                var item = new ListViewItem(u.DisplayName ?? u.ADUserName);
                item.SubItems.Add(u.ADUserName ?? "");
                item.SubItems.Add(RoleName(u.AppRole));
                item.SubItems.Add(u.IsActive ? "ใช้งาน" : "ปิดใช้");
                item.Tag = u;
                item.ForeColor = u.IsActive ? Color.Black : Color.DimGray;
                _lvOrgUsers.Items.Add(item);
            }
            _lvOrgUsers.EndUpdate();

            SetStatus(_lblOrgStatus,
                string.Format("{0}: {1} ผู้ใช้{2}",
                    org.OrgUnitNameTH ?? org.OrgUnitCode,
                    users.Count,
                    includeDescendants ? " (รวมหน่วยงานย่อย)" : ""),
                Color.Gray);
        }

        private HashSet<int> GetOrgAndDescendantIds(int rootOrgId)
        {
            var result = new HashSet<int>();
            if (rootOrgId <= 0) return result;

            var queue = new Queue<int>();
            queue.Enqueue(rootOrgId);
            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                if (!result.Add(currentId))
                    continue;

                foreach (var child in _allOrgs.Where(o => o.ParentOrgUnitID == currentId))
                    queue.Enqueue(child.OrgUnitID);
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tab 3 — Visibility handlers
        // ─────────────────────────────────────────────────────────────────────

        private UserAccount GetSelectedUser()
        {
            if (_lvUsers.SelectedItems.Count == 0) return null;
            return _lvUsers.SelectedItems[0].Tag as UserAccount;
        }

        private void RefreshVisibilityForUserIfSelected(int userId)
        {
            if (userId <= 0) return;
            var selected = GetSelectedUser();
            if (selected == null || selected.UserID != userId) return;

            var latest = _allUsers.FirstOrDefault(u => u.UserID == userId) ?? selected;
            LoadVisibilityList(latest);
        }

        private void LoadVisibilityList(UserAccount user)
        {
            if (_tabVisibility == null || _lvVisibility == null) return;
            if (user == null) { ClearVisibilityList(); return; }

            _lblVisibilityUser.Text = string.Format("สิทธิ์มองเห็นหน่วยงานของ: {0}",
                user.DisplayName ?? user.ADUserName);
            _btnGrantVisibility.Enabled = _visibilityController != null;
            _btnRevokeVisibility.Enabled = false;

            _lvVisibility.BeginUpdate();
            _lvVisibility.Items.Clear();

            var activeOrgLookup = _allOrgs
                .Where(o => o.IsActive && !o.IsDeleted)
                .ToDictionary(o => o.OrgUnitID, o => o);

            // Own OrgUnit — always visible, cannot be revoked
            if (user.OrgUnitID > 0)
            {
                OrgUnit ownOrg;
                if (activeOrgLookup.TryGetValue(user.OrgUnitID, out ownOrg))
                {
                    var baseItem = new ListViewItem(ownOrg.OrgUnitNameTH ?? ownOrg.OrgUnitCode);
                    baseItem.SubItems.Add(ownOrg.OrgUnitCode ?? "");
                    baseItem.SubItems.Add("(สิทธิ์พื้นฐาน)");
                    baseItem.SubItems.Add("—");
                    baseItem.Name = "own_org";
                    baseItem.Tag = null;
                    baseItem.ForeColor = Color.FromArgb(0, 100, 0);
                    baseItem.BackColor = Color.FromArgb(240, 255, 240);
                    _lvVisibility.Items.Add(baseItem);
                }
            }

            if (_visibilityController != null)
            {
                try
                {
                    // Get explicit grants — deduplicate by OrgUnitID (keep latest by GrantedDate)
                    var grants = _visibilityController.GetExplicitGrantsForUser(user.UserID, _currentUser)
                        .GroupBy(g => g.OrgUnitID)
                        .Select(g => g.OrderByDescending(x => x.GrantedDate).First())
                        .ToList();

                    // Build lookup for user names (GrantedBy)
                    var grantedByIds = grants.Select(g => g.GrantedBy).Distinct().ToList();
                    var userLookup = _allUsers
                        .Where(u => grantedByIds.Contains(u.UserID))
                        .ToDictionary(u => u.UserID, u => u.DisplayName ?? u.ADUserName);

                    // Build sort path: traverse parent chain to get "SC-006/DIV-009/DEPT-011"
                    Func<OrgUnit, string> getSortPath = null;
                    getSortPath = (o) =>
                    {
                        if (o == null) return "";
                        string self = o.OrgUnitCode ?? o.OrgUnitNameTH ?? "";
                        if (!o.ParentOrgUnitID.HasValue || o.ParentOrgUnitID == 0) return self;
                        OrgUnit parent;
                        if (!activeOrgLookup.TryGetValue(o.ParentOrgUnitID.Value, out parent)) return self;
                        return getSortPath(parent) + "/" + self;
                    };

                    // Resolve + sort grants in hierarchy order (SC → DIV → DEPT)
                    var resolvedGrants = grants
                        .Select(g => { OrgUnit o; activeOrgLookup.TryGetValue(g.OrgUnitID, out o); return new { grant = g, org = o }; })
                        .Where(x => x.org != null)
                        .OrderBy(x => getSortPath(x.org))
                        .ToList();

                    foreach (var entry in resolvedGrants)
                    {
                        var grant = entry.grant;
                        var org = entry.org;

                        // Visual indent + prefix by org level
                        string indent = org.OrgUnitType == 1 ? ""
                                      : org.OrgUnitType == 2 ? "   "
                                      : "      ";
                        string typePrefix = org.OrgUnitType == 1 ? "🏢 "
                                         : org.OrgUnitType == 2 ? "◆ "
                                         : "• ";
                        string orgName = indent + typePrefix + (org.OrgUnitNameTH ?? org.OrgUnitCode);
                        string orgCode = org.OrgUnitCode ?? "";

                        // Get GrantedBy user name
                        string grantedByName = "";
                        if (!userLookup.TryGetValue(grant.GrantedBy, out grantedByName))
                        {
                            grantedByName = grant.GrantedBy == _currentUser.UserID
                                ? (_currentUser.ADUserName ?? string.Format("UserID:{0}", _currentUser.UserID))
                                : string.Format("UserID:{0}", grant.GrantedBy);
                        }

                        var item = new ListViewItem(orgName);
                        item.SubItems.Add(orgCode);
                        item.SubItems.Add(grantedByName);
                        item.SubItems.Add(grant.GrantedDate.ToString("dd/MM/yyyy HH:mm"));
                        item.Tag = org;

                        // Color by org type for visual clarity
                        if (org.OrgUnitType == 1)
                            item.ForeColor = Color.FromArgb(25, 55, 105);
                        else if (org.OrgUnitType == 2)
                            item.ForeColor = Color.FromArgb(50, 90, 150);

                        _lvVisibility.Items.Add(item);
                    }

                    // If no explicit grants, show role-based fallback info
                    if (grants.Count == 0)
                    {
                        var visibleOrgs = _visibilityController.GetVisibleOrgUnits(user.UserID, _currentUser).ToList();
                        foreach (var org in visibleOrgs.Where(o => o.IsActive && !o.IsDeleted))
                        {
                            var item = new ListViewItem(org.OrgUnitNameTH ?? org.OrgUnitCode);
                            item.SubItems.Add(org.OrgUnitCode ?? "");
                            item.SubItems.Add("(ตามบทบาท)");
                            item.SubItems.Add("—");
                            item.Tag = org;
                            item.ForeColor = Color.Gray;
                            _lvVisibility.Items.Add(item);
                        }
                    }

                    SetStatus(_lblVisibilityStatus,
                        string.Format("{0} หน่วยงาน", _lvVisibility.Items.Count), Color.Gray);
                }
                catch (Exception ex)
                {
                    SetStatus(_lblVisibilityStatus, "โหลดไม่สำเร็จ: " + ex.Message, Color.Red);
                }
            }

            _lvVisibility.EndUpdate();
        }

        private void ClearVisibilityList()
        {
            if (_lvVisibility == null) return;
            _lvVisibility.Items.Clear();
            if (_lblVisibilityUser != null)
                _lblVisibilityUser.Text = "เลือกผู้ใช้จากแท็บ 'สิทธิ์ผู้ใช้' เพื่อดูสิทธิ์";
            if (_btnGrantVisibility != null) _btnGrantVisibility.Enabled = false;
            if (_btnRevokeVisibility != null) _btnRevokeVisibility.Enabled = false;
        }

        private void BtnGrantVisibility_Click(object sender, EventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null || _visibilityController == null) return;

            // Show OrgUnit picker — returns multiple selections via CheckedListBox
            var chosen = PickOrgUnitsFromFullHierarchy();
            if (chosen == null || chosen.Count == 0) return;

            var errors = new List<string>();
            foreach (var org in chosen)
            {
                try
                {
                    _visibilityController.GrantVisibility(user.UserID, org.OrgUnitID, _currentUser);
                    if (_auditLogController != null)
                        _auditLogController.Log(new AuditEntry
                        {
                            Module = "UserAccess",
                            Action = "GRANT_VISIBILITY",
                            UserID = _currentUser.UserID,
                            TableName = "UserOrgVisibility",
                            RecordID = user.UserID,
                            NewValue = string.Format("TargetUser:{0} OrgUnitID:{1} OrgName:{2}",
                                             user.ADUserName, org.OrgUnitID, org.OrgUnitNameTH ?? org.OrgUnitCode),
                            MachineName = _currentUser.MachineName,
                            IPAddress = _currentUser.IPAddress,
                            SessionID = _currentUser.SessionID,
                            Result = 1,
                            AuditDate = DateTime.Now
                        });
                }
                catch (Exception ex) { errors.Add(string.Format("{0}: {1}", org.OrgUnitNameTH ?? org.OrgUnitCode, ex.Message)); }
            }

            LoadVisibilityList(user);

            if (errors.Count == 0)
            {
                SetStatus(_lblVisibilityStatus,
                    string.Format("เพิ่มสิทธิ์ {0} หน่วยงานเรียบร้อย", chosen.Count), Color.DarkGreen);
            }
            else
            {
                MessageBox.Show("เพิ่มสิทธิ์ไม่สำเร็จบางรายการ:\n" + string.Join("\n", errors),
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetStatus(_lblVisibilityStatus, "เพิ่มสิทธิ์บางรายการไม่สำเร็จ", Color.OrangeRed);
            }
        }

        private void BtnRevokeVisibility_Click(object sender, EventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null || _visibilityController == null) return;

            var toRevoke = _lvVisibility.SelectedItems.Cast<ListViewItem>()
                .Where(item => item.Name != "own_org" && item.Tag is OrgUnit)
                .Select(item => item.Tag as OrgUnit)
                .ToList();

            if (toRevoke.Count == 0) return;

            string confirmMsg = toRevoke.Count == 1
                ? string.Format("ลบสิทธิ์มองเห็น '{0}' ของ '{1}' ใช่หรือไม่?",
                    toRevoke[0].OrgUnitNameTH ?? toRevoke[0].OrgUnitCode,
                    user.DisplayName ?? user.ADUserName)
                : string.Format("ลบสิทธิ์มองเห็น {0} หน่วยงาน ของ '{1}' ใช่หรือไม่?\n\n{2}",
                    toRevoke.Count,
                    user.DisplayName ?? user.ADUserName,
                    string.Join("\n", toRevoke.Select(o => "• " + (o.OrgUnitNameTH ?? o.OrgUnitCode))));

            if (MessageBox.Show(confirmMsg, "ยืนยันลบสิทธิ์",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            var errors = new List<string>();
            int successCount = 0;

            foreach (var org in toRevoke)
            {
                try
                {
                    _visibilityController.RevokeVisibility(user.UserID, org.OrgUnitID, _currentUser);
                    if (_auditLogController != null)
                        _auditLogController.Log(new AuditEntry
                        {
                            Module = "UserAccess",
                            Action = "REVOKE_VISIBILITY",
                            UserID = _currentUser.UserID,
                            TableName = "UserOrgVisibility",
                            RecordID = user.UserID,
                            OldValue = string.Format("TargetUser:{0} OrgUnitID:{1} OrgName:{2}",
                                             user.ADUserName, org.OrgUnitID, org.OrgUnitNameTH ?? org.OrgUnitCode),
                            MachineName = _currentUser.MachineName,
                            IPAddress = _currentUser.IPAddress,
                            SessionID = _currentUser.SessionID,
                            Result = 1,
                            AuditDate = DateTime.Now
                        });
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("{0}: {1}", org.OrgUnitNameTH ?? org.OrgUnitCode, ex.Message));
                }
            }

            LoadVisibilityList(user);

            if (errors.Count == 0)
            {
                SetStatus(_lblVisibilityStatus,
                    string.Format("ลบสิทธิ์ {0} หน่วยงานเรียบร้อย", successCount), Color.DarkGreen);
            }
            else
            {
                MessageBox.Show("ลบสิทธิ์ไม่สำเร็จบางรายการ:\n" + string.Join("\n", errors),
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetStatus(_lblVisibilityStatus,
                    string.Format("ลบสำเร็จ {0} รายการ, ไม่สำเร็จ {1} รายการ", successCount, errors.Count),
                    Color.OrangeRed);
            }
        }

        /// <summary>
        /// Opens a picker dialog with a CheckBox TreeView showing the full OrgUnit hierarchy.
        /// Rules:
        ///   1. Already-granted nodes are pre-checked and cannot be unchecked.
        ///   2. Checking a parent auto-checks all descendants; unchecking a parent unchecks all descendants.
        ///   3. Only newly checked nodes (not already granted) are returned.
        /// </summary>
        private List<OrgUnit> PickOrgUnitsFromFullHierarchy()
        {
            var results = new List<OrgUnit>();
            var allOrgs = (_orgLookupController != null
                ? _orgLookupController.GetHierarchy() ?? Enumerable.Empty<OrgUnit>()
                : _allOrgs)
                .Where(o => o.IsActive && !o.IsDeleted)
                .ToList();
            var activeOrgIds = new HashSet<int>(allOrgs.Select(o => o.OrgUnitID));

            // Collect already-granted IDs — use explicit grants only (no role-based fallback)
            var alreadyGrantedIds = new HashSet<int>();
            if (_visibilityController != null)
            {
                var selectedUser = GetSelectedUser();
                if (selectedUser != null)
                {
                    try
                    {
                        foreach (var g in _visibilityController.GetExplicitGrantsForUser(selectedUser.UserID, _currentUser))
                            alreadyGrantedIds.Add(g.OrgUnitID);
                    }
                    catch { }
                }
            }

            using (var dlg = new Form())
            {
                dlg.Text = "เลือกหน่วยงานที่จะให้สิทธิ์";
                dlg.Size = new Size(440, 580);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Font = new Font("Segoe UI", 9.5F);
                dlg.BackColor = Color.FromArgb(243, 246, 251);

                // ── Header ─────────────────────────────────────────────────────
                var pnlHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 52,
                    BackColor = Color.FromArgb(25, 55, 105),
                    Padding = new Padding(14, 0, 14, 0),
                };
                var lblTitle = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "เลือกหน่วยงานที่จะให้สิทธิ์",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                pnlHeader.Controls.Add(lblTitle);

                // ── TreeView (inside a white card panel) ──────────────────────
                var pnlCard = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Margin = new Padding(12),
                    Padding = new Padding(2),
                };

                var tree = new TreeView
                {
                    Dock = DockStyle.Fill,
                    CheckBoxes = true,
                    ShowLines = true,
                    ShowPlusMinus = true,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.White,
                    Font = new Font("Segoe UI", 9.5F),
                    HotTracking = true,
                    ItemHeight = 22,
                };
                pnlCard.Controls.Add(tree);

                // Recursively build TreeNodes — visited set prevents duplicate/circular rendering
                var visitedInTree = new HashSet<int>();
                Func<OrgUnit, TreeNode> buildNode = null;
                buildNode = (org) =>
                {
                    string prefix = org.OrgUnitType == 1 ? "🏢 "
                                  : org.OrgUnitType == 2 ? "📂 "
                                  : "📋 ";
                    var node = new TreeNode(prefix + (org.OrgUnitNameTH ?? org.OrgUnitCode))
                    {
                        Tag = org,
                        Checked = alreadyGrantedIds.Contains(org.OrgUnitID)
                    };
                    if (alreadyGrantedIds.Contains(org.OrgUnitID))
                        node.ForeColor = Color.Gray;

                    visitedInTree.Add(org.OrgUnitID);
                    foreach (var child in allOrgs
                        .Where(c => c.ParentOrgUnitID == org.OrgUnitID && !visitedInTree.Contains(c.OrgUnitID))
                        .OrderBy(c => c.OrgUnitType).ThenBy(c => c.SortOrder).ThenBy(c => c.Name))
                    {
                        node.Nodes.Add(buildNode(child));
                    }
                    return node;
                };

                tree.BeginUpdate();
                foreach (var root in allOrgs
                    .Where(o => !o.ParentOrgUnitID.HasValue || o.ParentOrgUnitID == 0)
                    .OrderBy(o => o.OrgUnitType).ThenBy(o => o.SortOrder).ThenBy(o => o.Name))
                {
                    tree.Nodes.Add(buildNode(root));
                }
                tree.ExpandAll();
                tree.EndUpdate();

                // Guard against re-entrant cascade calls
                bool _cascading = false;

                // Helper: set checked state on all descendant nodes
                Action<TreeNode, bool> cascadeCheck = null;
                cascadeCheck = (parent, check) =>
                {
                    foreach (TreeNode child in parent.Nodes)
                    {
                        var childOrg = child.Tag as OrgUnit;
                        if (childOrg != null && !alreadyGrantedIds.Contains(childOrg.OrgUnitID))
                            child.Checked = check;
                        cascadeCheck(child, check);
                    }
                };

                tree.AfterCheck += (ts, te) =>
                {
                    if (_cascading) return;
                    var org = te.Node.Tag as OrgUnit;
                    if (org == null) return;

                    // Block unchecking already-granted nodes
                    if (alreadyGrantedIds.Contains(org.OrgUnitID))
                    {
                        _cascading = true;
                        te.Node.Checked = true;
                        _cascading = false;
                        return;
                    }

                    // Cascade to children
                    if (te.Node.Nodes.Count > 0)
                    {
                        _cascading = true;
                        cascadeCheck(te.Node, te.Node.Checked);
                        _cascading = false;
                    }
                };

                // ── Footer ────────────────────────────────────────────────────
                var pnlBtns = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 56,
                    BackColor = Color.FromArgb(235, 239, 246),
                    Padding = new Padding(10),
                };
                var btnOk = new Button
                {
                    Text = "เลือก",
                    Width = 100,
                    Height = 34,
                    BackColor = Color.FromArgb(25, 55, 105),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCan = new Button
                {
                    Text = "ยกเลิก",
                    Width = 90,
                    Height = 34,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(80, 80, 80),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5F),
                    DialogResult = DialogResult.Cancel,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                };
                btnCan.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);

                // position buttons on the right
                pnlBtns.Controls.AddRange(new Control[] { btnOk, btnCan });
                pnlBtns.Layout += (ls, le) =>
                {
                    int y = (pnlBtns.ClientSize.Height - btnOk.Height) / 2;
                    btnCan.Location = new Point(pnlBtns.ClientSize.Width - btnCan.Width - 8, y);
                    btnOk.Location = new Point(btnCan.Left - btnOk.Width - 8, y);
                };

                btnOk.Click += (bs, be) =>
                {
                    // Collect all checked nodes that are NOT already granted
                    var newlyChecked = new List<OrgUnit>();
                    Action<TreeNodeCollection> collect = null;
                    collect = (nodes) =>
                    {
                        foreach (TreeNode n in nodes)
                        {
                            var o = n.Tag as OrgUnit;
                            if (o != null && n.Checked && !alreadyGrantedIds.Contains(o.OrgUnitID))
                                newlyChecked.Add(o);
                            collect(n.Nodes);
                        }
                    };
                    collect(tree.Nodes);

                    if (newlyChecked.Count == 0)
                    {
                        MessageBox.Show("กรุณาเลือกหน่วยงานใหม่อย่างน้อย 1 รายการ", "แจ้งเตือน");
                        return;
                    }
                    results.AddRange(newlyChecked);
                    dlg.DialogResult = DialogResult.OK;
                };

                dlg.CancelButton = btnCan;
                // DockStyle order: Fill first, Bottom, Top last
                dlg.Controls.Add(pnlCard);
                dlg.Controls.Add(pnlBtns);
                dlg.Controls.Add(pnlHeader);
                dlg.ShowDialog(this);
            }
            return results;
        }

        /// <summary>Helper wrapper for ListBox display.</summary>
        private class OrgUnitListItem
        {
            public OrgUnit OrgUnit { get; set; }
            public string DisplayText { get; set; }
            public override string ToString() { return DisplayText; }
        }

        private void BtnChangeRole_Click(object sender, EventArgs e)
        {
            if (_lvUsers.SelectedItems.Count == 0) return;
            var user = _lvUsers.SelectedItems[0].Tag as UserAccount;
            if (user == null) return;

            using (var dlg = new Form())
            {
                // ── Palette ────────────────────────────────────────────────────
                var navyDark = Color.FromArgb(25, 55, 105);
                var navyMid = Color.FromArgb(50, 90, 150);
                var navyLight = Color.FromArgb(215, 228, 248);
                var bodyBg = Color.FromArgb(243, 246, 251);
                var footerBg = Color.FromArgb(228, 234, 245);
                var lblColor = Color.FromArgb(55, 65, 90);
                var boldFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                var uiFont = new Font("Segoe UI", 10.5f);
                var lblFont = new Font("Segoe UI", 10f);

                dlg.Text = "เปลี่ยนบทบาทผู้ใช้";
                dlg.Size = new Size(420, 310);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = bodyBg;
                dlg.Font = uiFont;

                // ── Header ──────────────────────────────────────────────────────
                var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = navyDark };
                var pnlIcon = new Panel { Size = new Size(36, 36), Location = new Point(14, 11), BackColor = Color.Transparent };
                pnlIcon.Paint += (s, pe) =>
                {
                    pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    pe.Graphics.FillEllipse(new SolidBrush(navyMid), 0, 0, 35, 35);
                    using (var f = new Font("Segoe UI", 16f))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        pe.Graphics.DrawString("★", f, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
                };
                pnlHeader.Controls.Add(pnlIcon);
                pnlHeader.Controls.Add(new Label
                {
                    Text = "เปลี่ยนบทบาทผู้ใช้",
                    Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(58, 16),
                });

                // ── Body ────────────────────────────────────────────────────────
                var pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = bodyBg, Padding = new Padding(16, 14, 16, 8) };

                // Section: ข้อมูลผู้ใช้
                var barUser = new Panel { Left = 0, Top = 0, Width = 370, Height = 24, BackColor = navyLight };
                barUser.Controls.Add(new Label { Text = "  ข้อมูลผู้ใช้", Font = boldFont, ForeColor = navyDark, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft });
                pnlBody.Controls.Add(barUser);

                pnlBody.Controls.Add(new Label { Text = "ผู้ใช้:", Left = 0, Top = 34, Width = 90, Height = 24, Font = lblFont, ForeColor = lblColor, TextAlign = ContentAlignment.MiddleRight });
                pnlBody.Controls.Add(new Label
                {
                    Text = user.DisplayName ?? user.ADUserName,
                    Left = 96,
                    Top = 34,
                    Width = 270,
                    Height = 24,
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    ForeColor = navyDark,
                    TextAlign = ContentAlignment.MiddleLeft,
                });
                pnlBody.Controls.Add(new Label { Text = "บทบาทปัจจุบัน:", Left = 0, Top = 62, Width = 90, Height = 24, Font = lblFont, ForeColor = lblColor, TextAlign = ContentAlignment.MiddleRight });
                pnlBody.Controls.Add(new Label
                {
                    Text = RoleName(user.AppRole),
                    Left = 96,
                    Top = 62,
                    Width = 200,
                    Height = 24,
                    Font = lblFont,
                    ForeColor = Color.FromArgb(60, 120, 60),
                    TextAlign = ContentAlignment.MiddleLeft,
                });

                // Section: บทบาทใหม่
                var barRole = new Panel { Left = 0, Top = 98, Width = 370, Height = 24, BackColor = navyLight };
                barRole.Controls.Add(new Label { Text = "  เลือกบทบาทใหม่", Font = boldFont, ForeColor = navyDark, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft });
                pnlBody.Controls.Add(barRole);

                pnlBody.Controls.Add(new Label { Text = "บทบาท:", Left = 0, Top = 132, Width = 90, Height = 28, Font = lblFont, ForeColor = lblColor, TextAlign = ContentAlignment.MiddleRight });
                var cmb = new ComboBox
                {
                    Left = 96,
                    Top = 130,
                    Width = 270,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = uiFont,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                };
                cmb.Items.AddRange(new object[] { "Officer (1)", "SrOfficer (2)", "Manager (3)", "Admin (4)" });
                cmb.SelectedIndex = Math.Max(0, Math.Min(3, user.AppRole - 1));
                pnlBody.Controls.Add(cmb);

                // ── Footer ──────────────────────────────────────────────────────
                var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = footerBg };
                var btnOk = new Button
                {
                    Text = "บันทึก",
                    Size = new Size(110, 36),
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
                pnlFooter.Resize += (s, ev) =>
                {
                    btnOk.Location = new Point(pnlFooter.Width - btnOk.Width - btnCan.Width - 20, 9);
                    btnCan.Location = new Point(pnlFooter.Width - btnCan.Width - 10, 9);
                };
                btnOk.Location = new Point(420 - btnOk.Width - btnCan.Width - 20, 9);
                btnCan.Location = new Point(420 - btnCan.Width - 10, 9);
                pnlFooter.Controls.Add(btnOk);
                pnlFooter.Controls.Add(btnCan);

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCan;
                dlg.Controls.Add(pnlHeader);
                dlg.Controls.Add(pnlFooter);
                dlg.Controls.Add(pnlBody);

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                int newRole = cmb.SelectedIndex + 1;
                if (newRole == user.AppRole) return;

                int oldRole = user.AppRole;
                user.AppRole = newRole;
                try
                {
                    _userAdminController.UpdateUser(user);
                    if (_auditLogController != null)
                        _auditLogController.Log(new AuditEntry
                        {
                            Module = "UserAccess",
                            Action = "CHANGE_ROLE",
                            UserID = _currentUser.UserID,
                            TableName = "UserAccount",
                            RecordID = user.UserID,
                            OldValue = string.Format("Role:{0}", RoleName(oldRole)),
                            NewValue = string.Format("Role:{0}", RoleName(newRole)),
                            MachineName = _currentUser.MachineName,
                            IPAddress = _currentUser.IPAddress,
                            SessionID = _currentUser.SessionID,
                            Result = 1,
                            AuditDate = DateTime.Now
                        });
                    _lvUsers.SelectedItems[0].SubItems[4].Text = RoleName(newRole);
                    SetStatus(_lblStatus,
                        string.Format("เปลี่ยนบทบาท '{0}' เป็น {1} เรียบร้อย",
                            user.DisplayName ?? user.ADUserName, RoleName(newRole)),
                        Color.DarkGreen);
                }
                catch (Exception ex)
                {
                    user.AppRole = oldRole;
                    SetStatus(_lblStatus, "บันทึกไม่สำเร็จ: " + ex.Message, Color.Red);
                }
            }
        }

        private void BtnDeactivate_Click(object sender, EventArgs e)
        {
            if (_lvUsers.SelectedItems.Count == 0) return;
            var user = _lvUsers.SelectedItems[0].Tag as UserAccount;
            if (user == null || !user.IsActive) return;

            if (MessageBox.Show(
                    string.Format("ปิดใช้งานผู้ใช้ '{0}' ใช่หรือไม่?\n(ผู้ใช้จะไม่สามารถเข้าสู่ระบบได้)",
                        user.DisplayName ?? user.ADUserName),
                    "ยืนยันปิดใช้งาน",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                _userAdminController.DeactivateUser(user.UserID);
                user.IsActive = false;
                if (_auditLogController != null)
                    _auditLogController.Log(new AuditEntry
                    {
                        Module = "UserAccess",
                        Action = "DEACTIVATE_USER",
                        UserID = _currentUser.UserID,
                        TableName = "UserAccount",
                        RecordID = user.UserID,
                        NewValue = string.Format("User:{0} IsActive:false", user.ADUserName),
                        MachineName = _currentUser.MachineName,
                        IPAddress = _currentUser.IPAddress,
                        SessionID = _currentUser.SessionID,
                        Result = 1,
                        AuditDate = DateTime.Now
                    });
                var sel = _lvUsers.SelectedItems[0];
                sel.SubItems[5].Text = "ปิดใช้";
                sel.ForeColor = Color.DimGray;
                _btnDeactivate.Enabled = false;
                SetStatus(_lblStatus,
                    string.Format("ปิดใช้งาน '{0}' เรียบร้อย", user.DisplayName ?? user.ADUserName),
                    Color.DarkGreen);
            }
            catch (Exception ex)
            {
                SetStatus(_lblStatus, "บันทึกไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        private void BtnAssignOrg_Click(object sender, EventArgs e)
        {
            if (_lvUsers.SelectedItems.Count == 0) return;
            var user = _lvUsers.SelectedItems[0].Tag as UserAccount;
            if (user == null) return;

            ShowAssignDivDeptDialog(user);
        }

        private void ShowAssignDivDeptDialog(UserAccount user)
        {
            using (var dlg = new Form())
            {
                // ── Palette ────────────────────────────────────────────────────
                var navyDark = Color.FromArgb(25, 55, 105);
                var navyMid = Color.FromArgb(50, 90, 150);
                var navyLight = Color.FromArgb(215, 228, 248);
                var bodyBg = Color.FromArgb(243, 246, 251);
                var footerBg = Color.FromArgb(228, 234, 245);
                var savedBg = Color.FromArgb(232, 245, 233);
                var savedColor = Color.FromArgb(30, 110, 50);
                var lblColor = Color.FromArgb(55, 65, 90);
                var boldFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                var uiFont = new Font("Segoe UI", 10.5f);
                var lblFont = new Font("Segoe UI", 10f);

                dlg.Text = string.Format("กำหนดหน่วยงาน — {0}", user.DisplayName ?? user.ADUserName);
                dlg.Size = new Size(500, 520);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = bodyBg;
                dlg.Font = uiFont;

                // ── Header ────────────────────────────────────────────────────
                var pnlHdr = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = navyDark };
                var pnlIcon = new Panel { Size = new Size(36, 36), Location = new Point(14, 11), BackColor = Color.Transparent };
                pnlIcon.Paint += (s, pe) =>
                {
                    pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    pe.Graphics.FillEllipse(new SolidBrush(navyMid), 0, 0, 35, 35);
                    using (var f = new Font("Segoe UI", 15f))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        pe.Graphics.DrawString("🏢", f, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
                };
                pnlHdr.Controls.Add(pnlIcon);
                pnlHdr.Controls.Add(new Label
                {
                    Text = "กำหนดหน่วยงาน",
                    Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(58, 10),
                });
                pnlHdr.Controls.Add(new Label
                {
                    Text = user.DisplayName ?? user.ADUserName,
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(190, 210, 245),
                    AutoSize = true,
                    Location = new Point(58, 34),
                });

                // ── Body ─────────────────────────────────────────────────────
                var pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = bodyBg, AutoScroll = true };
                int lx = 14, lw = 130, tx = 152, fw = 300, rowH = 36;
                int y = 20;

                Action<string, int> addBar = (title, yy) =>
                {
                    var bar = new Panel { Left = lx, Top = yy, Width = lw + fw + 8, Height = 24, BackColor = navyLight };
                    bar.Controls.Add(new Label { Text = "  " + title, Font = boldFont, ForeColor = navyDark, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft });
                    pnlBody.Controls.Add(bar);
                };
                Action<string, int> addLbl = (text, yy) =>
                    pnlBody.Controls.Add(new Label { Text = text, Left = lx, Top = yy + 3, Width = lw, Font = lblFont, ForeColor = lblColor, TextAlign = ContentAlignment.MiddleRight });
                Func<int, ComboBox> addCmb = (yy) =>
                {
                    var c = new ComboBox { Left = tx, Top = yy, Width = fw, DropDownStyle = ComboBoxStyle.DropDownList, Font = uiFont, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
                    pnlBody.Controls.Add(c);
                    return c;
                };

                // ── กล่องหน่วยงานปัจจุบัน ──────────────────────────────────
                addBar("หน่วยงานปัจจุบัน", y); y += 28;
                var currentOrg0 = _allOrgs.FirstOrDefault(o => o.OrgUnitID == user.OrgUnitID);
                bool hasCurrentOrg = currentOrg0 != null;

                string pathText = hasCurrentOrg ? BuildOrgPath(currentOrg0) : "";

                var pnlCurrent = new Panel
                {
                    Left = lx,
                    Top = y,
                    Width = lw + fw + 8,
                    Height = 42,
                    BackColor = hasCurrentOrg ? savedBg : Color.FromArgb(245, 245, 250),
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(10, 8, 10, 8),
                };

                int currentLabelW = lw + fw - 18;
                var lblCurrentOrg = new Label
                {
                    Text = hasCurrentOrg
                        ? string.Format("✓  {0}  [{1}]", currentOrg0.OrgUnitNameTH ?? currentOrg0.Name, currentOrg0.OrgUnitCode)
                        : "ℹ  ยังไม่ได้กำหนดหน่วยงาน",
                    Left = 10,
                    Top = 20,
                    Width = currentLabelW,
                    Height = 24,
                    Font = new Font("Segoe UI", 9.5f, hasCurrentOrg ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = hasCurrentOrg ? savedColor : Color.Gray,
                    AutoSize = false,
                };

                // ใช้ preferred size จริงของ control เพื่อให้รองรับ DPI/ภาษาไทยได้แม่นยำ
                var currentPreferred = lblCurrentOrg.GetPreferredSize(new Size(currentLabelW, 0));
                lblCurrentOrg.Height = Math.Max(24, currentPreferred.Height + 2);
                pnlCurrent.Controls.Add(lblCurrentOrg);

                if (hasCurrentOrg)
                {
                    var lblPath = new Label
                    {
                        Text = pathText,
                        Left = 10,
                        Top = lblCurrentOrg.Bottom + 10,
                        Width = currentLabelW,
                        Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                        ForeColor = savedColor,
                        AutoSize = false,
                    };

                    var pathPreferred = lblPath.GetPreferredSize(new Size(currentLabelW, 0));
                    lblPath.Height = Math.Max(20, pathPreferred.Height + 2);
                    pnlCurrent.Controls.Add(lblPath);

                    // ความสูง panel ผูกกับขนาดข้อความจริง ลดปัญหาซ้อนเมื่อ DPI/ฟอนต์ต่างกัน
                    pnlCurrent.Height = lblPath.Bottom + 12;
                }
                else
                {
                    pnlCurrent.Height = lblCurrentOrg.Bottom + 10;
                }

                pnlBody.Controls.Add(pnlCurrent);
                y += pnlCurrent.Height + 22;

                // ── Section: เลือกหน่วยงานใหม่ ─────────────────────────────
                addBar("เลือกหน่วยงานใหม่  (SubCommittee → Division → Department)", y); y += 28;

                addLbl("SubCommittee:", y);
                var cmbSC = addCmb(y); y += rowH;

                addLbl("Division (ฝ่าย):", y);
                var cmbDiv = addCmb(y); cmbDiv.Enabled = false; y += rowH;

                addLbl("Department:", y);
                var cmbDept = addCmb(y); cmbDept.Enabled = false; y += rowH;

                // ── Summary breadcrumb ──────────────────────────────────────
                var lblSummary = new Label
                {
                    Left = tx,
                    Top = y,
                    Width = fw,
                    Height = 36,
                    ForeColor = navyMid,
                    Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                    AutoSize = false,
                };

                // โหลด SubCommittee
                var subCommittees = _allOrgs
                    .Where(o => o.OrgUnitType == 1 && o.IsActive)
                    .OrderBy(o => o.Name).ToList();

                cmbSC.Items.Add("— เลือก SubCommittee —");
                foreach (var sc in subCommittees)
                    cmbSC.Items.Add(string.Format("[{0}] {1}", sc.OrgUnitCode, sc.Name));
                cmbSC.SelectedIndex = 0;

                // Cascade: SC → Div
                cmbSC.SelectedIndexChanged += (s, ev) =>
                {
                    cmbDiv.Items.Clear();
                    cmbDept.Items.Clear();
                    cmbDiv.Enabled = false;
                    cmbDept.Enabled = false;
                    lblSummary.Text = "";

                    int scIdx = cmbSC.SelectedIndex - 1;
                    if (scIdx < 0) return;

                    var sc = subCommittees[scIdx];
                    var divs = _allOrgs
                        .Where(o => o.OrgUnitType == 2 && o.IsActive && o.ParentOrgUnitID == sc.OrgUnitID)
                        .OrderBy(o => o.Name).ToList();

                    cmbDiv.Tag = divs;
                    cmbDiv.Items.Add("— เลือก Division —");
                    foreach (var d in divs)
                        cmbDiv.Items.Add(string.Format("[{0}] {1}", d.OrgUnitCode, d.Name));
                    cmbDiv.SelectedIndex = 0;
                    cmbDiv.Enabled = divs.Count > 0;

                    lblSummary.Text = sc.Name;
                };

                // Cascade: Div → Dept
                cmbDiv.SelectedIndexChanged += (s, ev) =>
                {
                    cmbDept.Items.Clear();
                    cmbDept.Enabled = false;

                    var divs = cmbDiv.Tag as List<OrgUnit>;
                    int divIdx = cmbDiv.SelectedIndex - 1;
                    if (divs == null || divIdx < 0) return;

                    var div = divs[divIdx];
                    var depts = _allOrgs
                        .Where(o => o.OrgUnitType == 3 && o.IsActive && o.ParentOrgUnitID == div.OrgUnitID)
                        .OrderBy(o => o.Name).ToList();

                    cmbDept.Tag = depts;
                    cmbDept.Items.Add("— เลือก Department (ถ้ามี) —");
                    foreach (var d in depts)
                        cmbDept.Items.Add(string.Format("[{0}] {1}", d.OrgUnitCode, d.Name));
                    cmbDept.SelectedIndex = 0;
                    cmbDept.Enabled = true;

                    // อัปเดต summary
                    int scIdx = cmbSC.SelectedIndex - 1;
                    string scName = scIdx >= 0 ? subCommittees[scIdx].Name : "";
                    lblSummary.Text = string.Format("{0}  ›  {1}", scName, div.Name);
                };

                cmbDept.SelectedIndexChanged += (s, ev) =>
                {
                    var depts = cmbDept.Tag as List<OrgUnit>;
                    int deptIdx = cmbDept.SelectedIndex - 1;
                    if (depts == null || deptIdx < 0) return;

                    int scIdx = cmbSC.SelectedIndex - 1;
                    var divs = cmbDiv.Tag as List<OrgUnit>;
                    int divIdx = cmbDiv.SelectedIndex - 1;

                    string scName = scIdx >= 0 ? subCommittees[scIdx].Name : "";
                    string divName = (divs != null && divIdx >= 0) ? divs[divIdx].Name : "";
                    string deptName = depts[deptIdx].Name;
                    lblSummary.Text = string.Format("{0}  ›  {1}  ›  {2}", scName, divName, deptName);
                };

                // ── Pre-select ค่าปัจจุบัน ────────────────────────────────────
                // หา OrgUnit ปัจจุบันของ user แล้ว pre-select cascade
                var currentOrg = _allOrgs.FirstOrDefault(o => o.OrgUnitID == user.OrgUnitID);
                if (currentOrg != null)
                {
                    if (currentOrg.OrgUnitType == 1)
                    {
                        int idx = subCommittees.FindIndex(o => o.OrgUnitID == currentOrg.OrgUnitID);
                        if (idx >= 0) cmbSC.SelectedIndex = idx + 1;
                    }
                    else if (currentOrg.OrgUnitType == 2)
                    {
                        // หา parent SC แล้ว select SC ก่อน จากนั้น select Div
                        int scIdx = subCommittees.FindIndex(o => o.OrgUnitID == currentOrg.ParentOrgUnitID);
                        if (scIdx >= 0)
                        {
                            cmbSC.SelectedIndex = scIdx + 1;
                            var divs = cmbDiv.Tag as List<OrgUnit>;
                            if (divs != null)
                            {
                                int divIdx = divs.FindIndex(o => o.OrgUnitID == currentOrg.OrgUnitID);
                                if (divIdx >= 0) cmbDiv.SelectedIndex = divIdx + 1;
                            }
                        }
                    }
                    else if (currentOrg.OrgUnitType == 3)
                    {
                        var parentDiv = _allOrgs.FirstOrDefault(o => o.OrgUnitID == currentOrg.ParentOrgUnitID);
                        if (parentDiv != null)
                        {
                            int scIdx = subCommittees.FindIndex(o => o.OrgUnitID == parentDiv.ParentOrgUnitID);
                            if (scIdx >= 0)
                            {
                                cmbSC.SelectedIndex = scIdx + 1;
                                var divs = cmbDiv.Tag as List<OrgUnit>;
                                if (divs != null)
                                {
                                    int divIdx = divs.FindIndex(o => o.OrgUnitID == parentDiv.OrgUnitID);
                                    if (divIdx >= 0)
                                    {
                                        cmbDiv.SelectedIndex = divIdx + 1;
                                        var depts = cmbDept.Tag as List<OrgUnit>;
                                        if (depts != null)
                                        {
                                            int deptIdx = depts.FindIndex(o => o.OrgUnitID == currentOrg.OrgUnitID);
                                            if (deptIdx >= 0) cmbDept.SelectedIndex = deptIdx + 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                pnlBody.Controls.Add(lblSummary);

                // ── Footer ────────────────────────────────────────────────────
                var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = footerBg };
                var btnOk = new Button
                {
                    Text = "บันทึก",
                    Size = new Size(110, 36),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = navyDark,
                    ForeColor = Color.White,
                    Font = boldFont,
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
                pnlFoot.Resize += (s, ev) =>
                {
                    btnOk.Location = new Point(pnlFoot.Width - btnOk.Width - btnCan.Width - 20, 9);
                    btnCan.Location = new Point(pnlFoot.Width - btnCan.Width - 10, 9);
                };
                btnOk.Location = new Point(500 - btnOk.Width - btnCan.Width - 20, 9);
                btnCan.Location = new Point(500 - btnCan.Width - 10, 9);
                pnlFoot.Controls.Add(btnOk);
                pnlFoot.Controls.Add(btnCan);

                btnOk.Click += (s, ev) =>
                {
                    // หา OrgUnit ที่เลือก — ใช้ Dept ถ้ามี, ไม่งั้นใช้ Div, ไม่งั้นใช้ SC
                    OrgUnit selected = null;

                    var depts = cmbDept.Tag as List<OrgUnit>;
                    int deptIdx = cmbDept.SelectedIndex - 1;
                    if (depts != null && deptIdx >= 0)
                        selected = depts[deptIdx];

                    if (selected == null)
                    {
                        var divs = cmbDiv.Tag as List<OrgUnit>;
                        int divIdx = cmbDiv.SelectedIndex - 1;
                        if (divs != null && divIdx >= 0)
                            selected = divs[divIdx];
                    }

                    if (selected == null)
                    {
                        int scIdx = cmbSC.SelectedIndex - 1;
                        if (scIdx >= 0) selected = subCommittees[scIdx];
                    }

                    if (selected == null)
                    {
                        MessageBox.Show("กรุณาเลือกหน่วยงานอย่างน้อย SubCommittee", "แจ้งเตือน");
                        return;
                    }

                    try
                    {
                        int oldOrgId = user.OrgUnitID;
                        user.OrgUnitID = selected.OrgUnitID;
                        _userAdminController.UpdateUser(user);
                        if (_auditLogController != null)
                            _auditLogController.Log(new AuditEntry
                            {
                                Module = "UserAccess",
                                Action = "ASSIGN_ORG",
                                UserID = _currentUser.UserID,
                                TableName = "UserAccount",
                                RecordID = user.UserID,
                                OldValue = string.Format("OrgUnitID:{0}", oldOrgId),
                                NewValue = string.Format("OrgUnitID:{0} Name:{1}", selected.OrgUnitID, selected.Name),
                                MachineName = _currentUser.MachineName,
                                IPAddress = _currentUser.IPAddress,
                                SessionID = _currentUser.SessionID,
                                Result = 1,
                                AuditDate = DateTime.Now
                            });

                        var cached = _allUsers.FirstOrDefault(u => u.UserID == user.UserID);
                        if (cached != null) cached.OrgUnitID = selected.OrgUnitID;
                        RefreshVisibilityForUserIfSelected(user.UserID);

                        var sel = _lvUsers.SelectedItems[0];
                        sel.SubItems[2].Text = selected.OrgUnitType == 3 ? selected.Name : "";
                        sel.SubItems[3].Text = selected.OrgUnitType == 2 ? selected.Name :
                                               selected.OrgUnitType == 3
                                                   ? (_allOrgs.FirstOrDefault(o => o.OrgUnitID == selected.ParentOrgUnitID)?.Name ?? "")
                                                   : "";

                        SetStatus(_lblStatus,
                            string.Format("กำหนดหน่วยงาน '{0}' → '{1}' เรียบร้อย",
                                user.DisplayName ?? user.ADUserName, lblSummary.Text),
                            Color.DarkGreen);

                        dlg.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("บันทึกไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCan;
                dlg.Controls.Add(pnlHdr);    // DockStyle.Top
                dlg.Controls.Add(pnlFoot);   // DockStyle.Bottom
                dlg.Controls.Add(pnlBody);   // DockStyle.Fill
                dlg.ShowDialog(this);
            }
        }

        private void BtnAddUserToOrg_Click(object sender, EventArgs e)
        {
            var orgNode = _orgTree.SelectedNode;
            if (orgNode == null)
            {
                MessageBox.Show("กรุณาเลือกหน่วยงานจากต้นไม้ด้านซ้ายก่อน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var org = orgNode.Tag as OrgUnit;
            if (org == null) return;

            var alreadyInOrg = new HashSet<int>(
                _allUsers.Where(u => u.OrgUnitID == org.OrgUnitID).Select(u => u.UserID));
            var candidates = _allUsers.Where(u => u.IsActive && !alreadyInOrg.Contains(u.UserID)).ToList();

            if (candidates.Count == 0)
            {
                MessageBox.Show("ไม่มีผู้ใช้ที่พร้อมเพิ่มเข้าหน่วยงานนี้", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var chosen = PickUserFromList(candidates,
                string.Format("เพิ่มผู้ใช้ใน: {0}", org.OrgUnitNameTH ?? org.OrgUnitCode));
            if (chosen == null) return;

            int oldOrgId = chosen.OrgUnitID;
            chosen.OrgUnitID = org.OrgUnitID;
            try
            {
                _userAdminController.UpdateUser(chosen);
                if (_auditLogController != null)
                    _auditLogController.Log(new AuditEntry
                    {
                        Module = "UserAccess",
                        Action = "ADD_USER_TO_ORG",
                        UserID = _currentUser.UserID,
                        TableName = "UserAccount",
                        RecordID = chosen.UserID,
                        OldValue = string.Format("OrgUnitID:{0}", oldOrgId),
                        NewValue = string.Format("OrgUnitID:{0} Name:{1}",
                                         org.OrgUnitID, org.OrgUnitNameTH ?? org.OrgUnitCode),
                        MachineName = _currentUser.MachineName,
                        IPAddress = _currentUser.IPAddress,
                        SessionID = _currentUser.SessionID,
                        Result = 1,
                        AuditDate = DateTime.Now
                    });

                var cached = _allUsers.FirstOrDefault(u => u.UserID == chosen.UserID);
                if (cached != null) cached.OrgUnitID = org.OrgUnitID;

                if (_userAdminController != null)
                    _allUsers = (_userAdminController.GetAllUsers() ?? Enumerable.Empty<UserAccount>()).ToList();
                LoadUsersForOrgNode(org);
                RefreshVisibilityForUserIfSelected(chosen.UserID);

                SetStatus(_lblOrgStatus,
                    string.Format("เพิ่ม '{0}' เข้า '{1}' เรียบร้อย",
                        chosen.DisplayName ?? chosen.ADUserName,
                        org.OrgUnitNameTH ?? org.OrgUnitCode),
                    Color.DarkGreen);
            }
            catch (Exception ex)
            {
                chosen.OrgUnitID = oldOrgId;
                SetStatus(_lblOrgStatus, "บันทึกไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        private UserAccount PickUserFromList(List<UserAccount> candidates, string title)
        {
            UserAccount result = null;
            var navyDark = Color.FromArgb(25, 55, 105);
            var bodyBg = Color.FromArgb(243, 246, 251);

            using (var dlg = new Form())
            {
                dlg.Text = title;
                dlg.Size = new Size(500, 480);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Font = new Font("Segoe UI", 10F);
                dlg.BackColor = bodyBg;

                var pnlHdr = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = navyDark, Padding = new Padding(14, 0, 0, 0) };
                pnlHdr.Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    Text = title,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                });

                var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.White, Padding = new Padding(8, 6, 8, 0) };
                var lblSearch = new Label { Text = "ค้นหา:", Location = new Point(8, 8), Size = new Size(54, 24), TextAlign = ContentAlignment.MiddleRight };
                var txtFilter = new TextBox { Location = new Point(66, 6), Size = new Size(410, 24), Font = new Font("Segoe UI", 10F) };
                pnlSearch.Controls.Add(lblSearch);
                pnlSearch.Controls.Add(txtFilter);

                var lv = new ListView
                {
                    Dock = DockStyle.Fill,
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true,
                    HideSelection = false,
                    MultiSelect = false
                };
                lv.Columns.Add("ชื่อ-นามสกุล", 170);
                lv.Columns.Add("Username", 130);
                lv.Columns.Add("แผนก/ฝ่าย", 160);

                Action populate = () =>
                {
                    string q = txtFilter.Text.Trim().ToLower();
                    lv.BeginUpdate();
                    lv.Items.Clear();
                    foreach (var u in candidates)
                    {
                        if (!string.IsNullOrEmpty(q) &&
                            !(u.DisplayName ?? "").ToLower().Contains(q) &&
                            !(u.ADUserName ?? "").ToLower().Contains(q) &&
                            !(u.DepartmentName ?? "").ToLower().Contains(q) &&
                            !(u.DivisionName ?? "").ToLower().Contains(q))
                            continue;
                        var li = new ListViewItem(u.DisplayName ?? u.ADUserName);
                        li.SubItems.Add(u.ADUserName ?? "");
                        li.SubItems.Add(u.DepartmentName ?? u.DivisionName ?? "");
                        li.Tag = u;
                        lv.Items.Add(li);
                    }
                    lv.EndUpdate();
                };
                populate();
                txtFilter.TextChanged += (s, e2) => populate();

                var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.FromArgb(228, 234, 245), Padding = new Padding(8, 8, 8, 0) };
                var lblCount = new Label { Location = new Point(8, 14), Size = new Size(200, 20), ForeColor = Color.Gray };
                var btnOk = new Button
                {
                    Text = "✔ เพิ่ม",
                    Location = new Point(306, 6),
                    Size = new Size(90, 28),
                    Enabled = false,
                    BackColor = navyDark,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                };
                btnOk.FlatAppearance.BorderSize = 0;
                var btnCan = new Button
                {
                    Text = "ยกเลิก",
                    Location = new Point(404, 6),
                    Size = new Size(80, 28),
                    DialogResult = DialogResult.Cancel,
                    FlatStyle = FlatStyle.Flat,
                };
                pnlFoot.Controls.AddRange(new Control[] { lblCount, btnOk, btnCan });

                lv.SelectedIndexChanged += (s, e2) =>
                {
                    bool sel = lv.SelectedItems.Count > 0;
                    btnOk.Enabled = sel;
                    lblCount.Text = sel ? string.Format("เลือก: {0}", (lv.SelectedItems[0].Tag as UserAccount)?.ADUserName) : "";
                };
                Action confirm = () =>
                {
                    if (lv.SelectedItems.Count == 0) return;
                    result = lv.SelectedItems[0].Tag as UserAccount;
                    dlg.DialogResult = DialogResult.OK;
                };
                btnOk.Click += (s, e2) => confirm();
                lv.DoubleClick += (s, e2) => confirm();

                dlg.CancelButton = btnCan;
                dlg.Controls.Add(lv);
                dlg.Controls.Add(pnlFoot);
                dlg.Controls.Add(pnlSearch);
                dlg.Controls.Add(pnlHdr);
                dlg.ShowDialog(this);
            }
            return result;
        }

        private void BtnChangeOrg_Click(object sender, EventArgs e)
        {
            if (_lvOrgUsers.SelectedItems.Count == 0) return;
            var user = _lvOrgUsers.SelectedItems[0].Tag as UserAccount;
            if (user == null) return;

            OrgUnit chosen = PickOrgUnit();
            if (chosen == null || chosen.OrgUnitID == user.OrgUnitID) return;

            int oldOrgId = user.OrgUnitID;
            user.OrgUnitID = chosen.OrgUnitID;
            try
            {
                _userAdminController.UpdateUser(user);
                if (_auditLogController != null)
                    _auditLogController.Log(new AuditEntry
                    {
                        Module = "UserAccess",
                        Action = "CHANGE_ORG",
                        UserID = _currentUser.UserID,
                        TableName = "UserAccount",
                        RecordID = user.UserID,
                        OldValue = string.Format("OrgUnitID:{0}", oldOrgId),
                        NewValue = string.Format("OrgUnitID:{0} Name:{1}",
                                         chosen.OrgUnitID, chosen.OrgUnitNameTH ?? chosen.OrgUnitCode),
                        MachineName = _currentUser.MachineName,
                        IPAddress = _currentUser.IPAddress,
                        SessionID = _currentUser.SessionID,
                        Result = 1,
                        AuditDate = DateTime.Now
                    });
                _lvOrgUsers.Items.Remove(_lvOrgUsers.SelectedItems[0]);
                var cached = _allUsers.FirstOrDefault(u => u.UserID == user.UserID);
                if (cached != null) cached.OrgUnitID = chosen.OrgUnitID;
                RefreshVisibilityForUserIfSelected(user.UserID);
                SetStatus(_lblOrgStatus,
                    string.Format("ย้าย '{0}' ไปยัง '{1}' เรียบร้อย",
                        user.DisplayName ?? user.ADUserName,
                        chosen.OrgUnitNameTH ?? chosen.OrgUnitCode),
                    Color.DarkGreen);
            }
            catch (Exception ex)
            {
                user.OrgUnitID = oldOrgId;
                SetStatus(_lblOrgStatus, "บันทึกไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Org picker dialog
        // ─────────────────────────────────────────────────────────────────────

        private OrgUnit PickOrgUnit()
        {
            OrgUnit result = null;
            using (var dlg = new Form())
            {
                dlg.Text = "เลือกหน่วยงาน";
                dlg.Size = new Size(380, 510);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Font = new Font("Segoe UI", 10F);

                var lbl = new Label { Text = "เลือกหน่วยงานใหม่:", Left = 12, Top = 12, Width = 340 };
                var tree = new TreeView { Left = 12, Top = 36, Width = 340, Height = 380, ShowLines = true };

                tree.BeginUpdate();
                foreach (var root in _allOrgs.Where(o => !o.ParentOrgUnitID.HasValue || o.ParentOrgUnitID == 0))
                    tree.Nodes.Add(BuildPickerNode(root));
                tree.ExpandAll();
                tree.EndUpdate();

                var btnOk = new Button { Text = "บันทึก", Left = 12, Top = 428, Width = 90, Height = 28 };
                var btnCan = new Button { Text = "ยกเลิก", Left = 112, Top = 428, Width = 90, Height = 28, DialogResult = DialogResult.Cancel };
                btnOk.Click += (bs, be) =>
                {
                    if (tree.SelectedNode == null) { MessageBox.Show("กรุณาเลือกหน่วยงาน", "แจ้งเตือน"); return; }
                    result = tree.SelectedNode.Tag as OrgUnit;
                    dlg.DialogResult = DialogResult.OK;
                };
                dlg.CancelButton = btnCan;
                dlg.Controls.AddRange(new Control[] { lbl, tree, btnOk, btnCan });
                dlg.ShowDialog(this);
            }
            return result;
        }

        private TreeNode BuildPickerNode(OrgUnit org)
        {
            var node = new TreeNode(org.OrgUnitNameTH ?? org.OrgUnitCode) { Tag = org };
            foreach (var child in _allOrgs.Where(o => o.ParentOrgUnitID == org.OrgUnitID))
                node.Nodes.Add(BuildPickerNode(child));
            return node;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Utility
        // ─────────────────────────────────────────────────────────────────────

        private string BuildOrgPath(OrgUnit org)
        {
            if (org == null) return "";
            var parts = new System.Collections.Generic.List<string>();
            var current = org;
            while (current != null)
            {
                parts.Insert(0, current.OrgUnitNameTH ?? current.Name ?? current.OrgUnitCode);
                if (!current.ParentOrgUnitID.HasValue || current.ParentOrgUnitID == 0) break;
                current = _allOrgs.FirstOrDefault(o => o.OrgUnitID == current.ParentOrgUnitID);
            }
            return string.Join("  ›  ", parts);
        }

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

        private static void SetStatus(Label lbl, string text, Color color)
        {
            if (lbl == null) return;
            lbl.Text = text;
            lbl.ForeColor = color;
        }

        private void _btnAssignOrg_Click(object sender, EventArgs e)
        {

        }
    }
}
