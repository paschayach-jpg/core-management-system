using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IPermissionRepository
    {
        int Insert(FolderPermission permission);
        IEnumerable<FolderPermission> GetByFolder(int folderId);
        IEnumerable<FolderPermission> GetAllByFolder(int folderId); // includes revoked
        IEnumerable<FolderPermission> GetByPrincipal(int principalType, int principalId);
        void Revoke(int permissionId, int revokedBy, string revokeReason);
        void DeleteByFolder(int folderId);

        /// <summary>Alias for GetByFolder — returns active ACL entries for the folder.</summary>
        IEnumerable<FolderPermission> GetAclEntries(int folderId);
    }
}
