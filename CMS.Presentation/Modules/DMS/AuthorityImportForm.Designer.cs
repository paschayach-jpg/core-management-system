namespace CMS.Presentation
{
    partial class AuthorityImportForm
    {
        private System.ComponentModel.IContainer components = new System.ComponentModel.Container();

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private static void AddColumns(System.Windows.Forms.DataGridView grid, params string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
                grid.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = grid.Name + "Col" + i,
                    HeaderText = headers[i],
                    AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
                });
        }

        private static void ConfigureGrid(System.Windows.Forms.DataGridView g, string name)
        {
            g.Name = name;
            g.Dock = System.Windows.Forms.DockStyle.Fill;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.RowHeadersVisible = false;
            g.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            g.BackgroundColor = System.Drawing.Color.White;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(230, 236, 245);
            g.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            g.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            g.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 253);
        }

        private static System.Windows.Forms.Label CreateEmptyLabel(string name)
        {
            return new System.Windows.Forms.Label
            {
                Name = name,
                Dock = System.Windows.Forms.DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                ForeColor = System.Drawing.Color.Gray,
                BackColor = System.Drawing.Color.White
            };
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSteps = new System.Windows.Forms.Label();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.grpFile = new System.Windows.Forms.GroupBox();
            this.tblFile = new System.Windows.Forms.TableLayoutPanel();
            this.tblOptions = new System.Windows.Forms.TableLayoutPanel();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.lblSheet = new System.Windows.Forms.Label();
            this.cboSheet = new System.Windows.Forms.ComboBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabPlans = new System.Windows.Forms.TabPage();
            this.dgvPlans = new System.Windows.Forms.DataGridView();
            this.lblPlansHint = new System.Windows.Forms.Label();
            this.lblEmptyPlans = CreateEmptyLabel("lblEmptyPlans");
            this.tabLevels = new System.Windows.Forms.TabPage();
            this.tabIssues = new System.Windows.Forms.TabPage();
            this.dgvLevels = new System.Windows.Forms.DataGridView();
            this.dgvIssues = new System.Windows.Forms.DataGridView();
            this.lblEmptyLevels = CreateEmptyLabel("lblEmptyLevels");
            this.lblEmptyIssues = CreateEmptyLabel("lblEmptyIssues");
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblSummary = new System.Windows.Forms.Label();
            this.btnCommit = new System.Windows.Forms.Button();
            this.pnlSpacer = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);

            // pnlHeader
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 66;
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(240, 245, 251);
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(14, 8, 14, 8);
            this.pnlHeader.Controls.Add(this.lblSteps);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Height = 26;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Text = "นำเข้า Level ผู้ใช้ และสิทธิ์ตามตำแหน่งจากไฟล์ Excel";

            this.lblSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSteps.ForeColor = System.Drawing.Color.DimGray;
            this.lblSteps.Text = "ขั้นตอน:  ① เลือกไฟล์ Excel   →   ② เลือก Sheet   →   ③ กด \"ตรวจสอบ\" เพื่อดูตัวอย่าง   →   ④ กด \"นำเข้า\" เมื่อไม่มีข้อผิดพลาด";

            // pnlTop
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Height = 138;
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10, 6, 10, 2);
            this.pnlTop.Controls.Add(this.grpOptions);
            this.pnlTop.Controls.Add(this.grpFile);

            this.grpFile.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpFile.Height = 58;
            this.grpFile.Text = "① ไฟล์ Excel";
            this.grpFile.Controls.Add(this.tblFile);

            this.tblFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblFile.ColumnCount = 2;
            this.tblFile.RowCount = 1;
            this.tblFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.tblFile.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblFile.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tblFile.Controls.Add(this.txtFile, 0, 0);
            this.tblFile.Controls.Add(this.btnBrowse, 1, 0);

            this.txtFile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.txtFile.ReadOnly = true;
            this.toolTip.SetToolTip(this.txtFile, "ไฟล์ที่เลือก (.xlsx)");

            this.btnBrowse.Text = "เลือกไฟล์…";
            this.btnBrowse.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBrowse.Size = new System.Drawing.Size(110, 28);
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            this.grpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOptions.Text = "② Sheet และตัวเลือก";
            this.grpOptions.Controls.Add(this.tblOptions);

            this.tblOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblOptions.ColumnCount = 4;
            this.tblOptions.RowCount = 1;
            this.tblOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tblOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tblOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 194F));
            this.tblOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblOptions.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tblOptions.Controls.Add(this.lblSheet, 0, 0);
            this.tblOptions.Controls.Add(this.cboSheet, 1, 0);
            this.tblOptions.Controls.Add(this.btnPreview, 3, 0);

            this.lblSheet.Text = "Sheet:";
            this.lblSheet.AutoSize = true;
            this.lblSheet.Anchor = System.Windows.Forms.AnchorStyles.Left;

            this.cboSheet.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.cboSheet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSheet.SelectedIndexChanged += new System.EventHandler(this.InputChanged);


            this.btnPreview.Text = "③ ตรวจสอบ (Preview)";
            this.btnPreview.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnPreview.Size = new System.Drawing.Size(180, 30);
            this.btnPreview.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnPreview.Enabled = false;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);

            // tabs
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Padding = new System.Drawing.Point(14, 5);
            this.tabs.Controls.Add(this.tabLevels);
            this.tabs.Controls.Add(this.tabPlans);
            this.tabs.Controls.Add(this.tabIssues);

            this.tabPlans.Text = "สิทธิ์ตามตำแหน่ง (4 กล่อง)";
            this.tabPlans.Controls.Add(this.dgvPlans);
            this.tabPlans.Controls.Add(this.lblPlansHint);
            this.lblPlansHint.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPlansHint.Height = 24;
            this.lblPlansHint.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.lblPlansHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblPlansHint.Text = "อัปเดตเฉพาะตำแหน่งที่มี config อยู่แล้ว: สิทธิ์ ดู / เพิ่ม / แก้ไข / อนุมัติ ของแต่ละระดับความลับ (ไม่สร้างตำแหน่งใหม่)";
            ConfigureGrid(this.dgvPlans, "dgvPlans");
            AddColumns(this.dgvPlans, "สถานะ", "ตำแหน่ง", "HC · สูงสุด", "SC · ลับมาก", "NC · ปกติ", "PC · ทั่วไป");
            this.dgvPlans.Controls.Add(this.lblEmptyPlans);
            this.tabLevels.Text = "Level ผู้ใช้";
            this.tabLevels.Controls.Add(this.dgvLevels);
            this.tabIssues.Text = "ข้อผิดพลาด/คำเตือน";
            this.tabIssues.Controls.Add(this.dgvIssues);


            ConfigureGrid(this.dgvLevels, "dgvLevels");
            AddColumns(this.dgvLevels, "สถานะ", "รหัสพนักงาน", "ชื่อพนักงาน", "Level เดิม", "Level ใหม่");
            ConfigureGrid(this.dgvIssues, "dgvIssues");
            AddColumns(this.dgvIssues, "ระดับ", "แถวใน Excel", "รหัส", "รายละเอียด", "วิธีแก้");
            this.dgvLevels.Controls.Add(this.lblEmptyLevels);
            this.dgvIssues.Controls.Add(this.lblEmptyIssues);

            // pnlBottom
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height = 56;
            this.pnlBottom.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            this.pnlBottom.Controls.Add(this.lblSummary);
            this.pnlBottom.Controls.Add(this.btnCommit);
            this.pnlBottom.Controls.Add(this.pnlSpacer);
            this.pnlBottom.Controls.Add(this.btnClose);

            this.lblSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblSummary.Text = "เลือกไฟล์ Excel แล้วกด ตรวจสอบ";

            this.btnCommit.Text = "④ นำเข้า";
            this.btnCommit.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCommit.Width = 130;
            this.btnCommit.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnCommit.Enabled = false;
            this.btnCommit.Click += new System.EventHandler(this.btnCommit_Click);

            this.pnlSpacer.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlSpacer.Width = 8;

            this.btnClose.Text = "ปิด";
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.Width = 100;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ClientSize = new System.Drawing.Size(900, 620);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlBottom);
            this.MinimumSize = new System.Drawing.Size(820, 520);
            this.Name = "AuthorityImportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "นำเข้า Level และสิทธิ์จาก Excel";
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSteps;
        private System.Windows.Forms.GroupBox grpFile;
        private System.Windows.Forms.TableLayoutPanel tblFile;
        private System.Windows.Forms.TableLayoutPanel tblOptions;
        private System.Windows.Forms.TabPage tabPlans;
        private System.Windows.Forms.DataGridView dgvPlans;
        private System.Windows.Forms.Label lblPlansHint;
        private System.Windows.Forms.Label lblEmptyPlans;
        private System.Windows.Forms.GroupBox grpOptions;
        private System.Windows.Forms.Label lblEmptyLevels;
        private System.Windows.Forms.Label lblEmptyIssues;
        private System.Windows.Forms.Panel pnlSpacer;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblSheet;
        private System.Windows.Forms.ComboBox cboSheet;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabLevels;
        private System.Windows.Forms.TabPage tabIssues;
        private System.Windows.Forms.DataGridView dgvLevels;
        private System.Windows.Forms.DataGridView dgvIssues;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Button btnCommit;
        private System.Windows.Forms.Button btnClose;
    }
}
