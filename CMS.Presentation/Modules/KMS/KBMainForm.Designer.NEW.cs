namespace CMS.Presentation.Modules.KMS
{
    partial class KBMainForm
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
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.pnlToolbarRight = new System.Windows.Forms.Panel();
            this.txtToolbarSearch = new System.Windows.Forms.TextBox();
            this.lblToolbarSearchLabel = new System.Windows.Forms.Label();
            this.btnExportPdf = new System.Windows.Forms.Button();
            this.btnImportExcel = new System.Windows.Forms.Button();
            this.btnNewArticle = new System.Windows.Forms.Button();
            this.pnlFilterBar = new System.Windows.Forms.Panel();
            this.pnlFilterRight = new System.Windows.Forms.Panel();
            this.cboPageSize = new System.Windows.Forms.ComboBox();
            this.lblPageSizeLabel = new System.Windows.Forms.Label();
            this.cboSortBy = new System.Windows.Forms.ComboBox();
            this.lblSortByLabel = new System.Windows.Forms.Label();
            this.btnClearFilter = new System.Windows.Forms.Button();
            this.cboDepartment = new System.Windows.Forms.ComboBox();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.lblFilterLabel = new System.Windows.Forms.Label();
            this.pnlRightTop = new System.Windows.Forms.Panel();
            this.lblArticleTitle = new System.Windows.Forms.Label();
            this.pnlPagination = new System.Windows.Forms.Panel();
            this.pnlPaginationButtons = new System.Windows.Forms.Panel();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.btnPage3 = new System.Windows.Forms.Button();
            this.btnPage2 = new System.Windows.Forms.Button();
            this.btnPage1 = new System.Windows.Forms.Button();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlLeftFooter.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.panelMainRight.SuspendLayout();
            this.pnlArticleList.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlToolbarRight.SuspendLayout();
            this.pnlFilterBar.SuspendLayout();
            this.pnlFilterRight.SuspendLayout();
            this.pnlRightTop.SuspendLayout();
            this.pnlPagination.SuspendLayout();
            this.pnlPaginationButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.btnSettings);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1100, 32);
            this.pnlHeader.TabIndex = 1;
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.Transparent;
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(74)))), ((int)(((byte)(111)))));
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.Location = new System.Drawing.Point(1010, 0);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(90, 32);
            this.btnSettings.TabIndex = 0;
            this.btnSettings.Text = "⚙️ ตั้งค่า";
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(1100, 32);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "🏢 Core Management System — คลังความรู้ (KMS)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 32);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
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
            this.splitMain.Size = new System.Drawing.Size(1100, 648);
            this.splitMain.SplitterDistance = 210;
            this.splitMain.TabIndex = 0;
            // 
            // treeCategories
            // 
            this.treeCategories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.treeCategories.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeCategories.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.treeCategories.FullRowSelect = true;
            this.treeCategories.HotTracking = true;
            this.treeCategories.Indent = 16;
            this.treeCategories.Location = new System.Drawing.Point(0, 32);
            this.treeCategories.Name = "treeCategories";
            this.treeCategories.Size = new System.Drawing.Size(210, 576);
            this.treeCategories.TabIndex = 0;
            this.treeCategories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeCategories_AfterSelect);
            // 
            // pnlLeftFooter
            // 
            this.pnlLeftFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.pnlLeftFooter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLeftFooter.Controls.Add(this.btnNewCategory);
            this.pnlLeftFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLeftFooter.Location = new System.Drawing.Point(0, 608);
            this.pnlLeftFooter.Name = "pnlLeftFooter";
            this.pnlLeftFooter.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.pnlLeftFooter.Size = new System.Drawing.Size(210, 40);
            this.pnlLeftFooter.TabIndex = 1;
            // 
            // btnNewCategory
            // 
            this.btnNewCategory.BackColor = System.Drawing.Color.Transparent;
            this.btnNewCategory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNewCategory.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnNewCategory.FlatAppearance.BorderSize = 1;
            this.btnNewCategory.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(240)))));
            this.btnNewCategory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewCategory.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnNewCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnNewCategory.Location = new System.Drawing.Point(6, 5);
            this.btnNewCategory.Name = "btnNewCategory";
            this.btnNewCategory.Size = new System.Drawing.Size(196, 28);
            this.btnNewCategory.TabIndex = 0;
            this.btnNewCategory.Text = "⚙️ จัดการหมวดหมู่";
            this.btnNewCategory.UseVisualStyleBackColor = false;
            this.btnNewCategory.Click += new System.EventHandler(this.BtnNewCategory_Click);
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.pnlLeftHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLeftHeader.Controls.Add(this.lblCategoryHeader);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(210, 32);
            this.pnlLeftHeader.TabIndex = 2;
            // 
            // lblCategoryHeader
            // 
            this.lblCategoryHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCategoryHeader.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCategoryHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblCategoryHeader.Location = new System.Drawing.Point(0, 0);
            this.lblCategoryHeader.Name = "lblCategoryHeader";
            this.lblCategoryHeader.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblCategoryHeader.Size = new System.Drawing.Size(208, 30);
            this.lblCategoryHeader.TabIndex = 0;
            this.lblCategoryHeader.Text = "📚 ห้องความรู้";
            this.lblCategoryHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelMainRight
            // 
            this.panelMainRight.BackColor = System.Drawing.Color.White;
            this.panelMainRight.Controls.Add(this.pnlArticleList);
            this.panelMainRight.Controls.Add(this.pnlFilterBar);
            this.panelMainRight.Controls.Add(this.pnlToolbar);
            this.panelMainRight.Controls.Add(this.pnlRightTop);
            this.panelMainRight.Controls.Add(this.pnlPagination);
            this.panelMainRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMainRight.Location = new System.Drawing.Point(0, 0);
            this.panelMainRight.Name = "panelMainRight";
            this.panelMainRight.Size = new System.Drawing.Size(886, 648);
            this.panelMainRight.TabIndex = 0;
            // 
            // pnlArticleList
            // 
            this.pnlArticleList.AutoScroll = true;
            this.pnlArticleList.BackColor = System.Drawing.Color.White;
            this.pnlArticleList.Controls.Add(this.flpArticles);
            this.pnlArticleList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlArticleList.Location = new System.Drawing.Point(0, 104);
            this.pnlArticleList.Name = "pnlArticleList";
            this.pnlArticleList.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.pnlArticleList.Size = new System.Drawing.Size(886, 504);
            this.pnlArticleList.TabIndex = 0;
            // 
            // flpArticles
            // 
            this.flpArticles.AutoScroll = true;
            this.flpArticles.BackColor = System.Drawing.Color.Transparent;
            this.flpArticles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpArticles.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpArticles.Location = new System.Drawing.Point(10, 8);
            this.flpArticles.Name = "flpArticles";
            this.flpArticles.Size = new System.Drawing.Size(866, 488);
            this.flpArticles.TabIndex = 0;
            this.flpArticles.WrapContents = false;
