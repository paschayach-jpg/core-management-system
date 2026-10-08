using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.DMS
{
    partial class MyDownloadRequestsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private Panel pnlHeader;
        private Label lblTitle;
        private Panel pnlToolbar;
        private Label lblStatusFilter;
        private ComboBox cboStatus;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnRefresh;
        private DataGridView dgvRequests;
        private DataGridViewTextBoxColumn colRequestId;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colFileName;
        private DataGridViewTextBoxColumn colFolder;
        private DataGridViewTextBoxColumn colRequestedDate;
        private DataGridViewTextBoxColumn colApprover;
        private DataGridViewTextBoxColumn colReason;
        private DataGridViewTextBoxColumn colResolved;
        private DataGridViewTextBoxColumn colResolveRemark;
        private DataGridViewTextBoxColumn colExpiry;
        private Panel pnlButtons;
        private Button btnClose;
        private Label lblStatus;

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitle = new Label();
            this.pnlToolbar = new Panel();
            this.lblStatusFilter = new Label();
            this.cboStatus = new ComboBox();
            this.lblSearch = new Label();
            this.txtSearch = new TextBox();
            this.btnRefresh = new Button();
            this.dgvRequests = new DataGridView();
            this.colRequestId = new DataGridViewTextBoxColumn();
            this.colStatus = new DataGridViewTextBoxColumn();
            this.colFileName = new DataGridViewTextBoxColumn();
            this.colFolder = new DataGridViewTextBoxColumn();
            this.colRequestedDate = new DataGridViewTextBoxColumn();
            this.colApprover = new DataGridViewTextBoxColumn();
            this.colReason = new DataGridViewTextBoxColumn();
            this.colResolved = new DataGridViewTextBoxColumn();
            this.colResolveRemark = new DataGridViewTextBoxColumn();
            this.colExpiry = new DataGridViewTextBoxColumn();
            this.pnlButtons = new Panel();
            this.btnClose = new Button();
            this.lblStatus = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            // Form
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(1200, 640);
            this.MinimumSize = new Size(900, 480);
            this.Text = "คำขอดาวน์โหลดของฉัน";
            this.Font = new Font("Tahoma", 9.5f);
            this.BackColor = Color.White;

            // Header
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 46;
            this.pnlHeader.BackColor = Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Padding = new Padding(16, 0, 0, 0);
            this.lblTitle.Text = "⬇ คำขอดาวน์โหลดของฉัน — ติดตามสถานะการขออนุมัติดาวน์โหลดไฟล์ (ดูอย่างเดียว)";
            this.lblTitle.Font = new Font("Tahoma", 12f, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Toolbar
            this.pnlToolbar.Dock = DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.BackColor = Color.FromArgb(240, 244, 249);

            this.lblStatusFilter.Text = "สถานะ:";
            this.lblStatusFilter.AutoSize = true;
            this.lblStatusFilter.Location = new Point(12, 13);
            this.lblStatusFilter.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            this.cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboStatus.Location = new Point(70, 9);
            this.cboStatus.Width = 200;
            this.cboStatus.Items.AddRange(new object[] {
                "ทั้งหมด", "รออนุมัติ", "อนุมัติแล้ว (ยังไม่ดาวน์โหลด)", "ไม่อนุมัติ",
                "ดาวน์โหลดแล้ว", "ยกเลิก / หมดอายุ"
            });
            this.cboStatus.SelectedIndexChanged += new System.EventHandler(this.Filter_Changed);

            this.lblSearch.Text = "🔍 ค้นหาไฟล์:";
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new Point(290, 13);
            this.lblSearch.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            this.txtSearch.Location = new Point(385, 9);
            this.txtSearch.Width = 260;
            this.txtSearch.TextChanged += new System.EventHandler(this.Filter_Changed);

            this.btnRefresh.Text = "🔄 รีเฟรช";
            this.btnRefresh.Location = new Point(660, 7);
            this.btnRefresh.Size = new Size(100, 28);
            this.btnRefresh.BackColor = Color.FromArgb(40, 120, 70);
            this.btnRefresh.ForeColor = Color.White;
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            this.pnlToolbar.Controls.AddRange(new Control[] {
                this.lblStatusFilter, this.cboStatus, this.lblSearch, this.txtSearch, this.btnRefresh
            });

            // Grid
            this.colRequestId.Name = "colRequestId";
            this.colRequestId.HeaderText = "ID";
            this.colRequestId.Width = 55;
            this.colStatus.Name = "colStatus";
            this.colStatus.HeaderText = "สถานะ";
            this.colStatus.Width = 210;
            this.colFileName.Name = "colFileName";
            this.colFileName.HeaderText = "ชื่อไฟล์";
            this.colFileName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colFileName.FillWeight = 35;
            this.colFolder.Name = "colFolder";
            this.colFolder.HeaderText = "โฟลเดอร์";
            this.colFolder.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colFolder.FillWeight = 20;
            this.colRequestedDate.Name = "colRequestedDate";
            this.colRequestedDate.HeaderText = "วันที่ขอ";
            this.colRequestedDate.Width = 120;
            this.colApprover.Name = "colApprover";
            this.colApprover.HeaderText = "ผู้อนุมัติ";
            this.colApprover.Width = 170;
            this.colReason.Name = "colReason";
            this.colReason.HeaderText = "เหตุผลที่ขอ";
            this.colReason.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colReason.FillWeight = 25;
            this.colResolved.Name = "colResolved";
            this.colResolved.HeaderText = "ดำเนินการโดย / วันที่";
            this.colResolved.Width = 190;
            this.colResolveRemark.Name = "colResolveRemark";
            this.colResolveRemark.HeaderText = "หมายเหตุผู้อนุมัติ";
            this.colResolveRemark.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colResolveRemark.FillWeight = 20;
            this.colExpiry.Name = "colExpiry";
            this.colExpiry.HeaderText = "หมดอายุ";
            this.colExpiry.Width = 95;

            this.dgvRequests.Columns.AddRange(new DataGridViewColumn[] {
                this.colRequestId, this.colStatus, this.colFileName, this.colFolder, this.colRequestedDate,
                this.colApprover, this.colReason, this.colResolved, this.colResolveRemark, this.colExpiry
            });
            this.dgvRequests.Dock = DockStyle.Fill;
            this.dgvRequests.BackgroundColor = Color.White;
            this.dgvRequests.BorderStyle = BorderStyle.None;
            this.dgvRequests.AllowUserToAddRows = false;
            this.dgvRequests.AllowUserToDeleteRows = false;
            this.dgvRequests.ReadOnly = true;
            this.dgvRequests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvRequests.MultiSelect = false;
            this.dgvRequests.RowHeadersVisible = false;
            this.dgvRequests.EnableHeadersVisualStyles = false;
            this.dgvRequests.ColumnHeadersHeight = 32;
            this.dgvRequests.RowTemplate.Height = 28;
            this.dgvRequests.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 56, 100);
            this.dgvRequests.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvRequests.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.dgvRequests.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvRequests_CellFormatting);

            // Buttons
            this.pnlButtons.Dock = DockStyle.Bottom;
            this.pnlButtons.Height = 50;
            this.pnlButtons.BackColor = Color.FromArgb(240, 244, 249);

            this.btnClose.Text = "✕ ปิด";
            this.btnClose.Location = new Point(12, 11);
            this.btnClose.Size = new Size(90, 28);
            this.btnClose.BackColor = Color.FromArgb(100, 100, 100);
            this.btnClose.ForeColor = Color.White;
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.lblStatus.AutoSize = false;
            this.lblStatus.Location = new Point(116, 15);
            this.lblStatus.Size = new Size(1066, 22);
            this.lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.lblStatus.ForeColor = Color.FromArgb(90, 90, 90);

            this.pnlButtons.Controls.AddRange(new Control[] {
                this.btnClose, this.lblStatus
            });

            this.Controls.Add(this.dgvRequests);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);

            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
