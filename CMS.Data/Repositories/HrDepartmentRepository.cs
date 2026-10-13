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
                    "FROM EMS.m_Department ORDER BY DepartmentCode", conn))
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
                    "FROM EMS.m_Department WHERE DivisionID = @DivisionID ORDER BY DepartmentCode", conn))
                {
                    cmd.Parameters.AddWithValue("@DivisionID", divisionID);
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) results.Add(MapDepartment(reader));
                }
            }
            return results;
        }

        public int Save(HrDepartment department)
        {
            if (department == null) throw new ArgumentNullException("department");

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                if (department.DepartmentID > 0)
                {
                    using (var cmd = new SqlCommand(
                        "UPDATE EMS.m_Department " +
                        "SET DivisionID = @DivisionID, DepartmentCode = @DepartmentCode, " +
                        "    DepartmentName = @DepartmentName, DepartmentNameTh = @DepartmentNameTh, " +
                        "    DepartmentNameEn = @DepartmentNameEn, Level = @Level, IsActive = @IsActive " +
                        "WHERE DepartmentID = @DepartmentID", conn))
                    {
                        cmd.Parameters.AddWithValue("@DepartmentID", department.DepartmentID);
                        cmd.Parameters.AddWithValue("@DivisionID", department.DivisionID);
                        cmd.Parameters.AddWithValue("@DepartmentCode", department.DepartmentCode ?? "");
                        cmd.Parameters.AddWithValue("@DepartmentName", department.DepartmentName ?? "");
                        cmd.Parameters.AddWithValue("@DepartmentNameTh", department.DepartmentNameTh ?? department.DepartmentName ?? "");
                        cmd.Parameters.AddWithValue("@DepartmentNameEn", department.DepartmentNameEn ?? "");
                        cmd.Parameters.AddWithValue("@Level", department.Level ?? "");
                        cmd.Parameters.AddWithValue("@IsActive", department.IsActive);
                        cmd.ExecuteNonQuery();
                        return department.DepartmentID;
                    }
                }
                else
                {
                    // Check if DepartmentCode exists
                    using (var chkCmd = new SqlCommand("SELECT DepartmentID FROM EMS.m_Department WHERE DepartmentCode = @DepartmentCode", conn))
                    {
                        chkCmd.Parameters.AddWithValue("@DepartmentCode", department.DepartmentCode ?? "");
                        var existingId = chkCmd.ExecuteScalar();
                        if (existingId != null && existingId != DBNull.Value)
                        {
                            int id = Convert.ToInt32(existingId);
                            using (var updateCmd = new SqlCommand(
                                "UPDATE EMS.m_Department " +
                                "SET DivisionID = @DivisionID, " +
                                "    DepartmentName = @DepartmentName, DepartmentNameTh = @DepartmentNameTh, " +
                                "    DepartmentNameEn = @DepartmentNameEn, Level = @Level, IsActive = @IsActive " +
                                "WHERE DepartmentID = @DepartmentID", conn))
                            {
                                updateCmd.Parameters.AddWithValue("@DepartmentID", id);
                                updateCmd.Parameters.AddWithValue("@DivisionID", department.DivisionID);
                                updateCmd.Parameters.AddWithValue("@DepartmentName", department.DepartmentName ?? "");
                                updateCmd.Parameters.AddWithValue("@DepartmentNameTh", department.DepartmentNameTh ?? department.DepartmentName ?? "");
                                updateCmd.Parameters.AddWithValue("@DepartmentNameEn", department.DepartmentNameEn ?? "");
                                updateCmd.Parameters.AddWithValue("@Level", department.Level ?? "");
                                updateCmd.Parameters.AddWithValue("@IsActive", department.IsActive);
                                updateCmd.ExecuteNonQuery();
                                return id;
                            }
                        }
                    }

                    using (var cmd = new SqlCommand(
                        "INSERT INTO EMS.m_Department (DivisionID, DepartmentCode, DepartmentName, DepartmentNameTh, DepartmentNameEn, Level, IsActive, CreatedDate) " +
                        "VALUES (@DivisionID, @DepartmentCode, @DepartmentName, @DepartmentNameTh, @DepartmentNameEn, @Level, @IsActive, GETDATE()); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
                    {
                        cmd.Parameters.AddWithValue("@DivisionID", department.DivisionID);
                        cmd.Parameters.AddWithValue("@DepartmentCode", department.DepartmentCode ?? "");
                        cmd.Parameters.AddWithValue("@DepartmentName", department.DepartmentName ?? "");
                        cmd.Parameters.AddWithValue("@DepartmentNameTh", department.DepartmentNameTh ?? department.DepartmentName ?? "");
                        cmd.Parameters.AddWithValue("@DepartmentNameEn", department.DepartmentNameEn ?? "");
                        cmd.Parameters.AddWithValue("@Level", department.Level ?? "");
                        cmd.Parameters.AddWithValue("@IsActive", department.IsActive);
                        var newId = cmd.ExecuteScalar();
                        return newId != null && newId != DBNull.Value ? Convert.ToInt32(newId) : 0;
                    }
                }
            }
        }

        public bool Delete(int departmentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM EMS.m_Department WHERE DepartmentID = @DepartmentID", conn))
                {
                    cmd.Parameters.AddWithValue("@DepartmentID", departmentId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private static HrDepartment MapDepartment(SqlDataReader reader)
        {
            string legacyName = reader["DepartmentName"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DepartmentName"]);
            string nameTh     = reader["DepartmentNameTh"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DepartmentNameTh"]);

            return new HrDepartment
            {
                DepartmentID     = reader["DepartmentID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DepartmentID"]),
                DivisionID       = reader["DivisionID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DivisionID"]),
                DepartmentCode   = reader["DepartmentCode"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DepartmentCode"]),
                DepartmentName   = legacyName,
                // Older rows only have the legacy DepartmentName; use it so screens never show a blank Thai name.
                DepartmentNameTh = string.IsNullOrWhiteSpace(nameTh) ? legacyName : nameTh,
                DepartmentNameEn = reader["DepartmentNameEn"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DepartmentNameEn"]),
                Level            = reader["Level"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Level"]),
                IsActive         = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                CreatedDate      = reader["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedDate"])
            };
        }
    }
}
