using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Dialogs;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowDetailFormNew : Form
    {
        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private readonly IAuditService _auditService;
        private int _workflowInstanceId;
        private int _pendingStepInstanceId = 0;

        private Panel _activeNavItem;
        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover = Color.FromArgb(245, 249, 255);
        private int _activeTabIndex = 0;

        public string SourceNavTag { get; set; }
        public int WorkflowInstanceId => _workflowInstanceId;

        public WorkflowDetailFormNew()
        {
            InitializeComponent();
        }

        public WorkflowDetailFormNew(
            int workflowInstanceId,
            IWorkflowService workflowService,
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext,
            IAuditService auditService = null)
        {
            _workflowInstanceId = workflowInstanceId;
            _workflowService = workflowService;
            _templateService = templateService;
            _loggingService = loggingService;
            _userContext = userContext;
            _auditService = auditService;
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
                if (_workflowService != null && _workflowInstanceId > 0)
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
            if (tag == "cancel")  { CancelWorkflowAction(); return; }
            MessageBox.Show(string.Format("ฟังก์ชัน '{0}' อยู่ระหว่างพัฒนา", tag), "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnApproveAction()
        {
            if (_workflowService == null)
            {
                MessageBox.Show("Workflow service ไม่พร้อมใช้งาน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_pendingStepInstanceId <= 0)
            {
                MessageBox.Show("ไม่มีขั้นตอนที่รอดำเนินการสำหรับคุณในงานนี้",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var taskInfo = new WorkflowTaskResult
            {
                WorkflowStepInstanceId = _pendingStepInstanceId,
                WorkflowInstanceId     = _workflowInstanceId,
                WorkflowName           = lblTitle.Text,
                StepName               = lblCurrentStepValue.Text,
                InitiatedBy            = lblInitiatorValue.Text
            };

            using (var actionForm = new WorkflowStepActionForm(taskInfo))
            {
                actionForm.InitiatorName = lblInitiatorValue.Text;

                if (actionForm.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    string action   = actionForm.SelectedAction;
                    string comments = actionForm.Comments;

                    if (string.Equals(action, "Reject", StringComparison.OrdinalIgnoreCase))
                    {
                        _workflowService.RejectWorkflowStep(new RejectWorkflowStepRequest
                        {
                            WorkflowInstanceId     = _workflowInstanceId,
                            WorkflowStepInstanceId = _pendingStepInstanceId,
                            RejectionReason        = comments,
                            Remarks                = comments
                        }, _userContext);
                    }
                    else
                    {
                        _workflowService.CompleteWorkflowStep(new CompleteWorkflowStepRequest
                        {
                            WorkflowInstanceId     = _workflowInstanceId,
                            WorkflowStepInstanceId = _pendingStepInstanceId,
                            OutputData             = null,
                            Remarks                = comments
                        }, _userContext);
                    }

                    MessageBox.Show("ดำเนินการเสร็จสิ้น", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFromService();
                }
                catch (Exception ex)
                {
                    _loggingService?.Error("Error processing workflow step: " + ex.Message, ex);
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CancelWorkflowAction()
        {
            if (_workflowService == null)
            {
                MessageBox.Show("Workflow service ไม่พร้อมใช้งาน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show a modern cancel-reason dialog matching the app's navy/card style
            string reason = ShowCancelReasonDialog();
            if (reason == null) return; // user pressed Cancel

            try
            {
                _workflowService.CancelWorkflowInstance(_workflowInstanceId, reason, _userContext);
                MessageBox.Show("ยกเลิก Workflow เรียบร้อยแล้ว", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFromService();
            }
            catch (InvalidOperationException ioe)
            {
                MessageBox.Show(ioe.Message, "ไม่สามารถยกเลิกได้",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                _loggingService?.Error("Error cancelling workflow: " + ex.Message, ex);
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Returns the reason string entered, or null when the user dismissed without confirming.
        private string ShowCancelReasonDialog()
        {
            var navyDark  = Color.FromArgb(31,  56, 100);
            var navyBtn   = Color.FromArgb(41,  71, 120);
            var redBtn    = Color.FromArgb(192,  57,  43);

            var dlg = new Form
            {
                Text            = "",
                FormBorderStyle = FormBorderStyle.None,
                StartPosition   = FormStartPosition.CenterParent,
                Size            = new Size(480, 300),
                BackColor       = Color.White,
                MinimumSize     = new Size(480, 300),
            };

            // Title bar
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = navyDark };
            var lblDlgTitle = new Label
            {
                Text      = "ยกเลิก Workflow",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0)
            };
            titleBar.Controls.Add(lblDlgTitle);

            // Drag support
            bool dragging = false; Point dragStart = Point.Empty;
            titleBar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { dragging = true; dragStart = e.Location; } };
            titleBar.MouseMove += (s, e) => { if (dragging) dlg.Location = new Point(dlg.Location.X + e.X - dragStart.X, dlg.Location.Y + e.Y - dragStart.Y); };
            titleBar.MouseUp   += (s, e) => dragging = false;
            lblDlgTitle.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { dragging = true; dragStart = e.Location; } };
            lblDlgTitle.MouseMove += (s, e) => { if (dragging) dlg.Location = new Point(dlg.Location.X + e.X - dragStart.X, dlg.Location.Y + e.Y - dragStart.Y); };
            lblDlgTitle.MouseUp   += (s, e) => dragging = false;

            // Body
            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblPrompt = new Label
            {
                Text      = "ระบุเหตุผลในการยกเลิก (ไม่บังคับ):",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(60, 70, 90),
                AutoSize  = false,
                Size      = new Size(440, 22),
                Location  = new Point(20, 20)
            };

            var txtReason = new TextBox
            {
                Multiline   = true,
                ScrollBars  = ScrollBars.Vertical,
                Font        = new Font("Segoe UI", 9.5f),
                Size        = new Size(440, 90),
                Location    = new Point(20, 48),
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength   = 500
            };

            var lblWarning = new Label
            {
                Text      = "⚠  การยกเลิกไม่สามารถเรียกคืนได้",
                Font      = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 60, 40),
                AutoSize  = false,
                Size      = new Size(440, 22),
                Location  = new Point(20, 150)
            };

            var btnConfirm = new Button
            {
                Text      = "ยืนยันการยกเลิก",
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = redBtn,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(150, 34),
                Location  = new Point(310, 190),
                DialogResult = DialogResult.OK
            };
            btnConfirm.FlatAppearance.BorderSize = 0;

            var btnClose = new Button
            {
                Text      = "ปิด",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.White,
                BackColor = navyBtn,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(90, 34),
                Location  = new Point(210, 190),
                DialogResult = DialogResult.Cancel
            };
            btnClose.FlatAppearance.BorderSize = 0;

            body.Controls.AddRange(new Control[] { lblPrompt, txtReason, lblWarning, btnConfirm, btnClose });
            dlg.Controls.Add(body);
            dlg.Controls.Add(titleBar);
            dlg.AcceptButton = btnConfirm;
            dlg.CancelButton = btnClose;

            // Border
            dlg.Paint += (s, pe) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 210, 225), 1))
                    pe.Graphics.DrawRectangle(pen, 0, 0, dlg.Width - 1, dlg.Height - 1);
            };

            return dlg.ShowDialog(this) == DialogResult.OK ? txtReason.Text.Trim() : null;
        }

        // ════════════════════════════════════════════════════════════════════════
        // SIDEBAR — handled by WorkflowNavigationBuilder
        // ════════════════════════════════════════════════════════════════════════

        private void OnNavItemClick(string tag)
        {
            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                _workflowService,
                _templateService,
                null, // slaCalculator
                _loggingService,
                _userContext);
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
                // Guard: never show 100% while the workflow is still running — prevents misleading "done" state.
                if (pct >= 100 && instance.Status != WorkflowInstanceStatus.Completed
                                && instance.Status != WorkflowInstanceStatus.Cancelled)
                    pct = 99;
                lblStepBadge.Text = string.Format("Step {0}/{1}", instance.CompletedStepCount, instance.TotalStepCount);
                prgProgress.Value = Math.Min(pct, 100);
                lblProgressPct.Text = pct + "%";
                lblCurrentStepValue.Text = instance.CurrentStepName ?? "-";

                UpdateSlaBar(pct, Color.FromArgb(197, 120, 16));

                // Resolve pending step instance for the current user
                _pendingStepInstanceId = 0;
                try
                {
                    var myTasks = _workflowService.GetMyTasks(_userContext);
                    var pendingTask = myTasks?.FirstOrDefault(t =>
                        t.WorkflowInstanceId == _workflowInstanceId &&
                        (t.Status == WorkflowStepInstanceStatus.Pending ||
                         t.Status == WorkflowStepInstanceStatus.InProgress));
                    _pendingStepInstanceId = pendingTask?.WorkflowStepInstanceId ?? 0;
                }
                catch { /* non-fatal: approve button will show "no pending task" */ }

                PopulateTimeline(instance);
            }
            catch (Exception ex) { _loggingService?.Error(ex.Message, ex); LoadMockData(); }
        }

        private void LoadMockData()
        {
            lblTitle.Text = "ไม่มีข้อมูล";
            lblSubtitle.Text = "ไม่พบเวิร์กโฟลว์หรือบริการไม่พร้อมใช้งาน";
            lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Monitor";

            lblWFIDValue.Text = "-";
            lblTemplateValue.Text = "-";
            lblInitiatorValue.Text = "-";
            lblInitiatorRoleValue.Text = "-";
            lblStartDateValue.Text = "-";
            lblDueDateValue.Text = "-";
            lblLinkedDocValue.Text = "-";
            lblCRAFValue.Text = "-";

            lblStepBadge.Text = "Step 0/0";
            prgProgress.Value = 0;
            lblProgressPct.Text = "0%";
            lblCurrentStepValue.Text = "-";
            lblAssigneeValue.Text = "-";
            lblAssigneeRoleValue.Text = "-";
            lblDeadlineValue.Text = "-";
            lblSLAValue.Text = "-";
            lblStartedValue.Text = "-";

            lblStatusBadge.Text = "● ไม่มีข้อมูล";

            // Initiator avatar: -
            pnlInitiatorAvatar.Tag = "-|120,130,150";
            pnlInitiatorAvatar.Invalidate();

            // Assignee avatar: -
            pnlAssigneeAvatar.Tag = "-|120,130,150";
            pnlAssigneeAvatar.Invalidate();

            // Ribbon avatar: -
            pnlRibbonAvatar.Tag = "-|120,130,150";
            pnlRibbonAvatar.Invalidate();

            // SLA bar: 0%
            UpdateSlaBar(0, Color.FromArgb(120, 130, 150));

            RenderTimeline(new TimelineStep[0]);
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

        private void PopulateTimeline(WorkflowInstanceResult instance)
        {
            if (instance == null) return;
            
            if (instance.StepHistory == null || instance.StepHistory.Count == 0)
            {
                RenderTimeline(new TimelineStep[0]);
                return;
            }

            var timelineSteps = new List<TimelineStep>();
            foreach (var step in instance.StepHistory)
            {
                var timelineStep = new TimelineStep();
                timelineStep.Title = step.StepName;
                timelineStep.StepType = step.StepType.ToString();
                timelineStep.Remark = step.Remarks ?? "";

                // Actor Formatting
                if (step.Status == WorkflowStepInstanceStatus.Completed)
                {
                    timelineStep.Actor = !string.IsNullOrEmpty(step.CompletedBy) 
                        ? step.CompletedBy 
                        : (!string.IsNullOrEmpty(step.AssignedTo) ? step.AssignedTo : "ระบบ");
                }
                else
                {
                    timelineStep.Actor = !string.IsNullOrEmpty(step.AssignedTo) ? step.AssignedTo : "ระบบ";
                }

                // Status tag & color & Icon
                switch (step.Status)
                {
                    case WorkflowStepInstanceStatus.Completed:
                        timelineStep.Icon = "✓";
                        timelineStep.IconColor = Color.FromArgb(34, 139, 34); // Forest Green
                        timelineStep.StatusTag = "อนุมัติ";
                        timelineStep.StatusColor = Color.FromArgb(34, 139, 34);
                        break;

                    case WorkflowStepInstanceStatus.Rejected:
                        timelineStep.Icon = "✘";
                        timelineStep.IconColor = Color.FromArgb(192, 57, 43); // Alizarin Red
                        timelineStep.StatusTag = "ปฏิเสธ";
                        timelineStep.StatusColor = Color.FromArgb(192, 57, 43);
                        break;

                    case WorkflowStepInstanceStatus.InProgress:
                        timelineStep.Icon = "●";
                        timelineStep.IconColor = Color.FromArgb(230, 115, 0); // Orange
                        timelineStep.StatusTag = "กำลังดำเนินการ";
                        timelineStep.StatusColor = Color.FromArgb(230, 115, 0);
                        break;

                    case WorkflowStepInstanceStatus.Delegated:
                        timelineStep.Icon = "→";
                        timelineStep.IconColor = Color.FromArgb(0, 102, 204); // Blue
                        timelineStep.StatusTag = "มอบหมาย";
                        timelineStep.StatusColor = Color.FromArgb(0, 102, 204);
                        break;

                    case WorkflowStepInstanceStatus.Pending:
                    default:
                        timelineStep.Icon = "○";
                        timelineStep.IconColor = Color.FromArgb(180, 185, 195); // Light Gray
                        timelineStep.StatusTag = "ยังไม่เริ่ม";
                        timelineStep.StatusColor = Color.FromArgb(180, 185, 195);
                        break;
                }

                // Date/Deadline Info
                if (step.Status == WorkflowStepInstanceStatus.Completed || step.Status == WorkflowStepInstanceStatus.Rejected)
                {
                    timelineStep.DateInfo = step.CompletedDate.HasValue 
                        ? step.CompletedDate.Value.ToString("dd/MM/yyyy HH:mm") 
                        : "-";
                }
                else if (step.Status == WorkflowStepInstanceStatus.InProgress)
                {
                    if (step.SLADeadline == DateTime.MinValue || step.SLADeadline.Year < 2000)
                    {
                        timelineStep.DateInfo = "กำหนด -";
                    }
                    else
                    {
                        var remaining = step.SLADeadline - DateTime.Now;
                        if (remaining.TotalSeconds > 0)
                        {
                            int days = (int)Math.Ceiling(remaining.TotalDays);
                            timelineStep.DateInfo = string.Format("กำหนด {0} · คงเหลือ {1} วัน", step.SLADeadline.ToString("dd/MM/yyyy HH:mm"), days);
                        }
                        else
                        {
                            int days = (int)Math.Ceiling(Math.Abs(remaining.TotalDays));
                            timelineStep.DateInfo = string.Format("กำหนด {0} · เกินกำหนด {1} วัน", step.SLADeadline.ToString("dd/MM/yyyy HH:mm"), days);
                        }
                    }
                }
                else
                {
                    timelineStep.DateInfo = "— รอ Step ก่อนหน้า";
                }

                timelineSteps.Add(timelineStep);
            }

            RenderTimeline(timelineSteps.ToArray());
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
            row.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Left accent bar
            row.Controls.Add(new Panel { Location = new Point(0, 4), Size = new Size(3, rowH - 8), BackColor = step.IconColor });

            // Step icon
            var iconLbl = new Label { Text = step.Icon, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = step.IconColor, Location = new Point(14, (rowH - 26) / 2), Size = new Size(26, 26), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            row.Controls.Add(iconLbl);

            // Title
            var titleLbl = new Label { Text = step.Title, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(30, 36, 55), Location = new Point(46, 10), Size = new Size(270, 20), BackColor = Color.Transparent, AutoEllipsis = true };
            titleLbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            row.Controls.Add(titleLbl);

            // Step type badge
            if (!string.IsNullOrEmpty(step.StepType))
            {
                var typeLbl = new Label { Text = step.StepType, Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(80, 100, 140), BackColor = Color.FromArgb(235, 241, 254), Location = new Point(322, 12), Size = new Size(70, 16), TextAlign = ContentAlignment.MiddleCenter, AutoSize = false };
                row.Controls.Add(typeLbl);
            }

            // Actor
            var actorLbl = new Label { Text = "⬤  " + step.Actor, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(70, 80, 100), Location = new Point(46, 30), Size = new Size(300, 16), BackColor = Color.Transparent };
            actorLbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            row.Controls.Add(actorLbl);

            // Date
            int dateY = (isActive || hasRemark) ? 50 : 30;
            var dateLbl = new Label { Text = step.DateInfo, Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(120, 130, 150), Location = new Point(46, dateY), Size = new Size(380, 16), BackColor = Color.Transparent };
            dateLbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
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
                remarkLbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
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
            if (idx == 1)
            {
                WorkflowNavigationHandler.HandleNavigation("visualflow", this, _workflowService, _templateService, null, _loggingService, _userContext, null, _workflowInstanceId);
                return;
            }
            if (idx == 0)
            {
                pnlTimelineScroll.Visible = true;
                if (pnlAuditLogScroll != null) pnlAuditLogScroll.Visible = false;
            }
            else if (idx == 2)
            {
                pnlTimelineScroll.Visible = false;
                ShowAuditLogView();
            }
        }

        private Panel pnlAuditLogScroll;
        private Panel pnlAuditLogContent;

        private void ShowAuditLogView()
        {
            if (pnlAuditLogScroll == null)
            {
                pnlAuditLogScroll = new Panel
                {
                    AutoScroll = true,
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Name = "pnlAuditLogScroll"
                };
                pnlAuditLogContent = new Panel
                {
                    BackColor = Color.White,
                    Dock = DockStyle.Top,
                    Height = 400,
                    Name = "pnlAuditLogContent"
                };
                pnlAuditLogScroll.Controls.Add(pnlAuditLogContent);
                pnlTimelineCard.Controls.Add(pnlAuditLogScroll);
                pnlAuditLogScroll.BringToFront();
            }

            pnlAuditLogScroll.Visible = true;
            pnlAuditLogScroll.BringToFront();

            LoadAuditLogs();
        }

        private void LoadAuditLogs()
        {
            try
            {
                pnlAuditLogContent.Controls.Clear();
                if (_auditService == null || _workflowInstanceId <= 0)
                {
                    var lblEmpty = new Label
                    {
                        Text = "ไม่มีข้อมูล Audit Log (บริการไม่พร้อมใช้งาน)",
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                        ForeColor = Color.Gray,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill
                    };
                    pnlAuditLogContent.Controls.Add(lblEmpty);
                    pnlAuditLogContent.Height = 100;
                    return;
                }

                var logs = _auditService.GetWorkflowAuditLogs(_workflowInstanceId, _userContext);
                if (logs == null || logs.Count == 0)
                {
                    var lblEmpty = new Label
                    {
                        Text = "ไม่พบประวัติการแก้ไขใน Audit Log",
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                        ForeColor = Color.Gray,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill
                    };
                    pnlAuditLogContent.Controls.Add(lblEmpty);
                    pnlAuditLogContent.Height = 100;
                    return;
                }

                int y = 8;
                foreach (var log in logs)
                {
                    var row = BuildAuditLogRow(log);
                    row.Location = new Point(12, y);
                    pnlAuditLogContent.Controls.Add(row);
                    y += row.Height + 4;
                }
                pnlAuditLogContent.Height = y + 8;
            }
            catch (Exception ex)
            {
                _loggingService?.Error("Error loading audit logs: " + ex.Message, ex);
                MessageBox.Show("ไม่สามารถโหลด Audit Log ได้: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel BuildAuditLogRow(AuditLog log)
        {
            Color outcomeColor = Color.FromArgb(34, 139, 34); // Success green
            if (log.Result == 2) outcomeColor = Color.FromArgb(192, 57, 43); // Failure red
            else if (log.Result == 3) outcomeColor = Color.FromArgb(230, 115, 0); // Denied orange

            string details = log.NewValue ?? log.OldValue ?? "";
            bool hasDetails = !string.IsNullOrEmpty(details);
            int rowH = hasDetails ? 80 : 56;

            var row = new Panel
            {
                Size = new Size(pnlAuditLogContent.Width - 24, rowH),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Left accent bar
            row.Controls.Add(new Panel { Location = new Point(0, 4), Size = new Size(3, rowH - 8), BackColor = outcomeColor });

            // Action / Event Type
            string actionText = log.Action;
            if (actionText == "WorkflowInstanceCreated") actionText = "สร้าง Workflow";
            else if (actionText == "WorkflowStepCompleted") actionText = "ดำเนินการขั้นตอนเสร็จสิ้น";
            else if (actionText == "WorkflowStepRejected") actionText = "ปฏิเสธขั้นตอน";
            else if (actionText == "WorkflowInstanceCancelled") actionText = "ยกเลิก Workflow";
            else if (actionText == "WorkflowInstanceSuspended") actionText = "ระงับ Workflow";
            else if (actionText == "WorkflowInstanceResumed") actionText = "ให้ Workflow ทำงานต่อ";

            var actionLbl = new Label
            {
                Text = actionText,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 36, 55),
                Location = new Point(14, 10),
                Size = new Size(300, 20),
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            row.Controls.Add(actionLbl);

            // User / Actor
            var userLbl = new Label
            {
                Text = "⬤  ผู้ดำเนินการ: " + (log.UserName ?? "ระบบ"),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(70, 80, 100),
                Location = new Point(14, 30),
                Size = new Size(300, 16),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            row.Controls.Add(userLbl);

            // Timestamp / IP / Machine Info
            int dateY = hasDetails ? 50 : 30;
            string dateInfo = log.AuditDate.ToString("dd/MM/yyyy HH:mm:ss");
            if (!string.IsNullOrEmpty(log.IPAddress)) dateInfo += "  ·  IP: " + log.IPAddress;
            var dateLbl = new Label
            {
                Text = dateInfo,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(120, 130, 150),
                Location = new Point(14, dateY),
                Size = new Size(380, 16),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            row.Controls.Add(dateLbl);

            // Outcome Badge
            string outcomeText = log.Outcome;
            if (outcomeText == "Success") outcomeText = "สำเร็จ";
            else if (outcomeText == "Failure") outcomeText = "ล้มเหลว";
            else if (outcomeText == "Denied") outcomeText = "ถูกปฏิเสธ";

            var outcomeLbl = new Label
            {
                Text = outcomeText,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = outcomeColor,
                BackColor = Color.Transparent,
                Location = new Point(row.Width - 120, 12),
                Size = new Size(106, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            outcomeLbl.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r2 = new Rectangle(0, 0, outcomeLbl.Width - 1, outcomeLbl.Height - 1);
                using (var br = new SolidBrush(Color.FromArgb(30, outcomeColor)))
                using (var path = RoundedRect(r2, 4)) pe.Graphics.FillPath(br, path);
                using (var pen = new Pen(Color.FromArgb(80, outcomeColor)))
                using (var path = RoundedRect(r2, 4)) pe.Graphics.DrawPath(pen, path);
                pe.Graphics.DrawString(outcomeLbl.Text, outcomeLbl.Font, new SolidBrush(outcomeColor),
                    new RectangleF(0, 0, outcomeLbl.Width, outcomeLbl.Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            };
            row.Controls.Add(outcomeLbl);

            // Details/NewValue Description
            if (hasDetails)
            {
                var detailsLbl = new Label
                {
                    Text = "\"" + details + "\"",
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(100, 110, 130),
                    Location = new Point(14, 48),
                    Size = new Size(row.Width - 160, 24),
                    BackColor = Color.Transparent,
                    AutoEllipsis = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                row.Controls.Add(detailsLbl);
            }

            return row;
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
            PopulateTimeline(instance);
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

        private void btnBack_Click(object sender, EventArgs e)
        {
            string targetTag = string.IsNullOrEmpty(SourceNavTag) ? "mytasks" : SourceNavTag;
            WorkflowNavigationHandler.HandleNavigation(
                targetTag,
                this,
                _workflowService,
                _templateService,
                null, // slaCalculator
                _loggingService,
                _userContext);
        }
    }
}
