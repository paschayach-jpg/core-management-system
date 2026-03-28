-- ============================================================
-- SCS Stored Procedures  |  Project 2511AL01
-- All SPs use schema-qualified table names
-- ============================================================

USE SCS;
GO

-- ============================================================
-- core.OrgUnit
-- ============================================================

CREATE PROCEDURE core.sp_InsertOrgUnit
    @OrgUnitCode    NVARCHAR(20),
    @OrgUnitNameTH  NVARCHAR(200),
    @OrgUnitNameEN  NVARCHAR(200),
    @OrgUnitType    TINYINT,
    @ParentOrgUnitID INT,
    @SortOrder      INT,
    @CreatedBy      INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check for duplicate code
    IF EXISTS (SELECT 1 FROM core.OrgUnit WHERE OrgUnitCode = @OrgUnitCode AND IsDeleted = 0)
    BEGIN
        RAISERROR('รหัสหน่วยงาน %s มีอยู่ในระบบแล้ว', 16, 1, @OrgUnitCode);
        RETURN;
    END
    
    INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, CreatedBy)
    VALUES (@OrgUnitCode, @OrgUnitNameTH, @OrgUnitNameEN, @OrgUnitType, @ParentOrgUnitID, @SortOrder, @CreatedBy);
    SELECT SCOPE_IDENTITY() AS NewOrgUnitID;
END
GO

CREATE PROCEDURE core.sp_UpdateOrgUnit
    @OrgUnitID      INT,
    @OrgUnitNameTH  NVARCHAR(200),
    @OrgUnitNameEN  NVARCHAR(200),
    @SortOrder      INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.OrgUnit
    SET    OrgUnitNameTH = @OrgUnitNameTH,
           OrgUnitNameEN = @OrgUnitNameEN,
           SortOrder     = @SortOrder
    WHERE  OrgUnitID = @OrgUnitID;
END
GO

CREATE PROCEDURE core.sp_GetOrgUnitByID
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrgUnitID, OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType,
           ParentOrgUnitID, SortOrder, IsActive, IsDeleted, DeletedBy, CreatedBy, CreatedDate
    FROM   core.OrgUnit
    WHERE  OrgUnitID = @OrgUnitID AND IsDeleted = 0;
END
GO

CREATE PROCEDURE core.sp_GetOrgUnitHierarchy
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrgUnitID, OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType,
           ParentOrgUnitID, SortOrder, IsActive, IsDeleted, DeletedBy, CreatedBy, CreatedDate
    FROM   core.OrgUnit
    WHERE  IsActive = 1 AND IsDeleted = 0
    ORDER  BY OrgUnitType, ParentOrgUnitID, SortOrder;
END
GO

CREATE PROCEDURE core.sp_SoftDeleteOrgUnit
    @OrgUnitID INT,
    @DeletedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.OrgUnit
    SET    IsDeleted = 1, IsActive = 0, DeletedBy = @DeletedBy
    WHERE  OrgUnitID = @OrgUnitID;
END
GO

CREATE PROCEDURE core.sp_HasActiveDependents
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM storage.Folder    WHERE OrgUnitID = @OrgUnitID AND IsDeleted = 0
        UNION ALL
        SELECT 1 FROM core.UserAccount  WHERE OrgUnitID = @OrgUnitID AND IsActive  = 1
    )
        SELECT 1 AS HasDependents;
    ELSE
        SELECT 0 AS HasDependents;
END
GO

-- ============================================================
-- core.UserAccount
-- ============================================================

CREATE PROCEDURE core.sp_GetUserAccountByADUserName
    @ADUserName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, ADUserName, DisplayName, Email, AppRole, OrgUnitID,
           ConsentGiven, ConsentDate, IsActive, LastLoginDate, CreatedBy, CreatedDate
    FROM   core.UserAccount
    WHERE  ADUserName = @ADUserName;
END
GO

CREATE PROCEDURE core.sp_GetUserAccountByID
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, ADUserName, DisplayName, Email, AppRole, OrgUnitID,
           ConsentGiven, ConsentDate, IsActive, LastLoginDate, CreatedBy, CreatedDate
    FROM   core.UserAccount
    WHERE  UserID = @UserID;
END
GO

CREATE PROCEDURE core.sp_InsertUserAccount
    @ADUserName  NVARCHAR(100),
    @DisplayName NVARCHAR(200),
    @Email       NVARCHAR(254),
    @AppRole     TINYINT,
    @OrgUnitID   INT,
    @CreatedBy   INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO core.UserAccount (ADUserName, DisplayName, Email, AppRole, OrgUnitID, CreatedBy)
    VALUES (@ADUserName, @DisplayName, @Email, @AppRole, @OrgUnitID, @CreatedBy);
    SELECT SCOPE_IDENTITY() AS NewUserID;
END
GO

CREATE PROCEDURE core.sp_UpdateUserAccount
    @UserID      INT,
    @DisplayName NVARCHAR(200),
    @AppRole     TINYINT,
    @OrgUnitID   INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.UserAccount
    SET    DisplayName = @DisplayName,
           AppRole     = @AppRole,
           OrgUnitID   = @OrgUnitID
    WHERE  UserID = @UserID;
END
GO

CREATE PROCEDURE core.sp_UpdateUserConsent
    @UserID      INT,
    @Email       NVARCHAR(254),
    @ConsentGiven BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.UserAccount
    SET    Email        = @Email,
           ConsentGiven = @ConsentGiven,
           ConsentDate  = CASE WHEN @ConsentGiven = 1 THEN GETDATE() ELSE NULL END
    WHERE  UserID = @UserID;
END
GO

CREATE PROCEDURE core.sp_UpdateLastLogin
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.UserAccount SET LastLoginDate = GETDATE() WHERE UserID = @UserID;
END
GO

CREATE PROCEDURE core.sp_GetUserAccountsByRole
    @AppRole TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, ADUserName, DisplayName, Email, AppRole, OrgUnitID,
           ConsentGiven, ConsentDate, IsActive, LastLoginDate, CreatedBy, CreatedDate
    FROM   core.UserAccount
    WHERE  AppRole = @AppRole AND IsActive = 1;
END
GO

CREATE PROCEDURE core.sp_GetAllUserAccounts
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, ADUserName, DisplayName, Email, AppRole, OrgUnitID,
           ConsentGiven, ConsentDate, IsActive, LastLoginDate, CreatedBy, CreatedDate
    FROM   core.UserAccount
    WHERE  IsActive = 1
    ORDER  BY ADUserName;
END
GO

CREATE PROCEDURE core.sp_DeactivateUserAccount
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.UserAccount SET IsActive = 0 WHERE UserID = @UserID;
END
GO

-- ============================================================
-- security.PermissionGroup
-- ============================================================

CREATE PROCEDURE security.sp_InsertPermissionGroup
    @GroupCode   NVARCHAR(50),
    @GroupName   NVARCHAR(200),
    @Description NVARCHAR(500),
    @OrgUnitID   INT,
    @CreatedBy   INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO security.PermissionGroup (GroupCode, GroupName, Description, OrgUnitID, CreatedBy)
    VALUES (@GroupCode, @GroupName, @Description, @OrgUnitID, @CreatedBy);
    SELECT SCOPE_IDENTITY() AS NewGroupID;
END
GO

CREATE PROCEDURE security.sp_GetPermissionGroupByID
    @GroupID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GroupID, GroupCode, GroupName, Description, OrgUnitID, IsActive, CreatedBy, CreatedDate
    FROM   security.PermissionGroup
    WHERE  GroupID = @GroupID;
END
GO

CREATE PROCEDURE security.sp_GetAllPermissionGroups
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GroupID, GroupCode, GroupName, Description, OrgUnitID, IsActive, CreatedBy, CreatedDate
    FROM   security.PermissionGroup
    WHERE  IsActive = 1;
END
GO

CREATE PROCEDURE security.sp_DeactivatePermissionGroup
    @GroupID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE security.PermissionGroup SET IsActive = 0 WHERE GroupID = @GroupID;
END
GO

-- ============================================================
-- security.UserGroupMember
-- ============================================================

CREATE PROCEDURE security.sp_AddUserToGroup
    @GroupID INT,
    @UserID  INT,
    @AddedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM security.UserGroupMember WHERE GroupID = @GroupID AND UserID = @UserID)
        INSERT INTO security.UserGroupMember (GroupID, UserID, AddedBy)
        VALUES (@GroupID, @UserID, @AddedBy);
END
GO

CREATE PROCEDURE security.sp_RemoveUserFromGroup
    @GroupID INT,
    @UserID  INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE security.UserGroupMember
    SET    IsActive = 0
    WHERE  GroupID = @GroupID AND UserID = @UserID;
END
GO

CREATE PROCEDURE security.sp_GetGroupsByUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MemberID, m.GroupID, m.UserID, g.GroupCode, g.GroupName, m.JoinedDate, m.AddedBy, m.IsActive
    FROM   security.UserGroupMember m
    INNER  JOIN security.PermissionGroup g ON g.GroupID = m.GroupID
    WHERE  m.UserID = @UserID AND m.IsActive = 1 AND g.IsActive = 1;
END
GO

CREATE PROCEDURE security.sp_GetUsersByGroup
    @GroupID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MemberID, m.GroupID, m.UserID, u.ADUserName, u.DisplayName, u.AppRole, m.JoinedDate, m.AddedBy
    FROM   security.UserGroupMember m
    INNER  JOIN core.UserAccount u ON u.UserID = m.UserID
    WHERE  m.GroupID = @GroupID AND m.IsActive = 1 AND u.IsActive = 1;
END
GO

-- ============================================================
-- security.FolderPermission
-- ============================================================

CREATE PROCEDURE security.sp_InsertFolderPermission
    @FolderID       INT,
    @PrincipalType  TINYINT,
    @PrincipalID    INT,
    @CanRead        BIT,
    @CanWrite       BIT,
    @CanDelete      BIT,
    @CanModify      BIT,
    @CanFullControl BIT,
    @IsDeny         BIT,
    @ExpiryDate     DATETIME,
    @GrantedBy      INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO security.FolderPermission
        (FolderID, PrincipalType, PrincipalID, CanRead, CanWrite, CanDelete,
         CanModify, CanFullControl, IsDeny, ExpiryDate, GrantedBy)
    VALUES
        (@FolderID, @PrincipalType, @PrincipalID, @CanRead, @CanWrite, @CanDelete,
         @CanModify, @CanFullControl, @IsDeny, @ExpiryDate, @GrantedBy);
    SELECT SCOPE_IDENTITY() AS NewPermissionID;
END
GO

CREATE PROCEDURE security.sp_GetPermissionsByFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PermissionID, FolderID, PrincipalType, PrincipalID,
           CanRead, CanWrite, CanDelete, CanModify, CanFullControl, IsDeny,
           ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
    FROM   security.FolderPermission
    WHERE  FolderID = @FolderID
      AND  RevokedDate IS NULL
      AND  (ExpiryDate IS NULL OR ExpiryDate > GETDATE());
END
GO

CREATE PROCEDURE security.sp_GetPermissionsByPrincipal
    @PrincipalType TINYINT,
    @PrincipalID   INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PermissionID, FolderID, PrincipalType, PrincipalID,
           CanRead, CanWrite, CanDelete, CanModify, CanFullControl, IsDeny,
           ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
    FROM   security.FolderPermission
    WHERE  PrincipalType = @PrincipalType AND PrincipalID = @PrincipalID
      AND  RevokedDate IS NULL
      AND  (ExpiryDate IS NULL OR ExpiryDate > GETDATE());
END
GO

CREATE PROCEDURE security.sp_RevokePermission
    @PermissionID INT,
    @RevokedBy    INT,
    @RevokeReason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE security.FolderPermission
    SET    RevokedBy   = @RevokedBy,
           RevokedDate = GETDATE(),
           RevokeReason = @RevokeReason
    WHERE  PermissionID = @PermissionID;
END
GO

-- ============================================================
-- storage.FolderTemplate / FolderTemplateItem
-- ============================================================

CREATE PROCEDURE storage.sp_InsertFolderTemplate
    @TemplateCode NVARCHAR(50),
    @TemplateName NVARCHAR(200),
    @TargetType   VARCHAR(20),
    @Description  NVARCHAR(500),
    @CreatedBy    INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO storage.FolderTemplate (TemplateCode, TemplateName, TargetType, Description, CreatedBy)
    VALUES (@TemplateCode, @TemplateName, @TargetType, @Description, @CreatedBy);
    SELECT SCOPE_IDENTITY() AS NewTemplateID;
END
GO

CREATE PROCEDURE storage.sp_GetAllFolderTemplates
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TemplateID, TemplateCode, TemplateName, TargetType, Description, IsActive, CreatedBy, CreatedDate
    FROM   storage.FolderTemplate
    WHERE  IsActive = 1;
END
GO

CREATE PROCEDURE storage.sp_GetFolderTemplateItems
    @TemplateID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ItemID, TemplateID, ParentItemID, FolderName, ConfidentialityLevel, SortOrder, InheritPermission
    FROM   storage.FolderTemplateItem
    WHERE  TemplateID = @TemplateID
    ORDER  BY SortOrder;
END
GO

CREATE PROCEDURE storage.sp_InsertFolderTemplateItem
    @TemplateID          INT,
    @ParentItemID        INT,
    @FolderName          NVARCHAR(255),
    @ConfidentialityLevel TINYINT,
    @SortOrder           INT,
    @InheritPermission   BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO storage.FolderTemplateItem
        (TemplateID, ParentItemID, FolderName, ConfidentialityLevel, SortOrder, InheritPermission)
    VALUES
        (@TemplateID, @ParentItemID, @FolderName, @ConfidentialityLevel, @SortOrder, @InheritPermission);
    SELECT SCOPE_IDENTITY() AS NewItemID;
END
GO

-- ============================================================
-- storage.Folder
-- ============================================================

CREATE PROCEDURE storage.sp_InsertFolder
    @FolderName          NVARCHAR(255),
    @ParentFolderID      INT,
    @OrgUnitID           INT,
    @TemplateID          INT,
    @ConfidentialityLevel TINYINT,
    @FolderDepth         TINYINT,
    @PhysicalPath        NVARCHAR(500),
    @InheritPermission   BIT,
    @CreatedBy           INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO storage.Folder
        (FolderName, ParentFolderID, OrgUnitID, TemplateID, ConfidentialityLevel,
         FolderDepth, PhysicalPath, InheritPermission, CreatedBy)
    VALUES
        (@FolderName, @ParentFolderID, @OrgUnitID, @TemplateID, @ConfidentialityLevel,
         @FolderDepth, @PhysicalPath, @InheritPermission, @CreatedBy);
    SELECT SCOPE_IDENTITY() AS NewFolderID;
END
GO

CREATE PROCEDURE storage.sp_UpdateFolder
    @FolderID            INT,
    @FolderName          NVARCHAR(255),
    @ConfidentialityLevel TINYINT,
    @InheritPermission   BIT,
    @UpdatedBy           INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.Folder
    SET    FolderName          = @FolderName,
           ConfidentialityLevel = @ConfidentialityLevel,
           InheritPermission   = @InheritPermission,
           UpdatedBy           = @UpdatedBy,
           UpdatedDate         = GETDATE()
    WHERE  FolderID = @FolderID;
END
GO

CREATE PROCEDURE storage.sp_GetFolderByID
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
           [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
           [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
           [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
    FROM   [storage].[Folder]
    WHERE  [FolderID] = @FolderID;
END
GO

CREATE PROCEDURE storage.sp_GetFolderChildren
    @ParentFolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
           [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
           [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
           [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
    FROM   [storage].[Folder]
    WHERE  [ParentFolderID] = @ParentFolderID AND [IsDeleted] = 0;
END
GO

CREATE PROCEDURE storage.sp_GetRootFoldersByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
           [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
           [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
           [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
    FROM   [storage].[Folder]
    WHERE  [OrgUnitID] = @OrgUnitID AND [ParentFolderID] IS NULL AND [IsDeleted] = 0;
END
GO

CREATE PROCEDURE storage.sp_GetFolderDescendants
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH FolderCTE AS (
        SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
               [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
               [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
               [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
        FROM   [storage].[Folder] WHERE [ParentFolderID] = @FolderID
        UNION ALL
        SELECT f.[FolderID], f.[FolderGUID], f.[FolderName], f.[ParentFolderID], f.[OrgUnitID], f.[TemplateID],
               f.[ConfidentialityLevel], f.[FolderDepth], f.[PhysicalPath], f.[FolderSizeBytes], f.[FileCount],
               f.[IsArchived], f.[ArchivedBy], f.[ArchivedDate], f.[IsDeleted], f.[DeletedBy], f.[DeletedDate],
               f.[RecycleBinExpiry], f.[InheritPermission], f.[CreatedBy], f.[CreatedDate], f.[UpdatedBy], f.[UpdatedDate]
        FROM   [storage].[Folder] f
        INNER  JOIN FolderCTE cte ON f.[ParentFolderID] = cte.[FolderID]
    )
    SELECT * FROM FolderCTE;
END
GO

CREATE PROCEDURE storage.sp_SoftDeleteFolder
    @FolderID  INT,
    @DeletedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH FolderCTE AS (
        SELECT FolderID FROM storage.Folder WHERE FolderID = @FolderID
        UNION ALL
        SELECT f.FolderID FROM storage.Folder f
        INNER  JOIN FolderCTE cte ON f.ParentFolderID = cte.FolderID
    )
    UPDATE storage.Folder
    SET    IsDeleted = 1, DeletedBy = @DeletedBy,
           DeletedDate = GETDATE(), RecycleBinExpiry = DATEADD(DAY, 30, GETDATE())
    WHERE  FolderID IN (SELECT FolderID FROM FolderCTE);
END
GO

CREATE PROCEDURE storage.sp_RestoreFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH FolderCTE AS (
        SELECT FolderID FROM storage.Folder WHERE FolderID = @FolderID
        UNION ALL
        SELECT f.FolderID FROM storage.Folder f
        INNER  JOIN FolderCTE cte ON f.ParentFolderID = cte.FolderID
    )
    UPDATE storage.Folder
    SET    IsDeleted = 0, DeletedBy = NULL, DeletedDate = NULL, RecycleBinExpiry = NULL
    WHERE  FolderID IN (SELECT FolderID FROM FolderCTE);
END
GO

CREATE PROCEDURE storage.sp_GetDeletedFoldersByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
           [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
           [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
           [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
    FROM   [storage].[Folder]
    WHERE  [OrgUnitID] = @OrgUnitID AND [IsDeleted] = 1;
END
GO

-- ============================================================
-- storage.FileDocument
-- ============================================================

CREATE PROCEDURE storage.sp_InsertFileDocument
    @FolderID        INT,
    @OriginalFileName NVARCHAR(260),
    @StoredFileName  NVARCHAR(360),
    @FileExtension   VARCHAR(20),
    @MimeType        VARCHAR(100),
    @FileSizeBytes   BIGINT,
    @PhysicalPath    NVARCHAR(500),
    @Checksum        VARCHAR(64),
    @UploadedBy      INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO storage.FileDocument
        (FolderID, OriginalFileName, StoredFileName, FileExtension, MimeType,
         FileSizeBytes, PhysicalPath, Checksum, UploadedBy)
    VALUES
        (@FolderID, @OriginalFileName, @StoredFileName, @FileExtension, @MimeType,
         @FileSizeBytes, @PhysicalPath, @Checksum, @UploadedBy);
    SELECT SCOPE_IDENTITY() AS NewFileID;
END
GO

CREATE PROCEDURE storage.sp_GetFileDocumentByID
    @FileID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileID, FileGUID, FolderID, OriginalFileName, StoredFileName, FileExtension,
           MimeType, FileSizeBytes, PhysicalPath, Checksum, VirusScanStatus, VirusScanDate,
           FileStatus, UploadedBy, UploadedDate, ArchivedBy, ArchivedDate,
           DeletedBy, DeletedDate, Version
    FROM   storage.FileDocument
    WHERE  FileID = @FileID;
END
GO

CREATE PROCEDURE storage.sp_GetFileDocumentsByFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileID, FileGUID, FolderID, OriginalFileName, StoredFileName, FileExtension,
           MimeType, FileSizeBytes, PhysicalPath, Checksum, VirusScanStatus, VirusScanDate,
           FileStatus, UploadedBy, UploadedDate, ArchivedBy, ArchivedDate,
           DeletedBy, DeletedDate, Version
    FROM   storage.FileDocument
    WHERE  FolderID = @FolderID AND FileStatus != 3;  -- แสดงทุกสถานะยกเว้นที่ถูกลบ
END
GO

CREATE PROCEDURE storage.sp_UpdateVirusScanStatus
    @FileID          INT,
    @VirusScanStatus TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.FileDocument
    SET    VirusScanStatus = @VirusScanStatus, VirusScanDate = GETDATE()
    WHERE  FileID = @FileID;
END
GO

CREATE PROCEDURE storage.sp_SoftDeleteFileDocument
    @FileID    INT,
    @DeletedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.FileDocument
    SET    FileStatus = 3, DeletedBy = @DeletedBy, DeletedDate = GETDATE()
    WHERE  FileID = @FileID;
END
GO

CREATE PROCEDURE storage.sp_GetFileDocumentByChecksum
    @Checksum VARCHAR(64),
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileID, FileGUID, FolderID, OriginalFileName, StoredFileName, FileExtension,
           MimeType, FileSizeBytes, PhysicalPath, Checksum, VirusScanStatus, VirusScanDate,
           FileStatus, UploadedBy, UploadedDate, ArchivedBy, ArchivedDate,
           DeletedBy, DeletedDate, Version
    FROM   storage.FileDocument
    WHERE  Checksum = @Checksum AND FolderID = @FolderID AND FileStatus = 1;
END
GO

-- ============================================================
-- workflow.ApprovalRequest / ApprovalStep
-- ============================================================

CREATE PROCEDURE workflow.sp_InsertApprovalRequest
    @RequestType  VARCHAR(50),
    @FolderID     INT,
    @FileID       INT,
    @PermissionID INT,
    @RequestedBy  INT,
    @Subject      NVARCHAR(500),
    @Remark       NVARCHAR(MAX),
    @ExpiryDate   DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO workflow.ApprovalRequest
        (RequestType, FolderID, FileID, PermissionID, RequestedBy, Subject, Remark, ExpiryDate)
    VALUES
        (@RequestType, @FolderID, @FileID, @PermissionID, @RequestedBy, @Subject, @Remark, @ExpiryDate);
    SELECT SCOPE_IDENTITY() AS NewRequestID;
END
GO

CREATE PROCEDURE workflow.sp_InsertApprovalStep
    @RequestID  INT,
    @StepOrder  INT,
    @ApproverID INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO workflow.ApprovalStep (RequestID, StepOrder, ApproverID)
    VALUES (@RequestID, @StepOrder, @ApproverID);
    SELECT SCOPE_IDENTITY() AS NewStepID;
END
GO

CREATE PROCEDURE workflow.sp_GetApprovalRequestByID
    @RequestID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RequestID, RequestType, FolderID, FileID, PermissionID, RequestedBy,
           RequestedDate, RequestStatus, Subject, Remark, ExpiryDate,
           ResolvedBy, ResolvedDate, ResolveRemark
    FROM   workflow.ApprovalRequest
    WHERE  RequestID = @RequestID;
END
GO

CREATE PROCEDURE workflow.sp_GetPendingApprovalSteps
    @RequestID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StepID, RequestID, StepOrder, ApproverID, StepStatus,
           ActionDate, ActionRemark, DelegatedTo, NotifiedDate, EscalatedDate
    FROM   workflow.ApprovalStep
    WHERE  RequestID = @RequestID AND StepStatus = 1
    ORDER  BY StepOrder;
END
GO

CREATE PROCEDURE workflow.sp_GetAllApprovalSteps
    @RequestID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StepID, RequestID, StepOrder, ApproverID, StepStatus,
           ActionDate, ActionRemark, DelegatedTo, NotifiedDate, EscalatedDate
    FROM   workflow.ApprovalStep
    WHERE  RequestID = @RequestID
    ORDER  BY StepOrder;
END
GO

CREATE PROCEDURE workflow.sp_UpdateApprovalStepStatus
    @StepID       INT,
    @StepStatus   TINYINT,
    @ActionRemark NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE workflow.ApprovalStep
    SET    StepStatus = @StepStatus, ActionDate = GETDATE(), ActionRemark = @ActionRemark
    WHERE  StepID = @StepID;
END
GO

CREATE PROCEDURE workflow.sp_UpdateApprovalRequestStatus
    @RequestID     INT,
    @RequestStatus TINYINT,
    @ResolvedBy    INT,
    @ResolveRemark NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE workflow.ApprovalRequest
    SET    RequestStatus = @RequestStatus,
           ResolvedBy    = @ResolvedBy,
           ResolvedDate  = GETDATE(),
           ResolveRemark = @ResolveRemark
    WHERE  RequestID = @RequestID;
END
GO

CREATE PROCEDURE workflow.sp_GetExpiredPendingRequests
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RequestID, RequestType, FolderID, FileID, PermissionID, RequestedBy,
           RequestedDate, RequestStatus, Subject, Remark, ExpiryDate,
           ResolvedBy, ResolvedDate, ResolveRemark
    FROM   workflow.ApprovalRequest
    WHERE  RequestStatus = 1 AND ExpiryDate < GETDATE();
END
GO

-- ============================================================
-- audit.AuditLog (INSERT ONLY — immutable)
-- ============================================================

CREATE PROCEDURE audit.sp_InsertAuditLog
    @UserID    INT,
    @UserName  NVARCHAR(100),
    @UserRole  TINYINT,
    @Module    VARCHAR(50),
    @Action    VARCHAR(50),
    @TableName VARCHAR(100),
    @RecordID  INT,
    @OldValue  NVARCHAR(MAX),
    @NewValue  NVARCHAR(MAX),
    @IPAddress VARCHAR(45),
    @MachineName NVARCHAR(100),
    @SessionID NVARCHAR(100),
    @Result    TINYINT,
    @ErrorMessage NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO audit.AuditLog
        (UserID, UserName, UserRole, Module, Action, TableName, RecordID,
         OldValue, NewValue, IPAddress, MachineName, SessionID, Result, ErrorMessage)
    VALUES
        (@UserID, @UserName, @UserRole, @Module, @Action, @TableName, @RecordID,
         @OldValue, @NewValue, @IPAddress, @MachineName, @SessionID, @Result, @ErrorMessage);
    SELECT SCOPE_IDENTITY() AS NewAuditID;
END
GO

CREATE PROCEDURE audit.sp_QueryAuditLog
    @FromDate  DATETIME,
    @ToDate    DATETIME,
    @UserID    INT,
    @Module    VARCHAR(50),
    @Action    VARCHAR(50),
    @Result    TINYINT,
    @PageNumber INT,
    @PageSize   INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    SELECT COUNT(*) OVER () AS TotalCount,
           AuditID, AuditDate, UserID, UserName, UserRole, Module, Action,
           TableName, RecordID, OldValue, NewValue, IPAddress, MachineName,
           SessionID, Result, ErrorMessage
    FROM   audit.AuditLog
    WHERE  AuditDate >= @FromDate AND AuditDate <= @ToDate
      AND  (@UserID  IS NULL OR UserID  = @UserID)
      AND  (@Module  IS NULL OR Module  = @Module)
      AND  (@Action  IS NULL OR Action  = @Action)
      AND  (@Result  IS NULL OR Result  = @Result)
    ORDER  BY AuditDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================
-- storage.Folder — Additional SPs (GetAllFolders, GetFoldersByOrgUnit)
-- ============================================================

CREATE PROCEDURE storage.sp_GetAllFolders
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
           [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
           [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
           [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
    FROM   [storage].[Folder]
    WHERE  [IsDeleted] = 0
    ORDER  BY [OrgUnitID], [FolderDepth], [FolderName];
END
GO

CREATE PROCEDURE storage.sp_GetFoldersByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
           [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
           [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
           [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
    FROM   [storage].[Folder]
    WHERE  [OrgUnitID] = @OrgUnitID AND [IsDeleted] = 0
    ORDER  BY [FolderDepth], [FolderName];
END
GO

-- ============================================================
-- core.OrgUnit — GetByConfidentialityLevel (missing SP)
-- ============================================================

CREATE PROCEDURE core.sp_GetOrgUnitByConfidentialityLevel
    @ConfidentialityLevel TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT ou.OrgUnitID, ou.OrgUnitCode, ou.OrgUnitNameTH, ou.OrgUnitNameEN,
                    ou.OrgUnitType, ou.ParentOrgUnitID, ou.SortOrder,
                    ou.IsActive, ou.IsDeleted, ou.DeletedBy, ou.CreatedBy, ou.CreatedDate
    FROM   core.OrgUnit ou
    INNER  JOIN storage.Folder f ON f.OrgUnitID = ou.OrgUnitID
    WHERE  ou.IsActive = 1 AND ou.IsDeleted = 0
      AND  f.ConfidentialityLevel = @ConfidentialityLevel
      AND  f.IsDeleted = 0;
END
GO

-- ============================================================
-- core.Notification table (if not exists) + SPs
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables t
               INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
               WHERE s.name = 'core' AND t.name = 'Notification')
BEGIN
    CREATE TABLE core.Notification (
        NotificationId  INT IDENTITY(1,1) NOT NULL,
        RecipientUserId INT               NOT NULL,
        EventType       NVARCHAR(100)     NOT NULL,
        RelatedEntityId NVARCHAR(100)         NULL,
        MessageText     NVARCHAR(1000)    NOT NULL,
        IsRead          BIT               NOT NULL CONSTRAINT DF_core_Notif_IsRead    DEFAULT 0,
        DeliveredAt     DATETIME          NOT NULL CONSTRAINT DF_core_Notif_Delivered DEFAULT GETDATE(),
        ReadAt          DATETIME              NULL,
        CONSTRAINT PK_core_Notification         PRIMARY KEY (NotificationId),
        CONSTRAINT FK_core_Notification_User    FOREIGN KEY (RecipientUserId) REFERENCES core.UserAccount(UserID)
    );
    CREATE INDEX IX_core_Notif_User ON core.Notification(RecipientUserId, IsRead);
END
GO

CREATE PROCEDURE core.sp_InsertNotification
    @RecipientUserId INT,
    @EventType       NVARCHAR(100),
    @RelatedEntityId NVARCHAR(100),
    @MessageText     NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO core.Notification (RecipientUserId, EventType, RelatedEntityId, MessageText)
    VALUES (@RecipientUserId, @EventType, @RelatedEntityId, @MessageText);
    SELECT SCOPE_IDENTITY() AS NewNotificationId;
END
GO

CREATE PROCEDURE core.sp_GetUnreadNotificationsByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NotificationId, RecipientUserId, EventType, RelatedEntityId,
           MessageText, IsRead, DeliveredAt, ReadAt
    FROM   core.Notification
    WHERE  RecipientUserId = @UserId AND IsRead = 0
    ORDER  BY DeliveredAt DESC;
END
GO

CREATE PROCEDURE core.sp_MarkNotificationRead
    @NotificationId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.Notification
    SET    IsRead = 1, ReadAt = GETDATE()
    WHERE  NotificationId = @NotificationId;
END
GO

-- ============================================================
-- storage.Folder — PermanentDelete (cascade delete)
-- ============================================================

CREATE PROCEDURE storage.sp_PermanentDeleteFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Step 1: Collect all descendant folder IDs (including root)
        WITH FolderCTE AS (
            SELECT FolderID FROM storage.Folder WHERE FolderID = @FolderID
            UNION ALL
            SELECT f.FolderID FROM storage.Folder f
            INNER  JOIN FolderCTE cte ON f.ParentFolderID = cte.FolderID
        )
        -- Step 2: Delete all FileDocument records in those folders (must delete, not soft-delete, to avoid FK constraint)
        DELETE FROM storage.FileDocument
        WHERE  FolderID IN (SELECT FolderID FROM FolderCTE);

        -- Step 3: Delete FolderPermission entries
        WITH FolderCTE AS (
            SELECT FolderID FROM storage.Folder WHERE FolderID = @FolderID
            UNION ALL
            SELECT f.FolderID FROM storage.Folder f
            INNER  JOIN FolderCTE cte ON f.ParentFolderID = cte.FolderID
        )
        DELETE FROM security.FolderPermission
        WHERE  FolderID IN (SELECT FolderID FROM FolderCTE);

        -- Step 4: Delete Folder records
        WITH FolderCTE AS (
            SELECT FolderID FROM storage.Folder WHERE FolderID = @FolderID
            UNION ALL
            SELECT f.FolderID FROM storage.Folder f
            INNER  JOIN FolderCTE cte ON f.ParentFolderID = cte.FolderID
        )
        DELETE FROM storage.Folder
        WHERE  FolderID IN (SELECT FolderID FROM FolderCTE);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================
-- storage.FileDocument — UpdateFileDocument (missing SP)
-- ============================================================

CREATE PROCEDURE storage.sp_UpdateFileDocument
    @FileID      INT,
    @FileStatus  TINYINT,
    @ArchivedBy  INT,
    @ArchivedDate DATETIME,
    @DeletedBy   INT,
    @DeletedDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.FileDocument
    SET    FileStatus   = @FileStatus,
           ArchivedBy   = @ArchivedBy,
           ArchivedDate = @ArchivedDate,
           DeletedBy    = @DeletedBy,
           DeletedDate  = @DeletedDate
    WHERE  FileID = @FileID;
END
GO
