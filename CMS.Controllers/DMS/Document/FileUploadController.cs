using System;
using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FileUploadController : IFileUploadController
    {
        private readonly IFileService _fileService;
        private readonly IFolderService _folderService;

        public FileUploadController(IFileService fileService = null, IFolderService folderService = null)
        {
            _fileService = fileService;
            _folderService = folderService;
        }

        public void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files, int folderId, UserContext user)
        {
            if (_fileService == null || _folderService == null || view == null || user == null)
                return;

            var presenter = new FilePresenter(_fileService, _folderService, view, user);
            presenter.UploadFiles(files, folderId);
        }

        public void DownloadFile(IFileUploadView view, int fileDocumentId, UserContext user, Func<string, string> getSavePath)
        {
            if (_fileService == null || _folderService == null || view == null || user == null)
                return;

            var presenter = new FilePresenter(_fileService, _folderService, view, user);
            presenter.DownloadFile(fileDocumentId, getSavePath);
        }

        public IEnumerable<string> FindDuplicateNames(IEnumerable<string> fileNames, int folderId, UserContext user)
        {
            if (_fileService == null || user == null) return new List<string>();
            return _fileService.FindDuplicateNames(fileNames, folderId, user);
        }
    }
}
