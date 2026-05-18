namespace CMS.Presentation
{
    partial class MenuManagementForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Layout containers ────────────────────────────────────────────────
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitRight;

        // ── Left panel ───────────────────────────────────────────────────────
        private System.Windows.Forms.Label       lblMenuTree;
        private System.Windows.Forms.Panel       pnlTreeToolbar;
        private System.Windows.Forms.Button      btnExpandAll;
        private System.Windows.Forms.Button      btnCollapseAll;
        private System.Windows.Forms.TreeView    treeMenu;

        // ── Detail GroupBox ──────────────────────────────────────────────────
        private System.Windows.Forms.GroupBox      grpDetails;
        private System.Windows.Forms.Label         lblIDLabel;
        private System.Windows.Forms.Label         lblID;
        private System.Windows.Forms.Label         lblNameLabel;
        private System.Windows.Forms.TextBox       txtName;
        private System.Windows.Forms.Label         lblKeyLabel;
        private System.Windows.Forms.TextBox       txtKey;
        private System.Windows.Forms.Label         lblFormClassLabel;
        private System.Windows.Forms.TextBox       txtFormClass;
        private System.Windows.Forms.Label         lblOrderLabel;
        private System.Windows.Forms.NumericUpDown numOrder;
        private System.Windows.Forms.Label         lblRoleLabel;
        private System.Windows.Forms.ComboBox      cmbRole;
        private System.Windows.Forms.Label         lblParentLabel;
        private System.Windows.Forms.ComboBox      cmbParent;
        private System.Windows.Forms.CheckBox      chkActive;

        // ── Actions GroupBox ─────────────────────────────────────────────────
        private System.Windows.Forms.GroupBox grpActions;
        private System.Windows.Forms.Button   btnAdd;
        private System.Windows.Forms.Button   btnEdit;
        private System.Windows.Forms.Button   btnDelete;
        private System.Windows.Forms.Button   btnSave;
        private System.Windows.Forms.Button   btnCancel;
        private System.Windows.Forms.Label    lblStats;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.treeMenu = new System.Windows.Forms.TreeView();
            this.lblMenuTree = new System.Windows.Forms.Label();
            this.pnlTreeToolbar = new System.Windows.Forms.Panel();
            this.btnExpandAll = new System.Windows.Forms.Button();
            this.btnCollapseAll = new System.Windows.Forms.Button();
            this.splitRight = new System.Windows.Forms.SplitContainer();
            this.grpDetails = new System.Windows.Forms.GroupBox();
            this.lblIDLabel = new System.Windows.Forms.Label();
            this.lblID = new System.Windows.Forms.Label();
            this.lblNameLabel = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblKeyLabel = new System.Windows.Forms.Label();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.lblFormClassLabel = new System.Windows.Forms.Label();
            this.txtFormClass = new System.Windows.Forms.TextBox();
            this.lblOrderLabel = new System.Windows.Forms.Label();
            this.numOrder = new System.Windows.Forms.NumericUpDown();
            this.lblRoleLabel = new System.Windows.Forms.Label();
            this.cmbRole = new System.Windows.Forms.ComboBox();
            this.lblParentLabel = new System.Windows.Forms.Label();
            this.cmbParent = new System.Windows.Forms.ComboBox();
            this.chkActive = new System.Windows.Forms.CheckBox();
            this.grpActions = new System.Windows.Forms.GroupBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblStats = new System.Windows.Forms.Label();
            this.pnlTreeToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            this.grpDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOrder)).BeginInit();
            this.grpActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            //
            // splitMain.Panel1
            //
            this.splitMain.Panel1.Controls.Add(this.treeMenu);
            this.splitMain.Panel1.Controls.Add(this.pnlTreeToolbar);
            this.splitMain.Panel1.Controls.Add(this.lblMenuTree);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.splitRight);
            this.splitMain.Size = new System.Drawing.Size(757, 661);
            this.splitMain.SplitterDistance = 353;
            this.splitMain.TabIndex = 0;
            // 
            // treeMenu
            // 
            this.treeMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.treeMenu.FullRowSelect = true;
            this.treeMenu.HideSelection = false;
            this.treeMenu.Location = new System.Drawing.Point(0, 30);
            this.treeMenu.Name = "treeMenu";
            this.treeMenu.Size = new System.Drawing.Size(353, 631);
            this.treeMenu.TabIndex = 0;
            this.treeMenu.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeMenu_AfterSelect);
            //
            // lblMenuTree
            //
            this.lblMenuTree.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMenuTree.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblMenuTree.Location = new System.Drawing.Point(0, 0);
            this.lblMenuTree.Name = "lblMenuTree";
            this.lblMenuTree.Size = new System.Drawing.Size(353, 30);
            this.lblMenuTree.TabIndex = 1;
            this.lblMenuTree.Text = "📂 Menu Hierarchy";
            //
            // pnlTreeToolbar
            //
            this.pnlTreeToolbar.Controls.Add(this.btnCollapseAll);
            this.pnlTreeToolbar.Controls.Add(this.btnExpandAll);
            this.pnlTreeToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTreeToolbar.Location = new System.Drawing.Point(0, 30);
            this.pnlTreeToolbar.Name = "pnlTreeToolbar";
            this.pnlTreeToolbar.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pnlTreeToolbar.Size = new System.Drawing.Size(353, 30);
            this.pnlTreeToolbar.TabIndex = 2;
            //
            // btnExpandAll
            //
            this.btnExpandAll.Location = new System.Drawing.Point(4, 3);
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(110, 24);
            this.btnExpandAll.TabIndex = 0;
            this.btnExpandAll.Text = "⊞ ขยายทั้งหมด";
            this.btnExpandAll.Click += new System.EventHandler(this.BtnExpandAll_Click);
            //
            // btnCollapseAll
            //
            this.btnCollapseAll.Location = new System.Drawing.Point(118, 3);
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Size = new System.Drawing.Size(110, 24);
            this.btnCollapseAll.TabIndex = 1;
            this.btnCollapseAll.Text = "⊟ ย่อทั้งหมด";
            this.btnCollapseAll.Click += new System.EventHandler(this.BtnCollapseAll_Click);
            // 
            // splitRight
            // 
            this.splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRight.Location = new System.Drawing.Point(0, 0);
            this.splitRight.Name = "splitRight";
            this.splitRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitRight.Panel1
            // 
            this.splitRight.Panel1.Controls.Add(this.grpDetails);
            // 
            // splitRight.Panel2
            // 
            this.splitRight.Panel2.Controls.Add(this.grpActions);
            this.splitRight.Size = new System.Drawing.Size(400, 661);
            this.splitRight.SplitterDistance = 469;
            this.splitRight.TabIndex = 0;
            // 
            // grpDetails
            // 
            this.grpDetails.Controls.Add(this.lblIDLabel);
            this.grpDetails.Controls.Add(this.lblID);
            this.grpDetails.Controls.Add(this.lblNameLabel);
            this.grpDetails.Controls.Add(this.txtName);
            this.grpDetails.Controls.Add(this.lblKeyLabel);
            this.grpDetails.Controls.Add(this.txtKey);
            this.grpDetails.Controls.Add(this.lblFormClassLabel);
            this.grpDetails.Controls.Add(this.txtFormClass);
            this.grpDetails.Controls.Add(this.lblOrderLabel);
            this.grpDetails.Controls.Add(this.numOrder);
            this.grpDetails.Controls.Add(this.lblRoleLabel);
            this.grpDetails.Controls.Add(this.cmbRole);
            this.grpDetails.Controls.Add(this.lblParentLabel);
            this.grpDetails.Controls.Add(this.cmbParent);
            this.grpDetails.Controls.Add(this.chkActive);
            this.grpDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDetails.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpDetails.Location = new System.Drawing.Point(0, 0);
            this.grpDetails.Name = "grpDetails";
            this.grpDetails.Padding = new System.Windows.Forms.Padding(10);
            this.grpDetails.Size = new System.Drawing.Size(400, 469);
            this.grpDetails.TabIndex = 0;
            this.grpDetails.TabStop = false;
            this.grpDetails.Text = "📝 Menu Details";
            // 
            // lblIDLabel
            // 
            this.lblIDLabel.Location = new System.Drawing.Point(10, 25);
            this.lblIDLabel.Name = "lblIDLabel";
            this.lblIDLabel.Size = new System.Drawing.Size(100, 23);
            this.lblIDLabel.TabIndex = 0;
            this.lblIDLabel.Text = "ID:";
            // 
            // lblID
            // 
            this.lblID.Location = new System.Drawing.Point(120, 25);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(200, 23);
            this.lblID.TabIndex = 1;
            this.lblID.Text = "-";
            // 
            // lblNameLabel
            // 
            this.lblNameLabel.Location = new System.Drawing.Point(10, 55);
            this.lblNameLabel.Name = "lblNameLabel";
            this.lblNameLabel.Size = new System.Drawing.Size(100, 23);
            this.lblNameLabel.TabIndex = 2;
            this.lblNameLabel.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(120, 52);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(250, 23);
            this.txtName.TabIndex = 3;
            this.txtName.TextChanged += new System.EventHandler(this.TxtName_TextChanged);
            // 
            // lblKeyLabel
            // 
            this.lblKeyLabel.Location = new System.Drawing.Point(10, 85);
            this.lblKeyLabel.Name = "lblKeyLabel";
            this.lblKeyLabel.Size = new System.Drawing.Size(100, 23);
            this.lblKeyLabel.TabIndex = 4;
            this.lblKeyLabel.Text = "Key:";
            // 
            // txtKey
            // 
            this.txtKey.Location = new System.Drawing.Point(120, 82);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(250, 23);
            this.txtKey.TabIndex = 5;
            // 
            // lblFormClassLabel
            // 
            this.lblFormClassLabel.Location = new System.Drawing.Point(10, 115);
            this.lblFormClassLabel.Name = "lblFormClassLabel";
            this.lblFormClassLabel.Size = new System.Drawing.Size(100, 23);
            this.lblFormClassLabel.TabIndex = 6;
            this.lblFormClassLabel.Text = "Form Class:";
            // 
            // txtFormClass
            // 
            this.txtFormClass.Location = new System.Drawing.Point(120, 112);
            this.txtFormClass.Name = "txtFormClass";
            this.txtFormClass.Size = new System.Drawing.Size(250, 23);
            this.txtFormClass.TabIndex = 7;
            // 
            // lblOrderLabel
            // 
            this.lblOrderLabel.Location = new System.Drawing.Point(10, 145);
            this.lblOrderLabel.Name = "lblOrderLabel";
            this.lblOrderLabel.Size = new System.Drawing.Size(100, 23);
            this.lblOrderLabel.TabIndex = 8;
            this.lblOrderLabel.Text = "Display Order:";
            // 
            // numOrder
            // 
            this.numOrder.Location = new System.Drawing.Point(120, 142);
            this.numOrder.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numOrder.Name = "numOrder";
            this.numOrder.Size = new System.Drawing.Size(100, 23);
            this.numOrder.TabIndex = 9;
            // 
            // lblRoleLabel
            // 
            this.lblRoleLabel.Location = new System.Drawing.Point(10, 175);
            this.lblRoleLabel.Name = "lblRoleLabel";
            this.lblRoleLabel.Size = new System.Drawing.Size(100, 23);
            this.lblRoleLabel.TabIndex = 10;
            this.lblRoleLabel.Text = "Role Required:";
            // 
            // cmbRole
            // 
            this.cmbRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRole.Items.AddRange(new object[] {
            "(All users)",
            "1 - Officer",
            "2 - Manager",
            "3 - Approver",
            "4 - Admin"});
            this.cmbRole.Location = new System.Drawing.Point(120, 172);
            this.cmbRole.Name = "cmbRole";
            this.cmbRole.Size = new System.Drawing.Size(200, 23);
            this.cmbRole.TabIndex = 11;
            // 
            // lblParentLabel
            // 
            this.lblParentLabel.Location = new System.Drawing.Point(10, 205);
            this.lblParentLabel.Name = "lblParentLabel";
            this.lblParentLabel.Size = new System.Drawing.Size(100, 23);
            this.lblParentLabel.TabIndex = 12;
            this.lblParentLabel.Text = "Parent Menu:";
            // 
            // cmbParent
            // 
            this.cmbParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbParent.Location = new System.Drawing.Point(120, 202);
            this.cmbParent.Name = "cmbParent";
            this.cmbParent.Size = new System.Drawing.Size(250, 23);
            this.cmbParent.TabIndex = 13;
            // 
            // chkActive
            // 
            this.chkActive.Checked = true;
            this.chkActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkActive.Location = new System.Drawing.Point(120, 235);
            this.chkActive.Name = "chkActive";
            this.chkActive.Size = new System.Drawing.Size(100, 24);
            this.chkActive.TabIndex = 14;
            this.chkActive.Text = "Active";            // 
            // grpActions
            // 
            this.grpActions.Controls.Add(this.btnAdd);
            this.grpActions.Controls.Add(this.btnEdit);
            this.grpActions.Controls.Add(this.btnDelete);
            this.grpActions.Controls.Add(this.btnSave);
            this.grpActions.Controls.Add(this.btnCancel);
            this.grpActions.Controls.Add(this.lblStats);
            this.grpActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpActions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpActions.Location = new System.Drawing.Point(0, 0);
            this.grpActions.Name = "grpActions";
            this.grpActions.Padding = new System.Windows.Forms.Padding(10);
            this.grpActions.Size = new System.Drawing.Size(400, 188);
            this.grpActions.TabIndex = 0;
            this.grpActions.TabStop = false;
            this.grpActions.Text = "⚙️ Actions";
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(10, 22);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(120, 35);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "➕ Add Menu";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(140, 22);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(120, 35);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "✏️ Edit";
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(270, 22);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(120, 35);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "🗑️ Delete";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.Green;
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(10, 65);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(120, 35);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "💾 Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(140, 65);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 35);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "❌ Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblStats
            // 
            this.lblStats.Location = new System.Drawing.Point(10, 110);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(500, 30);
            this.lblStats.TabIndex = 5;
            this.lblStats.Text = "Statistics: loading...";
            // 
            // MenuManagementForm
            // 
            this.ClientSize = new System.Drawing.Size(757, 661);
            this.Controls.Add(this.splitMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "MenuManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "📋 Menu Management — CMS";
            this.Load += new System.EventHandler(this.MenuManagementForm_Load);
            this.pnlTreeToolbar.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.ResumeLayout(false);
            this.grpDetails.ResumeLayout(false);
            this.grpDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOrder)).EndInit();
            this.grpActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
