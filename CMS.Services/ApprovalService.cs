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
        private readonly IPermissionService      _permissionService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserAccountRepository  _userAccountRepository;
        private readonly IAuditService           _auditService;
        private readonly ILoggingService         _loggingService;

        public ApprovalService(
            IApprovalRepository     approvalRepository,
            IFolderRepository       folderRepository,
            IPermissionService      permissionService,
            INotificationRepository notificationRepository,
            IUserAccountRepository  userAccountRepository,
            IAuditService           auditService,
            ILoggingService         loggingService)
        {
            if (approvalRepository     == null) throw new ArgumentNullException("approvalRepository");
            if (folderRepository       == null) throw new ArgumentNullException("folderRepository");
            if (permissionService      == null) throw new ArgumentNullException("permissionService");
            if (notificationRepository == null) throw new ArgumentNullException("notificationRepository");
            if (userAccountRepository  == null) throw new ArgumentNullException("userAccountRepository");
            if (auditService           == null) throw new ArgumentNullException("auditService");
            if (loggingService         == null) throw new ArgumentNullException("loggingService");

            _approvalRepository     = approvalRepository;
            _folderRepository       = folderRepository;
            _permissionService      = permissionService;
            _notificationRepository = notificationRepository;
            _userAccountRepository  = userAccountRepository;
            _auditService           = auditService;
            _loggingService         = loggingService;
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

            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("Approving an approval step requires Manager or Admin role.");

            // Find the step by looking through all steps of pending requests
            ApprovalStep step = FindStep(stepId);
            if (step == null)
                throw new InvalidOperationException(string.Format("ApprovalStep with ID {0} was not found.", stepId));

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

            var nextStep = allSteps.FirstOrDefault(s =>
                s.StepOrder > step.StepOrder && s.StepStatus == 1);

            if (nextStep != null)
            {
                NotifyUser(nextStep.ApproverID, "ApprovalRequestPending", step.RequestID.ToString(),
                    string.Format("Approval request {0} is now pending your action.", step.RequestID));
            }
            else
            {
                // Final step — mark request approved
                _approvalRepository.UpdateRequestStatus(step.RequestID, 2, user.UserID, remark); // 2=Approved

                Log(user, "Workflow", "APPROVE", "workflow.ApprovalRequest", step.RequestID, 1,
                    "All steps approved. Permissions applied.");

                // Apply the permission change that was requested
                if (approvalRequest.RequestType == "CHANGE_PERMISSION" && approvalRequest.FolderID.HasValue)
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

                NotifyUser(approvalRequest.RequestedBy, "ApprovalRequestApproved",
                    step.RequestID.ToString(),
                    string.Format("คำขอเปลี่ยนสิทธิ์ (ID {0}) ได้รับการอนุมัติและมีผลแล้ว", step.RequestID));
            }
        }

        // -----------------------------------------------------------------------
        // RejectStep
        // -----------------------------------------------------------------------

        public void RejectStep(int stepId, string reason, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("Rejecting an approval step requires Manager or Admin role.");

            ApprovalStep step = FindStep(stepId);
            if (step == null)
                throw new InvalidOperationException(string.Format("ApprovalStep with ID {0} was not found.", stepId));

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

        public IEnumerable<ApprovalRequest> GetPendingForApprover(int approverId)
        {
            var results = new List<ApprovalRequest>();
            
            try
            {
                // Get all pending requests (not just expired ones)
                var allPendingRequests = _approvalRepository.GetPendingRequests().ToList();
                
                // For each request, check if the approver has a pending step
                foreach (var request in allPendingRequests)
                {
                    var pendingSteps = _approvalRepository.GetPendingSteps(request.RequestID);
                    if (pendingSteps.Any(step => step.ApproverID == approverId && step.StepStatus == 1))
                    {
                        results.Add(request);
                    }
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

            // Find active admins (AppRole = 4) to be the approver
            var admins = _userAccountRepository.GetByRole(4).Where(a => a.IsActive).ToList();
            if (!admins.Any())
                throw new InvalidOperationException("ไม่พบ Admin ที่ Active ในระบบ ไม่สามารถส่งคำขออนุมัติได้");

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

            var firstAdmin = admins.First();
            _approvalRepository.InsertStep(new ApprovalStep
            {
                RequestID  = requestId,
                StepOrder  = 1,
                ApproverID = firstAdmin.UserID,
                StepStatus = 1 // Pending
            });

            Log(user, "Workflow", "CREATE", "workflow.ApprovalRequest", requestId, 1,
                string.Format("DELETE_FOLDER request created for Folder {0}. Subject: {1}",
                    folderId, approvalRequest.Subject));

            NotifyUser(firstAdmin.UserID, "DeleteFolderRequested", requestId.ToString(),
                string.Format("มีคำขออนุมัติลบ Folder '{0}' (คำขอ ID {1}) รออนุมัติจากคุณ",
                    folder.FolderName, requestId));

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
            try
            {
                _auditService.Log(new AuditEntry
                {
                    Module      = module,
                    Action      = action,
                    UserID      = user.UserID,
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
            catch { }
        }

        private void LogSystem(string module, string action, string tableName,
                               int recordId, int result, string newValue = null)
        {
            try
            {
                _auditService.Log(new AuditEntry
                {
                    Module      = module,
                    Action      = action,
                    UserID      = 0,
                    TableName   = tableName,
                    RecordID    = recordId > 0 ? (int?)recordId : null,
                    NewValue    = newValue,
                    MachineName = "SYSTEM",
                    Result      = result,
                    AuditDate   = DateTime.Now
                });
            }
            catch { }
        }
    }
}
