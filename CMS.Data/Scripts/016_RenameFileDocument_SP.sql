-- ============================================================
-- Script 016: Add sp_RenameFileDocument stored procedure
-- Updates only OriginalFileName for a given FileID
-- ============================================================

USE [CMSDatabase]
GO

IF OBJECT_ID('storage.sp_RenameFileDocument', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_RenameFileDocument
GO

CREATE PROCEDURE storage.sp_RenameFileDocument
    @FileID           INT,
    @OriginalFileName NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE storage.FileDocument
    SET    OriginalFileName = @OriginalFileName
    WHERE  FileID = @FileID
      AND  DeletedDate IS NULL;   -- ไม่อนุญาตให้เปลี่ยนชื่อไฟล์ที่ถูกลบแล้ว
END
GO
