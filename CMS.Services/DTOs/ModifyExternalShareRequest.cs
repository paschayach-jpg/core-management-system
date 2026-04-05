using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request DTO for modifying an external share.
    /// Allows updating expiry date, download limit, and password.
    /// </summary>
    public class ModifyExternalShareRequest
    {
        public int ExternalShareID { get; set; }

        /// <summary>New expiry date (optional; if provided, must be ≤ 90 days from now).</summary>
        public DateTime? NewExpiryDate { get; set; }

        /// <summary>New download limit (optional; must be positive if provided).</summary>
        public int? NewDownloadLimit { get; set; }

        /// <summary>New password (optional; if provided, will be hashed and stored).</summary>
        public string NewPassword { get; set; }
    }
}
