using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request DTO for creating an external share via token-based link.
    /// Either FileID or FolderID must be specified, but not both.
    /// </summary>
    public class CreateExternalShareRequest
    {
        /// <summary>ID of the file to share (mutually exclusive with FolderID).</summary>
        public int? FileID { get; set; }

        /// <summary>ID of the folder to share (mutually exclusive with FileID).</summary>
        public int? FolderID { get; set; }

        /// <summary>Optional password for access protection.</summary>
        public string Password { get; set; }

        /// <summary>Optional expiry date (max 90 days from creation).</summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>Optional download limit (positive integer).</summary>
        public int? DownloadLimit { get; set; }

        /// <summary>Optional purpose/reason for the share.</summary>
        public string SharePurpose { get; set; }

        /// <summary>Whether to notify Sharer on access attempts (default: true).</summary>
        public bool NotifyOnAccess { get; set; } = true;
    }
}
