using System.Collections.Generic;
using System.IO;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFileBrowseController
    {
        IList<FileDocument> GetFilesInFolder(int folderId, UserContext user);
        IList<FileDocument> GetAllFilesGroupedByFolder(UserContext user);
        IList<FileDocument> GetFilesByOrgUnit(int orgUnitId, UserContext user);
        FileDownloadResult DownloadFile(int fileDocumentId, UserContext user, bool bypassPermissionCheck = false);
        FileOpenMode GetFileOpenMode(int fileDocumentId, UserContext user);
        UploadResult SaveFileNewVersion(int fileDocumentId, Stream content, UserContext user);
    }
}
