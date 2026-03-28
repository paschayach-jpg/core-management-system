-- Script to update FileCount for all folders
-- This ensures that the file count display in TreeView shows correct numbers

USE [CMS_Database]
GO

PRINT 'Starting FileCount update...'

-- Update FileCount for all folders based on actual file count
UPDATE f
SET FileCount = (
    SELECT COUNT(*)
    FROM storage.FileDocument fd
    WHERE fd.FolderID = f.FolderID
      AND fd.FileStatus != 3  -- FileStatus: 1=Active, 2=Archived, 3=Deleted
      AND fd.DeletedDate IS NULL  -- Additional check for non-deleted files
)
FROM storage.Folder f

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' folder file counts updated.'

-- Show some sample results
PRINT 'Sample folder file counts:'
SELECT TOP 10 
    FolderID,
    FolderName,
    FileCount,
    (SELECT COUNT(*) FROM storage.FileDocument fd WHERE fd.FolderID = f.FolderID AND fd.FileStatus != 3 AND fd.DeletedDate IS NULL) as ActualFileCount
FROM storage.Folder f
WHERE f.IsDeleted = 0
ORDER BY FileCount DESC

PRINT 'FileCount update completed.'