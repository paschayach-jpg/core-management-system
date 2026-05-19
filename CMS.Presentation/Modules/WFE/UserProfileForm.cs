using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class UserProfileForm : Form
    {
        // ── Dependencies ──────────────────────────────────────────────────────
        private readonly UserContext _userContext;

        // ── State ─────────────────────────────────────────────────────────────
        private Panel _activeNavItem;
        private Panel _activeTabPanel;
        private Panel _pnlTabContent;

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

        public UserProfileForm(UserContext userContext)
        {
            InitializeComponent();
            _userContext = userContext;
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

            var stats = new[]
            {
                new { Label = "Workflow อนุมัติ",  Value = "142 ครั้ง (30 วัน)", Badge = (string)null },
                new { Label = "เวลาเฉลี่ย",        Value = "1.8 วัน/งาน",        Badge = (string)null },
                new { Label = "SLA Compliance",     Value = "96.5%",              Badge = "96.5%"      },
                new { Label = "Template ออกแบบ",   Value = "3 Templates",         Badge = (string)null },
                new { Label = "เข้าสู่ระบบ",       Value = "วันนี้ 08:42",        Badge = (string)null },
            };

            foreach (var stat in stats)
            {
                inner.Controls.Add(new Label { Text = stat.Label, Font = new Font("Segoe UI", 8F), ForeColor = GrayText, AutoSize = true, Location = new Point(0, y) });
                if (stat.Badge != null)
                {
                    inner.Controls.Add(new Label
                    {
                        Text = "● " + stat.Badge, Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                        ForeColor = GreenOk, BackColor = Color.FromArgb(220, 252, 231),
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
                case "การแจ้งเตือน":   BuildPlaceholderTab("การแจ้งเตือน",   "🔔", "ตั้งค่าการแจ้งเตือนผ่าน Email และ In-App");           break;
                case "ข้อมูลส่วนตัว":  BuildPlaceholderTab("ข้อมูลส่วนตัว",  "👤", "แก้ไขชื่อ ตำแหน่ง แผนก และรายละเอียดส่วนตัว");       break;
                case "ความปลอดภัย":   BuildPlaceholderTab("ความปลอดภัย",    "🔐", "เปลี่ยนรหัสผ่าน ตั้งค่า 2FA และดูประวัติการเข้าสู่ระบบ"); break;
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
            banner.Controls.Add(new Label { Text = "ไม่ได้ใช้งาน · ไม่มีการมอบหมายงานอัตโนมัติ", Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(146, 64, 14), BackColor = Color.Transparent, AutoSize = true, Location = new Point(38, 30) });

            var chkEnabled = new CheckBox
            {
                Text     = "เปิดใช้",
                Font     = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(120, 53, 15),
                BackColor = Color.Transparent,
                Checked  = true,
                Anchor   = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
            };
            chkEnabled.Location = new Point(banner.Width - chkEnabled.GetPreferredSize(Size.Empty).Width - 16, 20);
            banner.Controls.Add(chkEnabled);
            container.Controls.Add(banner);
            y += 68;

            // ── Date range row ────────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("ช่วงเวลา — เริ่ม", y));
            container.Controls.Add(MakeFieldLabel("ช่วงเวลา — สิ้นสุด", y, right: true));
            y += 20;

            var txtStart = MakeTextBox("17/04/2569 09:00", y, 0, (container.Width - 72) / 2);
            var txtEnd   = MakeTextBox("24/04/2569 17:00", y, (container.Width - 72) / 2 + 16, (container.Width - 72) / 2);
            container.Controls.Add(txtStart);
            container.Controls.Add(txtEnd);
            y += 40;

            // ── Reason ────────────────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("เหตุผล", y));
            y += 20;
            var cmbReason = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat,
                Font          = new Font("Segoe UI", 9F),
                Location      = new Point(0, y),
                Size          = new Size(container.Width - 48, 28),
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            foreach (var r in new[] { "ลาพักร้อน", "ลาป่วย", "ลากิจ", "ไปราชการ", "อื่นๆ" })
                cmbReason.Items.Add(r);
            cmbReason.SelectedIndex = 0;
            container.Controls.Add(cmbReason);
            y += 40;

            // ── Delegate person ───────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("มอบหมายงานระหว่างลาให้", y));
            y += 22;

            var pnlDelegate = new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(container.Width - 48, 48),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(246, 248, 251),
            };
            pnlDelegate.Paint += (s, pe) =>
                pe.Graphics.DrawRectangle(new Pen(Separator), 0, 0, pnlDelegate.Width - 1, pnlDelegate.Height - 1);

            // Avatar
            var delAvatar = new Panel { Location = new Point(10, 11), Size = new Size(26, 26), BackColor = Color.Transparent };
            delAvatar.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pe.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(20, 184, 166)), 0, 0, 25, 25);
                using (var f  = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    pe.Graphics.DrawString("ส", f, Brushes.White, new RectangleF(0, 0, 26, 26), sf);
            };
            pnlDelegate.Controls.Add(delAvatar);
            pnlDelegate.Controls.Add(new Label { Text = "สิริพร พงศ์ชัย",                      Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62),  BackColor = Color.Transparent, AutoSize = true, Location = new Point(42, 8)  });
            pnlDelegate.Controls.Add(new Label { Text = "Sr. Legal Officer · สำนักกฎหมาย", Font = new Font("Segoe UI", 8F),               ForeColor = GrayText,                     BackColor = Color.Transparent, AutoSize = true, Location = new Point(42, 24) });

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
            btnChange.Click    += (s, e) => ShowInfo("เลือกผู้รับมอบหมาย: อยู่ระหว่างพัฒนา");
            pnlDelegate.Controls.Add(btnChange);
            pnlDelegate.Layout += (s, e) => btnChange.Location = new Point(pnlDelegate.Width - btnChange.Width - 10, 14);
            container.Controls.Add(pnlDelegate);
            y += 60;

            // ── Scope ─────────────────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("ขอบเขต", y));
            y += 22;

            var rbAll      = MakeRadio("ทุก Workflow ของฉัน",              y, true);
            var rbSelected = MakeRadio("เฉพาะ Template บางรายการ",          y + 26, false);
            var rbCategory = MakeRadio("เฉพาะหมวดหมู่: Knowledge, Legal",  y + 52, false);
            container.Controls.Add(rbAll);
            container.Controls.Add(rbSelected);
            container.Controls.Add(rbCategory);
            y += 88;

            // ── Auto-reply text ───────────────────────────────────────────────
            container.Controls.Add(MakeFieldLabel("ข้อความตอบอัตโนมัติ (Email / In-App)", y));
            y += 22;

            var autoReply = "สวัสดีค่ะ/ครับ — ลาพักร้อน 17-24 เม.ย. 2569 · งานเร่งด่วนกรุณาติดต่อคุณสิริพร (siriporn.p@company.co.th) ขอบคุณค่ะ/ครับ";
            var txtReply  = new TextBox
            {
                Location     = new Point(0, y),
                Size         = new Size(container.Width - 48, 72),
                Anchor       = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Multiline    = true,
                Font         = new Font("Segoe UI", 9F),
                Text         = autoReply,
                BorderStyle  = BorderStyle.FixedSingle,
                ForeColor    = Color.FromArgb(30, 36, 55),
            };
            container.Controls.Add(txtReply);
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

        // ════════════════════════════════════════════════════════════════════════
        // Placeholder Tabs
        // ════════════════════════════════════════════════════════════════════════

        private void BuildPlaceholderTab(string title, string icon, string description)
        {
            _pnlTabContent.Controls.Add(new Label
            {
                Text      = icon + "  " + title,
                Font      = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                AutoSize  = true,
                Location  = new Point(0, 20),
            });
            _pnlTabContent.Controls.Add(new Label
            {
                Text      = description + "\n\nอยู่ระหว่างพัฒนา",
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = GrayText,
                AutoSize  = true,
                Location  = new Point(0, 60),
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        // Actions
        // ════════════════════════════════════════════════════════════════════════

        private void OnSave()
        {
            ShowInfo("บันทึกการตั้งค่าเรียบร้อยแล้ว (Mock)");
        }

        private void OnCancel()
        {
            // reload original values
            LoadTabContent(_activeTabPanel?.Tag as string ?? "Out-of-Office");
        }

        // ════════════════════════════════════════════════════════════════════════
        // Navigation
        // ════════════════════════════════════════════════════════════════════════

        private void OnNavItemClick(string tag)
        {
            if (tag == "settings") return;
            WorkflowNavigationHandler.HandleNavigation(tag, this, null, null, null, null, _userContext, null);
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
}
