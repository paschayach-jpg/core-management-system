using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Visual Flow form — แสดง workflow instance เป็น visual canvas พร้อม right panel.
    /// ตาม wfe-form-ui.md pattern: Ribbon + Toolbar + Sidebar (WorkflowNavigationBuilder) + Canvas + Right panel.
    /// </summary>
    public partial class WorkflowVisualFlowForm : Form
    {
        private readonly IWorkflowService _workflowService;
        private readonly ILoggingService  _loggingService;
        private readonly UserContext      _userContext;
        private int   _zoomPercent  = 100;
        private Panel _activeNavItem;

        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover = Color.FromArgb(245, 249, 255);

        public WorkflowVisualFlowForm(
            IWorkflowService workflowService,
            ILoggingService  loggingService,
            UserContext      userContext)
        {
            InitializeComponent();
            _workflowService = workflowService;
            _loggingService  = loggingService;
            _userContext     = userContext;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Visual Flow: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                case "visual":   break;
                case "timeline": ShowInfo("Timeline view: ยังไม่ได้ Implement"); break;
                case "audit":    ShowInfo("Audit view: ยังไม่ได้ Implement");    break;
                case "reassign": ShowInfo("Reassign: ยังไม่ได้ Implement");      break;
                case "cancel":   ShowInfo("ยกเลิก Workflow: ยังไม่ได้ Implement"); break;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Status bar + Legend bar
        // ════════════════════════════════════════════════════════════════════

        private void BuildStatusBar()
        {
            pnlStatusBar.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(210, 215, 225)), 0, 0, pnlStatusBar.Width, 0);

            // ตรวจสอบ _userContext ก่อนใช้
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
                new { Color = Color.FromArgb(34,  153, 84),  Label = "เริ่มแล้ว" },
                new { Color = Color.FromArgb(230, 115, 0),   Label = "กำลังทำ"  },
                new { Color = Color.FromArgb(107, 114, 128), Label = "รอ"       },
                new { Color = Color.FromArgb(156, 163, 175), Label = "ข้าม"     },
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
            pnlRightScroll.Controls.Clear();
            int y = 12;

            // Step header
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "Step ปัจจุบัน", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 36, 55), Location = new Point(12, y), AutoSize = true,
            });
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "● Active", Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 133, 49), BackColor = Color.FromArgb(220, 252, 231),
                Location = new Point(160, y + 2), AutoSize = true, Padding = new Padding(4, 2, 4, 2),
            });
            y += 26;

            pnlRightScroll.Controls.Add(new Label
            {
                Text = "Manager อนุมัติ", Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62), Location = new Point(12, y), AutoSize = true,
            });
            y += 22;
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "Step 2 จาก 3", Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(110, 120, 140), Location = new Point(12, y), AutoSize = true,
            });
            y += 28;

            AddSeparator(y); y += 12;

            // Assignee avatar
            var avatarPanel = new Panel { Location = new Point(12, y), Size = new Size(36, 36), BackColor = Color.Transparent };
            avatarPanel.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(Color.FromArgb(59, 91, 219)))
                    pe.Graphics.FillEllipse(br, 0, 0, 35, 35);
                using (var f = new Font("Segoe UI", 12F, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    pe.Graphics.DrawString("ว", f, Brushes.White, new RectangleF(0, 0, 35, 35), sf);
            };
            pnlRightScroll.Controls.Add(avatarPanel);
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "วัชัย มีสุข", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62), Location = new Point(56, y + 4), AutoSize = true,
            });
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "Manager · Legal", Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(110, 120, 140), Location = new Point(56, y + 22), AutoSize = true,
            });
            y += 52;

            // กำหนด + เหลือ
            AddInfoRow("กำหนด", "20/04/2569 17:00", y, false); y += 22;
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "เหลือ", Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(110, 120, 140), Location = new Point(12, y), AutoSize = true,
            });
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "● 3 วัน", Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(197, 120, 16), BackColor = Color.FromArgb(254, 243, 199),
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
            barBg.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(117, 8), BackColor = Color.FromArgb(197, 120, 16) });
            pnlRightScroll.Controls.Add(barBg);
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "65%", Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(197, 120, 16), Location = new Point(198, y + 14), AutoSize = true,
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
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "เริ่ม → ส่งคำขอ → CRAF Check → Manager อนุมัติ → แจ้งผล → สิ้นสุด",
                Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(55, 65, 90),
                Location = new Point(12, y), Size = new Size(236, 52),
            });
            y += 60;
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "Legal ตรวจสอบ — Skipped (เนื่องจาก CRAF = 3NC)",
                Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(140, 150, 165),
                Location = new Point(12, y), Size = new Size(236, 36),
            });
            y += 48;

            AddSeparator(y); y += 12;

            // ACTIONS
            pnlRightScroll.Controls.Add(new Label
            {
                Text = "ACTIONS", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 36, 55), Location = new Point(12, y), AutoSize = true,
            });
            y += 22;

            var btnExecute = new Button
            {
                Text = "  ✓  ดำเนินการ Step นี้",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                Location = new Point(12, y), Size = new Size(236, 32),
            };
            btnExecute.FlatAppearance.BorderSize = 0;
            pnlRightScroll.Controls.Add(btnExecute);
            y += 38;

            var btnReassign = new Button
            {
                Text = "  ⇄  Reassign",
                Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(44, 62, 100),
                FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                Location = new Point(12, y), Size = new Size(236, 30),
            };
            btnReassign.FlatAppearance.BorderColor = Color.FromArgb(210, 215, 225);
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
            pnlRightScroll.Controls.Add(btnComment);
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
            btnZoomIn.Click  += (s, e) => { _zoomPercent = Math.Min(200, _zoomPercent + 10); lblZoomLevel.Text = _zoomPercent + "%"; pnlCanvas.Invalidate(); };
            btnZoomOut.Click += (s, e) => { _zoomPercent = Math.Max(25,  _zoomPercent - 10); lblZoomLevel.Text = _zoomPercent + "%"; pnlCanvas.Invalidate(); };
            btnExportPng.Click += (s, e) => ShowInfo("Export PNG: ยังไม่ได้ Implement");
        }

        private void PnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            float scale = _zoomPercent / 100f;
            g.ScaleTransform(scale, scale);
            DrawGrid(g);
            DrawSampleFlow(g);
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

        private void DrawSampleFlow(Graphics g)
        {
            // Nodes: X, Y, W, H, Label, Sub, Color, isActive
            DrawFlowNode(g, 60,  200, 80,  44, "เริ่ม",           "",                    Color.FromArgb(34, 153, 84),  false);
            DrawFlowNode(g, 200, 200, 110, 44, "ส่งคำขอ",         "สมชาย · 15/04 09:30", Color.FromArgb(34, 153, 84),  false);
            DrawFlowNode(g, 370, 200, 80,  44, "CRAFT",            "3NC",                 Color.FromArgb(34, 153, 84),  false);
            DrawFlowNode(g, 510, 160, 120, 44, "Manager อนุมัติ",  "วัชัย · 1 วัน",       Color.FromArgb(230, 115, 0),  true);
            DrawFlowNode(g, 510, 240, 120, 44, "Peer Review",      "Parallel · Required", Color.FromArgb(107, 114, 128),false);
            DrawFlowNode(g, 700, 200, 80,  44, "แจ้งผล",           "",                    Color.FromArgb(107, 114, 128),false);
            DrawFlowNode(g, 840, 200, 80,  44, "สิ้นสุด",          "",                    Color.FromArgb(185, 40, 40),  false);

            // Arrows
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

            // Shadow
            using (var shadow = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
                g.FillPath(shadow, RoundedRect(new Rectangle(x + 3, y + 3, w, h), 8));

            // Fill
            using (var br = new SolidBrush(isActive ? Color.FromArgb(255, 247, 235) : Color.White))
                g.FillPath(br, RoundedRect(rect, 8));

            // Top accent strip
            using (var abr = new SolidBrush(color))
            {
                var ap = new GraphicsPath();
                ap.AddArc(x, y, 16, 16, 180, 90);
                ap.AddArc(x + w - 16, y, 16, 16, 270, 90);
                ap.AddLine(x + w, y + 5, x, y + 5);
                ap.CloseFigure();
                g.FillPath(abr, ap);
            }

            // Border
            using (var pen = new Pen(isActive ? Color.FromArgb(230, 115, 0) : Color.FromArgb(210, 215, 225), isActive ? 2f : 1f))
                g.DrawPath(pen, RoundedRect(rect, 8));

            // Selection glow for active
            if (isActive)
            {
                using (var glowPen = new Pen(Color.FromArgb(80, 230, 115, 0), 4f))
                    g.DrawPath(glowPen, RoundedRect(new Rectangle(x - 2, y - 2, w + 4, h + 4), 10));
            }

            // Label
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            using (var f  = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var br = new SolidBrush(Color.FromArgb(30, 40, 65)))
                g.DrawString(label, f, br, new RectangleF(x + 4, y + 6, w - 8, h - 14), sf);

            // Sub-label
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
                null, // templateService
                null, // slaCalculator
                _loggingService,
                _userContext);
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
