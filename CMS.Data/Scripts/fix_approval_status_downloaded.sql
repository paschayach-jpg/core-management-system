-- ============================================================
-- Allow RequestStatus = 5 (Downloaded) on workflow.ApprovalRequest.
--
-- ApprovalService.ConsumeDownloadApproval marks a used DOWNLOAD_FILE approval as 5 so one approval
-- = one download, but CK_wf_ApprovalRequest_Status rejects 5. The old download code swallowed that
-- error, so approvals were never consumed (an approved file could be downloaded again and again).
--
-- Status values: 1=Pending, 2=Approved, 3=Rejected, 4=Expired/Withdrawn, 5=Downloaded
--
-- Preview first: it prints the current constraint, then ROLLS BACK unless @Commit = 1.
-- Rename with the next schema script number before committing. Safe to re-run.
-- ============================================================
SET XACT_ABORT ON;
DECLARE @Commit BIT = 0;

PRINT '-- Current constraint definition:';
SELECT cc.name AS ConstraintName, cc.definition
FROM   sys.check_constraints cc
WHERE  cc.parent_object_id = OBJECT_ID(N'workflow.ApprovalRequest')
  AND  cc.name = N'CK_wf_ApprovalRequest_Status';

PRINT '-- Status values currently in use:';
SELECT RequestStatus, COUNT(*) AS Requests
FROM   workflow.ApprovalRequest
GROUP BY RequestStatus
ORDER BY RequestStatus;

BEGIN TRAN;

IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE parent_object_id = OBJECT_ID(N'workflow.ApprovalRequest')
             AND name = N'CK_wf_ApprovalRequest_Status')
    ALTER TABLE workflow.ApprovalRequest DROP CONSTRAINT CK_wf_ApprovalRequest_Status;

ALTER TABLE workflow.ApprovalRequest WITH CHECK
    ADD CONSTRAINT CK_wf_ApprovalRequest_Status CHECK (RequestStatus IN (1, 2, 3, 4, 5));

PRINT '-- New constraint definition:';
SELECT cc.name AS ConstraintName, cc.definition
FROM   sys.check_constraints cc
WHERE  cc.parent_object_id = OBJECT_ID(N'workflow.ApprovalRequest')
  AND  cc.name = N'CK_wf_ApprovalRequest_Status';

IF @Commit = 1
BEGIN
    COMMIT TRAN;
    PRINT 'Committed.';
END
ELSE
BEGIN
    ROLLBACK TRAN;
    PRINT 'Rolled back (preview only). Set @Commit = 1 to apply.';
END
