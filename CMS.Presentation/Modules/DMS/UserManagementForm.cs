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
        private readonly IUserAdminController        _userAdminController;
        private readonly UserContext                 _currentUser;
        private readonly IAuditLogController         _auditLogController;

        private List<UserAccount>      _allUsers    = new List<UserAccount>();
        private List<HrDivision>       _divisions   = new List<HrDivision>();
        private List<HrDepartment>     _departments = new List<HrDepartment>();
        private List<HrPosition>       _positions   = new List<HrPosition>();
        private List<HrPositionLevel>  _levels      = new List<HrPositionLevel>();

        public UserManagementForm(IUserAdminController userAdminController, UserContext currentUser,
                                   CMS.Services.Interfaces.IAuditService auditService = null)
        {
            _userAdminController = userAdminController;
            _currentUser  = currentUser;
            _auditLogController = new AuditLogController(auditService);

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

        private void LoadHrData()
        {
            // ── Phase 1: โหลดจาก HR master tables ──────────────────────────────
            if (_userAdminController != null)
            {
                try { _divisions   = _userAdminController.GetDivisions().OrderBy(d => d.DivisionCode).ToList(); }
                catch { _divisions = new List<HrDivision>(); }

                try { _departments = _userAdminController.GetDepartments().OrderBy(d => d.DepartmentCode).ToList(); }
                catch { _departments = new List<HrDepartment>(); }

                try { _positions   = _userAdminController.GetPositions().OrderBy(p => p.PositionName).ToList(); }
                catch { _positions = new List<HrPosition>(); }

                try { _levels      = _userAdminController.GetPositionLevels().OrderBy(l => l.LevelCode).ToList(); }
                catch { _levels = new List<HrPositionLevel>(); }
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
                            DivisionID   = seq++,
                            DivisionCode = sample.DivisionCode ?? "",
                            DivisionName = g.Key,
                            IsActive     = true
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
                            DepartmentID     = seq++,
                            DivisionID       = divId,
                            DepartmentCode   = sample.DepartmentCode ?? "",
                            DepartmentName   = g.Key,
                            DepartmentNameTh = g.Key,
                            IsActive         = true
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
                        PositionID   = seq++,
                        PositionCode = "",
                        PositionName = g.Key,
                        IsActive     = true
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
                        LevelID   = seq++,
                        LevelCode = g.Key,
                        LevelName = g.Key,
                        IsActive  = true
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
                item.SubItems.Add(u.LevelCode      ?? "");
                item.SubItems.Add(RoleName(u.AppRole));
                item.SubItems.Add(u.IsActive ? "ใช้งาน" : "ปิดใช้");
                item.SubItems.Add(u.LastLoginDate.HasValue
                    ? u.LastLoginDate.Value.ToString("dd/MM/yy HH:mm") : "—");
                item.Tag       = u;
                item.ForeColor = u.IsActive ? Color.Black : Color.DimGray;
                lvUsers.Items.Add(item);
            }
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

        private void LvUsers_SelectionChanged(object sender, EventArgs e)
        {
            bool sel = lvUsers.SelectedItems.Count > 0;
            btnEdit.Enabled = sel;
            if (sel)
            {
                var u = lvUsers.SelectedItems[0].Tag as UserAccount;
                btnToggle.Text    = (u != null && u.IsActive) ? "🔒 ปิดใช้งาน" : "🔓 เปิดใช้งาน";
                btnToggle.Enabled = u != null;
                
                // แสดงข้อมูลที่เลือก
                if (u != null)
                {
                    string selectionInfo = string.Format("เลือก: {0} ({1}) - {2}", 
                        u.DisplayName ?? u.ADUserName, 
                        u.EmployeeCode ?? "ไม่มีรหัส",
                        u.OrgDisplay ?? "ไม่ระบุหน่วยงาน");
                    SetStatus(selectionInfo, Color.FromArgb(25, 55, 105));
                }
            }
            else
            {
                btnToggle.Enabled = false;
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
                    _userAdminController.DeactivateUser(user.UserID);
                else
                {
                    user.IsActive = true;
                    _userAdminController.UpdateUser(user);
                }
                _auditLogController.Log(new AuditEntry
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

            // ── Palette ────────────────────────────────────────────────────
            var navyDark   = Color.FromArgb(25,  55,  105);
            var navyMid    = Color.FromArgb(50,  90,  150);
            var navyLight  = Color.FromArgb(215, 228, 248);
            var bodyBg     = Color.FromArgb(243, 246, 251);
            var footerBg   = Color.FromArgb(228, 234, 245);
            var savedBg    = Color.FromArgb(232, 245, 233);   // สีเขียวอ่อน = ข้อมูลที่บันทึกแล้ว
            var savedBdr   = Color.FromArgb(160, 210, 170);
            var lblColor   = Color.FromArgb(55,  65,  90);
            var savedColor = Color.FromArgb(30,  110,  50);
            var uiFont     = new Font("Segoe UI", 10.5f);
            var boldFont   = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            var lblFont    = new Font("Segoe UI", 10f);
            var smallFont  = new Font("Segoe UI", 9f);

            var dlg = new Form
            {
                Text            = isNew ? "เพิ่มผู้ใช้ใหม่" : "แก้ไขข้อมูลผู้ใช้",
                Size            = new Size(600, 760),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                BackColor       = bodyBg,
                Font            = uiFont,
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
                Text      = isNew ? "เพิ่มผู้ใช้ใหม่" : "แก้ไขข้อมูลผู้ใช้",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(58, 16),
            };
            pnlHeader.Controls.Add(pnlIcon);
            pnlHeader.Controls.Add(lblTitle);

            // ── Body (scrollable) ────────────────────────────────────────────
            var pnlBody = new Panel
            {
                Dock        = DockStyle.Fill,   // เติมพื้นที่ระหว่าง Header และ Footer อัตโนมัติ
                BackColor   = bodyBg,
                AutoScroll  = true,
                Padding     = new Padding(0, 12, 0, 0),
            };

            int lw = 136, fw = 360, lx = 14, tx = 158;
            int inputH = 30, rowGap = 10, rowH = inputH + rowGap;
            int y = 26;

            Action<string, int> addLbl = (text, yy) =>
                pnlBody.Controls.Add(new Label
                {
                    Text      = text,
                    Left      = lx,
                    Top       = yy + 5,
                    Width     = lw,
                    Font      = lblFont,
                    ForeColor = lblColor,
                    TextAlign = ContentAlignment.MiddleRight,
                });

            Func<string, int, TextBox> addTxt = (val, yy) =>
            {
                var t = new TextBox
                {
                    Left        = tx,
                    Top         = yy,
                    Width       = fw,
                    Height      = inputH,
                    Text        = val,
                    Font        = uiFont,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor   = Color.White,
                };
                pnlBody.Controls.Add(t);
                return t;
            };

            Func<int, ComboBox> addCmb = (yy) =>
            {
                var c = new ComboBox
                {
                    Left          = tx,
                    Top           = yy,
                    Width         = fw,
                    Height        = inputH,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font          = uiFont,
                    FlatStyle     = FlatStyle.Flat,
                    BackColor     = Color.White,
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
                    Text      = "  " + title,
                    Font      = boldFont,
                    ForeColor = navyDark,
                    Dock      = DockStyle.Fill,
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
                    Text      = hasValue ? val : "(ยังไม่ได้ระบุ)",
                    Left      = tx,
                    Top       = yy,
                    Width     = fw,
                    Height    = 22,
                    Font      = hasValue ? new Font("Segoe UI", 10f, FontStyle.Bold) : smallFont,
                    ForeColor = hasValue ? savedColor : Color.DarkGray,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(4, 0, 0, 0),
                };
                pnlBody.Controls.Add(lbl);
                return lbl;
            };

            // ══════════════════════════════════════════════════════════════════
            // SECTION 1: ข้อมูลพื้นฐาน
            // ══════════════════════════════════════════════════════════════════
            addSectionBar("ข้อมูลพื้นฐาน", y); y += 34;

            addLbl("ชื่อผู้ใช้ AD:", y);
            var txtAD = addTxt(user.ADUserName ?? "", y);
            if (!isNew) txtAD.ReadOnly = true;  // AD username ไม่ควรเปลี่ยนหลังสร้าง
            y += rowH;

            addLbl("ชื่อ-นามสกุล:", y);
            var txtDisplay = addTxt(user.DisplayName ?? "", y);
            y += rowH;

            addLbl("รหัสพนักงาน:", y);
            var txtEmpCode = addTxt(user.EmployeeCode ?? "", y);
            y += rowH;

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
                    Left      = lx,
                    Top       = y,
                    Width     = lw + fw + 10,
                    Height    = hasOrgData ? 84 : 36,
                    BackColor = savedBg,
                    BorderStyle = BorderStyle.FixedSingle,
                };

                var lblSavedTitle = new Label
                {
                    Text      = hasOrgData
                        ? "✓ ข้อมูลองค์กรที่บันทึกไว้ในระบบ:"
                        : "ℹ️  ยังไม่มีข้อมูลองค์กรบันทึกไว้ในระบบ",
                    Left      = 6,
                    Top       = 4,
                    Width     = lw + fw - 4,
                    Height    = 20,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = hasOrgData ? savedColor : Color.Gray,
                    AutoSize  = false,
                };
                pnlSaved.Controls.Add(lblSavedTitle);

                if (hasOrgData)
                {
                    string orgLine1 = string.Format("ฝ่าย: {0}   แผนก: {1}",
                        string.IsNullOrWhiteSpace(user.DivisionName)    ? "(ไม่ระบุ)" : user.DivisionName,
                        string.IsNullOrWhiteSpace(user.DepartmentName)  ? "(ไม่ระบุ)" : user.DepartmentName);
                    string orgLine2 = string.Format("ตำแหน่ง: {0}   Level: {1}",
                        string.IsNullOrWhiteSpace(user.PositionName) ? "(ไม่ระบุ)" : user.PositionName,
                        string.IsNullOrWhiteSpace(user.LevelCode)    ? "(ไม่ระบุ)" : user.LevelCode);

                    pnlSaved.Controls.Add(new Label
                    {
                        Text      = orgLine1,
                        Left      = 8, Top = 26, Width = lw + fw - 10, Height = 20,
                        Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                        ForeColor = savedColor,
                        AutoSize  = false,
                    });
                    pnlSaved.Controls.Add(new Label
                    {
                        Text      = orgLine2,
                        Left      = 8, Top = 48, Width = lw + fw - 10, Height = 20,
                        Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                        ForeColor = savedColor,
                        AutoSize  = false,
                    });
                }
                pnlBody.Controls.Add(pnlSaved);
                y += pnlSaved.Height + 10;

                // คำอธิบายสำหรับ HR
                var lblHint = new Label
                {
                    Text      = "หากต้องการอัปเดตข้อมูลองค์กร เลือกค่าใหม่จาก dropdown ด้านล่าง แล้วกด \"บันทึก\"",
                    Left      = lx,
                    Top       = y,
                    Width     = lw + fw + 10,
                    Height    = 32,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Italic),
                    ForeColor = Color.FromArgb(80, 80, 120),
                    AutoSize  = false,
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
                var depts  = (selDiv != null && selDiv.DivisionID > 0)
                    ? _departments.Where(d => d.DivisionID == selDiv.DivisionID)
                    : _departments;
                foreach (var d in depts) cmbDept.Items.Add(d);
                cmbDept.SelectedIndex = 0;

                if (autoSelect)
                    SelectDepartment(cmbDept, user);
            };

            populateDept(true);   // initial load — pre-select user's saved dept
            // เมื่อ user เปลี่ยน ฝ่าย เอง → แค่ filter แผนก ไม่ override ให้เลือกเอง
            cmbDiv.SelectedIndexChanged += (s, ev) => populateDept(false);

            addLbl("ตำแหน่ง:", y);
            var cmbPos = addCmb(y);
            cmbPos.Items.Add(new HrPosition { PositionCode = "", PositionName = "-- ไม่เปลี่ยน / ไม่ระบุ --" });
            foreach (var p in _positions) cmbPos.Items.Add(p);
            cmbPos.DisplayMember = "PositionName";
            cmbPos.SelectedIndex = 0;
            SelectPosition(cmbPos, user);
            y += rowH;

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
                Text         = "บันทึก",
                Size         = new Size(120, 36),
                FlatStyle    = FlatStyle.Flat,
                BackColor    = navyDark,
                ForeColor    = Color.White,
                Font         = boldFont,
                DialogResult = DialogResult.OK,
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCan = new Button
            {
                Text         = "ยกเลิก",
                Size         = new Size(90, 36),
                FlatStyle    = FlatStyle.Flat,
                BackColor    = Color.FromArgb(220, 228, 245),
                ForeColor    = navyDark,
                Font         = boldFont,
                DialogResult = DialogResult.Cancel,
            };
            btnCan.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
            btnCan.FlatAppearance.BorderSize  = 1;

            pnlFooter.Resize += (s, e) =>
            {
                btnOk.Location  = new Point(pnlFooter.Width - btnOk.Width - btnCan.Width - 20, 9);
                btnCan.Location = new Point(pnlFooter.Width - btnCan.Width - 10, 9);
            };
            btnOk.Location  = new Point(520 - btnOk.Width - btnCan.Width - 20, 9);
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

                user.ADUserName   = txtAD.Text.Trim();
                user.DisplayName  = txtDisplay.Text.Trim();
                user.EmployeeCode = txtEmpCode.Text.Trim();
                user.AppRole      = cmbRole.SelectedIndex + 1;

                var selPos   = cmbPos.SelectedItem   as HrPosition;
                var selLevel = cmbLevel.SelectedItem as HrPositionLevel;

                // อัปเดต Position/Level เฉพาะเมื่อเลือกค่าจริง (ไม่ใช่ "-- ไม่เปลี่ยน --")
                if (selPos != null && selPos.PositionID > 0)
                    user.PositionName = selPos.PositionName;
                // ถ้าเลือก "-- ไม่เปลี่ยน --" ให้คง PositionName เดิมไว้

                if (selLevel != null && !string.IsNullOrEmpty(selLevel.LevelCode))
                    user.LevelCode = selLevel.LevelCode;

                var selDiv  = cmbDiv.SelectedItem  as HrDivision;
                var selDept = cmbDept.SelectedItem as HrDepartment;

                if (selDiv != null && selDiv.DivisionID > 0)
                {
                    user.DivisionId   = selDiv.DivisionID;
                    user.DivisionCode = selDiv.DivisionCode;
                    user.DivisionName = selDiv.DivisionName;
                }
                // ถ้าไม่ได้เลือก div → คง DivisionName เดิม (ไม่ clear)

                if (selDept != null && selDept.DepartmentID > 0)
                {
                    user.DepartmentId   = selDept.DepartmentID;
                    user.DepartmentCode = selDept.DepartmentCode;
                    // เก็บทั้ง Name และ NameTh
                    user.DepartmentName = !string.IsNullOrEmpty(selDept.DepartmentNameTh)
                        ? selDept.DepartmentNameTh
                        : selDept.DepartmentName;
                }

                try
                {
                    if (isNew) _userAdminController.CreateUser(user);
                    else       _userAdminController.UpdateUser(user);

                    _auditLogController.Log(new AuditEntry
                    {
                        Module      = "UserManagement",
                        Action      = isNew ? "CREATE_USER" : "EDIT_USER",
                        UserID      = _currentUser.UserID,
                        TableName   = "UserAccount",
                        RecordID    = user.UserID,
                        NewValue    = string.Format("AD:{0} Role:{1} Div:{2} Dept:{3} Pos:{4} Level:{5}",
                                         user.ADUserName, user.AppRole,
                                         user.DivisionCode, user.DepartmentCode,
                                         user.PositionName, user.LevelCode),
                        MachineName = _currentUser.MachineName,
                        IPAddress   = _currentUser.IPAddress,
                        SessionID   = _currentUser.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
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
                    if (string.Equals((dept.DisplayName        ?? "").Trim(), target, StringComparison.OrdinalIgnoreCase)
                     || string.Equals((dept.DepartmentNameTh  ?? "").Trim(), target, StringComparison.OrdinalIgnoreCase)
                     || string.Equals((dept.DepartmentName    ?? "").Trim(), target, StringComparison.OrdinalIgnoreCase))
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
