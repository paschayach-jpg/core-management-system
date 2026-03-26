namespace CMS.Presentation
{
    partial class CreateOrgUnitDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.tblLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblName = new System.Windows.Forms.Label();
            this.pnlNameRow = new System.Windows.Forms.Panel();
            this.btnSearchTH = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblNameEN = new System.Windows.Forms.Label();
            this.pnlNameENRow = new System.Windows.Forms.Panel();
            this.btnSearchEN = new System.Windows.Forms.Button();
            this.txtNameEN = new System.Windows.Forms.TextBox();
            this.lblCode = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.lblTier = new System.Windows.Forms.Label();
            this.cboTier = new System.Windows.Forms.ComboBox();
            this.lblParent = new System.Windows.Forms.Label();
            this.cboParent = new System.Windows.Forms.ComboBox();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.tblLayout.SuspendLayout();
            this.pnlNameRow.SuspendLayout();
            this.pnlNameENRow.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.pnlHeader.Size = new System.Drawing.Size(480, 56);
            this.pnlHeader.TabIndex = 2;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(10, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(470, 56);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "สร้างหน่วยงานใหม่";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tblLayout
            // 
            this.tblLayout.ColumnCount = 2;
            this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblLayout.Controls.Add(this.lblName, 0, 0);
            this.tblLayout.Controls.Add(this.pnlNameRow, 1, 0);
            this.tblLayout.Controls.Add(this.lblNameEN, 0, 1);
            this.tblLayout.Controls.Add(this.pnlNameENRow, 1, 1);
            this.tblLayout.Controls.Add(this.lblCode, 0, 2);
            this.tblLayout.Controls.Add(this.txtCode, 1, 2);
            this.tblLayout.Controls.Add(this.lblTier, 0, 3);
            this.tblLayout.Controls.Add(this.cboTier, 1, 3);
            this.tblLayout.Controls.Add(this.lblParent, 0, 4);
            this.tblLayout.Controls.Add(this.cboParent, 1, 4);
            this.tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblLayout.Location = new System.Drawing.Point(0, 40);
            this.tblLayout.Name = "tblLayout";
            this.tblLayout.Padding = new System.Windows.Forms.Padding(12, 10, 12, 8);
            this.tblLayout.RowCount = 5;
            this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblLayout.Size = new System.Drawing.Size(480, 236);
            this.tblLayout.TabIndex = 0;
            // 
            // lblName
            // 
            this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblName.Location = new System.Drawing.Point(15, 10);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(114, 36);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "ชื่อหน่วยงาน (TH):";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlNameRow
            // 
            this.pnlNameRow.Controls.Add(this.btnSearchTH);
            this.pnlNameRow.Controls.Add(this.txtName);
            this.pnlNameRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNameRow.Location = new System.Drawing.Point(135, 13);
            this.pnlNameRow.Name = "pnlNameRow";
            this.pnlNameRow.Size = new System.Drawing.Size(330, 30);
            this.pnlNameRow.TabIndex = 1;
            // 
            // btnSearchTH
            // 
            this.btnSearchTH.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnSearchTH.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchTH.FlatAppearance.BorderSize = 0;
            this.btnSearchTH.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchTH.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnSearchTH.ForeColor = System.Drawing.Color.White;
            this.btnSearchTH.Location = new System.Drawing.Point(263, 5);
            this.btnSearchTH.Name = "btnSearchTH";
            this.btnSearchTH.Size = new System.Drawing.Size(76, 26);
            this.btnSearchTH.TabIndex = 2;
            this.btnSearchTH.Text = "🔍 ค้นหา";
            this.btnSearchTH.UseVisualStyleBackColor = false;
            this.btnSearchTH.Click += new System.EventHandler(this.OnSearchNameClick);
            // 
            // txtName
            // 
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtName.Location = new System.Drawing.Point(3, 7);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(255, 23);
            this.txtName.TabIndex = 1;
            // 
            // lblNameEN
            // 
            this.lblNameEN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNameEN.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblNameEN.Location = new System.Drawing.Point(15, 46);
            this.lblNameEN.Name = "lblNameEN";
            this.lblNameEN.Size = new System.Drawing.Size(114, 36);
            this.lblNameEN.TabIndex = 2;
            this.lblNameEN.Text = "ชื่อหน่วยงาน (EN):";
            this.lblNameEN.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlNameENRow
            // 
            this.pnlNameENRow.Controls.Add(this.btnSearchEN);
            this.pnlNameENRow.Controls.Add(this.txtNameEN);
            this.pnlNameENRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNameENRow.Location = new System.Drawing.Point(135, 49);
            this.pnlNameENRow.Name = "pnlNameENRow";
            this.pnlNameENRow.Size = new System.Drawing.Size(330, 30);
            this.pnlNameENRow.TabIndex = 3;
            // 
            // btnSearchEN
            // 
            this.btnSearchEN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnSearchEN.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchEN.FlatAppearance.BorderSize = 0;
            this.btnSearchEN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchEN.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnSearchEN.ForeColor = System.Drawing.Color.White;
            this.btnSearchEN.Location = new System.Drawing.Point(263, 5);
            this.btnSearchEN.Name = "btnSearchEN";
            this.btnSearchEN.Size = new System.Drawing.Size(76, 26);
            this.btnSearchEN.TabIndex = 4;
            this.btnSearchEN.Text = "🔍 ค้นหา";
            this.btnSearchEN.UseVisualStyleBackColor = false;
            this.btnSearchEN.Click += new System.EventHandler(this.OnSearchNameClick);
            // 
            // txtNameEN
            // 
            this.txtNameEN.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtNameEN.Location = new System.Drawing.Point(3, 7);
            this.txtNameEN.Name = "txtNameEN";
            this.txtNameEN.Size = new System.Drawing.Size(255, 23);
            this.txtNameEN.TabIndex = 3;
            // 
            // lblCode
            // 
            this.lblCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCode.Location = new System.Drawing.Point(15, 82);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(114, 36);
            this.lblCode.TabIndex = 4;
            this.lblCode.Text = "รหัส (Code):";
            this.lblCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCode
            // 
            this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCode.Location = new System.Drawing.Point(135, 85);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(330, 23);
            this.txtCode.TabIndex = 5;
            // 
            // lblTier
            // 
            this.lblTier.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTier.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTier.Location = new System.Drawing.Point(15, 118);
            this.lblTier.Name = "lblTier";
            this.lblTier.Size = new System.Drawing.Size(114, 36);
            this.lblTier.TabIndex = 6;
            this.lblTier.Text = "ระดับ:";
            this.lblTier.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboTier
            // 
            this.cboTier.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboTier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTier.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboTier.Items.AddRange(new object[] {
            "SubCommittee",
            "Division",
            "Department"});
            this.cboTier.Location = new System.Drawing.Point(135, 121);
            this.cboTier.Name = "cboTier";
            this.cboTier.Size = new System.Drawing.Size(330, 23);
            this.cboTier.TabIndex = 6;
            this.cboTier.SelectedIndexChanged += new System.EventHandler(this.OnTierChanged);
            // 
            // lblParent
            // 
            this.lblParent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblParent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblParent.Location = new System.Drawing.Point(15, 154);
            this.lblParent.Name = "lblParent";
            this.lblParent.Size = new System.Drawing.Size(114, 74);
            this.lblParent.TabIndex = 7;
            this.lblParent.Text = "สังกัด:";
            this.lblParent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboParent
            // 
            this.cboParent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParent.Enabled = false;
            this.cboParent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboParent.Location = new System.Drawing.Point(135, 157);
            this.cboParent.Name = "cboParent";
            this.cboParent.Size = new System.Drawing.Size(330, 23);
            this.cboParent.TabIndex = 7;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.pnlFooter.Controls.Add(this.btnOk);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 276);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(8);
            this.pnlFooter.Size = new System.Drawing.Size(480, 44);
            this.pnlFooter.TabIndex = 1;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location = new System.Drawing.Point(296, 8);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 28);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "บันทึก";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.OnOkClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.Location = new System.Drawing.Point(384, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 28);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "ยกเลิก";
            // 
            // CreateOrgUnitDialog
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(480, 320);
            this.Controls.Add(this.tblLayout);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateOrgUnitDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "สร้างหน่วยงานใหม่";
            this.pnlHeader.ResumeLayout(false);
            this.tblLayout.ResumeLayout(false);
            this.tblLayout.PerformLayout();
            this.pnlNameRow.ResumeLayout(false);
            this.pnlNameRow.PerformLayout();
            this.pnlNameENRow.ResumeLayout(false);
            this.pnlNameENRow.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel            pnlHeader;
        private System.Windows.Forms.Label            lblHeader;
        private System.Windows.Forms.TableLayoutPanel tblLayout;
        private System.Windows.Forms.Label            lblName;
        private System.Windows.Forms.Panel            pnlNameRow;
        private System.Windows.Forms.TextBox          txtName;
        private System.Windows.Forms.Button           btnSearchTH;
        private System.Windows.Forms.Label            lblNameEN;
        private System.Windows.Forms.Panel            pnlNameENRow;
        private System.Windows.Forms.TextBox          txtNameEN;
        private System.Windows.Forms.Button           btnSearchEN;
        private System.Windows.Forms.Label            lblCode;
        private System.Windows.Forms.TextBox          txtCode;
        private System.Windows.Forms.Label            lblTier;
        private System.Windows.Forms.ComboBox         cboTier;
        private System.Windows.Forms.Label            lblParent;
        private System.Windows.Forms.ComboBox         cboParent;
        private System.Windows.Forms.Panel            pnlFooter;
        private System.Windows.Forms.Button           btnOk;
        private System.Windows.Forms.Button           btnCancel;
    }
}
