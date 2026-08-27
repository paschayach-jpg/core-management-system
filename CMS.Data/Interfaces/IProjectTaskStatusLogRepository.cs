using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IProjectTaskStatusLogRepository
    {
        bool Insert(ProjectTaskStatusLogModel m);
    }
}
