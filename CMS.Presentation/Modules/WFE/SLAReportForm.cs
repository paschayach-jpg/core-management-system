using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class SLAReportForm : Form, ISLAReportView
    {
        // ── Dependencies ─────────────────────────────────────────────────────────
        private readonly SLAReportPresenter _presenter;
        private readonly UserContext _userContext;

        // ── Chart data ───────────────────────────────────────────────────────────
        private List<SLATrendData> _trendData    = new List<SLATrendData>();
        private List<CycleTimeBar> _cycleTimeBars = new List<CycleTimeBar>();

        // ── Template list UI refs ────────────────────────────────────────────────
        private Panel _pnlTemplateListContent;
        private Button _btnPeriod30;

        // ── Trend/cycle chart panel refs ─────────────────────────────────────────
        private Panel _pnlTrendContent;
        private Panel _pnlCycleContent;
        private Panel _pnlOverdueContent;

        // ── Colors ───────────────────────────────────────────────────────────────
        private readonly Color _navyDark   = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover   = Color.FromArgb(245, 249, 255);
        private readonly Color _clrGreen   = Color.FromArgb(23,  133, 49);
        private readonly Color _clrAmber   = Color.FromArgb(197, 120, 16);
        private readonly Color _clrRed     = Color.FromArgb(194, 38,  54);
        private readonly Color _clrBlue    = Color.FromArgb(25,  100, 195);

        private static readonly Color[] AvatarColors =
        {
            Color.FromArgb(0,  102, 180), Color.FromArgb(155, 89,  182),
            Color.FromArgb(22, 160, 133), Color.FromArgb(192, 57,  43),
            Color.FromArgb(41, 128, 185), Color.FromArgb(230, 126, 34),
        };

        // ── Overdue mock data ────────────────────────────────────────────────────
        private struct OverdueRow { public string Name; public string Dept; public int Count; public double AvgDays; }
        private static readonly OverdueRow[] OverdueData =
        {
            new OverdueRow { Name="วิชัย ม.",   Dept="Legal", Count=5, AvgDays=2.1 },
            new OverdueRow { Name="ปริดา ส.",   Dept="Legal", Count=3, AvgDays=1.4 },
            new OverdueRow { Name="ธนพล ภ.",    Dept="IT",    Count=2, AvgDays=3.2 },
            new OverdueRow { Name="สุนิสา ร.", Dept="QA",    Count=2, AvgDays=1.1 },
        };

        // ── Nav ──────────────────────────────────────────────────────────────────
        private Panel _activeNavItem;

        public SLAReportForm(
            IWorkflowService workflowService,
            ISLACalculator slaCalculator,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _userContext = userContext ?? throw new ArgumentNullException("userContext");
            _presenter = new SLAReportPresenter(
                this, workflowService, slaCalculator, loggingService, userContext);
        }

        private void SLAReportForm_Load(object sender, EventArgs e)
        {
            BuildRibbonTabs();
            BuildToolbar();
            Panel initialNav;
            WorkflowNavigationBuilder.BuildSidebar(
                pnlSidebarContainer, "sla", OnNavItemClick, out initialNav);
            _activeNavItem = initialNav;

            BuildTemplateListPanel();
            BuildTrendChartPanel();
            BuildOverduePanel();
            BuildCycleChartPanel();

            _presenter.GenerateReport();
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
                new { Text = "มุมมอง",     Active = false },
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
                    Font      = t.Active ? new Font("Segoe UI", 9.5F, FontStyle.Bold)
                                         : new Font("Segoe UI", 9.5F),
                    Cursor    = Cursors.Hand,
                    TabStop   = false,
                };
                btn.FlatAppearance.BorderSize            = 0;
                btn.FlatAppearance.MouseOverBackColor    = Color.FromArgb(50, 80, 130);
                btn.FlatAppearance.MouseDownBackColor    = Color.FromArgb(40, 70, 120);

                if (t.Active)
                    btn.Paint += (s, pe) => pe.Graphics.FillRectangle(
                        Brushes.White, 0, btn.Height - 3, btn.Width, 3);

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
                new { Header = "รายงาน", Items = new[]
                {
                    new { Icon = "📊", Text = "SLA",        Tag = "rpt_sla",    Active = true  },
                    new { Icon = "📈", Text = "Volume",     Tag = "rpt_volume", Active = false },
                    new { Icon = "⏱", Text = "Cycle Time", Tag = "rpt_cycle",  Active = false },
                }},
                new { Header = "การจัดการ", Items = new[]
                {
                    new { Icon = "🗓", Text = "ช่วงเวลา", Tag = "period",  Active = false },
                    new { Icon = "▼", Text = "ตัวกรอง",  Tag = "filter",  Active = false },
                    new { Icon = "↺", Text = "รีเฟรช",   Tag = "refresh", Active = false },
                }},
                new { Header = "ส่งออก", Items = new[]
                {
                    new { Icon = "📗", Text = "Excel",  Tag = "excel", Active = false },
                    new { Icon = "📕", Text = "PDF",    Tag = "pdf",   Active = false },
                    new { Icon = "🖨", Text = "พิมพ์", Tag = "print", Active = false },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int groupStartX = x;
                foreach (var item in group.Items)
                {
                    var tag    = item.Tag;
                    var active = item.Active;

                    var pnlBtn = new Panel
                    {
                        Location  = new Point(x, 2),
                        Size      = new Size(58, 60),
                        BackColor = active ? Color.FromArgb(232, 240, 255) : Color.White,
                        Cursor    = Cursors.Hand,
                        Tag       = tag,
                    };

                    var lblIcon = new Label
                    {
                        Text      = item.Icon,
                        Font      = new Font("Segoe UI", 16F),
                        ForeColor = active ? _navyDark : _navyDark,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location  = new Point(0, 6),
                        Size      = new Size(58, 30),
                        BackColor = Color.Transparent,
                    };
                    var lblText = new Label
                    {
                        Text      = item.Text,
                        Font      = active ? new Font("Segoe UI", 7.5F, FontStyle.Bold)
                                           : new Font("Segoe UI", 7.5F),
                        ForeColor = Color.FromArgb(55, 65, 90),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location  = new Point(0, 36),
                        Size      = new Size(58, 20),
                        BackColor = Color.Transparent,
                    };

                    pnlBtn.Controls.Add(lblIcon);
                    pnlBtn.Controls.Add(lblText);

                    var bgNormal = active ? Color.FromArgb(232, 240, 255) : Color.White;
                    EventHandler enter = (s, ev) =>
                    {
                        pnlBtn.BackColor = _navHover;
                        lblIcon.BackColor = lblText.BackColor = _navHover;
                    };
                    EventHandler leave = (s, ev) =>
                    {
                        pnlBtn.BackColor = bgNormal;
                        lblIcon.BackColor = lblText.BackColor = Color.Transparent;
                    };
                    EventHandler click = (s, ev) => OnToolbarClick(tag);

                    foreach (Control c in new Control[] { pnlBtn, lblIcon, lblText })
                    { c.MouseEnter += enter; c.MouseLeave += leave; c.Click += click; }

                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 60;
                }

                int groupW = x - groupStartX;
                pnlToolbarContainer.Controls.Add(new Label
                {
                    Text      = group.Header,
                    Font      = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point(groupStartX, 62),
                    Size      = new Size(groupW, 12),
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
                case "refresh": _presenter.GenerateReport(); break;
                case "excel":
                    var dlg = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", DefaultExt = "xlsx" };
                    if (dlg.ShowDialog() == DialogResult.OK) _presenter.ExportToExcel(dlg.FileName);
                    break;
                default:
                    MessageBox.Show(string.Format("ฟังก์ชัน '{0}' อยู่ระหว่างพัฒนา", tag),
                        "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // SIDEBAR
        // ════════════════════════════════════════════════════════════════════════

        private void OnNavItemClick(string tag)
        {
            if (tag != "sla")
                MessageBox.Show(string.Format("หน้า '{0}' อยู่ระหว่างพัฒนา", tag),
                    "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ════════════════════════════════════════════════════════════════════════
        // PANEL BUILDERS
        // ════════════════════════════════════════════════════════════════════════

        private void BuildTemplateListPanel()
        {
            // Header row
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = Color.White,
            };

            var lblSectionTitle = new Label
            {
                Text      = "SLA Compliance ต่อ Template",
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                Location  = new Point(12, 12),
                AutoSize  = true,
            };
            pnlHeader.Controls.Add(lblSectionTitle);

            // Period tabs
            var periods = new[]
            {
                new { Text = "30 วัน",  Active = true  },
                new { Text = "ไตรมาส", Active = false },
                new { Text = "ปี",      Active = false },
            };
            int tx = 220;
            foreach (var p in periods)
            {
                var isActive = p.Active;
                var btn = new Button
                {
                    Text      = p.Text,
                    Location  = new Point(tx, 10),
                    Size      = new Size(60, 26),
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 8.5F, isActive ? FontStyle.Bold : FontStyle.Regular),
                    BackColor = isActive ? _navyDark : Color.White,
                    ForeColor = isActive ? Color.White : Color.FromArgb(80, 90, 110),
                    Cursor    = Cursors.Hand,
                    TabStop   = false,
                };
                btn.FlatAppearance.BorderColor = isActive ? _navyDark : Color.FromArgb(210, 215, 225);
                btn.FlatAppearance.MouseOverBackColor = isActive ? _navyDark : _navHover;
                pnlHeader.Controls.Add(btn);
                if (isActive) _btnPeriod30 = btn;
                tx += 62;
            }

            // Bottom border
            pnlHeader.Controls.Add(new Panel
                { BackColor = Color.FromArgb(220, 224, 232), Dock = DockStyle.Bottom, Height = 1 });

            // Content area (populated by DisplayMetrics)
            _pnlTemplateListContent = new Panel
            {
                Dock        = DockStyle.Fill,
                BackColor   = Color.White,
                AutoScroll  = true,
            };

            // Footnote
            var pnlNote = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 28,
                BackColor = Color.FromArgb(250, 251, 253),
            };
            pnlNote.Controls.Add(new Label
            {
                Text      = "ⓘ  เป้าหมาย 90% · 2 Templates ต่ำกว่าเป้า",
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(120, 130, 150),
                Location  = new Point(12, 6),
                AutoSize  = true,
            });

            pnlTemplateListOuter.Controls.Add(_pnlTemplateListContent);
            pnlTemplateListOuter.Controls.Add(pnlNote);
            pnlTemplateListOuter.Controls.Add(pnlHeader);
        }

        private void BuildTrendChartPanel()
        {
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = Color.White,
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "Trend รายสัปดาห์ — SLA%",
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                Location  = new Point(12, 12),
                AutoSize  = true,
            });
            // YoY badge
            var lblYoy = new Label
            {
                Text      = "+17 pts YoY",
                Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = _clrGreen,
                BackColor = Color.FromArgb(220, 248, 228),
                Location  = new Point(230, 12),
                Size      = new Size(84, 20),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            pnlHeader.Controls.Add(lblYoy);
            pnlHeader.Controls.Add(new Panel
                { BackColor = Color.FromArgb(220, 224, 232), Dock = DockStyle.Bottom, Height = 1 });

            _pnlTrendContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
            };
            _pnlTrendContent.Paint += PnlTrendContent_Paint;

            pnlTrendChartOuter.Controls.Add(_pnlTrendContent);
            pnlTrendChartOuter.Controls.Add(pnlHeader);
        }

        private void BuildOverduePanel()
        {
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = Color.White,
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "เกิน SLA — รายบุคคล",
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                Location  = new Point(12, 12),
                AutoSize  = true,
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = "Top 5 เดือนนี้",
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(120, 130, 150),
                Location  = new Point(200, 14),
                AutoSize  = true,
            });
            pnlHeader.Controls.Add(new Panel
                { BackColor = Color.FromArgb(220, 224, 232), Dock = DockStyle.Bottom, Height = 1 });

            // Column headers
            var pnlColHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 28,
                BackColor = Color.FromArgb(246, 248, 251),
            };
            pnlColHeader.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                var hTexts = new[] { "ผู้รับผิดชอบ", "หน่วยงาน", "จำนวน", "เฉลี่ยล่าช้า" };
                var hXPos  = new[] { 12, 200, 290, 340 };
                using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                using (var b = new SolidBrush(Color.FromArgb(55, 65, 90)))
                    for (int hi = 0; hi < hTexts.Length; hi++)
                        g.DrawString(hTexts[hi], f, b, hXPos[hi], 6);
                g.DrawLine(new Pen(Color.FromArgb(220, 224, 232)),
                    0, pnlColHeader.Height - 1, pnlColHeader.Width, pnlColHeader.Height - 1);
            };

            _pnlOverdueContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
            };
            _pnlOverdueContent.Paint += PnlOverdueContent_Paint;

            pnlOverdueOuter.Controls.Add(_pnlOverdueContent);
            pnlOverdueOuter.Controls.Add(pnlColHeader);
            pnlOverdueOuter.Controls.Add(pnlHeader);
        }

        private void BuildCycleChartPanel()
        {
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = Color.White,
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "Cycle Time Distribution",
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                Location  = new Point(12, 12),
                AutoSize  = true,
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = "เฉลี่ย 2.8 วัน · Median 2.1",
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(120, 130, 150),
                Location  = new Point(200, 14),
                AutoSize  = true,
            });
            pnlHeader.Controls.Add(new Panel
                { BackColor = Color.FromArgb(220, 224, 232), Dock = DockStyle.Bottom, Height = 1 });

            _pnlCycleContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
            };
            _pnlCycleContent.Paint += PnlCycleContent_Paint;

            pnlCycleChartOuter.Controls.Add(_pnlCycleContent);
            pnlCycleChartOuter.Controls.Add(pnlHeader);
        }

        // ════════════════════════════════════════════════════════════════════════
        // ISLAEREPORTVIEW IMPLEMENTATION
        // ════════════════════════════════════════════════════════════════════════

        public void DisplayMetrics(List<SLAMetricRow> metrics)
        {
            if (InvokeRequired) { Invoke(new Action(() => DisplayMetrics(metrics))); return; }

            _pnlTemplateListContent.Controls.Clear();

            int y = 4;
            foreach (var row in metrics)
            {
                var metric = row;
                var pnlRow = new Panel
                {
                    Location  = new Point(0, y),
                    Size      = new Size(_pnlTemplateListContent.ClientSize.Width, 40),
                    Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    BackColor = Color.White,
                };

                // Template code + name
                pnlRow.Controls.Add(new Label
                {
                    Text      = metric.TemplateCode + " · " + metric.TemplateName,
                    Font      = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(30, 36, 55),
                    Location  = new Point(12, 6),
                    Size      = new Size(160, 20),
                    AutoEllipsis = true,
                });

                // Progress bar background
                var barBg = new Panel
                {
                    Location  = new Point(178, 14),
                    Size      = new Size(160, 8),
                    BackColor = Color.FromArgb(228, 232, 240),
                };
                pnlRow.Controls.Add(barBg);

                // Progress bar fill
                Color barColor = metric.CompliancePercent >= 90 ? _clrGreen
                               : metric.CompliancePercent >= 85 ? _clrAmber
                               :                                   _clrRed;

                int fillW = (int)(160 * Math.Min(1.0, metric.CompliancePercent / 100.0));
                var barFill = new Panel
                {
                    Location  = new Point(0, 0),
                    Size      = new Size(fillW, 8),
                    BackColor = barColor,
                };
                barBg.Controls.Add(barFill);

                // Percent label
                pnlRow.Controls.Add(new Label
                {
                    Text      = string.Format("{0}%", (int)metric.CompliancePercent),
                    Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = barColor,
                    Location  = new Point(344, 4),
                    Size      = new Size(38, 20),
                    TextAlign = ContentAlignment.MiddleRight,
                });

                // n= count
                pnlRow.Controls.Add(new Label
                {
                    Text      = string.Format("n={0}", metric.Total),
                    Font      = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    Location  = new Point(386, 4),
                    Size      = new Size(38, 20),
                    TextAlign = ContentAlignment.MiddleLeft,
                });

                // Status badge
                string badge = metric.CompliancePercent >= 90 ? "ผ่าน"
                             : metric.CompliancePercent >= 85 ? "ใกล้"
                             :                                   "ต่ำ";
                var lblBadge = new Label
                {
                    Text      = badge,
                    Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = barColor,
                    BackColor = Color.FromArgb(30, barColor.R, barColor.G, barColor.B),
                    Location  = new Point(428, 6),
                    Size      = new Size(36, 18),
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                pnlRow.Controls.Add(lblBadge);

                // Row separator
                var sep = new Panel
                {
                    BackColor = Color.FromArgb(235, 238, 244),
                    Location  = new Point(0, 39),
                    Size      = new Size(700, 1),
                    Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                };
                pnlRow.Controls.Add(sep);

                _pnlTemplateListContent.Controls.Add(pnlRow);
                y += 40;
            }

            // Update KPI card overdue from metrics
            if (metrics.Count > 0)
            {
                int totalOverdue = metrics.Sum(m => m.Overdue);
                int total        = metrics.Sum(m => m.Total);
                lblCardOverdueCount.Text = string.Format("{0} / {1}", totalOverdue, total);
                if (total > 0)
                    lblCardOverdueSub.Text = string.Format("{0:F1}% ของทั้งหมด",
                        (double)totalOverdue / total * 100);

                double avgCompliance = metrics.Average(m => m.CompliancePercent);
                lblCardComplianceCount.Text = string.Format("{0:F1}%", avgCompliance);
            }
        }

        public void DisplayKpiSummary(SlaKpiSummary summary)
        {
            if (InvokeRequired) { Invoke(new Action(() => DisplayKpiSummary(summary))); return; }

            lblCardComplianceCount.Text = string.Format("{0:F1}%", summary.CompliancePercent);
            lblCardComplianceSub.Text   = summary.ComplianceDelta;
            lblCardCycleTimeCount.Text  = string.Format("{0:F1} วัน", summary.AvgCycleTimeDays);
            lblCardCycleTimeSub.Text    = summary.CycleTimeDelta;
            lblCardOverdueCount.Text    = string.Format("{0} / {1}", summary.OverdueCount, summary.TotalCount);
            if (summary.TotalCount > 0)
                lblCardOverdueSub.Text  = string.Format("{0:F1}% ของทั้งหมด",
                    (double)summary.OverdueCount / summary.TotalCount * 100);
            lblCardCompletedCount.Text  = summary.CompletedMtd.ToString();
            lblCardCompletedSub.Text    = summary.PeriodLabel;
        }

        public void DisplayComplianceTrendChart(List<SLATrendData> trendData)
        {
            if (InvokeRequired) { Invoke(new Action(() => DisplayComplianceTrendChart(trendData))); return; }
            _trendData = trendData ?? new List<SLATrendData>();
            _pnlTrendContent?.Invalidate();
        }

        public void DisplayCycleTimeDistribution(List<CycleTimeBar> bars)
        {
            if (InvokeRequired) { Invoke(new Action(() => DisplayCycleTimeDistribution(bars))); return; }
            _cycleTimeBars = bars ?? new List<CycleTimeBar>();
            _pnlCycleContent?.Invalidate();
        }

        public void DisplayStatusDistributionChart(List<SLADistributionData> distributionData)
        {
            // Legacy — superseded by DisplayCycleTimeDistribution
        }

        public void ExportToExcel(string filePath)
        {
            MessageBox.Show("Export ไปยัง " + filePath + "\n(ฟังก์ชันนี้อยู่ระหว่างพัฒนา)",
                "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowError(string message) =>
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowInfo(string message) =>
            MessageBox.Show(message, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public void GetDateRange(out DateTime fromDate, out DateTime toDate)
        {
            // TODO: wire up date picker controls
            fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            toDate   = DateTime.Now;
        }

        public int? GetTemplateFilter() => null;
        public string GetUserFilter()   => string.Empty;

        // ════════════════════════════════════════════════════════════════════════
        // CHART PAINT HANDLERS
        // ════════════════════════════════════════════════════════════════════════

        private void PnlTrendContent_Paint(object sender, PaintEventArgs e)
        {
            var g   = e.Graphics;
            var pnl = (Panel)sender;
            g.SmoothingMode        = SmoothingMode.AntiAlias;
            g.TextRenderingHint    = TextRenderingHint.ClearTypeGridFit;

            int padL = 44, padR = 20, padT = 16, padB = 32;
            int chartW = pnl.Width  - padL - padR;
            int chartH = pnl.Height - padT - padB;

            if (chartW <= 0 || chartH <= 0) return;

            double yMin = 60, yMax = 105;

            // Grid lines + Y labels
            var gridPen = new Pen(Color.FromArgb(235, 238, 244));
            using (var fSmall = new Font("Segoe UI", 7.5F))
            using (var bMuted = new SolidBrush(Color.FromArgb(140, 150, 165)))
            {
                foreach (var pct in new[] { 60, 70, 80, 90, 100 })
                {
                    int y = padT + chartH - (int)((pct - yMin) / (yMax - yMin) * chartH);
                    g.DrawLine(gridPen, padL, y, padL + chartW, y);
                    g.DrawString(pct + "%", fSmall, bMuted, 0, y - 8);
                }
            }

            // Dashed target line at 90%
            int targetY = padT + chartH - (int)((90 - yMin) / (yMax - yMin) * chartH);
            using (var dashPen = new Pen(Color.FromArgb(197, 120, 16), 1.2F))
            {
                dashPen.DashStyle = DashStyle.Dash;
                g.DrawLine(dashPen, padL, targetY, padL + chartW, targetY);
            }

            if (_trendData == null || _trendData.Count < 2)
            {
                gridPen.Dispose();
                return;
            }

            int n = _trendData.Count;
            float stepX = (float)chartW / (n - 1);

            // Compute points
            var pts = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                float px = padL + i * stepX;
                float py = padT + chartH - (float)((_trendData[i].CompliancePercent - yMin) / (yMax - yMin) * chartH);
                pts[i] = new PointF(px, py);
            }

            // Shaded area under curve
            var fillPts = new PointF[n + 2];
            fillPts[0] = new PointF(pts[0].X, padT + chartH);
            for (int i = 0; i < n; i++) fillPts[i + 1] = pts[i];
            fillPts[n + 1] = new PointF(pts[n - 1].X, padT + chartH);
            using (var fillBrush = new SolidBrush(Color.FromArgb(30, 25, 100, 195)))
                g.FillPolygon(fillBrush, fillPts);

            // Line
            using (var linePen = new Pen(_clrBlue, 2.2F))
                g.DrawLines(linePen, pts);

            // Dots + X labels
            using (var fSmall = new Font("Segoe UI", 7.5F))
            using (var bMuted = new SolidBrush(Color.FromArgb(140, 150, 165)))
            {
                for (int i = 0; i < n; i++)
                {
                    g.FillEllipse(Brushes.White, pts[i].X - 4, pts[i].Y - 4, 8, 8);
                    g.FillEllipse(new SolidBrush(_clrBlue), pts[i].X - 2.5f, pts[i].Y - 2.5f, 5, 5);

                    // Show every other X label to avoid overlap
                    if (i % 2 == 0 || i == n - 1)
                        g.DrawString(_trendData[i].WeekLabel, fSmall, bMuted,
                            pts[i].X - 10, padT + chartH + 4);
                }
            }

            // "Now: 91%" annotation near last point
            var lastPt = pts[n - 1];
            using (var fAnno = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var bAnno = new SolidBrush(_clrBlue))
                g.DrawString(string.Format("Now: {0:F0}%", _trendData[n - 1].CompliancePercent),
                    fAnno, bAnno, lastPt.X - 42, lastPt.Y - 18);

            gridPen.Dispose();
        }

        private void PnlCycleContent_Paint(object sender, PaintEventArgs e)
        {
            var g   = e.Graphics;
            var pnl = (Panel)sender;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            if (_cycleTimeBars == null || _cycleTimeBars.Count == 0) return;

            int padL = 8, padR = 8, padT = 10, padB = 38;
            int chartW = pnl.Width  - padL - padR;
            int chartH = pnl.Height - padT - padB;

            if (chartW <= 0 || chartH <= 0) return;

            int barCount = _cycleTimeBars.Count;
            int maxCount = _cycleTimeBars.Max(b => b.Count);
            float barW   = (float)chartW / barCount;
            float gap     = barW * 0.18f;
            float bW      = barW - gap * 2;

            using (var fSmall = new Font("Segoe UI", 7.5F))
            using (var fCount = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var bMuted = new SolidBrush(Color.FromArgb(140, 150, 165)))
            {
                for (int i = 0; i < barCount; i++)
                {
                    var bar   = _cycleTimeBars[i];
                    Color c   = bar.Category == "pass" ? _clrGreen
                              : bar.Category == "near" ? _clrAmber
                              :                          _clrRed;

                    float bH = maxCount > 0 ? (float)bar.Count / maxCount * chartH : 0;
                    float bX = padL + i * barW + gap;
                    float bY = padT + chartH - bH;

                    using (var br = new SolidBrush(c))
                        g.FillRectangle(br, bX, bY, bW, bH);

                    // Count label on top
                    if (bH > 14)
                        g.DrawString(bar.Count.ToString(), fCount, Brushes.White,
                            bX + (bW - g.MeasureString(bar.Count.ToString(), fCount).Width) / 2,
                            bY + 2);

                    // X-axis label
                    float labelX = bX + (bW - g.MeasureString(bar.Label, fSmall).Width) / 2;
                    g.DrawString(bar.Label, fSmall, bMuted, labelX, padT + chartH + 4);
                }
            }

            // Legend
            int lx = padL;
            int ly = padT + chartH + 20;
            var legendLabels = new[] { "ตาม SLA", "ใกล้ SLA", "เกิน" };
            var legendColors = new[] { _clrGreen,  _clrAmber,  _clrRed };
            using (var fLeg = new Font("Segoe UI", 7.5F))
            using (var bLabel = new SolidBrush(Color.FromArgb(80, 90, 110)))
            {
                for (int li = 0; li < legendLabels.Length; li++)
                {
                    g.FillRectangle(new SolidBrush(legendColors[li]), lx, ly + 2, 10, 10);
                    g.DrawString(legendLabels[li], fLeg, bLabel, lx + 13, ly);
                    lx += (int)g.MeasureString(legendLabels[li], fLeg).Width + 24;
                }
            }
        }

        private void PnlOverdueContent_Paint(object sender, PaintEventArgs e)
        {
            var g   = e.Graphics;
            var pnl = (Panel)sender;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int rowH = 44;
            int y    = 4;

            for (int i = 0; i < OverdueData.Length; i++)
            {
                var row = OverdueData[i];

                // Alternate background
                if (i % 2 == 1)
                    g.FillRectangle(new SolidBrush(Color.FromArgb(250, 251, 253)),
                        0, y, pnl.Width, rowH);

                // Avatar circle
                int avSz = 28;
                int avX  = 12;
                int avY  = y + (rowH - avSz) / 2;
                var avColor = AvatarColors[Math.Abs(row.Name.GetHashCode()) % AvatarColors.Length];
                using (var avBrush = new SolidBrush(avColor))
                using (var avPath = RoundedRect(new Rectangle(avX, avY, avSz, avSz), avSz / 2))
                    g.FillPath(avBrush, avPath);

                var initials = GetInitials(row.Name);
                g.DrawString(initials, new Font("Segoe UI", 9F, FontStyle.Bold), Brushes.White,
                    new RectangleF(avX, avY, avSz, avSz),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                // Name
                using (var fName = new Font("Segoe UI", 9F))
                    g.DrawString(row.Name, fName, new SolidBrush(Color.FromArgb(30, 36, 55)),
                        avX + avSz + 6, y + 12);

                // Dept badge
                int deptX = 200;
                Color deptColor = row.Dept == "Legal" ? Color.FromArgb(80, 130, 200)
                                : row.Dept == "IT"    ? Color.FromArgb(22, 160, 133)
                                : row.Dept == "QA"    ? Color.FromArgb(155, 89, 182)
                                :                       Color.FromArgb(120, 130, 150);
                using (var fDept = new Font("Segoe UI", 8F))
                {
                    g.DrawString("● " + row.Dept, fDept,
                        new SolidBrush(deptColor), deptX, y + 14);
                }

                // Count
                using (var fCount = new Font("Segoe UI", 9F, FontStyle.Bold))
                    g.DrawString(row.Count.ToString(), fCount,
                        new SolidBrush(Color.FromArgb(30, 36, 55)), 295, y + 13);

                // Avg days
                Color dayColor = row.AvgDays > 2.0 ? _clrRed : _clrAmber;
                using (var fDays = new Font("Segoe UI", 9F, FontStyle.Bold))
                    g.DrawString(string.Format("{0:F1} วัน", row.AvgDays), fDays,
                        new SolidBrush(dayColor), 340, y + 13);

                // Row separator
                g.DrawLine(new Pen(Color.FromArgb(235, 238, 244)),
                    0, y + rowH - 1, pnl.Width, y + rowH - 1);

                y += rowH;
            }
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
            return parts.Length >= 2
                ? parts[0].Substring(0, 1) + parts[1].Substring(0, 1)
                : name.Substring(0, Math.Min(2, name.Length));
        }
    }
}
