using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation
{
    public partial class ShareDialogExternal : Form
    {
        private readonly IShareCommandController _shareCommandController;
        private readonly UserContext _currentUser;

        private List<FileDocument> _selectedFiles;
        private Folder _selectedFolder;
        private Panel _pnlFileList;
        private Label _lblCount;
        private Dictionary<string, object> _controls;

        private const int FileRowHeight = 28;
        private const int FileRowWidth  = 500;

        public ShareDialogExternal(
            IShareCommandController shareCommandController,
            UserContext currentUser,
            List<FileDocument> files = null,
            Folder folder = null)
        {
            InitializeComponent();
            _shareCommandController = shareCommandController;
            _currentUser = currentUser;
            _selectedFiles = files != null ? new List<FileDocument>(files) : new List<FileDocument>();
            _selectedFolder = folder;

            InitializeControls();
        }

        private void InitializeComponent()
        {
            Color navyDark  = Color.FromArgb(25,  55,  105);
            Color navyLight = Color.FromArgb(215, 228, 248);
            Color bodyBg    = Color.FromArgb(243, 246, 251);
            Color footerBg  = Color.FromArgb(228, 234, 245);
            Color borderClr = Color.FromArgb(210, 218, 232);
            var   uiFont    = new Font("Segoe UI", 9f);
            var   boldFont  = new Font("Segoe UI", 9f, FontStyle.Bold);

            this.SuspendLayout();
            this.Text            = "แบ่งปัน File / Folder — ภายนอก";
            this.ClientSize      = new Size(540, 712);
            this.MinimumSize     = new Size(540, 712);
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

            // ── Helpers ─────────────────────────────────────────────────────
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
            Func<bool, int, TextBox> mkTxt = (multiline, h) => new TextBox
            {
                Font        = uiFont,
                Multiline   = multiline,
                Height      = multiline ? h : 24,
                BackColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };

            // ── File / Folder section ────────────────────────────────────────
            var barFile = mkBar("File / Folder เป้าหมาย");

            // Count label docked to right side of the section bar
            _lblCount = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(80, 100, 140),
                AutoSize  = false,
                Width     = 160,
                Height    = 26,
                Dock      = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                Padding   = new Padding(0, 0, 8, 0),
            };
            barFile.Controls.Add(_lblCount);

            // Scrollable list container — rows are positioned absolutely inside
            _pnlFileList = new Panel
            {
                Height      = 130,
                Dock        = DockStyle.Top,
                BackColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll  = true,
            };
            var pnlFileSection = _pnlFileList;

            // ── Password row ─────────────────────────────────────────────────
            var barSecurity  = mkBar("ความปลอดภัย");
            var pnlPassRow   = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblPass      = mkLbl("รหัสผ่าน (ถ้ามี) :");
            lblPass.Location = new Point(8, 7);
            var txtPassword  = mkTxt(false, 24);
            txtPassword.PasswordChar = '*';
            txtPassword.Location     = new Point(122, 6);
            txtPassword.Width        = 380;
            pnlPassRow.Controls.AddRange(new Control[] { lblPass, txtPassword });

            // ── Expiry row ────────────────────────────────────────────────────
            var barExpiry    = mkBar("วันหมดอายุ / จำกัดดาวน์โหลด");
            var pnlExpiryRow = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblExp       = mkLbl("วันหมดอายุ :");
            lblExp.Location  = new Point(8, 7);
            var dtpExpiry    = new DateTimePicker
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
            };
            chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;
            pnlExpiryRow.Controls.AddRange(new Control[] { lblExp, dtpExpiry, chkNoExpiry });

            // ── Download limit row ────────────────────────────────────────────
            var pnlLimitRow  = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblLim       = mkLbl("จำกัดดาวน์โหลด :");
            lblLim.Location  = new Point(8, 7);
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
            var barPurpose    = mkBar("วัตถุประสงค์ / การแจ้งเตือน");
            var pnlPurposeRow = new Panel { Height = 80, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblPurp       = mkLbl("วัตถุประสงค์ :");
            lblPurp.Location  = new Point(8, 6);
            var txtPurpose    = mkTxt(true, 64);
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

            // ── Result section ────────────────────────────────────────────────
            var barResult  = mkBar("ผลลัพธ์ — Share Link");
            var pnlLinkRow = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblLnk     = mkLbl("Share Link :");
            lblLnk.Location = new Point(8, 7);
            var txtShareLink = mkTxt(false, 24);
            txtShareLink.ReadOnly  = true;
            txtShareLink.Location  = new Point(122, 6);
            txtShareLink.Width     = 296;
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
            btnCopyLink.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtShareLink.Text))
                    Clipboard.SetText(txtShareLink.Text);
            };
            pnlLinkRow.Controls.AddRange(new Control[] { lblLnk, txtShareLink, btnCopyLink });

            var txtSecuritySummary = mkTxt(true, 64);
            txtSecuritySummary.ReadOnly  = true;
            txtSecuritySummary.Dock      = DockStyle.Top;
            txtSecuritySummary.Height    = 72;
            txtSecuritySummary.BackColor = Color.FromArgb(245, 248, 255);
            txtSecuritySummary.Font      = new Font("Segoe UI", 8.5f);

            // ── TableLayoutPanel ─────────────────────────────────────────────
            // Row heights: barFile(26), pnlFileSection(130), barSecurity(26), pnlPassRow(36),
            //              barExpiry(26), pnlExpiryRow(36), pnlLimitRow(36), barPurpose(26),
            //              pnlPurposeRow(80), pnlNotifyRow(32), barResult(26), pnlLinkRow(36),
            //              txtSecuritySummary(72) — total 592 px
            var table = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 1,
                RowCount    = 13,
                BackColor   = Color.Transparent,
                Padding     = new Padding(0),
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            int[] heights = { 26, 130, 26, 36, 26, 36, 36, 26, 80, 32, 26, 36, 72 };
            foreach (int h in heights) table.RowStyles.Add(new RowStyle(SizeType.Absolute, h));

            int r = 0;
            table.Controls.Add(barFile,            0, r++);
            table.Controls.Add(pnlFileSection,     0, r++);
            table.Controls.Add(barSecurity,        0, r++);
            table.Controls.Add(pnlPassRow,         0, r++);
            table.Controls.Add(barExpiry,          0, r++);
            table.Controls.Add(pnlExpiryRow,       0, r++);
            table.Controls.Add(pnlLimitRow,        0, r++);
            table.Controls.Add(barPurpose,         0, r++);
            table.Controls.Add(pnlPurposeRow,      0, r++);
            table.Controls.Add(pnlNotifyRow,       0, r++);
            table.Controls.Add(barResult,          0, r++);
            table.Controls.Add(pnlLinkRow,         0, r++);
            table.Controls.Add(txtSecuritySummary, 0, r++);

            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 6, 12, 0), BackColor = bodyBg };
            pnlBody.Controls.Add(table);

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);

            _controls = new Dictionary<string, object>
            {
                { "txtPassword",        txtPassword        },
                { "dtpExpiry",          dtpExpiry          },
                { "chkNoExpiry",        chkNoExpiry        },
                { "nudDownloadLimit",   nudDownloadLimit   },
                { "chkNoLimit",         chkNoLimit         },
                { "txtPurpose",         txtPurpose         },
                { "chkNotifyOnAccess",  chkNotifyOnAccess  },
                { "lblStatus",          lblStatus          },
                { "txtShareLink",       txtShareLink       },
                { "txtSecuritySummary", txtSecuritySummary },
            };

            btnOK.Click += (s, e) => HandleOK();
            this.ResumeLayout();
        }

        private void InitializeControls()
        {
            RefreshFileList();
        }

        private void RefreshFileList()
        {
            _pnlFileList.Controls.Clear();

            int y = 0;

            if (_selectedFolder != null)
            {
                _pnlFileList.Controls.Add(MakeFileRow("📁", _selectedFolder.FolderName, null, 0, y));
                y += FileRowHeight;
            }

            int rowIndex = _selectedFolder != null ? 1 : 0;
            foreach (var f in _selectedFiles)
            {
                _pnlFileList.Controls.Add(MakeFileRow("📄", f.OriginalFileName, f, rowIndex, y));
                y        += FileRowHeight;
                rowIndex++;
            }

            _pnlFileList.AutoScrollMinSize = new Size(0, y);

            int total = (_selectedFolder != null ? 1 : 0) + _selectedFiles.Count;
            _lblCount.Text = total == 0
                ? "  (ยังไม่ได้เลือก)"
                : string.Format("  จำนวน {0} รายการ  ", total);
        }

        private Panel MakeFileRow(string icon, string name, FileDocument file, int rowIndex, int yPos)
        {
            Color rowBg = rowIndex % 2 == 0
                ? Color.White
                : Color.FromArgb(240, 245, 255);

            var row = new Panel
            {
                Location  = new Point(0, yPos),
                Width     = FileRowWidth,
                Height    = FileRowHeight,
                BackColor = rowBg,
            };

            var lblIcon = new Label
            {
                Text      = icon,
                Location  = new Point(6, 5),
                Size      = new Size(22, 18),
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(60, 80, 130),
                BackColor = Color.Transparent,
            };

            var lblName = new Label
            {
                Text         = name,
                Location     = new Point(30, 5),
                Size         = new Size(FileRowWidth - 30 - 36, 18),
                Font         = new Font("Segoe UI", 9f),
                ForeColor    = Color.FromArgb(30, 40, 60),
                BackColor    = Color.Transparent,
                AutoEllipsis = true,
            };

            var btnRemove = new Button
            {
                Text      = "×",
                Location  = new Point(FileRowWidth - 32, 4),
                Size      = new Size(26, 20),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 30, 30),
                BackColor = Color.FromArgb(255, 240, 240),
                Cursor    = Cursors.Hand,
            };
            btnRemove.FlatAppearance.BorderColor = Color.FromArgb(210, 180, 180);
            btnRemove.FlatAppearance.BorderSize  = 1;

            if (file != null)
                btnRemove.Click += (s, e) => { _selectedFiles.Remove(file); RefreshFileList(); };
            else
                btnRemove.Click += (s, e) => { _selectedFolder = null; RefreshFileList(); };

            row.Controls.AddRange(new Control[] { lblIcon, lblName, btnRemove });
            return row;
        }

        private void HandleOK()
        {
            var txtPassword        = (TextBox)_controls["txtPassword"];
            var dtpExpiry          = (DateTimePicker)_controls["dtpExpiry"];
            var chkNoExpiry        = (CheckBox)_controls["chkNoExpiry"];
            var nudDownloadLimit   = (NumericUpDown)_controls["nudDownloadLimit"];
            var chkNoLimit         = (CheckBox)_controls["chkNoLimit"];
            var txtPurpose         = (TextBox)_controls["txtPurpose"];
            var chkNotifyOnAccess  = (CheckBox)_controls["chkNotifyOnAccess"];
            var lblStatus          = (Label)_controls["lblStatus"];
            var txtShareLink       = (TextBox)_controls["txtShareLink"];
            var txtSecuritySummary = (TextBox)_controls["txtSecuritySummary"];

            lblStatus.ForeColor = Color.FromArgb(180, 30, 30);

            if (_selectedFiles.Count == 0 && _selectedFolder == null)
            {
                lblStatus.Text = "กรุณาเลือกไฟล์หรือโฟลเดอร์อย่างน้อย 1 รายการ";
                return;
            }

            if (!chkNoExpiry.Checked && (dtpExpiry.Value - DateTime.Now).TotalDays > 90)
            {
                lblStatus.Text = "วันหมดอายุต้องไม่เกิน 90 วันนับจากวันนี้";
                return;
            }

            if (!chkNoLimit.Checked && nudDownloadLimit.Value <= 0)
            {
                lblStatus.Text = "จำนวนครั้งดาวน์โหลดต้องมากกว่า 0";
                return;
            }

            string   password      = string.IsNullOrEmpty(txtPassword.Text) ? null : txtPassword.Text;
            DateTime? expiry       = chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value;
            int?     downloadLimit = chkNoLimit.Checked  ? (int?)null      : (int)nudDownloadLimit.Value;
            string   purpose       = txtPurpose.Text;
            bool     notify        = chkNotifyOnAccess.Checked;

            try
            {
                // Tuple<name, url>
                var generatedLinks = new List<Tuple<string, string>>();

                // One token per file
                foreach (var file in _selectedFiles)
                {
                    var req = new CreateExternalShareRequest
                    {
                        FileID         = file.FileID,
                        Password       = password,
                        ExpiryDate     = expiry,
                        DownloadLimit  = downloadLimit,
                        SharePurpose   = purpose,
                        NotifyOnAccess = notify,
                    };
                    var result = _shareCommandController.CreateExternalShare(req, _currentUser);
                    generatedLinks.Add(Tuple.Create(file.OriginalFileName,
                        string.Format("https://cms.example.com/share/{0}", result.ShareToken)));
                }

                // Folder share (only when no files are selected alongside)
                if (_selectedFolder != null && _selectedFiles.Count == 0)
                {
                    var req = new CreateExternalShareRequest
                    {
                        FolderID       = _selectedFolder.FolderID,
                        Password       = password,
                        ExpiryDate     = expiry,
                        DownloadLimit  = downloadLimit,
                        SharePurpose   = purpose,
                        NotifyOnAccess = notify,
                    };
                    var result = _shareCommandController.CreateExternalShare(req, _currentUser);
                    generatedLinks.Add(Tuple.Create(_selectedFolder.FolderName,
                        string.Format("https://cms.example.com/share/{0}", result.ShareToken)));
                }

                txtShareLink.Text = generatedLinks.Count == 1
                    ? generatedLinks[0].Item2
                    : string.Format("({0} links — ดูรายละเอียดด้านล่าง)", generatedLinks.Count);

                var sb = new StringBuilder();
                sb.AppendLine(string.Format("สร้าง Share Link สำเร็จ {0} รายการ:", generatedLinks.Count));
                foreach (var link in generatedLinks)
                    sb.AppendLine(string.Format("  • {0}", link.Item1));
                sb.AppendLine();
                sb.AppendLine(string.Format("Password: {0}", password != null ? "ใช้รหัสผ่าน" : "ไม่มี"));
                sb.AppendLine(string.Format("วันหมดอายุ: {0}", expiry.HasValue ? expiry.Value.ToShortDateString() : "ไม่กำหนด"));
                sb.AppendLine(string.Format("จำกัดดาวน์โหลด: {0}", downloadLimit.HasValue ? downloadLimit.Value.ToString() : "ไม่จำกัด"));
                sb.AppendLine("สถานะ: รอการอนุมัติจากผู้จัดการ");
                txtSecuritySummary.Text = sb.ToString();

                lblStatus.ForeColor = Color.FromArgb(0, 110, 60);
                lblStatus.Text = string.Format("สำเร็จ: สร้าง {0} share link รอการอนุมัติ", generatedLinks.Count);
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"ข้อผิดพลาด: {ex.Message}";
            }
        }
    }
}
