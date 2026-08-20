using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IProjectAuditRepository
    {
        void Insert(ProjectAuditEntry entry);
    }
}
