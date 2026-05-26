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
            this.pnlEditHost    = new System.Windows.Forms.Panel();
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
            this.btnCloseForm   = new System.Windows.Forms.Button();
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

            // Form-level font — all controls inherit unless overridden
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            // ── Header ────────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 64;
            this.pnlHeader.Controls.Add(this.lblSubTitle);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Text      = "กำหนด Quota พื้นที่ Folder";
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location  = new System.Drawing.Point(14, 8);
            this.lblTitle.Size      = new System.Drawing.Size(520, 30);

            this.lblSubTitle.Text      = "ตั้งค่า Quota สำหรับแต่ละ OrgUnit (Sub / Division / Department)";
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255);
            this.lblSubTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubTitle.Location  = new System.Drawing.Point(16, 40);
            this.lblSubTitle.Size      = new System.Drawing.Size(580, 20);

            // ── Left panel (OrgUnit TreeView) ─────────────────────────────────
            this.pnlLeft.Dock        = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width       = 280;
            this.pnlLeft.BackColor   = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlLeft.BorderStyle = System.Windows.Forms.BorderStyle.None;

            this.lblTreeHeader.Text      = "  เลือก OrgUnit";
            this.lblTreeHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblTreeHeader.Height    = 32;
            this.lblTreeHeader.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.lblTreeHeader.ForeColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.lblTreeHeader.BackColor = System.Drawing.Color.FromArgb(220, 232, 248);
            this.lblTreeHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.tvOrgUnits.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.tvOrgUnits.BorderStyle   = System.Windows.Forms.BorderStyle.None;
            this.tvOrgUnits.ShowLines     = true;
            this.tvOrgUnits.FullRowSelect = true;
            this.tvOrgUnits.HideSelection = false;
            this.tvOrgUnits.ItemHeight    = 26;
            this.tvOrgUnits.Font          = new System.Drawing.Font("Segoe UI", 10);
            this.tvOrgUnits.BackColor     = System.Drawing.Color.FromArgb(245, 247, 250);
            this.tvOrgUnits.Indent        = 20;

            // tvOrgUnits (Fill) first, then lblTreeHeader (Top) → header docks at top
            this.pnlLeft.Controls.Add(this.tvOrgUnits);
            this.pnlLeft.Controls.Add(this.lblTreeHeader);

            // ── Right panel (edit form + grid) ────────────────────────────────
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;

            // pnlEditHost / pnlEdit
            this.pnlEditHost.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlEditHost.Height    = 220;
            this.pnlEditHost.BackColor = System.Drawing.Color.White;
            this.pnlEditHost.Padding   = new System.Windows.Forms.Padding(14, 10, 0, 12);

            this.pnlEdit.Location    = new System.Drawing.Point(14, 10);
            this.pnlEdit.Size        = new System.Drawing.Size(620, 196);
            this.pnlEdit.Anchor      = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.pnlEdit.BackColor   = System.Drawing.Color.White;
            this.pnlEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // Layout constants
            int ex = 16, ey = 14, lw = 116, ew = 92;
            // Row y-positions (label / NUD offset -2 for vertical alignment)
            int r0 = ey;          // Row 0: hint label
            int r1 = ey + 46;     // Row 1: Quota + Warning labels
            int r2 = ey + 90;     // Row 2: Critical labels
            int r3 = ey + 138;    // Row 3: buttons

            // Row 0: selected-org hint
            this.lblSelectedOrg.Text      = "กรุณาเลือก OrgUnit จากรายการด้านซ้าย";
            this.lblSelectedOrg.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Italic);
            this.lblSelectedOrg.ForeColor = System.Drawing.Color.Gray;
            this.lblSelectedOrg.Location  = new System.Drawing.Point(ex, r0);
            this.lblSelectedOrg.Size      = new System.Drawing.Size(560, 22);

            // Row 1: Quota (left) | Warning (right)
            this.lblQuota.Text      = "Quota (GB):";
            this.lblQuota.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblQuota.Location  = new System.Drawing.Point(ex, r1);
            this.lblQuota.Size      = new System.Drawing.Size(lw, 24);
            this.lblQuota.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudQuotaGB.Location      = new System.Drawing.Point(ex + lw + 4, r1 - 1);
            this.nudQuotaGB.Size          = new System.Drawing.Size(ew, 28);
            this.nudQuotaGB.Font          = new System.Drawing.Font("Segoe UI", 10);
            this.nudQuotaGB.Minimum       = 1;
            this.nudQuotaGB.Maximum       = 10000;
            this.nudQuotaGB.Value         = 40;
            this.nudQuotaGB.DecimalPlaces = 1;

            this.lblGB.Text      = "GB";
            this.lblGB.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblGB.Location  = new System.Drawing.Point(ex + lw + ew + 10, r1);
            this.lblGB.Size      = new System.Drawing.Size(32, 24);
            this.lblGB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblWarn.Text      = "แจ้งเตือน (%):";
            this.lblWarn.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblWarn.Location  = new System.Drawing.Point(ex + 310, r1);
            this.lblWarn.Size      = new System.Drawing.Size(128, 24);
            this.lblWarn.AutoSize  = false;
            this.lblWarn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudWarnPct.Location = new System.Drawing.Point(ex + 310 + 132, r1 - 1);
            this.nudWarnPct.Size     = new System.Drawing.Size(72, 28);
            this.nudWarnPct.Font     = new System.Drawing.Font("Segoe UI", 10);
            this.nudWarnPct.Minimum  = 50;
            this.nudWarnPct.Maximum  = 99;
            this.nudWarnPct.Value    = 80;

            this.lblWarnPct.Text      = "%";
            this.lblWarnPct.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblWarnPct.Location  = new System.Drawing.Point(ex + 310 + 132 + 76, r1);
            this.lblWarnPct.Size      = new System.Drawing.Size(26, 24);
            this.lblWarnPct.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Row 2: Critical (right side only)
            this.lblCritical.Text      = "วิกฤต (%):";
            this.lblCritical.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblCritical.Location  = new System.Drawing.Point(ex + 310, r2);
            this.lblCritical.Size      = new System.Drawing.Size(128, 24);
            this.lblCritical.AutoSize  = false;
            this.lblCritical.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudCriticalPct.Location = new System.Drawing.Point(ex + 310 + 132, r2 - 1);
            this.nudCriticalPct.Size     = new System.Drawing.Size(72, 28);
            this.nudCriticalPct.Font     = new System.Drawing.Font("Segoe UI", 10);
            this.nudCriticalPct.Minimum  = 51;
            this.nudCriticalPct.Maximum  = 100;
            this.nudCriticalPct.Value    = 95;

            this.lblCritPct.Text      = "%";
            this.lblCritPct.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblCritPct.Location  = new System.Drawing.Point(ex + 310 + 132 + 76, r2);
            this.lblCritPct.Size      = new System.Drawing.Size(26, 24);
            this.lblCritPct.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Row 3: Buttons
            this.btnSave.Text      = "บันทึก";
            this.btnSave.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnSave.Size      = new System.Drawing.Size(104, 36);
            this.btnSave.Location  = new System.Drawing.Point(ex, r3);
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(52, 120, 47);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnDelete.Text      = "ลบ";
            this.btnDelete.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnDelete.Size      = new System.Drawing.Size(104, 36);
            this.btnDelete.Location  = new System.Drawing.Point(ex + 114, r3);
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(180, 30, 30);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnClear.Text      = "ล้าง";
            this.btnClear.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnClear.Size      = new System.Drawing.Size(104, 36);
            this.btnClear.Location  = new System.Drawing.Point(ex + 228, r3);
            this.btnClear.BackColor = System.Drawing.Color.Gray;
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnCloseForm.Text      = "ปิด";
            this.btnCloseForm.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnCloseForm.Size      = new System.Drawing.Size(104, 36);
            this.btnCloseForm.Location  = new System.Drawing.Point(ex + 342, r3);
            this.btnCloseForm.BackColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.btnCloseForm.ForeColor = System.Drawing.Color.White;
            this.btnCloseForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlEdit.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblSelectedOrg,
                this.lblQuota, this.nudQuotaGB, this.lblGB,
                this.lblWarn, this.nudWarnPct, this.lblWarnPct,
                this.lblCritical, this.nudCriticalPct, this.lblCritPct,
                this.btnSave, this.btnDelete, this.btnClear, this.btnCloseForm
            });
            this.pnlEditHost.Controls.Add(this.pnlEdit);

            // dgvConfigs
            this.dgvConfigs.Dock                = System.Windows.Forms.DockStyle.Fill;
            this.dgvConfigs.AllowUserToAddRows  = false;
            this.dgvConfigs.ReadOnly            = true;
            this.dgvConfigs.SelectionMode       = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvConfigs.MultiSelect         = false;
            this.dgvConfigs.RowHeadersVisible   = false;
            this.dgvConfigs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvConfigs.BackgroundColor     = System.Drawing.Color.White;
            this.dgvConfigs.BorderStyle         = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvConfigs.Font                = new System.Drawing.Font("Segoe UI", 10);
            this.dgvConfigs.ColumnHeadersHeight = 34;
            this.dgvConfigs.RowTemplate.Height  = 32;
            this.dgvConfigs.ColumnHeadersDefaultCellStyle.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.dgvConfigs.ColumnHeadersDefaultCellStyle.Padding   = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.dgvConfigs.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.dgvConfigs.DefaultCellStyle.Padding   = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.dgvConfigs.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;

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
            this.colCfgType.FillWeight  = 90;
            this.colCfgType.MinimumWidth = 90;
            this.colCfgOrg.FillWeight   = 230;
            this.colCfgOrg.MinimumWidth = 240;
            this.colCfgQuota.FillWeight = 90;
            this.colCfgQuota.MinimumWidth = 90;
            this.colCfgWarn.FillWeight  = 85;
            this.colCfgWarn.MinimumWidth = 85;
            this.colCfgCrit.FillWeight  = 85;
            this.colCfgCrit.MinimumWidth = 85;
            this.colCfgDate.FillWeight  = 110;
            this.colCfgDate.MinimumWidth = 110;

            // dgvConfigs (Fill) first, then pnlEditHost (Top) -> edit panel keeps compact size
            this.pnlRight.Controls.Add(this.dgvConfigs);
            this.pnlRight.Controls.Add(this.pnlEditHost);

            // ── Footer ────────────────────────────────────────────────────────
            this.pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height    = 50;
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.pnlFooter.Padding   = new System.Windows.Forms.Padding(14, 9, 14, 9);

            this.lblStatus.Dock         = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Text         = "";
            this.lblStatus.Font         = new System.Drawing.Font("Segoe UI", 10);
            this.lblStatus.Padding      = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.lblStatus.TextAlign    = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.AutoEllipsis = true;

            this.btnClose.Text      = "ปิด";
            this.btnClose.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnClose.Size      = new System.Drawing.Size(90, 32);
            this.btnClose.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlFooter.Controls.Add(this.lblStatus);
            this.pnlFooter.Controls.Add(this.btnClose);

            // ── Form ──────────────────────────────────────────────────────────
            this.Text            = "กำหนด Quota พื้นที่ Folder";
            this.Size            = new System.Drawing.Size(1060, 700);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize     = new System.Drawing.Size(960, 620);

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
            this.btnCloseForm.Click          += new System.EventHandler(this.btnClose_Click);

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel        pnlHeader;
        private System.Windows.Forms.Label        lblTitle;
        private System.Windows.Forms.Label        lblSubTitle;
        private System.Windows.Forms.Panel        pnlLeft;
        private System.Windows.Forms.Label        lblTreeHeader;
        private System.Windows.Forms.TreeView     tvOrgUnits;
        private System.Windows.Forms.Panel        pnlRight;
        private System.Windows.Forms.Panel        pnlEditHost;
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
        private System.Windows.Forms.Button       btnCloseForm;
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
