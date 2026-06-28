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

        // -----------------------------------------------------------------------
        // Aliases used by tests
        // -----------------------------------------------------------------------

        /// <summary>Alias for OrgUnitType as OrgUnitTier enum.</summary>
        public CMS.Domain.Enums.OrgUnitTier Tier
        {
            get { return (CMS.Domain.Enums.OrgUnitTier)OrgUnitType; }
            set { OrgUnitType = (int)value; }
        }

        /// <summary>Alias for OrgUnitNameTH (primary display name).</summary>
        public string Name
        {
            get { return OrgUnitNameTH; }
            set { OrgUnitNameTH = value; }
        }

        /// <summary>Alias for ParentOrgUnitID.</summary>
        public int? ParentId
        {
            get { return ParentOrgUnitID; }
            set { ParentOrgUnitID = value; }
        }

        /// <summary>
        /// ถ้ามีค่า = เป็นการ Link Division ที่มีอยู่แล้วเข้า SubCommittee (ใช้ OrgUnitRelationship)
        /// ไม่ได้สร้าง OrgUnit ใหม่
        /// </summary>
        public int? LinkExistingOrgUnitId { get; set; }

        /// <summary>
        /// Pattern used when creating this node. Selected by user via dialog before create.
        /// Default = DivisionToDepartment for backward compatibility.
        /// </summary>
        public CMS.Domain.Enums.OrgStructurePattern StructurePattern { get; set; }
            = CMS.Domain.Enums.OrgStructurePattern.Legacy;
    }
}
