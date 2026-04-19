using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class FileService : IFileService
    {
        private const long   MaxFileSizeBytes = 100L * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx",
            ".ppt", ".pptx", ".jpg", ".png", ".gif", ".zip"
        };

        private readonly IFileDocumentRepository  _fileDocumentRepository;
        private readonly IFolderRepository        _folderRepository;
        private readonly IOrgUnitRepository       _orgUnitRepository;
        private readonly IVirusScanService        _virusScanService;
        private readonly IPermissionService       _permissionService;
        private readonly INetworkDriveHelper      _networkDriveHelper;
        private readonly IAuditService            _auditService;
        private readonly INotificationRepository  _notificationRepository;
        private readonly IUserAccountRepository   _userAccountRepository;
        private readonly ILoggingService          _loggingService;
        private readonly IPermissionRepository    _permissionRepository;
        private readonly IFilePermissionRepository _filePermissionRepository;

        public FileService(
            IFileDocumentRepository  fileDocumentRepository,
            IFolderRepository        folderRepository,
            IOrgUnitRepository       orgUnitRepository,
            IVirusScanService        virusScanService,
            IPermissionService       permissionService,
            INetworkDriveHelper      networkDriveHelper,
            IAuditService            auditService,
            INotificationRepository  notificationRepository,
            IUserAccountRepository   userAccountRepository,
            ILoggingService          loggingService,
            IPermissionRepository    permissionRepository    = null,
            IFilePermissionRepository filePermissionRepository = null)
        {
            if (fileDocumentRepository == null) throw new ArgumentNullException("fileDocumentRepository");
            if (folderRepository       == null) throw new ArgumentNullException("folderRepository");
            if (orgUnitRepository      == null) throw new ArgumentNullException("orgUnitRepository");
            if (virusScanService       == null) throw new ArgumentNullException("virusScanService");
            if (permissionService      == null) throw new ArgumentNullException("permissionService");
            if (networkDriveHelper     == null) throw new ArgumentNullException("networkDriveHelper");
            if (auditService           == null) throw new ArgumentNullException("auditService");
            if (notificationRepository == null) throw new ArgumentNullException("notificationRepository");
            if (userAccountRepository  == null) throw new ArgumentNullException("userAccountRepository");
            if (loggingService         == null) throw new ArgumentNullException("loggingService");

            _fileDocumentRepository   = fileDocumentRepository;
            _folderRepository         = folderRepository;
            _orgUnitRepository        = orgUnitRepository;
            _virusScanService         = virusScanService;
            _permissionService        = permissionService;
            _networkDriveHelper       = networkDriveHelper;
            _auditService             = auditService;
            _notificationRepository   = notificationRepository;
            _userAccountRepository    = userAccountRepository;
            _loggingService           = loggingService;
            _permissionRepository     = permissionRepository;
            _filePermissionRepository = filePermissionRepository;
        }

        // -----------------------------------------------------------------------
        // UploadFiles
        // -----------------------------------------------------------------------

        public IEnumerable<UploadResult> UploadFiles(
            IEnumerable<FileUploadRequest> files, int folderId, UserContext user)
        {
            if (files == null) throw new ArgumentNullException("files");
            if (user  == null) throw new ArgumentNullException("user");

            var fileList = files.ToList();
            var results  = new List<UploadResult>();

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));
            if (folder.IsDeleted)
                throw new InvalidOperationException("Cannot upload to a deleted folder.");
            if (folder.IsArchived)
                throw new InvalidOperationException("Cannot upload to an archived folder.");

            bool isPrivileged = user.AppRole >= 3;
            if (!isPrivileged)
            {
                // เงื่อนไขที่ 1: FolderAuthorityConfig — CanAdd ตาม LevelCode/ตำแหน่ง/แผนก/ฝ่าย
                bool hasAuthority = user.CanAddToFolder(folder.ConfidentialityLevel);

                // เงื่อนไขที่ 2: FolderPermission — Write โดยใช้ UserAccount (User, Role, Dept, Div)
                bool hasAclWrite = !hasAuthority && _permissionService.CheckPermission(user.UserID, folderId, "Write");

                if (!hasAuthority && !hasAclWrite)
                    throw new UnauthorizedAccessException(string.Format(
                        "User {0} does not have permission to upload to folder {1}.", user.UserID, folderId));
            }

            foreach (var request in fileList)
                results.Add(UploadSingleFile(request, folder, user));

            return results;
        }

        // -----------------------------------------------------------------------
        // DownloadFile
        // -----------------------------------------------------------------------

        public FileDownloadResult DownloadFile(int fileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var doc = _fileDocumentRepository.GetById(fileId);
            if (doc == null)
                throw new InvalidOperationException(string.Format("FileDocument with ID {0} was not found.", fileId));
            if (doc.FileStatus == 3)
                throw new InvalidOperationException("The requested file has been deleted.");

            // เงื่อนไขที่ 1+2: สิทธิ์จาก FolderAuthorityConfig / FolderPermission
            bool hasFolderAccess = _permissionService.CheckPermission(user.UserID, doc.FolderID, "Read");

            // เงื่อนไขที่ 3: ไฟล์ถูก share ตรงให้ user (FilePermission) — bypass folder check
            bool hasDirectShare = false;
            if (!hasFolderAccess && _filePermissionRepository != null)
            {
                var filePerms = _filePermissionRepository.GetByFile(doc.FileID);
                hasDirectShare = filePerms.Any(p =>
                    p.PrincipalType == 1 &&          // User
                    p.PrincipalID   == user.UserID &&
                    p.CanRead       == true &&
                    p.IsDeny        == false &&
                    p.RevokedDate   == null &&
                    !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today));
            }

            if (!hasFolderAccess && !hasDirectShare)
                throw new UnauthorizedAccessException(string.Format(
                    "User {0} does not have Read permission on folder {1}.", user.UserID, doc.FolderID));

            Stream fileStream;
            try
            {
                fileStream = _networkDriveHelper.ReadFile(doc.PhysicalPath);
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "FileService: Failed to read file '{0}' from network drive.", doc.PhysicalPath), ex);
                return new FileDownloadResult
                {
                    FileName     = doc.OriginalFileName,
                    Success      = false,
                    ErrorMessage = "The file could not be read from storage."
                };
            }

            string recomputedChecksum = ComputeSha256(fileStream);
            fileStream.Position = 0;

            if (!string.Equals(recomputedChecksum, doc.Checksum, StringComparison.OrdinalIgnoreCase))
            {
                Log(user, "File", "DOWNLOAD", "storage.FileDocument", fileId, 2,
                    string.Format("Checksum mismatch for '{0}'. Stored: {1}, Recomputed: {2}.",
                        doc.OriginalFileName, doc.Checksum, recomputedChecksum));

                _notificationRepository.Insert(new Notification
                {
                    RecipientUserId = user.UserID,
                    EventType       = "FileIntegrityFailure",
                    RelatedEntityId = fileId.ToString(),
                    MessageText     = string.Format(
                        "File '{0}' failed integrity check. Please contact your administrator.",
                        doc.OriginalFileName),
                    DeliveredAt = DateTime.Now
                });

                fileStream.Dispose();
                return new FileDownloadResult
                {
                    FileName     = doc.OriginalFileName,
                    Success      = false,
                    ErrorMessage = string.Format("File '{0}' failed integrity check.", doc.OriginalFileName)
                };
            }

            Log(user, "File", "DOWNLOAD", "storage.FileDocument", fileId, 1,
                string.Format("Downloaded file '{0}'.", doc.OriginalFileName));

            return new FileDownloadResult
            {
                FileName      = doc.OriginalFileName,
                FileStream    = fileStream,
                FileSizeBytes = doc.FileSizeBytes,
                Success       = true
            };
        }

        // -----------------------------------------------------------------------
        // DeleteFile
        // -----------------------------------------------------------------------

        public void DeleteFile(int fileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var doc = _fileDocumentRepository.GetById(fileId);
            if (doc == null)
                throw new InvalidOperationException(string.Format("FileDocument with ID {0} was not found.", fileId));
            if (doc.FileStatus == 3)
                throw new InvalidOperationException("The file has already been deleted.");

            bool isPrivilegedDel = user.AppRole >= 3;
            if (!isPrivilegedDel)
            {
                var parentFolder = _folderRepository.GetById(doc.FolderID);

                // เงื่อนไขที่ 1: FolderAuthorityConfig — CanEdit ตาม LevelCode/ตำแหน่ง/แผนก/ฝ่าย
                bool hasAuthority = parentFolder != null && user.CanEditFolder(parentFolder.ConfidentialityLevel);

                // เงื่อนไขที่ 2: FolderPermission — Delete โดยใช้ UserAccount (User, Role, Dept, Div)
                bool hasAclDelete = !hasAuthority && _permissionService.CheckPermission(user.UserID, doc.FolderID, "Delete");

                if (!hasAuthority && !hasAclDelete)
                    throw new UnauthorizedAccessException(string.Format(
                        "User {0} does not have permission to delete file from folder {1}.", user.UserID, doc.FolderID));
            }

            // File-level permissions override folder permissions when present
            if (_filePermissionRepository != null)
            {
                var filePerms = _filePermissionRepository.GetByFile(fileId).ToList();
                if (filePerms.Count > 0)
                {
                    var account   = _userAccountRepository.GetById(user.UserID);
                    int? userRole = account != null ? (int?)account.AppRole : null;

                    var applicable = filePerms.Where(e =>
                        (e.PrincipalType == 1 && e.PrincipalID == user.UserID) ||
                        (e.PrincipalType == 2 && userRole.HasValue && e.PrincipalID == userRole.Value)
                    ).ToList();

                    if (applicable.Count > 0)
                    {
                        // Explicit DENY blocks everyone including Admin/Manager
                        bool fileDeny = applicable.Any(e => e.IsDeny && (e.CanDelete || e.CanFullControl));
                        if (fileDeny)
                            throw new UnauthorizedAccessException(string.Format(
                                "User {0} is denied Delete permission on file {1}.", user.UserID, fileId));

                        // File permission exists but Delete is not granted → block
                        bool fileAllowDelete = applicable.Any(e => !e.IsDeny && (e.CanDelete || e.CanFullControl));
                        if (!fileAllowDelete)
                            throw new UnauthorizedAccessException(string.Format(
                                "User {0} does not have Delete permission on file {1}.", user.UserID, fileId));
                    }
                }
            }

            _fileDocumentRepository.SoftDelete(fileId, user.UserID);

            Log(user, "File", "DELETE", "storage.FileDocument", fileId, 1,
                string.Format("Soft-deleted file '{0}'.", doc.OriginalFileName));
        }

        // -----------------------------------------------------------------------
        // GetFilesInFolder
        // -----------------------------------------------------------------------

        public IEnumerable<FileDocument> GetFilesInFolder(int folderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            bool isPrivileged = user.AppRole >= 3;
            if (!isPrivileged)
            {
                var folder = _folderRepository.GetById(folderId);

                // เงื่อนไขที่ 1: ตรวจ FolderAuthorityConfig ด้วย LevelCode/ตำแหน่ง/แผนก/ฝ่าย ของ user
                bool hasAuthority = folder != null && user.CanViewFolder(folder.ConfidentialityLevel);

                // เงื่อนไขที่ 2: ตรวจ FolderPermission โดยใช้ UserAccount (UserID, Role, DepartmentId, DivisionId)
                bool hasAclRead = !hasAuthority && _permissionService.CheckPermission(user.UserID, folderId, "Read");

                if (!hasAuthority && !hasAclRead)
                    throw new UnauthorizedAccessException(string.Format(
                        "User {0} does not have permission to view files in folder {1}.", user.UserID, folderId));
            }

            return _fileDocumentRepository.GetByFolder(folderId)
                .Where(f => f.FileStatus != 3); // exclude deleted
        }

        // -----------------------------------------------------------------------
        // GetStorageReport
        // -----------------------------------------------------------------------

        public StorageUsageReport GetStorageReport(int orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var orgUnit = _orgUnitRepository.GetById(orgUnitId);
            if (orgUnit == null)
                throw new InvalidOperationException(string.Format("OrgUnit with ID {0} was not found.", orgUnitId));

            var rootFolders = _folderRepository.GetRootFoldersByOrgUnit(orgUnitId).ToList();
            var allFolders  = new List<Folder>();
            foreach (var root in rootFolders)
            {
                allFolders.Add(root);
                allFolders.AddRange(_folderRepository.GetDescendants(root.FolderID));
            }

            var report = new StorageUsageReport
            {
                OrgUnitID   = orgUnitId,
                OrgUnitName = orgUnit.OrgUnitNameTH
            };

            foreach (var folder in allFolders.Where(f => !f.IsDeleted))
            {
                report.Folders.Add(new FolderStorageEntry
                {
                    FolderID          = folder.FolderID,
                    FolderName        = folder.FolderName,
                    CurrentUsageBytes = folder.FolderSizeBytes
                });
            }

            return report;
        }

        public IEnumerable<FileDocument> GetAllFilesGroupedByFolder(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("GetAllFilesGroupedByFolder requires Manager or Admin role.");
            return _fileDocumentRepository.GetAllGroupedByFolder();
        }

        public IEnumerable<FileDocument> GetFilesByOrgUnit(int orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("GetFilesByOrgUnit requires Manager or Admin role.");
            return _fileDocumentRepository.GetByOrgUnit(orgUnitId);
        }

        public void RenameFile(int fileId, string newName, UserContext user)
        {
            if (user    == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("ชื่อไฟล์ต้องไม่ว่าง");

            var file = _fileDocumentRepository.GetById(fileId);
            if (file == null)
                throw new InvalidOperationException(string.Format("ไม่พบไฟล์ ID {0}", fileId));
            if (file.IsDeleted)
                throw new InvalidOperationException("ไม่สามารถเปลี่ยนชื่อไฟล์ที่ถูกลบแล้ว");

            bool isPrivileged = user.AppRole >= 3;
            if (!isPrivileged)
            {
                var parentFolder = _folderRepository.GetById(file.FolderID);

                // เงื่อนไขที่ 1: FolderAuthorityConfig — CanEdit ตาม LevelCode/ตำแหน่ง/แผนก/ฝ่าย
                bool hasAuthority = parentFolder != null && user.CanEditFolder(parentFolder.ConfidentialityLevel);

                // เงื่อนไขที่ 2: FolderPermission — Write โดยใช้ UserAccount (User, Role, Dept, Div)
                bool hasAclWrite = !hasAuthority && _permissionService.CheckPermission(user.UserID, file.FolderID, "Write");

                if (!hasAuthority && !hasAclWrite)
                    throw new UnauthorizedAccessException(
                        string.Format("ไม่มีสิทธิ์เปลี่ยนชื่อไฟล์ '{0}' (ต้องการสิทธิ์ Edit)", file.OriginalFileName));
            }

            string oldName = file.OriginalFileName;
            _fileDocumentRepository.RenameFile(fileId, newName.Trim());

            Log(user, "File", "RENAME", "storage.FileDocument", fileId, 1,
                string.Format("เปลี่ยนชื่อจาก '{0}' เป็น '{1}'", oldName, newName.Trim()));
        }

        // -----------------------------------------------------------------------
        // MoveFiles
        // -----------------------------------------------------------------------

        public void MoveFiles(MoveFileRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");

            if (request.FileIds == null || !request.FileIds.Any())
                throw new ArgumentException("ต้องระบุไฟล์อย่างน้อย 1 รายการ");

            var destination = _folderRepository.GetById(request.DestinationFolderId);
            if (destination == null)
                throw new InvalidOperationException(
                    string.Format("ไม่พบ Folder ปลายทาง ID {0}", request.DestinationFolderId));

            if (user.AppRole < 4)
            {
                if (!_permissionService.CheckPermission(user.UserID, request.DestinationFolderId, "Write"))
                    throw new UnauthorizedAccessException(
                        string.Format("ไม่มีสิทธิ์เขียนใน Folder ปลายทาง '{0}'", destination.FolderName));
            }

            foreach (int fileId in request.FileIds)
            {
                var file = _fileDocumentRepository.GetById(fileId);
                if (file == null)
                    throw new InvalidOperationException(
                        string.Format("ไม่พบไฟล์ ID {0}", fileId));

                if (file.FileStatus != 1)
                    throw new InvalidOperationException(
                        string.Format("ไม่สามารถย้ายไฟล์ ID {0} ได้ เนื่องจากสถานะไม่ใช่ Active", fileId));

                if (user.AppRole < 4)
                {
                    if (!_permissionService.CheckPermission(user.UserID, file.FolderID, "Write"))
                        throw new UnauthorizedAccessException(
                            string.Format("ไม่มีสิทธิ์เขียนใน Folder ต้นทาง (ID: {0})", file.FolderID));
                }

                int    oldFolderId   = file.FolderID;
                string oldPhysicalPath = file.PhysicalPath;   // เก็บ path เดิมไว้ก่อน

                string newPhysicalPath = _networkDriveHelper.BuildPath(
                    destination.OrgUnitID, request.DestinationFolderId, file.StoredFileName);

                // ── ย้ายไฟล์จริงบน disk ก่อน update DB ──────────────────────
                bool physicalMoved = false;
                if (!string.IsNullOrEmpty(oldPhysicalPath) && _networkDriveHelper.FileExists(oldPhysicalPath))
                {
                    _networkDriveHelper.MoveFile(oldPhysicalPath, newPhysicalPath);
                    physicalMoved = true;
                }
                // ถ้าไฟล์ต้นทางไม่มีบน disk (อาจถูก archive/เสียหาย) ให้ยังคง update DB ต่อไป
                // แต่บันทึก path ใหม่ไว้ถูกต้องเผื่อ upload ซ้ำในอนาคต

                // ── Update DB ─────────────────────────────────────────────────
                file.FolderID     = request.DestinationFolderId;
                file.PhysicalPath = newPhysicalPath;

                try
                {
                    _fileDocumentRepository.Update(file);
                }
                catch
                {
                    // DB update ล้มเหลว — ย้ายไฟล์กลับคืน (rollback physical)
                    if (physicalMoved)
                    {
                        try { _networkDriveHelper.MoveFile(newPhysicalPath, oldPhysicalPath); }
                        catch { /* best-effort rollback */ }
                    }
                    throw;
                }

                RefreshFolderFileCounts(oldFolderId, request.DestinationFolderId);

                _auditService.Log(new AuditEntry
                {
                    Module      = "File",
                    Action      = "MoveFile",
                    UserID      = user.UserID,
                    UserName    = user.ADUserName ?? string.Empty,
                    UserRole    = user.AppRole,
                    TableName   = "storage.FileDocument",
                    RecordID    = fileId,
                    OldValue    = string.Format("FolderID={0} Path={1}", oldFolderId, oldPhysicalPath),
                    NewValue    = string.Format("FolderID={0} Path={1}", request.DestinationFolderId, newPhysicalPath),
                    MachineName = user.MachineName,
                    IPAddress   = user.IPAddress,
                    SessionID   = user.SessionID,
                    Result      = 1,
                    AuditDate   = DateTime.Now
                });
            }
        }

        private void RefreshFolderFileCounts(params int[] folderIds)
        {
            foreach (int folderId in folderIds.Where(id => id > 0).Distinct())
                _folderRepository.UpdateFileCount(folderId);
        }


        // -----------------------------------------------------------------------
        // CopyFile
        // -----------------------------------------------------------------------

        public int CopyFile(int fileId, int destinationFolderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var source = _fileDocumentRepository.GetById(fileId);
            if (source == null)
                throw new InvalidOperationException(string.Format("ไม่พบไฟล์ ID {0}", fileId));
            if (source.IsDeleted)
                throw new InvalidOperationException("ไม่สามารถคัดลอกไฟล์ที่ถูกลบแล้ว");

            var destFolder = _folderRepository.GetById(destinationFolderId);
            if (destFolder == null)
                throw new InvalidOperationException(string.Format("ไม่พบ Folder ปลายทาง ID {0}", destinationFolderId));

            // Permission check on destination
            if (user.AppRole < 4)
            {
                if (!_permissionService.CheckPermission(user.UserID, destinationFolderId, "Write"))
                    throw new UnauthorizedAccessException(
                        string.Format("ไม่มีสิทธิ์เขียนใน Folder ปลายทาง '{0}'", destFolder.FolderName));
            }

            // Build a unique stored filename in destination (append copy index if collision)
            string baseName  = Path.GetFileNameWithoutExtension(source.StoredFileName ?? source.OriginalFileName);
            string ext       = source.FileExtension ?? Path.GetExtension(source.OriginalFileName ?? "");
            if (!ext.StartsWith(".")) ext = "." + ext;

            string storedName = baseName + "_copy" + ext;
            string destPath   = _networkDriveHelper.BuildPath(destFolder.OrgUnitID, destinationFolderId, storedName);

            int suffix = 1;
            while (_networkDriveHelper.FileExists(destPath))
            {
                storedName = string.Format("{0}_copy({1}){2}", baseName, suffix++, ext);
                destPath   = _networkDriveHelper.BuildPath(destFolder.OrgUnitID, destinationFolderId, storedName);
            }

            // Copy physical file
            if (!string.IsNullOrWhiteSpace(source.PhysicalPath) && _networkDriveHelper.FileExists(source.PhysicalPath))
            {
                string destDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrWhiteSpace(destDir) && !Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                File.Copy(source.PhysicalPath, destPath, overwrite: false);
            }

            // Create new DB record
            var newDoc = new FileDocument
            {
                FolderID         = destinationFolderId,
                OriginalFileName = source.OriginalFileName,
                StoredFileName   = storedName,
                FileExtension    = source.FileExtension,
                MimeType         = source.MimeType,
                FileSizeBytes    = source.FileSizeBytes,
                PhysicalPath     = destPath,
                Checksum         = source.Checksum,
                VirusScanStatus  = source.VirusScanStatus,
                VirusScanDate    = source.VirusScanDate,
                FileStatus       = 1,
                UploadedBy       = user.UserID,
                UploadedDate     = DateTime.Now,
                Version          = 1
            };

            int newId = _fileDocumentRepository.Insert(newDoc);
            RefreshFolderFileCounts(destinationFolderId);

            _auditService.Log(new AuditEntry
            {
                Module      = "File",
                Action      = "CopyFile",
                UserID      = user.UserID,
                UserName    = user.ADUserName ?? string.Empty,
                UserRole    = user.AppRole,
                TableName   = "storage.FileDocument",
                RecordID    = newId,
                OldValue    = string.Format("SourceFileID={0} FolderID={1}", fileId, source.FolderID),
                NewValue    = string.Format("NewFileID={0} FolderID={1} Path={2}", newId, destinationFolderId, destPath),
                MachineName = user.MachineName,
                IPAddress   = user.IPAddress,
                SessionID   = user.SessionID,
                Result      = 1,
                AuditDate   = DateTime.Now
            });

            return newId;
        }

        // -----------------------------------------------------------------------
        // Private: single-file upload pipeline
        // -----------------------------------------------------------------------

        private UploadResult UploadSingleFile(FileUploadRequest request, Folder folder, UserContext user)
        {
            string writtenPath = null;
            int?   insertedId  = null;

            try
            {
                string ext = Path.GetExtension(request.FileName);
                if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                    return Fail(request.FileName, string.Format(
                        "File extension '{0}' is not permitted.", ext));

                if (request.FileSizeBytes > MaxFileSizeBytes)
                    return Fail(request.FileName, string.Format(
                        "File size {0:N0} bytes exceeds the 100 MB limit.", request.FileSizeBytes));

                if (request.FileStream == null)
                    return Fail(request.FileName, "File stream is null.");

                // Resolve duplicate file name — append " copy", " copy(2)", " copy(3)", …
                string resolvedName = ResolveFileName(request.FileName, folder.FolderID);
                bool   isDuplicate  = !string.Equals(resolvedName, request.FileName, StringComparison.OrdinalIgnoreCase);

                // Check quota before proceeding
                if (folder.QuotaBytes.HasValue)
                {
                    long projectedUsage = folder.FolderSizeBytes + request.FileSizeBytes;
                    if (projectedUsage > folder.QuotaBytes.Value)
                    {
                        long remainingQuota = folder.QuotaBytes.Value - folder.FolderSizeBytes;
                        return Fail(request.FileName, string.Format(
                            "Upload would exceed folder quota. Current usage: {0:N0} bytes, Quota: {1:N0} bytes, Remaining: {2:N0} bytes.",
                            folder.FolderSizeBytes, folder.QuotaBytes.Value, remainingQuota));
                    }
                }

                request.FileStream.Position = 0;
                string checksum = ComputeSha256(request.FileStream);
                request.FileStream.Position = 0;

                // Virus scan — skipped (not in use); all files treated as clean
                ScanResult scanResult = new ScanResult { IsClean = true, ScannerError = false };

                // Write to network drive
                writtenPath = _networkDriveHelper.BuildPath(folder.OrgUnitID, folder.FolderID, resolvedName);
                _networkDriveHelper.WriteFile(writtenPath, request.FileStream);

                string storedFileName = Path.GetFileName(writtenPath);
                string mimeType = string.IsNullOrEmpty(request.MimeType) ? "application/octet-stream" : request.MimeType;

                var doc = new FileDocument
                {
                    FolderID         = folder.FolderID,
                    OriginalFileName = resolvedName,
                    StoredFileName   = storedFileName,
                    FileExtension    = ext,
                    MimeType         = mimeType,
                    FileSizeBytes    = request.FileSizeBytes,
                    PhysicalPath     = writtenPath,
                    Checksum         = checksum,
                    VirusScanStatus  = scanResult.ScannerError ? 0   // 0=Pending (scanner unavailable)
                                     : scanResult.IsClean    ? 1   // 1=Clean
                                                             : 2,  // 2=Infected
                    VirusScanDate    = DateTime.Now,
                    FileStatus       = 1, // 1=Active
                    UploadedBy       = user.UserID,
                    UploadedDate     = DateTime.Now,
                    Version          = 1
                };

                insertedId = _fileDocumentRepository.Insert(doc);

                // Auto-propagate folder permissions to the new file
                if (_permissionRepository != null && _filePermissionRepository != null)
                {
                    try
                    {
                        var folderPerms = _permissionRepository.GetByFolder(folder.FolderID)
                            .Where(p => p.RevokedDate == null
                                     && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today))
                            .ToList();

                        // Revoke any existing file permissions first to avoid duplicates
                        var existingFilePerms = _filePermissionRepository.GetByFile(insertedId.Value).ToList();
                        foreach (var ep in existingFilePerms)
                            _filePermissionRepository.Revoke(ep.FilePermissionID, user.UserID, "Replaced by folder propagation on upload");

                        foreach (var fp in folderPerms)
                        {
                            _filePermissionRepository.Insert(new FilePermission
                            {
                                FileID         = insertedId.Value,
                                PrincipalType  = (int)fp.PrincipalType,
                                PrincipalID    = fp.PrincipalID,
                                CanRead        = fp.CanRead,
                                CanWrite       = fp.CanWrite,
                                CanDelete      = fp.CanDelete,
                                CanModify      = fp.CanModify,
                                CanFullControl = fp.CanFullControl,
                                IsDeny         = fp.IsDeny,
                                ExpiryDate     = fp.ExpiryDate,
                                GrantedBy      = user.UserID,
                                GrantedDate    = DateTime.Now
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Permission propagation failure must not block upload
                        _loggingService.Error(string.Format(
                            "FileService: Failed to propagate folder permissions to file {0}: {1}",
                            insertedId.Value, ex.Message), ex);
                    }
                }

                // Update folder size and file count
                folder.FolderSizeBytes += request.FileSizeBytes;
                folder.FileCount       += 1;
                _folderRepository.Update(folder);
                
                // Try to ensure FileCount is accurate by calling stored procedure (if available)
                try
                {
                    _folderRepository.UpdateFileCount(folder.FolderID);
                }
                catch (Exception updateEx)
                {
                    // If stored procedure doesn't exist, just log and continue
                    _loggingService.Error(string.Format(
                        "FileService: Could not update FileCount via stored procedure for folder {0}: {1}",
                        folder.FolderID, updateEx.Message), updateEx);
                }

                Log(user, "File", "UPLOAD", "storage.FileDocument", insertedId.Value, 1,
                    string.Format("Uploaded '{0}' ({1:N0} bytes) to folder {2}.",
                        resolvedName, request.FileSizeBytes, folder.FolderID));

                return new UploadResult
                {
                    OriginalFileName = request.FileName,
                    ActualFileName   = resolvedName,
                    IsDuplicate      = isDuplicate,
                    Success          = true,
                    FileDocumentId   = insertedId
                };
            }
            catch (Exception ex)
            {
                if (writtenPath != null)
                {
                    try
                    {
                        if (_networkDriveHelper.FileExists(writtenPath))
                            _networkDriveHelper.DeleteFile(writtenPath);
                    }
                    catch (Exception rollbackEx)
                    {
                        _loggingService.Error(string.Format(
                            "FileService: Rollback failed — could not delete '{0}'.", writtenPath), rollbackEx);
                    }
                }

                if (insertedId.HasValue)
                {
                    try { _fileDocumentRepository.SoftDelete(insertedId.Value, user.UserID); }
                    catch (Exception rollbackEx)
                    {
                        _loggingService.Error(string.Format(
                            "FileService: Rollback failed — could not soft-delete FileDocument {0}.", insertedId.Value),
                            rollbackEx);
                    }
                }

                _loggingService.Error(string.Format("FileService: Upload failed for '{0}'.", request.FileName), ex);
                Log(user, "File", "UPLOAD", "storage.FileDocument", 0, 2, ex.Message);

                return Fail(request.FileName, "An unexpected error occurred during upload. The operation has been rolled back.");
            }
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static string ComputeSha256(Stream stream)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                var sb = new StringBuilder(64);
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private void Log(UserContext user, string module, string action, string tableName,
                         int recordId, int result, string newValue = null)
        {
            _auditService.Log(new AuditEntry
            {
                Module      = module,
                Action      = action,
                UserID      = user.UserID,
                UserName    = user.ADUserName ?? string.Empty,
                UserRole    = user.AppRole,
                TableName   = tableName,
                RecordID    = recordId > 0 ? (int?)recordId : null,
                NewValue    = newValue,
                MachineName = user.MachineName,
                IPAddress   = user.IPAddress,
                SessionID   = user.SessionID,
                Result      = result,
                AuditDate   = DateTime.Now
            });
        }

        private static UploadResult Fail(string fileName, string message)
        {
            return new UploadResult { OriginalFileName = fileName, Success = false, ErrorMessage = message };
        }

        public IEnumerable<string> FindDuplicateNames(IEnumerable<string> fileNames, int folderId, UserContext user)
        {
            if (fileNames == null) throw new ArgumentNullException("fileNames");
            if (user      == null) throw new ArgumentNullException("user");

            return fileNames
                .Where(name => !string.IsNullOrEmpty(name)
                            && _fileDocumentRepository.ExistsWithName(name, folderId))
                .ToList();
        }

        /// <summary>
        /// Returns a unique file name within the folder.
        /// If <paramref name="fileName"/> is already taken, counts existing copies and names the new file
        /// "{name} copy({count+1}).ext", guaranteeing no collision in a single extra check.
        /// </summary>
        private string ResolveFileName(string fileName, int folderId)
        {
            if (!_fileDocumentRepository.ExistsWithName(fileName, folderId))
                return fileName;

            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string ext            = Path.GetExtension(fileName);

            // Count all existing copies: original + any "name copy*" files
            string copyPrefix = nameWithoutExt + " copy";
            int existingCopies = _fileDocumentRepository.CountByNamePrefix(copyPrefix, folderId);

            // Start at count+1 and increment until a free slot is found (handles gaps/race conditions)
            int counter = existingCopies + 1;
            string candidate;
            do
            {
                candidate = string.Format("{0} copy({1}){2}", nameWithoutExt, counter, ext);
                counter++;
            }
            while (_fileDocumentRepository.ExistsWithName(candidate, folderId));

            return candidate;
        }
    }
}
