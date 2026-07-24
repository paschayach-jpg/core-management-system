using System;
using System.Collections.Generic;
using System.Data;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectStateService สำหรับ Presentation ของโมดูล Project (RGP)</summary>
    public sealed class ProjectStateController
    {
        private readonly IProjectStateService _stateService;

        public ProjectStateController() : this(RgpComposition.StateService) { }

        public ProjectStateController(IProjectStateService stateService)
        {
            _stateService = stateService;
        }

        public DataTable GetStatesWithProgress(int i_project_id) => _stateService.GetStatesWithProgress(i_project_id);

        public DataTable GetTemplateStates(int i_template_id) => _stateService.GetTemplateStates(i_template_id);

        public int AddState(ProjectStateModel model) => _stateService.AddState(model);

        public bool UpdateState(ProjectStateModel model) => _stateService.UpdateState(model);

        public bool DeleteState(int i_proj_state_id, out string errorMessage) =>
            _stateService.DeleteState(i_proj_state_id, out errorMessage);

        public DataTable GetStatesForCombo(int i_project_id) => _stateService.GetStatesForCombo(i_project_id);

        public int GetTotalEstimatedDays(int i_project_id, int excludeStateId = 0) =>
            _stateService.GetTotalEstimatedDays(i_project_id, excludeStateId);

        public DataTable GetActiveProjectsForRef(int i_exclude_project_id) =>
            _stateService.GetActiveProjectsForRef(i_exclude_project_id);

        public bool ReorderStates(List<Tuple<int, int>> orderList) => _stateService.ReorderStates(orderList);
    }
}
