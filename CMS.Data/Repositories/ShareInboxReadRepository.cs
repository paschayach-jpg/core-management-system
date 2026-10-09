using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class ShareInboxReadRepository : IShareInboxReadRepository
    {
        private readonly string _connectionString;
        private static bool _schemaEnsured = false;
        private static readonly object _schemaLock = new object();

        public ShareInboxReadRepository(IConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public ShareInboxReadRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private void EnsureSchema()
        {
            if (_schemaEnsured) return;
            lock (_schemaLock)
            {
                if (_schemaEnsured) return;
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'security')
BEGIN
    EXEC('CREATE SCHEMA security AUTHORIZATION dbo;');
END

IF OBJECT_ID('security.ShareInboxRead', 'U') IS NULL
BEGIN
    CREATE TABLE security.ShareInboxRead (
        UserID       INT      NOT NULL,
        ShareKind    TINYINT  NOT NULL,
        PermissionID INT      NOT NULL,
        ReadDate     DATETIME NOT NULL CONSTRAINT DF_ShareInboxRead_ReadDate DEFAULT GETDATE(),
        CONSTRAINT PK_ShareInboxRead PRIMARY KEY CLUSTERED (UserID, ShareKind, PermissionID)
    );

    CREATE NONCLUSTERED INDEX IX_ShareInboxRead_User 
    ON security.ShareInboxRead (UserID, ShareKind, PermissionID);

    -- Baseline: Mark items older than 7 days as read
    IF OBJECT_ID('security.FolderPermission', 'U') IS NOT NULL
    BEGIN
        INSERT INTO security.ShareInboxRead (UserID, ShareKind, PermissionID, ReadDate)
        SELECT DISTINCT fp.PrincipalID, 1, fp.PermissionID, GETDATE()
        FROM security.FolderPermission fp
        WHERE fp.PrincipalType = 1
          AND fp.GrantedDate IS NOT NULL
          AND fp.GrantedDate < DATEADD(day, -7, GETDATE())
          AND NOT EXISTS (
              SELECT 1 FROM security.ShareInboxRead r
              WHERE r.UserID = fp.PrincipalID AND r.ShareKind = 1 AND r.PermissionID = fp.PermissionID
          );
    END

    IF OBJECT_ID('security.FilePermission', 'U') IS NOT NULL
    BEGIN
        INSERT INTO security.ShareInboxRead (UserID, ShareKind, PermissionID, ReadDate)
        SELECT DISTINCT fp.PrincipalID, 2, fp.FilePermissionID, GETDATE()
        FROM security.FilePermission fp
        WHERE fp.PrincipalType = 1
          AND fp.GrantedDate IS NOT NULL
          AND fp.GrantedDate < DATEADD(day, -7, GETDATE())
          AND NOT EXISTS (
              SELECT 1 FROM security.ShareInboxRead r
              WHERE r.UserID = fp.PrincipalID AND r.ShareKind = 2 AND r.PermissionID = fp.FilePermissionID
          );
    END
END", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    _schemaEnsured = true;
                }
                catch
                {
                    // If schema creation fails (e.g. read-only user), mark ensured to prevent retry loops
                    _schemaEnsured = true;
                }
            }
        }

        public IList<ShareInboxReadKey> GetReadKeys(int userId)
        {
            EnsureSchema();
            var list = new List<ShareInboxReadKey>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
SELECT UserID, ShareKind, PermissionID, ReadDate
FROM security.ShareInboxRead
WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ShareInboxReadKey
                            {
                                UserID = reader.GetInt32(0),
                                ShareKind = reader.GetByte(1),
                                PermissionID = reader.GetInt32(2),
                                ReadDate = reader.GetDateTime(3)
                            });
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on failure / table not present
            }

            return list;
        }

        public void MarkRead(int userId, byte shareKind, int permissionId)
        {
            EnsureSchema();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
IF NOT EXISTS (
    SELECT 1 FROM security.ShareInboxRead 
    WHERE UserID = @UserID AND ShareKind = @ShareKind AND PermissionID = @PermissionID
)
BEGIN
    INSERT INTO security.ShareInboxRead (UserID, ShareKind, PermissionID, ReadDate)
    VALUES (@UserID, @ShareKind, @PermissionID, GETDATE())
END", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ShareKind", shareKind);
                    cmd.Parameters.AddWithValue("@PermissionID", permissionId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Safe ignore
            }
        }

        public void MarkReadMany(int userId, IEnumerable<ShareInboxReadKey> keys)
        {
            if (keys == null) return;
            EnsureSchema();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        const string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM security.ShareInboxRead 
    WHERE UserID = @UserID AND ShareKind = @ShareKind AND PermissionID = @PermissionID
)
BEGIN
    INSERT INTO security.ShareInboxRead (UserID, ShareKind, PermissionID, ReadDate)
    VALUES (@UserID, @ShareKind, @PermissionID, GETDATE())
END";
                        foreach (var key in keys)
                        {
                            if (key == null) continue;
                            using (var cmd = new SqlCommand(sql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.Parameters.AddWithValue("@ShareKind", key.ShareKind);
                                cmd.Parameters.AddWithValue("@PermissionID", key.PermissionID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
            }
            catch
            {
                // Safe ignore
            }
        }
    }
}
