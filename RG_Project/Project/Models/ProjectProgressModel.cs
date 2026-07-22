using System;

namespace RG_Project.Project.Models
{
    public class ProjectProgressModel
    {
        public int      i_progress_id   { get; set; }
        public int      i_proj_state_id { get; set; }
        public int      i_project_id    { get; set; }
        public string   c_status        { get; set; }
        public string   n_remark        { get; set; }
        public int      i_pct_complete  { get; set; }
        public DateTime d_action_date   { get; set; }
        public string   i_action_user   { get; set; }

        // Joined for display
        public string n_state_name { get; set; }
    }
}
