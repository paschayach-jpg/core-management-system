using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class HrPositionMappingRepository : IHrPositionMappingRepository
    {
        private readonly string _connectionString;

        // ตรงกับ query ที่ถูกต้อง:
        // SELECT a.MappingID, a.DivisionID, c.DivisionName, a.DepartmentID, d.DepartmentName,
        //        a.PositionID, b.PositionName, a.HeadCount, e.LevelCode
        // FROM HR_PositionMapping a
        // INNER JOIN HR_Position b ON a.PositionID = b.PositionID
        // INNER JOIN HR_Division c ON a.DivisionID = c.DivisionID
        // INNER JOIN HR_Department d ON a.DepartmentID = d.DepartmentID
        // INNER JOIN HR_Position e ON a.PositionID = e.PositionID  ← LevelCode
        private const string _selectSql =
            @"SELECT
                m.MappingID,
                m.DivisionID,   v.DivisionCode,  v.DivisionName,
                m.DepartmentID, d.DepartmentCode,
                ISNULL(d.DepartmentNameTh, d.DepartmentName) AS DepartmentName,
                m.PositionID,   p.PositionCode,  p.PositionName,
                m.HeadCount,    m.IsActive,       m.CreatedDate,
                ISNULL(p.Level,'') AS LevelCode
              FROM core.HR_PositionMapping m
              INNER JOIN core.HR_Position   p ON p.PositionID   = m.PositionID
              INNER JOIN core.HR_Division   v ON v.DivisionID   = m.DivisionID
              INNER JOIN core.HR_Department d ON d.DepartmentID = m.DepartmentID
              WHERE m.IsActive = 1";

        public HrPositionMappingRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<HrPositionMapping> GetAll()
        {
            return Query(_selectSql + " ORDER BY v.DivisionCode, d.DepartmentCode, p.PositionName", null);
        }

        public IEnumerable<HrPositionMapping> GetByDepartmentId(int departmentId)
        {
            return Query(
                _selectSql + " AND m.DepartmentID = @DepartmentID ORDER BY p.PositionName",
                cmd => cmd.Parameters.AddWithValue("@DepartmentID", departmentId));
        }

        public IEnumerable<HrPositionMapping> GetByDivisionId(int divisionId)
        {
            return Query(
                _selectSql + " AND m.DivisionID = @DivisionID ORDER BY d.DepartmentCode, p.PositionName",
                cmd => cmd.Parameters.AddWithValue("@DivisionID", divisionId));
        }

        private IEnumerable<HrPositionMapping> Query(string sql, Action<SqlCommand> addParams)
        {
            var results = new List<HrPositionMapping>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    addParams?.Invoke(cmd);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) results.Add(Map(r));
                }
            }
            return results;
        }

        private static HrPositionMapping Map(SqlDataReader r)
        {
            return new HrPositionMapping
            {
                MappingID      = (int)r["MappingID"],
                PositionID     = (int)r["PositionID"],
                DepartmentID   = r["DepartmentID"] == DBNull.Value ? (int?)null : (int)r["DepartmentID"],
                DivisionID     = (int)r["DivisionID"],
                HeadCount      = r["HeadCount"]    == DBNull.Value ? 0 : (int)r["HeadCount"],
                IsActive       = (bool)r["IsActive"],
                CreatedDate    = (DateTime)r["CreatedDate"],
                PositionCode   = r["PositionCode"]   == DBNull.Value ? "" : Convert.ToString(r["PositionCode"]),
                PositionName   = r["PositionName"]   == DBNull.Value ? "" : Convert.ToString(r["PositionName"]),
                Level          = r["LevelCode"]      == DBNull.Value ? "" : Convert.ToString(r["LevelCode"]),
                DepartmentCode = r["DepartmentCode"] == DBNull.Value ? "" : Convert.ToString(r["DepartmentCode"]),
                DepartmentName = r["DepartmentName"] == DBNull.Value ? "" : Convert.ToString(r["DepartmentName"]),
                DivisionCode   = r["DivisionCode"]   == DBNull.Value ? "" : Convert.ToString(r["DivisionCode"]),
                DivisionName   = r["DivisionName"]   == DBNull.Value ? "" : Convert.ToString(r["DivisionName"])
            };
        }
    }
}
