-- ============================================================
-- Script: 020_QuickAccess_Table.sql
-- Purpose: สร้าง Table และ Stored Procedures สำหรับ Quick Access / Favorite Folder
-- Requirements: 7.5
-- ============================================================

USE SCS;
GO

-- ============================================================
-- Table: storage.QuickAccessFolder
-- ============================================================
IF OBJECT_ID('storage.QuickAccessFolder', 'U') IS NULL
BEGIN
    CREATE TABLE storage.QuickAccessFolder (
        QuickAccessID INT IDENTITY(1,1) PRIMARY KEY,
        UserID        INT NOT NULL,
        FolderID      INT NOT NULL REFERENCES storage.Folder(FolderID) ON DELETE CASCADE,
        AddedDate     DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_QuickAccess UNIQUE (UserID, FolderID)
    );
    PRINT 'Created: storage.QuickAccessFolder';
END
ELSE
    PRINT 'Skipped: storage.QuickAccessFolder already exists';
GO

-- ============================================================
-- sp_GetQuickAccessByUser
-- คืน QuickAccessFolder ทั้งหมดของ user ที่ระบุ
-- ============================================================
IF OBJECT_ID('storage.sp_GetQuickAccessByUser', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetQuickAccessByUser;
GO

CREATE PROCEDURE storage.sp_GetQuickAccessByUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT qa.QuickAccessID, qa.UserID, qa.FolderID, qa.AddedDate
    FROM   storage.QuickAccessFolder qa
    WHERE  qa.UserID = @UserID
    ORDER BY qa.AddedDate DESC;
END
GO

PRINT 'Created: storage.sp_GetQuickAccessByUser';
GO

-- ============================================================
-- sp_AddQuickAccess
-- เพิ่ม QuickAccessFolder record (ignore duplicate via UNIQUE constraint)
-- ============================================================
IF OBJECT_ID('storage.sp_AddQuickAccess', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_AddQuickAccess;
GO

CREATE PROCEDURE storage.sp_AddQuickAccess
    @UserID   INT,
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1 FROM storage.QuickAccessFolder
        WHERE UserID = @UserID AND FolderID = @FolderID
    )
    BEGIN
        INSERT INTO storage.QuickAccessFolder (UserID, FolderID, AddedDate)
        VALUES (@UserID, @FolderID, GETDATE());
    END
END
GO

PRINT 'Created: storage.sp_AddQuickAccess';
GO

-- ============================================================
-- sp_RemoveQuickAccess
-- ลบ QuickAccessFolder record
-- ============================================================
IF OBJECT_ID('storage.sp_RemoveQuickAccess', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_RemoveQuickAccess;
GO

CREATE PROCEDURE storage.sp_RemoveQuickAccess
    @UserID   INT,
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM storage.QuickAccessFolder
    WHERE UserID = @UserID AND FolderID = @FolderID;
END
GO

PRINT 'Created: storage.sp_RemoveQuickAccess';
GO
