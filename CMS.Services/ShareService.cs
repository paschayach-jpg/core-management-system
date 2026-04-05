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
    /// Service for managing internal and external file/folder shares.
    /// Handles share creation, approval, revocation, and access control.
    /// </summary>
    public class ShareService : IShareService
    {
        private const int MaxShareDurationDays = 90;
        private const int PendingApprovalExpiryDays = 7;

        private readonly IExternalShareRepository _externalShareRepository;
        private readonly IShareTokenRepository _shareTokenRepository;
        private readonly IShareAccessLogRepository _shareAccessLogRepository;
        private readonly IFileDocumentRepository _fileDocumentRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IFilePermissionRepository _filePermissionRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IShareTokenService _shareTokenService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        public ShareService(
            IExternalShareRepository externalShareRepository,
            IShareTokenRepository shareTokenRepository,
            IShareAccessLogRepository shareAccessLogRepository,
            IFileDocumentRepository fileDocumentRepository,
            IFolderRepository folderRepository,
            IFilePermissionRepository filePermissionRepository,
            IPermissionRepository permissionRepository,
            IShareTokenService shareTokenService,
            IPermissionService permissionService,
            INotificationService notificationService,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            if (externalShareRepository == null) throw new ArgumentNullException("externalShareRepository");
            if (shareTokenRepository == null) throw new ArgumentNullException("shareTokenRepository");
            if (shareAccessLogRepository == null) throw new ArgumentNullException("shareAccessLogRepository");
            if (fileDocumentRepository == null) throw new ArgumentNullException("fileDocumentRepository");
            if (folderRepository == null) throw new ArgumentNullException("folderRepository");
            if (filePermissionRepository == null) throw new ArgumentNullException("filePermissionRepository");
            if (permissionRepository == null) throw new ArgumentNullException("permissionRepository");
            if (shareTokenService == null) throw new ArgumentNullException("shareTokenService");
            if (permissionService == null) throw new ArgumentNullException("permissionService");
            if (notificationService == null) throw new ArgumentNullException("notificationService");
            if (auditService == null) throw new ArgumentNullException("auditService");
            if (loggingService == null) throw new ArgumentNullException("loggingService");

            _externalShareRepository = externalShareRepository;
            _shareTokenRepository = shareTokenRepository;
            _shareAccessLogRepository = shareAccessLogRepository;
            _fileDocumentRepository = fileDocumentRepository;
            _folderRepository = folderRepository;
            _filePermissionRepository = filePermissionRepository;
            _permissionRepository = permissionRepository;
            _shareTokenService = shareTokenService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _auditService = auditService;
            _loggingService = loggingService;
        }

        // =====================================================================
        // Internal Sharing Methods
        // =====================================================================

        public FilePermission CreateInternalShare(CreateInternalShareRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user == null) throw new ArgumentNullException("user");

            // Validate user has Read permission on file
            if (!_permissionService.CheckPermission(user.UserID, request.FileID, "Read"))
                throw new UnauthorizedAccessException("Insufficient permissions to share this file.");

            // Create FilePermission record
            var filePermission = new FilePermission
            {
                FileID = request.FileID,
                PrincipalType = request.PrincipalType,
                PrincipalID = request.PrincipalID,
                CanRead = request.CanRead,
                CanWrite = request.CanWrite,
                CanDelete = request.CanDelete,
                CanModify = false,
                CanFullControl = false,
                IsDeny = false,
                ExpiryDate = request.ExpiryDate,
                GrantedBy = user.UserID,
                GrantedDate = DateTime.Now
            };

            int permissionId = _filePermissionRepository.Insert(filePermission);
            filePermission.FilePermissionID = permissionId;

            // Log audit entry
            _auditService.Log(new AuditEntry
            {
                Module = "FileSharing",
                Action = "CreateInternalShare",
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                TableName = "storage.FilePermission",
                RecordID = permissionId,
                NewValue = string.Format("PrincipalType={0}, PrincipalID={1}, Permissions=Read:{2},Write:{3},Delete:{4}",
                    request.PrincipalType, request.PrincipalID, request.CanRead, request.CanWrite, request.CanDelete),
                Result = 1,
                AuditDate = DateTime.Now
            });

            // Send notification
            var file = _fileDocumentRepository.GetById(request.FileID);
            _notificationService.Send(new NotificationRequest
            {
                RecipientUserId = request.PrincipalID,
                EventType = "InternalShareCreated",
                RelatedEntityId = permissionId.ToString(),
                MessageText = string.Format("File '{0}' has been shared with you by {1}. Access level: Read={2}, Write={3}, Delete={4}",
                    file?.OriginalFileName ?? "Unknown", user.ADUserName, request.CanRead, request.CanWrite, request.CanDelete)
            });

            return filePermission;
        }

        public void RevokeInternalShare(int filePermissionId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var permission = _filePermissionRepository.GetById(filePermissionId);
            if (permission == null)
                throw new InvalidOperationException("File permission not found.");

            // Validate user is Sharer or Admin
            if (permission.GrantedBy != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("Only the original Sharer or an Administrator can revoke this share.");

            _filePermissionRepository.Revoke(filePermissionId, user.UserID, "Revoked by Sharer");

            // Log audit entry
            _auditService.Log(new AuditEntry
            {
                Module = "FileSharing",
                Action = "RevokeInternalShare",
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                TableName = "storage.FilePermission",
                RecordID = filePermissionId,
                NewValue = "Status=Revoked",
                Result = 1,
                AuditDate = DateTime.Now
            });
        }

        // =====================================================================
        // External Sharing Methods
        // =====================================================================

        public ExternalShare CreateExternalShare(CreateExternalShareRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user == null) throw new ArgumentNullException("user");

            // Validate file or folder is specified
            if ((!request.FileID.HasValue && !request.FolderID.HasValue) ||
                (request.FileID.HasValue && request.FolderID.HasValue))
                throw new ArgumentException("Either FileID or FolderID must be specified, but not both.");

            // Validate user has Read permission
            int targetId = request.FileID ?? request.FolderID.Value;
            if (!_permissionService.CheckPermission(user.UserID, targetId, "Read"))
                throw new UnauthorizedAccessException("Insufficient permissions to share this file/folder.");

            // Validate confidentiality level
            if (request.FileID.HasValue)
            {
                var file = _fileDocumentRepository.GetById(request.FileID.Value);
                if (file == null)
                    throw new InvalidOperationException("File not found.");
                if (file.ConfidentialityLevel == 1) // 1HC
                    throw new InvalidOperationException("Highly Confidential files cannot be shared externally.");
            }
            else
            {
                var folder = _folderRepository.GetById(request.FolderID.Value);
                if (folder == null)
                    throw new InvalidOperationException("Folder not found.");
                if (folder.ConfidentialityLevel == 1) // 1HC
                    throw new InvalidOperationException("Folder contains restricted content.");

                // Check for HC files in folder
                var files = _fileDocumentRepository.GetByFolder(request.FolderID.Value);
                if (files.Any(f => f.ConfidentialityLevel == 1))
                    throw new InvalidOperationException("Folder contains restricted content.");
            }

            // Validate expiry duration
            if (request.ExpiryDate.HasValue)
            {
                var duration = (request.ExpiryDate.Value - DateTime.Now).TotalDays;
                if (duration > MaxShareDurationDays)
                    throw new InvalidOperationException(string.Format(
                        "Maximum share duration is {0} days.", MaxShareDurationDays));
            }

            // Generate token and hash password
            var token = _shareTokenService.GenerateToken();
            var passwordHash = request.Password != null ? _shareTokenService.HashPassword(request.Password) : null;

            // Create ExternalShare record
            var externalShare = new ExternalShare
            {
                ShareToken = token,
                FileID = request.FileID,
                FolderID = request.FolderID,
                SharerUserID = user.UserID,
                PasswordHash = passwordHash,
                ExpiryDate = request.ExpiryDate,
                DownloadLimit = request.DownloadLimit,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Pending",
                CreatedDate = DateTime.Now,
                SharePurpose = request.SharePurpose,
                NotifyOnAccess = request.NotifyOnAccess
            };

            int shareId = _externalShareRepository.Insert(externalShare);
            externalShare.ExternalShareID = shareId;

            // Create ShareToken record
            var shareToken = new ShareToken
            {
                TokenValue = token,
                ExternalShareID = shareId,
                Status = "Pending",
                CreatedDate = DateTime.Now
            };
            _shareTokenRepository.Insert(shareToken);

            // Log audit entry
            _auditService.Log(new AuditEntry
            {
                Module = "FileSharing",
                Action = "CreateExternalShare",
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                TableName = "storage.ExternalShare",
                RecordID = shareId,
                NewValue = string.Format("Token={0}, Status=Pending, Expiry={1}, DownloadLimit={2}",
                    token, request.ExpiryDate, request.DownloadLimit),
                Result = 1,
                AuditDate = DateTime.Now
            });

            // Send notification to Sharer
            var targetName = request.FileID.HasValue
                ? _fileDocumentRepository.GetById(request.FileID.Value)?.OriginalFileName ?? "Unknown"
                : _folderRepository.GetById(request.FolderID.Value)?.FolderName ?? "Unknown";

            _notificationService.Send(new NotificationRequest
            {
                RecipientUserId = user.UserID,
                EventType = "ExternalShareCreated",
                RelatedEntityId = shareId.ToString(),
                MessageText = string.Format(
                    "External share created for '{0}'. Link: {1}. Status: Pending approval. Expiry: {2}. Download limit: {3}",
                    targetName, token, request.ExpiryDate?.ToString("yyyy-MM-dd") ?? "None", request.DownloadLimit ?? 0)
            });

            return externalShare;
        }

        public void ApproveExternalShare(int externalShareId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var share = _externalShareRepository.GetById(externalShareId);
            if (share == null)
                throw new InvalidOperationException("External share not found.");

            if (share.ShareTokenStatus != "Pending")
                throw new InvalidOperationException("Only Pending shares can be approved.");

            share.ShareTokenStatus = "Active";
            share.ApprovedDate = DateTime.Now;
            share.ApproverUserID = user.UserID;

            _externalShareRepository.Update(share);

            // Update ShareToken status
            var tokens = _shareTokenRepository.GetByExternalShare(externalShareId).ToList();
            foreach (var token in tokens)
            {
                token.Status = "Active";
                token.ActivatedDate = DateTime.Now;
                _shareTokenRepository.Update(token);
            }

            // Log audit entry
            _auditService.Log(new AuditEntry
            {
                Module = "FileSharing",
                Action = "ApproveExternalShare",
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                TableName = "storage.ExternalShare",
                RecordID = externalShareId,
                NewValue = "Status=Active",
                Result = 1,
                AuditDate = DateTime.Now
            });

            // Send notification to Sharer
            _notificationService.Send(new NotificationRequest
            {
                RecipientUserId = share.SharerUserID,
                EventType = "ExternalShareApproved",
                RelatedEntityId = externalShareId.ToString(),
                MessageText = string.Format(
                    "Your external share has been approved. Link is now active. Token: {0}",
                    share.ShareToken)
            });
        }

        public void RejectExternalShare(int externalShareId, string rejectionReason, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var share = _externalShareRepository.GetById(externalShareId);
            if (share == null)
                throw new InvalidOperationException("External share not found.");

            if (share.ShareTokenStatus != "Pending")
                throw new InvalidOperationException("Only Pending shares can be rejected.");

            share.ShareTokenStatus = "Revoked";
            share.RevokedBy = user.UserID;
            share.RevokedDate = DateTime.Now;
            share.RevocationReason = rejectionReason;

            _externalShareRepository.Update(share);

            // Update ShareToken status
            var tokens = _shareTokenRepository.GetByExternalShare(externalShareId).ToList();
            foreach (var token in tokens)
            {
                token.Status = "Revoked";
                token.DisabledDate = DateTime.Now;
                token.DisabledByUserID = user.UserID;
                token.DisabledReason = rejectionReason;
                _shareTokenRepository.Update(token);
            }

            // Log audit entry
            _auditService.Log(new AuditEntry
            {
                Module = "FileSharing",
                Action = "RejectExternalShare",
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                TableName = "storage.ExternalShare",
                RecordID = externalShareId,
                NewValue = string.Format("Status=Revoked, Reason={0}", rejectionReason),
                Result = 1,
                AuditDate = DateTime.Now
            });

            // Send notification to Sharer
            _notificationService.Send(new NotificationRequest
            {
                RecipientUserId = share.SharerUserID,
                EventType = "ExternalShareRejected",
                RelatedEntityId = externalShareId.ToString(),
                MessageText = string.Format("Your external share has been rejected. Reason: {0}", rejectionReason)
            });
        }

        public void RevokeExternalShare(int externalShareId, string revocationReason, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var share = _externalShareRepository.GetById(externalShareId);
            if (share == null)
                throw new InvalidOperationException("External share not found.");

            // Validate user is Sharer or Admin
            if (share.SharerUserID != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("Only the original Sharer or an Administrator can revoke this share.");

            share.ShareTokenStatus = "Revoked";
            share.RevokedBy = user.UserID;
            share.RevokedDate = DateTime.Now;
            share.RevocationReason = revocationReason;

            _externalShareRepository.Update(share);

            // Update ShareToken status
            var tokens = _shareTokenRepository.GetByExternalShare(externalShareId).ToList();
            foreach (var token in tokens)
            {
                token.Status = "Revoked";
                token.DisabledDate = DateTime.Now;
                token.DisabledByUserID = user.UserID;
                token.DisabledReason = revocationReason;
                _shareTokenRepository.Update(token);
            }

            // Log audit entry
            _auditService.Log(new AuditEntry
            {
                Module = "FileSharing",
                Action = "RevokeExternalShare",
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                TableName = "storage.ExternalShare",
                RecordID = externalShareId,
                NewValue = string.Format("Status=Revoked, Reason={0}", revocationReason),
                Result = 1,
                AuditDate = DateTime.Now
            });

            // Send notification to Sharer
            _notificationService.Send(new NotificationRequest
            {
                RecipientUserId = share.SharerUserID,
                EventType = "ExternalShareRevoked",
                RelatedEntityId = externalShareId.ToString(),
                MessageText = string.Format("Your external share has been revoked. Reason: {0}", revocationReason)
            });
        }

        public void ModifyExternalShare(ModifyExternalShareRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user == null) throw new ArgumentNullException("user");

            var share = _externalShareRepository.GetById(request.ExternalShareID);
            if (share == null)
                throw new InvalidOperationException("External share not found.");

            // Validate user is Sharer or Admin
            if (share.SharerUserID != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("Only the original Sharer or an Administrator can modify this share.");

            // Validate share is not Expired or Disabled
            if (share.ShareTokenStatus == "Expired" || share.ShareTokenStatus == "Disabled")
                throw new InvalidOperationException(
                    string.Format("Cannot modify a {0} share.", share.ShareTokenStatus));

            string oldValue = string.Format("ExpiryDate={0}, DownloadLimit={1}",
                share.ExpiryDate, share.DownloadLimit);

            // Update expiry date if provided
            if (request.NewExpiryDate.HasValue)
            {
                var duration = (request.NewExpiryDate.Value - DateTime.Now).TotalDays;
                if (duration > MaxShareDurationDays)
                    throw new InvalidOperationException(string.Format(
                        "Maximum share duration is {0} days.", MaxShareDurationDays));
                share.ExpiryDate = request.NewExpiryDate;
            }

            // Update download limit if provided
            if (request.NewDownloadLimit.HasValue)
            {
                if (request.NewDownloadLimit.Value <= 0)
                    throw new ArgumentException("Download limit must be positive.");
                share.DownloadLimit = request.NewDownloadLimit;
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                share.PasswordHash = _shareTokenService.HashPassword(request.NewPassword);
            }

            _externalShareRepository.Update(share);

            // Log audit entry
            _auditService.Log(new AuditEntry
            {
                Module = "FileSharing",
                Action = "ModifyExternalShare",
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                TableName = "storage.ExternalShare",
                RecordID = request.ExternalShareID,
                OldValue = oldValue,
                NewValue = string.Format("ExpiryDate={0}, DownloadLimit={1}",
                    share.ExpiryDate, share.DownloadLimit),
                Result = 1,
                AuditDate = DateTime.Now
            });

            // Send notification to Sharer
            _notificationService.Send(new NotificationRequest
            {
                RecipientUserId = share.SharerUserID,
                EventType = "ExternalShareModified",
                RelatedEntityId = request.ExternalShareID.ToString(),
                MessageText = string.Format(
                    "Your external share has been modified. New expiry: {0}. New download limit: {1}",
                    request.NewExpiryDate?.ToString("yyyy-MM-dd") ?? "None", request.NewDownloadLimit ?? 0)
            });
        }

        // =====================================================================
        // Query Methods
        // =====================================================================

        public IEnumerable<FilePermission> GetInternalSharesForUser(int userId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Validate user is querying their own shares or is Admin
            if (userId != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("You can only view your own shares.");

            return _filePermissionRepository.GetByGrantedBy(userId);
        }

        public IEnumerable<ExternalShare> GetExternalSharesForUser(int userId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Validate user is querying their own shares or is Admin
            if (userId != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("You can only view your own shares.");

            return _externalShareRepository.GetBySharer(userId);
        }

        public IEnumerable<ExternalShare> GetExternalSharesByStatus(string status, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Validate user is Admin or Manager
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("Only Managers and Administrators can query shares by status.");

            return _externalShareRepository.GetByStatus(status);
        }

        public IEnumerable<ShareAccessLog> GetAccessLog(int externalShareId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var share = _externalShareRepository.GetById(externalShareId);
            if (share == null)
                throw new InvalidOperationException("External share not found.");

            // Validate user is Sharer or Admin
            if (share.SharerUserID != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("Only the original Sharer or an Administrator can view access logs.");

            return _shareAccessLogRepository.GetByExternalShare(externalShareId)
                .OrderByDescending(l => l.AccessTimestamp);
        }

        // =====================================================================
        // Validation Methods
        // =====================================================================

        public bool CanShareFile(int fileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Check Read permission
            if (!_permissionService.CheckPermission(user.UserID, fileId, "Read"))
                return false;

            // Check confidentiality level
            var file = _fileDocumentRepository.GetById(fileId);
            if (file == null || file.ConfidentialityLevel == 1) // 1HC
                return false;

            return true;
        }

        public bool CanShareFolder(int folderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Check Read permission
            if (!_permissionService.CheckPermission(user.UserID, folderId, "Read"))
                return false;

            // Check confidentiality level
            var folder = _folderRepository.GetById(folderId);
            if (folder == null || folder.ConfidentialityLevel == 1) // 1HC
                return false;

            // Check for HC files in folder
            var files = _fileDocumentRepository.GetByFolder(folderId);
            if (files.Any(f => f.ConfidentialityLevel == 1))
                return false;

            return true;
        }
    }
}
