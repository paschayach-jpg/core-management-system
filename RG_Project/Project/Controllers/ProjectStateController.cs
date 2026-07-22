using RG_Project.Project.Models;
using RG_Project.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    public class ProjectStateController
    {
        /// <summary>
        /// ดึง States ทั้งหมดของ Project พร้อม latest progress และ ref project info
        /// </summary>
        public DataTable GetStatesByProject(int i_project_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT
                    ps.i_proj_state_id,
                    ps.i_project_id,
                    ps.i_state_id,
                    ps.n_state_name,
                    ps.i_seq_no,
                    ps.c_description,
                    ps.f_required,
                    ps.c_status,
                    ps.i_estimated_days,
                    ps.i_ref_project_id,
                    ps.i_create_user,
                    ps.d_create_date,
                    ps.i_update_user,
                    ps.d_update_date,
                    ISNULL(pr.i_pct_complete, 0)  AS i_pct_complete,
                    ISNULL(pr.n_remark, '')        AS n_latest_remark,
                    -- ref project info
                    rp.c_project_no               AS c_ref_project_no,
                    rp.n_project_name             AS n_ref_project_name,
                    ISNULL(ref_st.i_pct_overall, 0) AS i_ref_pct_overall
                FROM t_project_state ps
                LEFT JOIN (
                    SELECT i_proj_state_id, MAX(i_progress_id) AS max_id
                    FROM t_project_progress
                    GROUP BY i_proj_state_id
                ) lp ON lp.i_proj_state_id = ps.i_proj_state_id
                LEFT JOIN t_project_progress pr ON pr.i_progress_id = lp.max_id
                -- ref project
                LEFT JOIN t_project rp ON rp.i_project_id = ps.i_ref_project_id AND rp.f_active = 'Y'
                LEFT JOIN (
                    SELECT
                        rps.i_project_id,
                        CAST(AVG(CAST(ISNULL(rpr.i_pct_complete, 0) AS FLOAT)) AS INT) AS i_pct_overall
                    FROM t_project_state rps
                    LEFT JOIN (
                        SELECT i_proj_state_id, MAX(i_progress_id) AS max_id
                        FROM t_project_progress
                        GROUP BY i_proj_state_id
                    ) rlp ON rlp.i_proj_state_id = rps.i_proj_state_id
                    LEFT JOIN t_project_progress rpr ON rpr.i_progress_id = rlp.max_id
                    GROUP BY rps.i_project_id
                ) ref_st ON ref_st.i_project_id = ps.i_ref_project_id
                WHERE ps.i_project_id = @i_project_id
                ORDER BY ps.i_seq_no";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>
        /// ดึง States ของ Template (สำหรับ preview ก่อน copy)
        /// </summary>
        public DataTable GetTemplateStates(int i_template_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT i_state_id, i_template_id, n_state_name, i_seq_no,
                       c_description, f_required, i_create_user, d_create_date
                FROM m_template_state
                WHERE i_template_id = @i_template_id
                ORDER BY i_seq_no";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_template_id", i_template_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>
        /// Copy States จาก Template ไปยัง Project (Bulk INSERT)
        /// </summary>
        public bool InsertStatesFromTemplate(int i_project_id, int i_template_id, string i_user)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO t_project_state
                    (i_project_id, i_state_id, n_state_name, i_seq_no,
                     c_description, f_required, c_status, i_create_user, d_create_date)
                SELECT
                    @i_project_id, i_state_id, n_state_name, i_seq_no,
                    c_description, f_required, 'Pending', @i_user, GETDATE()
                FROM m_template_state
                WHERE i_template_id = @i_template_id
                ORDER BY i_seq_no";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_project_id",  i_project_id);
                cmd.Parameters.AddWithValue("@i_template_id", i_template_id);
                cmd.Parameters.AddWithValue("@i_user",        i_user);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>
        /// Insert State ใหม่ใน Project คืนค่า i_proj_state_id
        /// </summary>
        public int InsertState(ProjectStateModel model)
        {
            int newId = -1;
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO t_project_state
                    (i_project_id, i_state_id, n_state_name, i_seq_no,
                     c_description, f_required, c_status, i_estimated_days,
                     i_ref_project_id,
                     i_create_user, d_create_date)
                OUTPUT INSERTED.i_proj_state_id
                VALUES
                    (@i_project_id, @i_state_id, @n_state_name, @i_seq_no,
                     @c_description, @f_required, 'Pending', @i_estimated_days,
                     @i_ref_project_id,
                     @i_create_user, GETDATE())";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_project_id",  model.i_project_id);
                cmd.Parameters.AddWithValue("@i_state_id",
                    model.i_state_id.HasValue ? (object)model.i_state_id.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@n_state_name",  model.n_state_name);
                cmd.Parameters.AddWithValue("@i_seq_no",      model.i_seq_no);
                cmd.Parameters.AddWithValue("@c_description",
                    string.IsNullOrEmpty(model.c_description) ? (object)DBNull.Value : model.c_description);
                cmd.Parameters.AddWithValue("@f_required",    model.f_required ?? "Y");
                cmd.Parameters.AddWithValue("@i_estimated_days",
                    model.i_estimated_days.HasValue ? (object)model.i_estimated_days.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@i_ref_project_id",
                    model.i_ref_project_id.HasValue ? (object)model.i_ref_project_id.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@i_create_user", model.i_create_user);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    newId = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return newId;
        }

        /// <summary>
        /// Update State (name, description, seq_no, required, estimated_days, ref_project_id)
        /// </summary>
        public bool UpdateState(ProjectStateModel model)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                UPDATE t_project_state SET
                    n_state_name      = @n_state_name,
                    i_seq_no          = @i_seq_no,
                    c_description     = @c_description,
                    f_required        = @f_required,
                    i_estimated_days  = @i_estimated_days,
                    i_ref_project_id  = @i_ref_project_id,
                    i_update_user     = @i_update_user,
                    d_update_date     = GETDATE()
                WHERE i_proj_state_id = @i_proj_state_id";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@n_state_name",    model.n_state_name);
                cmd.Parameters.AddWithValue("@i_seq_no",        model.i_seq_no);
                cmd.Parameters.AddWithValue("@c_description",
                    string.IsNullOrEmpty(model.c_description) ? (object)DBNull.Value : model.c_description);
                cmd.Parameters.AddWithValue("@f_required",      model.f_required ?? "Y");
                cmd.Parameters.AddWithValue("@i_estimated_days",
                    model.i_estimated_days.HasValue ? (object)model.i_estimated_days.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@i_ref_project_id",
                    model.i_ref_project_id.HasValue ? (object)model.i_ref_project_id.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@i_update_user",   model.i_update_user);
                cmd.Parameters.AddWithValue("@i_proj_state_id", model.i_proj_state_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>
        /// ลบ State (blocked ถ้ามี progress records)
        /// </summary>
        public bool DeleteState(int i_proj_state_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "DELETE FROM t_project_state WHERE i_proj_state_id = @id";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", i_proj_state_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>
        /// Update c_status ของ State
        /// </summary>
        public bool UpdateStateStatus(int i_proj_state_id, string c_status, string i_user)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                UPDATE t_project_state SET
                    c_status      = @c_status,
                    i_update_user = @i_user,
                    d_update_date = GETDATE()
                WHERE i_proj_state_id = @i_proj_state_id";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@c_status",       c_status);
                cmd.Parameters.AddWithValue("@i_user",         i_user);
                cmd.Parameters.AddWithValue("@i_proj_state_id", i_proj_state_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>
        /// ตรวจสอบว่า State มี Progress records อยู่ (ใช้ก่อนลบ)
        /// </summary>
        public bool HasProgressRecords(int i_proj_state_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "SELECT COUNT(*) FROM t_project_progress WHERE i_proj_state_id = @id";
            int count = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", i_proj_state_id);
                count = (int)cmd.ExecuteScalar();
            }
            conn.Disconnect();
            return count > 0;
        }

        /// <summary>
        /// Reorder states: update i_seq_no สำหรับหลาย state พร้อมกัน
        /// </summary>
        public bool ReorderStates(List<Tuple<int, int>> orderList)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            bool success = true;
            using (SqlTransaction tx = conn.connection.BeginTransaction())
            {
                try
                {
                    foreach (var item in orderList)
                    {
                        string sql = "UPDATE t_project_state SET i_seq_no=@seq WHERE i_proj_state_id=@id";
                        using (SqlCommand cmd = new SqlCommand(sql, conn.connection, tx))
                        {
                            cmd.Parameters.AddWithValue("@seq", item.Item2);
                            cmd.Parameters.AddWithValue("@id",  item.Item1);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    success = false;
                }
            }
            conn.Disconnect();
            return success;
        }

        /// <summary>
        /// คำนวณ i_seq_no ถัดไปสำหรับ Project
        /// </summary>
        public int GetNextSeqNo(int i_project_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "SELECT ISNULL(MAX(i_seq_no), 0) + 1 FROM t_project_state WHERE i_project_id = @id";
            int seq = 1;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", i_project_id);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    seq = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return seq;
        }

        /// <summary>
        /// ผลรวม i_estimated_days ของ project ยกเว้น state ที่กำลัง edit (excludeStateId = 0 = Add mode)
        /// </summary>
        public int GetTotalEstimatedDays(int i_project_id, int excludeStateId = 0)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT ISNULL(SUM(i_estimated_days), 0)
                FROM t_project_state
                WHERE i_project_id = @pid
                  AND (@excludeId = 0 OR i_proj_state_id <> @excludeId)";
            int total = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@pid",       i_project_id);
                cmd.Parameters.AddWithValue("@excludeId", excludeStateId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    total = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return total;
        }

        /// <summary>
        /// ดึงโครงการที่ยังไม่ Completed สำหรับ ComboBox เลือก ref project
        /// ยกเว้น i_exclude_project_id (โครงการปัจจุบัน)
        /// </summary>
        public DataTable GetActiveProjectsForRef(int i_exclude_project_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT
                    p.i_project_id,
                    p.c_project_no,
                    p.n_project_name,
                    p.c_status,
                    ISNULL(st.i_pct_overall, 0) AS i_pct_overall
                FROM t_project p
                LEFT JOIN (
                    SELECT
                        ps.i_project_id,
                        CAST(AVG(CAST(ISNULL(pr.i_pct_complete, 0) AS FLOAT)) AS INT) AS i_pct_overall
                    FROM t_project_state ps
                    LEFT JOIN (
                        SELECT i_proj_state_id, MAX(i_progress_id) AS max_id
                        FROM t_project_progress GROUP BY i_proj_state_id
                    ) lp ON lp.i_proj_state_id = ps.i_proj_state_id
                    LEFT JOIN t_project_progress pr ON pr.i_progress_id = lp.max_id
                    GROUP BY ps.i_project_id
                ) st ON st.i_project_id = p.i_project_id
                WHERE p.f_active = 'Y'
                  AND p.c_status <> 'Completed'
                  AND p.i_project_id <> @i_exclude_project_id
                ORDER BY p.d_create_date DESC";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_exclude_project_id", i_exclude_project_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>
        /// ดึง States สำหรับ ComboBox (id + name)
        /// </summary>
        public DataTable GetStatesForCombo(int i_project_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT i_proj_state_id, n_state_name
                FROM t_project_state
                WHERE i_project_id = @i_project_id
                ORDER BY i_seq_no";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            conn.Disconnect();
            return dt;
        }
    }
}
