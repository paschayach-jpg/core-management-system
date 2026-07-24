using System;

namespace CMS.Domain.Models
{
    public class ProjectDocumentWorkflowModel
    {
        public int      i_doc_wf_id     { get; set; }
        public int      i_proj_state_id { get; set; }
        public int      i_project_id    { get; set; }
        public string   n_doc_name      { get; set; }
        public string   c_doc_type      { get; set; }
        public string   f_required      { get; set; }
        public string   c_doc_status    { get; set; }
        public string   n_remark        { get; set; }
        public int      i_seq_no        { get; set; }
        public string   i_create_user   { get; set; }
        public DateTime d_create_date   { get; set; }
        public string   i_update_user   { get; set; }
        public DateTime? d_update_date  { get; set; }

        /// <summary>
        /// อ้างอิง doc_id จาก BUI_RG.dbo.m_document
        /// NULL = manual entry, NOT NULL = copy มาจาก template
        /// </summary>
        public int? i_doc_id { get; set; }

        // Joined for display
        public string n_state_name { get; set; }
    }
}
