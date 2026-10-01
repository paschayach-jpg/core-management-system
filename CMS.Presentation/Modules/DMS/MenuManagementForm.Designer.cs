namespace CMS.Presentation
{
    partial class MenuManagementForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Top Header ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.Label lblHeaderSubtitle;

        // ── Layout containers ────────────────────────────────────────────────
        private System.Windows.Forms.SplitContainer splitMain;

        // ── Left panel (Tree) ────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTreeHeader;
        private System.Windows.Forms.Label lblMenuTree;
        private System.Windows.Forms.Panel pnlTreeSearch;
        private System.Windows.Forms.TextBox txtSearchTree;
        private System.Windows.Forms.Panel pnlTreeToolbar;
        private System.Windows.Forms.Button btnExpandAll;
        private System.Windows.Forms.Button btnCollapseAll;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnRefreshTree;
        private System.Windows.Forms.TreeView treeMenu;
        private System.Windows.Forms.ImageList imgListTree;

        // ── Right panel (Card & Details) ─────────────────────────────────────
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Panel pnlCardHeader;
        private System.Windows.Forms.Label lblCardTitle;
        private System.Windows.Forms.Panel pnlFormContent;

        // Form Fields
        private System.Windows.Forms.Label lblIDLabel;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.Label lblNameLabel;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblKeyLabel;
        private System.Windows.Forms.TextBox txtKey;
        private System.Windows.Forms.Label lblFormClassLabel;
        private System.Windows.Forms.ComboBox cmbActionKey;
        private System.Windows.Forms.Label lblLevelLabel;
        private System.Windows.Forms.ComboBox cmbLevel;
        private System.Windows.Forms.Label lblParentLabel;
        private System.Windows.Forms.TextBox txtParentDisplay;
        private System.Windows.Forms.Button btnPickParent;
        private System.Windows.Forms.Button btnClearParent;
        private System.Windows.Forms.Label lblOrderLabel;
        private System.Windows.Forms.NumericUpDown numOrder;
        private System.Windows.Forms.Label lblRoleLabel;
        private System.Windows.Forms.ComboBox cmbRole;
        private System.Windows.Forms.Label lblStatusLabel;
        private System.Windows.Forms.CheckBox chkActive;
        private System.Windows.Forms.Label lblDescLabel;
        private System.Windows.Forms.TextBox txtDescription;

        // Hidden backward compatibility control
        private System.Windows.Forms.TextBox txtFormClass;
        private System.Windows.Forms.ComboBox cmbParent;

        // Action Buttons Bar
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDelete;

        // Status / Statistics Footer
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Label lblStats;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeaderSubtitle = new System.Windows.Forms.Label();
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.treeMenu = new System.Windows.Forms.TreeView();
            this.imgListTree = new System.Windows.Forms.ImageList(this.components);
            this.pnlTreeToolbar = new System.Windows.Forms.Panel();
            this.btnRefreshTree = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnCollapseAll = new System.Windows.Forms.Button();
            this.btnExpandAll = new System.Windows.Forms.Button();
            this.pnlTreeSearch = new System.Windows.Forms.Panel();
            this.txtSearchTree = new System.Windows.Forms.TextBox();
            this.pnlTreeHeader = new System.Windows.Forms.Panel();
            this.lblMenuTree = new System.Windows.Forms.Label();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.pnlFormContent = new System.Windows.Forms.Panel();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescLabel = new System.Windows.Forms.Label();
            this.chkActive = new System.Windows.Forms.CheckBox();
            this.lblStatusLabel = new System.Windows.Forms.Label();
            this.cmbRole = new System.Windows.Forms.ComboBox();
            this.lblRoleLabel = new System.Windows.Forms.Label();
            this.numOrder = new System.Windows.Forms.NumericUpDown();
            this.lblOrderLabel = new System.Windows.Forms.Label();
            this.btnClearParent = new System.Windows.Forms.Button();
            this.btnPickParent = new System.Windows.Forms.Button();
            this.txtParentDisplay = new System.Windows.Forms.TextBox();
            this.lblParentLabel = new System.Windows.Forms.Label();
            this.cmbLevel = new System.Windows.Forms.ComboBox();
            this.lblLevelLabel = new System.Windows.Forms.Label();
            this.cmbActionKey = new System.Windows.Forms.ComboBox();
            this.lblFormClassLabel = new System.Windows.Forms.Label();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.lblKeyLabel = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblNameLabel = new System.Windows.Forms.Label();
            this.lblID = new System.Windows.Forms.Label();
            this.lblIDLabel = new System.Windows.Forms.Label();
            this.txtFormClass = new System.Windows.Forms.TextBox();
            this.cmbParent = new System.Windows.Forms.ComboBox();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.pnlCardHeader = new System.Windows.Forms.Panel();
            this.lblCardTitle = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.lblStats = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlTreeToolbar.SuspendLayout();
            this.pnlTreeSearch.SuspendLayout();
            this.pnlTreeHeader.SuspendLayout();
            this.pnlCard.SuspendLayout();
            this.pnlFormContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOrder)).BeginInit();
            this.pnlActions.SuspendLayout();
            this.pnlCardHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.pnlHeader.Controls.Add(this.lblHeaderSubtitle);
            this.pnlHeader.Controls.Add(this.lblHeaderTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(18, 10, 18, 10);
            this.pnlHeader.Size = new System.Drawing.Size(984, 62);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeaderSubtitle
            // 
            this.lblHeaderSubtitle.AutoSize = true;
            this.lblHeaderSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblHeaderSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(220)))), ((int)(((byte)(252)))));
            this.lblHeaderSubtitle.Location = new System.Drawing.Point(18, 35);
            this.lblHeaderSubtitle.Name = "lblHeaderSubtitle";
            this.lblHeaderSubtitle.Size = new System.Drawing.Size(434, 15);
            this.lblHeaderSubtitle.TabIndex = 1;
            this.lblHeaderSubtitle.Text = "จัดการโครงสร้างเมนู, ลำดับการแสดงผล, สิทธิ์การเข้าถึง (Role), และ Action Key";
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.AutoSize = true;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblHeaderTitle.ForeColor = System.Drawing.Color.White;
            this.lblHeaderTitle.Location = new System.Drawing.Point(17, 10);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(325, 23);
            this.lblHeaderTitle.TabIndex = 0;
            this.lblHeaderTitle.Text = "🛠️ จัดการโครงสร้างเมนูระบบ (Menu Management)";
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(228)))), ((int)(((byte)(238)))));
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 62);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.BackColor = System.Drawing.Color.White;
            this.splitMain.Panel1.Controls.Add(this.treeMenu);
            this.splitMain.Panel1.Controls.Add(this.pnlTreeToolbar);
            this.splitMain.Panel1.Controls.Add(this.pnlTreeSearch);
            this.splitMain.Panel1.Controls.Add(this.pnlTreeHeader);
            this.splitMain.Panel1.Padding = new System.Windows.Forms.Padding(10);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(251)))));
            this.splitMain.Panel2.Controls.Add(this.pnlCard);
            this.splitMain.Panel2.Padding = new System.Windows.Forms.Padding(10);
            this.splitMain.Size = new System.Drawing.Size(984, 568);
            this.splitMain.SplitterDistance = 380;
            this.splitMain.SplitterWidth = 5;
            this.splitMain.TabIndex = 1;
            // 
            // treeMenu
            // 
            this.treeMenu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeMenu.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.treeMenu.FullRowSelect = true;
            this.treeMenu.HideSelection = false;
            this.treeMenu.ImageList = this.imgListTree;
            this.treeMenu.ItemHeight = 24;
            this.treeMenu.Location = new System.Drawing.Point(10, 114);
            this.treeMenu.Name = "treeMenu";
            this.treeMenu.ShowLines = true;
            this.treeMenu.ShowPlusMinus = true;
            this.treeMenu.ShowRootLines = true;
            this.treeMenu.Size = new System.Drawing.Size(360, 444);
            this.treeMenu.TabIndex = 3;
            this.treeMenu.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeMenu_AfterSelect);
            // 
            // imgListTree
            // 
            this.imgListTree.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imgListTree.ImageSize = new System.Drawing.Size(16, 16);
            this.imgListTree.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // pnlTreeToolbar
            // 
            this.pnlTreeToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.pnlTreeToolbar.Controls.Add(this.btnRefreshTree);
            this.pnlTreeToolbar.Controls.Add(this.btnMoveDown);
            this.pnlTreeToolbar.Controls.Add(this.btnMoveUp);
            this.pnlTreeToolbar.Controls.Add(this.btnCollapseAll);
            this.pnlTreeToolbar.Controls.Add(this.btnExpandAll);
            this.pnlTreeToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTreeToolbar.Location = new System.Drawing.Point(10, 78);
            this.pnlTreeToolbar.Name = "pnlTreeToolbar";
            this.pnlTreeToolbar.Padding = new System.Windows.Forms.Padding(4);
            this.pnlTreeToolbar.Size = new System.Drawing.Size(360, 36);
            this.pnlTreeToolbar.TabIndex = 2;
            // 
            // btnRefreshTree
            // 
            this.btnRefreshTree.BackColor = System.Drawing.Color.White;
            this.btnRefreshTree.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefreshTree.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(205)))), ((int)(((byte)(226)))));
            this.btnRefreshTree.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefreshTree.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnRefreshTree.Location = new System.Drawing.Point(260, 4);
            this.btnRefreshTree.Name = "btnRefreshTree";
            this.btnRefreshTree.Size = new System.Drawing.Size(42, 28);
            this.btnRefreshTree.TabIndex = 4;
            this.btnRefreshTree.Text = "🔄";
            this.btnRefreshTree.UseVisualStyleBackColor = false;
            this.btnRefreshTree.Click += new System.EventHandler(this.BtnRefreshTree_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.BackColor = System.Drawing.Color.White;
            this.btnMoveDown.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMoveDown.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(205)))), ((int)(((byte)(226)))));
            this.btnMoveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMoveDown.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnMoveDown.Location = new System.Drawing.Point(214, 4);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(42, 28);
            this.btnMoveDown.TabIndex = 3;
            this.btnMoveDown.Text = "🔽";
            this.btnMoveDown.UseVisualStyleBackColor = false;
            this.btnMoveDown.Click += new System.EventHandler(this.BtnMoveDown_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.BackColor = System.Drawing.Color.White;
            this.btnMoveUp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMoveUp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(205)))), ((int)(((byte)(226)))));
            this.btnMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMoveUp.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnMoveUp.Location = new System.Drawing.Point(168, 4);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(42, 28);
            this.btnMoveUp.TabIndex = 2;
            this.btnMoveUp.Text = "🔼";
            this.btnMoveUp.UseVisualStyleBackColor = false;
            this.btnMoveUp.Click += new System.EventHandler(this.BtnMoveUp_Click);
            // 
            // btnCollapseAll
            // 
            this.btnCollapseAll.BackColor = System.Drawing.Color.White;
            this.btnCollapseAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCollapseAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(205)))), ((int)(((byte)(226)))));
            this.btnCollapseAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCollapseAll.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnCollapseAll.Location = new System.Drawing.Point(86, 4);
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Size = new System.Drawing.Size(78, 28);
            this.btnCollapseAll.TabIndex = 1;
            this.btnCollapseAll.Text = "⊟ ย่อ";
            this.btnCollapseAll.UseVisualStyleBackColor = false;
            this.btnCollapseAll.Click += new System.EventHandler(this.BtnCollapseAll_Click);
            // 
            // btnExpandAll
            // 
            this.btnExpandAll.BackColor = System.Drawing.Color.White;
            this.btnExpandAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExpandAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(205)))), ((int)(((byte)(226)))));
            this.btnExpandAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpandAll.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnExpandAll.Location = new System.Drawing.Point(4, 4);
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(78, 28);
            this.btnExpandAll.TabIndex = 0;
            this.btnExpandAll.Text = "⊞ ขยาย";
            this.btnExpandAll.UseVisualStyleBackColor = false;
            this.btnExpandAll.Click += new System.EventHandler(this.BtnExpandAll_Click);
            // 
            // pnlTreeSearch
            // 
            this.pnlTreeSearch.BackColor = System.Drawing.Color.White;
            this.pnlTreeSearch.Controls.Add(this.txtSearchTree);
            this.pnlTreeSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTreeSearch.Location = new System.Drawing.Point(10, 44);
            this.pnlTreeSearch.Name = "pnlTreeSearch";
            this.pnlTreeSearch.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.pnlTreeSearch.Size = new System.Drawing.Size(360, 34);
            this.pnlTreeSearch.TabIndex = 1;
            // 
            // txtSearchTree
            // 
            this.txtSearchTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearchTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearchTree.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtSearchTree.ForeColor = System.Drawing.Color.Gray;
            this.txtSearchTree.Location = new System.Drawing.Point(0, 4);
            this.txtSearchTree.Name = "txtSearchTree";
            this.txtSearchTree.Size = new System.Drawing.Size(360, 24);
            this.txtSearchTree.TabIndex = 0;
            this.txtSearchTree.Text = "🔍 ค้นหาเมนู...";
            this.txtSearchTree.TextChanged += new System.EventHandler(this.TxtSearchTree_TextChanged);
            this.txtSearchTree.Enter += new System.EventHandler(this.TxtSearchTree_Enter);
            this.txtSearchTree.Leave += new System.EventHandler(this.TxtSearchTree_Leave);
            // 
            // pnlTreeHeader
            // 
            this.pnlTreeHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(241)))), ((int)(((byte)(250)))));
            this.pnlTreeHeader.Controls.Add(this.lblMenuTree);
            this.pnlTreeHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTreeHeader.Location = new System.Drawing.Point(10, 10);
            this.pnlTreeHeader.Name = "pnlTreeHeader";
            this.pnlTreeHeader.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlTreeHeader.Size = new System.Drawing.Size(360, 34);
            this.pnlTreeHeader.TabIndex = 0;
            // 
            // lblMenuTree
            // 
            this.lblMenuTree.AutoSize = true;
            this.lblMenuTree.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblMenuTree.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblMenuTree.Location = new System.Drawing.Point(6, 7);
            this.lblMenuTree.Name = "lblMenuTree";
            this.lblMenuTree.Size = new System.Drawing.Size(225, 19);
            this.lblMenuTree.TabIndex = 0;
            this.lblMenuTree.Text = "📂 โครงสร้างลำดับชั้นเมนู (Hierarchy)";
            // 
            // pnlCard
            // 
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCard.Controls.Add(this.pnlFormContent);
            this.pnlCard.Controls.Add(this.pnlActions);
            this.pnlCard.Controls.Add(this.pnlCardHeader);
            this.pnlCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCard.Location = new System.Drawing.Point(10, 10);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(579, 548);
            this.pnlCard.TabIndex = 0;
            // 
            // pnlFormContent
            // 
            this.pnlFormContent.AutoScroll = true;
            this.pnlFormContent.Controls.Add(this.txtDescription);
            this.pnlFormContent.Controls.Add(this.lblDescLabel);
            this.pnlFormContent.Controls.Add(this.chkActive);
            this.pnlFormContent.Controls.Add(this.lblStatusLabel);
            this.pnlFormContent.Controls.Add(this.cmbRole);
            this.pnlFormContent.Controls.Add(this.lblRoleLabel);
            this.pnlFormContent.Controls.Add(this.numOrder);
            this.pnlFormContent.Controls.Add(this.lblOrderLabel);
            this.pnlFormContent.Controls.Add(this.btnClearParent);
            this.pnlFormContent.Controls.Add(this.btnPickParent);
            this.pnlFormContent.Controls.Add(this.txtParentDisplay);
            this.pnlFormContent.Controls.Add(this.lblParentLabel);
            this.pnlFormContent.Controls.Add(this.cmbLevel);
            this.pnlFormContent.Controls.Add(this.lblLevelLabel);
            this.pnlFormContent.Controls.Add(this.cmbActionKey);
            this.pnlFormContent.Controls.Add(this.lblFormClassLabel);
            this.pnlFormContent.Controls.Add(this.txtKey);
            this.pnlFormContent.Controls.Add(this.lblKeyLabel);
            this.pnlFormContent.Controls.Add(this.txtName);
            this.pnlFormContent.Controls.Add(this.lblNameLabel);
            this.pnlFormContent.Controls.Add(this.lblID);
            this.pnlFormContent.Controls.Add(this.lblIDLabel);
            this.pnlFormContent.Controls.Add(this.txtFormClass);
            this.pnlFormContent.Controls.Add(this.cmbParent);
            this.pnlFormContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFormContent.Location = new System.Drawing.Point(0, 42);
            this.pnlFormContent.Name = "pnlFormContent";
            this.pnlFormContent.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlFormContent.Size = new System.Drawing.Size(577, 444);
            this.pnlFormContent.TabIndex = 1;
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtDescription.Location = new System.Drawing.Point(145, 305);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(412, 55);
            this.txtDescription.TabIndex = 18;
            // 
            // lblDescLabel
            // 
            this.lblDescLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDescLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblDescLabel.Location = new System.Drawing.Point(15, 305);
            this.lblDescLabel.Name = "lblDescLabel";
            this.lblDescLabel.Size = new System.Drawing.Size(125, 23);
            this.lblDescLabel.TabIndex = 17;
            this.lblDescLabel.Text = "คำอธิบาย / Tooltip:";
            // 
            // chkActive
            // 
            this.chkActive.AutoSize = true;
            this.chkActive.Checked = true;
            this.chkActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkActive.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.chkActive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(128)))), ((int)(((byte)(60)))));
            this.chkActive.Location = new System.Drawing.Point(145, 273);
            this.chkActive.Name = "chkActive";
            this.chkActive.Size = new System.Drawing.Size(193, 21);
            this.chkActive.TabIndex = 16;
            this.chkActive.Text = "🟢 เปิดใช้งานเมนูนี้ (Active)";
            this.chkActive.UseVisualStyleBackColor = true;
            // 
            // lblStatusLabel
            // 
            this.lblStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblStatusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblStatusLabel.Location = new System.Drawing.Point(15, 272);
            this.lblStatusLabel.Name = "lblStatusLabel";
            this.lblStatusLabel.Size = new System.Drawing.Size(125, 23);
            this.lblStatusLabel.TabIndex = 15;
            this.lblStatusLabel.Text = "สถานะการใช้งาน:";
            // 
            // cmbRole
            // 
            this.cmbRole.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbRole.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbRole.FormattingEnabled = true;
            this.cmbRole.Items.AddRange(new object[] {
            "(ทุกคน — All Users)",
            "👤 พนักงาน (User — Level 1)",
            "👔 หัวหน้างาน (Manager — Level 2)",
            "✅ ผู้อนุมัติ (Approver — Level 3)",
            "👑 ผู้ดูแลระบบ (Admin — Level 4)"});
            this.cmbRole.Location = new System.Drawing.Point(145, 238);
            this.cmbRole.Name = "cmbRole";
            this.cmbRole.Size = new System.Drawing.Size(412, 25);
            this.cmbRole.TabIndex = 14;
            // 
            // lblRoleLabel
            // 
            this.lblRoleLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblRoleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblRoleLabel.Location = new System.Drawing.Point(15, 240);
            this.lblRoleLabel.Name = "lblRoleLabel";
            this.lblRoleLabel.Size = new System.Drawing.Size(125, 23);
            this.lblRoleLabel.TabIndex = 13;
            this.lblRoleLabel.Text = "สิทธิ์ที่ต้องการ (Role):";
            // 
            // numOrder
            // 
            this.numOrder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numOrder.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numOrder.Location = new System.Drawing.Point(145, 204);
            this.numOrder.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numOrder.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numOrder.Name = "numOrder";
            this.numOrder.Size = new System.Drawing.Size(100, 24);
            this.numOrder.TabIndex = 12;
            this.numOrder.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblOrderLabel
            // 
            this.lblOrderLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblOrderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblOrderLabel.Location = new System.Drawing.Point(15, 206);
            this.lblOrderLabel.Name = "lblOrderLabel";
            this.lblOrderLabel.Size = new System.Drawing.Size(125, 23);
            this.lblOrderLabel.TabIndex = 11;
            this.lblOrderLabel.Text = "ลำดับแสดงผล:";
            // 
            // btnClearParent
            // 
            this.btnClearParent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearParent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnClearParent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearParent.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(205)))), ((int)(((byte)(226)))));
            this.btnClearParent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearParent.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnClearParent.Location = new System.Drawing.Point(495, 171);
            this.btnClearParent.Name = "btnClearParent";
            this.btnClearParent.Size = new System.Drawing.Size(62, 26);
            this.btnClearParent.TabIndex = 10;
            this.btnClearParent.Text = "✖ ล้าง";
            this.btnClearParent.UseVisualStyleBackColor = false;
            this.btnClearParent.Click += new System.EventHandler(this.BtnClearParent_Click);
            // 
            // btnPickParent
            // 
            this.btnPickParent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickParent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.btnPickParent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPickParent.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(67)))), ((int)(((byte)(122)))));
            this.btnPickParent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPickParent.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            this.btnPickParent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(67)))), ((int)(((byte)(122)))));
            this.btnPickParent.Location = new System.Drawing.Point(423, 171);
            this.btnPickParent.Name = "btnPickParent";
            this.btnPickParent.Size = new System.Drawing.Size(68, 26);
            this.btnPickParent.TabIndex = 9;
            this.btnPickParent.Text = "📂 เลือก";
            this.btnPickParent.UseVisualStyleBackColor = false;
            this.btnPickParent.Click += new System.EventHandler(this.BtnPickParent_Click);
            // 
            // txtParentDisplay
            // 
            this.txtParentDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtParentDisplay.BackColor = System.Drawing.Color.White;
            this.txtParentDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtParentDisplay.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtParentDisplay.Location = new System.Drawing.Point(145, 172);
            this.txtParentDisplay.Name = "txtParentDisplay";
            this.txtParentDisplay.ReadOnly = true;
            this.txtParentDisplay.Size = new System.Drawing.Size(272, 24);
            this.txtParentDisplay.TabIndex = 8;
            this.txtParentDisplay.Text = "(ไม่มี — Root)";
            this.txtParentDisplay.Click += new System.EventHandler(this.TxtParentDisplay_Click);
            // 
            // lblParentLabel
            // 
            this.lblParentLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblParentLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblParentLabel.Location = new System.Drawing.Point(15, 174);
            this.lblParentLabel.Name = "lblParentLabel";
            this.lblParentLabel.Size = new System.Drawing.Size(125, 23);
            this.lblParentLabel.TabIndex = 7;
            this.lblParentLabel.Text = "เมนูแม่ (Parent):";
            // 
            // cmbLevel
            // 
            this.cmbLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbLevel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbLevel.FormattingEnabled = true;
            this.cmbLevel.Items.AddRange(new object[] {
            "แม่ (Root Level)",
            "ลูก (Child Level)"});
            this.cmbLevel.Location = new System.Drawing.Point(145, 139);
            this.cmbLevel.Name = "cmbLevel";
            this.cmbLevel.Size = new System.Drawing.Size(412, 25);
            this.cmbLevel.TabIndex = 6;
            this.cmbLevel.SelectedIndexChanged += new System.EventHandler(this.CmbLevel_SelectedIndexChanged);
            // 
            // lblLevelLabel
            // 
            this.lblLevelLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblLevelLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblLevelLabel.Location = new System.Drawing.Point(15, 141);
            this.lblLevelLabel.Name = "lblLevelLabel";
            this.lblLevelLabel.Size = new System.Drawing.Size(125, 23);
            this.lblLevelLabel.TabIndex = 5;
            this.lblLevelLabel.Text = "ระดับเมนู (Level):";
            // 
            // cmbActionKey
            // 
            this.cmbActionKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbActionKey.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbActionKey.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbActionKey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbActionKey.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbActionKey.FormattingEnabled = true;
            this.cmbActionKey.Location = new System.Drawing.Point(145, 106);
            this.cmbActionKey.Name = "cmbActionKey";
            this.cmbActionKey.Size = new System.Drawing.Size(412, 25);
            this.cmbActionKey.TabIndex = 4;
            // 
            // lblFormClassLabel
            // 
            this.lblFormClassLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblFormClassLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblFormClassLabel.Location = new System.Drawing.Point(15, 108);
            this.lblFormClassLabel.Name = "lblFormClassLabel";
            this.lblFormClassLabel.Size = new System.Drawing.Size(125, 23);
            this.lblFormClassLabel.TabIndex = 3;
            this.lblFormClassLabel.Text = "Action Key:*";
            // 
            // txtKey
            // 
            this.txtKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKey.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.txtKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtKey.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtKey.Location = new System.Drawing.Point(145, 74);
            this.txtKey.Name = "txtKey";
            this.txtKey.ReadOnly = true;
            this.txtKey.Size = new System.Drawing.Size(412, 24);
            this.txtKey.TabIndex = 2;
            // 
            // lblKeyLabel
            // 
            this.lblKeyLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblKeyLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblKeyLabel.Location = new System.Drawing.Point(15, 76);
            this.lblKeyLabel.Name = "lblKeyLabel";
            this.lblKeyLabel.Size = new System.Drawing.Size(125, 23);
            this.lblKeyLabel.TabIndex = 1;
            this.lblKeyLabel.Text = "Key (รหัสอ้างอิง):*";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtName.Location = new System.Drawing.Point(145, 42);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(412, 24);
            this.txtName.TabIndex = 0;
            this.txtName.TextChanged += new System.EventHandler(this.TxtName_TextChanged);
            // 
            // lblNameLabel
            // 
            this.lblNameLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblNameLabel.Location = new System.Drawing.Point(15, 44);
            this.lblNameLabel.Name = "lblNameLabel";
            this.lblNameLabel.Size = new System.Drawing.Size(125, 23);
            this.lblNameLabel.TabIndex = 0;
            this.lblNameLabel.Text = "ชื่อเมนู (Name):*";
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(241)))), ((int)(((byte)(250)))));
            this.lblID.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblID.Location = new System.Drawing.Point(145, 14);
            this.lblID.Name = "lblID";
            this.lblID.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
            this.lblID.Size = new System.Drawing.Size(25, 21);
            this.lblID.TabIndex = 0;
            this.lblID.Text = "-";
            // 
            // lblIDLabel
            // 
            this.lblIDLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblIDLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblIDLabel.Location = new System.Drawing.Point(15, 15);
            this.lblIDLabel.Name = "lblIDLabel";
            this.lblIDLabel.Size = new System.Drawing.Size(125, 23);
            this.lblIDLabel.TabIndex = 0;
            this.lblIDLabel.Text = "รหัสเมนู (ID):";
            // 
            // txtFormClass
            // 
            this.txtFormClass.Location = new System.Drawing.Point(0, 0);
            this.txtFormClass.Name = "txtFormClass";
            this.txtFormClass.Size = new System.Drawing.Size(100, 20);
            this.txtFormClass.TabIndex = 0;
            this.txtFormClass.Visible = false;
            // 
            // cmbParent
            // 
            this.cmbParent.Location = new System.Drawing.Point(0, 0);
            this.cmbParent.Name = "cmbParent";
            this.cmbParent.Size = new System.Drawing.Size(121, 21);
            this.cmbParent.TabIndex = 0;
            this.cmbParent.Visible = false;
            // 
            // pnlActions
            // 
            this.pnlActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.pnlActions.Controls.Add(this.btnDelete);
            this.pnlActions.Controls.Add(this.btnCancel);
            this.pnlActions.Controls.Add(this.btnSave);
            this.pnlActions.Controls.Add(this.btnEdit);
            this.pnlActions.Controls.Add(this.btnAdd);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActions.Location = new System.Drawing.Point(0, 486);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlActions.Size = new System.Drawing.Size(577, 60);
            this.pnlActions.TabIndex = 2;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(475, 12);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(90, 36);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "🗑️ ลบเมนู";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(318, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(92, 36);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "✖️ ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(135)))), ((int)(((byte)(84)))));
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(216, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(96, 36);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "💾 บันทึก";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(119)))), ((int)(((byte)(6)))));
            this.btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnEdit.ForeColor = System.Drawing.Color.White;
            this.btnEdit.Location = new System.Drawing.Point(120, 12);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(90, 36);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "✏️ แก้ไข";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(12, 12);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(102, 36);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "➕ เพิ่มใหม่";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // pnlCardHeader
            // 
            this.pnlCardHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(241)))), ((int)(((byte)(250)))));
            this.pnlCardHeader.Controls.Add(this.lblCardTitle);
            this.pnlCardHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCardHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlCardHeader.Name = "pnlCardHeader";
            this.pnlCardHeader.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlCardHeader.Size = new System.Drawing.Size(577, 42);
            this.pnlCardHeader.TabIndex = 0;
            // 
            // lblCardTitle
            // 
            this.lblCardTitle.AutoSize = true;
            this.lblCardTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblCardTitle.Location = new System.Drawing.Point(10, 11);
            this.lblCardTitle.Name = "lblCardTitle";
            this.lblCardTitle.Size = new System.Drawing.Size(201, 19);
            this.lblCardTitle.TabIndex = 0;
            this.lblCardTitle.Text = "📝 รายละเอียดเมนู (Menu Details)";
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.pnlFooter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFooter.Controls.Add(this.lblStats);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 630);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(16, 6, 16, 6);
            this.pnlFooter.Size = new System.Drawing.Size(984, 32);
            this.pnlFooter.TabIndex = 2;
            // 
            // lblStats
            // 
            this.lblStats.AutoSize = true;
            this.lblStats.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblStats.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblStats.Location = new System.Drawing.Point(14, 8);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(298, 15);
            this.lblStats.TabIndex = 0;
            this.lblStats.Text = "📊 รวมทั้งหมด: 0 รายการ | 🟢 ใช้งาน: 0 | ⚪ ปิดใช้งาน: 0";
            // 
            // MenuManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(251)))));
            this.ClientSize = new System.Drawing.Size(984, 662);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(880, 580);
            this.Name = "MenuManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Menu Management — CMS";
            this.Load += new System.EventHandler(this.MenuManagementForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlTreeToolbar.ResumeLayout(false);
            this.pnlTreeSearch.ResumeLayout(false);
            this.pnlTreeSearch.PerformLayout();
            this.pnlTreeHeader.ResumeLayout(false);
            this.pnlTreeHeader.PerformLayout();
            this.pnlCard.ResumeLayout(false);
            this.pnlFormContent.ResumeLayout(false);
            this.pnlFormContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOrder)).EndInit();
            this.pnlActions.ResumeLayout(false);
            this.pnlCardHeader.ResumeLayout(false);
            this.pnlCardHeader.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
