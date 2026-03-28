-- ============================================================
-- Script: 019_GetFolderSiblings.sql
-- Purpose: สร้าง Stored Procedure sp_GetFolderSiblings
--          สำหรับดึง Folder ที่อยู่ใน Parent เดียวกัน
--          (ใช้ตรวจสอบ duplicate folder name)
-- ============================================================

USE SCS;
GO

-- ============================================================
-- sp_GetFolderSiblings
-- คืน Folder ทั้งหมดที่มี ParentFolderID และ OrgUnitID ตรงกัน
-- โดย filter IsDeleted = 0 เท่านั้น
-- ============================================================
IF OBJECT_ID('storage.sp_GetFolderSiblings', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFolderSiblings;
GO

CREATE PROCEDURE storage.sp_GetFolderSiblings
    @ParentFolderID INT     = NULL,
    @OrgUnitID      INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ParentFolderID IS NULL
    BEGIN
        -- Root-level folders: ParentFolderID IS NULL
        SELECT *
        FROM   storage.Folder
        WHERE  ParentFolderID IS NULL
          AND  OrgUnitID      = @OrgUnitID
          AND  IsDeleted      = 0
        ORDER BY FolderName;
    END
    ELSE
    BEGIN
        -- Sub-folders under a specific parent
        SELECT *
        FROM   storage.Folder
        WHERE  ParentFolderID = @ParentFolderID
          AND  OrgUnitID      = @OrgUnitID
          AND  IsDeleted      = 0
        ORDER BY FolderName;
    END
END
GO

PRINT 'Created: storage.sp_GetFolderSiblings';
GO
