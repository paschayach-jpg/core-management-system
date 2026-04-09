using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public class PermissionPresenter
    {
        private readonly IPermissionService      _permissionService;
        private readonly IApprovalService        _approvalService;
        private readonly IFolderService          _folderService;
        private readonly IFolderPermissionLookupController _folderPermissionLookupController;
        private readonly IPermissionView         _view;
        private readonly UserContext             _user;
        private readonly IFileDocumentLookupController _fileDocumentLookupController;
        private readonly IAuditService           _auditService;

        private Folder _currentFolder;

        public UserContext User => _user;

        public void SetCurrentFolder(Folder folder) { _currentFolder = folder; }

        public PermissionPresenter(
            IPermissionService      permissionService,
            IApprovalService        approvalService,
            IFolderService          folderService,
            IFolderPermissionLookupController folderPermissionLookupController,
            IPermissionView         view,
            UserContext             user,
            IFileDocumentLookupController fileDocumentLookupController = null,
            IAuditService           auditService           = null)
        {
            if (permissionService == null) throw new ArgumentNullException("permissionService");
            if (approvalService   == null) throw new ArgumentNullException("approvalService");
            if (folderService     == null) throw new ArgumentNullException("folderService");
            if (folderPermissionLookupController == null) throw new ArgumentNullException("folderPermissionLookupController");
            if (view              == null) throw new ArgumentNullException("view");
            if (user              == null) throw new ArgumentNullException("user");

            _permissionService      = permissionService;
            _approvalService        = approvalService;
            _folderService          = folderService;
            _folderPermissionLookupController = folderPermissionLookupController;
            _view                   = view;
            _user                   = user;
            _fileDocumentLookupController = fileDocumentLookupController;
            _auditService           = auditService;
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

                var entries = _folderPermissionLookupController.GetAllByFolder(folder.FolderID).ToList();

                // Auto-revoke entries that have passed their expiry date
                var expired = entries
                    .Where(e => e.RevokedDate == null
                             && e.ExpiryDate.HasValue
                             && e.ExpiryDate.Value.Date < DateTime.Today)
                    .ToList();

                if (expired.Count > 0)
                {
                    foreach (var exp in expired)
                        _folderPermissionLookupController.Revoke(exp.PermissionID, _user.UserID, "หมดอายุอัตโนมัติ");

                    entries = _folderPermissionLookupController.GetAllByFolder(folder.FolderID).ToList();
                }

                _view.ShowAclEntries(entries);

                EffectivePermission effective = _permissionService.GetEffectivePermission(
                    _user.UserID, folder.FolderID);
                _view.ShowEffectivePermissions(effective);

                var pending = _approvalService.GetAllApprovalsByFolder(folder.FolderID)
                    .Where(r => r.RequestStatus == 1 && r.RequestType == "CHANGE_PERMISSION")
                    .OrderByDescending(r => r.RequestedDate)
                    .ToList();

                _view.ShowPendingApprovals(pending);
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
            if (_user == null)
            {
                _view.ShowError("ไม่พบข้อมูล User — กรุณาล็อกอินใหม่");
                return;
            }

            if (_currentFolder == null)
            {
                try
                {
                    var folder = _folderService.GetFolder(request.FolderID, _user);
                    if (folder != null)
                        _currentFolder = folder;
                    else
                    {
                        _view.ShowError(string.Format("ไม่พบ Folder ID: {0}", request.FolderID));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _view.ShowError(string.Format("ไม่สามารถโหลด Folder ID {0}: {1}", request.FolderID, ex.Message));
                    return;
                }
            }

            try
            {
                _permissionService.SetFolderPermission(request, _user);
                if (_auditService != null)
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "Permission",
                        Action      = "SET_FOLDER_PERMISSION",
                        UserID      = _user.UserID,
                        TableName   = "FolderPermission",
                        RecordID    = _currentFolder.FolderID,
                        NewValue    = BuildPermissionRemark(request),
                        MachineName = _user.MachineName,
                        IPAddress   = _user.IPAddress,
                        SessionID   = _user.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                LoadAclEntries(_currentFolder);
                PromptPropagateToFiles();
            }
            catch (UnauthorizedAccessException ex) { _view.ShowError("Access denied: " + ex.Message); }
            catch (InvalidOperationException ex)   { _view.ShowError("Cannot set permission: " + ex.Message); }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FOREIGN KEY") || ex.Message.Contains("FK_"))
                    _view.ShowError("Principal ID ไม่มีอยู่ในระบบ หรือข้อมูลไม่ถูกต้อง\n\n" + ex.Message);
                else
                    _view.ShowError("Failed to set permission: " + ex.Message);
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
                _folderPermissionLookupController.Revoke(permissionId, _user.UserID, "Removed by user");
                if (_auditService != null)
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "Permission",
                        Action      = "REMOVE_FOLDER_PERMISSION",
                        UserID      = _user.UserID,
                        TableName   = "FolderPermission",
                        RecordID    = _currentFolder.FolderID,
                        OldValue    = string.Format("PermissionID:{0}", permissionId),
                        MachineName = _user.MachineName,
                        IPAddress   = _user.IPAddress,
                        SessionID   = _user.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                LoadAclEntries(_currentFolder);
                _view.ShowInfo("ลบรายการสิทธิ์เรียบร้อยแล้ว");
            }
            catch (UnauthorizedAccessException ex) { _view.ShowError("ไม่มีสิทธิ์ดำเนินการ: " + ex.Message); }
            catch (Exception ex)                   { _view.ShowError("ไม่สามารถลบสิทธิ์ได้: " + ex.Message); }
        }

        public void ChangeConfidentialityLevel(int folderId, int newLevel)
        {
            try
            {
                _folderService.ChangeConfidentialityLevel(folderId, newLevel, _user);
                if (_auditService != null)
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "Permission",
                        Action      = "CHANGE_CONF_LEVEL",
                        UserID      = _user.UserID,
                        TableName   = "Folder",
                        RecordID    = folderId,
                        OldValue    = _currentFolder != null ? string.Format("Level:{0}", _currentFolder.ConfidentialityLevel) : null,
                        NewValue    = string.Format("Level:{0}", newLevel),
                        MachineName = _user.MachineName,
                        IPAddress   = _user.IPAddress,
                        SessionID   = _user.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                _view.ShowInfo(string.Format(
                    "Confidentiality level changed to {0} and propagated to all inheriting descendants.",
                    ConfidentialityLabel(newLevel)));

                if (_currentFolder != null && _currentFolder.FolderID == folderId)
                {
                    _currentFolder.ConfidentialityLevel = newLevel;
                    _view.ShowConfidentialityLevel(newLevel);
                }
            }
            catch (UnauthorizedAccessException ex) { _view.ShowError("Access denied: " + ex.Message); }
            catch (InvalidOperationException ex)   { _view.ShowError("Cannot change confidentiality level: " + ex.Message); }
            catch (Exception ex)                   { _view.ShowError("Failed to change confidentiality level: " + ex.Message); }
        }

        private static string BuildPermissionRemark(SetPermissionRequest req)
        {
            return string.Format(
                "FolderID:{0} PrincipalType:{1} PrincipalID:{2} Read:{3} Write:{4} Delete:{5} Modify:{6} FullControl:{7} IsDeny:{8}",
                req.FolderID, req.PrincipalType, req.PrincipalID,
                req.CanRead, req.CanWrite, req.CanDelete, req.CanModify, req.CanFullControl, req.IsDeny);
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

        private void PromptPropagateToFiles()
        {
            if (_currentFolder == null || _fileDocumentLookupController == null)
            {
                _view.ShowInfo("✅ บันทึกสิทธิ์เรียบร้อยแล้ว");
                return;
            }

            int fileCount;
            try { fileCount = _fileDocumentLookupController.GetByFolder(_currentFolder.FolderID).Count(); }
            catch { fileCount = 0; }

            if (fileCount == 0)
            {
                _view.ShowInfo("✅ บันทึกสิทธิ์เรียบร้อยแล้ว\n\n(ไม่มีไฟล์ใน Folder นี้)");
                return;
            }

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
