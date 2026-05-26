using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class HrPositionRepository : IHrPositionRepository
    {
        private readonly string _connectionString;

        public HrPositionRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<HrPosition> GetAll()
        {
            var results = new List<HrPosition>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT PositionID, PositionCode, PositionName, " +
                    "       ISNULL(Level,'') AS Level, IsActive " +
                    "FROM core.HR_Role " +
                    "WHERE IsActive = 1 " +
                    "ORDER BY PositionName", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pos = new HrPosition
                            {
                                PositionID   = Convert.ToInt32(reader["PositionID"]),
                                PositionCode = reader["PositionCode"] == DBNull.Value ? "" : Convert.ToString(reader["PositionCode"]),
                                PositionName = reader["PositionName"] == DBNull.Value ? "" : Convert.ToString(reader["PositionName"]),
                                IsActive     = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"])
                            };
                            // Level เป็น alias ที่ set LevelCode
                            pos.LevelCode = reader["Level"] == DBNull.Value ? "" : Convert.ToString(reader["Level"]);
                            results.Add(pos);
                        }
                    }
                }
            }
            return results;
        }
    }
}
