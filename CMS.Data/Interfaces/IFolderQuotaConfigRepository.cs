using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IFolderQuotaConfigRepository
    {
        /// <summary>ค้นหา config ตาม OrgUnit — ใช้ ParentOrgUnitID เป็น tiebreaker (เฉพาะ Sub มี priority สูงกว่า)</summary>
        FolderQuotaConfig GetByOrgUnit(int orgUnitId, int? parentOrgUnitId = null);
        FolderQuotaConfig GetByMasterCode(int orgUnitType, string orgUnitCode);

        IEnumerable<FolderQuotaConfig> GetAll();

        /// <summary>Insert หรือ Update (upsert by OrgUnitID + ParentOrgUnitID)</summary>
        void Save(FolderQuotaConfig config);

        void Delete(int quotaConfigId, string orgUnitTypeName);

        /// <summary>ดึงจำนวนไฟล์และขนาดที่ใช้ทุก OrgUnit ใน 1 query</summary>
        Dictionary<int, OrgUnitFileStat> GetAllFileStats();
    }
}
