namespace CMS.Presentation.Modules.RGP
{
    partial class Frm_Project_Progress_Update
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
            this.lblStateName = new System.Windows.Forms.Label();
            this.lblStatusL = new System.Windows.Forms.Label();
            this.cbbStatus = new System.Windows.Forms.ComboBox();
            this.lblPctL = new System.Windows.Forms.Label();
            this.nudPct = new System.Windows.Forms.NumericUpDown();
            this.lblRemarkL = new System.Windows.Forms.Label();
            this.txtRemark = new System.Windows.Forms.TextBox();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPct)).BeginInit();
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
            this.lblTitle.Text = "อัปเดตความคืบหน้า";
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
            this.pnlBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.pnlBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlBody.Controls.Add(this.lblStateL, 0, 0);
            this.pnlBody.Controls.Add(this.lblStateName, 1, 0);
            this.pnlBody.Controls.Add(this.lblStatusL, 0, 1);
            this.pnlBody.Controls.Add(this.cbbStatus, 1, 1);
            this.pnlBody.Controls.Add(this.lblPctL, 0, 2);
            this.pnlBody.Controls.Add(this.nudPct, 1, 2);
            this.pnlBody.Controls.Add(this.lblRemarkL, 0, 3);
            this.pnlBody.Controls.Add(this.txtRemark, 1, 3);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 51);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.pnlBody.RowCount = 4;
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.pnlBody.Size = new System.Drawing.Size(500, 232);
            this.pnlBody.TabIndex = 0;
            // 
            // lblStateL
            // 
            this.lblStateL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStateL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStateL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStateL.Location = new System.Drawing.Point(13, 10);
            this.lblStateL.Name = "lblStateL";
            this.lblStateL.Size = new System.Drawing.Size(114, 35);
            this.lblStateL.TabIndex = 0;
            this.lblStateL.Text = "ขั้นตอน:";
            this.lblStateL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStateName
            // 
            this.lblStateName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStateName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStateName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblStateName.Location = new System.Drawing.Point(133, 10);
            this.lblStateName.Name = "lblStateName";
            this.lblStateName.Size = new System.Drawing.Size(354, 35);
            this.lblStateName.TabIndex = 1;
            this.lblStateName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStatusL
            // 
            this.lblStatusL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblStatusL.Location = new System.Drawing.Point(13, 45);
            this.lblStatusL.Name = "lblStatusL";
            this.lblStatusL.Size = new System.Drawing.Size(114, 35);
            this.lblStatusL.TabIndex = 2;
            this.lblStatusL.Text = "สถานะ:";
            this.lblStatusL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbStatus
            // 
            this.cbbStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbStatus.Items.AddRange(new object[] {
            "Pending",
            "InProgress",
            "Completed"});
            this.cbbStatus.Location = new System.Drawing.Point(133, 48);
            this.cbbStatus.Name = "cbbStatus";
            this.cbbStatus.Size = new System.Drawing.Size(354, 23);
            this.cbbStatus.TabIndex = 3;
            this.cbbStatus.SelectedIndexChanged += new System.EventHandler(this.cbbStatus_SelectedIndexChanged);
            // 
            // lblPctL
            // 
            this.lblPctL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPctL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPctL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblPctL.Location = new System.Drawing.Point(13, 80);
            this.lblPctL.Name = "lblPctL";
            this.lblPctL.Size = new System.Drawing.Size(114, 35);
            this.lblPctL.TabIndex = 4;
            this.lblPctL.Text = "% เสร็จ:";
            this.lblPctL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudPct
            // 
            this.nudPct.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.nudPct.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudPct.Location = new System.Drawing.Point(133, 83);
            this.nudPct.Name = "nudPct";
            this.nudPct.Size = new System.Drawing.Size(80, 23);
            this.nudPct.TabIndex = 5;
            this.nudPct.ValueChanged += new System.EventHandler(this.nudPct_ValueChanged);
            // 
            // lblRemarkL
            // 
            this.lblRemarkL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRemarkL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRemarkL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblRemarkL.Location = new System.Drawing.Point(13, 115);
            this.lblRemarkL.Name = "lblRemarkL";
            this.lblRemarkL.Size = new System.Drawing.Size(114, 117);
            this.lblRemarkL.TabIndex = 6;
            this.lblRemarkL.Text = "หมายเหตุ:";
            this.lblRemarkL.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtRemark
            // 
            this.txtRemark.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRemark.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRemark.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRemark.Location = new System.Drawing.Point(133, 118);
            this.txtRemark.MaxLength = 1000;
            this.txtRemark.Multiline = true;
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(354, 111);
            this.txtRemark.TabIndex = 7;
            // 
            // pnlButtons
            // 
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(0, 283);
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
            // Frm_Project_Progress_Update
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(500, 333);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Frm_Project_Progress_Update";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "อัปเดตความคืบหน้า";
            this.pnlHeader.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPct)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel            pnlHeader;
        private System.Windows.Forms.Label            lblTitle;
        private System.Windows.Forms.Panel            pnlAccent;
        private System.Windows.Forms.TableLayoutPanel pnlBody;
        private System.Windows.Forms.Label            lblStateL;
        private System.Windows.Forms.Label            lblStateName;
        private System.Windows.Forms.Label            lblStatusL;
        private System.Windows.Forms.ComboBox         cbbStatus;
        private System.Windows.Forms.Label            lblPctL;
        private System.Windows.Forms.NumericUpDown    nudPct;
        private System.Windows.Forms.Label            lblRemarkL;
        private System.Windows.Forms.TextBox          txtRemark;
        private System.Windows.Forms.FlowLayoutPanel  pnlButtons;
        private System.Windows.Forms.Button           btnSave;
        private System.Windows.Forms.Button           btnCancel;
    }
}
