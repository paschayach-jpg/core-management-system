namespace CMS.Presentation.Modules.KMS
{
    partial class ArticleViewerForm
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
            this.lblViewerTitle = new System.Windows.Forms.Label();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlToolbarSep = new System.Windows.Forms.Panel();
            this.btnPin = new System.Windows.Forms.Button();
            this.btnExportPdf = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnShare = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.splitViewer = new System.Windows.Forms.SplitContainer();
            this.webContent = new System.Windows.Forms.WebBrowser();
            this.pnlZoomStatus = new System.Windows.Forms.Panel();
            this.lblZoomIndicator = new System.Windows.Forms.Label();
            this.pnlInfoWrapper = new System.Windows.Forms.Panel();
            this.pnlArticleInfo = new System.Windows.Forms.Panel();
            this.tblMeta = new System.Windows.Forms.TableLayoutPanel();
            this.lblCodeVersion = new System.Windows.Forms.Label();
            this.lblAuthorVal = new System.Windows.Forms.Label();
            this.lblCategoryVal = new System.Windows.Forms.Label();
            this.lblPublishVal = new System.Windows.Forms.Label();
            this.lblConfidential = new System.Windows.Forms.Label();
            this.lblReviewDueVal = new System.Windows.Forms.Label();
            this.flpBadges = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTypeBadge = new System.Windows.Forms.Label();
            this.lblCategoryBadge = new System.Windows.Forms.Label();
            this.lblTitleCategory = new System.Windows.Forms.Label();
            this.lblStatusBadge = new System.Windows.Forms.Label();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlMappingSection = new System.Windows.Forms.Panel();
            this.tabMapping = new System.Windows.Forms.TabControl();
            this.tabAttach = new System.Windows.Forms.TabPage();
            this.dgvAttachLinked = new System.Windows.Forms.DataGridView();
            this.colAttachNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAttachName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAttachSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAttachDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAttachView = new System.Windows.Forms.DataGridViewButtonColumn();
            this.lblAttachLinked = new System.Windows.Forms.Label();
            this.tabRelated = new System.Windows.Forms.TabPage();
            this.dgvRelatedLinked = new System.Windows.Forms.DataGridView();
            this.lblRelatedLinked = new System.Windows.Forms.Label();
            this.tabWorkflow = new System.Windows.Forms.TabPage();
            this.dgvWorkflowLinked = new System.Windows.Forms.DataGridView();
            this.colWfNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWfCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWfName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWfStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnStartWorkflow = new System.Windows.Forms.Button();
            this.lblWorkflowLinked = new System.Windows.Forms.Label();
            this.tabPrograms = new System.Windows.Forms.TabPage();
            this.dgvProgramsLinked = new System.Windows.Forms.DataGridView();
            this.colProgNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProgCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProgName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProgDept = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblProgramsLinked = new System.Windows.Forms.Label();
            this.tabDocs = new System.Windows.Forms.TabPage();
            this.dgvDocsLinked = new System.Windows.Forms.DataGridView();
            this.colDocNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocRev = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDocCraf = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblDocsLinked = new System.Windows.Forms.Label();
            this.tabDepartments = new System.Windows.Forms.TabPage();
            this.dgvDepartmentsLinked = new System.Windows.Forms.DataGridView();
            this.colDeptNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblDepartmentsLinked = new System.Windows.Forms.Label();
            this.pnlMappingHeader = new System.Windows.Forms.Panel();
            this.lblMappingTitle = new System.Windows.Forms.Label();
            this.lblOfficialBadge = new System.Windows.Forms.Label();
            this.pnlVersionList = new System.Windows.Forms.Panel();
            this.colRelatedNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlToolbarSep.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitViewer)).BeginInit();
            this.splitViewer.Panel1.SuspendLayout();
            this.splitViewer.Panel2.SuspendLayout();
            this.splitViewer.SuspendLayout();
            this.pnlZoomStatus.SuspendLayout();
            this.pnlInfoWrapper.SuspendLayout();
            this.pnlArticleInfo.SuspendLayout();
            this.tblMeta.SuspendLayout();
            this.flpBadges.SuspendLayout();
            this.pnlSidebar.SuspendLayout();
            this.pnlMappingSection.SuspendLayout();
            this.tabMapping.SuspendLayout();
            this.tabAttach.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttachLinked)).BeginInit();
            this.tabRelated.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelatedLinked)).BeginInit();
            this.tabWorkflow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorkflowLinked)).BeginInit();
            this.tabPrograms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProgramsLinked)).BeginInit();
            this.tabDocs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocsLinked)).BeginInit();
            this.tabDepartments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDepartmentsLinked)).BeginInit();
            this.pnlMappingHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.pnlHeader.Controls.Add(this.lblViewerTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1100, 36);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblViewerTitle
            // 
            this.lblViewerTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblViewerTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblViewerTitle.ForeColor = System.Drawing.Color.White;
            this.lblViewerTitle.Location = new System.Drawing.Point(0, 0);
            this.lblViewerTitle.Name = "lblViewerTitle";
            this.lblViewerTitle.Size = new System.Drawing.Size(1100, 36);
            this.lblViewerTitle.TabIndex = 0;
            this.lblViewerTitle.Text = "  👁️ ดูบทความ";
            this.lblViewerTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
            this.pnlToolbar.Controls.Add(this.panel1);
            this.pnlToolbar.Controls.Add(this.pnlToolbarSep);
            this.pnlToolbar.Controls.Add(this.btnBack);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 36);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.pnlToolbar.Size = new System.Drawing.Size(1100, 53);
            this.pnlToolbar.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(88, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(484, 45);
            this.panel1.TabIndex = 100;
            // 
            // pnlToolbarSep
            // 
            this.pnlToolbarSep.BackColor = System.Drawing.Color.Transparent;
            this.pnlToolbarSep.Controls.Add(this.btnPin);
            this.pnlToolbarSep.Controls.Add(this.btnExportPdf);
            this.pnlToolbarSep.Controls.Add(this.btnPrint);
            this.pnlToolbarSep.Controls.Add(this.btnShare);
            this.pnlToolbarSep.Controls.Add(this.btnEdit);
            this.pnlToolbarSep.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlToolbarSep.Location = new System.Drawing.Point(572, 4);
            this.pnlToolbarSep.Name = "pnlToolbarSep";
            this.pnlToolbarSep.Size = new System.Drawing.Size(522, 45);
            this.pnlToolbarSep.TabIndex = 99;
            // 
            // btnPin
            // 
            this.btnPin.BackColor = System.Drawing.Color.White;
            this.btnPin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPin.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.btnPin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPin.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnPin.Location = new System.Drawing.Point(415, 0);
            this.btnPin.Name = "btnPin";
            this.btnPin.Size = new System.Drawing.Size(107, 45);
            this.btnPin.TabIndex = 4;
            this.btnPin.Text = "📌 Pin";
            this.btnPin.UseVisualStyleBackColor = false;
            this.btnPin.Click += new System.EventHandler(this.BtnPin_Click);
            // 
            // btnExportPdf
            // 
            this.btnExportPdf.BackColor = System.Drawing.Color.White;
            this.btnExportPdf.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportPdf.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnExportPdf.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.btnExportPdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportPdf.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnExportPdf.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnExportPdf.Location = new System.Drawing.Point(262, 0);
            this.btnExportPdf.Name = "btnExportPdf";
            this.btnExportPdf.Size = new System.Drawing.Size(153, 45);
            this.btnExportPdf.TabIndex = 3;
            this.btnExportPdf.Text = "📄 Export PDF";
            this.btnExportPdf.UseVisualStyleBackColor = false;
            this.btnExportPdf.Click += new System.EventHandler(this.BtnExportPdf_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.BackColor = System.Drawing.Color.White;
            this.btnPrint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrint.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPrint.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.btnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPrint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnPrint.Location = new System.Drawing.Point(176, 0);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(86, 45);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "🖨️ พิมพ์";
            this.btnPrint.UseVisualStyleBackColor = false;
            this.btnPrint.Click += new System.EventHandler(this.BtnPrint_Click);
            // 
            // btnShare
            // 
            this.btnShare.BackColor = System.Drawing.Color.White;
            this.btnShare.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShare.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnShare.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.btnShare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShare.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnShare.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnShare.Location = new System.Drawing.Point(90, 0);
            this.btnShare.Name = "btnShare";
            this.btnShare.Size = new System.Drawing.Size(86, 45);
            this.btnShare.TabIndex = 5;
            this.btnShare.Text = "📤 แชร์";
            this.btnShare.UseVisualStyleBackColor = false;
            this.btnShare.Click += new System.EventHandler(this.BtnShare_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnEdit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEdit.ForeColor = System.Drawing.Color.White;
            this.btnEdit.Location = new System.Drawing.Point(0, 0);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(90, 45);
            this.btnEdit.TabIndex = 4;
            this.btnEdit.Text = "✏️ แก้ไข";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // btnBack
            // 
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(240)))));
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBack.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.btnBack.Location = new System.Drawing.Point(6, 4);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(82, 45);
            this.btnBack.TabIndex = 5;
            this.btnBack.Text = "← กลับ";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            // 
            // splitViewer
            // 
            this.splitViewer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.splitViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitViewer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitViewer.Location = new System.Drawing.Point(0, 89);
            this.splitViewer.Name = "splitViewer";
            // 
            // splitViewer.Panel1
            // 
            this.splitViewer.Panel1.BackColor = System.Drawing.Color.White;
            this.splitViewer.Panel1.Controls.Add(this.webContent);
            this.splitViewer.Panel1.Controls.Add(this.pnlZoomStatus);
            this.splitViewer.Panel1.Controls.Add(this.pnlInfoWrapper);
            this.splitViewer.Panel1MinSize = 400;
            // 
            // splitViewer.Panel2
            // 
            this.splitViewer.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.splitViewer.Panel2.Controls.Add(this.pnlSidebar);
            this.splitViewer.Panel2MinSize = 260;
            this.splitViewer.Size = new System.Drawing.Size(1100, 591);
            this.splitViewer.SplitterDistance = 400;
            this.splitViewer.TabIndex = 2;
            // 
            // webContent
            // 
            this.webContent.AllowWebBrowserDrop = false;
            this.webContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webContent.IsWebBrowserContextMenuEnabled = false;
            this.webContent.Location = new System.Drawing.Point(0, 106);
            this.webContent.Name = "webContent";
            this.webContent.ScriptErrorsSuppressed = true;
            this.webContent.Size = new System.Drawing.Size(400, 463);
            this.webContent.TabIndex = 1;
            this.webContent.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.WebContent_Navigating);
            // 
            // pnlZoomStatus
            // 
            this.pnlZoomStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            this.pnlZoomStatus.Controls.Add(this.lblZoomIndicator);
            this.pnlZoomStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlZoomStatus.Location = new System.Drawing.Point(0, 569);
            this.pnlZoomStatus.Name = "pnlZoomStatus";
            this.pnlZoomStatus.Size = new System.Drawing.Size(400, 22);
            this.pnlZoomStatus.TabIndex = 2;
            this.pnlZoomStatus.Visible = false;
            // 
            // lblZoomIndicator
            // 
            this.lblZoomIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblZoomIndicator.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblZoomIndicator.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.lblZoomIndicator.Location = new System.Drawing.Point(0, 0);
            this.lblZoomIndicator.Name = "lblZoomIndicator";
            this.lblZoomIndicator.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.lblZoomIndicator.Size = new System.Drawing.Size(400, 22);
            this.lblZoomIndicator.TabIndex = 0;
            this.lblZoomIndicator.Text = "🔍 100%";
            this.lblZoomIndicator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlInfoWrapper
            // 
            this.pnlInfoWrapper.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(159)))), ((int)(((byte)(212)))));
            this.pnlInfoWrapper.Controls.Add(this.pnlArticleInfo);
            this.pnlInfoWrapper.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlInfoWrapper.Location = new System.Drawing.Point(0, 0);
            this.pnlInfoWrapper.Name = "pnlInfoWrapper";
            this.pnlInfoWrapper.Padding = new System.Windows.Forms.Padding(1);
            this.pnlInfoWrapper.Size = new System.Drawing.Size(400, 106);
            this.pnlInfoWrapper.TabIndex = 0;
            // 
            // pnlArticleInfo
            // 
            this.pnlArticleInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            this.pnlArticleInfo.Controls.Add(this.tblMeta);
            this.pnlArticleInfo.Controls.Add(this.flpBadges);
            this.pnlArticleInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlArticleInfo.Location = new System.Drawing.Point(1, 1);
            this.pnlArticleInfo.Name = "pnlArticleInfo";
            this.pnlArticleInfo.Padding = new System.Windows.Forms.Padding(10, 8, 10, 4);
            this.pnlArticleInfo.Size = new System.Drawing.Size(398, 104);
            this.pnlArticleInfo.TabIndex = 0;
            // 
            // tblMeta
            // 
            this.tblMeta.BackColor = System.Drawing.Color.Transparent;
            this.tblMeta.ColumnCount = 2;
            this.tblMeta.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblMeta.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblMeta.Controls.Add(this.lblCodeVersion, 0, 0);
            this.tblMeta.Controls.Add(this.lblAuthorVal, 1, 0);
            this.tblMeta.Controls.Add(this.lblCategoryVal, 0, 1);
            this.tblMeta.Controls.Add(this.lblPublishVal, 1, 1);
            this.tblMeta.Controls.Add(this.lblConfidential, 0, 2);
            this.tblMeta.Controls.Add(this.lblReviewDueVal, 1, 2);
            this.tblMeta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblMeta.Location = new System.Drawing.Point(10, 36);
            this.tblMeta.Name = "tblMeta";
            this.tblMeta.RowCount = 3;
            this.tblMeta.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tblMeta.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tblMeta.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tblMeta.Size = new System.Drawing.Size(378, 64);
            this.tblMeta.TabIndex = 1;
            // 
            // lblCodeVersion
            // 
            this.lblCodeVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCodeVersion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCodeVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.lblCodeVersion.Location = new System.Drawing.Point(3, 0);
            this.lblCodeVersion.Name = "lblCodeVersion";
            this.lblCodeVersion.Size = new System.Drawing.Size(183, 21);
            this.lblCodeVersion.TabIndex = 0;
            this.lblCodeVersion.Text = "รหัส: -  |  Version 1";
            this.lblCodeVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAuthorVal
            // 
            this.lblAuthorVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAuthorVal.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblAuthorVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.lblAuthorVal.Location = new System.Drawing.Point(192, 0);
            this.lblAuthorVal.Name = "lblAuthorVal";
            this.lblAuthorVal.Size = new System.Drawing.Size(183, 21);
            this.lblAuthorVal.TabIndex = 1;
            this.lblAuthorVal.Text = "ผู้เขียน: -";
            this.lblAuthorVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCategoryVal
            // 
            this.lblCategoryVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCategoryVal.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCategoryVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.lblCategoryVal.Location = new System.Drawing.Point(3, 21);
            this.lblCategoryVal.Name = "lblCategoryVal";
            this.lblCategoryVal.Size = new System.Drawing.Size(183, 21);
            this.lblCategoryVal.TabIndex = 2;
            this.lblCategoryVal.Text = "หมวดหมู่: -";
            this.lblCategoryVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPublishVal
            // 
            this.lblPublishVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPublishVal.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblPublishVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.lblPublishVal.Location = new System.Drawing.Point(192, 21);
            this.lblPublishVal.Name = "lblPublishVal";
            this.lblPublishVal.Size = new System.Drawing.Size(183, 21);
            this.lblPublishVal.TabIndex = 3;
            this.lblPublishVal.Text = "Publish: -";
            this.lblPublishVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblConfidential
            // 
            this.lblConfidential.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblConfidential.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblConfidential.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.lblConfidential.Location = new System.Drawing.Point(3, 42);
            this.lblConfidential.Name = "lblConfidential";
            this.lblConfidential.Size = new System.Drawing.Size(183, 22);
            this.lblConfidential.TabIndex = 4;
            this.lblConfidential.Text = "ระดับความลับ: -";
            this.lblConfidential.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblReviewDueVal
            // 
            this.lblReviewDueVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblReviewDueVal.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblReviewDueVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.lblReviewDueVal.Location = new System.Drawing.Point(192, 42);
            this.lblReviewDueVal.Name = "lblReviewDueVal";
            this.lblReviewDueVal.Size = new System.Drawing.Size(183, 22);
            this.lblReviewDueVal.TabIndex = 5;
            this.lblReviewDueVal.Text = "Review Due: -";
            this.lblReviewDueVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flpBadges
            // 
            this.flpBadges.BackColor = System.Drawing.Color.Transparent;
            this.flpBadges.Controls.Add(this.lblTypeBadge);
            this.flpBadges.Controls.Add(this.lblCategoryBadge);
            this.flpBadges.Controls.Add(this.lblTitleCategory);
            this.flpBadges.Controls.Add(this.lblStatusBadge);
            this.flpBadges.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpBadges.Location = new System.Drawing.Point(10, 8);
            this.flpBadges.Name = "flpBadges";
            this.flpBadges.Size = new System.Drawing.Size(378, 28);
            this.flpBadges.TabIndex = 0;
            this.flpBadges.WrapContents = false;
            // 
            // lblTypeBadge
            // 
            this.lblTypeBadge.AutoSize = true;
            this.lblTypeBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.lblTypeBadge.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblTypeBadge.ForeColor = System.Drawing.Color.White;
            this.lblTypeBadge.Location = new System.Drawing.Point(0, 4);
            this.lblTypeBadge.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblTypeBadge.Name = "lblTypeBadge";
            this.lblTypeBadge.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.lblTypeBadge.Size = new System.Drawing.Size(47, 23);
            this.lblTypeBadge.TabIndex = 0;
            this.lblTypeBadge.Text = "SOP";
            // 
            // lblCategoryBadge
            // 
            this.lblCategoryBadge.AutoSize = true;
            this.lblCategoryBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.lblCategoryBadge.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCategoryBadge.ForeColor = System.Drawing.Color.White;
            this.lblCategoryBadge.Location = new System.Drawing.Point(53, 4);
            this.lblCategoryBadge.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblCategoryBadge.Name = "lblCategoryBadge";
            this.lblCategoryBadge.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.lblCategoryBadge.Size = new System.Drawing.Size(25, 23);
            this.lblCategoryBadge.TabIndex = 1;
            this.lblCategoryBadge.Text = "-";
            // 
            // lblTitleCategory
            // 
            this.lblTitleCategory.AutoSize = true;
            this.lblTitleCategory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.lblTitleCategory.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblTitleCategory.ForeColor = System.Drawing.Color.White;
            this.lblTitleCategory.Location = new System.Drawing.Point(84, 4);
            this.lblTitleCategory.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblTitleCategory.Name = "lblTitleCategory";
            this.lblTitleCategory.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.lblTitleCategory.Size = new System.Drawing.Size(25, 23);
            this.lblTitleCategory.TabIndex = 2;
            this.lblTitleCategory.Text = "-";
            // 
            // lblStatusBadge
            // 
            this.lblStatusBadge.AutoSize = true;
            this.lblStatusBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(245)))), ((int)(((byte)(227)))));
            this.lblStatusBadge.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(132)))), ((int)(((byte)(73)))));
            this.lblStatusBadge.Location = new System.Drawing.Point(115, 4);
            this.lblStatusBadge.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblStatusBadge.Name = "lblStatusBadge";
            this.lblStatusBadge.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.lblStatusBadge.Size = new System.Drawing.Size(108, 23);
            this.lblStatusBadge.TabIndex = 3;
            this.lblStatusBadge.Text = "✅ Published";
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.AutoScroll = true;
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.pnlSidebar.Controls.Add(this.pnlMappingSection);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(696, 591);
            this.pnlSidebar.TabIndex = 0;
            // 
            // pnlMappingSection
            // 
            this.pnlMappingSection.BackColor = System.Drawing.Color.White;
            this.pnlMappingSection.Controls.Add(this.tabMapping);
            this.pnlMappingSection.Controls.Add(this.pnlMappingHeader);
            this.pnlMappingSection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMappingSection.Location = new System.Drawing.Point(0, 0);
            this.pnlMappingSection.Name = "pnlMappingSection";
            this.pnlMappingSection.Size = new System.Drawing.Size(696, 591);
            this.pnlMappingSection.TabIndex = 0;
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
            this.tabMapping.Location = new System.Drawing.Point(0, 36);
            this.tabMapping.Name = "tabMapping";
            this.tabMapping.SelectedIndex = 0;
            this.tabMapping.Size = new System.Drawing.Size(696, 555);
            this.tabMapping.TabIndex = 1;
            this.tabMapping.SelectedIndexChanged += new System.EventHandler(this.TabMapping_SelectedIndexChanged);
            // 
            // tabAttach
            // 
            this.tabAttach.AutoScroll = true;
            this.tabAttach.BackColor = System.Drawing.Color.White;
            this.tabAttach.Controls.Add(this.dgvAttachLinked);
            this.tabAttach.Controls.Add(this.lblAttachLinked);
            this.tabAttach.Location = new System.Drawing.Point(4, 29);
            this.tabAttach.Name = "tabAttach";
            this.tabAttach.Padding = new System.Windows.Forms.Padding(4);
            this.tabAttach.Size = new System.Drawing.Size(688, 522);
            this.tabAttach.TabIndex = 0;
            this.tabAttach.Text = "📎 ไฟล์แนบ";
            // 
            // dgvAttachLinked
            // 
            this.dgvAttachLinked.AllowUserToAddRows = false;
            this.dgvAttachLinked.AllowUserToDeleteRows = false;
            this.dgvAttachLinked.AllowUserToResizeRows = false;
            this.dgvAttachLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvAttachLinked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvAttachLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAttachLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAttachNum,
            this.colAttachName,
            this.colAttachSize,
            this.colAttachDate,
            this.colAttachView});
            this.dgvAttachLinked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAttachLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvAttachLinked.Name = "dgvAttachLinked";
            this.dgvAttachLinked.ReadOnly = true;
            this.dgvAttachLinked.RowHeadersVisible = false;
            this.dgvAttachLinked.RowHeadersWidth = 51;
            this.dgvAttachLinked.RowTemplate.Height = 28;
            this.dgvAttachLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAttachLinked.Size = new System.Drawing.Size(680, 490);
            this.dgvAttachLinked.TabIndex = 1;
            this.dgvAttachLinked.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvAttachLinked_CellContentClick);
            // 
            // colAttachNum
            // 
            this.colAttachNum.HeaderText = "#";
            this.colAttachNum.MinimumWidth = 6;
            this.colAttachNum.Name = "colAttachNum";
            this.colAttachNum.ReadOnly = true;
            this.colAttachNum.Width = 40;
            // 
            // colAttachName
            // 
            this.colAttachName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colAttachName.HeaderText = "ชื่อไฟล์";
            this.colAttachName.MinimumWidth = 6;
            this.colAttachName.Name = "colAttachName";
            this.colAttachName.ReadOnly = true;
            // 
            // colAttachSize
            // 
            this.colAttachSize.HeaderText = "ขนาด";
            this.colAttachSize.MinimumWidth = 6;
            this.colAttachSize.Name = "colAttachSize";
            this.colAttachSize.ReadOnly = true;
            this.colAttachSize.Width = 80;
            // 
            // colAttachDate
            // 
            this.colAttachDate.HeaderText = "วันที่แนบ";
            this.colAttachDate.MinimumWidth = 6;
            this.colAttachDate.Name = "colAttachDate";
            this.colAttachDate.ReadOnly = true;
            this.colAttachDate.Width = 110;
            // 
            // colAttachView
            // 
            this.colAttachView.HeaderText = "ดูไฟล์";
            this.colAttachView.MinimumWidth = 6;
            this.colAttachView.Name = "colAttachView";
            this.colAttachView.ReadOnly = true;
            this.colAttachView.Text = "👁️ ดู";
            this.colAttachView.UseColumnTextForButtonValue = true;
            this.colAttachView.Width = 80;
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
            this.lblAttachLinked.Size = new System.Drawing.Size(680, 24);
            this.lblAttachLinked.TabIndex = 0;
            this.lblAttachLinked.Text = "ไฟล์ที่แนบ (0 ไฟล์)";
            // 
            // tabRelated
            // 
            this.tabRelated.AutoScroll = true;
            this.tabRelated.BackColor = System.Drawing.Color.White;
            this.tabRelated.Controls.Add(this.dgvRelatedLinked);
            this.tabRelated.Controls.Add(this.lblRelatedLinked);
            this.tabRelated.Location = new System.Drawing.Point(4, 29);
            this.tabRelated.Name = "tabRelated";
            this.tabRelated.Padding = new System.Windows.Forms.Padding(4);
            this.tabRelated.Size = new System.Drawing.Size(688, 522);
            this.tabRelated.TabIndex = 1;
            this.tabRelated.Text = "🔗 ความรู้ที่เกี่ยวข้อง";
            // 
            // dgvRelatedLinked
            // 
            this.dgvRelatedLinked.AllowUserToAddRows = false;
            this.dgvRelatedLinked.AllowUserToDeleteRows = false;
            this.dgvRelatedLinked.AllowUserToResizeRows = false;
            this.dgvRelatedLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvRelatedLinked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvRelatedLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRelatedLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRelatedNum,
            this.colRelatedCode,
            this.colRelatedTitle,
            this.colRelatedType});
            this.dgvRelatedLinked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRelatedLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvRelatedLinked.Name = "dgvRelatedLinked";
            this.dgvRelatedLinked.ReadOnly = true;
            this.dgvRelatedLinked.RowHeadersVisible = false;
            this.dgvRelatedLinked.RowHeadersWidth = 51;
            this.dgvRelatedLinked.RowTemplate.Height = 28;
            this.dgvRelatedLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRelatedLinked.Size = new System.Drawing.Size(680, 490);
            this.dgvRelatedLinked.TabIndex = 1;
            this.dgvRelatedLinked.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvRelatedLinked_CellClick);
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
            this.lblRelatedLinked.Size = new System.Drawing.Size(680, 24);
            this.lblRelatedLinked.TabIndex = 0;
            this.lblRelatedLinked.Text = "บทความที่เกี่ยวข้อง (0 รายการ)";
            // 
            // tabWorkflow
            // 
            this.tabWorkflow.AutoScroll = true;
            this.tabWorkflow.BackColor = System.Drawing.Color.White;
            this.tabWorkflow.Controls.Add(this.dgvWorkflowLinked);
            this.tabWorkflow.Controls.Add(this.btnStartWorkflow);
            this.tabWorkflow.Controls.Add(this.lblWorkflowLinked);
            this.tabWorkflow.Location = new System.Drawing.Point(4, 29);
            this.tabWorkflow.Name = "tabWorkflow";
            this.tabWorkflow.Padding = new System.Windows.Forms.Padding(4);
            this.tabWorkflow.Size = new System.Drawing.Size(688, 522);
            this.tabWorkflow.TabIndex = 2;
            this.tabWorkflow.Text = "⚖️ เส้นงาน";
            // 
            // dgvWorkflowLinked
            // 
            this.dgvWorkflowLinked.AllowUserToAddRows = false;
            this.dgvWorkflowLinked.AllowUserToDeleteRows = false;
            this.dgvWorkflowLinked.AllowUserToResizeRows = false;
            this.dgvWorkflowLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvWorkflowLinked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvWorkflowLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWorkflowLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colWfNum,
            this.colWfCode,
            this.colWfName,
            this.colWfStatus});
            this.dgvWorkflowLinked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvWorkflowLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvWorkflowLinked.Name = "dgvWorkflowLinked";
            this.dgvWorkflowLinked.ReadOnly = true;
            this.dgvWorkflowLinked.RowHeadersVisible = false;
            this.dgvWorkflowLinked.RowHeadersWidth = 51;
            this.dgvWorkflowLinked.RowTemplate.Height = 28;
            this.dgvWorkflowLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvWorkflowLinked.Size = new System.Drawing.Size(680, 454);
            this.dgvWorkflowLinked.TabIndex = 1;
            // 
            // colWfNum
            // 
            this.colWfNum.HeaderText = "#";
            this.colWfNum.MinimumWidth = 6;
            this.colWfNum.Name = "colWfNum";
            this.colWfNum.ReadOnly = true;
            this.colWfNum.Width = 40;
            // 
            // colWfCode
            // 
            this.colWfCode.HeaderText = "รหัส";
            this.colWfCode.MinimumWidth = 6;
            this.colWfCode.Name = "colWfCode";
            this.colWfCode.ReadOnly = true;
            this.colWfCode.Width = 110;
            // 
            // colWfName
            // 
            this.colWfName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colWfName.HeaderText = "ชื่อ Workflow";
            this.colWfName.MinimumWidth = 6;
            this.colWfName.Name = "colWfName";
            this.colWfName.ReadOnly = true;
            // 
            // colWfStatus
            // 
            this.colWfStatus.HeaderText = "สถานะ";
            this.colWfStatus.MinimumWidth = 6;
            this.colWfStatus.Name = "colWfStatus";
            this.colWfStatus.ReadOnly = true;
            this.colWfStatus.Width = 80;
            // 
            // btnStartWorkflow
            // 
            this.btnStartWorkflow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnStartWorkflow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartWorkflow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnStartWorkflow.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.btnStartWorkflow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartWorkflow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnStartWorkflow.ForeColor = System.Drawing.Color.White;
            this.btnStartWorkflow.Location = new System.Drawing.Point(4, 482);
            this.btnStartWorkflow.Name = "btnStartWorkflow";
            this.btnStartWorkflow.Size = new System.Drawing.Size(680, 36);
            this.btnStartWorkflow.TabIndex = 2;
            this.btnStartWorkflow.Text = "▶️ เริ่ม Workflow";
            this.btnStartWorkflow.UseVisualStyleBackColor = false;
            this.btnStartWorkflow.Click += new System.EventHandler(this.BtnStartWorkflow_Click);
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
            this.lblWorkflowLinked.Size = new System.Drawing.Size(680, 24);
            this.lblWorkflowLinked.TabIndex = 0;
            this.lblWorkflowLinked.Text = "เส้นงานที่เกี่ยวข้อง (0 รายการ)";
            // 
            // tabPrograms
            // 
            this.tabPrograms.AutoScroll = true;
            this.tabPrograms.BackColor = System.Drawing.Color.White;
            this.tabPrograms.Controls.Add(this.dgvProgramsLinked);
            this.tabPrograms.Controls.Add(this.lblProgramsLinked);
            this.tabPrograms.Location = new System.Drawing.Point(4, 29);
            this.tabPrograms.Name = "tabPrograms";
            this.tabPrograms.Padding = new System.Windows.Forms.Padding(4);
            this.tabPrograms.Size = new System.Drawing.Size(688, 522);
            this.tabPrograms.TabIndex = 3;
            this.tabPrograms.Text = "💻 โปรแกรม";
            // 
            // dgvProgramsLinked
            // 
            this.dgvProgramsLinked.AllowUserToAddRows = false;
            this.dgvProgramsLinked.AllowUserToDeleteRows = false;
            this.dgvProgramsLinked.AllowUserToResizeRows = false;
            this.dgvProgramsLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvProgramsLinked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvProgramsLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProgramsLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colProgNum,
            this.colProgCode,
            this.colProgName,
            this.colProgDept});
            this.dgvProgramsLinked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProgramsLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvProgramsLinked.Name = "dgvProgramsLinked";
            this.dgvProgramsLinked.ReadOnly = true;
            this.dgvProgramsLinked.RowHeadersVisible = false;
            this.dgvProgramsLinked.RowHeadersWidth = 51;
            this.dgvProgramsLinked.RowTemplate.Height = 28;
            this.dgvProgramsLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProgramsLinked.Size = new System.Drawing.Size(680, 490);
            this.dgvProgramsLinked.TabIndex = 1;
            // 
            // colProgNum
            // 
            this.colProgNum.HeaderText = "#";
            this.colProgNum.MinimumWidth = 6;
            this.colProgNum.Name = "colProgNum";
            this.colProgNum.ReadOnly = true;
            this.colProgNum.Width = 40;
            // 
            // colProgCode
            // 
            this.colProgCode.HeaderText = "รหัส";
            this.colProgCode.MinimumWidth = 6;
            this.colProgCode.Name = "colProgCode";
            this.colProgCode.ReadOnly = true;
            this.colProgCode.Width = 110;
            // 
            // colProgName
            // 
            this.colProgName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colProgName.HeaderText = "ชื่อโปรแกรม";
            this.colProgName.MinimumWidth = 6;
            this.colProgName.Name = "colProgName";
            this.colProgName.ReadOnly = true;
            // 
            // colProgDept
            // 
            this.colProgDept.HeaderText = "ฝ่าย/แผนก";
            this.colProgDept.MinimumWidth = 6;
            this.colProgDept.Name = "colProgDept";
            this.colProgDept.ReadOnly = true;
            this.colProgDept.Width = 160;
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
            this.lblProgramsLinked.Size = new System.Drawing.Size(680, 24);
            this.lblProgramsLinked.TabIndex = 0;
            this.lblProgramsLinked.Text = "โปรแกรมที่เกี่ยวข้อง (0 รายการ)";
            // 
            // tabDocs
            // 
            this.tabDocs.AutoScroll = true;
            this.tabDocs.BackColor = System.Drawing.Color.White;
            this.tabDocs.Controls.Add(this.dgvDocsLinked);
            this.tabDocs.Controls.Add(this.lblDocsLinked);
            this.tabDocs.Location = new System.Drawing.Point(4, 29);
            this.tabDocs.Name = "tabDocs";
            this.tabDocs.Padding = new System.Windows.Forms.Padding(4);
            this.tabDocs.Size = new System.Drawing.Size(688, 522);
            this.tabDocs.TabIndex = 4;
            this.tabDocs.Text = "📋 เอกสาร";
            // 
            // dgvDocsLinked
            // 
            this.dgvDocsLinked.AllowUserToAddRows = false;
            this.dgvDocsLinked.AllowUserToDeleteRows = false;
            this.dgvDocsLinked.AllowUserToResizeRows = false;
            this.dgvDocsLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvDocsLinked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDocsLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDocsLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDocNum,
            this.colDocName,
            this.colDocRev,
            this.colDocCraf});
            this.dgvDocsLinked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDocsLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvDocsLinked.Name = "dgvDocsLinked";
            this.dgvDocsLinked.ReadOnly = true;
            this.dgvDocsLinked.RowHeadersVisible = false;
            this.dgvDocsLinked.RowHeadersWidth = 51;
            this.dgvDocsLinked.RowTemplate.Height = 28;
            this.dgvDocsLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDocsLinked.Size = new System.Drawing.Size(680, 490);
            this.dgvDocsLinked.TabIndex = 1;
            // 
            // colDocNum
            // 
            this.colDocNum.HeaderText = "#";
            this.colDocNum.MinimumWidth = 6;
            this.colDocNum.Name = "colDocNum";
            this.colDocNum.ReadOnly = true;
            this.colDocNum.Width = 40;
            // 
            // colDocName
            // 
            this.colDocName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDocName.HeaderText = "ชื่อเอกสาร";
            this.colDocName.MinimumWidth = 6;
            this.colDocName.Name = "colDocName";
            this.colDocName.ReadOnly = true;
            // 
            // colDocRev
            // 
            this.colDocRev.HeaderText = "Rev";
            this.colDocRev.MinimumWidth = 6;
            this.colDocRev.Name = "colDocRev";
            this.colDocRev.ReadOnly = true;
            this.colDocRev.Width = 55;
            // 
            // colDocCraf
            // 
            this.colDocCraf.HeaderText = "CRAF";
            this.colDocCraf.MinimumWidth = 6;
            this.colDocCraf.Name = "colDocCraf";
            this.colDocCraf.ReadOnly = true;
            this.colDocCraf.Width = 55;
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
            this.lblDocsLinked.Size = new System.Drawing.Size(680, 24);
            this.lblDocsLinked.TabIndex = 0;
            this.lblDocsLinked.Text = "เอกสารที่เกี่ยวข้อง (0 รายการ)";
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
            this.tabDepartments.Size = new System.Drawing.Size(688, 522);
            this.tabDepartments.TabIndex = 5;
            this.tabDepartments.Text = "🏢 ฝ่าย/แผนก ที่เกี่ยวข้อง";
            // 
            // dgvDepartmentsLinked
            // 
            this.dgvDepartmentsLinked.AllowUserToAddRows = false;
            this.dgvDepartmentsLinked.AllowUserToDeleteRows = false;
            this.dgvDepartmentsLinked.AllowUserToResizeRows = false;
            this.dgvDepartmentsLinked.BackgroundColor = System.Drawing.Color.White;
            this.dgvDepartmentsLinked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDepartmentsLinked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDepartmentsLinked.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDeptNum,
            this.colDeptCode,
            this.colDeptName,
            this.colDeptType});
            this.dgvDepartmentsLinked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDepartmentsLinked.Location = new System.Drawing.Point(4, 28);
            this.dgvDepartmentsLinked.Name = "dgvDepartmentsLinked";
            this.dgvDepartmentsLinked.ReadOnly = true;
            this.dgvDepartmentsLinked.RowHeadersVisible = false;
            this.dgvDepartmentsLinked.RowHeadersWidth = 51;
            this.dgvDepartmentsLinked.RowTemplate.Height = 28;
            this.dgvDepartmentsLinked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDepartmentsLinked.Size = new System.Drawing.Size(680, 490);
            this.dgvDepartmentsLinked.TabIndex = 1;
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
            this.colDeptName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDeptName.HeaderText = "ชื่อฝ่าย/แผนก";
            this.colDeptName.MinimumWidth = 6;
            this.colDeptName.Name = "colDeptName";
            this.colDeptName.ReadOnly = true;
            // 
            // colDeptType
            // 
            this.colDeptType.HeaderText = "ประเภท";
            this.colDeptType.MinimumWidth = 6;
            this.colDeptType.Name = "colDeptType";
            this.colDeptType.ReadOnly = true;
            this.colDeptType.Width = 120;
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
            this.lblDepartmentsLinked.Size = new System.Drawing.Size(680, 24);
            this.lblDepartmentsLinked.TabIndex = 0;
            this.lblDepartmentsLinked.Text = "ฝ่าย/แผนก ที่เกี่ยวข้อง (0 รายการ)";
            // 
            // pnlMappingHeader
            // 
            this.pnlMappingHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.pnlMappingHeader.Controls.Add(this.lblMappingTitle);
            this.pnlMappingHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMappingHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlMappingHeader.Name = "pnlMappingHeader";
            this.pnlMappingHeader.Padding = new System.Windows.Forms.Padding(10, 8, 10, 6);
            this.pnlMappingHeader.Size = new System.Drawing.Size(696, 36);
            this.pnlMappingHeader.TabIndex = 0;
            // 
            // lblMappingTitle
            // 
            this.lblMappingTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.lblMappingTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMappingTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMappingTitle.ForeColor = System.Drawing.Color.White;
            this.lblMappingTitle.Location = new System.Drawing.Point(10, 8);
            this.lblMappingTitle.Name = "lblMappingTitle";
            this.lblMappingTitle.Size = new System.Drawing.Size(676, 22);
            this.lblMappingTitle.TabIndex = 0;
            this.lblMappingTitle.Text = "🔗 MAPPING รายการที่เกี่ยวข้อง";
            this.lblMappingTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOfficialBadge
            // 
            this.lblOfficialBadge.AutoSize = true;
            this.lblOfficialBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblOfficialBadge.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblOfficialBadge.ForeColor = System.Drawing.Color.White;
            this.lblOfficialBadge.Location = new System.Drawing.Point(0, 0);
            this.lblOfficialBadge.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblOfficialBadge.Name = "lblOfficialBadge";
            this.lblOfficialBadge.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.lblOfficialBadge.Size = new System.Drawing.Size(100, 23);
            this.lblOfficialBadge.TabIndex = 0;
            this.lblOfficialBadge.Text = "🏛 นโยบายทางการ";
            this.lblOfficialBadge.Visible = false;
            // 
            // pnlVersionList
            // 
            this.pnlVersionList.BackColor = System.Drawing.Color.White;
            this.pnlVersionList.Location = new System.Drawing.Point(0, 0);
            this.pnlVersionList.Name = "pnlVersionList";
            this.pnlVersionList.Size = new System.Drawing.Size(200, 100);
            this.pnlVersionList.TabIndex = 0;
            // 
            // colRelatedNum
            // 
            this.colRelatedNum.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colRelatedNum.FillWeight = 200F;
            this.colRelatedNum.HeaderText = "#";
            this.colRelatedNum.MinimumWidth = 6;
            this.colRelatedNum.Name = "colRelatedNum";
            this.colRelatedNum.ReadOnly = true;
            this.colRelatedNum.Width = 47;
            // 
            // colRelatedCode
            // 
            this.colRelatedCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colRelatedCode.HeaderText = "รหัส";
            this.colRelatedCode.MinimumWidth = 6;
            this.colRelatedCode.Name = "colRelatedCode";
            this.colRelatedCode.ReadOnly = true;
            // 
            // colRelatedTitle
            // 
            this.colRelatedTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colRelatedTitle.HeaderText = "ชื่อบทความ";
            this.colRelatedTitle.MinimumWidth = 6;
            this.colRelatedTitle.Name = "colRelatedTitle";
            this.colRelatedTitle.ReadOnly = true;
            // 
            // colRelatedType
            // 
            this.colRelatedType.HeaderText = "ประเภท";
            this.colRelatedType.MinimumWidth = 6;
            this.colRelatedType.Name = "colRelatedType";
            this.colRelatedType.ReadOnly = true;
            this.colRelatedType.Width = 90;
            // 
            // ArticleViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1100, 680);
            this.Controls.Add(this.splitViewer);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(720, 420);
            this.Name = "ArticleViewerForm";
            this.Text = "ดูบทความ — KMS";
            this.Load += new System.EventHandler(this.ArticleViewerForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbarSep.ResumeLayout(false);
            this.splitViewer.Panel1.ResumeLayout(false);
            this.splitViewer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitViewer)).EndInit();
            this.splitViewer.ResumeLayout(false);
            this.pnlZoomStatus.ResumeLayout(false);
            this.pnlInfoWrapper.ResumeLayout(false);
            this.pnlArticleInfo.ResumeLayout(false);
            this.tblMeta.ResumeLayout(false);
            this.flpBadges.ResumeLayout(false);
            this.flpBadges.PerformLayout();
            this.pnlSidebar.ResumeLayout(false);
            this.pnlMappingSection.ResumeLayout(false);
            this.tabMapping.ResumeLayout(false);
            this.tabAttach.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttachLinked)).EndInit();
            this.tabRelated.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelatedLinked)).EndInit();
            this.tabWorkflow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorkflowLinked)).EndInit();
            this.tabPrograms.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProgramsLinked)).EndInit();
            this.tabDocs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocsLinked)).EndInit();
            this.tabDepartments.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDepartmentsLinked)).EndInit();
            this.pnlMappingHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel            pnlHeader;
        private System.Windows.Forms.Label            lblViewerTitle;
        private System.Windows.Forms.Panel            pnlToolbar;
        private System.Windows.Forms.Button           btnEdit;
        private System.Windows.Forms.Button           btnShare;
        private System.Windows.Forms.Button           btnPrint;
        private System.Windows.Forms.Button           btnExportPdf;
        private System.Windows.Forms.Button           btnPin;
        private System.Windows.Forms.Panel            pnlToolbarSep;
        private System.Windows.Forms.Button           btnBack;
        private System.Windows.Forms.SplitContainer   splitViewer;
        private System.Windows.Forms.Panel            pnlInfoWrapper;
        private System.Windows.Forms.Panel            pnlArticleInfo;
        private System.Windows.Forms.FlowLayoutPanel  flpBadges;
        private System.Windows.Forms.Label            lblTypeBadge;
        private System.Windows.Forms.Label            lblCategoryBadge;
        private System.Windows.Forms.Label            lblTitleCategory;
        private System.Windows.Forms.Label            lblOfficialBadge;
        private System.Windows.Forms.TableLayoutPanel tblMeta;
        private System.Windows.Forms.Label            lblCodeVersion;
        private System.Windows.Forms.Label            lblAuthorVal;
        private System.Windows.Forms.Label            lblCategoryVal;
        private System.Windows.Forms.Label            lblPublishVal;
        private System.Windows.Forms.Label            lblConfidential;
        private System.Windows.Forms.Label            lblReviewDueVal;
        private System.Windows.Forms.WebBrowser       webContent;
        private System.Windows.Forms.Panel            pnlZoomStatus;
        private System.Windows.Forms.Label            lblZoomIndicator;
        private System.Windows.Forms.Panel            pnlSidebar;
        private System.Windows.Forms.Panel            pnlVersionList;
        private System.Windows.Forms.Panel            panel1;
        private System.Windows.Forms.Panel            pnlMappingSection;
        private System.Windows.Forms.Panel            pnlMappingHeader;
        private System.Windows.Forms.Label            lblMappingTitle;
        // ── TabControl ────────────────────────────────────────────────────────
        private System.Windows.Forms.TabControl       tabMapping;
        private System.Windows.Forms.TabPage          tabAttach;
        private System.Windows.Forms.TabPage          tabRelated;
        private System.Windows.Forms.TabPage          tabWorkflow;
        private System.Windows.Forms.TabPage          tabPrograms;
        private System.Windows.Forms.TabPage          tabDocs;
        private System.Windows.Forms.TabPage          tabDepartments;
        // Attachments tab
        private System.Windows.Forms.DataGridView     dgvAttachLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAttachNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAttachName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAttachSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAttachDate;
        private System.Windows.Forms.DataGridViewButtonColumn colAttachView;
        private System.Windows.Forms.Label            lblAttachLinked;
        // Related tab
        private System.Windows.Forms.DataGridView     dgvRelatedLinked;
        private System.Windows.Forms.Label            lblRelatedLinked;
        // Workflow tab
        private System.Windows.Forms.DataGridView     dgvWorkflowLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWfNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWfCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWfName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWfStatus;
        private System.Windows.Forms.Label            lblWorkflowLinked;
        private System.Windows.Forms.Button           btnStartWorkflow;
        // Programs tab
        private System.Windows.Forms.DataGridView     dgvProgramsLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProgDept;
        private System.Windows.Forms.Label            lblProgramsLinked;
        // Docs tab
        private System.Windows.Forms.DataGridView     dgvDocsLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocRev;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDocCraf;
        private System.Windows.Forms.Label            lblDocsLinked;
        // Departments tab
        private System.Windows.Forms.DataGridView     dgvDepartmentsLinked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptType;
        private System.Windows.Forms.Label            lblDepartmentsLinked;
        private System.Windows.Forms.Label lblStatusBadge;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelatedType;
    }
}
