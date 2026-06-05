namespace CMS.Presentation
{
    partial class FolderQuotaConfigForm
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
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.lblTreeHeader = new System.Windows.Forms.Label();
            this.lblMasterType = new System.Windows.Forms.Label();
            this.cmbMasterType = new System.Windows.Forms.ComboBox();
            this.txtMasterSearch = new System.Windows.Forms.TextBox();
            this.lstOrgUnits = new System.Windows.Forms.ListBox();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pnlEditHost = new System.Windows.Forms.Panel();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.lblSelectedOrg = new System.Windows.Forms.Label();
            this.lblQuota = new System.Windows.Forms.Label();
            this.nudQuotaGB = new System.Windows.Forms.NumericUpDown();
            this.lblGB = new System.Windows.Forms.Label();
            this.lblWarn = new System.Windows.Forms.Label();
            this.nudWarnPct = new System.Windows.Forms.NumericUpDown();
            this.lblWarnPct = new System.Windows.Forms.Label();
            this.lblCritical = new System.Windows.Forms.Label();
            this.nudCriticalPct = new System.Windows.Forms.NumericUpDown();
            this.lblCritPct = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.dgvConfigs = new System.Windows.Forms.DataGridView();
            this.colCfgID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgOrg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgQuota = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgWarn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgCrit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgFileCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgUsedGB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgUsedPct = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.nudQuotaGB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWarnPct)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCriticalPct)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvConfigs)).BeginInit();
            this.SuspendLayout();

            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 64;
            this.pnlHeader.Controls.Add(this.lblSubTitle);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Text = "กำหนด Quota พื้นที่ Folder";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(14, 8);
            this.lblTitle.Size = new System.Drawing.Size(520, 30);

            this.lblSubTitle.Text = "กำหนดขนาดพื้นที่จัดเก็บเอกสารสำหรับแต่ละหน่วยงาน  ▶  Sub Committee / Division / Department";
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255);
            this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubTitle.Location = new System.Drawing.Point(16, 40);
            this.lblSubTitle.Size = new System.Drawing.Size(900, 20);

            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width = 300;
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);

            this.lblTreeHeader.Text = "  เลือกประเภท Master";
            this.lblTreeHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTreeHeader.Height = 32;
            this.lblTreeHeader.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.lblTreeHeader.ForeColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.lblTreeHeader.BackColor = System.Drawing.Color.FromArgb(220, 232, 248);
            this.lblTreeHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblMasterType.Text = "ประเภท:";
            this.lblMasterType.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMasterType.Height = 28;
            this.lblMasterType.Padding = new System.Windows.Forms.Padding(10, 7, 10, 0);
            this.lblMasterType.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblMasterType.ForeColor = System.Drawing.Color.FromArgb(26, 58, 108);

            this.cmbMasterType.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbMasterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMasterType.Font = new System.Drawing.Font("Segoe UI", 10);
            this.cmbMasterType.Height = 30;

            this.txtMasterSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtMasterSearch.Font = new System.Drawing.Font("Segoe UI", 10);
            this.txtMasterSearch.Height = 30;
            this.txtMasterSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMasterSearch.Text = "ค้นหาหน่วยงาน...";
            this.txtMasterSearch.ForeColor = System.Drawing.Color.Silver;

            this.lstOrgUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstOrgUnits.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstOrgUnits.Font = new System.Drawing.Font("Segoe UI", 10);
            this.lstOrgUnits.ItemHeight = 28;
            this.lstOrgUnits.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.lstOrgUnits.IntegralHeight = false;
            this.lstOrgUnits.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;

            this.pnlLeft.Controls.Add(this.lstOrgUnits);
            this.pnlLeft.Controls.Add(this.txtMasterSearch);
            this.pnlLeft.Controls.Add(this.cmbMasterType);
            this.pnlLeft.Controls.Add(this.lblMasterType);
            this.pnlLeft.Controls.Add(this.lblTreeHeader);

            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;

            this.pnlEditHost.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEditHost.Height = 220;
            this.pnlEditHost.BackColor = System.Drawing.Color.White;
            this.pnlEditHost.Padding = new System.Windows.Forms.Padding(14, 10, 0, 12);

            this.pnlEdit.Location = new System.Drawing.Point(14, 10);
            this.pnlEdit.Size = new System.Drawing.Size(660, 196);
            this.pnlEdit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.pnlEdit.BackColor = System.Drawing.Color.White;
            this.pnlEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            this.lblSelectedOrg.Text = "เลือกประเภท Master และเลือกรายการด้านซ้าย";
            this.lblSelectedOrg.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Italic);
            this.lblSelectedOrg.ForeColor = System.Drawing.Color.Gray;
            this.lblSelectedOrg.Location = new System.Drawing.Point(16, 14);
            this.lblSelectedOrg.Size = new System.Drawing.Size(600, 22);

            this.lblQuota.Text = "Quota (GB):";
            this.lblQuota.Location = new System.Drawing.Point(16, 60);
            this.lblQuota.Size = new System.Drawing.Size(116, 24);
            this.lblQuota.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudQuotaGB.Location = new System.Drawing.Point(136, 59);
            this.nudQuotaGB.Size = new System.Drawing.Size(92, 28);
            this.nudQuotaGB.Minimum = 0;
            this.nudQuotaGB.Maximum = 10000;
            this.nudQuotaGB.Value = 40;
            this.nudQuotaGB.DecimalPlaces = 1;

            this.lblGB.Text = "GB";
            this.lblGB.Location = new System.Drawing.Point(234, 60);
            this.lblGB.Size = new System.Drawing.Size(32, 24);
            this.lblGB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblWarn.Text = "บันทึกลง: -";
            this.lblWarn.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblWarn.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblWarn.Location = new System.Drawing.Point(16, 104);
            this.lblWarn.Size = new System.Drawing.Size(600, 24);
            this.lblWarn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudWarnPct.Visible = false;
            this.nudWarnPct.Minimum = 50;
            this.nudWarnPct.Maximum = 99;
            this.nudWarnPct.Value = 80;
            this.lblWarnPct.Visible = false;
            this.lblCritical.Visible = false;
            this.nudCriticalPct.Visible = false;
            this.nudCriticalPct.Minimum = 51;
            this.nudCriticalPct.Maximum = 100;
            this.nudCriticalPct.Value = 95;
            this.lblCritPct.Visible = false;

            this.btnSave.Text = "บันทึก";
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnSave.Size = new System.Drawing.Size(104, 36);
            this.btnSave.Location = new System.Drawing.Point(16, 152);
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(52, 120, 47);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnDelete.Text = "ล้างค่า Quota";
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnDelete.Size = new System.Drawing.Size(128, 36);
            this.btnDelete.Location = new System.Drawing.Point(130, 152);
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(180, 30, 30);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnClear.Text = "ยกเลิก";
            this.btnClear.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnClear.Size = new System.Drawing.Size(104, 36);
            this.btnClear.Location = new System.Drawing.Point(268, 152);
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(108, 108, 108);
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlEdit.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblSelectedOrg,
                this.lblQuota, this.nudQuotaGB, this.lblGB,
                this.lblWarn, this.nudWarnPct, this.lblWarnPct,
                this.lblCritical, this.nudCriticalPct, this.lblCritPct,
                this.btnSave, this.btnDelete, this.btnClear
            });
            this.pnlEditHost.Controls.Add(this.pnlEdit);

            this.dgvConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvConfigs.AllowUserToAddRows = false;
            this.dgvConfigs.ReadOnly = true;
            this.dgvConfigs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvConfigs.MultiSelect = false;
            this.dgvConfigs.RowHeadersVisible = false;
            this.dgvConfigs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvConfigs.BackgroundColor = System.Drawing.Color.White;
            this.dgvConfigs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvConfigs.Font = new System.Drawing.Font("Segoe UI", 10);
            this.dgvConfigs.ColumnHeadersHeight = 34;
            this.dgvConfigs.RowTemplate.Height = 32;
            this.dgvConfigs.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.dgvConfigs.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.dgvConfigs.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);

            this.colCfgID.HeaderText = "ID";
            this.colCfgID.Visible = false;
            this.colCfgType.HeaderText = "ประเภท";
            this.colCfgType.MinimumWidth = 100;
            this.colCfgType.FillWeight = 100;
            this.colCfgOrg.HeaderText = "หน่วยงาน";
            this.colCfgOrg.MinimumWidth = 260;
            this.colCfgOrg.FillWeight = 260;
            this.colCfgQuota.HeaderText = "ขนาด Quota";
            this.colCfgQuota.MinimumWidth = 110;
            this.colCfgQuota.FillWeight = 110;
            this.colCfgWarn.HeaderText = "ตาราง";
            this.colCfgWarn.MinimumWidth = 140;
            this.colCfgWarn.FillWeight = 140;
            this.colCfgCrit.Visible = false;
            this.colCfgDate.HeaderText = "แก้ไขล่าสุด";
            this.colCfgDate.MinimumWidth = 100;
            this.colCfgDate.FillWeight = 100;

            this.colCfgFileCount.HeaderText = "จำนวนไฟล์";
            this.colCfgFileCount.MinimumWidth = 90;
            this.colCfgFileCount.FillWeight = 90;
            this.colCfgFileCount.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;

            this.colCfgUsedGB.HeaderText = "ใช้ไปแล้ว";
            this.colCfgUsedGB.MinimumWidth = 90;
            this.colCfgUsedGB.FillWeight = 90;
            this.colCfgUsedGB.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;

            this.colCfgUsedPct.HeaderText = "% ใช้งาน";
            this.colCfgUsedPct.MinimumWidth = 80;
            this.colCfgUsedPct.FillWeight = 80;
            this.colCfgUsedPct.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;

            this.dgvConfigs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colCfgID, this.colCfgType, this.colCfgOrg,
                this.colCfgQuota, this.colCfgWarn, this.colCfgCrit, this.colCfgDate,
                this.colCfgFileCount, this.colCfgUsedGB, this.colCfgUsedPct
            });

            this.pnlRight.Controls.Add(this.dgvConfigs);
            this.pnlRight.Controls.Add(this.pnlEditHost);

            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height = 50;
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(14, 9, 14, 9);

            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Text = "";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.AutoEllipsis = true;

            this.btnClose.Text = "ปิด";
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnClose.Size = new System.Drawing.Size(90, 32);
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlFooter.Controls.Add(this.lblStatus);
            this.pnlFooter.Controls.Add(this.btnClose);

            this.Text = "กำหนด Quota พื้นที่ Folder";
            this.Size = new System.Drawing.Size(1120, 700);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(980, 620);

            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);

            this.cmbMasterType.SelectedIndexChanged += new System.EventHandler(this.cmbMasterType_SelectedIndexChanged);
            this.txtMasterSearch.TextChanged += new System.EventHandler(this.txtMasterSearch_TextChanged);
            this.txtMasterSearch.Enter += new System.EventHandler(this.txtMasterSearch_Enter);
            this.txtMasterSearch.Leave += new System.EventHandler(this.txtMasterSearch_Leave);
            this.lstOrgUnits.SelectedIndexChanged += new System.EventHandler(this.lstOrgUnits_SelectedIndexChanged);
            this.lstOrgUnits.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstOrgUnits_DrawItem);
            this.dgvConfigs.SelectionChanged += new System.EventHandler(this.dgvConfigs_SelectionChanged);
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            ((System.ComponentModel.ISupportInitialize)(this.nudQuotaGB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWarnPct)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCriticalPct)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvConfigs)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubTitle;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Label lblTreeHeader;
        private System.Windows.Forms.Label lblMasterType;
        private System.Windows.Forms.ComboBox cmbMasterType;
        private System.Windows.Forms.TextBox txtMasterSearch;
        private System.Windows.Forms.ListBox lstOrgUnits;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlEditHost;
        private System.Windows.Forms.Panel pnlEdit;
        private System.Windows.Forms.Label lblSelectedOrg;
        private System.Windows.Forms.Label lblQuota;
        private System.Windows.Forms.NumericUpDown nudQuotaGB;
        private System.Windows.Forms.Label lblGB;
        private System.Windows.Forms.Label lblWarn;
        private System.Windows.Forms.NumericUpDown nudWarnPct;
        private System.Windows.Forms.Label lblWarnPct;
        private System.Windows.Forms.Label lblCritical;
        private System.Windows.Forms.NumericUpDown nudCriticalPct;
        private System.Windows.Forms.Label lblCritPct;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.DataGridView dgvConfigs;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgOrg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgQuota;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgWarn;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgCrit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgDate;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgFileCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgUsedGB;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgUsedPct;
    }
}
