using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ACL entry for a folder.
    /// PrincipalType: 1=User, 3=Department (from Employee API), 4=Division (from Employee API)
    /// </summary>
    public class FolderPermission
    {
        public int PermissionID { get; set; }
        public int FolderID { get; set; }

        /// <summary>1=User, 3=Department (from Employee API), 4=Division (from Employee API)</summary>
        public int PrincipalType { get; set; }

        public int PrincipalID { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanModify { get; set; }
        public bool CanFullControl { get; set; }
        public bool IsDeny { get; set; }
        public bool InheritanceBroken { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int GrantedBy { get; set; }
        public DateTime GrantedDate { get; set; }
        public int? RevokedBy { get; set; }
        public DateTime? RevokedDate { get; set; }
        public string RevokeReason { get; set; }

        // Aliases
        public int PermissionId { get { return PermissionID; } set { PermissionID = value; } }
        public int PrincipalId  { get { return PrincipalID;  } set { PrincipalID  = value; } }
        public int GrantedByUserId { get { return GrantedBy; } set { GrantedBy = value; } }

        /// <summary>PrincipalType as enum for comparison with PrincipalType enum values.</summary>
        public PrincipalType PrincipalTypeEnum
        {
            get { return (PrincipalType)PrincipalType; }
            set { PrincipalType = (int)value; }
        }

        /// <summary>PermissionType derived from the permission flags (highest granted wins).</summary>
        public PermissionType PermissionType
        {
            get
            {
                if (CanFullControl) return PermissionType.FullControl;
                if (CanModify)      return PermissionType.Modify;
                if (CanDelete)      return PermissionType.Delete;
                if (CanWrite)       return PermissionType.Write;
                if (CanRead)        return PermissionType.Read;
                return PermissionType.Read;
            }
        }

        /// <summary>PermissionState derived from IsDeny flag.</summary>
        public PermissionState PermissionState
        {
            get { return IsDeny ? PermissionState.ExplicitDeny : PermissionState.Allow; }
        }
    }
}