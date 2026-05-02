-- ============================================================
-- Clean up old WFE tables before running new schema
-- This drops all WFE tables to allow fresh creation
-- ============================================================

PRINT 'Starting cleanup of old WFE schema...'
GO

-- Drop stored procedures first (no dependencies)
IF OBJECT_ID(N'WFE.sp_CreateWFInstance', N'P') IS NOT NULL
    DROP PROCEDURE WFE.sp_CreateWFInstance
IF OBJECT_ID(N'WFE.sp_UpdateWFInstanceStatus', N'P') IS NOT NULL
    DROP PROCEDURE WFE.sp_UpdateWFInstanceStatus
IF OBJECT_ID(N'WFE.sp_CreateWFStepInstance', N'P') IS NOT NULL
    DROP PROCEDURE WFE.sp_CreateWFStepInstance
PRINT 'Dropped stored procedures'
GO

-- Drop tables in reverse dependency order
-- 1. Audit log (references WFE.t_WFInstance)
IF OBJECT_ID(N'WFE.l_WFEAuditEvent', N'U') IS NOT NULL
    DROP TABLE WFE.l_WFEAuditEvent
PRINT 'Dropped WFE.l_WFEAuditEvent'
GO

-- 2. Step instances (references WFE.t_WFInstance and WFE.s_WFStep)
IF OBJECT_ID(N'WFE.t_WFStepInstance', N'U') IS NOT NULL
    DROP TABLE WFE.t_WFStepInstance
PRINT 'Dropped WFE.t_WFStepInstance'
GO

-- 3. Workflow instances (references WFE.s_WFDefinition and WFE.s_WFStep)
IF OBJECT_ID(N'WFE.t_WFInstance', N'U') IS NOT NULL
    DROP TABLE WFE.t_WFInstance
PRINT 'Dropped WFE.t_WFInstance'
GO

-- 4. Connections (references WFE.s_WFDefinition and WFE.s_WFStep)
IF OBJECT_ID(N'WFE.s_WFConnection', N'U') IS NOT NULL
    DROP TABLE WFE.s_WFConnection
PRINT 'Dropped WFE.s_WFConnection'
GO

-- 5. Steps (references WFE.s_WFDefinition)
IF OBJECT_ID(N'WFE.s_WFStep', N'U') IS NOT NULL
    DROP TABLE WFE.s_WFStep
PRINT 'Dropped WFE.s_WFStep'
GO

-- 6. Definition (parent table)
IF OBJECT_ID(N'WFE.s_WFDefinition', N'U') IS NOT NULL
    DROP TABLE WFE.s_WFDefinition
PRINT 'Dropped WFE.s_WFDefinition'
GO

-- Drop schema if empty
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'WFE')
    DROP SCHEMA WFE
PRINT 'Dropped WFE schema'
GO

PRINT '============================================================'
PRINT 'Cleanup complete! Ready for fresh schema creation.'
PRINT 'Next: Run 010_CreateWorkflowTables.sql'
PRINT '============================================================'
GO
