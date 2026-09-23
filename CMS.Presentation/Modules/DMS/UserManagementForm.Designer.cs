namespace CMS.Presentation
{
    partial class UserManagementForm
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
            this.pnlHeader      = new System.Windows.Forms.Panel();
            this.lblHeader      = new System.Windows.Forms.Label();
            this.pnlToolbar     = new System.Windows.Forms.Panel();
            this.txtSearch      = new System.Windows.Forms.TextBox();
            this.cmbStatus      = new System.Windows.Forms.ComboBox();
            this.cmbFilterColumn = new System.Windows.Forms.ComboBox();
            this.btnSearch      = new System.Windows.Forms.Button();
            this.btnCreate      = new System.Windows.Forms.Button();
            this.btnEdit        = new System.Windows.Forms.Button();
            this.btnToggle      = new System.Windows.Forms.Button();
            this.btnRefresh     = new System.Windows.Forms.Button();
            this.lvUsers        = new System.Windows.Forms.ListView();
            this.colID          = new System.Windows.Forms.ColumnHeader();
            this.colAD          = new System.Windows.Forms.ColumnHeader();
            this.colDisplay     = new System.Windows.Forms.ColumnHeader();
            this.colEmployee    = new System.Windows.Forms.ColumnHeader();
            this.colDept        = new System.Windows.Forms.ColumnHeader();
            this.colDiv         = new System.Windows.Forms.ColumnHeader();
            this.colPosition    = new System.Windows.Forms.ColumnHeader();
            this.colLevel       = new System.Windows.Forms.ColumnHeader();
            this.colRole        = new System.Windows.Forms.ColumnHeader();
            this.colStatus      = new System.Windows.Forms.ColumnHeader();
            this.colLastLogin   = new System.Windows.Forms.ColumnHeader();
            this.lblStatusBar   = new System.Windows.Forms.Label();

            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height   = 44;
            this.pnlHeader.Name     = "pnlHeader";
            this.pnlHeader.Padding  = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.pnlHeader.TabIndex = 0;
            //
            // lblHeader
            //
            this.lblHeader.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Name      = "lblHeader";
            this.lblHeader.Text      = "👤  จัดการบัญชีผู้ใช้งาน";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── pnlToolbar ───────────────────────────────────────────────────
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(243, 245, 250);
            this.pnlToolbar.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.txtSearch, this.cmbStatus, this.cmbFilterColumn, this.btnSearch,
                this.btnCreate, this.btnEdit, this.btnToggle, this.btnRefresh });
            this.pnlToolbar.Dock    = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height  = 46;
            this.pnlToolbar.Name    = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(6, 8, 6, 0);
            this.pnlToolbar.TabIndex = 1;
            //
            // txtSearch
            //
            this.txtSearch.Left  = 6;
            this.txtSearch.Top   = 10;
            this.txtSearch.Width = 200;
            this.txtSearch.Name  = "txtSearch";
            this.txtSearch.Font  = new System.Drawing.Font("Segoe UI", 10F);
            //
            // cmbStatus
            //
            this.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatus.Items.AddRange(new object[] { "ทุกสถานะ", "ใช้งาน", "ปิดใช้งาน" });
            this.cmbStatus.Left           = 214;
            this.cmbStatus.Top            = 8;
            this.cmbStatus.Width          = 120;
            this.cmbStatus.Name           = "cmbStatus";
            this.cmbStatus.Font           = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbStatus.SelectedIndex  = 0;
            //
            // cmbFilterColumn
            //
            this.cmbFilterColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterColumn.Items.AddRange(new object[] {
                "ทุกคอลัมน์", "ชื่อผู้ใช้ AD", "ชื่อ-นามสกุล", "รหัสพนักงาน",
                "แผนก", "ฝ่าย", "ตำแหน่ง", "บทบาท" });
            this.cmbFilterColumn.Left          = 342;
            this.cmbFilterColumn.Top           = 8;
            this.cmbFilterColumn.Width         = 130;
            this.cmbFilterColumn.Name          = "cmbFilterColumn";
            this.cmbFilterColumn.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbFilterColumn.SelectedIndex = 0;
            //
            // btnSearch
            //
            this.btnSearch.Left      = 480;
            this.btnSearch.Top       = 8;
            this.btnSearch.Width     = 80;
            this.btnSearch.Height    = 28;
            this.btnSearch.Name      = "btnSearch";
            this.btnSearch.Text      = "🔍 ค้นหา";
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font      = new System.Drawing.Font("Segoe UI", 9F);
            //
            // btnCreate
            //
            this.btnCreate.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreate.ForeColor = System.Drawing.Color.White;
            this.btnCreate.Left      = 578;
            this.btnCreate.Top       = 8;
            this.btnCreate.Width     = 100;
            this.btnCreate.Height    = 28;
            this.btnCreate.Name      = "btnCreate";
            this.btnCreate.Text      = "➕ เพิ่มผู้ใช้";
            this.btnCreate.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCreate.FlatAppearance.BorderSize = 0;
            //
            // btnEdit
            //
            this.btnEdit.Enabled  = false;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Left     = 686;
            this.btnEdit.Top      = 8;
            this.btnEdit.Width    = 90;
            this.btnEdit.Height   = 28;
            this.btnEdit.Name     = "btnEdit";
            this.btnEdit.Text     = "✏️ แก้ไข";
            this.btnEdit.Font     = new System.Drawing.Font("Segoe UI", 9F);
            //
            // btnToggle
            //
            this.btnToggle.Enabled  = false;
            this.btnToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggle.Left     = 784;
            this.btnToggle.Top      = 8;
            this.btnToggle.Width    = 110;
            this.btnToggle.Height   = 28;
            this.btnToggle.Name     = "btnToggle";
            this.btnToggle.Text     = "🔒 ปิดใช้งาน";
            this.btnToggle.Font     = new System.Drawing.Font("Segoe UI", 9F);
            //
            // btnRefresh
            //
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Left      = 902;
            this.btnRefresh.Top       = 8;
            this.btnRefresh.Width     = 80;
            this.btnRefresh.Height    = 28;
            this.btnRefresh.Name      = "btnRefresh";
            this.btnRefresh.Text      = "🔄 รีเฟรช";
            this.btnRefresh.Font      = new System.Drawing.Font("Segoe UI", 9F);

            // ── lvUsers ──────────────────────────────────────────────────────
            this.lvUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colID, this.colAD, this.colDisplay, this.colEmployee,
                this.colDept, this.colDiv, this.colPosition, this.colLevel,
                this.colRole, this.colStatus, this.colLastLogin });
            this.lvUsers.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.lvUsers.Font          = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lvUsers.FullRowSelect = true;
            this.lvUsers.GridLines     = true;
            this.lvUsers.HideSelection = false;
            this.lvUsers.MultiSelect   = true;
            this.lvUsers.Name          = "lvUsers";
            this.lvUsers.View          = System.Windows.Forms.View.Details;
            this.lvUsers.TabIndex      = 2;
            this.colID.Text        = "ID";           this.colID.Width        =  45;
            this.colAD.Text        = "ชื่อผู้ใช้ AD"; this.colAD.Width        = 140;
            this.colDisplay.Text   = "ชื่อ-นามสกุล"; this.colDisplay.Width   = 160;
            this.colEmployee.Text  = "รหัสพนักงาน";  this.colEmployee.Width  =  95;
            this.colDept.Text      = "แผนก";          this.colDept.Width      = 150;
            this.colDiv.Text       = "ฝ่าย";          this.colDiv.Width       = 140;
            this.colPosition.Text  = "ตำแหน่ง";       this.colPosition.Width  = 130;
            this.colLevel.Text     = "Level";          this.colLevel.Width     =  65;
            this.colRole.Text      = "บทบาท";         this.colRole.Width      =  90;
            this.colStatus.Text    = "สถานะ";         this.colStatus.Width    =  70;
            this.colLastLogin.Text = "เข้าสู่ระบบล่าสุด"; this.colLastLogin.Width = 130;

            // ── lblStatusBar ─────────────────────────────────────────────────
            this.lblStatusBar.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.lblStatusBar.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatusBar.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusBar.ForeColor = System.Drawing.Color.Gray;
            this.lblStatusBar.Height    = 24;
            this.lblStatusBar.Name      = "lblStatusBar";
            this.lblStatusBar.Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblStatusBar.Text      = "พร้อมใช้งาน";
            this.lblStatusBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── Form ─────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.ClientSize          = new System.Drawing.Size(1100, 680);
            this.Controls.Add(this.lvUsers);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.lblStatusBar);
            this.Font                = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize         = new System.Drawing.Size(900, 500);
            this.Name                = "UserManagementForm";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text                = "จัดการบัญชีผู้ใช้งาน";

            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.ResumeLayout(false);
        }

        // ── Control declarations ─────────────────────────────────────────────
        private System.Windows.Forms.Panel          pnlHeader;
        private System.Windows.Forms.Label          lblHeader;
        private System.Windows.Forms.Panel          pnlToolbar;
        private System.Windows.Forms.TextBox        txtSearch;
        private System.Windows.Forms.ComboBox       cmbStatus;
        private System.Windows.Forms.ComboBox       cmbFilterColumn;
        private System.Windows.Forms.Button         btnSearch;
        private System.Windows.Forms.Button         btnCreate;
        private System.Windows.Forms.Button         btnEdit;
        private System.Windows.Forms.Button         btnToggle;
        private System.Windows.Forms.Button         btnRefresh;
        private System.Windows.Forms.ListView       lvUsers;
        private System.Windows.Forms.ColumnHeader   colID;
        private System.Windows.Forms.ColumnHeader   colAD;
        private System.Windows.Forms.ColumnHeader   colDisplay;
        private System.Windows.Forms.ColumnHeader   colEmployee;
        private System.Windows.Forms.ColumnHeader   colDept;
        private System.Windows.Forms.ColumnHeader   colDiv;
        private System.Windows.Forms.ColumnHeader   colPosition;
        private System.Windows.Forms.ColumnHeader   colLevel;
        private System.Windows.Forms.ColumnHeader   colRole;
        private System.Windows.Forms.ColumnHeader   colStatus;
        private System.Windows.Forms.ColumnHeader   colLastLogin;
        private System.Windows.Forms.Label          lblStatusBar;
    }
}
