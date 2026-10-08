using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IApprovalFolderController
    {
        void RequestFolderDelete(int folderId, string reason, UserContext user);

        /// <summary>ส่งคำขออนุมัติดาวน์โหลดไฟล์ไปยัง Manager/Admin ที่เลือก</summary>
        ApprovalRequest SubmitDownloadRequest(int fileId, string reason, int approverId, UserContext user);

        /// <summary>ผู้อนุมัติที่เลือกได้สำหรับคำขอดาวน์โหลด (คนในแผนกเจ้าของไฟล์ที่ Level สูงกว่าผู้ขอ + Admin ต่อท้าย)</summary>
        IList<UserAccount> GetDownloadApprovers(int fileId, UserContext user);

        /// <summary>หา approved download request ที่ยังไม่ได้ใช้งาน (status=2)</summary>
        ApprovalRequest GetApprovedDownloadRequest(int fileId, int userId);

        /// <summary>บันทึกว่า approval ถูกใช้งานแล้ว หลัง download สำเร็จ</summary>
        void ConsumeDownloadApproval(int requestId, UserContext user);
    }
}
