using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents a token-based external share with security constraints.
    /// Status: Pending, Active, Expired, Disabled, Revoked
    /// </summary>
    public class ExternalShare
    {
        public int ExternalShareID { get; set; }
        public Guid ShareToken { get; set; }
        public int? FileID { get; set; }
        public int? FolderID { get; set; }
        public int SharerUserID { get; set; }
        public int? ApproverUserID { get; set; }

        // Security settings
        public string PasswordHash { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? DownloadLimit { get; set; }
        public int CurrentDownloadCount { get; set; }

        // Status: Pending, Active, Expired, Disabled, Revoked
        public string ShareTokenStatus { get; set; }

        // Lifecycle
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? DisabledDate { get; set; }
        public string DisabledReason { get; set; }
        public int? RevokedBy { get; set; }
        public DateTime? RevokedDate { get; set; }
        public string RevocationReason { get; set; }

        // Metadata
        public string SharePurpose { get; set; }
        public bool NotifyOnAccess { get; set; }
    }
}
