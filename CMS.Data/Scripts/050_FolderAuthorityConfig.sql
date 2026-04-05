-- ============================================================
-- Script 050: FolderAuthorityConfig
-- Schema ตรงกับ DB จริง:
-- 1 row = ตำแหน่ง/แผนก/ฝ่าย × 1 ConfidentialityLevel
-- (HC=1, SC=2, NC=3, PC=4) × (CanView, CanAdd, CanEdit, CanApprove)
-- ============================================================

-- Step 1: สร้างตารางถ้ายังไม่มี
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'core' AND t.name = 'FolderAuthorityConfig'
)
BEGIN
    CREATE TABLE core.FolderAuthorityConfig (
        ConfigID             INT IDENTITY(1,1) PRIMARY KEY,
        ConfigLabel          NVARCHAR(200) NOT NULL,

        -- ผูกกับ EmployeeInfo (Priority: PositionName > DepartmentCode > DivisionCode)
        PositionName         NVARCHAR(200) NULL,
        DepartmentCode       NVARCHAR(50)  NULL,
        DivisionCode         NVARCHAR(50)  NULL,

        -- ระดับ Folder: 1=HC, 2=SC, 3=NC, 4=PC
        ConfidentialityLevel TINYINT NOT NULL,

        -- สิทธิ์
        CanView    BIT NOT NULL DEFAULT 0,
        CanAdd     BIT NOT NULL DEFAULT 0,
        CanEdit    BIT NOT NULL DEFAULT 0,
        CanApprove BIT NOT NULL DEFAULT 0,

        IsActive    BIT      NOT NULL DEFAULT 1,
        CreatedBy   INT      NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedBy   INT      NULL,
        UpdatedDate DATETIME NULL
    );
    PRINT 'Created core.FolderAuthorityConfig';
END
GO

-- Step 2: เพิ่มคอลัมน์ที่อาจขาดหาย (idempotent)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('core.FolderAuthorityConfig') AND name = 'UpdatedBy')
    ALTER TABLE core.FolderAuthorityConfig ADD UpdatedBy INT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('core.FolderAuthorityConfig') AND name = 'UpdatedDate')
    ALTER TABLE core.FolderAuthorityConfig ADD UpdatedDate DATETIME NULL;
GO

-- Step 3: Sample Data (ตามภาพตัวอย่าง)
-- แต่ละ position × 4 levels = 4 rows ต่อ 1 ตำแหน่ง
IF NOT EXISTS (SELECT 1 FROM core.FolderAuthorityConfig WHERE ConfigLabel = 'ประธานกรรมการบริหาร')
BEGIN
    -- ประธานกรรมการบริหาร: Full Access ทุก Level
    INSERT INTO core.FolderAuthorityConfig (ConfigLabel, PositionName, DepartmentCode, DivisionCode, ConfidentialityLevel, CanView, CanAdd, CanEdit, CanApprove)
    VALUES
    ('ประธานกรรมการบริหาร', 'ประธานกรรมการบริหาร', 'กรรมการ', 'สำนักกรรมการผู้จัดการ', 1, 1,1,1,1),
    ('ประธานกรรมการบริหาร', 'ประธานกรรมการบริหาร', 'กรรมการ', 'สำนักกรรมการผู้จัดการ', 2, 1,1,1,1),
    ('ประธานกรรมการบริหาร', 'ประธานกรรมการบริหาร', 'กรรมการ', 'สำนักกรรมการผู้จัดการ', 3, 1,1,1,1),
    ('ประธานกรรมการบริหาร', 'ประธานกรรมการบริหาร', 'กรรมการ', 'สำนักกรรมการผู้จัดการ', 4, 1,1,1,1),

    -- กรรมการผู้จัดการ: Full Access ทุก Level
    ('กรรมการผู้จัดการ', 'กรรมการผู้จัดการ', 'กรรมการ', 'ผู้บริหาร', 1, 1,1,1,1),
    ('กรรมการผู้จัดการ', 'กรรมการผู้จัดการ', 'กรรมการ', 'ผู้บริหาร', 2, 1,1,1,1),
    ('กรรมการผู้จัดการ', 'กรรมการผู้จัดการ', 'กรรมการ', 'ผู้บริหาร', 3, 1,1,1,1),
    ('กรรมการผู้จัดการ', 'กรรมการผู้จัดการ', 'กรรมการ', 'ผู้บริหาร', 4, 1,1,1,1),

    -- ผู้จัดการแผนกตรวจสอบภายใน: จำกัด
    ('ผู้จัดการแผนกตรวจสอบภายใน', 'ผู้จัดการแผนกตรวจสอบภายใน', 'ตรวจสอบภายใน', 'ขั้นตรงคณะกรรมการตรวจสอบ', 1, 1,0,0,0),
    ('ผู้จัดการแผนกตรวจสอบภายใน', 'ผู้จัดการแผนกตรวจสอบภายใน', 'ตรวจสอบภายใน', 'ขั้นตรงคณะกรรมการตรวจสอบ', 2, 1,0,0,0),
    ('ผู้จัดการแผนกตรวจสอบภายใน', 'ผู้จัดการแผนกตรวจสอบภายใน', 'ตรวจสอบภายใน', 'ขั้นตรงคณะกรรมการตรวจสอบ', 3, 1,1,0,0),
    ('ผู้จัดการแผนกตรวจสอบภายใน', 'ผู้จัดการแผนกตรวจสอบภายใน', 'ตรวจสอบภายใน', 'ขั้นตรงคณะกรรมการตรวจสอบ', 4, 1,1,0,0),

    -- ผู้ช่วยผู้จัดการแผนกตรวจสอบภายใน
    ('ผู้ช่วยผู้จัดการแผนกตรวจสอบภายใน', 'ผู้ช่วยผู้จัดการแผนกตรวจสอบภายใน', 'ตรวจสอบภายใน', 'ขั้นตรงคณะกรรมการตรวจสอบ', 1, 0,0,0,0),
    ('ผู้ช่วยผู้จัดการแผนกตรวจสอบภายใน', 'ผู้ช่วยผู้จัดการแผนกตรวจสอบภายใน', 'ตรวจสอบภายใน', 'ขั้นตรงคณะกรรมการตรวจสอบ', 2, 0,0,0,0),
    ('ผู้ช่วยผู้จัดการแผนกตรวจสอบภายใน', 'ผู้ช่วยผู้จัดการแผนกตรวจสอบภายใน', 'ตรวจสอบภายใน', 'ขั้นตรงคณะกรรมการตรวจสอบ', 3, 1,1,0,0),
    ('ผู้ช่วยผู้จัดการแผนกตรวจสอบภายใน', 'ผู้ช่วยผู้จัดการแผนกตรวจสอบภายใน', 'ตรวจสอบภายใน', 'ขั้นตรงคณะกรรมการตรวจสอบ', 4, 1,1,0,0);

    PRINT 'Inserted sample FolderAuthorityConfig data';
END
GO

-- ── Stored Procedures ─────────────────────────────────────────

-- GetAll
IF OBJECT_ID('core.sp_GetAllFolderAuthorityConfigs', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_GetAllFolderAuthorityConfigs;
GO
CREATE PROCEDURE core.sp_GetAllFolderAuthorityConfigs
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM core.FolderAuthorityConfig
    WHERE IsActive = 1
    ORDER BY ConfigLabel, ConfidentialityLevel;
END
GO

-- GetByID
IF OBJECT_ID('core.sp_GetFolderAuthorityConfigByID', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_GetFolderAuthorityConfigByID;
GO
CREATE PROCEDURE core.sp_GetFolderAuthorityConfigByID
    @ConfigID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM core.FolderAuthorityConfig WHERE ConfigID = @ConfigID;
END
GO

-- ResolveFolderAuthority
-- คืน 4 rows (1 ต่อ Level) สำหรับพนักงาน 1 คน
-- Priority: PositionName > DepartmentCode > DivisionCode
IF OBJECT_ID('core.sp_ResolveFolderAuthority', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_ResolveFolderAuthority;
GO
CREATE PROCEDURE core.sp_ResolveFolderAuthority
    @PositionName   NVARCHAR(200),
    @DepartmentCode NVARCHAR(50),
    @DivisionCode   NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Priority 1: exact position match
    IF EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND PositionName = @PositionName
    )
    BEGIN
        SELECT * FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND PositionName = @PositionName
        ORDER BY ConfidentialityLevel;
        RETURN;
    END

    -- Priority 2: department match (no position specified)
    IF EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND DepartmentCode = @DepartmentCode AND PositionName IS NULL
    )
    BEGIN
        SELECT * FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND DepartmentCode = @DepartmentCode AND PositionName IS NULL
        ORDER BY ConfidentialityLevel;
        RETURN;
    END

    -- Priority 3: division match
    SELECT * FROM core.FolderAuthorityConfig
    WHERE IsActive = 1
      AND DivisionCode   = @DivisionCode
      AND PositionName   IS NULL
      AND DepartmentCode IS NULL
    ORDER BY ConfidentialityLevel;
END
GO

-- Insert
IF OBJECT_ID('core.sp_InsertFolderAuthorityConfig', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_InsertFolderAuthorityConfig;
GO
CREATE PROCEDURE core.sp_InsertFolderAuthorityConfig
    @ConfigLabel         NVARCHAR(200),
    @PositionName        NVARCHAR(200),
    @DepartmentCode      NVARCHAR(50),
    @DivisionCode        NVARCHAR(50),
    @ConfidentialityLevel TINYINT,
    @CanView    BIT,
    @CanAdd     BIT,
    @CanEdit    BIT,
    @CanApprove BIT,
    @CreatedBy  INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO core.FolderAuthorityConfig
        (ConfigLabel, PositionName, DepartmentCode, DivisionCode,
         ConfidentialityLevel, CanView, CanAdd, CanEdit, CanApprove, CreatedBy)
    VALUES
        (@ConfigLabel, @PositionName, @DepartmentCode, @DivisionCode,
         @ConfidentialityLevel, @CanView, @CanAdd, @CanEdit, @CanApprove, @CreatedBy);
    SELECT SCOPE_IDENTITY();
END
GO

-- Update
IF OBJECT_ID('core.sp_UpdateFolderAuthorityConfig', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_UpdateFolderAuthorityConfig;
GO
CREATE PROCEDURE core.sp_UpdateFolderAuthorityConfig
    @ConfigID            INT,
    @ConfigLabel         NVARCHAR(200),
    @PositionName        NVARCHAR(200),
    @DepartmentCode      NVARCHAR(50),
    @DivisionCode        NVARCHAR(50),
    @ConfidentialityLevel TINYINT,
    @CanView    BIT,
    @CanAdd     BIT,
    @CanEdit    BIT,
    @CanApprove BIT,
    @UpdatedBy  INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.FolderAuthorityConfig SET
        ConfigLabel          = @ConfigLabel,
        PositionName         = @PositionName,
        DepartmentCode       = @DepartmentCode,
        DivisionCode         = @DivisionCode,
        ConfidentialityLevel = @ConfidentialityLevel,
        CanView    = @CanView,
        CanAdd     = @CanAdd,
        CanEdit    = @CanEdit,
        CanApprove = @CanApprove,
        UpdatedBy  = @UpdatedBy,
        UpdatedDate = GETUTCDATE()
    WHERE ConfigID = @ConfigID;
END
GO

-- Delete (soft)
IF OBJECT_ID('core.sp_DeleteFolderAuthorityConfig', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_DeleteFolderAuthorityConfig;
GO
CREATE PROCEDURE core.sp_DeleteFolderAuthorityConfig
    @ConfigLabel NVARCHAR(200),
    @DeletedBy   INT
AS
BEGIN
    SET NOCOUNT ON;
    -- ลบทั้ง group (ทุก Level ของ ConfigLabel เดียวกัน)
    UPDATE core.FolderAuthorityConfig
    SET IsActive = 0, UpdatedBy = @DeletedBy, UpdatedDate = GETUTCDATE()
    WHERE ConfigLabel = @ConfigLabel;
END
GO

PRINT 'Script 050 completed.';
