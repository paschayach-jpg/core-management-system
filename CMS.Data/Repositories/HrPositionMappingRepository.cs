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

        private const string _selectSql =
            @"SELECT
                m.MappingID,
                m.DivisionID,   v.DivisionCode,  v.DivisionName,
                m.DepartmentID, d.DepartmentCode,
                ISNULL(d.DepartmentNameTh, d.DepartmentName) AS DepartmentName,
                m.PositionID,   p.PositionCode,  p.PositionName,
                m.HeadCount,    m.IsActive,       m.CreatedDate,
                ISNULL(p.Level,'') AS LevelCode
              FROM EMS.m_PositionMapping m
              INNER JOIN EMS.m_Role       p ON p.PositionID   = m.PositionID
              INNER JOIN EMS.m_Division   v ON v.DivisionID   = m.DivisionID
              INNER JOIN EMS.m_Department d ON d.DepartmentID = m.DepartmentID
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

        public int Save(HrPositionMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException("mapping");

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                if (mapping.MappingID > 0)
                {
                    using (var cmd = new SqlCommand(
                        "UPDATE EMS.m_PositionMapping " +
                        "SET DivisionID = @DivisionID, DepartmentID = @DepartmentID, PositionID = @PositionID, " +
                        "    HeadCount = @HeadCount, IsActive = @IsActive " +
                        "WHERE MappingID = @MappingID", conn))
                    {
                        cmd.Parameters.AddWithValue("@MappingID", mapping.MappingID);
                        cmd.Parameters.AddWithValue("@DivisionID", mapping.DivisionID);
                        cmd.Parameters.AddWithValue("@DepartmentID", (object)mapping.DepartmentID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PositionID", mapping.PositionID);
                        cmd.Parameters.AddWithValue("@HeadCount", mapping.HeadCount);
                        cmd.Parameters.AddWithValue("@IsActive", mapping.IsActive);
                        cmd.ExecuteNonQuery();
                        return mapping.MappingID;
                    }
                }
                else
                {
                    // Check if already mapped
                    using (var chkCmd = new SqlCommand(
                        "SELECT MappingID FROM EMS.m_PositionMapping " +
                        "WHERE DivisionID = @DivisionID AND DepartmentID = @DepartmentID AND PositionID = @PositionID", conn))
                    {
                        chkCmd.Parameters.AddWithValue("@DivisionID", mapping.DivisionID);
                        chkCmd.Parameters.AddWithValue("@DepartmentID", (object)mapping.DepartmentID ?? DBNull.Value);
                        chkCmd.Parameters.AddWithValue("@PositionID", mapping.PositionID);
                        var existingId = chkCmd.ExecuteScalar();
                        if (existingId != null && existingId != DBNull.Value)
                        {
                            int id = Convert.ToInt32(existingId);
                            using (var updateCmd = new SqlCommand(
                                "UPDATE EMS.m_PositionMapping " +
                                "SET HeadCount = @HeadCount, IsActive = @IsActive " +
                                "WHERE MappingID = @MappingID", conn))
                            {
                                updateCmd.Parameters.AddWithValue("@MappingID", id);
                                updateCmd.Parameters.AddWithValue("@HeadCount", mapping.HeadCount);
                                updateCmd.Parameters.AddWithValue("@IsActive", mapping.IsActive);
                                updateCmd.ExecuteNonQuery();
                                return id;
                            }
                        }
                    }

                    using (var cmd = new SqlCommand(
                        "INSERT INTO EMS.m_PositionMapping (DivisionID, DepartmentID, PositionID, HeadCount, IsActive, CreatedDate) " +
                        "VALUES (@DivisionID, @DepartmentID, @PositionID, @HeadCount, @IsActive, GETDATE()); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
                    {
                        cmd.Parameters.AddWithValue("@DivisionID", mapping.DivisionID);
                        cmd.Parameters.AddWithValue("@DepartmentID", (object)mapping.DepartmentID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PositionID", mapping.PositionID);
                        cmd.Parameters.AddWithValue("@HeadCount", mapping.HeadCount);
                        cmd.Parameters.AddWithValue("@IsActive", mapping.IsActive);
                        var newId = cmd.ExecuteScalar();
                        return newId != null && newId != DBNull.Value ? Convert.ToInt32(newId) : 0;
                    }
                }
            }
        }

        public bool Delete(int mappingId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM EMS.m_PositionMapping WHERE MappingID = @MappingID", conn))
                {
                    cmd.Parameters.AddWithValue("@MappingID", mappingId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private IEnumerable<HrPositionMapping> Query(string sql, Action<SqlCommand> addParams)
        {
            var results = new List<HrPositionMapping>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (addParams != null) addParams(cmd);
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
                MappingID      = r["MappingID"] == DBNull.Value ? 0 : Convert.ToInt32(r["MappingID"]),
                PositionID     = r["PositionID"] == DBNull.Value ? 0 : Convert.ToInt32(r["PositionID"]),
                DepartmentID   = r["DepartmentID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["DepartmentID"]),
                DivisionID     = r["DivisionID"] == DBNull.Value ? 0 : Convert.ToInt32(r["DivisionID"]),
                HeadCount      = r["HeadCount"] == DBNull.Value ? 0 : Convert.ToInt32(r["HeadCount"]),
                IsActive       = r["IsActive"] != DBNull.Value && Convert.ToBoolean(r["IsActive"]),
                CreatedDate    = r["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["CreatedDate"]),
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
