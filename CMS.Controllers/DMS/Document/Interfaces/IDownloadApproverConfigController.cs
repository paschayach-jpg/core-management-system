using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IDownloadApproverConfigController
    {
        IList<DownloadApproverConfig> GetAll();
        IList<DownloadApproverConfig> GetByOrg(string scopeType, string orgCode);
        void SaveForOrg(string scopeType, string orgCode, IList<int> approverUserIds, UserContext user);
    }
}
