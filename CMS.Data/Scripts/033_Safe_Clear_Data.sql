-- ============================================================
-- Script 033: Safe Clear Data
-- Safely clears data with proper error handling
-- ============================================================

PRINT 'Safe Data Clearing Process'
PRINT '========================='

-- Check if tables exist first
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrgUnit' AND schema_id = SCHEMA_ID('core'))
BEGIN
    PRINT 'ERROR: core.OrgUnit table does not exist!'
    RETURN
END

-- Show current data before clearing
PRINT 'Current data count:'
SELECT 
    COUNT(*) as TotalOrgUnits,
    COUNT(CASE WHEN IsDeleted = 0 AND IsActive = 1 THEN 1 END) as ActiveOrgUnits
FROM core.OrgUnit;

-- Create backup table with timestamp
DECLARE @BackupTableName NVARCHAR(100) = 'OrgUnit_Backup_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss')
DECLARE @SQL NVARCHAR(MAX)

BEGIN TRY
    -- Create backup
    SET @SQL = 'SELECT * INTO core.' + @BackupTableName + ' FROM core.OrgUnit'
    EXEC sp_executesql @SQL
    PRINT 'Backup created: core.' + @BackupTableName
    
    -- Clear foreign key references safely
    BEGIN TRANSACTION
    
    -- Clear UserAccount references
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        UPDATE core.UserAccount SET OrgUnitId = NULL WHERE OrgUnitId IS NOT NULL
        PRINT 'Cleared UserAccount references'
    END
    
    -- Clear FolderPermission references  
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FolderPermission' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        DELETE FROM core.FolderPermission WHERE PrincipalType IN (3, 4)
        PRINT 'Cleared FolderPermission references'
    END
    
    -- Soft delete all OrgUnits (safer than hard delete)
    UPDATE core.OrgUnit 
    SET IsDeleted = 1, 
        IsActive = 0,
        DeletedBy = 1,
        OrgUnitCode = OrgUnitCode + '_DELETED_' + FORMAT(GETDATE(), 'yyyyMMddHHmmss')
    WHERE IsDeleted = 0
    
    PRINT 'Soft deleted all existing OrgUnits'
    
    -- Reset identity if exists
    IF EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID('core.OrgUnit'))
    BEGIN
        DBCC CHECKIDENT ('core.OrgUnit', RESEED, 0)
        PRINT 'Reset identity seed'
    END
    
    COMMIT TRANSACTION
    PRINT '✅ Data cleared successfully!'
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION
        
    PRINT 'ERROR occurred during data clearing:'
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10))
    PRINT 'Error Message: ' + ERROR_MESSAGE()
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10))
    
    -- Restore from backup if needed
    PRINT 'Data clearing failed. Backup table available: core.' + @BackupTableName
END CATCH

-- Verify results
PRINT ''
PRINT 'Final verification:'
SELECT 
    COUNT(*) as TotalOrgUnits,
    COUNT(CASE WHEN IsDeleted = 0 AND IsActive = 1 THEN 1 END) as ActiveOrgUnits,
    COUNT(CASE WHEN IsDeleted = 1 THEN 1 END) as DeletedOrgUnits
FROM core.OrgUnit;

PRINT 'Safe clear data completed.'