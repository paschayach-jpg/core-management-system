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
    /// Service for managing access control and download tracking for external shares.
    /// Handles access validation, logging, and auto-termination of shares.
    /// Satisfies Requirements 6.1, 6.2, 6.3, 6.4, 6.5, 8.2, 8.3, 8.4, 9.1, 9.2, 9.3, 11.1, 11.2, 11.4.
    /// </summary>
    public class ShareAccessService : IShareAccessService
    {
        private readonly IExternalShareRepository _externalShareRepository;
        private readonly IShareAccessLogRepository _shareAccessLogRepository;
        private readonly IShareTokenService _shareTokenService;
        private readonly IFileDocumentRepository _fileDocumentRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly ILoggingService _loggingService;

        public ShareAccessService(
            IExternalShareRepository externalShareRepository,
            IShareAccessLogRepository shareAccessLogRepository,
            IShareTokenService shareTokenService,
            IFileDocumentRepository fileDocumentRepository,
            IFolderRepository folderRepository,
            IAuditService auditService,
            INotificationService notificationService,
            ILoggingService loggingService)
        {
            _externalShareRepository = externalShareRepository;
            _shareAccessLogRepository = shareAccessLogRepository;
            _shareTokenService = shareTokenService;
            _fileDocumentRepository = fileDocumentRepository;
            _folderRepository = folderRepository;
            _auditService = auditService;
            _notificationService = notificationService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Validates and grants access to an external share.
        /// Verifies token status, expiry date, password, and download limit.
        /// Increments download counter and logs access attempt.
        /// Validates: Requirements 6.1, 6.2, 6.3, 6.4, 6.5, 8.2, 8.3, 8.4
        /// </summary>
        public ShareAccessResult ValidateAndGrantAccess(Guid token, string password, UserContext user)
        {
            var result = new ShareAccessResult { IsGranted = false };

            try
            {
                // 1. Lookup share by token
                var share = _externalShareRepository.GetByToken(token);
                if (share == null)
                {
                    result.DenyReason = "Share link not found.";
                    LogAccessAttempt(new ShareAccessLog
                    {
                        ShareToken = token,
                        ExternalShareID = 0,
                        RecipientIdentifier = user?.ADUserName ?? "Anonymous",
                        AccessType = "Download",
                        IsSuccessful = false,
                        FailureReason = "TokenNotFound",
                        AccessTimestamp = DateTime.Now,
                        IPAddress = user?.IPAddress,
                        UserAgent = null
                    });
                    return result;
                }

                // 2. Verify token status is Active (Requirement 6.1)
                if (share.ShareTokenStatus != "Active")
                {
                    string failureReason;
                    switch (share.ShareTokenStatus)
                    {
                        case "Pending":
                            result.DenyReason = "Link pending approval.";
                            failureReason = "Pending";
                            break;
                        case "Expired":
                            result.DenyReason = "Link expired.";
                            failureReason = "Expired";
                            break;
                        case "Disabled":
                            result.DenyReason = "Link disabled.";
                            failureReason = "Disabled";
                            break;
                        case "Revoked":
                            result.DenyReason = "Link revoked.";
                            failureReason = "Revoked";
                            break;
                        default:
                            result.DenyReason = "Link is not active.";
                            failureReason = "InvalidStatus";
                            break;
                    }

                    LogAccessAttempt(new ShareAccessLog
                    {
                        ShareToken = token,
                        ExternalShareID = share.ExternalShareID,
                        RecipientIdentifier = user?.ADUserName ?? "Anonymous",
                        AccessType = "Download",
                        IsSuccessful = false,
                        FailureReason = failureReason,
                        AccessTimestamp = DateTime.Now,
                        IPAddress = user?.IPAddress,
                        UserAgent = null
                    });

                    return result;
                }

                // 3. Verify expiry date (Requirement 6.4, 6.5)
                if (share.ExpiryDate.HasValue && DateTime.Now > share.ExpiryDate.Value)
                {
                    result.DenyReason = "Link expired.";
                    LogAccessAttempt(new ShareAccessLog
                    {
                        ShareToken = token,
                        ExternalShareID = share.ExternalShareID,
                        RecipientIdentifier = user?.ADUserName ?? "Anonymous",
                        AccessType = "Download",
                        IsSuccessful = false,
                        FailureReason = "Expired",
                        AccessTimestamp = DateTime.Now,
                        IPAddress = user?.IPAddress,
                        UserAgent = null
                    });

                    return result;
                }

                // 4. Verify password if set (Requirement 6.3)
                if (!string.IsNullOrEmpty(share.PasswordHash))
                {
                    if (string.IsNullOrEmpty(password) || !_shareTokenService.VerifyPassword(password, share.PasswordHash))
                    {
                        result.DenyReason = "Incorrect password.";
                        LogAccessAttempt(new ShareAccessLog
                        {
                            ShareToken = token,
                            ExternalShareID = share.ExternalShareID,
                            RecipientIdentifier = user?.ADUserName ?? "Anonymous",
                            AccessType = "Download",
                            IsSuccessful = false,
                            FailureReason = "WrongPassword",
                            AccessTimestamp = DateTime.Now,
                            IPAddress = user?.IPAddress,
                            UserAgent = null
                        });

                        return result;
                    }
                }

                // 5. Verify download limit not reached (Requirement 8.2, 8.3, 8.4)
                if (share.DownloadLimit.HasValue && share.CurrentDownloadCount >= share.DownloadLimit.Value)
                {
                    result.DenyReason = "Download limit reached.";
                    LogAccessAttempt(new ShareAccessLog
                    {
                        ShareToken = token,
                        ExternalShareID = share.ExternalShareID,
                        RecipientIdentifier = user?.ADUserName ?? "Anonymous",
                        AccessType = "Download",
                        IsSuccessful = false,
                        FailureReason = "LimitReached",
                        AccessTimestamp = DateTime.Now,
                        IPAddress = user?.IPAddress,
                        UserAgent = null
                    });

                    return result;
                }

                // 6. Load file/folder information
                FileDocument file = null;
                Folder folder = null;
                long? fileSizeBytes = null;

                if (share.FileID.HasValue)
                {
                    file = _fileDocumentRepository.GetById(share.FileID.Value);
                    if (file != null)
                    {
                        fileSizeBytes = file.FileSizeBytes;
                    }
                }
                else if (share.FolderID.HasValue)
                {
                    folder = _folderRepository.GetById(share.FolderID.Value);
                }

                // 7. Log successful access attempt (Requirement 9.1, 9.2, 9.3)
                LogAccessAttempt(new ShareAccessLog
                {
                    ShareToken = token,
                    ExternalShareID = share.ExternalShareID,
                    RecipientIdentifier = user?.ADUserName ?? "Anonymous",
                    AccessType = "Download",
                    IsSuccessful = true,
                    FailureReason = null,
                    FileSizeBytes = fileSizeBytes,
                    AccessTimestamp = DateTime.Now,
                    IPAddress = user?.IPAddress,
                    UserAgent = null
                });

                // 8. Increment download counter and check if limit reached
                IncrementDownloadCount(share.ExternalShareID);

                // 9. Send access notification if enabled (Requirement 11.1, 11.2)
                if (share.NotifyOnAccess)
                {
                    var fileName = file?.OriginalFileName ?? folder?.FolderName ?? "Unknown";
                    _notificationService.Send(new NotificationRequest
                    {
                        RecipientUserId = share.SharerUserID,
                        EventType = "ShareAccessed",
                        RelatedEntityId = share.ExternalShareID.ToString(),
                        MessageText = $"Your shared file '{fileName}' was accessed by {(user?.ADUserName ?? "Anonymous")} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}."
                    });
                }

                // 10. Log audit entry
                _auditService.Log(new AuditEntry
                {
                    Module = "FileSharing",
                    Action = "AccessGranted",
                    UserID = user?.UserID ?? 0,
                    UserName = user?.ADUserName ?? "Anonymous",
                    TableName = "ExternalShare",
                    RecordID = share.ExternalShareID,
                    NewValue = $"Token={token}, Recipient={user?.ADUserName ?? "Anonymous"}, AccessTime={DateTime.Now}",
                    Result = 1
                });

                // 11. Return success result
                result.IsGranted = true;
                result.File = file;
                result.Folder = folder;
                result.RemainingDownloads = share.DownloadLimit.HasValue
                    ? Math.Max(0, share.DownloadLimit.Value - share.CurrentDownloadCount - 1)
                    : -1; // -1 means unlimited

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error validating access for token {token}: {ex.Message}", ex);
                result.DenyReason = "An error occurred while processing your request.";
                return result;
            }
        }

        /// <summary>
        /// Creates an immutable access log entry for successful or failed access attempts.
        /// Validates: Requirements 9.1, 9.2, 9.3
        /// </summary>
        public void LogAccessAttempt(ShareAccessLog log)
        {
            try
            {
                if (log == null)
                    throw new ArgumentNullException(nameof(log));

                // Ensure timestamp is set
                if (log.AccessTimestamp == default(DateTime))
                    log.AccessTimestamp = DateTime.Now;

                // Insert into database (append-only)
                var logId = _shareAccessLogRepository.Insert(log);

                _loggingService.Info($"Access log entry created: LogID={logId}, Token={log.ShareToken}, Success={log.IsSuccessful}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error logging access attempt: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Increments the download counter for an external share.
        /// If download limit is reached, automatically transitions status to Disabled.
        /// Validates: Requirements 8.2, 8.3, 8.4
        /// </summary>
        public void IncrementDownloadCount(int externalShareId)
        {
            try
            {
                var share = _externalShareRepository.GetById(externalShareId);
                if (share == null)
                    throw new InvalidOperationException($"External share {externalShareId} not found.");

                // Increment counter
                share.CurrentDownloadCount++;

                // Check if limit reached (Requirement 8.3, 8.4)
                if (share.DownloadLimit.HasValue && share.CurrentDownloadCount >= share.DownloadLimit.Value)
                {
                    share.ShareTokenStatus = "Disabled";
                    share.DisabledDate = DateTime.Now;
                    share.DisabledReason = "Download limit reached";

                    _loggingService.Info($"Share {externalShareId} disabled: download limit reached ({share.CurrentDownloadCount}/{share.DownloadLimit})");

                    // Log audit entry
                    _auditService.Log(new AuditEntry
                    {
                        Module = "FileSharing",
                        Action = "ShareDisabled",
                        UserID = 0,
                        UserName = "System",
                        TableName = "ExternalShare",
                        RecordID = externalShareId,
                        NewValue = $"Status=Disabled, Reason=DownloadLimitReached, Count={share.CurrentDownloadCount}/{share.DownloadLimit}",
                        Result = 1
                    });

                    // Send notification to sharer (Requirement 11.1, 11.2)
                    var fileOrFolder = share.FileID.HasValue
                        ? _fileDocumentRepository.GetById(share.FileID.Value)?.OriginalFileName
                        : _folderRepository.GetById(share.FolderID.Value)?.FolderName;

                    _notificationService.Send(new NotificationRequest
                    {
                        RecipientUserId = share.SharerUserID,
                        EventType = "ShareDisabled",
                        RelatedEntityId = externalShareId.ToString(),
                        MessageText = $"Your shared file '{fileOrFolder}' has been disabled because the download limit ({share.DownloadLimit}) has been reached."
                    });
                }

                // Update share in database
                _externalShareRepository.Update(share);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error incrementing download count for share {externalShareId}: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Checks if an external share has reached its download limit.
        /// </summary>
        public bool IsDownloadLimitReached(int externalShareId)
        {
            try
            {
                var share = _externalShareRepository.GetById(externalShareId);
                if (share == null)
                    return false;

                if (!share.DownloadLimit.HasValue)
                    return false;

                return share.CurrentDownloadCount >= share.DownloadLimit.Value;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error checking download limit for share {externalShareId}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Processes all expired shares and transitions them to Expired status.
        /// Called by background job or scheduled task.
        /// Validates: Requirements 4.5, 12.1
        /// </summary>
        public void ProcessExpiredShares()
        {
            try
            {
                _loggingService.Info("Starting ProcessExpiredShares...");

                // Get all Active shares
                var activeShares = _externalShareRepository.GetByStatus("Active").ToList();

                int processedCount = 0;
                foreach (var share in activeShares)
                {
                    // Check if expiry date has passed
                    if (share.ExpiryDate.HasValue && DateTime.Now > share.ExpiryDate.Value)
                    {
                        share.ShareTokenStatus = "Expired";
                        _externalShareRepository.Update(share);

                        _loggingService.Info($"Share {share.ExternalShareID} transitioned to Expired (ExpiryDate: {share.ExpiryDate})");

                        // Log audit entry
                        _auditService.Log(new AuditEntry
                        {
                            Module = "FileSharing",
                            Action = "ShareExpired",
                            UserID = 0,
                            UserName = "System",
                            TableName = "ExternalShare",
                            RecordID = share.ExternalShareID,
                            NewValue = $"Status=Expired, Reason=ExpiryDatePassed, ExpiryDate={share.ExpiryDate}",
                            Result = 1
                        });

                        // Send notification to sharer (Requirement 11.1, 11.2)
                        var fileOrFolder = share.FileID.HasValue
                            ? _fileDocumentRepository.GetById(share.FileID.Value)?.OriginalFileName
                            : _folderRepository.GetById(share.FolderID.Value)?.FolderName;

                        _notificationService.Send(new NotificationRequest
                        {
                            RecipientUserId = share.SharerUserID,
                            EventType = "ShareExpired",
                            RelatedEntityId = share.ExternalShareID.ToString(),
                            MessageText = $"Your shared file '{fileOrFolder}' has expired (Expiry Date: {share.ExpiryDate:yyyy-MM-dd HH:mm:ss})."
                        });

                        processedCount++;
                    }
                }

                _loggingService.Info($"ProcessExpiredShares completed: {processedCount} shares expired.");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error processing expired shares: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Processes all disabled shares and performs cleanup.
        /// Called by background job or scheduled task.
        /// Validates: Requirements 12.2
        /// </summary>
        public void ProcessDisabledShares()
        {
            try
            {
                _loggingService.Info("Starting ProcessDisabledShares...");

                // Get all Disabled shares
                var disabledShares = _externalShareRepository.GetByStatus("Disabled").ToList();

                _loggingService.Info($"ProcessDisabledShares found {disabledShares.Count} disabled shares.");

                // For now, just log them. In a real system, you might perform cleanup operations.
                // This could include:
                // - Archiving access logs
                // - Cleaning up temporary files
                // - Sending final notifications
                // - etc.

                foreach (var share in disabledShares)
                {
                    _loggingService.Info($"Disabled share: {share.ExternalShareID}, DisabledDate: {share.DisabledDate}, Reason: {share.DisabledReason}");
                }

                _loggingService.Info("ProcessDisabledShares completed.");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error processing disabled shares: {ex.Message}", ex);
            }
        }
    }
}
