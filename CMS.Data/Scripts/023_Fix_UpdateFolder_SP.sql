-- ============================================================
-- Script 023: แก้ไข storage.sp_UpdateFolder
-- เพิ่ม @ParentFolderID, @FolderDepth, @PhysicalPath เพื่อรองรับ MoveFolder
-- Run script นี้ใน SSMS กับ database SCS
-- ============================================================

USE SCS;
GO

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_UpdateFolder')
    DROP PROCEDURE storage.sp_UpdateFolder;
GO

CREATE PROCEDURE storage.sp_UpdateFolder
    @FolderID             INT,
    @FolderName           NVARCHAR(255),
    @ParentFolderID       INT           = NULL,
    @FolderDepth          INT           = NULL,
    @PhysicalPath         NVARCHAR(500) = NULL,
    @ConfidentialityLevel TINYINT,
    @InheritPermission    BIT,
    @UpdatedBy            INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.Folder
    SET    FolderName           = @FolderName,
           ParentFolderID       = COALESCE(@ParentFolderID, ParentFolderID),
           FolderDepth          = COALESCE(@FolderDepth,    FolderDepth),
           PhysicalPath         = COALESCE(@PhysicalPath,   PhysicalPath),
           ConfidentialityLevel = @ConfidentialityLevel,
           InheritPermission    = @InheritPermission,
           UpdatedBy            = @UpdatedBy,
           UpdatedDate          = GETDATE()
    WHERE  FolderID = @FolderID;
END
GO

PRINT '=== Script 023 เสร็จสิ้น: sp_UpdateFolder อัปเดตแล้ว ===';
GO
