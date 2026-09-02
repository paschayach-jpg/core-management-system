using System.IO;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Represents a single file submitted as part of a batch upload operation.
    /// </summary>
    public class FileUploadRequest
    {
        public string FileName { get; set; }
        public Stream FileStream { get; set; }
        public long FileSizeBytes { get; set; }
        public string MimeType { get; set; }

        /// <summary>
        /// When true the upload service will overwrite (replace) an existing file
        /// with the same name in the target folder instead of creating a copy.
        /// </summary>
        public bool OverwriteExisting { get; set; }

        /// <summary>
        /// Optional: The initial status of the uploaded file document. Defaults to 1 (Active).
        /// </summary>
        public int? InitialFileStatus { get; set; }
    }
}
