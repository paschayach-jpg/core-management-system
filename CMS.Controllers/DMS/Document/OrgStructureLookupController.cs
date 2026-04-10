using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class OrgStructureLookupController : IOrgStructureLookupController
    {
        private readonly IOrgStructureService _orgStructureService;

        public OrgStructureLookupController(IOrgStructureService orgStructureService = null)
        {
            _orgStructureService = orgStructureService;
        }

        public IList<OrgUnit> GetHierarchy()
        {
            if (_orgStructureService == null)
                return new List<OrgUnit>();

            return _orgStructureService.GetHierarchy().ToList();
        }
    }
}
