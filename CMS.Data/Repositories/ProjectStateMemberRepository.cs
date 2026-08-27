using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace CMS.Data.Repositories
{
    public class ProjectStateMemberRepository : IProjectStateMemberRepository
    {
        private readonly IConfigManager _configManager;

        public ProjectStateMemberRepository(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        public DataTable GetByState(int i_proj_state_id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT
                    sm.i_state_member_id, sm.n_member_name, sm.c_role, sm.c_member_type,
                    sm.i_create_user, sm.d_create_date,
                    ISNULL(nl.send_count, 0) AS i_notify_count
                FROM RGP.t_project_state_member sm
                LEFT JOIN (
                    SELECT i_state_member_id, COUNT(*) AS send_count
                    FROM RGP.t_project_notify_log
                    GROUP BY i_state_member_id
                ) nl ON nl.i_state_member_id = sm.i_state_member_id
                WHERE sm.i_proj_state_id = @sid
                ORDER BY sm.i_state_member_id";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@sid", i_proj_state_id);
                using (SqlDataReader r = cmd.ExecuteReader()) { dt.Load(r); }
            }
            }
            return dt;
        }

        public bool Insert(ProjectStateMemberModel m)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                INSERT INTO RGP.t_project_state_member
                    (i_proj_state_id, i_project_id, n_member_name, c_role, c_member_type, i_create_user, d_create_date)
                VALUES (@sid, @pid, @name, @role, @type, @user, GETDATE())";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@sid",  m.i_proj_state_id);
                cmd.Parameters.AddWithValue("@pid",  m.i_project_id);
                cmd.Parameters.AddWithValue("@name", m.n_member_name);
                cmd.Parameters.AddWithValue("@role", m.c_role);
                cmd.Parameters.AddWithValue("@type", (object)m.c_member_type ?? System.DBNull.Value);
                cmd.Parameters.AddWithValue("@user", m.i_create_user ?? "");
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        public bool Delete(int i_state_member_id)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = "DELETE FROM RGP.t_project_state_member WHERE i_state_member_id = @id";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", i_state_member_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }
    }
}
