-- ============================================================
-- CMS Stored Procedures Script 002
-- All database access goes through these stored procedures.
-- AuditLog is immutable: no UPDATE or DELETE SPs exist for it.
-- ============================================================

-- Create schemas if they don't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'core')
    EXEC('CREATE SCHEMA core');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'storage')
    EXEC('CREATE SCHEMA storage');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'security')
    EXEC('CREATE SCHEMA security');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'workflow')
    EXEC('CREATE SCHEMA workflow');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'audit')
    EXEC('CREATE SCHEMA audit');
GO

-- ============================================================
-- UserAccount Stored Procedures
-- ============================================================

CREATE PROCEDURE core.sp_GetUserAccountByAdIdentity
    @AdIdentity NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, AdIdentity, DisplayName, RoleId, OrgUnitId, IsActive, CreatedAt
    FROM   UserAccount
    WHERE  AdIdentity = @AdIdentity;
END
GO

CREATE PROCEDURE core.sp_GetUserAccountById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, AdIdentity, DisplayName, RoleId, OrgUnitId, IsActive, CreatedAt
    FROM   core.UserAccount
    WHERE  UserId = @UserId;
END
GO

CREATE PROCEDURE core.sp_InsertUserAccount
    @AdIdentity  NVARCHAR(200),
    @DisplayName NVARCHAR(200),
    @RoleId      TINYINT,
    @OrgUnitId   INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO core.UserAccount (AdIdentity, DisplayName, RoleId, OrgUnitId)
    VALUES (@AdIdentity, @DisplayName, @RoleId, @OrgUnitId);
    SELECT SCOPE_IDENTITY() AS NewUserId;
END
GO

CREATE PROCEDURE core.sp_UpdateUserAccount
    @UserId      INT,
    @DisplayName NVARCHAR(200),
    @RoleId      TINYINT,
    @OrgUnitId   INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.UserAccount
    SET    DisplayName = @DisplayName,
           RoleId      = @RoleId,
           OrgUnitId   = @OrgUnitId
    WHERE  UserId = @UserId;
END
GO

CREATE PROCEDURE core.sp_GetUserAccountsByRole
    @RoleId TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, AdIdentity, DisplayName, RoleId, OrgUnitId, IsActive, CreatedAt
    FROM   core.UserAccount
    WHERE  RoleId = @RoleId
      AND  IsActive = 1;
END
GO

CREATE PROCEDURE core.sp_DeactivateUserAccount
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.UserAccount
    SET    IsActive = 0
    WHERE  UserId = @UserId;
END
GO

-- ============================================================
-- OrgUnit Stored Procedures
-- ============================================================

CREATE PROCEDURE core.sp_InsertOrgUnit
    @ParentId INT,
    @Tier     TINYINT,
    @Name     NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO core.OrgUnit (ParentId, Tier, Name)
    VALUES (@ParentId, @Tier, @Name);
    SELECT SCOPE_IDENTITY() AS NewOrgUnitId;
END
GO

CREATE PROCEDURE core.sp_UpdateOrgUnit
    @OrgUnitId INT,
    @Name      NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.OrgUnit
    SET    Name      = @Name,
           UpdatedAt = GETUTCDATE()
    WHERE  OrgUnitId = @OrgUnitId;
END
GO

CREATE PROCEDURE core.sp_GetOrgUnitById
    @OrgUnitId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrgUnitId, ParentId, Tier, Name, IsActive, CreatedAt, UpdatedAt
    FROM   core.OrgUnit
    WHERE  OrgUnitId = @OrgUnitId;
END
GO

CREATE PROCEDURE core.sp_GetOrgUnitHierarchy
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrgUnitId, ParentId, Tier, Name, IsActive, CreatedAt, UpdatedAt
    FROM   core.OrgUnit
    WHERE  IsActive = 1
    ORDER  BY Tier, ParentId, Name;
END
GO

CREATE PROCEDURE core.sp_GetOrgUnitsByConfidentialityLevel
    @ConfidentialityLevel TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT ou.OrgUnitId, ou.ParentId, ou.Tier, ou.Name,
                    ou.IsActive, ou.CreatedAt, ou.UpdatedAt
    FROM   core.OrgUnit ou
    INNER  JOIN storage.Folder f ON f.OrgUnitId = ou.OrgUnitId
    WHERE  ou.IsActive = 1
      AND  f.ConfidentialityLevel = @ConfidentialityLevel
      AND  f.IsDeleted = 0;
END
GO

CREATE PROCEDURE core.sp_HasActiveDependents
    @OrgUnitId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM storage.Folder      WHERE OrgUnitId = @OrgUnitId AND IsDeleted = 0
        UNION ALL
        SELECT 1 FROM core.UserAccount WHERE OrgUnitId = @OrgUnitId AND IsActive  = 1
    )
        SELECT 1 AS HasDependents;
    ELSE
        SELECT 0 AS HasDependents;
END
GO

CREATE PROCEDURE core.sp_DeactivateOrgUnit
    @OrgUnitId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.OrgUnit
    SET    IsActive  = 0,
           UpdatedAt = GETUTCDATE()
    WHERE  OrgUnitId = @OrgUnitId;
END
GO

-- ============================================================
-- Folder Stored Procedures
-- ============================================================

CREATE PROCEDURE storage.sp_InsertFolder
    @ParentFolderId        INT,
    @OrgUnitId             INT,
    @Name                  NVARCHAR(500),
    @Depth                 TINYINT,
    @ConfidentialityLevel  TINYINT,
    @InheritConfidentiality BIT,
    @CreatedByUserId       INT,
    @QuotaBytes            BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO storage.Folder (ParentFolderId, OrgUnitId, Name, Depth,
                        ConfidentialityLevel, InheritConfidentiality,
                        CreatedByUserId, QuotaBytes)
    VALUES (@ParentFolderId, @OrgUnitId, @Name, @Depth,
            @ConfidentialityLevel, @InheritConfidentiality,
            @CreatedByUserId, @QuotaBytes);
    SELECT SCOPE_IDENTITY() AS NewFolderId;
END
GO

CREATE PROCEDURE storage.sp_UpdateFolder
    @FolderId              INT,
    @Name                  NVARCHAR(500),
    @ConfidentialityLevel  TINYINT,
    @InheritConfidentiality BIT,
    @QuotaBytes            BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.Folder
    SET    Name                   = @Name,
           ConfidentialityLevel   = @ConfidentialityLevel,
           InheritConfidentiality = @InheritConfidentiality,
           QuotaBytes             = @QuotaBytes
    WHERE  FolderId = @FolderId;
END
GO

CREATE PROCEDURE storage.sp_GetFolderById
    @FolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FolderId, ParentFolderId, OrgUnitId, Name, Depth,
           ConfidentialityLevel, InheritConfidentiality,
           IsDeleted, IsArchived, DeletedAt,
           CreatedByUserId, CreatedAt, QuotaBytes, TotalUsageBytes
    FROM   storage.Folder
    WHERE  FolderId = @FolderId;
END
GO

CREATE PROCEDURE storage.sp_GetFolderChildren
    @ParentFolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FolderId, ParentFolderId, OrgUnitId, Name, Depth,
           ConfidentialityLevel, InheritConfidentiality,
           IsDeleted, IsArchived, DeletedAt,
           CreatedByUserId, CreatedAt, QuotaBytes, TotalUsageBytes
    FROM   storage.Folder
    WHERE  ParentFolderId = @ParentFolderId
      AND  IsDeleted = 0;
END
GO

CREATE PROCEDURE storage.sp_GetFolderDepth
    @FolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Depth FROM storage.Folder WHERE FolderId = @FolderId;
END
GO

CREATE PROCEDURE storage.sp_GetFolderDescendants
    @FolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH FolderCTE AS (
        SELECT FolderId, ParentFolderId, OrgUnitId, Name, Depth,
               ConfidentialityLevel, InheritConfidentiality,
               IsDeleted, IsArchived, DeletedAt,
               CreatedByUserId, CreatedAt, QuotaBytes, TotalUsageBytes
        FROM   storage.Folder
        WHERE  ParentFolderId = @FolderId
        UNION ALL
        SELECT f.FolderId, f.ParentFolderId, f.OrgUnitId, f.Name, f.Depth,
               f.ConfidentialityLevel, f.InheritConfidentiality,
               f.IsDeleted, f.IsArchived, f.DeletedAt,
               f.CreatedByUserId, f.CreatedAt, f.QuotaBytes, f.TotalUsageBytes
        FROM   storage.Folder f
        INNER  JOIN FolderCTE cte ON f.ParentFolderId = cte.FolderId
    )
    SELECT * FROM FolderCTE;
END
GO

CREATE PROCEDURE storage.sp_MoveFolderToRecycleBin
    @FolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Soft-delete the folder and all descendants
    WITH FolderCTE AS (
        SELECT FolderId FROM storage.Folder WHERE FolderId = @FolderId
        UNION ALL
        SELECT f.FolderId FROM storage.Folder f
        INNER  JOIN FolderCTE cte ON f.ParentFolderId = cte.FolderId
    )
    UPDATE storage.Folder
    SET    IsDeleted = 1,
           DeletedAt = GETUTCDATE()
    WHERE  FolderId IN (SELECT FolderId FROM FolderCTE);
END
GO

CREATE PROCEDURE storage.sp_RestoreFolder
    @FolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH FolderCTE AS (
        SELECT FolderId FROM storage.Folder WHERE FolderId = @FolderId
        UNION ALL
        SELECT f.FolderId FROM storage.Folder f
        INNER  JOIN FolderCTE cte ON f.ParentFolderId = cte.FolderId
    )
    UPDATE storage.Folder
    SET    IsDeleted = 0,
           DeletedAt = NULL
    WHERE  FolderId IN (SELECT FolderId FROM FolderCTE);
END
GO

CREATE PROCEDURE storage.sp_PermanentDeleteFolder
    @FolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Step 1: Collect all descendant folder IDs (including the root)
        WITH FolderCTE AS (
            SELECT FolderId FROM storage.Folder WHERE FolderId = @FolderId
            UNION ALL
            SELECT f.FolderId FROM storage.Folder f
            INNER  JOIN FolderCTE cte ON f.ParentFolderId = cte.FolderId
        )
        -- Step 2: Delete all FileDocument records in those folders (must delete, not soft-delete, to avoid FK constraint)
        DELETE FROM storage.FileDocument
        WHERE  FolderId IN (SELECT FolderId FROM FolderCTE);

        -- Step 3: Delete FolderPermission entries
        WITH FolderCTE AS (
            SELECT FolderId FROM storage.Folder WHERE FolderId = @FolderId
            UNION ALL
            SELECT f.FolderId FROM storage.Folder f
            INNER  JOIN FolderCTE cte ON f.ParentFolderId = cte.FolderId
        )
        DELETE FROM security.FolderPermission
        WHERE  FolderId IN (SELECT FolderId FROM FolderCTE);

        -- Step 4: Delete Folder records (children first via CTE ordering)
        WITH FolderCTE AS (
            SELECT FolderId FROM storage.Folder WHERE FolderId = @FolderId
            UNION ALL
            SELECT f.FolderId FROM storage.Folder f
            INNER  JOIN FolderCTE cte ON f.ParentFolderId = cte.FolderId
        )
        DELETE FROM storage.Folder
        WHERE  FolderId IN (SELECT FolderId FROM FolderCTE);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE PROCEDURE storage.sp_GetRootFoldersByOrgUnit
    @OrgUnitId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FolderId, ParentFolderId, OrgUnitId, Name, Depth,
           ConfidentialityLevel, InheritConfidentiality,
           IsDeleted, IsArchived, DeletedAt,
           CreatedByUserId, CreatedAt, QuotaBytes, TotalUsageBytes
    FROM   storage.Folder
    WHERE  OrgUnitId = @OrgUnitId
      AND  Depth     = 1
      AND  IsDeleted = 0;
END
GO
    @FolderId             INT,
    @OriginalFileName     NVARCHAR(500),
    @StoredPath           NVARCHAR(1000),
    @FileSizeBytes        BIGINT,
    @Sha256Checksum       CHAR(64),
    @ConfidentialityLevel TINYINT,
    @UploaderUserId       INT,
    @ParentVersionId      INT,
    @RetentionExpiresAt   DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO FileDocument (FolderId, OriginalFileName, StoredPath, FileSizeBytes,
                              Sha256Checksum, ConfidentialityLevel, UploaderUserId,
                              ParentVersionId, RetentionExpiresAt)
    VALUES (@FolderId, @OriginalFileName, @StoredPath, @FileSizeBytes,
            @Sha256Checksum, @ConfidentialityLevel, @UploaderUserId,
            @ParentVersionId, @RetentionExpiresAt);
    SELECT SCOPE_IDENTITY() AS NewFileDocumentId;
END
GO

CREATE PROCEDURE sp_GetFileDocumentById
    @FileDocumentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileDocumentId, FolderId, OriginalFileName, StoredPath, FileSizeBytes,
           Sha256Checksum, ConfidentialityLevel, UploaderUserId, UploadedAt,
           IsDeleted, DeletedAt, ParentVersionId, RetentionExpiresAt, RetentionFlagged
    FROM   FileDocument
    WHERE  FileDocumentId = @FileDocumentId;
END
GO

CREATE PROCEDURE sp_GetFileDocumentsByFolder
    @FolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileDocumentId, FolderId, OriginalFileName, StoredPath, FileSizeBytes,
           Sha256Checksum, ConfidentialityLevel, UploaderUserId, UploadedAt,
           IsDeleted, DeletedAt, ParentVersionId, RetentionExpiresAt, RetentionFlagged
    FROM   FileDocument
    WHERE  FolderId = @FolderId
      AND  IsDeleted = 0;
END
GO

CREATE PROCEDURE sp_SoftDeleteFileDocument
    @FileDocumentId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE FileDocument
    SET    IsDeleted = 1,
           DeletedAt = GETUTCDATE()
    WHERE  FileDocumentId = @FileDocumentId;
END
GO

CREATE PROCEDURE sp_FlagFileDocumentRetention
    @FileDocumentId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE FileDocument
    SET    RetentionFlagged = 1
    WHERE  FileDocumentId = @FileDocumentId;
END
GO

CREATE PROCEDURE sp_GetFolderTotalUsageBytes
    @FolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH FolderCTE AS (
        SELECT FolderId FROM Folder WHERE FolderId = @FolderId
        UNION ALL
        SELECT f.FolderId FROM Folder f
        INNER  JOIN FolderCTE cte ON f.ParentFolderId = cte.FolderId
    )
    SELECT ISNULL(SUM(fd.FileSizeBytes), 0) AS TotalUsageBytes
    FROM   FileDocument fd
    WHERE  fd.FolderId IN (SELECT FolderId FROM FolderCTE)
      AND  fd.IsDeleted = 0;
END
GO

CREATE PROCEDURE sp_GetFileDocumentByChecksum
    @Sha256Checksum CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileDocumentId, FolderId, OriginalFileName, StoredPath, FileSizeBytes,
           Sha256Checksum, ConfidentialityLevel, UploaderUserId, UploadedAt,
           IsDeleted, DeletedAt, ParentVersionId, RetentionExpiresAt, RetentionFlagged
    FROM   FileDocument
    WHERE  Sha256Checksum = @Sha256Checksum
      AND  IsDeleted = 0;
END
GO

-- ============================================================
-- FolderPermission Stored Procedures
-- ============================================================

CREATE PROCEDURE sp_GetAclEntries
    @FolderId      INT,
    @PrincipalType TINYINT,
    @PrincipalId   INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PermissionId, FolderId, PrincipalType, PrincipalId,
           PermissionType, PermissionState, InheritanceBroken,
           GrantedByUserId, GrantedAt
    FROM   FolderPermission
    WHERE  FolderId = @FolderId
      AND  (@PrincipalType IS NULL OR PrincipalType = @PrincipalType)
      AND  (@PrincipalId   IS NULL OR PrincipalId   = @PrincipalId);
END
GO

CREATE PROCEDURE sp_InsertFolderPermission
    @FolderId         INT,
    @PrincipalType    TINYINT,
    @PrincipalId      INT,
    @PermissionType   TINYINT,
    @PermissionState  TINYINT,
    @InheritanceBroken BIT,
    @GrantedByUserId  INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO FolderPermission (FolderId, PrincipalType, PrincipalId,
                                  PermissionType, PermissionState,
                                  InheritanceBroken, GrantedByUserId)
    VALUES (@FolderId, @PrincipalType, @PrincipalId,
            @PermissionType, @PermissionState,
            @InheritanceBroken, @GrantedByUserId);
    SELECT SCOPE_IDENTITY() AS NewPermissionId;
END
GO

CREATE PROCEDURE sp_DeleteFolderPermission
    @PermissionId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM FolderPermission WHERE PermissionId = @PermissionId;
END
GO

CREATE PROCEDURE sp_GetPermissionsByFolder
    @FolderId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PermissionId, FolderId, PrincipalType, PrincipalId,
           PermissionType, PermissionState, InheritanceBroken,
           GrantedByUserId, GrantedAt
    FROM   FolderPermission
    WHERE  FolderId = @FolderId;
END
GO

-- ============================================================
-- ApprovalRequest Stored Procedures
-- ============================================================

CREATE PROCEDURE sp_InsertApprovalRequest
    @FolderId          INT,
    @RequestorUserId   INT,
    @TimeoutAt         DATETIME2,
    @PermissionChanges NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ApprovalRequest (FolderId, RequestorUserId, TimeoutAt, PermissionChanges)
    VALUES (@FolderId, @RequestorUserId, @TimeoutAt, @PermissionChanges);
    SELECT SCOPE_IDENTITY() AS NewApprovalRequestId;
END
GO

CREATE PROCEDURE sp_InsertApprovalStep
    @ApprovalRequestId INT,
    @StepOrder         INT,
    @ApproverType      TINYINT,
    @ApproverId        INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ApprovalStep (ApprovalRequestId, StepOrder, ApproverType, ApproverId)
    VALUES (@ApprovalRequestId, @StepOrder, @ApproverType, @ApproverId);
    SELECT SCOPE_IDENTITY() AS NewApprovalStepId;
END
GO

CREATE PROCEDURE sp_GetApprovalRequest
    @ApprovalRequestId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ApprovalRequestId, FolderId, RequestorUserId, Status,
           CreatedAt, TimeoutAt, PermissionChanges
    FROM   ApprovalRequest
    WHERE  ApprovalRequestId = @ApprovalRequestId;
END
GO

CREATE PROCEDURE sp_GetPendingApprovalSteps
    @ApprovalRequestId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ApprovalStepId, ApprovalRequestId, StepOrder, ApproverType,
           ApproverId, Status, ActedAt, Comments
    FROM   ApprovalStep
    WHERE  ApprovalRequestId = @ApprovalRequestId
      AND  Status = 0  -- Pending
    ORDER  BY StepOrder;
END
GO

CREATE PROCEDURE sp_UpdateApprovalStepStatus
    @ApprovalStepId INT,
    @Status         TINYINT,
    @Comments       NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ApprovalStep
    SET    Status   = @Status,
           ActedAt  = GETUTCDATE(),
           Comments = @Comments
    WHERE  ApprovalStepId = @ApprovalStepId;
END
GO

CREATE PROCEDURE sp_UpdateApprovalRequestStatus
    @ApprovalRequestId INT,
    @Status            TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ApprovalRequest
    SET    Status = @Status
    WHERE  ApprovalRequestId = @ApprovalRequestId;
END
GO

-- ============================================================
-- AuditLog Stored Procedures (INSERT ONLY — immutable)
-- ============================================================

CREATE PROCEDURE audit.sp_InsertAuditLog
    @UserID       INT,
    @UserName     NVARCHAR(200),
    @UserRole     TINYINT,
    @Module       NVARCHAR(100),
    @Action       NVARCHAR(100),
    @TableName    NVARCHAR(100),
    @RecordID     INT,
    @OldValue     NVARCHAR(MAX),
    @NewValue     NVARCHAR(MAX),
    @IPAddress    NVARCHAR(50),
    @MachineName  NVARCHAR(200),
    @SessionID    NVARCHAR(100),
    @Result       TINYINT,
    @ErrorMessage NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AuditLog (
        UserID, UserName, UserRole, Module, Action,
        TableName, RecordID, OldValue, NewValue,
        IPAddress, MachineName, SessionID, Result, ErrorMessage
    )
    VALUES (
        @UserID, @UserName, @UserRole, @Module, @Action,
        @TableName, @RecordID, @OldValue, @NewValue,
        @IPAddress, @MachineName, @SessionID, @Result, @ErrorMessage
    );
    SELECT SCOPE_IDENTITY() AS AuditID;
END
GO

CREATE PROCEDURE audit.sp_QueryAuditLog
    @FromDate   DATETIME2,
    @ToDate     DATETIME2,
    @UserID     INT,
    @Module     NVARCHAR(100),
    @Action     NVARCHAR(100),
    @Result     TINYINT,
    @PageNumber INT,
    @PageSize   INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT COUNT(*) OVER () AS TotalCount,
           AuditID, AuditDate, UserID, UserName, UserRole,
           Module, Action, TableName, RecordID,
           OldValue, NewValue, IPAddress, MachineName,
           SessionID, Result, ErrorMessage
    FROM   AuditLog
    WHERE  AuditDate >= @FromDate
      AND  AuditDate <= @ToDate
      AND  (@UserID  IS NULL OR UserID  = @UserID)
      AND  (@Module  IS NULL OR Module  = @Module)
      AND  (@Action  IS NULL OR Action  = @Action)
      AND  (@Result  IS NULL OR Result  = @Result)
    ORDER  BY AuditDate DESC
    OFFSET @Offset ROWS
    FETCH  NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================
-- Notification Stored Procedures
-- ============================================================

CREATE PROCEDURE sp_InsertNotification
    @RecipientUserId INT,
    @EventType       NVARCHAR(100),
    @RelatedEntityId NVARCHAR(100),
    @MessageText     NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Notification (RecipientUserId, EventType, RelatedEntityId, MessageText)
    VALUES (@RecipientUserId, @EventType, @RelatedEntityId, @MessageText);
    SELECT SCOPE_IDENTITY() AS NewNotificationId;
END
GO

CREATE PROCEDURE sp_GetUnreadNotificationsByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NotificationId, RecipientUserId, EventType, RelatedEntityId,
           MessageText, IsRead, DeliveredAt, ReadAt
    FROM   Notification
    WHERE  RecipientUserId = @UserId
      AND  IsRead = 0
    ORDER  BY DeliveredAt DESC;
END
GO

CREATE PROCEDURE sp_MarkNotificationRead
    @NotificationId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Notification
    SET    IsRead = 1,
           ReadAt = GETUTCDATE()
    WHERE  NotificationId = @NotificationId;
END
GO

-- ============================================================
-- Additional Folder Stored Procedure (added for FileService)
-- ============================================================

CREATE PROCEDURE sp_GetRootFoldersByOrgUnit
    @OrgUnitId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FolderId, ParentFolderId, OrgUnitId, Name, Depth,
           ConfidentialityLevel, InheritConfidentiality,
           IsDeleted, IsArchived, DeletedAt,
           CreatedByUserId, CreatedAt, QuotaBytes, TotalUsageBytes
    FROM   Folder
    WHERE  OrgUnitId = @OrgUnitId
      AND  Depth     = 1
      AND  IsDeleted = 0;
END
GO
