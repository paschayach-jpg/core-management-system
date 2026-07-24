using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CMS.Data.Repositories
{
    public class ProjectDocumentRepository : IProjectDocumentRepository
    {
        private readonly IConfigManager _configManager;

        public ProjectDocumentRepository(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        /// <summary>
        /// ดึง Document Workflow ของ State
        /// </summary>
        public DataTable GetDocsByState(int i_proj_state_id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT
                    dw.i_doc_wf_id,
                    dw.i_proj_state_id,
                    dw.i_project_id,
                    dw.n_doc_name,
                    dw.c_doc_type,
                    dw.f_required,
                    dw.c_doc_status,
                    dw.n_remark,
                    dw.i_seq_no,
                    dw.i_create_user,
                    dw.d_create_date,
                    dw.i_update_user,
                    dw.d_update_date,
                    ps.n_state_name
                FROM RGP.t_project_doc_workflow dw
                JOIN RGP.t_project_state ps ON ps.i_proj_state_id = dw.i_proj_state_id
                WHERE dw.i_proj_state_id = @i_proj_state_id
                ORDER BY dw.i_seq_no";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_proj_state_id", i_proj_state_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            }
            return dt;
        }

        /// <summary>
        /// ดึง Document Workflow ทั้งหมดของ Project (ทุก State)
        /// </summary>
        public DataTable GetDocsByProject(int i_project_id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT
                    dw.i_doc_wf_id,
                    dw.i_proj_state_id,
                    dw.i_project_id,
                    dw.n_doc_name,
                    dw.c_doc_type,
                    dw.f_required,
                    dw.c_doc_status,
                    dw.n_remark,
                    dw.i_seq_no,
                    dw.i_create_user,
                    dw.d_create_date,
                    dw.i_update_user,
                    dw.d_update_date,
                    ps.n_state_name,
                    ps.i_seq_no AS state_seq_no
                FROM RGP.t_project_doc_workflow dw
                JOIN RGP.t_project_state ps ON ps.i_proj_state_id = dw.i_proj_state_id
                WHERE dw.i_project_id = @i_project_id
                ORDER BY ps.i_seq_no, dw.i_seq_no";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            }
            return dt;
        }

        /// <summary>
        /// Insert Document Workflow item คืนค่า i_doc_wf_id
        /// i_doc_id ใน model จะถูก insert ด้วยถ้ามีค่า (NULL = manual entry)
        /// </summary>
        public int InsertDocWorkflow(ProjectDocumentWorkflowModel model)
        {
            int newId = -1;
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                INSERT INTO RGP.t_project_doc_workflow
                    (i_proj_state_id, i_project_id, i_doc_id, n_doc_name, c_doc_type,
                     f_required, c_doc_status, n_remark, i_seq_no, i_create_user, d_create_date)
                OUTPUT INSERTED.i_doc_wf_id
                VALUES
                    (@i_proj_state_id, @i_project_id, @i_doc_id, @n_doc_name, @c_doc_type,
                     @f_required, 'Pending', @n_remark, @i_seq_no, @i_create_user, GETDATE())";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_proj_state_id", model.i_proj_state_id);
                cmd.Parameters.AddWithValue("@i_project_id",    model.i_project_id);
                cmd.Parameters.AddWithValue("@i_doc_id",
                    model.i_doc_id.HasValue ? (object)model.i_doc_id.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@n_doc_name",      model.n_doc_name);
                cmd.Parameters.AddWithValue("@c_doc_type",
                    string.IsNullOrEmpty(model.c_doc_type) ? (object)DBNull.Value : model.c_doc_type);
                cmd.Parameters.AddWithValue("@f_required",      model.f_required ?? "Y");
                cmd.Parameters.AddWithValue("@n_remark",
                    string.IsNullOrEmpty(model.n_remark) ? (object)DBNull.Value : model.n_remark);
                cmd.Parameters.AddWithValue("@i_seq_no",        model.i_seq_no);
                cmd.Parameters.AddWithValue("@i_create_user",   model.i_create_user);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    newId = Convert.ToInt32(result);
            }
            }
            return newId;
        }

        /// <summary>
        /// Bulk copy Document Workflow จาก RGP.m_template_doc ไปยัง RGP.t_project_doc_workflow
        /// เรียกใช้หลัง InsertStatesFromTemplate() เพื่อ auto-populate เอกสารตาม template
        /// </summary>
        public bool InsertDocWorkflowFromTemplate(int i_project_id, string i_user)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            // JOIN RGP.t_project_state (เพิ่งสร้างใหม่) กับ RGP.m_template_doc โดย i_state_id
            // n_doc_name และ c_doc_type เป็น snapshot ที่เก็บไว้ใน RGP.m_template_doc แล้ว
            string sql = @"
                INSERT INTO RGP.t_project_doc_workflow
                    (i_proj_state_id, i_project_id, i_doc_id, n_doc_name,
                     c_doc_type, f_required, c_doc_status, i_seq_no,
                     i_create_user, d_create_date)
                SELECT
                    ps.i_proj_state_id,
                    ps.i_project_id,
                    td.i_doc_id,
                    td.n_doc_name,
                    td.c_doc_type,
                    td.f_required,
                    'Pending',
                    td.i_seq_no,
                    @i_user,
                    GETDATE()
                FROM  RGP.t_project_state ps
                JOIN  RGP.m_template_doc  td ON td.i_state_id = ps.i_state_id
                WHERE ps.i_project_id  = @i_project_id
                  AND ps.i_state_id    IS NOT NULL
                ORDER BY ps.i_seq_no, td.i_seq_no";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                cmd.Parameters.AddWithValue("@i_user",       i_user);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows >= 0;   // 0 rows is OK (template may have no docs yet)
        }

        /// <summary>
        /// Update Document Workflow item
        /// </summary>
        public bool UpdateDocWorkflow(ProjectDocumentWorkflowModel model)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                UPDATE RGP.t_project_doc_workflow SET
                    i_proj_state_id = @i_proj_state_id,
                    n_doc_name      = @n_doc_name,
                    c_doc_type      = @c_doc_type,
                    f_required      = @f_required,
                    c_doc_status    = @c_doc_status,
                    n_remark        = @n_remark,
                    i_seq_no        = @i_seq_no,
                    i_update_user   = @i_update_user,
                    d_update_date   = GETDATE()
                WHERE i_doc_wf_id = @i_doc_wf_id";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_proj_state_id", model.i_proj_state_id);
                cmd.Parameters.AddWithValue("@n_doc_name",      model.n_doc_name);
                cmd.Parameters.AddWithValue("@c_doc_type",
                    string.IsNullOrEmpty(model.c_doc_type) ? (object)DBNull.Value : model.c_doc_type);
                cmd.Parameters.AddWithValue("@f_required",      model.f_required ?? "Y");
                cmd.Parameters.AddWithValue("@c_doc_status",    model.c_doc_status ?? "Pending");
                cmd.Parameters.AddWithValue("@n_remark",
                    string.IsNullOrEmpty(model.n_remark) ? (object)DBNull.Value : model.n_remark);
                cmd.Parameters.AddWithValue("@i_seq_no",        model.i_seq_no);
                cmd.Parameters.AddWithValue("@i_update_user",   model.i_update_user);
                cmd.Parameters.AddWithValue("@i_doc_wf_id",     model.i_doc_wf_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        /// <summary>
        /// ลบ Document Workflow item
        /// </summary>
        public bool DeleteDocWorkflow(int i_doc_wf_id)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = "DELETE FROM RGP.t_project_doc_workflow WHERE i_doc_wf_id = @id";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", i_doc_wf_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        /// <summary>
        /// Update เฉพาะ c_doc_status
        /// </summary>
        public bool UpdateDocStatus(int i_doc_wf_id, string c_doc_status, string i_user)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                UPDATE RGP.t_project_doc_workflow SET
                    c_doc_status  = @c_doc_status,
                    i_update_user = @i_user,
                    d_update_date = GETDATE()
                WHERE i_doc_wf_id = @id";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@c_doc_status", c_doc_status);
                cmd.Parameters.AddWithValue("@i_user",       i_user);
                cmd.Parameters.AddWithValue("@id",           i_doc_wf_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        /// <summary>
        /// คำนวณ i_seq_no ถัดไปสำหรับ State
        /// </summary>
        public int GetNextSeqNo(int i_proj_state_id)
        {
            
            int seq = 1;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = "SELECT ISNULL(MAX(i_seq_no), 0) + 1 FROM RGP.t_project_doc_workflow WHERE i_proj_state_id = @id";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", i_proj_state_id);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    seq = Convert.ToInt32(result);
            }
            }
            return seq;
        }
    }
}
