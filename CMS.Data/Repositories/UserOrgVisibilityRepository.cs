using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class UserOrgVisibilityRepository : IUserOrgVisibilityRepository
    {
        private readonly string _connectionString;

        public UserOrgVisibilityRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<UserOrgVisibility> GetByUser(int userId)
        {
            var results = new List<UserOrgVisibility>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT UserID, OrgUnitID, GrantedBy, GrantedDate
                    FROM security.UserOrgVisibility
                    WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapRecord(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<UserOrgVisibility> GetByOrgUnit(int orgUnitId)
        {
            var results = new List<UserOrgVisibility>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT UserID, OrgUnitID, GrantedBy, GrantedDate
                    FROM security.UserOrgVisibility
                    WHERE OrgUnitID = @OrgUnitID", conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapRecord(reader));
                    }
                }
            }
            return results;
        }

        /// <summary>Upsert — calls MERGE stored procedure; no duplicate key exception.</summary>
        public void Grant(UserOrgVisibility record)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
IF EXISTS (
    SELECT 1
    FROM security.UserOrgVisibility
    WHERE UserID = @UserID AND OrgUnitID = @OrgUnitID
)
BEGIN
    UPDATE security.UserOrgVisibility
    SET GrantedBy = @GrantedBy,
        GrantedDate = GETDATE()
    WHERE UserID = @UserID AND OrgUnitID = @OrgUnitID;
END
ELSE
BEGIN
    INSERT INTO security.UserOrgVisibility (UserID, OrgUnitID, GrantedBy, GrantedDate)
    VALUES (@UserID, @OrgUnitID, @GrantedBy, GETDATE());
END", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID",    record.UserID);
                    cmd.Parameters.AddWithValue("@OrgUnitID", record.OrgUnitID);
                    cmd.Parameters.AddWithValue("@GrantedBy", record.GrantedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>Silent no-op if the row does not exist.</summary>
        public void Revoke(int userId, int orgUnitId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    DELETE FROM security.UserOrgVisibility
                    WHERE UserID = @UserID AND OrgUnitID = @OrgUnitID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID",    userId);
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static UserOrgVisibility MapRecord(SqlDataReader reader)
        {
            return new UserOrgVisibility
            {
                UserID      = (int)reader["UserID"],
                OrgUnitID   = (int)reader["OrgUnitID"],
                GrantedBy   = (int)reader["GrantedBy"],
                GrantedDate = (DateTime)reader["GrantedDate"]
            };
        }
    }
}
