-- ============================================================
-- 072_Share_Permission_Change_Request.sql
-- Support for Share Permission Change Requests & Approval
-- ============================================================

-- ── 1. Add TargetPermissionID column to workflow.ApprovalRequestItem ────
IF COL_LENGTH(N'workflow.ApprovalRequestItem', N'TargetPermissionID') IS NULL
BEGIN
    ALTER TABLE workflow.ApprovalRequestItem
        ADD TargetPermissionID INT NULL;
    PRINT 'Added workflow.ApprovalRequestItem.TargetPermissionID column.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApprovalRequestItem_TargetPermissionID' AND object_id = OBJECT_ID('workflow.ApprovalRequestItem'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ApprovalRequestItem_TargetPermissionID
        ON workflow.ApprovalRequestItem (TargetPermissionID)
        WHERE TargetPermissionID IS NOT NULL;
    PRINT 'Created index IX_ApprovalRequestItem_TargetPermissionID.';
END
GO

-- ── 2. Update CK_wf_ApprovalRequest_Type check constraint to include 'SHARE_PERMISSION_CHANGE' ───
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_wf_ApprovalRequest_Type')
BEGIN
    ALTER TABLE workflow.ApprovalRequest DROP CONSTRAINT CK_wf_ApprovalRequest_Type;
    ALTER TABLE workflow.ApprovalRequest WITH CHECK ADD CONSTRAINT CK_wf_ApprovalRequest_Type 
        CHECK (RequestType IN ('CREATE_HC', 'DELETE_FOLDER', 'CHANGE_PERMISSION', 'UPLOAD_HC', 'EXTERNAL_SHARE', 'DOWNLOAD_FILE', 'INTERNAL_SHARE', 'SHARE_PERMISSION_CHANGE'));
    PRINT 'Updated CK_wf_ApprovalRequest_Type check constraint to include SHARE_PERMISSION_CHANGE.';
END
GO
