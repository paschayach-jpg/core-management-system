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
    public class FolderService : IFolderService
    {
        private const int MaxFolderDepth = 10;
        private static readonly int[] MainRootConfidentialityLevels = { 1, 2, 3, 4 };

        private static readonly char[] InvalidFolderNameChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

        private static void ValidateFolderName(string name)
        {
            var found = name.Where(c => InvalidFolderNameChars.Contains(c)).Distinct().ToArray();
            if (found.Length > 0)
                throw new ArgumentException(
                    string.Format("ชื่อ Folder ไม่อนุญาตให้ใช้อักขระ: {0}", string.Join(" ", found)));
        }

        private static bool IsRootFolder(Folder folder)
        {
            return folder != null && (!folder.ParentFolderID.HasValue || folder.ParentFolderID.Value == 0);
        }

        private static bool IsMainRootLevel(int confidentialityLevel)
        {
            return MainRootConfidentialityLevels.Contains(confidentialityLevel);
        }

        private static string GetMainRootCode(int confidentialityLevel)
        {
            switch (confidentialityLevel)
            {
                case 1: return "1HC";
                case 2: return "2SC";
                case 3: return "3NC";
                case 4: return "4PC";
                default: return string.Empty;
            }
        }

        private static bool RootNameMatchesLevel(string folderName, int confidentialityLevel)
        {
            string expectedCode = GetMainRootCode(confidentialityLevel);
            if (string.IsNullOrWhiteSpace(expectedCode) || string.IsNullOrWhiteSpace(folderName))
                return false;

            return folderName.Trim().StartsWith(expectedCode, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMainRootFolder(Folder folder)
        {
            return IsRootFolder(folder)
                && IsMainRootLevel(folder.ConfidentialityLevel)
                && RootNameMatchesLevel(folder.FolderName, folder.ConfidentialityLevel);
        }

        private static bool IsMatchingCreateHcRequest(ApprovalRequest request, int orgUnitId, string folderName)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Remark) || string.IsNullOrWhiteSpace(folderName))
                return false;

            return request.Remark.Contains(string.Format("OrgUnitId={0}", orgUnitId))
                && request.Remark.Contains(string.Format("FolderName={0}", folderName.Trim()));
        }

        private Folder GetRootAncestor(Folder folder)
        {
            var current = folder;
            var visitedFolderIds = new HashSet<int>();

            while (current != null && current.ParentFolderID.HasValue && current.ParentFolderID.Value > 0)
            {
                if (!visitedFolderIds.Add(current.FolderID))
                    throw new InvalidOperationException("Folder hierarchy contains a cycle.");

                current = _folderRepository.GetById(current.ParentFolderID.Value);
            }

            return current;
        }

        private void EnsureRootCreationAllowed(string folderName, int orgUnitId, int? confidentialityLevel)
        {
            if (!confidentialityLevel.HasValue || !IsMainRootLevel(confidentialityLevel.Value))
                throw new InvalidOperationException(
                    "ไม่อนุญาตให้สร้าง Root Folder ใหม่โดยตรง ยกเว้น 4 Folder หลัก (1HC, 2SC, 3NC, 4PC)");

            string expectedCode = GetMainRootCode(confidentialityLevel.Value);
            if (!RootNameMatchesLevel(folderName, confidentialityLevel.Value))
                throw new InvalidOperationException(
                    string.Format("Root Folder ระดับ {0} ต้องขึ้นต้นด้วย '{0}'", expectedCode));

            var rootFolders = _folderRepository.GetRootFoldersByOrgUnit(orgUnitId)
                .Where(f => f != null && !f.IsDeleted && !f.IsArchived)
                .ToList();

            // 1HC ข้าม duplicate check เพราะอาจมีอยู่แต่ invisible (รอ approval / ยังไม่มี ACL)
            // การส่ง approval request ซ้ำจะถูก block ใน alreadyPending check ด้านล่างแทน
            if (confidentialityLevel.Value != 1)
            {
                if (rootFolders.Any(f => f.ConfidentialityLevel == confidentialityLevel.Value))
                    throw new InvalidOperationException(
                        string.Format("มี Root Folder หลักระดับ {0} อยู่แล้ว", expectedCode));
            }

            int mainRootCount = rootFolders.Count(f => IsMainRootLevel(f.ConfidentialityLevel) && f.ConfidentialityLevel != 1);
            if (mainRootCount >= MainRootConfidentialityLevels.Length - 1 && confidentialityLevel.Value != 1)
                throw new InvalidOperationException(
                    "ไม่อนุญาตให้สร้าง Root Folder เพิ่มนอก 4 Folder หลัก (1HC, 2SC, 3NC, 4PC)");
        }

        private void EnsureCreationUnderMainRoots(Folder parentFolder)
        {
            var rootFolder = GetRootAncestor(parentFolder);
            if (!IsMainRootFolder(rootFolder))
                throw new InvalidOperationException(
                    "อนุญาตให้สร้าง Folder ภายใต้ 4 Folder หลัก (1HC, 2SC, 3NC, 4PC) เท่านั้น");
        }

        private readonly IFolderRepository _folderRepository;
        private readonly IFileDocumentRepository _fileDocumentRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IPermissionService _permissionService;
        private readonly INetworkDriveHelper _networkDriveHelper;
        private readonly IAuditService _auditService;
        private readonly IFolderTemplateRepository _folderTemplateRepository;
        private readonly IApprovalRepository _approvalRepository;
        private readonly IUserOrgVisibilityService _visibilityService;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IFolderQuotaService _quotaService;

        // Session-level folder cache: key = orgUnitId (null = all), value = folder list
        // ลด DB calls ซ้ำเมื่อ navigate ไปมาใน session เดียวกัน
        private readonly Dictionary<int, List<Folder>> _folderCacheByOrg = new Dictionary<int, List<Folder>>();
        private List<Folder> _allFoldersCache = null;
        private static readonly int _cacheKeyAll = -1; // sentinel for "all orgunits"

        /// <summary>Invalidate folder cache (เรียกหลัง create/rename/delete/move)</summary>
        public void InvalidateFolderCache(int? orgUnitId = null)
        {
            // _allFoldersCache ครอบคลุมทุก OrgUnit ดังนั้นต้อง clear เสมอ
            // ไม่ว่าจะ invalidate OrgUnit ใดก็ตาม
            _allFoldersCache = null;

            if (orgUnitId.HasValue)
                _folderCacheByOrg.Remove(orgUnitId.Value);
            else
                _folderCacheByOrg.Clear();
        }

        public FolderService(
            IFolderRepository folderRepository,
            IFileDocumentRepository fileDocumentRepository,
            IPermissionRepository permissionRepository,
            IPermissionService permissionService,
            INetworkDriveHelper networkDriveHelper,
            IAuditService auditService,
            IFolderTemplateRepository folderTemplateRepository = null,
            IApprovalRepository approvalRepository = null,
            IUserOrgVisibilityService visibilityService = null,
            IUserAccountRepository userAccountRepository = null,
            IFolderQuotaService quotaService = null)
        {
            if (folderRepository == null) throw new ArgumentNullException("folderRepository");
            if (fileDocumentRepository == null) throw new ArgumentNullException("fileDocumentRepository");
            if (permissionRepository == null) throw new ArgumentNullException("permissionRepository");
            if (permissionService == null) throw new ArgumentNullException("permissionService");
            if (networkDriveHelper == null) throw new ArgumentNullException("networkDriveHelper");
            if (auditService == null) throw new ArgumentNullException("auditService");

            _folderRepository = folderRepository;
            _fileDocumentRepository = fileDocumentRepository;
            _permissionRepository = permissionRepository;
            _permissionService = permissionService;
            _networkDriveHelper = networkDriveHelper;
            _auditService = auditService;
            _folderTemplateRepository = folderTemplateRepository;
            _approvalRepository = approvalRepository;
            _visibilityService = visibilityService;
            _userAccountRepository = userAccountRepository;
            _quotaService = quotaService;
        }

        public Folder CreateFolder(CreateFolderRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Folder name is required.");

            ValidateFolderName(request.Name.Trim());

            if (request.ParentFolderId.HasValue)
            {
                bool isPrivileged = user.AppRole >= 3;
                if (!isPrivileged)
                {
                    var parent = _folderRepository.GetById(request.ParentFolderId.Value);

                    // เงื่อนไขที่ 1: FolderAuthorityConfig — CanAdd ตาม LevelCode/ตำแหน่ง/แผนก/ฝ่าย
                    bool hasAuthority = parent != null && user.CanAddToFolder(parent.ConfidentialityLevel);

                    // เงื่อนไขที่ 2: FolderPermission — Write โดยใช้ UserAccount (User, Role, Dept, Div)
                    bool hasAclWrite = !hasAuthority && _permissionService.CheckPermission(user.UserID, request.ParentFolderId.Value, "Write");

                    if (!hasAuthority && !hasAclWrite)
                        throw new UnauthorizedAccessException(
                            string.Format("ไม่มีสิทธิ์สร้าง Folder ใน '{0}' (ต้องการสิทธิ์ Add)", request.ParentFolderId.Value));
                }
            }

            // Duplicate check: compare case-insensitive + trimmed against active siblings
            // หมายเหตุ: 1HC ข้ามการ check ตรงนี้ เพราะจะถูก handle ใน approval flow ด้านล่าง
            // (อาจมี 1HC เก่าที่ invisible เพราะยังไม่มี ACL — ต้องส่ง approval ใหม่ให้ Admin เห็น)
            string trimmedName = request.Name.Trim();
            int resolvedConfidentiality = request.ConfidentialityLevel ?? 4;
            if (resolvedConfidentiality != 1)
            {
                var siblings = _folderRepository.GetSiblings(request.ParentFolderId, request.OrgUnitId);
                var duplicate = siblings.FirstOrDefault(
                    f => string.Equals(f.FolderName.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));
                if (duplicate != null)
                    throw new InvalidOperationException(
                        string.Format("มี Folder ชื่อ '{0}' อยู่แล้วใน Parent เดียวกัน", duplicate.FolderName));
            }

            int depth;
            int confidentiality;
            bool inheritPermission;

            if (request.ParentFolderId.HasValue)
            {
                var parent = _folderRepository.GetById(request.ParentFolderId.Value);
                if (parent == null)
                    throw new InvalidOperationException(string.Format(
                        "Parent folder with ID {0} was not found.", request.ParentFolderId.Value));
                if (parent.IsDeleted)
                    throw new InvalidOperationException("Cannot create a folder under a deleted parent folder.");
                if (parent.IsArchived)
                    throw new InvalidOperationException("Cannot create a folder under an archived parent folder.");

                EnsureCreationUnderMainRoots(parent);

                depth = parent.FolderDepth + 1;
                if (depth > MaxFolderDepth)
                    throw new InvalidOperationException("Maximum folder depth of 10 has been reached.");

                if (request.ConfidentialityLevel.HasValue)
                {
                    confidentiality = request.ConfidentialityLevel.Value;
                    inheritPermission = false;
                }
                else
                {
                    confidentiality = parent.ConfidentialityLevel;
                    inheritPermission = true;
                }
            }
            else
            {
                EnsureRootCreationAllowed(trimmedName, request.OrgUnitId, request.ConfidentialityLevel);

                depth = 1;  // root folder อยู่ที่ depth 1 (ไม่มี parent)
                confidentiality = request.ConfidentialityLevel ?? 4; // 4=Public
                inheritPermission = !request.ConfidentialityLevel.HasValue;
            }

            // HC/SC/NC/PC folders are created immediately.
            var folder = new Folder
            {
                ParentFolderID = request.ParentFolderId,
                OrgUnitID = request.OrgUnitId,
                FolderName = request.Name.Trim(),
                FolderDepth = depth,
                ConfidentialityLevel = confidentiality,
                InheritPermission = inheritPermission,
                IsDeleted = false,
                IsArchived = false,
                CreatedBy = user.UserID,
                CreatedDate = DateTime.Now
            };

            if (_quotaService != null)
            {
                _quotaService.EnforceQuota(folder.OrgUnitID);
                _quotaService.SendAlertIfNeeded(folder.OrgUnitID);
            }

            int newId = _folderRepository.Insert(folder);
            folder.FolderID = newId;

            InvalidateFolderCache(folder.OrgUnitID);
            Log(user, "Folder", "CREATE", "storage.Folder", newId, 1,
                string.Format("Created folder '{0}' at depth {1}.", folder.FolderName, depth));

            return folder;
        }

        /// <summary>
        /// Creates an HC folder after approval. Called by ApprovalService when CREATE_HC request is approved.
        /// Parses folder data from approval request Remark and creates the actual folder.
        /// </summary>
        public Folder CreateHCFolderAfterApproval(ApprovalRequest approvalRequest, UserContext user)
        {
            if (approvalRequest == null) throw new ArgumentNullException("approvalRequest");
            if (user == null) throw new ArgumentNullException("user");
            if (approvalRequest.RequestType != "CREATE_HC")
                throw new InvalidOperationException("This method only handles CREATE_HC approval requests.");

            // Parse folder data from Remark (format: ParentFolderId=X|OrgUnitId=Y|FolderName=Z|...)
            int? parentFolderId = null;
            int orgUnitId = 0;
            string folderName = "";
            int depth = 1;
            int confidentiality = 1;
            bool inheritPermission = false;

            try
            {
                var parts = approvalRequest.Remark.Split('|');
                var dict = new Dictionary<string, string>();
                foreach (var part in parts)
                {
                    var kv = part.Split('=');
                    if (kv.Length == 2)
                        dict[kv[0]] = kv[1];
                }

                parentFolderId = dict.ContainsKey("ParentFolderId") && int.TryParse(dict["ParentFolderId"], out int pf) && pf > 0 ? (int?)pf : null;
                orgUnitId = dict.ContainsKey("OrgUnitId") && int.TryParse(dict["OrgUnitId"], out int ou) ? ou : 0;
                folderName = dict.ContainsKey("FolderName") ? dict["FolderName"] : "";
                depth = dict.ContainsKey("FolderDepth") && int.TryParse(dict["FolderDepth"], out int d) ? d : 1;
                confidentiality = dict.ContainsKey("ConfidentialityLevel") && int.TryParse(dict["ConfidentialityLevel"], out int c) ? c : 1;
                inheritPermission = dict.ContainsKey("InheritPermission") && bool.TryParse(dict["InheritPermission"], out bool ip) ? ip : false;

                if (orgUnitId == 0 || string.IsNullOrEmpty(folderName))
                    throw new FormatException("Missing required folder data");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Format("Failed to parse folder data from approval request {0}: {1}",
                        approvalRequest.RequestID, ex.Message), ex);
            }

            // Validate parent folder still exists and is not deleted/archived
            if (parentFolderId.HasValue)
            {
                var parent = _folderRepository.GetById(parentFolderId.Value);
                if (parent == null)
                    throw new InvalidOperationException(
                        string.Format("Parent folder with ID {0} no longer exists.", parentFolderId.Value));
                if (parent.IsDeleted)
                    throw new InvalidOperationException("Cannot create folder under a deleted parent.");
                if (parent.IsArchived)
                    throw new InvalidOperationException("Cannot create folder under an archived parent.");

                EnsureCreationUnderMainRoots(parent);
            }
            else
            {
                EnsureRootCreationAllowed(folderName.Trim(), orgUnitId, confidentiality);
            }

            // Check for duplicate name again (in case it was created while approval was pending)
            var siblings = _folderRepository.GetSiblings(parentFolderId, orgUnitId);
            var duplicate = siblings.FirstOrDefault(
                f => string.Equals(f.FolderName.Trim(), folderName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (duplicate != null)
                throw new InvalidOperationException(
                    string.Format("A folder named '{0}' already exists in the same parent.", folderName));

            // Create the folder
            var folder = new Folder
            {
                ParentFolderID = parentFolderId,
                OrgUnitID = orgUnitId,
                FolderName = folderName,
                FolderDepth = depth,
                ConfidentialityLevel = confidentiality,
                InheritPermission = inheritPermission,
                IsDeleted = false,
                IsArchived = false,
                CreatedBy = approvalRequest.RequestedBy,
                CreatedDate = DateTime.Now
            };

            int newId = _folderRepository.Insert(folder);
            folder.FolderID = newId;

            // ให้ผู้ยื่นคำขอเห็นโฟลเดอร์ที่สร้างแล้วทันที (แก้ปัญหา default-deny เมื่อยังไม่มี ACL)
            try
            {
                if (_permissionService != null && approvalRequest.RequestedBy > 0)
                {
                    _permissionService.SetFolderPermission(new SetPermissionRequest
                    {
                        FolderID = newId,
                        PrincipalType = CMS.Domain.Enums.PrincipalType.User,
                        PrincipalID = approvalRequest.RequestedBy,
                        CanRead = true,
                        CanWrite = false,
                        CanDelete = false,
                        CanModify = false,
                        CanFullControl = false,
                        IsDeny = false,
                        BreakInheritance = false
                    }, user);
                }
            }
            catch (Exception ex)
            {
                Log(user, "Folder", "GRANT_READ_FAILED", "security.FolderPermission", newId, 0,
                    string.Format("Failed to grant requester read permission for HC folder '{0}': {1}", folderName, ex.Message));
            }

            InvalidateFolderCache(folder.OrgUnitID);
            Log(user, "Folder", "APPROVE", "storage.Folder", newId, 1,
                string.Format("CREATE_HC_APPROVED: HC Folder '{0}' created after approval of request {1}.", folderName, approvalRequest.RequestID));

            return folder;

        }

        public void RenameFolder(int folderId, string newName, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("New folder name is required.");

            ValidateFolderName(newName.Trim());

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));
            if (folder.IsDeleted)
                throw new InvalidOperationException("Cannot rename a deleted folder.");

            // Edit permission: CanWrite required
            // ยกเว้น: ห้ามแก้ไขชื่อ 4 Root Folders หลัก (depth=1, ไม่มี parent)
            bool isRootFolder = !folder.ParentFolderID.HasValue || folder.ParentFolderID == 0;
            if (isRootFolder)
                throw new InvalidOperationException(
                    string.Format("ไม่อนุญาตให้เปลี่ยนชื่อ Root Folder หลัก '{0}'", folder.FolderName));

            bool isPrivileged = user.AppRole >= 3;
            if (!isPrivileged)
            {
                // เงื่อนไขที่ 1: FolderAuthorityConfig — CanEdit ตาม LevelCode/ตำแหน่ง/แผนก/ฝ่าย
                bool hasAuthority = user.CanEditFolder(folder.ConfidentialityLevel);

                // เงื่อนไขที่ 2: FolderPermission — Write โดยใช้ UserAccount (User, Role, Dept, Div)
                bool hasAclWrite = !hasAuthority && _permissionService.CheckPermission(user.UserID, folderId, "Write");

                if (!hasAuthority && !hasAclWrite)
                    throw new UnauthorizedAccessException(
                        string.Format("ไม่มีสิทธิ์เปลี่ยนชื่อ Folder '{0}' (ต้องการสิทธิ์ Edit)", folder.FolderName));
            }

            // Duplicate check: compare case-insensitive + trimmed against active siblings
            string trimmedName = newName.Trim();
            var siblings = _folderRepository.GetSiblings(folder.ParentFolderID, folder.OrgUnitID);
            var duplicate = siblings.FirstOrDefault(
                f => f.FolderID != folderId &&
                     string.Equals(f.FolderName.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));
            if (duplicate != null)
                throw new InvalidOperationException(
                    string.Format("มี Folder ชื่อ '{0}' อยู่แล้วใน Parent เดียวกัน", duplicate.FolderName));

            string oldName = folder.FolderName;
            folder.FolderName = newName.Trim();
            _folderRepository.Update(folder);

            InvalidateFolderCache(folder.OrgUnitID);
            Log(user, "Folder", "UPDATE", "storage.Folder", folderId, 1,
                string.Format("Renamed from '{0}' to '{1}'.", oldName, newName.Trim()));
        }

        public void ArchiveFolder(int folderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));
            if (folder.IsDeleted)
                throw new InvalidOperationException("Cannot archive a deleted folder.");

            folder.IsArchived = true;
            folder.ArchivedBy = user.UserID;
            folder.ArchivedDate = DateTime.Now;
            _folderRepository.Update(folder);

            var descendants = _folderRepository.GetDescendants(folderId).ToList();
            foreach (var descendant in descendants)
            {
                descendant.IsArchived = true;
                descendant.ArchivedBy = user.UserID;
                descendant.ArchivedDate = DateTime.Now;
                _folderRepository.Update(descendant);
            }

            // Archive all FileDocuments in the folder and its descendants
            var allFolders = new List<Folder>(descendants) { folder };
            foreach (var f in allFolders)
            {
                var files = _fileDocumentRepository.GetByFolder(f.FolderID).ToList();
                foreach (var file in files)
                {
                    if (file.FileStatus != 3) // Don't archive already deleted files
                    {
                        file.FileStatus = 2; // 2=Archived
                        file.ArchivedBy = user.UserID;
                        file.ArchivedDate = DateTime.Now;
                        _fileDocumentRepository.Update(file);
                    }
                }
            }

            Log(user, "Folder", "UPDATE", "storage.Folder", folderId, 1,
                string.Format("Archived folder '{0}' and all descendants.", folder.FolderName));
        }

        public void DeleteFolder(int folderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));
            if (folder.IsDeleted)
                throw new InvalidOperationException("Folder is already in the recycle bin.");

            // Modify permission: CanModify required (ลบ folder — สิทธิ์สูงสุด)
            // Root Folders หลัก — เฉพาะ Admin (AppRole >= 3) เท่านั้นที่ลบได้
            bool isRootFolder = !folder.ParentFolderID.HasValue || folder.ParentFolderID == 0;
            bool isPrivileged = user.AppRole >= 3;
            if (isRootFolder && !isPrivileged)
                throw new InvalidOperationException(
                    string.Format("ไม่อนุญาตให้ลบ Root Folder หลัก '{0}'", folder.FolderName));
            if (!isPrivileged)
            {
                // เงื่อนไขที่ 1: FolderAuthorityConfig — CanEdit ตาม LevelCode/ตำแหน่ง/แผนก/ฝ่าย
                bool hasAuthority = user.CanEditFolder(folder.ConfidentialityLevel);

                // เงื่อนไขที่ 2: FolderPermission — Modify โดยใช้ UserAccount (User, Role, Dept, Div)
                bool hasAclModify = !hasAuthority && _permissionService.CheckPermission(user.UserID, folderId, "Modify");

                if (!hasAuthority && !hasAclModify)
                    throw new UnauthorizedAccessException(
                        string.Format("ไม่มีสิทธิ์ลบ Folder '{0}' (ต้องการสิทธิ์ Modify)", folder.FolderName));
            }

            // Req 5.1–5.3: folders with files must go through Admin approval workflow
            if (folder.FileCount > 0)
                throw new InvalidOperationException(
                    string.Format(
                        "ไม่สามารถลบ Folder '{0}' ได้โดยตรง เนื่องจากมีไฟล์อยู่ {1} รายการ\nกรุณายื่นคำขออนุมัติการลบผ่านระบบ Workflow",
                        folder.FolderName, folder.FileCount));

            _folderRepository.SoftDelete(folderId, user.UserID);

            InvalidateFolderCache(folder.OrgUnitID);
            Log(user, "Folder", "DELETE", "storage.Folder", folderId, 1,
                string.Format("Moved folder '{0}' to recycle bin.", folder.FolderName));
        }

        private const int RecycleBinRetentionDays = 30;

        public void RestoreFolder(int folderId, int? newParentId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));
            if (!folder.IsDeleted)
                throw new InvalidOperationException("Folder is not in the recycle bin.");

            if (folder.DeletedAt.HasValue &&
                folder.DeletedAt.Value < DateTime.Now.AddDays(-RecycleBinRetentionDays))
                throw new InvalidOperationException(string.Format(
                    "Folder '{0}' ถูกลบเมื่อ {1} วันที่แล้ว เกินระยะเวลากู้คืน {2} วัน",
                    folder.FolderName,
                    (int)(DateTime.Now - folder.DeletedAt.Value).TotalDays,
                    RecycleBinRetentionDays));

            if (newParentId == null)
            {
                if (folder.ParentFolderID.HasValue)
                {
                    var originalParent = _folderRepository.GetById(folder.ParentFolderID.Value);
                    if (originalParent == null || originalParent.IsDeleted)
                        throw new InvalidOperationException(
                            "The original parent folder no longer exists. Please select an alternative parent folder.");
                }
            }
            else
            {
                var newParent = _folderRepository.GetById(newParentId.Value);
                if (newParent == null)
                    throw new InvalidOperationException(string.Format(
                        "Alternative parent folder with ID {0} was not found.", newParentId.Value));
                if (newParent.IsDeleted)
                    throw new InvalidOperationException("The specified alternative parent folder is in the recycle bin.");

                int restoredDepth = newParent.FolderDepth + 1;
                if (restoredDepth > MaxFolderDepth)
                    throw new InvalidOperationException("Maximum folder depth of 10 has been reached.");

                folder.ParentFolderID = newParentId.Value;
                _folderRepository.Update(folder);
            }

            _folderRepository.Restore(folderId);

            Log(user, "Folder", "UPDATE", "storage.Folder", folderId, 1,
                string.Format("Restored folder '{0}'.", folder.FolderName));
        }

        public void PermanentlyDeleteFolder(int folderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Manager=3, Admin=4
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("Permanently deleting a folder requires Manager or Admin role.");

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));
            if (!folder.IsDeleted)
                throw new InvalidOperationException("Folder must be in the recycle bin before it can be permanently deleted.");

            try
            {
                // Step 1: Get all descendant folders (including the target folder)
                var descendants = _folderRepository.GetDescendants(folderId).ToList();
                var allFolders = new List<Folder>(descendants) { folder };

                // Step 2: Manually delete all files and related data to avoid FK constraints
                foreach (var f in allFolders.OrderByDescending(x => x.FolderDepth)) // Delete deepest first
                {
                    try
                    {
                        // Get ALL files in this folder (including soft-deleted) to avoid FK constraint
                        var files = _fileDocumentRepository.GetByFolder(f.FolderID).ToList();
                        // Also delete any soft-deleted files via direct hard-delete by folder
                        try { _fileDocumentRepository.DeleteAllByFolder(f.FolderID); }
                        catch { /* fallback to per-file delete below */ }

                        foreach (var file in files)
                        {
                            // Delete physical file if it exists
                            if (!string.IsNullOrEmpty(file.PhysicalPath) && _networkDriveHelper.FileExists(file.PhysicalPath))
                            {
                                try { _networkDriveHelper.DeleteFile(file.PhysicalPath); }
                                catch (Exception ex)
                                {
                                    Log(user, "File", "DELETE_PHYSICAL_ERROR", "FileSystem", file.FileID, 0,
                                        string.Format("Failed to delete physical file '{0}': {1}", file.PhysicalPath, ex.Message));
                                }
                            }

                            // Delete file record from database (best-effort — DeleteAllByFolder may have already done it)
                            try { _fileDocumentRepository.PermanentDelete(file.FileID); }
                            catch { }
                        }

                        // Delete approval requests and steps for this folder
                        try
                        {
                            if (_approvalRepository != null)
                                _approvalRepository.DeleteByFolder(f.FolderID);
                        }
                        catch (Exception ex)
                        {
                            Log(user, "Approval", "DELETE_ERROR", "workflow.ApprovalRequest", f.FolderID, 0,
                                string.Format("Failed to delete approval requests for folder '{0}': {1}", f.FolderName, ex.Message));
                        }

                        // Delete folder permissions
                        try
                        {
                            _permissionRepository.DeleteByFolder(f.FolderID);
                        }
                        catch (Exception ex)
                        {
                            Log(user, "Permission", "DELETE_ERROR", "security.FolderPermission", f.FolderID, 0,
                                string.Format("Failed to delete permissions for folder '{0}': {1}", f.FolderName, ex.Message));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(user, "Folder", "DELETE_CONTENT_ERROR", "storage.Folder", f.FolderID, 0,
                            string.Format("Error processing folder '{0}': {1}", f.FolderName, ex.Message));
                    }
                }

                // Step 3: Now delete folders from deepest to shallowest
                foreach (var f in allFolders.OrderByDescending(x => x.FolderDepth))
                {
                    try
                    {
                        _folderRepository.PermanentDelete(f.FolderID);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            string.Format("Failed to delete folder '{0}' (ID: {1}): {2}", f.FolderName, f.FolderID, ex.Message),
                            ex);
                    }
                }

                // Step 4: Log successful deletion
                Log(user, "Folder", "DELETE", "storage.Folder", folderId, 1,
                    string.Format("Permanently deleted folder '{0}' and all descendants.", folder.FolderName));
            }
            catch (Exception ex)
            {
                // Log the error and re-throw with more context
                Log(user, "Folder", "DELETE_ERROR", "storage.Folder", folderId, 0,
                    string.Format("Failed to permanently delete folder '{0}': {1}", folder.FolderName, ex.Message));

                throw new InvalidOperationException(
                    string.Format("ไม่สามารถลบ Folder '{0}' ได้: {1}", folder.FolderName, ex.Message),
                    ex);
            }
        }

        public void ApplyTemplate(int parentFolderId, int templateId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("Applying a template requires Manager or Admin role.");

            var parentFolder = _folderRepository.GetById(parentFolderId);
            if (parentFolder == null)
                throw new InvalidOperationException(string.Format("Parent folder with ID {0} was not found.", parentFolderId));
            if (parentFolder.IsDeleted)
                throw new InvalidOperationException("Cannot apply a template to a deleted folder.");

            var templateFolder = _folderRepository.GetById(templateId);
            if (templateFolder == null)
                throw new InvalidOperationException(string.Format("Template folder with ID {0} was not found.", templateId));

            CopyTemplateChildren(templateId, parentFolderId, parentFolder.FolderDepth, parentFolder.OrgUnitID, user);

            Log(user, "Folder", "CREATE", "storage.Folder", parentFolderId, 1,
                string.Format("Applied template {0} to folder '{1}'.", templateId, parentFolder.FolderName));
        }

        public IEnumerable<Folder> GetFolderContents(int folderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));
            if (folder.IsDeleted)
                throw new InvalidOperationException("Cannot list contents of a deleted folder.");

            bool isPrivileged = user.AppRole >= 3;
            if (!isPrivileged)
            {
                // เงื่อนไขที่ 1: ตรวจ FolderAuthorityConfig ด้วย LevelCode/ตำแหน่ง/แผนก/ฝ่าย ของ user
                //   → ถ้า user.Authority มี ConfidentialityLevel ตรงกับ folder และ CanView = true → ผ่าน
                bool hasAuthority = user.CanViewFolder(folder.ConfidentialityLevel);

                // เงื่อนไขที่ 2: ตรวจ FolderPermission โดยใช้ UserAccount (UserID, Role, DepartmentId, DivisionId)
                //   → CheckPermission ตรวจ entry ที่ตรงกับ user ทั้งแบบ direct (User), Role, แผนก, ฝ่าย
                bool hasAclRead = !hasAuthority && _permissionService.CheckPermission(user.UserID, folderId, "Read");

                if (!hasAuthority && !hasAclRead)
                    throw new UnauthorizedAccessException(string.Format(
                        "User {0} does not have permission to view folder {1}.", user.UserID, folderId));
            }

            return _folderRepository.GetChildren(folderId)
                .Where(f => !f.IsDeleted && !f.IsArchived);
        }

        public Folder GetFolder(int folderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return _folderRepository.GetById(folderId);
        }

        public void ChangeConfidentialityLevel(int folderId, int newLevel, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("Changing confidentiality level requires Manager or Admin role.");

            if (newLevel < 1 || newLevel > 4)
                throw new ArgumentOutOfRangeException("newLevel", "Confidentiality level must be between 1 and 4.");

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));

            // BR-001-07: Conf-Root folders are locked — cannot change their confidentiality level.
            if (folder.ParentFolderID == null)
                throw new InvalidOperationException("Conf-Root folder ระดับความลับถูก Lock ไม่สามารถเปลี่ยนได้");

            // Business Rule: Only direct children of a conf-root may change level.
            // Deeper sub-folders must follow their parent (cannot change independently).
            var parent = _folderRepository.GetById(folder.ParentFolderID.Value);
            if (parent == null || parent.ParentFolderID != null)
                throw new InvalidOperationException(
                    "เปลี่ยนระดับความลับได้เฉพาะ Folder ที่อยู่ภายใต้ Conf-Root โดยตรงเท่านั้น");

            // Move folder to the conf-root of the requested level within the same OrgUnit.
            var confRoots = _folderRepository.GetRootFoldersByOrgUnit(folder.OrgUnitID);
            var newRoot = confRoots.FirstOrDefault(r => r.ConfidentialityLevel == newLevel);
            if (newRoot == null)
                throw new InvalidOperationException(
                    string.Format("ไม่พบ Conf-Root ระดับ {0} ใน OrgUnit นี้", newLevel));

            folder.ParentFolderID = newRoot.FolderID;
            folder.ConfidentialityLevel = newLevel;
            folder.InheritPermission = false;
            _folderRepository.Update(folder);

            foreach (var descendant in _folderRepository.GetDescendants(folderId))
            {
                if (descendant.InheritPermission)
                {
                    descendant.ConfidentialityLevel = newLevel;
                    _folderRepository.Update(descendant);
                }
            }

            Log(user, "Folder", "UPDATE", "storage.Folder", folderId, 1,
                string.Format("Changed confidentiality level of folder '{0}' to {1}, moved under root '{2}'.",
                    folder.FolderName, newLevel, newRoot.FolderName));
        }

        public IEnumerable<Folder> GetRootFolders(int? orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Resolve visible OrgUnit IDs via visibility service (Req 5.1, 5.4)
            HashSet<int> visibleIds = null;
            if (_visibilityService != null)
            {
                visibleIds = new HashSet<int>(
                    _visibilityService.GetVisibleOrgUnits(user.UserID, user)
                                      .Select(o => o.OrgUnitID));
            }

            IEnumerable<Folder> roots = orgUnitId.HasValue
                ? _folderRepository.GetRootFoldersByOrgUnit(orgUnitId.Value)
                : _folderRepository.GetAllRootFolders();

            roots = roots.Where(f => !f.IsDeleted);

            // Filter to visible OrgUnits when service is wired (Req 5.2, 5.3)
            if (visibleIds != null)
                roots = roots.Where(f => visibleIds.Contains(f.OrgUnitID));

            // กรองเฉพาะ folder ที่ user มีสิทธิ์ Read
            // - Admin/Manager: ผ่านโดยอัตโนมัติ (ยกเว้น Explicit Deny) ผ่าน CheckPermission
            // - Officer/SrOfficer: อนุญาตเมื่อผ่าน Authority model หรือ ACL
            return roots.Where(f =>
            {
                bool isPrivileged = user.AppRole >= 3;
                if (isPrivileged)
                    return _permissionService.CheckPermission(user.UserID, f.FolderID, "Read");

                bool hasAuthority = user.CanViewFolder(f.ConfidentialityLevel);
                bool hasAclRead = _permissionService.CheckPermission(user.UserID, f.FolderID, "Read");
                return hasAuthority || hasAclRead;
            });
        }

        public IEnumerable<Folder> GetDeletedFolders(int? orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var cutoff = DateTime.Now.AddDays(-RecycleBinRetentionDays);

            IEnumerable<Folder> folders = orgUnitId.HasValue
                ? _folderRepository.GetDeletedByOrgUnit(orgUnitId.Value)
                : _folderRepository.GetAllDeletedFolders();

            // แสดงเฉพาะ folder ที่ถูกลบภายใน 30 วัน (ยังกู้คืนได้)
            return folders.Where(f => !f.DeletedAt.HasValue || f.DeletedAt.Value >= cutoff);
        }

        public IEnumerable<Folder> GetAllFolders(int? orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (orgUnitId.HasValue)
            {
                List<Folder> cached;
                if (!_folderCacheByOrg.TryGetValue(orgUnitId.Value, out cached))
                {
                    cached = _folderRepository.GetByOrgUnit(orgUnitId.Value).ToList();
                    _folderCacheByOrg[orgUnitId.Value] = cached;
                }
                return cached;
            }

            if (_allFoldersCache == null)
                _allFoldersCache = _folderRepository.GetAll().ToList();
            return _allFoldersCache;
        }

        /// <summary>
        /// ค้นหา Folder ของ OrgUnit ที่เลือกและ parent OrgUnit ทั้งหมด
        /// ใช้เมื่อเลือก Department เพื่อแสดง Folder ของ Department, Division, SubCommittee
        /// </summary>
        public IEnumerable<Folder> GetFoldersByOrgUnitAndParents(int orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // ค้นหา Folder ของ OrgUnit ที่เลือก
            var folders = _folderRepository.GetByOrgUnit(orgUnitId).ToList();

            // ถ้าไม่มี Folder ให้ค้นหา Folder ของ parent OrgUnit ด้วย
            // (ในกรณีที่ Department ไม่มี Folder แต่ Division หรือ SubCommittee มี)
            if (folders.Count == 0)
            {
                // ค้นหา Folder ของ parent OrgUnit ทั้งหมด
                // โดยค้นหา Folder ที่มี OrgUnitID ไม่เท่ากับ Department แต่อยู่ใน hierarchy เดียวกัน
                var allFolders = _folderRepository.GetAll().ToList();

                // ค้นหา Folder ที่อาจเป็นของ parent OrgUnit
                // (ในกรณีที่ Department ไม่มี Folder แต่ Division หรือ SubCommittee มี)
                folders = allFolders
                    .Where(f => !f.IsDeleted && !f.IsArchived)
                    .ToList();
            }

            return folders;
        }

        public IEnumerable<Folder> GetFoldersByOrgUnit(int orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            // reuse GetAllFolders cache
            return GetAllFolders(orgUnitId, user);
        }

        public void ApplyTemplateToOrgUnit(int orgUnitId, int templateId, string rootFolderName, int confidentialityLevel, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // ตรวจสิทธิ์ตาม FolderAuthorityConfig (CanAdd) — เหมือน CreateFolder
            int clampedLevel = (confidentialityLevel >= 1 && confidentialityLevel <= 4) ? confidentialityLevel : 4;
            bool hasAuthority = user.CanAddToFolder(clampedLevel);
            if (!hasAuthority)
                throw new UnauthorizedAccessException(
                    string.Format("ไม่มีสิทธิ์สร้าง Folder ระดับ {0} (ต้องการสิทธิ์ Add)", clampedLevel));

            if (_folderTemplateRepository == null)
                throw new InvalidOperationException("FolderTemplateRepository is not configured.");

            var items = _folderTemplateRepository.GetItems(templateId).ToList();
            if (items.Count == 0)
                throw new InvalidOperationException("Template นี้ไม่มี Sub-Folder กำหนดไว้");

            // Clamp confidentialityLevel to valid range 1-4
            if (confidentialityLevel < 1 || confidentialityLevel > 4)
                confidentialityLevel = 4;

            EnsureRootCreationAllowed(rootFolderName.Trim(), orgUnitId, confidentialityLevel);

            // สร้าง root folder ของ OrgUnit
            var rootFolder = new Folder
            {
                ParentFolderID = null,
                OrgUnitID = orgUnitId,
                FolderName = rootFolderName.Trim(),
                FolderDepth = 1,
                ConfidentialityLevel = confidentialityLevel,
                InheritPermission = false,
                IsDeleted = false,
                IsArchived = false,
                CreatedBy = user.UserID,
                CreatedDate = DateTime.Now
            };
            int rootId = _folderRepository.Insert(rootFolder);

            // สร้าง sub-folders ตาม template items (recursive)
            CreateFoldersFromItems(items, null, rootId, 1, orgUnitId, confidentialityLevel, user);

            Log(user, "Folder", "CREATE", "storage.Folder", rootId, 1,
                string.Format("สร้าง Folder '{0}' จาก Template ID {1} สำหรับ OrgUnit {2}", rootFolderName, templateId, orgUnitId));
        }

        /// <summary>
        /// สร้าง sub-folders จาก Template ภายใต้ folder ที่มีอยู่แล้ว
        /// ใช้เมื่อต้องการสร้างภายใต้ 1HC/2SC/3NC/4PC
        /// </summary>
        public void ApplyTemplateToFolder(int parentFolderId, int templateId, int confidentialityLevel, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (_folderTemplateRepository == null)
                throw new InvalidOperationException("FolderTemplateRepository is not configured.");

            var parentFolder = _folderRepository.GetById(parentFolderId);
            if (parentFolder == null)
                throw new InvalidOperationException(string.Format("Parent folder with ID {0} was not found.", parentFolderId));
            if (parentFolder.IsDeleted)
                throw new InvalidOperationException("Cannot apply a template to a deleted folder.");

            // ตรวจสิทธิ์ตาม FolderAuthorityConfig (CanAdd) หรือ ACL Write — เหมือน CreateFolder
            int targetLevel = (confidentialityLevel >= 1 && confidentialityLevel <= 4)
                ? confidentialityLevel
                : parentFolder.ConfidentialityLevel;
            bool hasAuthority = user.CanAddToFolder(targetLevel);
            bool hasAclWrite = !hasAuthority && _permissionService.CheckPermission(user.UserID, parentFolderId, "Write");
            if (!hasAuthority && !hasAclWrite)
                throw new UnauthorizedAccessException(
                    string.Format("ไม่มีสิทธิ์สร้าง Folder ใน '{0}' (ต้องการสิทธิ์ Add)", parentFolder.FolderName));

            var items = _folderTemplateRepository.GetItems(templateId).ToList();
            if (items.Count == 0)
                throw new InvalidOperationException("Template นี้ไม่มี Sub-Folder กำหนดไว้");

            if (confidentialityLevel < 1 || confidentialityLevel > 4)
                confidentialityLevel = parentFolder.ConfidentialityLevel;

            // สร้าง sub-folders ตาม template items ภายใต้ parentFolder
            CreateFoldersFromItems(items, null, parentFolderId, parentFolder.FolderDepth, parentFolder.OrgUnitID, confidentialityLevel, user);

            Log(user, "Folder", "CREATE", "storage.Folder", parentFolderId, 1,
                string.Format("สร้าง Sub-Folders จาก Template ID {0} ภายใต้ Folder '{1}'", templateId, parentFolder.FolderName));
        }

        public void ApplyTemplateToFolder(int parentFolderId, IList<FolderTemplateItem> items, int confidentialityLevel, UserContext user)
        {
            if (user  == null) throw new ArgumentNullException("user");
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("ไม่มี Sub-Folder กำหนดไว้");

            var parentFolder = _folderRepository.GetById(parentFolderId);
            if (parentFolder == null)
                throw new InvalidOperationException(string.Format("Parent folder with ID {0} was not found.", parentFolderId));
            if (parentFolder.IsDeleted)
                throw new InvalidOperationException("Cannot apply a template to a deleted folder.");

            int targetLevel = (confidentialityLevel >= 1 && confidentialityLevel <= 4)
                ? confidentialityLevel
                : parentFolder.ConfidentialityLevel;
            bool hasAuthority = user.CanAddToFolder(targetLevel);
            bool hasAclWrite  = !hasAuthority && _permissionService.CheckPermission(user.UserID, parentFolderId, "Write");
            if (!hasAuthority && !hasAclWrite)
                throw new UnauthorizedAccessException(
                    string.Format("ไม่มีสิทธิ์สร้าง Folder ใน '{0}' (ต้องการสิทธิ์ Add)", parentFolder.FolderName));

            if (confidentialityLevel < 1 || confidentialityLevel > 4)
                confidentialityLevel = parentFolder.ConfidentialityLevel;

            CreateFoldersFromItems(items, null, parentFolderId, parentFolder.FolderDepth, parentFolder.OrgUnitID, confidentialityLevel, user);

            Log(user, "Folder", "CREATE", "storage.Folder", parentFolderId, 1,
                string.Format("สร้าง Sub-Folders จาก Custom Items ภายใต้ Folder '{0}'", parentFolder.FolderName));
        }

        public void ApplyTemplateToOrgUnit(int orgUnitId, IList<FolderTemplateItem> items, string rootFolderName, int confidentialityLevel, UserContext user)
        {
            if (user  == null) throw new ArgumentNullException("user");
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("ไม่มี Sub-Folder กำหนดไว้");

            int clampedLevel = (confidentialityLevel >= 1 && confidentialityLevel <= 4) ? confidentialityLevel : 4;
            bool hasAuthority = user.CanAddToFolder(clampedLevel);
            if (!hasAuthority)
                throw new UnauthorizedAccessException(
                    string.Format("ไม่มีสิทธิ์สร้าง Folder ระดับ {0} (ต้องการสิทธิ์ Add)", clampedLevel));

            if (confidentialityLevel < 1 || confidentialityLevel > 4)
                confidentialityLevel = 4;

            EnsureRootCreationAllowed(rootFolderName.Trim(), orgUnitId, confidentialityLevel);

            var rootFolder = new Folder
            {
                ParentFolderID      = null,
                OrgUnitID           = orgUnitId,
                FolderName          = rootFolderName.Trim(),
                FolderDepth         = 1,
                ConfidentialityLevel = confidentialityLevel,
                InheritPermission   = false,
                IsDeleted           = false,
                IsArchived          = false,
                CreatedBy           = user.UserID,
                CreatedDate         = DateTime.Now
            };
            int rootId = _folderRepository.Insert(rootFolder);

            CreateFoldersFromItems(items, null, rootId, 1, orgUnitId, confidentialityLevel, user);

            Log(user, "Folder", "CREATE", "storage.Folder", rootId, 1,
                string.Format("สร้าง Folder '{0}' จาก Custom Items สำหรับ OrgUnit {1}", rootFolderName, orgUnitId));
        }

        private void CreateFoldersFromItems(
            IList<FolderTemplateItem> allItems,
            int? parentItemId,
            int parentFolderId,
            int parentDepth,
            int orgUnitId,
            int confidentialityLevel,
            UserContext user)
        {
            var children = allItems
                .Where(i => i.ParentItemID == parentItemId)
                .OrderBy(i => i.SortOrder)
                .ToList();

            foreach (var item in children)
            {
                int depth = parentDepth + 1;
                if (depth > MaxFolderDepth) continue;

                int itemConfidentiality = (item.InheritPermission || item.ConfidentialityLevel < 1 || item.ConfidentialityLevel > 4)
                    ? confidentialityLevel
                    : item.ConfidentialityLevel;

                var folder = new Folder
                {
                    ParentFolderID = parentFolderId,
                    OrgUnitID = orgUnitId,
                    FolderName = item.FolderName,
                    FolderDepth = depth,
                    ConfidentialityLevel = itemConfidentiality,
                    InheritPermission = item.InheritPermission,
                    IsDeleted = false,
                    IsArchived = false,
                    CreatedBy = user.UserID,
                    CreatedDate = DateTime.Now
                };

                int newId = _folderRepository.Insert(folder);
                CreateFoldersFromItems(allItems, item.ItemID, newId, depth, orgUnitId, itemConfidentiality, user);
            }
        }

        public void MoveFolder(MoveFolderRequest request, UserContext user)
        {
            // 3.1 — Null check
            if (request == null) throw new ArgumentNullException("request");
            if (user == null) throw new ArgumentNullException("user");

            int sourceFolderId = request.SourceFolderId;
            int destinationParentFolderId = request.DestinationParentFolderId;

            // 3.2 — Load source and destination
            var source = _folderRepository.GetById(sourceFolderId);
            var destination = _folderRepository.GetById(destinationParentFolderId);

            if (source == null)
                throw new InvalidOperationException(
                    string.Format("ไม่พบ Folder ต้นทาง ID {0}", sourceFolderId));
            if (destination == null)
                throw new InvalidOperationException(
                    string.Format("ไม่พบ Folder ปลายทาง ID {0}", destinationParentFolderId));

            // 3.1 — Permission check: role gate
            if (user.AppRole < 2)
                throw new UnauthorizedAccessException("ต้องมีสิทธิ์ระดับ SrOfficer ขึ้นไป");

            // 3.1 — Permission check on source and destination (skip for Admin)
            if (user.AppRole < 4)
            {
                bool hasSourceAuthority = user.CanAddToFolder(source.ConfidentialityLevel)
                                       || user.CanEditFolder(source.ConfidentialityLevel);
                bool hasSourceAcl = _permissionService.CheckPermission(user.UserID, sourceFolderId, "Write")
                                 || _permissionService.CheckPermission(user.UserID, sourceFolderId, "Modify");
                if (!hasSourceAuthority && !hasSourceAcl)
                    throw new UnauthorizedAccessException(
                        string.Format("ไม่มีสิทธิ์ย้ายจาก Folder ต้นทาง (ID: {0})", sourceFolderId));

                bool hasDestinationAuthority = user.CanAddToFolder(destination.ConfidentialityLevel)
                                            || user.CanEditFolder(destination.ConfidentialityLevel);
                bool hasDestinationAcl = _permissionService.CheckPermission(user.UserID, destinationParentFolderId, "Write")
                                      || _permissionService.CheckPermission(user.UserID, destinationParentFolderId, "Modify");
                if (!hasDestinationAuthority && !hasDestinationAcl)
                    throw new UnauthorizedAccessException("ไม่มีสิทธิ์ย้ายไปยัง Folder ปลายทาง");
            }

            // 3.2 — Circular reference: self-move
            if (sourceFolderId == destinationParentFolderId)
                throw new InvalidOperationException("ไม่สามารถย้าย Folder ไปยังตัวเองได้");

            // 3.2 — Circular reference: destination is a descendant of source
            // sp_GetFolderDescendants is a recursive CTE whose anchor row is the folder itself.
            // Exclude it: the stale copy would otherwise be written back after the move and
            // revert source.ParentFolderID to the old parent.
            var descendants = _folderRepository.GetDescendants(sourceFolderId)
                .Where(d => d.FolderID != sourceFolderId)
                .ToList();
            if (descendants.Any(d => d.FolderID == destinationParentFolderId))
                throw new InvalidOperationException("ไม่สามารถย้าย Folder ไปยัง Descendant ของตัวเองได้");

            // 3.2 — Depth limit check
            int maxDescendantDepth = descendants.Any()
                ? descendants.Max(d => d.FolderDepth)
                : source.FolderDepth;
            int subtreeHeight = maxDescendantDepth - source.FolderDepth;
            int projectedDepth = destination.FolderDepth + 1 + subtreeHeight;
            if (projectedDepth > MaxFolderDepth)
                throw new InvalidOperationException(
                    string.Format("การย้ายจะทำให้ depth เป็น {0} ซึ่งเกิน MaxFolderDepth ({1})",
                                  projectedDepth, MaxFolderDepth));

            // 3.3 — Duplicate name check (active siblings only; GetChildren returns non-deleted)
            var siblings = _folderRepository.GetChildren(destinationParentFolderId).ToList();
            if (siblings.Any(s => string.Equals(s.FolderName.Trim(), source.FolderName.Trim(),
                                                  StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException(
                    string.Format("มี Folder ชื่อ '{0}' อยู่แล้วใน Folder ปลายทาง", source.FolderName));

            // 3.3 / 3.4 — Record old values for audit and depth delta
            int? oldParentFolderId = source.ParentFolderID;
            int originalSourceDepth = source.FolderDepth;
            int depthDelta = (destination.FolderDepth + 1) - originalSourceDepth;

            // 3.3 — Update source folder
            int newConfidentialityLevel = destination.ConfidentialityLevel; // inherit from destination
            int newOrgUnitId = destination.OrgUnitID;            // inherit OrgUnit from destination

            // sp_UpdateFolder does not write OrgUnitID — set it explicitly on a cross-OrgUnit move,
            // otherwise the folder ends up under another unit's tree with its old OrgUnitID and
            // disappears from both units. Done first so a failure here leaves nothing half-moved.
            if (source.OrgUnitID != newOrgUnitId)
            {
                _folderRepository.UpdateOrgUnit(sourceFolderId, newOrgUnitId, user.UserID);
                foreach (var descendant in descendants)
                    _folderRepository.UpdateOrgUnit(descendant.FolderID, newOrgUnitId, user.UserID);
            }

            source.ParentFolderID = destinationParentFolderId;
            source.FolderDepth = destination.FolderDepth + 1;
            source.ConfidentialityLevel = newConfidentialityLevel;
            source.OrgUnitID = newOrgUnitId;
            source.PhysicalPath = _networkDriveHelper.BuildPath(newOrgUnitId, source.FolderID, source.FolderName);
            source.UpdatedBy = user.UserID;
            source.UpdatedDate = DateTime.Now;
            _folderRepository.Update(source);

            // Verify the move was persisted (sp_UpdateFolder must write ParentFolderID / ConfidentialityLevel)
            var persisted = _folderRepository.GetById(sourceFolderId);
            if (persisted == null
                || persisted.ParentFolderID != destinationParentFolderId
                || persisted.ConfidentialityLevel != newConfidentialityLevel
                || persisted.OrgUnitID != newOrgUnitId)
            {
                InvalidateFolderCache();
                throw new InvalidOperationException(string.Format(
                    "ย้าย Folder ไม่สำเร็จ: ข้อมูลใน DB ไม่ตรง (Parent={0}, Level={1}, OrgUnit={2}; คาดว่า Parent={3}, Level={4}, OrgUnit={5})",
                    persisted == null ? "-" : (persisted.ParentFolderID.HasValue ? persisted.ParentFolderID.Value.ToString() : "NULL"),
                    persisted == null ? "-" : persisted.ConfidentialityLevel.ToString(),
                    persisted == null ? "-" : persisted.OrgUnitID.ToString(),
                    destinationParentFolderId, newConfidentialityLevel, newOrgUnitId));
            }

            // 3.4 — Update all descendants
            foreach (var descendant in descendants)
            {
                descendant.FolderDepth += depthDelta;
                descendant.ConfidentialityLevel = newConfidentialityLevel;
                descendant.OrgUnitID = newOrgUnitId;
                descendant.PhysicalPath = _networkDriveHelper.BuildPath(newOrgUnitId, descendant.FolderID, descendant.FolderName);
                descendant.UpdatedBy = user.UserID;
                descendant.UpdatedDate = DateTime.Now;
                _folderRepository.Update(descendant);
            }

            // 3.4 — Update PhysicalPath of all FileDocuments in source + descendants
            var allFolderIds = new List<int> { sourceFolderId };
            allFolderIds.AddRange(descendants.Select(d => d.FolderID));

            foreach (int folderId in allFolderIds)
            {
                var files = _fileDocumentRepository.GetByFolder(folderId).ToList();
                foreach (var file in files)
                {
                    if (file.FileStatus == 1) // Active only
                    {
                        file.PhysicalPath = _networkDriveHelper.BuildPath(newOrgUnitId, file.FolderID, file.StoredFileName);
                        _fileDocumentRepository.Update(file);
                    }
                }
            }

            // 3.5 — Audit log
            InvalidateFolderCache(); // invalidate all — folder moved across OrgUnits possibly
            _auditService.Log(new AuditEntry
            {
                Module = "Folder",
                Action = "MoveFolder",
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                UserRole = user.AppRole,
                TableName = "storage.Folder",
                RecordID = sourceFolderId,
                OldValue = oldParentFolderId.HasValue ? oldParentFolderId.Value.ToString() : null,
                NewValue = destinationParentFolderId.ToString(),
                MachineName = user.MachineName,
                IPAddress = user.IPAddress,
                SessionID = user.SessionID,
                Result = 1,
                AuditDate = DateTime.Now
            });
        }

        public void CopyFolderStructure(int sourceFolderId, int destinationParentFolderId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("ต้องมีสิทธิ์ Manager หรือ Admin เพื่อคัดลอกโครงสร้าง Folder");

            var source = _folderRepository.GetById(sourceFolderId);
            if (source == null)
                throw new InvalidOperationException(string.Format("ไม่พบ Folder ID {0}", sourceFolderId));

            var destinationParent = _folderRepository.GetById(destinationParentFolderId);
            if (destinationParent == null)
                throw new InvalidOperationException(string.Format("ไม่พบ Destination Parent Folder ID {0}", destinationParentFolderId));

            int newDepth = destinationParent.FolderDepth + 1;
            if (newDepth > MaxFolderDepth)
                throw new InvalidOperationException(
                    string.Format("ไม่สามารถคัดลอก Folder ได้ เนื่องจาก depth จะเกิน MaxFolderDepth ({0})", MaxFolderDepth));

            var newRoot = new Folder
            {
                ParentFolderID = destinationParentFolderId,
                OrgUnitID = destinationParent.OrgUnitID,
                FolderName = source.FolderName,
                FolderDepth = newDepth,
                ConfidentialityLevel = source.ConfidentialityLevel,
                InheritPermission = source.InheritPermission,
                IsDeleted = false,
                IsArchived = false,
                CreatedBy = user.UserID,
                CreatedDate = DateTime.Now
            };

            int newRootId = _folderRepository.Insert(newRoot);

            CopyFolderStructureChildren(sourceFolderId, newRootId, newDepth, destinationParent.OrgUnitID, user);

            Log(user, "Folder", "COPY_STRUCTURE", "storage.Folder", newRootId, 1,
                string.Format("คัดลอกโครงสร้าง Folder จาก ID {0} ไปยัง Parent ID {1}", sourceFolderId, destinationParentFolderId));
        }

        private void CopyFolderStructureChildren(int sourceFolderId, int newParentId, int parentDepth, int orgUnitId, UserContext user)
        {
            foreach (var child in _folderRepository.GetChildren(sourceFolderId))
            {
                int childDepth = parentDepth + 1;
                if (childDepth > MaxFolderDepth)
                    throw new InvalidOperationException(
                        string.Format("ไม่สามารถคัดลอก Folder ได้ เนื่องจาก depth จะเกิน MaxFolderDepth ({0})", MaxFolderDepth));

                var newFolder = new Folder
                {
                    ParentFolderID = newParentId,
                    OrgUnitID = orgUnitId,
                    FolderName = child.FolderName,
                    FolderDepth = childDepth,
                    ConfidentialityLevel = child.ConfidentialityLevel,
                    InheritPermission = child.InheritPermission,
                    IsDeleted = false,
                    IsArchived = false,
                    CreatedBy = user.UserID,
                    CreatedDate = DateTime.Now
                };

                int newId = _folderRepository.Insert(newFolder);
                CopyFolderStructureChildren(child.FolderID, newId, childDepth, orgUnitId, user);
            }
        }

        private void CopyTemplateChildren(int templateFolderId, int newParentId, int parentDepth, int orgUnitId, UserContext user)
        {
            foreach (var child in _folderRepository.GetChildren(templateFolderId))
            {
                int childDepth = parentDepth + 1;
                if (childDepth > MaxFolderDepth) continue;

                var newFolder = new Folder
                {
                    ParentFolderID = newParentId,
                    OrgUnitID = orgUnitId,
                    FolderName = child.FolderName,
                    FolderDepth = childDepth,
                    ConfidentialityLevel = child.ConfidentialityLevel,
                    InheritPermission = child.InheritPermission,
                    IsDeleted = false,
                    IsArchived = false,
                    CreatedBy = user.UserID,
                    CreatedDate = DateTime.Now
                };

                int newId = _folderRepository.Insert(newFolder);
                newFolder.FolderID = newId;

                CopyTemplateChildren(child.FolderID, newId, childDepth, orgUnitId, user);
            }
        }

        public IEnumerable<Folder> GetDescendants(int folderId)
        {
            return _folderRepository.GetDescendants(folderId);
        }

        private void Log(UserContext user, string module, string action, string tableName,
                         int recordId, int result, string newValue = null)
        {
            _auditService.Log(new AuditEntry
            {
                Module = module,
                Action = action,
                UserID = user.UserID,
                UserName = user.ADUserName ?? string.Empty,
                UserRole = user.AppRole,
                TableName = tableName,
                RecordID = recordId,
                NewValue = newValue,
                MachineName = user.MachineName,
                IPAddress = user.IPAddress,
                SessionID = user.SessionID,
                Result = result,
                AuditDate = DateTime.Now
            });
        }

        /// <summary>
        /// ค้นหา Folder ทั้งหมดที่ user มีสิทธิ์เข้าถึง (filter ตาม OrgUnit visibility + permission)
        /// ใช้สำหรับ FolderPickerDialog และ ShareDialog เพื่อแสดงเฉพาะ folder ที่มีสิทธิ์
        ///
        /// Filtering logic (เหมือน GetRootFolders):
        /// 1. OrgUnit Visibility — Admin เห็นทั้งหมด, Manager เห็น subtree, Officer เห็น own unit
        /// 2. Permission — Admin/Manager: CheckPermission, Officer: Authority model หรือ ACL
        /// </summary>
        public IEnumerable<Folder> GetAccessibleFolders(int? orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            // Resolve visible OrgUnit IDs (same pattern as GetRootFolders)
            HashSet<int> visibleOrgIds = null;
            if (_visibilityService != null)
            {
                visibleOrgIds = new HashSet<int>(
                    _visibilityService.GetVisibleOrgUnits(user.UserID, user)
                                      .Select(o => o.OrgUnitID));
            }

            var allFolders = GetAllFolders(orgUnitId, user);

            return allFolders
                .Where(f => !f.IsDeleted && !f.IsArchived)
                .Where(f => visibleOrgIds == null || visibleOrgIds.Contains(f.OrgUnitID))
                .Where(f =>
                {
                    bool isPrivileged = user.AppRole >= 3;
                    if (isPrivileged)
                        return _permissionService.CheckPermission(user.UserID, f.FolderID, "Read");

                    bool hasAuthority = user.CanViewFolder(f.ConfidentialityLevel);
                    bool hasAclRead = _permissionService.CheckPermission(user.UserID, f.FolderID, "Read");
                    return hasAuthority || hasAclRead;
                });
        }
    }
}
