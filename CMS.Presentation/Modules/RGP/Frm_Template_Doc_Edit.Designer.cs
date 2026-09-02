namespace CMS.Presentation.Modules.RGP
{
    partial class Frm_Template_Doc_Edit
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlAccent = new System.Windows.Forms.Panel();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.dgvTemplates = new System.Windows.Forms.DataGridView();
            this.pnlTplEdit = new System.Windows.Forms.Panel();
            this.lblTplEditTitle = new System.Windows.Forms.Label();
            this.lblTplCode = new System.Windows.Forms.Label();
            this.txtTplCode = new System.Windows.Forms.TextBox();
            this.lblTplName = new System.Windows.Forms.Label();
            this.txtTplName = new System.Windows.Forms.TextBox();
            this.lblTplDesc = new System.Windows.Forms.Label();
            this.txtTplDesc = new System.Windows.Forms.TextBox();
            this.chkTplActive = new System.Windows.Forms.CheckBox();
            this.chkIsLegalDict = new System.Windows.Forms.CheckBox();
            this.btnSaveTpl = new System.Windows.Forms.Button();
            this.btnCancelTpl = new System.Windows.Forms.Button();
            this.btnDeleteTpl = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitDoc = new System.Windows.Forms.SplitContainer();
            this.dgvStates = new System.Windows.Forms.DataGridView();
            this.pnlTplHeader = new System.Windows.Forms.Panel();
            this.lblTplSelect = new System.Windows.Forms.Label();
            this.cbbTemplate = new System.Windows.Forms.ComboBox();
            this.pnlStateToolbar = new System.Windows.Forms.Panel();
            this.btnAddState = new System.Windows.Forms.Button();
            this.btnEditState = new System.Windows.Forms.Button();
            this.btnDeleteState = new System.Windows.Forms.Button();
            this.btnCopyStates = new System.Windows.Forms.Button();
            this.dgvDocs = new System.Windows.Forms.DataGridView();
            this.pnlDocHeader = new System.Windows.Forms.Panel();
            this.lblSelectedState = new System.Windows.Forms.Label();
            this.btnAddDoc = new System.Windows.Forms.Button();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTemplates)).BeginInit();
            this.pnlTplEdit.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDoc)).BeginInit();
            this.splitDoc.Panel1.SuspendLayout();
            this.splitDoc.Panel2.SuspendLayout();
            this.splitDoc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStates)).BeginInit();
            this.pnlTplHeader.SuspendLayout();
            this.pnlStateToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocs)).BeginInit();
            this.pnlDocHeader.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1200, 50);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(1200, 50);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "จัดการเส้นงาน Workflow ขั้นตอน และเอกสาร";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlAccent
            // 
            this.pnlAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(185)))), ((int)(((byte)(0)))));
            this.pnlAccent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAccent.Location = new System.Drawing.Point(0, 50);
            this.pnlAccent.Name = "pnlAccent";
            this.pnlAccent.Size = new System.Drawing.Size(1200, 3);
            this.pnlAccent.TabIndex = 1;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabPage1);
            this.tabMain.Controls.Add(this.tabPage2);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabMain.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.tabMain.ItemSize = new System.Drawing.Size(180, 30);
            this.tabMain.Location = new System.Drawing.Point(0, 53);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1200, 522);
            this.tabMain.TabIndex = 2;
            this.tabMain.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabMain_DrawItem);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.tabPage1.Controls.Add(this.splitMain);
            this.tabPage1.Location = new System.Drawing.Point(4, 34);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(1192, 484);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "  🗂 ประเภทโครงการ  ";
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(4, 4);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.dgvTemplates);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.splitMain.Panel2.Controls.Add(this.pnlTplEdit);
            this.splitMain.Size = new System.Drawing.Size(1184, 476);
            this.splitMain.SplitterDistance = 576;
            this.splitMain.TabIndex = 0;
            // 
            // dgvTemplates
            // 
            this.dgvTemplates.AllowUserToAddRows = false;
            this.dgvTemplates.AllowUserToDeleteRows = false;
            this.dgvTemplates.BackgroundColor = System.Drawing.Color.White;
            this.dgvTemplates.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvTemplates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTemplates.Location = new System.Drawing.Point(0, 0);
            this.dgvTemplates.MultiSelect = false;
            this.dgvTemplates.Name = "dgvTemplates";
            this.dgvTemplates.ReadOnly = true;
            this.dgvTemplates.RowHeadersVisible = false;
            this.dgvTemplates.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTemplates.Size = new System.Drawing.Size(576, 476);
            this.dgvTemplates.TabIndex = 0;
            this.dgvTemplates.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTemplates_CellDoubleClick);
            this.dgvTemplates.SelectionChanged += new System.EventHandler(this.dgvTemplates_SelectionChanged);
            // 
            // pnlTplEdit
            // 
            this.pnlTplEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlTplEdit.Controls.Add(this.lblTplEditTitle);
            this.pnlTplEdit.Controls.Add(this.lblTplCode);
            this.pnlTplEdit.Controls.Add(this.txtTplCode);
            this.pnlTplEdit.Controls.Add(this.lblTplName);
            this.pnlTplEdit.Controls.Add(this.txtTplName);
            this.pnlTplEdit.Controls.Add(this.lblTplDesc);
            this.pnlTplEdit.Controls.Add(this.txtTplDesc);
            this.pnlTplEdit.Controls.Add(this.chkTplActive);
            this.pnlTplEdit.Controls.Add(this.chkIsLegalDict);
            this.pnlTplEdit.Controls.Add(this.btnSaveTpl);
            this.pnlTplEdit.Controls.Add(this.btnCancelTpl);
            this.pnlTplEdit.Controls.Add(this.btnDeleteTpl);
            this.pnlTplEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTplEdit.Location = new System.Drawing.Point(0, 0);
            this.pnlTplEdit.Name = "pnlTplEdit";
            this.pnlTplEdit.Size = new System.Drawing.Size(604, 476);
            this.pnlTplEdit.TabIndex = 0;
            // 
            // lblTplEditTitle
            // 
            this.lblTplEditTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTplEditTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblTplEditTitle.Location = new System.Drawing.Point(12, 14);
            this.lblTplEditTitle.Name = "lblTplEditTitle";
            this.lblTplEditTitle.Size = new System.Drawing.Size(420, 22);
            this.lblTplEditTitle.TabIndex = 0;
            this.lblTplEditTitle.Text = "── เพิ่มประเภทโครงการใหม่ ──";
            // 
            // lblTplCode
            // 
            this.lblTplCode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTplCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblTplCode.Location = new System.Drawing.Point(12, 52);
            this.lblTplCode.Name = "lblTplCode";
            this.lblTplCode.Size = new System.Drawing.Size(90, 23);
            this.lblTplCode.TabIndex = 1;
            this.lblTplCode.Text = "รหัส:";
            this.lblTplCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTplCode
            // 
            this.txtTplCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTplCode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTplCode.Location = new System.Drawing.Point(108, 49);
            this.txtTplCode.MaxLength = 50;
            this.txtTplCode.Name = "txtTplCode";
            this.txtTplCode.Size = new System.Drawing.Size(180, 23);
            this.txtTplCode.TabIndex = 2;
            // 
            // lblTplName
            // 
            this.lblTplName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTplName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblTplName.Location = new System.Drawing.Point(12, 90);
            this.lblTplName.Name = "lblTplName";
            this.lblTplName.Size = new System.Drawing.Size(90, 23);
            this.lblTplName.TabIndex = 3;
            this.lblTplName.Text = "ชื่อ *:";
            this.lblTplName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTplName
            // 
            this.txtTplName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTplName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTplName.Location = new System.Drawing.Point(108, 87);
            this.txtTplName.MaxLength = 200;
            this.txtTplName.Name = "txtTplName";
            this.txtTplName.Size = new System.Drawing.Size(340, 23);
            this.txtTplName.TabIndex = 4;
            // 
            // lblTplDesc
            // 
            this.lblTplDesc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTplDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.lblTplDesc.Location = new System.Drawing.Point(12, 128);
            this.lblTplDesc.Name = "lblTplDesc";
            this.lblTplDesc.Size = new System.Drawing.Size(90, 23);
            this.lblTplDesc.TabIndex = 5;
            this.lblTplDesc.Text = "คำอธิบาย:";
            this.lblTplDesc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTplDesc
            // 
            this.txtTplDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTplDesc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTplDesc.Location = new System.Drawing.Point(108, 125);
            this.txtTplDesc.MaxLength = 500;
            this.txtTplDesc.Multiline = true;
            this.txtTplDesc.Name = "txtTplDesc";
            this.txtTplDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTplDesc.Size = new System.Drawing.Size(340, 75);
            this.txtTplDesc.TabIndex = 6;
            // 
            // chkTplActive
            // 
            this.chkTplActive.Checked = true;
            this.chkTplActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTplActive.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkTplActive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.chkTplActive.Location = new System.Drawing.Point(108, 214);
            this.chkTplActive.Name = "chkTplActive";
            this.chkTplActive.Size = new System.Drawing.Size(180, 22);
            this.chkTplActive.TabIndex = 7;
            this.chkTplActive.Text = "เปิดใช้งาน (Active)";
            // 
            // chkIsLegalDict
            // 
            this.chkIsLegalDict.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkIsLegalDict.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(80)))), ((int)(((byte)(160)))));
            this.chkIsLegalDict.Location = new System.Drawing.Point(108, 240);
            this.chkIsLegalDict.Name = "chkIsLegalDict";
            this.chkIsLegalDict.Size = new System.Drawing.Size(300, 22);
            this.chkIsLegalDict.TabIndex = 8;
            this.chkIsLegalDict.Text = "เป็นประเภทโครงการพจนานุกรมข้อมูลกฎหมาย";
            // 
            // btnSaveTpl
            // 
            this.btnSaveTpl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnSaveTpl.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveTpl.FlatAppearance.BorderSize = 0;
            this.btnSaveTpl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveTpl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveTpl.ForeColor = System.Drawing.Color.White;
            this.btnSaveTpl.Location = new System.Drawing.Point(108, 276);
            this.btnSaveTpl.Name = "btnSaveTpl";
            this.btnSaveTpl.Size = new System.Drawing.Size(90, 30);
            this.btnSaveTpl.TabIndex = 11;
            this.btnSaveTpl.Text = "บันทึก";
            this.btnSaveTpl.UseVisualStyleBackColor = false;
            this.btnSaveTpl.Click += new System.EventHandler(this.btnSaveTpl_Click);
            // 
            // btnCancelTpl
            // 
            this.btnCancelTpl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnCancelTpl.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelTpl.FlatAppearance.BorderSize = 0;
            this.btnCancelTpl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelTpl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancelTpl.ForeColor = System.Drawing.Color.White;
            this.btnCancelTpl.Location = new System.Drawing.Point(216, 276);
            this.btnCancelTpl.Name = "btnCancelTpl";
            this.btnCancelTpl.Size = new System.Drawing.Size(85, 30);
            this.btnCancelTpl.TabIndex = 12;
            this.btnCancelTpl.Text = "ยกเลิก";
            this.btnCancelTpl.UseVisualStyleBackColor = false;
            this.btnCancelTpl.Click += new System.EventHandler(this.btnCancelTpl_Click);
            // 
            // btnDeleteTpl
            // 
            this.btnDeleteTpl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.btnDeleteTpl.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteTpl.Enabled = false;
            this.btnDeleteTpl.FlatAppearance.BorderSize = 0;
            this.btnDeleteTpl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteTpl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDeleteTpl.ForeColor = System.Drawing.Color.White;
            this.btnDeleteTpl.Location = new System.Drawing.Point(321, 276);
            this.btnDeleteTpl.Name = "btnDeleteTpl";
            this.btnDeleteTpl.Size = new System.Drawing.Size(85, 30);
            this.btnDeleteTpl.TabIndex = 12;
            this.btnDeleteTpl.Text = "🗑 ลบ";
            this.btnDeleteTpl.UseVisualStyleBackColor = false;
            this.btnDeleteTpl.Click += new System.EventHandler(this.btnDeleteTpl_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.tabPage2.Controls.Add(this.splitDoc);
            this.tabPage2.Location = new System.Drawing.Point(4, 34);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(1192, 484);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "  📄 ขั้นตอนและเอกสาร";
            // 
            // splitDoc
            // 
            this.splitDoc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDoc.Location = new System.Drawing.Point(4, 4);
            this.splitDoc.Name = "splitDoc";
            // 
            // splitDoc.Panel1
            // 
            this.splitDoc.Panel1.Controls.Add(this.dgvStates);
            this.splitDoc.Panel1.Controls.Add(this.pnlTplHeader);
            this.splitDoc.Panel1.Controls.Add(this.pnlStateToolbar);
            // 
            // splitDoc.Panel2
            // 
            this.splitDoc.Panel2.Controls.Add(this.dgvDocs);
            this.splitDoc.Panel2.Controls.Add(this.pnlDocHeader);
            this.splitDoc.Panel2.Controls.Add(this.pnlBottom);
            this.splitDoc.Size = new System.Drawing.Size(1184, 476);
            this.splitDoc.SplitterDistance = 615;
            this.splitDoc.TabIndex = 0;
            // 
            // dgvStates
            // 
            this.dgvStates.AllowUserToAddRows = false;
            this.dgvStates.AllowUserToDeleteRows = false;
            this.dgvStates.BackgroundColor = System.Drawing.Color.White;
            this.dgvStates.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvStates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvStates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStates.Location = new System.Drawing.Point(0, 58);
            this.dgvStates.MultiSelect = false;
            this.dgvStates.Name = "dgvStates";
            this.dgvStates.ReadOnly = true;
            this.dgvStates.RowHeadersVisible = false;
            this.dgvStates.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStates.Size = new System.Drawing.Size(615, 346);
            this.dgvStates.TabIndex = 1;
            this.dgvStates.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvStates_CellDoubleClick);
            this.dgvStates.SelectionChanged += new System.EventHandler(this.dgvStates_SelectionChanged);
            // 
            // pnlTplHeader
            // 
            this.pnlTplHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.pnlTplHeader.Controls.Add(this.lblTplSelect);
            this.pnlTplHeader.Controls.Add(this.cbbTemplate);
            this.pnlTplHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTplHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlTplHeader.Name = "pnlTplHeader";
            this.pnlTplHeader.Size = new System.Drawing.Size(615, 58);
            this.pnlTplHeader.TabIndex = 0;
            // 
            // lblTplSelect
            // 
            this.lblTplSelect.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTplSelect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblTplSelect.Location = new System.Drawing.Point(4, 6);
            this.lblTplSelect.Name = "lblTplSelect";
            this.lblTplSelect.Size = new System.Drawing.Size(282, 18);
            this.lblTplSelect.TabIndex = 0;
            this.lblTplSelect.Text = "เส้นงาน Workflow";
            // 
            // cbbTemplate
            // 
            this.cbbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbbTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbTemplate.Location = new System.Drawing.Point(4, 30);
            this.cbbTemplate.Name = "cbbTemplate";
            this.cbbTemplate.Size = new System.Drawing.Size(282, 23);
            this.cbbTemplate.TabIndex = 1;
            this.cbbTemplate.SelectedIndexChanged += new System.EventHandler(this.cbbTemplate_SelectedIndexChanged);
            // 
            // pnlStateToolbar
            // 
            this.pnlStateToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(234)))), ((int)(((byte)(248)))));
            this.pnlStateToolbar.Controls.Add(this.btnAddState);
            this.pnlStateToolbar.Controls.Add(this.btnEditState);
            this.pnlStateToolbar.Controls.Add(this.btnDeleteState);
            this.pnlStateToolbar.Controls.Add(this.btnCopyStates);
            this.pnlStateToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStateToolbar.Location = new System.Drawing.Point(0, 404);
            this.pnlStateToolbar.Name = "pnlStateToolbar";
            this.pnlStateToolbar.Size = new System.Drawing.Size(615, 72);
            this.pnlStateToolbar.TabIndex = 2;
            // 
            // btnAddState
            // 
            this.btnAddState.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnAddState.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddState.Enabled = false;
            this.btnAddState.FlatAppearance.BorderSize = 0;
            this.btnAddState.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddState.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnAddState.ForeColor = System.Drawing.Color.White;
            this.btnAddState.Location = new System.Drawing.Point(4, 5);
            this.btnAddState.Name = "btnAddState";
            this.btnAddState.Size = new System.Drawing.Size(85, 28);
            this.btnAddState.TabIndex = 0;
            this.btnAddState.Text = "+ ขั้นตอน";
            this.btnAddState.UseVisualStyleBackColor = false;
            this.btnAddState.Click += new System.EventHandler(this.btnAddState_Click);
            // 
            // btnEditState
            // 
            this.btnEditState.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(110)))), ((int)(((byte)(165)))));
            this.btnEditState.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditState.Enabled = false;
            this.btnEditState.FlatAppearance.BorderSize = 0;
            this.btnEditState.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditState.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnEditState.ForeColor = System.Drawing.Color.White;
            this.btnEditState.Location = new System.Drawing.Point(96, 5);
            this.btnEditState.Name = "btnEditState";
            this.btnEditState.Size = new System.Drawing.Size(85, 28);
            this.btnEditState.TabIndex = 1;
            this.btnEditState.Text = "✏ แก้ไข";
            this.btnEditState.UseVisualStyleBackColor = false;
            this.btnEditState.Click += new System.EventHandler(this.btnEditState_Click);
            // 
            // btnDeleteState
            // 
            this.btnDeleteState.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.btnDeleteState.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteState.Enabled = false;
            this.btnDeleteState.FlatAppearance.BorderSize = 0;
            this.btnDeleteState.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteState.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnDeleteState.ForeColor = System.Drawing.Color.White;
            this.btnDeleteState.Location = new System.Drawing.Point(188, 5);
            this.btnDeleteState.Name = "btnDeleteState";
            this.btnDeleteState.Size = new System.Drawing.Size(85, 28);
            this.btnDeleteState.TabIndex = 2;
            this.btnDeleteState.Text = "🗑 ลบ";
            this.btnDeleteState.UseVisualStyleBackColor = false;
            this.btnDeleteState.Click += new System.EventHandler(this.btnDeleteState_Click);
            // 
            // btnCopyStates
            // 
            this.btnCopyStates.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(130)))), ((int)(((byte)(120)))));
            this.btnCopyStates.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCopyStates.FlatAppearance.BorderSize = 0;
            this.btnCopyStates.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopyStates.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnCopyStates.ForeColor = System.Drawing.Color.White;
            this.btnCopyStates.Location = new System.Drawing.Point(1, 40);
            this.btnCopyStates.Name = "btnCopyStates";
            this.btnCopyStates.Size = new System.Drawing.Size(140, 24);
            this.btnCopyStates.TabIndex = 3;
            this.btnCopyStates.Text = "📋 คัดลอกขั้นตอนจาก...";
            this.btnCopyStates.UseVisualStyleBackColor = false;
            this.btnCopyStates.Click += new System.EventHandler(this.btnCopyStates_Click);
            // 
            // dgvDocs
            // 
            this.dgvDocs.AllowUserToAddRows = false;
            this.dgvDocs.AllowUserToDeleteRows = false;
            this.dgvDocs.BackgroundColor = System.Drawing.Color.White;
            this.dgvDocs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDocs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvDocs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDocs.Location = new System.Drawing.Point(0, 32);
            this.dgvDocs.MultiSelect = false;
            this.dgvDocs.Name = "dgvDocs";
            this.dgvDocs.ReadOnly = true;
            this.dgvDocs.RowHeadersVisible = false;
            this.dgvDocs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDocs.Size = new System.Drawing.Size(565, 204);
            this.dgvDocs.TabIndex = 1;
            this.dgvDocs.SelectionChanged += new System.EventHandler(this.dgvDocs_SelectionChanged);
            this.dgvDocs.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDocs_CellDoubleClick);
            //
            // pnlDocHeader
            //
            this.pnlDocHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.pnlDocHeader.Controls.Add(this.btnAddDoc);
            this.pnlDocHeader.Controls.Add(this.lblSelectedState);
            this.pnlDocHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDocHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlDocHeader.Name = "pnlDocHeader";
            this.pnlDocHeader.Size = new System.Drawing.Size(565, 32);
            this.pnlDocHeader.TabIndex = 0;
            //
            // lblSelectedState
            //
            this.lblSelectedState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSelectedState.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblSelectedState.ForeColor = System.Drawing.Color.White;
            this.lblSelectedState.Location = new System.Drawing.Point(0, 0);
            this.lblSelectedState.Name = "lblSelectedState";
            this.lblSelectedState.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblSelectedState.Size = new System.Drawing.Size(565, 32);
            this.lblSelectedState.TabIndex = 0;
            this.lblSelectedState.Text = "← เลือกขั้นตอนจากรายการทางซ้าย";
            this.lblSelectedState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnAddDoc
            //
            this.btnAddDoc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(185)))), ((int)(((byte)(0)))));
            this.btnAddDoc.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddDoc.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAddDoc.FlatAppearance.BorderSize = 0;
            this.btnAddDoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddDoc.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddDoc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.btnAddDoc.Location = new System.Drawing.Point(425, 0);
            this.btnAddDoc.Name = "btnAddDoc";
            this.btnAddDoc.Size = new System.Drawing.Size(140, 32);
            this.btnAddDoc.TabIndex = 1;
            this.btnAddDoc.Text = "+ เพิ่มเอกสาร";
            this.btnAddDoc.UseVisualStyleBackColor = false;
            this.btnAddDoc.Click += new System.EventHandler(this.btnAddDoc_Click);
            //
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlBottom.Controls.Add(this.btnDelete);
            this.pnlBottom.Controls.Add(this.btnClose);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 426);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(565, 50);
            this.pnlBottom.TabIndex = 3;
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(10, 10);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(110, 30);
            this.btnDelete.TabIndex = 0;
            this.btnDelete.Text = "ลบเอกสาร";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(168)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(180, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(110, 30);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "ปิด";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Frm_Template_Doc_Edit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1200, 575);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(940, 560);
            this.Name = "Frm_Template_Doc_Edit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "จัดการเส้นงาน Workflow ขั้นตอน และเอกสาร";
            this.Load += new System.EventHandler(this.Frm_Template_Doc_Edit_Load);
            this.pnlHeader.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTemplates)).EndInit();
            this.pnlTplEdit.ResumeLayout(false);
            this.pnlTplEdit.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.splitDoc.Panel1.ResumeLayout(false);
            this.splitDoc.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDoc)).EndInit();
            this.splitDoc.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStates)).EndInit();
            this.pnlTplHeader.ResumeLayout(false);
            this.pnlStateToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocs)).EndInit();
            this.pnlDocHeader.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        // ── Control declarations ───────────────────────────────────────────────
        private System.Windows.Forms.Panel          pnlHeader;
        private System.Windows.Forms.Label          lblTitle;
        private System.Windows.Forms.Panel          pnlAccent;
        private System.Windows.Forms.TabControl     tabMain;

        // Tab1
        private System.Windows.Forms.TabPage        tabPage1;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.DataGridView   dgvTemplates;
        private System.Windows.Forms.Button         btnDeleteTpl;
        private System.Windows.Forms.Panel          pnlTplEdit;
        private System.Windows.Forms.Label          lblTplEditTitle;
        private System.Windows.Forms.Label          lblTplCode;
        private System.Windows.Forms.TextBox        txtTplCode;
        private System.Windows.Forms.Label          lblTplName;
        private System.Windows.Forms.TextBox        txtTplName;
        private System.Windows.Forms.Label          lblTplDesc;
        private System.Windows.Forms.TextBox        txtTplDesc;
        private System.Windows.Forms.CheckBox       chkTplActive;
        private System.Windows.Forms.CheckBox       chkIsLegalDict;
        private System.Windows.Forms.Button         btnSaveTpl;
        private System.Windows.Forms.Button         btnCancelTpl;

        // Tab2
        private System.Windows.Forms.TabPage        tabPage2;
        private System.Windows.Forms.SplitContainer splitDoc;
        private System.Windows.Forms.Panel          pnlTplHeader;
        private System.Windows.Forms.Label          lblTplSelect;
        private System.Windows.Forms.ComboBox       cbbTemplate;
        private System.Windows.Forms.DataGridView   dgvStates;
        private System.Windows.Forms.Panel          pnlStateToolbar;
        private System.Windows.Forms.Button         btnAddState;
        private System.Windows.Forms.Button         btnEditState;
        private System.Windows.Forms.Button         btnDeleteState;
        private System.Windows.Forms.Button         btnCopyStates;
        private System.Windows.Forms.Panel          pnlDocHeader;
        private System.Windows.Forms.Label          lblSelectedState;
        private System.Windows.Forms.Button         btnAddDoc;
        private System.Windows.Forms.DataGridView   dgvDocs;
        private System.Windows.Forms.Panel          pnlBottom;
        private System.Windows.Forms.Button         btnDelete;
        private System.Windows.Forms.Button         btnClose;
    }
}
