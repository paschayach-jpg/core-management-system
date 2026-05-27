using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class HrDepartmentRepository : IHrDepartmentRepository
    {
        private readonly string _connectionString;

        public HrDepartmentRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<HrDepartment> GetAll()
        {
            var results = new List<HrDepartment>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT DepartmentID, DivisionID, DepartmentCode, DepartmentName, IsActive, CreatedDate," +
                    "       ISNULL(Level,'') AS Level," +
                    "       ISNULL(DepartmentNameTh,'') AS DepartmentNameTh," +
                    "       ISNULL(DepartmentNameEn,'') AS DepartmentNameEn " +
                    "FROM EMS.m_Department WHERE IsActive = 1 ORDER BY DepartmentCode", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) results.Add(MapDepartment(reader));
                }
            }
            return results;
        }

        public IEnumerable<HrDepartment> GetByDivisionID(int divisionID)
        {
            var results = new List<HrDepartment>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT DepartmentID, DivisionID, DepartmentCode, DepartmentName, IsActive, CreatedDate," +
                    "       ISNULL(Level,'') AS Level," +
                    "       ISNULL(DepartmentNameTh,'') AS DepartmentNameTh," +
                    "       ISNULL(DepartmentNameEn,'') AS DepartmentNameEn " +
                    "FROM EMS.m_Department WHERE IsActive = 1 AND DivisionID = @DivisionID ORDER BY DepartmentCode", conn))
                {
                    cmd.Parameters.AddWithValue("@DivisionID", divisionID);
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) results.Add(MapDepartment(reader));
                }
            }
            return results;
        }

        private static HrDepartment MapDepartment(SqlDataReader reader)
        {
            return new HrDepartment
            {
                DepartmentID     = Convert.ToInt32(reader["DepartmentID"]),
                DivisionID       = Convert.ToInt32(reader["DivisionID"]),
                DepartmentCode   = reader["DepartmentCode"]   == DBNull.Value ? string.Empty : Convert.ToString(reader["DepartmentCode"]),
                DepartmentName   = reader["DepartmentName"]   == DBNull.Value ? string.Empty : Convert.ToString(reader["DepartmentName"]),
                IsActive         = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                CreatedDate      = reader["CreatedDate"]      == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedDate"]),
                Level            = reader["Level"]            == DBNull.Value ? string.Empty : Convert.ToString(reader["Level"]),
                DepartmentNameTh = reader["DepartmentNameTh"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DepartmentNameTh"]),
                DepartmentNameEn = reader["DepartmentNameEn"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DepartmentNameEn"])
            };
        }
    }
}

