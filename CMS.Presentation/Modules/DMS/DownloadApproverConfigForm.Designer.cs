using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.DMS
{
    partial class DownloadApproverConfigForm
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
        private Label lblSearch;
        private TextBox txtSearch;
        private CheckBox chkConfiguredOnly;
        private Button btnRefresh;
        private Button btnClose;
        private SplitContainer splitMain;
        private DataGridView dgvOrgs;
        private DataGridViewTextBoxColumn colOrgType;
        private DataGridViewTextBoxColumn colOrgCode;
        private DataGridViewTextBoxColumn colOrgName;
        private DataGridViewTextBoxColumn colOrgApprovers;
        private Panel pnlEditorHeader;
        private Label lblSelectedOrg;
        private Panel pnlPicker;
        private Label lblPickUser;
        private ComboBox cboUser;
        private Button btnAdd;
        private DataGridView dgvApprovers;
        private DataGridViewTextBoxColumn colApOrder;
        private DataGridViewTextBoxColumn colApName;
        private DataGridViewTextBoxColumn colApPosition;
        private DataGridViewTextBoxColumn colApLevel;
        private DataGridViewTextBoxColumn colApDepartment;
        private Panel pnlEditorButtons;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnRemove;
        private Button btnSave;
        private Panel pnlFooter;
        private Label lblStatus;

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitle = new Label();
            this.pnlFilter = new Panel();
            this.lblSearch = new Label();
            this.txtSearch = new TextBox();
            this.chkConfiguredOnly = new CheckBox();
            this.btnRefresh = new Button();
            this.btnClose = new Button();
            this.splitMain = new SplitContainer();
            this.dgvOrgs = new DataGridView();
            this.colOrgType = new DataGridViewTextBoxColumn();
            this.colOrgCode = new DataGridViewTextBoxColumn();
            this.colOrgName = new DataGridViewTextBoxColumn();
            this.colOrgApprovers = new DataGridViewTextBoxColumn();
            this.pnlEditorHeader = new Panel();
            this.lblSelectedOrg = new Label();
            this.pnlPicker = new Panel();
            this.lblPickUser = new Label();
            this.cboUser = new ComboBox();
            this.btnAdd = new Button();
            this.dgvApprovers = new DataGridView();
            this.colApOrder = new DataGridViewTextBoxColumn();
            this.colApName = new DataGridViewTextBoxColumn();
            this.colApPosition = new DataGridViewTextBoxColumn();
            this.colApLevel = new DataGridViewTextBoxColumn();
            this.colApDepartment = new DataGridViewTextBoxColumn();
            this.pnlEditorButtons = new Panel();
            this.btnMoveUp = new Button();
            this.btnMoveDown = new Button();
            this.btnRemove = new Button();
            this.btnSave = new Button();
            this.pnlFooter = new Panel();
            this.lblStatus = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrgs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvApprovers)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            this.pnlEditorHeader.SuspendLayout();
            this.pnlPicker.SuspendLayout();
            this.pnlEditorButtons.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // Form
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(1280, 720);
            this.MinimumSize = new Size(1000, 560);
            this.Text = "ตั้งค่าผู้อนุมัติดาวน์โหลดไฟล์";
            this.Font = new Font("Tahoma", 9.5f);
            this.BackColor = Color.White;

            // Header
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 46;
            this.pnlHeader.BackColor = Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Padding = new Padding(16, 0, 0, 0);
            this.lblTitle.Text = "⬇ ตั้งค่าผู้อนุมัติดาวน์โหลดไฟล์ — กำหนดรายชื่อผู้อนุมัติตามแผนก / ฝ่าย";
            this.lblTitle.Font = new Font("Tahoma", 12f, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Filter bar
            this.pnlFilter.Dock = DockStyle.Top;
            this.pnlFilter.Height = 44;
            this.pnlFilter.BackColor = Color.FromArgb(240, 244, 249);

            this.lblSearch.Text = "🔍 ค้นหา:";
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new Point(12, 13);
            this.lblSearch.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            this.txtSearch.Location = new Point(80, 9);
            this.txtSearch.Width = 260;
            this.txtSearch.TextChanged += new System.EventHandler(this.Filter_Changed);

            this.chkConfiguredOnly.Text = "เฉพาะที่ตั้งผู้อนุมัติแล้ว";
            this.chkConfiguredOnly.AutoSize = true;
            this.chkConfiguredOnly.Location = new Point(356, 11);
            this.chkConfiguredOnly.CheckedChanged += new System.EventHandler(this.Filter_Changed);

            this.btnRefresh.Text = "🔄 รีเฟรช";
            this.btnRefresh.Location = new Point(560, 7);
            this.btnRefresh.Size = new Size(110, 28);
            this.btnRefresh.BackColor = Color.FromArgb(40, 120, 70);
            this.btnRefresh.ForeColor = Color.White;
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            this.btnClose.Text = "✕ ปิด";
            this.btnClose.Location = new Point(676, 7);
            this.btnClose.Size = new Size(90, 28);
            this.btnClose.BackColor = Color.FromArgb(100, 100, 100);
            this.btnClose.ForeColor = Color.White;
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.pnlFilter.Controls.AddRange(new Control[] {
                this.lblSearch, this.txtSearch, this.chkConfiguredOnly, this.btnRefresh, this.btnClose
            });

            // Split
            this.splitMain.Dock = DockStyle.Fill;
            this.splitMain.SplitterDistance = 700;
            this.splitMain.SplitterWidth = 6;
            this.splitMain.SizeChanged += new System.EventHandler(this.splitMain_SizeChanged);
            this.splitMain.Panel1.Controls.Add(this.dgvOrgs);
            this.splitMain.Panel2.Controls.Add(this.dgvApprovers);
            this.splitMain.Panel2.Controls.Add(this.pnlEditorButtons);
            this.splitMain.Panel2.Controls.Add(this.pnlPicker);
            this.splitMain.Panel2.Controls.Add(this.pnlEditorHeader);

            // Org grid (left)
            this.colOrgType.Name = "colOrgType";
            this.colOrgType.HeaderText = "ประเภท";
            this.colOrgType.Width = 70;
            this.colOrgCode.Name = "colOrgCode";
            this.colOrgCode.HeaderText = "รหัส";
            this.colOrgCode.Width = 80;
            this.colOrgName.Name = "colOrgName";
            this.colOrgName.HeaderText = "ชื่อฝ่าย / แผนก";
            this.colOrgName.Width = 230;
            this.colOrgApprovers.Name = "colOrgApprovers";
            this.colOrgApprovers.HeaderText = "ผู้อนุมัติดาวน์โหลด";
            this.colOrgApprovers.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            this.dgvOrgs.Columns.AddRange(new DataGridViewColumn[] {
                this.colOrgType, this.colOrgCode, this.colOrgName, this.colOrgApprovers
            });
            this.dgvOrgs.Dock = DockStyle.Fill;
            this.dgvOrgs.BackgroundColor = Color.White;
            this.dgvOrgs.BorderStyle = BorderStyle.None;
            this.dgvOrgs.AllowUserToAddRows = false;
            this.dgvOrgs.AllowUserToDeleteRows = false;
            this.dgvOrgs.ReadOnly = true;
            this.dgvOrgs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvOrgs.MultiSelect = false;
            this.dgvOrgs.RowHeadersVisible = false;
            this.dgvOrgs.EnableHeadersVisualStyles = false;
            this.dgvOrgs.ColumnHeadersHeight = 32;
            this.dgvOrgs.RowTemplate.Height = 26;
            this.dgvOrgs.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 56, 100);
            this.dgvOrgs.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvOrgs.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.dgvOrgs.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 247, 251);
            this.dgvOrgs.SelectionChanged += new System.EventHandler(this.dgvOrgs_SelectionChanged);
            this.dgvOrgs.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvOrgs_CellFormatting);

            // Editor header (right)
            this.pnlEditorHeader.Dock = DockStyle.Top;
            this.pnlEditorHeader.Height = 40;
            this.pnlEditorHeader.BackColor = Color.FromArgb(222, 236, 255);
            this.pnlEditorHeader.Padding = new Padding(10, 0, 10, 0);
            this.pnlEditorHeader.Controls.Add(this.lblSelectedOrg);

            this.lblSelectedOrg.Dock = DockStyle.Fill;
            this.lblSelectedOrg.Font = new Font("Tahoma", 10.5f, FontStyle.Bold);
            this.lblSelectedOrg.ForeColor = Color.FromArgb(31, 56, 100);
            this.lblSelectedOrg.TextAlign = ContentAlignment.MiddleLeft;
            this.lblSelectedOrg.Text = "เลือกฝ่าย / แผนก ทางซ้าย";

            // User picker
            this.pnlPicker.Dock = DockStyle.Top;
            this.pnlPicker.Height = 44;
            this.pnlPicker.Padding = new Padding(10, 8, 10, 6);

            this.lblPickUser.Text = "เพิ่มผู้อนุมัติ:";
            this.lblPickUser.AutoSize = true;
            this.lblPickUser.Location = new Point(10, 13);
            this.lblPickUser.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            // พิมพ์ชื่อ / AD username แล้วกรองรายการแบบ "มีคำนี้อยู่" (ไม่ใช้ AutoComplete ของ ComboBox ซึ่งจับเฉพาะคำขึ้นต้น)
            this.cboUser.DropDownStyle = ComboBoxStyle.DropDown;
            this.cboUser.AutoCompleteMode = AutoCompleteMode.None;
            this.cboUser.MaxDropDownItems = 15;
            this.cboUser.Location = new Point(110, 9);
            this.cboUser.Width = 330;
            this.cboUser.TextUpdate += new System.EventHandler(this.cboUser_TextUpdate);
            this.cboUser.KeyDown += new KeyEventHandler(this.cboUser_KeyDown);

            this.btnAdd.Text = "➕ เพิ่ม";
            this.btnAdd.Location = new Point(448, 7);
            this.btnAdd.Size = new Size(90, 28);
            this.btnAdd.BackColor = Color.FromArgb(0, 105, 148);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            this.pnlPicker.Controls.AddRange(new Control[] { this.lblPickUser, this.cboUser, this.btnAdd });

            // Approver grid
            this.colApOrder.Name = "colApOrder";
            this.colApOrder.HeaderText = "ลำดับ";
            this.colApOrder.Width = 50;
            this.colApName.Name = "colApName";
            this.colApName.HeaderText = "ชื่อ-นามสกุล";
            this.colApName.Width = 190;
            this.colApPosition.Name = "colApPosition";
            this.colApPosition.HeaderText = "ตำแหน่ง";
            this.colApPosition.Width = 150;
            this.colApLevel.Name = "colApLevel";
            this.colApLevel.HeaderText = "Level";
            this.colApLevel.Width = 55;
            this.colApDepartment.Name = "colApDepartment";
            this.colApDepartment.HeaderText = "แผนก";
            this.colApDepartment.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            this.dgvApprovers.Columns.AddRange(new DataGridViewColumn[] {
                this.colApOrder, this.colApName, this.colApPosition, this.colApLevel, this.colApDepartment
            });
            this.dgvApprovers.Dock = DockStyle.Fill;
            this.dgvApprovers.BackgroundColor = Color.White;
            this.dgvApprovers.BorderStyle = BorderStyle.None;
            this.dgvApprovers.AllowUserToAddRows = false;
            this.dgvApprovers.AllowUserToDeleteRows = false;
            this.dgvApprovers.ReadOnly = true;
            this.dgvApprovers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvApprovers.MultiSelect = false;
            this.dgvApprovers.RowHeadersVisible = false;
            this.dgvApprovers.EnableHeadersVisualStyles = false;
            this.dgvApprovers.ColumnHeadersHeight = 32;
            this.dgvApprovers.RowTemplate.Height = 26;
            this.dgvApprovers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 56, 100);
            this.dgvApprovers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvApprovers.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.dgvApprovers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 247, 251);

            // Editor buttons
            this.pnlEditorButtons.Dock = DockStyle.Bottom;
            this.pnlEditorButtons.Height = 46;
            this.pnlEditorButtons.BackColor = Color.FromArgb(240, 244, 249);

            this.btnMoveUp.Text = "⬆ ขึ้น";
            this.btnMoveUp.Location = new Point(10, 9);
            this.btnMoveUp.Size = new Size(80, 28);
            this.btnMoveUp.FlatStyle = FlatStyle.Flat;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);

            this.btnMoveDown.Text = "⬇ ลง";
            this.btnMoveDown.Location = new Point(96, 9);
            this.btnMoveDown.Size = new Size(80, 28);
            this.btnMoveDown.FlatStyle = FlatStyle.Flat;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);

            this.btnRemove.Text = "➖ ลบ";
            this.btnRemove.Location = new Point(182, 9);
            this.btnRemove.Size = new Size(80, 28);
            this.btnRemove.BackColor = Color.FromArgb(192, 57, 43);
            this.btnRemove.ForeColor = Color.White;
            this.btnRemove.FlatStyle = FlatStyle.Flat;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);

            this.btnSave.Text = "💾 บันทึก";
            this.btnSave.Location = new Point(300, 9);
            this.btnSave.Size = new Size(110, 28);
            this.btnSave.BackColor = Color.FromArgb(40, 120, 70);
            this.btnSave.ForeColor = Color.White;
            this.btnSave.FlatStyle = FlatStyle.Flat;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.pnlEditorButtons.Controls.AddRange(new Control[] {
                this.btnMoveUp, this.btnMoveDown, this.btnRemove, this.btnSave
            });

            // Footer
            this.pnlFooter.Dock = DockStyle.Bottom;
            this.pnlFooter.Height = 28;
            this.pnlFooter.BackColor = Color.FromArgb(240, 244, 249);
            this.pnlFooter.Padding = new Padding(12, 4, 12, 4);
            this.pnlFooter.Controls.Add(this.lblStatus);

            this.lblStatus.Dock = DockStyle.Fill;
            this.lblStatus.ForeColor = Color.FromArgb(90, 90, 90);
            this.lblStatus.Text = "ไฟล์ในโฟลเดอร์ของแผนก → แสดงผู้อนุมัติของแผนก ต่อด้วยของฝ่าย  |  ไม่ได้ตั้ง → ใช้คนที่ Level สูงกว่าผู้ขอ + Admin";

            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlFilter);
            this.Controls.Add(this.pnlHeader);

            ((System.ComponentModel.ISupportInitialize)(this.dgvOrgs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvApprovers)).EndInit();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlFilter.ResumeLayout(false);
            this.pnlFilter.PerformLayout();
            this.pnlEditorHeader.ResumeLayout(false);
            this.pnlPicker.ResumeLayout(false);
            this.pnlPicker.PerformLayout();
            this.pnlEditorButtons.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
