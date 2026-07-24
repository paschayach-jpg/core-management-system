namespace RG_Project.Project.Forms
{
    partial class Frm_Project_Role
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvRoles    = new System.Windows.Forms.DataGridView();
            this.pnlInput    = new System.Windows.Forms.Panel();
            this.lblRoleL    = new System.Windows.Forms.Label();
            this.txtRoleName = new System.Windows.Forms.TextBox();
            this.btnAdd      = new System.Windows.Forms.Button();
            this.pnlToolbar  = new System.Windows.Forms.Panel();
            this.btnEdit     = new System.Windows.Forms.Button();
            this.btnDelete   = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRoles)).BeginInit();
            this.pnlInput.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlInput
            // 
            this.pnlInput.BackColor = System.Drawing.Color.FromArgb(230, 236, 248);
            this.pnlInput.Controls.Add(this.lblRoleL);
            this.pnlInput.Controls.Add(this.txtRoleName);
            this.pnlInput.Controls.Add(this.btnAdd);
            this.pnlInput.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlInput.Height = 46;
            this.pnlInput.Name = "pnlInput";
            this.pnlInput.TabIndex = 0;
            // 
            // lblRoleL
            // 
            this.lblRoleL.Location  = new System.Drawing.Point(8, 12);
            this.lblRoleL.Size      = new System.Drawing.Size(70, 23);
            this.lblRoleL.Text      = "ชื่อ Role:";
            this.lblRoleL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRoleName
            // 
            this.txtRoleName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRoleName.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRoleName.Location    = new System.Drawing.Point(84, 12);
            this.txtRoleName.MaxLength   = 100;
            this.txtRoleName.Name        = "txtRoleName";
            this.txtRoleName.Size        = new System.Drawing.Size(220, 23);
            this.txtRoleName.TabIndex    = 0;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor                 = System.Drawing.Color.FromArgb(43, 87, 154);
            this.btnAdd.Cursor                    = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle                 = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font                      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor                 = System.Drawing.Color.White;
            this.btnAdd.Location                  = new System.Drawing.Point(312, 10);
            this.btnAdd.Name                      = "btnAdd";
            this.btnAdd.Size                      = new System.Drawing.Size(80, 26);
            this.btnAdd.TabIndex                  = 1;
            this.btnAdd.Text                      = "+ เพิ่ม";
            this.btnAdd.UseVisualStyleBackColor    = false;
            this.btnAdd.Click                     += new System.EventHandler(this.btnAdd_Click);
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(242, 246, 252);
            this.pnlToolbar.Controls.Add(this.btnEdit);
            this.pnlToolbar.Controls.Add(this.btnDelete);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.TabIndex = 2;
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor                 = System.Drawing.Color.FromArgb(70, 110, 165);
            this.btnEdit.Cursor                    = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle                 = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font                      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnEdit.ForeColor                 = System.Drawing.Color.White;
            this.btnEdit.Location                  = new System.Drawing.Point(8, 8);
            this.btnEdit.Name                      = "btnEdit";
            this.btnEdit.Size                      = new System.Drawing.Size(80, 28);
            this.btnEdit.TabIndex                  = 0;
            this.btnEdit.Text                      = "แก้ไข";
            this.btnEdit.UseVisualStyleBackColor    = false;
            this.btnEdit.Click                     += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor                 = System.Drawing.Color.FromArgb(196, 53, 53);
            this.btnDelete.Cursor                    = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle                 = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font                      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDelete.ForeColor                 = System.Drawing.Color.White;
            this.btnDelete.Location                  = new System.Drawing.Point(96, 8);
            this.btnDelete.Name                      = "btnDelete";
            this.btnDelete.Size                      = new System.Drawing.Size(80, 28);
            this.btnDelete.TabIndex                  = 1;
            this.btnDelete.Text                      = "ลบ";
            this.btnDelete.UseVisualStyleBackColor    = false;
            this.btnDelete.Click                     += new System.EventHandler(this.btnDelete_Click);
            // 
            // dgvRoles
            // 
            this.dgvRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Top    |
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left   |
                System.Windows.Forms.AnchorStyles.Right));
            this.dgvRoles.Location = new System.Drawing.Point(0, 46);
            this.dgvRoles.Name     = "dgvRoles";
            this.dgvRoles.Size     = new System.Drawing.Size(440, 280);
            this.dgvRoles.TabIndex = 1;
            // 
            // Frm_Project_Role
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(440, 400);
            this.Controls.Add(this.dgvRoles);
            this.Controls.Add(this.pnlInput);
            this.Controls.Add(this.pnlToolbar);
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "Frm_Project_Role";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "จัดการ Role บุคคลที่เกี่ยวข้อง";
            this.Load            += new System.EventHandler(this.Frm_Project_Role_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRoles)).EndInit();
            this.pnlInput.ResumeLayout(false);
            this.pnlInput.PerformLayout();
            this.pnlToolbar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgvRoles;
        private System.Windows.Forms.Panel        pnlInput;
        private System.Windows.Forms.Label        lblRoleL;
        private System.Windows.Forms.TextBox      txtRoleName;
        private System.Windows.Forms.Button       btnAdd;
        private System.Windows.Forms.Panel        pnlToolbar;
        private System.Windows.Forms.Button       btnEdit;
        private System.Windows.Forms.Button       btnDelete;
    }
}
