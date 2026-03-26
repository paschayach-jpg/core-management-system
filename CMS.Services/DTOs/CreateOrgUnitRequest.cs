namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request DTO for creating a new OrgUnit.
    /// OrgUnitType: 1=SubCommittee, 2=Division, 3=Department
    /// </summary>
    public class CreateOrgUnitRequest
    {
        /// <summary>1=SubCommittee, 2=Division, 3=Department</summary>
        public int OrgUnitType { get; set; }

        public string OrgUnitNameTH { get; set; }
        public string OrgUnitNameEN { get; set; }
        public string OrgUnitCode { get; set; }

        /// <summary>Parent OrgUnitID. Required for Division and Department; null for SubCommittee.</summary>
        public int? ParentOrgUnitID { get; set; }
    }
}
