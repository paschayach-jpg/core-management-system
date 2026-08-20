using CMS.Infrastructure.Interfaces;
using CMS.Data.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CMS.Data.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IConfigManager _configManager;

        public EmployeeRepository(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        /// <summary>ดึงรายชื่อพนักงานที่ยังทำงานอยู่ จาก Common DB</summary>
        public DataTable GetActiveEmployees()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("CommonDbConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT
                    employee_code,
                    LOWER(employee_first_name_th + ' ' + employee_last_name_th) AS full_name
                FROM dbo.Employees
               -- WHERE date_end IS NULL OR date_end >= CAST(GETDATE() AS DATE)
                ORDER BY employee_first_name_th, employee_last_name_th";
            using (var cmd = new SqlCommand(sql, conn))
            using (var r = cmd.ExecuteReader())
                dt.Load(r);
            }
            return dt;
        }
    }
}
