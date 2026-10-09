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

            string userRemarkToken = !string.IsNullOrWhiteSpace(request.Remark)
                ? " UserRemark:" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Remark.Trim()))
                : "";

            // Remark format must match ParsePermissionRemark in ApprovalService
            string remark = string.Format(
                "FolderID:{0} PrincipalType:{1} PrincipalID:{2} Read:{3} Write:{4} Delete:{5} Modify:{6} FullControl:{7} IsDeny:{8}{9}",
                request.FolderID, (int)request.PrincipalType, request.PrincipalID,
                request.CanRead, request.CanWrite, request.CanDelete,
                request.CanModify, request.CanFullControl, request.IsDeny, userRemarkToken);

            var approvalReq = new Domain.Models.ApprovalRequest
            {
                RequestType   = "CHANGE_PERMISSION",
                FolderID      = request.FolderID,
                RequestedBy   = user.UserID,
                RequestedDate = DateTime.Now,
                RequestStatus = 1, // Pending
                Subject       = string.Format("{0}: {1}", FolderShareSubjectPrefix, folder.FolderName),
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

            var fileToShare = EnsureCanShareFile(request.FileID, user);

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

        private FileDocument EnsureCanShareFile(int fileId, UserContext user)
        {
            var fileToShare = _fileDocumentRepository.GetById(fileId);
            if (fileToShare == null)
                throw new InvalidOperationException(string.Format("File with ID {0} not found.", fileId));

            bool hasAccess = _permissionService.CheckPermission(user.UserID, fileToShare.FolderID, "Read")
                          || _permissionService.CheckPermission(user.UserID, fileToShare.FolderID, "Write");

            if (!hasAccess && fileToShare.UploadedBy == user.UserID)
                hasAccess = true;

            if (!hasAccess)
            {
                var filePerms = _filePermissionRepository.GetByFile(fileId);
                hasAccess = filePerms.Any(fp =>
                    fp.PrincipalType == 1 &&
                    fp.PrincipalID   == user.UserID &&
                    !fp.IsDeny &&
                    (fp.CanRead || fp.CanWrite) &&
                    (!fp.ExpiryDate.HasValue || fp.ExpiryDate.Value.Date >= DateTime.Today));
            }

            if (!hasAccess)
            {
                var parentFolder = _folderRepository.GetById(fileToShare.FolderID);
                if (parentFolder != null && parentFolder.ConfidentialityLevel == 4)
                    hasAccess = true;
            }

            if (!hasAccess)
                throw new UnauthorizedAccessException(string.Format("ไม่มีสิทธิ์เพียงพอในการแชร์ไฟล์ {0}", fileToShare.OriginalFileName));

            return fileToShare;
        }

        public InternalShareBatchResult CreateInternalShareBatch(CreateInternalShareBatchRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user == null) throw new ArgumentNullException("user");

            if (request.Recipients == null || request.Recipients.Count == 0)
                throw new ArgumentException("ต้องระบุผู้รับสิทธิ์อย่างน้อย 1 รายการ", "request");

            var result = new InternalShareBatchResult();

            // Validate recipients
            foreach (var r in request.Recipients)
            {
                if (r.ExpiryDate.HasValue)
                {
                    if (r.ExpiryDate.Value.Date < DateTime.Today)
                        throw new InvalidOperationException(string.Format("ผู้รับ '{0}': ไม่สามารถกำหนดวันหมดอายุย้อนหลังได้", r.PrincipalName ?? r.PrincipalID.ToString()));
                    if (r.ExpiryDate.Value.Date > DateTime.Today.AddYears(1))
                        throw new InvalidOperationException(string.Format("ผู้รับ '{0}': ไม่สามารถกำหนดอายุเกิน 1 ปีได้", r.PrincipalName ?? r.PrincipalID.ToString()));
                }
            }

            bool isFolder = request.FolderID.HasValue && (request.FileIDs == null || request.FileIDs.Count == 0);

            // Separate recipients into immediate vs approval
            var immediateRecipients = new List<ShareRecipientItem>();
            var approvalRecipients  = new List<ShareRecipientItem>();

            foreach (var r in request.Recipients)
            {
                bool requiresApproval = r.PrincipalType == 3 || user.AppRole < 3;
                if (requiresApproval)
                    approvalRecipients.Add(r);
                else
                    immediateRecipients.Add(r);
            }

            // 1. Process immediate recipients
            if (immediateRecipients.Count > 0)
            {
                if (isFolder)
                {
                    int folderId = request.FolderID.Value;
                    foreach (var r in immediateRecipients)
                    {
                        var permReq = new SetPermissionRequest
                        {
                            FolderID       = folderId,
                            PrincipalType  = (CMS.Domain.Enums.PrincipalType)r.PrincipalType,
                            PrincipalID    = r.PrincipalID,
                            CanRead        = r.CanRead,
                            CanWrite       = r.CanWrite,
                            CanDelete      = r.CanDelete,
                            CanModify      = false,
                            CanFullControl = false,
                            IsDeny         = false,
                            Remark         = request.Remark
                        };
                        _permissionService.SetFolderPermission(permReq, user);
                        result.ImmediateGrantedCount++;
                    }
                }
                else
                {
                    foreach (int fileId in request.FileIDs)
                    {
                        var fileToShare = EnsureCanShareFile(fileId, user);
                        foreach (var r in immediateRecipients)
                        {
                            var fp = new FilePermission
                            {
                                FileID         = fileId,
                                PrincipalType  = r.PrincipalType,
                                PrincipalID    = r.PrincipalID,
                                CanRead        = r.CanRead,
                                CanWrite       = r.CanWrite,
                                CanDelete      = r.CanDelete,
                                CanDownload    = r.CanDownload,
                                CanModify      = false,
                                CanFullControl = false,
                                IsDeny         = false,
                                ExpiryDate     = r.ExpiryDate,
                                GrantedBy      = user.UserID,
                                GrantedDate    = DateTime.Now
                            };

                            int permissionId = _filePermissionRepository.Insert(fp);
                            fp.FilePermissionID = permissionId;
                            result.ImmediateGrantedCount++;

                            _auditService.Log(new AuditEntry
                            {
                                Module    = "FileSharing",
                                Action    = "CreateInternalShare",
                                UserID    = user.UserID,
                                UserName  = user.ADUserName ?? string.Empty,
                                TableName = "storage.FilePermission",
                                RecordID  = permissionId,
                                NewValue  = string.Format("PrincipalType={0}, PrincipalID={1}, Permissions=Read:{2},Write:{3},Delete:{4},Download:{5}",
                                    r.PrincipalType, r.PrincipalID, r.CanRead, r.CanWrite, r.CanDelete, r.CanDownload),
                                Result    = 1,
                                AuditDate = DateTime.Now
                            });

                            if (r.PrincipalType == 1)
                            {
                                _notificationService.Send(new NotificationRequest
                                {
                                    RecipientUserId = r.PrincipalID,
                                    EventType       = "InternalShareCreated",
                                    RelatedEntityId = permissionId.ToString(),
                                    MessageText     = string.Format("File '{0}' has been shared with you by {1}. Access level: Read={2}, Write={3}, Delete={4}, Download={5}",
                                        fileToShare.OriginalFileName, user.ADUserName, r.CanRead, r.CanWrite, r.CanDelete, r.CanDownload)
                                });
                            }
                        }
                    }
                }
            }

            // 2. Process approval recipients (Group into 1 ApprovalRequest and insert ApprovalRequestItem rows)
            if (approvalRecipients.Count > 0 && _approvalRepository != null)
            {
                int? primaryFolderId = request.FolderID;
                int? primaryFileId = (request.FileIDs != null && request.FileIDs.Count == 1) ? (int?)request.FileIDs[0] : null;

                if (!primaryFolderId.HasValue && request.FileIDs != null && request.FileIDs.Count > 0)
                {
                    var firstFile = _fileDocumentRepository.GetById(request.FileIDs[0]);
                    if (firstFile != null) primaryFolderId = firstFile.FolderID;
                }

                IList<UserAccount> approvers = null;
                if (_approverResolver != null && primaryFileId.HasValue)
                {
                    approvers = _approverResolver.GetConfiguredShareApprovers(primaryFileId.Value, user);
                }
                if (approvers == null || approvers.Count == 0)
                {
                    approvers = ResolveApprovers(primaryFolderId, primaryFileId, user.UserID);
                }
                if (approvers == null || approvers.Count == 0)
                {
                    var fallbackAdmin = _userAccountRepository.GetAll()
                        .FirstOrDefault(u => u.AppRole == 4 && u.IsActive);
                    if (fallbackAdmin != null)
                        approvers = new List<UserAccount> { fallbackAdmin };
                }

                if (approvers != null && approvers.Count > 0)
                {
                    string targetName;
                    if (isFolder)
                    {
                        var folder = _folderRepository.GetById(request.FolderID.Value);
                        targetName = folder?.FolderName ?? ("Folder #" + request.FolderID.Value);
                    }
                    else
                    {
                        if (request.FileIDs.Count == 1)
                        {
                            var f = _fileDocumentRepository.GetById(request.FileIDs[0]);
                            targetName = f?.OriginalFileName ?? ("File #" + request.FileIDs[0]);
                        }
                        else
                        {
                            targetName = string.Format("{0} ไฟล์", request.FileIDs.Count);
                        }
                    }

                    int totalItems = isFolder ? approvalRecipients.Count : (request.FileIDs.Count * approvalRecipients.Count);
                    string subject = string.Format("ขออนุมัติแชร์ภายใน: {0} ให้ {1} ผู้รับ", targetName, approvalRecipients.Count);

                    string userRemarkToken = !string.IsNullOrWhiteSpace(request.Remark)
                        ? " UserRemark:" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Remark.Trim()))
                        : "";

                    string remark = string.Format("ShareBatch Items:{0}{1}", totalItems, userRemarkToken);

                    var approvalReq = new ApprovalRequest
                    {
                        RequestType   = "INTERNAL_SHARE",
                        FolderID      = primaryFolderId,
                        FileID        = primaryFileId,
                        RequestedBy   = user.UserID,
                        RequestedDate = DateTime.Now,
                        RequestStatus = 1, // Pending
                        Subject       = subject,
                        Remark        = remark,
                        ExpiryDate    = DateTime.Now.AddDays(PendingApprovalExpiryDays)
                    };

                    int requestId;
                    try
                    {
                        requestId = _approvalRepository.InsertRequest(approvalReq);
                    }
                    catch (Exception ex)
                    {
                        bool isTypeConstraint = ex.Message != null &&
                            ex.Message.IndexOf("CK_wf_ApprovalRequest_Type", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (isTypeConstraint)
                        {
                            approvalReq.RequestType = "CHANGE_PERMISSION";
                            requestId = _approvalRepository.InsertRequest(approvalReq);
                        }
                        else
                        {
                            throw;
                        }
                    }

                    approvalReq.RequestID = requestId;
                    result.ApprovalRequestIDs.Add(requestId);

                    // Insert ApprovalSteps
                    int stepOrder = 1;
                    foreach (var approver in approvers)
                    {
                        _approvalRepository.InsertStep(new ApprovalStep
                        {
                            RequestID  = requestId,
                            StepOrder  = stepOrder++,
                            ApproverID = approver.UserID,
                            StepStatus = 1
                        });
                    }

                    // Insert ApprovalRequestItems
                    if (isFolder)
                    {
                        foreach (var r in approvalRecipients)
                        {
                            var item = new ApprovalRequestItem
                            {
                                RequestID     = requestId,
                                FolderID      = request.FolderID,
                                PrincipalType = r.PrincipalType,
                                PrincipalID   = r.PrincipalID,
                                CanRead       = r.CanRead,
                                CanWrite      = r.CanWrite,
                                CanDelete     = r.CanDelete,
                                CanDownload   = false,
                                ExpiryDate    = r.ExpiryDate
                            };
                            _approvalRepository.InsertItem(item);
                        }
                    }
                    else
                    {
                        foreach (int fileId in request.FileIDs)
                        {
                            EnsureCanShareFile(fileId, user);
                            foreach (var r in approvalRecipients)
                            {
                                var item = new ApprovalRequestItem
                                {
                                    RequestID     = requestId,
                                    FileID        = fileId,
                                    PrincipalType = r.PrincipalType,
                                    PrincipalID   = r.PrincipalID,
                                    CanRead       = r.CanRead,
                                    CanWrite      = r.CanWrite,
                                    CanDelete     = r.CanDelete,
                                    CanDownload   = r.CanDownload,
                                    ExpiryDate    = r.ExpiryDate
                                };
                                _approvalRepository.InsertItem(item);
                            }
                        }
                    }

                    // Audit & notify approver once
                    string itemSummary = string.Join(", ", approvalRecipients.Select(r => r.PrincipalName ?? (r.PrincipalType + ":" + r.PrincipalID)));
                    _auditService.Log(new AuditEntry
                    {
                        Module    = "FileSharing",
                        Action    = "RequestInternalShareBatch",
                        UserID    = user.UserID,
                        UserName  = user.ADUserName ?? string.Empty,
                        TableName = "workflow.ApprovalRequest",
                        RecordID  = requestId,
                        NewValue  = string.Format("Target={0}, Recipients={1}, TotalItems={2}", targetName, itemSummary, totalItems),
                        Result    = 1,
                        AuditDate = DateTime.Now
                    });

                    _notificationService.Send(new NotificationRequest
                    {
                        RecipientUserId = approvers[0].UserID,
                        EventType       = "ApprovalRequestPending",
                        RelatedEntityId = requestId.ToString(),
                        MessageText     = string.Format("มีคำขออนุมัติแชร์ภายใน: {0} ({1} ผู้รับ) รอการตรวจสอบ", targetName, approvalRecipients.Count)
                    });
                }
            }

            return result;
        }

        private void SubmitInternalShareForApproval(CreateInternalShareRequest request, UserContext user, FileDocument file)
        {
            string expiryStr = request.ExpiryDate.HasValue
                ? request.ExpiryDate.Value.ToString("yyyy-MM-dd")
                : "";

            string userRemarkToken = !string.IsNullOrWhiteSpace(request.Remark)
                ? " UserRemark:" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Remark.Trim()))
                : "";

            string remark = string.Format(
                "InternalShareFile FileID:{0} PrincipalType:{1} PrincipalID:{2} Read:{3} Write:{4} Delete:{5} Expiry:{6}{7}",
                request.FileID, request.PrincipalType, request.PrincipalID,
                request.CanRead, request.CanWrite, request.CanDelete, expiryStr, userRemarkToken);

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

            IList<UserAccount> approvers = null;
            if (_approverResolver != null)
            {
                approvers = _approverResolver.GetConfiguredShareApprovers(request.FileID, user);
            }
            if (approvers == null || approvers.Count == 0)
            {
                approvers = ResolveApprovers(file.FolderID, null, user.UserID);
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
        private const string FolderShareRemarkPrefix   = "FolderID:";
        private const string FolderShareSubjectPrefix  = "ขออนุมัติแชร์โฟลเดอร์ภายใน";

        /// <summary>
        /// Reads back a share request's Remark:
        /// file  — written by SubmitInternalShareForApproval
        ///         ("InternalShareFile FileID:1 PrincipalType:1 PrincipalID:2 Read:True Write:False Delete:False Expiry:2026-11-14");
        /// folder — written by RequestFolderShareApproval
        ///         ("FolderID:5 PrincipalType:1 PrincipalID:2 Read:True Write:False Delete:False Modify:False ...").
        /// Folder remarks share their format with ordinary permission-change requests, so a folder
        /// request is recognised only when its Subject also carries the folder-share prefix.
        /// Returns null for any other request.
        /// </summary>
        private static InternalShareRequestItem ParseShareRequestRemark(string remark, string subject)
        {
            if (remark == null)
                return null;

            bool isFile   = remark.StartsWith(InternalShareRemarkPrefix, StringComparison.Ordinal);
            bool isFolder = remark.StartsWith(FolderShareRemarkPrefix, StringComparison.Ordinal)
                         && subject != null && subject.StartsWith(FolderShareSubjectPrefix, StringComparison.Ordinal);
            if (!isFile && !isFolder)
                return null;

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var token in remark.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int colon = token.IndexOf(':');
                if (colon > 0)
                    values[token.Substring(0, colon)] = token.Substring(colon + 1);
            }

            var item = new InternalShareRequestItem { IsFolder = isFolder };
            int number;
            bool flag;
            string value;
            if (isFile && values.TryGetValue("FileID", out value) && int.TryParse(value, out number)) item.FileID = number;
            if (isFolder && values.TryGetValue("FolderID", out value) && int.TryParse(value, out number)) item.FolderID = number;
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

        public FilePermission GetInternalShareById(int filePermissionId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var permission = _filePermissionRepository.GetById(filePermissionId);
            if (permission == null)
                throw new KeyNotFoundException("ไม่พบข้อมูลสิทธิ์การแชร์");

            if (permission.GrantedBy != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์เข้าถึงข้อมูลการแชร์นี้");

            return permission;
        }

        public void UpdateInternalSharePermission(UpdateSharePermissionRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var permission = _filePermissionRepository.GetById(request.FilePermissionID);
            if (permission == null)
                throw new KeyNotFoundException("ไม่พบข้อมูลสิทธิ์การแชร์ที่ต้องการแก้ไข");

            // Validate user is Sharer or Admin
            if (permission.GrantedBy != user.UserID && user.AppRole < 4)
                throw new UnauthorizedAccessException("เฉพาะเจ้าของที่แชร์หรือผู้ดูแลระบบเท่านั้นที่สามารถแก้ไขสิทธิ์ได้");

            if (permission.RevokedDate.HasValue)
                throw new InvalidOperationException("ไม่สามารถแก้ไขสิทธิ์ของรายการที่ถูกยกเลิกไปแล้วได้");

            if (!request.CanRead && !request.CanWrite && !request.CanDelete && !request.CanDownload)
                throw new ArgumentException("ต้องกำหนดสิทธิ์อย่างน้อย 1 รายการ (หากต้องการเพิกถอนสิทธิ์ทั้งหมด ให้ใช้ปุ่มยกเลิกการแชร์)");

            permission.CanRead = request.CanRead;
            permission.CanWrite = request.CanWrite;
            permission.CanDelete = request.CanDelete;
            permission.CanDownload = request.CanDownload;
            permission.ExpiryDate = request.ExpiryDate;

            _filePermissionRepository.Update(permission);

            // Audit log
            _auditService.Log(new AuditEntry
            {
                Module = "FileSharing",
                Action = "UpdateInternalSharePermission",
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                TableName = "storage.FilePermission",
                RecordID = permission.FilePermissionID,
                NewValue = string.Format("Read:{0}, Write:{1}, Delete:{2}, Download:{3}, Expiry:{4:yyyy-MM-dd}",
                    permission.CanRead, permission.CanWrite, permission.CanDelete, permission.CanDownload, permission.ExpiryDate),
                Result = 1,
                AuditDate = DateTime.Now
            });

            // Notification to recipient
            if (permission.PrincipalType == 1)
            {
                var file = _fileDocumentRepository.GetById(permission.FileID);
                _notificationService.Send(new NotificationRequest
                {
                    RecipientUserId = permission.PrincipalID,
                    EventType = "InternalShareUpdated",
                    RelatedEntityId = permission.FilePermissionID.ToString(),
                    MessageText = string.Format("สิทธิ์การเข้าถึงไฟล์ '{0}' ของคุณได้รับการแก้ไขโดย {1}. สิทธิ์ปัจจุบัน: อ่าน={2}, เขียน={3}, ลบ={4}, ดาวน์โหลด={5}",
                        file?.OriginalFileName ?? "Unknown", user.ADUserName,
                        permission.CanRead, permission.CanWrite, permission.CanDelete, permission.CanDownload)
                });
            }
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

            // CHANGE_PERMISSION holds folder-share requests and, when the DB constraint lacked
            // INTERNAL_SHARE, file-share requests too; other permission-change requests are skipped
            // by ParseShareRequestRemark.
            var requests = _approvalRepository.GetRequestsByRequester(userId, "INTERNAL_SHARE")
                .Concat(_approvalRepository.GetRequestsByRequester(userId, "CHANGE_PERMISSION"));

            foreach (var req in requests)
            {
                // Approved requests have become real permissions
                if (req.RequestStatus == 2) continue;

                var dbItems = _approvalRepository.GetItemsByRequest(req.RequestID).ToList();
                if (dbItems.Count > 0)
                {
                    foreach (var dbItem in dbItems)
                    {
                        var item = new InternalShareRequestItem
                        {
                            RequestID     = req.RequestID,
                            IsFolder      = dbItem.FolderID.HasValue && dbItem.FolderID.Value > 0,
                            FileID        = dbItem.FileID ?? 0,
                            FolderID      = dbItem.FolderID ?? 0,
                            PrincipalType = dbItem.PrincipalType,
                            PrincipalID   = dbItem.PrincipalID,
                            CanRead       = dbItem.CanRead,
                            CanWrite      = dbItem.CanWrite,
                            CanDelete     = dbItem.CanDelete,
                            CanDownload   = dbItem.CanDownload,
                            ShareExpiry   = dbItem.ExpiryDate,
                            RequestedDate = req.RequestedDate,
                            RequestExpiry = req.ExpiryDate,
                            RequestStatus = req.RequestStatus,
                            ResolvedDate  = req.ResolvedDate,
                            ResolveRemark = req.ResolveRemark,
                            IsWithdrawn   = req.RequestStatus == 4 && req.ResolveRemark != null
                                && req.ResolveRemark.StartsWith("Withdrawn", StringComparison.OrdinalIgnoreCase)
                        };
                        results.Add(item);
                    }
                    continue;
                }

                var legacyItem = ParseShareRequestRemark(req.Remark, req.Subject);
                if (legacyItem == null) continue;

                if (!legacyItem.IsFolder && legacyItem.FileID == 0 && req.FileID.HasValue) legacyItem.FileID = req.FileID.Value;
                if (legacyItem.IsFolder && legacyItem.FolderID == 0 && req.FolderID.HasValue) legacyItem.FolderID = req.FolderID.Value;
                legacyItem.RequestID     = req.RequestID;
                legacyItem.RequestedDate = req.RequestedDate;
                legacyItem.RequestExpiry = req.ExpiryDate;
                legacyItem.RequestStatus = req.RequestStatus;
                legacyItem.ResolvedDate  = req.ResolvedDate;
                legacyItem.ResolveRemark = req.ResolveRemark;
                legacyItem.IsWithdrawn   = req.RequestStatus == 4 && req.ResolveRemark != null
                    && req.ResolveRemark.StartsWith("Withdrawn", StringComparison.OrdinalIgnoreCase);
                results.Add(legacyItem);
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
