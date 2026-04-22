using System;
using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFileUploadController
    {
        void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files, int folderId, UserContext user);
        void DownloadFile(IFileUploadView view, int fileDocumentId, UserContext user, Func<string, string> getSavePath);
    }
}
