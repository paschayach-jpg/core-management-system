using RG_Project.Project.Models;
using RG_Project.Controllers;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    public class ProjectMemberRoleController
    {
        public DataTable GetAll()
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT i_role_id, n_role_name, f_active,
                       i_create_user, d_create_date, i_update_user, d_update_date
                FROM m_project_member_role ORDER BY i_role_id";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            using (SqlDataReader r = cmd.ExecuteReader()) { dt.Load(r); }
            conn.Disconnect();
            return dt;
        }

        public DataTable GetActive()
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "SELECT i_role_id, n_role_name FROM m_project_member_role WHERE f_active='Y' ORDER BY i_role_id";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            using (SqlDataReader r = cmd.ExecuteReader()) { dt.Load(r); }
            conn.Disconnect();
            return dt;
        }

        public bool Insert(string roleName, string userName)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"INSERT INTO m_project_member_role (n_role_name, i_create_user, d_create_date)
                           VALUES (@name, @user, GETDATE())";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@name", roleName.Trim());
                cmd.Parameters.AddWithValue("@user", userName ?? "");
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        public bool Update(int roleId, string roleName, string fActive, string userName)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"UPDATE m_project_member_role
                           SET n_role_name=@name, f_active=@active,
                               i_update_user=@user, d_update_date=GETDATE()
                           WHERE i_role_id=@id";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@name",   roleName.Trim());
                cmd.Parameters.AddWithValue("@active", fActive);
                cmd.Parameters.AddWithValue("@user",   userName ?? "");
                cmd.Parameters.AddWithValue("@id",     roleId);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        public bool Delete(int roleId)
        {
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = "DELETE FROM m_project_member_role WHERE i_role_id=@id";
            int rows = 0;
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", roleId);
                rows = cmd.ExecuteNonQuery();
            }
            conn.Disconnect();
            return rows > 0;
        }

        public DataTable GetSubCommittees()
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('b');
            string sql = "SELECT SubCommitteeID, SubCommitteeNameTH FROM CMS_DB.EMS.m_SubCommittee ORDER BY SubCommitteeNameTH";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            using (SqlDataReader r = cmd.ExecuteReader()) { dt.Load(r); }
            conn.Disconnect();
            return dt;
        }

        public DataTable GetDivisions()
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('b');
            string sql = "SELECT DivisionID, DivisionName, DivisionCode FROM CMS_DB.EMS.m_Division ORDER BY DivisionName";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            using (SqlDataReader r = cmd.ExecuteReader()) { dt.Load(r); }
            conn.Disconnect();
            return dt;
        }

        public DataTable GetDepartments()
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('b');
            string sql = "SELECT DepartmentID, DepartmentName, DepartmentCode FROM CMS_DB.EMS.m_Department ORDER BY DepartmentName";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            using (SqlDataReader r = cmd.ExecuteReader()) { dt.Load(r); }
            conn.Disconnect();
            return dt;
        }
    }
}
