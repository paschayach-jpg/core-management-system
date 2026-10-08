using System.Collections.Generic;
using System.Linq;
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

        public ApprovalRequest SubmitDownloadRequest(int fileId, string reason, int approverId, UserContext user)
        {
            if (_approvalService == null || user == null)
                return null;

            return _approvalService.SubmitDownloadRequest(fileId, reason, approverId, user);
        }

        public IList<UserAccount> GetDownloadApprovers(int fileId, UserContext user)
        {
            if (_approvalService == null || user == null)
                return new List<UserAccount>();

            return _approvalService.GetDownloadApprovers(fileId, user).ToList();
        }

        public ApprovalRequest GetApprovedDownloadRequest(int fileId, int userId)
        {
            if (_approvalService == null)
                return null;

            return _approvalService.GetApprovedDownloadRequest(fileId, userId);
        }

        public void ConsumeDownloadApproval(int requestId, UserContext user)
        {
            if (_approvalService == null || user == null)
                return;

            _approvalService.ConsumeDownloadApproval(requestId, user);
        }
    }
}
