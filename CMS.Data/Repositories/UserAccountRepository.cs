using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        public UserAccountRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public UserAccount GetByADUserName(string adUserName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_GetUserAccountByADUserName", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ADUserName", adUserName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapUserAccount(reader);
                        return null;
                    }
                }
            }
        }

        public UserAccount GetById(int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_GetUserAccountByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapUserAccount(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<UserAccount> GetByRole(int appRole)
        {
            var results = new List<UserAccount>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_GetUserAccountsByRole", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AppRole", (byte)appRole);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapUserAccount(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<UserAccount> GetAll()
        {
            var results = new List<UserAccount>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_GetAllUserAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapUserAccount(reader));
                    }
                }
            }
            return results;
        }

        public int Create(UserAccount account)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_InsertUserAccount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ADUserName",  account.ADUserName);
                    cmd.Parameters.AddWithValue("@DisplayName", account.DisplayName);
                    cmd.Parameters.AddWithValue("@Email",       (object)account.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppRole",     (byte)account.AppRole);
                    cmd.Parameters.AddWithValue("@OrgUnitID",   account.OrgUnitID);
                    cmd.Parameters.AddWithValue("@CreatedBy",   (object)account.CreatedBy ?? DBNull.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(UserAccount account)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_UpdateUserAccount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID",      account.UserID);
                    cmd.Parameters.AddWithValue("@DisplayName", account.DisplayName);
                    cmd.Parameters.AddWithValue("@AppRole",     (byte)account.AppRole);
                    cmd.Parameters.AddWithValue("@OrgUnitID",   account.OrgUnitID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateLastLogin(int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_UpdateLastLogin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateConsent(int userId, string email, bool consentGiven)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_UpdateUserConsent", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID",       userId);
                    cmd.Parameters.AddWithValue("@Email",        (object)email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ConsentGiven", consentGiven);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Deactivate(int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_DeactivateUserAccount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static UserAccount MapUserAccount(SqlDataReader reader)
        {
            return new UserAccount
            {
                UserID        = (int)reader["UserID"],
                ADUserName    = (string)reader["ADUserName"],
                DisplayName   = (string)reader["DisplayName"],
                Email         = reader["Email"] == DBNull.Value ? null : (string)reader["Email"],
                AppRole       = (byte)reader["AppRole"],
                OrgUnitID     = (int)reader["OrgUnitID"],
                ConsentGiven  = (bool)reader["ConsentGiven"],
                ConsentDate   = reader["ConsentDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ConsentDate"],
                IsActive      = (bool)reader["IsActive"],
                LastLoginDate = reader["LastLoginDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["LastLoginDate"],
                CreatedBy     = reader["CreatedBy"] == DBNull.Value ? (int?)null : (int)reader["CreatedBy"],
                CreatedDate   = (DateTime)reader["CreatedDate"]
            };
        }
    }
}
