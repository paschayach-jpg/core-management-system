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

        // -----------------------------------------------------------------------
        // Aliases used by tests
        // -----------------------------------------------------------------------

        /// <summary>Alias for OrgUnitID.</summary>
        public int OrgUnitId
        {
            get { return OrgUnitID; }
            set { OrgUnitID = value; }
        }

        /// <summary>Alias for OrgUnitNameTH (primary display name).</summary>
        public string NewName
        {
            get { return OrgUnitNameTH; }
            set { OrgUnitNameTH = value; }
        }
    }
}
