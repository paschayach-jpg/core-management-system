namespace CMS.Domain.Enums
{
    /// <summary>
    /// Whether an ACL entry grants or explicitly denies access.
    /// ExplicitDeny overrides any Allow entry for the same permission type.
    /// </summary>
    public enum PermissionState
    {
        Allow        = 1,
        ExplicitDeny = 2
    }
}
