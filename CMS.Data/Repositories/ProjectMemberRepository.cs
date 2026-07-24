using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CMS.Data.Repositories
{
    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly IConfigManager _configManager;

        public ProjectMemberRepository(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        public DataTable GetByProject(int i_project_id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT i_member_id, n_member_name, c_role, department_code
                FROM RGP.t_project_member
                WHERE i_project_id = @pid
                ORDER BY i_member_id";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", i_project_id);
                using (SqlDataReader r = cmd.ExecuteReader()) { dt.Load(r); }
            }
            }
            return dt;
        }

        public bool Insert(ProjectMemberModel m)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = @"
                INSERT INTO RGP.t_project_member (i_project_id, n_member_name, c_role, department_code, i_create_user, d_create_date)
                VALUES (@pid, @name, @role, @dept, @user, GETDATE())";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid",  m.i_project_id);
                cmd.Parameters.AddWithValue("@name", m.n_member_name);
                cmd.Parameters.AddWithValue("@role", m.c_role);
                cmd.Parameters.AddWithValue("@dept", (object)m.c_department_code ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@user", m.i_create_user);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        public bool Delete(int i_member_id)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ConnectionString")))
            {
                conn.Open();
            string sql = "DELETE FROM RGP.t_project_member WHERE i_member_id = @id";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", i_member_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

    }
}
