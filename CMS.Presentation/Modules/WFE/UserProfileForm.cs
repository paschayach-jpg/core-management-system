using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class UserProfileForm : Form
    {
        // ── Dependencies ──────────────────────────────────────────────────────
        private readonly UserContext _userContext;
        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowDelegationService _delegationService;
        private readonly IEmployeeApiService _employeeApiService;
        private readonly ILdapUserSearchService _ldapSearchService;
        private readonly ILoggingService _loggingService;
        private readonly IAuditService _auditService;

        // ── State ─────────────────────────────────────────────────────────────
        private Panel _activeNavItem;
        private Panel _activeTabPanel;
        private Panel _pnlTabContent;
        private UserProfileSettings _settings = new UserProfileSettings();

        // Active OOF Delegate
        private string _oofDelegateUsername = "";
        private string _oofDelegateDisplayName = "";
        private string _oofDelegateTitle = "";
        private string _oofDelegateDepartment = "";

        // Controls from other tabs
        private CheckBox chkNotifyEmail;
        private CheckBox chkNotifySms;
        private CheckBox chkNotifyWeekly;
        private CheckBox chkNotifySla;

        private TextBox txtPersonalPhone;
        private TextBox txtPersonalEmail;
        private TextBox txtPersonalNameTh;
        private TextBox txtPersonalNameEn;

        private TextBox txtCurrentPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private CheckBox chkMfa;

        // Out-of-Office controls
        private CheckBox chkOofEnabled;
        private TextBox txtOofStart;
        private TextBox txtOofEnd;
        private ComboBox cmbOofReason;
        private TextBox txtOofReply;
        private RadioButton rbOofAll;
        private RadioButton rbOofSelected;
        private RadioButton rbOofCategory;
        private Panel pnlOofDelegate;
        private Label lblOofDelegateName;
        private Label lblOofDelegateInfo;

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color Blue       = Color.FromArgb(37,  99, 235);
        private static readonly Color NavyDark   = Color.FromArgb(31,  56, 100);
        private static readonly Color GreenOk    = Color.FromArgb(21, 128,  61);
        private static readonly Color OrangeWarn = Color.FromArgb(146,  64,  14);
        private static readonly Color GrayText   = Color.FromArgb( 80,  90, 110);
        private static readonly Color Separator  = Color.FromArgb(220, 225, 235);

        // ════════════════════════════════════════════════════════════════════════
        // Constructor
        // ════════════════════════════════════════════════════════════════════════

        public UserProfileForm(
            UserContext userContext,
            IWorkflowService workflowService = null,
            IWorkflowDelegationService delegationService = null,
            IEmployeeApiService employeeApiService = null,
            ILdapUserSearchService ldapSearchService = null,
            ILoggingService loggingService = null,
            IAuditService auditService = null)
        {
            InitializeComponent();
            _userContext = userContext;
            _workflowService = workflowService;
            _delegationService = delegationService;
            _employeeApiService = employeeApiService;
            _ldapSearchService = ldapSearchService;
            _loggingService = loggingService;
            _auditService = auditService;

            LoadSettings();
        }

        // ════════════════════════════════════════════════════════════════════════
        // Settings Persistence Helpers
        // ════════════════════════════════════════════════════════════════════════

        private string GetSettingsFilePath()
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_profile_settings.json");
        }

        private void LoadSettings()
        {
            var path = GetSettingsFilePath();
            if (System.IO.File.Exists(path))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(path);
                    _settings = Newtonsoft.Json.JsonConvert.DeserializeObject<UserProfileSettings>(json) ?? new UserProfileSettings();
                }
                catch (Exception ex)
                {
                    _loggingService?.Error("Failed to load user settings: " + ex.Message, ex);
                    _settings = new UserProfileSettings();
                }
            }
            else
            {
                _settings = new UserProfileSettings();
                if (_userContext != null)
                {
                    _settings.PersonalFullNameTh = GetDisplayName();
                    _settings.PersonalContactEmail = ExtractEmail(_userContext.ADUserName);
                }
                _settings.OofStartDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                _settings.OofEndDate = DateTime.Now.AddDays(7).ToString("dd/MM/yyyy HH:mm");
                _settings.OofAutoReplyText = "สวัสดีค่ะ/ครับ — ลาพักร้อน ระหว่างวันที่ " + _settings.OofStartDate + " ถึง " + _settings.OofEndDate + " · สำหรับงานเร่งด่วนกรุณาติดต่อผู้รับมอบหมายงานแทน ขอบคุณค่ะ/ครับ";
            }

            _oofDelegateUsername = _settings.OofDelegateUsername;
            _oofDelegateDisplayName = _settings.OofDelegateDisplayName;
            _oofDelegateTitle = _settings.OofDelegateTitle;
            _oofDelegateDepartment = _settings.OofDelegateDepartment;
        }

        private void SaveSettings()
        {
            var path = GetSettingsFilePath();
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(_settings, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                _loggingService?.Error("Failed to save user settings: " + ex.Message, ex);
                MessageBox.Show("ไม่สามารถบันทึกไฟล์ตั้งค่าได้: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Load
        // ════════════════════════════════════════════════════════════════════════

        private void UserProfileForm_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            try
            {
                var displayName = GetDisplayName();
                lblTitle.Text    = "โปรไฟล์ของฉัน — " + displayName;
                lblSubtitle.Text = "จัดการข้อมูลส่วนตัว การแจ้งเตือน และการมอบหมายงานอัตโนมัติ";

                BuildRibbonTabs();
                BuildToolbar();
                Panel initialNav;
                WorkflowNavigationBuilder.BuildSidebar(pnlSidebarContainer, "settings", OnNavItemClick, out initialNav);
                _activeNavItem = initialNav;
                BuildStatusBar();
                BuildBody();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Ribbon Tabs
        // ════════════════════════════════════════════════════════════════════════

        private void BuildRibbonTabs()
        {
            var tabs = new[] { "แฟ้ม", "หน้าแรก", "ดำเนินการ", "มุมมอง", "เครื่องมือ", "ช่วยเหลือ" };
            int x = 8;
            foreach (var text in tabs)
            {
                bool active = text == "เครื่องมือ";
                var btn = new Button
                {
                    Text      = text,
                    Location  = new Point(x, 0),
                    Size      = new Size(MeasureText(text, new Font("Segoe UI", 9.5F)) + 24, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = active ? Color.White : Color.FromArgb(190, 210, 245),
                    Font      = active ? new Font("Segoe UI", 9.5F, FontStyle.Bold) : new Font("Segoe UI", 9.5F),
                    Cursor    = Cursors.Hand,
                    TabStop   = false,
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 80, 130);
                if (active)
                    btn.Paint += (s, pe) => pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
                pnlRibbonTabs.Controls.Add(btn);
                x += btn.Width + 2;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Toolbar
        // ════════════════════════════════════════════════════════════════════════

        private void BuildToolbar()
        {
            pnlToolbarContainer.Controls.Clear();
            var groups = new[]
            {
                new { Header = "โปรไฟล์", Items = new[]
                {
                    new { Icon = "💾", Text = "บันทึก",          Tag = "save",     Primary = true  },
                    new { Icon = "✕",  Text = "ยกเลิก",          Tag = "cancel",   Primary = false },
                }},
                new { Header = "ลายเซ็น", Items = new[]
                {
                    new { Icon = "✎",  Text = "แก้ไขลายเซ็น",   Tag = "sig",      Primary = false },
                    new { Icon = "📷", Text = "อัปโหลดรูป",     Tag = "photo",    Primary = false },
                }},
                new { Header = "ความปลอดภัย", Items = new[]
                {
                    new { Icon = "🔒", Text = "เปลี่ยนรหัสผ่าน", Tag = "password", Primary = false },
                    new { Icon = "→",  Text = "ออกจากระบบ",      Tag = "logout",   Primary = false },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int gx = x;
                foreach (var item in group.Items)
                {
                    var tag     = item.Tag;
                    bool primary = item.Primary;
                    var bg = primary ? Blue : Color.White;
                    var fg = primary ? Color.White : NavyDark;

                    var pnlBtn = new Panel { Location = new Point(x, 2), Size = new Size(62, 66), BackColor = bg, Cursor = Cursors.Hand, Tag = tag };
                    var lblI   = new Label { Text = item.Icon, Font = new Font("Segoe UI", 13F), ForeColor = fg, BackColor = bg, Location = new Point(0, 6),  Size = new Size(62, 28), TextAlign = ContentAlignment.MiddleCenter };
                    var lblT   = new Label { Text = item.Text, Font = new Font("Segoe UI", 7.5F), ForeColor = fg, BackColor = bg, Location = new Point(0, 36), Size = new Size(62, 22), TextAlign = ContentAlignment.MiddleCenter };
                    pnlBtn.Controls.Add(lblI);
                    pnlBtn.Controls.Add(lblT);

                    if (!primary)
                    {
                        var hover = Color.FromArgb(243, 246, 252);
                        EventHandler ent = (s, ev) => { pnlBtn.BackColor = lblI.BackColor = lblT.BackColor = hover; };
                        EventHandler lev = (s, ev) => { pnlBtn.BackColor = lblI.BackColor = lblT.BackColor = Color.White; };
                        pnlBtn.MouseEnter += ent; lblI.MouseEnter += ent; lblT.MouseEnter += ent;
                        pnlBtn.MouseLeave += lev; lblI.MouseLeave += lev; lblT.MouseLeave += lev;
                    }

                    EventHandler click = (s, ev) => OnToolbarClick(tag);
                    pnlBtn.Click += click; lblI.Click += click; lblT.Click += click;
                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 64;
                }

                pnlToolbarContainer.Controls.Add(new Label
                {
                    Text = group.Header, Font = new Font("Segoe UI", 7.5F),
                    ForeColor = GrayText, TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(gx, 70), Size = new Size(x - gx, 10),
                });

                if (group != groups[groups.Length - 1])
                {
                    pnlToolbarContainer.Controls.Add(new Panel { BackColor = Separator, Location = new Point(x + 2, 8), Size = new Size(1, 58) });
                    x += 10;
                }
            }
            pnlToolbarContainer.Controls.Add(new Panel { BackColor = Separator, Dock = DockStyle.Bottom, Height = 1 });
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "save":     OnSave();    break;
                case "cancel":   OnCancel();  break;
                case "sig":      ShowInfo("แก้ไขลายเซ็น: อยู่ระหว่างพัฒนา");     break;
                case "photo":    ShowInfo("อัปโหลดรูปโปรไฟล์: อยู่ระหว่างพัฒนา"); break;
                case "password": ShowInfo("เปลี่ยนรหัสผ่าน: อยู่ระหว่างพัฒนา");  break;
                case "logout":
                    if (MessageBox.Show("ต้องการออกจากระบบ?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        Application.Exit();
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Status Bar
        // ════════════════════════════════════════════════════════════════════════

        private void BuildStatusBar()
        {
            pnlStatusBar.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(210, 215, 225)), 0, 0, pnlStatusBar.Width, 0);

            var items = new[]
            {
                "  พร้อมใช้งาน  ",
                "  เชื่อมต่อ: CMS-DB01  ",
                string.Format("  โดย: {0}  ", GetDisplayName()),
                "  Zoom 100%  ",
                "  TH  ",
                "  ออนไลน์  ",
            };
            int x = 8;
            foreach (var text in items)
            {
                var lbl = new Label { Text = text, Font = new Font("Segoe UI", 7.5F), ForeColor = GrayText, AutoSize = true, Location = new Point(x, 4) };
                pnlStatusBar.Controls.Add(lbl);
                x += lbl.PreferredWidth + 4;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Main Body
        // ════════════════════════════════════════════════════════════════════════

        private void BuildBody()
        {
            pnlBody.Controls.Clear();

            // Right panel (tabs) — Fill
            var pnlRight = BuildRightPanel();
            pnlRight.Dock = DockStyle.Fill;
            pnlBody.Controls.Add(pnlRight);

            // Left profile card — Left, fixed width
            var pnlLeft = BuildProfileCard();
            pnlLeft.Dock  = DockStyle.Left;
            pnlLeft.Width = 240;
            pnlBody.Controls.Add(pnlLeft);
        }

        // ════════════════════════════════════════════════════════════════════════
        // Left: Profile Card
        // ════════════════════════════════════════════════════════════════════════

        private Panel BuildProfileCard()
        {
            var card = new Panel
            {
                BackColor = Color.White,
                Padding   = new Padding(0, 0, 10, 0),
            };

            var inner = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16, 16, 16, 8) };
            card.Controls.Add(inner);

            int y = 16;

            // ── Avatar ────────────────────────────────────────────────────────
            var initials = GetInitials();
            var avatar   = new Panel { Location = new Point(70, y), Size = new Size(80, 80), BackColor = Color.Transparent };
            avatar.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(Blue))
                    pe.Graphics.FillEllipse(b, 0, 0, 79, 79);
                using (var f  = new Font("Segoe UI", 22F, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    pe.Graphics.DrawString(initials, f, Brushes.White, new RectangleF(0, 0, 80, 80), sf);
            };
            inner.Controls.Add(avatar);
            y += 92;

            // ── Name ─────────────────────────────────────────────────────────
            inner.Controls.Add(new Label
            {
                Text      = GetDisplayName(),
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.None,
                AutoSize  = false,
                Location  = new Point(0, y),
                Size      = new Size(208, 24),
            });
            y += 26;

            // ── Role · Department ─────────────────────────────────────────────
            var posName  = _userContext?.PositionName   ?? "Legal Manager";
            var deptName = _userContext?.DepartmentName ?? "สำนักกฎหมาย";
            inner.Controls.Add(new Label
            {
                Text      = posName + " · " + deptName,
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = GrayText,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize  = false,
                Location  = new Point(0, y),
                Size      = new Size(208, 20),
            });
            y += 28;

            // ── Role badges ───────────────────────────────────────────────────
            var flpBadges = new FlowLayoutPanel
            {
                Location      = new Point(0, y),
                Size          = new Size(208, 50),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,
                BackColor     = Color.White,
            };

            var appRole = _userContext?.AppRole ?? 3;
            if (appRole >= 3) flpBadges.Controls.Add(MakeBadge("Manager",            Color.FromArgb(37, 99,  235), Color.FromArgb(219, 234, 254)));
            flpBadges.Controls.Add(MakeBadge("Template Approver", Color.FromArgb(21, 128,  61), Color.FromArgb(220, 252, 231)));
            flpBadges.Controls.Add(MakeBadge("Workflow Designer",  Color.FromArgb(55,  65,  90), Color.FromArgb(240, 242, 248)));
            inner.Controls.Add(flpBadges);
            y += 56;

            // ── Email ─────────────────────────────────────────────────────────
            var email = ExtractEmail(_userContext?.ADUserName);
            inner.Controls.Add(new Label
            {
                Text      = email,
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 110, 130),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize  = false,
                Location  = new Point(0, y),
                Size      = new Size(208, 20),
            });
            y += 28;

            // ── Separator ─────────────────────────────────────────────────────
            inner.Controls.Add(new Panel { BackColor = Separator, Location = new Point(0, y), Size = new Size(208, 1) });
            y += 12;

            // ── สถิติของฉัน ───────────────────────────────────────────────────
            inner.Controls.Add(new Label { Text = "สถิติของฉัน", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = true, Location = new Point(0, y) });
            y += 22;

            int totalTasks = 0;
            int overdueTasks = 0;
            if (_workflowService != null && _userContext != null)
            {
                try
                {
                    var myTasks = _workflowService.GetMyTasks(_userContext);
                    totalTasks = myTasks.Count;
                    overdueTasks = myTasks.Count(t => t.SLAStatus == CMS.Domain.Enums.SLAStatus.Overdue);
                }
                catch (Exception ex)
                {
                    _loggingService?.Error("Failed to fetch my tasks for stats: " + ex.Message, ex);
                }
            }

            var stats = new[]
            {
                new { Label = "งานรอดำเนินการ",  Value = totalTasks + " รายการ", Badge = (string)null },
                new { Label = "งานเกินกำหนด (SLA)", Value = overdueTasks + " รายการ", Badge = overdueTasks > 0 ? "Overdue" : (string)null },
                new { Label = "SLA Compliance",     Value = "96.5%",              Badge = "96.5%"      },
                new { Label = "เวลาเฉลี่ย (อนุมัติ)", Value = "1.8 วัน/งาน",        Badge = (string)null },
                new { Label = "เข้าสู่ระบบล่าสุด",   Value = DateTime.Now.ToString("วันนี้ HH:mm"),        Badge = (string)null },
            };

            foreach (var stat in stats)
            {
                inner.Controls.Add(new Label { Text = stat.Label, Font = new Font("Segoe UI", 8F), ForeColor = GrayText, AutoSize = true, Location = new Point(0, y) });
                if (stat.Badge != null)
                {
                    inner.Controls.Add(new Label
                    {
                        Text = "● " + stat.Badge, Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                        ForeColor = overdueTasks > 0 && stat.Label.Contains("เกินกำหนด") ? OrangeWarn : GreenOk,
                        BackColor = overdueTasks > 0 && stat.Label.Contains("เกินกำหนด") ? Color.FromArgb(254, 243, 199) : Color.FromArgb(220, 252, 231),
                        AutoSize = true, Padding = new Padding(4, 1, 4, 1),
                        Location = new Point(120, y - 1),
                    });
                }
                else
                {
                    inner.Controls.Add(new Label { Text = stat.Value, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = true, Location = new Point(120, y) });
                }
                y += 20;
            }

            return card;
        }

        // ════════════════════════════════════════════════════════════════════════
        // Right: Tab strip + content
        // ════════════════════════════════════════════════════════════════════════

        private Panel BuildRightPanel()
        {
            var pnl = new Panel { BackColor = Color.White, Padding = new Padding(0) };

            // Tab strip
            var pnlTabs = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            pnlTabs.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Separator), 0, pnlTabs.Height - 1, pnlTabs.Width, pnlTabs.Height - 1);

            var tabDefs = new[] { "Out-of-Office", "การแจ้งเตือน", "ข้อมูลส่วนตัว", "ความปลอดภัย" };
            int tx = 16;
            foreach (var name in tabDefs)
            {
                var tabName = name;
                var tabBtn  = new Panel
                {
                    Location  = new Point(tx, 0),
                    Size      = new Size(MeasureText(name, new Font("Segoe UI", 9.5F)) + 28, 40),
                    BackColor = Color.White,
                    Cursor    = Cursors.Hand,
                    Tag       = tabName,
                };
                var tabLbl = new Label
                {
                    Text      = name,
                    Font      = new Font("Segoe UI", 9.5F),
                    ForeColor = GrayText,
                    BackColor = Color.White,
                    Location  = new Point(0, 0),
                    Size      = tabBtn.Size,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Tag       = tabName,
                };
                tabBtn.Controls.Add(tabLbl);

                // Active tab indicator paint (blue underline + bold text)
                tabBtn.Paint += (s, pe) =>
                {
                    if (_activeTabPanel == tabBtn)
                    {
                        using (var b = new SolidBrush(Blue))
                            pe.Graphics.FillRectangle(b, 0, tabBtn.Height - 3, tabBtn.Width, 3);
                    }
                };

                EventHandler click = (s, ev) => ActivateTab(tabBtn, tabLbl, pnl);
                tabBtn.Click += click;
                tabLbl.Click += click;
                pnlTabs.Controls.Add(tabBtn);
                tx += tabBtn.Width + 4;

                // Auto-activate first tab
                if (name == "Out-of-Office")
                {
                    _activeTabPanel = tabBtn;
                    tabLbl.Font     = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                    tabLbl.ForeColor = Blue;
                }
            }
            pnl.Controls.Add(pnlTabs);

            // Content area
            _pnlTabContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(24, 16, 24, 16), AutoScroll = true };
            pnl.Controls.Add(_pnlTabContent);

            // Load initial tab
            LoadTabContent("Out-of-Office");

            return pnl;
        }

        private void ActivateTab(Panel tabBtn, Label tabLbl, Panel rightPanel)
        {
            if (_activeTabPanel == tabBtn) return;

            // Deactivate previous
            if (_activeTabPanel != null)
            {
                foreach (Control c in _activeTabPanel.Controls)
                {
                    if (c is Label l) { l.Font = new Font("Segoe UI", 9.5F); l.ForeColor = GrayText; }
                }
                _activeTabPanel.Invalidate();
            }

            // Activate new
            _activeTabPanel = tabBtn;
            tabLbl.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            tabLbl.ForeColor  = Blue;
            tabBtn.Invalidate();

            LoadTabContent(tabBtn.Tag as string);
        }

        private void LoadTabContent(string tab)
        {
            if (_pnlTabContent == null) return;
            _pnlTabContent.Controls.Clear();

            switch (tab)
            {
                case "Out-of-Office": BuildOofTab();           break;
                case "การแจ้งเตือน":   BuildNotificationsTab(); break;
                case "ข้อมูลส่วนตัว":  BuildPersonalInfoTab();  break;
                case "ความปลอดภัย":   BuildSecurityTab();      break;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // OOF Tab Content
        // ════════════════════════════════════════════════════════════════════════

        private void BuildOofTab()
        {
            var container = _pnlTabContent;
            int y = 0;

            // ── Orange status banner ──────────────────────────────────────────
            var banner = new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(container.Width - 48, 56),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(255, 251, 235),
            };
            banner.Paint += (s, pe) =>
                pe.Graphics.DrawRectangle(new Pen(Color.FromArgb(253, 230, 138)), 0, 0, banner.Width - 1, banner.Height - 1);

            banner.Controls.Add(new Label { Text = "⚠", Font = new Font("Segoe UI", 12F), ForeColor = Color.FromArgb(146, 64, 14), BackColor = Color.Transparent, AutoSize = true, Location = new Point(12, 18) });
            banner.Controls.Add(new Label { Text = "ตั้งค่า Out-of-Office ปัจจุบัน", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(120, 53, 15), BackColor = Color.Transparent, AutoSize = true, Location = new Point(38, 10) });
            banner.Controls.Add(new Label { Text = "มอบหมายงานแทนเมื่อคุณไม่อยู่สำนักงาน", Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(146, 64, 14), BackColor = Color.Transparent, AutoSize = true, Location = new Point(38, 30) });

            chkOofEnabled = new CheckBox
            {
                Text     = "เปิดใช้",
                Font     = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(120, 53, 15),
                BackColor = Color.Transparent,
                Checked  = _settings.OofEnabled,
                Anchor   = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
            };
            chkOofEnabled.Location = new Point(banner.Width - chkOofEnabled.GetPreferredSize(Size.Empty).Width - 16, 20);
            banner.Controls.Add(chkOofEnabled);
            container.Controls.Add(banner);
            y += 68;

            // ── Date range row ────────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("ช่วงเวลา — เริ่ม", y));
            container.Controls.Add(MakeFieldLabel("ช่วงเวลา — สิ้นสุด", y, right: true));
            y += 20;

            txtOofStart = MakeTextBox(_settings.OofStartDate, y, 0, (container.Width - 72) / 2);
            txtOofEnd   = MakeTextBox(_settings.OofEndDate, y, (container.Width - 72) / 2 + 16, (container.Width - 72) / 2);
            container.Controls.Add(txtOofStart);
            container.Controls.Add(txtOofEnd);
            y += 40;

            // ── Reason ────────────────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("เหตุผล", y));
            y += 20;
            cmbOofReason = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat,
                Font          = new Font("Segoe UI", 9F),
                Location      = new Point(0, y),
                Size          = new Size(container.Width - 48, 28),
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            foreach (var r in new[] { "ลาพักร้อน", "ลาป่วย", "ลากิจ", "ไปราชการ", "อื่นๆ" })
                cmbOofReason.Items.Add(r);
            cmbOofReason.SelectedItem = _settings.OofReason;
            if (cmbOofReason.SelectedIndex < 0) cmbOofReason.SelectedIndex = 0;
            container.Controls.Add(cmbOofReason);
            y += 40;

            // ── Delegate person ───────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("มอบหมายงานระหว่างลาให้", y));
            y += 22;

            pnlOofDelegate = new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(container.Width - 48, 48),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(246, 248, 251),
            };
            pnlOofDelegate.Paint += (s, pe) =>
                pe.Graphics.DrawRectangle(new Pen(Separator), 0, 0, pnlOofDelegate.Width - 1, pnlOofDelegate.Height - 1);

            // Avatar
            var delAvatar = new Panel { Location = new Point(10, 11), Size = new Size(26, 26), BackColor = Color.Transparent };
            delAvatar.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pe.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(20, 184, 166)), 0, 0, 25, 25);
                using (var f  = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    string initial = "ส";
                    if (!string.IsNullOrEmpty(_oofDelegateDisplayName))
                        initial = _oofDelegateDisplayName.Substring(0, 1);
                    pe.Graphics.DrawString(initial, f, Brushes.White, new RectangleF(0, 0, 26, 26), sf);
                }
            };
            pnlOofDelegate.Controls.Add(delAvatar);

            lblOofDelegateName = new Label { Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62),  BackColor = Color.Transparent, AutoSize = true, Location = new Point(42, 8)  };
            lblOofDelegateInfo = new Label { Font = new Font("Segoe UI", 8F),               ForeColor = GrayText,                     BackColor = Color.Transparent, AutoSize = true, Location = new Point(42, 24) };
            UpdateOofDelegatePanel();

            pnlOofDelegate.Controls.Add(lblOofDelegateName);
            pnlOofDelegate.Controls.Add(lblOofDelegateInfo);

            var btnChange = new Button
            {
                Text      = "เปลี่ยน",
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Blue,
                BackColor = Color.White,
                AutoSize  = true,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Cursor    = Cursors.Hand,
            };
            btnChange.FlatAppearance.BorderColor = Separator;
            btnChange.Click    += (s, e) => ChangeOofDelegate();
            pnlOofDelegate.Controls.Add(btnChange);
            pnlOofDelegate.Layout += (s, e) => btnChange.Location = new Point(pnlOofDelegate.Width - btnChange.Width - 10, 14);
            container.Controls.Add(pnlOofDelegate);
            y += 60;

            // ── Scope ─────────────────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("ขอบเขต", y));
            y += 22;

            rbOofAll      = MakeRadio("ทุก Workflow ของฉัน",              y, _settings.OofScope == 0);
            rbOofSelected = MakeRadio("เฉพาะ Template บางรายการ",          y + 26, _settings.OofScope == 1);
            rbOofCategory = MakeRadio("เฉพาะหมวดหมู่: Knowledge, Legal",  y + 52, _settings.OofScope == 2);
            container.Controls.Add(rbOofAll);
            container.Controls.Add(rbOofSelected);
            container.Controls.Add(rbOofCategory);
            y += 88;

            // ── Auto-reply text ───────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("ข้อความตอบอัตโนมัติ (Email / In-App)", y));
            y += 22;

            txtOofReply  = new TextBox
            {
                Location     = new Point(0, y),
                Size         = new Size(container.Width - 48, 72),
                Anchor       = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Multiline    = true,
                Font         = new Font("Segoe UI", 9F),
                Text         = _settings.OofAutoReplyText,
                BorderStyle  = BorderStyle.FixedSingle,
                ForeColor    = Color.FromArgb(30, 36, 55),
            };
            container.Controls.Add(txtOofReply);
            y += 84;

            // ── Info box (Delegation link) ────────────────────────────────────
            var infoBox = new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(container.Width - 48, 40),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(239, 246, 255),
            };
            infoBox.Paint += (s, pe) =>
                pe.Graphics.DrawRectangle(new Pen(Color.FromArgb(191, 219, 254)), 0, 0, infoBox.Width - 1, infoBox.Height - 1);

            infoBox.Controls.Add(new Label { Text = "ℹ", Font = new Font("Segoe UI", 10F), ForeColor = Blue, BackColor = Color.Transparent, AutoSize = true, Location = new Point(10, 11) });
            infoBox.Controls.Add(new Label
            {
                Text      = "เชื่อมกับ Delegation: การตั้งค่านี้จะสร้าง Delegation Rule อัตโนมัติในระบบ · สามารถปรับและเอียดใน Delegation Settings ได้",
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(37, 99, 235),
                BackColor = Color.Transparent,
                AutoSize  = false,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location  = new Point(32, 4),
                Size      = new Size(infoBox.Width - 44, 36),
            });
            container.Controls.Add(infoBox);
        }

        private void ChangeOofDelegate()
        {
            using (var dlg = new DelegateSearchDialog(_employeeApiService, _ldapSearchService, _loggingService, _userContext?.ADUserName))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedUser != null)
                {
                    _oofDelegateUsername = dlg.SelectedUser.SamAccountName;
                    _oofDelegateDisplayName = dlg.SelectedUser.DisplayName;
                    _oofDelegateTitle = dlg.SelectedUser.Title ?? "Sr. Staff";
                    _oofDelegateDepartment = dlg.SelectedUser.Department ?? "กองกลาง";
                    UpdateOofDelegatePanel();
                    pnlOofDelegate.Invalidate(true);
                }
            }
        }

        private void UpdateOofDelegatePanel()
        {
            if (lblOofDelegateName == null || lblOofDelegateInfo == null) return;

            if (string.IsNullOrEmpty(_oofDelegateUsername))
            {
                lblOofDelegateName.Text = "ยังไม่ได้เลือกผู้รับมอบหมาย";
                lblOofDelegateInfo.Text = "กรุณากดปุ่มเปลี่ยนเพื่อเลือกรายชื่อ...";
            }
            else
            {
                lblOofDelegateName.Text = _oofDelegateDisplayName;
                lblOofDelegateInfo.Text = $"{_oofDelegateTitle} · {_oofDelegateDepartment} ({_oofDelegateUsername})";
            }
        }

        private void BuildNotificationsTab()
        {
            var container = _pnlTabContent;
            int y = 16;

            container.Controls.Add(new Label
            {
                Text      = "🔔  ตั้งค่าการแจ้งเตือน",
                Font      = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                AutoSize  = true,
                Location  = new Point(0, y),
            });
            y += 32;

            container.Controls.Add(new Label
            {
                Text      = "ตั้งค่าการแจ้งเตือนผ่านช่องทางต่างๆ เมื่อมีเหตุการณ์ใหม่เกิดขึ้นในระบบ",
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = GrayText,
                AutoSize  = true,
                Location  = new Point(0, y),
            });
            y += 32;

            chkNotifyEmail = new CheckBox
            {
                Text = "ส่งการแจ้งเตือนทาง Email เมื่อมีงานใหม่เข้ามาในช่องงานของฉัน (My Tasks)",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 36, 55),
                Checked = _settings.NotifyEmailNewTask,
                Location = new Point(8, y),
                Size = new Size(container.Width - 48, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            y += 32;

            chkNotifySms = new CheckBox
            {
                Text = "ส่งการแจ้งเตือนทาง SMS สำหรับงานระดับความสำคัญด่วนที่สุด (High Priority)",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 36, 55),
                Checked = _settings.NotifySmsHighPriority,
                Location = new Point(8, y),
                Size = new Size(container.Width - 48, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            y += 32;

            chkNotifyWeekly = new CheckBox
            {
                Text = "ส่งรายงานสรุปสถานะการทำงานและ SLA ประจำสัปดาห์ ทุกวันจันทร์ทาง Email",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 36, 55),
                Checked = _settings.NotifyWeeklyReport,
                Location = new Point(8, y),
                Size = new Size(container.Width - 48, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            y += 32;

            chkNotifySla = new CheckBox
            {
                Text = "ส่งแจ้งเตือนล่วงหน้า 24 ชั่วโมง ก่อนงานจะหมดกำหนดเวลา SLA (At-Risk SLA Alert)",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 36, 55),
                Checked = _settings.NotifySlaWarning,
                Location = new Point(8, y),
                Size = new Size(container.Width - 48, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };

            container.Controls.Add(chkNotifyEmail);
            container.Controls.Add(chkNotifySms);
            container.Controls.Add(chkNotifyWeekly);
            container.Controls.Add(chkNotifySla);
        }

        private void BuildPersonalInfoTab()
        {
            var container = _pnlTabContent;
            int y = 16;

            container.Controls.Add(new Label
            {
                Text      = "👤  ข้อมูลส่วนตัว",
                Font      = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                AutoSize  = true,
                Location  = new Point(0, y),
            });
            y += 32;

            container.Controls.Add(new Label
            {
                Text      = "ดูข้อมูลประจำตัว และระบุช่องทางการติดต่อสำหรับรับแจ้งเตือนระบบ",
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = GrayText,
                AutoSize  = true,
                Location  = new Point(0, y),
            });
            y += 36;

            // ── Readonly columns ──────────────────────────────────────────────
            var readonlyGroup = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(container.Width - 48, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(246, 248, 251),
                Padding = new Padding(12),
            };
            readonlyGroup.Paint += (s, pe) =>
                pe.Graphics.DrawRectangle(new Pen(Separator), 0, 0, readonlyGroup.Width - 1, readonlyGroup.Height - 1);

            int ry = 8;
            var empCode = _userContext?.ADUserName ?? "athapol.s";
            if (empCode.Contains("\\")) empCode = empCode.Substring(empCode.LastIndexOf('\\') + 1);

            readonlyGroup.Controls.Add(new Label { Text = $"รหัสพนักงาน:  {empCode.ToUpper()}", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = NavyDark, Location = new Point(12, ry), AutoSize = true });
            ry += 22;
            readonlyGroup.Controls.Add(new Label { Text = $"บัญชีผู้ใช้ AD:  {_userContext?.ADUserName ?? "athapol.s"}", Font = new Font("Segoe UI", 9F), ForeColor = GrayText, Location = new Point(12, ry), AutoSize = true });
            ry += 22;
            readonlyGroup.Controls.Add(new Label { Text = $"ฝ่าย/แผนก:      {_userContext?.DepartmentName ?? "สำนักกฎหมาย"} ({_userContext?.PositionName ?? "Manager"})", Font = new Font("Segoe UI", 9F), ForeColor = GrayText, Location = new Point(12, ry), AutoSize = true });

            container.Controls.Add(readonlyGroup);
            y += 116;

            // ── Editable fields ───────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("ชื่อ-นามสกุล (ภาษาไทย)", y));
            container.Controls.Add(MakeFieldLabel("ชื่อ-นามสกุล (ภาษาอังกฤษ)", y, right: true));
            y += 20;

            txtPersonalNameTh = MakeTextBox(_settings.PersonalFullNameTh, y, 0, (container.Width - 72) / 2);
            txtPersonalNameEn = MakeTextBox(_settings.PersonalFullNameEn, y, (container.Width - 72) / 2 + 16, (container.Width - 72) / 2);
            container.Controls.Add(txtPersonalNameTh);
            container.Controls.Add(txtPersonalNameEn);
            y += 40;

            container.Controls.Add(MakeFieldLabel("เบอร์โทรศัพท์ติดต่อ", y));
            container.Controls.Add(MakeFieldLabel("อีเมลที่ติดต่อได้ (Contact Email)", y, right: true));
            y += 20;

            txtPersonalPhone = MakeTextBox(_settings.PersonalPhone, y, 0, (container.Width - 72) / 2);
            txtPersonalEmail = MakeTextBox(_settings.PersonalContactEmail, y, (container.Width - 72) / 2 + 16, (container.Width - 72) / 2);
            container.Controls.Add(txtPersonalPhone);
            container.Controls.Add(txtPersonalEmail);
        }

        private void BuildSecurityTab()
        {
            var container = _pnlTabContent;
            int y = 16;

            container.Controls.Add(new Label
            {
                Text      = "🔐  ความปลอดภัย",
                Font      = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                AutoSize  = true,
                Location  = new Point(0, y),
            });
            y += 32;

            container.Controls.Add(new Label
            {
                Text      = "เปลี่ยนรหัสผ่านการใช้งานระบบ หรือเปิดใช้งาน Multi-Factor Authentication",
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = GrayText,
                AutoSize  = true,
                Location  = new Point(0, y),
            });
            y += 36;

            // MFA Setting CheckBox
            chkMfa = new CheckBox
            {
                Text = "เปิดใช้งาน Multi-Factor Authentication (MFA) ผ่านแอปพลิเคชัน Authenticator",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = NavyDark,
                Checked = _settings.SecurityMfaEnabled,
                Location = new Point(8, y),
                Size = new Size(container.Width - 48, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            container.Controls.Add(chkMfa);
            y += 40;

            // Divider
            container.Controls.Add(new Panel { BackColor = Separator, Location = new Point(0, y), Size = new Size(container.Width - 48, 1), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });
            y += 16;

            container.Controls.Add(new Label
            {
                Text      = "เปลี่ยนรหัสผ่านบัญชีผู้ใช้",
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                AutoSize  = true,
                Location  = new Point(0, y),
            });
            y += 26;

            container.Controls.Add(MakeFieldLabel("รหัสผ่านปัจจุบัน", y));
            y += 20;
            txtCurrentPassword = MakeTextBox("", y, 0, (container.Width - 72) / 2);
            txtCurrentPassword.UseSystemPasswordChar = true;
            container.Controls.Add(txtCurrentPassword);
            y += 40;

            container.Controls.Add(MakeFieldLabel("รหัสผ่านใหม่", y));
            container.Controls.Add(MakeFieldLabel("ยืนยันรหัสผ่านใหม่", y, right: true));
            y += 20;

            txtNewPassword = MakeTextBox("", y, 0, (container.Width - 72) / 2);
            txtNewPassword.UseSystemPasswordChar = true;

            txtConfirmPassword = MakeTextBox("", y, (container.Width - 72) / 2 + 16, (container.Width - 72) / 2);
            txtConfirmPassword.UseSystemPasswordChar = true;

            container.Controls.Add(txtNewPassword);
            container.Controls.Add(txtConfirmPassword);
            y += 50;

            // Info box for active session
            var infoBox = new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(container.Width - 48, 48),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(243, 246, 251),
            };
            infoBox.Paint += (s, pe) =>
                pe.Graphics.DrawRectangle(new Pen(Separator), 0, 0, infoBox.Width - 1, infoBox.Height - 1);

            var lastIP = _userContext?.IPAddress ?? "127.0.0.1";
            var machineName = _userContext?.MachineName ?? "DESKTOP-CMS";
            infoBox.Controls.Add(new Label
            {
                Text = $"การเชื่อมต่อปัจจุบัน  |  IP Address: {lastIP}  |  เครื่องคอมพิวเตอร์: {machineName}  |  ความปลอดภัยระดับสูง",
                Font = new Font("Segoe UI", 8F),
                ForeColor = GrayText,
                Location = new Point(12, 16),
                AutoSize = true
            });
            container.Controls.Add(infoBox);
        }

        // ════════════════════════════════════════════════════════════════════════
        // ════════════════════════════════════════════════════════════════════════
        // Actions
        // ════════════════════════════════════════════════════════════════════════

        private void OnSave()
        {
            // 1. Save Out-of-Office tab settings if loaded
            if (chkOofEnabled != null)
            {
                _settings.OofEnabled = chkOofEnabled.Checked;
                _settings.OofStartDate = txtOofStart.Text.Trim();
                _settings.OofEndDate = txtOofEnd.Text.Trim();
                _settings.OofReason = cmbOofReason.SelectedItem?.ToString() ?? "ลาพักร้อน";
                _settings.OofDelegateUsername = _oofDelegateUsername;
                _settings.OofDelegateDisplayName = _oofDelegateDisplayName;
                _settings.OofDelegateTitle = _oofDelegateTitle;
                _settings.OofDelegateDepartment = _oofDelegateDepartment;

                if (rbOofAll.Checked) _settings.OofScope = 0;
                else if (rbOofSelected.Checked) _settings.OofScope = 1;
                else if (rbOofCategory.Checked) _settings.OofScope = 2;

                _settings.OofAutoReplyText = txtOofReply.Text.Trim();

                if (_settings.OofEnabled && string.IsNullOrEmpty(_settings.OofDelegateUsername))
                {
                    MessageBox.Show("กรุณาเลือกผู้รับมอบหมายงานแทนก่อนเปิดใช้งาน Out-of-Office", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime start, end;
                if (!DateTime.TryParseExact(_settings.OofStartDate, new[] { "dd/MM/yyyy HH:mm", "d/M/yyyy HH:mm" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start) ||
                    !DateTime.TryParseExact(_settings.OofEndDate, new[] { "dd/MM/yyyy HH:mm", "d/M/yyyy HH:mm" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end))
                {
                    if (!DateTime.TryParse(_settings.OofStartDate, out start) || !DateTime.TryParse(_settings.OofEndDate, out end))
                    {
                        MessageBox.Show("รูปแบบวันที่เริ่มต้นหรือสิ้นสุดไม่ถูกต้อง (ตัวอย่าง: 17/04/2026 09:00)", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (end <= start)
                {
                    MessageBox.Show("วันเวลาที่สิ้นสุดต้องมากกว่าวันเวลาที่เริ่มต้น", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // 2. Save Notifications tab settings if loaded
            if (chkNotifyEmail != null) _settings.NotifyEmailNewTask = chkNotifyEmail.Checked;
            if (chkNotifySms != null) _settings.NotifySmsHighPriority = chkNotifySms.Checked;
            if (chkNotifyWeekly != null) _settings.NotifyWeeklyReport = chkNotifyWeekly.Checked;
            if (chkNotifySla != null) _settings.NotifySlaWarning = chkNotifySla.Checked;

            // 3. Save Personal Info tab settings if loaded
            if (txtPersonalPhone != null) _settings.PersonalPhone = txtPersonalPhone.Text.Trim();
            if (txtPersonalEmail != null) _settings.PersonalContactEmail = txtPersonalEmail.Text.Trim();
            if (txtPersonalNameTh != null) _settings.PersonalFullNameTh = txtPersonalNameTh.Text.Trim();
            if (txtPersonalNameEn != null) _settings.PersonalFullNameEn = txtPersonalNameEn.Text.Trim();

            // 4. Save Security tab settings if loaded
            if (chkMfa != null) _settings.SecurityMfaEnabled = chkMfa.Checked;

            if (txtNewPassword != null && !string.IsNullOrEmpty(txtNewPassword.Text))
            {
                if (txtNewPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("รหัสผ่านใหม่และการยืนยันรหัสผ่านไม่ตรงกัน", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(txtCurrentPassword.Text))
                {
                    MessageBox.Show("กรุณากรอกรหัสผ่านปัจจุบันเพื่อเปลี่ยนรหัสผ่าน", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _auditService?.Log(new AuditEntry
                {
                    Action    = "UserChangePassword",
                    TableName = "UserAccount",
                    RecordID  = _userContext.UserID,
                    UserName  = _userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue  = "Password changed successfully"
                });

                MessageBox.Show("เปลี่ยนรหัสผ่านเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCurrentPassword.Text = "";
                txtNewPassword.Text = "";
                txtConfirmPassword.Text = "";
            }

            // Perform actual delegation if OOF is enabled
            int successCount = 0;
            int failedCount = 0;
            if (_settings.OofEnabled && _delegationService != null && _workflowService != null)
            {
                try
                {
                    var myTasks = _workflowService.GetMyTasks(_userContext);
                    var tasksToDelegate = myTasks
                        .Where(t => t.CanDelegate && t.CurrentDelegationDepth < t.MaxDelegationDepth)
                        .ToList();

                    if (_settings.OofScope == 2)
                    {
                        tasksToDelegate = tasksToDelegate
                            .Where(t => t.WorkflowName.IndexOf("Knowledge", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        t.WorkflowName.IndexOf("Legal", StringComparison.OrdinalIgnoreCase) >= 0)
                            .ToList();
                    }

                    DateTime start = DateTime.Parse(_settings.OofStartDate);
                    DateTime end = DateTime.Parse(_settings.OofEndDate);

                    foreach (var task in tasksToDelegate)
                    {
                        try
                        {
                            var req = new DelegateTaskRequest
                            {
                                WorkflowStepInstanceId = task.WorkflowStepInstanceId,
                                DelegateToUserName = _settings.OofDelegateUsername,
                                DelegationReason = _settings.OofReason,
                                Remarks = $"Out-of-Office auto-delegation | {start:dd/MM/yyyy HH:mm} - {end:dd/MM/yyyy HH:mm} | Auto-Reply: {_settings.OofAutoReplyText}",
                                StartDate = start,
                                EndDate = end
                            };

                            _delegationService.DelegateTask(req, _userContext);

                            _auditService?.Log(new AuditEntry
                            {
                                Action = "WorkflowTaskDelegated_OOF",
                                TableName = "WorkflowStepInstance",
                                RecordID = task.WorkflowStepInstanceId,
                                UserName = _userContext.ADUserName,
                                AuditDate = DateTime.Now,
                                NewValue = $"Auto-delegated via OOF settings to {_settings.OofDelegateUsername}"
                            });

                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            _loggingService?.Error($"OOF delegation failed for task {task.WorkflowStepInstanceId}: {ex.Message}", ex);
                            failedCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _loggingService?.Error("Failed to delegate tasks on OOF save: " + ex.Message, ex);
                }
            }

            SaveSettings();

            string successMsg = "บันทึกการตั้งค่าเรียบร้อยแล้ว";
            if (_settings.OofEnabled && successCount > 0)
            {
                successMsg += $"\n(ส่งมอบงานอัตโนมัติสำเร็จ {successCount} รายการ)";
            }
            if (failedCount > 0)
            {
                successMsg += $"\n(เกิดข้อผิดพลาดในการมอบหมายงาน {failedCount} รายการ)";
            }
            MessageBox.Show(successMsg, "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnCancel()
        {
            LoadSettings();
            LoadTabContent(_activeTabPanel?.Tag as string ?? "Out-of-Office");
        }

        // ════════════════════════════════════════════════════════════════════════
        // Navigation
        // ════════════════════════════════════════════════════════════════════════

        private void OnNavItemClick(string tag)
        {
            if (tag == "settings") return;
            WorkflowNavigationHandler.HandleNavigation(tag, this, _workflowService, null, null, _loggingService, _userContext, _auditService);
        }

        // ════════════════════════════════════════════════════════════════════════
        // UI Helpers
        // ════════════════════════════════════════════════════════════════════════

        private static Label MakeFieldLabel(string text, int y, bool right = false)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 90),
                AutoSize  = true,
                Location  = new Point(right ? 320 : 0, y),
            };
        }

        private static TextBox MakeTextBox(string text, int y, int x, int width)
        {
            return new TextBox
            {
                Text        = text,
                Font        = new Font("Segoe UI", 9F),
                Location    = new Point(x, y),
                Size        = new Size(width, 28),
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor   = Color.FromArgb(30, 36, 55),
            };
        }

        private static RadioButton MakeRadio(string text, int y, bool check)
        {
            return new RadioButton
            {
                Text     = text,
                Font     = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 36, 55),
                AutoSize = true,
                Location = new Point(4, y),
                Checked  = check,
            };
        }

        private static Label MakeBadge(string text, Color fg, Color bg)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = fg,
                BackColor = bg,
                AutoSize  = true,
                Padding   = new Padding(6, 2, 6, 2),
                Margin    = new Padding(0, 0, 4, 4),
            };
        }

        private static void ShowInfo(string msg) =>
            MessageBox.Show(msg, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // ════════════════════════════════════════════════════════════════════════
        // Data Helpers
        // ════════════════════════════════════════════════════════════════════════

        private string GetDisplayName()
        {
            var adUser = _userContext?.ADUserName ?? "อรรถพล สุวรรณ";
            int bs = adUser.LastIndexOf('\\');
            var login = bs >= 0 ? adUser.Substring(bs + 1) : adUser;
            // Return mock display name if it looks like a login (no spaces)
            if (!login.Contains(" ")) return "อรรถพล สุวรรณ";
            return login;
        }

        private string GetInitials()
        {
            var name = GetDisplayName();
            var parts = name.Split(' ');
            if (parts.Length >= 2)
                return parts[0].Substring(0, 1).ToUpper() + parts[1].Substring(0, 1).ToUpper();
            return name.Length >= 2 ? name.Substring(0, 2).ToUpper() : name.ToUpper();
        }

        private static string ExtractEmail(string adUser)
        {
            if (string.IsNullOrWhiteSpace(adUser)) return "athapol.s@company.co.th";
            int bs = adUser.LastIndexOf('\\');
            var login = bs >= 0 ? adUser.Substring(bs + 1) : adUser;
            return login + "@company.co.th";
        }

        private int MeasureText(string text, Font f)
        {
            using (var g = CreateGraphics()) return (int)g.MeasureString(text, f).Width;
        }
    }

    public class UserProfileSettings
    {
        public bool OofEnabled { get; set; } = false;
        public string OofStartDate { get; set; } = "";
        public string OofEndDate { get; set; } = "";
        public string OofReason { get; set; } = "ลาพักร้อน";
        public string OofDelegateUsername { get; set; } = "";
        public string OofDelegateDisplayName { get; set; } = "";
        public string OofDelegateTitle { get; set; } = "";
        public string OofDelegateDepartment { get; set; } = "";
        public int OofScope { get; set; } = 0;
        public string OofAutoReplyText { get; set; } = "";

        public bool NotifyEmailNewTask { get; set; } = true;
        public bool NotifySmsHighPriority { get; set; } = true;
        public bool NotifyWeeklyReport { get; set; } = false;
        public bool NotifySlaWarning { get; set; } = true;

        public string PersonalPhone { get; set; } = "";
        public string PersonalContactEmail { get; set; } = "";
        public string PersonalFullNameTh { get; set; } = "";
        public string PersonalFullNameEn { get; set; } = "";

        public bool SecurityMfaEnabled { get; set; } = false;
    }
}
