namespace CMS.Presentation.Modules.RGP
{
    partial class Frm_Project_State_Edit
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
            this.pnlAccent = new System.Windows.Forms.Panel();
            this.pnlBody = new System.Windows.Forms.TableLayoutPanel();
            this.lblStateName = new System.Windows.Forms.Label();
            this.txtStateName = new System.Windows.Forms.TextBox();
            this.lblSeqNo = new System.Windows.Forms.Label();
            this.nudSeqNo = new System.Windows.Forms.NumericUpDown();
            this.lblDesc = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblRequired = new System.Windows.Forms.Label();
            this.chkRequired = new System.Windows.Forms.CheckBox();
            this.lblEstDays = new System.Windows.Forms.Label();
            this.nudEstDays = new System.Windows.Forms.NumericUpDown();
            this.lblRefProject = new System.Windows.Forms.Label();
            this.pnlRefProject = new System.Windows.Forms.Panel();
            this.txtRefProject = new System.Windows.Forms.TextBox();
            this.btnSelectRef = new System.Windows.Forms.Button();
            this.btnClearRef = new System.Windows.Forms.Button();
            this.btnNewRefProject = new System.Windows.Forms.Button();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlMembers = new System.Windows.Forms.Panel();
            this.lblMembersTitle = new System.Windows.Forms.Label();
            this.pnlMemberToolbar = new System.Windows.Forms.Panel();
            this.btnAddStateMember = new System.Windows.Forms.Button();
            this.btnDeleteStateMember = new System.Windows.Forms.Button();
            this.btnViewTasks = new System.Windows.Forms.Button();
            this.dgvStateMembers = new System.Windows.Forms.DataGridView();
            this.pnlHeader.SuspendLayout();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSeqNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEstDays)).BeginInit();
            this.pnlRefProject.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlMembers.SuspendLayout();
            this.pnlMemberToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStateMembers)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(550, 48);
            this.pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(550, 48);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "เพิ่มขั้นตอน";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlAccent
            // 
            this.pnlAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(185)))), ((int)(((byte)(0)))));
            this.pnlAccent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAccent.Location = new System.Drawing.Point(0, 48);
            this.pnlAccent.Name = "pnlAccent";
            this.pnlAccent.Size = new System.Drawing.Size(550, 3);
            this.pnlAccent.TabIndex = 3;
            // 
            // pnlBody
            // 
            this.pnlBody.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlBody.ColumnCount = 2;
            this.pnlBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.pnlBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlBody.Controls.Add(this.lblStateName, 0, 0);
            this.pnlBody.Controls.Add(this.txtStateName, 1, 0);
            this.pnlBody.Controls.Add(this.lblSeqNo, 0, 1);
            this.pnlBody.Controls.Add(this.nudSeqNo, 1, 1);
            this.pnlBody.Controls.Add(this.lblDesc, 0, 2);
            this.pnlBody.Controls.Add(this.txtDescription, 1, 2);
            this.pnlBody.Controls.Add(this.lblRequired, 0, 3);
            this.pnlBody.Controls.Add(this.chkRequired, 1, 3);
            this.pnlBody.Controls.Add(this.lblEstDays, 0, 4);
            this.pnlBody.Controls.Add(this.nudEstDays, 1, 4);
            this.pnlBody.Controls.Add(this.lblRefProject, 0, 5);
            this.pnlBody.Controls.Add(this.pnlRefProject, 1, 5);
            this.pnlBody.Location = new System.Drawing.Point(0, 51);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.pnlBody.RowCount = 6;
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.pnlBody.Size = new System.Drawing.Size(550, 278);
            this.pnlBody.TabIndex = 0;
            // 
            // lblStateName
            // 
            this.lblStateName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStateName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStateName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStateName.Location = new System.Drawing.Point(13, 10);
            this.lblStateName.Name = "lblStateName";
            this.lblStateName.Size = new System.Drawing.Size(124, 35);
            this.lblStateName.TabIndex = 0;
            this.lblStateName.Text = "ชื่อขั้นตอน *:";
            this.lblStateName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtStateName
            // 
            this.txtStateName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStateName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStateName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtStateName.Location = new System.Drawing.Point(143, 13);
            this.txtStateName.MaxLength = 200;
            this.txtStateName.Name = "txtStateName";
            this.txtStateName.Size = new System.Drawing.Size(394, 23);
            this.txtStateName.TabIndex = 1;
            // 
            // lblSeqNo
            // 
            this.lblSeqNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSeqNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSeqNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblSeqNo.Location = new System.Drawing.Point(13, 45);
            this.lblSeqNo.Name = "lblSeqNo";
            this.lblSeqNo.Size = new System.Drawing.Size(124, 35);
            this.lblSeqNo.TabIndex = 2;
            this.lblSeqNo.Text = "ลำดับ:";
            this.lblSeqNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudSeqNo
            // 
            this.nudSeqNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.nudSeqNo.Location = new System.Drawing.Point(143, 48);
            this.nudSeqNo.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.nudSeqNo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSeqNo.Name = "nudSeqNo";
            this.nudSeqNo.Size = new System.Drawing.Size(80, 23);
            this.nudSeqNo.TabIndex = 3;
            this.nudSeqNo.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblDesc
            // 
            this.lblDesc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDesc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblDesc.Location = new System.Drawing.Point(13, 80);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(124, 80);
            this.lblDesc.TabIndex = 4;
            this.lblDesc.Text = "รายละเอียด:";
            this.lblDesc.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtDescription
            // 
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDescription.Location = new System.Drawing.Point(143, 83);
            this.txtDescription.MaxLength = 500;
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(394, 74);
            this.txtDescription.TabIndex = 5;
            // 
            // lblRequired
            // 
            this.lblRequired.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRequired.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRequired.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblRequired.Location = new System.Drawing.Point(13, 160);
            this.lblRequired.Name = "lblRequired";
            this.lblRequired.Size = new System.Drawing.Size(124, 35);
            this.lblRequired.TabIndex = 6;
            this.lblRequired.Text = "จำเป็น:";
            this.lblRequired.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkRequired
            // 
            this.chkRequired.Checked = true;
            this.chkRequired.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRequired.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRequired.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRequired.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.chkRequired.Location = new System.Drawing.Point(143, 163);
            this.chkRequired.Name = "chkRequired";
            this.chkRequired.Size = new System.Drawing.Size(394, 29);
            this.chkRequired.TabIndex = 7;
            this.chkRequired.Text = "กำหนดให้เป็นขั้นตอนที่จำเป็น (Required)";
            // 
            // lblEstDays
            // 
            this.lblEstDays.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEstDays.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstDays.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblEstDays.Location = new System.Drawing.Point(13, 195);
            this.lblEstDays.Name = "lblEstDays";
            this.lblEstDays.Size = new System.Drawing.Size(124, 35);
            this.lblEstDays.TabIndex = 8;
            this.lblEstDays.Text = "จำนวนวัน (คาดการณ์):";
            this.lblEstDays.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudEstDays
            // 
            this.nudEstDays.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.nudEstDays.Location = new System.Drawing.Point(143, 198);
            this.nudEstDays.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudEstDays.Name = "nudEstDays";
            this.nudEstDays.Size = new System.Drawing.Size(100, 23);
            this.nudEstDays.TabIndex = 8;
            // 
            // lblRefProject
            // 
            this.lblRefProject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRefProject.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRefProject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblRefProject.Location = new System.Drawing.Point(13, 230);
            this.lblRefProject.Name = "lblRefProject";
            this.lblRefProject.Size = new System.Drawing.Size(124, 48);
            this.lblRefProject.TabIndex = 9;
            this.lblRefProject.Text = "โครงการที่ ref:";
            this.lblRefProject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlRefProject
            // 
            this.pnlRefProject.Controls.Add(this.txtRefProject);
            this.pnlRefProject.Controls.Add(this.btnSelectRef);
            this.pnlRefProject.Controls.Add(this.btnClearRef);
            this.pnlRefProject.Controls.Add(this.btnNewRefProject);
            this.pnlRefProject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRefProject.Location = new System.Drawing.Point(143, 233);
            this.pnlRefProject.Name = "pnlRefProject";
            this.pnlRefProject.Size = new System.Drawing.Size(394, 42);
            this.pnlRefProject.TabIndex = 10;
            // 
            // txtRefProject
            // 
            this.txtRefProject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRefProject.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRefProject.Location = new System.Drawing.Point(0, 7);
            this.txtRefProject.Name = "txtRefProject";
            this.txtRefProject.ReadOnly = true;
            this.txtRefProject.Size = new System.Drawing.Size(175, 23);
            this.txtRefProject.TabIndex = 0;
            this.txtRefProject.Text = "(ไม่ได้ระบุ)";
            // 
            // btnSelectRef
            // 
            this.btnSelectRef.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnSelectRef.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelectRef.FlatAppearance.BorderSize = 0;
            this.btnSelectRef.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectRef.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSelectRef.ForeColor = System.Drawing.Color.White;
            this.btnSelectRef.Location = new System.Drawing.Point(180, 6);
            this.btnSelectRef.Name = "btnSelectRef";
            this.btnSelectRef.Size = new System.Drawing.Size(70, 25);
            this.btnSelectRef.TabIndex = 1;
            this.btnSelectRef.Text = "เลือก";
            this.btnSelectRef.UseVisualStyleBackColor = false;
            this.btnSelectRef.Click += new System.EventHandler(this.btnSelectRef_Click);
            // 
            // btnClearRef
            // 
            this.btnClearRef.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.btnClearRef.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearRef.FlatAppearance.BorderSize = 0;
            this.btnClearRef.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearRef.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClearRef.ForeColor = System.Drawing.Color.White;
            this.btnClearRef.Location = new System.Drawing.Point(255, 6);
            this.btnClearRef.Name = "btnClearRef";
            this.btnClearRef.Size = new System.Drawing.Size(55, 25);
            this.btnClearRef.TabIndex = 2;
            this.btnClearRef.Text = "ล้าง";
            this.btnClearRef.UseVisualStyleBackColor = false;
            this.btnClearRef.Visible = false;
            this.btnClearRef.Click += new System.EventHandler(this.btnClearRef_Click);
            // 
            // btnNewRefProject
            // 
            this.btnNewRefProject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(110)))), ((int)(((byte)(165)))));
            this.btnNewRefProject.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewRefProject.FlatAppearance.BorderSize = 0;
            this.btnNewRefProject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewRefProject.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnNewRefProject.ForeColor = System.Drawing.Color.White;
            this.btnNewRefProject.Location = new System.Drawing.Point(315, 6);
            this.btnNewRefProject.Name = "btnNewRefProject";
            this.btnNewRefProject.Size = new System.Drawing.Size(75, 25);
            this.btnNewRefProject.TabIndex = 3;
            this.btnNewRefProject.Text = "+ สร้างใหม่";
            this.btnNewRefProject.UseVisualStyleBackColor = false;
            this.btnNewRefProject.Click += new System.EventHandler(this.btnNewRefProject_Click);
            // 
            // pnlButtons
            // 
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(0, 635);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(8, 10, 8, 0);
            this.pnlButtons.Size = new System.Drawing.Size(550, 53);
            this.pnlButtons.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(451, 13);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(365, 13);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "บันทึก";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // pnlMembers
            // 
            this.pnlMembers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlMembers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlMembers.Controls.Add(this.lblMembersTitle);
            this.pnlMembers.Controls.Add(this.pnlMemberToolbar);
            this.pnlMembers.Controls.Add(this.dgvStateMembers);
            this.pnlMembers.Location = new System.Drawing.Point(0, 333);
            this.pnlMembers.Name = "pnlMembers";
            this.pnlMembers.Size = new System.Drawing.Size(550, 300);
            this.pnlMembers.TabIndex = 10;
            // 
            // lblMembersTitle
            // 
            this.lblMembersTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblMembersTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMembersTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMembersTitle.ForeColor = System.Drawing.Color.White;
            this.lblMembersTitle.Location = new System.Drawing.Point(0, 32);
            this.lblMembersTitle.Name = "lblMembersTitle";
            this.lblMembersTitle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblMembersTitle.Size = new System.Drawing.Size(550, 24);
            this.lblMembersTitle.TabIndex = 0;
            this.lblMembersTitle.Text = "บุคคลที่เกี่ยวข้องกับขั้นตอน";
            this.lblMembersTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlMemberToolbar
            // 
            this.pnlMemberToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlMemberToolbar.Controls.Add(this.btnAddStateMember);
            this.pnlMemberToolbar.Controls.Add(this.btnDeleteStateMember);
            this.pnlMemberToolbar.Controls.Add(this.btnViewTasks);
            this.pnlMemberToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMemberToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlMemberToolbar.Name = "pnlMemberToolbar";
            this.pnlMemberToolbar.Size = new System.Drawing.Size(550, 32);
            this.pnlMemberToolbar.TabIndex = 0;
            // 
            // btnAddStateMember
            // 
            this.btnAddStateMember.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnAddStateMember.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddStateMember.FlatAppearance.BorderSize = 0;
            this.btnAddStateMember.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddStateMember.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddStateMember.ForeColor = System.Drawing.Color.White;
            this.btnAddStateMember.Location = new System.Drawing.Point(4, 2);
            this.btnAddStateMember.Name = "btnAddStateMember";
            this.btnAddStateMember.Size = new System.Drawing.Size(80, 26);
            this.btnAddStateMember.TabIndex = 0;
            this.btnAddStateMember.Text = "+ เพิ่ม";
            this.btnAddStateMember.UseVisualStyleBackColor = false;
            this.btnAddStateMember.Click += new System.EventHandler(this.btnAddStateMember_Click);
            // 
            // btnDeleteStateMember
            // 
            this.btnDeleteStateMember.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.btnDeleteStateMember.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteStateMember.FlatAppearance.BorderSize = 0;
            this.btnDeleteStateMember.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteStateMember.ForeColor = System.Drawing.Color.White;
            this.btnDeleteStateMember.Location = new System.Drawing.Point(92, 2);
            this.btnDeleteStateMember.Name = "btnDeleteStateMember";
            this.btnDeleteStateMember.Size = new System.Drawing.Size(70, 26);
            this.btnDeleteStateMember.TabIndex = 1;
            this.btnDeleteStateMember.Text = "ลบ";
            this.btnDeleteStateMember.UseVisualStyleBackColor = false;
            this.btnDeleteStateMember.Click += new System.EventHandler(this.btnDeleteStateMember_Click);
            // 
            // btnViewTasks
            // 
            this.btnViewTasks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnViewTasks.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewTasks.FlatAppearance.BorderSize = 0;
            this.btnViewTasks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewTasks.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnViewTasks.ForeColor = System.Drawing.Color.White;
            this.btnViewTasks.Location = new System.Drawing.Point(170, 2);
            this.btnViewTasks.Name = "btnViewTasks";
            this.btnViewTasks.Size = new System.Drawing.Size(100, 26);
            this.btnViewTasks.TabIndex = 2;
            this.btnViewTasks.Text = "📋 Tasks";
            this.btnViewTasks.UseVisualStyleBackColor = false;
            this.btnViewTasks.Visible = false;
            this.btnViewTasks.Click += new System.EventHandler(this.btnViewTasks_Click);
            // 
            // dgvStateMembers
            // 
            this.dgvStateMembers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvStateMembers.Location = new System.Drawing.Point(0, 56);
            this.dgvStateMembers.Name = "dgvStateMembers";
            this.dgvStateMembers.Size = new System.Drawing.Size(550, 244);
            this.dgvStateMembers.TabIndex = 1;
            // 
            // Frm_Project_State_Edit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(550, 688);
            this.Controls.Add(this.pnlMembers);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Frm_Project_State_Edit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "เพิ่มขั้นตอน";
            this.Load += new System.EventHandler(this.Frm_Project_State_Edit_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSeqNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEstDays)).EndInit();
            this.pnlRefProject.ResumeLayout(false);
            this.pnlRefProject.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlMembers.ResumeLayout(false);
            this.pnlMemberToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStateMembers)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel             pnlHeader;
        private System.Windows.Forms.Label             lblTitle;
        private System.Windows.Forms.Panel             pnlAccent;
        private System.Windows.Forms.TableLayoutPanel  pnlBody;
        private System.Windows.Forms.Label             lblStateName;
        private System.Windows.Forms.TextBox           txtStateName;
        private System.Windows.Forms.Label             lblSeqNo;
        private System.Windows.Forms.NumericUpDown     nudSeqNo;
        private System.Windows.Forms.Label             lblDesc;
        private System.Windows.Forms.TextBox           txtDescription;
        private System.Windows.Forms.Label             lblRequired;
        private System.Windows.Forms.CheckBox          chkRequired;
        private System.Windows.Forms.Label             lblEstDays;
        private System.Windows.Forms.NumericUpDown     nudEstDays;
        private System.Windows.Forms.Label             lblRefProject;
        private System.Windows.Forms.Panel             pnlRefProject;
        private System.Windows.Forms.TextBox           txtRefProject;
        private System.Windows.Forms.Button            btnSelectRef;
        private System.Windows.Forms.Button            btnClearRef;
        private System.Windows.Forms.Button            btnNewRefProject;
        private System.Windows.Forms.FlowLayoutPanel   pnlButtons;
        private System.Windows.Forms.Button            btnSave;
        private System.Windows.Forms.Panel             pnlMembers;
        private System.Windows.Forms.Label             lblMembersTitle;
        private System.Windows.Forms.Panel             pnlMemberToolbar;
        private System.Windows.Forms.Button            btnAddStateMember;
        private System.Windows.Forms.Button            btnDeleteStateMember;
        private System.Windows.Forms.Button            btnViewTasks;
        private System.Windows.Forms.DataGridView      dgvStateMembers;
        private System.Windows.Forms.Button            btnCancel;
    }
}
