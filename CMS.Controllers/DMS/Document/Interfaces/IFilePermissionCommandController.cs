using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFilePermissionCommandController
    {
        void SetFilePermission(FileDocument file, SetFilePermissionRequest request, UserContext user);
        void RevokeFilePermission(FileDocument file, FilePermission entry, UserContext user, string reason);
        int GetActiveFolderPermissionCount(int folderId);
        int SyncFromFolderPermissions(FileDocument file, IList<FilePermission> currentAclEntries, UserContext user);
    }
}
