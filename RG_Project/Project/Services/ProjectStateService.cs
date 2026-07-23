using RG_Project.Project.Controllers;
using CMS.Domain.Models;
using System.Data;

namespace RG_Project.Project.Services
{
    public class ProjectStateService
    {
        private readonly ProjectStateController _stateCtrl;

        public ProjectStateService()
        {
            _stateCtrl = new ProjectStateController();
        }

        /// <summary>
        /// ดึง States ของ Project พร้อม latest progress (สำหรับ Tab State)
        /// </summary>
        public DataTable GetStatesWithProgress(int i_project_id)
        {
            return _stateCtrl.GetStatesByProject(i_project_id);
        }

        /// <summary>
        /// ดึง States ของ Template (สำหรับ preview)
        /// </summary>
        public DataTable GetTemplateStates(int i_template_id)
        {
            return _stateCtrl.GetTemplateStates(i_template_id);
        }

        /// <summary>
        /// เพิ่ม State ใหม่ใน Project (auto-seq_no)
        /// </summary>
        public int AddState(ProjectStateModel model)
        {
            model.i_seq_no = _stateCtrl.GetNextSeqNo(model.i_project_id);
            return _stateCtrl.InsertState(model);
        }

        /// <summary>
        /// แก้ไข State
        /// </summary>
        public bool UpdateState(ProjectStateModel model)
        {
            return _stateCtrl.UpdateState(model);
        }

        /// <summary>
        /// ลบ State — blocked ถ้ามี progress records
        /// </summary>
        public bool DeleteState(int i_proj_state_id, out string errorMessage)
        {
            errorMessage = "";
            if (_stateCtrl.HasProgressRecords(i_proj_state_id))
            {
                errorMessage = "ไม่สามารถลบขั้นตอนได้ เนื่องจากมีประวัติการอัปเดตความคืบหน้าแล้ว";
                return false;
            }
            return _stateCtrl.DeleteState(i_proj_state_id);
        }

        /// <summary>
        /// ดึง States สำหรับ ComboBox
        /// </summary>
        public DataTable GetStatesForCombo(int i_project_id)
        {
            return _stateCtrl.GetStatesForCombo(i_project_id);
        }

        /// <summary>
        /// ผลรวมวันคาดการณ์ของ project ยกเว้น state ที่กำลัง edit
        /// </summary>
        public int GetTotalEstimatedDays(int i_project_id, int excludeStateId = 0)
        {
            return _stateCtrl.GetTotalEstimatedDays(i_project_id, excludeStateId);
        }

        /// <summary>
        /// ดึงโครงการที่ยังไม่ Completed สำหรับ ComboBox เลือก ref project
        /// </summary>
        public DataTable GetActiveProjectsForRef(int i_exclude_project_id)
        {
            return _stateCtrl.GetActiveProjectsForRef(i_exclude_project_id);
        }
    }
}
