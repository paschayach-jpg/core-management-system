namespace CMS.Presentation.Modules.RGP
{
    partial class Frm_Project_Task_Status
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblStatusL = new System.Windows.Forms.Label();
            this.cbbStatus  = new System.Windows.Forms.ComboBox();
            this.lblPctL    = new System.Windows.Forms.Label();
            this.nudPct     = new System.Windows.Forms.NumericUpDown();
            this.btnOk      = new System.Windows.Forms.Button();
            this.btnCancel  = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudPct)).BeginInit();
            this.SuspendLayout();
            // 
            // lblStatusL
            // 
            this.lblStatusL.ForeColor = System.Drawing.Color.FromArgb(42, 57, 80);
            this.lblStatusL.Location  = new System.Drawing.Point(12, 24);
            this.lblStatusL.Name      = "lblStatusL";
            this.lblStatusL.Size      = new System.Drawing.Size(90, 23);
            this.lblStatusL.TabIndex  = 0;
            this.lblStatusL.Text      = "สถานะ:";
            this.lblStatusL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbStatus
            // 
            this.cbbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbStatus.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbStatus.Items.AddRange(new object[] {
                "📌 รอดำเนินการ",
                "🔄 กำลังทำ",
                "🔍 รอตรวจสอบ",
                "✅ เสร็จแล้ว",
                "⏸ On Hold" });
            this.cbbStatus.Location      = new System.Drawing.Point(110, 22);
            this.cbbStatus.Name          = "cbbStatus";
            this.cbbStatus.Size          = new System.Drawing.Size(200, 23);
            this.cbbStatus.TabIndex      = 1;
            this.cbbStatus.SelectedIndexChanged += new System.EventHandler(this.cbbStatus_SelectedIndexChanged);
            // 
            // lblPctL
            // 
            this.lblPctL.ForeColor = System.Drawing.Color.FromArgb(42, 57, 80);
            this.lblPctL.Location  = new System.Drawing.Point(12, 68);
            this.lblPctL.Name      = "lblPctL";
            this.lblPctL.Size      = new System.Drawing.Size(90, 23);
            this.lblPctL.TabIndex  = 2;
            this.lblPctL.Text      = "% เสร็จ:";
            this.lblPctL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudPct
            // 
            this.nudPct.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.nudPct.Location = new System.Drawing.Point(110, 66);
            this.nudPct.Maximum  = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudPct.Name     = "nudPct";
            this.nudPct.Size     = new System.Drawing.Size(80, 23);
            this.nudPct.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(43, 87, 154);
            this.btnOk.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location  = new System.Drawing.Point(140, 128);
            this.btnOk.Name      = "btnOk";
            this.btnOk.Size      = new System.Drawing.Size(80, 30);
            this.btnOk.TabIndex  = 4;
            this.btnOk.Text      = "บันทึก";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click    += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor    = System.Drawing.Color.FromArgb(100, 130, 168);
            this.btnCancel.Cursor       = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor    = System.Drawing.Color.White;
            this.btnCancel.Location     = new System.Drawing.Point(232, 128);
            this.btnCancel.Name         = "btnCancel";
            this.btnCancel.Size         = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex     = 5;
            this.btnCancel.Text         = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click       += new System.EventHandler(this.btnCancel_Click);
            // 
            // Frm_Project_Task_Status
            // 
            this.AcceptButton        = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(242, 246, 252);
            this.CancelButton        = this.btnCancel;
            this.ClientSize          = new System.Drawing.Size(340, 185);
            this.Controls.Add(this.lblStatusL);
            this.Controls.Add(this.cbbStatus);
            this.Controls.Add(this.lblPctL);
            this.Controls.Add(this.nudPct);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "Frm_Project_Task_Status";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "อัปเดตสถานะ Task";
            ((System.ComponentModel.ISupportInitialize)(this.nudPct)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label        lblStatusL;
        private System.Windows.Forms.ComboBox     cbbStatus;
        private System.Windows.Forms.Label        lblPctL;
        private System.Windows.Forms.NumericUpDown nudPct;
        private System.Windows.Forms.Button       btnOk;
        private System.Windows.Forms.Button       btnCancel;
    }
}
