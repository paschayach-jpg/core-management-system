/*
    074_Fix_AuditLog_Insert_AuditDate.sql
    Purpose:
      - Fix insert failure when AuditDate is NULL in audit.sp_InsertAuditLog
      - Ensure audit.AuditLog.AuditDate has a default constraint
      - Make procedure robust with @AuditDate optional parameter + GETDATE fallback
*/

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('audit.AuditLog', 'U') IS NULL
        THROW 50001, 'Table audit.AuditLog does not exist.', 1;

    IF OBJECT_ID('audit.sp_InsertAuditLog', 'P') IS NULL
        THROW 50002, 'Procedure audit.sp_InsertAuditLog does not exist.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c
            ON c.object_id = dc.parent_object_id
           AND c.column_id = dc.parent_column_id
        INNER JOIN sys.tables t
            ON t.object_id = c.object_id
        INNER JOIN sys.schemas s
            ON s.schema_id = t.schema_id
        WHERE s.name = 'audit'
          AND t.name = 'AuditLog'
          AND c.name = 'AuditDate'
    )
    BEGIN
        ALTER TABLE audit.AuditLog
        ADD CONSTRAINT DF_aud_AuditLog_Date DEFAULT (GETDATE()) FOR AuditDate;
    END;

    EXEC('CREATE OR ALTER PROCEDURE audit.sp_InsertAuditLog
        @UserID       INT,
        @UserName     NVARCHAR(100),
        @UserRole     TINYINT,
        @Module       VARCHAR(50),
        @Action       VARCHAR(50),
        @TableName    VARCHAR(100),
        @RecordID     INT,
        @OldValue     NVARCHAR(MAX),
        @NewValue     NVARCHAR(MAX),
        @IPAddress    VARCHAR(45),
        @MachineName  NVARCHAR(100),
        @SessionID    NVARCHAR(100),
        @Result       TINYINT,
        @ErrorMessage NVARCHAR(500),
        @AuditDate    DATETIME = NULL
    AS
    BEGIN
        SET NOCOUNT ON;

        INSERT INTO audit.AuditLog
            (AuditDate, UserID, UserName, UserRole, Module, Action,
             TableName, RecordID, OldValue, NewValue,
             IPAddress, MachineName, SessionID, Result, ErrorMessage)
        VALUES
            (ISNULL(@AuditDate, GETDATE()), @UserID, @UserName, @UserRole, @Module, @Action,
             @TableName, @RecordID, @OldValue, @NewValue,
             @IPAddress, @MachineName, @SessionID, @Result, @ErrorMessage);

        SELECT CONVERT(BIGINT, SCOPE_IDENTITY()) AS NewAuditID;
    END');

    COMMIT TRANSACTION;

    PRINT 'OK: audit.sp_InsertAuditLog fixed and AuditDate default constraint verified.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @Line INT = ERROR_LINE();
    DECLARE @Num INT = ERROR_NUMBER();

    RAISERROR('074 migration failed (Line %d, Error %d): %s', 16, 1, @Line, @Num, @Err);
END CATCH;

-- Optional verification
SELECT
    OBJECT_ID('audit.sp_InsertAuditLog', 'P') AS InsertProcId,
    OBJECT_ID('audit.AuditLog', 'U') AS AuditTableId;

SELECT dc.name AS DefaultConstraintName,
       OBJECT_NAME(dc.parent_object_id) AS TableName,
       c.name AS ColumnName,
       dc.definition
FROM sys.default_constraints dc
INNER JOIN sys.columns c
    ON c.object_id = dc.parent_object_id
   AND c.column_id = dc.parent_column_id
INNER JOIN sys.tables t
    ON t.object_id = c.object_id
INNER JOIN sys.schemas s
    ON s.schema_id = t.schema_id
WHERE s.name = 'audit'
  AND t.name = 'AuditLog'
  AND c.name = 'AuditDate';
