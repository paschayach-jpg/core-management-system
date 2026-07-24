using RG_Project.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    public class EmployeeController
    {
        /// <summary>ดึงรายชื่อพนักงานที่ยังทำงานอยู่ จาก Common DB</summary>
        public DataTable GetActiveEmployees()
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('m');
            string sql = @"
                SELECT
                    employee_code,
                    LOWER(employee_first_name_th + ' ' + employee_last_name_th) AS full_name
                FROM dbo.Employees
               -- WHERE date_end IS NULL OR date_end >= CAST(GETDATE() AS DATE)
                ORDER BY employee_first_name_th, employee_last_name_th";
            using (var cmd = new SqlCommand(sql, conn.connection))
            using (var r = cmd.ExecuteReader())
                dt.Load(r);
            conn.Disconnect();
            return dt;
        }
    }
}
