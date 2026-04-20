namespace CMS.Presentation
{
    partial class AuthorityConfigForm
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
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.split = new System.Windows.Forms.SplitContainer();
            this.tvOrg = new System.Windows.Forms.TreeView();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.lblFilter = new System.Windows.Forms.Label();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.grpEdit = new System.Windows.Forms.GroupBox();
            this.lblEditLabel = new System.Windows.Forms.Label();
            this.txtLabel = new System.Windows.Forms.TextBox();
            this.lblEditPosition = new System.Windows.Forms.Label();
            this.txtPosition = new System.Windows.Forms.TextBox();
            this.lblEditDept = new System.Windows.Forms.Label();
            this.txtDept = new System.Windows.Forms.TextBox();
            this.lblEditDiv = new System.Windows.Forms.Label();
            this.txtDiv = new System.Windows.Forms.TextBox();
            this.lblEditLevelCode = new System.Windows.Forms.Label();
            this.txtLevelCode = new System.Windows.Forms.TextBox();
            this.lblHC = new System.Windows.Forms.Label();
            this.chkHC_V = new System.Windows.Forms.CheckBox();
            this.chkHC_A = new System.Windows.Forms.CheckBox();
            this.chkHC_E = new System.Windows.Forms.CheckBox();
            this.chkHC_Ap = new System.Windows.Forms.CheckBox();
            this.lblSC = new System.Windows.Forms.Label();
            this.chkSC_V = new System.Windows.Forms.CheckBox();
            this.chkSC_A = new System.Windows.Forms.CheckBox();
            this.chkSC_E = new System.Windows.Forms.CheckBox();
            this.chkSC_Ap = new System.Windows.Forms.CheckBox();
            this.lblNC = new System.Windows.Forms.Label();
            this.chkNC_V = new System.Windows.Forms.CheckBox();
            this.chkNC_A = new System.Windows.Forms.CheckBox();
            this.chkNC_E = new System.Windows.Forms.CheckBox();
            this.chkNC_Ap = new System.Windows.Forms.CheckBox();
            this.lblPC = new System.Windows.Forms.Label();
            this.chkPC_V = new System.Windows.Forms.CheckBox();
            this.chkPC_A = new System.Windows.Forms.CheckBox();
            this.chkPC_E = new System.Windows.Forms.CheckBox();
            this.chkPC_Ap = new System.Windows.Forms.CheckBox();
            this.btnSaveRow = new System.Windows.Forms.Button();
            this.btnCancelRow = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split)).BeginInit();
            this.split.Panel1.SuspendLayout();
            this.split.Panel2.SuspendLayout();
            this.split.SuspendLayout();
            this.pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.pnlEdit.SuspendLayout();
            this.grpEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1084, 46);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1084, 46);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "  จัดการสิทธิ์ตาม ตำแหน่ง / แผนก / ฝ่าย  (Authority Config)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.pnlToolbar.Controls.Add(this.btnAdd);
            this.pnlToolbar.Controls.Add(this.btnEdit);
            this.pnlToolbar.Controls.Add(this.btnDelete);
            this.pnlToolbar.Controls.Add(this.btnClose);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 46);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1084, 44);
            this.pnlToolbar.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(6, 6);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(118, 32);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "+ เพิ่มใหม่";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(60)))));
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnEdit.ForeColor = System.Drawing.Color.White;
            this.btnEdit.Location = new System.Drawing.Point(130, 6);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(118, 32);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "✏ แก้ไข";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(254, 6);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(118, 32);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "🗑 ลบ";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(378, 6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(118, 32);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "✕ ปิด";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // split
            // 
            this.split.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split.Location = new System.Drawing.Point(0, 90);
            this.split.Name = "split";
            // 
            // split.Panel1
            // 
            this.split.Panel1.Controls.Add(this.tvOrg);
            // 
            // split.Panel2
            // 
            this.split.Panel2.Controls.Add(this.pnlRight);
            this.split.Size = new System.Drawing.Size(1084, 261);
            this.split.SplitterDistance = 203;
            this.split.TabIndex = 2;
            // 
            // tvOrg
            // 
            this.tvOrg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(253)))));
            this.tvOrg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tvOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvOrg.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tvOrg.Location = new System.Drawing.Point(0, 0);
            this.tvOrg.Name = "tvOrg";
            this.tvOrg.Size = new System.Drawing.Size(203, 261);
            this.tvOrg.TabIndex = 0;
            // 
            // pnlRight
            // 
            this.pnlRight.Controls.Add(this.dgv);
            this.pnlRight.Controls.Add(this.lblFilter);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(0, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(877, 261);
            this.pnlRight.TabIndex = 0;
            // 
            // dgv
            // 
            this.dgv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 24);
            this.dgv.Name = "dgv";
            this.dgv.RowTemplate.Height = 26;
            this.dgv.Size = new System.Drawing.Size(877, 237);
            this.dgv.TabIndex = 1;
            // 
            // lblFilter
            // 
            this.lblFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFilter.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblFilter.Location = new System.Drawing.Point(0, 0);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.lblFilter.Size = new System.Drawing.Size(877, 24);
            this.lblFilter.TabIndex = 0;
            this.lblFilter.Text = "แสดง: ทั้งหมด";
            // 
            // pnlEdit
            // 
            this.pnlEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.pnlEdit.Controls.Add(this.grpEdit);
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlEdit.Location = new System.Drawing.Point(0, 351);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(1084, 260);
            this.pnlEdit.TabIndex = 3;
            this.pnlEdit.Visible = false;
            // 
            // grpEdit
            // 
            this.grpEdit.Controls.Add(this.lblEditLabel);
            this.grpEdit.Controls.Add(this.txtLabel);
            this.grpEdit.Controls.Add(this.lblEditPosition);
            this.grpEdit.Controls.Add(this.txtPosition);
            this.grpEdit.Controls.Add(this.lblEditDept);
            this.grpEdit.Controls.Add(this.txtDept);
            this.grpEdit.Controls.Add(this.lblEditDiv);
            this.grpEdit.Controls.Add(this.txtDiv);
            this.grpEdit.Controls.Add(this.lblEditLevelCode);
            this.grpEdit.Controls.Add(this.txtLevelCode);
            this.grpEdit.Controls.Add(this.lblHC);
            this.grpEdit.Controls.Add(this.chkHC_V);
            this.grpEdit.Controls.Add(this.chkHC_A);
            this.grpEdit.Controls.Add(this.chkHC_E);
            this.grpEdit.Controls.Add(this.chkHC_Ap);
            this.grpEdit.Controls.Add(this.lblSC);
            this.grpEdit.Controls.Add(this.chkSC_V);
            this.grpEdit.Controls.Add(this.chkSC_A);
            this.grpEdit.Controls.Add(this.chkSC_E);
            this.grpEdit.Controls.Add(this.chkSC_Ap);
            this.grpEdit.Controls.Add(this.lblNC);
            this.grpEdit.Controls.Add(this.chkNC_V);
            this.grpEdit.Controls.Add(this.chkNC_A);
            this.grpEdit.Controls.Add(this.chkNC_E);
            this.grpEdit.Controls.Add(this.chkNC_Ap);
            this.grpEdit.Controls.Add(this.lblPC);
            this.grpEdit.Controls.Add(this.chkPC_V);
            this.grpEdit.Controls.Add(this.chkPC_A);
            this.grpEdit.Controls.Add(this.chkPC_E);
            this.grpEdit.Controls.Add(this.chkPC_Ap);
            this.grpEdit.Controls.Add(this.btnSaveRow);
            this.grpEdit.Controls.Add(this.btnCancelRow);
            this.grpEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpEdit.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.grpEdit.Location = new System.Drawing.Point(0, 0);
            this.grpEdit.Name = "grpEdit";
            this.grpEdit.Size = new System.Drawing.Size(1084, 260);
            this.grpEdit.TabIndex = 0;
            this.grpEdit.TabStop = false;
            this.grpEdit.Text = "รายละเอียด";
            // 
            // lblEditLabel
            // 
            this.lblEditLabel.AutoSize = true;
            this.lblEditLabel.Location = new System.Drawing.Point(10, 25);
            this.lblEditLabel.Name = "lblEditLabel";
            this.lblEditLabel.Size = new System.Drawing.Size(38, 15);
            this.lblEditLabel.TabIndex = 0;
            this.lblEditLabel.Text = "Label:";
            // 
            // txtLabel
            // 
            this.txtLabel.Location = new System.Drawing.Point(70, 22);
            this.txtLabel.Name = "txtLabel";
            this.txtLabel.Size = new System.Drawing.Size(140, 23);
            this.txtLabel.TabIndex = 0;
            // 
            // lblEditPosition
            // 
            this.lblEditPosition.AutoSize = true;
            this.lblEditPosition.Location = new System.Drawing.Point(220, 25);
            this.lblEditPosition.Name = "lblEditPosition";
            this.lblEditPosition.Size = new System.Drawing.Size(47, 15);
            this.lblEditPosition.TabIndex = 1;
            this.lblEditPosition.Text = "ตำแหน่ง:";
            // 
            // txtPosition
            // 
            this.txtPosition.Location = new System.Drawing.Point(290, 22);
            this.txtPosition.Name = "txtPosition";
            this.txtPosition.Size = new System.Drawing.Size(140, 23);
            this.txtPosition.TabIndex = 1;
            // 
            // lblEditDept
            // 
            this.lblEditDept.AutoSize = true;
            this.lblEditDept.Location = new System.Drawing.Point(10, 57);
            this.lblEditDept.Name = "lblEditDept";
            this.lblEditDept.Size = new System.Drawing.Size(37, 15);
            this.lblEditDept.TabIndex = 2;
            this.lblEditDept.Text = "แผนก:";
            // 
            // txtDept
            // 
            this.txtDept.Location = new System.Drawing.Point(70, 54);
            this.txtDept.Name = "txtDept";
            this.txtDept.Size = new System.Drawing.Size(140, 23);
            this.txtDept.TabIndex = 2;
            // 
            // lblEditDiv
            // 
            this.lblEditDiv.AutoSize = true;
            this.lblEditDiv.Location = new System.Drawing.Point(220, 57);
            this.lblEditDiv.Name = "lblEditDiv";
            this.lblEditDiv.Size = new System.Drawing.Size(28, 15);
            this.lblEditDiv.TabIndex = 3;
            this.lblEditDiv.Text = "ฝ่าย:";
            // 
            // txtDiv
            // 
            this.txtDiv.Location = new System.Drawing.Point(290, 54);
            this.txtDiv.Name = "txtDiv";
            this.txtDiv.Size = new System.Drawing.Size(140, 23);
            this.txtDiv.TabIndex = 3;
            // 
            // lblEditLevelCode
            // 
            this.lblEditLevelCode.AutoSize = true;
            this.lblEditLevelCode.Location = new System.Drawing.Point(440, 25);
            this.lblEditLevelCode.Name = "lblEditLevelCode";
            this.lblEditLevelCode.Size = new System.Drawing.Size(65, 15);
            this.lblEditLevelCode.TabIndex = 22;
            this.lblEditLevelCode.Text = "LevelCode:";
            // 
            // txtLevelCode
            // 
            this.txtLevelCode.Location = new System.Drawing.Point(510, 22);
            this.txtLevelCode.Name = "txtLevelCode";
            this.txtLevelCode.Size = new System.Drawing.Size(100, 23);
            this.txtLevelCode.TabIndex = 23;
            // 
            // lblHC
            // 
            this.lblHC.AutoSize = true;
            this.lblHC.Location = new System.Drawing.Point(10, 89);
            this.lblHC.Name = "lblHC";
            this.lblHC.Size = new System.Drawing.Size(27, 15);
            this.lblHC.TabIndex = 4;
            this.lblHC.Text = "HC:";
            // 
            // chkHC_V
            // 
            this.chkHC_V.AutoSize = true;
            this.chkHC_V.Location = new System.Drawing.Point(70, 87);
            this.chkHC_V.Name = "chkHC_V";
            this.chkHC_V.Size = new System.Drawing.Size(51, 19);
            this.chkHC_V.TabIndex = 4;
            this.chkHC_V.Text = "View";
            // 
            // chkHC_A
            // 
            this.chkHC_A.AutoSize = true;
            this.chkHC_A.Location = new System.Drawing.Point(124, 87);
            this.chkHC_A.Name = "chkHC_A";
            this.chkHC_A.Size = new System.Drawing.Size(48, 19);
            this.chkHC_A.TabIndex = 5;
            this.chkHC_A.Text = "Add";
            // 
            // chkHC_E
            // 
            this.chkHC_E.AutoSize = true;
            this.chkHC_E.Location = new System.Drawing.Point(178, 87);
            this.chkHC_E.Name = "chkHC_E";
            this.chkHC_E.Size = new System.Drawing.Size(46, 19);
            this.chkHC_E.TabIndex = 6;
            this.chkHC_E.Text = "Edit";
            // 
            // chkHC_Ap
            // 
            this.chkHC_Ap.AutoSize = true;
            this.chkHC_Ap.Location = new System.Drawing.Point(232, 87);
            this.chkHC_Ap.Name = "chkHC_Ap";
            this.chkHC_Ap.Size = new System.Drawing.Size(52, 19);
            this.chkHC_Ap.TabIndex = 7;
            this.chkHC_Ap.Text = "Appr";
            // 
            // lblSC
            // 
            this.lblSC.AutoSize = true;
            this.lblSC.Location = new System.Drawing.Point(10, 121);
            this.lblSC.Name = "lblSC";
            this.lblSC.Size = new System.Drawing.Size(24, 15);
            this.lblSC.TabIndex = 8;
            this.lblSC.Text = "SC:";
            // 
            // chkSC_V
            // 
            this.chkSC_V.AutoSize = true;
            this.chkSC_V.Location = new System.Drawing.Point(70, 119);
            this.chkSC_V.Name = "chkSC_V";
            this.chkSC_V.Size = new System.Drawing.Size(51, 19);
            this.chkSC_V.TabIndex = 8;
            this.chkSC_V.Text = "View";
            // 
            // chkSC_A
            // 
            this.chkSC_A.AutoSize = true;
            this.chkSC_A.Location = new System.Drawing.Point(124, 119);
            this.chkSC_A.Name = "chkSC_A";
            this.chkSC_A.Size = new System.Drawing.Size(48, 19);
            this.chkSC_A.TabIndex = 9;
            this.chkSC_A.Text = "Add";
            // 
            // chkSC_E
            // 
            this.chkSC_E.AutoSize = true;
            this.chkSC_E.Location = new System.Drawing.Point(178, 119);
            this.chkSC_E.Name = "chkSC_E";
            this.chkSC_E.Size = new System.Drawing.Size(46, 19);
            this.chkSC_E.TabIndex = 10;
            this.chkSC_E.Text = "Edit";
            // 
            // chkSC_Ap
            // 
            this.chkSC_Ap.AutoSize = true;
            this.chkSC_Ap.Location = new System.Drawing.Point(232, 119);
            this.chkSC_Ap.Name = "chkSC_Ap";
            this.chkSC_Ap.Size = new System.Drawing.Size(52, 19);
            this.chkSC_Ap.TabIndex = 11;
            this.chkSC_Ap.Text = "Appr";
            // 
            // lblNC
            // 
            this.lblNC.AutoSize = true;
            this.lblNC.Location = new System.Drawing.Point(10, 153);
            this.lblNC.Name = "lblNC";
            this.lblNC.Size = new System.Drawing.Size(27, 15);
            this.lblNC.TabIndex = 12;
            this.lblNC.Text = "NC:";
            // 
            // chkNC_V
            // 
            this.chkNC_V.AutoSize = true;
            this.chkNC_V.Location = new System.Drawing.Point(70, 151);
            this.chkNC_V.Name = "chkNC_V";
            this.chkNC_V.Size = new System.Drawing.Size(51, 19);
            this.chkNC_V.TabIndex = 12;
            this.chkNC_V.Text = "View";
            // 
            // chkNC_A
            // 
            this.chkNC_A.AutoSize = true;
            this.chkNC_A.Location = new System.Drawing.Point(124, 151);
            this.chkNC_A.Name = "chkNC_A";
            this.chkNC_A.Size = new System.Drawing.Size(48, 19);
            this.chkNC_A.TabIndex = 13;
            this.chkNC_A.Text = "Add";
            // 
            // chkNC_E
            // 
            this.chkNC_E.AutoSize = true;
            this.chkNC_E.Location = new System.Drawing.Point(178, 151);
            this.chkNC_E.Name = "chkNC_E";
            this.chkNC_E.Size = new System.Drawing.Size(46, 19);
            this.chkNC_E.TabIndex = 14;
            this.chkNC_E.Text = "Edit";
            // 
            // chkNC_Ap
            // 
            this.chkNC_Ap.AutoSize = true;
            this.chkNC_Ap.Location = new System.Drawing.Point(232, 151);
            this.chkNC_Ap.Name = "chkNC_Ap";
            this.chkNC_Ap.Size = new System.Drawing.Size(52, 19);
            this.chkNC_Ap.TabIndex = 15;
            this.chkNC_Ap.Text = "Appr";
            // 
            // lblPC
            // 
            this.lblPC.AutoSize = true;
            this.lblPC.Location = new System.Drawing.Point(10, 185);
            this.lblPC.Name = "lblPC";
            this.lblPC.Size = new System.Drawing.Size(25, 15);
            this.lblPC.TabIndex = 16;
            this.lblPC.Text = "PC:";
            // 
            // chkPC_V
            // 
            this.chkPC_V.AutoSize = true;
            this.chkPC_V.Location = new System.Drawing.Point(70, 183);
            this.chkPC_V.Name = "chkPC_V";
            this.chkPC_V.Size = new System.Drawing.Size(51, 19);
            this.chkPC_V.TabIndex = 16;
            this.chkPC_V.Text = "View";
            // 
            // chkPC_A
            // 
            this.chkPC_A.AutoSize = true;
            this.chkPC_A.Location = new System.Drawing.Point(124, 183);
            this.chkPC_A.Name = "chkPC_A";
            this.chkPC_A.Size = new System.Drawing.Size(48, 19);
            this.chkPC_A.TabIndex = 17;
            this.chkPC_A.Text = "Add";
            // 
            // chkPC_E
            // 
            this.chkPC_E.AutoSize = true;
            this.chkPC_E.Location = new System.Drawing.Point(178, 183);
            this.chkPC_E.Name = "chkPC_E";
            this.chkPC_E.Size = new System.Drawing.Size(46, 19);
            this.chkPC_E.TabIndex = 18;
            this.chkPC_E.Text = "Edit";
            // 
            // chkPC_Ap
            // 
            this.chkPC_Ap.AutoSize = true;
            this.chkPC_Ap.Location = new System.Drawing.Point(232, 183);
            this.chkPC_Ap.Name = "chkPC_Ap";
            this.chkPC_Ap.Size = new System.Drawing.Size(52, 19);
            this.chkPC_Ap.TabIndex = 19;
            this.chkPC_Ap.Text = "Appr";
            // 
            // btnSaveRow
            // 
            this.btnSaveRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.btnSaveRow.FlatAppearance.BorderSize = 0;
            this.btnSaveRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveRow.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnSaveRow.ForeColor = System.Drawing.Color.White;
            this.btnSaveRow.Location = new System.Drawing.Point(10, 215);
            this.btnSaveRow.Name = "btnSaveRow";
            this.btnSaveRow.Size = new System.Drawing.Size(100, 28);
            this.btnSaveRow.TabIndex = 20;
            this.btnSaveRow.Text = "💾 บันทึก";
            this.btnSaveRow.UseVisualStyleBackColor = false;
            this.btnSaveRow.Click += new System.EventHandler(this.btnSaveRow_Click);
            // 
            // btnCancelRow
            // 
            this.btnCancelRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnCancelRow.FlatAppearance.BorderSize = 0;
            this.btnCancelRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelRow.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnCancelRow.ForeColor = System.Drawing.Color.White;
            this.btnCancelRow.Location = new System.Drawing.Point(120, 215);
            this.btnCancelRow.Name = "btnCancelRow";
            this.btnCancelRow.Size = new System.Drawing.Size(100, 28);
            this.btnCancelRow.TabIndex = 21;
            this.btnCancelRow.Text = "✕ ยกเลิก";
            this.btnCancelRow.UseVisualStyleBackColor = false;
            this.btnCancelRow.Click += new System.EventHandler(this.btnCancelRow_Click);
            // 
            // AuthorityConfigForm
            // 
            this.ClientSize = new System.Drawing.Size(1084, 611);
            this.Controls.Add(this.split);
            this.Controls.Add(this.pnlEdit);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.Name = "AuthorityConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Authority Config";
            this.Load += new System.EventHandler(this.AuthorityConfigForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.split.Panel1.ResumeLayout(false);
            this.split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split)).EndInit();
            this.split.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.pnlEdit.ResumeLayout(false);
            this.grpEdit.ResumeLayout(false);
            this.grpEdit.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel         pnlHeader;
        private System.Windows.Forms.Label         lblTitle;
        private System.Windows.Forms.Panel         pnlToolbar;
        private System.Windows.Forms.Button        btnAdd;
        private System.Windows.Forms.Button        btnEdit;
        private System.Windows.Forms.Button        btnDelete;
        private System.Windows.Forms.Button        btnClose;
        private System.Windows.Forms.SplitContainer split;
        private System.Windows.Forms.TreeView      tvOrg;
        private System.Windows.Forms.Panel         pnlRight;
        private System.Windows.Forms.Label         lblFilter;
        private System.Windows.Forms.DataGridView  dgv;
        private System.Windows.Forms.Panel         pnlEdit;
        private System.Windows.Forms.GroupBox      grpEdit;
        private System.Windows.Forms.Label         lblEditLabel;
        private System.Windows.Forms.TextBox       txtLabel;
        private System.Windows.Forms.Label         lblEditPosition;
        private System.Windows.Forms.TextBox       txtPosition;
        private System.Windows.Forms.Label         lblEditDept;
        private System.Windows.Forms.TextBox       txtDept;
        private System.Windows.Forms.Label         lblEditDiv;
        private System.Windows.Forms.TextBox       txtDiv;
        private System.Windows.Forms.Label         lblEditLevelCode;
        private System.Windows.Forms.TextBox       txtLevelCode;
        private System.Windows.Forms.Label         lblHC;
        private System.Windows.Forms.CheckBox      chkHC_V;
        private System.Windows.Forms.CheckBox      chkHC_A;
        private System.Windows.Forms.CheckBox      chkHC_E;
        private System.Windows.Forms.CheckBox      chkHC_Ap;
        private System.Windows.Forms.Label         lblSC;
        private System.Windows.Forms.CheckBox      chkSC_V;
        private System.Windows.Forms.CheckBox      chkSC_A;
        private System.Windows.Forms.CheckBox      chkSC_E;
        private System.Windows.Forms.CheckBox      chkSC_Ap;
        private System.Windows.Forms.Label         lblNC;
        private System.Windows.Forms.CheckBox      chkNC_V;
        private System.Windows.Forms.CheckBox      chkNC_A;
        private System.Windows.Forms.CheckBox      chkNC_E;
        private System.Windows.Forms.CheckBox      chkNC_Ap;
        private System.Windows.Forms.Label         lblPC;
        private System.Windows.Forms.CheckBox      chkPC_V;
        private System.Windows.Forms.CheckBox      chkPC_A;
        private System.Windows.Forms.CheckBox      chkPC_E;
        private System.Windows.Forms.CheckBox      chkPC_Ap;
        private System.Windows.Forms.Button        btnSaveRow;
        private System.Windows.Forms.Button        btnCancelRow;
    }
}
