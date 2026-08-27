using System;

namespace CMS.Domain.Models
{
    /// <summary>Log การเปลี่ยนสถานะ Task — เก็บว่าใครเปลี่ยนจากสถานะไหนเป็นไหน และ % ณ ตอนที่เปลี่ยน</summary>
    public class ProjectTaskStatusLogModel
    {
        public int      i_log_id        { get; set; }
        public int      i_task_id       { get; set; }
        public string   c_old_status    { get; set; }
        public string   c_new_status    { get; set; }
        public int      i_pct_at_change { get; set; }
        public string   i_change_user   { get; set; }
        public DateTime d_change_date   { get; set; }
    }
}
