using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    /// <summary>
    /// ดึงและจัดการ Level จาก master table: EMS.m_Position
    /// SELECT LevelCode, LevelName, LevelCategory, SortOrder FROM EMS.m_Position
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
                    "SELECT DISTINCT [LevelCode], [LevelName], [LevelCategory], [SortOrder] " +
                    "FROM EMS.m_Position " +
                    "WHERE [LevelCode] IS NOT NULL AND [LevelCode] <> '' " +
                    "ORDER BY [SortOrder], [LevelCode]", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        int seq = 1;
                        while (reader.Read())
                        {
                            var code = reader["LevelCode"] == DBNull.Value
                                ? string.Empty
                                : Convert.ToString(reader["LevelCode"]).Trim();

                            var name = reader["LevelName"] == DBNull.Value
                                ? string.Empty
                                : Convert.ToString(reader["LevelName"]).Trim();

                            var category = reader["LevelCategory"] == DBNull.Value
                                ? string.Empty
                                : Convert.ToString(reader["LevelCategory"]).Trim();

                            int? sortOrder = null;
                            if (reader["SortOrder"] != DBNull.Value)
                            {
                                int parsedSort;
                                if (int.TryParse(Convert.ToString(reader["SortOrder"]), out parsedSort))
                                    sortOrder = parsedSort;
                            }

                            results.Add(new HrPositionLevel
                            {
                                LevelID       = seq++,
                                LevelCode     = code,
                                LevelName     = name,
                                LevelCategory = category,
                                SortOrder     = sortOrder,
                                IsActive      = true
                            });
                        }
                    }
                }
            }
            return results;
        }

        public bool Save(HrPositionLevel level)
        {
            if (level == null || string.IsNullOrWhiteSpace(level.LevelCode)) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var chkCmd = new SqlCommand("SELECT COUNT(*) FROM EMS.m_Position WHERE LevelCode = @LevelCode", conn))
                {
                    chkCmd.Parameters.AddWithValue("@LevelCode", level.LevelCode.Trim());
                    int count = Convert.ToInt32(chkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        using (var updateCmd = new SqlCommand(
                            "UPDATE EMS.m_Position " +
                            "SET LevelName = @LevelName, LevelCategory = @LevelCategory, SortOrder = @SortOrder " +
                            "WHERE LevelCode = @LevelCode", conn))
                        {
                            updateCmd.Parameters.AddWithValue("@LevelCode", level.LevelCode.Trim());
                            updateCmd.Parameters.AddWithValue("@LevelName", level.LevelName ?? "");
                            updateCmd.Parameters.AddWithValue("@LevelCategory", level.LevelCategory ?? "");
                            updateCmd.Parameters.AddWithValue("@SortOrder", (object)level.SortOrder ?? DBNull.Value);
                            return updateCmd.ExecuteNonQuery() > 0;
                        }
                    }
                    else
                    {
                        using (var insertCmd = new SqlCommand(
                            "INSERT INTO EMS.m_Position (LevelCode, LevelName, LevelCategory, SortOrder) " +
                            "VALUES (@LevelCode, @LevelName, @LevelCategory, @SortOrder)", conn))
                        {
                            insertCmd.Parameters.AddWithValue("@LevelCode", level.LevelCode.Trim());
                            insertCmd.Parameters.AddWithValue("@LevelName", level.LevelName ?? "");
                            insertCmd.Parameters.AddWithValue("@LevelCategory", level.LevelCategory ?? "");
                            insertCmd.Parameters.AddWithValue("@SortOrder", (object)level.SortOrder ?? DBNull.Value);
                            return insertCmd.ExecuteNonQuery() > 0;
                        }
                    }
                }
            }
        }

        public bool Delete(string levelCode)
        {
            if (string.IsNullOrWhiteSpace(levelCode)) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM EMS.m_Position WHERE LevelCode = @LevelCode", conn))
                {
                    cmd.Parameters.AddWithValue("@LevelCode", levelCode.Trim());
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
