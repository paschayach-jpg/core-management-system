using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class OrgUnitRepository : IOrgUnitRepository
    {
        private readonly string _connectionString;

        public OrgUnitRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(OrgUnit orgUnit)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_InsertOrgUnit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitCode",    orgUnit.OrgUnitCode);
                    cmd.Parameters.AddWithValue("@OrgUnitNameTH",  orgUnit.OrgUnitNameTH);
                    cmd.Parameters.AddWithValue("@OrgUnitNameEN",  (object)orgUnit.OrgUnitNameEN ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrgUnitType",    (byte)orgUnit.OrgUnitType);
                    cmd.Parameters.AddWithValue("@ParentOrgUnitID",(object)orgUnit.ParentOrgUnitID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SortOrder",      orgUnit.SortOrder);
                    cmd.Parameters.AddWithValue("@CreatedBy",      orgUnit.CreatedBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(OrgUnit orgUnit)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_UpdateOrgUnit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID",     orgUnit.OrgUnitID);
                    cmd.Parameters.AddWithValue("@OrgUnitNameTH", orgUnit.OrgUnitNameTH);
                    cmd.Parameters.AddWithValue("@OrgUnitNameEN", (object)orgUnit.OrgUnitNameEN ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SortOrder",     orgUnit.SortOrder);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public OrgUnit GetById(int orgUnitId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_GetOrgUnitByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapOrgUnit(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<OrgUnit> GetHierarchy()
        {
            var results = new List<OrgUnit>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_GetOrgUnitHierarchy", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapOrgUnit(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<OrgUnit> GetByConfidentialityLevel(ConfidentialityLevel level)
        {
            var results = new List<OrgUnit>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_GetOrgUnitByConfidentialityLevel", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ConfidentialityLevel", (int)level);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapOrgUnit(reader));
                    }
                }
            }
            return results;
        }

        public bool HasActiveDependents(int orgUnitId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_HasActiveDependents", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                }
            }
        }

        public void SoftDelete(int orgUnitId, int deletedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_SoftDeleteOrgUnit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    cmd.Parameters.AddWithValue("@DeletedBy", deletedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static OrgUnit MapOrgUnit(SqlDataReader reader)
        {
            return new OrgUnit
            {
                OrgUnitID       = (int)reader["OrgUnitID"],
                OrgUnitCode     = (string)reader["OrgUnitCode"],
                OrgUnitNameTH   = (string)reader["OrgUnitNameTH"],
                OrgUnitNameEN   = reader["OrgUnitNameEN"] == DBNull.Value ? null : (string)reader["OrgUnitNameEN"],
                OrgUnitType     = (byte)reader["OrgUnitType"],
                ParentOrgUnitID = reader["ParentOrgUnitID"] == DBNull.Value ? (int?)null : (int)reader["ParentOrgUnitID"],
                SortOrder       = (int)reader["SortOrder"],
                IsActive        = (bool)reader["IsActive"],
                IsDeleted       = (bool)reader["IsDeleted"],
                DeletedBy       = reader["DeletedBy"] == DBNull.Value ? (int?)null : (int)reader["DeletedBy"],
                CreatedBy       = (int)reader["CreatedBy"],
                CreatedDate     = (DateTime)reader["CreatedDate"]
            };
        }
    }
}
