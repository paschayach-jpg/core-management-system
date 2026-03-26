-- ============================================================
-- Script: 018_Reset_Data_KeepUserAccount.sql
-- Purpose: ล้างข้อมูลทั้งหมดเพื่อเริ่มต้นใหม่
--          ยกเว้น core.UserAccount (ห้ามลบ)
-- Order: ลบตาม FK dependency จากล่างขึ้นบน
-- ============================================================

USE SCS;
GO

PRINT '=== START RESET DATA (Keep UserAccount) ===';
PRINT 'Time: ' + CONVERT(NVARCHAR, GETDATE(), 120);
GO

-- ============================================================
-- Step 1: audit.AuditLog  (ไม่มี FK ออก → ลบก่อนได้)
-- ============================================================
DELETE FROM audit.AuditLog;
PRINT 'Deleted: audit.AuditLog  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

-- ============================================================
-- Step 2: workflow.ApprovalStep  (FK → ApprovalRequest)
-- ============================================================
DELETE FROM workflow.ApprovalStep;
PRINT 'Deleted: workflow.ApprovalStep  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

-- ============================================================
-- Step 3: workflow.ApprovalRequest  (FK → Folder, File, Permission)
-- ============================================================
DELETE FROM workflow.ApprovalRequest;
PRINT 'Deleted: workflow.ApprovalRequest  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

-- ============================================================
-- Step 4: security.FolderPermission  (FK → Folder, UserAccount)
-- ============================================================
DELETE FROM security.FolderPermission;
PRINT 'Deleted: security.FolderPermission  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

-- ============================================================
-- Step 5: security.UserGroupMember  (FK → PermissionGroup, UserAccount)
-- ============================================================
DELETE FROM security.UserGroupMember;
PRINT 'Deleted: security.UserGroupMember  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

-- ============================================================
-- Step 6: security.PermissionGroup  (FK → OrgUnit, UserAccount)
-- ============================================================
DELETE FROM security.PermissionGroup;
PRINT 'Deleted: security.PermissionGroup  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

-- ============================================================
-- Step 7: storage.FileDocument  (FK → Folder, UserAccount)
-- ============================================================
DELETE FROM storage.FileDocument;
PRINT 'Deleted: storage.FileDocument  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

-- ============================================================
-- Step 8: storage.Folder  (self-ref FK → ต้อง disable ก่อน)
-- ============================================================
ALTER TABLE storage.Folder NOCHECK CONSTRAINT FK_sto_Folder_Parent;
GO

DELETE FROM storage.Folder;
PRINT 'Deleted: storage.Folder  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

ALTER TABLE storage.Folder CHECK CONSTRAINT FK_sto_Folder_Parent;
GO

-- ============================================================
-- Step 9: storage.FolderTemplateItem  (FK → FolderTemplate, self-ref)
-- ============================================================
ALTER TABLE storage.FolderTemplateItem NOCHECK CONSTRAINT FK_sto_FTplItem_Parent;
GO

DELETE FROM storage.FolderTemplateItem;
PRINT 'Deleted: storage.FolderTemplateItem  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

ALTER TABLE storage.FolderTemplateItem CHECK CONSTRAINT FK_sto_FTplItem_Parent;
GO

-- ============================================================
-- Step 10: storage.FolderTemplate  (FK → UserAccount)
-- ============================================================
DELETE FROM storage.FolderTemplate;
PRINT 'Deleted: storage.FolderTemplate  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

-- ============================================================
-- Step 11: core.OrgUnit  (self-ref FK + FK จาก UserAccount.OrgUnitID)
--          ต้อง disable FK ทั้งหมดที่อ้างถึง OrgUnit ก่อน
-- ============================================================

-- Disable FK จาก UserAccount → OrgUnit ชั่วคราว
ALTER TABLE core.UserAccount NOCHECK CONSTRAINT FK_core_UserAccount_OrgUnit;
GO

-- Disable FK จาก OrgUnit → OrgUnit (self-ref parent)
ALTER TABLE core.OrgUnit NOCHECK CONSTRAINT FK_core_OrgUnit_Parent;
GO

-- Disable FK จาก OrgUnit → UserAccount (DeletedBy)
ALTER TABLE core.OrgUnit NOCHECK CONSTRAINT FK_core_OrgUnit_DeletedBy;
GO

DELETE FROM core.OrgUnit;
PRINT 'Deleted: core.OrgUnit  (' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows)';
GO

-- Reset IDENTITY ให้เริ่มที่ 1 ใหม่
DBCC CHECKIDENT ('core.OrgUnit', RESEED, 0);
GO

-- Re-enable FK
ALTER TABLE core.OrgUnit     CHECK CONSTRAINT FK_core_OrgUnit_Parent;
ALTER TABLE core.OrgUnit     CHECK CONSTRAINT FK_core_OrgUnit_DeletedBy;
ALTER TABLE core.UserAccount CHECK CONSTRAINT FK_core_UserAccount_OrgUnit;
GO

-- ============================================================
-- Step 12: Reset IDENTITY counters (ยกเว้น UserAccount)
-- ============================================================
DBCC CHECKIDENT ('storage.Folder',              RESEED, 0);
DBCC CHECKIDENT ('storage.FileDocument',        RESEED, 0);
DBCC CHECKIDENT ('storage.FolderTemplate',      RESEED, 0);
DBCC CHECKIDENT ('storage.FolderTemplateItem',  RESEED, 0);
DBCC CHECKIDENT ('security.FolderPermission',   RESEED, 0);
DBCC CHECKIDENT ('security.PermissionGroup',    RESEED, 0);
DBCC CHECKIDENT ('security.UserGroupMember',    RESEED, 0);
DBCC CHECKIDENT ('workflow.ApprovalRequest',    RESEED, 0);
DBCC CHECKIDENT ('workflow.ApprovalStep',       RESEED, 0);
DBCC CHECKIDENT ('audit.AuditLog',              RESEED, 0);
GO

PRINT '';
PRINT '=== RESET COMPLETE ===';
PRINT 'core.UserAccount: PRESERVED (not deleted)';
PRINT 'All other tables: CLEARED and IDENTITY reset to 1';
PRINT '';
PRINT 'Next step: สร้าง OrgUnit ใหม่ แล้ว UPDATE core.UserAccount SET OrgUnitID = <new_id>';
GO

-- ============================================================
-- Step 13: แสดงจำนวน UserAccount ที่เหลืออยู่
-- ============================================================
SELECT
    COUNT(*)        AS UserCount,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
    SUM(CASE WHEN AppRole  = 4 THEN 1 ELSE 0 END) AS AdminCount
FROM core.UserAccount;
GO
