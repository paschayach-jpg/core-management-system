using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// ตัดสินว่าผู้ใช้ดาวน์โหลดไฟล์ได้ทันทีหรือต้องขออนุมัติ และจัดการสิทธิ์ดาวน์โหลดรายคน
    /// ลำดับ: Admin → สิทธิ์รายคน → Authority Config (CanDownload) → คำขอที่อนุมัติแล้ว → ต้องขออนุมัติ
    /// </summary>
    public interface IDownloadPolicyService
    {
        DownloadCheckResult CheckDownload(int fileId, UserContext user);

        /// <summary>
        /// ดาวน์โหลดเพื่อบันทึกไฟล์: ตรวจกฎซ้ำ (โยน UnauthorizedAccessException ถ้าต้องขออนุมัติ),
        /// ใช้คำขอที่อนุมัติแล้ว (1 ครั้ง) และบันทึก audit
        /// </summary>
        FileDownloadResult DownloadForSave(int fileId, UserContext user);

        // ── สิทธิ์ดาวน์โหลดรายคน (Admin เท่านั้น) ──────────────────────────
        IList<DownloadUserPermission> GetUserPermissions();
        void SaveUserPermission(DownloadUserPermission permission, UserContext user);
        void RemoveUserPermission(int userId, UserContext user);
    }
}
