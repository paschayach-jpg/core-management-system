namespace CMS.Presentation
{
    partial class ShareManagementForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle intHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle intCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle intAltStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle intDateStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle extHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle extCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle extAltStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle extDateStyle = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSub = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabInternal = new System.Windows.Forms.TabPage();
            this.dgvInternal = new System.Windows.Forms.DataGridView();
            this.colIntId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntSharer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntTarget = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntResource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntRights = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntCreated = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntExpiry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntEnded = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntNote = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlIntFilter = new System.Windows.Forms.Panel();
            this.rbIntActive = new System.Windows.Forms.RadioButton();
            this.rbIntInactive = new System.Windows.Forms.RadioButton();
            this.rbIntAll = new System.Windows.Forms.RadioButton();
            this.lblIntSearch = new System.Windows.Forms.Label();
            this.txtIntSearch = new System.Windows.Forms.TextBox();
            this.pnlIntBar = new System.Windows.Forms.Panel();
            this.btnViewDetailInternal = new System.Windows.Forms.Button();
            this.btnEditInternal = new System.Windows.Forms.Button();
            this.btnRevokeInternal = new System.Windows.Forms.Button();
            this.btnWithdrawInternal = new System.Windows.Forms.Button();
            this.lblIntHint = new System.Windows.Forms.Label();
            this.tabExternal = new System.Windows.Forms.TabPage();
            this.dgvExternal = new System.Windows.Forms.DataGridView();
            this.colExtId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExtSharer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExtResource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExtCreated = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExtStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExtExpiry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExtLimit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExtDownloaded = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExtEnded = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExtNote = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlExtFilter = new System.Windows.Forms.Panel();
            this.rbExtActive = new System.Windows.Forms.RadioButton();
            this.rbExtInactive = new System.Windows.Forms.RadioButton();
            this.rbExtAll = new System.Windows.Forms.RadioButton();
            this.lblExtSearch = new System.Windows.Forms.Label();
            this.txtExtSearch = new System.Windows.Forms.TextBox();
            this.pnlExtBar = new System.Windows.Forms.Panel();
            this.btnToggleExt = new System.Windows.Forms.Button();
            this.btnRevokeExt = new System.Windows.Forms.Button();
            this.btnApproveExt = new System.Windows.Forms.Button();
            this.btnRejectExt = new System.Windows.Forms.Button();
            this.lblExtHint = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabInternal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInternal)).BeginInit();
            this.pnlIntFilter.SuspendLayout();
            this.pnlIntBar.SuspendLayout();
            this.tabExternal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvExternal)).BeginInit();
            this.pnlExtFilter.SuspendLayout();
            this.pnlExtBar.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSub);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1000, 60);
            this.pnlHeader.TabIndex = 2;
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "📋  จัดการการแบ่งปัน  (Share Management)";
            //
            // lblSub
            //
            this.lblSub.AutoSize = true;
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblSub.ForeColor = System.Drawing.Color.FromArgb(170, 200, 240);
            this.lblSub.Location = new System.Drawing.Point(17, 36);
            this.lblSub.Name = "lblSub";
            this.lblSub.Text = "ดูและจัดการ Internal / External share ของคุณ";
            //
            // pnlFooter
            //
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(228, 234, 245);
            this.pnlFooter.Controls.Add(this.btnRefresh);
            this.pnlFooter.Controls.Add(this.btnClose);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 590);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1000, 50);
            this.pnlFooter.TabIndex = 1;
            //
            // btnRefresh
            //
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(220, 228, 245);
            this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.btnRefresh.Location = new System.Drawing.Point(12, 9);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(96, 32);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "↺  รีเฟรช";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(892, 9);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 32);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "✕  ปิด";
            this.btnClose.UseVisualStyleBackColor = false;
            //
            // pnlBody
            //
            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlBody.Controls.Add(this.tabControl);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 60);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(10, 8, 10, 0);
            this.pnlBody.Size = new System.Drawing.Size(1000, 530);
            this.pnlBody.TabIndex = 0;
            //
            // tabControl
            //
            this.tabControl.Controls.Add(this.tabInternal);
            this.tabControl.Controls.Add(this.tabExternal);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.tabControl.Location = new System.Drawing.Point(10, 8);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(12, 4);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(980, 522);
            this.tabControl.TabIndex = 0;
            //
            // tabInternal
            //
            this.tabInternal.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.tabInternal.Controls.Add(this.dgvInternal);
            this.tabInternal.Controls.Add(this.pnlIntFilter);
            this.tabInternal.Controls.Add(this.pnlIntBar);
            this.tabInternal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabInternal.Name = "tabInternal";
            this.tabInternal.Size = new System.Drawing.Size(972, 489);
            this.tabInternal.TabIndex = 0;
            this.tabInternal.Text = "  📋 Internal Shares  ";
            //
            // dgvInternal
            //
            this.dgvInternal.AllowUserToAddRows = false;
            this.dgvInternal.AllowUserToDeleteRows = false;
            this.dgvInternal.AllowUserToResizeColumns = true;
            this.dgvInternal.AllowUserToResizeRows = false;
            this.dgvInternal.AutoGenerateColumns = false;
            this.dgvInternal.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.dgvInternal.BackgroundColor = System.Drawing.Color.White;
            this.dgvInternal.BorderStyle = System.Windows.Forms.BorderStyle.None;
            intAltStyle.BackColor = System.Drawing.Color.FromArgb(249, 251, 255);
            this.dgvInternal.AlternatingRowsDefaultCellStyle = intAltStyle;
            intHeaderStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            intHeaderStyle.BackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            intHeaderStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            intHeaderStyle.ForeColor = System.Drawing.Color.White;
            intHeaderStyle.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            intHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.dgvInternal.ColumnHeadersDefaultCellStyle = intHeaderStyle;
            this.dgvInternal.ColumnHeadersHeight = 32;
            this.dgvInternal.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvInternal.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIntId,
            this.colIntStatus,
            this.colIntResource,
            this.colIntTarget,
            this.colIntRights,
            this.colIntSharer,
            this.colIntCreated,
            this.colIntExpiry,
            this.colIntEnded,
            this.colIntNote});
            intCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            intCellStyle.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            intCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(50, 90, 150);
            intCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dgvInternal.DefaultCellStyle = intCellStyle;
            this.dgvInternal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvInternal.EnableHeadersVisualStyles = false;
            this.dgvInternal.GridColor = System.Drawing.Color.FromArgb(228, 232, 240);
            this.dgvInternal.Name = "dgvInternal";
            this.dgvInternal.ReadOnly = true;
            this.dgvInternal.RowHeadersVisible = false;
            this.dgvInternal.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.dgvInternal.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvInternal.TabIndex = 1;
            this.dgvInternal.SelectionChanged += new System.EventHandler(this.DgvInternal_SelectionChanged);
            this.dgvInternal.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DgvInternal_DataBindingComplete);
            //
            // colIntId
            //
            this.colIntId.DataPropertyName = "Id";
            this.colIntId.Name = "colIntId";
            this.colIntId.Visible = false;
            //
            // colIntStatus
            //
            this.colIntStatus.DataPropertyName = "Status";
            this.colIntStatus.HeaderText = "สถานะ";
            this.colIntStatus.MinimumWidth = 80;
            this.colIntStatus.Name = "colIntStatus";
            this.colIntStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntStatus.Width = 110;
            //
            // colIntResource
            //
            this.colIntResource.DataPropertyName = "Resource";
            this.colIntResource.HeaderText = "ไฟล์ที่แชร์";
            this.colIntResource.MinimumWidth = 100;
            this.colIntResource.Name = "colIntResource";
            this.colIntResource.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntResource.Width = 250;
            //
            // colIntTarget
            //
            this.colIntTarget.DataPropertyName = "Target";
            this.colIntTarget.HeaderText = "แชร์ให้";
            this.colIntTarget.MinimumWidth = 100;
            this.colIntTarget.Name = "colIntTarget";
            this.colIntTarget.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntTarget.Width = 200;
            //
            // colIntRights
            //
            this.colIntRights.DataPropertyName = "Rights";
            this.colIntRights.HeaderText = "สิทธิ์";
            this.colIntRights.MinimumWidth = 70;
            this.colIntRights.Name = "colIntRights";
            this.colIntRights.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntRights.Width = 120;
            //
            // colIntSharer
            //
            this.colIntSharer.DataPropertyName = "Sharer";
            this.colIntSharer.HeaderText = "ผู้แชร์ (เจ้าของ)";
            this.colIntSharer.MinimumWidth = 90;
            this.colIntSharer.Name = "colIntSharer";
            this.colIntSharer.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntSharer.Width = 140;
            //
            // colIntCreated
            //
            this.colIntCreated.DataPropertyName = "Created";
            intDateStyle.Format = "g";
            this.colIntCreated.DefaultCellStyle = intDateStyle;
            this.colIntCreated.HeaderText = "วันที่แชร์";
            this.colIntCreated.MinimumWidth = 90;
            this.colIntCreated.Name = "colIntCreated";
            this.colIntCreated.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntCreated.Width = 125;
            //
            // colIntExpiry
            //
            this.colIntExpiry.DataPropertyName = "Expiry";
            this.colIntExpiry.HeaderText = "หมดอายุ";
            this.colIntExpiry.MinimumWidth = 80;
            this.colIntExpiry.Name = "colIntExpiry";
            this.colIntExpiry.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntExpiry.Width = 100;
            //
            // colIntEnded
            //
            this.colIntEnded.DataPropertyName = "Ended";
            this.colIntEnded.HeaderText = "สิ้นสุดเมื่อ";
            this.colIntEnded.MinimumWidth = 80;
            this.colIntEnded.Name = "colIntEnded";
            this.colIntEnded.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntEnded.Width = 125;
            //
            // colIntNote
            //
            this.colIntNote.DataPropertyName = "Note";
            this.colIntNote.HeaderText = "หมายเหตุ";
            this.colIntNote.MinimumWidth = 80;
            this.colIntNote.Name = "colIntNote";
            this.colIntNote.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntNote.Width = 180;
            //
            // pnlIntFilter
            //
            this.pnlIntFilter.BackColor = System.Drawing.Color.FromArgb(235, 240, 250);
            this.pnlIntFilter.Controls.Add(this.rbIntActive);
            this.pnlIntFilter.Controls.Add(this.rbIntInactive);
            this.pnlIntFilter.Controls.Add(this.rbIntAll);
            this.pnlIntFilter.Controls.Add(this.lblIntSearch);
            this.pnlIntFilter.Controls.Add(this.txtIntSearch);
            this.pnlIntFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlIntFilter.Name = "pnlIntFilter";
            this.pnlIntFilter.Size = new System.Drawing.Size(972, 44);
            this.pnlIntFilter.TabIndex = 0;
            //
            // rbIntActive
            //
            this.rbIntActive.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbIntActive.Checked = true;
            this.rbIntActive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.rbIntActive.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 120, 60);
            this.rbIntActive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbIntActive.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rbIntActive.Location = new System.Drawing.Point(8, 7);
            this.rbIntActive.Name = "rbIntActive";
            this.rbIntActive.Size = new System.Drawing.Size(150, 30);
            this.rbIntActive.TabIndex = 0;
            this.rbIntActive.TabStop = true;
            this.rbIntActive.Text = "● ใช้งานอยู่";
            this.rbIntActive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbIntActive.CheckedChanged += new System.EventHandler(this.InternalFilter_CheckedChanged);
            //
            // rbIntInactive
            //
            this.rbIntInactive.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbIntInactive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.rbIntInactive.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(110, 110, 120);
            this.rbIntInactive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbIntInactive.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rbIntInactive.Location = new System.Drawing.Point(162, 7);
            this.rbIntInactive.Name = "rbIntInactive";
            this.rbIntInactive.Size = new System.Drawing.Size(170, 30);
            this.rbIntInactive.TabIndex = 1;
            this.rbIntInactive.Text = "○ ไม่ได้ใช้งาน";
            this.rbIntInactive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbIntInactive.CheckedChanged += new System.EventHandler(this.InternalFilter_CheckedChanged);
            //
            // rbIntAll
            //
            this.rbIntAll.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbIntAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.rbIntAll.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.rbIntAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbIntAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rbIntAll.Location = new System.Drawing.Point(336, 7);
            this.rbIntAll.Name = "rbIntAll";
            this.rbIntAll.Size = new System.Drawing.Size(110, 30);
            this.rbIntAll.TabIndex = 2;
            this.rbIntAll.Text = "ทั้งหมด";
            this.rbIntAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbIntAll.CheckedChanged += new System.EventHandler(this.InternalFilter_CheckedChanged);
            //
            // lblIntSearch
            //
            this.lblIntSearch.AutoSize = true;
            this.lblIntSearch.Location = new System.Drawing.Point(466, 14);
            this.lblIntSearch.Name = "lblIntSearch";
            this.lblIntSearch.Text = "🔍 ค้นหา:";
            //
            // txtIntSearch
            //
            this.txtIntSearch.Location = new System.Drawing.Point(530, 11);
            this.txtIntSearch.Name = "txtIntSearch";
            this.txtIntSearch.Size = new System.Drawing.Size(260, 23);
            this.txtIntSearch.TabIndex = 3;
            this.txtIntSearch.TextChanged += new System.EventHandler(this.TxtIntSearch_TextChanged);
            //
            // pnlIntBar
            //
            this.pnlIntBar.BackColor = System.Drawing.Color.FromArgb(235, 240, 250);
            this.pnlIntBar.Controls.Add(this.btnViewDetailInternal);
            this.pnlIntBar.Controls.Add(this.btnEditInternal);
            this.pnlIntBar.Controls.Add(this.btnRevokeInternal);
            this.pnlIntBar.Controls.Add(this.btnWithdrawInternal);
            this.pnlIntBar.Controls.Add(this.lblIntHint);
            this.pnlIntBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlIntBar.Name = "pnlIntBar";
            this.pnlIntBar.Size = new System.Drawing.Size(972, 46);
            this.pnlIntBar.TabIndex = 2;
            //
            // btnViewDetailInternal
            //
            this.btnViewDetailInternal.BackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.btnViewDetailInternal.FlatAppearance.BorderSize = 0;
            this.btnViewDetailInternal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewDetailInternal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnViewDetailInternal.ForeColor = System.Drawing.Color.White;
            this.btnViewDetailInternal.Location = new System.Drawing.Point(8, 8);
            this.btnViewDetailInternal.Name = "btnViewDetailInternal";
            this.btnViewDetailInternal.Size = new System.Drawing.Size(150, 30);
            this.btnViewDetailInternal.TabIndex = 0;
            this.btnViewDetailInternal.Text = "📋  ดูรายละเอียดผู้รับ...";
            this.btnViewDetailInternal.UseVisualStyleBackColor = false;
            this.btnViewDetailInternal.Click += new System.EventHandler(this.BtnViewDetailInternal_Click);
            //
            // btnEditInternal
            //
            this.btnEditInternal.BackColor = System.Drawing.Color.FromArgb(232, 242, 255);
            this.btnEditInternal.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnEditInternal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditInternal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEditInternal.ForeColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.btnEditInternal.Location = new System.Drawing.Point(164, 8);
            this.btnEditInternal.Name = "btnEditInternal";
            this.btnEditInternal.Size = new System.Drawing.Size(120, 30);
            this.btnEditInternal.TabIndex = 1;
            this.btnEditInternal.Text = "⚙️  ขอปรับสิทธิ์";
            this.btnEditInternal.UseVisualStyleBackColor = false;
            this.btnEditInternal.Click += new System.EventHandler(this.BtnEditInternal_Click);
            //
            // btnRevokeInternal
            //
            this.btnRevokeInternal.BackColor = System.Drawing.Color.FromArgb(250, 235, 235);
            this.btnRevokeInternal.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnRevokeInternal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRevokeInternal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRevokeInternal.ForeColor = System.Drawing.Color.FromArgb(160, 30, 30);
            this.btnRevokeInternal.Location = new System.Drawing.Point(290, 8);
            this.btnRevokeInternal.Name = "btnRevokeInternal";
            this.btnRevokeInternal.Size = new System.Drawing.Size(130, 30);
            this.btnRevokeInternal.TabIndex = 2;
            this.btnRevokeInternal.Text = "🚫  ยกเลิกการแชร์";
            this.btnRevokeInternal.UseVisualStyleBackColor = false;
            this.btnRevokeInternal.Click += new System.EventHandler(this.BtnRevokeInternal_Click);
            //
            // btnWithdrawInternal
            //
            this.btnWithdrawInternal.BackColor = System.Drawing.Color.FromArgb(232, 240, 254);
            this.btnWithdrawInternal.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnWithdrawInternal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWithdrawInternal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnWithdrawInternal.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.btnWithdrawInternal.Location = new System.Drawing.Point(426, 8);
            this.btnWithdrawInternal.Name = "btnWithdrawInternal";
            this.btnWithdrawInternal.Size = new System.Drawing.Size(150, 30);
            this.btnWithdrawInternal.TabIndex = 3;
            this.btnWithdrawInternal.Text = "↩  ยกเลิกคำขอ";
            this.btnWithdrawInternal.UseVisualStyleBackColor = false;
            this.btnWithdrawInternal.Click += new System.EventHandler(this.BtnWithdrawInternal_Click);
            //
            // lblIntHint
            //
            this.lblIntHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblIntHint.AutoSize = false;
            this.lblIntHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblIntHint.ForeColor = System.Drawing.Color.FromArgb(70, 90, 130);
            this.lblIntHint.Location = new System.Drawing.Point(584, 3);
            this.lblIntHint.Size = new System.Drawing.Size(380, 40);
            this.lblIntHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblIntHint.Name = "lblIntHint";
            this.lblIntHint.Text = "";
            //
            // tabExternal
            //
            this.tabExternal.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.tabExternal.Controls.Add(this.dgvExternal);
            this.tabExternal.Controls.Add(this.pnlExtFilter);
            this.tabExternal.Controls.Add(this.pnlExtBar);
            this.tabExternal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabExternal.Name = "tabExternal";
            this.tabExternal.Size = new System.Drawing.Size(972, 489);
            this.tabExternal.TabIndex = 1;
            this.tabExternal.Text = "  🌐 External Shares  ";
            //
            // dgvExternal
            //
            this.dgvExternal.AllowUserToAddRows = false;
            this.dgvExternal.AllowUserToDeleteRows = false;
            this.dgvExternal.AllowUserToResizeColumns = true;
            this.dgvExternal.AllowUserToResizeRows = false;
            this.dgvExternal.AutoGenerateColumns = false;
            this.dgvExternal.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.dgvExternal.BackgroundColor = System.Drawing.Color.White;
            this.dgvExternal.BorderStyle = System.Windows.Forms.BorderStyle.None;
            extAltStyle.BackColor = System.Drawing.Color.FromArgb(249, 251, 255);
            this.dgvExternal.AlternatingRowsDefaultCellStyle = extAltStyle;
            extHeaderStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            extHeaderStyle.BackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            extHeaderStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            extHeaderStyle.ForeColor = System.Drawing.Color.White;
            extHeaderStyle.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            extHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.dgvExternal.ColumnHeadersDefaultCellStyle = extHeaderStyle;
            this.dgvExternal.ColumnHeadersHeight = 32;
            this.dgvExternal.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvExternal.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colExtId,
            this.colExtStatus,
            this.colExtResource,
            this.colExtSharer,
            this.colExtCreated,
            this.colExtExpiry,
            this.colExtLimit,
            this.colExtDownloaded,
            this.colExtEnded,
            this.colExtNote});
            extCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            extCellStyle.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            extCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(50, 90, 150);
            extCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dgvExternal.DefaultCellStyle = extCellStyle;
            this.dgvExternal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvExternal.EnableHeadersVisualStyles = false;
            this.dgvExternal.GridColor = System.Drawing.Color.FromArgb(228, 232, 240);
            this.dgvExternal.Name = "dgvExternal";
            this.dgvExternal.ReadOnly = true;
            this.dgvExternal.RowHeadersVisible = false;
            this.dgvExternal.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.dgvExternal.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvExternal.TabIndex = 1;
            this.dgvExternal.SelectionChanged += new System.EventHandler(this.DgvExternal_SelectionChanged);
            this.dgvExternal.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DgvExternal_DataBindingComplete);
            //
            // colExtId
            //
            this.colExtId.DataPropertyName = "Id";
            this.colExtId.Name = "colExtId";
            this.colExtId.Visible = false;
            //
            // colExtStatus
            //
            this.colExtStatus.DataPropertyName = "Status";
            this.colExtStatus.HeaderText = "สถานะ";
            this.colExtStatus.MinimumWidth = 80;
            this.colExtStatus.Name = "colExtStatus";
            this.colExtStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExtStatus.Width = 110;
            //
            // colExtResource
            //
            this.colExtResource.DataPropertyName = "Resource";
            this.colExtResource.HeaderText = "ไฟล์ / โฟลเดอร์";
            this.colExtResource.MinimumWidth = 100;
            this.colExtResource.Name = "colExtResource";
            this.colExtResource.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExtResource.Width = 240;
            //
            // colExtSharer
            //
            this.colExtSharer.DataPropertyName = "Sharer";
            this.colExtSharer.HeaderText = "ผู้แชร์ (เจ้าของ)";
            this.colExtSharer.MinimumWidth = 90;
            this.colExtSharer.Name = "colExtSharer";
            this.colExtSharer.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExtSharer.Width = 140;
            //
            // colExtCreated
            //
            this.colExtCreated.DataPropertyName = "Created";
            intDateStyle.Format = "g";
            this.colExtCreated.DefaultCellStyle = intDateStyle;
            this.colExtCreated.HeaderText = "วันที่แชร์";
            this.colExtCreated.MinimumWidth = 90;
            this.colExtCreated.Name = "colExtCreated";
            this.colExtCreated.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExtCreated.Width = 125;
            //
            // colExtExpiry
            //
            this.colExtExpiry.DataPropertyName = "Expiry";
            this.colExtExpiry.HeaderText = "หมดอายุ";
            this.colExtExpiry.MinimumWidth = 80;
            this.colExtExpiry.Name = "colExtExpiry";
            this.colExtExpiry.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExtExpiry.Width = 100;
            //
            // colExtLimit
            //
            this.colExtLimit.DataPropertyName = "Limit";
            this.colExtLimit.HeaderText = "จำกัดดาวน์โหลด";
            this.colExtLimit.MinimumWidth = 70;
            this.colExtLimit.Name = "colExtLimit";
            this.colExtLimit.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExtLimit.Width = 110;
            //
            // colExtDownloaded
            //
            this.colExtDownloaded.DataPropertyName = "Downloaded";
            this.colExtDownloaded.HeaderText = "ดาวน์โหลดแล้ว";
            this.colExtDownloaded.MinimumWidth = 70;
            this.colExtDownloaded.Name = "colExtDownloaded";
            this.colExtDownloaded.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExtDownloaded.Width = 110;
            //
            // colExtEnded
            //
            this.colExtEnded.DataPropertyName = "Ended";
            this.colExtEnded.HeaderText = "สิ้นสุดเมื่อ";
            this.colExtEnded.MinimumWidth = 80;
            this.colExtEnded.Name = "colExtEnded";
            this.colExtEnded.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExtEnded.Width = 125;
            //
            // colExtNote
            //
            this.colExtNote.DataPropertyName = "Note";
            this.colExtNote.HeaderText = "หมายเหตุ";
            this.colExtNote.MinimumWidth = 80;
            this.colExtNote.Name = "colExtNote";
            this.colExtNote.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExtNote.Width = 180;
            //
            // pnlExtFilter
            //
            this.pnlExtFilter.BackColor = System.Drawing.Color.FromArgb(235, 240, 250);
            this.pnlExtFilter.Controls.Add(this.rbExtActive);
            this.pnlExtFilter.Controls.Add(this.rbExtInactive);
            this.pnlExtFilter.Controls.Add(this.rbExtAll);
            this.pnlExtFilter.Controls.Add(this.lblExtSearch);
            this.pnlExtFilter.Controls.Add(this.txtExtSearch);
            this.pnlExtFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlExtFilter.Name = "pnlExtFilter";
            this.pnlExtFilter.Size = new System.Drawing.Size(972, 44);
            this.pnlExtFilter.TabIndex = 0;
            //
            // rbExtActive
            //
            this.rbExtActive.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbExtActive.Checked = true;
            this.rbExtActive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.rbExtActive.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 120, 60);
            this.rbExtActive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbExtActive.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rbExtActive.Location = new System.Drawing.Point(8, 7);
            this.rbExtActive.Name = "rbExtActive";
            this.rbExtActive.Size = new System.Drawing.Size(150, 30);
            this.rbExtActive.TabIndex = 0;
            this.rbExtActive.TabStop = true;
            this.rbExtActive.Text = "● ใช้งานอยู่";
            this.rbExtActive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbExtActive.CheckedChanged += new System.EventHandler(this.ExternalFilter_CheckedChanged);
            //
            // rbExtInactive
            //
            this.rbExtInactive.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbExtInactive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.rbExtInactive.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(110, 110, 120);
            this.rbExtInactive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbExtInactive.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rbExtInactive.Location = new System.Drawing.Point(162, 7);
            this.rbExtInactive.Name = "rbExtInactive";
            this.rbExtInactive.Size = new System.Drawing.Size(170, 30);
            this.rbExtInactive.TabIndex = 1;
            this.rbExtInactive.Text = "○ ไม่ได้ใช้งาน";
            this.rbExtInactive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbExtInactive.CheckedChanged += new System.EventHandler(this.ExternalFilter_CheckedChanged);
            //
            // rbExtAll
            //
            this.rbExtAll.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbExtAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.rbExtAll.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.rbExtAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbExtAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rbExtAll.Location = new System.Drawing.Point(336, 7);
            this.rbExtAll.Name = "rbExtAll";
            this.rbExtAll.Size = new System.Drawing.Size(110, 30);
            this.rbExtAll.TabIndex = 2;
            this.rbExtAll.Text = "ทั้งหมด";
            this.rbExtAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbExtAll.CheckedChanged += new System.EventHandler(this.ExternalFilter_CheckedChanged);
            //
            // lblExtSearch
            //
            this.lblExtSearch.AutoSize = true;
            this.lblExtSearch.Location = new System.Drawing.Point(466, 14);
            this.lblExtSearch.Name = "lblExtSearch";
            this.lblExtSearch.Text = "🔍 ค้นหา:";
            //
            // txtExtSearch
            //
            this.txtExtSearch.Location = new System.Drawing.Point(530, 11);
            this.txtExtSearch.Name = "txtExtSearch";
            this.txtExtSearch.Size = new System.Drawing.Size(260, 23);
            this.txtExtSearch.TabIndex = 3;
            this.txtExtSearch.TextChanged += new System.EventHandler(this.TxtExtSearch_TextChanged);
            //
            // pnlExtBar
            //
            this.pnlExtBar.BackColor = System.Drawing.Color.FromArgb(235, 240, 250);
            this.pnlExtBar.Controls.Add(this.btnToggleExt);
            this.pnlExtBar.Controls.Add(this.btnRevokeExt);
            this.pnlExtBar.Controls.Add(this.btnApproveExt);
            this.pnlExtBar.Controls.Add(this.btnRejectExt);
            this.pnlExtBar.Controls.Add(this.lblExtHint);
            this.pnlExtBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlExtBar.Name = "pnlExtBar";
            this.pnlExtBar.Size = new System.Drawing.Size(972, 46);
            this.pnlExtBar.TabIndex = 2;
            //
            // btnToggleExt
            //
            this.btnToggleExt.BackColor = System.Drawing.Color.FromArgb(255, 248, 230);
            this.btnToggleExt.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnToggleExt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleExt.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnToggleExt.ForeColor = System.Drawing.Color.FromArgb(140, 80, 0);
            this.btnToggleExt.Location = new System.Drawing.Point(8, 8);
            this.btnToggleExt.Name = "btnToggleExt";
            this.btnToggleExt.Size = new System.Drawing.Size(130, 30);
            this.btnToggleExt.TabIndex = 0;
            this.btnToggleExt.Text = "⏸  ปิดชั่วคราว";
            this.btnToggleExt.UseVisualStyleBackColor = false;
            this.btnToggleExt.Click += new System.EventHandler(this.BtnToggleExt_Click);
            //
            // btnRevokeExt
            //
            this.btnRevokeExt.BackColor = System.Drawing.Color.FromArgb(250, 235, 235);
            this.btnRevokeExt.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnRevokeExt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRevokeExt.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRevokeExt.ForeColor = System.Drawing.Color.FromArgb(160, 30, 30);
            this.btnRevokeExt.Location = new System.Drawing.Point(144, 8);
            this.btnRevokeExt.Name = "btnRevokeExt";
            this.btnRevokeExt.Size = new System.Drawing.Size(150, 30);
            this.btnRevokeExt.TabIndex = 1;
            this.btnRevokeExt.Text = "🚫  ยกเลิกการแชร์";
            this.btnRevokeExt.UseVisualStyleBackColor = false;
            this.btnRevokeExt.Click += new System.EventHandler(this.BtnRevokeExt_Click);
            //
            // btnApproveExt
            //
            this.btnApproveExt.BackColor = System.Drawing.Color.FromArgb(25, 55, 105);
            this.btnApproveExt.FlatAppearance.BorderSize = 0;
            this.btnApproveExt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApproveExt.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnApproveExt.ForeColor = System.Drawing.Color.White;
            this.btnApproveExt.Location = new System.Drawing.Point(300, 8);
            this.btnApproveExt.Name = "btnApproveExt";
            this.btnApproveExt.Size = new System.Drawing.Size(96, 30);
            this.btnApproveExt.TabIndex = 2;
            this.btnApproveExt.Text = "✔  อนุมัติ";
            this.btnApproveExt.UseVisualStyleBackColor = false;
            this.btnApproveExt.Click += new System.EventHandler(this.BtnApproveExt_Click);
            //
            // btnRejectExt
            //
            this.btnRejectExt.BackColor = System.Drawing.Color.FromArgb(250, 235, 235);
            this.btnRejectExt.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnRejectExt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRejectExt.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRejectExt.ForeColor = System.Drawing.Color.FromArgb(160, 30, 30);
            this.btnRejectExt.Location = new System.Drawing.Point(402, 8);
            this.btnRejectExt.Name = "btnRejectExt";
            this.btnRejectExt.Size = new System.Drawing.Size(96, 30);
            this.btnRejectExt.TabIndex = 3;
            this.btnRejectExt.Text = "✕  ปฏิเสธ";
            this.btnRejectExt.UseVisualStyleBackColor = false;
            this.btnRejectExt.Click += new System.EventHandler(this.BtnRejectExt_Click);
            //
            // lblExtHint
            //
            this.lblExtHint.AutoSize = true;
            this.lblExtHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblExtHint.ForeColor = System.Drawing.Color.FromArgb(70, 90, 130);
            this.lblExtHint.Location = new System.Drawing.Point(508, 15);
            this.lblExtHint.Name = "lblExtHint";
            this.lblExtHint.Text = "";
            //
            // ShareManagementForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1000, 640);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(860, 520);
            this.Name = "ShareManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "จัดการการแบ่งปัน";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabInternal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvInternal)).EndInit();
            this.pnlIntFilter.ResumeLayout(false);
            this.pnlIntFilter.PerformLayout();
            this.pnlIntBar.ResumeLayout(false);
            this.pnlIntBar.PerformLayout();
            this.tabExternal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvExternal)).EndInit();
            this.pnlExtFilter.ResumeLayout(false);
            this.pnlExtFilter.PerformLayout();
            this.pnlExtBar.ResumeLayout(false);
            this.pnlExtBar.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabInternal;
        private System.Windows.Forms.DataGridView dgvInternal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntSharer;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntTarget;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntResource;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntRights;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntCreated;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntExpiry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntEnded;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntNote;
        private System.Windows.Forms.Panel pnlIntFilter;
        private System.Windows.Forms.RadioButton rbIntActive;
        private System.Windows.Forms.RadioButton rbIntInactive;
        private System.Windows.Forms.RadioButton rbIntAll;
        private System.Windows.Forms.Label lblIntSearch;
        private System.Windows.Forms.TextBox txtIntSearch;
        private System.Windows.Forms.Panel pnlIntBar;
        private System.Windows.Forms.Button btnViewDetailInternal;
        private System.Windows.Forms.Button btnRevokeInternal;
        private System.Windows.Forms.Button btnWithdrawInternal;
        private System.Windows.Forms.Button btnEditInternal;
        private System.Windows.Forms.Label lblIntHint;
        private System.Windows.Forms.TabPage tabExternal;
        private System.Windows.Forms.DataGridView dgvExternal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtSharer;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtResource;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtCreated;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtExpiry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtLimit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtDownloaded;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtEnded;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExtNote;
        private System.Windows.Forms.Panel pnlExtFilter;
        private System.Windows.Forms.RadioButton rbExtActive;
        private System.Windows.Forms.RadioButton rbExtInactive;
        private System.Windows.Forms.RadioButton rbExtAll;
        private System.Windows.Forms.Label lblExtSearch;
        private System.Windows.Forms.TextBox txtExtSearch;
        private System.Windows.Forms.Panel pnlExtBar;
        private System.Windows.Forms.Button btnToggleExt;
        private System.Windows.Forms.Button btnRevokeExt;
        private System.Windows.Forms.Button btnApproveExt;
        private System.Windows.Forms.Button btnRejectExt;
        private System.Windows.Forms.Label lblExtHint;
    }
}
