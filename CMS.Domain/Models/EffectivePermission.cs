namespace CMS.Domain.Models
{
    /// <summary>
    /// The computed effective permission set for a user on a specific folder,
    /// after applying ACL entries, Explicit_Deny overrides, and inheritance.
    /// </summary>
    public class EffectivePermission
    {
        public int UserId { get; set; }
        public int FolderId { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanModify { get; set; }
        public bool HasFullControl { get; set; }
    }
}
