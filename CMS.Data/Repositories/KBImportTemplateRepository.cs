using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class KBImportTemplateRepository : IKBImportTemplateRepository
    {
        private readonly string _connectionString;

        public KBImportTemplateRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public IEnumerable<KBImportTemplate> GetAll()
        {
            var results = new List<KBImportTemplate>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT TemplateID, TemplateName, Description, SortOrder, IsActive,
                           CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
                    FROM   KMS.m_KBImportTemplate
                    ORDER BY SortOrder, TemplateName";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        results.Add(Map(reader));
                }
            }
            return results;
        }

        public KBImportTemplate GetById(int templateId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT TemplateID, TemplateName, Description, SortOrder, IsActive,
                           CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
                    FROM   KMS.m_KBImportTemplate
                    WHERE  TemplateID = @TemplateID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return Map(reader);
                    }
                }
            }
            return null;
        }

        public bool ExistsByName(string templateName, int excludeTemplateId = 0)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT TOP 1 1
                    FROM   KMS.m_KBImportTemplate
                    WHERE  TemplateName = @TemplateName AND TemplateID <> @ExcludeTemplateID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TemplateName", templateName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ExcludeTemplateID", excludeTemplateId);
                    return cmd.ExecuteScalar() != null;
                }
            }
        }

        public IList<KBImportTemplateColumnMapping> GetMappingsByTemplateId(int templateId)
        {
            var results = new List<KBImportTemplateColumnMapping>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    SELECT TemplateColumnID, TemplateID, TargetColumnName, ExcelColumnRef, DisplayOrder
                    FROM   KMS.m_KBImportTemplateColumn
                    WHERE  TemplateID = @TemplateID
                    ORDER BY DisplayOrder";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapMapping(reader));
                    }
                }
            }
            return results;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public int SaveTemplateWithMappings(KBImportTemplate template, IList<KBImportTemplateColumnMapping> mappings)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int templateId = template.TemplateID > 0
                            ? UpdateTemplate(conn, tx, template)
                            : InsertTemplate(conn, tx, template);

                        const string deleteSql = "DELETE FROM KMS.m_KBImportTemplateColumn WHERE TemplateID = @TemplateID";
                        using (var cmd = new SqlCommand(deleteSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@TemplateID", templateId);
                            cmd.ExecuteNonQuery();
                        }

                        const string insertSql = @"
                            INSERT INTO KMS.m_KBImportTemplateColumn
                                (TemplateID, TargetColumnName, ExcelColumnRef, DisplayOrder)
                            VALUES
                                (@TemplateID, @TargetColumnName, @ExcelColumnRef, @DisplayOrder)";
                        foreach (var mapping in mappings)
                        {
                            using (var cmd = new SqlCommand(insertSql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@TemplateID", templateId);
                                cmd.Parameters.AddWithValue("@TargetColumnName", mapping.TargetColumnName ?? string.Empty);
                                cmd.Parameters.AddWithValue("@ExcelColumnRef", (object)mapping.ExcelColumnRef ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@DisplayOrder", mapping.DisplayOrder);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return templateId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Deactivate(int templateId, string updatedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    UPDATE KMS.m_KBImportTemplate
                    SET    IsActive = 0, UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE()
                    WHERE  TemplateID = @TemplateID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy ?? string.Empty);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static int InsertTemplate(SqlConnection conn, SqlTransaction tx, KBImportTemplate template)
        {
            const string sql = @"
                INSERT INTO KMS.m_KBImportTemplate
                    (TemplateName, Description, SortOrder, IsActive, CreatedBy, CreatedDate)
                VALUES
                    (@TemplateName, @Description, @SortOrder, 1, @CreatedBy, GETDATE());
                SELECT SCOPE_IDENTITY();";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@TemplateName", template.TemplateName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Description", (object)template.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SortOrder", template.SortOrder);
                cmd.Parameters.AddWithValue("@CreatedBy", template.CreatedBy ?? string.Empty);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static int UpdateTemplate(SqlConnection conn, SqlTransaction tx, KBImportTemplate template)
        {
            const string sql = @"
                UPDATE KMS.m_KBImportTemplate SET
                    TemplateName = @TemplateName,
                    Description  = @Description,
                    SortOrder    = @SortOrder,
                    UpdatedBy    = @UpdatedBy,
                    UpdatedDate  = GETDATE()
                WHERE TemplateID = @TemplateID";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@TemplateID", template.TemplateID);
                cmd.Parameters.AddWithValue("@TemplateName", template.TemplateName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Description", (object)template.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SortOrder", template.SortOrder);
                cmd.Parameters.AddWithValue("@UpdatedBy", template.UpdatedBy ?? string.Empty);
                cmd.ExecuteNonQuery();
            }
            return template.TemplateID;
        }

        private static KBImportTemplate Map(IDataReader r)
        {
            return new KBImportTemplate
            {
                TemplateID   = r.GetInt32(r.GetOrdinal("TemplateID")),
                TemplateName = r["TemplateName"] as string,
                Description  = r["Description"] as string,
                SortOrder    = r.IsDBNull(r.GetOrdinal("SortOrder")) ? 0 : r.GetInt32(r.GetOrdinal("SortOrder")),
                IsActive     = !r.IsDBNull(r.GetOrdinal("IsActive")) && r.GetBoolean(r.GetOrdinal("IsActive")),
                CreatedBy    = r["CreatedBy"] as string,
                CreatedDate  = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate")),
                UpdatedBy    = r["UpdatedBy"] as string,
                UpdatedDate  = r.IsDBNull(r.GetOrdinal("UpdatedDate")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("UpdatedDate")),
            };
        }

        private static KBImportTemplateColumnMapping MapMapping(IDataReader r)
        {
            return new KBImportTemplateColumnMapping
            {
                TemplateColumnID = r.GetInt32(r.GetOrdinal("TemplateColumnID")),
                TemplateID       = r.GetInt32(r.GetOrdinal("TemplateID")),
                TargetColumnName = r["TargetColumnName"] as string,
                ExcelColumnRef   = r["ExcelColumnRef"] as string,
                DisplayOrder     = r.IsDBNull(r.GetOrdinal("DisplayOrder")) ? 0 : r.GetInt32(r.GetOrdinal("DisplayOrder")),
            };
        }
    }
}
