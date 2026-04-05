-- ============================================================
-- Script 053: Fix OrgUnitCode Duplicates
-- ลบ OrgUnit ที่ใช้ auto-code เก่า (DEPT-NNN, DIV-NNN)
-- เก็บเฉพาะ record ที่ใช้รหัสจาก HR table (DEP-XX, DIV-XX)
-- ============================================================

SET NOCOUNT ON;
PRINT '� แสดง OrgUnit ที่จะถูกลบ (auto-code pattern)...';

-- แสดงก่อนลบ
SELECT OrgUnitID, OrgUnitCode, OrgUnitNameTH, OrgUnitType
FROM   core.OrgUnit
WHERE  IsDeleted = 0
  AND  (
           OrgUnitCode LIKE 'DEPT-[0-9][0-9][0-9]'
        OR OrgUnitCode LIKE 'DIV-[0-9][0-9][0-9]'
       );

-- ลบ folder ที่ผูกกับ OrgUnit auto-code ก่อน (ถ้ามี)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Folder' AND schema_id = SCHEMA_ID('storage'))
BEGIN
    DELETE FROM storage.Folder
    WHERE OrgUnitID IN (
        SELECT OrgUnitID FROM core.OrgUnit
        WHERE IsDeleted = 0
          AND (OrgUnitCode LIKE 'DEPT-[0-9][0-9][0-9]'
            OR OrgUnitCode LIKE 'DIV-[0-9][0-9][0-9]')
    );
    PRINT '✅ ลบ Folder ที่ผูกกับ auto-code OrgUnit แล้ว: ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows';
END

-- ลบ OrgUnit auto-code
DELETE FROM core.OrgUnit
WHERE  IsDeleted = 0
  AND  (
           OrgUnitCode LIKE 'DEPT-[0-9][0-9][0-9]'
        OR OrgUnitCode LIKE 'DIV-[0-9][0-9][0-9]'
       );

PRINT '✅ ลบ OrgUnit auto-code แล้ว: ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows';

-- แสดงผลหลังลบ
PRINT '';
PRINT '📋 OrgUnit ที่เหลืออยู่:';
SELECT OrgUnitID, OrgUnitCode, OrgUnitNameTH, OrgUnitType
FROM   core.OrgUnit
WHERE  IsDeleted = 0
ORDER  BY OrgUnitType, OrgUnitCode;
GO
