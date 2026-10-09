-- ============================================================
-- 071_Share_Batch_Request.sql
-- Batch internal share requests and per-recipient permission support
-- ============================================================

-- ── 1. Create workflow.ApprovalRequestItem table ────────────
IF OBJECT_ID(N'workflow.ApprovalRequestItem', N'U') IS NULL
BEGIN
    CREATE TABLE workflow.ApprovalRequestItem (
        ItemID              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RequestID           INT NOT NULL,
        FileID              INT NULL,
        FolderID            INT NULL,
        PrincipalType       INT NOT NULL,
        PrincipalID         INT NOT NULL,
        CanRead             BIT NOT NULL CONSTRAINT DF_ApprovalRequestItem_CanRead DEFAULT (1),
        CanWrite            BIT NOT NULL CONSTRAINT DF_ApprovalRequestItem_CanWrite DEFAULT (0),
        CanDelete           BIT NOT NULL CONSTRAINT DF_ApprovalRequestItem_CanDelete DEFAULT (0),
        CanDownload         BIT NOT NULL CONSTRAINT DF_ApprovalRequestItem_CanDownload DEFAULT (0),
        ExpiryDate          DATETIME2 NULL,
        AppliedPermissionID INT NULL,
        CONSTRAINT FK_ApprovalRequestItem_Request FOREIGN KEY (RequestID)
            REFERENCES workflow.ApprovalRequest (RequestID) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_ApprovalRequestItem_RequestID 
        ON workflow.ApprovalRequestItem (RequestID);

    PRINT 'Created workflow.ApprovalRequestItem table.';
END
GO

-- ── 2. Add CanDownload column to security.FilePermission ────
IF COL_LENGTH(N'security.FilePermission', N'CanDownload') IS NULL
BEGIN
    ALTER TABLE security.FilePermission
        ADD CanDownload BIT NOT NULL CONSTRAINT DF_FilePermission_CanDownload DEFAULT (0);
    PRINT 'Added security.FilePermission.CanDownload column.';
END
GO

-- ── 3. sp_InsertFilePermission supporting CanDownload ──────
IF OBJECT_ID(N'security.sp_InsertFilePermission', N'P') IS NULL
    EXEC (N'CREATE PROCEDURE [security].[sp_InsertFilePermission] AS RETURN;');
GO

ALTER PROCEDURE [security].[sp_InsertFilePermission]
    @FileID         INT,
    @PrincipalType  TINYINT,
    @PrincipalID    INT,
    @CanRead        BIT = 1,
    @CanWrite       BIT = 0,
    @CanDelete      BIT = 0,
    @CanModify      BIT = 0,
    @CanFullControl BIT = 0,
    @IsDeny         BIT = 0,
    @ExpiryDate     DATETIME2 = NULL,
    @GrantedBy      INT,
    @CanDownload    BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO security.FilePermission (
        FileID, PrincipalType, PrincipalID,
        CanRead, CanWrite, CanDelete, CanModify, CanFullControl, CanDownload,
        IsDeny, ExpiryDate, GrantedBy, GrantedDate
    )
    VALUES (
        @FileID, @PrincipalType, @PrincipalID,
        @CanRead, @CanWrite, @CanDelete, @CanModify, @CanFullControl, @CanDownload,
        @IsDeny, @ExpiryDate, @GrantedBy, GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS FilePermissionID;
END
GO

-- ── 4. sp_GetFilePermissionsByFile supporting CanDownload ───
IF OBJECT_ID(N'security.sp_GetFilePermissionsByFile', N'P') IS NULL
    EXEC (N'CREATE PROCEDURE [security].[sp_GetFilePermissionsByFile] AS RETURN;');
GO

ALTER PROCEDURE [security].[sp_GetFilePermissionsByFile]
    @FileID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        FilePermissionID, FileID, PrincipalType, PrincipalID,
        CanRead, CanWrite, CanDelete, CanModify, CanFullControl, CanDownload,
        IsDeny, ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
    FROM security.FilePermission
    WHERE FileID = @FileID AND RevokedDate IS NULL
    ORDER BY GrantedDate DESC;
END
GO

-- ── 5. sp_GetAllFilePermissionsByFile supporting CanDownload 
IF OBJECT_ID(N'security.sp_GetAllFilePermissionsByFile', N'P') IS NULL
    EXEC (N'CREATE PROCEDURE [security].[sp_GetAllFilePermissionsByFile] AS RETURN;');
GO

ALTER PROCEDURE [security].[sp_GetAllFilePermissionsByFile]
    @FileID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        FilePermissionID, FileID, PrincipalType, PrincipalID,
        CanRead, CanWrite, CanDelete, CanModify, CanFullControl, CanDownload,
        IsDeny, ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
    FROM security.FilePermission
    WHERE FileID = @FileID
    ORDER BY GrantedDate DESC;
END
GO

-- ── 6. Ensure 'INTERNAL_SHARE' is permitted in ApprovalRequest Type constraint ───
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_wf_ApprovalRequest_Type')
BEGIN
    ALTER TABLE workflow.ApprovalRequest DROP CONSTRAINT CK_wf_ApprovalRequest_Type;
    ALTER TABLE workflow.ApprovalRequest WITH CHECK ADD CONSTRAINT CK_wf_ApprovalRequest_Type 
        CHECK (RequestType IN ('CREATE_HC', 'DELETE_FOLDER', 'CHANGE_PERMISSION', 'UPLOAD_HC', 'EXTERNAL_SHARE', 'DOWNLOAD_FILE', 'INTERNAL_SHARE'));
    PRINT 'Updated CK_wf_ApprovalRequest_Type check constraint.';
END
GO
