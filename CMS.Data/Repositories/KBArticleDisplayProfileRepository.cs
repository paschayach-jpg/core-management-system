using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class KBArticleDisplayProfileRepository : IKBArticleDisplayProfileRepository
    {
        private readonly string _connectionString;

        public KBArticleDisplayProfileRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public IEnumerable<KBArticleDisplayProfile> GetAll()
        {
            var results = new List<KBArticleDisplayProfile>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT ProfileID, ProfileName, Description, IsActive, CreatedBy, CreatedDate
                    FROM   KMS.m_KBArticleDisplayProfile
                    ORDER BY ProfileName";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        results.Add(Map(reader));
                }
            }
            return results;
        }

        public KBArticleDisplayProfile GetById(int profileId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT ProfileID, ProfileName, Description, IsActive, CreatedBy, CreatedDate
                    FROM   KMS.m_KBArticleDisplayProfile
                    WHERE  ProfileID = @ProfileID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProfileID", profileId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return Map(reader);
                    }
                }
            }
            return null;
        }

        public bool ExistsByName(string profileName, int excludeProfileId = 0)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT TOP 1 1
                    FROM   KMS.m_KBArticleDisplayProfile
                    WHERE  ProfileName = @ProfileName AND ProfileID <> @ExcludeProfileID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProfileName", profileName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ExcludeProfileID", excludeProfileId);
                    return cmd.ExecuteScalar() != null;
                }
            }
        }

        public IList<KBArticleDisplayProfileColumn> GetColumnsByProfileId(int profileId)
        {
            var results = new List<KBArticleDisplayProfileColumn>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT ProfileColumnID, ProfileID, SlotKey, SlotDescription, DisplayLabel, IsVisible
                    FROM   KMS.m_KBArticleDisplayProfileColumn
                    WHERE  ProfileID = @ProfileID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProfileID", profileId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapColumn(reader));
                    }
                }
            }
            return results;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public int SaveProfileWithColumns(KBArticleDisplayProfile profile, IList<KBArticleDisplayProfileColumn> columns)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int profileId = profile.ProfileID > 0
                            ? UpdateProfile(conn, tx, profile)
                            : InsertProfile(conn, tx, profile);

                        const string deleteSql = "DELETE FROM KMS.m_KBArticleDisplayProfileColumn WHERE ProfileID = @ProfileID";
                        using (var cmd = new SqlCommand(deleteSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ProfileID", profileId);
                            cmd.ExecuteNonQuery();
                        }

                        const string insertSql = @"
                            INSERT INTO KMS.m_KBArticleDisplayProfileColumn
                                (ProfileID, SlotKey, SlotDescription, DisplayLabel, IsVisible)
                            VALUES
                                (@ProfileID, @SlotKey, @SlotDescription, @DisplayLabel, @IsVisible)";
                        foreach (var col in columns)
                        {
                            using (var cmd = new SqlCommand(insertSql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@ProfileID", profileId);
                                cmd.Parameters.AddWithValue("@SlotKey", col.SlotKey ?? string.Empty);
                                cmd.Parameters.AddWithValue("@SlotDescription", (object)col.SlotDescription ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@DisplayLabel", col.DisplayLabel ?? string.Empty);
                                cmd.Parameters.AddWithValue("@IsVisible", col.IsVisible);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return profileId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Deactivate(int profileId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "UPDATE KMS.m_KBArticleDisplayProfile SET IsActive = 0 WHERE ProfileID = @ProfileID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProfileID", profileId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Activate(int profileId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "UPDATE KMS.m_KBArticleDisplayProfile SET IsActive = 1 WHERE ProfileID = @ProfileID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProfileID", profileId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static int InsertProfile(SqlConnection conn, SqlTransaction tx, KBArticleDisplayProfile profile)
        {
            const string sql = @"
                INSERT INTO KMS.m_KBArticleDisplayProfile
                    (ProfileName, Description, IsActive, CreatedBy, CreatedDate)
                VALUES
                    (@ProfileName, @Description, 1, @CreatedBy, GETDATE());
                SELECT SCOPE_IDENTITY();";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@ProfileName", profile.ProfileName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Description", (object)profile.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", profile.CreatedBy ?? string.Empty);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static int UpdateProfile(SqlConnection conn, SqlTransaction tx, KBArticleDisplayProfile profile)
        {
            const string sql = @"
                UPDATE KMS.m_KBArticleDisplayProfile SET
                    ProfileName = @ProfileName,
                    Description = @Description
                WHERE ProfileID = @ProfileID";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@ProfileID", profile.ProfileID);
                cmd.Parameters.AddWithValue("@ProfileName", profile.ProfileName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Description", (object)profile.Description ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            return profile.ProfileID;
        }

        private static KBArticleDisplayProfile Map(IDataReader r)
        {
            return new KBArticleDisplayProfile
            {
                ProfileID   = r.GetInt32(r.GetOrdinal("ProfileID")),
                ProfileName = r["ProfileName"] as string,
                Description = r["Description"] as string,
                IsActive    = !r.IsDBNull(r.GetOrdinal("IsActive")) && r.GetBoolean(r.GetOrdinal("IsActive")),
                CreatedBy   = r["CreatedBy"] as string,
                CreatedDate = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate")),
            };
        }

        private static KBArticleDisplayProfileColumn MapColumn(IDataReader r)
        {
            return new KBArticleDisplayProfileColumn
            {
                ProfileColumnID = r.GetInt32(r.GetOrdinal("ProfileColumnID")),
                ProfileID       = r.GetInt32(r.GetOrdinal("ProfileID")),
                SlotKey         = r["SlotKey"] as string,
                SlotDescription = r["SlotDescription"] as string,
                DisplayLabel    = r["DisplayLabel"] as string,
                IsVisible       = !r.IsDBNull(r.GetOrdinal("IsVisible")) && r.GetBoolean(r.GetOrdinal("IsVisible")),
            };
        }
    }
}
