using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Interfaces;
using System;
using System.Data.SqlClient;

namespace CMS.Data.Repositories
{
    public class ProjectTaskStatusLogRepository : IProjectTaskStatusLogRepository
    {
        private readonly IConfigManager _configManager;

        public ProjectTaskStatusLogRepository(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        public bool Insert(ProjectTaskStatusLogModel m)
        {
            int rows = 0;
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO RGP.t_project_task_status_log
                        (i_task_id, c_old_status, c_new_status, i_pct_at_change, i_change_user, d_change_date)
                    VALUES
                        (@tid, @oldStat, @newStat, @pct, @user, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tid",     m.i_task_id);
                    cmd.Parameters.AddWithValue("@oldStat", (object)m.c_old_status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@newStat", m.c_new_status);
                    cmd.Parameters.AddWithValue("@pct",     m.i_pct_at_change);
                    cmd.Parameters.AddWithValue("@user",    (object)m.i_change_user ?? DBNull.Value);
                    rows = cmd.ExecuteNonQuery();
                }
            }
            return rows > 0;
        }
    }
}
