-- 072_Fix_FileCount_LiveCount.sql
-- แก้ไข sp_GetFolderByID, sp_GetFolderChildren, sp_GetRootFoldersByOrgUnit
-- ให้คำนวณ FileCount แบบ live จาก storage.FileDocument แทนการอ่านจาก column ที่เก็บไว้
-- เพื่อให้จำนวนไฟล์ที่แสดงในหน้า Folder Contents ถูกต้องเสมอ
-- Run: DBA / Dev — ไม่มีผลกระทบต่อ schema

USE [CMS_Database];
GO

-- ─────────────────────────────────────────────────────────────
-- 1. sp_GetFolderByID
-- ─────────────────────────────────────────────────────────────
ALTER PROCEDURE storage.sp_GetFolderByID
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.[FolderID], f.[FolderGUID], f.[FolderName], f.[ParentFolderID], f.[OrgUnitID], f.[TemplateID],
           f.[ConfidentialityLevel], f.[FolderDepth], f.[PhysicalPath], f.[FolderSizeBytes],
           (SELECT COUNT(*) FROM storage.FileDocument fd
            WHERE fd.FolderID = f.FolderID AND fd.FileStatus != 3 AND fd.DeletedDate IS NULL) AS FileCount,
           f.[IsArchived], f.[ArchivedBy], f.[ArchivedDate], f.[IsDeleted], f.[DeletedBy], f.[DeletedDate],
           f.[RecycleBinExpiry], f.[InheritPermission], f.[CreatedBy], f.[CreatedDate], f.[UpdatedBy], f.[UpdatedDate]
    FROM   [storage].[Folder] f
    WHERE  f.[FolderID] = @FolderID;
END
GO

-- ─────────────────────────────────────────────────────────────
-- 2. sp_GetFolderChildren
-- ─────────────────────────────────────────────────────────────
ALTER PROCEDURE storage.sp_GetFolderChildren
    @ParentFolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.[FolderID], f.[FolderGUID], f.[FolderName], f.[ParentFolderID], f.[OrgUnitID], f.[TemplateID],
           f.[ConfidentialityLevel], f.[FolderDepth], f.[PhysicalPath], f.[FolderSizeBytes],
           (SELECT COUNT(*) FROM storage.FileDocument fd
            WHERE fd.FolderID = f.FolderID AND fd.FileStatus != 3 AND fd.DeletedDate IS NULL) AS FileCount,
           f.[IsArchived], f.[ArchivedBy], f.[ArchivedDate], f.[IsDeleted], f.[DeletedBy], f.[DeletedDate],
           f.[RecycleBinExpiry], f.[InheritPermission], f.[CreatedBy], f.[CreatedDate], f.[UpdatedBy], f.[UpdatedDate]
    FROM   [storage].[Folder] f
    WHERE  f.[ParentFolderID] = @ParentFolderID AND f.[IsDeleted] = 0;
END
GO

-- ─────────────────────────────────────────────────────────────
-- 3. sp_GetRootFoldersByOrgUnit
-- ─────────────────────────────────────────────────────────────
ALTER PROCEDURE storage.sp_GetRootFoldersByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.[FolderID], f.[FolderGUID], f.[FolderName], f.[ParentFolderID], f.[OrgUnitID], f.[TemplateID],
           f.[ConfidentialityLevel], f.[FolderDepth], f.[PhysicalPath], f.[FolderSizeBytes],
           (SELECT COUNT(*) FROM storage.FileDocument fd
            WHERE fd.FolderID = f.FolderID AND fd.FileStatus != 3 AND fd.DeletedDate IS NULL) AS FileCount,
           f.[IsArchived], f.[ArchivedBy], f.[ArchivedDate], f.[IsDeleted], f.[DeletedBy], f.[DeletedDate],
           f.[RecycleBinExpiry], f.[InheritPermission], f.[CreatedBy], f.[CreatedDate], f.[UpdatedBy], f.[UpdatedDate]
    FROM   [storage].[Folder] f
    WHERE  f.[OrgUnitID] = @OrgUnitID AND f.[ParentFolderID] IS NULL AND f.[IsDeleted] = 0;
END
GO
