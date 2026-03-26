-- ============================================================
-- CMS DATABASE  |  SCHEMA DESIGN  |  Project 2511AL01
-- SQL Server 2019  |  Compatible: 2012+
-- Schemas: 5  |  Tables: 12  |  Indexes: 24
-- ============================================================
-- SCHEMA MAP:
--   core      → โครงสร้างองค์กร + ผู้ใช้
--   storage   → Folder + File
--   security  → Permission + Group
--   workflow  → Approval Request/Step
--   audit     → AuditLog (OIC Compliance)
-- ============================================================

USE SCS;
GO

-- ============================================================
-- Create Schemas (uncomment if not yet created)
-- ============================================================
--CREATE SCHEMA core;     GO
--CREATE SCHEMA storage;  GO
--CREATE SCHEMA security; GO
--CREATE SCHEMA workflow;  GO
--CREATE SCHEMA audit;    GO

-- ============================================================
-- SCHEMA: core  |  Tables: OrgUnit, UserAccount
-- ============================================================

CREATE TABLE core.OrgUnit (
    OrgUnitID       INT IDENTITY(1,1)   NOT NULL,
    OrgUnitCode     NVARCHAR(20)        NOT NULL,
    OrgUnitNameTH   NVARCHAR(200)       NOT NULL,
    OrgUnitNameEN   NVARCHAR(200)           NULL,
    OrgUnitType     TINYINT             NOT NULL,   -- 1=SubCommittee, 2=Division, 3=Department
    ParentOrgUnitID INT                     NULL,
    SortOrder       INT                 NOT NULL    CONSTRAINT DF_core_OrgUnit_SortOrder   DEFAULT 0,
    IsActive        BIT                 NOT NULL    CONSTRAINT DF_core_OrgUnit_IsActive    DEFAULT 1,
    IsDeleted       BIT                 NOT NULL    CONSTRAINT DF_core_OrgUnit_IsDeleted   DEFAULT 0,
    DeletedBy       INT                     NULL,
    CreatedBy       INT                 NOT NULL,
    CreatedDate     DATETIME            NOT NULL    CONSTRAINT DF_core_OrgUnit_CreatedDate DEFAULT GETDATE(),
    CONSTRAINT PK_core_OrgUnit          PRIMARY KEY (OrgUnitID),
    CONSTRAINT UQ_core_OrgUnit_Code     UNIQUE      (OrgUnitCode),
    CONSTRAINT CK_core_OrgUnit_Type     CHECK       (OrgUnitType IN (1, 2, 3)),
    CONSTRAINT FK_core_OrgUnit_Parent   FOREIGN KEY (ParentOrgUnitID) REFERENCES core.OrgUnit(OrgUnitID)
);
GO

CREATE TABLE core.UserAccount (
    UserID          INT IDENTITY(1,1)   NOT NULL,
    ADUserName      NVARCHAR(100)       NOT NULL,
    DisplayName     NVARCHAR(200)       NOT NULL,
    Email           NVARCHAR(254)           NULL,   -- PDPA: ต้องขอ Consent แยก
    AppRole         TINYINT             NOT NULL,   -- 1=Officer, 2=SrOfficer, 3=Manager, 4=Admin
    OrgUnitID       INT                 NOT NULL,
    ConsentGiven    BIT                 NOT NULL    CONSTRAINT DF_core_User_ConsentGiven DEFAULT 0,
    ConsentDate     DATETIME                NULL,
    IsActive        BIT                 NOT NULL    CONSTRAINT DF_core_User_IsActive     DEFAULT 1,
    LastLoginDate   DATETIME                NULL,
    CreatedBy       INT                     NULL,
    CreatedDate     DATETIME            NOT NULL    CONSTRAINT DF_core_User_CreatedDate  DEFAULT GETDATE(),
    CONSTRAINT PK_core_UserAccount          PRIMARY KEY (UserID),
    CONSTRAINT UQ_core_UserAccount_AD       UNIQUE      (ADUserName),
    CONSTRAINT CK_core_UserAccount_Role     CHECK       (AppRole IN (1, 2, 3, 4)),
    CONSTRAINT FK_core_UserAccount_OrgUnit  FOREIGN KEY (OrgUnitID) REFERENCES core.OrgUnit(OrgUnitID)
);
GO

-- ============================================================
-- SCHEMA: security  |  Tables: PermissionGroup, UserGroupMember, FolderPermission
-- ============================================================

CREATE TABLE security.PermissionGroup (
    GroupID         INT IDENTITY(1,1)   NOT NULL,
    GroupCode       NVARCHAR(50)        NOT NULL,
    GroupName       NVARCHAR(200)       NOT NULL,
    Description     NVARCHAR(500)           NULL,
    OrgUnitID       INT                     NULL,   -- NULL = Cross-OrgUnit group
    IsActive        BIT                 NOT NULL    CONSTRAINT DF_sec_PGroup_IsActive    DEFAULT 1,
    CreatedBy       INT                 NOT NULL,
    CreatedDate     DATETIME            NOT NULL    CONSTRAINT DF_sec_PGroup_CreatedDate DEFAULT GETDATE(),
    CONSTRAINT PK_sec_PermissionGroup       PRIMARY KEY (GroupID),
    CONSTRAINT UQ_sec_PermissionGroup_Code  UNIQUE      (GroupCode),
    CONSTRAINT FK_sec_PGroup_OrgUnit        FOREIGN KEY (OrgUnitID)  REFERENCES core.OrgUnit(OrgUnitID),
    CONSTRAINT FK_sec_PGroup_CreatedBy      FOREIGN KEY (CreatedBy)  REFERENCES core.UserAccount(UserID)
);
GO

CREATE TABLE security.UserGroupMember (
    MemberID        INT IDENTITY(1,1)   NOT NULL,
    GroupID         INT                 NOT NULL,
    UserID          INT                 NOT NULL,
    JoinedDate      DATETIME            NOT NULL    CONSTRAINT DF_sec_UGM_JoinedDate DEFAULT GETDATE(),
    AddedBy         INT                 NOT NULL,
    IsActive        BIT                 NOT NULL    CONSTRAINT DF_sec_UGM_IsActive   DEFAULT 1,
    CONSTRAINT PK_sec_UserGroupMember           PRIMARY KEY (MemberID),
    CONSTRAINT UQ_sec_UserGroupMember_Pair      UNIQUE      (GroupID, UserID),
    CONSTRAINT FK_sec_UGM_Group                 FOREIGN KEY (GroupID) REFERENCES security.PermissionGroup(GroupID),
    CONSTRAINT FK_sec_UGM_User                  FOREIGN KEY (UserID)  REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_sec_UGM_AddedBy               FOREIGN KEY (AddedBy) REFERENCES core.UserAccount(UserID)
);
GO

CREATE TABLE security.FolderPermission (
    PermissionID    INT IDENTITY(1,1)   NOT NULL,
    FolderID        INT                 NOT NULL,   -- FK → storage.Folder (added via ALTER below)
    PrincipalType   TINYINT             NOT NULL,   -- 1=User, 2=Role, 3=OrgUnit, 4=Group
    PrincipalID     INT                 NOT NULL,
    CanRead         BIT                 NOT NULL    CONSTRAINT DF_sec_Perm_CanRead        DEFAULT 0,
    CanWrite        BIT                 NOT NULL    CONSTRAINT DF_sec_Perm_CanWrite       DEFAULT 0,
    CanDelete       BIT                 NOT NULL    CONSTRAINT DF_sec_Perm_CanDelete      DEFAULT 0,
    CanModify       BIT                 NOT NULL    CONSTRAINT DF_sec_Perm_CanModify      DEFAULT 0,
    CanFullControl  BIT                 NOT NULL    CONSTRAINT DF_sec_Perm_CanFullControl DEFAULT 0,
    IsDeny          BIT                 NOT NULL    CONSTRAINT DF_sec_Perm_IsDeny         DEFAULT 0,
    ExpiryDate      DATETIME                NULL,
    GrantedBy       INT                 NOT NULL,
    GrantedDate     DATETIME            NOT NULL    CONSTRAINT DF_sec_Perm_GrantedDate    DEFAULT GETDATE(),
    RevokedBy       INT                     NULL,
    RevokedDate     DATETIME                NULL,
    RevokeReason    NVARCHAR(500)           NULL,
    CONSTRAINT PK_sec_FolderPermission              PRIMARY KEY (PermissionID),
    CONSTRAINT CK_sec_FolderPerm_PrincipalType      CHECK       (PrincipalType IN (1, 2, 3, 4)),
    CONSTRAINT FK_sec_FolderPerm_GrantedBy          FOREIGN KEY (GrantedBy) REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_sec_FolderPerm_RevokedBy          FOREIGN KEY (RevokedBy) REFERENCES core.UserAccount(UserID)
    -- FK_sec_FolderPerm_Folder → เพิ่มหลัง storage.Folder ถูกสร้าง (ALTER ด้านล่าง)
);
GO

-- ============================================================
-- SCHEMA: storage  |  Tables: FolderTemplate, FolderTemplateItem, Folder, FileDocument
-- ============================================================

CREATE TABLE storage.FolderTemplate (
    TemplateID      INT IDENTITY(1,1)   NOT NULL,
    TemplateCode    NVARCHAR(50)        NOT NULL,
    TemplateName    NVARCHAR(200)       NOT NULL,
    TargetType      VARCHAR(20)         NOT NULL,   -- SubCommittee | Division | Department | Any
    Description     NVARCHAR(500)           NULL,
    IsActive        BIT                 NOT NULL    CONSTRAINT DF_sto_FTpl_IsActive    DEFAULT 1,
    CreatedBy       INT                 NOT NULL,
    CreatedDate     DATETIME            NOT NULL    CONSTRAINT DF_sto_FTpl_CreatedDate DEFAULT GETDATE(),
    CONSTRAINT PK_sto_FolderTemplate        PRIMARY KEY (TemplateID),
    CONSTRAINT UQ_sto_FolderTemplate_Code   UNIQUE      (TemplateCode),
    CONSTRAINT CK_sto_FolderTemplate_Type   CHECK       (TargetType IN ('SubCommittee','Division','Department','Any')),
    CONSTRAINT FK_sto_FolderTemplate_Owner  FOREIGN KEY (CreatedBy) REFERENCES core.UserAccount(UserID)
);
GO

CREATE TABLE storage.FolderTemplateItem (
    ItemID               INT IDENTITY(1,1)   NOT NULL,
    TemplateID           INT                 NOT NULL,
    ParentItemID         INT                     NULL,
    FolderName           NVARCHAR(255)       NOT NULL,
    ConfidentialityLevel TINYINT             NOT NULL    CONSTRAINT DF_sto_FTplItem_ConfLevel DEFAULT 4,
    SortOrder            INT                 NOT NULL    CONSTRAINT DF_sto_FTplItem_Sort      DEFAULT 0,
    InheritPermission    BIT                 NOT NULL    CONSTRAINT DF_sto_FTplItem_Inherit   DEFAULT 1,
    CONSTRAINT PK_sto_FolderTemplateItem        PRIMARY KEY (ItemID),
    CONSTRAINT CK_sto_FTplItem_ConfLevel        CHECK       (ConfidentialityLevel IN (1, 2, 3, 4)),
    CONSTRAINT FK_sto_FTplItem_Template         FOREIGN KEY (TemplateID)   REFERENCES storage.FolderTemplate(TemplateID),
    CONSTRAINT FK_sto_FTplItem_Parent           FOREIGN KEY (ParentItemID) REFERENCES storage.FolderTemplateItem(ItemID)
);
GO

CREATE TABLE storage.Folder (
    FolderID             INT IDENTITY(1,1)   NOT NULL,
    FolderGUID           UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT DF_sto_Folder_GUID        DEFAULT NEWID(),
    FolderName           NVARCHAR(255)       NOT NULL,
    ParentFolderID       INT                     NULL,
    OrgUnitID            INT                 NOT NULL,
    TemplateID           INT                     NULL,
    ConfidentialityLevel TINYINT             NOT NULL    CONSTRAINT DF_sto_Folder_ConfLevel   DEFAULT 4,
    FolderDepth          TINYINT             NOT NULL    CONSTRAINT DF_sto_Folder_Depth       DEFAULT 0,
    PhysicalPath         NVARCHAR(500)       NOT NULL,
    FolderSizeBytes      BIGINT              NOT NULL    CONSTRAINT DF_sto_Folder_SizeBytes   DEFAULT 0,
    FileCount            INT                 NOT NULL    CONSTRAINT DF_sto_Folder_FileCount   DEFAULT 0,
    IsArchived           BIT                 NOT NULL    CONSTRAINT DF_sto_Folder_IsArchived  DEFAULT 0,
    ArchivedBy           INT                     NULL,
    ArchivedDate         DATETIME                NULL,
    IsDeleted            BIT                 NOT NULL    CONSTRAINT DF_sto_Folder_IsDeleted   DEFAULT 0,
    DeletedBy            INT                     NULL,
    DeletedDate          DATETIME                NULL,
    RecycleBinExpiry     DATETIME                NULL,   -- DeletedDate + 30 วัน
    InheritPermission    BIT                 NOT NULL    CONSTRAINT DF_sto_Folder_InheritPerm DEFAULT 1,
    CreatedBy            INT                 NOT NULL,
    CreatedDate          DATETIME            NOT NULL    CONSTRAINT DF_sto_Folder_CreatedDate DEFAULT GETDATE(),
    UpdatedBy            INT                     NULL,
    UpdatedDate          DATETIME                NULL,
    CONSTRAINT PK_sto_Folder                PRIMARY KEY (FolderID),
    CONSTRAINT UQ_sto_Folder_GUID           UNIQUE      (FolderGUID),
    CONSTRAINT CK_sto_Folder_ConfLevel      CHECK       (ConfidentialityLevel IN (1, 2, 3, 4)),
    CONSTRAINT CK_sto_Folder_Depth         CHECK       (FolderDepth BETWEEN 0 AND 10),
    CONSTRAINT FK_sto_Folder_Parent         FOREIGN KEY (ParentFolderID) REFERENCES storage.Folder(FolderID),
    CONSTRAINT FK_sto_Folder_OrgUnit        FOREIGN KEY (OrgUnitID)      REFERENCES core.OrgUnit(OrgUnitID),
    CONSTRAINT FK_sto_Folder_Template       FOREIGN KEY (TemplateID)     REFERENCES storage.FolderTemplate(TemplateID),
    CONSTRAINT FK_sto_Folder_CreatedBy      FOREIGN KEY (CreatedBy)      REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_sto_Folder_ArchivedBy     FOREIGN KEY (ArchivedBy)     REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_sto_Folder_DeletedBy      FOREIGN KEY (DeletedBy)      REFERENCES core.UserAccount(UserID)
);
GO

CREATE TABLE storage.FileDocument (
    FileID           INT IDENTITY(1,1)   NOT NULL,
    FileGUID         UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT DF_sto_File_GUID        DEFAULT NEWID(),
    FolderID         INT                 NOT NULL,
    OriginalFileName NVARCHAR(260)       NOT NULL,
    StoredFileName   NVARCHAR(360)       NOT NULL,
    FileExtension    VARCHAR(20)         NOT NULL,   -- lowercase .pdf / .docx
    MimeType         VARCHAR(100)        NOT NULL,   -- ป้องกัน extension spoofing
    FileSizeBytes    BIGINT              NOT NULL,
    PhysicalPath     NVARCHAR(500)       NOT NULL,
    Checksum         VARCHAR(64)         NOT NULL,   -- SHA-256
    VirusScanStatus  TINYINT             NOT NULL    CONSTRAINT DF_sto_File_VirusScan   DEFAULT 0,
    VirusScanDate    DATETIME                NULL,
    FileStatus       TINYINT             NOT NULL    CONSTRAINT DF_sto_File_Status      DEFAULT 1,
    UploadedBy       INT                 NOT NULL,
    UploadedDate     DATETIME            NOT NULL    CONSTRAINT DF_sto_File_UploadDate  DEFAULT GETDATE(),
    ArchivedBy       INT                     NULL,
    ArchivedDate     DATETIME                NULL,
    DeletedBy        INT                     NULL,
    DeletedDate      DATETIME                NULL,
    Version          INT                 NOT NULL    CONSTRAINT DF_sto_File_Version     DEFAULT 1,
    CONSTRAINT PK_sto_FileDocument              PRIMARY KEY (FileID),
    CONSTRAINT UQ_sto_FileDocument_GUID         UNIQUE      (FileGUID),
    CONSTRAINT CK_sto_FileDocument_VirusScan    CHECK       (VirusScanStatus IN (0, 1, 2, 3)),
    CONSTRAINT CK_sto_FileDocument_Status       CHECK       (FileStatus       IN (1, 2, 3)),
    CONSTRAINT FK_sto_FileDocument_Folder       FOREIGN KEY (FolderID)   REFERENCES storage.Folder(FolderID),
    CONSTRAINT FK_sto_FileDocument_Uploader     FOREIGN KEY (UploadedBy) REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_sto_FileDocument_ArchivedBy   FOREIGN KEY (ArchivedBy) REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_sto_FileDocument_DeletedBy    FOREIGN KEY (DeletedBy)  REFERENCES core.UserAccount(UserID)
);
GO

-- ============================================================
-- SCHEMA: workflow  |  Tables: ApprovalRequest, ApprovalStep
-- ============================================================

CREATE TABLE workflow.ApprovalRequest (
    RequestID       INT IDENTITY(1,1)   NOT NULL,
    RequestType     VARCHAR(50)         NOT NULL,   -- CREATE_HC | DELETE_FOLDER | CHANGE_PERMISSION | UPLOAD_HC
    FolderID        INT                     NULL,
    FileID          INT                     NULL,
    PermissionID    INT                     NULL,
    RequestedBy     INT                 NOT NULL,
    RequestedDate   DATETIME            NOT NULL    CONSTRAINT DF_wf_Req_ReqDate  DEFAULT GETDATE(),
    RequestStatus   TINYINT             NOT NULL    CONSTRAINT DF_wf_Req_Status   DEFAULT 1,  -- 1=Pending,2=Approved,3=Rejected,4=Expired
    Subject         NVARCHAR(500)           NULL,
    Remark          NVARCHAR(MAX)           NULL,
    ExpiryDate      DATETIME                NULL,
    ResolvedBy      INT                     NULL,
    ResolvedDate    DATETIME                NULL,
    ResolveRemark   NVARCHAR(500)           NULL,
    CONSTRAINT PK_wf_ApprovalRequest            PRIMARY KEY (RequestID),
    CONSTRAINT CK_wf_ApprovalRequest_Type       CHECK (RequestType    IN ('CREATE_HC','DELETE_FOLDER','CHANGE_PERMISSION','UPLOAD_HC')),
    CONSTRAINT CK_wf_ApprovalRequest_Status     CHECK (RequestStatus  IN (1, 2, 3, 4)),
    CONSTRAINT FK_wf_ApprovalRequest_Folder     FOREIGN KEY (FolderID)     REFERENCES storage.Folder(FolderID),
    CONSTRAINT FK_wf_ApprovalRequest_File       FOREIGN KEY (FileID)       REFERENCES storage.FileDocument(FileID),
    CONSTRAINT FK_wf_ApprovalRequest_Perm       FOREIGN KEY (PermissionID) REFERENCES security.FolderPermission(PermissionID),
    CONSTRAINT FK_wf_ApprovalRequest_ReqBy      FOREIGN KEY (RequestedBy)  REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_wf_ApprovalRequest_ResolvedBy FOREIGN KEY (ResolvedBy)   REFERENCES core.UserAccount(UserID)
);
GO

CREATE TABLE workflow.ApprovalStep (
    StepID          INT IDENTITY(1,1)   NOT NULL,
    RequestID       INT                 NOT NULL,
    StepOrder       INT                 NOT NULL,
    ApproverID      INT                 NOT NULL,
    StepStatus      TINYINT             NOT NULL    CONSTRAINT DF_wf_Step_Status  DEFAULT 1,  -- 1=Pending,2=Approved,3=Rejected,4=Delegated
    ActionDate      DATETIME                NULL,
    ActionRemark    NVARCHAR(500)           NULL,
    DelegatedTo     INT                     NULL,
    NotifiedDate    DATETIME                NULL,
    EscalatedDate   DATETIME                NULL,
    CONSTRAINT PK_wf_ApprovalStep           PRIMARY KEY (StepID),
    CONSTRAINT UQ_wf_ApprovalStep_Order     UNIQUE      (RequestID, StepOrder),
    CONSTRAINT CK_wf_ApprovalStep_Status    CHECK       (StepStatus IN (1, 2, 3, 4)),
    CONSTRAINT FK_wf_ApprovalStep_Request   FOREIGN KEY (RequestID)   REFERENCES workflow.ApprovalRequest(RequestID),
    CONSTRAINT FK_wf_ApprovalStep_Approver  FOREIGN KEY (ApproverID)  REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_wf_ApprovalStep_Delegate  FOREIGN KEY (DelegatedTo) REFERENCES core.UserAccount(UserID)
);
GO

-- ============================================================
-- SCHEMA: audit  |  Tables: AuditLog
-- ============================================================

CREATE TABLE audit.AuditLog (
    AuditID      BIGINT IDENTITY(1,1)  NOT NULL,
    AuditDate    DATETIME              NOT NULL    CONSTRAINT DF_aud_AuditLog_Date DEFAULT GETDATE(),
    UserID       INT                   NOT NULL,
    UserName     NVARCHAR(100)         NOT NULL,   -- Snapshot
    UserRole     TINYINT                   NULL,   -- Snapshot AppRole ณ เวลานั้น
    Module       VARCHAR(50)           NOT NULL,   -- OrgStructure | Folder | File | Permission
    Action       VARCHAR(50)           NOT NULL,   -- CREATE | UPDATE | DELETE | VIEW | UPLOAD | DOWNLOAD | APPROVE | REJECT
    TableName    VARCHAR(100)              NULL,
    RecordID     INT                       NULL,
    OldValue     NVARCHAR(MAX)             NULL,   -- JSON before
    NewValue     NVARCHAR(MAX)             NULL,   -- JSON after
    IPAddress    VARCHAR(45)               NULL,   -- IPv4/IPv6
    MachineName  NVARCHAR(100)             NULL,
    SessionID    NVARCHAR(100)             NULL,
    Result       TINYINT               NOT NULL,   -- 1=Success, 2=Failure, 3=Denied
    ErrorMessage NVARCHAR(500)             NULL,
    CONSTRAINT PK_aud_AuditLog          PRIMARY KEY (AuditID),
    CONSTRAINT CK_aud_AuditLog_Action   CHECK (Action IN ('CREATE','UPDATE','DELETE','VIEW','UPLOAD','DOWNLOAD','APPROVE','REJECT')),
    CONSTRAINT CK_aud_AuditLog_Result   CHECK (Result IN (1, 2, 3)),
    CONSTRAINT FK_aud_AuditLog_User     FOREIGN KEY (UserID) REFERENCES core.UserAccount(UserID)
);
GO

-- ============================================================
-- ALTER: FK ข้าม Schema ที่ต้องสร้างทีหลัง
-- ============================================================

ALTER TABLE security.FolderPermission
    ADD CONSTRAINT FK_sec_FolderPerm_Folder
    FOREIGN KEY (FolderID) REFERENCES storage.Folder(FolderID);
GO

ALTER TABLE core.OrgUnit
    ADD CONSTRAINT FK_core_OrgUnit_DeletedBy
    FOREIGN KEY (DeletedBy) REFERENCES core.UserAccount(UserID);
GO

-- ============================================================
-- INDEXES
-- ============================================================

-- core.OrgUnit
CREATE INDEX IX_core_OrgUnit_ParentID  ON core.OrgUnit(ParentOrgUnitID)  INCLUDE (OrgUnitType, IsActive);
CREATE INDEX IX_core_OrgUnit_Type      ON core.OrgUnit(OrgUnitType, IsActive);

-- core.UserAccount
CREATE INDEX IX_core_User_OrgUnit      ON core.UserAccount(OrgUnitID, IsActive);
CREATE INDEX IX_core_User_Role         ON core.UserAccount(AppRole,   IsActive);

-- security.PermissionGroup / UserGroupMember
CREATE INDEX IX_sec_UGM_User           ON security.UserGroupMember(UserID,  IsActive);
CREATE INDEX IX_sec_UGM_Group          ON security.UserGroupMember(GroupID, IsActive);

-- security.FolderPermission
CREATE INDEX IX_sec_Perm_Folder        ON security.FolderPermission(FolderID, PrincipalType, IsDeny);
CREATE INDEX IX_sec_Perm_Expiry        ON security.FolderPermission(ExpiryDate) WHERE ExpiryDate IS NOT NULL;
CREATE INDEX IX_sec_Perm_Principal     ON security.FolderPermission(PrincipalType, PrincipalID);

-- storage.Folder
CREATE INDEX IX_sto_Folder_ParentID    ON storage.Folder(ParentFolderID)  INCLUDE (FolderName, ConfidentialityLevel, IsDeleted);
CREATE INDEX IX_sto_Folder_OrgUnit     ON storage.Folder(OrgUnitID, IsDeleted);
CREATE INDEX IX_sto_Folder_ConfLevel   ON storage.Folder(ConfidentialityLevel, IsDeleted);
CREATE INDEX IX_sto_Folder_Recycle     ON storage.Folder(IsDeleted, RecycleBinExpiry) WHERE IsDeleted  = 1;
CREATE INDEX IX_sto_Folder_Archived    ON storage.Folder(IsArchived, OrgUnitID)       WHERE IsArchived = 1;

-- storage.FileDocument
CREATE INDEX IX_sto_File_FolderID      ON storage.FileDocument(FolderID, FileStatus) INCLUDE (OriginalFileName, FileSizeBytes, UploadedDate);
CREATE INDEX IX_sto_File_VirusScan     ON storage.FileDocument(VirusScanStatus)       WHERE VirusScanStatus = 0;
CREATE INDEX IX_sto_File_Checksum      ON storage.FileDocument(Checksum, FolderID);

-- workflow.ApprovalRequest
CREATE INDEX IX_wf_Approval_Status     ON workflow.ApprovalRequest(RequestStatus, RequestedDate) WHERE RequestStatus = 1;
CREATE INDEX IX_wf_Approval_ReqBy      ON workflow.ApprovalRequest(RequestedBy, RequestStatus);

-- workflow.ApprovalStep
CREATE INDEX IX_wf_Step_Request        ON workflow.ApprovalStep(RequestID,  StepOrder);
CREATE INDEX IX_wf_Step_Approver       ON workflow.ApprovalStep(ApproverID, StepStatus) WHERE StepStatus = 1;

-- audit.AuditLog
CREATE INDEX IX_aud_Audit_Date         ON audit.AuditLog(AuditDate DESC)  INCLUDE (UserID, Module, Action);
CREATE INDEX IX_aud_Audit_User         ON audit.AuditLog(UserID, AuditDate DESC);
CREATE INDEX IX_aud_Audit_Module       ON audit.AuditLog(Module, Action,  AuditDate DESC);
GO
