using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FolderPermissionLookupController : IFolderPermissionLookupController
    {
        private readonly IPermissionRepository _permissionRepository;

        public FolderPermissionLookupController(IPermissionRepository permissionRepository = null)
        {
            _permissionRepository = permissionRepository;
        }

        public IList<FolderPermission> GetByFolder(int folderId)
        {
            if (_permissionRepository == null)
                return new List<FolderPermission>();

            return _permissionRepository.GetByFolder(folderId).ToList();
        }

        public IList<FolderPermission> GetAllByFolder(int folderId)
        {
            if (_permissionRepository == null)
                return new List<FolderPermission>();

            return _permissionRepository.GetAllByFolder(folderId).ToList();
        }

        public void Revoke(int permissionId, int revokedBy, string revokeReason)
        {
            if (_permissionRepository == null)
                return;

            _permissionRepository.Revoke(permissionId, revokedBy, revokeReason);
        }
    }
}
