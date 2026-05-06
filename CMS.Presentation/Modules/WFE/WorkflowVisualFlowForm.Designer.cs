namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowVisualFlowForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Top chrome ───────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlRibbonTabs;
        private System.Windows.Forms.Panel pnlRibbonBorder;
        private System.Windows.Forms.Panel pnlToolbarContainer;

        // ── App body ─────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlAppBody;
        private System.Windows.Forms.Panel pnlSidebarContainer;
        private System.Windows.Forms.Panel pnlNavBorder;
        private System.Windows.Forms.Panel pnlContent;

        // ── Breadcrumb ───────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlBreadcrumb;
        private System.Windows.Forms.Label lblBreadcrumb;

        // ── Title bar ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblStepBadge;
        private System.Windows.Forms.Button btnExportPng;
        private System.Windows.Forms.Panel pnlTitleBorder;

        // ── Designer body (canvas + right panel) ─────────────────────────────
        private System.Windows.Forms.Panel pnlDesignerBody;

        // Canvas area
        private System.Windows.Forms.Panel pnlCanvasOuter;
        private System.Windows.Forms.Panel pnlLegendBar;
        private System.Windows.Forms.Panel pnlCanvasHeader;
        private System.Windows.Forms.Label lblZoomLevel;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Panel pnlCanvas;

        // Right panel
        private System.Windows.Forms.Panel pnlRightBorder;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlRightScroll;

        // ── Status bar ───────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlStatusBar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlRibbonTabs      = new System.Windows.Forms.Panel();
            this.pnlRibbonBorder    = new System.Windows.Forms.Panel();
            this.pnlToolbarContainer = new System.Windows.Forms.Panel();
            this.pnlAppBody         = new System.Windows.Forms.Panel();
            this.pnlSidebarContainer = new System.Windows.Forms.Panel();
            this.pnlNavBorder       = new System.Windows.Forms.Panel();
            this.pnlContent         = new System.Windows.Forms.Panel();
            this.pnlBreadcrumb      = new System.Windows.Forms.Panel();
            this.lblBreadcrumb      = new System.Windows.Forms.Label();
            this.pnlTitleBar        = new System.Windows.Forms.Panel();
            this.lblTitle           = new System.Windows.Forms.Label();
            this.lblSubtitle        = new System.Windows.Forms.Label();
            this.lblStepBadge       = new System.Windows.Forms.Label();
            this.btnExportPng       = new System.Windows.Forms.Button();
            this.pnlTitleBorder     = new System.Windows.Forms.Panel();
            this.pnlDesignerBody    = new System.Windows.Forms.Panel();
            this.pnlCanvasOuter     = new System.Windows.Forms.Panel();
            this.pnlLegendBar       = new System.Windows.Forms.Panel();
            this.pnlCanvasHeader    = new System.Windows.Forms.Panel();
            this.lblZoomLevel       = new System.Windows.Forms.Label();
            this.btnZoomOut         = new System.Windows.Forms.Button();
            this.btnZoomIn          = new System.Windows.Forms.Button();
            this.pnlCanvas          = new System.Windows.Forms.Panel();
            this.pnlRightBorder     = new System.Windows.Forms.Panel();
            this.pnlRight           = new System.Windows.Forms.Panel();
            this.pnlRightScroll     = new System.Windows.Forms.Panel();
            this.pnlStatusBar       = new System.Windows.Forms.Panel();

            this.SuspendLayout();

            // ── Form ─────────────────────────────────────────────────────────
            this.Text            = "Visual Flow — WFI-2569-00012";
            this.ClientSize      = new System.Drawing.Size(1400, 860);
            this.MinimumSize     = new System.Drawing.Size(1100, 700);
            this.WindowState     = System.Windows.Forms.FormWindowState.Maximized;
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor       = System.Drawing.Color.FromArgb(243, 246, 251);
            this.Load           += new System.EventHandler(this.WorkflowVisualFlowForm_Load);

            // ── Ribbon tab bar ────────────────────────────────────────────────
            this.pnlRibbonTabs.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabs.Height    = 32;
            this.pnlRibbonTabs.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlRibbonTabs.Name      = "pnlRibbonTabs";

            // ── Ribbon border ─────────────────────────────────────────────────
            this.pnlRibbonBorder.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonBorder.Height    = 1;
            this.pnlRibbonBorder.BackColor = System.Drawing.Color.FromArgb(200, 215, 240);
            this.pnlRibbonBorder.Name      = "pnlRibbonBorder";

            // ── Toolbar ───────────────────────────────────────────────────────
            this.pnlToolbarContainer.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarContainer.Height    = 82;
            this.pnlToolbarContainer.BackColor = System.Drawing.Color.White;
            this.pnlToolbarContainer.Name      = "pnlToolbarContainer";

            // ── Status bar ────────────────────────────────────────────────────
            this.pnlStatusBar.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatusBar.Height    = 24;
            this.pnlStatusBar.BackColor = System.Drawing.Color.FromArgb(244, 246, 250);
            this.pnlStatusBar.Name      = "pnlStatusBar";

            // ── App body ──────────────────────────────────────────────────────
            this.pnlAppBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlAppBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlAppBody.Name      = "pnlAppBody";

            // ── Sidebar ───────────────────────────────────────────────────────
            this.pnlSidebarContainer.Dock       = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebarContainer.Width      = 202;
            this.pnlSidebarContainer.BackColor  = System.Drawing.Color.FromArgb(26, 35, 67);
            this.pnlSidebarContainer.AutoScroll = true;
            this.pnlSidebarContainer.Name       = "pnlSidebarContainer";

            this.pnlNavBorder.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlNavBorder.Width     = 1;
            this.pnlNavBorder.BackColor = System.Drawing.Color.FromArgb(225, 228, 235);
            this.pnlNavBorder.Name      = "pnlNavBorder";

            // ── Content ───────────────────────────────────────────────────────
            this.pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlContent.Name      = "pnlContent";

            // Breadcrumb
            this.pnlBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Height    = 28;
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlBreadcrumb.Padding   = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlBreadcrumb.Name      = "pnlBreadcrumb";

            this.lblBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblBreadcrumb.Text      = "CMS  ›  Workflow Engine  ›  Monitor  ›  WFI-2569-00012  ›  Visual";
            this.lblBreadcrumb.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);

            // Title bar
            this.pnlTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height    = 62;
            this.pnlTitleBar.BackColor = System.Drawing.Color.White;
            this.pnlTitleBar.Padding   = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlTitleBar.Name      = "pnlTitleBar";

            this.lblTitle.Text      = "Visual Flow — WFI-2569-00012";
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitle.AutoSize  = true;
            this.lblTitle.Location  = new System.Drawing.Point(20, 8);
            this.lblTitle.Name      = "lblTitle";

            this.lblSubtitle.Text      = "อนุมัติ KB Article · Template WFD-003 v1 · มุมมอง Live ของเส้นทางที่เกิดขึ้นจริง";
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblSubtitle.AutoSize  = true;
            this.lblSubtitle.Location  = new System.Drawing.Point(20, 36);
            this.lblSubtitle.Name      = "lblSubtitle";

            this.lblStepBadge.Text      = "● Step 2/3 · กำลังดำเนินการ";
            this.lblStepBadge.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblStepBadge.ForeColor = System.Drawing.Color.FromArgb(197, 120, 16);
            this.lblStepBadge.BackColor = System.Drawing.Color.FromArgb(254, 243, 199);
            this.lblStepBadge.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblStepBadge.Location  = new System.Drawing.Point(700, 22);
            this.lblStepBadge.Size      = new System.Drawing.Size(200, 22);
            this.lblStepBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStepBadge.Name      = "lblStepBadge";

            this.btnExportPng.Text      = "⬇  Export PNG";
            this.btnExportPng.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnExportPng.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnExportPng.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportPng.BackColor = System.Drawing.Color.White;
            this.btnExportPng.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnExportPng.Location  = new System.Drawing.Point(910, 18);
            this.btnExportPng.Size      = new System.Drawing.Size(110, 26);
            this.btnExportPng.Name      = "btnExportPng";
            this.btnExportPng.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);

            this.pnlTitleBorder.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlTitleBorder.Height    = 1;
            this.pnlTitleBorder.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);

            this.pnlTitleBar.Controls.Add(this.lblTitle);
            this.pnlTitleBar.Controls.Add(this.lblSubtitle);
            this.pnlTitleBar.Controls.Add(this.lblStepBadge);
            this.pnlTitleBar.Controls.Add(this.btnExportPng);
            this.pnlTitleBar.Controls.Add(this.pnlTitleBorder);

            // ── Designer body ─────────────────────────────────────────────────
            this.pnlDesignerBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignerBody.BackColor = System.Drawing.Color.FromArgb(235, 238, 244);
            this.pnlDesignerBody.Name      = "pnlDesignerBody";

            // Right panel
            this.pnlRight.Dock      = System.Windows.Forms.DockStyle.Right;
            this.pnlRight.Width     = 260;
            this.pnlRight.BackColor = System.Drawing.Color.White;
            this.pnlRight.Name      = "pnlRight";

            this.pnlRightScroll.Dock       = System.Windows.Forms.DockStyle.Fill;
            this.pnlRightScroll.AutoScroll = true;
            this.pnlRightScroll.BackColor  = System.Drawing.Color.White;
            this.pnlRightScroll.Name       = "pnlRightScroll";
            this.pnlRight.Controls.Add(this.pnlRightScroll);

            this.pnlRightBorder.Dock      = System.Windows.Forms.DockStyle.Right;
            this.pnlRightBorder.Width     = 1;
            this.pnlRightBorder.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.pnlRightBorder.Name      = "pnlRightBorder";

            // Canvas outer
            this.pnlCanvasOuter.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlCanvasOuter.BackColor = System.Drawing.Color.FromArgb(235, 238, 244);
            this.pnlCanvasOuter.Name      = "pnlCanvasOuter";

            // Legend bar
            this.pnlLegendBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlLegendBar.Height    = 32;
            this.pnlLegendBar.BackColor = System.Drawing.Color.White;
            this.pnlLegendBar.Name      = "pnlLegendBar";

            // Canvas header (zoom controls)
            this.pnlCanvasHeader.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlCanvasHeader.Height    = 32;
            this.pnlCanvasHeader.BackColor = System.Drawing.Color.White;
            this.pnlCanvasHeader.Name      = "pnlCanvasHeader";

            this.lblZoomLevel.Text      = "100%";
            this.lblZoomLevel.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblZoomLevel.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblZoomLevel.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblZoomLevel.Location  = new System.Drawing.Point(700, 6);
            this.lblZoomLevel.Size      = new System.Drawing.Size(40, 20);
            this.lblZoomLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblZoomLevel.Name      = "lblZoomLevel";

            this.btnZoomOut.Text      = "−";
            this.btnZoomOut.Font      = new System.Drawing.Font("Segoe UI", 11F);
            this.btnZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomOut.BackColor = System.Drawing.Color.White;
            this.btnZoomOut.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnZoomOut.Location  = new System.Drawing.Point(748, 4);
            this.btnZoomOut.Size      = new System.Drawing.Size(24, 24);
            this.btnZoomOut.Name      = "btnZoomOut";
            this.btnZoomOut.FlatAppearance.BorderSize = 0;

            this.btnZoomIn.Text      = "+";
            this.btnZoomIn.Font      = new System.Drawing.Font("Segoe UI", 11F);
            this.btnZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomIn.BackColor = System.Drawing.Color.White;
            this.btnZoomIn.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnZoomIn.Location  = new System.Drawing.Point(774, 4);
            this.btnZoomIn.Size      = new System.Drawing.Size(24, 24);
            this.btnZoomIn.Name      = "btnZoomIn";
            this.btnZoomIn.FlatAppearance.BorderSize = 0;

            this.pnlCanvasHeader.Controls.Add(this.lblZoomLevel);
            this.pnlCanvasHeader.Controls.Add(this.btnZoomOut);
            this.pnlCanvasHeader.Controls.Add(this.btnZoomIn);

            // Canvas
            this.pnlCanvas.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlCanvas.BackColor = System.Drawing.Color.FromArgb(240, 243, 248);
            this.pnlCanvas.Name      = "pnlCanvas";

            this.pnlCanvasOuter.Controls.Add(this.pnlCanvas);
            this.pnlCanvasOuter.Controls.Add(this.pnlCanvasHeader);
            this.pnlCanvasOuter.Controls.Add(this.pnlLegendBar);

            this.pnlDesignerBody.Controls.Add(this.pnlCanvasOuter);
            this.pnlDesignerBody.Controls.Add(this.pnlRightBorder);
            this.pnlDesignerBody.Controls.Add(this.pnlRight);

            // ── Assemble content ──────────────────────────────────────────────
            this.pnlContent.Controls.Add(this.pnlDesignerBody);
            this.pnlContent.Controls.Add(this.pnlTitleBorder);
            this.pnlContent.Controls.Add(this.pnlTitleBar);
            this.pnlContent.Controls.Add(this.pnlBreadcrumb);

            this.pnlAppBody.Controls.Add(this.pnlContent);
            this.pnlAppBody.Controls.Add(this.pnlNavBorder);
            this.pnlAppBody.Controls.Add(this.pnlSidebarContainer);

            this.Controls.Add(this.pnlAppBody);
            this.Controls.Add(this.pnlStatusBar);
            this.Controls.Add(this.pnlToolbarContainer);
            this.Controls.Add(this.pnlRibbonBorder);
            this.Controls.Add(this.pnlRibbonTabs);

            this.ResumeLayout(false);
        }
    }
}
