namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request DTO for updating an existing OrgUnit.
    /// </summary>
    public class UpdateOrgUnitRequest
    {
        public int OrgUnitID { get; set; }
        public string OrgUnitNameTH { get; set; }
        public string OrgUnitNameEN { get; set; }
        public int? ParentOrgUnitID { get; set; }
    }
}
