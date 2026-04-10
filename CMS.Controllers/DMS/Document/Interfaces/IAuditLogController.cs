using CMS.Data.Models;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IAuditLogController
    {
        void Log(AuditEntry entry);
        PagedResult<AuditLog> Query(AuditQuery query, UserContext user);
    }
}
