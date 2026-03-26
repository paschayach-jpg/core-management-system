namespace CMS.Presentation
{
    partial class FolderTemplateForm
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
            this.components          = new System.ComponentModel.Container();
            this.pnlHeader           = new System.Windows.Forms.Panel();
            this.lblHeader           = new System.Windows.Forms.Label();
            this.pnlLeft             = new System.Windows.Forms.Panel();
            this.lblTemplateList     = new System.Windows.Forms.Label();
            this._btnNew             = new System.Windows.Forms.Button();
            this._lstTemplates       = new System.Windows.Forms.ListBox();
            this.divider             = new System.Windows.Forms.Panel();
            this.lblName             = new System.Windows.Forms.Label();
            this._txtName            = new System.Windows.Forms.TextBox();
            this.lblType             = new System.Windows.Forms.Label();
            this._cboType            = new System.Windows.Forms.ComboBox();
            this.lblSubFolder        = new System.Windows.Forms.Label();
            this._lstItems           = new System.Windows.Forms.ListView();
            this.colFolderName       = new System.Windows.Forms.ColumnHeader();
            this.colConfidential     = new System.Windows.Forms.ColumnHeader();
            this.colSortOrder        = new System.Windows.Forms.ColumnHeader();
            this.colInherit          = new System.Windows.Forms.ColumnHeader();
            this._btnAddFolder       = new System.Windows.Forms.Button();
            this._btnEditFolder      = new System.Windows.Forms.Button();
            this._btnDeleteFolder    = new System.Windows.Forms.Button();
            this.pnlBottom           = new System.Windows.Forms.Panel();
            this._btnSave            = new System.Windows.Forms.Button();
            this._btnDeleteTemplate  = new System.Windows.Forms.Button();
            this._btnCancel          = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Location  = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name      = "pnlHeader";
            this.pnlHeader.Size      = new System.Drawing.Size(820, 44);
            this.pnlHeader.TabIndex  = 0;

            // ── lblHeader ────────────────────────────────────────────────────
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location  = new System.Drawing.Point(12, 0);
            this.lblHeader.Name      = "lblHeader";
            this.lblHeader.Size      = new System.Drawing.Size(780, 44);
            this.lblHeader.TabIndex  = 0;
            this.lblHeader.Text      = "📋 จัดการ Template Folder";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── pnlLeft ───────────────────────────────────────────────────────
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlLeft.Controls.Add(this.lblTemplateList);
            this.pnlLeft.Controls.Add(this._btnNew);
            this.pnlLeft.Controls.Add(this._lstTemplates);
            this.pnlLeft.Location  = new System.Drawing.Point(0, 44);
            this.pnlLeft.Name      = "pnlLeft";
            this.pnlLeft.Size      = new System.Drawing.Size(240, 516);
            this.pnlLeft.TabIndex  = 1;

            // ── lblTemplateList ───────────────────────────────────────────────
            this.lblTemplateList.AutoSize  = true;
            this.lblTemplateList.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTemplateList.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.lblTemplateList.Location  = new System.Drawing.Point(8, 8);
            this.lblTemplateList.Name      = "lblTemplateList";
            this.lblTemplateList.TabIndex  = 0;
            this.lblTemplateList.Text      = "รายการ Template";

            // ── _btnNew ───────────────────────────────────────────────────────
            this._btnNew.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._btnNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnNew.FlatAppearance.BorderSize = 0;
            this._btnNew.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnNew.ForeColor = System.Drawing.Color.White;
            this._btnNew.Location  = new System.Drawing.Point(8, 30);
            this._btnNew.Name      = "_btnNew";
            this._btnNew.Size      = new System.Drawing.Size(100, 28);
            this._btnNew.TabIndex  = 1;
            this._btnNew.Text      = "+ สร้างใหม่";
            this._btnNew.UseVisualStyleBackColor = false;
            this._btnNew.Click    += new System.EventHandler(this.BtnNew_Click);

            // ── _lstTemplates ─────────────────────────────────────────────────
            this._lstTemplates.BorderStyle   = System.Windows.Forms.BorderStyle.None;
            this._lstTemplates.DrawMode      = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._lstTemplates.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this._lstTemplates.ItemHeight    = 36;
            this._lstTemplates.Location      = new System.Drawing.Point(0, 66);
            this._lstTemplates.Name          = "_lstTemplates";
            this._lstTemplates.Size          = new System.Drawing.Size(240, 450);
            this._lstTemplates.TabIndex      = 2;
            this._lstTemplates.DrawItem            += new System.Windows.Forms.DrawItemEventHandler(this.LstTemplates_DrawItem);
            this._lstTemplates.SelectedIndexChanged += new System.EventHandler(this.LstTemplates_Changed);

            // ── divider ───────────────────────────────────────────────────────
            this.divider.BackColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.divider.Location  = new System.Drawing.Point(240, 44);
            this.divider.Name      = "divider";
            this.divider.Size      = new System.Drawing.Size(1, 516);
            this.divider.TabIndex  = 2;

            // ── lblName ───────────────────────────────────────────────────────
            this.lblName.AutoSize = true;
            this.lblName.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblName.Location = new System.Drawing.Point(252, 56);
            this.lblName.Name     = "lblName";
            this.lblName.TabIndex = 3;
            this.lblName.Text     = "ชื่อ Template:";

            // ── _txtName ──────────────────────────────────────────────────────
            this._txtName.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this._txtName.Location = new System.Drawing.Point(362, 52);
            this._txtName.Name     = "_txtName";
            this._txtName.Size     = new System.Drawing.Size(440, 23);
            this._txtName.TabIndex = 4;

            // ── lblType ───────────────────────────────────────────────────────
            this.lblType.AutoSize = true;
            this.lblType.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblType.Location = new System.Drawing.Point(252, 90);
            this.lblType.Name     = "lblType";
            this.lblType.TabIndex = 5;
            this.lblType.Text     = "ประเภท:";

            // ── _cboType ──────────────────────────────────────────────────────
            this._cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboType.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this._cboType.Items.AddRange(new object[] { "Department", "Division", "SubCommittee", "Project", "Custom" });
            this._cboType.Location      = new System.Drawing.Point(362, 86);
            this._cboType.Name          = "_cboType";
            this._cboType.Size          = new System.Drawing.Size(200, 23);
            this._cboType.TabIndex      = 6;
            this._cboType.SelectedIndex = 0;

            // ── lblSubFolder ──────────────────────────────────────────────────
            this.lblSubFolder.AutoSize = true;
            this.lblSubFolder.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSubFolder.Location = new System.Drawing.Point(252, 124);
            this.lblSubFolder.Name     = "lblSubFolder";
            this.lblSubFolder.TabIndex = 7;
            this.lblSubFolder.Text     = "โครงสร้าง Sub-Folder:";

            // ── _lstItems ─────────────────────────────────────────────────────
            this._lstItems.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this._lstItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colFolderName, this.colConfidential, this.colSortOrder, this.colInherit
            });
            this._lstItems.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this._lstItems.FullRowSelect = true;
            this._lstItems.GridLines     = true;
            this._lstItems.Location      = new System.Drawing.Point(252, 144);
            this._lstItems.Name          = "_lstItems";
            this._lstItems.Size          = new System.Drawing.Size(552, 260);
            this._lstItems.TabIndex      = 8;
            this._lstItems.View          = System.Windows.Forms.View.Details;

            // Columns
            this.colFolderName.Text    = "ชื่อ Folder";      this.colFolderName.Width    = 280;
            this.colConfidential.Text  = "ระดับความลับ";    this.colConfidential.Width  = 120;
            this.colSortOrder.Text     = "ลำดับ";            this.colSortOrder.Width     = 60;
            this.colInherit.Text       = "Inherit";           this.colInherit.Width       = 80;

            // ── _btnAddFolder ─────────────────────────────────────────────────
            this._btnAddFolder.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._btnAddFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnAddFolder.FlatAppearance.BorderSize = 0;
            this._btnAddFolder.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnAddFolder.ForeColor = System.Drawing.Color.White;
            this._btnAddFolder.Location  = new System.Drawing.Point(252, 414);
            this._btnAddFolder.Name      = "_btnAddFolder";
            this._btnAddFolder.Size      = new System.Drawing.Size(110, 30);
            this._btnAddFolder.TabIndex  = 9;
            this._btnAddFolder.Text      = "+ เพิ่ม Folder";
            this._btnAddFolder.UseVisualStyleBackColor = false;
            this._btnAddFolder.Click    += new System.EventHandler(this.BtnAddFolder_Click);

            // ── _btnEditFolder ────────────────────────────────────────────────
            this._btnEditFolder.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this._btnEditFolder.Location = new System.Drawing.Point(372, 414);
            this._btnEditFolder.Name     = "_btnEditFolder";
            this._btnEditFolder.Size     = new System.Drawing.Size(90, 30);
            this._btnEditFolder.TabIndex = 10;
            this._btnEditFolder.Text     = "✏ แก้ไข";
            this._btnEditFolder.Click   += new System.EventHandler(this.BtnEditFolder_Click);

            // ── _btnDeleteFolder ──────────────────────────────────────────────
            this._btnDeleteFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDeleteFolder.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnDeleteFolder.ForeColor = System.Drawing.Color.FromArgb(180, 50, 50);
            this._btnDeleteFolder.Location  = new System.Drawing.Point(472, 414);
            this._btnDeleteFolder.Name      = "_btnDeleteFolder";
            this._btnDeleteFolder.Size      = new System.Drawing.Size(80, 30);
            this._btnDeleteFolder.TabIndex  = 11;
            this._btnDeleteFolder.Text      = "🗑 ลบ";
            this._btnDeleteFolder.Click    += new System.EventHandler(this.BtnDeleteFolder_Click);

            // ── pnlBottom ─────────────────────────────────────────────────────
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlBottom.Controls.Add(this._btnSave);
            this.pnlBottom.Controls.Add(this._btnDeleteTemplate);
            this.pnlBottom.Controls.Add(this._btnCancel);
            this.pnlBottom.Location  = new System.Drawing.Point(0, 500);
            this.pnlBottom.Name      = "pnlBottom";
            this.pnlBottom.Size      = new System.Drawing.Size(820, 60);
            this.pnlBottom.TabIndex  = 12;

            // ── _btnSave ──────────────────────────────────────────────────────
            this._btnSave.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSave.FlatAppearance.BorderSize = 0;
            this._btnSave.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnSave.ForeColor = System.Drawing.Color.White;
            this._btnSave.Location  = new System.Drawing.Point(252, 14);
            this._btnSave.Name      = "_btnSave";
            this._btnSave.Size      = new System.Drawing.Size(150, 34);
            this._btnSave.TabIndex  = 0;
            this._btnSave.Text      = "💾 บันทึก Template";
            this._btnSave.UseVisualStyleBackColor = false;
            this._btnSave.Click    += new System.EventHandler(this.BtnSave_Click);

            // ── _btnDeleteTemplate ────────────────────────────────────────────
            this._btnDeleteTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDeleteTemplate.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnDeleteTemplate.ForeColor = System.Drawing.Color.FromArgb(180, 50, 50);
            this._btnDeleteTemplate.Location  = new System.Drawing.Point(412, 14);
            this._btnDeleteTemplate.Name      = "_btnDeleteTemplate";
            this._btnDeleteTemplate.Size      = new System.Drawing.Size(120, 34);
            this._btnDeleteTemplate.TabIndex  = 1;
            this._btnDeleteTemplate.Text      = "🗑 ลบ Template";
            this._btnDeleteTemplate.Click    += new System.EventHandler(this.BtnDeleteTemplate_Click);

            // ── _btnCancel ────────────────────────────────────────────────────
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Font         = new System.Drawing.Font("Segoe UI", 9F);
            this._btnCancel.Location     = new System.Drawing.Point(720, 14);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(88, 34);
            this._btnCancel.TabIndex     = 2;
            this._btnCancel.Text         = "ยกเลิก";

            // ── FolderTemplateForm ────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.CancelButton        = this._btnCancel;
            this.ClientSize          = new System.Drawing.Size(820, 560);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this._btnDeleteFolder);
            this.Controls.Add(this._btnEditFolder);
            this.Controls.Add(this._btnAddFolder);
            this.Controls.Add(this._lstItems);
            this.Controls.Add(this.lblSubFolder);
            this.Controls.Add(this._cboType);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this._txtName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.divider);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "FolderTemplateForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "จัดการ Template Folder";
            this.Load           += new System.EventHandler(this.FolderTemplateForm_Load);

            this.pnlBottom.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeft.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #region Designer fields
        private System.Windows.Forms.Panel        pnlHeader;
        private System.Windows.Forms.Label        lblHeader;
        private System.Windows.Forms.Panel        pnlLeft;
        private System.Windows.Forms.Label        lblTemplateList;
        private System.Windows.Forms.Button       _btnNew;
        private System.Windows.Forms.ListBox      _lstTemplates;
        private System.Windows.Forms.Panel        divider;
        private System.Windows.Forms.Label        lblName;
        private System.Windows.Forms.TextBox      _txtName;
        private System.Windows.Forms.Label        lblType;
        private System.Windows.Forms.ComboBox     _cboType;
        private System.Windows.Forms.Label        lblSubFolder;
        private System.Windows.Forms.ListView     _lstItems;
        private System.Windows.Forms.ColumnHeader colFolderName;
        private System.Windows.Forms.ColumnHeader colConfidential;
        private System.Windows.Forms.ColumnHeader colSortOrder;
        private System.Windows.Forms.ColumnHeader colInherit;
        private System.Windows.Forms.Button       _btnAddFolder;
        private System.Windows.Forms.Button       _btnEditFolder;
        private System.Windows.Forms.Button       _btnDeleteFolder;
        private System.Windows.Forms.Panel        pnlBottom;
        private System.Windows.Forms.Button       _btnSave;
        private System.Windows.Forms.Button       _btnDeleteTemplate;
        private System.Windows.Forms.Button       _btnCancel;
        #endregion
    }
}
