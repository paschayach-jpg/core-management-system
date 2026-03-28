-- ============================================================
-- Script 022: แก้ไข storage.sp_UpdateFileDocument
-- เพิ่ม @FolderID และ @PhysicalPath เพื่อรองรับ MoveFile operation
-- Run script นี้ใน SSMS กับ database SCS
-- ============================================================

USE SCS;
GO

IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_UpdateFileDocument')
    DROP PROCEDURE storage.sp_UpdateFileDocument;
GO

CREATE PROCEDURE storage.sp_UpdateFileDocument
    @FileID       INT,
    @FolderID     INT,
    @PhysicalPath NVARCHAR(500),
    @FileStatus   TINYINT,
    @ArchivedBy   INT,
    @ArchivedDate DATETIME,
    @DeletedBy    INT,
    @DeletedDate  DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.FileDocument
    SET    FolderID     = @FolderID,
           PhysicalPath = @PhysicalPath,
           FileStatus   = @FileStatus,
           ArchivedBy   = @ArchivedBy,
           ArchivedDate = @ArchivedDate,
           DeletedBy    = @DeletedBy,
           DeletedDate  = @DeletedDate
    WHERE  FileID = @FileID;
END
GO

PRINT '=== Script 022 เสร็จสิ้น: sp_UpdateFileDocument อัปเดตแล้ว ===';
GO
