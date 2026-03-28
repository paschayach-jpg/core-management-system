-- ============================================================
-- Script 042: Create Missing Stored Procedures
-- สร้าง stored procedures ที่หายไปหรือไม่มีอยู่
-- ============================================================

PRINT '🔧 Create Missing Stored Procedures'
PRINT '=================================='
PRINT 'กำลังตรวจสอบและสร้าง stored procedures ที่จำเป็น...'
PRINT ''

-- ตรวจสอบว่า schema core มีอยู่หรือไม่
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'core')
BEGIN
    PRINT '❌ Schema core ไม่มีอยู่! กำลังสร้าง...'
    EXEC('CREATE SCHEMA core')
    PRINT '✅ สร้าง Schema core แล้ว'
END
ELSE
BEGIN
    PRINT '✅ Schema core มีอยู่แล้ว'
END

-- ตรวจสอบว่าตาราง OrgUnit มีอยู่หรือไม่
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrgUnit' AND schema_id = SCHEMA_ID('core'))
BEGIN
    PRINT '❌ ตาราง core.OrgUnit ไม่มีอยู่!'
    PRINT 'กรุณารัน schema creation scripts ก่อน'
    RETURN
END

PRINT '✅ ตาราง core.OrgUnit มีอยู่'

-- ตรวจสอบโครงสร้าง columns
PRINT ''
PRINT '🔍 ตรวจสอบโครงสร้าง Columns'
PRINT '=========================='

DECLARE @HasConfidentialityLevelValue BIT = 0
DECLARE @HasConfidentialityLevel BIT = 0

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'ConfidentialityLevelValue')
    SET @HasConfidentialityLevelValue = 1

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'core' AND TABLE_NAME = 'OrgUnit' AND COLUMN_NAME = 'ConfidentialityLevel')
    SET @HasConfidentialityLevel = 1

PRINT 'ConfidentialityLevelValue: ' + CASE WHEN @HasConfidentialityLevelValue = 1 THEN 'มี' ELSE 'ไม่มี' END
PRINT 'ConfidentialityLevel: ' + CASE WHEN @HasConfidentialityLevel = 1 THEN 'มี' ELSE 'ไม่มี' END

-- ตรวจสอบ stored procedures ที่มีอยู่
PRINT ''
PRINT '📋 ตรวจสอบ Stored Procedures ที่มีอยู่'
PRINT '=================================='

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_InsertOrgUnit') AND type in (N'P', N'PC'))
    PRINT '✅ core.sp_InsertOrgUnit - มีอยู่แล้ว'
ELSE
    PRINT '❌ core.sp_InsertOrgUnit - ไม่มี (จะสร้างใหม่)'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_GetOrgUnitHierarchy') AND type in (N'P', N'PC'))
    PRINT '✅ core.sp_GetOrgUnitHierarchy - มีอยู่แล้ว'
ELSE
    PRINT '❌ core.sp_GetOrgUnitHierarchy - ไม่มี (จะสร้างใหม่)'

-- สร้าง sp_InsertOrgUnit
PRINT ''
PRINT '🔧 สร้าง core.sp_InsertOrgUnit'
PRINT '=========================='

-- ลบ stored procedure เก่า (ถ้ามี)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_InsertOrgUnit') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE core.sp_InsertOrgUnit
    PRINT '✅ ลบ sp_InsertOrgUnit เก่าแล้ว'
END

-- สร้าง sp_InsertOrgUnit ใหม่
DECLARE @CreateInsertSP NVARCHAR(MAX) = '
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
        IsActive, IsDeleted'

-- เพิ่ม ConfidentialityLevel column ถ้ามี
IF @HasConfidentialityLevelValue = 1
    SET @CreateInsertSP = @CreateInsertSP + ', ConfidentialityLevelValue'
ELSE IF @HasConfidentialityLevel = 1
    SET @CreateInsertSP = @CreateInsertSP + ', ConfidentialityLevel'

SET @CreateInsertSP = @CreateInsertSP + '
    )
    VALUES (
        @OrgUnitCode, @OrgUnitNameTH, @OrgUnitNameEN, @OrgUnitType, 
        @ParentOrgUnitID, @SortOrder, @CreatedBy, GETDATE(), 
        1, 0'

-- เพิ่มค่า default สำหรับ ConfidentialityLevel
IF @HasConfidentialityLevelValue = 1 OR @HasConfidentialityLevel = 1
    SET @CreateInsertSP = @CreateInsertSP + ', 3'

SET @CreateInsertSP = @CreateInsertSP + '
    );
    
    SELECT SCOPE_IDENTITY() AS NewOrgUnitID;
END'

EXEC sp_executesql @CreateInsertSP
PRINT '✅ สร้าง core.sp_InsertOrgUnit สำเร็จ'

-- สร้าง sp_GetOrgUnitHierarchy
PRINT ''
PRINT '🔧 สร้าง core.sp_GetOrgUnitHierarchy'
PRINT '==============================='

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_GetOrgUnitHierarchy') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE core.sp_GetOrgUnitHierarchy
    PRINT '✅ ลบ sp_GetOrgUnitHierarchy เก่าแล้ว'
END

EXEC('
CREATE PROCEDURE core.sp_GetOrgUnitHierarchy
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        OrgUnitID,
        OrgUnitCode,
        OrgUnitNameTH,
        OrgUnitNameEN,
        OrgUnitType,
        ParentOrgUnitID,
        SortOrder,
        IsActive,
        IsDeleted,
        DeletedBy,
        CreatedBy,
        CreatedDate
    FROM core.OrgUnit
    ORDER BY OrgUnitType, SortOrder, OrgUnitNameTH;
END
')

PRINT '✅ สร้าง core.sp_GetOrgUnitHierarchy สำเร็จ'

-- สร้าง sp_UpdateOrgUnit
PRINT ''
PRINT '🔧 สร้าง core.sp_UpdateOrgUnit'
PRINT '=========================='

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_UpdateOrgUnit') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE core.sp_UpdateOrgUnit
    PRINT '✅ ลบ sp_UpdateOrgUnit เก่าแล้ว'
END

EXEC('
CREATE PROCEDURE core.sp_UpdateOrgUnit
    @OrgUnitID      INT,
    @OrgUnitNameTH  NVARCHAR(200),
    @OrgUnitNameEN  NVARCHAR(200),
    @SortOrder      INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE core.OrgUnit
    SET    OrgUnitNameTH = @OrgUnitNameTH,
           OrgUnitNameEN = @OrgUnitNameEN,
           SortOrder     = @SortOrder
    WHERE  OrgUnitID = @OrgUnitID;
END
')

PRINT '✅ สร้าง core.sp_UpdateOrgUnit สำเร็จ'

-- สร้าง sp_GetOrgUnitByID
PRINT ''
PRINT '🔧 สร้าง core.sp_GetOrgUnitByID'
PRINT '=========================='

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_GetOrgUnitByID') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE core.sp_GetOrgUnitByID
    PRINT '✅ ลบ sp_GetOrgUnitByID เก่าแล้ว'
END

EXEC('
CREATE PROCEDURE core.sp_GetOrgUnitByID
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        OrgUnitID,
        OrgUnitCode,
        OrgUnitNameTH,
        OrgUnitNameEN,
        OrgUnitType,
        ParentOrgUnitID,
        SortOrder,
        IsActive,
        IsDeleted,
        DeletedBy,
        CreatedBy,
        CreatedDate
    FROM core.OrgUnit
    WHERE OrgUnitID = @OrgUnitID;
END
')

PRINT '✅ สร้าง core.sp_GetOrgUnitByID สำเร็จ'

-- สร้าง sp_SoftDeleteOrgUnit
PRINT ''
PRINT '🔧 สร้าง core.sp_SoftDeleteOrgUnit'
PRINT '=============================='

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_SoftDeleteOrgUnit') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE core.sp_SoftDeleteOrgUnit
    PRINT '✅ ลบ sp_SoftDeleteOrgUnit เก่าแล้ว'
END

EXEC('
CREATE PROCEDURE core.sp_SoftDeleteOrgUnit
    @OrgUnitID INT,
    @DeletedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE core.OrgUnit
    SET IsDeleted = 1,
        IsActive = 0,
        DeletedBy = @DeletedBy
    WHERE OrgUnitID = @OrgUnitID;
END
')

PRINT '✅ สร้าง core.sp_SoftDeleteOrgUnit สำเร็จ'

-- ทดสอบ stored procedures
PRINT ''
PRINT '🧪 ทดสอบ Stored Procedures'
PRINT '======================='

BEGIN TRY
    -- ทดสอบ sp_GetOrgUnitHierarchy
    EXEC core.sp_GetOrgUnitHierarchy
    PRINT '✅ sp_GetOrgUnitHierarchy ทำงานได้'
    
    -- ทดสอบ sp_InsertOrgUnit
    EXEC core.sp_InsertOrgUnit 
        @OrgUnitCode = 'TEST-SP',
        @OrgUnitNameTH = 'ทดสอบ Stored Procedure',
        @OrgUnitNameEN = 'Test Stored Procedure',
        @OrgUnitType = 1,
        @ParentOrgUnitID = NULL,
        @SortOrder = 999,
        @CreatedBy = 1
    
    PRINT '✅ sp_InsertOrgUnit ทำงานได้'
    
    -- ลบข้อมูลทดสอบ
    DELETE FROM core.OrgUnit WHERE OrgUnitCode = 'TEST-SP'
    PRINT '✅ ลบข้อมูลทดสอบแล้ว'
    
END TRY
BEGIN CATCH
    PRINT '❌ เกิดข้อผิดพลาดในการทดสอบ:'
    PRINT 'Error: ' + ERROR_MESSAGE()
END CATCH

-- แสดงรายการ stored procedures ที่สร้างแล้ว
PRINT ''
PRINT '📋 Stored Procedures ที่สร้างเสร็จแล้ว'
PRINT '=================================='

SELECT 
    ROUTINE_NAME as StoredProcedureName,
    CREATED as CreatedDate
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_SCHEMA = 'core' 
  AND ROUTINE_TYPE = 'PROCEDURE'
  AND ROUTINE_NAME LIKE '%OrgUnit%'
ORDER BY ROUTINE_NAME

PRINT ''
PRINT '🎉 สร้าง Missing Stored Procedures เสร็จสิ้น!'
PRINT ''
PRINT '✅ Stored Procedures ที่สร้างแล้ว:'
PRINT '• core.sp_InsertOrgUnit - สำหรับเพิ่มข้อมูล'
PRINT '• core.sp_GetOrgUnitHierarchy - สำหรับดึงข้อมูลทั้งหมด'
PRINT '• core.sp_UpdateOrgUnit - สำหรับแก้ไขข้อมูล'
PRINT '• core.sp_GetOrgUnitByID - สำหรับดึงข้อมูลตาม ID'
PRINT '• core.sp_SoftDeleteOrgUnit - สำหรับลบข้อมูล'
PRINT ''
PRINT 'ตอนนี้ Application สามารถบันทึกข้อมูลได้แล้ว!'
PRINT ''
PRINT 'Script 042: Create Missing Stored Procedures เสร็จสิ้น! 🚀'