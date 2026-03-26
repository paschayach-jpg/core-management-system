-- ============================================================
-- Script 006: ลบข้อมูลทั้งหมด (ไม่ลบ schema / table / SP)
-- ลำดับการลบต้องเป็นไปตาม FK dependency
-- Run ใน SSMS กับ database SCS
-- ============================================================

USE SCS;
GO

-- ปิด FK check ชั่วคราวไม่ได้ใน SQL Server (ไม่มี DISABLE ALL CONSTRAINTS แบบ MySQL)
-- ต้องลบตามลำดับ child → parent

BEGIN TRANSACTION;
BEGIN TRY

    -- ── ชั้น 1: ตารางที่ไม่มีใครอ้างถึง ──────────────────────────────────

    -- Audit log (ไม่มี FK ออกไปหา OrgUnit โดยตรง)
    DELETE FROM core.AuditLog;

    -- Notification
    DELETE FROM core.Notification;

    -- Approval steps ก่อน ApprovalRequest
    DELETE FROM workflow.ApprovalStep;
    DELETE FROM workflow.ApprovalRequest;

    -- ── ชั้น 2: storage ──────────────────────────────────────────────────

    -- FileDocument ก่อน Folder
    DELETE FROM storage.FileDocument;

    -- FolderPermission ก่อน Folder
    DELETE FROM security.FolderPermission;

    -- FolderTemplateItem ก่อน FolderTemplate
    DELETE FROM storage.FolderTemplateItem;
    DELETE FROM storage.FolderTemplate;

    -- Folder (self-referencing FK — ต้องลบ children ก่อน parent)
    -- ใช้ recursive CTE เพื่อลบจาก leaf → root
    WITH FolderOrder AS (
        SELECT FolderID, ParentFolderID, 0 AS Lvl
        FROM   storage.Folder
        WHERE  ParentFolderID IS NULL
        UNION ALL
        SELECT f.FolderID, f.ParentFolderID, fo.Lvl + 1
        FROM   storage.Folder f
        INNER  JOIN FolderOrder fo ON f.ParentFolderID = fo.FolderID
    )
    DELETE FROM storage.Folder
    WHERE  FolderID IN (
        SELECT FolderID FROM FolderOrder
    );

    -- ── ชั้น 3: security ─────────────────────────────────────────────────

    -- PermissionGroup members ก่อน PermissionGroup
    DELETE FROM security.UserGroupMember;
    DELETE FROM security.PermissionGroup;

    -- ── ชั้น 4: core.UserAccount (FK → OrgUnit) ──────────────────────────

    -- ต้อง NULL ก่อนถ้า column เป็น nullable หรือลบตรง
    DELETE FROM core.UserAccount;

    -- ── ชั้น 5: core.OrgUnit (self-referencing FK) ───────────────────────

    -- ลบ leaf nodes ก่อน (children ก่อน parent)
    -- วิธีง่ายที่สุดคือ NULL ParentOrgUnitID ก่อน แล้ว DELETE
    UPDATE core.OrgUnit SET ParentOrgUnitID = NULL;
    DELETE FROM core.OrgUnit;

    COMMIT TRANSACTION;
    PRINT '=== ลบข้อมูลทั้งหมดสำเร็จ ===';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '=== ERROR: ' + ERROR_MESSAGE() + ' ===';
    THROW;
END CATCH
GO
