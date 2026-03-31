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
    /// Admin form for managing user accounts — create, edit role, activate/deactivate.
    /// </summary>
    public partial class UserManagementForm : Form
    {
        private readonly IUserAccountRepository _userRepo;
        private readonly UserContext            _currentUser;
        private readonly IAuditService          _auditService;

        private List<UserAccount> _allUsers = new List<UserAccount>();

        public UserManagementForm(IUserAccountRepository userRepo, UserContext currentUser,
                                   IAuditService auditService = null)
        {
            _userRepo     = userRepo;
            _currentUser  = currentUser;
            _auditService = auditService;

            InitializeComponent();
            WireEvents();
            LoadUsers();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event wiring
        // ─────────────────────────────────────────────────────────────────────

        private void WireEvents()
        {
            txtSearch.KeyDown                  += (s, e) => { if (e.KeyCode == Keys.Enter) ApplyFilter(); };
            cmbStatus.SelectedIndexChanged     += (s, e) => ApplyFilter();
            btnSearch.Click                    += (s, e) => ApplyFilter();
            btnCreate.Click                    += BtnCreate_Click;
            btnEdit.Click                      += BtnEdit_Click;
            btnToggle.Click                    += BtnToggle_Click;
            btnRefresh.Click                   += (s, e) => LoadUsers();
            lvUsers.SelectedIndexChanged       += LvUsers_SelectionChanged;
            lvUsers.DoubleClick                += BtnEdit_Click;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Data
        // ─────────────────────────────────────────────────────────────────────

        private void LoadUsers()
        {
            try
            {
                _allUsers = (_userRepo != null
                    ? _userRepo.GetAll() ?? Enumerable.Empty<UserAccount>()
                    : Enumerable.Empty<UserAccount>()).ToList();

                ApplyFilter();
                SetStatus(string.Format("โหลดสำเร็จ: {0} ผู้ใช้", _allUsers.Count), Color.Gray);
            }
            catch (Exception ex)
            {
                SetStatus("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        private void ApplyFilter()
        {
            string kw     = txtSearch.Text.Trim().ToLower();
            int    status = cmbStatus.SelectedIndex; // 0=all, 1=active, 2=inactive

            IEnumerable<UserAccount> result = _allUsers;

            if (!string.IsNullOrEmpty(kw))
                result = result.Where(u =>
                    (u.DisplayName   ?? "").ToLower().Contains(kw) ||
                    (u.ADUserName    ?? "").ToLower().Contains(kw) ||
                    (u.EmployeeCode  ?? "").ToLower().Contains(kw) ||
                    (u.DepartmentName ?? "").ToLower().Contains(kw));

            if (status == 1) result = result.Where(u => u.IsActive);
            if (status == 2) result = result.Where(u => !u.IsActive);

            PopulateList(result.ToList());
        }

        private void PopulateList(List<UserAccount> users)
        {
            lvUsers.BeginUpdate();
            lvUsers.Items.Clear();
            foreach (var u in users)
            {
                var item = new ListViewItem(u.UserID.ToString());
                item.SubItems.Add(u.ADUserName     ?? "");
                item.SubItems.Add(u.DisplayName    ?? "");
                item.SubItems.Add(u.EmployeeCode   ?? "");
                item.SubItems.Add(u.DepartmentName ?? "");
                item.SubItems.Add(u.DivisionName   ?? "");
                item.SubItems.Add(u.PositionName   ?? "");
                item.SubItems.Add(RoleName(u.AppRole));
                item.SubItems.Add(u.IsActive ? "ใช้งาน" : "ปิดใช้");
                item.SubItems.Add(u.LastLoginDate.HasValue
                    ? u.LastLoginDate.Value.ToString("dd/MM/yy HH:mm") : "—");
                item.Tag       = u;
                item.ForeColor = u.IsActive ? Color.Black : Color.DimGray;
                lvUsers.Items.Add(item);
            }
            lvUsers.EndUpdate();
            SetStatus(string.Format("{0} รายการ", users.Count), Color.Gray);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event Handlers
        // ─────────────────────────────────────────────────────────────────────

        private void LvUsers_SelectionChanged(object sender, EventArgs e)
        {
            bool sel = lvUsers.SelectedItems.Count > 0;
            btnEdit.Enabled = sel;
            if (sel)
            {
                var u = lvUsers.SelectedItems[0].Tag as UserAccount;
                btnToggle.Text    = (u != null && u.IsActive) ? "🔒 ปิดใช้งาน" : "🔓 เปิดใช้งาน";
                btnToggle.Enabled = u != null;
            }
            else
            {
                btnToggle.Enabled = false;
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
            if (lvUsers.SelectedItems.Count == 0) return;
            var user = lvUsers.SelectedItems[0].Tag as UserAccount;
            if (user == null) return;

            string action = user.IsActive ? "ปิดใช้งาน" : "เปิดใช้งาน";
            if (MessageBox.Show(
                    string.Format("{0} ผู้ใช้ '{1}' ใช่หรือไม่?",
                        action, user.DisplayName ?? user.ADUserName),
                    "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;

            try
            {
                bool wasActive = user.IsActive;
                if (user.IsActive)
                    _userRepo.Deactivate(user.UserID);
                else
                {
                    user.IsActive = true;
                    _userRepo.Update(user);
                }
                if (_auditService != null)
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "UserManagement",
                        Action      = wasActive ? "DEACTIVATE_USER" : "ACTIVATE_USER",
                        UserID      = _currentUser.UserID,
                        TableName   = "UserAccount",
                        RecordID    = user.UserID,
                        NewValue    = string.Format("User:{0} IsActive:{1}", user.ADUserName, !wasActive),
                        MachineName = _currentUser.MachineName,
                        IPAddress   = _currentUser.IPAddress,
                        SessionID   = _currentUser.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                LoadUsers();
                SetStatus(string.Format("{0} '{1}' เรียบร้อย",
                    action, user.DisplayName ?? user.ADUserName), Color.DarkGreen);
            }
            catch (Exception ex)
            {
                SetStatus("บันทึกไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Edit / Create dialog
        // ─────────────────────────────────────────────────────────────────────

        private Form BuildEditDialog(UserAccount user)
        {
            bool isNew = user == null;
            if (isNew) user = new UserAccount { IsActive = true, AppRole = 1 };

            var dlg = new Form
            {
                Text            = isNew ? "เพิ่มผู้ใช้ใหม่" : "แก้ไขข้อมูลผู้ใช้",
                Size            = new Size(420, 320),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false, MinimizeBox = false,
                Font            = new Font("Segoe UI", 10F)
            };

            int y = 16;
            Func<string, int, Control> addRow = (label, top) =>
            {
                dlg.Controls.Add(new Label { Text = label, Left = 12, Top = top + 3, Width = 110, TextAlign = ContentAlignment.MiddleRight });
                return null;
            };

            var txtAD      = AddField(dlg, "ชื่อผู้ใช้ AD:",   12, ref y, user.ADUserName   ?? "");
            var txtDisplay = AddField(dlg, "ชื่อ-นามสกุล:",    12, ref y, user.DisplayName  ?? "");
            var txtEmpCode = AddField(dlg, "รหัสพนักงาน:",     12, ref y, user.EmployeeCode ?? "");

            var lblRole = new Label { Text = "บทบาท:", Left = 12, Top = y + 3, Width = 110, TextAlign = ContentAlignment.MiddleRight };
            var cmbRole = new ComboBox { Left = 130, Top = y, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new object[] { "Officer (1)", "SrOfficer (2)", "Manager (3)", "Admin (4)" });
            cmbRole.SelectedIndex = Math.Max(0, Math.Min(3, user.AppRole - 1));
            dlg.Controls.Add(lblRole);
            dlg.Controls.Add(cmbRole);
            y += 34;

            var btnOk  = new Button { Text = "บันทึก",  Left = 130, Top = y, Width = 90, Height = 28, DialogResult = DialogResult.OK };
            var btnCan = new Button { Text = "ยกเลิก", Left = 228, Top = y, Width = 90, Height = 28, DialogResult = DialogResult.Cancel };
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCan;
            dlg.Controls.Add(btnOk);
            dlg.Controls.Add(btnCan);

            btnOk.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtAD.Text))
                { MessageBox.Show("กรุณาระบุชื่อผู้ใช้ AD", "แจ้งเตือน"); dlg.DialogResult = DialogResult.None; return; }

                user.ADUserName   = txtAD.Text.Trim();
                user.DisplayName  = txtDisplay.Text.Trim();
                user.EmployeeCode = txtEmpCode.Text.Trim();
                user.AppRole      = cmbRole.SelectedIndex + 1;

                try
                {
                    if (isNew) _userRepo.Create(user);
                    else       _userRepo.Update(user);
                    if (_auditService != null)
                        _auditService.Log(new AuditEntry
                        {
                            Module      = "UserManagement",
                            Action      = isNew ? "CREATE_USER" : "EDIT_USER",
                            UserID      = _currentUser.UserID,
                            TableName   = "UserAccount",
                            RecordID    = user.UserID,
                            NewValue    = string.Format("AD:{0} Display:{1} Role:{2}",
                                             user.ADUserName, user.DisplayName, user.AppRole),
                            MachineName = _currentUser.MachineName,
                            IPAddress   = _currentUser.IPAddress,
                            SessionID   = _currentUser.SessionID,
                            Result      = 1,
                            AuditDate   = DateTime.Now
                        });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("บันทึกไม่สำเร็จ: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dlg.DialogResult = DialogResult.None;
                }
            };

            return dlg;
        }

        private static TextBox AddField(Form dlg, string label, int left, ref int y, string value)
        {
            dlg.Controls.Add(new Label { Text = label, Left = left, Top = y + 3, Width = 110, TextAlign = ContentAlignment.MiddleRight });
            var txt = new TextBox { Left = left + 118, Top = y, Width = 240, Text = value };
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
                case 1:  return "Officer";
                case 2:  return "SrOfficer";
                case 3:  return "Manager";
                case 4:  return "Admin";
                default: return appRole.ToString();
            }
        }

        private void SetStatus(string text, Color color)
        {
            lblStatusBar.Text      = text;
            lblStatusBar.ForeColor = color;
        }
    }
}
