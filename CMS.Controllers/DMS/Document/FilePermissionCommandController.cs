using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FilePermissionCommandController : IFilePermissionCommandController
    {
        private readonly IFilePermissionService _filePermissionService;
        private readonly IFilePermissionAclController _filePermissionAclController;
        private readonly IFolderPermissionLookupController _folderPermissionLookupController;
        private readonly IAuditService _auditService;

        public FilePermissionCommandController(
            IFilePermissionService filePermissionService = null,
            IFilePermissionAclController filePermissionAclController = null,
            IFolderPermissionLookupController folderPermissionLookupController = null,
            IAuditService auditService = null)
        {
            _filePermissionService = filePermissionService;
            _filePermissionAclController = filePermissionAclController;
            _folderPermissionLookupController = folderPermissionLookupController;
            _auditService = auditService;
        }

        public void SetFilePermission(FileDocument file, SetFilePermissionRequest request, UserContext user)
        {
            if (_filePermissionService == null)
                throw new InvalidOperationException("File permission service is not available.");
            if (file == null)
                throw new ArgumentNullException("file");

            _filePermissionService.SetFilePermission(request, user);

            if (_auditService == null)
                return;

            _auditService.Log(new AuditEntry
            {
                Module = "FilePermission",
                Action = "SET_FILE_PERMISSION",
                UserID = user.UserID,
                TableName = "FilePermission",
                RecordID = file.FileID,
                NewValue = string.Format("File:{0} PrincipalType:{1} PrincipalID:{2} IsDeny:{3}",
                    file.OriginalFileName,
                    request.PrincipalType,
                    request.PrincipalID,
                    request.IsDeny),
                MachineName = user.MachineName,
                IPAddress = user.IPAddress,
                SessionID = user.SessionID,
                Result = 1,
                AuditDate = DateTime.Now
            });
        }

        public void RevokeFilePermission(FileDocument file, FilePermission entry, UserContext user, string reason)
        {
            if (_filePermissionService == null)
                throw new InvalidOperationException("File permission service is not available.");
            if (file == null)
                throw new ArgumentNullException("file");
            if (entry == null)
                throw new ArgumentNullException("entry");

            _filePermissionService.RevokeFilePermission(entry.FilePermissionID, user, reason);

            if (_auditService == null)
                return;

            _auditService.Log(new AuditEntry
            {
                Module = "FilePermission",
                Action = "REVOKE_FILE_PERMISSION",
                UserID = user.UserID,
                TableName = "FilePermission",
                RecordID = file.FileID,
                OldValue = string.Format("File:{0} FilePermissionID:{1}",
                    file.OriginalFileName,
                    entry.FilePermissionID),
                MachineName = user.MachineName,
                IPAddress = user.IPAddress,
                SessionID = user.SessionID,
                Result = 1,
                AuditDate = DateTime.Now
            });
        }

        public int GetActiveFolderPermissionCount(int folderId)
        {
            if (_folderPermissionLookupController == null)
                return 0;

            return _folderPermissionLookupController.GetByFolder(folderId)
                .Count(p => p.RevokedDate == null
                    && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today));
        }

        public int SyncFromFolderPermissions(FileDocument file, IList<FilePermission> currentAclEntries, UserContext user)
        {
            if (_filePermissionAclController == null)
                throw new InvalidOperationException("File permission ACL controller is not available.");
            if (_folderPermissionLookupController == null)
                throw new InvalidOperationException("Folder permission lookup controller is not available.");
            if (file == null)
                throw new ArgumentNullException("file");

            var folderPerms = _folderPermissionLookupController.GetByFolder(file.FolderID)
                .Where(p => p.RevokedDate == null
                         && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today))
                .ToList();

            if (currentAclEntries != null)
            {
                foreach (var fp in currentAclEntries)
                    _filePermissionAclController.Revoke(fp.FilePermissionID, user.UserID, "ซิงค์จาก Folder");
            }

            foreach (var fp in folderPerms)
            {
                _filePermissionAclController.Insert(new FilePermission
                {
                    FileID = file.FileID,
                    PrincipalType = (int)fp.PrincipalType,
                    PrincipalID = fp.PrincipalID,
                    CanRead = fp.CanRead,
                    CanWrite = fp.CanWrite,
                    CanDelete = fp.CanDelete,
                    CanModify = fp.CanModify,
                    CanFullControl = fp.CanFullControl,
                    IsDeny = fp.IsDeny,
                    ExpiryDate = fp.ExpiryDate,
                    GrantedBy = user.UserID,
                    GrantedDate = DateTime.Now
                });
            }

            if (_auditService != null)
            {
                _auditService.Log(new AuditEntry
                {
                    Module = "FilePermission",
                    Action = "SYNC_FILE_PERMISSION",
                    UserID = user.UserID,
                    TableName = "FilePermission",
                    RecordID = file.FileID,
                    NewValue = string.Format("File:{0} SyncedFromFolder:{1} Count:{2}",
                        file.OriginalFileName,
                        file.FolderID,
                        folderPerms.Count),
                    MachineName = user.MachineName,
                    IPAddress = user.IPAddress,
                    SessionID = user.SessionID,
                    Result = 1,
                    AuditDate = DateTime.Now
                });
            }

            return folderPerms.Count;
        }
    }
}
