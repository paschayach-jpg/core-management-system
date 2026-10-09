namespace CMS.Presentation.Modules.DMS
{
    partial class SharePermissionConfigDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlHeaderIcon = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSub = new System.Windows.Forms.Label();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblFileNameTitle = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();
            this.lblTargetTitle = new System.Windows.Forms.Label();
            this.lblSharer = new System.Windows.Forms.Label();
            this.lblSharerTitle = new System.Windows.Forms.Label();
            this.lblGrantedDate = new System.Windows.Forms.Label();
            this.lblGrantedDateTitle = new System.Windows.Forms.Label();
            this.grpPermissions = new System.Windows.Forms.GroupBox();
            this.chkRead = new System.Windows.Forms.CheckBox();
            this.chkDownload = new System.Windows.Forms.CheckBox();
            this.chkWrite = new System.Windows.Forms.CheckBox();
            this.chkDelete = new System.Windows.Forms.CheckBox();
            this.grpExpiry = new System.Windows.Forms.GroupBox();
            this.rbNoExpiry = new System.Windows.Forms.RadioButton();
            this.rbCustomExpiry = new System.Windows.Forms.RadioButton();
            this.dtpExpiry = new System.Windows.Forms.DateTimePicker();
            this.grpRemark = new System.Windows.Forms.GroupBox();
            this.txtRemark = new System.Windows.Forms.TextBox();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.grpPermissions.SuspendLayout();
            this.grpExpiry.SuspendLayout();
            this.grpRemark.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.pnlHeader.Controls.Add(this.pnlHeaderIcon);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSub);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(520, 60);
            this.pnlHeader.TabIndex = 0;
            // 
            // pnlHeaderIcon
            // 
            this.pnlHeaderIcon.Location = new System.Drawing.Point(14, 12);
            this.pnlHeaderIcon.Name = "pnlHeaderIcon";
            this.pnlHeaderIcon.Size = new System.Drawing.Size(36, 36);
            this.pnlHeaderIcon.TabIndex = 0;
            this.pnlHeaderIcon.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlHeaderIcon_Paint);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(58, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(268, 21);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "ปรับสิทธิ์การแชร์ (Edit Permissions)";
            // 
            // lblSub
            // 
            this.lblSub.AutoSize = true;
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(210)))), ((int)(((byte)(245)))));
            this.lblSub.Location = new System.Drawing.Point(59, 33);
            this.lblSub.Name = "lblSub";
            this.lblSub.Size = new System.Drawing.Size(273, 15);
            this.lblSub.TabIndex = 2;
            this.lblSub.Text = "แก้ไขระดับสิทธิ์การเข้าถึงและวันหมดอายุสำหรับผู้รับ";
            // 
            // pnlInfo
            // 
            this.pnlInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(242)))), ((int)(((byte)(252)))));
            this.pnlInfo.Controls.Add(this.lblFileName);
            this.pnlInfo.Controls.Add(this.lblFileNameTitle);
            this.pnlInfo.Controls.Add(this.lblTarget);
            this.pnlInfo.Controls.Add(this.lblTargetTitle);
            this.pnlInfo.Controls.Add(this.lblSharer);
            this.pnlInfo.Controls.Add(this.lblSharerTitle);
            this.pnlInfo.Controls.Add(this.lblGrantedDate);
            this.pnlInfo.Controls.Add(this.lblGrantedDateTitle);
            this.pnlInfo.Location = new System.Drawing.Point(16, 70);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(488, 92);
            this.pnlInfo.TabIndex = 1;
            this.pnlInfo.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlInfo_Paint);
            // 
            // lblFileNameTitle
            // 
            this.lblFileNameTitle.AutoSize = true;
            this.lblFileNameTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFileNameTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(70)))), ((int)(((byte)(100)))));
            this.lblFileNameTitle.Location = new System.Drawing.Point(12, 10);
            this.lblFileNameTitle.Name = "lblFileNameTitle";
            this.lblFileNameTitle.Size = new System.Drawing.Size(59, 15);
            this.lblFileNameTitle.TabIndex = 0;
            this.lblFileNameTitle.Text = "ไฟล์ที่แชร์ :";
            // 
            // lblFileName
            // 
            this.lblFileName.AutoEllipsis = true;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFileName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.lblFileName.Location = new System.Drawing.Point(75, 10);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(398, 18);
            this.lblFileName.TabIndex = 1;
            this.lblFileName.Text = "-";
            // 
            // lblTargetTitle
            // 
            this.lblTargetTitle.AutoSize = true;
            this.lblTargetTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTargetTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(70)))), ((int)(((byte)(100)))));
            this.lblTargetTitle.Location = new System.Drawing.Point(12, 36);
            this.lblTargetTitle.Name = "lblTargetTitle";
            this.lblTargetTitle.Size = new System.Drawing.Size(48, 15);
            this.lblTargetTitle.TabIndex = 2;
            this.lblTargetTitle.Text = "แชร์ให้ :";
            // 
            // lblTarget
            // 
            this.lblTarget.AutoEllipsis = true;
            this.lblTarget.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTarget.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(50)))));
            this.lblTarget.Location = new System.Drawing.Point(75, 36);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(398, 18);
            this.lblTarget.TabIndex = 3;
            this.lblTarget.Text = "-";
            // 
            // lblSharerTitle
            // 
            this.lblSharerTitle.AutoSize = true;
            this.lblSharerTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSharerTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(120)))));
            this.lblSharerTitle.Location = new System.Drawing.Point(12, 62);
            this.lblSharerTitle.Name = "lblSharerTitle";
            this.lblSharerTitle.Size = new System.Drawing.Size(47, 15);
            this.lblSharerTitle.TabIndex = 4;
            this.lblSharerTitle.Text = "ผู้แชร์ :";
            // 
            // lblSharer
            // 
            this.lblSharer.AutoEllipsis = true;
            this.lblSharer.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSharer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.lblSharer.Location = new System.Drawing.Point(62, 62);
            this.lblSharer.Name = "lblSharer";
            this.lblSharer.Size = new System.Drawing.Size(180, 18);
            this.lblSharer.TabIndex = 5;
            this.lblSharer.Text = "-";
            // 
            // lblGrantedDateTitle
            // 
            this.lblGrantedDateTitle.AutoSize = true;
            this.lblGrantedDateTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblGrantedDateTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(120)))));
            this.lblGrantedDateTitle.Location = new System.Drawing.Point(250, 62);
            this.lblGrantedDateTitle.Name = "lblGrantedDateTitle";
            this.lblGrantedDateTitle.Size = new System.Drawing.Size(56, 15);
            this.lblGrantedDateTitle.TabIndex = 6;
            this.lblGrantedDateTitle.Text = "วันที่แชร์ :";
            // 
            // lblGrantedDate
            // 
            this.lblGrantedDate.AutoEllipsis = true;
            this.lblGrantedDate.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblGrantedDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.lblGrantedDate.Location = new System.Drawing.Point(310, 62);
            this.lblGrantedDate.Name = "lblGrantedDate";
            this.lblGrantedDate.Size = new System.Drawing.Size(163, 18);
            this.lblGrantedDate.TabIndex = 7;
            this.lblGrantedDate.Text = "-";
            // 
            // grpPermissions
            // 
            this.grpPermissions.Controls.Add(this.chkRead);
            this.grpPermissions.Controls.Add(this.chkDownload);
            this.grpPermissions.Controls.Add(this.chkWrite);
            this.grpPermissions.Controls.Add(this.chkDelete);
            this.grpPermissions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPermissions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.grpPermissions.Location = new System.Drawing.Point(16, 172);
            this.grpPermissions.Name = "grpPermissions";
            this.grpPermissions.Size = new System.Drawing.Size(488, 90);
            this.grpPermissions.TabIndex = 2;
            this.grpPermissions.TabStop = false;
            this.grpPermissions.Text = " 🛡️  กำหนดสิทธิ์การเข้าถึง (Access Rights) ";
            // 
            // chkRead
            // 
            this.chkRead.AutoSize = true;
            this.chkRead.Checked = true;
            this.chkRead.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRead.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRead.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(50)))));
            this.chkRead.Location = new System.Drawing.Point(24, 28);
            this.chkRead.Name = "chkRead";
            this.chkRead.Size = new System.Drawing.Size(127, 19);
            this.chkRead.TabIndex = 0;
            this.chkRead.Text = "👁  เปิดดู / อ่าน (Read)";
            this.chkRead.UseVisualStyleBackColor = true;
            // 
            // chkDownload
            // 
            this.chkDownload.AutoSize = true;
            this.chkDownload.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkDownload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(50)))));
            this.chkDownload.Location = new System.Drawing.Point(250, 28);
            this.chkDownload.Name = "chkDownload";
            this.chkDownload.Size = new System.Drawing.Size(155, 19);
            this.chkDownload.TabIndex = 1;
            this.chkDownload.Text = "⬇  ดาวน์โหลด (Download)";
            this.chkDownload.UseVisualStyleBackColor = true;
            // 
            // chkWrite
            // 
            this.chkWrite.AutoSize = true;
            this.chkWrite.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkWrite.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(50)))));
            this.chkWrite.Location = new System.Drawing.Point(24, 58);
            this.chkWrite.Name = "chkWrite";
            this.chkWrite.Size = new System.Drawing.Size(142, 19);
            this.chkWrite.TabIndex = 2;
            this.chkWrite.Text = "✏  เพิ่ม / แก้ไข (Write)";
            this.chkWrite.UseVisualStyleBackColor = true;
            // 
            // chkDelete
            // 
            this.chkDelete.AutoSize = true;
            this.chkDelete.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(50)))));
            this.chkDelete.Location = new System.Drawing.Point(250, 58);
            this.chkDelete.Name = "chkDelete";
            this.chkDelete.Size = new System.Drawing.Size(100, 19);
            this.chkDelete.TabIndex = 3;
            this.chkDelete.Text = "🗑  ลบ (Delete)";
            this.chkDelete.UseVisualStyleBackColor = true;
            // 
            // grpExpiry
            // 
            this.grpExpiry.Controls.Add(this.rbNoExpiry);
            this.grpExpiry.Controls.Add(this.rbCustomExpiry);
            this.grpExpiry.Controls.Add(this.dtpExpiry);
            this.grpExpiry.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpExpiry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.grpExpiry.Location = new System.Drawing.Point(16, 270);
            this.grpExpiry.Name = "grpExpiry";
            this.grpExpiry.Size = new System.Drawing.Size(488, 72);
            this.grpExpiry.TabIndex = 3;
            this.grpExpiry.TabStop = false;
            this.grpExpiry.Text = " ⌛  กำหนดวันหมดอายุ (Expiry Date) ";
            // 
            // rbNoExpiry
            // 
            this.rbNoExpiry.AutoSize = true;
            this.rbNoExpiry.Checked = true;
            this.rbNoExpiry.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rbNoExpiry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(50)))));
            this.rbNoExpiry.Location = new System.Drawing.Point(24, 28);
            this.rbNoExpiry.Name = "rbNoExpiry";
            this.rbNoExpiry.Size = new System.Drawing.Size(117, 19);
            this.rbNoExpiry.TabIndex = 0;
            this.rbNoExpiry.TabStop = true;
            this.rbNoExpiry.Text = "ไม่มีกำหนดหมดอายุ";
            this.rbNoExpiry.UseVisualStyleBackColor = true;
            this.rbNoExpiry.CheckedChanged += new System.EventHandler(this.RbExpiry_CheckedChanged);
            // 
            // rbCustomExpiry
            // 
            this.rbCustomExpiry.AutoSize = true;
            this.rbCustomExpiry.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rbCustomExpiry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(50)))));
            this.rbCustomExpiry.Location = new System.Drawing.Point(165, 28);
            this.rbCustomExpiry.Name = "rbCustomExpiry";
            this.rbCustomExpiry.Size = new System.Drawing.Size(117, 19);
            this.rbCustomExpiry.TabIndex = 1;
            this.rbCustomExpiry.Text = "กำหนดวันหมดอายุ :";
            this.rbCustomExpiry.UseVisualStyleBackColor = true;
            this.rbCustomExpiry.CheckedChanged += new System.EventHandler(this.RbExpiry_CheckedChanged);
            // 
            // dtpExpiry
            // 
            this.dtpExpiry.CustomFormat = "dd/MM/yyyy";
            this.dtpExpiry.Enabled = false;
            this.dtpExpiry.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpExpiry.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpExpiry.Location = new System.Drawing.Point(290, 26);
            this.dtpExpiry.Name = "dtpExpiry";
            this.dtpExpiry.Size = new System.Drawing.Size(160, 23);
            this.dtpExpiry.TabIndex = 2;
            // 
            // grpRemark
            // 
            this.grpRemark.Controls.Add(this.txtRemark);
            this.grpRemark.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpRemark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.grpRemark.Location = new System.Drawing.Point(16, 350);
            this.grpRemark.Name = "grpRemark";
            this.grpRemark.Size = new System.Drawing.Size(488, 68);
            this.grpRemark.TabIndex = 4;
            this.grpRemark.TabStop = false;
            this.grpRemark.Text = " 📝  หมายเหตุการปรับปรุง (Remark - ทางเลือก) ";
            // 
            // txtRemark
            // 
            this.txtRemark.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRemark.Location = new System.Drawing.Point(24, 26);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(426, 23);
            this.txtRemark.TabIndex = 0;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.pnlFooter.Controls.Add(this.btnSave);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 432);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(520, 48);
            this.pnlFooter.TabIndex = 5;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(270, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(140, 32);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "💾  บันทึกการปรับสิทธิ์";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(228)))), ((int)(((byte)(245)))));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(235)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.btnCancel.Location = new System.Drawing.Point(420, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // SharePermissionConfigDialog
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(520, 480);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.grpRemark);
            this.Controls.Add(this.grpExpiry);
            this.Controls.Add(this.grpPermissions);
            this.Controls.Add(this.pnlInfo);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SharePermissionConfigDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "⚙️ ปรับสิทธิ์การแชร์ (Edit Share Permissions)";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlInfo.ResumeLayout(false);
            this.pnlInfo.PerformLayout();
            this.grpPermissions.ResumeLayout(false);
            this.grpPermissions.PerformLayout();
            this.grpExpiry.ResumeLayout(false);
            this.grpExpiry.PerformLayout();
            this.grpRemark.ResumeLayout(false);
            this.grpRemark.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlHeaderIcon;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.Label lblFileNameTitle;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblTargetTitle;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.Label lblSharerTitle;
        private System.Windows.Forms.Label lblSharer;
        private System.Windows.Forms.Label lblGrantedDateTitle;
        private System.Windows.Forms.Label lblGrantedDate;
        private System.Windows.Forms.GroupBox grpPermissions;
        private System.Windows.Forms.CheckBox chkRead;
        private System.Windows.Forms.CheckBox chkDownload;
        private System.Windows.Forms.CheckBox chkWrite;
        private System.Windows.Forms.CheckBox chkDelete;
        private System.Windows.Forms.GroupBox grpExpiry;
        private System.Windows.Forms.RadioButton rbNoExpiry;
        private System.Windows.Forms.RadioButton rbCustomExpiry;
        private System.Windows.Forms.DateTimePicker dtpExpiry;
        private System.Windows.Forms.GroupBox grpRemark;
        private System.Windows.Forms.TextBox txtRemark;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
