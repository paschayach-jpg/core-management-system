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
        void ActivateOrgUnit(int orgUnitId, UserContext user);
        void DeleteOrgUnit(int orgUnitId, UserContext user);
        void ForceDeleteOrgUnit(int orgUnitId, UserContext user);
        bool HasActiveDependents(int orgUnitId);
        IEnumerable<OrgUnit> GetHierarchy();
        IEnumerable<OrgUnit> GetVisibleHierarchy(UserContext user);
        IEnumerable<OrgUnit> GetByConfidentialityLevel(ConfidentialityLevel level);

        /// <summary>เชื่อม Division ที่มีอยู่แล้วกับ SubCommittee ผ่าน OrgUnitRelationship table</summary>
        void LinkDivisionToSubCommittee(int divisionId, int subCommitteeId, UserContext user);

        /// <summary>ดึง relationship ที่ active ทั้งหมด (ใช้ build tree)</summary>
        IEnumerable<OrgUnitRelationship> GetAllActiveRelationships();

        /// <summary>ดึง OrgUnitId ที่ user ซ่อนไว้ใน OrgStructureForm</summary>
        IEnumerable<int> GetHiddenOrgUnitIds(int userId);

        /// <summary>บันทึก OrgUnitId ที่ซ่อน (แทนที่ทั้งหมดของ user นี้)</summary>
        void SaveHiddenOrgUnitIds(int userId, IEnumerable<int> orgUnitIds);
    }
}
