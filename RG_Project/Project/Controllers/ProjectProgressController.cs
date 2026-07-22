using RG_Project.Project.Models;
using RG_Project.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    public class ProjectProgressController
    {
        /// <summary>
        /// ดึง Progress history ของ State (เรียงจากใหม่ไปเก่า)
        /// </summary>
        public DataTable GetProgressHistory(int i_proj_state_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT
                    pr.i_progress_id,
                    pr.i_proj_state_id,
                    pr.i_project_id,
                    pr.c_status,
                    pr.n_remark,
                    pr.i_pct_complete,
                    pr.d_action_date,
                    pr.i_action_user,
                    ps.n_state_name
                FROM t_project_progress pr
                JOIN t_project_state ps ON ps.i_proj_state_id = pr.i_proj_state_id
                WHERE pr.i_proj_state_id = @i_proj_state_id
                ORDER BY pr.d_action_date DESC";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_proj_state_id", i_proj_state_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>
        /// ดึง latest progress 1 row ต่อ State สำหรับ Project (ใช้ใน Tab Progress)
        /// รวม ref project info (c_ref_project_no, i_ref_pct_overall) ด้วย
        /// </summary>
        public DataTable GetLatestProgressByProject(int i_project_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT
                    ps.i_proj_state_id,
                    ps.i_project_id,
                    ps.n_state_name,
                    ps.i_seq_no,
                    ps.f_required,
                    ps.c_status                          AS state_c_status,
                    ISNULL(pr.c_status, 'Pending')       AS c_status,
                    ISNULL(pr.n_remark, '')              AS n_remark,
                    ISNULL(pr.i_pct_complete, 0)         AS i_pct_complete,
                    pr.d_action_date,
                    pr.i_action_user,
                    pr.i_progress_id,
                    -- ref project
                    ps.i_ref_project_id,
                    rp.c_project_no                      AS c_ref_project_no,
                    rp.n_project_name                    AS n_ref_project_name,
                    ISNULL(ref_st.i_pct_overall, 0)      AS i_ref_pct_overall
                FROM t_project_state ps
                LEFT JOIN (
                    SELECT i_proj_state_id, MAX(i_progress_id) AS max_id
                    FROM t_project_progress
                    GROUP BY i_proj_state_id
                ) lp ON lp.i_proj_state_id = ps.i_proj_state_id
                LEFT JOIN t_project_progress pr ON pr.i_progress_id = lp.max_id
                -- ref project header
                LEFT JOIN t_project rp ON rp.i_project_id = ps.i_ref_project_id AND rp.f_active = 'Y'
                -- ref project overall %
                LEFT JOIN (
                    SELECT
                        rps.i_project_id,
                        CAST(AVG(CAST(ISNULL(rpr.i_pct_complete, 0) AS FLOAT)) AS INT) AS i_pct_overall
                    FROM t_project_state rps
                    LEFT JOIN (
                        SELECT i_proj_state_id, MAX(i_progress_id) AS max_id
                        FROM t_project_progress GROUP BY i_proj_state_id
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
        /// Insert progress record คืนค่า i_progress_id
        /// </summary>
        public int InsertProgress(ProjectProgressModel model)
        {
            int newId = -1;
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO t_project_progress
                    (i_proj_state_id, i_project_id, c_status, n_remark, i_pct_complete,
                     d_action_date, i_action_user)
                OUTPUT INSERTED.i_progress_id
                VALUES
                    (@i_proj_state_id, @i_project_id, @c_status, @n_remark, @i_pct_complete,
                     GETDATE(), @i_action_user)";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_proj_state_id", model.i_proj_state_id);
                cmd.Parameters.AddWithValue("@i_project_id",    model.i_project_id);
                cmd.Parameters.AddWithValue("@c_status",        model.c_status);
                cmd.Parameters.AddWithValue("@n_remark",
                    string.IsNullOrEmpty(model.n_remark) ? (object)DBNull.Value : model.n_remark);
                cmd.Parameters.AddWithValue("@i_pct_complete",  model.i_pct_complete);
                cmd.Parameters.AddWithValue("@i_action_user",   model.i_action_user);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    newId = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return newId;
        }

        /// <summary>
        /// คำนวณ overall progress % จาก latest pct ของทุก State
        /// State ที่มี ref project ให้ใช้ i_pct_overall ของ ref project แทน i_pct_complete ของตัวเอง
        /// </summary>
        public int CalculateOverallProgress(int i_project_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT ISNULL(CAST(AVG(CAST(effective_pct AS FLOAT)) AS INT), 0)
                FROM (
                    SELECT
                        ps.i_proj_state_id,
                        CASE
                            WHEN ps.i_ref_project_id IS NOT NULL
                                THEN ISNULL(ref_st.i_pct_overall, 0)
                            ELSE ISNULL(pr.i_pct_complete, 0)
                        END AS effective_pct
                    FROM t_project_state ps
                    LEFT JOIN (
                        SELECT i_proj_state_id, MAX(i_progress_id) AS max_id
                        FROM t_project_progress
                        GROUP BY i_proj_state_id
                    ) lp ON lp.i_proj_state_id = ps.i_proj_state_id
                    LEFT JOIN t_project_progress pr ON pr.i_progress_id = lp.max_id
                    LEFT JOIN (
                        SELECT
                            rps.i_project_id,
                            CAST(AVG(CAST(ISNULL(rpr.i_pct_complete, 0) AS FLOAT)) AS INT) AS i_pct_overall
                        FROM t_project_state rps
                        LEFT JOIN (
                            SELECT i_proj_state_id, MAX(i_progress_id) AS max_id
                            FROM t_project_progress GROUP BY i_proj_state_id
                        ) rlp ON rlp.i_proj_state_id = rps.i_proj_state_id
                        LEFT JOIN t_project_progress rpr ON rpr.i_progress_id = rlp.max_id
                        GROUP BY rps.i_project_id
                    ) ref_st ON ref_st.i_project_id = ps.i_ref_project_id
                    WHERE ps.i_project_id = @i_project_id
                ) x";
            int pct = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    pct = Convert.ToInt32(result);
            }
            conn.Disconnect();
            return pct;
        }

        /// <summary>
        /// ตรวจสอบว่า Project มี InProgress/Completed state อย่างน้อย 1 อัน
        /// </summary>
        public bool HasAnyProgress(int i_project_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT COUNT(*) FROM t_project_state
                WHERE i_project_id = @i_project_id
                  AND c_status IN ('InProgress', 'Completed')";
            int count = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                count = (int)cmd.ExecuteScalar();
            }
            conn.Disconnect();
            return count > 0;
        }
    }
}
