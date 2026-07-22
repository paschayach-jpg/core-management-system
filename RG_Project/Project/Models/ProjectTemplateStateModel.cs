using System;

namespace RG_Project.Project.Models
{
    public class ProjectTemplateStateModel
    {
        public int      i_state_id    { get; set; }
        public int      i_template_id { get; set; }
        public string   n_state_name  { get; set; }
        public int      i_seq_no      { get; set; }
        public string   c_description { get; set; }
        public string   f_required    { get; set; }
        public string   i_create_user { get; set; }
        public DateTime d_create_date { get; set; }
    }
}
