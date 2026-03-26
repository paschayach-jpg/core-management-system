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

        public NetworkDriveHelper(IConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");
            _configManager = configManager;
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

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                content.CopyTo(fileStream);
            }
        }

        /// <summary>
        /// Returns a FileStream opened for reading at the given path.
        /// </summary>
        public Stream ReadFile(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
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
    }
}
