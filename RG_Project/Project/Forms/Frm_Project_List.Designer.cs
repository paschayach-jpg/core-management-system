namespace RG_Project.Project.Forms
{
    partial class Frm_Project_List
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lb_date = new System.Windows.Forms.Label();
            this.lb_user = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlAccent = new System.Windows.Forms.Panel();
            this.pnlFilter = new System.Windows.Forms.Panel();
            this.pnlFilterSep = new System.Windows.Forms.Panel();
            this.btnTasks = new System.Windows.Forms.Button();
            this.btnManageTemplate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtTotal = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.lblProjNoL = new System.Windows.Forms.Label();
            this.txtProjectNo = new System.Windows.Forms.TextBox();
            this.lblNameL = new System.Windows.Forms.Label();
            this.txtProjectName = new System.Windows.Forms.TextBox();
            this.lblStatusL = new System.Windows.Forms.Label();
            this.cbbStatus = new System.Windows.Forms.ComboBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.lblTmplL = new System.Windows.Forms.Label();
            this.cbbTemplate = new System.Windows.Forms.ComboBox();
            this.chkUseDateFilter = new System.Windows.Forms.CheckBox();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.lblTo = new System.Windows.Forms.Label();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.pnlToolbarSep = new System.Windows.Forms.Panel();
            this.dgvProjects = new System.Windows.Forms.DataGridView();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.lblTotal = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlHeader.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProjects)).BeginInit();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.lb_date);
            this.pnlHeader.Controls.Add(this.lb_user);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1100, 32);
            this.pnlHeader.TabIndex = 10;
            // 
            // lb_date
            // 
            this.lb_date.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_date.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lb_date.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(190)))), ((int)(((byte)(235)))));
            this.lb_date.Location = new System.Drawing.Point(730, 30);
            this.lb_date.Name = "lb_date";
            this.lb_date.Size = new System.Drawing.Size(360, 20);
            this.lb_date.TabIndex = 2;
            this.lb_date.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lb_user
            // 
            this.lb_user.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_user.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lb_user.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.lb_user.Location = new System.Drawing.Point(730, 8);
            this.lb_user.Name = "lb_user";
            this.lb_user.Size = new System.Drawing.Size(360, 20);
            this.lb_user.TabIndex = 1;
            this.lb_user.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(420, 34);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "ทะเบียนโครงการ";
            // 
            // pnlAccent
            // 
            this.pnlAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(185)))), ((int)(((byte)(0)))));
            this.pnlAccent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAccent.Location = new System.Drawing.Point(0, 32);
            this.pnlAccent.Name = "pnlAccent";
            this.pnlAccent.Size = new System.Drawing.Size(1100, 3);
            this.pnlAccent.TabIndex = 11;
            // 
            // pnlFilter
            // 
            this.pnlFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlFilter.Controls.Add(this.pnlFilterSep);
            this.pnlFilter.Controls.Add(this.btnManageTemplate);
            this.pnlFilter.Controls.Add(this.btnTasks);
            this.pnlFilter.Controls.Add(this.btnDelete);
            this.pnlFilter.Controls.Add(this.btnEdit);
            this.pnlFilter.Controls.Add(this.btnAdd);
            this.pnlFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilter.Location = new System.Drawing.Point(0, 35);
            this.pnlFilter.Name = "pnlFilter";
            this.pnlFilter.Size = new System.Drawing.Size(1100, 41);
            this.pnlFilter.TabIndex = 9;
            // 
            // pnlFilterSep
            // 
            this.pnlFilterSep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(210)))), ((int)(((byte)(235)))));
            this.pnlFilterSep.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFilterSep.Location = new System.Drawing.Point(0, 40);
            this.pnlFilterSep.Name = "pnlFilterSep";
            this.pnlFilterSep.Size = new System.Drawing.Size(1100, 1);
            this.pnlFilterSep.TabIndex = 99;
            // 
            // btnTasks
            // 
            this.btnTasks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(66)))), ((int)(((byte)(193)))));
            this.btnTasks.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTasks.FlatAppearance.BorderSize = 0;
            this.btnTasks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTasks.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnTasks.ForeColor = System.Drawing.Color.White;
            this.btnTasks.Location = new System.Drawing.Point(313, 6);
            this.btnTasks.Name = "btnTasks";
            this.btnTasks.Size = new System.Drawing.Size(120, 30);
            this.btnTasks.TabIndex = 17;
            this.btnTasks.Text = "Tasks";
            this.btnTasks.UseVisualStyleBackColor = false;
            this.btnTasks.Click += new System.EventHandler(this.btnTasks_Click);
            //
            // btnManageTemplate
            //
            this.btnManageTemplate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(120)))), ((int)(((byte)(90)))));
            this.btnManageTemplate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageTemplate.FlatAppearance.BorderSize = 0;
            this.btnManageTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageTemplate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnManageTemplate.ForeColor = System.Drawing.Color.White;
            this.btnManageTemplate.Location = new System.Drawing.Point(439, 6);
            this.btnManageTemplate.Name = "btnManageTemplate";
            this.btnManageTemplate.Size = new System.Drawing.Size(160, 30);
            this.btnManageTemplate.TabIndex = 18;
            this.btnManageTemplate.Text = "จัดการประเภทโครงการ";
            this.btnManageTemplate.UseVisualStyleBackColor = false;
            this.btnManageTemplate.Click += new System.EventHandler(this.btnManageTemplate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(225, 6);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(80, 30);
            this.btnDelete.TabIndex = 16;
            this.btnDelete.Text = "ลบ";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(110)))), ((int)(((byte)(165)))));
            this.btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnEdit.ForeColor = System.Drawing.Color.White;
            this.btnEdit.Location = new System.Drawing.Point(127, 6);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(90, 30);
            this.btnEdit.TabIndex = 15;
            this.btnEdit.Text = "แก้ไข";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(4, 6);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(115, 30);
            this.btnAdd.TabIndex = 14;
            this.btnAdd.Text = "เพิ่มโครงการ";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlToolbar.Controls.Add(this.panel1);
            this.pnlToolbar.Controls.Add(this.lblProjNoL);
            this.pnlToolbar.Controls.Add(this.txtProjectNo);
            this.pnlToolbar.Controls.Add(this.lblNameL);
            this.pnlToolbar.Controls.Add(this.txtProjectName);
            this.pnlToolbar.Controls.Add(this.lblStatusL);
            this.pnlToolbar.Controls.Add(this.cbbStatus);
            this.pnlToolbar.Controls.Add(this.btnSearch);
            this.pnlToolbar.Controls.Add(this.lblTmplL);
            this.pnlToolbar.Controls.Add(this.cbbTemplate);
            this.pnlToolbar.Controls.Add(this.chkUseDateFilter);
            this.pnlToolbar.Controls.Add(this.dtpFrom);
            this.pnlToolbar.Controls.Add(this.lblTo);
            this.pnlToolbar.Controls.Add(this.dtpTo);
            this.pnlToolbar.Controls.Add(this.pnlToolbarSep);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 76);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1100, 117);
            this.pnlToolbar.TabIndex = 8;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.panel1.Controls.Add(this.txtTotal);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Location = new System.Drawing.Point(17, 54);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(166, 47);
            this.panel1.TabIndex = 12;
            // 
            // txtTotal
            // 
            this.txtTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.txtTotal.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(222)));
            this.txtTotal.ForeColor = System.Drawing.Color.White;
            this.txtTotal.Location = new System.Drawing.Point(5, 23);
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.Size = new System.Drawing.Size(155, 19);
            this.txtTotal.TabIndex = 2;
            this.txtTotal.Text = "0";
            this.txtTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.panel2.Controls.Add(this.label4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(166, 21);
            this.panel2.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(3, 1);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "จำนวนทั้งหมด";
            // 
            // lblProjNoL
            // 
            this.lblProjNoL.AutoSize = true;
            this.lblProjNoL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProjNoL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblProjNoL.Location = new System.Drawing.Point(14, 29);
            this.lblProjNoL.Name = "lblProjNoL";
            this.lblProjNoL.Size = new System.Drawing.Size(72, 15);
            this.lblProjNoL.TabIndex = 100;
            this.lblProjNoL.Text = "เลขที่โครงการ:";
            // 
            // txtProjectNo
            // 
            this.txtProjectNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtProjectNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProjectNo.Location = new System.Drawing.Point(114, 25);
            this.txtProjectNo.Name = "txtProjectNo";
            this.txtProjectNo.Size = new System.Drawing.Size(130, 23);
            this.txtProjectNo.TabIndex = 101;
            this.txtProjectNo.TextChanged += new System.EventHandler(this.txtProjectNo_TextChanged);
            // 
            // lblNameL
            // 
            this.lblNameL.AutoSize = true;
            this.lblNameL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblNameL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblNameL.Location = new System.Drawing.Point(511, 82);
            this.lblNameL.Name = "lblNameL";
            this.lblNameL.Size = new System.Drawing.Size(60, 15);
            this.lblNameL.TabIndex = 102;
            this.lblNameL.Text = "ชื่อโครงการ:";
            // 
            // txtProjectName
            // 
            this.txtProjectName.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtProjectName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtProjectName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProjectName.Location = new System.Drawing.Point(589, 78);
            this.txtProjectName.Name = "txtProjectName";
            this.txtProjectName.Size = new System.Drawing.Size(180, 23);
            this.txtProjectName.TabIndex = 103;
            this.txtProjectName.TextChanged += new System.EventHandler(this.txtProjectName_TextChanged);
            // 
            // lblStatusL
            // 
            this.lblStatusL.AutoSize = true;
            this.lblStatusL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStatusL.Location = new System.Drawing.Point(783, 82);
            this.lblStatusL.Name = "lblStatusL";
            this.lblStatusL.Size = new System.Drawing.Size(39, 15);
            this.lblStatusL.TabIndex = 104;
            this.lblStatusL.Text = "สถานะ:";
            // 
            // cbbStatus
            // 
            this.cbbStatus.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.cbbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbStatus.Location = new System.Drawing.Point(833, 78);
            this.cbbStatus.Name = "cbbStatus";
            this.cbbStatus.Size = new System.Drawing.Size(145, 23);
            this.cbbStatus.TabIndex = 105;
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(597, 17);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(106, 30);
            this.btnSearch.TabIndex = 112;
            this.btnSearch.Text = "ค้นหา";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // lblTmplL
            // 
            this.lblTmplL.AutoSize = true;
            this.lblTmplL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTmplL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblTmplL.Location = new System.Drawing.Point(199, 82);
            this.lblTmplL.Name = "lblTmplL";
            this.lblTmplL.Size = new System.Drawing.Size(80, 15);
            this.lblTmplL.TabIndex = 106;
            this.lblTmplL.Text = "ประเภทโครงการ:";
            // 
            // cbbTemplate
            // 
            this.cbbTemplate.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.cbbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbTemplate.Location = new System.Drawing.Point(286, 78);
            this.cbbTemplate.Name = "cbbTemplate";
            this.cbbTemplate.Size = new System.Drawing.Size(215, 23);
            this.cbbTemplate.TabIndex = 107;
            // 
            // chkUseDateFilter
            // 
            this.chkUseDateFilter.AutoSize = true;
            this.chkUseDateFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkUseDateFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.chkUseDateFilter.Location = new System.Drawing.Point(272, 28);
            this.chkUseDateFilter.Name = "chkUseDateFilter";
            this.chkUseDateFilter.Size = new System.Drawing.Size(69, 19);
            this.chkUseDateFilter.TabIndex = 108;
            this.chkUseDateFilter.Text = "วันที่สร้าง:";
            // 
            // dtpFrom
            // 
            this.dtpFrom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(343, 25);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(110, 23);
            this.dtpFrom.TabIndex = 109;
            this.dtpFrom.ValueChanged += new System.EventHandler(this.dtpFrom_ValueChanged);
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblTo.Location = new System.Drawing.Point(460, 29);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(19, 15);
            this.lblTo.TabIndex = 110;
            this.lblTo.Text = "ถึง";
            // 
            // dtpTo
            // 
            this.dtpTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(481, 25);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(110, 23);
            this.dtpTo.TabIndex = 111;
            this.dtpTo.ValueChanged += new System.EventHandler(this.dtpTo_ValueChanged);
            // 
            // pnlToolbarSep
            // 
            this.pnlToolbarSep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(235)))));
            this.pnlToolbarSep.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlToolbarSep.Location = new System.Drawing.Point(0, 116);
            this.pnlToolbarSep.Name = "pnlToolbarSep";
            this.pnlToolbarSep.Size = new System.Drawing.Size(1100, 1);
            this.pnlToolbarSep.TabIndex = 99;
            // 
            // dgvProjects
            // 
            this.dgvProjects.AllowUserToResizeRows = false;
            this.dgvProjects.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(254)))));
            this.dgvProjects.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvProjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProjects.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvProjects.Location = new System.Drawing.Point(0, 193);
            this.dgvProjects.Name = "dgvProjects";
            this.dgvProjects.RowHeadersWidth = 25;
            this.dgvProjects.Size = new System.Drawing.Size(1100, 435);
            this.dgvProjects.TabIndex = 0;
            this.dgvProjects.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProjects_CellDoubleClick);
            // 
            // statusBar
            // 
            this.statusBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblTotal,
            this.lblVersion});
            this.statusBar.Location = new System.Drawing.Point(0, 628);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(1100, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 3;
            // 
            // lblTotal
            // 
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(85, 17);
            this.lblTotal.Text = "รายการทั้งหมด: 0";
            // 
            // lblVersion
            // 
            this.lblVersion.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(170)))), ((int)(((byte)(220)))));
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(91, 17);
            this.lblVersion.Text = "RG Claim System";
            // 
            // Frm_Project_List
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Controls.Add(this.dgvProjects);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlFilter);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusBar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 580);
            this.Name = "Frm_Project_List";
            this.Text = "ทะเบียนโครงการ";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Frm_Project_List_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlFilter.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProjects)).EndInit();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // ─── Declarations ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel                  pnlHeader;
        private System.Windows.Forms.Label                  lblTitle;
        private System.Windows.Forms.Label                  lb_user;
        private System.Windows.Forms.Label                  lb_date;
        private System.Windows.Forms.Panel                  pnlAccent;
        private System.Windows.Forms.Panel                  pnlFilter;
        private System.Windows.Forms.Panel                  pnlFilterSep;
        private System.Windows.Forms.Panel                  pnlToolbar;
        private System.Windows.Forms.Panel                  pnlToolbarSep;
        private System.Windows.Forms.Button                 btnAdd;
        private System.Windows.Forms.Button                 btnEdit;
        private System.Windows.Forms.Button                 btnDelete;
        private System.Windows.Forms.Button                 btnTasks;
        private System.Windows.Forms.Button                 btnManageTemplate;
        private System.Windows.Forms.DataGridView           dgvProjects;
        private System.Windows.Forms.StatusStrip            statusBar;
        private System.Windows.Forms.ToolStripStatusLabel   lblTotal;
        private System.Windows.Forms.ToolStripStatusLabel   lblVersion;
        private System.Windows.Forms.Label lblProjNoL;
        private System.Windows.Forms.TextBox txtProjectNo;
        private System.Windows.Forms.Label lblNameL;
        private System.Windows.Forms.TextBox txtProjectName;
        private System.Windows.Forms.Label lblStatusL;
        private System.Windows.Forms.ComboBox cbbStatus;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Label lblTmplL;
        private System.Windows.Forms.ComboBox cbbTemplate;
        private System.Windows.Forms.CheckBox chkUseDateFilter;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtTotal;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
    }
}
