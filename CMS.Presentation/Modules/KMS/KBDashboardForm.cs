// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// KMS Dashboard & Reports — แสดงสถิติ, รายงาน, และ KPI ของระบบ Knowledge Base
    /// Redesigned to match MOD-006/kms_dashboard.html
    /// </summary>
    public partial class KBDashboardForm : Form, IKBDashboardController
    {
        private KBDashboardController _presenter;
        private readonly UserContext _user;

        // Dynamically created controls
        private Label lblTotalArticles, lblTotalArticlesSub;
        private Label lblMonthlyViews, lblMonthlyViewsSub;
        private Label lblAvgRating, lblAvgRatingSub;
        private Label lblPublished, lblPublishedSub;
        private Label lblOverdueCount, lblDraftCount, lblExpiringCount, lblArchivedCount;

        // ── Constructor ─────────────────────────────────────────────────────────
        public KBDashboardForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
            BuildDashboardLayout();
        }

        /// <summary>
        /// Called by the composition root after the presenter is fully wired.
        /// Must be called before ShowDialog().
        /// </summary>
        public void AttachPresenter(KBDashboardController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form Load ───────────────────────────────────────────────────────────
        private void KBDashboardForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            // Status bar: user info
            lblStatusUser.Text = string.Format("👤  {0}  |  📅  {1}",
                _user.AdIdentity ?? "ผู้ใช้งาน",
                DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            // Initialize tabs
            ShowDashboardTab();
            
            // Build approval queue layout
            BuildApprovalQueueLayout();
            
            // Load data
            _presenter.Initialize();
        }

        // ── Build Dashboard Layout ──────────────────────────────────────────────
        private void BuildDashboardLayout()
        {
            pnlDashboard.SuspendLayout();
            pnlDashboard.Controls.Clear();
            pnlDashboard.AutoScroll = true;
            pnlDashboard.Padding = new Padding(16, 14, 16, 14);

            int yPos = 0;

            // 1. KPI Cards Row
            var pnlKPIRow = BuildKPIRow();
            pnlKPIRow.Location = new Point(0, yPos);
            pnlDashboard.Controls.Add(pnlKPIRow);
            yPos += pnlKPIRow.Height + 14;

            // 2. Charts Row (Monthly Trend + Top Articles)
            var pnlChartsRow = BuildChartsRow();
            pnlChartsRow.Location = new Point(0, yPos);
            pnlDashboard.Controls.Add(pnlChartsRow);
            yPos += pnlChartsRow.Height + 12;

            // 3. Alerts Row
            var pnlAlertsRow = BuildAlertsRow();
            pnlAlertsRow.Location = new Point(0, yPos);
            pnlDashboard.Controls.Add(pnlAlertsRow);
            yPos += pnlAlertsRow.Height + 12;

            // 4. Type Report Row
            var pnlTypeRow = BuildTypeReportRow();
            pnlTypeRow.Location = new Point(0, yPos);
            pnlDashboard.Controls.Add(pnlTypeRow);

            pnlDashboard.ResumeLayout();
        }

        // ── Build KPI Cards Row ─────────────────────────────────────────────────
        private Panel BuildKPIRow()
        {
            var pnl = new Panel
            {
                Size = new Size(pnlDashboard.ClientSize.Width - 32, 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int cardWidth = (pnl.Width - 30) / 4;

            // Card 1: Total Articles (Blue)
            var card1 = CreateKPICard("📚", "บทความทั้งหมด", Color.FromArgb(46, 116, 181), 0, cardWidth);
            lblTotalArticles = (Label)card1.Controls["value"];
            lblTotalArticlesSub = (Label)card1.Controls["sub"];
            pnl.Controls.Add(card1);

            // Card 2: Monthly Views (Green)
            var card2 = CreateKPICard("📖", "Views เดือนนี้", Color.FromArgb(39, 174, 96), cardWidth + 10, cardWidth);
            lblMonthlyViews = (Label)card2.Controls["value"];
            lblMonthlyViewsSub = (Label)card2.Controls["sub"];
            pnl.Controls.Add(card2);

            // Card 3: Avg Rating (Amber)
            var card3 = CreateKPICard("⭐", "Avg. Rating ระบบ", Color.FromArgb(243, 156, 18), (cardWidth + 10) * 2, cardWidth);
            lblAvgRating = (Label)card3.Controls["value"];
            lblAvgRatingSub = (Label)card3.Controls["sub"];
            pnl.Controls.Add(card3);

            // Card 4: Published (Green)
            var card4 = CreateKPICard("📊", "บทความ Published", Color.FromArgb(39, 174, 96), (cardWidth + 10) * 3, cardWidth);
            lblPublished = (Label)card4.Controls["value"];
            lblPublishedSub = (Label)card4.Controls["sub"];
            pnl.Controls.Add(card4);

            return pnl;
        }

        private Panel CreateKPICard(string icon, string label, Color borderColor, int x, int width)
        {
            var card = new Panel
            {
                Location = new Point(x, 0),
                Size = new Size(width, 110),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Top border
            var topBorder = new Panel
            {
                Dock = DockStyle.Top,
                Height = 3,
                BackColor = borderColor
            };
            card.Controls.Add(topBorder);

            var lblIcon = new Label
            {
                Location = new Point(14, 12),
                Size = new Size(40, 30),
                Text = icon,
                Font = new Font("Segoe UI", 18f),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblIcon);

            var lblLabel = new Label
            {
                Location = new Point(14, 45),
                Size = new Size(width - 28, 18),
                Text = label,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(127, 140, 141),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblLabel);

            var lblValue = new Label
            {
                Name = "value",
                Location = new Point(14, 63),
                Size = new Size(width - 28, 32),
                Text = "0",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 35, 50),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblValue);

            var lblSub = new Label
            {
                Name = "sub",
                Location = new Point(14, 95),
                Size = new Size(width - 28, 14),
                Text = "-",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(127, 140, 141),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblSub);

            return card;
        }

        // ── Build Charts Row ────────────────────────────────────────────────────
        private Panel BuildChartsRow()
        {
            var pnl = new Panel
            {
                Size = new Size(pnlDashboard.ClientSize.Width - 32, 240),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int halfWidth = (pnl.Width - 12) / 2;

            // Left: Monthly Chart
            pnlChart = CreateDashCard("📈 บทความใหม่รายเดือน (6 เดือนล่าสุด)", "ปีงบประมาณ 2568-69", 0, halfWidth, 240);
            pnl.Controls.Add(pnlChart);

            // Right: Top Articles
            pnlTopArticles = CreateDashCard("🏆 Top 5 บทความยอดนิยม (เดือนนี้)", "เรียงตาม Views", halfWidth + 12, halfWidth, 240);
            pnl.Controls.Add(pnlTopArticles);

            return pnl;
        }

        private Panel CreateDashCard(string title, string subtitle, int x, int width, int height)
        {
            var card = new Panel
            {
                Location = new Point(x, 0),
                Size = new Size(width, height),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Header
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = Color.FromArgb(30, 58, 95)
            };

            var lblTitle = new Label
            {
                Location = new Point(12, 6),
                AutoSize = true,
                Text = title,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            header.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Location = new Point(width - 180, 6),
                Size = new Size(165, 16),
                Text = subtitle,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(214, 228, 240),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight
            };
            header.Controls.Add(lblSubtitle);

            card.Controls.Add(header);

            // Body
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.White,
                AutoScroll = true
            };
            card.Controls.Add(body);

            // Store body reference
            card.Tag = body;

            return card;
        }

        // ── Build Alerts Row ────────────────────────────────────────────────────
        private Panel BuildAlertsRow()
        {
            var pnl = new Panel
            {
                Size = new Size(pnlDashboard.ClientSize.Width - 32, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            pnlAlerts = CreateDashCard("⚠️ รายงานที่ต้องดำเนินการ", "Action Required", 0, pnl.Width, 200);
            pnlAlerts.BackColor = Color.White;
            
            // Change header color to red
            var header = pnlAlerts.Controls.OfType<Panel>().FirstOrDefault();
            if (header != null)
            {
                header.BackColor = Color.FromArgb(146, 43, 33);
            }

            pnl.Controls.Add(pnlAlerts);

            // Build alert items in the body
            var body = (Panel)pnlAlerts.Tag;
            body.Controls.Clear();

            int y = 0;
            
            // Alert 1: Overdue Reviews
            var alert1 = CreateAlertItem(Color.FromArgb(231, 76, 60), "บทความครบกำหนด Review", "รายการเก่าสุด: 20 มี.ค. 69 (เกิน 18 วัน)", y);
            lblOverdueCount = (Label)alert1.Controls["count"];
            body.Controls.Add(alert1);
            y += 42;

            // Alert 2: Draft Articles
            var alert2 = CreateAlertItem(Color.FromArgb(243, 156, 18), "Draft ค้างนานเกิน 30 วัน", "ผู้แต่ง: วิชัย สมใจ, อารีย์ แสงดี", y);
            lblDraftCount = (Label)alert2.Controls["count"];
            body.Controls.Add(alert2);
            y += 42;

            // Alert 3: Expiring Soon
            var alert3 = CreateAlertItem(Color.FromArgb(230, 126, 34), "Expiring ใน 30 วัน", "KB-2568-055, KB-2568-067", y);
            lblExpiringCount = (Label)alert3.Controls["count"];
            body.Controls.Add(alert3);
            y += 42;

            // Alert 4: Archived
            var alert4 = CreateAlertItem(Color.FromArgb(127, 140, 141), "Archived ล่าสุด", "เดือนมีนาคม 2569", y);
            lblArchivedCount = (Label)alert4.Controls["count"];
            body.Controls.Add(alert4);

            return pnl;
        }

        private Panel CreateAlertItem(Color dotColor, string text, string subtext, int y)
        {
            var item = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(pnlAlerts.Width - 50, 40),
                BackColor = Color.Transparent
            };

            // Dot
            var dot = new Panel
            {
                Location = new Point(0, 5),
                Size = new Size(10, 10),
                BackColor = dotColor
            };
            dot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(dotColor), 0, 0, 10, 10);
            };
            item.Controls.Add(dot);

            // Count label
            var lblCount = new Label
            {
                Name = "count",
                Location = new Point(18, 2),
                AutoSize = true,
                Text = "0 รายการ",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = dotColor
            };
            item.Controls.Add(lblCount);

            // Text
            var lblText = new Label
            {
                Location = new Point(90, 2),
                Size = new Size(300, 16),
                Text = " — " + text,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(26, 35, 50)
            };
            item.Controls.Add(lblText);

            // Subtext
            var lblSubtext = new Label
            {
                Location = new Point(18, 20),
                Size = new Size(500, 14),
                Text = subtext,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(127, 140, 141)
            };
            item.Controls.Add(lblSubtext);

            // Separator
            var sep = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(240, 244, 249)
            };
            item.Controls.Add(sep);

            return item;
        }

        // ── Build Type Report Row ───────────────────────────────────────────────
        private Panel BuildTypeReportRow()
        {
            var pnl = new Panel
            {
                Size = new Size(pnlDashboard.ClientSize.Width - 32, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var card = CreateDashCard("🗂️ รายงานแยกตามประเภทบทความ", "รวมทุกสถานะ ณ เมษายน 2569", 0, pnl.Width, 180);
            
            var body = (Panel)card.Tag;
            body.Controls.Clear();

            int cardWidth = (body.Width - 36) / 4;
            
            // Type cards
            body.Controls.Add(CreateTypeCard("📋 SOP", Color.FromArgb(231, 76, 60), 0, cardWidth));
            body.Controls.Add(CreateTypeCard("❓ FAQ", Color.FromArgb(183, 119, 13), cardWidth + 12, cardWidth));
            body.Controls.Add(CreateTypeCard("📖 แนวทาง", Color.FromArgb(46, 116, 181), (cardWidth + 12) * 2, cardWidth));
            body.Controls.Add(CreateTypeCard("📄 Template", Color.FromArgb(39, 174, 96), (cardWidth + 12) * 3, cardWidth));

            pnl.Controls.Add(card);
            return pnl;
        }

        private Panel CreateTypeCard(string title, Color borderColor, int x, int width)
        {
            var card = new Panel
            {
                Location = new Point(x, 0),
                Size = new Size(width, 130),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Top border
            var topBorder = new Panel
            {
                Dock = DockStyle.Top,
                Height = 3,
                BackColor = borderColor
            };
            card.Controls.Add(topBorder);

            var lblTitle = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(width - 20, 20),
                Text = title,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = borderColor,
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblTitle);

            // Stats
            var stats = new[]
            {
                "✅ Published: 0",
                "🔴 Draft: 0",
                "🟡 In Review: 0",
                "⚫ Archived: 0"
            };

            int y = 35;
            foreach (var stat in stats)
            {
                var lbl = new Label
                {
                    Location = new Point(10, y),
                    Size = new Size(width - 20, 16),
                    Text = stat,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(74, 85, 104)
                };
                card.Controls.Add(lbl);
                y += 18;
            }

            return card;
        }

        // ── Build Approval Queue Layout ─────────────────────────────────────────
        private void BuildApprovalQueueLayout()
        {
            // The approval queue will be built in the tabApproval page
            // We'll create mock approval cards for demonstration
            
            // Clear existing controls in tabApproval
            tabApproval.Controls.Clear();

            // Create toolbar
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(248, 250, 251),
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnRefreshApproval = new Button
            {
                Location = new Point(10, 6),
                Size = new Size(90, 24),
                Text = "🔄 รีเฟรช",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnRefreshApproval.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
            toolbar.Controls.Add(btnRefreshApproval);

            var btnExportApproval = new Button
            {
                Location = new Point(110, 6),
                Size = new Size(90, 24),
                Text = "📊 Export",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnExportApproval.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
            toolbar.Controls.Add(btnExportApproval);

            var btnNotification = new Button
            {
                Location = new Point(210, 6),
                Size = new Size(160, 24),
                Text = "⚙️ ตั้งค่า Notification",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnNotification.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
            toolbar.Controls.Add(btnNotification);

            // Summary pills
            var pillPending = CreatePill("📋 รอ Review: 5", Color.FromArgb(214, 234, 248), Color.FromArgb(26, 82, 118), 580, 6);
            toolbar.Controls.Add(pillPending);

            var pillOverdue = CreatePill("🔴 เกินกำหนด: 1", Color.FromArgb(250, 219, 216), Color.FromArgb(146, 43, 33), 720, 6);
            toolbar.Controls.Add(pillOverdue);

            var pillReturned = CreatePill("↩️ ส่งคืน: 2", Color.FromArgb(254, 249, 231), Color.FromArgb(183, 119, 13), 860, 6);
            toolbar.Controls.Add(pillReturned);

            tabApproval.Controls.Add(toolbar);

            // Create filter bar
            var filterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(245, 247, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblFilter = new Label
            {
                Location = new Point(12, 10),
                AutoSize = true,
                Text = "ฟิลเตอร์:",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(127, 140, 141)
            };
            filterBar.Controls.Add(lblFilter);

            int filterX = 70;
            var cboStatus = CreateFilterCombo("สถานะ ▼", filterX);
            filterBar.Controls.Add(cboStatus);
            filterX += 110;

            var cboType = CreateFilterCombo("ประเภท ▼", filterX);
            filterBar.Controls.Add(cboType);
            filterX += 110;

            var cboDept = CreateFilterCombo("แผนก ▼", filterX);
            filterBar.Controls.Add(cboDept);
            filterX += 110;

            var cboSender = CreateFilterCombo("ผู้ส่ง ▼", filterX);
            filterBar.Controls.Add(cboSender);
            filterX += 110;

            var btnClearFilter = new Button
            {
                Location = new Point(filterX, 6),
                Size = new Size(60, 24),
                Text = "ล้าง",
                Font = new Font("Segoe UI", 8.5f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnClearFilter.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
            filterBar.Controls.Add(btnClearFilter);

            var lblSort = new Label
            {
                Location = new Point(filterBar.Width - 280, 10),
                AutoSize = true,
                Text = "เรียงตาม:",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(127, 140, 141),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            filterBar.Controls.Add(lblSort);

            var cboSort = CreateFilterCombo("วันที่ส่ง — เก่าสุดก่อน ▼", filterBar.Width - 200);
            cboSort.Width = 190;
            cboSort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            filterBar.Controls.Add(cboSort);

            tabApproval.Controls.Add(filterBar);

            // Create scroll panel for approval cards
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White
            };

            // Add mock approval cards
            int yPos = 0;
            
            // Card 1: Overdue
            var card1 = CreateApprovalCard(
                "overdue",
                "🔴 เกินกำหนด",
                "FAQ: วิธีการเคลมรถยนต์",
                "⚠️ SLA เกิน 2 วัน",
                "KB-2569-015", "FAQ", "วิชัย สมใจ", "01/04/2569",
                "7 วัน", "Claims",
                new[] { "🏷️ Motor", "🏷️ Claims", "🏷️ FAQ" },
                yPos
            );
            scrollPanel.Controls.Add(card1);
            yPos += card1.Height + 8;

            // Card 2: Urgent
            var card2 = CreateApprovalCard(
                "urgent",
                "🟠 เร่งด่วน",
                "SOP: แนวปฏิบัติการรับประกันภัยรถยนต์ v2",
                "SLA เหลือ: 1 วัน",
                "KB-2569-018", "SOP", "อารีย์ แสงดี", "03/04/2569",
                "5 วัน", "UW",
                new[] { "🏷️ Motor", "🏷️ UW" },
                yPos
            );
            scrollPanel.Controls.Add(card2);
            yPos += card2.Height + 8;

            // Card 3: Normal
            var card3 = CreateApprovalCard(
                "normal",
                "🟡 รอ Review",
                "แนวทาง: คู่มือการประเมินความเสี่ยง Marine",
                "SLA เหลือ: 2 วัน",
                "KB-2569-020", "แนวทาง", "สมชาย กิจการ", "04/04/2569",
                "4 วัน", "UW",
                new[] { "🏷️ Marine", "🏷️ Risk" },
                yPos
            );
            scrollPanel.Controls.Add(card3);
            yPos += card3.Height + 8;

            // Card 4: Normal
            var card4 = CreateApprovalCard(
                "normal",
                "🟡 รอ Review",
                "Wiki: คู่มือการใช้งาน RISC Claim สำหรับ Claims Officer",
                "SLA เหลือ: 3 วัน",
                "KB-2569-022", "Wiki", "มานี ดีใจ", "05/04/2569",
                "3 วัน", "Claims",
                new[] { "🏷️ RISC", "🏷️ Claims", "🏷️ Wiki" },
                yPos
            );
            scrollPanel.Controls.Add(card4);
            yPos += card4.Height + 8;

            // Card 5: Returned
            var card5 = CreateApprovalCard(
                "returned",
                "↩️ ส่งคืนแล้ว",
                "SOP: ระเบียบปฏิบัติงาน TFRS17 สำหรับฝ่ายบัญชี",
                "ส่งคืน: 06/04/2569",
                "KB-2569-019", "SOP", "สุรชัย บุญมาก", "02/04/2569",
                "↩️ เหตุผล: ต้องเพิ่ม Law Reference", "บัญชี",
                new[] { "🏷️ TFRS17", "🏷️ บัญชี" },
                yPos,
                true
            );
            scrollPanel.Controls.Add(card5);

            tabApproval.Controls.Add(scrollPanel);

            // Create footer with pagination
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = Color.FromArgb(248, 250, 251),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblPageInfo = new Label
            {
                Location = new Point(12, 10),
                AutoSize = true,
                Text = "แสดง 1-5 จาก 7 รายการ",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(127, 140, 141)
            };
            footer.Controls.Add(lblPageInfo);

            // Pagination buttons
            var paginationPanel = new Panel
            {
                Location = new Point(footer.Width - 150, 6),
                Size = new Size(140, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var btnPrev = CreatePageButton("«", 0, false);
            paginationPanel.Controls.Add(btnPrev);

            var btnPage1 = CreatePageButton("1", 30, true);
            paginationPanel.Controls.Add(btnPage1);

            var btnPage2 = CreatePageButton("2", 60, false);
            paginationPanel.Controls.Add(btnPage2);

            var btnNext = CreatePageButton("»", 90, false);
            paginationPanel.Controls.Add(btnNext);

            footer.Controls.Add(paginationPanel);

            tabApproval.Controls.Add(footer);
        }

        private Label CreatePill(string text, Color bgColor, Color fgColor, int x, int y)
        {
            var pill = new Label
            {
                Location = new Point(x, y),
                AutoSize = true,
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = bgColor,
                ForeColor = fgColor,
                Padding = new Padding(12, 3, 12, 3),
                BorderStyle = BorderStyle.FixedSingle
            };
            return pill;
        }

        private ComboBox CreateFilterCombo(string text, int x)
        {
            var combo = new ComboBox
            {
                Location = new Point(x, 6),
                Size = new Size(100, 24),
                Font = new Font("Segoe UI", 8.5f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            combo.Items.Add(text);
            combo.SelectedIndex = 0;
            return combo;
        }

        private Button CreatePageButton(string text, int x, bool active)
        {
            var btn = new Button
            {
                Location = new Point(x, 0),
                Size = new Size(26, 24),
                Text = text,
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = active ? Color.FromArgb(46, 116, 181) : Color.White,
                ForeColor = active ? Color.White : Color.FromArgb(26, 35, 50),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = active ? Color.FromArgb(46, 116, 181) : Color.FromArgb(208, 215, 222);
            return btn;
        }

        private Panel CreateApprovalCard(
            string type,
            string badge,
            string title,
            string slaText,
            string code,
            string articleType,
            string author,
            string submitDate,
            string waitingTime,
            string department,
            string[] tags,
            int yPos,
            bool isReturned = false)
        {
            var card = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(1140, isReturned ? 140 : 180),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Left border color
            Color borderColor = Color.FromArgb(243, 156, 18); // default yellow
            Color slaColor = Color.FromArgb(243, 156, 18);
            
            switch (type)
            {
                case "overdue":
                    borderColor = Color.FromArgb(231, 76, 60);
                    slaColor = Color.FromArgb(231, 76, 60);
                    break;
                case "urgent":
                    borderColor = Color.FromArgb(230, 126, 34);
                    slaColor = Color.FromArgb(230, 126, 34);
                    break;
                case "returned":
                    borderColor = Color.FromArgb(127, 140, 141);
                    slaColor = Color.FromArgb(127, 140, 141);
                    break;
            }

            var leftBorder = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = borderColor
            };
            card.Controls.Add(leftBorder);

            // Header
            var header = new Panel
            {
                Location = new Point(4, 0),
                Size = new Size(card.Width - 4, 32),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblBadge = new Label
            {
                Location = new Point(12, 6),
                AutoSize = true,
                Text = badge,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = slaColor,
                ForeColor = Color.White,
                Padding = new Padding(7, 2, 7, 2)
            };
            header.Controls.Add(lblBadge);

            var lblTitle = new Label
            {
                Location = new Point(lblBadge.Right + 10, 8),
                Size = new Size(600, 18),
                Text = title,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 35, 50)
            };
            header.Controls.Add(lblTitle);

            var lblSLA = new Label
            {
                Location = new Point(header.Width - 180, 8),
                Size = new Size(170, 18),
                Text = slaText,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = slaColor,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            header.Controls.Add(lblSLA);

            var headerSep = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            header.Controls.Add(headerSep);

            card.Controls.Add(header);

            // Body
            var body = new Panel
            {
                Location = new Point(4, 32),
                Size = new Size(card.Width - 4, card.Height - 32),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Meta line 1
            var lblMeta1 = new Label
            {
                Location = new Point(12, 8),
                AutoSize = true,
                Text = string.Format("📋 {0}     📑 {1}     👤 {2}     📤 ส่ง: {3}",
                    code, articleType, author, submitDate),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(127, 140, 141)
            };
            body.Controls.Add(lblMeta1);

            // Meta line 2
            var lblMeta2 = new Label
            {
                Location = new Point(12, 26),
                AutoSize = true,
                Text = string.Format("⏰ รอดำเนินการ: {0}     🏢 แผนก: {1}",
                    waitingTime, department),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(127, 140, 141)
            };
            body.Controls.Add(lblMeta2);

            // Tags
            int tagX = 12;
            int tagY = 48;
            foreach (var tag in tags)
            {
                var lblTag = new Label
                {
                    Location = new Point(tagX, tagY),
                    AutoSize = true,
                    Text = tag,
                    Font = new Font("Segoe UI", 8f),
                    BackColor = Color.FromArgb(245, 247, 250),
                    ForeColor = Color.FromArgb(74, 85, 104),
                    Padding = new Padding(6, 1, 6, 1),
                    BorderStyle = BorderStyle.FixedSingle
                };
                body.Controls.Add(lblTag);
                tagX += lblTag.Width + 4;
            }

            // Action buttons
            if (!isReturned)
            {
                var btnRead = new Button
                {
                    Location = new Point(12, 75),
                    Size = new Size(110, 26),
                    Text = "👁️ อ่าน Draft",
                    Font = new Font("Segoe UI", 9f),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };
                btnRead.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
                body.Controls.Add(btnRead);

                var btnApprove = new Button
                {
                    Location = new Point(130, 75),
                    Size = new Size(160, 26),
                    Text = "✅ Approve & Publish",
                    Font = new Font("Segoe UI", 9f),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand
                };
                btnApprove.FlatAppearance.BorderColor = Color.FromArgb(30, 139, 76);
                body.Controls.Add(btnApprove);

                var btnReject = new Button
                {
                    Location = new Point(298, 75),
                    Size = new Size(130, 26),
                    Text = "❌ ส่งคืนแก้ไข",
                    Font = new Font("Segoe UI", 9f),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(231, 76, 60),
                    Cursor = Cursors.Hand
                };
                btnReject.FlatAppearance.BorderColor = Color.FromArgb(231, 76, 60);
                body.Controls.Add(btnReject);
            }
            else
            {
                var btnReadReturned = new Button
                {
                    Location = new Point(12, 75),
                    Size = new Size(180, 26),
                    Text = "👁️ ดู Draft (แก้ไขใหม่)",
                    Font = new Font("Segoe UI", 9f),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };
                btnReadReturned.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
                body.Controls.Add(btnReadReturned);
            }

            card.Controls.Add(body);

            return card;
        }

        // ── IKBDashboardController Implementation ─────────────────────────────────────

        public void DisplayStats(KBDashboardStats stats)
        {
            if (stats == null) return;

            // KPI Cards
            lblTotalArticles.Text = stats.TotalArticles.ToString();
            lblTotalArticlesSub.Text = string.Format("↑ +{0} เดือนนี้ (+{1:0.0}%)", 
                stats.TotalArticles > 90 ? stats.TotalArticles - 90 : 0,
                stats.TotalArticles > 90 ? ((stats.TotalArticles - 90) * 100.0 / 90) : 0);
            lblTotalArticlesSub.ForeColor = Color.FromArgb(39, 174, 96);

            lblMonthlyViews.Text = stats.MonthlyViews.ToString("#,##0");
            lblMonthlyViewsSub.Text = "↑ +12% จากเดือนที่แล้ว";
            lblMonthlyViewsSub.ForeColor = Color.FromArgb(39, 174, 96);

            lblAvgRating.Text = stats.AverageRating.ToString("0.0");
            lblAvgRatingSub.Text = "/ 5.0 — ดีขึ้น 0.2";
            lblAvgRatingSub.ForeColor = Color.FromArgb(243, 156, 18);

            lblPublished.Text = stats.PublishedArticles.ToString();
            lblPublishedSub.Text = string.Format("✅ คิดเป็น {0}% ของทั้งหมด",
                stats.TotalArticles > 0 ? (stats.PublishedArticles * 100 / stats.TotalArticles) : 0);
            lblPublishedSub.ForeColor = Color.FromArgb(39, 174, 96);

            // Alert counts
            lblOverdueCount.Text = stats.OverdueReviews.ToString() + " รายการ";
            lblDraftCount.Text = stats.DraftArticles.ToString() + " รายการ";
            lblExpiringCount.Text = stats.ExpiringArticles.ToString() + " รายการ";
            lblArchivedCount.Text = stats.ArchivedArticles.ToString() + " รายการ";

            // Approval badge
            lblApprovalBadge.Text = stats.PendingApprovals.ToString();
        }

        public void DisplayMonthlyChart(IEnumerable<KBMonthlyArticle> monthlyData)
        {
            if (monthlyData == null) return;

            var body = (Panel)pnlChart.Tag;
            body.Controls.Clear();

            var data = monthlyData.ToList();
            if (data.Count == 0) return;

            int maxCount = data.Max(m => m.ArticleCount);
            if (maxCount == 0) maxCount = 1;

            int chartHeight = 120;
            int barWidth = (body.Width - 24) / data.Count;
            int x = 0;

            foreach (var month in data)
            {
                int barHeight = (int)((month.ArticleCount / (double)maxCount) * chartHeight);
                
                var lblValue = new Label
                {
                    Location = new Point(x, 5),
                    Size = new Size(barWidth - 5, 18),
                    Text = month.ArticleCount.ToString(),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 116, 181),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };
                body.Controls.Add(lblValue);

                var bar = new Panel
                {
                    Location = new Point(x + 5, chartHeight + 25 - barHeight),
                    Size = new Size(barWidth - 15, barHeight),
                    BackColor = Color.FromArgb(46, 116, 181),
                    Cursor = Cursors.Hand
                };
                bar.MouseEnter += (s, e) => bar.BackColor = Color.FromArgb(30, 90, 150);
                bar.MouseLeave += (s, e) => bar.BackColor = Color.FromArgb(46, 116, 181);
                body.Controls.Add(bar);

                var lblMonth = new Label
                {
                    Location = new Point(x, chartHeight + 30),
                    Size = new Size(barWidth - 5, 16),
                    Text = month.MonthName.Substring(0, Math.Min(3, month.MonthName.Length)) + ".",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(127, 140, 141),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };
                body.Controls.Add(lblMonth);

                x += barWidth;
            }
        }

        public void DisplayTopArticles(IEnumerable<KBTopArticle> topArticles)
        {
            if (topArticles == null) return;

            var body = (Panel)pnlTopArticles.Tag;
            body.Controls.Clear();

            var articles = topArticles.Take(5).ToList();
            int y = 0;
            int rank = 1;

            foreach (var article in articles)
            {
                var item = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(body.Width - 5, 38),
                    BackColor = Color.Transparent
                };

                var lblRank = new Label
                {
                    Location = new Point(0, 8),
                    Size = new Size(22, 22),
                    Text = rank.ToString(),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = GetRankColor(rank),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                item.Controls.Add(lblRank);

                var lblTitle = new Label
                {
                    Location = new Point(30, 4),
                    Size = new Size(body.Width - 130, 16),
                    Text = article.Title,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(26, 35, 50),
                    BackColor = Color.Transparent
                };
                item.Controls.Add(lblTitle);

                var lblMeta = new Label
                {
                    Location = new Point(30, 20),
                    Size = new Size(body.Width - 130, 14),
                    Text = string.Format("{0} | {1} | {2} | ⭐ {3:0.0}",
                        article.ArticleCode, article.ArticleType, article.Category, article.Rating),
                    Font = new Font("Segoe UI", 7.5f),
                    ForeColor = Color.FromArgb(127, 140, 141),
                    BackColor = Color.Transparent
                };
                item.Controls.Add(lblMeta);

                var lblViews = new Label
                {
                    Location = new Point(body.Width - 90, 12),
                    Size = new Size(80, 16),
                    Text = string.Format("{0} v", article.ViewCount),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 116, 181),
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.Transparent
                };
                item.Controls.Add(lblViews);

                var sep = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 1,
                    BackColor = Color.FromArgb(240, 244, 249)
                };
                item.Controls.Add(sep);

                body.Controls.Add(item);

                y += 38;
                rank++;
            }
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetStatus(string message)
        {
            lblStatusMessage.Text = message;
        }

        // ── Helper Methods ──────────────────────────────────────────────────────

        private Color GetRankColor(int rank)
        {
            switch (rank)
            {
                case 1: return Color.FromArgb(212, 172, 13);  // Gold
                case 2: return Color.FromArgb(127, 140, 141); // Silver
                case 3: return Color.FromArgb(230, 126, 34);  // Bronze
                default: return Color.FromArgb(214, 228, 240); // Light blue
            }
        }

        private void ShowDashboardTab()
        {
            tabControl.SelectedIndex = 0;
        }

        private void ShowApprovalTab()
        {
            tabControl.SelectedIndex = 1;
        }

        // ── Event Handlers ──────────────────────────────────────────────────────

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.RefreshDashboard();
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.ExportToExcel();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.PrintReport();
        }

        private void btnSendReport_Click(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.SendReport();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
