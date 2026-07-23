namespace CMS.Presentation.Modules.RGP
{
    partial class Frm_Select_StateMember
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lblHint = new System.Windows.Forms.Label();
            this.dgvMembers = new System.Windows.Forms.DataGridView();
            this.colId   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRole = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAddMember = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMembers)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHint
            // 
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblHint.Location = new System.Drawing.Point(8, 8);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(460, 20);
            this.lblHint.TabIndex = 0;
            this.lblHint.Text = "เลือกแผนกจากรายชื่อในโครงการ แล้วกด ตกลง";
            // 
            // dgvMembers
            // 
            this.dgvMembers.AllowUserToAddRows = false;
            this.dgvMembers.AutoGenerateColumns = false;
            this.dgvMembers.BackgroundColor = System.Drawing.Color.White;
            this.dgvMembers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvMembers.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMembers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvMembers.ColumnHeadersHeight = 28;
            this.dgvMembers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvMembers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colId, this.colName, this.colRole });
            this.dgvMembers.EnableHeadersVisualStyles = false;
            this.dgvMembers.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(220)))), ((int)(((byte)(240)))));
            this.dgvMembers.Location = new System.Drawing.Point(8, 31);
            this.dgvMembers.MultiSelect = false;
            this.dgvMembers.Name = "dgvMembers";
            this.dgvMembers.ReadOnly = true;
            this.dgvMembers.RowHeadersVisible = false;
            this.dgvMembers.RowTemplate.Height = 26;
            this.dgvMembers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMembers.Size = new System.Drawing.Size(460, 271);
            this.dgvMembers.TabIndex = 1;
            this.dgvMembers.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 244, 255);
            this.dgvMembers.DefaultCellStyle.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvMembers.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(42, 57, 80);
            this.dgvMembers.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(190, 215, 255);
            this.dgvMembers.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(20, 40, 80);
            this.dgvMembers.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMembers_CellDoubleClick);
            // 
            // colId
            // 
            this.colId.DataPropertyName = "i_member_id";
            this.colId.Name             = "colId";
            this.colId.Visible          = false;
            // 
            // colName
            // 
            this.colName.AutoSizeMode     = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.DataPropertyName = "n_member_name";
            this.colName.HeaderText       = "ชื่อ";
            this.colName.Name             = "colName";
            this.colName.MinimumWidth     = 200;
            // 
            // colRole
            // 
            this.colRole.DataPropertyName = "c_role";
            this.colRole.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colRole.HeaderText       = "บทบาท";
            this.colRole.Name             = "colRole";
            this.colRole.Width            = 160;
            //
            // btnAddMember
            //
            this.btnAddMember.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(120)))), ((int)(((byte)(71)))));
            this.btnAddMember.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddMember.FlatAppearance.BorderSize = 0;
            this.btnAddMember.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddMember.ForeColor = System.Drawing.Color.White;
            this.btnAddMember.Location = new System.Drawing.Point(8, 316);
            this.btnAddMember.Name = "btnAddMember";
            this.btnAddMember.Size = new System.Drawing.Size(130, 30);
            this.btnAddMember.TabIndex = 4;
            this.btnAddMember.Text = "+ เพิ่มแผนกที่เกี่ยวข้อง";
            this.btnAddMember.UseVisualStyleBackColor = false;
            this.btnAddMember.Click += new System.EventHandler(this.btnAddMember_Click);
            //
            // btnOk
            //
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnOk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location = new System.Drawing.Point(290, 316);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 30);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "ตกลง";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(382, 316);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // Frm_Select_StateMember
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(480, 360);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.dgvMembers);
            this.Controls.Add(this.btnAddMember);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Frm_Select_StateMember";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "เลือกแผนกที่เกี่ยวข้อง";
            this.Load += new System.EventHandler(this.Frm_Select_StateMember_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMembers)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label                  lblHint;
        private System.Windows.Forms.DataGridView           dgvMembers;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRole;
        private System.Windows.Forms.Button                 btnAddMember;
        private System.Windows.Forms.Button                 btnOk;
        private System.Windows.Forms.Button                 btnCancel;
    }
}
