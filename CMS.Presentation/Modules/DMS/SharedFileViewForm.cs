using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
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
        private readonly IFileBrowseController   _fileBrowseController;
        private readonly IShareLookupController  _shareLookupController;
        private readonly UserContext             _user;
        private readonly IDownloadPolicyController _downloadPolicyController;
        private readonly IApprovalFolderController _approvalFolderController;

        private FileDocument _doc;

        public SharedFileViewForm(
            int                     fileId,
            IFileBrowseController   fileBrowseController,
            IShareLookupController  shareLookupController,
            UserContext             user,
            IDownloadPolicyController downloadPolicyController = null,
            IApprovalFolderController approvalFolderController = null)
        {
            _downloadPolicyController = downloadPolicyController;
            _approvalFolderController = approvalFolderController;
            _fileId      = fileId;
            _fileBrowseController = fileBrowseController ?? throw new ArgumentNullException("fileBrowseController");
            _shareLookupController = shareLookupController ?? throw new ArgumentNullException("shareLookupController");
            _user        = user        ?? throw new ArgumentNullException("user");
            BuildUI();
        }

        // ════════════════════════════════════════════════════════════════════
        // UI
        // ════════════════════════════════════════════════════════════════════
        private Label  _lblFileName, _lblSize, _lblType, _lblFolder, _lblStatus;
        private Button _btnDownload, _btnOpen;

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
            _btnOpen = new Button
            {
                Text      = "👁  เปิดดู",
                Size      = new Size(110, 32),
                Location  = new Point(20, 240),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 90, 150),
                ForeColor = Color.White,
                Font      = boldFont,
                Enabled   = false,
            };
            _btnOpen.FlatAppearance.BorderSize = 0;
            _btnOpen.Click += (s, e) => DoOpen();

            _btnDownload = new Button
            {
                Text      = "⬇  ดาวน์โหลด",
                Size      = new Size(130, 32),
                Location  = new Point(138, 240),
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

            Controls.AddRange(new Control[] { pnlHeader, pnlInfo, _btnOpen, _btnDownload, btnClose });
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
                _doc = _shareLookupController.GetFileById(_fileId);
                if (_doc == null)
                {
                    _lblFileName.Text = "(ไม่พบไฟล์)";
                    _lblStatus.Text   = "ไม่พบข้อมูลในระบบ";
                    _btnDownload.Visible = false;
                    _btnOpen.Enabled     = false;
                    return;
                }

                _lblFileName.Text = _doc.OriginalFileName;
                _lblType.Text     = _doc.FileExtension?.TrimStart('.').ToUpperInvariant() ?? "—";
                _lblSize.Text     = FormatSize(_doc.FileSizeBytes);
                _lblFolder.Text   = _doc.FolderName ?? string.Format("FolderID={0}", _doc.FolderID);

                var activeShare = GetActiveDirectShare();
                bool hasActiveShare = activeShare != null;
                var parentFolder = _shareLookupController.GetFolderById(_doc.FolderID);
                bool isDeleted = _doc.IsDeleted || _doc.DeletedDate.HasValue
                              || (parentFolder != null && parentFolder.IsDeleted);
                _lblStatus.Text   = isDeleted
                    ? "⚠ ถูกลบแล้ว"
                    : hasActiveShare ? "✔ พร้อมใช้งาน" : "🚫 สิทธิ์แชร์ถูกยกเลิกหรือหมดอายุแล้ว";

                _btnOpen.Enabled = !isDeleted && hasActiveShare;

                // กรณี View อย่างเดียว (ไม่มีสิทธิ์ CanDownload หรือ CanWrite/Modify/FullControl) → ซ่อนปุ่มดาวน์โหลด
                bool canDownload = !isDeleted && hasActiveShare && CanUserDownload(activeShare);
                _btnDownload.Visible = canDownload;
                _btnDownload.Enabled = canDownload;
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private FilePermission GetActiveDirectShare()
        {
            var shares = _shareLookupController.GetFilePermissionsForUser(_user.UserID);
            return shares.FirstOrDefault(p => p.FileID == _fileId
                                         && p.CanRead
                                         && !p.IsDeny
                                         && !p.RevokedDate.HasValue
                                         && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today));
        }

        private bool HasActiveDirectShare()
        {
            return GetActiveDirectShare() != null;
        }

        private bool CanUserDownload(FilePermission share)
        {
            if (share == null) return false;

            // ตรวจสอบว่าได้รับสิทธิ์ Download หรือ Write / Modify / FullControl หรือไม่
            bool hasDownloadOrWritePermission = share.CanDownload || share.CanWrite || share.CanModify || share.CanFullControl;
            if (!hasDownloadOrWritePermission) return false;

            // ตรวจสอบผ่าน DownloadPolicyController หากมีลงทะเบียนไว้
            if (_downloadPolicyController != null)
            {
                try
                {
                    var check = _downloadPolicyController.CheckDownload(_fileId, _user);
                    return check != null && check.CanDownloadNow;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private async void DoOpen()
        {
            if (_doc == null) return;
            var activeShare = GetActiveDirectShare();
            if (activeShare == null)
            {
                MessageBox.Show("สิทธิ์แชร์ไฟล์นี้ถูกยกเลิกหรือหมดอายุแล้ว", "แชร์กับฉัน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFileInfo();
                return;
            }

            // เปิดจาก "แชร์กับฉัน" ใช้สิทธิ์ตามที่แชร์เท่านั้น — สิทธิ์ของ user เองต้องเปิดจากหน้า Folder
            bool shareAllowsEdit = activeShare.CanWrite || activeShare.CanModify || activeShare.CanFullControl;

            // Non-PDF: open a locked (View) or editable (Edit) temp copy per the share's permission.
            string ext = _doc.FileExtension;
            if (string.IsNullOrEmpty(ext))
                ext = Path.GetExtension(_doc.OriginalFileName ?? string.Empty);
            if (!string.Equals((ext ?? string.Empty).TrimStart('.'), "pdf", StringComparison.OrdinalIgnoreCase))
            {
                // A saved-back version gets a new FileID, so this view (bound to the old one) is stale.
                OfficeSecurityHelper.OpenFile(this, _doc, _user, _fileBrowseController, () =>
                {
                    if (!IsDisposed) Close();
                }, !shareAllowsEdit);
                return;
            }

            // PDF: เปิดสำเนาที่เข้ารหัสบล็อก Print/Save/Copy — กฎเดียวกับการเปิดจากหน้า Folder
            string tempDir = null;
            try
            {
                var result = _fileBrowseController.DownloadFile(_fileId, _user);
                if (result == null || !result.Success || result.FileStream == null)
                {
                    MessageBox.Show("เปิดไฟล์ไม่สำเร็จ: " + (ErrorTextHelper.MaskPaths(result?.ErrorMessage) ?? "Unknown error"),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                tempDir = OfficeSecurityHelper.CreateOpenDirectory(_fileId);
                string tempPath = Path.Combine(tempDir, OfficeSecurityHelper.SafeTempFileName(_doc.OriginalFileName, _fileId));
                using (result.FileStream)
                using (var fs = File.Create(tempPath))
                    result.FileStream.CopyTo(fs);

                await PdfSecurityHelper.OpenSecurePdfAsync(tempPath, this, PdfViewOptions.ViewOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ErrorTextHelper.MaskPaths(ex.Message), "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // สำเนาที่ยังไม่เข้ารหัส — ลบทิ้งเสมอ (viewer เปิดสำเนาที่เข้ารหัสแยกต่างหาก)
                if (tempDir != null)
                {
                    try { Directory.Delete(tempDir, true); }
                    catch { /* PurgeStaleTempFiles เก็บกวาดภายหลัง */ }
                }
            }
        }

        private void DoDownload()
        {
            if (_doc == null) return;
            if (!HasActiveDirectShare())
            {
                MessageBox.Show("สิทธิ์แชร์ไฟล์นี้ถูกยกเลิกหรือหมดอายุแล้ว", "แชร์กับฉัน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFileInfo();
                return;
            }

            // ใช้กฎดาวน์โหลดเดียวกับทุกหน้า (ดาวน์โหลดได้ทันที / ขออนุมัติ)
            CMS.Presentation.Services.FileDownloadGate.Download(
                this, _doc, _user, _downloadPolicyController, _approvalFolderController);
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024)        return bytes + " B";
            if (bytes < 1024 * 1024) return string.Format("{0:0.#} KB", bytes / 1024.0);
            return string.Format("{0:0.##} MB", bytes / (1024.0 * 1024));
        }
    }
}
