namespace CMS.Presentation.Modules.KMS
{
    partial class KBDashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.pnlDashboard = new System.Windows.Forms.Panel();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnSendReport = new System.Windows.Forms.Button();
            this.pnlKPI = new System.Windows.Forms.Panel();
            this.pnlKPI1 = new System.Windows.Forms.Panel();
            this.lblTotalArticles = new System.Windows.Forms.Label();
            this.lblTotalArticlesSub = new System.Windows.Forms.Label();
            this.pnlKPI2 = new System.Windows.Forms.Panel();
            this.lblMonthlyViews = new System.Windows.Forms.Label();
            this.lblMonthlyViewsSub = new System.Windows.Forms.Label();
            this.pnlKPI3 = new System.Windows.Forms.Panel();
            this.lblAvgRating = new System.Windows.Forms.Label();
            this.lblAvgRatingSub = new System.Windows.Forms.Label();
            this.pnlKPI4 = new System.Windows.Forms.Panel();
            this.lblPublished = new System.Windows.Forms.Label();
            this.lblPublishedSub = new System.Windows.Forms.Label();
            this.pnlChart = new System.Windows.Forms.Panel();
            this.pnlTopArticles = new System.Windows.Forms.Panel();
            this.pnlAlerts = new System.Windows.Forms.Panel();
            this.lblOverdueCount = new System.Windows.Forms.Label();
            this.lblDraftCount = new System.Windows.Forms.Label();
            this.lblExpiringCount = new System.Windows.Forms.Label();
            this.lblArchivedCount = new System.Windows.Forms.Label();
            this.tabApproval = new System.Windows.Forms.TabPage();
            this.lblApprovalBadge = new System.Windows.Forms.Label();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.lblStatusUser = new System.Windows.Forms.Label();
            this.lblStatusMessage = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabDashboard.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlKPI.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabDashboard);
            this.tabControl.Controls.Add(this.tabApproval);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1200, 700);
            this.tabControl.TabIndex = 0;
            // 
            // tabDashboard
            // 
            this.tabDashboard.Controls.Add(this.pnlDashboard);
            this.tabDashboard.Controls.Add(this.pnlToolbar);
            this.tabDashboard.Location = new System.Drawing.Point(4, 22);
            this.tabDashboard.Name = "tabDashboard";
            this.tabDashboard.Padding = new System.Windows.Forms.Padding(3);
            this.tabDashboard.Size = new System.Drawing.Size(1192, 674);
            this.tabDashboard.TabIndex = 0;
            this.tabDashboard.Text = "📊 KMS Dashboard";
            this.tabDashboard.UseVisualStyleBackColor = true;
            // 
            // pnlDashboard
            // 
            this.pnlDashboard.AutoScroll = true;
            this.pnlDashboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.Location = new System.Drawing.Point(3, 39);
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.Size = new System.Drawing.Size(1186, 632);
            this.pnlDashboard.TabIndex = 1;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.pnlToolbar.Controls.Add(this.btnRefresh);
            this.pnlToolbar.Controls.Add(this.btnExportExcel);
            this.pnlToolbar.Controls.Add(this.btnPrint);
            this.pnlToolbar.Controls.Add(this.btnSendReport);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(3, 3);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1186, 36);
            this.pnlToolbar.TabIndex = 0;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(10, 6);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(90, 24);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "🔄 รีเฟรช";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(110, 6);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(110, 24);
            this.btnExportExcel.TabIndex = 1;
            this.btnExportExcel.Text = "📋 Export Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(230, 6);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(80, 24);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "🖨️ Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnSendReport
            // 
            this.btnSendReport.Location = new System.Drawing.Point(320, 6);
            this.btnSendReport.Name = "btnSendReport";
            this.btnSendReport.Size = new System.Drawing.Size(110, 24);
            this.btnSendReport.TabIndex = 3;
            this.btnSendReport.Text = "📧 ส่งรายงาน";
            this.btnSendReport.UseVisualStyleBackColor = true;
            this.btnSendReport.Click += new System.EventHandler(this.btnSendReport_Click);
            // 
            // pnlKPI
            // 
            this.pnlKPI.Location = new System.Drawing.Point(10, 10);
            this.pnlKPI.Name = "pnlKPI";
            this.pnlKPI.Size = new System.Drawing.Size(1160, 100);
            this.pnlKPI.TabIndex = 0;
            // 
            // pnlChart
            // 
            this.pnlChart.BackColor = System.Drawing.Color.White;
            this.pnlChart.Location = new System.Drawing.Point(10, 120);
            this.pnlChart.Name = "pnlChart";
            this.pnlChart.Size = new System.Drawing.Size(570, 180);
            this.pnlChart.TabIndex = 1;
            // 
            // pnlTopArticles
            // 
            this.pnlTopArticles.AutoScroll = true;
            this.pnlTopArticles.BackColor = System.Drawing.Color.White;
            this.pnlTopArticles.Location = new System.Drawing.Point(590, 120);
            this.pnlTopArticles.Name = "pnlTopArticles";
            this.pnlTopArticles.Size = new System.Drawing.Size(570, 180);
            this.pnlTopArticles.TabIndex = 2;
            // 
            // pnlAlerts
            // 
            this.pnlAlerts.BackColor = System.Drawing.Color.White;
            this.pnlAlerts.Location = new System.Drawing.Point(10, 310);
            this.pnlAlerts.Name = "pnlAlerts";
            this.pnlAlerts.Size = new System.Drawing.Size(570, 200);
            this.pnlAlerts.TabIndex = 3;
            // 
            // tabApproval
            // 
            this.tabApproval.Location = new System.Drawing.Point(4, 22);
            this.tabApproval.Name = "tabApproval";
            this.tabApproval.Padding = new System.Windows.Forms.Padding(3);
            this.tabApproval.Size = new System.Drawing.Size(1192, 674);
            this.tabApproval.TabIndex = 1;
            this.tabApproval.Text = "✅ Approval Queue";
            this.tabApproval.UseVisualStyleBackColor = true;
            // 
            // lblApprovalBadge
            // 
            this.lblApprovalBadge.AutoSize = true;
            this.lblApprovalBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.lblApprovalBadge.ForeColor = System.Drawing.Color.White;
            this.lblApprovalBadge.Location = new System.Drawing.Point(150, 5);
            this.lblApprovalBadge.Name = "lblApprovalBadge";
            this.lblApprovalBadge.Size = new System.Drawing.Size(13, 13);
            this.lblApprovalBadge.TabIndex = 0;
            this.lblApprovalBadge.Text = "0";
            // 
            // pnlStatus
            // 
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlStatus.Controls.Add(this.lblStatusUser);
            this.pnlStatus.Controls.Add(this.lblStatusMessage);
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.Location = new System.Drawing.Point(0, 700);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(1200, 26);
            this.pnlStatus.TabIndex = 1;
            // 
            // lblStatusUser
            // 
            this.lblStatusUser.AutoSize = true;
            this.lblStatusUser.ForeColor = System.Drawing.Color.White;
            this.lblStatusUser.Location = new System.Drawing.Point(10, 6);
            this.lblStatusUser.Name = "lblStatusUser";
            this.lblStatusUser.Size = new System.Drawing.Size(100, 13);
            this.lblStatusUser.TabIndex = 0;
            this.lblStatusUser.Text = "👤 ผู้ใช้งาน";
            // 
            // lblStatusMessage
            // 
            this.lblStatusMessage.AutoSize = true;
            this.lblStatusMessage.ForeColor = System.Drawing.Color.White;
            this.lblStatusMessage.Location = new System.Drawing.Point(200, 6);
            this.lblStatusMessage.Name = "lblStatusMessage";
            this.lblStatusMessage.Size = new System.Drawing.Size(50, 13);
            this.lblStatusMessage.TabIndex = 1;
            this.lblStatusMessage.Text = "พร้อม";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(1100, 670);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(80, 24);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "ปิด";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // KBDashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 726);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlStatus);
            this.Name = "KBDashboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KMS Dashboard & Reports";
            this.Load += new System.EventHandler(this.KBDashboardForm_Load);
            this.tabControl.ResumeLayout(false);
            this.tabDashboard.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlKPI.ResumeLayout(false);
            this.pnlStatus.ResumeLayout(false);
            this.pnlStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDashboard;
        private System.Windows.Forms.TabPage tabApproval;
        private System.Windows.Forms.Panel pnlDashboard;
        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnSendReport;
        private System.Windows.Forms.Panel pnlKPI;
        private System.Windows.Forms.Panel pnlKPI1;
        private System.Windows.Forms.Label lblTotalArticles;
        private System.Windows.Forms.Label lblTotalArticlesSub;
        private System.Windows.Forms.Panel pnlKPI2;
        private System.Windows.Forms.Label lblMonthlyViews;
        private System.Windows.Forms.Label lblMonthlyViewsSub;
        private System.Windows.Forms.Panel pnlKPI3;
        private System.Windows.Forms.Label lblAvgRating;
        private System.Windows.Forms.Label lblAvgRatingSub;
        private System.Windows.Forms.Panel pnlKPI4;
        private System.Windows.Forms.Label lblPublished;
        private System.Windows.Forms.Label lblPublishedSub;
        private System.Windows.Forms.Panel pnlChart;
        private System.Windows.Forms.Panel pnlTopArticles;
        private System.Windows.Forms.Panel pnlAlerts;
        private System.Windows.Forms.Label lblOverdueCount;
        private System.Windows.Forms.Label lblDraftCount;
        private System.Windows.Forms.Label lblExpiringCount;
        private System.Windows.Forms.Label lblArchivedCount;
        private System.Windows.Forms.Label lblApprovalBadge;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblStatusUser;
        private System.Windows.Forms.Label lblStatusMessage;
        private System.Windows.Forms.Button btnClose;
    }
}
