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
    /// <summary>
    /// ADO.NET implementation of IWorkflowStepRepository.
    /// Manages WorkflowStep and WorkflowConnection rows for template definitions.
    /// </summary>
    public class WorkflowStepRepository : IWorkflowStepRepository
    {
        private readonly string _connectionString;

        public WorkflowStepRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        // ----------------------------------------------------------------
        // WorkflowStep — Read
        // ----------------------------------------------------------------

        public WorkflowStep GetStep(int workflowStepId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WorkflowStepId, WorkflowDefinitionId, StepName, StepType, Description, " +
                    "       AssignedRole, AssignedUser, AssignedOrgUnit, SLAHours, SLADays, " +
                    "       AllowDelegation, MaxDelegationDepth, SequenceNumber, IsDeleted " +
                    "FROM   WFE.s_WFStep " +
                    "WHERE  WorkflowStepId = @WorkflowStepId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowStepId", workflowStepId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapStep(reader) : null;
                    }
                }
            }
        }

        public IList<WorkflowStep> GetStepsByTemplate(int workflowDefinitionId)
        {
            var results = new List<WorkflowStep>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WorkflowStepId, WorkflowDefinitionId, StepName, StepType, Description, " +
                    "       AssignedRole, AssignedUser, AssignedOrgUnit, SLAHours, SLADays, " +
                    "       AllowDelegation, MaxDelegationDepth, SequenceNumber, IsDeleted " +
                    "FROM   WFE.s_WFStep " +
                    "WHERE  WorkflowDefinitionId = @WorkflowDefinitionId " +
                    "  AND  IsDeleted = 0 " +
                    "ORDER BY SequenceNumber ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowDefinitionId", workflowDefinitionId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapStep(reader));
                    }
                }
            }
            return results;
        }

        // ----------------------------------------------------------------
        // WorkflowStep — Write
        // ----------------------------------------------------------------

        public int CreateStep(WorkflowStep step)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "INSERT INTO WFE.s_WFStep " +
                    "    (WorkflowDefinitionId, StepName, StepType, Description, " +
                    "     AssignedRole, AssignedUser, AssignedOrgUnit, " +
                    "     SLAHours, SLADays, AllowDelegation, MaxDelegationDepth, " +
                    "     SequenceNumber, IsDeleted) " +
                    "OUTPUT INSERTED.WorkflowStepId " +
                    "VALUES (@WorkflowDefinitionId, @StepName, @StepType, @Description, " +
                    "        @AssignedRole, @AssignedUser, @AssignedOrgUnit, " +
                    "        @SLAHours, @SLADays, @AllowDelegation, @MaxDelegationDepth, " +
                    "        @SequenceNumber, 0)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    AddStepParameters(cmd, step);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateStep(WorkflowStep step)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE WFE.s_WFStep " +
                    "SET    StepName           = @StepName, " +
                    "       StepType           = @StepType, " +
                    "       Description        = @Description, " +
                    "       AssignedRole       = @AssignedRole, " +
                    "       AssignedUser       = @AssignedUser, " +
                    "       AssignedOrgUnit    = @AssignedOrgUnit, " +
                    "       SLAHours           = @SLAHours, " +
                    "       SLADays            = @SLADays, " +
                    "       AllowDelegation    = @AllowDelegation, " +
                    "       MaxDelegationDepth = @MaxDelegationDepth, " +
                    "       SequenceNumber     = @SequenceNumber " +
                    "WHERE  WorkflowStepId = @WorkflowStepId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowStepId", step.WorkflowStepId);
                    AddStepParameters(cmd, step);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SoftDeleteStep(int workflowStepId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE WFE.s_WFStep " +
                    "SET    IsDeleted = 1 " +
                    "WHERE  WorkflowStepId = @WorkflowStepId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowStepId", workflowStepId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // WorkflowConnection — Read
        // ----------------------------------------------------------------

        public IList<WorkflowConnection> GetConnectionsByTemplate(int workflowDefinitionId)
        {
            var results = new List<WorkflowConnection>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WorkflowConnectionId, WorkflowDefinitionId, FromStepId, ToStepId, " +
                    "       ConditionExpression, SequenceNumber, IsDeleted " +
                    "FROM   WFE.s_WFConnection " +
                    "WHERE  WorkflowDefinitionId = @WorkflowDefinitionId " +
                    "  AND  IsDeleted = 0 " +
                    "ORDER BY SequenceNumber ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowDefinitionId", workflowDefinitionId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapConnection(reader));
                    }
                }
            }
            return results;
        }

        public IList<WorkflowConnection> GetConnectionsFromStep(int fromStepId)
        {
            var results = new List<WorkflowConnection>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WorkflowConnectionId, WorkflowDefinitionId, FromStepId, ToStepId, " +
                    "       ConditionExpression, SequenceNumber, IsDeleted " +
                    "FROM   WFE.s_WFConnection " +
                    "WHERE  FromStepId = @FromStepId " +
                    "  AND  IsDeleted = 0 " +
                    "ORDER BY SequenceNumber ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@FromStepId", fromStepId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapConnection(reader));
                    }
                }
            }
            return results;
        }

        // ----------------------------------------------------------------
        // WorkflowConnection — Write
        // ----------------------------------------------------------------

        public int CreateConnection(WorkflowConnection connection)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "INSERT INTO WFE.s_WFConnection " +
                    "    (WorkflowDefinitionId, FromStepId, ToStepId, " +
                    "     ConditionExpression, SequenceNumber, IsDeleted) " +
                    "OUTPUT INSERTED.WorkflowConnectionId " +
                    "VALUES (@WorkflowDefinitionId, @FromStepId, @ToStepId, " +
                    "        @ConditionExpression, @SequenceNumber, 0)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowDefinitionId", connection.WorkflowDefinitionId);
                    cmd.Parameters.AddWithValue("@FromStepId",           connection.FromStepId);
                    cmd.Parameters.AddWithValue("@ToStepId",             connection.ToStepId);
                    cmd.Parameters.AddWithValue("@ConditionExpression",  (object)connection.ConditionExpression ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SequenceNumber",       connection.SequenceNumber);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateConnection(WorkflowConnection connection)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE WFE.s_WFConnection " +
                    "SET    FromStepId          = @FromStepId, " +
                    "       ToStepId            = @ToStepId, " +
                    "       ConditionExpression = @ConditionExpression, " +
                    "       SequenceNumber      = @SequenceNumber " +
                    "WHERE  WorkflowConnectionId = @WorkflowConnectionId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowConnectionId", connection.WorkflowConnectionId);
                    cmd.Parameters.AddWithValue("@FromStepId",           connection.FromStepId);
                    cmd.Parameters.AddWithValue("@ToStepId",             connection.ToStepId);
                    cmd.Parameters.AddWithValue("@ConditionExpression",  (object)connection.ConditionExpression ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SequenceNumber",       connection.SequenceNumber);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SoftDeleteConnection(int workflowConnectionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE WFE.s_WFConnection " +
                    "SET    IsDeleted = 1 " +
                    "WHERE  WorkflowConnectionId = @WorkflowConnectionId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WorkflowConnectionId", workflowConnectionId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        private static void AddStepParameters(SqlCommand cmd, WorkflowStep step)
        {
            cmd.Parameters.AddWithValue("@WorkflowDefinitionId", step.WorkflowDefinitionId);
            cmd.Parameters.AddWithValue("@StepName",             step.StepName);
            cmd.Parameters.AddWithValue("@StepType",             (byte)step.StepType);
            cmd.Parameters.AddWithValue("@Description",          (object)step.Description     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AssignedRole",         (object)step.AssignedRole    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AssignedUser",         (object)step.AssignedUser    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AssignedOrgUnit",      (object)step.AssignedOrgUnit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SLAHours",             (object)step.SLAHours        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SLADays",              (object)step.SLADays         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AllowDelegation",      step.AllowDelegation);
            cmd.Parameters.AddWithValue("@MaxDelegationDepth",   step.MaxDelegationDepth);
            cmd.Parameters.AddWithValue("@SequenceNumber",       step.SequenceNumber);
        }

        private static WorkflowStep MapStep(SqlDataReader reader)
        {
            return new WorkflowStep
            {
                WorkflowStepId       = (int)reader["WorkflowStepId"],
                WorkflowDefinitionId = (int)reader["WorkflowDefinitionId"],
                StepName             = (string)reader["StepName"],
                StepType             = (WorkflowStepType)(byte)reader["StepType"],
                Description          = reader["Description"] == DBNull.Value ? null : (string)reader["Description"],
                AssignedRole         = reader["AssignedRole"] == DBNull.Value ? null : (string)reader["AssignedRole"],
                AssignedUser         = reader["AssignedUser"] == DBNull.Value ? null : (string)reader["AssignedUser"],
                AssignedOrgUnit      = reader["AssignedOrgUnit"] == DBNull.Value ? null : (string)reader["AssignedOrgUnit"],
                SLAHours             = reader["SLAHours"] == DBNull.Value ? (int?)null : (int)reader["SLAHours"],
                SLADays              = reader["SLADays"] == DBNull.Value ? (int?)null : (int)reader["SLADays"],
                AllowDelegation      = (bool)reader["AllowDelegation"],
                MaxDelegationDepth   = (int)reader["MaxDelegationDepth"],
                SequenceNumber       = (int)reader["SequenceNumber"],
                IsDeleted            = (bool)reader["IsDeleted"]
            };
        }

        private static WorkflowConnection MapConnection(SqlDataReader reader)
        {
            return new WorkflowConnection
            {
                WorkflowConnectionId = (int)reader["WorkflowConnectionId"],
                WorkflowDefinitionId = (int)reader["WorkflowDefinitionId"],
                FromStepId           = (int)reader["FromStepId"],
                ToStepId             = (int)reader["ToStepId"],
                ConditionExpression  = reader["ConditionExpression"] == DBNull.Value ? null : (string)reader["ConditionExpression"],
                SequenceNumber       = (int)reader["SequenceNumber"],
                IsDeleted            = (bool)reader["IsDeleted"]
            };
        }
    }
}
