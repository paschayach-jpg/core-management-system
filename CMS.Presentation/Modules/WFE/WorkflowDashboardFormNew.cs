using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
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
        private int _hotItemIndex = -1;
        private string _activeStatusTab = "";   // "" = all, "pending" = not done, "done" = done
        private Button[] _statusTabButtons;

        // ── Win32 double-buffer for native ListView ───────────────────────────────
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private const uint LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036;
        private const uint LVS_EX_DOUBLEBUFFER          = 0x00010000;

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
            BuildStatusTabs();
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
                    new { Icon = "↺", Text = "คัดใหม่",   Tag = "refresh"   },
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
                case "execute": ExecuteSelectedTask(); break;
                case "detail":  OpenSelectedDetail();  break;
                case "forward": ForwardSelectedTask(); break;
                case "delegation":
                    WorkflowNavigationHandler.HandleNavigation("delegation", this, _workflowService, _templateService, _slaCalculator, _loggingService, _userContext);
                    break;
                case "list":
                    WorkflowNavigationHandler.HandleNavigation("monitor", this, _workflowService, _templateService, _slaCalculator, _loggingService, _userContext);
                    break;
                case "calendar":
                    MessageBox.Show("ระบบปฏิทินงานอยู่ระหว่างการพัฒนาร่วมกับระบบตารางนัดหมาย", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "excel": ExportTasksToExcel(); break;
                case "print": PrintTasksList(); break;
                default:
                    MessageBox.Show(string.Format("ฟังก์ชัน '{0}' อยู่ระหว่างพัฒนา", tag),
                        "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void ExecuteSelectedTask()
        {
            if (lvTasks.SelectedItems.Count == 0)
            { MessageBox.Show("กรุณาเลือกรายการก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            var item = lvTasks.SelectedItems[0].Tag as WorkflowTaskItem;
            if (item?.OriginalResult == null)
            { MessageBox.Show("ไม่พบข้อมูลงาน", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            var task = item.OriginalResult;
            if (task.Status == WorkflowStepInstanceStatus.Completed ||
                task.Status == WorkflowStepInstanceStatus.Rejected)
            {
                MessageBox.Show("งานนี้ดำเนินการเสร็จสิ้นแล้ว ไม่สามารถดำเนินการซ้ำได้",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var actionForm = new CMS.Presentation.Modules.Workflow.Dialogs.WorkflowStepActionForm(task))
            {
                actionForm.AllowForward = task.CanDelegate;
                actionForm.DueDate      = task.SLADeadline;
                actionForm.InitiatorName = task.InitiatedBy;

                if (actionForm.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    string action   = actionForm.SelectedAction;
                    string comments = actionForm.Comments;

                    if (string.Equals(action, "Reject", StringComparison.OrdinalIgnoreCase))
                    {
                        _workflowService.RejectWorkflowStep(new RejectWorkflowStepRequest
                        {
                            WorkflowInstanceId     = task.WorkflowInstanceId,
                            WorkflowStepInstanceId = task.WorkflowStepInstanceId,
                            RejectionReason        = comments,
                            Remarks                = comments
                        }, _userContext);
                    }
                    else
                    {
                        _workflowService.CompleteWorkflowStep(new CompleteWorkflowStepRequest
                        {
                            WorkflowInstanceId     = task.WorkflowInstanceId,
                            WorkflowStepInstanceId = task.WorkflowStepInstanceId,
                            OutputData             = null,
                            Remarks                = comments
                        }, _userContext);
                    }

                    MessageBox.Show("ดำเนินการเสร็จสิ้น", "Workflow", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _currentPage = 1;
                    LoadRealData();
                }
                catch (Exception ex)
                {
                    _loggingService?.Error("Failed to complete workflow task", ex);
                    MessageBox.Show("ดำเนินการไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenSelectedDetail()
        {
            if (lvTasks.SelectedItems.Count == 0)
            { MessageBox.Show("กรุณาเลือกรายการก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            var item = lvTasks.SelectedItems[0].Tag as WorkflowTaskItem;
            if (item == null || item.WorkflowInstanceId <= 0) return;

            WorkflowNavigationHandler.HandleNavigation(
                "detail", this, _workflowService, _templateService,
                _slaCalculator, _loggingService, _userContext,
                workflowInstanceId: item.WorkflowInstanceId);
        }

        private void ForwardSelectedTask()
        {
            if (lvTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var item = lvTasks.SelectedItems[0].Tag as WorkflowTaskItem;
            if (item?.OriginalResult == null) return;
            var task = item.OriginalResult;

            if (task.Status == WorkflowStepInstanceStatus.Completed || task.Status == WorkflowStepInstanceStatus.Rejected)
            {
                MessageBox.Show("งานนี้ดำเนินการเสร็จสิ้นแล้ว ไม่สามารถส่งต่อได้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CMS.Services.Interfaces.IWorkflowDelegationService delegationService = null;
            if (this.MdiParent is MDI_Container mdi)
            {
                delegationService = mdi.WorkflowDelegationService;
            }

            if (delegationService == null)
            {
                MessageBox.Show("Delegation service ไม่พร้อมใช้งาน", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new CMS.Presentation.Modules.Workflow.Dialogs.ReassignDialog(task))
            {
                bool isOverSla = task.SLAStatus == CMS.Domain.Enums.SLAStatus.Overdue;
                int daysOver = isOverSla ? Math.Max(0, (DateTime.Now - task.SLADeadline).Days) : 0;

                dialog.CurrentAssignee = new CMS.Presentation.Modules.Workflow.Dialogs.ReassignDialog.AssigneeInfo
                {
                    Name = ExtractDisplayName(_userContext?.ADUserName) ?? "ผู้รับผิดชอบ",
                    Role = _userContext?.PositionName ?? "Officer",
                    Dept = _userContext?.DepartmentName ?? "Legal",
                    IsOverSLA = isOverSla,
                    SLADaysOver = daysOver
                };

                dialog.Candidates = new List<CMS.Presentation.Modules.Workflow.Dialogs.ReassignDialog.CandidateInfo>
                {
                    new CMS.Presentation.Modules.Workflow.Dialogs.ReassignDialog.CandidateInfo { UserId = "สมชาย", Name = "สมชาย กาญจนา", Role = "Staff", MatchScore = 90 },
                    new CMS.Presentation.Modules.Workflow.Dialogs.ReassignDialog.CandidateInfo { UserId = "วิชัย", Name = "วิชัย มีสุข", Role = "Manager", MatchScore = 75 }
                };
                dialog.RoleOptions = new List<string> { "ทั้งหมด", "Officer", "Manager" };

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        string delegateTo = !string.IsNullOrWhiteSpace(dialog.SelectedUserId)
                            ? dialog.SelectedUserId
                            : dialog.SelectedUserName;

                        delegationService.DelegateTask(new DelegateTaskRequest
                        {
                            WorkflowStepInstanceId = task.WorkflowStepInstanceId,
                            DelegateToUserName = delegateTo,
                            DelegationReason = dialog.ReassignReason,
                            Remarks = dialog.MessageToNew
                        }, _userContext);

                        MessageBox.Show("ส่งต่องานสำเร็จ", "Workflow", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _currentPage = 1;
                        LoadRealData();
                    }
                    catch (Exception ex)
                    {
                        _loggingService?.Error("Failed to delegate task from dashboard", ex);
                        MessageBox.Show("ส่งต่องานไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportTasksToExcel()
        {
            if (_allTasks == null || _allTasks.Count == 0)
            {
                MessageBox.Show("ไม่มีข้อมูลงานสำหรับส่งออก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File (*.csv)|*.csv";
                sfd.FileName = string.Format("workflow-tasks-{0:dd-MM-yyyy}.csv", DateTime.Now);
                sfd.Title = "ส่งออกข้อมูลงาน Workflow";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            sw.Write('\uFEFF'); // Write BOM for UTF-8
                            sw.WriteLine("รหัส,Workflow/Step,ผู้รับผิดชอบ,กำหนด,SLA %,สถานะ");

                            foreach (var task in _allTasks)
                            {
                                string titleAndStep = string.Format("{0} - {1}", task.WorkflowTitle, task.WorkflowStep);
                                sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4:F1}%\",\"{5}\"",
                                    EscapeCsv(task.WorkflowId),
                                    EscapeCsv(titleAndStep),
                                    EscapeCsv(task.AssigneeName),
                                    task.DueDate,
                                    task.SlaPercent,
                                    EscapeCsv(task.Status)));
                            }
                        }
                        MessageBox.Show("ส่งออกข้อมูลสำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ส่งออกข้อมูลไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;
            return field.Replace("\"", "\"\"");
        }

        private void PrintTasksList()
        {
            if (_allTasks == null || _allTasks.Count == 0)
            {
                MessageBox.Show("ไม่มีข้อมูลสำหรับพิมพ์", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var pd = new System.Drawing.Printing.PrintDocument())
            {
                pd.DocumentName = "Workflow Tasks List";
                pd.PrintPage += (sender, ev) =>
                {
                    var g = ev.Graphics;
                    var fontTitle = new Font("Segoe UI", 14F, FontStyle.Bold);
                    var fontHeader = new Font("Segoe UI", 9F, FontStyle.Bold);
                    var fontRow = new Font("Segoe UI", 9F);
                    var brush = Brushes.Black;

                    int y = 50;
                    g.DrawString("รายการงาน Workflow ของฉัน", fontTitle, brush, 50, y);
                    y += 40;

                    // Headers
                    g.DrawString("รหัส", fontHeader, brush, 50, y);
                    g.DrawString("Workflow / Step", fontHeader, brush, 150, y);
                    g.DrawString("ผู้รับผิดชอบ", fontHeader, brush, 450, y);
                    g.DrawString("สถานะ", fontHeader, brush, 650, y);
                    g.DrawLine(Pens.Black, 50, y + 20, 750, y + 20);
                    y += 30;

                    // Rows
                    foreach (var task in _allTasks)
                    {
                        if (y > ev.MarginBounds.Bottom - 50)
                        {
                            ev.HasMorePages = true;
                            return;
                        }

                        g.DrawString(task.WorkflowId, fontRow, brush, 50, y);
                        string text = task.WorkflowTitle;
                        if (text.Length > 35) text = text.Substring(0, 32) + "...";
                        g.DrawString(text + "\n↳ " + task.WorkflowStep, fontRow, brush, 150, y);
                        g.DrawString(task.AssigneeName, fontRow, brush, 450, y);
                        g.DrawString(task.Status, fontRow, brush, 650, y);
                        y += 40;
                    }
                    ev.HasMorePages = false;
                };

                using (var ppd = new PrintPreviewDialog())
                {
                    ppd.Document = pd;
                    ppd.WindowState = FormWindowState.Maximized;
                    ppd.ShowDialog(this);
                }
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
        // STATUS TAB STRIP
        // ════════════════════════════════════════════════════════════════════════

        private void BuildStatusTabs()
        {
            pnlStatusTabs.Controls.Clear();

            var tabDefs = new[]
            {
                new { Key = "",        Label = "ทั้งหมด" },
                new { Key = "pending", Label = "รอดำเนินการ" },
                new { Key = "done",    Label = "เสร็จแล้ว" },
            };

            _statusTabButtons = new Button[tabDefs.Length];
            int x = 8;
            for (int i = 0; i < tabDefs.Length; i++)
            {
                string key   = tabDefs[i].Key;
                string label = tabDefs[i].Label;
                bool active  = key == _activeStatusTab;

                var btn = new Button
                {
                    Text      = label,
                    Tag       = key,
                    Location  = new Point(x, 0),
                    Size      = new Size(140, 38),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = active ? _navyDark : Color.FromArgb(110, 120, 140),
                    Font      = active
                        ? new Font("Segoe UI", 9F, FontStyle.Bold)
                        : new Font("Segoe UI", 9F),
                    Cursor    = Cursors.Hand,
                    TabStop   = false,
                };
                btn.FlatAppearance.BorderSize         = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 244, 250);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 238, 250);

                if (active)
                    btn.Paint += (s, pe) =>
                        pe.Graphics.FillRectangle(new SolidBrush(_navyDark), 0, btn.Height - 2, btn.Width, 2);

                string capturedKey = key;
                btn.Click += (s, e) =>
                {
                    _activeStatusTab = capturedKey;
                    _currentPage = 1;
                    BuildStatusTabs();
                    ApplyFilter();
                };

                _statusTabButtons[i] = btn;
                pnlStatusTabs.Controls.Add(btn);
                x += btn.Width + 2;
            }

            // Bottom separator line
            pnlStatusTabs.Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(220, 225, 235),
                Dock      = DockStyle.Bottom,
                Height    = 1,
            });
        }

        private void UpdateStatusTabCounts()
        {
            if (_statusTabButtons == null || _statusTabButtons.Length < 3) return;

            int total   = _allTasks.Count;
            int pending = _allTasks.Count(t => t.Status != "เสร็จ");
            int done    = _allTasks.Count(t => t.Status == "เสร็จ");

            string[] labels =
            {
                string.Format("ทั้งหมด  ({0})", total),
                string.Format("รอดำเนินการ  ({0})", pending),
                string.Format("เสร็จแล้ว  ({0})", done),
            };

            for (int i = 0; i < labels.Length; i++)
                if (_statusTabButtons[i] != null)
                    _statusTabButtons[i].Text = labels[i];
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
            txtSearch.Enter += (s, e) =>
            {
                if (txtSearch.Text == "ค้นหา...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = System.Drawing.SystemColors.WindowText;
                }
            };
            txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "ค้นหา...";
                    txtSearch.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
                }
            };
            btnClearFilter.Click += (s, e) => { cmbFilterStatus.SelectedIndex = 0; cmbFilterType.SelectedIndex = 0; txtSearch.Text = ""; };
            btnPrev.Click += (s, e) => GoPage(-1);
            btnNext.Click += (s, e) => GoPage(+1);
            btnExportHeader.Click += (s, e) => MessageBox.Show("ฟังก์ชัน Export อยู่ระหว่างพัฒนา", "ข้อมูล");
            lvTasks.SelectedIndexChanged += (s, e) => UpdateSelectionInfo();
            lvTasks.DoubleClick += (s, e) => OpenSelectedDetail();
            lvTasks.ShowItemToolTips = false;
            SendMessage(lvTasks.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, (IntPtr)LVS_EX_DOUBLEBUFFER, (IntPtr)LVS_EX_DOUBLEBUFFER);

            lvTasks.MouseMove += (s, e) =>
            {
                int idx = lvTasks.HitTest(e.Location).Item?.Index ?? -1;
                if (idx == _hotItemIndex) return;
                int old = _hotItemIndex;
                _hotItemIndex = idx;
                InvalidateListRow(old);
                InvalidateListRow(idx);
            };
            lvTasks.MouseLeave += (s, e) =>
            {
                if (_hotItemIndex == -1) return;
                int old = _hotItemIndex;
                _hotItemIndex = -1;
                InvalidateListRow(old);
            };
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
            bool sel = e.Item.Selected;
            bool hot = e.ItemIndex == _hotItemIndex;
            var bg = sel ? Color.FromArgb(232, 241, 255)
                   : hot ? Color.FromArgb(240, 245, 255)
                   : (e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253));
            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(235, 238, 244)), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void LvTasks_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var r = e.Bounds;
            var task = e.Item.Tag as WorkflowTaskItem;

            bool sel = e.Item.Selected;
            bool hot = e.ItemIndex == _hotItemIndex;
            var bg = sel ? Color.FromArgb(232, 241, 255)
                   : hot ? Color.FromArgb(240, 245, 255)
                   : (e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253));

            // Always fill cell background first — prevents hover hot-track from erasing content
            g.FillRectangle(new SolidBrush(bg), r);

            if (task == null) return;

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
                var stepR  = new RectangleF(rf.X, rf.Y + 22, rf.Width, 18);

                g.DrawString(task.WorkflowTitle, fBold, new SolidBrush(Color.FromArgb(30, 36, 55)), titleR,
                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

                var stepLabel = task.TotalSteps > 0
                    ? string.Format("↳ {0}  ({1}/{2})", task.WorkflowStep, task.StepOrder, task.TotalSteps)
                    : "↳ " + task.WorkflowStep;
                g.DrawString(stepLabel, fSmall, new SolidBrush(Color.FromArgb(110, 120, 140)), stepR,
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

            double slaPercent = task.SlaPercent;
            int daysLeft = task.DaysLeft;
            bool isCompleted = task.Status == "เสร็จ";
            string dayText;

            if (task.OriginalResult != null)
            {
                var deadline = task.OriginalResult.SLADeadline;
                var assigned  = task.OriginalResult.AssignedDate;
                // For completed tasks use completion time; otherwise use now (live)
                var reference = (isCompleted && task.CompletedDate.HasValue)
                    ? task.CompletedDate.Value
                    : DateTime.Now;
                daysLeft = (int)(deadline - reference).TotalDays;
                var totalMinutes = (deadline - assigned).TotalMinutes;
                if (totalMinutes > 0)
                {
                    var elapsed = (reference - assigned).TotalMinutes;
                    slaPercent = Math.Max(0, Math.Min(100, elapsed / totalMinutes * 100));
                }
                else
                {
                    slaPercent = 100;
                }
            }

            // For completed tasks: always show full green bar at completion point
            if (isCompleted)
            {
                Color doneColor = Color.FromArgb(23, 133, 49);
                int fill100 = (int)(barW * Math.Min(1.0, slaPercent / 100.0));
                g.FillRectangle(new SolidBrush(Color.FromArgb(228, 232, 240)), barX, barY, barW, barH);
                g.FillRectangle(new SolidBrush(doneColor), barX, barY, fill100, barH);
                using (var f = new Font("Segoe UI", 8F))
                    g.DrawString("✓ เสร็จ", f, new SolidBrush(doneColor),
                        new RectangleF(barX, barY + barH + 4, barW, 14),
                        new StringFormat { Alignment = StringAlignment.Far });
                return;
            }

            int fill = (int)(barW * Math.Min(1.0, slaPercent / 100.0));

            g.FillRectangle(new SolidBrush(Color.FromArgb(228, 232, 240)), barX, barY, barW, barH);

            Color barColor;
            if (slaPercent >= 100) barColor = Color.FromArgb(194, 38, 54);
            else if (slaPercent >= 70) barColor = Color.FromArgb(197, 120, 16);
            else barColor = Color.FromArgb(23, 133, 49);
            g.FillRectangle(new SolidBrush(barColor), barX, barY, fill, barH);

            dayText = daysLeft < 0
                ? string.Format("-{0} วัน", -daysLeft)
                : string.Format("{0} วัน", daysLeft);
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

            // Completion/Rejection always wins over SLA status —
            // a finished step that was late should show "เสร็จ", not "เกิน SLA".
            var statusText = result.Status == WorkflowStepInstanceStatus.Completed ? "เสร็จ"
                           : result.Status == WorkflowStepInstanceStatus.Rejected  ? "ปฏิเสธ"
                           : result.SLAStatus == SLAStatus.Overdue                 ? "เกิน SLA"
                           : result.Status == WorkflowStepInstanceStatus.Pending   ? "รอดำเนินการ"
                           : "รอ";

            return new WorkflowTaskItem
            {
                WorkflowId = $"WFI-{result.WorkflowInstanceId:D5}",
                WorkflowTitle = result.WorkflowName ?? "ไม่มีชื่อ",
                WorkflowStep = result.StepName ?? "ไม่ระบุ",
                StepOrder    = result.StepOrder,
                TotalSteps   = result.TotalSteps,
                AssigneeName = ExtractDisplayName(result.InitiatedBy) ?? "ไม่ระบุ",
                AssigneeRole = "Officer",
                DueDate = dueDate.ToString("dd/MM/yy"),
                DaysLeft = daysLeft,
                SlaPercent = slaPercent,
                Status = statusText,
                CompletedDate = result.CompletedDate,
                WorkflowInstanceId = result.WorkflowInstanceId,
                WorkflowStepInstanceId = result.WorkflowStepInstanceId,
                OriginalResult = result
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
            _allTasks = new List<WorkflowTaskItem>();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = _allTasks.AsEnumerable();

            // Status tab filter (applied first, before other filters)
            if (_activeStatusTab == "pending")
                filtered = filtered.Where(t => t.Status != "เสร็จ");
            else if (_activeStatusTab == "done")
                filtered = filtered.Where(t => t.Status == "เสร็จ");

            string rawQ = txtSearch.Text.Trim();
            string q = (rawQ == "ค้นหา..." || rawQ == "") ? "" : rawQ.ToLower();
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
            UpdateStatusTabCounts();
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
            _refreshTimer.Tick += (s, e) =>
            {
                lblSubtitle.Text = string.Format(
                    "รายการงานที่รอดำเนินการของคุณ — อัปเดตล่าสุด {0}", DateTime.Now.ToString("dd/MM/yy HH:mm"));
                // Repaint SLA column live without reloading from DB
                lvTasks.Invalidate();
            };
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

        private void InvalidateListRow(int index)
        {
            if (index < 0 || index >= lvTasks.Items.Count) return;
            var r = lvTasks.GetItemRect(index, ItemBoundsPortion.Entire);
            r.Width = lvTasks.ClientSize.Width;
            lvTasks.Invalidate(r);
        }

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
        public int StepOrder { get; set; }
        public int TotalSteps { get; set; }
        public string AssigneeName { get; set; }
        public string AssigneeRole { get; set; }
        public string DueDate { get; set; }
        public int DaysLeft { get; set; }
        public double SlaPercent { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; }
        public int WorkflowInstanceId { get; set; }
        public int WorkflowStepInstanceId { get; set; }
        public CMS.Services.DTOs.WorkflowTaskResult OriginalResult { get; set; }
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
