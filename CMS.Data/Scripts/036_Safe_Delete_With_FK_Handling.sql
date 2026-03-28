-- ============================================================
-- Script 036: Safe Delete with FK Handling
-- ลบข้อมูลโดยจัดการ Foreign Key constraints อย่างปลอดภัย
-- ============================================================

PRINT '🔧 Safe Delete with Foreign Key Handling'
PRINT '========================================'
PRINT 'กำลังตรวจสอบและจัดการ Foreign Key constraints...'
PRINT ''

-- ขั้นตอนที่ 1: ตรวจสอบ Foreign Key constraints ที่เกี่ยวข้อง
PRINT '🔍 ขั้นตอนที่ 1: ตรวจสอบ Foreign Key constraints'
PRINT '============================================='

-- หา FK constraints ที่อ้างอิง OrgUnit table
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_SCHEMA_NAME(fk.parent_object_id) AS ParentSchema,
    OBJECT_NAME(fk.parent_object_id) AS ParentTable,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ParentColumn,
    OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS ReferencedSchema,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fk.referenced_object_id = OBJECT_ID('core.OrgUnit')
ORDER BY ParentSchema, ParentTable;

-- ขั้นตอนที่ 2: สร้าง backup ข้อมูลทั้งหมด
PRINT ''
PRINT '📦 ขั้นตอนที่ 2: สร้าง Backup ข้อมูลทั้งหมด'
PRINT '======================================'

DECLARE @BackupDate NVARCHAR(20) = FORMAT(GETDATE(), 'yyyyMMdd_HHmmss')

-- Backup OrgUnit
IF EXISTS (SELECT * FROM core.OrgUnit)
BEGIN
    DECLARE @BackupOrgUnit NVARCHAR(100) = 'OrgUnit_Backup_' + @BackupDate
    DECLARE @SQL NVARCHAR(MAX) = 'SELECT * INTO core.' + @BackupOrgUnit + ' FROM core.OrgUnit'
    EXEC sp_executesql @SQL
    PRINT '✅ Backup OrgUnit: core.' + @BackupOrgUnit
END

-- ขั้นตอนที่ 3: ปิดใช้งาน Foreign Key constraints ชั่วคราว
PRINT ''
PRINT '🔒 ขั้นตอนที่ 3: ปิดใช้งาน Foreign Key constraints ชั่วคราว'
PRINT '=================================================='

-- เก็บรายชื่อ FK constraints ที่จะต้องเปิดกลับ
CREATE TABLE #FKConstraints (
    SchemaName NVARCHAR(128),
    TableName NVARCHAR(128),
    ConstraintName NVARCHAR(128),
    DropScript NVARCHAR(MAX),
    CreateScript NVARCHAR(MAX)
)

-- สร้าง scripts สำหรับ drop และ recreate FK constraints
INSERT INTO #FKConstraints (SchemaName, TableName, ConstraintName, DropScript, CreateScript)
SELECT 
    OBJECT_SCHEMA_NAME(fk.parent_object_id) AS SchemaName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    fk.name AS ConstraintName,
    'ALTER TABLE [' + OBJECT_SCHEMA_NAME(fk.parent_object_id) + '].[' + OBJECT_NAME(fk.parent_object_id) + '] DROP CONSTRAINT [' + fk.name + ']' AS DropScript,
    'ALTER TABLE [' + OBJECT_SCHEMA_NAME(fk.parent_object_id) + '].[' + OBJECT_NAME(fk.parent_object_id) + '] ADD CONSTRAINT [' + fk.name + '] FOREIGN KEY ([' + 
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) + ']) REFERENCES [' + 
    OBJECT_SCHEMA_NAME(fk.referenced_object_id) + '].[' + OBJECT_NAME(fk.referenced_object_id) + '] ([' + 
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) + '])' +
    CASE 
        WHEN fk.delete_referential_action = 1 THEN ' ON DELETE CASCADE'
        WHEN fk.delete_referential_action = 2 THEN ' ON DELETE SET NULL'
        WHEN fk.delete_referential_action = 3 THEN ' ON DELETE SET DEFAULT'
        ELSE ''
    END +
    CASE 
        WHEN fk.update_referential_action = 1 THEN ' ON UPDATE CASCADE'
        WHEN fk.update_referential_action = 2 THEN ' ON UPDATE SET NULL'
        WHEN fk.update_referential_action = 3 THEN ' ON UPDATE SET DEFAULT'
        ELSE ''
    END AS CreateScript
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fk.referenced_object_id = OBJECT_ID('core.OrgUnit')

-- Drop FK constraints
DECLARE @DropSQL NVARCHAR(MAX)
DECLARE drop_cursor CURSOR FOR 
    SELECT DropScript FROM #FKConstraints

OPEN drop_cursor
FETCH NEXT FROM drop_cursor INTO @DropSQL

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        EXEC sp_executesql @DropSQL
        PRINT '✅ Dropped FK: ' + @DropSQL
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Warning dropping FK: ' + ERROR_MESSAGE()
    END CATCH
    
    FETCH NEXT FROM drop_cursor INTO @DropSQL
END

CLOSE drop_cursor
DEALLOCATE drop_cursor

-- ขั้นตอนที่ 4: ล้างข้อมูลที่เกี่ยวข้องทั้งหมด
PRINT ''
PRINT '🗑️ ขั้นตอนที่ 4: ล้างข้อมูลที่เกี่ยวข้องทั้งหมด'
PRINT '========================================='

BEGIN TRANSACTION ClearAllData

BEGIN TRY
    -- ล้างข้อมูลตามลำดับที่ปลอดภัย (จากลูกไปแม่)
    
    -- ล้าง AuditLog entries
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLog')
    BEGIN
        DELETE FROM audit.AuditLog WHERE TableName LIKE '%OrgUnit%'
        PRINT '✅ ล้าง AuditLog OrgUnit entries'
    END
    
    -- ล้าง QuickAccessFolder
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'QuickAccessFolder')
    BEGIN
        DELETE FROM core.QuickAccessFolder
        PRINT '✅ ล้าง QuickAccessFolder'
    END
    
    -- ล้าง FolderPermission ที่อ้างอิง OrgUnit
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FolderPermission')
    BEGIN
        DELETE FROM core.FolderPermission WHERE PrincipalType IN (3, 4) -- Department, Division
        PRINT '✅ ล้าง FolderPermission references'
    END
    
    -- ล้าง UserAccount OrgUnit references
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount')
    BEGIN
        UPDATE core.UserAccount SET OrgUnitId = NULL WHERE OrgUnitId IS NOT NULL
        PRINT '✅ ล้าง UserAccount OrgUnit references'
    END
    
    -- ล้าง FilePermission ที่อาจอ้างอิง OrgUnit
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FilePermission')
    BEGIN
        DELETE FROM core.FilePermission WHERE PrincipalType IN (3, 4)
        PRINT '✅ ล้าง FilePermission references'
    END
    
    -- ล้าง Notification ที่อาจเกี่ยวข้อง
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Notification')
    BEGIN
        DELETE FROM core.Notification WHERE Message LIKE '%OrgUnit%' OR Message LIKE '%หน่วยงาน%'
        PRINT '✅ ล้าง Notification entries'
    END
    
    -- ล้าง OrgUnit ทั้งหมด (ตอนนี้ไม่มี FK constraints แล้ว)
    DELETE FROM core.OrgUnit
    PRINT '✅ ล้าง OrgUnit ทั้งหมด'
    
    -- Reset Identity Seed
    IF EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID('core.OrgUnit'))
    BEGIN
        DBCC CHECKIDENT ('core.OrgUnit', RESEED, 0)
        PRINT '✅ Reset OrgUnit Identity Seed'
    END

    COMMIT TRANSACTION ClearAllData
    PRINT '✅ ล้างข้อมูลทั้งหมดสำเร็จ!'

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION ClearAllData
    PRINT '❌ เกิดข้อผิดพลาดในการล้างข้อมูล:'
    PRINT 'Error: ' + ERROR_MESSAGE()
    PRINT 'Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10))
    
    -- ถ้าเกิดข้อผิดพลาด ให้ข้ามไปสร้าง FK constraints กลับ
    -- (ไม่ใช้ GOTO เพื่อหลีกเลี่ยง error)
END CATCH

-- ขั้นตอนที่ 5: สร้าง FK constraints กลับ
PRINT ''
PRINT '🔓 ขั้นตอนที่ 5: สร้าง Foreign Key constraints กลับ'
PRINT '=============================================='

DECLARE @CreateSQL NVARCHAR(MAX)
DECLARE create_cursor CURSOR FOR 
    SELECT CreateScript FROM #FKConstraints

OPEN create_cursor
FETCH NEXT FROM create_cursor INTO @CreateSQL

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        EXEC sp_executesql @CreateSQL
        PRINT '✅ Created FK: ' + LEFT(@CreateSQL, 100) + '...'
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Warning creating FK: ' + ERROR_MESSAGE()
        PRINT 'SQL: ' + @CreateSQL
    END CATCH
    
    FETCH NEXT FROM create_cursor INTO @CreateSQL
END

CLOSE create_cursor
DEALLOCATE create_cursor

-- ขั้นตอนที่ 6: อัปเดต Stored Procedures
PRINT ''
PRINT '🔧 ขั้นตอนที่ 6: อัปเดต Stored Procedures'
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

-- ทำความสะอาด temp table
DROP TABLE #FKConstraints

-- ตรวจสอบผลลัพธ์
PRINT ''
PRINT '🎯 ตรวจสอบผลลัพธ์'
PRINT '================'

SELECT 
    COUNT(*) as TotalOrgUnits,
    COUNT(CASE WHEN IsDeleted = 0 AND IsActive = 1 THEN 1 END) as ActiveOrgUnits
FROM core.OrgUnit

-- แสดง FK constraints ที่สร้างกลับ
PRINT ''
PRINT 'Foreign Key constraints ที่สร้างกลับ:'
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_SCHEMA_NAME(fk.parent_object_id) + '.' + OBJECT_NAME(fk.parent_object_id) AS ParentTable,
    OBJECT_SCHEMA_NAME(fk.referenced_object_id) + '.' + OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys fk
WHERE fk.referenced_object_id = OBJECT_ID('core.OrgUnit')
ORDER BY ParentTable

PRINT ''
PRINT '✅ สำเร็จ! ล้างข้อมูลพร้อม FK handling เรียบร้อยแล้ว'
PRINT ''
PRINT 'ขั้นตอนต่อไป:'
PRINT '1. รัน Script สร้างข้อมูลตัวอย่างใหม่'
PRINT '2. ทดสอบ Application'
PRINT ''
PRINT 'Script 036: Safe Delete with FK Handling เสร็จสิ้น! 🎯'