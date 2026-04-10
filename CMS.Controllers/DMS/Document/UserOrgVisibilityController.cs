using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class UserOrgVisibilityController : IUserOrgVisibilityController
    {
        private readonly IUserOrgVisibilityService _visibilityService;

        public UserOrgVisibilityController(IUserOrgVisibilityService visibilityService = null)
        {
            _visibilityService = visibilityService;
        }

        public IList<OrgUnit> GetVisibleOrgUnits(int userId, UserContext caller)
        {
            if (_visibilityService == null)
                return new List<OrgUnit>();

            return (_visibilityService.GetVisibleOrgUnits(userId, caller) ?? Enumerable.Empty<OrgUnit>()).ToList();
        }

        public IList<UserOrgVisibility> GetExplicitGrantsForUser(int userId, UserContext caller)
        {
            if (_visibilityService == null)
                return new List<UserOrgVisibility>();

            return (_visibilityService.GetExplicitGrantsForUser(userId, caller) ?? Enumerable.Empty<UserOrgVisibility>()).ToList();
        }

        public void GrantVisibility(int userId, int orgUnitId, UserContext caller)
        {
            if (_visibilityService == null)
                return;

            _visibilityService.GrantVisibility(userId, orgUnitId, caller);
        }

        public void RevokeVisibility(int userId, int orgUnitId, UserContext caller)
        {
            if (_visibilityService == null)
                return;

            _visibilityService.RevokeVisibility(userId, orgUnitId, caller);
        }
    }
}
