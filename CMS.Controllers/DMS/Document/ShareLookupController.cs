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
        private readonly IShareInboxReadRepository _shareInboxReadRepository;

        public ShareLookupController(
            IPermissionRepository permissionRepository = null,
            IFilePermissionRepository filePermissionRepository = null,
            IFolderRepository folderRepository = null,
            IFileDocumentRepository fileDocumentRepository = null,
            IUserAccountRepository userAccountRepository = null,
            IShareInboxReadRepository shareInboxReadRepository = null)
        {
            _permissionRepository = permissionRepository;
            _filePermissionRepository = filePermissionRepository;
            _folderRepository = folderRepository;
            _fileDocumentRepository = fileDocumentRepository;
            _userAccountRepository = userAccountRepository;
            _shareInboxReadRepository = shareInboxReadRepository;
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
                return string.Format("User #{0}", userId);

            try
            {
                var user = _userAccountRepository.GetById(userId);
                if (user != null && !string.IsNullOrEmpty(user.DisplayName))
                    return user.DisplayName;
            }
            catch
            {
            }

            return string.Format("User #{0}", userId);
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
                        if (!string.IsNullOrEmpty(user.DisplayName))
                            return user.DisplayName;
                        if (!string.IsNullOrEmpty(user.ADUserName))
                            return user.ADUserName;
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

        // ── Share Inbox (Unread / Read) ───────────────────────────────────────

        public ISet<string> GetReadShareKeys(int userId)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_shareInboxReadRepository == null)
                return set;

            try
            {
                var keys = _shareInboxReadRepository.GetReadKeys(userId);
                if (keys != null)
                {
                    foreach (var k in keys)
                        set.Add(string.Format("{0}_{1}", k.ShareKind, k.PermissionID));
                }
            }
            catch
            {
            }

            return set;
        }

        public int GetUnreadShareCount(int userId)
        {
            try
            {
                var readKeys = GetReadShareKeys(userId);
                var today = DateTime.Today;

                var folders = GetFolderPermissionsForUser(userId);
                int unreadFolders = folders.Count(p =>
                    p.CanRead && !p.IsDeny && p.RevokedDate == null
                    && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < today)
                    && !readKeys.Contains(string.Format("1_{0}", p.PermissionID))
                );

                var files = GetFilePermissionsForUser(userId);
                int unreadFiles = files.Count(p =>
                    p.CanRead && !p.IsDeny && p.RevokedDate == null
                    && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < today)
                    && !readKeys.Contains(string.Format("2_{0}", p.FilePermissionID))
                    && GetFileById(p.FileID) != null
                );

                return unreadFolders + unreadFiles;
            }
            catch
            {
                return 0;
            }
        }

        public void MarkShareRead(int userId, ShareInboxKind kind, int permissionId)
        {
            if (_shareInboxReadRepository == null) return;
            try
            {
                _shareInboxReadRepository.MarkRead(userId, (byte)kind, permissionId);
            }
            catch
            {
            }
        }

        public void MarkAllSharesRead(int userId)
        {
            if (_shareInboxReadRepository == null) return;
            try
            {
                var keys = new List<ShareInboxReadKey>();
                var folders = GetFolderPermissionsForUser(userId);
                foreach (var f in folders)
                    keys.Add(new ShareInboxReadKey(userId, ShareInboxKind.Folder, f.PermissionID));

                var files = GetFilePermissionsForUser(userId);
                foreach (var f in files)
                    keys.Add(new ShareInboxReadKey(userId, ShareInboxKind.File, f.FilePermissionID));

                _shareInboxReadRepository.MarkReadMany(userId, keys);
            }
            catch
            {
            }
        }
    }
}
