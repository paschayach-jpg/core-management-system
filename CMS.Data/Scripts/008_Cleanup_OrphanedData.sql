-- ============================================================
-- Cleanup Orphaned Data Script
-- This script cleans up any orphaned data that might be causing
-- foreign key constraint issues during permanent deletion
-- ============================================================

USE [SCS]
GO

PRINT 'Starting data cleanup process...'

-- Step 1: Find and report orphaned FileDocuments (files in deleted folders)
PRINT 'Checking for orphaned FileDocuments...'

SELECT 
    fd.FileID,
    fd.FolderID,
    fd.OriginalFileName,
    f.FolderName,
    f.IsDeleted as FolderIsDeleted,
    fd.FileStatus
FROM storage.FileDocument fd
LEFT JOIN storage.Folder f ON fd.FolderID = f.FolderID
WHERE f.FolderID IS NULL 
   OR (f.IsDeleted = 1 AND fd.FileStatus != 3)  -- Files in deleted folders that aren't marked as deleted

IF @@ROWCOUNT > 0
BEGIN
    PRINT 'Found orphaned FileDocuments. Cleaning up...'
    
    -- Mark files in deleted folders as deleted
    UPDATE fd 
    SET FileStatus = 3,  -- Deleted status
        DeletedBy = 1,   -- System user
        DeletedDate = GETUTCDATE()
    FROM storage.FileDocument fd
    INNER JOIN storage.Folder f ON fd.FolderID = f.FolderID
    WHERE f.IsDeleted = 1 AND fd.FileStatus != 3
    
    PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' FileDocuments marked as deleted.'
    
    -- Delete FileDocuments that reference non-existent folders
    DELETE fd
    FROM storage.FileDocument fd
    LEFT JOIN storage.Folder f ON fd.FolderID = f.FolderID
    WHERE f.FolderID IS NULL
    
    PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' orphaned FileDocuments removed.'
END
ELSE
BEGIN
    PRINT 'No orphaned FileDocuments found.'
END

-- Step 2: Find and report orphaned FolderPermissions
PRINT 'Checking for orphaned FolderPermissions...'

SELECT 
    fp.FolderID,
    fp.PrincipalId,
    fp.PermissionType,
    f.FolderName,
    f.IsDeleted as FolderIsDeleted
FROM security.FolderPermission fp
LEFT JOIN storage.Folder f ON fp.FolderID = f.FolderID
WHERE f.FolderID IS NULL

IF @@ROWCOUNT > 0
BEGIN
    PRINT 'Found orphaned FolderPermissions. Cleaning up...'
    
    DELETE fp
    FROM security.FolderPermission fp
    LEFT JOIN storage.Folder f ON fp.FolderID = f.FolderID
    WHERE f.FolderID IS NULL
    
    PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' orphaned FolderPermissions removed.'
END
ELSE
BEGIN
    PRINT 'No orphaned FolderPermissions found.'
END

-- Step 3: Find and report orphaned ApprovalRequests and ApprovalSteps
PRINT 'Checking for orphaned ApprovalRequests and ApprovalSteps...'

SELECT 
    ar.RequestID,
    ar.RequestType,
    ar.FolderID,
    ar.RequestStatus,
    (SELECT COUNT(*) FROM workflow.ApprovalStep astep WHERE astep.RequestID = ar.RequestID) as StepCount
FROM workflow.ApprovalRequest ar
WHERE ar.FolderID IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM storage.Folder f 
      WHERE f.FolderID = ar.FolderID
  )

IF @@ROWCOUNT > 0
BEGIN
    PRINT 'Found orphaned ApprovalRequests. Cleaning up...'
    
    -- Delete ApprovalSteps first
    DELETE astep
    FROM workflow.ApprovalStep astep
    INNER JOIN workflow.ApprovalRequest ar ON astep.RequestID = ar.RequestID
    WHERE ar.FolderID IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM storage.Folder f 
          WHERE f.FolderID = ar.FolderID
      )
    
    PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' orphaned ApprovalSteps removed.'
    
    -- Then delete ApprovalRequests
    DELETE ar
    FROM workflow.ApprovalRequest ar
    WHERE ar.FolderID IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM storage.Folder f 
          WHERE f.FolderID = ar.FolderID
      )
    
    PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' orphaned ApprovalRequests removed.'
END
ELSE
BEGIN
    PRINT 'No orphaned ApprovalRequests found.'
END

-- Step 4: Check for folders with invalid parent references
PRINT 'Checking for folders with invalid parent references...'

SELECT 
    f.FolderID,
    f.FolderName,
    f.ParentFolderID,
    f.IsDeleted
FROM storage.Folder f
WHERE f.ParentFolderID IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM storage.Folder p 
      WHERE p.FolderID = f.ParentFolderID
  )

IF @@ROWCOUNT > 0
BEGIN
    PRINT 'Found folders with invalid parent references.'
    PRINT 'These should be reviewed manually before cleanup.'
END
ELSE
BEGIN
    PRINT 'No folders with invalid parent references found.'
END

-- Step 5: Update folder file counts to ensure accuracy
PRINT 'Updating folder file counts...'

UPDATE f
SET FileCount = (
    SELECT COUNT(*)
    FROM storage.FileDocument fd
    WHERE fd.FolderID = f.FolderID
      AND fd.FileStatus != 3  -- Not deleted
)
FROM storage.Folder f

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' folder file counts updated.'

PRINT 'Data cleanup process completed successfully.'
PRINT 'You can now try the permanent delete operation again.'