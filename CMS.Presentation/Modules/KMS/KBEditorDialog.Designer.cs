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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.lblBreadcrumb = new System.Windows.Forms.Label();
            this.flpToolbarLeft = new System.Windows.Forms.FlowLayoutPanel();
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
            this.pnlGap2 = new System.Windows.Forms.Panel();
            this.pnlMappingOuter = new System.Windows.Forms.Panel();
            this.tabMapping = new System.Windows.Forms.TabControl();
            this.tabAttach = new System.Windows.Forms.TabPage();
            this.pnlDropZone = new System.Windows.Forms.Panel();
            this.lblAttachUpload = new System.Windows.Forms.Label();
            this.pnlAttachUpload = new System.Windows.Forms.Panel();
            this.btnSelectFromDMS = new System.Windows.Forms.Button();
            this.btnUploadFile = new System.Windows.Forms.Button();
            this.pnlAttachLinkedActions = new System.Windows.Forms.Panel();
            this.btnViewAttach = new System.Windows.Forms.Button();
            this.btnRemoveAttach = new System.Windows.Forms.Button();
            this.lstAttachLinked = new System.Windows.Forms.ListView();
            this.colAttachLinkedNum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAttachLinkedFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAttachLinkedSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAttachLinkedDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblAttachLinked = new System.Windows.Forms.Label();
            this.tabRelated = new System.Windows.Forms.TabPage();
            this.lstRelatedSearch = new System.Windows.Forms.ListView();
            this.colRelSearchCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRelSearchTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRelSearchType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRelSearchViews = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblRelatedSearch = new System.Windows.Forms.Label();
            this.pnlRelatedSearch = new System.Windows.Forms.Panel();
            this.btnSearchRelated = new System.Windows.Forms.Button();
            this.cboRelatedStatus = new System.Windows.Forms.ComboBox();
            this.cboRelatedCategory = new System.Windows.Forms.ComboBox();
            this.cboRelatedType = new System.Windows.Forms.ComboBox();
            this.txtSearchRelated = new System.Windows.Forms.TextBox();
            this.pnlRelatedLinkedActions = new System.Windows.Forms.Panel();
            this.btnRemoveRelatedNew = new System.Windows.Forms.Button();
            this.lstRelatedLinked = new System.Windows.Forms.ListView();
            this.colRelLinkedNum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRelLinkedCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRelLinkedTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRelLinkedType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblRelatedLinked = new System.Windows.Forms.Label();
            this.tabWorkflow = new System.Windows.Forms.TabPage();
            this.lstWorkflowAvailable = new System.Windows.Forms.ListView();
            this.colWorkflowAvailCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWorkflowAvailName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWorkflowAvailStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblWorkflowAvailable = new System.Windows.Forms.Label();
            this.pnlWorkflowLinkedActions = new System.Windows.Forms.Panel();
            this.btnStartWorkflow = new System.Windows.Forms.Button();
            this.btnViewWorkflowDiagram = new System.Windows.Forms.Button();
            this.btnRemoveWorkflow = new System.Windows.Forms.Button();
            this.lstWorkflowLinked = new System.Windows.Forms.ListView();
            this.colWorkflowLinkedNum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWorkflowLinkedCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWorkflowLinkedName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWorkflowLinkedStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblWorkflowLinked = new System.Windows.Forms.Label();
            this.tabPrograms = new System.Windows.Forms.TabPage();
            this.lstProgramsAvailable = new System.Windows.Forms.ListView();
            this.colProgramsAvailCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProgramsAvailName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProgramsAvailType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblProgramsAvailable = new System.Windows.Forms.Label();
            this.pnlProgramsLinkedActions = new System.Windows.Forms.Panel();
            this.btnViewProgramDetails = new System.Windows.Forms.Button();
            this.btnRemoveProgram = new System.Windows.Forms.Button();
            this.lstProgramsLinked = new System.Windows.Forms.ListView();
            this.colProgramsLinkedNum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProgramsLinkedCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProgramsLinkedName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProgramsLinkedType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblProgramsLinked = new System.Windows.Forms.Label();
            this.tabDocs = new System.Windows.Forms.TabPage();
            this.pnlDocsDropZone = new System.Windows.Forms.Panel();
            this.lblDocsUpload = new System.Windows.Forms.Label();
            this.pnlDocsUpload = new System.Windows.Forms.Panel();
            this.btnSelectDocFromDMS = new System.Windows.Forms.Button();
            this.btnUploadDoc = new System.Windows.Forms.Button();
            this.pnlDocsLinkedActions = new System.Windows.Forms.Panel();
            this.btnViewDoc = new System.Windows.Forms.Button();
            this.btnRemoveDoc = new System.Windows.Forms.Button();
            this.lstDocsLinked = new System.Windows.Forms.ListView();
            this.colDocsLinkedNum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDocsLinkedFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDocsLinkedSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDocsLinkedDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblDocsLinked = new System.Windows.Forms.Label();
            this.tabVersions = new System.Windows.Forms.TabPage();
            this.lstVersions = new System.Windows.Forms.ListView();
            this.colVerNum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVerDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVerBy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVerNote = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlMappingHeader = new System.Windows.Forms.Panel();
            this.lblMappingHeader = new System.Windows.Forms.Label();
            this.pnlGap1 = new System.Windows.Forms.Panel();
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
            this.tsRteBold = new System.Windows.Forms.ToolStripButton();
            this.tsRteItalic = new System.Windows.Forms.ToolStripButton();
            this.tsRteUnder = new System.Windows.Forms.ToolStripButton();
            this.tsRteSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteH1 = new System.Windows.Forms.ToolStripButton();
            this.tsRteH2 = new System.Windows.Forms.ToolStripButton();
            this.tsRteH3 = new System.Windows.Forms.ToolStripButton();
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
            this.tsRteSep6 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteUndo = new System.Windows.Forms.ToolStripButton();
            this.tsRteRedo = new System.Windows.Forms.ToolStripButton();
            this.pnlSummaryArea = new System.Windows.Forms.Panel();
            this.txtSummary = new System.Windows.Forms.TextBox();
            this.lblSummary = new System.Windows.Forms.Label();
            this.pnlContentHeader = new System.Windows.Forms.Panel();
            this.lblContentHeader = new System.Windows.Forms.Label();
            this.pnlGenInfoOuter = new System.Windows.Forms.Panel();
            this.tlpGenInfo = new System.Windows.Forms.TableLayoutPanel();
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
            this.lblTags = new System.Windows.Forms.Label();
            this.lblExpiry = new System.Windows.Forms.Label();
            this.flpTags = new System.Windows.Forms.FlowLayoutPanel();
            this.txtTagInput = new System.Windows.Forms.TextBox();
            this.pnlExpiry = new System.Windows.Forms.Panel();
            this.chkNoExpiry = new System.Windows.Forms.CheckBox();
            this.dtpExpiry = new System.Windows.Forms.DateTimePicker();
            this.chkOfficialPolicy = new System.Windows.Forms.CheckBox();
            this.lblViewCount = new System.Windows.Forms.Label();
            this.lblLikeCount = new System.Windows.Forms.Label();
            this.pnlGenInfoHeader = new System.Windows.Forms.Panel();
            this.lblGenInfoHeader = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.flpToolbarLeft.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.pnlScroll.SuspendLayout();
            this.pnlMappingOuter.SuspendLayout();
            this.tabMapping.SuspendLayout();
            this.tabAttach.SuspendLayout();
            this.pnlAttachUpload.SuspendLayout();
            this.pnlAttachLinkedActions.SuspendLayout();
            this.tabRelated.SuspendLayout();
            this.pnlRelatedSearch.SuspendLayout();
            this.pnlRelatedLinkedActions.SuspendLayout();
            this.tabWorkflow.SuspendLayout();
            this.pnlWorkflowLinkedActions.SuspendLayout();
            this.tabPrograms.SuspendLayout();
            this.pnlProgramsLinkedActions.SuspendLayout();
            this.tabDocs.SuspendLayout();
            this.pnlDocsUpload.SuspendLayout();
            this.pnlDocsLinkedActions.SuspendLayout();
            this.tabVersions.SuspendLayout();
            this.pnlMappingHeader.SuspendLayout();
            this.pnlContentOuter.SuspendLayout();
            this.pnlRteStatus.SuspendLayout();
            this.tsRte.SuspendLayout();
            this.pnlSummaryArea.SuspendLayout();
            this.pnlContentHeader.SuspendLayout();
            this.pnlGenInfoOuter.SuspendLayout();
            this.tlpGenInfo.SuspendLayout();
            this.flpTags.SuspendLayout();
            this.pnlExpiry.SuspendLayout();
            this.pnlGenInfoHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1408, 48);
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
            this.lblHeader.Size = new System.Drawing.Size(1408, 48);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "✏️ แก้ไขบทความ — Knowledge Base";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
            this.pnlToolbar.Controls.Add(this.lblBreadcrumb);
            this.pnlToolbar.Controls.Add(this.flpToolbarLeft);
            this.pnlToolbar.Controls.Add(this.pnlToolbarBorder);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 48);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1408, 42);
            this.pnlToolbar.TabIndex = 1;
            // 
            // lblBreadcrumb
            // 
            this.lblBreadcrumb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBreadcrumb.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(160)))));
            this.lblBreadcrumb.Location = new System.Drawing.Point(1068, 12);
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
            this.flpToolbarLeft.Size = new System.Drawing.Size(563, 40);
            this.flpToolbarLeft.TabIndex = 0;
            this.flpToolbarLeft.WrapContents = false;
            // 
            // btnSaveDraft
            // 
            this.btnSaveDraft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnSaveDraft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveDraft.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(95)))), ((int)(((byte)(154)))));
            this.btnSaveDraft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveDraft.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveDraft.ForeColor = System.Drawing.Color.White;
            this.btnSaveDraft.Location = new System.Drawing.Point(8, 7);
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
            this.btnSendReview.Location = new System.Drawing.Point(128, 7);
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
            this.btnPreview.Location = new System.Drawing.Point(260, 7);
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
            this.pnlSep1.Location = new System.Drawing.Point(356, 9);
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
            this.btnArchive.Location = new System.Drawing.Point(365, 7);
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
            this.btnCancel.Location = new System.Drawing.Point(472, 10);
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
            this.pnlToolbarBorder.Size = new System.Drawing.Size(1408, 2);
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
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 924);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.pnlStatusBar.Size = new System.Drawing.Size(1408, 28);
            this.pnlStatusBar.TabIndex = 3;
            // 
            // txtChangeDesc
            // 
            this.txtChangeDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChangeDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(75)))), ((int)(((byte)(118)))));
            this.txtChangeDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtChangeDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.txtChangeDesc.ForeColor = System.Drawing.Color.White;
            this.txtChangeDesc.Location = new System.Drawing.Point(1032, 4);
            this.txtChangeDesc.Name = "txtChangeDesc";
            this.txtChangeDesc.Size = new System.Drawing.Size(366, 26);
            this.txtChangeDesc.TabIndex = 5;
            // 
            // lblChangeDescLabel
            // 
            this.lblChangeDescLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChangeDescLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblChangeDescLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(215)))), ((int)(((byte)(230)))));
            this.lblChangeDescLabel.Location = new System.Drawing.Point(956, 5);
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
            this.pnlScroll.Controls.Add(this.pnlGap2);
            this.pnlScroll.Controls.Add(this.pnlMappingOuter);
            this.pnlScroll.Controls.Add(this.pnlGap1);
            this.pnlScroll.Controls.Add(this.pnlContentOuter);
            this.pnlScroll.Controls.Add(this.pnlGenInfoOuter);
            this.pnlScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlScroll.Location = new System.Drawing.Point(0, 90);
            this.pnlScroll.Name = "pnlScroll";
            this.pnlScroll.Padding = new System.Windows.Forms.Padding(10);
            this.pnlScroll.Size = new System.Drawing.Size(1408, 834);
            this.pnlScroll.TabIndex = 2;
            // 
            // pnlGap2
            // 
            this.pnlGap2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlGap2.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGap2.Location = new System.Drawing.Point(10, 560);
            this.pnlGap2.Name = "pnlGap2";
            this.pnlGap2.Size = new System.Drawing.Size(1388, 8);
            this.pnlGap2.TabIndex = 13;
            // 
            // pnlMappingOuter
            // 
            this.pnlMappingOuter.AutoScroll = true;
            this.pnlMappingOuter.BackColor = System.Drawing.Color.White;
            this.pnlMappingOuter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlMappingOuter.Controls.Add(this.tabMapping);
            this.pnlMappingOuter.Controls.Add(this.pnlMappingHeader);
            this.pnlMappingOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMappingOuter.Location = new System.Drawing.Point(10, 560);
            this.pnlMappingOuter.Name = "pnlMappingOuter";
            this.pnlMappingOuter.Size = new System.Drawing.Size(1388, 264);
            this.pnlMappingOuter.TabIndex = 14;
            // 
            // tabMapping
            // 
            this.tabMapping.Controls.Add(this.tabAttach);
            this.tabMapping.Controls.Add(this.tabRelated);
            this.tabMapping.Controls.Add(this.tabWorkflow);
            this.tabMapping.Controls.Add(this.tabPrograms);
            this.tabMapping.Controls.Add(this.tabDocs);
            this.tabMapping.Controls.Add(this.tabVersions);
            this.tabMapping.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabMapping.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabMapping.Location = new System.Drawing.Point(0, 28);
            this.tabMapping.Name = "tabMapping";
            this.tabMapping.SelectedIndex = 0;
            this.tabMapping.Size = new System.Drawing.Size(1365, 500);
            this.tabMapping.TabIndex = 1;
            // 
            // tabAttach
            // 
            this.tabAttach.BackColor = System.Drawing.Color.White;
            this.tabAttach.Controls.Add(this.pnlDropZone);
            this.tabAttach.Controls.Add(this.lblAttachUpload);
            this.tabAttach.Controls.Add(this.pnlAttachUpload);
            this.tabAttach.Controls.Add(this.pnlAttachLinkedActions);
            this.tabAttach.Controls.Add(this.lstAttachLinked);
            this.tabAttach.Controls.Add(this.lblAttachLinked);
            this.tabAttach.Location = new System.Drawing.Point(4, 29);
            this.tabAttach.Name = "tabAttach";
            this.tabAttach.Padding = new System.Windows.Forms.Padding(4);
            this.tabAttach.Size = new System.Drawing.Size(1357, 467);
            this.tabAttach.TabIndex = 0;
            this.tabAttach.Text = "📎 ไฟล์ที่แนบ";
            // 
            // pnlDropZone
            // 
            this.pnlDropZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(253)))));
            this.pnlDropZone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDropZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDropZone.Location = new System.Drawing.Point(4, 244);
            this.pnlDropZone.Name = "pnlDropZone";
            this.pnlDropZone.Size = new System.Drawing.Size(1349, 219);
            this.pnlDropZone.TabIndex = 5;
            // 
            // lblAttachUpload
            // 
            this.lblAttachUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblAttachUpload.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAttachUpload.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAttachUpload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblAttachUpload.Location = new System.Drawing.Point(4, 220);
            this.lblAttachUpload.Name = "lblAttachUpload";
            this.lblAttachUpload.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblAttachUpload.Size = new System.Drawing.Size(1349, 24);
            this.lblAttachUpload.TabIndex = 4;
            this.lblAttachUpload.Text = "เพิ่มไฟล์ใหม่";
            // 
            // pnlAttachUpload
            // 
            this.pnlAttachUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlAttachUpload.Controls.Add(this.btnSelectFromDMS);
            this.pnlAttachUpload.Controls.Add(this.btnUploadFile);
            this.pnlAttachUpload.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAttachUpload.Location = new System.Drawing.Point(4, 184);
            this.pnlAttachUpload.Name = "pnlAttachUpload";
            this.pnlAttachUpload.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlAttachUpload.Size = new System.Drawing.Size(1349, 36);
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
            this.btnSelectFromDMS.Size = new System.Drawing.Size(150, 24);
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
            this.btnUploadFile.Size = new System.Drawing.Size(140, 24);
            this.btnUploadFile.TabIndex = 0;
            this.btnUploadFile.Text = "📤 อัพโหลดไฟล์";
            this.btnUploadFile.UseVisualStyleBackColor = false;
            this.btnUploadFile.Click += new System.EventHandler(this.BtnUploadFileNew_Click);
            // 
            // pnlAttachLinkedActions
            // 
            this.pnlAttachLinkedActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlAttachLinkedActions.Controls.Add(this.btnViewAttach);
            this.pnlAttachLinkedActions.Controls.Add(this.btnRemoveAttach);
            this.pnlAttachLinkedActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAttachLinkedActions.Location = new System.Drawing.Point(4, 148);
            this.pnlAttachLinkedActions.Name = "pnlAttachLinkedActions";
            this.pnlAttachLinkedActions.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlAttachLinkedActions.Size = new System.Drawing.Size(1349, 36);
            this.pnlAttachLinkedActions.TabIndex = 2;
            // 
            // btnViewAttach
            // 
            this.btnViewAttach.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnViewAttach.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewAttach.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewAttach.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewAttach.ForeColor = System.Drawing.Color.White;
            this.btnViewAttach.Location = new System.Drawing.Point(114, 6);
            this.btnViewAttach.Name = "btnViewAttach";
            this.btnViewAttach.Size = new System.Drawing.Size(100, 24);
            this.btnViewAttach.TabIndex = 1;
            this.btnViewAttach.Text = "👁️ ดูไฟล์";
            this.btnViewAttach.UseVisualStyleBackColor = false;
            this.btnViewAttach.Click += new System.EventHandler(this.BtnViewAttach_Click);
            // 
            // btnRemoveAttach
            // 
            this.btnRemoveAttach.BackColor = System.Drawing.Color.White;
            this.btnRemoveAttach.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveAttach.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveAttach.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRemoveAttach.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnRemoveAttach.Location = new System.Drawing.Point(8, 6);
            this.btnRemoveAttach.Name = "btnRemoveAttach";
            this.btnRemoveAttach.Size = new System.Drawing.Size(100, 24);
            this.btnRemoveAttach.TabIndex = 0;
            this.btnRemoveAttach.Text = "🗑️ ลบไฟล์";
            this.btnRemoveAttach.UseVisualStyleBackColor = false;
            this.btnRemoveAttach.Click += new System.EventHandler(this.BtnRemoveAttachNew_Click);
            // 
            // lstAttachLinked
            // 
            this.lstAttachLinked.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstAttachLinked.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colAttachLinkedNum,
            this.colAttachLinkedFile,
            this.colAttachLinkedSize,
            this.colAttachLinkedDate});
            this.lstAttachLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstAttachLinked.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstAttachLinked.FullRowSelect = true;
            this.lstAttachLinked.GridLines = true;
            this.lstAttachLinked.HideSelection = false;
            this.lstAttachLinked.Location = new System.Drawing.Point(4, 28);
            this.lstAttachLinked.Name = "lstAttachLinked";
            this.lstAttachLinked.Size = new System.Drawing.Size(1349, 120);
            this.lstAttachLinked.TabIndex = 1;
            this.lstAttachLinked.UseCompatibleStateImageBehavior = false;
            this.lstAttachLinked.View = System.Windows.Forms.View.Details;
            this.lstAttachLinked.DoubleClick += new System.EventHandler(this.LstAttachLinked_DoubleClick);
            // 
            // colAttachLinkedNum
            // 
            this.colAttachLinkedNum.Text = "#";
            this.colAttachLinkedNum.Width = 40;
            // 
            // colAttachLinkedFile
            // 
            this.colAttachLinkedFile.Text = "ชื่อไฟล์";
            this.colAttachLinkedFile.Width = 400;
            // 
            // colAttachLinkedSize
            // 
            this.colAttachLinkedSize.Text = "ขนาด";
            this.colAttachLinkedSize.Width = 100;
            // 
            // colAttachLinkedDate
            // 
            this.colAttachLinkedDate.Text = "วันที่แนบ";
            this.colAttachLinkedDate.Width = 140;
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
            this.lblAttachLinked.Size = new System.Drawing.Size(1349, 24);
            this.lblAttachLinked.TabIndex = 0;
            this.lblAttachLinked.Text = "ไฟล์ที่แนบ (0 ไฟล์)";
            // 
            // tabRelated
            // 
            this.tabRelated.BackColor = System.Drawing.Color.White;
            this.tabRelated.Controls.Add(this.lstRelatedSearch);
            this.tabRelated.Controls.Add(this.lblRelatedSearch);
            this.tabRelated.Controls.Add(this.pnlRelatedSearch);
            this.tabRelated.Controls.Add(this.pnlRelatedLinkedActions);
            this.tabRelated.Controls.Add(this.lstRelatedLinked);
            this.tabRelated.Controls.Add(this.lblRelatedLinked);
            this.tabRelated.Location = new System.Drawing.Point(4, 29);
            this.tabRelated.Name = "tabRelated";
            this.tabRelated.Padding = new System.Windows.Forms.Padding(4);
            this.tabRelated.Size = new System.Drawing.Size(1378, 467);
            this.tabRelated.TabIndex = 1;
            this.tabRelated.Text = "🔗 ความรู้ที่เกี่ยวข้อง";
            // 
            // lstRelatedSearch
            // 
            this.lstRelatedSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstRelatedSearch.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colRelSearchCode,
            this.colRelSearchTitle,
            this.colRelSearchType,
            this.colRelSearchViews});
            this.lstRelatedSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstRelatedSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstRelatedSearch.FullRowSelect = true;
            this.lstRelatedSearch.GridLines = true;
            this.lstRelatedSearch.HideSelection = false;
            this.lstRelatedSearch.Location = new System.Drawing.Point(4, 274);
            this.lstRelatedSearch.Name = "lstRelatedSearch";
            this.lstRelatedSearch.Size = new System.Drawing.Size(1370, 189);
            this.lstRelatedSearch.TabIndex = 5;
            this.lstRelatedSearch.UseCompatibleStateImageBehavior = false;
            this.lstRelatedSearch.View = System.Windows.Forms.View.Details;
            this.lstRelatedSearch.DoubleClick += new System.EventHandler(this.LstRelatedSearch_DoubleClick);
            // 
            // colRelSearchCode
            // 
            this.colRelSearchCode.Text = "รหัสบทความ";
            this.colRelSearchCode.Width = 120;
            // 
            // colRelSearchTitle
            // 
            this.colRelSearchTitle.Text = "ชื่อบทความ";
            this.colRelSearchTitle.Width = 500;
            // 
            // colRelSearchType
            // 
            this.colRelSearchType.Text = "ประเภท";
            this.colRelSearchType.Width = 100;
            // 
            // colRelSearchViews
            // 
            this.colRelSearchViews.Text = "ดู";
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
            this.lblRelatedSearch.Size = new System.Drawing.Size(1370, 24);
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
            this.pnlRelatedSearch.Size = new System.Drawing.Size(1370, 36);
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
            this.cboRelatedStatus.Items.AddRange(new object[] {
            "สถานะ: Published ▼",
            "Draft",
            "Review",
            "Published",
            "Archived"});
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
            this.cboRelatedCategory.Items.AddRange(new object[] {
            "หมวดหมู่ ▼"});
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
            this.cboRelatedType.Items.AddRange(new object[] {
            "ประเภท ▼",
            "SOP",
            "FAQ",
            "Guideline",
            "Template",
            "Policy"});
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
            this.txtSearchRelated.Enter += new System.EventHandler(this.TxtSearchRelatedNew_Enter);
            this.txtSearchRelated.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearchRelatedNew_KeyDown);
            this.txtSearchRelated.Leave += new System.EventHandler(this.TxtSearchRelatedNew_Leave);
            // 
            // pnlRelatedLinkedActions
            // 
            this.pnlRelatedLinkedActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlRelatedLinkedActions.Controls.Add(this.btnRemoveRelatedNew);
            this.pnlRelatedLinkedActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRelatedLinkedActions.Location = new System.Drawing.Point(4, 178);
            this.pnlRelatedLinkedActions.Name = "pnlRelatedLinkedActions";
            this.pnlRelatedLinkedActions.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlRelatedLinkedActions.Size = new System.Drawing.Size(1370, 36);
            this.pnlRelatedLinkedActions.TabIndex = 2;
            // 
            // btnRemoveRelatedNew
            // 
            this.btnRemoveRelatedNew.BackColor = System.Drawing.Color.White;
            this.btnRemoveRelatedNew.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveRelatedNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveRelatedNew.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRemoveRelatedNew.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnRemoveRelatedNew.Location = new System.Drawing.Point(8, 6);
            this.btnRemoveRelatedNew.Name = "btnRemoveRelatedNew";
            this.btnRemoveRelatedNew.Size = new System.Drawing.Size(100, 24);
            this.btnRemoveRelatedNew.TabIndex = 0;
            this.btnRemoveRelatedNew.Text = "🗑️ ลบรายการ";
            this.btnRemoveRelatedNew.UseVisualStyleBackColor = false;
            this.btnRemoveRelatedNew.Click += new System.EventHandler(this.BtnRemoveRelatedNew_Click);
            // 
            // lstRelatedLinked
            // 
            this.lstRelatedLinked.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstRelatedLinked.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colRelLinkedNum,
            this.colRelLinkedCode,
            this.colRelLinkedTitle,
            this.colRelLinkedType});
            this.lstRelatedLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstRelatedLinked.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstRelatedLinked.FullRowSelect = true;
            this.lstRelatedLinked.GridLines = true;
            this.lstRelatedLinked.HideSelection = false;
            this.lstRelatedLinked.Location = new System.Drawing.Point(4, 28);
            this.lstRelatedLinked.Name = "lstRelatedLinked";
            this.lstRelatedLinked.Size = new System.Drawing.Size(1370, 150);
            this.lstRelatedLinked.TabIndex = 1;
            this.lstRelatedLinked.UseCompatibleStateImageBehavior = false;
            this.lstRelatedLinked.View = System.Windows.Forms.View.Details;
            this.lstRelatedLinked.DoubleClick += new System.EventHandler(this.LstRelatedLinked_DoubleClick);
            // 
            // colRelLinkedNum
            // 
            this.colRelLinkedNum.Text = "#";
            this.colRelLinkedNum.Width = 40;
            // 
            // colRelLinkedCode
            // 
            this.colRelLinkedCode.Text = "รหัสบทความ";
            this.colRelLinkedCode.Width = 120;
            // 
            // colRelLinkedTitle
            // 
            this.colRelLinkedTitle.Text = "ชื่อบทความ";
            this.colRelLinkedTitle.Width = 500;
            // 
            // colRelLinkedType
            // 
            this.colRelLinkedType.Text = "ประเภท";
            this.colRelLinkedType.Width = 100;
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
            this.lblRelatedLinked.Size = new System.Drawing.Size(1370, 24);
            this.lblRelatedLinked.TabIndex = 0;
            this.lblRelatedLinked.Text = "รายการที่ Link อยู่แล้ว (0 รายการ)";
            // 
            // tabWorkflow
            // 
            this.tabWorkflow.BackColor = System.Drawing.Color.White;
            this.tabWorkflow.Controls.Add(this.lstWorkflowAvailable);
            this.tabWorkflow.Controls.Add(this.lblWorkflowAvailable);
            this.tabWorkflow.Controls.Add(this.pnlWorkflowLinkedActions);
            this.tabWorkflow.Controls.Add(this.lstWorkflowLinked);
            this.tabWorkflow.Controls.Add(this.lblWorkflowLinked);
            this.tabWorkflow.Location = new System.Drawing.Point(4, 29);
            this.tabWorkflow.Name = "tabWorkflow";
            this.tabWorkflow.Padding = new System.Windows.Forms.Padding(4);
            this.tabWorkflow.Size = new System.Drawing.Size(1378, 467);
            this.tabWorkflow.TabIndex = 2;
            this.tabWorkflow.Text = "⚖️ เส้นงาน (Workflow)";
            // 
            // lstWorkflowAvailable
            // 
            this.lstWorkflowAvailable.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstWorkflowAvailable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colWorkflowAvailCode,
            this.colWorkflowAvailName,
            this.colWorkflowAvailStatus});
            this.lstWorkflowAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstWorkflowAvailable.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstWorkflowAvailable.FullRowSelect = true;
            this.lstWorkflowAvailable.GridLines = true;
            this.lstWorkflowAvailable.HideSelection = false;
            this.lstWorkflowAvailable.Location = new System.Drawing.Point(4, 208);
            this.lstWorkflowAvailable.Name = "lstWorkflowAvailable";
            this.lstWorkflowAvailable.Size = new System.Drawing.Size(1370, 255);
            this.lstWorkflowAvailable.TabIndex = 4;
            this.lstWorkflowAvailable.UseCompatibleStateImageBehavior = false;
            this.lstWorkflowAvailable.View = System.Windows.Forms.View.Details;
            this.lstWorkflowAvailable.DoubleClick += new System.EventHandler(this.LstWorkflowAvailable_DoubleClick);
            // 
            // colWorkflowAvailCode
            // 
            this.colWorkflowAvailCode.Text = "รหัส Workflow";
            this.colWorkflowAvailCode.Width = 140;
            // 
            // colWorkflowAvailName
            // 
            this.colWorkflowAvailName.Text = "ชื่อ Workflow";
            this.colWorkflowAvailName.Width = 400;
            // 
            // colWorkflowAvailStatus
            // 
            this.colWorkflowAvailStatus.Text = "สถานะ";
            this.colWorkflowAvailStatus.Width = 120;
            // 
            // lblWorkflowAvailable
            // 
            this.lblWorkflowAvailable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblWorkflowAvailable.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWorkflowAvailable.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWorkflowAvailable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblWorkflowAvailable.Location = new System.Drawing.Point(4, 184);
            this.lblWorkflowAvailable.Name = "lblWorkflowAvailable";
            this.lblWorkflowAvailable.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblWorkflowAvailable.Size = new System.Drawing.Size(1370, 24);
            this.lblWorkflowAvailable.TabIndex = 3;
            this.lblWorkflowAvailable.Text = "Workflow ที่พร้อมใช้งาน — ดับเบิลคลิกเพื่อเพิ่ม";
            // 
            // pnlWorkflowLinkedActions
            // 
            this.pnlWorkflowLinkedActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlWorkflowLinkedActions.Controls.Add(this.btnStartWorkflow);
            this.pnlWorkflowLinkedActions.Controls.Add(this.btnViewWorkflowDiagram);
            this.pnlWorkflowLinkedActions.Controls.Add(this.btnRemoveWorkflow);
            this.pnlWorkflowLinkedActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlWorkflowLinkedActions.Location = new System.Drawing.Point(4, 148);
            this.pnlWorkflowLinkedActions.Name = "pnlWorkflowLinkedActions";
            this.pnlWorkflowLinkedActions.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlWorkflowLinkedActions.Size = new System.Drawing.Size(1370, 36);
            this.pnlWorkflowLinkedActions.TabIndex = 2;
            // 
            // btnStartWorkflow
            // 
            this.btnStartWorkflow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnStartWorkflow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartWorkflow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartWorkflow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnStartWorkflow.ForeColor = System.Drawing.Color.White;
            this.btnStartWorkflow.Location = new System.Drawing.Point(240, 6);
            this.btnStartWorkflow.Name = "btnStartWorkflow";
            this.btnStartWorkflow.Size = new System.Drawing.Size(140, 24);
            this.btnStartWorkflow.TabIndex = 2;
            this.btnStartWorkflow.Text = "▶️ เริ่ม Workflow";
            this.btnStartWorkflow.UseVisualStyleBackColor = false;
            this.btnStartWorkflow.Click += new System.EventHandler(this.BtnStartWorkflow_Click);
            // 
            // btnViewWorkflowDiagram
            // 
            this.btnViewWorkflowDiagram.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnViewWorkflowDiagram.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewWorkflowDiagram.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewWorkflowDiagram.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewWorkflowDiagram.ForeColor = System.Drawing.Color.White;
            this.btnViewWorkflowDiagram.Location = new System.Drawing.Point(114, 6);
            this.btnViewWorkflowDiagram.Name = "btnViewWorkflowDiagram";
            this.btnViewWorkflowDiagram.Size = new System.Drawing.Size(120, 24);
            this.btnViewWorkflowDiagram.TabIndex = 1;
            this.btnViewWorkflowDiagram.Text = "📊 ดู Diagram";
            this.btnViewWorkflowDiagram.UseVisualStyleBackColor = false;
            this.btnViewWorkflowDiagram.Click += new System.EventHandler(this.BtnViewWorkflowDiagram_Click);
            // 
            // btnRemoveWorkflow
            // 
            this.btnRemoveWorkflow.BackColor = System.Drawing.Color.White;
            this.btnRemoveWorkflow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveWorkflow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveWorkflow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRemoveWorkflow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnRemoveWorkflow.Location = new System.Drawing.Point(8, 6);
            this.btnRemoveWorkflow.Name = "btnRemoveWorkflow";
            this.btnRemoveWorkflow.Size = new System.Drawing.Size(100, 24);
            this.btnRemoveWorkflow.TabIndex = 0;
            this.btnRemoveWorkflow.Text = "🗑️ ลบ";
            this.btnRemoveWorkflow.UseVisualStyleBackColor = false;
            this.btnRemoveWorkflow.Click += new System.EventHandler(this.BtnRemoveWorkflowNew_Click);
            // 
            // lstWorkflowLinked
            // 
            this.lstWorkflowLinked.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstWorkflowLinked.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colWorkflowLinkedNum,
            this.colWorkflowLinkedCode,
            this.colWorkflowLinkedName,
            this.colWorkflowLinkedStatus});
            this.lstWorkflowLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstWorkflowLinked.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstWorkflowLinked.FullRowSelect = true;
            this.lstWorkflowLinked.GridLines = true;
            this.lstWorkflowLinked.HideSelection = false;
            this.lstWorkflowLinked.Location = new System.Drawing.Point(4, 28);
            this.lstWorkflowLinked.Name = "lstWorkflowLinked";
            this.lstWorkflowLinked.Size = new System.Drawing.Size(1370, 120);
            this.lstWorkflowLinked.TabIndex = 1;
            this.lstWorkflowLinked.UseCompatibleStateImageBehavior = false;
            this.lstWorkflowLinked.View = System.Windows.Forms.View.Details;
            this.lstWorkflowLinked.DoubleClick += new System.EventHandler(this.LstWorkflowLinked_DoubleClick);
            // 
            // colWorkflowLinkedNum
            // 
            this.colWorkflowLinkedNum.Text = "#";
            this.colWorkflowLinkedNum.Width = 40;
            // 
            // colWorkflowLinkedCode
            // 
            this.colWorkflowLinkedCode.Text = "รหัส Workflow";
            this.colWorkflowLinkedCode.Width = 140;
            // 
            // colWorkflowLinkedName
            // 
            this.colWorkflowLinkedName.Text = "ชื่อ Workflow";
            this.colWorkflowLinkedName.Width = 400;
            // 
            // colWorkflowLinkedStatus
            // 
            this.colWorkflowLinkedStatus.Text = "สถานะ";
            this.colWorkflowLinkedStatus.Width = 120;
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
            this.lblWorkflowLinked.Size = new System.Drawing.Size(1370, 24);
            this.lblWorkflowLinked.TabIndex = 0;
            this.lblWorkflowLinked.Text = "Workflow ที่เชื่อมโยง (0 รายการ)";
            // 
            // tabPrograms
            // 
            this.tabPrograms.BackColor = System.Drawing.Color.White;
            this.tabPrograms.Controls.Add(this.lstProgramsAvailable);
            this.tabPrograms.Controls.Add(this.lblProgramsAvailable);
            this.tabPrograms.Controls.Add(this.pnlProgramsLinkedActions);
            this.tabPrograms.Controls.Add(this.lstProgramsLinked);
            this.tabPrograms.Controls.Add(this.lblProgramsLinked);
            this.tabPrograms.Location = new System.Drawing.Point(4, 29);
            this.tabPrograms.Name = "tabPrograms";
            this.tabPrograms.Padding = new System.Windows.Forms.Padding(4);
            this.tabPrograms.Size = new System.Drawing.Size(1378, 467);
            this.tabPrograms.TabIndex = 3;
            this.tabPrograms.Text = "💻 โปรแกรมที่เกี่ยวข้อง";
            // 
            // lstProgramsAvailable
            // 
            this.lstProgramsAvailable.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstProgramsAvailable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colProgramsAvailCode,
            this.colProgramsAvailName,
            this.colProgramsAvailType});
            this.lstProgramsAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstProgramsAvailable.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstProgramsAvailable.FullRowSelect = true;
            this.lstProgramsAvailable.GridLines = true;
            this.lstProgramsAvailable.HideSelection = false;
            this.lstProgramsAvailable.Location = new System.Drawing.Point(4, 208);
            this.lstProgramsAvailable.Name = "lstProgramsAvailable";
            this.lstProgramsAvailable.Size = new System.Drawing.Size(1370, 255);
            this.lstProgramsAvailable.TabIndex = 4;
            this.lstProgramsAvailable.UseCompatibleStateImageBehavior = false;
            this.lstProgramsAvailable.View = System.Windows.Forms.View.Details;
            this.lstProgramsAvailable.DoubleClick += new System.EventHandler(this.LstProgramsAvailable_DoubleClick);
            // 
            // colProgramsAvailCode
            // 
            this.colProgramsAvailCode.Text = "รหัสโปรแกรม";
            this.colProgramsAvailCode.Width = 120;
            // 
            // colProgramsAvailName
            // 
            this.colProgramsAvailName.Text = "ชื่อโปรแกรม";
            this.colProgramsAvailName.Width = 400;
            // 
            // colProgramsAvailType
            // 
            this.colProgramsAvailType.Text = "ประเภท";
            this.colProgramsAvailType.Width = 150;
            // 
            // lblProgramsAvailable
            // 
            this.lblProgramsAvailable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblProgramsAvailable.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblProgramsAvailable.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProgramsAvailable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblProgramsAvailable.Location = new System.Drawing.Point(4, 184);
            this.lblProgramsAvailable.Name = "lblProgramsAvailable";
            this.lblProgramsAvailable.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblProgramsAvailable.Size = new System.Drawing.Size(1370, 24);
            this.lblProgramsAvailable.TabIndex = 3;
            this.lblProgramsAvailable.Text = "โปรแกรมที่พร้อมใช้งาน — ดับเบิลคลิกเพื่อเพิ่ม";
            // 
            // pnlProgramsLinkedActions
            // 
            this.pnlProgramsLinkedActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlProgramsLinkedActions.Controls.Add(this.btnViewProgramDetails);
            this.pnlProgramsLinkedActions.Controls.Add(this.btnRemoveProgram);
            this.pnlProgramsLinkedActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlProgramsLinkedActions.Location = new System.Drawing.Point(4, 148);
            this.pnlProgramsLinkedActions.Name = "pnlProgramsLinkedActions";
            this.pnlProgramsLinkedActions.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlProgramsLinkedActions.Size = new System.Drawing.Size(1370, 36);
            this.pnlProgramsLinkedActions.TabIndex = 2;
            // 
            // btnViewProgramDetails
            // 
            this.btnViewProgramDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnViewProgramDetails.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewProgramDetails.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewProgramDetails.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewProgramDetails.ForeColor = System.Drawing.Color.White;
            this.btnViewProgramDetails.Location = new System.Drawing.Point(114, 6);
            this.btnViewProgramDetails.Name = "btnViewProgramDetails";
            this.btnViewProgramDetails.Size = new System.Drawing.Size(120, 24);
            this.btnViewProgramDetails.TabIndex = 1;
            this.btnViewProgramDetails.Text = "📋 ดูรายละเอียด";
            this.btnViewProgramDetails.UseVisualStyleBackColor = false;
            this.btnViewProgramDetails.Click += new System.EventHandler(this.BtnViewProgramDetails_Click);
            // 
            // btnRemoveProgram
            // 
            this.btnRemoveProgram.BackColor = System.Drawing.Color.White;
            this.btnRemoveProgram.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveProgram.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveProgram.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRemoveProgram.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnRemoveProgram.Location = new System.Drawing.Point(8, 6);
            this.btnRemoveProgram.Name = "btnRemoveProgram";
            this.btnRemoveProgram.Size = new System.Drawing.Size(100, 24);
            this.btnRemoveProgram.TabIndex = 0;
            this.btnRemoveProgram.Text = "🗑️ ลบ";
            this.btnRemoveProgram.UseVisualStyleBackColor = false;
            this.btnRemoveProgram.Click += new System.EventHandler(this.BtnRemoveProgramNew_Click);
            // 
            // lstProgramsLinked
            // 
            this.lstProgramsLinked.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstProgramsLinked.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colProgramsLinkedNum,
            this.colProgramsLinkedCode,
            this.colProgramsLinkedName,
            this.colProgramsLinkedType});
            this.lstProgramsLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstProgramsLinked.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstProgramsLinked.FullRowSelect = true;
            this.lstProgramsLinked.GridLines = true;
            this.lstProgramsLinked.HideSelection = false;
            this.lstProgramsLinked.Location = new System.Drawing.Point(4, 28);
            this.lstProgramsLinked.Name = "lstProgramsLinked";
            this.lstProgramsLinked.Size = new System.Drawing.Size(1370, 120);
            this.lstProgramsLinked.TabIndex = 1;
            this.lstProgramsLinked.UseCompatibleStateImageBehavior = false;
            this.lstProgramsLinked.View = System.Windows.Forms.View.Details;
            this.lstProgramsLinked.DoubleClick += new System.EventHandler(this.LstProgramsLinked_DoubleClick);
            // 
            // colProgramsLinkedNum
            // 
            this.colProgramsLinkedNum.Text = "#";
            this.colProgramsLinkedNum.Width = 40;
            // 
            // colProgramsLinkedCode
            // 
            this.colProgramsLinkedCode.Text = "รหัสโปรแกรม";
            this.colProgramsLinkedCode.Width = 120;
            // 
            // colProgramsLinkedName
            // 
            this.colProgramsLinkedName.Text = "ชื่อโปรแกรม";
            this.colProgramsLinkedName.Width = 400;
            // 
            // colProgramsLinkedType
            // 
            this.colProgramsLinkedType.Text = "ประเภท";
            this.colProgramsLinkedType.Width = 150;
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
            this.lblProgramsLinked.Size = new System.Drawing.Size(1370, 24);
            this.lblProgramsLinked.TabIndex = 0;
            this.lblProgramsLinked.Text = "โปรแกรมที่เชื่อมโยง (0 รายการ)";
            // 
            // tabDocs
            // 
            this.tabDocs.BackColor = System.Drawing.Color.White;
            this.tabDocs.Controls.Add(this.pnlDocsDropZone);
            this.tabDocs.Controls.Add(this.lblDocsUpload);
            this.tabDocs.Controls.Add(this.pnlDocsUpload);
            this.tabDocs.Controls.Add(this.pnlDocsLinkedActions);
            this.tabDocs.Controls.Add(this.lstDocsLinked);
            this.tabDocs.Controls.Add(this.lblDocsLinked);
            this.tabDocs.Location = new System.Drawing.Point(4, 29);
            this.tabDocs.Name = "tabDocs";
            this.tabDocs.Padding = new System.Windows.Forms.Padding(4);
            this.tabDocs.Size = new System.Drawing.Size(1378, 467);
            this.tabDocs.TabIndex = 4;
            this.tabDocs.Text = "📋 เอกสารที่เกี่ยวข้อง";
            // 
            // pnlDocsDropZone
            // 
            this.pnlDocsDropZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(253)))));
            this.pnlDocsDropZone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDocsDropZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDocsDropZone.Location = new System.Drawing.Point(4, 244);
            this.pnlDocsDropZone.Name = "pnlDocsDropZone";
            this.pnlDocsDropZone.Size = new System.Drawing.Size(1370, 219);
            this.pnlDocsDropZone.TabIndex = 5;
            // 
            // lblDocsUpload
            // 
            this.lblDocsUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblDocsUpload.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDocsUpload.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDocsUpload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblDocsUpload.Location = new System.Drawing.Point(4, 220);
            this.lblDocsUpload.Name = "lblDocsUpload";
            this.lblDocsUpload.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblDocsUpload.Size = new System.Drawing.Size(1370, 24);
            this.lblDocsUpload.TabIndex = 4;
            this.lblDocsUpload.Text = "เพิ่มเอกสารใหม่";
            // 
            // pnlDocsUpload
            // 
            this.pnlDocsUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlDocsUpload.Controls.Add(this.btnSelectDocFromDMS);
            this.pnlDocsUpload.Controls.Add(this.btnUploadDoc);
            this.pnlDocsUpload.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDocsUpload.Location = new System.Drawing.Point(4, 184);
            this.pnlDocsUpload.Name = "pnlDocsUpload";
            this.pnlDocsUpload.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlDocsUpload.Size = new System.Drawing.Size(1370, 36);
            this.pnlDocsUpload.TabIndex = 3;
            // 
            // btnSelectDocFromDMS
            // 
            this.btnSelectDocFromDMS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnSelectDocFromDMS.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelectDocFromDMS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectDocFromDMS.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSelectDocFromDMS.ForeColor = System.Drawing.Color.White;
            this.btnSelectDocFromDMS.Location = new System.Drawing.Point(154, 6);
            this.btnSelectDocFromDMS.Name = "btnSelectDocFromDMS";
            this.btnSelectDocFromDMS.Size = new System.Drawing.Size(150, 24);
            this.btnSelectDocFromDMS.TabIndex = 1;
            this.btnSelectDocFromDMS.Text = "📁 เลือกจาก DMS";
            this.btnSelectDocFromDMS.UseVisualStyleBackColor = false;
            this.btnSelectDocFromDMS.Click += new System.EventHandler(this.BtnSelectDocFromDMSNew_Click);
            // 
            // btnUploadDoc
            // 
            this.btnUploadDoc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnUploadDoc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUploadDoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUploadDoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnUploadDoc.ForeColor = System.Drawing.Color.White;
            this.btnUploadDoc.Location = new System.Drawing.Point(8, 6);
            this.btnUploadDoc.Name = "btnUploadDoc";
            this.btnUploadDoc.Size = new System.Drawing.Size(140, 24);
            this.btnUploadDoc.TabIndex = 0;
            this.btnUploadDoc.Text = "📤 อัพโหลดเอกสาร";
            this.btnUploadDoc.UseVisualStyleBackColor = false;
            this.btnUploadDoc.Click += new System.EventHandler(this.BtnUploadDocNew_Click);
            // 
            // pnlDocsLinkedActions
            // 
            this.pnlDocsLinkedActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlDocsLinkedActions.Controls.Add(this.btnViewDoc);
            this.pnlDocsLinkedActions.Controls.Add(this.btnRemoveDoc);
            this.pnlDocsLinkedActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDocsLinkedActions.Location = new System.Drawing.Point(4, 148);
            this.pnlDocsLinkedActions.Name = "pnlDocsLinkedActions";
            this.pnlDocsLinkedActions.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlDocsLinkedActions.Size = new System.Drawing.Size(1370, 36);
            this.pnlDocsLinkedActions.TabIndex = 2;
            // 
            // btnViewDoc
            // 
            this.btnViewDoc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnViewDoc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewDoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewDoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewDoc.ForeColor = System.Drawing.Color.White;
            this.btnViewDoc.Location = new System.Drawing.Point(114, 6);
            this.btnViewDoc.Name = "btnViewDoc";
            this.btnViewDoc.Size = new System.Drawing.Size(100, 24);
            this.btnViewDoc.TabIndex = 1;
            this.btnViewDoc.Text = "👁️ ดูเอกสาร";
            this.btnViewDoc.UseVisualStyleBackColor = false;
            this.btnViewDoc.Click += new System.EventHandler(this.BtnViewDocNew_Click);
            // 
            // btnRemoveDoc
            // 
            this.btnRemoveDoc.BackColor = System.Drawing.Color.White;
            this.btnRemoveDoc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveDoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveDoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRemoveDoc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnRemoveDoc.Location = new System.Drawing.Point(8, 6);
            this.btnRemoveDoc.Name = "btnRemoveDoc";
            this.btnRemoveDoc.Size = new System.Drawing.Size(100, 24);
            this.btnRemoveDoc.TabIndex = 0;
            this.btnRemoveDoc.Text = "🗑️ ลบเอกสาร";
            this.btnRemoveDoc.UseVisualStyleBackColor = false;
            this.btnRemoveDoc.Click += new System.EventHandler(this.BtnRemoveDocNew_Click);
            // 
            // lstDocsLinked
            // 
            this.lstDocsLinked.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstDocsLinked.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDocsLinkedNum,
            this.colDocsLinkedFile,
            this.colDocsLinkedSize,
            this.colDocsLinkedDate});
            this.lstDocsLinked.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstDocsLinked.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstDocsLinked.FullRowSelect = true;
            this.lstDocsLinked.GridLines = true;
            this.lstDocsLinked.HideSelection = false;
            this.lstDocsLinked.Location = new System.Drawing.Point(4, 28);
            this.lstDocsLinked.Name = "lstDocsLinked";
            this.lstDocsLinked.Size = new System.Drawing.Size(1370, 120);
            this.lstDocsLinked.TabIndex = 1;
            this.lstDocsLinked.UseCompatibleStateImageBehavior = false;
            this.lstDocsLinked.View = System.Windows.Forms.View.Details;
            this.lstDocsLinked.DoubleClick += new System.EventHandler(this.LstDocsLinked_DoubleClick);
            // 
            // colDocsLinkedNum
            // 
            this.colDocsLinkedNum.Text = "#";
            this.colDocsLinkedNum.Width = 40;
            // 
            // colDocsLinkedFile
            // 
            this.colDocsLinkedFile.Text = "ชื่อเอกสาร";
            this.colDocsLinkedFile.Width = 400;
            // 
            // colDocsLinkedSize
            // 
            this.colDocsLinkedSize.Text = "ขนาด";
            this.colDocsLinkedSize.Width = 100;
            // 
            // colDocsLinkedDate
            // 
            this.colDocsLinkedDate.Text = "วันที่แนบ";
            this.colDocsLinkedDate.Width = 140;
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
            this.lblDocsLinked.Size = new System.Drawing.Size(1370, 24);
            this.lblDocsLinked.TabIndex = 0;
            this.lblDocsLinked.Text = "เอกสารที่แนบ (0 เอกสาร)";
            // 
            // tabVersions
            // 
            this.tabVersions.BackColor = System.Drawing.Color.White;
            this.tabVersions.Controls.Add(this.lstVersions);
            this.tabVersions.Location = new System.Drawing.Point(4, 29);
            this.tabVersions.Name = "tabVersions";
            this.tabVersions.Padding = new System.Windows.Forms.Padding(4);
            this.tabVersions.Size = new System.Drawing.Size(1378, 467);
            this.tabVersions.TabIndex = 5;
            this.tabVersions.Text = "🕐 ประวัติ Version";
            // 
            // lstVersions
            // 
            this.lstVersions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstVersions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colVerNum,
            this.colVerDate,
            this.colVerBy,
            this.colVerNote});
            this.lstVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstVersions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstVersions.FullRowSelect = true;
            this.lstVersions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstVersions.HideSelection = false;
            this.lstVersions.Location = new System.Drawing.Point(4, 4);
            this.lstVersions.Name = "lstVersions";
            this.lstVersions.Size = new System.Drawing.Size(1370, 459);
            this.lstVersions.TabIndex = 0;
            this.lstVersions.UseCompatibleStateImageBehavior = false;
            this.lstVersions.View = System.Windows.Forms.View.Details;
            // 
            // colVerNum
            // 
            this.colVerNum.Text = "Version";
            this.colVerNum.Width = 70;
            // 
            // colVerDate
            // 
            this.colVerDate.Text = "วันที่บันทึก";
            this.colVerDate.Width = 140;
            // 
            // colVerBy
            // 
            this.colVerBy.Text = "แก้ไขโดย";
            this.colVerBy.Width = 150;
            // 
            // colVerNote
            // 
            this.colVerNote.Text = "หมายเหตุ";
            this.colVerNote.Width = 320;
            // 
            // pnlMappingHeader
            // 
            this.pnlMappingHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(82)))), ((int)(((byte)(119)))));
            this.pnlMappingHeader.Controls.Add(this.lblMappingHeader);
            this.pnlMappingHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMappingHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlMappingHeader.Name = "pnlMappingHeader";
            this.pnlMappingHeader.Size = new System.Drawing.Size(1365, 28);
            this.pnlMappingHeader.TabIndex = 0;
            // 
            // lblMappingHeader
            // 
            this.lblMappingHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMappingHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMappingHeader.ForeColor = System.Drawing.Color.White;
            this.lblMappingHeader.Location = new System.Drawing.Point(0, 0);
            this.lblMappingHeader.Name = "lblMappingHeader";
            this.lblMappingHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblMappingHeader.Size = new System.Drawing.Size(1365, 28);
            this.lblMappingHeader.TabIndex = 0;
            this.lblMappingHeader.Text = "🔗  Mapping รายการที่เกี่ยวข้อง";
            this.lblMappingHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlGap1
            // 
            this.pnlGap1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlGap1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGap1.Location = new System.Drawing.Point(10, 552);
            this.pnlGap1.Name = "pnlGap1";
            this.pnlGap1.Size = new System.Drawing.Size(1388, 8);
            this.pnlGap1.TabIndex = 11;
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
            this.pnlContentOuter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlContentOuter.Location = new System.Drawing.Point(10, 236);
            this.pnlContentOuter.Name = "pnlContentOuter";
            this.pnlContentOuter.Size = new System.Drawing.Size(1388, 316);
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
            this.webEditor.Size = new System.Drawing.Size(1386, 177);
            this.webEditor.TabIndex = 5;
            this.webEditor.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebEditor_DocumentCompleted);
            // 
            // pnlRteStatus
            // 
            this.pnlRteStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlRteStatus.Controls.Add(this.lblRteStatus);
            this.pnlRteStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlRteStatus.Location = new System.Drawing.Point(0, 292);
            this.pnlRteStatus.Name = "pnlRteStatus";
            this.pnlRteStatus.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.pnlRteStatus.Size = new System.Drawing.Size(1386, 22);
            this.pnlRteStatus.TabIndex = 4;
            // 
            // lblRteStatus
            // 
            this.lblRteStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRteStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblRteStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblRteStatus.Location = new System.Drawing.Point(10, 0);
            this.lblRteStatus.Name = "lblRteStatus";
            this.lblRteStatus.Size = new System.Drawing.Size(1366, 22);
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
            this.lblContent.Size = new System.Drawing.Size(1386, 22);
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
            this.tsRteBold,
            this.tsRteItalic,
            this.tsRteUnder,
            this.tsRteSep2,
            this.tsRteH1,
            this.tsRteH2,
            this.tsRteH3,
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
            this.tsRteAttach,
            this.tsRteSep6,
            this.tsRteUndo,
            this.tsRteRedo});
            this.tsRte.Location = new System.Drawing.Point(0, 65);
            this.tsRte.Name = "tsRte";
            this.tsRte.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tsRte.Size = new System.Drawing.Size(1386, 28);
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
            "8pt",
            "10pt",
            "12pt",
            "14pt",
            "18pt",
            "24pt",
            "36pt"});
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
            // tsRteSep2
            // 
            this.tsRteSep2.Name = "tsRteSep2";
            this.tsRteSep2.Size = new System.Drawing.Size(6, 28);
            // 
            // tsRteH1
            // 
            this.tsRteH1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteH1.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.tsRteH1.Name = "tsRteH1";
            this.tsRteH1.Size = new System.Drawing.Size(33, 25);
            this.tsRteH1.Text = "H1";
            this.tsRteH1.ToolTipText = "หัวข้อหลัก (Heading 1)";
            this.tsRteH1.Click += new System.EventHandler(this.TsRteH1_Click);
            // 
            // tsRteH2
            // 
            this.tsRteH2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteH2.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.tsRteH2.Name = "tsRteH2";
            this.tsRteH2.Size = new System.Drawing.Size(33, 25);
            this.tsRteH2.Text = "H2";
            this.tsRteH2.ToolTipText = "หัวข้อรอง (Heading 2)";
            this.tsRteH2.Click += new System.EventHandler(this.TsRteH2_Click);
            // 
            // tsRteH3
            // 
            this.tsRteH3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteH3.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.tsRteH3.Name = "tsRteH3";
            this.tsRteH3.Size = new System.Drawing.Size(33, 25);
            this.tsRteH3.Text = "H3";
            this.tsRteH3.ToolTipText = "หัวข้อย่อย (Heading 3)";
            this.tsRteH3.Click += new System.EventHandler(this.TsRteH3_Click);
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
            // tsRteSep6
            // 
            this.tsRteSep6.Name = "tsRteSep6";
            this.tsRteSep6.Size = new System.Drawing.Size(6, 28);
            // 
            // tsRteUndo
            // 
            this.tsRteUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteUndo.Name = "tsRteUndo";
            this.tsRteUndo.Size = new System.Drawing.Size(29, 25);
            this.tsRteUndo.Text = "↶";
            this.tsRteUndo.ToolTipText = "Undo (Ctrl+Z)";
            this.tsRteUndo.Click += new System.EventHandler(this.TsRteUndo_Click);
            // 
            // tsRteRedo
            // 
            this.tsRteRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteRedo.Name = "tsRteRedo";
            this.tsRteRedo.Size = new System.Drawing.Size(29, 25);
            this.tsRteRedo.Text = "↷";
            this.tsRteRedo.ToolTipText = "Redo (Ctrl+Y)";
            this.tsRteRedo.Click += new System.EventHandler(this.TsRteRedo_Click);
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
            this.pnlSummaryArea.Size = new System.Drawing.Size(1386, 37);
            this.pnlSummaryArea.TabIndex = 1;
            // 
            // txtSummary
            // 
            this.txtSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSummary.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSummary.Location = new System.Drawing.Point(174, 4);
            this.txtSummary.MaxLength = 1000;
            this.txtSummary.Name = "txtSummary";
            this.txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSummary.Size = new System.Drawing.Size(1199, 27);
            this.txtSummary.TabIndex = 1;
            // 
            // lblSummary
            // 
            this.lblSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblSummary.Location = new System.Drawing.Point(10, 4);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(1366, 27);
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
            this.pnlContentHeader.Size = new System.Drawing.Size(1386, 28);
            this.pnlContentHeader.TabIndex = 0;
            // 
            // lblContentHeader
            // 
            this.lblContentHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblContentHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblContentHeader.ForeColor = System.Drawing.Color.White;
            this.lblContentHeader.Location = new System.Drawing.Point(0, 0);
            this.lblContentHeader.Name = "lblContentHeader";
            this.lblContentHeader.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblContentHeader.Size = new System.Drawing.Size(1386, 28);
            this.lblContentHeader.TabIndex = 0;
            this.lblContentHeader.Text = "📝  เนื้อหาบทความ";
            this.lblContentHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlGenInfoOuter
            // 
            this.pnlGenInfoOuter.BackColor = System.Drawing.Color.White;
            this.pnlGenInfoOuter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGenInfoOuter.Controls.Add(this.tlpGenInfo);
            this.pnlGenInfoOuter.Controls.Add(this.pnlGenInfoHeader);
            this.pnlGenInfoOuter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGenInfoOuter.Location = new System.Drawing.Point(10, 10);
            this.pnlGenInfoOuter.Name = "pnlGenInfoOuter";
            this.pnlGenInfoOuter.Size = new System.Drawing.Size(1388, 226);
            this.pnlGenInfoOuter.TabIndex = 10;
            // 
            // tlpGenInfo
            // 
            this.tlpGenInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(251)))), ((int)(((byte)(252)))));
            this.tlpGenInfo.ColumnCount = 4;
            this.tlpGenInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpGenInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpGenInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpGenInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpGenInfo.Controls.Add(this.lblCode, 0, 0);
            this.tlpGenInfo.Controls.Add(this.lblType, 1, 0);
            this.tlpGenInfo.Controls.Add(this.lblStatus, 2, 0);
            this.tlpGenInfo.Controls.Add(this.lblVerLabel, 3, 0);
            this.tlpGenInfo.Controls.Add(this.txtCode, 0, 1);
            this.tlpGenInfo.Controls.Add(this.cboType, 1, 1);
            this.tlpGenInfo.Controls.Add(this.cboStatus, 2, 1);
            this.tlpGenInfo.Controls.Add(this.lblCurVersion, 3, 1);
            this.tlpGenInfo.Controls.Add(this.lblTitle, 0, 3);
            this.tlpGenInfo.Controls.Add(this.lblCategory, 3, 3);
            this.tlpGenInfo.Controls.Add(this.txtTitle, 0, 4);
            this.tlpGenInfo.Controls.Add(this.cboCategory, 3, 4);
            this.tlpGenInfo.Controls.Add(this.lblTags, 0, 6);
            this.tlpGenInfo.Controls.Add(this.lblExpiry, 2, 6);
            this.tlpGenInfo.Controls.Add(this.flpTags, 0, 7);
            this.tlpGenInfo.Controls.Add(this.pnlExpiry, 2, 7);
            this.tlpGenInfo.Controls.Add(this.chkOfficialPolicy, 0, 9);
            this.tlpGenInfo.Controls.Add(this.lblViewCount, 2, 9);
            this.tlpGenInfo.Controls.Add(this.lblLikeCount, 3, 9);
            this.tlpGenInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGenInfo.Location = new System.Drawing.Point(0, 28);
            this.tlpGenInfo.Name = "tlpGenInfo";
            this.tlpGenInfo.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.tlpGenInfo.RowCount = 10;
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tlpGenInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpGenInfo.Size = new System.Drawing.Size(1386, 196);
            this.tlpGenInfo.TabIndex = 1;
            // 
            // lblCode
            // 
            this.lblCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCode.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblCode.Location = new System.Drawing.Point(12, 8);
            this.lblCode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(337, 18);
            this.lblCode.TabIndex = 0;
            this.lblCode.Text = "รหัสบทความ";
            // 
            // lblType
            // 
            this.lblType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblType.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblType.Location = new System.Drawing.Point(353, 8);
            this.lblType.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(337, 18);
            this.lblType.TabIndex = 1;
            this.lblType.Text = "ประเภทบทความ *";
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblStatus.Location = new System.Drawing.Point(694, 8);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(337, 18);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "สถานะ";
            // 
            // lblVerLabel
            // 
            this.lblVerLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVerLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblVerLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblVerLabel.Location = new System.Drawing.Point(1035, 8);
            this.lblVerLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblVerLabel.Name = "lblVerLabel";
            this.lblVerLabel.Size = new System.Drawing.Size(339, 18);
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
            this.txtCode.Size = new System.Drawing.Size(337, 26);
            this.txtCode.TabIndex = 10;
            // 
            // cboType
            // 
            this.cboType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cboType.Location = new System.Drawing.Point(353, 26);
            this.cboType.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(337, 27);
            this.cboType.TabIndex = 11;
            // 
            // cboStatus
            // 
            this.cboStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboStatus.FormattingEnabled = true;
            this.cboStatus.Items.AddRange(new object[] {
            "📝 Draft",
            "🔍 รอ Review",
            "✅ Published",
            "📦 Archived"});
            this.cboStatus.Location = new System.Drawing.Point(694, 26);
            this.cboStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(337, 28);
            this.cboStatus.TabIndex = 12;
            this.cboStatus.SelectedIndexChanged += new System.EventHandler(this.CboStatus_SelectedIndexChanged);
            // 
            // lblCurVersion
            // 
            this.lblCurVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.lblCurVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCurVersion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCurVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(104)))));
            this.lblCurVersion.Location = new System.Drawing.Point(1035, 26);
            this.lblCurVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCurVersion.Name = "lblCurVersion";
            this.lblCurVersion.Size = new System.Drawing.Size(339, 28);
            this.lblCurVersion.TabIndex = 13;
            this.lblCurVersion.Text = "v1.0";
            this.lblCurVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            this.tlpGenInfo.SetColumnSpan(this.lblTitle, 3);
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblTitle.Location = new System.Drawing.Point(12, 64);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1019, 18);
            this.lblTitle.TabIndex = 20;
            this.lblTitle.Text = "ชื่อบทความ *";
            // 
            // lblCategory
            // 
            this.lblCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblCategory.Location = new System.Drawing.Point(1035, 64);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(339, 18);
            this.lblCategory.TabIndex = 21;
            this.lblCategory.Text = "หมวดหมู่ *";
            // 
            // txtTitle
            // 
            this.tlpGenInfo.SetColumnSpan(this.txtTitle, 3);
            this.txtTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.txtTitle.Location = new System.Drawing.Point(12, 82);
            this.txtTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.txtTitle.MaxLength = 500;
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(1019, 30);
            this.txtTitle.TabIndex = 22;
            // 
            // cboCategory
            // 
            this.cboCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cboCategory.Location = new System.Drawing.Point(1035, 82);
            this.cboCategory.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(339, 27);
            this.cboCategory.TabIndex = 23;
            // 
            // lblTags
            // 
            this.tlpGenInfo.SetColumnSpan(this.lblTags, 2);
            this.lblTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTags.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTags.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblTags.Location = new System.Drawing.Point(12, 120);
            this.lblTags.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(678, 18);
            this.lblTags.TabIndex = 30;
            this.lblTags.Text = "แท็ก (Tags) — คั่นด้วยเครื่องหมาย ,";
            // 
            // lblExpiry
            // 
            this.tlpGenInfo.SetColumnSpan(this.lblExpiry, 2);
            this.lblExpiry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExpiry.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblExpiry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblExpiry.Location = new System.Drawing.Point(694, 120);
            this.lblExpiry.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblExpiry.Name = "lblExpiry";
            this.lblExpiry.Size = new System.Drawing.Size(680, 18);
            this.lblExpiry.TabIndex = 31;
            this.lblExpiry.Text = "วันหมดอายุ (Review Due)";
            // 
            // flpTags
            // 
            this.flpTags.AutoSize = true;
            this.flpTags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tlpGenInfo.SetColumnSpan(this.flpTags, 2);
            this.flpTags.Controls.Add(this.txtTagInput);
            this.flpTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpTags.Location = new System.Drawing.Point(12, 138);
            this.flpTags.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.flpTags.MinimumSize = new System.Drawing.Size(678, 28);
            this.flpTags.Name = "flpTags";
            this.flpTags.Padding = new System.Windows.Forms.Padding(2);
            this.flpTags.Size = new System.Drawing.Size(678, 28);
            this.flpTags.TabIndex = 32;
            // 
            // txtTagInput
            // 
            this.txtTagInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTagInput.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTagInput.Location = new System.Drawing.Point(5, 5);
            this.txtTagInput.Name = "txtTagInput";
            this.txtTagInput.Size = new System.Drawing.Size(150, 20);
            this.txtTagInput.TabIndex = 0;
            this.txtTagInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtTagInput_KeyDown);
            // 
            // pnlExpiry
            // 
            this.pnlExpiry.BackColor = System.Drawing.Color.Transparent;
            this.tlpGenInfo.SetColumnSpan(this.pnlExpiry, 2);
            this.pnlExpiry.Controls.Add(this.chkNoExpiry);
            this.pnlExpiry.Controls.Add(this.dtpExpiry);
            this.pnlExpiry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlExpiry.Location = new System.Drawing.Point(694, 138);
            this.pnlExpiry.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.pnlExpiry.Name = "pnlExpiry";
            this.pnlExpiry.Size = new System.Drawing.Size(680, 28);
            this.pnlExpiry.TabIndex = 33;
            // 
            // chkNoExpiry
            // 
            this.chkNoExpiry.AutoSize = true;
            this.chkNoExpiry.Checked = true;
            this.chkNoExpiry.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNoExpiry.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkNoExpiry.Location = new System.Drawing.Point(158, 4);
            this.chkNoExpiry.Name = "chkNoExpiry";
            this.chkNoExpiry.Size = new System.Drawing.Size(118, 24);
            this.chkNoExpiry.TabIndex = 1;
            this.chkNoExpiry.Text = "ไม่มีวันหมดอายุ";
            this.chkNoExpiry.CheckedChanged += new System.EventHandler(this.ChkNoExpiry_CheckedChanged);
            // 
            // dtpExpiry
            // 
            this.dtpExpiry.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.dtpExpiry.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpExpiry.Location = new System.Drawing.Point(0, 1);
            this.dtpExpiry.Name = "dtpExpiry";
            this.dtpExpiry.Size = new System.Drawing.Size(150, 26);
            this.dtpExpiry.TabIndex = 0;
            // 
            // chkOfficialPolicy
            // 
            this.chkOfficialPolicy.AutoSize = true;
            this.tlpGenInfo.SetColumnSpan(this.chkOfficialPolicy, 2);
            this.chkOfficialPolicy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkOfficialPolicy.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkOfficialPolicy.Location = new System.Drawing.Point(12, 176);
            this.chkOfficialPolicy.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.chkOfficialPolicy.Name = "chkOfficialPolicy";
            this.chkOfficialPolicy.Size = new System.Drawing.Size(678, 22);
            this.chkOfficialPolicy.TabIndex = 40;
            this.chkOfficialPolicy.Text = "🏛 Official Policy";
            // 
            // lblViewCount
            // 
            this.lblViewCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblViewCount.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblViewCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblViewCount.Location = new System.Drawing.Point(694, 176);
            this.lblViewCount.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblViewCount.Name = "lblViewCount";
            this.lblViewCount.Size = new System.Drawing.Size(337, 22);
            this.lblViewCount.TabIndex = 41;
            this.lblViewCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLikeCount
            // 
            this.lblLikeCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLikeCount.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblLikeCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblLikeCount.Location = new System.Drawing.Point(1035, 176);
            this.lblLikeCount.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblLikeCount.Name = "lblLikeCount";
            this.lblLikeCount.Size = new System.Drawing.Size(339, 22);
            this.lblLikeCount.TabIndex = 42;
            this.lblLikeCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlGenInfoHeader
            // 
            this.pnlGenInfoHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlGenInfoHeader.Controls.Add(this.lblGenInfoHeader);
            this.pnlGenInfoHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGenInfoHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlGenInfoHeader.Name = "pnlGenInfoHeader";
            this.pnlGenInfoHeader.Size = new System.Drawing.Size(1386, 28);
            this.pnlGenInfoHeader.TabIndex = 0;
            // 
            // lblGenInfoHeader
            // 
            this.lblGenInfoHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGenInfoHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblGenInfoHeader.ForeColor = System.Drawing.Color.White;
            this.lblGenInfoHeader.Location = new System.Drawing.Point(0, 0);
            this.lblGenInfoHeader.Name = "lblGenInfoHeader";
            this.lblGenInfoHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblGenInfoHeader.Size = new System.Drawing.Size(1386, 28);
            this.lblGenInfoHeader.TabIndex = 0;
            this.lblGenInfoHeader.Text = "📋  ข้อมูลทั่วไป (General Information)";
            this.lblGenInfoHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // KBEditorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1408, 952);
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
            this.pnlMappingOuter.ResumeLayout(false);
            this.tabMapping.ResumeLayout(false);
            this.tabAttach.ResumeLayout(false);
            this.pnlAttachUpload.ResumeLayout(false);
            this.pnlAttachLinkedActions.ResumeLayout(false);
            this.tabRelated.ResumeLayout(false);
            this.pnlRelatedSearch.ResumeLayout(false);
            this.pnlRelatedSearch.PerformLayout();
            this.pnlRelatedLinkedActions.ResumeLayout(false);
            this.tabWorkflow.ResumeLayout(false);
            this.pnlWorkflowLinkedActions.ResumeLayout(false);
            this.tabPrograms.ResumeLayout(false);
            this.pnlProgramsLinkedActions.ResumeLayout(false);
            this.tabDocs.ResumeLayout(false);
            this.pnlDocsUpload.ResumeLayout(false);
            this.pnlDocsLinkedActions.ResumeLayout(false);
            this.tabVersions.ResumeLayout(false);
            this.pnlMappingHeader.ResumeLayout(false);
            this.pnlContentOuter.ResumeLayout(false);
            this.pnlContentOuter.PerformLayout();
            this.pnlRteStatus.ResumeLayout(false);
            this.tsRte.ResumeLayout(false);
            this.tsRte.PerformLayout();
            this.pnlSummaryArea.ResumeLayout(false);
            this.pnlSummaryArea.PerformLayout();
            this.pnlContentHeader.ResumeLayout(false);
            this.pnlGenInfoOuter.ResumeLayout(false);
            this.tlpGenInfo.ResumeLayout(false);
            this.tlpGenInfo.PerformLayout();
            this.flpTags.ResumeLayout(false);
            this.flpTags.PerformLayout();
            this.pnlExpiry.ResumeLayout(false);
            this.pnlExpiry.PerformLayout();
            this.pnlGenInfoHeader.ResumeLayout(false);
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
        private System.Windows.Forms.Panel       pnlGap1;
        private System.Windows.Forms.Panel       pnlGap2;

        // ── General Info section ──────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlGenInfoOuter;
        private System.Windows.Forms.Panel       pnlGenInfoHeader;
        private System.Windows.Forms.Label       lblGenInfoHeader;
        private System.Windows.Forms.TableLayoutPanel tlpGenInfo;
        private System.Windows.Forms.Label       lblCode;
        private System.Windows.Forms.TextBox     txtCode;
        private System.Windows.Forms.Label       lblType;
        private System.Windows.Forms.ComboBox    cboType;
        private System.Windows.Forms.Label       lblStatus;
        private System.Windows.Forms.ComboBox    cboStatus;
        private System.Windows.Forms.Label       lblVerLabel;
        private System.Windows.Forms.Label       lblCurVersion;
        private System.Windows.Forms.Label       lblTitle;
        private System.Windows.Forms.TextBox     txtTitle;
        private System.Windows.Forms.Label       lblCategory;
        private System.Windows.Forms.ComboBox    cboCategory;
        private System.Windows.Forms.Label       lblTags;
        private System.Windows.Forms.FlowLayoutPanel flpTags;
        private System.Windows.Forms.TextBox     txtTagInput;
        private System.Windows.Forms.Label       lblExpiry;
        private System.Windows.Forms.Panel       pnlExpiry;
        private System.Windows.Forms.DateTimePicker dtpExpiry;
        private System.Windows.Forms.CheckBox    chkNoExpiry;
        private System.Windows.Forms.CheckBox    chkOfficialPolicy;
        private System.Windows.Forms.Label       lblViewCount;
        private System.Windows.Forms.Label       lblLikeCount;

        // ── Content section ───────────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlContentOuter;
        private System.Windows.Forms.Panel       pnlContentHeader;
        private System.Windows.Forms.Label       lblContentHeader;
        private System.Windows.Forms.Panel       pnlSummaryArea;
        private System.Windows.Forms.Label       lblSummary;
        private System.Windows.Forms.TextBox     txtSummary;
        private System.Windows.Forms.ToolStrip   tsRte;
        private System.Windows.Forms.ToolStripComboBox  tsRteFontFamily;
        private System.Windows.Forms.ToolStripSeparator tsRteSep0;
        private System.Windows.Forms.ToolStripComboBox  tsRteFontSize;
        private System.Windows.Forms.ToolStripSeparator tsRteSep1;
        private System.Windows.Forms.ToolStripButton    tsRteBold;
        private System.Windows.Forms.ToolStripButton    tsRteItalic;
        private System.Windows.Forms.ToolStripButton    tsRteUnder;
        private System.Windows.Forms.ToolStripSeparator tsRteSep2;
        private System.Windows.Forms.ToolStripButton    tsRteH1;
        private System.Windows.Forms.ToolStripButton    tsRteH2;
        private System.Windows.Forms.ToolStripButton    tsRteH3;
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
        private System.Windows.Forms.ToolStripSeparator tsRteSep6;
        private System.Windows.Forms.ToolStripButton    tsRteUndo;
        private System.Windows.Forms.ToolStripButton    tsRteRedo;
        private System.Windows.Forms.Label       lblContent;
        private System.Windows.Forms.WebBrowser  webEditor;
        private System.Windows.Forms.Panel       pnlRteStatus;
        private System.Windows.Forms.Label       lblRteStatus;

        // ── Mapping section ───────────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlMappingOuter;
        private System.Windows.Forms.Panel       pnlMappingHeader;
        private System.Windows.Forms.Label       lblMappingHeader;
        private System.Windows.Forms.TabControl  tabMapping;
        private System.Windows.Forms.TabPage     tabAttach;
        // tabAttach - Attached Files Section
        private System.Windows.Forms.Label lblAttachLinked;
        private System.Windows.Forms.ListView lstAttachLinked;
        private System.Windows.Forms.ColumnHeader colAttachLinkedNum;
        private System.Windows.Forms.ColumnHeader colAttachLinkedFile;
        private System.Windows.Forms.ColumnHeader colAttachLinkedSize;
        private System.Windows.Forms.ColumnHeader colAttachLinkedDate;
        private System.Windows.Forms.Panel pnlAttachLinkedActions;
        private System.Windows.Forms.Button btnRemoveAttach;
        private System.Windows.Forms.Button btnViewAttach;
        // tabAttach - Upload Section
        private System.Windows.Forms.Panel pnlAttachUpload;
        private System.Windows.Forms.Button btnUploadFile;
        private System.Windows.Forms.Button btnSelectFromDMS;
        private System.Windows.Forms.Label lblAttachUpload;
        private System.Windows.Forms.Panel pnlDropZone;
        private System.Windows.Forms.TabPage     tabRelated;
        // tabRelated - Linked Articles Section
        private System.Windows.Forms.Label lblRelatedLinked;
        private System.Windows.Forms.ListView lstRelatedLinked;
        private System.Windows.Forms.ColumnHeader colRelLinkedNum;
        private System.Windows.Forms.ColumnHeader colRelLinkedCode;
        private System.Windows.Forms.ColumnHeader colRelLinkedTitle;
        private System.Windows.Forms.ColumnHeader colRelLinkedType;
        private System.Windows.Forms.Panel pnlRelatedLinkedActions;
        private System.Windows.Forms.Button btnRemoveRelatedNew;
        // tabRelated - Search Section
        private System.Windows.Forms.Panel pnlRelatedSearch;
        private System.Windows.Forms.TextBox txtSearchRelated;
        private System.Windows.Forms.ComboBox cboRelatedType;
        private System.Windows.Forms.ComboBox cboRelatedCategory;
        private System.Windows.Forms.ComboBox cboRelatedStatus;
        private System.Windows.Forms.Button btnSearchRelated;
        private System.Windows.Forms.Label lblRelatedSearch;
        private System.Windows.Forms.ListView lstRelatedSearch;
        private System.Windows.Forms.ColumnHeader colRelSearchCode;
        private System.Windows.Forms.ColumnHeader colRelSearchTitle;
        private System.Windows.Forms.ColumnHeader colRelSearchType;
        private System.Windows.Forms.ColumnHeader colRelSearchViews;
        private System.Windows.Forms.TabPage     tabWorkflow;
        // tabWorkflow - Linked Workflows Section
        private System.Windows.Forms.Label lblWorkflowLinked;
        private System.Windows.Forms.ListView lstWorkflowLinked;
        private System.Windows.Forms.ColumnHeader colWorkflowLinkedNum;
        private System.Windows.Forms.ColumnHeader colWorkflowLinkedCode;
        private System.Windows.Forms.ColumnHeader colWorkflowLinkedName;
        private System.Windows.Forms.ColumnHeader colWorkflowLinkedStatus;
        private System.Windows.Forms.Panel pnlWorkflowLinkedActions;
        private System.Windows.Forms.Button btnRemoveWorkflow;
        private System.Windows.Forms.Button btnViewWorkflowDiagram;
        private System.Windows.Forms.Button btnStartWorkflow;
        // tabWorkflow - Available Workflows Section
        private System.Windows.Forms.Label lblWorkflowAvailable;
        private System.Windows.Forms.ListView lstWorkflowAvailable;
        private System.Windows.Forms.ColumnHeader colWorkflowAvailCode;
        private System.Windows.Forms.ColumnHeader colWorkflowAvailName;
        private System.Windows.Forms.ColumnHeader colWorkflowAvailStatus;
        private System.Windows.Forms.TabPage     tabPrograms;
        // tabPrograms - Linked Programs Section
        private System.Windows.Forms.Label lblProgramsLinked;
        private System.Windows.Forms.ListView lstProgramsLinked;
        private System.Windows.Forms.ColumnHeader colProgramsLinkedNum;
        private System.Windows.Forms.ColumnHeader colProgramsLinkedCode;
        private System.Windows.Forms.ColumnHeader colProgramsLinkedName;
        private System.Windows.Forms.ColumnHeader colProgramsLinkedType;
        private System.Windows.Forms.Panel pnlProgramsLinkedActions;
        private System.Windows.Forms.Button btnRemoveProgram;
        private System.Windows.Forms.Button btnViewProgramDetails;
        // tabPrograms - Available Programs Section
        private System.Windows.Forms.Label lblProgramsAvailable;
        private System.Windows.Forms.ListView lstProgramsAvailable;
        private System.Windows.Forms.ColumnHeader colProgramsAvailCode;
        private System.Windows.Forms.ColumnHeader colProgramsAvailName;
        private System.Windows.Forms.ColumnHeader colProgramsAvailType;
        private System.Windows.Forms.TabPage     tabDocs;
        // tabDocs - Linked Documents Section
        private System.Windows.Forms.Label lblDocsLinked;
        private System.Windows.Forms.ListView lstDocsLinked;
        private System.Windows.Forms.ColumnHeader colDocsLinkedNum;
        private System.Windows.Forms.ColumnHeader colDocsLinkedFile;
        private System.Windows.Forms.ColumnHeader colDocsLinkedSize;
        private System.Windows.Forms.ColumnHeader colDocsLinkedDate;
        private System.Windows.Forms.Panel pnlDocsLinkedActions;
        private System.Windows.Forms.Button btnRemoveDoc;
        private System.Windows.Forms.Button btnViewDoc;
        // tabDocs - Upload Section
        private System.Windows.Forms.Panel pnlDocsUpload;
        private System.Windows.Forms.Button btnUploadDoc;
        private System.Windows.Forms.Button btnSelectDocFromDMS;
        private System.Windows.Forms.Label lblDocsUpload;
        private System.Windows.Forms.Panel pnlDocsDropZone;
        private System.Windows.Forms.TabPage     tabVersions;
        private System.Windows.Forms.ListView    lstVersions;
        private System.Windows.Forms.ColumnHeader colVerNum;
        private System.Windows.Forms.ColumnHeader colVerDate;
        private System.Windows.Forms.ColumnHeader colVerBy;
        private System.Windows.Forms.ColumnHeader colVerNote;
    }
}
