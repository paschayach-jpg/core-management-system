using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Form แสดงข้อมูลไฟล์ที่ถูก share ให้ user คนนี้
    /// เปิดจาก SharedWithMeForm เมื่อ double click รายการประเภทไฟล์
    /// ไม่กระทบโค้ดเดิมใดๆ
    /// </summary>
    public class SharedFileViewForm : Form
    {
        private readonly int                     _fileId;
        private readonly IFileService            _fileService;
        private readonly IFileDocumentRepository _fileDocRepo;
        private readonly UserContext             _user;

        private FileDocument _doc;

        public SharedFileViewForm(
            int                     fileId,
            IFileService            fileService,
            IFileDocumentRepository fileDocRepo,
            UserContext             user)
        {
            _fileId      = fileId;
            _fileService = fileService ?? throw new ArgumentNullException("fileService");
            _fileDocRepo = fileDocRepo ?? throw new ArgumentNullException("fileDocRepo");
            _user        = user        ?? throw new ArgumentNullException("user");
            BuildUI();
        }

        // ════════════════════════════════════════════════════════════════════
        // UI
        // ════════════════════════════════════════════════════════════════════
        private Label  _lblFileName, _lblSize, _lblType, _lblFolder, _lblStatus;
        private Button _btnDownload;

        private void BuildUI()
        {
            var navyDark = Color.FromArgb(25, 55, 105);
            var bodyBg   = Color.FromArgb(243, 246, 251);
            var uiFont   = new Font("Segoe UI", 9f);
            var boldFont = new Font("Segoe UI", 9f, FontStyle.Bold);

            Text            = "📄  ดูไฟล์ที่แบ่งปัน";
            ClientSize      = new Size(480, 300);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = bodyBg;
            Font            = uiFont;

            // Header
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = navyDark };
            var lblTitle  = new Label
            {
                Text      = "📄  ไฟล์ที่แบ่งปันให้คุณ",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(14, 14),
            };
            pnlHeader.Controls.Add(lblTitle);

            // Info panel
            var pnlInfo = new TableLayoutPanel
            {
                Left        = 20,
                Top         = 68,
                Width       = 440,
                Height      = 160,
                ColumnCount = 2,
                RowCount    = 5,
                Font        = uiFont,
            };
            pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++)
                pnlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            _lblFileName = new Label { AutoSize = true, Font = boldFont };
            _lblType     = new Label { AutoSize = true };
            _lblSize     = new Label { AutoSize = true };
            _lblFolder   = new Label { AutoSize = true };
            _lblStatus   = new Label { AutoSize = true };

            AddRow(pnlInfo, 0, "ชื่อไฟล์ :",   _lblFileName);
            AddRow(pnlInfo, 1, "ประเภท :",      _lblType);
            AddRow(pnlInfo, 2, "ขนาด :",        _lblSize);
            AddRow(pnlInfo, 3, "Folder :",       _lblFolder);
            AddRow(pnlInfo, 4, "สถานะ :",        _lblStatus);

            // Buttons
            _btnDownload = new Button
            {
                Text      = "⬇  ดาวน์โหลด",
                Size      = new Size(130, 32),
                Location  = new Point(20, 240),
                FlatStyle = FlatStyle.Flat,
                BackColor = navyDark,
                ForeColor = Color.White,
                Font      = boldFont,
                Enabled   = false,
            };
            _btnDownload.FlatAppearance.BorderSize = 0;
            _btnDownload.Click += (s, e) => DoDownload();

            var btnClose = new Button
            {
                Text         = "ปิด",
                Size         = new Size(80, 32),
                Location     = new Point(380, 240),
                FlatStyle    = FlatStyle.Flat,
                BackColor    = Color.FromArgb(220, 228, 245),
                ForeColor    = navyDark,
                Font         = boldFont,
                DialogResult = DialogResult.Cancel,
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
            btnClose.FlatAppearance.BorderSize  = 1;

            Controls.AddRange(new Control[] { pnlHeader, pnlInfo, _btnDownload, btnClose });
            Load += (s, e) => LoadFileInfo();
        }

        private static void AddRow(TableLayoutPanel tbl, int row, string label, Label value)
        {
            tbl.Controls.Add(new Label
            {
                Text      = label,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(80, 90, 110),
            }, 0, row);
            value.Dock = DockStyle.Fill;
            value.TextAlign = ContentAlignment.MiddleLeft;
            tbl.Controls.Add(value, 1, row);
        }

        // ════════════════════════════════════════════════════════════════════
        // Data
        // ════════════════════════════════════════════════════════════════════
        private void LoadFileInfo()
        {
            try
            {
                _doc = _fileDocRepo.GetById(_fileId);
                if (_doc == null)
                {
                    _lblFileName.Text = "(ไม่พบไฟล์)";
                    _lblStatus.Text   = "ไม่พบข้อมูลในระบบ";
                    return;
                }

                _lblFileName.Text = _doc.OriginalFileName;
                _lblType.Text     = _doc.FileExtension?.TrimStart('.').ToUpperInvariant() ?? "—";
                _lblSize.Text     = FormatSize(_doc.FileSizeBytes);
                _lblFolder.Text   = _doc.FolderName ?? string.Format("FolderID={0}", _doc.FolderID);
                _lblStatus.Text   = _doc.IsDeleted ? "⚠ ถูกลบแล้ว" : "✔ พร้อมใช้งาน";

                _btnDownload.Enabled = !_doc.IsDeleted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DoDownload()
        {
            if (_doc == null) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title    = "บันทึกไฟล์";
                sfd.FileName = _doc.OriginalFileName;
                sfd.Filter   = "All Files (*.*)|*.*";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var result = _fileService.DownloadFile(_fileId, _user);

                    if (result == null || !result.Success)
                    {
                        MessageBox.Show(
                            "ดาวน์โหลดไม่สำเร็จ: " + (result?.ErrorMessage ?? "Unknown error"),
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    using (var fs = File.Create(sfd.FileName))
                        result.FileStream.CopyTo(fs);

                    MessageBox.Show("บันทึกไฟล์สำเร็จ", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024)        return bytes + " B";
            if (bytes < 1024 * 1024) return string.Format("{0:0.#} KB", bytes / 1024.0);
            return string.Format("{0:0.##} MB", bytes / (1024.0 * 1024));
        }
    }
}
