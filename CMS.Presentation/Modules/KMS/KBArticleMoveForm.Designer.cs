namespace CMS.Presentation.Modules.KMS
{
    partial class KBArticleMoveForm
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
            this.pnlCenter = new System.Windows.Forms.Panel();
            this.dgvArticles = new System.Windows.Forms.DataGridView();
            this.colSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTitleCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlArticleTools = new System.Windows.Forms.Panel();
            this.btnSelectNone = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.lblArticleCount = new System.Windows.Forms.Label();
            this.lblArticlesTitle = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.treeDestination = new System.Windows.Forms.TreeView();
            this.lblDestPath = new System.Windows.Forms.Label();
            this.lblDestTitle = new System.Windows.Forms.Label();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.treeSource = new System.Windows.Forms.TreeView();
            this.pnlSourceMode = new System.Windows.Forms.Panel();
            this.chkIncludeSub = new System.Windows.Forms.CheckBox();
            this.rbIndividual = new System.Windows.Forms.RadioButton();
            this.rbWholeCategory = new System.Windows.Forms.RadioButton();
            this.lblSourceTitle = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.pnlSummaryText = new System.Windows.Forms.Panel();
            this.txtWarnings = new System.Windows.Forms.TextBox();
            this.lblSummary = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnMove = new System.Windows.Forms.Button();
            this.lblSummaryTitle = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlCenter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvArticles)).BeginInit();
            this.pnlArticleTools.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlSourceMode.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.pnlSummaryText.SuspendLayout();
            this.pnlButtons.SuspendLayout();
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
            this.lblHeader.Text = "  📦 ย้ายบทความไปหมวดอื่น — Knowledge Base (MOD-006)";
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
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlMain.Controls.Add(this.pnlCenter);
            this.pnlMain.Controls.Add(this.pnlRight);
            this.pnlMain.Controls.Add(this.pnlLeft);
            this.pnlMain.Controls.Add(this.pnlBottom);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 50);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(12);
            this.pnlMain.Size = new System.Drawing.Size(1200, 750);
            this.pnlMain.TabIndex = 1;
            //
            // pnlCenter
            //
            this.pnlCenter.Controls.Add(this.dgvArticles);
            this.pnlCenter.Controls.Add(this.pnlArticleTools);
            this.pnlCenter.Controls.Add(this.lblArticlesTitle);
            this.pnlCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCenter.Location = new System.Drawing.Point(372, 12);
            this.pnlCenter.Name = "pnlCenter";
            this.pnlCenter.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.pnlCenter.Size = new System.Drawing.Size(456, 546);
            this.pnlCenter.TabIndex = 1;
            //
            // lblArticlesTitle
            //
            this.lblArticlesTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblArticlesTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblArticlesTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblArticlesTitle.Location = new System.Drawing.Point(8, 0);
            this.lblArticlesTitle.Name = "lblArticlesTitle";
            this.lblArticlesTitle.Size = new System.Drawing.Size(440, 30);
            this.lblArticlesTitle.TabIndex = 0;
            this.lblArticlesTitle.Text = "รายการบทความต้นทาง";
            this.lblArticlesTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlArticleTools
            //
            this.pnlArticleTools.Controls.Add(this.btnSelectNone);
            this.pnlArticleTools.Controls.Add(this.btnSelectAll);
            this.pnlArticleTools.Controls.Add(this.lblArticleCount);
            this.pnlArticleTools.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlArticleTools.Location = new System.Drawing.Point(8, 30);
            this.pnlArticleTools.Name = "pnlArticleTools";
            this.pnlArticleTools.Size = new System.Drawing.Size(440, 36);
            this.pnlArticleTools.TabIndex = 1;
            //
            // btnSelectNone
            //
            this.btnSelectNone.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSelectNone.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSelectNone.Location = new System.Drawing.Point(300, 0);
            this.btnSelectNone.Name = "btnSelectNone";
            this.btnSelectNone.Size = new System.Drawing.Size(70, 36);
            this.btnSelectNone.TabIndex = 2;
            this.btnSelectNone.Text = "ล้างเลือก";
            this.btnSelectNone.UseVisualStyleBackColor = true;
            this.btnSelectNone.Click += new System.EventHandler(this.BtnSelectNone_Click);
            //
            // btnSelectAll
            //
            this.btnSelectAll.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSelectAll.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSelectAll.Location = new System.Drawing.Point(370, 0);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(70, 36);
            this.btnSelectAll.TabIndex = 1;
            this.btnSelectAll.Text = "เลือกทั้งหมด";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
            //
            // lblArticleCount
            //
            this.lblArticleCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblArticleCount.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblArticleCount.Location = new System.Drawing.Point(0, 0);
            this.lblArticleCount.Name = "lblArticleCount";
            this.lblArticleCount.Size = new System.Drawing.Size(440, 36);
            this.lblArticleCount.TabIndex = 0;
            this.lblArticleCount.Text = "ยังไม่ได้เลือกหมวดต้นทาง";
            this.lblArticleCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // dgvArticles
            //
            this.dgvArticles.AllowUserToAddRows = false;
            this.dgvArticles.AllowUserToDeleteRows = false;
            this.dgvArticles.AllowUserToResizeRows = false;
            this.dgvArticles.BackgroundColor = System.Drawing.Color.White;
            this.dgvArticles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvArticles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSelect,
            this.colCode,
            this.colTitle,
            this.colTitleCategory,
            this.colStatus,
            this.colCategory});
            this.dgvArticles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvArticles.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvArticles.Location = new System.Drawing.Point(8, 66);
            this.dgvArticles.MultiSelect = false;
            this.dgvArticles.Name = "dgvArticles";
            this.dgvArticles.RowHeadersVisible = false;
            this.dgvArticles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvArticles.Size = new System.Drawing.Size(440, 480);
            this.dgvArticles.TabIndex = 2;
            this.dgvArticles.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvArticles_CellValueChanged);
            this.dgvArticles.CurrentCellDirtyStateChanged += new System.EventHandler(this.DgvArticles_CurrentCellDirtyStateChanged);
            //
            // colSelect
            //
            this.colSelect.HeaderText = "ย้าย";
            this.colSelect.Name = "colSelect";
            this.colSelect.Width = 45;
            //
            // colCode
            //
            this.colCode.HeaderText = "รหัสบทความ";
            this.colCode.Name = "colCode";
            this.colCode.ReadOnly = true;
            this.colCode.Width = 150;
            //
            // colTitle
            //
            this.colTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTitle.HeaderText = "ชื่อเรื่อง";
            this.colTitle.MinimumWidth = 70;
            this.colTitle.Name = "colTitle";
            this.colTitle.ReadOnly = true;
                        //
            // colTitleCategory
            //
            this.colTitleCategory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTitleCategory.HeaderText = "หัวข้อ";
            this.colTitleCategory.MinimumWidth = 70;
            this.colTitleCategory.Name = "colTitleCategory";
            this.colTitleCategory.ReadOnly = true;
                        //
            // colStatus
            //
            this.colStatus.HeaderText = "สถานะ";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            this.colStatus.Width = 75;
            //
            // colCategory
            //
            this.colCategory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCategory.HeaderText = "หมวดปัจจุบัน";
            this.colCategory.MinimumWidth = 80;
            this.colCategory.Name = "colCategory";
            this.colCategory.ReadOnly = true;
            //
            // pnlRight
            //
            this.pnlRight.Controls.Add(this.treeDestination);
            this.pnlRight.Controls.Add(this.lblDestPath);
            this.pnlRight.Controls.Add(this.lblDestTitle);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRight.Location = new System.Drawing.Point(828, 12);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(330, 546);
            this.pnlRight.TabIndex = 2;
            //
            // lblDestTitle
            //
            this.lblDestTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDestTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblDestTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblDestTitle.Location = new System.Drawing.Point(0, 0);
            this.lblDestTitle.Name = "lblDestTitle";
            this.lblDestTitle.Size = new System.Drawing.Size(360, 30);
            this.lblDestTitle.TabIndex = 0;
            this.lblDestTitle.Text = "② เลือกหมวดปลายทาง (เลือกได้หมวดเดียว)";
            this.lblDestTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblDestPath
            //
            this.lblDestPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(245)))), ((int)(((byte)(233)))));
            this.lblDestPath.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDestPath.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDestPath.Location = new System.Drawing.Point(0, 486);
            this.lblDestPath.Name = "lblDestPath";
            this.lblDestPath.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblDestPath.Size = new System.Drawing.Size(360, 60);
            this.lblDestPath.TabIndex = 2;
            this.lblDestPath.Text = "ปลายทางที่เลือก: —";
            this.lblDestPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // treeDestination
            //
            this.treeDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeDestination.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.treeDestination.FullRowSelect = true;
            this.treeDestination.HideSelection = false;
            this.treeDestination.Indent = 16;
            this.treeDestination.Location = new System.Drawing.Point(0, 30);
            this.treeDestination.Name = "treeDestination";
            this.treeDestination.ShowLines = false;
            this.treeDestination.Size = new System.Drawing.Size(360, 456);
            this.treeDestination.TabIndex = 1;
            this.treeDestination.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeDestination_AfterSelect);
            //
            // pnlLeft
            //
            this.pnlLeft.Controls.Add(this.treeSource);
            this.pnlLeft.Controls.Add(this.pnlSourceMode);
            this.pnlLeft.Controls.Add(this.lblSourceTitle);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(12, 12);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(330, 546);
            this.pnlLeft.TabIndex = 0;
            //
            // lblSourceTitle
            //
            this.lblSourceTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSourceTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSourceTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblSourceTitle.Location = new System.Drawing.Point(0, 0);
            this.lblSourceTitle.Name = "lblSourceTitle";
            this.lblSourceTitle.Size = new System.Drawing.Size(360, 30);
            this.lblSourceTitle.TabIndex = 0;
            this.lblSourceTitle.Text = "① เลือกบทความต้นทาง";
            this.lblSourceTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlSourceMode
            //
            this.pnlSourceMode.Controls.Add(this.chkIncludeSub);
            this.pnlSourceMode.Controls.Add(this.rbIndividual);
            this.pnlSourceMode.Controls.Add(this.rbWholeCategory);
            this.pnlSourceMode.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSourceMode.Location = new System.Drawing.Point(0, 30);
            this.pnlSourceMode.Name = "pnlSourceMode";
            this.pnlSourceMode.Size = new System.Drawing.Size(360, 92);
            this.pnlSourceMode.TabIndex = 1;
            //
            // rbWholeCategory
            //
            this.rbWholeCategory.AutoSize = true;
            this.rbWholeCategory.Checked = true;
            this.rbWholeCategory.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.rbWholeCategory.Location = new System.Drawing.Point(4, 4);
            this.rbWholeCategory.Name = "rbWholeCategory";
            this.rbWholeCategory.Size = new System.Drawing.Size(300, 23);
            this.rbWholeCategory.TabIndex = 0;
            this.rbWholeCategory.TabStop = true;
            this.rbWholeCategory.Text = "ย้ายทั้งหมวด (ติ๊กเลือกหมวดในต้นไม้ด้านล่าง)";
            this.rbWholeCategory.UseVisualStyleBackColor = true;
            this.rbWholeCategory.CheckedChanged += new System.EventHandler(this.RbSourceMode_CheckedChanged);
            //
            // rbIndividual
            //
            this.rbIndividual.AutoSize = true;
            this.rbIndividual.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.rbIndividual.Location = new System.Drawing.Point(4, 32);
            this.rbIndividual.Name = "rbIndividual";
            this.rbIndividual.Size = new System.Drawing.Size(320, 23);
            this.rbIndividual.TabIndex = 1;
            this.rbIndividual.Text = "เลือกรายบทความ (คลิกหมวด แล้วติ๊กบทความในตาราง)";
            this.rbIndividual.UseVisualStyleBackColor = true;
            this.rbIndividual.CheckedChanged += new System.EventHandler(this.RbSourceMode_CheckedChanged);
            //
            // chkIncludeSub
            //
            this.chkIncludeSub.AutoSize = true;
            this.chkIncludeSub.Checked = true;
            this.chkIncludeSub.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeSub.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkIncludeSub.Location = new System.Drawing.Point(4, 62);
            this.chkIncludeSub.Name = "chkIncludeSub";
            this.chkIncludeSub.Size = new System.Drawing.Size(200, 23);
            this.chkIncludeSub.TabIndex = 2;
            this.chkIncludeSub.Text = "รวมบทความของหมวดย่อยทุกชั้น";
            this.chkIncludeSub.UseVisualStyleBackColor = true;
            this.chkIncludeSub.CheckedChanged += new System.EventHandler(this.ChkIncludeSub_CheckedChanged);
            //
            // treeSource
            //
            this.treeSource.CheckBoxes = true;
            this.treeSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeSource.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.treeSource.FullRowSelect = true;
            this.treeSource.HideSelection = false;
            this.treeSource.Indent = 16;
            this.treeSource.Location = new System.Drawing.Point(0, 122);
            this.treeSource.Name = "treeSource";
            this.treeSource.ShowLines = false;
            this.treeSource.Size = new System.Drawing.Size(360, 424);
            this.treeSource.TabIndex = 2;
            this.treeSource.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TreeSource_AfterCheck);
            this.treeSource.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeSource_AfterSelect);
            //
            // pnlBottom
            //
            this.pnlBottom.BackColor = System.Drawing.Color.White;
            this.pnlBottom.Controls.Add(this.pnlSummaryText);
            this.pnlBottom.Controls.Add(this.pnlButtons);
            this.pnlBottom.Controls.Add(this.lblSummaryTitle);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(12, 558);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Padding = new System.Windows.Forms.Padding(12, 6, 12, 10);
            this.pnlBottom.Size = new System.Drawing.Size(1176, 180);
            this.pnlBottom.TabIndex = 3;
            //
            // lblSummaryTitle
            //
            this.lblSummaryTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSummaryTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSummaryTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblSummaryTitle.Location = new System.Drawing.Point(12, 6);
            this.lblSummaryTitle.Name = "lblSummaryTitle";
            this.lblSummaryTitle.Size = new System.Drawing.Size(1152, 28);
            this.lblSummaryTitle.TabIndex = 0;
            this.lblSummaryTitle.Text = "③ สรุปและยืนยัน";
            this.lblSummaryTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Controls.Add(this.btnMove);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlButtons.Location = new System.Drawing.Point(854, 34);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(310, 136);
            this.pnlButtons.TabIndex = 1;
            //
            // btnMove
            //
            this.btnMove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.btnMove.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMove.Enabled = false;
            this.btnMove.FlatAppearance.BorderSize = 0;
            this.btnMove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMove.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnMove.ForeColor = System.Drawing.Color.White;
            this.btnMove.Location = new System.Drawing.Point(10, 8);
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(290, 48);
            this.btnMove.TabIndex = 0;
            this.btnMove.Text = "📦 ย้ายบทความ";
            this.btnMove.UseVisualStyleBackColor = false;
            this.btnMove.Click += new System.EventHandler(this.BtnMove_Click);
            //
            // btnClose
            //
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnClose.Location = new System.Drawing.Point(10, 66);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(290, 40);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "ปิด";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // pnlSummaryText
            //
            this.pnlSummaryText.Controls.Add(this.txtWarnings);
            this.pnlSummaryText.Controls.Add(this.lblSummary);
            this.pnlSummaryText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSummaryText.Location = new System.Drawing.Point(12, 34);
            this.pnlSummaryText.Name = "pnlSummaryText";
            this.pnlSummaryText.Size = new System.Drawing.Size(842, 136);
            this.pnlSummaryText.TabIndex = 2;
            //
            // lblSummary
            //
            this.lblSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblSummary.Location = new System.Drawing.Point(0, 0);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(842, 30);
            this.lblSummary.TabIndex = 0;
            this.lblSummary.Text = "ยังไม่ได้เลือกบทความต้นทาง";
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtWarnings
            //
            this.txtWarnings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(225)))));
            this.txtWarnings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtWarnings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtWarnings.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtWarnings.Location = new System.Drawing.Point(0, 30);
            this.txtWarnings.Multiline = true;
            this.txtWarnings.Name = "txtWarnings";
            this.txtWarnings.ReadOnly = true;
            this.txtWarnings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtWarnings.Size = new System.Drawing.Size(842, 106);
            this.txtWarnings.TabIndex = 1;
            this.txtWarnings.TabStop = false;
            //
            // KBArticleMoveForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.MinimumSize = new System.Drawing.Size(1000, 640);
            this.Name = "KBArticleMoveForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ย้ายบทความ — Knowledge Base";
            this.Load += new System.EventHandler(this.KBArticleMoveForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlCenter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvArticles)).EndInit();
            this.pnlArticleTools.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlSourceMode.ResumeLayout(false);
            this.pnlSourceMode.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.pnlSummaryText.ResumeLayout(false);
            this.pnlSummaryText.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlCenter;
        private System.Windows.Forms.DataGridView dgvArticles;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTitleCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCategory;
        private System.Windows.Forms.Panel pnlArticleTools;
        private System.Windows.Forms.Button btnSelectNone;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Label lblArticleCount;
        private System.Windows.Forms.Label lblArticlesTitle;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.TreeView treeDestination;
        private System.Windows.Forms.Label lblDestPath;
        private System.Windows.Forms.Label lblDestTitle;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.TreeView treeSource;
        private System.Windows.Forms.Panel pnlSourceMode;
        private System.Windows.Forms.CheckBox chkIncludeSub;
        private System.Windows.Forms.RadioButton rbIndividual;
        private System.Windows.Forms.RadioButton rbWholeCategory;
        private System.Windows.Forms.Label lblSourceTitle;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Panel pnlSummaryText;
        private System.Windows.Forms.TextBox txtWarnings;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnMove;
        private System.Windows.Forms.Label lblSummaryTitle;
    }
}
