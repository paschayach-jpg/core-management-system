-- 075_Fix_LiveCount_RemainingFolderSPs.sql
-- ALTER stored procedures ที่ยังใช้ f.FileCount (stored/cached column)
-- ให้คำนวณ FileCount แบบ live จาก storage.FileDocument แทน
-- ครอบคลุม SP ที่ยังไม่ได้แก้ใน script 072:
--   sp_GetFolderDescendants, sp_GetFoldersByOrgUnit, sp_GetAllFolders,
--   sp_GetActiveFolders, sp_GetFolderSiblings
-- นอกจากนี้ยังทำ one-time sync ปรับ FileCount ให้ตรงกับข้อมูลจริง
--
-- Run: DBA / Dev

USE [CMS_DB];
GO

-- ─────────────────────────────────────────────────────────────
-- 0. One-time sync: ปรับ FileCount ในตาราง storage.Folder
--    ให้ตรงกับ file ที่มีจริง (FileStatus != 3, DeletedDate IS NULL)
-- ─────────────────────────────────────────────────────────────
PRINT 'Step 0: Syncing stored FileCount column...';

UPDATE f
SET    f.FileCount = (
           SELECT COUNT(*)
           FROM   storage.FileDocument fd
           WHERE  fd.FolderID = f.FolderID
             AND  fd.FileStatus != 3
             AND  fd.DeletedDate IS NULL
       )
FROM   storage.Folder f;

PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' folder(s) FileCount updated.';
GO

-- ─────────────────────────────────────────────────────────────
-- 1. sp_GetFolderDescendants  (recursive CTE)
--    ลบ FileCount ออกจาก CTE แล้วคำนวณ live ใน outer SELECT
-- ─────────────────────────────────────────────────────────────
ALTER PROCEDURE storage.sp_GetFolderDescendants
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH FolderHierarchy AS (
        SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
               f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
               f.PhysicalPath, f.FolderSizeBytes,
               f.IsArchived, f.ArchivedBy, f.ArchivedDate,
               f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
               f.InheritPermission,
               f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate
        FROM   storage.Folder f
        WHERE  f.FolderID = @FolderID

        UNION ALL

        SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
               f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
               f.PhysicalPath, f.FolderSizeBytes,
               f.IsArchived, f.ArchivedBy, f.ArchivedDate,
               f.IsDeleted, f.DeletedBy, f.DeletedDate, f.RecycleBinExpiry,
               f.InheritPermission,
               f.CreatedBy, f.CreatedDate, f.UpdatedBy, f.UpdatedDate
        FROM   storage.Folder f
        INNER  JOIN FolderHierarchy fh ON f.ParentFolderID = fh.FolderID
    )
    SELECT fh.FolderID, fh.FolderGUID, fh.FolderName, fh.ParentFolderID,
           fh.OrgUnitID, fh.TemplateID, fh.ConfidentialityLevel, fh.FolderDepth,
           fh.PhysicalPath, fh.FolderSizeBytes,
           (SELECT COUNT(*)
            FROM   storage.FileDocument fd
            WHERE  fd.FolderID = fh.FolderID
              AND  fd.FileStatus != 3
              AND  fd.DeletedDate IS NULL) AS FileCount,
           fh.IsArchived, fh.ArchivedBy, fh.ArchivedDate,
           fh.IsDeleted, fh.DeletedBy, fh.DeletedDate, fh.RecycleBinExpiry,
           fh.InheritPermission,
           fh.CreatedBy, fh.CreatedDate, fh.UpdatedBy, fh.UpdatedDate,
           ISNULL(u.DisplayName, u.ADUserName) AS CreatedByName
    FROM   FolderHierarchy fh
    LEFT   JOIN core.UserAccount u ON u.UserID = fh.CreatedBy;
END
GO
PRINT 'Altered storage.sp_GetFolderDescendants — live FileCount';
GO

-- ─────────────────────────────────────────────────────────────
-- 2. sp_GetFoldersByOrgUnit
-- ─────────────────────────────────────────────────────────────
ALTER PROCEDURE storage.sp_GetFoldersByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes,
           (SELECT COUNT(*)
            FROM   storage.FileDocument fd
            WHERE  fd.FolderID = f.FolderID
              AND  fd.FileStatus != 3
              AND  fd.DeletedDate IS NULL) AS FileCount,
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
PRINT 'Altered storage.sp_GetFoldersByOrgUnit — live FileCount';
GO

-- ─────────────────────────────────────────────────────────────
-- 3. sp_GetAllFolders
-- ─────────────────────────────────────────────────────────────
ALTER PROCEDURE storage.sp_GetAllFolders
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes,
           (SELECT COUNT(*)
            FROM   storage.FileDocument fd
            WHERE  fd.FolderID = f.FolderID
              AND  fd.FileStatus != 3
              AND  fd.DeletedDate IS NULL) AS FileCount,
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
PRINT 'Altered storage.sp_GetAllFolders — live FileCount';
GO

-- ─────────────────────────────────────────────────────────────
-- 4. sp_GetActiveFolders
-- ─────────────────────────────────────────────────────────────
ALTER PROCEDURE storage.sp_GetActiveFolders
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes,
           (SELECT COUNT(*)
            FROM   storage.FileDocument fd
            WHERE  fd.FolderID = f.FolderID
              AND  fd.FileStatus != 3
              AND  fd.DeletedDate IS NULL) AS FileCount,
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
PRINT 'Altered storage.sp_GetActiveFolders — live FileCount';
GO

-- ─────────────────────────────────────────────────────────────
-- 5. sp_GetFolderSiblings
-- ─────────────────────────────────────────────────────────────
ALTER PROCEDURE storage.sp_GetFolderSiblings
    @ParentFolderID INT = NULL,
    @OrgUnitID      INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FolderID, f.FolderGUID, f.FolderName, f.ParentFolderID,
           f.OrgUnitID, f.TemplateID, f.ConfidentialityLevel, f.FolderDepth,
           f.PhysicalPath, f.FolderSizeBytes,
           (SELECT COUNT(*)
            FROM   storage.FileDocument fd
            WHERE  fd.FolderID = f.FolderID
              AND  fd.FileStatus != 3
              AND  fd.DeletedDate IS NULL) AS FileCount,
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
PRINT 'Altered storage.sp_GetFolderSiblings — live FileCount';
GO

PRINT '';
PRINT '=== Script 075 completed — all folder SPs now return live FileCount ===';
GO
