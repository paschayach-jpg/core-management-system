-- ============================================================
-- Script: 081_Add_DownloadFile_RequestType.sql
-- Purpose: Allow DOWNLOAD_FILE request type in workflow approvals
--          (file-download requests must be approved by Manager before
--           the file can be retrieved by the requesting user)
-- RequestStatus extension: 5 = Downloaded (approval consumed after download)
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
    'INTERNAL_SHARE',
    'DOWNLOAD_FILE'
));
GO
