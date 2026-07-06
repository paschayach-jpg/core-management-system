using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Manages per-user OrgUnit visibility permissions.
    /// Approach B: explicit grants take full precedence; role-based fallback applies
    /// only when a user has zero rows in security.UserOrgVisibility.
    /// Satisfies Requirements 4.1–4.7, 10.1–10.3.
    /// </summary>
    public class UserOrgVisibilityService : IUserOrgVisibilityService
    {
        private readonly IUserOrgVisibilityRepository _visibilityRepo;
        private readonly IOrgUnitRepository           _orgUnitRepo;
        private readonly IAuditService                _auditService;
        private readonly IUserAccountRepository       _userAccountRepo;

        public UserOrgVisibilityService(
            IUserOrgVisibilityRepository visibilityRepo,
            IOrgUnitRepository           orgUnitRepo,
            IAuditService                auditService = null,
            IUserAccountRepository       userAccountRepo = null)
        {
            if (visibilityRepo == null) throw new ArgumentNullException("visibilityRepo");
            if (orgUnitRepo    == null) throw new ArgumentNullException("orgUnitRepo");

            _visibilityRepo = visibilityRepo;
            _orgUnitRepo    = orgUnitRepo;
            _auditService   = auditService;
            _userAccountRepo = userAccountRepo;
        }

        // -----------------------------------------------------------------------
        // GetVisibleOrgUnits  (Req 4.2, 4.3, 10.1, 10.2, 10.3)
        // -----------------------------------------------------------------------

        public IEnumerable<OrgUnit> GetVisibleOrgUnits(int userId, UserContext caller)
        {
            if (caller == null) throw new ArgumentNullException("caller");

            var grants = _visibilityRepo.GetByUser(userId).ToList();

            if (grants.Count == 0)
            {
                if (userId == caller.UserID)
                {
                    return RoleBasedFallback(caller);
                }

                if (_userAccountRepo != null)
                {
                    var targetUser = _userAccountRepo.GetById(userId);
                    if (targetUser != null)
                    {
                        var targetContext = new UserContext
                        {
                            UserID = targetUser.UserID,
                            AppRole = targetUser.AppRole,
                            OrgUnitID = targetUser.OrgUnitID,
                            ADUserName = targetUser.ADUserName
                        };
                        return RoleBasedFallback(targetContext);
                    }
                }

                return RoleBasedFallback(caller);
            }

            // Explicit-grant mode: return only the granted OrgUnits
            var grantedIds = new HashSet<int>(grants.Select(g => g.OrgUnitID));
            return _orgUnitRepo.GetHierarchy()
                               .Where(o => grantedIds.Contains(o.OrgUnitID))
                               .ToList();
        }

        // -----------------------------------------------------------------------
        // GrantVisibility  (Req 4.4, 4.6)
        // -----------------------------------------------------------------------

        public void GrantVisibility(int userId, int orgUnitId, UserContext caller)
        {
            if (caller == null) throw new ArgumentNullException("caller");

            if (caller.AppRole < 4)
                throw new UnauthorizedAccessException(
                    "Granting OrgUnit visibility requires Admin role.");

            _visibilityRepo.Grant(new UserOrgVisibility
            {
                UserID      = userId,
                OrgUnitID   = orgUnitId,
                GrantedBy   = caller.UserID,
                GrantedDate = DateTime.Now
            });

            Log(caller, "GRANT",
                string.Format("Granted OrgUnit {0} visibility to User {1}.", orgUnitId, userId),
                userId);
        }

        // -----------------------------------------------------------------------
        // RevokeVisibility  (Req 4.5, 4.6)
        // -----------------------------------------------------------------------

        public void RevokeVisibility(int userId, int orgUnitId, UserContext caller)
        {
            if (caller == null) throw new ArgumentNullException("caller");

            if (caller.AppRole < 4)
                throw new UnauthorizedAccessException(
                    "Revoking OrgUnit visibility requires Admin role.");

            _visibilityRepo.Revoke(userId, orgUnitId);

            Log(caller, "REVOKE",
                string.Format("Revoked OrgUnit {0} visibility from User {1}.", orgUnitId, userId),
                userId);
        }

        // -----------------------------------------------------------------------
        // GetExplicitGrantsForUser  (UI helper — no fallback)
        // -----------------------------------------------------------------------

        public IEnumerable<UserOrgVisibility> GetExplicitGrantsForUser(int userId, UserContext caller)
        {
            if (caller == null) throw new ArgumentNullException("caller");
            return _visibilityRepo.GetByUser(userId).ToList();
        }

        // -----------------------------------------------------------------------
        // GetGrantsForOrgUnit  (Req 4.7)
        // -----------------------------------------------------------------------

        public IEnumerable<UserOrgVisibility> GetGrantsForOrgUnit(int orgUnitId, UserContext caller)
        {
            if (caller == null) throw new ArgumentNullException("caller");

            var grants = _visibilityRepo.GetByOrgUnit(orgUnitId).ToList();

            if (grants.Count == 0)
                return grants;

            // Populate transient display names
            var orgUnit = _orgUnitRepo.GetById(orgUnitId);
            var orgUnitName = orgUnit != null ? orgUnit.Name : null;

            // Build a lookup of grantedBy user names from the hierarchy (best-effort)
            var grantedByIds = new HashSet<int>(grants.Select(g => g.GrantedBy));
            var allOrgUnits  = _orgUnitRepo.GetHierarchy().ToList(); // used for context only

            foreach (var grant in grants)
            {
                grant.OrgUnitName = orgUnitName;
                // GrantedByName is populated by callers who have IUserAccountRepository;
                // here we set a placeholder so the field is never null
                grant.GrantedByName = string.Format("UserID:{0}", grant.GrantedBy);
            }

            return grants;
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Legacy role-based visibility rules applied when a user has no explicit grants.
        /// Admin(4) → all active OrgUnits
        /// Manager(3) → own OrgUnit + all descendants (BFS)
        /// Officer(1,2) → own OrgUnit only
        /// Satisfies Requirements 10.1, 10.2.
        /// </summary>
        private IEnumerable<OrgUnit> RoleBasedFallback(UserContext user)
        {
            var all = _orgUnitRepo.GetHierarchy().Where(o => o.IsActive && !o.IsDeleted).ToList();

            if (user.AppRole >= 4)
                return all; // Admin sees everything

            var own = all.FirstOrDefault(o => o.OrgUnitID == user.OrgUnitID);
            if (own == null)
                return Enumerable.Empty<OrgUnit>();

            if (user.AppRole <= 2)
                return new List<OrgUnit> { own }; // Officer — own unit only

            // Manager — own unit + all descendants via BFS
            return BfsDescendants(own, all);
        }

        private static IEnumerable<OrgUnit> BfsDescendants(OrgUnit root, IList<OrgUnit> all)
        {
            var result  = new List<OrgUnit>();
            var queue   = new Queue<int>();
            queue.Enqueue(root.OrgUnitID);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var node      = all.FirstOrDefault(o => o.OrgUnitID == currentId);
                if (node == null) continue;

                result.Add(node);

                foreach (var child in all.Where(o => o.ParentOrgUnitID == currentId))
                    queue.Enqueue(child.OrgUnitID);
            }

            return result;
        }

        private void Log(UserContext caller, string action, string detail, int targetUserId)
        {
            if (_auditService == null) return;
            _auditService.Log(new AuditEntry
            {
                Module      = "UserOrgVisibility",
                Action      = action,
                UserID      = caller.UserID,
                UserName    = caller.ADUserName ?? string.Empty,
                UserRole    = caller.AppRole,
                TableName   = "security.UserOrgVisibility",
                RecordID    = targetUserId,
                NewValue    = detail,
                MachineName = caller.MachineName,
                IPAddress   = caller.IPAddress,
                SessionID   = caller.SessionID,
                Result      = 1,
                AuditDate   = DateTime.Now
            });
        }
    }
}
