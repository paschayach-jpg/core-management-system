using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class RecycleBinController : IRecycleBinController
    {
        private readonly IFolderService           _folderService;
        private readonly IOrgStructureService      _orgStructureService;
        private readonly IFileDocumentRepository   _fileDocumentRepository;
        private readonly IFolderRepository         _folderRepository;

        public RecycleBinController(
            IFolderService           folderService           = null,
            IOrgStructureService     orgStructureService     = null,
            IFileDocumentRepository  fileDocumentRepository  = null,
            IFolderRepository        folderRepository        = null)
        {
            _folderService          = folderService;
            _orgStructureService    = orgStructureService;
            _fileDocumentRepository = fileDocumentRepository;
            _folderRepository       = folderRepository;
        }

        public IList<OrgUnit> GetOrgHierarchy()
        {
            if (_orgStructureService == null)
                return new List<OrgUnit>();

            return _orgStructureService.GetHierarchy().ToList();
        }

        public IList<Folder> GetDeletedFolders(int? orgUnitId, UserContext user)
        {
            if (_folderService == null)
                return new List<Folder>();

            return _folderService.GetDeletedFolders(orgUnitId, user).ToList();
        }

        public void RestoreFolder(int folderId, int? overrideParentFolderId, UserContext user)
        {
            if (_folderService == null)
                return;

            _folderService.RestoreFolder(folderId, overrideParentFolderId, user);
        }

        public void PermanentlyDeleteFolder(int folderId, UserContext user)
        {
            if (_folderService == null)
                return;

            if (user != null && user.Role != UserRole.Admin && _folderRepository != null)
            {
                var folder = _folderRepository.GetById(folderId);
                if (folder != null && folder.DeletedBy.HasValue && folder.DeletedBy.Value != user.UserID)
                    throw new UnauthorizedAccessException("ไม่มีสิทธิ์ลบถาวรรายการของผู้อื่น");
            }

            _folderService.PermanentlyDeleteFolder(folderId, user);
        }

        public IList<FileDocument> GetDeletedFiles(int? orgUnitId, UserContext user)
        {
            if (_fileDocumentRepository == null)
                return new List<FileDocument>();

            return _fileDocumentRepository.GetDeletedFiles(orgUnitId).ToList();
        }

        public void RestoreFile(int fileId, UserContext user)
        {
            if (_fileDocumentRepository == null)
                return;

            _fileDocumentRepository.RestoreFile(fileId);
        }

        public void PermanentlyDeleteFile(int fileId, UserContext user)
        {
            if (_fileDocumentRepository == null)
                return;

            if (user != null && user.Role != UserRole.Admin)
            {
                var file = _fileDocumentRepository.GetById(fileId);
                if (file != null && file.DeletedBy.HasValue && file.DeletedBy.Value != user.UserID)
                    throw new UnauthorizedAccessException("ไม่มีสิทธิ์ลบถาวรรายการของผู้อื่น");
            }

            _fileDocumentRepository.PermanentDelete(fileId);
        }
    }
}
