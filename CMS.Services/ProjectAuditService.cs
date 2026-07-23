using System;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Audit service เฉพาะโมดูล Project (RGP) — เทียบเท่า CMS.Services.AuditService แต่แยกกัน
    /// เพราะ shape/ตารางต่างกัน (t_project_audit_log ไม่ใช่ AuditLog ของ CMS)
    /// </summary>
    public class ProjectAuditService : IProjectAuditService
    {
        private readonly IProjectAuditRepository _auditRepository;

        public ProjectAuditService(IProjectAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public void Log(ProjectAuditEntry entry)
        {
            entry.ActionDate = DateTime.Now;
            entry.MachineName = Environment.MachineName;
            _auditRepository.Insert(entry);
        }
    }
}
