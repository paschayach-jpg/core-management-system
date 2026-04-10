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
            this.pnlToolbarSep = new System.Windows.Forms.Panel();
            this.btnPin = new System.Windows.Forms.Button();
            this.btnExportPdf = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnShare = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.splitViewer = new System.Windows.Forms.SplitContainer();
            this.webContent = new System.Windows.Forms.WebBrowser();
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
            this.lblStatusBadge = new System.Windows.Forms.Label();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlMappingSection = new System.Windows.Forms.Panel();
            this.pnlTabContent = new System.Windows.Forms.Panel();
            this.pnlTabHeaders = new System.Windows.Forms.Panel();
            this.btnTabDoc = new System.Windows.Forms.Button();
            this.btnTabProgram = new System.Windows.Forms.Button();
            this.btnTabWorkflow = new System.Windows.Forms.Button();
            this.btnTabRelated = new System.Windows.Forms.Button();
            this.btnTabAttach = new System.Windows.Forms.Button();
            this.pnlMappingHeader = new System.Windows.Forms.Panel();
            this.lblMappingTitle = new System.Windows.Forms.Label();
            this.lblOfficialBadge = new System.Windows.Forms.Label();
            this.pnlVersionList = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlToolbarSep.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitViewer)).BeginInit();
            this.splitViewer.Panel1.SuspendLayout();
            this.splitViewer.Panel2.SuspendLayout();
            this.splitViewer.SuspendLayout();
            this.pnlInfoWrapper.SuspendLayout();
            this.pnlArticleInfo.SuspendLayout();
            this.tblMeta.SuspendLayout();
            this.flpBadges.SuspendLayout();
            this.pnlSidebar.SuspendLayout();
            this.pnlMappingSection.SuspendLayout();
            this.pnlTabHeaders.SuspendLayout();
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
            this.webContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webContent.IsWebBrowserContextMenuEnabled = false;
            this.webContent.Location = new System.Drawing.Point(0, 106);
            this.webContent.Name = "webContent";
            this.webContent.ScriptErrorsSuppressed = true;
            this.webContent.Size = new System.Drawing.Size(400, 485);
            this.webContent.TabIndex = 1;
            this.webContent.WebBrowserShortcutsEnabled = false;
            this.webContent.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webContent_DocumentCompleted);
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
            // lblStatusBadge
            // 
            this.lblStatusBadge.AutoSize = true;
            this.lblStatusBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(245)))), ((int)(((byte)(227)))));
            this.lblStatusBadge.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(132)))), ((int)(((byte)(73)))));
            this.lblStatusBadge.Location = new System.Drawing.Point(84, 4);
            this.lblStatusBadge.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblStatusBadge.Name = "lblStatusBadge";
            this.lblStatusBadge.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.lblStatusBadge.Size = new System.Drawing.Size(108, 23);
            this.lblStatusBadge.TabIndex = 2;
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
            this.pnlMappingSection.Controls.Add(this.pnlTabContent);
            this.pnlMappingSection.Controls.Add(this.pnlTabHeaders);
            this.pnlMappingSection.Controls.Add(this.pnlMappingHeader);
            this.pnlMappingSection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMappingSection.Location = new System.Drawing.Point(0, 0);
            this.pnlMappingSection.Name = "pnlMappingSection";
            this.pnlMappingSection.Size = new System.Drawing.Size(696, 591);
            this.pnlMappingSection.TabIndex = 0;
            // 
            // pnlTabContent
            // 
            this.pnlTabContent.AutoScroll = true;
            this.pnlTabContent.BackColor = System.Drawing.Color.White;
            this.pnlTabContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTabContent.Location = new System.Drawing.Point(0, 72);
            this.pnlTabContent.Name = "pnlTabContent";
            this.pnlTabContent.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.pnlTabContent.Size = new System.Drawing.Size(696, 519);
            this.pnlTabContent.TabIndex = 2;
            // 
            // pnlTabHeaders
            // 
            this.pnlTabHeaders.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.pnlTabHeaders.Controls.Add(this.btnTabDoc);
            this.pnlTabHeaders.Controls.Add(this.btnTabProgram);
            this.pnlTabHeaders.Controls.Add(this.btnTabWorkflow);
            this.pnlTabHeaders.Controls.Add(this.btnTabRelated);
            this.pnlTabHeaders.Controls.Add(this.btnTabAttach);
            this.pnlTabHeaders.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTabHeaders.Location = new System.Drawing.Point(0, 36);
            this.pnlTabHeaders.Name = "pnlTabHeaders";
            this.pnlTabHeaders.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.pnlTabHeaders.Size = new System.Drawing.Size(696, 36);
            this.pnlTabHeaders.TabIndex = 1;
            // 
            // btnTabDoc
            // 
            this.btnTabDoc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.btnTabDoc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabDoc.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnTabDoc.FlatAppearance.BorderSize = 0;
            this.btnTabDoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabDoc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnTabDoc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnTabDoc.Location = new System.Drawing.Point(446, 0);
            this.btnTabDoc.Name = "btnTabDoc";
            this.btnTabDoc.Padding = new System.Windows.Forms.Padding(4, 0, 4, 2);
            this.btnTabDoc.Size = new System.Drawing.Size(110, 36);
            this.btnTabDoc.TabIndex = 4;
            this.btnTabDoc.Text = "📋 เอกสาร (1)";
            this.btnTabDoc.UseVisualStyleBackColor = false;
            this.btnTabDoc.Click += new System.EventHandler(this.BtnTab_Click);
            // 
            // btnTabProgram
            // 
            this.btnTabProgram.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.btnTabProgram.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabProgram.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnTabProgram.FlatAppearance.BorderSize = 0;
            this.btnTabProgram.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabProgram.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnTabProgram.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnTabProgram.Location = new System.Drawing.Point(326, 0);
            this.btnTabProgram.Name = "btnTabProgram";
            this.btnTabProgram.Padding = new System.Windows.Forms.Padding(4, 0, 4, 2);
            this.btnTabProgram.Size = new System.Drawing.Size(120, 36);
            this.btnTabProgram.TabIndex = 3;
            this.btnTabProgram.Text = "💻 โปรแกรม (5)";
            this.btnTabProgram.UseVisualStyleBackColor = false;
            this.btnTabProgram.Click += new System.EventHandler(this.BtnTab_Click);
            // 
            // btnTabWorkflow
            // 
            this.btnTabWorkflow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.btnTabWorkflow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabWorkflow.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnTabWorkflow.FlatAppearance.BorderSize = 0;
            this.btnTabWorkflow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabWorkflow.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnTabWorkflow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnTabWorkflow.Location = new System.Drawing.Point(216, 0);
            this.btnTabWorkflow.Name = "btnTabWorkflow";
            this.btnTabWorkflow.Padding = new System.Windows.Forms.Padding(4, 0, 4, 2);
            this.btnTabWorkflow.Size = new System.Drawing.Size(110, 36);
            this.btnTabWorkflow.TabIndex = 2;
            this.btnTabWorkflow.Text = "⚖️ เส้นงาน (2)";
            this.btnTabWorkflow.UseVisualStyleBackColor = false;
            this.btnTabWorkflow.Click += new System.EventHandler(this.BtnTab_Click);
            // 
            // btnTabRelated
            // 
            this.btnTabRelated.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.btnTabRelated.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabRelated.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnTabRelated.FlatAppearance.BorderSize = 0;
            this.btnTabRelated.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabRelated.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnTabRelated.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.btnTabRelated.Location = new System.Drawing.Point(106, 0);
            this.btnTabRelated.Name = "btnTabRelated";
            this.btnTabRelated.Padding = new System.Windows.Forms.Padding(4, 0, 4, 2);
            this.btnTabRelated.Size = new System.Drawing.Size(110, 36);
            this.btnTabRelated.TabIndex = 1;
            this.btnTabRelated.Text = "🔗 ความรู้ (3)";
            this.btnTabRelated.UseVisualStyleBackColor = false;
            this.btnTabRelated.Click += new System.EventHandler(this.BtnTab_Click);
            // 
            // btnTabAttach
            // 
            this.btnTabAttach.BackColor = System.Drawing.Color.White;
            this.btnTabAttach.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabAttach.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnTabAttach.FlatAppearance.BorderSize = 0;
            this.btnTabAttach.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabAttach.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnTabAttach.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnTabAttach.Location = new System.Drawing.Point(6, 0);
            this.btnTabAttach.Name = "btnTabAttach";
            this.btnTabAttach.Padding = new System.Windows.Forms.Padding(4, 0, 4, 2);
            this.btnTabAttach.Size = new System.Drawing.Size(100, 36);
            this.btnTabAttach.TabIndex = 0;
            this.btnTabAttach.Text = "📎 ไฟล์แนบ (2)";
            this.btnTabAttach.UseVisualStyleBackColor = false;
            this.btnTabAttach.Click += new System.EventHandler(this.BtnTab_Click);
            // 
            // pnlMappingHeader
            // 
            this.pnlMappingHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
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
            this.lblMappingTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMappingTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMappingTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
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
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(88, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(484, 45);
            this.panel1.TabIndex = 100;
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
            this.pnlInfoWrapper.ResumeLayout(false);
            this.pnlArticleInfo.ResumeLayout(false);
            this.tblMeta.ResumeLayout(false);
            this.flpBadges.ResumeLayout(false);
            this.flpBadges.PerformLayout();
            this.pnlSidebar.ResumeLayout(false);
            this.pnlMappingSection.ResumeLayout(false);
            this.pnlTabHeaders.ResumeLayout(false);
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
        private System.Windows.Forms.Label            lblStatusBadge;
        private System.Windows.Forms.Label            lblOfficialBadge;
        private System.Windows.Forms.TableLayoutPanel tblMeta;
        private System.Windows.Forms.Label            lblCodeVersion;
        private System.Windows.Forms.Label            lblAuthorVal;
        private System.Windows.Forms.Label            lblCategoryVal;
        private System.Windows.Forms.Label            lblPublishVal;
        private System.Windows.Forms.Label            lblConfidential;
        private System.Windows.Forms.Label            lblReviewDueVal;
        private System.Windows.Forms.WebBrowser       webContent;
        private System.Windows.Forms.Panel            pnlSidebar;
        private System.Windows.Forms.Panel            pnlVersionList;
        private System.Windows.Forms.Panel            panel1;
        private System.Windows.Forms.Panel            pnlMappingSection;
        private System.Windows.Forms.Panel            pnlMappingHeader;
        private System.Windows.Forms.Label            lblMappingTitle;
        private System.Windows.Forms.Panel            pnlTabHeaders;
        private System.Windows.Forms.Button           btnTabAttach;
        private System.Windows.Forms.Button           btnTabRelated;
        private System.Windows.Forms.Button           btnTabWorkflow;
        private System.Windows.Forms.Button           btnTabProgram;
        private System.Windows.Forms.Button           btnTabDoc;
        private System.Windows.Forms.Panel            pnlTabContent;
    }
}
