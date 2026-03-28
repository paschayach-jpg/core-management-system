-- ============================================================
-- Script 034: Simple Test Insert
-- Tests basic insert functionality step by step
-- ============================================================

PRINT 'Simple Test Insert Process'
PRINT '========================='

-- Check table structure first
PRINT 'Checking table structure...'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit'
ORDER BY ORDINAL_POSITION;

-- Test 1: Insert a simple SubCommittee
PRINT ''
PRINT 'Test 1: Inserting SubCommittee...'
BEGIN TRY
    INSERT INTO core.OrgUnit (
        OrgUnitCode, 
        OrgUnitNameTH, 
        OrgUnitNameEN, 
        OrgUnitType, 
        ParentOrgUnitID, 
        SortOrder, 
        IsActive, 
        IsDeleted, 
        CreatedBy, 
        CreatedDate,
        ConfidentialityLevelValue
    )
    VALUES (
        'SC-TEST', 
        'ทดสอบคณะกรรมการ', 
        'Test Committee', 
        1, 
        NULL, 
        1, 
        1, 
        0, 
        1, 
        GETDATE(),
        3
    );
    
    PRINT '✅ SubCommittee inserted successfully'
    
    -- Get the inserted ID
    DECLARE @TestSC_ID INT = SCOPE_IDENTITY()
    PRINT 'Inserted SubCommittee ID: ' + CAST(@TestSC_ID AS NVARCHAR(10))
    
END TRY
BEGIN CATCH
    PRINT '❌ Error inserting SubCommittee:'
    PRINT 'Error: ' + ERROR_MESSAGE()
END CATCH

-- Test 2: Insert a Division with parent
PRINT ''
PRINT 'Test 2: Inserting Division with parent...'
BEGIN TRY
    DECLARE @ParentID INT = (SELECT TOP 1 OrgUnitID FROM core.OrgUnit WHERE OrgUnitCode = 'SC-TEST')
    
    INSERT INTO core.OrgUnit (
        OrgUnitCode, 
        OrgUnitNameTH, 
        OrgUnitNameEN, 
        OrgUnitType, 
        ParentOrgUnitID, 
        SortOrder, 
        IsActive, 
        IsDeleted, 
        CreatedBy, 
        CreatedDate,
        ConfidentialityLevelValue
    )
    VALUES (
        'DIV-TEST1', 
        'ทดสอบฝ่ายที่มีแม่', 
        'Test Division with Parent', 
        2, 
        @ParentID, 
        1, 
        1, 
        0, 
        1, 
        GETDATE(),
        3
    );
    
    PRINT '✅ Division with parent inserted successfully'
    
END TRY
BEGIN CATCH
    PRINT '❌ Error inserting Division with parent:'
    PRINT 'Error: ' + ERROR_MESSAGE()
END CATCH

-- Test 3: Insert an independent Division
PRINT ''
PRINT 'Test 3: Inserting independent Division...'
BEGIN TRY
    INSERT INTO core.OrgUnit (
        OrgUnitCode, 
        OrgUnitNameTH, 
        OrgUnitNameEN, 
        OrgUnitType, 
        ParentOrgUnitID, 
        SortOrder, 
        IsActive, 
        IsDeleted, 
        CreatedBy, 
        CreatedDate,
        ConfidentialityLevelValue
    )
    VALUES (
        'DIV-TEST2', 
        'ทดสอบฝ่ายอิสระ', 
        'Test Independent Division', 
        2, 
        NULL, 
        1, 
        1, 
        0, 
        1, 
        GETDATE(),
        3
    );
    
    PRINT '✅ Independent Division inserted successfully'
    
END TRY
BEGIN CATCH
    PRINT '❌ Error inserting independent Division:'
    PRINT 'Error: ' + ERROR_MESSAGE()
END CATCH

-- Show results
PRINT ''
PRINT 'Test Results:'
SELECT 
    OrgUnitID,
    OrgUnitCode,
    OrgUnitNameTH,
    OrgUnitType,
    ParentOrgUnitID,
    CASE 
        WHEN OrgUnitType = 1 THEN 'SubCommittee'
        WHEN OrgUnitType = 2 THEN 'Division'
        WHEN OrgUnitType = 3 THEN 'Department'
    END as TypeName,
    CASE 
        WHEN ParentOrgUnitID IS NULL THEN 'Independent'
        ELSE 'Has Parent: ' + CAST(ParentOrgUnitID AS NVARCHAR(10))
    END as ParentStatus
FROM core.OrgUnit 
WHERE OrgUnitCode LIKE '%TEST%' 
  AND IsDeleted = 0 
  AND IsActive = 1
ORDER BY OrgUnitType, OrgUnitCode;

PRINT ''
PRINT 'Simple test insert completed.'
PRINT 'If all tests passed, the basic structure is working correctly.'