using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IApprovalInboxController
    {
        IList<ApprovalRequest> GetPendingForApprover(int approverId);
        IList<ApprovalStep> GetApprovalSteps(int requestId);
        void ApproveStep(int stepId, string remark, UserContext user);
        void RejectStep(int stepId, string reason, UserContext user);
        void ProcessEscalations();
    }
}
