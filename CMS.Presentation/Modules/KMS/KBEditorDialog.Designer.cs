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
            this.lstAttach = new System.Windows.Forms.ListView();
            this.colAttFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAttSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAttDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabRelated = new System.Windows.Forms.TabPage();
            this.lstRelated = new System.Windows.Forms.ListView();
            this.colRelCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRelTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRelType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabWorkflow = new System.Windows.Forms.TabPage();
            this.lstWorkflow = new System.Windows.Forms.ListView();
            this.colWfCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWfName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWfStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPrograms = new System.Windows.Forms.TabPage();
            this.lstPrograms = new System.Windows.Forms.ListView();
            this.colProgName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProgType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabDocs = new System.Windows.Forms.TabPage();
            this.lstDocs = new System.Windows.Forms.ListView();
            this.colDocName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDocStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.txtContent = new System.Windows.Forms.RichTextBox();
            this.pnlRteStatus = new System.Windows.Forms.Panel();
            this.lblRteStatus = new System.Windows.Forms.Label();
            this.lblContent = new System.Windows.Forms.Label();
            this.tsRte = new System.Windows.Forms.ToolStrip();
            this.tsRteBold = new System.Windows.Forms.ToolStripButton();
            this.tsRteItalic = new System.Windows.Forms.ToolStripButton();
            this.tsRteUnder = new System.Windows.Forms.ToolStripButton();
            this.tsRteSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteH1 = new System.Windows.Forms.ToolStripButton();
            this.tsRteH2 = new System.Windows.Forms.ToolStripButton();
            this.tsRteH3 = new System.Windows.Forms.ToolStripButton();
            this.tsRteSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteBullet = new System.Windows.Forms.ToolStripButton();
            this.tsRteNumList = new System.Windows.Forms.ToolStripButton();
            this.tsRteSep3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRteLink = new System.Windows.Forms.ToolStripButton();
            this.tsRteImage = new System.Windows.Forms.ToolStripButton();
            this.tsRteAttach = new System.Windows.Forms.ToolStripButton();
            this.tsRteSep4 = new System.Windows.Forms.ToolStripSeparator();
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
            this.lblStatusDisplay = new System.Windows.Forms.Label();
            this.lblCurVersion = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblCategory = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.lblTags = new System.Windows.Forms.Label();
            this.lblExpiry = new System.Windows.Forms.Label();
            this.txtTags = new System.Windows.Forms.TextBox();
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
            this.tabRelated.SuspendLayout();
            this.tabWorkflow.SuspendLayout();
            this.tabPrograms.SuspendLayout();
            this.tabDocs.SuspendLayout();
            this.tabVersions.SuspendLayout();
            this.pnlMappingHeader.SuspendLayout();
            this.pnlContentOuter.SuspendLayout();
            this.pnlRteStatus.SuspendLayout();
            this.tsRte.SuspendLayout();
            this.pnlSummaryArea.SuspendLayout();
            this.pnlContentHeader.SuspendLayout();
            this.pnlGenInfoOuter.SuspendLayout();
            this.tlpGenInfo.SuspendLayout();
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
            this.pnlGap2.Location = new System.Drawing.Point(10, 810);
            this.pnlGap2.Name = "pnlGap2";
            this.pnlGap2.Size = new System.Drawing.Size(1388, 8);
            this.pnlGap2.TabIndex = 13;
            // 
            // pnlMappingOuter
            // 
            this.pnlMappingOuter.BackColor = System.Drawing.Color.White;
            this.pnlMappingOuter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlMappingOuter.Controls.Add(this.tabMapping);
            this.pnlMappingOuter.Controls.Add(this.pnlMappingHeader);
            this.pnlMappingOuter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMappingOuter.Location = new System.Drawing.Point(10, 560);
            this.pnlMappingOuter.Name = "pnlMappingOuter";
            this.pnlMappingOuter.Size = new System.Drawing.Size(1388, 250);
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
            this.tabMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMapping.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabMapping.Location = new System.Drawing.Point(0, 28);
            this.tabMapping.Name = "tabMapping";
            this.tabMapping.SelectedIndex = 0;
            this.tabMapping.Size = new System.Drawing.Size(1386, 220);
            this.tabMapping.TabIndex = 1;
            // 
            // tabAttach
            // 
            this.tabAttach.BackColor = System.Drawing.Color.White;
            this.tabAttach.Controls.Add(this.lstAttach);
            this.tabAttach.Location = new System.Drawing.Point(4, 29);
            this.tabAttach.Name = "tabAttach";
            this.tabAttach.Padding = new System.Windows.Forms.Padding(4);
            this.tabAttach.Size = new System.Drawing.Size(1378, 187);
            this.tabAttach.TabIndex = 0;
            this.tabAttach.Text = "📎 ไฟล์ที่แนบ";
            // 
            // lstAttach
            // 
            this.lstAttach.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstAttach.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colAttFile,
            this.colAttSize,
            this.colAttDate});
            this.lstAttach.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstAttach.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstAttach.FullRowSelect = true;
            this.lstAttach.HideSelection = false;
            this.lstAttach.Location = new System.Drawing.Point(4, 4);
            this.lstAttach.Name = "lstAttach";
            this.lstAttach.Size = new System.Drawing.Size(1370, 179);
            this.lstAttach.TabIndex = 0;
            this.lstAttach.UseCompatibleStateImageBehavior = false;
            this.lstAttach.View = System.Windows.Forms.View.Details;
            // 
            // colAttFile
            // 
            this.colAttFile.Text = "ชื่อไฟล์";
            this.colAttFile.Width = 400;
            // 
            // colAttSize
            // 
            this.colAttSize.Text = "ขนาด";
            this.colAttSize.Width = 80;
            // 
            // colAttDate
            // 
            this.colAttDate.Text = "วันที่อัพโหลด";
            this.colAttDate.Width = 140;
            // 
            // tabRelated
            // 
            this.tabRelated.BackColor = System.Drawing.Color.White;
            this.tabRelated.Controls.Add(this.lstRelated);
            this.tabRelated.Location = new System.Drawing.Point(4, 29);
            this.tabRelated.Name = "tabRelated";
            this.tabRelated.Padding = new System.Windows.Forms.Padding(4);
            this.tabRelated.Size = new System.Drawing.Size(949, 187);
            this.tabRelated.TabIndex = 1;
            this.tabRelated.Text = "🔗 ความรู้ที่เกี่ยวข้อง";
            // 
            // lstRelated
            // 
            this.lstRelated.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstRelated.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colRelCode,
            this.colRelTitle,
            this.colRelType});
            this.lstRelated.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstRelated.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstRelated.FullRowSelect = true;
            this.lstRelated.HideSelection = false;
            this.lstRelated.Location = new System.Drawing.Point(4, 4);
            this.lstRelated.Name = "lstRelated";
            this.lstRelated.Size = new System.Drawing.Size(941, 179);
            this.lstRelated.TabIndex = 0;
            this.lstRelated.UseCompatibleStateImageBehavior = false;
            this.lstRelated.View = System.Windows.Forms.View.Details;
            // 
            // colRelCode
            // 
            this.colRelCode.Text = "รหัสบทความ";
            this.colRelCode.Width = 120;
            // 
            // colRelTitle
            // 
            this.colRelTitle.Text = "ชื่อบทความ";
            this.colRelTitle.Width = 380;
            // 
            // colRelType
            // 
            this.colRelType.Text = "ประเภท";
            this.colRelType.Width = 100;
            // 
            // tabWorkflow
            // 
            this.tabWorkflow.BackColor = System.Drawing.Color.White;
            this.tabWorkflow.Controls.Add(this.lstWorkflow);
            this.tabWorkflow.Location = new System.Drawing.Point(4, 29);
            this.tabWorkflow.Name = "tabWorkflow";
            this.tabWorkflow.Padding = new System.Windows.Forms.Padding(4);
            this.tabWorkflow.Size = new System.Drawing.Size(949, 187);
            this.tabWorkflow.TabIndex = 2;
            this.tabWorkflow.Text = "⚖️ เส้นงาน (Workflow)";
            // 
            // lstWorkflow
            // 
            this.lstWorkflow.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstWorkflow.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colWfCode,
            this.colWfName,
            this.colWfStatus});
            this.lstWorkflow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstWorkflow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstWorkflow.FullRowSelect = true;
            this.lstWorkflow.HideSelection = false;
            this.lstWorkflow.Location = new System.Drawing.Point(4, 4);
            this.lstWorkflow.Name = "lstWorkflow";
            this.lstWorkflow.Size = new System.Drawing.Size(941, 179);
            this.lstWorkflow.TabIndex = 0;
            this.lstWorkflow.UseCompatibleStateImageBehavior = false;
            this.lstWorkflow.View = System.Windows.Forms.View.Details;
            // 
            // colWfCode
            // 
            this.colWfCode.Text = "รหัส Workflow";
            this.colWfCode.Width = 140;
            // 
            // colWfName
            // 
            this.colWfName.Text = "ชื่อ Workflow";
            this.colWfName.Width = 360;
            // 
            // colWfStatus
            // 
            this.colWfStatus.Text = "สถานะ";
            this.colWfStatus.Width = 100;
            // 
            // tabPrograms
            // 
            this.tabPrograms.BackColor = System.Drawing.Color.White;
            this.tabPrograms.Controls.Add(this.lstPrograms);
            this.tabPrograms.Location = new System.Drawing.Point(4, 29);
            this.tabPrograms.Name = "tabPrograms";
            this.tabPrograms.Padding = new System.Windows.Forms.Padding(4);
            this.tabPrograms.Size = new System.Drawing.Size(949, 187);
            this.tabPrograms.TabIndex = 3;
            this.tabPrograms.Text = "💻 โปรแกรมที่เกี่ยวข้อง";
            // 
            // lstPrograms
            // 
            this.lstPrograms.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstPrograms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colProgName,
            this.colProgType});
            this.lstPrograms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstPrograms.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstPrograms.FullRowSelect = true;
            this.lstPrograms.HideSelection = false;
            this.lstPrograms.Location = new System.Drawing.Point(4, 4);
            this.lstPrograms.Name = "lstPrograms";
            this.lstPrograms.Size = new System.Drawing.Size(941, 179);
            this.lstPrograms.TabIndex = 0;
            this.lstPrograms.UseCompatibleStateImageBehavior = false;
            this.lstPrograms.View = System.Windows.Forms.View.Details;
            // 
            // colProgName
            // 
            this.colProgName.Text = "ชื่อโปรแกรม";
            this.colProgName.Width = 380;
            // 
            // colProgType
            // 
            this.colProgType.Text = "ประเภท";
            this.colProgType.Width = 180;
            // 
            // tabDocs
            // 
            this.tabDocs.BackColor = System.Drawing.Color.White;
            this.tabDocs.Controls.Add(this.lstDocs);
            this.tabDocs.Location = new System.Drawing.Point(4, 29);
            this.tabDocs.Name = "tabDocs";
            this.tabDocs.Padding = new System.Windows.Forms.Padding(4);
            this.tabDocs.Size = new System.Drawing.Size(949, 187);
            this.tabDocs.TabIndex = 4;
            this.tabDocs.Text = "📋 เอกสารที่เกี่ยวข้อง";
            // 
            // lstDocs
            // 
            this.lstDocs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstDocs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDocName,
            this.colDocStatus});
            this.lstDocs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDocs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstDocs.FullRowSelect = true;
            this.lstDocs.HideSelection = false;
            this.lstDocs.Location = new System.Drawing.Point(4, 4);
            this.lstDocs.Name = "lstDocs";
            this.lstDocs.Size = new System.Drawing.Size(941, 179);
            this.lstDocs.TabIndex = 0;
            this.lstDocs.UseCompatibleStateImageBehavior = false;
            this.lstDocs.View = System.Windows.Forms.View.Details;
            // 
            // colDocName
            // 
            this.colDocName.Text = "ชื่อเอกสาร";
            this.colDocName.Width = 420;
            // 
            // colDocStatus
            // 
            this.colDocStatus.Text = "สถานะ";
            this.colDocStatus.Width = 120;
            // 
            // tabVersions
            // 
            this.tabVersions.BackColor = System.Drawing.Color.White;
            this.tabVersions.Controls.Add(this.lstVersions);
            this.tabVersions.Location = new System.Drawing.Point(4, 29);
            this.tabVersions.Name = "tabVersions";
            this.tabVersions.Padding = new System.Windows.Forms.Padding(4);
            this.tabVersions.Size = new System.Drawing.Size(949, 187);
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
            this.lstVersions.Size = new System.Drawing.Size(941, 179);
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
            this.pnlMappingHeader.Size = new System.Drawing.Size(1386, 28);
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
            this.lblMappingHeader.Size = new System.Drawing.Size(1386, 28);
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
            this.pnlContentOuter.Controls.Add(this.txtContent);
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
            // txtContent
            // 
            this.txtContent.BackColor = System.Drawing.Color.White;
            this.txtContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtContent.DetectUrls = false;
            this.txtContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtContent.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.txtContent.Location = new System.Drawing.Point(0, 151);
            this.txtContent.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.txtContent.Name = "txtContent";
            this.txtContent.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtContent.Size = new System.Drawing.Size(1386, 141);
            this.txtContent.TabIndex = 5;
            this.txtContent.Text = "";
            this.txtContent.TextChanged += new System.EventHandler(this.TxtContent_TextChanged);
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
            this.lblContent.Location = new System.Drawing.Point(0, 129);
            this.lblContent.Name = "lblContent";
            this.lblContent.Padding = new System.Windows.Forms.Padding(10, 4, 10, 0);
            this.lblContent.Size = new System.Drawing.Size(1386, 22);
            this.lblContent.TabIndex = 3;
            this.lblContent.Text = "เนื้อหาบทความ (รองรับ Markdown / Plain Text)";
            // 
            // tsRte
            // 
            this.tsRte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.tsRte.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsRte.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.tsRte.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsRteBold,
            this.tsRteItalic,
            this.tsRteUnder,
            this.tsRteSep1,
            this.tsRteH1,
            this.tsRteH2,
            this.tsRteH3,
            this.tsRteSep2,
            this.tsRteBullet,
            this.tsRteNumList,
            this.tsRteSep3,
            this.tsRteLink,
            this.tsRteImage,
            this.tsRteAttach,
            this.tsRteSep4,
            this.tsRteUndo,
            this.tsRteRedo});
            this.tsRte.Location = new System.Drawing.Point(0, 102);
            this.tsRte.Name = "tsRte";
            this.tsRte.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tsRte.Size = new System.Drawing.Size(1386, 27);
            this.tsRte.TabIndex = 2;
            // 
            // tsRteBold
            // 
            this.tsRteBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteBold.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsRteBold.Name = "tsRteBold";
            this.tsRteBold.Size = new System.Drawing.Size(29, 24);
            this.tsRteBold.Text = "B";
            this.tsRteBold.ToolTipText = "Bold (Ctrl+B)";
            this.tsRteBold.Click += new System.EventHandler(this.TsRteBold_Click);
            // 
            // tsRteItalic
            // 
            this.tsRteItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteItalic.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.tsRteItalic.Name = "tsRteItalic";
            this.tsRteItalic.Size = new System.Drawing.Size(29, 24);
            this.tsRteItalic.Text = "I";
            this.tsRteItalic.ToolTipText = "Italic (Ctrl+I)";
            this.tsRteItalic.Click += new System.EventHandler(this.TsRteItalic_Click);
            // 
            // tsRteUnder
            // 
            this.tsRteUnder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteUnder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
            this.tsRteUnder.Name = "tsRteUnder";
            this.tsRteUnder.Size = new System.Drawing.Size(29, 24);
            this.tsRteUnder.Text = "U";
            this.tsRteUnder.ToolTipText = "Underline (Ctrl+U)";
            this.tsRteUnder.Click += new System.EventHandler(this.TsRteUnder_Click);
            // 
            // tsRteSep1
            // 
            this.tsRteSep1.Name = "tsRteSep1";
            this.tsRteSep1.Size = new System.Drawing.Size(6, 27);
            // 
            // tsRteH1
            // 
            this.tsRteH1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteH1.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.tsRteH1.Name = "tsRteH1";
            this.tsRteH1.Size = new System.Drawing.Size(33, 24);
            this.tsRteH1.Text = "H1";
            this.tsRteH1.ToolTipText = "หัวข้อหลัก (Heading 1)";
            this.tsRteH1.Click += new System.EventHandler(this.TsRteH1_Click);
            // 
            // tsRteH2
            // 
            this.tsRteH2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteH2.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.tsRteH2.Name = "tsRteH2";
            this.tsRteH2.Size = new System.Drawing.Size(33, 24);
            this.tsRteH2.Text = "H2";
            this.tsRteH2.ToolTipText = "หัวข้อรอง (Heading 2)";
            this.tsRteH2.Click += new System.EventHandler(this.TsRteH2_Click);
            // 
            // tsRteH3
            // 
            this.tsRteH3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteH3.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.tsRteH3.Name = "tsRteH3";
            this.tsRteH3.Size = new System.Drawing.Size(33, 24);
            this.tsRteH3.Text = "H3";
            this.tsRteH3.ToolTipText = "หัวข้อย่อย (Heading 3)";
            this.tsRteH3.Click += new System.EventHandler(this.TsRteH3_Click);
            // 
            // tsRteSep2
            // 
            this.tsRteSep2.Name = "tsRteSep2";
            this.tsRteSep2.Size = new System.Drawing.Size(6, 27);
            // 
            // tsRteBullet
            // 
            this.tsRteBullet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteBullet.Name = "tsRteBullet";
            this.tsRteBullet.Size = new System.Drawing.Size(29, 24);
            this.tsRteBullet.Text = "•";
            this.tsRteBullet.ToolTipText = "รายการสัญลักษณ์ (Bullet List)";
            this.tsRteBullet.Click += new System.EventHandler(this.TsRteBullet_Click);
            // 
            // tsRteNumList
            // 
            this.tsRteNumList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteNumList.Name = "tsRteNumList";
            this.tsRteNumList.Size = new System.Drawing.Size(29, 24);
            this.tsRteNumList.Text = "1.";
            this.tsRteNumList.ToolTipText = "รายการตัวเลข (Numbered List)";
            this.tsRteNumList.Click += new System.EventHandler(this.TsRteNumList_Click);
            // 
            // tsRteSep3
            // 
            this.tsRteSep3.Name = "tsRteSep3";
            this.tsRteSep3.Size = new System.Drawing.Size(6, 27);
            // 
            // tsRteLink
            // 
            this.tsRteLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteLink.Name = "tsRteLink";
            this.tsRteLink.Size = new System.Drawing.Size(34, 24);
            this.tsRteLink.Text = "🔗";
            this.tsRteLink.ToolTipText = "แทรก Hyperlink";
            this.tsRteLink.Click += new System.EventHandler(this.TsRteLink_Click);
            // 
            // tsRteImage
            // 
            this.tsRteImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteImage.Name = "tsRteImage";
            this.tsRteImage.Size = new System.Drawing.Size(34, 24);
            this.tsRteImage.Text = "📷";
            this.tsRteImage.ToolTipText = "แทรกรูปภาพ";
            this.tsRteImage.Click += new System.EventHandler(this.TsRteImage_Click);
            // 
            // tsRteAttach
            // 
            this.tsRteAttach.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteAttach.Name = "tsRteAttach";
            this.tsRteAttach.Size = new System.Drawing.Size(29, 24);
            this.tsRteAttach.Text = "📎";
            this.tsRteAttach.ToolTipText = "แนบไฟล์";
            this.tsRteAttach.Click += new System.EventHandler(this.TsRteAttach_Click);
            // 
            // tsRteSep4
            // 
            this.tsRteSep4.Name = "tsRteSep4";
            this.tsRteSep4.Size = new System.Drawing.Size(6, 27);
            // 
            // tsRteUndo
            // 
            this.tsRteUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteUndo.Name = "tsRteUndo";
            this.tsRteUndo.Size = new System.Drawing.Size(29, 24);
            this.tsRteUndo.Text = "↶";
            this.tsRteUndo.ToolTipText = "Undo (Ctrl+Z)";
            this.tsRteUndo.Click += new System.EventHandler(this.TsRteUndo_Click);
            // 
            // tsRteRedo
            // 
            this.tsRteRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsRteRedo.Name = "tsRteRedo";
            this.tsRteRedo.Size = new System.Drawing.Size(29, 24);
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
            this.pnlSummaryArea.Size = new System.Drawing.Size(1386, 74);
            this.pnlSummaryArea.TabIndex = 1;
            // 
            // txtSummary
            // 
            this.txtSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSummary.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSummary.Location = new System.Drawing.Point(10, 22);
            this.txtSummary.MaxLength = 1000;
            this.txtSummary.Multiline = true;
            this.txtSummary.Name = "txtSummary";
            this.txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSummary.Size = new System.Drawing.Size(1366, 52);
            this.txtSummary.TabIndex = 1;
            // 
            // lblSummary
            // 
            this.lblSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblSummary.Location = new System.Drawing.Point(10, 4);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(1366, 18);
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
            this.tlpGenInfo.Controls.Add(this.lblStatusDisplay, 2, 1);
            this.tlpGenInfo.Controls.Add(this.lblCurVersion, 3, 1);
            this.tlpGenInfo.Controls.Add(this.lblTitle, 0, 3);
            this.tlpGenInfo.Controls.Add(this.lblCategory, 3, 3);
            this.tlpGenInfo.Controls.Add(this.txtTitle, 0, 4);
            this.tlpGenInfo.Controls.Add(this.cboCategory, 3, 4);
            this.tlpGenInfo.Controls.Add(this.lblTags, 0, 6);
            this.tlpGenInfo.Controls.Add(this.lblExpiry, 2, 6);
            this.tlpGenInfo.Controls.Add(this.txtTags, 0, 7);
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
            // lblStatusDisplay
            // 
            this.lblStatusDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(237)))), ((int)(((byte)(236)))));
            this.lblStatusDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusDisplay.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatusDisplay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(43)))), ((int)(((byte)(33)))));
            this.lblStatusDisplay.Location = new System.Drawing.Point(694, 26);
            this.lblStatusDisplay.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStatusDisplay.Name = "lblStatusDisplay";
            this.lblStatusDisplay.Size = new System.Drawing.Size(337, 28);
            this.lblStatusDisplay.TabIndex = 12;
            this.lblStatusDisplay.Text = "📝 Draft";
            this.lblStatusDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            // txtTags
            // 
            this.tlpGenInfo.SetColumnSpan(this.txtTags, 2);
            this.txtTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTags.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.txtTags.Location = new System.Drawing.Point(12, 138);
            this.txtTags.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.txtTags.Name = "txtTags";
            this.txtTags.Size = new System.Drawing.Size(678, 26);
            this.txtTags.TabIndex = 32;
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
            this.tabRelated.ResumeLayout(false);
            this.tabWorkflow.ResumeLayout(false);
            this.tabPrograms.ResumeLayout(false);
            this.tabDocs.ResumeLayout(false);
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
        private System.Windows.Forms.Label       lblStatusDisplay;
        private System.Windows.Forms.Label       lblVerLabel;
        private System.Windows.Forms.Label       lblCurVersion;
        private System.Windows.Forms.Label       lblTitle;
        private System.Windows.Forms.TextBox     txtTitle;
        private System.Windows.Forms.Label       lblCategory;
        private System.Windows.Forms.ComboBox    cboCategory;
        private System.Windows.Forms.Label       lblTags;
        private System.Windows.Forms.TextBox     txtTags;
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
        private System.Windows.Forms.ToolStripButton    tsRteBold;
        private System.Windows.Forms.ToolStripButton    tsRteItalic;
        private System.Windows.Forms.ToolStripButton    tsRteUnder;
        private System.Windows.Forms.ToolStripSeparator tsRteSep1;
        private System.Windows.Forms.ToolStripButton    tsRteH1;
        private System.Windows.Forms.ToolStripButton    tsRteH2;
        private System.Windows.Forms.ToolStripButton    tsRteH3;
        private System.Windows.Forms.ToolStripSeparator tsRteSep2;
        private System.Windows.Forms.ToolStripButton    tsRteBullet;
        private System.Windows.Forms.ToolStripButton    tsRteNumList;
        private System.Windows.Forms.ToolStripSeparator tsRteSep3;
        private System.Windows.Forms.ToolStripButton    tsRteLink;
        private System.Windows.Forms.ToolStripButton    tsRteImage;
        private System.Windows.Forms.ToolStripButton    tsRteAttach;
        private System.Windows.Forms.ToolStripSeparator tsRteSep4;
        private System.Windows.Forms.ToolStripButton    tsRteUndo;
        private System.Windows.Forms.ToolStripButton    tsRteRedo;
        private System.Windows.Forms.Label       lblContent;
        private System.Windows.Forms.RichTextBox txtContent;
        private System.Windows.Forms.Panel       pnlRteStatus;
        private System.Windows.Forms.Label       lblRteStatus;

        // ── Mapping section ───────────────────────────────────────────────────
        private System.Windows.Forms.Panel       pnlMappingOuter;
        private System.Windows.Forms.Panel       pnlMappingHeader;
        private System.Windows.Forms.Label       lblMappingHeader;
        private System.Windows.Forms.TabControl  tabMapping;
        private System.Windows.Forms.TabPage     tabAttach;
        private System.Windows.Forms.ListView    lstAttach;
        private System.Windows.Forms.ColumnHeader colAttFile;
        private System.Windows.Forms.ColumnHeader colAttSize;
        private System.Windows.Forms.ColumnHeader colAttDate;
        private System.Windows.Forms.TabPage     tabRelated;
        private System.Windows.Forms.ListView    lstRelated;
        private System.Windows.Forms.ColumnHeader colRelCode;
        private System.Windows.Forms.ColumnHeader colRelTitle;
        private System.Windows.Forms.ColumnHeader colRelType;
        private System.Windows.Forms.TabPage     tabWorkflow;
        private System.Windows.Forms.ListView    lstWorkflow;
        private System.Windows.Forms.ColumnHeader colWfCode;
        private System.Windows.Forms.ColumnHeader colWfName;
        private System.Windows.Forms.ColumnHeader colWfStatus;
        private System.Windows.Forms.TabPage     tabPrograms;
        private System.Windows.Forms.ListView    lstPrograms;
        private System.Windows.Forms.ColumnHeader colProgName;
        private System.Windows.Forms.ColumnHeader colProgType;
        private System.Windows.Forms.TabPage     tabDocs;
        private System.Windows.Forms.ListView    lstDocs;
        private System.Windows.Forms.ColumnHeader colDocName;
        private System.Windows.Forms.ColumnHeader colDocStatus;
        private System.Windows.Forms.TabPage     tabVersions;
        private System.Windows.Forms.ListView    lstVersions;
        private System.Windows.Forms.ColumnHeader colVerNum;
        private System.Windows.Forms.ColumnHeader colVerDate;
        private System.Windows.Forms.ColumnHeader colVerBy;
        private System.Windows.Forms.ColumnHeader colVerNote;
    }
}
