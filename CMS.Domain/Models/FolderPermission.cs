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

        /// <summary>1=User, 2=Role, 3=Department (from Employee API), 4=Division (from Employee API)</summary>
        public PrincipalType PrincipalType { get; set; }

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
        public int FolderId     { get { return FolderID;     } set { FolderID     = value; } }
        public int PrincipalId  { get { return PrincipalID;  } set { PrincipalID  = value; } }
        public int GrantedByUserId { get { return GrantedBy; } set { GrantedBy = value; } }

        /// <summary>PrincipalType as int for legacy code.</summary>
        public int PrincipalTypeInt
        {
            get { return (int)PrincipalType; }
            set { PrincipalType = (PrincipalType)value; }
        }

        /// <summary>PrincipalType as enum (same as PrincipalType property — kept for compatibility).</summary>
        public PrincipalType PrincipalTypeEnum
        {
            get { return PrincipalType; }
            set { PrincipalType = value; }
        }

        // -----------------------------------------------------------------------
        // Settable PermissionType / PermissionState (aliases that map to flags)
        // -----------------------------------------------------------------------

        private PermissionType? _permissionTypeOverride;
        private PermissionState? _permissionStateOverride;

        /// <summary>
        /// PermissionType: settable alias that maps to the boolean flags.
        /// When set, updates the corresponding Can* flag. When read, returns the highest granted type.
        /// </summary>
        public PermissionType PermissionType
        {
            get
            {
                if (_permissionTypeOverride.HasValue) return _permissionTypeOverride.Value;
                if (CanFullControl) return PermissionType.FullControl;
                if (CanModify)      return PermissionType.Modify;
                if (CanDelete)      return PermissionType.Delete;
                if (CanWrite)       return PermissionType.Write;
                return PermissionType.Read;
            }
            set
            {
                _permissionTypeOverride = value;
                // Reset all flags then set the appropriate one
                CanRead = CanWrite = CanDelete = CanModify = CanFullControl = false;
                switch (value)
                {
                    case PermissionType.FullControl: CanFullControl = true; break;
                    case PermissionType.Modify:      CanModify      = true; break;
                    case PermissionType.Delete:      CanDelete      = true; break;
                    case PermissionType.Write:       CanWrite       = true; break;
                    case PermissionType.Read:        CanRead        = true; break;
                }
            }
        }

        /// <summary>
        /// PermissionState: settable alias that maps to IsDeny flag.
        /// </summary>
        public PermissionState PermissionState
        {
            get
            {
                if (_permissionStateOverride.HasValue) return _permissionStateOverride.Value;
                return IsDeny ? PermissionState.ExplicitDeny : PermissionState.Allow;
            }
            set
            {
                _permissionStateOverride = value;
                IsDeny = (value == PermissionState.ExplicitDeny);
            }
        }
    }
}