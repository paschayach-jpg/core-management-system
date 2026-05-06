using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IShareLookupController
    {
        IList<FolderPermission> GetFolderPermissionsForUser(int userId);
        IList<FilePermission> GetFilePermissionsForUser(int userId);
        Folder GetFolderById(int folderId);
        FileDocument GetFileById(int fileId);
        string ResolveGranterName(int userId);
        string ResolvePrincipalName(int principalType, int principalId);
        string ResolveFileName(int fileId);
        void RevokeFolderPermission(int permissionId, int revokedBy);
        void RevokeFilePermission(int filePermissionId, int revokedBy);
    }
}
