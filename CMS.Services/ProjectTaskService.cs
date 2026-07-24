using System;
using System.Data;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly IProjectTaskRepository _taskRepo;
        private readonly IProjectStateRepository _stateRepo;
        private readonly IProjectProgressService _progressSvc;
        private readonly IProjectAuditService _auditService;

        public ProjectTaskService(
            IProjectTaskRepository taskRepo,
            IProjectStateRepository stateRepo,
            IProjectProgressService progressSvc,
            IProjectAuditService auditService)
        {
            _taskRepo = taskRepo;
            _stateRepo = stateRepo;
            _progressSvc = progressSvc;
            _auditService = auditService;
        }

        // ─── Task CRUD ────────────────────────────────────────────────────────────

        public DataTable GetByState(int i_proj_state_id) =>
            _taskRepo.GetByState(i_proj_state_id);

        public DataTable GetByProject(int i_project_id) =>
            _taskRepo.GetByProject(i_project_id);

        public DataTable GetSubtasks(int i_parent_task_id) =>
            _taskRepo.GetSubtasks(i_parent_task_id);

        public DataTable GetSubtasksForDashboard(int parentTaskId) =>
            _taskRepo.GetSubtasksForDashboard(parentTaskId);

        /// <summary>ดึง Task ทั้งหมดของ Project แบบ Recursive Tree (ทุก depth)</summary>
        public DataTable GetAllTasksForProject(int projectId) =>
            _taskRepo.GetAllTasksForProject(projectId);

        /// <summary>สร้าง Task ใหม่ พร้อม auto-generate TaskCode</summary>
        public int CreateTask(ProjectTaskModel model)
        {
            // GenerateTaskCode() จะ throw exception ถ้าสร้าง code ไม่ได้
            model.c_task_code = _taskRepo.GenerateTaskCode();
            if (string.IsNullOrEmpty(model.c_task_code))
                throw new Exception("ไม่สามารถสร้าง Task Code ได้ กรุณาตรวจสอบ RunningNumber (running_type='TK')");

            int newId = _taskRepo.InsertTask(model);
            _auditService.Log(new ProjectAuditEntry
            {
                UserID = model.i_create_user,
                Module = "Project",
                Action = newId > 0 ? "CREATE_PROJECT_TASK" : "CREATE_PROJECT_TASK_FAILED",
                TableName = "t_project_task",
                RecordID = newId > 0 ? newId : (int?)null,
                Result = newId > 0 ? 1 : 2,
            });
            return newId;
        }

        public bool UpdateTask(ProjectTaskModel model) =>
            _taskRepo.UpdateTask(model);

        /// <summary>
        /// อัปเดต Status + % ของ Task
        /// ถ้า Status=Done → sync % ของ State ด้วย
        /// </summary>
        public bool UpdateStatus(int i_task_id, int i_proj_state_id, int i_project_id,
            string c_status, int i_pct_complete, string i_user, out string errorMessage)
        {
            errorMessage = "";
            bool ok = _taskRepo.UpdateStatus(i_task_id, c_status, i_pct_complete, i_user);
            if (!ok) { errorMessage = "บันทึกสถานะ Task ไม่สำเร็จ"; return false; }

            // Sync ความคืบหน้าของ State จาก avg Task pct
            SyncStatePct(i_proj_state_id, i_project_id, i_user);

            _auditService.Log(new ProjectAuditEntry
            {
                UserID = i_user,
                Module = "Project",
                Action = "UPDATE_PROJECT_TASK_STATUS",
                TableName = "t_project_task",
                RecordID = i_task_id,
                Result = 1,
            });
            return true;
        }

        /// <summary>คำนวณ % ของ State ใหม่จาก avg Task pct แล้ว insert progress record</summary>
        public void SyncStatePct(int i_proj_state_id, int i_project_id, string i_user)
        {
            int pct = _taskRepo.CalcStatePctFromTasks(i_proj_state_id);

            // insert progress record เพื่อ update State
            var prog = new ProjectProgressModel
            {
                i_proj_state_id = i_proj_state_id,
                i_project_id = i_project_id,
                i_pct_complete = pct,
                n_remark = "Auto-sync จาก Tasks",
                i_action_user = i_user
            };
            _progressSvc.UpdateStateProgress(prog, out _);
        }

        public bool SoftDelete(int i_task_id, string i_user) =>
            _taskRepo.SoftDelete(i_task_id, i_user);

        public bool Restore(int i_task_id, string i_user) =>
            _taskRepo.Restore(i_task_id, i_user);

        public bool HasActiveTasks(int i_proj_state_id) =>
            _taskRepo.HasActiveTasks(i_proj_state_id);

        // ─── Comments ─────────────────────────────────────────────────────────────

        public DataTable GetComments(int i_task_id) =>
            _taskRepo.GetComments(i_task_id);

        public int AddComment(ProjectTaskCommentModel model) =>
            _taskRepo.InsertComment(model);

        public bool DeleteComment(int i_comment_id) =>
            _taskRepo.SoftDeleteComment(i_comment_id);

        public bool EditComment(int i_comment_id, string content) =>
            _taskRepo.EditComment(i_comment_id, content);
    }
}
