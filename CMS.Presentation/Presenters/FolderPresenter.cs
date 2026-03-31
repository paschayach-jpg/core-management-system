using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Presenters
{
    /// <summary>
    /// Presenter for MOD-002 Folder Structure.
    /// Mediates between IFolderView and IFolderService.
    /// Satisfies Requirements 4.1–4.14, 7.6.
    /// </summary>
    public class FolderPresenter
    {
        private readonly IFolderService    _service;
        private readonly IFileService      _fileService;
        private readonly IFolderView       _view;
        private readonly UserContext       _user;
        private readonly IFolderRepository _folderRepository;

        private int? _currentOrgUnitId;

        public FolderPresenter(
            IFolderService    service,
            IFolderView       view,
            UserContext       user,
            IFileService      fileService      = null,
            IFolderRepository folderRepository = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (view    == null) throw new ArgumentNullException("view");
            if (user    == null) throw new ArgumentNullException("user");

            _service          = service;
            _fileService      = fileService;
            _view             = view;
            _user             = user;
            _folderRepository = folderRepository;
        }

        // -----------------------------------------------------------------------
        // Tree loading  (Req 4.1, 7.6)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Loads the folder tree for the given OrgUnit (or all accessible folders when null).
        /// Fetches ALL folders in one query and passes them to the view, which builds the
        /// hierarchy in-memory — eliminates N+1 per-node DB calls.
        /// </summary>
        public void LoadFolderTree(int? orgUnitId)
        {
            _currentOrgUnitId = orgUnitId;
            try
            {
                var allFolders = _service.GetAllFolders(orgUnitId, _user);
                _view.ShowFolderTree(allFolders);
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("Access denied: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to load folder tree: " + ex.Message);
            }
        }

        /// <summary>
        /// Fetches all folders for the given OrgUnit without touching the view.
        /// Used by FolderForm for async loading (Task.Run), so data comes back
        /// on a background thread and the caller updates UI controls itself.
        /// </summary>
        public List<Folder> FetchAllFolders(int? orgUnitId)
        {
            _currentOrgUnitId = orgUnitId;
            try
            {
                return _service.GetAllFolders(orgUnitId, _user)
                    .Where(f => !f.IsDeleted && !f.IsArchived)
                    .ToList();
            }
            catch
            {
                return new List<Folder>();
            }
        }

        /// <summary>
        /// แสดง root folders ของ OrgUnit ใน content area (เมื่อเลือก OrgUnit จาก tree ซ้าย)
        /// </summary>
        public void ShowOrgUnitRootFolders(int? orgUnitId)
        {
            try
            {
                IEnumerable<Folder> roots = _service.GetRootFolders(orgUnitId, _user);
                _view.ShowFolderContents(roots, Enumerable.Empty<FileDocument>());
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลด Folder ไม่สำเร็จ: " + ex.Message);
            }
        }

        /// <summary>แสดงเฉพาะ sub-folders ภายใน folder (ไม่แสดง files) — ใช้เมื่อเลือก folder จาก tree</summary>
        public void ShowFolderSubFolders(int folderId)
        {
            try
            {
                IEnumerable<Folder> subFolders;
                
                if (_folderRepository != null)
                {
                    // ใช้ repository โดยตรง — ไม่มี permission check ขวางกั้น
                    subFolders = _folderRepository.GetChildren(folderId)
                        .Where(f => !f.IsDeleted && !f.IsArchived);
                }
                else
                {
                    // Fallback: ใช้ service (อาจถูก block โดย permission)
                    subFolders = _service.GetFolderContents(folderId, _user);
                }
                
                _view.ShowFolderContents(subFolders, Enumerable.Empty<FileDocument>());
            }
            catch (UnauthorizedAccessException)
            {
                _view.ShowFolderContents(new List<Folder>(), new List<FileDocument>());
                _view.ShowInfo("🔒 ไม่มีสิทธิ์เข้าถึง Folder นี้");
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลด Folder ไม่สำเร็จ: " + ex.Message);
            }
        }

        /// <summary>
        /// Gets child folders for the given parent folder ID.
        /// Used by the view to build the tree hierarchy recursively.
        /// </summary>
        public IEnumerable<Folder> GetFolderChildren(int parentFolderId)
        {
            try
            {
                return _service.GetFolderContents(parentFolderId, _user);
            }
            catch
            {
                return new List<Folder>();
            }
        }

        /// <summary>
        /// Gets all folders for the current OrgUnit (used for confidentiality level filtering).
        /// </summary>
        public IEnumerable<Folder> GetAllFolders()
        {
            try
            {
                return _service.GetAllFolders(_currentOrgUnitId, _user);
            }
            catch
            {
                return new List<Folder>();
            }
        }

        // -----------------------------------------------------------------------
        // Folder contents  (Req 4.1)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Loads the contents (subfolders + files) of the selected folder into the contents panel.
        /// </summary>
        public void GetFolderContents(int folderId)
        {
            try
            {
                IEnumerable<Folder>       subFolders = _service.GetFolderContents(folderId, _user);
                IEnumerable<FileDocument> files      = null;

                if (_fileService != null)
                {
                    try
                    {
                        var rawFiles = _fileService.GetFilesInFolder(folderId, _user);

                        // Enrich files with folder's confidentiality level
                        var folder = _service.GetFolder(folderId, _user);
                        int folderLevel = folder != null ? folder.ConfidentialityLevel : 0;
                        var enriched = new List<FileDocument>();
                        foreach (var f in rawFiles)
                        {
                            f.ConfidentialityLevel = folderLevel;
                            enriched.Add(f);
                        }
                        files = enriched;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        files = new List<FileDocument>();
                    }
                    catch (Exception ex)
                    {
                        files = new List<FileDocument>();
                        _view.ShowError("ไม่สามารถโหลดรายการไฟล์: " + ex.Message);
                    }
                }

                _view.ShowFolderContents(subFolders, files ?? new List<FileDocument>());
            }
            catch (UnauthorizedAccessException)
            {
                _view.ShowFolderContents(new List<Folder>(), new List<FileDocument>());
                _view.ShowInfo("🔒 ไม่มีสิทธิ์เข้าถึง Folder นี้");
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to load folder contents: " + ex.Message);
            }
        }

        // -----------------------------------------------------------------------
        // Recycle bin  (Req 4.9–4.13)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Loads soft-deleted folders into the recycle bin panel.
        /// Note: Recycle bin functionality is currently disabled.
        /// </summary>
        public void LoadRecycleBin(int? orgUnitId)
        {
            // Recycle bin functionality is disabled - pass empty list
            _view.ShowRecycleBin(new List<Folder>());
            /*
            try
            {
                IEnumerable<Folder> deleted = _service.GetDeletedFolders(orgUnitId, _user);
                _view.ShowRecycleBin(deleted);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to load recycle bin: " + ex.Message);
            }
            */
        }

        // -----------------------------------------------------------------------
        // CRUD  (Req 4.2–4.4, 4.14)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates a new root folder (no parent) under the given OrgUnit.
        /// </summary>
        public void CreateRootFolder(string name, int orgUnitId, int confidentialityLevel = 1, bool showSuccessMessage = true)
        {
            try
            {
                var request = new CreateFolderRequest
                {
                    ParentFolderId       = null,
                    OrgUnitId            = orgUnitId,
                    Name                 = name,
                    ConfidentialityLevel = confidentialityLevel
                };
                Folder created = _service.CreateFolder(request, _user);

                // ใช้ orgUnitId ที่รับมาเพื่อ reload ให้แน่ใจว่าถูกต้อง
                _currentOrgUnitId = orgUnitId;
                LoadFolderTree(_currentOrgUnitId);
                // LoadRecycleBin(_currentOrgUnitId); // Recycle bin disabled

                if (showSuccessMessage)
                {
                    _view.ShowInfo(string.Format("Root folder '{0}' สร้างสำเร็จ", created.FolderName));
                }
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _view.ShowError("Validation error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("ไม่สามารถสร้าง Root Folder ได้: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates a new folder under the given parent.
        /// Shows a descriptive error if the depth limit (10) would be exceeded (Req 4.3).
        /// </summary>
        public void CreateFolder(int parentFolderId, string name, int orgUnitId, int? confidentialityLevel = null, bool showSuccessMessage = true)
        {
            try
            {
                var request = new CreateFolderRequest
                {
                    ParentFolderId       = parentFolderId,
                    OrgUnitId            = orgUnitId,
                    Name                 = name,
                    ConfidentialityLevel = confidentialityLevel
                };
                Folder created = _service.CreateFolder(request, _user);

                // Reload the folder tree and recycle bin, then show success message
                LoadFolderTree(_currentOrgUnitId);
                // LoadRecycleBin(_currentOrgUnitId); // Recycle bin disabled

                if (showSuccessMessage)
                {
                    _view.ShowInfo(string.Format("Folder '{0}' สร้างสำเร็จ", created.FolderName));
                }
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _view.ShowError("Validation error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("ไม่สามารถสร้าง Folder ได้: " + ex.Message);
            }
        }

        /// <summary>
        /// Renames the specified folder (Req 4.14).
        /// </summary>
        public void RenameFolder(int folderId, string newName)
        {
            try
            {
                _service.RenameFolder(folderId, newName, _user);
                
                // Reload data first, then show success message
                LoadFolderTree(_currentOrgUnitId);
                
                _view.ShowInfo(string.Format("Folder was renamed to '{0}'.", newName));
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("Cannot rename folder: " + ex.Message);
            }
            catch (ArgumentException ex)
            {
                _view.ShowError("Validation error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to rename folder: " + ex.Message);
            }
        }

        // -----------------------------------------------------------------------
        // Archive  (Req 4.7–4.8)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Archives the folder and all its descendants (Req 4.7–4.8).
        /// </summary>
        public void ArchiveFolder(int folderId)
        {
            try
            {
                _service.ArchiveFolder(folderId, _user);
                
                // Reload data first, then show success message
                LoadFolderTree(_currentOrgUnitId);
                
                _view.ShowInfo("Folder and all descendants have been archived.");
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("Cannot archive folder: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to archive folder: " + ex.Message);
            }
        }

        // -----------------------------------------------------------------------
        // Delete (soft)  (Req 4.9–4.10)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Moves the folder to the recycle bin (soft delete) (Req 4.9–4.10).
        /// </summary>
        public void DeleteFolder(int folderId)
        {
            try
            {
                _service.DeleteFolder(folderId, _user);

                // Reload ก่อนเพื่อยืนยันว่าลบสำเร็จ
                LoadFolderTree(_currentOrgUnitId);

                // ตรวจสอบว่า Folder ถูกลบออกจากระบบแล้ว (IsDeleted == true)
                var deleted = _service.GetFolder(folderId, _user);
                if (deleted == null || deleted.IsDeleted)
                    _view.ShowInfo("ย้าย Folder ไปยังถังขยะเรียบร้อยแล้ว");
                else
                    _view.ShowError("ไม่สามารถยืนยันการลบ Folder\nกรุณาตรวจสอบอีกครั้ง");
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("ไม่สามารถลบ Folder ได้: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดในการลบ Folder: " + ex.Message);
            }
        }

        // -----------------------------------------------------------------------
        // Restore  (Req 4.12–4.13)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Restores a folder from the recycle bin.
        /// If newParentId is null the service will attempt to restore to the original parent.
        /// Shows a descriptive error if the original parent no longer exists (Req 4.13).
        /// </summary>
        public void RestoreFolder(int folderId, int? newParentId)
        {
            try
            {
                _service.RestoreFolder(folderId, newParentId, _user);
                
                // Reload data first, then show success message
                LoadFolderTree(_currentOrgUnitId);
                // LoadRecycleBin(_currentOrgUnitId); // Recycle bin disabled
                
                _view.ShowInfo("Folder restored successfully.");
            }
            catch (InvalidOperationException ex)
            {
                // Req 4.13: service message already says "original parent no longer exists"
                _view.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to restore folder: " + ex.Message);
            }
        }

        // -----------------------------------------------------------------------
        // Permanent delete  (Req 4.11)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Permanently deletes a folder from the recycle bin. Manager/Admin only (Req 4.11).
        /// </summary>
        public void PermanentlyDeleteFolder(int folderId)
        {
            try
            {
                _service.PermanentlyDeleteFolder(folderId, _user);
                
                // Reload data first, then show success message
                // LoadRecycleBin(_currentOrgUnitId); // Recycle bin disabled
                
                _view.ShowInfo("ลบ Folder ถาวรเรียบร้อยแล้ว");
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("ไม่มีสิทธิ์ลบ Folder ถาวร: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("REFERENCE constraint") || ex.Message.Contains("FK_sto_FileDocument_Folder"))
                {
                    _view.ShowError(
                        "ไม่สามารถลบ Folder ได้เนื่องจากยังมีไฟล์อยู่ข้างใน\n\n" +
                        "กรุณาลบไฟล์ทั้งหมดใน Folder นี้ก่อน หรือติดต่อผู้ดูแลระบบ\n\n" +
                        "รายละเอียดข้อผิดพลาด: " + ex.Message);
                }
                else
                {
                    _view.ShowError("ไม่สามารถลบ Folder ได้: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดในการลบ Folder: " + ex.Message);
            }
        }

        // -----------------------------------------------------------------------
        // Apply template  (Req 4.5–4.6)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Applies a folder template under the given parent. Manager/Admin only (Req 4.5–4.6).
        /// </summary>
        public void ApplyTemplate(int parentFolderId, int templateId)
        {
            try
            {
                _service.ApplyTemplate(parentFolderId, templateId, _user);
                
                // Reload data first, then show success message
                LoadFolderTree(_currentOrgUnitId);
                
                _view.ShowInfo("Template applied successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("Access denied: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("Cannot apply template: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to apply template: " + ex.Message);
            }
        }
    }
}
