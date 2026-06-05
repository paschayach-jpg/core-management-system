using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly string _connectionString;

        public AuditRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public long Insert(AuditLog entry)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("audit.sp_InsertAuditLog", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID",       entry.UserID);
                    cmd.Parameters.AddWithValue("@UserName",     entry.UserName);
                    cmd.Parameters.AddWithValue("@UserRole",     (object)entry.UserRole ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Module",       entry.Module);
                    cmd.Parameters.AddWithValue("@Action",       entry.Action);
                    cmd.Parameters.AddWithValue("@TableName",    (object)entry.TableName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RecordID",     (object)entry.RecordID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OldValue",     (object)entry.OldValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewValue",     (object)entry.NewValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IPAddress",    (object)entry.IPAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MachineName",  (object)entry.MachineName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SessionID",    (object)entry.SessionID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Result",       (byte)entry.Result);
                    cmd.Parameters.AddWithValue("@ErrorMessage", (object)entry.ErrorMessage ?? DBNull.Value);
                    return Convert.ToInt64(cmd.ExecuteScalar());
                }
            }
        }

        public PagedResult<AuditLog> Query(AuditQuery query)
        {
            var items = new List<AuditLog>();
            int totalCount = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("audit.sp_QueryAuditLog", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate",   query.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",     query.ToDate);
                    cmd.Parameters.AddWithValue("@UserID",     (object)query.UserID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserName",   (object)query.UserName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Module",     (object)query.Module ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action",     (object)query.Action ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Result",     (object)query.Result ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PageNumber", query.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize",   query.PageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (totalCount == 0)
                                totalCount = Convert.ToInt32(reader["TotalCount"]);
                            items.Add(MapAuditLog(reader));
                        }
                    }
                }
            }

            return new PagedResult<AuditLog>
            {
                Items      = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize   = query.PageSize
            };
        }

        public List<AuditLog> GetAuditLogsByWorkflowInstance(int workflowInstanceId)
        {
            var results = new List<AuditLog>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT AuditID, AuditDate, UserID, UserName, UserRole, Module, Action, TableName, RecordID, " +
                    "       OldValue, NewValue, IPAddress, MachineName, SessionID, Result, ErrorMessage " +
                    "FROM   audit.AuditLog " +
                    "WHERE  (TableName = 'WorkflowInstance' AND RecordID = @WorkflowInstanceId) " +
                    "   OR  (TableName = 'WorkflowStepInstance' AND RecordID IN (SELECT WFStepInstanceId FROM WFE.t_WFStepInstance WHERE WFInstanceId = @WorkflowInstanceId)) " +
                    "ORDER BY AuditDate ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowInstanceId", workflowInstanceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapAuditLog(reader));
                    }
                }
            }
            return results;
        }

        private static AuditLog MapAuditLog(SqlDataReader reader)
        {
            return new AuditLog
            {
                AuditID      = (long)reader["AuditID"],
                AuditDate    = (DateTime)reader["AuditDate"],
                UserID       = (int)reader["UserID"],
                UserName     = (string)reader["UserName"],
                UserRole     = reader["UserRole"] == DBNull.Value ? (int?)null : (int)(byte)reader["UserRole"],
                Module       = (string)reader["Module"],
                Action       = (string)reader["Action"],
                TableName    = reader["TableName"] == DBNull.Value ? null : (string)reader["TableName"],
                RecordID     = reader["RecordID"] == DBNull.Value ? (int?)null : (int)reader["RecordID"],
                OldValue     = reader["OldValue"] == DBNull.Value ? null : (string)reader["OldValue"],
                NewValue     = reader["NewValue"] == DBNull.Value ? null : (string)reader["NewValue"],
                IPAddress    = reader["IPAddress"] == DBNull.Value ? null : (string)reader["IPAddress"],
                MachineName  = reader["MachineName"] == DBNull.Value ? null : (string)reader["MachineName"],
                SessionID    = reader["SessionID"] == DBNull.Value ? null : (string)reader["SessionID"],
                Result       = (byte)reader["Result"],
                ErrorMessage = reader["ErrorMessage"] == DBNull.Value ? null : (string)reader["ErrorMessage"]
            };
        }
    }
}
