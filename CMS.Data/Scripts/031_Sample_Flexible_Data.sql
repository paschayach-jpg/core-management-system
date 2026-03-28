-- ============================================================
-- Script 031: Sample Flexible Data
-- Creates sample data to demonstrate the new flexible structure
-- ============================================================

PRINT 'Creating Sample Data for Flexible Structure...'
PRINT '=============================================='

-- Insert sample SubCommittees
PRINT 'Creating SubCommittees...'

INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
('SC-001', 'คณะกรรมการบริหารการพัฒนาบุคคล', 'Human Resource Development Committee', 1, NULL, 1, 1, 0, 1, GETDATE(), 3),
('SC-002', 'คณะกรรมการโครงการพิเศษ', 'Special Project Committee', 1, NULL, 2, 1, 0, 1, GETDATE(), 3),
('SC-003', 'คณะกรรมการกำกับดูแล', 'Governance Committee', 1, NULL, 3, 1, 0, 1, GETDATE(), 2);

PRINT 'Created 3 SubCommittees'

-- Get SubCommittee IDs for reference
DECLARE @SC1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-001')
DECLARE @SC2_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-002')
DECLARE @SC3_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-003')

-- Insert Divisions - some belong to SubCommittees, some are independent
PRINT 'Creating Divisions...'

-- Divisions belonging to SC-001 (Human Resource Development)
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
('DIV-001', 'ฝ่ายทรัพยากรบุคคล', 'Human Resources Division', 2, @SC1_ID, 1, 1, 0, 1, GETDATE(), 3),
('DIV-002', 'ฝ่ายพัฒนาบุคลากร', 'Personnel Development Division', 2, @SC1_ID, 2, 1, 0, 1, GETDATE(), 3);

-- Divisions belonging to SC-002 (Special Projects)
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
('DIV-003', 'ฝ่ายวิจัยและพัฒนา', 'Research and Development Division', 2, @SC2_ID, 1, 1, 0, 1, GETDATE(), 3),
('DIV-004', 'ฝ่ายนวัตกรรม', 'Innovation Division', 2, @SC2_ID, 2, 1, 0, 1, GETDATE(), 3);

-- Divisions belonging to SC-003 (Governance)
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
('DIV-005', 'ฝ่ายตรวจสอบภายใน', 'Internal Audit Division', 2, @SC3_ID, 1, 1, 0, 1, GETDATE(), 2);

-- Independent Divisions (flexible - can work with any SubCommittee)
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
('DIV-006', 'ฝ่ายเทคโนโลยีสารสนเทศ', 'Information Technology Division', 2, NULL, 1, 1, 0, 1, GETDATE(), 3),
('DIV-007', 'ฝ่ายกฎหมายและกำกับดูแล', 'Legal and Compliance Division', 2, NULL, 2, 1, 0, 1, GETDATE(), 2),
('DIV-008', 'ฝ่ายบัญชีและการเงิน', 'Accounting and Finance Division', 2, NULL, 3, 1, 0, 1, GETDATE(), 3);

PRINT 'Created 8 Divisions (5 with parents, 3 independent)'

-- Get Division IDs for Departments
DECLARE @DIV1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-001')
DECLARE @DIV2_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-002')
DECLARE @DIV6_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-006')
DECLARE @DIV7_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-007')
DECLARE @DIV8_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-008')

-- Insert sample Departments
PRINT 'Creating Departments...'

INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
-- Under HR Division
('DEPT-001', 'แผนกสรรหาและคัดเลือก', 'Recruitment Department', 3, @DIV1_ID, 1, 1, 0, 1, GETDATE(), 3),
('DEPT-002', 'แผนกสวัสดิการพนักงาน', 'Employee Welfare Department', 3, @DIV1_ID, 2, 1, 0, 1, GETDATE(), 3),

-- Under Personnel Development Division
('DEPT-003', 'แผนกฝึกอบรม', 'Training Department', 3, @DIV2_ID, 1, 1, 0, 1, GETDATE(), 3),
('DEPT-004', 'แผนกประเมินผลงาน', 'Performance Evaluation Department', 3, @DIV2_ID, 2, 1, 0, 1, GETDATE(), 3),

-- Under IT Division (independent)
('DEPT-005', 'แผนกพัฒนาระบบ', 'System Development Department', 3, @DIV6_ID, 1, 1, 0, 1, GETDATE(), 3),
('DEPT-006', 'แผนกโครงสร้างพื้นฐาน', 'Infrastructure Department', 3, @DIV6_ID, 2, 1, 0, 1, GETDATE(), 3),

-- Under Legal Division (independent)
('DEPT-007', 'แผนกกฎหมายธุรกิจ', 'Business Law Department', 3, @DIV7_ID, 1, 1, 0, 1, GETDATE(), 2),

-- Under Finance Division (independent)
('DEPT-008', 'แผนกบัญชี', 'Accounting Department', 3, @DIV8_ID, 1, 1, 0, 1, GETDATE(), 3),
('DEPT-009', 'แผนกการเงิน', 'Finance Department', 3, @DIV8_ID, 2, 1, 0, 1, GETDATE(), 3);

PRINT 'Created 9 Departments'

-- Show the created structure
PRINT ''
PRINT 'Sample Structure Created:'
PRINT '========================'

WITH OrgHierarchy AS (
    SELECT 
        OrgUnitID,
        OrgUnitCode,
        OrgUnitNameTH,
        OrgUnitType,
        ParentOrgUnitID,
        0 as Level,
        CAST(OrgUnitNameTH AS NVARCHAR(500)) as Path
    FROM core.OrgUnit 
    WHERE OrgUnitType = 1 AND IsDeleted = 0 AND IsActive = 1
    
    UNION ALL
    
    SELECT 
        child.OrgUnitID,
        child.OrgUnitCode,
        child.OrgUnitNameTH,
        child.OrgUnitType,
        child.ParentOrgUnitID,
        parent.Level + 1,
        parent.Path + ' → ' + child.OrgUnitNameTH
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
    END as Structure,
    OrgUnitCode
FROM OrgHierarchy
ORDER BY Level, OrgUnitNameTH;

-- Show independent divisions
PRINT ''
PRINT 'Independent Divisions:'
SELECT 
    '🔗 📁 ' + OrgUnitNameTH + ' [Division]' as IndependentStructure,
    OrgUnitCode,
    (SELECT COUNT(*) FROM core.OrgUnit dept 
     WHERE dept.ParentOrgUnitID = div.OrgUnitID 
       AND dept.OrgUnitType = 3 
       AND dept.IsDeleted = 0) as DepartmentCount
FROM core.OrgUnit div
WHERE div.OrgUnitType = 2 
  AND div.ParentOrgUnitID IS NULL 
  AND div.IsDeleted = 0 
  AND div.IsActive = 1
ORDER BY div.OrgUnitNameTH;

PRINT ''
PRINT '✅ Sample data created successfully!'
PRINT ''
PRINT 'Summary:'
SELECT 
    CASE OrgUnitType 
        WHEN 1 THEN 'SubCommittees'
        WHEN 2 THEN 'Divisions'
        WHEN 3 THEN 'Departments'
    END as Type,
    COUNT(*) as Count,
    COUNT(CASE WHEN ParentOrgUnitID IS NULL THEN 1 END) as Independent,
    COUNT(CASE WHEN ParentOrgUnitID IS NOT NULL THEN 1 END) as WithParent
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1
GROUP BY OrgUnitType
ORDER BY OrgUnitType;

PRINT 'Script 031: Sample data creation completed!'