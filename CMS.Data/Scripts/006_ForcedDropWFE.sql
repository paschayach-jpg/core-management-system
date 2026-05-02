-- ============================================================
-- FORCED DROP — ลบ WFE Schema ให้หมด
-- ============================================================

PRINT 'Starting FORCED cleanup...'
GO

-- Disable foreign key constraints temporarily
EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO

-- Drop all tables in WFE schema
DECLARE @sql NVARCHAR(MAX) = ''
SELECT @sql = @sql + 'DROP TABLE ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) + '; '
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'WFE' AND TABLE_TYPE = 'BASE TABLE'

IF LEN(@sql) > 0
BEGIN
    PRINT 'Dropping tables...'
    EXEC sp_executesql @sql
    PRINT 'Tables dropped successfully'
END
ELSE
BEGIN
    PRINT 'No tables found to drop'
END
GO

-- Drop all stored procedures in WFE schema
DECLARE @sql NVARCHAR(MAX) = ''
SELECT @sql = @sql + 'DROP PROCEDURE ' + QUOTENAME(ROUTINE_SCHEMA) + '.' + QUOTENAME(ROUTINE_NAME) + '; '
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'WFE' AND ROUTINE_TYPE = 'PROCEDURE'

IF LEN(@sql) > 0
BEGIN
    PRINT 'Dropping stored procedures...'
    EXEC sp_executesql @sql
    PRINT 'Stored procedures dropped successfully'
END
ELSE
BEGIN
    PRINT 'No procedures found to drop'
END
GO

-- Drop WFE schema
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'WFE')
BEGIN
    DROP SCHEMA WFE
    PRINT 'WFE schema dropped successfully'
END
ELSE
BEGIN
    PRINT 'WFE schema does not exist'
END
GO

-- Re-enable foreign key constraints
EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
GO

PRINT '============================================================'
PRINT 'FORCED cleanup complete!'
PRINT 'Next: Run 010_CreateWorkflowTables.sql'
PRINT '============================================================'
GO
