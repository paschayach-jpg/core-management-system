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
                using (var cmd = new SqlCommand(@"
                    UPDATE core.UserAccount SET
                        DisplayName     = @DisplayName,
                        AppRole         = @AppRole,
                        OrgUnitID       = @OrgUnitID,
                        EmployeeCode    = @EmployeeCode,
                        DivisionId      = @DivisionId,
                        DivisionCode    = @DivisionCode,
                        DivisionName    = @DivisionName,
                        DepartmentId    = @DepartmentId,
                        DepartmentCode  = @DepartmentCode,
                        DepartmentName  = @DepartmentName,
                        PositionName    = @PositionName,
                        LevelCode       = @LevelCode,
                        UpdatedAt       = GETUTCDATE()
                    WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID",        account.UserID);
                    cmd.Parameters.AddWithValue("@DisplayName",   (object)account.DisplayName  ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppRole",       (byte)account.AppRole);
                    cmd.Parameters.AddWithValue("@OrgUnitID",     account.OrgUnitID);
                    cmd.Parameters.AddWithValue("@EmployeeCode",  (object)account.EmployeeCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DivisionId",    (object)account.DivisionId   ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DivisionCode",  (object)account.DivisionCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DivisionName",  (object)account.DivisionName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentId",  (object)account.DepartmentId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentCode",(object)account.DepartmentCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentName",(object)account.DepartmentName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PositionName",  (object)account.PositionName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LevelCode",     (object)account.LevelCode    ?? DBNull.Value);
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

        /// <summary>
        /// Update LastLoginDate + LastLoginIP ในครั้งเดียว
        /// </summary>
        public void UpdateLastLoginWithIP(int userId, string ipAddress)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE core.UserAccount
                    SET    LastLoginDate = GETUTCDATE(),
                           LastLoginIP   = @IPAddress,
                           UpdatedAt     = GETUTCDATE()
                    WHERE  UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID",    userId);
                    cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Upsert: ถ้ามี ADUserName อยู่แล้ว → Update ข้อมูล + IP
        ///         ถ้าไม่มี → Insert ใหม่
        /// คืน UserID ผ่าน return value, isNew ผ่าน out parameter
        /// </summary>
        public int RegisterOrUpdate(UserAccount account, string ipAddress, out bool isNew)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // ตรวจว่ามีอยู่แล้วหรือไม่
                int existingId = 0;
                using (var chk = new SqlCommand(
                    "SELECT UserID FROM core.UserAccount WHERE ADUserName = @ADUserName", conn))
                {
                    chk.Parameters.AddWithValue("@ADUserName", account.ADUserName);
                    var result = chk.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        existingId = Convert.ToInt32(result);
                }

                if (existingId > 0)
                {
                    // Update: DisplayName, Email, EmployeeCode, IP, UpdatedAt
                    using (var upd = new SqlCommand(@"
                        UPDATE core.UserAccount SET
                            DisplayName   = @DisplayName,
                            Email         = @Email,
                            EmployeeCode  = @EmployeeCode,
                            LastLoginDate = GETUTCDATE(),
                            LastLoginIP   = @IPAddress,
                            UpdatedAt     = GETUTCDATE()
                        WHERE UserID = @UserID", conn))
                    {
                        upd.Parameters.AddWithValue("@UserID",       existingId);
                        upd.Parameters.AddWithValue("@DisplayName",  account.DisplayName ?? account.ADUserName);
                        upd.Parameters.AddWithValue("@Email",        (object)account.Email        ?? DBNull.Value);
                        upd.Parameters.AddWithValue("@EmployeeCode", (object)account.EmployeeCode ?? DBNull.Value);
                        upd.Parameters.AddWithValue("@IPAddress",    (object)ipAddress            ?? DBNull.Value);
                        upd.ExecuteNonQuery();
                    }
                    isNew = false;
                    return existingId;
                }
                else
                {
                    // Insert ใหม่
                    using (var ins = new SqlCommand(@"
                        INSERT INTO core.UserAccount
                            (ADUserName, DisplayName, Email, AppRole, OrgUnitID,
                             EmployeeCode, IsActive, ConsentGiven,
                             LastLoginDate, LastLoginIP, CreatedDate, UpdatedAt)
                        VALUES
                            (@ADUserName, @DisplayName, @Email, @AppRole, @OrgUnitID,
                             @EmployeeCode, 1, 0,
                             GETUTCDATE(), @IPAddress, GETUTCDATE(), GETUTCDATE());
                        SELECT SCOPE_IDENTITY();", conn))
                    {
                        ins.Parameters.AddWithValue("@ADUserName",   account.ADUserName);
                        ins.Parameters.AddWithValue("@DisplayName",  account.DisplayName ?? account.ADUserName);
                        ins.Parameters.AddWithValue("@Email",        (object)account.Email        ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@AppRole",      (byte)(account.AppRole > 0 ? account.AppRole : 1));
                        ins.Parameters.AddWithValue("@OrgUnitID",    account.OrgUnitID > 0 ? account.OrgUnitID : 1);
                        ins.Parameters.AddWithValue("@EmployeeCode", (object)account.EmployeeCode ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@IPAddress",    (object)ipAddress            ?? DBNull.Value);
                        isNew = true;
                        return Convert.ToInt32(ins.ExecuteScalar());
                    }
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

        public IEnumerable<UserAccount> GetByDepartmentId(int departmentId)
        {
            var result = new List<UserAccount>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT * FROM core.UserAccount WHERE DepartmentId = @DepartmentId AND IsActive = 1", conn))
                {
                    cmd.Parameters.AddWithValue("@DepartmentId", departmentId);
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            result.Add(MapUserAccount(reader));
                }
            }
            return result;
        }

        public IEnumerable<string> GetPositionsByDepartmentCode(string departmentCode)
        {
            var result = new List<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT DISTINCT PositionName " +
                    "FROM core.UserAccount " +
                    "WHERE DepartmentCode = @DepartmentCode " +
                    "  AND IsActive = 1 " +
                    "  AND PositionName IS NOT NULL AND PositionName <> '' " +
                    "ORDER BY PositionName", conn))
                {
                    cmd.Parameters.AddWithValue("@DepartmentCode", departmentCode);
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            result.Add(Convert.ToString(reader["PositionName"]));
                }
            }
            return result;
        }

        public IEnumerable<UserAccount> GetByDivisionId(int divisionId)
        {
            var result = new List<UserAccount>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT * FROM core.UserAccount WHERE DivisionId = @DivisionId AND IsActive = 1", conn))
                {
                    cmd.Parameters.AddWithValue("@DivisionId", divisionId);
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            result.Add(MapUserAccount(reader));
                }
            }
            return result;
        }

        public void UpdateEmployeeInfo(int userId, string employeeCode,
            string titleNameEn, string firstNameEn, string lastNameEn,
            string titleNameTh, string firstNameTh, string lastNameTh,
            int? departmentId, string departmentCode, string departmentNameEn, string departmentNameTh,
            int? divisionId,   string divisionCode,   string divisionNameEn,   string divisionNameTh,
            string positionName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE core.UserAccount SET
                        EmployeeCode        = @EmployeeCode,
                        EmployeeTitleNameEn = @TitleNameEn,
                        EmployeeFirstNameEn = @FirstNameEn,
                        EmployeeLastNameEn  = @LastNameEn,
                        EmployeeTitleNameTh = @TitleNameTh,
                        EmployeeFirstNameTh = @FirstNameTh,
                        EmployeeLastNameTh  = @LastNameTh,
                        DepartmentId        = @DepartmentId,
                        DepartmentCode      = @DepartmentCode,
                        DepartmentNameEn    = @DepartmentNameEn,
                        DepartmentName      = @DepartmentNameTh,
                        DivisionId          = @DivisionId,
                        DivisionCode        = @DivisionCode,
                        DivisionNameEn      = @DivisionNameEn,
                        DivisionName        = @DivisionNameTh,
                        PositionName        = @PositionName,
                        UpdatedAt           = GETUTCDATE()
                    WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID",          userId);
                    cmd.Parameters.AddWithValue("@EmployeeCode",    (object)employeeCode    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TitleNameEn",     (object)titleNameEn     ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FirstNameEn",     (object)firstNameEn     ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastNameEn",      (object)lastNameEn      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TitleNameTh",     (object)titleNameTh     ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FirstNameTh",     (object)firstNameTh     ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastNameTh",      (object)lastNameTh      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentId",    (object)departmentId    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentCode",  (object)departmentCode  ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentNameEn",(object)departmentNameEn?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentNameTh",(object)departmentNameTh?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DivisionId",      (object)divisionId      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DivisionCode",    (object)divisionCode    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DivisionNameEn",  (object)divisionNameEn  ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DivisionNameTh",  (object)divisionNameTh  ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PositionName",    (object)positionName    ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static UserAccount MapUserAccount(SqlDataReader reader)
        {
            var ua = new UserAccount
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

            // Employee API fields (nullable — columns may not exist in older DB)
            try { ua.EmployeeCode        = reader["EmployeeCode"]        == DBNull.Value ? null : (string)reader["EmployeeCode"];        } catch { }
            try { ua.EmployeeTitleNameEn = reader["EmployeeTitleNameEn"] == DBNull.Value ? null : (string)reader["EmployeeTitleNameEn"]; } catch { }
            try { ua.EmployeeFirstNameEn = reader["EmployeeFirstNameEn"] == DBNull.Value ? null : (string)reader["EmployeeFirstNameEn"]; } catch { }
            try { ua.EmployeeLastNameEn  = reader["EmployeeLastNameEn"]  == DBNull.Value ? null : (string)reader["EmployeeLastNameEn"];  } catch { }
            try { ua.EmployeeTitleNameTh = reader["EmployeeTitleNameTh"] == DBNull.Value ? null : (string)reader["EmployeeTitleNameTh"]; } catch { }
            try { ua.EmployeeFirstNameTh = reader["EmployeeFirstNameTh"] == DBNull.Value ? null : (string)reader["EmployeeFirstNameTh"]; } catch { }
            try { ua.EmployeeLastNameTh  = reader["EmployeeLastNameTh"]  == DBNull.Value ? null : (string)reader["EmployeeLastNameTh"];  } catch { }
            try { ua.DepartmentId        = reader["DepartmentId"]        == DBNull.Value ? (int?)null : (int)reader["DepartmentId"];     } catch { }
            try { ua.DepartmentCode      = reader["DepartmentCode"]      == DBNull.Value ? null : (string)reader["DepartmentCode"];      } catch { }
            try { ua.DepartmentName      = reader["DepartmentName"]      == DBNull.Value ? null : (string)reader["DepartmentName"];      } catch { }
            try { ua.DepartmentNameEn    = reader["DepartmentNameEn"]    == DBNull.Value ? null : (string)reader["DepartmentNameEn"];    } catch { }
            try { ua.DivisionId          = reader["DivisionId"]          == DBNull.Value ? (int?)null : (int)reader["DivisionId"];       } catch { }
            try { ua.DivisionCode        = reader["DivisionCode"]        == DBNull.Value ? null : (string)reader["DivisionCode"];        } catch { }
            try { ua.DivisionName        = reader["DivisionName"]        == DBNull.Value ? null : (string)reader["DivisionName"];        } catch { }
            try { ua.DivisionNameEn      = reader["DivisionNameEn"]      == DBNull.Value ? null : (string)reader["DivisionNameEn"];      } catch { }
            try { ua.PositionName        = reader["PositionName"]        == DBNull.Value ? null : (string)reader["PositionName"];        } catch { }
            try { ua.LevelCode           = reader["LevelCode"]           == DBNull.Value ? null : (string)reader["LevelCode"];           } catch { }
            try { ua.UpdatedAt           = reader["UpdatedAt"]           == DBNull.Value ? (DateTime?)null : (DateTime)reader["UpdatedAt"]; } catch { }
            try { ua.LastLoginIP         = reader["LastLoginIP"]         == DBNull.Value ? null : (string)reader["LastLoginIP"];           } catch { }

            return ua;
        }
    }
}
