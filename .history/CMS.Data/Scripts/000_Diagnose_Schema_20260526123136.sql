-- 000_Diagnose_Schema.sql
-- Purpose: Inspect actual column structure of WFE.t_WFStepInstance and related objects
--          to determine what columns exist and what names they use.
-- Usage: Run in SSMS and examine output to understand schema before migration.

SET NOCOUNT ON;

PRINT '====================================================================';
PRINT 'DIAGNOSIS: WFE.t_WFStepInstance Schema';
PRINT '====================================================================';

-- 1. Check if table exists
IF OBJECT_ID('WFE.t_WFStepInstance', 'U') IS NOT NULL
BEGIN
    PRINT '✓ Table WFE.t_WFStepInstance EXISTS';
    PRINT '';
    
    -- 2. List all columns in WFE.t_WFStepInstance
    PRINT 'Columns in WFE.t_WFStepInstance:';
    PRINT '---------------------------------';
    SELECT
        c.name AS ColumnName,
        t.name AS DataType,
        c.max_length AS MaxLength,
        c.is_nullable AS IsNullable,
        c.is_computed AS IsComputed,
        c.is_identity AS IsIdentity
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('WFE.t_WFStepInstance', 'U')
    ORDER BY c.column_id;
    
    PRINT '';
    PRINT '====================================================================';
    PRINT 'Expected columns by application code:';
    PRINT '====================================================================';
    PRINT 'WorkflowStepInstanceId, WorkflowInstanceId, WorkflowStepId';
    PRINT 'AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy';
    PRINT 'OutputData, SLADeadline, SLAStatus, IsDeleted';
    PRINT '';
    
    -- 3. Check which expected columns are missing
    PRINT 'Missing or misnamed columns:';
    PRINT '-----------------------------';
    SELECT CASE
               WHEN NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'AssignedDate')
                    AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('AssignedOn','StartedDate'))
                    THEN 'AssignedDate (or alias AssignedOn/StartedDate) - MISSING'
               ELSE NULL
           END AS Issue
    WHERE NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'AssignedDate')
      AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('AssignedOn','StartedDate'))
    
    UNION ALL
    
    SELECT CASE
               WHEN NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'CompletedBy')
                    AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('CompletedByAD','CompletedUser'))
                    THEN 'CompletedBy (or alias CompletedByAD/CompletedUser) - MISSING'
               ELSE NULL
           END AS Issue
    WHERE NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'CompletedBy')
      AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('CompletedByAD','CompletedUser'))
    
    UNION ALL
    
    SELECT CASE
               WHEN NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'OutputData')
                    AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('Output','Payload'))
                    THEN 'OutputData (or alias Output/Payload) - MISSING'
               ELSE NULL
           END AS Issue
    WHERE NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'OutputData')
      AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('Output','Payload'))
    
    UNION ALL
    
    SELECT CASE
               WHEN NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'SLADeadline')
                    AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('DueDate','Deadline'))
                    THEN 'SLADeadline (or alias DueDate/Deadline) - MISSING'
               ELSE NULL
           END AS Issue
    WHERE NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'SLADeadline')
      AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('DueDate','Deadline'))
    
    UNION ALL
    
    SELECT CASE
               WHEN NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'SLAStatus')
                    AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('SLAState'))
                    THEN 'SLAStatus (or alias SLAState) - MISSING'
               ELSE NULL
           END AS Issue
    WHERE NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name = 'SLAStatus')
      AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WFE.t_WFStepInstance','U') AND name IN ('SLAState'));
    
    PRINT '';
END
ELSE
BEGIN
    PRINT '✗ Table WFE.t_WFStepInstance NOT FOUND in current database context';
    PRINT 'Possible issues:';
    PRINT '  - Incorrect schema name (not WFE)';
    PRINT '  - Incorrect table name (not t_WFStepInstance)';
    PRINT '  - Table in different database';
    PRINT '';
END

-- 4. Check stored procedures that reference these columns
PRINT '====================================================================';
PRINT 'Stored Procedures in dbo schema:';
PRINT '====================================================================';
SELECT
    OBJECT_NAME(object_id) AS ProcedureName,
    OBJECT_SCHEMA_NAME(object_id) AS SchemaName
FROM sys.sql_modules
WHERE OBJECT_SCHEMA_NAME(object_id) = 'dbo'
  AND (definition LIKE '%WFStepInstance%'
       OR definition LIKE '%WorkflowStepInstance%'
       OR definition LIKE '%t_WFStepInstance%')
ORDER BY OBJECT_NAME(object_id);

PRINT '';

-- 5. Check WorkflowStep table (referenced by joins)
PRINT '====================================================================';
PRINT 'WorkflowStep table schema:';
PRINT '====================================================================';
IF OBJECT_ID('dbo.WorkflowStep', 'U') IS NOT NULL
BEGIN
    PRINT '✓ Table dbo.WorkflowStep EXISTS';
    SELECT
        c.name AS ColumnName,
        t.name AS DataType,
        c.is_nullable AS IsNullable,
        c.is_computed AS IsComputed
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.WorkflowStep', 'U')
    ORDER BY c.column_id;
END
ELSE
BEGIN
    PRINT '✗ Table dbo.WorkflowStep NOT FOUND';
END

PRINT '';
PRINT '====================================================================';
PRINT 'Summary of recommendations:';
PRINT '====================================================================';
PRINT 'If columns are named with different conventions (e.g., WFStepInstanceId)';
PRINT 'then use the migration script to add aliases.';
PRINT 'If columns are completely missing, you may need to:';
PRINT '  1. Run a full data migration script';
PRINT '  2. Review and restore from a backup schema';
PRINT '';
