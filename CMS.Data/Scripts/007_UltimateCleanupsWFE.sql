-- ============================================================
-- ULTIMATE CLEANUP — ลบ WFE Schema ให้หมด (Force All)
-- ============================================================

PRINT 'Starting ULTIMATE cleanup of WFE schema...'
GO

-- Step 1: Drop all FOREIGN KEY constraints in WFE schema
PRINT 'Step 1: Dropping all FOREIGN KEY constraints...'
DECLARE @fk_sql NVARCHAR(MAX) = ''
SELECT @fk_sql = @fk_sql +
    'ALTER TABLE ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) +
    ' DROP CONSTRAINT ' + QUOTENAME(CONSTRAINT_NAME) + '; '
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE CONSTRAINT_TYPE = 'FOREIGN KEY' AND TABLE_SCHEMA = 'WFE'

IF LEN(@fk_sql) > 0
BEGIN
    EXEC sp_executesql @fk_sql
    PRINT '  ✓ Foreign keys dropped'
END
ELSE
BEGIN
    PRINT '  ✓ No foreign keys found'
END
GO

-- Step 2: Drop all CHECK constraints in WFE schema
PRINT 'Step 2: Dropping all CHECK constraints...'
DECLARE @ck_sql NVARCHAR(MAX) = ''
SELECT @ck_sql = @ck_sql +
    'ALTER TABLE ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) +
    ' DROP CONSTRAINT ' + QUOTENAME(CONSTRAINT_NAME) + '; '
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE CONSTRAINT_TYPE = 'CHECK' AND TABLE_SCHEMA = 'WFE'

IF LEN(@ck_sql) > 0
BEGIN
    EXEC sp_executesql @ck_sql
    PRINT '  ✓ Check constraints dropped'
END
ELSE
BEGIN
    PRINT '  ✓ No check constraints found'
END
GO

-- Step 3: Drop all DEFAULT constraints in WFE schema
PRINT 'Step 3: Dropping all DEFAULT constraints...'
DECLARE @df_sql NVARCHAR(MAX) = ''
SELECT @df_sql = @df_sql +
    'ALTER TABLE ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) +
    ' DROP CONSTRAINT ' + QUOTENAME(CONSTRAINT_NAME) + '; '
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE CONSTRAINT_TYPE = 'DEFAULT' AND TABLE_SCHEMA = 'WFE'

IF LEN(@df_sql) > 0
BEGIN
    EXEC sp_executesql @df_sql
    PRINT '  ✓ Default constraints dropped'
END
ELSE
BEGIN
    PRINT '  ✓ No default constraints found'
END
GO

-- Step 4: Drop all indexes (except primary keys)
PRINT 'Step 4: Dropping all indexes...'
DECLARE @idx_sql NVARCHAR(MAX) = ''
SELECT @idx_sql = @idx_sql +
    'DROP INDEX ' + QUOTENAME(INDEX_NAME) + ' ON ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) + '; '
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = 'WFE' AND INDEX_NAME IS NOT NULL AND CONSTRAINT_NAME IS NULL

IF LEN(@idx_sql) > 0
BEGIN
    EXEC sp_executesql @idx_sql
    PRINT '  ✓ Indexes dropped'
END
ELSE
BEGIN
    PRINT '  ✓ No indexes found'
END
GO

-- Step 5: Drop all views in WFE schema
PRINT 'Step 5: Dropping all views...'
DECLARE @view_sql NVARCHAR(MAX) = ''
SELECT @view_sql = @view_sql +
    'DROP VIEW ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) + '; '
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'WFE' AND TABLE_TYPE = 'VIEW'

IF LEN(@view_sql) > 0
BEGIN
    EXEC sp_executesql @view_sql
    PRINT '  ✓ Views dropped'
END
ELSE
BEGIN
    PRINT '  ✓ No views found'
END
GO

-- Step 6: Drop all stored procedures in WFE schema
PRINT 'Step 6: Dropping all stored procedures...'
DECLARE @proc_sql NVARCHAR(MAX) = ''
SELECT @proc_sql = @proc_sql +
    'DROP PROCEDURE ' + QUOTENAME(ROUTINE_SCHEMA) + '.' + QUOTENAME(ROUTINE_NAME) + '; '
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'WFE' AND ROUTINE_TYPE = 'PROCEDURE'

IF LEN(@proc_sql) > 0
BEGIN
    EXEC sp_executesql @proc_sql
    PRINT '  ✓ Procedures dropped'
END
ELSE
BEGIN
    PRINT '  ✓ No procedures found'
END
GO

-- Step 7: Drop all functions in WFE schema
PRINT 'Step 7: Dropping all functions...'
DECLARE @func_sql NVARCHAR(MAX) = ''
SELECT @func_sql = @func_sql +
    'DROP FUNCTION ' + QUOTENAME(ROUTINE_SCHEMA) + '.' + QUOTENAME(ROUTINE_NAME) + '; '
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'WFE' AND ROUTINE_TYPE = 'FUNCTION'

IF LEN(@func_sql) > 0
BEGIN
    EXEC sp_executesql @func_sql
    PRINT '  ✓ Functions dropped'
END
ELSE
BEGIN
    PRINT '  ✓ No functions found'
END
GO

-- Step 8: Drop all tables in WFE schema
PRINT 'Step 8: Dropping all tables...'
DECLARE @tbl_sql NVARCHAR(MAX) = ''
SELECT @tbl_sql = @tbl_sql +
    'DROP TABLE ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) + '; '
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'WFE' AND TABLE_TYPE = 'BASE TABLE'

IF LEN(@tbl_sql) > 0
BEGIN
    EXEC sp_executesql @tbl_sql
    PRINT '  ✓ Tables dropped'
END
ELSE
BEGIN
    PRINT '  ✓ No tables found'
END
GO

-- Step 9: Drop WFE schema
PRINT 'Step 9: Dropping WFE schema...'
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'WFE')
BEGIN
    DROP SCHEMA WFE
    PRINT '  ✓ Schema dropped'
END
ELSE
BEGIN
    PRINT '  ✓ Schema does not exist'
END
GO

PRINT '============================================================'
PRINT 'ULTIMATE cleanup complete!'
PRINT 'WFE schema is now completely removed.'
PRINT 'Next: Run 010_CreateWorkflowTables.sql'
PRINT '============================================================'
GO
