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
            this.btnDashboard = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlViewerHost = new System.Windows.Forms.Panel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.treeCategories = new System.Windows.Forms.TreeView();
            this.pnlLeftFooter = new System.Windows.Forms.Panel();
            this.btnNewCategory = new System.Windows.Forms.Button();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.lblCategoryHeader = new System.Windows.Forms.Label();
            this.panelMainRight = new System.Windows.Forms.Panel();
            this.tabArticles = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgvArticles = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.pnlArticleList = new System.Windows.Forms.Panel();
            this.flpArticles = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlFilterBar = new System.Windows.Forms.Panel();
            this.cboDepartment = new System.Windows.Forms.ComboBox();
            this.btnClearFilter = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboSearchField = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.pnlFilterRight = new System.Windows.Forms.Panel();
            this.lblPageSizeLabel = new System.Windows.Forms.Label();
            this.cboSortBy = new System.Windows.Forms.ComboBox();
            this.lblSortByLabel = new System.Windows.Forms.Label();
            this.txtToolbarSearch = new System.Windows.Forms.TextBox();
            this.lblToolbarSearchLabel = new System.Windows.Forms.Label();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.btnTestNotification = new System.Windows.Forms.Button();
            this.btnExportPdf = new System.Windows.Forms.Button();
            this.btnImportExcel = new System.Windows.Forms.Button();
            this.btnNewArticle = new System.Windows.Forms.Button();
            this.pnlRightTop = new System.Windows.Forms.Panel();
            this.lblArticleTitle = new System.Windows.Forms.Label();
            this.pnlPagination = new System.Windows.Forms.Panel();
            this.pnlPaginationButtons = new System.Windows.Forms.Panel();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.btnPage3 = new System.Windows.Forms.Button();
            this.btnPage2 = new System.Windows.Forms.Button();
            this.btnPage1 = new System.Windows.Forms.Button();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.cboPageSize = new System.Windows.Forms.ComboBox();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlLeftFooter.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.panelMainRight.SuspendLayout();
            this.tabArticles.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvArticles)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.pnlArticleList.SuspendLayout();
            this.pnlFilterBar.SuspendLayout();
            this.pnlFilterRight.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlRightTop.SuspendLayout();
            this.pnlPagination.SuspendLayout();
            this.pnlPaginationButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.btnDashboard);
            this.pnlHeader.Controls.Add(this.btnSettings);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1596, 32);
            this.pnlHeader.TabIndex = 1;
            // 
            // btnDashboard
            // 
            this.btnDashboard.BackColor = System.Drawing.Color.Transparent;
            this.btnDashboard.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDashboard.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnDashboard.FlatAppearance.BorderSize = 0;
            this.btnDashboard.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(74)))), ((int)(((byte)(111)))));
            this.btnDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashboard.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDashboard.ForeColor = System.Drawing.Color.White;
            this.btnDashboard.Location = new System.Drawing.Point(1290, 0);
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.Size = new System.Drawing.Size(169, 32);
            this.btnDashboard.TabIndex = 1;
            this.btnDashboard.Text = "📊 Dashboard";
            this.btnDashboard.UseVisualStyleBackColor = false;
            this.btnDashboard.Click += new System.EventHandler(this.BtnDashboard_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.Transparent;
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(74)))), ((int)(((byte)(111)))));
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.Location = new System.Drawing.Point(1459, 0);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(137, 32);
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
            this.lblTitle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(1596, 32);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "🏢 Core Management System — คลังความรู้ (KMS)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlViewerHost
            // 
            this.pnlViewerHost.Location = new System.Drawing.Point(12, 345);
            this.pnlViewerHost.Name = "pnlViewerHost";
            this.pnlViewerHost.Size = new System.Drawing.Size(309, 110);
            this.pnlViewerHost.TabIndex = 0;
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
            this.splitMain.Panel1.Controls.Add(this.pnlViewerHost);
            this.splitMain.Panel1MinSize = 160;
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.BackColor = System.Drawing.Color.White;
            this.splitMain.Panel2.Controls.Add(this.panelMainRight);
            this.splitMain.Panel2MinSize = 400;
            this.splitMain.Size = new System.Drawing.Size(1596, 955);
            this.splitMain.SplitterDistance = 303;
            this.splitMain.TabIndex = 0;
            // 
            // treeCategories
            // 
            this.treeCategories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.treeCategories.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeCategories.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeCategories.FullRowSelect = true;
            this.treeCategories.HotTracking = true;
            this.treeCategories.Indent = 16;
            this.treeCategories.Location = new System.Drawing.Point(0, 32);
            this.treeCategories.Name = "treeCategories";
            this.treeCategories.Size = new System.Drawing.Size(303, 873);
            this.treeCategories.TabIndex = 0;
            this.treeCategories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeCategories_AfterSelect);
            // 
            // pnlLeftFooter
            // 
            this.pnlLeftFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.pnlLeftFooter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLeftFooter.Controls.Add(this.btnNewCategory);
            this.pnlLeftFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLeftFooter.Location = new System.Drawing.Point(0, 905);
            this.pnlLeftFooter.Name = "pnlLeftFooter";
            this.pnlLeftFooter.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.pnlLeftFooter.Size = new System.Drawing.Size(303, 50);
            this.pnlLeftFooter.TabIndex = 1;
            // 
            // btnNewCategory
            // 
            this.btnNewCategory.BackColor = System.Drawing.Color.Transparent;
            this.btnNewCategory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNewCategory.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnNewCategory.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(240)))));
            this.btnNewCategory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewCategory.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNewCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnNewCategory.Location = new System.Drawing.Point(6, 5);
            this.btnNewCategory.Name = "btnNewCategory";
            this.btnNewCategory.Size = new System.Drawing.Size(289, 38);
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
            this.pnlLeftHeader.Size = new System.Drawing.Size(303, 32);
            this.pnlLeftHeader.TabIndex = 2;
            // 
            // lblCategoryHeader
            // 
            this.lblCategoryHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCategoryHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCategoryHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblCategoryHeader.Location = new System.Drawing.Point(0, 0);
            this.lblCategoryHeader.Name = "lblCategoryHeader";
            this.lblCategoryHeader.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblCategoryHeader.Size = new System.Drawing.Size(301, 30);
            this.lblCategoryHeader.TabIndex = 0;
            this.lblCategoryHeader.Text = "📚 ห้องความรู้";
            this.lblCategoryHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelMainRight
            // 
            this.panelMainRight.BackColor = System.Drawing.Color.White;
            this.panelMainRight.Controls.Add(this.tabArticles);
            this.panelMainRight.Controls.Add(this.pnlFilterBar);
            this.panelMainRight.Controls.Add(this.pnlToolbar);
            this.panelMainRight.Controls.Add(this.pnlRightTop);
            this.panelMainRight.Controls.Add(this.pnlPagination);
            this.panelMainRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMainRight.Location = new System.Drawing.Point(0, 0);
            this.panelMainRight.Name = "panelMainRight";
            this.panelMainRight.Size = new System.Drawing.Size(1289, 955);
            this.panelMainRight.TabIndex = 0;
            // 
            // tabArticles
            // 
            this.tabArticles.Controls.Add(this.tabPage1);
            this.tabArticles.Controls.Add(this.tabPage2);
            this.tabArticles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabArticles.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.tabArticles.Location = new System.Drawing.Point(0, 125);
            this.tabArticles.Name = "tabArticles";
            this.tabArticles.SelectedIndex = 0;
            this.tabArticles.Size = new System.Drawing.Size(1289, 780);
            this.tabArticles.TabIndex = 4;
            this.tabArticles.SelectedIndexChanged += new System.EventHandler(this.TabArticles_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.White;
            this.tabPage1.Controls.Add(this.dgvArticles);
            this.tabPage1.Location = new System.Drawing.Point(4, 37);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1281, 739);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "📋 ตารางบทความ";
            // 
            // dgvArticles
            // 
            this.dgvArticles.AllowUserToAddRows = false;
            this.dgvArticles.AllowUserToDeleteRows = false;
            this.dgvArticles.AllowUserToResizeRows = false;
            this.dgvArticles.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvArticles.BackgroundColor = System.Drawing.Color.White;
            this.dgvArticles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvArticles.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvArticles.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvArticles.ColumnHeadersHeight = 29;
            this.dgvArticles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvArticles.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(225)))), ((int)(((byte)(232)))));
            this.dgvArticles.Location = new System.Drawing.Point(3, 3);
            this.dgvArticles.MultiSelect = false;
            this.dgvArticles.Name = "dgvArticles";
            this.dgvArticles.RowHeadersVisible = false;
            this.dgvArticles.RowHeadersWidth = 51;
            this.dgvArticles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvArticles.Size = new System.Drawing.Size(1275, 733);
            this.dgvArticles.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.White;
            this.tabPage2.Controls.Add(this.pnlArticleList);
            this.tabPage2.Location = new System.Drawing.Point(4, 37);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1281, 739);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "🃏 การ์ดบทความ";
            // 
            // pnlArticleList
            // 
            this.pnlArticleList.AutoScroll = true;
            this.pnlArticleList.BackColor = System.Drawing.Color.White;
            this.pnlArticleList.Controls.Add(this.flpArticles);
            this.pnlArticleList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlArticleList.Location = new System.Drawing.Point(3, 3);
            this.pnlArticleList.Name = "pnlArticleList";
            this.pnlArticleList.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.pnlArticleList.Size = new System.Drawing.Size(1275, 733);
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
            this.flpArticles.Size = new System.Drawing.Size(1255, 717);
            this.flpArticles.TabIndex = 0;
            this.flpArticles.WrapContents = false;
            // 
            // pnlFilterBar
            // 
            this.pnlFilterBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.pnlFilterBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFilterBar.Controls.Add(this.cboDepartment);
            this.pnlFilterBar.Controls.Add(this.btnClearFilter);
            this.pnlFilterBar.Controls.Add(this.label1);
            this.pnlFilterBar.Controls.Add(this.cboSearchField);
            this.pnlFilterBar.Controls.Add(this.label2);
            this.pnlFilterBar.Controls.Add(this.cboStatus);
            this.pnlFilterBar.Controls.Add(this.label3);
            this.pnlFilterBar.Controls.Add(this.cboType);
            this.pnlFilterBar.Controls.Add(this.pnlFilterRight);
            this.pnlFilterBar.Controls.Add(this.txtToolbarSearch);
            this.pnlFilterBar.Controls.Add(this.lblToolbarSearchLabel);
            this.pnlFilterBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilterBar.Location = new System.Drawing.Point(0, 78);
            this.pnlFilterBar.Name = "pnlFilterBar";
            this.pnlFilterBar.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.pnlFilterBar.Size = new System.Drawing.Size(1289, 47);
            this.pnlFilterBar.TabIndex = 2;
            // 
            // cboDepartment
            // 
            this.cboDepartment.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboDepartment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDepartment.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.cboDepartment.FormattingEnabled = true;
            this.cboDepartment.Items.AddRange(new object[] {
            "ฝ่าย/แผนก ▼",
            "สินไหมทดแทน",
            "รับประกันภัย",
            "กฎหมาย",
            "IT"});
            this.cboDepartment.Location = new System.Drawing.Point(854, 5);
            this.cboDepartment.Name = "cboDepartment";
            this.cboDepartment.Size = new System.Drawing.Size(150, 27);
            this.cboDepartment.TabIndex = 2;
            this.cboDepartment.Visible = false;
            // 
            // btnClearFilter
            // 
            this.btnClearFilter.BackColor = System.Drawing.Color.White;
            this.btnClearFilter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearFilter.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnClearFilter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnClearFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFilter.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(35)))), ((int)(((byte)(50)))));
            this.btnClearFilter.Location = new System.Drawing.Point(780, 5);
            this.btnClearFilter.Margin = new System.Windows.Forms.Padding(0);
            this.btnClearFilter.Name = "btnClearFilter";
            this.btnClearFilter.Size = new System.Drawing.Size(74, 35);
            this.btnClearFilter.TabIndex = 1;
            this.btnClearFilter.Text = "ล้าง";
            this.btnClearFilter.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.label1.Location = new System.Drawing.Point(775, 5);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.label1.Size = new System.Drawing.Size(5, 19);
            this.label1.TabIndex = 6;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cboSearchField
            // 
            this.cboSearchField.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboSearchField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSearchField.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboSearchField.FormattingEnabled = true;
            this.cboSearchField.Location = new System.Drawing.Point(646, 5);
            this.cboSearchField.Name = "cboSearchField";
            this.cboSearchField.Size = new System.Drawing.Size(129, 36);
            this.cboSearchField.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.label2.Location = new System.Drawing.Point(641, 5);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.label2.Size = new System.Drawing.Size(5, 19);
            this.label2.TabIndex = 7;
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cboStatus
            // 
            this.cboStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboStatus.FormattingEnabled = true;
            this.cboStatus.Location = new System.Drawing.Point(521, 5);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(120, 36);
            this.cboStatus.TabIndex = 3;
            this.cboStatus.SelectedIndexChanged += new System.EventHandler(this.CboFilter_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.label3.Location = new System.Drawing.Point(516, 5);
            this.label3.Name = "label3";
            this.label3.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.label3.Size = new System.Drawing.Size(5, 19);
            this.label3.TabIndex = 8;
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cboType
            // 
            this.cboType.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboType.FormattingEnabled = true;
            this.cboType.ItemHeight = 28;
            this.cboType.Items.AddRange(new object[] {
            "ประเภท ▼",
            "SOP",
            "FAQ",
            "Wiki",
            "แนวทาง"});
            this.cboType.Location = new System.Drawing.Point(396, 5);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(120, 36);
            this.cboType.TabIndex = 4;
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.cboType_SelectedIndexChanged);
            // 
            // pnlFilterRight
            // 
            this.pnlFilterRight.Controls.Add(this.lblPageSizeLabel);
            this.pnlFilterRight.Controls.Add(this.cboSortBy);
            this.pnlFilterRight.Controls.Add(this.lblSortByLabel);
            this.pnlFilterRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlFilterRight.Location = new System.Drawing.Point(949, 5);
            this.pnlFilterRight.Name = "pnlFilterRight";
            this.pnlFilterRight.Size = new System.Drawing.Size(328, 35);
            this.pnlFilterRight.TabIndex = 0;
            this.pnlFilterRight.Visible = false;
            // 
            // lblPageSizeLabel
            // 
            this.lblPageSizeLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblPageSizeLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblPageSizeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblPageSizeLabel.Location = new System.Drawing.Point(268, 0);
            this.lblPageSizeLabel.Name = "lblPageSizeLabel";
            this.lblPageSizeLabel.Size = new System.Drawing.Size(60, 35);
            this.lblPageSizeLabel.TabIndex = 1;
            this.lblPageSizeLabel.Text = "แสดง:";
            this.lblPageSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboSortBy
            // 
            this.cboSortBy.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboSortBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSortBy.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.cboSortBy.FormattingEnabled = true;
            this.cboSortBy.Items.AddRange(new object[] {
            "วันที่ล่าสุด",
            "ชื่อบทความ",
            "ยอดอ่าน",
            "คะแนน"});
            this.cboSortBy.Location = new System.Drawing.Point(70, 0);
            this.cboSortBy.Name = "cboSortBy";
            this.cboSortBy.Size = new System.Drawing.Size(140, 25);
            this.cboSortBy.TabIndex = 2;
            // 
            // lblSortByLabel
            // 
            this.lblSortByLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSortByLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblSortByLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblSortByLabel.Location = new System.Drawing.Point(0, 0);
            this.lblSortByLabel.Name = "lblSortByLabel";
            this.lblSortByLabel.Size = new System.Drawing.Size(70, 35);
            this.lblSortByLabel.TabIndex = 3;
            this.lblSortByLabel.Text = "เรียงตาม:";
            this.lblSortByLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtToolbarSearch
            // 
            this.txtToolbarSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtToolbarSearch.Dock = System.Windows.Forms.DockStyle.Left;
            this.txtToolbarSearch.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtToolbarSearch.Location = new System.Drawing.Point(76, 5);
            this.txtToolbarSearch.Name = "txtToolbarSearch";
            this.txtToolbarSearch.Size = new System.Drawing.Size(320, 34);
            this.txtToolbarSearch.TabIndex = 0;
            this.txtToolbarSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearch_KeyDown);
            // 
            // lblToolbarSearchLabel
            // 
            this.lblToolbarSearchLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblToolbarSearchLabel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblToolbarSearchLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblToolbarSearchLabel.Location = new System.Drawing.Point(10, 5);
            this.lblToolbarSearchLabel.Name = "lblToolbarSearchLabel";
            this.lblToolbarSearchLabel.Size = new System.Drawing.Size(66, 35);
            this.lblToolbarSearchLabel.TabIndex = 1;
            this.lblToolbarSearchLabel.Text = "ค้นหา:";
            this.lblToolbarSearchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.pnlToolbar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlToolbar.Controls.Add(this.btnTestNotification);
            this.pnlToolbar.Controls.Add(this.btnExportPdf);
            this.pnlToolbar.Controls.Add(this.btnImportExcel);
            this.pnlToolbar.Controls.Add(this.btnNewArticle);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 32);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.pnlToolbar.Size = new System.Drawing.Size(1289, 46);
            this.pnlToolbar.TabIndex = 1;
            // 
            // btnTestNotification
            // 
            this.btnTestNotification.BackColor = System.Drawing.Color.White;
            this.btnTestNotification.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTestNotification.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnTestNotification.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnTestNotification.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(240)))));
            this.btnTestNotification.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestNotification.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestNotification.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(35)))), ((int)(((byte)(50)))));
            this.btnTestNotification.Location = new System.Drawing.Point(360, 4);
            this.btnTestNotification.Name = "btnTestNotification";
            this.btnTestNotification.Size = new System.Drawing.Size(140, 36);
            this.btnTestNotification.TabIndex = 4;
            this.btnTestNotification.Text = "🔔 Test Notification";
            this.btnTestNotification.UseVisualStyleBackColor = false;
            this.btnTestNotification.Click += new System.EventHandler(this.BtnTestNotification_Click);
            // 
            // btnExportPdf
            // 
            this.btnExportPdf.BackColor = System.Drawing.Color.White;
            this.btnExportPdf.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportPdf.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnExportPdf.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnExportPdf.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(240)))));
            this.btnExportPdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportPdf.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportPdf.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(35)))), ((int)(((byte)(50)))));
            this.btnExportPdf.Location = new System.Drawing.Point(250, 4);
            this.btnExportPdf.Name = "btnExportPdf";
            this.btnExportPdf.Size = new System.Drawing.Size(110, 36);
            this.btnExportPdf.TabIndex = 1;
            this.btnExportPdf.Text = "📊 Export PDF";
            this.btnExportPdf.UseVisualStyleBackColor = false;
            this.btnExportPdf.Click += new System.EventHandler(this.BtnReports_Click);
            // 
            // btnImportExcel
            // 
            this.btnImportExcel.BackColor = System.Drawing.Color.White;
            this.btnImportExcel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnImportExcel.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnImportExcel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnImportExcel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(240)))));
            this.btnImportExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportExcel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnImportExcel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(35)))), ((int)(((byte)(50)))));
            this.btnImportExcel.Location = new System.Drawing.Point(130, 4);
            this.btnImportExcel.Name = "btnImportExcel";
            this.btnImportExcel.Size = new System.Drawing.Size(120, 36);
            this.btnImportExcel.TabIndex = 2;
            this.btnImportExcel.Text = "📥 Import Excel";
            this.btnImportExcel.UseVisualStyleBackColor = false;
            this.btnImportExcel.Click += new System.EventHandler(this.BtnImportExcel_Click);
            // 
            // btnNewArticle
            // 
            this.btnNewArticle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnNewArticle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewArticle.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnNewArticle.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(95)))), ((int)(((byte)(154)))));
            this.btnNewArticle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(104)))), ((int)(((byte)(165)))));
            this.btnNewArticle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewArticle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNewArticle.ForeColor = System.Drawing.Color.White;
            this.btnNewArticle.Location = new System.Drawing.Point(10, 4);
            this.btnNewArticle.Name = "btnNewArticle";
            this.btnNewArticle.Size = new System.Drawing.Size(120, 36);
            this.btnNewArticle.TabIndex = 3;
            this.btnNewArticle.Text = "+ สร้างบทความ";
            this.btnNewArticle.UseVisualStyleBackColor = false;
            this.btnNewArticle.Click += new System.EventHandler(this.BtnNewArticle_Click);
            // 
            // pnlRightTop
            // 
            this.pnlRightTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.pnlRightTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRightTop.Controls.Add(this.lblArticleTitle);
            this.pnlRightTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRightTop.Location = new System.Drawing.Point(0, 0);
            this.pnlRightTop.Name = "pnlRightTop";
            this.pnlRightTop.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.pnlRightTop.Size = new System.Drawing.Size(1289, 32);
            this.pnlRightTop.TabIndex = 2;
            // 
            // lblArticleTitle
            // 
            this.lblArticleTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblArticleTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblArticleTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblArticleTitle.Location = new System.Drawing.Point(8, 0);
            this.lblArticleTitle.Name = "lblArticleTitle";
            this.lblArticleTitle.Size = new System.Drawing.Size(1279, 30);
            this.lblArticleTitle.TabIndex = 0;
            this.lblArticleTitle.Text = "  เลือกหมวดหมู่เพื่อดูบทความ";
            this.lblArticleTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlPagination
            // 
            this.pnlPagination.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.pnlPagination.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPagination.Controls.Add(this.pnlPaginationButtons);
            this.pnlPagination.Controls.Add(this.lblPageInfo);
            this.pnlPagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlPagination.Location = new System.Drawing.Point(0, 905);
            this.pnlPagination.Name = "pnlPagination";
            this.pnlPagination.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.pnlPagination.Size = new System.Drawing.Size(1289, 50);
            this.pnlPagination.TabIndex = 3;
            // 
            // pnlPaginationButtons
            // 
            this.pnlPaginationButtons.Controls.Add(this.btnNextPage);
            this.pnlPaginationButtons.Controls.Add(this.btnPage3);
            this.pnlPaginationButtons.Controls.Add(this.btnPage2);
            this.pnlPaginationButtons.Controls.Add(this.btnPage1);
            this.pnlPaginationButtons.Controls.Add(this.btnPrevPage);
            this.pnlPaginationButtons.Controls.Add(this.cboPageSize);
            this.pnlPaginationButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlPaginationButtons.Location = new System.Drawing.Point(692, 6);
            this.pnlPaginationButtons.Name = "pnlPaginationButtons";
            this.pnlPaginationButtons.Size = new System.Drawing.Size(585, 36);
            this.pnlPaginationButtons.TabIndex = 1;
            // 
            // btnNextPage
            // 
            this.btnNextPage.BackColor = System.Drawing.Color.White;
            this.btnNextPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNextPage.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnNextPage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnNextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextPage.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnNextPage.Location = new System.Drawing.Point(459, 0);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(124, 36);
            this.btnNextPage.TabIndex = 4;
            this.btnNextPage.Text = "ถัดไป »";
            this.btnNextPage.UseVisualStyleBackColor = false;
            // 
            // btnPage3
            // 
            this.btnPage3.BackColor = System.Drawing.Color.White;
            this.btnPage3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPage3.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPage3.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnPage3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPage3.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnPage3.Location = new System.Drawing.Point(398, 0);
            this.btnPage3.Name = "btnPage3";
            this.btnPage3.Size = new System.Drawing.Size(61, 36);
            this.btnPage3.TabIndex = 3;
            this.btnPage3.Text = "3";
            this.btnPage3.UseVisualStyleBackColor = false;
            // 
            // btnPage2
            // 
            this.btnPage2.BackColor = System.Drawing.Color.White;
            this.btnPage2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPage2.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPage2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnPage2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPage2.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnPage2.Location = new System.Drawing.Point(337, 0);
            this.btnPage2.Name = "btnPage2";
            this.btnPage2.Size = new System.Drawing.Size(61, 36);
            this.btnPage2.TabIndex = 2;
            this.btnPage2.Text = "2";
            this.btnPage2.UseVisualStyleBackColor = false;
            // 
            // btnPage1
            // 
            this.btnPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnPage1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPage1.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPage1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnPage1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPage1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnPage1.ForeColor = System.Drawing.Color.White;
            this.btnPage1.Location = new System.Drawing.Point(276, 0);
            this.btnPage1.Name = "btnPage1";
            this.btnPage1.Size = new System.Drawing.Size(61, 36);
            this.btnPage1.TabIndex = 1;
            this.btnPage1.Text = "1";
            this.btnPage1.UseVisualStyleBackColor = false;
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.BackColor = System.Drawing.Color.White;
            this.btnPrevPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrevPage.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPrevPage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnPrevPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevPage.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnPrevPage.Location = new System.Drawing.Point(150, 0);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(126, 36);
            this.btnPrevPage.TabIndex = 0;
            this.btnPrevPage.Text = "« ก่อนหน้า";
            this.btnPrevPage.UseVisualStyleBackColor = false;
            // 
            // cboPageSize
            // 
            this.cboPageSize.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPageSize.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.cboPageSize.FormattingEnabled = true;
            this.cboPageSize.Items.AddRange(new object[] {
            "20 รายการ",
            "50 รายการ",
            "100 รายการ"});
            this.cboPageSize.Location = new System.Drawing.Point(0, 0);
            this.cboPageSize.Name = "cboPageSize";
            this.cboPageSize.Size = new System.Drawing.Size(150, 36);
            this.cboPageSize.TabIndex = 0;
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblPageInfo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblPageInfo.Location = new System.Drawing.Point(10, 6);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(200, 36);
            this.lblPageInfo.TabIndex = 0;
            this.lblPageInfo.Text = "แสดง 1-20 จาก 36 รายการ";
            this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // KBMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1596, 987);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "KBMainForm";
            this.Text = "ห้องสมุดความรู้ (KMS) — MOD-006 | KBMain";
            this.Load += new System.EventHandler(this.KBMainForm_Load);
            this.Resize += new System.EventHandler(this.KBMainForm_Resize);
            this.pnlHeader.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlLeftFooter.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.panelMainRight.ResumeLayout(false);
            this.tabArticles.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvArticles)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.pnlArticleList.ResumeLayout(false);
            this.pnlFilterBar.ResumeLayout(false);
            this.pnlFilterBar.PerformLayout();
            this.pnlFilterRight.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlRightTop.ResumeLayout(false);
            this.pnlPagination.ResumeLayout(false);
            this.pnlPaginationButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel           pnlHeader;
        private System.Windows.Forms.Label           lblTitle;
        private System.Windows.Forms.Button          btnDashboard;
        private System.Windows.Forms.Button          btnSettings;
        private System.Windows.Forms.SplitContainer  splitMain;
        private System.Windows.Forms.Panel           pnlViewerHost;    // created in KBMainForm.cs constructor
        private System.Windows.Forms.Panel           pnlCategoryHost;  // created in KBMainForm.cs constructor
        private System.Windows.Forms.Panel           pnlEditorHost;    // created in KBMainForm.cs constructor
        private System.Windows.Forms.Panel           pnlImportHost;    // created in KBMainForm.cs constructor
        private System.Windows.Forms.Panel           panelMainRight;
        private System.Windows.Forms.Panel           pnlLeftHeader;
        private System.Windows.Forms.Label           lblCategoryHeader;
        private System.Windows.Forms.TreeView        treeCategories;
        private System.Windows.Forms.Panel           pnlLeftFooter;
        private System.Windows.Forms.Button          btnNewCategory;
        private System.Windows.Forms.Panel           pnlRightTop;
        private System.Windows.Forms.Label           lblArticleTitle;
        private System.Windows.Forms.Panel           pnlToolbar;
        private System.Windows.Forms.ComboBox        cboSearchField;
        private System.Windows.Forms.TextBox         txtToolbarSearch;
        private System.Windows.Forms.Label           lblToolbarSearchLabel;
        private System.Windows.Forms.Button          btnExportPdf;
        private System.Windows.Forms.Button          btnTestNotification;
        private System.Windows.Forms.Button          btnImportExcel;
        private System.Windows.Forms.Button          btnNewArticle;
        private System.Windows.Forms.Panel           pnlFilterBar;
        private System.Windows.Forms.Panel           pnlFilterRight;
        private System.Windows.Forms.ComboBox        cboPageSize;
        private System.Windows.Forms.Label           lblPageSizeLabel;
        private System.Windows.Forms.ComboBox        cboSortBy;
        private System.Windows.Forms.Label           lblSortByLabel;
        private System.Windows.Forms.Button          btnClearFilter;
        private System.Windows.Forms.ComboBox        cboDepartment;
        private System.Windows.Forms.ComboBox        cboStatus;
        private System.Windows.Forms.ComboBox        cboType;
        private System.Windows.Forms.TabControl      tabArticles;
        private System.Windows.Forms.TabPage         tabPage1;
        private System.Windows.Forms.DataGridView    dgvArticles;
        private System.Windows.Forms.TabPage         tabPage2;
        private System.Windows.Forms.Panel           pnlArticleList;
        private System.Windows.Forms.FlowLayoutPanel flpArticles;
        private System.Windows.Forms.Panel           pnlPagination;
        private System.Windows.Forms.Panel           pnlPaginationButtons;
        private System.Windows.Forms.Button          btnNextPage;
        private System.Windows.Forms.Button          btnPage3;
        private System.Windows.Forms.Button          btnPage2;
        private System.Windows.Forms.Button          btnPage1;
        private System.Windows.Forms.Button          btnPrevPage;
        private System.Windows.Forms.Label           lblPageInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}
