using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IOrgStructureLookupController
    {
        IList<OrgUnit> GetHierarchy();
    }
}
