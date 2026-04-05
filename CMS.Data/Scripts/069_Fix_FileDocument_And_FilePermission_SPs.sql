-- ============================================================
-- 069_Fix_FileDocument_And_FilePermission_SPs.sql
-- แก้ปัญหา "ไม่พบข้อมูลในระบบ" ใน SharedFileViewForm
-- สร้าง/แทนที่ SP ที่จำเป็นสำหรับ File + FilePermission
-- Run ซ้ำได้ปลอดภัย (DROP IF EXISTS + CREATE)
-- ============================================================

-- ============================================================
-- 0. Ensure schemas exist
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'storage')
    EXEC('CREATE SCHEMA storage');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'security')
    EXEC('CREATE SCHEMA security');
GO

-- ============================================================
-- 1. storage.FileDocument table (ถ้ายังไม่มี)
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = 'storage' AND t.name = 'FileDocument'
)
BEGIN
    CREATE TABLE storage.FileDocument (
        FileID            INT           IDENTITY(1,1) PRIMARY KEY,
        FileGUID          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        FolderID          INT           NOT NULL,
        OriginalFileName  NVARCHAR(260) NOT NULL,
        StoredFileName    NVARCHAR(260) NOT NULL,
        FileExtension     NVARCHAR(20)  NOT NULL,
        MimeType          NVARCHAR(100) NOT NULL,
        FileSizeBytes     BIGINT        NOT NULL DEFAULT 0,
        PhysicalPath      NVARCHAR(500) NOT NULL,
        Checksum          VARCHAR(64)   NULL,
        VirusScanStatus   TINYINT       NOT NULL DEFAULT 0,
        VirusScanDate     DATETIME      NULL,
        FileStatus        TINYINT       NOT NULL DEFAULT 1,
        UploadedBy        INT           NOT NULL,
        UploadedDate      DATETIME      NOT NULL DEFAULT GETDATE(),
        ArchivedBy        INT           NULL,
        ArchivedDate      DATETIME      NULL,
        DeletedBy         INT           NULL,
        DeletedDate       DATETIME      NULL,
        Version           INT           NOT NULL DEFAULT 1
    );
    PRINT 'Created table storage.FileDocument';
END
ELSE
    PRINT 'Table storage.FileDocument already exists';
GO

-- ============================================================
-- 2. security.FilePermission table (ถ้ายังไม่มี)
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = 'security' AND t.name = 'FilePermission'
)
BEGIN
    CREATE TABLE security.FilePermission (
        FilePermissionID INT      IDENTITY(1,1) PRIMARY KEY,
        FileID           INT      NOT NULL,
        PrincipalType    TINYINT  NOT NULL,  -- 1=User, 2=Role, 3=OrgUnit, 4=Group
        PrincipalID      INT      NOT NULL,
        CanRead          BIT      NOT NULL DEFAULT 0,
        CanWrite         BIT      NOT NULL DEFAULT 0,
        CanDelete        BIT      NOT NULL DEFAULT 0,
        CanModify        BIT      NOT NULL DEFAULT 0,
        CanFullControl   BIT      NOT NULL DEFAULT 0,
        IsDeny           BIT      NOT NULL DEFAULT 0,
        ExpiryDate       DATETIME NULL,
        GrantedBy        INT      NOT NULL,
        GrantedDate      DATETIME NOT NULL DEFAULT GETDATE(),
        RevokedBy        INT      NULL,
        RevokedDate      DATETIME NULL,
        RevokeReason     NVARCHAR(500) NULL
    );
    CREATE INDEX IX_FilePermission_FileID    ON security.FilePermission(FileID);
    CREATE INDEX IX_FilePermission_Principal ON security.FilePermission(PrincipalType, PrincipalID);
    PRINT 'Created table security.FilePermission';
END
ELSE
    PRINT 'Table security.FilePermission already exists';
GO

-- ============================================================
-- 3. storage.sp_GetFileDocumentByID
-- ============================================================
IF OBJECT_ID('storage.sp_GetFileDocumentByID', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFileDocumentByID;
GO
CREATE PROCEDURE storage.sp_GetFileDocumentByID
    @FileID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FileID, f.FileGUID, f.FolderID, f.OriginalFileName, f.StoredFileName,
           f.FileExtension, f.MimeType, f.FileSizeBytes, f.PhysicalPath, f.Checksum,
           f.VirusScanStatus, f.VirusScanDate, f.FileStatus,
           f.UploadedBy, f.UploadedDate,
           f.ArchivedBy, f.ArchivedDate,
           f.DeletedBy, f.DeletedDate, f.Version,
           ISNULL(fo.FolderName, '') AS FolderName
    FROM   storage.FileDocument f
    LEFT JOIN storage.Folder fo ON fo.FolderID = f.FolderID
    WHERE  f.FileID = @FileID;
END
GO
PRINT 'Created storage.sp_GetFileDocumentByID';

-- ============================================================
-- 4. storage.sp_GetFileDocumentsByFolder
-- ============================================================
IF OBJECT_ID('storage.sp_GetFileDocumentsByFolder', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFileDocumentsByFolder;
GO
CREATE PROCEDURE storage.sp_GetFileDocumentsByFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FileID, f.FileGUID, f.FolderID, f.OriginalFileName, f.StoredFileName,
           f.FileExtension, f.MimeType, f.FileSizeBytes, f.PhysicalPath, f.Checksum,
           f.VirusScanStatus, f.VirusScanDate, f.FileStatus,
           f.UploadedBy, f.UploadedDate,
           f.ArchivedBy, f.ArchivedDate,
           f.DeletedBy, f.DeletedDate, f.Version,
           ISNULL(fo.FolderName, '') AS FolderName
    FROM   storage.FileDocument f
    LEFT JOIN storage.Folder fo ON fo.FolderID = f.FolderID
    WHERE  f.FolderID = @FolderID
      AND  f.FileStatus != 3;   -- ยกเว้นที่ถูกลบถาวร
END
GO
PRINT 'Created storage.sp_GetFileDocumentsByFolder';

-- ============================================================
-- 5. security.sp_InsertFilePermission
-- ============================================================
IF OBJECT_ID('security.sp_InsertFilePermission', 'P') IS NOT NULL
    DROP PROCEDURE security.sp_InsertFilePermission;
GO
CREATE PROCEDURE security.sp_InsertFilePermission
    @FileID         INT,
    @PrincipalType  TINYINT,
    @PrincipalID    INT,
    @CanRead        BIT,
    @CanWrite       BIT,
    @CanDelete      BIT,
    @CanModify      BIT,
    @CanFullControl BIT,
    @IsDeny         BIT,
    @ExpiryDate     DATETIME = NULL,
    @GrantedBy      INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO security.FilePermission
        (FileID, PrincipalType, PrincipalID, CanRead, CanWrite, CanDelete,
         CanModify, CanFullControl, IsDeny, ExpiryDate, GrantedBy, GrantedDate)
    VALUES
        (@FileID, @PrincipalType, @PrincipalID, @CanRead, @CanWrite, @CanDelete,
         @CanModify, @CanFullControl, @IsDeny, @ExpiryDate, @GrantedBy, GETDATE());
    SELECT SCOPE_IDENTITY() AS NewFilePermissionID;
END
GO
PRINT 'Created security.sp_InsertFilePermission';

-- ============================================================
-- 6. security.sp_GetFilePermissionsByFile (active only)
-- ============================================================
IF OBJECT_ID('security.sp_GetFilePermissionsByFile', 'P') IS NOT NULL
    DROP PROCEDURE security.sp_GetFilePermissionsByFile;
GO
CREATE PROCEDURE security.sp_GetFilePermissionsByFile
    @FileID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FilePermissionID, FileID, PrincipalType, PrincipalID,
           CanRead, CanWrite, CanDelete, CanModify, CanFullControl, IsDeny,
           ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
    FROM   security.FilePermission
    WHERE  FileID = @FileID AND RevokedDate IS NULL;
END
GO
PRINT 'Created security.sp_GetFilePermissionsByFile';

-- ============================================================
-- 7. security.sp_GetAllFilePermissionsByFile (รวม revoked)
-- ============================================================
IF OBJECT_ID('security.sp_GetAllFilePermissionsByFile', 'P') IS NOT NULL
    DROP PROCEDURE security.sp_GetAllFilePermissionsByFile;
GO
CREATE PROCEDURE security.sp_GetAllFilePermissionsByFile
    @FileID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FilePermissionID, FileID, PrincipalType, PrincipalID,
           CanRead, CanWrite, CanDelete, CanModify, CanFullControl, IsDeny,
           ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
    FROM   security.FilePermission
    WHERE  FileID = @FileID
    ORDER  BY GrantedDate DESC;
END
GO
PRINT 'Created security.sp_GetAllFilePermissionsByFile';

-- ============================================================
-- 8. security.sp_RevokeFilePermission
-- ============================================================
IF OBJECT_ID('security.sp_RevokeFilePermission', 'P') IS NOT NULL
    DROP PROCEDURE security.sp_RevokeFilePermission;
GO
CREATE PROCEDURE security.sp_RevokeFilePermission
    @FilePermissionID INT,
    @RevokedBy        INT,
    @RevokeReason     NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE security.FilePermission
    SET    RevokedBy   = @RevokedBy,
           RevokedDate = GETDATE(),
           RevokeReason = @RevokeReason
    WHERE  FilePermissionID = @FilePermissionID;
END
GO
PRINT 'Created security.sp_RevokeFilePermission';

PRINT '=== Script 069 completed successfully ===';
