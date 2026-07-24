using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IProjectTaskRepository
    {
        string GenerateTaskCode();
        DataTable GetByState(int i_proj_state_id, bool includeDeleted = false);
        DataTable GetByProject(int i_project_id);
        DataTable GetSubtasks(int i_parent_task_id);
        DataTable GetSubtasksForDashboard(int parentTaskId);
        DataTable GetAllTasksForProject(int projectId);
        int InsertTask(ProjectTaskModel m);
        bool UpdateTask(ProjectTaskModel m);
        bool UpdateStatus(int i_task_id, string c_status, int i_pct_complete, string i_user);
        bool SoftDelete(int i_task_id, string i_user);
        bool Restore(int i_task_id, string i_user);
        int CalcStatePctFromTasks(int i_proj_state_id);
        bool HasActiveTasks(int i_proj_state_id);
        DataTable GetComments(int i_task_id);
        int InsertComment(ProjectTaskCommentModel m);
        bool SoftDeleteComment(int i_comment_id);
        bool EditComment(int i_comment_id, string n_content);
    }
}
