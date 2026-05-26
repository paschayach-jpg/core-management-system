using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Workflow Dashboard — Navy Blue / Office 2022 / Windows Explorer layout.
    /// Left nav sidebar + ribbon tabs + icon toolbar + owner-drawn task list.
    /// </summary>
    public partial class WorkflowDashboardFormNew : Form
    {
        // ── Dependencies ────────────────────────────────────────────────────────
        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowTemplateService _templateService;
        private readonly ISLACalculator _slaCalculator;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;

        // ── State ────────────────────────────────────────────────────────────────
        private List<WorkflowTaskItem> _allTasks = new List<WorkflowTaskItem>();
        private int _currentPage = 1;
        private int _totalPages = 1;
        private const int PageSize = 10;
        private Timer _refreshTimer;

        // ── Nav ──────────────────────────────────────────────────────────────────
        private Panel _activeNavItem;
        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover = Color.FromArgb(245, 249, 255);

        public WorkflowDashboardFormNew(
            IWorkflowService workflowService = null,
            ISLACalculator slaCalculator = null,
            ILoggingService loggingService = null,
            UserContext userContext = null,
            IWorkflowTemplateService templateService = null)
        {
            _workflowService = workflowService;
            _slaCalculator = slaCalculator;
            _loggingService = loggingService;
            _userContext = userContext;
            _templateService = templateService;
            InitializeComponent();
        }

        private void WorkflowDashboardFormNew_Load(object sender, EventArgs e)
        {
            BuildRibbonTabs();
            BuildToolbar();
            Panel initialNav;
            WorkflowNavigationBuilder.BuildSidebar(
                pnlSidebarContainer, "mytasks", OnNavItemClick, out initialNav);
            _activeNavItem = initialNav;
            SetupListView();
            LoadRealData();
            StartAutoRefresh();
        }

        // ════════════════════════════════════════════════════════════════════════
        // RIBBON TABS
        // ════════════════════════════════════════════════════════════════════════

        private void BuildRibbonTabs()
        {
            var tabs = new[]
            {
                new { Text = "แฟ้ม",      Active = false },
                new { Text = "หน้าแรก",   Active = true  },
                new { Text = "ดำเนินการ", Active = false },
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
                    Font = t.Active
                        ? new Font("Segoe UI", 9.5F, FontStyle.Bold)
                        : new Font("Segoe UI", 9.5F),
                    Cursor = Cursors.Hand,
                    TabStop = false,
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 80, 130);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 70, 120);

                if (t.Active)
                {
                    // White underline for active tab
                    btn.Paint += (s, pe) =>
                    {
                        pe.Graphics.FillRectangle(Brushes.White,
                            0, btn.Height - 3, btn.Width, 3);
                    };
                }

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
        // TOOLBAR (icon buttons + group labels)
        // ════════════════════════════════════════════════════════════════════════

        private void BuildToolbar()
        {
            // Group definitions: (icon, label, group-header, onClick-tag)
            var groups = new[]
            {
                new { Header = "ดำเนินการ", Items = new[]
                {
                    new { Icon = "✔", Text = "ดำเนินการ", Tag = "execute"   },
                    new { Icon = "👁", Text = "รายละเอียด",Tag = "detail"    },
                    new { Icon = "→", Text = "ส่งต่อ",    Tag = "forward"   },
                    new { Icon = "↺", Text = "เริ่มใหม่", Tag = "refresh"   },
                }},
                new { Header = "Workflow",   Items = new[]
                {
                    new { Icon = "👥", Text = "Delegation", Tag = "delegation" },
                    new { Icon = "▼", Text = "ตัวกรอง",   Tag = "filter"     },
                    new { Icon = "☰", Text = "รายการ",    Tag = "list"       },
                    new { Icon = "📅", Text = "ปฏิทิน",   Tag = "calendar"   },
                }},
                new { Header = "ส่งออก",    Items = new[]
                {
                    new { Icon = "📊", Text = "Excel",  Tag = "excel" },
                    new { Icon = "🖨", Text = "พิมพ์", Tag = "print"  },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int groupStartX = x;

                foreach (var item in group.Items)
                {
                    var tag = item.Tag;
                    var icon = item.Icon;
                    var label = item.Text;

                    var pnlBtn = new Panel
                    {
                        Location = new Point(x, 2),
                        Size = new Size(58, 60),
                        BackColor = Color.White,
                        Cursor = Cursors.Hand,
                        Tag = tag,
                    };

                    var lblIcon = new Label
                    {
                        Text = icon,
                        Font = new Font("Segoe UI", 16F),
                        ForeColor = _navyDark,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.None,
                        Location = new Point(0, 6),
                        Size = new Size(58, 30),
                    };
                    var lblText = new Label
                    {
                        Text = label,
                        Font = new Font("Segoe UI", 7.5F),
                        ForeColor = Color.FromArgb(55, 65, 90),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, 36),
                        Size = new Size(58, 20),
                    };

                    pnlBtn.Controls.Add(lblIcon);
                    pnlBtn.Controls.Add(lblText);

                    // Hover effects
                    EventHandler enter = (s, ev) =>
                    {
                        pnlBtn.BackColor = _navHover;
                        lblIcon.BackColor = _navHover;
                        lblText.BackColor = _navHover;
                    };
                    EventHandler leave = (s, ev) =>
                    {
                        pnlBtn.BackColor = Color.White;
                        lblIcon.BackColor = Color.White;
                        lblText.BackColor = Color.White;
                    };
                    EventHandler click = (s, ev) => OnToolbarClick(tag);

                    pnlBtn.MouseEnter += enter; lblIcon.MouseEnter += enter; lblText.MouseEnter += enter;
                    pnlBtn.MouseLeave += leave; lblIcon.MouseLeave += leave; lblText.MouseLeave += leave;
                    pnlBtn.Click += click; lblIcon.Click += click; lblText.Click += click;

                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 60;
                }

                // Group label at bottom
                int groupW = x - groupStartX;
                var lblGroup = new Label
                {
                    Text = group.Header,
                    Font = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(groupStartX, 62),
                    Size = new Size(groupW, 10),
                };
                pnlToolbarContainer.Controls.Add(lblGroup);

                // Group separator
                if (group != groups[groups.Length - 1])
                {
                    var sep = new Panel
                    {
                        BackColor = Color.FromArgb(220, 225, 235),
                        Location = new Point(x + 2, 8),
                        Size = new Size(1, 54),
                    };
                    pnlToolbarContainer.Controls.Add(sep);
                    x += 10;
                }
            }

            // Bottom border of toolbar
            var toolBorder = new Panel
            {
                BackColor = Color.FromArgb(220, 225, 235),
                Dock = DockStyle.Bottom,
                Height = 1,
            };
            pnlToolbarContainer.Controls.Add(toolBorder);
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "refresh": _currentPage = 1; LoadRealData(); break;
                case "filter": txtSearch.Focus(); break;
                case "execute":
                case "detail":
                    if (lvTasks.SelectedItems.Count == 0)
                        MessageBox.Show("กรุณาเลือกรายการก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                default:
                    MessageBox.Show(string.Format("ฟังก์ชัน '{0}' อยู่ระหว่างพัฒนา", tag),
                        "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // SIDEBAR NAVIGATION
        // ════════════════════════════════════════════════════════════════════════

        private void OnNavItemClick(string tag)
        {
            // Handle local navigation first
            switch (tag)
            {
                case "mytasks":
                    lblTitle.Text = "งาน Workflow ของฉัน";
                    lblBreadcrumb.Text = "Core Management System  ›  Workflow Engine  ›  งานของฉัน";
                    _currentPage = 1;
                    LoadRealData();
                    return;
            }

            // Delegate to navigation handler for other forms
            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                _workflowService,
                _templateService,
                _slaCalculator,
                _loggingService,
                _userContext);
        }

        // ════════════════════════════════════════════════════════════════════════
        // LISTVIEW SETUP + OWNER DRAW
        // ════════════════════════════════════════════════════════════════════════

        private void SetupListView()
        {
            // Force 46px row height via ImageList trick
            var img = new ImageList { ImageSize = new Size(1, 46) };
            lvTasks.SmallImageList = img;

            lvTasks.DrawColumnHeader += LvTasks_DrawColumnHeader;
            lvTasks.DrawItem += LvTasks_DrawItem;
            lvTasks.DrawSubItem += LvTasks_DrawSubItem;

            cmbFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            cmbFilterType.SelectedIndexChanged += (s, e) => ApplyFilter();
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            btnClearFilter.Click += (s, e) => { cmbFilterStatus.SelectedIndex = 0; cmbFilterType.SelectedIndex = 0; txtSearch.Text = ""; };
            btnPrev.Click += (s, e) => GoPage(-1);
            btnNext.Click += (s, e) => GoPage(+1);
            btnExportHeader.Click += (s, e) => MessageBox.Show("ฟังก์ชัน Export อยู่ระหว่างพัฒนา", "ข้อมูล");
            lvTasks.SelectedIndexChanged += (s, e) => UpdateSelectionInfo();
        }

        // ── Column header painting ───────────────────────────────────────────────

        private void LvTasks_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            var g = e.Graphics;
            g.FillRectangle(new SolidBrush(Color.FromArgb(246, 248, 251)), e.Bounds);
            // Bottom border
            g.DrawLine(new Pen(Color.FromArgb(220, 224, 232)), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            // Header text
            if (e.ColumnIndex == 0) return; // checkbox column: leave blank
            using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            using (var f = new Font("Segoe UI Semibold", 9F, FontStyle.Bold))
            {
                var r = new RectangleF(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height);
                g.DrawString(e.Header.Text, f, new SolidBrush(Color.FromArgb(55, 65, 90)), r, sf);
            }
        }

        // ── Row painting ─────────────────────────────────────────────────────────

        private void LvTasks_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // Row background
            bool sel = e.Item.Selected;
            var bg = sel ? Color.FromArgb(232, 241, 255) : (e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253));
            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
            // Bottom row line
            e.Graphics.DrawLine(new Pen(Color.FromArgb(235, 238, 244)), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void LvTasks_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var r = e.Bounds;
            var task = e.Item.Tag as WorkflowTaskItem;
            if (task == null) return;

            bool sel = e.Item.Selected;
            var bg = sel ? Color.FromArgb(232, 241, 255) : (e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253));

            switch (e.ColumnIndex)
            {
                case 0: // checkbox
                    DrawCheckBox(g, r, sel);
                    break;

                case 1: // WFI code — blue link style
                    using (var f = new Font("Segoe UI", 9F))
                    using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                        g.DrawString(task.WorkflowId, f, new SolidBrush(Color.FromArgb(0, 90, 170)), PadRect(r, 10, 0), sf);
                    break;

                case 2: // Workflow title + step (two lines)
                    DrawWorkflowCell(g, r, task);
                    break;

                case 3: // Assignee: avatar + name + role
                    DrawAssigneeCell(g, r, task);
                    break;

                case 4: // Due date + relative
                    DrawDueDateCell(g, r, task);
                    break;

                case 5: // SLA progress bar
                    DrawSlaCell(g, r, task);
                    break;

                case 6: // Status badge pill
                    DrawStatusBadge(g, r, task.Status);
                    break;
            }
        }

        private void DrawCheckBox(Graphics g, Rectangle r, bool checked_)
        {
            int sz = 14;
            var box = new Rectangle(r.X + (r.Width - sz) / 2, r.Y + (r.Height - sz) / 2, sz, sz);
            if (checked_)
            {
                g.FillRectangle(new SolidBrush(_navyDark), box);
                g.DrawString("✓", new Font("Segoe UI", 8F, FontStyle.Bold), Brushes.White,
                    new RectangleF(box.X - 1, box.Y, box.Width + 2, box.Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            else
            {
                g.DrawRectangle(new Pen(Color.FromArgb(190, 198, 215)), box);
            }
        }

        private void DrawWorkflowCell(Graphics g, Rectangle r, WorkflowTaskItem task)
        {
            var rf = PadRect(r, 10, 4);
            using (var fBold = new Font("Segoe UI", 9.5F, FontStyle.Bold))
            using (var fSmall = new Font("Segoe UI", 8.5F))
            {
                var titleR = new RectangleF(rf.X, rf.Y, rf.Width, 20);
                var stepR = new RectangleF(rf.X, rf.Y + 22, rf.Width, 18);
                g.DrawString(task.WorkflowTitle, fBold, new SolidBrush(Color.FromArgb(30, 36, 55)), titleR,
                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                g.DrawString("↳ " + task.WorkflowStep, fSmall, new SolidBrush(Color.FromArgb(110, 120, 140)), stepR,
                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
            }
        }

        private static readonly Color[] AvatarColors =
        {
            Color.FromArgb(0,  102, 180), Color.FromArgb(155, 89,  182),
            Color.FromArgb(22, 160, 133), Color.FromArgb(192, 57,  43),
            Color.FromArgb(41, 128, 185), Color.FromArgb(230, 126, 34),
        };

        private void DrawAssigneeCell(Graphics g, Rectangle r, WorkflowTaskItem task)
        {
            int avSz = 28;
            int avX = r.X + 10;
            int avY = r.Y + (r.Height - avSz) / 2;
            var avColor = AvatarColors[Math.Abs(task.AssigneeName.GetHashCode()) % AvatarColors.Length];
            var avRect = new Rectangle(avX, avY, avSz, avSz);

            using (var b = new SolidBrush(avColor))
            using (var path = RoundedRect(avRect, avSz / 2))
                g.FillPath(b, path);

            var initials = GetInitials(task.AssigneeName);
            g.DrawString(initials, new Font("Segoe UI", 9F, FontStyle.Bold), Brushes.White, avRect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            using (var fName = new Font("Segoe UI", 9F))
            using (var fRole = new Font("Segoe UI", 8F))
            {
                var nameR = new RectangleF(avX + avSz + 6, r.Y + 7, r.Width - avSz - 26, 18);
                var roleR = new RectangleF(avX + avSz + 6, r.Y + 24, r.Width - avSz - 26, 16);
                g.DrawString(task.AssigneeName, fName, new SolidBrush(Color.FromArgb(30, 36, 55)), nameR,
                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                g.DrawString(task.AssigneeRole, fRole, new SolidBrush(Color.FromArgb(110, 120, 140)), roleR,
                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
            }
        }

        private void DrawDueDateCell(Graphics g, Rectangle r, WorkflowTaskItem task)
        {
            var rf = PadRect(r, 10, 4);
            using (var fDate = new Font("Segoe UI", 9F))
            using (var fRel = new Font("Segoe UI", 8F))
            {
                g.DrawString(task.DueDate, fDate, new SolidBrush(Color.FromArgb(30, 36, 55)),
                    new RectangleF(rf.X, rf.Y, rf.Width, 20));
                var relColor = task.DaysLeft < 0 ? Color.FromArgb(194, 38, 54)
                             : task.DaysLeft < 3 ? Color.FromArgb(197, 120, 16)
                             : Color.FromArgb(100, 115, 135);
                g.DrawString(task.DaysLeft < 0
                    ? string.Format("เกิน {0} วัน", -task.DaysLeft)
                    : string.Format("เหลือ {0} วัน", task.DaysLeft),
                    fRel, new SolidBrush(relColor),
                    new RectangleF(rf.X, rf.Y + 22, rf.Width, 18));
            }
        }

        private void DrawSlaCell(Graphics g, Rectangle r, WorkflowTaskItem task)
        {
            int barW = r.Width - 20;
            int barH = 8;
            int barX = r.X + 10;
            int barY = r.Y + (r.Height - barH) / 2 - 6;
            int fill = (int)(barW * Math.Min(1.0, task.SlaPercent / 100.0));

            // Background
            g.FillRectangle(new SolidBrush(Color.FromArgb(228, 232, 240)), barX, barY, barW, barH);

            // Fill color: red if SLA exceeded, orange if <30% left, green otherwise
            Color barColor;
            if (task.SlaPercent >= 100) barColor = Color.FromArgb(194, 38, 54);
            else if (task.SlaPercent >= 70) barColor = Color.FromArgb(197, 120, 16);
            else barColor = Color.FromArgb(23, 133, 49);
            g.FillRectangle(new SolidBrush(barColor), barX, barY, fill, barH);

            // Days text
            string dayText = task.DaysLeft < 0
                ? string.Format("-{0} วัน", -task.DaysLeft)
                : string.Format("{0} วัน", task.DaysLeft);
            using (var f = new Font("Segoe UI", 8F))
                g.DrawString(dayText, f, new SolidBrush(barColor),
                    new RectangleF(barX, barY + barH + 4, barW, 14),
                    new StringFormat { Alignment = StringAlignment.Far });
        }

        private static readonly Dictionary<string, Color> StatusColors = new Dictionary<string, Color>
        {
            { "เกิน SLA",      Color.FromArgb(194, 38,  54)  },
            { "รอดำเนินการ",   Color.FromArgb(197, 120, 16)  },
            { "รอ",            Color.FromArgb(80,  130, 200)  },
            { "เสร็จ",         Color.FromArgb(23,  133, 49)  },
        };

        private void DrawStatusBadge(Graphics g, Rectangle r, string status)
        {
            Color c;
            if (!StatusColors.TryGetValue(status, out c)) c = Color.FromArgb(120, 130, 150);

            // Badge pill
            var sz = g.MeasureString(status, new Font("Segoe UI", 8.5F, FontStyle.Bold));
            int bw = (int)sz.Width + 18;
            int bh = 20;
            int bx = r.X + 10;
            int by = r.Y + (r.Height - bh) / 2;
            var br = new Rectangle(bx, by, bw, bh);

            using (var bg = new SolidBrush(Color.FromArgb(30, c)))
            using (var path = RoundedRect(br, 4))
                g.FillPath(bg, path);

            using (var pen = new Pen(Color.FromArgb(80, c)))
            using (var path = RoundedRect(br, 4))
                g.DrawPath(pen, path);

            g.DrawString(status, new Font("Segoe UI", 8.5F, FontStyle.Bold), new SolidBrush(c),
                new RectangleF(bx, by, bw, bh),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        // ════════════════════════════════════════════════════════════════════════
        // DATA
        // ════════════════════════════════════════════════════════════════════════

        private void LoadRealData()
        {
            try
            {
                if (_workflowService == null || _userContext == null)
                {
                    LoadMockData();
                    return;
                }

                var tasks = _workflowService.GetMyTasks(_userContext);
                if (tasks == null || tasks.Count == 0)
                {
                    _allTasks.Clear();
                    ApplyFilter();
                    return;
                }

                _allTasks = tasks.Select(MapToTaskItem).ToList();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _loggingService?.Error("Cannot load workflow tasks", ex);
                MessageBox.Show("ไม่สามารถโหลดรายการงานได้: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadMockData();
            }
        }

        private WorkflowTaskItem MapToTaskItem(WorkflowTaskResult result)
        {
            var dueDate = result.SLADeadline;
            var now = DateTime.Now;
            var daysLeft = (int)(dueDate - now).TotalDays;

            var slaPercent = 0;
            if (result.TimeRemainingMinutes.HasValue)
            {
                var totalMinutes = (dueDate - result.AssignedDate).TotalMinutes;
                var elapsedPercent = (totalMinutes - result.TimeRemainingMinutes.Value) / totalMinutes * 100;
                slaPercent = Math.Min(100, (int)elapsedPercent);
            }

            var statusText = result.SLAStatus == SLAStatus.Overdue ? "เกิน SLA"
                           : result.Status == WorkflowStepInstanceStatus.Pending ? "รอดำเนินการ"
                           : result.Status == WorkflowStepInstanceStatus.Completed ? "เสร็จ"
                           : "รอ";

            return new WorkflowTaskItem
            {
                WorkflowId = $"WFI-{result.WorkflowInstanceId:D5}",
                WorkflowTitle = result.WorkflowName ?? "ไม่มีชื่อ",
                WorkflowStep = result.StepName ?? "ไม่ระบุ",
                AssigneeName = ExtractDisplayName(result.InitiatedBy) ?? "ไม่ระบุ",
                AssigneeRole = "Officer",
                DueDate = dueDate.ToString("dd/MM/yy"),
                DaysLeft = daysLeft,
                SlaPercent = slaPercent,
                Status = statusText,
                CompletedDate = result.CompletedDate
            };
        }

        private string ExtractDisplayName(string adUser)
        {
            if (string.IsNullOrWhiteSpace(adUser)) return null;
            var parts = adUser.Split('\\');
            return parts.Length > 1 ? parts[1] : adUser;
        }

        private void LoadMockData()
        {
            _allTasks = new List<WorkflowTaskItem>
            {
                new WorkflowTaskItem { WorkflowId="WFI-2569-00012", WorkflowTitle="อนุมัติ KB Article",         WorkflowStep="ผู้จัดการตรวจสอบเนื้อหา (2/3)", AssigneeName="สมชาย กาญจนา",   AssigneeRole="Officer · IT",     DueDate="17/04/69", DaysLeft=-2,  SlaPercent=102, Status="เกิน SLA"    },
                new WorkflowTaskItem { WorkflowId="WFI-2569-00015", WorkflowTitle="ขออนุมัติ Upload ไฟล์ 1HC", WorkflowStep="ผู้จัดการตรวจสอบไฟล์ (1/2)",   AssigneeName="วิภา สุริยวงศ์",  AssigneeRole="Sr.Officer · HR",  DueDate="20/04/69", DaysLeft=2,   SlaPercent=60,  Status="รอดำเนินการ" },
                new WorkflowTaskItem { WorkflowId="WFI-2569-00018", WorkflowTitle="อนุมัติเปลี่ยน Permission",  WorkflowStep="ตรวจสอบสิทธิ์ (1/3)",           AssigneeName="ประวิทย์ ชัยวงศ์", AssigneeRole="Officer · Ops",    DueDate="25/04/69", DaysLeft=7,   SlaPercent=40,  Status="รอ"           },
                new WorkflowTaskItem { WorkflowId="WFI-2569-00020", WorkflowTitle="Review SOP Document",       WorkflowStep="Review เนื้อหา (1/2)",           AssigneeName="สุนิสา รัตนพร",   AssigneeRole="Officer · QA",     DueDate="30/04/69", DaysLeft=12,  SlaPercent=20,  Status="รอ"           },
                new WorkflowTaskItem { WorkflowId="WFI-2569-00023", WorkflowTitle="อนุมัติ Work Instruction",  WorkflowStep="ผู้จัดการอนุมัติ (1/3)",         AssigneeName="นิรุตต์ รัฒนกุล", AssigneeRole="Manager · Docs",   DueDate="02/05/69", DaysLeft=14,  SlaPercent=10,  Status="รอ"           },
                new WorkflowTaskItem { WorkflowId="WFI-2569-00025", WorkflowTitle="อนุมัติ Knowledge Article", WorkflowStep="ตรวจสอบเนื้อหา (1/2)",          AssigneeName="กัลยา สิทธิวงศ์", AssigneeRole="SrOfficer · KB",   DueDate="05/05/69", DaysLeft=18,  SlaPercent=5,   Status="รอ"           },
                new WorkflowTaskItem { WorkflowId="WFI-2569-00028", WorkflowTitle="ขออนุมัติ Upload ไฟล์ NC",  WorkflowStep="ผู้จัดการตรวจสอบ (1/2)",        AssigneeName="อดิศักดิ์ วงษ์ดี",AssigneeRole="Officer · IT",     DueDate="01/05/69", DaysLeft=10,  SlaPercent=30,  Status="รอดำเนินการ" },
                new WorkflowTaskItem { WorkflowId="WFI-2569-00030", WorkflowTitle="อนุมัติแก้ไข Template",    WorkflowStep="ตรวจสอบ Template (2/3)",         AssigneeName="ชลธิชา มีสุข",    AssigneeRole="Officer · Admin",  DueDate="28/04/69", DaysLeft=6,   SlaPercent=50,  Status="รอดำเนินการ" },
            };

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = _allTasks.AsEnumerable();

            string q = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(q))
                filtered = filtered.Where(t =>
                    t.WorkflowId.ToLower().Contains(q) ||
                    t.WorkflowTitle.ToLower().Contains(q) ||
                    t.AssigneeName.ToLower().Contains(q));

            if (cmbFilterStatus.SelectedIndex > 0)
            {
                var s = cmbFilterStatus.SelectedItem.ToString();
                filtered = filtered.Where(t => t.Status.Contains(s.Replace("ส", "ส")));
            }
            if (cmbFilterType.SelectedIndex > 0)
            {
                var s = cmbFilterType.SelectedItem.ToString();
                filtered = filtered.Where(t => t.WorkflowTitle.Contains(s));
            }

            var result = filtered.ToList();
            _totalPages = Math.Max(1, (int)Math.Ceiling(result.Count / (double)PageSize));
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            var paged = result.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

            lvTasks.BeginUpdate();
            lvTasks.Items.Clear();
            foreach (var t in paged)
            {
                var item = new ListViewItem("") { Tag = t };
                item.SubItems.AddRange(new[] { t.WorkflowId, t.WorkflowTitle, t.AssigneeName, t.DueDate, "", t.Status });
                lvTasks.Items.Add(item);
            }
            lvTasks.EndUpdate();

            lblPage.Text = string.Format("หน้า {0} / {1}", _currentPage, _totalPages);
            btnPrev.Enabled = _currentPage > 1;
            btnNext.Enabled = _currentPage < _totalPages;
            lblCardPendingCount.Text = result.Count(t => t.Status != "เสร็จ").ToString();
            lblCardSLACount.Text = result.Count(t => t.Status == "เกิน SLA").ToString();
            lblCardDoneCount.Text = result.Count(t => t.Status == "เสร็จ" && t.CompletedDate.HasValue && t.CompletedDate.Value.Date == DateTime.Today).ToString();
            UpdateSelectionInfo();
        }

        private void GoPage(int delta)
        {
            _currentPage += delta;
            ApplyFilter();
        }

        private void UpdateSelectionInfo()
        {
            int total = lvTasks.Items.Count;
            int sel = lvTasks.SelectedItems.Count;
            lblSelectionInfo.Text = sel > 0
                ? string.Format("เลือก {0} รายการจาก {1} รายการ", sel, total)
                : string.Format("แสดง {0} รายการ", total);
        }

        private void StartAutoRefresh()
        {
            _refreshTimer = new Timer { Interval = 60000 };
            _refreshTimer.Tick += (s, e) => lblSubtitle.Text = string.Format(
                "รายการงานที่รอดำเนินการของคุณ — อัปเดตล่าสุด {0}", DateTime.Now.ToString("dd/MM/yy HH:mm"));
            _refreshTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            base.OnFormClosed(e);
        }

        // ════════════════════════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════════════════════════

        private static RectangleF PadRect(Rectangle r, int hPad, int vPad)
            => new RectangleF(r.X + hPad, r.Y + vPad, r.Width - hPad * 2, r.Height - vPad * 2);

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
            if (parts.Length >= 2)
                return parts[0].Substring(0, 1) + parts[1].Substring(0, 1);
            return name.Substring(0, Math.Min(2, name.Length));
        }
    }

    // ── Data model ──────────────────────────────────────────────────────────────
    public class WorkflowTaskItem
    {
        public string WorkflowId { get; set; }
        public string WorkflowTitle { get; set; }
        public string WorkflowStep { get; set; }
        public string AssigneeName { get; set; }
        public string AssigneeRole { get; set; }
        public string DueDate { get; set; }
        public int DaysLeft { get; set; }
        public double SlaPercent { get; set; }
        public string Status { get; set; }
    }

    // Legacy display item kept for backward compatibility
    public class WorkflowDisplayItem
    {
        public string WorkflowId { get; set; }
        public string WorkflowType { get; set; }
        public string AssigneeName { get; set; }
        public string DueDate { get; set; }
        public string Status { get; set; }
    }
}
