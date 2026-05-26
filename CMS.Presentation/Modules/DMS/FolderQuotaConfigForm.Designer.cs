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
            this.pnlHeader      = new System.Windows.Forms.Panel();
            this.lblTitle       = new System.Windows.Forms.Label();
            this.lblSubTitle    = new System.Windows.Forms.Label();
            this.pnlLeft        = new System.Windows.Forms.Panel();
            this.lblTreeHeader  = new System.Windows.Forms.Label();
            this.tvOrgUnits     = new System.Windows.Forms.TreeView();
            this.pnlRight       = new System.Windows.Forms.Panel();
            this.pnlEdit        = new System.Windows.Forms.Panel();
            this.lblSelectedOrg = new System.Windows.Forms.Label();
            this.lblQuota       = new System.Windows.Forms.Label();
            this.nudQuotaGB     = new System.Windows.Forms.NumericUpDown();
            this.lblGB          = new System.Windows.Forms.Label();
            this.lblWarn        = new System.Windows.Forms.Label();
            this.nudWarnPct     = new System.Windows.Forms.NumericUpDown();
            this.lblWarnPct     = new System.Windows.Forms.Label();
            this.lblCritical    = new System.Windows.Forms.Label();
            this.nudCriticalPct = new System.Windows.Forms.NumericUpDown();
            this.lblCritPct     = new System.Windows.Forms.Label();
            this.btnSave        = new System.Windows.Forms.Button();
            this.btnDelete      = new System.Windows.Forms.Button();
            this.btnClear       = new System.Windows.Forms.Button();
            this.dgvConfigs     = new System.Windows.Forms.DataGridView();
            this.colCfgID       = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgType     = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgOrg      = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgQuota    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgWarn     = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgCrit     = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCfgDate     = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlFooter      = new System.Windows.Forms.Panel();
            this.lblStatus      = new System.Windows.Forms.Label();
            this.btnClose       = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // ── Header ────────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 60;
            this.pnlHeader.Controls.Add(this.lblSubTitle);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Text      = "กำหนด Quota พื้นที่ Folder";
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location  = new System.Drawing.Point(12, 8);
            this.lblTitle.Size      = new System.Drawing.Size(500, 26);

            this.lblSubTitle.Text      = "ตั้งค่า Quota สำหรับแต่ละ OrgUnit (Sub / Division / Department)";
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255);
            this.lblSubTitle.Font      = new System.Drawing.Font("Segoe UI", 9);
            this.lblSubTitle.Location  = new System.Drawing.Point(14, 36);
            this.lblSubTitle.Size      = new System.Drawing.Size(560, 18);

            // ── Left panel (OrgUnit TreeView) ─────────────────────────────────
            this.pnlLeft.Dock        = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width       = 270;
            this.pnlLeft.BackColor   = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlLeft.BorderStyle = System.Windows.Forms.BorderStyle.None;

            this.lblTreeHeader.Text          = "  เลือก OrgUnit";
            this.lblTreeHeader.Dock          = System.Windows.Forms.DockStyle.Top;
            this.lblTreeHeader.Height        = 28;
            this.lblTreeHeader.Font          = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            this.lblTreeHeader.ForeColor     = System.Drawing.Color.FromArgb(26, 58, 108);
            this.lblTreeHeader.BackColor     = System.Drawing.Color.FromArgb(220, 232, 248);
            this.lblTreeHeader.TextAlign     = System.Drawing.ContentAlignment.MiddleLeft;

            this.tvOrgUnits.Dock         = System.Windows.Forms.DockStyle.Fill;
            this.tvOrgUnits.BorderStyle  = System.Windows.Forms.BorderStyle.None;
            this.tvOrgUnits.ShowLines    = true;
            this.tvOrgUnits.FullRowSelect = true;
            this.tvOrgUnits.HideSelection = false;
            this.tvOrgUnits.ItemHeight   = 22;
            this.tvOrgUnits.Font         = new System.Drawing.Font("Segoe UI", 9);
            this.tvOrgUnits.BackColor    = System.Drawing.Color.FromArgb(245, 247, 250);
            this.tvOrgUnits.Indent       = 18;

            // tvOrgUnits (Fill) first, then lblTreeHeader (Top) → header docks at top
            this.pnlLeft.Controls.Add(this.tvOrgUnits);
            this.pnlLeft.Controls.Add(this.lblTreeHeader);

            // ── Right panel (edit form + grid) ────────────────────────────────
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;

            // pnlEdit
            this.pnlEdit.Dock        = System.Windows.Forms.DockStyle.Top;
            this.pnlEdit.Height      = 132;
            this.pnlEdit.BackColor   = System.Drawing.Color.White;
            this.pnlEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            int ex = 12, ey = 8, lw = 112, ew = 90;

            // Row 0: selected-org label
            this.lblSelectedOrg.Text      = "กรุณาเลือก OrgUnit จากรายการด้านซ้าย";
            this.lblSelectedOrg.Font      = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Italic);
            this.lblSelectedOrg.ForeColor = System.Drawing.Color.Gray;
            this.lblSelectedOrg.Location  = new System.Drawing.Point(ex, ey);
            this.lblSelectedOrg.Size      = new System.Drawing.Size(580, 22);

            // Row 1: Quota (left) | Warning (right)
            this.lblQuota.Text     = "Quota (GB):";
            this.lblQuota.Location = new System.Drawing.Point(ex, ey + 32);
            this.lblQuota.Size     = new System.Drawing.Size(lw, 22);

            this.nudQuotaGB.Location     = new System.Drawing.Point(ex + lw, ey + 30);
            this.nudQuotaGB.Size         = new System.Drawing.Size(ew, 26);
            this.nudQuotaGB.Minimum      = 1;
            this.nudQuotaGB.Maximum      = 10000;
            this.nudQuotaGB.Value        = 40;
            this.nudQuotaGB.DecimalPlaces = 1;

            this.lblGB.Text     = "GB";
            this.lblGB.Location = new System.Drawing.Point(ex + lw + ew + 4, ey + 32);
            this.lblGB.Size     = new System.Drawing.Size(28, 22);

            this.lblWarn.Text     = "แจ้งเตือน (%):";
            this.lblWarn.Location = new System.Drawing.Point(ex + 320, ey + 32);
            this.lblWarn.Size     = new System.Drawing.Size(105, 22);

            this.nudWarnPct.Location = new System.Drawing.Point(ex + 428, ey + 30);
            this.nudWarnPct.Size     = new System.Drawing.Size(65, 26);
            this.nudWarnPct.Minimum  = 50;
            this.nudWarnPct.Maximum  = 99;
            this.nudWarnPct.Value    = 80;

            this.lblWarnPct.Text     = "%";
            this.lblWarnPct.Location = new System.Drawing.Point(ex + 496, ey + 32);
            this.lblWarnPct.Size     = new System.Drawing.Size(20, 22);

            // Row 2: Critical (right side only)
            this.lblCritical.Text     = "วิกฤต (%):";
            this.lblCritical.Location = new System.Drawing.Point(ex + 320, ey + 62);
            this.lblCritical.Size     = new System.Drawing.Size(105, 22);

            this.nudCriticalPct.Location = new System.Drawing.Point(ex + 428, ey + 60);
            this.nudCriticalPct.Size     = new System.Drawing.Size(65, 26);
            this.nudCriticalPct.Minimum  = 51;
            this.nudCriticalPct.Maximum  = 100;
            this.nudCriticalPct.Value    = 95;

            this.lblCritPct.Text     = "%";
            this.lblCritPct.Location = new System.Drawing.Point(ex + 496, ey + 62);
            this.lblCritPct.Size     = new System.Drawing.Size(20, 22);

            // Row 3: Buttons
            this.btnSave.Text      = "บันทึก";
            this.btnSave.Size      = new System.Drawing.Size(96, 32);
            this.btnSave.Location  = new System.Drawing.Point(ex, ey + 90);
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(52, 120, 47);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnDelete.Text      = "ลบ";
            this.btnDelete.Size      = new System.Drawing.Size(96, 32);
            this.btnDelete.Location  = new System.Drawing.Point(ex + 102, ey + 90);
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(180, 30, 30);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnClear.Text      = "ล้าง";
            this.btnClear.Size      = new System.Drawing.Size(96, 32);
            this.btnClear.Location  = new System.Drawing.Point(ex + 204, ey + 90);
            this.btnClear.BackColor = System.Drawing.Color.Gray;
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlEdit.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblSelectedOrg,
                this.lblQuota, this.nudQuotaGB, this.lblGB,
                this.lblWarn, this.nudWarnPct, this.lblWarnPct,
                this.lblCritical, this.nudCriticalPct, this.lblCritPct,
                this.btnSave, this.btnDelete, this.btnClear
            });

            // dgvConfigs
            this.dgvConfigs.Dock                = System.Windows.Forms.DockStyle.Fill;
            this.dgvConfigs.AllowUserToAddRows  = false;
            this.dgvConfigs.ReadOnly            = true;
            this.dgvConfigs.SelectionMode       = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvConfigs.MultiSelect         = false;
            this.dgvConfigs.RowHeadersVisible   = false;
            this.dgvConfigs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvConfigs.BackgroundColor     = System.Drawing.Color.White;

            this.colCfgID.HeaderText   = "ID";          this.colCfgID.Visible    = false;
            this.colCfgType.HeaderText = "ประเภท";       this.colCfgType.Width    = 90;
            this.colCfgOrg.HeaderText  = "OrgUnit";      this.colCfgOrg.FillWeight = 200;
            this.colCfgQuota.HeaderText = "Quota";       this.colCfgQuota.Width   = 90;
            this.colCfgWarn.HeaderText  = "Warning";     this.colCfgWarn.Width    = 80;
            this.colCfgCrit.HeaderText  = "Critical";    this.colCfgCrit.Width    = 80;
            this.colCfgDate.HeaderText  = "แก้ไขล่าสุด"; this.colCfgDate.Width    = 100;

            this.dgvConfigs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colCfgID, this.colCfgType, this.colCfgOrg,
                this.colCfgQuota, this.colCfgWarn, this.colCfgCrit, this.colCfgDate
            });

            // dgvConfigs (Fill) first, then pnlEdit (Top) → edit panel docks at top
            this.pnlRight.Controls.Add(this.dgvConfigs);
            this.pnlRight.Controls.Add(this.pnlEdit);

            // ── Footer ────────────────────────────────────────────────────────
            this.pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height    = 40;
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

            this.lblStatus.Dock    = System.Windows.Forms.DockStyle.Left;
            this.lblStatus.Text    = "";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);

            this.btnClose.Text      = "ปิด";
            this.btnClose.Size      = new System.Drawing.Size(80, 28);
            this.btnClose.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlFooter.Controls.Add(this.lblStatus);
            this.pnlFooter.Controls.Add(this.btnClose);

            // ── Form ──────────────────────────────────────────────────────────
            this.Text            = "กำหนด Quota พื้นที่ Folder";
            this.Size            = new System.Drawing.Size(980, 660);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize     = new System.Drawing.Size(900, 580);

            // Dock order: pnlRight (Fill) added first, pnlLeft (Left) next,
            // then pnlHeader (Top) and pnlFooter (Bottom) last.
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);

            this.tvOrgUnits.AfterSelect      += new System.Windows.Forms.TreeViewEventHandler(this.tvOrgUnits_AfterSelect);
            this.dgvConfigs.SelectionChanged += new System.EventHandler(this.dgvConfigs_SelectionChanged);
            this.btnSave.Click               += new System.EventHandler(this.btnSave_Click);
            this.btnDelete.Click             += new System.EventHandler(this.btnDelete_Click);
            this.btnClear.Click              += new System.EventHandler(this.btnClear_Click);
            this.btnClose.Click              += new System.EventHandler(this.btnClose_Click);

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel        pnlHeader;
        private System.Windows.Forms.Label        lblTitle;
        private System.Windows.Forms.Label        lblSubTitle;
        private System.Windows.Forms.Panel        pnlLeft;
        private System.Windows.Forms.Label        lblTreeHeader;
        private System.Windows.Forms.TreeView     tvOrgUnits;
        private System.Windows.Forms.Panel        pnlRight;
        private System.Windows.Forms.Panel        pnlEdit;
        private System.Windows.Forms.Label        lblSelectedOrg;
        private System.Windows.Forms.Label        lblQuota;
        private System.Windows.Forms.NumericUpDown nudQuotaGB;
        private System.Windows.Forms.Label        lblGB;
        private System.Windows.Forms.Label        lblWarn;
        private System.Windows.Forms.NumericUpDown nudWarnPct;
        private System.Windows.Forms.Label        lblWarnPct;
        private System.Windows.Forms.Label        lblCritical;
        private System.Windows.Forms.NumericUpDown nudCriticalPct;
        private System.Windows.Forms.Label        lblCritPct;
        private System.Windows.Forms.Button       btnSave;
        private System.Windows.Forms.Button       btnDelete;
        private System.Windows.Forms.Button       btnClear;
        private System.Windows.Forms.DataGridView dgvConfigs;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgOrg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgQuota;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgWarn;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgCrit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCfgDate;
        private System.Windows.Forms.Panel        pnlFooter;
        private System.Windows.Forms.Label        lblStatus;
        private System.Windows.Forms.Button       btnClose;
    }
}
