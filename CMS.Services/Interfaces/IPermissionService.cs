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
        /// Propagates all active folder permissions to every file in the folder.
        /// Revokes existing file permissions for the same principals first, then inserts new ones.
        /// Returns the number of files updated.
        /// </summary>
        int PropagatePermissionsToFiles(int folderId, UserContext user);
    }
}
