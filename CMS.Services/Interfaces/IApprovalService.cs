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
    }
}
