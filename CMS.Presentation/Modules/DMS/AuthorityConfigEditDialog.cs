using System;
using System.Drawing;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.ImportFile
{
    public partial class AuthorityConfigEditDialog : Form
    {
        public FolderAuthorityConfig Config { get; private set; }

        public AuthorityConfigEditDialog(FolderAuthorityConfig config, bool isEdit)
        {
            InitializeComponent();
            Config = config ?? new FolderAuthorityConfig();
            this.Text = isEdit ? "แก้ไขสิทธิ์การเข้าถึง" : "เพิ่มสิทธิ์การเข้าถึงใหม่";
            
            if (isEdit)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            txtLabel.Text = Config.ConfigLabel ?? "";
            txtPosition.Text = Config.PositionName ?? "";
            txtLevelCode.Text = Config.LevelCode ?? "";
            txtDept.Text = Config.DepartmentCode ?? "";
            txtDiv.Text = Config.DivisionCode ?? "";

            chkHC_V.Checked = Config.HC_View;
            chkHC_A.Checked = Config.HC_Add;
            chkHC_E.Checked = Config.HC_Edit;
            chkHC_Ap.Checked = Config.HC_Approve;

            chkSC_V.Checked = Config.SC_View;
            chkSC_A.Checked = Config.SC_Add;
            chkSC_E.Checked = Config.SC_Edit;
            chkSC_Ap.Checked = Config.SC_Approve;

            chkNC_V.Checked = Config.NC_View;
            chkNC_A.Checked = Config.NC_Add;
            chkNC_E.Checked = Config.NC_Edit;
            chkNC_Ap.Checked = Config.NC_Approve;

            chkPC_V.Checked = Config.PC_View;
            chkPC_A.Checked = Config.PC_Add;
            chkPC_E.Checked = Config.PC_Edit;
            chkPC_Ap.Checked = Config.PC_Approve;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLabel.Text))
            {
                MessageBox.Show("กรุณากรอก Label", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLabel.Focus();
                return;
            }

            Config.ConfigLabel = txtLabel.Text.Trim();
            Config.PositionName = string.IsNullOrWhiteSpace(txtPosition.Text) ? null : txtPosition.Text.Trim();
            Config.LevelCode = string.IsNullOrWhiteSpace(txtLevelCode.Text) ? null : txtLevelCode.Text.Trim().ToUpper();
            Config.DepartmentCode = string.IsNullOrWhiteSpace(txtDept.Text) ? null : txtDept.Text.Trim();
            Config.DivisionCode = string.IsNullOrWhiteSpace(txtDiv.Text) ? null : txtDiv.Text.Trim();

            Config.HC_View = chkHC_V.Checked;
            Config.HC_Add = chkHC_A.Checked;
            Config.HC_Edit = chkHC_E.Checked;
            Config.HC_Approve = chkHC_Ap.Checked;

            Config.SC_View = chkSC_V.Checked;
            Config.SC_Add = chkSC_A.Checked;
            Config.SC_Edit = chkSC_E.Checked;
            Config.SC_Approve = chkSC_Ap.Checked;

            Config.NC_View = chkNC_V.Checked;
            Config.NC_Add = chkNC_A.Checked;
            Config.NC_Edit = chkNC_E.Checked;
            Config.NC_Approve = chkNC_Ap.Checked;

            Config.PC_View = chkPC_V.Checked;
            Config.PC_Add = chkPC_A.Checked;
            Config.PC_Edit = chkPC_E.Checked;
            Config.PC_Approve = chkPC_Ap.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.ClientSize = new Size(780, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);

            // Header panel
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(31, 56, 100)
            };
            var lblTitle = new Label
            {
                Text = "📝 แก้ไขสิทธิ์การเข้าถึง",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(760, 60),
                Location = new Point(10, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblTitle);
            this.Controls.Add(pnlHeader);

            // Main content panel
            var pnlContent = new Panel
            {
                Location = new Point(20, 80),
                Size = new Size(740, 380),
                AutoScroll = true
            };

            int y = 10;

            // Label
            AddLabel(pnlContent, "Label:", 10, y);
            txtLabel = new TextBox { Location = new Point(95, y), Size = new Size(300, 23) };
            pnlContent.Controls.Add(txtLabel);

            // Position
            AddLabel(pnlContent, "ตำแหน่ง:", 410, y);
            txtPosition = new TextBox { Location = new Point(485, y), Size = new Size(255, 23) };
            pnlContent.Controls.Add(txtPosition);
            y += 35;

            // LevelCode
            AddLabel(pnlContent, "LevelCode:", 10, y);
            txtLevelCode = new TextBox { Location = new Point(95, y), Size = new Size(100, 23) };
            pnlContent.Controls.Add(txtLevelCode);
            y += 35;

            // Department
            AddLabel(pnlContent, "แผนก:", 10, y);
            txtDept = new TextBox { Location = new Point(95, y), Size = new Size(300, 23) };
            pnlContent.Controls.Add(txtDept);

            // Division
            AddLabel(pnlContent, "ฝ่าย:", 410, y);
            txtDiv = new TextBox { Location = new Point(485, y), Size = new Size(255, 23) };
            pnlContent.Controls.Add(txtDiv);
            y += 45;

            // Permissions section
            var lblPermissions = new Label
            {
                Text = "สิทธิ์การเข้าถึงตามระดับความลับ",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(10, y),
                Size = new Size(720, 25),
                ForeColor = Color.FromArgb(31, 56, 100)
            };
            pnlContent.Controls.Add(lblPermissions);
            y += 30;

            // HC - High Confidential
            y = AddPermissionGroup(pnlContent, y, "HC - สูงสุด (Highly Confidential)", 
                Color.FromArgb(180, 30, 30), out chkHC_V, out chkHC_A, out chkHC_E, out chkHC_Ap);

            // SC - Strictly Confidential
            y = AddPermissionGroup(pnlContent, y, "SC - ลับมาก (Strictly Confidential)", 
                Color.FromArgb(200, 100, 0), out chkSC_V, out chkSC_A, out chkSC_E, out chkSC_Ap);

            // NC - Normal Confidential
            y = AddPermissionGroup(pnlContent, y, "NC - ปกติ (Normal Confidential)", 
                Color.FromArgb(0, 90, 160), out chkNC_V, out chkNC_A, out chkNC_E, out chkNC_Ap);

            // PC - Public
            y = AddPermissionGroup(pnlContent, y, "PC - ทั่วไป (Public)", 
                Color.FromArgb(0, 130, 60), out chkPC_V, out chkPC_A, out chkPC_E, out chkPC_Ap);

            this.Controls.Add(pnlContent);

            // Button panel
            var pnlButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var btnSave = new Button
            {
                Text = "💾 บันทึก",
                Size = new Size(120, 35),
                Location = new Point(530, 12),
                BackColor = Color.FromArgb(56, 142, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;

            var btnCancel = new Button
            {
                Text = "✖ ยกเลิก",
                Size = new Size(120, 35),
                Location = new Point(660, 12),
                BackColor = Color.FromArgb(120, 120, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += btnCancel_Click;

            pnlButtons.Controls.Add(btnSave);
            pnlButtons.Controls.Add(btnCancel);
            this.Controls.Add(pnlButtons);

            this.ResumeLayout(false);
        }

        private void AddLabel(Panel parent, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y + 3),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            parent.Controls.Add(lbl);
        }

        private int AddPermissionGroup(Panel parent, int y, string title, Color color,
            out CheckBox chkView, out CheckBox chkAdd, out CheckBox chkEdit, out CheckBox chkApprove)
        {
            var pnlGroup = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(720, 50),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var lblTitle = new Label
            {
                Text = title,
                Location = new Point(10, 5),
                Size = new Size(350, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = color
            };
            pnlGroup.Controls.Add(lblTitle);

            chkView = new CheckBox { Text = "ดู", Location = new Point(10, 28), Size = new Size(80, 20) };
            chkAdd = new CheckBox { Text = "เพิ่ม", Location = new Point(100, 28), Size = new Size(80, 20) };
            chkEdit = new CheckBox { Text = "แก้ไข", Location = new Point(190, 28), Size = new Size(80, 20) };
            chkApprove = new CheckBox { Text = "อนุมัติ", Location = new Point(280, 28), Size = new Size(80, 20) };

            pnlGroup.Controls.Add(chkView);
            pnlGroup.Controls.Add(chkAdd);
            pnlGroup.Controls.Add(chkEdit);
            pnlGroup.Controls.Add(chkApprove);

            parent.Controls.Add(pnlGroup);

            return y + 55;
        }

        private TextBox txtLabel;
        private TextBox txtPosition;
        private TextBox txtLevelCode;
        private TextBox txtDept;
        private TextBox txtDiv;

        private CheckBox chkHC_V, chkHC_A, chkHC_E, chkHC_Ap;
        private CheckBox chkSC_V, chkSC_A, chkSC_E, chkSC_Ap;
        private CheckBox chkNC_V, chkNC_A, chkNC_E, chkNC_Ap;
        private CheckBox chkPC_V, chkPC_A, chkPC_E, chkPC_Ap;
    }
}
