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
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog for creating internal shares with specific users, departments, roles, or groups.
    /// Satisfies Requirements 1.1, 1.2, 1.3
    /// </summary>
    public partial class ShareDialogInternal : Form
    {
        private readonly IShareService _shareService;
        private readonly IFileService _fileService;
        private readonly IFolderService _folderService;
        private readonly IPermissionService _permissionService;
        private readonly IUserAccountLookupController _userAccountLookupController;
        private readonly IOrgStructureService _orgStructureService;
        private readonly UserContext _currentUser;

        private FileDocument _selectedFile;
        private Folder _selectedFolder;
        private Dictionary<string, object> _controls;

        public ShareDialogInternal(
            IShareService shareService,
            IFileService fileService,
            IFolderService folderService,
            IPermissionService permissionService,
            IUserAccountLookupController userAccountLookupController,
            IOrgStructureService orgStructureService,
            UserContext currentUser,
            FileDocument file = null,
            Folder folder = null)
        {
            InitializeComponent();
            _shareService = shareService;
            _fileService = fileService;
            _folderService = folderService;
            _permissionService = permissionService;
            _userAccountLookupController = userAccountLookupController;
            _orgStructureService = orgStructureService;
            _currentUser = currentUser;
            _selectedFile = file;
            _selectedFolder = folder;

            InitializeControls();
        }

        private void InitializeComponent()
        {
            // ── Palette (ใช้แบบเดียวกับ UserPermissionSummaryForm) ───────────
            Color navyDark    = Color.FromArgb(25,  55,  105);
            Color navyMid     = Color.FromArgb(50,  90,  150);
            Color navyLight   = Color.FromArgb(215, 228, 248);
            Color bodyBg      = Color.FromArgb(243, 246, 251);
            Color footerBg    = Color.FromArgb(228, 234, 245);
            Color borderGray  = Color.FromArgb(210, 218, 232);
            var   uiFont      = new Font("Segoe UI", 9f);
            var   labelFont   = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            this.SuspendLayout();
            this.Text            = "แบ่งปัน File / Folder — ภายใน";
            this.Font            = uiFont;
            this.ClientSize      = new Size(520, 480);
            this.MinimumSize     = new Size(520, 480);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = bodyBg;

            // ════════════════════════════════════════════════════════════════
            // Header — dark blue bar
            // ════════════════════════════════════════════════════════════════
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 60,
                BackColor = navyDark,
            };

            // icon circle
            var pnlIcon = new Panel { Size = new Size(36, 36), Location = new Point(14, 12), BackColor = navyMid };
            pnlIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(navyMid), 0, 0, 35, 35);
                using (var f = new Font("Segoe UI", 15f, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString("🔗", f, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
            };

            var lblTitle = new Label
            {
                Text      = "แบ่งปัน File / Folder — ภายใน",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(58, 10),
            };
            var lblSub = new Label
            {
                Text      = "กำหนดสิทธิ์การเข้าถึงให้ผู้ใช้ / แผนก / บทบาทในองค์กร",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(170, 200, 240),
                AutoSize  = true,
                Location  = new Point(59, 34),
            };
            pnlHeader.Controls.AddRange(new Control[] { pnlIcon, lblTitle, lblSub });

            // ════════════════════════════════════════════════════════════════
            // Footer — buttons
            // ════════════════════════════════════════════════════════════════
            var pnlFooter = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = footerBg,
                Padding   = new Padding(10, 10, 14, 10),
            };

            var lblStatus = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(180, 30, 30),
                AutoSize  = false,
                Size      = new Size(290, 32),
                Location  = new Point(12, 10),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var btnOK = new Button
            {
                Text      = "✔  ตกลง",
                Size      = new Size(96, 32),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = navyDark,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Location = new Point(pnlFooter.Width - 210, 10);

            var btnCancel = new Button
            {
                Text         = "✕  ยกเลิก",
                Size         = new Size(96, 32),
                Anchor       = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle    = FlatStyle.Flat,
                BackColor    = Color.FromArgb(220, 225, 235),
                ForeColor    = Color.FromArgb(50, 60, 80),
                Font         = new Font("Segoe UI", 9f),
                DialogResult = DialogResult.Cancel,
            };
            btnCancel.FlatAppearance.BorderColor = borderGray;
            btnCancel.FlatAppearance.BorderSize  = 1;
            btnCancel.Location = new Point(pnlFooter.Width - 108, 10);

            pnlFooter.Controls.AddRange(new Control[] { lblStatus, btnOK, btnCancel });

            // ════════════════════════════════════════════════════════════════
            // Body — content area
            // ════════════════════════════════════════════════════════════════
            var pnlBody = new Panel
            {
                Dock      = DockStyle.Fill,
                Padding   = new Padding(16, 12, 16, 0),
                BackColor = bodyBg,
            };

            // helper: section bar
            Func<string, Label> mkBar = txt => new Label
            {
                Text      = "  " + txt,
                Font      = labelFont,
                ForeColor = navyDark,
                BackColor = navyLight,
                Dock      = DockStyle.Top,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleLeft,
            };

            // helper: row label
            Func<string, Label> mkLbl = txt => new Label
            {
                Text      = txt,
                Font      = uiFont,
                ForeColor = Color.FromArgb(55, 65, 85),
                AutoSize  = false,
                Width     = 108,
                Height    = 22,
                TextAlign = ContentAlignment.MiddleRight,
            };

            // ── Section: เป้าหมาย ────────────────────────────────────────────
            int bodyW = 520 - 16 - 16;   // ความกว้างใช้งานได้ใน body

            // File/Folder row
            var pnlFileRow = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            var lblFile    = mkLbl("File / Folder :");
            lblFile.Location = new Point(0, 7);
            var btnSelectFile = new Button
            {
                Text      = "เลือก...",
                Size      = new Size(76, 26),
                Location  = new Point(114, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(235, 240, 252),
                ForeColor = navyDark,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            };
            btnSelectFile.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
            btnSelectFile.Click += (s, e) => SelectFileOrFolder();

            var lblSelectedFile = new Label
            {
                Text      = "(ยังไม่ได้เลือก)",
                Font      = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 110, 140),
                AutoSize  = false,
                Location  = new Point(196, 7),
                Width     = bodyW - 196,
                Height    = 22,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            pnlFileRow.Controls.AddRange(new Control[] { lblFile, btnSelectFile, lblSelectedFile });

            // Share With row
            var pnlShareRow = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblWith = mkLbl("แบ่งปันให้ :");
            lblWith.Location = new Point(0, 7);
            var cboPrincipalType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = uiFont,
                Location      = new Point(114, 6),
                Width         = 200,
                FlatStyle     = FlatStyle.Flat,
                BackColor     = Color.White,
            };
            cboPrincipalType.Items.AddRange(new[] { "User", "Department", "Role", "Group" });
            cboPrincipalType.SelectedIndex = 0;
            pnlShareRow.Controls.AddRange(new Control[] { lblWith, cboPrincipalType });

            // ── Section bar: เลือกเป้าหมาย ─────────────────────────────────
            var barTarget = mkBar("เลือกเป้าหมาย");
            barTarget.Dock = DockStyle.None;

            // ListBox — custom draw
            var lstPrincipal = new ListBox
            {
                Font             = uiFont,
                IntegralHeight   = false,
                Height           = 110,
                Dock             = DockStyle.None,
                SelectionMode    = SelectionMode.MultiExtended,
                BorderStyle      = BorderStyle.None,
                BackColor        = Color.White,
                ItemHeight       = 24,
            };
            var pnlListBorder = new Panel
            {
                Height      = 112,
                Dock        = DockStyle.None,
                BackColor   = Color.White,
                Padding     = new Padding(1),
            };
            pnlListBorder.Paint += (s, e) =>
            {
                using (var pen = new Pen(borderGray, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlListBorder.Width - 1, pnlListBorder.Height - 1);
            };
            lstPrincipal.Dock = DockStyle.Fill;
            pnlListBorder.Controls.Add(lstPrincipal);

            // ── Section bar: สิทธิ์ ──────────────────────────────────────────
            var barPerm = mkBar("สิทธิ์การเข้าถึง");
            barPerm.Dock = DockStyle.None;

            // Permission chips
            var chkRead   = MakePermChip("👁  ดู",    navyLight, navyDark, true);
            var chkWrite  = MakePermChip("✏  แก้ไข", navyLight, navyDark, false);
            var chkDelete = MakePermChip("🗑  ลบ",    navyLight, navyDark, false);
            var pnlPerms  = new FlowLayoutPanel
            {
                Dock          = DockStyle.None,
                Height        = 36,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor     = Color.Transparent,
                WrapContents  = false,
            };
            pnlPerms.Controls.AddRange(new Control[] { chkRead, chkWrite, chkDelete });

            // ── Section bar: วันหมดอายุ ──────────────────────────────────────
            var barExpiry = mkBar("วันหมดอายุ");
            barExpiry.Dock = DockStyle.None;

            var dtpExpiry = new DateTimePicker
            {
                Format    = DateTimePickerFormat.Short,
                Font      = uiFont,
                Width     = 160,
                CalendarForeColor = navyDark,
            };
            dtpExpiry.Value   = DateTime.Now.AddDays(30);
            dtpExpiry.Enabled = false;

            var chkNoExpiry = new CheckBox
            {
                Text      = "ไม่มีกำหนด",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 50),
                Checked   = true,
                AutoSize  = true,
            };
            chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;

            var pnlExpiryRow = new Panel { Dock = DockStyle.None, Height = 36, BackColor = Color.Transparent };
            dtpExpiry.Location    = new Point(0, 7);
            chkNoExpiry.Location  = new Point(170, 9);
            pnlExpiryRow.Controls.AddRange(new Control[] { dtpExpiry, chkNoExpiry });

            // ── Layout tiling (top-down stacking) ───────────────────────────
            // ใช้ TableLayoutPanel ควบคุม layout ให้ไม่ overlap
            var table = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 1,
                RowCount    = 9,
                BackColor   = Color.Transparent,
                Padding     = new Padding(0),
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // rows: fileRow, shareRow, sep, barTarget, listBorder, barPerm, permsRow, barExpiry, expiryRow
            int[] heights = { 36, 36, 6, 28, 116, 28, 40, 28, 40 };
            foreach (int h in heights)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, h));

            table.Controls.Add(pnlFileRow,     0, 0);
            table.Controls.Add(pnlShareRow,    0, 1);
            table.Controls.Add(new Panel { BackColor = borderGray, Height = 1, Dock = DockStyle.Bottom }, 0, 2);
            table.Controls.Add(barTarget,      0, 3);
            table.Controls.Add(pnlListBorder,  0, 4);
            table.Controls.Add(barPerm,        0, 5);
            table.Controls.Add(pnlPerms,       0, 6);
            table.Controls.Add(barExpiry,      0, 7);
            table.Controls.Add(pnlExpiryRow,   0, 8);

            pnlBody.Controls.Add(table);

            // ── Add to form ─────────────────────────────────────────────────
            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);

            // Store references
            _controls = new Dictionary<string, object>
            {
                { "lblSelectedFile",  lblSelectedFile  },
                { "cboPrincipalType", cboPrincipalType },
                { "lstPrincipal",     lstPrincipal     },
                { "chkRead",          chkRead          },
                { "chkWrite",         chkWrite         },
                { "chkDelete",        chkDelete        },
                { "dtpExpiry",        dtpExpiry        },
                { "chkNoExpiry",      chkNoExpiry      },
                { "lblStatus",        lblStatus        },
            };

            btnOK.Click += (s, e) => HandleOK();
            cboPrincipalType.SelectedIndexChanged += (s, e) => LoadPrincipals();

            // Anchor footer buttons dynamically
            pnlFooter.Resize += (s, e) =>
            {
                btnOK.Left     = pnlFooter.Width - 210;
                btnCancel.Left = pnlFooter.Width - 108;
            };

            this.ResumeLayout();
        }

        /// <summary>สร้าง CheckBox แบบ chip style สำหรับ permission</summary>
        private static CheckBox MakePermChip(string text, Color bgChecked, Color fg, bool isChecked)
        {
            Color bgOff  = Color.FromArgb(240, 240, 245);
            Color fgOff  = Color.FromArgb(140, 145, 160);

            var chk = new CheckBox
            {
                Text      = text,
                Checked   = isChecked,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = isChecked ? fg : fgOff,
                AutoSize  = false,
                Size      = new Size(96, 30),
                Margin    = new Padding(0, 4, 10, 0),
                Appearance = Appearance.Button,
                FlatStyle  = FlatStyle.Flat,
                BackColor  = isChecked ? bgChecked : bgOff,
                TextAlign  = ContentAlignment.MiddleCenter,
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
            var lblSelectedFile = (Label)_controls["lblSelectedFile"];

            if (_selectedFile != null)
                lblSelectedFile.Text = string.Format("File: {0}", _selectedFile.OriginalFileName);
            else if (_selectedFolder != null)
                lblSelectedFile.Text = string.Format("Folder: {0}", _selectedFolder.FolderName);

            LoadPrincipals();
        }

        private void SelectFileOrFolder()
        {
            // Simplified: In a real implementation, show a file/folder browser dialog
            MessageBox.Show("File/Folder selection would be implemented here.", "Info");
        }

        private void LoadPrincipals()
        {
            var cboPrincipalType = (ComboBox)_controls["cboPrincipalType"];
            var lstPrincipal = (ListBox)_controls["lstPrincipal"];

            lstPrincipal.Items.Clear();

            string principalType = cboPrincipalType.SelectedItem?.ToString();

            try
            {
                if (principalType == "User")
                {
                    var users = _userAccountLookupController?.GetAllUsers() ?? Enumerable.Empty<UserAccount>();
                    foreach (var user in users)
                    {
                        lstPrincipal.Items.Add(new PrincipalItem { Id = user.UserID, Name = user.DisplayName ?? user.ADUserName, Data = user });
                    }
                }
                else if (principalType == "Department")
                {
                    var depts = _orgStructureService.GetHierarchy()
                        .Where(o => o.Tier == OrgUnitTier.Department)
                        .ToList();
                    foreach (var dept in depts)
                    {
                        lstPrincipal.Items.Add(new PrincipalItem { Id = dept.OrgUnitID, Name = dept.Name, Data = dept });
                    }
                }
                else if (principalType == "Role")
                {
                    // Load roles - simplified for now
                    lstPrincipal.Items.Add(new PrincipalItem { Id = 1, Name = "Admin", Data = null });
                    lstPrincipal.Items.Add(new PrincipalItem { Id = 2, Name = "Manager", Data = null });
                    lstPrincipal.Items.Add(new PrincipalItem { Id = 3, Name = "Officer", Data = null });
                }
                else if (principalType == "Group")
                {
                    // Load groups - simplified for now
                    lstPrincipal.Items.Add(new PrincipalItem { Id = 1, Name = "Finance Team", Data = null });
                    lstPrincipal.Items.Add(new PrincipalItem { Id = 2, Name = "HR Team", Data = null });
                    lstPrincipal.Items.Add(new PrincipalItem { Id = 3, Name = "IT Team", Data = null });
                }

                lstPrincipal.DisplayMember = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading principals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleOK()
        {
            var lstPrincipal = (ListBox)_controls["lstPrincipal"];
            var chkRead = (CheckBox)_controls["chkRead"];
            var chkWrite = (CheckBox)_controls["chkWrite"];
            var chkDelete = (CheckBox)_controls["chkDelete"];
            var dtpExpiry = (DateTimePicker)_controls["dtpExpiry"];
            var chkNoExpiry = (CheckBox)_controls["chkNoExpiry"];
            var lblStatus = (Label)_controls["lblStatus"];
            var cboPrincipalType = (ComboBox)_controls["cboPrincipalType"];

            // Validation
            if (_selectedFile == null && _selectedFolder == null)
            {
                lblStatus.Text = "Error: Please select a file or folder.";
                return;
            }

            if (lstPrincipal.SelectedItem == null)
            {
                lblStatus.Text = "Error: Please select a target.";
                return;
            }

            if (!chkRead.Checked && !chkWrite.Checked && !chkDelete.Checked)
            {
                lblStatus.Text = "Error: Please select at least one permission.";
                return;
            }

            try
            {
                var selectedPrincipal = (PrincipalItem)lstPrincipal.SelectedItem;
                string principalType = cboPrincipalType.SelectedItem?.ToString() ?? "User";
                int principalTypeId = GetPrincipalTypeId(principalType);

                if (_selectedFile != null)
                {
                    // Share ไฟล์ → ใช้ FilePermission
                    var request = new CreateInternalShareRequest
                    {
                        FileID        = _selectedFile.FileID,
                        PrincipalType = principalTypeId,
                        PrincipalID   = selectedPrincipal.Id,
                        CanRead       = chkRead.Checked,
                        CanWrite      = chkWrite.Checked,
                        CanDelete     = chkDelete.Checked,
                        ExpiryDate    = chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value
                    };
                    _shareService.CreateInternalShare(request, _currentUser);
                }
                else if (_selectedFolder != null)
                {
                    // Share Folder → ใช้ FolderPermission ผ่าน IPermissionService
                    var setReq = new CMS.Services.DTOs.SetPermissionRequest
                    {
                        FolderID       = _selectedFolder.FolderID,
                        PrincipalType  = (CMS.Domain.Enums.PrincipalType)principalTypeId,
                        PrincipalID    = selectedPrincipal.Id,
                        CanRead        = chkRead.Checked,
                        CanWrite       = chkWrite.Checked,
                        CanDelete      = chkDelete.Checked,
                        CanModify      = false,
                        CanFullControl = false,
                        IsDeny         = false
                    };
                    _permissionService.SetFolderPermission(setReq, _currentUser);
                }

                lblStatus.Text = "Success: Share created.";
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
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
            public int Id { get; set; }
            public string Name { get; set; }
            public object Data { get; set; }

            public override string ToString() => Name;
        }
    }
}
