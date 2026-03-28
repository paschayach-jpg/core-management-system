using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    public interface IFolderService
    {
        Folder CreateFolder(CreateFolderRequest request, UserContext user);
        void RenameFolder(int folderId, string newName, UserContext user);
        void ArchiveFolder(int folderId, UserContext user);
        void DeleteFolder(int folderId, UserContext user);
        void RestoreFolder(int folderId, int? newParentId, UserContext user);
        void PermanentlyDeleteFolder(int folderId, UserContext user);
        void ApplyTemplate(int parentFolderId, int templateId, UserContext user);
        void ApplyTemplateToOrgUnit(int orgUnitId, int templateId, string rootFolderName, int confidentialityLevel, UserContext user);
        IEnumerable<Folder> GetFolderContents(int folderId, UserContext user);
        Folder GetFolder(int folderId, UserContext user);
        void ChangeConfidentialityLevel(int folderId, int newLevel, UserContext user);
        IEnumerable<Folder> GetRootFolders(int? orgUnitId, UserContext user);
        IEnumerable<Folder> GetDeletedFolders(int? orgUnitId, UserContext user);
        IEnumerable<Folder> GetAllFolders(int? orgUnitId, UserContext user);
        IEnumerable<Folder> GetFoldersByOrgUnit(int orgUnitId, UserContext user);
        void CopyFolderStructure(int sourceFolderId, int destinationParentFolderId, UserContext user);
        void MoveFolder(MoveFolderRequest request, UserContext user);
        IEnumerable<Folder> GetDescendants(int folderId);
    }
}
