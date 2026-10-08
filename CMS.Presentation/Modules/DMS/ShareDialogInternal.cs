using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Controllers.DMS.Document;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Services.DTOs;

namespace CMS.Presentation
{
    public partial class ShareDialogInternal : Form
    {
        private readonly IShareCommandController _shareCommandController;
        private readonly IUserAccountLookupController _userAccountLookupController;
        private readonly IUserAccessOrgLookupController _orgLookupController;
        private readonly IFolderReadLookupController _folderReadLookupController;
        private readonly UserContext _currentUser;

        private List<FileDocument> _selectedFiles;
        private Folder _selectedFolder;
        private Folder _parentFolder;
        private Dictionary<string, object> _controls;
        private List<PrincipalItem> _allPrincipals = new List<PrincipalItem>();

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        public ShareDialogInternal(
            IShareCommandController shareCommandController,
            IUserAccountLookupController userAccountLookupController,
            IUserAccessOrgLookupController orgLookupController,
            UserContext currentUser,
            IFolderReadLookupController folderReadLookupController = null,
            List<FileDocument> files = null,
            Folder folder = null,
            Folder parentFolder = null)
        {
            InitializeComponent();
            _shareCommandController = shareCommandController;
            _userAccountLookupController = userAccountLookupController;
            _orgLookupController = orgLookupController;
            _folderReadLookupController = folderReadLookupController;
            _currentUser = currentUser;
            _selectedFiles = files ?? new List<FileDocument>();
            _selectedFolder = folder;
            _parentFolder = parentFolder;

            InitializeControls();
        }

        private void InitializeComponent()
        {
            Color navyDark   = Color.FromArgb(25,  55,  105);
            Color navyMid    = Color.FromArgb(50,  90,  150);
            Color navyLight  = Color.FromArgb(215, 228, 248);
            Color bodyBg     = Color.FromArgb(243, 246, 251);
            Color footerBg   = Color.FromArgb(228, 234, 245);
            Color borderGray = Color.FromArgb(210, 218, 232);
            var uiFont    = new Font("Segoe UI", 9f);
            var labelFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            this.SuspendLayout();
            this.Text            = "แบ่งปัน File / Folder — ภายใน";
            this.Font            = uiFont;
            this.ClientSize      = new Size(520, 560);
            this.MinimumSize     = new Size(520, 560);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = bodyBg;

            // ── Header ───────────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = navyDark };
            var pnlIcon   = new Panel { Size = new Size(36, 36), Location = new Point(14, 12), BackColor = navyMid };
            pnlIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(navyMid), 0, 0, 35, 35);
                using (var f  = new Font("Segoe UI", 15f, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString("🔗", f, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
            };
            var lblTitle = new Label
            {
                Text      = "แบ่งปัน File / Folder — ภายใน",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(58, 10),
            };
            var lblSub = new Label
            {
                Text      = "กำหนดสิทธิ์การเข้าถึงให้ผู้ใช้ / แผนก / บทบาทในองค์กร",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(170, 200, 240), AutoSize = true, Location = new Point(59, 34),
            };
            pnlHeader.Controls.AddRange(new Control[] { pnlIcon, lblTitle, lblSub });

            // ── Footer ───────────────────────────────────────────────────────
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom, Height = 52, BackColor = footerBg,
                Padding = new Padding(10, 10, 14, 10),
            };
            var lblStatus = new Label
            {
                Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(180, 30, 30),
                AutoSize = false, Size = new Size(290, 32), Location = new Point(12, 10),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            var btnOK = new Button
            {
                Text = "✔  ตกลง", Size = new Size(96, 32),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat, BackColor = navyDark, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Location = new Point(pnlFooter.Width - 210, 10);
            var btnCancel = new Button
            {
                Text = "✕  ยกเลิก", Size = new Size(96, 32),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(220, 225, 235),
                ForeColor = Color.FromArgb(50, 60, 80), Font = new Font("Segoe UI", 9f),
                DialogResult = DialogResult.Cancel,
            };
            btnCancel.FlatAppearance.BorderColor = borderGray;
            btnCancel.FlatAppearance.BorderSize  = 1;
            btnCancel.Location = new Point(pnlFooter.Width - 108, 10);
            pnlFooter.Controls.AddRange(new Control[] { lblStatus, btnOK, btnCancel });

            // ── Body ─────────────────────────────────────────────────────────
            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 12, 16, 0), BackColor = bodyBg };

            Func<string, Label> mkBar = txt => new Label
            {
                Text = "  " + txt, Font = labelFont, ForeColor = navyDark, BackColor = navyLight,
                Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleLeft,
            };
            Func<string, Label> mkLbl = txt => new Label
            {
                Text = txt, Font = uiFont, ForeColor = Color.FromArgb(55, 65, 85),
                AutoSize = false, Width = 108, Height = 22, TextAlign = ContentAlignment.MiddleRight,
            };

            int bodyW = 520 - 32;

            // ── Row: File / Folder ────────────────────────────────────────────
            var pnlFileRow = new Panel { Height = 66, Dock = DockStyle.Top, BackColor = Color.Transparent, Padding = new Padding(0, 4, 0, 0) };
            var lblFile = mkLbl("File / Folder :");
            lblFile.Location  = new Point(0, 4);
            lblFile.Height    = 56;
            lblFile.TextAlign = ContentAlignment.TopRight;
            var txtFiles = new TextBox
            {
                ReadOnly = true, Multiline = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = navyDark, BackColor = Color.FromArgb(243, 246, 251),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(114, 4), Width = bodyW - 114, Height = 56,
            };
            pnlFileRow.Controls.AddRange(new Control[] { lblFile, txtFiles });

            // ── Row: แบ่งปันให้ + ช่องค้นหา ──────────────────────────────────
            var pnlShareRow = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblWith = mkLbl("แบ่งปันให้ :");
            lblWith.Location = new Point(0, 7);
            var cboPrincipalType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, Font = uiFont,
                Location = new Point(114, 6), Width = 142, FlatStyle = FlatStyle.Flat, BackColor = Color.White,
            };
            cboPrincipalType.Items.AddRange(new[] { "User", "Department", "Role", "Group" });
            cboPrincipalType.SelectedIndex = 0;
            var txtSearch = new TextBox
            {
                Font = uiFont, Location = new Point(262, 6), Width = 188,
                BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White,
            };
            pnlShareRow.Controls.AddRange(new Control[] { lblWith, cboPrincipalType, txtSearch });

            // ── Left: Source list ─────────────────────────────────────────────
            var lstPrincipal = new ListBox
            {
                Font = uiFont, IntegralHeight = false,
                Dock = DockStyle.Fill, SelectionMode = SelectionMode.One,
                BorderStyle = BorderStyle.None, BackColor = Color.White, ItemHeight = 24,
            };
            var pnlLeftBorder = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            pnlLeftBorder.Paint += (s, e) =>
            {
                using (var pen = new Pen(borderGray, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlLeftBorder.Width - 1, pnlLeftBorder.Height - 1);
            };
            lstPrincipal.Dock = DockStyle.Fill;
            pnlLeftBorder.Controls.Add(lstPrincipal);

            // ── Right: Selected basket ────────────────────────────────────────
            var lstSelected = new ListBox
            {
                Font = uiFont, IntegralHeight = false,
                Dock = DockStyle.Fill, SelectionMode = SelectionMode.MultiExtended,
                BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(248, 252, 255), ItemHeight = 24,
            };
            var pnlRightBorder = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 252, 255), Padding = new Padding(1) };
            pnlRightBorder.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(160, 195, 240), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlRightBorder.Width - 1, pnlRightBorder.Height - 1);
            };
            lstSelected.Dock = DockStyle.Fill;
            pnlRightBorder.Controls.Add(lstSelected);

            var lblLeftHeader = new Label
            {
                Text = "  เลือกเป้าหมาย", Font = labelFont, ForeColor = navyDark, BackColor = navyLight,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            };
            var lblRightHeader = new Label
            {
                Text = "  รายชื่อที่เลือก (0 คน)", Font = labelFont, ForeColor = navyDark,
                BackColor = Color.FromArgb(220, 235, 255),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            };

            var btnRemove = new Button
            {
                Text = "✕  ลบที่เลือก", Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 235, 235),
                ForeColor = Color.FromArgb(180, 40, 40), Font = new Font("Segoe UI", 8.5f),
            };
            btnRemove.FlatAppearance.BorderColor = Color.FromArgb(220, 180, 180);
            btnRemove.FlatAppearance.BorderSize  = 1;

            // ── Inner 2-column table (source | basket) ────────────────────────
            var innerTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3,
                BackColor = Color.Transparent, Padding = new Padding(0), Margin = new Padding(0),
            };
            innerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            innerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            innerTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            innerTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            innerTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            innerTable.Controls.Add(lblLeftHeader,  0, 0);
            innerTable.Controls.Add(lblRightHeader, 1, 0);
            innerTable.Controls.Add(pnlLeftBorder,  0, 1);
            innerTable.Controls.Add(pnlRightBorder, 1, 1);
            innerTable.Controls.Add(new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill }, 0, 2);
            innerTable.Controls.Add(btnRemove,       1, 2);

            var pnlDual = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 2, 0, 2) };
            pnlDual.Controls.Add(innerTable);

            // ── Section: สิทธิ์ ───────────────────────────────────────────────
            var barPerm = mkBar("สิทธิ์การเข้าถึง");
            barPerm.Dock = DockStyle.None;
            var chkRead   = MakePermChip("👁  ดู",    navyLight, navyDark, true);
            var chkWrite  = MakePermChip("✏  แก้ไข", navyLight, navyDark, false);
            var chkDelete = MakePermChip("🗑  ลบ",    navyLight, navyDark, false);
            var pnlPerms  = new FlowLayoutPanel
            {
                Dock = DockStyle.None, Height = 36, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent, WrapContents = false,
            };
            pnlPerms.Controls.AddRange(new Control[] { chkRead, chkWrite, chkDelete });

            // ── Section: วันหมดอายุ ───────────────────────────────────────────
            var barExpiry = mkBar("วันหมดอายุ");
            barExpiry.Dock = DockStyle.None;
            var dtpExpiry = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short, Font = uiFont,
                Width = 160, CalendarForeColor = navyDark,
            };
            dtpExpiry.Value   = DateTime.Today.AddDays(30);
            dtpExpiry.MinDate = DateTime.Today;
            dtpExpiry.MaxDate = DateTime.Today.AddYears(1);
            dtpExpiry.Enabled = false;
            var chkNoExpiry = new CheckBox
            {
                Text      = "ไม่มีกำหนดอายุ",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 50),
                Checked   = true,
                AutoSize  = true,
            };
            chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;

            var pnlExpiryRow = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            dtpExpiry.Location   = new Point(0, 7);
            chkNoExpiry.Location = new Point(170, 9);
            pnlExpiryRow.Controls.AddRange(new Control[] { dtpExpiry, chkNoExpiry });

            // ── Outer TableLayoutPanel ────────────────────────────────────────
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8,
                BackColor = Color.Transparent, Padding = new Padding(0),
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // rows: fileRow, shareRow, sep, dualPanel, barPerm, permsRow, barExpiry, expiryRow
            int[] heights = { 68, 36, 6, 190, 28, 40, 28, 40 };
            foreach (int h in heights)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, h));

            table.Controls.Add(pnlFileRow,   0, 0);
            table.Controls.Add(pnlShareRow,  0, 1);
            table.Controls.Add(new Panel { BackColor = borderGray, Height = 1, Dock = DockStyle.Bottom }, 0, 2);
            table.Controls.Add(pnlDual,      0, 3);
            table.Controls.Add(barPerm,      0, 4);
            table.Controls.Add(pnlPerms,     0, 5);
            table.Controls.Add(barExpiry,    0, 6);
            table.Controls.Add(pnlExpiryRow, 0, 7);

            pnlBody.Controls.Add(table);
            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);

            _controls = new Dictionary<string, object>
            {
                { "txtFiles",          txtFiles          },
                { "cboPrincipalType",  cboPrincipalType  },
                { "txtSearch",         txtSearch         },
                { "lstPrincipal",      lstPrincipal      },
                { "lstSelected",       lstSelected       },
                { "lblRightHeader",    lblRightHeader    },
                { "chkRead",           chkRead           },
                { "chkWrite",          chkWrite          },
                { "chkDelete",         chkDelete         },
                { "dtpExpiry",         dtpExpiry         },
                { "chkNoExpiry",       chkNoExpiry       },
                { "lblStatus",         lblStatus         },
            };

            btnOK.Click += (s, e) => HandleOK();

            cboPrincipalType.SelectedIndexChanged += (s, e) =>
            {
                _allPrincipals.Clear();
                ((ListBox)_controls["lstSelected"]).Items.Clear();
                UpdateSelectedCount();
                LoadPrincipals();
            };

            txtSearch.TextChanged += (s, e) => FilterPrincipals(((TextBox)_controls["txtSearch"]).Text);

            // คลิกซ้ายครั้งเดียว → เพิ่มไปยัง basket ขวา
            lstPrincipal.Click += (s, e) =>
            {
                if (lstPrincipal.SelectedItem is PrincipalItem item)
                    AddToSelected(item);
            };

            lstSelected.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                    RemoveSelectedItems();
            };

            var ctxMenu = new ContextMenu();
            ctxMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("ลบออก", (s, e) => RemoveSelectedItems()));
            lstSelected.ContextMenu = ctxMenu;

            btnRemove.Click += (s, e) => RemoveSelectedItems();

            pnlFooter.Resize += (s, e) =>
            {
                btnOK.Left     = pnlFooter.Width - 210;
                btnCancel.Left = pnlFooter.Width - 108;
            };

            this.Load += (s, e) => SendMessage(txtSearch.Handle, EM_SETCUEBANNER, 1, "ค้นหาชื่อ...");

            this.ResumeLayout();
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

        private void InitializeControls()
        {
            var txtFiles = (TextBox)_controls["txtFiles"];
            if (_selectedFiles.Count > 0)
            {
                txtFiles.Text = string.Join(Environment.NewLine,
                    _selectedFiles.Select(f => "📄  " + f.OriginalFileName));
            }
            else if (_selectedFolder != null)
            {
                txtFiles.Text = _parentFolder != null
                    ? "📁  " + _parentFolder.FolderName + "  ›  " + _selectedFolder.FolderName
                    : "📁  " + _selectedFolder.FolderName;
            }
            else
            {
                txtFiles.Text = "(ไม่ระบุ)";
            }

            LoadPrincipals();
        }

        private void LoadPrincipals()
        {
            var cboPrincipalType = (ComboBox)_controls["cboPrincipalType"];
            string principalType = cboPrincipalType.SelectedItem?.ToString();

            if (_allPrincipals.Count == 0)
            {
                try
                {
                    if (principalType == "User")
                    {
                        // Only active accounts — inactive rows include bulk-imported employee placeholders
                        // (e.g. "ชื่อ นามสกุล (- -)") that cannot log in and must not be share targets.
                        var users = (_userAccountLookupController?.GetAllUsers() ?? Enumerable.Empty<UserAccount>())
                            .Where(u => u.IsActive)
                            .OrderBy(u => u.DisplayName ?? u.ADUserName, StringComparer.CurrentCulture);
                        // Show "DisplayName (ADUserName)" so users can be told apart and searched by either.
                        foreach (var u in users)
                            _allPrincipals.Add(new PrincipalItem { Id = u.UserID, Name = FormatUserLabel(u), PrincipalType = "User", Data = u });
                    }
                    else if (principalType == "Department")
                    {
                        var depts = (_orgLookupController != null ? _orgLookupController.GetHierarchy() : Enumerable.Empty<OrgUnit>())
                            .Where(o => o.Tier == OrgUnitTier.Department)
                            .GroupBy(o => o.OrgUnitID)
                            .Select(g => g.First())
                            .GroupBy(o => o.Name, StringComparer.OrdinalIgnoreCase)
                            .Select(g => g.First())
                            .ToList();
                        foreach (var d in depts)
                            _allPrincipals.Add(new PrincipalItem { Id = d.OrgUnitID, Name = d.Name, PrincipalType = "Department", Data = d });
                    }
                    else if (principalType == "Role")
                    {
                        _allPrincipals.Add(new PrincipalItem { Id = 4, Name = "Admin",       PrincipalType = "Role" });
                        _allPrincipals.Add(new PrincipalItem { Id = 3, Name = "Manager",     PrincipalType = "Role" });
                        _allPrincipals.Add(new PrincipalItem { Id = 2, Name = "Sr. Officer", PrincipalType = "Role" });
                        _allPrincipals.Add(new PrincipalItem { Id = 1, Name = "Officer",     PrincipalType = "Role" });
                    }
                    else if (principalType == "Group")
                    {
                        _allPrincipals.Add(new PrincipalItem { Id = 1, Name = "Finance Team", PrincipalType = "Group" });
                        _allPrincipals.Add(new PrincipalItem { Id = 2, Name = "HR Team",      PrincipalType = "Group" });
                        _allPrincipals.Add(new PrincipalItem { Id = 3, Name = "IT Team",      PrincipalType = "Group" });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading principals: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            FilterPrincipals(((TextBox)_controls["txtSearch"]).Text);
        }

        private static string FormatUserLabel(UserAccount u)
        {
            bool hasDisplay = !string.IsNullOrWhiteSpace(u.DisplayName);
            bool hasLogin = !string.IsNullOrWhiteSpace(u.ADUserName);
            if (hasDisplay && hasLogin) return string.Format("{0} ({1})", u.DisplayName.Trim(), u.ADUserName.Trim());
            return hasDisplay ? u.DisplayName.Trim() : (hasLogin ? u.ADUserName.Trim() : "User " + u.UserID);
        }

        private void FilterPrincipals(string filter)
        {
            var lst = (ListBox)_controls["lstPrincipal"];
            lst.BeginUpdate();
            lst.Items.Clear();
            var items = string.IsNullOrWhiteSpace(filter)
                ? _allPrincipals
                : _allPrincipals.Where(p => p.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            foreach (var item in items)
                lst.Items.Add(item);
            lst.DisplayMember = "Name";
            lst.EndUpdate();
        }

        private void AddToSelected(PrincipalItem item)
        {
            var lst = (ListBox)_controls["lstSelected"];
            if (lst.Items.Cast<PrincipalItem>().Any(p => p.Id == item.Id && p.PrincipalType == item.PrincipalType))
                return;
            lst.Items.Add(item);
            UpdateSelectedCount();
        }

        private void RemoveSelectedItems()
        {
            var lst      = (ListBox)_controls["lstSelected"];
            var toRemove = lst.SelectedItems.Cast<PrincipalItem>().ToList();
            foreach (var item in toRemove)
                lst.Items.Remove(item);
            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            var lst = (ListBox)_controls["lstSelected"];
            var lbl = (Label)_controls["lblRightHeader"];
            lbl.Text = string.Format("  รายชื่อที่เลือก ({0} คน)", lst.Items.Count);
        }

        private void HandleOK()
        {
            var lstSelected      = (ListBox)_controls["lstSelected"];
            var chkRead          = (CheckBox)_controls["chkRead"];
            var chkWrite         = (CheckBox)_controls["chkWrite"];
            var chkDelete        = (CheckBox)_controls["chkDelete"];
            var dtpExpiry        = (DateTimePicker)_controls["dtpExpiry"];
            var chkNoExpiry      = (CheckBox)_controls["chkNoExpiry"];
            var lblStatus        = (Label)_controls["lblStatus"];
            var cboPrincipalType = (ComboBox)_controls["cboPrincipalType"];

            if (_selectedFiles.Count == 0 && _selectedFolder == null)
            {
                lblStatus.Text = "ไม่พบไฟล์หรือโฟลเดอร์ที่ต้องการแบ่งปัน";
                return;
            }

            if (lstSelected.Items.Count == 0)
            {
                lblStatus.Text = "กรุณาเลือกผู้รับสิทธิ์อย่างน้อย 1 รายการ";
                return;
            }

            if (!chkRead.Checked && !chkWrite.Checked && !chkDelete.Checked)
            {
                lblStatus.Text = "กรุณาเลือกสิทธิ์การเข้าถึงอย่างน้อย 1 รายการ";
                return;
            }

            if (!chkNoExpiry.Checked)
            {
                if (dtpExpiry.Value.Date < DateTime.Today)
                {
                    lblStatus.Text = "ไม่สามารถกำหนดวันหมดอายุย้อนหลังได้";
                    return;
                }
                if (dtpExpiry.Value.Date > DateTime.Today.AddYears(1))
                {
                    lblStatus.Text = "ไม่สามารถกำหนดอายุเกิน 1 ปีได้";
                    return;
                }
            }

            string principalType   = cboPrincipalType.SelectedItem?.ToString() ?? "User";
            int    principalTypeId = GetPrincipalTypeId(principalType);
            DateTime? expiry       = chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value;

            try
            {
                foreach (PrincipalItem principal in lstSelected.Items)
                {
                    if (_selectedFiles.Count > 0)
                    {
                        foreach (var file in _selectedFiles)
                        {
                            _shareCommandController.CreateInternalShare(new CreateInternalShareRequest
                            {
                                FileID        = file.FileID,
                                PrincipalType = principalTypeId,
                                PrincipalID   = principal.Id,
                                CanRead       = chkRead.Checked,
                                CanWrite      = chkWrite.Checked,
                                CanDelete     = chkDelete.Checked,
                                ExpiryDate    = expiry,
                            }, _currentUser);
                        }
                    }
                    else
                    {
                        var folderPermReq = new SetPermissionRequest
                        {
                            FolderID       = _selectedFolder.FolderID,
                            PrincipalType  = (PrincipalType)principalTypeId,
                            PrincipalID    = principal.Id,
                            CanRead        = chkRead.Checked,
                            CanWrite       = chkWrite.Checked,
                            CanDelete      = chkDelete.Checked,
                            CanModify      = false,
                            CanFullControl = false,
                            IsDeny         = false,
                        };

                        // Department shares (principalTypeId == 3) need approval from all roles
                        // because they grant visibility to an entire org unit.
                        bool folderNeedsApproval = _currentUser.AppRole < 3 || principalTypeId == 3;
                        if (folderNeedsApproval)
                            _shareCommandController.RequestFolderShareApproval(folderPermReq, _currentUser);
                        else
                            _shareCommandController.SetFolderPermission(folderPermReq, _currentUser);
                    }
                }

                bool needsApproval = _currentUser.AppRole < 3 || principalTypeId == 3;

                // Build target name for the message
                string targetName = _selectedFiles.Count > 0
                    ? (_selectedFiles.Count == 1
                        ? _selectedFiles[0].OriginalFileName
                        : string.Format("{0} ไฟล์", _selectedFiles.Count))
                    : (_selectedFolder != null ? _selectedFolder.FolderName : "—");

                // Build recipient summary
                var recipientNames = lstSelected.Items.Cast<PrincipalItem>()
                    .Select(p => p.Name).ToArray();
                string recipientSummary = recipientNames.Length <= 3
                    ? string.Join(", ", recipientNames)
                    : string.Format("{0}, {1} และอีก {2} รายการ",
                        recipientNames[0], recipientNames[1], recipientNames.Length - 2);

                // Build permission summary
                var perms = new System.Collections.Generic.List<string>();
                if (chkRead.Checked)   perms.Add("ดู");
                if (chkWrite.Checked)  perms.Add("แก้ไข");
                if (chkDelete.Checked) perms.Add("ลบ");
                string permSummary = string.Join(" / ", perms);

                string expiryText = chkNoExpiry.Checked
                    ? "ไม่มีกำหนดอายุ"
                    : string.Format("หมดอายุ {0}", expiry.Value.ToString("dd/MM/yyyy"));

                string msgTitle, msgBody;
                MessageBoxIcon msgIcon;

                if (needsApproval)
                {
                    msgTitle = "บันทึกคำขอแบ่งปันเรียบร้อยแล้ว";
                    msgBody  = string.Format(
                        "บันทึกคำขอแบ่งปันเรียบร้อยแล้ว และส่งให้ผู้มีอำนาจอนุมัติแล้ว\r\n\r\n" +
                        "   รายการ    : {0}\r\n" +
                        "   ผู้รับสิทธิ์ : {1}\r\n" +
                        "   สิทธิ์      : {2}\r\n" +
                        "   อายุสิทธิ์  : {3}\r\n\r\n" +
                        "สถานะ: รอการอนุมัติจากผู้มีอำนาจ\r\n" +
                        "ผู้รับจะสามารถเข้าถึงได้หลังจากได้รับการอนุมัติแล้ว",
                        targetName, recipientSummary, permSummary, expiryText);
                    msgIcon = MessageBoxIcon.Information;

                    lblStatus.Text = string.Format(
                        "ส่งคำขออนุมัติเรียบร้อยแล้ว ({0} รายการ) — รอผู้มีอำนาจอนุมัติ",
                        lstSelected.Items.Count);
                }
                else
                {
                    msgTitle = "แบ่งปันเรียบร้อยแล้ว";
                    msgBody  = string.Format(
                        "บันทึกการแบ่งปันเรียบร้อยแล้ว ผู้รับสามารถเข้าถึงได้ทันที\r\n\r\n" +
                        "   รายการ    : {0}\r\n" +
                        "   ผู้รับสิทธิ์ : {1}\r\n" +
                        "   สิทธิ์      : {2}\r\n" +
                        "   อายุสิทธิ์  : {3}",
                        targetName, recipientSummary, permSummary, expiryText);
                    msgIcon = MessageBoxIcon.Information;

                    lblStatus.Text = _selectedFiles.Count > 1
                        ? string.Format("แบ่งปันเรียบร้อยแล้ว ({0} ไฟล์, {1} ผู้รับ)", _selectedFiles.Count, lstSelected.Items.Count)
                        : string.Format("แบ่งปันเรียบร้อยแล้ว ({0} รายการ)", lstSelected.Items.Count);
                }

                MessageBox.Show(msgBody, msgTitle, MessageBoxButtons.OK, msgIcon);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "เกิดข้อผิดพลาด: " + ex.Message;
                MessageBox.Show(
                    "เกิดข้อผิดพลาดในการบันทึก:\r\n\r\n" + ex.Message,
                    "ไม่สามารถดำเนินการได้",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private int GetPrincipalTypeId(string principalType)
        {
            switch (principalType)
            {
                case "User":       return 1;
                case "Department": return 3;
                case "Role":       return 2;
                case "Group":      return 4;
                default:           return 1;
            }
        }

        private class PrincipalItem
        {
            public int    Id            { get; set; }
            public string Name          { get; set; }
            public string PrincipalType { get; set; }
            public object Data          { get; set; }
            public override string ToString() => Name;
        }
    }
}
