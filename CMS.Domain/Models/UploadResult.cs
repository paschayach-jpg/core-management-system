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

        /// <summary>True when the original file name was already taken and the file was saved under a new name.</summary>
        public bool IsDuplicate { get; set; }

        /// <summary>The actual file name used for storage. Differs from OriginalFileName when IsDuplicate is true.</summary>
        public string ActualFileName { get; set; }
    }
}
