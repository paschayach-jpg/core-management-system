using System.Data;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectProgressService สำหรับ Presentation ของโมดูล Project (RGP)</summary>
    public sealed class ProjectProgressController
    {
        private readonly IProjectProgressService _progressService;

        public ProjectProgressController() : this(RgpComposition.ProgressService) { }

        public ProjectProgressController(IProjectProgressService progressService)
        {
            _progressService = progressService;
        }

        public bool UpdateStateProgress(ProjectProgressModel model, out string errorMessage) =>
            _progressService.UpdateStateProgress(model, out errorMessage);

        public DataTable GetProgressHistory(int i_proj_state_id) => _progressService.GetProgressHistory(i_proj_state_id);

        public DataTable GetLatestProgressByProject(int i_project_id) => _progressService.GetLatestProgressByProject(i_project_id);

        public int GetOverallCompletionPct(int i_project_id) => _progressService.GetOverallCompletionPct(i_project_id);
    }
}
