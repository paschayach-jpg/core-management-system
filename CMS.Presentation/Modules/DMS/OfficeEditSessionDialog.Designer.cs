namespace CMS.Presentation
{
    partial class OfficeEditSessionDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.tmrWatch = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            //
            // lblFileName
            //
            this.lblFileName.AutoEllipsis = true;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblFileName.ForeColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.lblFileName.Location = new System.Drawing.Point(14, 12);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(432, 22);
            this.lblFileName.TabIndex = 0;
            //
            // lblInstruction
            //
            this.lblInstruction.Location = new System.Drawing.Point(14, 40);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new System.Drawing.Size(432, 48);
            this.lblInstruction.TabIndex = 1;
            this.lblInstruction.Text = "แก้ไขไฟล์ในโปรแกรม แล้วกดบันทึก (Ctrl+S) ในโปรแกรมก่อน จากนั้นกด \"บันทึกกลับเข้า CMS\" เพื่อเก็บเป็นเวอร์ชันใหม่";
            //
            // lblStatus
            //
            this.lblStatus.ForeColor = System.Drawing.Color.DimGray;
            this.lblStatus.Location = new System.Drawing.Point(14, 92);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(432, 20);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "ยังไม่มีการแก้ไข";
            //
            // btnSave
            //
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.btnSave.Enabled = false;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(214, 122);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(140, 30);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "บันทึกกลับเข้า CMS";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // btnClose
            //
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(220, 228, 245);
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.btnClose.Location = new System.Drawing.Point(364, 122);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(82, 30);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "ปิด";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // tmrWatch
            //
            this.tmrWatch.Interval = 2000;
            this.tmrWatch.Tick += new System.EventHandler(this.TmrWatch_Tick);
            //
            // OfficeEditSessionDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.ClientSize = new System.Drawing.Size(460, 166);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "OfficeEditSessionDialog";
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "แก้ไขไฟล์";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OfficeEditSessionDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OfficeEditSessionDialog_FormClosed);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblInstruction;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Timer tmrWatch;
    }
}
