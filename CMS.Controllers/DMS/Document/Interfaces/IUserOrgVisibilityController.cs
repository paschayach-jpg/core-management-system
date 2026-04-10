using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IUserOrgVisibilityController
    {
        IList<OrgUnit> GetVisibleOrgUnits(int userId, UserContext caller);
        IList<UserOrgVisibility> GetExplicitGrantsForUser(int userId, UserContext caller);
        void GrantVisibility(int userId, int orgUnitId, UserContext caller);
        void RevokeVisibility(int userId, int orgUnitId, UserContext caller);
    }
}
