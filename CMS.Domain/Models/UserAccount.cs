using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// CMS user account linked to Active Directory.
    /// Maps to core.UserAccount in SCS database.
    /// </summary>
    public class UserAccount
    {
        public int UserID { get; set; }
        public string ADUserName { get; set; }
        public string DisplayName { get; set; }

        /// <summary>PDPA: stored only with consent.</summary>
        public string Email { get; set; }

        /// <summary>1=Officer, 2=SrOfficer, 3=Manager, 4=Admin</summary>
        public int AppRole { get; set; }

        public int OrgUnitID { get; set; }
        public bool ConsentGiven { get; set; }
        public DateTime? ConsentDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        // Aliases
        public string AdIdentity { get { return ADUserName; } set { ADUserName = value; } }

        /// <summary>Alias for UserID.</summary>
        public int UserId
        {
            get { return UserID; }
            set { UserID = value; }
        }

        /// <summary>Alias for AppRole as UserRole enum.</summary>
        public UserRole Role
        {
            get { return (UserRole)AppRole; }
            set { AppRole = (int)value; }
        }
    }
}