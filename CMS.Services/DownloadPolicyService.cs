using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Decides whether a user may save a file without an approval request:
    ///   1) Admin (AppRole 4)
    ///   2) workflow.DownloadUserPermission for the file's confidentiality box
    ///   3) the user's resolved FolderAuthorityConfig.CanDownload for that box (UserContext.Authority, as at login)
    ///   4) an approved, unused DOWNLOAD_FILE request (one approval = one download)
    ///   otherwise an approval request is needed.
    /// Opening a file for viewing is not covered here; it still uses IFileService.DownloadFile directly.
    /// </summary>
    public class DownloadPolicyService : IDownloadPolicyService
    {
        private const int AdminRole = 4;

        private readonly IDownloadUserPermissionRepository _userPermissionRepository;
        private readonly IFileDocumentRepository _fileDocumentRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IApprovalService _approvalService;
        private readonly IFileService _fileService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        public DownloadPolicyService(
            IDownloadUserPermissionRepository userPermissionRepository,
            IFileDocumentRepository fileDocumentRepository,
            IFolderRepository folderRepository,
            IApprovalService approvalService,
            IFileService fileService,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            if (userPermissionRepository == null) throw new ArgumentNullException("userPermissionRepository");
            if (fileDocumentRepository   == null) throw new ArgumentNullException("fileDocumentRepository");
            if (folderRepository         == null) throw new ArgumentNullException("folderRepository");
            if (approvalService          == null) throw new ArgumentNullException("approvalService");
            if (fileService              == null) throw new ArgumentNullException("fileService");
            if (auditService             == null) throw new ArgumentNullException("auditService");
            if (loggingService           == null) throw new ArgumentNullException("loggingService");

            _userPermissionRepository = userPermissionRepository;
            _fileDocumentRepository   = fileDocumentRepository;
            _folderRepository         = folderRepository;
            _approvalService          = approvalService;
            _fileService              = fileService;
            _auditService             = auditService;
            _loggingService           = loggingService;
        }

        // ── Decision ──────────────────────────────────────────────────────────

        public DownloadCheckResult CheckDownload(int fileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            int level = FileConfidentialityLevel(fileId);
            var result = new DownloadCheckResult { ConfidentialityLevel = level };

            if (user.AppRole == AdminRole)
                return Direct(result, "Admin");

            DownloadUserPermission personal = null;
            try
            {
                personal = _userPermissionRepository.GetByUser(user.UserID);
            }
            catch (Exception ex)
            {
                // ตารางยังไม่ถูกสร้าง (ยังไม่ได้รัน download_policy.sql) → ข้ามสิทธิ์รายคน
                _loggingService.Error("DownloadPolicyService: Failed to load user download permission.", ex);
            }
            if (personal != null && personal.AllowsLevel(level))
                return Direct(result, "สิทธิ์รายคน");

            FolderAuthorityConfig cfg;
            if (user.Authority != null && user.Authority.TryGetValue(level, out cfg) && cfg != null && cfg.CanDownload)
                return Direct(result, "Authority Config: " + (cfg.ConfigLabel ?? ""));

            var approved = _approvalService.GetApprovedDownloadRequest(fileId, user.UserID);
            if (approved != null)
            {
                result.Decision          = DownloadDecision.Approved;
                result.ApprovalRequestId = approved.RequestID;
                result.Reason            = "คำขออนุมัติ ID " + approved.RequestID;
                return result;
            }

            result.Decision = DownloadDecision.NeedsApproval;
            result.Reason   = "ต้องขออนุมัติ";
            return result;
        }

        public FileDownloadResult DownloadForSave(int fileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var check = CheckDownload(fileId, user);
            if (!check.CanDownloadNow)
                throw new UnauthorizedAccessException("ไฟล์นี้ต้องได้รับอนุมัติก่อนดาวน์โหลด กรุณาส่งคำขออนุมัติ");

            var file = _fileService.DownloadFile(fileId, user);

            if (file != null && file.Success)
            {
                if (check.Decision == DownloadDecision.Approved && check.ApprovalRequestId.HasValue)
                    _approvalService.ConsumeDownloadApproval(check.ApprovalRequestId.Value, user);

                _auditService.Log(new AuditEntry
                {
                    Module    = "Download",
                    Action    = "DOWNLOAD_FILE",
                    UserID    = user.UserID,
                    UserName  = user.ADUserName ?? string.Empty,
                    UserRole  = user.AppRole,
                    TableName = "storage.FileDocument",
                    RecordID  = fileId,
                    NewValue  = string.Format("Level:{0} By:{1}", check.ConfidentialityLevel, check.Reason),
                    Result    = 1,
                    AuditDate = DateTime.Now
                });
            }
            return file;
        }

        private static DownloadCheckResult Direct(DownloadCheckResult result, string reason)
        {
            result.Decision = DownloadDecision.Direct;
            result.Reason   = reason;
            return result;
        }

        /// <summary>FileDocument.ConfidentialityLevel is transient; the folder holds the real level.</summary>
        private int FileConfidentialityLevel(int fileId)
        {
            var doc = _fileDocumentRepository.GetById(fileId);
            if (doc == null)
                throw new InvalidOperationException(string.Format("FileDocument with ID {0} was not found.", fileId));

            var folder = _folderRepository.GetById(doc.FolderID);
            return folder != null ? folder.ConfidentialityLevel : doc.ConfidentialityLevel;
        }

        // ── Per-user permission admin ─────────────────────────────────────────

        public IList<DownloadUserPermission> GetUserPermissions()
        {
            return _userPermissionRepository.GetAll().ToList();
        }

        public void SaveUserPermission(DownloadUserPermission permission, UserContext user)
        {
            if (permission == null) throw new ArgumentNullException("permission");
            RequireAdmin(user);
            if (permission.UserID <= 0) throw new ArgumentException("UserID ไม่ถูกต้อง", "permission");

            var old = _userPermissionRepository.GetByUser(permission.UserID);

            if (!permission.CanHC && !permission.CanSC && !permission.CanNC && !permission.CanPC)
            {
                // ไม่ติ๊กเลย = ไม่มีสิทธิ์รายคน
                if (old != null) RemoveUserPermission(permission.UserID, user);
                return;
            }

            _userPermissionRepository.Upsert(permission, user.UserID);
            Audit(user, "SAVE_DOWNLOAD_USER_PERMISSION", permission.UserID, Describe(old), Describe(permission));
        }

        public void RemoveUserPermission(int userId, UserContext user)
        {
            RequireAdmin(user);
            var old = _userPermissionRepository.GetByUser(userId);
            if (old == null) return;

            _userPermissionRepository.Deactivate(userId, user.UserID);
            Audit(user, "REMOVE_DOWNLOAD_USER_PERMISSION", userId, Describe(old), null);
        }

        private static void RequireAdmin(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole != AdminRole)
                throw new UnauthorizedAccessException("การตั้งค่าสิทธิ์ดาวน์โหลดทำได้เฉพาะ Admin");
        }

        private static string Describe(DownloadUserPermission p)
        {
            if (p == null) return null;
            return string.Format("UserID:{0} HC:{1} SC:{2} NC:{3} PC:{4}", p.UserID, p.CanHC, p.CanSC, p.CanNC, p.CanPC);
        }

        private void Audit(UserContext user, string action, int userId, string oldValue, string newValue)
        {
            _auditService.Log(new AuditEntry
            {
                Module    = "Download",
                Action    = action,
                UserID    = user.UserID,
                UserName  = user.ADUserName ?? string.Empty,
                UserRole  = user.AppRole,
                TableName = "workflow.DownloadUserPermission",
                RecordID  = userId,
                OldValue  = oldValue,
                NewValue  = newValue,
                Result    = 1,
                AuditDate = DateTime.Now
            });
        }
    }
}
