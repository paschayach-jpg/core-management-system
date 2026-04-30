-- ============================================================
-- Workflow Engine Tables
-- Creates all tables needed for workflow definition, execution, and tracking
-- ============================================================

-- Create dbo.WorkflowDefinition table (Workflow Templates)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.WorkflowDefinition') AND type = N'U')
BEGIN
    CREATE TABLE dbo.WorkflowDefinition (
        WorkflowDefinitionId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Version INT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy NVARCHAR(255) NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ModifiedBy NVARCHAR(255) NULL,
        ModifiedDate DATETIME NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedDate DATETIME NULL
    )

    CREATE NONCLUSTERED INDEX IX_WorkflowDefinition_Name ON dbo.WorkflowDefinition(Name)
    CREATE NONCLUSTERED INDEX IX_WorkflowDefinition_IsActive ON dbo.WorkflowDefinition(IsActive)
    CREATE NONCLUSTERED INDEX IX_WorkflowDefinition_IsDeleted ON dbo.WorkflowDefinition(IsDeleted)
    PRINT 'Created table: dbo.WorkflowDefinition'
END
GO

-- Create dbo.WorkflowStep table (Steps within workflow templates)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.WorkflowStep') AND type = N'U')
BEGIN
    CREATE TABLE dbo.WorkflowStep (
        WorkflowStepId INT IDENTITY(1,1) PRIMARY KEY,
        WorkflowDefinitionId INT NOT NULL REFERENCES dbo.WorkflowDefinition(WorkflowDefinitionId),
        StepName NVARCHAR(200) NOT NULL,
        StepType TINYINT NOT NULL DEFAULT 0,           -- 0=Standard, 1=Parallel, 2=Decision, etc.
        Description NVARCHAR(MAX) NULL,
        AssignedRole NVARCHAR(255) NULL,               -- Role or user assignment
        AssignedUser NVARCHAR(255) NULL,
        AssignedOrgUnit NVARCHAR(255) NULL,
        SLAHours INT NULL,                             -- Service level agreement in hours
        SLADays INT NULL,                              -- Service level agreement in days
        AllowDelegation BIT NOT NULL DEFAULT 0,        -- Can step be delegated
        MaxDelegationDepth INT NOT NULL DEFAULT 1,     -- Max delegation chain depth
        SequenceNumber INT NOT NULL,                   -- Order of step in workflow
        IsDeleted BIT NOT NULL DEFAULT 0
    )

    CREATE NONCLUSTERED INDEX IX_WorkflowStep_WorkflowID ON dbo.WorkflowStep(WorkflowDefinitionId)
    CREATE NONCLUSTERED INDEX IX_WorkflowStep_Sequence ON dbo.WorkflowStep(WorkflowDefinitionId, SequenceNumber)
    CREATE NONCLUSTERED INDEX IX_WorkflowStep_IsDeleted ON dbo.WorkflowStep(IsDeleted)
    PRINT 'Created table: dbo.WorkflowStep'
END
GO

-- Create dbo.WorkflowConnection table (Connections between steps)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.WorkflowConnection') AND type = N'U')
BEGIN
    CREATE TABLE dbo.WorkflowConnection (
        WorkflowConnectionId INT IDENTITY(1,1) PRIMARY KEY,
        WorkflowDefinitionId INT NOT NULL REFERENCES dbo.WorkflowDefinition(WorkflowDefinitionId),
        FromStepId INT NOT NULL REFERENCES dbo.WorkflowStep(WorkflowStepId),
        ToStepId INT NOT NULL REFERENCES dbo.WorkflowStep(WorkflowStepId),
        ConditionExpression NVARCHAR(MAX) NULL,       -- Condition for routing (JSON or expression)
        SequenceNumber INT NOT NULL,                  -- Order of connections from a step
        IsDeleted BIT NOT NULL DEFAULT 0
    )

    CREATE NONCLUSTERED INDEX IX_WorkflowConnection_WorkflowID ON dbo.WorkflowConnection(WorkflowDefinitionId)
    CREATE NONCLUSTERED INDEX IX_WorkflowConnection_FromStep ON dbo.WorkflowConnection(FromStepId)
    CREATE NONCLUSTERED INDEX IX_WorkflowConnection_ToStep ON dbo.WorkflowConnection(ToStepId)
    CREATE NONCLUSTERED INDEX IX_WorkflowConnection_IsDeleted ON dbo.WorkflowConnection(IsDeleted)
    PRINT 'Created table: dbo.WorkflowConnection'
END
GO

-- Create dbo.WorkflowInstance table (Running workflow instances)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.WorkflowInstance') AND type = N'U')
BEGIN
    CREATE TABLE dbo.WorkflowInstance (
        WorkflowInstanceId INT IDENTITY(1,1) PRIMARY KEY,
        WorkflowDefinitionId INT NOT NULL REFERENCES dbo.WorkflowDefinition(WorkflowDefinitionId),
        TemplateVersion INT NOT NULL,                 -- Version of template used
        Status TINYINT NOT NULL DEFAULT 0,            -- 0=Draft, 1=InProgress, 2=OnHold, 3=Completed, 4=Rejected, 5=Cancelled
        InitiatedBy NVARCHAR(255) NOT NULL,           -- User who initiated this workflow instance
        InitiatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CurrentStepId INT NULL REFERENCES dbo.WorkflowStep(WorkflowStepId),  -- Current step in workflow
        CompletedDate DATETIME NULL,
        InputParameters NVARCHAR(MAX) NULL,           -- JSON with input parameters
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedDate DATETIME NULL
    )

    CREATE NONCLUSTERED INDEX IX_WorkflowInstance_DefinitionID ON dbo.WorkflowInstance(WorkflowDefinitionId)
    CREATE NONCLUSTERED INDEX IX_WorkflowInstance_Status ON dbo.WorkflowInstance(Status)
    CREATE NONCLUSTERED INDEX IX_WorkflowInstance_IsDeleted ON dbo.WorkflowInstance(IsDeleted)
    CREATE NONCLUSTERED INDEX IX_WorkflowInstance_CurrentStep ON dbo.WorkflowInstance(CurrentStepId)
    PRINT 'Created table: dbo.WorkflowInstance'
END
GO

-- Create stored procedure for creating workflow instances
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.sp_WF_CreateWorkflowInstance') AND type = N'P')
BEGIN
    EXEC sp_executesql N'
        CREATE PROCEDURE dbo.sp_WF_CreateWorkflowInstance
            @WorkflowDefinitionId INT,
            @TemplateVersion INT,
            @InitiatedBy NVARCHAR(255),
            @InputParameters NVARCHAR(MAX) = NULL,
            @NewWorkflowInstanceId INT OUTPUT
        AS
        BEGIN
            SET NOCOUNT ON
            INSERT INTO dbo.WorkflowInstance
                (WorkflowDefinitionId, TemplateVersion, Status, InitiatedBy, InitiatedDate, InputParameters, IsDeleted)
            VALUES
                (@WorkflowDefinitionId, @TemplateVersion, 0, @InitiatedBy, GETUTCDATE(), @InputParameters, 0)

            SET @NewWorkflowInstanceId = SCOPE_IDENTITY()
        END
    '
    PRINT 'Created stored procedure: dbo.sp_WF_CreateWorkflowInstance'
END
GO

-- Create stored procedure for updating workflow instance status
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.sp_WF_UpdateWorkflowInstanceStatus') AND type = N'P')
BEGIN
    EXEC sp_executesql N'
        CREATE PROCEDURE dbo.sp_WF_UpdateWorkflowInstanceStatus
            @WorkflowInstanceId INT,
            @Status TINYINT,
            @CurrentStepId INT = NULL,
            @CompletedDate DATETIME = NULL
        AS
        BEGIN
            SET NOCOUNT ON
            UPDATE dbo.WorkflowInstance
            SET Status = @Status,
                CurrentStepId = @CurrentStepId,
                CompletedDate = @CompletedDate
            WHERE WorkflowInstanceId = @WorkflowInstanceId
        END
    '
    PRINT 'Created stored procedure: dbo.sp_WF_UpdateWorkflowInstanceStatus'
END
GO

PRINT 'All Workflow Engine tables and stored procedures created successfully'
GO
