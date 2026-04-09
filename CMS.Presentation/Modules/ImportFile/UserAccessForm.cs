using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

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
        private readonly IUserAdminController    _userAdminController;
        private readonly IOrgStructureService     _orgService;
        private readonly IEmployeeApiService      _employeeApi;
        private readonly UserContext              _currentUser;
        private readonly IUserOrgVisibilityService _visibilityService;
        private readonly IAuditService            _auditService;

        private List<UserAccount> _allUsers = new List<UserAccount>();
        private List<OrgUnit>     _allOrgs  = new List<OrgUnit>();

        // Tab 3 controls (created programmatically)
        private TabPage  _tabVisibility;
        private ListView _lvVisibility;
        private Button   _btnGrantVisibility;
        private Button   _btnRevokeVisibility;
        private Label    _lblVisibilityStatus;
        private Label    _lblVisibilityUser;

        public UserAccessForm(
            IUserAdminController      userAdminController,
            IOrgStructureService      orgService,
            UserContext               currentUser,
            IEmployeeApiService       employeeApi       = null,
            IUserOrgVisibilityService visibilityService = null,
            IAuditService             auditService      = null)
        {
            _userAdminController = userAdminController;
            _orgService        = orgService;
            _currentUser       = currentUser;
            _employeeApi       = employeeApi;
            _visibilityService = visibilityService;
            _auditService      = auditService;

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
                Dock      = DockStyle.Top,
                Height    = 28,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "เลือกผู้ใช้จากแท็บ 'สิทธิ์ผู้ใช้' เพื่อดูสิทธิ์"
            };

            // ListView showing granted OrgUnits
            _lvVisibility = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                HideSelection = false,
                MultiSelect   = false
            };
            _lvVisibility.Columns.Add("หน่วยงาน",       280);
            _lvVisibility.Columns.Add("รหัสหน่วยงาน",   120);
            _lvVisibility.Columns.Add("ให้สิทธิ์โดย",   160);
            _lvVisibility.Columns.Add("วันที่ให้สิทธิ์", 140);

            // Bottom button panel
            var pnlVisBtns = new Panel { Dock = DockStyle.Bottom, Height = 44 };
            pnlVisBtns.Padding = new Padding(4, 8, 4, 0);

            _btnGrantVisibility = new Button
            {
                Text     = "➕ เพิ่มสิทธิ์",
                Location = new Point(0, 8),
                Size     = new Size(110, 28),
                Enabled  = false
            };

            _btnRevokeVisibility = new Button
            {
                Text     = "➖ ลบสิทธิ์",
                Location = new Point(118, 8),
                Size     = new Size(110, 28),
                Enabled  = false
            };

            _lblVisibilityStatus = new Label
            {
                Location  = new Point(240, 14),
                Size      = new Size(500, 23),
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
            _txtSearch.KeyDown                    += (s, e) => { if (e.KeyCode == Keys.Enter) FilterUsers(); };
            _cmbRoleFilter.SelectedIndexChanged   += (s, e) => FilterUsers();
            btnFind.Click                         += (s, e) => FilterUsers();
            _lvUsers.SelectedIndexChanged         += LvUsers_SelectionChanged;
            _lvUsers.DoubleClick                  += BtnChangeRole_Click;
            _btnChangeRole.Click                  += BtnChangeRole_Click;
            _btnDeactivate.Click                  += BtnDeactivate_Click;
            _btnAssignOrg.Click                   += BtnAssignOrg_Click;
            _btnRefresh.Click                     += (s, e) => LoadData();
            // Tab 2
            _orgTree.AfterSelect                  += OrgTree_AfterSelect;
            _lvOrgUsers.SelectedIndexChanged      += (s, e) =>
                _btnChangeOrg.Enabled = _lvOrgUsers.SelectedItems.Count > 0;
            _btnChangeOrg.Click                   += BtnChangeOrg_Click;
            // Tab 3
            _lvVisibility.SelectedIndexChanged    += (s, e) =>
                _btnRevokeVisibility.Enabled = _lvVisibility.SelectedItems.Count > 0;
            _btnGrantVisibility.Click             += BtnGrantVisibility_Click;
            _btnRevokeVisibility.Click            += BtnRevokeVisibility_Click;
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

                _allOrgs = (_orgService != null
                    ? _orgService.GetHierarchy() ?? Enumerable.Empty<OrgUnit>()
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
            string search  = _txtSearch.Text.Trim().ToLower();
            int    roleIdx = _cmbRoleFilter.SelectedIndex;

            IEnumerable<UserAccount> result = _allUsers;

            if (!string.IsNullOrEmpty(search))
                result = result.Where(u =>
                    (u.DisplayName  ?? "").ToLower().Contains(search) ||
                    (u.ADUserName   ?? "").ToLower().Contains(search) ||
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
                item.SubItems.Add(u.ADUserName     ?? "");
                item.SubItems.Add(u.DepartmentName ?? "");
                item.SubItems.Add(u.DivisionName   ?? "");
                item.SubItems.Add(RoleName(u.AppRole));
                item.SubItems.Add(u.IsActive ? "ใช้งาน" : "ปิดใช้");
                item.Tag       = u;
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
            if (_visibilityService != null)
            {
                try
                {
                    var visibleSet = new HashSet<int>(
                        _visibilityService.GetVisibleOrgUnits(_currentUser.UserID, _currentUser)
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
            _btnAssignOrg.Enabled  = sel;
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

            var users = _allUsers.Where(u => u.OrgUnitID == org.OrgUnitID).ToList();
            _lvOrgUsers.BeginUpdate();
            _lvOrgUsers.Items.Clear();
            foreach (var u in users)
            {
                var item = new ListViewItem(u.DisplayName ?? u.ADUserName);
                item.SubItems.Add(u.ADUserName ?? "");
                item.SubItems.Add(RoleName(u.AppRole));
                item.SubItems.Add(u.IsActive ? "ใช้งาน" : "ปิดใช้");
                item.Tag       = u;
                item.ForeColor = u.IsActive ? Color.Black : Color.DimGray;
                _lvOrgUsers.Items.Add(item);
            }
            _lvOrgUsers.EndUpdate();
            SetStatus(_lblOrgStatus,
                string.Format("{0}: {1} ผู้ใช้", org.OrgUnitNameTH ?? org.OrgUnitCode, users.Count),
                Color.Gray);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tab 3 — Visibility handlers
        // ─────────────────────────────────────────────────────────────────────

        private UserAccount GetSelectedUser()
        {
            if (_lvUsers.SelectedItems.Count == 0) return null;
            return _lvUsers.SelectedItems[0].Tag as UserAccount;
        }

        private void LoadVisibilityList(UserAccount user)
        {
            if (_tabVisibility == null || _lvVisibility == null) return;
            if (user == null) { ClearVisibilityList(); return; }

            _lblVisibilityUser.Text = string.Format("สิทธิ์มองเห็นหน่วยงานของ: {0}",
                user.DisplayName ?? user.ADUserName);
            _btnGrantVisibility.Enabled  = _visibilityService != null;
            _btnRevokeVisibility.Enabled = false;

            _lvVisibility.BeginUpdate();
            _lvVisibility.Items.Clear();

            if (_visibilityService != null)
            {
                try
                {
                    // Get explicit grants with GrantedBy and GrantedDate information
                    var grants = _visibilityService.GetExplicitGrantsForUser(user.UserID, _currentUser).ToList();
                    
                    // Build lookup for OrgUnit names
                    var orgLookup = _allOrgs.ToDictionary(o => o.OrgUnitID, o => o);
                    
                    // Build lookup for user names (GrantedBy)
                    var grantedByIds = grants.Select(g => g.GrantedBy).Distinct().ToList();
                    var userLookup = _allUsers
                        .Where(u => grantedByIds.Contains(u.UserID))
                        .ToDictionary(u => u.UserID, u => u.DisplayName ?? u.ADUserName);
                    
                    foreach (var grant in grants)
                    {
                        // Get OrgUnit info
                        OrgUnit org = null;
                        orgLookup.TryGetValue(grant.OrgUnitID, out org);
                        
                        string orgName = org != null ? (org.OrgUnitNameTH ?? org.OrgUnitCode) : grant.OrgUnitName ?? string.Format("ID:{0}", grant.OrgUnitID);
                        string orgCode = org != null ? (org.OrgUnitCode ?? "") : "";
                        
                        // Get GrantedBy user name
                        string grantedByName = "";
                        if (userLookup.TryGetValue(grant.GrantedBy, out grantedByName))
                        {
                            // Found in user list
                        }
                        else if (grant.GrantedBy == _currentUser.UserID)
                        {
                            grantedByName = _currentUser.ADUserName ?? string.Format("UserID:{0}", _currentUser.UserID);
                        }
                        else
                        {
                            grantedByName = string.Format("UserID:{0}", grant.GrantedBy);
                        }
                        
                        var item = new ListViewItem(orgName);
                        item.SubItems.Add(orgCode);
                        item.SubItems.Add(grantedByName);
                        item.SubItems.Add(grant.GrantedDate.ToString("dd/MM/yyyy HH:mm"));
                        item.Tag = org ?? (object)grant; // Store OrgUnit if available, otherwise grant
                        _lvVisibility.Items.Add(item);
                    }
                    
                    // If no explicit grants, show role-based fallback info
                    if (grants.Count == 0)
                    {
                        var visibleOrgs = _visibilityService.GetVisibleOrgUnits(user.UserID, _currentUser).ToList();
                        foreach (var org in visibleOrgs)
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
            if (_btnGrantVisibility  != null) _btnGrantVisibility.Enabled  = false;
            if (_btnRevokeVisibility != null) _btnRevokeVisibility.Enabled = false;
        }

        private void BtnGrantVisibility_Click(object sender, EventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null || _visibilityService == null) return;

            // Show OrgUnit picker — returns multiple selections via CheckedListBox
            var chosen = PickOrgUnitsFromFullHierarchy();
            if (chosen == null || chosen.Count == 0) return;

            var errors = new List<string>();
            foreach (var org in chosen)
            {
                try
                {
                    _visibilityService.GrantVisibility(user.UserID, org.OrgUnitID, _currentUser);
                    if (_auditService != null)
                        _auditService.Log(new AuditEntry
                        {
                            Module      = "UserAccess",
                            Action      = "GRANT_VISIBILITY",
                            UserID      = _currentUser.UserID,
                            TableName   = "UserOrgVisibility",
                            RecordID    = user.UserID,
                            NewValue    = string.Format("TargetUser:{0} OrgUnitID:{1} OrgName:{2}",
                                             user.ADUserName, org.OrgUnitID, org.OrgUnitNameTH ?? org.OrgUnitCode),
                            MachineName = _currentUser.MachineName,
                            IPAddress   = _currentUser.IPAddress,
                            SessionID   = _currentUser.SessionID,
                            Result      = 1,
                            AuditDate   = DateTime.Now
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
            if (user == null || _visibilityService == null) return;
            if (_lvVisibility.SelectedItems.Count == 0) return;

            var org = _lvVisibility.SelectedItems[0].Tag as OrgUnit;
            if (org == null) return;

            try
            {
                _visibilityService.RevokeVisibility(user.UserID, org.OrgUnitID, _currentUser);
                if (_auditService != null)
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "UserAccess",
                        Action      = "REVOKE_VISIBILITY",
                        UserID      = _currentUser.UserID,
                        TableName   = "UserOrgVisibility",
                        RecordID    = user.UserID,
                        OldValue    = string.Format("TargetUser:{0} OrgUnitID:{1} OrgName:{2}",
                                         user.ADUserName, org.OrgUnitID, org.OrgUnitNameTH ?? org.OrgUnitCode),
                        MachineName = _currentUser.MachineName,
                        IPAddress   = _currentUser.IPAddress,
                        SessionID   = _currentUser.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                LoadVisibilityList(user);
                MessageBox.Show(
                    string.Format("ลบสิทธิ์มองเห็น '{0}' ของ '{1}' เรียบร้อย",
                        org.OrgUnitNameTH ?? org.OrgUnitCode,
                        user.DisplayName ?? user.ADUserName),
                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus(_lblVisibilityStatus, "ลบสิทธิ์เรียบร้อย", Color.DarkGreen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ลบสิทธิ์ไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(_lblVisibilityStatus, "ลบสิทธิ์ไม่สำเร็จ: " + ex.Message, Color.Red);
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
            var allOrgs = (_orgService != null
                ? _orgService.GetHierarchy() ?? Enumerable.Empty<OrgUnit>()
                : _allOrgs).ToList();

            // Collect already-granted IDs — use explicit grants only (no role-based fallback)
            var alreadyGrantedIds = new HashSet<int>();
            if (_visibilityService != null)
            {
                var selectedUser = GetSelectedUser();
                if (selectedUser != null)
                {
                    try
                    {
                        foreach (var g in _visibilityService.GetExplicitGrantsForUser(selectedUser.UserID, _currentUser))
                            alreadyGrantedIds.Add(g.OrgUnitID);
                    }
                    catch { }
                }
            }

            using (var dlg = new Form())
            {
                dlg.Text            = "เลือกหน่วยงานที่จะให้สิทธิ์";
                dlg.Size            = new Size(440, 580);
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.Font            = new Font("Segoe UI", 9.5F);
                dlg.BackColor       = Color.FromArgb(243, 246, 251);

                // ── Header ─────────────────────────────────────────────────────
                var pnlHeader = new Panel
                {
                    Dock      = DockStyle.Top,
                    Height    = 52,
                    BackColor = Color.FromArgb(25, 55, 105),
                    Padding   = new Padding(14, 0, 14, 0),
                };
                var lblTitle = new Label
                {
                    Dock      = DockStyle.Fill,
                    Text      = "เลือกหน่วยงานที่จะให้สิทธิ์",
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                pnlHeader.Controls.Add(lblTitle);

                // ── TreeView (inside a white card panel) ──────────────────────
                var pnlCard = new Panel
                {
                    Dock        = DockStyle.Fill,
                    BackColor   = Color.White,
                    Margin      = new Padding(12),
                    Padding     = new Padding(2),
                };

                var tree = new TreeView
                {
                    Dock          = DockStyle.Fill,
                    CheckBoxes    = true,
                    ShowLines     = true,
                    ShowPlusMinus = true,
                    BorderStyle   = BorderStyle.None,
                    BackColor     = Color.White,
                    Font          = new Font("Segoe UI", 9.5F),
                    HotTracking   = true,
                    ItemHeight    = 22,
                };
                pnlCard.Controls.Add(tree);

                // Recursively build TreeNodes
                Func<OrgUnit, TreeNode> buildNode = null;
                buildNode = (org) =>
                {
                    string prefix = org.OrgUnitType == 1 ? "🏢 "
                                  : org.OrgUnitType == 2 ? "📂 "
                                  : "📋 ";
                    var node = new TreeNode(prefix + (org.OrgUnitNameTH ?? org.OrgUnitCode))
                    {
                        Tag     = org,
                        Checked = alreadyGrantedIds.Contains(org.OrgUnitID)
                    };
                    // Gray out already-granted nodes via ForeColor
                    if (alreadyGrantedIds.Contains(org.OrgUnitID))
                        node.ForeColor = Color.Gray;

                    foreach (var child in allOrgs
                        .Where(c => c.ParentOrgUnitID == org.OrgUnitID)
                        .OrderBy(c => c.SortOrder).ThenBy(c => c.Name))
                    {
                        node.Nodes.Add(buildNode(child));
                    }
                    return node;
                };

                tree.BeginUpdate();
                foreach (var root in allOrgs
                    .Where(o => !o.ParentOrgUnitID.HasValue || o.ParentOrgUnitID == 0)
                    .OrderBy(o => o.SortOrder).ThenBy(o => o.Name))
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
                    Dock      = DockStyle.Bottom,
                    Height    = 56,
                    BackColor = Color.FromArgb(235, 239, 246),
                    Padding   = new Padding(10),
                };
                var btnOk = new Button
                {
                    Text      = "เลือก",
                    Width     = 100, Height = 34,
                    BackColor = Color.FromArgb(25, 55, 105),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCan = new Button
                {
                    Text         = "ยกเลิก",
                    Width        = 90, Height = 34,
                    BackColor    = Color.White,
                    ForeColor    = Color.FromArgb(80, 80, 80),
                    FlatStyle    = FlatStyle.Flat,
                    Font         = new Font("Segoe UI", 9.5F),
                    DialogResult = DialogResult.Cancel,
                    Anchor       = AnchorStyles.Right | AnchorStyles.Top,
                };
                btnCan.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);

                // position buttons on the right
                pnlBtns.Controls.AddRange(new Control[] { btnOk, btnCan });
                pnlBtns.Layout += (ls, le) =>
                {
                    int y   = (pnlBtns.ClientSize.Height - btnOk.Height) / 2;
                    btnCan.Location = new Point(pnlBtns.ClientSize.Width - btnCan.Width - 8, y);
                    btnOk.Location  = new Point(btnCan.Left - btnOk.Width - 8, y);
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
            public OrgUnit OrgUnit     { get; set; }
            public string  DisplayText { get; set; }
            public override string ToString() { return DisplayText; }
        }

        private void BtnChangeRole_Click(object sender, EventArgs e)
        {
            if (_lvUsers.SelectedItems.Count == 0) return;
            var user = _lvUsers.SelectedItems[0].Tag as UserAccount;
            if (user == null) return;

            using (var dlg = new Form())
            {
                dlg.Text            = "เปลี่ยนบทบาทผู้ใช้";
                dlg.Size            = new Size(360, 180);
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.Font            = new Font("Segoe UI", 10F);

                var lbl = new Label
                {
                    Text   = string.Format("ผู้ใช้: {0}\nบทบาทปัจจุบัน: {1}",
                                user.DisplayName ?? user.ADUserName, RoleName(user.AppRole)),
                    Left = 12, Top = 12, Width = 320, Height = 48
                };
                var cmb = new ComboBox { Left = 12, Top = 64, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
                cmb.Items.AddRange(new object[] { "Officer (1)", "SrOfficer (2)", "Manager (3)", "Admin (4)" });
                cmb.SelectedIndex = Math.Max(0, Math.Min(3, user.AppRole - 1));

                var btnOk  = new Button { Text = "บันทึก",  Left = 12,  Top = 104, Width = 90, Height = 28, DialogResult = DialogResult.OK };
                var btnCan = new Button { Text = "ยกเลิก", Left = 112, Top = 104, Width = 90, Height = 28, DialogResult = DialogResult.Cancel };
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCan;
                dlg.Controls.AddRange(new Control[] { lbl, cmb, btnOk, btnCan });

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                int newRole = cmb.SelectedIndex + 1;
                if (newRole == user.AppRole) return;

                int oldRole  = user.AppRole;
                user.AppRole = newRole;
                try
                {
                    _userAdminController.UpdateUser(user);
                    if (_auditService != null)
                        _auditService.Log(new AuditEntry
                        {
                            Module      = "UserAccess",
                            Action      = "CHANGE_ROLE",
                            UserID      = _currentUser.UserID,
                            TableName   = "UserAccount",
                            RecordID    = user.UserID,
                            OldValue    = string.Format("Role:{0}", RoleName(oldRole)),
                            NewValue    = string.Format("Role:{0}", RoleName(newRole)),
                            MachineName = _currentUser.MachineName,
                            IPAddress   = _currentUser.IPAddress,
                            SessionID   = _currentUser.SessionID,
                            Result      = 1,
                            AuditDate   = DateTime.Now
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
                if (_auditService != null)
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "UserAccess",
                        Action      = "DEACTIVATE_USER",
                        UserID      = _currentUser.UserID,
                        TableName   = "UserAccount",
                        RecordID    = user.UserID,
                        NewValue    = string.Format("User:{0} IsActive:false", user.ADUserName),
                        MachineName = _currentUser.MachineName,
                        IPAddress   = _currentUser.IPAddress,
                        SessionID   = _currentUser.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                var sel = _lvUsers.SelectedItems[0];
                sel.SubItems[5].Text   = "ปิดใช้";
                sel.ForeColor          = Color.DimGray;
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
                dlg.Text            = string.Format("กำหนดหน่วยงาน — {0}", user.DisplayName ?? user.ADUserName);
                dlg.Size            = new Size(480, 460);
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.Font            = new Font("Segoe UI", 10F);

                // ── Header ────────────────────────────────────────────────────
                var pnlHdr = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(0, 51, 102) };
                pnlHdr.Controls.Add(new Label {
                    Text = "🏢  กำหนดหน่วยงาน (SubCommittee → Division → Department)",
                    Dock = DockStyle.Fill, ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0)
                });

                // ── SubCommittee ──────────────────────────────────────────────
                var lblSC  = new Label { Text = "SubCommittee:", Left = 12, Top = 52, Width = 120, TextAlign = ContentAlignment.MiddleRight };
                var cmbSC  = new ComboBox { Left = 140, Top = 50, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };

                // ── Division ──────────────────────────────────────────────────
                var lblDiv = new Label { Text = "Division (ฝ่าย):", Left = 12, Top = 96, Width = 120, TextAlign = ContentAlignment.MiddleRight };
                var cmbDiv = new ComboBox { Left = 140, Top = 94, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };

                // ── Department ────────────────────────────────────────────────
                var lblDept = new Label { Text = "Department (แผนก):", Left = 12, Top = 140, Width = 120, TextAlign = ContentAlignment.MiddleRight };
                var cmbDept = new ComboBox { Left = 140, Top = 138, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };

                // ── Summary label ─────────────────────────────────────────────
                var lblSummary = new Label {
                    Left = 12, Top = 185, Width = 440, Height = 40,
                    ForeColor = Color.FromArgb(0, 80, 160),
                    Font = new Font("Segoe UI", 9f, FontStyle.Italic)
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
                    cmbDiv.Enabled  = false;
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

                    int scIdx  = cmbSC.SelectedIndex - 1;
                    var divs   = cmbDiv.Tag as List<OrgUnit>;
                    int divIdx = cmbDiv.SelectedIndex - 1;

                    string scName   = scIdx  >= 0 ? subCommittees[scIdx].Name : "";
                    string divName  = (divs != null && divIdx >= 0) ? divs[divIdx].Name : "";
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

                // ── Footer ────────────────────────────────────────────────────
                var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.FromArgb(243, 243, 243) };
                var btnOk   = new Button {
                    Text = "✔ บันทึก", Size = new Size(90, 28),
                    BackColor = Color.FromArgb(0, 51, 102), ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Right | AnchorStyles.Top
                };
                btnOk.FlatAppearance.BorderSize = 0;
                var btnCan = new Button {
                    Text = "ยกเลิก", Size = new Size(80, 28), FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Right | AnchorStyles.Top
                };
                pnlFoot.Controls.Add(btnOk);
                pnlFoot.Controls.Add(btnCan);
                pnlFoot.Resize += (s, ev) => {
                    btnCan.Left = pnlFoot.ClientSize.Width - btnCan.Width - 8;
                    btnOk.Left  = btnCan.Left - btnOk.Width - 8;
                    btnOk.Top   = btnCan.Top = (pnlFoot.Height - btnOk.Height) / 2;
                };

                btnOk.Click += (s, ev) =>
                {
                    // หา OrgUnit ที่เลือก — ใช้ Dept ถ้ามี, ไม่งั้นใช้ Div, ไม่งั้นใช้ SC
                    OrgUnit selected = null;

                    var depts  = cmbDept.Tag as List<OrgUnit>;
                    int deptIdx = cmbDept.SelectedIndex - 1;
                    if (depts != null && deptIdx >= 0)
                        selected = depts[deptIdx];

                    if (selected == null)
                    {
                        var divs   = cmbDiv.Tag as List<OrgUnit>;
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
                        int oldOrgId   = user.OrgUnitID;
                        user.OrgUnitID = selected.OrgUnitID;
                        _userAdminController.UpdateUser(user);
                        if (_auditService != null)
                            _auditService.Log(new AuditEntry
                            {
                                Module      = "UserAccess",
                                Action      = "ASSIGN_ORG",
                                UserID      = _currentUser.UserID,
                                TableName   = "UserAccount",
                                RecordID    = user.UserID,
                                OldValue    = string.Format("OrgUnitID:{0}", oldOrgId),
                                NewValue    = string.Format("OrgUnitID:{0} Name:{1}", selected.OrgUnitID, selected.Name),
                                MachineName = _currentUser.MachineName,
                                IPAddress   = _currentUser.IPAddress,
                                SessionID   = _currentUser.SessionID,
                                Result      = 1,
                                AuditDate   = DateTime.Now
                            });

                        // อัปเดต cache
                        var cached = _allUsers.FirstOrDefault(u => u.UserID == user.UserID);
                        if (cached != null) cached.OrgUnitID = selected.OrgUnitID;

                        // อัปเดต ListView
                        var sel = _lvUsers.SelectedItems[0];
                        // แสดง path ใน column แผนก
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
                        MessageBox.Show("บันทึกไม่สำเร็จ: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                dlg.Controls.Add(lblSummary);
                dlg.Controls.Add(cmbDept);
                dlg.Controls.Add(lblDept);
                dlg.Controls.Add(cmbDiv);
                dlg.Controls.Add(lblDiv);
                dlg.Controls.Add(cmbSC);
                dlg.Controls.Add(lblSC);
                dlg.Controls.Add(pnlFoot);
                dlg.Controls.Add(pnlHdr);
                dlg.ShowDialog(this);
            }
        }

        private void BtnChangeOrg_Click(object sender, EventArgs e)
        {
            if (_lvOrgUsers.SelectedItems.Count == 0) return;
            var user = _lvOrgUsers.SelectedItems[0].Tag as UserAccount;
            if (user == null) return;

            OrgUnit chosen = PickOrgUnit();
            if (chosen == null || chosen.OrgUnitID == user.OrgUnitID) return;

            int oldOrgId   = user.OrgUnitID;
            user.OrgUnitID = chosen.OrgUnitID;
            try
            {
                _userAdminController.UpdateUser(user);
                if (_auditService != null)
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "UserAccess",
                        Action      = "CHANGE_ORG",
                        UserID      = _currentUser.UserID,
                        TableName   = "UserAccount",
                        RecordID    = user.UserID,
                        OldValue    = string.Format("OrgUnitID:{0}", oldOrgId),
                        NewValue    = string.Format("OrgUnitID:{0} Name:{1}",
                                         chosen.OrgUnitID, chosen.OrgUnitNameTH ?? chosen.OrgUnitCode),
                        MachineName = _currentUser.MachineName,
                        IPAddress   = _currentUser.IPAddress,
                        SessionID   = _currentUser.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                _lvOrgUsers.Items.Remove(_lvOrgUsers.SelectedItems[0]);
                var cached = _allUsers.FirstOrDefault(u => u.UserID == user.UserID);
                if (cached != null) cached.OrgUnitID = chosen.OrgUnitID;
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
                dlg.Text            = "เลือกหน่วยงาน";
                dlg.Size            = new Size(380, 510);
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.Font            = new Font("Segoe UI", 10F);

                var lbl  = new Label { Text = "เลือกหน่วยงานใหม่:", Left = 12, Top = 12, Width = 340 };
                var tree = new TreeView { Left = 12, Top = 36, Width = 340, Height = 380, ShowLines = true };

                tree.BeginUpdate();
                foreach (var root in _allOrgs.Where(o => !o.ParentOrgUnitID.HasValue || o.ParentOrgUnitID == 0))
                    tree.Nodes.Add(BuildPickerNode(root));
                tree.ExpandAll();
                tree.EndUpdate();

                var btnOk  = new Button { Text = "บันทึก",  Left = 12,  Top = 428, Width = 90, Height = 28 };
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

        private static string RoleName(int appRole)
        {
            switch (appRole)
            {
                case 1:  return "Officer";
                case 2:  return "SrOfficer";
                case 3:  return "Manager";
                case 4:  return "Admin";
                default: return appRole.ToString();
            }
        }

        private static void SetStatus(Label lbl, string text, Color color)
        {
            if (lbl == null) return;
            lbl.Text      = text;
            lbl.ForeColor = color;
        }
    }
}
