using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class FileService : IFileService
    {
        private const long MaxFileSizeBytes = 100L * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx",
            ".ppt", ".pptx", ".jpg", ".png", ".gif", ".zip"
        };

        private readonly IFileDocumentRepository _fileDocumentRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IOrgUnitRepository _orgUnitRepository;
        private readonly IVirusScanService _virusScanService;
        private readonly IPermissionService _permissionService;
        private readonly INetworkDriveHelper _networkDriveHelper;
        private readonly IAuditService _auditService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ILoggingService _loggingService;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IFilePermissionRepository _filePermissionRepository;
        private readonly IFolderQuotaService _folderQuotaService;

        public FileService(
            IFileDocumentRepository fileDocumentRepository,
            IFolderRepository folderRepository,
            IOrgUnitRepository orgUnitRepository,
            IVirusScanService virusScanService,
            IPermissionService permissionService,
            INetworkDriveHelper networkDriveHelper,
            IAuditService auditService,
            INotificationRepository notificationRepository,
            IUserAccountRepository userAccountRepository,
            ILoggingService loggingService,
            IPermissionRepository permissionRepository = null,
            IFilePermissionRepository filePermissionRepository = null,
            IFolderQuotaService folderQuotaService = null)
        {
            if (fileDocumentRepository == null) throw new ArgumentNullException("fileDocumentRepository");
            if (folderRepository == null) throw new ArgumentNullException("folderRepository");
            if (orgUnitRepository == null) throw new ArgumentNullException("orgUnitRepository");
            if (virusScanService == null) throw new ArgumentNullException("virusScanService");
            if (permissionService == null) throw new ArgumentNullException("permissionService");
            if (networkDriveHelper == null) throw new ArgumentNullException("networkDriveHelper");
            if (auditService == null) throw new ArgumentNullException("auditService");
            if (notificationRepository == null) throw new ArgumentNullException("notificationRepository");
            if (userAccountRepository == null) throw new ArgumentNullException("userAccountRepository");
            if (loggingService == null) throw new ArgumentNullException("loggingService");

            _fileDocumentRepository = fileDocumentRepository;
            _folderRepository = folderRepository;
            _orgUnitRepository = orgUnitRepository;
            _virusScanService = virusScanService;
            _permissionService = permissionService;
            _networkDriveHelper = networkDriveHelper;
            _auditService = auditService;
            _notificationRepository = notificationRepository;
            _userAccountRepository = userAccountRepository;
            _loggingService = loggingService;
            _permissionRepository = permissionRepository;
            _filePermissionRepository = filePermissionRepository;
            _folderQuotaService = folderQuotaService;
        }

        // -----------------------------------------------------------------------
        // UploadFiles
        // -----------------------------------------------------------------------

        public IEnumerable<UploadResult> UploadFiles(
            IEnumerable<FileUploadRequest> files, int folderId, UserContext user)
        {
            if (files == null) throw new ArgumentNullException("files");
            if (user == null) throw new ArgumentNullException("user");

            var fileList = files.ToList();
            var results = new List<UploadResult>();

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
                        "คุณไม่มีสิทธิ์อัปโหลดไฟล์ลงในโฟลเดอร์ \"{0}\"", folder.FolderName));
            }

            var quotaBlockMessage = ValidateBatchAgainstOrgQuota(fileList, folder);
            if (!string.IsNullOrEmpty(quotaBlockMessage))
                return fileList.Select(f => Fail(f.FileName, quotaBlockMessage)).ToList();

            foreach (var request in fileList)
                results.Add(UploadSingleFile(request, folder, user));

            return results;
        }

        public string CheckUploadFolder(int folderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                return "ไม่พบโฟลเดอร์ปลายทางที่เลือกในระบบ DMS";
            if (folder.IsDeleted)
                return "โฟลเดอร์ปลายทางที่เลือกถูกลบแล้ว";
            if (folder.IsArchived)
                return "โฟลเดอร์ปลายทางที่เลือกถูกเก็บถาวร (Archive) ไม่สามารถอัปโหลดไฟล์ได้";

            if (user.AppRole >= 3)
                return null;

            if (user.CanAddToFolder(folder.ConfidentialityLevel)
                || _permissionService.CheckPermission(user.UserID, folderId, "Write"))
                return null;

            return string.Format("คุณไม่มีสิทธิ์อัปโหลดไฟล์ลงโฟลเดอร์ \"{0}\"", folder.FolderName);
        }

        private string ValidateBatchAgainstOrgQuota(IEnumerable<FileUploadRequest> files, Folder folder)
        {
            if (_folderQuotaService == null || files == null || folder == null)
                return null;

            var usage = _folderQuotaService.GetUsage(folder.OrgUnitID);
            if (usage == null || usage.QuotaBytes <= 0)
                return null;

            long deltaBytes = 0;
            foreach (var request in files)
            {
                if (request == null)
                    continue;

                long existingSize = 0;
                if (request.OverwriteExisting && !string.IsNullOrEmpty(request.FileName))
                {
                    var existing = _fileDocumentRepository.GetByOriginalName(request.FileName, folder.FolderID);
                    existingSize = existing != null ? existing.FileSizeBytes : 0;
                }

                deltaBytes += Math.Max(0, request.FileSizeBytes - existingSize);
            }

            long remaining = usage.QuotaBytes - usage.UsedBytes;
            if (deltaBytes <= remaining)
                return null;

            return string.Format(
                "ไม่สามารถอัปโหลดได้ เนื่องจากพื้นที่คงเหลือ {0} แต่ไฟล์ที่เลือกมีขนาดรวม {1} (ใช้ไป {2} จาก quota {3})",
                FormatBytes(Math.Max(0, remaining)),
                FormatBytes(deltaBytes),
                FormatBytes(usage.UsedBytes),
                FormatBytes(usage.QuotaBytes));
        }

        private string ValidateIncomingBytesAgainstOrgQuota(Folder folder, long incomingBytes, string actionName)
        {
            if (_folderQuotaService == null || folder == null || incomingBytes <= 0)
                return null;

            var usage = _folderQuotaService.GetUsage(folder.OrgUnitID);
            if (usage == null || usage.QuotaBytes <= 0)
                return null;

            long remaining = usage.QuotaBytes - usage.UsedBytes;
            if (incomingBytes <= remaining)
                return null;

            return BuildQuotaExceededMessage(actionName, usage, incomingBytes);
        }

        private static string BuildQuotaExceededMessage(string actionName, QuotaUsageResult usage, long incomingBytes)
        {
            return string.Format(
                "ไม่สามารถ{0}ไฟล์ได้ในระบบ เนื่องจากพื้นที่คงเหลือ {1} แต่ไฟล์ที่จะ{0}มีขนาดรวม {2} (ใช้ไป {3} จาก quota {4})",
                actionName,
                FormatBytes(Math.Max(0, usage.QuotaBytes - usage.UsedBytes)),
                FormatBytes(Math.Max(0, incomingBytes)),
                FormatBytes(usage.UsedBytes),
                FormatBytes(usage.QuotaBytes));
        }

        // -----------------------------------------------------------------------
        // DownloadFile
        // -----------------------------------------------------------------------

        public FileDownloadResult DownloadFile(int fileId, UserContext user, bool bypassPermissionCheck = false)
        {
            if (user == null) throw new ArgumentNullException("user");

            var doc = _fileDocumentRepository.GetById(fileId);
            if (doc == null)
                throw new InvalidOperationException(string.Format("FileDocument with ID {0} was not found.", fileId));
            if (doc.FileStatus == 3)
                throw new InvalidOperationException("The requested file has been deleted.");

            if (!bypassPermissionCheck)
            {
                // เงื่อนไขที่ 1: FolderAuthorityConfig (ดู) ตาม Level/ตำแหน่ง/แผนก/ฝ่าย — กฎเดียวกับ GetFilesInFolder
                // ใครเห็นไฟล์ในโฟลเดอร์ได้ ก็อ่านไฟล์ได้ (การขออนุมัติดาวน์โหลดตรวจแยกใน DownloadPolicyService)
                var docFolder = _folderRepository.GetById(doc.FolderID);
                bool hasAuthority = docFolder != null && user.CanViewFolder(docFolder.ConfidentialityLevel);

                // เงื่อนไขที่ 2: FolderPermission (ACL) — Manager/Admin ผ่านเว้นแต่มี Deny
                bool hasFolderAccess = hasAuthority
                    || _permissionService.CheckPermission(user.UserID, doc.FolderID, "Read");

                // เงื่อนไขที่ 3: ไฟล์ถูก share ตรงให้ user (FilePermission) — bypass folder check
                bool hasDirectShare = false;
                if (!hasFolderAccess && _filePermissionRepository != null)
                {
                    var filePerms = _filePermissionRepository.GetByFile(doc.FileID);
                    hasDirectShare = filePerms.Any(p =>
                        p.PrincipalType == 1 &&          // User
                        p.PrincipalID == user.UserID &&
                        p.CanRead == true &&
                        p.IsDeny == false &&
                        p.RevokedDate == null &&
                        !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today));
                }

                if (!hasFolderAccess && !hasDirectShare)
                    throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์เข้าถึงหรือดาวน์โหลดไฟล์นี้");
            }

            if (string.IsNullOrEmpty(doc.PhysicalPath))
            {
                _loggingService.Error(string.Format(
                    "FileService: PhysicalPath is null/empty for file ID {0} ('{1}').", fileId, doc.OriginalFileName), null);
                return new FileDownloadResult
                {
                    FileName = doc.OriginalFileName,
                    Success = false,
                    ErrorMessage = "ไม่พบที่จัดเก็บไฟล์ในระบบ กรุณาติดต่อผู้ดูแลระบบ"
                };
            }

            if (!_networkDriveHelper.FileExists(doc.PhysicalPath))
            {
                _loggingService.Error(string.Format(
                    "FileService: File not found at path '{0}' for file ID {1}.", doc.PhysicalPath, fileId), null);
                return new FileDownloadResult
                {
                    FileName = doc.OriginalFileName,
                    Success = false,
                    ErrorMessage = string.Format("ไม่พบไฟล์ในพื้นที่จัดเก็บ ({0})", System.IO.Path.GetFileName(doc.PhysicalPath))
                };
            }

            Stream fileStream;
            try
            {
                fileStream = _networkDriveHelper.ReadFile(doc.PhysicalPath);
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "FileService: Failed to read file '{0}' from network drive. Error: {1}", doc.PhysicalPath, ex.Message), ex);
                return new FileDownloadResult
                {
                    FileName = doc.OriginalFileName,
                    Success = false,
                    ErrorMessage = string.Format("เปิดไฟล์ไม่ได้: {0}", ex.Message)
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
                    EventType = "FileIntegrityFailure",
                    RelatedEntityId = fileId.ToString(),
                    MessageText = string.Format(
                        "File '{0}' failed integrity check. Please contact your administrator.",
                        doc.OriginalFileName),
                    DeliveredAt = DateTime.Now
                });

                fileStream.Dispose();
                return new FileDownloadResult
                {
                    FileName = doc.OriginalFileName,
                    Success = false,
                    ErrorMessage = string.Format("File '{0}' failed integrity check.", doc.OriginalFileName)
                };
            }

            Log(user, "File", "DOWNLOAD", "storage.FileDocument", fileId, 1,
                string.Format("Downloaded file '{0}'.", doc.OriginalFileName));

            return new FileDownloadResult
            {
                FileName = doc.OriginalFileName,
                FileStream = fileStream,
                FileSizeBytes = doc.FileSizeBytes,
                Success = true
            };
        }

        // -----------------------------------------------------------------------
        // Open mode (View / Edit) and save-back
        // -----------------------------------------------------------------------

        public FileOpenMode GetFileOpenMode(int fileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var doc = _fileDocumentRepository.GetById(fileId);
            if (doc == null || doc.IsDeleted)
                return FileOpenMode.None;

            var folder = _folderRepository.GetById(doc.FolderID);
            var userFilePerms = GetActiveUserFilePermissions(doc.FileID, user.UserID);

            bool canRead =
                (folder != null && user.CanViewFolder(folder.ConfidentialityLevel))
                || _permissionService.CheckPermission(user.UserID, doc.FolderID, "Read")
                || userFilePerms.Any(p => !p.IsDeny && (p.CanRead || p.CanWrite || p.CanModify || p.CanFullControl));
            if (!canRead)
                return FileOpenMode.None;

            // Archived files and files in archived/deleted folders are never editable.
            if (doc.FileStatus == 2 || folder == null || folder.IsDeleted || folder.IsArchived)
                return FileOpenMode.View;

            // An explicit file-level DENY on write wins over any folder-level grant.
            if (userFilePerms.Any(p => p.IsDeny && (p.CanWrite || p.CanModify || p.CanFullControl)))
                return FileOpenMode.View;

            // Edit must be granted explicitly. GetEffectivePermission evaluates the real ACL entries
            // (deny-first, inherited) WITHOUT the Manager/Admin bypass that CheckPermission applies,
            // so an Admin whose FolderAuthorityConfig has no "แก้ไข" for this level gets View only.
            var eff = _permissionService.GetEffectivePermission(user.UserID, doc.FolderID);
            bool canEdit =
                user.CanEditFolder(folder.ConfidentialityLevel)
                || (eff != null && (eff.CanWrite || eff.CanModify || eff.HasFullControl))
                || userFilePerms.Any(p => !p.IsDeny && (p.CanWrite || p.CanModify || p.CanFullControl));

            return canEdit ? FileOpenMode.Edit : FileOpenMode.View;
        }

        public UploadResult SaveFileNewVersion(int fileId, Stream content, UserContext user)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (user == null) throw new ArgumentNullException("user");

            var doc = _fileDocumentRepository.GetById(fileId);
            if (doc == null)
                throw new InvalidOperationException(string.Format("FileDocument with ID {0} was not found.", fileId));

            if (GetFileOpenMode(fileId, user) != FileOpenMode.Edit)
            {
                Log(user, "File", "SAVE_NEW_VERSION", "storage.FileDocument", fileId, 2,
                    string.Format("Denied save-back of '{0}': user has no edit permission.", doc.OriginalFileName));
                throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์แก้ไขไฟล์นี้");
            }

            // Someone may have replaced, renamed or deleted the file while it was open.
            var current = _fileDocumentRepository.GetByOriginalName(doc.OriginalFileName, doc.FolderID);
            if (doc.IsDeleted || current == null || current.FileID != doc.FileID)
                return Fail(doc.OriginalFileName,
                    "ไฟล์นี้ถูกแก้ไข ย้าย หรือลบไปแล้วหลังจากที่คุณเปิด กรุณาเปิดไฟล์ล่าสุดแล้วแก้ไขใหม่");

            var folder = _folderRepository.GetById(doc.FolderID);

            using (var buffer = new MemoryStream())
            {
                content.CopyTo(buffer);
                buffer.Position = 0;

                var request = new FileUploadRequest
                {
                    FileName = doc.OriginalFileName,
                    FileStream = buffer,
                    FileSizeBytes = buffer.Length,
                    MimeType = doc.MimeType,
                    OverwriteExisting = true
                };

                var quotaBlockMessage = ValidateBatchAgainstOrgQuota(new[] { request }, folder);
                if (!string.IsNullOrEmpty(quotaBlockMessage))
                    return Fail(doc.OriginalFileName, quotaBlockMessage);

                // Snapshot before the replace: the new version gets only folder-propagated ACLs,
                // so direct file shares must be carried over explicitly.
                var oldFilePerms = _filePermissionRepository != null
                    ? GetActiveFilePermissions(doc.FileID)
                    : new List<FilePermission>();

                // Folder-level write is not required here: GetFileOpenMode already granted Edit,
                // which may come from a file-level share only.
                var result = UploadSingleFile(request, folder, user);

                if (result.Success && result.FileDocumentId.HasValue)
                {
                    CarryOverFilePermissions(oldFilePerms, result.FileDocumentId.Value, user);

                    Log(user, "File", "SAVE_NEW_VERSION", "storage.FileDocument", result.FileDocumentId.Value, 1,
                        string.Format("Saved edited '{0}' as version {1} (replaces FileID {2}, {3:N0} bytes).",
                            doc.OriginalFileName, doc.Version + 1, doc.FileID, buffer.Length));
                }

                return result;
            }
        }

        private List<FilePermission> GetActiveFilePermissions(int fileId)
        {
            if (_filePermissionRepository == null)
                return new List<FilePermission>();

            return _filePermissionRepository.GetByFile(fileId)
                .Where(p => p.RevokedDate == null
                         && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today))
                .ToList();
        }

        private List<FilePermission> GetActiveUserFilePermissions(int fileId, int userId)
        {
            return GetActiveFilePermissions(fileId)
                .Where(p => p.PrincipalType == 1 && p.PrincipalID == userId)   // 1=User
                .ToList();
        }

        private void CarryOverFilePermissions(List<FilePermission> oldPerms, int newFileId, UserContext user)
        {
            if (_filePermissionRepository == null || oldPerms == null || oldPerms.Count == 0)
                return;

            try
            {
                var existing = new HashSet<string>(
                    _filePermissionRepository.GetByFile(newFileId)
                        .Where(p => p.RevokedDate == null)
                        .Select(p => p.PrincipalType + ":" + p.PrincipalID));

                foreach (var p in oldPerms)
                {
                    if (!existing.Add(p.PrincipalType + ":" + p.PrincipalID))
                        continue;

                    _filePermissionRepository.Insert(new FilePermission
                    {
                        FileID = newFileId,
                        PrincipalType = p.PrincipalType,
                        PrincipalID = p.PrincipalID,
                        CanRead = p.CanRead,
                        CanWrite = p.CanWrite,
                        CanDelete = p.CanDelete,
                        CanModify = p.CanModify,
                        CanFullControl = p.CanFullControl,
                        CanDownload = p.CanDownload,
                        IsDeny = p.IsDeny,
                        ExpiryDate = p.ExpiryDate,
                        GrantedBy = p.GrantedBy,
                        GrantedDate = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                // The new version is already committed; a failed carry-over must not undo it.
                _loggingService.Error(string.Format(
                    "FileService: Failed to carry file permissions over to new version {0}: {1}",
                    newFileId, ex.Message), ex);
            }
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
                    throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์ลบไฟล์ในโฟลเดอร์นี้");
            }

            // File-level permissions override folder permissions when present
            if (_filePermissionRepository != null)
            {
                var filePerms = _filePermissionRepository.GetByFile(fileId).ToList();
                if (filePerms.Count > 0)
                {
                    var account = _userAccountRepository.GetById(user.UserID);
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
                            throw new UnauthorizedAccessException("คุณถูกปฏิเสธสิทธิ์ในการลบไฟล์นี้");

                        // File permission exists but Delete is not granted → block
                        bool fileAllowDelete = applicable.Any(e => !e.IsDeny && (e.CanDelete || e.CanFullControl));
                        if (!fileAllowDelete)
                            throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์ในการลบไฟล์นี้");
                    }
                }
            }

            _fileDocumentRepository.SoftDelete(fileId, user.UserID);

            // Keep Folder.FileCount in sync after soft-delete.
            try
            {
                _folderRepository.UpdateFileCount(doc.FolderID);
            }
            catch (Exception updateEx)
            {
                _loggingService.Error(string.Format(
                    "FileService: Could not update FileCount via stored procedure after delete for folder {0}: {1}",
                    doc.FolderID, updateEx.Message), updateEx);
            }

            Log(user, "File", "DELETE", "storage.FileDocument", fileId, 1,
                string.Format("Soft-deleted file '{0}'.", doc.OriginalFileName));
        }

        // -----------------------------------------------------------------------
        // Hide/Unhide Files (MOD-003)
        // -----------------------------------------------------------------------

        public void HideFiles(List<int> fileIds, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (fileIds == null) throw new ArgumentNullException("fileIds");

            foreach (var fileId in fileIds)
            {
                var doc = _fileDocumentRepository.GetById(fileId);
                if (doc == null || doc.IsDeleted)
                    continue;

                doc.IsHidden = true;
                doc.HiddenBy = user.ADUserName;
                doc.HiddenDate = DateTime.Now;

                _fileDocumentRepository.Update(doc);

                Log(user, "File", "HIDE", "storage.FileDocument", fileId, 1,
                    string.Format("Hid file '{0}'.", doc.OriginalFileName));
            }
        }

        public void UnhideFiles(List<int> fileIds, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (fileIds == null) throw new ArgumentNullException("fileIds");

            foreach (var fileId in fileIds)
            {
                var doc = _fileDocumentRepository.GetById(fileId);
                if (doc == null || doc.IsDeleted)
                    continue;

                doc.IsHidden = false;
                doc.HiddenBy = null;
                doc.HiddenDate = null;

                _fileDocumentRepository.Update(doc);

                Log(user, "File", "UNHIDE", "storage.FileDocument", fileId, 1,
                    string.Format("Unhid file '{0}'.", doc.OriginalFileName));
            }
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
                    throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์ดูไฟล์ในโฟลเดอร์นี้");
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
            var allFolders = new List<Folder>();
            foreach (var root in rootFolders)
            {
                allFolders.Add(root);
                allFolders.AddRange(_folderRepository.GetDescendants(root.FolderID));
            }

            var report = new StorageUsageReport
            {
                OrgUnitID = orgUnitId,
                OrgUnitName = orgUnit.OrgUnitNameTH
            };

            foreach (var folder in allFolders.Where(f => !f.IsDeleted))
            {
                report.Folders.Add(new FolderStorageEntry
                {
                    FolderID = folder.FolderID,
                    FolderName = folder.FolderName,
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
            if (user == null) throw new ArgumentNullException("user");
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
            if (user == null) throw new ArgumentNullException("user");

            if (request.FileIds == null || !request.FileIds.Any())
                throw new ArgumentException("ต้องระบุไฟล์อย่างน้อย 1 รายการ");

            var destination = _folderRepository.GetById(request.DestinationFolderId);
            if (destination == null)
                throw new InvalidOperationException(
                    string.Format("ไม่พบ Folder ปลายทาง ID {0}", request.DestinationFolderId));

            if (user.AppRole < 4)
            {
                bool hasDestinationAuthority = user.CanAddToFolder(destination.ConfidentialityLevel)
                                            || user.CanEditFolder(destination.ConfidentialityLevel);
                bool hasDestinationAcl = _permissionService.CheckPermission(user.UserID, request.DestinationFolderId, "Write")
                                      || _permissionService.CheckPermission(user.UserID, request.DestinationFolderId, "Modify");

                if (!hasDestinationAuthority && !hasDestinationAcl)
                    throw new UnauthorizedAccessException(
                        string.Format("ไม่มีสิทธิ์ย้ายไปยัง Folder ปลายทาง '{0}'", destination.FolderName));
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
                    var sourceFolder = _folderRepository.GetById(file.FolderID);
                    bool hasSourceAuthority = sourceFolder != null
                                           && (user.CanAddToFolder(sourceFolder.ConfidentialityLevel)
                                            || user.CanEditFolder(sourceFolder.ConfidentialityLevel));
                    bool hasSourceAcl = _permissionService.CheckPermission(user.UserID, file.FolderID, "Write")
                                   || _permissionService.CheckPermission(user.UserID, file.FolderID, "Modify");

                    if (!hasSourceAuthority && !hasSourceAcl)
                        throw new UnauthorizedAccessException(
                            string.Format("ไม่มีสิทธิ์ย้ายจาก Folder ต้นทาง (ID: {0})", file.FolderID));
                }

                int oldFolderId = file.FolderID;
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
                file.FolderID = request.DestinationFolderId;
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
                    Module = "File",
                    Action = "MoveFile",
                    UserID = user.UserID,
                    UserName = user.ADUserName ?? string.Empty,
                    UserRole = user.AppRole,
                    TableName = "storage.FileDocument",
                    RecordID = fileId,
                    OldValue = string.Format("FolderID={0} Path={1}", oldFolderId, oldPhysicalPath),
                    NewValue = string.Format("FolderID={0} Path={1}", request.DestinationFolderId, newPhysicalPath),
                    MachineName = user.MachineName,
                    IPAddress = user.IPAddress,
                    SessionID = user.SessionID,
                    Result = 1,
                    AuditDate = DateTime.Now
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

        public int CopyFile(int fileId, int destinationFolderId, UserContext user, bool overwrite = false)
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

            // Permission check on destination — same rule as upload: FolderAuthorityConfig CanAdd
            // for the destination level, or ACL Write. ACL-only rejected users whose add right
            // comes from their position/level (e.g. Officer with NC "เพิ่ม").
            if (user.AppRole < 4)
            {
                bool hasAuthority = user.CanAddToFolder(destFolder.ConfidentialityLevel);
                bool hasAclWrite = !hasAuthority && _permissionService.CheckPermission(user.UserID, destinationFolderId, "Write");
                if (!hasAuthority && !hasAclWrite)
                    throw new UnauthorizedAccessException(
                        string.Format("ไม่มีสิทธิ์เขียนใน Folder ปลายทาง '{0}'", destFolder.FolderName));
            }

            long existingDestSize = 0;
            if (overwrite && !string.IsNullOrEmpty(source.OriginalFileName))
            {
                var existingForQuota = _fileDocumentRepository.GetByOriginalName(source.OriginalFileName, destinationFolderId);
                existingDestSize = existingForQuota != null ? existingForQuota.FileSizeBytes : 0;
            }

            var quotaBlockMessage = ValidateIncomingBytesAgainstOrgQuota(
                destFolder,
                Math.Max(0, source.FileSizeBytes - existingDestSize),
                "วาง");
            if (!string.IsNullOrEmpty(quotaBlockMessage))
                throw new InvalidOperationException(quotaBlockMessage);

            string ext = source.FileExtension ?? Path.GetExtension(source.OriginalFileName ?? "");
            if (!ext.StartsWith(".")) ext = "." + ext;

            string storedName;
            string destPath;
            string uniqueOriginalName;
            string auditAction;
            FileDocument existing = null;
            int newVersion = 1;

            if (overwrite)
            {
                // Replace mode: keep original display name and bump the version of the file being
                // replaced (same as upload replace). The old record is soft-deleted only after the
                // new one is inserted, so a failed copy does not lose the existing file.
                existing = _fileDocumentRepository.GetByOriginalName(source.OriginalFileName, destinationFolderId);
                if (existing != null)
                    newVersion = existing.Version + 1;

                uniqueOriginalName = source.OriginalFileName;
                // Use timestamp-based stored name to guarantee uniqueness — avoids self-copy
                // when pasting to the same folder the source file lives in
                string origBase = Path.GetFileNameWithoutExtension(source.StoredFileName ?? source.OriginalFileName);
                storedName = string.Format("{0}_{1:yyyyMMddHHmmssfff}{2}", origBase, DateTime.Now, ext);
                destPath = _networkDriveHelper.BuildPath(destFolder.OrgUnitID, destinationFolderId, storedName);
                auditAction = "CopyFile_Replace";
            }
            else
            {
                // SaveCopy mode: always create a new file with unique _copy suffix
                string baseName = Path.GetFileNameWithoutExtension(source.StoredFileName ?? source.OriginalFileName);
                storedName = baseName + "_copy" + ext;
                destPath = _networkDriveHelper.BuildPath(destFolder.OrgUnitID, destinationFolderId, storedName);
                int suffix = 1;
                while (_networkDriveHelper.FileExists(destPath))
                {
                    storedName = string.Format("{0}_copy({1}){2}", baseName, suffix++, ext);
                    destPath = _networkDriveHelper.BuildPath(destFolder.OrgUnitID, destinationFolderId, storedName);
                }

                string origBase = Path.GetFileNameWithoutExtension(source.OriginalFileName ?? "");
                string origExt = Path.GetExtension(source.OriginalFileName ?? "");
                uniqueOriginalName = source.OriginalFileName;
                int origSuffix = 1;
                while (_fileDocumentRepository.ExistsWithName(uniqueOriginalName, destinationFolderId))
                {
                    uniqueOriginalName = string.Format("{0} ({1}){2}", origBase, origSuffix++, origExt);
                }
                auditAction = "CopyFile";
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
                FolderID = destinationFolderId,
                OriginalFileName = uniqueOriginalName,
                StoredFileName = storedName,
                FileExtension = source.FileExtension,
                MimeType = source.MimeType,
                FileSizeBytes = source.FileSizeBytes,
                PhysicalPath = destPath,
                Checksum = source.Checksum,
                VirusScanStatus = source.VirusScanStatus,
                VirusScanDate = source.VirusScanDate,
                FileStatus = 1,
                UploadedBy = user.UserID,
                UploadedDate = DateTime.Now,
                Version = newVersion
            };

            int newId = _fileDocumentRepository.Insert(newDoc);
            if (existing != null)
            {
                _fileDocumentRepository.UpdateVersion(newId, newVersion);
                _fileDocumentRepository.SoftDelete(existing.FileID, user.UserID);
            }
            destFolder.FolderSizeBytes = Math.Max(0, destFolder.FolderSizeBytes - existingDestSize) + source.FileSizeBytes;
            if (!overwrite || existingDestSize <= 0)
                destFolder.FileCount += 1;
            _folderRepository.Update(destFolder);
            RefreshFolderFileCounts(destinationFolderId);

            _auditService.Log(new AuditEntry
            {
                Module = "File",
                Action = auditAction,
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                UserRole = user.AppRole,
                TableName = "storage.FileDocument",
                RecordID = newId,
                OldValue = string.Format("SourceFileID={0} FolderID={1}", fileId, source.FolderID),
                NewValue = string.Format("NewFileID={0} FolderID={1} Path={2}", newId, destinationFolderId, destPath),
                MachineName = user.MachineName,
                IPAddress = user.IPAddress,
                SessionID = user.SessionID,
                Result = 1,
                AuditDate = DateTime.Now
            });

            return newId;
        }

        // -----------------------------------------------------------------------
        // Private: single-file upload pipeline
        // -----------------------------------------------------------------------

        private UploadResult UploadSingleFile(FileUploadRequest request, Folder folder, UserContext user)
        {
            string writtenPath = null;
            int? insertedId = null;

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

                // ── Resolve display name, physical stored name, and version ──────────
                FileDocument existingDoc = null;
                int newVersion = 1;
                string resolvedName;   // OriginalFileName shown to users
                string physicalName;   // actual file name written to network drive

                if (request.OverwriteExisting)
                {
                    // Replace mode: keep original display name; use timestamp-based stored name
                    // so the new physical file does not collide with the old one (soft-delete only)
                    existingDoc = _fileDocumentRepository.GetByOriginalName(request.FileName, folder.FolderID);
                    if (existingDoc != null)
                        newVersion = existingDoc.Version + 1;

                    resolvedName = request.FileName;
                    string origBase = Path.GetFileNameWithoutExtension(request.FileName);
                    physicalName = string.Format("{0}_{1:yyyyMMddHHmmssfff}{2}", origBase, DateTime.Now, ext);
                }
                else
                {
                    resolvedName = ResolveFileName(request.FileName, folder.FolderID);
                    physicalName = resolvedName;
                }

                bool isDuplicate = !string.Equals(resolvedName, request.FileName, StringComparison.OrdinalIgnoreCase);

                // Check quota before proceeding (deduct old file size when replacing)
                if (folder.QuotaBytes.HasValue)
                {
                    long existingSize = existingDoc != null ? existingDoc.FileSizeBytes : 0;
                    long projectedUsage = folder.FolderSizeBytes - existingSize + request.FileSizeBytes;
                    if (projectedUsage > folder.QuotaBytes.Value)
                    {
                        long remainingQuota = folder.QuotaBytes.Value - (folder.FolderSizeBytes - existingSize);
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
                writtenPath = _networkDriveHelper.BuildPath(folder.OrgUnitID, folder.FolderID, physicalName);
                _networkDriveHelper.WriteFile(writtenPath, request.FileStream);

                // ── Verify the physical write landed correctly before creating a DB row ──
                long actualSize = _networkDriveHelper.GetFileSize(writtenPath);
                if (actualSize < 0)
                    throw new IOException(string.Format(
                        "ไฟล์ที่เขียนไม่พบบน network share หลังอัปโหลด ('{0}')", writtenPath));
                if (actualSize == 0 && request.FileSizeBytes > 0)
                    throw new IOException("ไฟล์ที่เขียนมีขนาด 0 bytes");
                if (request.FileSizeBytes > 0 && actualSize != request.FileSizeBytes)
                    throw new IOException(string.Format(
                        "ขนาดไฟล์ที่เขียนไม่ตรง (คาด {0:N0} ได้ {1:N0} bytes)",
                        request.FileSizeBytes, actualSize));

                string storedFileName = Path.GetFileName(writtenPath);
                string mimeType = string.IsNullOrEmpty(request.MimeType) ? "application/octet-stream" : request.MimeType;

                var doc = new FileDocument
                {
                    FolderID = folder.FolderID,
                    OriginalFileName = resolvedName,
                    StoredFileName = storedFileName,
                    FileExtension = ext,
                    MimeType = mimeType,
                    FileSizeBytes = actualSize,
                    PhysicalPath = writtenPath,
                    Checksum = checksum,
                    VirusScanStatus = scanResult.ScannerError ? 0   // 0=Pending (scanner unavailable)
                                     : scanResult.IsClean ? 1   // 1=Clean
                                                             : 2,  // 2=Infected
                    VirusScanDate = DateTime.Now,
                    FileStatus = request.InitialFileStatus ?? 1,
                    UploadedBy = user.UserID,
                    UploadedDate = DateTime.Now,
                    Version = newVersion
                };

                insertedId = _fileDocumentRepository.Insert(doc);

                // The file is written and the row is committed. From here on, any failure in a
                // follow-up bookkeeping step must NOT roll back the upload — otherwise we would
                // delete a good physical file and soft-delete a good row.
                try
                {
                // Replace: bump version and soft-delete old record only after new record is inserted
                if (existingDoc != null)
                {
                    _fileDocumentRepository.UpdateVersion(insertedId.Value, newVersion);
                    _fileDocumentRepository.SoftDelete(existingDoc.FileID, user.UserID);
                }

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
                                FileID = insertedId.Value,
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
                if (existingDoc != null)
                {
                    // Replace: adjust size delta; count stays the same
                    folder.FolderSizeBytes = Math.Max(0, folder.FolderSizeBytes - existingDoc.FileSizeBytes) + request.FileSizeBytes;
                }
                else
                {
                    folder.FolderSizeBytes += request.FileSizeBytes;
                    folder.FileCount += 1;
                }
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
                        resolvedName, actualSize, folder.FolderID));
                }
                catch (Exception postCommitEx)
                {
                    _loggingService.Error(string.Format(
                        "FileService: Upload of '{0}' (FileID {1}) succeeded but a follow-up step failed: {2}",
                        resolvedName, insertedId, postCommitEx.Message), postCommitEx);
                }

                return new UploadResult
                {
                    OriginalFileName = request.FileName,
                    ActualFileName = resolvedName,
                    IsDuplicate = isDuplicate,
                    Success = true,
                    FileDocumentId = insertedId
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
                        string tmp = writtenPath + ".uploading";
                        if (_networkDriveHelper.FileExists(tmp))
                            _networkDriveHelper.DeleteFile(tmp);
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
                Module = module,
                Action = action,
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                UserRole = user.AppRole,
                TableName = tableName,
                RecordID = recordId > 0 ? (int?)recordId : null,
                NewValue = newValue,
                MachineName = user.MachineName,
                IPAddress = user.IPAddress,
                SessionID = user.SessionID,
                Result = result,
                AuditDate = DateTime.Now
            });
        }

        private static UploadResult Fail(string fileName, string message)
        {
            return new UploadResult { OriginalFileName = fileName, Success = false, ErrorMessage = message };
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024)
                return string.Format("{0:N2} GB", bytes / 1073741824.0);
            if (bytes >= 1024L * 1024)
                return string.Format("{0:N1} MB", bytes / 1048576.0);
            if (bytes >= 1024L)
                return string.Format("{0:N1} KB", bytes / 1024.0);
            return string.Format("{0:N0} bytes", bytes);
        }

        public IEnumerable<string> FindDuplicateNames(IEnumerable<string> fileNames, int folderId, UserContext user)
        {
            if (fileNames == null) throw new ArgumentNullException("fileNames");
            if (user == null) throw new ArgumentNullException("user");

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
            string ext = Path.GetExtension(fileName);

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
