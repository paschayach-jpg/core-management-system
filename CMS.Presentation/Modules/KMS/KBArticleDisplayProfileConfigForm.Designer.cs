namespace CMS.Presentation.Modules.KMS
{
    partial class KBArticleDisplayProfileConfigForm
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
            this.btnBack = new System.Windows.Forms.Button();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.dgvColumnMapping = new System.Windows.Forms.DataGridView();
            this.colSlotDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDisplayLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.pnlBottomSection = new System.Windows.Forms.Panel();
            this.pnlCategoryAssignment = new System.Windows.Forms.Panel();
            this.clbCategories = new System.Windows.Forms.CheckedListBox();
            this.lblCategoryAssignmentTitle = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlEditCard = new System.Windows.Forms.Panel();
            this.lblProfileDescription = new System.Windows.Forms.Label();
            this.txtProfileDescription = new System.Windows.Forms.TextBox();
            this.lblProfileName = new System.Windows.Forms.Label();
            this.txtProfileName = new System.Windows.Forms.TextBox();
            this.divider = new System.Windows.Forms.Panel();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.dgvProfiles = new System.Windows.Forms.DataGridView();
            this.colProfileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.btnNewProfile = new System.Windows.Forms.Button();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnMapping)).BeginInit();
            this.pnlBottomSection.SuspendLayout();
            this.pnlCategoryAssignment.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlEditCard.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProfiles)).BeginInit();
            this.pnlLeftHeader.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Controls.Add(this.btnBack);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1200, 50);
            this.pnlHeader.TabIndex = 0;
            //
            // lblHeader
            //
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(90, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(1110, 50);
            this.lblHeader.TabIndex = 1;
            this.lblHeader.Text = "  🏷️ ตั้งค่า Article Display Profile — Knowledge Base (MOD-006)";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnBack
            //
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(75)))), ((int)(((byte)(130)))));
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Location = new System.Drawing.Point(0, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(90, 50);
            this.btnBack.TabIndex = 0;
            this.btnBack.Text = "← กลับ";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            //
            // pnlMain
            //
            this.pnlMain.Controls.Add(this.pnlRight);
            this.pnlMain.Controls.Add(this.divider);
            this.pnlMain.Controls.Add(this.pnlLeft);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 50);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1200, 850);
            this.pnlMain.TabIndex = 1;
            //
            // pnlRight
            //
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlRight.Controls.Add(this.dgvColumnMapping);
            this.pnlRight.Controls.Add(this.pnlBottomSection);
            this.pnlRight.Controls.Add(this.pnlEditCard);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(281, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(12);
            this.pnlRight.Size = new System.Drawing.Size(919, 850);
            this.pnlRight.TabIndex = 2;
            //
            // dgvColumnMapping
            //
            this.dgvColumnMapping.AllowUserToAddRows = false;
            this.dgvColumnMapping.AllowUserToDeleteRows = false;
            this.dgvColumnMapping.AutoGenerateColumns = false;
            this.dgvColumnMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumnMapping.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSlotDescription,
            this.colDisplayLabel,
            this.colIsVisible});
            this.dgvColumnMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvColumnMapping.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvColumnMapping.Location = new System.Drawing.Point(12, 102);
            this.dgvColumnMapping.Name = "dgvColumnMapping";
            this.dgvColumnMapping.RowHeadersVisible = false;
            this.dgvColumnMapping.Size = new System.Drawing.Size(895, 456);
            this.dgvColumnMapping.TabIndex = 1;
            //
            // colSlotDescription
            //
            this.colSlotDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colSlotDescription.HeaderText = "จุดที่แสดงผล (dgvArticles / Card)";
            this.colSlotDescription.Name = "colSlotDescription";
            this.colSlotDescription.ReadOnly = true;
            //
            // colDisplayLabel
            //
            this.colDisplayLabel.HeaderText = "Header / Caption";
            this.colDisplayLabel.Name = "colDisplayLabel";
            this.colDisplayLabel.Width = 260;
            //
            // colIsVisible
            //
            this.colIsVisible.HeaderText = "แสดง";
            this.colIsVisible.Name = "colIsVisible";
            this.colIsVisible.Width = 60;
            //
            // pnlBottomSection
            //
            this.pnlBottomSection.Controls.Add(this.pnlCategoryAssignment);
            this.pnlBottomSection.Controls.Add(this.pnlButtons);
            this.pnlBottomSection.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottomSection.Location = new System.Drawing.Point(12, 558);
            this.pnlBottomSection.Name = "pnlBottomSection";
            this.pnlBottomSection.Size = new System.Drawing.Size(895, 280);
            this.pnlBottomSection.TabIndex = 3;
            //
            // pnlCategoryAssignment
            //
            this.pnlCategoryAssignment.Controls.Add(this.clbCategories);
            this.pnlCategoryAssignment.Controls.Add(this.lblCategoryAssignmentTitle);
            this.pnlCategoryAssignment.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCategoryAssignment.Location = new System.Drawing.Point(0, 0);
            this.pnlCategoryAssignment.Name = "pnlCategoryAssignment";
            this.pnlCategoryAssignment.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.pnlCategoryAssignment.Size = new System.Drawing.Size(895, 227);
            this.pnlCategoryAssignment.TabIndex = 0;
            //
            // clbCategories
            //
            this.clbCategories.CheckOnClick = true;
            this.clbCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbCategories.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.clbCategories.FormattingEnabled = true;
            this.clbCategories.Location = new System.Drawing.Point(0, 26);
            this.clbCategories.Name = "clbCategories";
            this.clbCategories.Size = new System.Drawing.Size(895, 201);
            this.clbCategories.TabIndex = 1;
            //
            // lblCategoryAssignmentTitle
            //
            this.lblCategoryAssignmentTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCategoryAssignmentTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCategoryAssignmentTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblCategoryAssignmentTitle.Location = new System.Drawing.Point(0, 8);
            this.lblCategoryAssignmentTitle.Name = "lblCategoryAssignmentTitle";
            this.lblCategoryAssignmentTitle.Size = new System.Drawing.Size(895, 26);
            this.lblCategoryAssignmentTitle.TabIndex = 0;
            this.lblCategoryAssignmentTitle.Text = "☑️ หมวดที่ใช้ Profile นี้ (ติ๊กที่หมวดต้นสาย — หมวดย่อยที่ยังไม่ผูก Profile ของตัวเองจะ inherit ต่ออัตโนมัติ)";
            this.lblCategoryAssignmentTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Controls.Add(this.btnDelete);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 227);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.pnlButtons.Size = new System.Drawing.Size(895, 53);
            this.pnlButtons.TabIndex = 1;
            //
            // btnClose
            //
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnClose.Location = new System.Drawing.Point(300, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 32);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "❌ ปิด";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // btnDelete
            //
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(150, 8);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(140, 32);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "🗑️ ปิดใช้งาน";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            //
            // btnSave
            //
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(0, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(130, 32);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "💾 บันทึก";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // pnlEditCard
            //
            this.pnlEditCard.Controls.Add(this.lblProfileDescription);
            this.pnlEditCard.Controls.Add(this.txtProfileDescription);
            this.pnlEditCard.Controls.Add(this.lblProfileName);
            this.pnlEditCard.Controls.Add(this.txtProfileName);
            this.pnlEditCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEditCard.Location = new System.Drawing.Point(12, 12);
            this.pnlEditCard.Name = "pnlEditCard";
            this.pnlEditCard.Size = new System.Drawing.Size(895, 90);
            this.pnlEditCard.TabIndex = 0;
            //
            // lblProfileDescription
            //
            this.lblProfileDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProfileDescription.Location = new System.Drawing.Point(3, 52);
            this.lblProfileDescription.Name = "lblProfileDescription";
            this.lblProfileDescription.Size = new System.Drawing.Size(120, 22);
            this.lblProfileDescription.TabIndex = 2;
            this.lblProfileDescription.Text = "คำอธิบาย";
            //
            // txtProfileDescription
            //
            this.txtProfileDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProfileDescription.Location = new System.Drawing.Point(130, 49);
            this.txtProfileDescription.MaxLength = 500;
            this.txtProfileDescription.Name = "txtProfileDescription";
            this.txtProfileDescription.Size = new System.Drawing.Size(500, 23);
            this.txtProfileDescription.TabIndex = 3;
            //
            // lblProfileName
            //
            this.lblProfileName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProfileName.Location = new System.Drawing.Point(3, 15);
            this.lblProfileName.Name = "lblProfileName";
            this.lblProfileName.Size = new System.Drawing.Size(120, 22);
            this.lblProfileName.TabIndex = 0;
            this.lblProfileName.Text = "ชื่อ Profile *";
            //
            // txtProfileName
            //
            this.txtProfileName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProfileName.Location = new System.Drawing.Point(130, 12);
            this.txtProfileName.MaxLength = 200;
            this.txtProfileName.Name = "txtProfileName";
            this.txtProfileName.Size = new System.Drawing.Size(350, 23);
            this.txtProfileName.TabIndex = 1;
            //
            // divider
            //
            this.divider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.divider.Dock = System.Windows.Forms.DockStyle.Left;
            this.divider.Location = new System.Drawing.Point(280, 0);
            this.divider.Name = "divider";
            this.divider.Size = new System.Drawing.Size(1, 850);
            this.divider.TabIndex = 1;
            //
            // pnlLeft
            //
            this.pnlLeft.Controls.Add(this.dgvProfiles);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(8);
            this.pnlLeft.Size = new System.Drawing.Size(280, 850);
            this.pnlLeft.TabIndex = 0;
            //
            // dgvProfiles
            //
            this.dgvProfiles.AllowUserToAddRows = false;
            this.dgvProfiles.AllowUserToDeleteRows = false;
            this.dgvProfiles.AutoGenerateColumns = false;
            this.dgvProfiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProfiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colProfileName});
            this.dgvProfiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProfiles.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvProfiles.Location = new System.Drawing.Point(8, 84);
            this.dgvProfiles.MultiSelect = false;
            this.dgvProfiles.Name = "dgvProfiles";
            this.dgvProfiles.ReadOnly = true;
            this.dgvProfiles.RowHeadersVisible = false;
            this.dgvProfiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProfiles.Size = new System.Drawing.Size(264, 758);
            this.dgvProfiles.TabIndex = 1;
            this.dgvProfiles.SelectionChanged += new System.EventHandler(this.DgvProfiles_SelectionChanged);
            //
            // colProfileName
            //
            this.colProfileName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colProfileName.HeaderText = "ชื่อ Profile";
            this.colProfileName.Name = "colProfileName";
            this.colProfileName.ReadOnly = true;
            //
            // pnlLeftHeader
            //
            this.pnlLeftHeader.Controls.Add(this.btnNewProfile);
            this.pnlLeftHeader.Controls.Add(this.lblListTitle);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(8, 8);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(264, 76);
            this.pnlLeftHeader.TabIndex = 0;
            //
            // btnNewProfile
            //
            this.btnNewProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnNewProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewProfile.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNewProfile.FlatAppearance.BorderSize = 0;
            this.btnNewProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewProfile.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNewProfile.ForeColor = System.Drawing.Color.White;
            this.btnNewProfile.Location = new System.Drawing.Point(0, 30);
            this.btnNewProfile.Name = "btnNewProfile";
            this.btnNewProfile.Size = new System.Drawing.Size(264, 36);
            this.btnNewProfile.TabIndex = 1;
            this.btnNewProfile.Text = "➕ Profile ใหม่";
            this.btnNewProfile.UseVisualStyleBackColor = false;
            this.btnNewProfile.Click += new System.EventHandler(this.BtnNewProfile_Click);
            //
            // lblListTitle
            //
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblListTitle.Location = new System.Drawing.Point(0, 0);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(264, 30);
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "📋 รายการ Profile";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // KBArticleDisplayProfileConfigForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 900);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Name = "KBArticleDisplayProfileConfigForm";
            this.Text = "ตั้งค่า Article Display Profile";
            this.Load += new System.EventHandler(this.KBArticleDisplayProfileConfigForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnMapping)).EndInit();
            this.pnlBottomSection.ResumeLayout(false);
            this.pnlCategoryAssignment.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlEditCard.ResumeLayout(false);
            this.pnlEditCard.PerformLayout();
            this.pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProfiles)).EndInit();
            this.pnlLeftHeader.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.DataGridView dgvColumnMapping;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisplayLabel;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsVisible;
        private System.Windows.Forms.Panel pnlBottomSection;
        private System.Windows.Forms.Panel pnlCategoryAssignment;
        private System.Windows.Forms.CheckedListBox clbCategories;
        private System.Windows.Forms.Label lblCategoryAssignmentTitle;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel pnlEditCard;
        private System.Windows.Forms.Label lblProfileDescription;
        private System.Windows.Forms.TextBox txtProfileDescription;
        private System.Windows.Forms.Label lblProfileName;
        private System.Windows.Forms.TextBox txtProfileName;
        private System.Windows.Forms.Panel divider;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.DataGridView dgvProfiles;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProfileName;
        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Button btnNewProfile;
        private System.Windows.Forms.Label lblListTitle;
    }
}
