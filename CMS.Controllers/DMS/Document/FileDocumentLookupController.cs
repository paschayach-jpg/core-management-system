using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FileDocumentLookupController : IFileDocumentLookupController
    {
        private readonly IFileDocumentRepository _fileDocumentRepository;

        public FileDocumentLookupController(IFileDocumentRepository fileDocumentRepository = null)
        {
            _fileDocumentRepository = fileDocumentRepository;
        }

        public IList<FileDocument> GetByFolder(int folderId)
        {
            if (_fileDocumentRepository == null)
                return new List<FileDocument>();

            return _fileDocumentRepository.GetByFolder(folderId).ToList();
        }

        public IList<FileDocument> GetAllGroupedByFolder()
        {
            if (_fileDocumentRepository == null)
                return new List<FileDocument>();

            return _fileDocumentRepository.GetAllGroupedByFolder().ToList();
        }
    }
}
