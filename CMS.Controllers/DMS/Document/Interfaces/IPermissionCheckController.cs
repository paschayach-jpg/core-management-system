using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IPermissionCheckController
    {
        EffectivePermission GetEffectivePermission(int userId, int folderId);
        bool CheckPermission(int userId, int folderId, string permissionType);

        /// <summary>Pre-warm PermissionService cache จาก session ACL ที่โหลดตอน login</summary>
        void PreloadAcl(Dictionary<int, List<FolderPermission>> sessionAcl);
    }
}
