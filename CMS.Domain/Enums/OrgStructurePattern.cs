namespace CMS.Domain.Enums
{
    /// <summary>
    /// Defines which node-level pattern is used when creating an org structure.
    /// Selected per-operation via dialog before create.
    /// Legacy = original 3-tier (SubCommittee → Division → Department), used for backward compat.
    /// </summary>
    public enum OrgStructurePattern
    {
        Legacy               = 0,  // Original 3-tier: SubCommittee → Division → Department
        DivisionToDepartment = 1,  // 2-tier: Division (root) → Department
        LevelZeroOnly        = 2   // Flat: Division nodes only, no parent allowed
    }
}
