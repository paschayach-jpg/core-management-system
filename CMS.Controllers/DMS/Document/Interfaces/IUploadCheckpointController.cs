using System;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IUploadCheckpointController
    {
        void Save(UploadCheckpoint checkpoint);
        UploadCheckpoint Load(int folderId, string fileName);
        void Delete(int folderId, string fileName);
        void DeleteExpired(TimeSpan maxAge);
    }
}