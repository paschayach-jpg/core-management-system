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
                    "FROM EMS.m_Division WHERE IsActive = 1 ORDER BY DivisionCode", conn))
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

        private static HrDivision MapDivision(SqlDataReader reader)
        {
            return new HrDivision
            {
                DivisionID   = Convert.ToInt32(reader["DivisionID"]),
                DivisionCode = reader["DivisionCode"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DivisionCode"]).Trim(),
                DivisionName = reader["DivisionName"] == DBNull.Value ? string.Empty : Convert.ToString(reader["DivisionName"]).Trim(),
                IsActive     = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                CreatedDate  = reader["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedDate"])
            };
        }
    }
}

