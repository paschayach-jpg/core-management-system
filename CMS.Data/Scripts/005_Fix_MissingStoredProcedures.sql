-- ============================================================
-- Script 005: สร้าง Stored Procedures ที่ขาดหายไป
-- Run script นี้ใน SSMS กับ database SCS
-- ============================================================

USE SCS;
GO

-- ============================================================
-- core.sp_GetAllUserAccounts
-- ใช้โดย UserAccountRepository.GetAll()
-- เรียกตอนเปิด "เพิ่มสิทธิ์" dialog ใน PermissionForm
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'core' AND p.name = 'sp_GetAllUserAccounts')
    DROP PROCEDURE core.sp_GetAllUserAccounts;
GO

CREATE PROCEDURE core.sp_GetAllUserAccounts
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, ADUserName, DisplayName, Email, AppRole, OrgUnitID,
           ConsentGiven, ConsentDate, IsActive, LastLoginDate, CreatedBy, CreatedDate
    FROM   core.UserAccount
    WHERE  IsActive = 1
    ORDER  BY ADUserName;
END
GO

-- ============================================================
-- storage.sp_GetAllFolders
-- ใช้โดย FolderRepository.GetAll()
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_GetAllFolders')
    DROP PROCEDURE storage.sp_GetAllFolders;
GO

CREATE PROCEDURE storage.sp_GetAllFolders
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
           [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
           [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
           [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
    FROM   [storage].[Folder]
    WHERE  [IsDeleted] = 0
    ORDER  BY [OrgUnitID], [FolderDepth], [FolderName];
END
GO

-- ============================================================
-- storage.sp_GetFoldersByOrgUnit
-- ใช้โดย FolderRepository.GetByOrgUnit()
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_GetFoldersByOrgUnit')
    DROP PROCEDURE storage.sp_GetFoldersByOrgUnit;
GO

CREATE PROCEDURE storage.sp_GetFoldersByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
           [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
           [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
           [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
    FROM   [storage].[Folder]
    WHERE  [OrgUnitID] = @OrgUnitID AND [IsDeleted] = 0
    ORDER  BY [FolderDepth], [FolderName];
END
GO

-- ============================================================
-- core.sp_GetOrgUnitByConfidentialityLevel
-- ใช้โดย OrgUnitRepository.GetByConfidentialityLevel()
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'core' AND p.name = 'sp_GetOrgUnitByConfidentialityLevel')
    DROP PROCEDURE core.sp_GetOrgUnitByConfidentialityLevel;
GO

CREATE PROCEDURE core.sp_GetOrgUnitByConfidentialityLevel
    @ConfidentialityLevel TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT ou.OrgUnitID, ou.OrgUnitCode, ou.OrgUnitNameTH, ou.OrgUnitNameEN,
                    ou.OrgUnitType, ou.ParentOrgUnitID, ou.SortOrder,
                    ou.IsActive, ou.IsDeleted, ou.DeletedBy, ou.CreatedBy, ou.CreatedDate
    FROM   core.OrgUnit ou
    INNER  JOIN storage.Folder f ON f.OrgUnitID = ou.OrgUnitID
    WHERE  ou.IsActive = 1 AND ou.IsDeleted = 0
      AND  f.ConfidentialityLevel = @ConfidentialityLevel
      AND  f.IsDeleted = 0;
END
GO

-- ============================================================
-- core.Notification table (ถ้ายังไม่มี)
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables t
               INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
               WHERE s.name = 'core' AND t.name = 'Notification')
BEGIN
    CREATE TABLE core.Notification (
        NotificationId  INT IDENTITY(1,1) NOT NULL,
        RecipientUserId INT               NOT NULL,
        EventType       NVARCHAR(100)     NOT NULL,
        RelatedEntityId NVARCHAR(100)         NULL,
        MessageText     NVARCHAR(1000)    NOT NULL,
        IsRead          BIT               NOT NULL CONSTRAINT DF_core_Notif_IsRead    DEFAULT 0,
        DeliveredAt     DATETIME          NOT NULL CONSTRAINT DF_core_Notif_Delivered DEFAULT GETDATE(),
        ReadAt          DATETIME              NULL,
        CONSTRAINT PK_core_Notification      PRIMARY KEY (NotificationId),
        CONSTRAINT FK_core_Notification_User FOREIGN KEY (RecipientUserId)
            REFERENCES core.UserAccount(UserID)
    );
    CREATE INDEX IX_core_Notif_User ON core.Notification(RecipientUserId, IsRead);
    PRINT 'Created table core.Notification';
END
GO

-- ============================================================
-- core.sp_InsertNotification
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'core' AND p.name = 'sp_InsertNotification')
    DROP PROCEDURE core.sp_InsertNotification;
GO

CREATE PROCEDURE core.sp_InsertNotification
    @RecipientUserId INT,
    @EventType       NVARCHAR(100),
    @RelatedEntityId NVARCHAR(100),
    @MessageText     NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO core.Notification (RecipientUserId, EventType, RelatedEntityId, MessageText)
    VALUES (@RecipientUserId, @EventType, @RelatedEntityId, @MessageText);
    SELECT SCOPE_IDENTITY() AS NewNotificationId;
END
GO

-- ============================================================
-- core.sp_GetUnreadNotificationsByUser
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'core' AND p.name = 'sp_GetUnreadNotificationsByUser')
    DROP PROCEDURE core.sp_GetUnreadNotificationsByUser;
GO

CREATE PROCEDURE core.sp_GetUnreadNotificationsByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NotificationId, RecipientUserId, EventType, RelatedEntityId,
           MessageText, IsRead, DeliveredAt, ReadAt
    FROM   core.Notification
    WHERE  RecipientUserId = @UserId AND IsRead = 0
    ORDER  BY DeliveredAt DESC;
END
GO

-- ============================================================
-- core.sp_MarkNotificationRead
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'core' AND p.name = 'sp_MarkNotificationRead')
    DROP PROCEDURE core.sp_MarkNotificationRead;
GO

CREATE PROCEDURE core.sp_MarkNotificationRead
    @NotificationId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.Notification
    SET    IsRead = 1, ReadAt = GETDATE()
    WHERE  NotificationId = @NotificationId;
END
GO

-- ============================================================
-- storage.sp_PermanentDeleteFolder
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_PermanentDeleteFolder')
    DROP PROCEDURE storage.sp_PermanentDeleteFolder;
GO

CREATE PROCEDURE storage.sp_PermanentDeleteFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Step 1: Collect all descendant folder IDs (including the root) into a temp table
        WITH FolderCTE AS (
            SELECT FolderID FROM storage.Folder WHERE FolderID = @FolderID
            UNION ALL
            SELECT f.FolderID FROM storage.Folder f
            INNER JOIN FolderCTE cte ON f.ParentFolderID = cte.FolderID
        )
        SELECT FolderID INTO #FoldersToDelete FROM FolderCTE;

        -- Step 2: Delete all FileDocument records in those folders (must delete, not soft-delete, to avoid FK constraint)
        DELETE FROM storage.FileDocument
        WHERE  FolderID IN (SELECT FolderID FROM #FoldersToDelete);

        -- Step 3: Delete FolderPermission entries
        DELETE FROM security.FolderPermission
        WHERE  FolderID IN (SELECT FolderID FROM #FoldersToDelete);

        -- Step 4: Delete Folder records (children first by ordering by depth descending)
        DELETE FROM storage.Folder
        WHERE  FolderID IN (SELECT FolderID FROM #FoldersToDelete)
        ORDER BY FolderDepth DESC;

        -- Clean up temp table
        DROP TABLE #FoldersToDelete;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        -- Clean up temp table if it exists
        IF OBJECT_ID('tempdb..#FoldersToDelete') IS NOT NULL
            DROP TABLE #FoldersToDelete;
        THROW;
    END CATCH
END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================
-- storage.sp_UpdateFileDocument
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_UpdateFileDocument')
    DROP PROCEDURE storage.sp_UpdateFileDocument;
GO

CREATE PROCEDURE storage.sp_UpdateFileDocument
    @FileID       INT,
    @FileStatus   TINYINT,
    @ArchivedBy   INT,
    @ArchivedDate DATETIME,
    @DeletedBy    INT,
    @DeletedDate  DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.FileDocument
    SET    FileStatus   = @FileStatus,
           ArchivedBy   = @ArchivedBy,
           ArchivedDate = @ArchivedDate,
           DeletedBy    = @DeletedBy,
           DeletedDate  = @DeletedDate
    WHERE  FileID = @FileID;
END
GO

PRINT '=== Script 005 เสร็จสิ้น: สร้าง SP ที่ขาดหายไปทั้งหมดแล้ว ===';
GO

-- ============================================================
-- storage.sp_GetFileDocumentsByFolder  (patch: FileStatus != 3)
-- แก้ไขจาก FileStatus = 1 เป็น FileStatus != 3
-- เพื่อให้แสดงไฟล์ที่เพิ่งอัพโหลด (VirusScanStatus=0) ด้วย
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_GetFileDocumentsByFolder')
    DROP PROCEDURE storage.sp_GetFileDocumentsByFolder;
GO

CREATE PROCEDURE storage.sp_GetFileDocumentsByFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileID, FileGUID, FolderID, OriginalFileName, StoredFileName, FileExtension,
           MimeType, FileSizeBytes, PhysicalPath, Checksum, VirusScanStatus, VirusScanDate,
           FileStatus, UploadedBy, UploadedDate, ArchivedBy, ArchivedDate,
           DeletedBy, DeletedDate, Version
    FROM   storage.FileDocument
    WHERE  FolderID = @FolderID AND FileStatus != 3;  -- แสดงทุกสถานะยกเว้นที่ถูกลบ
END
GO

PRINT '=== sp_GetFileDocumentsByFolder patched (FileStatus != 3) ===';
GO

-- ============================================================
-- storage.sp_GetAllFileDocumentsGroupedByFolder
-- ดึงไฟล์ทั้งหมดจาก storage.FileDocument พร้อม FolderName
-- เรียงตาม FolderID แล้วตาม OriginalFileName
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_GetAllFileDocumentsGroupedByFolder')
    DROP PROCEDURE storage.sp_GetAllFileDocumentsGroupedByFolder;
GO

CREATE PROCEDURE storage.sp_GetAllFileDocumentsGroupedByFolder
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        fd.FileID, fd.FileGUID, fd.FolderID,
        COALESCE(f.FolderName, N'Folder ' + CAST(fd.FolderID AS NVARCHAR(20))) AS FolderName,
        fd.OriginalFileName, fd.StoredFileName, fd.FileExtension,
        fd.MimeType, fd.FileSizeBytes, fd.PhysicalPath, fd.Checksum,
        fd.VirusScanStatus, fd.VirusScanDate,
        fd.FileStatus, fd.UploadedBy, fd.UploadedDate,
        fd.ArchivedBy, fd.ArchivedDate,
        fd.DeletedBy, fd.DeletedDate, fd.Version
    FROM   storage.FileDocument fd
    LEFT   JOIN storage.Folder f ON f.FolderID = fd.FolderID
    WHERE  fd.FileStatus != 3   -- ไม่รวมที่ถูกลบ
    ORDER  BY fd.FolderID, fd.OriginalFileName;
END
GO

PRINT '=== sp_GetAllFileDocumentsGroupedByFolder created ===';
GO

-- ============================================================
-- storage.sp_GetFileDocumentsByOrgUnit
-- ดึงไฟล์ทั้งหมดใน OrgUnit ที่ระบุ รวม child OrgUnit ทั้งหมด
-- JOIN chain: core.OrgUnit → storage.Folder → storage.FileDocument
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_GetFileDocumentsByOrgUnit')
    DROP PROCEDURE storage.sp_GetFileDocumentsByOrgUnit;
GO

CREATE PROCEDURE storage.sp_GetFileDocumentsByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- รวบรวม OrgUnitID ทั้งหมดที่เป็น child ของ @OrgUnitID (recursive)
    WITH OrgCTE AS (
        SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitID = @OrgUnitID AND IsDeleted = 0
        UNION ALL
        SELECT o.OrgUnitID FROM core.OrgUnit o
        INNER JOIN OrgCTE cte ON o.ParentOrgUnitID = cte.OrgUnitID
        WHERE o.IsDeleted = 0
    )
    SELECT
        fd.FileID, fd.FileGUID, fd.FolderID,
        COALESCE(f.FolderName, N'Folder ' + CAST(fd.FolderID AS NVARCHAR(20))) AS FolderName,
        fd.OriginalFileName, fd.StoredFileName, fd.FileExtension,
        fd.MimeType, fd.FileSizeBytes, fd.PhysicalPath, fd.Checksum,
        fd.VirusScanStatus, fd.VirusScanDate,
        fd.FileStatus, fd.UploadedBy, fd.UploadedDate,
        fd.ArchivedBy, fd.ArchivedDate,
        fd.DeletedBy, fd.DeletedDate, fd.Version
    FROM   storage.FileDocument fd
    INNER  JOIN storage.Folder f  ON f.FolderID  = fd.FolderID
    INNER  JOIN OrgCTE            ON OrgCTE.OrgUnitID = f.OrgUnitID
    WHERE  fd.FileStatus != 3
      AND  f.IsDeleted = 0
    ORDER  BY f.FolderName, fd.OriginalFileName;
END
GO

PRINT '=== sp_GetFileDocumentsByOrgUnit created ===';
GO

-- ============================================================
-- security.sp_GetPermissionsByFolder  (patch: ensure exists)
-- ดึงสิทธิ์ทั้งหมดของ Folder ที่ยังไม่ถูก Revoke และยังไม่หมดอายุ
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'security' AND p.name = 'sp_GetPermissionsByFolder')
    DROP PROCEDURE security.sp_GetPermissionsByFolder;
GO

CREATE PROCEDURE security.sp_GetPermissionsByFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PermissionID, FolderID, PrincipalType, PrincipalID,
           CanRead, CanWrite, CanDelete, CanModify, CanFullControl, IsDeny,
           ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
    FROM   security.FolderPermission
    WHERE  FolderID    = @FolderID
      AND  RevokedDate IS NULL
      AND  (ExpiryDate IS NULL OR ExpiryDate > GETDATE());
END
GO

PRINT '=== security.sp_GetPermissionsByFolder patched ===';
GO
