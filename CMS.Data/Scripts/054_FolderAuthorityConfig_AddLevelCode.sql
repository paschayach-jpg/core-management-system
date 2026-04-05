-- ============================================================
-- Script 054: Add LevelCode to FolderAuthorityConfig
-- ผูก FolderAuthorityConfig กับ 4 Folder หลัก
-- LevelCode: '1HC' | '2SC' | '3NC' | '4PC'
-- ============================================================

-- 1. เพิ่ม column LevelCode
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('core.FolderAuthorityConfig') AND name = 'LevelCode'
)
BEGIN
    ALTER TABLE core.FolderAuthorityConfig
        ADD LevelCode NVARCHAR(10) NULL;
    PRINT '✅ เพิ่ม column LevelCode แล้ว';
END
GO

-- 2. Populate LevelCode จาก ConfidentialityLevel ที่มีอยู่
UPDATE core.FolderAuthorityConfig
SET LevelCode = CASE ConfidentialityLevel
    WHEN 1 THEN '1HC'
    WHEN 2 THEN '2SC'
    WHEN 3 THEN '3NC'
    WHEN 4 THEN '4PC'
    ELSE NULL
END
WHERE LevelCode IS NULL;
PRINT '✅ Populate LevelCode จาก ConfidentialityLevel แล้ว: ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows';
GO

-- 3. เพิ่ม CHECK constraint
IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID('core.FolderAuthorityConfig')
      AND name = 'CK_FolderAuthorityConfig_LevelCode'
)
BEGIN
    ALTER TABLE core.FolderAuthorityConfig
        ADD CONSTRAINT CK_FolderAuthorityConfig_LevelCode
        CHECK (LevelCode IN ('1HC', '2SC', '3NC', '4PC'));
    PRINT '✅ เพิ่ม CHECK constraint LevelCode แล้ว';
END
GO

-- 4. Update Stored Procedures ให้รองรับ LevelCode

-- GetAll
CREATE OR ALTER PROCEDURE core.sp_GetAllFolderAuthorityConfigs
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM core.FolderAuthorityConfig
    WHERE IsActive = 1
    ORDER BY ConfigLabel, ConfidentialityLevel;
END
GO

-- ResolveFolderAuthority — คืน 4 rows พร้อม LevelCode
CREATE OR ALTER PROCEDURE core.sp_ResolveFolderAuthority
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

    -- Priority 2: department match
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

-- ResolveByLevelCode — ดึงสิทธิ์ของ folder level ที่ระบุ
CREATE OR ALTER PROCEDURE core.sp_ResolveFolderAuthorityByLevel
    @PositionName   NVARCHAR(200),
    @DepartmentCode NVARCHAR(50),
    @DivisionCode   NVARCHAR(50),
    @LevelCode      NVARCHAR(10)   -- '1HC' | '2SC' | '3NC' | '4PC'
AS
BEGIN
    SET NOCOUNT ON;

    -- Priority 1: position + level
    IF EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND PositionName = @PositionName AND LevelCode = @LevelCode
    )
    BEGIN
        SELECT TOP 1 * FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND PositionName = @PositionName AND LevelCode = @LevelCode;
        RETURN;
    END

    -- Priority 2: department + level
    IF EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND DepartmentCode = @DepartmentCode
          AND LevelCode = @LevelCode AND PositionName IS NULL
    )
    BEGIN
        SELECT TOP 1 * FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND DepartmentCode = @DepartmentCode
          AND LevelCode = @LevelCode AND PositionName IS NULL;
        RETURN;
    END

    -- Priority 3: division + level
    SELECT TOP 1 * FROM core.FolderAuthorityConfig
    WHERE IsActive = 1
      AND DivisionCode   = @DivisionCode
      AND LevelCode      = @LevelCode
      AND PositionName   IS NULL
      AND DepartmentCode IS NULL;
END
GO

-- Insert — รับ LevelCode แทน ConfidentialityLevel
CREATE OR ALTER PROCEDURE core.sp_InsertFolderAuthorityConfig
    @ConfigLabel    NVARCHAR(200),
    @PositionName   NVARCHAR(200),
    @DepartmentCode NVARCHAR(50),
    @DivisionCode   NVARCHAR(50),
    @LevelCode      NVARCHAR(10),   -- '1HC' | '2SC' | '3NC' | '4PC'
    @CanView    BIT,
    @CanAdd     BIT,
    @CanEdit    BIT,
    @CanApprove BIT,
    @CreatedBy  INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Level TINYINT = CASE @LevelCode
        WHEN '1HC' THEN 1 WHEN '2SC' THEN 2
        WHEN '3NC' THEN 3 WHEN '4PC' THEN 4 ELSE 0 END;

    INSERT INTO core.FolderAuthorityConfig
        (ConfigLabel, PositionName, DepartmentCode, DivisionCode,
         LevelCode, ConfidentialityLevel,
         CanView, CanAdd, CanEdit, CanApprove, CreatedBy)
    VALUES
        (@ConfigLabel, @PositionName, @DepartmentCode, @DivisionCode,
         @LevelCode, @Level,
         @CanView, @CanAdd, @CanEdit, @CanApprove, @CreatedBy);
    SELECT SCOPE_IDENTITY();
END
GO

-- Update
CREATE OR ALTER PROCEDURE core.sp_UpdateFolderAuthorityConfig
    @ConfigID       INT,
    @ConfigLabel    NVARCHAR(200),
    @PositionName   NVARCHAR(200),
    @DepartmentCode NVARCHAR(50),
    @DivisionCode   NVARCHAR(50),
    @LevelCode      NVARCHAR(10),
    @CanView    BIT,
    @CanAdd     BIT,
    @CanEdit    BIT,
    @CanApprove BIT,
    @UpdatedBy  INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Level TINYINT = CASE @LevelCode
        WHEN '1HC' THEN 1 WHEN '2SC' THEN 2
        WHEN '3NC' THEN 3 WHEN '4PC' THEN 4 ELSE 0 END;

    UPDATE core.FolderAuthorityConfig SET
        ConfigLabel          = @ConfigLabel,
        PositionName         = @PositionName,
        DepartmentCode       = @DepartmentCode,
        DivisionCode         = @DivisionCode,
        LevelCode            = @LevelCode,
        ConfidentialityLevel = @Level,
        CanView    = @CanView,  CanAdd     = @CanAdd,
        CanEdit    = @CanEdit,  CanApprove = @CanApprove,
        UpdatedBy  = @UpdatedBy, UpdatedDate = GETUTCDATE()
    WHERE ConfigID = @ConfigID;
END
GO

PRINT '✅ Script 054 เสร็จสมบูรณ์';
GO
