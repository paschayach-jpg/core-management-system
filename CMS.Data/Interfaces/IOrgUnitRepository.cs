using System.Collections.Generic;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IOrgUnitRepository
    {
        int Insert(OrgUnit orgUnit);
        void Update(OrgUnit orgUnit);
        OrgUnit GetById(int orgUnitId);
        IEnumerable<OrgUnit> GetHierarchy();
        IEnumerable<OrgUnit> GetByConfidentialityLevel(ConfidentialityLevel level);
        bool HasActiveDependents(int orgUnitId);
        void SoftDelete(int orgUnitId, int deletedBy);
    }
}
