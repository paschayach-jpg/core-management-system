using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IShareCommandController
    {
        FilePermission CreateInternalShare(CreateInternalShareRequest request, UserContext user);
        InternalShareBatchResult CreateInternalShareBatch(CreateInternalShareBatchRequest request, UserContext user);
        ExternalShare CreateExternalShare(CreateExternalShareRequest request, UserContext user);
        void SetFolderPermission(SetPermissionRequest request, UserContext user);
        void RequestFolderShareApproval(SetPermissionRequest request, UserContext user);
    }
}