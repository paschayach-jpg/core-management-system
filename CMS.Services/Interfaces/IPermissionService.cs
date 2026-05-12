using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    public interface IPermissionService
    {
        EffectivePermission GetEffectivePermission(int userId, int folderId);
        void SetFolderPermission(SetPermissionRequest request, UserContext user);
        bool CheckPermission(int userId, int folderId, string permissionType);

        /// <summary>Overload that accepts PermissionType enum.</summary>
        bool CheckPermission(int userId, int folderId, CMS.Domain.Enums.PermissionType permissionType);

        /// <summary>
        /// Pre-warm _aclCache จาก data ที่โหลดตอน login ครั้งเดียว
        /// ทำให้ CheckPermission ไม่ต้อง query DB ตลอด session
        /// </summary>
        void PreloadAcl(Dictionary<int, List<FolderPermission>> sessionAcl);

        /// <summary>
        /// Propagates all active folder permissions to every file in the folder.
        /// Revokes existing file permissions for the same principals first, then inserts new ones.
        /// Returns the number of files updated.
        /// </summary>
        int PropagatePermissionsToFiles(int folderId, UserContext user);
    }
}
