using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.DMS
{
    /// <summary>
    /// Dialog ให้ User กรอกเหตุผลและเลือก Manager/Admin เพื่อส่งคำขออนุมัติดาวน์โหลดไฟล์
    /// </summary>
    public sealed class DownloadApprovalRequestDialog : Form
    {
        private readonly IApprovalFolderController _approvalFolderController;
        private readonly IUserAccountLookupController _userLookup;
        private readonly FileDocument _file;
        private readonly UserContext _user;

        private ComboBox _cboApprover;
        private TextBox  _txtReason;
        private Label    _lblStatus;
        private Button   _btnSubmit;

        private List<UserAccount> _approvers = new List<UserAccount>();

        public DownloadApprovalRequestDialog(
            FileDocument file,
            UserContext user,
            IApprovalFolderController approvalFolderController,
            IUserAccountLookupController userLookup)
        {
            if (file   == null) throw new ArgumentNullException("file");
            if (user   == null) throw new ArgumentNullException("user");
            if (approvalFolderController == null) throw new ArgumentNullException("approvalFolderController");

            _file                    = file;
            _user                    = user;
            _approvalFolderController = approvalFolderController;
            _userLookup              = userLookup;

            BuildUI();
            LoadApprovers();
        }

        // ── UI ──────────────────────────────────────────────────────────────────

        private void BuildUI()
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
            this.Text            = "ขออนุมัติดาวน์โหลดไฟล์";
            this.Font            = uiFont;
            this.ClientSize      = new Size(480, 370);
            this.MinimumSize     = new Size(480, 370);
            this.MaximumSize     = new Size(480, 370);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = bodyBg;

            // ── Header ──────────────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = navyDark };
            var pnlIcon   = new Panel { Size = new Size(36, 36), Location = new Point(14, 12), BackColor = navyMid };
            pnlIcon.Paint += (s, e) =>
            {
                using (var f  = new Font("Segoe UI", 15f))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString("⬇", f, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
            };
            var lblTitle = new Label
            {
                Text = "ขออนุมัติดาวน์โหลดไฟล์",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(58, 10),
            };
            var lblSub = new Label
            {
                Text = "ทุกการดาวน์โหลดต้องได้รับอนุมัติจาก Manager ก่อน",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(170, 200, 240), AutoSize = true, Location = new Point(59, 34),
            };
            pnlHeader.Controls.AddRange(new Control[] { pnlIcon, lblTitle, lblSub });

            // ── Footer ──────────────────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = footerBg };

            _lblStatus = new Label
            {
                Text = "", Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(180, 30, 30),
                AutoSize = false, Size = new Size(250, 32), Location = new Point(12, 10),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _btnSubmit = new Button
            {
                Text      = "✔  ส่งคำขอ",
                Size      = new Size(110, 32),
                Location  = new Point(480 - 226, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = navyDark,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            _btnSubmit.FlatAppearance.BorderSize = 0;
            _btnSubmit.Click += btnSubmit_Click;

            var btnCancel = new Button
            {
                Text         = "✕  ยกเลิก",
                Size         = new Size(100, 32),
                Location     = new Point(480 - 110, 10),
                FlatStyle    = FlatStyle.Flat,
                BackColor    = Color.FromArgb(220, 225, 235),
                ForeColor    = Color.FromArgb(50, 60, 80),
                DialogResult = DialogResult.Cancel,
            };
            btnCancel.FlatAppearance.BorderColor = borderGray;
            btnCancel.FlatAppearance.BorderSize  = 1;
            pnlFooter.Controls.AddRange(new Control[] { _lblStatus, _btnSubmit, btnCancel });
            this.CancelButton = btnCancel;

            // ── Body ─────────────────────────────────────────────────────────────
            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 10, 16, 0), BackColor = bodyBg };

            Func<string, Label> mkLbl = txt => new Label
            {
                Text = txt, Font = labelFont, ForeColor = Color.FromArgb(55, 65, 85),
                AutoSize = false, Width = 100, Height = 22, TextAlign = ContentAlignment.MiddleRight,
            };

            int bodyW = 480 - 32;
            int ctrlX = 108;
            int ctrlW = bodyW - ctrlX;

            // ── Row: ชื่อไฟล์ ───────────────────────────────────────────────────
            var pnlFile = new Panel { Height = 34, Dock = DockStyle.Top, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            var lblFile = mkLbl("ชื่อไฟล์ :");
            lblFile.Location = new Point(0, 6);
            var txtFile = new TextBox
            {
                Text        = _file.OriginalFileName,
                ReadOnly    = true,
                Location    = new Point(ctrlX, 6),
                Width       = ctrlW,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = Color.FromArgb(235, 240, 250),
                ForeColor   = navyDark,
                Font        = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            pnlFile.Controls.AddRange(new Control[] { lblFile, txtFile });

            // ── Row: ระดับความลับ ────────────────────────────────────────────────
            var pnlConf = new Panel { Height = 34, Dock = DockStyle.Top, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            var lblConf = mkLbl("ระดับ :");
            lblConf.Location = new Point(0, 6);
            var lblConfVal = new Label
            {
                Text      = ConfidentialityLabel(_file.ConfidentialityLevel),
                Location  = new Point(ctrlX, 6),
                Width     = ctrlW,
                Height    = 22,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ConfidentialityColor(_file.ConfidentialityLevel),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            pnlConf.Controls.AddRange(new Control[] { lblConf, lblConfVal });

            // ── Separator bar: ข้อมูลคำขอ ────────────────────────────────────────
            var barReq = new Label
            {
                Text = "  ข้อมูลคำขอ", Font = labelFont, ForeColor = navyDark, BackColor = navyLight,
                Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleLeft,
            };

            // ── Row: ผู้อนุมัติ ─────────────────────────────────────────────────
            var pnlApprover = new Panel { Height = 34, Dock = DockStyle.Top, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            var lblApprover = mkLbl("ผู้อนุมัติ :");
            lblApprover.Location = new Point(0, 6);
            _cboApprover = new ComboBox
            {
                Location      = new Point(ctrlX, 6),
                Width         = ctrlW,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = uiFont,
            };
            pnlApprover.Controls.AddRange(new Control[] { lblApprover, _cboApprover });

            // ── Row: เหตุผล (multiline) ─────────────────────────────────────────
            var pnlReason = new Panel { Height = 100, Dock = DockStyle.Top, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            var lblReason = mkLbl("เหตุผล :");
            lblReason.Location = new Point(0, 6);
            _txtReason = new TextBox
            {
                Multiline   = true,
                ScrollBars  = ScrollBars.Vertical,
                Location    = new Point(ctrlX, 6),
                Width       = ctrlW,
                Height      = 88,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = uiFont,
            };
            pnlReason.Controls.AddRange(new Control[] { lblReason, _txtReason });

            // ── Note ────────────────────────────────────────────────────────────
            var lblNote = new Label
            {
                Text      = "* คำขอจะถูกส่งไปยัง Manager ที่เลือก\r\n  หลังได้รับอนุมัติแล้วจึงจะดาวน์โหลดได้",
                Dock      = DockStyle.Top,
                Height    = 40,
                ForeColor = Color.FromArgb(120, 60, 0),
                BackColor = Color.FromArgb(255, 245, 220),
                Font      = new Font("Segoe UI", 8.5f),
                Padding   = new Padding(6, 4, 0, 0),
            };

            // Stack bottom-to-top (Dock=Top adds in reverse order)
            pnlBody.Controls.Add(lblNote);
            pnlBody.Controls.Add(pnlReason);
            pnlBody.Controls.Add(pnlApprover);
            pnlBody.Controls.Add(barReq);
            pnlBody.Controls.Add(pnlConf);
            pnlBody.Controls.Add(pnlFile);

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);
            this.ResumeLayout(false);
        }

        // ── Data ────────────────────────────────────────────────────────────────

        private void LoadApprovers()
        {
            try
            {
                IList<UserAccount> all = (_userLookup != null)
                    ? _userLookup.GetAllUsers()
                    : new List<UserAccount>();

                // Manager (3) and Admin (4), active, exclude self
                _approvers = all
                    .Where(u => u.IsActive && u.AppRole >= 3 && u.UserID != _user.UserID)
                    .OrderByDescending(u => u.AppRole)
                    .ThenBy(u => u.DisplayName ?? u.ADUserName)
                    .ToList();

                _cboApprover.Items.Clear();
                foreach (var u in _approvers)
                {
                    string label = string.Format("{0}  [{1}]",
                        !string.IsNullOrEmpty(u.DisplayName) ? u.DisplayName : u.ADUserName,
                        u.AppRole >= 4 ? "Admin" : "Manager");
                    _cboApprover.Items.Add(label);
                }

                if (_cboApprover.Items.Count > 0)
                    _cboApprover.SelectedIndex = 0;
                else
                    SetStatus("ไม่พบ Manager/Admin ในระบบ — ไม่สามารถส่งคำขอได้", Color.OrangeRed);
            }
            catch (Exception ex)
            {
                SetStatus("โหลดรายชื่อผู้อนุมัติไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        // ── Events ──────────────────────────────────────────────────────────────

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            var selectedApprover = _approvers[_cboApprover.SelectedIndex];

            try
            {
                _btnSubmit.Enabled = false;
                SetStatus("กำลังส่งคำขอ...", Color.RoyalBlue);

                _approvalFolderController.SubmitDownloadRequest(
                    _file.FileID,
                    _txtReason.Text.Trim(),
                    selectedApprover.UserID,
                    _user);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                SetStatus("ส่งคำขอไม่สำเร็จ: " + ex.Message, Color.Red);
                _btnSubmit.Enabled = true;
            }
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private bool ValidateInput()
        {
            if (_cboApprover.SelectedIndex < 0)
            {
                SetStatus("กรุณาเลือกผู้อนุมัติ", Color.OrangeRed);
                _cboApprover.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(_txtReason.Text))
            {
                SetStatus("กรุณากรอกเหตุผลการดาวน์โหลด", Color.OrangeRed);
                _txtReason.BackColor = Color.FromArgb(255, 230, 230);
                _txtReason.Focus();
                return false;
            }
            _txtReason.BackColor = SystemColors.Window;
            SetStatus(string.Empty, Color.Black);
            return true;
        }

        private void SetStatus(string text, Color color)
        {
            _lblStatus.Text      = text;
            _lblStatus.ForeColor = color;
        }

        private static string ConfidentialityLabel(int level)
        {
            switch (level)
            {
                case 1: return "1HC — High Confidential";
                case 2: return "2SC — Semi Confidential";
                case 3: return "3NC — Normal Confidential";
                case 4: return "4PC — Public Confidential";
                default: return level.ToString();
            }
        }

        private static Color ConfidentialityColor(int level)
        {
            switch (level)
            {
                case 1: return Color.DarkRed;
                case 2: return Color.DarkOrange;
                case 3: return Color.DarkBlue;
                case 4: return Color.DarkGreen;
                default: return Color.Black;
            }
        }
    }
}
