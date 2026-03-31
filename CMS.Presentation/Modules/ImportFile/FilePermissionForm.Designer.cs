namespace CMS.Presentation
{
    partial class FilePermissionForm
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
            this.components    = new System.ComponentModel.Container();
            this.pnlHeader     = new System.Windows.Forms.Panel();
            this._lblTitle     = new System.Windows.Forms.Label();
            this.pnlInfo       = new System.Windows.Forms.Panel();
            this._lblFileInfo  = new System.Windows.Forms.Label();
            this.lblAclHeader  = new System.Windows.Forms.Label();
            this._lstAcl       = new System.Windows.Forms.ListView();
            this.colType       = new System.Windows.Forms.ColumnHeader();
            this.colPrincipal  = new System.Windows.Forms.ColumnHeader();
            this.colRead       = new System.Windows.Forms.ColumnHeader();
            this.colWrite      = new System.Windows.Forms.ColumnHeader();
            this.colDelete     = new System.Windows.Forms.ColumnHeader();
            this.colModify     = new System.Windows.Forms.ColumnHeader();
            this.colFull       = new System.Windows.Forms.ColumnHeader();
            this.colDeny       = new System.Windows.Forms.ColumnHeader();
            this.colExpiry     = new System.Windows.Forms.ColumnHeader();
            this.colGrantedBy  = new System.Windows.Forms.ColumnHeader();
            this._btnAdd       = new System.Windows.Forms.Button();
            this._btnRemove    = new System.Windows.Forms.Button();
            this._btnSyncFolder = new System.Windows.Forms.Button();
            this._btnClose     = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this._lblTitle);
            this.pnlHeader.Dock     = System.Windows.Forms.DockStyle.None;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name     = "pnlHeader";
            this.pnlHeader.Size     = new System.Drawing.Size(700, 44);
            this.pnlHeader.TabIndex = 0;

            // ── _lblTitle ────────────────────────────────────────────────────
            this._lblTitle.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.White;
            this._lblTitle.Location  = new System.Drawing.Point(12, 0);
            this._lblTitle.Name      = "_lblTitle";
            this._lblTitle.Size      = new System.Drawing.Size(660, 44);
            this._lblTitle.TabIndex  = 0;
            this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── pnlInfo ───────────────────────────────────────────────────────
            this.pnlInfo.BackColor   = System.Drawing.Color.FromArgb(232, 240, 254);
            this.pnlInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlInfo.Controls.Add(this._lblFileInfo);
            this.pnlInfo.Location    = new System.Drawing.Point(12, 52);
            this.pnlInfo.Name        = "pnlInfo";
            this.pnlInfo.Size        = new System.Drawing.Size(676, 32);
            this.pnlInfo.TabIndex    = 1;

            // ── _lblFileInfo ──────────────────────────────────────────────────
            this._lblFileInfo.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._lblFileInfo.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._lblFileInfo.Location  = new System.Drawing.Point(8, 0);
            this._lblFileInfo.Name      = "_lblFileInfo";
            this._lblFileInfo.Size      = new System.Drawing.Size(660, 30);
            this._lblFileInfo.TabIndex  = 0;
            this._lblFileInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── lblAclHeader ──────────────────────────────────────────────────
            this.lblAclHeader.AutoSize  = true;
            this.lblAclHeader.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAclHeader.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.lblAclHeader.Location  = new System.Drawing.Point(12, 94);
            this.lblAclHeader.Name      = "lblAclHeader";
            this.lblAclHeader.TabIndex  = 2;
            this.lblAclHeader.Text      = "รายการสิทธิ์ปัจจุบัน:";

            // ── _lstAcl ───────────────────────────────────────────────────────
            this._lstAcl.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colType, this.colPrincipal,
                this.colRead, this.colWrite, this.colDelete, this.colModify,
                this.colFull, this.colDeny,
                this.colExpiry, this.colGrantedBy
            });
            this._lstAcl.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this._lstAcl.FullRowSelect = true;
            this._lstAcl.GridLines     = true;
            this._lstAcl.Location      = new System.Drawing.Point(12, 114);
            this._lstAcl.Name          = "_lstAcl";
            this._lstAcl.Size          = new System.Drawing.Size(676, 280);
            this._lstAcl.TabIndex      = 3;
            this._lstAcl.View          = System.Windows.Forms.View.Details;

            // Columns
            this.colType.Text      = "ประเภท";     this.colType.Width     = 80;
            this.colPrincipal.Text = "Principal";  this.colPrincipal.Width = 160;
            this.colRead.Text      = "Read";        this.colRead.Width      = 50;
            this.colWrite.Text     = "Write";       this.colWrite.Width     = 50;
            this.colDelete.Text    = "Delete";      this.colDelete.Width    = 50;
            this.colModify.Text    = "Modify";      this.colModify.Width    = 50;
            this.colFull.Text      = "Full";        this.colFull.Width      = 50;
            this.colDeny.Text      = "Deny";        this.colDeny.Width      = 50;
            this.colExpiry.Text    = "วันหมดอายุ"; this.colExpiry.Width    = 100;
            this.colGrantedBy.Text = "ให้สิทธิ์โดย"; this.colGrantedBy.Width = 100;

            // ── _btnAdd ───────────────────────────────────────────────────────
            this._btnAdd.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnAdd.FlatAppearance.BorderSize = 0;
            this._btnAdd.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnAdd.ForeColor = System.Drawing.Color.White;
            this._btnAdd.Location  = new System.Drawing.Point(12, 406);
            this._btnAdd.Name      = "_btnAdd";
            this._btnAdd.Size      = new System.Drawing.Size(110, 32);
            this._btnAdd.TabIndex  = 4;
            this._btnAdd.Text      = "➕ เพิ่มสิทธิ์";
            this._btnAdd.UseVisualStyleBackColor = false;
            this._btnAdd.Click    += new System.EventHandler(this.BtnAdd_Click);

            // ── _btnRemove ────────────────────────────────────────────────────
            this._btnRemove.BackColor = System.Drawing.Color.FromArgb(180, 50, 50);
            this._btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnRemove.FlatAppearance.BorderSize = 0;
            this._btnRemove.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnRemove.ForeColor = System.Drawing.Color.White;
            this._btnRemove.Location  = new System.Drawing.Point(132, 406);
            this._btnRemove.Name      = "_btnRemove";
            this._btnRemove.Size      = new System.Drawing.Size(110, 32);
            this._btnRemove.TabIndex  = 5;
            this._btnRemove.Text      = "🗑 ลบสิทธิ์";
            this._btnRemove.UseVisualStyleBackColor = false;
            this._btnRemove.Click    += new System.EventHandler(this.BtnRemove_Click);

            // ── _btnSyncFolder ────────────────────────────────────────────────
            this._btnSyncFolder.BackColor = System.Drawing.Color.FromArgb(0, 120, 80);
            this._btnSyncFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSyncFolder.FlatAppearance.BorderSize = 0;
            this._btnSyncFolder.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnSyncFolder.ForeColor = System.Drawing.Color.White;
            this._btnSyncFolder.Location  = new System.Drawing.Point(252, 406);
            this._btnSyncFolder.Name      = "_btnSyncFolder";
            this._btnSyncFolder.Size      = new System.Drawing.Size(150, 32);
            this._btnSyncFolder.TabIndex  = 6;
            this._btnSyncFolder.Text      = "🔄 ซิงค์จาก Folder";
            this._btnSyncFolder.UseVisualStyleBackColor = false;
            this._btnSyncFolder.Click    += new System.EventHandler(this.BtnSyncFolder_Click);

            // ── _btnClose ─────────────────────────────────────────────────────
            this._btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnClose.Font         = new System.Drawing.Font("Segoe UI", 9F);
            this._btnClose.Location     = new System.Drawing.Point(590, 406);
            this._btnClose.Name         = "_btnClose";
            this._btnClose.Size         = new System.Drawing.Size(98, 32);
            this._btnClose.TabIndex     = 6;
            this._btnClose.Text         = "ปิด";

            // ── FilePermissionForm ────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.CancelButton        = this._btnClose;
            this.ClientSize          = new System.Drawing.Size(700, 480);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._btnSyncFolder);
            this.Controls.Add(this._btnRemove);
            this.Controls.Add(this._btnAdd);
            this.Controls.Add(this._lstAcl);
            this.Controls.Add(this.lblAclHeader);
            this.Controls.Add(this.pnlInfo);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "FilePermissionForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "ตั้งค่าสิทธิ์ไฟล์";
            this.Load           += new System.EventHandler(this.FilePermissionForm_Load);

            this.pnlHeader.ResumeLayout(false);
            this.pnlInfo.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #region Designer fields
        private System.Windows.Forms.Panel        pnlHeader;
        private System.Windows.Forms.Label        _lblTitle;
        private System.Windows.Forms.Panel        pnlInfo;
        private System.Windows.Forms.Label        _lblFileInfo;
        private System.Windows.Forms.Label        lblAclHeader;
        private System.Windows.Forms.ListView     _lstAcl;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colPrincipal;
        private System.Windows.Forms.ColumnHeader colRead;
        private System.Windows.Forms.ColumnHeader colWrite;
        private System.Windows.Forms.ColumnHeader colDelete;
        private System.Windows.Forms.ColumnHeader colModify;
        private System.Windows.Forms.ColumnHeader colFull;
        private System.Windows.Forms.ColumnHeader colDeny;
        private System.Windows.Forms.ColumnHeader colExpiry;
        private System.Windows.Forms.ColumnHeader colGrantedBy;
        private System.Windows.Forms.Button       _btnAdd;
        private System.Windows.Forms.Button       _btnRemove;
        private System.Windows.Forms.Button       _btnSyncFolder;
        private System.Windows.Forms.Button       _btnClose;
        #endregion
    }
}
