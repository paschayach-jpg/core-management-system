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

        private static readonly char[] InvalidFolderNameChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

        private static void ValidateFolderName(string name)
        {
            var found = name.Where(c => InvalidFolderNameChars.Contains(c)).Distinct().ToArray();
            if (found.Length > 0)
                throw new ArgumentException(
                    string.Format("ชื่อ Folder ไม่อนุญาตให้ใช้อักขระ: {0}", string.Join(" ", found)));
        }

        private readonly IFolderRepository            _folderRepository;
        private readonly IFileDocumentRepository      _fileDocumentRepository;
        private readonly IPermissionRepository        _permissionRepository;
        private readonly IPermissionService           _permissionService;
        private readonly INetworkDriveHelper          _networkDriveHelper;
        private readonly IAuditService                _auditService;
        private readonly IFolderTemplateRepository    _folderTemplateRepository;
        private readonly IApprovalRepository          _approvalRepository;
        private readonly IUserOrgVisibilityService    _visibilityService;

        public FolderService(
            IFolderRepository          folderRepository,
            IFileDocumentRepository    fileDocumentRepository,
            IPermissionRepository      permissionRepository,
            IPermissionService         permissionService,
            INetworkDriveHelper        networkDriveHelper,
            IAuditService              auditService,
            IFolderTemplateRepository  folderTemplateRepository = null,
            IApprovalRepository        approvalRepository       = null,
            IUserOrgVisibilityService  visibilityService        = null)
        {
            if (folderRepository       == null) throw new ArgumentNullException("folderRepository");
            if (fileDocumentRepository == null) throw new ArgumentNullException("fileDocumentRepository");
            if (permissionRepository   == null) throw new ArgumentNullException("permissionRepository");
            if (permissionService      == null) throw new ArgumentNullException("permissionService");
            if (networkDriveHelper     == null) throw new ArgumentNullException("networkDriveHelper");
            if (auditService           == null) throw new ArgumentNullException("auditService");

            _folderRepository          = folderRepository;
            _fileDocumentRepository    = fileDocumentRepository;
            _permissionRepository      = permissionRepository;
            _permissionService         = permissionService;
            _networkDriveHelper        = networkDriveHelper;
            _auditService              = auditService;
            _folderTemplateRepository  = folderTemplateRepository;
            _approvalRepository        = approvalRepository;
            _visibilityService         = visibilityService;
        }

        public Folder CreateFolder(CreateFolderRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Folder name is required.");

            ValidateFolderName(request.Name.Trim());

            // Duplicate check: compare case-insensitive + trimmed against active siblings
            string trimmedName = request.Name.Trim();
            var siblings = _folderRepository.GetSiblings(request.ParentFolderId, request.OrgUnitId);
            var duplicate = siblings.FirstOrDefault(
                f => string.Equals(f.FolderName.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));
            if (duplicate != null)
                throw new InvalidOperationException(
                    string.Format("มี Folder ชื่อ '{0}' อยู่แล้วใน Parent เดียวกัน", duplicate.FolderName));

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
                depth = 1;  // root folder อยู่ที่ depth 1 (ไม่มี parent)
                confidentiality   = request.ConfidentialityLevel ?? 4; // 4=Public
                inheritPermission = !request.ConfidentialityLevel.HasValue;
            }

            var folder = new Folder
            {
                ParentFolderID     = request.ParentFolderId,
                OrgUnitID          = request.OrgUnitId,
                FolderName         = request.Name.Trim(),
                FolderDepth        = depth,
                ConfidentialityLevel = confidentiality,
                InheritPermission  = inheritPermission,
                IsDeleted          = false,
                IsArchived         = false,
                CreatedBy          = user.UserID,
                CreatedDate        = DateTime.Now
            };

            int newId = _folderRepository.Insert(folder);
            folder.FolderID = newId;

            Log(user, "Folder", "CREATE", "storage.Folder", newId, 1,
                string.Format("Created folder '{0}' at depth {1}.", folder.FolderName, depth));

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

            folder.IsArchived  = true;
            folder.ArchivedBy  = user.UserID;
            folder.ArchivedDate = DateTime.Now;
            _folderRepository.Update(folder);

            var descendants = _folderRepository.GetDescendants(folderId).ToList();
            foreach (var descendant in descendants)
            {
                descendant.IsArchived   = true;
                descendant.ArchivedBy   = user.UserID;
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

            // Req 5.1–5.3: folders with files must go through Admin approval workflow
            if (folder.FileCount > 0)
                throw new InvalidOperationException(
                    string.Format(
                        "ไม่สามารถลบ Folder '{0}' ได้โดยตรง เนื่องจากมีไฟล์อยู่ {1} รายการ\nกรุณายื่นคำขออนุมัติการลบผ่านระบบ Workflow",
                        folder.FolderName, folder.FileCount));

            _folderRepository.SoftDelete(folderId, user.UserID);

            Log(user, "Folder", "DELETE", "storage.Folder", folderId, 1,
                string.Format("Moved folder '{0}' to recycle bin.", folder.FolderName));
        }

        public void RestoreFolder(int folderId, int? newParentId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));
            if (!folder.IsDeleted)
                throw new InvalidOperationException("Folder is not in the recycle bin.");

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

            if (!_permissionService.CheckPermission(user.UserID, folderId, "Read"))
                throw new UnauthorizedAccessException(string.Format(
                    "User {0} does not have Read permission on folder {1}.", user.UserID, folderId));

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

            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                throw new InvalidOperationException(string.Format("Folder with ID {0} was not found.", folderId));

            folder.ConfidentialityLevel = newLevel;
            folder.InheritPermission    = false;
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
                string.Format("Changed confidentiality level of folder '{0}' to {1}.", folder.FolderName, newLevel));
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

            // กรองเฉพาะ folder ที่ user มีสิทธิ์ Read (Admin/Manager ผ่านโดยอัตโนมัติเว้นแต่ถูก Deny)
            return roots.Where(f => _permissionService.CheckPermission(user.UserID, f.FolderID, "Read"));
        }

        public IEnumerable<Folder> GetDeletedFolders(int? orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (orgUnitId.HasValue)
                return _folderRepository.GetDeletedByOrgUnit(orgUnitId.Value);

            return _folderRepository.GetAllDeletedFolders();
        }

        public IEnumerable<Folder> GetAllFolders(int? orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (orgUnitId.HasValue)
                return _folderRepository.GetByOrgUnit(orgUnitId.Value);

            return _folderRepository.GetAll();
        }

        public IEnumerable<Folder> GetFoldersByOrgUnit(int orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return _folderRepository.GetByOrgUnit(orgUnitId);
        }

        public void ApplyTemplateToOrgUnit(int orgUnitId, int templateId, string rootFolderName, int confidentialityLevel, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("การสร้าง Folder จาก Template ต้องการสิทธิ์ระดับ Manager หรือ Admin");
            if (_folderTemplateRepository == null)
                throw new InvalidOperationException("FolderTemplateRepository is not configured.");

            var items = _folderTemplateRepository.GetItems(templateId).ToList();
            if (items.Count == 0)
                throw new InvalidOperationException("Template นี้ไม่มี Sub-Folder กำหนดไว้");

            // Clamp confidentialityLevel to valid range 1-4
            if (confidentialityLevel < 1 || confidentialityLevel > 4)
                confidentialityLevel = 4;

            // สร้าง root folder ของ OrgUnit
            var rootFolder = new Folder
            {
                ParentFolderID       = null,
                OrgUnitID            = orgUnitId,
                FolderName           = rootFolderName.Trim(),
                FolderDepth          = 1,
                ConfidentialityLevel = confidentialityLevel,
                InheritPermission    = false,
                IsDeleted            = false,
                IsArchived           = false,
                CreatedBy            = user.UserID,
                CreatedDate          = DateTime.Now
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
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("การสร้าง Folder จาก Template ต้องการสิทธิ์ระดับ Manager หรือ Admin");
            if (_folderTemplateRepository == null)
                throw new InvalidOperationException("FolderTemplateRepository is not configured.");

            var parentFolder = _folderRepository.GetById(parentFolderId);
            if (parentFolder == null)
                throw new InvalidOperationException(string.Format("Parent folder with ID {0} was not found.", parentFolderId));
            if (parentFolder.IsDeleted)
                throw new InvalidOperationException("Cannot apply a template to a deleted folder.");

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
                    ParentFolderID       = parentFolderId,
                    OrgUnitID            = orgUnitId,
                    FolderName           = item.FolderName,
                    FolderDepth          = depth,
                    ConfidentialityLevel = itemConfidentiality,
                    InheritPermission    = item.InheritPermission,
                    IsDeleted            = false,
                    IsArchived           = false,
                    CreatedBy            = user.UserID,
                    CreatedDate          = DateTime.Now
                };

                int newId = _folderRepository.Insert(folder);
                CreateFoldersFromItems(allItems, item.ItemID, newId, depth, orgUnitId, itemConfidentiality, user);
            }
        }

        public void MoveFolder(MoveFolderRequest request, UserContext user)
        {
            // 3.1 — Null check
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");

            int sourceFolderId            = request.SourceFolderId;
            int destinationParentFolderId = request.DestinationParentFolderId;

            // 3.1 — Permission check: role gate
            if (user.AppRole < 2)
                throw new UnauthorizedAccessException("ต้องมีสิทธิ์ระดับ SrOfficer ขึ้นไป");

            // 3.1 — Permission check: Write on source and destination (skip for Admin)
            if (user.AppRole < 4)
            {
                if (!_permissionService.CheckPermission(user.UserID, sourceFolderId, "Write"))
                    throw new UnauthorizedAccessException(
                        string.Format("ไม่มีสิทธิ์เขียนใน Folder ต้นทาง (ID: {0})", sourceFolderId));

                if (!_permissionService.CheckPermission(user.UserID, destinationParentFolderId, "Write"))
                    throw new UnauthorizedAccessException("ไม่มีสิทธิ์เขียนใน Folder ปลายทาง");
            }

            // 3.2 — Load source and destination
            var source      = _folderRepository.GetById(sourceFolderId);
            var destination = _folderRepository.GetById(destinationParentFolderId);

            if (source == null)
                throw new InvalidOperationException(
                    string.Format("ไม่พบ Folder ต้นทาง ID {0}", sourceFolderId));
            if (destination == null)
                throw new InvalidOperationException(
                    string.Format("ไม่พบ Folder ปลายทาง ID {0}", destinationParentFolderId));

            // 3.2 — Circular reference: self-move
            if (sourceFolderId == destinationParentFolderId)
                throw new InvalidOperationException("ไม่สามารถย้าย Folder ไปยังตัวเองได้");

            // 3.2 — Circular reference: destination is a descendant of source
            var descendants = _folderRepository.GetDescendants(sourceFolderId).ToList();
            if (descendants.Any(d => d.FolderID == destinationParentFolderId))
                throw new InvalidOperationException("ไม่สามารถย้าย Folder ไปยัง Descendant ของตัวเองได้");

            // 3.2 — Depth limit check
            int maxDescendantDepth = descendants.Any()
                ? descendants.Max(d => d.FolderDepth)
                : source.FolderDepth;
            int subtreeHeight  = maxDescendantDepth - source.FolderDepth;
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
            int  originalSourceDepth = source.FolderDepth;
            int  depthDelta = (destination.FolderDepth + 1) - originalSourceDepth;

            // 3.3 — Update source folder
            source.ParentFolderID = destinationParentFolderId;
            source.FolderDepth    = destination.FolderDepth + 1;
            source.PhysicalPath   = _networkDriveHelper.BuildPath(source.OrgUnitID, source.FolderID, source.FolderName);
            source.UpdatedBy      = user.UserID;
            source.UpdatedDate    = DateTime.Now;
            _folderRepository.Update(source);

            // 3.4 — Update all descendants
            foreach (var descendant in descendants)
            {
                descendant.FolderDepth  += depthDelta;
                descendant.PhysicalPath  = _networkDriveHelper.BuildPath(descendant.OrgUnitID, descendant.FolderID, descendant.FolderName);
                descendant.UpdatedBy     = user.UserID;
                descendant.UpdatedDate   = DateTime.Now;
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
                        file.PhysicalPath = _networkDriveHelper.BuildPath(source.OrgUnitID, file.FolderID, file.StoredFileName);
                        _fileDocumentRepository.Update(file);
                    }
                }
            }

            // 3.5 — Audit log
            _auditService.Log(new AuditEntry
            {
                Module      = "Folder",
                Action      = "MoveFolder",
                UserID      = user.UserID,
                UserName    = user.ADUserName ?? string.Empty,
                UserRole    = user.AppRole,
                TableName   = "storage.Folder",
                RecordID    = sourceFolderId,
                OldValue    = oldParentFolderId.HasValue ? oldParentFolderId.Value.ToString() : null,
                NewValue    = destinationParentFolderId.ToString(),
                MachineName = user.MachineName,
                IPAddress   = user.IPAddress,
                SessionID   = user.SessionID,
                Result      = 1,
                AuditDate   = DateTime.Now
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
                ParentFolderID       = destinationParentFolderId,
                OrgUnitID            = destinationParent.OrgUnitID,
                FolderName           = source.FolderName,
                FolderDepth          = newDepth,
                ConfidentialityLevel = source.ConfidentialityLevel,
                InheritPermission    = source.InheritPermission,
                IsDeleted            = false,
                IsArchived           = false,
                CreatedBy            = user.UserID,
                CreatedDate          = DateTime.Now
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
                    ParentFolderID       = newParentId,
                    OrgUnitID            = orgUnitId,
                    FolderName           = child.FolderName,
                    FolderDepth          = childDepth,
                    ConfidentialityLevel = child.ConfidentialityLevel,
                    InheritPermission    = child.InheritPermission,
                    IsDeleted            = false,
                    IsArchived           = false,
                    CreatedBy            = user.UserID,
                    CreatedDate          = DateTime.Now
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
                    ParentFolderID       = newParentId,
                    OrgUnitID            = orgUnitId,
                    FolderName           = child.FolderName,
                    FolderDepth          = childDepth,
                    ConfidentialityLevel = child.ConfidentialityLevel,
                    InheritPermission    = child.InheritPermission,
                    IsDeleted            = false,
                    IsArchived           = false,
                    CreatedBy            = user.UserID,
                    CreatedDate          = DateTime.Now
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
                Module      = module,
                Action      = action,
                UserID      = user.UserID,
                UserName    = user.ADUserName ?? string.Empty,
                UserRole    = user.AppRole,
                TableName   = tableName,
                RecordID    = recordId,
                NewValue    = newValue,
                MachineName = user.MachineName,
                IPAddress   = user.IPAddress,
                SessionID   = user.SessionID,
                Result      = result,
                AuditDate   = DateTime.Now
            });
        }
    }
}
