using CMS.Data.Models;
using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Data.Interfaces
{
    public interface IAuditRepository
    {
        long Insert(AuditLog entry);
        PagedResult<AuditLog> Query(AuditQuery query);
        List<AuditLog> GetAuditLogsByWorkflowInstance(int workflowInstanceId);
    }
}
