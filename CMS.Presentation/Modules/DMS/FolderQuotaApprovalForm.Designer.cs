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
            this.pnlHeader    = new System.Windows.Forms.Panel();
            this.lblTitle     = new System.Windows.Forms.Label();
            this.lblSubTitle  = new System.Windows.Forms.Label();
            this.pnlToolbar   = new System.Windows.Forms.Panel();
            this.chkPendingOnly = new System.Windows.Forms.CheckBox();
            this.btnRefresh   = new System.Windows.Forms.Button();
            this.dgvRequests  = new System.Windows.Forms.DataGridView();
            this.colID        = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOrg       = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCurrent   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRequested = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReason    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRequestBy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReqDate   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlAction    = new System.Windows.Forms.Panel();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.lblDetail    = new System.Windows.Forms.Label();
            this.lblRemark    = new System.Windows.Forms.Label();
            this.txtRemark    = new System.Windows.Forms.TextBox();
            this.btnApprove   = new System.Windows.Forms.Button();
            this.btnReject    = new System.Windows.Forms.Button();
            this.pnlFooter    = new System.Windows.Forms.Panel();
            this.lblStatus    = new System.Windows.Forms.Label();
            this.btnClose     = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // Header
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top; this.pnlHeader.Height = 60;
            this.pnlHeader.Controls.Add(this.lblSubTitle); this.pnlHeader.Controls.Add(this.lblTitle);
            this.lblTitle.Text = "อนุมัติคำขอเพิ่ม Quota พื้นที่ Folder";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White; this.lblTitle.Location = new System.Drawing.Point(12, 8); this.lblTitle.Size = new System.Drawing.Size(600, 26);
            this.lblSubTitle.Text = "ตรวจสอบและอนุมัติ / ปฏิเสธคำขอเพิ่ม quota พื้นที่ folder";
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255); this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 9);
            this.lblSubTitle.Location = new System.Drawing.Point(14, 36); this.lblSubTitle.Size = new System.Drawing.Size(500, 18);

            // Toolbar
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top; this.pnlToolbar.Height = 36;
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);

            this.chkPendingOnly.Text = "แสดงเฉพาะรอดำเนินการ"; this.chkPendingOnly.Checked = true;
            this.chkPendingOnly.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkPendingOnly.Width = 180;

            this.btnRefresh.Text = "รีเฟรช"; this.btnRefresh.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRefresh.Width = 80;
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(26, 58, 108); this.btnRefresh.ForeColor = System.Drawing.Color.White; this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pnlToolbar.Controls.Add(this.chkPendingOnly); this.pnlToolbar.Controls.Add(this.btnRefresh);

            // dgvRequests
            this.dgvRequests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRequests.AllowUserToAddRows = false; this.dgvRequests.ReadOnly = true;
            this.dgvRequests.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRequests.MultiSelect = false; this.dgvRequests.RowHeadersVisible = false;
            this.dgvRequests.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRequests.BackgroundColor = System.Drawing.Color.White;

            this.colID.HeaderText = "ID";         this.colID.Width = 40; this.colID.Visible = false;
            this.colOrg.HeaderText = "OrgUnit";    this.colOrg.FillWeight = 160;
            this.colCurrent.HeaderText = "Quota เดิม"; this.colCurrent.Width = 90;
            this.colRequested.HeaderText = "ขอใหม่"; this.colRequested.Width = 90;
            this.colReason.HeaderText = "เหตุผล";   this.colReason.FillWeight = 200;
            this.colRequestBy.HeaderText = "ผู้ขอ"; this.colRequestBy.Width = 110;
            this.colReqDate.HeaderText = "วันที่ขอ"; this.colReqDate.Width = 120;
            this.colStatus.HeaderText = "สถานะ";    this.colStatus.Width = 100;

            this.dgvRequests.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colID, this.colOrg, this.colCurrent, this.colRequested,
                this.colReason, this.colRequestBy, this.colReqDate, this.colStatus
            });

            // pnlAction (bottom action panel)
            this.pnlAction.Dock = System.Windows.Forms.DockStyle.Bottom; this.pnlAction.Height = 110;
            this.pnlAction.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAction.BackColor = System.Drawing.Color.FromArgb(252, 252, 252);

            this.lblDetailTitle.Text = "รายละเอียดที่เลือก:";
            this.lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.Location = new System.Drawing.Point(10, 8); this.lblDetailTitle.Size = new System.Drawing.Size(120, 18);

            this.lblDetail.Text = "-"; this.lblDetail.Location = new System.Drawing.Point(10, 26);
            this.lblDetail.Size = new System.Drawing.Size(500, 36); this.lblDetail.ForeColor = System.Drawing.Color.FromArgb(30, 60, 120);

            this.lblRemark.Text = "หมายเหตุ:"; this.lblRemark.Location = new System.Drawing.Point(10, 68); this.lblRemark.Size = new System.Drawing.Size(80, 20);
            this.txtRemark.Location = new System.Drawing.Point(92, 65); this.txtRemark.Size = new System.Drawing.Size(430, 24); this.txtRemark.MaxLength = 300;

            this.btnApprove.Text = "อนุมัติ"; this.btnApprove.Size = new System.Drawing.Size(100, 32);
            this.btnApprove.Location = new System.Drawing.Point(540, 30);
            this.btnApprove.BackColor = System.Drawing.Color.FromArgb(52, 120, 47); this.btnApprove.ForeColor = System.Drawing.Color.White; this.btnApprove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApprove.Enabled = false;

            this.btnReject.Text = "ปฏิเสธ"; this.btnReject.Size = new System.Drawing.Size(100, 32);
            this.btnReject.Location = new System.Drawing.Point(540, 68);
            this.btnReject.BackColor = System.Drawing.Color.FromArgb(180, 30, 30); this.btnReject.ForeColor = System.Drawing.Color.White; this.btnReject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReject.Enabled = false;

            this.pnlAction.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblDetailTitle, this.lblDetail, this.lblRemark, this.txtRemark,
                this.btnApprove, this.btnReject
            });

            // Footer
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom; this.pnlFooter.Height = 40;
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Left; this.lblStatus.Text = "";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnClose.Text = "ปิด"; this.btnClose.Width = 80;
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.BackColor = System.Drawing.Color.Gray; this.btnClose.ForeColor = System.Drawing.Color.White; this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pnlFooter.Controls.Add(this.lblStatus); this.pnlFooter.Controls.Add(this.btnClose);

            // Form
            this.Text = "อนุมัติคำขอเพิ่ม Quota Folder";
            this.Size = new System.Drawing.Size(860, 640);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            this.Controls.Add(this.dgvRequests);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlAction);
            this.Controls.Add(this.pnlFooter);

            this.dgvRequests.SelectionChanged      += new System.EventHandler(this.dgvRequests_SelectionChanged);
            this.chkPendingOnly.CheckedChanged     += new System.EventHandler(this.chkPendingOnly_CheckedChanged);
            this.btnApprove.Click += new System.EventHandler(this.btnApprove_Click);
            this.btnReject.Click  += new System.EventHandler(this.btnReject_Click);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            this.btnClose.Click   += new System.EventHandler(this.btnClose_Click);

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel    pnlHeader;
        private System.Windows.Forms.Label    lblTitle;
        private System.Windows.Forms.Label    lblSubTitle;
        private System.Windows.Forms.Panel    pnlToolbar;
        private System.Windows.Forms.CheckBox chkPendingOnly;
        private System.Windows.Forms.Button   btnRefresh;
        private System.Windows.Forms.DataGridView dgvRequests;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOrg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCurrent;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRequested;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReason;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRequestBy;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.Panel    pnlAction;
        private System.Windows.Forms.Label    lblDetailTitle;
        private System.Windows.Forms.Label    lblDetail;
        private System.Windows.Forms.Label    lblRemark;
        private System.Windows.Forms.TextBox  txtRemark;
        private System.Windows.Forms.Button   btnApprove;
        private System.Windows.Forms.Button   btnReject;
        private System.Windows.Forms.Panel    pnlFooter;
        private System.Windows.Forms.Label    lblStatus;
        private System.Windows.Forms.Button   btnClose;
    }
}
