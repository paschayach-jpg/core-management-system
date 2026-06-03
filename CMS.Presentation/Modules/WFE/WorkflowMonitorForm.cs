using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowMonitorForm : Form, IWorkflowMonitorView
    {
        private readonly WorkflowMonitorPresenter _presenter;
        private readonly UserContext _userContext;
        private Timer _autoRefreshTimer;

        // ── Nav state ────────────────────────────────────────────────────────────
        private Panel _activeNavItem;
        private readonly Color _navyDark  = Color.FromArgb(31,  56, 100);
        private readonly Color _navHover  = Color.FromArgb(245, 249, 255);

        // ── Display data ─────────────────────────────────────────────────────────
        private List<MonitorRowItem> _displayRows = new List<MonitorRowItem>();
        private int _currentPage = 1;
        private int _totalPages  = 1;
        private const int PageSize = 8;

        // ── Tooltip (custom, avoids owner-draw flicker) ──────────────────────────
        private readonly ToolTip _lvTooltip = new ToolTip { AutomaticDelay = 600, AutoPopDelay = 4000, ShowAlways = true };
        private string _lastTooltip = null;

        // ── Double-buffer for owner-draw ListView (eliminates WM_ERASEBKGND flicker) ──
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private const uint LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036;
        private const uint LVS_EX_DOUBLEBUFFER          = 0x00010000;

        public WorkflowMonitorForm(
            IWorkflowService workflowService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _userContext = userContext;
            
            // Only create presenter if services are available
            if (workflowService != null && loggingService != null && userContext != null)
            {
                _presenter = new WorkflowMonitorPresenter(this, workflowService, loggingService, userContext);
            }
        }

        private void WorkflowMonitorForm_Load(object sender, EventArgs e)
        {
            try
            {
                BuildRibbonTabs();
                BuildToolbar();

                // ใช้ WorkflowNavigationBuilder แทน BuildSidebar() เพื่อให้เมนูเหมือนกันทุก Form
                WorkflowNavigationBuilder.BuildSidebar(
                    pnlSidebarContainer, "monitor", OnNavClick, out _activeNavItem);

                SetupListView();

                // Only load data if presenter is available
                if (_presenter != null)
                {
                    _presenter.RefreshMonitor();

                    _autoRefreshTimer          = new Timer { Interval = 60000 };
                    _autoRefreshTimer.Tick    += (s, a) => _presenter.RefreshMonitor();
                    _autoRefreshTimer.Start();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading monitor: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // RIBBON TABS
        // ════════════════════════════════════════════════════════════════════════

        private void BuildRibbonTabs()
        {
            var tabs = new[] { "แฟ้ม", "หน้าแรก", "ดำเนินการ", "มุมมอง", "เครื่องมือ", "ช่วยเหลือ" };
            int x = 8;
            foreach (var text in tabs)
            {
                bool active = text == "หน้าแรก";
                int w = MeasureTextWidth(text, new Font("Segoe UI", 9.5F)) + 24;
                var btn = new Button
                {
                    Text      = text,
                    Location  = new Point(x, 0),
                    Size      = new Size(w, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = active ? Color.White : Color.FromArgb(190, 210, 245),
                    Font      = active ? new Font("Segoe UI", 9.5F, FontStyle.Bold) : new Font("Segoe UI", 9.5F),
                    Cursor    = Cursors.Hand,
                    TabStop   = false,
                };
                btn.FlatAppearance.BorderSize         = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 80, 130);
                if (active)
                    btn.Paint += (s, pe) => pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
                pnlRibbonTabs.Controls.Add(btn);
                x += w + 2;
            }
        }

        private int MeasureTextWidth(string text, Font f)
        {
            using (var g = CreateGraphics()) return (int)g.MeasureString(text, f).Width;
        }

        // ════════════════════════════════════════════════════════════════════════
        // TOOLBAR
        // ════════════════════════════════════════════════════════════════════════

        private void BuildToolbar()
        {
            var groups = new[]
            {
                new { Header = "คำสั่ง", Items = new[]
                {
                    new { Icon = "👁", Text = "ดูรายละเอียด", Tag = "detail"   },
                    new { Icon = "↺", Text = "Reassign",      Tag = "reassign" },
                    new { Icon = "✕", Text = "ยกเลิก",        Tag = "cancel"   },
                }},
                new { Header = "ตัวกรอง", Items = new[]
                {
                    new { Icon = "▼", Text = "ตัวกรอง",         Tag = "filter"   },
                    new { Icon = "📅", Text = "ช่วงวันที่",      Tag = "daterange"},
                    new { Icon = "⚠", Text = "เฉพาะเกิน SLA",   Tag = "slaonly" },
                }},
                new { Header = "รายงาน", Items = new[]
                {
                    new { Icon = "📊", Text = "SLA Report", Tag = "slareport" },
                    new { Icon = "↑",  Text = "Export",      Tag = "export"   },
                }},
            };

            int x = 8;
            int lastGroup = groups.Length - 1;
            for (int gi = 0; gi < groups.Length; gi++)
            {
                var group = groups[gi];
                int groupStartX = x;
                foreach (var item in group.Items)
                {
                    var tag   = item.Tag;
                    var pnlBtn = new Panel { Location = new Point(x, 2), Size = new Size(58, 60), BackColor = Color.White, Cursor = Cursors.Hand, Tag = tag };
                    var lblI  = new Label  { Text = item.Icon, Font = new Font("Segoe UI", 16F), ForeColor = _navyDark, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 6), Size = new Size(58, 30) };
                    var lblT  = new Label  { Text = item.Text, Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(55, 65, 90), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 36), Size = new Size(58, 20) };
                    pnlBtn.Controls.Add(lblI); pnlBtn.Controls.Add(lblT);

                    EventHandler enter = (s, ev) => { pnlBtn.BackColor = lblI.BackColor = lblT.BackColor = _navHover; };
                    EventHandler leave = (s, ev) => { pnlBtn.BackColor = lblI.BackColor = lblT.BackColor = Color.White; };
                    EventHandler click = (s, ev) => OnToolbarClick(tag);
                    pnlBtn.MouseEnter += enter; lblI.MouseEnter += enter; lblT.MouseEnter += enter;
                    pnlBtn.MouseLeave += leave; lblI.MouseLeave += leave; lblT.MouseLeave += leave;
                    pnlBtn.Click      += click; lblI.Click      += click; lblT.Click      += click;
                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 60;
                }

                pnlToolbarContainer.Controls.Add(new Label
                {
                    Text = group.Header, Font = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150), TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(groupStartX, 62), Size = new Size(x - groupStartX, 10),
                });

                if (gi < lastGroup)
                {
                    pnlToolbarContainer.Controls.Add(new Panel { BackColor = Color.FromArgb(220, 225, 235), Location = new Point(x + 2, 8), Size = new Size(1, 54) });
                    x += 10;
                }
            }

            pnlToolbarContainer.Controls.Add(new Panel { BackColor = Color.FromArgb(220, 225, 235), Dock = DockStyle.Bottom, Height = 1 });
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "detail":
                    if (lvInstances.SelectedItems.Count == 0) { MessageBox.Show("กรุณาเลือกรายการก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                    break;
                case "slaonly":
                    cmbFilterStatus.SelectedIndex = 2; // "เกิน SLA"
                    ApplyLocalFilter();
                    break;
                case "filter":
                    cmbFilterStatus.Focus(); break;
                default:
                    MessageBox.Show(string.Format("ฟังก์ชัน '{0}' อยู่ระหว่างพัฒนา", tag), "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // SIDEBAR NAVIGATION — ใช้ WorkflowNavigationBuilder (shared)
        // ════════════════════════════════════════════════════════════════════════

        private void OnNavClick(string tag)
        {
            // Handle local navigation first
            switch (tag)
            {
                case "monitor":
                    lblTitle.Text      = "ตรวจสอบ Workflow ทั้งระบบ";
                    lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Monitor";
                    if (_presenter != null)
                        _presenter.RefreshMonitor();
                    return;
            }

            // Delegate to navigation handler for other forms
            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                null, // workflowService - managed by presenter
                null, // templateService
                null, // slaCalculator
                null, // loggingService - managed by presenter
                _userContext,
                null); // auditService
        }

        // ════════════════════════════════════════════════════════════════════════
        // LISTVIEW SETUP + OWNER DRAW
        // ════════════════════════════════════════════════════════════════════════

        private void SetupListView()
        {
            lvInstances.SmallImageList   = new ImageList { ImageSize = new Size(1, 46) };
            lvInstances.ShowItemToolTips = false;   // prevent owner-draw flicker/disappear bug
            SendMessage(lvInstances.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, (IntPtr)LVS_EX_DOUBLEBUFFER, (IntPtr)LVS_EX_DOUBLEBUFFER);
            lvInstances.DrawColumnHeader += LvInstances_DrawColumnHeader;
            lvInstances.DrawItem         += LvInstances_DrawItem;
            lvInstances.DrawSubItem      += LvInstances_DrawSubItem;
            lvInstances.SelectedIndexChanged += (s, e) => UpdateResultLabel();
            lvInstances.MouseMove  += LvInstances_MouseMove;
            lvInstances.MouseLeave += (s, e) => { _lvTooltip.Hide(lvInstances); _lastTooltip = null; };

            cmbFilterStatus.SelectedIndexChanged += (s, e) => ApplyLocalFilter();
            cmbFilterType.SelectedIndexChanged   += (s, e) => ApplyLocalFilter();
            txtSearch.Enter   += (s, e) => { if (txtSearch.ForeColor == Color.FromArgb(140, 150, 165)) { txtSearch.Text = ""; txtSearch.ForeColor = Color.FromArgb(30, 36, 55); } };
            txtSearch.Leave   += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "ADUserName..."; txtSearch.ForeColor = Color.FromArgb(140, 150, 165); } };
            txtSearch.TextChanged += (s, e) => { if (txtSearch.ForeColor != Color.FromArgb(140, 150, 165)) ApplyLocalFilter(); };
            btnSearch.Click       += (s, e) => ApplyLocalFilter();
            btnClearFilter.Click  += (s, e) => ClearFilters();
            btnPrev.Click         += (s, e) => GoPage(-1);
            btnNext.Click         += (s, e) => GoPage(+1);
            btnRefresh.Click      += btnRefresh_Click;
            btnExport.Click       += (s, e) => MessageBox.Show("ฟังก์ชัน Export อยู่ระหว่างพัฒนา", "ข้อมูล");
        }

        private void LvInstances_MouseMove(object sender, MouseEventArgs e)
        {
            var item = lvInstances.GetItemAt(e.X, e.Y);
            var row  = item?.Tag as MonitorRowItem;
            if (row == null) { _lvTooltip.Hide(lvInstances); _lastTooltip = null; return; }

            var tip = row.WorkflowName;
            if (tip == _lastTooltip) return;
            _lastTooltip = tip;
            _lvTooltip.Show(tip, lvInstances, e.X + 12, e.Y + 12);
        }

        private void LvInstances_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(246, 248, 251)), e.Bounds);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 224, 232)), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            if (e.ColumnIndex < 0) return;
            using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            using (var f  = new Font("Segoe UI Semibold", 9F, FontStyle.Bold))
                e.Graphics.DrawString(e.Header.Text, f, new SolidBrush(Color.FromArgb(55, 65, 90)), new RectangleF(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height), sf);
        }

        private void LvInstances_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var bg = e.Item.Selected ? Color.FromArgb(232, 241, 255) : (e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253));
            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(235, 238, 244)), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void LvInstances_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var g    = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var row = e.Item.Tag as MonitorRowItem;
            if (row == null) return;
            var r = e.Bounds;

            switch (e.ColumnIndex)
            {
                case 0: // ID — blue link
                    using (var f = new Font("Segoe UI", 9F))
                    using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                        g.DrawString(row.InstanceId, f, new SolidBrush(Color.FromArgb(0, 90, 170)), PadRect(r, 10, 0), sf);
                    break;

                case 1: // Workflow / Template
                    using (var fB = new Font("Segoe UI", 9.5F, FontStyle.Bold))
                    {
                        var rf = PadRect(r, 10, 4);
                        g.DrawString(row.WorkflowName, fB, new SolidBrush(Color.FromArgb(30, 36, 55)), new RectangleF(rf.X, rf.Y, rf.Width, 20), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                    }
                    break;

                case 2: // ผู้ยื่น — avatar + name
                    DrawAvatar(g, r, row.AssigneeName);
                    break;

                case 3: // เริ่ม — start date
                    using (var f = new Font("Segoe UI", 9F))
                    using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                        g.DrawString(row.StartDate, f, new SolidBrush(Color.FromArgb(30, 36, 55)), PadRect(r, 8, 0), sf);
                    break;

                case 4: // Step progress
                    using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(row.StepProgress, f, new SolidBrush(Color.FromArgb(55, 65, 90)), r, sf);
                    break;

                case 5: // Step ปัจจุบัน
                    using (var f = new Font("Segoe UI", 9F))
                    using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                        g.DrawString(row.CurrentStepName, f, new SolidBrush(Color.FromArgb(30, 36, 55)), PadRect(r, 8, 0), sf);
                    break;

                case 6: // สถานะ badge
                    DrawStatusBadge(g, r, row.StatusText);
                    break;
            }
        }

        private static readonly Color[] AvatarColors =
        {
            Color.FromArgb(0,  102, 180), Color.FromArgb(155, 89,  182),
            Color.FromArgb(22, 160, 133), Color.FromArgb(192, 57,  43),
            Color.FromArgb(41, 128, 185), Color.FromArgb(230, 126, 34),
        };

        private void DrawAvatar(Graphics g, Rectangle r, string name)
        {
            int sz  = 26;
            int avX = r.X + 10;
            int avY = r.Y + (r.Height - sz) / 2;
            var col = AvatarColors[Math.Abs((name ?? "").GetHashCode()) % AvatarColors.Length];
            var ar  = new Rectangle(avX, avY, sz, sz);
            using (var b = new SolidBrush(col)) using (var p = RoundedRect(ar, sz / 2)) g.FillPath(b, p);
            g.DrawString(GetInitials(name), new Font("Segoe UI", 8.5F, FontStyle.Bold), Brushes.White, ar,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            using (var f = new Font("Segoe UI", 8.5F))
                g.DrawString(name, f, new SolidBrush(Color.FromArgb(30, 36, 55)),
                    new RectangleF(avX + sz + 5, r.Y + 14, r.Width - sz - 24, 18),
                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
        }

        private static readonly Dictionary<string, Color> StatusColors = new Dictionary<string, Color>
        {
            { "กำลังทำงาน",        Color.FromArgb(25,  100, 195) },
            { "OD",               Color.FromArgb(194,  38,  54) },
            { "Pending",          Color.FromArgb(197, 120,  16) },
            { "รอ",               Color.FromArgb(60,  120, 200) },
            { "Completed",        Color.FromArgb(23,  133,  49) },
            { "Rejected",         Color.FromArgb(194,  38,  54) },
            { "รอ Review สุดท้าย", Color.FromArgb(60,  120, 200) },
        };

        private void DrawStatusBadge(Graphics g, Rectangle r, string status)
        {
            Color c;
            if (!StatusColors.TryGetValue(status, out c)) c = Color.FromArgb(120, 130, 150);

            string display = "● " + status;
            var sz  = g.MeasureString(display, new Font("Segoe UI", 8.5F, FontStyle.Bold));
            int bw  = (int)sz.Width + 16;
            int bh  = 22;
            int bx  = r.X + 8;
            int by  = r.Y + (r.Height - bh) / 2;
            var br  = new Rectangle(bx, by, bw, bh);

            using (var bgB = new SolidBrush(Color.FromArgb(30, c)))
            using (var path = RoundedRect(br, 4))
                g.FillPath(bgB, path);
            using (var pen  = new Pen(Color.FromArgb(80, c)))
            using (var path = RoundedRect(br, 4))
                g.DrawPath(pen, path);

            g.DrawString(display, new Font("Segoe UI", 8.5F, FontStyle.Bold), new SolidBrush(c),
                new RectangleF(bx, by, bw, bh),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        // ════════════════════════════════════════════════════════════════════════
        // IWorkflowMonitorView IMPLEMENTATION
        // ════════════════════════════════════════════════════════════════════════

        public void DisplayMetrics(WorkflowMonitorMetrics metrics)
        {
            if (metrics == null) return;
            try
            {
                // Map metrics to stat cards
                lblTotalInstances.Text     = metrics.InProgressInstances.ToString();
                lblFailed.Text             = metrics.FailedInstances.ToString();
                lblCompleted.Text          = metrics.CompletedInstances.ToString();
                lblSuspended.Text          = metrics.SuspendedInstances.ToString();
                lblAverageCompletionTime.Text = string.Format("เฉลี่ย {0:F1} ชม./WF", metrics.AverageCompletionTimeHours);
                // Keep inProgress in subtitle area
                lblSubtitle.Text = string.Format("มุมมองผู้ดูแล - แสดงทุก Workflow Instance ในระบบ | ทั้งหมด {0} Instance", metrics.TotalInstances);
            }
            catch (Exception ex) { ShowError($"DisplayMetrics error: {ex.Message}"); }
        }

        public void DisplaySLAChart(List<SLAChartData> chartData)
        {
            if (chartData == null) return;
            try
            {
                dgvSLAChart.Rows.Clear();
                if (!dgvSLAChart.Columns.Contains("TemplateName"))
                {
                    dgvSLAChart.Columns.Add("TemplateName", "Template"); dgvSLAChart.Columns.Add("OnTimeCount", "On Time");
                    dgvSLAChart.Columns.Add("AtRiskCount", "At Risk");   dgvSLAChart.Columns.Add("OverdueCount", "Overdue");
                    dgvSLAChart.Columns.Add("CompliancePercentage", "Compliance %");
                }
                // Update เกิน SLA card with overdue count
                int totalOverdue = chartData.Sum(d => d.OverdueCount);
                if (InvokeRequired) Invoke((Action)(() => lblFailed.Text = totalOverdue.ToString()));
                else lblFailed.Text = totalOverdue.ToString();

                foreach (var d in chartData)
                    dgvSLAChart.Rows.Add(d.TemplateName, d.OnTimeCount, d.AtRiskCount, d.OverdueCount, $"{d.CompliancePercentage}%");
            }
            catch (Exception ex) { ShowError($"DisplaySLAChart error: {ex.Message}"); }
        }

        public void DisplayTaskDistribution(List<TaskDistributionData> distribution)
        {
            if (distribution == null) return;
            try
            {
                dgvTaskDistribution.Rows.Clear();
                if (!dgvTaskDistribution.Columns.Contains("UserName"))
                {
                    dgvTaskDistribution.Columns.Add("UserName", "User"); dgvTaskDistribution.Columns.Add("Role", "Role");
                    dgvTaskDistribution.Columns.Add("PendingTasks", "Pending");  dgvTaskDistribution.Columns.Add("CompletedTasks", "Completed");
                    dgvTaskDistribution.Columns.Add("AverageCompletionTime", "Avg (h)");
                }
                foreach (var d in distribution)
                    dgvTaskDistribution.Rows.Add(d.UserName, d.Role, d.PendingTasks, d.CompletedTasks, $"{d.AverageCompletionTimeHours:F1}");
            }
            catch (Exception ex) { ShowError($"DisplayTaskDistribution error: {ex.Message}"); }
        }

        public void DisplayBottleneckAnalysis(List<BottleneckData> bottlenecks)
        {
            if (bottlenecks == null) return;
            try
            {
                dgvBottlenecks.Rows.Clear();
                if (!dgvBottlenecks.Columns.Contains("TemplateName"))
                {
                    dgvBottlenecks.Columns.Add("TemplateName", "Template"); dgvBottlenecks.Columns.Add("StepName", "Step");
                    dgvBottlenecks.Columns.Add("AverageCompletionTime", "Avg (h)"); dgvBottlenecks.Columns.Add("TotalInstances", "Total");
                    dgvBottlenecks.Columns.Add("OverdueCount", "Overdue");
                }
                foreach (var d in bottlenecks)
                    dgvBottlenecks.Rows.Add(d.TemplateName, d.StepName, $"{d.AverageCompletionTimeHours:F1}", d.TotalInstances, d.OverdueCount);
            }
            catch (Exception ex) { ShowError($"DisplayBottleneckAnalysis error: {ex.Message}"); }
        }

        public void DisplayEscalationStats(EscalationStatistics stats)
        {
            if (stats?.EscalationsByReason == null) return;
            try
            {
                dgvEscalations.Rows.Clear();
                if (!dgvEscalations.Columns.Contains("EscalationReason"))
                {
                    dgvEscalations.Columns.Add("EscalationReason", "Reason"); dgvEscalations.Columns.Add("Count", "Count");
                    dgvEscalations.Columns.Add("Percentage", "%"); dgvEscalations.Columns.Add("LastOccurrence", "Last");
                }
                int total = stats.TotalEscalations;
                foreach (var kvp in stats.EscalationsByReason)
                {
                    int pct = total > 0 ? (kvp.Value * 100) / total : 0;
                    dgvEscalations.Rows.Add(kvp.Key, kvp.Value, $"{pct}%", stats.LastEscalationDate?.ToString("dd/MM/yy HH:mm") ?? "N/A");
                }
                // Update รออนุมัติ card with total escalations
                lblSuspended.Text = total.ToString();
            }
            catch (Exception ex) { ShowError($"DisplayEscalationStats error: {ex.Message}"); }
        }

        public void ShowError(string message)
            => MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowInfo(string message)
            => MessageBox.Show(message, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // ════════════════════════════════════════════════════════════════════════
        // LOCAL FILTER + PAGINATION
        // ════════════════════════════════════════════════════════════════════════

        private void ApplyLocalFilter()
        {
            var filtered = _displayRows.AsEnumerable();

            string q = txtSearch.ForeColor == Color.FromArgb(140, 150, 165) ? "" : txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(q))
                filtered = filtered.Where(r => r.InstanceId.ToLower().Contains(q) || r.WorkflowName.ToLower().Contains(q) || r.AssigneeName.ToLower().Contains(q));

            if (cmbFilterStatus.SelectedIndex > 0)
            {
                var s = cmbFilterStatus.SelectedItem.ToString();
                filtered = filtered.Where(r => r.StatusText.Contains(s));
            }
            if (cmbFilterType.SelectedIndex > 0)
            {
                var s = cmbFilterType.SelectedItem.ToString();
                filtered = filtered.Where(r => r.WorkflowName.Contains(s));
            }

            var result = filtered.ToList();
            _totalPages = Math.Max(1, (int)Math.Ceiling(result.Count / (double)PageSize));
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            var page = result.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();
            lvInstances.BeginUpdate();
            lvInstances.Items.Clear();
            foreach (var row in page)
            {
                var item = new ListViewItem("") { Tag = row };
                item.SubItems.AddRange(new[] { row.InstanceId, row.WorkflowName, row.AssigneeName, row.StartDate, row.StepProgress, row.CurrentStepName, row.StatusText });
                lvInstances.Items.Add(item);
            }
            lvInstances.EndUpdate();

            lblPage.Text    = string.Format("แสดง {0}-{1}", (_currentPage - 1) * PageSize + 1, Math.Min(_currentPage * PageSize, result.Count));
            btnPrev.Enabled = _currentPage > 1;
            btnNext.Enabled = _currentPage < _totalPages;
            lblResultCount.Text = string.Format("พบ {0} รายการ", result.Count);
        }

        private void GoPage(int delta)
        {
            _currentPage = Math.Max(1, Math.Min(_totalPages, _currentPage + delta));
            ApplyLocalFilter();
        }

        private void ClearFilters()
        {
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterType.SelectedIndex   = 0;
            txtSearch.Text      = "ADUserName...";
            txtSearch.ForeColor = Color.FromArgb(140, 150, 165);
            _currentPage = 1;
            ApplyLocalFilter();
        }

        private void UpdateResultLabel()
        {
            int sel   = lvInstances.SelectedItems.Count;
            int total = lvInstances.Items.Count;
            if (sel > 0)
                lblResultCount.Text = string.Format("เลือก {0} รายการ | พบ {1} รายการ", sel, total);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.RefreshMonitor();
        }

        private void WorkflowMonitorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _autoRefreshTimer?.Stop();
            _autoRefreshTimer?.Dispose();
        }

        // ════════════════════════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════════════════════════

        private static RectangleF PadRect(Rectangle r, int hp, int vp)
            => new RectangleF(r.X + hp, r.Y + vp, r.Width - hp * 2, r.Height - vp * 2);

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

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ');
            return parts.Length >= 2
                ? parts[0].Substring(0, 1) + parts[1].Substring(0, 1)
                : name.Substring(0, Math.Min(2, name.Length));
        }
    }

    // ── Display row model ───────────────────────────────────────────────────────
    public class MonitorRowItem
    {
        public string InstanceId      { get; set; }
        public string WorkflowName    { get; set; }
        public string AssigneeName    { get; set; }
        public string StartDate       { get; set; }
        public string StepProgress    { get; set; }   // "2/3"
        public string CurrentStepName { get; set; }
        public string StatusText      { get; set; }   // "Pending", "OD", etc.
    }
}
