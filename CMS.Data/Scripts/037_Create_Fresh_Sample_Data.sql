-- ============================================================
-- Script 037: Create Fresh Sample Data
-- สร้างข้อมูลตัวอย่างใหม่หลังจากล้างข้อมูลแล้ว
-- ============================================================

PRINT '📊 Create Fresh Sample Data'
PRINT '=========================='
PRINT 'กำลังสร้างข้อมูลตัวอย่างใหม่สำหรับโครงสร้างยืดหยุ่น...'
PRINT ''

-- ตรวจสอบว่าไม่มีข้อมูลเก่าเหลืออยู่
IF EXISTS (SELECT * FROM core.OrgUnit WHERE IsDeleted = 0 AND IsActive = 1)
BEGIN
    PRINT '⚠️ พบข้อมูล OrgUnit เก่าที่ยังไม่ได้ลบ:'
    SELECT OrgUnitCode, OrgUnitNameTH, OrgUnitType FROM core.OrgUnit WHERE IsDeleted = 0 AND IsActive = 1
    PRINT ''
    PRINT 'กรุณารัน Script 036 เพื่อล้างข้อมูลก่อน หรือดำเนินการต่อ? (Y/N)'
    -- RETURN -- ปิด comment ถ้าต้องการให้หยุดเมื่อพบข้อมูลเก่า
END

BEGIN TRANSACTION CreateSampleData

BEGIN TRY
    -- ขั้นตอนที่ 1: สร้าง SubCommittees
    PRINT '🏢 ขั้นตอนที่ 1: สร้าง SubCommittees'
    PRINT '=============================='

    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('SC-001', 'คณะกรรมการบริหารการพัฒนาบุคคล', 'Human Resource Development Committee', 1, NULL, 1, 1, 0, 1, GETDATE(), 3),
    ('SC-002', 'คณะกรรมการโครงการพิเศษ', 'Special Project Committee', 1, NULL, 2, 1, 0, 1, GETDATE(), 3),
    ('SC-003', 'คณะกรรมการกำกับดูแล', 'Governance Committee', 1, NULL, 3, 1, 0, 1, GETDATE(), 2)

    PRINT '✅ สร้าง SubCommittees: 3 หน่วย'

    -- ดึง SubCommittee IDs
    DECLARE @SC1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-001')
    DECLARE @SC2_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-002')
    DECLARE @SC3_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-003')

    -- ขั้นตอนที่ 2: สร้าง Divisions (แบบยืดหยุ่น)
    PRINT ''
    PRINT '📁 ขั้นตอนที่ 2: สร้าง Divisions (แบบยืดหยุ่น)'
    PRINT '========================================'

    -- Divisions สังกัด SC-001 (HR Development)
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DIV-001', 'ฝ่ายทรัพยากรบุคคล', 'Human Resources Division', 2, @SC1_ID, 1, 1, 0, 1, GETDATE(), 3),
    ('DIV-002', 'ฝ่ายพัฒนาบุคลากร', 'Personnel Development Division', 2, @SC1_ID, 2, 1, 0, 1, GETDATE(), 3),
    ('DIV-003', 'ฝ่ายสวัสดิการพนักงาน', 'Employee Welfare Division', 2, @SC1_ID, 3, 1, 0, 1, GETDATE(), 3)

    PRINT '✅ สร้าง Divisions สังกัด SC-001: 3 หน่วย'

    -- Divisions สังกัด SC-002 (Special Projects)
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DIV-004', 'ฝ่ายวิจัยและพัฒนา', 'Research and Development Division', 2, @SC2_ID, 1, 1, 0, 1, GETDATE(), 3),
    ('DIV-005', 'ฝ่ายนวัตกรรม', 'Innovation Division', 2, @SC2_ID, 2, 1, 0, 1, GETDATE(), 3)

    PRINT '✅ สร้าง Divisions สังกัด SC-002: 2 หน่วย'

    -- Divisions สังกัด SC-003 (Governance)
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DIV-006', 'ฝ่ายตรวจสอบภายใน', 'Internal Audit Division', 2, @SC3_ID, 1, 1, 0, 1, GETDATE(), 2),
    ('DIV-007', 'ฝ่ายบริหารความเสี่ยง', 'Risk Management Division', 2, @SC3_ID, 2, 1, 0, 1, GETDATE(), 2)

    PRINT '✅ สร้าง Divisions สังกัด SC-003: 2 หน่วย'

    -- Independent Divisions (ยืดหยุ่น - ไม่สังกัดใคร)
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DIV-008', 'ฝ่ายเทคโนโลยีสารสนเทศ', 'Information Technology Division', 2, NULL, 1, 1, 0, 1, GETDATE(), 3),
    ('DIV-009', 'ฝ่ายกฎหมายและกำกับดูแล', 'Legal and Compliance Division', 2, NULL, 2, 1, 0, 1, GETDATE(), 2),
    ('DIV-010', 'ฝ่ายบัญชีและการเงิน', 'Accounting and Finance Division', 2, NULL, 3, 1, 0, 1, GETDATE(), 3),
    ('DIV-011', 'ฝ่ายประชาสัมพันธ์', 'Public Relations Division', 2, NULL, 4, 1, 0, 1, GETDATE(), 3)

    PRINT '✅ สร้าง Independent Divisions: 4 หน่วย'

    -- ขั้นตอนที่ 3: สร้าง Departments
    PRINT ''
    PRINT '📋 ขั้นตอนที่ 3: สร้าง Departments'
    PRINT '=============================='

    -- ดึง Division IDs สำหรับ Departments
    DECLARE @DIV1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-001') -- HR
    DECLARE @DIV2_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-002') -- Personnel Dev
    DECLARE @DIV4_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-004') -- R&D
    DECLARE @DIV8_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-008') -- IT
    DECLARE @DIV9_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-009') -- Legal
    DECLARE @DIV10_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-010') -- Finance

    -- Departments under HR Division
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DEPT-001', 'แผนกสรรหาและคัดเลือก', 'Recruitment Department', 3, @DIV1_ID, 1, 1, 0, 1, GETDATE(), 3),
    ('DEPT-002', 'แผนกประเมินผลงาน', 'Performance Evaluation Department', 3, @DIV1_ID, 2, 1, 0, 1, GETDATE(), 3)

    -- Departments under Personnel Development Division
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DEPT-003', 'แผนกฝึกอบรม', 'Training Department', 3, @DIV2_ID, 1, 1, 0, 1, GETDATE(), 3),
    ('DEPT-004', 'แผนกพัฒนาทักษะ', 'Skill Development Department', 3, @DIV2_ID, 2, 1, 0, 1, GETDATE(), 3)

    -- Departments under R&D Division
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DEPT-005', 'แผนกวิจัยเทคโนโลยี', 'Technology Research Department', 3, @DIV4_ID, 1, 1, 0, 1, GETDATE(), 3)

    -- Departments under IT Division (Independent)
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DEPT-006', 'แผนกพัฒนาระบบ', 'System Development Department', 3, @DIV8_ID, 1, 1, 0, 1, GETDATE(), 3),
    ('DEPT-007', 'แผนกโครงสร้างพื้นฐาน', 'Infrastructure Department', 3, @DIV8_ID, 2, 1, 0, 1, GETDATE(), 3),
    ('DEPT-008', 'แผนกความปลอดภัยไซเบอร์', 'Cybersecurity Department', 3, @DIV8_ID, 3, 1, 0, 1, GETDATE(), 2)

    -- Departments under Legal Division (Independent)
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DEPT-009', 'แผนกกฎหมายธุรกิจ', 'Business Law Department', 3, @DIV9_ID, 1, 1, 0, 1, GETDATE(), 2),
    ('DEPT-010', 'แผนกกำกับดูแลการปฏิบัติงาน', 'Compliance Department', 3, @DIV9_ID, 2, 1, 0, 1, GETDATE(), 2)

    -- Departments under Finance Division (Independent)
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate, ConfidentialityLevelValue
    )
    VALUES 
    ('DEPT-011', 'แผนกบัญชี', 'Accounting Department', 3, @DIV10_ID, 1, 1, 0, 1, GETDATE(), 3),
    ('DEPT-012', 'แผนกการเงิน', 'Finance Department', 3, @DIV10_ID, 2, 1, 0, 1, GETDATE(), 3),
    ('DEPT-013', 'แผนกงบประมาณ', 'Budget Department', 3, @DIV10_ID, 3, 1, 0, 1, GETDATE(), 3)

    PRINT '✅ สร้าง Departments: 13 หน่วย'

    COMMIT TRANSACTION CreateSampleData
    PRINT ''
    PRINT '✅ สร้างข้อมูลตัวอย่างสำเร็จ!'

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION CreateSampleData
    PRINT '❌ เกิดข้อผิดพลาดในการสร้างข้อมูล:'
    PRINT 'Error: ' + ERROR_MESSAGE()
    PRINT 'Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10))
    RETURN
END CATCH

-- แสดงผลลัพธ์
PRINT ''
PRINT '🎯 โครงสร้างองค์กรใหม่ที่สร้างเสร็จแล้ว'
PRINT '=================================='

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
PRINT 'ฝ่ายอิสระ (ยืดหยุ่น - สามารถทำงานกับ SubCommittee ใดก็ได้):'
SELECT 
    '🔗 📁 ' + OrgUnitNameTH + ' [Division]' as [ฝ่ายอิสระ],
    OrgUnitCode as [รหัส],
    (SELECT COUNT(*) FROM core.OrgUnit dept 
     WHERE dept.ParentOrgUnitID = div.OrgUnitID 
       AND dept.OrgUnitType = 3 
       AND dept.IsDeleted = 0) as [จำนวนแผนก]
FROM core.OrgUnit div
WHERE div.OrgUnitType = 2 AND div.ParentOrgUnitID IS NULL AND div.IsDeleted = 0 AND div.IsActive = 1
ORDER BY div.OrgUnitNameTH

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
PRINT '🎉 สำเร็จ! โครงสร้างยืดหยุ่นพร้อมใช้งาน!'
PRINT ''
PRINT '✅ คุณสมบัติที่ได้:'
PRINT '• 3 SubCommittees พร้อม Divisions ย่อย'
PRINT '• 4 Independent Divisions (ยืดหยุ่น)'
PRINT '• 13 Departments กระจายตามฝ่าย'
PRINT '• รองรับ Many-to-Many relationships'
PRINT '• ป้องกัน duplicate code errors'
PRINT ''
PRINT 'Script 037: Create Fresh Sample Data เสร็จสิ้น! 🚀'