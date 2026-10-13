using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Non-modal companion window for a file opened in Edit mode. Watches the writable temp copy and
    /// lets the user save it back into CMS as a new version (via IFileService.SaveFileNewVersion).
    /// </summary>
    public partial class OfficeEditSessionDialog : Form
    {
        private readonly FileDocument _file;
        private readonly string _tempPath;
        private readonly UserContext _user;
        private readonly IFileBrowseController _fileBrowseController;
        private readonly Action _onNewVersionSaved;

        private readonly DateTime _initialWriteUtc;
        private readonly string _initialHash;
        private bool _saved;

        internal OfficeEditSessionDialog(FileDocument file, string tempPath, UserContext user,
            IFileBrowseController fileBrowseController, Action onNewVersionSaved)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(tempPath)) throw new ArgumentNullException("tempPath");
            if (fileBrowseController == null) throw new ArgumentNullException("fileBrowseController");

            InitializeComponent();

            _file = file;
            _tempPath = tempPath;
            _user = user;
            _fileBrowseController = fileBrowseController;
            _onNewVersionSaved = onNewVersionSaved;

            _initialWriteUtc = File.GetLastWriteTimeUtc(tempPath);
            _initialHash = ComputeHash(tempPath);

            lblFileName.Text = file.OriginalFileName;
            Text = "แก้ไขไฟล์ - " + file.OriginalFileName;
            tmrWatch.Start();
        }

        private bool IsModifiedOnDisk
        {
            get
            {
                try { return File.Exists(_tempPath) && File.GetLastWriteTimeUtc(_tempPath) != _initialWriteUtc; }
                catch { return false; }
            }
        }

        private void TmrWatch_Tick(object sender, EventArgs e)
        {
            if (_saved) return;

            bool modified = IsModifiedOnDisk;
            btnSave.Enabled = modified;
            lblStatus.Text = modified
                ? string.Format("ตรวจพบการแก้ไขล่าสุดเมื่อ {0:HH:mm:ss}", File.GetLastWriteTime(_tempPath))
                : "ยังไม่มีการแก้ไข";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            byte[] content;
            try
            {
                // Office still holds the file open; share read/write so we can read the saved copy.
                using (var fs = new FileStream(_tempPath, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete))
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    content = ms.ToArray();
                }
            }
            catch (IOException)
            {
                MessageBox.Show("อ่านไฟล์ไม่ได้ กรุณาบันทึกหรือปิดไฟล์ในโปรแกรมก่อน แล้วลองอีกครั้ง",
                    "บันทึกไม่ได้", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.Equals(ComputeHash(content), _initialHash, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("ไฟล์ยังไม่มีการเปลี่ยนแปลง กรุณากดบันทึก (Ctrl+S) ในโปรแกรมก่อน",
                    "ไม่มีการแก้ไข", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnSave.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var result = _fileBrowseController.SaveFileNewVersion(_file.FileID, new MemoryStream(content), _user);
                if (result == null || !result.Success)
                {
                    MessageBox.Show("บันทึกไม่สำเร็จ: " + (result != null ? result.ErrorMessage : "Unknown error"),
                        "บันทึกไม่ได้", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnSave.Enabled = true;
                    return;
                }

                _saved = true;
                tmrWatch.Stop();
                MessageBox.Show(
                    "บันทึกเป็นเวอร์ชันใหม่ใน CMS แล้ว\nกรุณาปิดไฟล์ในโปรแกรม หากต้องการแก้ไขต่อให้เปิดไฟล์ล่าสุดจาก CMS อีกครั้ง",
                    "บันทึกสำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (_onNewVersionSaved != null)
                {
                    try { _onNewVersionSaved(); }
                    catch (Exception ex) { OfficeSecurityHelper.LogError(_file.OriginalFileName, ex); }
                }

                Close();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "ไม่มีสิทธิ์", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                OfficeSecurityHelper.LogError(_file.OriginalFileName, ex);
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "บันทึกไม่ได้",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSave.Enabled = true;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OfficeEditSessionDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_saved || !IsModifiedOnDisk)
                return;

            var answer = MessageBox.Show(
                "ไฟล์ถูกแก้ไขแต่ยังไม่ได้บันทึกกลับเข้า CMS การแก้ไขจะไม่ถูกเก็บในระบบ\nต้องการปิดหรือไม่?",
                "ยังไม่ได้บันทึก", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes)
                e.Cancel = true;
        }

        private void OfficeEditSessionDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrWatch.Stop();
            OfficeSecurityHelper.ScheduleCleanup(Path.GetDirectoryName(_tempPath));
        }

        private static string ComputeHash(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var sha = SHA256.Create())
                {
                    return BitConverter.ToString(sha.ComputeHash(fs));
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ComputeHash(byte[] content)
        {
            using (var sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(content));
            }
        }
    }
}
