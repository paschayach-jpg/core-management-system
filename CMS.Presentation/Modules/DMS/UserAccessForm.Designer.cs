namespace CMS.Presentation
{
    partial class UserAccessForm
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
            this.lblHeader = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabUsers = new System.Windows.Forms.TabPage();
            this._lvUsers = new System.Windows.Forms.ListView();
            this.colDisplayName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colADUser = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDept = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDiv = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRole = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlBtns = new System.Windows.Forms.Panel();
            this._btnChangeRole = new System.Windows.Forms.Button();
            this._btnDeactivate = new System.Windows.Forms.Button();
            this._btnAssignOrg = new System.Windows.Forms.Button();
            this._btnRefresh = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.lblFind = new System.Windows.Forms.Label();
            this._txtSearch = new System.Windows.Forms.TextBox();
            this.lblRole = new System.Windows.Forms.Label();
            this._cmbRoleFilter = new System.Windows.Forms.ComboBox();
            this.btnFind = new System.Windows.Forms.Button();
            this.lblDept = new System.Windows.Forms.Label();
            this._cmbDeptFilter = new System.Windows.Forms.ComboBox();
            this.tabUserStatus = new System.Windows.Forms.TabControl();
            this.tabActiveUsers = new System.Windows.Forms.TabPage();
            this.tabInactiveUsers = new System.Windows.Forms.TabPage();
            this.tabOrg = new System.Windows.Forms.TabPage();
            this.splitOrg = new System.Windows.Forms.SplitContainer();
            this.pnlOrgLeft = new System.Windows.Forms.Panel();
            this._orgTree = new System.Windows.Forms.TreeView();
            this.lblOrgTree = new System.Windows.Forms.Label();
            this.pnlOrgRight = new System.Windows.Forms.Panel();
            this._lvOrgUsers = new System.Windows.Forms.ListView();
            this.colOrgName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colOrgAD = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colOrgRole = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colOrgStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlOrgBtns = new System.Windows.Forms.Panel();
            this._btnChangeOrg = new System.Windows.Forms.Button();
            this._btnAddUserToOrg = new System.Windows.Forms.Button();
            this._lblOrgStatus = new System.Windows.Forms.Label();
            this.lblOrgUsers = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabUsers.SuspendLayout();
            this.pnlBtns.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.tabUserStatus.SuspendLayout();
            this.tabOrg.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitOrg)).BeginInit();
            this.splitOrg.Panel1.SuspendLayout();
            this.splitOrg.Panel2.SuspendLayout();
            this.splitOrg.SuspendLayout();
            this.pnlOrgLeft.SuspendLayout();
            this.pnlOrgRight.SuspendLayout();
            this.pnlOrgBtns.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.pnlHeader.Size = new System.Drawing.Size(960, 44);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(14, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(946, 44);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "⚙  จัดการสิทธิ์การเข้าถึง";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabUsers);
            this.tabControl.Controls.Add(this.tabOrg);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabControl.Location = new System.Drawing.Point(0, 44);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(960, 636);
            this.tabControl.TabIndex = 1;
            // 
            // tabUsers
            // 
            this.tabUsers.Controls.Add(this._lvUsers);
            this.tabUsers.Controls.Add(this.pnlBtns);
            this.tabUsers.Controls.Add(this.tabUserStatus);
            this.tabUsers.Controls.Add(this.pnlSearch);
            this.tabUsers.Location = new System.Drawing.Point(4, 26);
            this.tabUsers.Name = "tabUsers";
            this.tabUsers.Padding = new System.Windows.Forms.Padding(8);
            this.tabUsers.Size = new System.Drawing.Size(952, 606);
            this.tabUsers.TabIndex = 0;
            this.tabUsers.Text = "  สิทธิ์ผู้ใช้  ";
            this.tabUsers.UseVisualStyleBackColor = true;
            // 
            // _lvUsers
            // 
            this._lvUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDisplayName,
            this.colADUser,
            this.colDept,
            this.colDiv,
            this.colRole,
            this.colStatus});
            this._lvUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lvUsers.FullRowSelect = true;
            this._lvUsers.GridLines = true;
            this._lvUsers.HideSelection = false;
            this._lvUsers.Location = new System.Drawing.Point(8, 82);
            this._lvUsers.MultiSelect = false;
            this._lvUsers.Name = "_lvUsers";
            this._lvUsers.Size = new System.Drawing.Size(936, 472);
            this._lvUsers.TabIndex = 2;
            this._lvUsers.UseCompatibleStateImageBehavior = false;
            this._lvUsers.View = System.Windows.Forms.View.Details;
            // 
            // colDisplayName
            // 
            this.colDisplayName.Text = "ชื่อ-นามสกุล";
            this.colDisplayName.Width = 175;
            // 
            // colADUser
            // 
            this.colADUser.Text = "ชื่อผู้ใช้ AD";
            this.colADUser.Width = 145;
            // 
            // colDept
            // 
            this.colDept.Text = "แผนก";
            this.colDept.Width = 160;
            // 
            // colDiv
            // 
            this.colDiv.Text = "ฝ่าย";
            this.colDiv.Width = 145;
            // 
            // colRole
            // 
            this.colRole.Text = "บทบาท";
            this.colRole.Width = 110;
            // 
            // colStatus
            // 
            this.colStatus.Text = "สถานะ";
            this.colStatus.Width = 80;
            // 
            // pnlBtns
            // 
            this.pnlBtns.Controls.Add(this._btnChangeRole);
            this.pnlBtns.Controls.Add(this._btnDeactivate);
            this.pnlBtns.Controls.Add(this._btnAssignOrg);
            this.pnlBtns.Controls.Add(this._btnRefresh);
            this.pnlBtns.Controls.Add(this._lblStatus);
            this.pnlBtns.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBtns.Location = new System.Drawing.Point(8, 554);
            this.pnlBtns.Name = "pnlBtns";
            this.pnlBtns.Padding = new System.Windows.Forms.Padding(4, 8, 4, 0);
            this.pnlBtns.Size = new System.Drawing.Size(936, 44);
            this.pnlBtns.TabIndex = 1;
            // 
            // _btnChangeRole
            // 
            this._btnChangeRole.Enabled = false;
            this._btnChangeRole.Location = new System.Drawing.Point(0, 8);
            this._btnChangeRole.Name = "_btnChangeRole";
            this._btnChangeRole.Size = new System.Drawing.Size(130, 28);
            this._btnChangeRole.TabIndex = 0;
            this._btnChangeRole.Text = "เปลี่ยนบทบาท";
            // 
            // _btnDeactivate
            // 
            this._btnDeactivate.Enabled = false;
            this._btnDeactivate.Location = new System.Drawing.Point(138, 8);
            this._btnDeactivate.Name = "_btnDeactivate";
            this._btnDeactivate.Size = new System.Drawing.Size(110, 28);
            this._btnDeactivate.TabIndex = 1;
            this._btnDeactivate.Text = "ปิดใช้งาน";
            // 
            // _btnAssignOrg
            // 
            this._btnAssignOrg.Enabled = false;
            this._btnAssignOrg.Location = new System.Drawing.Point(256, 8);
            this._btnAssignOrg.Name = "_btnAssignOrg";
            this._btnAssignOrg.Size = new System.Drawing.Size(140, 28);
            this._btnAssignOrg.TabIndex = 2;
            this._btnAssignOrg.Text = "🏢 กำหนดหน่วยงาน";
            this._btnAssignOrg.Click += new System.EventHandler(this._btnAssignOrg_Click);
            // 
            // _btnRefresh
            // 
            this._btnRefresh.Location = new System.Drawing.Point(404, 8);
            this._btnRefresh.Name = "_btnRefresh";
            this._btnRefresh.Size = new System.Drawing.Size(80, 28);
            this._btnRefresh.TabIndex = 3;
            this._btnRefresh.Text = "รีเฟรช";
            // 
            // _lblStatus
            // 
            this._lblStatus.ForeColor = System.Drawing.Color.Gray;
            this._lblStatus.Location = new System.Drawing.Point(496, 14);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(430, 23);
            this._lblStatus.TabIndex = 3;
            // 
            // pnlSearch
            // 
            this.pnlSearch.Controls.Add(this.lblFind);
            this.pnlSearch.Controls.Add(this._txtSearch);
            this.pnlSearch.Controls.Add(this.lblRole);
            this.pnlSearch.Controls.Add(this._cmbRoleFilter);
            this.pnlSearch.Controls.Add(this.btnFind);
            this.pnlSearch.Controls.Add(this.lblDept);
            this.pnlSearch.Controls.Add(this._cmbDeptFilter);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(8, 8);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Padding = new System.Windows.Forms.Padding(4, 8, 4, 0);
            this.pnlSearch.Size = new System.Drawing.Size(936, 44);
            this.pnlSearch.TabIndex = 0;
            // 
            // lblFind
            // 
            this.lblFind.Location = new System.Drawing.Point(0, 12);
            this.lblFind.Name = "lblFind";
            this.lblFind.Size = new System.Drawing.Size(52, 23);
            this.lblFind.TabIndex = 0;
            this.lblFind.Text = "ค้นหา:";
            this.lblFind.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _txtSearch
            // 
            this._txtSearch.Location = new System.Drawing.Point(56, 8);
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.Size = new System.Drawing.Size(200, 25);
            this._txtSearch.TabIndex = 1;
            // 
            // lblRole
            // 
            this.lblRole.Location = new System.Drawing.Point(270, 12);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(52, 23);
            this.lblRole.TabIndex = 2;
            this.lblRole.Text = "บทบาท:";
            this.lblRole.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _cmbRoleFilter
            // 
            this._cmbRoleFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbRoleFilter.Items.AddRange(new object[] {
            "ทุกบทบาท",
            "Officer (1)",
            "SrOfficer (2)",
            "Manager (3)",
            "Admin (4)"});
            this._cmbRoleFilter.Location = new System.Drawing.Point(326, 8);
            this._cmbRoleFilter.Name = "_cmbRoleFilter";
            this._cmbRoleFilter.Size = new System.Drawing.Size(145, 25);
            this._cmbRoleFilter.TabIndex = 3;
            // 
            // btnFind
            // 
            this.btnFind.Location = new System.Drawing.Point(482, 6);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(80, 28);
            this.btnFind.TabIndex = 4;
            this.btnFind.Text = "ค้นหา";
            //
            // lblDept
            //
            this.lblDept.Location = new System.Drawing.Point(580, 12);
            this.lblDept.Name = "lblDept";
            this.lblDept.Size = new System.Drawing.Size(48, 23);
            this.lblDept.TabIndex = 5;
            this.lblDept.Text = "แผนก:";
            this.lblDept.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // _cmbDeptFilter
            //
            this._cmbDeptFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbDeptFilter.Location = new System.Drawing.Point(632, 8);
            this._cmbDeptFilter.Name = "_cmbDeptFilter";
            this._cmbDeptFilter.Size = new System.Drawing.Size(180, 25);
            this._cmbDeptFilter.TabIndex = 6;
            //
            // tabUserStatus — header-only strip that switches _lvUsers between active / inactive users
            //
            this.tabUserStatus.Controls.Add(this.tabActiveUsers);
            this.tabUserStatus.Controls.Add(this.tabInactiveUsers);
            this.tabUserStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabUserStatus.Location = new System.Drawing.Point(8, 52);
            this.tabUserStatus.Name = "tabUserStatus";
            this.tabUserStatus.SelectedIndex = 0;
            this.tabUserStatus.Size = new System.Drawing.Size(936, 30);
            this.tabUserStatus.TabIndex = 3;
            //
            // tabActiveUsers
            //
            this.tabActiveUsers.Location = new System.Drawing.Point(4, 26);
            this.tabActiveUsers.Name = "tabActiveUsers";
            this.tabActiveUsers.Size = new System.Drawing.Size(928, 0);
            this.tabActiveUsers.TabIndex = 0;
            this.tabActiveUsers.Text = "  ใช้งาน  ";
            this.tabActiveUsers.UseVisualStyleBackColor = true;
            //
            // tabInactiveUsers
            //
            this.tabInactiveUsers.Location = new System.Drawing.Point(4, 26);
            this.tabInactiveUsers.Name = "tabInactiveUsers";
            this.tabInactiveUsers.Size = new System.Drawing.Size(928, 0);
            this.tabInactiveUsers.TabIndex = 1;
            this.tabInactiveUsers.Text = "  ปิดใช้งาน  ";
            this.tabInactiveUsers.UseVisualStyleBackColor = true;
            //
            // tabOrg
            // 
            this.tabOrg.Controls.Add(this.splitOrg);
            this.tabOrg.Location = new System.Drawing.Point(4, 26);
            this.tabOrg.Name = "tabOrg";
            this.tabOrg.Padding = new System.Windows.Forms.Padding(8);
            this.tabOrg.Size = new System.Drawing.Size(952, 606);
            this.tabOrg.TabIndex = 1;
            this.tabOrg.Text = "  หน่วยงานผู้ใช้  ";
            this.tabOrg.UseVisualStyleBackColor = true;
            // 
            // splitOrg
            // 
            this.splitOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitOrg.Location = new System.Drawing.Point(8, 8);
            this.splitOrg.Name = "splitOrg";
            // 
            // splitOrg.Panel1
            // 
            this.splitOrg.Panel1.Controls.Add(this.pnlOrgLeft);
            // 
            // splitOrg.Panel2
            // 
            this.splitOrg.Panel2.Controls.Add(this.pnlOrgRight);
            this.splitOrg.Size = new System.Drawing.Size(936, 590);
            this.splitOrg.SplitterDistance = 262;
            this.splitOrg.TabIndex = 0;
            // 
            // pnlOrgLeft
            // 
            this.pnlOrgLeft.Controls.Add(this._orgTree);
            this.pnlOrgLeft.Controls.Add(this.lblOrgTree);
            this.pnlOrgLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOrgLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlOrgLeft.Name = "pnlOrgLeft";
            this.pnlOrgLeft.Size = new System.Drawing.Size(262, 590);
            this.pnlOrgLeft.TabIndex = 0;
            // 
            // _orgTree
            // 
            this._orgTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this._orgTree.HideSelection = false;
            this._orgTree.Location = new System.Drawing.Point(0, 28);
            this._orgTree.Name = "_orgTree";
            this._orgTree.Size = new System.Drawing.Size(262, 562);
            this._orgTree.TabIndex = 0;
            // 
            // lblOrgTree
            // 
            this.lblOrgTree.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOrgTree.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblOrgTree.Location = new System.Drawing.Point(0, 0);
            this.lblOrgTree.Name = "lblOrgTree";
            this.lblOrgTree.Size = new System.Drawing.Size(262, 28);
            this.lblOrgTree.TabIndex = 1;
            this.lblOrgTree.Text = "โครงสร้างองค์กร";
            this.lblOrgTree.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlOrgRight
            // 
            this.pnlOrgRight.Controls.Add(this._lvOrgUsers);
            this.pnlOrgRight.Controls.Add(this.pnlOrgBtns);
            this.pnlOrgRight.Controls.Add(this.lblOrgUsers);
            this.pnlOrgRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOrgRight.Location = new System.Drawing.Point(0, 0);
            this.pnlOrgRight.Name = "pnlOrgRight";
            this.pnlOrgRight.Size = new System.Drawing.Size(670, 590);
            this.pnlOrgRight.TabIndex = 0;
            // 
            // _lvOrgUsers
            // 
            this._lvOrgUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colOrgName,
            this.colOrgAD,
            this.colOrgRole,
            this.colOrgStatus});
            this._lvOrgUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lvOrgUsers.FullRowSelect = true;
            this._lvOrgUsers.GridLines = true;
            this._lvOrgUsers.HideSelection = false;
            this._lvOrgUsers.Location = new System.Drawing.Point(0, 28);
            this._lvOrgUsers.MultiSelect = false;
            this._lvOrgUsers.Name = "_lvOrgUsers";
            this._lvOrgUsers.Size = new System.Drawing.Size(670, 518);
            this._lvOrgUsers.TabIndex = 0;
            this._lvOrgUsers.UseCompatibleStateImageBehavior = false;
            this._lvOrgUsers.View = System.Windows.Forms.View.Details;
            // 
            // colOrgName
            // 
            this.colOrgName.Text = "ชื่อ-นามสกุล";
            this.colOrgName.Width = 190;
            // 
            // colOrgAD
            // 
            this.colOrgAD.Text = "ชื่อผู้ใช้ AD";
            this.colOrgAD.Width = 150;
            // 
            // colOrgRole
            // 
            this.colOrgRole.Text = "บทบาท";
            this.colOrgRole.Width = 110;
            // 
            // colOrgStatus
            // 
            this.colOrgStatus.Text = "สถานะ";
            this.colOrgStatus.Width = 80;
            // 
            // pnlOrgBtns
            // 
            this.pnlOrgBtns.Controls.Add(this._btnChangeOrg);
            this.pnlOrgBtns.Controls.Add(this._btnAddUserToOrg);
            this.pnlOrgBtns.Controls.Add(this._lblOrgStatus);
            this.pnlOrgBtns.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlOrgBtns.Location = new System.Drawing.Point(0, 546);
            this.pnlOrgBtns.Name = "pnlOrgBtns";
            this.pnlOrgBtns.Padding = new System.Windows.Forms.Padding(4, 8, 4, 0);
            this.pnlOrgBtns.Size = new System.Drawing.Size(670, 44);
            this.pnlOrgBtns.TabIndex = 1;
            // 
            // _btnChangeOrg
            //
            this._btnChangeOrg.Enabled = false;
            this._btnChangeOrg.Location = new System.Drawing.Point(0, 8);
            this._btnChangeOrg.Name = "_btnChangeOrg";
            this._btnChangeOrg.Size = new System.Drawing.Size(145, 28);
            this._btnChangeOrg.TabIndex = 0;
            this._btnChangeOrg.Text = "เปลี่ยนหน่วยงาน";
            //
            // _btnAddUserToOrg
            //
            this._btnAddUserToOrg.Enabled = false;
            this._btnAddUserToOrg.Location = new System.Drawing.Point(153, 8);
            this._btnAddUserToOrg.Name = "_btnAddUserToOrg";
            this._btnAddUserToOrg.Size = new System.Drawing.Size(168, 28);
            this._btnAddUserToOrg.TabIndex = 1;
            this._btnAddUserToOrg.Text = "➕ เพิ่มผู้ใช้ในหน่วยงานนี้";
            //
            // _lblOrgStatus
            //
            this._lblOrgStatus.ForeColor = System.Drawing.Color.Gray;
            this._lblOrgStatus.Location = new System.Drawing.Point(329, 14);
            this._lblOrgStatus.Name = "_lblOrgStatus";
            this._lblOrgStatus.Size = new System.Drawing.Size(330, 23);
            this._lblOrgStatus.TabIndex = 2;
            // 
            // lblOrgUsers
            // 
            this.lblOrgUsers.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOrgUsers.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblOrgUsers.Location = new System.Drawing.Point(0, 0);
            this.lblOrgUsers.Name = "lblOrgUsers";
            this.lblOrgUsers.Size = new System.Drawing.Size(670, 28);
            this.lblOrgUsers.TabIndex = 2;
            this.lblOrgUsers.Text = "ผู้ใช้ในหน่วยงาน";
            this.lblOrgUsers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UserAccessForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(960, 680);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(800, 560);
            this.Name = "UserAccessForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "จัดการสิทธิ์การเข้าถึง";
            this.pnlHeader.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabUsers.ResumeLayout(false);
            this.pnlBtns.ResumeLayout(false);
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.tabUserStatus.ResumeLayout(false);
            this.tabOrg.ResumeLayout(false);
            this.splitOrg.Panel1.ResumeLayout(false);
            this.splitOrg.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitOrg)).EndInit();
            this.splitOrg.ResumeLayout(false);
            this.pnlOrgLeft.ResumeLayout(false);
            this.pnlOrgRight.ResumeLayout(false);
            this.pnlOrgBtns.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        // ── Control declarations ─────────────────────────────────────────────
        private System.Windows.Forms.Panel          pnlHeader;
        private System.Windows.Forms.Label          lblHeader;
        private System.Windows.Forms.TabControl     tabControl;
        private System.Windows.Forms.TabPage        tabUsers;
        private System.Windows.Forms.TabPage        tabOrg;
        // Tab 1
        private System.Windows.Forms.Panel          pnlSearch;
        private System.Windows.Forms.Label          lblFind;
        private System.Windows.Forms.TextBox        _txtSearch;
        private System.Windows.Forms.Label          lblRole;
        private System.Windows.Forms.ComboBox       _cmbRoleFilter;
        private System.Windows.Forms.Button         btnFind;
        private System.Windows.Forms.Label          lblDept;
        private System.Windows.Forms.ComboBox       _cmbDeptFilter;
        private System.Windows.Forms.TabControl     tabUserStatus;
        private System.Windows.Forms.TabPage        tabActiveUsers;
        private System.Windows.Forms.TabPage        tabInactiveUsers;
        private System.Windows.Forms.ListView       _lvUsers;
        private System.Windows.Forms.ColumnHeader   colDisplayName;
        private System.Windows.Forms.ColumnHeader   colADUser;
        private System.Windows.Forms.ColumnHeader   colDept;
        private System.Windows.Forms.ColumnHeader   colDiv;
        private System.Windows.Forms.ColumnHeader   colRole;
        private System.Windows.Forms.ColumnHeader   colStatus;
        private System.Windows.Forms.Panel          pnlBtns;
        private System.Windows.Forms.Button         _btnChangeRole;
        private System.Windows.Forms.Button         _btnDeactivate;
        private System.Windows.Forms.Button         _btnAssignOrg;
        private System.Windows.Forms.Button         _btnRefresh;
        private System.Windows.Forms.Label          _lblStatus;
        // Tab 2
        private System.Windows.Forms.SplitContainer splitOrg;
        private System.Windows.Forms.Panel          pnlOrgLeft;
        private System.Windows.Forms.Label          lblOrgTree;
        private System.Windows.Forms.TreeView       _orgTree;
        private System.Windows.Forms.Panel          pnlOrgRight;
        private System.Windows.Forms.Label          lblOrgUsers;
        private System.Windows.Forms.ListView       _lvOrgUsers;
        private System.Windows.Forms.ColumnHeader   colOrgName;
        private System.Windows.Forms.ColumnHeader   colOrgAD;
        private System.Windows.Forms.ColumnHeader   colOrgRole;
        private System.Windows.Forms.ColumnHeader   colOrgStatus;
        private System.Windows.Forms.Panel          pnlOrgBtns;
        private System.Windows.Forms.Button         _btnChangeOrg;
        private System.Windows.Forms.Button         _btnAddUserToOrg;
        private System.Windows.Forms.Label          _lblOrgStatus;
    }
}
