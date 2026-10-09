using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IDownloadApproverConfigController
    {
        IList<DownloadApproverConfig> GetAll(string approvalType = DownloadApproverConfig.TypeDownload);
        IList<DownloadApproverConfig> GetByOrg(string scopeType, string orgCode, string approvalType = DownloadApproverConfig.TypeDownload);
        void SaveForOrg(string scopeType, string orgCode, IList<int> approverUserIds, UserContext user, string approvalType = DownloadApproverConfig.TypeDownload);
    }
}
