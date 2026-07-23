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
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ProjectDbConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT i_state_member_id, n_member_name, c_role, i_create_user, d_create_date
                FROM t_project_state_member
                WHERE i_proj_state_id = @sid
                ORDER BY i_state_member_id";
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
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ProjectDbConnectionString")))
            {
                conn.Open();
            string sql = @"
                INSERT INTO t_project_state_member
                    (i_proj_state_id, i_project_id, n_member_name, c_role, i_create_user, d_create_date)
                VALUES (@sid, @pid, @name, @role, @user, GETDATE())";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@sid",  m.i_proj_state_id);
                cmd.Parameters.AddWithValue("@pid",  m.i_project_id);
                cmd.Parameters.AddWithValue("@name", m.n_member_name);
                cmd.Parameters.AddWithValue("@role", m.c_role);
                cmd.Parameters.AddWithValue("@user", m.i_create_user ?? "");
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        public bool Delete(int i_state_member_id)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ProjectDbConnectionString")))
            {
                conn.Open();
            string sql = "DELETE FROM t_project_state_member WHERE i_state_member_id = @id";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", i_state_member_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }
    }
}
