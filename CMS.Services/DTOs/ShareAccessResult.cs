using CMS.Domain.Models;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result of validating and granting access to an external share.
    /// </summary>
    public class ShareAccessResult
    {
        /// <summary>True if access was granted; false if denied.</summary>
        public bool IsGranted { get; set; }

        /// <summary>Populated when IsGranted is false; user-friendly error message.</summary>
        public string DenyReason { get; set; }

        /// <summary>The file being shared (if FileID is set).</summary>
        public FileDocument File { get; set; }

        /// <summary>The folder being shared (if FolderID is set).</summary>
        public Folder Folder { get; set; }

        /// <summary>Number of downloads remaining before limit is reached (if limit is set).</summary>
        public int RemainingDownloads { get; set; }
    }
}
