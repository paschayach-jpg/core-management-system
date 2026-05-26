/*
================================================================================
  Script  : 080_WFE_ApprovalLifecycle_And_SLAReport.sql
  Module  : WFE — Workflow Engine
  Purpose : 1) Add approval lifecycle columns to WFE.s_WFDefinition
            2) Create reporting stored procedures used by WFReportRepository
  Run on  : CMS database (SQL Server 2019+, Collation Thai_CI_AS)
  Idempotent: YES — all blocks guarded with IF NOT EXISTS / IF OBJECT_ID
================================================================================
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============================================================================
   PART 1 — Add approval lifecycle columns to WFE.s_WFDefinition
   ============================================================================ */

-- ApprovalStatus: 0=Draft, 1=PendingApproval, 2=Active, 3=Inactive, 4=Deprecated
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('WFE.s_WFDefinition') AND name = 'ApprovalStatus'
)
BEGIN
    ALTER TABLE WFE.s_WFDefinition
        ADD ApprovalStatus TINYINT NOT NULL CONSTRAINT DF_WFDef_ApprovalStatus DEFAULT (0);
    PRINT 'Column ApprovalStatus added.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('WFE.s_WFDefinition') AND name = 'SubmittedBy'
)
BEGIN
    ALTER TABLE WFE.s_WFDefinition ADD SubmittedBy NVARCHAR(100) NULL;
    PRINT 'Column SubmittedBy added.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('WFE.s_WFDefinition') AND name = 'SubmittedDate'
)
BEGIN
    ALTER TABLE WFE.s_WFDefinition ADD SubmittedDate DATETIME NULL;
    PRINT 'Column SubmittedDate added.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('WFE.s_WFDefinition') AND name = 'ApprovedBy'
)
BEGIN
    ALTER TABLE WFE.s_WFDefinition ADD ApprovedBy NVARCHAR(100) NULL;
    PRINT 'Column ApprovedBy added.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('WFE.s_WFDefinition') AND name = 'ApprovedDate'
)
BEGIN
    ALTER TABLE WFE.s_WFDefinition ADD ApprovedDate DATETIME NULL;
    PRINT 'Column ApprovedDate added.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('WFE.s_WFDefinition') AND name = 'RejectionReason'
)
BEGIN
    ALTER TABLE WFE.s_WFDefinition ADD RejectionReason NVARCHAR(500) NULL;
    PRINT 'Column RejectionReason added.';
END
GO

-- Back-fill: existing IsActive=1 rows → Active(2), IsActive=0 rows → Draft(0)
UPDATE WFE.s_WFDefinition
SET    ApprovalStatus = CASE WHEN IsActive = 1 THEN 2 ELSE 0 END
WHERE  ApprovalStatus = 0
  AND  ISNULL(SubmittedDate, '1900-01-01') = '1900-01-01';
PRINT 'Back-fill ApprovalStatus complete.';
GO

-- Index for sidebar badge query (PendingApproval filter)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('WFE.s_WFDefinition')
      AND name = 'IX_WFDef_ApprovalStatus_IsDeleted'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_WFDef_ApprovalStatus_IsDeleted
        ON WFE.s_WFDefinition (ApprovalStatus, IsDeleted)
        INCLUDE (WFDefinitionId, WFName);
    PRINT 'Index IX_WFDef_ApprovalStatus_IsDeleted created.';
END
GO

/* ============================================================================
   PART 2 — Reporting Stored Procedures (SLA Report)
   ============================================================================ */

/* ----------------------------------------------------------------------------
   SP: dbo.sp_WF_GetSLAComplianceMetrics
   Aggregate SLA compliance for the given date range / template filter.
   ---------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.sp_WF_GetSLAComplianceMetrics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_WF_GetSLAComplianceMetrics;
GO
CREATE PROCEDURE dbo.sp_WF_GetSLAComplianceMetrics
    @WorkflowDefinitionId INT      = NULL,
    @FromDate             DATETIME = NULL,
    @ToDate               DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*)                                                                   AS TotalSteps,
        SUM(CASE WHEN wsi.SLAStatus = 1 THEN 1 ELSE 0 END)                       AS OnTimeCount,
        SUM(CASE WHEN wsi.SLAStatus = 2 THEN 1 ELSE 0 END)                       AS AtRiskCount,
        SUM(CASE WHEN wsi.SLAStatus = 3 THEN 1 ELSE 0 END)                       AS OverdueCount,
        ISNULL(
            CAST(SUM(CASE WHEN wsi.SLAStatus = 1 THEN 1 ELSE 0 END) AS float)
            / NULLIF(COUNT(*), 0) * 100, 0)                                       AS OnTimePercent,
        ISNULL(AVG(DATEDIFF(MINUTE,
            wsi.AssignedDate,
            ISNULL(wsi.CompletedDate, GETUTCDATE())) / 60.0), 0)                 AS AvgCompletionHours
    FROM   dbo.WorkflowStepInstance wsi
    INNER  JOIN dbo.WorkflowInstance wi
           ON wi.WorkflowInstanceId = wsi.WorkflowInstanceId AND wi.IsDeleted = 0
    WHERE  wsi.IsDeleted = 0
      AND  (@WorkflowDefinitionId IS NULL OR wi.WorkflowDefinitionId = @WorkflowDefinitionId)
      AND  (@FromDate IS NULL OR wsi.AssignedDate >= @FromDate)
      AND  (@ToDate   IS NULL OR wsi.AssignedDate <= @ToDate);
END
GO

/* ----------------------------------------------------------------------------
   SP: dbo.sp_WF_GetTaskDistribution
   Per-assignee task counts for the overdue-by-person table.
   ---------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.sp_WF_GetTaskDistribution', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_WF_GetTaskDistribution;
GO
CREATE PROCEDURE dbo.sp_WF_GetTaskDistribution
    @WorkflowDefinitionId INT      = NULL,
    @FromDate             DATETIME = NULL,
    @ToDate               DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        wsi.AssignedTo                                                             AS AssignedTo,
        COUNT(*)                                                                   AS TotalAssigned,
        SUM(CASE WHEN wsi.Status = 3 THEN 1 ELSE 0 END)                          AS Completed,
        SUM(CASE WHEN wsi.Status NOT IN (3, 4) THEN 1 ELSE 0 END)                AS Pending,
        SUM(CASE WHEN wsi.Status = 4 THEN 1 ELSE 0 END)                          AS Rejected,
        SUM(CASE WHEN wsi.SLAStatus = 3 THEN 1 ELSE 0 END)                       AS Overdue
    FROM   dbo.WorkflowStepInstance wsi
    INNER  JOIN dbo.WorkflowInstance wi
           ON wi.WorkflowInstanceId = wsi.WorkflowInstanceId AND wi.IsDeleted = 0
    WHERE  wsi.IsDeleted = 0
      AND  wsi.AssignedTo IS NOT NULL
      AND  (@WorkflowDefinitionId IS NULL OR wi.WorkflowDefinitionId = @WorkflowDefinitionId)
      AND  (@FromDate IS NULL OR wsi.AssignedDate >= @FromDate)
      AND  (@ToDate   IS NULL OR wsi.AssignedDate <= @ToDate)
    GROUP  BY wsi.AssignedTo
    ORDER  BY Overdue DESC, TotalAssigned DESC;
END
GO

/* ----------------------------------------------------------------------------
   SP: dbo.sp_WF_GetBottleneckAnalysis
   Per-step average/max completion times.
   ---------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.sp_WF_GetBottleneckAnalysis', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_WF_GetBottleneckAnalysis;
GO
CREATE PROCEDURE dbo.sp_WF_GetBottleneckAnalysis
    @WorkflowDefinitionId INT      = NULL,
    @FromDate             DATETIME = NULL,
    @ToDate               DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ws.WorkflowStepId,
        ws.StepName,
        ws.StepType,
        ws.SequenceNumber,
        wd.WFName                                                                  AS WorkflowDefinitionName,
        COUNT(*)                                                                   AS TotalExecutions,
        SUM(CASE WHEN wsi.Status = 3 THEN 1 ELSE 0 END)                          AS CompletedCount,
        SUM(CASE WHEN wsi.SLAStatus = 3 THEN 1 ELSE 0 END)                       AS OverdueCount,
        AVG(CASE WHEN wsi.CompletedDate IS NOT NULL
            THEN DATEDIFF(MINUTE, wsi.AssignedDate, wsi.CompletedDate) / 60.0
            ELSE NULL END)                                                         AS AvgCompletionHours,
        MAX(CASE WHEN wsi.CompletedDate IS NOT NULL
            THEN DATEDIFF(MINUTE, wsi.AssignedDate, wsi.CompletedDate) / 60.0
            ELSE NULL END)                                                         AS MaxCompletionHours
    FROM   dbo.WorkflowStepInstance wsi
    INNER  JOIN dbo.WorkflowInstance wi
           ON wi.WorkflowInstanceId = wsi.WorkflowInstanceId AND wi.IsDeleted = 0
    INNER  JOIN dbo.WorkflowStep ws
           ON ws.WorkflowStepId = wsi.WorkflowStepId
    INNER  JOIN WFE.s_WFDefinition wd
           ON wd.WFDefinitionId = wi.WorkflowDefinitionId AND wd.IsDeleted = 0
    WHERE  wsi.IsDeleted = 0
      AND  (@WorkflowDefinitionId IS NULL OR wi.WorkflowDefinitionId = @WorkflowDefinitionId)
      AND  (@FromDate IS NULL OR wsi.AssignedDate >= @FromDate)
      AND  (@ToDate   IS NULL OR wsi.AssignedDate <= @ToDate)
    GROUP  BY ws.WorkflowStepId, ws.StepName, ws.StepType, ws.SequenceNumber, wd.WFName
    ORDER  BY AvgCompletionHours DESC;
END
GO

PRINT '080_WFE_ApprovalLifecycle_And_SLAReport.sql completed successfully.';
GO
