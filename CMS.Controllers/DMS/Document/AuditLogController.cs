using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class AuditLogController : IAuditLogController
    {
        private readonly IAuditService _auditService;

        public AuditLogController(IAuditService auditService = null)
        {
            _auditService = auditService;
        }

        public void Log(AuditEntry entry)
        {
            if (_auditService == null || entry == null)
                return;

            _auditService.Log(entry);
        }

        public PagedResult<AuditLog> Query(AuditQuery query, UserContext user)
        {
            if (_auditService == null || query == null || user == null)
                return new PagedResult<AuditLog>();

            return _auditService.Query(query, user);
        }
    }
}
