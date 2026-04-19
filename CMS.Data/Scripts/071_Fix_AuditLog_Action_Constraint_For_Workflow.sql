/*
    071_Fix_AuditLog_Action_Constraint_For_Workflow.sql
    Purpose:
      - Fix missing audit logs for workflow/custom actions (e.g. REQUEST_CREATE_HC)
      - Previous constraint CK_aud_AuditLog_Action allowed only fixed verbs
        and blocked insert in audit.sp_InsertAuditLog.
*/

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (
        SELECT 1
        FROM sys.check_constraints cc
        INNER JOIN sys.tables t ON cc.parent_object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE cc.name = 'CK_aud_AuditLog_Action'
          AND s.name = 'audit'
          AND t.name = 'AuditLog'
    )
    BEGIN
        ALTER TABLE audit.AuditLog DROP CONSTRAINT CK_aud_AuditLog_Action;
    END;

    -- Allow any non-empty action code (supports custom workflow actions).
    ALTER TABLE audit.AuditLog
    ADD CONSTRAINT CK_aud_AuditLog_Action
    CHECK (LEN(LTRIM(RTRIM(ISNULL(Action, '')))) > 0);

    COMMIT TRANSACTION;

    PRINT 'OK: CK_aud_AuditLog_Action updated to allow custom action codes.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @Line INT = ERROR_LINE();
    DECLARE @Num INT = ERROR_NUMBER();

    RAISERROR('071 migration failed (Line %d, Error %d): %s', 16, 1, @Line, @Num, @Err);
END CATCH;

-- Optional verification
SELECT name, definition
FROM sys.check_constraints
WHERE name = 'CK_aud_AuditLog_Action';
