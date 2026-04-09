using System;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public class FolderSettingsController : IFolderSettingsController
    {
        private readonly IFolderRepository _folderRepository;

        public FolderSettingsController(IFolderRepository folderRepository)
        {
            _folderRepository = folderRepository;
        }

        public void UpdateFolder(Folder folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (_folderRepository == null)
                throw new InvalidOperationException("Folder repository is not available.");

            _folderRepository.Update(folder);
        }
    }
}
