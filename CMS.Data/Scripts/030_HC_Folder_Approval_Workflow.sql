-- ============================================================
-- Script: 030_HC_Folder_Approval_Workflow.sql
-- Purpose: Add support for HC folder creation approval workflow
-- Description: 
--   - Adds CREATE_HC to the RequestType check constraint
--   - Ensures workflow.ApprovalRequest table supports storing
--     HC folder creation requests with folder metadata in Remark
-- ============================================================

-- Update the CHECK constraint on RequestType to include CREATE_HC
-- First, drop the existing constraint
ALTER TABLE workflow.ApprovalRequest
DROP CONSTRAINT CK_wf_ApprovalRequest_Type;

-- Add the new constraint with CREATE_HC included
ALTER TABLE workflow.ApprovalRequest
ADD CONSTRAINT CK_wf_ApprovalRequest_Type 
CHECK (RequestType IN ('CREATE_HC', 'DELETE_FOLDER', 'CHANGE_PERMISSION', 'UPLOAD_HC'));

-- Add index on RequestType for faster filtering of CREATE_HC requests
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_wf_ApprovalRequest_Type')
BEGIN
    CREATE INDEX IX_wf_ApprovalRequest_Type 
    ON workflow.ApprovalRequest(RequestType, RequestStatus);
END
GO

-- Stored procedure to get pending CREATE_HC requests
IF OBJECT_ID('workflow.sp_GetPendingCreateHCRequests', 'P') IS NOT NULL
    DROP PROCEDURE workflow.sp_GetPendingCreateHCRequests;
GO

CREATE PROCEDURE workflow.sp_GetPendingCreateHCRequests
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RequestID, RequestType, FolderID, FileID, PermissionID, RequestedBy,
           RequestedDate, RequestStatus, Subject, Remark, ExpiryDate,
           ResolvedBy, ResolvedDate, ResolveRemark
    FROM   workflow.ApprovalRequest
    WHERE  RequestType = 'CREATE_HC' AND RequestStatus = 1
    ORDER  BY RequestedDate DESC;
END
GO
