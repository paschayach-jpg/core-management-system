-- ============================================================================
-- Script: 010_CreateWorkflowTables.sql
-- Purpose: Create Workflow Definition and related tables
-- ============================================================================

-- Create dbo.WorkflowDefinition table (Workflow Template Definitions)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.WorkflowDefinition') AND type = N'U')
BEGIN
    CREATE TABLE dbo.WorkflowDefinition
    (
        WorkflowDefinitionId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS,
        Description NVARCHAR(MAX) NULL COLLATE Thai_CI_AS,
        TemplateDefinition NVARCHAR(MAX) NULL,
        Version INT DEFAULT 1,
        IsActive BIT DEFAULT 1,
        CreatedBy NVARCHAR(100) NOT NULL COLLATE Thai_CI_AS,
        CreatedDate DATETIME DEFAULT GETDATE(),
        ModifiedBy NVARCHAR(100) NULL COLLATE Thai_CI_AS,
        ModifiedDate DATETIME NULL,
        IsDeleted BIT DEFAULT 0,
        DeletedDate DATETIME NULL
    )

    CREATE NONCLUSTERED INDEX IX_WorkflowDefinition_Name ON dbo.WorkflowDefinition(Name)
    CREATE NONCLUSTERED INDEX IX_WorkflowDefinition_IsActive ON dbo.WorkflowDefinition(IsActive)
    CREATE NONCLUSTERED INDEX IX_WorkflowDefinition_IsDeleted ON dbo.WorkflowDefinition(IsDeleted)

    PRINT 'Created table: dbo.WorkflowDefinition'
END
ELSE
BEGIN
    PRINT 'Table dbo.WorkflowDefinition already exists'
END
GO

-- Create dbo.WorkflowStep table (Define steps within a workflow)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkflowStep]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WorkflowStep]
    (
        [WorkflowStepId]         INT PRIMARY KEY IDENTITY(1,1),
        [WorkflowDefinitionId]   INT NOT NULL,
        [StepNumber]             INT NOT NULL,
        [StepName]               NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS,
        [Description]            NVARCHAR(MAX) NULL COLLATE Thai_CI_AS,
        [Action]                 NVARCHAR(200) NULL COLLATE Thai_CI_AS,
        [NextStep]               NVARCHAR(200) NULL COLLATE Thai_CI_AS,
        [IsParallelApproval]     BIT DEFAULT 0,
        [TimeoutDays]            INT NULL,
        [CreatedDate]            DATETIME DEFAULT GETUTCDATE(),

        CONSTRAINT [FK_WorkflowStep_WorkflowDefinition]
            FOREIGN KEY ([WorkflowDefinitionId])
            REFERENCES [dbo].[WorkflowDefinition]([WorkflowDefinitionId])
            ON DELETE CASCADE
    )

    CREATE NONCLUSTERED INDEX [IX_WorkflowStep_WorkflowID] ON [dbo].[WorkflowStep]([WorkflowDefinitionId])

    PRINT 'Created table: dbo.WorkflowStep'
END
ELSE
BEGIN
    PRINT 'Table dbo.WorkflowStep already exists'
END
GO

-- Create dbo.WorkflowApprover table (Define approvers for each step)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkflowApprover]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WorkflowApprover]
    (
        [WorkflowApproverId]     INT PRIMARY KEY IDENTITY(1,1),
        [WorkflowStepId]         INT NOT NULL,
        [ApproverGroup]          NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS,
        [ApprovalLevel]          INT DEFAULT 1,
        [IsRequired]             BIT DEFAULT 1,
        [SequenceOrder]          INT DEFAULT 1,
        [CreatedDate]            DATETIME DEFAULT GETUTCDATE(),

        CONSTRAINT [FK_WorkflowApprover_WorkflowStep]
            FOREIGN KEY ([WorkflowStepId])
            REFERENCES [dbo].[WorkflowStep]([WorkflowStepId])
            ON DELETE CASCADE
    )

    CREATE NONCLUSTERED INDEX [IX_WorkflowApprover_StepID] ON [dbo].[WorkflowApprover]([WorkflowStepId])

    PRINT 'Created table: dbo.WorkflowApprover'
END
ELSE
BEGIN
    PRINT 'Table dbo.WorkflowApprover already exists'
END
GO

PRINT '✓ Workflow tables created successfully'
GO
