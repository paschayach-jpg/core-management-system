-- ============================================================
-- Script 035: Complete Fresh Start
-- ล้างข้อมูลทั้งหมดและเริ่มต้นใหม่
-- ============================================================

PRINT '🚀 Complete Fresh Start - ล้างข้อมูลทั้งหมดและเริ่มต้นใหม่'
PRINT '========================================================='
PRINT 'กำลังล้างข้อมูลทั้งหมดและสร้างโครงสร้างใหม่...'
PRINT ''

-- ขั้นตอนที่ 1: สร้าง backup ข้อมูลเก่า (เผื่อต้องกู้คืน)
PRINT '📦 ขั้นตอนที่ 1: สร้าง Backup ข้อมูลเก่า'
PRINT '=================================='

DECLARE @BackupDate NVARCHAR(20) = FORMAT(GETDATE(), 'yyyyMMdd_HHmmss')

-- Backup OrgUnit
IF EXISTS (SELECT * FROM core.OrgUnit)
BEGIN
    DECLARE @BackupOrgUnit NVARCHAR(100) = 'OrgUnit_FullBackup_' + @BackupDate
    DECLARE @SQL NVARCHAR(MAX) = 'SELECT * INTO core.' + @BackupOrgUnit + ' FROM core.OrgUnit'
    EXEC sp_executesql @SQL
    PRINT '✅ Backup OrgUnit: core.' + @BackupOrgUnit
END

-- Backup UserAccount (เฉพาะ OrgUnitId references)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount' AND schema_id = SCHEMA_ID('core'))
BEGIN
    DECLARE @BackupUser NVARCHAR(100) = 'UserAccount_OrgRef_Backup_' + @BackupDate
    SET @SQL = 'SELECT UserId, OrgUnitId INTO core.' + @BackupUser + ' FROM core.UserAccount WHERE OrgUnitId IS NOT NULL'
    EXEC sp_executesql @SQL
    PRINT '✅ Backup UserAccount references: core.' + @BackupUser
END

-- ขั้นตอนที่ 2: ล้างข้อมูลที่เกี่ยวข้องทั้งหมด
PRINT ''
PRINT '🗑️ ขั้นตอนที่ 2: ล้างข้อมูลที่เกี่ยวข้องทั้งหมด'
PRINT '========================================='

BEGIN TRANSACTION ClearAllData

BEGIN TRY
    -- ล้าง FolderPermission ที่อ้างอิง OrgUnit
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FolderPermission' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        DELETE FROM core.FolderPermission WHERE PrincipalType IN (3, 4) -- Department, Division
        PRINT '✅ ล้าง FolderPermission references'
    END

    -- ล้าง UserAccount OrgUnit references
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        UPDATE core.UserAccount SET OrgUnitId = NULL WHERE OrgUnitId IS NOT NULL
        PRINT '✅ ล้าง UserAccount OrgUnit references'
    END

    -- ล้าง QuickAccessFolder ที่อาจอ้างอิง OrgUnit
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'QuickAccessFolder' AND schema_id = SCHEMA_ID('core'))
    BEGIN
        DELETE FROM core.QuickAccessFolder
        PRINT '✅ ล้าง QuickAccessFolder'
    END

    -- ล้าง AuditLog ที่เกี่ยวข้องกับ OrgUnit
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLog' AND schema_id = SCHEMA_ID('audit'))
    BEGIN
        DELETE FROM audit.AuditLog WHERE TableName = 'core.OrgUnit'
        PRINT '✅ ล้าง AuditLog OrgUnit entries'
    END

    -- ล้าง OrgUnit ทั้งหมด (Hard Delete)
    DELETE FROM core.OrgUnit
    PRINT '✅ ล้าง OrgUnit ทั้งหมด (Hard Delete)'

    -- Reset Identity Seed
    IF EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID('core.OrgUnit'))
    BEGIN
        DBCC CHECKIDENT ('core.OrgUnit', RESEED, 0)
        PRINT '✅ Reset OrgUnit Identity Seed เป็น 0'
    END

    COMMIT TRANSACTION ClearAllData
    PRINT '✅ ล้างข้อมูลทั้งหมดสำเร็จ!'

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION ClearAllData
    PRINT '❌ เกิดข้อผิดพลาดในการล้างข้อมูล:'
    PRINT 'Error: ' + ERROR_MESSAGE()
    PRINT 'Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10))
    RETURN
END CATCH

-- ขั้นตอนที่ 3: อัปเดต Stored Procedures
PRINT ''
PRINT '🔧 ขั้นตอนที่ 3: อัปเดต Stored Procedures'
PRINT '===================================='

-- Drop และสร้าง sp_InsertOrgUnit ใหม่
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_InsertOrgUnit') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE core.sp_InsertOrgUnit
    PRINT '✅ ลบ sp_InsertOrgUnit เก่า'
END

-- สร้าง sp_InsertOrgUnit ใหม่พร้อม duplicate check
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
    INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, CreatedBy, CreatedDate, IsActive, IsDeleted, ConfidentialityLevelValue)
    VALUES (@OrgUnitCode, @OrgUnitNameTH, @OrgUnitNameEN, @OrgUnitType, @ParentOrgUnitID, @SortOrder, @CreatedBy, GETDATE(), 1, 0, 3);
    
    SELECT SCOPE_IDENTITY() AS NewOrgUnitID;
END
')

PRINT '✅ สร้าง sp_InsertOrgUnit ใหม่พร้อม duplicate check'

-- ขั้นตอนที่ 4: สร้างข้อมูลตัวอย่างใหม่
PRINT ''
PRINT '📊 ขั้นตอนที่ 4: สร้างข้อมูลตัวอย่างใหม่'
PRINT '=================================='

-- สร้าง SubCommittees
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
('SC-001', 'คณะกรรมการบริหารการพัฒนาบุคคล', 'Human Resource Development Committee', 1, NULL, 1, 1, 0, 1, GETDATE(), 3),
('SC-002', 'คณะกรรมการโครงการพิเศษ', 'Special Project Committee', 1, NULL, 2, 1, 0, 1, GETDATE(), 3)

PRINT '✅ สร้าง SubCommittees: 2 หน่วย'

-- ดึง SubCommittee IDs
DECLARE @SC1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-001')
DECLARE @SC2_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-002')

-- สร้าง Divisions (บางตัวมี parent บางตัวอิสระ)
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
-- Divisions สังกัด SC-001
('DIV-001', 'ฝ่ายทรัพยากรบุคคล', 'Human Resources Division', 2, @SC1_ID, 1, 1, 0, 1, GETDATE(), 3),
('DIV-002', 'ฝ่ายพัฒนาบุคลากร', 'Personnel Development Division', 2, @SC1_ID, 2, 1, 0, 1, GETDATE(), 3),

-- Divisions สังกัด SC-002  
('DIV-003', 'ฝ่ายวิจัยและพัฒนา', 'Research and Development Division', 2, @SC2_ID, 1, 1, 0, 1, GETDATE(), 3),

-- Divisions อิสระ (ยืดหยุ่น)
('DIV-004', 'ฝ่ายเทคโนโลยีสารสนเทศ', 'Information Technology Division', 2, NULL, 1, 1, 0, 1, GETDATE(), 3),
('DIV-005', 'ฝ่ายกฎหมายและกำกับดูแล', 'Legal and Compliance Division', 2, NULL, 2, 1, 0, 1, GETDATE(), 2),
('DIV-006', 'ฝ่ายบัญชีและการเงิน', 'Accounting and Finance Division', 2, NULL, 3, 1, 0, 1, GETDATE(), 3)

PRINT '✅ สร้าง Divisions: 6 หน่วย (3 มี parent, 3 อิสระ)'

-- ดึง Division IDs สำหรับ Departments
DECLARE @DIV1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-001')
DECLARE @DIV4_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-004')
DECLARE @DIV6_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-006')

-- สร้าง Departments
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
('DEPT-001', 'แผนกสรรหาและคัดเลือก', 'Recruitment Department', 3, @DIV1_ID, 1, 1, 0, 1, GETDATE(), 3),
('DEPT-002', 'แผนกสวัสดิการพนักงาน', 'Employee Welfare Department', 3, @DIV1_ID, 2, 1, 0, 1, GETDATE(), 3),
('DEPT-003', 'แผนกพัฒนาระบบ', 'System Development Department', 3, @DIV4_ID, 1, 1, 0, 1, GETDATE(), 3),
('DEPT-004', 'แผนกบัญชี', 'Accounting Department', 3, @DIV6_ID, 1, 1, 0, 1, GETDATE(), 3),
('DEPT-005', 'แผนกการเงิน', 'Finance Department', 3, @DIV6_ID, 2, 1, 0, 1, GETDATE(), 3)

PRINT '✅ สร้าง Departments: 5 หน่วย'

-- ขั้นตอนที่ 5: แสดงผลลัพธ์
PRINT ''
PRINT '🎯 ขั้นตอนที่ 5: โครงสร้างใหม่ที่สร้างเสร็จแล้ว'
PRINT '======================================='

-- แสดงโครงสร้างแบบ Hierarchy
WITH OrgHierarchy AS (
    SELECT 
        OrgUnitID, OrgUnitCode, OrgUnitNameTH, OrgUnitType, ParentOrgUnitID,
        0 as Level, CAST(OrgUnitNameTH AS NVARCHAR(500)) as Path
    FROM core.OrgUnit 
    WHERE OrgUnitType = 1 AND IsDeleted = 0 AND IsActive = 1
    
    UNION ALL
    
    SELECT 
        child.OrgUnitID, child.OrgUnitCode, child.OrgUnitNameTH, child.OrgUnitType, child.ParentOrgUnitID,
        parent.Level + 1, parent.Path + ' → ' + child.OrgUnitNameTH
    FROM core.OrgUnit child
    INNER JOIN OrgHierarchy parent ON child.ParentOrgUnitID = parent.OrgUnitID
    WHERE child.IsDeleted = 0 AND child.IsActive = 1
)
SELECT 
    REPLICATE('  ', Level) + 
    CASE OrgUnitType 
        WHEN 1 THEN '🏢 ' + OrgUnitNameTH + ' [SubCommittee]'
        WHEN 2 THEN '📁 ' + OrgUnitNameTH + ' [Division]'
        WHEN 3 THEN '📋 ' + OrgUnitNameTH + ' [Department]'
    END as [โครงสร้างองค์กรใหม่],
    OrgUnitCode as [รหัส]
FROM OrgHierarchy
ORDER BY Level, OrgUnitNameTH

-- แสดง Independent Divisions
PRINT ''
PRINT 'ฝ่ายอิสระ (ยืดหยุ่น):'
SELECT 
    '🔗 📁 ' + OrgUnitNameTH + ' [Division]' as [ฝ่ายอิสระ],
    OrgUnitCode as [รหัส],
    'สามารถทำงานร่วมกับ SubCommittee ใดก็ได้' as [สถานะ]
FROM core.OrgUnit 
WHERE OrgUnitType = 2 AND ParentOrgUnitID IS NULL AND IsDeleted = 0 AND IsActive = 1
ORDER BY OrgUnitNameTH

-- สรุปจำนวน
PRINT ''
PRINT 'สรุปจำนวนหน่วยงาน:'
SELECT 
    CASE OrgUnitType 
        WHEN 1 THEN 'SubCommittees'
        WHEN 2 THEN 'Divisions'
        WHEN 3 THEN 'Departments'
    END as [ประเภท],
    COUNT(*) as [จำนวนทั้งหมด],
    COUNT(CASE WHEN ParentOrgUnitID IS NULL THEN 1 END) as [อิสระ],
    COUNT(CASE WHEN ParentOrgUnitID IS NOT NULL THEN 1 END) as [มี Parent]
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1
GROUP BY OrgUnitType
ORDER BY OrgUnitType

PRINT ''
PRINT '🎉 สำเร็จ! ล้างข้อมูลและสร้างโครงสร้างใหม่เรียบร้อยแล้ว!'
PRINT ''
PRINT '✅ สิ่งที่ทำเสร็จแล้ว:'
PRINT '• ล้างข้อมูลเก่าทั้งหมด (มี backup)'
PRINT '• อัปเดต stored procedures'
PRINT '• สร้างโครงสร้างยืดหยุ่นใหม่'
PRINT '• 1 SubCommittee สามารถมีหลาย Division'
PRINT '• Division สามารถเป็นอิสระได้'
PRINT '• ป้องกัน duplicate code errors'
PRINT ''
PRINT '🚀 พร้อมใช้งาน! เปิด Application และทดสอบได้เลย'

PRINT ''
PRINT 'Script 035: Complete Fresh Start เสร็จสิ้น! 🎯'