using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class ApprovalWorkflowController : IApprovalWorkflowController
    {
        private readonly IApprovalService _approvalService;

        public ApprovalWorkflowController(IApprovalService approvalService = null)
        {
            _approvalService = approvalService;
        }

        public IList<ApprovalRequest> GetAllApprovalsByFolder(int folderId)
        {
            if (_approvalService == null)
                return new List<ApprovalRequest>();

            return (_approvalService.GetAllApprovalsByFolder(folderId) ?? Enumerable.Empty<ApprovalRequest>()).ToList();
        }

        public IList<ApprovalStep> GetApprovalSteps(int requestId)
        {
            if (_approvalService == null)
                return new List<ApprovalStep>();

            return (_approvalService.GetApprovalSteps(requestId) ?? Enumerable.Empty<ApprovalStep>()).ToList();
        }

        public void ApproveStep(int stepId, string comment, UserContext approver)
        {
            if (_approvalService == null)
                return;

            _approvalService.ApproveStep(stepId, comment, approver);
        }
    }
}
