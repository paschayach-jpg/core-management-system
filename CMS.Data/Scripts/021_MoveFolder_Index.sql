-- 021_MoveFolder_Index.sql
-- Index สำหรับ GetDescendants query ที่ใช้บ่อยใน MoveFolder
-- ใช้ IF NOT EXISTS guard เพื่อให้ idempotent

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Folder_ParentFolderID_IsDeleted'
      AND object_id = OBJECT_ID('storage.Folder')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Folder_ParentFolderID_IsDeleted
    ON storage.Folder (ParentFolderID, IsDeleted)
    INCLUDE (FolderID, FolderName, FolderDepth, PhysicalPath);
END
GO
