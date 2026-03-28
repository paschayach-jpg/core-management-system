-- ============================================================
-- Script 032: Complete Reset and Setup
-- Runs all necessary scripts to reset and setup flexible structure
-- ============================================================

PRINT '🚀 Complete Reset and Setup for Flexible Organization Structure'
PRINT '=============================================================='
PRINT 'This script will:'
PRINT '1. Update stored procedures (prevent duplicates)'
PRINT '2. Clear existing data (with backup)'
PRINT '3. Create sample flexible structure'
PRINT ''

-- Step 1: Update stored procedures (from Script 025)
PRINT '📝 Step 1: Updating stored procedures...'
PRINT '======================================='

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_InsertOrgUnit') AND type in (N'P', N'PC'))
    DROP PROCEDURE core.sp_InsertOrgUnit;

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
    
    -- Check for duplicate code
    IF EXISTS (SELECT 1 FROM core.OrgUnit WHERE OrgUnitCode = @OrgUnitCode AND IsDeleted = 0)
    BEGIN
        RAISERROR('รหัสหน่วยงาน %s มีอยู่ในระบบแล้ว', 16, 1, @OrgUnitCode);
        RETURN;
    END
    
    INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, CreatedBy)
    VALUES (@OrgUnitCode, @OrgUnitNameTH, @OrgUnitNameEN, @OrgUnitType, @ParentOrgUnitID, @SortOrder, @CreatedBy);
    SELECT SCOPE_IDENTITY() AS NewOrgUnitID;
END

PRINT '✅ Stored procedures updated'

-- Step 2: Clear existing data (from Script 030)
PRINT ''
PRINT '🗑️ Step 2: Clearing existing data...'
PRINT '=================================='

-- Backup current data
DECLARE @backupTable NVARCHAR(100) = 'OrgUnit_Backup_' + FORMAT(GETDATE(), 'yyyyMMddHHmmss')
IF EXISTS (SELECT * FROM core.OrgUnit)
BEGIN
    DECLARE @sql NVARCHAR(MAX) = 'SELECT * INTO core.' + @backupTable + ' FROM core.OrgUnit'
    EXEC sp_executesql @sql
    PRINT 'Backup created: core.' + @backupTable
END

-- Clear related data
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FolderPermission')
BEGIN
    DELETE FROM core.FolderPermission WHERE PrincipalType IN (3, 4);
    PRINT 'Cleared FolderPermission references'
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount')
BEGIN
    UPDATE core.UserAccount SET OrgUnitId = NULL WHERE OrgUnitId IS NOT NULL;
    PRINT 'Cleared UserAccount OrgUnit references'
END

-- Soft delete existing OrgUnits
UPDATE core.OrgUnit 
SET IsDeleted = 1, 
    IsActive = 0,
    DeletedBy = 1,
    OrgUnitCode = OrgUnitCode + '_DEL_' + FORMAT(GETDATE(), 'yyyyMMddHHmmss')
WHERE IsDeleted = 0;

-- Reset identity seed
IF EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID('core.OrgUnit'))
BEGIN
    DBCC CHECKIDENT ('core.OrgUnit', RESEED, 0);
END

PRINT '✅ Data cleared successfully'

-- Step 3: Create sample data (from Script 031)
PRINT ''
PRINT '📊 Step 3: Creating sample flexible structure...'
PRINT '=============================================='

-- Create SubCommittees
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
('SC-001', 'คณะกรรมการบริหารการพัฒนาบุคคล', 'Human Resource Development Committee', 1, NULL, 1, 1, 0, 1, GETDATE(), 3),
('SC-002', 'คณะกรรมการโครงการพิเศษ', 'Special Project Committee', 1, NULL, 2, 1, 0, 1, GETDATE(), 3);

-- Get SubCommittee IDs
DECLARE @SC1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-001')
DECLARE @SC2_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-002')

-- Create Divisions (some with parents, some independent)
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
-- Divisions under SC-001
('DIV-001', 'ฝ่ายทรัพยากรบุคคล', 'Human Resources Division', 2, @SC1_ID, 1, 1, 0, 1, GETDATE(), 3),
('DIV-002', 'ฝ่ายพัฒนาบุคลากร', 'Personnel Development Division', 2, @SC1_ID, 2, 1, 0, 1, GETDATE(), 3),

-- Divisions under SC-002
('DIV-003', 'ฝ่ายวิจัยและพัฒนา', 'Research and Development Division', 2, @SC2_ID, 1, 1, 0, 1, GETDATE(), 3),

-- Independent Divisions (flexible)
('DIV-004', 'ฝ่ายเทคโนโลยีสารสนเทศ', 'Information Technology Division', 2, NULL, 1, 1, 0, 1, GETDATE(), 3),
('DIV-005', 'ฝ่ายกฎหมายและกำกับดูแล', 'Legal and Compliance Division', 2, NULL, 2, 1, 0, 1, GETDATE(), 2);

-- Get Division IDs for Departments
DECLARE @DIV1_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-001')
DECLARE @DIV4_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-004')
DECLARE @DIV5_ID INT = (SELECT OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'DIV-005')

-- Create sample Departments
INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, ConfidentialityLevelValue)
VALUES 
('DEPT-001', 'แผนกสรรหาและคัดเลือก', 'Recruitment Department', 3, @DIV1_ID, 1, 1, 0, 1, GETDATE(), 3),
('DEPT-002', 'แผนกพัฒนาระบบ', 'System Development Department', 3, @DIV4_ID, 1, 1, 0, 1, GETDATE(), 3),
('DEPT-003', 'แผนกกฎหมายธุรกิจ', 'Business Law Department', 3, @DIV5_ID, 1, 1, 0, 1, GETDATE(), 2);

PRINT '✅ Sample data created'

-- Step 4: Show final structure
PRINT ''
PRINT '🎯 Final Result: Flexible Organization Structure'
PRINT '=============================================='

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
    END as [Flexible Organization Structure],
    OrgUnitCode as Code
FROM OrgHierarchy
ORDER BY Level, OrgUnitNameTH;

-- Show independent divisions
PRINT ''
PRINT 'Independent Divisions (Flexible):'
SELECT 
    '🔗 📁 ' + OrgUnitNameTH + ' [Division]' as [Independent Divisions],
    OrgUnitCode as Code,
    'Can work with any SubCommittee' as Status
FROM core.OrgUnit 
WHERE OrgUnitType = 2 AND ParentOrgUnitID IS NULL AND IsDeleted = 0 AND IsActive = 1
ORDER BY OrgUnitNameTH;

PRINT ''
PRINT '📊 Summary:'
SELECT 
    CASE OrgUnitType 
        WHEN 1 THEN 'SubCommittees'
        WHEN 2 THEN 'Divisions'
        WHEN 3 THEN 'Departments'
    END as [Organization Type],
    COUNT(*) as [Total Count],
    COUNT(CASE WHEN ParentOrgUnitID IS NULL THEN 1 END) as [Independent],
    COUNT(CASE WHEN ParentOrgUnitID IS NOT NULL THEN 1 END) as [With Parent]
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1
GROUP BY OrgUnitType
ORDER BY OrgUnitType;

PRINT ''
PRINT '🎉 SUCCESS! Flexible organization structure is ready!'
PRINT ''
PRINT '✅ What you can do now:'
PRINT '• 1 SubCommittee can have multiple Divisions'
PRINT '• Divisions can be independent (flexible)'
PRINT '• Departments must belong to a Division'
PRINT '• No more duplicate code errors'
PRINT '• Clean, fresh start with sample data'
PRINT ''
PRINT 'Next: Run your application and test the new flexible structure!'

PRINT ''
PRINT 'Script 032: Complete reset and setup completed successfully! 🚀'