using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class PermissionCheckController : IPermissionCheckController
    {
        private readonly IPermissionService _permissionService;

        public PermissionCheckController(IPermissionService permissionService = null)
        {
            _permissionService = permissionService;
        }

        public EffectivePermission GetEffectivePermission(int userId, int folderId)
        {
            if (_permissionService == null)
                return null;

            return _permissionService.GetEffectivePermission(userId, folderId);
        }

        public bool CheckPermission(int userId, int folderId, string permissionType)
        {
            if (_permissionService == null)
                return false;

            return _permissionService.CheckPermission(userId, folderId, permissionType);
        }

        public void PreloadAcl(Dictionary<int, List<FolderPermission>> sessionAcl)
        {
            _permissionService?.PreloadAcl(sessionAcl);
        }
    }
}
