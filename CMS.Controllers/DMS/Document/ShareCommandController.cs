using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class ShareCommandController : IShareCommandController
    {
        private readonly IShareService _shareService;
        private readonly IPermissionService _permissionService;

        public ShareCommandController(IShareService shareService = null, IPermissionService permissionService = null)
        {
            _shareService = shareService;
            _permissionService = permissionService;
        }

        public FilePermission CreateInternalShare(CreateInternalShareRequest request, UserContext user)
        {
            if (_shareService == null)
                return null;

            return _shareService.CreateInternalShare(request, user);
        }

        public InternalShareBatchResult CreateInternalShareBatch(CreateInternalShareBatchRequest request, UserContext user)
        {
            if (_shareService == null)
                return new InternalShareBatchResult();

            return _shareService.CreateInternalShareBatch(request, user);
        }

        public ExternalShare CreateExternalShare(CreateExternalShareRequest request, UserContext user)
        {
            if (_shareService == null)
                return null;

            return _shareService.CreateExternalShare(request, user);
        }

        public void SetFolderPermission(SetPermissionRequest request, UserContext user)
        {
            if (_permissionService == null)
                return;

            _permissionService.SetFolderPermission(request, user);
        }

        public void RequestFolderShareApproval(SetPermissionRequest request, UserContext user)
        {
            if (_shareService == null)
                return;

            _shareService.RequestFolderShareApproval(request, user);
        }
    }
}