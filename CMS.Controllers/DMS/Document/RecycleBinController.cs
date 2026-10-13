using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class RecycleBinController : IRecycleBinController
    {
        private readonly IFolderService _folderService;
        private readonly IOrgStructureService _orgStructureService;

        public RecycleBinController(
            IFolderService folderService = null,
            IOrgStructureService orgStructureService = null)
        {
            _folderService = folderService;
            _orgStructureService = orgStructureService;
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

            _folderService.PermanentlyDeleteFolder(folderId, user);
        }

        public bool CanRestoreFolder(Folder folder, UserContext user)
        {
            return _folderService != null && _folderService.CanRestoreDeletedFolder(folder, user);
        }

        public bool CanPermanentlyDeleteFolder(Folder folder, UserContext user)
        {
            return _folderService != null && _folderService.CanPermanentlyDeleteFolder(folder, user);
        }

        public IList<FileDocument> GetDeletedFiles(int? orgUnitId, UserContext user)
        {
            if (_folderService == null)
                return new List<FileDocument>();

            return _folderService.GetDeletedFiles(orgUnitId, user).ToList();
        }

        public void RestoreFile(int fileId, UserContext user)
        {
            if (_folderService == null)
                return;

            _folderService.RestoreFile(fileId, user);
        }

        public void PermanentlyDeleteFile(int fileId, UserContext user)
        {
            if (_folderService == null)
                return;

            _folderService.PermanentlyDeleteFile(fileId, user);
        }

        public bool CanManageFile(FileDocument file, UserContext user)
        {
            return _folderService != null && _folderService.CanManageDeletedFile(file, user);
        }
    }
}
