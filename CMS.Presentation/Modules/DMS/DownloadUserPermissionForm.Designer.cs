using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.DMS
{
    partial class DownloadUserPermissionForm
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
        private Label lblPickUser;
        private ComboBox cboUser;
        private Button btnAdd;
        private Label lblFilter;
        private TextBox txtFilter;
        private DataGridView dgvPermissions;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colAdUser;
        private DataGridViewTextBoxColumn colPosition;
        private DataGridViewTextBoxColumn colLevel;
        private DataGridViewTextBoxColumn colDepartment;
        private DataGridViewCheckBoxColumn colHC;
        private DataGridViewCheckBoxColumn colSC;
        private DataGridViewCheckBoxColumn colNC;
        private DataGridViewCheckBoxColumn colPC;
        private Panel pnlButtons;
        private Button btnRemove;
        private Button btnSave;
        private Button btnRefresh;
        private Button btnClose;
        private Label lblStatus;

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitle = new Label();
            this.pnlToolbar = new Panel();
            this.lblPickUser = new Label();
            this.cboUser = new ComboBox();
            this.btnAdd = new Button();
            this.lblFilter = new Label();
            this.txtFilter = new TextBox();
            this.dgvPermissions = new DataGridView();
            this.colName = new DataGridViewTextBoxColumn();
            this.colAdUser = new DataGridViewTextBoxColumn();
            this.colPosition = new DataGridViewTextBoxColumn();
            this.colLevel = new DataGridViewTextBoxColumn();
            this.colDepartment = new DataGridViewTextBoxColumn();
            this.colHC = new DataGridViewCheckBoxColumn();
            this.colSC = new DataGridViewCheckBoxColumn();
            this.colNC = new DataGridViewCheckBoxColumn();
            this.colPC = new DataGridViewCheckBoxColumn();
            this.pnlButtons = new Panel();
            this.btnRemove = new Button();
            this.btnSave = new Button();
            this.btnRefresh = new Button();
            this.btnClose = new Button();
            this.lblStatus = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.dgvPermissions)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            // Form
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(1100, 640);
            this.MinimumSize = new Size(900, 480);
            this.Text = "สิทธิ์ดาวน์โหลดรายคน";
            this.Font = new Font("Tahoma", 9.5f);
            this.BackColor = Color.White;

            // Header
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 46;
            this.pnlHeader.BackColor = Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Padding = new Padding(16, 0, 0, 0);
            this.lblTitle.Text = "⬇ สิทธิ์ดาวน์โหลดรายคน — ติ๊กระดับชั้นความลับที่ดาวน์โหลดได้ทันทีโดยไม่ต้องขออนุมัติ";
            this.lblTitle.Font = new Font("Tahoma", 12f, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Toolbar
            this.pnlToolbar.Dock = DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.BackColor = Color.FromArgb(240, 244, 249);

            this.lblPickUser.Text = "เพิ่มผู้ใช้:";
            this.lblPickUser.AutoSize = true;
            this.lblPickUser.Location = new Point(12, 13);
            this.lblPickUser.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            // พิมพ์ชื่อ / AD username แล้วกรองแบบ "มีคำนี้อยู่"
            this.cboUser.DropDownStyle = ComboBoxStyle.DropDown;
            this.cboUser.AutoCompleteMode = AutoCompleteMode.None;
            this.cboUser.MaxDropDownItems = 15;
            this.cboUser.Location = new Point(90, 9);
            this.cboUser.Width = 380;
            this.cboUser.TextUpdate += new System.EventHandler(this.cboUser_TextUpdate);
            this.cboUser.KeyDown += new KeyEventHandler(this.cboUser_KeyDown);

            this.btnAdd.Text = "➕ เพิ่ม";
            this.btnAdd.Location = new Point(478, 7);
            this.btnAdd.Size = new Size(90, 28);
            this.btnAdd.BackColor = Color.FromArgb(0, 105, 148);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            this.lblFilter.Text = "🔍 กรองรายการ:";
            this.lblFilter.AutoSize = true;
            this.lblFilter.Location = new Point(600, 13);
            this.lblFilter.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            this.txtFilter.Location = new Point(710, 9);
            this.txtFilter.Width = 220;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);

            this.pnlToolbar.Controls.AddRange(new Control[] {
                this.lblPickUser, this.cboUser, this.btnAdd, this.lblFilter, this.txtFilter
            });

            // Grid
            this.colName.Name = "colName";
            this.colName.HeaderText = "ชื่อ-นามสกุล";
            this.colName.ReadOnly = true;
            this.colName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colName.FillWeight = 30;
            this.colAdUser.Name = "colAdUser";
            this.colAdUser.HeaderText = "AD Username";
            this.colAdUser.ReadOnly = true;
            this.colAdUser.Width = 130;
            this.colPosition.Name = "colPosition";
            this.colPosition.HeaderText = "ตำแหน่ง";
            this.colPosition.ReadOnly = true;
            this.colPosition.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colPosition.FillWeight = 25;
            this.colLevel.Name = "colLevel";
            this.colLevel.HeaderText = "Level";
            this.colLevel.ReadOnly = true;
            this.colLevel.Width = 55;
            this.colDepartment.Name = "colDepartment";
            this.colDepartment.HeaderText = "แผนก";
            this.colDepartment.ReadOnly = true;
            this.colDepartment.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.colDepartment.FillWeight = 25;
            this.colHC.Name = "colHC";
            this.colHC.HeaderText = "HC";
            this.colHC.Width = 55;
            this.colSC.Name = "colSC";
            this.colSC.HeaderText = "SC";
            this.colSC.Width = 55;
            this.colNC.Name = "colNC";
            this.colNC.HeaderText = "NC";
            this.colNC.Width = 55;
            this.colPC.Name = "colPC";
            this.colPC.HeaderText = "PC";
            this.colPC.Width = 55;

            this.dgvPermissions.Columns.AddRange(new DataGridViewColumn[] {
                this.colName, this.colAdUser, this.colPosition, this.colLevel, this.colDepartment,
                this.colHC, this.colSC, this.colNC, this.colPC
            });
            this.dgvPermissions.Dock = DockStyle.Fill;
            this.dgvPermissions.BackgroundColor = Color.White;
            this.dgvPermissions.BorderStyle = BorderStyle.None;
            this.dgvPermissions.AllowUserToAddRows = false;
            this.dgvPermissions.AllowUserToDeleteRows = false;
            this.dgvPermissions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvPermissions.MultiSelect = false;
            this.dgvPermissions.RowHeadersVisible = false;
            this.dgvPermissions.EnableHeadersVisualStyles = false;
            this.dgvPermissions.ColumnHeadersHeight = 32;
            this.dgvPermissions.RowTemplate.Height = 26;
            this.dgvPermissions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 56, 100);
            this.dgvPermissions.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvPermissions.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.dgvPermissions.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 247, 251);
            this.dgvPermissions.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvPermissions_CurrentCellDirtyStateChanged);
            this.dgvPermissions.CellValueChanged += new DataGridViewCellEventHandler(this.dgvPermissions_CellValueChanged);

            // Buttons
            this.pnlButtons.Dock = DockStyle.Bottom;
            this.pnlButtons.Height = 50;
            this.pnlButtons.BackColor = Color.FromArgb(240, 244, 249);

            this.btnRemove.Text = "➖ ลบผู้ใช้ที่เลือก";
            this.btnRemove.Location = new Point(12, 11);
            this.btnRemove.Size = new Size(140, 28);
            this.btnRemove.BackColor = Color.FromArgb(192, 57, 43);
            this.btnRemove.ForeColor = Color.White;
            this.btnRemove.FlatStyle = FlatStyle.Flat;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);

            this.btnSave.Text = "💾 บันทึก";
            this.btnSave.Location = new Point(160, 11);
            this.btnSave.Size = new Size(110, 28);
            this.btnSave.BackColor = Color.FromArgb(40, 120, 70);
            this.btnSave.ForeColor = Color.White;
            this.btnSave.FlatStyle = FlatStyle.Flat;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnRefresh.Text = "🔄 รีเฟรช";
            this.btnRefresh.Location = new Point(278, 11);
            this.btnRefresh.Size = new Size(100, 28);
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            this.btnClose.Text = "✕ ปิด";
            this.btnClose.Location = new Point(386, 11);
            this.btnClose.Size = new Size(90, 28);
            this.btnClose.BackColor = Color.FromArgb(100, 100, 100);
            this.btnClose.ForeColor = Color.White;
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.lblStatus.AutoSize = false;
            this.lblStatus.Location = new Point(490, 15);
            this.lblStatus.Size = new Size(600, 22);
            this.lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.lblStatus.ForeColor = Color.FromArgb(90, 90, 90);
            this.lblStatus.Text = "Admin ดาวน์โหลดได้ทุกระดับเสมอ  |  สิทธิ์ตาม Level/ตำแหน่ง ตั้งที่ \"จัดการสิทธิ์ Authority\" (คอลัมน์ ⬇)";

            this.pnlButtons.Controls.AddRange(new Control[] {
                this.btnRemove, this.btnSave, this.btnRefresh, this.btnClose, this.lblStatus
            });

            this.Controls.Add(this.dgvPermissions);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);

            ((System.ComponentModel.ISupportInitialize)(this.dgvPermissions)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
