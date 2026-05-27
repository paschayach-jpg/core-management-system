using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    public interface IFolderQuotaService
    {
        // ---- Usage ----
        QuotaUsageResult GetUsage(int orgUnitId, int? parentOrgUnitId = null);
        QuotaAlertLevel  CheckAlert(int orgUnitId, int? parentOrgUnitId = null);

        /// <summary>โยน QuotaExceededException ถ้า quota เต็ม</summary>
        void EnforceQuota(int orgUnitId, int? parentOrgUnitId = null);

        /// <summary>ส่ง notification แจ้งเตือนถ้า usage ถึง threshold (fire-and-forget safe)</summary>
        void SendAlertIfNeeded(int orgUnitId, int? parentOrgUnitId = null);

        // ---- Config ----
        IEnumerable<FolderQuotaConfig> GetAllConfigs();
        FolderQuotaConfig GetConfig(int orgUnitId, int? parentOrgUnitId = null);
        void SaveConfig(FolderQuotaConfig config, UserContext user);
        void DeleteConfig(int quotaConfigId, string orgUnitTypeName, UserContext user);

        // ---- Change Request ----
        int SubmitChangeRequest(SubmitQuotaChangeRequest request, UserContext user);
        IEnumerable<FolderQuotaChangeRequest> GetPendingRequests();
        IEnumerable<FolderQuotaChangeRequest> GetAllRequests();
        IEnumerable<FolderQuotaChangeRequest> GetRequestsByOrgUnit(int orgUnitId);
        void ApproveRequest(int requestId, string remark, UserContext user);
        void RejectRequest(int requestId, string remark, UserContext user);
        void CancelRequest(int requestId, UserContext user);
    }
}
