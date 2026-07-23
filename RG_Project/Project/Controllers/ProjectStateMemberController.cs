using CMS.Domain.Models;
using RG_Project.Controllers;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    public class ProjectStateMemberController
    {
        public DataTable GetByState(int i_proj_state_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT i_state_member_id, n_member_name, c_role, i_create_user, d_create_date
                FROM t_project_state_member
                WHERE i_proj_state_id = @sid
                ORDER BY i_state_member_id";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@sid", i_proj_state_id);
                using (SqlDataReader r = cmd.ExecuteReader()) { dt.Load(r); }
            }
            conn.Disconnect();
            return dt;
        }

        public bool Insert(ProjectStateMemberModel m)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO t_project_state_member
                    (i_proj_state_id, i_project_id, n_member_name, c_role, i_create_user, d_create_date)
                VALUES (@sid, @pid, @name, @role, @user, GETDATE())";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@sid",  m.i_proj_state_id);
                cmd.Parameters.AddWithValue("@pid",  m.i_project_id);
                cmd.Parameters.AddWithValue("@name", m.n_member_name);
                cmd.Parameters.AddWithValue("@role", m.c_role);
                cmd.Parameters.AddWithValue("@user", m.i_create_user ?? "");
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        public bool Delete(int i_state_member_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "DELETE FROM t_project_state_member WHERE i_state_member_id = @id";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", i_state_member_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }
    }
}
