using CMS.Domain.Models;
using RG_Project.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    public class ProjectTaskController
    {
        // ─── Task Running Number ──────────────────────────────────────────────────
        public string GenerateTaskCode()
        {
            string code = "";
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            using (SqlTransaction tx = conn.connection.BeginTransaction())
            {
                try
                {
                    int thisYear = DateTime.Now.Year;

                    // 1. Lock + read row
                    string sel = @"SELECT current_number, prefix, format, digit_number, ISNULL(Year_UW,0) AS Year_UW
                                   FROM RunningNumber WITH (UPDLOCK, ROWLOCK)
                                   WHERE running_type = 'TK'";
                    int cur = 0; string pfx = "TK", fmt = "PPP-YY-######"; int dig = 6, yr = 0;
                    bool rowExists = false;
                    using (var cmd = new SqlCommand(sel, conn.connection, tx))
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            rowExists = true;
                            cur = Convert.ToInt32(r["current_number"]);
                            pfx = r["prefix"].ToString();
                            fmt = r["format"].ToString();
                            dig = Convert.ToInt32(r["digit_number"]);
                            yr  = Convert.ToInt32(r["Year_UW"]);
                        }
                    }

                    // 2. Reset เมื่อขึ้นปีใหม่
                    if (yr != thisYear) cur = 0;
                    int next = cur + 1;

                    if (rowExists)
                    {
                        using (var cmd = new SqlCommand(
                            "UPDATE RunningNumber SET current_number=@n, Year_UW=@y WHERE running_type='TK'",
                            conn.connection, tx))
                        {
                            cmd.Parameters.AddWithValue("@n", next);
                            cmd.Parameters.AddWithValue("@y", thisYear);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // ยังไม่มี row → INSERT ใหม่
                        // ดึง max_length ของคอลัมน์ product จาก sys.columns
                        int prodMaxLen = 20;
                        using (var cmdLen = new SqlCommand(
                            @"SELECT max_length FROM sys.columns
                              WHERE object_id = OBJECT_ID('RunningNumber') AND name = 'product'",
                            conn.connection, tx))
                        {
                            object lenVal = cmdLen.ExecuteScalar();
                            if (lenVal != null && lenVal != DBNull.Value)
                                prodMaxLen = Convert.ToInt32(lenVal);
                        }

                        // ดึง product จาก row อื่น ตัดให้ไม่เกิน max_length
                        string product = "";
                        using (var cmdProd = new SqlCommand(
                            "SELECT TOP 1 LEFT(ISNULL(product,''), @len) FROM RunningNumber",
                            conn.connection, tx))
                        {
                            cmdProd.Parameters.AddWithValue("@len", prodMaxLen);
                            object val = cmdProd.ExecuteScalar();
                            if (val != null && val != DBNull.Value) product = val.ToString();
                        }

                        using (var cmd = new SqlCommand(
                            @"INSERT INTO RunningNumber
                                  (running_type, prefix, format, digit_number, current_number, Year_UW, product)
                              VALUES ('TK','TK','PPP-YY-######',6,@n,@y,@prod)",
                            conn.connection, tx))
                        {
                            cmd.Parameters.AddWithValue("@n",    next);
                            cmd.Parameters.AddWithValue("@y",    thisYear);
                            cmd.Parameters.AddWithValue("@prod", product);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    string yy  = DateTime.Now.ToString("yy");
                    string num = next.ToString().PadLeft(dig, '0');
                    code = fmt.Replace("PPP", pfx).Replace("YY", yy).Replace("######", num);
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new Exception("GenerateTaskCode failed: " + ex.Message, ex);
                }
            }
            conn.Disconnect();
            return code;
        }

        // ─── CRUD ─────────────────────────────────────────────────────────────────

        /// <summary>ดึง Tasks ของ State พร้อม SLA flag</summary>
        public DataTable GetByState(int i_proj_state_id, bool includeDeleted = false)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT
                    t.i_task_id, t.c_task_code, t.i_proj_state_id, t.i_project_id,
                    t.i_parent_task_id, t.i_subtask_depth,
                    t.n_title, t.n_description,
                    t.i_priority, t.c_status, t.i_pct_complete,
                    t.n_assignee, t.d_due_date, t.d_start_date,
                    t.i_estimated_hours, t.i_actual_hours,
                    t.f_active, t.d_deleted_date, t.d_recycle_expiry,
                    t.i_create_user, t.d_create_date, t.i_update_user, t.d_update_date,
                    ps.n_state_name,
                    -- SLA: เวลาผ่านไป >= 80% ของ (due - create)
                    CASE WHEN t.d_due_date IS NOT NULL AND t.c_status NOT IN ('Done','Cancelled')
                              AND DATEDIFF(DAY, t.d_create_date, GETDATE()) * 1.0
                                / NULLIF(DATEDIFF(DAY, t.d_create_date, t.d_due_date), 0) >= 0.8
                         THEN 1 ELSE 0 END AS b_sla_warning,
                    CASE WHEN t.d_due_date IS NOT NULL AND t.c_status NOT IN ('Done','Cancelled')
                              AND t.d_due_date < CAST(GETDATE() AS DATE)
                         THEN 1 ELSE 0 END AS b_overdue,
                    -- Subtask summary
                    ISNULL(sub.i_subtask_count, 0) AS i_subtask_count,
                    ISNULL(sub.i_subtask_done,  0) AS i_subtask_done
                FROM t_project_task t
                JOIN t_project_state ps ON ps.i_proj_state_id = t.i_proj_state_id
                LEFT JOIN (
                    SELECT i_parent_task_id,
                           COUNT(*)                                          AS i_subtask_count,
                           SUM(CASE WHEN c_status='Done' THEN 1 ELSE 0 END) AS i_subtask_done
                    FROM t_project_task
                    WHERE i_parent_task_id IS NOT NULL AND f_active='Y'
                    GROUP BY i_parent_task_id
                ) sub ON sub.i_parent_task_id = t.i_task_id
                WHERE t.i_proj_state_id = @sid
                  AND t.i_parent_task_id IS NULL  -- root tasks only (subtasks load separately)
                  AND (@incDel = 1 OR t.f_active = 'Y')
                ORDER BY t.i_priority DESC, t.d_due_date, t.d_create_date";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@sid",    i_proj_state_id);
                cmd.Parameters.AddWithValue("@incDel", includeDeleted ? 1 : 0);
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>ดึง Tasks ทั้งหมดของ Project (ใช้ใน Kanban + Dashboard)</summary>
        public DataTable GetByProject(int i_project_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT
                    t.i_task_id, t.c_task_code, t.i_proj_state_id, t.i_project_id,
                    t.i_parent_task_id, t.i_subtask_depth,
                    t.n_title, t.n_description,
                    t.i_priority, t.c_status, t.i_pct_complete,
                    t.n_assignee, t.d_due_date, t.d_start_date,
                    t.f_active, t.d_create_date, t.i_update_user, t.d_update_date,
                    ps.n_state_name,
                    CASE WHEN t.d_due_date IS NOT NULL AND t.c_status NOT IN ('Done','Cancelled')
                              AND DATEDIFF(DAY, t.d_create_date, GETDATE()) * 1.0
                                / NULLIF(DATEDIFF(DAY, t.d_create_date, t.d_due_date), 0) >= 0.8
                         THEN 1 ELSE 0 END AS b_sla_warning,
                    CASE WHEN t.d_due_date IS NOT NULL AND t.c_status NOT IN ('Done','Cancelled')
                              AND t.d_due_date < CAST(GETDATE() AS DATE)
                         THEN 1 ELSE 0 END AS b_overdue,
                    ISNULL(sub.i_subtask_count, 0) AS i_subtask_count,
                    ISNULL(sub.i_subtask_done,  0) AS i_subtask_done
                FROM t_project_task t
                JOIN t_project_state ps ON ps.i_proj_state_id = t.i_proj_state_id
                LEFT JOIN (
                    SELECT i_parent_task_id,
                           COUNT(*) AS i_subtask_count,
                           SUM(CASE WHEN c_status='Done' THEN 1 ELSE 0 END) AS i_subtask_done
                    FROM t_project_task
                    WHERE i_parent_task_id IS NOT NULL AND f_active='Y'
                    GROUP BY i_parent_task_id
                ) sub ON sub.i_parent_task_id = t.i_task_id
                WHERE t.i_project_id = @pid AND t.f_active = 'Y'
                  AND t.i_parent_task_id IS NULL
                ORDER BY t.i_proj_state_id, t.i_priority DESC, t.d_due_date";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@pid", i_project_id);
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>ดึง Subtasks ของ Task</summary>
        public DataTable GetSubtasks(int i_parent_task_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT t.*, ps.n_state_name,
                    CASE WHEN t.d_due_date < CAST(GETDATE() AS DATE)
                              AND t.c_status NOT IN ('Done','Cancelled')
                         THEN 1 ELSE 0 END AS b_overdue
                FROM t_project_task t
                JOIN t_project_state ps ON ps.i_proj_state_id = t.i_proj_state_id
                WHERE t.i_parent_task_id = @pid AND t.f_active = 'Y'
                ORDER BY t.i_priority DESC, t.d_due_date";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@pid", i_parent_task_id);
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>ดึง Subtasks สำหรับ Dashboard (schema เดียวกับ GetByProject)</summary>
        public DataTable GetSubtasksForDashboard(int parentTaskId)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
        SELECT
            t.i_task_id, t.c_task_code, t.i_proj_state_id, t.i_project_id,
            t.i_parent_task_id, t.i_subtask_depth,
            t.n_title, t.n_description,
            t.i_priority, t.c_status, t.i_pct_complete,
            t.n_assignee, t.d_due_date, t.d_start_date,
            t.f_active, t.d_create_date, t.i_update_user, t.d_update_date,
            ps.n_state_name,
            CASE WHEN t.d_due_date IS NOT NULL AND t.c_status NOT IN ('Done','Cancelled')
                      AND DATEDIFF(DAY, t.d_create_date, GETDATE()) * 1.0
                        / NULLIF(DATEDIFF(DAY, t.d_create_date, t.d_due_date), 0) >= 0.8
                 THEN 1 ELSE 0 END AS b_sla_warning,
            CASE WHEN t.d_due_date IS NOT NULL AND t.c_status NOT IN ('Done','Cancelled')
                      AND t.d_due_date < CAST(GETDATE() AS DATE)
                 THEN 1 ELSE 0 END AS b_overdue,
            0 AS i_subtask_count,
            0 AS i_subtask_done
        FROM t_project_task t
        JOIN t_project_state ps ON ps.i_proj_state_id = t.i_proj_state_id
        WHERE t.i_parent_task_id = @pid AND t.f_active = 'Y'
        ORDER BY t.i_priority DESC, t.d_due_date, t.d_create_date";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@pid", parentTaskId);
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>ดึง Task ทั้งหมดของ Project แบบ Recursive Tree พร้อม tree_depth และ sort_path</summary>
        public DataTable GetAllTasksForProject(int projectId)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                ;WITH TaskTree AS (
                    SELECT t.i_task_id, t.c_task_code, t.i_proj_state_id, t.i_project_id,
                           t.i_parent_task_id, t.i_subtask_depth,
                           t.n_title, t.n_description, t.i_priority, t.c_status, t.i_pct_complete,
                           t.n_assignee, t.d_due_date, t.d_start_date, t.f_active,
                           t.d_create_date, t.i_update_user, t.d_update_date, t.i_create_user,
                           ps.n_state_name,
                           0 AS tree_depth,
                           CAST(RIGHT(REPLICATE('0',10)+CAST(t.i_task_id AS VARCHAR(10)),10)
                                AS NVARCHAR(MAX)) AS sort_path
                    FROM t_project_task t
                    JOIN t_project_state ps ON ps.i_proj_state_id = t.i_proj_state_id
                    WHERE t.i_project_id = @pid
                      AND t.i_parent_task_id IS NULL AND t.f_active = 'Y'
                    UNION ALL
                    SELECT t.i_task_id, t.c_task_code, t.i_proj_state_id, t.i_project_id,
                           t.i_parent_task_id, t.i_subtask_depth,
                           t.n_title, t.n_description, t.i_priority, t.c_status, t.i_pct_complete,
                           t.n_assignee, t.d_due_date, t.d_start_date, t.f_active,
                           t.d_create_date, t.i_update_user, t.d_update_date, t.i_create_user,
                           ps.n_state_name,
                           tt.tree_depth + 1,
                           CAST(tt.sort_path+'/'+RIGHT(REPLICATE('0',10)+CAST(t.i_task_id AS VARCHAR(10)),10)
                                AS NVARCHAR(MAX))
                    FROM t_project_task t
                    INNER JOIN TaskTree tt ON t.i_parent_task_id = tt.i_task_id
                    JOIN t_project_state ps ON ps.i_proj_state_id = t.i_proj_state_id
                    WHERE t.f_active = 'Y'
                ),
                ChildCnt AS (
                    SELECT i_parent_task_id,
                           COUNT(*) AS i_subtask_count,
                           SUM(CASE WHEN c_status='Done' THEN 1 ELSE 0 END) AS i_subtask_done
                    FROM t_project_task
                    WHERE i_parent_task_id IS NOT NULL AND f_active='Y'
                    GROUP BY i_parent_task_id
                )
                SELECT tt.i_task_id, tt.c_task_code, tt.i_proj_state_id, tt.i_project_id,
                       tt.i_parent_task_id, tt.n_title, tt.n_description,
                       tt.i_priority, tt.c_status, tt.i_pct_complete,
                       tt.n_assignee, tt.d_due_date, tt.d_start_date,
                       tt.d_create_date, tt.i_update_user, tt.d_update_date,
                       tt.n_state_name, tt.tree_depth,
                       CASE WHEN tt.d_due_date IS NOT NULL AND tt.c_status NOT IN ('Done','Cancelled')
                                 AND tt.d_due_date < CAST(GETDATE() AS DATE)
                            THEN 1 ELSE 0 END AS b_overdue,
                       CASE WHEN tt.d_due_date IS NOT NULL AND tt.c_status NOT IN ('Done','Cancelled')
                                 AND DATEDIFF(DAY,tt.d_create_date,GETDATE())*1.0
                                   / NULLIF(DATEDIFF(DAY,tt.d_create_date,tt.d_due_date),0) >= 0.8
                            THEN 1 ELSE 0 END AS b_sla_warning,
                       ISNULL(cc.i_subtask_count,0) AS i_subtask_count,
                       ISNULL(cc.i_subtask_done, 0) AS i_subtask_done
                FROM TaskTree tt
                LEFT JOIN ChildCnt cc ON cc.i_parent_task_id = tt.i_task_id
                ORDER BY tt.sort_path
                OPTION (MAXRECURSION 50)";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@pid", projectId);
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>Insert Task ใหม่ คืน i_task_id</summary>
        public int InsertTask(ProjectTaskModel m)
        {
            int newId = -1;
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO t_project_task
                    (c_task_code, i_proj_state_id, i_project_id, i_parent_task_id,
                     i_subtask_depth, n_title, n_description, i_priority, c_status,
                     i_pct_complete, n_assignee, d_due_date, d_start_date,
                     i_estimated_hours, f_active, i_create_user, d_create_date)
                OUTPUT INSERTED.i_task_id
                VALUES
                    (@code, @sid, @pid, @parent,
                     @depth, @title, @desc, @prio, @stat,
                     @pct, @asgn, @due, @start,
                     @est, 'Y', @cu, GETDATE())";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@code",   m.c_task_code);
                cmd.Parameters.AddWithValue("@sid",    m.i_proj_state_id);
                cmd.Parameters.AddWithValue("@pid",    m.i_project_id);
                cmd.Parameters.AddWithValue("@parent", m.i_parent_task_id.HasValue ? (object)m.i_parent_task_id.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@depth",  m.i_subtask_depth);
                cmd.Parameters.AddWithValue("@title",  m.n_title);
                cmd.Parameters.AddWithValue("@desc",   string.IsNullOrEmpty(m.n_description) ? (object)DBNull.Value : m.n_description);
                cmd.Parameters.AddWithValue("@prio",   m.i_priority);
                cmd.Parameters.AddWithValue("@stat",   m.c_status ?? "Todo");
                cmd.Parameters.AddWithValue("@pct",    m.i_pct_complete);
                cmd.Parameters.AddWithValue("@asgn",   string.IsNullOrEmpty(m.n_assignee) ? (object)DBNull.Value : m.n_assignee);
                cmd.Parameters.AddWithValue("@due",    m.d_due_date.HasValue ? (object)m.d_due_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@start",  m.d_start_date.HasValue ? (object)m.d_start_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@est",    m.i_estimated_hours.HasValue ? (object)m.i_estimated_hours.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@cu",     m.i_create_user);
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) newId = Convert.ToInt32(res);
            }
            conn.Disconnect();
            return newId;
        }

        /// <summary>Update Task</summary>
        public bool UpdateTask(ProjectTaskModel m)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                UPDATE t_project_task SET
                    n_title           = @title,
                    n_description     = @desc,
                    i_priority        = @prio,
                    n_assignee        = @asgn,
                    d_due_date        = @due,
                    d_start_date      = @start,
                    i_estimated_hours = @est,
                    i_actual_hours    = @actual,
                    n_extend_reason   = @reason,
                    i_update_user     = @uu,
                    d_update_date     = GETDATE()
                WHERE i_task_id = @id AND f_active = 'Y'";
            int rows = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@title",  m.n_title);
                cmd.Parameters.AddWithValue("@desc",   string.IsNullOrEmpty(m.n_description) ? (object)DBNull.Value : m.n_description);
                cmd.Parameters.AddWithValue("@prio",   m.i_priority);
                cmd.Parameters.AddWithValue("@asgn",   string.IsNullOrEmpty(m.n_assignee) ? (object)DBNull.Value : m.n_assignee);
                cmd.Parameters.AddWithValue("@due",    m.d_due_date.HasValue ? (object)m.d_due_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@start",  m.d_start_date.HasValue ? (object)m.d_start_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@est",    m.i_estimated_hours.HasValue ? (object)m.i_estimated_hours.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@actual", m.i_actual_hours.HasValue ? (object)m.i_actual_hours.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@reason", string.IsNullOrEmpty(m.n_extend_reason) ? (object)DBNull.Value : m.n_extend_reason);
                cmd.Parameters.AddWithValue("@uu",     m.i_update_user);
                cmd.Parameters.AddWithValue("@id",     m.i_task_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>Update Status + % ของ Task</summary>
        public bool UpdateStatus(int i_task_id, string c_status, int i_pct_complete, string i_user)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                UPDATE t_project_task SET
                    c_status       = @stat,
                    i_pct_complete = @pct,
                    i_update_user  = @uu,
                    d_update_date  = GETDATE()
                WHERE i_task_id = @id AND f_active = 'Y'";
            int rows = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@stat", c_status);
                cmd.Parameters.AddWithValue("@pct",  i_pct_complete);
                cmd.Parameters.AddWithValue("@uu",   i_user);
                cmd.Parameters.AddWithValue("@id",   i_task_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>Soft-delete Task → Recycle Bin 30 วัน</summary>
        public bool SoftDelete(int i_task_id, string i_user)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                UPDATE t_project_task SET
                    f_active         = 'N',
                    d_deleted_date   = GETDATE(),
                    d_recycle_expiry = DATEADD(DAY, 30, GETDATE()),
                    i_update_user    = @uu,
                    d_update_date    = GETDATE()
                WHERE i_task_id = @id";
            int rows = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@uu", i_user);
                cmd.Parameters.AddWithValue("@id", i_task_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>กู้คืน Task จาก Recycle Bin</summary>
        public bool Restore(int i_task_id, string i_user)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                UPDATE t_project_task SET
                    f_active         = 'Y',
                    d_deleted_date   = NULL,
                    d_recycle_expiry = NULL,
                    i_update_user    = @uu,
                    d_update_date    = GETDATE()
                WHERE i_task_id = @id";
            int rows = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@uu", i_user);
                cmd.Parameters.AddWithValue("@id", i_task_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        /// <summary>คำนวณ % ของ State จาก Tasks ทั้งหมด (avg pct)</summary>
        public int CalcStatePctFromTasks(int i_proj_state_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT ISNULL(CAST(AVG(CAST(i_pct_complete AS FLOAT)) AS INT), 0)
                FROM t_project_task
                WHERE i_proj_state_id = @sid AND f_active = 'Y'
                  AND i_parent_task_id IS NULL";
            int pct = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@sid", i_proj_state_id);
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) pct = Convert.ToInt32(res);
            }
            conn.Disconnect();
            return pct;
        }

        /// <summary>ตรวจว่า State มี Task ที่ active อยู่ไหม (ใช้ก่อนลบ State)</summary>
        public bool HasActiveTasks(int i_proj_state_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "SELECT COUNT(*) FROM t_project_task WHERE i_proj_state_id=@sid AND f_active='Y'";
            int count = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@sid", i_proj_state_id);
                count = (int)cmd.ExecuteScalar();
            }
            conn.Disconnect();
            return count > 0;
        }

        // ─── Comments ─────────────────────────────────────────────────────────────

        public DataTable GetComments(int i_task_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT i_comment_id, i_task_id, i_parent_comment_id, i_thread_depth,
                       n_content, f_is_edited, f_is_deleted,
                       i_create_user, d_create_date, d_edited_date
                FROM t_project_task_comment
                WHERE i_task_id = @tid
                ORDER BY d_create_date";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@tid", i_task_id);
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }

        public int InsertComment(ProjectTaskCommentModel m)
        {
            int newId = -1;
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO t_project_task_comment
                    (i_task_id, i_parent_comment_id, i_thread_depth, n_content,
                     f_is_edited, f_is_deleted, i_create_user, d_create_date)
                OUTPUT INSERTED.i_comment_id
                VALUES (@tid, @parent, @depth, @content, 'N', 'N', @cu, GETDATE())";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@tid",    m.i_task_id);
                cmd.Parameters.AddWithValue("@parent", m.i_parent_comment_id.HasValue ? (object)m.i_parent_comment_id.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@depth",  m.i_thread_depth);
                cmd.Parameters.AddWithValue("@content", m.n_content);
                cmd.Parameters.AddWithValue("@cu",     m.i_create_user);
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) newId = Convert.ToInt32(res);
            }
            conn.Disconnect();
            return newId;
        }

        public bool SoftDeleteComment(int i_comment_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "UPDATE t_project_task_comment SET f_is_deleted='Y' WHERE i_comment_id=@id";
            int rows = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", i_comment_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        public bool EditComment(int i_comment_id, string n_content)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"UPDATE t_project_task_comment SET
                               n_content   = @c,
                               f_is_edited = 'Y',
                               d_edited_date = GETDATE()
                           WHERE i_comment_id = @id";
            int rows = 0;
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@c",  n_content);
                cmd.Parameters.AddWithValue("@id", i_comment_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }
    }
}
