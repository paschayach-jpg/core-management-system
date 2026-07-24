/*
================================================================================
  ระบบ: Core Management System (CMS) — โปรเจ็ค 2511AL01
  ไฟล์: 03_KMS_WFE_TMS_Schema.sql
  Schemas: KMS, WFE, TMS
  วัตถุประสงค์:
    KMS = Knowledge Base, Article, Announcement (MOD-006, MOD-007)
    WFE = Workflow Engine (MOD-008)
    TMS = Task Management, Todo, Notes, Work Instructions (MOD-009 ถึง MOD-012)
  Database: SQL Server 2019 | Collation: Thai_CI_AS
================================================================================
*/

-- สร้าง Schemas
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'KMS') EXEC('CREATE SCHEMA KMS AUTHORIZATION dbo');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'WFE') EXEC('CREATE SCHEMA WFE AUTHORIZATION dbo');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'TMS') EXEC('CREATE SCHEMA TMS AUTHORIZATION dbo');
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
================================================================================
  KMS — Knowledge Management System
================================================================================
*/

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: KMS.m_KBCategory
  คำอธิบาย: หมวดหมู่บทความใน Knowledge Base รองรับ Hierarchy
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('KMS.m_KBCategory', 'U') IS NOT NULL DROP TABLE KMS.m_KBCategory;
GO
CREATE TABLE KMS.m_KBCategory
(
    CategoryID      INT IDENTITY(1,1)   NOT NULL,
    ParentCategoryID INT                NULL,        -- Self-reference: NULL = Root Category
    CategoryCode    VARCHAR(20)         NOT NULL,   -- รหัสหมวด เช่น KB-IT-001
    CategoryName    NVARCHAR(100)       NOT NULL,   -- ชื่อหมวดหมู่
    Icon            VARCHAR(50)         NULL,        -- Icon class เช่น fa-book
    Description     NVARCHAR(300)       NULL,
    SortOrder       INT                 NOT NULL CONSTRAINT DF_KMS_KBCat_Sort DEFAULT(0),
    IsActive        BIT                 NOT NULL CONSTRAINT DF_KMS_KBCat_Act  DEFAULT(1),
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_KMS_KBCat_Cdt  DEFAULT(GETDATE()),

    CONSTRAINT PK_KMS_KBCategory       PRIMARY KEY CLUSTERED (CategoryID ASC),
    CONSTRAINT UQ_KMS_KBCat_Code       UNIQUE (CategoryCode),
    CONSTRAINT FK_KMS_KBCat_Parent     FOREIGN KEY (ParentCategoryID) REFERENCES KMS.m_KBCategory (CategoryID)
);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: KMS.t_KBArticle
  คำอธิบาย: บทความ Knowledge Base (SOP, FAQ, Guideline, Template)
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('KMS.t_KBArticle', 'U') IS NOT NULL DROP TABLE KMS.t_KBArticle;
GO
CREATE TABLE KMS.t_KBArticle
(
    ArticleID           INT IDENTITY(1,1)   NOT NULL,
    ArticleCode         VARCHAR(20)         NOT NULL,   -- รหัสบทความ เช่น KB-2566-001
    Title               NVARCHAR(500)       NOT NULL,   -- ชื่อบทความ
    ContentHTML         NVARCHAR(MAX)       NOT NULL,   -- เนื้อหาบทความ (HTML)
    Summary             NVARCHAR(1000)      NULL,        -- บทสรุปสั้น
    CategoryID          INT                 NOT NULL,   -- FK → KMS.m_KBCategory
    ArticleType         TINYINT             NOT NULL,   -- 1=SOP, 2=FAQ, 3=Guideline, 4=Template, 5=Policy
    Status              TINYINT             NOT NULL CONSTRAINT DF_KMS_Art_Stat DEFAULT(1), -- 1=Draft,2=Review,3=Published,4=Archived
    CurrentVersion      INT                 NOT NULL CONSTRAINT DF_KMS_Art_Ver  DEFAULT(1),
    PublishedDate       DATETIME            NULL,
    ExpiryDate          DATETIME            NULL,        -- NULL = ไม่มีวันหมดอายุ
    ReviewDueDate       DATETIME            NULL,        -- วันที่ต้อง Review ใหม่
    ViewCount           INT                 NOT NULL CONSTRAINT DF_KMS_Art_VC   DEFAULT(0),
    LikeCount           INT                 NOT NULL CONSTRAINT DF_KMS_Art_Like DEFAULT(0),
    IsOfficialPolicy    BIT                 NOT NULL CONSTRAINT DF_KMS_Art_Pol  DEFAULT(0), -- เป็น Official Policy หรือไม่
    Tags                NVARCHAR(500)       NULL,        -- Tags ค้นหา (Comma-separated)
    CreatedBy           NVARCHAR(100)       NOT NULL,
    CreatedDate         DATETIME            NOT NULL CONSTRAINT DF_KMS_Art_Cdt  DEFAULT(GETDATE()),
    ReviewedBy          NVARCHAR(100)       NULL,
    ReviewedDate        DATETIME            NULL,
    UpdatedBy           NVARCHAR(100)       NULL,
    UpdatedDate         DATETIME            NULL,

    CONSTRAINT PK_KMS_KBArticle        PRIMARY KEY CLUSTERED (ArticleID ASC),
    CONSTRAINT UQ_KMS_Art_Code         UNIQUE (ArticleCode),
    CONSTRAINT CK_KMS_Art_Type         CHECK (ArticleType IN (1, 2, 3, 4, 5)),
    CONSTRAINT CK_KMS_Art_Status       CHECK (Status IN (1, 2, 3, 4)),
    CONSTRAINT FK_KMS_Art_Category     FOREIGN KEY (CategoryID) REFERENCES KMS.m_KBCategory (CategoryID)
);
GO
CREATE NONCLUSTERED INDEX IX_KMS_Art_Status    ON KMS.t_KBArticle (Status, PublishedDate DESC, CategoryID);
CREATE NONCLUSTERED INDEX IX_KMS_Art_Category  ON KMS.t_KBArticle (CategoryID, Status, ArticleType);
CREATE NONCLUSTERED INDEX IX_KMS_Art_Expiry    ON KMS.t_KBArticle (ExpiryDate, Status) WHERE ExpiryDate IS NOT NULL;
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: KMS.t_KBVersion
  คำอธิบาย: Version History ของบทความ
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('KMS.t_KBVersion', 'U') IS NOT NULL DROP TABLE KMS.t_KBVersion;
GO
CREATE TABLE KMS.t_KBVersion
(
    VersionID           INT IDENTITY(1,1)   NOT NULL,
    ArticleID           INT                 NOT NULL,
    VersionNumber       INT                 NOT NULL,
    ContentSnapshot     NVARCHAR(MAX)       NOT NULL,   -- Snapshot เนื้อหาทั้งหมด
    ChangeDescription   NVARCHAR(500)       NULL,
    CreatedBy           NVARCHAR(100)       NOT NULL,
    CreatedDate         DATETIME            NOT NULL CONSTRAINT DF_KMS_KBVer_Cdt DEFAULT(GETDATE()),

    CONSTRAINT PK_KMS_KBVersion    PRIMARY KEY CLUSTERED (VersionID ASC),
    CONSTRAINT UQ_KMS_KBVer_Unique UNIQUE (ArticleID, VersionNumber),
    CONSTRAINT FK_KMS_KBVer_Art    FOREIGN KEY (ArticleID) REFERENCES KMS.t_KBArticle (ArticleID)
);
GO
CREATE NONCLUSTERED INDEX IX_KMS_KBVer_Article ON KMS.t_KBVersion (ArticleID, VersionNumber DESC);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: KMS.t_KBArticleLink
  คำอธิบาย: เชื่อม Article กับ Entity อื่นๆ (Folder, Task, Program)
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('KMS.t_KBArticleLink', 'U') IS NOT NULL DROP TABLE KMS.t_KBArticleLink;
GO
CREATE TABLE KMS.t_KBArticleLink
(
    LinkID          INT IDENTITY(1,1)   NOT NULL,
    ArticleID       INT                 NOT NULL,
    EntityType      TINYINT             NOT NULL,   -- 1=Folder, 2=Task, 3=Program, 4=Law, 5=Project
    EntityID        INT                 NOT NULL,
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_KMS_ArtLink_Cdt DEFAULT(GETDATE()),

    CONSTRAINT PK_KMS_ArtLink         PRIMARY KEY CLUSTERED (LinkID ASC),
    CONSTRAINT UQ_KMS_ArtLink_Unique  UNIQUE (ArticleID, EntityType, EntityID),
    CONSTRAINT CK_KMS_ArtLink_EType   CHECK (EntityType IN (1, 2, 3, 4, 5)),
    CONSTRAINT FK_KMS_ArtLink_Art     FOREIGN KEY (ArticleID) REFERENCES KMS.t_KBArticle (ArticleID)
);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: KMS.t_Announcement
  คำอธิบาย: ประกาศ/แจ้งเวียนในองค์กร (MOD-007)
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('KMS.t_Announcement', 'U') IS NOT NULL DROP TABLE KMS.t_Announcement;
GO
CREATE TABLE KMS.t_Announcement
(
    AnnouncementID  INT IDENTITY(1,1)   NOT NULL,
    AnnouncementCode VARCHAR(20)        NOT NULL,   -- รหัสประกาศ เช่น ANN-2566-001
    Title           NVARCHAR(500)       NOT NULL,
    ContentHTML     NVARCHAR(MAX)       NOT NULL,
    AttachmentPath  NVARCHAR(500)       NULL,        -- ไฟล์แนบ
    Priority        TINYINT             NOT NULL,   -- 1=Critical, 2=High, 3=Medium, 4=Low
    Status          TINYINT             NOT NULL CONSTRAINT DF_KMS_Ann_Stat DEFAULT(1), -- 1=Draft, 2=Active, 3=Expired
    StartDate       DATETIME            NULL,
    EndDate         DATETIME            NULL,
    TargetAll       BIT                 NOT NULL CONSTRAINT DF_KMS_Ann_All  DEFAULT(0), -- ส่งทุกคนในองค์กร
    ShowPopup       BIT                 NOT NULL CONSTRAINT DF_KMS_Ann_Pop  DEFAULT(1),
    MandatoryRead   BIT                 NOT NULL CONSTRAINT DF_KMS_Ann_Mand DEFAULT(0), -- ต้องกดยืนยันอ่าน
    ShowOnce        BIT                 NOT NULL CONSTRAINT DF_KMS_Ann_Once DEFAULT(0),
    SendEmail       BIT                 NOT NULL CONSTRAINT DF_KMS_Ann_Mail DEFAULT(0),
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_KMS_Ann_Cdt  DEFAULT(GETDATE()),
    PublishedBy     NVARCHAR(100)       NULL,
    PublishedDate   DATETIME            NULL,

    CONSTRAINT PK_KMS_Announcement    PRIMARY KEY CLUSTERED (AnnouncementID ASC),
    CONSTRAINT UQ_KMS_Ann_Code        UNIQUE (AnnouncementCode),
    CONSTRAINT CK_KMS_Ann_Priority    CHECK (Priority IN (1, 2, 3, 4)),
    CONSTRAINT CK_KMS_Ann_Status      CHECK (Status IN (1, 2, 3))
);
GO
CREATE NONCLUSTERED INDEX IX_KMS_Ann_Active
    ON KMS.t_Announcement (Status, StartDate, EndDate, Priority)
    WHERE Status IN (2, 3);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: KMS.t_AnnouncementTarget
  คำอธิบาย: กลุ่มเป้าหมายของประกาศ
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('KMS.t_AnnouncementTarget', 'U') IS NOT NULL DROP TABLE KMS.t_AnnouncementTarget;
GO
CREATE TABLE KMS.t_AnnouncementTarget
(
    TargetID        INT IDENTITY(1,1)   NOT NULL,
    AnnouncementID  INT                 NOT NULL,
    TargetType      TINYINT             NOT NULL,   -- 1=OrgUnit, 2=Role, 3=User, 4=Group
    TargetEntityID  INT                 NOT NULL,

    CONSTRAINT PK_KMS_AnnTarget        PRIMARY KEY CLUSTERED (TargetID ASC),
    CONSTRAINT UQ_KMS_AnnTarget_Unique UNIQUE (AnnouncementID, TargetType, TargetEntityID),
    CONSTRAINT CK_KMS_AnnTarget_Type   CHECK (TargetType IN (1, 2, 3, 4)),
    CONSTRAINT FK_KMS_AnnTarget_Ann    FOREIGN KEY (AnnouncementID) REFERENCES KMS.t_Announcement (AnnouncementID)
);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: KMS.l_AnnouncementReadLog
  คำอธิบาย: Log การอ่านประกาศของแต่ละ User
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('KMS.l_AnnouncementReadLog', 'U') IS NOT NULL DROP TABLE KMS.l_AnnouncementReadLog;
GO
CREATE TABLE KMS.l_AnnouncementReadLog
(
    LogID           INT IDENTITY(1,1)   NOT NULL,
    AnnouncementID  INT                 NOT NULL,
    UserID          INT                 NOT NULL,
    ADUserName      NVARCHAR(100)       NOT NULL,
    ViewedDate      DATETIME            NOT NULL CONSTRAINT DF_KMS_AnnLog_Dt  DEFAULT(GETDATE()),
    IsAcknowledged  BIT                 NOT NULL CONSTRAINT DF_KMS_AnnLog_Ack DEFAULT(0),
    AcknowledgedDate DATETIME           NULL,

    CONSTRAINT PK_KMS_AnnReadLog       PRIMARY KEY CLUSTERED (LogID ASC),
    CONSTRAINT UQ_KMS_AnnRead_UserAnn  UNIQUE (AnnouncementID, UserID)
);
GO
CREATE NONCLUSTERED INDEX IX_KMS_AnnLog_User
    ON KMS.l_AnnouncementReadLog (UserID, ViewedDate DESC)
    INCLUDE (AnnouncementID, IsAcknowledged);
GO


/*
================================================================================
  WFE — Workflow Engine
================================================================================
*/

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: WFE.m_WorkflowDefinition
  คำอธิบาย: คำนิยาม Workflow (Template) ที่ Admin กำหนดไว้
             Version Freeze: Instance ถูก Freeze ตาม Version ณ เวลาที่เริ่ม
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('WFE.m_WorkflowDefinition', 'U') IS NOT NULL DROP TABLE WFE.m_WorkflowDefinition;
GO
CREATE TABLE WFE.m_WorkflowDefinition
(
    WorkflowDefID   INT IDENTITY(1,1)   NOT NULL,
    WorkflowCode    VARCHAR(20)         NOT NULL,   -- รหัส Workflow เช่น WF-DOC-001
    WorkflowName    NVARCHAR(200)       NOT NULL,   -- ชื่อ Workflow
    Category        TINYINT             NOT NULL,   -- 1=Document Approval, 2=Task Approval, 3=HR, 4=IT, 5=Custom
    Status          TINYINT             NOT NULL CONSTRAINT DF_WFE_Def_Stat DEFAULT(1), -- 1=Draft, 2=Active, 3=Inactive
    DefinitionJSON  NVARCHAR(MAX)       NOT NULL,   -- Serialized Node Graph (Steps, Transitions, Conditions)
    Version         INT                 NOT NULL CONSTRAINT DF_WFE_Def_Ver  DEFAULT(1),
    IsTemplate      BIT                 NOT NULL CONSTRAINT DF_WFE_Def_Tmpl DEFAULT(0),
    TriggerType     TINYINT             NOT NULL CONSTRAINT DF_WFE_Def_Trig DEFAULT(1), -- 1=Manual, 2=Auto-File, 3=Scheduled
    SLAPolicy       NVARCHAR(500)       NULL,        -- JSON SLA Policy
    Description     NVARCHAR(500)       NULL,
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_WFE_Def_Cdt  DEFAULT(GETDATE()),
    ApprovedBy      NVARCHAR(100)       NULL,
    ApprovedDate    DATETIME            NULL,
    UpdatedBy       NVARCHAR(100)       NULL,
    UpdatedDate     DATETIME            NULL,

    CONSTRAINT PK_WFE_WorkflowDef     PRIMARY KEY CLUSTERED (WorkflowDefID ASC),
    CONSTRAINT UQ_WFE_Def_Code        UNIQUE (WorkflowCode),
    CONSTRAINT CK_WFE_Def_Category    CHECK (Category IN (1, 2, 3, 4, 5)),
    CONSTRAINT CK_WFE_Def_Status      CHECK (Status IN (1, 2, 3))
);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: WFE.m_WorkflowStep
  คำอธิบาย: ขั้นตอน (Step) ของ Workflow Definition
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('WFE.m_WorkflowStep', 'U') IS NOT NULL DROP TABLE WFE.m_WorkflowStep;
GO
CREATE TABLE WFE.m_WorkflowStep
(
    StepID              INT IDENTITY(1,1)   NOT NULL,
    WorkflowDefID       INT                 NOT NULL,   -- FK → WFE.m_WorkflowDefinition
    StepCode            VARCHAR(20)         NOT NULL,   -- รหัส Step เช่น STEP-001
    StepName            NVARCHAR(200)       NOT NULL,
    StepType            TINYINT             NOT NULL,   -- 1=Approval, 2=Review, 3=Notification, 4=Action, 5=Decision
    AssigneeType        TINYINT             NOT NULL,   -- 1=User, 2=Role, 3=OrgUnit, 4=Manager
    AssigneeID          INT                 NOT NULL,   -- PrincipalID ตาม AssigneeType
    SLAHours            INT                 NOT NULL CONSTRAINT DF_WFE_Step_SLA  DEFAULT(0),   -- 0 = ไม่กำหนด SLA
    EscalationHours     INT                 NOT NULL CONSTRAINT DF_WFE_Step_Esc  DEFAULT(0),
    EscalateToUserID    INT                 NULL,
    Instructions        NVARCHAR(MAX)       NULL,        -- คำแนะนำสำหรับผู้ดำเนินการ
    ConditionExpression NVARCHAR(500)       NULL,        -- Expression สำหรับ Routing (เช่น Amount > 100000)
    AllowReject         BIT                 NOT NULL CONSTRAINT DF_WFE_Step_Rej  DEFAULT(1),
    AllowDelegate       BIT                 NOT NULL CONSTRAINT DF_WFE_Step_Del  DEFAULT(0),
    RequireComment      BIT                 NOT NULL CONSTRAINT DF_WFE_Step_Cmt  DEFAULT(0),
    SortOrder           INT                 NOT NULL CONSTRAINT DF_WFE_Step_Sort DEFAULT(0),

    CONSTRAINT PK_WFE_WorkflowStep    PRIMARY KEY CLUSTERED (StepID ASC),
    CONSTRAINT UQ_WFE_Step_Code       UNIQUE (WorkflowDefID, StepCode),
    CONSTRAINT CK_WFE_Step_Type       CHECK (StepType IN (1, 2, 3, 4, 5)),
    CONSTRAINT CK_WFE_Step_AssType    CHECK (AssigneeType IN (1, 2, 3, 4)),
    CONSTRAINT FK_WFE_Step_Def        FOREIGN KEY (WorkflowDefID) REFERENCES WFE.m_WorkflowDefinition (WorkflowDefID)
);
GO
CREATE NONCLUSTERED INDEX IX_WFE_Step_Def
    ON WFE.m_WorkflowStep (WorkflowDefID, SortOrder)
    INCLUDE (StepID, StepCode, StepName, StepType, AssigneeType, AssigneeID);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: WFE.t_WorkflowInstance
  คำอธิบาย: Instance ของ Workflow ที่กำลังดำเนินการอยู่
             ถูก Freeze ตาม Definition Version ณ เวลาที่เริ่ม
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('WFE.t_WorkflowInstance', 'U') IS NOT NULL DROP TABLE WFE.t_WorkflowInstance;
GO
CREATE TABLE WFE.t_WorkflowInstance
(
    InstanceID          INT IDENTITY(1,1)   NOT NULL,
    InstanceCode        VARCHAR(30)         NOT NULL,   -- รหัส Instance เช่น WFI-2566-00001
    WorkflowDefID       INT                 NOT NULL,   -- FK → WFE.m_WorkflowDefinition
    FrozenDefVersion    INT                 NOT NULL,   -- Version ที่ Freeze ไว้ตอนเริ่ม
    FrozenDefJSON       NVARCHAR(MAX)       NOT NULL,   -- Snapshot JSON ของ Definition ตอนเริ่ม
    Status              TINYINT             NOT NULL CONSTRAINT DF_WFE_Inst_Stat DEFAULT(1), -- 1=Running, 2=Completed, 3=Rejected, 4=Cancelled, 5=Faulted
    CurrentStepID       INT                 NULL,       -- FK → WFE.t_WorkflowStepInstance
    TriggerType         TINYINT             NOT NULL CONSTRAINT DF_WFE_Inst_Trig DEFAULT(1),
    TriggerByUserID     INT                 NOT NULL,
    LinkedFileID        INT                 NULL,       -- เชื่อมกับ File (ถ้ามี)
    LinkedTaskID        INT                 NULL,       -- เชื่อมกับ Task (ถ้ามี)
    LinkedFolderID      INT                 NULL,       -- เชื่อมกับ Folder (ถ้ามี)
    Subject             NVARCHAR(500)       NULL,        -- หัวเรื่อง/คำอธิบาย
    ContextDataJSON     NVARCHAR(MAX)       NULL,        -- JSON Data ที่ส่งผ่าน Workflow
    StartDate           DATETIME            NOT NULL CONSTRAINT DF_WFE_Inst_Sdt  DEFAULT(GETDATE()),
    CompletedDate       DATETIME            NULL,
    DueDate             DATETIME            NULL,        -- Calculated: StartDate + Max SLA
    CompletionPct       DECIMAL(5,2)        NOT NULL CONSTRAINT DF_WFE_Inst_Pct  DEFAULT(0),
    FaultMessage        NVARCHAR(MAX)       NULL,        -- Error ถ้า Status=5 (Faulted)
    CreatedBy           NVARCHAR(100)       NOT NULL,
    CreatedDate         DATETIME            NOT NULL CONSTRAINT DF_WFE_Inst_Cdt  DEFAULT(GETDATE()),

    CONSTRAINT PK_WFE_WFInstance       PRIMARY KEY CLUSTERED (InstanceID ASC),
    CONSTRAINT UQ_WFE_Inst_Code        UNIQUE (InstanceCode),
    CONSTRAINT CK_WFE_Inst_Status      CHECK (Status IN (1, 2, 3, 4, 5)),
    CONSTRAINT FK_WFE_Inst_Def         FOREIGN KEY (WorkflowDefID) REFERENCES WFE.m_WorkflowDefinition (WorkflowDefID)
);
GO
CREATE NONCLUSTERED INDEX IX_WFE_Inst_Running
    ON WFE.t_WorkflowInstance (Status, CurrentStepID)
    WHERE Status = 1;
GO
CREATE NONCLUSTERED INDEX IX_WFE_Inst_User
    ON WFE.t_WorkflowInstance (TriggerByUserID, Status, StartDate DESC)
    INCLUDE (InstanceCode, WorkflowDefID, Subject);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: WFE.t_WorkflowStepInstance
  คำอธิบาย: Step Instance ของแต่ละ Workflow ที่กำลังดำเนินการ (Runtime)
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('WFE.t_WorkflowStepInstance', 'U') IS NOT NULL DROP TABLE WFE.t_WorkflowStepInstance;
GO
CREATE TABLE WFE.t_WorkflowStepInstance
(
    StepInstanceID  INT IDENTITY(1,1)   NOT NULL,
    InstanceID      INT                 NOT NULL,   -- FK → WFE.t_WorkflowInstance
    StepID          INT                 NOT NULL,   -- FK → WFE.m_WorkflowStep (Definition)
    StepOrder       INT                 NOT NULL,   -- ลำดับ Step ใน Instance
    AssignedUserID  INT                 NOT NULL,   -- User ที่รับผิดชอบ Step นี้
    AssignedBy      NVARCHAR(100)       NOT NULL,
    Status          TINYINT             NOT NULL CONSTRAINT DF_WFE_StepI_Stat DEFAULT(1), -- 1=Pending, 2=InProgress, 3=Approved, 4=Rejected, 5=Delegated, 6=Escalated
    StartedDate     DATETIME            NULL,
    DueDate         DATETIME            NULL,
    CompletedDate   DATETIME            NULL,
    Decision        VARCHAR(50)         NULL,        -- Approved, Rejected, Delegate
    Comments        NVARCHAR(MAX)       NULL,
    IsEscalated     BIT                 NOT NULL CONSTRAINT DF_WFE_StepI_Esc DEFAULT(0),
    EscalatedDate   DATETIME            NULL,
    DelegatedToID   INT                 NULL,        -- ถ้า Delegate ให้ User อื่น
    CompletedBy     NVARCHAR(100)       NULL,        -- ADUserName

    CONSTRAINT PK_WFE_StepInstance     PRIMARY KEY CLUSTERED (StepInstanceID ASC),
    CONSTRAINT CK_WFE_StepI_Status     CHECK (Status IN (1, 2, 3, 4, 5, 6)),
    CONSTRAINT FK_WFE_StepI_Instance   FOREIGN KEY (InstanceID) REFERENCES WFE.t_WorkflowInstance (InstanceID),
    CONSTRAINT FK_WFE_StepI_Step       FOREIGN KEY (StepID) REFERENCES WFE.m_WorkflowStep (StepID)
);
GO
CREATE NONCLUSTERED INDEX IX_WFE_StepI_Assign
    ON WFE.t_WorkflowStepInstance (AssignedUserID, Status)
    INCLUDE (InstanceID, StepID, DueDate, IsEscalated);
GO
CREATE NONCLUSTERED INDEX IX_WFE_StepI_Due
    ON WFE.t_WorkflowStepInstance (DueDate, Status)
    WHERE Status IN (1, 2);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: WFE.s_SystemCalendar
  คำอธิบาย: ปฏิทินวันทำงาน (สำหรับคำนวณ SLA ที่ไม่นับวันหยุด)
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('WFE.s_SystemCalendar', 'U') IS NOT NULL DROP TABLE WFE.s_SystemCalendar;
GO
CREATE TABLE WFE.s_SystemCalendar
(
    CalendarDate    DATE                NOT NULL,   -- วันที่
    IsWorkingDay    BIT                 NOT NULL CONSTRAINT DF_WFE_Cal_Work DEFAULT(1),
    IsHoliday       BIT                 NOT NULL CONSTRAINT DF_WFE_Cal_Hol  DEFAULT(0),
    HolidayName     NVARCHAR(100)       NULL,        -- ชื่อวันหยุด
    HolidayType     TINYINT             NULL,        -- 1=National, 2=Royal, 3=Company
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_WFE_Cal_Cdt  DEFAULT(GETDATE()),

    CONSTRAINT PK_WFE_SysCalendar  PRIMARY KEY CLUSTERED (CalendarDate ASC)
);
GO


/*
================================================================================
  TMS — Task Management System
================================================================================
*/

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: TMS.t_TaskItem
  คำอธิบาย: Task หลักของระบบ รองรับ Sub-task สูงสุด 3 ระดับ (BR-009)
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('TMS.t_TaskItem', 'U') IS NOT NULL DROP TABLE TMS.t_TaskItem;
GO
CREATE TABLE TMS.t_TaskItem
(
    TaskID                  INT IDENTITY(1,1)   NOT NULL,
    TaskCode                VARCHAR(30)         NOT NULL,   -- รหัส Task เช่น TASK-2566-00001
    Title                   NVARCHAR(500)       NOT NULL,   -- ชื่อ Task
    DescriptionHTML         NVARCHAR(MAX)       NULL,
    Priority                TINYINT             NOT NULL CONSTRAINT DF_TMS_Task_Pri  DEFAULT(2), -- 1=Critical, 2=High, 3=Medium, 4=Low
    Status                  TINYINT             NOT NULL CONSTRAINT DF_TMS_Task_Stat DEFAULT(1), -- 1=Backlog, 2=Todo, 3=InProgress, 4=Done, 5=Cancelled
    ParentTaskID            INT                 NULL,       -- FK → Self (Sub-task)
    SubtaskDepth            TINYINT             NOT NULL CONSTRAINT DF_TMS_Task_Dep  DEFAULT(0), -- 0=Root, Max=3
    CompletionPct           DECIMAL(5,2)        NOT NULL CONSTRAINT DF_TMS_Task_Pct  DEFAULT(0),
    AssignedToUserID        INT                 NOT NULL,
    AssignedToRoleID        INT                 NULL,
    AssignedToOrgUnitID     INT                 NULL,
    CreatedByUserID         INT                 NOT NULL,
    DueDate                 DATETIME            NULL,
    StartDate               DATETIME            NULL,
    EstimatedHours          DECIMAL(8,2)        NULL,
    ActualHours             DECIMAL(8,2)        NULL,
    WorkflowStepInstanceID  INT                 NULL,       -- FK → WFE.t_WorkflowStepInstance (ถ้า Task มาจาก WF)
    ProjectID               INT                 NULL,       -- FK → PMS.t_Project (ถ้า Task สังกัด Project)
    Tags                    NVARCHAR(500)       NULL,
    Category                NVARCHAR(100)       NULL,       -- หมวดหมู่ Task
    IsDeleted               BIT                 NOT NULL CONSTRAINT DF_TMS_Task_Del  DEFAULT(0),
    RecycleBinExpiry        DATETIME            NULL,
    CreatedBy               NVARCHAR(100)       NOT NULL,
    CreatedDate             DATETIME            NOT NULL CONSTRAINT DF_TMS_Task_Cdt  DEFAULT(GETDATE()),
    UpdatedBy               NVARCHAR(100)       NULL,
    UpdatedDate             DATETIME            NULL,
    CompletedBy             NVARCHAR(100)       NULL,
    CompletedDate           DATETIME            NULL,

    CONSTRAINT PK_TMS_TaskItem         PRIMARY KEY CLUSTERED (TaskID ASC),
    CONSTRAINT UQ_TMS_Task_Code        UNIQUE (TaskCode),
    CONSTRAINT CK_TMS_Task_Priority    CHECK (Priority IN (1, 2, 3, 4)),
    CONSTRAINT CK_TMS_Task_Status      CHECK (Status IN (1, 2, 3, 4, 5)),
    CONSTRAINT CK_TMS_Task_Depth       CHECK (SubtaskDepth BETWEEN 0 AND 3),
    CONSTRAINT FK_TMS_Task_Parent      FOREIGN KEY (ParentTaskID) REFERENCES TMS.t_TaskItem (TaskID)
);
GO
CREATE NONCLUSTERED INDEX IX_TMS_Task_Assign
    ON TMS.t_TaskItem (AssignedToUserID, Status, IsDeleted)
    INCLUDE (TaskCode, Title, Priority, DueDate, CompletionPct);
GO
CREATE NONCLUSTERED INDEX IX_TMS_Task_Due
    ON TMS.t_TaskItem (DueDate, Status)
    INCLUDE (TaskID, AssignedToUserID, Priority, Title)
    WHERE IsDeleted = 0 AND Status < 4;
GO
CREATE NONCLUSTERED INDEX IX_TMS_Task_Project
    ON TMS.t_TaskItem (ProjectID, Status, IsDeleted)
    WHERE ProjectID IS NOT NULL;
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: TMS.t_TaskComment
  คำอธิบาย: Comment ใน Task รองรับ Thread ตอบกลับ
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('TMS.t_TaskComment', 'U') IS NOT NULL DROP TABLE TMS.t_TaskComment;
GO
CREATE TABLE TMS.t_TaskComment
(
    CommentID       INT IDENTITY(1,1)   NOT NULL,
    TaskID          INT                 NOT NULL,   -- FK → TMS.t_TaskItem
    ParentCommentID INT                 NULL,       -- FK → Self (Thread Reply)
    ContentHTML     NVARCHAR(MAX)       NOT NULL,
    ThreadDepth     TINYINT             NOT NULL CONSTRAINT DF_TMS_Cmt_Dep DEFAULT(0),
    IsEdited        BIT                 NOT NULL CONSTRAINT DF_TMS_Cmt_Ed  DEFAULT(0),
    IsDeleted       BIT                 NOT NULL CONSTRAINT DF_TMS_Cmt_Del DEFAULT(0),
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_TMS_Cmt_Cdt DEFAULT(GETDATE()),
    EditedDate      DATETIME            NULL,

    CONSTRAINT PK_TMS_TaskComment      PRIMARY KEY CLUSTERED (CommentID ASC),
    CONSTRAINT FK_TMS_Cmt_Task        FOREIGN KEY (TaskID) REFERENCES TMS.t_TaskItem (TaskID),
    CONSTRAINT FK_TMS_Cmt_Parent      FOREIGN KEY (ParentCommentID) REFERENCES TMS.t_TaskComment (CommentID)
);
GO
CREATE NONCLUSTERED INDEX IX_TMS_Cmt_Task
    ON TMS.t_TaskComment (TaskID, CreatedDate)
    INCLUDE (CommentID, ParentCommentID, CreatedBy, IsEdited, IsDeleted);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: TMS.t_TaskAttachment
  คำอธิบาย: ไฟล์แนบใน Task (Link ไปยัง DMS หรือ Upload แยก)
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('TMS.t_TaskAttachment', 'U') IS NOT NULL DROP TABLE TMS.t_TaskAttachment;
GO
CREATE TABLE TMS.t_TaskAttachment
(
    AttachmentID    INT IDENTITY(1,1)   NOT NULL,
    TaskID          INT                 NOT NULL,
    AttachType      TINYINT             NOT NULL,   -- 1=DMSFile (Link), 2=DirectUpload
    DMSFileID       INT                 NULL,        -- FK → DMS.t_FileDocument (ถ้า Type=1)
    FileName        NVARCHAR(300)       NULL,        -- ชื่อไฟล์ (ถ้า Type=2)
    FilePath        NVARCHAR(500)       NULL,        -- Path (ถ้า Type=2)
    FileSizeBytes   BIGINT              NULL,
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_TMS_Att_Cdt DEFAULT(GETDATE()),

    CONSTRAINT PK_TMS_TaskAttach       PRIMARY KEY CLUSTERED (AttachmentID ASC),
    CONSTRAINT CK_TMS_Att_Type         CHECK (AttachType IN (1, 2)),
    CONSTRAINT FK_TMS_Att_Task         FOREIGN KEY (TaskID) REFERENCES TMS.t_TaskItem (TaskID)
);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: TMS.t_TodoItem
  คำอธิบาย: Todo List ส่วนตัวและส่วนรวมของทีม (MOD-010)
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('TMS.t_TodoItem', 'U') IS NOT NULL DROP TABLE TMS.t_TodoItem;
GO
CREATE TABLE TMS.t_TodoItem
(
    TodoID          INT IDENTITY(1,1)   NOT NULL,
    Title           NVARCHAR(500)       NOT NULL,
    ListType        TINYINT             NOT NULL CONSTRAINT DF_TMS_Todo_LT   DEFAULT(1), -- 1=Personal, 2=Team, 3=Project
    OwnedByUserID   INT                 NOT NULL,
    SharedOrgUnitID INT                 NULL,       -- ถ้า ListType=2 แชร์กับทีม
    LinkedProjectID INT                 NULL,
    LinkedTaskID    INT                 NULL,       -- FK → TMS.t_TaskItem
    Priority        TINYINT             NOT NULL CONSTRAINT DF_TMS_Todo_Pri  DEFAULT(2),
    IsCompleted     BIT                 NOT NULL CONSTRAINT DF_TMS_Todo_Done DEFAULT(0),
    DueDate         DATETIME            NULL,
    Category        NVARCHAR(100)       NULL,
    SortOrder       INT                 NOT NULL CONSTRAINT DF_TMS_Todo_Sort DEFAULT(0),
    IsRecurring     BIT                 NOT NULL CONSTRAINT DF_TMS_Todo_Rec  DEFAULT(0),
    RecurringType   TINYINT             NULL,       -- 1=Daily, 2=Weekly, 3=Monthly
    RecurringEndDate DATETIME           NULL,
    IsDeleted       BIT                 NOT NULL CONSTRAINT DF_TMS_Todo_Del  DEFAULT(0),
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_TMS_Todo_Cdt  DEFAULT(GETDATE()),
    CompletedDate   DATETIME            NULL,

    CONSTRAINT PK_TMS_TodoItem         PRIMARY KEY CLUSTERED (TodoID ASC),
    CONSTRAINT CK_TMS_Todo_Type        CHECK (ListType IN (1, 2, 3)),
    CONSTRAINT CK_TMS_Todo_Priority    CHECK (Priority IN (1, 2, 3, 4)),
    CONSTRAINT FK_TMS_Todo_Task        FOREIGN KEY (LinkedTaskID) REFERENCES TMS.t_TaskItem (TaskID)
);
GO
CREATE NONCLUSTERED INDEX IX_TMS_Todo_Owner
    ON TMS.t_TodoItem (OwnedByUserID, IsCompleted, IsDeleted)
    INCLUDE (Title, Priority, DueDate, SortOrder);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: TMS.t_Note
  คำอธิบาย: Notes และ Comments ทั่วไป (MOD-011) เชื่อมได้หลาย Entity
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('TMS.t_Note', 'U') IS NOT NULL DROP TABLE TMS.t_Note;
GO
CREATE TABLE TMS.t_Note
(
    NoteID          INT IDENTITY(1,1)   NOT NULL,
    Title           NVARCHAR(300)       NULL,
    ContentHTML     NVARCHAR(MAX)       NOT NULL,
    NoteType        TINYINT             NOT NULL,   -- 1=General, 2=Meeting, 3=Decision, 4=Risk
    EntityType      TINYINT             NULL,       -- 1=Task, 2=Project, 3=File, 4=WorkflowInstance
    EntityID        INT                 NULL,
    IsPinned        BIT                 NOT NULL CONSTRAINT DF_TMS_Note_Pin DEFAULT(0),
    IsPrivate       BIT                 NOT NULL CONSTRAINT DF_TMS_Note_Prv DEFAULT(0),  -- ถ้า Private เห็นแค่เจ้าของ
    OwnedByUserID   INT                 NOT NULL,
    Tags            NVARCHAR(300)       NULL,
    IsDeleted       BIT                 NOT NULL CONSTRAINT DF_TMS_Note_Del DEFAULT(0),
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_TMS_Note_Cdt DEFAULT(GETDATE()),
    UpdatedBy       NVARCHAR(100)       NULL,
    UpdatedDate     DATETIME            NULL,

    CONSTRAINT PK_TMS_Note             PRIMARY KEY CLUSTERED (NoteID ASC),
    CONSTRAINT CK_TMS_Note_Type        CHECK (NoteType IN (1, 2, 3, 4))
);
GO
CREATE NONCLUSTERED INDEX IX_TMS_Note_Entity
    ON TMS.t_Note (EntityType, EntityID, IsDeleted)
    INCLUDE (NoteID, Title, NoteType, CreatedBy, CreatedDate);
GO

/*─────────────────────────────────────────────────────────────────────────────
  TABLE: TMS.t_WorkInstruction
  คำอธิบาย: คู่มือ/วิธีปฏิบัติงาน (MOD-012) เชื่อมกับ Task และ KB
─────────────────────────────────────────────────────────────────────────────*/
IF OBJECT_ID('TMS.t_WorkInstruction', 'U') IS NOT NULL DROP TABLE TMS.t_WorkInstruction;
GO
CREATE TABLE TMS.t_WorkInstruction
(
    WIID            INT IDENTITY(1,1)   NOT NULL,
    WICode          VARCHAR(20)         NOT NULL,   -- รหัส WI เช่น WI-2566-001
    Title           NVARCHAR(500)       NOT NULL,
    ContentHTML     NVARCHAR(MAX)       NOT NULL,
    WIType          TINYINT             NOT NULL,   -- 1=Process, 2=Procedure, 3=Work Instruction
    OrgUnitID       INT                 NULL,
    RelatedTaskTypeID INT               NULL,
    Status          TINYINT             NOT NULL CONSTRAINT DF_TMS_WI_Stat DEFAULT(1), -- 1=Draft, 2=Active, 3=Obsolete
    Version         INT                 NOT NULL CONSTRAINT DF_TMS_WI_Ver  DEFAULT(1),
    EffectiveDate   DATE                NULL,
    ReviewDueDate   DATE                NULL,
    Tags            NVARCHAR(300)       NULL,
    CreatedBy       NVARCHAR(100)       NOT NULL,
    CreatedDate     DATETIME            NOT NULL CONSTRAINT DF_TMS_WI_Cdt  DEFAULT(GETDATE()),
    ApprovedBy      NVARCHAR(100)       NULL,
    ApprovedDate    DATETIME            NULL,

    CONSTRAINT PK_TMS_WorkInstruction  PRIMARY KEY CLUSTERED (WIID ASC),
    CONSTRAINT UQ_TMS_WI_Code         UNIQUE (WICode),
    CONSTRAINT CK_TMS_WI_Type         CHECK (WIType IN (1, 2, 3)),
    CONSTRAINT CK_TMS_WI_Status       CHECK (Status IN (1, 2, 3))
);
GO

PRINT '✅ KMS, WFE, TMS Schemas — สร้าง Table ทั้งหมดเสร็จสิ้น (22 Tables)';
GO
