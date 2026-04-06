using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private readonly IUserAccountRepository      _userRepo;
        private readonly UserContext                 _currentUser;
        private readonly IAuditService               _auditService;
        private readonly IHrDivisionRepository       _hrDivRepo;
        private readonly IHrDepartmentRepository     _hrDeptRepo;
        private readonly IHrPositionRepository       _hrPosRepo;
        private readonly IHrPositionLevelRepository  _hrLevelRepo;

        private List<UserAccount>      _allUsers    = new List<UserAccount>();
        private List<HrDivision>       _divisions   = new List<HrDivision>();
        private List<HrDepartment>     _departments = new List<HrDepartment>();
        private List<HrPosition>       _positions   = new List<HrPosition>();
        private List<HrPositionLevel>  _levels      = new List<HrPositionLevel>();

        public UserManagementForm(IUserAccountRepository userRepo, UserContext currentUser,
                                   IAuditService auditService = null,
                                   IHrDivisionRepository hrDivRepo = null,
                                   IHrDepartmentRepository hrDeptRepo = null,
                                   IHrPositionRepository hrPosRepo = null,
                                   IHrPositionLevelRepository hrLevelRepo = null)
        {
            _userRepo     = userRepo;
            _currentUser  = currentUser;
            _auditService = auditService;
            _hrDivRepo    = hrDivRepo;
            _hrDeptRepo   = hrDeptRepo;
            _hrPosRepo    = hrPosRepo;
            _hrLevelRepo  = hrLevelRepo;

            InitializeComponent();
            WireEvents();
            LoadHrData();
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

        private void LoadHrData()
        {
            if (_hrDivRepo   != null) _divisions   = _hrDivRepo.GetAll().OrderBy(d => d.DivisionCode).ToList();
            if (_hrDeptRepo  != null) _departments = _hrDeptRepo.GetAll().OrderBy(d => d.DepartmentCode).ToList();
            if (_hrPosRepo   != null) _positions   = _hrPosRepo.GetAll().OrderBy(p => p.PositionName).ToList();
            if (_hrLevelRepo != null) _levels      = _hrLevelRepo.GetAll().OrderBy(l => l.LevelCode).ToList();
        }

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

            // ── Palette (เหมือน UserPermissionSummaryForm) ──────────────────
            var navyDark  = Color.FromArgb(25,  55,  105);
            var navyMid   = Color.FromArgb(50,  90,  150);
            var navyLight = Color.FromArgb(215, 228, 248);
            var bodyBg    = Color.FromArgb(243, 246, 251);
            var footerBg  = Color.FromArgb(228, 234, 245);
            var lblColor  = Color.FromArgb(55,  65,  90);
            var uiFont    = new Font("Segoe UI", 10.5f);
            var boldFont  = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            var lblFont   = new Font("Segoe UI", 10f);

            var dlg = new Form
            {
                Text            = isNew ? "เพิ่มผู้ใช้ใหม่" : "แก้ไขข้อมูลผู้ใช้",
                Size            = new Size(480, 520),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                BackColor       = bodyBg,
                Font            = uiFont,
            };

            // ── Header ──────────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = navyDark };

            var pnlIcon = new Panel { Size = new Size(36, 36), Location = new Point(14, 11), BackColor = Color.Transparent };
            pnlIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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

            // ── Body ────────────────────────────────────────────────────────
            var pnlBody = new Panel
            {
                Left      = 0,
                Top       = 58,
                Width     = 480,
                Height    = 358,
                BackColor = bodyBg,
            };

            int lw = 112, fw = 290, lx = 16, tx = 138, rowH = 36;
            int y = 18;

            // helper: label
            Action<string, int> addLbl = (text, yy) =>
                pnlBody.Controls.Add(new Label
                {
                    Text      = text,
                    Left      = lx,
                    Top       = yy + 3,
                    Width     = lw,
                    Font      = lblFont,
                    ForeColor = lblColor,
                    TextAlign = ContentAlignment.MiddleRight,
                });

            // helper: textbox
            Func<string, int, TextBox> addTxt = (val, yy) =>
            {
                var t = new TextBox
                {
                    Left        = tx,
                    Top         = yy,
                    Width       = fw,
                    Text        = val,
                    Font        = uiFont,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor   = Color.White,
                };
                pnlBody.Controls.Add(t);
                return t;
            };

            // helper: combobox
            Func<int, ComboBox> addCmb = (yy) =>
            {
                var c = new ComboBox
                {
                    Left          = tx,
                    Top           = yy,
                    Width         = fw,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font          = uiFont,
                    FlatStyle     = FlatStyle.Flat,
                    BackColor     = Color.White,
                };
                pnlBody.Controls.Add(c);
                return c;
            };

            // ── Section: ข้อมูลพื้นฐาน ────────────────────────────────────────
            var barBasic = new Panel { Left = lx, Top = y, Width = lw + fw + 12, Height = 22, BackColor = navyLight };
            barBasic.Controls.Add(new Label
            {
                Text      = "  ข้อมูลพื้นฐาน",
                Font      = boldFont,
                ForeColor = navyDark,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            });
            pnlBody.Controls.Add(barBasic);
            y += 28;

            addLbl("ชื่อผู้ใช้ AD:", y);
            var txtAD = addTxt(user.ADUserName ?? "", y);
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
            y += rowH + 4;

            // ── Section: ข้อมูลองค์กร ─────────────────────────────────────────
            var barOrg = new Panel { Left = lx, Top = y, Width = lw + fw + 12, Height = 22, BackColor = navyLight };
            barOrg.Controls.Add(new Label
            {
                Text      = "  ข้อมูลองค์กร",
                Font      = boldFont,
                ForeColor = navyDark,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            });
            pnlBody.Controls.Add(barOrg);
            y += 28;

            addLbl("ฝ่าย:", y);
            var cmbDiv = addCmb(y);
            cmbDiv.Items.Add(new HrDivision { DivisionCode = "", DivisionName = "-- ไม่ระบุ --" });
            foreach (var d in _divisions) cmbDiv.Items.Add(d);
            cmbDiv.DisplayMember = "DivisionName";
            cmbDiv.SelectedIndex = 0;
            
            // Set division by DivisionId (more reliable than DivisionCode)
            if (user.DivisionId.HasValue && user.DivisionId.Value > 0)
            {
                for (int i = 0; i < cmbDiv.Items.Count; i++)
                {
                    var div = cmbDiv.Items[i] as HrDivision;
                    if (div != null && div.DivisionID == user.DivisionId.Value)
                    {
                        cmbDiv.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(user.DivisionCode))
            {
                for (int i = 0; i < cmbDiv.Items.Count; i++)
                {
                    var div = cmbDiv.Items[i] as HrDivision;
                    if (div != null && div.DivisionCode == user.DivisionCode)
                    { cmbDiv.SelectedIndex = i; break; }
                }
            }
            else if (!string.IsNullOrEmpty(user.DivisionName))
            {
                // Fallback: match by name (when ID/code not in master data)
                for (int i = 0; i < cmbDiv.Items.Count; i++)
                {
                    var div = cmbDiv.Items[i] as HrDivision;
                    if (div != null && div.DivisionName == user.DivisionName)
                    { cmbDiv.SelectedIndex = i; break; }
                }
            }
            y += rowH;

            addLbl("แผนก:", y);
            var cmbDept = addCmb(y);
            cmbDept.DisplayMember = "DepartmentName";
            y += rowH;

            Action<string> populateDept = (currentDeptCode) =>
            {
                cmbDept.Items.Clear();
                cmbDept.Items.Add(new HrDepartment { DepartmentCode = "", DepartmentName = "-- ไม่ระบุ --" });
                var selDiv = cmbDiv.SelectedItem as HrDivision;
                var depts  = (selDiv != null && selDiv.DivisionID > 0)
                    ? _departments.Where(d => d.DivisionID == selDiv.DivisionID)
                    : _departments;
                foreach (var d in depts) cmbDept.Items.Add(d);
                cmbDept.SelectedIndex = 0;
                
                // Set department by DepartmentId (more reliable than DepartmentCode)
                if (user.DepartmentId.HasValue && user.DepartmentId.Value > 0)
                {
                    for (int i = 0; i < cmbDept.Items.Count; i++)
                    {
                        var dept = cmbDept.Items[i] as HrDepartment;
                        if (dept != null && dept.DepartmentID == user.DepartmentId.Value)
                        {
                            cmbDept.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(currentDeptCode))
                {
                    for (int i = 0; i < cmbDept.Items.Count; i++)
                    {
                        var dept = cmbDept.Items[i] as HrDepartment;
                        if (dept != null && dept.DepartmentCode == currentDeptCode)
                        { cmbDept.SelectedIndex = i; break; }
                    }
                }
                else if (!string.IsNullOrEmpty(user.DepartmentName))
                {
                    // Fallback: match by name
                    for (int i = 0; i < cmbDept.Items.Count; i++)
                    {
                        var dept = cmbDept.Items[i] as HrDepartment;
                        if (dept != null && dept.DepartmentName == user.DepartmentName)
                        { cmbDept.SelectedIndex = i; break; }
                    }
                }
            };
            populateDept(user.DepartmentCode ?? "");
            cmbDiv.SelectedIndexChanged += (s, ev) => populateDept("");

            addLbl("ตำแหน่ง:", y);
            var cmbPos = addCmb(y);
            cmbPos.Items.Add(new HrPosition { PositionCode = "", PositionName = "-- ไม่ระบุ --" });
            foreach (var p in _positions) cmbPos.Items.Add(p);
            cmbPos.DisplayMember = "PositionName";
            cmbPos.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(user.PositionName))
                for (int i = 0; i < cmbPos.Items.Count; i++)
                    if (((HrPosition)cmbPos.Items[i]).PositionName == user.PositionName)
                    { cmbPos.SelectedIndex = i; break; }
            y += rowH;

            addLbl("Level:", y);
            var cmbLevel = addCmb(y);
            cmbLevel.Items.Add(new HrPositionLevel { LevelCode = "", LevelName = "-- ไม่ระบุ --" });
            foreach (var lv in _levels) cmbLevel.Items.Add(lv);
            cmbLevel.DisplayMember = "LevelCode";
            cmbLevel.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(user.LevelCode))
                for (int i = 0; i < cmbLevel.Items.Count; i++)
                    if (((HrPositionLevel)cmbLevel.Items[i]).LevelCode == user.LevelCode)
                    { cmbLevel.SelectedIndex = i; break; }

            Action<string> selectLevelByCode = (code) =>
            {
                if (string.IsNullOrEmpty(code)) return;
                for (int i = 0; i < cmbLevel.Items.Count; i++)
                    if (((HrPositionLevel)cmbLevel.Items[i]).LevelCode == code)
                    { cmbLevel.SelectedIndex = i; return; }
            };
            cmbPos.SelectedIndexChanged += (s, ev) =>
            {
                var pos = cmbPos.SelectedItem as HrPosition;
                if (pos != null && !string.IsNullOrEmpty(pos.Level))
                    selectLevelByCode(pos.Level);
            };

            // ── Footer ──────────────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = footerBg };

            var btnOk = new Button
            {
                Text         = "บันทึก",
                Size         = new Size(110, 34),
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
                Size         = new Size(90, 34),
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
                btnOk.Location  = new Point(pnlFooter.Width - btnOk.Width - btnCan.Width - 20, 10);
                btnCan.Location = new Point(pnlFooter.Width - btnCan.Width - 10, 10);
            };
            btnOk.Location  = new Point(480 - btnOk.Width - btnCan.Width - 20, 10);
            btnCan.Location = new Point(480 - btnCan.Width - 10, 10);

            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCan;
            pnlFooter.Controls.Add(btnOk);
            pnlFooter.Controls.Add(btnCan);

            // ── Compose ─────────────────────────────────────────────────────
            dlg.Controls.Add(pnlBody);
            dlg.Controls.Add(pnlHeader);
            dlg.Controls.Add(pnlFooter);

            // ── Save logic ──────────────────────────────────────────────────
            btnOk.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtAD.Text))
                { MessageBox.Show("กรุณาระบุชื่อผู้ใช้ AD", "แจ้งเตือน"); dlg.DialogResult = DialogResult.None; return; }

                user.ADUserName   = txtAD.Text.Trim();
                user.DisplayName  = txtDisplay.Text.Trim();
                user.EmployeeCode = txtEmpCode.Text.Trim();
                user.AppRole      = cmbRole.SelectedIndex + 1;

                var selPos   = cmbPos.SelectedItem   as HrPosition;
                var selLevel = cmbLevel.SelectedItem as HrPositionLevel;
                user.PositionName = (selPos   != null && selPos.PositionID > 0)                     ? selPos.PositionName    : null;
                user.LevelCode    = (selLevel != null && !string.IsNullOrEmpty(selLevel.LevelCode)) ? selLevel.LevelCode : null;

                var selDiv  = cmbDiv.SelectedItem  as HrDivision;
                var selDept = cmbDept.SelectedItem as HrDepartment;

                if (selDiv != null && selDiv.DivisionID > 0)
                {
                    user.DivisionId   = selDiv.DivisionID;
                    user.DivisionCode = selDiv.DivisionCode;
                    user.DivisionName = selDiv.DivisionName;
                }
                else { user.DivisionId = null; user.DivisionCode = null; user.DivisionName = null; }

                if (selDept != null && selDept.DepartmentID > 0)
                {
                    user.DepartmentId   = selDept.DepartmentID;
                    user.DepartmentCode = selDept.DepartmentCode;
                    user.DepartmentName = selDept.DepartmentName;
                }
                else { user.DepartmentId = null; user.DepartmentCode = null; user.DepartmentName = null; }

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
                    MessageBox.Show("บันทึกไม่สำเร็จ: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dlg.DialogResult = DialogResult.None;
                }
            };

            return dlg;
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
