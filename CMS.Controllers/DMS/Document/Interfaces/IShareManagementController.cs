using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IShareManagementController
    {
        IList<FilePermission> GetInternalSharesForUser(int userId, UserContext user);
        IList<InternalShareRequestItem> GetInternalShareRequestsForUser(int userId, UserContext user);
        IList<ExternalShare> GetExternalSharesForUser(int userId, UserContext user);
        IList<FilePermission> GetAllInternalShares(UserContext admin);
        IList<ExternalShare>  GetAllExternalShares(UserContext admin);
        IList<ShareAccessLog> GetAccessLog(int externalShareId, UserContext user);
        FilePermission GetInternalShareById(int filePermissionId, UserContext user);
        void UpdateInternalSharePermission(UpdateSharePermissionRequest request, UserContext user);
        void RevokeInternalShare(int filePermissionId, UserContext user);
        void WithdrawInternalShareRequest(int requestId, UserContext user);
        void ApproveExternalShare(int externalShareId, UserContext user);
        void RejectExternalShare(int externalShareId, string reason, UserContext user);
        void RevokeExternalShare(int externalShareId, string reason, UserContext user);
        void ToggleExternalShare(int externalShareId, UserContext user);
    }
}
