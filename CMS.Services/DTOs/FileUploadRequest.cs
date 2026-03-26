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
    }
}
