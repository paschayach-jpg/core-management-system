using System.Data;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IProjectTaskService
    {
        DataTable GetByState(int i_proj_state_id);
        DataTable GetByProject(int i_project_id);
        DataTable GetSubtasks(int i_parent_task_id);
        DataTable GetSubtasksForDashboard(int parentTaskId);
        DataTable GetAllTasksForProject(int projectId);
        int CreateTask(ProjectTaskModel model);
        bool UpdateTask(ProjectTaskModel model);
        bool UpdateStatus(int i_task_id, int i_proj_state_id, int i_project_id, string c_status, int i_pct_complete, string i_user, out string errorMessage);
        void SyncStatePct(int i_proj_state_id, int i_project_id, string i_user);
        bool SoftDelete(int i_task_id, string i_user);
        bool Restore(int i_task_id, string i_user);
        bool HasActiveTasks(int i_proj_state_id);
        DataTable GetComments(int i_task_id);
        int AddComment(ProjectTaskCommentModel model);
        bool DeleteComment(int i_comment_id);
        bool EditComment(int i_comment_id, string content);
    }
}
