namespace RG_Project.Project.Models
{
    public class ProjectStateMemberModel
    {
        public int    i_state_member_id { get; set; }
        public int    i_proj_state_id   { get; set; }
        public int    i_project_id      { get; set; }
        public string n_member_name     { get; set; }
        public string c_role            { get; set; }
        public string i_create_user     { get; set; }
    }
}
