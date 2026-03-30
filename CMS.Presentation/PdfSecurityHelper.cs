using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text.pdf;

namespace CMS.Presentation
{
    /// <summary>
    /// Utility class for opening PDF files with iTextSharp encryption-based security.
    /// Encrypts to a temp file, opens with the system viewer, then cleans up.
    /// </summary>
    internal static class PdfSecurityHelper
    {
        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Opens a PDF with configurable print permission.
        /// print=true  → Allow printing only (block save/copy/edit)
        /// print=false → Block all (view only)
        /// </summary>
        public static async Task OpenSecurePdfAsync(string sourcePath, Form form, bool print)
        {
            int permissions = PdfWriter.ALLOW_SCREENREADERS
                            | PdfWriter.ALLOW_ASSEMBLY;

            if (print)
                permissions |= PdfWriter.ALLOW_PRINTING;

            await OpenWithPermissionsAsync(sourcePath, form, permissions);
        }

        /// <summary>
        /// Opens a PDF with both Print and Save blocked.
        /// Restrictions applied:
        ///   • No ALLOW_PRINTING           → block print dialog
        ///   • No ALLOW_DEGRADED_PRINTING  → block low-quality print
        ///   • No ALLOW_COPY               → block copy text / "Save as PDF" via print-to-PDF
        ///   • No ALLOW_MODIFY_CONTENTS    → block editing
        ///   • No ALLOW_FILL_IN            → block form fill
        /// Only screen reading and page navigation are permitted.
        /// </summary>
        public static async Task OpenBlockPrintSaveAsync(string sourcePath, Form form)
        {
            int permissions = PdfWriter.ALLOW_SCREENREADERS
                            | PdfWriter.ALLOW_ASSEMBLY;
            // ❌ ALLOW_PRINTING            — blocks Print menu
            // ❌ ALLOW_DEGRADED_PRINTING   — blocks low-res print
            // ❌ ALLOW_COPY               — blocks text copy & "Print to PDF" workaround
            // ❌ ALLOW_MODIFY_CONTENTS    — blocks edit
            // ❌ ALLOW_FILL_IN            — blocks form fill

            await OpenWithPermissionsAsync(sourcePath, form, permissions);
        }

        // ── Core logic ─────────────────────────────────────────────────────────

        /// <summary>
        /// Encrypts <paramref name="sourcePath"/> to a temp file using the given
        /// permission flags, opens it in the default viewer, then cleans up.
        /// </summary>
        private static async Task OpenWithPermissionsAsync(string sourcePath, Form form, int permissions)
        {
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

                // ── Encrypt to temp file via PdfStamper ───────────────────────
                byte[] userPw  = new byte[0];
                byte[] ownerPw = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N"));

                // Read source into byte[] first — avoids file-lock and internal null issues
                // when iTextSharp processes PDFs with non-standard internal structures
                PdfReader.unethicalreading = true;
                byte[] sourceBytes = File.ReadAllBytes(sourcePath);

                using (PdfReader reader = new PdfReader(sourceBytes))
                using (MemoryStream ms = new MemoryStream())
                {
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
