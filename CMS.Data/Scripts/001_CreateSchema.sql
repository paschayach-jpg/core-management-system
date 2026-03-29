-- ============================================================
-- CMS Database Schema Migration Script 001
-- Creates all tables, constraints, indexes, and defaults
-- ============================================================

-- Step 1: Create UserAccount WITHOUT the FK to OrgUnit (circular dependency)
CREATE TABLE UserAccount (
    UserId           INT IDENTITY PRIMARY KEY,
    AdIdentity       NVARCHAR(200) NOT NULL UNIQUE,
    DisplayName      NVARCHAR(200) NOT NULL,
    RoleId           TINYINT NOT NULL,  -- Officer=1,SrOfficer=2,Manager=3,Admin=4
    OrgUnitId        INT NULL,          -- FK added after OrgUnit is created
    IsActive         BIT NOT NULL DEFAULT 1,
    CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Step 2: Create OrgUnit (self-referencing hierarchy)
CREATE TABLE OrgUnit (
    OrgUnitId    INT IDENTITY PRIMARY KEY,
    ParentId     INT NULL REFERENCES OrgUnit(OrgUnitId),
    Tier         TINYINT NOT NULL CHECK (Tier IN (1,2,3)),
    Name         NVARCHAR(200) NOT NULL,
    IsActive     BIT NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Step 3: Add FK from UserAccount to OrgUnit now that OrgUnit exists
ALTER TABLE UserAccount
    ADD CONSTRAINT FK_UserAccount_OrgUnit
    FOREIGN KEY (OrgUnitId) REFERENCES OrgUnit(OrgUnitId);
GO

-- Step 4: Create Folder (references UserAccount and OrgUnit, both now exist)
CREATE TABLE Folder (
    FolderId             INT IDENTITY PRIMARY KEY,
    ParentFolderId       INT NULL REFERENCES Folder(FolderId),
    OrgUnitId            INT NOT NULL REFERENCES OrgUnit(OrgUnitId),
    Name                 NVARCHAR(500) NOT NULL,
    Depth                TINYINT NOT NULL CHECK (Depth BETWEEN 1 AND 10),
    ConfidentialityLevel TINYINT NOT NULL DEFAULT 4,
    InheritConfidentiality BIT NOT NULL DEFAULT 1,
    IsDeleted            BIT NOT NULL DEFAULT 0,
    IsArchived           BIT NOT NULL DEFAULT 0,
    DeletedAt            DATETIME2 NULL,
    CreatedByUserId      INT NOT NULL REFERENCES UserAccount(UserId),
    CreatedAt            DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    QuotaBytes           BIGINT NULL,
    TotalUsageBytes      BIGINT NOT NULL DEFAULT 0
);
GO

-- Step 5: Create FileDocument
CREATE TABLE FileDocument (
    FileDocumentId       INT IDENTITY PRIMARY KEY,
    FolderId             INT NOT NULL REFERENCES Folder(FolderId),
    OriginalFileName     NVARCHAR(500) NOT NULL,
    StoredPath           NVARCHAR(1000) NOT NULL,
    FileSizeBytes        BIGINT NOT NULL,
    Sha256Checksum       CHAR(64) NOT NULL,
    ConfidentialityLevel TINYINT NOT NULL,
    UploaderUserId       INT NOT NULL REFERENCES UserAccount(UserId),
    UploadedAt           DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted            BIT NOT NULL DEFAULT 0,
    DeletedAt            DATETIME2 NULL,
    ParentVersionId      INT NULL REFERENCES FileDocument(FileDocumentId),
    RetentionExpiresAt   DATETIME2 NULL,
    RetentionFlagged     BIT NOT NULL DEFAULT 0
);
GO

-- Step 6: Create FolderPermission
CREATE TABLE FolderPermission (
    PermissionId     INT IDENTITY PRIMARY KEY,
    FolderId         INT NOT NULL REFERENCES Folder(FolderId),
    PrincipalType    TINYINT NOT NULL,  -- 0=User, 1=Role
    PrincipalId      INT NOT NULL,
    PermissionType   TINYINT NOT NULL,  -- Read=1,Write=2,Delete=3,Modify=4,FullControl=5
    PermissionState  TINYINT NOT NULL,  -- Allow=1, ExplicitDeny=2
    InheritanceBroken BIT NOT NULL DEFAULT 0,
    GrantedByUserId  INT NOT NULL REFERENCES UserAccount(UserId),
    GrantedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Step 7: Create AuditLog (immutable — no UPDATE/DELETE SPs)
CREATE TABLE AuditLog (
    AuditID      BIGINT IDENTITY PRIMARY KEY,
    AuditDate    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UserID       INT NOT NULL,
    UserName     NVARCHAR(200) NOT NULL,
    UserRole     TINYINT NULL,
    Module       NVARCHAR(100) NOT NULL,
    Action       NVARCHAR(100) NOT NULL,
    TableName    NVARCHAR(100) NULL,
    RecordID     INT NULL,
    OldValue     NVARCHAR(MAX) NULL,
    NewValue     NVARCHAR(MAX) NULL,
    IPAddress    NVARCHAR(50) NULL,
    MachineName  NVARCHAR(200) NULL,
    SessionID    NVARCHAR(100) NULL,
    Result       TINYINT NOT NULL,  -- 1=Success, 2=Failure, 3=Denied
    ErrorMessage NVARCHAR(MAX) NULL
);
GO

-- Step 8: Create ApprovalRequest
CREATE TABLE ApprovalRequest (
    ApprovalRequestId INT IDENTITY PRIMARY KEY,
    FolderId          INT NOT NULL REFERENCES Folder(FolderId),
    RequestorUserId   INT NOT NULL REFERENCES UserAccount(UserId),
    Status            TINYINT NOT NULL DEFAULT 0,  -- Pending=0,Approved=1,Rejected=2,Withdrawn=3
    CreatedAt         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TimeoutAt         DATETIME2 NOT NULL,
    PermissionChanges NVARCHAR(MAX) NOT NULL
);
GO

-- Step 9: Create ApprovalStep
CREATE TABLE ApprovalStep (
    ApprovalStepId    INT IDENTITY PRIMARY KEY,
    ApprovalRequestId INT NOT NULL REFERENCES ApprovalRequest(ApprovalRequestId),
    StepOrder         INT NOT NULL,
    ApproverType      TINYINT NOT NULL,  -- 0=User, 1=Role
    ApproverId        INT NOT NULL,
    Status            TINYINT NOT NULL DEFAULT 0,  -- Pending=0,Approved=1,Rejected=2,Escalated=3
    ActedAt           DATETIME2 NULL,
    Comments          NVARCHAR(MAX) NULL
);
GO

-- Step 10: Create Notification
CREATE TABLE Notification (
    NotificationId   INT IDENTITY PRIMARY KEY,
    RecipientUserId  INT NOT NULL REFERENCES UserAccount(UserId),
    EventType        NVARCHAR(100) NOT NULL,
    RelatedEntityId  NVARCHAR(100) NULL,
    MessageText      NVARCHAR(1000) NOT NULL,
    IsRead           BIT NOT NULL DEFAULT 0,
    DeliveredAt      DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ReadAt           DATETIME2 NULL
);
GO

-- ============================================================
-- Indexes
-- ============================================================

CREATE UNIQUE INDEX IX_UserAccount_AdIdentity
    ON UserAccount(AdIdentity);
GO

CREATE INDEX IX_Folder_OrgUnitId
    ON Folder(OrgUnitId);
GO

CREATE INDEX IX_Folder_ParentFolderId
    ON Folder(ParentFolderId);
GO

CREATE INDEX IX_FileDocument_FolderId
    ON FileDocument(FolderId);
GO

CREATE INDEX IX_FileDocument_Sha256Checksum
    ON FileDocument(Sha256Checksum);
GO

CREATE INDEX IX_FolderPermission_FolderId
    ON FolderPermission(FolderId);
GO

CREATE INDEX IX_AuditLog_ActorUserId_EventTimestamp
    ON AuditLog(ActorUserId, EventTimestamp);
GO

CREATE INDEX IX_AuditLog_EventTimestamp
    ON AuditLog(EventTimestamp);
GO

CREATE INDEX IX_Notification_RecipientUserId_IsRead
    ON Notification(RecipientUserId, IsRead);
GO

CREATE INDEX IX_ApprovalRequest_Status
    ON ApprovalRequest(Status);
GO
