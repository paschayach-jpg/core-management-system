using System;

namespace RG_Project.Project.Models
{
    public class EFilingRequestModel
    {
        public int      i_efiling_req_id { get; set; }
        public int      i_proj_state_id  { get; set; }
        public int      i_project_id     { get; set; }
        public string   c_efiling_ref_no { get; set; }
        public string   n_doc_name       { get; set; }
        public string   c_request_status { get; set; }
        public DateTime d_request_date   { get; set; }
        public DateTime? d_receive_date  { get; set; }
        public string   n_remark         { get; set; }
        public string   i_request_user   { get; set; }
        public string   i_update_user    { get; set; }
        public DateTime? d_update_date   { get; set; }

        // Joined for display
        public string n_state_name { get; set; }
    }
}
