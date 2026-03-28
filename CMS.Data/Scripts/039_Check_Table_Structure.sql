-- ============================================================
-- Script 039: Check Table Structure
-- ตรวจสอบโครงสร้างตาราง OrgUnit
-- ============================================================

PRINT '🔍 Check OrgUnit Table Structure'
PRINT '==============================='

-- ตรวจสอบว่าตาราง OrgUnit มีอยู่หรือไม่
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OrgUnit' AND schema_id = SCHEMA_ID('core'))
BEGIN
    PRINT '✅ ตาราง core.OrgUnit มีอยู่'
    
    -- แสดงโครงสร้าง columns
    PRINT ''
    PRINT 'โครงสร้าง Columns:'
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        IS_NULLABLE as IsNullable,
        COLUMN_DEFAULT as DefaultValue,
        CHARACTER_MAXIMUM_LENGTH as MaxLength
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit'
    ORDER BY ORDINAL_POSITION
    
    -- ตรวจสอบ columns ที่สำคัญ
    PRINT ''
    PRINT 'ตรวจสอบ Columns ที่สำคัญ:'
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'ConfidentialityLevelValue')
        PRINT '✅ ConfidentialityLevelValue - มีอยู่'
    ELSE
        PRINT '❌ ConfidentialityLevelValue - ไม่มี'
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'ConfidentialityLevel')
        PRINT '✅ ConfidentialityLevel - มีอยู่'
    ELSE
        PRINT '❌ ConfidentialityLevel - ไม่มี'
        
    -- ตรวจสอบ columns อื่นๆ
    DECLARE @MissingColumns TABLE (ColumnName NVARCHAR(100))
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'OrgUnitID')
        INSERT INTO @MissingColumns VALUES ('OrgUnitID')
        
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'OrgUnitCode')
        INSERT INTO @MissingColumns VALUES ('OrgUnitCode')
        
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'OrgUnitNameTH')
        INSERT INTO @MissingColumns VALUES ('OrgUnitNameTH')
        
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'OrgUnitType')
        INSERT INTO @MissingColumns VALUES ('OrgUnitType')
        
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'IsActive')
        INSERT INTO @MissingColumns VALUES ('IsActive')
        
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'IsDeleted')
        INSERT INTO @MissingColumns VALUES ('IsDeleted')
    
    -- แสดง missing columns
    IF EXISTS (SELECT * FROM @MissingColumns)
    BEGIN
        PRINT ''
        PRINT '❌ Columns ที่หายไป:'
        SELECT ColumnName FROM @MissingColumns
    END
    ELSE
    BEGIN
        PRINT '✅ Columns หลักครบถ้วน'
    END
    
    -- แสดงข้อมูลตัวอย่าง (ถ้ามี)
    PRINT ''
    PRINT 'ข้อมูลตัวอย่าง (5 แถวแรก):'
    
    DECLARE @SampleSQL NVARCHAR(MAX) = 'SELECT TOP 5 * FROM core.OrgUnit ORDER BY OrgUnitID'
    
    BEGIN TRY
        EXEC sp_executesql @SampleSQL
    END TRY
    BEGIN CATCH
        PRINT 'ไม่สามารถแสดงข้อมูลตัวอย่างได้: ' + ERROR_MESSAGE()
    END CATCH
    
END
ELSE
BEGIN
    PRINT '❌ ตาราง core.OrgUnit ไม่มีอยู่!'
    
    -- ตรวจสอบว่ามี schema core หรือไม่
    IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'core')
        PRINT '✅ Schema core มีอยู่'
    ELSE
        PRINT '❌ Schema core ไม่มีอยู่!'
        
    -- แสดงตารางทั้งหมดใน schema core
    PRINT ''
    PRINT 'ตารางทั้งหมดใน schema core:'
    SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'core'
END

PRINT ''
PRINT 'Script 039: Check Table Structure เสร็จสิ้น!'