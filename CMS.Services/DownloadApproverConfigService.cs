using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class DownloadApproverConfigService : IDownloadApproverConfigService
    {
        private const int AdminRole = 4;

        private readonly IDownloadApproverConfigRepository _repo;
        private readonly IAuditService _auditService;

        public DownloadApproverConfigService(IDownloadApproverConfigRepository repo, IAuditService auditService)
        {
            if (repo         == null) throw new ArgumentNullException("repo");
            if (auditService == null) throw new ArgumentNullException("auditService");
            _repo         = repo;
            _auditService = auditService;
        }

        public IList<DownloadApproverConfig> GetAll()
        {
            return _repo.GetAll().ToList();
        }

        public IList<DownloadApproverConfig> GetByOrg(string scopeType, string orgCode)
        {
            return _repo.GetByOrg(scopeType, orgCode).ToList();
        }

        public void SaveForOrg(string scopeType, string orgCode, IList<int> approverUserIds, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole != AdminRole)
                throw new UnauthorizedAccessException("การตั้งค่าผู้อนุมัติดาวน์โหลดทำได้เฉพาะ Admin");
            if (scopeType != DownloadApproverConfig.ScopeDepartment && scopeType != DownloadApproverConfig.ScopeDivision)
                throw new ArgumentException("scopeType ต้องเป็น 'D' หรือ 'V'", "scopeType");
            if (string.IsNullOrWhiteSpace(orgCode))
                throw new ArgumentException("orgCode ต้องไม่ว่าง", "orgCode");
            if (approverUserIds == null) throw new ArgumentNullException("approverUserIds");

            var ids = approverUserIds.Where(id => id > 0).Distinct().ToList();
            string oldValue = string.Join(",", _repo.GetByOrg(scopeType, orgCode).Select(c => c.ApproverUserID));

            _repo.ReplaceForOrg(scopeType, orgCode, ids, user.UserID);

            _auditService.Log(new AuditEntry
            {
                Module    = "Workflow",
                Action    = "SAVE_DOWNLOAD_APPROVERS",
                UserID    = user.UserID,
                UserName  = user.ADUserName ?? string.Empty,
                UserRole  = user.AppRole,
                TableName = "workflow.DownloadApproverConfig",
                OldValue  = string.Format("{0}:{1} Approvers:[{2}]", scopeType, orgCode, oldValue),
                NewValue  = string.Format("{0}:{1} Approvers:[{2}]", scopeType, orgCode, string.Join(",", ids)),
                Result    = 1,
                AuditDate = DateTime.Now
            });
        }
    }
}
