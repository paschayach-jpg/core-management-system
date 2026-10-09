using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class ShareManagementController : IShareManagementController
    {
        private readonly IShareService _shareService;
        private readonly IApprovalService _approvalService;

        public ShareManagementController(IShareService shareService = null, IApprovalService approvalService = null)
        {
            _shareService = shareService;
            _approvalService = approvalService;
        }

        public IList<FilePermission> GetInternalSharesForUser(int userId, UserContext user)
        {
            if (_shareService == null)
                return new List<FilePermission>();

            return _shareService.GetInternalSharesForUser(userId, user).ToList();
        }

        public IList<InternalShareRequestItem> GetInternalShareRequestsForUser(int userId, UserContext user)
        {
            if (_shareService == null)
                return new List<InternalShareRequestItem>();

            return _shareService.GetInternalShareRequestsForUser(userId, user).ToList();
        }

        public IList<ExternalShare> GetExternalSharesForUser(int userId, UserContext user)
        {
            if (_shareService == null)
                return new List<ExternalShare>();

            return _shareService.GetExternalSharesForUser(userId, user).ToList();
        }

        public IList<FilePermission> GetAllInternalShares(UserContext admin)
        {
            if (_shareService == null)
                return new List<FilePermission>();

            return _shareService.GetAllInternalShares(admin).ToList();
        }

        public IList<ExternalShare> GetAllExternalShares(UserContext admin)
        {
            if (_shareService == null)
                return new List<ExternalShare>();

            return _shareService.GetAllExternalShares(admin).ToList();
        }

        public IList<ShareAccessLog> GetAccessLog(int externalShareId, UserContext user)
        {
            if (_shareService == null)
                return new List<ShareAccessLog>();

            return _shareService.GetAccessLog(externalShareId, user).ToList();
        }

        public FilePermission GetInternalShareById(int filePermissionId, UserContext user)
        {
            if (_shareService == null)
                return null;

            return _shareService.GetInternalShareById(filePermissionId, user);
        }

        public void UpdateInternalSharePermission(UpdateSharePermissionRequest request, UserContext user)
        {
            if (_shareService == null)
                throw new System.InvalidOperationException("ระบบ ShareService ไม่พร้อมใช้งาน กรุณาติดต่อผู้ดูแลระบบ");

            _shareService.UpdateInternalSharePermission(request, user);
        }

        public void RevokeInternalShare(int filePermissionId, UserContext user)
        {
            if (_shareService == null)
                return;

            _shareService.RevokeInternalShare(filePermissionId, user);
        }

        public void WithdrawInternalShareRequest(int requestId, UserContext user)
        {
            if (_approvalService == null)
                throw new System.InvalidOperationException("ระบบขออนุมัติไม่พร้อมใช้งาน กรุณาติดต่อผู้ดูแลระบบ");

            // ApprovalService checks requester + pending status, closes the steps and writes the audit log
            _approvalService.WithdrawRequest(requestId, user);
        }

        public void ApproveExternalShare(int externalShareId, UserContext user)
        {
            if (_shareService == null)
                return;

            _shareService.ApproveExternalShare(externalShareId, user);
        }

        public void RejectExternalShare(int externalShareId, string reason, UserContext user)
        {
            if (_shareService == null)
                return;

            _shareService.RejectExternalShare(externalShareId, reason, user);
        }

        public void RevokeExternalShare(int externalShareId, string reason, UserContext user)
        {
            if (_shareService == null)
                return;

            _shareService.RevokeExternalShare(externalShareId, reason, user);
        }

        public void ToggleExternalShare(int externalShareId, UserContext user)
        {
            if (_shareService == null)
                return;

            _shareService.ToggleExternalShare(externalShareId, user);
        }
    }
}
