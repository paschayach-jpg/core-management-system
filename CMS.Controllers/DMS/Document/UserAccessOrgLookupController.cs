using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class UserAccessOrgLookupController : IUserAccessOrgLookupController
    {
        private readonly IOrgStructureService _orgStructureService;

        public UserAccessOrgLookupController(IOrgStructureService orgStructureService = null)
        {
            _orgStructureService = orgStructureService;
        }

        public IList<OrgUnit> GetHierarchy()
        {
            if (_orgStructureService == null)
                return new List<OrgUnit>();

            return (_orgStructureService.GetHierarchy() ?? Enumerable.Empty<OrgUnit>()).ToList();
        }

        public IList<OrgUnit> GetVisibleHierarchy(UserContext user)
        {
            if (_orgStructureService == null)
                return new List<OrgUnit>();

            return (_orgStructureService.GetVisibleHierarchy(user) ?? Enumerable.Empty<OrgUnit>()).ToList();
        }

        public IList<OrgUnitRelationship> GetActiveRelationships()
        {
            if (_orgStructureService == null)
                return new List<OrgUnitRelationship>();

            return (_orgStructureService.GetAllActiveRelationships() ?? Enumerable.Empty<OrgUnitRelationship>()).ToList();
        }
    }
}
