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
    /// <summary>
    /// ADO.NET implementation of IWorkflowTemplateRepository.
    /// Manages WorkflowDefinition rows including versioning and soft-delete.
    /// </summary>
    public class WorkflowTemplateRepository : IWorkflowTemplateRepository
    {
        private readonly string _connectionString;

        public WorkflowTemplateRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        // ----------------------------------------------------------------
        // Create
        // ----------------------------------------------------------------

        public int CreateTemplate(WorkflowDefinition template)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "INSERT INTO WFE.s_WFDefinition " +
                    "    (Name, Description, Version, IsActive, CreatedBy, CreatedDate, IsDeleted) " +
                    "OUTPUT INSERTED.WFDefinitionId " +
                    "VALUES (@Name, @Description, @Version, @IsActive, @CreatedBy, GETUTCDATE(), 0)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Name",        template.Name);
                    cmd.Parameters.AddWithValue("@Description", (object)template.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Version",     template.Version);
                    cmd.Parameters.AddWithValue("@IsActive",    template.IsActive);
                    cmd.Parameters.AddWithValue("@CreatedBy",   template.CreatedBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // ----------------------------------------------------------------
        // Read
        // ----------------------------------------------------------------

        public WorkflowDefinition GetTemplate(int workflowDefinitionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 " +
                    "    WFDefinitionId, Name, Description, Version, IsActive, " +
                    "    CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsDeleted, DeletedDate " +
                    "FROM WFE.s_WFDefinition " +
                    "WHERE WFDefinitionId = @WFDefinitionId " +
                    "  AND IsDeleted = 0 " +
                    "ORDER BY Version DESC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WFDefinitionId", workflowDefinitionId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapTemplate(reader) : null;
                    }
                }
            }
        }

        public WorkflowDefinition GetTemplateVersion(int workflowDefinitionId, int version)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WFDefinitionId, Name, Description, Version, IsActive, " +
                    "       CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsDeleted, DeletedDate " +
                    "FROM   WFE.s_WFDefinition " +
                    "WHERE  WFDefinitionId = @WFDefinitionId " +
                    "  AND  Version = @Version " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WFDefinitionId", workflowDefinitionId);
                    cmd.Parameters.AddWithValue("@Version",              version);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapTemplate(reader) : null;
                    }
                }
            }
        }

        public IList<WorkflowDefinition> GetTemplateVersions(int workflowDefinitionId)
        {
            var results = new List<WorkflowDefinition>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT WFDefinitionId, Name, Description, Version, IsActive, " +
                    "       CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsDeleted, DeletedDate " +
                    "FROM   WFE.s_WFDefinition " +
                    "WHERE  WFDefinitionId = @WFDefinitionId " +
                    "  AND  IsDeleted = 0 " +
                    "ORDER BY Version DESC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WFDefinitionId", workflowDefinitionId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapTemplate(reader));
                    }
                }
            }
            return results;
        }

        public PagedResult<WorkflowDefinition> GetTemplates(WorkflowTemplateFilter filter)
        {
            var items      = new List<WorkflowDefinition>();
            int totalCount = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var where = new System.Text.StringBuilder("WHERE IsDeleted = 0");
                if (filter.IsActive.HasValue)
                    where.Append(" AND IsActive = @IsActive");
                if (!string.IsNullOrEmpty(filter.NameContains))
                    where.Append(" AND Name LIKE @NameContains");

                int offset = (filter.PageNumber - 1) * filter.PageSize;

                string sql =
                    "SELECT COUNT(*) OVER () AS TotalCount, " +
                    "       WFDefinitionId, Name, Description, Version, IsActive, " +
                    "       CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsDeleted, DeletedDate " +
                    "FROM   WFE.s_WFDefinition " +
                    where +
                    " ORDER BY Name ASC, Version DESC " +
                    " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    if (filter.IsActive.HasValue)
                        cmd.Parameters.AddWithValue("@IsActive", filter.IsActive.Value);
                    if (!string.IsNullOrEmpty(filter.NameContains))
                        cmd.Parameters.AddWithValue("@NameContains", "%" + filter.NameContains + "%");
                    cmd.Parameters.AddWithValue("@Offset",   offset);
                    cmd.Parameters.AddWithValue("@PageSize", filter.PageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (totalCount == 0)
                                totalCount = (int)reader["TotalCount"];
                            items.Add(MapTemplate(reader));
                        }
                    }
                }
            }

            return new PagedResult<WorkflowDefinition>
            {
                Items      = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize   = filter.PageSize
            };
        }

        public bool HasActiveInstances(int workflowDefinitionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT CASE WHEN EXISTS (" +
                    "    SELECT 1 FROM dbo.WorkflowInstance " +
                    "    WHERE  WFDefinitionId = @WFDefinitionId " +
                    "      AND  Status NOT IN (3, 4) " +   // not Completed or Rejected
                    "      AND  IsDeleted = 0" +
                    ") THEN 1 ELSE 0 END", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WFDefinitionId", workflowDefinitionId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                }
            }
        }

        // ----------------------------------------------------------------
        // Update
        // ----------------------------------------------------------------

        public void UpdateTemplate(WorkflowDefinition template)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE WFE.s_WFDefinition " +
                    "SET    Name         = @Name, " +
                    "       Description  = @Description, " +
                    "       Version      = @Version, " +
                    "       IsActive     = @IsActive, " +
                    "       ModifiedBy   = @ModifiedBy, " +
                    "       ModifiedDate = GETUTCDATE() " +
                    "WHERE  WFDefinitionId = @WFDefinitionId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WFDefinitionId", template.WFDefinitionId);
                    cmd.Parameters.AddWithValue("@Name",        template.Name);
                    cmd.Parameters.AddWithValue("@Description", (object)template.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Version",     template.Version);
                    cmd.Parameters.AddWithValue("@IsActive",    template.IsActive);
                    cmd.Parameters.AddWithValue("@ModifiedBy",  (object)template.ModifiedBy ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Delete (soft)
        // ----------------------------------------------------------------

        public void SoftDeleteTemplate(int workflowDefinitionId, string deletedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE WFE.s_WFDefinition " +
                    "SET    IsDeleted = 1, DeletedDate = GETUTCDATE(), " +
                    "       ModifiedBy = @DeletedBy, ModifiedDate = GETUTCDATE() " +
                    "WHERE  WFDefinitionId = @WFDefinitionId " +
                    "  AND  IsDeleted = 0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@WFDefinitionId", workflowDefinitionId);
                    cmd.Parameters.AddWithValue("@DeletedBy",            deletedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Mapping
        // ----------------------------------------------------------------

        private static WorkflowDefinition MapTemplate(SqlDataReader reader)
        {
            return new WorkflowDefinition
            {
                WFDefinitionId = (int)reader["WFDefinitionId"],
                Name                 = (string)reader["Name"],
                Description          = reader["Description"] == DBNull.Value ? null : (string)reader["Description"],
                Version              = (int)reader["Version"],
                IsActive             = (bool)reader["IsActive"],
                CreatedBy            = (string)reader["CreatedBy"],
                CreatedDate          = (DateTime)reader["CreatedDate"],
                ModifiedBy           = reader["ModifiedBy"] == DBNull.Value ? null : (string)reader["ModifiedBy"],
                ModifiedDate         = reader["ModifiedDate"] == DBNull.Value ? default(DateTime) : (DateTime)reader["ModifiedDate"],
                IsDeleted            = (bool)reader["IsDeleted"],
                DeletedDate          = reader["DeletedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DeletedDate"]
            };
        }
    }
}
