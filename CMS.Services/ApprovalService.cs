using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Implements IApprovalService: manages the multi-step approval workflow.
    /// RequestStatus: 1=Pending, 2=Approved, 3=Rejected, 4=Expired
    /// StepStatus:    1=Pending, 2=Approved, 3=Rejected, 4=Delegated
    /// </summary>
    public class ApprovalService : IApprovalService
    {
        private readonly IApprovalRepository     _approvalRepository;
        private readonly IFolderRepository       _folderRepository;
        private readonly IFolderService          _folderService;
        private readonly IPermissionService      _permissionService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserAccountRepository  _userAccountRepository;
        private readonly IAuditService           _auditService;
        private readonly ILoggingService         _loggingService;
        private readonly IExternalShareRepository  _externalShareRepository;
        private readonly IShareTokenRepository     _shareTokenRepository;
        private readonly IFilePermissionRepository _filePermissionRepository;
        private readonly IApproverResolver         _approverResolver;

        public ApprovalService(
            IApprovalRepository     approvalRepository,
            IFolderRepository       folderRepository,
            IFolderService          folderService,
            IPermissionService      permissionService,
            INotificationRepository notificationRepository,
            IUserAccountRepository  userAccountRepository,
            IAuditService           auditService,
            ILoggingService         loggingService,
            IExternalShareRepository  externalShareRepository   = null,
            IShareTokenRepository     shareTokenRepository      = null,
            IFilePermissionRepository filePermissionRepository  = null,
            IApproverResolver         approverResolver          = null)
        {
            if (approvalRepository     == null) throw new ArgumentNullException("approvalRepository");
            if (folderRepository       == null) throw new ArgumentNullException("folderRepository");
            if (folderService          == null) throw new ArgumentNullException("folderService");
            if (permissionService      == null) throw new ArgumentNullException("permissionService");
            if (notificationRepository == null) throw new ArgumentNullException("notificationRepository");
            if (userAccountRepository  == null) throw new ArgumentNullException("userAccountRepository");
            if (auditService           == null) throw new ArgumentNullException("auditService");
            if (loggingService         == null) throw new ArgumentNullException("loggingService");

            _approvalRepository     = approvalRepository;
            _folderRepository       = folderRepository;
            _folderService          = folderService;
            _permissionService      = permissionService;
            _notificationRepository = notificationRepository;
            _userAccountRepository  = userAccountRepository;
            _auditService           = auditService;
            _loggingService         = loggingService;
            _externalShareRepository   = externalShareRepository;
            _shareTokenRepository      = shareTokenRepository;
            _filePermissionRepository  = filePermissionRepository;
            _approverResolver          = approverResolver;
        }

        // -----------------------------------------------------------------------
        // SubmitPermissionRequest
        // -----------------------------------------------------------------------

        public ApprovalRequest SubmitPermissionRequest(SubmitApprovalRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");

            // Manager=3, Admin=4
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("Submitting a permission request requires Manager or Admin role.");

            // Validate folder for HC/SC (ConfidentialityLevel 1 or 2)
            if (request.FolderID.HasValue)
            {
                var folder = _folderRepository.GetById(request.FolderID.Value);
                if (folder == null)
                    throw new InvalidOperationException(string.Format(
                        "Folder with ID {0} was not found.", request.FolderID.Value));

                if (folder.ConfidentialityLevel > 2)
                    throw new InvalidOperationException(string.Format(
                        "Approval workflow is only required for HC (1) and SC (2) folders. " +
                        "Folder {0} has confidentiality level {1}.",
                        request.FolderID.Value, folder.ConfidentialityLevel));
            }

            if (string.IsNullOrEmpty(request.Subject))
                throw new ArgumentException("Subject must not be empty.", "request");

            if (request.Steps == null || request.Steps.Count == 0)
                throw new ArgumentException("At least one approval step must be defined.", "request");

            var approvalRequest = new ApprovalRequest
            {
                RequestType    = request.RequestType ?? "CHANGE_PERMISSION",
                FolderID       = request.FolderID,
                FileID         = request.FileID,
                RequestedBy    = user.UserID,
                RequestedDate  = DateTime.Now,
                RequestStatus  = 1, // Pending
                Subject        = request.Subject,
                Remark         = request.Remark,
                ExpiryDate     = request.ExpiryDate ?? DateTime.Now.AddDays(1)
            };

            int requestId = _approvalRepository.InsertRequest(approvalRequest);
            approvalRequest.RequestID = requestId;

            foreach (var stepDef in request.Steps.OrderBy(s => s.StepOrder))
            {
                var step = new ApprovalStep
                {
                    RequestID  = requestId,
                    StepOrder  = stepDef.StepOrder,
                    ApproverID = stepDef.ApproverID,
                    StepStatus = 1 // Pending
                };
                _approvalRepository.InsertStep(step);
            }

            Log(user, "Workflow", "CREATE", "workflow.ApprovalRequest", requestId, 1,
                string.Format("Approval request created. Subject: {0}", request.Subject));

            // Notify first approver
            var firstStep = request.Steps.OrderBy(s => s.StepOrder).First();
            NotifyUser(firstStep.ApproverID, "ApprovalRequestCreated", requestId.ToString(),
                string.Format("You have a pending approval request (ID {0}) that requires your action.", requestId));

            return approvalRequest;
        }

        // -----------------------------------------------------------------------
        // ApproveStep
        // -----------------------------------------------------------------------

        public void ApproveStep(int stepId, string remark, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Find the step by looking through all steps of pending requests
            ApprovalStep step = FindStep(stepId);
            if (step == null)
                throw new InvalidOperationException(string.Format("ApprovalStep with ID {0} was not found.", stepId));

            EnsureCanActOnStep(step, user);

            if (step.StepStatus != 1 && step.StepStatus != 4) // 1=Pending, 4=Delegated
                throw new InvalidOperationException(string.Format(
                    "ApprovalStep {0} cannot be approved because its status is {1}.", stepId, step.StepStatus));

            var approvalRequest = _approvalRepository.GetRequest(step.RequestID);
            if (approvalRequest == null)
                throw new InvalidOperationException(string.Format("ApprovalRequest {0} was not found.", step.RequestID));

            if (approvalRequest.RequestStatus != 1)
                throw new InvalidOperationException(string.Format(
                    "ApprovalRequest {0} cannot be acted on because its status is {1}.",
                    step.RequestID, approvalRequest.RequestStatus));

            // Mark step approved
            _approvalRepository.UpdateStepStatus(stepId, 2, remark); // 2=Approved

            Log(user, "Workflow", "APPROVE", "workflow.ApprovalStep", stepId, 1,
                string.Format("Step {0} of request {1} approved.", step.StepOrder, step.RequestID));

            var allSteps = _approvalRepository.GetAllSteps(step.RequestID)
                .OrderBy(s => s.StepOrder).ToList();

            bool isCreateHc = string.Equals(approvalRequest.RequestType, "CREATE_HC", StringComparison.OrdinalIgnoreCase);
            ApprovalStep nextStep = null;

            if (isCreateHc)
            {
                // CREATE_HC ใช้นโยบายอนุมัติคนแรกจบ: ปิดขั้นที่เหลือทั้งหมดแบบ auto-close
                foreach (var pending in allSteps.Where(s => s.StepID != step.StepID && s.StepStatus == 1))
                {
                    _approvalRepository.UpdateStepStatus(pending.StepID, 4,
                        "Auto-closed: CREATE_HC is finalized by first admin approval.");
                }
            }
            else
            {
                nextStep = allSteps.FirstOrDefault(s =>
                    s.StepOrder > step.StepOrder && s.StepStatus == 1);
            }

            if (nextStep != null)
            {
                NotifyUser(nextStep.ApproverID, "ApprovalRequestPending", step.RequestID.ToString(),
                    string.Format("Approval request {0} is now pending your action.", step.RequestID));
            }
            else
            {
                if (isCreateHc)
                {
                    // CREATE_HC ต้องสร้างโฟลเดอร์สำเร็จก่อนค่อย mark request เป็น approved
                    try
                    {
                        _folderService.CreateHCFolderAfterApproval(approvalRequest, user);
                        Log(user, "Folder", "APPROVE", "workflow.ApprovalRequest", step.RequestID, 1,
                            string.Format("CREATE_HC_APPROVED: HC Folder created after approval of request {0}.", step.RequestID));
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error(string.Format(
                            "ApprovalService.ApproveStep: Failed to create HC folder for request {0}. {1}",
                            step.RequestID, ex.Message), ex);

                        throw new InvalidOperationException(
                            string.Format("ไม่สามารถสร้าง 1HC Folder หลังอนุมัติได้: {0}", ex.Message), ex);
                    }
                }

                // Final step — mark request approved
                _approvalRepository.UpdateRequestStatus(step.RequestID, 2, user.UserID, remark); // 2=Approved

                Log(user, "Workflow", "APPROVE", "workflow.ApprovalRequest", step.RequestID, 1,
                    "All steps approved. Permissions applied.");

                // Apply the permission change that was requested
                int externalShareIdFromRemark = ParseExternalShareId(approvalRequest.Remark);
                bool isExternalShareRequest =
                    string.Equals(approvalRequest.RequestType, "EXTERNAL_SHARE", StringComparison.OrdinalIgnoreCase)
                    || externalShareIdFromRemark > 0;

                if (approvalRequest.RequestType == "CHANGE_PERMISSION" &&
                    approvalRequest.FolderID.HasValue &&
                    !isExternalShareRequest)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"ApproveStep: Applying permission for request {step.RequestID}");
                        System.Diagnostics.Debug.WriteLine($"  Remark: {approvalRequest.Remark}");
                        System.Diagnostics.Debug.WriteLine($"  FolderID: {approvalRequest.FolderID.Value}");
                        
                        var permReq = ParsePermissionRemark(approvalRequest.Remark, approvalRequest.FolderID.Value);
                        if (permReq != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"  Parsed permission request successfully");
                            System.Diagnostics.Debug.WriteLine($"  PrincipalType: {permReq.PrincipalType}, PrincipalID: {permReq.PrincipalID}");
                            System.Diagnostics.Debug.WriteLine($"  Permissions: Read:{permReq.CanRead}, Write:{permReq.CanWrite}, Delete:{permReq.CanDelete}");
                            
                            _permissionService.SetFolderPermission(permReq, user);
                            
                            System.Diagnostics.Debug.WriteLine($"  Permission applied successfully");
                            
                            Log(user, "Workflow", "APPLY_PERMISSION", "security.FolderPermission",
                                step.RequestID, 1,
                                string.Format("Permission applied for FolderID:{0} after approval.", approvalRequest.FolderID.Value));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  Failed to parse permission remark");
                            _loggingService.Error(string.Format(
                                "ApprovalService.ApproveStep: Failed to parse permission remark for request {0}. Remark: {1}",
                                step.RequestID, approvalRequest.Remark));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Exception applying permission: {ex.Message}");
                        _loggingService.Error(string.Format(
                            "ApprovalService.ApproveStep: Failed to apply permission for request {0}. {1}",
                            step.RequestID, ex.Message), ex);
                    }
                }
                else if (approvalRequest.RequestType == "DELETE_FOLDER" && approvalRequest.FolderID.HasValue)
                {
                    try
                    {
                        _folderRepository.SoftDelete(approvalRequest.FolderID.Value, user.UserID);
                        Log(user, "Folder", "DELETE", "storage.Folder", approvalRequest.FolderID.Value, 1,
                            string.Format("Folder soft-deleted after approval of request {0}.", step.RequestID));
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error(string.Format(
                            "ApprovalService.ApproveStep: Failed to soft-delete folder {0} for request {1}. {2}",
                            approvalRequest.FolderID.Value, step.RequestID, ex.Message), ex);
                    }
                }
                else if (isExternalShareRequest && _externalShareRepository != null)
                {
                    try
                    {
                        int externalShareId = externalShareIdFromRemark;
                        if (externalShareId > 0)
                        {
                            var share = _externalShareRepository.GetById(externalShareId);
                            if (share != null && share.ShareTokenStatus == "Pending")
                            {
                                share.ShareTokenStatus = "Active";
                                share.ApprovedDate    = DateTime.Now;
                                share.ApproverUserID  = user.UserID;
                                _externalShareRepository.Update(share);

                                if (_shareTokenRepository != null)
                                {
                                    foreach (var tok in _shareTokenRepository.GetByExternalShare(externalShareId))
                                    {
                                        tok.Status        = "Active";
                                        tok.ActivatedDate = DateTime.Now;
                                        _shareTokenRepository.Update(tok);
                                    }
                                }

                                Log(user, "FileSharing", "APPROVE_EXTERNAL_SHARE",
                                    "storage.ExternalShare", externalShareId, 1,
                                    string.Format("ExternalShare {0} activated after approval of request {1}.",
                                        externalShareId, step.RequestID));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error(string.Format(
                            "ApprovalService.ApproveStep: Failed to activate ExternalShare for request {0}. {1}",
                            step.RequestID, ex.Message), ex);
                    }
                }

                bool isInternalShareFile =
                    string.Equals(approvalRequest.RequestType, "INTERNAL_SHARE", StringComparison.OrdinalIgnoreCase)
                    || (approvalRequest.Remark != null &&
                        approvalRequest.Remark.StartsWith("InternalShareFile", StringComparison.OrdinalIgnoreCase));

                if (isInternalShareFile && _filePermissionRepository != null)
                {
                    try
                    {
                        var fp = ParseInternalShareRemark(approvalRequest.Remark);
                        if (fp != null && fp.FileID > 0)
                        {
                            fp.GrantedBy   = approvalRequest.RequestedBy;
                            fp.GrantedDate = DateTime.Now;
                            _filePermissionRepository.Insert(fp);
                            Log(user, "FileSharing", "APPROVE_INTERNAL_SHARE",
                                "storage.FilePermission", fp.FileID, 1,
                                string.Format("FilePermission created for FileID:{0} after approval of request {1}.",
                                    fp.FileID, step.RequestID));
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error(string.Format(
                            "ApprovalService.ApproveStep: Failed to create FilePermission for INTERNAL_SHARE request {0}. {1}",
                            step.RequestID, ex.Message), ex);
                    }
                }

                string approvedMessage = isCreateHc
                    ? string.Format("คำขอสร้าง 1HC Folder (ID {0}) ได้รับการอนุมัติและสร้างโฟลเดอร์เรียบร้อยแล้ว", step.RequestID)
                    : isExternalShareRequest
                        ? string.Format("คำขอแชร์ภายนอก (ID {0}) ได้รับการอนุมัติและเปิดใช้งานลิงก์แล้ว", step.RequestID)
                    : isInternalShareFile
                        ? string.Format("คำขอแชร์ไฟล์ภายใน (ID {0}) ได้รับการอนุมัติและมีผลแล้ว", step.RequestID)
                    : string.Equals(approvalRequest.RequestType, "DOWNLOAD_FILE", StringComparison.OrdinalIgnoreCase)
                        ? string.Format("คำขอดาวน์โหลดไฟล์ (ID {0}) ได้รับการอนุมัติแล้ว — กดดาวน์โหลดที่ไฟล์ได้ 1 ครั้ง", step.RequestID)
                        : string.Format("คำขอเปลี่ยนสิทธิ์ (ID {0}) ได้รับการอนุมัติและมีผลแล้ว", step.RequestID);

                NotifyUser(approvalRequest.RequestedBy, "ApprovalRequestApproved",
                    step.RequestID.ToString(), approvedMessage);
            }
        }

        // -----------------------------------------------------------------------
        // RejectStep
        // -----------------------------------------------------------------------

        public void RejectStep(int stepId, string reason, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            ApprovalStep step = FindStep(stepId);
            if (step == null)
                throw new InvalidOperationException(string.Format("ApprovalStep with ID {0} was not found.", stepId));

            EnsureCanActOnStep(step, user);

            if (step.StepStatus != 1 && step.StepStatus != 4)
                throw new InvalidOperationException(string.Format(
                    "ApprovalStep {0} cannot be rejected because its status is {1}.", stepId, step.StepStatus));

            var approvalRequest = _approvalRepository.GetRequest(step.RequestID);
            if (approvalRequest == null)
                throw new InvalidOperationException(string.Format("ApprovalRequest {0} was not found.", step.RequestID));

            if (approvalRequest.RequestStatus != 1)
                throw new InvalidOperationException(string.Format(
                    "ApprovalRequest {0} cannot be acted on because its status is {1}.",
                    step.RequestID, approvalRequest.RequestStatus));

            _approvalRepository.UpdateStepStatus(stepId, 3, reason); // 3=Rejected
            _approvalRepository.UpdateRequestStatus(step.RequestID, 3, user.UserID, reason); // 3=Rejected

            Log(user, "Workflow", "REJECT", "workflow.ApprovalStep", stepId, 1,
                string.Format("Step {0} of request {1} rejected. Reason: {2}",
                    step.StepOrder, step.RequestID, reason));

            NotifyUser(approvalRequest.RequestedBy, "ApprovalRequestRejected",
                step.RequestID.ToString(),
                string.Format("Your permission request (ID {0}) has been rejected. Reason: {1}",
                    step.RequestID, string.IsNullOrEmpty(reason) ? "No reason provided." : reason));
        }

        // -----------------------------------------------------------------------
        // WithdrawRequest
        // -----------------------------------------------------------------------

        public void WithdrawRequest(int requestId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var approvalRequest = _approvalRepository.GetRequest(requestId);
            if (approvalRequest == null)
                throw new InvalidOperationException(string.Format("ApprovalRequest with ID {0} was not found.", requestId));

            if (approvalRequest.RequestStatus != 1)
                throw new InvalidOperationException(string.Format(
                    "ApprovalRequest {0} cannot be withdrawn because its status is {1}.",
                    requestId, approvalRequest.RequestStatus));

            if (approvalRequest.RequestedBy != user.UserID)
                throw new UnauthorizedAccessException(string.Format(
                    "User {0} is not the requestor of ApprovalRequest {1}.", user.UserID, requestId));

            foreach (var step in _approvalRepository.GetPendingSteps(requestId))
                _approvalRepository.UpdateStepStatus(step.StepID, 3, "Withdrawn by requestor."); // 3=Rejected

            _approvalRepository.UpdateRequestStatus(requestId, 4, user.UserID, "Withdrawn by requestor."); // 4=Expired

            Log(user, "Workflow", "DELETE", "workflow.ApprovalRequest", requestId, 1,
                string.Format("Request {0} withdrawn by requestor {1}.", requestId, user.UserID));
        }

        // -----------------------------------------------------------------------
        // GetPendingForApprover
        // -----------------------------------------------------------------------

        public IEnumerable<ApprovalRequest> GetRequestsByRequester(int userId, string requestType)
        {
            try
            {
                return _approvalRepository.GetRequestsByRequester(userId, requestType).ToList();
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ApprovalService.GetRequestsByRequester: Failed for UserID {0}, type {1}.", userId, requestType), ex);
                return new List<ApprovalRequest>();
            }
        }

        public IEnumerable<ApprovalRequest> GetPendingForApprover(int approverId)
        {
            var results = new List<ApprovalRequest>();
            
            try
            {
                var approverUser = _userAccountRepository.GetById(approverId);
                bool isAdmin    = approverUser != null && approverUser.IsActive && approverUser.AppRole == 4;

                var allPendingRequests = _approvalRepository.GetPendingRequests().ToList();
                
                foreach (var request in allPendingRequests)
                {
                    string requestType = (request.RequestType ?? string.Empty).Trim().ToUpperInvariant();

                    // CREATE_HC — แสดงให้ Admin ทุกคนเห็นโดยตรง (ไม่ต้อง check step)
                    if (requestType == "CREATE_HC" && isAdmin)
                    {
                        results.Add(request);
                        continue;
                    }

                    // Request types อื่น — ตรวจ pending step ที่ assign หา approver นี้
                    var pendingSteps = _approvalRepository.GetPendingSteps(request.RequestID).ToList();
                    bool hasOwnPendingStep = pendingSteps.Any(step => step.ApproverID == approverId);

                    // Fallback: Admin เห็น delete request ที่ยังมี step ค้าง (คนอื่นเห็นเฉพาะ step ที่ assign ให้ตัวเอง)
                    bool isDeleteFallback = isAdmin
                        && (requestType == "DELETE" || requestType == "DELETE_FILE"
                            || requestType == "FILE_DELETE" || requestType == "DELETE_FOLDER")
                        && pendingSteps.Any();

                    if (hasOwnPendingStep || isDeleteFallback)
                        results.Add(request);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"ApprovalService.GetPendingForApprover: Failed to get pending requests for approver {approverId}.", ex);
            }
            
            return results;
        }

        // -----------------------------------------------------------------------
        // GetApprovalSteps
        // -----------------------------------------------------------------------

        public IEnumerable<ApprovalStep> GetApprovalSteps(int requestId)
        {
            try
            {
                return _approvalRepository.GetAllSteps(requestId);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"ApprovalService.GetApprovalSteps: Failed to get steps for request {requestId}.", ex);
                return new List<ApprovalStep>();
            }
        }

        // -----------------------------------------------------------------------
        // RequestFolderDelete
        // -----------------------------------------------------------------------

        public ApprovalRequest RequestFolderDelete(int folderId, string reason, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));
            if (folder.IsDeleted)
                throw new InvalidOperationException("Folder is already in the recycle bin.");
            if (folder.FileCount == 0)
                throw new InvalidOperationException("Folder มีไฟล์อยู่ 0 รายการ สามารถลบได้โดยตรงโดยไม่ต้องขออนุมัติ");

            // ผู้อนุมัติ: ผู้มีสิทธิ์อนุมัติของแผนกเจ้าของโฟลเดอร์ (fallback ฝ่าย → Admin)
            var approvers = ResolveFolderDeleteApprovers(folderId, user.UserID);
            if (!approvers.Any())
                throw new InvalidOperationException("ไม่พบผู้อนุมัติที่ Active ในระบบ ไม่สามารถส่งคำขออนุมัติได้");

            var approvalRequest = new ApprovalRequest
            {
                RequestType   = "DELETE_FOLDER",
                FolderID      = folderId,
                RequestedBy   = user.UserID,
                RequestedDate = DateTime.Now,
                RequestStatus = 1, // Pending
                Subject       = string.Format("ขออนุมัติลบ Folder: {0}", folder.FolderName),
                Remark        = reason ?? string.Empty,
                ExpiryDate    = DateTime.Now.AddDays(7)
            };

            int requestId = _approvalRepository.InsertRequest(approvalRequest);
            approvalRequest.RequestID = requestId;

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

                NotifyUser(approver.UserID, "DeleteFolderRequested", requestId.ToString(),
                    string.Format("มีคำขออนุมัติลบ Folder '{0}' (คำขอ ID {1}) รออนุมัติจากคุณ",
                        folder.FolderName, requestId));
            }

            Log(user, "Workflow", "CREATE", "workflow.ApprovalRequest", requestId, 1,
                string.Format("DELETE_FOLDER request created for Folder {0}. Subject: {1}",
                    folderId, approvalRequest.Subject));

            return approvalRequest;
        }

        // -----------------------------------------------------------------------
        // GetApprovalsByFolder
        // -----------------------------------------------------------------------

        public IEnumerable<ApprovalRequest> GetApprovalsByFolder(int folderId)
        {
            try
            {
                return _approvalRepository.GetPendingRequests()
                    .Where(r => r.FolderID == folderId)
                    .OrderByDescending(r => r.RequestedDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format("ApprovalService.GetApprovalsByFolder: Failed for folder {0}.", folderId), ex);
                return new List<ApprovalRequest>();
            }
        }

        // -----------------------------------------------------------------------
        // GetAllApprovalsByFolder  (all statuses — for display in Workflow tab)
        // -----------------------------------------------------------------------

        public IEnumerable<ApprovalRequest> GetAllApprovalsByFolder(int folderId)
        {
            try
            {
                return _approvalRepository.GetAllRequestsByFolder(folderId);
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format("ApprovalService.GetAllApprovalsByFolder: Failed for folder {0}.", folderId), ex);
                return new List<ApprovalRequest>();
            }
        }

        // -----------------------------------------------------------------------
        // ProcessEscalations
        // -----------------------------------------------------------------------

        public void ProcessEscalations()
        {
            try
            {
                var expiredRequests = _approvalRepository.GetExpiredPendingRequests();

                foreach (var request in expiredRequests)
                {
                    try
                    {
                        EscalateRequest(request);
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error(string.Format(
                            "ApprovalService.ProcessEscalations: Failed to escalate request {0}.", request.RequestID), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("ApprovalService.ProcessEscalations: Failed to retrieve expired requests.", ex);
            }
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private void EscalateRequest(ApprovalRequest request)
        {
            var pendingSteps = _approvalRepository.GetPendingSteps(request.RequestID)
                .OrderBy(s => s.StepOrder).ToList();

            if (!pendingSteps.Any()) return;

            var currentStep = pendingSteps.First();

            // Mark current step as Delegated (4)
            _approvalRepository.UpdateStepStatus(currentStep.StepID, 4, "Escalated due to timeout.");

            // Escalate to Manager (3) or Admin (4)
            int escalationRole = currentStep.ApproverID == 3 ? 4 : 3;

            var allSteps = _approvalRepository.GetAllSteps(request.RequestID)
                .OrderBy(s => s.StepOrder).ToList();
            int nextStepOrder = allSteps.Max(s => s.StepOrder) + 1;

            var escalationStep = new ApprovalStep
            {
                RequestID  = request.RequestID,
                StepOrder  = nextStepOrder,
                ApproverID = escalationRole,
                StepStatus = 1 // Pending
            };

            _approvalRepository.InsertStep(escalationStep);

            LogSystem("Workflow", "UPDATE", "workflow.ApprovalRequest", request.RequestID, 1,
                string.Format("Request {0} escalated to role {1} due to timeout.", request.RequestID, escalationRole));

            NotifyUser(request.RequestedBy, "ApprovalEscalation", request.RequestID.ToString(),
                string.Format("Your permission request (ID {0}) has been escalated due to timeout.", request.RequestID));

            // Notify all users with the escalation role
            var escalationUsers = _userAccountRepository.GetByRole(escalationRole);
            foreach (var u in escalationUsers.Where(u => u.IsActive))
            {
                NotifyUser(u.UserID, "ApprovalEscalation", request.RequestID.ToString(),
                    string.Format("Approval request {0} has been escalated to your role.", request.RequestID));
            }
        }

        /// <summary>
        /// Only the user assigned to the step (e.g. a department approver) or an Admin may act on it.
        /// </summary>
        private static void EnsureCanActOnStep(ApprovalStep step, UserContext user)
        {
            if (user.AppRole == 4 || step.ApproverID == user.UserID)
                return;

            throw new UnauthorizedAccessException(string.Format(
                "ApprovalStep {0} is assigned to another approver.", step.StepID));
        }

        private List<UserAccount> ResolveFolderDeleteApprovers(int folderId, int requesterId)
        {
            if (_approverResolver != null)
                return _approverResolver.ResolveForFolder(folderId, requesterId).ToList();

            // Legacy: Admin ทุกคน (ยกเว้นตัวเอง ถ้ามี Admin คนอื่น)
            var admins = _userAccountRepository.GetByRole(4).Where(a => a.IsActive).ToList();
            var others = admins.Where(a => a.UserID != requesterId).ToList();
            return others.Any() ? others : admins;
        }

        private ApprovalStep FindStep(int stepId)
        {
            try
            {
                // Get all pending requests and search their steps
                var allPendingRequests = _approvalRepository.GetPendingRequests().ToList();
                
                foreach (var request in allPendingRequests)
                {
                    var steps = _approvalRepository.GetAllSteps(request.RequestID);
                    var step = steps.FirstOrDefault(s => s.StepID == stepId);
                    if (step != null)
                        return step;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"ApprovalService.FindStep: Failed to find step {stepId}.", ex);
                return null;
            }
        }
        private static int ParseExternalShareId(string remark)
        {
            if (string.IsNullOrEmpty(remark)) return 0;
            foreach (var token in remark.Split(' '))
            {
                if (token.StartsWith("ExternalShareID:", StringComparison.OrdinalIgnoreCase))
                {
                    int id;
                    if (int.TryParse(token.Substring("ExternalShareID:".Length), out id))
                        return id;
                }
            }
            return 0;
        }

        /// <summary>
        /// Parses the permission remark string stored in ApprovalRequest.Remark back into a SetPermissionRequest.
        /// Format: "FolderID:X PrincipalType:Y PrincipalID:Z Read:True Write:True Delete:False Modify:False FullControl:False IsDeny:False"
        /// </summary>
        private static SetPermissionRequest ParsePermissionRemark(string remark, int folderId)
        {
            if (string.IsNullOrEmpty(remark)) return null;

            try
            {
                System.Diagnostics.Debug.WriteLine($"ParsePermissionRemark: Parsing remark: {remark}");
                
                var parts = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var token in remark.Split(' '))
                {
                    var idx = token.IndexOf(':');
                    if (idx > 0)
                    {
                        string key = token.Substring(0, idx);
                        string value = token.Substring(idx + 1);
                        parts[key] = value;
                        System.Diagnostics.Debug.WriteLine($"  Parsed: {key} = {value}");
                    }
                }

                var request = new SetPermissionRequest
                {
                    FolderID       = folderId,
                    PrincipalType  = (PrincipalType)RemarkGetInt(parts, "PrincipalType"),
                    PrincipalID    = RemarkGetInt(parts, "PrincipalID"),
                    CanRead        = RemarkGetBool(parts, "Read"),
                    CanWrite       = RemarkGetBool(parts, "Write"),
                    CanDelete      = RemarkGetBool(parts, "Delete"),
                    CanModify      = RemarkGetBool(parts, "Modify"),
                    CanFullControl = RemarkGetBool(parts, "FullControl"),
                    IsDeny         = RemarkGetBool(parts, "IsDeny")
                };

                System.Diagnostics.Debug.WriteLine($"ParsePermissionRemark: Created request - PrincipalType:{request.PrincipalType}, PrincipalID:{request.PrincipalID}, Read:{request.CanRead}");
                return request;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ParsePermissionRemark Error: {ex.Message}");
                return null;
            }
        }

        private static bool RemarkGetBool(System.Collections.Generic.Dictionary<string, string> parts, string key)
        {
            string v;
            return parts.TryGetValue(key, out v) && string.Equals(v, "True", StringComparison.OrdinalIgnoreCase);
        }

        private static int RemarkGetInt(System.Collections.Generic.Dictionary<string, string> parts, string key)
        {
            string v;
            int result;
            return parts.TryGetValue(key, out v) && int.TryParse(v, out result) ? result : 0;
        }

        private static DateTime? RemarkGetDate(System.Collections.Generic.Dictionary<string, string> parts, string key)
        {
            string v;
            if (!parts.TryGetValue(key, out v) || string.IsNullOrEmpty(v)) return null;
            DateTime d;
            return DateTime.TryParse(v, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out d) ? (DateTime?)d : null;
        }

        private static Domain.Models.FilePermission ParseInternalShareRemark(string remark)
        {
            if (string.IsNullOrEmpty(remark)) return null;
            if (!remark.StartsWith("InternalShareFile", StringComparison.OrdinalIgnoreCase)) return null;

            var parts = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var token in remark.Split(' '))
            {
                var idx = token.IndexOf(':');
                if (idx > 0)
                    parts[token.Substring(0, idx)] = token.Substring(idx + 1);
            }

            return new Domain.Models.FilePermission
            {
                FileID         = RemarkGetInt(parts, "FileID"),
                PrincipalType  = RemarkGetInt(parts, "PrincipalType"),
                PrincipalID    = RemarkGetInt(parts, "PrincipalID"),
                CanRead        = RemarkGetBool(parts, "Read"),
                CanWrite       = RemarkGetBool(parts, "Write"),
                CanDelete      = RemarkGetBool(parts, "Delete"),
                CanModify      = false,
                CanFullControl = false,
                IsDeny         = false,
                ExpiryDate     = RemarkGetDate(parts, "Expiry"),
            };
        }

        private void NotifyUser(int userId, string eventType, string relatedEntityId, string message)
        {
            try
            {
                _notificationRepository.Insert(new Notification
                {
                    RecipientUserId = userId,
                    EventType       = eventType,
                    RelatedEntityId = relatedEntityId,
                    MessageText     = message,
                    DeliveredAt     = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format("ApprovalService: Failed to notify user {0}.", userId), ex);
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

        private void LogSystem(string module, string action, string tableName,
                               int recordId, int result, string newValue = null)
        {
            _auditService.Log(new AuditEntry
            {
                Module      = module,
                Action      = action,
                UserID      = 0,
                UserName    = "SYSTEM",
                TableName   = tableName,
                RecordID    = recordId > 0 ? (int?)recordId : null,
                NewValue    = newValue,
                MachineName = "SYSTEM",
                Result      = result,
                AuditDate   = DateTime.Now
            });
        }

        // -----------------------------------------------------------------------
        // SubmitDownloadRequest — ทุก role ส่งคำขอได้
        // -----------------------------------------------------------------------

        public ApprovalRequest SubmitDownloadRequest(int fileId, string reason, int approverId, UserContext user)
        {
            if (user == null)   throw new ArgumentNullException("user");
            if (approverId <= 0) throw new ArgumentException("approverId must be valid.", "approverId");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("reason must not be empty.", "reason");

            if (_approverResolver != null
                && !GetDownloadApprovers(fileId, user).Any(a => a.UserID == approverId))
                throw new UnauthorizedAccessException("ผู้อนุมัติที่เลือกไม่มีสิทธิ์อนุมัติไฟล์นี้ หรือ Level ไม่สูงกว่าผู้ขอ");

            var approvalRequest = new ApprovalRequest
            {
                RequestType   = "DOWNLOAD_FILE",
                FileID        = fileId,
                RequestedBy   = user.UserID,
                RequestedDate = DateTime.Now,
                RequestStatus = 1, // Pending
                Subject       = string.Format("ขออนุมัติดาวน์โหลดไฟล์ ID {0}", fileId),
                Remark        = reason,
                ExpiryDate    = DateTime.Now.AddDays(3)
            };

            int requestId = _approvalRepository.InsertRequest(approvalRequest);
            approvalRequest.RequestID = requestId;

            _approvalRepository.InsertStep(new ApprovalStep
            {
                RequestID  = requestId,
                StepOrder  = 1,
                ApproverID = approverId,
                StepStatus = 1
            });

            Log(user, "Download", "CREATE", "workflow.ApprovalRequest", requestId, 1,
                string.Format("DOWNLOAD_FILE request created. FileID:{0} Approver:{1}", fileId, approverId));

            NotifyUser(approverId, "DownloadFileRequested", requestId.ToString(),
                string.Format("มีคำขออนุมัติดาวน์โหลดไฟล์ (คำขอ ID {0}) รออนุมัติจากคุณ โดย {1}",
                    requestId, user.ADUserName ?? user.UserID.ToString()));

            return approvalRequest;
        }

        // -----------------------------------------------------------------------
        // GetDownloadApprovers — ผู้อนุมัติที่เลือกได้สำหรับคำขอดาวน์โหลด
        // -----------------------------------------------------------------------

        public IEnumerable<UserAccount> GetDownloadApprovers(int fileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Legacy (ไม่มี resolver): Manager/Admin ทั้งระบบ
            if (_approverResolver == null)
                return _userAccountRepository.GetAll()
                    .Where(u => u.IsActive && u.AppRole >= 3 && u.UserID != user.UserID)
                    .ToList();

            // 1) ผู้อนุมัติที่ Admin ตั้งไว้ของแผนก + ฝ่าย → ใช้รายชื่อนี้เท่านั้น (ไม่ต่อท้าย Admin)
            var configured = _approverResolver.GetConfiguredDownloadApprovers(fileId, user);
            if (configured.Any())
                return configured;

            // 2) ไม่มี config: พนักงานในแผนกเจ้าของไฟล์ที่ Level สูงกว่าผู้ขอ
            // ลำดับ Level: 1A < 1B < 2A < 2B < 3A < 3B < 4A < 4B (เลขมาก = อาวุโสกว่า)
            var members = _approverResolver.GetDepartmentMembersForFile(fileId, user);

            int requesterRank = LevelRank(user.LevelCode);
            var candidates = members
                .Where(u => LevelRank(u.LevelCode) > requesterRank)
                .OrderBy(u => LevelRank(u.LevelCode))
                .ThenBy(u => u.DisplayName ?? u.ADUserName)
                .ToList();

            // Admin แสดงต่อท้ายทุกครั้ง (ไม่ซ้ำกับคนที่อยู่ในรายชื่อแผนกแล้ว)
            var listed = new HashSet<int>(candidates.Select(u => u.UserID));
            candidates.AddRange(_approverResolver.GetAdmins(user.UserID)
                .Where(a => listed.Add(a.UserID))
                .OrderBy(a => a.DisplayName ?? a.ADUserName));

            return candidates;
        }

        /// <summary>
        /// Level "1A" → 1*26+0 = 26, "1B" → 27, "2A" → 52 ... larger = more senior.
        /// Empty or unrecognised codes rank lowest (-1), so they never appear above anyone.
        /// </summary>
        private static int LevelRank(string levelCode)
        {
            if (string.IsNullOrWhiteSpace(levelCode)) return -1;
            string code = levelCode.Trim().ToUpperInvariant();

            int i = 0;
            while (i < code.Length && char.IsDigit(code[i])) i++;
            int number;
            if (i == 0 || !int.TryParse(code.Substring(0, i), out number)) return -1;

            int letter = (i < code.Length && code[i] >= 'A' && code[i] <= 'Z') ? code[i] - 'A' : 0;
            return number * 26 + letter;
        }

        // -----------------------------------------------------------------------
        // GetApprovedDownloadRequest
        // -----------------------------------------------------------------------

        public ApprovalRequest GetApprovedDownloadRequest(int fileId, int userId)
        {
            try
            {
                return _approvalRepository.GetApprovedDownloadRequest(fileId, userId);
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ApprovalService.GetApprovedDownloadRequest: Failed for FileID {0}, UserID {1}.", fileId, userId), ex);
                return null;
            }
        }

        // -----------------------------------------------------------------------
        // ConsumeDownloadApproval — mark status=5 (Downloaded) after file is saved
        // -----------------------------------------------------------------------

        public void ConsumeDownloadApproval(int requestId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // 5 = Downloaded (consumed — one approval = one download)
            _approvalRepository.UpdateRequestStatus(requestId, 5, user.UserID, "ดาวน์โหลดเรียบร้อยแล้ว");

            Log(user, "Download", "UPDATE", "workflow.ApprovalRequest", requestId, 1,
                string.Format("DOWNLOAD_FILE approval {0} consumed by user {1}.", requestId, user.ADUserName));
        }
    }
}
