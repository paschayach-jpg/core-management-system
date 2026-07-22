using System;

namespace RG_Project.Project.Models
{
    public class ProjectTemplateModel
    {
        public int      i_template_id   { get; set; }
        public string   c_template_code { get; set; }
        public string   n_template_name { get; set; }
        public string   c_description   { get; set; }
        public string   f_active        { get; set; }
        public string   i_create_user   { get; set; }
        public DateTime d_create_date   { get; set; }
        public string   i_update_user   { get; set; }
        public DateTime? d_update_date  { get; set; }
    }
}
