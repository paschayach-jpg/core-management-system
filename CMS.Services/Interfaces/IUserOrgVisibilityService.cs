using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service that manages per-user OrgUnit visibility permissions.
    /// Encapsulates role-based fallback logic and explicit grant management.
    /// Satisfies Requirements 4.1–4.7, 10.1–10.3.
    /// </summary>
    public interface IUserOrgVisibilityService
    {
        /// <summary>
        /// Returns the OrgUnits visible to the specified user.
        /// If the user has no explicit grants, falls back to role-based rules.
        /// If the user has at least one grant, returns only the granted set.
        /// </summary>
        IEnumerable<OrgUnit> GetVisibleOrgUnits(int userId, UserContext caller);

        /// <summary>
        /// Grants visibility of an OrgUnit to a user.
        /// Caller must be Admin (AppRole == 4); throws UnauthorizedAccessException otherwise.
        /// </summary>
        void GrantVisibility(int userId, int orgUnitId, UserContext caller);

        /// <summary>
        /// Revokes visibility of an OrgUnit from a user.
        /// Caller must be Admin (AppRole == 4); throws UnauthorizedAccessException otherwise.
        /// Silent no-op if the grant does not exist.
        /// </summary>
        void RevokeVisibility(int userId, int orgUnitId, UserContext caller);

        /// <summary>
        /// Returns all visibility grants for a given OrgUnit, with transient
        /// OrgUnitName and GrantedByName populated.
        /// </summary>
        IEnumerable<UserOrgVisibility> GetGrantsForOrgUnit(int orgUnitId, UserContext caller);
    }
}
