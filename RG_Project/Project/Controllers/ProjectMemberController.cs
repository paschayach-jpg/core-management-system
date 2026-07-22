using RG_Project.Project.Models;
using System;
using RG_Project.Controllers;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    public class ProjectMemberController
    {
        public DataTable GetByProject(int i_project_id)
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT i_member_id, n_member_name, c_role, department_code
                FROM t_project_member
                WHERE i_project_id = @pid
                ORDER BY i_member_id";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@pid", i_project_id);
                using (SqlDataReader r = cmd.ExecuteReader()) { dt.Load(r); }
            }
            conn.Disconnect();
            return dt;
        }

        public bool Insert(ProjectMemberModel m)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                INSERT INTO t_project_member (i_project_id, n_member_name, c_role, c_department_code, i_create_user, d_create_date)
                VALUES (@pid, @name, @role, @dept, @user, GETDATE())";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@pid",  m.i_project_id);
                cmd.Parameters.AddWithValue("@name", m.n_member_name);
                cmd.Parameters.AddWithValue("@role", m.c_role);
                cmd.Parameters.AddWithValue("@dept", (object)m.c_department_code ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@user", m.i_create_user);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        public bool Delete(int i_member_id)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "DELETE FROM t_project_member WHERE i_member_id = @id";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", i_member_id);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

    }
}
