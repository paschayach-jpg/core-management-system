using System;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class UploadCheckpointController : IUploadCheckpointController
    {
        private readonly IUploadCheckpointService _uploadCheckpointService;

        public UploadCheckpointController(IUploadCheckpointService uploadCheckpointService = null)
        {
            _uploadCheckpointService = uploadCheckpointService;
        }

        public void Save(UploadCheckpoint checkpoint)
        {
            if (_uploadCheckpointService == null)
                return;

            _uploadCheckpointService.Save(checkpoint);
        }

        public UploadCheckpoint Load(int folderId, string fileName)
        {
            if (_uploadCheckpointService == null)
                return null;

            return _uploadCheckpointService.Load(folderId, fileName);
        }

        public void Delete(int folderId, string fileName)
        {
            if (_uploadCheckpointService == null)
                return;

            _uploadCheckpointService.Delete(folderId, fileName);
        }

        public void DeleteExpired(TimeSpan maxAge)
        {
            if (_uploadCheckpointService == null)
                return;

            _uploadCheckpointService.DeleteExpired(maxAge);
        }
    }
}