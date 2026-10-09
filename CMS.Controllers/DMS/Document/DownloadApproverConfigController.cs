using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class DownloadApproverConfigController : IDownloadApproverConfigController
    {
        private readonly IDownloadApproverConfigService _service;

        public DownloadApproverConfigController(IDownloadApproverConfigService service = null)
        {
            _service = service;
        }

        public IList<DownloadApproverConfig> GetAll(string approvalType = DownloadApproverConfig.TypeDownload)
        {
            if (_service == null)
                return new List<DownloadApproverConfig>();

            return _service.GetAll(approvalType);
        }

        public IList<DownloadApproverConfig> GetByOrg(string scopeType, string orgCode, string approvalType = DownloadApproverConfig.TypeDownload)
        {
            if (_service == null)
                return new List<DownloadApproverConfig>();

            return _service.GetByOrg(scopeType, orgCode, approvalType);
        }

        public void SaveForOrg(string scopeType, string orgCode, IList<int> approverUserIds, UserContext user, string approvalType = DownloadApproverConfig.TypeDownload)
        {
            if (_service == null || user == null)
                return;

            _service.SaveForOrg(scopeType, orgCode, approverUserIds, user, approvalType);
        }
    }
}
