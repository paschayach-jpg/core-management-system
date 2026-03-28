-- ============================================================
-- Script 040: Fixed Clear and Reset
-- ล้างข้อมูลและรีเซ็ตโดยไม่ใช้ columns ที่อาจไม่มี
-- ============================================================

PRINT '🚀 Fixed Clear and Reset'
PRINT '======================='
PRINT 'กำลังล้างข้อมูลและรีเซ็ตระบบ (แก้ไข column issues)...'
PRINT ''

-- ขั้นตอนที่ 1: ตรวจสอบโครงสร้างตาราง
PRINT '🔍 ขั้นตอนที่ 1: ตรวจสอบโครงสร้างตาราง'
PRINT '===================================='

-- ตรวจสอบว่าตาราง OrgUnit มีอยู่
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrgUnit' AND schema_id = SCHEMA_ID('core'))
BEGIN
    PRINT '❌ ตาราง core.OrgUnit ไม่มีอยู่!'
    PRINT 'กรุณารัน schema creation scripts ก่อน'
    RETURN
END

PRINT '✅ ตาราง core.OrgUnit มีอยู่'

-- ตรวจสอบ columns ที่จำเป็น
DECLARE @HasConfidentialityLevelValue BIT = 0
DECLARE @HasConfidentialityLevel BIT = 0

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'ConfidentialityLevelValue')
    SET @HasConfidentialityLevelValue = 1

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'ConfidentialityLevel')
    SET @HasConfidentialityLevel = 1

PRINT 'ConfidentialityLevelValue: ' + CASE WHEN @HasConfidentialityLevelValue = 1 THEN 'มี' ELSE 'ไม่มี' END
PRINT 'ConfidentialityLevel: ' + CASE WHEN @HasConfidentialityLevel = 1 THEN 'มี' ELSE 'ไม่มี' END

-- ขั้นตอนที่ 2: สร้าง backup
PRINT ''
PRINT '📦 ขั้นตอนที่ 2: สร้าง Backup'
PRINT '=========================='

DECLARE @BackupDate NVARCHAR(20) = FORMAT(GETDATE(), 'yyyyMMdd_HHmmss')

IF EXISTS (SELECT * FROM core.OrgUnit)
BEGIN
    DECLARE @BackupTable NVARCHAR(100) = 'OrgUnit_Backup_' + @BackupDate
    DECLARE @SQL NVARCHAR(MAX) = 'SELECT * INTO core.' + @BackupTable + ' FROM core.OrgUnit'
    EXEC sp_executesql @SQL
    PRINT '✅ Backup สร้างแล้ว: core.' + @BackupTable
END

-- ขั้นตอนที่ 3: ล้างข้อมูลที่เกี่ยวข้อง
PRINT ''
PRINT '🗑️ ขั้นตอนที่ 3: ล้างข้อมูลที่เกี่ยวข้อง'
PRINT '=================================='

BEGIN TRANSACTION SimpleClean

BEGIN TRY
    -- ล้าง UserAccount references
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        UPDATE core.UserAccount SET OrgUnitId = NULL WHERE OrgUnitId IS NOT NULL
        PRINT '✅ ล้าง UserAccount OrgUnit references'
    END

    -- ล้าง FolderPermission
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FolderPermission' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        DELETE FROM core.FolderPermission WHERE PrincipalType IN (3, 4)
        PRINT '✅ ล้าง FolderPermission references'
    END

    -- ล้าง FilePermission
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FilePermission' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        DELETE FROM core.FilePermission WHERE PrincipalType IN (3, 4)
        PRINT '✅ ล้าง FilePermission references'
    END

    -- ล้าง QuickAccessFolder
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

-- ขั้นตอนที่ 4: ล้าง OrgUnit data
PRINT ''
PRINT '🔄 ขั้นตอนที่ 4: ล้าง OrgUnit Data'
PRINT '=============================='

BEGIN TRY
    -- Soft Delete ก่อน
    UPDATE core.OrgUnit 
    SET IsDeleted = 1, 
        IsActive = 0,
        DeletedBy = 1,
        OrgUnitCode = OrgUnitCode + '_DEL_' + @BackupDate
    WHERE IsDeleted = 0

    PRINT '✅ Soft Delete OrgUnits สำเร็จ'

END TRY
BEGIN CATCH
    PRINT '⚠️ Soft Delete ไม่สำเร็จ, ลอง Hard Delete:'
    PRINT 'Error: ' + ERROR_MESSAGE()
    
    BEGIN TRY
        -- Hard Delete ตามลำดับ
        DELETE FROM core.OrgUnit WHERE OrgUnitType = 3 -- Departments
        DELETE FROM core.OrgUnit WHERE OrgUnitType = 2 -- Divisions  
        DELETE FROM core.OrgUnit WHERE OrgUnitType = 1 -- SubCommittees
        
        PRINT '✅ Hard Delete สำเร็จ'
        
    END TRY
    BEGIN CATCH
        PRINT '❌ Hard Delete ก็ไม่ได้:'
        PRINT 'Error: ' + ERROR_MESSAGE()
        RETURN
    END CATCH
END CATCH

-- Reset Identity Seed
IF EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID('core.OrgUnit'))
BEGIN
    DBCC CHECKIDENT ('core.OrgUnit', RESEED, 0)
    PRINT '✅ Reset Identity Seed'
END

-- ขั้นตอนที่ 5: อัปเดต Stored Procedures
PRINT ''
PRINT '🔧 ขั้นตอนที่ 5: อัปเดต Stored Procedures'
PRINT '===================================='

-- Drop sp_InsertOrgUnit เก่า
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_InsertOrgUnit') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE core.sp_InsertOrgUnit
    PRINT '✅ ลบ sp_InsertOrgUnit เก่า'
END

-- สร้าง sp_InsertOrgUnit ใหม่ (ไม่ใช้ ConfidentialityLevelValue)
DECLARE @CreateSPSQL NVARCHAR(MAX) = '
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
    
    -- Insert ข้อมูล (ไม่ใช้ ConfidentialityLevelValue)
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, CreatedBy, CreatedDate, 
        IsActive, IsDeleted'

-- เพิ่ม ConfidentialityLevel column ถ้ามี
IF @HasConfidentialityLevelValue = 1
    SET @CreateSPSQL = @CreateSPSQL + ', ConfidentialityLevelValue'
ELSE IF @HasConfidentialityLevel = 1
    SET @CreateSPSQL = @CreateSPSQL + ', ConfidentialityLevel'

SET @CreateSPSQL = @CreateSPSQL + '
    )
    VALUES (
        @OrgUnitCode, @OrgUnitNameTH, @OrgUnitNameEN, @OrgUnitType, 
        @ParentOrgUnitID, @SortOrder, @CreatedBy, GETDATE(), 
        1, 0'

-- เพิ่มค่า default สำหรับ ConfidentialityLevel
IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
    SET @CreateSPSQL = @CreateSPSQL + ', 3'

SET @CreateSPSQL = @CreateSPSQL + '
    );
    
    SELECT SCOPE_IDENTITY() AS NewOrgUnitID;
END'

EXEC sp_executesql @CreateSPSQL
PRINT '✅ สร้าง sp_InsertOrgUnit ใหม่ (แก้ไข column issues)'

-- ขั้นตอนที่ 6: ทดสอบระบบ
PRINT ''
PRINT '🧪 ขั้นตอนที่ 6: ทดสอบระบบ'
PRINT '========================'

-- สร้าง INSERT statement ที่ถูกต้อง
DECLARE @TestInsertSQL NVARCHAR(MAX) = '
INSERT INTO core.OrgUnit (
    OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
    ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
    CreatedBy, CreatedDate'

-- เพิ่ม ConfidentialityLevel column ถ้ามี
IF @HasConfidentialityLevelValue = 1
    SET @TestInsertSQL = @TestInsertSQL + ', ConfidentialityLevelValue'
ELSE IF @HasConfidentialityLevel = 1
    SET @TestInsertSQL = @TestInsertSQL + ', ConfidentialityLevel'

SET @TestInsertSQL = @TestInsertSQL + '
)
VALUES (
    ''TEST-001'', ''ทดสอบหน่วยงาน'', ''Test Unit'', 1, 
    NULL, 1, 1, 0, 
    1, GETDATE()'

-- เพิ่มค่า default สำหรับ ConfidentialityLevel
IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
    SET @TestInsertSQL = @TestInsertSQL + ', 3'

SET @TestInsertSQL = @TestInsertSQL + ')'

BEGIN TRY
    EXEC sp_executesql @TestInsertSQL
    PRINT '✅ ทดสอบ Insert สำเร็จ - ระบบพร้อมใช้งาน!'
    
    -- ลบข้อมูลทดสอบ
    DELETE FROM core.OrgUnit WHERE OrgUnitCode = 'TEST-001'
    PRINT '✅ ลบข้อมูลทดสอบแล้ว'
    
END TRY
BEGIN CATCH
    PRINT '❌ ทดสอบ Insert ไม่สำเร็จ:'
    PRINT 'Error: ' + ERROR_MESSAGE()
END CATCH

-- ตรวจสอบผลลัพธ์สุดท้าย
PRINT ''
PRINT '🎯 ผลลัพธ์สุดท้าย'
PRINT '==============='

SELECT 
    COUNT(*) as TotalOrgUnits,
    COUNT(CASE WHEN IsDeleted = 0 AND IsActive = 1 THEN 1 END) as ActiveOrgUnits,
    COUNT(CASE WHEN IsDeleted = 1 THEN 1 END) as DeletedOrgUnits
FROM core.OrgUnit

PRINT ''
PRINT '🎉 Fixed Clear and Reset เสร็จสิ้น!'
PRINT ''
PRINT '✅ สิ่งที่ทำเสร็จแล้ว:'
PRINT '• ตรวจสอบโครงสร้างตาราง'
PRINT '• สร้าง backup ข้อมูลเก่า'
PRINT '• ล้างข้อมูลที่เกี่ยวข้องทั้งหมด'
PRINT '• ล้าง OrgUnit data'
PRINT '• อัปเดต stored procedures (แก้ไข column issues)'
PRINT '• ทดสอบระบบพร้อมใช้งาน'
PRINT ''
PRINT 'ขั้นตอนต่อไป: รัน Script สร้างข้อมูลตัวอย่างใหม่'
PRINT ''
PRINT 'Script 040: Fixed Clear and Reset เสร็จสิ้น! 🚀'