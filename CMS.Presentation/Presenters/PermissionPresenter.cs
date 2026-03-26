using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Presenters
{
    public class PermissionPresenter
    {
        private readonly IPermissionService      _permissionService;
        private readonly IApprovalService        _approvalService;
        private readonly IFolderService          _folderService;
        private readonly IPermissionRepository   _permissionRepo;
        private readonly IPermissionView         _view;
        private readonly UserContext             _user;
        private readonly IFileDocumentRepository _fileDocumentRepository;

        private Folder _currentFolder;

        public PermissionPresenter(
            IPermissionService      permissionService,
            IApprovalService        approvalService,
            IFolderService          folderService,
            IPermissionRepository   permissionRepo,
            IPermissionView         view,
            UserContext             user,
            IFileDocumentRepository fileDocumentRepository = null)
        {
            if (permissionService == null) throw new ArgumentNullException("permissionService");
            if (approvalService   == null) throw new ArgumentNullException("approvalService");
            if (folderService     == null) throw new ArgumentNullException("folderService");
            if (permissionRepo    == null) throw new ArgumentNullException("permissionRepo");
            if (view              == null) throw new ArgumentNullException("view");
            if (user              == null) throw new ArgumentNullException("user");

            _permissionService      = permissionService;
            _approvalService        = approvalService;
            _folderService          = folderService;
            _permissionRepo         = permissionRepo;
            _view                   = view;
            _user                   = user;
            _fileDocumentRepository = fileDocumentRepository;
        }

        public void LoadAclEntries(Folder folder)
        {
            if (folder == null)
            {
                _view.ShowError("No folder selected.");
                return;
            }

            _currentFolder = folder;

            try
            {
                IEnumerable<FolderPermission> entries = _permissionRepo.GetAllByFolder(folder.FolderID);

                // Auto-revoke entries that have passed their expiry date
                var expired = entries
                    .Where(e => e.RevokedDate == null
                             && e.ExpiryDate.HasValue
                             && e.ExpiryDate.Value.Date < DateTime.Today)
                    .ToList();
                foreach (var exp in expired)
                    _permissionRepo.Revoke(exp.PermissionID, _user.UserID, "หมดอายุอัตโนมัติ");

                // Reload after auto-revoke if any were expired
                if (expired.Count > 0)
                    entries = _permissionRepo.GetAllByFolder(folder.FolderID);

                _view.ShowAclEntries(entries ?? Enumerable.Empty<FolderPermission>());

                EffectivePermission effective = _permissionService.GetEffectivePermission(
                    _user.UserID, folder.FolderID);
                _view.ShowEffectivePermissions(effective);

                // Show any pending approval requests for this folder
                var pending = _approvalService.GetAllApprovalsByFolder(folder.FolderID)
                    .Where(r => r.RequestStatus == 1 && r.RequestType == "CHANGE_PERMISSION")
                    .OrderByDescending(r => r.RequestedDate)
                    .ToList();
                _view.ShowPendingApprovals(pending);

                // ConfidentialityLevel is now int: 1=HC, 2=SC, 3=NC, 4=Public
                _view.ShowConfidentialityLevel(folder.ConfidentialityLevel);
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("Access denied: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to load permissions: " + ex.Message);
            }
        }

        public void SetPermission(SetPermissionRequest request)
        {
            if (_currentFolder == null)
            {
                _view.ShowError("No folder selected.");
                return;
            }

            try
            {
                // HC=1, SC=2 require approval workflow
                bool requiresApproval = _currentFolder.ConfidentialityLevel == 1
                                     || _currentFolder.ConfidentialityLevel == 2;

                if (requiresApproval)
                {
                    var approvalRequest = new SubmitApprovalRequest
                    {
                        RequestType = "CHANGE_PERMISSION",
                        FolderID    = _currentFolder.FolderID,
                        Subject     = string.Format("ขอเปลี่ยนสิทธิ์ Folder: {0}", _currentFolder.FolderName),
                        Remark      = BuildPermissionRemark(request),
                        Steps       = BuildDefaultApprovalSteps(_user)
                    };

                    _approvalService.SubmitPermissionRequest(approvalRequest, _user);

                    // Reload ACL list — LoadAclEntries will also call ShowPendingApprovals
                    LoadAclEntries(_currentFolder);

                    _view.ShowInfo(
                        "📋 ส่งคำขอเปลี่ยนสิทธิ์เพื่ออนุมัติแล้ว\n\n" +
                        "Folder นี้เป็นระดับ " + (_currentFolder.ConfidentialityLevel == 1 ? "1HC (Highly Confidential)" : "2SC (Secret)") + "\n" +
                        "การเปลี่ยนสิทธิ์จะมีผลหลังจากผู้อนุมัติอนุมัติคำขอแล้ว\n\n" +
                        "สามารถตรวจสอบสถานะได้ที่เมนู 'อนุมัติ'");
                }
                else
                {
                    _permissionService.SetFolderPermission(request, _user);
                    LoadAclEntries(_currentFolder);
                    PromptPropagateToFiles();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("Access denied: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("Cannot set permission: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Check if it's a foreign key constraint error
                if (ex.Message.Contains("FOREIGN KEY") || ex.Message.Contains("FK_"))
                {
                    _view.ShowError("Principal ID ไม่มีอยู่ในระบบ หรือข้อมูลไม่ถูกต้อง\n\n" + ex.Message);
                }
                else
                {
                    _view.ShowError("Failed to set permission: " + ex.Message);
                }
            }
        }

        public void RemovePermission(int permissionId)
        {
            if (_currentFolder == null)
            {
                _view.ShowError("ไม่พบ Folder ที่เลือก");
                return;
            }

            try
            {
                _permissionRepo.Revoke(permissionId, _user.UserID, "Removed by user");
                // Reload list first so the row disappears immediately, then notify
                LoadAclEntries(_currentFolder);
                _view.ShowInfo("ลบรายการสิทธิ์เรียบร้อยแล้ว");
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("ไม่มีสิทธิ์ดำเนินการ: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("ไม่สามารถลบสิทธิ์ได้: " + ex.Message);
            }
        }

        public void ChangeConfidentialityLevel(int folderId, int newLevel)
        {
            try
            {
                _folderService.ChangeConfidentialityLevel(folderId, newLevel, _user);
                _view.ShowInfo(string.Format(
                    "Confidentiality level changed to {0} and propagated to all inheriting descendants.",
                    ConfidentialityLabel(newLevel)));

                if (_currentFolder != null && _currentFolder.FolderID == folderId)
                {
                    _currentFolder.ConfidentialityLevel = newLevel;
                    _view.ShowConfidentialityLevel(newLevel);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("Access denied: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("Cannot change confidentiality level: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to change confidentiality level: " + ex.Message);
            }
        }

        private static string BuildPermissionRemark(SetPermissionRequest req)
        {
            return string.Format(
                "FolderID:{0} PrincipalType:{1} PrincipalID:{2} Read:{3} Write:{4} Delete:{5} Modify:{6} FullControl:{7} IsDeny:{8}",
                req.FolderID, req.PrincipalType, req.PrincipalID,
                req.CanRead, req.CanWrite, req.CanDelete, req.CanModify, req.CanFullControl, req.IsDeny);
        }

        private static IList<ApprovalStepDefinition> BuildDefaultApprovalSteps(UserContext user)
        {
            // Use current user as approver (or their manager if available)
            // For now, use current user's ID as the approver
            int approverId = user != null ? user.UserID : 1;
            
            return new List<ApprovalStepDefinition>
            {
                new ApprovalStepDefinition
                {
                    StepOrder  = 1,
                    ApproverID = approverId
                }
            };
        }

        private static string ConfidentialityLabel(int level)
        {
            switch (level)
            {
                case 1: return "1 — Highly Confidential";
                case 2: return "2 — Secret";
                case 3: return "3 — Confidential";
                case 4: return "4 — Public";
                default: return level.ToString();
            }
        }

        // ── File permission propagation ───────────────────────────────────────

        private void PromptPropagateToFiles()
        {
            if (_currentFolder == null || _fileDocumentRepository == null)
            {
                _view.ShowInfo("✅ บันทึกสิทธิ์เรียบร้อยแล้ว");
                return;
            }

            int fileCount;
            try { fileCount = _fileDocumentRepository.GetByFolder(_currentFolder.FolderID).Count(); }
            catch { fileCount = 0; }

            if (fileCount == 0)
            {
                _view.ShowInfo("✅ บันทึกสิทธิ์เรียบร้อยแล้ว\n\n(ไม่มีไฟล์ใน Folder นี้)");
                return;
            }

            // Auto-propagate — ไฟล์ใน Folder จะได้รับสิทธิ์เดียวกับ Folder เสมอ
            try
            {
                int updated = _permissionService.PropagatePermissionsToFiles(
                    _currentFolder.FolderID, _user);
                _view.ShowInfo(string.Format(
                    "✅ บันทึกสิทธิ์เรียบร้อยแล้ว\n\nอัปเดตสิทธิ์ไฟล์ {0} รายการให้ตรงกับ Folder อัตโนมัติ",
                    updated));
            }
            catch (Exception ex)
            {
                _view.ShowError("บันทึกสิทธิ์ Folder สำเร็จ แต่ไม่สามารถอัปเดตสิทธิ์ไฟล์ได้\n\n" + ex.Message);
            }
        }
    }
}
