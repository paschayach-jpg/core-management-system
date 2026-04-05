-- ============================================================
-- Script 052: Clear All Folder Data
-- ลบข้อมูล Folder ทั้งหมดพร้อม dependent tables
-- ลำดับ: ลูกสุดก่อน → แม่หลัง
-- ============================================================


SET NOCOUNT ON;
PRINT '🧹 เริ่มลบข้อมูล Folder ทั้งหมด...'
PRINT '=================================================='

-- 1. AuditLog (ไม่มี FK ออก → ลบก่อน)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditLog' AND schema_id = SCHEMA_ID('audit'))
BEGIN
    DELETE FROM audit.AuditLog;
    PRINT '✅ audit.AuditLog (' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' rows)'
END

-- 2. ApprovalStep → ApprovalRequest
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ApprovalStep' AND schema_id = SCHEMA_ID('workflow'))
BEGIN
    DELETE FROM workflow.ApprovalStep;
    PRINT '✅ workflow.ApprovalStep (' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' rows)'
END

-- 3. ApprovalRequest (FK → Folder, FileDocument, FolderPermission)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ApprovalRequest' AND schema_id = SCHEMA_ID('workflow'))
BEGIN
    DELETE FROM workflow.ApprovalRequest;
    PRINT '✅ workflow.ApprovalRequest (' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' rows)'
END

-- 4. FilePermission (FK → FileDocument)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FilePermission' AND schema_id = SCHEMA_ID('security'))
BEGIN
    DELETE FROM security.FilePermission;
    PRINT '✅ security.FilePermission (' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' rows)'
END

-- 5. FileDocument (FK → Folder)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FileDocument' AND schema_id = SCHEMA_ID('storage'))
BEGIN
    DELETE FROM storage.FileDocument;
    PRINT '✅ storage.FileDocument (' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' rows)'
END

-- 6. FolderPermission (FK → Folder)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FolderPermission' AND schema_id = SCHEMA_ID('security'))
BEGIN
    DELETE FROM security.FolderPermission;
    PRINT '✅ security.FolderPermission (' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' rows)'
END

-- 7. QuickAccess (FK → Folder, ON DELETE CASCADE แต่ลบเองเพื่อความชัดเจน)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'QuickAccess' AND schema_id = SCHEMA_ID('storage'))
BEGIN
    DELETE FROM storage.QuickAccess;
    PRINT '✅ storage.QuickAccess (' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' rows)'
END

-- 8. Folder (self-referencing FK → ParentFolderID)
--    ต้อง disable FK ก่อนเพราะ self-ref
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Folder' AND schema_id = SCHEMA_ID('storage'))
BEGIN
    ALTER TABLE storage.Folder NOCHECK CONSTRAINT FK_sto_Folder_Parent;
    DELETE FROM storage.Folder;
    ALTER TABLE storage.Folder CHECK CONSTRAINT FK_sto_Folder_Parent;
    PRINT '✅ storage.Folder (' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' rows)'
END

-- 9. Reset IDENTITY counters
DBCC CHECKIDENT ('storage.Folder',          RESEED, 0);
DBCC CHECKIDENT ('storage.FileDocument',    RESEED, 0);
DBCC CHECKIDENT ('security.FolderPermission', RESEED, 0);
DBCC CHECKIDENT ('security.FilePermission', RESEED, 0);
DBCC CHECKIDENT ('workflow.ApprovalRequest', RESEED, 0);
DBCC CHECKIDENT ('workflow.ApprovalStep',   RESEED, 0);
DBCC CHECKIDENT ('audit.AuditLog',          RESEED, 0);
PRINT '✅ Reset IDENTITY counters แล้ว'

PRINT ''
PRINT '=================================================='
PRINT '✅ ลบข้อมูล Folder ทั้งหมดเรียบร้อยแล้ว'
PRINT '=================================================='
GO
