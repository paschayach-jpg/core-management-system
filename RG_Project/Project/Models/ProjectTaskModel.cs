using System;

namespace RG_Project.Project.Models
{
    public class ProjectTaskModel
    {
        public int      i_task_id           { get; set; }
        public string   c_task_code         { get; set; }
        public int      i_proj_state_id     { get; set; }
        public int      i_project_id        { get; set; }
        public int?     i_parent_task_id    { get; set; }
        public int      i_subtask_depth     { get; set; }
        public string   n_title             { get; set; }
        public string   n_description       { get; set; }
        public int      i_priority          { get; set; } = 2;
        public string   c_status            { get; set; } = "Todo";
        public int      i_pct_complete      { get; set; }
        public string   n_assignee          { get; set; }
        public DateTime? d_due_date         { get; set; }
        public DateTime? d_start_date       { get; set; }
        public int?     i_estimated_hours   { get; set; }
        public int?     i_actual_hours      { get; set; }
        public string   n_extend_reason     { get; set; }
        public string   f_active            { get; set; } = "Y";
        public DateTime? d_deleted_date     { get; set; }
        public DateTime? d_recycle_expiry   { get; set; }
        public string   i_create_user       { get; set; }
        public DateTime d_create_date       { get; set; }
        public string   i_update_user       { get; set; }
        public DateTime? d_update_date      { get; set; }

        // ── Joined for display ──────────────────────────────────────────
        public string   n_state_name        { get; set; }
        public string   n_priority_label    { get; set; }  // Low/Medium/High/Critical
        public bool     b_sla_warning       { get; set; }  // เวลาผ่านไป >= 80%
        public bool     b_overdue           { get; set; }  // DueDate ผ่านแล้ว
        public int      i_subtask_count     { get; set; }
        public int      i_subtask_done      { get; set; }

        // ── Priority label helper ────────────────────────────────────────
        public static string PriorityLabel(int p)
        {
            switch (p)
            {
                case 1: return "Low";
                case 2: return "Medium";
                case 3: return "High";
                case 4: return "Critical";
                default: return "Medium";
            }
        }
    }

    public class ProjectTaskCommentModel
    {
        public int      i_comment_id        { get; set; }
        public int      i_task_id           { get; set; }
        public int?     i_parent_comment_id { get; set; }
        public int      i_thread_depth      { get; set; }
        public string   n_content           { get; set; }
        public string   f_is_edited         { get; set; } = "N";
        public string   f_is_deleted        { get; set; } = "N";
        public string   i_create_user       { get; set; }
        public DateTime d_create_date       { get; set; }
        public DateTime? d_edited_date      { get; set; }
    }
}
