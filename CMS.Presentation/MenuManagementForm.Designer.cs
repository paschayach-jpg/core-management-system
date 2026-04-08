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
            this.components  = new System.ComponentModel.Container();
            this.splitMain   = new System.Windows.Forms.SplitContainer();
            this.splitRight  = new System.Windows.Forms.SplitContainer();
            this.lblMenuTree = new System.Windows.Forms.Label();
            this.treeMenu    = new System.Windows.Forms.TreeView();

            this.grpDetails      = new System.Windows.Forms.GroupBox();
            this.lblIDLabel      = new System.Windows.Forms.Label();
            this.lblID           = new System.Windows.Forms.Label();
            this.lblNameLabel    = new System.Windows.Forms.Label();
            this.txtName         = new System.Windows.Forms.TextBox();
            this.lblKeyLabel     = new System.Windows.Forms.Label();
            this.txtKey          = new System.Windows.Forms.TextBox();
            this.lblFormClassLabel = new System.Windows.Forms.Label();
            this.txtFormClass    = new System.Windows.Forms.TextBox();
            this.lblOrderLabel   = new System.Windows.Forms.Label();
            this.numOrder        = new System.Windows.Forms.NumericUpDown();
            this.lblRoleLabel    = new System.Windows.Forms.Label();
            this.cmbRole         = new System.Windows.Forms.ComboBox();
            this.chkActive       = new System.Windows.Forms.CheckBox();

            this.grpActions = new System.Windows.Forms.GroupBox();
            this.btnAdd     = new System.Windows.Forms.Button();
            this.btnEdit    = new System.Windows.Forms.Button();
            this.btnDelete  = new System.Windows.Forms.Button();
            this.btnSave    = new System.Windows.Forms.Button();
            this.btnCancel  = new System.Windows.Forms.Button();
            this.lblStats   = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOrder)).BeginInit();
            this.grpDetails.SuspendLayout();
            this.grpActions.SuspendLayout();
            this.SuspendLayout();

            // ── splitMain ────────────────────────────────────────────────────
            this.splitMain.Dock             = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.SplitterDistance = 350;
            this.splitMain.Orientation      = System.Windows.Forms.Orientation.Vertical;
            this.splitMain.Panel1.Controls.Add(this.treeMenu);
            this.splitMain.Panel1.Controls.Add(this.lblMenuTree);
            this.splitMain.Panel2.Controls.Add(this.splitRight);

            // ── lblMenuTree ──────────────────────────────────────────────────
            this.lblMenuTree.Text      = "📂 Menu Hierarchy";
            this.lblMenuTree.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblMenuTree.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblMenuTree.Height    = 30;

            // ── treeMenu ─────────────────────────────────────────────────────
            this.treeMenu.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.treeMenu.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.treeMenu.FullRowSelect = true;
            this.treeMenu.HideSelection = false;
            this.treeMenu.AfterSelect  += new System.Windows.Forms.TreeViewEventHandler(this.TreeMenu_AfterSelect);

            // ── splitRight ───────────────────────────────────────────────────
            this.splitRight.Dock             = System.Windows.Forms.DockStyle.Fill;
            this.splitRight.Orientation      = System.Windows.Forms.Orientation.Horizontal;
            this.splitRight.SplitterDistance = 320;
            this.splitRight.Panel1.Controls.Add(this.grpDetails);
            this.splitRight.Panel2.Controls.Add(this.grpActions);

            // ── grpDetails ───────────────────────────────────────────────────
            this.grpDetails.Text    = "📝 Menu Details";
            this.grpDetails.Dock    = System.Windows.Forms.DockStyle.Fill;
            this.grpDetails.Padding = new System.Windows.Forms.Padding(10);
            this.grpDetails.Font    = new System.Drawing.Font("Segoe UI", 9F);

            // row y=20 — ID
            this.lblIDLabel.Text   = "ID:";
            this.lblIDLabel.Left   = 10;  this.lblIDLabel.Top = 25; this.lblIDLabel.Width = 100;
            this.lblID.Text        = "-";
            this.lblID.Left        = 120; this.lblID.Top = 25;      this.lblID.Width = 200;

            // row y=55 — Name
            this.lblNameLabel.Text = "Name:";
            this.lblNameLabel.Left = 10;  this.lblNameLabel.Top = 55; this.lblNameLabel.Width = 100;
            this.txtName.Left      = 120; this.txtName.Top = 52;      this.txtName.Width = 250;

            // row y=85 — Key
            this.lblKeyLabel.Text  = "Key:";
            this.lblKeyLabel.Left  = 10;  this.lblKeyLabel.Top = 85; this.lblKeyLabel.Width = 100;
            this.txtKey.Left       = 120; this.txtKey.Top = 82;      this.txtKey.Width = 250;

            // row y=115 — Form Class
            this.lblFormClassLabel.Text = "Form Class:";
            this.lblFormClassLabel.Left = 10;  this.lblFormClassLabel.Top = 115; this.lblFormClassLabel.Width = 100;
            this.txtFormClass.Left      = 120; this.txtFormClass.Top = 112;      this.txtFormClass.Width = 250;

            // row y=145 — Display Order
            this.lblOrderLabel.Text    = "Display Order:";
            this.lblOrderLabel.Left    = 10;  this.lblOrderLabel.Top = 145; this.lblOrderLabel.Width = 100;
            this.numOrder.Left         = 120; this.numOrder.Top = 142;      this.numOrder.Width = 100;
            this.numOrder.Minimum      = 0;
            this.numOrder.Maximum      = 9999;

            // row y=175 — Role Required
            this.lblRoleLabel.Text     = "Role Required:";
            this.lblRoleLabel.Left     = 10;  this.lblRoleLabel.Top = 175; this.lblRoleLabel.Width = 100;
            this.cmbRole.Left          = 120; this.cmbRole.Top = 172;      this.cmbRole.Width = 200;
            this.cmbRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRole.Items.AddRange(new object[] { "(All users)", "1 - Officer", "2 - Manager", "3 - Approver", "4 - Admin" });
            this.cmbRole.SelectedIndex = 0;

            // row y=205 — Active
            this.chkActive.Text    = "Active";
            this.chkActive.Left    = 120; this.chkActive.Top = 205; this.chkActive.Width = 100;
            this.chkActive.Checked = true;

            this.grpDetails.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblIDLabel, this.lblID,
                this.lblNameLabel, this.txtName,
                this.lblKeyLabel, this.txtKey,
                this.lblFormClassLabel, this.txtFormClass,
                this.lblOrderLabel, this.numOrder,
                this.lblRoleLabel, this.cmbRole,
                this.chkActive
            });

            // ── grpActions ───────────────────────────────────────────────────
            this.grpActions.Text    = "⚙️ Actions";
            this.grpActions.Dock    = System.Windows.Forms.DockStyle.Fill;
            this.grpActions.Padding = new System.Windows.Forms.Padding(10);
            this.grpActions.Font    = new System.Drawing.Font("Segoe UI", 9F);

            // Row 1 buttons
            this.btnAdd.Text     = "➕ Add Menu";
            this.btnAdd.Left     = 10;  this.btnAdd.Top = 22;  this.btnAdd.Width = 120; this.btnAdd.Height = 35;
            this.btnAdd.Click   += new System.EventHandler(this.btnAdd_Click);

            this.btnEdit.Text    = "✏️ Edit";
            this.btnEdit.Left    = 140; this.btnEdit.Top = 22;  this.btnEdit.Width = 120; this.btnEdit.Height = 35;
            this.btnEdit.Click  += new System.EventHandler(this.btnEdit_Click);

            this.btnDelete.Text  = "🗑️ Delete";
            this.btnDelete.Left  = 270; this.btnDelete.Top = 22; this.btnDelete.Width = 120; this.btnDelete.Height = 35;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            // Row 2 buttons
            this.btnSave.Text       = "💾 Save";
            this.btnSave.Left       = 10;  this.btnSave.Top = 65;  this.btnSave.Width = 120; this.btnSave.Height = 35;
            this.btnSave.BackColor  = System.Drawing.Color.Green;
            this.btnSave.ForeColor  = System.Drawing.Color.White;
            this.btnSave.Click     += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Text     = "❌ Cancel";
            this.btnCancel.Left     = 140; this.btnCancel.Top = 65; this.btnCancel.Width = 120; this.btnCancel.Height = 35;
            this.btnCancel.Click   += new System.EventHandler(this.btnCancel_Click);

            // Stats label
            this.lblStats.Text   = "Statistics: loading...";
            this.lblStats.Left   = 10;  this.lblStats.Top = 110; this.lblStats.Width = 500; this.lblStats.Height = 30;

            this.grpActions.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.btnAdd, this.btnEdit, this.btnDelete,
                this.btnSave, this.btnCancel,
                this.lblStats
            });

            // ── Form ─────────────────────────────────────────────────────────
            this.Text            = "📋 Menu Management — CMS";
            this.Size            = new System.Drawing.Size(1200, 700);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.Controls.Add(this.splitMain);
            this.Load           += new System.EventHandler(this.MenuManagementForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            this.splitRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numOrder)).EndInit();
            this.grpDetails.ResumeLayout(false);
            this.grpDetails.PerformLayout();
            this.grpActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
