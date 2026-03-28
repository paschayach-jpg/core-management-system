-- ระบบอัพเดท FileCount อัตโนมัติ
-- สร้าง Stored Procedures และ Triggers เพื่ออัพเดท FileCount ให้เป็นปัจจุบันเสมอ

USE [CMS_Database]
GO

-- ===== 1. สร้าง Stored Procedure สำหรับอัพเดท FileCount =====

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'storage.sp_UpdateFolderFileCount') AND type in (N'P', N'PC'))
    DROP PROCEDURE storage.sp_UpdateFolderFileCount
GO

CREATE PROCEDURE storage.sp_UpdateFolderFileCount
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- อัพเดท FileCount สำหรับโฟลเดอร์ที่ระบุ
    UPDATE storage.Folder
    SET FileCount = (
        SELECT COUNT(*)
        FROM storage.FileDocument fd
        WHERE fd.FolderID = @FolderID
          AND fd.FileStatus != 3  -- ไม่นับไฟล์ที่ถูกลบ (Status 3 = Deleted)
          AND fd.DeletedDate IS NULL
    )
    WHERE FolderID = @FolderID;
    
    PRINT 'Updated FileCount for FolderID: ' + CAST(@FolderID AS VARCHAR(10));
END
GO

-- ===== 2. สร้าง Stored Procedure สำหรับอัพเดท FileCount ทุกโฟลเดอร์ =====

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'storage.sp_UpdateAllFolderFileCounts') AND type in (N'P', N'PC'))
    DROP PROCEDURE storage.sp_UpdateAllFolderFileCounts
GO

CREATE PROCEDURE storage.sp_UpdateAllFolderFileCounts
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UpdatedCount INT;
    
    -- อัพเดท FileCount สำหรับทุกโฟลเดอร์
    UPDATE f
    SET FileCount = (
        SELECT COUNT(*)
        FROM storage.FileDocument fd
        WHERE fd.FolderID = f.FolderID
          AND fd.FileStatus != 3  -- ไม่นับไฟล์ที่ถูกลบ
          AND fd.DeletedDate IS NULL
    )
    FROM storage.Folder f;
    
    SET @UpdatedCount = @@ROWCOUNT;
    PRINT 'Updated FileCount for ' + CAST(@UpdatedCount AS VARCHAR(10)) + ' folders.';
END
GO

-- ===== 3. สร้าง Trigger สำหรับอัพเดท FileCount เมื่อเพิ่มไฟล์ =====

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'storage.tr_FileDocument_Insert_UpdateFileCount'))
    DROP TRIGGER storage.tr_FileDocument_Insert_UpdateFileCount
GO

CREATE TRIGGER storage.tr_FileDocument_Insert_UpdateFileCount
ON storage.FileDocument
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- อัพเดท FileCount สำหรับโฟลเดอร์ที่มีไฟล์ใหม่
    UPDATE f
    SET FileCount = FileCount + 1
    FROM storage.Folder f
    INNER JOIN inserted i ON f.FolderID = i.FolderID
    WHERE i.FileStatus != 3 AND i.DeletedDate IS NULL;
END
GO

-- ===== 4. สร้าง Trigger สำหรับอัพเดท FileCount เมื่อลบไฟล์ =====

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'storage.tr_FileDocument_Update_UpdateFileCount'))
    DROP TRIGGER storage.tr_FileDocument_Update_UpdateFileCount
GO

CREATE TRIGGER storage.tr_FileDocument_Update_UpdateFileCount
ON storage.FileDocument
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ตรวจสอบการเปลี่ยนแปลง FileStatus หรือ DeletedDate
    IF UPDATE(FileStatus) OR UPDATE(DeletedDate)
    BEGIN
        -- รวบรวมโฟลเดอร์ที่ได้รับผลกระทบ
        DECLARE @AffectedFolders TABLE (FolderID INT);
        
        INSERT INTO @AffectedFolders (FolderID)
        SELECT DISTINCT FolderID FROM inserted
        UNION
        SELECT DISTINCT FolderID FROM deleted;
        
        -- อัพเดท FileCount สำหรับโฟลเดอร์ที่ได้รับผลกระทบ
        UPDATE f
        SET FileCount = (
            SELECT COUNT(*)
            FROM storage.FileDocument fd
            WHERE fd.FolderID = f.FolderID
              AND fd.FileStatus != 3
              AND fd.DeletedDate IS NULL
        )
        FROM storage.Folder f
        INNER JOIN @AffectedFolders af ON f.FolderID = af.FolderID;
    END
END
GO

-- ===== 5. สร้าง Trigger สำหรับอัพเดท FileCount เมื่อลบไฟล์จริง =====

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'storage.tr_FileDocument_Delete_UpdateFileCount'))
    DROP TRIGGER storage.tr_FileDocument_Delete_UpdateFileCount
GO

CREATE TRIGGER storage.tr_FileDocument_Delete_UpdateFileCount
ON storage.FileDocument
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- อัพเดท FileCount สำหรับโฟลเดอร์ที่มีไฟล์ถูกลบ
    UPDATE f
    SET FileCount = FileCount - 1
    FROM storage.Folder f
    INNER JOIN deleted d ON f.FolderID = d.FolderID
    WHERE d.FileStatus != 3 AND d.DeletedDate IS NULL
      AND f.FileCount > 0; -- ป้องกันไม่ให้เป็นค่าลบ
END
GO

-- ===== 6. รัน Initial Update เพื่อให้ FileCount ถูกต้อง =====

PRINT 'Running initial FileCount update for all folders...';
EXEC storage.sp_UpdateAllFolderFileCounts;

-- ===== 7. แสดงผลลัพธ์ตัวอย่าง =====

PRINT 'Sample folder file counts after update:';
SELECT TOP 10 
    FolderID,
    FolderName,
    FileCount,
    (SELECT COUNT(*) FROM storage.FileDocument fd 
     WHERE fd.FolderID = f.FolderID 
       AND fd.FileStatus != 3 
       AND fd.DeletedDate IS NULL) as ActualFileCount
FROM storage.Folder f
WHERE f.IsDeleted = 0
ORDER BY FileCount DESC;

PRINT 'FileCount auto-update system installed successfully!';
PRINT 'FileCount will now be automatically updated when files are added, modified, or deleted.';