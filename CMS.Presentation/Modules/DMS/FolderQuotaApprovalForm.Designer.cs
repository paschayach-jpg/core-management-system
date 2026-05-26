namespace CMS.Presentation
{
    partial class FolderQuotaApprovalForm
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
            this.pnlToolbar     = new System.Windows.Forms.Panel();
            this.chkPendingOnly = new System.Windows.Forms.CheckBox();
            this.btnRefresh     = new System.Windows.Forms.Button();
            this.dgvRequests    = new System.Windows.Forms.DataGridView();
            this.colID          = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOrg         = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCurrent     = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRequested   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReason      = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRequestBy   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReqDate     = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus      = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlAction      = new System.Windows.Forms.Panel();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.lblDetail      = new System.Windows.Forms.Label();
            this.lblRemark      = new System.Windows.Forms.Label();
            this.txtRemark      = new System.Windows.Forms.TextBox();
            this.btnApprove     = new System.Windows.Forms.Button();
            this.btnReject      = new System.Windows.Forms.Button();
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

            this.lblTitle.Text      = "อนุมัติคำขอเพิ่ม Quota พื้นที่ Folder";
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location  = new System.Drawing.Point(14, 8);
            this.lblTitle.Size      = new System.Drawing.Size(640, 30);

            this.lblSubTitle.Text      = "ตรวจสอบและอนุมัติ / ปฏิเสธคำขอเพิ่ม quota พื้นที่ folder";
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255);
            this.lblSubTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubTitle.Location  = new System.Drawing.Point(16, 40);
            this.lblSubTitle.Size      = new System.Drawing.Size(540, 20);

            // ── Toolbar ───────────────────────────────────────────────────────
            this.pnlToolbar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height    = 44;
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            this.pnlToolbar.Padding   = new System.Windows.Forms.Padding(10, 6, 10, 6);

            this.chkPendingOnly.Text    = "แสดงเฉพาะรอดำเนินการ";
            this.chkPendingOnly.Font    = new System.Drawing.Font("Segoe UI", 10);
            this.chkPendingOnly.Checked = true;
            this.chkPendingOnly.Dock    = System.Windows.Forms.DockStyle.Left;
            this.chkPendingOnly.Width   = 210;

            this.btnRefresh.Text      = "รีเฟรช";
            this.btnRefresh.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnRefresh.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnRefresh.Width     = 100;
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlToolbar.Controls.Add(this.chkPendingOnly);
            this.pnlToolbar.Controls.Add(this.btnRefresh);

            // ── DataGridView ──────────────────────────────────────────────────
            this.dgvRequests.Dock                = System.Windows.Forms.DockStyle.Fill;
            this.dgvRequests.AllowUserToAddRows  = false;
            this.dgvRequests.ReadOnly            = true;
            this.dgvRequests.SelectionMode       = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRequests.MultiSelect         = false;
            this.dgvRequests.RowHeadersVisible   = false;
            this.dgvRequests.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRequests.BackgroundColor     = System.Drawing.Color.White;
            this.dgvRequests.BorderStyle         = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvRequests.Font                = new System.Drawing.Font("Segoe UI", 10);
            this.dgvRequests.ColumnHeadersHeight = 34;
            this.dgvRequests.RowTemplate.Height  = 32;
            this.dgvRequests.ColumnHeadersDefaultCellStyle.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.dgvRequests.ColumnHeadersDefaultCellStyle.Padding   = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.dgvRequests.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.dgvRequests.DefaultCellStyle.Padding   = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.dgvRequests.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;

            this.colID.HeaderText        = "ID";         this.colID.Visible          = false;
            this.colOrg.HeaderText       = "OrgUnit";    this.colOrg.FillWeight      = 160;
            this.colOrg.MinimumWidth     = 160;
            this.colCurrent.HeaderText   = "Quota เดิม"; this.colCurrent.Width       = 100;
            this.colCurrent.MinimumWidth = 100;
            this.colRequested.HeaderText   = "ขอใหม่";   this.colRequested.Width     = 100;
            this.colRequested.MinimumWidth = 100;
            this.colReason.HeaderText    = "เหตุผล";     this.colReason.FillWeight   = 200;
            this.colReason.MinimumWidth  = 140;
            this.colRequestBy.HeaderText   = "ผู้ขอ";    this.colRequestBy.Width     = 120;
            this.colRequestBy.MinimumWidth = 120;
            this.colReqDate.HeaderText   = "วันที่ขอ";   this.colReqDate.Width       = 120;
            this.colReqDate.MinimumWidth = 120;
            this.colStatus.HeaderText    = "สถานะ";      this.colStatus.Width        = 110;
            this.colStatus.MinimumWidth  = 110;

            this.dgvRequests.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colID, this.colOrg, this.colCurrent, this.colRequested,
                this.colReason, this.colRequestBy, this.colReqDate, this.colStatus
            });

            // ── Action panel (bottom, approve/reject) ─────────────────────────
            this.pnlAction.Dock        = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAction.Height      = 134;
            this.pnlAction.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAction.BackColor   = System.Drawing.Color.FromArgb(252, 252, 252);

            int ax = 14, ay = 12;

            this.lblDetailTitle.Text      = "รายละเอียดที่เลือก:";
            this.lblDetailTitle.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.Location  = new System.Drawing.Point(ax, ay);
            this.lblDetailTitle.Size      = new System.Drawing.Size(160, 24);
            this.lblDetailTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblDetail.Text      = "-";
            this.lblDetail.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblDetail.ForeColor = System.Drawing.Color.FromArgb(30, 60, 120);
            this.lblDetail.Location  = new System.Drawing.Point(ax, ay + 30);
            this.lblDetail.Size      = new System.Drawing.Size(560, 38);

            this.lblRemark.Text      = "หมายเหตุ:";
            this.lblRemark.Font      = new System.Drawing.Font("Segoe UI", 10);
            this.lblRemark.Location  = new System.Drawing.Point(ax, ay + 78);
            this.lblRemark.Size      = new System.Drawing.Size(88, 26);
            this.lblRemark.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtRemark.Font     = new System.Drawing.Font("Segoe UI", 10);
            this.txtRemark.Location = new System.Drawing.Point(ax + 96, ay + 78);
            this.txtRemark.Size     = new System.Drawing.Size(472, 28);
            this.txtRemark.MaxLength = 300;

            this.btnApprove.Text      = "อนุมัติ";
            this.btnApprove.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnApprove.Size      = new System.Drawing.Size(110, 38);
            this.btnApprove.Location  = new System.Drawing.Point(590, ay + 12);
            this.btnApprove.BackColor = System.Drawing.Color.FromArgb(52, 120, 47);
            this.btnApprove.ForeColor = System.Drawing.Color.White;
            this.btnApprove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApprove.Enabled   = false;

            this.btnReject.Text      = "ปฏิเสธ";
            this.btnReject.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.btnReject.Size      = new System.Drawing.Size(110, 38);
            this.btnReject.Location  = new System.Drawing.Point(590, ay + 60);
            this.btnReject.BackColor = System.Drawing.Color.FromArgb(180, 30, 30);
            this.btnReject.ForeColor = System.Drawing.Color.White;
            this.btnReject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReject.Enabled   = false;

            this.pnlAction.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblDetailTitle, this.lblDetail,
                this.lblRemark, this.txtRemark,
                this.btnApprove, this.btnReject
            });

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
            this.btnClose.BackColor = System.Drawing.Color.Gray;
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlFooter.Controls.Add(this.lblStatus);
            this.pnlFooter.Controls.Add(this.btnClose);

            // ── Form ──────────────────────────────────────────────────────────
            this.Text          = "อนุมัติคำขอเพิ่ม Quota Folder";
            this.Size          = new System.Drawing.Size(960, 700);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize   = new System.Drawing.Size(860, 600);

            // Dock order: dgvRequests (Fill) added first so it fills remaining space
            this.Controls.Add(this.dgvRequests);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlAction);
            this.Controls.Add(this.pnlFooter);

            this.dgvRequests.SelectionChanged  += new System.EventHandler(this.dgvRequests_SelectionChanged);
            this.chkPendingOnly.CheckedChanged += new System.EventHandler(this.chkPendingOnly_CheckedChanged);
            this.btnApprove.Click              += new System.EventHandler(this.btnApprove_Click);
            this.btnReject.Click               += new System.EventHandler(this.btnReject_Click);
            this.btnRefresh.Click              += new System.EventHandler(this.btnRefresh_Click);
            this.btnClose.Click                += new System.EventHandler(this.btnClose_Click);

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel     pnlHeader;
        private System.Windows.Forms.Label     lblTitle;
        private System.Windows.Forms.Label     lblSubTitle;
        private System.Windows.Forms.Panel     pnlToolbar;
        private System.Windows.Forms.CheckBox  chkPendingOnly;
        private System.Windows.Forms.Button    btnRefresh;
        private System.Windows.Forms.DataGridView dgvRequests;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOrg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCurrent;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRequested;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReason;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRequestBy;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.Panel     pnlAction;
        private System.Windows.Forms.Label     lblDetailTitle;
        private System.Windows.Forms.Label     lblDetail;
        private System.Windows.Forms.Label     lblRemark;
        private System.Windows.Forms.TextBox   txtRemark;
        private System.Windows.Forms.Button    btnApprove;
        private System.Windows.Forms.Button    btnReject;
        private System.Windows.Forms.Panel     pnlFooter;
        private System.Windows.Forms.Label     lblStatus;
        private System.Windows.Forms.Button    btnClose;
    }
}
