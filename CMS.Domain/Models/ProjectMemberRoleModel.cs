using System;

namespace CMS.Domain.Models
{
    public class ProjectMemberRoleModel
    {
        public int      i_role_id     { get; set; }
        public string   n_role_name   { get; set; }
        public string   f_active      { get; set; }
        public string   i_create_user { get; set; }
        public DateTime? d_create_date { get; set; }
        public string   i_update_user { get; set; }
        public DateTime? d_update_date { get; set; }
    }
}
