using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFilePermissionAclController
    {
        IList<FilePermission> GetByFile(int fileId);
        void Revoke(int filePermissionId, int revokedBy, string revokeReason);
        int Insert(FilePermission permission);
    }
}
