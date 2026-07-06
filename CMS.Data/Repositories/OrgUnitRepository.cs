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
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("EMS.sp_InsertOrgUnit", conn))
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
            catch (SqlException ex)
            {
                // Unique constraint (index) OR stored-procedure RAISERROR code-dup check
                if (ex.Number == 2627 || ex.Number == 2601 ||
                    ex.Message.IndexOf("มีอยู่ในระบบแล้ว", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "รหัสหน่วยงาน '{0}' มีอยู่ในระบบแล้ว กรุณาใช้รหัสอื่น", orgUnit.OrgUnitCode), ex);
                }
                throw;
            }
        }

        public void Update(OrgUnit orgUnit)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_UpdateOrgUnit", conn))
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

        public void UpdateTypeAndParent(int orgUnitId, int newType, int? newParentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    UPDATE EMS.OrgUnit
                    SET    OrgUnitType     = @NewType,
                           ParentOrgUnitID = @NewParentId
                    WHERE  OrgUnitID = @OrgUnitID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitID",   orgUnitId);
                    cmd.Parameters.AddWithValue("@NewType",      newType);
                    cmd.Parameters.AddWithValue("@NewParentId",  (object)newParentId ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public OrgUnit GetById(int orgUnitId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_GetOrgUnitByID", conn))
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

        public OrgUnit GetByCode(string orgUnitCode)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM EMS.OrgUnit WHERE OrgUnitCode = @OrgUnitCode AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitCode", orgUnitCode);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapOrgUnit(reader);
                        return null;
                    }
                }
            }
        }

        public bool ExistsByCode(string orgUnitCode)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM EMS.OrgUnit WHERE OrgUnitCode = @OrgUnitCode AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitCode", orgUnitCode);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public bool ExistsByCodeUnderParent(string orgUnitCode, int? parentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT COUNT(*) FROM EMS.OrgUnit
                    WHERE OrgUnitCode = @Code AND IsDeleted = 0
                      AND ((@ParentId IS NULL AND ParentOrgUnitID IS NULL)
                        OR ParentOrgUnitID = @ParentId)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Code", orgUnitCode);
                    cmd.Parameters.AddWithValue("@ParentId", (object)parentId ?? DBNull.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public bool ExistsByNameUnderParent(string nameTH, int? parentId, int? excludeOrgUnitId = null)
        {
            const string sql = @"
                SELECT COUNT(*) FROM EMS.OrgUnit
                WHERE OrgUnitNameTH = @OrgUnitNameTH
                  AND IsDeleted = 0
                  AND (
                        (@ParentOrgUnitID IS NULL AND ParentOrgUnitID IS NULL)
                     OR (ParentOrgUnitID = @ParentOrgUnitID)
                  )
                  AND (@ExcludeID IS NULL OR OrgUnitID <> @ExcludeID)";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitNameTH", nameTH);
                    cmd.Parameters.AddWithValue("@ParentOrgUnitID", (object)parentId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExcludeID",       (object)excludeOrgUnitId ?? DBNull.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public IEnumerable<OrgUnit> GetHierarchy()
        {
            var results = new List<OrgUnit>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_GetOrgUnitHierarchy", conn))
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
                using (var cmd = new SqlCommand("EMS.sp_GetOrgUnitByConfidentialityLevel", conn))
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
                using (var cmd = new SqlCommand("EMS.sp_HasActiveDependents", conn))
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
                using (var cmd = new SqlCommand("EMS.sp_SoftDeleteOrgUnit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    cmd.Parameters.AddWithValue("@DeletedBy", deletedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>Alias for SoftDelete — deactivates the OrgUnit without requiring a deletedBy user.</summary>
        public void Deactivate(int orgUnitId)
        {
            SoftDelete(orgUnitId, 0);
        }

        public void Activate(int orgUnitId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    UPDATE EMS.OrgUnit
                    SET    IsActive    = 1,
                           IsDeleted   = 0,
                           DeletedBy   = NULL,
                           DeletedDate = NULL
                    WHERE  OrgUnitID = @OrgUnitID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ForceDeleteWithDependents(int orgUnitId, int deletedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Soft-delete all active folders belonging to this OrgUnit
                        var cmdFolders = new SqlCommand(
                            @"UPDATE storage.Folder
                              SET    IsDeleted = 1, DeletedDate = GETDATE(), DeletedBy = @DeletedBy
                              WHERE  OrgUnitID = @OrgUnitID AND IsDeleted = 0", conn, tx);
                        cmdFolders.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                        cmdFolders.Parameters.AddWithValue("@DeletedBy", deletedBy);
                        cmdFolders.ExecuteNonQuery();

                        // 2. EMS.UserAccount.OrgUnitID เป็น NOT NULL — ไม่สามารถ set เป็น NULL ได้
                        // ผู้ใช้งานยังคง reference ไปยัง OrgUnit นี้ แต่เนื่องจาก OrgUnit
                        // จะถูก soft-delete (IsDeleted=1) ระบบจะ filter ออกเองเมื่อ query หน่วยงาน active
                        // บัญชีผู้ใช้งานยังคงใช้งานได้ปกติ

                        // 3. Soft-delete the OrgUnit itself
                        var cmdOrg = new SqlCommand("EMS.sp_SoftDeleteOrgUnit", conn, tx);
                        cmdOrg.CommandType = CommandType.StoredProcedure;
                        cmdOrg.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                        cmdOrg.Parameters.AddWithValue("@DeletedBy", deletedBy);
                        cmdOrg.ExecuteNonQuery();

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // ── Many-to-Many: SubCommittee ↔ Division ─────────────────────────────

        public void LinkDivisionToSubCommittee(int divisionId, int subCommitteeId, int createdBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                try
                {
                    using (var cmd = new SqlCommand("EMS.sp_AddOrgUnitRelationship", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SubCommitteeID", subCommitteeId);
                        cmd.Parameters.AddWithValue("@DivisionID",     divisionId);
                        cmd.Parameters.AddWithValue("@CreatedBy",      createdBy);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    // Backward-compat: some DBs have CreatedDate as NOT NULL without a default.
                    // Fallback to explicit upsert that always sets CreatedDate.
                    var isCreatedDateNullError =
                        ex.Message.IndexOf("CreatedDate", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        ex.Message.IndexOf("OrgUnitRelationship", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (!isCreatedDateNullError)
                        throw;

                    const string fallbackSql = @"
IF EXISTS (SELECT 1 FROM EMS.OrgUnitRelationship
           WHERE SubCommitteeID = @SubCommitteeID AND DivisionID = @DivisionID)
BEGIN
    UPDATE EMS.OrgUnitRelationship
    SET IsActive = 1,
        CreatedBy = @CreatedBy,
        CreatedDate = ISNULL(CreatedDate, GETDATE())
    WHERE SubCommitteeID = @SubCommitteeID AND DivisionID = @DivisionID;
END
ELSE
BEGIN
    INSERT INTO EMS.OrgUnitRelationship
        (SubCommitteeID, DivisionID, IsActive, CreatedBy, CreatedDate)
    VALUES
        (@SubCommitteeID, @DivisionID, 1, @CreatedBy, GETDATE());
END";

                    using (var fallbackCmd = new SqlCommand(fallbackSql, conn))
                    {
                        fallbackCmd.CommandType = CommandType.Text;
                        fallbackCmd.Parameters.AddWithValue("@SubCommitteeID", subCommitteeId);
                        fallbackCmd.Parameters.AddWithValue("@DivisionID", divisionId);
                        fallbackCmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                        fallbackCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public bool IsDivisionLinkedToSubCommittee(int divisionId, int subCommitteeId)
        {
            const string sql = @"
                SELECT COUNT(*) FROM EMS.OrgUnitRelationship
                WHERE DivisionID = @DivisionID AND SubCommitteeID = @SubCommitteeID AND IsActive = 1";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DivisionID",    divisionId);
                    cmd.Parameters.AddWithValue("@SubCommitteeID", subCommitteeId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public IEnumerable<OrgUnitRelationship> GetAllActiveRelationships()
        {
            const string sql = @"
                SELECT RelationshipID, SubCommitteeID, DivisionID, IsActive
                FROM EMS.OrgUnitRelationship WHERE IsActive = 1";
            var results = new List<OrgUnitRelationship>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new OrgUnitRelationship
                        {
                            RelationshipID = (int)reader["RelationshipID"],
                            SubCommitteeID = (int)reader["SubCommitteeID"],
                            DivisionID     = (int)reader["DivisionID"],
                            IsActive       = (bool)reader["IsActive"]
                        });
                    }
                }
            }
            return results;
        }

        // ── m_SubCommittee ─────────────────────────────────────────────────────

        public void InsertSubCommitteeRecord(int orgUnitId, string code, string nameTH, string nameEN, string createdByADUser)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_InsertSubCommittee", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID",          orgUnitId);
                    cmd.Parameters.AddWithValue("@SubCommitteeCode",   code);
                    cmd.Parameters.AddWithValue("@SubCommitteeNameTH", nameTH);
                    cmd.Parameters.AddWithValue("@SubCommitteeNameEN", (object)nameEN ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy",          createdByADUser);
                    cmd.ExecuteScalar();
                }
            }
        }

        public void SoftDeleteSubCommitteeRecord(int orgUnitId, string deletedByADUser)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_SoftDeleteSubCommittee", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID",  orgUnitId);
                    cmd.Parameters.AddWithValue("@DeletedBy",  deletedByADUser);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ── User view preferences ────────────────────────────────────────────────

        public IEnumerable<int> GetHiddenOrgUnitIds(int userId)
        {
            var results = new List<int>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_GetHiddenOrgUnitIds", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add((int)reader["OrgUnitId"]);
                    }
                }
            }
            return results;
        }

        public void SaveHiddenOrgUnitIds(int userId, IEnumerable<int> orgUnitIds)
        {
            var ids = orgUnitIds == null ? new List<int>() : new List<int>(orgUnitIds);
            string xmlIds = ids.Count > 0
                ? "<ids>" + string.Join("", System.Linq.Enumerable.Select(ids, i => "<id>" + i + "</id>")) + "</ids>"
                : null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_SaveHiddenOrgUnitIds", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@XmlIds", (object)xmlIds ?? DBNull.Value);
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
                DeletedDate     = reader["DeletedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DeletedDate"],
                CreatedBy       = (int)reader["CreatedBy"],
                CreatedDate     = (DateTime)reader["CreatedDate"]
            };
        }
    }
}

