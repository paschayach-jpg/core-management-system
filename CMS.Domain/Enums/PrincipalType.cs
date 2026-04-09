namespace CMS.Domain.Enums
{
    /// <summary>
    /// Type of principal for FolderPermission ACL entry.
    /// 1=User (from DB), 2=Role, 3=Department (from Employee API), 4=Division (from Employee API)
    /// </summary>
    public enum PrincipalType
    {
        User       = 1,
        Role       = 2,
        Department = 3,
        Division   = 4,
        Group = 5,
        OrgUnit = 6
    }
}
