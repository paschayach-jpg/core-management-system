-- 090_WFE_Add_Column_Aliases.sql
-- Purpose: Add physical nullable columns to WFE.t_WFStepInstance for legacy
--          column names expected by the application, and copy values from
--          likely candidate columns if present. This avoids creating
--          computed-columns that reference other computed-columns.
-- Usage: Review, then run in the target database (SSMS / sqlcmd). Set the
--        proper database context before executing (uncomment USE if needed).

SET NOCOUNT ON;

-- USE [YourDatabase];

BEGIN TRY
    BEGIN TRANSACTION;

    -- If a prior computed column exists for a target name, drop it so we can
    -- replace it with a real nullable physical column. This is conservative
    -- and only drops if the column is computed.
    DECLARE @objId INT = OBJECT_ID('WFE.t_WFStepInstance');

    IF EXISTS(
        SELECT 1
        FROM sys.columns c
        JOIN sys.objects o ON c.object_id = o.object_id
        WHERE o.object_id = @objId
          AND c.name = 'WorkflowStepInstanceId'
          AND c.is_computed = 1)
        ALTER TABLE WFE.t_WFStepInstance DROP COLUMN WorkflowStepInstanceId;

    IF EXISTS(
        SELECT 1
        FROM sys.columns c
        JOIN sys.objects o ON c.object_id = o.object_id
        WHERE o.object_id = @objId
          AND c.name = 'WorkflowInstanceId'
          AND c.is_computed = 1)
        ALTER TABLE WFE.t_WFStepInstance DROP COLUMN WorkflowInstanceId;

    IF EXISTS(
        SELECT 1
        FROM sys.columns c
        JOIN sys.objects o ON c.object_id = o.object_id
        WHERE o.object_id = @objId
          AND c.name = 'WorkflowStepId'
          AND c.is_computed = 1)
        ALTER TABLE WFE.t_WFStepInstance DROP COLUMN WorkflowStepId;

    IF EXISTS(
        SELECT 1
        FROM sys.columns c
        JOIN sys.objects o ON c.object_id = o.object_id
        WHERE o.object_id = @objId
          AND c.name = 'AssignedDate'
          AND c.is_computed = 1)
        ALTER TABLE WFE.t_WFStepInstance DROP COLUMN AssignedDate;

    IF EXISTS(
        SELECT 1
        FROM sys.columns c
        JOIN sys.objects o ON c.object_id = o.object_id
        WHERE o.object_id = @objId
          AND c.name = 'CompletedBy'
          AND c.is_computed = 1)
        ALTER TABLE WFE.t_WFStepInstance DROP COLUMN CompletedBy;

    IF EXISTS(
        SELECT 1
        FROM sys.columns c
        JOIN sys.objects o ON c.object_id = o.object_id
        WHERE o.object_id = @objId
          AND c.name = 'OutputData'
          AND c.is_computed = 1)
        ALTER TABLE WFE.t_WFStepInstance DROP COLUMN OutputData;

    IF EXISTS(
        SELECT 1
        FROM sys.columns c
        JOIN sys.objects o ON c.object_id = o.object_id
        WHERE o.object_id = @objId
          AND c.name = 'SLADeadline'
          AND c.is_computed = 1)
        ALTER TABLE WFE.t_WFStepInstance DROP COLUMN SLADeadline;

    IF EXISTS(
        SELECT 1
        FROM sys.columns c
        JOIN sys.objects o ON c.object_id = o.object_id
        WHERE o.object_id = @objId
          AND c.name = 'SLAStatus'
          AND c.is_computed = 1)
        ALTER TABLE WFE.t_WFStepInstance DROP COLUMN SLAStatus;

    ------------------------------------------------------------------
    -- Add physical nullable target columns if they don't exist, then copy
    -- data from candidate columns when available.
    ------------------------------------------------------------------

    -- WorkflowStepInstanceId (INT)
    IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowStepInstanceId') IS NULL
    BEGIN
        ALTER TABLE WFE.t_WFStepInstance ADD WorkflowStepInstanceId INT NULL;
        IF COL_LENGTH('WFE.t_WFStepInstance','WFStepInstanceId') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET WorkflowStepInstanceId = WFStepInstanceId WHERE WorkflowStepInstanceId IS NULL;
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','WFStepInstanceID') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET WorkflowStepInstanceId = WFStepInstanceID WHERE WorkflowStepInstanceId IS NULL;
    END

    -- WorkflowInstanceId (INT)
    IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowInstanceId') IS NULL
    BEGIN
        ALTER TABLE WFE.t_WFStepInstance ADD WorkflowInstanceId INT NULL;
        IF COL_LENGTH('WFE.t_WFStepInstance','WFInstanceId') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET WorkflowInstanceId = WFInstanceId WHERE WorkflowInstanceId IS NULL;
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowInstanceID') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET WorkflowInstanceId = WorkflowInstanceID WHERE WorkflowInstanceId IS NULL;
    END

    -- WorkflowStepId (INT)
    IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowStepId') IS NULL
    BEGIN
        ALTER TABLE WFE.t_WFStepInstance ADD WorkflowStepId INT NULL;
        IF COL_LENGTH('WFE.t_WFStepInstance','WFStepId') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET WorkflowStepId = WFStepId WHERE WorkflowStepId IS NULL;
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowStepID') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET WorkflowStepId = WorkflowStepID WHERE WorkflowStepId IS NULL;
    END

    -- AssignedDate (DATETIME)
    IF COL_LENGTH('WFE.t_WFStepInstance','AssignedDate') IS NULL
    BEGIN
        ALTER TABLE WFE.t_WFStepInstance ADD AssignedDate DATETIME NULL;
        IF COL_LENGTH('WFE.t_WFStepInstance','AssignedOn') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET AssignedDate = AssignedOn WHERE AssignedDate IS NULL;
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','StartedDate') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET AssignedDate = StartedDate WHERE AssignedDate IS NULL;
    END

    -- CompletedBy (nvarchar)
    IF COL_LENGTH('WFE.t_WFStepInstance','CompletedBy') IS NULL
    BEGIN
        ALTER TABLE WFE.t_WFStepInstance ADD CompletedBy NVARCHAR(256) NULL;
        IF COL_LENGTH('WFE.t_WFStepInstance','CompletedByAD') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET CompletedBy = CompletedByAD WHERE CompletedBy IS NULL;
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','CompletedUser') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET CompletedBy = CompletedUser WHERE CompletedBy IS NULL;
    END

    -- OutputData (nvarchar(max))
    IF COL_LENGTH('WFE.t_WFStepInstance','OutputData') IS NULL
    BEGIN
        ALTER TABLE WFE.t_WFStepInstance ADD OutputData NVARCHAR(MAX) NULL;
        IF COL_LENGTH('WFE.t_WFStepInstance','Output') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET OutputData = Output WHERE OutputData IS NULL;
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','Payload') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET OutputData = Payload WHERE OutputData IS NULL;
    END

    -- SLADeadline (DATETIME)
    IF COL_LENGTH('WFE.t_WFStepInstance','SLADeadline') IS NULL
    BEGIN
        ALTER TABLE WFE.t_WFStepInstance ADD SLADeadline DATETIME NULL;
        IF COL_LENGTH('WFE.t_WFStepInstance','DueDate') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET SLADeadline = DueDate WHERE SLADeadline IS NULL;
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','Deadline') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET SLADeadline = Deadline WHERE SLADeadline IS NULL;
    END

    -- SLAStatus (tinyint)
    IF COL_LENGTH('WFE.t_WFStepInstance','SLAStatus') IS NULL
    BEGIN
        ALTER TABLE WFE.t_WFStepInstance ADD SLAStatus TINYINT NULL;
        IF COL_LENGTH('WFE.t_WFStepInstance','SLAState') IS NOT NULL
            UPDATE WFE.t_WFStepInstance SET SLAStatus = SLAState WHERE SLAStatus IS NULL;
    END

    COMMIT TRANSACTION;
    PRINT 'Migration completed: physical alias columns added and populated where candidates were found.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE();
    RAISERROR('Migration failed: %s', 16, 1, @ErrMsg);
END CATCH

-- After running: verify columns and sample rows
-- SELECT TOP 10 WorkflowStepInstanceId, WFStepInstanceId, WorkflowInstanceId, WFInstanceId, WorkflowStepId, WFStepId, AssignedDate, AssignedOn, SLADeadline, DueDate

