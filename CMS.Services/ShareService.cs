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
        private readonly IApprovalRepository _approvalRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IApproverResolver _approverResolver;

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
            ILoggingService loggingService,
            IApprovalRepository approvalRepository = null,
            IUserAccountRepository userAccountRepository = null,
            IApproverResolver approverResolver = null)
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
            _approvalRepository = approvalRepository;
            _userAccountRepository = userAccountRepository;
            _approverResolver = approverResolver;
        }

        // =====================================================================
        // Internal Sharing Methods
        // =====================================================================

        public void RequestFolderShareApproval(SetPermissionRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");

            if (_approvalRepository == null || _userAccountRepository == null)
                throw new InvalidOperationException("ระบบขออนุมัติไม่พร้อมใช้งาน กรุณาติดต่อผู้ดูแลระบบ");

            var folder = _folderRepository.GetById(request.FolderID);
            if (folder == null)
                throw new InvalidOperationException(string.Format("ไม่พบ Folder ID {0}", request.FolderID));

            // Remark format must match ParsePermissionRemark in ApprovalService
            string remark = string.Format(
                "FolderID:{0} PrincipalType:{1} PrincipalID:{2} Read:{3} Write:{4} Delete:{5} Modify:{6} FullControl:{7} IsDeny:{8}",
                request.FolderID, (int)request.PrincipalType, request.PrincipalID,
                request.CanRead, request.CanWrite, request.CanDelete,
                request.CanModify, request.CanFullControl, request.IsDeny);

            var approvalReq = new Domain.Models.ApprovalRequest
            {
                RequestType   = "CHANGE_PERMISSION",
                FolderID      = request.FolderID,
                RequestedBy   = user.UserID,
                RequestedDate = DateTime.Now,
                RequestStatus = 1, // Pending
                Subject       = string.Format("ขออนุมัติแชร์โฟลเดอร์ภายใน: {0}", folder.FolderName),
                Remark        = remark,
                ExpiryDate    = DateTime.Now.AddDays(PendingApprovalExpiryDays)
            };

            int approvalId = _approvalRepository.InsertRequest(approvalReq);

            var approvers = ResolveApprovers(request.FolderID, null, user.UserID);

            int stepOrder = 1;
            foreach (var approver in approvers)
            {
                _approvalRepository.InsertStep(new Domain.Models.ApprovalStep
                {
                    RequestID  = approvalId,
                    StepOrder  = stepOrder++,
                    ApproverID = approver.UserID,
                    StepStatus = 1 // Pending
                });
            }

            _auditService.Log(new AuditEntry
            {
                Module    = "FileSharing",
                Action    = "RequestFolderShare",
                UserID    = user.UserID,
                UserName  = user.ADUserName ?? string.Empty,
                TableName = "security.FolderPermission",
                RecordID  = request.FolderID,
                NewValue  = remark,
                Result    = 1,
                AuditDate = DateTime.Now
            });

            foreach (var approver in approvers)
            {
                _notificationService.Send(new NotificationRequest
                {
                    RecipientUserId = approver.UserID,
                    EventType       = "FolderShareApprovalRequested",
                    RelatedEntityId = approvalId.ToString(),
                    MessageText     = string.Format(
                        "{0} ขออนุมัติแชร์โฟลเดอร์ '{1}' ให้ผู้รับ (RequestID:{2})",
                        user.ADUserName, folder.FolderName, approvalId)
                });
            }
        }

        public FilePermission CreateInternalShare(CreateInternalShareRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user == null) throw new ArgumentNullException("user");

            // Validate user has access to the file's parent folder (Read OR Write is sufficient to share)
            var fileToShare = _fileDocumentRepository.GetById(request.FileID);
            if (fileToShare == null)
                throw new InvalidOperationException(string.Format("File with ID {0} not found.", request.FileID));
            bool hasAccess = _permissionService.CheckPermission(user.UserID, fileToShare.FolderID, "Read")
                          || _permissionService.CheckPermission(user.UserID, fileToShare.FolderID, "Write");

            // File uploader always retains the right to share their own file
            if (!hasAccess && fileToShare.UploadedBy == user.UserID)
                hasAccess = true;

            // Fallback: user may have been granted direct file-level access via a prior share
            if (!hasAccess)
            {
                var filePerms = _filePermissionRepository.GetByFile(request.FileID);
                hasAccess = filePerms.Any(fp =>
                    fp.PrincipalType == 1 &&
                    fp.PrincipalID   == user.UserID &&
                    !fp.IsDeny &&
                    (fp.CanRead || fp.CanWrite) &&
                    (!fp.ExpiryDate.HasValue || fp.ExpiryDate.Value.Date >= DateTime.Today));
            }

            // 4PC (Public Confidential, level 4) is the open tier — no explicit ACL entries needed.
            // ConfidentialityLevel on FileDocument is transient; read it from the folder instead.
            if (!hasAccess)
            {
                var parentFolder = _folderRepository.GetById(fileToShare.FolderID);
                if (parentFolder != null && parentFolder.ConfidentialityLevel == 4)
                    hasAccess = true;
            }

            if (!hasAccess)
                throw new UnauthorizedAccessException("ไม่มีสิทธิ์เพียงพอในการแชร์ไฟล์นี้");

            // Validate expiry date
            if (request.ExpiryDate.HasValue)
            {
                if (request.ExpiryDate.Value.Date < DateTime.Today)
                    throw new InvalidOperationException("ไม่สามารถกำหนดวันหมดอายุย้อนหลังได้");
                if (request.ExpiryDate.Value.Date > DateTime.Today.AddYears(1))
                    throw new InvalidOperationException("ไม่สามารถกำหนดอายุเกิน 1 ปีได้");
            }

            // Department shares (PrincipalType == 3) always require Manager approval because
            // they grant visibility to an entire org unit — the blast radius is too large to bypass.
            // Officer/SrOfficer (AppRole < 3) always need approval regardless of principal type.
            bool requiresApproval = request.PrincipalType == 3 || user.AppRole < 3;
            if (requiresApproval && _approvalRepository != null && _userAccountRepository != null)
            {
                SubmitInternalShareForApproval(request, user, fileToShare);
                return null;
            }

            // Manager/Admin sharing with a specific User, Role, or Group: create permission immediately
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

            // Only notify when PrincipalType == 1 (User): PrincipalID is a valid UserID.
            // For Role/Group the recipient is not a single user; skip individual notification.
            if (request.PrincipalType == 1)
            {
                var file = _fileDocumentRepository.GetById(request.FileID);
                _notificationService.Send(new NotificationRequest
                {
                    RecipientUserId = request.PrincipalID,
                    EventType = "InternalShareCreated",
                    RelatedEntityId = permissionId.ToString(),
                    MessageText = string.Format("File '{0}' has been shared with you by {1}. Access level: Read={2}, Write={3}, Delete={4}",
                        file?.OriginalFileName ?? "Unknown", user.ADUserName, request.CanRead, request.CanWrite, request.CanDelete)
                });
            }

            return filePermission;
        }

        private void SubmitInternalShareForApproval(CreateInternalShareRequest request, UserContext user, FileDocument file)
        {
            string expiryStr = request.ExpiryDate.HasValue
                ? request.ExpiryDate.Value.ToString("yyyy-MM-dd")
                : "";
            string remark = string.Format(
                "InternalShareFile FileID:{0} PrincipalType:{1} PrincipalID:{2} Read:{3} Write:{4} Delete:{5} Expiry:{6}",
                request.FileID, request.PrincipalType, request.PrincipalID,
                request.CanRead, request.CanWrite, request.CanDelete, expiryStr);

            var approvalReq = new Domain.Models.ApprovalRequest
            {
                RequestType   = "INTERNAL_SHARE",
                FileID        = request.FileID,
                RequestedBy   = user.UserID,
                RequestedDate = DateTime.Now,
                RequestStatus = 1, // Pending
                Subject       = string.Format("ขออนุมัติแชร์ไฟล์ภายใน: {0}",
                    file.OriginalFileName ?? "File #" + request.FileID),
                Remark        = remark,
                ExpiryDate    = DateTime.Now.AddDays(PendingApprovalExpiryDays)
            };

            int approvalId;
            try
            {
                approvalId = _approvalRepository.InsertRequest(approvalReq);
            }
            catch (Exception ex)
            {
                bool isTypeConstraint = ex.Message != null &&
                    ex.Message.IndexOf("CK_wf_ApprovalRequest_Type", StringComparison.OrdinalIgnoreCase) >= 0;
                if (!isTypeConstraint) throw;
                approvalReq.RequestType = "CHANGE_PERMISSION";
                approvalId = _approvalRepository.InsertRequest(approvalReq);
            }

            var approvers = ResolveApprovers(file.FolderID, null, user.UserID);

            int stepOrder = 1;
            foreach (var approver in approvers)
            {
                _approvalRepository.InsertStep(new Domain.Models.ApprovalStep
                {
                    RequestID  = approvalId,
                    StepOrder  = stepOrder++,
                    ApproverID = approver.UserID,
                    StepStatus = 1 // Pending
                });
            }

            _auditService.Log(new AuditEntry
            {
                Module    = "FileSharing",
                Action    = "RequestInternalShare",
                UserID    = user.UserID,
                UserName  = user.ADUserName ?? string.Empty,
                TableName = "storage.FilePermission",
                RecordID  = request.FileID,
                NewValue  = remark,
                Result    = 1,
                AuditDate = DateTime.Now
            });

            foreach (var approver in approvers)
            {
                _notificationService.Send(new NotificationRequest
                {
                    RecipientUserId = approver.UserID,
                    EventType       = "InternalShareApprovalRequested",
                    RelatedEntityId = approvalId.ToString(),
                    MessageText     = string.Format(
                        "{0} ขออนุมัติแชร์ไฟล์ '{1}' ให้ผู้รับ (RequestID:{2})",
                        user.ADUserName, file.OriginalFileName ?? "File #" + request.FileID, approvalId)
                });
            }
        }

        private const string InternalShareRemarkPrefix = "InternalShareFile";

        /// <summary>
        /// Reads back the Remark written by SubmitInternalShareForApproval
        /// ("InternalShareFile FileID:1 PrincipalType:1 PrincipalID:2 Read:True Write:False Delete:False Expiry:2026-11-14").
        /// Returns null when the remark is not an internal-share remark.
        /// </summary>
        private static InternalShareRequestItem ParseInternalShareRemark(string remark)
        {
            if (remark == null || !remark.StartsWith(InternalShareRemarkPrefix, StringComparison.Ordinal))
                return null;

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var token in remark.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int colon = token.IndexOf(':');
                if (colon > 0)
                    values[token.Substring(0, colon)] = token.Substring(colon + 1);
            }

            var item = new InternalShareRequestItem();
            int number;
            bool flag;
            string value;
            if (values.TryGetValue("FileID", out value) && int.TryParse(value, out number)) item.FileID = number;
            if (values.TryGetValue("PrincipalType", out value) && int.TryParse(value, out number)) item.PrincipalType = number;
            if (values.TryGetValue("PrincipalID", out value) && int.TryParse(value, out number)) item.PrincipalID = number;
            if (values.TryGetValue("Read", out value) && bool.TryParse(value, out flag)) item.CanRead = flag;
            if (values.TryGetValue("Write", out value) && bool.TryParse(value, out flag)) item.CanWrite = flag;
            if (values.TryGetValue("Delete", out value) && bool.TryParse(value, out flag)) item.CanDelete = flag;

            DateTime expiry;
            if (values.TryGetValue("Expiry", out value)
                && DateTime.TryParseExact(value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out expiry))
                item.ShareExpiry = expiry;

            return item;
        }

        /// <summary>
        /// Approvers for a share request: the folder's department approvers via IApproverResolver,
        /// or the legacy Manager → Admin lookup when no resolver is wired.
        /// </summary>
        private List<UserAccount> ResolveApprovers(int? folderId, int? fileId, int requesterId)
        {
            if (_approverResolver != null)
            {
                if (folderId.HasValue)
                    return _approverResolver.ResolveForFolder(folderId.Value, requesterId).ToList();
                if (fileId.HasValue)
                    return _approverResolver.ResolveForFile(fileId.Value, requesterId).ToList();
            }

            if (_userAccountRepository == null)
                return new List<UserAccount>();

            var approvers = _userAccountRepository.GetByRole(3) // Manager
                .Where(a => a.IsActive && a.UserID != requesterId)
                .ToList();
            if (!approvers.Any())
                approvers = _userAccountRepository.GetByRole(4) // Admin fallback
                    .Where(a => a.IsActive).ToList();
            return approvers;
        }

        /// <summary>
        /// Approve/reject of a pending external share follows the same approver set that
        /// CreateExternalShare used for the request (ResolveApprovers). Administrators may also
        /// decide; nobody may decide on their own request.
        /// </summary>
        private void EnsureCanDecideExternalShare(ExternalShare share, UserContext user)
        {
            if (share.SharerUserID == user.UserID)
                throw new UnauthorizedAccessException("ไม่สามารถอนุมัติหรือปฏิเสธคำขอแชร์ของตนเองได้");

            if (user.AppRole >= 4)
                return;

            var approvers = ResolveApprovers(share.FolderID, share.FileID, share.SharerUserID);
            if (!approvers.Any(a => a.UserID == user.UserID))
                throw new UnauthorizedAccessException("คุณไม่ได้เป็นผู้อนุมัติของคำขอแชร์นี้");
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

            // Create ApprovalRequest so managers can approve in Approval Inbox
            if (_approvalRepository != null && _userAccountRepository != null)
            {
                var approvers = ResolveApprovers(request.FolderID, request.FileID, user.UserID);

                if (approvers.Any())
                {
                    var approvalRequest = new Domain.Models.ApprovalRequest
                    {
                        RequestType   = "EXTERNAL_SHARE",
                        FileID        = request.FileID,
                        FolderID      = request.FolderID,
                        RequestedBy   = user.UserID,
                        RequestedDate = DateTime.Now,
                        RequestStatus = 1, // Pending
                        Subject       = string.Format("ขออนุมัติแชร์ภายนอก: {0}", targetName),
                        Remark        = string.Format("ExternalShareID:{0}", shareId),
                        ExpiryDate    = DateTime.Now.AddDays(PendingApprovalExpiryDays)
                    };
                    int approvalId;
                    try
                    {
                        approvalId = _approvalRepository.InsertRequest(approvalRequest);
                    }
                    catch (Exception ex)
                    {
                        // Backward compatibility: old DB constraint may not include EXTERNAL_SHARE.
                        bool isRequestTypeConstraint = ex.Message != null &&
                            ex.Message.IndexOf("CK_wf_ApprovalRequest_Type", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (!isRequestTypeConstraint)
                            throw;

                        _loggingService.Warn(
                            "ShareService.CreateExternalShare: Falling back to RequestType=CHANGE_PERMISSION " +
                            "because CK_wf_ApprovalRequest_Type does not allow EXTERNAL_SHARE.");

                        approvalRequest.RequestType = "CHANGE_PERMISSION";
                        approvalId = _approvalRepository.InsertRequest(approvalRequest);
                    }

                    int stepOrder = 1;
                    foreach (var approver in approvers)
                    {
                        _approvalRepository.InsertStep(new Domain.Models.ApprovalStep
                        {
                            RequestID  = approvalId,
                            StepOrder  = stepOrder++,
                            ApproverID = approver.UserID,
                            StepStatus = 1 // Pending
                        });
                    }
                }
            }

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

            EnsureCanDecideExternalShare(share, user);

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

            EnsureCanDecideExternalShare(share, user);

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

        public void ToggleExternalShare(int externalShareId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var share = _externalShareRepository.GetById(externalShareId);
            if (share == null)
                throw new InvalidOperationException("External share not found.");

            if (share.SharerUserID != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("Only the original Sharer or an Administrator can toggle this share.");

            if (share.ShareTokenStatus != "Active" && share.ShareTokenStatus != "Disabled")
                throw new InvalidOperationException(
                    string.Format("Cannot toggle a share with status '{0}'.", share.ShareTokenStatus));

            bool isDisabling = share.ShareTokenStatus == "Active";
            share.ShareTokenStatus = isDisabling ? "Disabled" : "Active";

            if (isDisabling)
            {
                share.DisabledDate  = DateTime.Now;
                share.DisabledReason = "Disabled by owner";
            }
            else
            {
                share.DisabledDate   = null;
                share.DisabledReason = null;
            }

            _externalShareRepository.Update(share);

            _auditService.Log(new AuditEntry
            {
                Module     = "FileSharing",
                Action     = isDisabling ? "DisableExternalShare" : "EnableExternalShare",
                UserID     = user.UserID,
                UserName   = user.ADUserName ?? string.Empty,
                TableName  = "storage.ExternalShare",
                RecordID   = externalShareId,
                NewValue   = string.Format("Status={0}", share.ShareTokenStatus),
                Result     = 1,
                AuditDate  = DateTime.Now
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

        public IEnumerable<InternalShareRequestItem> GetInternalShareRequestsForUser(int userId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Validate user is querying their own requests or is Admin
            if (userId != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("You can only view your own shares.");

            var results = new List<InternalShareRequestItem>();
            if (_approvalRepository == null)
                return results;

            // CHANGE_PERMISSION is the fallback type used when the DB constraint lacked INTERNAL_SHARE;
            // only those carrying an internal-share remark belong here (folder shares use another remark).
            var requests = _approvalRepository.GetRequestsByRequester(userId, "INTERNAL_SHARE")
                .Concat(_approvalRepository.GetRequestsByRequester(userId, "CHANGE_PERMISSION"));

            foreach (var req in requests)
            {
                // Approved requests already show up as FilePermission rows
                if (req.RequestStatus == 2) continue;

                var item = ParseInternalShareRemark(req.Remark);
                if (item == null) continue;

                if (item.FileID == 0 && req.FileID.HasValue) item.FileID = req.FileID.Value;
                item.RequestID     = req.RequestID;
                item.RequestedDate = req.RequestedDate;
                item.RequestExpiry = req.ExpiryDate;
                item.RequestStatus = req.RequestStatus;
                item.ResolvedDate  = req.ResolvedDate;
                item.ResolveRemark = req.ResolveRemark;
                item.IsWithdrawn   = req.RequestStatus == 4 && req.ResolveRemark != null
                    && req.ResolveRemark.StartsWith("Withdrawn", StringComparison.OrdinalIgnoreCase);
                results.Add(item);
            }

            return results.OrderByDescending(r => r.RequestedDate).ToList();
        }

        public IEnumerable<ExternalShare> GetExternalSharesForUser(int userId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Validate user is querying their own shares or is Admin
            if (userId != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("You can only view your own shares.");

            return _externalShareRepository.GetBySharer(userId);
        }

        public IEnumerable<FilePermission> GetAllInternalShares(UserContext admin)
        {
            if (admin == null) throw new ArgumentNullException("admin");
            if (admin.AppRole < 4)
                throw new UnauthorizedAccessException("เฉพาะ Admin เท่านั้นที่สามารถดูรายการ share ทั้งหมดได้");

            return _filePermissionRepository.GetAll();
        }

        public IEnumerable<ExternalShare> GetAllExternalShares(UserContext admin)
        {
            if (admin == null) throw new ArgumentNullException("admin");
            if (admin.AppRole < 4)
                throw new UnauthorizedAccessException("เฉพาะ Admin เท่านั้นที่สามารถดูรายการ share ทั้งหมดได้");

            return _externalShareRepository.GetAll();
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
