using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class HrDivisionRepository : IHrDivisionRepository
    {
        private readonly string _connectionString;

        public HrDivisionRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<HrDivision> GetAll()
        {
            var results = new List<HrDivision>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT DivisionID, DivisionCode, DivisionName, IsActive, CreatedDate " +
                    "FROM EMS.m_Division ORDER BY DivisionCode", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapDivision(reader));
                    }
                }
            }
            return results;
        }

        public int Save(HrDivision division)
        {
            if (division == null) throw new ArgumentNullException("division");

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                if (division.DivisionID > 0)
                {
                    using (var cmd = new SqlCommand(
                        "UPDATE EMS.m_Division " +
                        "SET DivisionCode = @DivisionCode, DivisionName = @DivisionName, IsActive = @IsActive " +
                        "WHERE DivisionID = @DivisionID", conn))
                    {
                        cmd.Parameters.AddWithValue("@DivisionID", division.DivisionID);
                        cmd.Parameters.AddWithValue("@DivisionCode", division.DivisionCode ?? "");
                        cmd.Parameters.AddWithValue("@DivisionName", division.DivisionName ?? "");
                        cmd.Parameters.AddWithValue("@IsActive", division.IsActive);
                        cmd.ExecuteNonQuery();
                        return division.DivisionID;
                    }
                }
                else
                {
                    // Check if DivisionCode already exists
                    using (var chkCmd = new SqlCommand("SELECT DivisionID FROM EMS.m_Division WHERE DivisionCode = @DivisionCode", conn))
                    {
                        chkCmd.Parameters.AddWithValue("@DivisionCode", division.DivisionCode ?? "");
                        var existingId = chkCmd.ExecuteScalar();
                        if (existingId != null && existingId != DBNull.Value)
                        {
                            int id = Convert.ToInt32(existingId);
                            using (var updateCmd = new SqlCommand(
                                "UPDATE EMS.m_Division " +
                                "SET DivisionName = @DivisionName, IsActive = @IsActive " +
                                "WHERE DivisionID = @DivisionID", conn))
                            {
                                updateCmd.Parameters.AddWithValue("@DivisionID", id);
                                updateCmd.Parameters.AddWithValue("@DivisionName", division.DivisionName ?? "");
                                updateCmd.Parameters.AddWithValue("@IsActive", division.IsActive);
                                updateCmd.ExecuteNonQuery();
                                return id;
                            }
                        }
                    }

                    using (var cmd = new SqlCommand(
                        "INSERT INTO EMS.m_Division (DivisionCode, DivisionName, IsActive, CreatedDate) " +
                        "VALUES (@DivisionCode, @DivisionName, @IsActive, GETDATE()); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
                    {
                        cmd.Parameters.AddWithValue("@DivisionCode", division.DivisionCode ?? "");
                        cmd.Parameters.AddWithValue("@DivisionName", division.DivisionName ?? "");
                        cmd.Parameters.AddWithValue("@IsActive", division.IsActive);
                        var newId = cmd.ExecuteScalar();
                        return newId != null && newId != DBNull.Value ? Convert.ToInt32(newId) : 0;
                    }
                }
            }
        }

        public bool Delete(int divisionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM EMS.m_Division WHERE DivisionID = @DivisionID", conn))
                {
                    cmd.Parameters.AddWithValue("@DivisionID", divisionId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private static HrDivision MapDivision(SqlDataReader reader)
        {
            return new HrDivision
            {
                DivisionID   = Convert.ToInt32(reader["DivisionID"]),
                DivisionCode = reader["DivisionCode"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DivisionCode"]),
                DivisionName = reader["DivisionName"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DivisionName"]),
                IsActive     = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                CreatedDate  = reader["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedDate"])
            };
        }
    }
}
