using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    /// <summary>
    /// ADO.NET implementation of IDelegationRepository.
    /// Delegation records are append-only; the stored procedure handles
    /// deactivating the previous active record and updating AssignedTo.
    /// </summary>
    public class DelegationRepository : IDelegationRepository
    {
        private readonly string _connectionString;

        public DelegationRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        // ----------------------------------------------------------------
        // Create
        // ----------------------------------------------------------------

        public int CreateDelegation(WFDelegation delegation, int maxDelegationDepth)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_WF_CreateDelegation", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@WorkflowStepInstanceId", delegation.WorkflowStepInstanceId);
                    cmd.Parameters.AddWithValue("@DelegatedFrom",          delegation.DelegatedFrom);
                    cmd.Parameters.AddWithValue("@DelegatedTo",            delegation.DelegatedTo);
                    cmd.Parameters.AddWithValue("@DelegationReason",       delegation.DelegationReason);
                    cmd.Parameters.AddWithValue("@MaxDelegationDepth",     maxDelegationDepth);

                    var outParam = new SqlParameter("@NewWFDelegationId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outParam);
                    cmd.ExecuteNonQuery();
                    return (int)outParam.Value;
                }
            }
        }

        // ----------------------------------------------------------------
        // Read
        // ----------------------------------------------------------------

        public WFDelegation GetDelegation(int wfDelegationId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WFDelegationId, WorkflowStepInstanceId, DelegatedFrom, DelegatedTo, " +
                    "       DelegationReason, DelegationDate, DelegationDepth, IsActive, IsDeleted " +
                    "FROM   dbo.WFDelegation " +
                    "WHERE  WFDelegationId = @WFDelegationId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WFDelegationId", wfDelegationId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapDelegation(reader) : null;
                    }
                }
            }
        }

        public IList<WFDelegation> GetDelegationsByStepInstance(int workflowStepInstanceId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_WF_GetDelegationHistory", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@WorkflowStepInstanceId", workflowStepInstanceId);

                    var results = new List<WFDelegation>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapDelegation(reader));
                    }
                    return results;
                }
            }
        }

        public WFDelegation GetActiveDelegation(int workflowStepInstanceId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 " +
                    "       WFDelegationId, WorkflowStepInstanceId, DelegatedFrom, DelegatedTo, " +
                    "       DelegationReason, DelegationDate, DelegationDepth, IsActive, IsDeleted " +
                    "FROM   dbo.WFDelegation " +
                    "WHERE  WorkflowStepInstanceId = @WorkflowStepInstanceId " +
                    "  AND  IsActive  = 1 " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowStepInstanceId", workflowStepInstanceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapDelegation(reader) : null;
                    }
                }
            }
        }

        public int GetDelegationDepth(int workflowStepInstanceId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT ISNULL(MAX(DelegationDepth), 0) " +
                    "FROM   dbo.WFDelegation " +
                    "WHERE  WorkflowStepInstanceId = @WorkflowStepInstanceId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowStepInstanceId", workflowStepInstanceId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void DeactivateDelegations(int workflowStepInstanceId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE dbo.WFDelegation " +
                    "SET    IsActive = 0 " +
                    "WHERE  WorkflowStepInstanceId = @WorkflowStepInstanceId " +
                    "  AND  IsActive  = 1 " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowStepInstanceId", workflowStepInstanceId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Update
        // ----------------------------------------------------------------

        public void UpdateDelegation(WFDelegation delegation)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE dbo.WFDelegation " +
                    "SET    IsActive  = @IsActive, " +
                    "       IsDeleted = @IsDeleted " +
                    "WHERE  WFDelegationId = @WFDelegationId", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WFDelegationId", delegation.WFDelegationId);
                    cmd.Parameters.AddWithValue("@IsActive",       delegation.IsActive);
                    cmd.Parameters.AddWithValue("@IsDeleted",      delegation.IsDeleted);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Mapping
        // ----------------------------------------------------------------

        private static WFDelegation MapDelegation(SqlDataReader reader)
        {
            return new WFDelegation
            {
                WFDelegationId          = (int)reader["WFDelegationId"],
                WorkflowStepInstanceId  = (int)reader["WorkflowStepInstanceId"],
                DelegatedFrom           = (string)reader["DelegatedFrom"],
                DelegatedTo             = (string)reader["DelegatedTo"],
                DelegationReason        = (string)reader["DelegationReason"],
                DelegationDate          = (DateTime)reader["DelegationDate"],
                DelegationDepth         = (int)reader["DelegationDepth"],
                IsActive                = (bool)reader["IsActive"],
                IsDeleted               = (bool)reader["IsDeleted"]
            };
        }
    }
}
