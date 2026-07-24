using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CMS.Data.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IConfigManager _configManager;

        public ProjectRepository(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        /// <summary>
        /// สร้าง Running Number จากตาราง RunningNumber WHERE running_type = 'PRJ'
        /// (ย้ายมาจาก ProjectService.GenerateProjectNo() เดิม — ตอนนี้ throw แทนการ swallow/MessageBox)
        /// </summary>
        public string GenerateProjectNo()
        {
            string projectNo;
            // RunningNumber ยังอยู่ที่ BUI_RG เดิม (ไม่ย้ายมา CMS_DB.RGP) — ต้องคง connection string เดิมไว้เฉพาะจุดนี้
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ProjectDbConnectionString")))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlSelect = @"
                            SELECT current_number, prefix, format, digit_number, Year_UW
                            FROM RunningNumber WITH (UPDLOCK, ROWLOCK)
                            WHERE running_type = 'PRJ'";

                        int currentNum = 0;
                        string prefix = "PRJ";
                        string fmt = "PPP-YY-######";
                        int digits = 6;
                        int yearUW = 0;

                        using (SqlCommand cmd = new SqlCommand(sqlSelect, conn, tx))
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new Exception("ไม่พบข้อมูล RunningNumber สำหรับ running_type = 'PRJ'");

                            currentNum = Convert.ToInt32(reader["current_number"]);
                            prefix = reader["prefix"].ToString();
                            fmt = reader["format"].ToString();
                            digits = Convert.ToInt32(reader["digit_number"]);
                            yearUW = Convert.ToInt32(reader["Year_UW"]);
                        }

                        int currentYear = DateTime.Now.Year;

                        if (yearUW != currentYear)
                            currentNum = 0;

                        int nextNum = currentNum + 1;

                        string sqlUpdate = @"
                            UPDATE RunningNumber
                            SET current_number = @next, Year_UW = @year
                            WHERE running_type = 'PRJ'";

                        using (SqlCommand cmd = new SqlCommand(sqlUpdate, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@next", nextNum);
                            cmd.Parameters.AddWithValue("@year", currentYear);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();

                        string yy = DateTime.Now.ToString("yy");
                        string yymm = DateTime.Now.ToString("yyMM");
                        string numStr = nextNum.ToString().PadLeft(digits, '0');

                        projectNo = fmt
                            .Replace("PPP", prefix)
                            .Replace("YYMM", yymm)
                            .Replace("YY", yy)
                            .Replace("######", numStr);
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        throw new Exception("GenerateProjectNo failed: " + ex.Message, ex);
                    }
                }
            }
            return projectNo;
        }

        /// <summary>
        /// ค้นหาโครงการตาม filter ต่างๆ พร้อม progress summary
        /// </summary>
        public DataTable SearchProjects(string c_project_no, string n_project_name,
            string c_status, string i_template_id, string d_from, string d_to)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT
                    p.i_project_id,
                    p.c_project_no,
                    p.n_project_name,
                    p.i_template_id,
                    ISNULL(t.n_template_name, N'-') AS n_template_name,
                    p.c_status,
                    p.c_description,
                    p.d_start_date,
                    p.d_due_date,
                    p.d_close_date,
                    p.f_active,
                    p.i_create_user,
                    p.d_create_date,
                    p.i_update_user,
                    p.d_update_date,
                    ISNULL(lpr.i_action_user, p.i_update_user) AS i_last_worker,
                    ISNULL(lpr.d_action_date, p.d_update_date) AS d_last_update,
                    ISNULL(st.i_total_states, 0)     AS i_total_states,
                    ISNULL(st.i_completed_states, 0) AS i_completed_states,
                    ISNULL(st.i_pct_overall, 0)      AS i_pct_overall
                FROM RGP.t_project p
                LEFT JOIN RGP.m_project_template t ON t.i_template_id = p.i_template_id
                LEFT JOIN (
                    SELECT
                        ps.i_project_id,
                        COUNT(*)                                        AS i_total_states,
                        SUM(CASE WHEN ps.c_status='Completed' THEN 1 ELSE 0 END) AS i_completed_states,
                        CAST(AVG(CAST(
                            CASE
                                WHEN ps.i_ref_project_id IS NOT NULL
                                    THEN ISNULL(ref_st.i_pct_overall, 0)
                                ELSE ISNULL(pr.i_pct_complete, 0)
                            END
                        AS FLOAT)) AS INT) AS i_pct_overall
                    FROM RGP.t_project_state ps
                    LEFT JOIN (
                        SELECT i_proj_state_id, MAX(i_progress_id) AS max_id
                        FROM RGP.t_project_progress
                        GROUP BY i_proj_state_id
                    ) lp ON lp.i_proj_state_id = ps.i_proj_state_id
                    LEFT JOIN RGP.t_project_progress pr ON pr.i_progress_id = lp.max_id
                    -- ref project overall %
                    LEFT JOIN (
                        SELECT
                            rps.i_project_id,
                            CAST(AVG(CAST(ISNULL(rpr.i_pct_complete, 0) AS FLOAT)) AS INT) AS i_pct_overall
                        FROM RGP.t_project_state rps
                        LEFT JOIN (
                            SELECT i_proj_state_id, MAX(i_progress_id) AS max_id
                            FROM RGP.t_project_progress GROUP BY i_proj_state_id
                        ) rlp ON rlp.i_proj_state_id = rps.i_proj_state_id
                        LEFT JOIN RGP.t_project_progress rpr ON rpr.i_progress_id = rlp.max_id
                        GROUP BY rps.i_project_id
                    ) ref_st ON ref_st.i_project_id = ps.i_ref_project_id
                    GROUP BY ps.i_project_id
                ) st ON st.i_project_id = p.i_project_id
                LEFT JOIN (
                    -- ดึง progress ล่าสุดของทั้ง project
                    SELECT pp.i_action_user, pp.d_action_date, ps2.i_project_id
                    FROM RGP.t_project_progress pp
                    INNER JOIN RGP.t_project_state ps2 ON ps2.i_proj_state_id = pp.i_proj_state_id
                    WHERE pp.i_progress_id = (
                        SELECT MAX(pp2.i_progress_id)
                        FROM RGP.t_project_progress pp2
                        INNER JOIN RGP.t_project_state ps3 ON ps3.i_proj_state_id = pp2.i_proj_state_id
                        WHERE ps3.i_project_id = ps2.i_project_id
                    )
                ) lpr ON lpr.i_project_id = p.i_project_id
                WHERE p.f_active = 'Y'
                  AND (@c_project_no   = '' OR p.c_project_no   LIKE '%' + @c_project_no   + '%')
                  AND (@n_project_name = '' OR p.n_project_name LIKE '%' + @n_project_name + '%')
                  AND (@c_status       = '' OR p.c_status       = @c_status)
                  AND (@i_template_id  = '' OR p.i_template_id  = @i_template_id_int)
                  AND (@d_from = '' OR CAST(p.d_create_date AS DATE) >= @d_from_date)
                  AND (@d_to   = '' OR CAST(p.d_create_date AS DATE) <= @d_to_date)
                ORDER BY p.d_create_date DESC";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@c_project_no",   c_project_no   ?? "");
                cmd.Parameters.AddWithValue("@n_project_name", n_project_name ?? "");
                cmd.Parameters.AddWithValue("@c_status",       c_status       ?? "");
                cmd.Parameters.AddWithValue("@i_template_id",  i_template_id  ?? "");
                cmd.Parameters.AddWithValue("@i_template_id_int",
                    string.IsNullOrEmpty(i_template_id) ? (object)DBNull.Value : int.Parse(i_template_id));
                cmd.Parameters.AddWithValue("@d_from", d_from ?? "");
                cmd.Parameters.AddWithValue("@d_to",   d_to   ?? "");
                cmd.Parameters.AddWithValue("@d_from_date",
                    string.IsNullOrEmpty(d_from) ? (object)DBNull.Value : DateTime.Parse(d_from));
                cmd.Parameters.AddWithValue("@d_to_date",
                    string.IsNullOrEmpty(d_to) ? (object)DBNull.Value : DateTime.Parse(d_to));
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            }
            return dt;
        }

        /// <summary>
        /// ดึงข้อมูล Project ตาม ID
        /// </summary>
        public DataTable GetProjectById(int i_project_id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT p.*, ISNULL(t.n_template_name, '-') AS n_template_name,
                       ISNULL(t.f_is_legal_dict,'N') AS f_is_legal_dict
                FROM RGP.t_project p
                LEFT JOIN RGP.m_project_template t ON t.i_template_id = p.i_template_id
                WHERE p.i_project_id = @i_project_id AND p.f_active = 'Y'";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            }
            return dt;
        }

        /// <summary>
        /// Insert Project ใหม่ คืนค่า i_project_id
        /// </summary>
        public int InsertProject(ProjectModel model)
        {
            int newId = -1;
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                INSERT INTO RGP.t_project
                    (c_project_no, n_project_name, i_template_id, c_status, c_description,
                     d_start_date, d_due_date, f_active, i_create_user, d_create_date)
                OUTPUT INSERTED.i_project_id
                VALUES
                    (@c_project_no, @n_project_name, @i_template_id, @c_status, @c_description,
                     @d_start_date, @d_due_date, 'Y', @i_create_user, GETDATE())";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@c_project_no",   model.c_project_no);
                cmd.Parameters.AddWithValue("@n_project_name", model.n_project_name);
                cmd.Parameters.AddWithValue("@i_template_id",
                    model.i_template_id.HasValue ? (object)model.i_template_id.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@c_status",      model.c_status ?? "Open");
                cmd.Parameters.AddWithValue("@c_description",
                    string.IsNullOrEmpty(model.c_description) ? (object)DBNull.Value : model.c_description);
                cmd.Parameters.AddWithValue("@d_start_date",
                    model.d_start_date.HasValue ? (object)model.d_start_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@d_due_date",
                    model.d_due_date.HasValue ? (object)model.d_due_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@i_create_user", model.i_create_user);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    newId = Convert.ToInt32(result);
            }
            }
            return newId;
        }

        /// <summary>
        /// Update Project header fields
        /// </summary>
        public bool UpdateProject(ProjectModel model)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                UPDATE RGP.t_project SET
                    n_project_name = @n_project_name,
                    i_template_id  = @i_template_id,
                    c_description  = @c_description,
                    d_start_date   = @d_start_date,
                    d_due_date     = @d_due_date,
                    i_update_user  = @i_update_user,
                    d_update_date  = GETDATE()
                WHERE i_project_id = @i_project_id AND f_active = 'Y'";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@n_project_name", model.n_project_name);
                cmd.Parameters.AddWithValue("@i_template_id",
                    model.i_template_id.HasValue ? (object)model.i_template_id.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@c_description",
                    string.IsNullOrEmpty(model.c_description) ? (object)DBNull.Value : model.c_description);
                cmd.Parameters.AddWithValue("@d_start_date",
                    model.d_start_date.HasValue ? (object)model.d_start_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@d_due_date",
                    model.d_due_date.HasValue ? (object)model.d_due_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@i_update_user", model.i_update_user);
                cmd.Parameters.AddWithValue("@i_project_id",  model.i_project_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        /// <summary>บันทึก JSON ข้อมูลพจนานุกรมกฎหมาย</summary>
        public bool UpdateLegalDictJson(int i_project_id, string json, string i_user)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                UPDATE RGP.t_project SET
                    n_legal_dict_json = @json,
                    i_update_user     = @user,
                    d_update_date     = GETDATE()
                WHERE i_project_id = @pid";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@json", string.IsNullOrEmpty(json) ? (object)DBNull.Value : json);
                cmd.Parameters.AddWithValue("@user", i_user);
                cmd.Parameters.AddWithValue("@pid",  i_project_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        /// <summary>
        /// Soft-delete Project (f_active='N', c_status='Cancelled')
        /// </summary>
        public bool DeleteProject(int i_project_id, string i_user)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                UPDATE RGP.t_project SET
                    f_active      = 'N',
                    c_status      = 'Cancelled',
                    i_update_user = @i_user,
                    d_update_date = GETDATE()
                WHERE i_project_id = @i_project_id";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                cmd.Parameters.AddWithValue("@i_user", i_user);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        /// <summary>
        /// ปิด Project: c_status='Completed', d_close_date=NOW()
        /// </summary>
        public bool CloseProject(int i_project_id, string i_user)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                UPDATE RGP.t_project SET
                    c_status      = 'Completed',
                    d_close_date  = GETDATE(),
                    i_update_user = @i_user,
                    d_update_date = GETDATE()
                WHERE i_project_id = @i_project_id AND f_active = 'Y'";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                cmd.Parameters.AddWithValue("@i_user", i_user);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        /// <summary>
        /// อัปเดต c_status ของ Project (ใช้จาก ProgressService)
        /// </summary>
        public bool UpdateProjectStatus(int i_project_id, string c_status, string i_user)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                UPDATE RGP.t_project SET
                    c_status      = @c_status,
                    d_close_date  = CASE WHEN @c_status='Completed' THEN GETDATE() ELSE d_close_date END,
                    i_update_user = @i_user,
                    d_update_date = GETDATE()
                WHERE i_project_id = @i_project_id AND f_active = 'Y'";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@c_status",      c_status);
                cmd.Parameters.AddWithValue("@i_user",        i_user);
                cmd.Parameters.AddWithValue("@i_project_id",  i_project_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        /// <summary>
        /// ตรวจสอบว่า Project มี State ที่ผ่าน Pending แล้วหรือไม่ (ใช้ก่อนลบ)
        /// </summary>
        public bool HasActiveProgress(int i_project_id)
        {
            
            int count = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT COUNT(*) FROM RGP.t_project_state
                WHERE i_project_id = @i_project_id AND c_status <> 'Pending'";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                count = (int)cmd.ExecuteScalar();
            }
            }
            return count > 0;
        }

        /// <summary>
        /// ตรวจสอบว่า Project No. มีในระบบแล้วหรือไม่ (f_active='Y')
        /// </summary>
        public bool IsProjectNoExists(string c_project_no)
        {
            
            int count = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = "SELECT COUNT(*) FROM RGP.t_project WHERE c_project_no = @c_project_no AND f_active = 'Y'";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@c_project_no", c_project_no);
                count = (int)cmd.ExecuteScalar();
            }
            }
            return count > 0;
        }

        /// <summary>
        /// ตรวจสอบว่า required states ครบ Completed แล้วหรือไม่ (ใช้ก่อน Close)
        /// </summary>
        public bool AllRequiredStatesCompleted(int i_project_id)
        {
            
            int count = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT COUNT(*) FROM RGP.t_project_state
                WHERE i_project_id = @i_project_id
                  AND f_required = 'Y'
                  AND c_status <> 'Completed'";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                count = (int)cmd.ExecuteScalar();
            }
            }
            return count == 0;
        }
    }
}
