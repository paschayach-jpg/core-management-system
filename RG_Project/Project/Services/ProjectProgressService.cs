using RG_Project.Project.Controllers;
using RG_Project.Project.Models;
using System.Data;

namespace RG_Project.Project.Services
{
    public class ProjectProgressService
    {
        private readonly ProjectProgressController _progressCtrl;
        private readonly ProjectStateController    _stateCtrl;
        private readonly ProjectController         _projectCtrl;

        public ProjectProgressService()
        {
            _progressCtrl = new ProjectProgressController();
            _stateCtrl    = new ProjectStateController();
            _projectCtrl  = new ProjectController();
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
                int progressId = _progressCtrl.InsertProgress(model);
                if (progressId < 0)
                {
                    errorMessage = "บันทึกความคืบหน้าไม่สำเร็จ";
                    return false;
                }

                // Step 3: Update state status
                _stateCtrl.UpdateStateStatus(model.i_proj_state_id, model.c_status, model.i_action_user);

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

            if (_projectCtrl.AllRequiredStatesCompleted(i_project_id)
                && _progressCtrl.HasAnyProgress(i_project_id))
            {
                newProjectStatus = "Completed";
            }
            else if (_progressCtrl.HasAnyProgress(i_project_id))
            {
                newProjectStatus = "InProgress";
            }
            else
            {
                newProjectStatus = "Open";
            }

            _projectCtrl.UpdateProjectStatus(i_project_id, newProjectStatus, i_user);
        }

        /// <summary>
        /// แปลง pct เป็น status string
        /// </summary>
        private string GetStatusFromPct(int pct)
        {
            if (pct == 0)   return "Pending";
            if (pct == 100) return "Completed";
            return "InProgress";
        }

        /// <summary>
        /// ดึง Progress history ของ State
        /// </summary>
        public DataTable GetProgressHistory(int i_proj_state_id)
        {
            return _progressCtrl.GetProgressHistory(i_proj_state_id);
        }

        /// <summary>
        /// ดึง latest progress ต่อ State ทั้งหมดของ Project
        /// </summary>
        public DataTable GetLatestProgressByProject(int i_project_id)
        {
            return _progressCtrl.GetLatestProgressByProject(i_project_id);
        }

        /// <summary>
        /// คำนวณ overall progress %
        /// </summary>
        public int GetOverallCompletionPct(int i_project_id)
        {
            return _progressCtrl.CalculateOverallProgress(i_project_id);
        }
    }
}
