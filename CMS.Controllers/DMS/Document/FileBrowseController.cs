using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FileBrowseController : IFileBrowseController
    {
        private readonly IFileService _fileService;

        public FileBrowseController(IFileService fileService = null)
        {
            _fileService = fileService;
        }

        public IList<FileDocument> GetFilesInFolder(int folderId, UserContext user)
        {
            if (_fileService == null)
                return new List<FileDocument>();

            return _fileService.GetFilesInFolder(folderId, user).ToList();
        }

        public IList<FileDocument> GetAllFilesGroupedByFolder(UserContext user)
        {
            if (_fileService == null)
                return new List<FileDocument>();

            return _fileService.GetAllFilesGroupedByFolder(user).ToList();
        }

        public IList<FileDocument> GetFilesByOrgUnit(int orgUnitId, UserContext user)
        {
            if (_fileService == null)
                return new List<FileDocument>();

            return _fileService.GetFilesByOrgUnit(orgUnitId, user).ToList();
        }

        public FileDownloadResult DownloadFile(int fileDocumentId, UserContext user, bool bypassPermissionCheck = false)
        {
            if (_fileService == null)
                return new FileDownloadResult { Success = false, ErrorMessage = "File service is not available." };

            return _fileService.DownloadFile(fileDocumentId, user, bypassPermissionCheck);
        }
    }
}
