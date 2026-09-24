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
    /// storage.t_project_attachment — RGP project &lt;-&gt; DMS file link table.
    /// Same connection string as the DMS repositories ("ConnectionString"), so the
    /// JOIN onto storage.FileDocument / storage.Folder runs in-database.
    /// </summary>
    public class ProjectAttachmentRepository : IProjectAttachmentRepository
    {
        private readonly IConfigManager _configManager;

        public ProjectAttachmentRepository(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        private SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(_configManager.Get<string>("ConnectionString"));
            conn.Open();
            return conn;
        }

        public IList<ProjectAttachmentModel> GetByProject(int projectId)
        {
            var results = new List<ProjectAttachmentModel>();
            const string sql = @"
                SELECT  a.i_attach_id,
                        a.i_project_id,
                        a.i_file_id,
                        a.i_create_user,
                        a.d_create_date,
                        fd.OriginalFileName     AS n_file_name,
                        fd.FileExtension        AS c_file_ext,
                        fd.FileSizeBytes        AS l_file_size_bytes,
                        fd.UploadedBy           AS i_uploaded_by,
                        fd.UploadedDate         AS d_uploaded_date,
                        ISNULL(f.ConfidentialityLevel, 0) AS i_confidentiality_level,
                        f.FolderName            AS n_folder_name
                FROM    storage.t_project_attachment a
                JOIN    storage.FileDocument fd ON fd.FileID = a.i_file_id AND fd.FileStatus <> 3
                LEFT JOIN storage.Folder     f  ON f.FolderID = fd.FolderID
                WHERE   a.i_project_id = @pid
                ORDER BY a.d_create_date DESC";

            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", projectId);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        results.Add(Map(r));
                }
            }
            return results;
        }

        public int Insert(int projectId, int fileId, string user)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM storage.t_project_attachment
                               WHERE i_project_id = @pid AND i_file_id = @fid)
                BEGIN
                    INSERT INTO storage.t_project_attachment
                        (i_project_id, i_file_id, i_create_user, d_create_date)
                    VALUES (@pid, @fid, @usr, GETDATE());
                END
                SELECT i_attach_id FROM storage.t_project_attachment
                WHERE i_project_id = @pid AND i_file_id = @fid;";

            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", projectId);
                cmd.Parameters.AddWithValue("@fid", fileId);
                cmd.Parameters.AddWithValue("@usr", user ?? string.Empty);
                object result = cmd.ExecuteScalar();
                return (result == null || result == DBNull.Value) ? -1 : Convert.ToInt32(result);
            }
        }

        public bool DeleteByProjectAndFile(int projectId, int fileId)
        {
            const string sql = @"DELETE FROM storage.t_project_attachment
                                 WHERE i_project_id = @pid AND i_file_id = @fid";
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", projectId);
                cmd.Parameters.AddWithValue("@fid", fileId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public int DeleteByProject(int projectId)
        {
            const string sql = @"DELETE FROM storage.t_project_attachment WHERE i_project_id = @pid";
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", projectId);
                return cmd.ExecuteNonQuery();
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static ProjectAttachmentModel Map(IDataReader r)
        {
            return new ProjectAttachmentModel
            {
                i_attach_id             = r.GetInt32(r.GetOrdinal("i_attach_id")),
                i_project_id            = r.GetInt32(r.GetOrdinal("i_project_id")),
                i_file_id               = r.GetInt32(r.GetOrdinal("i_file_id")),
                i_create_user           = r["i_create_user"] as string,
                d_create_date           = GetDate(r, "d_create_date"),
                n_file_name             = r["n_file_name"] as string,
                c_file_ext              = r["c_file_ext"] as string,
                l_file_size_bytes       = r.IsDBNull(r.GetOrdinal("l_file_size_bytes")) ? 0L : Convert.ToInt64(r["l_file_size_bytes"]),
                i_uploaded_by           = r.IsDBNull(r.GetOrdinal("i_uploaded_by")) ? 0 : Convert.ToInt32(r["i_uploaded_by"]),
                d_uploaded_date         = GetDate(r, "d_uploaded_date"),
                i_confidentiality_level = r.IsDBNull(r.GetOrdinal("i_confidentiality_level")) ? 0 : Convert.ToInt32(r["i_confidentiality_level"]),
                n_folder_name           = r["n_folder_name"] as string
            };
        }

        private static DateTime GetDate(IDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? DateTime.MinValue : r.GetDateTime(i);
        }
    }
}
