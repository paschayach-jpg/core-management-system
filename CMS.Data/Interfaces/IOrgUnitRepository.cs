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
        OrgUnit GetByCode(string orgUnitCode);
        bool ExistsByCode(string orgUnitCode);

        /// <summary>
        /// ตรวจสอบว่ามีชื่อ OrgUnitNameTH ซ้ำกันภายใต้ Parent เดียวกันหรือไม่
        /// excludeOrgUnitId ใช้สำหรับกรณี Update เพื่อยกเว้นตัวเองออกจากการตรวจ
        /// </summary>
        bool ExistsByNameUnderParent(string nameTH, int? parentId, int? excludeOrgUnitId = null);

        IEnumerable<OrgUnit> GetHierarchy();
        IEnumerable<OrgUnit> GetByConfidentialityLevel(ConfidentialityLevel level);
        bool HasActiveDependents(int orgUnitId);
        void SoftDelete(int orgUnitId, int deletedBy);

        /// <summary>Alias for SoftDelete — deactivates the OrgUnit (no deletedBy required).</summary>
        void Deactivate(int orgUnitId);
    }
}
