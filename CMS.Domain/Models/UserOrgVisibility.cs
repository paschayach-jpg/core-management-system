using System;

namespace CMS.Domain.Models
{
    public class UserOrgVisibility
    {
        public int      UserID      { get; set; }
        public int      OrgUnitID   { get; set; }
        public int      GrantedBy   { get; set; }
        public DateTime GrantedDate { get; set; }

        // Transient — populated by service layer, not DB
        public string OrgUnitName   { get; set; }
        public string GrantedByName { get; set; }
    }
}
