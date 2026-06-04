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
using CMS.Presentation.Modules.Workflow.Dialogs;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using Newtonsoft.Json;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Visual Flow form — แสดง workflow instance เป็น visual canvas พร้อม right panel.
    /// ตาม wfe-form-ui.md pattern: Ribbon + Toolbar + Sidebar (WorkflowNavigationBuilder) + Canvas + Right panel.
    /// </summary>
    public partial class WorkflowVisualFlowForm : Form
    {
        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService  _loggingService;
        private readonly UserContext      _userContext;
        private int _workflowInstanceId;

        // Live data state variables
        private List<WorkflowStep> _definitionSteps = new List<WorkflowStep>();
        private List<WorkflowConnection> _definitionConnections = new List<WorkflowConnection>();
        private List<WorkflowStepInstance> _stepInstances = new List<WorkflowStepInstance>();
        private WorkflowInstanceResult _instanceMetadata;

        private object _selectedNode; // Currently selected step (WorkflowStep) or step instance
        private Dictionary<int, Rectangle> _nodeBounds = new Dictionary<int, Rectangle>();

        private int   _zoomPercent  = 100;
        private Panel _activeNavItem;

        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover = Color.FromArgb(245, 249, 255);

        // Constructor compatible with MDI_Container.cs
        public WorkflowVisualFlowForm(
            IWorkflowService workflowService,
            ILoggingService  loggingService,
            UserContext      userContext)
            : this(workflowService, null, loggingService, userContext, 0)
        {
        }

        // Main constructor supporting workflowInstanceId
        public WorkflowVisualFlowForm(
            IWorkflowService workflowService,
            IWorkflowTemplateService templateService,
            ILoggingService  loggingService,
            UserContext      userContext,
            int workflowInstanceId)
        {
            InitializeComponent();
            _workflowService = workflowService;
            _templateService = templateService;
            _loggingService  = loggingService;
            _userContext     = userContext;
            _workflowInstanceId = workflowInstanceId;
        }

        private void WorkflowVisualFlowForm_Load(object sender, EventArgs e)
        {
            try
            {
                BuildRibbonTabs();
                BuildToolbar();
                Panel initialNav;
                WorkflowNavigationBuilder.BuildSidebar(
                    pnlSidebarContainer, "monitor", OnNavItemClick, out initialNav);
                _activeNavItem = initialNav;
                BuildStatusBar();
                BuildLegendBar();
                BuildRightPanel();
                WireCanvasEvents();

                if (_workflowInstanceId > 0)
                {
                    LoadWorkflowData(_workflowInstanceId);
                }
                else
                {
                    // Delay slightly to allow form layout to initialize and show first
                    this.BeginInvoke(new Action(ShowSelectInstanceDialog));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Visual Flow: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load metadata and step logs from database
        private void LoadWorkflowData(int instanceId)
        {
            try
            {
                if (instanceId <= 0)
                {
                    _workflowInstanceId = 0;
                    _definitionSteps = null;
                    _definitionConnections = null;
                    _stepInstances = null;
                    _instanceMetadata = null;
                    _selectedNode = null;
                    lblTitle.Text = "Visual Flow — No Instance";
                    lblSubtitle.Text = "Please select a workflow instance to view.";
                    lblStepBadge.Text = "";
                    pnlCanvas.Invalidate();
                    return;
                }

                _workflowInstanceId = instanceId;

                // 1. Get workflow instance metadata
                _instanceMetadata = _workflowService.GetWorkflowInstance(instanceId, _userContext);
                if (_instanceMetadata == null)
                {
                    MessageBox.Show("ไม่พบข้อมูล Workflow Instance ID " + instanceId, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string code = "WFI-" + _instanceMetadata.WorkflowInstanceId.ToString("D6");
                string tmplName = !string.IsNullOrWhiteSpace(_instanceMetadata.TemplateName) ? _instanceMetadata.TemplateName : (_instanceMetadata.WorkflowName ?? "Template");
                this.Text = "Visual Flow — " + code;
                lblTitle.Text = "Visual Flow — " + code;
                lblSubtitle.Text = string.Format("{0} v{1} · มุมมอง Live", tmplName, _instanceMetadata.TemplateVersion);
                lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Monitor  ›  " + code + "  ›  Visual";
                
                // Update Step Badge
                int completedCount = _instanceMetadata.CompletedStepCount;
                int totalCount = _instanceMetadata.TotalStepCount;
                string statusText = "กำลังดำเนินการ";
                if (_instanceMetadata.Status == WorkflowInstanceStatus.Completed) statusText = "เสร็จสิ้น";
                else if (_instanceMetadata.Status == WorkflowInstanceStatus.Suspended) statusText = "ระงับชั่วคราว";
                else if (_instanceMetadata.Status == WorkflowInstanceStatus.Rejected) statusText = "ปฏิเสธ/ยกเลิก";

                lblStepBadge.Text = string.Format("● Step {0}/{1} · {2}", completedCount, totalCount, statusText);
                if (_instanceMetadata.Status == WorkflowInstanceStatus.Completed)
                {
                    lblStepBadge.ForeColor = Color.FromArgb(34, 153, 84);
                    lblStepBadge.BackColor = Color.FromArgb(220, 252, 231);
                }
                else
                {
                    lblStepBadge.ForeColor = Color.FromArgb(197, 120, 16);
                    lblStepBadge.BackColor = Color.FromArgb(254, 243, 199);
                }

                // 2. Load steps, connections and instances from DB using repositories
                var configManager = new CMS.Infrastructure.ConfigManager();
                var stepRepo = new CMS.Data.Repositories.WorkflowStepRepository(configManager);
                var stepInstanceRepo = new CMS.Data.Repositories.WorkflowStepInstanceRepository(configManager);

                _definitionSteps = stepRepo.GetStepsByTemplate(_instanceMetadata.WorkflowDefinitionId)?.ToList() ?? new List<WorkflowStep>();
                _definitionConnections = stepRepo.GetConnectionsByTemplate(_instanceMetadata.WorkflowDefinitionId)?.ToList() ?? new List<WorkflowConnection>();
                _stepInstances = stepInstanceRepo.GetStepInstancesByWorkflow(instanceId)?.ToList() ?? new List<WorkflowStepInstance>();

                // When s_WFStep is incomplete (designer saves to TemplateDefinition JSON, not step table),
                // load full step layout from the template JSON so all nodes are visible.
                TryLoadStepsFromTemplateJson(_instanceMetadata.WorkflowDefinitionId, configManager);

                // Select the current active step by default
                var currentActiveInstance = _stepInstances.FirstOrDefault(si => si.Status == WorkflowStepInstanceStatus.InProgress || si.Status == WorkflowStepInstanceStatus.Pending);
                if (currentActiveInstance != null)
                {
                    _selectedNode = _definitionSteps.FirstOrDefault(s => s.WorkflowStepId == currentActiveInstance.WorkflowStepId);
                }
                else if (_definitionSteps.Count > 0)
                {
                    _selectedNode = _definitionSteps.Last();
                }

                if (_nodeBounds.Count == 0)
                    CalculateAutoLayout();
                UpdateRightPanelWithSelectedNode();
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                _loggingService?.Error("Error loading workflow visual flow data: " + ex.Message, ex);
                MessageBox.Show("เกิดข้อผิดพลาดในการโหลดข้อมูล: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Show search/select modal when opened from sidebar without preselected ID
        private void ShowSelectInstanceDialog()
        {
            using (var dialog = new Form())
            {
                dialog.Text = "เลือกรายการ Workflow Instance";
                dialog.Size = new Size(500, 360);
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.Font = new Font("Segoe UI", 9F);
                dialog.BackColor = Color.White;

                var lblPrompt = new Label
                {
                    Text = "กรุณาเลือก Workflow Instance ที่ต้องการแสดงผล Visual Flow:",
                    Location = new Point(20, 20),
                    Size = new Size(460, 20),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };

                var lstInstances = new ListBox
                {
                    Location = new Point(20, 50),
                    Size = new Size(446, 200),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Fetch workflow instances
                List<WorkflowInstanceResult> instances = new List<WorkflowInstanceResult>();
                try
                {
                    var paged = _workflowService.GetWorkflowInstances(new WorkflowInstanceFilter { PageSize = 100 }, _userContext);
                    if (paged != null && paged.Items != null)
                    {
                        instances = paged.Items.ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ไม่สามารถโหลดรายการ Workflow ได้: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                foreach (var wfi in instances)
                {
                    string code = "WFI-" + wfi.WorkflowInstanceId.ToString("D6");
                    string name = string.IsNullOrEmpty(wfi.TemplateName) ? (wfi.WorkflowName ?? "Template") : wfi.TemplateName;
                    string status = wfi.Status.ToString();
                    lstInstances.Items.Add(string.Format("{0} - {1} ({2}) [{3}]", code, name, wfi.InitiatedBy, status));
                }

                var btnSelect = new Button
                {
                    Text = "เลือก",
                    DialogResult = DialogResult.OK,
                    Location = new Point(280, 275),
                    Size = new Size(90, 30),
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnSelect.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "ยกเลิก",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(376, 275),
                    Size = new Size(90, 30),
                    FlatStyle = FlatStyle.Flat
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(210, 215, 225);

                dialog.Controls.Add(lblPrompt);
                dialog.Controls.Add(lstInstances);
                dialog.Controls.Add(btnSelect);
                dialog.Controls.Add(btnCancel);

                dialog.AcceptButton = btnSelect;
                dialog.CancelButton = btnCancel;

                if (dialog.ShowDialog(this) == DialogResult.OK && lstInstances.SelectedIndex >= 0)
                {
                    var selectedIndex = lstInstances.SelectedIndex;
                    var selectedWfi = instances[selectedIndex];
                    LoadWorkflowData(selectedWfi.WorkflowInstanceId);
                }
                else
                {
                    if (_workflowInstanceId == 0)
                    {
                        this.BeginInvoke(new Action(() => this.Close()));
                    }
                }
            }
        }

        // Load step layout from TemplateDefinition JSON when s_WFStep is incomplete.
        // The designer saves only to TemplateDefinition JSON (not s_WFStep rows), so the step
        // table may have only the Start step. This method fills _definitionSteps/_definitionConnections
        // from JSON and populates _nodeBounds using the designer's canvas positions.
        private void TryLoadStepsFromTemplateJson(int workflowDefinitionId, IConfigManager configManager)
        {
            try
            {
                var templateRepo = new CMS.Data.Repositories.WorkflowTemplateRepository(configManager);
                var templateRecord = templateRepo.GetTemplate(workflowDefinitionId);
                if (templateRecord == null || string.IsNullOrWhiteSpace(templateRecord.TemplateDefinition))
                    return;

                var designerDef = JsonConvert.DeserializeObject<WorkflowDesignerTemplateDefinition>(templateRecord.TemplateDefinition);
                if (designerDef?.Steps == null || designerDef.Steps.Count <= _definitionSteps.Count)
                    return;

                // Replace step list with full set from designer JSON
                _definitionSteps = designerDef.Steps.Select(s => new WorkflowStep
                {
                    WorkflowStepId       = s.WorkflowStepId,
                    WorkflowDefinitionId = workflowDefinitionId,
                    StepName             = s.StepName,
                    StepType             = ParseDesignerStepType(s.StepType),
                    SequenceNumber       = s.WorkflowStepId,
                    SLADays              = s.SLADays > 0 ? (int?)s.SLADays : null,
                    AllowDelegation      = false,
                    IsDeleted            = false
                }).ToList();

                _definitionConnections = (designerDef.Connections ?? new List<WorkflowConnectionDesignData>())
                    .Select(c => new WorkflowConnection
                    {
                        WorkflowDefinitionId = workflowDefinitionId,
                        FromStepId           = c.FromStepId,
                        ToStepId             = c.ToStepId,
                        ConditionExpression  = c.Condition,
                        IsDeleted            = false
                    }).ToList();

                // Use designer canvas positions directly — no auto-layout needed
                _nodeBounds.Clear();
                foreach (var s in designerDef.Steps)
                    _nodeBounds[s.WorkflowStepId] = new Rectangle(s.CanvasX, s.CanvasY, 110, 44);
            }
            catch { /* fallback: use s_WFStep data and CalculateAutoLayout */ }
        }

        private static WorkflowStepType ParseDesignerStepType(string stepType)
        {
            switch ((stepType ?? "").ToLowerInvariant())
            {
                case "start":        return WorkflowStepType.Start;
                case "end":          return WorkflowStepType.End;
                case "decision":     return WorkflowStepType.Decision;
                case "parallel":     return WorkflowStepType.Parallel;
                case "notification": return WorkflowStepType.Notification;
                case "review":       return WorkflowStepType.Review;
                case "merge":        return WorkflowStepType.Merge;
                default:             return WorkflowStepType.Approval;
            }
        }

        // Auto-layout steps horizontally by sequence number and vertically centered when parallel
        private void CalculateAutoLayout()
        {
            _nodeBounds.Clear();
            if (_definitionSteps == null || _definitionSteps.Count == 0) return;

            // Group steps by their SequenceNumber
            var grouped = _definitionSteps
                .GroupBy(s => s.SequenceNumber)
                .OrderBy(g => g.Key)
                .ToList();

            int yMid = 220;
            int nodeW = 120;
            int nodeH = 46;
            int xSpacing = 160;
            int ySpacing = 30;

            for (int colIndex = 0; colIndex < grouped.Count; colIndex++)
            {
                var group = grouped[colIndex].ToList();
                int x = 60 + colIndex * xSpacing;
                int k = group.Count;

                int totalHeight = k * nodeH + (k - 1) * ySpacing;
                int startY = yMid - totalHeight / 2;

                for (int rowIndex = 0; rowIndex < k; rowIndex++)
                {
                    var step = group[rowIndex];
                    int y = startY + rowIndex * (nodeH + ySpacing);
                    _nodeBounds[step.WorkflowStepId] = new Rectangle(x, y, nodeW, nodeH);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Ribbon Tabs — wfe-form-ui.md §1
        // ════════════════════════════════════════════════════════════════════

        private void BuildRibbonTabs()
        {
            var tabs = new[]
            {
                new { Text = "แฟ้ม",       Active = false },
                new { Text = "หน้าแรก",    Active = false },
                new { Text = "ดำเนินการ",  Active = false },
                new { Text = "มุมมอง",     Active = true  },
                new { Text = "เครื่องมือ", Active = false },
                new { Text = "ช่วยเหลือ",  Active = false },
            };
            int x = 8;
            foreach (var t in tabs)
            {
                var btn = new Button
                {
                    Text      = t.Text,
                    Location  = new Point(x, 0),
                    Size      = new Size(MeasureTabWidth(t.Text) + 24, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = t.Active ? Color.White : Color.FromArgb(190, 210, 245),
                    Font      = t.Active
                        ? new Font("Segoe UI", 9.5F, FontStyle.Bold)
                        : new Font("Segoe UI", 9.5F),
                    Cursor  = Cursors.Hand,
                    TabStop = false,
                };
                btn.FlatAppearance.BorderSize           = 0;
                btn.FlatAppearance.MouseOverBackColor   = Color.FromArgb(50, 80, 130);
                btn.FlatAppearance.MouseDownBackColor   = Color.FromArgb(40, 70, 120);
                if (t.Active)
                    btn.Paint += (s, pe) =>
                        pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
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

        // ════════════════════════════════════════════════════════════════════
        // Toolbar — wfe-form-ui.md §2
        // ════════════════════════════════════════════════════════════════════

        private void BuildToolbar()
        {
            var groups = new[]
            {
                new { Header = "นำทาง", Items = new[]
                {
                    new { Icon = "◀",  Text = "ย้อนกลับ", Tag = "back"   },
                }},
                new { Header = "มุมมอง", Items = new[]
                {
                    new { Icon = "👁",  Text = "Visual",   Tag = "visual"   },
                    new { Icon = "📋",  Text = "Timeline", Tag = "timeline" },
                    new { Icon = "📜",  Text = "Audit",    Tag = "audit"    },
                }},
                new { Header = "การจัดการ", Items = new[]
                {
                    new { Icon = "⇄",  Text = "Reassign", Tag = "reassign" },
                    new { Icon = "✖",  Text = "ยกเลิก",   Tag = "cancel"   },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int gx = x;
                foreach (var item in group.Items)
                {
                    var tag = item.Tag;
                    var pnlBtn = new Panel
                    {
                        Location  = new Point(x, 2),
                        Size      = new Size(58, 60),
                        BackColor = Color.White,
                        Cursor    = Cursors.Hand,
                        Tag       = tag,
                    };
                    var lblIcon = new Label
                    {
                        Text      = item.Icon,
                        Font      = new Font("Segoe UI", 16F),
                        ForeColor = _navyDark,
                        Location  = new Point(0, 6),
                        Size      = new Size(58, 28),
                        TextAlign = ContentAlignment.MiddleCenter,
                    };
                    var lblText = new Label
                    {
                        Text      = item.Text,
                        Font      = new Font("Segoe UI", 7.5F),
                        ForeColor = Color.FromArgb(55, 65, 90),
                        Location  = new Point(0, 34),
                        Size      = new Size(58, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                    };
                    pnlBtn.Controls.Add(lblIcon);
                    pnlBtn.Controls.Add(lblText);

                    EventHandler enter = (s, ev) => { pnlBtn.BackColor = lblIcon.BackColor = lblText.BackColor = _navHover; };
                    EventHandler leave = (s, ev) => { pnlBtn.BackColor = lblIcon.BackColor = lblText.BackColor = Color.White; };
                    EventHandler click = (s, ev) => OnToolbarClick(tag);
                    foreach (Control c in new Control[] { pnlBtn, lblIcon, lblText })
                    { c.MouseEnter += enter; c.MouseLeave += leave; c.Click += click; }

                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 60;
                }

                pnlToolbarContainer.Controls.Add(new Label
                {
                    Text      = group.Header,
                    Font      = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point(gx, 62),
                    Size      = new Size(x - gx, 10),
                });

                if (group != groups[groups.Length - 1])
                {
                    pnlToolbarContainer.Controls.Add(new Panel
                    {
                        BackColor = Color.FromArgb(220, 225, 235),
                        Location  = new Point(x + 2, 8),
                        Size      = new Size(1, 54),
                    });
                    x += 10;
                }
            }

            pnlToolbarContainer.Controls.Add(new Panel
                { BackColor = Color.FromArgb(220, 225, 235), Dock = DockStyle.Bottom, Height = 1 });
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "back":
                    WorkflowNavigationHandler.HandleNavigation("detail", this, _workflowService, _templateService, null, _loggingService, _userContext, null, _workflowInstanceId);
                    break;
                case "visual":   
                    break;
                case "timeline": 
                    ShowInfo("Timeline view: ยังไม่ได้ Implement"); 
                    break;
                case "audit":    
                    ShowInfo("Audit view: ยังไม่ได้ Implement");    
                    break;
                case "reassign": 
                    if (_stepInstances != null)
                    {
                        var activeStep = _stepInstances.FirstOrDefault(si => si.Status == WorkflowStepInstanceStatus.InProgress || si.Status == WorkflowStepInstanceStatus.Pending);
                        if (activeStep != null)
                        {
                            ReassignCurrentStep(activeStep);
                        }
                        else
                        {
                            MessageBox.Show("ไม่พบ Step ที่กำลังดำเนินการสำหรับ Reassign", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    break;
                case "cancel":   
                    CancelWorkflowInstance(); 
                    break;
            }
        }

        private void CancelWorkflowInstance()
        {
            if (_workflowInstanceId <= 0 || _instanceMetadata == null) return;

            if (MessageBox.Show("คุณต้องการระงับ/ยกเลิก Workflow Instance นี้ใช่หรือไม่?", "ยืนยันการระงับขั้นตอน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _workflowService.SuspendWorkflowInstance(_workflowInstanceId, _userContext);
                    MessageBox.Show("ระงับ Workflow Instance เรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadWorkflowData(_workflowInstanceId);
                }
                catch (Exception ex)
                {
                    _loggingService?.Error("Failed to suspend workflow instance", ex);
                    MessageBox.Show("ระงับไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Status bar + Legend bar
        // ════════════════════════════════════════════════════════════════════

        private void BuildStatusBar()
        {
            pnlStatusBar.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(210, 215, 225)), 0, 0, pnlStatusBar.Width, 0);

            var userName = _userContext != null
                ? (_userContext.ADUserName ?? _userContext.AdIdentity ?? "Guest")
                : "Guest";

            var items = new[]
            {
                "  พร้อมใช้งาน  ",
                "  เชื่อมต่อ: CMS-DB01  ",
                string.Format("  โดย: {0}  ", userName),
                "  Zoom 100%  ",
                "  TH  ",
                "  ออนไลน์  ",
            };
            int x = 8;
            foreach (var text in items)
            {
                var lbl = new Label
                {
                    Text      = text,
                    Font      = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(80, 90, 110),
                    AutoSize  = true,
                    Location  = new Point(x, 4),
                };
                pnlStatusBar.Controls.Add(lbl);
                x += lbl.PreferredWidth + 4;
            }
        }

        private void BuildLegendBar()
        {
            pnlLegendBar.BackColor = Color.White;
            pnlLegendBar.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(220, 225, 235)), 0, pnlLegendBar.Height - 1, pnlLegendBar.Width, pnlLegendBar.Height - 1);

            var legends = new[]
            {
                new { Color = Color.FromArgb(34,  153, 84),  Label = "เสร็จสิ้น" },
                new { Color = Color.FromArgb(230, 115, 0),   Label = "กำลังทำ"  },
                new { Color = Color.FromArgb(107, 114, 128), Label = "รอ"       },
                new { Color = Color.FromArgb(156, 163, 175), Label = "ข้าม/โอน" },
            };

            int x = 12;
            foreach (var leg in legends)
            {
                var dot = new Panel
                {
                    Location  = new Point(x, 10),
                    Size      = new Size(12, 12),
                    BackColor = leg.Color,
                };
                var lbl = new Label
                {
                    Text      = leg.Label,
                    Font      = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(55, 65, 90),
                    Location  = new Point(x + 16, 8),
                    AutoSize  = true,
                };
                pnlLegendBar.Controls.Add(dot);
                pnlLegendBar.Controls.Add(lbl);
                x += lbl.PreferredWidth + 36;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Right panel — Step info + Actions
        // ════════════════════════════════════════════════════════════════════

        private void BuildRightPanel()
        {
            UpdateRightPanelWithSelectedNode();
        }

        private void UpdateRightPanelWithSelectedNode()
        {
            pnlRightScroll.Controls.Clear();
            if (_selectedNode == null)
            {
                var lblEmpty = new Label
                {
                    Text = "กรุณาเลือกโหนดเพื่อดูรายละเอียด",
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = Color.FromArgb(100, 110, 130),
                    Location = new Point(12, 20),
                    AutoSize = true
                };
                pnlRightScroll.Controls.Add(lblEmpty);
                return;
            }

            if (_selectedNode is WorkflowStep step)
            {
                int y = 12;

                // Find corresponding step instance
                var stepInstance = _stepInstances.FirstOrDefault(si => si.WorkflowStepId == step.WorkflowStepId);
                string statusText = "Pending";
                Color statusBg = Color.FromArgb(243, 244, 246);
                Color statusFg = Color.FromArgb(107, 114, 128);

                if (stepInstance != null)
                {
                    switch (stepInstance.Status)
                    {
                        case WorkflowStepInstanceStatus.Completed:
                            statusText = "Completed";
                            statusBg = Color.FromArgb(220, 252, 231);
                            statusFg = Color.FromArgb(34, 153, 84);
                            break;
                        case WorkflowStepInstanceStatus.InProgress:
                        case WorkflowStepInstanceStatus.Pending:
                            statusText = "Active";
                            statusBg = Color.FromArgb(254, 243, 199);
                            statusFg = Color.FromArgb(230, 115, 0);
                            break;
                        case WorkflowStepInstanceStatus.Rejected:
                            statusText = "Rejected";
                            statusBg = Color.FromArgb(254, 226, 226);
                            statusFg = Color.FromArgb(185, 40, 40);
                            break;
                        case WorkflowStepInstanceStatus.Delegated:
                            statusText = "Delegated";
                            statusBg = Color.FromArgb(243, 244, 246);
                            statusFg = Color.FromArgb(156, 163, 175);
                            break;
                    }
                }

                // Step header
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = "รายละเอียด Step", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 36, 55), Location = new Point(12, y), AutoSize = true,
                });
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = "● " + statusText, Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = statusFg, BackColor = statusBg,
                    Location = new Point(160, y + 2), AutoSize = true, Padding = new Padding(4, 2, 4, 2),
                });
                y += 26;

                pnlRightScroll.Controls.Add(new Label
                {
                    Text = step.StepName, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(22, 36, 62), Location = new Point(12, y), AutoSize = true,
                });
                y += 22;

                // Find step index
                int stepIdx = _definitionSteps.IndexOf(step) + 1;
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = string.Format("Step {0} จาก {1}", stepIdx, _definitionSteps.Count), Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(110, 120, 140), Location = new Point(12, y), AutoSize = true,
                });
                y += 28;

                AddSeparator(y); y += 12;

                // Assignee avatar & info
                string assigneeName = step.AssignedUser ?? step.AssignedRole ?? "Unassigned";
                string assigneeRoleText = !string.IsNullOrEmpty(step.AssignedRole) ? step.AssignedRole : "User Assignment";
                if (stepInstance != null && !string.IsNullOrEmpty(stepInstance.AssignedTo))
                {
                    assigneeName = stepInstance.AssignedTo;
                }

                var avatarPanel = new Panel { Location = new Point(12, y), Size = new Size(36, 36), BackColor = Color.Transparent };
                string initialChar = assigneeName.Length > 0 ? assigneeName.Substring(0, 1) : "?";
                avatarPanel.Paint += (s, pe) =>
                {
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var br = new SolidBrush(Color.FromArgb(59, 91, 219)))
                        pe.Graphics.FillEllipse(br, 0, 0, 35, 35);
                    using (var f = new Font("Segoe UI", 12F, FontStyle.Bold))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        pe.Graphics.DrawString(initialChar, f, Brushes.White, new RectangleF(0, 0, 35, 35), sf);
                };
                pnlRightScroll.Controls.Add(avatarPanel);

                pnlRightScroll.Controls.Add(new Label
                {
                    Text = assigneeName, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(22, 36, 62), Location = new Point(56, y + 4), AutoSize = true,
                });
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = assigneeRoleText, Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(110, 120, 140), Location = new Point(56, y + 22), AutoSize = true,
                });
                y += 52;

                // SLA and Deadline details
                string deadlineStr = "ไม่มีกำหนด SLA";
                string remainingStr = "-";
                int slaPct = 0;
                Color slaBarColor = Color.FromArgb(34, 153, 84);

                if (stepInstance != null)
                {
                    deadlineStr = stepInstance.SLADeadline.ToString("dd/MM/yyyy HH:mm");
                    var totalSlaWindow = stepInstance.SLADeadline - stepInstance.AssignedDate;
                    var timeUsed = DateTime.Now - stepInstance.AssignedDate;
                    if (stepInstance.Status == WorkflowStepInstanceStatus.Completed && stepInstance.CompletedDate.HasValue)
                    {
                        timeUsed = stepInstance.CompletedDate.Value - stepInstance.AssignedDate;
                        remainingStr = "เสร็จสิ้น";
                    }
                    else
                    {
                        var diff = stepInstance.SLADeadline - DateTime.Now;
                        remainingStr = diff.TotalMinutes < 0 ? "เกินกำหนด" : FormatTimeSpan(diff);
                    }

                    if (totalSlaWindow.TotalSeconds > 0)
                    {
                        slaPct = (int)(timeUsed.TotalSeconds / totalSlaWindow.TotalSeconds * 100);
                        if (slaPct > 100) slaPct = 100;
                        if (slaPct < 0) slaPct = 0;
                    }
                    else
                    {
                        slaPct = 100;
                    }

                    if (slaPct > 90) slaBarColor = Color.FromArgb(185, 40, 40); // Red
                    else if (slaPct > 60) slaBarColor = Color.FromArgb(197, 120, 16); // Orange
                }

                AddInfoRow("กำหนด", deadlineStr, y, false); y += 22;
                
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = "เหลือ", Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(110, 120, 140), Location = new Point(12, y), AutoSize = true,
                });
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = "● " + remainingStr, Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = slaBarColor, BackColor = Color.FromArgb(243, 244, 246),
                    Location = new Point(70, y - 2), AutoSize = true, Padding = new Padding(4, 2, 4, 2),
                });
                y += 28;

                // SLA bar
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = "SLA Used", Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(110, 120, 140), Location = new Point(12, y), AutoSize = true,
                });
                var barBg = new Panel { Location = new Point(12, y + 18), Size = new Size(180, 8), BackColor = Color.FromArgb(228, 232, 240) };
                barBg.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size((int)(180 * slaPct / 100.0), 8), BackColor = slaBarColor });
                pnlRightScroll.Controls.Add(barBg);
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = slaPct + "%", Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = slaBarColor, Location = new Point(198, y + 14), AutoSize = true,
                });
                y += 40;

                AddSeparator(y); y += 12;

                // PATH
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = "PATH ที่วิ่ง", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 36, 55), Location = new Point(12, y), AutoSize = true,
                });
                y += 20;

                List<string> pathNames = new List<string>();
                foreach (var stepInst in _stepInstances.OrderBy(si => si.AssignedDate))
                {
                    var matchingStep = _definitionSteps.FirstOrDefault(s => s.WorkflowStepId == stepInst.WorkflowStepId);
                    if (matchingStep != null)
                    {
                        pathNames.Add(matchingStep.StepName);
                    }
                }
                if (pathNames.Count == 0)
                {
                    pathNames.Add("เริ่ม");
                }
                string pathText = string.Join(" → ", pathNames);

                var lblPath = new Label
                {
                    Text = pathText,
                    Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(55, 65, 90),
                    Location = new Point(12, y), Size = new Size(236, 52),
                };
                pnlRightScroll.Controls.Add(lblPath);
                y += 60;

                AddSeparator(y); y += 12;

                // ACTIONS
                pnlRightScroll.Controls.Add(new Label
                {
                    Text = "ACTIONS", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 36, 55), Location = new Point(12, y), AutoSize = true,
                });
                y += 22;

                bool canAction = stepInstance != null && 
                                 (stepInstance.Status == WorkflowStepInstanceStatus.InProgress || 
                                  stepInstance.Status == WorkflowStepInstanceStatus.Pending);

                var btnExecute = new Button
                {
                    Text = "  ✓  ดำเนินการ Step นี้",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    BackColor = canAction ? Color.FromArgb(37, 99, 235) : Color.FromArgb(156, 163, 175),
                    Enabled = canAction,
                    Location = new Point(12, y), Size = new Size(236, 32),
                };
                btnExecute.FlatAppearance.BorderSize = 0;
                btnExecute.Click += (s, ev) => ExecuteCurrentStep(stepInstance);
                pnlRightScroll.Controls.Add(btnExecute);
                y += 38;

                var btnReassign = new Button
                {
                    Text = "  ⇄  Reassign",
                    Font = new Font("Segoe UI", 9F), 
                    ForeColor = canAction ? Color.FromArgb(44, 62, 100) : Color.FromArgb(156, 163, 175),
                    FlatStyle = FlatStyle.Flat, 
                    BackColor = Color.White,
                    Enabled = canAction,
                    Location = new Point(12, y), Size = new Size(236, 30),
                };
                btnReassign.FlatAppearance.BorderColor = Color.FromArgb(210, 215, 225);
                btnReassign.Click += (s, ev) => ReassignCurrentStep(stepInstance);
                pnlRightScroll.Controls.Add(btnReassign);
                y += 36;

                var btnComment = new Button
                {
                    Text = "  💬  แสดงความเห็น",
                    Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(44, 62, 100),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                    Location = new Point(12, y), Size = new Size(236, 30),
                };
                btnComment.FlatAppearance.BorderColor = Color.FromArgb(210, 215, 225);
                btnComment.Click += (s, ev) => ShowInfo("ความคิดเห็น: เพิ่มสำเร็จ (Mock)");
                pnlRightScroll.Controls.Add(btnComment);
            }
        }

        private void ExecuteCurrentStep(WorkflowStepInstance stepInstance)
        {
            if (stepInstance == null) return;
            
            var stepDef = _definitionSteps.FirstOrDefault(s => s.WorkflowStepId == stepInstance.WorkflowStepId);
            var task = new WorkflowTaskResult
            {
                WorkflowStepInstanceId = stepInstance.WorkflowStepInstanceId,
                WorkflowInstanceId = stepInstance.WorkflowInstanceId,
                WorkflowName = _instanceMetadata?.WorkflowName ?? "ไม่ระบุ",
                StepName = stepDef?.StepName ?? "ไม่ระบุ",
                StepDescription = stepDef?.Description,
                Status = stepInstance.Status,
                AssignedDate = stepInstance.AssignedDate,
                CompletedDate = stepInstance.CompletedDate,
                SLADeadline = stepInstance.SLADeadline,
                SLAStatus = stepInstance.SLAStatus,
                CanDelegate = stepDef?.AllowDelegation ?? false,
                MaxDelegationDepth = stepDef?.MaxDelegationDepth ?? 0,
                TimeRemainingMinutes = (int)(stepInstance.SLADeadline - DateTime.Now).TotalMinutes,
                InitiatedBy = _instanceMetadata?.InitiatedBy
            };

            using (var actionForm = new WorkflowStepActionForm(task))
            {
                actionForm.AllowForward = task.CanDelegate;
                actionForm.DueDate = task.SLADeadline;
                actionForm.InitiatorName = _instanceMetadata?.InitiatedBy;

                if (actionForm.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        string action = actionForm.SelectedAction;
                        string comments = actionForm.Comments;

                        if (string.Equals(action, "Reject", StringComparison.OrdinalIgnoreCase))
                        {
                            _workflowService.RejectWorkflowStep(new RejectWorkflowStepRequest
                            {
                                WorkflowInstanceId = task.WorkflowInstanceId,
                                WorkflowStepInstanceId = task.WorkflowStepInstanceId,
                                RejectionReason = comments,
                                Remarks = comments
                            }, _userContext);
                        }
                        else if (string.Equals(action, "Forward", StringComparison.OrdinalIgnoreCase))
                        {
                            ReassignCurrentStep(stepInstance);
                            return;
                        }
                        else
                        {
                            _workflowService.CompleteWorkflowStep(new CompleteWorkflowStepRequest
                            {
                                WorkflowInstanceId = task.WorkflowInstanceId,
                                WorkflowStepInstanceId = task.WorkflowStepInstanceId,
                                OutputData = null,
                                Remarks = comments
                            }, _userContext);
                        }

                        MessageBox.Show("ดำเนินการเสร็จสิ้น", "Workflow", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadWorkflowData(_workflowInstanceId);
                    }
                    catch (Exception ex)
                    {
                        _loggingService?.Error("Failed to execute workflow task from visual flow", ex);
                        MessageBox.Show("ดำเนินการไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ReassignCurrentStep(WorkflowStepInstance stepInstance)
        {
            if (stepInstance == null) return;

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

            var stepDef = _definitionSteps.FirstOrDefault(s => s.WorkflowStepId == stepInstance.WorkflowStepId);
            var task = new WorkflowTaskResult
            {
                WorkflowInstanceId = stepInstance.WorkflowInstanceId,
                WorkflowStepInstanceId = stepInstance.WorkflowStepInstanceId,
                WorkflowName = _instanceMetadata?.WorkflowName ?? "ไม่ระบุ",
                StepName = stepDef?.StepName ?? "ไม่ระบุ",
                StepDescription = stepDef?.Description,
                Status = stepInstance.Status,
                AssignedDate = stepInstance.AssignedDate,
                SLADeadline = stepInstance.SLADeadline,
                SLAStatus = stepInstance.SLAStatus,
                InitiatedBy = _instanceMetadata?.InitiatedBy,
                CanDelegate = true
            };

            using (var dialog = new ReassignDialog(task))
            {
                dialog.CurrentAssignee = new ReassignDialog.AssigneeInfo
                {
                    Name = stepInstance.AssignedTo ?? "ผู้รับผิดชอบ",
                    Role = "Officer"
                };

                dialog.Candidates = BuildReassignCandidates(stepInstance.AssignedTo);
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
                            WorkflowStepInstanceId = stepInstance.WorkflowStepInstanceId,
                            DelegateToUserName = delegateTo,
                            DelegationReason = dialog.ReassignReason,
                            Remarks = dialog.MessageToNew
                        }, _userContext);

                        MessageBox.Show("Reassign งานสำเร็จ", "Workflow", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadWorkflowData(_workflowInstanceId);
                    }
                    catch (Exception ex)
                    {
                        _loggingService?.Error("Failed to reassign task from visual flow", ex);
                        MessageBox.Show("Reassign งานไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private List<ReassignDialog.CandidateInfo> BuildReassignCandidates(string currentAssignee)
        {
            var candidates = _stepInstances
                .Select(s => s.AssignedTo)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(u => !string.Equals(u, currentAssignee, StringComparison.OrdinalIgnoreCase))
                .Select(u => new ReassignDialog.CandidateInfo
                {
                    UserId = u,
                    Name = u,
                    Role = "Staff/Officer",
                    MatchScore = 85
                })
                .ToList();

            if (candidates.Count == 0)
            {
                candidates.Add(new ReassignDialog.CandidateInfo { UserId = "สมชาย", Name = "สมชาย กาญจนา", Role = "Staff", MatchScore = 90 });
                candidates.Add(new ReassignDialog.CandidateInfo { UserId = "วิชัย", Name = "วิชัย มีสุข", Role = "Manager", MatchScore = 75 });
            }
            return candidates;
        }

        private void AddSeparator(int y)
        {
            pnlRightScroll.Controls.Add(new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(260, 1),
                BackColor = Color.FromArgb(220, 225, 235),
            });
        }

        private void AddInfoRow(string label, string value, int y, bool valueBold)
        {
            pnlRightScroll.Controls.Add(new Label
            {
                Text = label, Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(110, 120, 140), Location = new Point(12, y), AutoSize = true,
            });
            pnlRightScroll.Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 8.5F, valueBold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = Color.FromArgb(22, 36, 62), Location = new Point(70, y), AutoSize = true,
            });
        }

        // ════════════════════════════════════════════════════════════════════
        // Canvas — GDI+ Visual Flow
        // ════════════════════════════════════════════════════════════════════

        private void WireCanvasEvents()
        {
            pnlCanvas.Paint  += PnlCanvas_Paint;
            pnlCanvas.MouseClick += PnlCanvas_MouseClick;
            btnZoomIn.Click  += (s, e) => { _zoomPercent = Math.Min(200, _zoomPercent + 10); lblZoomLevel.Text = _zoomPercent + "%"; pnlCanvas.Invalidate(); };
            btnZoomOut.Click += (s, e) => { _zoomPercent = Math.Max(25,  _zoomPercent - 10); lblZoomLevel.Text = _zoomPercent + "%"; pnlCanvas.Invalidate(); };
            btnExportPng.Click += (s, e) => ExportCanvasToPng();
        }

        private void PnlCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            float scale = _zoomPercent / 100f;
            float clickX = e.X / scale;
            float clickY = e.Y / scale;

            object clickedNode = null;
            foreach (var kvp in _nodeBounds)
            {
                if (kvp.Value.Contains((int)clickX, (int)clickY))
                {
                    var stepId = kvp.Key;
                    var step = _definitionSteps.FirstOrDefault(s => s.WorkflowStepId == stepId);
                    if (step != null)
                    {
                        clickedNode = step;
                        break;
                    }
                }
            }

            if (clickedNode != null)
            {
                _selectedNode = clickedNode;
                UpdateRightPanelWithSelectedNode();
                pnlCanvas.Invalidate();
            }
        }

        private void PnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            float scale = _zoomPercent / 100f;
            g.ScaleTransform(scale, scale);
            DrawGrid(g);
            DrawLiveFlow(g);
        }

        private void DrawGrid(Graphics g)
        {
            int w = (int)(pnlCanvas.Width  / (_zoomPercent / 100f));
            int h = (int)(pnlCanvas.Height / (_zoomPercent / 100f));
            using (var br = new SolidBrush(Color.FromArgb(40, 160, 170, 190)))
                for (int x = 0; x < w; x += 20)
                    for (int y = 0; y < h; y += 20)
                        g.FillEllipse(br, x - 1, y - 1, 2, 2);
        }

        private void DrawLiveFlow(Graphics g)
        {
            if (_definitionSteps == null || _definitionSteps.Count == 0)
            {
                DrawSampleFlow(g);
                return;
            }

            // Draw Connections
            foreach (var conn in _definitionConnections)
            {
                if (conn.IsDeleted) continue;
                if (_nodeBounds.TryGetValue(conn.FromStepId, out Rectangle rectFrom) &&
                    _nodeBounds.TryGetValue(conn.ToStepId, out Rectangle rectTo))
                {
                    int x1 = rectFrom.Right;
                    int y1 = rectFrom.Top + rectFrom.Height / 2;
                    int x2 = rectTo.Left;
                    int y2 = rectTo.Top + rectTo.Height / 2;

                    DrawArrow(g, x1, y1, x2, y2, conn.ConditionExpression);
                }
            }

            // Draw Nodes
            foreach (var step in _definitionSteps)
            {
                if (step.IsDeleted) continue;
                if (_nodeBounds.TryGetValue(step.WorkflowStepId, out Rectangle rect))
                {
                    Color nodeColor = Color.FromArgb(107, 114, 128); // default Pending (Gray)
                    string subText = "";
                    bool isActive = false;

                    var stepInstance = _stepInstances.FirstOrDefault(si => si.WorkflowStepId == step.WorkflowStepId);
                    if (stepInstance != null)
                    {
                        if (stepInstance.Status == WorkflowStepInstanceStatus.Completed)
                        {
                            nodeColor = Color.FromArgb(34, 153, 84); // Green
                            subText = string.Format("{0} · {1:dd/MM HH:mm}", 
                                string.IsNullOrEmpty(stepInstance.CompletedBy) ? stepInstance.AssignedTo : stepInstance.CompletedBy, 
                                stepInstance.CompletedDate);
                        }
                        else if (stepInstance.Status == WorkflowStepInstanceStatus.InProgress || 
                                 stepInstance.Status == WorkflowStepInstanceStatus.Pending)
                        {
                            nodeColor = Color.FromArgb(230, 115, 0); // Orange
                            isActive = true;
                            var diff = stepInstance.SLADeadline - DateTime.Now;
                            if (diff.TotalMinutes < 0)
                            {
                                subText = "Overdue " + FormatTimeSpan(diff.Duration());
                                nodeColor = Color.FromArgb(185, 40, 40); // Red if overdue
                            }
                            else
                            {
                                subText = FormatTimeSpan(diff) + " remaining";
                            }
                        }
                        else if (stepInstance.Status == WorkflowStepInstanceStatus.Rejected)
                        {
                            nodeColor = Color.FromArgb(185, 40, 40); // Red
                            subText = "Rejected";
                        }
                        else if (stepInstance.Status == WorkflowStepInstanceStatus.Delegated)
                        {
                            nodeColor = Color.FromArgb(156, 163, 175); // Light Gray
                            subText = "Delegated";
                        }
                    }

                    bool isSelected = false;
                    if (_selectedNode is WorkflowStep sNode && sNode.WorkflowStepId == step.WorkflowStepId)
                    {
                        isSelected = true;
                    }

                    if (step.StepType == WorkflowStepType.Decision)
                        DrawDiamondNode(g, rect, step.StepName, nodeColor, isSelected || isActive);
                    else
                        DrawFlowNode(g, rect.X, rect.Y, rect.Width, rect.Height, step.StepName, subText, nodeColor, isSelected || isActive);
                }
            }
        }

        private static void DrawDiamondNode(Graphics g, Rectangle rect, string label, Color color, bool isActive)
        {
            int cx = rect.X + rect.Width / 2;
            int cy = rect.Y + rect.Height / 2;
            int hw = rect.Width / 2;
            int hh = rect.Height / 2;
            var pts = new[]
            {
                new Point(cx,      cy - hh),
                new Point(cx + hw, cy),
                new Point(cx,      cy + hh),
                new Point(cx - hw, cy),
            };
            using (var br = new SolidBrush(Color.White))
                g.FillPolygon(br, pts);
            using (var pen = new Pen(isActive ? Color.FromArgb(230, 115, 0) : color, isActive ? 2f : 1.5f))
                g.DrawPolygon(pen, pts);
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var f  = new Font("Segoe UI", 7.5F, FontStyle.Bold))
            using (var textBr = new SolidBrush(color))
                g.DrawString(label, f, textBr, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), sf);
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalDays >= 1)
                return string.Format("{0:0} วัน", ts.TotalDays);
            if (ts.TotalHours >= 1)
                return string.Format("{0:0} ชม.", ts.TotalHours);
            return string.Format("{0:0} นาที", ts.TotalMinutes);
        }

        private void ExportCanvasToPng()
        {
            if (_definitionSteps == null || _definitionSteps.Count == 0)
            {
                MessageBox.Show("ไม่มีข้อมูลสำหรับการ Export", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image (*.png)|*.png";
                sfd.FileName = string.Format("workflow-visualflow-WFI-{0:D6}.png", _workflowInstanceId);
                sfd.Title = "บันทึกรูปภาพ Visual Flow";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        int maxX = 800;
                        int maxY = 400;
                        foreach (var rect in _nodeBounds.Values)
                        {
                            if (rect.Right + 100 > maxX) maxX = rect.Right + 100;
                            if (rect.Bottom + 100 > maxY) maxY = rect.Bottom + 100;
                        }

                        using (var bmp = new Bitmap(maxX, maxY))
                        {
                            using (var g = Graphics.FromImage(bmp))
                            {
                                g.Clear(Color.FromArgb(240, 243, 248));
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                                using (var br = new SolidBrush(Color.FromArgb(40, 160, 170, 190)))
                                    for (int x = 0; x < maxX; x += 20)
                                        for (int y = 0; y < maxY; y += 20)
                                            g.FillEllipse(br, x - 1, y - 1, 2, 2);

                                DrawLiveFlow(g);
                            }
                            bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        MessageBox.Show("บันทึกรูปภาพสำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        _loggingService?.Error("Failed to export PNG of canvas", ex);
                        MessageBox.Show("ส่งออกรูปภาพไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DrawSampleFlow(Graphics g)
        {
            DrawFlowNode(g, 60,  200, 80,  44, "เริ่ม",           "",                    Color.FromArgb(34, 153, 84),  false);
            DrawFlowNode(g, 200, 200, 110, 44, "ส่งคำขอ",         "สมชาย · 15/04 09:30", Color.FromArgb(34, 153, 84),  false);
            DrawFlowNode(g, 370, 200, 80,  44, "CRAFT",            "3NC",                 Color.FromArgb(34, 153, 84),  false);
            DrawFlowNode(g, 510, 160, 120, 44, "Manager อนุมัติ",  "วิชัย · 1 วัน",       Color.FromArgb(230, 115, 0),  true);
            DrawFlowNode(g, 510, 240, 120, 44, "Peer Review",      "Parallel · Required", Color.FromArgb(107, 114, 128),false);
            DrawFlowNode(g, 700, 200, 80,  44, "แจ้งผล",           "",                    Color.FromArgb(107, 114, 128),false);
            DrawFlowNode(g, 840, 200, 80,  44, "สิ้นสุด",          "",                    Color.FromArgb(185, 40, 40),  false);

            DrawArrow(g, 140, 222, 200, 222, null);
            DrawArrow(g, 310, 222, 370, 222, null);
            DrawArrow(g, 450, 222, 510, 182, "3NC");
            DrawArrow(g, 450, 222, 510, 262, "กลับ");
            DrawArrow(g, 630, 182, 700, 222, null);
            DrawArrow(g, 780, 222, 840, 222, null);
        }

        private void DrawFlowNode(Graphics g, int x, int y, int w, int h,
            string label, string sub, Color color, bool isActive)
        {
            var rect = new Rectangle(x, y, w, h);

            using (var shadow = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
                g.FillPath(shadow, RoundedRect(new Rectangle(x + 3, y + 3, w, h), 8));

            using (var br = new SolidBrush(isActive ? Color.FromArgb(255, 247, 235) : Color.White))
                g.FillPath(br, RoundedRect(rect, 8));

            using (var abr = new SolidBrush(color))
            {
                var ap = new GraphicsPath();
                ap.AddArc(x, y, 16, 16, 180, 90);
                ap.AddArc(x + w - 16, y, 16, 16, 270, 90);
                ap.AddLine(x + w, y + 5, x, y + 5);
                ap.CloseFigure();
                g.FillPath(abr, ap);
            }

            using (var pen = new Pen(isActive ? Color.FromArgb(230, 115, 0) : Color.FromArgb(210, 215, 225), isActive ? 2f : 1f))
                g.DrawPath(pen, RoundedRect(rect, 8));

            if (isActive)
            {
                using (var glowPen = new Pen(Color.FromArgb(80, 230, 115, 0), 4f))
                    g.DrawPath(glowPen, RoundedRect(new Rectangle(x - 2, y - 2, w + 4, h + 4), 10));
            }

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            using (var f  = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var br = new SolidBrush(Color.FromArgb(30, 40, 65)))
                g.DrawString(label, f, br, new RectangleF(x + 4, y + 6, w - 8, h - 14), sf);

            if (!string.IsNullOrEmpty(sub))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                using (var f  = new Font("Segoe UI", 7F))
                using (var br = new SolidBrush(Color.FromArgb(140, 150, 165)))
                    g.DrawString(sub, f, br, new RectangleF(x + 2, y + h - 14, w - 4, 13), sf);
        }

        private static void DrawArrow(Graphics g, int x1, int y1, int x2, int y2, string label)
        {
            using (var pen = new Pen(Color.FromArgb(160, 170, 190), 1.5f) { EndCap = LineCap.ArrowAnchor })
            {
                int cx = (x1 + x2) / 2;
                g.DrawBezier(pen, x1, y1, cx, y1, cx, y2, x2, y2);
            }
            if (!string.IsNullOrEmpty(label))
            {
                int mx = (x1 + x2) / 2, my = (y1 + y2) / 2 - 10;
                using (var f  = new Font("Segoe UI", 7F))
                using (var br = new SolidBrush(Color.FromArgb(100, 110, 130)))
                    g.DrawString(label, f, br, new PointF(mx, my));
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private void OnNavItemClick(string tag)
        {
            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                _workflowService,
                _templateService,
                null, // slaCalculator
                _loggingService,
                _userContext,
                null,
                _workflowInstanceId);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (_workflowInstanceId > 0)
            {
                WorkflowNavigationHandler.HandleNavigation(
                    "detail",
                    this,
                    _workflowService,
                    _templateService,
                    null,
                    _loggingService,
                    _userContext,
                    null,
                    _workflowInstanceId);
            }
            else
            {
                WorkflowNavigationHandler.HandleNavigation(
                    "monitor",
                    this,
                    _workflowService,
                    _templateService,
                    null,
                    _loggingService,
                    _userContext);
            }
        }

        private void ShowInfo(string message)
            => MessageBox.Show(message, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X,          r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d,  r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d,  r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,          r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
