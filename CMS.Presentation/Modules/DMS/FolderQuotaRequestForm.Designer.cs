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
            this.pnlForm         = new System.Windows.Forms.Panel();
            this.lblOrgUnit      = new System.Windows.Forms.Label();
            this.cmbOrgUnit      = new System.Windows.Forms.ComboBox();
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

            // Header
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(26, 58, 108);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top; this.pnlHeader.Height = 60;
            this.pnlHeader.Controls.Add(this.lblSubTitle); this.pnlHeader.Controls.Add(this.lblTitle);
            this.lblTitle.Text = "ขอเพิ่มพื้นที่ Quota Folder";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White; this.lblTitle.Location = new System.Drawing.Point(12, 8); this.lblTitle.Size = new System.Drawing.Size(500, 26);
            this.lblSubTitle.Text = "ยื่นคำขออนุมัติเพิ่มขนาดพื้นที่ folder ต่อผู้ดูแลระบบ";
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255); this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 9);
            this.lblSubTitle.Location = new System.Drawing.Point(14, 36); this.lblSubTitle.Size = new System.Drawing.Size(500, 18);

            // pnlForm
            this.pnlForm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlForm.Dock = System.Windows.Forms.DockStyle.Top; this.pnlForm.Height = 160;
            this.pnlForm.Padding = new System.Windows.Forms.Padding(10);

            int ex = 10, lw = 130, cw = 260;
            this.lblOrgUnit.Text = "OrgUnit:"; this.lblOrgUnit.Location = new System.Drawing.Point(ex, 14); this.lblOrgUnit.Size = new System.Drawing.Size(lw, 20);
            this.cmbOrgUnit.Location = new System.Drawing.Point(ex + lw, 11); this.cmbOrgUnit.Size = new System.Drawing.Size(cw, 24);
            this.cmbOrgUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.lblCurrentQuota.Text = ""; this.lblCurrentQuota.Location = new System.Drawing.Point(ex + lw + cw + 10, 14);
            this.lblCurrentQuota.Size = new System.Drawing.Size(300, 20); this.lblCurrentQuota.ForeColor = System.Drawing.Color.FromArgb(0, 80, 160);

            this.lblRequestGB.Text = "ขอเพิ่มเป็น (GB):"; this.lblRequestGB.Location = new System.Drawing.Point(ex, 46); this.lblRequestGB.Size = new System.Drawing.Size(lw, 20);
            this.nudRequestGB.Location = new System.Drawing.Point(ex + lw, 43); this.nudRequestGB.Size = new System.Drawing.Size(90, 24);
            this.nudRequestGB.Minimum = 1; this.nudRequestGB.Maximum = 10000; this.nudRequestGB.Value = 40; this.nudRequestGB.DecimalPlaces = 1;
            this.lblGBUnit.Text = "GB"; this.lblGBUnit.Location = new System.Drawing.Point(ex + lw + 96, 46); this.lblGBUnit.Size = new System.Drawing.Size(30, 20);

            this.lblReason.Text = "เหตุผล:"; this.lblReason.Location = new System.Drawing.Point(ex, 78); this.lblReason.Size = new System.Drawing.Size(lw, 20);
            this.txtReason.Location = new System.Drawing.Point(ex + lw, 75); this.txtReason.Size = new System.Drawing.Size(cw, 56);
            this.txtReason.Multiline = true; this.txtReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical; this.txtReason.MaxLength = 500;

            this.btnSubmit.Text = "ส่งคำขอ"; this.btnSubmit.Size = new System.Drawing.Size(110, 32);
            this.btnSubmit.Location = new System.Drawing.Point(ex + lw + cw + 10, 75);
            this.btnSubmit.BackColor = System.Drawing.Color.FromArgb(26, 58, 108); this.btnSubmit.ForeColor = System.Drawing.Color.White; this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.pnlForm.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblOrgUnit, this.cmbOrgUnit, this.lblCurrentQuota,
                this.lblRequestGB, this.nudRequestGB, this.lblGBUnit,
                this.lblReason, this.txtReason, this.btnSubmit
            });

            // history label
            this.lblHistory.Text = "ประวัติคำขอของฉัน"; this.lblHistory.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            this.lblHistory.Dock = System.Windows.Forms.DockStyle.Top; this.lblHistory.Height = 24;
            this.lblHistory.Padding = new System.Windows.Forms.Padding(8, 4, 0, 0);

            // dgvMyRequests
            this.dgvMyRequests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMyRequests.AllowUserToAddRows = false; this.dgvMyRequests.ReadOnly = true;
            this.dgvMyRequests.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMyRequests.MultiSelect = false; this.dgvMyRequests.RowHeadersVisible = false;
            this.dgvMyRequests.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMyRequests.BackgroundColor = System.Drawing.Color.White;

            this.colReqID.HeaderText = "ID";             this.colReqID.Width = 40; this.colReqID.Visible = false;
            this.colReqOrg.HeaderText = "OrgUnit";        this.colReqOrg.FillWeight = 150;
            this.colReqCurrent.HeaderText = "Quota เดิม";   this.colReqCurrent.Width = 90;
            this.colReqRequested.HeaderText = "ขอใหม่";    this.colReqRequested.Width = 90;
            this.colReqReason.HeaderText = "เหตุผล";       this.colReqReason.FillWeight = 200;
            this.colReqStatus.HeaderText = "สถานะ";        this.colReqStatus.Width = 100;
            this.colReqDate.HeaderText = "วันที่ขอ";        this.colReqDate.Width = 120;

            this.dgvMyRequests.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colReqID, this.colReqOrg, this.colReqCurrent, this.colReqRequested,
                this.colReqReason, this.colReqStatus, this.colReqDate
            });

            // Footer
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom; this.pnlFooter.Height = 40;
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Left; this.lblStatus.Text = "";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnCancel.Text = "ยกเลิกคำขอ"; this.btnCancel.Size = new System.Drawing.Size(110, 28);
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(180, 80, 0); this.btnCancel.ForeColor = System.Drawing.Color.White; this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Text = "ปิด"; this.btnClose.Size = new System.Drawing.Size(80, 28);
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.BackColor = System.Drawing.Color.Gray; this.btnClose.ForeColor = System.Drawing.Color.White; this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pnlFooter.Controls.Add(this.lblStatus); this.pnlFooter.Controls.Add(this.btnCancel); this.pnlFooter.Controls.Add(this.btnClose);

            // Form
            this.Text = "ขอเพิ่มพื้นที่ Quota Folder";
            this.Size = new System.Drawing.Size(820, 580);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            this.Controls.Add(this.dgvMyRequests);
            this.Controls.Add(this.lblHistory);
            this.Controls.Add(this.pnlForm);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);

            this.cmbOrgUnit.SelectedIndexChanged += new System.EventHandler(this.cmbOrgUnit_SelectedIndexChanged);
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnClose.Click  += new System.EventHandler(this.btnClose_Click);

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel    pnlHeader;
        private System.Windows.Forms.Label    lblTitle;
        private System.Windows.Forms.Label    lblSubTitle;
        private System.Windows.Forms.Panel    pnlForm;
        private System.Windows.Forms.Label    lblOrgUnit;
        private System.Windows.Forms.ComboBox cmbOrgUnit;
        private System.Windows.Forms.Label    lblCurrentQuota;
        private System.Windows.Forms.Label    lblRequestGB;
        private System.Windows.Forms.NumericUpDown nudRequestGB;
        private System.Windows.Forms.Label    lblGBUnit;
        private System.Windows.Forms.Label    lblReason;
        private System.Windows.Forms.TextBox  txtReason;
        private System.Windows.Forms.Button   btnSubmit;
        private System.Windows.Forms.Label    lblHistory;
        private System.Windows.Forms.DataGridView dgvMyRequests;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqOrg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqCurrent;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqRequested;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqReason;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReqDate;
        private System.Windows.Forms.Panel    pnlFooter;
        private System.Windows.Forms.Label    lblStatus;
        private System.Windows.Forms.Button   btnCancel;
        private System.Windows.Forms.Button   btnClose;
    }
}
