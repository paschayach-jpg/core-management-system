using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog for creating external shares via GUID-based tokens.
    /// Satisfies Requirements 2.1, 2.2, 2.3, 2.4, 4.1, 4.2, 7.2
    /// </summary>
    public partial class ShareDialogExternal : Form
    {
        private readonly IShareService _shareService;
        private readonly IFileService _fileService;
        private readonly IFolderService _folderService;
        private readonly UserContext _currentUser;

        private FileDocument _selectedFile;
        private Folder _selectedFolder;
        private Guid _generatedToken;
        private Dictionary<string, object> _controls;

        public ShareDialogExternal(
            IShareService shareService,
            IFileService fileService,
            IFolderService folderService,
            UserContext currentUser,
            FileDocument file = null,
            Folder folder = null)
        {
            InitializeComponent();
            _shareService = shareService;
            _fileService = fileService;
            _folderService = folderService;
            _currentUser = currentUser;
            _selectedFile = file;
            _selectedFolder = folder;

            InitializeControls();
        }

        private void InitializeComponent()
        {
            // ── Palette ─────────────────────────────────────────────────────
            Color navyDark  = Color.FromArgb(25,  55,  105);
            Color navyLight = Color.FromArgb(215, 228, 248);
            Color bodyBg    = Color.FromArgb(243, 246, 251);
            Color footerBg  = Color.FromArgb(228, 234, 245);
            Color borderClr = Color.FromArgb(210, 218, 232);
            var   uiFont    = new Font("Segoe UI", 9f);
            var   boldFont  = new Font("Segoe UI", 9f, FontStyle.Bold);

            this.SuspendLayout();
            this.Text            = "แบ่งปัน File / Folder — ภายนอก";
            this.ClientSize      = new Size(540, 620);
            this.MinimumSize     = new Size(540, 620);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = bodyBg;
            this.Font            = uiFont;

            // ── Header ──────────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = navyDark };
            var lblTitle  = new Label
            {
                Text      = "🌐  แบ่งปัน File / Folder — ภายนอก",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(16, 10),
            };
            var lblSub = new Label
            {
                Text      = "สร้าง link สำหรับแบ่งปันกับบุคคลภายนอกองค์กรผ่าน token",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(170, 200, 240),
                AutoSize  = true,
                Location  = new Point(17, 36),
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // ── Footer ──────────────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = footerBg };

            var lblStatus = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(180, 30, 30),
                AutoSize  = false,
                Size      = new Size(290, 32),
                Location  = new Point(12, 9),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var btnOK = new Button
            {
                Text      = "🔗  สร้าง Link",
                Size      = new Size(110, 32),
                Location  = new Point(308, 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = navyDark,
                ForeColor = Color.White,
                Font      = boldFont,
            };
            btnOK.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text         = "✕  ยกเลิก",
                Size         = new Size(96, 32),
                Location     = new Point(426, 9),
                FlatStyle    = FlatStyle.Flat,
                BackColor    = Color.FromArgb(220, 225, 235),
                ForeColor    = Color.FromArgb(50, 60, 80),
                Font         = boldFont,
                DialogResult = DialogResult.Cancel,
            };
            btnCancel.FlatAppearance.BorderColor = borderClr;
            btnCancel.FlatAppearance.BorderSize  = 1;
            pnlFooter.Controls.AddRange(new Control[] { lblStatus, btnOK, btnCancel });

            // ── Helper: label ────────────────────────────────────────────────
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

            // ── Helper: section bar ──────────────────────────────────────────
            Func<string, Panel> mkBar = txt =>
            {
                var p = new Panel { Height = 26, BackColor = navyLight, Dock = DockStyle.Top };
                p.Controls.Add(new Label
                {
                    Text      = "  " + txt,
                    Font      = boldFont,
                    ForeColor = navyDark,
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                });
                return p;
            };

            // ── Helper: styled TextBox ───────────────────────────────────────
            Func<bool, int, TextBox> mkTxt = (multiline, h) => new TextBox
            {
                Font        = uiFont,
                Multiline   = multiline,
                Height      = multiline ? h : 24,
                BackColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };

            // ── File/Folder row ─────────────────────────────────────────────
            var barFile = mkBar("File / Folder เป้าหมาย");

            var pnlFileRow = new Panel { Height = 38, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblFile   = mkLbl("File / Folder :");
            lblFile.Location = new Point(8, 9);
            var btnSelectFile = new Button
            {
                Text      = "เลือก...",
                Size      = new Size(76, 26),
                Location  = new Point(122, 6),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(235, 240, 252),
                ForeColor = navyDark,
                Font      = boldFont,
            };
            btnSelectFile.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
            btnSelectFile.Click += (s, e) => SelectFileOrFolder();
            var lblSelectedFile = new Label
            {
                Text      = "(ยังไม่ได้เลือก)",
                Font      = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 110, 140),
                AutoSize  = false,
                Location  = new Point(204, 9),
                Width     = 320,
                Height    = 22,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            pnlFileRow.Controls.AddRange(new Control[] { lblFile, btnSelectFile, lblSelectedFile });

            // ── Password row ─────────────────────────────────────────────────
            var barSecurity = mkBar("ความปลอดภัย");

            var pnlPassRow = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblPass = mkLbl("รหัสผ่าน (ถ้ามี) :");
            lblPass.Location = new Point(8, 7);
            var txtPassword = mkTxt(false, 24);
            txtPassword.PasswordChar = '*';
            txtPassword.Location = new Point(122, 6);
            txtPassword.Width    = 380;
            pnlPassRow.Controls.AddRange(new Control[] { lblPass, txtPassword });

            // ── Expiry row ────────────────────────────────────────────────────
            var barExpiry = mkBar("วันหมดอายุ / จำกัดดาวน์โหลด");

            var pnlExpiryRow = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblExp = mkLbl("วันหมดอายุ :");
            lblExp.Location = new Point(8, 7);
            var dtpExpiry = new DateTimePicker
            {
                Format   = DateTimePickerFormat.Short,
                Font     = uiFont,
                Width    = 160,
                Location = new Point(122, 7),
            };
            dtpExpiry.Value = DateTime.Now.AddDays(30);
            var chkNoExpiry = new CheckBox
            {
                Text      = "ไม่มีกำหนด",
                Font      = boldFont,
                ForeColor = Color.FromArgb(0, 120, 50),
                AutoSize  = true,
                Location  = new Point(290, 10),
                Checked   = false,
            };
            chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;
            pnlExpiryRow.Controls.AddRange(new Control[] { lblExp, dtpExpiry, chkNoExpiry });

            // ── Download limit row ────────────────────────────────────────────
            var pnlLimitRow = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblLim = mkLbl("จำกัดดาวน์โหลด :");
            lblLim.Location = new Point(8, 7);
            var nudDownloadLimit = new NumericUpDown
            {
                Location = new Point(122, 7),
                Width    = 90,
                Minimum  = 1,
                Maximum  = 1000,
                Value    = 10,
                Font     = uiFont,
            };
            var chkNoLimit = new CheckBox
            {
                Text      = "ไม่จำกัด",
                Font      = boldFont,
                ForeColor = Color.FromArgb(0, 120, 50),
                AutoSize  = true,
                Location  = new Point(220, 10),
            };
            chkNoLimit.CheckedChanged += (s, e) => nudDownloadLimit.Enabled = !chkNoLimit.Checked;
            pnlLimitRow.Controls.AddRange(new Control[] { lblLim, nudDownloadLimit, chkNoLimit });

            // ── Purpose + Notify ─────────────────────────────────────────────
            var barPurpose = mkBar("วัตถุประสงค์ / การแจ้งเตือน");

            var pnlPurposeRow = new Panel { Height = 80, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblPurp = mkLbl("วัตถุประสงค์ :");
            lblPurp.Location = new Point(8, 6);
            var txtPurpose = mkTxt(true, 64);
            txtPurpose.Location = new Point(122, 4);
            txtPurpose.Width    = 380;
            txtPurpose.Height   = 64;
            pnlPurposeRow.Controls.AddRange(new Control[] { lblPurp, txtPurpose });

            var pnlNotifyRow = new Panel { Height = 32, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var chkNotifyOnAccess = new CheckBox
            {
                Text      = "🔔  แจ้งเตือนเมื่อมีผู้เข้าถึง",
                Font      = boldFont,
                ForeColor = navyDark,
                AutoSize  = true,
                Location  = new Point(122, 7),
                Checked   = true,
            };
            pnlNotifyRow.Controls.Add(chkNotifyOnAccess);

            // ── Result section (Share link + Security summary) ────────────────
            var barResult = mkBar("ผลลัพธ์ — Share Link");

            var pnlLinkRow = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblLnk = mkLbl("Share Link :");
            lblLnk.Location = new Point(8, 7);
            var txtShareLink = mkTxt(false, 24);
            txtShareLink.ReadOnly = true;
            txtShareLink.Location = new Point(122, 6);
            txtShareLink.Width    = 296;
            txtShareLink.BackColor = Color.FromArgb(245, 248, 255);
            var btnCopyLink = new Button
            {
                Text      = "📋  คัดลอก",
                Size      = new Size(88, 26),
                Location  = new Point(424, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 228, 245),
                ForeColor = navyDark,
                Font      = boldFont,
            };
            btnCopyLink.FlatAppearance.BorderColor = borderClr;
            btnCopyLink.FlatAppearance.BorderSize  = 1;
            btnCopyLink.Click += (s, e) => { if (!string.IsNullOrEmpty(txtShareLink.Text)) Clipboard.SetText(txtShareLink.Text); };
            pnlLinkRow.Controls.AddRange(new Control[] { lblLnk, txtShareLink, btnCopyLink });

            var txtSecuritySummary = mkTxt(true, 64);
            txtSecuritySummary.ReadOnly  = true;
            txtSecuritySummary.Dock      = DockStyle.Top;
            txtSecuritySummary.Height    = 72;
            txtSecuritySummary.BackColor = Color.FromArgb(245, 248, 255);
            txtSecuritySummary.Font      = new Font("Segoe UI", 8.5f);

            // ── TableLayoutPanel — stack sections top-down ───────────────────
            var table = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 1,
                RowCount    = 13,
                BackColor   = Color.Transparent,
                Padding     = new Padding(0),
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            int[] heights = { 26, 38, 26, 36, 26, 36, 36, 26, 80, 32, 26, 36, 72 };
            foreach (int h in heights) table.RowStyles.Add(new RowStyle(SizeType.Absolute, h));

            int r = 0;
            table.Controls.Add(barFile,           0, r++);
            table.Controls.Add(pnlFileRow,        0, r++);
            table.Controls.Add(barSecurity,       0, r++);
            table.Controls.Add(pnlPassRow,        0, r++);
            table.Controls.Add(barExpiry,         0, r++);
            table.Controls.Add(pnlExpiryRow,      0, r++);
            table.Controls.Add(pnlLimitRow,       0, r++);
            table.Controls.Add(barPurpose,        0, r++);
            table.Controls.Add(pnlPurposeRow,     0, r++);
            table.Controls.Add(pnlNotifyRow,      0, r++);
            table.Controls.Add(barResult,         0, r++);
            table.Controls.Add(pnlLinkRow,        0, r++);
            table.Controls.Add(txtSecuritySummary,0, r++);

            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 6, 12, 0), BackColor = bodyBg };
            pnlBody.Controls.Add(table);

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);

            // Store references
            _controls = new Dictionary<string, object>
            {
                { "lblSelectedFile",   lblSelectedFile   },
                { "txtPassword",       txtPassword       },
                { "dtpExpiry",         dtpExpiry         },
                { "chkNoExpiry",       chkNoExpiry       },
                { "nudDownloadLimit",  nudDownloadLimit  },
                { "chkNoLimit",        chkNoLimit        },
                { "txtPurpose",        txtPurpose        },
                { "chkNotifyOnAccess", chkNotifyOnAccess },
                { "lblStatus",         lblStatus         },
                { "txtShareLink",      txtShareLink      },
                { "txtSecuritySummary",txtSecuritySummary},
            };

            btnOK.Click += (s, e) => HandleOK();

            this.ResumeLayout();
        }

        private void InitializeControls()
        {
            var lblSelectedFile = (Label)_controls["lblSelectedFile"];

            if (_selectedFile != null)
                lblSelectedFile.Text = string.Format("File: {0}", _selectedFile.OriginalFileName);
            else if (_selectedFolder != null)
                lblSelectedFile.Text = string.Format("Folder: {0}", _selectedFolder.FolderName);
        }

        private void SelectFileOrFolder()
        {
            // Simplified: In a real implementation, show a file/folder browser dialog
            MessageBox.Show("File/Folder selection would be implemented here.", "Info");
        }

        private void HandleOK()
        {
            var txtPassword = (TextBox)_controls["txtPassword"];
            var dtpExpiry = (DateTimePicker)_controls["dtpExpiry"];
            var chkNoExpiry = (CheckBox)_controls["chkNoExpiry"];
            var nudDownloadLimit = (NumericUpDown)_controls["nudDownloadLimit"];
            var chkNoLimit = (CheckBox)_controls["chkNoLimit"];
            var txtPurpose = (TextBox)_controls["txtPurpose"];
            var chkNotifyOnAccess = (CheckBox)_controls["chkNotifyOnAccess"];
            var lblStatus = (Label)_controls["lblStatus"];
            var txtShareLink = (TextBox)_controls["txtShareLink"];
            var txtSecuritySummary = (TextBox)_controls["txtSecuritySummary"];

            // Validation
            if (_selectedFile == null && _selectedFolder == null)
            {
                lblStatus.Text = "Error: Please select a file or folder.";
                return;
            }

            if (!chkNoExpiry.Checked)
            {
                var duration = (dtpExpiry.Value - DateTime.Now).TotalDays;
                if (duration > 90)
                {
                    lblStatus.Text = "Error: Expiry date exceeds maximum allowed duration of 90 days.";
                    return;
                }
            }

            if (!chkNoLimit.Checked && nudDownloadLimit.Value <= 0)
            {
                lblStatus.Text = "Error: Download limit must be greater than 0.";
                return;
            }

            try
            {
                var request = new CreateExternalShareRequest
                {
                    FileID = _selectedFile?.FileID,
                    FolderID = _selectedFolder?.FolderID,
                    Password = string.IsNullOrEmpty(txtPassword.Text) ? null : txtPassword.Text,
                    ExpiryDate = chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value,
                    DownloadLimit = chkNoLimit.Checked ? (int?)null : (int)nudDownloadLimit.Value,
                    SharePurpose = txtPurpose.Text,
                    NotifyOnAccess = chkNotifyOnAccess.Checked
                };

                var result = _shareService.CreateExternalShare(request, _currentUser);
                _generatedToken = result.ShareToken;

                // Display share link and security settings
                string shareLink = $"https://cms.example.com/share/{_generatedToken}";
                txtShareLink.Text = shareLink;

                var securitySummary = new System.Text.StringBuilder();
                securitySummary.AppendLine("Security Settings:");
                securitySummary.AppendLine($"  Password Protected: {(request.Password != null ? "Yes" : "No")}");
                securitySummary.AppendLine($"  Expiry Date: {(request.ExpiryDate.HasValue ? request.ExpiryDate.Value.ToShortDateString() : "Never")}");
                securitySummary.AppendLine($"  Download Limit: {(request.DownloadLimit.HasValue ? request.DownloadLimit.Value.ToString() : "Unlimited")}");
                securitySummary.AppendLine($"  Status: Pending Approval");

                txtSecuritySummary.Text = securitySummary.ToString();
                lblStatus.Text = "Success: External share created and pending manager approval.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
            }
        }
    }
}
