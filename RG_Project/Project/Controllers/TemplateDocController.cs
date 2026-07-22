using RG_Project.Project.Models;
using RG_Project.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    /// <summary>
    /// DAL สำหรับ m_template_doc — จัดการ mapping Template State ↔ Master Document
    /// </summary>
    public class TemplateDocController
    {
        // NOTE: m_document และ m_document_type อยู่ใน BUI_RG DB
        //       ถ้าอยู่ใน DB เดียวกับ Project tables ให้เปลี่ยน prefix เป็น dbo.
        private const string MDocRef     = "[BUI_RG].[dbo].[m_document]";
        private const string MDocTypeRef = "[BUI_RG].[dbo].[m_document_type]";

        /// <summary>
        /// ดึง Document Type ทั้งหมดจาก m_document_type สำหรับ ComboBox filter
        /// NOTE: ตรวจสอบชื่อ column (doc_type_id, doc_type_name) ให้ตรงกับ DB จริง
        /// </summary>
        public DataTable GetDocumentTypes()
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = $@"
                SELECT doc_type_id, doc_type_name
                FROM {MDocTypeRef}
                ORDER BY doc_type_name";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                dt.Load(reader);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>
        /// ดึง Document ทั้งหมดจาก m_document พร้อม doc_type_id สำหรับ filter
        /// </summary>
        public DataTable GetMasterDocuments()
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = $@"
                SELECT doc_id, doc_code, doc_name, doc_type_id,
                       CONCAT(doc_code, ' - ', doc_name) AS display_name
                FROM {MDocRef}
                ORDER BY doc_name";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                dt.Load(reader);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>
        /// ดึง template docs ทั้งหมดของ Template State นั้น
        /// </summary>
        public DataTable GetDocsByState(int i_state_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT
                    td.i_template_doc_id,
                    td.i_template_id,
                    td.i_state_id,
                    td.i_doc_id,
                    td.n_doc_name,
                    td.c_doc_type,
                    td.f_required,
                    td.i_seq_no,
                    td.n_remark,
                    td.i_create_user,
                    td.d_create_date,
                    td.i_update_user,
                    td.d_update_date,
                    ms.n_state_name,
                    pt.n_template_name
                FROM  m_template_doc    td
                JOIN  m_template_state  ms ON ms.i_state_id    = td.i_state_id
                JOIN  m_project_template pt ON pt.i_template_id = td.i_template_id
                WHERE td.i_state_id = @i_state_id
                ORDER BY td.i_seq_no";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_state_id", i_state_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>
        /// ดึง template docs ทั้งหมดของ Template (ทุก State)
        /// </summary>
        public DataTable GetDocsByTemplate(int i_template_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT
                    td.i_template_doc_id,
                    td.i_template_id,
                    td.i_state_id,
                    td.i_doc_id,
                    td.n_doc_name,
                    td.c_doc_type,
                    td.f_required,
                    td.i_seq_no,
                    td.n_remark,
                    ms.n_state_name,
                    ms.i_seq_no AS state_seq_no
                FROM  m_template_doc    td
                JOIN  m_template_state  ms ON ms.i_state_id    = td.i_state_id
                WHERE td.i_template_id = @i_template_id
                ORDER BY ms.i_seq_no, td.i_seq_no";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_template_id", i_template_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>
        /// Insert template doc mapping — คืนค่า i_template_doc_id ใหม่
        /// </summary>
        public int InsertTemplateDoc(TemplateDocModel model)
        {
            int newId = -1;
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO m_template_doc
                    (i_template_id, i_state_id, i_doc_id, n_doc_name, c_doc_type,
                     f_required, i_seq_no, n_remark, i_create_user, d_create_date)
                OUTPUT INSERTED.i_template_doc_id
                VALUES
                    (@i_template_id, @i_state_id, @i_doc_id, @n_doc_name, @c_doc_type,
                     @f_required, @i_seq_no, @n_remark, @i_create_user, GETDATE())";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_template_id", model.i_template_id);
                cmd.Parameters.AddWithValue("@i_state_id",    model.i_state_id);
                cmd.Parameters.AddWithValue("@i_doc_id",      model.i_doc_id);
                cmd.Parameters.AddWithValue("@n_doc_name",    model.n_doc_name);
                cmd.Parameters.AddWithValue("@c_doc_type",
                    string.IsNullOrEmpty(model.c_doc_type) ? (object)DBNull.Value : model.c_doc_type);
                cmd.Parameters.AddWithValue("@f_required",    model.f_required ?? "Y");
                cmd.Parameters.AddWithValue("@i_seq_no",      model.i_seq_no);
                cmd.Parameters.AddWithValue("@n_remark",
                    string.IsNullOrEmpty(model.n_remark) ? (object)DBNull.Value : model.n_remark);
                cmd.Parameters.AddWithValue("@i_create_user",
                    string.IsNullOrEmpty(model.i_create_user) ? (object)DBNull.Value : model.i_create_user);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    newId = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return newId;
        }

        /// <summary>
        /// Update template doc mapping
        /// </summary>
        public bool UpdateTemplateDoc(TemplateDocModel model)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                UPDATE m_template_doc SET
                    n_doc_name    = @n_doc_name,
                    c_doc_type    = @c_doc_type,
                    f_required    = @f_required,
                    i_seq_no      = @i_seq_no,
                    n_remark      = @n_remark,
                    i_update_user = @i_update_user,
                    d_update_date = GETDATE()
                WHERE i_template_doc_id = @i_template_doc_id";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@n_doc_name",        model.n_doc_name);
                cmd.Parameters.AddWithValue("@c_doc_type",
                    string.IsNullOrEmpty(model.c_doc_type) ? (object)DBNull.Value : model.c_doc_type);
                cmd.Parameters.AddWithValue("@f_required",        model.f_required ?? "Y");
                cmd.Parameters.AddWithValue("@i_seq_no",          model.i_seq_no);
                cmd.Parameters.AddWithValue("@n_remark",
                    string.IsNullOrEmpty(model.n_remark) ? (object)DBNull.Value : model.n_remark);
                cmd.Parameters.AddWithValue("@i_update_user",
                    string.IsNullOrEmpty(model.i_update_user) ? (object)DBNull.Value : model.i_update_user);
                cmd.Parameters.AddWithValue("@i_template_doc_id", model.i_template_doc_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>
        /// ลบ template doc mapping
        /// </summary>
        public bool DeleteTemplateDoc(int i_template_doc_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "DELETE FROM m_template_doc WHERE i_template_doc_id = @id";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", i_template_doc_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>
        /// ตรวจสอบว่า doc นี้ mapping กับ state นี้อยู่แล้วหรือไม่
        /// </summary>
        public bool IsDuplicate(int i_state_id, int i_doc_id, int excludeId = -1)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT COUNT(*)
                FROM m_template_doc
                WHERE i_state_id = @i_state_id
                  AND i_doc_id   = @i_doc_id
                  AND (@exclude_id = -1 OR i_template_doc_id <> @exclude_id)";
            int count = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_state_id",  i_state_id);
                cmd.Parameters.AddWithValue("@i_doc_id",    i_doc_id);
                cmd.Parameters.AddWithValue("@exclude_id",  excludeId);
                count = (int)cmd.ExecuteScalar();
            }
            conn.Disconnect();
            return count > 0;
        }

        /// <summary>
        /// คำนวณ i_seq_no ถัดไปสำหรับ State นั้น
        /// </summary>
        public int GetNextSeqNo(int i_state_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "SELECT ISNULL(MAX(i_seq_no), 0) + 1 FROM m_template_doc WHERE i_state_id = @id";
            int seq = 1;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", i_state_id);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    seq = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return seq;
        }
    }
}
