namespace CMS.Domain.Enums
{
    /// <summary>
    /// Type of principal for FolderPermission ACL entry.
    /// 1=User (from DB), 3=Department (from Employee API), 4=Division (from Employee API)
    /// </summary>
    public enum PrincipalType
    {
        User       = 1,
        Department = 3,
        Division   = 4
    }
}
