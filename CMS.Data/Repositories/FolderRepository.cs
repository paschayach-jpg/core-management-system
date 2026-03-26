using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private readonly string _connectionString;

        public FolderRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(Folder folder)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_InsertFolder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderName",          folder.FolderName);
                    cmd.Parameters.AddWithValue("@ParentFolderID",      (object)folder.ParentFolderID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrgUnitID",           folder.OrgUnitID);
                    cmd.Parameters.AddWithValue("@TemplateID",          (object)folder.TemplateID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ConfidentialityLevel",(byte)folder.ConfidentialityLevel);
                    cmd.Parameters.AddWithValue("@FolderDepth",         (byte)folder.FolderDepth);
                    // Generate PhysicalPath if not provided: {OrgUnitID}\{GUID}
                    string physicalPath = folder.PhysicalPath ?? 
                        string.Format("{0}\\{1}", folder.OrgUnitID, Guid.NewGuid().ToString("N"));
                    cmd.Parameters.AddWithValue("@PhysicalPath",        physicalPath);
                    cmd.Parameters.AddWithValue("@InheritPermission",   folder.InheritPermission);
                    cmd.Parameters.AddWithValue("@CreatedBy",           folder.CreatedBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(Folder folder)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_UpdateFolder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderID",           folder.FolderID);
                    cmd.Parameters.AddWithValue("@FolderName",         folder.FolderName);
                    cmd.Parameters.AddWithValue("@ConfidentialityLevel",(byte)folder.ConfidentialityLevel);
                    cmd.Parameters.AddWithValue("@InheritPermission",  folder.InheritPermission);
                    cmd.Parameters.AddWithValue("@UpdatedBy",          (object)folder.UpdatedBy ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Folder GetById(int folderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFolderByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapFolder(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<Folder> GetChildren(int parentFolderId)
        {
            var results = new List<Folder>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFolderChildren", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ParentFolderID", parentFolderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapFolder(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<Folder> GetDescendants(int folderId)
        {
            var results = new List<Folder>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFolderDescendants", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapFolder(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<Folder> GetAllRootFolders()
        {
            var results = new List<Folder>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT * FROM storage.Folder WHERE ParentFolderID IS NULL AND IsDeleted = 0 ORDER BY FolderName",
                    conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapFolder(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<Folder> GetAllDeletedFolders()
        {
            var results = new List<Folder>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT * FROM storage.Folder WHERE IsDeleted = 1 ORDER BY DeletedDate DESC",
                    conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapFolder(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<Folder> GetRootFoldersByOrgUnit(int orgUnitId)
        {
            var results = new List<Folder>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetRootFoldersByOrgUnit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapFolder(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<Folder> GetDeletedByOrgUnit(int orgUnitId)
        {
            var results = new List<Folder>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetDeletedFoldersByOrgUnit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapFolder(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<Folder> GetByOrgUnit(int orgUnitId)
        {
            var results = new List<Folder>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFoldersByOrgUnit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapFolder(reader));
                    }
                }
            }
            return results;
        }

        public IEnumerable<Folder> GetAll()
        {
            var results = new List<Folder>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetAllFolders", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapFolder(reader));
                    }
                }
            }
            return results;
        }

        public void SoftDelete(int folderId, int deletedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_SoftDeleteFolder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderID",  folderId);
                    cmd.Parameters.AddWithValue("@DeletedBy", deletedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Restore(int folderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_RestoreFolder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void PermanentDelete(int folderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM storage.Folder WHERE FolderID = @FolderId", conn))
                {
                    cmd.Parameters.AddWithValue("@FolderId", folderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static Folder MapFolder(SqlDataReader reader)
        {
            return new Folder
            {
                FolderID             = (int)reader["FolderID"],
                FolderGUID           = (Guid)reader["FolderGUID"],
                FolderName           = (string)reader["FolderName"],
                ParentFolderID       = reader["ParentFolderID"] == DBNull.Value ? (int?)null : (int)reader["ParentFolderID"],
                OrgUnitID            = (int)reader["OrgUnitID"],
                TemplateID           = reader["TemplateID"] == DBNull.Value ? (int?)null : (int)reader["TemplateID"],
                ConfidentialityLevel = (byte)reader["ConfidentialityLevel"],
                FolderDepth          = (byte)reader["FolderDepth"],
                PhysicalPath         = (string)reader["PhysicalPath"],
                FolderSizeBytes      = (long)reader["FolderSizeBytes"],
                FileCount            = (int)reader["FileCount"],
                IsArchived           = (bool)reader["IsArchived"],
                ArchivedBy           = reader["ArchivedBy"] == DBNull.Value ? (int?)null : (int)reader["ArchivedBy"],
                ArchivedDate         = reader["ArchivedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ArchivedDate"],
                IsDeleted            = (bool)reader["IsDeleted"],
                DeletedBy            = reader["DeletedBy"] == DBNull.Value ? (int?)null : (int)reader["DeletedBy"],
                DeletedDate          = reader["DeletedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DeletedDate"],
                RecycleBinExpiry     = reader["RecycleBinExpiry"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["RecycleBinExpiry"],
                InheritPermission    = (bool)reader["InheritPermission"],
                CreatedBy            = (int)reader["CreatedBy"],
                CreatedDate          = (DateTime)reader["CreatedDate"],
                UpdatedBy            = reader["UpdatedBy"] == DBNull.Value ? (int?)null : (int)reader["UpdatedBy"],
                UpdatedDate          = reader["UpdatedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["UpdatedDate"]
            };
        }
    }
}
