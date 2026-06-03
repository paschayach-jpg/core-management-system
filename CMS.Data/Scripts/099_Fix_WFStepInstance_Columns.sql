-- 099_Fix_WFStepInstance_Columns.sql
-- Purpose: Fix incorrect datatypes for OutputData, SLADeadline, and SLAStatus in WFE.t_WFStepInstance.
--          - OutputData should be NVARCHAR(MAX) (currently datetime)
--          - SLADeadline should be DATETIME (currently nvarchar(100))
--          - SLAStatus should be TINYINT (currently nvarchar(100))
-- Idempotent: YES

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @objId INT = OBJECT_ID(N'WFE.t_WFStepInstance', N'U');
    IF @objId IS NULL
    BEGIN
        RAISERROR('Table WFE.t_WFStepInstance not found in current database context.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -- 1. Alter OutputData to NVARCHAR(MAX) if it is datetime
    IF EXISTS (
        SELECT 1 
        FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = @objId 
          AND c.name = 'OutputData' 
          AND t.name = 'datetime'
    )
    BEGIN
        PRINT 'Altering OutputData from datetime to NVARCHAR(MAX)...';
        ALTER TABLE WFE.t_WFStepInstance ALTER COLUMN OutputData NVARCHAR(MAX) NULL;
        PRINT 'OutputData altered successfully.';
    END
    ELSE
    BEGIN
        PRINT 'OutputData is already not datetime or has been updated.';
    END

    -- 2. Alter SLADeadline to DATETIME if it is nvarchar
    IF EXISTS (
        SELECT 1 
        FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = @objId 
          AND c.name = 'SLADeadline' 
          AND t.name IN ('nvarchar', 'varchar', 'char', 'nchar')
    )
    BEGIN
        PRINT 'Altering SLADeadline from string to DATETIME...';
        
        -- Make sure there are no incompatible values (e.g. empty strings) that cause conversion errors
        UPDATE WFE.t_WFStepInstance
        SET SLADeadline = NULL
        WHERE ISDATE(SLADeadline) = 0 AND SLADeadline IS NOT NULL;

        ALTER TABLE WFE.t_WFStepInstance ALTER COLUMN SLADeadline DATETIME NULL;
        PRINT 'SLADeadline altered successfully.';
    END
    ELSE
    BEGIN
        PRINT 'SLADeadline is already DATETIME or has been updated.';
    END

    -- 3. Alter SLAStatus to TINYINT if it is nvarchar/varchar
    IF EXISTS (
        SELECT 1 
        FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = @objId 
          AND c.name = 'SLAStatus' 
          AND t.name IN ('nvarchar', 'varchar', 'char', 'nchar')
    )
    BEGIN
        PRINT 'Altering SLAStatus from string to TINYINT...';

        -- Clean up non-numeric values
        UPDATE WFE.t_WFStepInstance
        SET SLAStatus = NULL
        WHERE SLAStatus IS NOT NULL AND TRY_CAST(SLAStatus AS TINYINT) IS NULL;

        ALTER TABLE WFE.t_WFStepInstance ALTER COLUMN SLAStatus TINYINT NULL;
        PRINT 'SLAStatus altered successfully.';
    END
    ELSE
    BEGIN
        PRINT 'SLAStatus is already TINYINT or has been updated.';
    END

    COMMIT TRANSACTION;
    PRINT 'Schema fix for WFE.t_WFStepInstance completed successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE();
    RAISERROR('Migration failed: %s', 16, 1, @ErrMsg);
END CATCH
