using System.IO;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// The result of a file download request, including the verified file stream.
    /// </summary>
    public class FileDownloadResult
    {
        /// <summary>The original file name.</summary>
        public string FileName { get; set; }

        /// <summary>
        /// Readable stream of the file content.
        /// Only populated when <see cref="Success"/> is true.
        /// </summary>
        public Stream FileStream { get; set; }

        /// <summary>File size in bytes.</summary>
        public long FileSizeBytes { get; set; }

        /// <summary>True when the integrity check passed and the file is ready to serve.</summary>
        public bool Success { get; set; }

        /// <summary>Populated when <see cref="Success"/> is false.</summary>
        public string ErrorMessage { get; set; }
    }
}
