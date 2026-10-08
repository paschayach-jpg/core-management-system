using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>ตั้งค่าผู้อนุมัติดาวน์โหลดไฟล์ต่อแผนก ('D') / ฝ่าย ('V')</summary>
    public interface IDownloadApproverConfigService
    {
        IList<DownloadApproverConfig> GetAll();
        IList<DownloadApproverConfig> GetByOrg(string scopeType, string orgCode);

        /// <summary>แทนที่รายชื่อผู้อนุมัติทั้งชุดของแผนก/ฝ่าย (Admin เท่านั้น) — ลำดับในรายการ = ลำดับที่แสดง</summary>
        void SaveForOrg(string scopeType, string orgCode, IList<int> approverUserIds, UserContext user);
    }
}
