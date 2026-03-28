-- ============================================================
-- Script 041: Fixed Sample Data
-- สร้างข้อมูลตัวอย่างโดยไม่ใช้ columns ที่อาจไม่มี
-- ============================================================

PRINT '📊 Fixed Sample Data Creation'
PRINT '============================'
PRINT 'กำลังสร้างข้อมูลตัวอย่างใหม่ (แก้ไข column issues)...'
PRINT ''

-- ตรวจสอบโครงสร้างตาราง
PRINT '🔍 ตรวจสอบโครงสร้างตาราง'
PRINT '======================'

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrgUnit' AND schema_id = SCHEMA_ID('core'))
BEGIN
    PRINT '❌ ตาราง core.OrgUnit ไม่มีอยู่!'
    RETURN
END

-- ตรวจสอบ columns ที่จำเป็น
DECLARE @HasConfidentialityLevelValue BIT = 0
DECLARE @HasConfidentialityLevel BIT = 0

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'ConfidentialityLevelValue')
    SET @HasConfidentialityLevelValue = 1

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'ConfidentialityLevel')
    SET @HasConfidentialityLevel = 1

PRINT '✅ ตาราง core.OrgUnit มีอยู่'
PRINT 'ConfidentialityLevelValue: ' + CASE WHEN @HasConfidentialityLevelValue = 1 THEN 'มี' ELSE 'ไม่มี' END
PRINT 'ConfidentialityLevel: ' + CASE WHEN @HasConfidentialityLevel = 1 THEN 'มี' ELSE 'ไม่มี' END

-- ตรวจสอบว่าไม่มีข้อมูลเก่า
IF EXISTS (SELECT * FROM core.OrgUnit WHERE IsDeleted = 0 AND IsActive = 1)
BEGIN
    PRINT ''
    PRINT '⚠️ พบข้อมูล OrgUnit เก่าที่ยังไม่ได้ลบ:'
    SELECT OrgUnitCode, OrgUnitNameTH, OrgUnitType FROM core.OrgUnit WHERE IsDeleted = 0 AND IsActive = 1
    PRINT 'กรุณารัน Script 040 เพื่อล้างข้อมูลก่อน'
    -- RETURN -- ปิด comment ถ้าต้องการให้หยุดเมื่อพบข้อมูลเก่า
END

BEGIN TRANSACTION CreateSampleData

BEGIN TRY
    -- สร้าง base INSERT statement
    DECLARE @BaseInsertSQL NVARCHAR(MAX) = '
    INSERT INTO core.OrgUnit (
        OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, 
        ParentOrgUnitID, SortOrder, IsActive, IsDeleted, 
        CreatedBy, CreatedDate'

    -- เพิ่ม ConfidentialityLevel column ถ้ามี
    IF @HasConfidentialityLevelValue = 1
        SET @BaseInsertSQL = @BaseInsertSQL + ', ConfidentialityLevelValue'
    ELSE IF @HasConfidentialityLevel = 1
        SET @BaseInsertSQL = @BaseInsertSQL + ', ConfidentialityLevel'

    SET @BaseInsertSQL = @BaseInsertSQL + ') VALUES '

    -- ขั้นตอนที่ 1: สร้าง SubCommittees
    PRINT ''
    PRINT '🏢 ขั้นตอนที่ 1: สร้าง SubCommittees'
    PRINT '=============================='

    DECLARE @SubCommitteeSQL NVARCHAR(MAX) = @BaseInsertSQL + '
    (''SC-001'', ''คณะกรรมการบริหารการพัฒนาบุคคล'', ''Human Resource Development Committee'', 1, NULL, 1, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @SubCommitteeSQL = @SubCommitteeSQL + ', 3'
    
    SET @SubCommitteeSQL = @SubCommitteeSQL + '),
    (''SC-002'', ''คณะกรรมการโครงการพิเศษ'', ''Special Project Committee'', 1, NULL, 2, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @SubCommitteeSQL = @SubCommitteeSQL + ', 3'
    
    SET @SubCommitteeSQL = @SubCommitteeSQL + '),
    (''SC-003'', ''คณะกรรมการกำกับดูแล'', ''Governance Committee'', 1, NULL, 3, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @SubCommitteeSQL = @SubCommitteeSQL + ', 2'
    
    SET @SubCommitteeSQL = @SubCommitteeSQL + ')'

    EXEC sp_executesql @SubCommitteeSQL
    PRINT '✅ สร้าง SubCommittees: 3 หน่วย'

    -- ดึง SubCommittee IDs
    DECLARE @SC1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-001')
    DECLARE @SC2_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-002')
    DECLARE @SC3_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-003')

    -- ขั้นตอนที่ 2: สร้าง Divisions
    PRINT ''
    PRINT '📁 ขั้นตอนที่ 2: สร้าง Divisions'
    PRINT '============================'

    -- Divisions สังกัด SC-001
    DECLARE @DivisionSQL1 NVARCHAR(MAX) = @BaseInsertSQL + '
    (''DIV-001'', ''ฝ่ายทรัพยากรบุคคล'', ''Human Resources Division'', 2, ' + CAST(@SC1_ID AS NVARCHAR(10)) + ', 1, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @DivisionSQL1 = @DivisionSQL1 + ', 3'
    
    SET @DivisionSQL1 = @DivisionSQL1 + '),
    (''DIV-002'', ''ฝ่ายพัฒนาบุคลากร'', ''Personnel Development Division'', 2, ' + CAST(@SC1_ID AS NVARCHAR(10)) + ', 2, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @DivisionSQL1 = @DivisionSQL1 + ', 3'
    
    SET @DivisionSQL1 = @DivisionSQL1 + ')'

    EXEC sp_executesql @DivisionSQL1
    PRINT '✅ สร้าง Divisions สังกัด SC-001: 2 หน่วย'

    -- Divisions สังกัด SC-002
    DECLARE @DivisionSQL2 NVARCHAR(MAX) = @BaseInsertSQL + '
    (''DIV-003'', ''ฝ่ายวิจัยและพัฒนา'', ''Research and Development Division'', 2, ' + CAST(@SC2_ID AS NVARCHAR(10)) + ', 1, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @DivisionSQL2 = @DivisionSQL2 + ', 3'
    
    SET @DivisionSQL2 = @DivisionSQL2 + '),
    (''DIV-004'', ''ฝ่ายนวัตกรรม'', ''Innovation Division'', 2, ' + CAST(@SC2_ID AS NVARCHAR(10)) + ', 2, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @DivisionSQL2 = @DivisionSQL2 + ', 3'
    
    SET @DivisionSQL2 = @DivisionSQL2 + ')'

    EXEC sp_executesql @DivisionSQL2
    PRINT '✅ สร้าง Divisions สังกัด SC-002: 2 หน่วย'

    -- Independent Divisions
    DECLARE @IndependentDivSQL NVARCHAR(MAX) = @BaseInsertSQL + '
    (''DIV-005'', ''ฝ่ายเทคโนโลยีสารสนเทศ'', ''Information Technology Division'', 2, NULL, 1, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @IndependentDivSQL = @IndependentDivSQL + ', 3'
    
    SET @IndependentDivSQL = @IndependentDivSQL + '),
    (''DIV-006'', ''ฝ่ายกฎหมายและกำกับดูแล'', ''Legal and Compliance Division'', 2, NULL, 2, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @IndependentDivSQL = @IndependentDivSQL + ', 2'
    
    SET @IndependentDivSQL = @IndependentDivSQL + '),
    (''DIV-007'', ''ฝ่ายบัญชีและการเงิน'', ''Accounting and Finance Division'', 2, NULL, 3, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @IndependentDivSQL = @IndependentDivSQL + ', 3'
    
    SET @IndependentDivSQL = @IndependentDivSQL + ')'

    EXEC sp_executesql @IndependentDivSQL
    PRINT '✅ สร้าง Independent Divisions: 3 หน่วย'

    -- ขั้นตอนที่ 3: สร้าง Departments
    PRINT ''
    PRINT '📋 ขั้นตอนที่ 3: สร้าง Departments'
    PRINT '=============================='

    -- ดึง Division IDs
    DECLARE @DIV1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-001')
    DECLARE @DIV2_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-002')
    DECLARE @DIV5_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-005')
    DECLARE @DIV7_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-007')

    -- Departments
    DECLARE @DepartmentSQL NVARCHAR(MAX) = @BaseInsertSQL + '
    (''DEPT-001'', ''แผนกสรรหาและคัดเลือก'', ''Recruitment Department'', 3, ' + CAST(@DIV1_ID AS NVARCHAR(10)) + ', 1, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @DepartmentSQL = @DepartmentSQL + ', 3'
    
    SET @DepartmentSQL = @DepartmentSQL + '),
    (''DEPT-002'', ''แผนกฝึกอบรม'', ''Training Department'', 3, ' + CAST(@DIV2_ID AS NVARCHAR(10)) + ', 1, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @DepartmentSQL = @DepartmentSQL + ', 3'
    
    SET @DepartmentSQL = @DepartmentSQL + '),
    (''DEPT-003'', ''แผนกพัฒนาระบบ'', ''System Development Department'', 3, ' + CAST(@DIV5_ID AS NVARCHAR(10)) + ', 1, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @DepartmentSQL = @DepartmentSQL + ', 3'
    
    SET @DepartmentSQL = @DepartmentSQL + '),
    (''DEPT-004'', ''แผนกบัญชี'', ''Accounting Department'', 3, ' + CAST(@DIV7_ID AS NVARCHAR(10)) + ', 1, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @DepartmentSQL = @DepartmentSQL + ', 3'
    
    SET @DepartmentSQL = @DepartmentSQL + '),
    (''DEPT-005'', ''แผนกการเงิน'', ''Finance Department'', 3, ' + CAST(@DIV7_ID AS NVARCHAR(10)) + ', 2, 1, 0, 1, GETDATE()'
    
    IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
        SET @DepartmentSQL = @DepartmentSQL + ', 3'
    
    SET @DepartmentSQL = @DepartmentSQL + ')'

    EXEC sp_executesql @DepartmentSQL
    PRINT '✅ สร้าง Departments: 5 หน่วย'

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
        0 as Level
    FROM core.OrgUnit 
    WHERE OrgUnitType = 1 AND IsDeleted = 0 AND IsActive = 1
    
    UNION ALL
    
    SELECT 
        child.OrgUnitID, child.OrgUnitCode, child.OrgUnitNameTH, child.OrgUnitType, child.ParentOrgUnitID,
        parent.Level + 1
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
    OrgUnitCode as [รหัส]
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
PRINT '🎉 สำเร็จ! โครงสร้างยืดหยุ่นพร้อมใช้งาน!'
PRINT ''
PRINT '✅ คุณสมบัติที่ได้:'
PRINT '• 3 SubCommittees พร้อม Divisions ย่อย'
PRINT '• 3 Independent Divisions (ยืดหยุ่น)'
PRINT '• 5 Departments กระจายตามฝ่าย'
PRINT '• รองรับ Many-to-Many relationships'
PRINT '• แก้ไข column compatibility issues'
PRINT ''
PRINT 'Script 041: Fixed Sample Data เสร็จสิ้น! 🚀'