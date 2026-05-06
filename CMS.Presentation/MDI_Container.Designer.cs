namespace CMS.Presentation
{
    partial class MDI_Container
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MDI_Container));
            this.pnlDashboard = new System.Windows.Forms.Panel();
            this.pnlDashContent = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblRecentFiles = new System.Windows.Forms.Label();
            this.lblStats = new System.Windows.Forms.Label();
            this.lblStatsContent = new System.Windows.Forms.Label();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblQuickAccess = new System.Windows.Forms.Label();
            this.btnDashOrgUnit = new System.Windows.Forms.Button();
            this.btnDashUpload = new System.Windows.Forms.Button();
            this.btnDashApproval = new System.Windows.Forms.Button();
            this.btnDashFolder = new System.Windows.Forms.Button();
            this.lvRecentFiles = new System.Windows.Forms.ListView();
            this.splitterDash = new System.Windows.Forms.Splitter();
            this.pnlDashLeft = new System.Windows.Forms.Panel();
            this.treeViewFolders = new System.Windows.Forms.TreeView();
            this.txtSearchFolder = new System.Windows.Forms.TextBox();
            this.pnlTreeViewToggle = new System.Windows.Forms.Panel();
            this.btnToggleByOrg = new System.Windows.Forms.Button();
            this.btnToggleByConfidentiality = new System.Windows.Forms.Button();
            this.lblTreeTitle = new System.Windows.Forms.Label();
            this.lblDashTitle = new System.Windows.Forms.Label();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuItemHome = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAdmin = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemShare = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemManageShares = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemUpload = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemNewFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemDownload = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemRename = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMove = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemFavorites = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemFolderSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemPermission = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAuditLog = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemApproval = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemInsert = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemInsertDoc = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemInsertImage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemInsertZip = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemView = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemViewList = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemViewGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemViewOrg = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemTools = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemVirusScan = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemChecksum = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemRecycleBin = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemArchive = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemOrgAdmin = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemReportExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemPDPA = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMod001 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMod002 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMod003 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMod004 = new System.Windows.Forms.ToolStripMenuItem();
            this.ribbonStrip = new System.Windows.Forms.ToolStrip();
            this.btnRibbonUpload = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonNewFolder = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonDownload = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonRename = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonMove = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.sepRibbon1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRibbonTemplate = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonFavorites = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonFolderSettings = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonPermission = new System.Windows.Forms.ToolStripButton();
            this.sepRibbon2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRibbonAuditLog = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonApproval = new System.Windows.Forms.ToolStripButton();
            this.lblApprovalBadge = new System.Windows.Forms.ToolStripLabel();
            this.btnRibbonUserAccess = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonUserMgmt = new System.Windows.Forms.ToolStripButton();
            this.ribbonStripAdmin = new System.Windows.Forms.ToolStrip();
            this.btnRibbonSysConfig = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonOrgAdmin = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonReportExcel = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonPDPA = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonFolderArchive = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonCopyStructure = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonChangeLevel = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonRecycleBin = new System.Windows.Forms.ToolStripButton();
            this.sepRibbon3 = new System.Windows.Forms.ToolStripSeparator();
            this.sepRibbon5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRibbonViewList = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonViewGrid = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonViewOrg = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonVirusScan = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonChecksum = new System.Windows.Forms.ToolStripButton();
            this.btnRibbonArchive = new System.Windows.Forms.ToolStripButton();
            this.sepRibbon4 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatusReady = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusUser = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusDept = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusFolder = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusPath = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusDateTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlDashboard.SuspendLayout();
            this.pnlDashContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlDashLeft.SuspendLayout();
            this.pnlTreeViewToggle.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.ribbonStrip.SuspendLayout();
            this.ribbonStripAdmin.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlDashboard
            // 
            this.pnlDashboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.pnlDashboard.Controls.Add(this.pnlDashContent);
            this.pnlDashboard.Controls.Add(this.splitterDash);
            this.pnlDashboard.Controls.Add(this.pnlDashLeft);
            this.pnlDashboard.Controls.Add(this.lblDashTitle);
            this.pnlDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.Location = new System.Drawing.Point(0, 52);
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.Size = new System.Drawing.Size(1469, 694);
            this.pnlDashboard.TabIndex = 0;
            // 
            // pnlDashContent
            // 
            this.pnlDashContent.AutoScroll = true;
            this.pnlDashContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.pnlDashContent.Controls.Add(this.splitContainer1);
            this.pnlDashContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashContent.Location = new System.Drawing.Point(304, 50);
            this.pnlDashContent.Name = "pnlDashContent";
            this.pnlDashContent.Padding = new System.Windows.Forms.Padding(20);
            this.pnlDashContent.Size = new System.Drawing.Size(1165, 644);
            this.pnlDashContent.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(20, 20);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblRecentFiles);
            this.splitContainer1.Panel1.Controls.Add(this.lblStats);
            this.splitContainer1.Panel1.Controls.Add(this.lblStatsContent);
            this.splitContainer1.Panel1.Controls.Add(this.lblWelcome);
            this.splitContainer1.Panel1.Controls.Add(this.lblQuickAccess);
            this.splitContainer1.Panel1.Controls.Add(this.btnDashOrgUnit);
            this.splitContainer1.Panel1.Controls.Add(this.btnDashUpload);
            this.splitContainer1.Panel1.Controls.Add(this.btnDashApproval);
            this.splitContainer1.Panel1.Controls.Add(this.btnDashFolder);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lvRecentFiles);
            this.splitContainer1.Size = new System.Drawing.Size(1125, 604);
            this.splitContainer1.SplitterDistance = 258;
            this.splitContainer1.TabIndex = 10;
            // 
            // lblRecentFiles
            // 
            this.lblRecentFiles.AutoSize = true;
            this.lblRecentFiles.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblRecentFiles.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblRecentFiles.Location = new System.Drawing.Point(200, 217);
            this.lblRecentFiles.Name = "lblRecentFiles";
            this.lblRecentFiles.Size = new System.Drawing.Size(160, 20);
            this.lblRecentFiles.TabIndex = 8;
            this.lblRecentFiles.Text = "📄 ไฟล์ที่อัพโหลดล่าสุด";
            // 
            // lblStats
            // 
            this.lblStats.AutoSize = true;
            this.lblStats.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblStats.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblStats.Location = new System.Drawing.Point(823, 147);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(94, 20);
            this.lblStats.TabIndex = 6;
            this.lblStats.Text = "📈 สถิติระบบ";
            // 
            // lblStatsContent
            // 
            this.lblStatsContent.AutoSize = true;
            this.lblStatsContent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatsContent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblStatsContent.Location = new System.Drawing.Point(823, 177);
            this.lblStatsContent.Name = "lblStatsContent";
            this.lblStatsContent.Size = new System.Drawing.Size(102, 60);
            this.lblStatsContent.TabIndex = 7;
            this.lblStatsContent.Text = "• ไฟล์ทั้งหมด: --\n• Folder ทั้งหมด: --\n• รายการรออนุมัติ: --\n• ผู้ใช้งาน: --";
            this.lblStatsContent.Click += new System.EventHandler(this.lblStatsContent_Click);
            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblWelcome.Location = new System.Drawing.Point(20, 16);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(356, 25);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "ยินดีต้อนรับสู่ Core Management System";
            // 
            // lblQuickAccess
            // 
            this.lblQuickAccess.AutoSize = true;
            this.lblQuickAccess.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblQuickAccess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblQuickAccess.Location = new System.Drawing.Point(40, 51);
            this.lblQuickAccess.Name = "lblQuickAccess";
            this.lblQuickAccess.Size = new System.Drawing.Size(155, 20);
            this.lblQuickAccess.TabIndex = 1;
            this.lblQuickAccess.Text = "🚀 เข้าถึงอย่างรวดเร็ว";
            // 
            // btnDashOrgUnit
            // 
            this.btnDashOrgUnit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnDashOrgUnit.FlatAppearance.BorderSize = 0;
            this.btnDashOrgUnit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashOrgUnit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDashOrgUnit.ForeColor = System.Drawing.Color.White;
            this.btnDashOrgUnit.Location = new System.Drawing.Point(490, 91);
            this.btnDashOrgUnit.Name = "btnDashOrgUnit";
            this.btnDashOrgUnit.Size = new System.Drawing.Size(140, 60);
            this.btnDashOrgUnit.TabIndex = 5;
            this.btnDashOrgUnit.Text = "🏢 โครงสร้างองค์กร";
            this.btnDashOrgUnit.UseVisualStyleBackColor = false;
            // 
            // btnDashUpload
            // 
            this.btnDashUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnDashUpload.FlatAppearance.BorderSize = 0;
            this.btnDashUpload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashUpload.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDashUpload.ForeColor = System.Drawing.Color.White;
            this.btnDashUpload.Location = new System.Drawing.Point(40, 91);
            this.btnDashUpload.Name = "btnDashUpload";
            this.btnDashUpload.Size = new System.Drawing.Size(140, 60);
            this.btnDashUpload.TabIndex = 2;
            this.btnDashUpload.Text = "📤 อัพโหลดไฟล์";
            this.btnDashUpload.UseVisualStyleBackColor = false;
            // 
            // btnDashApproval
            // 
            this.btnDashApproval.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnDashApproval.FlatAppearance.BorderSize = 0;
            this.btnDashApproval.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashApproval.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDashApproval.ForeColor = System.Drawing.Color.White;
            this.btnDashApproval.Location = new System.Drawing.Point(340, 91);
            this.btnDashApproval.Name = "btnDashApproval";
            this.btnDashApproval.Size = new System.Drawing.Size(140, 60);
            this.btnDashApproval.TabIndex = 4;
            this.btnDashApproval.Text = "✅ อนุมัติ";
            this.btnDashApproval.UseVisualStyleBackColor = false;
            // 
            // btnDashFolder
            // 
            this.btnDashFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnDashFolder.FlatAppearance.BorderSize = 0;
            this.btnDashFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashFolder.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDashFolder.ForeColor = System.Drawing.Color.White;
            this.btnDashFolder.Location = new System.Drawing.Point(190, 91);
            this.btnDashFolder.Name = "btnDashFolder";
            this.btnDashFolder.Size = new System.Drawing.Size(140, 60);
            this.btnDashFolder.TabIndex = 3;
            this.btnDashFolder.Text = "📁 สร้าง Folder";
            this.btnDashFolder.UseVisualStyleBackColor = false;
            // 
            // lvRecentFiles
            // 
            this.lvRecentFiles.BackColor = System.Drawing.Color.White;
            this.lvRecentFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvRecentFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvRecentFiles.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lvRecentFiles.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.lvRecentFiles.FullRowSelect = true;
            this.lvRecentFiles.GridLines = true;
            this.lvRecentFiles.HideSelection = false;
            this.lvRecentFiles.Location = new System.Drawing.Point(0, 0);
            this.lvRecentFiles.Name = "lvRecentFiles";
            this.lvRecentFiles.Size = new System.Drawing.Size(1125, 342);
            this.lvRecentFiles.TabIndex = 9;
            this.lvRecentFiles.UseCompatibleStateImageBehavior = false;
            this.lvRecentFiles.View = System.Windows.Forms.View.Details;
            // 
            // splitterDash
            // 
            this.splitterDash.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(211)))), ((int)(((byte)(232)))));
            this.splitterDash.Location = new System.Drawing.Point(300, 50);
            this.splitterDash.Name = "splitterDash";
            this.splitterDash.Size = new System.Drawing.Size(4, 644);
            this.splitterDash.TabIndex = 1;
            this.splitterDash.TabStop = false;
            // 
            // pnlDashLeft
            // 
            this.pnlDashLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.pnlDashLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDashLeft.Controls.Add(this.treeViewFolders);
            this.pnlDashLeft.Controls.Add(this.txtSearchFolder);
            this.pnlDashLeft.Controls.Add(this.pnlTreeViewToggle);
            this.pnlDashLeft.Controls.Add(this.lblTreeTitle);
            this.pnlDashLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlDashLeft.Location = new System.Drawing.Point(0, 50);
            this.pnlDashLeft.Name = "pnlDashLeft";
            this.pnlDashLeft.Padding = new System.Windows.Forms.Padding(8);
            this.pnlDashLeft.Size = new System.Drawing.Size(300, 644);
            this.pnlDashLeft.TabIndex = 2;
            // 
            // treeViewFolders
            // 
            this.treeViewFolders.BackColor = System.Drawing.Color.White;
            this.treeViewFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewFolders.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.treeViewFolders.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.treeViewFolders.ItemHeight = 22;
            this.treeViewFolders.Location = new System.Drawing.Point(8, 97);
            this.treeViewFolders.Margin = new System.Windows.Forms.Padding(2);
            this.treeViewFolders.Name = "treeViewFolders";
            this.treeViewFolders.Size = new System.Drawing.Size(282, 537);
            this.treeViewFolders.TabIndex = 0;
            // 
            // txtSearchFolder
            // 
            this.txtSearchFolder.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtSearchFolder.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearchFolder.ForeColor = System.Drawing.Color.Gray;
            this.txtSearchFolder.Location = new System.Drawing.Point(8, 74);
            this.txtSearchFolder.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.txtSearchFolder.Name = "txtSearchFolder";
            this.txtSearchFolder.Size = new System.Drawing.Size(282, 23);
            this.txtSearchFolder.TabIndex = 1;
            this.txtSearchFolder.Text = "🔍 ค้นหา Folder...";
            // 
            // pnlTreeViewToggle
            // 
            this.pnlTreeViewToggle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.pnlTreeViewToggle.Controls.Add(this.btnToggleByOrg);
            this.pnlTreeViewToggle.Controls.Add(this.btnToggleByConfidentiality);
            this.pnlTreeViewToggle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTreeViewToggle.Location = new System.Drawing.Point(8, 38);
            this.pnlTreeViewToggle.Name = "pnlTreeViewToggle";
            this.pnlTreeViewToggle.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.pnlTreeViewToggle.Size = new System.Drawing.Size(282, 36);
            this.pnlTreeViewToggle.TabIndex = 2;
            // 
            // btnToggleByOrg
            // 
            this.btnToggleByOrg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnToggleByOrg.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnToggleByOrg.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnToggleByOrg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleByOrg.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnToggleByOrg.ForeColor = System.Drawing.Color.White;
            this.btnToggleByOrg.Location = new System.Drawing.Point(0, 4);
            this.btnToggleByOrg.Name = "btnToggleByOrg";
            this.btnToggleByOrg.Size = new System.Drawing.Size(103, 28);
            this.btnToggleByOrg.TabIndex = 0;
            this.btnToggleByOrg.Text = "🏢 ตามองค์กร";
            this.btnToggleByOrg.UseVisualStyleBackColor = false;
            // 
            // btnToggleByConfidentiality
            // 
            this.btnToggleByConfidentiality.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnToggleByConfidentiality.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnToggleByConfidentiality.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnToggleByConfidentiality.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleByConfidentiality.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnToggleByConfidentiality.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnToggleByConfidentiality.Location = new System.Drawing.Point(0, 4);
            this.btnToggleByConfidentiality.Name = "btnToggleByConfidentiality";
            this.btnToggleByConfidentiality.Size = new System.Drawing.Size(282, 28);
            this.btnToggleByConfidentiality.TabIndex = 1;
            this.btnToggleByConfidentiality.Text = "🔒 ตามความลับ";
            this.btnToggleByConfidentiality.UseVisualStyleBackColor = false;
            // 
            // lblTreeTitle
            // 
            this.lblTreeTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTreeTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTreeTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblTreeTitle.Location = new System.Drawing.Point(8, 8);
            this.lblTreeTitle.Name = "lblTreeTitle";
            this.lblTreeTitle.Size = new System.Drawing.Size(282, 30);
            this.lblTreeTitle.TabIndex = 2;
            this.lblTreeTitle.Text = "📁 ตามองค์กร";
            this.lblTreeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDashTitle
            // 
            this.lblDashTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblDashTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDashTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblDashTitle.ForeColor = System.Drawing.Color.White;
            this.lblDashTitle.Location = new System.Drawing.Point(0, 0);
            this.lblDashTitle.Name = "lblDashTitle";
            this.lblDashTitle.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblDashTitle.Size = new System.Drawing.Size(1469, 50);
            this.lblDashTitle.TabIndex = 3;
            this.lblDashTitle.Text = "📊 Dashboard";
            this.lblDashTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.menuStrip.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.menuStrip.ForeColor = System.Drawing.Color.White;
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemHome,
            this.menuItemAdmin,
            this.menuItemShare,
            this.menuItemManageShares});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1469, 27);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Visible = false;  // Hide the top menu bar with Share buttons
            // 
            // menuItemHome
            // 
            this.menuItemHome.Name = "menuItemHome";
            this.menuItemHome.Size = new System.Drawing.Size(67, 23);
            this.menuItemHome.Text = "หน้าหลัก";
            // 
            // menuItemAdmin
            // 
            this.menuItemAdmin.Name = "menuItemAdmin";
            this.menuItemAdmin.Size = new System.Drawing.Size(52, 23);
            this.menuItemAdmin.Text = "ผู้ดูแล";
            // 
            // menuItemShare
            // 
            this.menuItemShare.Name = "menuItemShare";
            this.menuItemShare.Size = new System.Drawing.Size(78, 23);
            this.menuItemShare.Text = "📤 Share";
            // 
            // menuItemManageShares
            // 
            this.menuItemManageShares.Name = "menuItemManageShares";
            this.menuItemManageShares.Size = new System.Drawing.Size(135, 23);
            this.menuItemManageShares.Text = "📋 Manage Shares";
            // 
            // menuItemUpload
            // 
            this.menuItemUpload.Name = "menuItemUpload";
            this.menuItemUpload.Size = new System.Drawing.Size(149, 22);
            this.menuItemUpload.Text = "📤 อัพโหลด";
            // 
            // menuItemNewFolder
            // 
            this.menuItemNewFolder.Name = "menuItemNewFolder";
            this.menuItemNewFolder.Size = new System.Drawing.Size(149, 22);
            this.menuItemNewFolder.Text = "📁 Folder ใหม่";
            // 
            // menuItemDownload
            // 
            this.menuItemDownload.Name = "menuItemDownload";
            this.menuItemDownload.Size = new System.Drawing.Size(149, 22);
            this.menuItemDownload.Text = "⬇ ดาวน์โหลด";
            // 
            // menuItemRename
            // 
            this.menuItemRename.Name = "menuItemRename";
            this.menuItemRename.Size = new System.Drawing.Size(149, 22);
            this.menuItemRename.Text = "✏️ เปลี่ยนชื่อ";
            // 
            // menuItemMove
            // 
            this.menuItemMove.Name = "menuItemMove";
            this.menuItemMove.Size = new System.Drawing.Size(149, 22);
            this.menuItemMove.Text = "✂️ ย้าย";
            // 
            // menuItemDelete
            // 
            this.menuItemDelete.Name = "menuItemDelete";
            this.menuItemDelete.Size = new System.Drawing.Size(149, 22);
            this.menuItemDelete.Text = "🗑️ ลบ";
            // 
            // menuItemTemplate
            // 
            this.menuItemTemplate.Name = "menuItemTemplate";
            this.menuItemTemplate.Size = new System.Drawing.Size(149, 22);
            this.menuItemTemplate.Text = "📋 Template";
            // 
            // menuItemFavorites
            // 
            this.menuItemFavorites.Name = "menuItemFavorites";
            this.menuItemFavorites.Size = new System.Drawing.Size(149, 22);
            this.menuItemFavorites.Text = "⭐ Favorites";
            // 
            // menuItemFolderSettings
            // 
            this.menuItemFolderSettings.Name = "menuItemFolderSettings";
            this.menuItemFolderSettings.Size = new System.Drawing.Size(149, 22);
            this.menuItemFolderSettings.Text = "⚙️ ตั้งค่า Folder";
            // 
            // menuItemPermission
            // 
            this.menuItemPermission.Name = "menuItemPermission";
            this.menuItemPermission.Size = new System.Drawing.Size(149, 22);
            this.menuItemPermission.Text = "🔒 สิทธิ์";
            // 
            // menuItemAuditLog
            // 
            this.menuItemAuditLog.Name = "menuItemAuditLog";
            this.menuItemAuditLog.Size = new System.Drawing.Size(149, 22);
            this.menuItemAuditLog.Text = "📜 Audit Log";
            // 
            // menuItemApproval
            // 
            this.menuItemApproval.Name = "menuItemApproval";
            this.menuItemApproval.Size = new System.Drawing.Size(149, 22);
            this.menuItemApproval.Text = "✅ อนุมัติ";
            // 
            // menuItemInsert
            // 
            this.menuItemInsert.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemInsertDoc,
            this.menuItemInsertImage,
            this.menuItemInsertZip});
            this.menuItemInsert.Name = "menuItemInsert";
            this.menuItemInsert.Size = new System.Drawing.Size(44, 20);
            this.menuItemInsert.Text = "แทรก";
            // 
            // menuItemInsertDoc
            // 
            this.menuItemInsertDoc.Name = "menuItemInsertDoc";
            this.menuItemInsertDoc.Size = new System.Drawing.Size(134, 22);
            this.menuItemInsertDoc.Text = "📄 เอกสาร";
            // 
            // menuItemInsertImage
            // 
            this.menuItemInsertImage.Name = "menuItemInsertImage";
            this.menuItemInsertImage.Size = new System.Drawing.Size(134, 22);
            this.menuItemInsertImage.Text = "🖼️ รูปภาพ";
            // 
            // menuItemInsertZip
            // 
            this.menuItemInsertZip.Name = "menuItemInsertZip";
            this.menuItemInsertZip.Size = new System.Drawing.Size(134, 22);
            this.menuItemInsertZip.Text = "🗜️ ไฟล์บีบอัด";
            // 
            // menuItemView
            // 
            this.menuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemViewList,
            this.menuItemViewGrid,
            this.menuItemViewOrg});
            this.menuItemView.Name = "menuItemView";
            this.menuItemView.Size = new System.Drawing.Size(51, 20);
            this.menuItemView.Text = "มุมมอง";
            // 
            // menuItemViewList
            // 
            this.menuItemViewList.Name = "menuItemViewList";
            this.menuItemViewList.Size = new System.Drawing.Size(162, 22);
            this.menuItemViewList.Text = "☰ รายการ";
            // 
            // menuItemViewGrid
            // 
            this.menuItemViewGrid.Name = "menuItemViewGrid";
            this.menuItemViewGrid.Size = new System.Drawing.Size(162, 22);
            this.menuItemViewGrid.Text = "⊞ ตาราง";
            // 
            // menuItemViewOrg
            // 
            this.menuItemViewOrg.Name = "menuItemViewOrg";
            this.menuItemViewOrg.Size = new System.Drawing.Size(162, 22);
            this.menuItemViewOrg.Text = "🏢 โครงสร้างองค์กร";
            // 
            // menuItemTools
            // 
            this.menuItemTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemVirusScan,
            this.menuItemChecksum,
            this.menuItemRecycleBin,
            this.menuItemArchive});
            this.menuItemTools.Name = "menuItemTools";
            this.menuItemTools.Size = new System.Drawing.Size(58, 20);
            this.menuItemTools.Text = "เครื่องมือ";
            // 
            // menuItemVirusScan
            // 
            this.menuItemVirusScan.Name = "menuItemVirusScan";
            this.menuItemVirusScan.Size = new System.Drawing.Size(145, 22);
            this.menuItemVirusScan.Text = "🛡️ Virus Scan";
            // 
            // menuItemChecksum
            // 
            this.menuItemChecksum.Name = "menuItemChecksum";
            this.menuItemChecksum.Size = new System.Drawing.Size(145, 22);
            this.menuItemChecksum.Text = "🔢 Checksum";
            // 
            // menuItemRecycleBin
            // 
            this.menuItemRecycleBin.Name = "menuItemRecycleBin";
            this.menuItemRecycleBin.Size = new System.Drawing.Size(145, 22);
            this.menuItemRecycleBin.Text = "🗑️ ถังขยะ";
            // 
            // menuItemArchive
            // 
            this.menuItemArchive.Name = "menuItemArchive";
            this.menuItemArchive.Size = new System.Drawing.Size(145, 22);
            this.menuItemArchive.Text = "📦 Archive";
            // 
            // menuItemConfig
            // 
            this.menuItemConfig.Name = "menuItemConfig";
            this.menuItemConfig.Size = new System.Drawing.Size(165, 22);
            this.menuItemConfig.Text = "⚙️ ตั้งค่าระบบ";
            // 
            // menuItemOrgAdmin
            // 
            this.menuItemOrgAdmin.Name = "menuItemOrgAdmin";
            this.menuItemOrgAdmin.Size = new System.Drawing.Size(165, 22);
            this.menuItemOrgAdmin.Text = "🏢 โครงสร้างองค์กร";
            // 
            // menuItemReportExcel
            // 
            this.menuItemReportExcel.Name = "menuItemReportExcel";
            this.menuItemReportExcel.Size = new System.Drawing.Size(165, 22);
            this.menuItemReportExcel.Text = "📊 รายงาน Excel";
            // 
            // menuItemPDPA
            // 
            this.menuItemPDPA.Name = "menuItemPDPA";
            this.menuItemPDPA.Size = new System.Drawing.Size(165, 22);
            this.menuItemPDPA.Text = "🔏 PDPA Consent";
            // 
            // menuItemMod001
            // 
            this.menuItemMod001.Name = "menuItemMod001";
            this.menuItemMod001.Size = new System.Drawing.Size(32, 19);
            this.menuItemMod001.Visible = false;
            // 
            // menuItemMod002
            // 
            this.menuItemMod002.Name = "menuItemMod002";
            this.menuItemMod002.Size = new System.Drawing.Size(32, 19);
            this.menuItemMod002.Visible = false;
            // 
            // menuItemMod003
            // 
            this.menuItemMod003.Name = "menuItemMod003";
            this.menuItemMod003.Size = new System.Drawing.Size(32, 19);
            this.menuItemMod003.Visible = false;
            // 
            // menuItemMod004
            // 
            this.menuItemMod004.Name = "menuItemMod004";
            this.menuItemMod004.Size = new System.Drawing.Size(32, 19);
            this.menuItemMod004.Visible = false;
            // 
            // ribbonStrip
            // 
            this.ribbonStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.ribbonStrip.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ribbonStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.ribbonStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ribbonStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ribbonStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRibbonUpload,
            this.btnRibbonNewFolder,
            this.btnRibbonDownload,
            this.btnRibbonRename,
            this.btnRibbonMove,
            this.btnRibbonDelete,
            this.toolStripButton1});
            this.ribbonStrip.Location = new System.Drawing.Point(0, 27);
            this.ribbonStrip.Name = "ribbonStrip";
            this.ribbonStrip.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.ribbonStrip.Size = new System.Drawing.Size(1469, 25);
            this.ribbonStrip.TabIndex = 1;
            // 
            // btnRibbonUpload
            // 
            this.btnRibbonUpload.Name = "btnRibbonUpload";
            this.btnRibbonUpload.Size = new System.Drawing.Size(80, 24);
            this.btnRibbonUpload.Text = "📤 อัพโหลด";
            this.btnRibbonUpload.ToolTipText = "อัพโหลดไฟล์ไปยัง Folder ที่เลือก";
            this.btnRibbonUpload.Visible = false;
            // 
            // btnRibbonNewFolder
            // 
            this.btnRibbonNewFolder.Name = "btnRibbonNewFolder";
            this.btnRibbonNewFolder.Size = new System.Drawing.Size(99, 24);
            this.btnRibbonNewFolder.Text = "📁 Folder ใหม่";
            this.btnRibbonNewFolder.ToolTipText = "สร้าง Folder ใหม่";
            this.btnRibbonNewFolder.Visible = false;
            // 
            // btnRibbonDownload
            // 
            this.btnRibbonDownload.Name = "btnRibbonDownload";
            this.btnRibbonDownload.Size = new System.Drawing.Size(83, 24);
            this.btnRibbonDownload.Text = "⬇ ดาวน์โหลด";
            this.btnRibbonDownload.ToolTipText = "ดาวน์โหลดไฟล์ที่เลือก";
            this.btnRibbonDownload.Visible = false;
            // 
            // btnRibbonRename
            // 
            this.btnRibbonRename.Name = "btnRibbonRename";
            this.btnRibbonRename.Size = new System.Drawing.Size(86, 24);
            this.btnRibbonRename.Text = "✏️ เปลี่ยนชื่อ";
            this.btnRibbonRename.ToolTipText = "เปลี่ยนชื่อรายการที่เลือก";
            this.btnRibbonRename.Visible = false;
            // 
            // btnRibbonMove
            // 
            this.btnRibbonMove.Name = "btnRibbonMove";
            this.btnRibbonMove.Size = new System.Drawing.Size(56, 24);
            this.btnRibbonMove.Text = "✂️ ย้าย";
            this.btnRibbonMove.ToolTipText = "ย้ายรายการที่เลือก";
            this.btnRibbonMove.Visible = false;
            // 
            // btnRibbonDelete
            // 
            this.btnRibbonDelete.Name = "btnRibbonDelete";
            this.btnRibbonDelete.Size = new System.Drawing.Size(52, 24);
            this.btnRibbonDelete.Text = "🗑️ ลบ";
            this.btnRibbonDelete.ToolTipText = "ลบรายการที่เลือก";
            this.btnRibbonDelete.Visible = false;
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Visible = false;
            // 
            // sepRibbon1
            // 
            this.sepRibbon1.Name = "sepRibbon1";
            this.sepRibbon1.Size = new System.Drawing.Size(6, 26);
            // 
            // btnRibbonTemplate
            // 
            this.btnRibbonTemplate.Name = "btnRibbonTemplate";
            this.btnRibbonTemplate.Size = new System.Drawing.Size(88, 23);
            this.btnRibbonTemplate.Text = "📋 Template";
            this.btnRibbonTemplate.ToolTipText = "Apply Folder Template";
            // 
            // btnRibbonFavorites
            // 
            this.btnRibbonFavorites.Name = "btnRibbonFavorites";
            this.btnRibbonFavorites.Size = new System.Drawing.Size(84, 23);
            this.btnRibbonFavorites.Text = "⭐ Favorites";
            this.btnRibbonFavorites.ToolTipText = "รายการโปรด";
            this.btnRibbonFavorites.Visible = false;
            // 
            // btnRibbonFolderSettings
            // 
            this.btnRibbonFolderSettings.Name = "btnRibbonFolderSettings";
            this.btnRibbonFolderSettings.Size = new System.Drawing.Size(105, 23);
            this.btnRibbonFolderSettings.Text = "⚙️ ตั้งค่า Folder";
            this.btnRibbonFolderSettings.ToolTipText = "ตั้งค่า Folder ที่เลือก";
            // 
            // btnRibbonPermission
            // 
            this.btnRibbonPermission.Name = "btnRibbonPermission";
            this.btnRibbonPermission.Size = new System.Drawing.Size(56, 23);
            this.btnRibbonPermission.Text = "🔒 สิทธิ์";
            this.btnRibbonPermission.ToolTipText = "จัดการสิทธิ์การเข้าถึง";
            // 
            // sepRibbon2
            // 
            this.sepRibbon2.Name = "sepRibbon2";
            this.sepRibbon2.Size = new System.Drawing.Size(6, 26);
            // 
            // btnRibbonAuditLog
            // 
            this.btnRibbonAuditLog.Name = "btnRibbonAuditLog";
            this.btnRibbonAuditLog.Size = new System.Drawing.Size(96, 23);
            this.btnRibbonAuditLog.Text = "📜 Audit Log";
            this.btnRibbonAuditLog.ToolTipText = "ดูประวัติการใช้งาน";
            // 
            // btnRibbonApproval
            // 
            this.btnRibbonApproval.Name = "btnRibbonApproval";
            this.btnRibbonApproval.Size = new System.Drawing.Size(67, 23);
            this.btnRibbonApproval.Text = "✅ อนุมัติ";
            this.btnRibbonApproval.ToolTipText = "รายการรออนุมัติ";
            // 
            // lblApprovalBadge
            // 
            this.lblApprovalBadge.BackColor = System.Drawing.Color.Crimson;
            this.lblApprovalBadge.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblApprovalBadge.ForeColor = System.Drawing.Color.White;
            this.lblApprovalBadge.Name = "lblApprovalBadge";
            this.lblApprovalBadge.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.lblApprovalBadge.Size = new System.Drawing.Size(6, 23);
            this.lblApprovalBadge.Visible = false;
            // 
            // btnRibbonUserAccess
            // 
            this.btnRibbonUserAccess.Name = "btnRibbonUserAccess";
            this.btnRibbonUserAccess.Size = new System.Drawing.Size(112, 23);
            this.btnRibbonUserAccess.Text = "👤 จัดการสิทธิ์ผู้ใช้";
            this.btnRibbonUserAccess.ToolTipText = "จัดการสิทธิ์และบัญชีผู้ใช้";
            // 
            // btnRibbonUserMgmt
            // 
            this.btnRibbonUserMgmt.Name = "btnRibbonUserMgmt";
            this.btnRibbonUserMgmt.Size = new System.Drawing.Size(120, 23);
            this.btnRibbonUserMgmt.Text = "🗂️ จัดการบัญชีผู้ใช้";
            this.btnRibbonUserMgmt.ToolTipText = "เพิ่ม/แก้ไข/ปิดใช้งานบัญชีผู้ใช้";
            // 
            // ribbonStripAdmin
            // 
            this.ribbonStripAdmin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.ribbonStripAdmin.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ribbonStripAdmin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.ribbonStripAdmin.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ribbonStripAdmin.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ribbonStripAdmin.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRibbonSysConfig,
            this.btnRibbonOrgAdmin,
            this.btnRibbonAuditLog,
            this.btnRibbonReportExcel,
            this.btnRibbonPDPA,
            this.sepRibbon1,
            this.btnRibbonTemplate,
            this.btnRibbonFolderArchive,
            this.btnRibbonCopyStructure,
            this.btnRibbonApproval,
            this.lblApprovalBadge,
            this.btnRibbonChangeLevel,
            this.btnRibbonRecycleBin,
            this.sepRibbon3,
            this.btnRibbonUserAccess,
            this.btnRibbonUserMgmt,
            this.btnRibbonPermission,
            this.btnRibbonFolderSettings});
            this.ribbonStripAdmin.Location = new System.Drawing.Point(0, 53);
            this.ribbonStripAdmin.Name = "ribbonStripAdmin";
            this.ribbonStripAdmin.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.ribbonStripAdmin.Size = new System.Drawing.Size(1469, 26);
            this.ribbonStripAdmin.TabIndex = 2;
            this.ribbonStripAdmin.Visible = false;
            // 
            // btnRibbonSysConfig
            // 
            this.btnRibbonSysConfig.Name = "btnRibbonSysConfig";
            this.btnRibbonSysConfig.Size = new System.Drawing.Size(90, 23);
            this.btnRibbonSysConfig.Text = "⚙️ ตั้งค่าระบบ";
            this.btnRibbonSysConfig.ToolTipText = "ตั้งค่าระบบ (Admin)";
            // 
            // btnRibbonOrgAdmin
            // 
            this.btnRibbonOrgAdmin.Name = "btnRibbonOrgAdmin";
            this.btnRibbonOrgAdmin.Size = new System.Drawing.Size(120, 23);
            this.btnRibbonOrgAdmin.Text = "🏢 โครงสร้างองค์กร";
            this.btnRibbonOrgAdmin.ToolTipText = "จัดการโครงสร้างองค์กร";
            // 
            // btnRibbonReportExcel
            // 
            this.btnRibbonReportExcel.Name = "btnRibbonReportExcel";
            this.btnRibbonReportExcel.Size = new System.Drawing.Size(106, 23);
            this.btnRibbonReportExcel.Text = "📊 รายงาน Excel";
            this.btnRibbonReportExcel.ToolTipText = "ออกรายงาน Excel";
            // 
            // btnRibbonPDPA
            // 
            this.btnRibbonPDPA.Name = "btnRibbonPDPA";
            this.btnRibbonPDPA.Size = new System.Drawing.Size(124, 23);
            this.btnRibbonPDPA.Text = "🔏 PDPA Consent";
            this.btnRibbonPDPA.ToolTipText = "จัดการ PDPA Consent";
            // 
            // btnRibbonFolderArchive
            // 
            this.btnRibbonFolderArchive.Name = "btnRibbonFolderArchive";
            this.btnRibbonFolderArchive.Size = new System.Drawing.Size(81, 23);
            this.btnRibbonFolderArchive.Text = "📦 Archive";
            this.btnRibbonFolderArchive.ToolTipText = "Archive Folder ที่เลือก";
            // 
            // btnRibbonCopyStructure
            // 
            this.btnRibbonCopyStructure.Name = "btnRibbonCopyStructure";
            this.btnRibbonCopyStructure.Size = new System.Drawing.Size(123, 23);
            this.btnRibbonCopyStructure.Text = "📂 คัดลอกโครงสร้าง";
            this.btnRibbonCopyStructure.ToolTipText = "คัดลอกโครงสร้าง Folder ไปยัง Parent ใหม่";
            // 
            // btnRibbonChangeLevel
            // 
            this.btnRibbonChangeLevel.Name = "btnRibbonChangeLevel";
            this.btnRibbonChangeLevel.Size = new System.Drawing.Size(95, 23);
            this.btnRibbonChangeLevel.Text = "🔒 เปลี่ยนระดับ";
            this.btnRibbonChangeLevel.ToolTipText = "เปลี่ยน Confidentiality Level ของ Folder";
            // 
            // btnRibbonRecycleBin
            // 
            this.btnRibbonRecycleBin.Name = "btnRibbonRecycleBin";
            this.btnRibbonRecycleBin.Size = new System.Drawing.Size(69, 23);
            this.btnRibbonRecycleBin.Text = "🗑️ ถังขยะ";
            this.btnRibbonRecycleBin.ToolTipText = "เปิดถังขยะ";
            // 
            // sepRibbon3
            // 
            this.sepRibbon3.Name = "sepRibbon3";
            this.sepRibbon3.Size = new System.Drawing.Size(6, 26);
            // 
            // sepRibbon5
            // 
            this.sepRibbon5.Name = "sepRibbon5";
            this.sepRibbon5.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRibbonViewList
            // 
            this.btnRibbonViewList.Name = "btnRibbonViewList";
            this.btnRibbonViewList.Size = new System.Drawing.Size(58, 22);
            this.btnRibbonViewList.Text = "☰ รายการ";
            this.btnRibbonViewList.ToolTipText = "แสดงแบบรายการ";
            // 
            // btnRibbonViewGrid
            // 
            this.btnRibbonViewGrid.Name = "btnRibbonViewGrid";
            this.btnRibbonViewGrid.Size = new System.Drawing.Size(51, 22);
            this.btnRibbonViewGrid.Text = "⊞ ตาราง";
            this.btnRibbonViewGrid.ToolTipText = "แสดงแบบตาราง";
            // 
            // btnRibbonViewOrg
            // 
            this.btnRibbonViewOrg.Name = "btnRibbonViewOrg";
            this.btnRibbonViewOrg.Size = new System.Drawing.Size(99, 22);
            this.btnRibbonViewOrg.Text = "🏢 โครงสร้างองค์กร";
            this.btnRibbonViewOrg.ToolTipText = "แสดงโครงสร้างองค์กร";
            // 
            // btnRibbonVirusScan
            // 
            this.btnRibbonVirusScan.Name = "btnRibbonVirusScan";
            this.btnRibbonVirusScan.Size = new System.Drawing.Size(78, 22);
            this.btnRibbonVirusScan.Text = "🛡️ Virus Scan";
            this.btnRibbonVirusScan.ToolTipText = "สแกนไวรัสไฟล์ที่เลือก";
            // 
            // btnRibbonChecksum
            // 
            this.btnRibbonChecksum.Name = "btnRibbonChecksum";
            this.btnRibbonChecksum.Size = new System.Drawing.Size(82, 22);
            this.btnRibbonChecksum.Text = "🔢 Checksum";
            this.btnRibbonChecksum.ToolTipText = "ตรวจสอบ Checksum";
            // 
            // btnRibbonArchive
            // 
            this.btnRibbonArchive.Name = "btnRibbonArchive";
            this.btnRibbonArchive.Size = new System.Drawing.Size(66, 19);
            this.btnRibbonArchive.Text = "📦 Archive";
            this.btnRibbonArchive.ToolTipText = "Archive รายการที่เลือก";
            // 
            // sepRibbon4
            // 
            this.sepRibbon4.Name = "sepRibbon4";
            this.sepRibbon4.Size = new System.Drawing.Size(6, 6);
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.statusStrip.ForeColor = System.Drawing.Color.White;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusReady,
            this.lblStatusUser,
            this.lblStatusDept,
            this.lblStatusFolder,
            this.lblStatusPath,
            this.lblStatusDateTime});
            this.statusStrip.Location = new System.Drawing.Point(0, 746);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1469, 22);
            this.statusStrip.TabIndex = 2;
            // 
            // lblStatusReady
            // 
            this.lblStatusReady.ForeColor = System.Drawing.Color.White;
            this.lblStatusReady.Name = "lblStatusReady";
            this.lblStatusReady.Size = new System.Drawing.Size(75, 17);
            this.lblStatusReady.Text = "✅ พร้อมใช้งาน";
            // 
            // lblStatusUser
            // 
            this.lblStatusUser.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblStatusUser.ForeColor = System.Drawing.Color.White;
            this.lblStatusUser.Name = "lblStatusUser";
            this.lblStatusUser.Size = new System.Drawing.Size(4, 17);
            // 
            // lblStatusDept
            // 
            this.lblStatusDept.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblStatusDept.ForeColor = System.Drawing.Color.White;
            this.lblStatusDept.Name = "lblStatusDept";
            this.lblStatusDept.Size = new System.Drawing.Size(4, 17);
            // 
            // lblStatusFolder
            // 
            this.lblStatusFolder.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblStatusFolder.ForeColor = System.Drawing.Color.White;
            this.lblStatusFolder.Name = "lblStatusFolder";
            this.lblStatusFolder.Size = new System.Drawing.Size(1363, 17);
            this.lblStatusFolder.Spring = true;
            // 
            // lblStatusPath
            // 
            this.lblStatusPath.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblStatusPath.ForeColor = System.Drawing.Color.White;
            this.lblStatusPath.Name = "lblStatusPath";
            this.lblStatusPath.Size = new System.Drawing.Size(4, 17);
            // 
            // lblStatusDateTime
            // 
            this.lblStatusDateTime.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblStatusDateTime.ForeColor = System.Drawing.Color.White;
            this.lblStatusDateTime.Name = "lblStatusDateTime";
            this.lblStatusDateTime.Size = new System.Drawing.Size(4, 17);
            // 
            // MDI_Container
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1469, 768);
            this.Controls.Add(this.pnlDashboard);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.ribbonStripAdmin);
            this.Controls.Add(this.ribbonStrip);
            this.Controls.Add(this.menuStrip);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MDI_Container";
            this.Text = "Core Management System (CMS)";
            this.Load += new System.EventHandler(this.MDI_Container_Load);
            this.pnlDashboard.ResumeLayout(false);
            this.pnlDashContent.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlDashLeft.ResumeLayout(false);
            this.pnlDashLeft.PerformLayout();
            this.pnlTreeViewToggle.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ribbonStrip.ResumeLayout(false);
            this.ribbonStrip.PerformLayout();
            this.ribbonStripAdmin.ResumeLayout(false);
            this.ribbonStripAdmin.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // ── Field declarations ────────────────────────────────────────────────
        private System.Windows.Forms.MenuStrip menuStrip;
        // หน้าหลัก
        private System.Windows.Forms.ToolStripMenuItem menuItemHome;
        private System.Windows.Forms.ToolStripMenuItem menuItemUpload;
        private System.Windows.Forms.ToolStripMenuItem menuItemNewFolder;
        private System.Windows.Forms.ToolStripMenuItem menuItemDownload;
        private System.Windows.Forms.ToolStripMenuItem menuItemRename;
        private System.Windows.Forms.ToolStripMenuItem menuItemMove;
        private System.Windows.Forms.ToolStripMenuItem menuItemDelete;
        private System.Windows.Forms.ToolStripMenuItem menuItemTemplate;
        private System.Windows.Forms.ToolStripMenuItem menuItemFavorites;
        private System.Windows.Forms.ToolStripMenuItem menuItemFolderSettings;
        private System.Windows.Forms.ToolStripMenuItem menuItemPermission;
        private System.Windows.Forms.ToolStripMenuItem menuItemAuditLog;
        private System.Windows.Forms.ToolStripMenuItem menuItemApproval;
        private System.Windows.Forms.ToolStripMenuItem menuItemShare;
        private System.Windows.Forms.ToolStripMenuItem menuItemManageShares;
        // แทรก
        private System.Windows.Forms.ToolStripMenuItem menuItemInsert;
        private System.Windows.Forms.ToolStripMenuItem menuItemInsertDoc;
        private System.Windows.Forms.ToolStripMenuItem menuItemInsertImage;
        private System.Windows.Forms.ToolStripMenuItem menuItemInsertZip;
        // มุมมอง
        private System.Windows.Forms.ToolStripMenuItem menuItemView;
        private System.Windows.Forms.ToolStripMenuItem menuItemViewList;
        private System.Windows.Forms.ToolStripMenuItem menuItemViewGrid;
        private System.Windows.Forms.ToolStripMenuItem menuItemViewOrg;
        // เครื่องมือ
        private System.Windows.Forms.ToolStripMenuItem menuItemTools;
        private System.Windows.Forms.ToolStripMenuItem menuItemVirusScan;
        private System.Windows.Forms.ToolStripMenuItem menuItemChecksum;
        private System.Windows.Forms.ToolStripMenuItem menuItemRecycleBin;
        private System.Windows.Forms.ToolStripMenuItem menuItemArchive;
        // ผู้ดูแล
        private System.Windows.Forms.ToolStripMenuItem menuItemAdmin;
        private System.Windows.Forms.ToolStripMenuItem menuItemConfig;
        private System.Windows.Forms.ToolStripMenuItem menuItemOrgAdmin;
        private System.Windows.Forms.ToolStripMenuItem menuItemReportExcel;
        private System.Windows.Forms.ToolStripMenuItem menuItemPDPA;
        // legacy (hidden, kept for role-visibility wiring)
        private System.Windows.Forms.ToolStripMenuItem menuItemMod001;
        private System.Windows.Forms.ToolStripMenuItem menuItemMod002;
        private System.Windows.Forms.ToolStripMenuItem menuItemMod003;
        private System.Windows.Forms.ToolStripMenuItem menuItemMod004;

        // Ribbon
        private System.Windows.Forms.ToolStrip ribbonStrip;
        private System.Windows.Forms.ToolStrip ribbonStripAdmin;
        private System.Windows.Forms.ToolStripButton btnRibbonUpload;
        private System.Windows.Forms.ToolStripButton btnRibbonNewFolder;
        private System.Windows.Forms.ToolStripButton btnRibbonDownload;
        private System.Windows.Forms.ToolStripButton btnRibbonRename;
        private System.Windows.Forms.ToolStripButton btnRibbonMove;
        private System.Windows.Forms.ToolStripButton btnRibbonDelete;
        private System.Windows.Forms.ToolStripButton btnRibbonTemplate;
        private System.Windows.Forms.ToolStripButton btnRibbonFavorites;
        private System.Windows.Forms.ToolStripSeparator sepRibbon1;
        private System.Windows.Forms.ToolStripButton btnRibbonFolderSettings;
        private System.Windows.Forms.ToolStripButton btnRibbonPermission;
        private System.Windows.Forms.ToolStripButton btnRibbonAuditLog;
        private System.Windows.Forms.ToolStripButton btnRibbonApproval;
        private System.Windows.Forms.ToolStripLabel lblApprovalBadge;
        private System.Windows.Forms.ToolStripSeparator sepRibbon2;
        private System.Windows.Forms.ToolStripButton btnRibbonViewList;
        private System.Windows.Forms.ToolStripButton btnRibbonViewGrid;
        private System.Windows.Forms.ToolStripButton btnRibbonViewOrg;
        private System.Windows.Forms.ToolStripSeparator sepRibbon3;
        private System.Windows.Forms.ToolStripButton btnRibbonVirusScan;
        private System.Windows.Forms.ToolStripButton btnRibbonChecksum;
        private System.Windows.Forms.ToolStripButton btnRibbonRecycleBin;
        private System.Windows.Forms.ToolStripButton btnRibbonArchive;
        private System.Windows.Forms.ToolStripSeparator sepRibbon4;
        private System.Windows.Forms.ToolStripButton btnRibbonFolderArchive;
        private System.Windows.Forms.ToolStripButton btnRibbonChangeLevel;
        private System.Windows.Forms.ToolStripButton btnRibbonCopyStructure;
        private System.Windows.Forms.ToolStripSeparator sepRibbon5;
        private System.Windows.Forms.ToolStripButton btnRibbonSysConfig;
        private System.Windows.Forms.ToolStripButton btnRibbonOrgAdmin;
        private System.Windows.Forms.ToolStripButton btnRibbonUserAccess;
        private System.Windows.Forms.ToolStripButton btnRibbonUserMgmt;
        private System.Windows.Forms.ToolStripButton btnRibbonReportExcel;
        private System.Windows.Forms.ToolStripButton btnRibbonPDPA;

        // StatusStrip
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusReady;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusUser;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusDept;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusFolder;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusPath;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusDateTime;

        // Dashboard
        private System.Windows.Forms.Panel pnlDashboard;
        private System.Windows.Forms.Label lblDashTitle;
        private System.Windows.Forms.Panel pnlDashLeft;
        private System.Windows.Forms.Label lblTreeTitle;
        private System.Windows.Forms.TextBox txtSearchFolder;
        private System.Windows.Forms.TreeView treeViewFolders;
        private System.Windows.Forms.Panel pnlTreeViewToggle;
        private System.Windows.Forms.Button btnToggleByOrg;
        private System.Windows.Forms.Button btnToggleByConfidentiality;
        private System.Windows.Forms.Splitter splitterDash;
        private System.Windows.Forms.Panel pnlDashContent;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblQuickAccess;
        private System.Windows.Forms.Button btnDashUpload;
        private System.Windows.Forms.Button btnDashFolder;
        private System.Windows.Forms.Button btnDashApproval;
        private System.Windows.Forms.Button btnDashOrgUnit;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.Label lblStatsContent;
        private System.Windows.Forms.Label lblRecentFiles;
        private System.Windows.Forms.ListView lvRecentFiles;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
    }
}
