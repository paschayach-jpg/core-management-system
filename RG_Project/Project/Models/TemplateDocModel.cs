using System;

namespace RG_Project.Project.Models
{
    /// <summary>
    /// Model สำหรับ m_template_doc — Mapping ระหว่าง Template State กับ Master Document
    /// </summary>
    public class TemplateDocModel
    {
        public int       i_template_doc_id { get; set; }
        public int       i_template_id     { get; set; }
        public int       i_state_id        { get; set; }

        /// <summary>doc_id จาก BUI_RG.dbo.m_document</summary>
        public int       i_doc_id          { get; set; }

        /// <summary>Snapshot ชื่อเอกสาร จาก m_document.doc_name ณ เวลาที่ mapping</summary>
        public string    n_doc_name        { get; set; }

        /// <summary>Snapshot รหัสเอกสาร จาก m_document.doc_code</summary>
        public string    c_doc_type        { get; set; }

        /// <summary>Y = บังคับ, N = ไม่บังคับ</summary>
        public string    f_required        { get; set; } = "Y";

        public int       i_seq_no          { get; set; } = 1;
        public string    n_remark          { get; set; }
        public string    i_create_user     { get; set; }
        public DateTime  d_create_date     { get; set; }
        public string    i_update_user     { get; set; }
        public DateTime? d_update_date     { get; set; }

        // ─── For display (joined) ───────────────────────────────────
        public string    n_state_name      { get; set; }
        public string    n_template_name   { get; set; }
    }
}
