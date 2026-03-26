-- ============================================================
-- Script 006: Debug + Fix สิทธิ์ไม่แสดงใน PermissionForm
-- Run ใน SSMS กับ database SCS
-- ============================================================

USE SCS;
GO

-- ── Step 1: ตรวจสอบว่า SP มีอยู่ไหม ──────────────────────────
SELECT s.name AS SchemaName, p.name AS ProcName
FROM   sys.procedures p
INNER  JOIN sys.schemas s ON s.schema_id = p.schema_id
WHERE  p.name = 'sp_GetPermissionsByFolder';
GO

-- ── Step 2: ดูข้อมูลดิบใน security.FolderPermission ──────────
SELECT PermissionID, FolderID, PrincipalType, PrincipalID,
       CanRead, CanWrite, RevokedDate, ExpiryDate
FROM   security.FolderPermission
ORDER  BY FolderID;
GO

-- ── Step 3: ทดสอบ SP โดยตรง (เปลี่ยน FolderID ตามจริง) ──────
EXEC security.sp_GetPermissionsByFolder @FolderID = 12;
GO

-- ── Step 4: สร้าง/แทนที่ SP ให้แน่ใจว่าถูกต้อง ───────────────
IF EXISTS (SELECT * FROM sys.procedures p
           INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
           WHERE s.name = 'security' AND p.name = 'sp_GetPermissionsByFolder')
    DROP PROCEDURE security.sp_GetPermissionsByFolder;
GO

CREATE PROCEDURE security.sp_GetPermissionsByFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PermissionID, FolderID, PrincipalType, PrincipalID,
           CanRead, CanWrite, CanDelete, CanModify, CanFullControl, IsDeny,
           ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
    FROM   security.FolderPermission
    WHERE  FolderID    = @FolderID
      AND  RevokedDate IS NULL
      AND  (ExpiryDate IS NULL OR ExpiryDate > GETDATE());
END
GO

PRINT '=== security.sp_GetPermissionsByFolder recreated ===';
GO

-- ── Step 5: ทดสอบอีกครั้งหลัง recreate ──────────────────────
EXEC security.sp_GetPermissionsByFolder @FolderID = 12;
GO
