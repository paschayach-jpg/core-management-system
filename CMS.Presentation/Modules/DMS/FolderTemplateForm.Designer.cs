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
            this.components         = new System.ComponentModel.Container();

            // ── Declare all controls ─────────────────────────────────────────
            this.pnlHeader          = new System.Windows.Forms.Panel();
            this.lblHeader          = new System.Windows.Forms.Label();

            this.pnlBottom          = new System.Windows.Forms.Panel();
            this.pnlBottomLine      = new System.Windows.Forms.Panel();
            this._btnSave           = new System.Windows.Forms.Button();
            this._btnDeleteTemplate = new System.Windows.Forms.Button();
            this._btnCancel         = new System.Windows.Forms.Button();

            this.pnlMain            = new System.Windows.Forms.Panel();

            this.pnlLeft            = new System.Windows.Forms.Panel();
            this.pnlLeftHeader      = new System.Windows.Forms.Panel();
            this.lblTemplateList    = new System.Windows.Forms.Label();
            this._btnNew            = new System.Windows.Forms.Button();
            this._lstTemplates      = new System.Windows.Forms.ListBox();

            this.divider            = new System.Windows.Forms.Panel();

            this.pnlRight           = new System.Windows.Forms.Panel();
            this.pnlInfo            = new System.Windows.Forms.Panel();
            this.lblName            = new System.Windows.Forms.Label();
            this._txtName           = new System.Windows.Forms.TextBox();
            this.lblType            = new System.Windows.Forms.Label();
            this._cboType           = new System.Windows.Forms.ComboBox();
            this.lblOrgUnit         = new System.Windows.Forms.Label();
            this._cboOrgUnit        = new System.Windows.Forms.ComboBox();
            this.pnlInfoLine        = new System.Windows.Forms.Panel();
            this.pnlSubHeader       = new System.Windows.Forms.Panel();
            this.lblSubFolder       = new System.Windows.Forms.Label();
            this.pnlItemActions     = new System.Windows.Forms.Panel();
            this.pnlItemActionsLine = new System.Windows.Forms.Panel();
            this._btnAddFolder      = new System.Windows.Forms.Button();
            this._btnEditFolder     = new System.Windows.Forms.Button();
            this._btnDeleteFolder   = new System.Windows.Forms.Button();
            this._lstItems          = new System.Windows.Forms.ListView();
            this.colFolderName      = new System.Windows.Forms.ColumnHeader();
            this.colConfidential    = new System.Windows.Forms.ColumnHeader();
            this.colSortOrder       = new System.Windows.Forms.ColumnHeader();
            this.colInherit         = new System.Windows.Forms.ColumnHeader();

            this.pnlHeader.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.pnlSubHeader.SuspendLayout();
            this.pnlItemActions.SuspendLayout();
            this.SuspendLayout();

            // ════════════════════════════════════════════════════════════════
            // pnlHeader  (Dock.Top)
            // ════════════════════════════════════════════════════════════════
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 52;
            this.pnlHeader.Name      = "pnlHeader";
            this.pnlHeader.Controls.Add(this.lblHeader);

            this.lblHeader.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Name      = "lblHeader";
            this.lblHeader.Padding   = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblHeader.Text      = "📋  จัดการ Template Folder";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ════════════════════════════════════════════════════════════════
            // pnlBottom  (Dock.Bottom)
            // ════════════════════════════════════════════════════════════════
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(238, 240, 245);
            this.pnlBottom.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height    = 62;
            this.pnlBottom.Name      = "pnlBottom";
            this.pnlBottom.Controls.Add(this._btnSave);
            this.pnlBottom.Controls.Add(this._btnDeleteTemplate);
            this.pnlBottom.Controls.Add(this._btnCancel);
            this.pnlBottom.Controls.Add(this.pnlBottomLine);

            this.pnlBottomLine.BackColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.pnlBottomLine.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlBottomLine.Height    = 1;
            this.pnlBottomLine.Name      = "pnlBottomLine";

            // บันทึก Template
            this._btnSave.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSave.FlatAppearance.BorderSize = 0;
            this._btnSave.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._btnSave.ForeColor = System.Drawing.Color.White;
            this._btnSave.Location  = new System.Drawing.Point(16, 14);
            this._btnSave.Name      = "_btnSave";
            this._btnSave.Size      = new System.Drawing.Size(160, 34);
            this._btnSave.Text      = "💾  บันทึก Template";
            this._btnSave.UseVisualStyleBackColor = false;
            this._btnSave.Cursor    = System.Windows.Forms.Cursors.Hand;
            this._btnSave.Click    += new System.EventHandler(this.BtnSave_Click);

            // ลบ Template
            this._btnDeleteTemplate.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this._btnDeleteTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDeleteTemplate.FlatAppearance.BorderSize = 0;
            this._btnDeleteTemplate.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnDeleteTemplate.ForeColor = System.Drawing.Color.White;
            this._btnDeleteTemplate.Location  = new System.Drawing.Point(188, 14);
            this._btnDeleteTemplate.Name      = "_btnDeleteTemplate";
            this._btnDeleteTemplate.Size      = new System.Drawing.Size(140, 34);
            this._btnDeleteTemplate.Text      = "🗑  ลบ Template";
            this._btnDeleteTemplate.UseVisualStyleBackColor = false;
            this._btnDeleteTemplate.Cursor    = System.Windows.Forms.Cursors.Hand;
            this._btnDeleteTemplate.Click    += new System.EventHandler(this.BtnDeleteTemplate_Click);

            // ยกเลิก
            this._btnCancel.BackColor = System.Drawing.Color.White;
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(190, 190, 190);
            this._btnCancel.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnCancel.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this._btnCancel.Location  = new System.Drawing.Point(716, 14);
            this._btnCancel.Name      = "_btnCancel";
            this._btnCancel.Size      = new System.Drawing.Size(88, 34);
            this._btnCancel.Text      = "ยกเลิก";
            this._btnCancel.Cursor    = System.Windows.Forms.Cursors.Hand;

            // ════════════════════════════════════════════════════════════════
            // pnlMain  (Dock.Fill)
            // ════════════════════════════════════════════════════════════════
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Controls.Add(this.pnlRight);
            this.pnlMain.Controls.Add(this.divider);
            this.pnlMain.Controls.Add(this.pnlLeft);

            // ── pnlLeft (Dock.Left, W=220) ──────────────────────────────────
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlLeft.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Name      = "pnlLeft";
            this.pnlLeft.Width     = 220;
            this.pnlLeft.Controls.Add(this._lstTemplates);
            this.pnlLeft.Controls.Add(this._btnNew);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);

            // pnlLeftHeader
            this.pnlLeftHeader.BackColor = System.Drawing.Color.FromArgb(235, 238, 245);
            this.pnlLeftHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Height    = 40;
            this.pnlLeftHeader.Name      = "pnlLeftHeader";
            this.pnlLeftHeader.Padding   = new System.Windows.Forms.Padding(10, 0, 8, 0);
            this.pnlLeftHeader.Controls.Add(this.lblTemplateList);

            this.lblTemplateList.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblTemplateList.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTemplateList.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblTemplateList.Name      = "lblTemplateList";
            this.lblTemplateList.Text      = "รายการ Template";
            this.lblTemplateList.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // _btnNew
            this._btnNew.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._btnNew.Dock      = System.Windows.Forms.DockStyle.Top;
            this._btnNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnNew.FlatAppearance.BorderSize = 0;
            this._btnNew.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnNew.ForeColor = System.Drawing.Color.White;
            this._btnNew.Height    = 36;
            this._btnNew.Name      = "_btnNew";
            this._btnNew.Text      = "+  สร้างใหม่";
            this._btnNew.UseVisualStyleBackColor = false;
            this._btnNew.Cursor    = System.Windows.Forms.Cursors.Hand;
            this._btnNew.Click    += new System.EventHandler(this.BtnNew_Click);

            // _lstTemplates
            this._lstTemplates.BorderStyle   = System.Windows.Forms.BorderStyle.None;
            this._lstTemplates.Dock          = System.Windows.Forms.DockStyle.Fill;
            this._lstTemplates.DrawMode      = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._lstTemplates.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this._lstTemplates.ItemHeight    = 38;
            this._lstTemplates.Name          = "_lstTemplates";
            this._lstTemplates.DrawItem            += new System.Windows.Forms.DrawItemEventHandler(this.LstTemplates_DrawItem);
            this._lstTemplates.SelectedIndexChanged += new System.EventHandler(this.LstTemplates_Changed);

            // ── divider ──────────────────────────────────────────────────────
            this.divider.BackColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.divider.Dock      = System.Windows.Forms.DockStyle.Left;
            this.divider.Name      = "divider";
            this.divider.Width     = 1;

            // ── pnlRight (Dock.Fill) ─────────────────────────────────────────
            this.pnlRight.BackColor = System.Drawing.Color.White;
            this.pnlRight.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Name      = "pnlRight";
            this.pnlRight.Controls.Add(this._lstItems);
            this.pnlRight.Controls.Add(this.pnlItemActions);
            this.pnlRight.Controls.Add(this.pnlSubHeader);
            this.pnlRight.Controls.Add(this.pnlInfoLine);
            this.pnlRight.Controls.Add(this.pnlInfo);

            // pnlInfo (Dock.Top, H=132)
            this.pnlInfo.BackColor = System.Drawing.Color.FromArgb(250, 251, 253);
            this.pnlInfo.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlInfo.Height    = 132;
            this.pnlInfo.Name      = "pnlInfo";
            this.pnlInfo.Padding   = new System.Windows.Forms.Padding(16, 12, 16, 8);
            this.pnlInfo.Controls.Add(this.lblName);
            this.pnlInfo.Controls.Add(this._txtName);
            this.pnlInfo.Controls.Add(this.lblType);
            this.pnlInfo.Controls.Add(this._cboType);
            this.pnlInfo.Controls.Add(this.lblOrgUnit);
            this.pnlInfo.Controls.Add(this._cboOrgUnit);

            // ชื่อ Template row
            this.lblName.AutoSize  = true;
            this.lblName.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.lblName.Location  = new System.Drawing.Point(16, 16);
            this.lblName.Name      = "lblName";
            this.lblName.Text      = "ชื่อ Template:";

            this._txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtName.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this._txtName.Location    = new System.Drawing.Point(120, 12);
            this._txtName.Name        = "_txtName";
            this._txtName.Size        = new System.Drawing.Size(440, 24);
            this._txtName.TabIndex    = 0;
            this._txtName.Anchor      = System.Windows.Forms.AnchorStyles.Top
                                      | System.Windows.Forms.AnchorStyles.Left
                                      | System.Windows.Forms.AnchorStyles.Right;

            // ประเภท row
            this.lblType.AutoSize  = true;
            this.lblType.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.lblType.Location  = new System.Drawing.Point(16, 52);
            this.lblType.Name      = "lblType";
            this.lblType.Text      = "ประเภท:";

            this._cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboType.Font          = new System.Drawing.Font("Segoe UI", 9.5F);
            this._cboType.Items.AddRange(new object[] { "Department", "Division", "SubCommittee", "Project", "Custom" });
            this._cboType.Location      = new System.Drawing.Point(120, 48);
            this._cboType.Name          = "_cboType";
            this._cboType.Size          = new System.Drawing.Size(220, 26);
            this._cboType.TabIndex      = 1;
            this._cboType.SelectedIndex = 0;
            this._cboType.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;

            // หน่วยงาน row
            this.lblOrgUnit.AutoSize  = true;
            this.lblOrgUnit.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblOrgUnit.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.lblOrgUnit.Location  = new System.Drawing.Point(16, 90);
            this.lblOrgUnit.Name      = "lblOrgUnit";
            this.lblOrgUnit.Text      = "หน่วยงาน:";

            this._cboOrgUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboOrgUnit.Font          = new System.Drawing.Font("Segoe UI", 9.5F);
            this._cboOrgUnit.Location      = new System.Drawing.Point(120, 86);
            this._cboOrgUnit.Name          = "_cboOrgUnit";
            this._cboOrgUnit.Size          = new System.Drawing.Size(340, 26);
            this._cboOrgUnit.TabIndex      = 2;
            this._cboOrgUnit.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;

            // pnlInfoLine (Dock.Top, H=1) — separator
            this.pnlInfoLine.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.pnlInfoLine.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlInfoLine.Height    = 1;
            this.pnlInfoLine.Name      = "pnlInfoLine";

            // pnlSubHeader (Dock.Top, H=34)
            this.pnlSubHeader.BackColor = System.Drawing.Color.FromArgb(245, 247, 252);
            this.pnlSubHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlSubHeader.Height    = 34;
            this.pnlSubHeader.Name      = "pnlSubHeader";
            this.pnlSubHeader.Padding   = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.pnlSubHeader.Controls.Add(this.lblSubFolder);

            this.lblSubFolder.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblSubFolder.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSubFolder.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblSubFolder.Name      = "lblSubFolder";
            this.lblSubFolder.Text      = "โครงสร้าง Sub-Folder:";
            this.lblSubFolder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // pnlItemActions (Dock.Bottom, H=52)
            this.pnlItemActions.BackColor = System.Drawing.Color.FromArgb(248, 249, 252);
            this.pnlItemActions.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlItemActions.Height    = 52;
            this.pnlItemActions.Name      = "pnlItemActions";
            this.pnlItemActions.Padding   = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlItemActions.Controls.Add(this.pnlItemActionsLine);
            this.pnlItemActions.Controls.Add(this._btnAddFolder);
            this.pnlItemActions.Controls.Add(this._btnEditFolder);
            this.pnlItemActions.Controls.Add(this._btnDeleteFolder);

            this.pnlItemActionsLine.BackColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.pnlItemActionsLine.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlItemActionsLine.Height    = 1;
            this.pnlItemActionsLine.Name      = "pnlItemActionsLine";

            // + เพิ่ม Folder
            this._btnAddFolder.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._btnAddFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnAddFolder.FlatAppearance.BorderSize = 0;
            this._btnAddFolder.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnAddFolder.ForeColor = System.Drawing.Color.White;
            this._btnAddFolder.Location  = new System.Drawing.Point(12, 12);
            this._btnAddFolder.Name      = "_btnAddFolder";
            this._btnAddFolder.Size      = new System.Drawing.Size(120, 30);
            this._btnAddFolder.Text      = "+  เพิ่ม Folder";
            this._btnAddFolder.UseVisualStyleBackColor = false;
            this._btnAddFolder.Cursor    = System.Windows.Forms.Cursors.Hand;
            this._btnAddFolder.Click    += new System.EventHandler(this.BtnAddFolder_Click);

            // ✏ แก้ไข
            this._btnEditFolder.BackColor = System.Drawing.Color.White;
            this._btnEditFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnEditFolder.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._btnEditFolder.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnEditFolder.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._btnEditFolder.Location  = new System.Drawing.Point(142, 12);
            this._btnEditFolder.Name      = "_btnEditFolder";
            this._btnEditFolder.Size      = new System.Drawing.Size(100, 30);
            this._btnEditFolder.Text      = "✏  แก้ไข";
            this._btnEditFolder.UseVisualStyleBackColor = false;
            this._btnEditFolder.Cursor    = System.Windows.Forms.Cursors.Hand;
            this._btnEditFolder.Click    += new System.EventHandler(this.BtnEditFolder_Click);

            // 🗑 ลบ
            this._btnDeleteFolder.BackColor = System.Drawing.Color.FromArgb(255, 245, 245);
            this._btnDeleteFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDeleteFolder.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this._btnDeleteFolder.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this._btnDeleteFolder.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this._btnDeleteFolder.Location  = new System.Drawing.Point(252, 12);
            this._btnDeleteFolder.Name      = "_btnDeleteFolder";
            this._btnDeleteFolder.Size      = new System.Drawing.Size(90, 30);
            this._btnDeleteFolder.Text      = "🗑  ลบ";
            this._btnDeleteFolder.UseVisualStyleBackColor = false;
            this._btnDeleteFolder.Cursor    = System.Windows.Forms.Cursors.Hand;
            this._btnDeleteFolder.Click    += new System.EventHandler(this.BtnDeleteFolder_Click);

            // ── _lstItems (Dock.Fill) ────────────────────────────────────────
            this._lstItems.BackColor    = System.Drawing.Color.White;
            this._lstItems.BorderStyle  = System.Windows.Forms.BorderStyle.None;
            this._lstItems.Dock         = System.Windows.Forms.DockStyle.Fill;
            this._lstItems.Font         = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lstItems.FullRowSelect = true;
            this._lstItems.GridLines    = false;
            this._lstItems.HeaderStyle  = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._lstItems.Name         = "_lstItems";
            this._lstItems.View         = System.Windows.Forms.View.Details;
            this._lstItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                this.colFolderName, this.colConfidential, this.colSortOrder, this.colInherit
            });

            this.colFolderName.Text   = "ชื่อ Folder";   this.colFolderName.Width   = 290;
            this.colConfidential.Text = "ระดับ";          this.colConfidential.Width = 130;
            this.colSortOrder.Text    = "ลำดับ";           this.colSortOrder.Width    = 60;
            this.colInherit.Text      = "Inherit";         this.colInherit.Width      = 60;

            // ════════════════════════════════════════════════════════════════
            // FolderTemplateForm
            // ════════════════════════════════════════════════════════════════
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.CancelButton        = this._btnCancel;
            this.ClientSize          = new System.Drawing.Size(830, 560);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "FolderTemplateForm";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "จัดการ Template Folder";
            this.Load               += new System.EventHandler(this.FolderTemplateForm_Load);

            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlHeader);

            this.pnlItemActions.ResumeLayout(false);
            this.pnlSubHeader.ResumeLayout(false);
            this.pnlInfo.ResumeLayout(false);
            this.pnlInfo.PerformLayout();
            this.pnlRight.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #region Designer fields
        private System.Windows.Forms.Panel        pnlHeader;
        private System.Windows.Forms.Label        lblHeader;
        private System.Windows.Forms.Panel        pnlBottom;
        private System.Windows.Forms.Panel        pnlBottomLine;
        private System.Windows.Forms.Button       _btnSave;
        private System.Windows.Forms.Button       _btnDeleteTemplate;
        private System.Windows.Forms.Button       _btnCancel;
        private System.Windows.Forms.Panel        pnlMain;
        private System.Windows.Forms.Panel        pnlLeft;
        private System.Windows.Forms.Panel        pnlLeftHeader;
        private System.Windows.Forms.Label        lblTemplateList;
        private System.Windows.Forms.Button       _btnNew;
        private System.Windows.Forms.ListBox      _lstTemplates;
        private System.Windows.Forms.Panel        divider;
        private System.Windows.Forms.Panel        pnlRight;
        private System.Windows.Forms.Panel        pnlInfo;
        private System.Windows.Forms.Label        lblName;
        private System.Windows.Forms.TextBox      _txtName;
        private System.Windows.Forms.Label        lblType;
        private System.Windows.Forms.ComboBox     _cboType;
        private System.Windows.Forms.Label        lblOrgUnit;
        private System.Windows.Forms.ComboBox     _cboOrgUnit;
        private System.Windows.Forms.Panel        pnlInfoLine;
        private System.Windows.Forms.Panel        pnlSubHeader;
        private System.Windows.Forms.Label        lblSubFolder;
        private System.Windows.Forms.Panel        pnlItemActions;
        private System.Windows.Forms.Panel        pnlItemActionsLine;
        private System.Windows.Forms.Button       _btnAddFolder;
        private System.Windows.Forms.Button       _btnEditFolder;
        private System.Windows.Forms.Button       _btnDeleteFolder;
        private System.Windows.Forms.ListView     _lstItems;
        private System.Windows.Forms.ColumnHeader colFolderName;
        private System.Windows.Forms.ColumnHeader colConfidential;
        private System.Windows.Forms.ColumnHeader colSortOrder;
        private System.Windows.Forms.ColumnHeader colInherit;
        #endregion
    }
}
