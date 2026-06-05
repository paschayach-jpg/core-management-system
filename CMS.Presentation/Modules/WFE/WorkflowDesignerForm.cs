using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Presentation.Modules.Workflow.Dialogs;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using Newtonsoft.Json;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowDesignerForm : Form, IWorkflowDesignerView
    {
        // ── Dependencies ─────────────────────────────────────────────────────────
        private readonly WorkflowDesignerPresenter _presenter;
        private readonly UserContext _userContext;
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private int?   _templateId;
        private string _templateName;
        private string _templateDescription;

        // ── Canvas state ─────────────────────────────────────────────────────────
        private List<WorkflowStepDesignData> _steps       = new List<WorkflowStepDesignData>();
        private List<WorkflowConnectionDesignData> _conns = new List<WorkflowConnectionDesignData>();
        private WorkflowStepDesignData _selectedStep;
        private bool _snapEnabled = true;
        private bool _gridEnabled = true;
        private int _zoomPercent  = 100;

        // ── Drag state ───────────────────────────────────────────────────────────
        private WorkflowStepDesignData _dragStep;
        private Point _dragOffset;
        private bool _isDragging;

        // ── Nav ──────────────────────────────────────────────────────────────────
        private Panel _activeNavItem;

        // ── Colors ───────────────────────────────────────────────────────────────
        private readonly Color _navyDark  = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover  = Color.FromArgb(245, 249, 255);

        // ── Step type visual config ───────────────────────────────────────────────
        private static readonly Dictionary<string, Color> StepFill = new Dictionary<string, Color>
        {
            { "Start",        Color.FromArgb(34, 153, 84)  },
            { "End",          Color.FromArgb(185, 40, 40)  },
            { "Approval",     Color.FromArgb(44, 100, 210) },
            { "Review",       Color.FromArgb(44, 100, 210) },
            { "Notification", Color.FromArgb(197, 120, 16) },
            { "Sub-Workflow", Color.FromArgb(100, 60, 190) },
            { "System Task",  Color.FromArgb(80, 90, 110)  },
            { "Decision",     Color.FromArgb(197, 100, 16) },
            { "Parallel",     Color.FromArgb(30, 120, 180) },
            { "Merge",        Color.FromArgb(90, 100, 115) },
            { "Wait Timer",   Color.FromArgb(22, 142, 142) },
        };

        // ── Step geometry ─────────────────────────────────────────────────────────
        private const int NodeW  = 110;
        private const int NodeH  = 44;
        private const int GridSz = 20;

        // Parameterless constructor — Visual Studio Designer only
        public WorkflowDesignerForm()
        {
            InitializeComponent();
        }

        public WorkflowDesignerForm(
            int? templateId,
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _templateId   = templateId;
            _userContext  = userContext;
            _templateService = templateService;
            _loggingService  = loggingService;

            // Only create presenter if services are available
            if (templateService != null && loggingService != null && userContext != null)
            {
                _presenter = new WorkflowDesignerPresenter(
                    this, templateService, loggingService, userContext);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // FORM LOAD
        // ════════════════════════════════════════════════════════════════════════

        private void WorkflowDesignerForm_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            try
            {
                BuildRibbonTabs();
                BuildToolbar();
                Panel initialNav;
                WorkflowNavigationBuilder.BuildSidebar(
                    pnlSidebarContainer, "designer", OnNavItemClick, out initialNav);
                _activeNavItem = initialNav;
                BuildPalette();
                BuildStatusBar();
                WireCanvasEvents();

                // Reset defaults so we don't show hardcoded mock values
                if (!_templateId.HasValue)
                {
                    lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Templates  ›  New Template";
                    lblTitle.Text = "(ยังไม่ได้บันทึก Template ใหม่)";
                    lblSubtitle.Text = "กรุณาออกแบบกระบวนการและคลิกปุ่มบันทึกบนเครื่องมือเพื่อตั้งชื่อเทมเพลต";
                    lblStatusBadge.ForeColor = Color.FromArgb(120, 130, 150);
                    lblStatusBadge.Text = "●  New Template (Draft)";
                }

                // Only load data if presenter is available
                if (_presenter != null)
                {
                    if (_templateId.HasValue)
                        _presenter.LoadTemplate(_templateId.Value);
                    else
                        _presenter.CreateNewTemplate();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading designer: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // RIBBON TABS
        // ════════════════════════════════════════════════════════════════════════

        private void BuildRibbonTabs()
        {
            var tabs = new[]
            {
                new { Text = "แฟ้ม",       Active = false },
                new { Text = "หน้าแรก",    Active = true  },
                new { Text = "ดำเนินการ",  Active = false },
                new { Text = "รูปมอง",     Active = false },
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
                btn.FlatAppearance.BorderSize              = 0;
                btn.FlatAppearance.MouseOverBackColor      = Color.FromArgb(50, 80, 130);
                btn.FlatAppearance.MouseDownBackColor      = Color.FromArgb(40, 70, 120);

                if (t.Active)
                {
                    btn.Paint += (s, pe) =>
                        pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
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
        // TOOLBAR
        // ════════════════════════════════════════════════════════════════════════

        private void BuildToolbar()
        {
            var groups = new[]
            {
                new { Header = "File", Items = new[]
                {
                    new { Icon = "💾", Text = "บันทึก",        Tag = "save"       },
                    new { Icon = "📄", Text = "เวอร์ชั่นใหม่", Tag = "newversion"  },
                    new { Icon = "👁", Text = "Preview",        Tag = "preview"    },
                    new { Icon = "🚀", Text = "Publish",        Tag = "publish"    },
                }},
                new { Header = "แก้ไข", Items = new[]
                {
                    new { Icon = "↩", Text = "Undo", Tag = "undo" },
                    new { Icon = "↪", Text = "Redo", Tag = "redo" },
                }},
                new { Header = "เครื่องมือ", Items = new[]
                {
                    new { Icon = "🗑", Text = "ลบ",      Tag = "delete"     },
                    new { Icon = "⊡", Text = "Snap",     Tag = "snap"       },
                    new { Icon = "⊞", Text = "Grid",     Tag = "grid"       },
                    new { Icon = "⟷", Text = "จัดเส้น", Tag = "autolayout" },
                }},
                new { Header = "ตรวจสอบ", Items = new[]
                {
                    new { Icon = "✓", Text = "Validate", Tag = "validate" },
                    new { Icon = "▶", Text = "Test Run", Tag = "testrun"  },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int groupStartX = x;

                foreach (var item in group.Items)
                {
                    var tag   = item.Tag;
                    var icon  = item.Icon;
                    var label = item.Text;

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
                        Text      = icon,
                        Font      = new Font("Segoe UI", 15F),
                        ForeColor = _navyDark,
                        Location  = new Point(0, 6),
                        Size      = new Size(58, 28),
                        TextAlign = ContentAlignment.MiddleCenter,
                    };
                    var lblText = new Label
                    {
                        Text      = label,
                        Font      = new Font("Segoe UI", 7.5F),
                        ForeColor = Color.FromArgb(55, 65, 90),
                        Location  = new Point(0, 34),
                        Size      = new Size(58, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                    };
                    pnlBtn.Controls.Add(lblIcon);
                    pnlBtn.Controls.Add(lblText);

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

                    foreach (Control c in new Control[] { pnlBtn, lblIcon, lblText })
                    {
                        c.MouseEnter += enter;
                        c.MouseLeave += leave;
                        c.Click      += click;
                    }

                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 60;
                }

                // Group label
                int gw = x - groupStartX;
                pnlToolbarContainer.Controls.Add(new Label
                {
                    Text      = group.Header,
                    Font      = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point(groupStartX, 64),
                    Size      = new Size(gw, 12),
                });

                // Separator (except last group)
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
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "save":        DoSave();        break;
                case "newversion":  DoNewVersion();  break;
                case "publish":     DoPublish();     break;
                case "preview":     ShowInfo("Preview: ยังไม่ได้ Implement"); break;
                case "undo":        ShowInfo("Undo: ยังไม่ได้ Implement");    break;
                case "redo":        ShowInfo("Redo: ยังไม่ได้ Implement");    break;
                case "delete":      DeleteSelected(); break;
                case "snap":        ToggleSnap();    break;
                case "grid":        ToggleGrid();    break;
                case "autolayout":  AutoLayout();    break;
                case "validate":    _presenter.ValidateTemplate(); break;
                case "testrun":     DoTestRun(); break;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // PALETTE
        // ════════════════════════════════════════════════════════════════════════

        private void BuildPalette()
        {
            var sections = new[]
            {
                new { Header = "START/END", Items = new[]
                {
                    new { Icon = "▷", Text = "Start",    Type = "Start" },
                    new { Icon = "□", Text = "End",      Type = "End"   },
                }},
                new { Header = "ACTIVITY", Items = new[]
                {
                    new { Icon = "✓", Text = "Approval",     Type = "Approval"     },
                    new { Icon = "👁", Text = "Review",       Type = "Review"       },
                    new { Icon = "🔔", Text = "Notification", Type = "Notification" },
                    new { Icon = "⚙", Text = "Sub-Workflow", Type = "Sub-Workflow" },
                    new { Icon = "⚡", Text = "System Task",  Type = "System Task"  },
                }},
                new { Header = "CONTROL FLOW", Items = new[]
                {
                    new { Icon = "◇", Text = "Decision",   Type = "Decision"   },
                    new { Icon = "⫶", Text = "Parallel",   Type = "Parallel"   },
                    new { Icon = "⋀", Text = "Merge",      Type = "Merge"      },
                    new { Icon = "⏰", Text = "Wait Timer", Type = "Wait Timer" },
                }},
            };

            int y = 4;
            foreach (var sec in sections)
            {
                // Section header
                pnlPaletteItems.Controls.Add(new Label
                {
                    Text      = sec.Header,
                    Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(130, 140, 160),
                    Location  = new Point(12, y + 2),
                    Size      = new Size(160, 16),
                    AutoSize  = false,
                });
                y += 22;

                foreach (var item in sec.Items)
                {
                    var type  = item.Type;
                    var color = StepFill.ContainsKey(type) ? StepFill[type] : Color.Gray;

                    var pnlItem = new Panel
                    {
                        Location  = new Point(8, y),
                        Size      = new Size(170, 30),
                        BackColor = Color.White,
                        Cursor    = Cursors.Hand,
                        Tag       = type,
                    };

                    // Color swatch
                    var swatch = new Panel
                    {
                        Location  = new Point(0, 6),
                        Size      = new Size(4, 18),
                        BackColor = color,
                    };

                    var lblIcon = new Label
                    {
                        Text      = item.Icon,
                        Font      = new Font("Segoe UI", 10F),
                        ForeColor = color,
                        Location  = new Point(8, 5),
                        Size      = new Size(22, 22),
                        TextAlign = ContentAlignment.MiddleCenter,
                    };

                    var lblText = new Label
                    {
                        Text      = item.Text,
                        Font      = new Font("Segoe UI", 9F),
                        ForeColor = Color.FromArgb(40, 50, 70),
                        Location  = new Point(32, 6),
                        Size      = new Size(110, 20),
                        TextAlign = ContentAlignment.MiddleLeft,
                    };

                    var lblHandle = new Label
                    {
                        Text      = "⋮⋮",
                        Font      = new Font("Segoe UI", 9F),
                        ForeColor = Color.FromArgb(180, 185, 200),
                        Location  = new Point(148, 6),
                        Size      = new Size(18, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                    };

                    pnlItem.Controls.Add(swatch);
                    pnlItem.Controls.Add(lblIcon);
                    pnlItem.Controls.Add(lblText);
                    pnlItem.Controls.Add(lblHandle);

                    // Hover
                    Color hoverBg = Color.FromArgb(245, 248, 255);
                    EventHandler enter = (s, ev) => { foreach (Control c in pnlItem.Controls) c.BackColor = hoverBg; pnlItem.BackColor = hoverBg; };
                    EventHandler leave = (s, ev) => { foreach (Control c in pnlItem.Controls) { if (c != swatch) c.BackColor = Color.White; } pnlItem.BackColor = Color.White; };

                    foreach (Control c in new Control[] { pnlItem, lblIcon, lblText, lblHandle })
                    {
                        c.MouseEnter += enter;
                        c.MouseLeave += leave;
                    }

                    // Double-click to add to canvas
                    EventHandler dblClick = (s, ev) => AddStepToCanvas(type);
                    pnlItem.DoubleClick += dblClick;
                    lblText.DoubleClick += dblClick;
                    lblIcon.DoubleClick += dblClick;

                    pnlPaletteItems.Controls.Add(pnlItem);
                    y += 32;
                }
                y += 6;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // STATUS BAR
        // ════════════════════════════════════════════════════════════════════════

        private void BuildStatusBar()
        {
            var statusItems = new[]
            {
                new { Text = "พร้อมใช้งาน",                    Align = ContentAlignment.MiddleLeft  },
                new { Text = "เชื่อมต่อ: CMS-DB01",           Align = ContentAlignment.MiddleLeft  },
                new { Text = "โดย: อรรถพล สุวรรณ (Admin)",    Align = ContentAlignment.MiddleLeft  },
                new { Text = "Zoom 100%",                       Align = ContentAlignment.MiddleRight },
                new { Text = "TH",                              Align = ContentAlignment.MiddleRight },
                new { Text = "ออนไลน์",                         Align = ContentAlignment.MiddleRight },
            };

            pnlStatusBar.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(210, 215, 225)), 0, 0, pnlStatusBar.Width, 0);

            int x = 8;
            foreach (var si in statusItems)
            {
                var lbl = new Label
                {
                    Text      = "  " + si.Text + "  ",
                    Font      = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(80, 90, 110),
                    AutoSize  = true,
                    Location  = new Point(x, 4),
                    TextAlign = si.Align,
                };
                pnlStatusBar.Controls.Add(lbl);
                x += lbl.PreferredWidth + 4;

                if (x < pnlStatusBar.Width - 200)
                {
                    pnlStatusBar.Controls.Add(new Panel
                    {
                        BackColor = Color.FromArgb(210, 215, 225),
                        Location  = new Point(x, 4),
                        Size      = new Size(1, 16),
                    });
                    x += 6;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // CANVAS EVENTS
        // ════════════════════════════════════════════════════════════════════════

        private void WireCanvasEvents()
        {
            // Eliminate flicker during drag by enabling double-buffering on the owner-draw panel
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, pnlCanvas, new object[] { true });

            pnlCanvas.Paint      += pnlCanvas_Paint;
            pnlCanvas.MouseDown  += pnlCanvas_MouseDown;
            pnlCanvas.MouseMove  += pnlCanvas_MouseMove;
            pnlCanvas.MouseUp    += pnlCanvas_MouseUp;
            pnlCanvas.DoubleClick+= pnlCanvas_DoubleClick;

            btnZoomIn.Click  += (s, e) => { _zoomPercent = Math.Min(200, _zoomPercent + 10); RefreshZoom(); };
            btnZoomOut.Click += (s, e) => { _zoomPercent = Math.Max(25,  _zoomPercent - 10); RefreshZoom(); };
            btnZoomFit.Click += (s, e) => { _zoomPercent = 100; RefreshZoom(); };
        }

        private void RefreshZoom()
        {
            lblZoomLevel.Text = $"{_zoomPercent}%";
            pnlCanvas.Invalidate();
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float scale = _zoomPercent / 100f;
            g.ScaleTransform(scale, scale);

            // Grid dots
            if (_gridEnabled)
                DrawGrid(g, pnlCanvas.Width, pnlCanvas.Height, scale);

            // Connections
            foreach (var conn in _conns)
            {
                var from = _steps.FirstOrDefault(s => s.WorkflowStepId == conn.FromStepId);
                var to   = _steps.FirstOrDefault(s => s.WorkflowStepId == conn.ToStepId);
                if (from != null && to != null)
                    DrawArrow(g, from, to, conn.Condition);
            }

            // Steps
            foreach (var step in _steps)
                DrawNode(g, step, step == _selectedStep);
        }

        private void DrawGrid(Graphics g, int w, int h, float scale)
        {
            int scaledW = (int)(w / scale);
            int scaledH = (int)(h / scale);
            using (var pen = new SolidBrush(Color.FromArgb(50, 160, 170, 190)))
            {
                for (int x = 0; x < scaledW; x += GridSz)
                    for (int y = 0; y < scaledH; y += GridSz)
                        g.FillEllipse(pen, x - 1, y - 1, 2, 2);
            }
        }

        private void DrawNode(Graphics g, WorkflowStepDesignData step, bool selected)
        {
            var fill  = StepFill.ContainsKey(step.StepType) ? StepFill[step.StepType] : Color.Gray;
            var rect  = new Rectangle(step.CanvasX, step.CanvasY, NodeW, NodeH);

            bool isDiamond = step.StepType == "Decision";

            if (isDiamond)
            {
                DrawDiamond(g, step, fill, selected);
                return;
            }

            // Shadow
            using (var shadow = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            {
                var sr = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height);
                g.FillPath(shadow, RoundRect(sr, 8));
            }

            // Fill
            using (var br = new SolidBrush(Color.White))
                g.FillPath(br, RoundRect(rect, 8));

            // Top accent strip (step type color)
            var accentRect = new Rectangle(rect.X, rect.Y, rect.Width, 5);
            using (var abr = new SolidBrush(fill))
            {
                var accentPath = new GraphicsPath();
                accentPath.AddArc(rect.X, rect.Y, 16, 16, 180, 90);
                accentPath.AddArc(rect.Right - 16, rect.Y, 16, 16, 270, 90);
                accentPath.AddLine(rect.Right, rect.Y + 5, rect.X, rect.Y + 5);
                accentPath.CloseFigure();
                g.FillPath(abr, accentPath);
            }

            // Border
            using (var pen = new Pen(selected ? Color.FromArgb(59, 91, 219) : Color.FromArgb(210, 215, 225), selected ? 2f : 1f))
                g.DrawPath(pen, RoundRect(rect, 8));

            // Selection glow
            if (selected)
            {
                using (var glowPen = new Pen(Color.FromArgb(80, 59, 91, 219), 4f))
                {
                    var glowRect = new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4);
                    g.DrawPath(glowPen, RoundRect(glowRect, 10));
                }
            }

            // Step name
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            using (var textBr = new SolidBrush(Color.FromArgb(30, 40, 65)))
            using (var font = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                var textRect = new RectangleF(rect.X + 4, rect.Y + 8, rect.Width - 8, rect.Height - 10);
                g.DrawString(step.StepName, font, textBr, textRect, sf);
            }

            // Sub-label (step type in small text)
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
            using (var smallBr = new SolidBrush(Color.FromArgb(140, 150, 165)))
            using (var smallFont = new Font("Segoe UI", 7F))
            {
                string sub = step.AssignedUser != null && step.AssignedUser.Length > 0
                    ? TruncateGroup(step.AssignedUser) : step.StepType;
                g.DrawString(sub, smallFont, smallBr,
                    new RectangleF(rect.X + 2, rect.Y + NodeH - 14, rect.Width - 4, 13),
                    sf);
            }
        }

        private void DrawDiamond(Graphics g, WorkflowStepDesignData step, Color fill, bool selected)
        {
            int cx = step.CanvasX + NodeW / 2;
            int cy = step.CanvasY + NodeH / 2;
            int hw = NodeW / 2;
            int hh = NodeH / 2;

            var pts = new[] {
                new Point(cx, cy - hh),
                new Point(cx + hw, cy),
                new Point(cx, cy + hh),
                new Point(cx - hw, cy),
            };

            using (var br = new SolidBrush(Color.White))
                g.FillPolygon(br, pts);
            using (var pen = new Pen(selected ? Color.FromArgb(59, 91, 219) : fill, selected ? 2f : 1.5f))
                g.DrawPolygon(pen, pts);

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var textBr = new SolidBrush(fill))
            using (var font = new Font("Segoe UI", 7.5F, FontStyle.Bold))
            {
                g.DrawString(step.StepName, font, textBr,
                    new RectangleF(step.CanvasX, step.CanvasY, NodeW, NodeH), sf);
            }
        }

        private void DrawArrow(Graphics g, WorkflowStepDesignData from, WorkflowStepDesignData to, string condition)
        {
            int x1 = from.CanvasX + NodeW;
            int y1 = from.CanvasY + NodeH / 2;
            int x2 = to.CanvasX;
            int y2 = to.CanvasY + NodeH / 2;

            using (var pen = new Pen(Color.FromArgb(160, 170, 190), 1.5f) { EndCap = LineCap.ArrowAnchor })
            {
                int cx = (x1 + x2) / 2;
                g.DrawBezier(pen, x1, y1, cx, y1, cx, y2, x2, y2);
            }

            // Condition label
            if (!string.IsNullOrEmpty(condition))
            {
                int mx = (x1 + x2) / 2;
                int my = (y1 + y2) / 2 - 10;
                using (var br = new SolidBrush(Color.FromArgb(100, 110, 130)))
                using (var f  = new Font("Segoe UI", 7F))
                    g.DrawString(condition, f, br, new PointF(mx, my));
            }
        }

        private void pnlCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            float scale = _zoomPercent / 100f;
            int mx = (int)(e.X / scale);
            int my = (int)(e.Y / scale);

            var hit = HitTestStep(mx, my);
            if (hit != null)
            {
                _selectedStep  = hit;
                _dragStep      = hit;
                _dragOffset    = new Point(mx - hit.CanvasX, my - hit.CanvasY);
                _isDragging    = true;
                ShowStepProperties(hit);
                pnlCanvas.Invalidate();
            }
            else
            {
                _selectedStep = null;
                _isDragging   = false;
                pnlCanvas.Invalidate();
            }
        }

        private void pnlCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _dragStep == null) return;

            float scale = _zoomPercent / 100f;
            int nx = (int)(e.X / scale) - _dragOffset.X;
            int ny = (int)(e.Y / scale) - _dragOffset.Y;

            if (_snapEnabled)
            {
                nx = (nx / GridSz) * GridSz;
                ny = (ny / GridSz) * GridSz;
            }

            _dragStep.CanvasX = Math.Max(0, nx);
            _dragStep.CanvasY = Math.Max(0, ny);
            pnlCanvas.Invalidate();
        }

        private void pnlCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _dragStep   = null;
        }

        private void pnlCanvas_DoubleClick(object sender, EventArgs e)
        {
            if (_selectedStep != null)
                EditStepConnection(_selectedStep);
        }

        private WorkflowStepDesignData HitTestStep(int x, int y)
        {
            return _steps.LastOrDefault(s =>
                x >= s.CanvasX && x <= s.CanvasX + NodeW &&
                y >= s.CanvasY && y <= s.CanvasY + NodeH);
        }

        // ════════════════════════════════════════════════════════════════════════
        // PROPERTIES PANEL
        // ════════════════════════════════════════════════════════════════════════

        private bool _propEventsWired = false;

        private void ShowStepProperties(WorkflowStepDesignData step)
        {
            // ── Badge ────────────────────────────────────────────────────────
            var typeLabel = step.StepType ?? "Step";
            lblSelectedBadge.Text = $"●  Selected · {typeLabel}";
            if (StepFill.ContainsKey(step.StepType))
            {
                var c = StepFill[step.StepType];
                lblSelectedBadge.ForeColor = c;
                lblSelectedBadge.BackColor = Color.FromArgb(30, c.R, c.G, c.B);
            }

            // ── Populate fields ──────────────────────────────────────────────
            txtStepName.Text = step.StepName;

            // Assignee — ถ้า AssignedUser ตรงกับ item ใน combo ให้ select ไม่งั้น add
            if (!string.IsNullOrEmpty(step.AssignedUser))
            {
                int idx = cmbAssignee.FindStringExact(step.AssignedUser);
                if (idx >= 0)
                    cmbAssignee.SelectedIndex = idx;
                else
                {
                    cmbAssignee.Items.Add(step.AssignedUser);
                    cmbAssignee.SelectedItem = step.AssignedUser;
                }
            }
            else
            {
                cmbAssignee.SelectedIndex = cmbAssignee.Items.Count > 0 ? 0 : -1;
            }

            // SLA
            if (step.SLADays > 0 && step.SLADays <= 365)
                numSLA.Value = step.SLADays;
            else
                numSLA.Value = 2;

            // Priority
            cmbPriority.SelectedIndex = Math.Max(0, Math.Min(step.Priority, cmbPriority.Items.Count - 1));

            // Escalation
            chkEscalation.Checked = step.EscalationEnabled;

            // Actions
            chkApprove.Checked = step.AllowApprove;
            chkReject.Checked  = step.AllowReject;
            chkForward.Checked = step.AllowForward;
            chkAck.Checked     = step.AllowAck;

            // Work Instruction
            if (!string.IsNullOrEmpty(step.WorkInstructionRef))
                linkWorkInstruction.Text = $"⇗  {step.WorkInstructionRef}";
            else
                linkWorkInstruction.Text = "⇗  (ยังไม่ได้กำหนด)";

            // Conditions
            var outgoingConnections = GetOutgoingConnections(step.WorkflowStepId);
            var outgoingConnection = outgoingConnections.Count == 1 ? outgoingConnections[0] : null;
            txtConditions.Text = outgoingConnection != null
                ? (outgoingConnection.Condition ?? string.Empty)
                : (outgoingConnections.Count > 1
                    ? string.Format("มี {0} เส้นทางออก - ดับเบิลคลิกที่ Step เพื่อจัดการแต่ละเงื่อนไข", outgoingConnections.Count)
                    : (step.ConditionExpression ?? string.Empty));

            // ── Wire save-back events (once) ─────────────────────────────────
            if (!_propEventsWired)
            {
                _propEventsWired = true;

                txtStepName.TextChanged += (s, e) =>
                {
                    if (_selectedStep != null)
                    {
                        _selectedStep.StepName = txtStepName.Text;
                        pnlCanvas.Invalidate();
                    }
                };
                cmbAssignee.SelectedIndexChanged += (s, e) =>
                {
                    if (_selectedStep != null)
                        _selectedStep.AssignedUser = cmbAssignee.SelectedItem as string ?? string.Empty;
                };
                numSLA.ValueChanged += (s, e) =>
                {
                    if (_selectedStep != null)
                        _selectedStep.SLADays = (int)numSLA.Value;
                };
                cmbPriority.SelectedIndexChanged += (s, e) =>
                {
                    if (_selectedStep != null)
                        _selectedStep.Priority = cmbPriority.SelectedIndex;
                };
                chkEscalation.CheckedChanged += (s, e) =>
                {
                    if (_selectedStep != null)
                        _selectedStep.EscalationEnabled = chkEscalation.Checked;
                };
                chkApprove.CheckedChanged += (s, e) =>
                {
                    if (_selectedStep != null) _selectedStep.AllowApprove = chkApprove.Checked;
                };
                chkReject.CheckedChanged += (s, e) =>
                {
                    if (_selectedStep != null) _selectedStep.AllowReject = chkReject.Checked;
                };
                chkForward.CheckedChanged += (s, e) =>
                {
                    if (_selectedStep != null) _selectedStep.AllowForward = chkForward.Checked;
                };
                chkAck.CheckedChanged += (s, e) =>
                {
                    if (_selectedStep != null) _selectedStep.AllowAck = chkAck.Checked;
                };
                txtConditions.TextChanged += (s, e) =>
                {
                    if (_selectedStep != null)
                    {
                        var currentConnections = GetOutgoingConnections(_selectedStep.WorkflowStepId);
                        var currentConnection = currentConnections.Count == 1 ? currentConnections[0] : null;
                        if (currentConnection != null)
                            currentConnection.Condition = txtConditions.Text;
                        else if (currentConnections.Count == 0)
                            _selectedStep.ConditionExpression = txtConditions.Text;
                        pnlCanvas.Invalidate();
                    }
                };
            }

            pnlProperties.Visible = true;
        }

        // ════════════════════════════════════════════════════════════════════════
        // TOOLBAR ACTIONS
        // ════════════════════════════════════════════════════════════════════════

        private void AddStepToCanvas(string stepType)
        {
            int id   = _steps.Count + 1;
            int col  = (_steps.Count % 5);
            int row  = (_steps.Count / 5);
            var step = new WorkflowStepDesignData
            {
                WorkflowStepId = id,
                StepName       = stepType == "Start" || stepType == "End" ? stepType : $"{stepType} {id}",
                StepType       = stepType,
                CanvasX        = 60 + col * (NodeW + 40),
                CanvasY        = 80 + row * (NodeH + 60),
                AssignedUser   = "",
                Description    = "",
            };
            DrawStep(step);
        }

        private void DeleteSelected()
        {
            if (_selectedStep == null) { ShowError("เลือก Step ที่ต้องการลบก่อน"); return; }

            var r = MessageBox.Show(
                $"ลบ Step '{_selectedStep.StepName}' ?", "ยืนยันการลบ",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (r == DialogResult.Yes)
            {
                _conns.RemoveAll(c => c.FromStepId == _selectedStep.WorkflowStepId
                                   || c.ToStepId   == _selectedStep.WorkflowStepId);
                _steps.Remove(_selectedStep);
                _selectedStep = null;
                pnlCanvas.Invalidate();
            }
        }

        private List<WorkflowConnectionDesignData> GetOutgoingConnections(int stepId)
        {
            return _conns.Where(c => c.FromStepId == stepId).ToList();
        }

        private WorkflowConnectionDesignData GetSingleOutgoingConnection(int stepId)
        {
            var connections = GetOutgoingConnections(stepId);
            return connections.Count == 1 ? connections[0] : null;
        }

        private void EditStepConnection(WorkflowStepDesignData fromStep)
        {
            if (fromStep == null) return;

            var targetSteps = _steps.Where(s => s.WorkflowStepId != fromStep.WorkflowStepId)
                .OrderBy(s => s.WorkflowStepId)
                .ToList();

            if (targetSteps.Count == 0)
            {
                ShowError("ต้องมี Step อื่นอย่างน้อย 1 รายการเพื่อเชื่อมเส้นทาง");
                return;
            }

            using (var dlg = new Form())
            {
                dlg.Text            = "";
                dlg.FormBorderStyle = FormBorderStyle.None;
                dlg.ClientSize      = new Size(460, 320);
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.BackColor       = Color.FromArgb(235, 238, 245);
                dlg.Paint += (s, pe) =>
                    pe.Graphics.DrawRectangle(new Pen(Color.FromArgb(180, 190, 210)), 0, 0, dlg.Width - 1, dlg.Height - 1);

                // ── Title bar ─────────────────────────────────────────────────
                var pnlTitleBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = _navyDark };
                var lblDlgTitle = new Label
                {
                    Text      = "  จัดการเส้นทาง  ·  " + fromStep.StepName,
                    Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(12, 0, 0, 0),
                };
                var btnX = new Button
                {
                    Text      = "✕",
                    Dock      = DockStyle.Right,
                    Width     = 44,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(160, 200, 245),
                    Font      = new Font("Segoe UI", 11F),
                    Cursor    = Cursors.Hand,
                };
                btnX.FlatAppearance.BorderSize         = 0;
                btnX.FlatAppearance.MouseOverBackColor = Color.FromArgb(185, 40, 40);
                btnX.Click += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

                // drag-to-move on title bar
                Point dragPt  = Point.Empty;
                bool  dlgDrag = false;
                MouseEventHandler titleDown = (s, e) => { dlgDrag = true;  dragPt = e.Location; };
                MouseEventHandler titleUp   = (s, e) => { dlgDrag = false; };
                MouseEventHandler titleMove = (s, e) =>
                {
                    if (!dlgDrag) return;
                    dlg.Location = new Point(dlg.Left + e.X - dragPt.X, dlg.Top + e.Y - dragPt.Y);
                };
                foreach (Control c in new Control[] { pnlTitleBar, lblDlgTitle })
                {
                    c.MouseDown += titleDown;
                    c.MouseUp   += titleUp;
                    c.MouseMove += titleMove;
                }

                pnlTitleBar.Controls.Add(lblDlgTitle);
                pnlTitleBar.Controls.Add(btnX);

                // ── Card body ─────────────────────────────────────────────────
                var pnlCard = new Panel { BackColor = Color.White, Location = new Point(12, 56), Size = new Size(436, 200) };
                pnlCard.Paint += (s, pe) =>
                    pe.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 225, 235)), 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);

                var lblFrom = new Label
                {
                    Text      = "จาก Step",
                    Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 110, 130),
                    Location  = new Point(16, 16),
                    AutoSize  = true,
                };
                var txtFrom = new TextBox
                {
                    Location    = new Point(16, 34),
                    Size        = new Size(186, 28),
                    ReadOnly    = true,
                    Text        = fromStep.StepName,
                    Font        = new Font("Segoe UI", 9F),
                    BackColor   = Color.FromArgb(245, 248, 255),
                    BorderStyle = BorderStyle.FixedSingle,
                };
                var lblArrow = new Label
                {
                    Text      = "→",
                    Font      = new Font("Segoe UI", 13F),
                    ForeColor = _navyDark,
                    Location  = new Point(206, 35),
                    Size      = new Size(22, 24),
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                var lblTo = new Label
                {
                    Text      = "ไป Step",
                    Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 110, 130),
                    Location  = new Point(232, 16),
                    AutoSize  = true,
                };
                var cmbTo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location      = new Point(232, 34),
                    Size          = new Size(188, 28),
                    Font          = new Font("Segoe UI", 9F),
                };
                var lblCondition = new Label
                {
                    Text      = "Condition Expression  (ว่างเปล่า = ผ่านเสมอ)",
                    Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 110, 130),
                    Location  = new Point(16, 74),
                    AutoSize  = true,
                };
                var txtCondition = new TextBox
                {
                    Location    = new Point(16, 92),
                    Size        = new Size(404, 92),
                    Multiline   = true,
                    ScrollBars  = ScrollBars.Vertical,
                    Font        = new Font("Segoe UI", 9F),
                    BorderStyle = BorderStyle.FixedSingle,
                };
                pnlCard.Controls.AddRange(new Control[] { lblFrom, txtFrom, lblArrow, lblTo, cmbTo, lblCondition, txtCondition });

                // ── Footer ────────────────────────────────────────────────────
                var pnlFooter = new Panel { BackColor = Color.FromArgb(247, 249, 252), Location = new Point(12, 264), Size = new Size(436, 44) };
                pnlFooter.Paint += (s, pe) =>
                    pe.Graphics.DrawLine(new Pen(Color.FromArgb(220, 225, 235)), 0, 0, pnlFooter.Width, 0);

                var btnSave = new Button
                {
                    Text         = "บันทึกเส้นทาง",
                    Size         = new Size(116, 28),
                    Location     = new Point(196, 8),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = _navyDark,
                    ForeColor    = Color.White,
                    Font         = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor       = Cursors.Hand,
                    DialogResult = DialogResult.OK,
                };
                btnSave.FlatAppearance.BorderSize         = 0;
                btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(44, 75, 130);

                var btnRemove = new Button
                {
                    Text      = "ลบเส้นทาง",
                    Size      = new Size(96, 28),
                    Location  = new Point(8, 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(185, 40, 40),
                    Font      = new Font("Segoe UI", 9F),
                    Cursor    = Cursors.Hand,
                };
                btnRemove.FlatAppearance.BorderColor        = Color.FromArgb(220, 160, 160);
                btnRemove.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 245, 245);

                var btnCancel = new Button
                {
                    Text         = "ยกเลิก",
                    Size         = new Size(80, 28),
                    Location     = new Point(348, 8),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = Color.White,
                    ForeColor    = Color.FromArgb(80, 90, 110),
                    Font         = new Font("Segoe UI", 9F),
                    Cursor       = Cursors.Hand,
                    DialogResult = DialogResult.Cancel,
                };
                btnCancel.FlatAppearance.BorderColor        = Color.FromArgb(210, 215, 225);
                btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 248, 255);

                pnlFooter.Controls.AddRange(new Control[] { btnRemove, btnSave, btnCancel });

                dlg.Controls.Add(pnlTitleBar);
                dlg.Controls.Add(pnlCard);
                dlg.Controls.Add(pnlFooter);
                dlg.AcceptButton = btnSave;
                dlg.CancelButton = btnCancel;

                // ── Data binding ──────────────────────────────────────────────
                cmbTo.DataSource    = targetSteps;
                cmbTo.DisplayMember = "StepName";
                cmbTo.ValueMember   = "WorkflowStepId";

                var existingConnection = _conns.FirstOrDefault(c => c.FromStepId == fromStep.WorkflowStepId);
                if (existingConnection != null)
                {
                    cmbTo.SelectedValue = existingConnection.ToStepId;
                    txtCondition.Text   = existingConnection.Condition ?? string.Empty;
                }

                EventHandler loadSelectedConnection = (s, e) =>
                {
                    var selectedTarget = cmbTo.SelectedItem as WorkflowStepDesignData;
                    if (selectedTarget == null) return;
                    var selectedConnection = _conns.FirstOrDefault(c =>
                        c.FromStepId == fromStep.WorkflowStepId &&
                        c.ToStepId   == selectedTarget.WorkflowStepId);
                    txtCondition.Text = selectedConnection != null ? selectedConnection.Condition ?? string.Empty : string.Empty;
                };
                cmbTo.SelectedIndexChanged += loadSelectedConnection;

                btnRemove.Enabled = existingConnection != null;
                btnRemove.Click += (s, e) =>
                {
                    var selectedTarget = cmbTo.SelectedItem as WorkflowStepDesignData;
                    if (selectedTarget == null) return;
                    var conn = _conns.FirstOrDefault(c =>
                        c.FromStepId == fromStep.WorkflowStepId &&
                        c.ToStepId   == selectedTarget.WorkflowStepId);
                    if (conn != null)
                    {
                        _conns.Remove(conn);
                        pnlCanvas.Invalidate();
                    }
                    dlg.DialogResult = DialogResult.Cancel;
                    dlg.Close();
                };

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var toStep = cmbTo.SelectedItem as WorkflowStepDesignData;
                if (toStep == null) return;

                var matchedConnection = _conns.FirstOrDefault(c =>
                    c.FromStepId == fromStep.WorkflowStepId &&
                    c.ToStepId   == toStep.WorkflowStepId);

                if (matchedConnection == null)
                {
                    _conns.Add(new WorkflowConnectionDesignData
                    {
                        FromStepId = fromStep.WorkflowStepId,
                        ToStepId   = toStep.WorkflowStepId,
                        Condition  = txtCondition.Text.Trim()
                    });
                }
                else
                {
                    matchedConnection.Condition = txtCondition.Text.Trim();
                }

                pnlCanvas.Invalidate();
                ShowStepProperties(fromStep);
            }
        }

        private void ToggleSnap()
        {
            _snapEnabled = !_snapEnabled;
            lblCanvasInfo.Text = $"🔶  Canvas · Grid: {(_gridEnabled ? "on" : "off")} · Snap: {(_snapEnabled ? "on" : "off")}";
        }

        private void ToggleGrid()
        {
            _gridEnabled = !_gridEnabled;
            lblCanvasInfo.Text = $"🔶  Canvas · Grid: {(_gridEnabled ? "on" : "off")} · Snap: {(_snapEnabled ? "on" : "off")}";
            pnlCanvas.Invalidate();
        }

        private void AutoLayout()
        {
            if (_steps.Count == 0) return;
            int x = 60, y = 80;
            foreach (var step in _steps)
            {
                step.CanvasX = x;
                step.CanvasY = y;
                x += NodeW + 50;
                if (x > 900) { x = 60; y += NodeH + 70; }
            }
            pnlCanvas.Invalidate();
        }

        private void DoSave()
        {
            if (string.IsNullOrWhiteSpace(txtStepName.Text) && _steps.Count == 0)
            {
                ShowError("ยังไม่มี Step ใน Workflow");
                return;
            }

            // Prompt for Name and Description if this is a new template
            if (!_templateId.HasValue)
            {
                using (var nameDlg = new CMS.Presentation.Modules.Workflow.Dialogs.TemplateNameDialog())
                {
                    if (nameDlg.ShowDialog(this) == DialogResult.OK)
                    {
                        // Store raw name/description — never embed the "WFD-XXX · " display prefix
                        _templateName        = nameDlg.TemplateName;
                        _templateDescription = nameDlg.TemplateDescription;
                        lblTitle.Text        = nameDlg.TemplateName;
                        lblSubtitle.Text     = nameDlg.TemplateDescription;
                    }
                    else
                    {
                        return; // User cancelled
                    }
                }
            }

            if (_presenter != null)
                _presenter.SaveTemplate(_templateName ?? lblTitle.Text, _templateDescription);
            else
                ShowError("ไม่สามารถบันทึกได้ เนื่องจากไม่มีการเชื่อมต่อกับระบบ");
        }

        private void DoNewVersion()
        {
            ShowInfo("สร้างเวอร์ชั่นใหม่: ยังไม่ได้ Implement");
        }

        private void DoPublish()
        {
            var r = MessageBox.Show(
                "Publish Workflow Template นี้ ?", "ยืนยัน Publish",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                lblStatusBadge.ForeColor = Color.FromArgb(23, 133, 49);
                lblStatusBadge.Text      = "●  Published";
            }
        }

        private void DoTestRun()
        {
            if (!_templateId.HasValue)
            {
                var confirm = MessageBox.Show(
                    "กรุณาบันทึก Template ก่อนทำการทดสอบ Test Run\n\nต้องการบันทึกเดี๋ยวนี้เลยหรือไม่?",
                    "บันทึก Template",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    DoSave();
                    if (!_templateId.HasValue) return; // Save failed
                }
                else
                {
                    return;
                }
            }

            if (_presenter == null)
            {
                ShowError("ไม่พบ Presenter สำหรับการตรวจสอบ");
                return;
            }

            // Validate design first
            if (!_presenter.ValidateTemplate())
            {
                return;
            }

            // Resolve WorkflowService from MDI Parent
            IWorkflowService workflowService = null;
            if (this.MdiParent is MDI_Container mdi)
            {
                workflowService = mdi.WorkflowService;
            }

            if (workflowService == null)
            {
                ShowError("ไม่พบ Workflow Service ในระบบ MDI Container");
                return;
            }

            try
            {
                var currentTemplate = _templateService.GetTemplate(_templateId.Value, _userContext);
                if (currentTemplate != null && !currentTemplate.IsActive)
                {
                    var activateConfirm = MessageBox.Show(
                        "การทำ Test Run จำเป็นต้องเปิดใช้งาน (Activate) Template ก่อน\n\nต้องการเปิดใช้งาน Template นี้เพื่อเริ่มการทดสอบใช่หรือไม่?",
                        "เปิดใช้งาน Template",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (activateConfirm == DialogResult.Yes)
                    {
                        var updateRequest = new UpdateTemplateRequest
                        {
                            WorkflowDefinitionId = _templateId.Value,
                            Name = currentTemplate.Name,
                            Description = currentTemplate.Description,
                            IsActive = true,
                            ChangeDescription = "Activated for Test Run"
                        };
                        _templateService.UpdateTemplate(updateRequest, _userContext);
                        
                        lblStatusBadge.ForeColor = Color.FromArgb(23, 133, 49);
                        lblStatusBadge.Text      = "●  Active";
                    }
                    else
                    {
                        return;
                    }
                }

                using (var dialog = new WorkflowStartDialog(_templateService, _loggingService, _userContext))
                {
                    dialog.PreSelectedTemplateId = _templateId;

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        var startRequest = new CreateWorkflowInstanceRequest
                        {
                            WorkflowDefinitionId = dialog.SelectedTemplateId,
                            Description = dialog.InstanceDescription,
                            Priority = dialog.Priority,
                            InputParameters = dialog.InputParametersJson
                        };

                        var result = workflowService.CreateWorkflowInstance(startRequest, _userContext);

                        if (result != null && result.WorkflowInstanceId > 0)
                        {
                            MessageBox.Show(
                                string.Format("เริ่มทดสอบกระบวนการ (Test Run) สำเร็จ!\n\nInstance ID: {0}\nสถานะ: {1}",
                                    result.WorkflowInstanceId, result.Status),
                                "Test Run สำเร็จ",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Redirect user to Workflow Dashboard or Workflow Monitor to track execution
                            if (this.MdiParent is MDI_Container mdiContainer)
                            {
                                mdiContainer.OpenWorkflowDashboard();
                            }
                        }
                        else
                        {
                            ShowError("เริ่มการทดสอบกระบวนการไม่สำเร็จ");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("Test Run execution failed: " + ex.Message, ex);
                ShowError("เกิดข้อผิดพลาดในการทำ Test Run: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // NAV
        // ════════════════════════════════════════════════════════════════════════

        private void OnNavItemClick(string tag)
        {
            // Handle local navigation first
            if (tag == "designer")
            {
                // Already on this page
                return;
            }

            // Delegate to navigation handler for other forms
            // Note: WorkflowDesignerForm doesn't store service references directly
            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                null, // workflowService
                null, // templateService - managed by presenter
                null, // slaCalculator
                null, // loggingService - managed by presenter
                _userContext,
                null); // auditService
        }

        // ════════════════════════════════════════════════════════════════════════
        // IWorkflowDesignerView — interface implementation
        // ════════════════════════════════════════════════════════════════════════

        public void DisplayTemplate(WorkflowTemplateResult template)
        {
            if (template == null) return;
            var templateCode = string.Format("WFD-{0:D3}", template.WorkflowDefinitionId);

            // Store raw name/description so DoSave() never accidentally writes the display label back
            _templateName        = template.Name ?? "Template";
            _templateDescription = template.Description;

            lblBreadcrumb.Text = string.Format("CMS  ›  Workflow Engine  ›  Templates  ›  {0} · {1}",
                templateCode, _templateName);
            lblTitle.Text    = string.Format("{0} · {1}", templateCode, _templateName);
            lblSubtitle.Text = string.IsNullOrWhiteSpace(_templateDescription)
                ? string.Format("เวอร์ชัน {0} · {1}", template.Version, template.StatusLabel)
                : _templateDescription;
            _templateId      = template.WorkflowDefinitionId;

            if (template.IsActive)
            {
                lblStatusBadge.ForeColor = Color.FromArgb(23, 133, 49);
                lblStatusBadge.Text      = "●  Active";
            }
            else
            {
                lblStatusBadge.ForeColor = Color.FromArgb(197, 80, 0);
                lblStatusBadge.Text      = "●  Draft · ยังไม่ได้เผยแพร่";
            }
        }

        public void DrawStep(WorkflowStepDesignData step)
        {
            if (step == null) return;
            _steps.Add(step);
            pnlCanvas.Invalidate();
        }

        public void DrawConnection(int fromStepId, int toStepId, string condition)
        {
            _conns.Add(new WorkflowConnectionDesignData
            {
                FromStepId = fromStepId,
                ToStepId   = toStepId,
                Condition  = condition,
            });
            pnlCanvas.Invalidate();
        }

        public void ClearCanvas()
        {
            _steps.Clear();
            _conns.Clear();
            _selectedStep = null;
            pnlCanvas.Invalidate();
        }

        public string GetTemplateDefinitionJson()
        {
            return JsonConvert.SerializeObject(new WorkflowDesignerTemplateDefinition
            {
                Steps = _steps,
                Connections = _conns
            }, Formatting.Indented);
        }

        public void ShowStepConfigDialog(WorkflowStepDesignData step) { }

        public void ShowConnectionConfigDialog(int fromStepId, int toStepId) { }

        public void DisplayValidationErrors(List<string> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                lblValidateBadge.Text      = "✓  Validate (0 errors)";
                lblValidateBadge.ForeColor = Color.FromArgb(23, 133, 49);
                ShowInfo("Validate ผ่าน — ไม่พบข้อผิดพลาด");
                return;
            }
            lblValidateBadge.Text      = $"⚠  Validate ({errors.Count} errors)";
            lblValidateBadge.ForeColor = Color.FromArgb(185, 40, 40);
            ShowError("พบข้อผิดพลาด:\n\n" + string.Join("\n", errors));
        }

        public void ShowError(string message) =>
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void ShowInfo(string message) =>
            MessageBox.Show(message, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // ════════════════════════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════════════════════════

        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            int d    = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static string TruncateGroup(string s)
        {
            const string prefix = "AD Group: ";
            if (s.StartsWith(prefix)) s = s.Substring(prefix.Length);
            return s.Length > 18 ? s.Substring(0, 16) + "…" : s;
        }
    }

    public class WorkflowConnectionDesignData
    {
        public int    FromStepId { get; set; }
        public int    ToStepId   { get; set; }
        public string Condition  { get; set; }
    }

    public class WorkflowDesignerTemplateDefinition
    {
        public List<WorkflowStepDesignData> Steps { get; set; } = new List<WorkflowStepDesignData>();
        public List<WorkflowConnectionDesignData> Connections { get; set; } = new List<WorkflowConnectionDesignData>();
    }
}
