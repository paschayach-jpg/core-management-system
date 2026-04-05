using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class ShareTokenRepository : IShareTokenRepository
    {
        private readonly string _connectionString;

        public ShareTokenRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(ShareToken shareToken)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"INSERT INTO storage.ShareToken 
                      (TokenValue, ExternalShareID, Status, CreatedDate, ActivatedDate, ExpiryDate,
                       DisabledDate, DisabledByUserID, DisabledReason)
                      VALUES 
                      (@TokenValue, @ExternalShareID, @Status, @CreatedDate, @ActivatedDate, @ExpiryDate,
                       @DisabledDate, @DisabledByUserID, @DisabledReason);
                      SELECT CAST(SCOPE_IDENTITY() as int)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@TokenValue", shareToken.TokenValue);
                    cmd.Parameters.AddWithValue("@ExternalShareID", shareToken.ExternalShareID);
                    cmd.Parameters.AddWithValue("@Status", shareToken.Status);
                    cmd.Parameters.AddWithValue("@CreatedDate", shareToken.CreatedDate);
                    cmd.Parameters.AddWithValue("@ActivatedDate", (object)shareToken.ActivatedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpiryDate", (object)shareToken.ExpiryDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisabledDate", (object)shareToken.DisabledDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisabledByUserID", (object)shareToken.DisabledByUserID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisabledReason", (object)shareToken.DisabledReason ?? DBNull.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public ShareToken GetById(int shareTokenId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT ShareTokenID, TokenValue, ExternalShareID, Status, CreatedDate, ActivatedDate,
                             ExpiryDate, DisabledDate, DisabledByUserID, DisabledReason
                      FROM storage.ShareToken
                      WHERE ShareTokenID = @ShareTokenID", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ShareTokenID", shareTokenId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapShareToken(reader);
                        return null;
                    }
                }
            }
        }

        public ShareToken GetByTokenValue(Guid tokenValue)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT ShareTokenID, TokenValue, ExternalShareID, Status, CreatedDate, ActivatedDate,
                             ExpiryDate, DisabledDate, DisabledByUserID, DisabledReason
                      FROM storage.ShareToken
                      WHERE TokenValue = @TokenValue", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@TokenValue", tokenValue);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapShareToken(reader);
                        return null;
                    }
                }
            }
        }

        public void Update(ShareToken shareToken)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"UPDATE storage.ShareToken
                      SET TokenValue = @TokenValue,
                          ExternalShareID = @ExternalShareID,
                          Status = @Status,
                          CreatedDate = @CreatedDate,
                          ActivatedDate = @ActivatedDate,
                          ExpiryDate = @ExpiryDate,
                          DisabledDate = @DisabledDate,
                          DisabledByUserID = @DisabledByUserID,
                          DisabledReason = @DisabledReason
                      WHERE ShareTokenID = @ShareTokenID", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ShareTokenID", shareToken.ShareTokenID);
                    cmd.Parameters.AddWithValue("@TokenValue", shareToken.TokenValue);
                    cmd.Parameters.AddWithValue("@ExternalShareID", shareToken.ExternalShareID);
                    cmd.Parameters.AddWithValue("@Status", shareToken.Status);
                    cmd.Parameters.AddWithValue("@CreatedDate", shareToken.CreatedDate);
                    cmd.Parameters.AddWithValue("@ActivatedDate", (object)shareToken.ActivatedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpiryDate", (object)shareToken.ExpiryDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisabledDate", (object)shareToken.DisabledDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisabledByUserID", (object)shareToken.DisabledByUserID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisabledReason", (object)shareToken.DisabledReason ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<ShareToken> GetByExternalShare(int externalShareId)
        {
            var results = new List<ShareToken>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT ShareTokenID, TokenValue, ExternalShareID, Status, CreatedDate, ActivatedDate,
                             ExpiryDate, DisabledDate, DisabledByUserID, DisabledReason
                      FROM storage.ShareToken
                      WHERE ExternalShareID = @ExternalShareID
                      ORDER BY CreatedDate DESC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ExternalShareID", externalShareId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapShareToken(reader));
                    }
                }
            }
            return results;
        }

        private static ShareToken MapShareToken(SqlDataReader reader)
        {
            return new ShareToken
            {
                ShareTokenID = (int)reader["ShareTokenID"],
                TokenValue = (Guid)reader["TokenValue"],
                ExternalShareID = (int)reader["ExternalShareID"],
                Status = (string)reader["Status"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                ActivatedDate = reader["ActivatedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ActivatedDate"],
                ExpiryDate = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ExpiryDate"],
                DisabledDate = reader["DisabledDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DisabledDate"],
                DisabledByUserID = reader["DisabledByUserID"] == DBNull.Value ? (int?)null : (int)reader["DisabledByUserID"],
                DisabledReason = reader["DisabledReason"] == DBNull.Value ? null : (string)reader["DisabledReason"]
            };
        }
    }
}
