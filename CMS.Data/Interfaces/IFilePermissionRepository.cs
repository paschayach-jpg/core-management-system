using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IFilePermissionRepository
    {
        int Insert(FilePermission permission);
        FilePermission GetById(int filePermissionId);
        IEnumerable<FilePermission> GetByFile(int fileId);
        IEnumerable<FilePermission> GetAllByFile(int fileId); // includes revoked
        IEnumerable<FilePermission> GetByGrantedBy(int userId);

        /// <summary>ดึง FilePermission ที่ share ให้ principal นี้ (ใช้สำหรับ Shared With Me)</summary>
        IEnumerable<FilePermission> GetByPrincipal(int principalType, int principalId);

        void Revoke(int filePermissionId, int revokedBy, string revokeReason);
        void DeleteByFile(int fileId);
    }
}
