using System;
using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation
{
    partial class ShareDialogInternal
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

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Color navyDark   = Color.FromArgb(25,  55,  105);
            Color navyMid    = Color.FromArgb(50,  90,  150);
            Color navyLight  = Color.FromArgb(215, 228, 248);
            Color bodyBg     = Color.FromArgb(243, 246, 251);
            Color footerBg   = Color.FromArgb(228, 234, 245);
            Color borderGray = Color.FromArgb(210, 218, 232);
            var uiFont    = new Font("Segoe UI", 9f);
            var labelFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlIcon = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSub = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.pnlFileRow = new System.Windows.Forms.Panel();
            this.lblFile = new System.Windows.Forms.Label();
            this.txtFiles = new System.Windows.Forms.TextBox();
            this.pnlShareRow = new System.Windows.Forms.Panel();
            this.lblWith = new System.Windows.Forms.Label();
            this.cboPrincipalType = new System.Windows.Forms.ComboBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.pnlDual = new System.Windows.Forms.Panel();
            this.innerTable = new System.Windows.Forms.TableLayoutPanel();
            this.lblLeftHeader = new System.Windows.Forms.Label();
            this.lblRightHeader = new System.Windows.Forms.Label();
            this.pnlLeftBorder = new System.Windows.Forms.Panel();
            this.lstPrincipal = new System.Windows.Forms.ListBox();
            this.pnlRightBorder = new System.Windows.Forms.Panel();
            this.lstSelected = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.barPerm = new System.Windows.Forms.Label();
            this.pnlPerms = new System.Windows.Forms.FlowLayoutPanel();
            this.chkRead = MakePermChip("👁  ดู", navyLight, navyDark, true);
            this.chkWrite = MakePermChip("✏  แก้ไข", navyLight, navyDark, false);
            this.chkDelete = MakePermChip("🗑  ลบ", navyLight, navyDark, false);
            this.barExpiry = new System.Windows.Forms.Label();
            this.pnlExpiryRow = new System.Windows.Forms.Panel();
            this.dtpExpiry = new System.Windows.Forms.DateTimePicker();
            this.chkNoExpiry = new System.Windows.Forms.CheckBox();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.tmrSearchDebounce = new System.Windows.Forms.Timer(this.components);
            this.toolTipInfo = new System.Windows.Forms.ToolTip(this.components);

            this.SuspendLayout();

            // ── Form ─────────────────────────────────────────────────────────
            this.Text            = "แบ่งปัน File / Folder — ภายใน";
            this.Font            = uiFont;
            this.ClientSize      = new System.Drawing.Size(520, 560);
            this.MinimumSize     = new System.Drawing.Size(520, 560);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = bodyBg;

            // ── Header ───────────────────────────────────────────────────────
            this.pnlHeader.Dock      = DockStyle.Top;
            this.pnlHeader.Height    = 60;
            this.pnlHeader.BackColor = navyDark;

            this.pnlIcon.Size      = new Size(36, 36);
            this.pnlIcon.Location  = new Point(14, 12);
            this.pnlIcon.BackColor = navyMid;

            this.lblTitle.Text      = "แบ่งปัน File / Folder — ภายใน";
            this.lblTitle.Font      = new Font("Segoe UI", 12f, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.AutoSize  = true;
            this.lblTitle.Location  = new Point(58, 10);

            this.lblSub.Text      = "กำหนดสิทธิ์การเข้าถึงให้ผู้ใช้ / แผนก / บทบาทในองค์กร";
            this.lblSub.Font      = new Font("Segoe UI", 8f);
            this.lblSub.ForeColor = Color.FromArgb(170, 200, 240);
            this.lblSub.AutoSize  = true;
            this.lblSub.Location  = new Point(59, 34);

            this.pnlHeader.Controls.AddRange(new Control[] { this.pnlIcon, this.lblTitle, this.lblSub });

            // ── Footer ───────────────────────────────────────────────────────
            this.pnlFooter.Dock      = DockStyle.Bottom;
            this.pnlFooter.Height    = 52;
            this.pnlFooter.BackColor = footerBg;
            this.pnlFooter.Padding   = new Padding(10, 10, 14, 10);

            this.lblStatus.Text      = "";
            this.lblStatus.Font      = new Font("Segoe UI", 8.5f);
            this.lblStatus.ForeColor = Color.FromArgb(180, 30, 30);
            this.lblStatus.AutoSize  = false;
            this.lblStatus.Size      = new Size(290, 32);
            this.lblStatus.Location  = new Point(12, 10);
            this.lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            this.btnOK.Text = "✔  ตกลง";
            this.btnOK.Size = new Size(96, 32);
            this.btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnOK.FlatStyle = FlatStyle.Flat;
            this.btnOK.BackColor = navyDark;
            this.btnOK.ForeColor = Color.White;
            this.btnOK.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.btnOK.FlatAppearance.BorderSize = 0;
            this.btnOK.Location = new Point(310, 10);

            this.btnCancel.Text = "✕  ยกเลิก";
            this.btnCancel.Size = new Size(96, 32);
            this.btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.BackColor = Color.FromArgb(220, 225, 235);
            this.btnCancel.ForeColor = Color.FromArgb(50, 60, 80);
            this.btnCancel.Font = new Font("Segoe UI", 9f);
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = borderGray;
            this.btnCancel.FlatAppearance.BorderSize  = 1;
            this.btnCancel.Location = new Point(412, 10);

            this.pnlFooter.Controls.AddRange(new Control[] { this.lblStatus, this.btnOK, this.btnCancel });

            // ── Body ─────────────────────────────────────────────────────────
            this.pnlBody.Dock    = DockStyle.Fill;
            this.pnlBody.Padding = new Padding(16, 12, 16, 0);
            this.pnlBody.BackColor = bodyBg;

            int bodyW = 520 - 32;

            // ── Row: File / Folder ────────────────────────────────────────────
            this.pnlFileRow.Height = 66;
            this.pnlFileRow.Dock = DockStyle.Top;
            this.pnlFileRow.BackColor = Color.Transparent;
            this.pnlFileRow.Padding = new Padding(0, 4, 0, 0);

            this.lblFile.Text      = "File / Folder :";
            this.lblFile.Font      = uiFont;
            this.lblFile.ForeColor = Color.FromArgb(55, 65, 85);
            this.lblFile.AutoSize  = false;
            this.lblFile.Width     = 108;
            this.lblFile.Height    = 56;
            this.lblFile.Location  = new Point(0, 4);
            this.lblFile.TextAlign = ContentAlignment.TopRight;

            this.txtFiles.ReadOnly    = true;
            this.txtFiles.Multiline   = true;
            this.txtFiles.Font        = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.txtFiles.ForeColor   = navyDark;
            this.txtFiles.BackColor   = Color.FromArgb(243, 246, 251);
            this.txtFiles.BorderStyle = BorderStyle.None;
            this.txtFiles.ScrollBars  = ScrollBars.Vertical;
            this.txtFiles.Location    = new Point(114, 4);
            this.txtFiles.Width       = bodyW - 114;
            this.txtFiles.Height      = 56;

            this.pnlFileRow.Controls.AddRange(new Control[] { this.lblFile, this.txtFiles });

            // ── Row: แบ่งปันให้ + ช่องค้นหา ──────────────────────────────────
            this.pnlShareRow.Height = 36;
            this.pnlShareRow.Dock = DockStyle.Top;
            this.pnlShareRow.BackColor = Color.Transparent;

            this.lblWith.Text      = "แบ่งปันให้ :";
            this.lblWith.Font      = uiFont;
            this.lblWith.ForeColor = Color.FromArgb(55, 65, 85);
            this.lblWith.AutoSize  = false;
            this.lblWith.Width     = 108;
            this.lblWith.Height    = 22;
            this.lblWith.Location  = new Point(0, 7);
            this.lblWith.TextAlign = ContentAlignment.MiddleRight;

            this.cboPrincipalType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboPrincipalType.Font = uiFont;
            this.cboPrincipalType.Location = new Point(114, 6);
            this.cboPrincipalType.Width = 142;
            this.cboPrincipalType.FlatStyle = FlatStyle.Flat;
            this.cboPrincipalType.BackColor = Color.White;
            this.cboPrincipalType.Items.AddRange(new[] { "User", "Department", "Role", "Group" });
            this.cboPrincipalType.SelectedIndex = 0;

            this.txtSearch.Font = uiFont;
            this.txtSearch.Location = new Point(262, 6);
            this.txtSearch.Width = bodyW - 262; // ยืดตามความกว้างแถว
            this.txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.txtSearch.BorderStyle = BorderStyle.FixedSingle;
            this.txtSearch.BackColor = Color.White;

            this.pnlShareRow.Controls.AddRange(new Control[] { this.lblWith, this.cboPrincipalType, this.txtSearch });

            // ── Left: Source list ─────────────────────────────────────────────
            this.lstPrincipal.Font = uiFont;
            this.lstPrincipal.IntegralHeight = false;
            this.lstPrincipal.Dock = DockStyle.Fill;
            this.lstPrincipal.SelectionMode = SelectionMode.One;
            this.lstPrincipal.BorderStyle = BorderStyle.None;
            this.lstPrincipal.BackColor = Color.White;
            this.lstPrincipal.ItemHeight = 24;

            this.pnlLeftBorder.Dock = DockStyle.Fill;
            this.pnlLeftBorder.BackColor = Color.White;
            this.pnlLeftBorder.Padding = new Padding(1);
            this.pnlLeftBorder.Controls.Add(this.lstPrincipal);

            // ── Right: Selected basket ────────────────────────────────────────
            this.lstSelected.Font = uiFont;
            this.lstSelected.IntegralHeight = false;
            this.lstSelected.Dock = DockStyle.Fill;
            this.lstSelected.SelectionMode = SelectionMode.MultiExtended;
            this.lstSelected.BorderStyle = BorderStyle.None;
            this.lstSelected.BackColor = Color.FromArgb(248, 252, 255);
            this.lstSelected.ItemHeight = 24;

            this.pnlRightBorder.Dock = DockStyle.Fill;
            this.pnlRightBorder.BackColor = Color.FromArgb(248, 252, 255);
            this.pnlRightBorder.Padding = new Padding(1);
            this.pnlRightBorder.Controls.Add(this.lstSelected);

            this.lblLeftHeader.Text = "  เลือกเป้าหมาย";
            this.lblLeftHeader.Font = labelFont;
            this.lblLeftHeader.ForeColor = navyDark;
            this.lblLeftHeader.BackColor = navyLight;
            this.lblLeftHeader.Dock = DockStyle.Fill;
            this.lblLeftHeader.TextAlign = ContentAlignment.MiddleLeft;

            this.lblRightHeader.Text = "  รายชื่อที่เลือก (0 คน)";
            this.lblRightHeader.Font = labelFont;
            this.lblRightHeader.ForeColor = navyDark;
            this.lblRightHeader.BackColor = Color.FromArgb(220, 235, 255);
            this.lblRightHeader.Dock = DockStyle.Fill;
            this.lblRightHeader.TextAlign = ContentAlignment.MiddleLeft;

            this.btnAdd.Text = "เพิ่ม ›";
            this.btnAdd.Dock = DockStyle.Fill;
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.BackColor = Color.FromArgb(230, 240, 255);
            this.btnAdd.ForeColor = navyDark;
            this.btnAdd.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            this.btnAdd.FlatAppearance.BorderColor = Color.FromArgb(180, 205, 240);
            this.btnAdd.FlatAppearance.BorderSize = 1;

            this.btnRemove.Text = "✕  ลบที่เลือก";
            this.btnRemove.Dock = DockStyle.Fill;
            this.btnRemove.FlatStyle = FlatStyle.Flat;
            this.btnRemove.BackColor = Color.FromArgb(255, 235, 235);
            this.btnRemove.ForeColor = Color.FromArgb(180, 40, 40);
            this.btnRemove.Font = new Font("Segoe UI", 8.5f);
            this.btnRemove.FlatAppearance.BorderColor = Color.FromArgb(220, 180, 180);
            this.btnRemove.FlatAppearance.BorderSize  = 1;

            // ── Inner 2-column table (source | basket) ────────────────────────
            this.innerTable.Dock = DockStyle.Fill;
            this.innerTable.ColumnCount = 2;
            this.innerTable.RowCount = 3;
            this.innerTable.BackColor = Color.Transparent;
            this.innerTable.Padding = new Padding(0);
            this.innerTable.Margin = new Padding(0);

            this.innerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            this.innerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            this.innerTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            this.innerTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.innerTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            this.innerTable.Controls.Add(this.lblLeftHeader,  0, 0);
            this.innerTable.Controls.Add(this.lblRightHeader, 1, 0);
            this.innerTable.Controls.Add(this.pnlLeftBorder,  0, 1);
            this.innerTable.Controls.Add(this.pnlRightBorder, 1, 1);
            this.innerTable.Controls.Add(this.btnAdd,         0, 2);
            this.innerTable.Controls.Add(this.btnRemove,       1, 2);

            this.pnlDual.Dock = DockStyle.Fill;
            this.pnlDual.BackColor = Color.Transparent;
            this.pnlDual.Padding = new Padding(0, 2, 0, 2);
            this.pnlDual.Controls.Add(this.innerTable);

            // ── Section: สิทธิ์ ───────────────────────────────────────────────
            this.barPerm.Text = "  สิทธิ์การเข้าถึง";
            this.barPerm.Font = labelFont;
            this.barPerm.ForeColor = navyDark;
            this.barPerm.BackColor = navyLight;
            this.barPerm.Dock = DockStyle.None;
            this.barPerm.Height = 26;
            this.barPerm.TextAlign = ContentAlignment.MiddleLeft;

            this.pnlPerms.Dock = DockStyle.Fill; // แสดงปุ่มสิทธิ์ทั้งหมดครบถ้วน
            this.pnlPerms.Height = 36;
            this.pnlPerms.FlowDirection = FlowDirection.LeftToRight;
            this.pnlPerms.BackColor = Color.Transparent;
            this.pnlPerms.WrapContents = false;
            this.pnlPerms.Controls.AddRange(new Control[] { this.chkRead, this.chkWrite, this.chkDelete });

            // ── Section: วันหมดอายุ ───────────────────────────────────────────
            this.barExpiry.Text = "  วันหมดอายุ";
            this.barExpiry.Font = labelFont;
            this.barExpiry.ForeColor = navyDark;
            this.barExpiry.BackColor = navyLight;
            this.barExpiry.Dock = DockStyle.None;
            this.barExpiry.Height = 26;
            this.barExpiry.TextAlign = ContentAlignment.MiddleLeft;

            this.dtpExpiry.Format = DateTimePickerFormat.Custom;
            this.dtpExpiry.CustomFormat = "dd/MM/yyyy";
            this.dtpExpiry.Font = uiFont;
            this.dtpExpiry.Width = 160;
            this.dtpExpiry.CalendarForeColor = navyDark;
            this.dtpExpiry.Value   = DateTime.Today.AddDays(30);
            this.dtpExpiry.MinDate = DateTime.Today;
            this.dtpExpiry.MaxDate = DateTime.Today.AddYears(1);
            this.dtpExpiry.Enabled = false;

            this.chkNoExpiry.Text      = "ไม่มีกำหนดอายุ";
            this.chkNoExpiry.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.chkNoExpiry.ForeColor = Color.FromArgb(0, 120, 50);
            this.chkNoExpiry.Checked   = true;
            this.chkNoExpiry.AutoSize  = true;

            this.pnlExpiryRow.Dock = DockStyle.Fill;
            this.pnlExpiryRow.BackColor = Color.Transparent;
            this.dtpExpiry.Location   = new Point(0, 7);
            this.chkNoExpiry.Location = new Point(170, 9);
            this.pnlExpiryRow.Controls.AddRange(new Control[] { this.dtpExpiry, this.chkNoExpiry });

            // ── Outer TableLayoutPanel ────────────────────────────────────────
            this.table.Dock = DockStyle.Fill;
            this.table.ColumnCount = 1;
            this.table.RowCount = 8;
            this.table.BackColor = Color.Transparent;
            this.table.Padding = new Padding(0);

            int[] heights = { 68, 36, 6, 190, 28, 40, 28, 40 };
            foreach (int h in heights)
                this.table.RowStyles.Add(new RowStyle(SizeType.Absolute, h));

            this.table.Controls.Add(this.pnlFileRow,   0, 0);
            this.table.Controls.Add(this.pnlShareRow,  0, 1);
            this.table.Controls.Add(new Panel { BackColor = borderGray, Height = 1, Dock = DockStyle.Bottom }, 0, 2);
            this.table.Controls.Add(this.pnlDual,      0, 3);
            this.table.Controls.Add(this.barPerm,      0, 4);
            this.table.Controls.Add(this.pnlPerms,     0, 5);
            this.table.Controls.Add(this.barExpiry,    0, 6);
            this.table.Controls.Add(this.pnlExpiryRow, 0, 7);

            this.pnlBody.Controls.Add(this.table);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);

            this.tmrSearchDebounce.Interval = 250;

            this.pnlIcon.Paint += new PaintEventHandler(this.pnlIcon_Paint);
            this.pnlLeftBorder.Paint += new PaintEventHandler(this.pnlLeftBorder_Paint);
            this.pnlRightBorder.Paint += new PaintEventHandler(this.pnlRightBorder_Paint);
            this.pnlFooter.Resize += new EventHandler(this.pnlFooter_Resize);

            this.ResumeLayout(false);
        }

        private static CheckBox MakePermChip(string text, Color bgChecked, Color fg, bool isChecked)
        {
            Color bgOff = Color.FromArgb(240, 240, 245);
            Color fgOff = Color.FromArgb(140, 145, 160);
            var chk = new CheckBox
            {
                Text = text, Checked = isChecked,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = isChecked ? fg : fgOff,
                AutoSize = false, Size = new Size(96, 30),
                Margin = new Padding(0, 4, 10, 0),
                Appearance = Appearance.Button, FlatStyle = FlatStyle.Flat,
                BackColor = isChecked ? bgChecked : bgOff,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            chk.FlatAppearance.BorderColor        = Color.FromArgb(180, 200, 235);
            chk.FlatAppearance.CheckedBackColor   = bgChecked;
            chk.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 215, 240);
            chk.CheckedChanged += (s, e) =>
            {
                chk.BackColor = chk.Checked ? bgChecked : bgOff;
                chk.ForeColor = chk.Checked ? fg        : fgOff;
            };
            return chk;
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlIcon;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Panel pnlFileRow;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.TextBox txtFiles;
        private System.Windows.Forms.Panel pnlShareRow;
        private System.Windows.Forms.Label lblWith;
        private System.Windows.Forms.ComboBox cboPrincipalType;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Panel pnlDual;
        private System.Windows.Forms.TableLayoutPanel innerTable;
        private System.Windows.Forms.Label lblLeftHeader;
        private System.Windows.Forms.Label lblRightHeader;
        private System.Windows.Forms.Panel pnlLeftBorder;
        private System.Windows.Forms.ListBox lstPrincipal;
        private System.Windows.Forms.Panel pnlRightBorder;
        private System.Windows.Forms.ListBox lstSelected;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label barPerm;
        private System.Windows.Forms.FlowLayoutPanel pnlPerms;
        private System.Windows.Forms.CheckBox chkRead;
        private System.Windows.Forms.CheckBox chkWrite;
        private System.Windows.Forms.CheckBox chkDelete;
        private System.Windows.Forms.Label barExpiry;
        private System.Windows.Forms.Panel pnlExpiryRow;
        private System.Windows.Forms.DateTimePicker dtpExpiry;
        private System.Windows.Forms.CheckBox chkNoExpiry;
        private System.Windows.Forms.TableLayoutPanel table;
        private System.Windows.Forms.Timer tmrSearchDebounce;
        private System.Windows.Forms.ToolTip toolTipInfo;
    }
}
