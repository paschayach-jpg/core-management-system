using System;
using System.IO;
using CMS.Infrastructure.Interfaces;

namespace CMS.Infrastructure
{
    /// <summary>
    /// Handles file I/O on the configured network drive (Requirement 5.10, 15.1).
    /// The base path is read from IConfigManager using the key "NetworkDrivePath".
    /// </summary>
    public class NetworkDriveHelper : INetworkDriveHelper
    {
        private readonly IConfigManager _configManager;
        private readonly NetworkShareConnector _shareConnector;

        public NetworkDriveHelper(IConfigManager configManager, NetworkShareConnector shareConnector = null)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");
            _configManager = configManager;
            _shareConnector = shareConnector;
        }

        private void EnsureConnected()
        {
            if (_shareConnector == null) return;
            string msg;
            _shareConnector.TryEnsureConnected(out msg);
        }

        /// <summary>
        /// Constructs a path in the form: {NetworkDrivePath}\{orgUnitId}\{folderId}\{fileName}
        /// </summary>
        public string BuildPath(int orgUnitId, int folderId, string fileName)
        {
            string basePath = _configManager.Get<string>("NetworkDrivePath");
            return Path.Combine(basePath, orgUnitId.ToString(), folderId.ToString(), fileName);
        }

        /// <summary>
        /// Writes the stream to the given path, creating any missing directories first.
        /// </summary>
        public void WriteFile(string path, Stream content)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (content == null) throw new ArgumentNullException("content");
            EnsureConnected();

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Write to a temp file first, flush to disk, then atomically swap it in.
            // This keeps an existing good file intact if the transfer fails part-way,
            // and never leaves a truncated file at the real path.
            string tempPath = path + ".uploading";
            try
            {
                using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    content.CopyTo(fileStream);
                    fileStream.Flush(true);
                }

                if (File.Exists(path))
                    File.Delete(path);
                File.Move(tempPath, path);
            }
            catch
            {
                try { if (File.Exists(tempPath)) File.Delete(tempPath); }
                catch { /* best-effort cleanup */ }
                throw;
            }
        }

        /// <summary>
        /// Returns the size in bytes of the file at the given path, or -1 if it does
        /// not exist / cannot be accessed.
        /// </summary>
        public long GetFileSize(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            try
            {
                EnsureConnected();
                var info = new FileInfo(path);
                return info.Exists ? info.Length : -1;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns a FileStream opened for reading at the given path.
        /// </summary>
        public Stream ReadFile(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            EnsureConnected();
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Deletes the file at the given path if it exists.
        /// </summary>
        public void DeleteFile(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// Returns true if a file exists at the given path.
        /// </summary>
        public bool FileExists(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            return File.Exists(path);
        }

        /// <summary>
        /// ย้ายไฟล์จาก sourcePath ไปยัง destPath จริงบน disk
        /// สร้าง directory ปลายทางอัตโนมัติถ้ายังไม่มี
        /// ถ้า destPath มีไฟล์ชื่อเดิมอยู่แล้วจะ overwrite
        /// </summary>
        public void MoveFile(string sourcePath, string destPath)
        {
            if (sourcePath == null) throw new ArgumentNullException("sourcePath");
            if (destPath   == null) throw new ArgumentNullException("destPath");

            string destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            // ถ้า dest มีไฟล์อยู่แล้วให้ลบก่อน (File.Move ไม่ overwrite ใน .NET Framework)
            if (File.Exists(destPath))
                File.Delete(destPath);

            File.Move(sourcePath, destPath);
        }
    }
}
