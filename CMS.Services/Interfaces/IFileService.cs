using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    public interface IFileService
    {
        IEnumerable<UploadResult> UploadFiles(IEnumerable<FileUploadRequest> files, int folderId, UserContext user);
        FileDownloadResult DownloadFile(int fileId, UserContext user, bool bypassPermissionCheck = false);
        void DeleteFile(int fileId, UserContext user);
        StorageUsageReport GetStorageReport(int orgUnitId, UserContext user);

        /// <summary>Returns all active (non-deleted) files in the given folder.</summary>
        IEnumerable<FileDocument> GetFilesInFolder(int folderId, UserContext user);

        /// <summary>
        /// Returns ALL active files from storage.FileDocument grouped by folder (no permission filter).
        /// Admin/Manager use only.
        /// </summary>
        IEnumerable<FileDocument> GetAllFilesGroupedByFolder(UserContext user);
        IEnumerable<FileDocument> GetFilesByOrgUnit(int orgUnitId, UserContext user);
        void RenameFile(int fileId, string newName, UserContext user);
        void MoveFiles(MoveFileRequest request, UserContext user);

        /// <summary>
        /// คัดลอกไฟล์ไปยัง Folder ปลายทาง (สร้าง record ใหม่ พร้อม copy physical file)
        /// เมื่อ overwrite = true ระบบจะ soft-delete ไฟล์เดิมที่มีชื่อซ้ำ แล้วเขียนทับด้วยไฟล์ใหม่
        /// </summary>
        int CopyFile(int fileId, int destinationFolderId, UserContext user, bool overwrite = false);

        /// <summary>Returns the subset of <paramref name="fileNames"/> that already exist (active) in the folder.</summary>
        IEnumerable<string> FindDuplicateNames(IEnumerable<string> fileNames, int folderId, UserContext user);

        /// <summary>Hide the specified files (IsHidden = true).</summary>
        void HideFiles(List<int> fileIds, UserContext user);

        /// <summary>Unhide the specified files (IsHidden = false).</summary>
        void UnhideFiles(List<int> fileIds, UserContext user);
    }
}
