namespace CMS.Presentation.Modules.Workflow
{
    partial class UserProfileForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Top chrome ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlRibbonTabs;
        private System.Windows.Forms.Panel pnlRibbonBorder;
        private System.Windows.Forms.Panel pnlToolbarContainer;

        // ── App body ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlAppBody;
        private System.Windows.Forms.Panel pnlSidebarContainer;
        private System.Windows.Forms.Panel pnlNavBorder;
        private System.Windows.Forms.Panel pnlContent;

        // ── Breadcrumb ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlBreadcrumb;
        private System.Windows.Forms.Label lblBreadcrumb;

        // ── Title bar ─────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;

        // ── Main body (left card + right tabs) ────────────────────────────────
        private System.Windows.Forms.Panel pnlBody;

        // ── Status bar ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlStatusBar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

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
            this.pnlBody             = new System.Windows.Forms.Panel();
            this.pnlStatusBar        = new System.Windows.Forms.Panel();

            this.SuspendLayout();

            // ── Form ─────────────────────────────────────────────────────────
            this.Text          = "โปรไฟล์ของฉัน";
            this.ClientSize    = new System.Drawing.Size(1366, 800);
            this.MinimumSize   = new System.Drawing.Size(1000, 650);
            this.WindowState   = System.Windows.Forms.FormWindowState.Maximized;
            this.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor     = System.Drawing.Color.FromArgb(243, 246, 251);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // ── Ribbon tab bar ────────────────────────────────────────────────
            this.pnlRibbonTabs.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabs.Height    = 32;
            this.pnlRibbonTabs.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlRibbonTabs.Padding   = new System.Windows.Forms.Padding(4, 0, 4, 0);

            // ── Ribbon border ─────────────────────────────────────────────────
            this.pnlRibbonBorder.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonBorder.Height    = 1;
            this.pnlRibbonBorder.BackColor = System.Drawing.Color.FromArgb(200, 215, 240);

            // ── Toolbar ───────────────────────────────────────────────────────
            this.pnlToolbarContainer.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarContainer.Height    = 82;
            this.pnlToolbarContainer.BackColor = System.Drawing.Color.White;

            // ── App body ──────────────────────────────────────────────────────
            this.pnlAppBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlAppBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);

            // ── Sidebar ───────────────────────────────────────────────────────
            this.pnlSidebarContainer.Dock       = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebarContainer.Width      = 202;
            this.pnlSidebarContainer.BackColor  = System.Drawing.Color.White;
            this.pnlSidebarContainer.AutoScroll = true;

            this.pnlNavBorder.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlNavBorder.Width     = 1;
            this.pnlNavBorder.BackColor = System.Drawing.Color.FromArgb(225, 228, 235);

            // ── Content area ──────────────────────────────────────────────────
            this.pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);

            // Breadcrumb
            this.pnlBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Height    = 28;
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlBreadcrumb.Padding   = new System.Windows.Forms.Padding(20, 0, 16, 0);

            this.lblBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblBreadcrumb.Text      = "CMS  ›  บัญชีของฉัน  ›  โปรไฟล์";
            this.lblBreadcrumb.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);

            // Title bar
            this.pnlTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height    = 62;
            this.pnlTitleBar.BackColor = System.Drawing.Color.White;
            this.pnlTitleBar.Padding   = new System.Windows.Forms.Padding(20, 0, 16, 0);

            this.lblTitle.Text      = "โปรไฟล์ของฉัน";
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitle.AutoSize  = true;
            this.lblTitle.Location  = new System.Drawing.Point(20, 8);

            this.lblSubtitle.Text      = "จัดการข้อมูลส่วนตัว การแจ้งเตือน และการมอบหมายงานอัตโนมัติ";
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblSubtitle.AutoSize  = true;
            this.lblSubtitle.Location  = new System.Drawing.Point(20, 36);

            var pnlTitleBorder = new System.Windows.Forms.Panel
            {
                Dock      = System.Windows.Forms.DockStyle.Bottom,
                Height    = 1,
                BackColor = System.Drawing.Color.FromArgb(220, 225, 235),
            };
            this.pnlTitleBar.Controls.Add(this.lblTitle);
            this.pnlTitleBar.Controls.Add(this.lblSubtitle);
            this.pnlTitleBar.Controls.Add(pnlTitleBorder);

            // Main body
            this.pnlBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlBody.Padding   = new System.Windows.Forms.Padding(16, 12, 16, 12);

            this.pnlContent.Controls.Add(this.pnlBody);
            this.pnlContent.Controls.Add(this.pnlTitleBar);
            this.pnlContent.Controls.Add(this.pnlBreadcrumb);

            this.pnlAppBody.Controls.Add(this.pnlContent);
            this.pnlAppBody.Controls.Add(this.pnlNavBorder);
            this.pnlAppBody.Controls.Add(this.pnlSidebarContainer);

            // Status bar
            this.pnlStatusBar.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatusBar.Height    = 24;
            this.pnlStatusBar.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);

            this.Controls.Add(this.pnlAppBody);
            this.Controls.Add(this.pnlStatusBar);
            this.Controls.Add(this.pnlToolbarContainer);
            this.Controls.Add(this.pnlRibbonBorder);
            this.Controls.Add(this.pnlRibbonTabs);

            this.Load += new System.EventHandler(this.UserProfileForm_Load);
            this.ResumeLayout(false);
        }
    }
}
