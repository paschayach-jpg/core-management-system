using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowDetailFormNew : Form
    {
        private readonly IWorkflowService _workflowService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private int _workflowInstanceId;

        private Panel _activeNavItem;
        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover = Color.FromArgb(245, 249, 255);
        private int _activeTabIndex = 0;

        public WorkflowDetailFormNew()
        {
            InitializeComponent();
        }

        public WorkflowDetailFormNew(
            int workflowInstanceId,
            IWorkflowService workflowService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            _workflowInstanceId = workflowInstanceId;
            _workflowService = workflowService;
            _loggingService = loggingService;
            _userContext = userContext;
            InitializeComponent();
        }

        private void WorkflowDetailFormNew_Load(object sender, EventArgs e)
        {
            try
            {
                BuildRibbonTabs();
                BuildToolbar();
                WorkflowNavigationBuilder.BuildSidebar(
                    pnlSidebarContainer, "monitor", OnNavItemClick, out _activeNavItem);
                BuildTimelineTabs();
                BuildPaintHandlers();
                if (_workflowService != null)
                    LoadFromService();
                else
                    LoadMockData();
            }
            catch (Exception ex)
            {
                _loggingService?.Error("Error loading workflow detail: " + ex.Message, ex);
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // PAINT HANDLERS — avatar circles, SLA bar
        // ════════════════════════════════════════════════════════════════════════

        private void BuildPaintHandlers()
        {
            WireAvatarPaint(pnlRibbonAvatar);
            WireAvatarPaint(pnlInitiatorAvatar);
            WireAvatarPaint(pnlAssigneeAvatar);
            WireSlaBarPaint(pnlSlaBar);
            btnMoreActions.Click += (s, ev) =>
                MessageBox.Show("เมนูเพิ่มเติมอยู่ระหว่างพัฒนา", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void WireAvatarPaint(Panel pnl)
        {
            if (pnl == null) return;
            pnl.Paint += (s, pe) =>
            {
                string tag = pnl.Tag as string ?? "?|90,120,170";
                string[] parts = tag.Split('|');
                string initials = parts[0];
                Color bg = ParseColor(parts.Length > 1 ? parts[1] : "90,120,170");

                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var b = new SolidBrush(bg))
                using (var path = RoundedRect(r, pnl.Width / 2))
                    pe.Graphics.FillPath(b, path);

                using (var f = new Font("Segoe UI", 7.5F, FontStyle.Bold))
                    pe.Graphics.DrawString(initials, f, Brushes.White,
                        new RectangleF(0, 0, pnl.Width, pnl.Height),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            };
        }

        private void WireSlaBarPaint(Panel pnl)
        {
            if (pnl == null) return;
            pnl.Paint += (s, pe) =>
            {
                string tag = pnl.Tag as string ?? "40|197,120,16";
                string[] parts = tag.Split('|');
                int pct = int.TryParse(parts[0], out int p) ? p : 40;
                Color col = ParseColor(parts.Length > 1 ? parts[1] : "197,120,16");

                int fillW = (int)(pnl.Width * Math.Min(pct, 100) / 100.0);
                pe.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(228, 232, 240)), 0, 0, pnl.Width, pnl.Height);
                if (fillW > 0)
                    pe.Graphics.FillRectangle(new SolidBrush(col), 0, 0, fillW, pnl.Height);
            };
        }

        private static Color ParseColor(string rgb)
        {
            var p = rgb.Split(',');
            if (p.Length == 3 &&
                int.TryParse(p[0], out int r) &&
                int.TryParse(p[1], out int g) &&
                int.TryParse(p[2], out int b))
                return Color.FromArgb(r, g, b);
            return Color.FromArgb(90, 120, 170);
        }

        // ════════════════════════════════════════════════════════════════════════
        // RIBBON TABS
        // ════════════════════════════════════════════════════════════════════════

        private void BuildRibbonTabs()
        {
            var tabs = new[]
            {
                new { Text = "แฟ้ม",      Active = false },
                new { Text = "หน้าแรก",   Active = false },
                new { Text = "ดำเนินการ", Active = true  },
                new { Text = "มุมมอง",    Active = false },
                new { Text = "เครื่องมือ",Active = false },
                new { Text = "ช่วยเหลือ", Active = false },
            };
            int x = 8;
            foreach (var t in tabs)
            {
                var btn = new Button
                {
                    Text = t.Text,
                    Location = new Point(x, 0),
                    Size = new Size(MeasureTabWidth(t.Text) + 24, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = t.Active ? Color.White : Color.FromArgb(190, 210, 245),
                    Font = t.Active ? new Font("Segoe UI", 9.5F, FontStyle.Bold) : new Font("Segoe UI", 9.5F),
                    Cursor = Cursors.Hand,
                    TabStop = false,
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 80, 130);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 70, 120);
                if (t.Active)
                    btn.Paint += (s, pe) => pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
                pnlRibbonTabs.Controls.Add(btn);
                x += btn.Width + 2;
            }
        }

        private int MeasureTabWidth(string text)
        {
            using (var g = CreateGraphics())
            using (var f = new Font("Segoe UI", 9.5F))
                return (int)g.MeasureString(text, f).Width;
        }

        // ════════════════════════════════════════════════════════════════════════
        // TOOLBAR
        // ════════════════════════════════════════════════════════════════════════

        private void BuildToolbar()
        {
            var groups = new[]
            {
                new { Header = "ดำเนินการ", Items = new[]
                {
                    new { Icon = "✔", Text = "Approve Step", Tag = "approve",  Primary = true  },
                    new { Icon = "✘", Text = "Reject",       Tag = "reject",   Primary = false },
                    new { Icon = "→", Text = "Forward",      Tag = "forward",  Primary = false },
                    new { Icon = "↺", Text = "Reassign",     Tag = "reassign", Primary = false },
                    new { Icon = "✕", Text = "ยกเลิก",       Tag = "cancel",   Primary = false },
                }},
                new { Header = "จัดการ Instance", Items = new[]
                {
                    new { Icon = "🖨", Text = "พิมพ์",    Tag = "print",     Primary = false },
                    new { Icon = "📎", Text = "ไฟล์แนบ", Tag = "attach",    Primary = false },
                    new { Icon = "📋", Text = "Audit Log", Tag = "auditlog",  Primary = false },
                }},
                new { Header = "อ้างอิง", Items = new[]
                {
                    new { Icon = "📚", Text = "ดู KB Article", Tag = "kbarticle", Primary = false },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int groupStartX = x;
                foreach (var item in group.Items)
                {
                    var tag = item.Tag; var icon = item.Icon; var label = item.Text; var primary = item.Primary;
                    var pnlBtn = new Panel
                    {
                        Location = new Point(x, 4),
                        Size = new Size(primary ? 78 : 66, 60),
                        BackColor = primary ? Color.FromArgb(0, 102, 204) : Color.White,
                        Cursor = Cursors.Hand,
                        Tag = tag,
                    };
                    var lblIcon = new Label
                    {
                        Text = icon,
                        Font = new Font("Segoe UI", 15F),
                        ForeColor = primary ? Color.White : _navyDark,
                        BackColor = Color.Transparent,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, 4),
                        Size = new Size(pnlBtn.Width, 28),
                    };
                    var lblText = new Label
                    {
                        Text = label,
                        Font = primary ? new Font("Segoe UI", 7.5F, FontStyle.Bold) : new Font("Segoe UI", 7.5F),
                        ForeColor = primary ? Color.White : Color.FromArgb(55, 65, 90),
                        BackColor = Color.Transparent,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, 33),
                        Size = new Size(pnlBtn.Width, 22),
                    };
                    pnlBtn.Controls.Add(lblIcon);
                    pnlBtn.Controls.Add(lblText);

                    Color hoverBg = primary ? Color.FromArgb(0, 82, 180) : _navHover;
                    EventHandler enter = (s, ev) => { pnlBtn.BackColor = hoverBg; lblIcon.BackColor = hoverBg; lblText.BackColor = hoverBg; };
                    EventHandler leave = (s, ev) => { pnlBtn.BackColor = primary ? Color.FromArgb(0, 102, 204) : Color.White; lblIcon.BackColor = Color.Transparent; lblText.BackColor = Color.Transparent; };
                    EventHandler click = (s, ev) => OnToolbarClick(tag);
                    pnlBtn.MouseEnter += enter; lblIcon.MouseEnter += enter; lblText.MouseEnter += enter;
                    pnlBtn.MouseLeave += leave; lblIcon.MouseLeave += leave; lblText.MouseLeave += leave;
                    pnlBtn.Click += click; lblIcon.Click += click; lblText.Click += click;

                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += pnlBtn.Width + 4;
                }

                int groupW = x - groupStartX - 4;
                var lblGroup = new Label
                {
                    Text = group.Header,
                    Font = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(groupStartX, 66),
                    Size = new Size(groupW, 12),
                };
                pnlToolbarContainer.Controls.Add(lblGroup);

                if (group != groups[groups.Length - 1])
                {
                    var sep = new Panel { BackColor = Color.FromArgb(220, 225, 235), Location = new Point(x + 2, 8), Size = new Size(1, 54) };
                    pnlToolbarContainer.Controls.Add(sep);
                    x += 12;
                }
            }
        }

        private void OnToolbarClick(string tag)
        {
            if (tag == "approve") { btnApproveAction(); return; }
            MessageBox.Show(string.Format("ฟังก์ชัน '{0}' อยู่ระหว่างพัฒนา", tag), "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnApproveAction()
        {
            if (MessageBox.Show("ยืนยันการอนุมัติขั้นตอนนี้?", "ยืนยันการอนุมัติ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    MessageBox.Show("อนุมัติขั้นตอนเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (_workflowService != null) LoadFromService(); else LoadMockData();
                }
                catch (Exception ex)
                {
                    _loggingService?.Error("Error approving step: " + ex.Message, ex);
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // SIDEBAR — handled by WorkflowNavigationBuilder
        // ════════════════════════════════════════════════════════════════════════

        private void OnNavItemClick(string tag)
        {
            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                null, // workflowService
                null, // templateService
                null, // slaCalculator
                null, // loggingService
                null); // userContext
        }

        // ════════════════════════════════════════════════════════════════════════
        // DATA LOADING
        // ════════════════════════════════════════════════════════════════════════

        private void LoadFromService()
        {
            try
            {
                var instance = _workflowService.GetWorkflowInstance(_workflowInstanceId, _userContext);
                if (instance == null) return;

                string code = "WFI-" + instance.WorkflowInstanceId.ToString("D6");
                string tmplName = !string.IsNullOrWhiteSpace(instance.TemplateName) ? instance.TemplateName : (instance.WorkflowName ?? "Template");
                string tmplDisp = tmplName + " v" + instance.TemplateVersion;

                this.Text = code + " — " + tmplName;
                lblTitle.Text = code + " · " + tmplName;
                lblSubtitle.Text = "Instance ของ Template " + tmplDisp;
                lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Monitor  ›  " + code;
                lblWFIDValue.Text = code;
                lblTemplateValue.Text = tmplDisp;
                lblInitiatorValue.Text = string.IsNullOrWhiteSpace(instance.InitiatedBy) ? "-" : instance.InitiatedBy;
                lblStartDateValue.Text = instance.InitiatedDate.ToString("dd/MM/yyyy HH:mm");
                lblStartedValue.Text = instance.InitiatedDate.ToString("dd/MM/yyyy HH:mm");

                int pct = instance.TotalStepCount > 0
                    ? (int)((double)instance.CompletedStepCount / instance.TotalStepCount * 100) : 0;
                lblStepBadge.Text = string.Format("Step {0}/{1}", instance.CompletedStepCount, instance.TotalStepCount);
                prgProgress.Value = Math.Min(pct, 100);
                lblProgressPct.Text = pct + "%";
                lblCurrentStepValue.Text = instance.CurrentStepName ?? "-";

                UpdateSlaBar(pct, Color.FromArgb(197, 120, 16));
            }
            catch (Exception ex) { _loggingService?.Error(ex.Message, ex); LoadMockData(); }
        }

        private void LoadMockData()
        {
            lblTitle.Text = "WFI-2569-00012 · อนุมัติ KB Article";
            lblSubtitle.Text = "Instance ของ Template WFD-003 v1 · Frozen Snapshot";
            lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Monitor  ›  WFI-2569-00012";

            lblWFIDValue.Text = "WFI-2569-00012";
            lblTemplateValue.Text = "WFD-003 · อนุมัติ KB Article  v1";
            lblInitiatorValue.Text = "สมชาย กาญจนา";
            lblInitiatorRoleValue.Text = "Officer · IT";
            lblStartDateValue.Text = "15/04/2569 09:30";
            lblDueDateValue.Text = "22/04/2569 17:00 (รวม)";
            lblLinkedDocValue.Text = "🔗  KBArticle#1234  คู่มือ PDPA";
            lblCRAFValue.Text = "● 3NC · General";

            lblStepBadge.Text = "Step 2/3";
            prgProgress.Value = 55;
            lblProgressPct.Text = "55%";
            lblCurrentStepValue.Text = "ผู้จัดการตรวจสอบเนื้อหา";
            lblAssigneeValue.Text = "วิชัย มีสุข";
            lblAssigneeRoleValue.Text = "Manager · Legal";
            lblDeadlineValue.Text = "20/04/2569 17:00";
            lblSLAValue.Text = "เหลือ 3 วัน";
            lblStartedValue.Text = "15/04/2569 09:32";

            lblStatusBadge.Text = "● กำลังดำเนินการ";

            // Initiator avatar: สมชาย กาญจนา → "สก" orange
            pnlInitiatorAvatar.Tag = "สก|230,115,0";
            pnlInitiatorAvatar.Invalidate();

            // Assignee avatar: วิชัย มีสุข → "วม" blue
            pnlAssigneeAvatar.Tag = "วม|41,128,185";
            pnlAssigneeAvatar.Invalidate();

            // Ribbon avatar: อรวรรณ สุวรรณ → "AS" navy
            pnlRibbonAvatar.Tag = "AS|90,120,170";
            pnlRibbonAvatar.Invalidate();

            // SLA bar: 40% orange
            UpdateSlaBar(40, Color.FromArgb(197, 120, 16));

            LoadMockTimeline();
        }

        private void UpdateSlaBar(int percent, Color barColor)
        {
            pnlSlaBar.Tag = string.Format("{0}|{1},{2},{3}", percent, barColor.R, barColor.G, barColor.B);
            pnlSlaBar.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════════════
        // TIMELINE
        // ════════════════════════════════════════════════════════════════════════

        private struct TimelineStep
        {
            public string Icon; public Color IconColor;
            public string Title; public string StepType; public string Actor; public string DateInfo;
            public string StatusTag; public Color StatusColor; public string Remark;
        }

        private void LoadMockTimeline()
        {
            var steps = new[]
            {
                new TimelineStep { Icon="✓", IconColor=Color.FromArgb(34,139,34),  Title="จุดเริ่มต้น",             StepType="Start",        Actor="ระบบ · Initial",     DateInfo="15/04/69 09:30", StatusTag="",          StatusColor=Color.Transparent,          Remark="" },
                new TimelineStep { Icon="✓", IconColor=Color.FromArgb(34,139,34),  Title="ผู้สร้างส่งคำขอ",         StepType="Approval",     Actor="สมชาย กาญจนา",      DateInfo="15/04/69 09:30", StatusTag="อนุมัติ",   StatusColor=Color.FromArgb(34,139,34),  Remark="ขออนุมัติเผยแพร่ Article คู่มือ PDPA ฉบับปรับปรุง — ตรวจสอบกับฝ่ายกฎหมายแล้ว" },
                new TimelineStep { Icon="●", IconColor=Color.FromArgb(230,115,0),   Title="ผู้จัดการตรวจสอบเนื้อหา", StepType="Approval",     Actor="วิชัย มีสุข",         DateInfo="กำหนด 20/04/2569 17:00 · คงเหลือ 3 วัน", StatusTag="รอดำเนินการ",StatusColor=Color.FromArgb(230,115,0),   Remark="" },
                new TimelineStep { Icon="○", IconColor=Color.FromArgb(180,185,195), Title="แจ้งผลการอนุมัติ",        StepType="Notification", Actor="ระบบส่งอีเมล",       DateInfo="— รอ Step ก่อนหน้า",              StatusTag="",          StatusColor=Color.Transparent,          Remark="" },
                new TimelineStep { Icon="○", IconColor=Color.FromArgb(180,185,195), Title="เผยแพร่ Article สำเร็จ",  StepType="End",          Actor="ระบบ",               DateInfo="— รอ Step ก่อนหน้า",              StatusTag="",          StatusColor=Color.Transparent,          Remark="" },
            };
            RenderTimeline(steps);
        }

        private void RenderTimeline(TimelineStep[] steps)
        {
            pnlTimelineContent.Controls.Clear();
            int y = 8;
            foreach (var step in steps)
            {
                var row = BuildTimelineRow(step);
                row.Location = new Point(12, y);
                pnlTimelineContent.Controls.Add(row);
                y += row.Height + 4;
            }
            pnlTimelineContent.Height = y + 8;
        }

        private Panel BuildTimelineRow(TimelineStep step)
        {
            bool hasRemark = !string.IsNullOrEmpty(step.Remark);
            bool isActive = step.Icon == "●";
            int rowH = (isActive || hasRemark) ? 80 : 56;
            var row = new Panel { Size = new Size(pnlTimelineContent.Width - 24, rowH), BackColor = isActive ? Color.FromArgb(255, 251, 242) : Color.White };

            // Left accent bar
            row.Controls.Add(new Panel { Location = new Point(0, 4), Size = new Size(3, rowH - 8), BackColor = step.IconColor });

            // Step icon
            var iconLbl = new Label { Text = step.Icon, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = step.IconColor, Location = new Point(14, (rowH - 26) / 2), Size = new Size(26, 26), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            row.Controls.Add(iconLbl);

            // Title
            var titleLbl = new Label { Text = step.Title, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(30, 36, 55), Location = new Point(46, 10), Size = new Size(270, 20), BackColor = Color.Transparent, AutoEllipsis = true };
            row.Controls.Add(titleLbl);

            // Step type badge
            if (!string.IsNullOrEmpty(step.StepType))
            {
                var typeLbl = new Label { Text = step.StepType, Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(80, 100, 140), BackColor = Color.FromArgb(235, 241, 254), Location = new Point(322, 12), Size = new Size(70, 16), TextAlign = ContentAlignment.MiddleCenter, AutoSize = false };
                row.Controls.Add(typeLbl);
            }

            // Actor
            var actorLbl = new Label { Text = "⬤  " + step.Actor, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(70, 80, 100), Location = new Point(46, 30), Size = new Size(300, 16), BackColor = Color.Transparent };
            row.Controls.Add(actorLbl);

            // Date
            int dateY = (isActive || hasRemark) ? 50 : 30;
            var dateLbl = new Label { Text = step.DateInfo, Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(120, 130, 150), Location = new Point(46, dateY), Size = new Size(380, 16), BackColor = Color.Transparent };
            row.Controls.Add(dateLbl);

            // Status badge (pill)
            if (!string.IsNullOrEmpty(step.StatusTag))
            {
                var sc = step.StatusColor;
                var statusLbl = new Label
                {
                    Text = step.StatusTag,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = sc,
                    BackColor = Color.Transparent,
                    Location = new Point(row.Width - 120, 12),
                    Size = new Size(106, 22),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                };
                statusLbl.Paint += (s, pe) =>
                {
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var r2 = new Rectangle(0, 0, statusLbl.Width - 1, statusLbl.Height - 1);
                    using (var br = new SolidBrush(Color.FromArgb(30, sc)))
                    using (var path = RoundedRect(r2, 4)) pe.Graphics.FillPath(br, path);
                    using (var pen = new Pen(Color.FromArgb(80, sc)))
                    using (var path = RoundedRect(r2, 4)) pe.Graphics.DrawPath(pen, path);
                    pe.Graphics.DrawString(statusLbl.Text, statusLbl.Font, new SolidBrush(sc),
                        new RectangleF(0, 0, statusLbl.Width, statusLbl.Height),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                };
                row.Controls.Add(statusLbl);
            }

            // Remark
            if (hasRemark)
            {
                var remarkLbl = new Label { Text = "\"" + step.Remark + "\"", Font = new Font("Segoe UI", 8.5F, FontStyle.Italic), ForeColor = Color.FromArgb(100, 110, 130), Location = new Point(46, 48), Size = new Size(row.Width - 160, 24), BackColor = Color.Transparent, AutoEllipsis = true };
                row.Controls.Add(remarkLbl);
            }
            return row;
        }

        // ════════════════════════════════════════════════════════════════════════
        // TIMELINE TABS
        // ════════════════════════════════════════════════════════════════════════

        private void BuildTimelineTabs()
        {
            var tabNames = new[] { "Timeline", "Visual Flow", "Audit Log" };
            int x = 0;
            for (int i = 0; i < tabNames.Length; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text = tabNames[i],
                    Location = new Point(x, 0),
                    Size = new Size(100, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = i == 0 ? _navyDark : Color.FromArgb(110, 120, 140),
                    Font = i == 0 ? new Font("Segoe UI", 9F, FontStyle.Bold) : new Font("Segoe UI", 9F),
                    Cursor = Cursors.Hand,
                    TabStop = false,
                    Tag = idx,
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 244, 250);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 238, 250);
                if (i == 0) btn.Paint += (s, pe) => pe.Graphics.FillRectangle(new SolidBrush(_navyDark), 0, btn.Height - 2, btn.Width, 2);
                btn.Click += (s, ev) => OnTimelineTabClick((int)btn.Tag, (Button)s);
                pnlTimelineTabs.Controls.Add(btn);
                x += 102;
            }
        }

        private void OnTimelineTabClick(int idx, Button clicked)
        {
            _activeTabIndex = idx;
            foreach (Control c in pnlTimelineTabs.Controls)
            {
                if (c is Button b)
                {
                    bool active = (int)b.Tag == idx;
                    b.ForeColor = active ? _navyDark : Color.FromArgb(110, 120, 140);
                    b.Font = active ? new Font("Segoe UI", 9F, FontStyle.Bold) : new Font("Segoe UI", 9F);
                    b.Invalidate();
                }
            }
            if (idx != 0)
                MessageBox.Show("หน้า '" + new[] { "Timeline", "Visual Flow", "Audit Log" }[idx] + "' อยู่ระหว่างพัฒนา", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ════════════════════════════════════════════════════════════════════════
        // PUBLIC API
        // ════════════════════════════════════════════════════════════════════════

        public void DisplayInstanceDetails(WorkflowInstanceResult instance)
        {
            if (instance == null) return;
            string code = "WFI-" + instance.WorkflowInstanceId.ToString("D6");
            string tmplName = !string.IsNullOrWhiteSpace(instance.TemplateName) ? instance.TemplateName : (instance.WorkflowName ?? "Template");
            lblTitle.Text = code + " · " + tmplName;
            lblWFIDValue.Text = code;
            lblTemplateValue.Text = tmplName + " v" + instance.TemplateVersion;
            lblInitiatorValue.Text = string.IsNullOrWhiteSpace(instance.InitiatedBy) ? "-" : instance.InitiatedBy;
            lblStartDateValue.Text = instance.InitiatedDate.ToString("dd/MM/yyyy HH:mm");
            lblStartedValue.Text = instance.InitiatedDate.ToString("dd/MM/yyyy HH:mm");
            int pct = instance.TotalStepCount > 0 ? (int)((double)instance.CompletedStepCount / instance.TotalStepCount * 100) : 0;
            lblStepBadge.Text = string.Format("Step {0}/{1}", instance.CompletedStepCount, instance.TotalStepCount);
            prgProgress.Value = Math.Min(pct, 100);
            lblProgressPct.Text = pct + "%";
            lblCurrentStepValue.Text = instance.CurrentStepName ?? "-";
            UpdateSlaBar(pct, Color.FromArgb(197, 120, 16));
        }

        public void ShowError(string message) => MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        public void ShowInfo(string message) => MessageBox.Show(message, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // ════════════════════════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════════════════════════

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void txtGlobalSearch_GotFocus(object sender, EventArgs e)
        {
            if (txtGlobalSearch.Text == "ค้นหา Workflow, Template, คำสั่ง...")
                txtGlobalSearch.Text = "";
        }

        private void txtGlobalSearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGlobalSearch.Text))
                txtGlobalSearch.Text = "ค้นหา Workflow, Template, คำสั่ง...";
        }
    }
}
