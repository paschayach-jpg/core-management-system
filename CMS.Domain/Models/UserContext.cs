using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents the authenticated user''s session context, resolved from Active Directory.
    /// </summary>
    public class UserContext
    {
        public int UserID { get; set; }
        public string ADUserName { get; set; }

        /// <summary>1=Officer, 2=SrOfficer, 3=Manager, 4=Admin</summary>
        public int AppRole { get; set; }

        public int OrgUnitID { get; set; }

        public string MachineName { get; set; }
        public string IPAddress { get; set; }
        public string SessionID { get; set; }

        // -----------------------------------------------------------------------
        // Aliases
        // -----------------------------------------------------------------------

        public int UserId
        {
            get { return UserID; }
            set { UserID = value; }
        }

        public string WorkstationId
        {
            get { return MachineName; }
            set { MachineName = value; }
        }

        public UserRole Role
        {
            get { return (UserRole)AppRole; }
            set { AppRole = (int)value; }
        }

        /// <summary>Alias for ADUserName (domain\username).</summary>
        public string AdIdentity
        {
            get { return ADUserName; }
            set { ADUserName = value; }
        }
    }
}