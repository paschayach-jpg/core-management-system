using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class ExternalShareRepository : IExternalShareRepository
    {
        private readonly string _connectionString;

        public ExternalShareRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(ExternalShare externalShare)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"INSERT INTO storage.ExternalShare 
                      (ShareToken, FileID, FolderID, SharerUserID, ApproverUserID, PasswordHash, 
                       ExpiryDate, DownloadLimit, CurrentDownloadCount, ShareTokenStatus, 
                       CreatedDate, SharePurpose, NotifyOnAccess)
                      VALUES 
                      (@ShareToken, @FileID, @FolderID, @SharerUserID, @ApproverUserID, @PasswordHash,
                       @ExpiryDate, @DownloadLimit, @CurrentDownloadCount, @ShareTokenStatus,
                       @CreatedDate, @SharePurpose, @NotifyOnAccess);
                      SELECT CAST(SCOPE_IDENTITY() as int)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ShareToken", externalShare.ShareToken);
                    cmd.Parameters.AddWithValue("@FileID", (object)externalShare.FileID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FolderID", (object)externalShare.FolderID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SharerUserID", externalShare.SharerUserID);
                    cmd.Parameters.AddWithValue("@ApproverUserID", (object)externalShare.ApproverUserID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PasswordHash", (object)externalShare.PasswordHash ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpiryDate", (object)externalShare.ExpiryDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DownloadLimit", (object)externalShare.DownloadLimit ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CurrentDownloadCount", externalShare.CurrentDownloadCount);
                    cmd.Parameters.AddWithValue("@ShareTokenStatus", externalShare.ShareTokenStatus);
                    cmd.Parameters.AddWithValue("@CreatedDate", externalShare.CreatedDate);
                    cmd.Parameters.AddWithValue("@SharePurpose", (object)externalShare.SharePurpose ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NotifyOnAccess", externalShare.NotifyOnAccess);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public ExternalShare GetById(int externalShareId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT ExternalShareID, ShareToken, FileID, FolderID, SharerUserID, ApproverUserID,
                             PasswordHash, ExpiryDate, DownloadLimit, CurrentDownloadCount, ShareTokenStatus,
                             CreatedDate, ApprovedDate, DisabledDate, DisabledReason, RevokedBy, RevokedDate,
                             RevocationReason, SharePurpose, NotifyOnAccess
                      FROM storage.ExternalShare
                      WHERE ExternalShareID = @ExternalShareID", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ExternalShareID", externalShareId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapExternalShare(reader);
                        return null;
                    }
                }
            }
        }

        public ExternalShare GetByToken(Guid shareToken)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT ExternalShareID, ShareToken, FileID, FolderID, SharerUserID, ApproverUserID,
                             PasswordHash, ExpiryDate, DownloadLimit, CurrentDownloadCount, ShareTokenStatus,
                             CreatedDate, ApprovedDate, DisabledDate, DisabledReason, RevokedBy, RevokedDate,
                             RevocationReason, SharePurpose, NotifyOnAccess
                      FROM storage.ExternalShare
                      WHERE ShareToken = @ShareToken", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ShareToken", shareToken);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapExternalShare(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<ExternalShare> GetBySharer(int sharerUserId)
        {
            var results = new List<ExternalShare>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT ExternalShareID, ShareToken, FileID, FolderID, SharerUserID, ApproverUserID,
                             PasswordHash, ExpiryDate, DownloadLimit, CurrentDownloadCount, ShareTokenStatus,
                             CreatedDate, ApprovedDate, DisabledDate, DisabledReason, RevokedBy, RevokedDate,
                             RevocationReason, SharePurpose, NotifyOnAccess
                      FROM storage.ExternalShare
                      WHERE SharerUserID = @SharerUserID
                      ORDER BY CreatedDate DESC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SharerUserID", sharerUserId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapExternalShare(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<ExternalShare> GetByStatus(string status)
        {
            var results = new List<ExternalShare>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT ExternalShareID, ShareToken, FileID, FolderID, SharerUserID, ApproverUserID,
                             PasswordHash, ExpiryDate, DownloadLimit, CurrentDownloadCount, ShareTokenStatus,
                             CreatedDate, ApprovedDate, DisabledDate, DisabledReason, RevokedBy, RevokedDate,
                             RevocationReason, SharePurpose, NotifyOnAccess
                      FROM storage.ExternalShare
                      WHERE ShareTokenStatus = @Status
                      ORDER BY CreatedDate DESC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Status", status);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapExternalShare(reader));
                    }
                }
            }
            return results;
        }

        public void Update(ExternalShare externalShare)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"UPDATE storage.ExternalShare
                      SET ShareToken = @ShareToken,
                          FileID = @FileID,
                          FolderID = @FolderID,
                          SharerUserID = @SharerUserID,
                          ApproverUserID = @ApproverUserID,
                          PasswordHash = @PasswordHash,
                          ExpiryDate = @ExpiryDate,
                          DownloadLimit = @DownloadLimit,
                          CurrentDownloadCount = @CurrentDownloadCount,
                          ShareTokenStatus = @ShareTokenStatus,
                          ApprovedDate = @ApprovedDate,
                          DisabledDate = @DisabledDate,
                          DisabledReason = @DisabledReason,
                          RevokedBy = @RevokedBy,
                          RevokedDate = @RevokedDate,
                          RevocationReason = @RevocationReason,
                          SharePurpose = @SharePurpose,
                          NotifyOnAccess = @NotifyOnAccess
                      WHERE ExternalShareID = @ExternalShareID", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ExternalShareID", externalShare.ExternalShareID);
                    cmd.Parameters.AddWithValue("@ShareToken", externalShare.ShareToken);
                    cmd.Parameters.AddWithValue("@FileID", (object)externalShare.FileID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FolderID", (object)externalShare.FolderID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SharerUserID", externalShare.SharerUserID);
                    cmd.Parameters.AddWithValue("@ApproverUserID", (object)externalShare.ApproverUserID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PasswordHash", (object)externalShare.PasswordHash ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpiryDate", (object)externalShare.ExpiryDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DownloadLimit", (object)externalShare.DownloadLimit ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CurrentDownloadCount", externalShare.CurrentDownloadCount);
                    cmd.Parameters.AddWithValue("@ShareTokenStatus", externalShare.ShareTokenStatus);
                    cmd.Parameters.AddWithValue("@ApprovedDate", (object)externalShare.ApprovedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisabledDate", (object)externalShare.DisabledDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisabledReason", (object)externalShare.DisabledReason ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RevokedBy", (object)externalShare.RevokedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RevokedDate", (object)externalShare.RevokedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RevocationReason", (object)externalShare.RevocationReason ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SharePurpose", (object)externalShare.SharePurpose ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NotifyOnAccess", externalShare.NotifyOnAccess);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<ExternalShare> GetAll()
        {
            var results = new List<ExternalShare>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT ExternalShareID, ShareToken, FileID, FolderID, SharerUserID, ApproverUserID,
                             PasswordHash, ExpiryDate, DownloadLimit, CurrentDownloadCount, ShareTokenStatus,
                             CreatedDate, ApprovedDate, DisabledDate, DisabledReason, RevokedBy, RevokedDate,
                             RevocationReason, SharePurpose, NotifyOnAccess
                      FROM storage.ExternalShare
                      ORDER BY CreatedDate DESC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapExternalShare(reader));
                    }
                }
            }
            return results;
        }

        private static ExternalShare MapExternalShare(SqlDataReader reader)
        {
            return new ExternalShare
            {
                ExternalShareID = (int)reader["ExternalShareID"],
                ShareToken = (Guid)reader["ShareToken"],
                FileID = reader["FileID"] == DBNull.Value ? (int?)null : (int)reader["FileID"],
                FolderID = reader["FolderID"] == DBNull.Value ? (int?)null : (int)reader["FolderID"],
                SharerUserID = (int)reader["SharerUserID"],
                ApproverUserID = reader["ApproverUserID"] == DBNull.Value ? (int?)null : (int)reader["ApproverUserID"],
                PasswordHash = reader["PasswordHash"] == DBNull.Value ? null : (string)reader["PasswordHash"],
                ExpiryDate = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ExpiryDate"],
                DownloadLimit = reader["DownloadLimit"] == DBNull.Value ? (int?)null : (int)reader["DownloadLimit"],
                CurrentDownloadCount = (int)reader["CurrentDownloadCount"],
                ShareTokenStatus = (string)reader["ShareTokenStatus"],
                CreatedDate = (DateTime)reader["CreatedDate"],
                ApprovedDate = reader["ApprovedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ApprovedDate"],
                DisabledDate = reader["DisabledDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DisabledDate"],
                DisabledReason = reader["DisabledReason"] == DBNull.Value ? null : (string)reader["DisabledReason"],
                RevokedBy = reader["RevokedBy"] == DBNull.Value ? (int?)null : (int)reader["RevokedBy"],
                RevokedDate = reader["RevokedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["RevokedDate"],
                RevocationReason = reader["RevocationReason"] == DBNull.Value ? null : (string)reader["RevocationReason"],
                SharePurpose = reader["SharePurpose"] == DBNull.Value ? null : (string)reader["SharePurpose"],
                NotifyOnAccess = (bool)reader["NotifyOnAccess"]
            };
        }
    }
}
