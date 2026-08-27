using System;

namespace CMS.Domain.Models
{
    /// <summary>Log การส่ง Notify ให้บุคคลที่เกี่ยวข้องกับขั้นตอน — เก็บว่าส่งให้ใคร ตอนไหน ผูกกับ Task ใด</summary>
    public class ProjectNotifyLogModel
    {
        public int      i_notify_id       { get; set; }
        public int      i_project_id      { get; set; }
        public int      i_proj_state_id   { get; set; }
        public int      i_state_member_id { get; set; }
        public string   n_recipient_name  { get; set; }
        public int?     i_task_id         { get; set; }
        public string   f_task_created    { get; set; } // "Y" = สร้าง task ใหม่รอบนี้, "N" = ใช้ task เดิม
        public string   c_api_result      { get; set; }
        public string   n_message         { get; set; }
        public string   i_send_user       { get; set; }
        public DateTime d_send_date       { get; set; }
    }
}
