-- ============================================================
-- Simple Cleanup Script for Permanent Folder Deletion
-- This script removes all foreign key dependencies before deletion
-- Run this before attempting permanent folder deletion
-- ============================================================

USE [SCS]
GO

PRINT 'Starting simple cleanup for permanent folder deletion...'

-- Step 1: Show what we're about to clean up
PRINT 'Folders currently in recycle bin:'
SELECT 
    f.FolderID,
    f.FolderName,
    f.DeletedDate,
    (SELECT COUNT(*) FROM storage.FileDocument fd WHERE fd.FolderID = f.FolderID) as FileCount,
    (SELECT COUNT(*) FROM security.FolderPermission fp WHERE fp.FolderID = f.FolderID) as PermissionCount,
    (SELECT COUNT(*) FROM workflow.ApprovalRequest ar WHERE ar.FolderID = f.FolderID) as ApprovalCount,
    (SELECT COUNT(*) 
     FROM workflow.ApprovalStep astep 
     INNER JOIN workflow.ApprovalRequest ar ON astep.RequestID = ar.RequestID 
     WHERE ar.FolderID = f.FolderID) as ApprovalStepCount
FROM storage.Folder f
WHERE f.IsDeleted = 1
ORDER BY f.FolderName

-- Step 2: Clean up all dependencies for deleted folders
PRINT 'Cleaning up dependencies for deleted folders...'

-- Delete files in deleted folders
DELETE fd
FROM storage.FileDocument fd
INNER JOIN storage.Folder f ON fd.FolderID = f.FolderID
WHERE f.IsDeleted = 1

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' files deleted.'

-- Delete permissions for deleted folders
DELETE fp
FROM security.FolderPermission fp
INNER JOIN storage.Folder f ON fp.FolderID = f.FolderID
WHERE f.IsDeleted = 1

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' permissions deleted.'

-- Delete approval requests for deleted folders (must delete steps first)
PRINT 'Deleting approval steps for deleted folders...'
DELETE astep
FROM workflow.ApprovalStep astep
INNER JOIN workflow.ApprovalRequest ar ON astep.RequestID = ar.RequestID
INNER JOIN storage.Folder f ON ar.FolderID = f.FolderID
WHERE f.IsDeleted = 1

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' approval steps deleted.'

PRINT 'Deleting approval requests for deleted folders...'
DELETE ar
FROM workflow.ApprovalRequest ar
INNER JOIN storage.Folder f ON ar.FolderID = f.FolderID
WHERE f.IsDeleted = 1

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' approval requests deleted.'

-- Step 3: Show remaining folders ready for deletion
PRINT 'Folders now ready for permanent deletion:'
SELECT 
    f.FolderID,
    f.FolderName,
    f.DeletedDate,
    (SELECT COUNT(*) FROM storage.FileDocument fd WHERE fd.FolderID = f.FolderID) as RemainingFiles,
    (SELECT COUNT(*) FROM security.FolderPermission fp WHERE fp.FolderID = f.FolderID) as RemainingPermissions,
    (SELECT COUNT(*) FROM workflow.ApprovalRequest ar WHERE ar.FolderID = f.FolderID) as RemainingApprovals,
    (SELECT COUNT(*) 
     FROM workflow.ApprovalStep astep 
     INNER JOIN workflow.ApprovalRequest ar ON astep.RequestID = ar.RequestID 
     WHERE ar.FolderID = f.FolderID) as RemainingApprovalSteps
FROM storage.Folder f
WHERE f.IsDeleted = 1
ORDER BY f.FolderName

PRINT 'Cleanup completed. You can now try permanent deletion from the application.'
PRINT 'All foreign key dependencies have been removed.'