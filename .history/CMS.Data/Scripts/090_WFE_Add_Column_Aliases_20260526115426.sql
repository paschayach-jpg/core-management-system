-- 090_WFE_Add_Column_Aliases.sql
-- Purpose: Add alias/computed columns to WFE.t_WFStepInstance so legacy code
--          or stored procedures referencing names like WorkflowStepInstanceId
--          continue to work when the physical columns use WF-prefixed names.
-- Usage: Review, then run in the target database (SSMS / sqlcmd).

SET NOCOUNT ON;

-- Change this to the target database if running outside of current DB context
-- USE [YourDatabase];

BEGIN TRY
    BEGIN TRANSACTION;

    ------------------------------------------------------------------
    -- Helper: create computed alias if candidate exists, otherwise create
    -- a nullable column and copy values from candidate if available.
    ------------------------------------------------------------------

    -- WorkflowStepInstanceId <- WFStepInstanceId | WFStepInstanceID
    IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowStepInstanceId') IS NULL
    BEGIN
        IF COL_LENGTH('WFE.t_WFStepInstance','WFStepInstanceId') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD WorkflowStepInstanceId AS WFStepInstanceId;
        END
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','WFStepInstanceID') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD WorkflowStepInstanceId AS WFStepInstanceID;
        END
        ELSE
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD WorkflowStepInstanceId INT NULL;
        END
    END

    -- WorkflowInstanceId <- WFInstanceId | WorkflowInstanceID
    IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowInstanceId') IS NULL
    BEGIN
        IF COL_LENGTH('WFE.t_WFStepInstance','WFInstanceId') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD WorkflowInstanceId AS WFInstanceId;
        END
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowInstanceID') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD WorkflowInstanceId AS WorkflowInstanceID;
        END
        ELSE
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD WorkflowInstanceId INT NULL;
        END
    END

    -- WorkflowStepId <- WFStepId | WorkflowStepID
    IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowStepId') IS NULL
    BEGIN
        IF COL_LENGTH('WFE.t_WFStepInstance','WFStepId') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD WorkflowStepId AS WFStepId;
        END
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','WorkflowStepID') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD WorkflowStepId AS WorkflowStepID;
        END
        ELSE
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD WorkflowStepId INT NULL;
        END
    END

    -- AssignedDate <- AssignedOn | StartedDate
    IF COL_LENGTH('WFE.t_WFStepInstance','AssignedDate') IS NULL
    BEGIN
        IF COL_LENGTH('WFE.t_WFStepInstance','AssignedOn') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD AssignedDate AS AssignedOn;
        END
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','StartedDate') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD AssignedDate AS StartedDate;
        END
        ELSE
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD AssignedDate DATETIME NULL;
        END
    END

    -- CompletedBy <- CompletedByAD | CompletedUser
    IF COL_LENGTH('WFE.t_WFStepInstance','CompletedBy') IS NULL
    BEGIN
        IF COL_LENGTH('WFE.t_WFStepInstance','CompletedByAD') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD CompletedBy AS CompletedByAD;
        END
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','CompletedUser') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD CompletedBy AS CompletedUser;
        END
        ELSE
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD CompletedBy NVARCHAR(256) NULL;
        END
    END

    -- OutputData <- Output | Payload
    IF COL_LENGTH('WFE.t_WFStepInstance','OutputData') IS NULL
    BEGIN
        IF COL_LENGTH('WFE.t_WFStepInstance','OutputData') IS NOT NULL
        BEGIN
            -- redundant, but safe: already exists
        END
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','Output') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD OutputData AS Output;
        END
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','Payload') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD OutputData AS Payload;
        END
        ELSE
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD OutputData NVARCHAR(MAX) NULL;
        END
    END

    -- SLADeadline <- DueDate | Deadline
    IF COL_LENGTH('WFE.t_WFStepInstance','SLADeadline') IS NULL
    BEGIN
        IF COL_LENGTH('WFE.t_WFStepInstance','DueDate') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD SLADeadline AS DueDate;
        END
        ELSE IF COL_LENGTH('WFE.t_WFStepInstance','Deadline') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD SLADeadline AS Deadline;
        END
        ELSE
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD SLADeadline DATETIME NULL;
        END
    END

    -- SLAStatus <- SLAState
    IF COL_LENGTH('WFE.t_WFStepInstance','SLAStatus') IS NULL
    BEGIN
        IF COL_LENGTH('WFE.t_WFStepInstance','SLAState') IS NOT NULL
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD SLAStatus AS SLAState;
        END
        ELSE
        BEGIN
            ALTER TABLE WFE.t_WFStepInstance ADD SLAStatus TINYINT NULL;
        END
    END

    COMMIT TRANSACTION;
    PRINT 'Migration completed: alias columns added where candidates were found.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE();
    RAISERROR('Migration failed: %s', 16, 1, @ErrMsg);
END CATCH

-- Notes:
-- - This script is intentionally conservative and idempotent: re-running is safe.
-- - Computed alias columns are added where a likely source column exists.
-- - If no candidate exists, a nullable column is added instead to avoid breaking callers.
-- - After running, test the application and stored procedures. If you prefer to
--   rename physical columns or update stored procedures instead, review the
--   produced alias columns and adapt as needed.
