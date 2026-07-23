using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IProjectProgressRepository
    {
        DataTable GetProgressHistory(int i_proj_state_id);
        DataTable GetLatestProgressByProject(int i_project_id);
        int InsertProgress(ProjectProgressModel model);
        int CalculateOverallProgress(int i_project_id);
        bool HasAnyProgress(int i_project_id);
    }
}
