-- ============================================================
-- Fix for sp_PermanentDeleteFolder Foreign Key Constraint Issue
-- This script fixes the permanent folder deletion to properly handle
-- foreign key constraints by deleting files before folders
-- ============================================================

USE [SCS]
GO

-- Drop existing procedure
IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'storage' AND p.name = 'sp_PermanentDeleteFolder')
    DROP PROCEDURE storage.sp_PermanentDeleteFolder;
GO

-- Create fixed procedure
CREATE PROCEDURE storage.sp_PermanentDeleteFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input
    IF @FolderID IS NULL OR @FolderID <= 0
    BEGIN
        RAISERROR('Invalid FolderID provided', 16, 1);
        RETURN;
    END
    
    -- Check if folder exists and is deleted (in recycle bin)
    IF NOT EXISTS (SELECT 1 FROM storage.Folder WHERE FolderID = @FolderID AND IsDeleted = 1)
    BEGIN
        RAISERROR('Folder not found or not in recycle bin', 16, 1);
        RETURN;
    END
    
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Step 1: Collect all descendant folder IDs (including the root) into a temp table
        -- This avoids the CTE reuse issue
        CREATE TABLE #FoldersToDelete (
            FolderID INT PRIMARY KEY,
            FolderDepth INT
        );
        
        -- Use recursive CTE to find all descendants
        WITH FolderCTE AS (
            -- Anchor: Start with the target folder
            SELECT FolderID, FolderDepth 
            FROM storage.Folder 
            WHERE FolderID = @FolderID
            
            UNION ALL
            
            -- Recursive: Find children
            SELECT f.FolderID, f.FolderDepth
            FROM storage.Folder f
            INNER JOIN FolderCTE cte ON f.ParentFolderID = cte.FolderID
        )
        INSERT INTO #FoldersToDelete (FolderID, FolderDepth)
        SELECT FolderID, FolderDepth FROM FolderCTE;

        -- Step 2: Delete all FileDocument records in those folders first
        -- This prevents foreign key constraint violations
        DELETE fd FROM storage.FileDocument fd
        INNER JOIN #FoldersToDelete ftd ON fd.FolderID = ftd.FolderID;

        -- Step 3: Delete FolderPermission entries
        DELETE fp FROM security.FolderPermission fp
        INNER JOIN #FoldersToDelete ftd ON fp.FolderID = ftd.FolderID;

        -- Step 4: Delete ApprovalStep entries first, then ApprovalRequest entries
        DELETE astep FROM workflow.ApprovalStep astep
        INNER JOIN workflow.ApprovalRequest ar ON astep.RequestID = ar.RequestID
        INNER JOIN #FoldersToDelete ftd ON ar.FolderID = ftd.FolderID;

        DELETE ar FROM workflow.ApprovalRequest ar
        INNER JOIN #FoldersToDelete ftd ON ar.FolderID = ftd.FolderID;

        -- Step 5: Delete Folder records (children first by ordering by depth descending)
        -- Higher depth = deeper in hierarchy, so delete deepest first
        DELETE f FROM storage.Folder f
        INNER JOIN #FoldersToDelete ftd ON f.FolderID = ftd.FolderID
        ORDER BY ftd.FolderDepth DESC;

        -- Clean up temp table
        DROP TABLE #FoldersToDelete;

        COMMIT TRANSACTION;
        
        PRINT 'Folder and all descendants permanently deleted successfully.';
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        -- Clean up temp table if it exists
        IF OBJECT_ID('tempdb..#FoldersToDelete') IS NOT NULL
            DROP TABLE #FoldersToDelete;
            
        -- Re-throw the error with more context
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR('Failed to permanently delete folder: %s', @ErrorSeverity, @ErrorState, @ErrorMessage);
    END CATCH
END
GO

PRINT 'sp_PermanentDeleteFolder procedure updated successfully.';