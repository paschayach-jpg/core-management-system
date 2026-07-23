using System;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    /// <summary>
    /// Audit repository เฉพาะโมดูล Project (RGP) — เขียนลง t_project_audit_log
    /// แยกจาก CMS.Data.Repositories.AuditRepository เพราะ shape/ตารางต่างกัน
    /// </summary>
    public class ProjectAuditRepository : IProjectAuditRepository
    {
        private readonly IConfigManager _configManager;

        public ProjectAuditRepository(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        public void Insert(ProjectAuditEntry entry)
        {
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ProjectDbConnectionString")))
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO t_project_audit_log
                        (i_user_id, n_module, n_action, n_table_name, i_record_id,
                         i_result, n_message, n_machine_name, d_action_date)
                    VALUES
                        (@user_id, @module, @action, @table_name, @record_id,
                         @result, @message, @machine_name, @action_date)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_id", entry.UserID ?? "");
                    cmd.Parameters.AddWithValue("@module", entry.Module ?? "");
                    cmd.Parameters.AddWithValue("@action", entry.Action ?? "");
                    cmd.Parameters.AddWithValue("@table_name", (object)entry.TableName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@record_id", entry.RecordID.HasValue ? (object)entry.RecordID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@result", entry.Result);
                    cmd.Parameters.AddWithValue("@message", (object)entry.Message ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@machine_name", (object)entry.MachineName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@action_date", entry.ActionDate);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
