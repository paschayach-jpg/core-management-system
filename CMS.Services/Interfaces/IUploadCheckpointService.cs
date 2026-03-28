using System;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IUploadCheckpointService
    {
        void Save(UploadCheckpoint checkpoint);
        UploadCheckpoint Load(int folderId, string fileName);
        void Delete(int folderId, string fileName);
        void DeleteExpired(TimeSpan maxAge);
    }
}
