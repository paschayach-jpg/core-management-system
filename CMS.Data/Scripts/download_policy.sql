-- ============================================================
-- Download policy: who may download a file without an approval request.
--   1) EMS.FolderAuthorityConfig.CanDownload  — per Level / position / department / division × confidentiality box
--   2) workflow.DownloadUserPermission         — per user × confidentiality box (HC/SC/NC/PC)
--   Admins (AppRole 4) always download directly (enforced in DownloadPolicyService).
-- EMS.sp_ResolveFolderAuthority is re-created with the same matching logic plus the CanDownload column.
-- Rename with the next schema script number before committing. Safe to re-run.
-- ============================================================

-- ── 1. CanDownload on FolderAuthorityConfig ─────────────────────────────────
IF COL_LENGTH(N'EMS.FolderAuthorityConfig', N'CanDownload') IS NULL
BEGIN
    ALTER TABLE EMS.FolderAuthorityConfig
        ADD CanDownload BIT NOT NULL CONSTRAINT DF_FolderAuthorityConfig_CanDownload DEFAULT (0);
    PRINT 'Added EMS.FolderAuthorityConfig.CanDownload';
END
GO

-- ── 2. sp_ResolveFolderAuthority returns CanDownload ────────────────────────
IF OBJECT_ID(N'EMS.sp_ResolveFolderAuthority', N'P') IS NULL
    EXEC (N'CREATE PROCEDURE [EMS].[sp_ResolveFolderAuthority] AS RETURN;');
GO

ALTER PROCEDURE [EMS].[sp_ResolveFolderAuthority]
    @LevelCode      NVARCHAR(20)  = NULL,
    @PositionName   NVARCHAR(200) = NULL,
    @DepartmentCode NVARCHAR(50)  = NULL,
    @DivisionCode   NVARCHAR(50)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Priority 0: LevelCode + DepartmentCode
    IF @LevelCode IS NOT NULL AND @DepartmentCode IS NOT NULL
       AND EXISTS (SELECT 1 FROM EMS.FolderAuthorityConfig
                   WHERE IsActive = 1 AND LevelCode = @LevelCode AND DepartmentCode = @DepartmentCode)
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove, CanDownload,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   EMS.FolderAuthorityConfig
        WHERE  IsActive = 1 AND LevelCode = @LevelCode AND DepartmentCode = @DepartmentCode
        ORDER BY ConfidentialityLevel;
        RETURN;
    END

    -- Priority 1: LevelCode only
    IF @LevelCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM EMS.FolderAuthorityConfig
        WHERE IsActive = 1 AND LevelCode = @LevelCode AND (DepartmentCode IS NULL OR DepartmentCode = ''))
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove, CanDownload,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   EMS.FolderAuthorityConfig
        WHERE  IsActive = 1 AND LevelCode = @LevelCode AND (DepartmentCode IS NULL OR DepartmentCode = '')
        ORDER BY ConfidentialityLevel;
        RETURN;
    END

    -- Priority 2: PositionName
    IF @PositionName IS NOT NULL AND EXISTS (
        SELECT 1 FROM EMS.FolderAuthorityConfig WHERE IsActive = 1 AND PositionName = @PositionName)
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove, CanDownload,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   EMS.FolderAuthorityConfig
        WHERE  IsActive = 1 AND PositionName = @PositionName
        ORDER BY ConfidentialityLevel;
        RETURN;
    END

    -- Priority 3: DepartmentCode
    IF @DepartmentCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM EMS.FolderAuthorityConfig
        WHERE IsActive = 1 AND DepartmentCode = @DepartmentCode
          AND PositionName IS NULL AND (LevelCode IS NULL OR LevelCode = ''))
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove, CanDownload,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   EMS.FolderAuthorityConfig
        WHERE  IsActive = 1 AND DepartmentCode = @DepartmentCode
          AND  PositionName IS NULL AND (LevelCode IS NULL OR LevelCode = '')
        ORDER BY ConfidentialityLevel;
        RETURN;
    END

    -- Priority 4: DivisionCode
    SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
           LevelCode, ConfidentialityLevel,
           CanView, CanAdd, CanEdit, CanApprove, CanDownload,
           IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
    FROM   EMS.FolderAuthorityConfig
    WHERE  IsActive = 1 AND DivisionCode = @DivisionCode
      AND  PositionName IS NULL AND DepartmentCode IS NULL AND (LevelCode IS NULL OR LevelCode = '')
    ORDER BY ConfidentialityLevel;
END
GO

-- ── 3. Per-user direct-download permission ──────────────────────────────────
IF OBJECT_ID(N'workflow.DownloadUserPermission', N'U') IS NULL
BEGIN
    CREATE TABLE workflow.DownloadUserPermission
    (
        PermissionID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DownloadUserPermission PRIMARY KEY,
        UserID       INT      NOT NULL,
        CanHC        BIT      NOT NULL CONSTRAINT DF_DownloadUserPermission_HC DEFAULT (0),
        CanSC        BIT      NOT NULL CONSTRAINT DF_DownloadUserPermission_SC DEFAULT (0),
        CanNC        BIT      NOT NULL CONSTRAINT DF_DownloadUserPermission_NC DEFAULT (0),
        CanPC        BIT      NOT NULL CONSTRAINT DF_DownloadUserPermission_PC DEFAULT (0),
        IsActive     BIT      NOT NULL CONSTRAINT DF_DownloadUserPermission_IsActive DEFAULT (1),
        CreatedBy    INT      NULL,
        CreatedDate  DATETIME NOT NULL CONSTRAINT DF_DownloadUserPermission_CreatedDate DEFAULT (GETDATE()),
        UpdatedBy    INT      NULL,
        UpdatedDate  DATETIME NULL,
        CONSTRAINT FK_DownloadUserPermission_User FOREIGN KEY (UserID) REFERENCES EMS.UserAccount (UserID),
        CONSTRAINT UQ_DownloadUserPermission_User UNIQUE (UserID)
    );
    PRINT 'Created workflow.DownloadUserPermission';
END
GO
