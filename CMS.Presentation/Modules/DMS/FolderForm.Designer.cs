namespace CMS.Presentation
{
    partial class FolderForm
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
            this.pnlFilter = new System.Windows.Forms.Panel();
            this.lblFilterStatus = new System.Windows.Forms.Label();
            this.btnClearFilter = new System.Windows.Forms.Button();
            this.btnApplyFilter = new System.Windows.Forms.Button();
            this.txtUploadedBy = new System.Windows.Forms.TextBox();
            this.lblFilterUploader = new System.Windows.Forms.Label();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.lblFilterTo = new System.Windows.Forms.Label();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.lblFilterFrom = new System.Windows.Forms.Label();
            this.cmbExtension = new System.Windows.Forms.ComboBox();
            this.lblFilterExt = new System.Windows.Forms.Label();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnNewFolder = new System.Windows.Forms.ToolStripButton();
            this.btnAuto4Folders = new System.Windows.Forms.ToolStripButton();
            this.btnRename = new System.Windows.Forms.ToolStripButton();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnArchive = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.sep2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnUploadFile = new System.Windows.Forms.ToolStripButton();
            this.btnMove = new System.Windows.Forms.ToolStripButton();
            this.sep4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnApplyTemplate = new System.Windows.Forms.ToolStripButton();
            this.sep5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnHideFile = new System.Windows.Forms.ToolStripButton();
            this.btnUnhideFile = new System.Windows.Forms.ToolStripButton();
            this.sep3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnChangeLevel = new System.Windows.Forms.ToolStripButton();
            this.btnCopyStructure = new System.Windows.Forms.ToolStripButton();
            this.lblConfidentialityBadge = new System.Windows.Forms.ToolStripLabel();
            this.pnlNavBar = new System.Windows.Forms.Panel();
            this.btnNavBack = new System.Windows.Forms.Button();
            this.btnNavForward = new System.Windows.Forms.Button();
            this.btnNavUp = new System.Windows.Forms.Button();
            this.btnNavRefresh = new System.Windows.Forms.Button();
            this.txtBreadcrumb = new System.Windows.Forms.TextBox();
            this.btnViewList = new System.Windows.Forms.Button();
            this.btnViewGrid = new System.Windows.Forms.Button();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.pnlFilterBar = new System.Windows.Forms.Panel();
            this.lblFilterInfo = new System.Windows.Forms.Label();
            this.chk1HC = new System.Windows.Forms.CheckBox();
            this.chk2SC = new System.Windows.Forms.CheckBox();
            this.chk3NC = new System.Windows.Forms.CheckBox();
            this.chk4PC = new System.Windows.Forms.CheckBox();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitLR = new System.Windows.Forms.SplitContainer();
            this.btnToggleSidebar = new System.Windows.Forms.Button();
            this.pnlExplorer = new System.Windows.Forms.Panel();
            this.folderTreeView = new System.Windows.Forms.TreeView();
            this.txtFolderSearch = new System.Windows.Forms.TextBox();
            this.lblOrgUnitStatus = new System.Windows.Forms.Label();
            this.pnlOrgFilter = new System.Windows.Forms.Panel();
            this.treeOrgUnit = new System.Windows.Forms.TreeView();
            this.lblExplorerHeader = new System.Windows.Forms.Label();
            this.pnlRecycleBinNode = new System.Windows.Forms.Panel();
            this.lblRecycleBin = new System.Windows.Forms.Label();
            this.lblRecycleBinCount = new System.Windows.Forms.Label();
            this.splitContent = new System.Windows.Forms.SplitContainer();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.contentsListView = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colConfidentiality = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUploadedBy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colOrgUnit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPermissions = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlContentHeader = new System.Windows.Forms.Panel();
            this.lblContentInfo = new System.Windows.Forms.Label();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.lblDetailContent = new System.Windows.Forms.Label();
            this.lblDetailHeader = new System.Windows.Forms.Label();
            this.recycleBinGroupBox = new System.Windows.Forms.GroupBox();
            this.recycleBinListView = new System.Windows.Forms.ListView();
            this.colRbName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRbConfidentiality = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRbDeletedAt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.recycleBinToolStrip = new System.Windows.Forms.ToolStrip();
            this.btnRbSelectAll = new System.Windows.Forms.ToolStripButton();
            this.lblRbSelCount = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRestore = new System.Windows.Forms.ToolStripButton();
            this.btnRestoreToParent = new System.Windows.Forms.ToolStripButton();
            this.btnPermanentDelete = new System.Windows.Forms.ToolStripButton();
            this.pnlFilter.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.pnlNavBar.SuspendLayout();
            this.pnlFilterBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLR)).BeginInit();
            this.splitLR.Panel1.SuspendLayout();
            this.splitLR.Panel2.SuspendLayout();
            this.splitLR.SuspendLayout();
            this.pnlExplorer.SuspendLayout();
            this.pnlOrgFilter.SuspendLayout();
            this.pnlRecycleBinNode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContent)).BeginInit();
            this.splitContent.Panel1.SuspendLayout();
            this.splitContent.Panel2.SuspendLayout();
            this.splitContent.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlContentHeader.SuspendLayout();
            this.pnlDetail.SuspendLayout();
            this.recycleBinGroupBox.SuspendLayout();
            this.recycleBinToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFilter
            // 
            this.pnlFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.pnlFilter.Controls.Add(this.lblFilterStatus);
            this.pnlFilter.Controls.Add(this.btnClearFilter);
            this.pnlFilter.Controls.Add(this.btnApplyFilter);
            this.pnlFilter.Controls.Add(this.txtUploadedBy);
            this.pnlFilter.Controls.Add(this.lblFilterUploader);
            this.pnlFilter.Controls.Add(this.dtpTo);
            this.pnlFilter.Controls.Add(this.lblFilterTo);
            this.pnlFilter.Controls.Add(this.dtpFrom);
            this.pnlFilter.Controls.Add(this.lblFilterFrom);
            this.pnlFilter.Controls.Add(this.cmbExtension);
            this.pnlFilter.Controls.Add(this.lblFilterExt);
            this.pnlFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilter.Location = new System.Drawing.Point(0, 0);
            this.pnlFilter.Name = "pnlFilter";
            this.pnlFilter.Padding = new System.Windows.Forms.Padding(4);
            this.pnlFilter.Size = new System.Drawing.Size(925, 32);
            this.pnlFilter.TabIndex = 6;
            // 
            // lblFilterStatus
            // 
            this.lblFilterStatus.AutoSize = true;
            this.lblFilterStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblFilterStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblFilterStatus.Location = new System.Drawing.Point(862, 8);
            this.lblFilterStatus.Name = "lblFilterStatus";
            this.lblFilterStatus.Size = new System.Drawing.Size(0, 15);
            this.lblFilterStatus.TabIndex = 10;
            this.lblFilterStatus.Visible = false;
            // 
            // btnClearFilter
            // 
            this.btnClearFilter.FlatAppearance.BorderSize = 0;
            this.btnClearFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnClearFilter.Location = new System.Drawing.Point(784, 4);
            this.btnClearFilter.Name = "btnClearFilter";
            this.btnClearFilter.Size = new System.Drawing.Size(70, 23);
            this.btnClearFilter.TabIndex = 9;
            this.btnClearFilter.Text = "✕ ล้าง";
            this.btnClearFilter.Click += new System.EventHandler(this.btnClearFilter_Click);
            // 
            // btnApplyFilter
            // 
            this.btnApplyFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnApplyFilter.FlatAppearance.BorderSize = 0;
            this.btnApplyFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnApplyFilter.ForeColor = System.Drawing.Color.White;
            this.btnApplyFilter.Location = new System.Drawing.Point(698, 4);
            this.btnApplyFilter.Name = "btnApplyFilter";
            this.btnApplyFilter.Size = new System.Drawing.Size(80, 23);
            this.btnApplyFilter.TabIndex = 8;
            this.btnApplyFilter.Text = "🔍 ค้นหา";
            this.btnApplyFilter.UseVisualStyleBackColor = false;
            this.btnApplyFilter.Click += new System.EventHandler(this.btnApplyFilter_Click);
            // 
            // txtUploadedBy
            // 
            this.txtUploadedBy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtUploadedBy.Location = new System.Drawing.Point(560, 5);
            this.txtUploadedBy.Name = "txtUploadedBy";
            this.txtUploadedBy.Size = new System.Drawing.Size(130, 21);
            this.txtUploadedBy.TabIndex = 7;
            // 
            // lblFilterUploader
            // 
            this.lblFilterUploader.AutoSize = true;
            this.lblFilterUploader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFilterUploader.Location = new System.Drawing.Point(496, 8);
            this.lblFilterUploader.Name = "lblFilterUploader";
            this.lblFilterUploader.Size = new System.Drawing.Size(56, 15);
            this.lblFilterUploader.TabIndex = 6;
            this.lblFilterUploader.Text = "ผู้อัพโหลด:";
            // 
            // dtpTo
            // 
            this.dtpTo.Checked = false;
            this.dtpTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(378, 5);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.ShowCheckBox = true;
            this.dtpTo.Size = new System.Drawing.Size(110, 21);
            this.dtpTo.TabIndex = 5;
            // 
            // lblFilterTo
            // 
            this.lblFilterTo.AutoSize = true;
            this.lblFilterTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFilterTo.Location = new System.Drawing.Point(344, 8);
            this.lblFilterTo.Name = "lblFilterTo";
            this.lblFilterTo.Size = new System.Drawing.Size(22, 15);
            this.lblFilterTo.TabIndex = 4;
            this.lblFilterTo.Text = "ถึง:";
            // 
            // dtpFrom
            // 
            this.dtpFrom.Checked = false;
            this.dtpFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(226, 5);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.ShowCheckBox = true;
            this.dtpFrom.Size = new System.Drawing.Size(110, 21);
            this.dtpFrom.TabIndex = 3;
            // 
            // lblFilterFrom
            // 
            this.lblFilterFrom.AutoSize = true;
            this.lblFilterFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFilterFrom.Location = new System.Drawing.Point(182, 8);
            this.lblFilterFrom.Name = "lblFilterFrom";
            this.lblFilterFrom.Size = new System.Drawing.Size(37, 15);
            this.lblFilterFrom.TabIndex = 2;
            this.lblFilterFrom.Text = "ตั้งแต่:";
            // 
            // cmbExtension
            // 
            this.cmbExtension.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cmbExtension.Location = new System.Drawing.Point(84, 5);
            this.cmbExtension.Name = "cmbExtension";
            this.cmbExtension.Size = new System.Drawing.Size(90, 23);
            this.cmbExtension.TabIndex = 1;
            // 
            // lblFilterExt
            // 
            this.lblFilterExt.AutoSize = true;
            this.lblFilterExt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFilterExt.Location = new System.Drawing.Point(4, 8);
            this.lblFilterExt.Name = "lblFilterExt";
            this.lblFilterExt.Size = new System.Drawing.Size(69, 15);
            this.lblFilterExt.TabIndex = 0;
            this.lblFilterExt.Text = "นามสกุลไฟล์:";
            // 
            // toolStrip
            // 
            this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.toolStrip.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNewFolder,
            this.btnAuto4Folders,
            this.btnRename,
            this.sep1,
            this.btnArchive,
            this.btnDelete,
            this.sep2,
            this.btnUploadFile,
            this.btnMove,
            this.sep4,
            this.btnApplyTemplate,
            this.sep5,
            this.btnHideFile,
            this.btnUnhideFile,
            this.sep3,
            this.btnChangeLevel,
            this.btnCopyStructure,
            this.lblConfidentialityBadge});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.toolStrip.Size = new System.Drawing.Size(1197, 26);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Visible = false;
            // 
            // btnNewFolder
            // 
            this.btnNewFolder.Name = "btnNewFolder";
            this.btnNewFolder.Size = new System.Drawing.Size(99, 23);
            this.btnNewFolder.Text = "📁 Folder ใหม่";
            this.btnNewFolder.ToolTipText = "สร้าง Folder ใหม่";
            this.btnNewFolder.Click += new System.EventHandler(this.btnNewFolder_Click);
            // 
            // btnAuto4Folders
            // 
            this.btnAuto4Folders.Name = "btnAuto4Folders";
            this.btnAuto4Folders.Size = new System.Drawing.Size(126, 23);
            this.btnAuto4Folders.Text = "🎯 Auto 4 Folders";
            this.btnAuto4Folders.ToolTipText = "สร้าง Auto 4 Folders (1HC, 2SC, 3NC, 4PC)";
            this.btnAuto4Folders.Click += new System.EventHandler(this.btnAuto4Folders_Click);
            // 
            // btnRename
            // 
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(86, 23);
            this.btnRename.Text = "✏️ เปลี่ยนชื่อ";
            this.btnRename.ToolTipText = "เปลี่ยนชื่อ Folder ที่เลือก";
            this.btnRename.Visible = false;
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // sep1
            // 
            this.sep1.Name = "sep1";
            this.sep1.Size = new System.Drawing.Size(6, 26);
            // 
            // btnArchive
            // 
            this.btnArchive.Name = "btnArchive";
            this.btnArchive.Size = new System.Drawing.Size(81, 23);
            this.btnArchive.Text = "📦 Archive";
            this.btnArchive.ToolTipText = "Archive Folder และ sub-folders ทั้งหมด";
            this.btnArchive.Visible = false;
            this.btnArchive.Click += new System.EventHandler(this.btnArchive_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(52, 23);
            this.btnDelete.Text = "🗑️ ลบ";
            this.btnDelete.ToolTipText = "ย้าย Folder ไปถังขยะ";
            this.btnDelete.Visible = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // sep2
            // 
            this.sep2.Name = "sep2";
            this.sep2.Size = new System.Drawing.Size(6, 26);
            // 
            // btnUploadFile
            // 
            this.btnUploadFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnUploadFile.ForeColor = System.Drawing.Color.White;
            this.btnUploadFile.Name = "btnUploadFile";
            this.btnUploadFile.Size = new System.Drawing.Size(101, 23);
            this.btnUploadFile.Text = "📤 อัพโหลดไฟล์";
            this.btnUploadFile.ToolTipText = "อัพโหลดไฟล์ไปยัง Folder ที่เลือก";
            this.btnUploadFile.Click += new System.EventHandler(this.btnUploadFile_Click);
            // 
            // btnMove
            // 
            this.btnMove.Enabled = false;
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(56, 23);
            this.btnMove.Text = "📦 ย้าย";
            this.btnMove.ToolTipText = "ย้าย Folder หรือไฟล์ที่เลือกไปยัง Folder ปลายทาง";
            this.btnMove.Click += new System.EventHandler(this.BtnMove_Click);
            // 
            // sep4
            // 
            this.sep4.Name = "sep4";
            this.sep4.Size = new System.Drawing.Size(6, 26);
            // 
            // btnApplyTemplate
            // 
            this.btnApplyTemplate.Name = "btnApplyTemplate";
            this.btnApplyTemplate.Size = new System.Drawing.Size(88, 23);
            this.btnApplyTemplate.Text = "📋 Template";
            this.btnApplyTemplate.ToolTipText = "Apply Template Folder";
            this.btnApplyTemplate.Click += new System.EventHandler(this.btnApplyTemplate_Click);
            // 
            // sep5
            // 
            this.sep5.Name = "sep5";
            this.sep5.Size = new System.Drawing.Size(6, 26);
            // 
            // btnHideFile
            // 
            this.btnHideFile.Name = "btnHideFile";
            this.btnHideFile.Size = new System.Drawing.Size(57, 23);
            this.btnHideFile.Text = "🔒 ซ่อน";
            this.btnHideFile.ToolTipText = "ซ่อนไฟล์ที่เลือก";
            this.btnHideFile.Click += new System.EventHandler(this.btnHideFile_Click);
            // 
            // btnUnhideFile
            // 
            this.btnUnhideFile.Name = "btnUnhideFile";
            this.btnUnhideFile.Size = new System.Drawing.Size(64, 23);
            this.btnUnhideFile.Text = "👁️ แสดง";
            this.btnUnhideFile.ToolTipText = "แสดงไฟล์ที่ซ่อนไว้";
            this.btnUnhideFile.Click += new System.EventHandler(this.btnUnhideFile_Click);
            // 
            // sep3
            // 
            this.sep3.Name = "sep3";
            this.sep3.Size = new System.Drawing.Size(6, 26);
            // 
            // btnChangeLevel
            // 
            this.btnChangeLevel.Name = "btnChangeLevel";
            this.btnChangeLevel.Size = new System.Drawing.Size(95, 23);
            this.btnChangeLevel.Text = "🔒 เปลี่ยนระดับ";
            this.btnChangeLevel.ToolTipText = "เปลี่ยน Confidentiality Level (Manager/Admin)";
            this.btnChangeLevel.Visible = false;
            this.btnChangeLevel.Click += new System.EventHandler(this.btnChangeLevel_Click);
            // 
            // btnCopyStructure
            // 
            this.btnCopyStructure.Name = "btnCopyStructure";
            this.btnCopyStructure.Size = new System.Drawing.Size(123, 23);
            this.btnCopyStructure.Text = "📂 คัดลอกโครงสร้าง";
            this.btnCopyStructure.ToolTipText = "คัดลอกโครงสร้าง Folder ไปยัง Parent ใหม่ (Manager/Admin)";
            this.btnCopyStructure.Visible = false;
            this.btnCopyStructure.Click += new System.EventHandler(this.BtnCopyStructure_Click);
            // 
            // lblConfidentialityBadge
            // 
            this.lblConfidentialityBadge.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblConfidentialityBadge.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblConfidentialityBadge.Name = "lblConfidentialityBadge";
            this.lblConfidentialityBadge.Size = new System.Drawing.Size(0, 26);
            // 
            // pnlNavBar
            // 
            this.pnlNavBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.pnlNavBar.Controls.Add(this.btnNavBack);
            this.pnlNavBar.Controls.Add(this.btnNavForward);
            this.pnlNavBar.Controls.Add(this.btnNavUp);
            this.pnlNavBar.Controls.Add(this.btnNavRefresh);
            this.pnlNavBar.Controls.Add(this.txtBreadcrumb);
            this.pnlNavBar.Controls.Add(this.btnViewList);
            this.pnlNavBar.Controls.Add(this.btnViewGrid);
            this.pnlNavBar.Controls.Add(this.lblSearch);
            this.pnlNavBar.Controls.Add(this.txtSearch);
            this.pnlNavBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlNavBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlNavBar.Location = new System.Drawing.Point(0, 0);
            this.pnlNavBar.Name = "pnlNavBar";
            this.pnlNavBar.Padding = new System.Windows.Forms.Padding(4);
            this.pnlNavBar.Size = new System.Drawing.Size(1197, 44);
            this.pnlNavBar.TabIndex = 5;
            // 
            // btnNavBack
            // 
            this.btnNavBack.FlatAppearance.BorderSize = 0;
            this.btnNavBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavBack.Location = new System.Drawing.Point(4, 4);
            this.btnNavBack.Name = "btnNavBack";
            this.btnNavBack.Size = new System.Drawing.Size(26, 24);
            this.btnNavBack.TabIndex = 0;
            this.btnNavBack.Text = "◀";
            // 
            // btnNavForward
            // 
            this.btnNavForward.FlatAppearance.BorderSize = 0;
            this.btnNavForward.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavForward.Location = new System.Drawing.Point(32, 4);
            this.btnNavForward.Name = "btnNavForward";
            this.btnNavForward.Size = new System.Drawing.Size(26, 24);
            this.btnNavForward.TabIndex = 1;
            this.btnNavForward.Text = "▶";
            // 
            // btnNavUp
            // 
            this.btnNavUp.FlatAppearance.BorderSize = 0;
            this.btnNavUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavUp.Location = new System.Drawing.Point(60, 4);
            this.btnNavUp.Name = "btnNavUp";
            this.btnNavUp.Size = new System.Drawing.Size(26, 24);
            this.btnNavUp.TabIndex = 2;
            this.btnNavUp.Text = "▲";
            // 
            // btnNavRefresh
            // 
            this.btnNavRefresh.FlatAppearance.BorderSize = 0;
            this.btnNavRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavRefresh.Location = new System.Drawing.Point(88, 4);
            this.btnNavRefresh.Name = "btnNavRefresh";
            this.btnNavRefresh.Size = new System.Drawing.Size(26, 24);
            this.btnNavRefresh.TabIndex = 3;
            this.btnNavRefresh.Text = "↺";
            this.btnNavRefresh.Click += new System.EventHandler(this.btnNavRefresh_Click);
            // 
            // txtBreadcrumb
            // 
            this.txtBreadcrumb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBreadcrumb.BackColor = System.Drawing.Color.White;
            this.txtBreadcrumb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBreadcrumb.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.txtBreadcrumb.Location = new System.Drawing.Point(120, 6);
            this.txtBreadcrumb.Multiline = true;
            this.txtBreadcrumb.Name = "txtBreadcrumb";
            this.txtBreadcrumb.ReadOnly = true;
            this.txtBreadcrumb.Size = new System.Drawing.Size(798, 28);
            this.txtBreadcrumb.TabIndex = 4;
            this.txtBreadcrumb.Text = "🏠 ราก";
            // 
            // btnViewList
            // 
            this.btnViewList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnViewList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnViewList.FlatAppearance.BorderSize = 0;
            this.btnViewList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewList.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnViewList.ForeColor = System.Drawing.Color.White;
            this.btnViewList.Location = new System.Drawing.Point(924, 7);
            this.btnViewList.Name = "btnViewList";
            this.btnViewList.Size = new System.Drawing.Size(36, 26);
            this.btnViewList.TabIndex = 5;
            this.btnViewList.Text = "☰";
            this.btnViewList.UseVisualStyleBackColor = false;
            this.btnViewList.Visible = false;
            this.btnViewList.Click += new System.EventHandler(this.btnViewList_Click);
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnViewGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(228)))), ((int)(((byte)(242)))));
            this.btnViewGrid.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnViewGrid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewGrid.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnViewGrid.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnViewGrid.Location = new System.Drawing.Point(964, 7);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(36, 26);
            this.btnViewGrid.TabIndex = 6;
            this.btnViewGrid.Text = "⊞";
            this.btnViewGrid.UseVisualStyleBackColor = false;
            this.btnViewGrid.Visible = false;
            this.btnViewGrid.Click += new System.EventHandler(this.btnViewGrid_Click);
            // 
            // lblSearch
            // 
            this.lblSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblSearch.Location = new System.Drawing.Point(1008, 11);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(22, 20);
            this.lblSearch.TabIndex = 7;
            this.lblSearch.Text = "🔍";
            this.lblSearch.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSearch.Visible = false;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.BackColor = System.Drawing.Color.White;
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.txtSearch.ForeColor = System.Drawing.Color.Gray;
            this.txtSearch.Location = new System.Drawing.Point(1034, 9);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(155, 22);
            this.txtSearch.TabIndex = 8;
            this.txtSearch.Text = "ค้นหาใน Folder...";
            this.txtSearch.Visible = false;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.Enter += new System.EventHandler(this.txtSearch_Enter);
            this.txtSearch.Leave += new System.EventHandler(this.txtSearch_Leave);
            // 
            // pnlFilterBar
            // 
            this.pnlFilterBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(244)))), ((int)(((byte)(252)))));
            this.pnlFilterBar.Controls.Add(this.lblFilterInfo);
            this.pnlFilterBar.Controls.Add(this.chk1HC);
            this.pnlFilterBar.Controls.Add(this.chk2SC);
            this.pnlFilterBar.Controls.Add(this.chk3NC);
            this.pnlFilterBar.Controls.Add(this.chk4PC);
            this.pnlFilterBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilterBar.Location = new System.Drawing.Point(0, 44);
            this.pnlFilterBar.Name = "pnlFilterBar";
            this.pnlFilterBar.Padding = new System.Windows.Forms.Padding(4, 4, 4, 2);
            this.pnlFilterBar.Size = new System.Drawing.Size(1197, 28);
            this.pnlFilterBar.TabIndex = 4;
            this.pnlFilterBar.Visible = false;
            // 
            // lblFilterInfo
            // 
            this.lblFilterInfo.AutoSize = true;
            this.lblFilterInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilterInfo.Location = new System.Drawing.Point(4, 6);
            this.lblFilterInfo.Name = "lblFilterInfo";
            this.lblFilterInfo.Size = new System.Drawing.Size(69, 17);
            this.lblFilterInfo.TabIndex = 0;
            this.lblFilterInfo.Text = "กรองระดับ:";
            this.lblFilterInfo.Visible = false;
            // 
            // chk1HC
            // 
            this.chk1HC.AutoSize = true;
            this.chk1HC.Checked = true;
            this.chk1HC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk1HC.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk1HC.ForeColor = System.Drawing.Color.DarkRed;
            this.chk1HC.Location = new System.Drawing.Point(80, 5);
            this.chk1HC.Name = "chk1HC";
            this.chk1HC.Size = new System.Drawing.Size(68, 21);
            this.chk1HC.TabIndex = 1;
            this.chk1HC.Text = "■ 1HC";
            this.chk1HC.Visible = false;
            // 
            // chk2SC
            // 
            this.chk2SC.AutoSize = true;
            this.chk2SC.Checked = true;
            this.chk2SC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk2SC.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk2SC.ForeColor = System.Drawing.Color.OrangeRed;
            this.chk2SC.Location = new System.Drawing.Point(154, 5);
            this.chk2SC.Name = "chk2SC";
            this.chk2SC.Size = new System.Drawing.Size(67, 21);
            this.chk2SC.TabIndex = 2;
            this.chk2SC.Text = "■ 2SC";
            this.chk2SC.Visible = false;
            // 
            // chk3NC
            // 
            this.chk3NC.AutoSize = true;
            this.chk3NC.Checked = true;
            this.chk3NC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk3NC.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk3NC.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.chk3NC.Location = new System.Drawing.Point(228, 5);
            this.chk3NC.Name = "chk3NC";
            this.chk3NC.Size = new System.Drawing.Size(68, 21);
            this.chk3NC.TabIndex = 3;
            this.chk3NC.Text = "■ 3NC";
            this.chk3NC.Visible = false;
            // 
            // chk4PC
            // 
            this.chk4PC.AutoSize = true;
            this.chk4PC.Checked = true;
            this.chk4PC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk4PC.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk4PC.ForeColor = System.Drawing.Color.DarkGreen;
            this.chk4PC.Location = new System.Drawing.Point(301, 5);
            this.chk4PC.Name = "chk4PC";
            this.chk4PC.Size = new System.Drawing.Size(67, 21);
            this.chk4PC.TabIndex = 4;
            this.chk4PC.Text = "■ 4PC";
            this.chk4PC.Visible = false;
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 72);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.splitLR);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.recycleBinGroupBox);
            this.splitMain.Panel2Collapsed = true;
            this.splitMain.Panel2MinSize = 0;
            this.splitMain.Size = new System.Drawing.Size(1197, 730);
            this.splitMain.SplitterDistance = 533;
            this.splitMain.TabIndex = 3;
            // 
            // splitLR
            // 
            this.splitLR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLR.Location = new System.Drawing.Point(0, 0);
            this.splitLR.Name = "splitLR";
            // 
            // splitLR.Panel1
            // 
            this.splitLR.Panel1.Controls.Add(this.btnToggleSidebar);
            this.splitLR.Panel1.Controls.Add(this.pnlExplorer);
            this.splitLR.Panel1MinSize = 20;
            // 
            // splitLR.Panel2
            // 
            this.splitLR.Panel2.Controls.Add(this.splitContent);
            this.splitLR.Size = new System.Drawing.Size(1197, 730);
            this.splitLR.SplitterDistance = 268;
            this.splitLR.TabIndex = 0;
            // 
            // btnToggleSidebar
            // 
            this.btnToggleSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnToggleSidebar.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnToggleSidebar.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.btnToggleSidebar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleSidebar.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnToggleSidebar.ForeColor = System.Drawing.Color.Gray;
            this.btnToggleSidebar.Location = new System.Drawing.Point(248, 0);
            this.btnToggleSidebar.Name = "btnToggleSidebar";
            this.btnToggleSidebar.Size = new System.Drawing.Size(20, 730);
            this.btnToggleSidebar.TabIndex = 1;
            this.btnToggleSidebar.Text = "◀";
            this.btnToggleSidebar.UseVisualStyleBackColor = false;
            this.btnToggleSidebar.Click += new System.EventHandler(this.btnToggleSidebar_Click);
            // 
            // pnlExplorer
            // 
            this.pnlExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.pnlExplorer.Controls.Add(this.folderTreeView);
            this.pnlExplorer.Controls.Add(this.txtFolderSearch);
            this.pnlExplorer.Controls.Add(this.lblOrgUnitStatus);
            this.pnlExplorer.Controls.Add(this.pnlOrgFilter);
            this.pnlExplorer.Controls.Add(this.lblExplorerHeader);
            this.pnlExplorer.Controls.Add(this.pnlRecycleBinNode);
            this.pnlExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlExplorer.Location = new System.Drawing.Point(0, 0);
            this.pnlExplorer.Name = "pnlExplorer";
            this.pnlExplorer.Size = new System.Drawing.Size(268, 730);
            this.pnlExplorer.TabIndex = 0;
            // 
            // folderTreeView
            // 
            this.folderTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.folderTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.folderTreeView.HotTracking = true;
            this.folderTreeView.Location = new System.Drawing.Point(0, 412);
            this.folderTreeView.Name = "folderTreeView";
            this.folderTreeView.Size = new System.Drawing.Size(268, 290);
            this.folderTreeView.TabIndex = 0;
            this.folderTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.folderTreeView_AfterSelect);
            // 
            // txtFolderSearch
            // 
            this.txtFolderSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtFolderSearch.ForeColor = System.Drawing.Color.Gray;
            this.txtFolderSearch.Location = new System.Drawing.Point(0, 392);
            this.txtFolderSearch.Name = "txtFolderSearch";
            this.txtFolderSearch.Size = new System.Drawing.Size(268, 20);
            this.txtFolderSearch.TabIndex = 1;
            this.txtFolderSearch.Text = "ค้นหา Folder...";
            // 
            // lblOrgUnitStatus
            // 
            this.lblOrgUnitStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(245)))));
            this.lblOrgUnitStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOrgUnitStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblOrgUnitStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblOrgUnitStatus.Location = new System.Drawing.Point(0, 372);
            this.lblOrgUnitStatus.Name = "lblOrgUnitStatus";
            this.lblOrgUnitStatus.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.lblOrgUnitStatus.Size = new System.Drawing.Size(268, 20);
            this.lblOrgUnitStatus.TabIndex = 5;
            this.lblOrgUnitStatus.Text = "📂 กำลังดู: — ทั้งหมด —";
            this.lblOrgUnitStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlOrgFilter
            // 
            this.pnlOrgFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.pnlOrgFilter.Controls.Add(this.treeOrgUnit);
            this.pnlOrgFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOrgFilter.Location = new System.Drawing.Point(0, 26);
            this.pnlOrgFilter.Name = "pnlOrgFilter";
            this.pnlOrgFilter.Padding = new System.Windows.Forms.Padding(4);
            this.pnlOrgFilter.Size = new System.Drawing.Size(268, 346);
            this.pnlOrgFilter.TabIndex = 2;
            // 
            // treeOrgUnit
            // 
            this.treeOrgUnit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeOrgUnit.BackColor = System.Drawing.Color.White;
            this.treeOrgUnit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeOrgUnit.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.treeOrgUnit.HotTracking = true;
            this.treeOrgUnit.Location = new System.Drawing.Point(4, 4);
            this.treeOrgUnit.Name = "treeOrgUnit";
            this.treeOrgUnit.Size = new System.Drawing.Size(295, 338);
            this.treeOrgUnit.TabIndex = 1;
            this.treeOrgUnit.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeOrgUnit_AfterSelect);
            // 
            // lblExplorerHeader
            // 
            this.lblExplorerHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblExplorerHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblExplorerHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblExplorerHeader.ForeColor = System.Drawing.Color.White;
            this.lblExplorerHeader.Location = new System.Drawing.Point(0, 0);
            this.lblExplorerHeader.Name = "lblExplorerHeader";
            this.lblExplorerHeader.Size = new System.Drawing.Size(268, 26);
            this.lblExplorerHeader.TabIndex = 3;
            this.lblExplorerHeader.Text = "  📁 Folder Explorer";
            this.lblExplorerHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlRecycleBinNode
            // 
            this.pnlRecycleBinNode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.pnlRecycleBinNode.Controls.Add(this.lblRecycleBin);
            this.pnlRecycleBinNode.Controls.Add(this.lblRecycleBinCount);
            this.pnlRecycleBinNode.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlRecycleBinNode.Location = new System.Drawing.Point(0, 702);
            this.pnlRecycleBinNode.Name = "pnlRecycleBinNode";
            this.pnlRecycleBinNode.Padding = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.pnlRecycleBinNode.Size = new System.Drawing.Size(268, 28);
            this.pnlRecycleBinNode.TabIndex = 4;
            // 
            // lblRecycleBin
            // 
            this.lblRecycleBin.AutoSize = true;
            this.lblRecycleBin.ForeColor = System.Drawing.Color.Gray;
            this.lblRecycleBin.Location = new System.Drawing.Point(8, 5);
            this.lblRecycleBin.Name = "lblRecycleBin";
            this.lblRecycleBin.Size = new System.Drawing.Size(52, 13);
            this.lblRecycleBin.TabIndex = 0;
            this.lblRecycleBin.Text = "🗑️ ถังขยะ";
            // 
            // lblRecycleBinCount
            // 
            this.lblRecycleBinCount.AutoSize = true;
            this.lblRecycleBinCount.BackColor = System.Drawing.Color.Gray;
            this.lblRecycleBinCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblRecycleBinCount.ForeColor = System.Drawing.Color.White;
            this.lblRecycleBinCount.Location = new System.Drawing.Point(80, 5);
            this.lblRecycleBinCount.Name = "lblRecycleBinCount";
            this.lblRecycleBinCount.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.lblRecycleBinCount.Size = new System.Drawing.Size(20, 15);
            this.lblRecycleBinCount.TabIndex = 1;
            this.lblRecycleBinCount.Text = "0";
            // 
            // splitContent
            // 
            this.splitContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContent.IsSplitterFixed = true;
            this.splitContent.Location = new System.Drawing.Point(0, 0);
            this.splitContent.Name = "splitContent";
            // 
            // splitContent.Panel1
            // 
            this.splitContent.Panel1.Controls.Add(this.pnlContent);
            this.splitContent.Panel1.Controls.Add(this.pnlContentHeader);
            // 
            // splitContent.Panel2
            // 
            this.splitContent.Panel2.Controls.Add(this.pnlDetail);
            this.splitContent.Panel2Collapsed = true;
            this.splitContent.Panel2MinSize = 100;
            this.splitContent.Size = new System.Drawing.Size(925, 730);
            this.splitContent.SplitterDistance = 25;
            this.splitContent.TabIndex = 0;
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.contentsListView);
            this.pnlContent.Controls.Add(this.pnlFilter);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 34);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(925, 696);
            this.pnlContent.TabIndex = 0;
            // 
            // contentsListView
            // 
            this.contentsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colType,
            this.colConfidentiality,
            this.colSize,
            this.colDate,
            this.colUploadedBy,
            this.colVersion,
            this.colOrgUnit,
            this.colPermissions});
            this.contentsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentsListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.contentsListView.FullRowSelect = true;
            this.contentsListView.HideSelection = false;
            this.contentsListView.Location = new System.Drawing.Point(0, 32);
            this.contentsListView.Name = "contentsListView";
            this.contentsListView.Size = new System.Drawing.Size(925, 664);
            this.contentsListView.TabIndex = 1;
            this.contentsListView.UseCompatibleStateImageBehavior = false;
            this.contentsListView.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "ชื่อ ↑";
            this.colName.Width = 320;
            // 
            // colType
            // 
            this.colType.Text = "ประเภท";
            this.colType.Width = 80;
            // 
            // colConfidentiality
            // 
            this.colConfidentiality.Text = "ระดับ";
            this.colConfidentiality.Width = 50;
            // 
            // colSize
            // 
            this.colSize.Text = "ขนาด";
            this.colSize.Width = 70;
            // 
            // colDate
            // 
            this.colDate.Text = "วันที่";
            this.colDate.Width = 120;
            // 
            // colUploadedBy
            // 
            this.colUploadedBy.Text = "โดย";
            this.colUploadedBy.Width = 70;
            // 
            // colVersion
            // 
            this.colVersion.Text = "Ver";
            this.colVersion.Width = 40;
            // 
            // colOrgUnit
            // 
            this.colOrgUnit.Text = "หน่วยงาน";
            this.colOrgUnit.Width = 140;
            // 
            // colPermissions
            // 
            this.colPermissions.Text = "สิทธิ์";
            this.colPermissions.Width = 90;
            // 
            // pnlContentHeader
            // 
            this.pnlContentHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(245)))));
            this.pnlContentHeader.Controls.Add(this.lblContentInfo);
            this.pnlContentHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlContentHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlContentHeader.Name = "pnlContentHeader";
            this.pnlContentHeader.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.pnlContentHeader.Size = new System.Drawing.Size(925, 34);
            this.pnlContentHeader.TabIndex = 2;
            // 
            // lblContentInfo
            // 
            this.lblContentInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblContentInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblContentInfo.Location = new System.Drawing.Point(8, 5);
            this.lblContentInfo.Name = "lblContentInfo";
            this.lblContentInfo.Size = new System.Drawing.Size(909, 24);
            this.lblContentInfo.TabIndex = 0;
            this.lblContentInfo.Text = "— | — | ไม่มีรายการที่เลือก";
            this.lblContentInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlDetail
            // 
            this.pnlDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.pnlDetail.Controls.Add(this.lblDetailContent);
            this.pnlDetail.Controls.Add(this.lblDetailHeader);
            this.pnlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetail.Location = new System.Drawing.Point(0, 0);
            this.pnlDetail.Name = "pnlDetail";
            this.pnlDetail.Padding = new System.Windows.Forms.Padding(8);
            this.pnlDetail.Size = new System.Drawing.Size(96, 100);
            this.pnlDetail.TabIndex = 0;
            // 
            // lblDetailContent
            // 
            this.lblDetailContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDetailContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDetailContent.ForeColor = System.Drawing.Color.Gray;
            this.lblDetailContent.Location = new System.Drawing.Point(8, 34);
            this.lblDetailContent.Name = "lblDetailContent";
            this.lblDetailContent.Size = new System.Drawing.Size(80, 58);
            this.lblDetailContent.TabIndex = 0;
            this.lblDetailContent.Text = "เลือกรายการเพื่อดูรายละเอียด";
            this.lblDetailContent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDetailHeader
            // 
            this.lblDetailHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblDetailHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblDetailHeader.ForeColor = System.Drawing.Color.White;
            this.lblDetailHeader.Location = new System.Drawing.Point(8, 8);
            this.lblDetailHeader.Name = "lblDetailHeader";
            this.lblDetailHeader.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblDetailHeader.Size = new System.Drawing.Size(80, 26);
            this.lblDetailHeader.TabIndex = 1;
            this.lblDetailHeader.Text = "📄 รายละเอียด";
            this.lblDetailHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recycleBinGroupBox
            // 
            this.recycleBinGroupBox.Controls.Add(this.recycleBinListView);
            this.recycleBinGroupBox.Controls.Add(this.recycleBinToolStrip);
            this.recycleBinGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recycleBinGroupBox.Location = new System.Drawing.Point(0, 0);
            this.recycleBinGroupBox.Name = "recycleBinGroupBox";
            this.recycleBinGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.recycleBinGroupBox.Size = new System.Drawing.Size(150, 46);
            this.recycleBinGroupBox.TabIndex = 0;
            this.recycleBinGroupBox.TabStop = false;
            this.recycleBinGroupBox.Text = "🗑️ ถังขยะ";
            this.recycleBinGroupBox.Visible = false;
            // 
            // recycleBinListView
            // 
            this.recycleBinListView.CheckBoxes = true;
            this.recycleBinListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colRbName,
            this.colRbConfidentiality,
            this.colRbDeletedAt});
            this.recycleBinListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recycleBinListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.recycleBinListView.FullRowSelect = true;
            this.recycleBinListView.GridLines = true;
            this.recycleBinListView.HideSelection = false;
            this.recycleBinListView.Location = new System.Drawing.Point(4, 43);
            this.recycleBinListView.Name = "recycleBinListView";
            this.recycleBinListView.Size = new System.Drawing.Size(142, 0);
            this.recycleBinListView.TabIndex = 1;
            this.recycleBinListView.UseCompatibleStateImageBehavior = false;
            this.recycleBinListView.View = System.Windows.Forms.View.Details;
            // 
            // colRbName
            // 
            this.colRbName.Text = "ชื่อ";
            this.colRbName.Width = 280;
            // 
            // colRbConfidentiality
            // 
            this.colRbConfidentiality.Text = "ระดับ";
            this.colRbConfidentiality.Width = 80;
            // 
            // colRbDeletedAt
            // 
            this.colRbDeletedAt.Text = "วันที่ลบ";
            this.colRbDeletedAt.Width = 140;
            // 
            // recycleBinToolStrip
            // 
            this.recycleBinToolStrip.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.recycleBinToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.recycleBinToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.recycleBinToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRbSelectAll,
            this.lblRbSelCount,
            this.toolStripSeparator1,
            this.btnRestore,
            this.btnRestoreToParent,
            this.btnPermanentDelete});
            this.recycleBinToolStrip.Location = new System.Drawing.Point(4, 17);
            this.recycleBinToolStrip.Name = "recycleBinToolStrip";
            this.recycleBinToolStrip.Size = new System.Drawing.Size(142, 26);
            this.recycleBinToolStrip.TabIndex = 2;
            this.recycleBinToolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.recycleBinToolStrip_ItemClicked);
            // 
            // btnRbSelectAll
            // 
            this.btnRbSelectAll.CheckOnClick = true;
            this.btnRbSelectAll.Name = "btnRbSelectAll";
            this.btnRbSelectAll.Size = new System.Drawing.Size(100, 23);
            this.btnRbSelectAll.Text = "☑ เลือกทั้งหมด";
            this.btnRbSelectAll.ToolTipText = "เลือก/ยกเลิกทั้งหมด";
            // 
            // lblRbSelCount
            // 
            this.lblRbSelCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblRbSelCount.Name = "lblRbSelCount";
            this.lblRbSelCount.Size = new System.Drawing.Size(112, 19);
            this.lblRbSelCount.Text = "เลือกแล้ว 0 รายการ";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
            // 
            // btnRestore
            // 
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(59, 23);
            this.btnRestore.Text = "♻️ กู้คืน";
            this.btnRestore.ToolTipText = "กู้คืน Folder ไปยัง Parent เดิม";
            // 
            // btnRestoreToParent
            // 
            this.btnRestoreToParent.Name = "btnRestoreToParent";
            this.btnRestoreToParent.Size = new System.Drawing.Size(93, 23);
            this.btnRestoreToParent.Text = "📂 กู้คืนไปยัง...";
            this.btnRestoreToParent.ToolTipText = "กู้คืน Folder ไปยัง Parent อื่น";
            // 
            // btnPermanentDelete
            // 
            this.btnPermanentDelete.Name = "btnPermanentDelete";
            this.btnPermanentDelete.Size = new System.Drawing.Size(78, 23);
            this.btnPermanentDelete.Text = "❌ ลบถาวร";
            this.btnPermanentDelete.ToolTipText = "ลบถาวร (Manager/Admin)";
            // 
            // FolderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1197, 802);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlFilterBar);
            this.Controls.Add(this.pnlNavBar);
            this.Controls.Add(this.toolStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FolderForm";
            this.Text = "MOD-002: จัดการ Folder";
            this.Load += new System.EventHandler(this.FolderForm_Load);
            this.pnlFilter.ResumeLayout(false);
            this.pnlFilter.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.pnlNavBar.ResumeLayout(false);
            this.pnlNavBar.PerformLayout();
            this.pnlFilterBar.ResumeLayout(false);
            this.pnlFilterBar.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitLR.Panel1.ResumeLayout(false);
            this.splitLR.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLR)).EndInit();
            this.splitLR.ResumeLayout(false);
            this.pnlExplorer.ResumeLayout(false);
            this.pnlExplorer.PerformLayout();
            this.pnlOrgFilter.ResumeLayout(false);
            this.pnlRecycleBinNode.ResumeLayout(false);
            this.pnlRecycleBinNode.PerformLayout();
            this.splitContent.Panel1.ResumeLayout(false);
            this.splitContent.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContent)).EndInit();
            this.splitContent.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlContentHeader.ResumeLayout(false);
            this.pnlDetail.ResumeLayout(false);
            this.recycleBinGroupBox.ResumeLayout(false);
            this.recycleBinGroupBox.PerformLayout();
            this.recycleBinToolStrip.ResumeLayout(false);
            this.recycleBinToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // ── Field declarations ────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnNewFolder;
        private System.Windows.Forms.ToolStripButton btnAuto4Folders;
        private System.Windows.Forms.ToolStripButton btnRename;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripButton btnArchive;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripSeparator sep2;
        private System.Windows.Forms.ToolStripButton btnUploadFile;
        private System.Windows.Forms.ToolStripButton btnMove;
        private System.Windows.Forms.ToolStripSeparator sep4;
        private System.Windows.Forms.ToolStripButton btnApplyTemplate;
        private System.Windows.Forms.ToolStripSeparator sep5;
        private System.Windows.Forms.ToolStripButton btnHideFile;
        private System.Windows.Forms.ToolStripButton btnUnhideFile;
        private System.Windows.Forms.ToolStripSeparator sep3;
        private System.Windows.Forms.ToolStripButton btnChangeLevel;
        private System.Windows.Forms.ToolStripButton btnCopyStructure;
        private System.Windows.Forms.ToolStripLabel lblConfidentialityBadge;

        private System.Windows.Forms.Panel pnlNavBar;
        private System.Windows.Forms.TextBox txtBreadcrumb;
        private System.Windows.Forms.Button btnNavBack;
        private System.Windows.Forms.Button btnNavForward;
        private System.Windows.Forms.Button btnNavUp;
        private System.Windows.Forms.Button btnNavRefresh;
        private System.Windows.Forms.Button btnViewList;
        private System.Windows.Forms.Button btnViewGrid;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;

        private System.Windows.Forms.Panel pnlFilterBar;
        private System.Windows.Forms.Label lblFilterInfo;
        private System.Windows.Forms.CheckBox chk1HC;
        private System.Windows.Forms.CheckBox chk2SC;
        private System.Windows.Forms.CheckBox chk3NC;
        private System.Windows.Forms.CheckBox chk4PC;

        private System.Windows.Forms.SplitContainer splitMain;

        private System.Windows.Forms.Panel pnlExplorer;
        private System.Windows.Forms.Label lblExplorerHeader;
        private System.Windows.Forms.Button btnToggleSidebar;
        private System.Windows.Forms.Panel pnlOrgFilter;
        private System.Windows.Forms.TreeView treeOrgUnit;
        private System.Windows.Forms.Label lblOrgUnitStatus;
        private System.Windows.Forms.TextBox txtFolderSearch;
        private System.Windows.Forms.TreeView folderTreeView;
        private System.Windows.Forms.Panel pnlRecycleBinNode;
        private System.Windows.Forms.Label lblRecycleBin;
        private System.Windows.Forms.Label lblRecycleBinCount;

        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlContentHeader;
        private System.Windows.Forms.Label lblContentInfo;
        private System.Windows.Forms.ListView contentsListView;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colConfidentiality;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colUploadedBy;
        private System.Windows.Forms.ColumnHeader colVersion;
        private System.Windows.Forms.ColumnHeader colOrgUnit;
        private System.Windows.Forms.ColumnHeader colPermissions;

        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Label lblDetailHeader;
        private System.Windows.Forms.Label lblDetailContent;

        private System.Windows.Forms.GroupBox recycleBinGroupBox;
        private System.Windows.Forms.ToolStrip recycleBinToolStrip;
        private System.Windows.Forms.ToolStripButton btnRbSelectAll;
        private System.Windows.Forms.ToolStripLabel lblRbSelCount;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnRestore;
        private System.Windows.Forms.ToolStripButton btnRestoreToParent;
        private System.Windows.Forms.ToolStripButton btnPermanentDelete;
        private System.Windows.Forms.ListView recycleBinListView;
        private System.Windows.Forms.ColumnHeader colRbName;
        private System.Windows.Forms.ColumnHeader colRbConfidentiality;
        private System.Windows.Forms.ColumnHeader colRbDeletedAt;
        private System.Windows.Forms.SplitContainer splitLR;
        private System.Windows.Forms.SplitContainer splitContent;

        // Filter panel controls (Req 6.1)
        private System.Windows.Forms.Panel pnlFilter;
        private System.Windows.Forms.Label lblFilterExt;
        private System.Windows.Forms.ComboBox cmbExtension;
        private System.Windows.Forms.Label lblFilterFrom;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.Label lblFilterTo;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.Label lblFilterUploader;
        private System.Windows.Forms.TextBox txtUploadedBy;
        private System.Windows.Forms.Button btnApplyFilter;
        private System.Windows.Forms.Button btnClearFilter;
        private System.Windows.Forms.Label lblFilterStatus;
    }
}
