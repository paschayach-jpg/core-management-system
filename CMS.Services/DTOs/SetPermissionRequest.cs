namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request DTO for setting an explicit ACL entry on a folder.
    /// PrincipalType: 1=User, 2=Role, 3=OrgUnit, 4=Group
    /// </summary>
    public class SetPermissionRequest
    {
        public int FolderID { get; set; }

        /// <summary>1=User, 2=Role, 3=OrgUnit, 4=Group</summary>
        public CMS.Domain.Enums.PrincipalType PrincipalType { get; set; }

        public int PrincipalID { get; set; }

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanModify { get; set; }
        public bool CanFullControl { get; set; }

        /// <summary>When true, this entry is a deny entry.</summary>
        public bool IsDeny { get; set; }

        /// <summary>หมายเหตุ / เหตุผลในการขออนุมัติหรือแบ่งปัน</summary>
        public string Remark { get; set; }

        // -----------------------------------------------------------------------
        // Aliases used by tests
        // -----------------------------------------------------------------------

        /// <summary>Alias for FolderID.</summary>
        public int FolderId
        {
            get { return FolderID; }
            set { FolderID = value; }
        }

        /// <summary>Alias for PrincipalID.</summary>
        public int PrincipalId
        {
            get { return PrincipalID; }
            set { PrincipalID = value; }
        }

        /// <summary>Alias for PrincipalType as enum (same as PrincipalType — kept for compatibility).</summary>
        public CMS.Domain.Enums.PrincipalType PrincipalTypeEnum
        {
            get { return PrincipalType; }
            set { PrincipalType = value; }
        }

        /// <summary>
        /// PermissionType alias — maps to the Can* flags.
        /// Setting this updates the corresponding boolean flag.
        /// </summary>
        public CMS.Domain.Enums.PermissionType PermissionType
        {
            get
            {
                if (CanFullControl) return CMS.Domain.Enums.PermissionType.FullControl;
                if (CanModify)      return CMS.Domain.Enums.PermissionType.Modify;
                if (CanDelete)      return CMS.Domain.Enums.PermissionType.Delete;
                if (CanWrite)       return CMS.Domain.Enums.PermissionType.Write;
                return CMS.Domain.Enums.PermissionType.Read;
            }
            set
            {
                CanRead = CanWrite = CanDelete = CanModify = CanFullControl = false;
                switch (value)
                {
                    case CMS.Domain.Enums.PermissionType.FullControl: CanFullControl = true; break;
                    case CMS.Domain.Enums.PermissionType.Modify:      CanModify      = true; break;
                    case CMS.Domain.Enums.PermissionType.Delete:      CanDelete      = true; break;
                    case CMS.Domain.Enums.PermissionType.Write:       CanWrite       = true; break;
                    case CMS.Domain.Enums.PermissionType.Read:        CanRead        = true; break;
                }
            }
        }

        /// <summary>
        /// PermissionState alias — maps to IsDeny flag.
        /// </summary>
        public CMS.Domain.Enums.PermissionState PermissionState
        {
            get { return IsDeny ? CMS.Domain.Enums.PermissionState.ExplicitDeny : CMS.Domain.Enums.PermissionState.Allow; }
            set { IsDeny = (value == CMS.Domain.Enums.PermissionState.ExplicitDeny); }
        }

        /// <summary>
        /// BreakInheritance — when true, the resulting FolderPermission entry will have InheritanceBroken = true.
        /// </summary>
        public bool BreakInheritance { get; set; }
    }
}
