using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Admin form for managing user access rights.
    /// Tab 1 (สิทธิ์ผู้ใช้): Change user role (Officer/SrOfficer/Manager/Admin), deactivate user.
    /// Tab 2 (หน่วยงานผู้ใช้): View org hierarchy and reassign a user's org unit.
    /// </summary>
    public class UserAccessForm : Form
    {
        private readonly IUserAccountRepository _userRepo;
        private readonly IOrgStructureService   _orgService;
        private readonly UserContext            _currentUser;

        // Tab 1 controls
        private TextBox  _txtSearch;
        private ComboBox _cmbRoleFilter;
        private ListView _lvUsers;
        private Button   _btnChangeRole;
        private Button   _btnDeactivate;
        private Button   _btnRefresh;
        private Label    _lblStatus;

        // Tab 2 controls
        private TreeView _orgTree;
        private ListView _lvOrgUsers;
        private Button   _btnChangeOrg;
        private Label    _lblOrgStatus;

        // Data cache
        private List<UserAccount> _allUsers = new List<UserAccount>();
        private List<OrgUnit>     _allOrgs  = new List<OrgUnit>();

        public UserAccessForm(
            IUserAccountRepository userRepo,
            IOrgStructureService   orgService,
            UserContext            currentUser)
        {
            _userRepo    = userRepo;
            _orgService  = orgService;
            _currentUser = currentUser;

            BuildUI();
            LoadData();
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI Construction
        // ─────────────────────────────────────────────────────────────────────

        private void BuildUI()
        {
            Text            = "จัดการสิทธิ์การเข้าถึง";
            Size            = new Size(960, 680);
            StartPosition   = FormStartPosition.CenterScreen;
            MinimumSize     = new Size(800, 560);
            Font            = new Font("Segoe UI", 10F);
            BackColor       = Color.White;

            // Header banner
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = Color.FromArgb(31, 56, 100),
                Padding   = new Padding(14, 0, 0, 0)
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "⚙  จัดการสิทธิ์การเข้าถึง",
                Dock      = DockStyle.Fill,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            });

            // TabControl
            var tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            var tabUsers   = new TabPage("  สิทธิ์ผู้ใช้  ");
            var tabOrg     = new TabPage("  หน่วยงานผู้ใช้  ");
            tabControl.TabPages.Add(tabUsers);
            tabControl.TabPages.Add(tabOrg);

            BuildUserTab(tabUsers);
            BuildOrgTab(tabOrg);

            Controls.Add(tabControl);
            Controls.Add(pnlHeader);
        }

        // ── Tab 1: User Role Management ───────────────────────────────────────

        private void BuildUserTab(TabPage tab)
        {
            tab.Padding = new Padding(8);

            // Search bar (top)
            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(4, 8, 4, 0) };

            var lblFind   = new Label { Text = "ค้นหา:", Left = 0,   Top = 12, Width = 52, TextAlign = ContentAlignment.MiddleRight };
            _txtSearch    = new TextBox { Left = 56, Top = 8, Width = 200 };
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) FilterUsers(); };

            var lblRole    = new Label { Text = "บทบาท:", Left = 270, Top = 12, Width = 52, TextAlign = ContentAlignment.MiddleRight };
            _cmbRoleFilter = new ComboBox { Left = 326, Top = 8, Width = 145, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbRoleFilter.Items.AddRange(new object[] { "ทุกบทบาท", "Officer (1)", "SrOfficer (2)", "Manager (3)", "Admin (4)" });
            _cmbRoleFilter.SelectedIndex = 0;
            _cmbRoleFilter.SelectedIndexChanged += (s, e) => FilterUsers();

            var btnFind = new Button { Text = "ค้นหา", Left = 482, Top = 6, Width = 80, Height = 28 };
            btnFind.Click += (s, e) => FilterUsers();

            pnlSearch.Controls.AddRange(new Control[] { lblFind, _txtSearch, lblRole, _cmbRoleFilter, btnFind });

            // Action bar (bottom)
            var pnlBtns    = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(4, 8, 4, 0) };
            _btnChangeRole = new Button { Text = "เปลี่ยนบทบาท",  Left = 0,   Top = 8, Width = 130, Height = 28, Enabled = false };
            _btnDeactivate = new Button { Text = "ปิดใช้งาน",      Left = 138, Top = 8, Width = 110, Height = 28, Enabled = false };
            _btnRefresh    = new Button { Text = "รีเฟรช",         Left = 256, Top = 8, Width = 80,  Height = 28 };
            _lblStatus     = new Label  { Left = 348, Top = 14, Width = 430, ForeColor = Color.Gray };

            _btnChangeRole.Click += BtnChangeRole_Click;
            _btnDeactivate.Click += BtnDeactivate_Click;
            _btnRefresh.Click    += (s, e) => LoadData();

            pnlBtns.Controls.AddRange(new Control[] { _btnChangeRole, _btnDeactivate, _btnRefresh, _lblStatus });

            // Users list view (fills centre)
            _lvUsers = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                MultiSelect   = false,
                HideSelection = false
            };
            _lvUsers.Columns.Add("ชื่อ-นามสกุล",   175);
            _lvUsers.Columns.Add("ชื่อผู้ใช้ AD",    145);
            _lvUsers.Columns.Add("แผนก",             160);
            _lvUsers.Columns.Add("ฝ่าย",              145);
            _lvUsers.Columns.Add("บทบาท",             110);
            _lvUsers.Columns.Add("สถานะ",              80);
            _lvUsers.SelectedIndexChanged += LvUsers_SelectionChanged;
            _lvUsers.DoubleClick          += BtnChangeRole_Click;

            tab.Controls.Add(_lvUsers);
            tab.Controls.Add(pnlBtns);
            tab.Controls.Add(pnlSearch);
        }

        // ── Tab 2: Org Unit Assignment ────────────────────────────────────────

        private void BuildOrgTab(TabPage tab)
        {
            tab.Padding = new Padding(8);

            var split = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                Orientation      = Orientation.Vertical,
                SplitterDistance = 310
            };

            // Left: org tree
            var pnlLeft = new Panel { Dock = DockStyle.Fill };
            pnlLeft.Controls.Add(new Label
            {
                Text      = "โครงสร้างองค์กร",
                Dock      = DockStyle.Top,
                Height    = 28,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            });
            _orgTree = new TreeView { Dock = DockStyle.Fill, ShowLines = true, ShowPlusMinus = true, HideSelection = false };
            _orgTree.AfterSelect += OrgTree_AfterSelect;
            pnlLeft.Controls.Add(_orgTree);

            // Right: users in selected org unit
            var pnlRight = new Panel { Dock = DockStyle.Fill };
            pnlRight.Controls.Add(new Label
            {
                Text      = "ผู้ใช้ในหน่วยงาน",
                Dock      = DockStyle.Top,
                Height    = 28,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            });

            _lvOrgUsers = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                MultiSelect   = false,
                HideSelection = false
            };
            _lvOrgUsers.Columns.Add("ชื่อ-นามสกุล",  190);
            _lvOrgUsers.Columns.Add("ชื่อผู้ใช้ AD",   150);
            _lvOrgUsers.Columns.Add("บทบาท",            110);
            _lvOrgUsers.Columns.Add("สถานะ",              80);
            _lvOrgUsers.SelectedIndexChanged += (s, e) =>
                _btnChangeOrg.Enabled = _lvOrgUsers.SelectedItems.Count > 0;

            var pnlOrgBtns = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(4, 8, 4, 0) };
            _btnChangeOrg  = new Button { Text = "เปลี่ยนหน่วยงาน", Left = 0,   Top = 8, Width = 145, Height = 28, Enabled = false };
            _lblOrgStatus  = new Label  { Left = 155, Top = 14, Width = 360, ForeColor = Color.Gray };
            _btnChangeOrg.Click += BtnChangeOrg_Click;
            pnlOrgBtns.Controls.AddRange(new Control[] { _btnChangeOrg, _lblOrgStatus });

            pnlRight.Controls.Add(_lvOrgUsers);
            pnlRight.Controls.Add(pnlOrgBtns);

            split.Panel1.Controls.Add(pnlLeft);
            split.Panel2.Controls.Add(pnlRight);
            tab.Controls.Add(split);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Data Loading
        // ─────────────────────────────────────────────────────────────────────

        private void LoadData()
        {
            try
            {
                _allUsers = (_userRepo != null
                    ? _userRepo.GetAll() ?? Enumerable.Empty<UserAccount>()
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
            string search  = _txtSearch      != null ? _txtSearch.Text.Trim().ToLower() : "";
            int    roleIdx = _cmbRoleFilter  != null ? _cmbRoleFilter.SelectedIndex : 0;

            IEnumerable<UserAccount> result = _allUsers;

            if (!string.IsNullOrEmpty(search))
                result = result.Where(u =>
                    (u.DisplayName  ?? "").ToLower().Contains(search) ||
                    (u.ADUserName   ?? "").ToLower().Contains(search) ||
                    (u.EmployeeCode ?? "").ToLower().Contains(search));

            // roleIdx 0 = all, 1 = Officer (AppRole 1) … 4 = Admin (AppRole 4)
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
            foreach (var root in _allOrgs.Where(o => !o.ParentOrgUnitID.HasValue || o.ParentOrgUnitID == 0))
                _orgTree.Nodes.Add(BuildOrgTreeNode(root));
            _orgTree.ExpandAll();
            _orgTree.EndUpdate();
        }

        private TreeNode BuildOrgTreeNode(OrgUnit org)
        {
            string prefix = org.OrgUnitType == 1 ? "▶ " : org.OrgUnitType == 2 ? "  ◆ " : "    • ";
            var node = new TreeNode(prefix + (org.OrgUnitNameTH ?? org.OrgUnitCode)) { Tag = org };
            foreach (var child in _allOrgs.Where(o => o.ParentOrgUnitID == org.OrgUnitID))
                node.Nodes.Add(BuildOrgTreeNode(child));
            return node;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event Handlers
        // ─────────────────────────────────────────────────────────────────────

        private void LvUsers_SelectionChanged(object sender, EventArgs e)
        {
            bool sel = _lvUsers.SelectedItems.Count > 0;
            _btnChangeRole.Enabled = sel;
            if (sel)
            {
                var u = _lvUsers.SelectedItems[0].Tag as UserAccount;
                _btnDeactivate.Enabled = u != null && u.IsActive;
            }
            else
            {
                _btnDeactivate.Enabled = false;
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
                    Text     = string.Format("ผู้ใช้: {0}\nบทบาทปัจจุบัน: {1}",
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
                    _userRepo.Update(user);
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
                _userRepo.Deactivate(user.UserID);
                user.IsActive = false;
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
                _userRepo.Update(user);
                // Remove from the current org view
                _lvOrgUsers.Items.Remove(_lvOrgUsers.SelectedItems[0]);
                // Update cache so refresh is consistent
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
