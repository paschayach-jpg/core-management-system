using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFileCommandController
    {
        void RenameFile(int fileId, string newName, UserContext user);
        void DeleteFile(int fileId, UserContext user);
        void MoveFiles(MoveFileRequest request, UserContext user);
        int CopyFile(int fileId, int destinationFolderId, UserContext user);
    }
}
