namespace CMS.Domain.Models
{
    /// <summary>
    /// The outcome of a single file upload attempt within a batch operation.
    /// </summary>
    public class UploadResult
    {
        public string OriginalFileName { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>Set when Success is true; null on failure.</summary>
        public int? FileDocumentId { get; set; }
    }
}
