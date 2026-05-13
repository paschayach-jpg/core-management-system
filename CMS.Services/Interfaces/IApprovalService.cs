using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    public interface IApprovalService
    {
        ApprovalRequest SubmitPermissionRequest(SubmitApprovalRequest request, UserContext user);
        void ApproveStep(int stepId, string remark, UserContext user);
        void RejectStep(int stepId, string reason, UserContext user);
        void WithdrawRequest(int requestId, UserContext user);
        void ProcessEscalations();
        IEnumerable<ApprovalRequest> GetPendingForApprover(int approverId);
        IEnumerable<ApprovalStep> GetApprovalSteps(int requestId);
        IEnumerable<ApprovalRequest> GetApprovalsByFolder(int folderId);
        IEnumerable<ApprovalRequest> GetAllApprovalsByFolder(int folderId);
        ApprovalRequest RequestFolderDelete(int folderId, string reason, UserContext user);

        /// <summary>ส่งคำขออนุมัติดาวน์โหลดไฟล์ — ทุก role ส่งได้</summary>
        ApprovalRequest SubmitDownloadRequest(int fileId, string reason, int approverId, UserContext user);

        /// <summary>หา approved download request ที่ยังไม่ได้ใช้งาน (status=2) สำหรับ fileId + userId</summary>
        ApprovalRequest GetApprovedDownloadRequest(int fileId, int userId);

        /// <summary>บันทึกว่า approval ถูกใช้งานแล้ว (status=2→5=Downloaded)</summary>
        void ConsumeDownloadApproval(int requestId, UserContext user);
    }
}
