using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.DMS
{
    partial class AuthorityByOrgReportForm
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
        private Panel pnlFilter;
        private Label lblDivision;
        private ComboBox cboDivision;
        private Label lblDepartment;
        private ComboBox cboDepartment;
        private Label lblSearch;
        private TextBox txtSearch;
        private CheckBox chkActiveOnly;
        private CheckBox chkNoAuthorityOnly;
        private Button btnRefresh;
        private Button btnExport;
        private Button btnClose;
        private DataGridView dgvUsers;
        private Panel pnlFooter;
        private Label lblSummary;
        private Label lblLegend;

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitle = new Label();
            this.pnlFilter = new Panel();
            this.lblDivision = new Label();
            this.cboDivision = new ComboBox();
            this.lblDepartment = new Label();
            this.cboDepartment = new ComboBox();
            this.lblSearch = new Label();
            this.txtSearch = new TextBox();
            this.chkActiveOnly = new CheckBox();
            this.chkNoAuthorityOnly = new CheckBox();
            this.btnRefresh = new Button();
            this.btnExport = new Button();
            this.btnClose = new Button();
            this.dgvUsers = new DataGridView();
            this.pnlFooter = new Panel();
            this.lblSummary = new Label();
            this.lblLegend = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // Form
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(1280, 720);
            this.MinimumSize = new Size(1000, 560);
            this.Text = "ตรวจสอบสิทธิ์ตามหน่วยงาน";
            this.Font = new Font("Tahoma", 9.5f);
            this.BackColor = Color.White;

            // Header
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 46;
            this.pnlHeader.BackColor = Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Padding = new Padding(16, 0, 0, 0);
            this.lblTitle.Text = "🛡 ตรวจสอบสิทธิ์ตามหน่วยงาน — สิทธิ์ตามระดับชั้นความลับของแต่ละคน";
            this.lblTitle.Font = new Font("Tahoma", 12f, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Filter bar
            this.pnlFilter.Dock = DockStyle.Top;
            this.pnlFilter.Height = 78;
            this.pnlFilter.BackColor = Color.FromArgb(240, 244, 249);

            this.lblDivision.Text = "ฝ่าย:";
            this.lblDivision.AutoSize = true;
            this.lblDivision.Location = new Point(12, 13);
            this.lblDivision.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            this.cboDivision.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboDivision.Location = new Point(60, 9);
            this.cboDivision.Width = 260;
            this.cboDivision.SelectedIndexChanged += new System.EventHandler(this.cboDivision_SelectedIndexChanged);

            this.lblDepartment.Text = "แผนก:";
            this.lblDepartment.AutoSize = true;
            this.lblDepartment.Location = new Point(336, 13);
            this.lblDepartment.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            this.cboDepartment.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboDepartment.Location = new Point(390, 9);
            this.cboDepartment.Width = 260;
            this.cboDepartment.SelectedIndexChanged += new System.EventHandler(this.Filter_Changed);

            this.lblSearch.Text = "🔍 ค้นหา:";
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new Point(12, 47);
            this.lblSearch.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            this.txtSearch.Location = new Point(80, 43);
            this.txtSearch.Width = 240;
            this.txtSearch.TextChanged += new System.EventHandler(this.Filter_Changed);

            this.chkActiveOnly.Text = "เฉพาะบัญชีที่ใช้งาน";
            this.chkActiveOnly.AutoSize = true;
            this.chkActiveOnly.Checked = true;
            this.chkActiveOnly.Location = new Point(336, 45);
            this.chkActiveOnly.CheckedChanged += new System.EventHandler(this.Filter_Changed);

            this.chkNoAuthorityOnly.Text = "เฉพาะคนที่ไม่มีสิทธิ์ตาม config";
            this.chkNoAuthorityOnly.AutoSize = true;
            this.chkNoAuthorityOnly.Location = new Point(490, 45);
            this.chkNoAuthorityOnly.CheckedChanged += new System.EventHandler(this.Filter_Changed);

            this.btnRefresh.Text = "🔄 รีเฟรช";
            this.btnRefresh.Location = new Point(680, 8);
            this.btnRefresh.Size = new Size(110, 28);
            this.btnRefresh.BackColor = Color.FromArgb(40, 120, 70);
            this.btnRefresh.ForeColor = Color.White;
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            this.btnExport.Text = "📤 Export CSV";
            this.btnExport.Location = new Point(796, 8);
            this.btnExport.Size = new Size(120, 28);
            this.btnExport.BackColor = Color.FromArgb(0, 105, 148);
            this.btnExport.ForeColor = Color.White;
            this.btnExport.FlatStyle = FlatStyle.Flat;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);

            this.btnClose.Text = "✕ ปิด";
            this.btnClose.Location = new Point(922, 8);
            this.btnClose.Size = new Size(90, 28);
            this.btnClose.BackColor = Color.FromArgb(100, 100, 100);
            this.btnClose.ForeColor = Color.White;
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.pnlFilter.Controls.AddRange(new Control[] {
                this.lblDivision, this.cboDivision, this.lblDepartment, this.cboDepartment,
                this.lblSearch, this.txtSearch, this.chkActiveOnly, this.chkNoAuthorityOnly,
                this.btnRefresh, this.btnExport, this.btnClose
            });

            // Grid
            this.dgvUsers.Dock = DockStyle.Fill;
            this.dgvUsers.BackgroundColor = Color.White;
            this.dgvUsers.BorderStyle = BorderStyle.None;
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AllowUserToDeleteRows = false;
            this.dgvUsers.ReadOnly = true;
            this.dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsers.MultiSelect = false;
            this.dgvUsers.RowHeadersVisible = false;
            this.dgvUsers.EnableHeadersVisualStyles = false;
            this.dgvUsers.ColumnHeadersHeight = 32;
            this.dgvUsers.RowTemplate.Height = 26;
            this.dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 56, 100);
            this.dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 247, 251);
            this.dgvUsers.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvUsers_CellFormatting);
            this.dgvUsers.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvUsers_CellDoubleClick);

            // Footer
            this.pnlFooter.Dock = DockStyle.Bottom;
            this.pnlFooter.Height = 48;
            this.pnlFooter.BackColor = Color.FromArgb(240, 244, 249);
            this.pnlFooter.Padding = new Padding(12, 4, 12, 4);

            this.lblSummary.Dock = DockStyle.Top;
            this.lblSummary.Height = 20;
            this.lblSummary.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.lblSummary.ForeColor = Color.FromArgb(31, 56, 100);

            this.lblLegend.Dock = DockStyle.Fill;
            this.lblLegend.ForeColor = Color.FromArgb(90, 90, 90);
            this.lblLegend.Text = "แก้ไข = แก้ไข/ลบ/แชร์   |   PC ดู (พื้นฐาน) = ทุกคนดูระดับทั่วไปได้เมื่อไม่มี config   |   Admin/Manager ผ่านทุก folder   |   ดับเบิลคลิกแถวเพื่อดูรายละเอียดรายคน (รวม ACL และการแชร์)";

            this.pnlFooter.Controls.Add(this.lblLegend);
            this.pnlFooter.Controls.Add(this.lblSummary);

            this.Controls.Add(this.dgvUsers);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlFilter);
            this.Controls.Add(this.pnlHeader);

            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlFilter.ResumeLayout(false);
            this.pnlFilter.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
