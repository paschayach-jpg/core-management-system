using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>ตั้งค่าผู้อนุมัติดาวน์โหลดไฟล์ ('D') หรือ ผู้อนุมัติแชร์ไฟล์ ('S') ต่อแผนก ('D') / ฝ่าย ('V')</summary>
    public interface IDownloadApproverConfigService
    {
        IList<DownloadApproverConfig> GetAll(string approvalType = DownloadApproverConfig.TypeDownload);
        IList<DownloadApproverConfig> GetByOrg(string scopeType, string orgCode, string approvalType = DownloadApproverConfig.TypeDownload);

        /// <summary>แทนที่รายชื่อผู้อนุมัติทั้งชุดของแผนก/ฝ่าย (Admin เท่านั้น) — ลำดับในรายการ = ลำดับที่แสดง</summary>
        void SaveForOrg(string scopeType, string orgCode, IList<int> approverUserIds, UserContext user, string approvalType = DownloadApproverConfig.TypeDownload);
    }
}
