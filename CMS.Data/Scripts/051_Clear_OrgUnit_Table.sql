-- ============================================================
-- Script 051: Clear OrgUnit Table
-- ลบข้อมูลทั้งหมดใน core.OrgUnit และตารางที่เกี่ยวข้อง
-- ============================================================


PRINT '🧹 เริ่มลบข้อมูล OrgUnit และตารางที่เกี่ยวข้อง...'
PRINT '=================================================='

-- ลบตามลำดับ FK dependencies (จากลูกไปหาแม่)

-- 1. ลบ UserOrgVisibility (FK → OrgUnit)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserOrgVisibility' AND schema_id = SCHEMA_ID('security'))
BEGIN
    DELETE FROM security.UserOrgVisibility;
    PRINT '✅ ลบข้อมูล security.UserOrgVisibility แล้ว'
END

-- 2. ลบ OrgUnitRelationship (FK → OrgUnit)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OrgUnitRelationship' AND schema_id = SCHEMA_ID('core'))
BEGIN
    DELETE FROM core.OrgUnitRelationship;
    PRINT '✅ ลบข้อมูล core.OrgUnitRelationship แล้ว'
END

-- 3. Update UserAccount.OrgUnitID → NULL (FK → OrgUnit)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount' AND schema_id = SCHEMA_ID('core'))
BEGIN
    UPDATE core.UserAccount SET OrgUnitID = NULL WHERE OrgUnitID IS NOT NULL;
    PRINT '✅ Set core.UserAccount.OrgUnitID = NULL แล้ว'
END

-- 4. Update PermissionGroup.OrgUnitID → NULL (FK → OrgUnit)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PermissionGroup' AND schema_id = SCHEMA_ID('security'))
BEGIN
    UPDATE security.PermissionGroup SET OrgUnitID = NULL WHERE OrgUnitID IS NOT NULL;
    PRINT '✅ Set security.PermissionGroup.OrgUnitID = NULL แล้ว'
END

-- 5. Update Folder.OrgUnitID → NULL (FK → OrgUnit)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Folder' AND schema_id = SCHEMA_ID('storage'))
BEGIN
    UPDATE storage.Folder SET OrgUnitID = NULL WHERE OrgUnitID IS NOT NULL;
    PRINT '✅ Set storage.Folder.OrgUnitID = NULL แล้ว'
END

-- 6. ลบ OrgUnit (ตารางหลัก)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OrgUnit' AND schema_id = SCHEMA_ID('core'))
BEGIN
    DELETE FROM core.OrgUnit;
    PRINT '✅ ลบข้อมูล core.OrgUnit แล้ว'
END

PRINT ''
PRINT '=================================================='
PRINT '✅ ลบข้อมูล OrgUnit และตารางที่เกี่ยวข้องเรียบร้อยแล้ว'
PRINT '=================================================='
GO
