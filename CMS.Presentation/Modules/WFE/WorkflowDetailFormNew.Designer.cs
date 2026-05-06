namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowDetailFormNew
    {
        private System.ComponentModel.IContainer components = null;

        // ── Top chrome ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlRibbonTabs;
        private System.Windows.Forms.Panel pnlRibbonBorder;
        private System.Windows.Forms.Panel pnlToolbarContainer;
        private System.Windows.Forms.Panel pnlToolbarBorder;
        private System.Windows.Forms.TextBox txtGlobalSearch;
        private System.Windows.Forms.Label lblUserName;

        // ── App body ────────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlAppBody;
        private System.Windows.Forms.Panel pnlSidebarContainer;
        private System.Windows.Forms.Panel pnlNavBorder;
        private System.Windows.Forms.Panel pnlContent;

        // ── Content: breadcrumb + title bar ────────────────────────────────────
        private System.Windows.Forms.Panel pnlBreadcrumb;
        private System.Windows.Forms.Label lblBreadcrumb;
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblStatusBadge;

        // ── Detail body ─────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlDetailBody;

        // ── Top cards row ───────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTopCards;

        // Left card: ข้อมูล Workflow ─────────────────────────────────────────
        private System.Windows.Forms.Panel pnlCardLeft;
        private System.Windows.Forms.Label lblCardLeftTitle;
        private System.Windows.Forms.Panel pnlCardLeftDivider;
        private System.Windows.Forms.Label lblWFIDLabel;
        private System.Windows.Forms.Label lblWFIDValue;
        private System.Windows.Forms.Label lblTemplateLabel;
        private System.Windows.Forms.Label lblTemplateValue;
        private System.Windows.Forms.Label lblInitiatorLabel;
        private System.Windows.Forms.Label lblInitiatorValue;
        private System.Windows.Forms.Label lblInitiatorRoleValue;
        private System.Windows.Forms.Label lblStartDateLabel;
        private System.Windows.Forms.Label lblStartDateValue;
        private System.Windows.Forms.Label lblDueDateLabel;
        private System.Windows.Forms.Label lblDueDateValue;
        private System.Windows.Forms.Label lblLinkedDocLabel;
        private System.Windows.Forms.Label lblLinkedDocValue;
        private System.Windows.Forms.Label lblCRAFLabel;
        private System.Windows.Forms.Label lblCRAFValue;

        // Right card: ความคืบหน้า ───────────────────────────────────────────
        private System.Windows.Forms.Panel pnlCardRight;
        private System.Windows.Forms.Label lblCardRightTitle;
        private System.Windows.Forms.Label lblStepBadge;
        private System.Windows.Forms.Panel pnlCardRightDivider;
        private System.Windows.Forms.Label lblProgressLabel;
        private System.Windows.Forms.ProgressBar prgProgress;
        private System.Windows.Forms.Label lblProgressPct;
        private System.Windows.Forms.Label lblCurrentStepLabel;
        private System.Windows.Forms.Label lblCurrentStepValue;
        private System.Windows.Forms.Label lblAssigneeLabel;
        private System.Windows.Forms.Label lblAssigneeValue;
        private System.Windows.Forms.Label lblAssigneeRoleValue;
        private System.Windows.Forms.Label lblDeadlineLabel;
        private System.Windows.Forms.Label lblDeadlineValue;
        private System.Windows.Forms.Label lblSLALabel;
        private System.Windows.Forms.Label lblSLAValue;

        // ── Extra runtime controls ─────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlRibbonAvatar;
        private System.Windows.Forms.Panel pnlInitiatorAvatar;
        private System.Windows.Forms.Panel pnlAssigneeAvatar;
        private System.Windows.Forms.Panel pnlSlaBar;
        private System.Windows.Forms.Button btnMoreActions;
        private System.Windows.Forms.Label lblStartedValue;

        // ── Timeline card ────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTimelineCard;
        private System.Windows.Forms.Panel pnlTimelineHeader;
        private System.Windows.Forms.Label lblTimelineTitle;
        private System.Windows.Forms.Panel pnlTimelineTabs;
        private System.Windows.Forms.Panel pnlTimelineDivider;
        private System.Windows.Forms.Panel pnlTimelineScroll;
        private System.Windows.Forms.Panel pnlTimelineContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlRibbonTabs = new System.Windows.Forms.Panel();
            this.pnlRibbonBorder = new System.Windows.Forms.Panel();
            this.pnlToolbarContainer = new System.Windows.Forms.Panel();
            this.pnlToolbarBorder = new System.Windows.Forms.Panel();
            this.txtGlobalSearch = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.pnlAppBody = new System.Windows.Forms.Panel();
            this.pnlSidebarContainer = new System.Windows.Forms.Panel();
            this.pnlNavBorder = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlBreadcrumb = new System.Windows.Forms.Panel();
            this.lblBreadcrumb = new System.Windows.Forms.Label();
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblStatusBadge = new System.Windows.Forms.Label();
            this.pnlDetailBody = new System.Windows.Forms.Panel();
            this.pnlTopCards = new System.Windows.Forms.Panel();
            this.pnlCardLeft = new System.Windows.Forms.Panel();
            this.lblCardLeftTitle = new System.Windows.Forms.Label();
            this.pnlCardLeftDivider = new System.Windows.Forms.Panel();
            this.lblWFIDLabel = new System.Windows.Forms.Label();
            this.lblWFIDValue = new System.Windows.Forms.Label();
            this.lblTemplateLabel = new System.Windows.Forms.Label();
            this.lblTemplateValue = new System.Windows.Forms.Label();
            this.lblInitiatorLabel = new System.Windows.Forms.Label();
            this.lblInitiatorValue = new System.Windows.Forms.Label();
            this.lblInitiatorRoleValue = new System.Windows.Forms.Label();
            this.lblStartDateLabel = new System.Windows.Forms.Label();
            this.lblStartDateValue = new System.Windows.Forms.Label();
            this.lblDueDateLabel = new System.Windows.Forms.Label();
            this.lblDueDateValue = new System.Windows.Forms.Label();
            this.lblLinkedDocLabel = new System.Windows.Forms.Label();
            this.lblLinkedDocValue = new System.Windows.Forms.Label();
            this.lblCRAFLabel = new System.Windows.Forms.Label();
            this.lblCRAFValue = new System.Windows.Forms.Label();
            this.pnlCardRight = new System.Windows.Forms.Panel();
            this.lblCardRightTitle = new System.Windows.Forms.Label();
            this.lblStepBadge = new System.Windows.Forms.Label();
            this.pnlCardRightDivider = new System.Windows.Forms.Panel();
            this.lblProgressLabel = new System.Windows.Forms.Label();
            this.prgProgress = new System.Windows.Forms.ProgressBar();
            this.lblProgressPct = new System.Windows.Forms.Label();
            this.lblCurrentStepLabel = new System.Windows.Forms.Label();
            this.lblCurrentStepValue = new System.Windows.Forms.Label();
            this.lblAssigneeLabel = new System.Windows.Forms.Label();
            this.lblAssigneeValue = new System.Windows.Forms.Label();
            this.lblAssigneeRoleValue = new System.Windows.Forms.Label();
            this.lblDeadlineLabel = new System.Windows.Forms.Label();
            this.lblDeadlineValue = new System.Windows.Forms.Label();
            this.lblSLALabel = new System.Windows.Forms.Label();
            this.lblSLAValue = new System.Windows.Forms.Label();
            this.pnlTimelineCard = new System.Windows.Forms.Panel();
            this.pnlTimelineHeader = new System.Windows.Forms.Panel();
            this.lblTimelineTitle = new System.Windows.Forms.Label();
            this.pnlTimelineTabs = new System.Windows.Forms.Panel();
            this.pnlTimelineDivider = new System.Windows.Forms.Panel();
            this.pnlTimelineScroll = new System.Windows.Forms.Panel();
            this.pnlTimelineContent = new System.Windows.Forms.Panel();
            this.pnlRibbonAvatar = new System.Windows.Forms.Panel();
            this.pnlInitiatorAvatar = new System.Windows.Forms.Panel();
            this.pnlAssigneeAvatar = new System.Windows.Forms.Panel();
            this.pnlSlaBar = new System.Windows.Forms.Panel();
            this.btnMoreActions = new System.Windows.Forms.Button();
            this.lblStartedValue = new System.Windows.Forms.Label();

            this.SuspendLayout();

            // ── pnlRibbonTabs ───────────────────────────────────────────────────
            this.pnlRibbonTabs.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlRibbonTabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabs.Height = 32;
            this.pnlRibbonTabs.Name = "pnlRibbonTabs";
            this.pnlRibbonTabs.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.pnlRibbonTabs.Controls.Add(this.txtGlobalSearch);
            this.pnlRibbonTabs.Controls.Add(this.lblUserName);

            // txtGlobalSearch
            this.txtGlobalSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.txtGlobalSearch.BackColor = System.Drawing.Color.FromArgb(46, 73, 118);
            this.txtGlobalSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtGlobalSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtGlobalSearch.ForeColor = System.Drawing.Color.White;
            this.txtGlobalSearch.Location = new System.Drawing.Point(705, 5);
            this.txtGlobalSearch.Size = new System.Drawing.Size(290, 23);
            this.txtGlobalSearch.Name = "txtGlobalSearch";
            this.txtGlobalSearch.GotFocus += new System.EventHandler(this.txtGlobalSearch_GotFocus);
            this.txtGlobalSearch.LostFocus += new System.EventHandler(this.txtGlobalSearch_LostFocus);

            // lblUserName
            this.lblUserName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblUserName.AutoSize = true;
            this.lblUserName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblUserName.ForeColor = System.Drawing.Color.White;
            this.lblUserName.Location = new System.Drawing.Point(1015, 9);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Text = "อรวรรณ สุวรรณ (Admin)";

            // pnlRibbonAvatar (circular avatar in ribbon, anchored right)
            this.pnlRibbonAvatar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.pnlRibbonAvatar.BackColor = System.Drawing.Color.FromArgb(90, 120, 170);
            this.pnlRibbonAvatar.Location = new System.Drawing.Point(985, 4);
            this.pnlRibbonAvatar.Size = new System.Drawing.Size(26, 26);
            this.pnlRibbonAvatar.Name = "pnlRibbonAvatar";
            this.pnlRibbonAvatar.Tag = "AS|90,120,170";
            this.pnlRibbonTabs.Controls.Add(this.pnlRibbonAvatar);

            // ── pnlRibbonBorder ─────────────────────────────────────────────────
            this.pnlRibbonBorder.BackColor = System.Drawing.Color.FromArgb(200, 215, 240);
            this.pnlRibbonBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonBorder.Height = 1;
            this.pnlRibbonBorder.Name = "pnlRibbonBorder";

            // ── pnlToolbarContainer ─────────────────────────────────────────────
            this.pnlToolbarContainer.BackColor = System.Drawing.Color.White;
            this.pnlToolbarContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarContainer.Height = 78;
            this.pnlToolbarContainer.Name = "pnlToolbarContainer";
            this.pnlToolbarContainer.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);

            // ── pnlToolbarBorder ────────────────────────────────────────────────
            this.pnlToolbarBorder.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.pnlToolbarBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarBorder.Height = 1;
            this.pnlToolbarBorder.Name = "pnlToolbarBorder";

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
            this.pnlContent.Controls.Add(this.pnlDetailBody);
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
            this.lblBreadcrumb.Name = "lblBreadcrumb";
            this.lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Monitor  ›  WFI-2569-00012";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── pnlTitleBar ─────────────────────────────────────────────────────
            this.pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height = 64;
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Padding = new System.Windows.Forms.Padding(20, 6, 16, 0);
            this.pnlTitleBar.Controls.Add(this.lblStatusBadge);
            this.pnlTitleBar.Controls.Add(this.lblSubtitle);
            this.pnlTitleBar.Controls.Add(this.lblTitle);

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitle.Location = new System.Drawing.Point(20, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "WFI-2569-00012 · อนุมัติ KB Article";

            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblSubtitle.Location = new System.Drawing.Point(20, 38);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "Instance ของ Template WFD-003 v1 - Frozen Snapshot";

            this.lblStatusBadge.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblStatusBadge.BackColor = System.Drawing.Color.FromArgb(255, 242, 222);
            this.lblStatusBadge.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(181, 116, 27);
            this.lblStatusBadge.Location = new System.Drawing.Point(800, 18);
            this.lblStatusBadge.Size = new System.Drawing.Size(130, 26);
            this.lblStatusBadge.Name = "lblStatusBadge";
            this.lblStatusBadge.Text = "● รอดำเนินการ";
            this.lblStatusBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── pnlDetailBody ───────────────────────────────────────────────────
            this.pnlDetailBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlDetailBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetailBody.Name = "pnlDetailBody";
            this.pnlDetailBody.Padding = new System.Windows.Forms.Padding(16, 0, 16, 16);
            this.pnlDetailBody.Controls.Add(this.pnlTimelineCard);
            this.pnlDetailBody.Controls.Add(this.pnlTopCards);

            // ── pnlTopCards ─────────────────────────────────────────────────────
            this.pnlTopCards.BackColor = System.Drawing.Color.Transparent;
            this.pnlTopCards.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopCards.Height = 260;
            this.pnlTopCards.Name = "pnlTopCards";
            this.pnlTopCards.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.pnlTopCards.Controls.Add(this.pnlCardRight);
            this.pnlTopCards.Controls.Add(this.pnlCardLeft);

            // ══ LEFT CARD ════════════════════════════════════════════════════════
            this.pnlCardLeft.BackColor = System.Drawing.Color.White;
            this.pnlCardLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlCardLeft.Width = 490;
            this.pnlCardLeft.Name = "pnlCardLeft";
            this.pnlCardLeft.Controls.Add(this.pnlCardLeftDivider);
            this.pnlCardLeft.Controls.Add(this.lblCardLeftTitle);
            this.pnlCardLeft.Controls.Add(this.lblWFIDLabel);
            this.pnlCardLeft.Controls.Add(this.lblWFIDValue);
            this.pnlCardLeft.Controls.Add(this.lblTemplateLabel);
            this.pnlCardLeft.Controls.Add(this.lblTemplateValue);
            this.pnlCardLeft.Controls.Add(this.lblInitiatorLabel);
            this.pnlCardLeft.Controls.Add(this.lblInitiatorValue);
            this.pnlCardLeft.Controls.Add(this.lblInitiatorRoleValue);
            this.pnlCardLeft.Controls.Add(this.lblStartDateLabel);
            this.pnlCardLeft.Controls.Add(this.lblStartDateValue);
            this.pnlCardLeft.Controls.Add(this.lblDueDateLabel);
            this.pnlCardLeft.Controls.Add(this.lblDueDateValue);
            this.pnlCardLeft.Controls.Add(this.lblLinkedDocLabel);
            this.pnlCardLeft.Controls.Add(this.lblLinkedDocValue);
            this.pnlCardLeft.Controls.Add(this.lblCRAFLabel);
            this.pnlCardLeft.Controls.Add(this.lblCRAFValue);

            // card title
            this.lblCardLeftTitle.AutoSize = true;
            this.lblCardLeftTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCardLeftTitle.ForeColor = System.Drawing.Color.FromArgb(30, 40, 65);
            this.lblCardLeftTitle.Location = new System.Drawing.Point(18, 14);
            this.lblCardLeftTitle.Name = "lblCardLeftTitle";
            this.lblCardLeftTitle.Text = "ข้อมูล Workflow";

            // divider
            this.pnlCardLeftDivider.BackColor = System.Drawing.Color.FromArgb(225, 228, 236);
            this.pnlCardLeftDivider.Location = new System.Drawing.Point(18, 36);
            this.pnlCardLeftDivider.Size = new System.Drawing.Size(450, 1);
            this.pnlCardLeftDivider.Name = "pnlCardLeftDivider";

            // row: WF ID
            this.lblWFIDLabel.AutoSize = true;
            this.lblWFIDLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblWFIDLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblWFIDLabel.Location = new System.Drawing.Point(18, 46);
            this.lblWFIDLabel.Name = "lblWFIDLabel";
            this.lblWFIDLabel.Text = "รหัส";
            this.lblWFIDValue.AutoEllipsis = true;
            this.lblWFIDValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWFIDValue.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblWFIDValue.Location = new System.Drawing.Point(158, 46);
            this.lblWFIDValue.Size = new System.Drawing.Size(300, 16);
            this.lblWFIDValue.Name = "lblWFIDValue";
            this.lblWFIDValue.Text = "WFI-2569-00012";

            // row: Template
            this.lblTemplateLabel.AutoSize = true;
            this.lblTemplateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTemplateLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblTemplateLabel.Location = new System.Drawing.Point(18, 72);
            this.lblTemplateLabel.Name = "lblTemplateLabel";
            this.lblTemplateLabel.Text = "Template";
            this.lblTemplateValue.AutoEllipsis = true;
            this.lblTemplateValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTemplateValue.ForeColor = System.Drawing.Color.FromArgb(0, 90, 158);
            this.lblTemplateValue.Location = new System.Drawing.Point(158, 72);
            this.lblTemplateValue.Size = new System.Drawing.Size(300, 16);
            this.lblTemplateValue.Name = "lblTemplateValue";
            this.lblTemplateValue.Text = "WFD-003 · อนุมัติ KB Article v1";

            // row: Initiator
            this.lblInitiatorLabel.AutoSize = true;
            this.lblInitiatorLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblInitiatorLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblInitiatorLabel.Location = new System.Drawing.Point(18, 98);
            this.lblInitiatorLabel.Name = "lblInitiatorLabel";
            this.lblInitiatorLabel.Text = "ผู้ยื่น";
            this.lblInitiatorValue.AutoEllipsis = true;
            this.lblInitiatorValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblInitiatorValue.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblInitiatorValue.Location = new System.Drawing.Point(158, 98);
            this.lblInitiatorValue.Size = new System.Drawing.Size(300, 16);
            this.lblInitiatorValue.Name = "lblInitiatorValue";
            this.lblInitiatorValue.Text = "สมชาย กาญจนา";
            // role sub-label
            this.lblInitiatorRoleValue.AutoSize = true;
            this.lblInitiatorRoleValue.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInitiatorRoleValue.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblInitiatorRoleValue.Location = new System.Drawing.Point(158, 114);
            this.lblInitiatorRoleValue.Name = "lblInitiatorRoleValue";
            this.lblInitiatorRoleValue.Text = "Officer · IT";

            // row: Start Date
            this.lblStartDateLabel.AutoSize = true;
            this.lblStartDateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStartDateLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblStartDateLabel.Location = new System.Drawing.Point(18, 136);
            this.lblStartDateLabel.Name = "lblStartDateLabel";
            this.lblStartDateLabel.Text = "เริ่มต้น";
            this.lblStartDateValue.AutoEllipsis = true;
            this.lblStartDateValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStartDateValue.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblStartDateValue.Location = new System.Drawing.Point(158, 136);
            this.lblStartDateValue.Size = new System.Drawing.Size(300, 16);
            this.lblStartDateValue.Name = "lblStartDateValue";
            this.lblStartDateValue.Text = "15/04/2569 09:30";

            // row: Due Date
            this.lblDueDateLabel.AutoSize = true;
            this.lblDueDateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDueDateLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblDueDateLabel.Location = new System.Drawing.Point(18, 162);
            this.lblDueDateLabel.Name = "lblDueDateLabel";
            this.lblDueDateLabel.Text = "กำหนดเสร็จ";
            this.lblDueDateValue.AutoEllipsis = true;
            this.lblDueDateValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDueDateValue.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblDueDateValue.Location = new System.Drawing.Point(158, 162);
            this.lblDueDateValue.Size = new System.Drawing.Size(300, 16);
            this.lblDueDateValue.Name = "lblDueDateValue";
            this.lblDueDateValue.Text = "22/04/2569 17:00 (รวม)";

            // row: Linked Doc
            this.lblLinkedDocLabel.AutoSize = true;
            this.lblLinkedDocLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLinkedDocLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblLinkedDocLabel.Location = new System.Drawing.Point(18, 188);
            this.lblLinkedDocLabel.Name = "lblLinkedDocLabel";
            this.lblLinkedDocLabel.Text = "เชื่อมโยง";
            this.lblLinkedDocValue.AutoEllipsis = true;
            this.lblLinkedDocValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLinkedDocValue.ForeColor = System.Drawing.Color.FromArgb(0, 90, 158);
            this.lblLinkedDocValue.Location = new System.Drawing.Point(158, 188);
            this.lblLinkedDocValue.Size = new System.Drawing.Size(300, 16);
            this.lblLinkedDocValue.Name = "lblLinkedDocValue";
            this.lblLinkedDocValue.Text = "KBArticle#1234  คู่มือ PDPA";

            // row: CRAF
            this.lblCRAFLabel.AutoSize = true;
            this.lblCRAFLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCRAFLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblCRAFLabel.Location = new System.Drawing.Point(18, 214);
            this.lblCRAFLabel.Name = "lblCRAFLabel";
            this.lblCRAFLabel.Text = "CRAF";
            this.lblCRAFValue.AutoEllipsis = true;
            this.lblCRAFValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCRAFValue.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblCRAFValue.Location = new System.Drawing.Point(158, 214);
            this.lblCRAFValue.Size = new System.Drawing.Size(300, 16);
            this.lblCRAFValue.Name = "lblCRAFValue";
            this.lblCRAFValue.Text = "3NC · General";

            // lblStartedValue (duplicate start time shown in left card footer)
            this.lblStartedValue.AutoSize = true;
            this.lblStartedValue.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStartedValue.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblStartedValue.Location = new System.Drawing.Point(158, 232);
            this.lblStartedValue.Name = "lblStartedValue";
            this.lblStartedValue.Text = "15/04/2569 09:32";
            this.pnlCardLeft.Controls.Add(this.lblStartedValue);

            // pnlInitiatorAvatar (circle in left card near initiator row)
            this.pnlInitiatorAvatar.BackColor = System.Drawing.Color.FromArgb(230, 115, 0);
            this.pnlInitiatorAvatar.Location = new System.Drawing.Point(136, 96);
            this.pnlInitiatorAvatar.Size = new System.Drawing.Size(22, 22);
            this.pnlInitiatorAvatar.Name = "pnlInitiatorAvatar";
            this.pnlInitiatorAvatar.Tag = "สก|230,115,0";
            this.pnlCardLeft.Controls.Add(this.pnlInitiatorAvatar);

            // ══ RIGHT CARD ═══════════════════════════════════════════════════════
            this.pnlCardRight.BackColor = System.Drawing.Color.White;
            this.pnlCardRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCardRight.Name = "pnlCardRight";
            this.pnlCardRight.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.pnlCardRight.Controls.Add(this.pnlCardRightDivider);
            this.pnlCardRight.Controls.Add(this.lblStepBadge);
            this.pnlCardRight.Controls.Add(this.lblCardRightTitle);
            this.pnlCardRight.Controls.Add(this.lblProgressLabel);
            this.pnlCardRight.Controls.Add(this.prgProgress);
            this.pnlCardRight.Controls.Add(this.lblProgressPct);
            this.pnlCardRight.Controls.Add(this.lblCurrentStepLabel);
            this.pnlCardRight.Controls.Add(this.lblCurrentStepValue);
            this.pnlCardRight.Controls.Add(this.lblAssigneeLabel);
            this.pnlCardRight.Controls.Add(this.lblAssigneeValue);
            this.pnlCardRight.Controls.Add(this.lblAssigneeRoleValue);
            this.pnlCardRight.Controls.Add(this.lblDeadlineLabel);
            this.pnlCardRight.Controls.Add(this.lblDeadlineValue);
            this.pnlCardRight.Controls.Add(this.lblSLALabel);
            this.pnlCardRight.Controls.Add(this.lblSLAValue);

            // card title
            this.lblCardRightTitle.AutoSize = true;
            this.lblCardRightTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCardRightTitle.ForeColor = System.Drawing.Color.FromArgb(30, 40, 65);
            this.lblCardRightTitle.Location = new System.Drawing.Point(18, 14);
            this.lblCardRightTitle.Name = "lblCardRightTitle";
            this.lblCardRightTitle.Text = "ความคืบหน้า";

            this.lblStepBadge.BackColor = System.Drawing.Color.FromArgb(255, 242, 222);
            this.lblStepBadge.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblStepBadge.ForeColor = System.Drawing.Color.FromArgb(181, 116, 27);
            this.lblStepBadge.Location = new System.Drawing.Point(200, 15);
            this.lblStepBadge.Size = new System.Drawing.Size(60, 18);
            this.lblStepBadge.Name = "lblStepBadge";
            this.lblStepBadge.Text = "Step 2/3";
            this.lblStepBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // divider
            this.pnlCardRightDivider.BackColor = System.Drawing.Color.FromArgb(225, 228, 236);
            this.pnlCardRightDivider.Location = new System.Drawing.Point(18, 36);
            this.pnlCardRightDivider.Size = new System.Drawing.Size(400, 1);
            this.pnlCardRightDivider.Name = "pnlCardRightDivider";

            // progress bar
            this.lblProgressLabel.AutoSize = true;
            this.lblProgressLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProgressLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblProgressLabel.Location = new System.Drawing.Point(18, 46);
            this.lblProgressLabel.Name = "lblProgressLabel";
            this.lblProgressLabel.Text = "Progress";

            this.prgProgress.Location = new System.Drawing.Point(18, 64);
            this.prgProgress.Name = "prgProgress";
            this.prgProgress.Size = new System.Drawing.Size(360, 14);
            this.prgProgress.Value = 55;

            this.lblProgressPct.AutoSize = true;
            this.lblProgressPct.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProgressPct.ForeColor = System.Drawing.Color.FromArgb(32, 60, 103);
            this.lblProgressPct.Location = new System.Drawing.Point(382, 62);
            this.lblProgressPct.Name = "lblProgressPct";
            this.lblProgressPct.Text = "55%";

            // row: Current Step
            this.lblCurrentStepLabel.AutoSize = true;
            this.lblCurrentStepLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCurrentStepLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblCurrentStepLabel.Location = new System.Drawing.Point(18, 88);
            this.lblCurrentStepLabel.Name = "lblCurrentStepLabel";
            this.lblCurrentStepLabel.Text = "Step ปัจจุบัน";
            this.lblCurrentStepValue.AutoEllipsis = true;
            this.lblCurrentStepValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCurrentStepValue.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblCurrentStepValue.Location = new System.Drawing.Point(158, 88);
            this.lblCurrentStepValue.Size = new System.Drawing.Size(280, 16);
            this.lblCurrentStepValue.Name = "lblCurrentStepValue";
            this.lblCurrentStepValue.Text = "ผู้จัดการตรวจสอบเนื้อหา";

            // row: Assignee
            this.lblAssigneeLabel.AutoSize = true;
            this.lblAssigneeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAssigneeLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblAssigneeLabel.Location = new System.Drawing.Point(18, 116);
            this.lblAssigneeLabel.Name = "lblAssigneeLabel";
            this.lblAssigneeLabel.Text = "ผู้รับผิดชอบ";
            this.lblAssigneeValue.AutoEllipsis = true;
            this.lblAssigneeValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAssigneeValue.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblAssigneeValue.Location = new System.Drawing.Point(158, 116);
            this.lblAssigneeValue.Size = new System.Drawing.Size(280, 16);
            this.lblAssigneeValue.Name = "lblAssigneeValue";
            this.lblAssigneeValue.Text = "วิชัย มีสุข";
            // role sub-label
            this.lblAssigneeRoleValue.AutoSize = true;
            this.lblAssigneeRoleValue.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblAssigneeRoleValue.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblAssigneeRoleValue.Location = new System.Drawing.Point(158, 132);
            this.lblAssigneeRoleValue.Name = "lblAssigneeRoleValue";
            this.lblAssigneeRoleValue.Text = "Manager · Legal";

            // row: Deadline
            this.lblDeadlineLabel.AutoSize = true;
            this.lblDeadlineLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDeadlineLabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblDeadlineLabel.Location = new System.Drawing.Point(18, 154);
            this.lblDeadlineLabel.Name = "lblDeadlineLabel";
            this.lblDeadlineLabel.Text = "กำหนด";
            this.lblDeadlineValue.AutoEllipsis = true;
            this.lblDeadlineValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDeadlineValue.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblDeadlineValue.Location = new System.Drawing.Point(158, 154);
            this.lblDeadlineValue.Size = new System.Drawing.Size(280, 16);
            this.lblDeadlineValue.Name = "lblDeadlineValue";
            this.lblDeadlineValue.Text = "20/04/2569 17:00";

            // row: SLA
            this.lblSLALabel.AutoSize = true;
            this.lblSLALabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSLALabel.ForeColor = System.Drawing.Color.FromArgb(120, 126, 136);
            this.lblSLALabel.Location = new System.Drawing.Point(18, 180);
            this.lblSLALabel.Name = "lblSLALabel";
            this.lblSLALabel.Text = "SLA";
            this.lblSLAValue.AutoSize = true;
            this.lblSLAValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSLAValue.ForeColor = System.Drawing.Color.FromArgb(197, 120, 16);
            this.lblSLAValue.Location = new System.Drawing.Point(158, 180);
            this.lblSLAValue.Name = "lblSLAValue";
            this.lblSLAValue.Text = "เหลือ 3 วัน";

            // pnlAssigneeAvatar (circle in right card near assignee row)
            this.pnlAssigneeAvatar.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.pnlAssigneeAvatar.Location = new System.Drawing.Point(136, 114);
            this.pnlAssigneeAvatar.Size = new System.Drawing.Size(22, 22);
            this.pnlAssigneeAvatar.Name = "pnlAssigneeAvatar";
            this.pnlAssigneeAvatar.Tag = "วม|41,128,185";
            this.pnlCardRight.Controls.Add(this.pnlAssigneeAvatar);

            // pnlSlaBar (custom-drawn SLA progress bar in right card)
            this.pnlSlaBar.BackColor = System.Drawing.Color.FromArgb(240, 243, 248);
            this.pnlSlaBar.Location = new System.Drawing.Point(18, 202);
            this.pnlSlaBar.Size = new System.Drawing.Size(380, 14);
            this.pnlSlaBar.Name = "pnlSlaBar";
            this.pnlSlaBar.Tag = "40|197,120,16";
            this.pnlCardRight.Controls.Add(this.pnlSlaBar);

            // btnMoreActions (additional actions button — placed in toolbar area)
            this.btnMoreActions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMoreActions.ForeColor = System.Drawing.Color.FromArgb(80, 90, 110);
            this.btnMoreActions.Location = new System.Drawing.Point(600, 16);
            this.btnMoreActions.Name = "btnMoreActions";
            this.btnMoreActions.Size = new System.Drawing.Size(80, 46);
            this.btnMoreActions.Text = "⋯\nเพิ่มเติม";
            this.btnMoreActions.FlatAppearance.BorderSize = 0;
            this.pnlToolbarContainer.Controls.Add(this.btnMoreActions);

            // ══ TIMELINE CARD ════════════════════════════════════════════════════
            this.pnlTimelineCard.BackColor = System.Drawing.Color.White;
            this.pnlTimelineCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTimelineCard.Name = "pnlTimelineCard";
            this.pnlTimelineCard.Controls.Add(this.pnlTimelineScroll);
            this.pnlTimelineCard.Controls.Add(this.pnlTimelineDivider);
            this.pnlTimelineCard.Controls.Add(this.pnlTimelineHeader);

            // Timeline header
            this.pnlTimelineHeader.BackColor = System.Drawing.Color.White;
            this.pnlTimelineHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTimelineHeader.Height = 40;
            this.pnlTimelineHeader.Name = "pnlTimelineHeader";
            this.pnlTimelineHeader.Padding = new System.Windows.Forms.Padding(18, 6, 16, 0);
            this.pnlTimelineHeader.Controls.Add(this.pnlTimelineTabs);
            this.pnlTimelineHeader.Controls.Add(this.lblTimelineTitle);

            this.lblTimelineTitle.AutoSize = true;
            this.lblTimelineTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTimelineTitle.ForeColor = System.Drawing.Color.FromArgb(30, 40, 65);
            this.lblTimelineTitle.Location = new System.Drawing.Point(18, 10);
            this.lblTimelineTitle.Name = "lblTimelineTitle";
            this.lblTimelineTitle.Text = "Timeline ขั้นตอน";

            this.pnlTimelineTabs.BackColor = System.Drawing.Color.White;
            this.pnlTimelineTabs.Location = new System.Drawing.Point(200, 4);
            this.pnlTimelineTabs.Size = new System.Drawing.Size(320, 32);
            this.pnlTimelineTabs.Name = "pnlTimelineTabs";

            // divider
            this.pnlTimelineDivider.BackColor = System.Drawing.Color.FromArgb(225, 228, 236);
            this.pnlTimelineDivider.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTimelineDivider.Height = 1;
            this.pnlTimelineDivider.Name = "pnlTimelineDivider";

            // Scrollable content
            this.pnlTimelineScroll.AutoScroll = true;
            this.pnlTimelineScroll.BackColor = System.Drawing.Color.White;
            this.pnlTimelineScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTimelineScroll.Name = "pnlTimelineScroll";
            this.pnlTimelineScroll.Controls.Add(this.pnlTimelineContent);

            this.pnlTimelineContent.BackColor = System.Drawing.Color.White;
            this.pnlTimelineContent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTimelineContent.Height = 400;
            this.pnlTimelineContent.Name = "pnlTimelineContent";

            // ── FORM ────────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.ClientSize = new System.Drawing.Size(1180, 760);
            this.Controls.Add(this.pnlAppBody);
            this.Controls.Add(this.pnlToolbarBorder);
            this.Controls.Add(this.pnlToolbarContainer);
            this.Controls.Add(this.pnlRibbonBorder);
            this.Controls.Add(this.pnlRibbonTabs);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "WorkflowDetailFormNew";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WFI-2569-00012 - อนุมัติ KB Article";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.WorkflowDetailFormNew_Load);

            this.ResumeLayout(false);
        }
    }
}
