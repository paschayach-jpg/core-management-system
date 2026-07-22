using RG_Project.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    /// <summary>
    /// DAL สำหรับ m_project_template — CRUD ประเภทโครงการ
    /// </summary>
    public class ProjectTemplateController
    {
        /// <summary>ดึงรายการ template ทั้งหมด พร้อมจำนวน state และโครงการ</summary>
        public DataTable GetAll()
        {
            var dt = new DataTable();
            var conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT
                    t.i_template_id,
                    t.c_template_code,
                    t.n_template_name,
                    t.c_description,
                    t.f_active,
                    ISNULL(t.f_is_legal_dict,'N') AS f_is_legal_dict,
                    t.d_create_date,
                    (SELECT COUNT(*) FROM m_template_state  s WHERE s.i_template_id = t.i_template_id) AS i_state_count,
                    (SELECT COUNT(*) FROM t_project          p WHERE p.i_template_id = t.i_template_id) AS i_project_count
                FROM m_project_template t
                ORDER BY t.n_template_name";
            using (var cmd = new SqlCommand(sql, conn.connection))
            using (var r = cmd.ExecuteReader())
                dt.Load(r);
            conn.Disconnect();
            return dt;
        }

        /// <summary>Insert template ใหม่ — คืนค่า i_template_id ใหม่</summary>
        public int Insert(string code, string name, string desc, string user, bool isLegalDict = false)
        {
            int newId = -1;
            var conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO m_project_template
                    (c_template_code, n_template_name, c_description, f_active, f_is_legal_dict, i_create_user, d_create_date)
                OUTPUT INSERTED.i_template_id
                VALUES (@code, @name, @desc, 'Y', @ldict, @user, GETDATE())";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@code",  string.IsNullOrWhiteSpace(code) ? (object)DBNull.Value : code.Trim());
                cmd.Parameters.AddWithValue("@name",  name.Trim());
                cmd.Parameters.AddWithValue("@desc",  string.IsNullOrWhiteSpace(desc) ? (object)DBNull.Value : desc.Trim());
                cmd.Parameters.AddWithValue("@ldict", isLegalDict ? "Y" : "N");
                cmd.Parameters.AddWithValue("@user",  user);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    newId = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return newId;
        }

        /// <summary>Update template</summary>
        public bool Update(int id, string code, string name, string desc, string active, string user, bool isLegalDict = false)
        {
            var conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                UPDATE m_project_template SET
                    c_template_code = @code,
                    n_template_name = @name,
                    c_description   = @desc,
                    f_active        = @active,
                    f_is_legal_dict = @ldict,
                    i_update_user   = @user,
                    d_update_date   = GETDATE()
                WHERE i_template_id = @id";
            int rows = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@code",   string.IsNullOrWhiteSpace(code) ? (object)DBNull.Value : code.Trim());
                cmd.Parameters.AddWithValue("@name",   name.Trim());
                cmd.Parameters.AddWithValue("@desc",   string.IsNullOrWhiteSpace(desc) ? (object)DBNull.Value : desc.Trim());
                cmd.Parameters.AddWithValue("@active", active ?? "Y");
                cmd.Parameters.AddWithValue("@ldict",  isLegalDict ? "Y" : "N");
                cmd.Parameters.AddWithValue("@user",   user);
                cmd.Parameters.AddWithValue("@id",     id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>ตรวจสอบว่า Template เป็น Legal Dict หรือไม่</summary>
        public bool IsLegalDict(int templateId)
        {
            var conn = new ConectionDB();
            conn.Connect('c');
            string val = "N";
            using (var cmd = new SqlCommand(
                "SELECT ISNULL(f_is_legal_dict,'N') FROM m_project_template WHERE i_template_id=@id",
                conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", templateId);
                var r = cmd.ExecuteScalar();
                if (r != null && r != DBNull.Value) val = r.ToString();
            }
            conn.Disconnect();
            return val == "Y";
        }

        /// <summary>คัดลอก state จาก source template ไปยัง target template</summary>
        public int CopyStatesFrom(int sourceTemplateId, int targetTemplateId, string user)
        {
            var conn = new ConectionDB();
            conn.Connect('c');

            // ลบ state เดิมของ target ก่อน (cascade doc ด้วย)
            using (var cmd = new SqlCommand(
                "DELETE FROM m_template_doc WHERE i_state_id IN (SELECT i_state_id FROM m_template_state WHERE i_template_id=@tid)",
                conn.connection))
            { cmd.Parameters.AddWithValue("@tid", targetTemplateId); cmd.ExecuteNonQuery(); }

            using (var cmd = new SqlCommand(
                "DELETE FROM m_template_state WHERE i_template_id=@tid", conn.connection))
            { cmd.Parameters.AddWithValue("@tid", targetTemplateId); cmd.ExecuteNonQuery(); }

            // Copy state จาก source
            string copySql = @"
                INSERT INTO m_template_state
                    (i_template_id, n_state_name, c_description, f_required, i_seq_no, i_create_user, d_create_date)
                SELECT @tid, n_state_name, c_description, f_required, i_seq_no, @user, GETDATE()
                FROM m_template_state
                WHERE i_template_id = @src
                ORDER BY i_seq_no";
            int rows = 0;
            using (var cmd = new SqlCommand(copySql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@tid",  targetTemplateId);
                cmd.Parameters.AddWithValue("@src",  sourceTemplateId);
                cmd.Parameters.AddWithValue("@user", user);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows;
        }

        /// <summary>ดึง template ที่ active ทั้งหมด สำหรับ picker</summary>
        public DataTable GetAllActive()
        {
            var dt = new DataTable();
            var conn = new ConectionDB();
            conn.Connect('c');
            using (var cmd = new SqlCommand(
                "SELECT i_template_id, n_template_name FROM m_project_template WHERE f_active='Y' ORDER BY n_template_name",
                conn.connection))
            using (var r = cmd.ExecuteReader())
                dt.Load(r);
            conn.Disconnect();
            return dt;
        }

        /// <summary>ลบ template — ตรวจสอบ dependency ก่อน</summary>
        public bool Delete(int id, out string error)
        {
            error = "";
            var conn = new ConectionDB();
            conn.Connect('c');

            // ตรวจว่ามีโครงการใช้งาน Template นี้อยู่หรือไม่
            int projCount = 0;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM t_project WHERE i_template_id = @id", conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                projCount = (int)cmd.ExecuteScalar();
            }
            if (projCount > 0)
            {
                conn.Disconnect();
                error = $"ไม่สามารถลบได้ มีโครงการที่ใช้งาน Template นี้อยู่ {projCount} รายการ";
                return false;
            }

            // ลบ cascade: m_template_doc → m_template_state → m_project_template
            using (var cmd = new SqlCommand("DELETE FROM m_template_doc   WHERE i_template_id = @id", conn.connection))
            { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }

            using (var cmd = new SqlCommand("DELETE FROM m_template_state WHERE i_template_id = @id", conn.connection))
            { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }

            int rows = 0;
            using (var cmd = new SqlCommand("DELETE FROM m_project_template WHERE i_template_id = @id", conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        // ════════════════════════════════════════════════════════════════════════
        // Template STATE (m_template_state) CRUD
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>ดึงขั้นตอนของ Template</summary>
        public DataTable GetStates(int templateId)
        {
            var dt = new DataTable();
            var conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT i_state_id, n_state_name, c_description, f_required,
                       i_seq_no,
                       (SELECT COUNT(*) FROM m_template_doc d WHERE d.i_state_id = s.i_state_id) AS i_doc_count
                FROM m_template_state s
                WHERE s.i_template_id = @id
                ORDER BY s.i_seq_no";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", templateId);
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>Insert ขั้นตอน — คืนค่า i_state_id ใหม่</summary>
        public int InsertState(int templateId, string name, string desc,
                               bool required, int seqNo, int? estimatedDays, string user)
        {
            int newId = -1;
            var conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO m_template_state
                    (i_template_id, n_state_name, c_description, f_required,
                     i_seq_no, i_create_user, d_create_date)
                OUTPUT INSERTED.i_state_id
                VALUES (@tid, @name, @desc, @req, @seq, @user, GETDATE())";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@tid",  templateId);
                cmd.Parameters.AddWithValue("@name", name.Trim());
                cmd.Parameters.AddWithValue("@desc", string.IsNullOrWhiteSpace(desc)
                    ? (object)DBNull.Value : desc.Trim());
                cmd.Parameters.AddWithValue("@req",  required ? "Y" : "N");
                cmd.Parameters.AddWithValue("@seq",  seqNo);
                cmd.Parameters.AddWithValue("@user", user);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    newId = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return newId;
        }

        /// <summary>Update ขั้นตอน</summary>
        public bool UpdateState(int stateId, string name, string desc,
                                bool required, int seqNo, int? estimatedDays, string user)
        {
            var conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                UPDATE m_template_state SET
                    n_state_name  = @name,
                    c_description = @desc,
                    f_required    = @req,
                    i_seq_no      = @seq,
                    i_update_user = @user,
                    d_update_date = GETDATE()
                WHERE i_state_id = @id";
            int rows = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@name", name.Trim());
                cmd.Parameters.AddWithValue("@desc", string.IsNullOrWhiteSpace(desc)
                    ? (object)DBNull.Value : desc.Trim());
                cmd.Parameters.AddWithValue("@req",  required ? "Y" : "N");
                cmd.Parameters.AddWithValue("@seq",  seqNo);
                cmd.Parameters.AddWithValue("@user", user);
                cmd.Parameters.AddWithValue("@id",   stateId);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>ลบขั้นตอน — ตรวจ dependency m_template_doc ก่อน</summary>
        public bool DeleteState(int stateId, out string error)
        {
            error = "";
            var conn = new ConectionDB();
            conn.Connect('c');
            int docCount = 0;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM m_template_doc WHERE i_state_id = @id", conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", stateId);
                docCount = (int)cmd.ExecuteScalar();
            }
            if (docCount > 0)
            {
                conn.Disconnect();
                error = $"ไม่สามารถลบได้ ขั้นตอนนี้มีเอกสาร {docCount} รายการผูกอยู่\nกรุณาลบเอกสารออกก่อน";
                return false;
            }
            int rows = 0;
            using (var cmd = new SqlCommand(
                "DELETE FROM m_template_state WHERE i_state_id = @id", conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", stateId);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>คำนวณ seq_no ถัดไปสำหรับ Template</summary>
        public int GetNextStateSeqNo(int templateId)
        {
            var conn = new ConectionDB();
            conn.Connect('c');
            int seq = 1;
            using (var cmd = new SqlCommand(
                "SELECT ISNULL(MAX(i_seq_no), 0) + 1 FROM m_template_state WHERE i_template_id = @id",
                conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", templateId);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    seq = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return seq;
        }

        // ════════════════════════════════════════════════════════════════════════
        // Template CODE duplicate check
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>ตรวจ code ซ้ำ</summary>
        public bool IsCodeDuplicate(string code, int excludeId = -1)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            var conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT COUNT(*)
                FROM m_project_template
                WHERE c_template_code = @code
                  AND (@excludeId = -1 OR i_template_id <> @excludeId)";
            int count = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@code",      code.Trim());
                cmd.Parameters.AddWithValue("@excludeId", excludeId);
                count = (int)cmd.ExecuteScalar();
            }
            conn.Disconnect();
            return count > 0;
        }
    }
}
