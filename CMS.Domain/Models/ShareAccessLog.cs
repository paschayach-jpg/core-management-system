using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Immutable record of access attempts (successful and failed).
    /// This table is append-only; no updates or deletes allowed.
    /// </summary>
    public class ShareAccessLog
    {
        public long AccessLogID { get; set; }
        public Guid ShareToken { get; set; }
        public int ExternalShareID { get; set; }
        public string RecipientIdentifier { get; set; } // User ID or "Anonymous"
        public string AccessType { get; set; } // "View" or "Download"
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; } // "WrongPassword", "Expired", "LimitReached", etc.
        public long? FileSizeBytes { get; set; }
        public DateTime AccessTimestamp { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
