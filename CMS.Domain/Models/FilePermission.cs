using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ACL entry for a file (separate from folder-level permissions).
    /// PrincipalType: 1=User, 2=Role, 3=OrgUnit, 4=Group
    /// </summary>
    public class FilePermission
    {
        public int FilePermissionID { get; set; }
        public int FileID { get; set; }

        /// <summary>1=User, 2=Role, 3=OrgUnit, 4=Group</summary>
        public int PrincipalType { get; set; }

        public int PrincipalID { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanModify { get; set; }
        public bool CanFullControl { get; set; }
        public bool IsDeny { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int GrantedBy { get; set; }
        public DateTime GrantedDate { get; set; }
        public int? RevokedBy { get; set; }
        public DateTime? RevokedDate { get; set; }
        public string RevokeReason { get; set; }

        public PrincipalType PrincipalTypeEnum
        {
            get { return (PrincipalType)PrincipalType; }
            set { PrincipalType = (int)value; }
        }
    }
}
