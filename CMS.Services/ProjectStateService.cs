using System;
using System.Collections.Generic;
using System.Data;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class ProjectStateService : IProjectStateService
    {
        private readonly IProjectStateRepository _stateRepo;
        private readonly IProjectAuditService _auditService;

        public ProjectStateService(IProjectStateRepository stateRepo, IProjectAuditService auditService)
        {
            _stateRepo = stateRepo;
            _auditService = auditService;
        }

        /// <summary>
        /// ดึง States ของ Project พร้อม latest progress (สำหรับ Tab State)
        /// </summary>
        public DataTable GetStatesWithProgress(int i_project_id)
        {
            return _stateRepo.GetStatesByProject(i_project_id);
        }

        /// <summary>
        /// ดึง States ของ Template (สำหรับ preview)
        /// </summary>
        public DataTable GetTemplateStates(int i_template_id)
        {
            return _stateRepo.GetTemplateStates(i_template_id);
        }

        /// <summary>
        /// เพิ่ม State ใหม่ใน Project (auto-seq_no)
        /// </summary>
        public int AddState(ProjectStateModel model)
        {
            model.i_seq_no = _stateRepo.GetNextSeqNo(model.i_project_id);
            int newId = _stateRepo.InsertState(model);
            _auditService.Log(new ProjectAuditEntry
            {
                UserID = model.i_create_user,
                Module = "Project",
                Action = newId > 0 ? "CREATE_PROJECT_STATE" : "CREATE_PROJECT_STATE_FAILED",
                TableName = "t_project_state",
                RecordID = newId > 0 ? newId : (int?)null,
                Result = newId > 0 ? 1 : 2,
            });
            return newId;
        }

        /// <summary>
        /// แก้ไข State
        /// </summary>
        public bool UpdateState(ProjectStateModel model)
        {
            return _stateRepo.UpdateState(model);
        }

        /// <summary>
        /// ลบ State — blocked ถ้ามี progress records
        /// </summary>
        public bool DeleteState(int i_proj_state_id, out string errorMessage)
        {
            errorMessage = "";
            if (_stateRepo.HasProgressRecords(i_proj_state_id))
            {
                errorMessage = "ไม่สามารถลบขั้นตอนได้ เนื่องจากมีประวัติการอัปเดตความคืบหน้าแล้ว";
                return false;
            }

            bool ok = _stateRepo.DeleteState(i_proj_state_id);
            _auditService.Log(new ProjectAuditEntry
            {
                Module = "Project",
                Action = ok ? "DELETE_PROJECT_STATE" : "DELETE_PROJECT_STATE_FAILED",
                TableName = "t_project_state",
                RecordID = i_proj_state_id,
                Result = ok ? 1 : 2,
            });
            return ok;
        }

        /// <summary>
        /// ดึง States สำหรับ ComboBox
        /// </summary>
        public DataTable GetStatesForCombo(int i_project_id)
        {
            return _stateRepo.GetStatesForCombo(i_project_id);
        }

        /// <summary>
        /// ผลรวมวันคาดการณ์ของ project ยกเว้น state ที่กำลัง edit
        /// </summary>
        public int GetTotalEstimatedDays(int i_project_id, int excludeStateId = 0)
        {
            return _stateRepo.GetTotalEstimatedDays(i_project_id, excludeStateId);
        }

        /// <summary>
        /// ดึงโครงการที่ยังไม่ Completed สำหรับ ComboBox เลือก ref project
        /// </summary>
        public DataTable GetActiveProjectsForRef(int i_exclude_project_id)
        {
            return _stateRepo.GetActiveProjectsForRef(i_exclude_project_id);
        }

        /// <summary>Reorder states: update i_seq_no สำหรับหลาย state พร้อมกัน</summary>
        public bool ReorderStates(List<Tuple<int, int>> orderList)
        {
            return _stateRepo.ReorderStates(orderList);
        }
    }
}
