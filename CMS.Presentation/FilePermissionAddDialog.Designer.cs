namespace CMS.Presentation
{
    partial class FilePermissionAddDialog
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
            this.components      = new System.ComponentModel.Container();
            this.pnlHeader       = new System.Windows.Forms.Panel();
            this.lblHeader       = new System.Windows.Forms.Label();
            this.lblPrincipalType = new System.Windows.Forms.Label();
            this.rdUser          = new System.Windows.Forms.RadioButton();
            this.rdGroup         = new System.Windows.Forms.RadioButton();
            this.rdRole          = new System.Windows.Forms.RadioButton();
            this.lblSelect       = new System.Windows.Forms.Label();
            this.cboSelect       = new System.Windows.Forms.ComboBox();
            this.lblPermissions  = new System.Windows.Forms.Label();
            this.pnlPerm         = new System.Windows.Forms.Panel();
            this.pnlPermHdr      = new System.Windows.Forms.Panel();
            this.lblPermHdrTitle = new System.Windows.Forms.Label();
            this.lblPermHdrAllow = new System.Windows.Forms.Label();
            this.lblPermHdrDeny  = new System.Windows.Forms.Label();
            // Permission rows
            this.pnlRowRead      = new System.Windows.Forms.Panel();
            this.lblPermRead     = new System.Windows.Forms.Label();
            this.rdAllowRead     = new System.Windows.Forms.RadioButton();
            this.rdDenyRead      = new System.Windows.Forms.RadioButton();
            this.pnlRowWrite     = new System.Windows.Forms.Panel();
            this.lblPermWrite    = new System.Windows.Forms.Label();
            this.rdAllowWrite    = new System.Windows.Forms.RadioButton();
            this.rdDenyWrite     = new System.Windows.Forms.RadioButton();
            this.pnlRowDelete    = new System.Windows.Forms.Panel();
            this.lblPermDelete   = new System.Windows.Forms.Label();
            this.rdAllowDelete   = new System.Windows.Forms.RadioButton();
            this.rdDenyDelete    = new System.Windows.Forms.RadioButton();
            this.pnlRowModify    = new System.Windows.Forms.Panel();
            this.lblPermModify   = new System.Windows.Forms.Label();
            this.rdAllowModify   = new System.Windows.Forms.RadioButton();
            this.rdDenyModify    = new System.Windows.Forms.RadioButton();
            this.pnlRowFull      = new System.Windows.Forms.Panel();
            this.lblPermFull     = new System.Windows.Forms.Label();
            this.rdAllowFull     = new System.Windows.Forms.RadioButton();
            this.rdDenyFull      = new System.Windows.Forms.RadioButton();
            // Buttons
            this.btnOk           = new System.Windows.Forms.Button();
            this.btnCancel       = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlPerm.SuspendLayout();
            this.pnlPermHdr.SuspendLayout();
            this.pnlRowRead.SuspendLayout();
            this.pnlRowWrite.SuspendLayout();
            this.pnlRowDelete.SuspendLayout();
            this.pnlRowModify.SuspendLayout();
            this.pnlRowFull.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Location  = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name      = "pnlHeader";
            this.pnlHeader.Size      = new System.Drawing.Size(460, 44);
            this.pnlHeader.TabIndex  = 0;

            // ── lblHeader ────────────────────────────────────────────────────
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location  = new System.Drawing.Point(12, 0);
            this.lblHeader.Name      = "lblHeader";
            this.lblHeader.Size      = new System.Drawing.Size(420, 44);
            this.lblHeader.TabIndex  = 0;
            this.lblHeader.Text      = "🔒 เพิ่ม/แก้ไข สิทธิ์ไฟล์";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── lblPrincipalType ─────────────────────────────────────────────
            this.lblPrincipalType.AutoSize = true;
            this.lblPrincipalType.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPrincipalType.Location = new System.Drawing.Point(12, 58);
            this.lblPrincipalType.Name     = "lblPrincipalType";
            this.lblPrincipalType.TabIndex = 1;
            this.lblPrincipalType.Text     = "ประเภทผู้ใช้:";

            // ── rdUser ───────────────────────────────────────────────────────
            this.rdUser.AutoSize  = true;
            this.rdUser.Checked   = true;
            this.rdUser.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.rdUser.Location  = new System.Drawing.Point(24, 80);
            this.rdUser.Name      = "rdUser";
            this.rdUser.TabIndex  = 2;
            this.rdUser.TabStop   = true;
            this.rdUser.Text      = "ผู้ใช้เฉพาะราย";
            this.rdUser.CheckedChanged += new System.EventHandler(this.rdUser_CheckedChanged);

            // ── rdGroup ──────────────────────────────────────────────────────
            this.rdGroup.AutoSize = true;
            this.rdGroup.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.rdGroup.Location = new System.Drawing.Point(160, 80);
            this.rdGroup.Name     = "rdGroup";
            this.rdGroup.TabIndex = 3;
            this.rdGroup.Text     = "กลุ่ม/แผนก";
            this.rdGroup.CheckedChanged += new System.EventHandler(this.rdGroup_CheckedChanged);

            // ── rdRole ───────────────────────────────────────────────────────
            this.rdRole.AutoSize = true;
            this.rdRole.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.rdRole.Location = new System.Drawing.Point(280, 80);
            this.rdRole.Name     = "rdRole";
            this.rdRole.TabIndex = 4;
            this.rdRole.Text     = "Role";
            this.rdRole.CheckedChanged += new System.EventHandler(this.rdRole_CheckedChanged);

            // ── lblSelect ────────────────────────────────────────────────────
            this.lblSelect.AutoSize = true;
            this.lblSelect.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSelect.Location = new System.Drawing.Point(12, 116);
            this.lblSelect.Name     = "lblSelect";
            this.lblSelect.TabIndex = 5;
            this.lblSelect.Text     = "เลือกผู้ใช้:";

            // ── cboSelect ────────────────────────────────────────────────────
            this.cboSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboSelect.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cboSelect.Location      = new System.Drawing.Point(120, 112);
            this.cboSelect.Name          = "cboSelect";
            this.cboSelect.Size          = new System.Drawing.Size(328, 23);
            this.cboSelect.TabIndex      = 6;

            // ── lblPermissions ───────────────────────────────────────────────
            this.lblPermissions.AutoSize = true;
            this.lblPermissions.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPermissions.Location = new System.Drawing.Point(12, 150);
            this.lblPermissions.Name     = "lblPermissions";
            this.lblPermissions.TabIndex = 7;
            this.lblPermissions.Text     = "สิทธิการเข้าถึง:";

            // ── pnlPerm ───────────────────────────────────────────────────────
            this.pnlPerm.BackColor   = System.Drawing.Color.White;
            this.pnlPerm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPerm.Controls.Add(this.pnlPermHdr);
            this.pnlPerm.Controls.Add(this.pnlRowRead);
            this.pnlPerm.Controls.Add(this.pnlRowWrite);
            this.pnlPerm.Controls.Add(this.pnlRowDelete);
            this.pnlPerm.Controls.Add(this.pnlRowModify);
            this.pnlPerm.Controls.Add(this.pnlRowFull);
            this.pnlPerm.Location    = new System.Drawing.Point(12, 170);
            this.pnlPerm.Name        = "pnlPerm";
            this.pnlPerm.Size        = new System.Drawing.Size(436, 177);
            this.pnlPerm.TabIndex    = 8;

            // ── pnlPermHdr ───────────────────────────────────────────────────
            this.pnlPermHdr.BackColor = System.Drawing.Color.FromArgb(240, 244, 250);
            this.pnlPermHdr.Controls.Add(this.lblPermHdrTitle);
            this.pnlPermHdr.Controls.Add(this.lblPermHdrAllow);
            this.pnlPermHdr.Controls.Add(this.lblPermHdrDeny);
            this.pnlPermHdr.Location  = new System.Drawing.Point(0, 0);
            this.pnlPermHdr.Name      = "pnlPermHdr";
            this.pnlPermHdr.Size      = new System.Drawing.Size(434, 27);
            this.pnlPermHdr.TabIndex  = 0;

            this.lblPermHdrTitle.AutoSize  = true;
            this.lblPermHdrTitle.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPermHdrTitle.Location  = new System.Drawing.Point(10, 5);
            this.lblPermHdrTitle.Name      = "lblPermHdrTitle";
            this.lblPermHdrTitle.TabIndex  = 0;
            this.lblPermHdrTitle.Text      = "สิทธิ์";

            this.lblPermHdrAllow.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPermHdrAllow.Location  = new System.Drawing.Point(272, 5);
            this.lblPermHdrAllow.Name      = "lblPermHdrAllow";
            this.lblPermHdrAllow.Size      = new System.Drawing.Size(60, 18);
            this.lblPermHdrAllow.TabIndex  = 1;
            this.lblPermHdrAllow.Text      = "Allow";
            this.lblPermHdrAllow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblPermHdrDeny.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPermHdrDeny.Location  = new System.Drawing.Point(365, 5);
            this.lblPermHdrDeny.Name      = "lblPermHdrDeny";
            this.lblPermHdrDeny.Size      = new System.Drawing.Size(55, 18);
            this.lblPermHdrDeny.TabIndex  = 2;
            this.lblPermHdrDeny.Text      = "Deny";
            this.lblPermHdrDeny.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── Row: Read ─────────────────────────────────────────────────────
            this.pnlRowRead.BackColor = System.Drawing.Color.White;
            this.pnlRowRead.Controls.Add(this.lblPermRead);
            this.pnlRowRead.Controls.Add(this.rdAllowRead);
            this.pnlRowRead.Controls.Add(this.rdDenyRead);
            this.pnlRowRead.Location  = new System.Drawing.Point(0, 28);
            this.pnlRowRead.Name      = "pnlRowRead";
            this.pnlRowRead.Size      = new System.Drawing.Size(434, 29);
            this.pnlRowRead.TabIndex  = 1;

            this.lblPermRead.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPermRead.Location = new System.Drawing.Point(10, 5);
            this.lblPermRead.Name     = "lblPermRead";
            this.lblPermRead.Size     = new System.Drawing.Size(240, 20);
            this.lblPermRead.TabIndex = 0;
            this.lblPermRead.Text     = "📖 อ่าน (Read)";

            this.rdAllowRead.Location = new System.Drawing.Point(291, 5);
            this.rdAllowRead.Name     = "rdAllowRead";
            this.rdAllowRead.Size     = new System.Drawing.Size(20, 20);
            this.rdAllowRead.TabIndex = 1;

            this.rdDenyRead.Location  = new System.Drawing.Point(388, 5);
            this.rdDenyRead.Name      = "rdDenyRead";
            this.rdDenyRead.Size      = new System.Drawing.Size(20, 20);
            this.rdDenyRead.TabIndex  = 2;

            // ── Row: Write ────────────────────────────────────────────────────
            this.pnlRowWrite.BackColor = System.Drawing.Color.FromArgb(248, 250, 255);
            this.pnlRowWrite.Controls.Add(this.lblPermWrite);
            this.pnlRowWrite.Controls.Add(this.rdAllowWrite);
            this.pnlRowWrite.Controls.Add(this.rdDenyWrite);
            this.pnlRowWrite.Location  = new System.Drawing.Point(0, 57);
            this.pnlRowWrite.Name      = "pnlRowWrite";
            this.pnlRowWrite.Size      = new System.Drawing.Size(434, 29);
            this.pnlRowWrite.TabIndex  = 2;

            this.lblPermWrite.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPermWrite.Location = new System.Drawing.Point(10, 5);
            this.lblPermWrite.Name     = "lblPermWrite";
            this.lblPermWrite.Size     = new System.Drawing.Size(240, 20);
            this.lblPermWrite.TabIndex = 0;
            this.lblPermWrite.Text     = "✏️ เขียน (Write)";

            this.rdAllowWrite.Location = new System.Drawing.Point(291, 5);
            this.rdAllowWrite.Name     = "rdAllowWrite";
            this.rdAllowWrite.Size     = new System.Drawing.Size(20, 20);
            this.rdAllowWrite.TabIndex = 1;

            this.rdDenyWrite.Location  = new System.Drawing.Point(388, 5);
            this.rdDenyWrite.Name      = "rdDenyWrite";
            this.rdDenyWrite.Size      = new System.Drawing.Size(20, 20);
            this.rdDenyWrite.TabIndex  = 2;

            // ── Row: Delete ───────────────────────────────────────────────────
            this.pnlRowDelete.BackColor = System.Drawing.Color.White;
            this.pnlRowDelete.Controls.Add(this.lblPermDelete);
            this.pnlRowDelete.Controls.Add(this.rdAllowDelete);
            this.pnlRowDelete.Controls.Add(this.rdDenyDelete);
            this.pnlRowDelete.Location  = new System.Drawing.Point(0, 86);
            this.pnlRowDelete.Name      = "pnlRowDelete";
            this.pnlRowDelete.Size      = new System.Drawing.Size(434, 29);
            this.pnlRowDelete.TabIndex  = 3;

            this.lblPermDelete.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPermDelete.Location = new System.Drawing.Point(10, 5);
            this.lblPermDelete.Name     = "lblPermDelete";
            this.lblPermDelete.Size     = new System.Drawing.Size(240, 20);
            this.lblPermDelete.TabIndex = 0;
            this.lblPermDelete.Text     = "🗑 ลบ (Delete)";

            this.rdAllowDelete.Location = new System.Drawing.Point(291, 5);
            this.rdAllowDelete.Name     = "rdAllowDelete";
            this.rdAllowDelete.Size     = new System.Drawing.Size(20, 20);
            this.rdAllowDelete.TabIndex = 1;

            this.rdDenyDelete.Location  = new System.Drawing.Point(388, 5);
            this.rdDenyDelete.Name      = "rdDenyDelete";
            this.rdDenyDelete.Size      = new System.Drawing.Size(20, 20);
            this.rdDenyDelete.TabIndex  = 2;

            // ── Row: Modify ───────────────────────────────────────────────────
            this.pnlRowModify.BackColor = System.Drawing.Color.FromArgb(248, 250, 255);
            this.pnlRowModify.Controls.Add(this.lblPermModify);
            this.pnlRowModify.Controls.Add(this.rdAllowModify);
            this.pnlRowModify.Controls.Add(this.rdDenyModify);
            this.pnlRowModify.Location  = new System.Drawing.Point(0, 115);
            this.pnlRowModify.Name      = "pnlRowModify";
            this.pnlRowModify.Size      = new System.Drawing.Size(434, 29);
            this.pnlRowModify.TabIndex  = 4;

            this.lblPermModify.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPermModify.Location = new System.Drawing.Point(10, 5);
            this.lblPermModify.Name     = "lblPermModify";
            this.lblPermModify.Size     = new System.Drawing.Size(240, 20);
            this.lblPermModify.TabIndex = 0;
            this.lblPermModify.Text     = "🔧 แก้ไข (Modify)";

            this.rdAllowModify.Location = new System.Drawing.Point(291, 5);
            this.rdAllowModify.Name     = "rdAllowModify";
            this.rdAllowModify.Size     = new System.Drawing.Size(20, 20);
            this.rdAllowModify.TabIndex = 1;

            this.rdDenyModify.Location  = new System.Drawing.Point(388, 5);
            this.rdDenyModify.Name      = "rdDenyModify";
            this.rdDenyModify.Size      = new System.Drawing.Size(20, 20);
            this.rdDenyModify.TabIndex  = 2;

            // ── Row: Full Control ─────────────────────────────────────────────
            this.pnlRowFull.BackColor = System.Drawing.Color.White;
            this.pnlRowFull.Controls.Add(this.lblPermFull);
            this.pnlRowFull.Controls.Add(this.rdAllowFull);
            this.pnlRowFull.Controls.Add(this.rdDenyFull);
            this.pnlRowFull.Location  = new System.Drawing.Point(0, 144);
            this.pnlRowFull.Name      = "pnlRowFull";
            this.pnlRowFull.Size      = new System.Drawing.Size(434, 29);
            this.pnlRowFull.TabIndex  = 5;

            this.lblPermFull.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPermFull.Location = new System.Drawing.Point(10, 5);
            this.lblPermFull.Name     = "lblPermFull";
            this.lblPermFull.Size     = new System.Drawing.Size(240, 20);
            this.lblPermFull.TabIndex = 0;
            this.lblPermFull.Text     = "👑 Full Control";

            this.rdAllowFull.Location = new System.Drawing.Point(291, 5);
            this.rdAllowFull.Name     = "rdAllowFull";
            this.rdAllowFull.Size     = new System.Drawing.Size(20, 20);
            this.rdAllowFull.TabIndex = 1;

            this.rdDenyFull.Location  = new System.Drawing.Point(388, 5);
            this.rdDenyFull.Name      = "rdDenyFull";
            this.rdDenyFull.Size      = new System.Drawing.Size(20, 20);
            this.rdDenyFull.TabIndex  = 2;

            // ── btnOk ─────────────────────────────────────────────────────────
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location  = new System.Drawing.Point(248, 420);
            this.btnOk.Name      = "btnOk";
            this.btnOk.Size      = new System.Drawing.Size(96, 32);
            this.btnOk.TabIndex  = 9;
            this.btnOk.Text      = "บันทึก";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click    += new System.EventHandler(this.btnOk_Click);

            // ── btnCancel ─────────────────────────────────────────────────────
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font         = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.Location     = new System.Drawing.Point(354, 420);
            this.btnCancel.Name         = "btnCancel";
            this.btnCancel.Size         = new System.Drawing.Size(96, 32);
            this.btnCancel.TabIndex     = 10;
            this.btnCancel.Text         = "ยกเลิก";

            // ── FilePermissionAddDialog ───────────────────────────────────────
            this.AcceptButton        = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.CancelButton        = this.btnCancel;
            this.ClientSize          = new System.Drawing.Size(460, 490);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.pnlPerm);
            this.Controls.Add(this.lblPermissions);
            this.Controls.Add(this.cboSelect);
            this.Controls.Add(this.lblSelect);
            this.Controls.Add(this.rdRole);
            this.Controls.Add(this.rdGroup);
            this.Controls.Add(this.rdUser);
            this.Controls.Add(this.lblPrincipalType);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "FilePermissionAddDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "เพิ่ม/แก้ไข สิทธิ์ไฟล์";
            this.Load           += new System.EventHandler(this.FilePermissionAddDialog_Load);

            this.pnlRowRead.ResumeLayout(false);
            this.pnlRowWrite.ResumeLayout(false);
            this.pnlRowDelete.ResumeLayout(false);
            this.pnlRowModify.ResumeLayout(false);
            this.pnlRowFull.ResumeLayout(false);
            this.pnlPermHdr.ResumeLayout(false);
            this.pnlPermHdr.PerformLayout();
            this.pnlPerm.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #region Designer fields
        private System.Windows.Forms.Panel       pnlHeader;
        private System.Windows.Forms.Label       lblHeader;
        private System.Windows.Forms.Label       lblPrincipalType;
        private System.Windows.Forms.RadioButton rdUser;
        private System.Windows.Forms.RadioButton rdGroup;
        private System.Windows.Forms.RadioButton rdRole;
        private System.Windows.Forms.Label       lblSelect;
        private System.Windows.Forms.ComboBox    cboSelect;
        private System.Windows.Forms.Label       lblPermissions;
        private System.Windows.Forms.Panel       pnlPerm;
        private System.Windows.Forms.Panel       pnlPermHdr;
        private System.Windows.Forms.Label       lblPermHdrTitle;
        private System.Windows.Forms.Label       lblPermHdrAllow;
        private System.Windows.Forms.Label       lblPermHdrDeny;
        private System.Windows.Forms.Panel       pnlRowRead;
        private System.Windows.Forms.Label       lblPermRead;
        private System.Windows.Forms.RadioButton rdAllowRead;
        private System.Windows.Forms.RadioButton rdDenyRead;
        private System.Windows.Forms.Panel       pnlRowWrite;
        private System.Windows.Forms.Label       lblPermWrite;
        private System.Windows.Forms.RadioButton rdAllowWrite;
        private System.Windows.Forms.RadioButton rdDenyWrite;
        private System.Windows.Forms.Panel       pnlRowDelete;
        private System.Windows.Forms.Label       lblPermDelete;
        private System.Windows.Forms.RadioButton rdAllowDelete;
        private System.Windows.Forms.RadioButton rdDenyDelete;
        private System.Windows.Forms.Panel       pnlRowModify;
        private System.Windows.Forms.Label       lblPermModify;
        private System.Windows.Forms.RadioButton rdAllowModify;
        private System.Windows.Forms.RadioButton rdDenyModify;
        private System.Windows.Forms.Panel       pnlRowFull;
        private System.Windows.Forms.Label       lblPermFull;
        private System.Windows.Forms.RadioButton rdAllowFull;
        private System.Windows.Forms.RadioButton rdDenyFull;
        private System.Windows.Forms.Button      btnOk;
        private System.Windows.Forms.Button      btnCancel;
        #endregion
    }
}
