using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IProjectStateRepository
    {
        DataTable GetStatesByProject(int i_project_id);
        DataTable GetTemplateStates(int i_template_id);
        bool InsertStatesFromTemplate(int i_project_id, int i_template_id, string i_user);
        int InsertState(ProjectStateModel model);
        bool UpdateState(ProjectStateModel model);
        bool DeleteState(int i_proj_state_id);
        bool UpdateStateStatus(int i_proj_state_id, string c_status, string i_user);
        bool HasProgressRecords(int i_proj_state_id);
        bool ReorderStates(List<Tuple<int, int>> orderList);
        int GetNextSeqNo(int i_project_id);
        int GetTotalEstimatedDays(int i_project_id, int excludeStateId = 0);
        DataTable GetActiveProjectsForRef(int i_exclude_project_id);
        DataTable GetStatesForCombo(int i_project_id);
    }
}
