using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IApprovalRepository
    {
        int InsertRequest(ApprovalRequest request);
        int InsertStep(ApprovalStep step);
        ApprovalRequest GetRequest(int requestId);
        IEnumerable<ApprovalStep> GetPendingSteps(int requestId);
        IEnumerable<ApprovalStep> GetAllSteps(int requestId);
        IEnumerable<ApprovalRequest> GetExpiredPendingRequests();
        IEnumerable<ApprovalRequest> GetPendingRequests();
        IEnumerable<ApprovalRequest> GetAllRequestsByFolder(int folderId);
        void UpdateStepStatus(int stepId, int stepStatus, string actionRemark);
        void UpdateRequestStatus(int requestId, int requestStatus, int resolvedBy, string resolveRemark);

        /// <summary>Hard-deletes all ApprovalSteps then ApprovalRequests for the given folder (used during permanent folder deletion).</summary>
        void DeleteByFolder(int folderId);

        /// <summary>ลบ pending CREATE_HC requests (และ steps) ทั้งหมดของ OrgUnit ที่กำหนด เพื่อเคลียร์ข้อมูลเก่าก่อนสร้างคำขอใหม่</summary>
        void DeletePendingHCRequestsForOrgUnit(int orgUnitId);
    }
}
