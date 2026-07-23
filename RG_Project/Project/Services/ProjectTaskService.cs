using RG_Project.Project.Controllers;
using CMS.Domain.Models;
using System;
using System.Data;

namespace RG_Project.Project.Services
{
    public class ProjectTaskService
    {
        private readonly ProjectTaskController    _taskCtrl;
        private readonly ProjectStateController   _stateCtrl;
        private readonly ProjectProgressService   _progressSvc;

        public ProjectTaskService()
        {
            _taskCtrl    = new ProjectTaskController();
            _stateCtrl   = new ProjectStateController();
            _progressSvc = new ProjectProgressService();
        }

        // ─── Task CRUD ────────────────────────────────────────────────────────────

        public DataTable GetByState(int i_proj_state_id) =>
            _taskCtrl.GetByState(i_proj_state_id);

        public DataTable GetByProject(int i_project_id) =>
            _taskCtrl.GetByProject(i_project_id);

        public DataTable GetSubtasks(int i_parent_task_id) =>
            _taskCtrl.GetSubtasks(i_parent_task_id);

        public DataTable GetSubtasksForDashboard(int parentTaskId) =>
            _taskCtrl.GetSubtasksForDashboard(parentTaskId);

        /// <summary>ดึง Task ทั้งหมดของ Project แบบ Recursive Tree (ทุก depth)</summary>
        public DataTable GetAllTasksForProject(int projectId) =>
            _taskCtrl.GetAllTasksForProject(projectId);

        /// <summary>สร้าง Task ใหม่ พร้อม auto-generate TaskCode</summary>
        public int CreateTask(ProjectTaskModel model)
        {
            // GenerateTaskCode() จะ throw exception ถ้าสร้าง code ไม่ได้
            model.c_task_code = _taskCtrl.GenerateTaskCode();
            if (string.IsNullOrEmpty(model.c_task_code))
                throw new Exception("ไม่สามารถสร้าง Task Code ได้ กรุณาตรวจสอบ RunningNumber (running_type='TK')");
            return _taskCtrl.InsertTask(model);
        }

        public bool UpdateTask(ProjectTaskModel model) =>
            _taskCtrl.UpdateTask(model);

        /// <summary>
        /// อัปเดต Status + % ของ Task
        /// ถ้า Status=Done → sync % ของ State ด้วย
        /// </summary>
        public bool UpdateStatus(int i_task_id, int i_proj_state_id, int i_project_id,
            string c_status, int i_pct_complete, string i_user, out string errorMessage)
        {
            errorMessage = "";
            bool ok = _taskCtrl.UpdateStatus(i_task_id, c_status, i_pct_complete, i_user);
            if (!ok) { errorMessage = "บันทึกสถานะ Task ไม่สำเร็จ"; return false; }

            // Sync ความคืบหน้าของ State จาก avg Task pct
            SyncStatePct(i_proj_state_id, i_project_id, i_user);
            return true;
        }

        /// <summary>คำนวณ % ของ State ใหม่จาก avg Task pct แล้ว insert progress record</summary>
        public void SyncStatePct(int i_proj_state_id, int i_project_id, string i_user)
        {
            int pct = _taskCtrl.CalcStatePctFromTasks(i_proj_state_id);

            // insert progress record เพื่อ update State
            var prog = new ProjectProgressModel
            {
                i_proj_state_id = i_proj_state_id,
                i_project_id    = i_project_id,
                i_pct_complete  = pct,
                n_remark        = "Auto-sync จาก Tasks",
                i_action_user   = i_user
            };
            _progressSvc.UpdateStateProgress(prog, out _);
        }

        public bool SoftDelete(int i_task_id, string i_user) =>
            _taskCtrl.SoftDelete(i_task_id, i_user);

        public bool Restore(int i_task_id, string i_user) =>
            _taskCtrl.Restore(i_task_id, i_user);

        public bool HasActiveTasks(int i_proj_state_id) =>
            _taskCtrl.HasActiveTasks(i_proj_state_id);

        // ─── Comments ─────────────────────────────────────────────────────────────

        public DataTable GetComments(int i_task_id) =>
            _taskCtrl.GetComments(i_task_id);

        public int AddComment(ProjectTaskCommentModel model) =>
            _taskCtrl.InsertComment(model);

        public bool DeleteComment(int i_comment_id) =>
            _taskCtrl.SoftDeleteComment(i_comment_id);

        public bool EditComment(int i_comment_id, string content) =>
            _taskCtrl.EditComment(i_comment_id, content);
    }
}
