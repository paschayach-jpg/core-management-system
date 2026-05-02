namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowTemplateListForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlRibbon;
        private System.Windows.Forms.Panel pnlRibbonTabBar;
        private System.Windows.Forms.Panel pnlRibbonContent;
        private System.Windows.Forms.Panel pnlGrpTemplate;
        private System.Windows.Forms.Panel pnlGrpPublish;
        private System.Windows.Forms.Panel pnlGrpIO;
        private System.Windows.Forms.Panel pnlSep1;
        private System.Windows.Forms.Panel pnlSep2;
        private System.Windows.Forms.Label lblGrpTemplate;
        private System.Windows.Forms.Label lblGrpPublish;
        private System.Windows.Forms.Label lblGrpIO;
        private System.Windows.Forms.Label lblTabSheet;
        private System.Windows.Forms.Label lblTabHome;
        private System.Windows.Forms.Label lblTabAction;
        private System.Windows.Forms.Label lblTabView;
        private System.Windows.Forms.Label lblTabTools;
        private System.Windows.Forms.Label lblTabHelp;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnDuplicate;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.Button btnDeactivate;
        private System.Windows.Forms.Button btnVersions;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlSidebarSep;
        private System.Windows.Forms.Panel pnlNavActive;
        private System.Windows.Forms.Label lblSecWork;
        private System.Windows.Forms.Label lblSecManage;
        private System.Windows.Forms.Label lblSecSettings;
        private System.Windows.Forms.Label lblNavMyTasks;
        private System.Windows.Forms.Label lblNavMyTasksBadge;
        private System.Windows.Forms.Label lblNavMonitor;
        private System.Windows.Forms.Label lblNavMonitorBadge;
        private System.Windows.Forms.Label lblNavStartWF;
        private System.Windows.Forms.Label lblNavHistoryWF;
        private System.Windows.Forms.Label lblNavTemplates;
        private System.Windows.Forms.Label lblNavDesigner;
        private System.Windows.Forms.Label lblNavApproval;
        private System.Windows.Forms.Label lblNavApprovalBadge;
        private System.Windows.Forms.Label lblNavDelegation;
        private System.Windows.Forms.Label lblNavSLA;
        private System.Windows.Forms.Label lblNavConfig;
        private System.Windows.Forms.Label lblIconMyTasks;
        private System.Windows.Forms.Label lblIconMonitor;
        private System.Windows.Forms.Label lblIconStartWF;
        private System.Windows.Forms.Label lblIconHistoryWF;
        private System.Windows.Forms.Label lblIconTemplates;
        private System.Windows.Forms.Label lblIconDesigner;
        private System.Windows.Forms.Label lblIconApproval;
        private System.Windows.Forms.Label lblIconDelegation;
        private System.Windows.Forms.Label lblIconSLA;
        private System.Windows.Forms.Label lblIconConfig;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlBreadcrumb;
        private System.Windows.Forms.Label lblBreadcrumb;
        private System.Windows.Forms.Panel pnlPageHeader;
        private System.Windows.Forms.Label lblPageTitle;
        private System.Windows.Forms.Label lblPageSubtitle;
        private System.Windows.Forms.Button btnCreateNew;
        private System.Windows.Forms.Panel pnlFilters;
        private System.Windows.Forms.Label lblCategoryFilter;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblStatusFilter;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblResultCount;
        private System.Windows.Forms.DataGridView dgvTemplates;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSteps;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOwner;
        private System.Windows.Forms.DataGridViewTextBoxColumn colModifiedDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUsage;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Label lblSelectionInfo;
        private System.Windows.Forms.LinkLabel lnkShowAll;

        // Search panel with icon
        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.Label lblSearchIcon;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle headerStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle centerStyle = new System.Windows.Forms.DataGridViewCellStyle();

            this.pnlRibbon = new System.Windows.Forms.Panel();
            this.pnlRibbonTabBar = new System.Windows.Forms.Panel();
            this.pnlRibbonContent = new System.Windows.Forms.Panel();
            this.pnlGrpTemplate = new System.Windows.Forms.Panel();
            this.pnlGrpPublish = new System.Windows.Forms.Panel();
            this.pnlGrpIO = new System.Windows.Forms.Panel();
            this.pnlSep1 = new System.Windows.Forms.Panel();
            this.pnlSep2 = new System.Windows.Forms.Panel();
            this.lblGrpTemplate = new System.Windows.Forms.Label();
            this.lblGrpPublish = new System.Windows.Forms.Label();
            this.lblGrpIO = new System.Windows.Forms.Label();
            this.lblTabSheet = new System.Windows.Forms.Label();
            this.lblTabHome = new System.Windows.Forms.Label();
            this.lblTabAction = new System.Windows.Forms.Label();
            this.lblTabView = new System.Windows.Forms.Label();
            this.lblTabTools = new System.Windows.Forms.Label();
            this.lblTabHelp = new System.Windows.Forms.Label();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnDuplicate = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnPublish = new System.Windows.Forms.Button();
            this.btnDeactivate = new System.Windows.Forms.Button();
            this.btnVersions = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlSidebarSep = new System.Windows.Forms.Panel();
            this.pnlNavActive = new System.Windows.Forms.Panel();
            this.lblSecWork = new System.Windows.Forms.Label();
            this.lblSecManage = new System.Windows.Forms.Label();
            this.lblSecSettings = new System.Windows.Forms.Label();
            this.lblNavMyTasks = new System.Windows.Forms.Label();
            this.lblNavMyTasksBadge = new System.Windows.Forms.Label();
            this.lblNavMonitor = new System.Windows.Forms.Label();
            this.lblNavMonitorBadge = new System.Windows.Forms.Label();
            this.lblNavStartWF = new System.Windows.Forms.Label();
            this.lblNavHistoryWF = new System.Windows.Forms.Label();
            this.lblNavTemplates = new System.Windows.Forms.Label();
            this.lblNavDesigner = new System.Windows.Forms.Label();
            this.lblNavApproval = new System.Windows.Forms.Label();
            this.lblNavApprovalBadge = new System.Windows.Forms.Label();
            this.lblNavDelegation = new System.Windows.Forms.Label();
            this.lblNavSLA = new System.Windows.Forms.Label();
            this.lblNavConfig = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlBreadcrumb = new System.Windows.Forms.Panel();
            this.lblBreadcrumb = new System.Windows.Forms.Label();
            this.pnlPageHeader = new System.Windows.Forms.Panel();
            this.lblPageTitle = new System.Windows.Forms.Label();
            this.lblPageSubtitle = new System.Windows.Forms.Label();
            this.btnCreateNew = new System.Windows.Forms.Button();
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.lblCategoryFilter = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblStatusFilter = new System.Windows.Forms.Label();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblResultCount = new System.Windows.Forms.Label();
            this.dgvTemplates = new System.Windows.Forms.DataGridView();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSteps = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOwner = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colModifiedDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUsage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.lblSelectionInfo = new System.Windows.Forms.Label();
            this.lnkShowAll = new System.Windows.Forms.LinkLabel();
            this.lblIconMyTasks = new System.Windows.Forms.Label();
            this.lblIconMonitor = new System.Windows.Forms.Label();
            this.lblIconStartWF = new System.Windows.Forms.Label();
            this.lblIconHistoryWF = new System.Windows.Forms.Label();
            this.lblIconTemplates = new System.Windows.Forms.Label();
            this.lblIconDesigner = new System.Windows.Forms.Label();
            this.lblIconApproval = new System.Windows.Forms.Label();
            this.lblIconDelegation = new System.Windows.Forms.Label();
            this.lblIconSLA = new System.Windows.Forms.Label();
            this.lblIconConfig = new System.Windows.Forms.Label();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.lblSearchIcon = new System.Windows.Forms.Label();

            this.pnlRibbon.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.pnlRibbonTabBar.SuspendLayout();
            this.pnlRibbonContent.SuspendLayout();
            this.pnlGrpTemplate.SuspendLayout();
            this.pnlGrpPublish.SuspendLayout();
            this.pnlGrpIO.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.pnlSidebar.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlBreadcrumb.SuspendLayout();
            this.pnlPageHeader.SuspendLayout();
            this.pnlFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTemplates)).BeginInit();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            this.pnlRibbon.BackColor = System.Drawing.Color.White;
            this.pnlRibbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbon.Height = 120;
            this.pnlRibbon.Name = "pnlRibbon";
            this.pnlRibbon.TabIndex = 0;
            this.pnlRibbon.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlRibbon_Paint);
            this.pnlRibbon.Controls.Add(this.pnlRibbonContent);
            this.pnlRibbon.Controls.Add(this.pnlRibbonTabBar);

            this.pnlRibbonTabBar.BackColor = System.Drawing.Color.FromArgb(248, 249, 252);
            this.pnlRibbonTabBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabBar.Height = 30;
            this.pnlRibbonTabBar.Name = "pnlRibbonTabBar";
            this.pnlRibbonTabBar.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblTabSheet, this.lblTabHome, this.lblTabAction,
                this.lblTabView, this.lblTabTools, this.lblTabHelp });
            this.pnlRibbonTabBar.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlRibbonTabBar_Paint);

            this.lblTabSheet.AutoSize = true;
            this.lblTabSheet.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTabSheet.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblTabSheet.Location = new System.Drawing.Point(4, 7);
            this.lblTabSheet.Name = "lblTabSheet";
            this.lblTabSheet.Text = "แผ่น";
            this.lblTabSheet.Cursor = System.Windows.Forms.Cursors.Hand;

            this.lblTabHome.AutoSize = true;
            this.lblTabHome.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTabHome.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblTabHome.Location = new System.Drawing.Point(52, 7);
            this.lblTabHome.Name = "lblTabHome";
            this.lblTabHome.Text = "หน้าแรก";
            this.lblTabHome.Cursor = System.Windows.Forms.Cursors.Hand;

            this.lblTabAction.AutoSize = true;
            this.lblTabAction.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTabAction.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblTabAction.Location = new System.Drawing.Point(110, 7);
            this.lblTabAction.Name = "lblTabAction";
            this.lblTabAction.Text = "ดำเนินการ";
            this.lblTabAction.Cursor = System.Windows.Forms.Cursors.Hand;

            this.lblTabView.AutoSize = true;
            this.lblTabView.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTabView.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblTabView.Location = new System.Drawing.Point(176, 7);
            this.lblTabView.Name = "lblTabView";
            this.lblTabView.Text = "มุมมอง";
            this.lblTabView.Cursor = System.Windows.Forms.Cursors.Hand;

            this.lblTabTools.AutoSize = true;
            this.lblTabTools.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTabTools.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblTabTools.Location = new System.Drawing.Point(228, 7);
            this.lblTabTools.Name = "lblTabTools";
            this.lblTabTools.Text = "เครื่องมือ";
            this.lblTabTools.Cursor = System.Windows.Forms.Cursors.Hand;

            this.lblTabHelp.AutoSize = true;
            this.lblTabHelp.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTabHelp.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblTabHelp.Location = new System.Drawing.Point(290, 7);
            this.lblTabHelp.Name = "lblTabHelp";
            this.lblTabHelp.Text = "ช่วยเหลือ";
            this.lblTabHelp.Cursor = System.Windows.Forms.Cursors.Hand;

            this.pnlRibbonContent.BackColor = System.Drawing.Color.White;
            this.pnlRibbonContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRibbonContent.Name = "pnlRibbonContent";
            this.pnlRibbonContent.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.pnlGrpTemplate, this.pnlSep1,
                this.pnlGrpPublish, this.pnlSep2,
                this.pnlGrpIO });

            this.pnlSep1.BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.pnlSep1.Location = new System.Drawing.Point(294, 4);
            this.pnlSep1.Size = new System.Drawing.Size(1, 72);
            this.pnlSep1.Name = "pnlSep1";

            this.pnlSep2.BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.pnlSep2.Location = new System.Drawing.Point(514, 4);
            this.pnlSep2.Size = new System.Drawing.Size(1, 72);
            this.pnlSep2.Name = "pnlSep2";

            this.pnlGrpTemplate.BackColor = System.Drawing.Color.White;
            this.pnlGrpTemplate.Location = new System.Drawing.Point(4, 2);
            this.pnlGrpTemplate.Size = new System.Drawing.Size(284, 82);
            this.pnlGrpTemplate.Name = "pnlGrpTemplate";
            this.pnlGrpTemplate.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.btnCreate, this.btnDuplicate, this.btnEdit, this.btnDelete, this.lblGrpTemplate });

            this.lblGrpTemplate.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblGrpTemplate.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblGrpTemplate.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblGrpTemplate.Height = 18;
            this.lblGrpTemplate.Name = "lblGrpTemplate";
            this.lblGrpTemplate.Text = "Template";
            this.lblGrpTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.btnCreate.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnCreate.FlatAppearance.BorderSize = 0;
            this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreate.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.btnCreate.ForeColor = System.Drawing.Color.White;
            this.btnCreate.Location = new System.Drawing.Point(4, 4);
            this.btnCreate.Size = new System.Drawing.Size(74, 62);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Text = "+\r\nสร้างใหม่";
            this.btnCreate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnCreate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);

            this.btnDuplicate.BackColor = System.Drawing.Color.White;
            this.btnDuplicate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.btnDuplicate.FlatAppearance.BorderSize = 1;
            this.btnDuplicate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDuplicate.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.btnDuplicate.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnDuplicate.Location = new System.Drawing.Point(84, 4);
            this.btnDuplicate.Size = new System.Drawing.Size(68, 62);
            this.btnDuplicate.Name = "btnDuplicate";
            this.btnDuplicate.Text = "Duplicate";
            this.btnDuplicate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnDuplicate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDuplicate.Click += new System.EventHandler(this.btnDuplicate_Click);

            this.btnEdit.BackColor = System.Drawing.Color.White;
            this.btnEdit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.btnEdit.FlatAppearance.BorderSize = 1;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.btnEdit.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnEdit.Location = new System.Drawing.Point(158, 4);
            this.btnEdit.Size = new System.Drawing.Size(58, 62);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Text = "แก้ไข";
            this.btnEdit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);

            this.btnDelete.BackColor = System.Drawing.Color.White;
            this.btnDelete.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.btnDelete.FlatAppearance.BorderSize = 1;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnDelete.Location = new System.Drawing.Point(222, 4);
            this.btnDelete.Size = new System.Drawing.Size(54, 62);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Text = "ลบ";
            this.btnDelete.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            this.pnlGrpPublish.BackColor = System.Drawing.Color.White;
            this.pnlGrpPublish.Location = new System.Drawing.Point(302, 2);
            this.pnlGrpPublish.Size = new System.Drawing.Size(206, 82);
            this.pnlGrpPublish.Name = "pnlGrpPublish";
            this.pnlGrpPublish.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.btnPublish, this.btnDeactivate, this.btnVersions, this.lblGrpPublish });

            this.lblGrpPublish.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblGrpPublish.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblGrpPublish.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblGrpPublish.Height = 18;
            this.lblGrpPublish.Name = "lblGrpPublish";
            this.lblGrpPublish.Text = "Publish";
            this.lblGrpPublish.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.btnPublish.BackColor = System.Drawing.Color.White;
            this.btnPublish.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.btnPublish.FlatAppearance.BorderSize = 1;
            this.btnPublish.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPublish.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.btnPublish.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnPublish.Location = new System.Drawing.Point(4, 4);
            this.btnPublish.Size = new System.Drawing.Size(64, 62);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Text = "Publish";
            this.btnPublish.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnPublish.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);

            this.btnDeactivate.BackColor = System.Drawing.Color.White;
            this.btnDeactivate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.btnDeactivate.FlatAppearance.BorderSize = 1;
            this.btnDeactivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeactivate.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.btnDeactivate.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnDeactivate.Location = new System.Drawing.Point(74, 4);
            this.btnDeactivate.Size = new System.Drawing.Size(74, 62);
            this.btnDeactivate.Name = "btnDeactivate";
            this.btnDeactivate.Text = "Deactivate";
            this.btnDeactivate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnDeactivate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeactivate.Click += new System.EventHandler(this.btnDeactivate_Click);

            this.btnVersions.BackColor = System.Drawing.Color.White;
            this.btnVersions.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.btnVersions.FlatAppearance.BorderSize = 1;
            this.btnVersions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVersions.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.btnVersions.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnVersions.Location = new System.Drawing.Point(154, 4);
            this.btnVersions.Size = new System.Drawing.Size(50, 62);
            this.btnVersions.Name = "btnVersions";
            this.btnVersions.Text = "ประวัติ";
            this.btnVersions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnVersions.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVersions.Click += new System.EventHandler(this.btnVersions_Click);

            this.pnlGrpIO.BackColor = System.Drawing.Color.White;
            this.pnlGrpIO.Location = new System.Drawing.Point(522, 2);
            this.pnlGrpIO.Size = new System.Drawing.Size(148, 82);
            this.pnlGrpIO.Name = "pnlGrpIO";
            this.pnlGrpIO.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.btnImport, this.btnExport, this.lblGrpIO });

            this.lblGrpIO.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblGrpIO.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblGrpIO.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblGrpIO.Height = 18;
            this.lblGrpIO.Name = "lblGrpIO";
            this.lblGrpIO.Text = "นำเข้า/ส่งออก";
            this.lblGrpIO.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.btnImport.BackColor = System.Drawing.Color.White;
            this.btnImport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.btnImport.FlatAppearance.BorderSize = 1;
            this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImport.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.btnImport.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnImport.Location = new System.Drawing.Point(4, 4);
            this.btnImport.Size = new System.Drawing.Size(64, 62);
            this.btnImport.Name = "btnImport";
            this.btnImport.Text = "Import";
            this.btnImport.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnImport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);

            this.btnExport.BackColor = System.Drawing.Color.White;
            this.btnExport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.btnExport.FlatAppearance.BorderSize = 1;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.btnExport.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnExport.Location = new System.Drawing.Point(74, 4);
            this.btnExport.Size = new System.Drawing.Size(64, 62);
            this.btnExport.Name = "btnExport";
            this.btnExport.Text = "Export";
            this.btnExport.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);

            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Controls.Add(this.pnlMain);
            this.pnlBody.Controls.Add(this.pnlSidebarSep);
            this.pnlBody.Controls.Add(this.pnlSidebar);

            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(26, 35, 67);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Width = 215;
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.pnlSidebar.AutoScroll = true;
            // Sidebar controls will be populated dynamically by WorkflowNavigationBuilder

            // Sidebar controls initialization removed — use WorkflowNavigationBuilder instead
            this.pnlSidebarSep.BackColor = System.Drawing.Color.FromArgb(42, 55, 95);
            this.pnlSidebarSep.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebarSep.Width = 1;
            this.pnlSidebarSep.Name = "pnlSidebarSep";

            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Controls.Add(this.dgvTemplates);
            this.pnlMain.Controls.Add(this.pnlFooter);
            this.pnlMain.Controls.Add(this.pnlFilters);
            this.pnlMain.Controls.Add(this.pnlPageHeader);
            this.pnlMain.Controls.Add(this.pnlBreadcrumb);

            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlBreadcrumb.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Height = 30;
            this.pnlBreadcrumb.Name = "pnlBreadcrumb";
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);

            this.lblBreadcrumb.AutoSize = true;
            this.lblBreadcrumb.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblBreadcrumb.Location = new System.Drawing.Point(16, 8);
            this.lblBreadcrumb.Name = "lblBreadcrumb";
            this.lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Templates";

            this.pnlPageHeader.BackColor = System.Drawing.Color.White;
            this.pnlPageHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPageHeader.Height = 72;
            this.pnlPageHeader.Name = "pnlPageHeader";
            this.pnlPageHeader.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblPageTitle, this.lblPageSubtitle, this.btnCreateNew });
            this.pnlPageHeader.Resize += new System.EventHandler(this.pnlPageHeader_Resize);

            this.lblPageTitle.AutoSize = true;
            this.lblPageTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblPageTitle.ForeColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.lblPageTitle.Location = new System.Drawing.Point(16, 10);
            this.lblPageTitle.Name = "lblPageTitle";
            this.lblPageTitle.Text = "จัดการ Template Workflow";

            this.lblPageSubtitle.AutoSize = true;
            this.lblPageSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPageSubtitle.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblPageSubtitle.Location = new System.Drawing.Point(16, 44);
            this.lblPageSubtitle.Name = "lblPageSubtitle";
            this.lblPageSubtitle.Text = "Template = พิมพ์เขียวที่ออกแบบไว้ล่วงหน้า — Freeze เมื่อเริ่ม Instance";

            this.btnCreateNew.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnCreateNew.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnCreateNew.FlatAppearance.BorderSize = 0;
            this.btnCreateNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreateNew.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCreateNew.ForeColor = System.Drawing.Color.White;
            this.btnCreateNew.Location = new System.Drawing.Point(840, 18);
            this.btnCreateNew.Size = new System.Drawing.Size(158, 36);
            this.btnCreateNew.Name = "btnCreateNew";
            this.btnCreateNew.Text = "+ สร้าง Template ใหม่";
            this.btnCreateNew.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreateNew.Click += new System.EventHandler(this.btnCreate_Click);

            this.pnlFilters.BackColor = System.Drawing.Color.White;
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Height = 52;
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblCategoryFilter, this.cmbCategory,
                this.lblStatusFilter, this.cmbStatus,
                this.pnlSearch, this.lblResultCount });
            this.pnlFilters.Resize += new System.EventHandler(this.pnlFilters_Resize);
            this.pnlFilters.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlFilters_Paint);

            this.lblCategoryFilter.AutoSize = true;
            this.lblCategoryFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCategoryFilter.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.lblCategoryFilter.Location = new System.Drawing.Point(16, 16);
            this.lblCategoryFilter.Name = "lblCategoryFilter";
            this.lblCategoryFilter.Text = "Category:";

            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbCategory.Location = new System.Drawing.Point(80, 12);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(130, 23);
            this.cmbCategory.TabIndex = 0;
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_Changed);

            this.lblStatusFilter.AutoSize = true;
            this.lblStatusFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusFilter.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.lblStatusFilter.Location = new System.Drawing.Point(222, 16);
            this.lblStatusFilter.Name = "lblStatusFilter";
            this.lblStatusFilter.Text = "Status:";

            this.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbStatus.Location = new System.Drawing.Point(272, 12);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Size = new System.Drawing.Size(130, 23);
            this.cmbStatus.TabIndex = 1;
            this.cmbStatus.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_Changed);

            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.txtSearch.Location = new System.Drawing.Point(28, 4);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(188, 19);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.Text = "ค้นหาชื่อ Template...";
            this.txtSearch.Enter += new System.EventHandler(this.txtSearch_Enter);
            this.txtSearch.Leave += new System.EventHandler(this.txtSearch_Leave);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);

            this.pnlSearch.BackColor = System.Drawing.Color.White;
            this.pnlSearch.Location = new System.Drawing.Point(416, 10);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(220, 27);
            this.pnlSearch.TabIndex = 2;
            this.pnlSearch.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSearch_Paint);
            this.pnlSearch.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblSearchIcon, this.txtSearch });

            this.lblSearchIcon.AutoSize = false;
            this.lblSearchIcon.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10.5F);
            this.lblSearchIcon.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblSearchIcon.Location = new System.Drawing.Point(5, 0);
            this.lblSearchIcon.Name = "lblSearchIcon";
            this.lblSearchIcon.Size = new System.Drawing.Size(22, 27);
            this.lblSearchIcon.Text = "";
            this.lblSearchIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblResultCount.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblResultCount.AutoSize = true;
            this.lblResultCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblResultCount.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblResultCount.Location = new System.Drawing.Point(660, 16);
            this.lblResultCount.Name = "lblResultCount";
            this.lblResultCount.Text = "พบ 0 Template ทั้งหมด";

            headerStyle.BackColor = System.Drawing.Color.FromArgb(248, 249, 252);
            headerStyle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            headerStyle.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            headerStyle.SelectionBackColor = System.Drawing.Color.FromArgb(248, 249, 252);
            headerStyle.SelectionForeColor = System.Drawing.Color.FromArgb(55, 65, 81);

            cellStyle.BackColor = System.Drawing.Color.White;
            cellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            cellStyle.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            cellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(238, 242, 255);
            cellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(29, 78, 216);
            cellStyle.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);

            centerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            centerStyle.BackColor = System.Drawing.Color.White;
            centerStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            centerStyle.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);

            this.dgvTemplates.AllowUserToAddRows = false;
            this.dgvTemplates.AllowUserToDeleteRows = false;
            this.dgvTemplates.AllowUserToResizeRows = false;
            this.dgvTemplates.BackgroundColor = System.Drawing.Color.White;
            this.dgvTemplates.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvTemplates.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvTemplates.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvTemplates.ColumnHeadersDefaultCellStyle = headerStyle;
            this.dgvTemplates.ColumnHeadersHeight = 36;
            this.dgvTemplates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvTemplates.DefaultCellStyle = cellStyle;
            this.dgvTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTemplates.GridColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.dgvTemplates.MultiSelect = false;
            this.dgvTemplates.Name = "dgvTemplates";
            this.dgvTemplates.ReadOnly = true;
            this.dgvTemplates.RowHeadersVisible = false;
            this.dgvTemplates.RowTemplate.Height = 40;
            this.dgvTemplates.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTemplates.TabIndex = 3;
            this.dgvTemplates.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colId, this.colName, this.colCategory, this.colSteps,
                this.colVersion, this.colStatus, this.colOwner,
                this.colModifiedDate, this.colUsage });
            this.dgvTemplates.SelectionChanged += new System.EventHandler(this.dgvTemplates_SelectionChanged);
            this.dgvTemplates.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvTemplates_CellPainting);

            this.colId.DataPropertyName = "DisplayId";
            this.colId.HeaderText = "รหัส";
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Width = 84;

            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "ชื่อ";
            this.colName.MinimumWidth = 140;
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;

            this.colCategory.DataPropertyName = "Category";
            this.colCategory.HeaderText = "Category";
            this.colCategory.Name = "colCategory";
            this.colCategory.ReadOnly = true;
            this.colCategory.Width = 110;

            this.colSteps.DataPropertyName = "StepCount";
            this.colSteps.DefaultCellStyle = centerStyle;
            this.colSteps.HeaderText = "Steps";
            this.colSteps.Name = "colSteps";
            this.colSteps.ReadOnly = true;
            this.colSteps.Width = 62;

            this.colVersion.DataPropertyName = "Version";
            this.colVersion.DefaultCellStyle = centerStyle;
            this.colVersion.HeaderText = "Version";
            this.colVersion.Name = "colVersion";
            this.colVersion.ReadOnly = true;
            this.colVersion.Width = 72;

            this.colStatus.DataPropertyName = "StatusLabel";
            this.colStatus.HeaderText = "สถานะ";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            this.colStatus.Width = 100;

            this.colOwner.DataPropertyName = "OwnerDepartment";
            this.colOwner.HeaderText = "เจ้าของ";
            this.colOwner.Name = "colOwner";
            this.colOwner.ReadOnly = true;
            this.colOwner.Width = 120;

            this.colModifiedDate.DataPropertyName = "ModifiedDate";
            this.colModifiedDate.HeaderText = "อัปเดต";
            this.colModifiedDate.Name = "colModifiedDate";
            this.colModifiedDate.ReadOnly = true;
            this.colModifiedDate.Width = 96;

            this.colUsage.DataPropertyName = "ActiveInstanceCount";
            this.colUsage.HeaderText = "ใช้งาน (30 วัน)";
            this.colUsage.Name = "colUsage";
            this.colUsage.ReadOnly = true;
            this.colUsage.Width = 130;

            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height = 36;
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblSelectionInfo, this.lnkShowAll });
            this.pnlFooter.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlFooter_Paint);
            this.pnlFooter.Resize += new System.EventHandler(this.pnlFooter_Resize);

            this.lblSelectionInfo.AutoSize = true;
            this.lblSelectionInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSelectionInfo.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblSelectionInfo.Location = new System.Drawing.Point(12, 10);
            this.lblSelectionInfo.Name = "lblSelectionInfo";
            this.lblSelectionInfo.Text = "เลือก 0 รายการ";

            this.lnkShowAll.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lnkShowAll.AutoSize = true;
            this.lnkShowAll.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lnkShowAll.Location = new System.Drawing.Point(880, 10);
            this.lnkShowAll.Name = "lnkShowAll";
            this.lnkShowAll.Text = "แสดงทั้งหมด";
            this.lnkShowAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkShowAll_LinkClicked);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1280, 760);
            this.MinimumSize = new System.Drawing.Size(1024, 680);
            this.Name = "WorkflowTemplateListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "จัดการ Template Workflow";
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlRibbon);
            this.Load += new System.EventHandler(this.WorkflowTemplateListForm_Load);

            this.pnlRibbon.ResumeLayout(false);
            this.pnlRibbonTabBar.ResumeLayout(false);
            this.pnlRibbonTabBar.PerformLayout();
            this.pnlRibbonContent.ResumeLayout(false);
            this.pnlGrpTemplate.ResumeLayout(false);
            this.pnlGrpPublish.ResumeLayout(false);
            this.pnlGrpIO.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlSidebar.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlBreadcrumb.ResumeLayout(false);
            this.pnlBreadcrumb.PerformLayout();
            this.pnlPageHeader.ResumeLayout(false);
            this.pnlPageHeader.PerformLayout();
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTemplates)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
