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

        public UserContext User => _user;

        public void SetCurrentFolder(Folder folder) { _currentFolder = folder; }

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
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] ── LoadAclEntries START ──────────────────────────────");
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] FolderID={folder.FolderID}  Name={folder.FolderName}  Level={folder.ConfidentialityLevel}");
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] UserID={_user.UserID}  AppRole={_user.AppRole}");

                var entries = _permissionRepo.GetAllByFolder(folder.FolderID).ToList();
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] Repository → {entries.Count} total entries (incl. revoked)");

                // Auto-revoke entries that have passed their expiry date
                var expired = entries
                    .Where(e => e.RevokedDate == null
                             && e.ExpiryDate.HasValue
                             && e.ExpiryDate.Value.Date < DateTime.Today)
                    .ToList();

                if (expired.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[PRESENTER] Auto-revoking {expired.Count} expired entries");
                    foreach (var exp in expired)
                        _permissionRepo.Revoke(exp.PermissionID, _user.UserID, "หมดอายุอัตโนมัติ");

                    entries = _permissionRepo.GetAllByFolder(folder.FolderID).ToList();
                    System.Diagnostics.Debug.WriteLine($"[PRESENTER] After auto-revoke reload → {entries.Count} entries");
                }

                System.Diagnostics.Debug.WriteLine($"[PRESENTER] Calling view.ShowAclEntries with {entries.Count} entries");
                _view.ShowAclEntries(entries);

                EffectivePermission effective = _permissionService.GetEffectivePermission(
                    _user.UserID, folder.FolderID);
                _view.ShowEffectivePermissions(effective);

                var pending = _approvalService.GetAllApprovalsByFolder(folder.FolderID)
                    .Where(r => r.RequestStatus == 1 && r.RequestType == "CHANGE_PERMISSION")
                    .OrderByDescending(r => r.RequestedDate)
                    .ToList();
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] Pending approvals: {pending.Count}");

                _view.ShowPendingApprovals(pending);
                _view.ShowConfidentialityLevel(folder.ConfidentialityLevel);
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] ── LoadAclEntries END ────────────────────────────────");
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] UNAUTHORIZED: {ex.Message}");
                _view.ShowError("Access denied: " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] EXCEPTION ({ex.GetType().Name}): {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] StackTrace: {ex.StackTrace}");
                _view.ShowError("Failed to load permissions: " + ex.Message);
            }
        }

        public void SetPermission(SetPermissionRequest request)
        {
            // ── ตรวจสอบ User ──────────────────────────────────────────────────
            if (_user == null)
            {
                _view.ShowError("ไม่พบข้อมูล User — กรุณาล็อกอินใหม่");
                return;
            }
            System.Diagnostics.Debug.WriteLine($"[PRESENTER] SetPermission: UserID={_user.UserID} Role={_user.AppRole}");

            // ── ตรวจสอบ Folder ────────────────────────────────────────────────
            if (_currentFolder == null)
            {
                // Fallback: โหลด Folder จาก FolderID ใน request
                try
                {
                    var folder = _folderService.GetFolder(request.FolderID, _user);
                    if (folder != null)
                    {
                        _currentFolder = folder;
                        System.Diagnostics.Debug.WriteLine($"[PRESENTER] SetPermission: Folder loaded from request — ID={folder.FolderID} Name={folder.FolderName}");
                    }
                    else
                    {
                        _view.ShowError($"ไม่พบ Folder ID: {request.FolderID}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _view.ShowError($"ไม่สามารถโหลด Folder ID {request.FolderID}: {ex.Message}");
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[PRESENTER] SetPermission: FolderID={_currentFolder.FolderID} Name={_currentFolder.FolderName} Level={_currentFolder.ConfidentialityLevel}");
            System.Diagnostics.Debug.WriteLine($"[PRESENTER] SetPermission: PrincipalType={request.PrincipalType} PrincipalID={request.PrincipalID} Read={request.CanRead} Write={request.CanWrite}");

            try
            {
                // บันทึกสิทธิ์ทันที — ไม่ผ่านขั้นตอนอนุมัติ
                _permissionService.SetFolderPermission(request, _user);
                System.Diagnostics.Debug.WriteLine($"[PRESENTER] SetPermission: SaveOK — reloading ACL...");
                LoadAclEntries(_currentFolder);
                PromptPropagateToFiles();
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
