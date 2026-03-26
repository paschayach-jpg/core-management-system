-- ============================================================
-- 014_FilePermission_Table_And_SPs.sql
-- Creates security.FilePermission table and stored procedures
-- ============================================================

-- Table
IF NOT EXISTS (SELECT 1 FROM sys.tables t
               JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = 'security' AND t.name = 'FilePermission')
BEGIN
    CREATE TABLE security.FilePermission (
        FilePermissionID INT          IDENTITY(1,1) PRIMARY KEY,
        FileID           INT          NOT NULL,
        PrincipalType    TINYINT      NOT NULL,  -- 1=User, 2=Role, 3=OrgUnit, 4=Group
        PrincipalID      INT          NOT NULL,
        CanRead          BIT          NOT NULL DEFAULT 0,
        CanWrite         BIT          NOT NULL DEFAULT 0,
        CanDelete        BIT          NOT NULL DEFAULT 0,
        CanModify        BIT          NOT NULL DEFAULT 0,
        CanFullControl   BIT          NOT NULL DEFAULT 0,
        IsDeny           BIT          NOT NULL DEFAULT 0,
        ExpiryDate       DATETIME     NULL,
        GrantedBy        INT          NOT NULL,
        GrantedDate      DATETIME     NOT NULL DEFAULT GETDATE(),
        RevokedBy        INT          NULL,
        RevokedDate      DATETIME     NULL,
        RevokeReason     NVARCHAR(500) NULL
    );
END
GO

-- ── sp_InsertFilePermission ──────────────────────────────────────────────────
IF OBJECT_ID('security.sp_InsertFilePermission', 'P') IS NOT NULL
    DROP PROCEDURE security.sp_InsertFilePermission;
GO
CREATE PROCEDURE security.sp_InsertFilePermission
    @FileID         INT,
    @PrincipalType  TINYINT,
    @PrincipalID    INT,
    @CanRead        BIT,
    @CanWrite       BIT,
    @CanDelete      BIT,
    @CanModify      BIT,
    @CanFullControl BIT,
    @IsDeny         BIT,
    @ExpiryDate     DATETIME = NULL,
    @GrantedBy      INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO security.FilePermission
        (FileID, PrincipalType, PrincipalID, CanRead, CanWrite, CanDelete,
         CanModify, CanFullControl, IsDeny, ExpiryDate, GrantedBy, GrantedDate)
    VALUES
        (@FileID, @PrincipalType, @PrincipalID, @CanRead, @CanWrite, @CanDelete,
         @CanModify, @CanFullControl, @IsDeny, @ExpiryDate, @GrantedBy, GETDATE());
    SELECT SCOPE_IDENTITY();
END
GO

-- ── sp_GetFilePermissionsByFile (active only) ────────────────────────────────
IF OBJECT_ID('security.sp_GetFilePermissionsByFile', 'P') IS NOT NULL
    DROP PROCEDURE security.sp_GetFilePermissionsByFile;
GO
CREATE PROCEDURE security.sp_GetFilePermissionsByFile
    @FileID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM security.FilePermission
    WHERE FileID = @FileID AND RevokedDate IS NULL;
END
GO

-- ── sp_GetAllFilePermissionsByFile (includes revoked) ───────────────────────
IF OBJECT_ID('security.sp_GetAllFilePermissionsByFile', 'P') IS NOT NULL
    DROP PROCEDURE security.sp_GetAllFilePermissionsByFile;
GO
CREATE PROCEDURE security.sp_GetAllFilePermissionsByFile
    @FileID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM security.FilePermission
    WHERE FileID = @FileID
    ORDER BY GrantedDate DESC;
END
GO

-- ── sp_RevokeFilePermission ──────────────────────────────────────────────────
IF OBJECT_ID('security.sp_RevokeFilePermission', 'P') IS NOT NULL
    DROP PROCEDURE security.sp_RevokeFilePermission;
GO
CREATE PROCEDURE security.sp_RevokeFilePermission
    @FilePermissionID INT,
    @RevokedBy        INT,
    @RevokeReason     NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE security.FilePermission
    SET RevokedBy     = @RevokedBy,
        RevokedDate   = GETDATE(),
        RevokeReason  = @RevokeReason
    WHERE FilePermissionID = @FilePermissionID;
END
GO
