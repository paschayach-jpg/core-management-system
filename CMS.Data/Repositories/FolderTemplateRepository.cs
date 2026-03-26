using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class FolderTemplateRepository : IFolderTemplateRepository
    {
        private readonly string _connectionString;

        public FolderTemplateRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(FolderTemplate template)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_InsertFolderTemplate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TemplateCode", template.TemplateCode);
                    cmd.Parameters.AddWithValue("@TemplateName", template.TemplateName);
                    cmd.Parameters.AddWithValue("@TargetType",   template.TargetType);
                    cmd.Parameters.AddWithValue("@Description",  (object)template.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy",    template.CreatedBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public IEnumerable<FolderTemplate> GetAll()
        {
            var results = new List<FolderTemplate>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetAllFolderTemplates", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapTemplate(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<FolderTemplateItem> GetItems(int templateId)
        {
            var results = new List<FolderTemplateItem>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFolderTemplateItems", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapTemplateItem(reader));
                    }
                }
            }
            return results;
        }

        public int InsertItem(FolderTemplateItem item)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_InsertFolderTemplateItem", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TemplateID",          item.TemplateID);
                    cmd.Parameters.AddWithValue("@ParentItemID",        (object)item.ParentItemID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FolderName",          item.FolderName);
                    cmd.Parameters.AddWithValue("@ConfidentialityLevel",(byte)item.ConfidentialityLevel);
                    cmd.Parameters.AddWithValue("@SortOrder",           item.SortOrder);
                    cmd.Parameters.AddWithValue("@InheritPermission",   item.InheritPermission);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(FolderTemplate template)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_UpdateFolderTemplate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TemplateID",   template.TemplateID);
                    cmd.Parameters.AddWithValue("@TemplateCode", template.TemplateCode);
                    cmd.Parameters.AddWithValue("@TemplateName", template.TemplateName);
                    cmd.Parameters.AddWithValue("@TargetType",   template.TargetType);
                    cmd.Parameters.AddWithValue("@Description",  (object)template.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive",     template.IsActive);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int templateId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_DeleteFolderTemplate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public FolderTemplate GetById(int templateId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFolderTemplateById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapTemplate(reader) : null;
                    }
                }
            }
        }

        public void UpdateItem(FolderTemplateItem item)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_UpdateFolderTemplateItem", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemID",              item.ItemID);
                    cmd.Parameters.AddWithValue("@FolderName",          item.FolderName);
                    cmd.Parameters.AddWithValue("@ConfidentialityLevel",(byte)item.ConfidentialityLevel);
                    cmd.Parameters.AddWithValue("@SortOrder",           item.SortOrder);
                    cmd.Parameters.AddWithValue("@InheritPermission",   item.InheritPermission);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteItem(int itemId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_DeleteFolderTemplateItem", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemID", itemId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteItemsByTemplate(int templateId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "DELETE FROM storage.FolderTemplateItem WHERE TemplateID = @TemplateID", conn))
                {
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static FolderTemplate MapTemplate(SqlDataReader reader)
        {
            return new FolderTemplate
            {
                TemplateID   = (int)reader["TemplateID"],
                TemplateCode = (string)reader["TemplateCode"],
                TemplateName = (string)reader["TemplateName"],
                TargetType   = (string)reader["TargetType"],
                Description  = reader["Description"] == DBNull.Value ? null : (string)reader["Description"],
                IsActive     = (bool)reader["IsActive"],
                CreatedBy    = (int)reader["CreatedBy"],
                CreatedDate  = (DateTime)reader["CreatedDate"]
            };
        }

        private static FolderTemplateItem MapTemplateItem(SqlDataReader reader)
        {
            return new FolderTemplateItem
            {
                ItemID               = (int)reader["ItemID"],
                TemplateID           = (int)reader["TemplateID"],
                ParentItemID         = reader["ParentItemID"] == DBNull.Value ? (int?)null : (int)reader["ParentItemID"],
                FolderName           = (string)reader["FolderName"],
                ConfidentialityLevel = (byte)reader["ConfidentialityLevel"],
                SortOrder            = (int)reader["SortOrder"],
                InheritPermission    = (bool)reader["InheritPermission"]
            };
        }
    }
}
