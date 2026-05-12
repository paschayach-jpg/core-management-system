using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// Presenter for MOD-002 Folder Structure.
    /// Mediates between IFolderView and folder controllers.
    /// Satisfies Requirements 4.1–4.14, 7.6.
    /// </summary>
    public class FolderPresenter
    {
        private readonly IFolderCommandController _folderCommandController;
        private readonly IFileBrowseController _fileBrowseController;
        private readonly IFolderView       _view;
        private readonly UserContext       _user;
        private readonly IFolderReadLookupController _folderReadLookupController;

        private int? _currentOrgUnitId;
        private int? _currentFolderId;   // tracks the folder currently shown in content panel

        public FolderPresenter(
            IFolderView       view,
            UserContext       user,
            IFileBrowseController fileBrowseController = null,
            IFolderCommandController folderCommandController = null,
            IFolderReadLookupController folderReadLookupController = null)
        {
            if (view    == null) throw new ArgumentNullException("view");
            if (user    == null) throw new ArgumentNullException("user");
            if (folderCommandController == null) throw new ArgumentNullException("folderCommandController");
            if (folderReadLookupController == null) throw new ArgumentNullException("folderReadLookupController");

            _folderCommandController = folderCommandController;
            _fileBrowseController = fileBrowseController;
            _view                 = view;
            _user                 = user;
            _folderReadLookupController = folderReadLookupController;
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
                // invalidate cache ก่อนทุกครั้งเพื่อให้ tree แสดงข้อมูลล่าสุดจาก DB เสมอ
                _folderReadLookupController.InvalidateFolderCache(orgUnitId);
                var allFolders = _folderReadLookupController.GetAllFolders(orgUnitId, _user);
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
                return _folderReadLookupController.GetAllFolders(orgUnitId, _user)
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
            _currentFolderId = null; // viewing root level — no specific folder selected
            try
            {
                IEnumerable<Folder> roots = _folderReadLookupController.GetRootFolders(orgUnitId, _user);
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

                if (_folderReadLookupController != null)
                {
                    // ใช้ controller abstraction — ไม่มี permission check ขวางกั้น
                    subFolders = _folderReadLookupController.GetChildren(folderId)
                        .Where(f => !f.IsDeleted && !f.IsArchived);
                }
                else
                {
                    subFolders = Enumerable.Empty<Folder>();
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
                return _folderReadLookupController.GetChildren(parentFolderId);
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
                return _folderReadLookupController.GetAllFolders(_currentOrgUnitId, _user);
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
            _currentFolderId = folderId;
            try
            {
                // โฟลเดอร์อาจถูกลบ/ย้ายระหว่าง refresh ทำให้ ID เดิมไม่ถูกต้อง
                // ให้ fallback ไป root view แทนการเด้ง error popup
                var folder = _folderReadLookupController.GetFolder(folderId, _user);
                if (folder == null || folder.IsDeleted || folder.IsArchived)
                {
                    _currentFolderId = null;
                    ShowOrgUnitRootFolders(_currentOrgUnitId);
                    _view.ShowInfo("ℹ️ โฟลเดอร์ที่เลือกไม่มีอยู่แล้ว ระบบแสดงรายการโฟลเดอร์หลักแทน");
                    return;
                }

                IEnumerable<Folder>       subFolders = _folderReadLookupController.GetChildrenWithPermission(folderId, _user)
                    .Where(f => !f.IsDeleted && !f.IsArchived);
                IEnumerable<FileDocument> files      = null;

                if (_fileBrowseController != null)
                {
                    try
                    {
                        var rawFiles = _fileBrowseController.GetFilesInFolder(folderId, _user);

                        // Enrich files with folder's confidentiality level
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
                        _view.ShowInfo("🔒 ไม่มีสิทธิ์อ่านไฟล์ใน Folder นี้");
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
                // รองรับกรณี stale folder id จาก UI state เก่า
                if (!string.IsNullOrEmpty(ex.Message)
                    && ex.Message.IndexOf("was not found", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _currentFolderId = null;
                    ShowOrgUnitRootFolders(_currentOrgUnitId);
                    _view.ShowInfo("ℹ️ โฟลเดอร์เดิมไม่พบแล้ว ระบบแสดงรายการโฟลเดอร์หลักแทน");
                    return;
                }

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
        }

        // -----------------------------------------------------------------------
        // CRUD  (Req 4.2–4.4, 4.14)
        // -----------------------------------------------------------------------

        private void RefreshContentPanel()
        {
            if (_currentFolderId.HasValue)
                GetFolderContents(_currentFolderId.Value);
            else
                ShowOrgUnitRootFolders(_currentOrgUnitId);
        }

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
                Folder created = _folderCommandController.CreateFolder(request, _user);

                _currentOrgUnitId = orgUnitId;
                LoadFolderTree(_currentOrgUnitId);
                RefreshContentPanel();

                if (showSuccessMessage)
                    _view.ShowInfo(string.Format("Root folder '{0}' สร้างสำเร็จ", created.FolderName));
            }
            catch (InvalidOperationException ex) { _view.ShowError(ex.Message); }
            catch (ArgumentException ex)         { _view.ShowError("Validation error: " + ex.Message); }
            catch (Exception ex)                 { _view.ShowError("ไม่สามารถสร้าง Root Folder ได้: " + ex.Message); }
        }

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
                Folder created = _folderCommandController.CreateFolder(request, _user);

                _currentOrgUnitId = orgUnitId;
                LoadFolderTree(_currentOrgUnitId);
                RefreshContentPanel();

                if (showSuccessMessage)
                    _view.ShowInfo(string.Format("Folder '{0}' สร้างสำเร็จ", created.FolderName));
            }
            catch (InvalidOperationException ex) { _view.ShowError(ex.Message); }
            catch (ArgumentException ex)         { _view.ShowError("Validation error: " + ex.Message); }
            catch (Exception ex)                 { _view.ShowError("ไม่สามารถสร้าง Folder ได้: " + ex.Message); }
        }

        public void RenameFolder(int folderId, string newName)
        {
            try
            {
                _folderCommandController.RenameFolder(folderId, newName, _user);
                LoadFolderTree(_currentOrgUnitId);
                _view.ShowInfo(string.Format("เปลี่ยนชื่อโฟลเดอร์เป็น \"{0}\" เรียบร้อยแล้ว", newName));
            }
            catch (InvalidOperationException ex) { _view.ShowError("ไม่สามารถเปลี่ยนชื่อโฟลเดอร์ได้: " + ex.Message); }
            catch (ArgumentException ex)         { _view.ShowError("ข้อมูลไม่ถูกต้อง: " + ex.Message); }
            catch (Exception ex)                 { _view.ShowError("เกิดข้อผิดพลาดในการเปลี่ยนชื่อ: " + ex.Message); }
        }

        // -----------------------------------------------------------------------
        // Archive  (Req 4.7–4.8)
        // -----------------------------------------------------------------------

        public void ArchiveFolder(int folderId)
        {
            try
            {
                _folderCommandController.ArchiveFolder(folderId, _user);
                LoadFolderTree(_currentOrgUnitId);
                _view.ShowInfo("Folder and all descendants have been archived.");
            }
            catch (InvalidOperationException ex) { _view.ShowError("Cannot archive folder: " + ex.Message); }
            catch (Exception ex)                 { _view.ShowError("Failed to archive folder: " + ex.Message); }
        }

        // -----------------------------------------------------------------------
        // Delete (soft)  (Req 4.9–4.10)
        // -----------------------------------------------------------------------

        public void DeleteFolder(int folderId)
        {
            try
            {
                _folderCommandController.DeleteFolder(folderId, _user);
                LoadFolderTree(_currentOrgUnitId);
                RefreshContentPanel();

                var deleted = _folderReadLookupController.GetFolder(folderId, _user);
                if (deleted == null || deleted.IsDeleted)
                    _view.ShowInfo("ย้าย Folder ไปยังถังขยะเรียบร้อยแล้ว");
                else
                    _view.ShowError("ไม่สามารถยืนยันการลบ Folder\nกรุณาตรวจสอบอีกครั้ง");
            }
            catch (InvalidOperationException ex) { _view.ShowError("ไม่สามารถลบ Folder ได้: " + ex.Message); }
            catch (Exception ex)                 { _view.ShowError("เกิดข้อผิดพลาดในการลบ Folder: " + ex.Message); }
        }

        // -----------------------------------------------------------------------
        // Restore  (Req 4.12–4.13)
        // -----------------------------------------------------------------------

        public void RestoreFolder(int folderId, int? newParentId)
        {
            try
            {
                _folderCommandController.RestoreFolder(folderId, newParentId, _user);
                LoadFolderTree(_currentOrgUnitId);
                _view.ShowInfo("Folder restored successfully.");
            }
            catch (InvalidOperationException ex) { _view.ShowError(ex.Message); }
            catch (Exception ex)                 { _view.ShowError("Failed to restore folder: " + ex.Message); }
        }

        // -----------------------------------------------------------------------
        // Permanent delete  (Req 4.11)
        // -----------------------------------------------------------------------

        public void PermanentlyDeleteFolder(int folderId)
        {
            try
            {
                _folderCommandController.PermanentlyDeleteFolder(folderId, _user);
                _view.ShowInfo("ลบ Folder ถาวรเรียบร้อยแล้ว");
            }
            catch (UnauthorizedAccessException ex) { _view.ShowError("ไม่มีสิทธิ์ลบ Folder ถาวร: " + ex.Message); }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("REFERENCE constraint") || ex.Message.Contains("FK_sto_FileDocument_Folder"))
                    _view.ShowError("ไม่สามารถลบ Folder ได้เนื่องจากยังมีไฟล์อยู่ข้างใน\n\nกรุณาลบไฟล์ทั้งหมดใน Folder นี้ก่อน หรือติดต่อผู้ดูแลระบบ\n\nรายละเอียดข้อผิดพลาด: " + ex.Message);
                else
                    _view.ShowError("ไม่สามารถลบ Folder ได้: " + ex.Message);
            }
            catch (Exception ex) { _view.ShowError("เกิดข้อผิดพลาดในการลบ Folder: " + ex.Message); }
        }

        // -----------------------------------------------------------------------
        // Apply template  (Req 4.5–4.6)
        // -----------------------------------------------------------------------

        public void ApplyTemplate(int parentFolderId, int templateId)
        {
            try
            {
                _folderCommandController.ApplyTemplate(parentFolderId, templateId, _user);
                LoadFolderTree(_currentOrgUnitId);
                _view.ShowInfo("Template applied successfully.");
            }
            catch (UnauthorizedAccessException ex) { _view.ShowError("Access denied: " + ex.Message); }
            catch (InvalidOperationException ex)   { _view.ShowError("Cannot apply template: " + ex.Message); }
            catch (Exception ex)                   { _view.ShowError("Failed to apply template: " + ex.Message); }
        }
    }
}
