using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class ShareAccessLogRepository : IShareAccessLogRepository
    {
        private readonly string _connectionString;

        public ShareAccessLogRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        /// <summary>
        /// Inserts an access log entry (append-only).
        /// </summary>
        public long Insert(ShareAccessLog accessLog)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"INSERT INTO storage.ShareAccessLog 
                      (ShareToken, ExternalShareID, RecipientIdentifier, AccessType, IsSuccessful,
                       FailureReason, FileSizeBytes, AccessTimestamp, IPAddress, UserAgent)
                      VALUES 
                      (@ShareToken, @ExternalShareID, @RecipientIdentifier, @AccessType, @IsSuccessful,
                       @FailureReason, @FileSizeBytes, @AccessTimestamp, @IPAddress, @UserAgent);
                      SELECT CAST(SCOPE_IDENTITY() as bigint)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ShareToken", accessLog.ShareToken);
                    cmd.Parameters.AddWithValue("@ExternalShareID", accessLog.ExternalShareID);
                    cmd.Parameters.AddWithValue("@RecipientIdentifier", (object)accessLog.RecipientIdentifier ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccessType", accessLog.AccessType);
                    cmd.Parameters.AddWithValue("@IsSuccessful", accessLog.IsSuccessful);
                    cmd.Parameters.AddWithValue("@FailureReason", (object)accessLog.FailureReason ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileSizeBytes", (object)accessLog.FileSizeBytes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccessTimestamp", accessLog.AccessTimestamp);
                    cmd.Parameters.AddWithValue("@IPAddress", (object)accessLog.IPAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserAgent", (object)accessLog.UserAgent ?? DBNull.Value);
                    return Convert.ToInt64(cmd.ExecuteScalar());
                }
            }
        }

        public IEnumerable<ShareAccessLog> GetByExternalShare(int externalShareId)
        {
            var results = new List<ShareAccessLog>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT AccessLogID, ShareToken, ExternalShareID, RecipientIdentifier, AccessType,
                             IsSuccessful, FailureReason, FileSizeBytes, AccessTimestamp, IPAddress, UserAgent
                      FROM storage.ShareAccessLog
                      WHERE ExternalShareID = @ExternalShareID
                      ORDER BY AccessTimestamp ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ExternalShareID", externalShareId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapShareAccessLog(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<ShareAccessLog> GetByToken(Guid shareToken)
        {
            var results = new List<ShareAccessLog>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT AccessLogID, ShareToken, ExternalShareID, RecipientIdentifier, AccessType,
                             IsSuccessful, FailureReason, FileSizeBytes, AccessTimestamp, IPAddress, UserAgent
                      FROM storage.ShareAccessLog
                      WHERE ShareToken = @ShareToken
                      ORDER BY AccessTimestamp ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ShareToken", shareToken);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapShareAccessLog(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<ShareAccessLog> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            var results = new List<ShareAccessLog>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT AccessLogID, ShareToken, ExternalShareID, RecipientIdentifier, AccessType,
                             IsSuccessful, FailureReason, FileSizeBytes, AccessTimestamp, IPAddress, UserAgent
                      FROM storage.ShareAccessLog
                      WHERE AccessTimestamp >= @StartDate AND AccessTimestamp <= @EndDate
                      ORDER BY AccessTimestamp ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapShareAccessLog(reader));
                    }
                }
            }
            return results;
        }

        private static ShareAccessLog MapShareAccessLog(SqlDataReader reader)
        {
            return new ShareAccessLog
            {
                AccessLogID = (long)reader["AccessLogID"],
                ShareToken = (Guid)reader["ShareToken"],
                ExternalShareID = (int)reader["ExternalShareID"],
                RecipientIdentifier = reader["RecipientIdentifier"] == DBNull.Value ? null : (string)reader["RecipientIdentifier"],
                AccessType = (string)reader["AccessType"],
                IsSuccessful = (bool)reader["IsSuccessful"],
                FailureReason = reader["FailureReason"] == DBNull.Value ? null : (string)reader["FailureReason"],
                FileSizeBytes = reader["FileSizeBytes"] == DBNull.Value ? (long?)null : (long)reader["FileSizeBytes"],
                AccessTimestamp = (DateTime)reader["AccessTimestamp"],
                IPAddress = reader["IPAddress"] == DBNull.Value ? null : (string)reader["IPAddress"],
                UserAgent = reader["UserAgent"] == DBNull.Value ? null : (string)reader["UserAgent"]
            };
        }
    }
}
