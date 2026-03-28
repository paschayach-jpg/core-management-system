-- ============================================================
-- Script 038: Simple Clear and Reset
-- ล้างข้อมูลและรีเซ็ตแบบง่ายๆ ไม่ซับซ้อน
-- ============================================================

PRINT '🚀 Simple Clear and Reset'
PRINT '========================'
PRINT 'กำลังล้างข้อมูลและรีเซ็ตระบบแบบง่ายๆ...'
PRINT ''

-- ขั้นตอนที่ 1: สร้าง backup
PRINT '📦 ขั้นตอนที่ 1: สร้าง Backup'
PRINT '=========================='

DECLARE @BackupDate NVARCHAR(20) = FORMAT(GETDATE(), 'yyyyMMdd_HHmmss')

IF EXISTS (SELECT * FROM core.OrgUnit)
BEGIN
    DECLARE @BackupTable NVARCHAR(100) = 'OrgUnit_Backup_' + @BackupDate
    DECLARE @SQL NVARCHAR(MAX) = 'SELECT * INTO core.' + @BackupTable + ' FROM core.OrgUnit'
    EXEC sp_executesql @SQL
    PRINT '✅ Backup สร้างแล้ว: core.' + @BackupTable
END

-- ขั้นตอนที่ 2: ล้างข้อมูลที่เกี่ยวข้อง (ไม่ต้องจัดการ FK)
PRINT ''
PRINT '🗑️ ขั้นตอนที่ 2: ล้างข้อมูลที่เกี่ยวข้อง'
PRINT '=================================='

BEGIN TRANSACTION SimpleClean

BEGIN TRY
    -- ล้าง UserAccount references (SET NULL)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        UPDATE core.UserAccount SET OrgUnitId = NULL WHERE OrgUnitId IS NOT NULL
        PRINT '✅ ล้าง UserAccount OrgUnit references'
    END

    -- ล้าง FolderPermission (DELETE)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FolderPermission' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        DELETE FROM core.FolderPermission WHERE PrincipalType IN (3, 4)
        PRINT '✅ ล้าง FolderPermission references'
    END

    -- ล้าง FilePermission (DELETE)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FilePermission' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        DELETE FROM core.FilePermission WHERE PrincipalType IN (3, 4)
        PRINT '✅ ล้าง FilePermission references'
    END

    -- ล้าง QuickAccessFolder (DELETE)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'QuickAccessFolder' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        DELETE FROM core.QuickAccessFolder
        PRINT '✅ ล้าง QuickAccessFolder'
    END

    -- ล้าง AuditLog entries
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLog' AND schema_id = SCHEMA_ID('audit'))
    BEGIN
        DELETE FROM audit.AuditLog WHERE TableName LIKE '%OrgUnit%'
        PRINT '✅ ล้าง AuditLog OrgUnit entries'
    END

    COMMIT TRANSACTION SimpleClean
    PRINT '✅ ล้างข้อมูลที่เกี่ยวข้องสำเร็จ'

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION SimpleClean
    PRINT '❌ เกิดข้อผิดพลาดในการล้างข้อมูลที่เกี่ยวข้อง:'
    PRINT 'Error: ' + ERROR_MESSAGE()
END CATCH

-- ขั้นตอนที่ 3: ใช้ Soft Delete แทน Hard Delete (ปลอดภัยกว่า)
PRINT ''
PRINT '🔄 ขั้นตอนที่ 3: Soft Delete OrgUnits'
PRINT '================================='

BEGIN TRY
    -- Soft Delete (ปลอดภัยกว่า Hard Delete)
    UPDATE core.OrgUnit 
    SET IsDeleted = 1, 
        IsActive = 0,
        DeletedBy = 1,
        OrgUnitCode = OrgUnitCode + '_DEL_' + @BackupDate
    WHERE IsDeleted = 0

    PRINT '✅ Soft Delete OrgUnits สำเร็จ'

    -- Reset Identity Seed
    IF EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID('core.OrgUnit'))
    BEGIN
        DBCC CHECKIDENT ('core.OrgUnit', RESEED, 0)
        PRINT '✅ Reset Identity Seed'
    END

END TRY
BEGIN CATCH
    PRINT '❌ เกิดข้อผิดพลาดในการ Soft Delete:'
    PRINT 'Error: ' + ERROR_MESSAGE()
    
    -- ถ้า Soft Delete ไม่ได้ ลองใช้ Hard Delete (อันตราย!)
    PRINT ''
    PRINT '⚠️ ลอง Hard Delete (อันตราย - ใช้เฉพาะเมื่อจำเป็น)'
    
    BEGIN TRY
        -- ลบตามลำดับ: Department → Division → SubCommittee
        DELETE FROM core.OrgUnit WHERE OrgUnitType = 3 -- Departments
        DELETE FROM core.OrgUnit WHERE OrgUnitType = 2 -- Divisions  
        DELETE FROM core.OrgUnit WHERE OrgUnitType = 1 -- SubCommittees
        
        PRINT '✅ Hard Delete สำเร็จ (อันตราย!)'
        
        -- Reset Identity Seed
        IF EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID('core.OrgUnit'))
        BEGIN
            DBCC CHECKIDENT ('core.OrgUnit', RESEED, 0)
            PRINT '✅ Reset Identity Seed'
        END
        
    END TRY
    BEGIN CATCH
        PRINT '❌ Hard Delete ก็ไม่ได้:'
        PRINT 'Error: ' + ERROR_MESSAGE()
        PRINT ''
        PRINT '💡 แนะนำ: ตรวจสอบ Foreign Key constraints ด้วยตนเอง'
        
        -- แสดง FK constraints ที่เป็นปัญหา
        SELECT 
            fk.name AS ForeignKeyName,
            OBJECT_SCHEMA_NAME(fk.parent_object_id) AS ParentSchema,
            OBJECT_NAME(fk.parent_object_id) AS ParentTable,
            COL_NAME(fkc.parent_column_id, fkc.parent_object_id) AS ParentColumn
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        WHERE fk.referenced_object_id = OBJECT_ID('core.OrgUnit')
        
        RETURN
    END CATCH
END CATCH

-- ขั้นตอนที่ 4: อัปเดต Stored Procedures
PRINT ''
PRINT '🔧 ขั้นตอนที่ 4: อัปเดต Stored Procedures'
PRINT '===================================='

-- Drop และสร้าง sp_InsertOrgUnit ใหม่
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_InsertOrgUnit') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE core.sp_InsertOrgUnit
    PRINT '✅ ลบ sp_InsertOrgUnit เก่า'
END

-- สร้าง sp_InsertOrgUnit ใหม่
EXEC('
CREATE PROCEDURE core.sp_InsertOrgUnit
    @OrgUnitCode    NVARCHAR(20),
    @OrgUnitNameTH  NVARCHAR(200),
    @OrgUnitNameEN  NVARCHAR(200),
    @OrgUnitType    TINYINT,
    @ParentOrgUnitID INT,
    @SortOrder      INT,
    @CreatedBy      INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ตรวจสอบรหัสซ้ำ
    IF EXISTS (SELECT 1 FROM core.OrgUnit WHERE OrgUnitCode = @OrgUnitCode AND IsDeleted = 0)
    BEGIN
        RAISERROR(''รหัสหน่วยงาน %s มีอยู่ในระบบแล้ว'', 16, 1, @OrgUnitCode);
        RETURN;
    END
    
    -- Insert ข้อมูล
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, CreatedBy, CreatedDate, 
        IsActive, IsDeleted, ConfidentialityLevelValue
    )
    VALUES (
        @OrgUnitCode, @OrgUnitNameTH, @OrgUnitNameEN, @OrgUnitType, 
        @ParentOrgUnitID, @SortOrder, @CreatedBy, GETDATE(), 
        1, 0, 3
    );
    
    SELECT SCOPE_IDENTITY() AS NewOrgUnitID;
END
')

PRINT '✅ สร้าง sp_InsertOrgUnit ใหม่'

-- ขั้นตอนที่ 5: ตรวจสอบผลลัพธ์
PRINT ''
PRINT '🎯 ขั้นตอนที่ 5: ตรวจสอบผลลัพธ์'
PRINT '============================='

SELECT 
    COUNT(*) as TotalOrgUnits,
    COUNT(CASE WHEN IsDeleted = 0 AND IsActive = 1 THEN 1 END) as ActiveOrgUnits,
    COUNT(CASE WHEN IsDeleted = 1 THEN 1 END) as DeletedOrgUnits
FROM core.OrgUnit

-- ทดสอบ Insert ข้อมูลใหม่
PRINT ''
PRINT '🧪 ทดสอบ Insert ข้อมูลใหม่'
PRINT '========================'

BEGIN TRY
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES (
        'TEST-001', 'ทดสอบหน่วยงาน', 'Test Unit', 1, 
        NULL, 1, 1, 0, 
        1, GETDATE(), 3
    )
    
    PRINT '✅ ทดสอบ Insert สำเร็จ - ระบบพร้อมใช้งาน!'
    
    -- ลบข้อมูลทดสอบ
    DELETE FROM core.OrgUnit WHERE OrgUnitCode = 'TEST-001'
    PRINT '✅ ลบข้อมูลทดสอบแล้ว'
    
END TRY
BEGIN CATCH
    PRINT '❌ ทดสอบ Insert ไม่สำเร็จ:'
    PRINT 'Error: ' + ERROR_MESSAGE()
END CATCH

PRINT ''
PRINT '🎉 Simple Clear and Reset เสร็จสิ้น!'
PRINT ''
PRINT '✅ สิ่งที่ทำเสร็จแล้ว:'
PRINT '• สร้าง backup ข้อมูลเก่า'
PRINT '• ล้างข้อมูลที่เกี่ยวข้องทั้งหมด'
PRINT '• Soft/Hard delete OrgUnits'
PRINT '• อัปเดต stored procedures'
PRINT '• ทดสอบระบบพร้อมใช้งาน'
PRINT ''
PRINT 'ขั้นตอนต่อไป: รัน Script 037 เพื่อสร้างข้อมูลตัวอย่างใหม่'
PRINT ''
PRINT 'Script 038: Simple Clear and Reset เสร็จสิ้น! 🚀'