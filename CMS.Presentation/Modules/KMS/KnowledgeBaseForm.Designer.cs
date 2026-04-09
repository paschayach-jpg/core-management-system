namespace CMS.Presentation.Modules.KMS
{
    partial class KnowledgeBaseForm
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
            this.btnSettings = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.treeCategories = new System.Windows.Forms.TreeView();
            this.pnlLeftFooter = new System.Windows.Forms.Panel();
            this.btnNewCategory = new System.Windows.Forms.Button();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.lblCategoryHeader = new System.Windows.Forms.Label();
            this.panelMainRight = new System.Windows.Forms.Panel();
            this.pnlArticleList = new System.Windows.Forms.Panel();
            this.flpArticles = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.cboFilter = new System.Windows.Forms.ComboBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchIcon = new System.Windows.Forms.Label();
            this.pnlRightTop = new System.Windows.Forms.Panel();
            this.lblArticleTitle = new System.Windows.Forms.Label();
            this.pnlRightFooter = new System.Windows.Forms.Panel();
            this.lblArticleCount = new System.Windows.Forms.Label();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnImportExcel = new System.Windows.Forms.Button();
            this.btnNewArticle = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlLeftFooter.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.panelMainRight.SuspendLayout();
            this.pnlArticleList.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.pnlRightTop.SuspendLayout();
            this.pnlRightFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.pnlHeader.Controls.Add(this.btnSettings);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1100, 52);
            this.pnlHeader.TabIndex = 1;
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))));
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.Location = new System.Drawing.Point(990, 0);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(110, 52);
            this.btnSettings.TabIndex = 0;
            this.btnSettings.Text = "⚙️ ตั้งค่า";
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1100, 52);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "  📚 ห้องสมุดความรู้ (KMS)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(210)))), ((int)(((byte)(225)))));
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 52);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.splitMain.Panel1.Controls.Add(this.treeCategories);
            this.splitMain.Panel1.Controls.Add(this.pnlLeftFooter);
            this.splitMain.Panel1.Controls.Add(this.pnlLeftHeader);
            this.splitMain.Panel1MinSize = 160;
            // 
            // splitMain.Panel2
            //
            this.splitMain.Panel2.BackColor = System.Drawing.Color.White;
            this.splitMain.Panel2.Controls.Add(this.panelMainRight);
            this.splitMain.Panel2MinSize = 400;
            this.splitMain.Size = new System.Drawing.Size(1100, 628);
            this.splitMain.SplitterDistance = 260;
            this.splitMain.TabIndex = 0;
            //
            // panelMainRight
            //
            this.panelMainRight.BackColor = System.Drawing.Color.White;
            this.panelMainRight.Controls.Add(this.pnlArticleList);
            this.panelMainRight.Controls.Add(this.pnlSearch);
            this.panelMainRight.Controls.Add(this.pnlRightTop);
            this.panelMainRight.Controls.Add(this.pnlRightFooter);
            this.panelMainRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMainRight.Name = "panelMainRight";
            this.panelMainRight.TabIndex = 0;
            //
            // treeCategories
            //
            this.treeCategories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.treeCategories.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeCategories.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.treeCategories.FullRowSelect = true;
            this.treeCategories.HotTracking = true;
            this.treeCategories.Indent = 18;
            this.treeCategories.Location = new System.Drawing.Point(0, 38);
            this.treeCategories.Name = "treeCategories";
            this.treeCategories.Size = new System.Drawing.Size(696, 544);
            this.treeCategories.TabIndex = 0;
            this.treeCategories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeCategories_AfterSelect);
            // 
            // pnlLeftFooter
            // 
            this.pnlLeftFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(248)))));
            this.pnlLeftFooter.Controls.Add(this.btnNewCategory);
            this.pnlLeftFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLeftFooter.Location = new System.Drawing.Point(0, 582);
            this.pnlLeftFooter.Name = "pnlLeftFooter";
            this.pnlLeftFooter.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlLeftFooter.Size = new System.Drawing.Size(696, 46);
            this.pnlLeftFooter.TabIndex = 1;
            // 
            // btnNewCategory
            // 
            this.btnNewCategory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnNewCategory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNewCategory.FlatAppearance.BorderSize = 0;
            this.btnNewCategory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewCategory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnNewCategory.ForeColor = System.Drawing.Color.White;
            this.btnNewCategory.Location = new System.Drawing.Point(8, 6);
            this.btnNewCategory.Name = "btnNewCategory";
            this.btnNewCategory.Size = new System.Drawing.Size(680, 34);
            this.btnNewCategory.TabIndex = 0;
            this.btnNewCategory.Text = "+ หมวดหมู่ใหม่";
            this.btnNewCategory.UseVisualStyleBackColor = false;
            this.btnNewCategory.Click += new System.EventHandler(this.BtnNewCategory_Click);
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(248)))));
            this.pnlLeftHeader.Controls.Add(this.lblCategoryHeader);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(696, 38);
            this.pnlLeftHeader.TabIndex = 2;
            // 
            // lblCategoryHeader
            // 
            this.lblCategoryHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCategoryHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCategoryHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblCategoryHeader.Location = new System.Drawing.Point(0, 0);
            this.lblCategoryHeader.Name = "lblCategoryHeader";
            this.lblCategoryHeader.Size = new System.Drawing.Size(696, 38);
            this.lblCategoryHeader.TabIndex = 0;
            this.lblCategoryHeader.Text = "  หมวดหมู่";
            this.lblCategoryHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlArticleList
            // 
            this.pnlArticleList.AutoScroll = true;
            this.pnlArticleList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(253)))));
            this.pnlArticleList.Controls.Add(this.flpArticles);
            this.pnlArticleList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlArticleList.Location = new System.Drawing.Point(0, 80);
            this.pnlArticleList.Name = "pnlArticleList";
            this.pnlArticleList.Padding = new System.Windows.Forms.Padding(8);
            this.pnlArticleList.Size = new System.Drawing.Size(400, 500);
            this.pnlArticleList.TabIndex = 0;
            // 
            // flpArticles
            // 
            this.flpArticles.AutoScroll = true;
            this.flpArticles.BackColor = System.Drawing.Color.Transparent;
            this.flpArticles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpArticles.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpArticles.Location = new System.Drawing.Point(8, 8);
            this.flpArticles.Name = "flpArticles";
            this.flpArticles.Size = new System.Drawing.Size(384, 484);
            this.flpArticles.TabIndex = 0;
            this.flpArticles.WrapContents = false;
            // 
            // pnlSearch
            // 
            this.pnlSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(251)))), ((int)(((byte)(253)))));
            this.pnlSearch.Controls.Add(this.cboFilter);
            this.pnlSearch.Controls.Add(this.btnSearch);
            this.pnlSearch.Controls.Add(this.txtSearch);
            this.pnlSearch.Controls.Add(this.lblSearchIcon);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(0, 38);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlSearch.Size = new System.Drawing.Size(400, 42);
            this.pnlSearch.TabIndex = 1;
            // 
            // cboFilter
            // 
            this.cboFilter.Dock = System.Windows.Forms.DockStyle.Right;
            this.cboFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboFilter.Location = new System.Drawing.Point(182, 6);
            this.cboFilter.Name = "cboFilter";
            this.cboFilter.Size = new System.Drawing.Size(130, 28);
            this.cboFilter.TabIndex = 0;
            this.cboFilter.SelectedIndexChanged += new System.EventHandler(this.CboFilter_SelectedIndexChanged);
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(180)))));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(312, 6);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 30);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = "ค้นหา";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtSearch.Location = new System.Drawing.Point(36, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(356, 29);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearch_KeyDown);
            // 
            // lblSearchIcon
            // 
            this.lblSearchIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSearchIcon.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblSearchIcon.Location = new System.Drawing.Point(8, 6);
            this.lblSearchIcon.Name = "lblSearchIcon";
            this.lblSearchIcon.Size = new System.Drawing.Size(28, 30);
            this.lblSearchIcon.TabIndex = 3;
            this.lblSearchIcon.Text = "🔍";
            this.lblSearchIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlRightTop
            // 
            this.pnlRightTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(248)))));
            this.pnlRightTop.Controls.Add(this.lblArticleTitle);
            this.pnlRightTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRightTop.Location = new System.Drawing.Point(0, 0);
            this.pnlRightTop.Name = "pnlRightTop";
            this.pnlRightTop.Size = new System.Drawing.Size(400, 38);
            this.pnlRightTop.TabIndex = 2;
            // 
            // lblArticleTitle
            // 
            this.lblArticleTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblArticleTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblArticleTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblArticleTitle.Location = new System.Drawing.Point(0, 0);
            this.lblArticleTitle.Name = "lblArticleTitle";
            this.lblArticleTitle.Size = new System.Drawing.Size(400, 38);
            this.lblArticleTitle.TabIndex = 0;
            this.lblArticleTitle.Text = "  เลือกหมวดหมู่เพื่อดูบทความ";
            this.lblArticleTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlRightFooter
            // 
            this.pnlRightFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(248)))));
            this.pnlRightFooter.Controls.Add(this.lblArticleCount);
            this.pnlRightFooter.Controls.Add(this.btnReports);
            this.pnlRightFooter.Controls.Add(this.btnImportExcel);
            this.pnlRightFooter.Controls.Add(this.btnNewArticle);
            this.pnlRightFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlRightFooter.Location = new System.Drawing.Point(0, 580);
            this.pnlRightFooter.Name = "pnlRightFooter";
            this.pnlRightFooter.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.pnlRightFooter.Size = new System.Drawing.Size(400, 48);
            this.pnlRightFooter.TabIndex = 3;
            // 
            // lblArticleCount
            // 
            this.lblArticleCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblArticleCount.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblArticleCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(130)))));
            this.lblArticleCount.Location = new System.Drawing.Point(378, 7);
            this.lblArticleCount.Name = "lblArticleCount";
            this.lblArticleCount.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.lblArticleCount.Size = new System.Drawing.Size(14, 34);
            this.lblArticleCount.TabIndex = 0;
            this.lblArticleCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnReports
            // 
            this.btnReports.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(130)))));
            this.btnReports.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReports.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnReports.FlatAppearance.BorderSize = 0;
            this.btnReports.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReports.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnReports.ForeColor = System.Drawing.Color.White;
            this.btnReports.Location = new System.Drawing.Point(268, 7);
            this.btnReports.Name = "btnReports";
            this.btnReports.Size = new System.Drawing.Size(110, 34);
            this.btnReports.TabIndex = 1;
            this.btnReports.Text = "📊 รายงาน";
            this.btnReports.UseVisualStyleBackColor = false;
            this.btnReports.Click += new System.EventHandler(this.BtnReports_Click);
            // 
            // btnImportExcel
            // 
            this.btnImportExcel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(124)))), ((int)(((byte)(65)))));
            this.btnImportExcel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnImportExcel.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnImportExcel.FlatAppearance.BorderSize = 0;
            this.btnImportExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportExcel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnImportExcel.ForeColor = System.Drawing.Color.White;
            this.btnImportExcel.Location = new System.Drawing.Point(138, 7);
            this.btnImportExcel.Name = "btnImportExcel";
            this.btnImportExcel.Size = new System.Drawing.Size(130, 34);
            this.btnImportExcel.TabIndex = 2;
            this.btnImportExcel.Text = "📥 Import Excel";
            this.btnImportExcel.UseVisualStyleBackColor = false;
            this.btnImportExcel.Click += new System.EventHandler(this.BtnImportExcel_Click);
            // 
            // btnNewArticle
            // 
            this.btnNewArticle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnNewArticle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewArticle.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnNewArticle.FlatAppearance.BorderSize = 0;
            this.btnNewArticle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewArticle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnNewArticle.ForeColor = System.Drawing.Color.White;
            this.btnNewArticle.Location = new System.Drawing.Point(8, 7);
            this.btnNewArticle.Name = "btnNewArticle";
            this.btnNewArticle.Size = new System.Drawing.Size(130, 34);
            this.btnNewArticle.TabIndex = 3;
            this.btnNewArticle.Text = "+ บทความใหม่";
            this.btnNewArticle.UseVisualStyleBackColor = false;
            this.btnNewArticle.Click += new System.EventHandler(this.BtnNewArticle_Click);
            // 
            // KnowledgeBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1100, 680);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "KnowledgeBaseForm";
            this.Text = "ห้องสมุดความรู้ (KMS) — MOD-006";
            this.Load += new System.EventHandler(this.KnowledgeBaseForm_Load);
            this.Resize += new System.EventHandler(this.KnowledgeBaseForm_Resize);
            this.pnlHeader.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlLeftFooter.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.panelMainRight.ResumeLayout(false);
            this.pnlArticleList.ResumeLayout(false);
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.pnlRightTop.ResumeLayout(false);
            this.pnlRightFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel           pnlHeader;
        private System.Windows.Forms.Label           lblTitle;
        private System.Windows.Forms.Button          btnSettings;
        private System.Windows.Forms.SplitContainer  splitMain;
        private System.Windows.Forms.Panel           panelMainRight;
        private System.Windows.Forms.Panel           pnlLeftHeader;
        private System.Windows.Forms.Label           lblCategoryHeader;
        private System.Windows.Forms.TreeView        treeCategories;
        private System.Windows.Forms.Panel           pnlLeftFooter;
        private System.Windows.Forms.Button          btnNewCategory;
        private System.Windows.Forms.Panel           pnlRightTop;
        private System.Windows.Forms.Label           lblArticleTitle;
        private System.Windows.Forms.Panel           pnlSearch;
        private System.Windows.Forms.Label           lblSearchIcon;
        private System.Windows.Forms.TextBox         txtSearch;
        private System.Windows.Forms.Button          btnSearch;
        private System.Windows.Forms.ComboBox        cboFilter;
        private System.Windows.Forms.Panel           pnlArticleList;
        private System.Windows.Forms.FlowLayoutPanel flpArticles;
        private System.Windows.Forms.Panel           pnlRightFooter;
        private System.Windows.Forms.Button          btnNewArticle;
        private System.Windows.Forms.Button          btnImportExcel;
        private System.Windows.Forms.Button          btnReports;
        private System.Windows.Forms.Label           lblArticleCount;
    }
}
