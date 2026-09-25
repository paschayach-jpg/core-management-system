namespace CMS.Presentation.Modules.KMS
{
    partial class KBImportTemplateConfigForm
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
            this.colTargetColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExcelMapping = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlEditCard = new System.Windows.Forms.Panel();
            this.lblTemplateDescription = new System.Windows.Forms.Label();
            this.txtTemplateDescription = new System.Windows.Forms.TextBox();
            this.lblTemplateName = new System.Windows.Forms.Label();
            this.txtTemplateName = new System.Windows.Forms.TextBox();
            this.divider = new System.Windows.Forms.Panel();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.dgvTemplates = new System.Windows.Forms.DataGridView();
            this.colTemplateName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.btnNewTemplate = new System.Windows.Forms.Button();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnMapping)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.pnlEditCard.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTemplates)).BeginInit();
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
            this.lblHeader.Text = "  ⚙️ ตั้งค่า Excel Import Template — Knowledge Base (MOD-006)";
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
            this.pnlMain.Size = new System.Drawing.Size(1200, 750);
            this.pnlMain.TabIndex = 1;
            //
            // pnlRight
            //
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlRight.Controls.Add(this.dgvColumnMapping);
            this.pnlRight.Controls.Add(this.pnlButtons);
            this.pnlRight.Controls.Add(this.pnlEditCard);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(281, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(12);
            this.pnlRight.Size = new System.Drawing.Size(919, 750);
            this.pnlRight.TabIndex = 2;
            //
            // dgvColumnMapping
            //
            this.dgvColumnMapping.AllowUserToAddRows = false;
            this.dgvColumnMapping.AllowUserToDeleteRows = false;
            this.dgvColumnMapping.AutoGenerateColumns = false;
            this.dgvColumnMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumnMapping.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTargetColumn,
            this.colDescription,
            this.colExcelMapping});
            this.dgvColumnMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvColumnMapping.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvColumnMapping.Location = new System.Drawing.Point(12, 102);
            this.dgvColumnMapping.Name = "dgvColumnMapping";
            this.dgvColumnMapping.RowHeadersVisible = false;
            this.dgvColumnMapping.Size = new System.Drawing.Size(895, 583);
            this.dgvColumnMapping.TabIndex = 1;
            //
            // colTargetColumn
            //
            this.colTargetColumn.HeaderText = "คอลัมน์ปลายทาง (KMS.t_KBArticle)";
            this.colTargetColumn.Name = "colTargetColumn";
            this.colTargetColumn.ReadOnly = true;
            this.colTargetColumn.Width = 220;
            //
            // colDescription
            //
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDescription.HeaderText = "คำอธิบาย";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            //
            // colExcelMapping
            //
            this.colExcelMapping.HeaderText = "Excel Column / Header ที่ Map มา";
            this.colExcelMapping.Name = "colExcelMapping";
            this.colExcelMapping.Width = 220;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Controls.Add(this.btnDelete);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(12, 685);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.pnlButtons.Size = new System.Drawing.Size(895, 53);
            this.pnlButtons.TabIndex = 2;
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
            this.pnlEditCard.Controls.Add(this.lblTemplateDescription);
            this.pnlEditCard.Controls.Add(this.txtTemplateDescription);
            this.pnlEditCard.Controls.Add(this.lblTemplateName);
            this.pnlEditCard.Controls.Add(this.txtTemplateName);
            this.pnlEditCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEditCard.Location = new System.Drawing.Point(12, 12);
            this.pnlEditCard.Name = "pnlEditCard";
            this.pnlEditCard.Size = new System.Drawing.Size(895, 90);
            this.pnlEditCard.TabIndex = 0;
            //
            // lblTemplateDescription
            //
            this.lblTemplateDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTemplateDescription.Location = new System.Drawing.Point(3, 52);
            this.lblTemplateDescription.Name = "lblTemplateDescription";
            this.lblTemplateDescription.Size = new System.Drawing.Size(120, 22);
            this.lblTemplateDescription.TabIndex = 2;
            this.lblTemplateDescription.Text = "คำอธิบาย";
            //
            // txtTemplateDescription
            //
            this.txtTemplateDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTemplateDescription.Location = new System.Drawing.Point(130, 49);
            this.txtTemplateDescription.MaxLength = 500;
            this.txtTemplateDescription.Name = "txtTemplateDescription";
            this.txtTemplateDescription.Size = new System.Drawing.Size(500, 23);
            this.txtTemplateDescription.TabIndex = 3;
            //
            // lblTemplateName
            //
            this.lblTemplateName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTemplateName.Location = new System.Drawing.Point(3, 15);
            this.lblTemplateName.Name = "lblTemplateName";
            this.lblTemplateName.Size = new System.Drawing.Size(120, 22);
            this.lblTemplateName.TabIndex = 0;
            this.lblTemplateName.Text = "ชื่อ Template *";
            //
            // txtTemplateName
            //
            this.txtTemplateName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTemplateName.Location = new System.Drawing.Point(130, 12);
            this.txtTemplateName.MaxLength = 200;
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(350, 23);
            this.txtTemplateName.TabIndex = 1;
            //
            // divider
            //
            this.divider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.divider.Dock = System.Windows.Forms.DockStyle.Left;
            this.divider.Location = new System.Drawing.Point(280, 0);
            this.divider.Name = "divider";
            this.divider.Size = new System.Drawing.Size(1, 750);
            this.divider.TabIndex = 1;
            //
            // pnlLeft
            //
            this.pnlLeft.Controls.Add(this.dgvTemplates);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(8);
            this.pnlLeft.Size = new System.Drawing.Size(280, 750);
            this.pnlLeft.TabIndex = 0;
            //
            // dgvTemplates
            //
            this.dgvTemplates.AllowUserToAddRows = false;
            this.dgvTemplates.AllowUserToDeleteRows = false;
            this.dgvTemplates.AutoGenerateColumns = false;
            this.dgvTemplates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTemplates.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTemplateName});
            this.dgvTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTemplates.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvTemplates.Location = new System.Drawing.Point(8, 84);
            this.dgvTemplates.MultiSelect = false;
            this.dgvTemplates.Name = "dgvTemplates";
            this.dgvTemplates.ReadOnly = true;
            this.dgvTemplates.RowHeadersVisible = false;
            this.dgvTemplates.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTemplates.Size = new System.Drawing.Size(264, 658);
            this.dgvTemplates.TabIndex = 1;
            this.dgvTemplates.SelectionChanged += new System.EventHandler(this.DgvTemplates_SelectionChanged);
            //
            // colTemplateName
            //
            this.colTemplateName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTemplateName.HeaderText = "ชื่อ Template";
            this.colTemplateName.Name = "colTemplateName";
            this.colTemplateName.ReadOnly = true;
            //
            // pnlLeftHeader
            //
            this.pnlLeftHeader.Controls.Add(this.btnNewTemplate);
            this.pnlLeftHeader.Controls.Add(this.lblListTitle);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(8, 8);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(264, 76);
            this.pnlLeftHeader.TabIndex = 0;
            //
            // btnNewTemplate
            //
            this.btnNewTemplate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnNewTemplate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewTemplate.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNewTemplate.FlatAppearance.BorderSize = 0;
            this.btnNewTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewTemplate.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNewTemplate.ForeColor = System.Drawing.Color.White;
            this.btnNewTemplate.Location = new System.Drawing.Point(0, 30);
            this.btnNewTemplate.Name = "btnNewTemplate";
            this.btnNewTemplate.Size = new System.Drawing.Size(264, 36);
            this.btnNewTemplate.TabIndex = 1;
            this.btnNewTemplate.Text = "➕ Template ใหม่";
            this.btnNewTemplate.UseVisualStyleBackColor = false;
            this.btnNewTemplate.Click += new System.EventHandler(this.BtnNewTemplate_Click);
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
            this.lblListTitle.Text = "📋 รายการ Template";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // KBImportTemplateConfigForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Name = "KBImportTemplateConfigForm";
            this.Text = "ตั้งค่า Excel Import Template";
            this.Load += new System.EventHandler(this.KBImportTemplateConfigForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnMapping)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.pnlEditCard.ResumeLayout(false);
            this.pnlEditCard.PerformLayout();
            this.pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTemplates)).EndInit();
            this.pnlLeftHeader.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.DataGridView dgvColumnMapping;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExcelMapping;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel pnlEditCard;
        private System.Windows.Forms.Label lblTemplateDescription;
        private System.Windows.Forms.TextBox txtTemplateDescription;
        private System.Windows.Forms.Label lblTemplateName;
        private System.Windows.Forms.TextBox txtTemplateName;
        private System.Windows.Forms.Panel divider;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.DataGridView dgvTemplates;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTemplateName;
        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Button btnNewTemplate;
        private System.Windows.Forms.Label lblListTitle;
    }
}
