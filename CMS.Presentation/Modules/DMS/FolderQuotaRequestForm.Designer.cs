namespace CMS.Presentation
{
    partial class FolderQuotaRequestForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader       = new System.Windows.Forms.Panel();
            this.lblTitle        = new System.Windows.Forms.Label();
            this.lblSubTitle     = new System.Windows.Forms.Label();
            this.pnlLeft         = new System.Windows.Forms.Panel();
            this.lblTreeHeader   = new System.Windows.Forms.Label();
            this.tvOrgUnits      = new System.Windows.Forms.TreeView();
            this.pnlRight        = new System.Windows.Forms.Panel();
            this.pnlForm         = new System.Windows.Forms.Panel();
            this.lblSelectedOrg  = new System.Windows.Forms.Label();
            this.lblCurrentQuota = new System.Windows.Forms.Label();
            this.lblRequestGB    = new System.Windows.Forms.Label();
            this.nudRequestGB    = new System.Windows.Forms.NumericUpDown();
            this.lblGBUnit       = new System.Windows.Forms.Label();
            this.lblReason       = new System.Windows.Forms.Label();
            this.txtReason       = new System.Windows.Forms.TextBox();
            this.btnSubmit       = new System.Windows.Forms.Button();
            this.lblHistory      = new System.Windows.Forms.Label();
            this.dgvMyRequests   = new System.Windows.Forms.DataGridView();
            this.colReqID        = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReqOrg       = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReqCurrent   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReqRequested = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReqReason    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReqStatus    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReqDate      = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlFooter       = new System.Windows.Forms.Panel();
            this.lblStatus       = new System.Windows.Forms.Label();
            this.btnCancel       = new System.Windows.Forms.Button();
            this.btnClose        = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // Form-level font — all controls inherit unless overridden
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            // ── Header ────────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 64;
            this.pnlHeader.Controls.Add(this.lblSubTitle);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Text      = "ขอเพิ่มพื้นที่ Quota Folder";
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location  = new System.Drawing.Point(14, 8);
            this.lblTitle.Size      = new System.Drawing.Size(520, 30);

            this.lblSubTitle.Text      = "ยื่นคำขออนุมัติเพิ่มขนาดพื้นที่ folder ต่อผู้ดูแลระบบ";
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255);
            this.lblSubTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubTitle.Location  = new System.Drawing.Point(16, 40);
            this.lblSubTitle.Size      = new System.Drawing.Size(520, 20);

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

            this.pnlLeft.Controls.Add(this.tvOrgUnits);
            this.pnlLeft.Controls.Add(this.lblTreeHeader);

            // ── Right panel ───────────────────────────────────────────────────
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;

            // pnlForm (input area)
            this.pnlForm.Dock        = System.Windows.Forms.DockStyle.Top;
            this.pnlForm.Height      = 214;
            this.pnlForm.BackColor   = System.Drawing.Color.White;
            this.pnlForm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            int ex = 16, ey = 14;
            // Row y-positions
            int r0 = ey;        // hint label
            int r1 = ey + 38;   // current quota info
            int r2 = ey + 78;   // request GB input
            int r3 = ey + 120;  // reason + submit button

            // Row 0: selected org placeholder
            this.lblSelectedOrg.Text      = "กรุณาเลือก OrgUnit จากรายการด้านซ้าย";
            this.lblSelectedOrg.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Italic);
            this.lblSelectedOrg.ForeColor = System.Drawing.Color.Gray;
            this.lblSelectedOrg.Location  = new System.Drawing.Point(ex, r0);
            this.lblSelectedOrg.Size      = new System.Drawing.Size(520, 22);

            // Row 1: current quota info
            this.lblCurrentQuota.Text      = "";
            this.lblCurrentQuota.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblCurrentQuota.ForeColor = System.Drawing.Color.FromArgb(0, 80, 160);
            this.lblCurrentQuota.Location  = new System.Drawing.Point(ex, r1);
            this.lblCurrentQuota.Size      = new System.Drawing.Size(520, 24);
            this.lblCurrentQuota.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Row 2: request GB
            this.lblRequestGB.Text      = "ขอเพิ่มเป็น (GB):";
            this.lblRequestGB.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblRequestGB.Location  = new System.Drawing.Point(ex, r2);
            this.lblRequestGB.Size      = new System.Drawing.Size(140, 24);
            this.lblRequestGB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudRequestGB.Location      = new System.Drawing.Point(ex + 144, r2 - 1);
            this.nudRequestGB.Size          = new System.Drawing.Size(92, 28);
            this.nudRequestGB.Font          = new System.Drawing.Font("Segoe UI", 10);
            this.nudRequestGB.Minimum       = 1;
            this.nudRequestGB.Maximum       = 10000;
            this.nudRequestGB.Value         = 40;
            this.nudRequestGB.DecimalPlaces = 1;

            this.lblGBUnit.Text      = "GB";
            this.lblGBUnit.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblGBUnit.Location  = new System.Drawing.Point(ex + 244, r2);
            this.lblGBUnit.Size      = new System.Drawing.Size(32, 24);
            this.lblGBUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Row 3: reason (multiline) + submit button
            this.lblReason.Text      = "เหตุผล:";
            this.lblReason.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblReason.Location  = new System.Drawing.Point(ex, r3);
            this.lblReason.Size      = new System.Drawing.Size(140, 24);
            this.lblReason.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtReason.Location   = new System.Drawing.Point(ex + 144, r3);
            this.txtReason.Size       = new System.Drawing.Size(300, 72);
            this.txtReason.Multiline  = true;
            this.txtReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtReason.MaxLength  = 500;
            this.txtReason.Font       = new System.Drawing.Font("Segoe UI", 10);

            this.btnSubmit.Text      = "ส่งคำขอ";
            this.btnSubmit.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnSubmit.Size      = new System.Drawing.Size(114, 38);
            this.btnSubmit.Location  = new System.Drawing.Point(ex + 458, r3);
            this.btnSubmit.BackColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.btnSubmit.ForeColor = System.Drawing.Color.White;
            this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlForm.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblSelectedOrg, this.lblCurrentQuota,
                this.lblRequestGB, this.nudRequestGB, this.lblGBUnit,
                this.lblReason, this.txtReason, this.btnSubmit
            });

            // history section header
            this.lblHistory.Text      = "  ประวัติคำขอของฉัน";
            this.lblHistory.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.lblHistory.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblHistory.Height    = 32;
            this.lblHistory.BackColor = System.Drawing.Color.FromArgb(220, 232, 248);
            this.lblHistory.ForeColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.lblHistory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblHistory.Padding   = new System.Windows.Forms.Padding(4, 0, 0, 0);

            // dgvMyRequests
            this.dgvMyRequests.Dock                = System.Windows.Forms.DockStyle.Fill;
            this.dgvMyRequests.AllowUserToAddRows  = false;
            this.dgvMyRequests.ReadOnly            = true;
            this.dgvMyRequests.SelectionMode       = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMyRequests.MultiSelect         = false;
            this.dgvMyRequests.RowHeadersVisible   = false;
            this.dgvMyRequests.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMyRequests.BackgroundColor     = System.Drawing.Color.White;
            this.dgvMyRequests.BorderStyle         = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvMyRequests.Font                = new System.Drawing.Font("Segoe UI", 10);
            this.dgvMyRequests.ColumnHeadersHeight = 34;
            this.dgvMyRequests.RowTemplate.Height  = 32;
            this.dgvMyRequests.ColumnHeadersDefaultCellStyle.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.dgvMyRequests.ColumnHeadersDefaultCellStyle.Padding   = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.dgvMyRequests.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.dgvMyRequests.DefaultCellStyle.Padding   = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.dgvMyRequests.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;

            this.colReqID.HeaderText        = "ID";         this.colReqID.Visible         = false;
            this.colReqOrg.HeaderText       = "OrgUnit";    this.colReqOrg.FillWeight     = 150;
            this.colReqOrg.MinimumWidth     = 160;
            this.colReqCurrent.HeaderText   = "Quota เดิม"; this.colReqCurrent.Width      = 100;
            this.colReqCurrent.MinimumWidth = 100;
            this.colReqRequested.HeaderText = "ขอใหม่";     this.colReqRequested.Width    = 100;
            this.colReqRequested.MinimumWidth = 100;
            this.colReqReason.HeaderText    = "เหตุผล";     this.colReqReason.FillWeight  = 200;
            this.colReqReason.MinimumWidth  = 140;
            this.colReqStatus.HeaderText    = "สถานะ";      this.colReqStatus.Width       = 110;
            this.colReqStatus.MinimumWidth  = 110;
            this.colReqDate.HeaderText      = "วันที่ขอ";   this.colReqDate.Width         = 120;
            this.colReqDate.MinimumWidth    = 120;

            this.dgvMyRequests.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colReqID, this.colReqOrg, this.colReqCurrent, this.colReqRequested,
                this.colReqReason, this.colReqStatus, this.colReqDate
            });

            // dgvMyRequests (Fill) first, then lblHistory (Top), then pnlForm (Top)
            this.pnlRight.Controls.Add(this.dgvMyRequests);
            this.pnlRight.Controls.Add(this.lblHistory);
            this.pnlRight.Controls.Add(this.pnlForm);

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

            this.btnCancel.Text      = "ยกเลิกคำขอ";
            this.btnCancel.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnCancel.Size      = new System.Drawing.Size(120, 32);
            this.btnCancel.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(180, 80, 0);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnClose.Text      = "ปิด";
            this.btnClose.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnClose.Size      = new System.Drawing.Size(90, 32);
            this.btnClose.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnClose.BackColor = System.Drawing.Color.Gray;
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlFooter.Controls.Add(this.lblStatus);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Controls.Add(this.btnClose);

            // ── Form ──────────────────────────────────────────────────────────
            this.Text          = "ขอเพิ่มพื้นที่ Quota Folder";
            this.Size          = new System.Drawing.Size(1060, 700);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize   = new System.Drawing.Size(960, 620);

            // Dock order: pnlRight (Fill) first, pnlLeft (Left) next, then Header/Footer
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);

            this.tvOrgUnits.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvOrgUnits_AfterSelect);
            this.btnSubmit.Click        += new System.EventHandler(this.btnSubmit_Click);
            this.btnCancel.Click        += new System.EventHandler(this.btnCancel_Click);
            this.btnClose.Click         += new System.EventHandler(this.btnClose_Click);

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel         pnlHeader;
        private System.Windows.Forms.Label         lblTitle;
        private System.Windows.Forms.Label         lblSubTitle;
        private System.Windows.Forms.Panel         pnlLeft;
        private System.Windows.Forms.Label         lblTreeHeader;
        private System.Windows.Forms.TreeView      tvOrgUnits;
        private System.Windows.Forms.Panel         pnlRight;
        private System.Windows.Forms.Panel         pnlForm;
        private System.Windows.Forms.Label         lblSelectedOrg;
        private System.Windows.Forms.Label         lblCurrentQuota;
        private System.Windows.Forms.Label         lblRequestGB;
        private System.Windows.Forms.NumericUpDown nudRequestGB;
        private System.Windows.Forms.Label         lblGBUnit;
        private System.Windows.Forms.Label         lblReason;
        private System.Windows.Forms.TextBox       txtReason;
        private System.Windows.Forms.Button        btnSubmit;
        private System.Windows.Forms.Label         lblHistory;
        private System.Windows.Forms.DataGridView  dgvMyRequests;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqOrg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqCurrent;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqRequested;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqReason;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqDate;
        private System.Windows.Forms.Panel         pnlFooter;
        private System.Windows.Forms.Label         lblStatus;
        private System.Windows.Forms.Button        btnCancel;
        private System.Windows.Forms.Button        btnClose;
    }
}
