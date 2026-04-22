using System;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FileCommandController : IFileCommandController
    {
        private readonly IFileService _fileService;

        public FileCommandController(IFileService fileService = null)
        {
            _fileService = fileService;
        }

        public void RenameFile(int fileId, string newName, UserContext user)
        {
            if (_fileService == null)
                throw new InvalidOperationException("File service is not available.");

            _fileService.RenameFile(fileId, newName, user);
        }

        public void DeleteFile(int fileId, UserContext user)
        {
            if (_fileService == null)
                throw new InvalidOperationException("File service is not available.");

            _fileService.DeleteFile(fileId, user);
        }

        public void MoveFiles(MoveFileRequest request, UserContext user)
        {
            if (_fileService == null)
                throw new InvalidOperationException("File service is not available.");

            _fileService.MoveFiles(request, user);
        }

        public int CopyFile(int fileId, int destinationFolderId, UserContext user)
        {
            if (_fileService == null)
                throw new InvalidOperationException("File service is not available.");

            return _fileService.CopyFile(fileId, destinationFolderId, user);
        }
    }
}
