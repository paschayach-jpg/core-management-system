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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.btnClear = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlExcelPreview = new System.Windows.Forms.Panel();
            this.dgvExcelPreview = new System.Windows.Forms.DataGridView();
            this.pnlExcelPreviewHeader = new System.Windows.Forms.Panel();
            this.lblExcelPreviewTitle = new System.Windows.Forms.Label();
            this.btnUploadExcel = new System.Windows.Forms.Button();
            this.pnlEditCard = new System.Windows.Forms.Panel();
            this.lblTemplateDescription = new System.Windows.Forms.Label();
            this.txtTemplateDescription = new System.Windows.Forms.TextBox();
            this.lblTemplateName = new System.Windows.Forms.Label();
            this.txtTemplateName = new System.Windows.Forms.TextBox();
            this.lblLayoutConfig = new System.Windows.Forms.Label();
            this.cboLayoutConfig = new System.Windows.Forms.ComboBox();
            this.lblArticleTypeConfig = new System.Windows.Forms.Label();
            this.cboArticleTypeConfig = new System.Windows.Forms.ComboBox();
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
            this.pnlExcelPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvExcelPreview)).BeginInit();
            this.pnlExcelPreviewHeader.SuspendLayout();
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
            this.pnlRight.Controls.Add(this.pnlButtons);
            this.pnlRight.Controls.Add(this.pnlExcelPreview);
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
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvColumnMapping.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvColumnMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumnMapping.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTargetColumn,
            this.colDescription,
            this.colExcelMapping});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvColumnMapping.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvColumnMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvColumnMapping.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvColumnMapping.Location = new System.Drawing.Point(16, 393);
            this.dgvColumnMapping.Margin = new System.Windows.Forms.Padding(4);
            this.dgvColumnMapping.Name = "dgvColumnMapping";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvColumnMapping.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvColumnMapping.RowHeadersVisible = false;
            this.dgvColumnMapping.RowHeadersWidth = 51;
            this.dgvColumnMapping.Size = new System.Drawing.Size(1194, 573);
            this.dgvColumnMapping.TabIndex = 1;
            // 
            // colTargetColumn
            // 
            this.colTargetColumn.HeaderText = "คอลัมน์ปลายทาง (KMS.t_KBArticle)";
            this.colTargetColumn.MinimumWidth = 6;
            this.colTargetColumn.Name = "colTargetColumn";
            this.colTargetColumn.ReadOnly = true;
            this.colTargetColumn.Width = 220;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDescription.HeaderText = "คำอธิบาย";
            this.colDescription.MinimumWidth = 6;
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            // 
            // colExcelMapping
            // 
            this.colExcelMapping.HeaderText = "Excel Column / Header ที่ Map มา";
            this.colExcelMapping.MinimumWidth = 6;
            this.colExcelMapping.Name = "colExcelMapping";
            this.colExcelMapping.Width = 220;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnClear);
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Controls.Add(this.btnDelete);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(16, 966);
            this.pnlButtons.Margin = new System.Windows.Forms.Padding(4);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.pnlButtons.Size = new System.Drawing.Size(1194, 65);
            this.pnlButtons.TabIndex = 2;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClear.FlatAppearance.BorderSize = 0;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnClear.Location = new System.Drawing.Point(547, 10);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(160, 39);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "🧹 ล้างข้อมูล";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9.5F);
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
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
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
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
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
            // pnlExcelPreview
            // 
            this.pnlExcelPreview.Controls.Add(this.dgvExcelPreview);
            this.pnlExcelPreview.Controls.Add(this.pnlExcelPreviewHeader);
            this.pnlExcelPreview.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlExcelPreview.Location = new System.Drawing.Point(16, 171);
            this.pnlExcelPreview.Margin = new System.Windows.Forms.Padding(4);
            this.pnlExcelPreview.Name = "pnlExcelPreview";
            this.pnlExcelPreview.Size = new System.Drawing.Size(1194, 222);
            this.pnlExcelPreview.TabIndex = 3;
            // 
            // dgvExcelPreview
            // 
            this.dgvExcelPreview.AllowUserToAddRows = false;
            this.dgvExcelPreview.AllowUserToDeleteRows = false;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvExcelPreview.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvExcelPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvExcelPreview.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvExcelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvExcelPreview.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvExcelPreview.Location = new System.Drawing.Point(0, 42);
            this.dgvExcelPreview.Margin = new System.Windows.Forms.Padding(4);
            this.dgvExcelPreview.Name = "dgvExcelPreview";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvExcelPreview.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvExcelPreview.RowHeadersWidth = 51;
            this.dgvExcelPreview.Size = new System.Drawing.Size(1194, 180);
            this.dgvExcelPreview.TabIndex = 0;
            // 
            // pnlExcelPreviewHeader
            // 
            this.pnlExcelPreviewHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.pnlExcelPreviewHeader.Controls.Add(this.lblExcelPreviewTitle);
            this.pnlExcelPreviewHeader.Controls.Add(this.btnUploadExcel);
            this.pnlExcelPreviewHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlExcelPreviewHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlExcelPreviewHeader.Margin = new System.Windows.Forms.Padding(4);
            this.pnlExcelPreviewHeader.Name = "pnlExcelPreviewHeader";
            this.pnlExcelPreviewHeader.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.pnlExcelPreviewHeader.Size = new System.Drawing.Size(1194, 42);
            this.pnlExcelPreviewHeader.TabIndex = 1;
            // 
            // lblExcelPreviewTitle
            // 
            this.lblExcelPreviewTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExcelPreviewTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblExcelPreviewTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblExcelPreviewTitle.Location = new System.Drawing.Point(5, 0);
            this.lblExcelPreviewTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExcelPreviewTitle.Name = "lblExcelPreviewTitle";
            this.lblExcelPreviewTitle.Size = new System.Drawing.Size(936, 42);
            this.lblExcelPreviewTitle.TabIndex = 0;
            this.lblExcelPreviewTitle.Text = "ตัวอย่างคอลัมน์จาก Excel — คลิกหัวคอลัมน์เพื่อเลือก Mapping ให้แถวที่กำลังเลือกอย" +
    "ู่ (ดับเบิลคลิก = ยกเลิก)";
            this.lblExcelPreviewTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnUploadExcel
            // 
            this.btnUploadExcel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnUploadExcel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUploadExcel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnUploadExcel.FlatAppearance.BorderSize = 0;
            this.btnUploadExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUploadExcel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUploadExcel.ForeColor = System.Drawing.Color.White;
            this.btnUploadExcel.Location = new System.Drawing.Point(941, 0);
            this.btnUploadExcel.Margin = new System.Windows.Forms.Padding(4);
            this.btnUploadExcel.Name = "btnUploadExcel";
            this.btnUploadExcel.Size = new System.Drawing.Size(253, 42);
            this.btnUploadExcel.TabIndex = 1;
            this.btnUploadExcel.Text = "📂 อัปโหลด Excel Template";
            this.btnUploadExcel.UseVisualStyleBackColor = false;
            this.btnUploadExcel.Click += new System.EventHandler(this.BtnUploadExcel_Click);
            // 
            // pnlEditCard
            // 
            this.pnlEditCard.Controls.Add(this.lblTemplateDescription);
            this.pnlEditCard.Controls.Add(this.txtTemplateDescription);
            this.pnlEditCard.Controls.Add(this.lblTemplateName);
            this.pnlEditCard.Controls.Add(this.txtTemplateName);
            this.pnlEditCard.Controls.Add(this.lblLayoutConfig);
            this.pnlEditCard.Controls.Add(this.cboLayoutConfig);
            this.pnlEditCard.Controls.Add(this.lblArticleTypeConfig);
            this.pnlEditCard.Controls.Add(this.cboArticleTypeConfig);
            this.pnlEditCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEditCard.Location = new System.Drawing.Point(16, 15);
            this.pnlEditCard.Margin = new System.Windows.Forms.Padding(4);
            this.pnlEditCard.Name = "pnlEditCard";
            this.pnlEditCard.Size = new System.Drawing.Size(1194, 156);
            this.pnlEditCard.TabIndex = 0;
            // 
            // lblTemplateDescription
            // 
            this.lblTemplateDescription.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblTemplateDescription.Location = new System.Drawing.Point(4, 64);
            this.lblTemplateDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTemplateDescription.Name = "lblTemplateDescription";
            this.lblTemplateDescription.Size = new System.Drawing.Size(160, 27);
            this.lblTemplateDescription.TabIndex = 2;
            this.lblTemplateDescription.Text = "คำอธิบาย";
            // 
            // txtTemplateDescription
            // 
            this.txtTemplateDescription.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtTemplateDescription.Location = new System.Drawing.Point(173, 60);
            this.txtTemplateDescription.Margin = new System.Windows.Forms.Padding(4);
            this.txtTemplateDescription.MaxLength = 500;
            this.txtTemplateDescription.Name = "txtTemplateDescription";
            this.txtTemplateDescription.Size = new System.Drawing.Size(665, 34);
            this.txtTemplateDescription.TabIndex = 3;
            // 
            // lblTemplateName
            // 
            this.lblTemplateName.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblTemplateName.Location = new System.Drawing.Point(4, 18);
            this.lblTemplateName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTemplateName.Name = "lblTemplateName";
            this.lblTemplateName.Size = new System.Drawing.Size(160, 27);
            this.lblTemplateName.TabIndex = 0;
            this.lblTemplateName.Text = "ชื่อ Template *";
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtTemplateName.Location = new System.Drawing.Point(173, 15);
            this.txtTemplateName.Margin = new System.Windows.Forms.Padding(4);
            this.txtTemplateName.MaxLength = 200;
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(665, 34);
            this.txtTemplateName.TabIndex = 1;
            // 
            // lblLayoutConfig
            // 
            this.lblLayoutConfig.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblLayoutConfig.Location = new System.Drawing.Point(4, 110);
            this.lblLayoutConfig.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLayoutConfig.Name = "lblLayoutConfig";
            this.lblLayoutConfig.Size = new System.Drawing.Size(160, 27);
            this.lblLayoutConfig.TabIndex = 4;
            this.lblLayoutConfig.Text = "ประเภทไฟล์ *";
            // 
            // cboLayoutConfig
            // 
            this.cboLayoutConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLayoutConfig.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.cboLayoutConfig.Location = new System.Drawing.Point(173, 106);
            this.cboLayoutConfig.Margin = new System.Windows.Forms.Padding(4);
            this.cboLayoutConfig.Name = "cboLayoutConfig";
            this.cboLayoutConfig.Size = new System.Drawing.Size(442, 36);
            this.cboLayoutConfig.TabIndex = 5;
            // 
            // lblArticleTypeConfig
            // 
            this.lblArticleTypeConfig.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblArticleTypeConfig.Location = new System.Drawing.Point(623, 110);
            this.lblArticleTypeConfig.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblArticleTypeConfig.Name = "lblArticleTypeConfig";
            this.lblArticleTypeConfig.Size = new System.Drawing.Size(200, 27);
            this.lblArticleTypeConfig.TabIndex = 6;
            this.lblArticleTypeConfig.Text = "ประเภทบทความ (ArticleType) *";
            // 
            // cboArticleTypeConfig
            // 
            this.cboArticleTypeConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboArticleTypeConfig.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.cboArticleTypeConfig.Location = new System.Drawing.Point(830, 106);
            this.cboArticleTypeConfig.Margin = new System.Windows.Forms.Padding(4);
            this.cboArticleTypeConfig.Name = "cboArticleTypeConfig";
            this.cboArticleTypeConfig.Size = new System.Drawing.Size(347, 36);
            this.cboArticleTypeConfig.TabIndex = 7;
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
            this.pnlLeft.Controls.Add(this.dgvTemplates);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Margin = new System.Windows.Forms.Padding(4);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.pnlLeft.Size = new System.Drawing.Size(373, 1046);
            this.pnlLeft.TabIndex = 0;
            // 
            // dgvTemplates
            // 
            this.dgvTemplates.AllowUserToAddRows = false;
            this.dgvTemplates.AllowUserToDeleteRows = false;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTemplates.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvTemplates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTemplates.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTemplateName});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvTemplates.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTemplates.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgvTemplates.Location = new System.Drawing.Point(11, 104);
            this.dgvTemplates.Margin = new System.Windows.Forms.Padding(4);
            this.dgvTemplates.MultiSelect = false;
            this.dgvTemplates.Name = "dgvTemplates";
            this.dgvTemplates.ReadOnly = true;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTemplates.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dgvTemplates.RowHeadersVisible = false;
            this.dgvTemplates.RowHeadersWidth = 51;
            this.dgvTemplates.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTemplates.Size = new System.Drawing.Size(351, 932);
            this.dgvTemplates.TabIndex = 1;
            this.dgvTemplates.SelectionChanged += new System.EventHandler(this.DgvTemplates_SelectionChanged);
            // 
            // colTemplateName
            // 
            this.colTemplateName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTemplateName.HeaderText = "ชื่อ Template";
            this.colTemplateName.MinimumWidth = 6;
            this.colTemplateName.Name = "colTemplateName";
            this.colTemplateName.ReadOnly = true;
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.Controls.Add(this.btnNewTemplate);
            this.pnlLeftHeader.Controls.Add(this.lblListTitle);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(11, 10);
            this.pnlLeftHeader.Margin = new System.Windows.Forms.Padding(4);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(351, 94);
            this.pnlLeftHeader.TabIndex = 0;
            // 
            // btnNewTemplate
            // 
            this.btnNewTemplate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnNewTemplate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewTemplate.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNewTemplate.FlatAppearance.BorderSize = 0;
            this.btnNewTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewTemplate.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnNewTemplate.ForeColor = System.Drawing.Color.White;
            this.btnNewTemplate.Location = new System.Drawing.Point(0, 37);
            this.btnNewTemplate.Margin = new System.Windows.Forms.Padding(4);
            this.btnNewTemplate.Name = "btnNewTemplate";
            this.btnNewTemplate.Size = new System.Drawing.Size(351, 44);
            this.btnNewTemplate.TabIndex = 1;
            this.btnNewTemplate.Text = "➕ Template ใหม่";
            this.btnNewTemplate.UseVisualStyleBackColor = false;
            this.btnNewTemplate.Click += new System.EventHandler(this.BtnNewTemplate_Click);
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
            this.lblListTitle.Text = "📋 รายการ Template";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // KBImportTemplateConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 1108);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "KBImportTemplateConfigForm";
            this.Text = "ตั้งค่า Excel Import Template";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.KBImportTemplateConfigForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnMapping)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.pnlExcelPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvExcelPreview)).EndInit();
            this.pnlExcelPreviewHeader.ResumeLayout(false);
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
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel pnlEditCard;
        private System.Windows.Forms.Label lblTemplateDescription;
        private System.Windows.Forms.TextBox txtTemplateDescription;
        private System.Windows.Forms.Label lblTemplateName;
        private System.Windows.Forms.TextBox txtTemplateName;
        private System.Windows.Forms.Label lblLayoutConfig;
        private System.Windows.Forms.ComboBox cboLayoutConfig;
        private System.Windows.Forms.Label lblArticleTypeConfig;
        private System.Windows.Forms.ComboBox cboArticleTypeConfig;
        private System.Windows.Forms.Panel pnlExcelPreview;
        private System.Windows.Forms.DataGridView dgvExcelPreview;
        private System.Windows.Forms.Panel pnlExcelPreviewHeader;
        private System.Windows.Forms.Button btnUploadExcel;
        private System.Windows.Forms.Label lblExcelPreviewTitle;
        private System.Windows.Forms.Panel divider;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.DataGridView dgvTemplates;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTemplateName;
        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Button btnNewTemplate;
        private System.Windows.Forms.Label lblListTitle;
    }
}
