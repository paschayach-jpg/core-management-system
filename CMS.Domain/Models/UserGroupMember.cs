using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Maps a user to a permission group (Many-to-Many).
    /// Maps to security.UserGroupMember in SCS database.
    /// </summary>
    public class UserGroupMember
    {
        public int MemberID { get; set; }
        public int GroupID { get; set; }
        public int UserID { get; set; }
        public DateTime JoinedDate { get; set; }
        public int AddedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
