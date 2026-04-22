using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IApprovalWorkflowController
    {
        IList<ApprovalRequest> GetAllApprovalsByFolder(int folderId);
        IList<ApprovalStep> GetApprovalSteps(int requestId);
        void ApproveStep(int stepId, string comment, UserContext approver);
    }
}
