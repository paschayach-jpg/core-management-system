using System;

namespace CMS.Domain.Models
{
    public class ProjectStateModel
    {
        public int      i_proj_state_id { get; set; }
        public int      i_project_id    { get; set; }
        public int?     i_state_id      { get; set; }
        public string   n_state_name    { get; set; }
        public int      i_seq_no        { get; set; }
        public string   c_description   { get; set; }
        public string   f_required      { get; set; }
        public string   c_status        { get; set; }
        public string   i_create_user   { get; set; }
        public DateTime d_create_date   { get; set; }
        public string   i_update_user   { get; set; }
        public DateTime? d_update_date  { get; set; }

        // Joined fields from latest progress
        public int    i_pct_complete    { get; set; }
        public string n_latest_remark   { get; set; }

        // จำนวนวันที่คาดว่าจะทำงาน
        public int?   i_estimated_days  { get; set; }

        // โครงการที่ ref (1:1, เฉพาะโครงการที่ยังไม่ Completed)
        public int?   i_ref_project_id   { get; set; }
        public string n_ref_project_name { get; set; }  // joined for display
        public string c_ref_project_no   { get; set; }  // joined for display
        public int    i_ref_pct_overall  { get; set; }  // คำนวณ on-the-fly
    }
}
