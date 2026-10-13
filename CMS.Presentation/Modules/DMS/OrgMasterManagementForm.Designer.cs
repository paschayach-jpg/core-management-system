using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.DMS
{
    partial class OrgMasterManagementForm
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
        private Button btnImportExcel;
        private Button btnDownloadTemplate;
        private Button btnRefresh;
        private Button btnClose;

        private TabControl tabMaster;
        private TabPage tabDivision;
        private TabPage tabDepartment;
        private TabPage tabPosition;
        private TabPage tabLevel;
        private TabPage tabMapping;

        // Tab 1: Division
        private Panel pnlDivBar;
        private Label lblSearchDiv;
        private TextBox txtSearchDiv;
        private Button btnAddDiv;
        private Button btnEditDiv;
        private Button btnDeleteDiv;
        private DataGridView dgvDivision;

        // Tab 2: Department
        private Panel pnlDeptBar;
        private Label lblSearchDept;
        private TextBox txtSearchDept;
        private Label lblFilterDivOnDept;
        private ComboBox cboFilterDivOnDept;
        private Button btnAddDept;
        private Button btnEditDept;
        private Button btnDeleteDept;
        private DataGridView dgvDepartment;

        // Tab 3: Position
        private Panel pnlPosBar;
        private Label lblSearchPos;
        private TextBox txtSearchPos;
        private Button btnAddPos;
        private Button btnEditPos;
        private Button btnDeletePos;
        private DataGridView dgvPosition;

        // Tab 4: Level
        private Panel pnlLevelBar;
        private Label lblSearchLevel;
        private TextBox txtSearchLevel;
        private Button btnAddLevel;
        private Button btnEditLevel;
        private Button btnDeleteLevel;
        private DataGridView dgvLevel;

        // Tab 5: Mapping
        private Panel pnlMapBar;
        private Label lblSearchMap;
        private TextBox txtSearchMap;
        private Label lblFilterDivOnMap;
        private ComboBox cboFilterDivOnMap;
        private Button btnAddMap;
        private Button btnEditMap;
        private Button btnDeleteMap;
        private DataGridView dgvMapping;
        private CheckBox chkMapTreeView;
        private TreeView tvMapping;

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitle = new Label();
            this.pnlToolbar = new Panel();
            this.btnImportExcel = new Button();
            this.btnDownloadTemplate = new Button();
            this.btnRefresh = new Button();
            this.btnClose = new Button();

            this.tabMaster = new TabControl();
            this.tabDivision = new TabPage();
            this.tabDepartment = new TabPage();
            this.tabPosition = new TabPage();
            this.tabLevel = new TabPage();
            this.tabMapping = new TabPage();

            // Form
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(1080, 680);
            this.Text = "จัดการข้อมูล Master โครงสร้างองค์กรและตำแหน่ง";
            this.Font = new Font("Tahoma", 9.5f);
            this.BackColor = Color.White;

            // Header
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 46;
            this.pnlHeader.BackColor = Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Padding = new Padding(16, 0, 0, 0);
            this.lblTitle.Text = "🏢 จัดการข้อมูล Master โครงสร้างองค์กรและตำแหน่ง (Org Master Management)";
            this.lblTitle.Font = new Font("Tahoma", 12f, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Toolbar
            this.pnlToolbar.Dock = DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.BackColor = Color.FromArgb(240, 244, 249);
            this.pnlToolbar.Padding = new Padding(12, 6, 12, 6);

            ConfigureTopButton(this.btnImportExcel, "📥 นำเข้าจาก Excel", Color.FromArgb(0, 105, 148), 12, 6, 150);
            this.btnImportExcel.Click += new System.EventHandler(this.btnImportExcel_Click);

            ConfigureTopButton(this.btnDownloadTemplate, "📑 โหลด Template", Color.FromArgb(70, 90, 120), 168, 6, 150);
            this.btnDownloadTemplate.Click += new System.EventHandler(this.btnDownloadTemplate_Click);

            ConfigureTopButton(this.btnRefresh, "🔄 รีเฟรช", Color.FromArgb(40, 120, 70), 324, 6, 110);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            ConfigureTopButton(this.btnClose, "✕ ปิด", Color.FromArgb(100, 100, 100), 440, 6, 90);
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.pnlToolbar.Controls.AddRange(new Control[] { this.btnImportExcel, this.btnDownloadTemplate, this.btnRefresh, this.btnClose });

            // TabControl
            this.tabMaster.Dock = DockStyle.Fill;
            this.tabMaster.Padding = new Point(14, 8);
            this.tabMaster.Font = new Font("Tahoma", 10f, FontStyle.Regular);

            // Tab 1: Division
            SetupDivisionTab();
            // Tab 2: Department
            SetupDepartmentTab();
            // Tab 3: Position
            SetupPositionTab();
            // Tab 4: Level
            SetupLevelTab();
            // Tab 5: Mapping
            SetupMappingTab();

            this.tabMaster.TabPages.AddRange(new TabPage[] {
                this.tabDivision, this.tabDepartment, this.tabPosition, this.tabLevel, this.tabMapping
            });

            this.Controls.Add(this.tabMaster);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
        }

        private void ConfigureTopButton(Button btn, string text, Color bg, int x, int y, int width)
        {
            btn.Text = text;
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Tahoma", 9f, FontStyle.Bold);
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 32);
            btn.Cursor = Cursors.Hand;
        }

        private void SetupDivisionTab()
        {
            this.tabDivision.Text = "  🏢 ฝ่าย (Division)  ";
            this.tabDivision.Padding = new Padding(8);

            this.pnlDivBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            this.lblSearchDiv = new Label { Text = "🔍 ค้นหา:", Location = new Point(8, 11), AutoSize = true, Font = new Font("Tahoma", 9.5f, FontStyle.Bold) };
            this.txtSearchDiv = new TextBox { Location = new Point(75, 8), Width = 220 };
            this.txtSearchDiv.TextChanged += (s, e) => FilterDivisionGrid();

            this.btnAddDiv = new Button { Text = "+ เพิ่มฝ่าย", Location = new Point(310, 6), Size = new Size(100, 28), BackColor = Color.FromArgb(31, 56, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 9f, FontStyle.Bold) };
            this.btnAddDiv.Click += new System.EventHandler(this.btnAddDiv_Click);

            this.btnEditDiv = new Button { Text = "✏ แก้ไข", Location = new Point(416, 6), Size = new Size(85, 28), BackColor = Color.FromArgb(60, 100, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnEditDiv.Click += new System.EventHandler(this.btnEditDiv_Click);

            this.btnDeleteDiv = new Button { Text = "🗑 ลบ", Location = new Point(507, 6), Size = new Size(75, 28), BackColor = Color.FromArgb(140, 30, 30), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnDeleteDiv.Click += new System.EventHandler(this.btnDeleteDiv_Click);

            this.pnlDivBar.Controls.AddRange(new Control[] { this.lblSearchDiv, this.txtSearchDiv, this.btnAddDiv, this.btnEditDiv, this.btnDeleteDiv });

            this.dgvDivision = CreateStandardGrid();
            this.tabDivision.Controls.Add(this.dgvDivision);
            this.tabDivision.Controls.Add(this.pnlDivBar);
        }

        private void SetupDepartmentTab()
        {
            this.tabDepartment.Text = "  📂 แผนก (Department)  ";
            this.tabDepartment.Padding = new Padding(8);

            this.pnlDeptBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            this.lblSearchDept = new Label { Text = "🔍 ค้นหา:", Location = new Point(8, 11), AutoSize = true, Font = new Font("Tahoma", 9.5f, FontStyle.Bold) };
            this.txtSearchDept = new TextBox { Location = new Point(75, 8), Width = 180 };
            this.txtSearchDept.TextChanged += (s, e) => FilterDepartmentGrid();

            this.lblFilterDivOnDept = new Label { Text = "ฝ่าย:", Location = new Point(265, 11), AutoSize = true, Font = new Font("Tahoma", 9.5f, FontStyle.Bold) };
            this.cboFilterDivOnDept = new ComboBox { Location = new Point(305, 8), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            this.cboFilterDivOnDept.SelectedIndexChanged += (s, e) => FilterDepartmentGrid();

            this.btnAddDept = new Button { Text = "+ เพิ่มแผนก", Location = new Point(495, 6), Size = new Size(100, 28), BackColor = Color.FromArgb(31, 56, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 9f, FontStyle.Bold) };
            this.btnAddDept.Click += new System.EventHandler(this.btnAddDept_Click);

            this.btnEditDept = new Button { Text = "✏ แก้ไข", Location = new Point(601, 6), Size = new Size(85, 28), BackColor = Color.FromArgb(60, 100, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnEditDept.Click += new System.EventHandler(this.btnEditDept_Click);

            this.btnDeleteDept = new Button { Text = "🗑 ลบ", Location = new Point(692, 6), Size = new Size(75, 28), BackColor = Color.FromArgb(140, 30, 30), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnDeleteDept.Click += new System.EventHandler(this.btnDeleteDept_Click);

            this.pnlDeptBar.Controls.AddRange(new Control[] {
                this.lblSearchDept, this.txtSearchDept, this.lblFilterDivOnDept, this.cboFilterDivOnDept,
                this.btnAddDept, this.btnEditDept, this.btnDeleteDept
            });

            this.dgvDepartment = CreateStandardGrid();
            this.tabDepartment.Controls.Add(this.dgvDepartment);
            this.tabDepartment.Controls.Add(this.pnlDeptBar);
        }

        private void SetupPositionTab()
        {
            this.tabPosition.Text = "  👤 ตำแหน่ง (Position)  ";
            this.tabPosition.Padding = new Padding(8);

            this.pnlPosBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            this.lblSearchPos = new Label { Text = "🔍 ค้นหา:", Location = new Point(8, 11), AutoSize = true, Font = new Font("Tahoma", 9.5f, FontStyle.Bold) };
            this.txtSearchPos = new TextBox { Location = new Point(75, 8), Width = 220 };
            this.txtSearchPos.TextChanged += (s, e) => FilterPositionGrid();

            this.btnAddPos = new Button { Text = "+ เพิ่มตำแหน่ง", Location = new Point(310, 6), Size = new Size(110, 28), BackColor = Color.FromArgb(31, 56, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 9f, FontStyle.Bold) };
            this.btnAddPos.Click += new System.EventHandler(this.btnAddPos_Click);

            this.btnEditPos = new Button { Text = "✏ แก้ไข", Location = new Point(426, 6), Size = new Size(85, 28), BackColor = Color.FromArgb(60, 100, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnEditPos.Click += new System.EventHandler(this.btnEditPos_Click);

            this.btnDeletePos = new Button { Text = "🗑 ลบ", Location = new Point(517, 6), Size = new Size(75, 28), BackColor = Color.FromArgb(140, 30, 30), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnDeletePos.Click += new System.EventHandler(this.btnDeletePos_Click);

            this.pnlPosBar.Controls.AddRange(new Control[] { this.lblSearchPos, this.txtSearchPos, this.btnAddPos, this.btnEditPos, this.btnDeletePos });

            this.dgvPosition = CreateStandardGrid();
            this.tabPosition.Controls.Add(this.dgvPosition);
            this.tabPosition.Controls.Add(this.pnlPosBar);
        }

        private void SetupLevelTab()
        {
            this.tabLevel.Text = "  🏷 ระดับตำแหน่ง (Level)  ";
            this.tabLevel.Padding = new Padding(8);

            this.pnlLevelBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            this.lblSearchLevel = new Label { Text = "🔍 ค้นหา:", Location = new Point(8, 11), AutoSize = true, Font = new Font("Tahoma", 9.5f, FontStyle.Bold) };
            this.txtSearchLevel = new TextBox { Location = new Point(75, 8), Width = 220 };
            this.txtSearchLevel.TextChanged += (s, e) => FilterLevelGrid();

            this.btnAddLevel = new Button { Text = "+ เพิ่มระดับ", Location = new Point(310, 6), Size = new Size(100, 28), BackColor = Color.FromArgb(31, 56, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 9f, FontStyle.Bold) };
            this.btnAddLevel.Click += new System.EventHandler(this.btnAddLevel_Click);

            this.btnEditLevel = new Button { Text = "✏ แก้ไข", Location = new Point(416, 6), Size = new Size(85, 28), BackColor = Color.FromArgb(60, 100, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnEditLevel.Click += new System.EventHandler(this.btnEditLevel_Click);

            this.btnDeleteLevel = new Button { Text = "🗑 ลบ", Location = new Point(507, 6), Size = new Size(75, 28), BackColor = Color.FromArgb(140, 30, 30), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnDeleteLevel.Click += new System.EventHandler(this.btnDeleteLevel_Click);

            this.pnlLevelBar.Controls.AddRange(new Control[] { this.lblSearchLevel, this.txtSearchLevel, this.btnAddLevel, this.btnEditLevel, this.btnDeleteLevel });

            this.dgvLevel = CreateStandardGrid();
            this.tabLevel.Controls.Add(this.dgvLevel);
            this.tabLevel.Controls.Add(this.pnlLevelBar);
        }

        private void SetupMappingTab()
        {
            this.tabMapping.Text = "  🔗 ผังโครงสร้าง (Mapping)  ";
            this.tabMapping.Padding = new Padding(8);

            this.pnlMapBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            this.lblSearchMap = new Label { Text = "🔍 ค้นหา:", Location = new Point(8, 11), AutoSize = true, Font = new Font("Tahoma", 9.5f, FontStyle.Bold) };
            this.txtSearchMap = new TextBox { Location = new Point(75, 8), Width = 180 };
            this.txtSearchMap.TextChanged += (s, e) => FilterMappingGrid();

            this.lblFilterDivOnMap = new Label { Text = "ฝ่าย:", Location = new Point(265, 11), AutoSize = true, Font = new Font("Tahoma", 9.5f, FontStyle.Bold) };
            this.cboFilterDivOnMap = new ComboBox { Location = new Point(305, 8), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            this.cboFilterDivOnMap.SelectedIndexChanged += (s, e) => FilterMappingGrid();

            this.btnAddMap = new Button { Text = "+ ผูกตำแหน่งใหม่", Location = new Point(495, 6), Size = new Size(125, 28), BackColor = Color.FromArgb(31, 56, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 9f, FontStyle.Bold) };
            this.btnAddMap.Click += new System.EventHandler(this.btnAddMap_Click);

            this.btnEditMap = new Button { Text = "✏ แก้ไข", Location = new Point(626, 6), Size = new Size(85, 28), BackColor = Color.FromArgb(60, 100, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnEditMap.Click += new System.EventHandler(this.btnEditMap_Click);

            this.btnDeleteMap = new Button { Text = "🗑 ลบ", Location = new Point(717, 6), Size = new Size(75, 28), BackColor = Color.FromArgb(140, 30, 30), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnDeleteMap.Click += new System.EventHandler(this.btnDeleteMap_Click);

            this.chkMapTreeView = new CheckBox { Text = "🌳 มุมมองแบบ Tree", Appearance = Appearance.Button, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(808, 6), Size = new Size(140, 28), BackColor = Color.FromArgb(240, 244, 249), FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 9f, FontStyle.Bold) };
            this.chkMapTreeView.FlatAppearance.CheckedBackColor = Color.FromArgb(200, 220, 240);
            this.chkMapTreeView.CheckedChanged += new System.EventHandler(this.chkMapTreeView_CheckedChanged);

            this.pnlMapBar.Controls.AddRange(new Control[] {
                this.lblSearchMap, this.txtSearchMap, this.lblFilterDivOnMap, this.cboFilterDivOnMap,
                this.btnAddMap, this.btnEditMap, this.btnDeleteMap, this.chkMapTreeView
            });

            this.tvMapping = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Tahoma", 10f),
                ItemHeight = 26,
                HideSelection = false,
                FullRowSelect = true,
                ShowLines = false,
                Visible = false
            };
            this.tvMapping.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(this.tvMapping_NodeMouseDoubleClick);

            this.dgvMapping = CreateStandardGrid();
            this.dgvMapping.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) btnEditMap_Click(s, e); };
            this.tabMapping.Controls.Add(this.dgvMapping);
            this.tabMapping.Controls.Add(this.tvMapping);
            this.tabMapping.Controls.Add(this.pnlMapBar);
        }

        private DataGridView CreateStandardGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 32,
                RowTemplate = { Height = 28 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 56, 100);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 247, 251);
            return grid;
        }
    }
}
