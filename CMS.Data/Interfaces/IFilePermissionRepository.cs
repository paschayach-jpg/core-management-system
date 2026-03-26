using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IFilePermissionRepository
    {
        int Insert(FilePermission permission);
        IEnumerable<FilePermission> GetByFile(int fileId);
        IEnumerable<FilePermission> GetAllByFile(int fileId); // includes revoked
        void Revoke(int filePermissionId, int revokedBy, string revokeReason);
        void DeleteByFile(int fileId);
    }
}
