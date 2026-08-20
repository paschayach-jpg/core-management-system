namespace RG_Project.Project.Forms
{
    partial class Frm_Project_Task_Edit
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitleBar = new System.Windows.Forms.Label();
            this.pnlAccent = new System.Windows.Forms.Panel();
            this.tabComments = new System.Windows.Forms.TabControl();
            this.tabTaskInfo = new System.Windows.Forms.TabPage();
            this.pnlBody = new System.Windows.Forms.TableLayoutPanel();
            this.lblStateL = new System.Windows.Forms.Label();
            this.lblState = new System.Windows.Forms.Label();
            this.lblTitleL = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.lblDescL = new System.Windows.Forms.Label();
            this.txtDesc = new System.Windows.Forms.TextBox();
            this.lblPrioL = new System.Windows.Forms.Label();
            this.cbbPriority = new System.Windows.Forms.ComboBox();
            this.lblAssignL = new System.Windows.Forms.Label();
            this.cbbAssignee = new System.Windows.Forms.ComboBox();
            this.lblDueL = new System.Windows.Forms.Label();
            this.dtpDue = new System.Windows.Forms.DateTimePicker();
            this.lblStartL = new System.Windows.Forms.Label();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.lblHoursL = new System.Windows.Forms.Label();
            this.nudHours = new System.Windows.Forms.NumericUpDown();
            this.lblPctL = new System.Windows.Forms.Label();
            this.nudPct = new System.Windows.Forms.NumericUpDown();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.tabComments.SuspendLayout();
            this.tabTaskInfo.SuspendLayout();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPct)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.lblTitleBar);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(540, 44);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitleBar
            // 
            this.lblTitleBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitleBar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitleBar.ForeColor = System.Drawing.Color.White;
            this.lblTitleBar.Location = new System.Drawing.Point(0, 0);
            this.lblTitleBar.Name = "lblTitleBar";
            this.lblTitleBar.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblTitleBar.Size = new System.Drawing.Size(540, 44);
            this.lblTitleBar.TabIndex = 0;
            this.lblTitleBar.Text = "สร้าง Task ใหม่";
            this.lblTitleBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlAccent
            // 
            this.pnlAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(185)))), ((int)(((byte)(0)))));
            this.pnlAccent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAccent.Location = new System.Drawing.Point(0, 44);
            this.pnlAccent.Name = "pnlAccent";
            this.pnlAccent.Size = new System.Drawing.Size(540, 3);
            this.pnlAccent.TabIndex = 1;
            // 
            // tabComments
            // 
            this.tabComments.Controls.Add(this.tabTaskInfo);
            this.tabComments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabComments.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tabComments.Location = new System.Drawing.Point(0, 47);
            this.tabComments.Name = "tabComments";
            this.tabComments.SelectedIndex = 0;
            this.tabComments.Size = new System.Drawing.Size(540, 483);
            this.tabComments.TabIndex = 10;
            // 
            // tabTaskInfo
            // 
            this.tabTaskInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.tabTaskInfo.Controls.Add(this.pnlBody);
            this.tabTaskInfo.Location = new System.Drawing.Point(4, 24);
            this.tabTaskInfo.Name = "tabTaskInfo";
            this.tabTaskInfo.Size = new System.Drawing.Size(532, 455);
            this.tabTaskInfo.TabIndex = 0;
            this.tabTaskInfo.Text = "📋 ข้อมูล Task";
            // 
            // pnlBody
            // 
            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlBody.ColumnCount = 2;
            this.pnlBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.pnlBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlBody.Controls.Add(this.lblStateL, 0, 0);
            this.pnlBody.Controls.Add(this.lblState, 1, 0);
            this.pnlBody.Controls.Add(this.lblTitleL, 0, 1);
            this.pnlBody.Controls.Add(this.txtTitle, 1, 1);
            this.pnlBody.Controls.Add(this.lblDescL, 0, 2);
            this.pnlBody.Controls.Add(this.txtDesc, 1, 2);
            this.pnlBody.Controls.Add(this.lblPrioL, 0, 3);
            this.pnlBody.Controls.Add(this.cbbPriority, 1, 3);
            this.pnlBody.Controls.Add(this.lblAssignL, 0, 4);
            this.pnlBody.Controls.Add(this.cbbAssignee, 1, 4);
            this.pnlBody.Controls.Add(this.lblDueL, 0, 5);
            this.pnlBody.Controls.Add(this.dtpDue, 1, 5);
            this.pnlBody.Controls.Add(this.lblStartL, 0, 6);
            this.pnlBody.Controls.Add(this.dtpStart, 1, 6);
            this.pnlBody.Controls.Add(this.lblHoursL, 0, 7);
            this.pnlBody.Controls.Add(this.nudHours, 1, 7);
            this.pnlBody.Controls.Add(this.lblPctL, 0, 8);
            this.pnlBody.Controls.Add(this.nudPct, 1, 8);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 0);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(10, 8, 10, 0);
            this.pnlBody.RowCount = 9;
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.pnlBody.Size = new System.Drawing.Size(532, 455);
            this.pnlBody.TabIndex = 2;
            // 
            // lblStateL
            // 
            this.lblStateL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStateL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStateL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStateL.Location = new System.Drawing.Point(13, 8);
            this.lblStateL.Name = "lblStateL";
            this.lblStateL.Size = new System.Drawing.Size(114, 34);
            this.lblStateL.TabIndex = 0;
            this.lblStateL.Text = "ขั้นตอน:";
            this.lblStateL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblState
            // 
            this.lblState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblState.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblState.Location = new System.Drawing.Point(133, 8);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(386, 34);
            this.lblState.TabIndex = 1;
            this.lblState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTitleL
            // 
            this.lblTitleL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitleL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTitleL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblTitleL.Location = new System.Drawing.Point(13, 42);
            this.lblTitleL.Name = "lblTitleL";
            this.lblTitleL.Size = new System.Drawing.Size(114, 34);
            this.lblTitleL.TabIndex = 2;
            this.lblTitleL.Text = "หัวข้อ *:";
            this.lblTitleL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTitle
            // 
            this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTitle.Location = new System.Drawing.Point(133, 45);
            this.txtTitle.MaxLength = 300;
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(386, 23);
            this.txtTitle.TabIndex = 3;
            // 
            // lblDescL
            // 
            this.lblDescL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDescL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblDescL.Location = new System.Drawing.Point(13, 76);
            this.lblDescL.Name = "lblDescL";
            this.lblDescL.Size = new System.Drawing.Size(114, 80);
            this.lblDescL.TabIndex = 4;
            this.lblDescL.Text = "รายละเอียด:";
            this.lblDescL.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtDesc
            // 
            this.txtDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDesc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDesc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDesc.Location = new System.Drawing.Point(133, 79);
            this.txtDesc.MaxLength = 2000;
            this.txtDesc.Multiline = true;
            this.txtDesc.Name = "txtDesc";
            this.txtDesc.Size = new System.Drawing.Size(386, 74);
            this.txtDesc.TabIndex = 5;
            // 
            // lblPrioL
            // 
            this.lblPrioL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPrioL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPrioL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblPrioL.Location = new System.Drawing.Point(13, 156);
            this.lblPrioL.Name = "lblPrioL";
            this.lblPrioL.Size = new System.Drawing.Size(114, 34);
            this.lblPrioL.TabIndex = 6;
            this.lblPrioL.Text = "ความสำคัญ:";
            this.lblPrioL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbPriority
            // 
            this.cbbPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbPriority.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbPriority.Items.AddRange(new object[] {
            "🟢 Low",
            "🟡 Medium",
            "🟠 High",
            "🔴 Critical"});
            this.cbbPriority.Location = new System.Drawing.Point(133, 159);
            this.cbbPriority.Name = "cbbPriority";
            this.cbbPriority.Size = new System.Drawing.Size(150, 23);
            this.cbbPriority.TabIndex = 3;
            // 
            // lblAssignL
            // 
            this.lblAssignL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAssignL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAssignL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblAssignL.Location = new System.Drawing.Point(13, 190);
            this.lblAssignL.Name = "lblAssignL";
            this.lblAssignL.Size = new System.Drawing.Size(114, 34);
            this.lblAssignL.TabIndex = 7;
            this.lblAssignL.Text = "ผู้รับผิดชอบ:";
            this.lblAssignL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbAssignee
            // 
            this.cbbAssignee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbAssignee.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbAssignee.Location = new System.Drawing.Point(133, 193);
            this.cbbAssignee.Name = "cbbAssignee";
            this.cbbAssignee.Size = new System.Drawing.Size(230, 23);
            this.cbbAssignee.TabIndex = 4;
            // 
            // lblDueL
            // 
            this.lblDueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDueL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDueL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblDueL.Location = new System.Drawing.Point(13, 224);
            this.lblDueL.Name = "lblDueL";
            this.lblDueL.Size = new System.Drawing.Size(114, 34);
            this.lblDueL.TabIndex = 8;
            this.lblDueL.Text = "วันกำหนดเสร็จ:";
            this.lblDueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dtpDue
            // 
            this.dtpDue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpDue.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDue.Location = new System.Drawing.Point(133, 227);
            this.dtpDue.Name = "dtpDue";
            this.dtpDue.ShowCheckBox = true;
            this.dtpDue.Size = new System.Drawing.Size(140, 23);
            this.dtpDue.TabIndex = 5;
            // 
            // lblStartL
            // 
            this.lblStartL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStartL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStartL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStartL.Location = new System.Drawing.Point(13, 258);
            this.lblStartL.Name = "lblStartL";
            this.lblStartL.Size = new System.Drawing.Size(114, 34);
            this.lblStartL.TabIndex = 9;
            this.lblStartL.Text = "วันเริ่มต้น:";
            this.lblStartL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dtpStart
            // 
            this.dtpStart.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpStart.Location = new System.Drawing.Point(133, 261);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.ShowCheckBox = true;
            this.dtpStart.Size = new System.Drawing.Size(140, 23);
            this.dtpStart.TabIndex = 6;
            // 
            // lblHoursL
            // 
            this.lblHoursL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHoursL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHoursL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblHoursL.Location = new System.Drawing.Point(13, 292);
            this.lblHoursL.Name = "lblHoursL";
            this.lblHoursL.Size = new System.Drawing.Size(114, 34);
            this.lblHoursL.TabIndex = 10;
            this.lblHoursL.Text = "ชั่วโมง (คาด):";
            this.lblHoursL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudHours
            // 
            this.nudHours.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.nudHours.Location = new System.Drawing.Point(133, 295);
            this.nudHours.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudHours.Name = "nudHours";
            this.nudHours.Size = new System.Drawing.Size(80, 23);
            this.nudHours.TabIndex = 7;
            // 
            // lblPctL
            // 
            this.lblPctL.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPctL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPctL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblPctL.Location = new System.Drawing.Point(13, 326);
            this.lblPctL.Name = "lblPctL";
            this.lblPctL.Size = new System.Drawing.Size(114, 34);
            this.lblPctL.TabIndex = 11;
            this.lblPctL.Text = "% เสร็จ:";
            this.lblPctL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudPct
            // 
            this.nudPct.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.nudPct.Location = new System.Drawing.Point(133, 329);
            this.nudPct.Name = "nudPct";
            this.nudPct.Size = new System.Drawing.Size(80, 23);
            this.nudPct.TabIndex = 8;
            // 
            // pnlButtons
            // 
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(0, 530);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(8, 10, 8, 0);
            this.pnlButtons.Size = new System.Drawing.Size(540, 50);
            this.pnlButtons.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(441, 13);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 1;
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
            this.btnSave.Location = new System.Drawing.Point(355, 13);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "บันทึก";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Frm_Project_Task_Edit
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(540, 580);
            this.Controls.Add(this.tabComments);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Frm_Project_Task_Edit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Task";
            this.Load += new System.EventHandler(this.Frm_Project_Task_Edit_Load);
            this.pnlHeader.ResumeLayout(false);
            this.tabComments.ResumeLayout(false);
            this.tabTaskInfo.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPct)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel             pnlHeader;
        private System.Windows.Forms.Label             lblTitleBar;
        private System.Windows.Forms.Panel             pnlAccent;
        private System.Windows.Forms.TabControl        tabComments;
        private System.Windows.Forms.TabPage           tabTaskInfo;
        private System.Windows.Forms.TableLayoutPanel  pnlBody;
        private System.Windows.Forms.Label             lblStateL;
        private System.Windows.Forms.Label             lblState;
        private System.Windows.Forms.Label             lblTitleL;
        private System.Windows.Forms.TextBox           txtTitle;
        private System.Windows.Forms.Label             lblDescL;
        private System.Windows.Forms.TextBox           txtDesc;
        private System.Windows.Forms.Label             lblPrioL;
        private System.Windows.Forms.ComboBox          cbbPriority;
        private System.Windows.Forms.Label             lblAssignL;
        private System.Windows.Forms.ComboBox          cbbAssignee;
        private System.Windows.Forms.Label             lblDueL;
        private System.Windows.Forms.DateTimePicker    dtpDue;
        private System.Windows.Forms.Label             lblStartL;
        private System.Windows.Forms.DateTimePicker    dtpStart;
        private System.Windows.Forms.Label             lblHoursL;
        private System.Windows.Forms.NumericUpDown     nudHours;
        private System.Windows.Forms.Label             lblPctL;
        private System.Windows.Forms.NumericUpDown     nudPct;
        private System.Windows.Forms.FlowLayoutPanel   pnlButtons;
        private System.Windows.Forms.Button            btnSave;
        private System.Windows.Forms.Button            btnCancel;
    }
}
