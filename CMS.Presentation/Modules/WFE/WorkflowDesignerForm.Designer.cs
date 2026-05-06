namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowDesignerForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Top chrome ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlRibbonTabs;
        private System.Windows.Forms.Panel pnlRibbonBorder;
        private System.Windows.Forms.Panel pnlToolbarContainer;
        private System.Windows.Forms.Panel pnlToolbarBottomBorder;

        // ── App body ─────────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlAppBody;
        private System.Windows.Forms.Panel pnlSidebarContainer;
        private System.Windows.Forms.Panel pnlNavBorder;
        private System.Windows.Forms.Panel pnlContent;

        // ── Breadcrumb ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlBreadcrumb;
        private System.Windows.Forms.Label lblBreadcrumb;

        // ── Title bar ────────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblStatusBadge;
        private System.Windows.Forms.Label lblValidateBadge;
        private System.Windows.Forms.Panel pnlTitleBottomBorder;

        // ── Designer body (3 columns) ────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlDesignerBody;

        // Palette (left column)
        private System.Windows.Forms.Panel pnlPalette;
        private System.Windows.Forms.Label lblPaletteHeader;
        private System.Windows.Forms.Panel pnlPaletteItems;
        private System.Windows.Forms.Panel pnlPaletteBorder;

        // Canvas (center column)
        private System.Windows.Forms.Panel pnlCanvasOuter;
        private System.Windows.Forms.Panel pnlCanvasHeader;
        private System.Windows.Forms.Label lblCanvasInfo;
        private System.Windows.Forms.Label lblZoomLevel;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button btnZoomFit;
        private System.Windows.Forms.Panel pnlCanvas;

        // Properties (right column)
        private System.Windows.Forms.Panel pnlPropertiesBorder;
        private System.Windows.Forms.Panel pnlProperties;
        private System.Windows.Forms.Label lblPropertiesHeader;
        private System.Windows.Forms.Label lblSelectedBadge;
        private System.Windows.Forms.Panel pnlPropContent;

        // Properties fields
        private System.Windows.Forms.Label lblStepNameLbl;
        private System.Windows.Forms.TextBox txtStepName;
        private System.Windows.Forms.Label lblAssigneeLbl;
        private System.Windows.Forms.ComboBox cmbAssignee;
        private System.Windows.Forms.Label lblSLALbl;
        private System.Windows.Forms.NumericUpDown numSLA;
        private System.Windows.Forms.Label lblPriorityLbl;
        private System.Windows.Forms.ComboBox cmbPriority;
        private System.Windows.Forms.Label lblEscalationSection;
        private System.Windows.Forms.CheckBox chkEscalation;
        private System.Windows.Forms.Label lblActionsLbl;
        private System.Windows.Forms.CheckBox chkApprove;
        private System.Windows.Forms.CheckBox chkReject;
        private System.Windows.Forms.CheckBox chkForward;
        private System.Windows.Forms.CheckBox chkAck;
        private System.Windows.Forms.Label lblWorkInstrLbl;
        private System.Windows.Forms.LinkLabel linkWorkInstruction;
        private System.Windows.Forms.Label lblWIBadge;
        private System.Windows.Forms.Label lblConditionsLbl;
        private System.Windows.Forms.TextBox txtConditions;

        // ── Status bar ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlStatusBar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlRibbonTabs          = new System.Windows.Forms.Panel();
            this.pnlRibbonBorder        = new System.Windows.Forms.Panel();
            this.pnlToolbarContainer    = new System.Windows.Forms.Panel();
            this.pnlToolbarBottomBorder = new System.Windows.Forms.Panel();
            this.pnlStatusBar           = new System.Windows.Forms.Panel();
            this.pnlAppBody             = new System.Windows.Forms.Panel();
            this.pnlSidebarContainer    = new System.Windows.Forms.Panel();
            this.pnlNavBorder           = new System.Windows.Forms.Panel();
            this.pnlContent             = new System.Windows.Forms.Panel();
            this.pnlBreadcrumb          = new System.Windows.Forms.Panel();
            this.lblBreadcrumb          = new System.Windows.Forms.Label();
            this.pnlTitleBar            = new System.Windows.Forms.Panel();
            this.lblTitle               = new System.Windows.Forms.Label();
            this.lblSubtitle            = new System.Windows.Forms.Label();
            this.lblStatusBadge         = new System.Windows.Forms.Label();
            this.lblValidateBadge       = new System.Windows.Forms.Label();
            this.pnlTitleBottomBorder   = new System.Windows.Forms.Panel();
            this.pnlDesignerBody        = new System.Windows.Forms.Panel();
            this.pnlPalette             = new System.Windows.Forms.Panel();
            this.lblPaletteHeader       = new System.Windows.Forms.Label();
            this.pnlPaletteItems        = new System.Windows.Forms.Panel();
            this.pnlPaletteBorder       = new System.Windows.Forms.Panel();
            this.pnlCanvasOuter         = new System.Windows.Forms.Panel();
            this.pnlCanvasHeader        = new System.Windows.Forms.Panel();
            this.lblCanvasInfo          = new System.Windows.Forms.Label();
            this.lblZoomLevel           = new System.Windows.Forms.Label();
            this.btnZoomOut             = new System.Windows.Forms.Button();
            this.btnZoomIn              = new System.Windows.Forms.Button();
            this.btnZoomFit             = new System.Windows.Forms.Button();
            this.pnlCanvas              = new System.Windows.Forms.Panel();
            this.pnlPropertiesBorder    = new System.Windows.Forms.Panel();
            this.pnlProperties          = new System.Windows.Forms.Panel();
            this.lblPropertiesHeader    = new System.Windows.Forms.Label();
            this.lblSelectedBadge       = new System.Windows.Forms.Label();
            this.pnlPropContent         = new System.Windows.Forms.Panel();
            this.lblStepNameLbl         = new System.Windows.Forms.Label();
            this.txtStepName            = new System.Windows.Forms.TextBox();
            this.lblAssigneeLbl         = new System.Windows.Forms.Label();
            this.cmbAssignee            = new System.Windows.Forms.ComboBox();
            this.lblSLALbl              = new System.Windows.Forms.Label();
            this.numSLA                 = new System.Windows.Forms.NumericUpDown();
            this.lblPriorityLbl         = new System.Windows.Forms.Label();
            this.cmbPriority            = new System.Windows.Forms.ComboBox();
            this.lblEscalationSection   = new System.Windows.Forms.Label();
            this.chkEscalation          = new System.Windows.Forms.CheckBox();
            this.lblActionsLbl          = new System.Windows.Forms.Label();
            this.chkApprove             = new System.Windows.Forms.CheckBox();
            this.chkReject              = new System.Windows.Forms.CheckBox();
            this.chkForward             = new System.Windows.Forms.CheckBox();
            this.chkAck                 = new System.Windows.Forms.CheckBox();
            this.lblWorkInstrLbl        = new System.Windows.Forms.Label();
            this.linkWorkInstruction    = new System.Windows.Forms.LinkLabel();
            this.lblWIBadge             = new System.Windows.Forms.Label();
            this.lblConditionsLbl       = new System.Windows.Forms.Label();
            this.txtConditions          = new System.Windows.Forms.TextBox();

            this.numSLA.BeginInit();
            this.SuspendLayout();

            // ── pnlRibbonTabs ────────────────────────────────────────────────────
            this.pnlRibbonTabs.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlRibbonTabs.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabs.Height    = 32;
            this.pnlRibbonTabs.Name      = "pnlRibbonTabs";

            // ── pnlRibbonBorder ──────────────────────────────────────────────────
            this.pnlRibbonBorder.BackColor = System.Drawing.Color.FromArgb(200, 215, 240);
            this.pnlRibbonBorder.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonBorder.Height    = 1;
            this.pnlRibbonBorder.Name      = "pnlRibbonBorder";

            // ── pnlToolbarContainer ──────────────────────────────────────────────
            this.pnlToolbarContainer.BackColor = System.Drawing.Color.White;
            this.pnlToolbarContainer.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarContainer.Height    = 82;
            this.pnlToolbarContainer.Name      = "pnlToolbarContainer";

            // ── pnlToolbarBottomBorder ───────────────────────────────────────────
            this.pnlToolbarBottomBorder.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.pnlToolbarBottomBorder.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarBottomBorder.Height    = 1;
            this.pnlToolbarBottomBorder.Name      = "pnlToolbarBottomBorder";

            // ── pnlStatusBar ─────────────────────────────────────────────────────
            this.pnlStatusBar.BackColor = System.Drawing.Color.FromArgb(244, 246, 250);
            this.pnlStatusBar.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatusBar.Height    = 24;
            this.pnlStatusBar.Name      = "pnlStatusBar";

            // ── pnlAppBody ───────────────────────────────────────────────────────
            this.pnlAppBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlAppBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlAppBody.Name      = "pnlAppBody";

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
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name      = "pnlContent";

            // ── pnlBreadcrumb ────────────────────────────────────────────────────
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Height    = 28;
            this.pnlBreadcrumb.Name      = "pnlBreadcrumb";
            this.pnlBreadcrumb.Padding   = new System.Windows.Forms.Padding(20, 0, 16, 0);

            this.lblBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblBreadcrumb.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblBreadcrumb.Name      = "lblBreadcrumb";
            this.lblBreadcrumb.Text      = "CMS  ›  Workflow Engine  ›  Templates  ›  WFD-003 · อนุมัติ KB Article";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);

            // ── pnlTitleBar ──────────────────────────────────────────────────────
            this.pnlTitleBar.BackColor = System.Drawing.Color.White;
            this.pnlTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height    = 70;
            this.pnlTitleBar.Name      = "pnlTitleBar";

            this.lblTitle.AutoSize  = true;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitle.Location  = new System.Drawing.Point(20, 8);
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Text      = "WFD-003 · อนุมัติ KB Article";

            this.lblSubtitle.AutoSize  = true;
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblSubtitle.Location  = new System.Drawing.Point(20, 42);
            this.lblSubtitle.Name      = "lblSubtitle";
            this.lblSubtitle.Text      = "เวอร์ชั่น 2 (Draft) · Base: v1 Published";

            this.lblStatusBadge.AutoSize  = true;
            this.lblStatusBadge.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.lblStatusBadge.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(197, 80, 0);
            this.lblStatusBadge.Location  = new System.Drawing.Point(660, 26);
            this.lblStatusBadge.Name      = "lblStatusBadge";
            this.lblStatusBadge.Text      = "●  Draft · ยังไม่ได้เผยแพร่";

            this.lblValidateBadge.AutoSize  = true;
            this.lblValidateBadge.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.lblValidateBadge.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblValidateBadge.ForeColor = System.Drawing.Color.FromArgb(23, 133, 49);
            this.lblValidateBadge.Location  = new System.Drawing.Point(840, 26);
            this.lblValidateBadge.Name      = "lblValidateBadge";
            this.lblValidateBadge.Text      = "✓  Validate (0 errors)";

            this.pnlTitleBar.Controls.Add(this.lblValidateBadge);
            this.pnlTitleBar.Controls.Add(this.lblStatusBadge);
            this.pnlTitleBar.Controls.Add(this.lblSubtitle);
            this.pnlTitleBar.Controls.Add(this.lblTitle);

            // ── pnlTitleBottomBorder ─────────────────────────────────────────────
            this.pnlTitleBottomBorder.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.pnlTitleBottomBorder.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBottomBorder.Height    = 1;
            this.pnlTitleBottomBorder.Name      = "pnlTitleBottomBorder";

            // ── pnlDesignerBody ──────────────────────────────────────────────────
            this.pnlDesignerBody.BackColor = System.Drawing.Color.FromArgb(235, 238, 244);
            this.pnlDesignerBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignerBody.Name      = "pnlDesignerBody";

            // ── pnlPalette ───────────────────────────────────────────────────────
            this.pnlPalette.BackColor = System.Drawing.Color.White;
            this.pnlPalette.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlPalette.Width     = 188;
            this.pnlPalette.Name      = "pnlPalette";

            this.lblPaletteHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblPaletteHeader.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPaletteHeader.ForeColor = System.Drawing.Color.FromArgb(30, 40, 65);
            this.lblPaletteHeader.Height    = 36;
            this.lblPaletteHeader.Name      = "lblPaletteHeader";
            this.lblPaletteHeader.Padding   = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblPaletteHeader.Text      = "Palette";
            this.lblPaletteHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.pnlPaletteItems.AutoScroll = true;
            this.pnlPaletteItems.Dock       = System.Windows.Forms.DockStyle.Fill;
            this.pnlPaletteItems.Name       = "pnlPaletteItems";
            this.pnlPaletteItems.BackColor  = System.Drawing.Color.White;

            this.pnlPalette.Controls.Add(this.pnlPaletteItems);
            this.pnlPalette.Controls.Add(this.lblPaletteHeader);

            // ── pnlPaletteBorder ─────────────────────────────────────────────────
            this.pnlPaletteBorder.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.pnlPaletteBorder.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlPaletteBorder.Width     = 1;
            this.pnlPaletteBorder.Name      = "pnlPaletteBorder";

            // ── pnlProperties ────────────────────────────────────────────────────
            this.pnlProperties.BackColor = System.Drawing.Color.White;
            this.pnlProperties.Dock      = System.Windows.Forms.DockStyle.Right;
            this.pnlProperties.Width     = 240;
            this.pnlProperties.Name      = "pnlProperties";

            this.lblPropertiesHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblPropertiesHeader.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPropertiesHeader.ForeColor = System.Drawing.Color.FromArgb(30, 40, 65);
            this.lblPropertiesHeader.Height    = 36;
            this.lblPropertiesHeader.Name      = "lblPropertiesHeader";
            this.lblPropertiesHeader.Padding   = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblPropertiesHeader.Text      = "Properties";
            this.lblPropertiesHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblSelectedBadge.BackColor = System.Drawing.Color.FromArgb(235, 240, 255);
            this.lblSelectedBadge.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblSelectedBadge.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSelectedBadge.ForeColor = System.Drawing.Color.FromArgb(44, 90, 200);
            this.lblSelectedBadge.Height    = 26;
            this.lblSelectedBadge.Name      = "lblSelectedBadge";
            this.lblSelectedBadge.Padding   = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblSelectedBadge.Text      = "●  Selected · Approval";
            this.lblSelectedBadge.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── pnlPropContent (scrollable fields) ──────────────────────────────
            this.pnlPropContent.AutoScroll = true;
            this.pnlPropContent.Dock       = System.Windows.Forms.DockStyle.Fill;
            this.pnlPropContent.Name       = "pnlPropContent";
            this.pnlPropContent.BackColor  = System.Drawing.Color.White;

            /// Properties fields (hardcoded for Designer compatibility)
            // ชื่อ Step
            this.lblStepNameLbl.AutoSize  = true;
            this.lblStepNameLbl.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStepNameLbl.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblStepNameLbl.Location  = new System.Drawing.Point(14, 10);
            this.lblStepNameLbl.Name      = "lblStepNameLbl";
            this.lblStepNameLbl.Text      = "ชื่อ Step";
            this.txtStepName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStepName.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtStepName.Location    = new System.Drawing.Point(14, 26);
            this.txtStepName.Name        = "txtStepName";
            this.txtStepName.Size        = new System.Drawing.Size(212, 24);
            this.txtStepName.Text        = "Legal ตรวจ";
            // Assignee
            this.lblAssigneeLbl.AutoSize  = true;
            this.lblAssigneeLbl.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblAssigneeLbl.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblAssigneeLbl.Location  = new System.Drawing.Point(14, 56);
            this.lblAssigneeLbl.Name      = "lblAssigneeLbl";
            this.lblAssigneeLbl.Text      = "Assignee";
            this.cmbAssignee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAssignee.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;
            this.cmbAssignee.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbAssignee.Location      = new System.Drawing.Point(14, 72);
            this.cmbAssignee.Name          = "cmbAssignee";
            this.cmbAssignee.Size          = new System.Drawing.Size(212, 24);
            this.cmbAssignee.Items.AddRange(new object[] {
                "AD Group: CMS-Legal-Managers",
                "AD Group: CMS-Managers",
                "AD Group: CMS-Officers",
                "ผู้ยื่นเอกสาร",
                "หัวหน้าหน่วยงาน" });
            this.cmbAssignee.SelectedIndex = 0;
            // SLA + Priority (same row)
            this.lblSLALbl.AutoSize  = true;
            this.lblSLALbl.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblSLALbl.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblSLALbl.Location  = new System.Drawing.Point(14, 102);
            this.lblSLALbl.Name      = "lblSLALbl";
            this.lblSLALbl.Text      = "SLA (วัน)";

            this.lblPriorityLbl.AutoSize  = true;
            this.lblPriorityLbl.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblPriorityLbl.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblPriorityLbl.Location  = new System.Drawing.Point(124, 102);
            this.lblPriorityLbl.Name      = "lblPriorityLbl";
            this.lblPriorityLbl.Text      = "Priority";
            this.numSLA.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.numSLA.Location = new System.Drawing.Point(14, 118);
            this.numSLA.Minimum  = new decimal(new int[] { 1, 0, 0, 0 });
            this.numSLA.Maximum  = new decimal(new int[] { 365, 0, 0, 0 });
            this.numSLA.Value    = new decimal(new int[] { 2, 0, 0, 0 });
            this.numSLA.Name     = "numSLA";
            this.numSLA.Size     = new System.Drawing.Size(100, 24);

            this.cmbPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPriority.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;
            this.cmbPriority.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbPriority.Location      = new System.Drawing.Point(124, 118);
            this.cmbPriority.Name          = "cmbPriority";
            this.cmbPriority.Size          = new System.Drawing.Size(102, 24);
            this.cmbPriority.Items.AddRange(new object[] { "ปกติ", "สูง", "ด่วน", "ด่วนมาก" });
            this.cmbPriority.SelectedIndex = 0;
            // Escalation section label
            this.lblEscalationSection.AutoSize  = true;
            this.lblEscalationSection.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblEscalationSection.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblEscalationSection.Location  = new System.Drawing.Point(14, 150);
            this.lblEscalationSection.Name      = "lblEscalationSection";
            this.lblEscalationSection.Text      = "Escalation";
            this.chkEscalation.Font     = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkEscalation.ForeColor = System.Drawing.Color.FromArgb(44, 52, 70);
            this.chkEscalation.Location = new System.Drawing.Point(14, 166);
            this.chkEscalation.Name     = "chkEscalation";
            this.chkEscalation.Size     = new System.Drawing.Size(212, 34);
            this.chkEscalation.Text     = "เมื่อเกิน SLA → แจ้ง Head of Legal";
            this.chkEscalation.Checked  = true;
            // Actions
            this.lblActionsLbl.AutoSize  = true;
            this.lblActionsLbl.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblActionsLbl.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblActionsLbl.Location  = new System.Drawing.Point(14, 208);
            this.lblActionsLbl.Name      = "lblActionsLbl";
            this.lblActionsLbl.Text      = "Actions ที่ใช้ได้";
            this.chkApprove.AutoSize  = true;
            this.chkApprove.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.chkApprove.Location  = new System.Drawing.Point(14, 226);
            this.chkApprove.Name      = "chkApprove";
            this.chkApprove.Text      = "Approve";
            this.chkApprove.Checked   = true;

            this.chkReject.AutoSize  = true;
            this.chkReject.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.chkReject.Location  = new System.Drawing.Point(96, 226);
            this.chkReject.Name      = "chkReject";
            this.chkReject.Text      = "Reject";
            this.chkReject.Checked   = true;
            this.chkForward.AutoSize  = true;
            this.chkForward.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.chkForward.Location  = new System.Drawing.Point(14, 250);
            this.chkForward.Name      = "chkForward";
            this.chkForward.Text      = "Forward";
            this.chkForward.Checked   = true;

            this.chkAck.AutoSize  = true;
            this.chkAck.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.chkAck.Location  = new System.Drawing.Point(96, 250);
            this.chkAck.Name      = "chkAck";
            this.chkAck.Text      = "Ack";
            this.chkAck.Checked   = false;
            // Work Instruction
            this.lblWorkInstrLbl.AutoSize  = true;
            this.lblWorkInstrLbl.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblWorkInstrLbl.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblWorkInstrLbl.Location  = new System.Drawing.Point(14, 282);
            this.lblWorkInstrLbl.Name      = "lblWorkInstrLbl";
            this.lblWorkInstrLbl.Text      = "Work Instruction";
            this.linkWorkInstruction.AutoSize  = false;
            this.linkWorkInstruction.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.linkWorkInstruction.Location  = new System.Drawing.Point(14, 298);
            this.linkWorkInstruction.Name      = "linkWorkInstruction";
            this.linkWorkInstruction.Size      = new System.Drawing.Size(158, 22);
            this.linkWorkInstruction.Text      = "⇗  WI-002 · การตรวจ Legal";

            this.lblWIBadge.AutoSize  = true;
            this.lblWIBadge.BackColor = System.Drawing.Color.FromArgb(230, 115, 0);
            this.lblWIBadge.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblWIBadge.ForeColor = System.Drawing.Color.White;
            this.lblWIBadge.Location  = new System.Drawing.Point(176, 300);
            this.lblWIBadge.Name      = "lblWIBadge";
            this.lblWIBadge.Padding   = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.lblWIBadge.Text      = "บังคับ";
            // Conditions / Guards
            this.lblConditionsLbl.AutoSize  = true;
            this.lblConditionsLbl.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblConditionsLbl.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblConditionsLbl.Location  = new System.Drawing.Point(14, 330);
            this.lblConditionsLbl.Name      = "lblConditionsLbl";
            this.lblConditionsLbl.Text      = "Conditions / Guards";
            this.txtConditions.BackColor   = System.Drawing.Color.FromArgb(246, 248, 252);
            this.txtConditions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtConditions.Font        = new System.Drawing.Font("Consolas", 8.5F);
            this.txtConditions.ForeColor   = System.Drawing.Color.FromArgb(50, 60, 90);
            this.txtConditions.Location    = new System.Drawing.Point(14, 346);
            this.txtConditions.Multiline   = true;
            this.txtConditions.Name        = "txtConditions";
            this.txtConditions.ScrollBars  = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConditions.Size        = new System.Drawing.Size(212, 72);
            this.txtConditions.Text        = "context.CRAF IN (\"1HC\", \"2SC\")\r\n&& context.DocumentType == \"KB\"";

            // Add fields to pnlPropContent
            this.pnlPropContent.Controls.Add(this.txtConditions);
            this.pnlPropContent.Controls.Add(this.lblConditionsLbl);
            this.pnlPropContent.Controls.Add(this.lblWIBadge);
            this.pnlPropContent.Controls.Add(this.linkWorkInstruction);
            this.pnlPropContent.Controls.Add(this.lblWorkInstrLbl);
            this.pnlPropContent.Controls.Add(this.chkAck);
            this.pnlPropContent.Controls.Add(this.chkForward);
            this.pnlPropContent.Controls.Add(this.chkReject);
            this.pnlPropContent.Controls.Add(this.chkApprove);
            this.pnlPropContent.Controls.Add(this.lblActionsLbl);
            this.pnlPropContent.Controls.Add(this.chkEscalation);
            this.pnlPropContent.Controls.Add(this.lblEscalationSection);
            this.pnlPropContent.Controls.Add(this.cmbPriority);
            this.pnlPropContent.Controls.Add(this.numSLA);
            this.pnlPropContent.Controls.Add(this.lblPriorityLbl);
            this.pnlPropContent.Controls.Add(this.lblSLALbl);
            this.pnlPropContent.Controls.Add(this.cmbAssignee);
            this.pnlPropContent.Controls.Add(this.lblAssigneeLbl);
            this.pnlPropContent.Controls.Add(this.txtStepName);
            this.pnlPropContent.Controls.Add(this.lblStepNameLbl);

            this.pnlProperties.Controls.Add(this.pnlPropContent);
            this.pnlProperties.Controls.Add(this.lblSelectedBadge);
            this.pnlProperties.Controls.Add(this.lblPropertiesHeader);

            // ── pnlPropertiesBorder ──────────────────────────────────────────────
            this.pnlPropertiesBorder.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.pnlPropertiesBorder.Dock      = System.Windows.Forms.DockStyle.Right;
            this.pnlPropertiesBorder.Width     = 1;
            this.pnlPropertiesBorder.Name      = "pnlPropertiesBorder";

            // ── pnlCanvasOuter ───────────────────────────────────────────────────
            this.pnlCanvasOuter.BackColor = System.Drawing.Color.FromArgb(235, 238, 244);
            this.pnlCanvasOuter.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlCanvasOuter.Name      = "pnlCanvasOuter";

            // Canvas header bar
            this.pnlCanvasHeader.BackColor = System.Drawing.Color.FromArgb(245, 247, 252);
            this.pnlCanvasHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlCanvasHeader.Height    = 28;
            this.pnlCanvasHeader.Name      = "pnlCanvasHeader";

            this.lblCanvasInfo.AutoSize  = true;
            this.lblCanvasInfo.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCanvasInfo.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblCanvasInfo.Location  = new System.Drawing.Point(10, 6);
            this.lblCanvasInfo.Name      = "lblCanvasInfo";
            this.lblCanvasInfo.Text      = "🔶  Canvas · Grid: on · Snap: on";

            // Zoom controls (positioned right via Anchor; exact location set in Form_Load)
            this.btnZoomOut.Anchor       = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnZoomOut.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomOut.FlatAppearance.BorderSize = 0;
            this.btnZoomOut.Font         = new System.Drawing.Font("Segoe UI", 10F);
            this.btnZoomOut.ForeColor    = System.Drawing.Color.FromArgb(70, 80, 100);
            this.btnZoomOut.Location     = new System.Drawing.Point(560, 3);
            this.btnZoomOut.Name         = "btnZoomOut";
            this.btnZoomOut.Size         = new System.Drawing.Size(22, 22);
            this.btnZoomOut.Text         = "−";
            this.btnZoomOut.TabStop      = false;

            this.lblZoomLevel.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.lblZoomLevel.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblZoomLevel.ForeColor = System.Drawing.Color.FromArgb(70, 80, 100);
            this.lblZoomLevel.Location  = new System.Drawing.Point(582, 7);
            this.lblZoomLevel.Name      = "lblZoomLevel";
            this.lblZoomLevel.Size      = new System.Drawing.Size(40, 16);
            this.lblZoomLevel.Text      = "100%";
            this.lblZoomLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.btnZoomIn.Anchor       = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnZoomIn.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomIn.FlatAppearance.BorderSize = 0;
            this.btnZoomIn.Font         = new System.Drawing.Font("Segoe UI", 10F);
            this.btnZoomIn.ForeColor    = System.Drawing.Color.FromArgb(70, 80, 100);
            this.btnZoomIn.Location     = new System.Drawing.Point(622, 3);
            this.btnZoomIn.Name         = "btnZoomIn";
            this.btnZoomIn.Size         = new System.Drawing.Size(22, 22);
            this.btnZoomIn.Text         = "+";
            this.btnZoomIn.TabStop      = false;

            this.btnZoomFit.Anchor       = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.btnZoomFit.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomFit.FlatAppearance.BorderSize = 0;
            this.btnZoomFit.Font         = new System.Drawing.Font("Segoe UI", 9F);
            this.btnZoomFit.ForeColor    = System.Drawing.Color.FromArgb(70, 80, 100);
            this.btnZoomFit.Location     = new System.Drawing.Point(646, 3);
            this.btnZoomFit.Name         = "btnZoomFit";
            this.btnZoomFit.Size         = new System.Drawing.Size(32, 22);
            this.btnZoomFit.Text         = "Fit";
            this.btnZoomFit.TabStop      = false;

            this.pnlCanvasHeader.Controls.Add(this.btnZoomFit);
            this.pnlCanvasHeader.Controls.Add(this.btnZoomIn);
            this.pnlCanvasHeader.Controls.Add(this.lblZoomLevel);
            this.pnlCanvasHeader.Controls.Add(this.btnZoomOut);
            this.pnlCanvasHeader.Controls.Add(this.lblCanvasInfo);

            // ── pnlCanvas (drawing surface) ──────────────────────────────────────
            this.pnlCanvas.BackColor    = System.Drawing.Color.FromArgb(232, 235, 245);
            this.pnlCanvas.Dock         = System.Windows.Forms.DockStyle.Fill;
            this.pnlCanvas.Name         = "pnlCanvas";
            this.pnlCanvas.AutoScroll   = true;

            this.pnlCanvasOuter.Controls.Add(this.pnlCanvas);
            this.pnlCanvasOuter.Controls.Add(this.pnlCanvasHeader);

            // ── Assemble pnlDesignerBody (Fill first, Right, Left last) ──────────
            this.pnlDesignerBody.Controls.Add(this.pnlCanvasOuter);       // Fill
            this.pnlDesignerBody.Controls.Add(this.pnlPropertiesBorder);  // Right inner
            this.pnlDesignerBody.Controls.Add(this.pnlProperties);        // Right outer
            this.pnlDesignerBody.Controls.Add(this.pnlPaletteBorder);     // Left inner
            this.pnlDesignerBody.Controls.Add(this.pnlPalette);           // Left outer

            // ── Assemble pnlContent (Fill first, Top last = topmost) ─────────────
            this.pnlContent.Controls.Add(this.pnlDesignerBody);
            this.pnlContent.Controls.Add(this.pnlTitleBottomBorder);
            this.pnlContent.Controls.Add(this.pnlTitleBar);
            this.pnlContent.Controls.Add(this.pnlBreadcrumb);

            // ── Assemble pnlAppBody ──────────────────────────────────────────────
            this.pnlAppBody.Controls.Add(this.pnlContent);
            this.pnlAppBody.Controls.Add(this.pnlNavBorder);
            this.pnlAppBody.Controls.Add(this.pnlSidebarContainer);

            // ── Form ─────────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(243, 246, 251);
            this.ClientSize          = new System.Drawing.Size(1400, 860);
            this.Font                = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize         = new System.Drawing.Size(1100, 700);
            this.Name                = "WorkflowDesignerForm";
            this.Text                = "WFD-003 · ออกแบบ Workflow — Core Management System";
            this.WindowState         = System.Windows.Forms.FormWindowState.Maximized;
            this.Load               += new System.EventHandler(this.WorkflowDesignerForm_Load);

            // Controls.Add order: Fill, Bottom, Top-inner → Top-outer
            this.Controls.Add(this.pnlAppBody);
            this.Controls.Add(this.pnlStatusBar);
            this.Controls.Add(this.pnlToolbarBottomBorder);
            this.Controls.Add(this.pnlToolbarContainer);
            this.Controls.Add(this.pnlRibbonBorder);
            this.Controls.Add(this.pnlRibbonTabs);

            this.numSLA.EndInit();
            this.ResumeLayout(false);
        }
    }
}
