using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class ShareLookupController : IShareLookupController
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IFilePermissionRepository _filePermissionRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IFileDocumentRepository _fileDocumentRepository;
        private readonly IUserAccountRepository _userAccountRepository;

        public ShareLookupController(
            IPermissionRepository permissionRepository = null,
            IFilePermissionRepository filePermissionRepository = null,
            IFolderRepository folderRepository = null,
            IFileDocumentRepository fileDocumentRepository = null,
            IUserAccountRepository userAccountRepository = null)
        {
            _permissionRepository = permissionRepository;
            _filePermissionRepository = filePermissionRepository;
            _folderRepository = folderRepository;
            _fileDocumentRepository = fileDocumentRepository;
            _userAccountRepository = userAccountRepository;
        }

        public IList<FolderPermission> GetFolderPermissionsForUser(int userId)
        {
            if (_permissionRepository == null)
                return new List<FolderPermission>();

            try
            {
                return _permissionRepository
                    .GetByPrincipal((int)PrincipalType.User, userId)
                    .Where(p => p.RevokedDate == null)
                    .OrderByDescending(p => p.GrantedDate)
                    .ToList();
            }
            catch
            {
                return new List<FolderPermission>();
            }
        }

        public IList<FilePermission> GetFilePermissionsForUser(int userId)
        {
            if (_filePermissionRepository == null)
                return new List<FilePermission>();

            try
            {
                return _filePermissionRepository
                    .GetByPrincipal((int)PrincipalType.User, userId)
                    .ToList();
            }
            catch
            {
                return new List<FilePermission>();
            }
        }

        public Folder GetFolderById(int folderId)
        {
            if (_folderRepository == null)
                return null;

            try
            {
                return _folderRepository.GetById(folderId);
            }
            catch
            {
                return null;
            }
        }

        public FileDocument GetFileById(int fileId)
        {
            if (_fileDocumentRepository == null)
                return null;

            try
            {
                return _fileDocumentRepository.GetById(fileId);
            }
            catch
            {
                return null;
            }
        }

        public string ResolveGranterName(int userId)
        {
            if (_userAccountRepository == null)
                return string.Format("UserID: {0}", userId);

            try
            {
                var user = _userAccountRepository.GetById(userId);
                if (user == null)
                    return string.Format("UserID: {0}", userId);

                return string.IsNullOrEmpty(user.DisplayName) ? user.ADUserName : user.DisplayName;
            }
            catch
            {
                return string.Format("UserID: {0}", userId);
            }
        }

        public string ResolvePrincipalName(int principalType, int principalId)
        {
            if (principalType == (int)PrincipalType.User && _userAccountRepository != null)
            {
                try
                {
                    var user = _userAccountRepository.GetById(principalId);
                    if (user != null)
                    {
                        var name = user.DisplayName ?? user.ADUserName;
                        if (!string.IsNullOrEmpty(name))
                            return name;
                    }
                }
                catch
                {
                }
            }

            switch (principalType)
            {
                case (int)PrincipalType.User:
                    return string.Format("User #{0}", principalId);
                case (int)PrincipalType.Role:
                    return string.Format("Role #{0}", principalId);
                case (int)PrincipalType.Department:
                    return string.Format("OrgUnit #{0}", principalId);
                case (int)PrincipalType.Division:
                    return string.Format("Group #{0}", principalId);
                default:
                    return string.Format("#{0}", principalId);
            }
        }

        public string ResolveFileName(int fileId)
        {
            var file = GetFileById(fileId);
            if (file != null && !string.IsNullOrEmpty(file.OriginalFileName))
                return file.OriginalFileName;

            return string.Format("File #{0}", fileId);
        }

        public void RevokeFolderPermission(int permissionId, int revokedBy)
        {
            _permissionRepository?.Revoke(permissionId, revokedBy, "ยกเลิกโดย Admin");
        }

        public void RevokeFilePermission(int filePermissionId, int revokedBy)
        {
            _filePermissionRepository?.Revoke(filePermissionId, revokedBy, "ยกเลิกโดย Admin");
        }
    }
}
