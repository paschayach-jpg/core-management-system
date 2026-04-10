using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class ApprovalFolderController : IApprovalFolderController
    {
        private readonly IApprovalService _approvalService;

        public ApprovalFolderController(IApprovalService approvalService = null)
        {
            _approvalService = approvalService;
        }

        public void RequestFolderDelete(int folderId, string reason, UserContext user)
        {
            if (_approvalService == null || user == null)
                return;

            _approvalService.RequestFolderDelete(folderId, reason, user);
        }
    }
}
