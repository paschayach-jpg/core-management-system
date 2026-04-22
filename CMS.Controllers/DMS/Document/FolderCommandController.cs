using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FolderCommandController : IFolderCommandController
    {
        private readonly IFolderService _folderService;
        public Folder CreateFolder(CreateFolderRequest request, UserContext user)
        {
            if (_folderService == null || request == null || user == null)
                return null;

            return _folderService.CreateFolder(request, user);
        }

        public Folder CreateHCFolderAfterApproval(ApprovalRequest approvalRequest, UserContext user)
        {
            if (_folderService == null || approvalRequest == null || user == null)
                return null;

            return _folderService.CreateHCFolderAfterApproval(approvalRequest, user);
        }

        public void RenameFolder(int folderId, string newName, UserContext user)
        {
            if (_folderService == null || user == null)
                return;

            _folderService.RenameFolder(folderId, newName, user);
        }

        public void ArchiveFolder(int folderId, UserContext user)
        {
            if (_folderService == null || user == null)
                return;

            _folderService.ArchiveFolder(folderId, user);
        }

        public void DeleteFolder(int folderId, UserContext user)
        {
            if (_folderService == null || user == null)
                return;

            _folderService.DeleteFolder(folderId, user);
        }

        public void RestoreFolder(int folderId, int? newParentId, UserContext user)
        {
            if (_folderService == null || user == null)
                return;

            _folderService.RestoreFolder(folderId, newParentId, user);
        }

        public void PermanentlyDeleteFolder(int folderId, UserContext user)
        {
            if (_folderService == null || user == null)
                return;

            _folderService.PermanentlyDeleteFolder(folderId, user);
        }


        public FolderCommandController(IFolderService folderService = null)
        {
            _folderService = folderService;
        }

        public void MoveFolder(MoveFolderRequest request, UserContext user)
        {
            if (_folderService == null || request == null || user == null)
                return;

            _folderService.MoveFolder(request, user);
        }

        public void ChangeConfidentialityLevel(int folderId, int newLevel, UserContext user)
        {
            if (_folderService == null || user == null)
                return;

            _folderService.ChangeConfidentialityLevel(folderId, newLevel, user);
        }

        public void CopyFolderStructure(int sourceFolderId, int destinationParentFolderId, UserContext user)
        {
            if (_folderService == null || user == null)
                return;

            _folderService.CopyFolderStructure(sourceFolderId, destinationParentFolderId, user);
        }

        public void ApplyTemplate(int parentFolderId, int templateId, UserContext user)
        {
            if (_folderService == null || user == null)
                return;

            _folderService.ApplyTemplate(parentFolderId, templateId, user);
        }

        public IList<Folder> GetDescendants(int folderId)
        {
            if (_folderService == null)
                return new List<Folder>();

            return _folderService.GetDescendants(folderId).ToList();
        }
    }
}
