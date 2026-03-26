using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Named permission group for group-based ACL.
    /// Maps to security.PermissionGroup in SCS database.
    /// </summary>
    public class PermissionGroup
    {
        public int GroupID { get; set; }
        public string GroupCode { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        /// <summary>NULL = cross-OrgUnit group.</summary>
        public int? OrgUnitID { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
