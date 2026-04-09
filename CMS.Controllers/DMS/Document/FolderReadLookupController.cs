using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FolderReadLookupController : IFolderReadLookupController
    {
        private readonly IFolderRepository _folderRepository;

        public FolderReadLookupController(IFolderRepository folderRepository = null)
        {
            _folderRepository = folderRepository;
        }

        public IList<Folder> GetActiveFolders()
        {
            if (_folderRepository == null)
                return new List<Folder>();

            return _folderRepository.GetActiveFolders().ToList();
        }

        public IList<Folder> GetChildren(int folderId)
        {
            if (_folderRepository == null)
                return new List<Folder>();

            return _folderRepository.GetChildren(folderId).ToList();
        }

        public Folder GetById(int folderId)
        {
            if (_folderRepository == null)
                return null;

            return _folderRepository.GetById(folderId);
        }
    }
}
