using System;
using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.ImportFile
{
    partial class AuthorityConfigEditDialog
    {
        private static readonly Color TextColor = Color.FromArgb(33, 33, 33);
        private static readonly Color NavyColor = Color.FromArgb(31, 56, 100);
        private static readonly Color RowAltColor = Color.FromArgb(244, 247, 251);
        private static readonly Color GridLineColor = Color.FromArgb(200, 208, 218);

        private Panel pnlHeader;
        private Label lblTitle;
        private Panel pnlContent;
        private TableLayoutPanel tblInfo;
        private Label lblPermissions;
        private TableLayoutPanel tblPermissions;
        private FlowLayoutPanel pnlButtons;
        private Button btnSave;
        private Button btnCancel;

        private TextBox txtLabel;
        private ComboBox cboPosition;
        private ComboBox cboLevelCode;
        private ComboBox cboDept;
        private ComboBox cboDiv;

        private CheckBox chkHC_V, chkHC_A, chkHC_E, chkHC_Ap, chkHC_Dl;
        private CheckBox chkSC_V, chkSC_A, chkSC_E, chkSC_Ap, chkSC_Dl;
        private CheckBox chkNC_V, chkNC_A, chkNC_E, chkNC_Ap, chkNC_Dl;
        private CheckBox chkPC_V, chkPC_A, chkPC_E, chkPC_Ap, chkPC_Dl;

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitle = new Label();
            this.pnlContent = new Panel();
            this.tblInfo = new TableLayoutPanel();
            this.lblPermissions = new Label();
            this.tblPermissions = new TableLayoutPanel();
            this.pnlButtons = new FlowLayoutPanel();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.txtLabel = new TextBox();
            this.cboPosition = new ComboBox();
            this.cboLevelCode = new ComboBox();
            this.cboDept = new ComboBox();
            this.cboDiv = new ComboBox();

            this.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.tblInfo.SuspendLayout();
            this.tblPermissions.SuspendLayout();
            this.pnlButtons.SuspendLayout();

            // Form
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(760, 470);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.ForeColor = TextColor;
            this.Font = new Font("Tahoma", 9.5f);
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;

            // Header
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 46;
            this.pnlHeader.BackColor = NavyColor;
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Padding = new Padding(16, 0, 0, 0);
            this.lblTitle.Text = "แก้ไขสิทธิ์การเข้าถึง";
            this.lblTitle.Font = new Font("Tahoma", 12.5f, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Info table: 4 columns (label | input | label | input)
            this.tblInfo.Dock = DockStyle.Top;
            this.tblInfo.AutoSize = true;
            this.tblInfo.ColumnCount = 4;
            this.tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85F));
            this.tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F));
            this.tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tblInfo.RowCount = 3;
            for (int i = 0; i < 3; i++)
            {
                this.tblInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            }
            this.tblInfo.Height = 102;

            // Row 0: ฝ่าย (Div) -> แผนก (Dept)
            AddInfoLabel(this.tblInfo, "ฝ่าย:", 0, 0);
            ConfigureCombo(this.cboDiv);
            this.tblInfo.Controls.Add(this.cboDiv, 1, 0);

            AddInfoLabel(this.tblInfo, "แผนก:", 2, 0);
            ConfigureCombo(this.cboDept);
            this.tblInfo.Controls.Add(this.cboDept, 3, 0);

            // Row 1: ตำแหน่ง (Position) -> ระดับตำแหน่ง (Level)
            AddInfoLabel(this.tblInfo, "ตำแหน่ง:", 0, 1);
            ConfigureCombo(this.cboPosition);
            this.tblInfo.Controls.Add(this.cboPosition, 1, 1);

            AddInfoLabel(this.tblInfo, "ระดับตำแหน่ง:", 2, 1);
            ConfigureCombo(this.cboLevelCode);
            this.tblInfo.Controls.Add(this.cboLevelCode, 3, 1);

            // Row 2: ชื่อสิทธิ์ (Label)
            AddInfoLabel(this.tblInfo, "ชื่อสิทธิ์:", 0, 2);
            ConfigureInput(this.txtLabel);
            this.tblInfo.SetColumnSpan(this.txtLabel, 3);
            this.tblInfo.Controls.Add(this.txtLabel, 1, 2);

            // Permissions title
            this.lblPermissions.Dock = DockStyle.Top;
            this.lblPermissions.Height = 32;
            this.lblPermissions.Padding = new Padding(0, 8, 0, 0);
            this.lblPermissions.Text = "สิทธิ์การเข้าถึงตามระดับความลับ";
            this.lblPermissions.Font = new Font("Tahoma", 10.5f, FontStyle.Bold);
            this.lblPermissions.ForeColor = NavyColor;
            this.lblPermissions.TextAlign = ContentAlignment.MiddleLeft;

            // Permission table: header + 4 rows, 6 columns
            this.tblPermissions.Dock = DockStyle.Fill;
            this.tblPermissions.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            this.tblPermissions.ForeColor = GridLineColor;
            this.tblPermissions.ColumnCount = 6;
            this.tblPermissions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            for (int i = 0; i < 5; i++)
            {
                this.tblPermissions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            }
            this.tblPermissions.RowCount = 5;
            this.tblPermissions.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            for (int i = 0; i < 4; i++)
            {
                this.tblPermissions.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            }

            AddPermissionHeader(this.tblPermissions, "ระดับความลับ", 0);
            AddPermissionHeader(this.tblPermissions, "ดู", 1);
            AddPermissionHeader(this.tblPermissions, "เพิ่ม", 2);
            AddPermissionHeader(this.tblPermissions, "แก้ไข", 3);
            AddPermissionHeader(this.tblPermissions, "อนุมัติ", 4);
            AddPermissionHeader(this.tblPermissions, "⬇ ดาวน์โหลด", 5);

            AddPermissionRow(this.tblPermissions, 1, "HC - สูงสุด (Highly Confidential)", Color.FromArgb(160, 20, 20),
                out this.chkHC_V, out this.chkHC_A, out this.chkHC_E, out this.chkHC_Ap, out this.chkHC_Dl);
            AddPermissionRow(this.tblPermissions, 2, "SC - ลับมาก (Strictly Confidential)", Color.FromArgb(175, 85, 0),
                out this.chkSC_V, out this.chkSC_A, out this.chkSC_E, out this.chkSC_Ap, out this.chkSC_Dl);
            AddPermissionRow(this.tblPermissions, 3, "NC - ปกติ (Normal Confidential)", Color.FromArgb(0, 80, 150),
                out this.chkNC_V, out this.chkNC_A, out this.chkNC_E, out this.chkNC_Ap, out this.chkNC_Dl);
            AddPermissionRow(this.tblPermissions, 4, "PC - ทั่วไป (Public)", Color.FromArgb(0, 110, 50),
                out this.chkPC_V, out this.chkPC_A, out this.chkPC_E, out this.chkPC_Ap, out this.chkPC_Dl);

            // Content (add Fill first, then Top controls bottom-up)
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.Padding = new Padding(16, 10, 16, 8);
            this.pnlContent.Controls.Add(this.tblPermissions);
            this.pnlContent.Controls.Add(this.lblPermissions);
            this.pnlContent.Controls.Add(this.tblInfo);

            // Buttons (RightToLeft flow: first added sits at the right edge)
            this.pnlButtons.Dock = DockStyle.Bottom;
            this.pnlButtons.Height = 52;
            this.pnlButtons.BackColor = Color.FromArgb(236, 239, 244);
            this.pnlButtons.FlowDirection = FlowDirection.RightToLeft;
            this.pnlButtons.Padding = new Padding(16, 9, 16, 9);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnSave);

            this.btnSave.Text = "บันทึก";
            this.btnSave.Size = new Size(100, 32);
            this.btnSave.Margin = new Padding(8, 0, 0, 0);
            this.btnSave.BackColor = Color.FromArgb(30, 110, 50);
            this.btnSave.ForeColor = Color.White;
            this.btnSave.FlatStyle = FlatStyle.Flat;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.btnSave.Cursor = Cursors.Hand;
            this.btnSave.TabIndex = 6;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.Size = new Size(100, 32);
            this.btnCancel.Margin = new Padding(0);
            this.btnCancel.BackColor = Color.White;
            this.btnCancel.ForeColor = Color.FromArgb(60, 60, 60);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.FlatAppearance.BorderSize = 1;
            this.btnCancel.FlatAppearance.BorderColor = Color.FromArgb(140, 140, 140);
            this.btnCancel.Font = new Font("Tahoma", 9.5f);
            this.btnCancel.Cursor = Cursors.Hand;
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // Form controls (Fill first, then Bottom, then Top)
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlHeader);

            this.pnlButtons.ResumeLayout(false);
            this.tblPermissions.ResumeLayout(false);
            this.tblInfo.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void AddInfoLabel(TableLayoutPanel table, string text, int column, int row)
        {
            var lbl = new Label
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = new Font("Tahoma", 9.5f, FontStyle.Bold),
                ForeColor = TextColor
            };
            table.Controls.Add(lbl, column, row);
        }

        private void ConfigureInput(TextBox txt)
        {
            txt.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txt.Margin = new Padding(3, 3, 12, 3);
            txt.Font = new Font("Tahoma", 9.5f);
            txt.BorderStyle = BorderStyle.FixedSingle;
        }

        private void ConfigureCombo(ComboBox cbo)
        {
            cbo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cbo.Margin = new Padding(3, 3, 12, 3);
            cbo.Font = new Font("Tahoma", 9.5f);
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void AddPermissionHeader(TableLayoutPanel table, string text, int column)
        {
            var lbl = new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(column == 0 ? 10 : 0, 0, 0, 0),
                BackColor = NavyColor,
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9.5f, FontStyle.Bold),
                TextAlign = column == 0 ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleCenter
            };
            table.Controls.Add(lbl, column, 0);
        }

        private void AddPermissionRow(TableLayoutPanel table, int row, string title, Color titleColor,
            out CheckBox chkView, out CheckBox chkAdd, out CheckBox chkEdit, out CheckBox chkApprove,
            out CheckBox chkDownload)
        {
            Color rowColor = (row % 2 == 0) ? RowAltColor : Color.White;

            var lbl = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(10, 0, 0, 0),
                BackColor = rowColor,
                ForeColor = titleColor,
                Font = new Font("Tahoma", 9.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            table.Controls.Add(lbl, 0, row);

            chkView = CreateCell(table, row, 1, rowColor);
            chkAdd = CreateCell(table, row, 2, rowColor);
            chkEdit = CreateCell(table, row, 3, rowColor);
            chkApprove = CreateCell(table, row, 4, rowColor);
            chkDownload = CreateCell(table, row, 5, rowColor);   // ดาวน์โหลดได้ทันทีโดยไม่ต้องขออนุมัติ
        }

        private CheckBox CreateCell(TableLayoutPanel table, int row, int column, Color backColor)
        {
            var chk = new CheckBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackColor = backColor,
                CheckAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            table.Controls.Add(chk, column, row);
            return chk;
        }
    }
}
