using System.Data;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class ProjectProgressService : IProjectProgressService
    {
        private readonly IProjectProgressRepository _progressRepo;
        private readonly IProjectStateRepository _stateRepo;
        private readonly IProjectRepository _projectRepo;

        public ProjectProgressService(
            IProjectProgressRepository progressRepo,
            IProjectStateRepository stateRepo,
            IProjectRepository projectRepo)
        {
            _progressRepo = progressRepo;
            _stateRepo = stateRepo;
            _projectRepo = projectRepo;
        }

        /// <summary>
        /// อัปเดต Progress ของ State:
        /// 1. Insert t_project_progress
        /// 2. Update t_project_state.c_status ตาม pct
        /// 3. Recalculate t_project.c_status
        /// </summary>
        public bool UpdateStateProgress(ProjectProgressModel model, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // Step 1: Set status from pct
                model.c_status = GetStatusFromPct(model.i_pct_complete);

                // Step 2: Insert progress record
                int progressId = _progressRepo.InsertProgress(model);
                if (progressId < 0)
                {
                    errorMessage = "บันทึกความคืบหน้าไม่สำเร็จ";
                    return false;
                }

                // Step 3: Update state status
                _stateRepo.UpdateStateStatus(model.i_proj_state_id, model.c_status, model.i_action_user);

                // Step 4: Recalculate project status
                RecalculateProjectStatus(model.i_project_id, model.i_action_user);

                return true;
            }
            catch (System.Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// คำนวณ project status ใหม่จากสถานะ states ทั้งหมด
        /// </summary>
        private void RecalculateProjectStatus(int i_project_id, string i_user)
        {
            string newProjectStatus;

            if (_projectRepo.AllRequiredStatesCompleted(i_project_id)
                && _progressRepo.HasAnyProgress(i_project_id))
            {
                newProjectStatus = "Completed";
            }
            else if (_progressRepo.HasAnyProgress(i_project_id))
            {
                newProjectStatus = "InProgress";
            }
            else
            {
                newProjectStatus = "Open";
            }

            _projectRepo.UpdateProjectStatus(i_project_id, newProjectStatus, i_user);
        }

        /// <summary>
        /// แปลง pct เป็น status string
        /// </summary>
        private string GetStatusFromPct(int pct)
        {
            if (pct == 0) return "Pending";
            if (pct == 100) return "Completed";
            return "InProgress";
        }

        /// <summary>
        /// ดึง Progress history ของ State
        /// </summary>
        public DataTable GetProgressHistory(int i_proj_state_id)
        {
            return _progressRepo.GetProgressHistory(i_proj_state_id);
        }

        /// <summary>
        /// ดึง latest progress ต่อ State ทั้งหมดของ Project
        /// </summary>
        public DataTable GetLatestProgressByProject(int i_project_id)
        {
            return _progressRepo.GetLatestProgressByProject(i_project_id);
        }

        /// <summary>
        /// คำนวณ overall progress %
        /// </summary>
        public int GetOverallCompletionPct(int i_project_id)
        {
            return _progressRepo.CalculateOverallProgress(i_project_id);
        }
    }
}
