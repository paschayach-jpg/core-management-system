using System.Collections.Generic;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    public interface IOrgStructureService
    {
        OrgUnit CreateOrgUnit(CreateOrgUnitRequest request, UserContext user);
        OrgUnit UpdateOrgUnit(UpdateOrgUnitRequest request, UserContext user);
        OrgUnit MoveOrgUnit(UpdateOrgUnitRequest request, UserContext user);
        void DeactivateOrgUnit(int orgUnitId, UserContext user);
        void DeleteOrgUnit(int orgUnitId, UserContext user);
        IEnumerable<OrgUnit> GetHierarchy();
        IEnumerable<OrgUnit> GetVisibleHierarchy(UserContext user);
        IEnumerable<OrgUnit> GetByConfidentialityLevel(ConfidentialityLevel level);
    }
}
