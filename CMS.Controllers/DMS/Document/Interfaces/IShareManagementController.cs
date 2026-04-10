using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IShareManagementController
    {
        IList<FilePermission> GetInternalSharesForUser(int userId, UserContext user);
        IList<ExternalShare> GetExternalSharesForUser(int userId, UserContext user);
        IList<ShareAccessLog> GetAccessLog(int externalShareId, UserContext user);
        void RevokeInternalShare(int filePermissionId, UserContext user);
        void ApproveExternalShare(int externalShareId, UserContext user);
        void RejectExternalShare(int externalShareId, string reason, UserContext user);
        void RevokeExternalShare(int externalShareId, string reason, UserContext user);
    }
}
