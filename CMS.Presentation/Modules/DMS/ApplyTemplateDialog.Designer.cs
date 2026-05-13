namespace CMS.Presentation
{
    partial class ApplyTemplateDialog
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
            this.components = new System.ComponentModel.Container();
            this.toolTipMain = new System.Windows.Forms.ToolTip(this.components);
            this.toolTipMain.AutoPopDelay = 8000;
            this.toolTipMain.InitialDelay = 400;
            this.toolTipMain.ReshowDelay = 100;
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlFooterLine = new System.Windows.Forms.Panel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.pnlListBox = new System.Windows.Forms.Panel();
            this.lstTemplates = new System.Windows.Forms.ListBox();
            this.lblTemplateDesc = new System.Windows.Forms.Label();
            this.lblTemplate = new System.Windows.Forms.Label();
            this.sepOrgTemplate = new System.Windows.Forms.Panel();
            this.treeOrgUnit = new System.Windows.Forms.TreeView();
            this.lblOrgUnit = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pnlTree = new System.Windows.Forms.Panel();
            this.treePreview = new System.Windows.Forms.TreeView();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.cboConfidentiality = new System.Windows.Forms.ComboBox();
            this.lblConf = new System.Windows.Forms.Label();
            this.sep2 = new System.Windows.Forms.Panel();
            this.txtRootName = new System.Windows.Forms.TextBox();
            this.lblRootName = new System.Windows.Forms.Label();
            this.lblPreview = new System.Windows.Forms.Label();
            this.lblPathBreadcrumb = new System.Windows.Forms.Label();
            this.pnlOrgToolbar = new System.Windows.Forms.Panel();
            this.btnExpandAll = new System.Windows.Forms.Button();
            this.btnCollapseAll = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlOrgToolbar.SuspendLayout();
            this.pnlListBox.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlTree.SuspendLayout();
            this.pnlOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(803, 50);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblHeader.Size = new System.Drawing.Size(803, 50);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "📁  สร้าง Folder อัตโนมัติจาก Template";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlStatus
            // 
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(238)))), ((int)(((byte)(245)))));
            this.pnlStatus.Controls.Add(this.lblStatus);
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.Location = new System.Drawing.Point(0, 512);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(803, 32);
            this.pnlStatus.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblStatus.Location = new System.Drawing.Point(0, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblStatus.Size = new System.Drawing.Size(803, 32);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "เลือกหน่วยงานและ Template เพื่อเริ่มต้น";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Controls.Add(this.btnApply);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Controls.Add(this.pnlFooterLine);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 460);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(0, 8, 12, 8);
            this.pnlFooter.Size = new System.Drawing.Size(803, 52);
            this.pnlFooter.TabIndex = 2;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.btnApply.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApply.FlatAppearance.BorderSize = 0;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnApply.ForeColor = System.Drawing.Color.White;
            this.btnApply.Location = new System.Drawing.Point(553, 9);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(140, 34);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "✅  สร้าง Folder";
            this.btnApply.UseVisualStyleBackColor = false;
            this.btnApply.Click += new System.EventHandler(this.BtnApply_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnCancel.Location = new System.Drawing.Point(701, 9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 34);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // pnlFooterLine
            // 
            this.pnlFooterLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pnlFooterLine.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFooterLine.Location = new System.Drawing.Point(0, 8);
            this.pnlFooterLine.Name = "pnlFooterLine";
            this.pnlFooterLine.Size = new System.Drawing.Size(791, 1);
            this.pnlFooterLine.TabIndex = 2;
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(215)))), ((int)(((byte)(225)))));
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 50);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.BackColor = System.Drawing.Color.White;
            this.splitMain.Panel1.Controls.Add(this.pnlLeft);
            this.splitMain.Panel1MinSize = 220;
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.splitMain.Panel2.Controls.Add(this.pnlRight);
            this.splitMain.Panel2MinSize = 300;
            this.splitMain.Size = new System.Drawing.Size(803, 410);
            this.splitMain.SplitterDistance = 499;
            this.splitMain.TabIndex = 3;
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.pnlListBox);
            this.pnlLeft.Controls.Add(this.lblTemplateDesc);
            this.pnlLeft.Controls.Add(this.lblTemplate);
            this.pnlLeft.Controls.Add(this.sepOrgTemplate);
            this.pnlLeft.Controls.Add(this.treeOrgUnit);
            this.pnlLeft.Controls.Add(this.pnlOrgToolbar);
            this.pnlLeft.Controls.Add(this.lblOrgUnit);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlLeft.Size = new System.Drawing.Size(499, 410);
            this.pnlLeft.TabIndex = 0;
            // 
            // pnlListBox
            // 
            this.pnlListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(252)))));
            this.pnlListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlListBox.Controls.Add(this.lstTemplates);
            this.pnlListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlListBox.Location = new System.Drawing.Point(12, 93);
            this.pnlListBox.Name = "pnlListBox";
            this.pnlListBox.Size = new System.Drawing.Size(475, 271);
            this.pnlListBox.TabIndex = 0;
            // 
            // lstTemplates
            // 
            this.lstTemplates.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(252)))));
            this.lstTemplates.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTemplates.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lstTemplates.ItemHeight = 17;
            this.lstTemplates.Location = new System.Drawing.Point(0, 0);
            this.lstTemplates.Name = "lstTemplates";
            this.lstTemplates.Size = new System.Drawing.Size(473, 269);
            this.lstTemplates.TabIndex = 1;
            this.lstTemplates.SelectedIndexChanged += new System.EventHandler(this.LstTemplates_Changed);
            // 
            // lblTemplateDesc
            // 
            this.lblTemplateDesc.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblTemplateDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblTemplateDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblTemplateDesc.Location = new System.Drawing.Point(12, 364);
            this.lblTemplateDesc.Name = "lblTemplateDesc";
            this.lblTemplateDesc.Padding = new System.Windows.Forms.Padding(2, 4, 2, 0);
            this.lblTemplateDesc.Size = new System.Drawing.Size(475, 36);
            this.lblTemplateDesc.TabIndex = 1;
            // 
            // lblTemplate
            // 
            this.lblTemplate.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTemplate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTemplate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblTemplate.Location = new System.Drawing.Point(12, 69);
            this.lblTemplate.Name = "lblTemplate";
            this.lblTemplate.Size = new System.Drawing.Size(475, 24);
            this.lblTemplate.TabIndex = 2;
            this.lblTemplate.Text = "📋  เลือก Template";
            // 
            // sepOrgTemplate
            // 
            this.sepOrgTemplate.BackColor = System.Drawing.Color.Transparent;
            this.sepOrgTemplate.Dock = System.Windows.Forms.DockStyle.Top;
            this.sepOrgTemplate.Location = new System.Drawing.Point(12, 59);
            this.sepOrgTemplate.Name = "sepOrgTemplate";
            this.sepOrgTemplate.Size = new System.Drawing.Size(475, 10);
            this.sepOrgTemplate.TabIndex = 3;
            // 
            // treeOrgUnit
            // 
            this.treeOrgUnit.Dock            = System.Windows.Forms.DockStyle.Top;
            this.treeOrgUnit.Font            = new System.Drawing.Font("Segoe UI", 9.5f);
            this.treeOrgUnit.Height          = 200;
            this.treeOrgUnit.HideSelection   = false;
            this.treeOrgUnit.FullRowSelect   = true;
            this.treeOrgUnit.ShowLines       = true;
            this.treeOrgUnit.ShowPlusMinus   = true;
            this.treeOrgUnit.Scrollable      = true;
            this.treeOrgUnit.ShowNodeToolTips = true;
            this.treeOrgUnit.BorderStyle     = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeOrgUnit.Name            = "treeOrgUnit";
            this.treeOrgUnit.TabIndex        = 0;
            this.treeOrgUnit.AfterSelect    += new System.Windows.Forms.TreeViewEventHandler(this.TreeOrgUnit_AfterSelect);
            //
            // pnlOrgToolbar
            //
            this.pnlOrgToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(243)))), ((int)(((byte)(250)))));
            this.pnlOrgToolbar.Controls.Add(this.btnCollapseAll);
            this.pnlOrgToolbar.Controls.Add(this.btnExpandAll);
            this.pnlOrgToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOrgToolbar.Height = 28;
            this.pnlOrgToolbar.Name = "pnlOrgToolbar";
            this.pnlOrgToolbar.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.pnlOrgToolbar.TabIndex = 5;
            //
            // btnExpandAll
            //
            this.btnExpandAll.BackColor = System.Drawing.Color.White;
            this.btnExpandAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExpandAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(190)))), ((int)(((byte)(215)))));
            this.btnExpandAll.FlatAppearance.BorderSize = 1;
            this.btnExpandAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpandAll.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnExpandAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.btnExpandAll.Location = new System.Drawing.Point(0, 3);
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(90, 22);
            this.btnExpandAll.TabIndex = 0;
            this.btnExpandAll.Text = "⊞ ขยายทั้งหมด";
            this.btnExpandAll.UseVisualStyleBackColor = false;
            this.btnExpandAll.Click += new System.EventHandler(this.BtnExpandAll_Click);
            //
            // btnCollapseAll
            //
            this.btnCollapseAll.BackColor = System.Drawing.Color.White;
            this.btnCollapseAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCollapseAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(190)))), ((int)(((byte)(215)))));
            this.btnCollapseAll.FlatAppearance.BorderSize = 1;
            this.btnCollapseAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCollapseAll.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnCollapseAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.btnCollapseAll.Location = new System.Drawing.Point(94, 3);
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Size = new System.Drawing.Size(80, 22);
            this.btnCollapseAll.TabIndex = 1;
            this.btnCollapseAll.Text = "⊟ ยุบทั้งหมด";
            this.btnCollapseAll.UseVisualStyleBackColor = false;
            this.btnCollapseAll.Click += new System.EventHandler(this.BtnCollapseAll_Click);
            //
            // lblOrgUnit
            //
            this.lblOrgUnit.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOrgUnit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblOrgUnit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblOrgUnit.Location = new System.Drawing.Point(12, 10);
            this.lblOrgUnit.Name = "lblOrgUnit";
            this.lblOrgUnit.Size = new System.Drawing.Size(475, 24);
            this.lblOrgUnit.TabIndex = 4;
            this.lblOrgUnit.Text = "🏢  หน่วยงาน";
            // 
            // pnlRight
            // 
            this.pnlRight.Controls.Add(this.pnlTree);
            this.pnlRight.Controls.Add(this.pnlOptions);
            this.pnlRight.Controls.Add(this.lblPathBreadcrumb);
            this.pnlRight.Controls.Add(this.lblPreview);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(0, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlRight.Size = new System.Drawing.Size(300, 410);
            this.pnlRight.TabIndex = 0;
            // 
            // pnlTree
            // 
            this.pnlTree.BackColor = System.Drawing.Color.White;
            this.pnlTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTree.Controls.Add(this.treePreview);
            this.pnlTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTree.Location = new System.Drawing.Point(12, 34);
            this.pnlTree.Name = "pnlTree";
            this.pnlTree.Size = new System.Drawing.Size(276, 256);
            this.pnlTree.TabIndex = 0;
            // 
            // treePreview
            // 
            this.treePreview.BackColor       = System.Drawing.Color.White;
            this.treePreview.BorderStyle     = System.Windows.Forms.BorderStyle.None;
            this.treePreview.Dock            = System.Windows.Forms.DockStyle.Fill;
            this.treePreview.Font            = new System.Drawing.Font("Segoe UI", 9.5F);
            this.treePreview.Location        = new System.Drawing.Point(0, 0);
            this.treePreview.Name            = "treePreview";
            this.treePreview.Scrollable      = true;
            this.treePreview.ShowNodeToolTips = true;
            this.treePreview.Size            = new System.Drawing.Size(274, 254);
            this.treePreview.TabIndex        = 0;
            // 
            // pnlOptions
            // 
            this.pnlOptions.BackColor = System.Drawing.Color.Transparent;
            this.pnlOptions.Controls.Add(this.cboConfidentiality);
            this.pnlOptions.Controls.Add(this.lblConf);
            this.pnlOptions.Controls.Add(this.sep2);
            this.pnlOptions.Controls.Add(this.txtRootName);
            this.pnlOptions.Controls.Add(this.lblRootName);
            this.pnlOptions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlOptions.Location = new System.Drawing.Point(12, 290);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(276, 110);
            this.pnlOptions.TabIndex = 1;
            // 
            // cboConfidentiality
            // 
            this.cboConfidentiality.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboConfidentiality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboConfidentiality.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboConfidentiality.Items.AddRange(new object[] {
            "🔴  1HC — Highly Confidential",
            "🟠  2SC — Secret",
            "🔵  3NC — Confidential",
            "🟢  4PC — Public / Common"});
            this.cboConfidentiality.Location = new System.Drawing.Point(0, 80);
            this.cboConfidentiality.Name = "cboConfidentiality";
            this.cboConfidentiality.Size = new System.Drawing.Size(276, 25);
            this.cboConfidentiality.TabIndex = 3;
            // 
            // lblConf
            // 
            this.lblConf.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblConf.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblConf.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblConf.Location = new System.Drawing.Point(0, 56);
            this.lblConf.Name = "lblConf";
            this.lblConf.Size = new System.Drawing.Size(276, 24);
            this.lblConf.TabIndex = 4;
            this.lblConf.Text = "🔐  ระดับ";
            // 
            // sep2
            // 
            this.sep2.BackColor = System.Drawing.Color.Transparent;
            this.sep2.Dock = System.Windows.Forms.DockStyle.Top;
            this.sep2.Location = new System.Drawing.Point(0, 48);
            this.sep2.Name = "sep2";
            this.sep2.Size = new System.Drawing.Size(276, 8);
            this.sep2.TabIndex = 5;
            // 
            // txtRootName
            // 
            this.txtRootName.BackColor = System.Drawing.Color.White;
            this.txtRootName.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtRootName.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtRootName.Location = new System.Drawing.Point(0, 24);
            this.txtRootName.Name = "txtRootName";
            this.txtRootName.Size = new System.Drawing.Size(276, 24);
            this.txtRootName.TabIndex = 2;
            this.txtRootName.TextChanged += new System.EventHandler(this.TxtRootName_TextChanged);
            // 
            // lblRootName
            // 
            this.lblRootName.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRootName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRootName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblRootName.Location = new System.Drawing.Point(0, 0);
            this.lblRootName.Name = "lblRootName";
            this.lblRootName.Size = new System.Drawing.Size(276, 24);
            this.lblRootName.TabIndex = 6;
            this.lblRootName.Text = "📝  ชื่อ Root Folder";
            //
            // lblPathBreadcrumb
            //
            this.lblPathBreadcrumb.AutoEllipsis = true;
            this.lblPathBreadcrumb.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.lblPathBreadcrumb.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPathBreadcrumb.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPathBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(120)))));
            this.lblPathBreadcrumb.Name = "lblPathBreadcrumb";
            this.lblPathBreadcrumb.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lblPathBreadcrumb.Size = new System.Drawing.Size(276, 22);
            this.lblPathBreadcrumb.TabIndex = 3;
            this.lblPathBreadcrumb.Text = "";
            this.lblPathBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblPreview
            //
            this.lblPreview.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPreview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblPreview.Location = new System.Drawing.Point(12, 10);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(276, 24);
            this.lblPreview.TabIndex = 2;
            this.lblPreview.Text = "🌳  ตัวอย่างโครงสร้าง Folder";
            // 
            // ApplyTemplateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(903, 580);
            this.MinimumSize = new System.Drawing.Size(700, 520);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ApplyTemplateDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "สร้าง Folder จาก Template";
            this.Load += new System.EventHandler(this.ApplyTemplateDialog_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlStatus.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlOrgToolbar.ResumeLayout(false);
            this.pnlListBox.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.pnlTree.ResumeLayout(false);
            this.pnlOptions.ResumeLayout(false);
            this.pnlOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        // ── Designer fields ───────────────────────────────────────────────────
        private System.Windows.Forms.ToolTip        toolTipMain;
        private System.Windows.Forms.Panel          pnlOrgToolbar;
        private System.Windows.Forms.Button         btnExpandAll;
        private System.Windows.Forms.Button         btnCollapseAll;
        private System.Windows.Forms.Panel          pnlHeader;
        private System.Windows.Forms.Label          lblHeader;
        private System.Windows.Forms.Panel          pnlStatus;
        private System.Windows.Forms.Label          lblStatus;
        private System.Windows.Forms.Panel          pnlFooter;
        private System.Windows.Forms.Panel          pnlFooterLine;
        private System.Windows.Forms.Button         btnApply;
        private System.Windows.Forms.Button         btnCancel;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Panel          pnlLeft;
        private System.Windows.Forms.Label          lblOrgUnit;
        private System.Windows.Forms.TreeView       treeOrgUnit;
        private System.Windows.Forms.Panel          sepOrgTemplate;
        private System.Windows.Forms.Label          lblTemplate;
        private System.Windows.Forms.Panel          pnlListBox;
        private System.Windows.Forms.ListBox        lstTemplates;
        private System.Windows.Forms.Label          lblTemplateDesc;
        private System.Windows.Forms.Panel          pnlRight;
        private System.Windows.Forms.Label          lblPreview;
        private System.Windows.Forms.Label          lblPathBreadcrumb;
        private System.Windows.Forms.Panel          pnlTree;
        private System.Windows.Forms.TreeView       treePreview;
        private System.Windows.Forms.Panel          pnlOptions;
        private System.Windows.Forms.ComboBox       cboConfidentiality;
        private System.Windows.Forms.Label          lblConf;
        private System.Windows.Forms.Panel          sep2;
        private System.Windows.Forms.TextBox        txtRootName;
        private System.Windows.Forms.Label          lblRootName;
    }
}
