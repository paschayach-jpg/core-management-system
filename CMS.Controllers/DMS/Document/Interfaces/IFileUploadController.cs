using System;
using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFileUploadController
    {
        void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files, int folderId, UserContext user);
        void DownloadFile(IFileUploadView view, int fileDocumentId, UserContext user, Func<string, string> getSavePath);

        /// <summary>Returns the subset of file names that already exist (active) in the target folder.</summary>
        IEnumerable<string> FindDuplicateNames(IEnumerable<string> fileNames, int folderId, UserContext user);
    }
}
