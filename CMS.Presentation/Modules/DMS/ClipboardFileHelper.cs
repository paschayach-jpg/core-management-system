using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// Reads files (copied in Windows Explorer) or images (screenshots) from the system clipboard
    /// so they can be pasted into the upload flow with Ctrl+V. Must be called on the UI thread.
    /// </summary>
    internal static class ClipboardFileHelper
    {
        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

        private static string PasteDir
        {
            get { return Path.Combine(Path.GetTempPath(), "CMS_Paste"); }
        }

        /// <summary>
        /// Changes every time anything is placed on the system clipboard. Lets callers tell whether the
        /// clipboard was touched after an in-app Cut/Copy without having to clear it.
        /// </summary>
        public static uint GetSequenceNumber()
        {
            try { return GetClipboardSequenceNumber(); }
            catch (Exception) { return 0; }
        }

        public static bool HasPasteableContent()
        {
            try { return Clipboard.ContainsFileDropList() || Clipboard.ContainsImage(); }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// True when the focused control is a text-entry control, so Ctrl+V must paste text there
        /// instead of being treated as a file paste. Walks nested containers to the real focus owner.
        /// </summary>
        public static bool IsTextInputFocused(Control active)
        {
            while (active != null)
            {
                if (active is TextBoxBase || active is ComboBox || active is UpDownBase)
                    return true;
                var container = active as ContainerControl;
                if (container == null || container.ActiveControl == null || container.ActiveControl == active)
                    break;
                active = container.ActiveControl;
            }
            return false;
        }

        /// <summary>
        /// Returns local file/folder paths from the clipboard. An image is saved to a temp PNG first
        /// (removed by <see cref="PurgeTempFiles"/>). Returns an empty array when nothing usable is on the clipboard.
        /// </summary>
        public static string[] GetPaths()
        {
            try
            {
                if (Clipboard.ContainsFileDropList())
                {
                    var list = Clipboard.GetFileDropList();
                    var paths = new List<string>();
                    foreach (string p in list) paths.Add(p);
                    return paths.ToArray();
                }

                if (Clipboard.ContainsImage())
                {
                    using (var img = Clipboard.GetImage())
                    {
                        if (img == null) return new string[0];

                        string dir = PasteDir;
                        Directory.CreateDirectory(dir);
                        PurgeTempFiles(TimeSpan.FromHours(1));
                        string file = Path.Combine(dir,
                            string.Format("Pasted_{0:yyyyMMdd_HHmmss_fff}.png", DateTime.Now));
                        img.Save(file, ImageFormat.Png);
                        return new[] { file };
                    }
                }
            }
            catch (Exception) { }

            return new string[0];
        }

        /// <summary>
        /// Deletes pasted-image temp files older than <paramref name="olderThan"/> (all of them when null/zero).
        /// Pasted screenshots can be confidential, so they must not accumulate in %TEMP%.
        /// </summary>
        public static void PurgeTempFiles(TimeSpan? olderThan)
        {
            try
            {
                string dir = PasteDir;
                if (!Directory.Exists(dir)) return;

                DateTime cutoff = olderThan.HasValue ? DateTime.Now - olderThan.Value : DateTime.MaxValue;
                foreach (string f in Directory.GetFiles(dir, "Pasted_*.png"))
                {
                    try
                    {
                        if (File.GetLastWriteTime(f) < cutoff) File.Delete(f);
                    }
                    catch (Exception) { /* in use by an upload; the next purge removes it */ }
                }

                try { if (!Directory.EnumerateFileSystemEntries(dir).Any()) Directory.Delete(dir); }
                catch (Exception) { }
            }
            catch (Exception) { }
        }
    }
}
