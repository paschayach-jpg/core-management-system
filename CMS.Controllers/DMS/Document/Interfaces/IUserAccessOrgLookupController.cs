using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IUserAccessOrgLookupController
    {
        IList<OrgUnit> GetHierarchy();
        IList<OrgUnit> GetVisibleHierarchy(UserContext user);
    }
}
