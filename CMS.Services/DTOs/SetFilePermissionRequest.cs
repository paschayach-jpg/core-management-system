namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request DTO for setting an explicit ACL entry on a file.
    /// PrincipalType: 1=User, 2=Role, 3=OrgUnit, 4=Group
    /// </summary>
    public class SetFilePermissionRequest
    {
        public int FileID { get; set; }

        /// <summary>1=User, 2=Role, 3=OrgUnit, 4=Group</summary>
        public int PrincipalType { get; set; }

        public int PrincipalID { get; set; }

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanModify { get; set; }
        public bool CanFullControl { get; set; }

        /// <summary>When true, this entry is a deny entry.</summary>
        public bool IsDeny { get; set; }
    }
}
