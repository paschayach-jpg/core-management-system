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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.dgvColumnMapping = new System.Windows.Forms.DataGridView();
            this.colSlotKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSlotDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDisplayLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.pnlBottomSection = new System.Windows.Forms.Panel();
            this.pnlCategoryAssignment = new System.Windows.Forms.Panel();
            this.treeCategories = new System.Windows.Forms.TreeView();
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
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(4);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1600, 62);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(120, 0);
            this.lblHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(1480, 62);
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
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Location = new System.Drawing.Point(0, 0);
            this.btnBack.Margin = new System.Windows.Forms.Padding(4);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(120, 62);
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
            this.pnlMain.Location = new System.Drawing.Point(0, 62);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(4);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1600, 1046);
            this.pnlMain.TabIndex = 1;
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlRight.Controls.Add(this.dgvColumnMapping);
            this.pnlRight.Controls.Add(this.pnlBottomSection);
            this.pnlRight.Controls.Add(this.pnlEditCard);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(374, 0);
            this.pnlRight.Margin = new System.Windows.Forms.Padding(4);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(16, 15, 16, 15);
            this.pnlRight.Size = new System.Drawing.Size(1226, 1046);
            this.pnlRight.TabIndex = 2;
            // 
            // dgvColumnMapping
            // 
            this.dgvColumnMapping.AllowUserToAddRows = false;
            this.dgvColumnMapping.AllowUserToDeleteRows = false;
            this.dgvColumnMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumnMapping.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSlotKey,
            this.colSlotDescription,
            this.colDisplayLabel,
            this.colIsVisible});
            this.dgvColumnMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvColumnMapping.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvColumnMapping.Location = new System.Drawing.Point(16, 126);
            this.dgvColumnMapping.Margin = new System.Windows.Forms.Padding(4);
            this.dgvColumnMapping.Name = "dgvColumnMapping";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvColumnMapping.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvColumnMapping.RowHeadersVisible = false;
            this.dgvColumnMapping.RowHeadersWidth = 51;
            this.dgvColumnMapping.Size = new System.Drawing.Size(1194, 560);
            this.dgvColumnMapping.TabIndex = 1;
            // 
            // colSlotKey
            // 
            this.colSlotKey.HeaderText = "จุดที่แสดงผล";
            this.colSlotKey.MinimumWidth = 6;
            this.colSlotKey.Name = "colSlotKey";
            this.colSlotKey.ReadOnly = true;
            this.colSlotKey.Width = 130;
            // 
            // colSlotDescription
            // 
            this.colSlotDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colSlotDescription.HeaderText = "คำอธิบาย (แก้ไขได้)";
            this.colSlotDescription.MinimumWidth = 6;
            this.colSlotDescription.Name = "colSlotDescription";
            // 
            // colDisplayLabel
            // 
            this.colDisplayLabel.HeaderText = "Header / Caption";
            this.colDisplayLabel.MinimumWidth = 6;
            this.colDisplayLabel.Name = "colDisplayLabel";
            this.colDisplayLabel.Width = 260;
            // 
            // colIsVisible
            // 
            this.colIsVisible.HeaderText = "แสดง";
            this.colIsVisible.MinimumWidth = 6;
            this.colIsVisible.Name = "colIsVisible";
            this.colIsVisible.Width = 60;
            // 
            // pnlBottomSection
            // 
            this.pnlBottomSection.Controls.Add(this.pnlCategoryAssignment);
            this.pnlBottomSection.Controls.Add(this.pnlButtons);
            this.pnlBottomSection.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottomSection.Location = new System.Drawing.Point(16, 686);
            this.pnlBottomSection.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBottomSection.Name = "pnlBottomSection";
            this.pnlBottomSection.Size = new System.Drawing.Size(1194, 345);
            this.pnlBottomSection.TabIndex = 3;
            // 
            // pnlCategoryAssignment
            // 
            this.pnlCategoryAssignment.Controls.Add(this.treeCategories);
            this.pnlCategoryAssignment.Controls.Add(this.lblCategoryAssignmentTitle);
            this.pnlCategoryAssignment.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCategoryAssignment.Location = new System.Drawing.Point(0, 0);
            this.pnlCategoryAssignment.Margin = new System.Windows.Forms.Padding(4);
            this.pnlCategoryAssignment.Name = "pnlCategoryAssignment";
            this.pnlCategoryAssignment.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.pnlCategoryAssignment.Size = new System.Drawing.Size(1194, 279);
            this.pnlCategoryAssignment.TabIndex = 0;
            // 
            // treeCategories
            // 
            this.treeCategories.CheckBoxes = true;
            this.treeCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeCategories.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeCategories.HideSelection = false;
            this.treeCategories.Location = new System.Drawing.Point(0, 42);
            this.treeCategories.Margin = new System.Windows.Forms.Padding(4);
            this.treeCategories.Name = "treeCategories";
            this.treeCategories.Size = new System.Drawing.Size(1194, 237);
            this.treeCategories.TabIndex = 1;
            // 
            // lblCategoryAssignmentTitle
            // 
            this.lblCategoryAssignmentTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCategoryAssignmentTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCategoryAssignmentTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblCategoryAssignmentTitle.Location = new System.Drawing.Point(0, 10);
            this.lblCategoryAssignmentTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCategoryAssignmentTitle.Name = "lblCategoryAssignmentTitle";
            this.lblCategoryAssignmentTitle.Size = new System.Drawing.Size(1194, 32);
            this.lblCategoryAssignmentTitle.TabIndex = 0;
            this.lblCategoryAssignmentTitle.Text = "☑️ หมวดที่ใช้ Profile นี้ (ติ๊กที่หมวดต้นสาย — หมวดย่อยที่ยังไม่ผูก Profile ของตั" +
    "วเองจะ inherit ต่ออัตโนมัติ)";
            this.lblCategoryAssignmentTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Controls.Add(this.btnDelete);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 280);
            this.pnlButtons.Margin = new System.Windows.Forms.Padding(4);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.pnlButtons.Size = new System.Drawing.Size(1194, 65);
            this.pnlButtons.TabIndex = 1;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnClose.Location = new System.Drawing.Point(400, 10);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(133, 39);
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
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(200, 10);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(187, 39);
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
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(0, 10);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(173, 39);
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
            this.pnlEditCard.Location = new System.Drawing.Point(16, 15);
            this.pnlEditCard.Margin = new System.Windows.Forms.Padding(4);
            this.pnlEditCard.Name = "pnlEditCard";
            this.pnlEditCard.Size = new System.Drawing.Size(1194, 111);
            this.pnlEditCard.TabIndex = 0;
            // 
            // lblProfileDescription
            // 
            this.lblProfileDescription.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblProfileDescription.Location = new System.Drawing.Point(4, 64);
            this.lblProfileDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProfileDescription.Name = "lblProfileDescription";
            this.lblProfileDescription.Size = new System.Drawing.Size(160, 27);
            this.lblProfileDescription.TabIndex = 2;
            this.lblProfileDescription.Text = "คำอธิบาย";
            // 
            // txtProfileDescription
            // 
            this.txtProfileDescription.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtProfileDescription.Location = new System.Drawing.Point(173, 60);
            this.txtProfileDescription.Margin = new System.Windows.Forms.Padding(4);
            this.txtProfileDescription.MaxLength = 500;
            this.txtProfileDescription.Name = "txtProfileDescription";
            this.txtProfileDescription.Size = new System.Drawing.Size(665, 34);
            this.txtProfileDescription.TabIndex = 3;
            // 
            // lblProfileName
            // 
            this.lblProfileName.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblProfileName.Location = new System.Drawing.Point(4, 18);
            this.lblProfileName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProfileName.Name = "lblProfileName";
            this.lblProfileName.Size = new System.Drawing.Size(160, 27);
            this.lblProfileName.TabIndex = 0;
            this.lblProfileName.Text = "ชื่อ Profile *";
            // 
            // txtProfileName
            // 
            this.txtProfileName.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtProfileName.Location = new System.Drawing.Point(173, 15);
            this.txtProfileName.Margin = new System.Windows.Forms.Padding(4);
            this.txtProfileName.MaxLength = 200;
            this.txtProfileName.Name = "txtProfileName";
            this.txtProfileName.Size = new System.Drawing.Size(665, 34);
            this.txtProfileName.TabIndex = 1;
            // 
            // divider
            // 
            this.divider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.divider.Dock = System.Windows.Forms.DockStyle.Left;
            this.divider.Location = new System.Drawing.Point(373, 0);
            this.divider.Margin = new System.Windows.Forms.Padding(4);
            this.divider.Name = "divider";
            this.divider.Size = new System.Drawing.Size(1, 1046);
            this.divider.TabIndex = 1;
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.dgvProfiles);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Margin = new System.Windows.Forms.Padding(4);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.pnlLeft.Size = new System.Drawing.Size(373, 1046);
            this.pnlLeft.TabIndex = 0;
            // 
            // dgvProfiles
            // 
            this.dgvProfiles.AllowUserToAddRows = false;
            this.dgvProfiles.AllowUserToDeleteRows = false;
            this.dgvProfiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProfiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colProfileName});
            this.dgvProfiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProfiles.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvProfiles.Location = new System.Drawing.Point(11, 104);
            this.dgvProfiles.Margin = new System.Windows.Forms.Padding(4);
            this.dgvProfiles.MultiSelect = false;
            this.dgvProfiles.Name = "dgvProfiles";
            this.dgvProfiles.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvProfiles.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvProfiles.RowHeadersVisible = false;
            this.dgvProfiles.RowHeadersWidth = 51;
            this.dgvProfiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProfiles.Size = new System.Drawing.Size(351, 932);
            this.dgvProfiles.TabIndex = 1;
            this.dgvProfiles.SelectionChanged += new System.EventHandler(this.DgvProfiles_SelectionChanged);
            // 
            // colProfileName
            // 
            this.colProfileName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colProfileName.HeaderText = "ชื่อ Profile";
            this.colProfileName.MinimumWidth = 6;
            this.colProfileName.Name = "colProfileName";
            this.colProfileName.ReadOnly = true;
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.Controls.Add(this.btnNewProfile);
            this.pnlLeftHeader.Controls.Add(this.lblListTitle);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(11, 10);
            this.pnlLeftHeader.Margin = new System.Windows.Forms.Padding(4);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(351, 94);
            this.pnlLeftHeader.TabIndex = 0;
            // 
            // btnNewProfile
            // 
            this.btnNewProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnNewProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewProfile.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNewProfile.FlatAppearance.BorderSize = 0;
            this.btnNewProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewProfile.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnNewProfile.ForeColor = System.Drawing.Color.White;
            this.btnNewProfile.Location = new System.Drawing.Point(0, 37);
            this.btnNewProfile.Margin = new System.Windows.Forms.Padding(4);
            this.btnNewProfile.Name = "btnNewProfile";
            this.btnNewProfile.Size = new System.Drawing.Size(351, 44);
            this.btnNewProfile.TabIndex = 1;
            this.btnNewProfile.Text = "➕ Profile ใหม่";
            this.btnNewProfile.UseVisualStyleBackColor = false;
            this.btnNewProfile.Click += new System.EventHandler(this.BtnNewProfile_Click);
            // 
            // lblListTitle
            // 
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblListTitle.Location = new System.Drawing.Point(0, 0);
            this.lblListTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(351, 37);
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "📋 รายการ Profile";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // KBArticleDisplayProfileConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 1108);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "KBArticleDisplayProfileConfigForm";
            this.Text = "ตั้งค่า Article Display Profile";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
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
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisplayLabel;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsVisible;
        private System.Windows.Forms.Panel pnlBottomSection;
        private System.Windows.Forms.Panel pnlCategoryAssignment;
        private System.Windows.Forms.TreeView treeCategories;
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
