using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class FileDocumentRepository : IFileDocumentRepository
    {
        private readonly string _connectionString;

        public FileDocumentRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(FileDocument document)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_InsertFileDocument", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderID",         document.FolderID);
                    cmd.Parameters.AddWithValue("@OriginalFileName", document.OriginalFileName);
                    cmd.Parameters.AddWithValue("@StoredFileName",   document.StoredFileName);
                    cmd.Parameters.AddWithValue("@FileExtension",    document.FileExtension);
                    cmd.Parameters.AddWithValue("@MimeType",         document.MimeType);
                    cmd.Parameters.AddWithValue("@FileSizeBytes",    document.FileSizeBytes);
                    cmd.Parameters.AddWithValue("@PhysicalPath",     document.PhysicalPath);
                    cmd.Parameters.AddWithValue("@Checksum",         document.Checksum);
                    cmd.Parameters.AddWithValue("@UploadedBy",       document.UploadedBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public FileDocument GetById(int fileId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFileDocumentByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileID", fileId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapFileDocument(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<FileDocument> GetByFolder(int folderId)
        {
            var results = new List<FileDocument>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFileDocumentsByFolder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) results.Add(MapFileDocument(reader));
                    }
                }
            }
            return results;
        }

        public void UpdateVirusScanStatus(int fileId, int virusScanStatus)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_UpdateVirusScanStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileID",          fileId);
                    cmd.Parameters.AddWithValue("@VirusScanStatus", (byte)virusScanStatus);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(FileDocument document)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_UpdateFileDocument", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileID",       document.FileID);
                    cmd.Parameters.AddWithValue("@FolderID",     document.FolderID);
                    cmd.Parameters.AddWithValue("@PhysicalPath", (object)document.PhysicalPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileStatus",   (byte)document.FileStatus);
                    cmd.Parameters.AddWithValue("@ArchivedBy",   (object)document.ArchivedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ArchivedDate", (object)document.ArchivedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeletedBy",    (object)document.DeletedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeletedDate",  (object)document.DeletedDate ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SoftDelete(int fileId, int deletedBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_SoftDeleteFileDocument", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileID",    fileId);
                    cmd.Parameters.AddWithValue("@DeletedBy", deletedBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void PermanentDelete(int fileId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM storage.FileDocument WHERE FileID = @FileID", conn))
                {
                    cmd.Parameters.AddWithValue("@FileID", fileId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public FileDocument GetByChecksum(string checksum, int folderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFileDocumentByChecksum", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Checksum", checksum);
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapFileDocument(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<FileDocument> GetAllGroupedByFolder()
        {
            var results = new List<FileDocument>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetAllFileDocumentsGroupedByFolder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var doc = MapFileDocument(reader);
                            doc.FolderName = reader["FolderName"] as string;
                            results.Add(doc);
                        }
                    }
                }
            }
            return results;
        }

        public IEnumerable<FileDocument> GetByOrgUnit(int orgUnitId)
        {
            var results = new List<FileDocument>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_GetFileDocumentsByOrgUnit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OrgUnitID", orgUnitId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var doc = MapFileDocument(reader);
                            doc.FolderName = reader["FolderName"] as string;
                            results.Add(doc);
                        }
                    }
                }
            }
            return results;
        }

        public void DeleteAllByFolder(int folderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "DELETE FROM storage.FileDocument WHERE FolderID = @FolderID", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@FolderID", folderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void RenameFile(int fileId, string newName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("storage.sp_RenameFileDocument", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileID",           fileId);
                    cmd.Parameters.AddWithValue("@OriginalFileName", newName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static FileDocument MapFileDocument(SqlDataReader reader)
        {
            var doc = new FileDocument
            {
                FileID           = (int)reader["FileID"],
                FileGUID         = (Guid)reader["FileGUID"],
                FolderID         = (int)reader["FolderID"],
                OriginalFileName = (string)reader["OriginalFileName"],
                StoredFileName   = (string)reader["StoredFileName"],
                FileExtension    = (string)reader["FileExtension"],
                MimeType         = (string)reader["MimeType"],
                FileSizeBytes    = (long)reader["FileSizeBytes"],
                PhysicalPath     = (string)reader["PhysicalPath"],
                Checksum         = (string)reader["Checksum"],
                VirusScanStatus  = (byte)reader["VirusScanStatus"],
                VirusScanDate    = reader["VirusScanDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["VirusScanDate"],
                FileStatus       = (byte)reader["FileStatus"],
                UploadedBy       = (int)reader["UploadedBy"],
                UploadedDate     = (DateTime)reader["UploadedDate"],
                ArchivedBy       = reader["ArchivedBy"] == DBNull.Value ? (int?)null : (int)reader["ArchivedBy"],
                ArchivedDate     = reader["ArchivedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ArchivedDate"],
                DeletedBy        = reader["DeletedBy"] == DBNull.Value ? (int?)null : (int)reader["DeletedBy"],
                DeletedDate      = reader["DeletedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DeletedDate"],
                Version          = (int)reader["Version"]
            };

            // FolderName — optional column (present in newer SPs)
            try { doc.FolderName = reader["FolderName"] as string; } catch { }

            // UploadedByName — optional column (added in script 070)
            try { doc.UploadedByName = reader["UploadedByName"] as string; } catch { }

            return doc;
        }
    }
}
