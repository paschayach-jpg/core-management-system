using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FilePermissionAclController : IFilePermissionAclController
    {
        private readonly IFilePermissionRepository _filePermissionRepository;

        public FilePermissionAclController(IFilePermissionRepository filePermissionRepository = null)
        {
            _filePermissionRepository = filePermissionRepository;
        }

        public IList<FilePermission> GetByFile(int fileId)
        {
            if (_filePermissionRepository == null)
                return new List<FilePermission>();

            return _filePermissionRepository.GetByFile(fileId).ToList();
        }

        public void Revoke(int filePermissionId, int revokedBy, string revokeReason)
        {
            if (_filePermissionRepository == null)
                return;

            _filePermissionRepository.Revoke(filePermissionId, revokedBy, revokeReason);
        }

        public int Insert(FilePermission permission)
        {
            if (_filePermissionRepository == null || permission == null)
                return 0;

            return _filePermissionRepository.Insert(permission);
        }
    }
}
