-- ============================================================
-- Script: 080_Add_InternalShare_RequestType.sql
-- Purpose: Allow INTERNAL_SHARE request type in workflow approvals
--          (file-share requests from any user to a Department principal)
-- ============================================================

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_wf_ApprovalRequest_Type'
      AND parent_object_id = OBJECT_ID('workflow.ApprovalRequest')
)
BEGIN
    ALTER TABLE workflow.ApprovalRequest
    DROP CONSTRAINT CK_wf_ApprovalRequest_Type;
END
GO

ALTER TABLE workflow.ApprovalRequest
ADD CONSTRAINT CK_wf_ApprovalRequest_Type
CHECK (RequestType IN (
    'CREATE_HC',
    'DELETE_FOLDER',
    'CHANGE_PERMISSION',
    'UPLOAD_HC',
    'EXTERNAL_SHARE',
    'INTERNAL_SHARE'
));
GO
