using System;

namespace CMS.Presentation.Modules.KMS
{
    partial class KBEditorDialog
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.lblViewCount = new System.Windows.Forms.Label();
            this.lblLikeCount = new System.Windows.Forms.Label();
            this.lblBreadcrumb = new System.Windows.Forms.Label();
            this.flpToolbarLeft = new System.Windows.Forms.FlowLayoutPanel();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnSaveDraft = new System.Windows.Forms.Button();
            this.btnSendReview = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.pnlSep1 = new System.Windows.Forms.Panel();
            this.btnArchive = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlToolbarBorder = new System.Windows.Forms.Panel();
            this.pnlStatusBar = new System.Windows.Forms.Panel();
            this.txtChangeDesc = new System.Windows.Forms.TextBox();
            this.lblChangeDescLabel = new System.Windows.Forms.Label();
            this.lblDateInfo = new System.Windows.Forms.Label();
            this.lblAuthorInfo = new System.Windows.Forms.Label();
            this.lblStatusInfo = new System.Windows.Forms.Label();
            this.lblVerInfo = new System.Windows.Forms.Label();
            this.pnlScroll = new System.Windows.Forms.Panel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitTopBottom = new System.Windows.Forms.SplitContainer();
            this.pnlGenInfoOuter = new System.Windows.Forms.Panel();
            this.tlpGenInfo = new System.Windows.Forms.TableLayoutPanel();
            this.lblExpiry = new System.Windows.Forms.Label();
            this.lblTags = new System.Windows.Forms.Label();
            this.flpTags = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlTagChipTemplate = new System.Windows.Forms.Panel();
            this.lblTagChipTemplate = new System.Windows.Forms.Label();
            this.btnTagChipRemoveTemplate = new System.Windows.Forms.Button();
            this.txtTagInput = new System.Windows.Forms.TextBox();
            this.lblCode = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblVerLabel = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.lblCurVersion = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblCategory = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.lblCraf = new System.Windows.Forms.Label();
            this.cboCraf = new System.Windows.Forms.ComboBox();
            this.dtpExpiry = new System.Windows.Forms.DateTimePicker();
            this.pnlGenInfoHeader = new System.Windows.Forms.Panel();
            this.btnCollapseGenInfo = new System.Windows.Forms.Button();
            this.lblGenInfoHeader = new System.Windows.Forms.Label();
            this.pnlContentOuter = new System.Windows.Forms.Panel();
            this.webEditor = new System.Windows.Forms.WebBrowser();
            this.pnlRteStatus = new System.Windows.Forms.Panel();
            this.lblRteStatus = new System.Windows.Forms.Label();
            this.lblContent = new System.Windows.Forms.Label();
            this.tsRte = new System.Windows.Forms.ToolStrip();
            this.tsRteFontFamily = new System.Windows.Forms.ToolStripComboBox();
            this.tsRteSep0 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteFontSize = new System.Windows.Forms.ToolStripComboBox();
            this.tsRteSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteFontSizeDec = new System.Windows.Forms.ToolStripButton();
            this.tsRteFontSizeInc = new System.Windows.Forms.ToolStripButton();
            this.tsRteBold = new System.Windows.Forms.ToolStripButton();
            this.tsRteItalic = new System.Windows.Forms.ToolStripButton();
            this.tsRteUnder = new System.Windows.Forms.ToolStripButton();
            this.tsRteSep3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteAlignLeft = new System.Windows.Forms.ToolStripButton();
            this.tsRteAlignCenter = new System.Windows.Forms.ToolStripButton();
            this.tsRteAlignRight = new System.Windows.Forms.ToolStripButton();
            this.tsRteSep4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteBullet = new System.Windows.Forms.ToolStripButton();
            this.tsRteNumList = new System.Windows.Forms.ToolStripButton();
            this.tsRteSep5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteLink = new System.Windows.Forms.ToolStripButton();
            this.tsRteTable = new System.Windows.Forms.ToolStripButton();
            this.tsRteCode = new System.Windows.Forms.ToolStripButton();
            this.tsRteImage = new System.Windows.Forms.ToolStripButton();
            this.tsRteAttach = new System.Windows.Forms.ToolStripButton();
            this.pnlSummaryArea = new System.Windows.Forms.Panel();
            this.txtSummary = new System.Windows.Forms.TextBox();
            this.lblSummary = new System.Windows.Forms.Label();
            this.pnlContentHeader = new System.Windows.Forms.Panel();
            this.lblContentHeader = new System.Windows.Forms.Label();
            this.pnlMappingOuter = new System.Windows.Forms.Panel();
            this.tabMapping = new System.Windows.Forms.TabControl();
            this.tabAttach = new System.Windows.Forms.TabPage();
            this.pnlAttachUpload = new System.Windows.Forms.Panel();
            this.btnSelectFromDMS = new System.Windows.Forms.Button();
            this.btnUploadFile = new System.Windows.Forms.Button();
            this.dgvAttachLinked = new System.Windows.Forms.DataGridView();
            this.colNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFilename = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colView = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.lblAttachLinked = new System.Windows.Forms.Label();
            this.tabRelated = new System.Windows.Forms.TabPage();
            this.dgvRelatedSearch = new System.Windows.Forms.DataGridView();
            this.colRelatedSearchCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedSearchTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedSummary = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedSearchType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedSearchViews = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblRelatedSearch = new System.Windows.Forms.Label();
            this.pnlRelatedSearch = new System.Windows.Forms.Panel();
            this.btnSearchRelated = new System.Windows.Forms.Button();
            this.cboRelatedStatus = new System.Windows.Forms.ComboBox();
            this.cboRelatedCategory = new System.Windows.Forms.ComboBox();
            this.cboRelatedType = new System.Windows.Forms.ComboBox();
            this.txtSearchRelated = new System.Windows.Forms.TextBox();
            this.dgvRelatedLinked = new System.Windows.Forms.DataGridView();
            this.colRelatedLinkedNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedLinkedSummary = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedLinkedDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.lblRelatedLinked = new System.Windows.Forms.Label();
            this.tabWorkflow = new System.Windows.Forms.TabPage();
            this.dgvWorkflowSearch = new System.Windows.Forms.DataGridView();
            this.colWorkflowSearchCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWorkflowSearchType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblWorkflowSearch = new System.Windows.Forms.Label();
            this.pnlWorkflowSearch = new System.Windows.Forms.Panel();
            this.btnSearchWorkflow = new System.Windows.Forms.Button();
            this.cboWorkflowStatus = new System.Windows.Forms.ComboBox();
            this.cboWorkflowCategory = new System.Windows.Forms.ComboBox();
            this.cboWorkflowType = new System.Windows.Forms.ComboBox();
            this.txtSearchWorkflow = new System.Windows.Forms.TextBox();
            this.dgvWorkflowLinked = new System.Windows.Forms.DataGridView();
            this.colWorkflowLinkedNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWorkflowLinkedCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWorkflowLinkedName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWorkflowLinkedStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWorkflowLinkedDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.lblWorkflowLinked = new System.Windows.Forms.Label();
            this.tabPrograms = new System.Windows.Forms.TabPage();
            this.dgvProgramsSearch = new System.Windows.Forms.DataGridView();
            this.colSearchCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSearchName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSearchType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSearchStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblProgramsSearch = new System.Windows.Forms.Label();
            this.pnlProgramsSearch = new System.Windows.Forms.Panel();
            this.btnSearchPrograms = new System.Windows.Forms.Button();
            this.cboProgramsStatus = new System.Windows.Forms.ComboBox();
            this.cboProgramsCategory = new System.Windows.Forms.ComboBox();
            this.cboProgramsType = new System.Windows.Forms.ComboBox();
            this.txtSearchPrograms = new System.Windows.Forms.TextBox();
            this.dgvProgramsLinked = new System.Windows.Forms.DataGridView();
            this.colProgramsLinkedNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProgramsLinkedCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProgramsLinkedName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProgramsLinkedType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProgramsLinkedDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.lblProgramsLinked = new System.Windows.Forms.Label();
            this.tabDocs = new System.Windows.Forms.TabPage();
            this.dgvDocsSearch = new System.Windows.Forms.DataGridView();
            this.colDocsSearchCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocsSearchTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocsSearchFolder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocsSearchSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblDocsSearch = new System.Windows.Forms.Label();
            this.pnlDocsSearch = new System.Windows.Forms.Panel();
            this.btnSearchDocs = new System.Windows.Forms.Button();
            this.cboDocsStatus = new System.Windows.Forms.ComboBox();
            this.cboDocsFolder = new System.Windows.Forms.ComboBox();
            this.cboDocsType = new System.Windows.Forms.ComboBox();
            this.txtSearchDocs = new System.Windows.Forms.TextBox();
            this.dgvDocsLinked = new System.Windows.Forms.DataGridView();
            this.colDocsLinkedNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocsLinkedSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocsLinkedDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocsLinkedView = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colDocsLinkedDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.lblDocsLinked = new System.Windows.Forms.Label();
            this.tabDepartments = new System.Windows.Forms.TabPage();
            this.dgvDepartmentsLinked = new System.Windows.Forms.DataGridView();
            this.colDeptNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.lblDepartmentsLinked = new System.Windows.Forms.Label();
            this.pnlMappingHeader = new System.Windows.Forms.Panel();
            this.btnCollapseMappingPanel = new System.Windows.Forms.Button();
            this.lblMappingHeader = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.flpToolbarLeft.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.pnlScroll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitTopBottom)).BeginInit();
            this.splitTopBottom.Panel1.SuspendLayout();
            this.splitTopBottom.Panel2.SuspendLayout();
            this.splitTopBottom.SuspendLayout();
            this.pnlGenInfoOuter.SuspendLayout();
            this.tlpGenInfo.SuspendLayout();
            this.flpTags.SuspendLayout();
            this.pnlTagChipTemplate.SuspendLayout();
            this.pnlGenInfoHeader.SuspendLayout();
            this.pnlContentOuter.SuspendLayout();
            this.pnlRteStatus.SuspendLayout();
            this.tsRte.SuspendLayout();
            this.pnlSummaryArea.SuspendLayout();
            this.pnlContentHeader.SuspendLayout();
            this.pnlMappingOuter.SuspendLayout();
            this.tabMapping.SuspendLayout();
            this.tabAttach.SuspendLayout();
            this.pnlAttachUpload.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttachLinked)).BeginInit();
            this.tabRelated.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelatedSearch)).BeginInit();
            this.pnlRelatedSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelatedLinked)).BeginInit();
            this.tabWorkflow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorkflowSearch)).BeginInit();
            this.pnlWorkflowSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorkflowLinked)).BeginInit();
            this.tabPrograms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProgramsSearch)).BeginInit();
            this.pnlProgramsSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProgramsLinked)).BeginInit();
            this.tabDocs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocsSearch)).BeginInit();
            this.pnlDocsSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocsLinked)).BeginInit();
            this.tabDepartments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDepartmentsLinked)).BeginInit();
            this.pnlMappingHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1733, 48);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblHeader.Size = new System.Drawing.Size(1733, 48);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "✏️ แก้ไขบทความ — Knowledge Base";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
            this.pnlToolbar.Controls.Add(this.lblViewCount);
            this.pnlToolbar.Controls.Add(this.lblLikeCount);
            this.pnlToolbar.Controls.Add(this.lblBreadcrumb);
            this.pnlToolbar.Controls.Add(this.flpToolbarLeft);
            this.pnlToolbar.Controls.Add(this.pnlToolbarBorder);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 48);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1733, 42);
            this.pnlToolbar.TabIndex = 1;
            // 
            // lblViewCount
            // 
            this.lblViewCount.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblViewCount.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblViewCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblViewCount.Location = new System.Drawing.Point(727, 0);
            this.lblViewCount.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblViewCount.Name = "lblViewCount";
            this.lblViewCount.Size = new System.Drawing.Size(74, 40);
            this.lblViewCount.TabIndex = 41;
            this.lblViewCount.Text = "x";
            this.lblViewCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLikeCount
            // 
            this.lblLikeCount.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblLikeCount.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblLikeCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblLikeCount.Location = new System.Drawing.Point(655, 0);
            this.lblLikeCount.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblLikeCount.Name = "lblLikeCount";
            this.lblLikeCount.Size = new System.Drawing.Size(72, 40);
            this.lblLikeCount.TabIndex = 42;
            this.lblLikeCount.Text = "ป";
            this.lblLikeCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBreadcrumb
            // 
            this.lblBreadcrumb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBreadcrumb.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(160)))));
            this.lblBreadcrumb.Location = new System.Drawing.Point(1393, 12);
            this.lblBreadcrumb.Name = "lblBreadcrumb";
            this.lblBreadcrumb.Size = new System.Drawing.Size(320, 18);
            this.lblBreadcrumb.TabIndex = 99;
            this.lblBreadcrumb.Text = "KMS › หมวดหมู่ › แก้ไขบทความ";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpToolbarLeft
            // 
            this.flpToolbarLeft.AutoSize = true;
            this.flpToolbarLeft.BackColor = System.Drawing.Color.Transparent;
            this.flpToolbarLeft.Controls.Add(this.btnBack);
            this.flpToolbarLeft.Controls.Add(this.btnSaveDraft);
            this.flpToolbarLeft.Controls.Add(this.btnSendReview);
            this.flpToolbarLeft.Controls.Add(this.btnPreview);
            this.flpToolbarLeft.Controls.Add(this.pnlSep1);
            this.flpToolbarLeft.Controls.Add(this.btnArchive);
            this.flpToolbarLeft.Controls.Add(this.btnCancel);
            this.flpToolbarLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.flpToolbarLeft.Location = new System.Drawing.Point(0, 0);
            this.flpToolbarLeft.Name = "flpToolbarLeft";
            this.flpToolbarLeft.Padding = new System.Windows.Forms.Padding(8, 7, 0, 0);
            this.flpToolbarLeft.Size = new System.Drawing.Size(655, 40);
            this.flpToolbarLeft.TabIndex = 0;
            this.flpToolbarLeft.WrapContents = false;
            // 
            // btnBack
            // 
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(98)))), ((int)(((byte)(104)))));
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Location = new System.Drawing.Point(8, 7);
            this.btnBack.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(88, 28);
            this.btnBack.TabIndex = 10;
            this.btnBack.Text = "◀ ย้อนกลับ";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            // 
            // btnSaveDraft
            // 
            this.btnSaveDraft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnSaveDraft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveDraft.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(95)))), ((int)(((byte)(154)))));
            this.btnSaveDraft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveDraft.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveDraft.ForeColor = System.Drawing.Color.White;
            this.btnSaveDraft.Location = new System.Drawing.Point(100, 7);
            this.btnSaveDraft.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnSaveDraft.Name = "btnSaveDraft";
            this.btnSaveDraft.Size = new System.Drawing.Size(116, 28);
            this.btnSaveDraft.TabIndex = 0;
            this.btnSaveDraft.Text = "💾 บันทึก Draft";
            this.btnSaveDraft.UseVisualStyleBackColor = false;
            this.btnSaveDraft.Click += new System.EventHandler(this.BtnSaveDraft_Click);
            // 
            // btnSendReview
            // 
            this.btnSendReview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnSendReview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSendReview.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(132)))), ((int)(((byte)(73)))));
            this.btnSendReview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendReview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSendReview.ForeColor = System.Drawing.Color.White;
            this.btnSendReview.Location = new System.Drawing.Point(220, 7);
            this.btnSendReview.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnSendReview.Name = "btnSendReview";
            this.btnSendReview.Size = new System.Drawing.Size(128, 28);
            this.btnSendReview.TabIndex = 1;
            this.btnSendReview.Text = "📤 ส่งขอ Review";
            this.btnSendReview.UseVisualStyleBackColor = false;
            this.btnSendReview.Click += new System.EventHandler(this.BtnSendReview_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(244)))), ((int)(((byte)(253)))));
            this.btnPreview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPreview.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(159)))), ((int)(((byte)(212)))));
            this.btnPreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPreview.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.btnPreview.Location = new System.Drawing.Point(352, 7);
            this.btnPreview.Margin = new System.Windows.Forms.Padding(0);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(88, 28);
            this.btnPreview.TabIndex = 2;
            this.btnPreview.Text = "👁️ Preview";
            this.btnPreview.UseVisualStyleBackColor = false;
            this.btnPreview.Click += new System.EventHandler(this.BtnPreview_Click);
            // 
            // pnlSep1
            // 
            this.pnlSep1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.pnlSep1.Location = new System.Drawing.Point(448, 9);
            this.pnlSep1.Margin = new System.Windows.Forms.Padding(8, 2, 8, 2);
            this.pnlSep1.Name = "pnlSep1";
            this.pnlSep1.Size = new System.Drawing.Size(1, 24);
            this.pnlSep1.TabIndex = 99;
            // 
            // btnArchive
            // 
            this.btnArchive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(90)))), ((int)(((byte)(43)))));
            this.btnArchive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnArchive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(70)))), ((int)(((byte)(30)))));
            this.btnArchive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArchive.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnArchive.ForeColor = System.Drawing.Color.White;
            this.btnArchive.Location = new System.Drawing.Point(457, 7);
            this.btnArchive.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnArchive.Name = "btnArchive";
            this.btnArchive.Size = new System.Drawing.Size(100, 28);
            this.btnArchive.TabIndex = 3;
            this.btnArchive.Text = "📦 Archive";
            this.btnArchive.UseVisualStyleBackColor = false;
            this.btnArchive.Visible = false;
            this.btnArchive.Click += new System.EventHandler(this.BtnArchive_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnCancel.Location = new System.Drawing.Point(564, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 28);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "❌ ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // pnlToolbarBorder
            // 
            this.pnlToolbarBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.pnlToolbarBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlToolbarBorder.Location = new System.Drawing.Point(0, 40);
            this.pnlToolbarBorder.Name = "pnlToolbarBorder";
            this.pnlToolbarBorder.Size = new System.Drawing.Size(1733, 2);
            this.pnlToolbarBorder.TabIndex = 99;
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlStatusBar.Controls.Add(this.txtChangeDesc);
            this.pnlStatusBar.Controls.Add(this.lblChangeDescLabel);
            this.pnlStatusBar.Controls.Add(this.lblDateInfo);
            this.pnlStatusBar.Controls.Add(this.lblAuthorInfo);
            this.pnlStatusBar.Controls.Add(this.lblStatusInfo);
            this.pnlStatusBar.Controls.Add(this.lblVerInfo);
            this.pnlStatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 1033);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.pnlStatusBar.Size = new System.Drawing.Size(1733, 28);
            this.pnlStatusBar.TabIndex = 3;
            // 
            // txtChangeDesc
            // 
            this.txtChangeDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChangeDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(75)))), ((int)(((byte)(118)))));
            this.txtChangeDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtChangeDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.txtChangeDesc.ForeColor = System.Drawing.Color.White;
            this.txtChangeDesc.Location = new System.Drawing.Point(1357, 4);
            this.txtChangeDesc.Name = "txtChangeDesc";
            this.txtChangeDesc.Size = new System.Drawing.Size(366, 26);
            this.txtChangeDesc.TabIndex = 5;
            // 
            // lblChangeDescLabel
            // 
            this.lblChangeDescLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChangeDescLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblChangeDescLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(215)))), ((int)(((byte)(230)))));
            this.lblChangeDescLabel.Location = new System.Drawing.Point(1281, 5);
            this.lblChangeDescLabel.Name = "lblChangeDescLabel";
            this.lblChangeDescLabel.Size = new System.Drawing.Size(70, 18);
            this.lblChangeDescLabel.TabIndex = 4;
            this.lblChangeDescLabel.Text = "หมายเหตุ:";
            this.lblChangeDescLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDateInfo
            // 
            this.lblDateInfo.AutoSize = true;
            this.lblDateInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDateInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(220)))));
            this.lblDateInfo.Location = new System.Drawing.Point(420, 6);
            this.lblDateInfo.Name = "lblDateInfo";
            this.lblDateInfo.Size = new System.Drawing.Size(56, 20);
            this.lblDateInfo.TabIndex = 3;
            this.lblDateInfo.Text = "วันที่: —";
            // 
            // lblAuthorInfo
            // 
            this.lblAuthorInfo.AutoSize = true;
            this.lblAuthorInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblAuthorInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(220)))));
            this.lblAuthorInfo.Location = new System.Drawing.Point(240, 6);
            this.lblAuthorInfo.Name = "lblAuthorInfo";
            this.lblAuthorInfo.Size = new System.Drawing.Size(79, 20);
            this.lblAuthorInfo.TabIndex = 2;
            this.lblAuthorInfo.Text = "สร้างโดย: —";
            // 
            // lblStatusInfo
            // 
            this.lblStatusInfo.AutoSize = true;
            this.lblStatusInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.lblStatusInfo.Location = new System.Drawing.Point(120, 6);
            this.lblStatusInfo.Name = "lblStatusInfo";
            this.lblStatusInfo.Size = new System.Drawing.Size(68, 20);
            this.lblStatusInfo.TabIndex = 1;
            this.lblStatusInfo.Text = "📝 Draft";
            // 
            // lblVerInfo
            // 
            this.lblVerInfo.AutoSize = true;
            this.lblVerInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblVerInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.lblVerInfo.Location = new System.Drawing.Point(10, 6);
            this.lblVerInfo.Name = "lblVerInfo";
            this.lblVerInfo.Size = new System.Drawing.Size(79, 20);
            this.lblVerInfo.TabIndex = 0;
            this.lblVerInfo.Text = "Version: —";
            // 
            // pnlScroll
            // 
            this.pnlScroll.AutoScroll = true;
            this.pnlScroll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlScroll.Controls.Add(this.splitMain);
            this.pnlScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlScroll.Location = new System.Drawing.Point(0, 90);
            this.pnlScroll.Name = "pnlScroll";
            this.pnlScroll.Padding = new System.Windows.Forms.Padding(10);
            this.pnlScroll.Size = new System.Drawing.Size(1733, 943);
            this.pnlScroll.TabIndex = 2;
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(10, 10);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.splitTopBottom);
            this.splitMain.Panel1MinSize = 600;
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.pnlMappingOuter);
            this.splitMain.Panel2MinSize = 400;
            this.splitMain.Size = new System.Drawing.Size(1713, 923);
            this.splitMain.SplitterDistance = 1178;
            this.splitMain.SplitterWidth = 6;
            this.splitMain.TabIndex = 0;
            // 
            // splitTopBottom
            // 
            this.splitTopBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitTopBottom.Location = new System.Drawing.Point(0, 0);
            this.splitTopBottom.Name = "splitTopBottom";
            this.splitTopBottom.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitTopBottom.Panel1
            // 
            this.splitTopBottom.Panel1.Controls.Add(this.pnlGenInfoOuter);
            this.splitTopBottom.Panel1MinSize = 220;
            // 
            // splitTopBottom.Panel2
            // 
            this.splitTopBottom.Panel2.Controls.Add(this.pnlContentOuter);
            this.splitTopBottom.Panel2MinSize = 250;
            this.splitTopBottom.Size = new System.Drawing.Size(1178, 923);
            this.splitTopBottom.SplitterDistance = 220;
            this.splitTopBottom.SplitterWidth = 6;
            this.splitTopBottom.TabIndex = 0;
            // 
            // pnlGenInfoOuter
            // 
            this.pnlGenInfoOuter.BackColor = System.Drawing.Color.White;
            this.pnlGenInfoOuter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGenInfoOuter.Controls.Add(this.tlpGenInfo);
            this.pnlGenInfoOuter.Controls.Add(this.pnlGenInfoHeader);
            this.pnlGenInfoOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGenInfoOuter.Location = new System.Drawing.Point(0, 0);
            this.pnlGenInfoOuter.Name = "pnlGenInfoOuter";
            this.pnlGenInfoOuter.Size = new System.Drawing.Size(1178, 220);
            this.pnlGenInfoOuter.TabIndex = 10;
            // 
            // tlpGenInfo
            // 
            this.tlpGenInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(251)))), ((int)(((byte)(252)))));
            this.tlpGenInfo.ColumnCount = 4;
            this.tlpGenInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.60281F));
            this.tlpGenInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.83253F));
            this.tlpGenInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.53178F));
            this.tlpGenInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.03288F));
            this.tlpGenInfo.Controls.Add(this.lblExpiry, 2, 5);
            this.tlpGenInfo.Controls.Add(this.lblTags, 0, 5);
            this.tlpGenInfo.Controls.Add(this.flpTags, 0, 6);
            this.tlpGenInfo.Controls.Add(this.lblCode, 0, 0);
            this.tlpGenInfo.Controls.Add(this.lblType, 1, 0);
            this.tlpGenInfo.Controls.Add(this.lblStatus, 2, 0);
            this.tlpGenInfo.Controls.Add(this.lblVerLabel, 3, 0);
            this.tlpGenInfo.Controls.Add(this.txtCode, 0, 1);
            this.tlpGenInfo.Controls.Add(this.cboType, 1, 1);
            this.tlpGenInfo.Controls.Add(this.cboStatus, 2, 1);
            this.tlpGenInfo.Controls.Add(this.lblCurVersion, 3, 1);
            this.tlpGenInfo.Controls.Add(this.lblTitle, 0, 3);
            this.tlpGenInfo.Controls.Add(this.lblCategory, 2, 3);
            this.tlpGenInfo.Controls.Add(this.txtTitle, 0, 4);
            this.tlpGenInfo.Controls.Add(this.cboCategory, 2, 4);
            this.tlpGenInfo.Controls.Add(this.lblCraf, 3, 3);
            this.tlpGenInfo.Controls.Add(this.cboCraf, 3, 4);
            this.tlpGenInfo.Controls.Add(this.dtpExpiry, 2, 6);
            this.tlpGenInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGenInfo.Location = new System.Drawing.Point(0, 28);
            this.tlpGenInfo.Name = "tlpGenInfo";
            this.tlpGenInfo.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.tlpGenInfo.RowCount = 7;
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 11F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpGenInfo.Size = new System.Drawing.Size(1176, 190);
            this.tlpGenInfo.TabIndex = 1;
            // 
            // lblExpiry
            // 
            this.lblExpiry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExpiry.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblExpiry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblExpiry.Location = new System.Drawing.Point(559, 120);
            this.lblExpiry.Margin = new System.Windows.Forms.Padding(2, 10, 2, 0);
            this.lblExpiry.Name = "lblExpiry";
            this.lblExpiry.Size = new System.Drawing.Size(314, 23);
            this.lblExpiry.TabIndex = 32;
            this.lblExpiry.Text = "วันหมดอายุ (Review Due)";
            // 
            // lblTags
            // 
            this.lblTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTags.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTags.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblTags.Location = new System.Drawing.Point(12, 120);
            this.lblTags.Margin = new System.Windows.Forms.Padding(2, 10, 2, 5);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(291, 18);
            this.lblTags.TabIndex = 30;
            this.lblTags.Text = "แท็ก (Tags) — คั่นด้วยเครื่องหมาย ,";
            // 
            // flpTags
            // 
            this.flpTags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tlpGenInfo.SetColumnSpan(this.flpTags, 2);
            this.flpTags.Controls.Add(this.pnlTagChipTemplate);
            this.flpTags.Controls.Add(this.txtTagInput);
            this.flpTags.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpTags.Location = new System.Drawing.Point(12, 143);
            this.flpTags.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.flpTags.Name = "flpTags";
            this.flpTags.Size = new System.Drawing.Size(543, 34);
            this.flpTags.TabIndex = 35;
            // 
            // pnlTagChipTemplate
            // 
            this.pnlTagChipTemplate.AutoSize = true;
            this.pnlTagChipTemplate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(234)))), ((int)(((byte)(248)))));
            this.pnlTagChipTemplate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTagChipTemplate.Controls.Add(this.lblTagChipTemplate);
            this.pnlTagChipTemplate.Controls.Add(this.btnTagChipRemoveTemplate);
            this.pnlTagChipTemplate.Location = new System.Drawing.Point(2, 2);
            this.pnlTagChipTemplate.Margin = new System.Windows.Forms.Padding(2);
            this.pnlTagChipTemplate.Name = "pnlTagChipTemplate";
            this.pnlTagChipTemplate.Padding = new System.Windows.Forms.Padding(6, 2, 2, 2);
            this.pnlTagChipTemplate.Size = new System.Drawing.Size(65, 28);
            this.pnlTagChipTemplate.TabIndex = 99;
            this.pnlTagChipTemplate.Visible = false;
            // 
            // lblTagChipTemplate
            // 
            this.lblTagChipTemplate.AutoSize = true;
            this.lblTagChipTemplate.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTagChipTemplate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblTagChipTemplate.Location = new System.Drawing.Point(6, 2);
            this.lblTagChipTemplate.Name = "lblTagChipTemplate";
            this.lblTagChipTemplate.Padding = new System.Windows.Forms.Padding(0, 2, 4, 0);
            this.lblTagChipTemplate.Size = new System.Drawing.Size(4, 22);
            this.lblTagChipTemplate.TabIndex = 0;
            // 
            // btnTagChipRemoveTemplate
            // 
            this.btnTagChipRemoveTemplate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTagChipRemoveTemplate.FlatAppearance.BorderSize = 0;
            this.btnTagChipRemoveTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTagChipRemoveTemplate.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnTagChipRemoveTemplate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.btnTagChipRemoveTemplate.Location = new System.Drawing.Point(45, 2);
            this.btnTagChipRemoveTemplate.Margin = new System.Windows.Forms.Padding(0);
            this.btnTagChipRemoveTemplate.Name = "btnTagChipRemoveTemplate";
            this.btnTagChipRemoveTemplate.Size = new System.Drawing.Size(16, 16);
            this.btnTagChipRemoveTemplate.TabIndex = 1;
            this.btnTagChipRemoveTemplate.Text = "×";
            this.btnTagChipRemoveTemplate.UseVisualStyleBackColor = true;
            // 
            // txtTagInput
            // 
            this.txtTagInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTagInput.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTagInput.Location = new System.Drawing.Point(72, 3);
            this.txtTagInput.Name = "txtTagInput";
            this.txtTagInput.Size = new System.Drawing.Size(104, 27);
            this.txtTagInput.TabIndex = 0;
            this.txtTagInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtTagInput_KeyDown);
            // 
            // lblCode
            // 
            this.lblCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCode.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblCode.Location = new System.Drawing.Point(12, 8);
            this.lblCode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(291, 18);
            this.lblCode.TabIndex = 0;
            this.lblCode.Text = "รหัสบทความ";
            // 
            // lblType
            // 
            this.lblType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblType.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblType.Location = new System.Drawing.Point(307, 8);
            this.lblType.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(248, 18);
            this.lblType.TabIndex = 1;
            this.lblType.Text = "ประเภทบทความ *";
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblStatus.Location = new System.Drawing.Point(559, 8);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(314, 18);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "สถานะ";
            // 
            // lblVerLabel
            // 
            this.lblVerLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVerLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblVerLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblVerLabel.Location = new System.Drawing.Point(877, 8);
            this.lblVerLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblVerLabel.Name = "lblVerLabel";
            this.lblVerLabel.Size = new System.Drawing.Size(287, 18);
            this.lblVerLabel.TabIndex = 3;
            this.lblVerLabel.Text = "เวอร์ชัน";
            // 
            // txtCode
            // 
            this.txtCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCode.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.txtCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(104)))));
            this.txtCode.Location = new System.Drawing.Point(12, 26);
            this.txtCode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.txtCode.Name = "txtCode";
            this.txtCode.ReadOnly = true;
            this.txtCode.Size = new System.Drawing.Size(291, 26);
            this.txtCode.TabIndex = 10;
            // 
            // cboType
            // 
            this.cboType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cboType.Location = new System.Drawing.Point(307, 26);
            this.cboType.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(248, 27);
            this.cboType.TabIndex = 11;
            // 
            // cboStatus
            // 
            this.cboStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatus.Enabled = false;
            this.cboStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboStatus.FormattingEnabled = true;
            this.cboStatus.Items.AddRange(new object[] {
            "📝 Draft",
            "🔍 รอ Review",
            "✅ Published",
            "📦 Archived"});
            this.cboStatus.Location = new System.Drawing.Point(559, 26);
            this.cboStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(314, 28);
            this.cboStatus.TabIndex = 12;
            this.cboStatus.SelectedIndexChanged += new System.EventHandler(this.CboStatus_SelectedIndexChanged);
            // 
            // lblCurVersion
            // 
            this.lblCurVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.lblCurVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCurVersion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCurVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(104)))));
            this.lblCurVersion.Location = new System.Drawing.Point(877, 26);
            this.lblCurVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCurVersion.Name = "lblCurVersion";
            this.lblCurVersion.Size = new System.Drawing.Size(287, 28);
            this.lblCurVersion.TabIndex = 13;
            this.lblCurVersion.Text = "v1.0";
            this.lblCurVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblTitle.Location = new System.Drawing.Point(12, 63);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(291, 19);
            this.lblTitle.TabIndex = 20;
            this.lblTitle.Text = "ชื่อบทความ *";
            // 
            // lblCategory
            // 
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblCategory.Location = new System.Drawing.Point(559, 63);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(84, 18);
            this.lblCategory.TabIndex = 21;
            this.lblCategory.Text = "หมวดหมู่ *";
            // 
            // txtTitle
            // 
            this.tlpGenInfo.SetColumnSpan(this.txtTitle, 2);
            this.txtTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.txtTitle.Location = new System.Drawing.Point(12, 82);
            this.txtTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.txtTitle.MaxLength = 500;
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(543, 30);
            this.txtTitle.TabIndex = 22;
            // 
            // cboCategory
            // 
            this.cboCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cboCategory.Location = new System.Drawing.Point(559, 82);
            this.cboCategory.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(314, 27);
            this.cboCategory.TabIndex = 23;
            // 
            // lblCraf
            // 
            this.lblCraf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCraf.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCraf.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblCraf.Location = new System.Drawing.Point(877, 63);
            this.lblCraf.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCraf.Name = "lblCraf";
            this.lblCraf.Size = new System.Drawing.Size(287, 19);
            this.lblCraf.TabIndex = 31;
            this.lblCraf.Text = "ระดับความลับ (CRAF) *";
            // 
            // cboCraf
            // 
            this.cboCraf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboCraf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCraf.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cboCraf.Location = new System.Drawing.Point(877, 82);
            this.cboCraf.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.cboCraf.Name = "cboCraf";
            this.cboCraf.Size = new System.Drawing.Size(287, 27);
            this.cboCraf.TabIndex = 33;
            // 
            // dtpExpiry
            // 
            this.dtpExpiry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpExpiry.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.dtpExpiry.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpExpiry.Location = new System.Drawing.Point(560, 146);
            this.dtpExpiry.Name = "dtpExpiry";
            this.dtpExpiry.Size = new System.Drawing.Size(312, 26);
            this.dtpExpiry.TabIndex = 34;
            // 
            // pnlGenInfoHeader
            // 
            this.pnlGenInfoHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlGenInfoHeader.Controls.Add(this.btnCollapseGenInfo);
            this.pnlGenInfoHeader.Controls.Add(this.lblGenInfoHeader);
            this.pnlGenInfoHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGenInfoHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlGenInfoHeader.Name = "pnlGenInfoHeader";
            this.pnlGenInfoHeader.Size = new System.Drawing.Size(1176, 28);
            this.pnlGenInfoHeader.TabIndex = 0;
            // 
            // btnCollapseGenInfo
            // 
            this.btnCollapseGenInfo.BackColor = System.Drawing.Color.Transparent;
            this.btnCollapseGenInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCollapseGenInfo.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCollapseGenInfo.FlatAppearance.BorderSize = 0;
            this.btnCollapseGenInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCollapseGenInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCollapseGenInfo.ForeColor = System.Drawing.Color.White;
            this.btnCollapseGenInfo.Location = new System.Drawing.Point(1140, 0);
            this.btnCollapseGenInfo.Name = "btnCollapseGenInfo";
            this.btnCollapseGenInfo.Size = new System.Drawing.Size(36, 28);
            this.btnCollapseGenInfo.TabIndex = 1;
            this.btnCollapseGenInfo.Text = "▲";
            this.btnCollapseGenInfo.UseVisualStyleBackColor = false;
            this.btnCollapseGenInfo.Click += new System.EventHandler(this.btnCollapseGenInfo_Click);
            // 
            // lblGenInfoHeader
            // 
            this.lblGenInfoHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGenInfoHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblGenInfoHeader.ForeColor = System.Drawing.Color.White;
            this.lblGenInfoHeader.Location = new System.Drawing.Point(0, 0);
            this.lblGenInfoHeader.Name = "lblGenInfoHeader";
            this.lblGenInfoHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblGenInfoHeader.Size = new System.Drawing.Size(1176, 28);
            this.lblGenInfoHeader.TabIndex = 0;
            this.lblGenInfoHeader.Text = "📋  ข้อมูลทั่วไป (General Information)";
            this.lblGenInfoHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlContentOuter
            // 
            this.pnlContentOuter.BackColor = System.Drawing.Color.White;
            this.pnlContentOuter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlContentOuter.Controls.Add(this.webEditor);
            this.pnlContentOuter.Controls.Add(this.pnlRteStatus);
            this.pnlContentOuter.Controls.Add(this.lblContent);
            this.pnlContentOuter.Controls.Add(this.tsRte);
            this.pnlContentOuter.Controls.Add(this.pnlSummaryArea);
            this.pnlContentOuter.Controls.Add(this.pnlContentHeader);
            this.pnlContentOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContentOuter.Location = new System.Drawing.Point(0, 0);
            this.pnlContentOuter.Name = "pnlContentOuter";
            this.pnlContentOuter.Size = new System.Drawing.Size(1178, 697);
            this.pnlContentOuter.TabIndex = 12;
            // 
            // webEditor
            // 
            this.webEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webEditor.IsWebBrowserContextMenuEnabled = false;
            this.webEditor.Location = new System.Drawing.Point(0, 115);
            this.webEditor.MinimumSize = new System.Drawing.Size(20, 20);
            this.webEditor.Name = "webEditor";
            this.webEditor.ScriptErrorsSuppressed = true;
            this.webEditor.Size = new System.Drawing.Size(1176, 558);
            this.webEditor.TabIndex = 5;
            this.webEditor.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebEditor_DocumentCompleted);
            // 
            // pnlRteStatus
            // 
            this.pnlRteStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlRteStatus.Controls.Add(this.lblRteStatus);
            this.pnlRteStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlRteStatus.Location = new System.Drawing.Point(0, 673);
            this.pnlRteStatus.Name = "pnlRteStatus";
            this.pnlRteStatus.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.pnlRteStatus.Size = new System.Drawing.Size(1176, 22);
            this.pnlRteStatus.TabIndex = 4;
            // 
            // lblRteStatus
            // 
            this.lblRteStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRteStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblRteStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblRteStatus.Location = new System.Drawing.Point(10, 0);
            this.lblRteStatus.Name = "lblRteStatus";
            this.lblRteStatus.Size = new System.Drawing.Size(1156, 22);
            this.lblRteStatus.TabIndex = 0;
            this.lblRteStatus.Text = "🖊️ พร้อมแก้ไข  |  จำนวนอักขระ: 0";
            this.lblRteStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblContent
            // 
            this.lblContent.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblContent.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblContent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblContent.Location = new System.Drawing.Point(0, 93);
            this.lblContent.Name = "lblContent";
            this.lblContent.Padding = new System.Windows.Forms.Padding(10, 4, 10, 0);
            this.lblContent.Size = new System.Drawing.Size(1176, 22);
            this.lblContent.TabIndex = 3;
            this.lblContent.Text = "เนื้อหาบทความ (HTML Editor — WYSIWYG)";
            // 
            // tsRte
            // 
            this.tsRte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.tsRte.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsRte.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.tsRte.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsRteFontFamily,
            this.tsRteSep0,
            this.tsRteFontSize,
            this.tsRteSep1,
            this.tsRteFontSizeDec,
            this.tsRteFontSizeInc,
            this.tsRteBold,
            this.tsRteItalic,
            this.tsRteUnder,
            this.tsRteSep3,
            this.tsRteAlignLeft,
            this.tsRteAlignCenter,
            this.tsRteAlignRight,
            this.tsRteSep4,
            this.tsRteBullet,
            this.tsRteNumList,
            this.tsRteSep5,
            this.tsRteLink,
            this.tsRteTable,
            this.tsRteCode,
            this.tsRteImage,
            this.tsRteAttach});
            this.tsRte.Location = new System.Drawing.Point(0, 65);
            this.tsRte.Name = "tsRte";
            this.tsRte.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tsRte.Size = new System.Drawing.Size(1176, 28);
            this.tsRte.TabIndex = 2;
            // 
            // tsRteFontFamily
            // 
            this.tsRteFontFamily.AutoSize = false;
            this.tsRteFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tsRteFontFamily.Items.AddRange(new object[] {
            "Segoe UI",
            "Arial",
            "Tahoma",
            "Times New Roman",
            "Calibri",
            "Consolas",
            "Courier New",
            "Georgia"});
            this.tsRteFontFamily.Name = "tsRteFontFamily";
            this.tsRteFontFamily.Size = new System.Drawing.Size(130, 28);
            this.tsRteFontFamily.ToolTipText = "Font Family";
            this.tsRteFontFamily.SelectedIndexChanged += new System.EventHandler(this.TsRteFontFamily_SelectedIndexChanged);
            // 
            // tsRteSep0
            // 
            this.tsRteSep0.Name = "tsRteSep0";
            this.tsRteSep0.Size = new System.Drawing.Size(6, 28);
            // 
            // tsRteFontSize
            // 
            this.tsRteFontSize.AutoSize = false;
            this.tsRteFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tsRteFontSize.Items.AddRange(new object[] {
            "8",
            "9",
            "10",
            "11",
            "12",
            "14",
            "16",
            "18",
            "20",
            "22",
            "24",
            "26",
            "28",
            "36",
            "48",
            "72"});
            this.tsRteFontSize.Name = "tsRteFontSize";
            this.tsRteFontSize.Size = new System.Drawing.Size(70, 28);
            this.tsRteFontSize.ToolTipText = "Font Size";
            this.tsRteFontSize.SelectedIndexChanged += new System.EventHandler(this.TsRteFontSize_SelectedIndexChanged);
            // 
            // tsRteSep1
            // 
            this.tsRteSep1.Name = "tsRteSep1";
            this.tsRteSep1.Size = new System.Drawing.Size(6, 28);
            // 
            // tsRteFontSizeDec
            // 
            this.tsRteFontSizeDec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteFontSizeDec.Name = "tsRteFontSizeDec";
            this.tsRteFontSizeDec.Size = new System.Drawing.Size(29, 25);
            this.tsRteFontSizeDec.Text = "−";
            this.tsRteFontSizeDec.ToolTipText = "Decrease Font Size";
            this.tsRteFontSizeDec.Click += new System.EventHandler(this.TsRteFontSizeDec_Click);
            // 
            // tsRteFontSizeInc
            // 
            this.tsRteFontSizeInc.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteFontSizeInc.Name = "tsRteFontSizeInc";
            this.tsRteFontSizeInc.Size = new System.Drawing.Size(29, 25);
            this.tsRteFontSizeInc.Text = "+";
            this.tsRteFontSizeInc.ToolTipText = "Increase Font Size";
            this.tsRteFontSizeInc.Click += new System.EventHandler(this.TsRteFontSizeInc_Click);
            // 
            // tsRteBold
            // 
            this.tsRteBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteBold.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsRteBold.Name = "tsRteBold";
            this.tsRteBold.Size = new System.Drawing.Size(29, 25);
            this.tsRteBold.Text = "B";
            this.tsRteBold.ToolTipText = "Bold (Ctrl+B)";
            this.tsRteBold.Click += new System.EventHandler(this.TsRteBold_Click);
            // 
            // tsRteItalic
            // 
            this.tsRteItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteItalic.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.tsRteItalic.Name = "tsRteItalic";
            this.tsRteItalic.Size = new System.Drawing.Size(29, 25);
            this.tsRteItalic.Text = "I";
            this.tsRteItalic.ToolTipText = "Italic (Ctrl+I)";
            this.tsRteItalic.Click += new System.EventHandler(this.TsRteItalic_Click);
            // 
            // tsRteUnder
            // 
            this.tsRteUnder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteUnder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
            this.tsRteUnder.Name = "tsRteUnder";
            this.tsRteUnder.Size = new System.Drawing.Size(29, 25);
            this.tsRteUnder.Text = "U";
            this.tsRteUnder.ToolTipText = "Underline (Ctrl+U)";
            this.tsRteUnder.Click += new System.EventHandler(this.TsRteUnder_Click);
            // 
            // tsRteSep3
            // 
            this.tsRteSep3.Name = "tsRteSep3";
            this.tsRteSep3.Size = new System.Drawing.Size(6, 28);
            // 
            // tsRteAlignLeft
            // 
            this.tsRteAlignLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteAlignLeft.Name = "tsRteAlignLeft";
            this.tsRteAlignLeft.Size = new System.Drawing.Size(30, 25);
            this.tsRteAlignLeft.Text = "≡L";
            this.tsRteAlignLeft.ToolTipText = "ชิดซ้าย (Align Left)";
            this.tsRteAlignLeft.Click += new System.EventHandler(this.TsRteAlignLeft_Click);
            // 
            // tsRteAlignCenter
            // 
            this.tsRteAlignCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteAlignCenter.Name = "tsRteAlignCenter";
            this.tsRteAlignCenter.Size = new System.Drawing.Size(32, 25);
            this.tsRteAlignCenter.Text = "≡C";
            this.tsRteAlignCenter.ToolTipText = "กึ่งกลาง (Align Center)";
            this.tsRteAlignCenter.Click += new System.EventHandler(this.TsRteAlignCenter_Click);
            // 
            // tsRteAlignRight
            // 
            this.tsRteAlignRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteAlignRight.Name = "tsRteAlignRight";
            this.tsRteAlignRight.Size = new System.Drawing.Size(32, 25);
            this.tsRteAlignRight.Text = "≡R";
            this.tsRteAlignRight.ToolTipText = "ชิดขวา (Align Right)";
            this.tsRteAlignRight.Click += new System.EventHandler(this.TsRteAlignRight_Click);
            // 
            // tsRteSep4
            // 
            this.tsRteSep4.Name = "tsRteSep4";
            this.tsRteSep4.Size = new System.Drawing.Size(6, 28);
            // 
            // tsRteBullet
            // 
            this.tsRteBullet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteBullet.Name = "tsRteBullet";
            this.tsRteBullet.Size = new System.Drawing.Size(29, 25);
            this.tsRteBullet.Text = "•";
            this.tsRteBullet.ToolTipText = "รายการสัญลักษณ์ (Bullet List)";
            this.tsRteBullet.Click += new System.EventHandler(this.TsRteBullet_Click);
            // 
            // tsRteNumList
            // 
            this.tsRteNumList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteNumList.Name = "tsRteNumList";
            this.tsRteNumList.Size = new System.Drawing.Size(29, 25);
            this.tsRteNumList.Text = "1.";
            this.tsRteNumList.ToolTipText = "รายการตัวเลข (Numbered List)";
            this.tsRteNumList.Click += new System.EventHandler(this.TsRteNumList_Click);
            // 
            // tsRteSep5
            // 
            this.tsRteSep5.Name = "tsRteSep5";
            this.tsRteSep5.Size = new System.Drawing.Size(6, 28);
            // 
            // tsRteLink
            // 
            this.tsRteLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteLink.Name = "tsRteLink";
            this.tsRteLink.Size = new System.Drawing.Size(34, 25);
            this.tsRteLink.Text = "🔗";
            this.tsRteLink.ToolTipText = "แทรก Hyperlink";
            this.tsRteLink.Click += new System.EventHandler(this.TsRteLink_Click);
            // 
            // tsRteTable
            // 
            this.tsRteTable.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteTable.Name = "tsRteTable";
            this.tsRteTable.Size = new System.Drawing.Size(29, 25);
            this.tsRteTable.Text = "⊞";
            this.tsRteTable.ToolTipText = "แทรกตาราง (Insert Table)";
            this.tsRteTable.Click += new System.EventHandler(this.TsRteTable_Click);
            // 
            // tsRteCode
            // 
            this.tsRteCode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteCode.Font = new System.Drawing.Font("Consolas", 8.5F, System.Drawing.FontStyle.Bold);
            this.tsRteCode.Name = "tsRteCode";
            this.tsRteCode.Size = new System.Drawing.Size(29, 25);
            this.tsRteCode.Text = "<>";
            this.tsRteCode.ToolTipText = "แทรก Code Block";
            this.tsRteCode.Click += new System.EventHandler(this.TsRteCode_Click);
            // 
            // tsRteImage
            // 
            this.tsRteImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteImage.Name = "tsRteImage";
            this.tsRteImage.Size = new System.Drawing.Size(34, 25);
            this.tsRteImage.Text = "📷";
            this.tsRteImage.ToolTipText = "แทรกรูปภาพ";
            this.tsRteImage.Click += new System.EventHandler(this.TsRteImage_Click);
            // 
            // tsRteAttach
            // 
            this.tsRteAttach.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteAttach.Name = "tsRteAttach";
            this.tsRteAttach.Size = new System.Drawing.Size(29, 25);
            this.tsRteAttach.Text = "📎";
            this.tsRteAttach.ToolTipText = "แนบไฟล์";
            this.tsRteAttach.Click += new System.EventHandler(this.TsRteAttach_Click);
            // 
            // pnlSummaryArea
            // 
            this.pnlSummaryArea.BackColor = System.Drawing.Color.White;
            this.pnlSummaryArea.Controls.Add(this.txtSummary);
            this.pnlSummaryArea.Controls.Add(this.lblSummary);
            this.pnlSummaryArea.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSummaryArea.Location = new System.Drawing.Point(0, 28);
            this.pnlSummaryArea.Name = "pnlSummaryArea";
            this.pnlSummaryArea.Padding = new System.Windows.Forms.Padding(10, 4, 10, 0);
            this.pnlSummaryArea.Size = new System.Drawing.Size(1176, 37);
            this.pnlSummaryArea.TabIndex = 1;
            // 
            // txtSummary
            // 
            this.txtSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSummary.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSummary.Location = new System.Drawing.Point(168, 4);
            this.txtSummary.MaxLength = 1000;
            this.txtSummary.Name = "txtSummary";
            this.txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSummary.Size = new System.Drawing.Size(998, 27);
            this.txtSummary.TabIndex = 1;
            // 
            // lblSummary
            // 
            this.lblSummary.AutoSize = true;
            this.lblSummary.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblSummary.Location = new System.Drawing.Point(10, 4);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(158, 20);
            this.lblSummary.TabIndex = 0;
            this.lblSummary.Text = "บทสรุปสั้น (Summary)";
            // 
            // pnlContentHeader
            // 
            this.pnlContentHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlContentHeader.Controls.Add(this.lblContentHeader);
            this.pnlContentHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlContentHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlContentHeader.Name = "pnlContentHeader";
            this.pnlContentHeader.Size = new System.Drawing.Size(1176, 28);
            this.pnlContentHeader.TabIndex = 0;
            // 
            // lblContentHeader
            // 
            this.lblContentHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblContentHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblContentHeader.ForeColor = System.Drawing.Color.White;
            this.lblContentHeader.Location = new System.Drawing.Point(0, 0);
            this.lblContentHeader.Name = "lblContentHeader";
            this.lblContentHeader.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblContentHeader.Size = new System.Drawing.Size(1176, 28);
            this.lblContentHeader.TabIndex = 0;
            this.lblContentHeader.Text = "📝  เนื้อหาบทความ";
            this.lblContentHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlMappingOuter
            // 
            this.pnlMappingOuter.AutoScroll = true;
            this.pnlMappingOuter.BackColor = System.Drawing.Color.White;
            this.pnlMappingOuter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlMappingOuter.Controls.Add(this.tabMapping);
            this.pnlMappingOuter.Controls.Add(this.pnlMappingHeader);
            this.pnlMappingOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMappingOuter.Location = new System.Drawing.Point(0, 0);
            this.pnlMappingOuter.Name = "pnlMappingOuter";
            this.pnlMappingOuter.Size = new System.Drawing.Size(529, 923);
            this.pnlMappingOuter.TabIndex = 14;
            // 
            // tabMapping
            // 
            this.tabMapping.Controls.Add(this.tabAttach);
            this.tabMapping.Controls.Add(this.tabRelated);
            this.tabMapping.Controls.Add(this.tabWorkflow);
            this.tabMapping.Controls.Add(this.tabPrograms);
            this.tabMapping.Controls.Add(this.tabDocs);
            this.tabMapping.Controls.Add(this.tabDepartments);
            this.tabMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMapping.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabMapping.Location = new System.Drawing.Point(0, 28);
            this.tabMapping.Name = "tabMapping";
            this.tabMapping.SelectedIndex = 0;
            this.tabMapping.Size = new System.Drawing.Size(527, 893);
            this.tabMapping.TabIndex = 1;
            // 
            // tabAttach
            // 
            this.tabAttach.AutoScroll = true;
            this.tabAttach.BackColor = System.Drawing.Color.White;
            this.tabAttach.Controls.Add(this.pnlAttachUpload);
            this.tabAttach.Controls.Add(this.dgvAttachLinked);
            this.tabAttach.Controls.Add(this.lblAttachLinked);
            this.tabAttach.Location = new System.Drawing.Point(4, 29);
            this.tabAttach.Name = "tabAttach";
            this.tabAttach.Padding = new System.Windows.Forms.Padding(4);
            this.tabAttach.Size = new System.Drawing.Size(519, 860);
            this.tabAttach.TabIndex = 0;
            this.tabAttach.Text = "📎 ไฟล์ที่แนบ";
            // 
            // pnlAttachUpload
            // 
            this.pnlAttachUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlAttachUpload.Controls.Add(this.btnSelectFromDMS);
            this.pnlAttachUpload.Controls.Add(this.btnUploadFile);
            this.pnlAttachUpload.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAttachUpload.Location = new System.Drawing.Point(4, 148);
            this.pnlAttachUpload.Name = "pnlAttachUpload";
            this.pnlAttachUpload.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlAttachUpload.Size = new System.Drawing.Size(511, 54);
            this.pnlAttachUpload.TabIndex = 3;
            // 
            // btnSelectFromDMS
            // 
            this.btnSelectFromDMS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnSelectFromDMS.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelectFromDMS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectFromDMS.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSelectFromDMS.ForeColor = System.Drawing.Color.White;
            this.btnSelectFromDMS.Location = new System.Drawing.Point(154, 6);
            this.btnSelectFromDMS.Name = "btnSelectFromDMS";
            this.btnSelectFromDMS.Size = new System.Drawing.Size(150, 39);
            this.btnSelectFromDMS.TabIndex = 1;
            this.btnSelectFromDMS.Text = "📁 เลือกจาก DMS";
            this.btnSelectFromDMS.UseVisualStyleBackColor = false;
            this.btnSelectFromDMS.Click += new System.EventHandler(this.BtnSelectFromDMS_Click);
            // 
            // btnUploadFile
            // 
            this.btnUploadFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnUploadFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUploadFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUploadFile.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnUploadFile.ForeColor = System.Drawing.Color.White;
            this.btnUploadFile.Location = new System.Drawing.Point(8, 6);
            this.btnUploadFile.Name = "btnUploadFile";
            this.btnUploadFile.Size = new System.Drawing.Size(140, 39);
            this.btnUploadFile.TabIndex = 0;
            this.btnUploadFile.Text = "📤 อัพโหลดไฟล์";
            this.btnUploadFile.UseVisualStyleBackColor = false;
            this.btnUploadFile.Click += new System.EventHandler(this.BtnUploadFileNew_Click);
            // 
            // dgvAttachLinked
            // 
            this.dgvAttachLinked.AllowUserToAddRows = false;
            this.dgvAttachLinked.AllowUserToDeleteRows = false;
            this.dgvAttachLinked.AllowUserToResizeRows = false;
            this.dgvAttachLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvAttachLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAttachLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNum,
            this.colFilename,
            this.colSize,
            this.colDate,
            this.colView,
            this.colDelete});
            this.dgvAttachLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvAttachLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvAttachLinked.Name = "dgvAttachLinked";
            this.dgvAttachLinked.RowHeadersVisible = false;
            this.dgvAttachLinked.RowHeadersWidth = 51;
            this.dgvAttachLinked.RowTemplate.Height = 28;
            this.dgvAttachLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAttachLinked.Size = new System.Drawing.Size(511, 120);
            this.dgvAttachLinked.TabIndex = 1;
            this.dgvAttachLinked.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvAttachLinked_CellContentClick);
            // 
            // colNum
            // 
            this.colNum.HeaderText = "#";
            this.colNum.MinimumWidth = 6;
            this.colNum.Name = "colNum";
            this.colNum.ReadOnly = true;
            this.colNum.Width = 50;
            // 
            // colFilename
            // 
            this.colFilename.HeaderText = "ชื่อไฟล์";
            this.colFilename.MinimumWidth = 6;
            this.colFilename.Name = "colFilename";
            this.colFilename.ReadOnly = true;
            this.colFilename.Width = 400;
            // 
            // colSize
            // 
            this.colSize.HeaderText = "ขนาด";
            this.colSize.MinimumWidth = 6;
            this.colSize.Name = "colSize";
            this.colSize.ReadOnly = true;
            this.colSize.Width = 125;
            // 
            // colDate
            // 
            this.colDate.HeaderText = "วันที่แนบ";
            this.colDate.MinimumWidth = 6;
            this.colDate.Name = "colDate";
            this.colDate.ReadOnly = true;
            this.colDate.Width = 150;
            // 
            // colView
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(95)))), ((int)(((byte)(154)))));
            this.colView.DefaultCellStyle = dataGridViewCellStyle1;
            this.colView.HeaderText = "ดูไฟล์";
            this.colView.MinimumWidth = 6;
            this.colView.Name = "colView";
            this.colView.Text = "👁️ ดู";
            this.colView.UseColumnTextForButtonValue = true;
            this.colView.Width = 80;
            // 
            // colDelete
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.colDelete.DefaultCellStyle = dataGridViewCellStyle2;
            this.colDelete.HeaderText = "ลบไฟล์";
            this.colDelete.MinimumWidth = 6;
            this.colDelete.Name = "colDelete";
            this.colDelete.Text = "🗑️ ลบ";
            this.colDelete.UseColumnTextForButtonValue = true;
            this.colDelete.Width = 80;
            // 
            // lblAttachLinked
            // 
            this.lblAttachLinked.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblAttachLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAttachLinked.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAttachLinked.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblAttachLinked.Location = new System.Drawing.Point(4, 4);
            this.lblAttachLinked.Name = "lblAttachLinked";
            this.lblAttachLinked.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblAttachLinked.Size = new System.Drawing.Size(511, 24);
            this.lblAttachLinked.TabIndex = 0;
            this.lblAttachLinked.Text = "ไฟล์ที่แนบ (0 ไฟล์)";
            // 
            // tabRelated
            // 
            this.tabRelated.AutoScroll = true;
            this.tabRelated.BackColor = System.Drawing.Color.White;
            this.tabRelated.Controls.Add(this.dgvRelatedSearch);
            this.tabRelated.Controls.Add(this.lblRelatedSearch);
            this.tabRelated.Controls.Add(this.pnlRelatedSearch);
            this.tabRelated.Controls.Add(this.dgvRelatedLinked);
            this.tabRelated.Controls.Add(this.lblRelatedLinked);
            this.tabRelated.Location = new System.Drawing.Point(4, 29);
            this.tabRelated.Name = "tabRelated";
            this.tabRelated.Padding = new System.Windows.Forms.Padding(4);
            this.tabRelated.Size = new System.Drawing.Size(519, 860);
            this.tabRelated.TabIndex = 1;
            this.tabRelated.Text = "🔗 ความรู้ที่เกี่ยวข้อง";
            // 
            // dgvRelatedSearch
            // 
            this.dgvRelatedSearch.AllowUserToAddRows = false;
            this.dgvRelatedSearch.AllowUserToDeleteRows = false;
            this.dgvRelatedSearch.AllowUserToResizeRows = false;
            this.dgvRelatedSearch.BackgroundColor = System.Drawing.Color.White;
            this.dgvRelatedSearch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRelatedSearch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRelatedSearchCode,
            this.colRelatedSearchTitle,
            this.colRelatedSummary,
            this.colRelatedSearchType,
            this.colRelatedSearchViews});
            this.dgvRelatedSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvRelatedSearch.Location = new System.Drawing.Point(4, 274);
            this.dgvRelatedSearch.Name = "dgvRelatedSearch";
            this.dgvRelatedSearch.ReadOnly = true;
            this.dgvRelatedSearch.RowHeadersVisible = false;
            this.dgvRelatedSearch.RowHeadersWidth = 51;
            this.dgvRelatedSearch.RowTemplate.Height = 28;
            this.dgvRelatedSearch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRelatedSearch.Size = new System.Drawing.Size(511, 300);
            this.dgvRelatedSearch.TabIndex = 5;
            this.dgvRelatedSearch.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvRelatedSearch_CellDoubleClick);
            // 
            // colRelatedSearchCode
            // 
            this.colRelatedSearchCode.HeaderText = "รหัสบทความ";
            this.colRelatedSearchCode.MinimumWidth = 6;
            this.colRelatedSearchCode.Name = "colRelatedSearchCode";
            this.colRelatedSearchCode.ReadOnly = true;
            this.colRelatedSearchCode.Visible = false;
            this.colRelatedSearchCode.Width = 120;
            // 
            // colRelatedSearchTitle
            // 
            this.colRelatedSearchTitle.HeaderText = "ชื่อบทความ";
            this.colRelatedSearchTitle.MinimumWidth = 6;
            this.colRelatedSearchTitle.Name = "colRelatedSearchTitle";
            this.colRelatedSearchTitle.ReadOnly = true;
            this.colRelatedSearchTitle.Width = 125;
            // 
            // colRelatedSummary
            // 
            this.colRelatedSummary.HeaderText = "Summary";
            this.colRelatedSummary.MinimumWidth = 6;
            this.colRelatedSummary.Name = "colRelatedSummary";
            this.colRelatedSummary.ReadOnly = true;
            this.colRelatedSummary.Width = 125;
            // 
            // colRelatedSearchType
            // 
            this.colRelatedSearchType.HeaderText = "ประเภท";
            this.colRelatedSearchType.MinimumWidth = 6;
            this.colRelatedSearchType.Name = "colRelatedSearchType";
            this.colRelatedSearchType.ReadOnly = true;
            this.colRelatedSearchType.Width = 125;
            // 
            // colRelatedSearchViews
            // 
            this.colRelatedSearchViews.HeaderText = "ดู";
            this.colRelatedSearchViews.MinimumWidth = 6;
            this.colRelatedSearchViews.Name = "colRelatedSearchViews";
            this.colRelatedSearchViews.ReadOnly = true;
            this.colRelatedSearchViews.Width = 60;
            // 
            // lblRelatedSearch
            // 
            this.lblRelatedSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblRelatedSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRelatedSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRelatedSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblRelatedSearch.Location = new System.Drawing.Point(4, 250);
            this.lblRelatedSearch.Name = "lblRelatedSearch";
            this.lblRelatedSearch.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblRelatedSearch.Size = new System.Drawing.Size(511, 24);
            this.lblRelatedSearch.TabIndex = 4;
            this.lblRelatedSearch.Text = "ผลการค้นหา — ดับเบิลคลิกเพื่อเพิ่ม";
            // 
            // pnlRelatedSearch
            // 
            this.pnlRelatedSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlRelatedSearch.Controls.Add(this.btnSearchRelated);
            this.pnlRelatedSearch.Controls.Add(this.cboRelatedStatus);
            this.pnlRelatedSearch.Controls.Add(this.cboRelatedCategory);
            this.pnlRelatedSearch.Controls.Add(this.cboRelatedType);
            this.pnlRelatedSearch.Controls.Add(this.txtSearchRelated);
            this.pnlRelatedSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRelatedSearch.Location = new System.Drawing.Point(4, 214);
            this.pnlRelatedSearch.Name = "pnlRelatedSearch";
            this.pnlRelatedSearch.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlRelatedSearch.Size = new System.Drawing.Size(511, 36);
            this.pnlRelatedSearch.TabIndex = 3;
            // 
            // btnSearchRelated
            // 
            this.btnSearchRelated.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnSearchRelated.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchRelated.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchRelated.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSearchRelated.ForeColor = System.Drawing.Color.White;
            this.btnSearchRelated.Location = new System.Drawing.Point(682, 6);
            this.btnSearchRelated.Name = "btnSearchRelated";
            this.btnSearchRelated.Size = new System.Drawing.Size(80, 24);
            this.btnSearchRelated.TabIndex = 4;
            this.btnSearchRelated.Text = "🔎 ค้นหา";
            this.btnSearchRelated.UseVisualStyleBackColor = false;
            this.btnSearchRelated.Click += new System.EventHandler(this.BtnSearchRelated_Click);
            // 
            // cboRelatedStatus
            // 
            this.cboRelatedStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRelatedStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboRelatedStatus.FormattingEnabled = true;
            this.cboRelatedStatus.Location = new System.Drawing.Point(546, 6);
            this.cboRelatedStatus.Name = "cboRelatedStatus";
            this.cboRelatedStatus.Size = new System.Drawing.Size(130, 28);
            this.cboRelatedStatus.TabIndex = 3;
            // 
            // cboRelatedCategory
            // 
            this.cboRelatedCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRelatedCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboRelatedCategory.FormattingEnabled = true;
            this.cboRelatedCategory.Location = new System.Drawing.Point(420, 6);
            this.cboRelatedCategory.Name = "cboRelatedCategory";
            this.cboRelatedCategory.Size = new System.Drawing.Size(120, 28);
            this.cboRelatedCategory.TabIndex = 2;
            // 
            // cboRelatedType
            // 
            this.cboRelatedType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRelatedType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboRelatedType.FormattingEnabled = true;
            this.cboRelatedType.Location = new System.Drawing.Point(314, 6);
            this.cboRelatedType.Name = "cboRelatedType";
            this.cboRelatedType.Size = new System.Drawing.Size(100, 28);
            this.cboRelatedType.TabIndex = 1;
            // 
            // txtSearchRelated
            // 
            this.txtSearchRelated.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearchRelated.ForeColor = System.Drawing.Color.Gray;
            this.txtSearchRelated.Location = new System.Drawing.Point(8, 6);
            this.txtSearchRelated.Name = "txtSearchRelated";
            this.txtSearchRelated.Size = new System.Drawing.Size(300, 27);
            this.txtSearchRelated.TabIndex = 0;
            this.txtSearchRelated.Text = "🔍 ค้นหาชื่อบทความ / รหัส / แท็ก...";
            this.txtSearchRelated.TextChanged += new System.EventHandler(this.TxtSearchRelated_TextChanged);
            this.txtSearchRelated.Enter += new System.EventHandler(this.TxtSearchRelatedNew_Enter);
            this.txtSearchRelated.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearchRelatedNew_KeyDown);
            this.txtSearchRelated.Leave += new System.EventHandler(this.TxtSearchRelatedNew_Leave);
            // 
            // dgvRelatedLinked
            // 
            this.dgvRelatedLinked.AllowUserToAddRows = false;
            this.dgvRelatedLinked.AllowUserToDeleteRows = false;
            this.dgvRelatedLinked.AllowUserToResizeRows = false;
            this.dgvRelatedLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvRelatedLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRelatedLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRelatedLinkedNum,
            this.colCode,
            this.colTitle,
            this.colRelatedLinkedSummary,
            this.colType,
            this.colRelatedLinkedDelete});
            this.dgvRelatedLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvRelatedLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvRelatedLinked.Name = "dgvRelatedLinked";
            this.dgvRelatedLinked.RowHeadersVisible = false;
            this.dgvRelatedLinked.RowHeadersWidth = 51;
            this.dgvRelatedLinked.RowTemplate.Height = 28;
            this.dgvRelatedLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRelatedLinked.Size = new System.Drawing.Size(511, 186);
            this.dgvRelatedLinked.TabIndex = 1;
            this.dgvRelatedLinked.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvRelatedLinked_CellContentClick);
            // 
            // colRelatedLinkedNum
            // 
            this.colRelatedLinkedNum.HeaderText = "#";
            this.colRelatedLinkedNum.MinimumWidth = 6;
            this.colRelatedLinkedNum.Name = "colRelatedLinkedNum";
            this.colRelatedLinkedNum.ReadOnly = true;
            this.colRelatedLinkedNum.Width = 40;
            // 
            // colCode
            // 
            this.colCode.HeaderText = "รหัสบทความ";
            this.colCode.MinimumWidth = 6;
            this.colCode.Name = "colCode";
            this.colCode.ReadOnly = true;
            this.colCode.Visible = false;
            this.colCode.Width = 120;
            // 
            // colTitle
            // 
            this.colTitle.HeaderText = "ชื่อบทความ";
            this.colTitle.MinimumWidth = 6;
            this.colTitle.Name = "colTitle";
            this.colTitle.ReadOnly = true;
            this.colTitle.Width = 125;
            // 
            // colRelatedLinkedSummary
            // 
            this.colRelatedLinkedSummary.HeaderText = "Summary";
            this.colRelatedLinkedSummary.MinimumWidth = 6;
            this.colRelatedLinkedSummary.Name = "colRelatedLinkedSummary";
            this.colRelatedLinkedSummary.Width = 125;
            // 
            // colType
            // 
            this.colType.HeaderText = "ประเภท";
            this.colType.MinimumWidth = 6;
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            this.colType.Width = 125;
            // 
            // colRelatedLinkedDelete
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.colRelatedLinkedDelete.DefaultCellStyle = dataGridViewCellStyle3;
            this.colRelatedLinkedDelete.HeaderText = "ลบ";
            this.colRelatedLinkedDelete.MinimumWidth = 6;
            this.colRelatedLinkedDelete.Name = "colRelatedLinkedDelete";
            this.colRelatedLinkedDelete.Text = "🗑️ ลบ";
            this.colRelatedLinkedDelete.UseColumnTextForButtonValue = true;
            this.colRelatedLinkedDelete.Width = 80;
            // 
            // lblRelatedLinked
            // 
            this.lblRelatedLinked.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblRelatedLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRelatedLinked.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRelatedLinked.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblRelatedLinked.Location = new System.Drawing.Point(4, 4);
            this.lblRelatedLinked.Name = "lblRelatedLinked";
            this.lblRelatedLinked.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblRelatedLinked.Size = new System.Drawing.Size(511, 24);
            this.lblRelatedLinked.TabIndex = 0;
            this.lblRelatedLinked.Text = "รายการที่ Link อยู่แล้ว (0 รายการ)";
            // 
            // tabWorkflow
            // 
            this.tabWorkflow.AutoScroll = true;
            this.tabWorkflow.BackColor = System.Drawing.Color.White;
            this.tabWorkflow.Controls.Add(this.dgvWorkflowSearch);
            this.tabWorkflow.Controls.Add(this.lblWorkflowSearch);
            this.tabWorkflow.Controls.Add(this.pnlWorkflowSearch);
            this.tabWorkflow.Controls.Add(this.dgvWorkflowLinked);
            this.tabWorkflow.Controls.Add(this.lblWorkflowLinked);
            this.tabWorkflow.Location = new System.Drawing.Point(4, 29);
            this.tabWorkflow.Name = "tabWorkflow";
            this.tabWorkflow.Padding = new System.Windows.Forms.Padding(4);
            this.tabWorkflow.Size = new System.Drawing.Size(519, 860);
            this.tabWorkflow.TabIndex = 2;
            this.tabWorkflow.Text = "⚖️ เส้นงาน (Workflow)";
            // 
            // dgvWorkflowSearch
            // 
            this.dgvWorkflowSearch.AllowUserToAddRows = false;
            this.dgvWorkflowSearch.AllowUserToDeleteRows = false;
            this.dgvWorkflowSearch.BackgroundColor = System.Drawing.Color.White;
            this.dgvWorkflowSearch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWorkflowSearch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colWorkflowSearchCode,
            this.colName,
            this.colWorkflowSearchType,
            this.colStatus});
            this.dgvWorkflowSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvWorkflowSearch.Location = new System.Drawing.Point(4, 208);
            this.dgvWorkflowSearch.MultiSelect = false;
            this.dgvWorkflowSearch.Name = "dgvWorkflowSearch";
            this.dgvWorkflowSearch.ReadOnly = true;
            this.dgvWorkflowSearch.RowHeadersVisible = false;
            this.dgvWorkflowSearch.RowHeadersWidth = 51;
            this.dgvWorkflowSearch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvWorkflowSearch.Size = new System.Drawing.Size(511, 648);
            this.dgvWorkflowSearch.TabIndex = 4;
            this.dgvWorkflowSearch.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvWorkflowSearch_CellDoubleClick);
            // 
            // colWorkflowSearchCode
            // 
            this.colWorkflowSearchCode.HeaderText = "รหัส Workflow";
            this.colWorkflowSearchCode.MinimumWidth = 6;
            this.colWorkflowSearchCode.Name = "colWorkflowSearchCode";
            this.colWorkflowSearchCode.ReadOnly = true;
            this.colWorkflowSearchCode.Width = 120;
            // 
            // colName
            // 
            this.colName.HeaderText = "ชื่อ Workflow";
            this.colName.MinimumWidth = 6;
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Width = 300;
            // 
            // colWorkflowSearchType
            // 
            this.colWorkflowSearchType.HeaderText = "ประเภท";
            this.colWorkflowSearchType.MinimumWidth = 6;
            this.colWorkflowSearchType.Name = "colWorkflowSearchType";
            this.colWorkflowSearchType.ReadOnly = true;
            this.colWorkflowSearchType.Width = 120;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "สถานะ";
            this.colStatus.MinimumWidth = 6;
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            this.colStatus.Width = 125;
            // 
            // lblWorkflowSearch
            // 
            this.lblWorkflowSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblWorkflowSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWorkflowSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWorkflowSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblWorkflowSearch.Location = new System.Drawing.Point(4, 184);
            this.lblWorkflowSearch.Name = "lblWorkflowSearch";
            this.lblWorkflowSearch.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblWorkflowSearch.Size = new System.Drawing.Size(511, 24);
            this.lblWorkflowSearch.TabIndex = 3;
            this.lblWorkflowSearch.Text = "ผลการค้นหา (0 รายการ) — ดับเบิลคลิกเพื่อเพิ่ม";
            // 
            // pnlWorkflowSearch
            // 
            this.pnlWorkflowSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlWorkflowSearch.Controls.Add(this.btnSearchWorkflow);
            this.pnlWorkflowSearch.Controls.Add(this.cboWorkflowStatus);
            this.pnlWorkflowSearch.Controls.Add(this.cboWorkflowCategory);
            this.pnlWorkflowSearch.Controls.Add(this.cboWorkflowType);
            this.pnlWorkflowSearch.Controls.Add(this.txtSearchWorkflow);
            this.pnlWorkflowSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlWorkflowSearch.Location = new System.Drawing.Point(4, 148);
            this.pnlWorkflowSearch.Name = "pnlWorkflowSearch";
            this.pnlWorkflowSearch.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlWorkflowSearch.Size = new System.Drawing.Size(511, 36);
            this.pnlWorkflowSearch.TabIndex = 2;
            // 
            // btnSearchWorkflow
            // 
            this.btnSearchWorkflow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearchWorkflow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnSearchWorkflow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchWorkflow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchWorkflow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSearchWorkflow.ForeColor = System.Drawing.Color.White;
            this.btnSearchWorkflow.Location = new System.Drawing.Point(435, 6);
            this.btnSearchWorkflow.Name = "btnSearchWorkflow";
            this.btnSearchWorkflow.Size = new System.Drawing.Size(68, 24);
            this.btnSearchWorkflow.TabIndex = 4;
            this.btnSearchWorkflow.Text = "🔍 ค้นหา";
            this.btnSearchWorkflow.UseVisualStyleBackColor = false;
            this.btnSearchWorkflow.Click += new System.EventHandler(this.BtnSearchWorkflow_Click);
            // 
            // cboWorkflowStatus
            // 
            this.cboWorkflowStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboWorkflowStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWorkflowStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboWorkflowStatus.FormattingEnabled = true;
            this.cboWorkflowStatus.Location = new System.Drawing.Point(355, 6);
            this.cboWorkflowStatus.Name = "cboWorkflowStatus";
            this.cboWorkflowStatus.Size = new System.Drawing.Size(74, 28);
            this.cboWorkflowStatus.TabIndex = 3;
            // 
            // cboWorkflowCategory
            // 
            this.cboWorkflowCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboWorkflowCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWorkflowCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboWorkflowCategory.FormattingEnabled = true;
            this.cboWorkflowCategory.Location = new System.Drawing.Point(255, 6);
            this.cboWorkflowCategory.Name = "cboWorkflowCategory";
            this.cboWorkflowCategory.Size = new System.Drawing.Size(94, 28);
            this.cboWorkflowCategory.TabIndex = 2;
            // 
            // cboWorkflowType
            // 
            this.cboWorkflowType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboWorkflowType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWorkflowType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboWorkflowType.FormattingEnabled = true;
            this.cboWorkflowType.Location = new System.Drawing.Point(165, 6);
            this.cboWorkflowType.Name = "cboWorkflowType";
            this.cboWorkflowType.Size = new System.Drawing.Size(84, 28);
            this.cboWorkflowType.TabIndex = 1;
            // 
            // txtSearchWorkflow
            // 
            this.txtSearchWorkflow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearchWorkflow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearchWorkflow.Location = new System.Drawing.Point(8, 6);
            this.txtSearchWorkflow.Name = "txtSearchWorkflow";
            this.txtSearchWorkflow.Size = new System.Drawing.Size(151, 27);
            this.txtSearchWorkflow.TabIndex = 0;
            this.txtSearchWorkflow.Enter += new System.EventHandler(this.TxtSearchWorkflow_Enter);
            this.txtSearchWorkflow.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearchWorkflow_KeyDown);
            this.txtSearchWorkflow.Leave += new System.EventHandler(this.TxtSearchWorkflow_Leave);
            // 
            // dgvWorkflowLinked
            // 
            this.dgvWorkflowLinked.AllowUserToAddRows = false;
            this.dgvWorkflowLinked.AllowUserToDeleteRows = false;
            this.dgvWorkflowLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvWorkflowLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWorkflowLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colWorkflowLinkedNum,
            this.colWorkflowLinkedCode,
            this.colWorkflowLinkedName,
            this.colWorkflowLinkedStatus,
            this.colWorkflowLinkedDelete});
            this.dgvWorkflowLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvWorkflowLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvWorkflowLinked.MultiSelect = false;
            this.dgvWorkflowLinked.Name = "dgvWorkflowLinked";
            this.dgvWorkflowLinked.ReadOnly = true;
            this.dgvWorkflowLinked.RowHeadersVisible = false;
            this.dgvWorkflowLinked.RowHeadersWidth = 51;
            this.dgvWorkflowLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvWorkflowLinked.Size = new System.Drawing.Size(511, 120);
            this.dgvWorkflowLinked.TabIndex = 1;
            this.dgvWorkflowLinked.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvWorkflowLinked_CellContentClick);
            // 
            // colWorkflowLinkedNum
            // 
            this.colWorkflowLinkedNum.HeaderText = "#";
            this.colWorkflowLinkedNum.MinimumWidth = 6;
            this.colWorkflowLinkedNum.Name = "colWorkflowLinkedNum";
            this.colWorkflowLinkedNum.ReadOnly = true;
            this.colWorkflowLinkedNum.Width = 40;
            // 
            // colWorkflowLinkedCode
            // 
            this.colWorkflowLinkedCode.HeaderText = "รหัส Workflow";
            this.colWorkflowLinkedCode.MinimumWidth = 6;
            this.colWorkflowLinkedCode.Name = "colWorkflowLinkedCode";
            this.colWorkflowLinkedCode.ReadOnly = true;
            this.colWorkflowLinkedCode.Width = 120;
            // 
            // colWorkflowLinkedName
            // 
            this.colWorkflowLinkedName.HeaderText = "ชื่อ Workflow";
            this.colWorkflowLinkedName.MinimumWidth = 6;
            this.colWorkflowLinkedName.Name = "colWorkflowLinkedName";
            this.colWorkflowLinkedName.ReadOnly = true;
            this.colWorkflowLinkedName.Width = 250;
            // 
            // colWorkflowLinkedStatus
            // 
            this.colWorkflowLinkedStatus.HeaderText = "สถานะ";
            this.colWorkflowLinkedStatus.MinimumWidth = 6;
            this.colWorkflowLinkedStatus.Name = "colWorkflowLinkedStatus";
            this.colWorkflowLinkedStatus.ReadOnly = true;
            this.colWorkflowLinkedStatus.Width = 125;
            // 
            // colWorkflowLinkedDelete
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.colWorkflowLinkedDelete.DefaultCellStyle = dataGridViewCellStyle4;
            this.colWorkflowLinkedDelete.HeaderText = "ลบ";
            this.colWorkflowLinkedDelete.MinimumWidth = 6;
            this.colWorkflowLinkedDelete.Name = "colWorkflowLinkedDelete";
            this.colWorkflowLinkedDelete.ReadOnly = true;
            this.colWorkflowLinkedDelete.Text = "🗑️ ลบ";
            this.colWorkflowLinkedDelete.UseColumnTextForButtonValue = true;
            this.colWorkflowLinkedDelete.Width = 80;
            // 
            // lblWorkflowLinked
            // 
            this.lblWorkflowLinked.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblWorkflowLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWorkflowLinked.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWorkflowLinked.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblWorkflowLinked.Location = new System.Drawing.Point(4, 4);
            this.lblWorkflowLinked.Name = "lblWorkflowLinked";
            this.lblWorkflowLinked.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblWorkflowLinked.Size = new System.Drawing.Size(511, 24);
            this.lblWorkflowLinked.TabIndex = 0;
            this.lblWorkflowLinked.Text = "Workflow ที่เชื่อมโยง (0 รายการ)";
            // 
            // tabPrograms
            // 
            this.tabPrograms.AutoScroll = true;
            this.tabPrograms.BackColor = System.Drawing.Color.White;
            this.tabPrograms.Controls.Add(this.dgvProgramsSearch);
            this.tabPrograms.Controls.Add(this.lblProgramsSearch);
            this.tabPrograms.Controls.Add(this.pnlProgramsSearch);
            this.tabPrograms.Controls.Add(this.dgvProgramsLinked);
            this.tabPrograms.Controls.Add(this.lblProgramsLinked);
            this.tabPrograms.Location = new System.Drawing.Point(4, 29);
            this.tabPrograms.Name = "tabPrograms";
            this.tabPrograms.Padding = new System.Windows.Forms.Padding(4);
            this.tabPrograms.Size = new System.Drawing.Size(519, 860);
            this.tabPrograms.TabIndex = 3;
            this.tabPrograms.Text = "💻 โปรแกรมที่เกี่ยวข้อง";
            // 
            // dgvProgramsSearch
            // 
            this.dgvProgramsSearch.AllowUserToAddRows = false;
            this.dgvProgramsSearch.AllowUserToDeleteRows = false;
            this.dgvProgramsSearch.AllowUserToResizeRows = false;
            this.dgvProgramsSearch.BackgroundColor = System.Drawing.Color.White;
            this.dgvProgramsSearch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProgramsSearch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSearchCode,
            this.colSearchName,
            this.colSearchType,
            this.colSearchStatus});
            this.dgvProgramsSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvProgramsSearch.Location = new System.Drawing.Point(4, 274);
            this.dgvProgramsSearch.Name = "dgvProgramsSearch";
            this.dgvProgramsSearch.ReadOnly = true;
            this.dgvProgramsSearch.RowHeadersVisible = false;
            this.dgvProgramsSearch.RowHeadersWidth = 51;
            this.dgvProgramsSearch.RowTemplate.Height = 28;
            this.dgvProgramsSearch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProgramsSearch.Size = new System.Drawing.Size(511, 300);
            this.dgvProgramsSearch.TabIndex = 5;
            this.dgvProgramsSearch.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvProgramsSearch_CellDoubleClick);
            // 
            // colSearchCode
            // 
            this.colSearchCode.HeaderText = "รหัสโปรแกรม";
            this.colSearchCode.MinimumWidth = 6;
            this.colSearchCode.Name = "colSearchCode";
            this.colSearchCode.ReadOnly = true;
            this.colSearchCode.Width = 120;
            // 
            // colSearchName
            // 
            this.colSearchName.HeaderText = "ชื่อโปรแกรม";
            this.colSearchName.MinimumWidth = 6;
            this.colSearchName.Name = "colSearchName";
            this.colSearchName.ReadOnly = true;
            this.colSearchName.Width = 300;
            // 
            // colSearchType
            // 
            this.colSearchType.HeaderText = "ประเภท";
            this.colSearchType.MinimumWidth = 6;
            this.colSearchType.Name = "colSearchType";
            this.colSearchType.ReadOnly = true;
            this.colSearchType.Width = 150;
            // 
            // colSearchStatus
            // 
            this.colSearchStatus.HeaderText = "สถานะ";
            this.colSearchStatus.MinimumWidth = 6;
            this.colSearchStatus.Name = "colSearchStatus";
            this.colSearchStatus.ReadOnly = true;
            this.colSearchStatus.Width = 125;
            // 
            // lblProgramsSearch
            // 
            this.lblProgramsSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblProgramsSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblProgramsSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProgramsSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblProgramsSearch.Location = new System.Drawing.Point(4, 250);
            this.lblProgramsSearch.Name = "lblProgramsSearch";
            this.lblProgramsSearch.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblProgramsSearch.Size = new System.Drawing.Size(511, 24);
            this.lblProgramsSearch.TabIndex = 4;
            this.lblProgramsSearch.Text = "ผลการค้นหา — ดับเบิลคลิกเพื่อเพิ่ม";
            // 
            // pnlProgramsSearch
            // 
            this.pnlProgramsSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlProgramsSearch.Controls.Add(this.btnSearchPrograms);
            this.pnlProgramsSearch.Controls.Add(this.cboProgramsStatus);
            this.pnlProgramsSearch.Controls.Add(this.cboProgramsCategory);
            this.pnlProgramsSearch.Controls.Add(this.cboProgramsType);
            this.pnlProgramsSearch.Controls.Add(this.txtSearchPrograms);
            this.pnlProgramsSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlProgramsSearch.Location = new System.Drawing.Point(4, 214);
            this.pnlProgramsSearch.Name = "pnlProgramsSearch";
            this.pnlProgramsSearch.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlProgramsSearch.Size = new System.Drawing.Size(511, 36);
            this.pnlProgramsSearch.TabIndex = 3;
            // 
            // btnSearchPrograms
            // 
            this.btnSearchPrograms.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnSearchPrograms.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchPrograms.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchPrograms.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSearchPrograms.ForeColor = System.Drawing.Color.White;
            this.btnSearchPrograms.Location = new System.Drawing.Point(682, 6);
            this.btnSearchPrograms.Name = "btnSearchPrograms";
            this.btnSearchPrograms.Size = new System.Drawing.Size(80, 24);
            this.btnSearchPrograms.TabIndex = 4;
            this.btnSearchPrograms.Text = "🔎 ค้นหา";
            this.btnSearchPrograms.UseVisualStyleBackColor = false;
            this.btnSearchPrograms.Click += new System.EventHandler(this.BtnSearchPrograms_Click);
            // 
            // cboProgramsStatus
            // 
            this.cboProgramsStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProgramsStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboProgramsStatus.FormattingEnabled = true;
            this.cboProgramsStatus.Location = new System.Drawing.Point(546, 6);
            this.cboProgramsStatus.Name = "cboProgramsStatus";
            this.cboProgramsStatus.Size = new System.Drawing.Size(130, 28);
            this.cboProgramsStatus.TabIndex = 3;
            // 
            // cboProgramsCategory
            // 
            this.cboProgramsCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProgramsCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboProgramsCategory.FormattingEnabled = true;
            this.cboProgramsCategory.Location = new System.Drawing.Point(420, 6);
            this.cboProgramsCategory.Name = "cboProgramsCategory";
            this.cboProgramsCategory.Size = new System.Drawing.Size(120, 28);
            this.cboProgramsCategory.TabIndex = 2;
            // 
            // cboProgramsType
            // 
            this.cboProgramsType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProgramsType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboProgramsType.FormattingEnabled = true;
            this.cboProgramsType.Location = new System.Drawing.Point(314, 6);
            this.cboProgramsType.Name = "cboProgramsType";
            this.cboProgramsType.Size = new System.Drawing.Size(100, 28);
            this.cboProgramsType.TabIndex = 1;
            // 
            // txtSearchPrograms
            // 
            this.txtSearchPrograms.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearchPrograms.ForeColor = System.Drawing.Color.Gray;
            this.txtSearchPrograms.Location = new System.Drawing.Point(8, 6);
            this.txtSearchPrograms.Name = "txtSearchPrograms";
            this.txtSearchPrograms.Size = new System.Drawing.Size(300, 27);
            this.txtSearchPrograms.TabIndex = 0;
            this.txtSearchPrograms.Text = "🔍 ค้นหาชื่อโปรแกรม / รหัส...";
            this.txtSearchPrograms.Enter += new System.EventHandler(this.TxtSearchPrograms_Enter);
            this.txtSearchPrograms.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearchPrograms_KeyDown);
            this.txtSearchPrograms.Leave += new System.EventHandler(this.TxtSearchPrograms_Leave);
            // 
            // dgvProgramsLinked
            // 
            this.dgvProgramsLinked.AllowUserToAddRows = false;
            this.dgvProgramsLinked.AllowUserToDeleteRows = false;
            this.dgvProgramsLinked.AllowUserToResizeRows = false;
            this.dgvProgramsLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvProgramsLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProgramsLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colProgramsLinkedNum,
            this.colProgramsLinkedCode,
            this.colProgramsLinkedName,
            this.colProgramsLinkedType,
            this.colProgramsLinkedDelete});
            this.dgvProgramsLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvProgramsLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvProgramsLinked.Name = "dgvProgramsLinked";
            this.dgvProgramsLinked.RowHeadersVisible = false;
            this.dgvProgramsLinked.RowHeadersWidth = 51;
            this.dgvProgramsLinked.RowTemplate.Height = 28;
            this.dgvProgramsLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProgramsLinked.Size = new System.Drawing.Size(511, 186);
            this.dgvProgramsLinked.TabIndex = 1;
            this.dgvProgramsLinked.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvProgramsLinked_CellContentClick);
            // 
            // colProgramsLinkedNum
            // 
            this.colProgramsLinkedNum.HeaderText = "#";
            this.colProgramsLinkedNum.MinimumWidth = 6;
            this.colProgramsLinkedNum.Name = "colProgramsLinkedNum";
            this.colProgramsLinkedNum.ReadOnly = true;
            this.colProgramsLinkedNum.Width = 40;
            // 
            // colProgramsLinkedCode
            // 
            this.colProgramsLinkedCode.HeaderText = "รหัสโปรแกรม";
            this.colProgramsLinkedCode.MinimumWidth = 6;
            this.colProgramsLinkedCode.Name = "colProgramsLinkedCode";
            this.colProgramsLinkedCode.ReadOnly = true;
            this.colProgramsLinkedCode.Width = 120;
            // 
            // colProgramsLinkedName
            // 
            this.colProgramsLinkedName.HeaderText = "ชื่อโปรแกรม";
            this.colProgramsLinkedName.MinimumWidth = 6;
            this.colProgramsLinkedName.Name = "colProgramsLinkedName";
            this.colProgramsLinkedName.ReadOnly = true;
            this.colProgramsLinkedName.Width = 300;
            // 
            // colProgramsLinkedType
            // 
            this.colProgramsLinkedType.HeaderText = "ประเภท";
            this.colProgramsLinkedType.MinimumWidth = 6;
            this.colProgramsLinkedType.Name = "colProgramsLinkedType";
            this.colProgramsLinkedType.ReadOnly = true;
            this.colProgramsLinkedType.Width = 150;
            // 
            // colProgramsLinkedDelete
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.colProgramsLinkedDelete.DefaultCellStyle = dataGridViewCellStyle5;
            this.colProgramsLinkedDelete.HeaderText = "ลบ";
            this.colProgramsLinkedDelete.MinimumWidth = 6;
            this.colProgramsLinkedDelete.Name = "colProgramsLinkedDelete";
            this.colProgramsLinkedDelete.Text = "🗑️ ลบ";
            this.colProgramsLinkedDelete.UseColumnTextForButtonValue = true;
            this.colProgramsLinkedDelete.Width = 80;
            // 
            // lblProgramsLinked
            // 
            this.lblProgramsLinked.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblProgramsLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblProgramsLinked.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProgramsLinked.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblProgramsLinked.Location = new System.Drawing.Point(4, 4);
            this.lblProgramsLinked.Name = "lblProgramsLinked";
            this.lblProgramsLinked.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblProgramsLinked.Size = new System.Drawing.Size(511, 24);
            this.lblProgramsLinked.TabIndex = 0;
            this.lblProgramsLinked.Text = "โปรแกรมที่เชื่อมโยง (0 รายการ)";
            // 
            // tabDocs
            // 
            this.tabDocs.AutoScroll = true;
            this.tabDocs.BackColor = System.Drawing.Color.White;
            this.tabDocs.Controls.Add(this.dgvDocsSearch);
            this.tabDocs.Controls.Add(this.lblDocsSearch);
            this.tabDocs.Controls.Add(this.pnlDocsSearch);
            this.tabDocs.Controls.Add(this.dgvDocsLinked);
            this.tabDocs.Controls.Add(this.lblDocsLinked);
            this.tabDocs.Location = new System.Drawing.Point(4, 29);
            this.tabDocs.Name = "tabDocs";
            this.tabDocs.Padding = new System.Windows.Forms.Padding(4);
            this.tabDocs.Size = new System.Drawing.Size(519, 860);
            this.tabDocs.TabIndex = 4;
            this.tabDocs.Text = "📋 เอกสารที่เกี่ยวข้อง";
            // 
            // dgvDocsSearch
            // 
            this.dgvDocsSearch.AllowUserToAddRows = false;
            this.dgvDocsSearch.AllowUserToDeleteRows = false;
            this.dgvDocsSearch.AllowUserToResizeRows = false;
            this.dgvDocsSearch.BackgroundColor = System.Drawing.Color.White;
            this.dgvDocsSearch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDocsSearch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDocsSearchCode,
            this.colDocsSearchTitle,
            this.colDocsSearchFolder,
            this.colDocsSearchSize});
            this.dgvDocsSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvDocsSearch.Location = new System.Drawing.Point(4, 274);
            this.dgvDocsSearch.Name = "dgvDocsSearch";
            this.dgvDocsSearch.ReadOnly = true;
            this.dgvDocsSearch.RowHeadersVisible = false;
            this.dgvDocsSearch.RowHeadersWidth = 51;
            this.dgvDocsSearch.RowTemplate.Height = 28;
            this.dgvDocsSearch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDocsSearch.Size = new System.Drawing.Size(511, 300);
            this.dgvDocsSearch.TabIndex = 5;
            this.dgvDocsSearch.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvDocsSearch_CellDoubleClick);
            // 
            // colDocsSearchCode
            // 
            this.colDocsSearchCode.HeaderText = "รหัสเอกสาร";
            this.colDocsSearchCode.MinimumWidth = 6;
            this.colDocsSearchCode.Name = "colDocsSearchCode";
            this.colDocsSearchCode.ReadOnly = true;
            this.colDocsSearchCode.Width = 120;
            // 
            // colDocsSearchTitle
            // 
            this.colDocsSearchTitle.HeaderText = "ชื่อเอกสาร";
            this.colDocsSearchTitle.MinimumWidth = 6;
            this.colDocsSearchTitle.Name = "colDocsSearchTitle";
            this.colDocsSearchTitle.ReadOnly = true;
            this.colDocsSearchTitle.Width = 250;
            // 
            // colDocsSearchFolder
            // 
            this.colDocsSearchFolder.HeaderText = "โฟลเดอร์";
            this.colDocsSearchFolder.MinimumWidth = 6;
            this.colDocsSearchFolder.Name = "colDocsSearchFolder";
            this.colDocsSearchFolder.ReadOnly = true;
            this.colDocsSearchFolder.Width = 150;
            // 
            // colDocsSearchSize
            // 
            this.colDocsSearchSize.HeaderText = "ขนาด";
            this.colDocsSearchSize.MinimumWidth = 6;
            this.colDocsSearchSize.Name = "colDocsSearchSize";
            this.colDocsSearchSize.ReadOnly = true;
            this.colDocsSearchSize.Width = 80;
            // 
            // lblDocsSearch
            // 
            this.lblDocsSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblDocsSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDocsSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDocsSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblDocsSearch.Location = new System.Drawing.Point(4, 250);
            this.lblDocsSearch.Name = "lblDocsSearch";
            this.lblDocsSearch.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblDocsSearch.Size = new System.Drawing.Size(511, 24);
            this.lblDocsSearch.TabIndex = 4;
            this.lblDocsSearch.Text = "ผลการค้นหา — ดับเบิลคลิกเพื่อเพิ่ม";
            // 
            // pnlDocsSearch
            // 
            this.pnlDocsSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlDocsSearch.Controls.Add(this.btnSearchDocs);
            this.pnlDocsSearch.Controls.Add(this.cboDocsStatus);
            this.pnlDocsSearch.Controls.Add(this.cboDocsFolder);
            this.pnlDocsSearch.Controls.Add(this.cboDocsType);
            this.pnlDocsSearch.Controls.Add(this.txtSearchDocs);
            this.pnlDocsSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDocsSearch.Location = new System.Drawing.Point(4, 214);
            this.pnlDocsSearch.Name = "pnlDocsSearch";
            this.pnlDocsSearch.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlDocsSearch.Size = new System.Drawing.Size(511, 36);
            this.pnlDocsSearch.TabIndex = 3;
            // 
            // btnSearchDocs
            // 
            this.btnSearchDocs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnSearchDocs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchDocs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchDocs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSearchDocs.ForeColor = System.Drawing.Color.White;
            this.btnSearchDocs.Location = new System.Drawing.Point(682, 6);
            this.btnSearchDocs.Name = "btnSearchDocs";
            this.btnSearchDocs.Size = new System.Drawing.Size(80, 24);
            this.btnSearchDocs.TabIndex = 4;
            this.btnSearchDocs.Text = "🔎 ค้นหา";
            this.btnSearchDocs.UseVisualStyleBackColor = false;
            this.btnSearchDocs.Click += new System.EventHandler(this.BtnSearchDocs_Click);
            // 
            // cboDocsStatus
            // 
            this.cboDocsStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDocsStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboDocsStatus.FormattingEnabled = true;
            this.cboDocsStatus.Location = new System.Drawing.Point(546, 6);
            this.cboDocsStatus.Name = "cboDocsStatus";
            this.cboDocsStatus.Size = new System.Drawing.Size(130, 28);
            this.cboDocsStatus.TabIndex = 3;
            // 
            // cboDocsFolder
            // 
            this.cboDocsFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDocsFolder.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboDocsFolder.FormattingEnabled = true;
            this.cboDocsFolder.Location = new System.Drawing.Point(420, 6);
            this.cboDocsFolder.Name = "cboDocsFolder";
            this.cboDocsFolder.Size = new System.Drawing.Size(120, 28);
            this.cboDocsFolder.TabIndex = 2;
            // 
            // cboDocsType
            // 
            this.cboDocsType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDocsType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboDocsType.FormattingEnabled = true;
            this.cboDocsType.Location = new System.Drawing.Point(314, 6);
            this.cboDocsType.Name = "cboDocsType";
            this.cboDocsType.Size = new System.Drawing.Size(100, 28);
            this.cboDocsType.TabIndex = 1;
            // 
            // txtSearchDocs
            // 
            this.txtSearchDocs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearchDocs.ForeColor = System.Drawing.Color.Gray;
            this.txtSearchDocs.Location = new System.Drawing.Point(8, 6);
            this.txtSearchDocs.Name = "txtSearchDocs";
            this.txtSearchDocs.Size = new System.Drawing.Size(300, 27);
            this.txtSearchDocs.TabIndex = 0;
            this.txtSearchDocs.Text = "🔍 ค้นหาชื่อเอกสาร / รหัส...";
            this.txtSearchDocs.Enter += new System.EventHandler(this.TxtSearchDocs_Enter);
            this.txtSearchDocs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearchDocs_KeyDown);
            this.txtSearchDocs.Leave += new System.EventHandler(this.TxtSearchDocs_Leave);
            // 
            // dgvDocsLinked
            // 
            this.dgvDocsLinked.AllowUserToAddRows = false;
            this.dgvDocsLinked.AllowUserToDeleteRows = false;
            this.dgvDocsLinked.AllowUserToResizeRows = false;
            this.dgvDocsLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvDocsLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDocsLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDocsLinkedNum,
            this.colDocName,
            this.colDocsLinkedSize,
            this.colDocsLinkedDate,
            this.colDocsLinkedView,
            this.colDocsLinkedDelete});
            this.dgvDocsLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvDocsLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvDocsLinked.Name = "dgvDocsLinked";
            this.dgvDocsLinked.RowHeadersVisible = false;
            this.dgvDocsLinked.RowHeadersWidth = 51;
            this.dgvDocsLinked.RowTemplate.Height = 28;
            this.dgvDocsLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDocsLinked.Size = new System.Drawing.Size(511, 186);
            this.dgvDocsLinked.TabIndex = 1;
            this.dgvDocsLinked.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvDocsLinked_CellContentClick);
            // 
            // colDocsLinkedNum
            // 
            this.colDocsLinkedNum.HeaderText = "#";
            this.colDocsLinkedNum.MinimumWidth = 6;
            this.colDocsLinkedNum.Name = "colDocsLinkedNum";
            this.colDocsLinkedNum.ReadOnly = true;
            this.colDocsLinkedNum.Width = 40;
            // 
            // colDocName
            // 
            this.colDocName.HeaderText = "ชื่อเอกสาร";
            this.colDocName.MinimumWidth = 6;
            this.colDocName.Name = "colDocName";
            this.colDocName.ReadOnly = true;
            this.colDocName.Width = 250;
            // 
            // colDocsLinkedSize
            // 
            this.colDocsLinkedSize.HeaderText = "ขนาด";
            this.colDocsLinkedSize.MinimumWidth = 6;
            this.colDocsLinkedSize.Name = "colDocsLinkedSize";
            this.colDocsLinkedSize.ReadOnly = true;
            this.colDocsLinkedSize.Width = 80;
            // 
            // colDocsLinkedDate
            // 
            this.colDocsLinkedDate.HeaderText = "วันที่";
            this.colDocsLinkedDate.MinimumWidth = 6;
            this.colDocsLinkedDate.Name = "colDocsLinkedDate";
            this.colDocsLinkedDate.ReadOnly = true;
            this.colDocsLinkedDate.Width = 120;
            // 
            // colDocsLinkedView
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(95)))), ((int)(((byte)(154)))));
            this.colDocsLinkedView.DefaultCellStyle = dataGridViewCellStyle6;
            this.colDocsLinkedView.HeaderText = "ดู";
            this.colDocsLinkedView.MinimumWidth = 6;
            this.colDocsLinkedView.Name = "colDocsLinkedView";
            this.colDocsLinkedView.Text = "👁️ ดู";
            this.colDocsLinkedView.UseColumnTextForButtonValue = true;
            this.colDocsLinkedView.Width = 70;
            // 
            // colDocsLinkedDelete
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.colDocsLinkedDelete.DefaultCellStyle = dataGridViewCellStyle7;
            this.colDocsLinkedDelete.HeaderText = "ลบ";
            this.colDocsLinkedDelete.MinimumWidth = 6;
            this.colDocsLinkedDelete.Name = "colDocsLinkedDelete";
            this.colDocsLinkedDelete.Text = "🗑️ ลบ";
            this.colDocsLinkedDelete.UseColumnTextForButtonValue = true;
            this.colDocsLinkedDelete.Width = 70;
            // 
            // lblDocsLinked
            // 
            this.lblDocsLinked.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblDocsLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDocsLinked.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDocsLinked.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblDocsLinked.Location = new System.Drawing.Point(4, 4);
            this.lblDocsLinked.Name = "lblDocsLinked";
            this.lblDocsLinked.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblDocsLinked.Size = new System.Drawing.Size(511, 24);
            this.lblDocsLinked.TabIndex = 0;
            this.lblDocsLinked.Text = "เอกสารที่เชื่อมโยง (0 เอกสาร)";
            // 
            // tabDepartments
            // 
            this.tabDepartments.AutoScroll = true;
            this.tabDepartments.BackColor = System.Drawing.Color.White;
            this.tabDepartments.Controls.Add(this.dgvDepartmentsLinked);
            this.tabDepartments.Controls.Add(this.lblDepartmentsLinked);
            this.tabDepartments.Location = new System.Drawing.Point(4, 29);
            this.tabDepartments.Name = "tabDepartments";
            this.tabDepartments.Padding = new System.Windows.Forms.Padding(4);
            this.tabDepartments.Size = new System.Drawing.Size(519, 860);
            this.tabDepartments.TabIndex = 6;
            this.tabDepartments.Text = "🏢 ฝ่าย/แผนก ที่เกี่ยวข้อง";
            // 
            // dgvDepartmentsLinked
            // 
            this.dgvDepartmentsLinked.AllowUserToAddRows = false;
            this.dgvDepartmentsLinked.AllowUserToDeleteRows = false;
            this.dgvDepartmentsLinked.AllowUserToResizeRows = false;
            this.dgvDepartmentsLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvDepartmentsLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDepartmentsLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDeptNum,
            this.colDeptCode,
            this.colDeptName,
            this.colDeptType,
            this.colDeptDelete});
            this.dgvDepartmentsLinked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDepartmentsLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvDepartmentsLinked.Name = "dgvDepartmentsLinked";
            this.dgvDepartmentsLinked.RowHeadersVisible = false;
            this.dgvDepartmentsLinked.RowHeadersWidth = 51;
            this.dgvDepartmentsLinked.RowTemplate.Height = 28;
            this.dgvDepartmentsLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDepartmentsLinked.Size = new System.Drawing.Size(511, 828);
            this.dgvDepartmentsLinked.TabIndex = 1;
            this.dgvDepartmentsLinked.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvDepartmentsLinked_CellContentClick);
            // 
            // colDeptNum
            // 
            this.colDeptNum.HeaderText = "#";
            this.colDeptNum.MinimumWidth = 6;
            this.colDeptNum.Name = "colDeptNum";
            this.colDeptNum.ReadOnly = true;
            this.colDeptNum.Width = 40;
            // 
            // colDeptCode
            // 
            this.colDeptCode.HeaderText = "รหัส";
            this.colDeptCode.MinimumWidth = 6;
            this.colDeptCode.Name = "colDeptCode";
            this.colDeptCode.ReadOnly = true;
            this.colDeptCode.Width = 125;
            // 
            // colDeptName
            // 
            this.colDeptName.HeaderText = "ชื่อฝ่าย/แผนก";
            this.colDeptName.MinimumWidth = 6;
            this.colDeptName.Name = "colDeptName";
            this.colDeptName.ReadOnly = true;
            this.colDeptName.Width = 300;
            // 
            // colDeptType
            // 
            this.colDeptType.HeaderText = "ประเภท";
            this.colDeptType.MinimumWidth = 6;
            this.colDeptType.Name = "colDeptType";
            this.colDeptType.ReadOnly = true;
            this.colDeptType.Width = 120;
            // 
            // colDeptDelete
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle8.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.colDeptDelete.DefaultCellStyle = dataGridViewCellStyle8;
            this.colDeptDelete.HeaderText = "ลบ";
            this.colDeptDelete.MinimumWidth = 6;
            this.colDeptDelete.Name = "colDeptDelete";
            this.colDeptDelete.Text = "🗑️ ลบ";
            this.colDeptDelete.UseColumnTextForButtonValue = true;
            this.colDeptDelete.Width = 80;
            // 
            // lblDepartmentsLinked
            // 
            this.lblDepartmentsLinked.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblDepartmentsLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDepartmentsLinked.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDepartmentsLinked.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblDepartmentsLinked.Location = new System.Drawing.Point(4, 4);
            this.lblDepartmentsLinked.Name = "lblDepartmentsLinked";
            this.lblDepartmentsLinked.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblDepartmentsLinked.Size = new System.Drawing.Size(511, 24);
            this.lblDepartmentsLinked.TabIndex = 0;
            this.lblDepartmentsLinked.Text = "ฝ่าย/แผนก ที่เกี่ยวข้อง (0 รายการ)";
            // 
            // pnlMappingHeader
            // 
            this.pnlMappingHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(82)))), ((int)(((byte)(119)))));
            this.pnlMappingHeader.Controls.Add(this.btnCollapseMappingPanel);
            this.pnlMappingHeader.Controls.Add(this.lblMappingHeader);
            this.pnlMappingHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMappingHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlMappingHeader.Name = "pnlMappingHeader";
            this.pnlMappingHeader.Size = new System.Drawing.Size(527, 28);
            this.pnlMappingHeader.TabIndex = 0;
            // 
            // btnCollapseMappingPanel
            // 
            this.btnCollapseMappingPanel.BackColor = System.Drawing.Color.Transparent;
            this.btnCollapseMappingPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCollapseMappingPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCollapseMappingPanel.FlatAppearance.BorderSize = 0;
            this.btnCollapseMappingPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCollapseMappingPanel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCollapseMappingPanel.ForeColor = System.Drawing.Color.White;
            this.btnCollapseMappingPanel.Location = new System.Drawing.Point(0, 0);
            this.btnCollapseMappingPanel.Name = "btnCollapseMappingPanel";
            this.btnCollapseMappingPanel.Size = new System.Drawing.Size(36, 28);
            this.btnCollapseMappingPanel.TabIndex = 1;
            this.btnCollapseMappingPanel.Text = "▶";
            this.btnCollapseMappingPanel.UseVisualStyleBackColor = false;
            this.btnCollapseMappingPanel.Click += new System.EventHandler(this.btnCollapseMappingPanel_Click);
            // 
            // lblMappingHeader
            // 
            this.lblMappingHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMappingHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMappingHeader.ForeColor = System.Drawing.Color.White;
            this.lblMappingHeader.Location = new System.Drawing.Point(0, 0);
            this.lblMappingHeader.Name = "lblMappingHeader";
            this.lblMappingHeader.Padding = new System.Windows.Forms.Padding(35, 0, 0, 0);
            this.lblMappingHeader.Size = new System.Drawing.Size(527, 28);
            this.lblMappingHeader.TabIndex = 0;
            this.lblMappingHeader.Text = "Mapping รายการที่เกี่ยวข้อง";
            this.lblMappingHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // KBEditorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1733, 1061);
            this.Controls.Add(this.pnlScroll);
            this.Controls.Add(this.pnlStatusBar);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(860, 620);
            this.Name = "KBEditorDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "แก้ไขบทความ — Knowledge Base | KMS";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.KBEditorDialog_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.flpToolbarLeft.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.pnlStatusBar.PerformLayout();
            this.pnlScroll.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitTopBottom.Panel1.ResumeLayout(false);
            this.splitTopBottom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitTopBottom)).EndInit();
            this.splitTopBottom.ResumeLayout(false);
            this.pnlGenInfoOuter.ResumeLayout(false);
            this.tlpGenInfo.ResumeLayout(false);
            this.tlpGenInfo.PerformLayout();
            this.flpTags.ResumeLayout(false);
            this.flpTags.PerformLayout();
            this.pnlTagChipTemplate.ResumeLayout(false);
            this.pnlTagChipTemplate.PerformLayout();
            this.pnlGenInfoHeader.ResumeLayout(false);
            this.pnlContentOuter.ResumeLayout(false);
            this.pnlContentOuter.PerformLayout();
            this.pnlRteStatus.ResumeLayout(false);
            this.tsRte.ResumeLayout(false);
            this.tsRte.PerformLayout();
            this.pnlSummaryArea.ResumeLayout(false);
            this.pnlSummaryArea.PerformLayout();
            this.pnlContentHeader.ResumeLayout(false);
            this.pnlMappingOuter.ResumeLayout(false);
            this.tabMapping.ResumeLayout(false);
            this.tabAttach.ResumeLayout(false);
            this.pnlAttachUpload.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttachLinked)).EndInit();
            this.tabRelated.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelatedSearch)).EndInit();
            this.pnlRelatedSearch.ResumeLayout(false);
            this.pnlRelatedSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelatedLinked)).EndInit();
            this.tabWorkflow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorkflowSearch)).EndInit();
            this.pnlWorkflowSearch.ResumeLayout(false);
            this.pnlWorkflowSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorkflowLinked)).EndInit();
            this.tabPrograms.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProgramsSearch)).EndInit();
            this.pnlProgramsSearch.ResumeLayout(false);
            this.pnlProgramsSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProgramsLinked)).EndInit();
            this.tabDocs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocsSearch)).EndInit();
            this.pnlDocsSearch.ResumeLayout(false);
            this.pnlDocsSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocsLinked)).EndInit();
            this.tabDepartments.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDepartmentsLinked)).EndInit();
            this.pnlMappingHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        // ── Top bar controls ──────────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlHeader;
        private System.Windows.Forms.Label       lblHeader;
        private System.Windows.Forms.Panel       pnlToolbar;
        private System.Windows.Forms.Panel       pnlToolbarBorder;
        private System.Windows.Forms.FlowLayoutPanel flpToolbarLeft;
        private System.Windows.Forms.Button      btnSaveDraft;
        private System.Windows.Forms.Button      btnSendReview;
        private System.Windows.Forms.Button      btnPreview;
        private System.Windows.Forms.Panel       pnlSep1;
        private System.Windows.Forms.Button      btnArchive;
        private System.Windows.Forms.Button      btnBack;
        private System.Windows.Forms.Button      btnCancel;
        private System.Windows.Forms.Label       lblBreadcrumb;

        // ── Status bar controls ───────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlStatusBar;
        private System.Windows.Forms.Label       lblVerInfo;
        private System.Windows.Forms.Label       lblStatusInfo;
        private System.Windows.Forms.Label       lblAuthorInfo;
        private System.Windows.Forms.Label       lblDateInfo;
        private System.Windows.Forms.Label       lblChangeDescLabel;
        private System.Windows.Forms.TextBox     txtChangeDesc;

        // ── Scroll area ───────────────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlScroll;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitTopBottom;


        // ── General Info section ──────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlGenInfoOuter;
        private System.Windows.Forms.Panel       pnlGenInfoHeader;
        private System.Windows.Forms.Button      btnCollapseGenInfo;
        private System.Windows.Forms.Label       lblGenInfoHeader;

        // ── Content section ───────────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlContentOuter;
        private System.Windows.Forms.Panel       pnlSummaryArea;
        private System.Windows.Forms.Label       lblSummary;
        private System.Windows.Forms.TextBox     txtSummary;
        private System.Windows.Forms.ToolStrip   tsRte;
        private System.Windows.Forms.ToolStripComboBox  tsRteFontFamily;
        private System.Windows.Forms.ToolStripSeparator tsRteSep0;
        private System.Windows.Forms.ToolStripComboBox  tsRteFontSize;
        private System.Windows.Forms.ToolStripButton    tsRteFontSizeDec;
        private System.Windows.Forms.ToolStripButton    tsRteFontSizeInc;
        private System.Windows.Forms.ToolStripSeparator tsRteSep1;
        private System.Windows.Forms.ToolStripButton    tsRteBold;
        private System.Windows.Forms.ToolStripButton    tsRteItalic;
        private System.Windows.Forms.ToolStripButton    tsRteUnder;
        private System.Windows.Forms.ToolStripSeparator tsRteSep3;
        private System.Windows.Forms.ToolStripButton    tsRteAlignLeft;
        private System.Windows.Forms.ToolStripButton    tsRteAlignCenter;
        private System.Windows.Forms.ToolStripButton    tsRteAlignRight;
        private System.Windows.Forms.ToolStripSeparator tsRteSep4;
        private System.Windows.Forms.ToolStripButton    tsRteBullet;
        private System.Windows.Forms.ToolStripButton    tsRteNumList;
        private System.Windows.Forms.ToolStripSeparator tsRteSep5;
        private System.Windows.Forms.ToolStripButton    tsRteLink;
        private System.Windows.Forms.ToolStripButton    tsRteTable;
        private System.Windows.Forms.ToolStripButton    tsRteCode;
        private System.Windows.Forms.ToolStripButton    tsRteImage;
        private System.Windows.Forms.ToolStripButton    tsRteAttach;
        private System.Windows.Forms.Label       lblContent;
        private System.Windows.Forms.WebBrowser  webEditor;
        private System.Windows.Forms.Panel       pnlRteStatus;
        private System.Windows.Forms.Label       lblRteStatus;

        // ── Mapping section ───────────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlMappingOuter;
        private System.Windows.Forms.Panel       pnlMappingHeader;
        private System.Windows.Forms.Button      btnCollapseMappingPanel;
        private System.Windows.Forms.TabControl  tabMapping;
        private System.Windows.Forms.TabPage     tabAttach;
        // tabAttach - Attached Files Section
        private System.Windows.Forms.Label lblAttachLinked;
        private System.Windows.Forms.DataGridView dgvAttachLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAttachNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAttachFilename;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAttachSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAttachDate;
        private System.Windows.Forms.DataGridViewButtonColumn colAttachView;
        private System.Windows.Forms.DataGridViewButtonColumn colAttachDelete;
        // tabAttach - Upload Section
        private System.Windows.Forms.Panel pnlAttachUpload;
        private System.Windows.Forms.Button btnUploadFile;
        private System.Windows.Forms.Button btnSelectFromDMS;
        private System.Windows.Forms.TabPage     tabRelated;
        // tabRelated - Linked Articles Section
        private System.Windows.Forms.Label lblRelatedLinked;
        private System.Windows.Forms.DataGridView dgvRelatedLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedLinkedCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedLinkedTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedLinkedType;
        // tabRelated - Search Section
        private System.Windows.Forms.Panel pnlRelatedSearch;
        private System.Windows.Forms.TextBox txtSearchRelated;
        private System.Windows.Forms.ComboBox cboRelatedType;
        private System.Windows.Forms.ComboBox cboRelatedCategory;
        private System.Windows.Forms.ComboBox cboRelatedStatus;
        private System.Windows.Forms.Button btnSearchRelated;
        private System.Windows.Forms.Label lblRelatedSearch;
        private System.Windows.Forms.DataGridView dgvRelatedSearch;
        private System.Windows.Forms.TabPage     tabDepartments;
        // tabDepartments - Linked Departments Section
        private System.Windows.Forms.Label lblDepartmentsLinked;
        private System.Windows.Forms.DataGridView dgvDepartmentsLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptType;
        private System.Windows.Forms.DataGridViewButtonColumn colDeptDelete;
        private System.Windows.Forms.TabPage     tabWorkflow;
        // tabWorkflow - Linked Workflows Section
        private System.Windows.Forms.Label lblWorkflowLinked;
        private System.Windows.Forms.DataGridView dgvWorkflowLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorkflowLinkedNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorkflowLinkedCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorkflowLinkedName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorkflowLinkedStatus;
        private System.Windows.Forms.DataGridViewButtonColumn colWorkflowLinkedDelete;
        // tabWorkflow - Search Section
        private System.Windows.Forms.Panel pnlWorkflowSearch;
        private System.Windows.Forms.TextBox txtSearchWorkflow;
        private System.Windows.Forms.ComboBox cboWorkflowType;
        private System.Windows.Forms.ComboBox cboWorkflowCategory;
        private System.Windows.Forms.ComboBox cboWorkflowStatus;
        private System.Windows.Forms.Button btnSearchWorkflow;
        private System.Windows.Forms.Label lblWorkflowSearch;
        private System.Windows.Forms.DataGridView dgvWorkflowSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorkflowSearchCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorkflowSearchName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorkflowSearchType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorkflowSearchStatus;
        private System.Windows.Forms.TabPage     tabPrograms;
        // tabPrograms - Linked Programs Section
        private System.Windows.Forms.Label lblProgramsLinked;
        private System.Windows.Forms.DataGridView dgvProgramsLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgramsLinkedNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgramsLinkedCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgramsLinkedName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgramsLinkedType;
        private System.Windows.Forms.DataGridViewButtonColumn colProgramsLinkedDelete;
        // tabPrograms - Search Section
        private System.Windows.Forms.Panel pnlProgramsSearch;
        private System.Windows.Forms.TextBox txtSearchPrograms;
        private System.Windows.Forms.ComboBox cboProgramsType;
        private System.Windows.Forms.ComboBox cboProgramsCategory;
        private System.Windows.Forms.ComboBox cboProgramsStatus;
        private System.Windows.Forms.Button btnSearchPrograms;
        private System.Windows.Forms.Label lblProgramsSearch;
        private System.Windows.Forms.DataGridView dgvProgramsSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgramsSearchCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgramsSearchName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgramsSearchType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgramsSearchStatus;
        private System.Windows.Forms.TabPage     tabDocs;
        // tabDocs - Linked Documents Section
        private System.Windows.Forms.Label lblDocsLinked;
        private System.Windows.Forms.DataGridView dgvDocsLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocsLinkedNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocsLinkedName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocsLinkedSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocsLinkedDate;
        private System.Windows.Forms.DataGridViewButtonColumn colDocsLinkedView;
        private System.Windows.Forms.DataGridViewButtonColumn colDocsLinkedDelete;
        // tabDocs - Search Section
        private System.Windows.Forms.Panel pnlDocsSearch;
        private System.Windows.Forms.TextBox txtSearchDocs;
        private System.Windows.Forms.ComboBox cboDocsType;
        private System.Windows.Forms.ComboBox cboDocsFolder;
        private System.Windows.Forms.ComboBox cboDocsStatus;
        private System.Windows.Forms.Button btnSearchDocs;
        private System.Windows.Forms.Label lblDocsSearch;
        private System.Windows.Forms.DataGridView dgvDocsSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocsSearchCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocsSearchTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocsSearchFolder;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocsSearchSize;
        private System.Windows.Forms.Panel pnlContentHeader;
        private System.Windows.Forms.Label lblContentHeader;
        private System.Windows.Forms.Label lblMappingHeader;
        private System.Windows.Forms.TableLayoutPanel tlpGenInfo;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblVerLabel;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.ComboBox cboStatus;
        private System.Windows.Forms.Label lblCurVersion;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.Label lblCraf;
        private System.Windows.Forms.Label lblViewCount;
        private System.Windows.Forms.ComboBox cboCraf;
        private System.Windows.Forms.Label lblExpiry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFilename;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
        private System.Windows.Forms.DataGridViewButtonColumn colView;
        private System.Windows.Forms.DataGridViewButtonColumn colDelete;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSearchCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSearchName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSearchType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSearchStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedSearchCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedSearchTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedSummary;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedSearchType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedSearchViews;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedLinkedNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedLinkedSummary;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewButtonColumn colRelatedLinkedDelete;
        private System.Windows.Forms.FlowLayoutPanel flpTags;
        private System.Windows.Forms.Panel pnlTagChipTemplate;
        private System.Windows.Forms.Label lblTagChipTemplate;
        private System.Windows.Forms.Button btnTagChipRemoveTemplate;
        private System.Windows.Forms.TextBox txtTagInput;
        private System.Windows.Forms.DateTimePicker dtpExpiry;
        private System.Windows.Forms.Label lblLikeCount;
    }
}
