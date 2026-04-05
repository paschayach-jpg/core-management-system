using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    /// <summary>
    /// ดึง Level ที่ unique จาก core.HR_Position.Level
    /// (ไม่มีตาราง HR_PositionLevel แยกต่างหาก)
    /// </summary>
    public class HrPositionLevelRepository : IHrPositionLevelRepository
    {
        private readonly string _connectionString;

        public HrPositionLevelRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<HrPositionLevel> GetAll()
        {
            var results = new List<HrPositionLevel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT DISTINCT Level AS LevelCode " +
                    "FROM core.HR_Position " +
                    "WHERE IsActive = 1 AND Level IS NOT NULL AND Level <> '' " +
                    "ORDER BY Level", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        int seq = 1;
                        while (reader.Read())
                        {
                            var code = reader["LevelCode"] == DBNull.Value
                                ? string.Empty
                                : Convert.ToString(reader["LevelCode"]);
                            results.Add(new HrPositionLevel
                            {
                                LevelID   = seq++,
                                LevelCode = code,
                                LevelName = code,
                                IsActive  = true
                            });
                        }
                    }
                }
            }
            return results;
        }
    }
}
