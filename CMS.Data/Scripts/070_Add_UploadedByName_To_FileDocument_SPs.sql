-- ============================================================
-- Script: 070_Add_UploadedByName_To_FileDocument_SPs.sql
-- Purpose: แก้ไข stored procedures ที่ดึงข้อมูล FileDocument และ Folder
--          ให้ JOIN กับ core.UserAccount เพื่อแสดงชื่อผู้ upload/สร้าง
-- ============================================================

USE [SCS];
GO

-- ============================================================
-- PART 1: FileDocument Stored Procedures
-- ============================================================

-- ============================================================
-- 1. storage.sp_GetFileDocumentsByFolder
--    เพิ่ม UploadedByName จาก core.UserAccount
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
           ISNULL(fo.FolderName, '') AS FolderName,
           ISNULL(u.DisplayName, u.ADUserName) AS UploadedByName
    FROM   storage.FileDocument f
    LEFT JOIN storage.Folder fo ON fo.FolderID = f.FolderID
    LEFT JOIN core.UserAccount u ON u.UserID = f.UploadedBy
    WHERE  f.FolderID = @FolderID
      AND  f.FileStatus != 3;   -- ยกเว้นที่ถูกลบถาวร
END
GO
PRINT 'Updated storage.sp_GetFileDocumentsByFolder with UploadedByName';
GO

-- ============================================================
-- 2. storage.sp_GetAllFileDocumentsGroupedByFolder
--    เพิ่ม UploadedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetAllFileDocumentsGroupedByFolder', 'P') IS NOT NULL
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
        fd.DeletedBy, fd.DeletedDate, fd.Version,
        ISNULL(u.DisplayName, u.ADUserName) AS UploadedByName
    FROM   storage.FileDocument fd
    LEFT   JOIN storage.Folder f ON f.FolderID = fd.FolderID
    LEFT   JOIN core.UserAccount u ON u.UserID = fd.UploadedBy
    WHERE  fd.FileStatus != 3   -- ไม่รวมที่ถูกลบ
    ORDER  BY fd.FolderID, fd.OriginalFileName;
END
GO
PRINT 'Updated storage.sp_GetAllFileDocumentsGroupedByFolder with UploadedByName';
GO

-- ============================================================
-- 3. storage.sp_GetFileDocumentByID
--    เพิ่ม UploadedByName จาก core.UserAccount
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
           ISNULL(u.DisplayName, u.ADUserName) AS UploadedByName
    FROM   storage.FileDocument f
    LEFT   JOIN core.UserAccount u ON u.UserID = f.UploadedBy
    WHERE  f.FileID = @FileID;
END
GO
PRINT 'Updated storage.sp_GetFileDocumentByID with UploadedByName';
GO

-- ============================================================
-- 4. storage.sp_GetFileDocumentsByOrgUnit
--    เพิ่ม UploadedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetFileDocumentsByOrgUnit', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFileDocumentsByOrgUnit;
GO

CREATE PROCEDURE storage.sp_GetFileDocumentsByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT fd.FileID, fd.FileGUID, fd.FolderID,
           COALESCE(f.FolderName, N'Folder ' + CAST(fd.FolderID AS NVARCHAR(20))) AS FolderName,
           fd.OriginalFileName, fd.StoredFileName, fd.FileExtension,
           fd.MimeType, fd.FileSizeBytes, fd.PhysicalPath, fd.Checksum,
           fd.VirusScanStatus, fd.VirusScanDate,
           fd.FileStatus, fd.UploadedBy, fd.UploadedDate,
           fd.ArchivedBy, fd.ArchivedDate,
           fd.DeletedBy, fd.DeletedDate, fd.Version,
           ISNULL(u.DisplayName, u.ADUserName) AS UploadedByName
    FROM   storage.FileDocument fd
    INNER  JOIN storage.Folder f ON f.FolderID = fd.FolderID
    LEFT   JOIN core.UserAccount u ON u.UserID = fd.UploadedBy
    WHERE  f.OrgUnitID = @OrgUnitID
      AND  fd.FileStatus != 3   -- ไม่รวมที่ถูกลบ
    ORDER  BY fd.FolderID, fd.OriginalFileName;
END
GO
PRINT 'Updated storage.sp_GetFileDocumentsByOrgUnit with UploadedByName';
GO

PRINT '';
PRINT '=== Script 070 completed successfully ===';
PRINT 'All FileDocument stored procedures now include UploadedByName';
GO

-- ============================================================
-- PART 2: Folder Stored Procedures
-- ============================================================

-- ============================================================
-- 5. storage.sp_GetFolderByID
--    เพิ่ม CreatedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetFolderByID', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFolderByID;
GO

CREATE PROCEDURE storage.sp_GetFolderByID
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes, f.FileCount,
           f.IsArchived, f.ArchivedBy, f.ArchivedDate,
           f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
           f.InheritPermission,
           f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate,
           ISNULL(u.DisplayName, u.ADUserName) AS CreatedByName
    FROM   storage.Folder f
    LEFT   JOIN core.UserAccount u ON u.UserID = f.CreatedBy
    WHERE  f.FolderID = @FolderID;
END
GO
PRINT 'Updated storage.sp_GetFolderByID with CreatedByName';
GO

-- ============================================================
-- 6. storage.sp_GetAllFolders
--    เพิ่ม CreatedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetAllFolders', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetAllFolders;
GO

CREATE PROCEDURE storage.sp_GetAllFolders
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes, f.FileCount,
           f.IsArchived, f.ArchivedBy, f.ArchivedDate,
           f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
           f.InheritPermission,
           f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate,
           ISNULL(u.DisplayName, u.ADUserName) AS CreatedByName
    FROM   storage.Folder f
    LEFT   JOIN core.UserAccount u ON u.UserID = f.CreatedBy
    WHERE  f.IsDeleted = 0
    ORDER  BY f.FolderDepth, f.FolderName;
END
GO
PRINT 'Updated storage.sp_GetAllFolders with CreatedByName';
GO

PRINT '';
PRINT '=== Script 070 completed successfully ===';
PRINT 'All FileDocument and Folder stored procedures now include user names';
GO

-- ============================================================
-- 7. storage.sp_GetFolderChildren
--    เพิ่ม CreatedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetFolderChildren', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFolderChildren;
GO

CREATE PROCEDURE storage.sp_GetFolderChildren
    @ParentFolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes, f.FileCount,
           f.IsArchived, f.ArchivedBy, f.ArchivedDate,
           f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
           f.InheritPermission,
           f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate,
           ISNULL(u.DisplayName, u.ADUserName) AS CreatedByName
    FROM   storage.Folder f
    LEFT   JOIN core.UserAccount u ON u.UserID = f.CreatedBy
    WHERE  f.ParentFolderID = @ParentFolderID
      AND  f.IsDeleted = 0;
END
GO
PRINT 'Updated storage.sp_GetFolderChildren with CreatedByName';
GO

-- ============================================================
-- 8. storage.sp_GetFolderDescendants
--    เพิ่ม CreatedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetFolderDescendants', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFolderDescendants;
GO

CREATE PROCEDURE storage.sp_GetFolderDescendants
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH FolderHierarchy AS (
        -- Anchor: เริ่มจาก folder ที่ระบุ
        SELECT FolderID, FolderGUID, FolderName, ParentFolderID,
               OrgUnitID, TemplateID, ConfidentialityLevel, FolderDepth,
               PhysicalPath, FolderSizeBytes, FileCount,
               IsArchived, ArchivedBy, ArchivedDate,
               IsDeleted, DeletedBy, DeletedDate, RecycleBinExpiry,
               InheritPermission,
               CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   storage.Folder
        WHERE  FolderID = @FolderID
        
        UNION ALL
        
        -- Recursive: ดึง children ทั้งหมด
        SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
               f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
               f.PhysicalPath, f.FolderSizeBytes, f.FileCount,
               f.IsArchived, f.ArchivedBy, f.ArchivedDate,
               f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
               f.InheritPermission,
               f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate
        FROM   storage.Folder f
        INNER  JOIN FolderHierarchy fh ON f.ParentFolderID = fh.FolderID
    )
    SELECT fh.FolderID, fh.FolderGUID, fh.FolderName, fh.ParentFolderID,
           fh.OrgUnitID, fh.TemplateID, fh.ConfidentialityLevel, fh.FolderDepth,
           fh.PhysicalPath, fh.FolderSizeBytes, fh.FileCount,
           fh.IsArchived, fh.ArchivedBy, fh.ArchivedDate,
           fh.IsDeleted, fh.DeletedBy, fh.DeletedDate, fh.RecycleBinExpiry,
           fh.InheritPermission,
           fh.CreatedBy, fh.CreatedDate, fh.UpdatedBy, fh.UpdatedDate,
           ISNULL(u.DisplayName, u.ADUserName) AS CreatedByName
    FROM   FolderHierarchy fh
    LEFT   JOIN core.UserAccount u ON u.UserID = fh.CreatedBy
    WHERE  fh.FolderID != @FolderID  -- ไม่รวม folder ต้นทาง
    ORDER  BY fh.FolderDepth, fh.FolderName;
END
GO
PRINT 'Updated storage.sp_GetFolderDescendants with CreatedByName';
GO

-- ============================================================
-- 9. storage.sp_GetRootFoldersByOrgUnit
--    เพิ่ม CreatedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetRootFoldersByOrgUnit', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetRootFoldersByOrgUnit;
GO

CREATE PROCEDURE storage.sp_GetRootFoldersByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes, f.FileCount,
           f.IsArchived, f.ArchivedBy, f.ArchivedDate,
           f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
           f.InheritPermission,
           f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate,
           ISNULL(u.DisplayName, u.ADUserName) AS CreatedByName
    FROM   storage.Folder f
    LEFT   JOIN core.UserAccount u ON u.UserID = f.CreatedBy
    WHERE  f.OrgUnitID = @OrgUnitID
      AND  (f.ParentFolderID IS NULL OR f.ParentFolderID = 0)
      AND  f.IsDeleted = 0
    ORDER  BY f.FolderName;
END
GO
PRINT 'Updated storage.sp_GetRootFoldersByOrgUnit with CreatedByName';
GO

-- ============================================================
-- 10. storage.sp_GetFoldersByOrgUnit
--     เพิ่ม CreatedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetFoldersByOrgUnit', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFoldersByOrgUnit;
GO

CREATE PROCEDURE storage.sp_GetFoldersByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes, f.FileCount,
           f.IsArchived, f.ArchivedBy, f.ArchivedDate,
           f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
           f.InheritPermission,
           f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate,
           ISNULL(u.DisplayName, u.ADUserName) AS CreatedByName
    FROM   storage.Folder f
    LEFT   JOIN core.UserAccount u ON u.UserID = f.CreatedBy
    WHERE  f.OrgUnitID = @OrgUnitID
      AND  f.IsDeleted = 0
    ORDER  BY f.FolderDepth, f.FolderName;
END
GO
PRINT 'Updated storage.sp_GetFoldersByOrgUnit with CreatedByName';
GO

-- ============================================================
-- 11. storage.sp_GetActiveFolders
--     เพิ่ม CreatedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetActiveFolders', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetActiveFolders;
GO

CREATE PROCEDURE storage.sp_GetActiveFolders
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes, f.FileCount,
           f.IsArchived, f.ArchivedBy, f.ArchivedDate,
           f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
           f.InheritPermission,
           f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate,
           ISNULL(u.DisplayName, u.ADUserName) AS CreatedByName
    FROM   storage.Folder f
    LEFT   JOIN core.UserAccount u ON u.UserID = f.CreatedBy
    WHERE  f.IsDeleted = 0
      AND  f.IsArchived = 0
    ORDER  BY f.FolderDepth, f.FolderName;
END
GO
PRINT 'Updated storage.sp_GetActiveFolders with CreatedByName';
GO

-- ============================================================
-- 12. storage.sp_GetFolderSiblings
--     เพิ่ม CreatedByName จาก core.UserAccount
-- ============================================================
IF OBJECT_ID('storage.sp_GetFolderSiblings', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFolderSiblings;
GO

CREATE PROCEDURE storage.sp_GetFolderSiblings
    @ParentFolderID INT = NULL,
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes, f.FileCount,
           f.IsArchived, f.ArchivedBy, f.ArchivedDate,
           f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
           f.InheritPermission,
           f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate,
           ISNULL(u.DisplayName, u.ADUserName) AS CreatedByName
    FROM   storage.Folder f
    LEFT   JOIN core.UserAccount u ON u.UserID = f.CreatedBy
    WHERE  f.OrgUnitID = @OrgUnitID
      AND  ((@ParentFolderID IS NULL AND f.ParentFolderID IS NULL)
         OR f.ParentFolderID = @ParentFolderID)
      AND  f.IsDeleted = 0
    ORDER  BY f.FolderName;
END
GO
PRINT 'Updated storage.sp_GetFolderSiblings with CreatedByName';
GO

PRINT '';
PRINT '=== All Folder stored procedures updated successfully ===';
GO
