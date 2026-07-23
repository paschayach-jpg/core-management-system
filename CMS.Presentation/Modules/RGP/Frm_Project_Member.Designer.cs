namespace CMS.Presentation.Modules.RGP
{
    partial class Frm_Project_Member
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblType = new System.Windows.Forms.Label();
            this.cbbType = new System.Windows.Forms.ComboBox();
            this.lblName = new System.Windows.Forms.Label();
            this.cbbName = new System.Windows.Forms.ComboBox();
            this.lblRole = new System.Windows.Forms.Label();
            this.cbbRole = new System.Windows.Forms.ComboBox();
            this.btnManageRole = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblType
            // 
            this.lblType.Location = new System.Drawing.Point(12, 20);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(80, 23);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "ประเภท:";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbType
            // 
            this.cbbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbType.Location = new System.Drawing.Point(100, 20);
            this.cbbType.Name = "cbbType";
            this.cbbType.Size = new System.Drawing.Size(180, 23);
            this.cbbType.TabIndex = 0;
            this.cbbType.SelectedIndexChanged += new System.EventHandler(this.cbbType_SelectedIndexChanged);
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(12, 56);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(80, 23);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "ชื่อ:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbName
            // 
            this.cbbName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbName.FormattingEnabled = true;
            this.cbbName.Location = new System.Drawing.Point(100, 56);
            this.cbbName.Name = "cbbName";
            this.cbbName.Size = new System.Drawing.Size(230, 23);
            this.cbbName.TabIndex = 1;
            this.cbbName.Visible = false;
            // 
            // lblRole
            // 
            this.lblRole.Location = new System.Drawing.Point(12, 124);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(80, 23);
            this.lblRole.TabIndex = 2;
            this.lblRole.Text = "บทบาท:";
            this.lblRole.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbbRole
            // 
            this.cbbRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbRole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbRole.Location = new System.Drawing.Point(100, 123);
            this.cbbRole.Name = "cbbRole";
            this.cbbRole.Size = new System.Drawing.Size(180, 23);
            this.cbbRole.TabIndex = 2;
            // 
            // btnManageRole
            // 
            this.btnManageRole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnManageRole.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageRole.FlatAppearance.BorderSize = 0;
            this.btnManageRole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageRole.ForeColor = System.Drawing.Color.White;
            this.btnManageRole.Location = new System.Drawing.Point(296, 123);
            this.btnManageRole.Name = "btnManageRole";
            this.btnManageRole.Size = new System.Drawing.Size(42, 24);
            this.btnManageRole.TabIndex = 3;
            this.btnManageRole.Text = "⚙";
            this.btnManageRole.UseVisualStyleBackColor = false;
            this.btnManageRole.Click += new System.EventHandler(this.btnManageRole_Click);
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnOk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location = new System.Drawing.Point(108, 168);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 30);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "ตกลง";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(200, 168);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // txtName
            // 
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtName.Location = new System.Drawing.Point(100, 94);
            this.txtName.MaxLength = 200;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(230, 23);
            this.txtName.TabIndex = 1;
            this.txtName.Visible = false;
            // 
            // Frm_Project_Member
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(350, 233);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.cbbType);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.cbbName);
            this.Controls.Add(this.lblRole);
            this.Controls.Add(this.cbbRole);
            this.Controls.Add(this.btnManageRole);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Frm_Project_Member";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "แผนกที่เกี่ยวข้อง";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label    lblType;
        private System.Windows.Forms.ComboBox cbbType;
        private System.Windows.Forms.Label    lblName;
        private System.Windows.Forms.Label    lblRole;
        private System.Windows.Forms.ComboBox cbbRole;
        private System.Windows.Forms.Button   btnManageRole;
        private System.Windows.Forms.Button   btnOk;
        private System.Windows.Forms.Button   btnCancel;
        private System.Windows.Forms.ComboBox cbbName;
        private System.Windows.Forms.TextBox txtName;
    }
}
