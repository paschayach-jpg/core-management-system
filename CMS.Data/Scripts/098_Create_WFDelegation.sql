/*
================================================================================
  Script  : 098_Create_WFDelegation.sql
  Module  : WFE — Workflow Engine
  Purpose : 1) Create dbo.WFDelegation table if it doesn't exist
            2) Create stored procedure sp_WF_CreateDelegation
            3) Create stored procedure sp_WF_GetDelegationHistory
  Run on  : CMS database (SQL Server 2019+, Collation Thai_CI_AS)
  Idempotent: YES — all blocks guarded with IF NOT EXISTS / IF OBJECT_ID
================================================================================
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Create table dbo.WFDelegation
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.WFDelegation') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.WFDelegation (
        WFDelegationId          INT IDENTITY(1,1) NOT NULL,
        WorkflowStepInstanceId  INT NOT NULL,
        DelegatedFrom           NVARCHAR(100) NOT NULL,
        DelegatedTo             NVARCHAR(100) NOT NULL,
        DelegationReason        NVARCHAR(500) NULL,
        DelegationDate          DATETIME NOT NULL DEFAULT (GETDATE()),
        DelegationDepth         INT NOT NULL,
        IsActive                BIT NOT NULL DEFAULT (1),
        IsDeleted               BIT NOT NULL DEFAULT (0),
        StartDate               DATETIME NULL,
        EndDate                 DATETIME NULL,
        Remarks                 NVARCHAR(500) NULL,
        CONSTRAINT PK_WFDelegation PRIMARY KEY CLUSTERED (WFDelegationId ASC)
    );
    PRINT 'Table dbo.WFDelegation created.';
END
ELSE
BEGIN
    -- Ensure columns exist if table already existed (e.g. from partial previous creation)
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.WFDelegation') AND name = 'StartDate')
    BEGIN
        ALTER TABLE dbo.WFDelegation ADD StartDate DATETIME NULL;
        PRINT 'Column StartDate added to dbo.WFDelegation.';
    END
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.WFDelegation') AND name = 'EndDate')
    BEGIN
        ALTER TABLE dbo.WFDelegation ADD EndDate DATETIME NULL;
        PRINT 'Column EndDate added to dbo.WFDelegation.';
    END
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.WFDelegation') AND name = 'Remarks')
    BEGIN
        ALTER TABLE dbo.WFDelegation ADD Remarks NVARCHAR(500) NULL;
        PRINT 'Column Remarks added to dbo.WFDelegation.';
    END
END
GO

-- 2. Create stored procedure dbo.sp_WF_CreateDelegation
IF OBJECT_ID('dbo.sp_WF_CreateDelegation', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_WF_CreateDelegation;
GO

CREATE PROCEDURE dbo.sp_WF_CreateDelegation
    @WorkflowStepInstanceId INT,
    @DelegatedFrom          NVARCHAR(100),
    @DelegatedTo            NVARCHAR(100),
    @DelegationReason       NVARCHAR(500) = NULL,
    @MaxDelegationDepth     INT = 3,
    @StartDate              DATETIME = NULL,
    @EndDate                DATETIME = NULL,
    @Remarks                NVARCHAR(500) = NULL,
    @NewWFDelegationId      INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Deactivate any previous active delegation for this step instance
    UPDATE dbo.WFDelegation
    SET    IsActive = 0
    WHERE  WorkflowStepInstanceId = @WorkflowStepInstanceId
      AND  IsActive = 1;

    -- Get current depth
    DECLARE @CurrentDepth INT = 0;
    SELECT @CurrentDepth = ISNULL(MAX(DelegationDepth), 0)
    FROM   dbo.WFDelegation
    WHERE  WorkflowStepInstanceId = @WorkflowStepInstanceId
      AND  IsDeleted = 0;

    DECLARE @NewDepth INT = @CurrentDepth + 1;

    IF @NewDepth > @MaxDelegationDepth
    BEGIN
        RAISERROR('Maximum delegation depth exceeded', 16, 1);
        RETURN;
    END

    -- Insert new delegation
    INSERT INTO dbo.WFDelegation (
        WorkflowStepInstanceId,
        DelegatedFrom,
        DelegatedTo,
        DelegationReason,
        DelegationDate,
        DelegationDepth,
        IsActive,
        IsDeleted,
        StartDate,
        EndDate,
        Remarks
    ) VALUES (
        @WorkflowStepInstanceId,
        @DelegatedFrom,
        @DelegatedTo,
        @DelegationReason,
        GETDATE(),
        @NewDepth,
        1,
        0,
        @StartDate,
        @EndDate,
        @Remarks
    );

    SET @NewWFDelegationId = SCOPE_IDENTITY();
END
GO

PRINT 'Stored procedure dbo.sp_WF_CreateDelegation created.';
GO

-- 3. Create stored procedure dbo.sp_WF_GetDelegationHistory
IF OBJECT_ID('dbo.sp_WF_GetDelegationHistory', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_WF_GetDelegationHistory;
GO

CREATE PROCEDURE dbo.sp_WF_GetDelegationHistory
    @WorkflowStepInstanceId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT WFDelegationId,
           WorkflowStepInstanceId,
           DelegatedFrom,
           DelegatedTo,
           DelegationReason,
           DelegationDate,
           DelegationDepth,
           IsActive,
           IsDeleted,
           StartDate,
           EndDate,
           Remarks
    FROM   dbo.WFDelegation
    WHERE  WorkflowStepInstanceId = @WorkflowStepInstanceId
      AND  IsDeleted = 0
    ORDER BY DelegationDepth ASC;
END
GO

PRINT 'Stored procedure dbo.sp_WF_GetDelegationHistory created.';
GO
