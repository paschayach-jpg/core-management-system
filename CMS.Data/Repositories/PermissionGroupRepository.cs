using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class PermissionGroupRepository : IPermissionGroupRepository
    {
        private readonly string _connectionString;

        public PermissionGroupRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(PermissionGroup group)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_InsertPermissionGroup", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@GroupCode",   group.GroupCode);
                    cmd.Parameters.AddWithValue("@GroupName",   group.GroupName);
                    cmd.Parameters.AddWithValue("@Description", (object)group.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrgUnitID",   (object)group.OrgUnitID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy",   group.CreatedBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public PermissionGroup GetById(int groupId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_GetPermissionGroupByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@GroupID", groupId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapGroup(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<PermissionGroup> GetAll()
        {
            var results = new List<PermissionGroup>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_GetAllPermissionGroups", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapGroup(reader));
                    }
                }
            }
            return results;
        }

        public void Deactivate(int groupId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_DeactivatePermissionGroup", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@GroupID", groupId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddUserToGroup(int groupId, int userId, int addedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_AddUserToGroup", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@GroupID",  groupId);
                    cmd.Parameters.AddWithValue("@UserID",   userId);
                    cmd.Parameters.AddWithValue("@AddedBy",  addedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void RemoveUserFromGroup(int groupId, int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_RemoveUserFromGroup", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@GroupID", groupId);
                    cmd.Parameters.AddWithValue("@UserID",  userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<UserGroupMember> GetGroupsByUser(int userId)
        {
            var results = new List<UserGroupMember>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_GetGroupsByUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapMember(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<UserGroupMember> GetUsersByGroup(int groupId)
        {
            var results = new List<UserGroupMember>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("security.sp_GetUsersByGroup", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@GroupID", groupId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapMember(reader));
                    }
                }
            }
            return results;
        }

        private static PermissionGroup MapGroup(SqlDataReader reader)
        {
            return new PermissionGroup
            {
                GroupID     = (int)reader["GroupID"],
                GroupCode   = (string)reader["GroupCode"],
                GroupName   = (string)reader["GroupName"],
                Description = reader["Description"] == DBNull.Value ? null : (string)reader["Description"],
                OrgUnitID   = reader["OrgUnitID"] == DBNull.Value ? (int?)null : (int)reader["OrgUnitID"],
                IsActive    = (bool)reader["IsActive"],
                CreatedBy   = (int)reader["CreatedBy"],
                CreatedDate = (DateTime)reader["CreatedDate"]
            };
        }

        private static UserGroupMember MapMember(SqlDataReader reader)
        {
            return new UserGroupMember
            {
                MemberID   = (int)reader["MemberID"],
                GroupID    = (int)reader["GroupID"],
                UserID     = (int)reader["UserID"],
                JoinedDate = (DateTime)reader["JoinedDate"],
                AddedBy    = (int)reader["AddedBy"],
                IsActive   = (bool)reader["IsActive"]
            };
        }
    }
}
