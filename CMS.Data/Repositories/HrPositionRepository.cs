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
                    "FROM EMS.m_Role " +
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
                            pos.LevelCode = reader["Level"] == DBNull.Value ? "" : Convert.ToString(reader["Level"]);
                            results.Add(pos);
                        }
                    }
                }
            }
            return results;
        }

        public int Save(HrPosition position)
        {
            if (position == null) throw new ArgumentNullException("position");

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string levelValue = !string.IsNullOrEmpty(position.LevelCode) ? position.LevelCode : position.PositionLevel ?? "";

                if (position.PositionID > 0)
                {
                    using (var cmd = new SqlCommand(
                        "UPDATE EMS.m_Role " +
                        "SET PositionCode = @PositionCode, PositionName = @PositionName, " +
                        "    Level = @Level, IsActive = @IsActive " +
                        "WHERE PositionID = @PositionID", conn))
                    {
                        cmd.Parameters.AddWithValue("@PositionID", position.PositionID);
                        cmd.Parameters.AddWithValue("@PositionCode", position.PositionCode ?? "");
                        cmd.Parameters.AddWithValue("@PositionName", position.PositionName ?? "");
                        cmd.Parameters.AddWithValue("@Level", levelValue);
                        cmd.Parameters.AddWithValue("@IsActive", position.IsActive);
                        cmd.ExecuteNonQuery();
                        return position.PositionID;
                    }
                }
                else
                {
                    // Check by PositionName or PositionCode
                    using (var chkCmd = new SqlCommand(
                        "SELECT PositionID FROM EMS.m_Role WHERE PositionName = @PositionName OR (PositionCode <> '' AND PositionCode = @PositionCode)", conn))
                    {
                        chkCmd.Parameters.AddWithValue("@PositionName", position.PositionName ?? "");
                        chkCmd.Parameters.AddWithValue("@PositionCode", position.PositionCode ?? "");
                        var existingId = chkCmd.ExecuteScalar();
                        if (existingId != null && existingId != DBNull.Value)
                        {
                            int id = Convert.ToInt32(existingId);
                            using (var updateCmd = new SqlCommand(
                                "UPDATE EMS.m_Role " +
                                "SET PositionCode = @PositionCode, PositionName = @PositionName, " +
                                "    Level = @Level, IsActive = @IsActive " +
                                "WHERE PositionID = @PositionID", conn))
                            {
                                updateCmd.Parameters.AddWithValue("@PositionID", id);
                                updateCmd.Parameters.AddWithValue("@PositionCode", position.PositionCode ?? "");
                                updateCmd.Parameters.AddWithValue("@PositionName", position.PositionName ?? "");
                                updateCmd.Parameters.AddWithValue("@Level", levelValue);
                                updateCmd.Parameters.AddWithValue("@IsActive", position.IsActive);
                                updateCmd.ExecuteNonQuery();
                                return id;
                            }
                        }
                    }

                    using (var cmd = new SqlCommand(
                        "INSERT INTO EMS.m_Role (PositionCode, PositionName, Level, IsActive) " +
                        "VALUES (@PositionCode, @PositionName, @Level, @IsActive); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
                    {
                        cmd.Parameters.AddWithValue("@PositionCode", position.PositionCode ?? "");
                        cmd.Parameters.AddWithValue("@PositionName", position.PositionName ?? "");
                        cmd.Parameters.AddWithValue("@Level", levelValue);
                        cmd.Parameters.AddWithValue("@IsActive", position.IsActive);
                        var newId = cmd.ExecuteScalar();
                        return newId != null && newId != DBNull.Value ? Convert.ToInt32(newId) : 0;
                    }
                }
            }
        }

        public bool Delete(int positionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM EMS.m_Role WHERE PositionID = @PositionID", conn))
                {
                    cmd.Parameters.AddWithValue("@PositionID", positionId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
