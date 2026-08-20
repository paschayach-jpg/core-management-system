namespace CMS.Presentation.Modules.RGP
{
    partial class Frm_Project_Detail
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblProjectNo = new System.Windows.Forms.Label();
            this.lb_user = new System.Windows.Forms.Label();
            this.lb_date = new System.Windows.Forms.Label();
            this.pnlAccent = new System.Windows.Forms.Panel();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabInfo = new System.Windows.Forms.TabPage();
            this.pnlInfo = new System.Windows.Forms.TableLayoutPanel();
            this.lblProjNoL = new System.Windows.Forms.Label();
            this.pnlProjectNoRow = new System.Windows.Forms.Panel();
            this.txtProjectNo = new System.Windows.Forms.TextBox();
            this.chkAutoGenProjectNo = new System.Windows.Forms.CheckBox();
            this.lblNameL = new System.Windows.Forms.Label();
            this.txtProjectName = new System.Windows.Forms.TextBox();
            this.lblTmplL = new System.Windows.Forms.Label();
            this.cbbTemplate = new System.Windows.Forms.ComboBox();
            this.lblStatusL = new System.Windows.Forms.Label();
            this.lblStatusDisplay = new System.Windows.Forms.Label();
            this.lblStartL = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.lblDueL = new System.Windows.Forms.Label();
            this.dtpDueDate = new System.Windows.Forms.DateTimePicker();
            this.lblDescL = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.pnlAudit = new System.Windows.Forms.Panel();
            this.lblCrByL = new System.Windows.Forms.Label();
            this.lblCreateBy = new System.Windows.Forms.Label();
            this.lblCrDtL = new System.Windows.Forms.Label();
            this.lblCreateDate = new System.Windows.Forms.Label();
            this.lblUpByL = new System.Windows.Forms.Label();
            this.lblUpdateBy = new System.Windows.Forms.Label();
            this.lblUpDtL = new System.Windows.Forms.Label();
            this.lblUpdateDate = new System.Windows.Forms.Label();
            this.tabStates = new System.Windows.Forms.TabPage();
            this.dgvStates = new System.Windows.Forms.DataGridView();
            this.pnlStateToolbar = new System.Windows.Forms.Panel();
            this.btnAddState = new System.Windows.Forms.Button();
            this.btnEditState = new System.Windows.Forms.Button();
            this.btnDeleteState = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnViewTasks = new System.Windows.Forms.Button();
            this.tabProgress = new System.Windows.Forms.TabPage();
            this.dgvProgress = new System.Windows.Forms.DataGridView();
            this.pnlProgressHeader = new System.Windows.Forms.Panel();
            this.lblOverallPct = new System.Windows.Forms.Label();
            this.progressOverall = new System.Windows.Forms.ProgressBar();
            this.btnUpdateProgress = new System.Windows.Forms.Button();
            this.tabDocs = new System.Windows.Forms.TabPage();
            this.dgvDocWorkflow = new System.Windows.Forms.DataGridView();
            this.pnlDocToolbar = new System.Windows.Forms.Panel();
            this.lblStateFilterL = new System.Windows.Forms.Label();
            this.cbbStateFilter = new System.Windows.Forms.ComboBox();
            this.btnAddDoc = new System.Windows.Forms.Button();
            this.btnEditDoc = new System.Windows.Forms.Button();
            this.btnDeleteDoc = new System.Windows.Forms.Button();
            this.btnUpdateDocStatus = new System.Windows.Forms.Button();
            this.tabMembers = new System.Windows.Forms.TabPage();
            this.pnlMemberToolbar = new System.Windows.Forms.Panel();
            this.btnAddMember = new System.Windows.Forms.Button();
            this.btnDeleteMember = new System.Windows.Forms.Button();
            this.dgvMembers = new System.Windows.Forms.DataGridView();
            this.cbbStatus = new System.Windows.Forms.ComboBox();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabInfo.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.pnlProjectNoRow.SuspendLayout();
            this.pnlAudit.SuspendLayout();
            this.tabStates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStates)).BeginInit();
            this.pnlStateToolbar.SuspendLayout();
            this.tabProgress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProgress)).BeginInit();
            this.pnlProgressHeader.SuspendLayout();
            this.tabDocs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocWorkflow)).BeginInit();
            this.pnlDocToolbar.SuspendLayout();
            this.tabMembers.SuspendLayout();
            this.pnlMemberToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMembers)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblProjectNo);
            this.pnlHeader.Controls.Add(this.lb_user);
            this.pnlHeader.Controls.Add(this.lb_date);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(900, 63);
            this.pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(14, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(380, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "รายละเอียดโครงการ";
            // 
            // lblProjectNo
            // 
            this.lblProjectNo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProjectNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(210)))), ((int)(((byte)(80)))));
            this.lblProjectNo.Location = new System.Drawing.Point(14, 37);
            this.lblProjectNo.Name = "lblProjectNo";
            this.lblProjectNo.Size = new System.Drawing.Size(400, 18);
            this.lblProjectNo.TabIndex = 1;
            // 
            // lb_user
            // 
            this.lb_user.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_user.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lb_user.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(210)))), ((int)(((byte)(240)))));
            this.lb_user.Location = new System.Drawing.Point(696, 9);
            this.lb_user.Name = "lb_user";
            this.lb_user.Size = new System.Drawing.Size(200, 18);
            this.lb_user.TabIndex = 2;
            this.lb_user.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lb_date
            // 
            this.lb_date.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_date.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lb_date.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(210)))), ((int)(((byte)(240)))));
            this.lb_date.Location = new System.Drawing.Point(696, 30);
            this.lb_date.Name = "lb_date";
            this.lb_date.Size = new System.Drawing.Size(200, 18);
            this.lb_date.TabIndex = 3;
            this.lb_date.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pnlAccent
            // 
            this.pnlAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(185)))), ((int)(((byte)(0)))));
            this.pnlAccent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAccent.Location = new System.Drawing.Point(0, 63);
            this.pnlAccent.Name = "pnlAccent";
            this.pnlAccent.Size = new System.Drawing.Size(900, 3);
            this.pnlAccent.TabIndex = 3;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabInfo);
            this.tabMain.Controls.Add(this.tabStates);
            this.tabMain.Controls.Add(this.tabProgress);
            this.tabMain.Controls.Add(this.tabDocs);
            this.tabMain.Controls.Add(this.tabMembers);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabMain.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tabMain.ItemSize = new System.Drawing.Size(140, 28);
            this.tabMain.Location = new System.Drawing.Point(0, 66);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(900, 524);
            this.tabMain.TabIndex = 0;
            this.tabMain.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabMain_DrawItem);
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);
            // 
            // tabInfo
            // 
            this.tabInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.tabInfo.Controls.Add(this.pnlInfo);
            this.tabInfo.Controls.Add(this.pnlAudit);
            this.tabInfo.Location = new System.Drawing.Point(4, 32);
            this.tabInfo.Name = "tabInfo";
            this.tabInfo.Size = new System.Drawing.Size(892, 488);
            this.tabInfo.TabIndex = 0;
            this.tabInfo.Text = "ข้อมูลโครงการ";
            // 
            // pnlInfo
            // 
            this.pnlInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlInfo.ColumnCount = 2;
            this.pnlInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.pnlInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlInfo.Controls.Add(this.lblProjNoL, 0, 0);
            this.pnlInfo.Controls.Add(this.pnlProjectNoRow, 1, 0);
            this.pnlInfo.Controls.Add(this.lblNameL, 0, 1);
            this.pnlInfo.Controls.Add(this.txtProjectName, 1, 1);
            this.pnlInfo.Controls.Add(this.lblTmplL, 0, 2);
            this.pnlInfo.Controls.Add(this.cbbTemplate, 1, 2);
            this.pnlInfo.Controls.Add(this.lblStatusL, 0, 3);
            this.pnlInfo.Controls.Add(this.lblStatusDisplay, 1, 3);
            this.pnlInfo.Controls.Add(this.lblStartL, 0, 4);
            this.pnlInfo.Controls.Add(this.dtpStartDate, 1, 4);
            this.pnlInfo.Controls.Add(this.lblDueL, 0, 5);
            this.pnlInfo.Controls.Add(this.dtpDueDate, 1, 5);
            this.pnlInfo.Controls.Add(this.lblDescL, 0, 6);
            this.pnlInfo.Controls.Add(this.txtDescription, 1, 6);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInfo.Location = new System.Drawing.Point(0, 0);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.pnlInfo.RowCount = 7;
            this.pnlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.pnlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.pnlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.pnlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.pnlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.pnlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.pnlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.pnlInfo.Size = new System.Drawing.Size(892, 458);
            this.pnlInfo.TabIndex = 0;
            // 
            // lblProjNoL
            // 
            this.lblProjNoL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblProjNoL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProjNoL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblProjNoL.Location = new System.Drawing.Point(13, 10);
            this.lblProjNoL.Name = "lblProjNoL";
            this.lblProjNoL.Size = new System.Drawing.Size(134, 32);
            this.lblProjNoL.TabIndex = 0;
            this.lblProjNoL.Text = "เลขที่โครงการ:";
            this.lblProjNoL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlProjectNoRow
            // 
            this.pnlProjectNoRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlProjectNoRow.Controls.Add(this.txtProjectNo);
            this.pnlProjectNoRow.Controls.Add(this.chkAutoGenProjectNo);
            this.pnlProjectNoRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlProjectNoRow.Location = new System.Drawing.Point(153, 13);
            this.pnlProjectNoRow.Name = "pnlProjectNoRow";
            this.pnlProjectNoRow.Size = new System.Drawing.Size(726, 26);
            this.pnlProjectNoRow.TabIndex = 1;
            // 
            // txtProjectNo
            // 
            this.txtProjectNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtProjectNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProjectNo.Location = new System.Drawing.Point(0, 3);
            this.txtProjectNo.MaxLength = 50;
            this.txtProjectNo.Name = "txtProjectNo";
            this.txtProjectNo.Size = new System.Drawing.Size(260, 23);
            this.txtProjectNo.TabIndex = 0;
            // 
            // chkAutoGenProjectNo
            // 
            this.chkAutoGenProjectNo.AutoSize = true;
            this.chkAutoGenProjectNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkAutoGenProjectNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.chkAutoGenProjectNo.Location = new System.Drawing.Point(270, 5);
            this.chkAutoGenProjectNo.Name = "chkAutoGenProjectNo";
            this.chkAutoGenProjectNo.Size = new System.Drawing.Size(164, 19);
            this.chkAutoGenProjectNo.TabIndex = 1;
            this.chkAutoGenProjectNo.Text = "Auto Generate Project No.";
            this.chkAutoGenProjectNo.CheckedChanged += new System.EventHandler(this.chkAutoGenProjectNo_CheckedChanged);
            // 
            // lblNameL
            // 
            this.lblNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNameL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblNameL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblNameL.Location = new System.Drawing.Point(13, 42);
            this.lblNameL.Name = "lblNameL";
            this.lblNameL.Size = new System.Drawing.Size(134, 32);
            this.lblNameL.TabIndex = 2;
            this.lblNameL.Text = "ชื่อโครงการ *:";
            this.lblNameL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtProjectName
            // 
            this.txtProjectName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtProjectName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProjectName.Location = new System.Drawing.Point(153, 45);
            this.txtProjectName.MaxLength = 300;
            this.txtProjectName.Name = "txtProjectName";
            this.txtProjectName.Size = new System.Drawing.Size(726, 23);
            this.txtProjectName.TabIndex = 3;
            // 
            // lblTmplL
            // 
            this.lblTmplL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTmplL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTmplL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblTmplL.Location = new System.Drawing.Point(13, 74);
            this.lblTmplL.Name = "lblTmplL";
            this.lblTmplL.Size = new System.Drawing.Size(134, 32);
            this.lblTmplL.TabIndex = 4;
            this.lblTmplL.Text = "ประเภทโครงการ";
            this.lblTmplL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbTemplate
            // 
            this.cbbTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbTemplate.Location = new System.Drawing.Point(153, 77);
            this.cbbTemplate.Name = "cbbTemplate";
            this.cbbTemplate.Size = new System.Drawing.Size(726, 23);
            this.cbbTemplate.TabIndex = 5;
            this.cbbTemplate.SelectedIndexChanged += new System.EventHandler(this.cbbTemplate_SelectedIndexChanged);
            // 
            // lblStatusL
            // 
            this.lblStatusL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStatusL.Location = new System.Drawing.Point(13, 106);
            this.lblStatusL.Name = "lblStatusL";
            this.lblStatusL.Size = new System.Drawing.Size(134, 32);
            this.lblStatusL.TabIndex = 6;
            this.lblStatusL.Text = "สถานะ:";
            this.lblStatusL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStatusDisplay
            // 
            this.lblStatusDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(228)))), ((int)(((byte)(248)))));
            this.lblStatusDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusDisplay.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatusDisplay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblStatusDisplay.Location = new System.Drawing.Point(153, 106);
            this.lblStatusDisplay.Name = "lblStatusDisplay";
            this.lblStatusDisplay.Size = new System.Drawing.Size(726, 32);
            this.lblStatusDisplay.TabIndex = 7;
            this.lblStatusDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStartL
            // 
            this.lblStartL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStartL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStartL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStartL.Location = new System.Drawing.Point(13, 138);
            this.lblStartL.Name = "lblStartL";
            this.lblStartL.Size = new System.Drawing.Size(134, 32);
            this.lblStartL.TabIndex = 8;
            this.lblStartL.Text = "วันที่เริ่ม:";
            this.lblStartL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpStartDate.Location = new System.Drawing.Point(153, 141);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.ShowCheckBox = true;
            this.dtpStartDate.Size = new System.Drawing.Size(150, 23);
            this.dtpStartDate.TabIndex = 9;
            // 
            // lblDueL
            // 
            this.lblDueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDueL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDueL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblDueL.Location = new System.Drawing.Point(13, 170);
            this.lblDueL.Name = "lblDueL";
            this.lblDueL.Size = new System.Drawing.Size(134, 32);
            this.lblDueL.TabIndex = 10;
            this.lblDueL.Text = "วันกำหนดเสร็จ:";
            this.lblDueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dtpDueDate
            // 
            this.dtpDueDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpDueDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDueDate.Location = new System.Drawing.Point(153, 173);
            this.dtpDueDate.Name = "dtpDueDate";
            this.dtpDueDate.ShowCheckBox = true;
            this.dtpDueDate.Size = new System.Drawing.Size(150, 23);
            this.dtpDueDate.TabIndex = 11;
            // 
            // lblDescL
            // 
            this.lblDescL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDescL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblDescL.Location = new System.Drawing.Point(13, 202);
            this.lblDescL.Name = "lblDescL";
            this.lblDescL.Size = new System.Drawing.Size(134, 256);
            this.lblDescL.TabIndex = 12;
            this.lblDescL.Text = "รายละเอียด:";
            this.lblDescL.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtDescription
            // 
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDescription.Location = new System.Drawing.Point(153, 205);
            this.txtDescription.MaxLength = 1000;
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(726, 250);
            this.txtDescription.TabIndex = 13;
            // 
            // pnlAudit
            // 
            this.pnlAudit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlAudit.Controls.Add(this.lblCrByL);
            this.pnlAudit.Controls.Add(this.lblCreateBy);
            this.pnlAudit.Controls.Add(this.lblCrDtL);
            this.pnlAudit.Controls.Add(this.lblCreateDate);
            this.pnlAudit.Controls.Add(this.lblUpByL);
            this.pnlAudit.Controls.Add(this.lblUpdateBy);
            this.pnlAudit.Controls.Add(this.lblUpDtL);
            this.pnlAudit.Controls.Add(this.lblUpdateDate);
            this.pnlAudit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAudit.Location = new System.Drawing.Point(0, 458);
            this.pnlAudit.Name = "pnlAudit";
            this.pnlAudit.Padding = new System.Windows.Forms.Padding(10, 5, 10, 0);
            this.pnlAudit.Size = new System.Drawing.Size(892, 30);
            this.pnlAudit.TabIndex = 1;
            // 
            // lblCrByL
            // 
            this.lblCrByL.AutoSize = true;
            this.lblCrByL.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCrByL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(130)))), ((int)(((byte)(150)))));
            this.lblCrByL.Location = new System.Drawing.Point(10, 8);
            this.lblCrByL.Name = "lblCrByL";
            this.lblCrByL.Size = new System.Drawing.Size(45, 13);
            this.lblCrByL.TabIndex = 0;
            this.lblCrByL.Text = "สร้างโดย:";
            // 
            // lblCreateBy
            // 
            this.lblCreateBy.AutoSize = true;
            this.lblCreateBy.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCreateBy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblCreateBy.Location = new System.Drawing.Point(68, 8);
            this.lblCreateBy.Name = "lblCreateBy";
            this.lblCreateBy.Size = new System.Drawing.Size(0, 13);
            this.lblCreateBy.TabIndex = 1;
            // 
            // lblCrDtL
            // 
            this.lblCrDtL.AutoSize = true;
            this.lblCrDtL.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCrDtL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(130)))), ((int)(((byte)(150)))));
            this.lblCrDtL.Location = new System.Drawing.Point(200, 8);
            this.lblCrDtL.Name = "lblCrDtL";
            this.lblCrDtL.Size = new System.Drawing.Size(28, 13);
            this.lblCrDtL.TabIndex = 2;
            this.lblCrDtL.Text = "วันที่:";
            // 
            // lblCreateDate
            // 
            this.lblCreateDate.AutoSize = true;
            this.lblCreateDate.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCreateDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblCreateDate.Location = new System.Drawing.Point(235, 8);
            this.lblCreateDate.Name = "lblCreateDate";
            this.lblCreateDate.Size = new System.Drawing.Size(0, 13);
            this.lblCreateDate.TabIndex = 3;
            // 
            // lblUpByL
            // 
            this.lblUpByL.AutoSize = true;
            this.lblUpByL.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblUpByL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(130)))), ((int)(((byte)(150)))));
            this.lblUpByL.Location = new System.Drawing.Point(400, 8);
            this.lblUpByL.Name = "lblUpByL";
            this.lblUpByL.Size = new System.Drawing.Size(47, 13);
            this.lblUpByL.TabIndex = 4;
            this.lblUpByL.Text = "แก้ไขโดย:";
            // 
            // lblUpdateBy
            // 
            this.lblUpdateBy.AutoSize = true;
            this.lblUpdateBy.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblUpdateBy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblUpdateBy.Location = new System.Drawing.Point(465, 8);
            this.lblUpdateBy.Name = "lblUpdateBy";
            this.lblUpdateBy.Size = new System.Drawing.Size(0, 13);
            this.lblUpdateBy.TabIndex = 5;
            // 
            // lblUpDtL
            // 
            this.lblUpDtL.AutoSize = true;
            this.lblUpDtL.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblUpDtL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(130)))), ((int)(((byte)(150)))));
            this.lblUpDtL.Location = new System.Drawing.Point(600, 8);
            this.lblUpDtL.Name = "lblUpDtL";
            this.lblUpDtL.Size = new System.Drawing.Size(28, 13);
            this.lblUpDtL.TabIndex = 6;
            this.lblUpDtL.Text = "วันที่:";
            // 
            // lblUpdateDate
            // 
            this.lblUpdateDate.AutoSize = true;
            this.lblUpdateDate.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblUpdateDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblUpdateDate.Location = new System.Drawing.Point(635, 8);
            this.lblUpdateDate.Name = "lblUpdateDate";
            this.lblUpdateDate.Size = new System.Drawing.Size(0, 13);
            this.lblUpdateDate.TabIndex = 7;
            // 
            // tabStates
            // 
            this.tabStates.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.tabStates.Controls.Add(this.dgvStates);
            this.tabStates.Controls.Add(this.pnlStateToolbar);
            this.tabStates.Location = new System.Drawing.Point(4, 32);
            this.tabStates.Name = "tabStates";
            this.tabStates.Size = new System.Drawing.Size(892, 488);
            this.tabStates.TabIndex = 1;
            this.tabStates.Text = "ขั้นตอน";
            // 
            // dgvStates
            // 
            this.dgvStates.AllowUserToResizeRows = false;
            this.dgvStates.BackgroundColor = System.Drawing.Color.White;
            this.dgvStates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStates.Location = new System.Drawing.Point(0, 44);
            this.dgvStates.Name = "dgvStates";
            this.dgvStates.Size = new System.Drawing.Size(892, 444);
            this.dgvStates.TabIndex = 0;
            this.dgvStates.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvStates_CellDoubleClick);
            // 
            // pnlStateToolbar
            // 
            this.pnlStateToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlStateToolbar.Controls.Add(this.btnAddState);
            this.pnlStateToolbar.Controls.Add(this.btnEditState);
            this.pnlStateToolbar.Controls.Add(this.btnDeleteState);
            this.pnlStateToolbar.Controls.Add(this.btnMoveUp);
            this.pnlStateToolbar.Controls.Add(this.btnMoveDown);
            this.pnlStateToolbar.Controls.Add(this.btnViewTasks);
            this.pnlStateToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStateToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlStateToolbar.Name = "pnlStateToolbar";
            this.pnlStateToolbar.Padding = new System.Windows.Forms.Padding(5, 7, 5, 0);
            this.pnlStateToolbar.Size = new System.Drawing.Size(892, 44);
            this.pnlStateToolbar.TabIndex = 1;
            // 
            // btnAddState
            // 
            this.btnAddState.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnAddState.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddState.FlatAppearance.BorderSize = 0;
            this.btnAddState.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddState.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnAddState.ForeColor = System.Drawing.Color.White;
            this.btnAddState.Location = new System.Drawing.Point(5, 7);
            this.btnAddState.Name = "btnAddState";
            this.btnAddState.Size = new System.Drawing.Size(90, 28);
            this.btnAddState.TabIndex = 0;
            this.btnAddState.Text = "เพิ่มขั้นตอน";
            this.btnAddState.UseVisualStyleBackColor = false;
            this.btnAddState.Click += new System.EventHandler(this.btnAddState_Click);
            // 
            // btnEditState
            // 
            this.btnEditState.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(110)))), ((int)(((byte)(165)))));
            this.btnEditState.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditState.FlatAppearance.BorderSize = 0;
            this.btnEditState.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditState.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnEditState.ForeColor = System.Drawing.Color.White;
            this.btnEditState.Location = new System.Drawing.Point(105, 7);
            this.btnEditState.Name = "btnEditState";
            this.btnEditState.Size = new System.Drawing.Size(80, 28);
            this.btnEditState.TabIndex = 1;
            this.btnEditState.Text = "แก้ไข";
            this.btnEditState.UseVisualStyleBackColor = false;
            this.btnEditState.Click += new System.EventHandler(this.btnEditState_Click);
            // 
            // btnDeleteState
            // 
            this.btnDeleteState.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.btnDeleteState.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteState.FlatAppearance.BorderSize = 0;
            this.btnDeleteState.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteState.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDeleteState.ForeColor = System.Drawing.Color.White;
            this.btnDeleteState.Location = new System.Drawing.Point(195, 7);
            this.btnDeleteState.Name = "btnDeleteState";
            this.btnDeleteState.Size = new System.Drawing.Size(80, 28);
            this.btnDeleteState.TabIndex = 2;
            this.btnDeleteState.Text = "ลบ";
            this.btnDeleteState.UseVisualStyleBackColor = false;
            this.btnDeleteState.Click += new System.EventHandler(this.btnDeleteState_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnMoveUp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMoveUp.FlatAppearance.BorderSize = 0;
            this.btnMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMoveUp.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnMoveUp.ForeColor = System.Drawing.Color.White;
            this.btnMoveUp.Location = new System.Drawing.Point(285, 7);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(85, 28);
            this.btnMoveUp.TabIndex = 3;
            this.btnMoveUp.Text = "↑ เลื่อนขึ้น";
            this.btnMoveUp.UseVisualStyleBackColor = false;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnMoveDown.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMoveDown.FlatAppearance.BorderSize = 0;
            this.btnMoveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMoveDown.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnMoveDown.ForeColor = System.Drawing.Color.White;
            this.btnMoveDown.Location = new System.Drawing.Point(380, 7);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(85, 28);
            this.btnMoveDown.TabIndex = 4;
            this.btnMoveDown.Text = "↓ เลื่อนลง";
            this.btnMoveDown.UseVisualStyleBackColor = false;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // btnViewTasks
            // 
            this.btnViewTasks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnViewTasks.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewTasks.FlatAppearance.BorderSize = 0;
            this.btnViewTasks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewTasks.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnViewTasks.ForeColor = System.Drawing.Color.White;
            this.btnViewTasks.Location = new System.Drawing.Point(475, 7);
            this.btnViewTasks.Name = "btnViewTasks";
            this.btnViewTasks.Size = new System.Drawing.Size(100, 28);
            this.btnViewTasks.TabIndex = 5;
            this.btnViewTasks.Text = "📋 Tasks";
            this.btnViewTasks.UseVisualStyleBackColor = false;
            this.btnViewTasks.Visible = false;
            this.btnViewTasks.Click += new System.EventHandler(this.btnViewTasks_Click);
            // 
            // tabProgress
            // 
            this.tabProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.tabProgress.Controls.Add(this.dgvProgress);
            this.tabProgress.Controls.Add(this.pnlProgressHeader);
            this.tabProgress.Location = new System.Drawing.Point(4, 32);
            this.tabProgress.Name = "tabProgress";
            this.tabProgress.Size = new System.Drawing.Size(892, 488);
            this.tabProgress.TabIndex = 2;
            this.tabProgress.Text = "ความคืบหน้า";
            // 
            // dgvProgress
            // 
            this.dgvProgress.AllowUserToResizeRows = false;
            this.dgvProgress.BackgroundColor = System.Drawing.Color.White;
            this.dgvProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProgress.Location = new System.Drawing.Point(0, 44);
            this.dgvProgress.Name = "dgvProgress";
            this.dgvProgress.Size = new System.Drawing.Size(892, 444);
            this.dgvProgress.TabIndex = 0;
            this.dgvProgress.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProgress_CellDoubleClick);
            // 
            // pnlProgressHeader
            // 
            this.pnlProgressHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlProgressHeader.Controls.Add(this.lblOverallPct);
            this.pnlProgressHeader.Controls.Add(this.progressOverall);
            this.pnlProgressHeader.Controls.Add(this.btnUpdateProgress);
            this.pnlProgressHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlProgressHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlProgressHeader.Name = "pnlProgressHeader";
            this.pnlProgressHeader.Padding = new System.Windows.Forms.Padding(5, 7, 5, 0);
            this.pnlProgressHeader.Size = new System.Drawing.Size(892, 44);
            this.pnlProgressHeader.TabIndex = 1;
            // 
            // lblOverallPct
            // 
            this.lblOverallPct.AutoSize = true;
            this.lblOverallPct.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblOverallPct.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblOverallPct.Location = new System.Drawing.Point(5, 11);
            this.lblOverallPct.Name = "lblOverallPct";
            this.lblOverallPct.Size = new System.Drawing.Size(115, 15);
            this.lblOverallPct.TabIndex = 0;
            this.lblOverallPct.Text = "ความคืบหน้ารวม: 0%";
            // 
            // progressOverall
            // 
            this.progressOverall.Location = new System.Drawing.Point(185, 11);
            this.progressOverall.Name = "progressOverall";
            this.progressOverall.Size = new System.Drawing.Size(280, 20);
            this.progressOverall.TabIndex = 1;
            // 
            // btnUpdateProgress
            // 
            this.btnUpdateProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnUpdateProgress.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdateProgress.FlatAppearance.BorderSize = 0;
            this.btnUpdateProgress.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdateProgress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUpdateProgress.ForeColor = System.Drawing.Color.White;
            this.btnUpdateProgress.Location = new System.Drawing.Point(475, 7);
            this.btnUpdateProgress.Name = "btnUpdateProgress";
            this.btnUpdateProgress.Size = new System.Drawing.Size(140, 28);
            this.btnUpdateProgress.TabIndex = 2;
            this.btnUpdateProgress.Text = "อัปเดตความคืบหน้า";
            this.btnUpdateProgress.UseVisualStyleBackColor = false;
            this.btnUpdateProgress.Click += new System.EventHandler(this.btnUpdateProgress_Click);
            // 
            // tabDocs
            // 
            this.tabDocs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.tabDocs.Controls.Add(this.dgvDocWorkflow);
            this.tabDocs.Controls.Add(this.pnlDocToolbar);
            this.tabDocs.Location = new System.Drawing.Point(4, 32);
            this.tabDocs.Name = "tabDocs";
            this.tabDocs.Size = new System.Drawing.Size(892, 488);
            this.tabDocs.TabIndex = 3;
            this.tabDocs.Text = "เอกสาร";
            // 
            // dgvDocWorkflow
            // 
            this.dgvDocWorkflow.AllowUserToResizeRows = false;
            this.dgvDocWorkflow.BackgroundColor = System.Drawing.Color.White;
            this.dgvDocWorkflow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDocWorkflow.Location = new System.Drawing.Point(0, 44);
            this.dgvDocWorkflow.Name = "dgvDocWorkflow";
            this.dgvDocWorkflow.Size = new System.Drawing.Size(892, 444);
            this.dgvDocWorkflow.TabIndex = 0;
            this.dgvDocWorkflow.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDocWorkflow_CellDoubleClick);
            // 
            // pnlDocToolbar
            // 
            this.pnlDocToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlDocToolbar.Controls.Add(this.lblStateFilterL);
            this.pnlDocToolbar.Controls.Add(this.cbbStateFilter);
            this.pnlDocToolbar.Controls.Add(this.btnAddDoc);
            this.pnlDocToolbar.Controls.Add(this.btnEditDoc);
            this.pnlDocToolbar.Controls.Add(this.btnDeleteDoc);
            this.pnlDocToolbar.Controls.Add(this.btnUpdateDocStatus);
            this.pnlDocToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDocToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlDocToolbar.Name = "pnlDocToolbar";
            this.pnlDocToolbar.Padding = new System.Windows.Forms.Padding(5, 7, 5, 0);
            this.pnlDocToolbar.Size = new System.Drawing.Size(892, 44);
            this.pnlDocToolbar.TabIndex = 1;
            // 
            // lblStateFilterL
            // 
            this.lblStateFilterL.AutoSize = true;
            this.lblStateFilterL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStateFilterL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStateFilterL.Location = new System.Drawing.Point(5, 11);
            this.lblStateFilterL.Name = "lblStateFilterL";
            this.lblStateFilterL.Size = new System.Drawing.Size(44, 15);
            this.lblStateFilterL.TabIndex = 0;
            this.lblStateFilterL.Text = "ขั้นตอน:";
            // 
            // cbbStateFilter
            // 
            this.cbbStateFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbStateFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbStateFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbStateFilter.Location = new System.Drawing.Point(60, 7);
            this.cbbStateFilter.Name = "cbbStateFilter";
            this.cbbStateFilter.Size = new System.Drawing.Size(200, 23);
            this.cbbStateFilter.TabIndex = 1;
            this.cbbStateFilter.SelectedIndexChanged += new System.EventHandler(this.cbbStateFilter_SelectedIndexChanged);
            // 
            // btnAddDoc
            // 
            this.btnAddDoc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnAddDoc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddDoc.FlatAppearance.BorderSize = 0;
            this.btnAddDoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddDoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnAddDoc.ForeColor = System.Drawing.Color.White;
            this.btnAddDoc.Location = new System.Drawing.Point(270, 7);
            this.btnAddDoc.Name = "btnAddDoc";
            this.btnAddDoc.Size = new System.Drawing.Size(90, 28);
            this.btnAddDoc.TabIndex = 2;
            this.btnAddDoc.Text = "เพิ่มเอกสาร";
            this.btnAddDoc.UseVisualStyleBackColor = false;
            this.btnAddDoc.Click += new System.EventHandler(this.btnAddDoc_Click);
            // 
            // btnEditDoc
            // 
            this.btnEditDoc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(110)))), ((int)(((byte)(165)))));
            this.btnEditDoc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditDoc.FlatAppearance.BorderSize = 0;
            this.btnEditDoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditDoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnEditDoc.ForeColor = System.Drawing.Color.White;
            this.btnEditDoc.Location = new System.Drawing.Point(370, 7);
            this.btnEditDoc.Name = "btnEditDoc";
            this.btnEditDoc.Size = new System.Drawing.Size(80, 28);
            this.btnEditDoc.TabIndex = 3;
            this.btnEditDoc.Text = "แก้ไข";
            this.btnEditDoc.UseVisualStyleBackColor = false;
            this.btnEditDoc.Click += new System.EventHandler(this.btnEditDoc_Click);
            // 
            // btnDeleteDoc
            // 
            this.btnDeleteDoc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.btnDeleteDoc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteDoc.FlatAppearance.BorderSize = 0;
            this.btnDeleteDoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteDoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDeleteDoc.ForeColor = System.Drawing.Color.White;
            this.btnDeleteDoc.Location = new System.Drawing.Point(460, 7);
            this.btnDeleteDoc.Name = "btnDeleteDoc";
            this.btnDeleteDoc.Size = new System.Drawing.Size(80, 28);
            this.btnDeleteDoc.TabIndex = 4;
            this.btnDeleteDoc.Text = "ลบ";
            this.btnDeleteDoc.UseVisualStyleBackColor = false;
            this.btnDeleteDoc.Click += new System.EventHandler(this.btnDeleteDoc_Click);
            // 
            // btnUpdateDocStatus
            // 
            this.btnUpdateDocStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(110)))), ((int)(((byte)(165)))));
            this.btnUpdateDocStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdateDocStatus.FlatAppearance.BorderSize = 0;
            this.btnUpdateDocStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdateDocStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnUpdateDocStatus.ForeColor = System.Drawing.Color.White;
            this.btnUpdateDocStatus.Location = new System.Drawing.Point(550, 7);
            this.btnUpdateDocStatus.Name = "btnUpdateDocStatus";
            this.btnUpdateDocStatus.Size = new System.Drawing.Size(100, 28);
            this.btnUpdateDocStatus.TabIndex = 5;
            this.btnUpdateDocStatus.Text = "อัปเดตสถานะ";
            this.btnUpdateDocStatus.UseVisualStyleBackColor = false;
            this.btnUpdateDocStatus.Click += new System.EventHandler(this.btnUpdateDocStatus_Click);
            // 
            // tabMembers
            // 
            this.tabMembers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.tabMembers.Controls.Add(this.pnlMemberToolbar);
            this.tabMembers.Controls.Add(this.dgvMembers);
            this.tabMembers.Location = new System.Drawing.Point(4, 32);
            this.tabMembers.Name = "tabMembers";
            this.tabMembers.Size = new System.Drawing.Size(892, 488);
            this.tabMembers.TabIndex = 4;
            this.tabMembers.Text = "แผนกที่เกี่ยวข้อง";
            // 
            // pnlMemberToolbar
            // 
            this.pnlMemberToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlMemberToolbar.Controls.Add(this.btnAddMember);
            this.pnlMemberToolbar.Controls.Add(this.btnDeleteMember);
            this.pnlMemberToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMemberToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlMemberToolbar.Name = "pnlMemberToolbar";
            this.pnlMemberToolbar.Size = new System.Drawing.Size(892, 42);
            this.pnlMemberToolbar.TabIndex = 0;
            // 
            // btnAddMember
            // 
            this.btnAddMember.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnAddMember.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddMember.FlatAppearance.BorderSize = 0;
            this.btnAddMember.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddMember.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddMember.ForeColor = System.Drawing.Color.White;
            this.btnAddMember.Location = new System.Drawing.Point(8, 6);
            this.btnAddMember.Name = "btnAddMember";
            this.btnAddMember.Size = new System.Drawing.Size(90, 30);
            this.btnAddMember.TabIndex = 0;
            this.btnAddMember.Text = "+ เพิ่ม";
            this.btnAddMember.UseVisualStyleBackColor = false;
            this.btnAddMember.Click += new System.EventHandler(this.btnAddMember_Click);
            // 
            // btnDeleteMember
            // 
            this.btnDeleteMember.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.btnDeleteMember.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteMember.FlatAppearance.BorderSize = 0;
            this.btnDeleteMember.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteMember.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDeleteMember.ForeColor = System.Drawing.Color.White;
            this.btnDeleteMember.Location = new System.Drawing.Point(106, 6);
            this.btnDeleteMember.Name = "btnDeleteMember";
            this.btnDeleteMember.Size = new System.Drawing.Size(80, 30);
            this.btnDeleteMember.TabIndex = 1;
            this.btnDeleteMember.Text = "ลบ";
            this.btnDeleteMember.UseVisualStyleBackColor = false;
            this.btnDeleteMember.Click += new System.EventHandler(this.btnDeleteMember_Click);
            // 
            // dgvMembers
            // 
            this.dgvMembers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMembers.Location = new System.Drawing.Point(0, 42);
            this.dgvMembers.Name = "dgvMembers";
            this.dgvMembers.Size = new System.Drawing.Size(892, 451);
            this.dgvMembers.TabIndex = 1;
            // 
            // cbbStatus
            // 
            this.cbbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbStatus.Items.AddRange(new object[] {
            "Open",
            "InProgress",
            "Completed",
            "Cancelled"});
            this.cbbStatus.Location = new System.Drawing.Point(0, 0);
            this.cbbStatus.Name = "cbbStatus";
            this.cbbStatus.Size = new System.Drawing.Size(140, 21);
            this.cbbStatus.TabIndex = 0;
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlBottom.Controls.Add(this.btnSave);
            this.pnlBottom.Controls.Add(this.btnClose);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 590);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(900, 50);
            this.pnlBottom.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(10, 9);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 32);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "บันทึก";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(810, 9);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(80, 32);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "ปิด";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Frm_Project_Detail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(900, 640);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "Frm_Project_Detail";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "รายละเอียดโครงการ";
            this.Load += new System.EventHandler(this.Frm_Project_Detail_Load);
            this.pnlHeader.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabInfo.ResumeLayout(false);
            this.pnlInfo.ResumeLayout(false);
            this.pnlInfo.PerformLayout();
            this.pnlProjectNoRow.ResumeLayout(false);
            this.pnlProjectNoRow.PerformLayout();
            this.pnlAudit.ResumeLayout(false);
            this.pnlAudit.PerformLayout();
            this.tabStates.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStates)).EndInit();
            this.pnlStateToolbar.ResumeLayout(false);
            this.tabProgress.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProgress)).EndInit();
            this.pnlProgressHeader.ResumeLayout(false);
            this.pnlProgressHeader.PerformLayout();
            this.tabDocs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocWorkflow)).EndInit();
            this.pnlDocToolbar.ResumeLayout(false);
            this.pnlDocToolbar.PerformLayout();
            this.tabMembers.ResumeLayout(false);
            this.pnlMemberToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMembers)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        // ─── Field Declarations ───────────────────────────────────────────────
        private System.Windows.Forms.Panel            pnlHeader;
        private System.Windows.Forms.Label            lblTitle;
        private System.Windows.Forms.Label            lblProjectNo;
        private System.Windows.Forms.Label            lb_user;
        private System.Windows.Forms.Label            lb_date;
        private System.Windows.Forms.Panel            pnlAccent;
        private System.Windows.Forms.TabControl       tabMain;
        private System.Windows.Forms.TabPage          tabInfo;
        private System.Windows.Forms.TabPage          tabStates;
        private System.Windows.Forms.TabPage          tabProgress;
        private System.Windows.Forms.TabPage          tabDocs;
        private System.Windows.Forms.TabPage          tabMembers;
        private System.Windows.Forms.DataGridView     dgvMembers;
        private System.Windows.Forms.Panel            pnlMemberToolbar;
        private System.Windows.Forms.Button           btnAddMember;
        private System.Windows.Forms.Button           btnDeleteMember;
        private System.Windows.Forms.TableLayoutPanel pnlInfo;
        private System.Windows.Forms.Label            lblProjNoL;
        private System.Windows.Forms.Panel            pnlProjectNoRow;
        private System.Windows.Forms.TextBox          txtProjectNo;
        private System.Windows.Forms.CheckBox         chkAutoGenProjectNo;
        private System.Windows.Forms.Label            lblNameL;
        private System.Windows.Forms.TextBox          txtProjectName;
        private System.Windows.Forms.Label            lblTmplL;
        private System.Windows.Forms.ComboBox         cbbTemplate;
        private System.Windows.Forms.Label            lblStatusL;
        private System.Windows.Forms.Label            lblStatusDisplay;
        private System.Windows.Forms.ComboBox         cbbStatus;
        private System.Windows.Forms.Label            lblStartL;
        private System.Windows.Forms.DateTimePicker   dtpStartDate;
        private System.Windows.Forms.Label            lblDueL;
        private System.Windows.Forms.DateTimePicker   dtpDueDate;
        private System.Windows.Forms.Label            lblDescL;
        private System.Windows.Forms.TextBox          txtDescription;
        private System.Windows.Forms.Panel            pnlAudit;
        private System.Windows.Forms.Label            lblCrByL;
        private System.Windows.Forms.Label            lblCreateBy;
        private System.Windows.Forms.Label            lblCrDtL;
        private System.Windows.Forms.Label            lblCreateDate;
        private System.Windows.Forms.Label            lblUpByL;
        private System.Windows.Forms.Label            lblUpdateBy;
        private System.Windows.Forms.Label            lblUpDtL;
        private System.Windows.Forms.Label            lblUpdateDate;
        private System.Windows.Forms.Panel            pnlStateToolbar;
        private System.Windows.Forms.Button           btnAddState;
        private System.Windows.Forms.Button           btnEditState;
        private System.Windows.Forms.Button           btnDeleteState;
        private System.Windows.Forms.Button           btnMoveUp;
        private System.Windows.Forms.Button           btnMoveDown;
        private System.Windows.Forms.Button           btnViewTasks;
        private System.Windows.Forms.DataGridView     dgvStates;
        private System.Windows.Forms.Panel            pnlProgressHeader;
        private System.Windows.Forms.Label            lblOverallPct;
        private System.Windows.Forms.ProgressBar      progressOverall;
        private System.Windows.Forms.Button           btnUpdateProgress;
        private System.Windows.Forms.DataGridView     dgvProgress;
        private System.Windows.Forms.Panel            pnlDocToolbar;
        private System.Windows.Forms.Label            lblStateFilterL;
        private System.Windows.Forms.ComboBox         cbbStateFilter;
        private System.Windows.Forms.Button           btnAddDoc;
        private System.Windows.Forms.Button           btnEditDoc;
        private System.Windows.Forms.Button           btnDeleteDoc;
        private System.Windows.Forms.Button           btnUpdateDocStatus;
        private System.Windows.Forms.DataGridView     dgvDocWorkflow;
        private System.Windows.Forms.Panel            pnlBottom;
        private System.Windows.Forms.Button           btnSave;
        private System.Windows.Forms.Button           btnClose;
    }
}
