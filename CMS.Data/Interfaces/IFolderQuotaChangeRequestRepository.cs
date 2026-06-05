using System.Collections.Generic;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IFolderQuotaChangeRequestRepository
    {
        int Insert(FolderQuotaChangeRequest request);
        FolderQuotaChangeRequest GetById(int requestId);
        IEnumerable<FolderQuotaChangeRequest> GetByOrgUnit(int orgUnitId);
        IEnumerable<FolderQuotaChangeRequest> GetPending();
        IEnumerable<FolderQuotaChangeRequest> GetAll();
        void UpdateStatus(int requestId, QuotaRequestStatus status, int reviewedBy, string remark);
    }
}
