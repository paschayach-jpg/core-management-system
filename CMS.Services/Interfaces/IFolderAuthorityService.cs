using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IFolderAuthorityService
    {
        IEnumerable<FolderAuthorityConfig> GetAll();

        /// <summary>
        /// Resolve สิทธิ์ทั้ง 4 Level สำหรับพนักงาน 1 คน
        /// Return dict: key=ConfidentialityLevel (1-4), value=config
        /// ถ้าไม่พบ match คืน dict ว่าง (ไม่มีสิทธิ์เลย)
        /// </summary>
        System.Collections.Generic.Dictionary<int, FolderAuthorityConfig> ResolveForEmployee(
            string positionName, string departmentCode, string divisionCode, string levelCode = null);

        /// <summary>Resolve จาก UserAccount โดยตรง</summary>
        System.Collections.Generic.Dictionary<int, FolderAuthorityConfig> ResolveForUser(UserAccount user);

        /// <summary>บันทึก 1 row (Insert หรือ Update)</summary>
        int  Save(FolderAuthorityConfig config, int userId);

        /// <summary>ลบทั้ง group (ทุก Level ของ ConfigLabel เดียวกัน)</summary>
        void Delete(string configLabel, int userId);
    }
}
