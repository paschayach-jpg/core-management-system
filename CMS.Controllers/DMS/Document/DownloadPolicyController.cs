using System;
using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class DownloadPolicyController : IDownloadPolicyController
    {
        private readonly IDownloadPolicyService _service;

        public DownloadPolicyController(IDownloadPolicyService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            _service = service;
        }

        public DownloadCheckResult CheckDownload(int fileId, UserContext user)
        {
            return _service.CheckDownload(fileId, user);
        }

        public FileDownloadResult DownloadForSave(int fileId, UserContext user)
        {
            return _service.DownloadForSave(fileId, user);
        }

        public IList<MyDownloadRequestItem> GetMyDownloadRequests(UserContext user)
        {
            return _service.GetMyDownloadRequests(user);
        }

        public IList<DownloadUserPermission> GetUserPermissions()
        {
            return _service.GetUserPermissions();
        }

        public void SaveUserPermission(DownloadUserPermission permission, UserContext user)
        {
            _service.SaveUserPermission(permission, user);
        }

        public void RemoveUserPermission(int userId, UserContext user)
        {
            _service.RemoveUserPermission(userId, user);
        }
    }
}
