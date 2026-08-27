namespace CMS.Domain.Models
{
    public class ProjectStateMemberModel
    {
        public int    i_state_member_id { get; set; }
        public int    i_proj_state_id   { get; set; }
        public int    i_project_id      { get; set; }
        public string n_member_name     { get; set; }
        public string c_role            { get; set; }
        public string c_member_type     { get; set; }
        public string i_create_user     { get; set; }
        public int    i_notify_count    { get; set; } // จำนวนครั้งที่ส่ง Notify — คำนวณจาก JOIN ไม่ใช่คอลัมน์จริงในตาราง
    }
}
