using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents the token lifecycle and validation state.
    /// Status: Pending, Active, Expired, Disabled, Revoked
    /// </summary>
    public class ShareToken
    {
        public int ShareTokenID { get; set; }
        public Guid TokenValue { get; set; }
        public int ExternalShareID { get; set; }
        public string Status { get; set; } // Pending, Active, Expired, Disabled, Revoked
        public DateTime CreatedDate { get; set; }
        public DateTime? ActivatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? DisabledDate { get; set; }
        public int? DisabledByUserID { get; set; }
        public string DisabledReason { get; set; }
    }
}
