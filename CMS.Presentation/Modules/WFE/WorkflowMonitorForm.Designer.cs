namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowMonitorForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Top chrome ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlRibbonTabs;
        private System.Windows.Forms.Panel pnlRibbonBorder;
        private System.Windows.Forms.Panel pnlToolbarContainer;

        // ── App body ────────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlAppBody;
        private System.Windows.Forms.Panel pnlSidebarContainer;
        private System.Windows.Forms.Panel pnlNavBorder;
        private System.Windows.Forms.Panel pnlContent;

        // ── Content: breadcrumb + title ─────────────────────────────────────────
        private System.Windows.Forms.Panel pnlBreadcrumb;
        private System.Windows.Forms.Label lblBreadcrumb;
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnExport;

        // ── Dashboard body ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlDashboard;

        // ── Stat cards (4) ───────────────────────────────────────────────────────
        private System.Windows.Forms.FlowLayoutPanel flpCards;

        // Card 1: กำลังทำงาน
        private System.Windows.Forms.Panel pnlCardActive;
        private System.Windows.Forms.Label lblCardActiveIcon;
        private System.Windows.Forms.Label lblCardActiveTitle;
        private System.Windows.Forms.Label lblTotalInstances;        // mapped from interface
        private System.Windows.Forms.Label lblCardActiveSub;

        // Card 2: เกิน SLA
        private System.Windows.Forms.Panel pnlCardSLA;
        private System.Windows.Forms.Label lblCardSLAIcon;
        private System.Windows.Forms.Label lblCardSLATitle;
        private System.Windows.Forms.Label lblFailed;                // mapped from interface
        private System.Windows.Forms.Label lblCardSLASub;

        // Card 3: เสร็จวันนี้
        private System.Windows.Forms.Panel pnlCardDone;
        private System.Windows.Forms.Label lblCardDoneIcon;
        private System.Windows.Forms.Label lblCardDoneTitle;
        private System.Windows.Forms.Label lblCompleted;             // mapped from interface
        private System.Windows.Forms.Label lblCardDoneSub;
        private System.Windows.Forms.Label lblAverageCompletionTime; // mapped from interface

        // Card 4: รออนุมัติ
        private System.Windows.Forms.Panel pnlCardPending;
        private System.Windows.Forms.Label lblCardPendingIcon;
        private System.Windows.Forms.Label lblCardPendingTitle;
        private System.Windows.Forms.Label lblSuspended;             // mapped from interface
        private System.Windows.Forms.Label lblCardPendingSub;

        // Hidden/unused legacy labels (kept for interface compat)
        private System.Windows.Forms.Label lblTotalInstancesLabel;
        private System.Windows.Forms.Label lblCompletedLabel;
        private System.Windows.Forms.Label lblFailedLabel;
        private System.Windows.Forms.Label lblInProgressLabel;
        private System.Windows.Forms.Label lblInProgress;
        private System.Windows.Forms.Label lblSuspendedLabel;
        private System.Windows.Forms.Label lblAverageCompletionTimeLabel;

        // ── Filter bar ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlFilterBar;
        private System.Windows.Forms.Label lblFilterStatus;
        private System.Windows.Forms.ComboBox cmbFilterStatus;
        private System.Windows.Forms.Label lblFilterTemplate;
        private System.Windows.Forms.ComboBox cmbFilterType;
        private System.Windows.Forms.Label lblFilterAssignee;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblFilterDateFrom;
        private System.Windows.Forms.TextBox txtDateFrom;
        private System.Windows.Forms.Label lblFilterDateTo;
        private System.Windows.Forms.TextBox txtDateTo;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClearFilter;

        // ── Instance list ───────────────────────────────────────────────────────
        private System.Windows.Forms.ListView lvInstances;
        private System.Windows.Forms.ColumnHeader colId;
        private System.Windows.Forms.ColumnHeader colWorkflow;
        private System.Windows.Forms.ColumnHeader colAssignee;
        private System.Windows.Forms.ColumnHeader colStart;
        private System.Windows.Forms.ColumnHeader colStep;
        private System.Windows.Forms.ColumnHeader colCurrentStep;
        private System.Windows.Forms.ColumnHeader colStatus;

        // ── Pagination ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlPagination;
        private System.Windows.Forms.Label lblResultCount;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Label lblPage;
        private System.Windows.Forms.Button btnNext;

        // Hidden DataGridViews (legacy SLA/distribution data — not shown in main view)
        private System.Windows.Forms.DataGridView dgvSLAChart;
        private System.Windows.Forms.DataGridView dgvTaskDistribution;
        private System.Windows.Forms.DataGridView dgvBottlenecks;
        private System.Windows.Forms.DataGridView dgvEscalations;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components              = new System.ComponentModel.Container();
            this.pnlRibbonTabs           = new System.Windows.Forms.Panel();
            this.pnlRibbonBorder         = new System.Windows.Forms.Panel();
            this.pnlToolbarContainer     = new System.Windows.Forms.Panel();
            this.pnlAppBody              = new System.Windows.Forms.Panel();
            this.pnlSidebarContainer     = new System.Windows.Forms.Panel();
            this.pnlNavBorder            = new System.Windows.Forms.Panel();
            this.pnlContent              = new System.Windows.Forms.Panel();
            this.pnlBreadcrumb           = new System.Windows.Forms.Panel();
            this.lblBreadcrumb           = new System.Windows.Forms.Label();
            this.pnlTitleBar             = new System.Windows.Forms.Panel();
            this.lblTitle                = new System.Windows.Forms.Label();
            this.lblSubtitle             = new System.Windows.Forms.Label();
            this.btnRefresh              = new System.Windows.Forms.Button();
            this.btnExport               = new System.Windows.Forms.Button();
            this.pnlDashboard            = new System.Windows.Forms.Panel();
            this.flpCards                = new System.Windows.Forms.FlowLayoutPanel();

            this.pnlCardActive           = new System.Windows.Forms.Panel();
            this.lblCardActiveIcon       = new System.Windows.Forms.Label();
            this.lblCardActiveTitle      = new System.Windows.Forms.Label();
            this.lblTotalInstances       = new System.Windows.Forms.Label();
            this.lblCardActiveSub        = new System.Windows.Forms.Label();

            this.pnlCardSLA              = new System.Windows.Forms.Panel();
            this.lblCardSLAIcon          = new System.Windows.Forms.Label();
            this.lblCardSLATitle         = new System.Windows.Forms.Label();
            this.lblFailed               = new System.Windows.Forms.Label();
            this.lblCardSLASub           = new System.Windows.Forms.Label();

            this.pnlCardDone             = new System.Windows.Forms.Panel();
            this.lblCardDoneIcon         = new System.Windows.Forms.Label();
            this.lblCardDoneTitle        = new System.Windows.Forms.Label();
            this.lblCompleted            = new System.Windows.Forms.Label();
            this.lblCardDoneSub          = new System.Windows.Forms.Label();
            this.lblAverageCompletionTime= new System.Windows.Forms.Label();

            this.pnlCardPending          = new System.Windows.Forms.Panel();
            this.lblCardPendingIcon      = new System.Windows.Forms.Label();
            this.lblCardPendingTitle     = new System.Windows.Forms.Label();
            this.lblSuspended            = new System.Windows.Forms.Label();
            this.lblCardPendingSub       = new System.Windows.Forms.Label();

            // Legacy hidden labels
            this.lblTotalInstancesLabel       = new System.Windows.Forms.Label();
            this.lblCompletedLabel            = new System.Windows.Forms.Label();
            this.lblFailedLabel               = new System.Windows.Forms.Label();
            this.lblInProgressLabel           = new System.Windows.Forms.Label();
            this.lblInProgress                = new System.Windows.Forms.Label();
            this.lblSuspendedLabel            = new System.Windows.Forms.Label();
            this.lblAverageCompletionTimeLabel= new System.Windows.Forms.Label();

            this.pnlFilterBar            = new System.Windows.Forms.Panel();
            this.lblFilterStatus         = new System.Windows.Forms.Label();
            this.cmbFilterStatus         = new System.Windows.Forms.ComboBox();
            this.lblFilterTemplate       = new System.Windows.Forms.Label();
            this.cmbFilterType           = new System.Windows.Forms.ComboBox();
            this.lblFilterAssignee       = new System.Windows.Forms.Label();
            this.txtSearch               = new System.Windows.Forms.TextBox();
            this.lblFilterDateFrom       = new System.Windows.Forms.Label();
            this.txtDateFrom             = new System.Windows.Forms.TextBox();
            this.lblFilterDateTo         = new System.Windows.Forms.Label();
            this.txtDateTo               = new System.Windows.Forms.TextBox();
            this.btnSearch               = new System.Windows.Forms.Button();
            this.btnClearFilter          = new System.Windows.Forms.Button();

            this.lvInstances             = new System.Windows.Forms.ListView();
            this.colId                   = new System.Windows.Forms.ColumnHeader();
            this.colWorkflow             = new System.Windows.Forms.ColumnHeader();
            this.colAssignee             = new System.Windows.Forms.ColumnHeader();
            this.colStart                = new System.Windows.Forms.ColumnHeader();
            this.colStep                 = new System.Windows.Forms.ColumnHeader();
            this.colCurrentStep          = new System.Windows.Forms.ColumnHeader();
            this.colStatus               = new System.Windows.Forms.ColumnHeader();

            this.pnlPagination           = new System.Windows.Forms.Panel();
            this.lblResultCount          = new System.Windows.Forms.Label();
            this.btnPrev                 = new System.Windows.Forms.Button();
            this.lblPage                 = new System.Windows.Forms.Label();
            this.btnNext                 = new System.Windows.Forms.Button();

            this.dgvSLAChart             = new System.Windows.Forms.DataGridView();
            this.dgvTaskDistribution     = new System.Windows.Forms.DataGridView();
            this.dgvBottlenecks          = new System.Windows.Forms.DataGridView();
            this.dgvEscalations          = new System.Windows.Forms.DataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvSLAChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTaskDistribution)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBottlenecks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEscalations)).BeginInit();
            this.pnlRibbonTabs.SuspendLayout();
            this.pnlToolbarContainer.SuspendLayout();
            this.pnlAppBody.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlBreadcrumb.SuspendLayout();
            this.pnlTitleBar.SuspendLayout();
            this.pnlDashboard.SuspendLayout();
            this.flpCards.SuspendLayout();
            this.pnlFilterBar.SuspendLayout();
            this.pnlPagination.SuspendLayout();
            this.SuspendLayout();

            // ── pnlRibbonTabs ───────────────────────────────────────────────────
            this.pnlRibbonTabs.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlRibbonTabs.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabs.Height    = 32;
            this.pnlRibbonTabs.Name      = "pnlRibbonTabs";
            this.pnlRibbonTabs.Padding   = new System.Windows.Forms.Padding(4, 0, 4, 0);

            // ── pnlRibbonBorder ─────────────────────────────────────────────────
            this.pnlRibbonBorder.BackColor = System.Drawing.Color.FromArgb(200, 215, 240);
            this.pnlRibbonBorder.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonBorder.Height    = 1;
            this.pnlRibbonBorder.Name      = "pnlRibbonBorder";

            // ── pnlToolbarContainer ─────────────────────────────────────────────
            this.pnlToolbarContainer.BackColor = System.Drawing.Color.White;
            this.pnlToolbarContainer.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarContainer.Height    = 82;
            this.pnlToolbarContainer.Name      = "pnlToolbarContainer";
            this.pnlToolbarContainer.Padding   = new System.Windows.Forms.Padding(6, 0, 6, 0);

            // ── pnlAppBody ──────────────────────────────────────────────────────
            this.pnlAppBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlAppBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlAppBody.Name      = "pnlAppBody";
            this.pnlAppBody.Controls.Add(this.pnlContent);
            this.pnlAppBody.Controls.Add(this.pnlNavBorder);
            this.pnlAppBody.Controls.Add(this.pnlSidebarContainer);

            // ── pnlSidebarContainer ─────────────────────────────────────────────
            this.pnlSidebarContainer.BackColor  = System.Drawing.Color.FromArgb(26, 35, 67);
            this.pnlSidebarContainer.Dock       = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebarContainer.Width      = 202;
            this.pnlSidebarContainer.Name       = "pnlSidebarContainer";
            this.pnlSidebarContainer.Padding    = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.pnlSidebarContainer.AutoScroll = true;

            // ── pnlNavBorder ────────────────────────────────────────────────────
            this.pnlNavBorder.BackColor = System.Drawing.Color.FromArgb(225, 228, 235);
            this.pnlNavBorder.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlNavBorder.Width     = 1;
            this.pnlNavBorder.Name      = "pnlNavBorder";

            // ── pnlContent ──────────────────────────────────────────────────────
            this.pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlContent.Name      = "pnlContent";
            this.pnlContent.Controls.Add(this.pnlDashboard);
            this.pnlContent.Controls.Add(this.pnlTitleBar);
            this.pnlContent.Controls.Add(this.pnlBreadcrumb);

            // ── pnlBreadcrumb ───────────────────────────────────────────────────
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Height    = 28;
            this.pnlBreadcrumb.Name      = "pnlBreadcrumb";
            this.pnlBreadcrumb.Padding   = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);

            this.lblBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblBreadcrumb.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblBreadcrumb.Text      = "CMS  ›  Workflow Engine  ›  Monitor";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBreadcrumb.Name      = "lblBreadcrumb";

            // ── pnlTitleBar ─────────────────────────────────────────────────────
            this.pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height    = 56;
            this.pnlTitleBar.Name      = "pnlTitleBar";
            this.pnlTitleBar.Padding   = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlTitleBar.Controls.Add(this.btnExport);
            this.pnlTitleBar.Controls.Add(this.btnRefresh);
            this.pnlTitleBar.Controls.Add(this.lblSubtitle);
            this.pnlTitleBar.Controls.Add(this.lblTitle);

            this.lblTitle.AutoSize  = true;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitle.Location  = new System.Drawing.Point(20, 6);
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Text      = "ตรวจสอบ Workflow ทั้งระบบ";

            this.lblSubtitle.AutoSize  = true;
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblSubtitle.Location  = new System.Drawing.Point(20, 36);
            this.lblSubtitle.Name      = "lblSubtitle";
            this.lblSubtitle.Text      = "มุมมองผู้ดูแล - แสดงทุก Workflow Instance ในระบบ";

            this.btnRefresh.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnRefresh.BackColor = System.Drawing.Color.White;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnRefresh.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnRefresh.Location  = new System.Drawing.Point(790, 16);
            this.btnRefresh.Name      = "btnRefresh";
            this.btnRefresh.Size      = new System.Drawing.Size(86, 26);
            this.btnRefresh.Text      = "↺  รีเฟรช";
            this.btnRefresh.Click    += new System.EventHandler(this.btnRefresh_Click);

            this.btnExport.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnExport.BackColor = System.Drawing.Color.White;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnExport.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnExport.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnExport.Location  = new System.Drawing.Point(884, 16);
            this.btnExport.Name      = "btnExport";
            this.btnExport.Size      = new System.Drawing.Size(80, 26);
            this.btnExport.Text      = "↑  Export";

            // ── pnlDashboard ────────────────────────────────────────────────────
            this.pnlDashboard.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlDashboard.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.Name      = "pnlDashboard";
            this.pnlDashboard.Padding   = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.pnlDashboard.Controls.Add(this.lvInstances);
            this.pnlDashboard.Controls.Add(this.pnlPagination);
            this.pnlDashboard.Controls.Add(this.pnlFilterBar);
            this.pnlDashboard.Controls.Add(this.flpCards);

            // ── flpCards ─────────────────────────────────────────────────────────
            this.flpCards.BackColor     = System.Drawing.Color.FromArgb(243, 246, 251);
            this.flpCards.Dock          = System.Windows.Forms.DockStyle.Top;
            this.flpCards.Height        = 112;
            this.flpCards.Name          = "flpCards";
            this.flpCards.Padding       = new System.Windows.Forms.Padding(0, 12, 0, 8);
            this.flpCards.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flpCards.WrapContents  = false;
            this.flpCards.Controls.Add(this.pnlCardActive);
            this.flpCards.Controls.Add(this.pnlCardSLA);
            this.flpCards.Controls.Add(this.pnlCardDone);
            this.flpCards.Controls.Add(this.pnlCardPending);

            // Card 1: กำลังทำงาน
            this.pnlCardActive.BackColor   = System.Drawing.Color.White;
            this.pnlCardActive.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardActive.Margin      = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlCardActive.Size        = new System.Drawing.Size(252, 88);
            this.pnlCardActive.Padding     = new System.Windows.Forms.Padding(14, 10, 14, 8);
            this.pnlCardActive.Name        = "pnlCardActive";
            this.pnlCardActive.Controls.Add(this.lblCardActiveSub);
            this.pnlCardActive.Controls.Add(this.lblTotalInstances);
            this.pnlCardActive.Controls.Add(this.lblCardActiveTitle);
            this.pnlCardActive.Controls.Add(this.lblCardActiveIcon);

            this.lblCardActiveIcon.AutoSize  = true;
            this.lblCardActiveIcon.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardActiveIcon.Location  = new System.Drawing.Point(14, 8);
            this.lblCardActiveIcon.Text      = "▶";
            this.lblCardActiveIcon.ForeColor = System.Drawing.Color.FromArgb(25, 100, 195);
            this.lblCardActiveIcon.Name      = "lblCardActiveIcon";

            this.lblCardActiveTitle.AutoSize  = true;
            this.lblCardActiveTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardActiveTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardActiveTitle.Location  = new System.Drawing.Point(40, 12);
            this.lblCardActiveTitle.Text      = "กำลังทำงาน";
            this.lblCardActiveTitle.Name      = "lblCardActiveTitle";

            this.lblTotalInstances.AutoSize  = true;
            this.lblTotalInstances.Font      = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblTotalInstances.ForeColor = System.Drawing.Color.FromArgb(25, 100, 195);
            this.lblTotalInstances.Location  = new System.Drawing.Point(14, 28);
            this.lblTotalInstances.Text      = "0";
            this.lblTotalInstances.Name      = "lblTotalInstances";

            this.lblCardActiveSub.AutoSize  = true;
            this.lblCardActiveSub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardActiveSub.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.lblCardActiveSub.Location  = new System.Drawing.Point(14, 66);
            this.lblCardActiveSub.Text      = "ทั้งระบบ";
            this.lblCardActiveSub.Name      = "lblCardActiveSub";

            // Card 2: เกิน SLA
            this.pnlCardSLA.BackColor   = System.Drawing.Color.White;
            this.pnlCardSLA.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardSLA.Margin      = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlCardSLA.Size        = new System.Drawing.Size(252, 88);
            this.pnlCardSLA.Padding     = new System.Windows.Forms.Padding(14, 10, 14, 8);
            this.pnlCardSLA.Name        = "pnlCardSLA";
            this.pnlCardSLA.Controls.Add(this.lblCardSLASub);
            this.pnlCardSLA.Controls.Add(this.lblFailed);
            this.pnlCardSLA.Controls.Add(this.lblCardSLATitle);
            this.pnlCardSLA.Controls.Add(this.lblCardSLAIcon);

            this.lblCardSLAIcon.AutoSize  = true;
            this.lblCardSLAIcon.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardSLAIcon.Location  = new System.Drawing.Point(14, 8);
            this.lblCardSLAIcon.Text      = "⚠";
            this.lblCardSLAIcon.ForeColor = System.Drawing.Color.FromArgb(194, 38, 54);
            this.lblCardSLAIcon.Name      = "lblCardSLAIcon";

            this.lblCardSLATitle.AutoSize  = true;
            this.lblCardSLATitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardSLATitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardSLATitle.Location  = new System.Drawing.Point(40, 12);
            this.lblCardSLATitle.Text      = "เกิน SLA";
            this.lblCardSLATitle.Name      = "lblCardSLATitle";

            this.lblFailed.AutoSize  = true;
            this.lblFailed.Font      = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblFailed.ForeColor = System.Drawing.Color.FromArgb(194, 38, 54);
            this.lblFailed.Location  = new System.Drawing.Point(14, 28);
            this.lblFailed.Text      = "0";
            this.lblFailed.Name      = "lblFailed";

            this.lblCardSLASub.AutoSize  = true;
            this.lblCardSLASub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardSLASub.ForeColor = System.Drawing.Color.FromArgb(194, 38, 54);
            this.lblCardSLASub.Location  = new System.Drawing.Point(14, 66);
            this.lblCardSLASub.Text      = "-1 จากเมื่อวาน";
            this.lblCardSLASub.Name      = "lblCardSLASub";

            // Card 3: เสร็จวันนี้
            this.pnlCardDone.BackColor   = System.Drawing.Color.White;
            this.pnlCardDone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardDone.Margin      = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlCardDone.Size        = new System.Drawing.Size(252, 88);
            this.pnlCardDone.Padding     = new System.Windows.Forms.Padding(14, 10, 14, 8);
            this.pnlCardDone.Name        = "pnlCardDone";
            this.pnlCardDone.Controls.Add(this.lblAverageCompletionTime);
            this.pnlCardDone.Controls.Add(this.lblCompleted);
            this.pnlCardDone.Controls.Add(this.lblCardDoneTitle);
            this.pnlCardDone.Controls.Add(this.lblCardDoneIcon);

            this.lblCardDoneIcon.AutoSize  = true;
            this.lblCardDoneIcon.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardDoneIcon.Location  = new System.Drawing.Point(14, 8);
            this.lblCardDoneIcon.Text      = "✔";
            this.lblCardDoneIcon.ForeColor = System.Drawing.Color.FromArgb(23, 133, 49);
            this.lblCardDoneIcon.Name      = "lblCardDoneIcon";

            this.lblCardDoneTitle.AutoSize  = true;
            this.lblCardDoneTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardDoneTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardDoneTitle.Location  = new System.Drawing.Point(40, 12);
            this.lblCardDoneTitle.Text      = "เสร็จวันนี้";
            this.lblCardDoneTitle.Name      = "lblCardDoneTitle";

            this.lblCompleted.AutoSize  = true;
            this.lblCompleted.Font      = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblCompleted.ForeColor = System.Drawing.Color.FromArgb(23, 133, 49);
            this.lblCompleted.Location  = new System.Drawing.Point(14, 28);
            this.lblCompleted.Text      = "0";
            this.lblCompleted.Name      = "lblCompleted";

            this.lblAverageCompletionTime.AutoSize  = true;
            this.lblAverageCompletionTime.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblAverageCompletionTime.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.lblAverageCompletionTime.Location  = new System.Drawing.Point(14, 66);
            this.lblAverageCompletionTime.Text      = "เฉลี่ย — วัน/WF";
            this.lblAverageCompletionTime.Name      = "lblAverageCompletionTime";

            // Card 4: รออนุมัติ
            this.pnlCardPending.BackColor   = System.Drawing.Color.White;
            this.pnlCardPending.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardPending.Margin      = new System.Windows.Forms.Padding(0);
            this.pnlCardPending.Size        = new System.Drawing.Size(252, 88);
            this.pnlCardPending.Padding     = new System.Windows.Forms.Padding(14, 10, 14, 8);
            this.pnlCardPending.Name        = "pnlCardPending";
            this.pnlCardPending.Controls.Add(this.lblCardPendingSub);
            this.pnlCardPending.Controls.Add(this.lblSuspended);
            this.pnlCardPending.Controls.Add(this.lblCardPendingTitle);
            this.pnlCardPending.Controls.Add(this.lblCardPendingIcon);

            this.lblCardPendingIcon.AutoSize  = true;
            this.lblCardPendingIcon.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardPendingIcon.Location  = new System.Drawing.Point(14, 8);
            this.lblCardPendingIcon.Text      = "⏰";
            this.lblCardPendingIcon.ForeColor = System.Drawing.Color.FromArgb(197, 120, 16);
            this.lblCardPendingIcon.Name      = "lblCardPendingIcon";

            this.lblCardPendingTitle.AutoSize  = true;
            this.lblCardPendingTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardPendingTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardPendingTitle.Location  = new System.Drawing.Point(40, 12);
            this.lblCardPendingTitle.Text      = "รออนุมัติ";
            this.lblCardPendingTitle.Name      = "lblCardPendingTitle";

            this.lblSuspended.AutoSize  = true;
            this.lblSuspended.Font      = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblSuspended.ForeColor = System.Drawing.Color.FromArgb(197, 120, 16);
            this.lblSuspended.Location  = new System.Drawing.Point(14, 28);
            this.lblSuspended.Text      = "0";
            this.lblSuspended.Name      = "lblSuspended";

            this.lblCardPendingSub.AutoSize  = true;
            this.lblCardPendingSub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardPendingSub.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.lblCardPendingSub.Location  = new System.Drawing.Point(14, 66);
            this.lblCardPendingSub.Text      = "กระจาย 8 Template";
            this.lblCardPendingSub.Name      = "lblCardPendingSub";

            // ── Hidden legacy labels (not shown in UI) ──────────────────────────
            this.lblTotalInstancesLabel.Visible  = false;
            this.lblTotalInstancesLabel.Location = new System.Drawing.Point(-200, -200);
            this.lblCompletedLabel.Visible  = false;
            this.lblCompletedLabel.Location = new System.Drawing.Point(-200, -200);
            this.lblFailedLabel.Visible  = false;
            this.lblFailedLabel.Location = new System.Drawing.Point(-200, -200);
            this.lblInProgressLabel.Visible  = false;
            this.lblInProgressLabel.Location = new System.Drawing.Point(-200, -200);
            this.lblInProgress.Visible  = false;
            this.lblInProgress.Location = new System.Drawing.Point(-200, -200);
            this.lblSuspendedLabel.Visible  = false;
            this.lblSuspendedLabel.Location = new System.Drawing.Point(-200, -200);
            this.lblAverageCompletionTimeLabel.Visible  = false;
            this.lblAverageCompletionTimeLabel.Location = new System.Drawing.Point(-200, -200);

            // ── pnlFilterBar ────────────────────────────────────────────────────
            this.pnlFilterBar.BackColor = System.Drawing.Color.White;
            this.pnlFilterBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlFilterBar.Height    = 50;
            this.pnlFilterBar.Name      = "pnlFilterBar";

            this.lblFilterStatus.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFilterStatus.ForeColor = System.Drawing.Color.FromArgb(60, 70, 90);
            this.lblFilterStatus.Location  = new System.Drawing.Point(12, 14);
            this.lblFilterStatus.Size      = new System.Drawing.Size(48, 22);
            this.lblFilterStatus.Text      = "สถานะ:";
            this.lblFilterStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFilterStatus.Name      = "lblFilterStatus";

            this.cmbFilterStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterStatus.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;
            this.cmbFilterStatus.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbFilterStatus.Items.AddRange(new object[] { "กำลังทำงาน", "ทั้งหมด", "เกิน SLA", "เสร็จสิ้น", "ถูกปฏิเสธ" });
            this.cmbFilterStatus.SelectedIndex = 0;
            this.cmbFilterStatus.Location      = new System.Drawing.Point(64, 11);
            this.cmbFilterStatus.Size          = new System.Drawing.Size(120, 24);
            this.cmbFilterStatus.Name          = "cmbFilterStatus";

            this.lblFilterTemplate.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFilterTemplate.ForeColor = System.Drawing.Color.FromArgb(60, 70, 90);
            this.lblFilterTemplate.Location  = new System.Drawing.Point(192, 14);
            this.lblFilterTemplate.Size      = new System.Drawing.Size(60, 22);
            this.lblFilterTemplate.Text      = "Template:";
            this.lblFilterTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFilterTemplate.Name      = "lblFilterTemplate";

            this.cmbFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterType.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;
            this.cmbFilterType.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbFilterType.Items.AddRange(new object[] { "ทุก Template", "KB Article", "Upload IHC", "เปลี่ยน Permission", "Review SOP", "Work Instruction", "IT Landscape", "Onboard" });
            this.cmbFilterType.SelectedIndex = 0;
            this.cmbFilterType.Location      = new System.Drawing.Point(256, 11);
            this.cmbFilterType.Size          = new System.Drawing.Size(130, 24);
            this.cmbFilterType.Name          = "cmbFilterType";

            this.lblFilterAssignee.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFilterAssignee.ForeColor = System.Drawing.Color.FromArgb(60, 70, 90);
            this.lblFilterAssignee.Location  = new System.Drawing.Point(394, 14);
            this.lblFilterAssignee.Size      = new System.Drawing.Size(40, 22);
            this.lblFilterAssignee.Text      = "ผู้ยื่น:";
            this.lblFilterAssignee.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFilterAssignee.Name      = "lblFilterAssignee";

            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.ForeColor   = System.Drawing.Color.FromArgb(140, 150, 165);
            this.txtSearch.Location    = new System.Drawing.Point(438, 12);
            this.txtSearch.Size        = new System.Drawing.Size(130, 24);
            this.txtSearch.Text        = "ADUserName...";
            this.txtSearch.Name        = "txtSearch";

            this.lblFilterDateFrom.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFilterDateFrom.ForeColor = System.Drawing.Color.FromArgb(60, 70, 90);
            this.lblFilterDateFrom.Location  = new System.Drawing.Point(576, 14);
            this.lblFilterDateFrom.Size      = new System.Drawing.Size(34, 22);
            this.lblFilterDateFrom.Text      = "วันที่:";
            this.lblFilterDateFrom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFilterDateFrom.Name      = "lblFilterDateFrom";

            this.txtDateFrom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDateFrom.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDateFrom.Location    = new System.Drawing.Point(614, 12);
            this.txtDateFrom.Size        = new System.Drawing.Size(96, 24);
            this.txtDateFrom.Text        = "01/04/2569";
            this.txtDateFrom.Name        = "txtDateFrom";

            this.lblFilterDateTo.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFilterDateTo.ForeColor = System.Drawing.Color.FromArgb(60, 70, 90);
            this.lblFilterDateTo.Location  = new System.Drawing.Point(716, 14);
            this.lblFilterDateTo.Size      = new System.Drawing.Size(22, 22);
            this.lblFilterDateTo.Text      = "ถึง";
            this.lblFilterDateTo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblFilterDateTo.Name      = "lblFilterDateTo";

            this.txtDateTo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDateTo.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDateTo.Location    = new System.Drawing.Point(742, 12);
            this.txtDateTo.Size        = new System.Drawing.Size(96, 24);
            this.txtDateTo.Text        = "30/04/2569";
            this.txtDateTo.Name        = "txtDateTo";

            this.btnSearch.BackColor  = System.Drawing.Color.FromArgb(31, 56, 100);
            this.btnSearch.FlatStyle  = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.Font       = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSearch.ForeColor  = System.Drawing.Color.White;
            this.btnSearch.Location   = new System.Drawing.Point(846, 11);
            this.btnSearch.Size       = new System.Drawing.Size(72, 26);
            this.btnSearch.Text       = "🔍 ค้นหา";
            this.btnSearch.Name       = "btnSearch";

            this.btnClearFilter.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnClearFilter.BackColor = System.Drawing.Color.White;
            this.btnClearFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFilter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnClearFilter.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnClearFilter.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.btnClearFilter.Location  = new System.Drawing.Point(926, 11);
            this.btnClearFilter.Size      = new System.Drawing.Size(82, 26);
            this.btnClearFilter.Text      = "ล้างตัวกรอง";
            this.btnClearFilter.Name      = "btnClearFilter";

            this.pnlFilterBar.Controls.Add(this.btnClearFilter);
            this.pnlFilterBar.Controls.Add(this.btnSearch);
            this.pnlFilterBar.Controls.Add(this.txtDateTo);
            this.pnlFilterBar.Controls.Add(this.lblFilterDateTo);
            this.pnlFilterBar.Controls.Add(this.txtDateFrom);
            this.pnlFilterBar.Controls.Add(this.lblFilterDateFrom);
            this.pnlFilterBar.Controls.Add(this.txtSearch);
            this.pnlFilterBar.Controls.Add(this.lblFilterAssignee);
            this.pnlFilterBar.Controls.Add(this.cmbFilterType);
            this.pnlFilterBar.Controls.Add(this.lblFilterTemplate);
            this.pnlFilterBar.Controls.Add(this.cmbFilterStatus);
            this.pnlFilterBar.Controls.Add(this.lblFilterStatus);

            // ── lvInstances ─────────────────────────────────────────────────────
            this.lvInstances.BackColor      = System.Drawing.Color.White;
            this.lvInstances.BorderStyle    = System.Windows.Forms.BorderStyle.None;
            this.lvInstances.Dock           = System.Windows.Forms.DockStyle.Fill;
            this.lvInstances.Font           = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lvInstances.FullRowSelect  = true;
            this.lvInstances.GridLines      = false;
            this.lvInstances.HideSelection  = false;
            this.lvInstances.MultiSelect    = true;
            this.lvInstances.Name           = "lvInstances";
            this.lvInstances.OwnerDraw      = true;
            this.lvInstances.View           = System.Windows.Forms.View.Details;
            this.lvInstances.ShowItemToolTips = true;
            this.lvInstances.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colId, this.colWorkflow, this.colAssignee,
                this.colStart, this.colStep, this.colCurrentStep, this.colStatus });

            this.colId.Text          = "ID";              this.colId.Width          = 148;
            this.colWorkflow.Text    = "Workflow / Template"; this.colWorkflow.Width = 230;
            this.colAssignee.Text    = "ผู้ยื่น";         this.colAssignee.Width    = 120;
            this.colStart.Text       = "เริ่ม";            this.colStart.Width       = 80;
            this.colStep.Text        = "Step";             this.colStep.Width        = 62;
            this.colCurrentStep.Text = "Step ปัจจุบัน";    this.colCurrentStep.Width = 190;
            this.colStatus.Text      = "สถานะ";            this.colStatus.Width      = 150;

            // ── pnlPagination ───────────────────────────────────────────────────
            this.pnlPagination.BackColor = System.Drawing.Color.White;
            this.pnlPagination.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlPagination.Height    = 44;
            this.pnlPagination.Name      = "pnlPagination";
            this.pnlPagination.Padding   = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlPagination.Controls.Add(this.btnNext);
            this.pnlPagination.Controls.Add(this.lblPage);
            this.pnlPagination.Controls.Add(this.btnPrev);
            this.pnlPagination.Controls.Add(this.lblResultCount);

            this.lblResultCount.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblResultCount.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblResultCount.Location  = new System.Drawing.Point(12, 12);
            this.lblResultCount.Size      = new System.Drawing.Size(320, 22);
            this.lblResultCount.Text      = "พบ 0 รายการ";
            this.lblResultCount.Name      = "lblResultCount";

            this.btnPrev.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnPrev.BackColor = System.Drawing.Color.White;
            this.btnPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrev.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnPrev.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnPrev.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnPrev.Location  = new System.Drawing.Point(770, 8);
            this.btnPrev.Size      = new System.Drawing.Size(76, 28);
            this.btnPrev.Text      = "←  ก่อนหน้า";
            this.btnPrev.Name      = "btnPrev";

            this.lblPage.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.lblPage.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPage.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.lblPage.Location  = new System.Drawing.Point(853, 12);
            this.lblPage.Size      = new System.Drawing.Size(70, 22);
            this.lblPage.Text      = "แสดง 1-8";
            this.lblPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPage.Name      = "lblPage";

            this.btnNext.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnNext.BackColor = System.Drawing.Color.White;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnNext.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnNext.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnNext.Location  = new System.Drawing.Point(930, 8);
            this.btnNext.Size      = new System.Drawing.Size(72, 28);
            this.btnNext.Text      = "ถัดไป  →";
            this.btnNext.Name      = "btnNext";

            // Hidden DataGridViews (legacy, not visible in main UI)
            this.dgvSLAChart.Visible          = false;
            this.dgvTaskDistribution.Visible   = false;
            this.dgvBottlenecks.Visible        = false;
            this.dgvEscalations.Visible        = false;
            this.dgvSLAChart.AllowUserToAddRows    = false;
            this.dgvTaskDistribution.AllowUserToAddRows = false;
            this.dgvBottlenecks.AllowUserToAddRows = false;
            this.dgvEscalations.AllowUserToAddRows = false;
            this.dgvSLAChart.ReadOnly          = true;
            this.dgvTaskDistribution.ReadOnly  = true;
            this.dgvBottlenecks.ReadOnly       = true;
            this.dgvEscalations.ReadOnly       = true;

            // ── Form ────────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(243, 246, 251);
            this.ClientSize          = new System.Drawing.Size(1280, 800);
            this.Font                = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize         = new System.Drawing.Size(1000, 650);
            this.Name                = "WorkflowMonitorForm";
            this.Text                = "ตรวจสอบ Workflow — Core Management System";
            this.WindowState         = System.Windows.Forms.FormWindowState.Maximized;
            this.Load               += new System.EventHandler(this.WorkflowMonitorForm_Load);
            this.FormClosing        += new System.Windows.Forms.FormClosingEventHandler(this.WorkflowMonitorForm_FormClosing);

            this.Controls.Add(this.pnlAppBody);
            this.Controls.Add(this.pnlToolbarContainer);
            this.Controls.Add(this.pnlRibbonBorder);
            this.Controls.Add(this.pnlRibbonTabs);

            this.pnlRibbonTabs.ResumeLayout(false);
            this.pnlToolbarContainer.ResumeLayout(false);
            this.pnlAppBody.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlBreadcrumb.ResumeLayout(false);
            this.pnlTitleBar.ResumeLayout(false);
            this.pnlDashboard.ResumeLayout(false);
            this.flpCards.ResumeLayout(false);
            this.pnlFilterBar.ResumeLayout(false);
            this.pnlPagination.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSLAChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTaskDistribution)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBottlenecks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEscalations)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
