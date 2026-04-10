using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFolderCommandController
    {
        Folder CreateFolder(CreateFolderRequest request, UserContext user);
        Folder CreateHCFolderAfterApproval(ApprovalRequest approvalRequest, UserContext user);
        void RenameFolder(int folderId, string newName, UserContext user);
        void ArchiveFolder(int folderId, UserContext user);
        void DeleteFolder(int folderId, UserContext user);
        void RestoreFolder(int folderId, int? newParentId, UserContext user);
        void PermanentlyDeleteFolder(int folderId, UserContext user);
        void MoveFolder(MoveFolderRequest request, UserContext user);
        void ChangeConfidentialityLevel(int folderId, int newLevel, UserContext user);
        void CopyFolderStructure(int sourceFolderId, int destinationParentFolderId, UserContext user);
        void ApplyTemplate(int parentFolderId, int templateId, UserContext user);
        IList<Folder> GetDescendants(int folderId);
    }
}
