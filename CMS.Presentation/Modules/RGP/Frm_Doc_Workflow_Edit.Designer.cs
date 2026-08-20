namespace CMS.Presentation.Modules.RGP
{
    partial class Frm_Doc_Workflow_Edit
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
            this.lblStateL = new System.Windows.Forms.Label();
            this.cbbState = new System.Windows.Forms.ComboBox();
            this.lblDocNameL = new System.Windows.Forms.Label();
            this.txtDocName = new System.Windows.Forms.TextBox();
            this.lblDocTypeL = new System.Windows.Forms.Label();
            this.txtDocType = new System.Windows.Forms.TextBox();
            this.lblReqL = new System.Windows.Forms.Label();
            this.chkRequired = new System.Windows.Forms.CheckBox();
            this.lblStatusL = new System.Windows.Forms.Label();
            this.cbbDocStatus = new System.Windows.Forms.ComboBox();
            this.lblSeqNoL = new System.Windows.Forms.Label();
            this.nudSeqNo = new System.Windows.Forms.NumericUpDown();
            this.lblRemarkL = new System.Windows.Forms.Label();
            this.txtRemark = new System.Windows.Forms.TextBox();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSeqNo)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(500, 48);
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
            this.lblTitle.Size = new System.Drawing.Size(500, 48);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "เพิ่มเอกสาร";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlAccent
            // 
            this.pnlAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(185)))), ((int)(((byte)(0)))));
            this.pnlAccent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAccent.Location = new System.Drawing.Point(0, 48);
            this.pnlAccent.Name = "pnlAccent";
            this.pnlAccent.Size = new System.Drawing.Size(500, 3);
            this.pnlAccent.TabIndex = 3;
            // 
            // pnlBody
            // 
            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlBody.ColumnCount = 2;
            this.pnlBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.pnlBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlBody.Controls.Add(this.lblStateL, 0, 0);
            this.pnlBody.Controls.Add(this.cbbState, 1, 0);
            this.pnlBody.Controls.Add(this.lblDocNameL, 0, 1);
            this.pnlBody.Controls.Add(this.txtDocName, 1, 1);
            this.pnlBody.Controls.Add(this.lblDocTypeL, 0, 2);
            this.pnlBody.Controls.Add(this.txtDocType, 1, 2);
            this.pnlBody.Controls.Add(this.lblReqL, 0, 3);
            this.pnlBody.Controls.Add(this.chkRequired, 1, 3);
            this.pnlBody.Controls.Add(this.lblStatusL, 0, 4);
            this.pnlBody.Controls.Add(this.cbbDocStatus, 1, 4);
            this.pnlBody.Controls.Add(this.lblSeqNoL, 0, 5);
            this.pnlBody.Controls.Add(this.nudSeqNo, 1, 5);
            this.pnlBody.Controls.Add(this.lblRemarkL, 0, 6);
            this.pnlBody.Controls.Add(this.txtRemark, 1, 6);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 51);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.pnlBody.RowCount = 7;
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.pnlBody.Size = new System.Drawing.Size(500, 305);
            this.pnlBody.TabIndex = 0;
            // 
            // lblStateL
            // 
            this.lblStateL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStateL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStateL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStateL.Location = new System.Drawing.Point(13, 10);
            this.lblStateL.Name = "lblStateL";
            this.lblStateL.Size = new System.Drawing.Size(124, 35);
            this.lblStateL.TabIndex = 0;
            this.lblStateL.Text = "ขั้นตอน *:";
            this.lblStateL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbState
            // 
            this.cbbState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbbState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbState.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbState.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbState.Location = new System.Drawing.Point(143, 13);
            this.cbbState.Name = "cbbState";
            this.cbbState.Size = new System.Drawing.Size(344, 23);
            this.cbbState.TabIndex = 1;
            // 
            // lblDocNameL
            // 
            this.lblDocNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDocNameL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDocNameL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblDocNameL.Location = new System.Drawing.Point(13, 45);
            this.lblDocNameL.Name = "lblDocNameL";
            this.lblDocNameL.Size = new System.Drawing.Size(124, 35);
            this.lblDocNameL.TabIndex = 2;
            this.lblDocNameL.Text = "ชื่อเอกสาร *:";
            this.lblDocNameL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDocName
            // 
            this.txtDocName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDocName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDocName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDocName.Location = new System.Drawing.Point(143, 48);
            this.txtDocName.MaxLength = 300;
            this.txtDocName.Name = "txtDocName";
            this.txtDocName.Size = new System.Drawing.Size(344, 23);
            this.txtDocName.TabIndex = 3;
            // 
            // lblDocTypeL
            // 
            this.lblDocTypeL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDocTypeL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDocTypeL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblDocTypeL.Location = new System.Drawing.Point(13, 80);
            this.lblDocTypeL.Name = "lblDocTypeL";
            this.lblDocTypeL.Size = new System.Drawing.Size(124, 35);
            this.lblDocTypeL.TabIndex = 4;
            this.lblDocTypeL.Text = "ประเภทเอกสาร:";
            this.lblDocTypeL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDocType
            // 
            this.txtDocType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDocType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDocType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDocType.Location = new System.Drawing.Point(143, 83);
            this.txtDocType.MaxLength = 50;
            this.txtDocType.Name = "txtDocType";
            this.txtDocType.Size = new System.Drawing.Size(344, 23);
            this.txtDocType.TabIndex = 5;
            // 
            // lblReqL
            // 
            this.lblReqL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblReqL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblReqL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblReqL.Location = new System.Drawing.Point(13, 115);
            this.lblReqL.Name = "lblReqL";
            this.lblReqL.Size = new System.Drawing.Size(124, 35);
            this.lblReqL.TabIndex = 6;
            this.lblReqL.Text = "จำเป็น:";
            this.lblReqL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkRequired
            // 
            this.chkRequired.Checked = true;
            this.chkRequired.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRequired.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRequired.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRequired.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.chkRequired.Location = new System.Drawing.Point(143, 118);
            this.chkRequired.Name = "chkRequired";
            this.chkRequired.Size = new System.Drawing.Size(344, 29);
            this.chkRequired.TabIndex = 7;
            this.chkRequired.Text = "กำหนดให้เป็นเอกสารที่จำเป็น";
            // 
            // lblStatusL
            // 
            this.lblStatusL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStatusL.Location = new System.Drawing.Point(13, 150);
            this.lblStatusL.Name = "lblStatusL";
            this.lblStatusL.Size = new System.Drawing.Size(124, 35);
            this.lblStatusL.TabIndex = 8;
            this.lblStatusL.Text = "สถานะ:";
            this.lblStatusL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbDocStatus
            // 
            this.cbbDocStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbbDocStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbDocStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbDocStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbDocStatus.Items.AddRange(new object[] {
            "Pending",
            "Received",
            "Verified",
            "Rejected"});
            this.cbbDocStatus.Location = new System.Drawing.Point(143, 153);
            this.cbbDocStatus.Name = "cbbDocStatus";
            this.cbbDocStatus.Size = new System.Drawing.Size(344, 23);
            this.cbbDocStatus.TabIndex = 9;
            // 
            // lblSeqNoL
            // 
            this.lblSeqNoL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSeqNoL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSeqNoL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblSeqNoL.Location = new System.Drawing.Point(13, 185);
            this.lblSeqNoL.Name = "lblSeqNoL";
            this.lblSeqNoL.Size = new System.Drawing.Size(124, 35);
            this.lblSeqNoL.TabIndex = 10;
            this.lblSeqNoL.Text = "ลำดับ:";
            this.lblSeqNoL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudSeqNo
            // 
            this.nudSeqNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.nudSeqNo.Location = new System.Drawing.Point(143, 188);
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
            this.nudSeqNo.TabIndex = 11;
            this.nudSeqNo.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblRemarkL
            // 
            this.lblRemarkL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRemarkL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRemarkL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblRemarkL.Location = new System.Drawing.Point(13, 220);
            this.lblRemarkL.Name = "lblRemarkL";
            this.lblRemarkL.Size = new System.Drawing.Size(124, 85);
            this.lblRemarkL.TabIndex = 12;
            this.lblRemarkL.Text = "หมายเหตุ:";
            this.lblRemarkL.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtRemark
            // 
            this.txtRemark.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRemark.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRemark.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRemark.Location = new System.Drawing.Point(143, 223);
            this.txtRemark.MaxLength = 500;
            this.txtRemark.Multiline = true;
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(344, 79);
            this.txtRemark.TabIndex = 13;
            // 
            // pnlButtons
            // 
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(0, 356);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(8, 10, 8, 0);
            this.pnlButtons.Size = new System.Drawing.Size(500, 50);
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
            this.btnCancel.Location = new System.Drawing.Point(401, 13);
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
            this.btnSave.Location = new System.Drawing.Point(305, 13);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 30);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "บันทึก";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Frm_Doc_Workflow_Edit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(500, 406);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Frm_Doc_Workflow_Edit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "เพิ่มเอกสาร";
            this.Load += new System.EventHandler(this.Frm_Doc_Workflow_Edit_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSeqNo)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel            pnlHeader;
        private System.Windows.Forms.Label            lblTitle;
        private System.Windows.Forms.Panel            pnlAccent;
        private System.Windows.Forms.TableLayoutPanel pnlBody;
        private System.Windows.Forms.Label            lblStateL;
        private System.Windows.Forms.ComboBox         cbbState;
        private System.Windows.Forms.Label            lblDocNameL;
        private System.Windows.Forms.TextBox          txtDocName;
        private System.Windows.Forms.Label            lblDocTypeL;
        private System.Windows.Forms.TextBox          txtDocType;
        private System.Windows.Forms.Label            lblReqL;
        private System.Windows.Forms.CheckBox         chkRequired;
        private System.Windows.Forms.Label            lblStatusL;
        private System.Windows.Forms.ComboBox         cbbDocStatus;
        private System.Windows.Forms.Label            lblRemarkL;
        private System.Windows.Forms.TextBox          txtRemark;
        private System.Windows.Forms.Label            lblSeqNoL;
        private System.Windows.Forms.NumericUpDown    nudSeqNo;
        private System.Windows.Forms.FlowLayoutPanel  pnlButtons;
        private System.Windows.Forms.Button           btnSave;
        private System.Windows.Forms.Button           btnCancel;
    }
}
