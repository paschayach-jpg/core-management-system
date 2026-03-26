namespace CMS.Domain.Enums
{
    /// <summary>
    /// The five permission types supported on each folder ACL entry.
    /// </summary>
    public enum PermissionType
    {
        Read        = 1,
        Write       = 2,
        Delete      = 3,
        Modify      = 4,
        FullControl = 5
    }
}
