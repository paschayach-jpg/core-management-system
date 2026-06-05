namespace CMS.Domain.Models
{
    /// <summary>
    /// Many-to-many link between SubCommittee and Division.
    /// Maps to EMS.OrgUnitRelationship.
    /// </summary>
    public class OrgUnitRelationship
    {
        public int RelationshipID  { get; set; }
        public int SubCommitteeID  { get; set; }
        public int DivisionID      { get; set; }
        public bool IsActive       { get; set; }
    }
}

