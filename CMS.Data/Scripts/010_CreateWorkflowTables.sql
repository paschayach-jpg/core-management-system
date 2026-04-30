-- ============================================================
-- Workflow Engine (WFE) Schema & Tables
-- Creates all tables needed for workflow definition, execution, and tracking
-- ============================================================

-- Create WFE Schema if not exists
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'WFE')
BEGIN
    EXEC('CREATE SCHEMA WFE')
    PRINT 'Created schema: WFE'
END
GO

-- Drop old dbo tables if they exist (migration from incorrect schema)
IF OBJECT_ID(N'dbo.WorkflowApprover', N'U') IS NOT NULL DROP TABLE dbo.WorkflowApprover
IF OBJECT_ID(N'dbo.WorkflowConnection', N'U') IS NOT NULL DROP TABLE dbo.WorkflowConnection
IF OBJECT_ID(N'dbo.WorkflowInstance', N'U') IS NOT NULL DROP TABLE dbo.WorkflowInstance
IF OBJECT_ID(N'dbo.WorkflowStep', N'U') IS NOT NULL DROP TABLE dbo.WorkflowStep
IF OBJECT_ID(N'dbo.WorkflowDefinition', N'U') IS NOT NULL DROP TABLE dbo.WorkflowDefinition
PRINT 'Dropped old dbo workflow tables'
GO

-- Create WFE.s_WFDefinition table (Workflow Templates - Setup)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'WFE.s_WFDefinition') AND type = N'U')
BEGIN
    CREATE TABLE WFE.s_WFDefinition (
        WFDefinitionId INT IDENTITY(1,1) PRIMARY KEY,
        WFName NVARCHAR(200) NOT NULL,
        WFDescription NVARCHAR(MAX) NULL,
        Version INT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy NVARCHAR(255) NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ModifiedBy NVARCHAR(255) NULL,
        ModifiedDate DATETIME NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedDate DATETIME NULL
    )

    CREATE NONCLUSTERED INDEX IX_WFDefinition_Name ON WFE.s_WFDefinition(WFName)
    CREATE NONCLUSTERED INDEX IX_WFDefinition_IsActive ON WFE.s_WFDefinition(IsActive)
    CREATE NONCLUSTERED INDEX IX_WFDefinition_IsDeleted ON WFE.s_WFDefinition(IsDeleted)
    PRINT 'Created table: WFE.s_WFDefinition'
END
GO

-- Create WFE.s_WFStep table (Steps within workflow templates - Setup)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'WFE.s_WFStep') AND type = N'U')
BEGIN
    CREATE TABLE WFE.s_WFStep (
        WFStepId INT IDENTITY(1,1) PRIMARY KEY,
        WFDefinitionId INT NOT NULL REFERENCES WFE.s_WFDefinition(WFDefinitionId),
        StepName NVARCHAR(200) NOT NULL,
        StepType TINYINT NOT NULL DEFAULT 0,
        Description NVARCHAR(MAX) NULL,
        AssignedRole NVARCHAR(255) NULL,
        AssignedUser NVARCHAR(255) NULL,
        AssignedOrgUnit NVARCHAR(255) NULL,
        SLAHours INT NULL,
        SLADays INT NULL,
        AllowDelegation BIT NOT NULL DEFAULT 0,
        MaxDelegationDepth INT NOT NULL DEFAULT 1,
        SequenceNumber INT NOT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    )

    CREATE NONCLUSTERED INDEX IX_WFStep_DefinitionID ON WFE.s_WFStep(WFDefinitionId)
    CREATE NONCLUSTERED INDEX IX_WFStep_Sequence ON WFE.s_WFStep(WFDefinitionId, SequenceNumber)
    CREATE NONCLUSTERED INDEX IX_WFStep_IsDeleted ON WFE.s_WFStep(IsDeleted)
    PRINT 'Created table: WFE.s_WFStep'
END
GO

-- Create WFE.s_WFConnection table (Connections between steps - Setup)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'WFE.s_WFConnection') AND type = N'U')
BEGIN
    CREATE TABLE WFE.s_WFConnection (
        WFConnectionId INT IDENTITY(1,1) PRIMARY KEY,
        WFDefinitionId INT NOT NULL REFERENCES WFE.s_WFDefinition(WFDefinitionId),
        FromStepId INT NOT NULL REFERENCES WFE.s_WFStep(WFStepId),
        ToStepId INT NOT NULL REFERENCES WFE.s_WFStep(WFStepId),
        ConditionExpression NVARCHAR(MAX) NULL,
        SequenceNumber INT NOT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    )

    CREATE NONCLUSTERED INDEX IX_WFConnection_DefinitionID ON WFE.s_WFConnection(WFDefinitionId)
    CREATE NONCLUSTERED INDEX IX_WFConnection_FromStep ON WFE.s_WFConnection(FromStepId)
    CREATE NONCLUSTERED INDEX IX_WFConnection_ToStep ON WFE.s_WFConnection(ToStepId)
    CREATE NONCLUSTERED INDEX IX_WFConnection_IsDeleted ON WFE.s_WFConnection(IsDeleted)
    PRINT 'Created table: WFE.s_WFConnection'
END
GO

-- Create WFE.t_WFInstance table (Running workflow instances - Transaction)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'WFE.t_WFInstance') AND type = N'U')
BEGIN
    CREATE TABLE WFE.t_WFInstance (
        WFInstanceId INT IDENTITY(1,1) PRIMARY KEY,
        WFDefinitionId INT NOT NULL REFERENCES WFE.s_WFDefinition(WFDefinitionId),
        TemplateVersion INT NOT NULL,
        Status TINYINT NOT NULL DEFAULT 0,
        InitiatedBy NVARCHAR(255) NOT NULL,
        InitiatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CurrentStepId INT NULL REFERENCES WFE.s_WFStep(WFStepId),
        CompletedDate DATETIME NULL,
        InputParameters NVARCHAR(MAX) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedDate DATETIME NULL
    )

    CREATE NONCLUSTERED INDEX IX_WFInstance_DefinitionID ON WFE.t_WFInstance(WFDefinitionId)
    CREATE NONCLUSTERED INDEX IX_WFInstance_Status ON WFE.t_WFInstance(Status)
    CREATE NONCLUSTERED INDEX IX_WFInstance_IsDeleted ON WFE.t_WFInstance(IsDeleted)
    CREATE NONCLUSTERED INDEX IX_WFInstance_CurrentStep ON WFE.t_WFInstance(CurrentStepId)
    PRINT 'Created table: WFE.t_WFInstance'
END
GO

-- Create WFE.t_WFStepInstance table (Step execution tracking - Transaction)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'WFE.t_WFStepInstance') AND type = N'U')
BEGIN
    CREATE TABLE WFE.t_WFStepInstance (
        WFStepInstanceId INT IDENTITY(1,1) PRIMARY KEY,
        WFInstanceId INT NOT NULL REFERENCES WFE.t_WFInstance(WFInstanceId),
        WFStepId INT NOT NULL REFERENCES WFE.s_WFStep(WFStepId),
        Status TINYINT NOT NULL DEFAULT 0,
        AssignedTo NVARCHAR(255) NULL,
        StartedDate DATETIME NULL,
        CompletedDate DATETIME NULL,
        Remarks NVARCHAR(MAX) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    )

    CREATE NONCLUSTERED INDEX IX_WFStepInstance_InstanceID ON WFE.t_WFStepInstance(WFInstanceId)
    CREATE NONCLUSTERED INDEX IX_WFStepInstance_StepID ON WFE.t_WFStepInstance(WFStepId)
    CREATE NONCLUSTERED INDEX IX_WFStepInstance_Status ON WFE.t_WFStepInstance(Status)
    PRINT 'Created table: WFE.t_WFStepInstance'
END
GO

-- Create WFE.l_WFEAuditEvent table (Audit logging - Log)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'WFE.l_WFEAuditEvent') AND type = N'U')
BEGIN
    CREATE TABLE WFE.l_WFEAuditEvent (
        AuditEventId BIGINT IDENTITY(1,1) PRIMARY KEY,
        WFInstanceId INT NULL REFERENCES WFE.t_WFInstance(WFInstanceId),
        EventType NVARCHAR(50) NOT NULL,
        EventDetails NVARCHAR(MAX) NULL,
        PerformedBy NVARCHAR(255) NOT NULL,
        PerformedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        IPAddress NVARCHAR(50) NULL
    )

    CREATE NONCLUSTERED INDEX IX_WFEAuditEvent_InstanceID ON WFE.l_WFEAuditEvent(WFInstanceId)
    CREATE NONCLUSTERED INDEX IX_WFEAuditEvent_PerformedDate ON WFE.l_WFEAuditEvent(PerformedDate)
    CREATE NONCLUSTERED INDEX IX_WFEAuditEvent_EventType ON WFE.l_WFEAuditEvent(EventType)
    PRINT 'Created table: WFE.l_WFEAuditEvent'
END
GO

-- Drop old stored procedures if they exist (with errors)
IF OBJECT_ID(N'WFE.sp_CreateWFInstance', N'P') IS NOT NULL DROP PROCEDURE WFE.sp_CreateWFInstance
IF OBJECT_ID(N'WFE.sp_UpdateWFInstanceStatus', N'P') IS NOT NULL DROP PROCEDURE WFE.sp_UpdateWFInstanceStatus
PRINT 'Dropped old stored procedures'
GO

-- Create stored procedure for creating workflow instances
CREATE PROCEDURE WFE.sp_CreateWFInstance
    @WFDefinitionId INT,
    @TemplateVersion INT,
    @InitiatedBy NVARCHAR(255),
    @InputParameters NVARCHAR(MAX) = NULL,
    @NewWFInstanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON
    INSERT INTO WFE.t_WFInstance (WFDefinitionId, TemplateVersion, Status, InitiatedBy, InitiatedDate, InputParameters, IsDeleted)
    VALUES (@WFDefinitionId, @TemplateVersion, 0, @InitiatedBy, GETUTCDATE(), @InputParameters, 0)

    SET @NewWFInstanceId = SCOPE_IDENTITY()
END
GO
PRINT 'Created stored procedure: WFE.sp_CreateWFInstance'
GO

-- Create stored procedure for updating workflow instance status
CREATE PROCEDURE WFE.sp_UpdateWFInstanceStatus
    @WFInstanceId INT,
    @Status TINYINT,
    @CurrentStepId INT = NULL,
    @CompletedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON
    UPDATE WFE.t_WFInstance
    SET Status = @Status, CurrentStepId = @CurrentStepId, CompletedDate = @CompletedDate
    WHERE WFInstanceId = @WFInstanceId
END
GO
PRINT 'Created stored procedure: WFE.sp_UpdateWFInstanceStatus'
GO

-- Create stored procedure for creating step instances
CREATE PROCEDURE WFE.sp_CreateWFStepInstance
    @WFInstanceId INT,
    @WFStepId INT,
    @AssignedTo NVARCHAR(255) = NULL,
    @NewWFStepInstanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON
    INSERT INTO WFE.t_WFStepInstance (WFInstanceId, WFStepId, Status, AssignedTo, IsDeleted)
    VALUES (@WFInstanceId, @WFStepId, 0, @AssignedTo, 0)

    SET @NewWFStepInstanceId = SCOPE_IDENTITY()
END
GO
PRINT 'Created stored procedure: WFE.sp_CreateWFStepInstance'
GO

PRINT 'All Workflow Engine (WFE) tables and stored procedures created successfully'
GO
