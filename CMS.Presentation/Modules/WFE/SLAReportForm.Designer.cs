namespace CMS.Presentation.Modules.Workflow
{
    partial class SLAReportForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Chrome ──────────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlRibbonTabs;
        private System.Windows.Forms.Panel pnlRibbonBorder;
        private System.Windows.Forms.Panel pnlToolbarContainer;

        // ── App body ─────────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlAppBody;
        private System.Windows.Forms.Panel pnlSidebarContainer;
        private System.Windows.Forms.Panel pnlNavBorder;
        private System.Windows.Forms.Panel pnlContent;

        // ── Page header ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlBreadcrumb;
        private System.Windows.Forms.Label lblBreadcrumb;
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Button btnExportHeader;

        // ── Dashboard body ───────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlDashboard;
        private System.Windows.Forms.FlowLayoutPanel flpCards;

        // Card 1 – SLA Compliance
        private System.Windows.Forms.Panel pnlCardCompliance;
        private System.Windows.Forms.Label lblCardComplianceIcon;
        private System.Windows.Forms.Label lblCardComplianceTitle;
        private System.Windows.Forms.Label lblCardComplianceCount;
        private System.Windows.Forms.Label lblCardComplianceSub;

        // Card 2 – Avg Cycle Time
        private System.Windows.Forms.Panel pnlCardCycleTime;
        private System.Windows.Forms.Label lblCardCycleTimeIcon;
        private System.Windows.Forms.Label lblCardCycleTimeTitle;
        private System.Windows.Forms.Label lblCardCycleTimeCount;
        private System.Windows.Forms.Label lblCardCycleTimeSub;

        // Card 3 – เกิน SLA
        private System.Windows.Forms.Panel pnlCardOverdue;
        private System.Windows.Forms.Label lblCardOverdueIcon;
        private System.Windows.Forms.Label lblCardOverdueTitle;
        private System.Windows.Forms.Label lblCardOverdueCount;
        private System.Windows.Forms.Label lblCardOverdueSub;

        // Card 4 – Workflow เสร็จสิ้น
        private System.Windows.Forms.Panel pnlCardCompleted;
        private System.Windows.Forms.Label lblCardCompletedIcon;
        private System.Windows.Forms.Label lblCardCompletedTitle;
        private System.Windows.Forms.Label lblCardCompletedCount;
        private System.Windows.Forms.Label lblCardCompletedSub;

        // ── Main 2×2 grid ────────────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMainContent;
        private System.Windows.Forms.Panel pnlTemplateListOuter;
        private System.Windows.Forms.Panel pnlTrendChartOuter;
        private System.Windows.Forms.Panel pnlOverdueOuter;
        private System.Windows.Forms.Panel pnlCycleChartOuter;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlRibbonTabs       = new System.Windows.Forms.Panel();
            this.pnlRibbonBorder     = new System.Windows.Forms.Panel();
            this.pnlToolbarContainer = new System.Windows.Forms.Panel();
            this.pnlAppBody          = new System.Windows.Forms.Panel();
            this.pnlSidebarContainer = new System.Windows.Forms.Panel();
            this.pnlNavBorder        = new System.Windows.Forms.Panel();
            this.pnlContent          = new System.Windows.Forms.Panel();
            this.pnlBreadcrumb       = new System.Windows.Forms.Panel();
            this.lblBreadcrumb       = new System.Windows.Forms.Label();
            this.pnlTitleBar         = new System.Windows.Forms.Panel();
            this.lblTitle            = new System.Windows.Forms.Label();
            this.lblSubtitle         = new System.Windows.Forms.Label();
            this.btnExportHeader     = new System.Windows.Forms.Button();
            this.pnlDashboard        = new System.Windows.Forms.Panel();
            this.flpCards            = new System.Windows.Forms.FlowLayoutPanel();

            this.pnlCardCompliance      = new System.Windows.Forms.Panel();
            this.lblCardComplianceIcon  = new System.Windows.Forms.Label();
            this.lblCardComplianceTitle = new System.Windows.Forms.Label();
            this.lblCardComplianceCount = new System.Windows.Forms.Label();
            this.lblCardComplianceSub   = new System.Windows.Forms.Label();

            this.pnlCardCycleTime      = new System.Windows.Forms.Panel();
            this.lblCardCycleTimeIcon  = new System.Windows.Forms.Label();
            this.lblCardCycleTimeTitle = new System.Windows.Forms.Label();
            this.lblCardCycleTimeCount = new System.Windows.Forms.Label();
            this.lblCardCycleTimeSub   = new System.Windows.Forms.Label();

            this.pnlCardOverdue      = new System.Windows.Forms.Panel();
            this.lblCardOverdueIcon  = new System.Windows.Forms.Label();
            this.lblCardOverdueTitle = new System.Windows.Forms.Label();
            this.lblCardOverdueCount = new System.Windows.Forms.Label();
            this.lblCardOverdueSub   = new System.Windows.Forms.Label();

            this.pnlCardCompleted      = new System.Windows.Forms.Panel();
            this.lblCardCompletedIcon  = new System.Windows.Forms.Label();
            this.lblCardCompletedTitle = new System.Windows.Forms.Label();
            this.lblCardCompletedCount = new System.Windows.Forms.Label();
            this.lblCardCompletedSub   = new System.Windows.Forms.Label();

            this.tlpMainContent       = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTemplateListOuter = new System.Windows.Forms.Panel();
            this.pnlTrendChartOuter   = new System.Windows.Forms.Panel();
            this.pnlOverdueOuter      = new System.Windows.Forms.Panel();
            this.pnlCycleChartOuter   = new System.Windows.Forms.Panel();

            this.SuspendLayout();

            // ── pnlRibbonTabs ────────────────────────────────────────────────────
            this.pnlRibbonTabs.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlRibbonTabs.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabs.Height   = 32;
            this.pnlRibbonTabs.Name     = "pnlRibbonTabs";

            // ── pnlRibbonBorder ──────────────────────────────────────────────────
            this.pnlRibbonBorder.BackColor = System.Drawing.Color.FromArgb(200, 215, 240);
            this.pnlRibbonBorder.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonBorder.Height   = 1;
            this.pnlRibbonBorder.Name     = "pnlRibbonBorder";

            // ── pnlToolbarContainer ──────────────────────────────────────────────
            this.pnlToolbarContainer.BackColor = System.Drawing.Color.White;
            this.pnlToolbarContainer.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarContainer.Height   = 82;
            this.pnlToolbarContainer.Name     = "pnlToolbarContainer";

            // ── pnlAppBody ───────────────────────────────────────────────────────
            this.pnlAppBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlAppBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlAppBody.Name      = "pnlAppBody";
            this.pnlAppBody.Controls.Add(this.pnlContent);
            this.pnlAppBody.Controls.Add(this.pnlNavBorder);
            this.pnlAppBody.Controls.Add(this.pnlSidebarContainer);

            // ── pnlSidebarContainer ──────────────────────────────────────────────
            this.pnlSidebarContainer.BackColor  = System.Drawing.Color.FromArgb(26, 35, 67);
            this.pnlSidebarContainer.Dock       = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebarContainer.Width      = 202;
            this.pnlSidebarContainer.Name       = "pnlSidebarContainer";
            this.pnlSidebarContainer.AutoScroll = true;

            // ── pnlNavBorder ─────────────────────────────────────────────────────
            this.pnlNavBorder.BackColor = System.Drawing.Color.FromArgb(225, 228, 235);
            this.pnlNavBorder.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlNavBorder.Width     = 1;
            this.pnlNavBorder.Name      = "pnlNavBorder";

            // ── pnlContent ───────────────────────────────────────────────────────
            this.pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlContent.Name      = "pnlContent";
            this.pnlContent.Controls.Add(this.pnlDashboard);
            this.pnlContent.Controls.Add(this.pnlTitleBar);
            this.pnlContent.Controls.Add(this.pnlBreadcrumb);

            // ── pnlBreadcrumb ────────────────────────────────────────────────────
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Height    = 28;
            this.pnlBreadcrumb.Name      = "pnlBreadcrumb";
            this.pnlBreadcrumb.Padding   = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);

            this.lblBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblBreadcrumb.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblBreadcrumb.Text      = "CMS  ›  Workflow Engine  ›  Reports  ›  SLA";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBreadcrumb.Name      = "lblBreadcrumb";

            // ── pnlTitleBar ──────────────────────────────────────────────────────
            this.pnlTitleBar.BackColor = System.Drawing.Color.White;
            this.pnlTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height    = 62;
            this.pnlTitleBar.Name      = "pnlTitleBar";
            this.pnlTitleBar.Padding   = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlTitleBar.Controls.Add(this.btnExportHeader);
            this.pnlTitleBar.Controls.Add(this.lblSubtitle);
            this.pnlTitleBar.Controls.Add(this.lblTitle);

            this.lblTitle.AutoSize  = true;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitle.Location  = new System.Drawing.Point(20, 6);
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Text      = "รายงาน SLA — Workflow Engine";

            this.lblSubtitle.AutoSize  = true;
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblSubtitle.Location  = new System.Drawing.Point(20, 36);
            this.lblSubtitle.Name      = "lblSubtitle";
            this.lblSubtitle.Text      = "ช่วงข้อมูล: 01/04/2569 – 30/04/2569 · ทุก Template · ทุกหน่วยงาน";

            this.btnExportHeader.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnExportHeader.BackColor = System.Drawing.Color.White;
            this.btnExportHeader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportHeader.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnExportHeader.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnExportHeader.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnExportHeader.Location  = new System.Drawing.Point(940, 18);
            this.btnExportHeader.Name      = "btnExportHeader";
            this.btnExportHeader.Size      = new System.Drawing.Size(80, 26);
            this.btnExportHeader.Text      = "↓  Export";

            // ── pnlDashboard ─────────────────────────────────────────────────────
            this.pnlDashboard.BackColor = System.Drawing.Color.White;
            this.pnlDashboard.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.Name      = "pnlDashboard";
            this.pnlDashboard.Padding   = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlDashboard.Controls.Add(this.tlpMainContent);
            this.pnlDashboard.Controls.Add(this.flpCards);

            // ── flpCards ─────────────────────────────────────────────────────────
            this.flpCards.BackColor      = System.Drawing.Color.White;
            this.flpCards.Dock           = System.Windows.Forms.DockStyle.Top;
            this.flpCards.FlowDirection  = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flpCards.Height         = 100;
            this.flpCards.Name           = "flpCards";
            this.flpCards.Padding        = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.flpCards.WrapContents   = false;

            // ── Card 1: SLA Compliance (green) ───────────────────────────────────
            this.pnlCardCompliance.BackColor    = System.Drawing.Color.White;
            this.pnlCardCompliance.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardCompliance.Margin       = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlCardCompliance.Name         = "pnlCardCompliance";
            this.pnlCardCompliance.Size         = new System.Drawing.Size(272, 84);
            this.pnlCardCompliance.Controls.Add(this.lblCardComplianceSub);
            this.pnlCardCompliance.Controls.Add(this.lblCardComplianceCount);
            this.pnlCardCompliance.Controls.Add(this.lblCardComplianceTitle);
            this.pnlCardCompliance.Controls.Add(this.lblCardComplianceIcon);

            this.lblCardComplianceIcon.AutoSize  = true;
            this.lblCardComplianceIcon.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardComplianceIcon.ForeColor = System.Drawing.Color.FromArgb(23, 133, 49);
            this.lblCardComplianceIcon.Location  = new System.Drawing.Point(14, 8);
            this.lblCardComplianceIcon.Name      = "lblCardComplianceIcon";
            this.lblCardComplianceIcon.Text      = "✓";

            this.lblCardComplianceTitle.AutoSize  = true;
            this.lblCardComplianceTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardComplianceTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardComplianceTitle.Location  = new System.Drawing.Point(40, 12);
            this.lblCardComplianceTitle.Name      = "lblCardComplianceTitle";
            this.lblCardComplianceTitle.Text      = "SLA Compliance";

            this.lblCardComplianceCount.AutoSize  = false;
            this.lblCardComplianceCount.Size      = new System.Drawing.Size(240, 32);
            this.lblCardComplianceCount.Font      = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblCardComplianceCount.ForeColor = System.Drawing.Color.FromArgb(23, 133, 49);
            this.lblCardComplianceCount.Location  = new System.Drawing.Point(14, 28);
            this.lblCardComplianceCount.Name      = "lblCardComplianceCount";
            this.lblCardComplianceCount.Text      = "91.4%";

            this.lblCardComplianceSub.AutoSize  = false;
            this.lblCardComplianceSub.Size      = new System.Drawing.Size(240, 18);
            this.lblCardComplianceSub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardComplianceSub.ForeColor = System.Drawing.Color.FromArgb(23, 133, 49);
            this.lblCardComplianceSub.Location  = new System.Drawing.Point(14, 60);
            this.lblCardComplianceSub.Name      = "lblCardComplianceSub";
            this.lblCardComplianceSub.Text      = "+2.1 ตามเป้า 90%";

            // ── Card 2: Avg Cycle Time (blue) ────────────────────────────────────
            this.pnlCardCycleTime.BackColor    = System.Drawing.Color.White;
            this.pnlCardCycleTime.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardCycleTime.Margin       = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlCardCycleTime.Name         = "pnlCardCycleTime";
            this.pnlCardCycleTime.Size         = new System.Drawing.Size(272, 84);
            this.pnlCardCycleTime.Controls.Add(this.lblCardCycleTimeSub);
            this.pnlCardCycleTime.Controls.Add(this.lblCardCycleTimeCount);
            this.pnlCardCycleTime.Controls.Add(this.lblCardCycleTimeTitle);
            this.pnlCardCycleTime.Controls.Add(this.lblCardCycleTimeIcon);

            this.lblCardCycleTimeIcon.AutoSize  = true;
            this.lblCardCycleTimeIcon.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardCycleTimeIcon.ForeColor = System.Drawing.Color.FromArgb(25, 100, 195);
            this.lblCardCycleTimeIcon.Location  = new System.Drawing.Point(14, 8);
            this.lblCardCycleTimeIcon.Name      = "lblCardCycleTimeIcon";
            this.lblCardCycleTimeIcon.Text      = "⏱";

            this.lblCardCycleTimeTitle.AutoSize  = true;
            this.lblCardCycleTimeTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardCycleTimeTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardCycleTimeTitle.Location  = new System.Drawing.Point(40, 12);
            this.lblCardCycleTimeTitle.Name      = "lblCardCycleTimeTitle";
            this.lblCardCycleTimeTitle.Text      = "Avg Cycle Time";

            this.lblCardCycleTimeCount.AutoSize  = false;
            this.lblCardCycleTimeCount.Size      = new System.Drawing.Size(240, 32);
            this.lblCardCycleTimeCount.Font      = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblCardCycleTimeCount.ForeColor = System.Drawing.Color.FromArgb(25, 100, 195);
            this.lblCardCycleTimeCount.Location  = new System.Drawing.Point(14, 28);
            this.lblCardCycleTimeCount.Name      = "lblCardCycleTimeCount";
            this.lblCardCycleTimeCount.Text      = "2.8 วัน";

            this.lblCardCycleTimeSub.AutoSize  = false;
            this.lblCardCycleTimeSub.Size      = new System.Drawing.Size(240, 18);
            this.lblCardCycleTimeSub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardCycleTimeSub.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.lblCardCycleTimeSub.Location  = new System.Drawing.Point(14, 60);
            this.lblCardCycleTimeSub.Name      = "lblCardCycleTimeSub";
            this.lblCardCycleTimeSub.Text      = "-0.4 จากเดือนก่อน";

            // ── Card 3: เกิน SLA (red) ────────────────────────────────────────────
            this.pnlCardOverdue.BackColor    = System.Drawing.Color.White;
            this.pnlCardOverdue.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardOverdue.Margin       = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlCardOverdue.Name         = "pnlCardOverdue";
            this.pnlCardOverdue.Size         = new System.Drawing.Size(272, 84);
            this.pnlCardOverdue.Controls.Add(this.lblCardOverdueSub);
            this.pnlCardOverdue.Controls.Add(this.lblCardOverdueCount);
            this.pnlCardOverdue.Controls.Add(this.lblCardOverdueTitle);
            this.pnlCardOverdue.Controls.Add(this.lblCardOverdueIcon);

            this.lblCardOverdueIcon.AutoSize  = true;
            this.lblCardOverdueIcon.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardOverdueIcon.ForeColor = System.Drawing.Color.FromArgb(194, 38, 54);
            this.lblCardOverdueIcon.Location  = new System.Drawing.Point(14, 8);
            this.lblCardOverdueIcon.Name      = "lblCardOverdueIcon";
            this.lblCardOverdueIcon.Text      = "⚠";

            this.lblCardOverdueTitle.AutoSize  = true;
            this.lblCardOverdueTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardOverdueTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardOverdueTitle.Location  = new System.Drawing.Point(40, 12);
            this.lblCardOverdueTitle.Name      = "lblCardOverdueTitle";
            this.lblCardOverdueTitle.Text      = "เกิน SLA";

            this.lblCardOverdueCount.AutoSize  = false;
            this.lblCardOverdueCount.Size      = new System.Drawing.Size(240, 32);
            this.lblCardOverdueCount.Font      = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblCardOverdueCount.ForeColor = System.Drawing.Color.FromArgb(194, 38, 54);
            this.lblCardOverdueCount.Location  = new System.Drawing.Point(14, 28);
            this.lblCardOverdueCount.Name      = "lblCardOverdueCount";
            this.lblCardOverdueCount.Text      = "18 / 211";

            this.lblCardOverdueSub.AutoSize  = false;
            this.lblCardOverdueSub.Size      = new System.Drawing.Size(240, 18);
            this.lblCardOverdueSub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardOverdueSub.ForeColor = System.Drawing.Color.FromArgb(194, 38, 54);
            this.lblCardOverdueSub.Location  = new System.Drawing.Point(14, 60);
            this.lblCardOverdueSub.Name      = "lblCardOverdueSub";
            this.lblCardOverdueSub.Text      = "8.5% ของทั้งหมด";

            // ── Card 4: Workflow เสร็จสิ้น (amber) ───────────────────────────────
            this.pnlCardCompleted.BackColor    = System.Drawing.Color.White;
            this.pnlCardCompleted.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardCompleted.Margin       = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.pnlCardCompleted.Name         = "pnlCardCompleted";
            this.pnlCardCompleted.Size         = new System.Drawing.Size(272, 84);
            this.pnlCardCompleted.Controls.Add(this.lblCardCompletedSub);
            this.pnlCardCompleted.Controls.Add(this.lblCardCompletedCount);
            this.pnlCardCompleted.Controls.Add(this.lblCardCompletedTitle);
            this.pnlCardCompleted.Controls.Add(this.lblCardCompletedIcon);

            this.lblCardCompletedIcon.AutoSize  = true;
            this.lblCardCompletedIcon.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardCompletedIcon.ForeColor = System.Drawing.Color.FromArgb(197, 120, 16);
            this.lblCardCompletedIcon.Location  = new System.Drawing.Point(14, 8);
            this.lblCardCompletedIcon.Name      = "lblCardCompletedIcon";
            this.lblCardCompletedIcon.Text      = "⚑";

            this.lblCardCompletedTitle.AutoSize  = true;
            this.lblCardCompletedTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardCompletedTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardCompletedTitle.Location  = new System.Drawing.Point(40, 12);
            this.lblCardCompletedTitle.Name      = "lblCardCompletedTitle";
            this.lblCardCompletedTitle.Text      = "Workflow เสร็จสิ้น";

            this.lblCardCompletedCount.AutoSize  = false;
            this.lblCardCompletedCount.Size      = new System.Drawing.Size(240, 32);
            this.lblCardCompletedCount.Font      = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblCardCompletedCount.ForeColor = System.Drawing.Color.FromArgb(197, 120, 16);
            this.lblCardCompletedCount.Location  = new System.Drawing.Point(14, 28);
            this.lblCardCompletedCount.Name      = "lblCardCompletedCount";
            this.lblCardCompletedCount.Text      = "193";

            this.lblCardCompletedSub.AutoSize  = false;
            this.lblCardCompletedSub.Size      = new System.Drawing.Size(240, 18);
            this.lblCardCompletedSub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardCompletedSub.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.lblCardCompletedSub.Location  = new System.Drawing.Point(14, 60);
            this.lblCardCompletedSub.Name      = "lblCardCompletedSub";
            this.lblCardCompletedSub.Text      = "เมษายน 2569 (MTD)";

            this.flpCards.Controls.Add(this.pnlCardCompliance);
            this.flpCards.Controls.Add(this.pnlCardCycleTime);
            this.flpCards.Controls.Add(this.pnlCardOverdue);
            this.flpCards.Controls.Add(this.pnlCardCompleted);

            // ── tlpMainContent (2 cols × 2 rows) ────────────────────────────────
            this.tlpMainContent.BackColor   = System.Drawing.Color.White;
            this.tlpMainContent.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.tlpMainContent.Name        = "tlpMainContent";
            this.tlpMainContent.ColumnCount = 2;
            this.tlpMainContent.RowCount    = 2;
            this.tlpMainContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tlpMainContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tlpMainContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tlpMainContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));

            this.pnlTemplateListOuter.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlTemplateListOuter.BackColor = System.Drawing.Color.White;
            this.pnlTemplateListOuter.Margin    = new System.Windows.Forms.Padding(0, 0, 6, 6);
            this.pnlTemplateListOuter.Name      = "pnlTemplateListOuter";

            this.pnlTrendChartOuter.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlTrendChartOuter.BackColor = System.Drawing.Color.White;
            this.pnlTrendChartOuter.Margin    = new System.Windows.Forms.Padding(6, 0, 0, 6);
            this.pnlTrendChartOuter.Name      = "pnlTrendChartOuter";

            this.pnlOverdueOuter.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlOverdueOuter.BackColor = System.Drawing.Color.White;
            this.pnlOverdueOuter.Margin    = new System.Windows.Forms.Padding(0, 6, 6, 0);
            this.pnlOverdueOuter.Name      = "pnlOverdueOuter";

            this.pnlCycleChartOuter.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlCycleChartOuter.BackColor = System.Drawing.Color.White;
            this.pnlCycleChartOuter.Margin    = new System.Windows.Forms.Padding(6, 6, 0, 0);
            this.pnlCycleChartOuter.Name      = "pnlCycleChartOuter";

            this.tlpMainContent.Controls.Add(this.pnlTemplateListOuter, 0, 0);
            this.tlpMainContent.Controls.Add(this.pnlTrendChartOuter,   1, 0);
            this.tlpMainContent.Controls.Add(this.pnlOverdueOuter,      0, 1);
            this.tlpMainContent.Controls.Add(this.pnlCycleChartOuter,   1, 1);

            // ── Form ─────────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode  = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor      = System.Drawing.Color.FromArgb(243, 246, 251);
            this.ClientSize     = new System.Drawing.Size(1280, 800);
            this.Font           = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize    = new System.Drawing.Size(1000, 650);
            this.Name           = "SLAReportForm";
            this.Text           = "SLA Report — Workflow Engine";
            this.WindowState    = System.Windows.Forms.FormWindowState.Maximized;
            this.Load          += new System.EventHandler(this.SLAReportForm_Load);

            this.Controls.Add(this.pnlAppBody);
            this.Controls.Add(this.pnlToolbarContainer);
            this.Controls.Add(this.pnlRibbonBorder);
            this.Controls.Add(this.pnlRibbonTabs);

            this.ResumeLayout(false);
        }
    }
}
