using CMS.Data.Models;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IAuditRepository
    {
        long Insert(AuditLog entry);
        PagedResult<AuditLog> Query(AuditQuery query);
    }
}
