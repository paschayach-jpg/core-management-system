namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowDelegationForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlRibbonTabs;
        private System.Windows.Forms.Panel pnlRibbonBorder;
        private System.Windows.Forms.Panel pnlToolbarContainer;
        private System.Windows.Forms.Panel pnlAppBody;
        private System.Windows.Forms.Panel pnlSidebarContainer;
        private System.Windows.Forms.Panel pnlNavBorder;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlBreadcrumb;
        private System.Windows.Forms.Label lblBreadcrumb;
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Button btnCreateDelegation;
        private System.Windows.Forms.Panel pnlDashboard;
        private System.Windows.Forms.Panel pnlDelegationCard;
        private System.Windows.Forms.Label lblCardHeader;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.TextBox txtFromUser;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.ComboBox cmbDelegateTo;
        private System.Windows.Forms.Label lblScope;
        private System.Windows.Forms.ComboBox cmbScope;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.Label lblEnd;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.Label lblOptions;
        private System.Windows.Forms.CheckBox chkEmail;
        private System.Windows.Forms.CheckBox chkOutOfOffice;
        private System.Windows.Forms.CheckBox chkRequireReason;
        private System.Windows.Forms.Label lblReason;
        private System.Windows.Forms.TextBox txtReason;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel pnlHistoryHeader;
        private System.Windows.Forms.Label lblHistory;
        private System.Windows.Forms.DataGridView dgvDelegations;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFromTo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colScope;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNote;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlRibbonTabs = new System.Windows.Forms.Panel();
            this.pnlRibbonBorder = new System.Windows.Forms.Panel();
            this.pnlToolbarContainer = new System.Windows.Forms.Panel();
            this.pnlAppBody = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlDashboard = new System.Windows.Forms.Panel();
            this.dgvDelegations = new System.Windows.Forms.DataGridView();
            this.colFromTo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colScope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNote = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlHistoryHeader = new System.Windows.Forms.Panel();
            this.lblHistory = new System.Windows.Forms.Label();
            this.pnlDelegationCard = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtReason = new System.Windows.Forms.TextBox();
            this.lblReason = new System.Windows.Forms.Label();
            this.chkRequireReason = new System.Windows.Forms.CheckBox();
            this.chkOutOfOffice = new System.Windows.Forms.CheckBox();
            this.chkEmail = new System.Windows.Forms.CheckBox();
            this.lblOptions = new System.Windows.Forms.Label();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.lblEnd = new System.Windows.Forms.Label();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.lblStart = new System.Windows.Forms.Label();
            this.cmbScope = new System.Windows.Forms.ComboBox();
            this.lblScope = new System.Windows.Forms.Label();
            this.cmbDelegateTo = new System.Windows.Forms.ComboBox();
            this.lblTo = new System.Windows.Forms.Label();
            this.txtFromUser = new System.Windows.Forms.TextBox();
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblCardHeader = new System.Windows.Forms.Label();
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.btnCreateDelegation = new System.Windows.Forms.Button();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlBreadcrumb = new System.Windows.Forms.Panel();
            this.lblBreadcrumb = new System.Windows.Forms.Label();
            this.pnlNavBorder = new System.Windows.Forms.Panel();
            this.pnlSidebarContainer = new System.Windows.Forms.Panel();
            this.pnlAppBody.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlDashboard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDelegations)).BeginInit();
            this.pnlHistoryHeader.SuspendLayout();
            this.pnlDelegationCard.SuspendLayout();
            this.pnlTitleBar.SuspendLayout();
            this.pnlBreadcrumb.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlRibbonTabs
            // 
            this.pnlRibbonTabs.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlRibbonTabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonTabs.Location = new System.Drawing.Point(0, 0);
            this.pnlRibbonTabs.Name = "pnlRibbonTabs";
            this.pnlRibbonTabs.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.pnlRibbonTabs.Size = new System.Drawing.Size(1280, 32);
            this.pnlRibbonTabs.TabIndex = 0;
            // 
            // pnlRibbonBorder
            // 
            this.pnlRibbonBorder.BackColor = System.Drawing.Color.FromArgb(200, 215, 240);
            this.pnlRibbonBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRibbonBorder.Location = new System.Drawing.Point(0, 32);
            this.pnlRibbonBorder.Name = "pnlRibbonBorder";
            this.pnlRibbonBorder.Size = new System.Drawing.Size(1280, 1);
            this.pnlRibbonBorder.TabIndex = 1;
            // 
            // pnlToolbarContainer
            // 
            this.pnlToolbarContainer.BackColor = System.Drawing.Color.White;
            this.pnlToolbarContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbarContainer.Location = new System.Drawing.Point(0, 33);
            this.pnlToolbarContainer.Name = "pnlToolbarContainer";
            this.pnlToolbarContainer.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.pnlToolbarContainer.Size = new System.Drawing.Size(1280, 82);
            this.pnlToolbarContainer.TabIndex = 2;
            // 
            // pnlAppBody
            // 
            this.pnlAppBody.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlAppBody.Controls.Add(this.pnlContent);
            this.pnlAppBody.Controls.Add(this.pnlNavBorder);
            this.pnlAppBody.Controls.Add(this.pnlSidebarContainer);
            this.pnlAppBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAppBody.Location = new System.Drawing.Point(0, 115);
            this.pnlAppBody.Name = "pnlAppBody";
            this.pnlAppBody.Size = new System.Drawing.Size(1280, 685);
            this.pnlAppBody.TabIndex = 3;
            // 
            // pnlContent
            // 
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlContent.Controls.Add(this.pnlDashboard);
            this.pnlContent.Controls.Add(this.pnlTitleBar);
            this.pnlContent.Controls.Add(this.pnlBreadcrumb);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(203, 0);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(1077, 685);
            this.pnlContent.TabIndex = 2;
            // 
            // pnlDashboard
            // 
            this.pnlDashboard.BackColor = System.Drawing.Color.White;
            this.pnlDashboard.Controls.Add(this.dgvDelegations);
            this.pnlDashboard.Controls.Add(this.pnlHistoryHeader);
            this.pnlDashboard.Controls.Add(this.pnlDelegationCard);
            this.pnlDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.Location = new System.Drawing.Point(0, 90);
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
            this.pnlDashboard.Size = new System.Drawing.Size(1077, 595);
            this.pnlDashboard.TabIndex = 2;
            // 
            // dgvDelegations
            // 
            this.dgvDelegations.AllowUserToAddRows = false;
            this.dgvDelegations.AllowUserToDeleteRows = false;
            this.dgvDelegations.AllowUserToResizeRows = false;
            this.dgvDelegations.BackgroundColor = System.Drawing.Color.White;
            this.dgvDelegations.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDelegations.ColumnHeadersHeight = 32;
            this.dgvDelegations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvDelegations.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFromTo,
            this.colScope,
            this.colStartDate,
            this.colEndDate,
            this.colStatus,
            this.colNote});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(232, 241, 255);
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDelegations.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDelegations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDelegations.GridColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.dgvDelegations.Location = new System.Drawing.Point(16, 374);
            this.dgvDelegations.MultiSelect = false;
            this.dgvDelegations.Name = "dgvDelegations";
            this.dgvDelegations.ReadOnly = true;
            this.dgvDelegations.RowHeadersVisible = false;
            this.dgvDelegations.RowTemplate.Height = 34;
            this.dgvDelegations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDelegations.Size = new System.Drawing.Size(1045, 213);
            this.dgvDelegations.TabIndex = 2;
            this.dgvDelegations.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvDelegations_CellFormatting);
            // 
            // colFromTo
            // 
            this.colFromTo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colFromTo.HeaderText = "จาก → ถึง";
            this.colFromTo.MinimumWidth = 220;
            this.colFromTo.Name = "colFromTo";
            this.colFromTo.ReadOnly = true;
            // 
            // colScope
            // 
            this.colScope.HeaderText = "ขอบเขต";
            this.colScope.Name = "colScope";
            this.colScope.ReadOnly = true;
            this.colScope.Width = 190;
            // 
            // colStartDate
            // 
            this.colStartDate.HeaderText = "เริ่ม";
            this.colStartDate.Name = "colStartDate";
            this.colStartDate.ReadOnly = true;
            this.colStartDate.Width = 95;
            // 
            // colEndDate
            // 
            this.colEndDate.HeaderText = "สิ้นสุด";
            this.colEndDate.Name = "colEndDate";
            this.colEndDate.ReadOnly = true;
            this.colEndDate.Width = 95;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "สถานะ";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            this.colStatus.Width = 110;
            // 
            // colNote
            // 
            this.colNote.HeaderText = "หมายเหตุ";
            this.colNote.Name = "colNote";
            this.colNote.ReadOnly = true;
            this.colNote.Width = 210;
            // 
            // pnlHistoryHeader
            // 
            this.pnlHistoryHeader.Controls.Add(this.lblHistory);
            this.pnlHistoryHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHistoryHeader.Location = new System.Drawing.Point(16, 340);
            this.pnlHistoryHeader.Name = "pnlHistoryHeader";
            this.pnlHistoryHeader.Size = new System.Drawing.Size(1045, 34);
            this.pnlHistoryHeader.TabIndex = 1;
            // 
            // lblHistory
            // 
            this.lblHistory.AutoSize = true;
            this.lblHistory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHistory.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblHistory.Location = new System.Drawing.Point(0, 10);
            this.lblHistory.Name = "lblHistory";
            this.lblHistory.Size = new System.Drawing.Size(129, 15);
            this.lblHistory.TabIndex = 0;
            this.lblHistory.Text = "คิวที่มีอยู่แล้ว (ของคุณ)";
            // 
            // pnlDelegationCard
            // 
            this.pnlDelegationCard.BackColor = System.Drawing.Color.White;
            this.pnlDelegationCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDelegationCard.Controls.Add(this.btnCancel);
            this.pnlDelegationCard.Controls.Add(this.btnSave);
            this.pnlDelegationCard.Controls.Add(this.txtReason);
            this.pnlDelegationCard.Controls.Add(this.lblReason);
            this.pnlDelegationCard.Controls.Add(this.chkRequireReason);
            this.pnlDelegationCard.Controls.Add(this.chkOutOfOffice);
            this.pnlDelegationCard.Controls.Add(this.chkEmail);
            this.pnlDelegationCard.Controls.Add(this.lblOptions);
            this.pnlDelegationCard.Controls.Add(this.dtpEnd);
            this.pnlDelegationCard.Controls.Add(this.lblEnd);
            this.pnlDelegationCard.Controls.Add(this.dtpStart);
            this.pnlDelegationCard.Controls.Add(this.lblStart);
            this.pnlDelegationCard.Controls.Add(this.cmbScope);
            this.pnlDelegationCard.Controls.Add(this.lblScope);
            this.pnlDelegationCard.Controls.Add(this.cmbDelegateTo);
            this.pnlDelegationCard.Controls.Add(this.lblTo);
            this.pnlDelegationCard.Controls.Add(this.txtFromUser);
            this.pnlDelegationCard.Controls.Add(this.lblFrom);
            this.pnlDelegationCard.Controls.Add(this.lblCardHeader);
            this.pnlDelegationCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDelegationCard.Location = new System.Drawing.Point(16, 8);
            this.pnlDelegationCard.Name = "pnlDelegationCard";
            this.pnlDelegationCard.Size = new System.Drawing.Size(1045, 332);
            this.pnlDelegationCard.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.btnCancel.Location = new System.Drawing.Point(840, 289);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(33, 102, 172);
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(926, 289);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(106, 30);
            this.btnSave.TabIndex = 18;
            this.btnSave.Text = "บันทึก Delegation";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtReason
            // 
            this.txtReason.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtReason.Location = new System.Drawing.Point(16, 216);
            this.txtReason.Multiline = true;
            this.txtReason.Name = "txtReason";
            this.txtReason.Size = new System.Drawing.Size(1016, 62);
            this.txtReason.TabIndex = 16;
            // 
            // lblReason
            // 
            this.lblReason.AutoSize = true;
            this.lblReason.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblReason.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblReason.Location = new System.Drawing.Point(13, 198);
            this.lblReason.Name = "lblReason";
            this.lblReason.Size = new System.Drawing.Size(108, 15);
            this.lblReason.TabIndex = 15;
            this.lblReason.Text = "เหตุผล / ข้อความแทน";
            // 
            // chkRequireReason
            // 
            this.chkRequireReason.AutoSize = true;
            this.chkRequireReason.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkRequireReason.Location = new System.Drawing.Point(705, 172);
            this.chkRequireReason.Name = "chkRequireReason";
            this.chkRequireReason.Size = new System.Drawing.Size(213, 19);
            this.chkRequireReason.TabIndex = 14;
            this.chkRequireReason.Text = "Require ยืนยันจากผู้รับมอบหมาย";
            this.chkRequireReason.UseVisualStyleBackColor = true;
            // 
            // chkOutOfOffice
            // 
            this.chkOutOfOffice.AutoSize = true;
            this.chkOutOfOffice.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkOutOfOffice.Location = new System.Drawing.Point(705, 149);
            this.chkOutOfOffice.Name = "chkOutOfOffice";
            this.chkOutOfOffice.Size = new System.Drawing.Size(176, 19);
            this.chkOutOfOffice.TabIndex = 13;
            this.chkOutOfOffice.Text = "ส่ง Out-of-Office reply อัตโนมัติ";
            this.chkOutOfOffice.UseVisualStyleBackColor = true;
            // 
            // chkEmail
            // 
            this.chkEmail.AutoSize = true;
            this.chkEmail.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkEmail.Location = new System.Drawing.Point(705, 126);
            this.chkEmail.Name = "chkEmail";
            this.chkEmail.Size = new System.Drawing.Size(166, 19);
            this.chkEmail.TabIndex = 12;
            this.chkEmail.Text = "แจ้ง Email ผู้รับมอบหมาย";
            this.chkEmail.UseVisualStyleBackColor = true;
            // 
            // lblOptions
            // 
            this.lblOptions.AutoSize = true;
            this.lblOptions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblOptions.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblOptions.Location = new System.Drawing.Point(702, 105);
            this.lblOptions.Name = "lblOptions";
            this.lblOptions.Size = new System.Drawing.Size(54, 15);
            this.lblOptions.TabIndex = 11;
            this.lblOptions.Text = "ตัวเลือก";
            // 
            // dtpEnd
            // 
            this.dtpEnd.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpEnd.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEnd.Location = new System.Drawing.Point(358, 166);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(330, 23);
            this.dtpEnd.TabIndex = 10;
            // 
            // lblEnd
            // 
            this.lblEnd.AutoSize = true;
            this.lblEnd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblEnd.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblEnd.Location = new System.Drawing.Point(355, 148);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(43, 15);
            this.lblEnd.TabIndex = 9;
            this.lblEnd.Text = "สิ้นสุด";
            // 
            // dtpStart
            // 
            this.dtpStart.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpStart.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStart.Location = new System.Drawing.Point(16, 166);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(330, 23);
            this.dtpStart.TabIndex = 8;
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStart.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblStart.Location = new System.Drawing.Point(13, 148);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(29, 15);
            this.lblStart.TabIndex = 7;
            this.lblStart.Text = "เริ่ม";
            // 
            // cmbScope
            // 
            this.cmbScope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScope.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbScope.FormattingEnabled = true;
            this.cmbScope.Location = new System.Drawing.Point(705, 78);
            this.cmbScope.Name = "cmbScope";
            this.cmbScope.Size = new System.Drawing.Size(327, 23);
            this.cmbScope.TabIndex = 6;
            // 
            // lblScope
            // 
            this.lblScope.AutoSize = true;
            this.lblScope.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblScope.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblScope.Location = new System.Drawing.Point(702, 60);
            this.lblScope.Name = "lblScope";
            this.lblScope.Size = new System.Drawing.Size(52, 15);
            this.lblScope.TabIndex = 5;
            this.lblScope.Text = "ขอบเขต";
            // 
            // cmbDelegateTo
            // 
            this.cmbDelegateTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbDelegateTo.SelectionChangeCommitted += new System.EventHandler(this.cmbDelegateTo_SelectionChangeCommitted);
            this.cmbDelegateTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbDelegateTo.FormattingEnabled = true;
            this.cmbDelegateTo.Location = new System.Drawing.Point(358, 78);
            this.cmbDelegateTo.Name = "cmbDelegateTo";
            this.cmbDelegateTo.Size = new System.Drawing.Size(330, 23);
            this.cmbDelegateTo.TabIndex = 4;
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTo.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblTo.Location = new System.Drawing.Point(355, 60);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(104, 15);
            this.lblTo.TabIndex = 3;
            this.lblTo.Text = "ถึง (ผู้รับมอบหมาย)";
            // 
            // txtFromUser
            // 
            this.txtFromUser.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.txtFromUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFromUser.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFromUser.Location = new System.Drawing.Point(16, 78);
            this.txtFromUser.Name = "txtFromUser";
            this.txtFromUser.ReadOnly = true;
            this.txtFromUser.Size = new System.Drawing.Size(330, 23);
            this.txtFromUser.TabIndex = 2;
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFrom.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblFrom.Location = new System.Drawing.Point(13, 60);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(109, 15);
            this.lblFrom.TabIndex = 1;
            this.lblFrom.Text = "จาก (ผู้มอบหมาย)";
            // 
            // lblCardHeader
            // 
            this.lblCardHeader.AutoSize = true;
            this.lblCardHeader.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblCardHeader.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblCardHeader.Location = new System.Drawing.Point(12, 14);
            this.lblCardHeader.Name = "lblCardHeader";
            this.lblCardHeader.Size = new System.Drawing.Size(136, 19);
            this.lblCardHeader.TabIndex = 0;
            this.lblCardHeader.Text = "+ สร้าง Delegation ใหม่";
            // 
            // pnlTitleBar
            // 
            this.pnlTitleBar.BackColor = System.Drawing.Color.White;
            this.pnlTitleBar.Controls.Add(this.btnCreateDelegation);
            this.pnlTitleBar.Controls.Add(this.lblSubtitle);
            this.pnlTitleBar.Controls.Add(this.lblTitle);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Location = new System.Drawing.Point(0, 28);
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Padding = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlTitleBar.Size = new System.Drawing.Size(1077, 62);
            this.pnlTitleBar.TabIndex = 1;
            // 
            // btnCreateDelegation
            // 
            this.btnCreateDelegation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateDelegation.BackColor = System.Drawing.Color.FromArgb(33, 102, 172);
            this.btnCreateDelegation.FlatAppearance.BorderSize = 0;
            this.btnCreateDelegation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreateDelegation.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCreateDelegation.ForeColor = System.Drawing.Color.White;
            this.btnCreateDelegation.Location = new System.Drawing.Point(932, 16);
            this.btnCreateDelegation.Name = "btnCreateDelegation";
            this.btnCreateDelegation.Size = new System.Drawing.Size(133, 30);
            this.btnCreateDelegation.TabIndex = 2;
            this.btnCreateDelegation.Text = "+ สร้าง Delegation";
            this.btnCreateDelegation.UseVisualStyleBackColor = false;
            this.btnCreateDelegation.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblSubtitle.Location = new System.Drawing.Point(20, 38);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(366, 15);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "ตั้งค่าการส่งต่อ Workflow ในช่วงเวลาที่กำหนด เช่น ลาพักร้อน/ลาป่วย";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitle.Location = new System.Drawing.Point(20, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(208, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "การมอบหมายงาน (Delegation)";
            // 
            // pnlBreadcrumb
            // 
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);
            this.pnlBreadcrumb.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Location = new System.Drawing.Point(0, 0);
            this.pnlBreadcrumb.Name = "pnlBreadcrumb";
            this.pnlBreadcrumb.Padding = new System.Windows.Forms.Padding(20, 0, 16, 0);
            this.pnlBreadcrumb.Size = new System.Drawing.Size(1077, 28);
            this.pnlBreadcrumb.TabIndex = 0;
            // 
            // lblBreadcrumb
            // 
            this.lblBreadcrumb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBreadcrumb.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblBreadcrumb.Location = new System.Drawing.Point(20, 0);
            this.lblBreadcrumb.Name = "lblBreadcrumb";
            this.lblBreadcrumb.Size = new System.Drawing.Size(1041, 28);
            this.lblBreadcrumb.TabIndex = 0;
            this.lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  Delegation";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlNavBorder
            // 
            this.pnlNavBorder.BackColor = System.Drawing.Color.FromArgb(225, 228, 235);
            this.pnlNavBorder.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlNavBorder.Location = new System.Drawing.Point(202, 0);
            this.pnlNavBorder.Name = "pnlNavBorder";
            this.pnlNavBorder.Size = new System.Drawing.Size(1, 685);
            this.pnlNavBorder.TabIndex = 1;
            // 
            // pnlSidebarContainer
            // 
            this.pnlSidebarContainer.AutoScroll = true;
            this.pnlSidebarContainer.BackColor = System.Drawing.Color.FromArgb(26, 35, 67);
            this.pnlSidebarContainer.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebarContainer.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebarContainer.Name = "pnlSidebarContainer";
            this.pnlSidebarContainer.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.pnlSidebarContainer.Size = new System.Drawing.Size(202, 685);
            this.pnlSidebarContainer.TabIndex = 0;
            // 
            // WorkflowDelegationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.pnlAppBody);
            this.Controls.Add(this.pnlToolbarContainer);
            this.Controls.Add(this.pnlRibbonBorder);
            this.Controls.Add(this.pnlRibbonTabs);
            this.MinimumSize = new System.Drawing.Size(1000, 650);
            this.Name = "WorkflowDelegationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Delegation Settings";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.WorkflowDelegationForm_Load);
            this.pnlAppBody.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlDashboard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDelegations)).EndInit();
            this.pnlHistoryHeader.ResumeLayout(false);
            this.pnlHistoryHeader.PerformLayout();
            this.pnlDelegationCard.ResumeLayout(false);
            this.pnlDelegationCard.PerformLayout();
            this.pnlTitleBar.ResumeLayout(false);
            this.pnlTitleBar.PerformLayout();
            this.pnlBreadcrumb.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
