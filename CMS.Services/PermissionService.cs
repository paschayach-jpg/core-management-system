using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Implements IPermissionService: evaluates effective permissions using
    /// IsDeny-first evaluation and parent-folder inheritance traversal.
    /// PrincipalType: 1=User, 2=Role, 3=OrgUnit, 4=Group
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository     _permissionRepository;
        private readonly IFolderRepository         _folderRepository;
        private readonly IUserAccountRepository    _userAccountRepository;
        private readonly IAuditService             _auditService;
        private readonly IFileDocumentRepository   _fileDocumentRepository;
        private readonly IFilePermissionRepository _filePermissionRepository;

        // Session-level permission cache: key = "userId:folderId:permType"
        // ลด DB calls จาก N+1 เหลือ 1 ต่อ folder ต่อ session
        private readonly Dictionary<string, bool> _permCache = new Dictionary<string, bool>();
        // ACL entries cache: key = folderId
        private readonly Dictionary<int, List<FolderPermission>> _aclCache = new Dictionary<int, List<FolderPermission>>();
        // Folder object cache: key = folderId — ใช้ระหว่าง recursion traversal
        private readonly Dictionary<int, Folder> _folderCache = new Dictionary<int, Folder>();
        // User account cache: key = userId
        private readonly Dictionary<int, UserAccount> _userAccountCache = new Dictionary<int, UserAccount>();

        public PermissionService(
            IPermissionRepository     permissionRepository,
            IFolderRepository         folderRepository,
            IUserAccountRepository    userAccountRepository,
            IAuditService             auditService             = null,
            IFileDocumentRepository   fileDocumentRepository   = null,
            IFilePermissionRepository filePermissionRepository = null)
        {
            if (permissionRepository  == null) throw new ArgumentNullException("permissionRepository");
            if (folderRepository      == null) throw new ArgumentNullException("folderRepository");
            if (userAccountRepository == null) throw new ArgumentNullException("userAccountRepository");

            _permissionRepository     = permissionRepository;
            _folderRepository         = folderRepository;
            _userAccountRepository    = userAccountRepository;
            _auditService             = auditService;
            _fileDocumentRepository   = fileDocumentRepository;
            _filePermissionRepository = filePermissionRepository;
        }

        public EffectivePermission GetEffectivePermission(int userId, int folderId)
        {
            return new EffectivePermission
            {
                UserId         = userId,
                FolderId       = folderId,
                CanRead        = EvaluatePermission(userId, folderId, "Read"),
                CanWrite       = EvaluatePermission(userId, folderId, "Write"),
                CanDelete      = EvaluatePermission(userId, folderId, "Delete"),
                CanModify      = EvaluatePermission(userId, folderId, "Modify"),
                HasFullControl = EvaluatePermission(userId, folderId, "FullControl")
            };
        }

        public void SetFolderPermission(SetPermissionRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");

            // Manager=3, Admin=4
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("Setting folder permissions requires Manager or Admin role.");

            var folder = _folderRepository.GetById(request.FolderID);
            if (folder == null)
                throw new InvalidOperationException(string.Format(
                    "Folder with ID {0} was not found.", request.FolderID));

            // Revoke existing entries for the same principal+folder+isDeny type to avoid duplicates
            var existing = _permissionRepository.GetAclEntries(request.FolderID)
                .Where(e => e.PrincipalType == request.PrincipalType
                         && e.PrincipalID   == request.PrincipalID
                         && e.IsDeny        == request.IsDeny)
                .ToList();

            foreach (var old in existing)
                _permissionRepository.Revoke(old.PermissionID, user.UserID, "Replaced by new permission entry");

            var entry = new FolderPermission
            {
                FolderID          = request.FolderID,
                PrincipalType     = request.PrincipalType,
                PrincipalID       = request.PrincipalID,
                CanRead           = request.CanRead,
                CanWrite          = request.CanWrite,
                CanDelete         = request.CanDelete,
                CanModify         = request.CanModify,
                CanFullControl    = request.CanFullControl,
                IsDeny            = request.IsDeny,
                InheritanceBroken = request.BreakInheritance,
                GrantedBy         = user.UserID,
                GrantedDate       = DateTime.Now
            };

            _permissionRepository.Insert(entry);

            if (_auditService != null)
            {
                _auditService.Log(new AuditEntry
                {
                    Module      = "Permission",
                    Action      = request.IsDeny ? "DENY" : "GRANT",
                    UserID      = user.UserID,
                    UserName    = user.ADUserName ?? string.Empty,
                    UserRole    = user.AppRole,
                    TableName   = "security.FolderPermission",
                    RecordID    = request.FolderID,
                    NewValue    = string.Format("Principal {0} (type {1}) on folder {2}.",
                                      request.PrincipalID, request.PrincipalType, request.FolderID),
                    MachineName = user.MachineName,
                    IPAddress   = user.IPAddress,
                    SessionID   = user.SessionID,
                    Result      = 1,
                    AuditDate   = DateTime.Now
                });
            }
        }
        public int PropagatePermissionsToFiles(int folderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("Propagating permissions requires Manager or Admin role.");

            if (_fileDocumentRepository == null || _filePermissionRepository == null)
                return 0;

            // Get all active folder permissions to propagate
            var folderPerms = _permissionRepository.GetByFolder(folderId)
                .Where(p => p.RevokedDate == null
                         && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today))
                .ToList();

            var files = _fileDocumentRepository.GetByFolder(folderId).ToList();
            if (files.Count == 0) return 0;

            foreach (var file in files)
            {
                // Revoke all existing active file permissions for this file
                var existingFilePerms = _filePermissionRepository.GetByFile(file.FileID).ToList();
                foreach (var fp in existingFilePerms)
                    _filePermissionRepository.Revoke(fp.FilePermissionID, user.UserID, "Propagated from folder permission");

                // Insert new file permissions mirroring folder permissions
                foreach (var fp in folderPerms)
                {
                    _filePermissionRepository.Insert(new FilePermission
                    {
                        FileID         = file.FileID,
                        PrincipalType  = (int)fp.PrincipalType,
                        PrincipalID    = fp.PrincipalID,
                        CanRead        = fp.CanRead,
                        CanWrite       = fp.CanWrite,
                        CanDelete      = fp.CanDelete,
                        CanModify      = fp.CanModify,
                        CanFullControl = fp.CanFullControl,
                        IsDeny         = fp.IsDeny,
                        ExpiryDate     = fp.ExpiryDate,
                        GrantedBy      = user.UserID,
                        GrantedDate    = DateTime.Now
                    });
                }
            }

            if (_auditService != null)
                _auditService.Log(new AuditEntry
                {
                    Module      = "Permission",
                    Action      = "PROPAGATE_TO_FILES",
                    UserID      = user.UserID,
                    UserName    = user.ADUserName ?? string.Empty,
                    UserRole    = user.AppRole,
                    TableName   = "security.FilePermission",
                    RecordID    = folderId,
                    NewValue    = string.Format("Propagated {0} permission(s) to {1} file(s) in folder {2}.",
                                      folderPerms.Count, files.Count, folderId),
                    MachineName = user.MachineName,
                    IPAddress   = user.IPAddress,
                    SessionID   = user.SessionID,
                    Result      = 1,
                    AuditDate   = DateTime.Now
                });

            return files.Count;
        }


        public bool CheckPermission(int userId, int folderId, string permissionType)
        {
            string cacheKey = string.Format("{0}:{1}:{2}", userId, folderId, permissionType);
            bool cached;
            if (_permCache.TryGetValue(cacheKey, out cached)) return cached;

            bool result = CheckPermissionInternal(userId, folderId, permissionType);
            _permCache[cacheKey] = result;
            return result;
        }

        private bool CheckPermissionInternal(int userId, int folderId, string permissionType)
        {
            var account = GetCachedUserAccount(userId);

            // Admin (4) and Manager (3): check only for explicit DENY (one DB call), then bypass
            if (account != null && account.AppRole >= 3)
            {
                int? userRole  = (int?)account.AppRole;
                var allEntries = GetCachedAclEntries(folderId);
                var applicable = allEntries.Where(e =>
                    (e.PrincipalType == PrincipalType.User       && e.PrincipalID == userId) ||
                    (e.PrincipalType == PrincipalType.Role       && e.PrincipalID == userRole.Value) ||
                    (e.PrincipalType == PrincipalType.Department && account.DepartmentId.HasValue && e.PrincipalID == account.DepartmentId.Value) ||
                    (e.PrincipalType == PrincipalType.Division   && account.DivisionId.HasValue   && e.PrincipalID == account.DivisionId.Value)
                ).ToList();

                bool hasDeny = applicable.Any(e => e.IsDeny && MatchesFlag(e, permissionType));
                if (hasDeny) return false;

                return true; // Admin/Manager bypass (no explicit deny found)
            }

            // Regular users: EvaluatePermission handles DENY + ALLOW + inheritance
            return EvaluatePermission(userId, folderId, permissionType);
        }

        private UserAccount GetCachedUserAccount(int userId)
        {
            UserAccount account;
            if (!_userAccountCache.TryGetValue(userId, out account))
            {
                account = _userAccountRepository.GetById(userId);
                _userAccountCache[userId] = account;
            }
            return account;
        }

        private Folder GetCachedFolder(int folderId)
        {
            Folder folder;
            if (!_folderCache.TryGetValue(folderId, out folder))
            {
                folder = _folderRepository.GetById(folderId);
                _folderCache[folderId] = folder;
            }
            return folder;
        }

        /// <summary>Cache ACL entries per folder to avoid repeated DB calls in N+1 scenarios.</summary>
        private List<FolderPermission> GetCachedAclEntries(int folderId)
        {
            List<FolderPermission> entries;
            if (!_aclCache.TryGetValue(folderId, out entries))
            {
                entries = _permissionRepository.GetAclEntries(folderId).ToList();
                _aclCache[folderId] = entries;
            }
            return entries;
        }

        /// <summary>
        /// Pre-warm _aclCache จาก data ที่โหลดตอน login ครั้งเดียว
        /// ทำให้ทุก CheckPermission ใน session นี้เป็น dictionary lookup ไม่มี DB call
        /// </summary>
        public void PreloadAcl(Dictionary<int, List<FolderPermission>> sessionAcl)
        {
            if (sessionAcl == null) return;
            foreach (var kv in sessionAcl)
                _aclCache[kv.Key] = kv.Value;
        }

        /// <summary>Invalidate cache for a folder (call after permission changes).</summary>
        public void InvalidateCache(int folderId)
        {
            _aclCache.Remove(folderId);
            _folderCache.Remove(folderId);
            var keysToRemove = _permCache.Keys
                .Where(k => k.StartsWith(":" + folderId + ":") || k.Contains(":" + folderId + ":"))
                .ToList();
            foreach (var k in keysToRemove) _permCache.Remove(k);
        }

        /// <summary>Invalidate user account cache (call after user data changes).</summary>
        public void InvalidateUserCache(int userId)
        {
            _userAccountCache.Remove(userId);
            var keysToRemove = _permCache.Keys
                .Where(k => k.StartsWith(userId + ":"))
                .ToList();
            foreach (var k in keysToRemove) _permCache.Remove(k);
        }

        /// <summary>Overload that accepts PermissionType enum.</summary>
        public bool CheckPermission(int userId, int folderId, CMS.Domain.Enums.PermissionType permissionType)
        {
            return CheckPermission(userId, folderId, permissionType.ToString());
        }

        // -----------------------------------------------------------------------
        // Core evaluation
        // -----------------------------------------------------------------------

        private bool EvaluatePermission(int userId, int folderId, string permissionType)
        {
            var userAccount = GetCachedUserAccount(userId);
            var visited = new HashSet<int>();
            return EvaluatePermissionRecursive(userId, folderId, permissionType, visited, userAccount);
        }

        private bool EvaluatePermissionRecursive(
            int userId, int folderId, string permissionType, HashSet<int> visited, UserAccount userAccount)
        {
            if (visited.Contains(folderId)) return false;
            visited.Add(folderId);

            var folder = GetCachedFolder(folderId);
            if (folder == null) return false;

            var allEntries = GetCachedAclEntries(folderId);

            int? userRole = userAccount != null ? (int?)userAccount.AppRole : null;

            // Entries applicable to this user:
            //   • User     : ตรงกับ UserID โดยตรง
            //   • Role     : ตรงกับ AppRole (1=Officer, 2=SrOfficer, 3=Manager, 4=Admin)
            //   • Department: ตรงกับ DepartmentId ของ user (จาก Employee API)
            //   • Division  : ตรงกับ DivisionId  ของ user (จาก Employee API)
            var applicable = allEntries.Where(e =>
                (e.PrincipalType == PrincipalType.User       && e.PrincipalID == userId) ||
                (e.PrincipalType == PrincipalType.Role       && userRole.HasValue && e.PrincipalID == userRole.Value) ||
                (e.PrincipalType == PrincipalType.Department && userAccount != null && userAccount.DepartmentId.HasValue && e.PrincipalID == userAccount.DepartmentId.Value) ||
                (e.PrincipalType == PrincipalType.Division   && userAccount != null && userAccount.DivisionId.HasValue   && e.PrincipalID == userAccount.DivisionId.Value)
            ).ToList();

            // IsDeny overrides everything
            bool hasDeny = applicable.Any(e => e.IsDeny && MatchesFlag(e, permissionType));
            if (hasDeny) return false;

            // Any allow entry grants access
            bool hasAllow = applicable.Any(e => !e.IsDeny && MatchesFlag(e, permissionType));
            if (hasAllow) return true;

            // Inherit from parent unless any ACL entry has InheritanceBroken = true
            bool inheritanceBroken = allEntries.Any(e => e.InheritanceBroken);
            if (!inheritanceBroken && folder.ParentFolderID.HasValue)
                return EvaluatePermissionRecursive(userId, folder.ParentFolderID.Value, permissionType, visited, userAccount);

            return false; // default deny
        }

        private static bool MatchesFlag(FolderPermission entry, string permissionType)
        {
            if (entry.CanFullControl) return true;

            switch (permissionType)
            {
                case "Read":
                    // View: เห็น folder/file
                    return entry.CanRead;

                case "Write":
                    // Add/Edit: สร้าง sub-folder, upload, rename file/sub-folder
                    return entry.CanWrite || entry.CanModify;

                case "Delete":
                    // Delete: ลบได้เฉพาะ "ไฟล์" เท่านั้น
                    return entry.CanDelete || entry.CanModify;

                case "Modify":
                    // Modify: ลบ "Folder" และจัดการโครงสร้าง (สิทธิ์สูงสุด รองจาก FullControl)
                    return entry.CanModify;

                case "FullControl":
                    return entry.CanFullControl;

                default:
                    return false;
            }
        }
    }
}
