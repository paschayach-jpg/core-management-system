using System;
using System.Collections.Generic;
using System.Data;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IProjectStateService
    {
        DataTable GetStatesWithProgress(int i_project_id);
        DataTable GetTemplateStates(int i_template_id);
        int AddState(ProjectStateModel model);
        bool UpdateState(ProjectStateModel model);
        bool DeleteState(int i_proj_state_id, out string errorMessage);
        DataTable GetStatesForCombo(int i_project_id);
        int GetTotalEstimatedDays(int i_project_id, int excludeStateId = 0);
        DataTable GetActiveProjectsForRef(int i_exclude_project_id);
        bool ReorderStates(List<Tuple<int, int>> orderList);
    }
}
