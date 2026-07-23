using System;

namespace CMS.Domain.Models
{
    public class ProjectModel
    {
        public int      i_project_id    { get; set; }
        public string   c_project_no    { get; set; }
        public string   n_project_name  { get; set; }
        public int?     i_template_id   { get; set; }
        public string   n_template_name { get; set; }
        public string   c_status        { get; set; }
        public string   c_description   { get; set; }
        public DateTime? d_start_date   { get; set; }
        public DateTime? d_due_date     { get; set; }
        public DateTime? d_close_date   { get; set; }
        public string   f_active        { get; set; }
        public string   i_create_user   { get; set; }
        public DateTime d_create_date   { get; set; }
        public string   i_update_user   { get; set; }
        public DateTime? d_update_date  { get; set; }

        // Computed/joined fields for display
        public int i_total_states     { get; set; }
        public int i_completed_states { get; set; }
        public int i_pct_overall      { get; set; }
        public string   i_last_worker  { get; set; }  // ผู้ทำงานล่าสุด (จาก progress)
        public DateTime? d_last_update { get; set; }  // วันที่ทำงานล่าสุด
    }
}
