using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CMS.Domain.Models;
using iTextSharp.text.pdf;

namespace CMS.Presentation
{
    /// <summary>
    /// PDF permission options derived from the user's effective folder/file permission.
    /// </summary>
    internal struct PdfViewOptions
    {
        public bool CanPrint { get; set; }
        public bool CanSave  { get; set; }
        public bool CanCopy  { get; set; }

        /// <summary>
        /// View-only preset — blocks print, save, and copy.
        /// </summary>
        public static PdfViewOptions ViewOnly
        {
            get { return new PdfViewOptions { CanPrint = false, CanSave = false, CanCopy = false }; }
        }

        /// <summary>
        /// Derives PDF view options from the user's effective permission on the folder.
        /// Mapping:
        ///   Manager / Admin (AppRole ≥ 3)         → full access
        ///   HasFullControl / CanModify / CanWrite  → print + save + copy
        ///   CanRead only                           → view only
        /// </summary>
        public static PdfViewOptions FromEffective(EffectivePermission eff, UserContext user)
        {
            if (eff == null || user == null)
                return ViewOnly;

            bool isManager = user.AppRole >= 3;
            bool hasWrite  = eff.HasFullControl || eff.CanModify || eff.CanWrite;

            return new PdfViewOptions
            {
                CanPrint = isManager || hasWrite,
                CanSave  = isManager || hasWrite,
                CanCopy  = isManager || hasWrite,
            };
        }
    }

    /// <summary>
    /// Utility class for opening PDF files with iTextSharp encryption-based security.
    /// Encrypts to a temp file using permissions derived from the user's effective rights,
    /// opens with the system viewer, then cleans up.
    /// </summary>
    internal static class PdfSecurityHelper
    {
        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Opens a PDF applying print/save/copy restrictions from <paramref name="options"/>.
        /// </summary>
        public static async Task OpenSecurePdfAsync(string sourcePath, Form form, PdfViewOptions options)
        {
            int permissions = PdfWriter.ALLOW_SCREENREADERS
                            | PdfWriter.ALLOW_ASSEMBLY;

            if (options.CanPrint)
                permissions |= PdfWriter.ALLOW_PRINTING | PdfWriter.ALLOW_DEGRADED_PRINTING;

            if (options.CanSave || options.CanCopy)
                permissions |= PdfWriter.ALLOW_COPY;

            await OpenWithPermissionsAsync(sourcePath, form, permissions);
        }

        /// <summary>
        /// Opens a PDF with both Print and Save blocked (view only).
        /// </summary>
        public static async Task OpenBlockPrintSaveAsync(string sourcePath, Form form)
        {
            await OpenSecurePdfAsync(sourcePath, form, PdfViewOptions.ViewOnly);
        }

        // ── Core logic ─────────────────────────────────────────────────────────

        private static async Task OpenWithPermissionsAsync(string sourcePath, Form form, int permissions)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                MessageBox.Show("พาธไฟล์ไม่ถูกต้อง", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(sourcePath))
            {
                MessageBox.Show("ไม่พบไฟล์ที่ต้องการเปิด", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string tempPath = null;

            try
            {
                tempPath = Path.Combine(
                    Path.GetTempPath(),
                    string.Format("restricted_{0:N}_{1}", Guid.NewGuid(), Path.GetFileName(sourcePath))
                );

                // ── Encrypt to temp file ───────────────────────────────────────
                byte[] userPw  = new byte[0];
                byte[] ownerPw = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N"));

                PdfReader.unethicalreading = true;
                byte[] sourceBytes = File.ReadAllBytes(sourcePath);

                if (sourceBytes == null || sourceBytes.Length == 0)
                    throw new InvalidDataException("ไฟล์ PDF ว่างหรืออ่านข้อมูลไม่ได้");

                using (PdfReader reader = new PdfReader(sourceBytes))
                using (MemoryStream ms = new MemoryStream())
                {
                    if (reader.NumberOfPages <= 0)
                        throw new InvalidDataException("ไฟล์ PDF ไม่มีหน้าเอกสารหรือโครงสร้างไม่ถูกต้อง");

                    using (PdfStamper stamper = new PdfStamper(reader, ms))
                    {
                        stamper.SetEncryption(
                            userPw,
                            ownerPw,
                            permissions,
                            PdfWriter.ENCRYPTION_AES_128
                        );
                    }
                    File.WriteAllBytes(tempPath, ms.ToArray());
                }

                // ── Open with default viewer ───────────────────────────────────
                form.Invoke(new Action(() =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName        = tempPath,
                        UseShellExecute = true
                    });
                }));

                // ── Wait then cleanup ──────────────────────────────────────────
                await CleanupTempFileAsync(tempPath, retryCount: 5, delayMs: 3000);
            }
            catch (NullReferenceException ex)
            {
                if (tempPath != null) TryDeleteFile(tempPath);

                MessageBox.Show(
                    string.Format("ไม่สามารถประมวลผล PDF นี้ได้ (โครงสร้างไฟล์อาจไม่รองรับ): {0}", ex.Message),
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                if (tempPath != null) TryDeleteFile(tempPath);

                MessageBox.Show(
                    string.Format("เกิดข้อผิดพลาดในการเปิดไฟล์: {0}", ex.Message),
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static async Task CleanupTempFileAsync(string path, int retryCount, int delayMs)
        {
            for (int i = 0; i < retryCount; i++)
            {
                await Task.Delay(delayMs);
                if (TryDeleteFile(path)) return;
            }
            System.Diagnostics.Debug.WriteLine(string.Format("[WARN] Could not delete temp file: {0}", path));
        }

        private static bool TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("[DeleteFail] {0}", ex.Message));
                return false;
            }
        }
    }
}
