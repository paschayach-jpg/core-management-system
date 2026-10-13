using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class ApprovalInboxController : IApprovalInboxController
    {
        private readonly IApprovalService _approvalService;

        public ApprovalInboxController(IApprovalService approvalService = null)
        {
            _approvalService = approvalService;
        }

        public IList<ApprovalRequest> GetPendingForApprover(int approverId)
        {
            if (_approvalService == null)
                return new List<ApprovalRequest>();

            return _approvalService.GetPendingForApprover(approverId).ToList();
        }

        public IList<ApprovalStep> GetApprovalSteps(int requestId)
        {
            if (_approvalService == null)
                return new List<ApprovalStep>();

            return _approvalService.GetApprovalSteps(requestId).ToList();
        }

        public void ApproveStep(int stepId, string remark, UserContext user)
        {
            if (_approvalService == null)
                return;

            _approvalService.ApproveStep(stepId, remark, user);
        }

        public void RejectStep(int stepId, string reason, UserContext user)
        {
            if (_approvalService == null)
                return;

            _approvalService.RejectStep(stepId, reason, user);
        }

        public void ProcessEscalations()
        {
            if (_approvalService == null)
                return;

            _approvalService.ProcessEscalations();
        }

        public IList<ApprovalRequestItem> GetApprovalRequestItems(int requestId)
        {
            if (_approvalService == null)
                return new List<ApprovalRequestItem>();

            return _approvalService.GetApprovalRequestItems(requestId).ToList();
        }

        public IDictionary<int, string> GetRequestDepartmentNames(IEnumerable<ApprovalRequest> requests)
        {
            if (_approvalService == null)
                return new Dictionary<int, string>();

            return _approvalService.GetRequestDepartmentNames(requests);
        }
    }
}
