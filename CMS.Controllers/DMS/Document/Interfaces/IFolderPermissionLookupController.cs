using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFolderPermissionLookupController
    {
        IList<FolderPermission> GetByFolder(int folderId);
        IList<FolderPermission> GetAllByFolder(int folderId);
        void Revoke(int permissionId, int revokedBy, string revokeReason);
    }
}
