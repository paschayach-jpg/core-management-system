namespace RG_Project.Project.Forms
{
    partial class Frm_Project_Role_Edit
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblName    = new System.Windows.Forms.Label();
            this.txtName    = new System.Windows.Forms.TextBox();
            this.chkActive  = new System.Windows.Forms.CheckBox();
            this.btnOk      = new System.Windows.Forms.Button();
            this.btnCancel  = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.Location  = new System.Drawing.Point(12, 20);
            this.lblName.Size      = new System.Drawing.Size(70, 23);
            this.lblName.Text      = "ชื่อ Role:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtName
            // 
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtName.Location    = new System.Drawing.Point(88, 20);
            this.txtName.MaxLength   = 100;
            this.txtName.Name        = "txtName";
            this.txtName.Size        = new System.Drawing.Size(210, 23);
            this.txtName.TabIndex    = 0;
            // 
            // chkActive
            // 
            this.chkActive.Location = new System.Drawing.Point(88, 54);
            this.chkActive.Name     = "chkActive";
            this.chkActive.Size     = new System.Drawing.Size(80, 20);
            this.chkActive.TabIndex = 1;
            this.chkActive.Text     = "ใช้งาน";
            // 
            // btnOk
            // 
            this.btnOk.BackColor                 = System.Drawing.Color.FromArgb(43, 87, 154);
            this.btnOk.Cursor                    = System.Windows.Forms.Cursors.Hand;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatStyle                 = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Font                      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnOk.ForeColor                 = System.Drawing.Color.White;
            this.btnOk.Location                  = new System.Drawing.Point(88, 90);
            this.btnOk.Name                      = "btnOk";
            this.btnOk.Size                      = new System.Drawing.Size(80, 28);
            this.btnOk.TabIndex                  = 2;
            this.btnOk.Text                      = "บันทึก";
            this.btnOk.UseVisualStyleBackColor    = false;
            this.btnOk.Click                     += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor                 = System.Drawing.Color.FromArgb(100, 130, 168);
            this.btnCancel.Cursor                    = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult              = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle                 = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor                 = System.Drawing.Color.White;
            this.btnCancel.Location                  = new System.Drawing.Point(180, 90);
            this.btnCancel.Name                      = "btnCancel";
            this.btnCancel.Size                      = new System.Drawing.Size(80, 28);
            this.btnCancel.TabIndex                  = 3;
            this.btnCancel.Text                      = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor    = false;
            // 
            // Frm_Project_Role_Edit
            // 
            this.AcceptButton        = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton        = this.btnCancel;
            this.ClientSize          = new System.Drawing.Size(320, 135);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblName, this.txtName, this.chkActive,
                this.btnOk,   this.btnCancel });
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "Frm_Project_Role_Edit";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "แก้ไข Role";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label    lblName;
        private System.Windows.Forms.TextBox  txtName;
        private System.Windows.Forms.CheckBox chkActive;
        private System.Windows.Forms.Button   btnOk;
        private System.Windows.Forms.Button   btnCancel;
    }
}
