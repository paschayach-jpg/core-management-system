using System.Data;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectTaskService สำหรับ Presentation ของโมดูล Project (RGP)</summary>
    public sealed class ProjectTaskController
    {
        private readonly IProjectTaskService _taskService;

        public ProjectTaskController() : this(RgpComposition.TaskService) { }

        public ProjectTaskController(IProjectTaskService taskService)
        {
            _taskService = taskService;
        }

        public DataTable GetByState(int i_proj_state_id) => _taskService.GetByState(i_proj_state_id);

        public DataTable GetByProject(int i_project_id) => _taskService.GetByProject(i_project_id);

        public DataTable GetSubtasks(int i_parent_task_id) => _taskService.GetSubtasks(i_parent_task_id);

        public DataTable GetSubtasksForDashboard(int parentTaskId) => _taskService.GetSubtasksForDashboard(parentTaskId);

        public DataTable GetAllTasksForProject(int projectId) => _taskService.GetAllTasksForProject(projectId);

        public int CreateTask(ProjectTaskModel model) => _taskService.CreateTask(model);

        public bool UpdateTask(ProjectTaskModel model) => _taskService.UpdateTask(model);

        public bool UpdateStatus(int i_task_id, int i_proj_state_id, int i_project_id,
            string c_status, int i_pct_complete, string i_user, out string errorMessage) =>
            _taskService.UpdateStatus(i_task_id, i_proj_state_id, i_project_id, c_status, i_pct_complete, i_user, out errorMessage);

        public void SyncStatePct(int i_proj_state_id, int i_project_id, string i_user) =>
            _taskService.SyncStatePct(i_proj_state_id, i_project_id, i_user);

        public bool SoftDelete(int i_task_id, string i_user) => _taskService.SoftDelete(i_task_id, i_user);

        public bool Restore(int i_task_id, string i_user) => _taskService.Restore(i_task_id, i_user);

        public bool HasActiveTasks(int i_proj_state_id) => _taskService.HasActiveTasks(i_proj_state_id);

        public DataTable GetComments(int i_task_id) => _taskService.GetComments(i_task_id);

        public int AddComment(ProjectTaskCommentModel model) => _taskService.AddComment(model);

        public bool DeleteComment(int i_comment_id) => _taskService.DeleteComment(i_comment_id);

        public bool EditComment(int i_comment_id, string content) => _taskService.EditComment(i_comment_id, content);
    }
}
