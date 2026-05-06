using CMS.Domain.Models;
using CMS.Services.DTOs;
using System.Collections.Generic;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFileCommandController
    {
        void RenameFile(int fileId, string newName, UserContext user);
        void DeleteFile(int fileId, UserContext user);
        void MoveFiles(MoveFileRequest request, UserContext user);
        int CopyFile(int fileId, int destinationFolderId, UserContext user);
        void HideFiles(List<int> fileIds, UserContext user);
        void UnhideFiles(List<int> fileIds, UserContext user);
    }
}
