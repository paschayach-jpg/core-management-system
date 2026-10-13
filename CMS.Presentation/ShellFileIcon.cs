using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// Returns the Windows shell icon registered for a file extension (e.g. the Word icon for .docx)
    /// without needing the file on disk. Results are cached per extension and size.
    /// </summary>
    internal static class ShellFileIcon
    {
        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private static readonly Dictionary<string, Image> _cache =
            new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the shell icon for <paramref name="extension"/> ("docx" or ".docx") rendered at
        /// <paramref name="size"/>. Returns null when no icon can be resolved.
        /// </summary>
        public static Image Get(string extension, Size size)
        {
            string ext = (extension ?? string.Empty).Trim().TrimStart('.');
            if (ext.Length == 0) return null;

            string key = string.Format("{0}|{1}x{2}", ext, size.Width, size.Height);
            lock (_cache)
            {
                Image cached;
                if (_cache.TryGetValue(key, out cached)) return cached;
            }

            Image image = null;
            try
            {
                var info = new SHFILEINFO();
                uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | (size.Width <= 16 ? SHGFI_SMALLICON : SHGFI_LARGEICON);
                SHGetFileInfo("file." + ext, FILE_ATTRIBUTE_NORMAL, ref info, (uint)Marshal.SizeOf(info), flags);

                if (info.hIcon != IntPtr.Zero)
                {
                    try
                    {
                        using (var icon = Icon.FromHandle(info.hIcon))
                        using (var source = icon.ToBitmap())
                        {
                            image = Resize(source, size);
                        }
                    }
                    finally
                    {
                        DestroyIcon(info.hIcon);
                    }
                }
            }
            catch
            {
                image = null;
            }

            lock (_cache)
            {
                _cache[key] = image;
            }
            return image;
        }

        /// <summary>
        /// Registers the shell icon for <paramref name="extension"/> in <paramref name="list"/> (sized to
        /// the list's ImageSize) on first use and returns its key ("ext_docx"), or null when no icon
        /// can be resolved.
        /// </summary>
        public static string EnsureImageKey(ImageList list, string extension)
        {
            string ext = (extension ?? string.Empty).Trim().TrimStart('.').ToLowerInvariant();
            if (list == null || ext.Length == 0) return null;

            string key = "ext_" + ext;
            if (list.Images.ContainsKey(key)) return key;

            Image icon = Get(ext, list.ImageSize);
            if (icon == null) return null;

            list.Images.Add(key, icon);
            return key;
        }

        private static Image Resize(Image source, Size size)
        {
            var bmp = new Bitmap(size.Width, size.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(source, new Rectangle(0, 0, size.Width, size.Height));
            }
            return bmp;
        }
    }
}
