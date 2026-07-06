using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Organizational unit — 3-tier hierarchy: SubCommittee(1) -> Division(2) -> Department(3).
    /// Maps to EMS.OrgUnit in SCS database.
    /// </summary>
    public class OrgUnit
    {
        public int OrgUnitID { get; set; }
        public string OrgUnitCode { get; set; }
        public string OrgUnitNameTH { get; set; }
        public string OrgUnitNameEN { get; set; }

        /// <summary>1=SubCommittee, 2=Division, 3=Department</summary>
        public int OrgUnitType { get; set; }

        public int? ParentOrgUnitID { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>Confidentiality level: 1=HC, 2=SC, 3=NC, 4=PC</summary>
        public int ConfidentialityLevelValue { get; set; }

        // -----------------------------------------------------------------------
        // Friendly aliases used by service/presentation layers
        // -----------------------------------------------------------------------

        /// <summary>Alias for OrgUnitID.</summary>
        public int OrgUnitId
        {
            get { return OrgUnitID; }
            set { OrgUnitID = value; }
        }

        /// <summary>Alias for ParentOrgUnitID.</summary>
        public int? ParentId
        {
            get { return ParentOrgUnitID; }
            set { ParentOrgUnitID = value; }
        }

        /// <summary>Display name — falls back to OrgUnitNameEN then OrgUnitCode.</summary>
        public string Name
        {
            get { return !string.IsNullOrEmpty(OrgUnitNameTH) ? OrgUnitNameTH : (!string.IsNullOrEmpty(OrgUnitNameEN) ? OrgUnitNameEN : OrgUnitCode); }
            set { OrgUnitNameTH = value; }
        }

        /// <summary>Tier derived from OrgUnitType.</summary>
        public OrgUnitTier Tier
        {
            get { return (OrgUnitTier)OrgUnitType; }
            set { OrgUnitType = (int)value; }
        }

        /// <summary>Confidentiality level as enum, mapped from ConfidentialityLevelValue.</summary>
        public ConfidentialityLevel ConfidentialityLevel
        {
            get { return (ConfidentialityLevel)ConfidentialityLevelValue; }
            set { ConfidentialityLevelValue = (int)value; }
        }
    }
}
