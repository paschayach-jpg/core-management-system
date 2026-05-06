namespace CMS.Presentation.Modules.Workflow
{
    partial class TemplateApprovalForm
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
        private System.Windows.Forms.Panel pnlTitleBorder;

        // ── Dashboard body ───────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlDashboard;

        // Summary cards
        private System.Windows.Forms.FlowLayoutPanel flpCards;

        // Filter bar
        private System.Windows.Forms.Panel pnlFilterBar;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClearFilter;
        private System.Windows.Forms.Label lblResultCount;

        // List view
        private System.Windows.Forms.ListView lvTemplates;

        // Pagination footer
        private System.Windows.Forms.Panel pnlPagination;
        private System.Windows.Forms.Label lblSelectionInfo;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Label lblPage;
        private System.Windows.Forms.Button btnNext;

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

            // ── Containers ───────────────────────────────────────────────────
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
            this.pnlTitleBorder      = new System.Windows.Forms.Panel();
            this.pnlDashboard        = new System.Windows.Forms.Panel();
            this.flpCards            = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlFilterBar        = new System.Windows.Forms.Panel();
            this.cmbCategory         = new System.Windows.Forms.ComboBox();
            this.txtSearch           = new System.Windows.Forms.TextBox();
            this.btnSearch           = new System.Windows.Forms.Button();
            this.btnClearFilter      = new System.Windows.Forms.Button();
            this.lblResultCount      = new System.Windows.Forms.Label();
            this.lvTemplates         = new System.Windows.Forms.ListView();
            this.pnlPagination       = new System.Windows.Forms.Panel();
            this.lblSelectionInfo    = new System.Windows.Forms.Label();
            this.btnPrev             = new System.Windows.Forms.Button();
            this.lblPage             = new System.Windows.Forms.Label();
            this.btnNext             = new System.Windows.Forms.Button();
            this.pnlStatusBar        = new System.Windows.Forms.Panel();

            this.SuspendLayout();

            // ── Form ─────────────────────────────────────────────────────────
            this.Text            = "อนุมัติ Workflow Template";
            this.ClientSize      = new System.Drawing.Size(1280, 800);
            this.MinimumSize     = new System.Drawing.Size(1000, 650);
            this.WindowState     = System.Windows.Forms.FormWindowState.Maximized;
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor       = System.Drawing.Color.FromArgb(243, 246, 251);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;

            // ── Ribbon tab bar ────────────────────────────────────────────────
            this.pnlRibbonTabs.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabs.Height    = 32;
            this.pnlRibbonTabs.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlRibbonTabs.Padding   = new System.Windows.Forms.Padding(4, 0, 4, 0);
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

            // ── App body ──────────────────────────────────────────────────────
            this.pnlAppBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlAppBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlAppBody.Name      = "pnlAppBody";

            // ── Sidebar ───────────────────────────────────────────────────────
            this.pnlSidebarContainer.Dock       = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebarContainer.Width      = 202;
            this.pnlSidebarContainer.BackColor  = System.Drawing.Color.White;
            this.pnlSidebarContainer.AutoScroll = true;
            this.pnlSidebarContainer.Name       = "pnlSidebarContainer";

            this.pnlNavBorder.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlNavBorder.Width     = 1;
            this.pnlNavBorder.BackColor = System.Drawing.Color.FromArgb(225, 228, 235);
            this.pnlNavBorder.Name      = "pnlNavBorder";

            // ── Content area ──────────────────────────────────────────────────
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
            this.lblBreadcrumb.Text      = "CMS  ›  Workflow Engine  ›  อนุมัติ Template";
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

            this.lblTitle.Text      = "☑  อนุมัติ Workflow Template";
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitle.AutoSize  = true;
            this.lblTitle.Location  = new System.Drawing.Point(20, 8);

            this.lblSubtitle.Text      = "รายการ Template ที่รออนุมัติจากผู้ดูแลระบบ";
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblSubtitle.AutoSize  = true;
            this.lblSubtitle.Location  = new System.Drawing.Point(20, 36);

            this.pnlTitleBorder.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlTitleBorder.Height    = 1;
            this.pnlTitleBorder.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);

            this.pnlTitleBar.Controls.Add(this.lblTitle);
            this.pnlTitleBar.Controls.Add(this.lblSubtitle);
            this.pnlTitleBar.Controls.Add(this.pnlTitleBorder);

            // Dashboard body
            this.pnlDashboard.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.BackColor = System.Drawing.Color.White;
            this.pnlDashboard.Padding   = new System.Windows.Forms.Padding(16, 8, 16, 0);
            this.pnlDashboard.Name      = "pnlDashboard";

            // Summary cards
            this.flpCards.Dock          = System.Windows.Forms.DockStyle.Top;
            this.flpCards.Height        = 98;
            this.flpCards.BackColor     = System.Drawing.Color.White;
            this.flpCards.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flpCards.WrapContents  = false;
            this.flpCards.Padding       = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.flpCards.Name          = "flpCards";

            // Filter bar
            this.pnlFilterBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlFilterBar.Height    = 44;
            this.pnlFilterBar.BackColor = System.Drawing.Color.White;
            this.pnlFilterBar.Padding   = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.pnlFilterBar.Name      = "pnlFilterBar";

            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;
            this.cmbCategory.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbCategory.Location      = new System.Drawing.Point(0, 8);
            this.cmbCategory.Size          = new System.Drawing.Size(140, 26);
            this.cmbCategory.Name          = "cmbCategory";

            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.ForeColor   = System.Drawing.Color.FromArgb(140, 150, 165);
            this.txtSearch.Text        = "ค้นหาชื่อ Template...";
            this.txtSearch.Location    = new System.Drawing.Point(150, 8);
            this.txtSearch.Size        = new System.Drawing.Size(220, 26);
            this.txtSearch.Name        = "txtSearch";

            this.btnSearch.FlatStyle  = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Text       = "🔍  ค้นหา";
            this.btnSearch.Font       = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSearch.ForeColor  = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnSearch.Location   = new System.Drawing.Point(380, 8);
            this.btnSearch.Size       = new System.Drawing.Size(90, 26);
            this.btnSearch.Name       = "btnSearch";
            this.btnSearch.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);

            this.btnClearFilter.FlatStyle  = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFilter.Text       = "⏷  ล้างตัวกรอง";
            this.btnClearFilter.Font       = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClearFilter.ForeColor  = System.Drawing.Color.FromArgb(80, 90, 110);
            this.btnClearFilter.Anchor     = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClearFilter.Size       = new System.Drawing.Size(120, 26);
            this.btnClearFilter.Name       = "btnClearFilter";
            this.btnClearFilter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);

            this.lblResultCount.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblResultCount.ForeColor = System.Drawing.Color.FromArgb(110, 120, 130);
            this.lblResultCount.AutoSize  = true;
            this.lblResultCount.Location  = new System.Drawing.Point(480, 12);
            this.lblResultCount.Name      = "lblResultCount";
            this.lblResultCount.Text      = "พบ 0 รายการ";

            this.pnlFilterBar.Controls.Add(this.cmbCategory);
            this.pnlFilterBar.Controls.Add(this.txtSearch);
            this.pnlFilterBar.Controls.Add(this.btnSearch);
            this.pnlFilterBar.Controls.Add(this.btnClearFilter);
            this.pnlFilterBar.Controls.Add(this.lblResultCount);

            // ListView
            this.lvTemplates.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.lvTemplates.View          = System.Windows.Forms.View.Details;
            this.lvTemplates.OwnerDraw     = true;
            this.lvTemplates.FullRowSelect = true;
            this.lvTemplates.GridLines     = false;
            this.lvTemplates.HideSelection = false;
            this.lvTemplates.MultiSelect   = false;
            this.lvTemplates.BorderStyle   = System.Windows.Forms.BorderStyle.None;
            this.lvTemplates.Font          = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lvTemplates.Name          = "lvTemplates";

            // Pagination
            this.pnlPagination.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlPagination.Height    = 44;
            this.pnlPagination.BackColor = System.Drawing.Color.White;
            this.pnlPagination.Padding   = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlPagination.Name      = "pnlPagination";

            this.lblSelectionInfo.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSelectionInfo.ForeColor = System.Drawing.Color.FromArgb(110, 120, 130);
            this.lblSelectionInfo.AutoSize  = true;
            this.lblSelectionInfo.Location  = new System.Drawing.Point(12, 14);
            this.lblSelectionInfo.Text      = "เลือก 0 รายการ";

            this.btnPrev.FlatStyle  = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrev.Text       = "←  ก่อนหน้า";
            this.btnPrev.Font       = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPrev.ForeColor  = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnPrev.Anchor     = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnPrev.Size       = new System.Drawing.Size(100, 26);
            this.btnPrev.Name       = "btnPrev";
            this.btnPrev.FlatAppearance.BorderSize = 0;

            this.lblPage.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPage.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.lblPage.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblPage.Size      = new System.Drawing.Size(80, 26);
            this.lblPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPage.Text      = "หน้า 1 / 1";

            this.btnNext.FlatStyle  = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Text       = "ถัดไป  →";
            this.btnNext.Font       = new System.Drawing.Font("Segoe UI", 9F);
            this.btnNext.ForeColor  = System.Drawing.Color.FromArgb(44, 62, 100);
            this.btnNext.Anchor     = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnNext.Size       = new System.Drawing.Size(90, 26);
            this.btnNext.Name       = "btnNext";
            this.btnNext.FlatAppearance.BorderSize = 0;

            this.pnlPagination.Controls.Add(this.lblSelectionInfo);
            this.pnlPagination.Controls.Add(this.btnPrev);
            this.pnlPagination.Controls.Add(this.lblPage);
            this.pnlPagination.Controls.Add(this.btnNext);

            // Status bar
            this.pnlStatusBar.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatusBar.Height    = 24;
            this.pnlStatusBar.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlStatusBar.Name      = "pnlStatusBar";

            // ── Assemble ──────────────────────────────────────────────────────
            this.pnlDashboard.Controls.Add(this.lvTemplates);
            this.pnlDashboard.Controls.Add(this.pnlPagination);
            this.pnlDashboard.Controls.Add(this.pnlFilterBar);
            this.pnlDashboard.Controls.Add(this.flpCards);

            this.pnlContent.Controls.Add(this.pnlDashboard);
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
