namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowDashboardFormNew
    {
        private System.ComponentModel.IContainer components = null;

        // ── Top chrome ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlRibbonTabs;
        private System.Windows.Forms.Panel pnlToolbarContainer;
        private System.Windows.Forms.Panel pnlRibbonBorder;

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
        private System.Windows.Forms.Button btnExportHeader;

        // ── Dashboard body ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlDashboard;

        // ── Summary cards (4) ───────────────────────────────────────────────────
        private System.Windows.Forms.FlowLayoutPanel flpCards;

        private System.Windows.Forms.Panel pnlCardActive;
        private System.Windows.Forms.Label lblCardActiveIcon;
        private System.Windows.Forms.Label lblCardActiveTitle;
        private System.Windows.Forms.Label lblCardActiveCount;
        private System.Windows.Forms.Label lblCardActiveSub;

        private System.Windows.Forms.Panel pnlCardSLA;
        private System.Windows.Forms.Label lblCardSLAIcon;
        private System.Windows.Forms.Label lblCardSLATitle;
        private System.Windows.Forms.Label lblCardSLACount;
        private System.Windows.Forms.Label lblCardSLASub;

        private System.Windows.Forms.Panel pnlCardDone;
        private System.Windows.Forms.Label lblCardDoneIcon;
        private System.Windows.Forms.Label lblCardDoneTitle;
        private System.Windows.Forms.Label lblCardDoneCount;
        private System.Windows.Forms.Label lblCardDoneSub;

        private System.Windows.Forms.Panel pnlCardPending;
        private System.Windows.Forms.Label lblCardPendingIcon;
        private System.Windows.Forms.Label lblCardPendingTitle;
        private System.Windows.Forms.Label lblCardPendingCount;
        private System.Windows.Forms.Label lblCardPendingSub;

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

        // ── Task list ───────────────────────────────────────────────────────────
        private System.Windows.Forms.ListView lvTasks;
        private System.Windows.Forms.ColumnHeader colId;
        private System.Windows.Forms.ColumnHeader colWorkflow;
        private System.Windows.Forms.ColumnHeader colAssignee;
        private System.Windows.Forms.ColumnHeader colStart;
        private System.Windows.Forms.ColumnHeader colStep;
        private System.Windows.Forms.ColumnHeader colCurrentStep;
        private System.Windows.Forms.ColumnHeader colStatus;

        // ── Pagination ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlPagination;
        private System.Windows.Forms.Label lblSelectionInfo;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Label lblPage;
        private System.Windows.Forms.Button btnNext;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlRibbonTabs = new System.Windows.Forms.Panel();
            this.pnlToolbarContainer = new System.Windows.Forms.Panel();
            this.pnlRibbonBorder = new System.Windows.Forms.Panel();
            this.pnlAppBody = new System.Windows.Forms.Panel();
            this.pnlSidebarContainer = new System.Windows.Forms.Panel();
            this.pnlNavBorder = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlBreadcrumb = new System.Windows.Forms.Panel();
            this.lblBreadcrumb = new System.Windows.Forms.Label();
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnExportHeader = new System.Windows.Forms.Button();
            this.pnlDashboard = new System.Windows.Forms.Panel();
            this.flpCards = new System.Windows.Forms.FlowLayoutPanel();

            this.pnlCardActive = new System.Windows.Forms.Panel();
            this.lblCardActiveIcon = new System.Windows.Forms.Label();
            this.lblCardActiveTitle = new System.Windows.Forms.Label();
            this.lblCardActiveCount = new System.Windows.Forms.Label();
            this.lblCardActiveSub = new System.Windows.Forms.Label();

            this.pnlCardSLA = new System.Windows.Forms.Panel();
            this.lblCardSLAIcon = new System.Windows.Forms.Label();
            this.lblCardSLATitle = new System.Windows.Forms.Label();
            this.lblCardSLACount = new System.Windows.Forms.Label();
            this.lblCardSLASub = new System.Windows.Forms.Label();

            this.pnlCardDone = new System.Windows.Forms.Panel();
            this.lblCardDoneIcon = new System.Windows.Forms.Label();
            this.lblCardDoneTitle = new System.Windows.Forms.Label();
            this.lblCardDoneCount = new System.Windows.Forms.Label();
            this.lblCardDoneSub = new System.Windows.Forms.Label();

            this.pnlCardPending = new System.Windows.Forms.Panel();
            this.lblCardPendingIcon = new System.Windows.Forms.Label();
            this.lblCardPendingTitle = new System.Windows.Forms.Label();
            this.lblCardPendingCount = new System.Windows.Forms.Label();
            this.lblCardPendingSub = new System.Windows.Forms.Label();

            this.pnlFilterBar = new System.Windows.Forms.Panel();
            this.lblFilterStatus = new System.Windows.Forms.Label();
            this.cmbFilterStatus = new System.Windows.Forms.ComboBox();
            this.lblFilterTemplate = new System.Windows.Forms.Label();
            this.cmbFilterType = new System.Windows.Forms.ComboBox();
            this.lblFilterAssignee = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblFilterDateFrom = new System.Windows.Forms.Label();
            this.txtDateFrom = new System.Windows.Forms.TextBox();
            this.lblFilterDateTo = new System.Windows.Forms.Label();
            this.txtDateTo = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClearFilter = new System.Windows.Forms.Button();

            this.lvTasks = new System.Windows.Forms.ListView();
            this.colId = new System.Windows.Forms.ColumnHeader();
            this.colWorkflow = new System.Windows.Forms.ColumnHeader();
            this.colAssignee = new System.Windows.Forms.ColumnHeader();
            this.colStart = new System.Windows.Forms.ColumnHeader();
            this.colStep = new System.Windows.Forms.ColumnHeader();
            this.colCurrentStep = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();

            this.pnlPagination = new System.Windows.Forms.Panel();
            this.lblSelectionInfo = new System.Windows.Forms.Label();
            this.btnPrev = new System.Windows.Forms.Button();
            this.lblPage = new System.Windows.Forms.Label();
            this.btnNext = new System.Windows.Forms.Button();

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
            this.pnlRibbonTabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabs.Height = 32;
            this.pnlRibbonTabs.Name = "pnlRibbonTabs";
            this.pnlRibbonTabs.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);

            // ── pnlRibbonBorder ─────────────────────────────────────────────────
            this.pnlRibbonBorder.BackColor = System.Drawing.Color.FromArgb(200, 215, 240);
            this.pnlRibbonBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonBorder.Height = 1;
            this.pnlRibbonBorder.Name = "pnlRibbonBorder";

            // ── pnlToolbarContainer ─────────────────────────────────────────────
            this.pnlToolbarContainer.BackColor = System.Drawing.Color.White;
            this.pnlToolbarContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarContainer.Height = 82;
            this.pnlToolbarContainer.Name = "pnlToolbarContainer";
            this.pnlToolbarContainer.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);

            // ── pnlAppBody ──────────────────────────────────────────────────────
            this.pnlAppBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAppBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlAppBody.Name = "pnlAppBody";
            this.pnlAppBody.Controls.Add(this.pnlContent);
            this.pnlAppBody.Controls.Add(this.pnlNavBorder);
            this.pnlAppBody.Controls.Add(this.pnlSidebarContainer);

            // ── pnlSidebarContainer ─────────────────────────────────────────────
            this.pnlSidebarContainer.BackColor = System.Drawing.Color.FromArgb(26, 35, 67);
            this.pnlSidebarContainer.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebarContainer.Width = 202;
            this.pnlSidebarContainer.Name = "pnlSidebarContainer";
            this.pnlSidebarContainer.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.pnlSidebarContainer.AutoScroll = true;

            // ── pnlNavBorder ────────────────────────────────────────────────────
            this.pnlNavBorder.BackColor = System.Drawing.Color.FromArgb(225, 228, 235);
            this.pnlNavBorder.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlNavBorder.Width = 1;
            this.pnlNavBorder.Name = "pnlNavBorder";

            // ── pnlContent ──────────────────────────────────────────────────────
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Controls.Add(this.pnlDashboard);
            this.pnlContent.Controls.Add(this.pnlTitleBar);
            this.pnlContent.Controls.Add(this.pnlBreadcrumb);

            // ── pnlBreadcrumb ───────────────────────────────────────────────────
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlBreadcrumb.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Height = 28;
            this.pnlBreadcrumb.Name = "pnlBreadcrumb";
            this.pnlBreadcrumb.Padding = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);

            this.lblBreadcrumb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBreadcrumb.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Monitor";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBreadcrumb.Name = "lblBreadcrumb";

            // ── pnlTitleBar ─────────────────────────────────────────────────────
            this.pnlTitleBar.BackColor = System.Drawing.Color.White;
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height = 62;
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Padding = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlTitleBar.Controls.Add(this.btnExportHeader);
            this.pnlTitleBar.Controls.Add(this.btnRefresh);
            this.pnlTitleBar.Controls.Add(this.lblSubtitle);
            this.pnlTitleBar.Controls.Add(this.lblTitle);

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitle.Location = new System.Drawing.Point(20, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "งาน Workflow ของฉัน";

            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblSubtitle.Location = new System.Drawing.Point(20, 36);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "รายการงานที่รอดำเนินการของคุณ — อัปเดตล่าสุด 16/04/2569 10:24";

            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnRefresh.BackColor = System.Drawing.Color.White;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnRefresh.Location = new System.Drawing.Point(784, 18);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(102, 26);
            this.btnRefresh.Text = "🗓  สัปดาห์นี้";

            this.btnExportHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnExportHeader.BackColor = System.Drawing.Color.White;
            this.btnExportHeader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportHeader.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnExportHeader.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnExportHeader.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnExportHeader.Location = new System.Drawing.Point(894, 18);
            this.btnExportHeader.Name = "btnExportHeader";
            this.btnExportHeader.Size = new System.Drawing.Size(80, 26);
            this.btnExportHeader.Text = "↓  Export";

            // ── pnlDashboard ────────────────────────────────────────────────────
            this.pnlDashboard.BackColor = System.Drawing.Color.White;
            this.pnlDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.Padding = new System.Windows.Forms.Padding(16, 8, 16, 0);
            this.pnlDashboard.Controls.Add(this.lvTasks);
            this.pnlDashboard.Controls.Add(this.pnlPagination);
            this.pnlDashboard.Controls.Add(this.pnlFilterBar);
            this.pnlDashboard.Controls.Add(this.flpCards);

            // ── flpCards (4 cards) ───────────────────────────────────────────────
            this.flpCards.BackColor = System.Drawing.Color.White;
            this.flpCards.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpCards.Height = 98;
            this.flpCards.Name = "flpCards";
            this.flpCards.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.flpCards.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flpCards.WrapContents = false;
            this.flpCards.Controls.Add(this.pnlCardSLA);
            this.flpCards.Controls.Add(this.pnlCardPending);
            this.flpCards.Controls.Add(this.pnlCardDone);

            // ── pnlCardActive — กำลังทำงาน ────────────────────────────────────────
            this.pnlCardActive.BackColor = System.Drawing.Color.White;
            this.pnlCardActive.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardActive.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlCardActive.Name = "pnlCardActive";
            this.pnlCardActive.Size = new System.Drawing.Size(252, 88);
            this.pnlCardActive.Padding = new System.Windows.Forms.Padding(14, 10, 14, 8);
            this.pnlCardActive.Controls.Add(this.lblCardActiveSub);
            this.pnlCardActive.Controls.Add(this.lblCardActiveCount);
            this.pnlCardActive.Controls.Add(this.lblCardActiveTitle);
            this.pnlCardActive.Controls.Add(this.lblCardActiveIcon);

            this.lblCardActiveIcon.AutoSize = true;
            this.lblCardActiveIcon.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardActiveIcon.Location = new System.Drawing.Point(14, 8);
            this.lblCardActiveIcon.Name = "lblCardActiveIcon";
            this.lblCardActiveIcon.Text = "▶";
            this.lblCardActiveIcon.ForeColor = System.Drawing.Color.FromArgb(25, 100, 195);

            this.lblCardActiveTitle.AutoSize = true;
            this.lblCardActiveTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardActiveTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardActiveTitle.Location = new System.Drawing.Point(40, 12);
            this.lblCardActiveTitle.Name = "lblCardActiveTitle";
            this.lblCardActiveTitle.Text = "กำลังทำงาน";

            this.lblCardActiveCount.AutoSize = true;
            this.lblCardActiveCount.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblCardActiveCount.ForeColor = System.Drawing.Color.FromArgb(25, 100, 195);
            this.lblCardActiveCount.Location = new System.Drawing.Point(14, 28);
            this.lblCardActiveCount.Name = "lblCardActiveCount";
            this.lblCardActiveCount.Text = "24";

            this.lblCardActiveSub.AutoSize = true;
            this.lblCardActiveSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardActiveSub.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.lblCardActiveSub.Location = new System.Drawing.Point(14, 66);
            this.lblCardActiveSub.Name = "lblCardActiveSub";
            this.lblCardActiveSub.Text = "ทั้งระบบ";

            // ── pnlCardSLA — เกิน SLA ──────────────────────────────────────────
            this.pnlCardSLA.BackColor = System.Drawing.Color.White;
            this.pnlCardSLA.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardSLA.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlCardSLA.Name = "pnlCardSLA";
            this.pnlCardSLA.Size = new System.Drawing.Size(328, 82);
            this.pnlCardSLA.Padding = new System.Windows.Forms.Padding(14, 10, 14, 8);
            this.pnlCardSLA.Controls.Add(this.lblCardSLASub);
            this.pnlCardSLA.Controls.Add(this.lblCardSLACount);
            this.pnlCardSLA.Controls.Add(this.lblCardSLATitle);
            this.pnlCardSLA.Controls.Add(this.lblCardSLAIcon);

            this.lblCardSLAIcon.AutoSize = true;
            this.lblCardSLAIcon.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardSLAIcon.Location = new System.Drawing.Point(14, 8);
            this.lblCardSLAIcon.Name = "lblCardSLAIcon";
            this.lblCardSLAIcon.Text = "⚠";
            this.lblCardSLAIcon.ForeColor = System.Drawing.Color.FromArgb(194, 38, 54);

            this.lblCardSLATitle.AutoSize = true;
            this.lblCardSLATitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardSLATitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardSLATitle.Location = new System.Drawing.Point(40, 12);
            this.lblCardSLATitle.Name = "lblCardSLATitle";
            this.lblCardSLATitle.Text = "เกิน SLA";

            this.lblCardSLACount.AutoSize = true;
            this.lblCardSLACount.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblCardSLACount.ForeColor = System.Drawing.Color.FromArgb(194, 38, 54);
            this.lblCardSLACount.Location = new System.Drawing.Point(14, 30);
            this.lblCardSLACount.Name = "lblCardSLACount";
            this.lblCardSLACount.Text = "3";

            this.lblCardSLASub.AutoSize = true;
            this.lblCardSLASub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardSLASub.ForeColor = System.Drawing.Color.FromArgb(194, 38, 54);
            this.lblCardSLASub.Location = new System.Drawing.Point(14, 66);
            this.lblCardSLASub.Name = "lblCardSLASub";
            this.lblCardSLASub.Text = "ต้องดำเนินการด่วน";

            // ── pnlCardDone — เสร็จวันนี้ ──────────────────────────────────────
            this.pnlCardDone.BackColor = System.Drawing.Color.White;
            this.pnlCardDone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardDone.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlCardDone.Name = "pnlCardDone";
            this.pnlCardDone.Size = new System.Drawing.Size(328, 82);
            this.pnlCardDone.Padding = new System.Windows.Forms.Padding(14, 10, 14, 8);
            this.pnlCardDone.Controls.Add(this.lblCardDoneSub);
            this.pnlCardDone.Controls.Add(this.lblCardDoneCount);
            this.pnlCardDone.Controls.Add(this.lblCardDoneTitle);
            this.pnlCardDone.Controls.Add(this.lblCardDoneIcon);

            this.lblCardDoneIcon.AutoSize = true;
            this.lblCardDoneIcon.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardDoneIcon.Location = new System.Drawing.Point(14, 8);
            this.lblCardDoneIcon.Name = "lblCardDoneIcon";
            this.lblCardDoneIcon.Text = "✔";
            this.lblCardDoneIcon.ForeColor = System.Drawing.Color.FromArgb(23, 133, 49);

            this.lblCardDoneTitle.AutoSize = true;
            this.lblCardDoneTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardDoneTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardDoneTitle.Location = new System.Drawing.Point(40, 12);
            this.lblCardDoneTitle.Name = "lblCardDoneTitle";
            this.lblCardDoneTitle.Text = "เสร็จวันนี้";

            this.lblCardDoneCount.AutoSize = true;
            this.lblCardDoneCount.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblCardDoneCount.ForeColor = System.Drawing.Color.FromArgb(23, 133, 49);
            this.lblCardDoneCount.Location = new System.Drawing.Point(14, 30);
            this.lblCardDoneCount.Name = "lblCardDoneCount";
            this.lblCardDoneCount.Text = "5";

            this.lblCardDoneSub.AutoSize = true;
            this.lblCardDoneSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardDoneSub.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.lblCardDoneSub.Location = new System.Drawing.Point(14, 66);
            this.lblCardDoneSub.Name = "lblCardDoneSub";
            this.lblCardDoneSub.Text = "+2 จากเมื่อวาน";

            // ── pnlCardPending — รออนุมัติ ──────────────────────────────────────
            this.pnlCardPending.BackColor = System.Drawing.Color.White;
            this.pnlCardPending.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardPending.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.pnlCardPending.Name = "pnlCardPending";
            this.pnlCardPending.Size = new System.Drawing.Size(328, 82);
            this.pnlCardPending.Padding = new System.Windows.Forms.Padding(14, 10, 14, 8);
            this.pnlCardPending.Controls.Add(this.lblCardPendingSub);
            this.pnlCardPending.Controls.Add(this.lblCardPendingCount);
            this.pnlCardPending.Controls.Add(this.lblCardPendingTitle);
            this.pnlCardPending.Controls.Add(this.lblCardPendingIcon);

            this.lblCardPendingIcon.AutoSize = true;
            this.lblCardPendingIcon.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblCardPendingIcon.Location = new System.Drawing.Point(14, 8);
            this.lblCardPendingIcon.Name = "lblCardPendingIcon";
            this.lblCardPendingIcon.Text = "⏰";
            this.lblCardPendingIcon.ForeColor = System.Drawing.Color.FromArgb(197, 120, 16);

            this.lblCardPendingTitle.AutoSize = true;
            this.lblCardPendingTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCardPendingTitle.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.lblCardPendingTitle.Location = new System.Drawing.Point(40, 12);
            this.lblCardPendingTitle.Name = "lblCardPendingTitle";
            this.lblCardPendingTitle.Text = "รอดำเนินการ";

            this.lblCardPendingCount.AutoSize = true;
            this.lblCardPendingCount.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblCardPendingCount.ForeColor = System.Drawing.Color.FromArgb(197, 120, 16);
            this.lblCardPendingCount.Location = new System.Drawing.Point(14, 30);
            this.lblCardPendingCount.Name = "lblCardPendingCount";
            this.lblCardPendingCount.Text = "8";

            this.lblCardPendingSub.AutoSize = true;
            this.lblCardPendingSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardPendingSub.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.lblCardPendingSub.Location = new System.Drawing.Point(14, 66);
            this.lblCardPendingSub.Name = "lblCardPendingSub";
            this.lblCardPendingSub.Text = "งานของคุณในวันนี้";

            // ── pnlFilterBar ────────────────────────────────────────────────────
            this.pnlFilterBar.BackColor = System.Drawing.Color.White;
            this.pnlFilterBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilterBar.Height = 44;
            this.pnlFilterBar.Name = "pnlFilterBar";
            this.pnlFilterBar.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);

            // สถานะ:
            this.lblFilterStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFilterStatus.ForeColor = System.Drawing.Color.FromArgb(60, 70, 90);
            this.lblFilterStatus.Location = new System.Drawing.Point(12, 14);
            this.lblFilterStatus.Name = "lblFilterStatus";
            this.lblFilterStatus.Size = new System.Drawing.Size(54, 22);
            this.lblFilterStatus.Text = "ตัวกรอง:";
            this.lblFilterStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.cmbFilterStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbFilterStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbFilterStatus.Items.AddRange(new object[] { "ทั้งหมด", "เกิน SLA", "รอดำเนินการ", "รอ", "เสร็จ" });
            this.cmbFilterStatus.SelectedIndex = 0;
            this.cmbFilterStatus.Location = new System.Drawing.Point(68, 9);
            this.cmbFilterStatus.Name = "cmbFilterStatus";
            this.cmbFilterStatus.Size = new System.Drawing.Size(110, 24);

            // Template:
            this.lblFilterTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFilterTemplate.ForeColor = System.Drawing.Color.FromArgb(60, 70, 90);
            this.lblFilterTemplate.Location = new System.Drawing.Point(184, 12);
            this.lblFilterTemplate.Name = "lblFilterTemplate";
            this.lblFilterTemplate.Size = new System.Drawing.Size(92, 22);
            this.lblFilterTemplate.Text = "ประเภท Workflow";
            this.lblFilterTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.cmbFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbFilterType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbFilterType.Items.AddRange(new object[] { "ทั้งหมด", "KB Article", "Upload ไฟล์ 1HC", "เปลี่ยน Permission", "Review SOP", "Work Instruction" });
            this.cmbFilterType.SelectedIndex = 0;
            this.cmbFilterType.Location = new System.Drawing.Point(278, 9);
            this.cmbFilterType.Name = "cmbFilterType";
            this.cmbFilterType.Size = new System.Drawing.Size(160, 24);

            // ผู้ยื่น:
            this.lblFilterAssignee.Visible = false;

            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.txtSearch.Location = new System.Drawing.Point(446, 9);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(150, 24);
            this.txtSearch.Text = "ค้นหา...";

            // วันที่:
            this.lblFilterDateFrom.Visible = false;

            this.txtDateFrom.Visible = false;

            this.lblFilterDateTo.Visible = false;

            this.txtDateTo.Visible = false;

            this.btnSearch.Visible = false;

            this.btnClearFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnClearFilter.BackColor = System.Drawing.Color.White;
            this.btnClearFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFilter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnClearFilter.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnClearFilter.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.btnClearFilter.Location = new System.Drawing.Point(872, 8);
            this.btnClearFilter.Name = "btnClearFilter";
            this.btnClearFilter.Size = new System.Drawing.Size(136, 26);
            this.btnClearFilter.Text = "⏷  ล้างตัวกรอง";

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

            // ── lvTasks ─────────────────────────────────────────────────────────
            this.lvTasks.BackColor = System.Drawing.Color.White;
            this.lvTasks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvTasks.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lvTasks.FullRowSelect = true;
            this.lvTasks.GridLines = false;
            this.lvTasks.HideSelection = false;
            this.lvTasks.MultiSelect = true;
            this.lvTasks.Name = "lvTasks";
            this.lvTasks.OwnerDraw = true;
            this.lvTasks.View = System.Windows.Forms.View.Details;
            this.lvTasks.ShowItemToolTips = true;
            this.lvTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colId, this.colWorkflow, this.colAssignee,
                this.colStart, this.colStep, this.colCurrentStep, this.colStatus });

            this.colId.Text = ""; this.colId.Width = 34;
            this.colWorkflow.Text = "รหัส"; this.colWorkflow.Width = 150;
            this.colAssignee.Text = "Workflow / Step"; this.colAssignee.Width = 260;
            this.colStart.Text = "ผู้รับผิดชอบ"; this.colStart.Width = 210;
            this.colStep.Text = "กำหนด"; this.colStep.Width = 120;
            this.colCurrentStep.Text = "SLA"; this.colCurrentStep.Width = 160;
            this.colStatus.Text = "สถานะ"; this.colStatus.Width = 150;

            // ── pnlPagination ───────────────────────────────────────────────────
            this.pnlPagination.BackColor = System.Drawing.Color.White;
            this.pnlPagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlPagination.Height = 44;
            this.pnlPagination.Name = "pnlPagination";
            this.pnlPagination.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlPagination.Controls.Add(this.btnNext);
            this.pnlPagination.Controls.Add(this.lblPage);
            this.pnlPagination.Controls.Add(this.btnPrev);
            this.pnlPagination.Controls.Add(this.lblSelectionInfo);

            this.lblSelectionInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSelectionInfo.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblSelectionInfo.Location = new System.Drawing.Point(12, 12);
            this.lblSelectionInfo.Name = "lblSelectionInfo";
            this.lblSelectionInfo.Size = new System.Drawing.Size(320, 22);
            this.lblSelectionInfo.Text = "เลือก 1 รายการจาก 8 รายการ";

            this.btnPrev.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnPrev.BackColor = System.Drawing.Color.White;
            this.btnPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrev.FlatAppearance.BorderSize = 0;
            this.btnPrev.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnPrev.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnPrev.Location = new System.Drawing.Point(790, 8);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(62, 28);
            this.btnPrev.Text = "←  ก่อนหน้า";

            this.lblPage.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.lblPage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPage.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.lblPage.Location = new System.Drawing.Point(854, 12);
            this.lblPage.Name = "lblPage";
            this.lblPage.Size = new System.Drawing.Size(76, 22);
            this.lblPage.Text = "หน้า 1 / 2";
            this.lblPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnNext.BackColor = System.Drawing.Color.White;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnNext.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnNext.Location = new System.Drawing.Point(936, 8);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(66, 28);
            this.btnNext.Text = "ถัดไป  →";

            // ── Form ────────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1000, 650);
            this.Name = "WorkflowDashboardFormNew";
            this.Text = "ตรวจสอบ Workflow — Core Management System";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.WorkflowDashboardFormNew_Load);

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
            this.ResumeLayout(false);
        }
    }
}
