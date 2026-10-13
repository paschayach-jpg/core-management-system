using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// SubCommittee → Division membership for org trees. A Division belongs to its own parent
    /// (ParentOrgUnitID) and to every SubCommittee it is linked to via EMS.OrgUnitRelationship.
    /// </summary>
    internal sealed class OrgTreeLinks
    {
        private readonly Dictionary<int, HashSet<int>> _divisionsBySubCommittee = new Dictionary<int, HashSet<int>>();
        private readonly HashSet<int> _linkedDivisions = new HashSet<int>();

        public static readonly OrgTreeLinks Empty = new OrgTreeLinks(null);

        public OrgTreeLinks(IEnumerable<OrgUnitRelationship> relationships)
        {
            if (relationships == null) return;

            foreach (var r in relationships)
            {
                if (r == null || !r.IsActive) continue;

                HashSet<int> set;
                if (!_divisionsBySubCommittee.TryGetValue(r.SubCommitteeID, out set))
                {
                    set = new HashSet<int>();
                    _divisionsBySubCommittee[r.SubCommitteeID] = set;
                }
                set.Add(r.DivisionID);
                _linkedDivisions.Add(r.DivisionID);
            }
        }

        /// <summary>True when <paramref name="division"/> is the parent-owned child or a linked child of the SubCommittee.</summary>
        public bool BelongsTo(OrgUnit division, int subCommitteeId)
        {
            return division != null && (division.ParentOrgUnitID == subCommitteeId || IsLinkedUnder(division, subCommitteeId));
        }

        /// <summary>True when the Division appears under the SubCommittee only through a relationship (not its own parent).</summary>
        public bool IsLinkedUnder(OrgUnit division, int subCommitteeId)
        {
            HashSet<int> set;
            return division != null
                && division.ParentOrgUnitID != subCommitteeId
                && _divisionsBySubCommittee.TryGetValue(subCommitteeId, out set)
                && set.Contains(division.OrgUnitID);
        }

        /// <summary>True when the Division is linked to at least one SubCommittee.</summary>
        public bool HasAnyLink(OrgUnit division)
        {
            return division != null && _linkedDivisions.Contains(division.OrgUnitID);
        }
    }
}
