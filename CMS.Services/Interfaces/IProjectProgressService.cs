using System.Data;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IProjectProgressService
    {
        bool UpdateStateProgress(ProjectProgressModel model, out string errorMessage);
        DataTable GetProgressHistory(int i_proj_state_id);
        DataTable GetLatestProgressByProject(int i_project_id);
        int GetOverallCompletionPct(int i_project_id);
    }
}
