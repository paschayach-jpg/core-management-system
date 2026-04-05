using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request DTO for creating an internal share.
    /// PrincipalType: 1=User, 2=Role, 3=OrgUnit, 4=Group
    /// </summary>
    public class CreateInternalShareRequest
    {
        public int FileID { get; set; }

        /// <summary>1=User, 2=Role, 3=OrgUnit, 4=Group</summary>
        public int PrincipalType { get; set; }

        public int PrincipalID { get; set; }

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }

        /// <summary>Optional expiry date for time-limited shares.</summary>
        public DateTime? ExpiryDate { get; set; }
    }
}
