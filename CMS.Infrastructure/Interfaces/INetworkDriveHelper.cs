using System.IO;

namespace CMS.Infrastructure.Interfaces
{
    /// <summary>
    /// Provides file operations on the network drive (Requirement 5.10, 15.1).
    /// </summary>
    public interface INetworkDriveHelper
    {
        /// <summary>Constructs the full network path for a file.</summary>
        string BuildPath(int orgUnitId, int folderId, string fileName);

        /// <summary>Writes the stream content to the specified path, creating directories as needed.</summary>
        void WriteFile(string path, Stream content);

        /// <summary>Returns a readable stream for the file at the specified path.</summary>
        Stream ReadFile(string path);

        /// <summary>Deletes the file at the specified path if it exists.</summary>
        void DeleteFile(string path);

        /// <summary>Returns true if a file exists at the specified path.</summary>
        bool FileExists(string path);

        /// <summary>
        /// Returns the size in bytes of the file at the specified path,
        /// or -1 if the file does not exist / cannot be accessed.
        /// </summary>
        long GetFileSize(string path);

        /// <summary>
        /// ย้ายไฟล์จาก sourcePath ไปยัง destPath จริงบน disk
        /// สร้าง directory ปลายทางอัตโนมัติถ้ายังไม่มี
        /// </summary>
        void MoveFile(string sourcePath, string destPath);
    }
}
