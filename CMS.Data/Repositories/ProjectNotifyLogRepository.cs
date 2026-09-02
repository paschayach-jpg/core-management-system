using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CMS.Data.Repositories
{
    public class ProjectNotifyLogRepository : IProjectNotifyLogRepository
    {
        private readonly IConfigManager _configManager;

        public ProjectNotifyLogRepository(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        public int? GetExistingTaskId(int i_proj_state_id, int i_state_member_id)
        {
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
                string sql = @"
                    SELECT TOP 1 i_task_id
                    FROM RGP.t_project_notify_log
                    WHERE i_proj_state_id = @sid AND i_state_member_id = @mid AND i_task_id IS NOT NULL
                    ORDER BY d_send_date";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", i_proj_state_id);
                    cmd.Parameters.AddWithValue("@mid", i_state_member_id);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                    return null;
                }
            }
        }

        public bool Insert(ProjectNotifyLogModel m)
        {
            int rows = 0;
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO RGP.t_project_notify_log
                        (i_project_id, i_proj_state_id, i_state_member_id, n_recipient_name,
                         i_task_id, f_task_created, c_api_result, n_message, i_send_user, d_send_date)
                    VALUES
                        (@pid, @sid, @mid, @name, @taskId, @created, @result, @msg, @user, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@pid",     m.i_project_id);
                    cmd.Parameters.AddWithValue("@sid",     m.i_proj_state_id);
                    cmd.Parameters.AddWithValue("@mid",     m.i_state_member_id);
                    cmd.Parameters.AddWithValue("@name",    m.n_recipient_name ?? "");
                    cmd.Parameters.AddWithValue("@taskId",  m.i_task_id.HasValue ? (object)m.i_task_id.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@created", m.f_task_created ?? "N");
                    cmd.Parameters.AddWithValue("@result",  m.c_api_result ?? "Success");
                    cmd.Parameters.AddWithValue("@msg",     (object)m.n_message ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@user",    (object)m.i_send_user ?? DBNull.Value);
                    rows = cmd.ExecuteNonQuery();
                }
            }
            return rows > 0;
        }
    }
}
