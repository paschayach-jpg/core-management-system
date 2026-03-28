-- ============================================================
-- Script 030: Clear Data for New Structure
-- Clears existing OrgUnit data to start fresh with flexible structure
-- ============================================================

PRINT 'Clearing OrgUnit Data for New Flexible Structure...'
PRINT '=================================================='

-- Backup current data (optional - comment out if not needed)
PRINT 'Creating backup of current data...'
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrgUnit_Backup_' + FORMAT(GETDATE(), 'yyyyMMdd'))
BEGIN
    DECLARE @backupTable NVARCHAR(100) = 'OrgUnit_Backup_' + FORMAT(GETDATE(), 'yyyyMMdd')
    DECLARE @sql NVARCHAR(MAX) = 'SELECT * INTO core.' + @backupTable + ' FROM core.OrgUnit'
    EXEC sp_executesql @sql
    PRINT 'Backup created: core.' + @backupTable
END

-- Show current data count before clearing
PRINT 'Current data count:'
SELECT 
    'Total OrgUnits' as DataType,
    COUNT(*) as Count
FROM core.OrgUnit
UNION ALL
SELECT 
    'Active OrgUnits' as DataType,
    COUNT(*) as Count
FROM core.OrgUnit WHERE IsDeleted = 0 AND IsActive = 1
UNION ALL
SELECT 
    'SubCommittees' as DataType,
    COUNT(*) as Count
FROM core.OrgUnit WHERE OrgUnitType = 1 AND IsDeleted = 0
UNION ALL
SELECT 
    'Divisions' as DataType,
    COUNT(*) as Count
FROM core.OrgUnit WHERE OrgUnitType = 2 AND IsDeleted = 0
UNION ALL
SELECT 
    'Departments' as DataType,
    COUNT(*) as Count
FROM core.OrgUnit WHERE OrgUnitType = 3 AND IsDeleted = 0;

-- Clear related data first (if any foreign key constraints exist)
PRINT ''
PRINT 'Clearing related data...'

-- Clear any permissions or relationships that might reference OrgUnits
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FolderPermission')
BEGIN
    DELETE FROM core.FolderPermission WHERE PrincipalType IN (3, 4); -- Department, Division
    PRINT 'Cleared FolderPermission references'
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount')
BEGIN
    UPDATE core.UserAccount SET OrgUnitId = NULL WHERE OrgUnitId IS NOT NULL;
    PRINT 'Cleared UserAccount OrgUnit references'
END

-- Clear OrgUnit data
PRINT ''
PRINT 'Clearing OrgUnit data...'

-- Method 1: Soft delete (recommended - preserves audit trail)
UPDATE core.OrgUnit 
SET IsDeleted = 1, 
    IsActive = 0,
    DeletedBy = 1, -- System user
    OrgUnitCode = OrgUnitCode + '_DELETED_' + FORMAT(GETDATE(), 'yyyyMMddHHmmss')
WHERE IsDeleted = 0;

PRINT 'Soft deleted all existing OrgUnits'

-- Method 2: Hard delete (uncomment if you want to completely remove data)
-- WARNING: This will permanently delete all data!
/*
DELETE FROM core.OrgUnit;
PRINT 'Hard deleted all OrgUnit data'
*/

-- Reset identity seed (if using identity column)
IF EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID('core.OrgUnit'))
BEGIN
    DBCC CHECKIDENT ('core.OrgUnit', RESEED, 0);
    PRINT 'Reset OrgUnit identity seed'
END

-- Verify data is cleared
PRINT ''
PRINT 'Verification - Active data count after clearing:'
SELECT 
    'Active OrgUnits' as DataType,
    COUNT(*) as Count
FROM core.OrgUnit WHERE IsDeleted = 0 AND IsActive = 1
UNION ALL
SELECT 
    'Total OrgUnits (including deleted)' as DataType,
    COUNT(*) as Count
FROM core.OrgUnit;

PRINT ''
PRINT '✅ Data cleared successfully!'
PRINT ''
PRINT 'Next Steps:'
PRINT '1. Run your application'
PRINT '2. Create new SubCommittees'
PRINT '3. Create Divisions (can be independent or belong to SubCommittees)'
PRINT '4. Create Departments under Divisions'
PRINT ''
PRINT 'Example structure you can now create:'
PRINT '🏢 SubCommittee A'
PRINT '├── 📁 Division 1'
PRINT '├── 📁 Division 2'
PRINT '└── 📁 Division 3'
PRINT ''
PRINT '🏢 SubCommittee B'
PRINT '├── 📁 Division 4'
PRINT '└── 📁 Division 5'
PRINT ''
PRINT '🔗 Independent Division X (flexible)'
PRINT '🔗 Independent Division Y (flexible)'

PRINT ''
PRINT 'Script 030: Data clearing completed successfully!'