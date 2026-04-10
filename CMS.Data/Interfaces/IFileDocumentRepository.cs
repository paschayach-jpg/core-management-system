using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IFileDocumentRepository
    {
        int Insert(FileDocument document);
        FileDocument GetById(int fileId);
        IEnumerable<FileDocument> GetByFolder(int folderId);
        void Update(FileDocument document);
        void SoftDelete(int fileId, int deletedBy);
        void PermanentDelete(int fileId);
        void UpdateVirusScanStatus(int fileId, int virusScanStatus);
        FileDocument GetByChecksum(string checksum, int folderId);
        IEnumerable<FileDocument> GetAllGroupedByFolder();
        IEnumerable<FileDocument> GetByOrgUnit(int orgUnitId);
        void RenameFile(int fileId, string newName);

        /// <summary>Hard-deletes ALL FileDocument records for a folder regardless of FileStatus (used during permanent folder deletion).</summary>
        void DeleteAllByFolder(int folderId);

        /// <summary>Returns true when an active (non-deleted) file with the given original name already exists in the folder.</summary>
        bool ExistsWithName(string fileName, int folderId);

        /// <summary>Returns the number of active files whose name starts with <paramref name="namePrefix"/> in the folder (used for copy-counter naming).</summary>
        int CountByNamePrefix(string namePrefix, int folderId);
    }
}
