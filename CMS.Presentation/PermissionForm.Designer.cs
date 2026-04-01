namespace CMS.Presentation
{
    partial class PermissionForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.tblGeneral = new System.Windows.Forms.TableLayoutPanel();
            this.lblLevelLabel = new System.Windows.Forms.Label();
            this.cboConfLevel = new System.Windows.Forms.ComboBox();
            this.lblColorLabel = new System.Windows.Forms.Label();
            this.pnlColorRow = new System.Windows.Forms.Panel();
            this.pnlColorSwatch = new System.Windows.Forms.Panel();
            this.lblColorText = new System.Windows.Forms.Label();
            this.lblNameLabel = new System.Windows.Forms.Label();
            this.txtFolderName = new System.Windows.Forms.TextBox();
            this.lblPathLabel = new System.Windows.Forms.Label();
            this.txtPhysicalPath = new System.Windows.Forms.TextBox();
            this.lblSizeLabel = new System.Windows.Forms.Label();
            this.lblSizeValue = new System.Windows.Forms.Label();
            this.sepGeneral = new System.Windows.Forms.Panel();
            this.lblInheritLabel = new System.Windows.Forms.Label();
            this.chkInheritPerm = new System.Windows.Forms.CheckBox();
            this.tabPermission = new System.Windows.Forms.TabPage();
            this.aclListView = new System.Windows.Forms.ListView();
            this.colPrincipal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPrincipalType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRead = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWrite = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDelete = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colModify = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFull = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colExpiry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlPermToolbar = new System.Windows.Forms.Panel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnForceLoad = new System.Windows.Forms.Button();
            this.lblAclStatus = new System.Windows.Forms.Label();
            this.btnDiagnostic = new System.Windows.Forms.Button();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tblGeneral.SuspendLayout();
            this.pnlColorRow.SuspendLayout();
            this.tabPermission.SuspendLayout();
            this.pnlPermToolbar.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.btnClose);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(10, 0, 8, 0);
            this.pnlHeader.Size = new System.Drawing.Size(834, 40);
            this.pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(10, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(780, 40);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "⚙ ตั้งค่า Folder";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(790, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(36, 40);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabGeneral);
            this.tabControl.Controls.Add(this.tabPermission);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabControl.Location = new System.Drawing.Point(0, 40);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(834, 432);
            this.tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.tblGeneral);
            this.tabGeneral.Location = new System.Drawing.Point(4, 26);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(8);
            this.tabGeneral.Size = new System.Drawing.Size(826, 402);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "ทั่วไป";
            // 
            // tblGeneral
            // 
            this.tblGeneral.ColumnCount = 2;
            this.tblGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tblGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblGeneral.Controls.Add(this.lblLevelLabel, 0, 0);
            this.tblGeneral.Controls.Add(this.cboConfLevel, 1, 0);
            this.tblGeneral.Controls.Add(this.lblColorLabel, 0, 1);
            this.tblGeneral.Controls.Add(this.pnlColorRow, 1, 1);
            this.tblGeneral.Controls.Add(this.lblNameLabel, 0, 2);
            this.tblGeneral.Controls.Add(this.txtFolderName, 1, 2);
            this.tblGeneral.Controls.Add(this.lblPathLabel, 0, 3);
            this.tblGeneral.Controls.Add(this.txtPhysicalPath, 1, 3);
            this.tblGeneral.Controls.Add(this.lblSizeLabel, 0, 4);
            this.tblGeneral.Controls.Add(this.lblSizeValue, 1, 4);
            this.tblGeneral.Controls.Add(this.sepGeneral, 0, 5);
            this.tblGeneral.Controls.Add(this.lblInheritLabel, 0, 6);
            this.tblGeneral.Controls.Add(this.chkInheritPerm, 1, 6);
            this.tblGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblGeneral.Location = new System.Drawing.Point(8, 8);
            this.tblGeneral.Name = "tblGeneral";
            this.tblGeneral.Padding = new System.Windows.Forms.Padding(8, 12, 8, 8);
            this.tblGeneral.RowCount = 8;
            this.tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblGeneral.Size = new System.Drawing.Size(810, 386);
            this.tblGeneral.TabIndex = 0;
            // 
            // lblLevelLabel
            // 
            this.lblLevelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLevelLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblLevelLabel.Location = new System.Drawing.Point(11, 12);
            this.lblLevelLabel.Name = "lblLevelLabel";
            this.lblLevelLabel.Size = new System.Drawing.Size(124, 34);
            this.lblLevelLabel.TabIndex = 0;
            this.lblLevelLabel.Text = "ระดับ:";
            this.lblLevelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboConfLevel
            // 
            this.cboConfLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboConfLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboConfLevel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboConfLevel.Items.AddRange(new object[] {
            "1HC - Highly Confidential",
            "2SC - Secret",
            "3NC - Normal Confidential",
            "4PC - Public/Common"});
            this.cboConfLevel.Location = new System.Drawing.Point(141, 15);
            this.cboConfLevel.Name = "cboConfLevel";
            this.cboConfLevel.Size = new System.Drawing.Size(658, 23);
            this.cboConfLevel.TabIndex = 1;
            // 
            // lblColorLabel
            // 
            this.lblColorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblColorLabel.Location = new System.Drawing.Point(11, 46);
            this.lblColorLabel.Name = "lblColorLabel";
            this.lblColorLabel.Size = new System.Drawing.Size(124, 34);
            this.lblColorLabel.TabIndex = 2;
            this.lblColorLabel.Text = "สีแสดงผล:";
            this.lblColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlColorRow
            // 
            this.pnlColorRow.Controls.Add(this.pnlColorSwatch);
            this.pnlColorRow.Controls.Add(this.lblColorText);
            this.pnlColorRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlColorRow.Location = new System.Drawing.Point(141, 49);
            this.pnlColorRow.Name = "pnlColorRow";
            this.pnlColorRow.Size = new System.Drawing.Size(658, 28);
            this.pnlColorRow.TabIndex = 3;
            // 
            // pnlColorSwatch
            // 
            this.pnlColorSwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlColorSwatch.Location = new System.Drawing.Point(0, 5);
            this.pnlColorSwatch.Name = "pnlColorSwatch";
            this.pnlColorSwatch.Size = new System.Drawing.Size(28, 22);
            this.pnlColorSwatch.TabIndex = 0;
            // 
            // lblColorText
            // 
            this.lblColorText.AutoSize = true;
            this.lblColorText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblColorText.Location = new System.Drawing.Point(36, 8);
            this.lblColorText.Name = "lblColorText";
            this.lblColorText.Size = new System.Drawing.Size(0, 15);
            this.lblColorText.TabIndex = 1;
            // 
            // lblNameLabel
            // 
            this.lblNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNameLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNameLabel.Location = new System.Drawing.Point(11, 80);
            this.lblNameLabel.Name = "lblNameLabel";
            this.lblNameLabel.Size = new System.Drawing.Size(124, 34);
            this.lblNameLabel.TabIndex = 4;
            this.lblNameLabel.Text = "ชื่อ Folder:";
            this.lblNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtFolderName
            // 
            this.txtFolderName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFolderName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFolderName.Location = new System.Drawing.Point(141, 83);
            this.txtFolderName.Name = "txtFolderName";
            this.txtFolderName.Size = new System.Drawing.Size(658, 23);
            this.txtFolderName.TabIndex = 5;
            // 
            // lblPathLabel
            // 
            this.lblPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPathLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPathLabel.Location = new System.Drawing.Point(11, 114);
            this.lblPathLabel.Name = "lblPathLabel";
            this.lblPathLabel.Size = new System.Drawing.Size(124, 34);
            this.lblPathLabel.TabIndex = 6;
            this.lblPathLabel.Text = "Physical Path:";
            this.lblPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPhysicalPath
            // 
            this.txtPhysicalPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.txtPhysicalPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPhysicalPath.Font = new System.Drawing.Font("Courier New", 8.5F);
            this.txtPhysicalPath.Location = new System.Drawing.Point(141, 117);
            this.txtPhysicalPath.Name = "txtPhysicalPath";
            this.txtPhysicalPath.ReadOnly = true;
            this.txtPhysicalPath.Size = new System.Drawing.Size(658, 20);
            this.txtPhysicalPath.TabIndex = 7;
            // 
            // lblSizeLabel
            // 
            this.lblSizeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSizeLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSizeLabel.Location = new System.Drawing.Point(11, 148);
            this.lblSizeLabel.Name = "lblSizeLabel";
            this.lblSizeLabel.Size = new System.Drawing.Size(124, 34);
            this.lblSizeLabel.TabIndex = 8;
            this.lblSizeLabel.Text = "ขนาดรวม:";
            this.lblSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSizeValue
            // 
            this.lblSizeValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSizeValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSizeValue.Location = new System.Drawing.Point(141, 148);
            this.lblSizeValue.Name = "lblSizeValue";
            this.lblSizeValue.Size = new System.Drawing.Size(658, 34);
            this.lblSizeValue.TabIndex = 9;
            this.lblSizeValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sepGeneral
            // 
            this.sepGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.tblGeneral.SetColumnSpan(this.sepGeneral, 2);
            this.sepGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sepGeneral.Location = new System.Drawing.Point(11, 185);
            this.sepGeneral.Name = "sepGeneral";
            this.sepGeneral.Size = new System.Drawing.Size(788, 4);
            this.sepGeneral.TabIndex = 10;
            // 
            // lblInheritLabel
            // 
            this.lblInheritLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInheritLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInheritLabel.Location = new System.Drawing.Point(11, 192);
            this.lblInheritLabel.Name = "lblInheritLabel";
            this.lblInheritLabel.Size = new System.Drawing.Size(124, 30);
            this.lblInheritLabel.TabIndex = 11;
            this.lblInheritLabel.Text = "รับสิทธิ์จาก Parent:";
            this.lblInheritLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkInheritPerm
            // 
            this.chkInheritPerm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkInheritPerm.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkInheritPerm.Location = new System.Drawing.Point(141, 195);
            this.chkInheritPerm.Name = "chkInheritPerm";
            this.chkInheritPerm.Size = new System.Drawing.Size(658, 24);
            this.chkInheritPerm.TabIndex = 12;
            this.chkInheritPerm.Text = "ใช้ Inherit Permission";
            // 
            // tabPermission
            // 
            this.tabPermission.Controls.Add(this.aclListView);
            this.tabPermission.Controls.Add(this.pnlPermToolbar);
            this.tabPermission.Location = new System.Drawing.Point(4, 26);
            this.tabPermission.Name = "tabPermission";
            this.tabPermission.Size = new System.Drawing.Size(826, 402);
            this.tabPermission.TabIndex = 1;
            this.tabPermission.Text = "สิทธิ์";
            // 
            // aclListView
            // 
            this.aclListView.BackColor = System.Drawing.Color.White;
            this.aclListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPrincipal,
            this.colPrincipalType,
            this.colRead,
            this.colWrite,
            this.colDelete,
            this.colModify,
            this.colFull,
            this.colExpiry,
            this.colStatus});
            this.aclListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aclListView.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.aclListView.FullRowSelect = true;
            this.aclListView.GridLines = true;
            this.aclListView.HideSelection = false;
            this.aclListView.Location = new System.Drawing.Point(0, 36);
            this.aclListView.Name = "aclListView";
            this.aclListView.Size = new System.Drawing.Size(826, 366);
            this.aclListView.TabIndex = 1;
            this.aclListView.UseCompatibleStateImageBehavior = false;
            this.aclListView.View = System.Windows.Forms.View.Details;
            // 
            // colPrincipal
            // 
            this.colPrincipal.Text = "ผู้ใช้/กลุ่ม";
            this.colPrincipal.Width = 160;
            // 
            // colPrincipalType
            // 
            this.colPrincipalType.Text = "ประเภท";
            this.colPrincipalType.Width = 80;
            // 
            // colRead
            // 
            this.colRead.Text = "อ่าน";
            this.colRead.Width = 55;
            // 
            // colWrite
            // 
            this.colWrite.Text = "เขียน";
            this.colWrite.Width = 55;
            // 
            // colDelete
            // 
            this.colDelete.Text = "ลบ";
            this.colDelete.Width = 45;
            // 
            // colModify
            // 
            this.colModify.Text = "แก้ไข";
            this.colModify.Width = 55;
            // 
            // colFull
            // 
            this.colFull.Text = "Full";
            this.colFull.Width = 45;
            // 
            // colExpiry
            // 
            this.colExpiry.Text = "หมดอายุ";
            this.colExpiry.Width = 80;
            // 
            // colStatus
            // 
            this.colStatus.Text = "สถานะ";
            this.colStatus.Width = 80;
            // 
            // pnlPermToolbar
            // 
            this.pnlPermToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.pnlPermToolbar.Controls.Add(this.btnAdd);
            this.pnlPermToolbar.Controls.Add(this.btnEdit);
            this.pnlPermToolbar.Controls.Add(this.btnRemove);
            this.pnlPermToolbar.Controls.Add(this.btnRefresh);
            this.pnlPermToolbar.Controls.Add(this.btnForceLoad);
            this.pnlPermToolbar.Controls.Add(this.lblAclStatus);
            this.pnlPermToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPermToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlPermToolbar.Name = "pnlPermToolbar";
            this.pnlPermToolbar.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.pnlPermToolbar.Size = new System.Drawing.Size(826, 36);
            this.pnlPermToolbar.TabIndex = 0;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(6, 4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(72, 26);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "+ เพิ่ม";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnEdit.Location = new System.Drawing.Point(84, 4);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(72, 26);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "แก้ไข";
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnRemove.Location = new System.Drawing.Point(162, 4);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(72, 26);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "ลบ";
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnRefresh.Location = new System.Drawing.Point(240, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(72, 26);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "🔄 รีเฟรช";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnForceLoad
            // 
            this.btnForceLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(0)))));
            this.btnForceLoad.FlatAppearance.BorderSize = 0;
            this.btnForceLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForceLoad.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnForceLoad.ForeColor = System.Drawing.Color.White;
            this.btnForceLoad.Location = new System.Drawing.Point(318, 4);
            this.btnForceLoad.Name = "btnForceLoad";
            this.btnForceLoad.Size = new System.Drawing.Size(120, 26);
            this.btnForceLoad.TabIndex = 5;
            this.btnForceLoad.Text = "💾 บังคับดึงข้อมูล";
            this.btnForceLoad.UseVisualStyleBackColor = false;
            this.btnForceLoad.Click += new System.EventHandler(this.btnForceLoad_Click);
            // 
            // lblAclStatus
            // 
            this.lblAclStatus.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblAclStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblAclStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblAclStatus.Location = new System.Drawing.Point(620, 4);
            this.lblAclStatus.Name = "lblAclStatus";
            this.lblAclStatus.Size = new System.Drawing.Size(200, 28);
            this.lblAclStatus.TabIndex = 3;
            this.lblAclStatus.Text = "0 รายการ";
            this.lblAclStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnDiagnostic
            // 
            this.btnDiagnostic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDiagnostic.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnDiagnostic.Location = new System.Drawing.Point(318, 4);
            this.btnDiagnostic.Name = "btnDiagnostic";
            this.btnDiagnostic.Size = new System.Drawing.Size(80, 26);
            this.btnDiagnostic.TabIndex = 4;
            this.btnDiagnostic.Text = "🔍 วินิจฉัย";
            this.btnDiagnostic.Click += new System.EventHandler(this.btnDiagnostic_Click);
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.pnlFooter.Controls.Add(this.btnSave);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 472);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(8);
            this.pnlFooter.Size = new System.Drawing.Size(834, 48);
            this.pnlFooter.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(668, 9);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(150, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "บันทึก (ส่งอนุมัติ)";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.Location = new System.Drawing.Point(744, 9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "ยกเลิก";
            // 
            // PermissionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 520);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PermissionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ตั้งค่า Folder";
            this.Load += new System.EventHandler(this.PermissionForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tblGeneral.ResumeLayout(false);
            this.tblGeneral.PerformLayout();
            this.pnlColorRow.ResumeLayout(false);
            this.pnlColorRow.PerformLayout();
            this.tabPermission.ResumeLayout(false);
            this.pnlPermToolbar.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        // ── Field declarations ────────────────────────────────────────────────
        private System.Windows.Forms.Panel              pnlHeader;
        private System.Windows.Forms.Label              lblTitle;
        private System.Windows.Forms.Button             btnClose;
        private System.Windows.Forms.TabControl         tabControl;
        private System.Windows.Forms.TabPage            tabGeneral;
        private System.Windows.Forms.TabPage            tabPermission;
        private System.Windows.Forms.TableLayoutPanel   tblGeneral;
        private System.Windows.Forms.Label              lblLevelLabel;
        private System.Windows.Forms.ComboBox           cboConfLevel;
        private System.Windows.Forms.Label              lblColorLabel;
        private System.Windows.Forms.Panel              pnlColorRow;
        private System.Windows.Forms.Panel              pnlColorSwatch;
        private System.Windows.Forms.Label              lblColorText;
        private System.Windows.Forms.Label              lblNameLabel;
        private System.Windows.Forms.TextBox            txtFolderName;
        private System.Windows.Forms.Label              lblPathLabel;
        private System.Windows.Forms.TextBox            txtPhysicalPath;
        private System.Windows.Forms.Label              lblSizeLabel;
        private System.Windows.Forms.Label              lblSizeValue;
        private System.Windows.Forms.Panel              sepGeneral;
        private System.Windows.Forms.Label              lblInheritLabel;
        private System.Windows.Forms.CheckBox           chkInheritPerm;
        private System.Windows.Forms.Panel              pnlPermToolbar;
        private System.Windows.Forms.Button             btnAdd;
        private System.Windows.Forms.Button             btnEdit;
        private System.Windows.Forms.Button             btnRemove;
        private System.Windows.Forms.Button             btnRefresh;
        private System.Windows.Forms.Button             btnDiagnostic;
        private System.Windows.Forms.Button             btnForceLoad;
        private System.Windows.Forms.Label              lblAclStatus;
        private System.Windows.Forms.ListView           aclListView;
        private System.Windows.Forms.ColumnHeader       colPrincipal;
        private System.Windows.Forms.ColumnHeader       colPrincipalType;
        private System.Windows.Forms.ColumnHeader       colRead;
        private System.Windows.Forms.ColumnHeader       colWrite;
        private System.Windows.Forms.ColumnHeader       colDelete;
        private System.Windows.Forms.ColumnHeader       colModify;
        private System.Windows.Forms.ColumnHeader       colFull;
        private System.Windows.Forms.ColumnHeader       colExpiry;
        private System.Windows.Forms.ColumnHeader       colStatus;
        private System.Windows.Forms.Panel              pnlFooter;
        private System.Windows.Forms.Button             btnSave;
        private System.Windows.Forms.Button             btnCancel;
    }
}
