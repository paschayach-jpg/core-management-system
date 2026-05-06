using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Panel control that renders a preview of a selected FileDocument.
    /// Supports: image (PictureBox), PDF (WebBrowser), Office docs (unsupported message),
    /// and all other types (icon + metadata).
    /// Load errors are caught internally and shown as a label — never thrown to the caller.
    /// </summary>
    public class FilePreviewPanel : Panel
    {
        // -----------------------------------------------------------------------
        // Private fields
        // -----------------------------------------------------------------------

        private static readonly string[] ImageExtensions  = { ".jpg", ".jpeg", ".png", ".gif" };
        private static readonly string[] PdfExtensions    = { ".pdf" };
        private static readonly string[] OfficeExtensions = { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };

        private PictureBox  _pictureBox;
        private WebBrowser  _webBrowser;
        private Label       _messageLabel;
        private Panel       _metaPanel;

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        public FilePreviewPanel()
        {
            InitializeControls();
        }

        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Display a preview of <paramref name="file"/> located at <paramref name="physicalPath"/>.
        /// Any load error is shown inside the panel without propagating to the caller.
        /// </summary>
        public void ShowFile(FileDocument file, string physicalPath)
        {
            if (file == null) throw new ArgumentNullException("file");

            Clear();

            try
            {
                string ext = (file.FileExtension ?? Path.GetExtension(physicalPath ?? string.Empty))
                             .ToLowerInvariant();

                if (IsExtension(ext, ImageExtensions))
                    ShowImage(physicalPath);
                else if (IsExtension(ext, PdfExtensions))
                    ShowPdf(physicalPath);
                else if (IsExtension(ext, OfficeExtensions))
                    ShowUnsupportedMessage();
                else
                    ShowMetadata(file);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Remove all preview content and reset the panel to an empty state.
        /// </summary>
        public void Clear()
        {
            // Dispose WebBrowser navigation to avoid memory leaks
            if (_webBrowser.Url != null)
                _webBrowser.Navigate("about:blank");

            // Release PictureBox image
            if (_pictureBox.Image != null)
            {
                _pictureBox.Image.Dispose();
                _pictureBox.Image = null;
            }

            // Clear meta panel
            _metaPanel.Controls.Clear();

            // Hide all child controls
            _pictureBox.Visible   = false;
            _webBrowser.Visible   = false;
            _messageLabel.Visible = false;
            _metaPanel.Visible    = false;
        }

        // -----------------------------------------------------------------------
        // Private helpers — show modes
        // -----------------------------------------------------------------------

        private void ShowImage(string physicalPath)
        {
            if (string.IsNullOrEmpty(physicalPath) || !File.Exists(physicalPath))
            {
                ShowError("ไม่พบไฟล์ภาพที่ระบุ");
                return;
            }

            // Load into MemoryStream so the file handle is released immediately
            byte[] bytes = File.ReadAllBytes(physicalPath);
            using (var ms = new MemoryStream(bytes))
            {
                _pictureBox.Image = Image.FromStream(ms);
            }

            _pictureBox.Visible = true;
        }

        private void ShowPdf(string physicalPath)
        {
            if (string.IsNullOrEmpty(physicalPath) || !File.Exists(physicalPath))
            {
                ShowError("ไม่พบไฟล์ PDF ที่ระบุ");
                return;
            }

            _webBrowser.Navigate(physicalPath);
            _webBrowser.Visible = true;
        }

        private void ShowUnsupportedMessage()
        {
            _messageLabel.Text    = "ไม่รองรับ preview สำหรับไฟล์ประเภทนี้ กรุณาดาวน์โหลดเพื่อเปิดดู";
            _messageLabel.ForeColor = SystemColors.GrayText;
            _messageLabel.Visible = true;
        }

        private void ShowMetadata(FileDocument file)
        {
            _metaPanel.Controls.Clear();

            // File icon (generic document icon from system)
            var iconBox = new PictureBox
            {
                Size     = new Size(48, 48),
                Location = new Point(8, 8),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            try
            {
                string ext = (file.FileExtension ?? string.Empty).TrimStart('.');
                // Use shell icon if available; fall back to a simple placeholder
                iconBox.Image = GetFileTypeIcon(file.FileExtension);
            }
            catch
            {
                // Ignore icon load failure — metadata text is still shown
            }

            int y = 64;
            var lines = new[]
            {
                string.Format("ชื่อไฟล์:    {0}", file.OriginalFileName ?? file.StoredFileName),
                string.Format("ขนาด:        {0}", FormatFileSize(file.FileSizeBytes)),
                string.Format("อัพโหลดโดย: {0}", file.UploadedByName ?? "-"),
                string.Format("วันที่อัพโหลด: {0}", file.UploadedDate.ToString("dd/MM/yyyy HH:mm"))
            };

            _metaPanel.Controls.Add(iconBox);

            foreach (var line in lines)
            {
                var lbl = new Label
                {
                    Text      = line,
                    AutoSize  = true,
                    Location  = new Point(8, y),
                    Font      = new Font("Tahoma", 9f)
                };
                _metaPanel.Controls.Add(lbl);
                y += 22;
            }

            _metaPanel.Visible = true;
        }

        private void ShowError(string message)
        {
            _messageLabel.Text      = string.Format("เกิดข้อผิดพลาดในการโหลด preview:\n{0}", message);
            _messageLabel.ForeColor = Color.Firebrick;
            _messageLabel.Visible   = true;
        }

        // -----------------------------------------------------------------------
        // Private helpers — utilities
        // -----------------------------------------------------------------------

        private static bool IsExtension(string ext, string[] list)
        {
            foreach (var e in list)
                if (string.Equals(ext, e, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024)
                return string.Format("{0:0.##} GB", bytes / (1024.0 * 1024 * 1024));
            if (bytes >= 1024 * 1024)
                return string.Format("{0:0.##} MB", bytes / (1024.0 * 1024));
            if (bytes >= 1024)
                return string.Format("{0:0.##} KB", bytes / 1024.0);
            return string.Format("{0} bytes", bytes);
        }

        private static Image GetFileTypeIcon(string extension)
        {
            // Use Shell32 to get the system icon for the file type
            string tempFile = Path.Combine(
                Path.GetTempPath(),
                "cms_icon_probe" + (extension ?? ".bin"));

            // Create a zero-byte temp file so Shell can resolve the icon
            if (!File.Exists(tempFile))
                File.WriteAllBytes(tempFile, new byte[0]);

            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(tempFile);
                return icon != null ? icon.ToBitmap() : null;
            }
            finally
            {
                try { File.Delete(tempFile); } catch { /* ignore */ }
            }
        }

        // -----------------------------------------------------------------------
        // Control initialisation
        // -----------------------------------------------------------------------

        private void InitializeControls()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor   = SystemColors.Window;

            // PictureBox — for images
            _pictureBox = new PictureBox
            {
                Dock     = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Visible  = false
            };

            // WebBrowser — for PDF
            _webBrowser = new WebBrowser
            {
                Dock    = DockStyle.Fill,
                Visible = false,
                ScrollBarsEnabled = true
            };

            // Label — for unsupported types and errors
            _messageLabel = new Label
            {
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Tahoma", 10f),
                Visible   = false,
                AutoSize  = false
            };

            // Panel — for metadata display
            _metaPanel = new Panel
            {
                Dock    = DockStyle.Fill,
                Visible = false,
                AutoScroll = true
            };

            this.Controls.Add(_pictureBox);
            this.Controls.Add(_webBrowser);
            this.Controls.Add(_messageLabel);
            this.Controls.Add(_metaPanel);
        }
    }
}
