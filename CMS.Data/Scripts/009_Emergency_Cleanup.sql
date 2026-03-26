-- ============================================================
-- Emergency Cleanup Script for Foreign Key Constraint Issues
-- This script forcefully removes problematic data to allow
-- permanent folder deletion to work
-- ============================================================

USE [SCS]
GO

PRINT 'Starting emergency cleanup...'

-- Step 1: Find folders in recycle bin that have files
PRINT 'Finding folders in recycle bin with files...'

SELECT 
    f.FolderID,
    f.FolderName,
    COUNT(fd.FileID) as FileCount
FROM storage.Folder f
LEFT JOIN storage.FileDocument fd ON f.FolderID = fd.FolderID
WHERE f.IsDeleted = 1  -- In recycle bin
GROUP BY f.FolderID, f.FolderName
HAVING COUNT(fd.FileID) > 0
ORDER BY f.FolderName

-- Step 2: Delete all files in deleted folders
PRINT 'Deleting files in deleted folders...'

DELETE fd
FROM storage.FileDocument fd
INNER JOIN storage.Folder f ON fd.FolderID = f.FolderID
WHERE f.IsDeleted = 1

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' files deleted from recycle bin folders.'

-- Step 3: Delete permissions for deleted folders
PRINT 'Deleting permissions for deleted folders...'

DELETE fp
FROM security.FolderPermission fp
INNER JOIN storage.Folder f ON fp.FolderID = f.FolderID
WHERE f.IsDeleted = 1

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' permissions deleted for recycle bin folders.'

-- Step 4: Delete approval requests for deleted folders (must delete steps first)
PRINT 'Deleting approval steps for deleted folders...'

DELETE astep
FROM workflow.ApprovalStep astep
INNER JOIN workflow.ApprovalRequest ar ON astep.RequestID = ar.RequestID
INNER JOIN storage.Folder f ON ar.FolderID = f.FolderID
WHERE f.IsDeleted = 1

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' approval steps deleted for recycle bin folders.'

PRINT 'Deleting approval requests for deleted folders...'

DELETE ar
FROM workflow.ApprovalRequest ar
INNER JOIN storage.Folder f ON ar.FolderID = f.FolderID
WHERE f.IsDeleted = 1

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' approval requests deleted for recycle bin folders.'

-- Step 5: Show remaining folders in recycle bin
PRINT 'Remaining folders in recycle bin:'

SELECT 
    f.FolderID,
    f.FolderName,
    f.DeletedDate,
    COUNT(fd.FileID) as RemainingFiles
FROM storage.Folder f
LEFT JOIN storage.FileDocument fd ON f.FolderID = fd.FolderID
WHERE f.IsDeleted = 1
GROUP BY f.FolderID, f.FolderName, f.DeletedDate
ORDER BY f.FolderName

PRINT 'Emergency cleanup completed. You can now try permanent deletion again.'