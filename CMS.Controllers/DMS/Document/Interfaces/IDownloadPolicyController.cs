using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IDownloadPolicyController
    {
        /// <summary>ดาวน์โหลดได้ทันที / มีอนุมัติแล้ว / ต้องขออนุมัติ</summary>
        DownloadCheckResult CheckDownload(int fileId, UserContext user);

        /// <summary>ดาวน์โหลดเพื่อบันทึกไฟล์ (ตรวจกฎซ้ำ + ใช้คำขอที่อนุมัติแล้ว + audit)</summary>
        FileDownloadResult DownloadForSave(int fileId, UserContext user);

        IList<DownloadUserPermission> GetUserPermissions();
        void SaveUserPermission(DownloadUserPermission permission, UserContext user);
        void RemoveUserPermission(int userId, UserContext user);
    }
}
