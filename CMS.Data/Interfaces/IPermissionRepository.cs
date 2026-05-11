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

        /// <summary>
        /// Pre-load: returns all active ACL entries for every folder where the user (by UserID,
        /// Role, DepartmentId, or DivisionId) has at least one explicit entry.
        /// Used once at login to populate PermissionService._aclCache.
        /// </summary>
        IEnumerable<FolderPermission> GetAllForUser(int userId, int roleId, int? deptId, int? divId);
    }
}
