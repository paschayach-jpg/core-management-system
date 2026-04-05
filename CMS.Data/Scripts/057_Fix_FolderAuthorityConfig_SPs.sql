-- ============================================================
-- Script 057: Fix sp_InsertFolderAuthorityConfig & sp_UpdateFolderAuthorityConfig
-- เพิ่ม @LevelCode parameter ให้ตรงกับ C# code
-- *** รันใน database SCS โดยตรง (ไม่ต้องใช้ USE) ***
-- ============================================================

-- เพิ่ม column LevelCode ถ้ายังไม่มี
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('core.FolderAuthorityConfig') AND name = 'LevelCode'
)
BEGIN
    ALTER TABLE core.FolderAuthorityConfig ADD LevelCode NVARCHAR(10) NULL;
    UPDATE core.FolderAuthorityConfig
    SET LevelCode = CASE ConfidentialityLevel
        WHEN 1 THEN '1HC' WHEN 2 THEN '2SC'
        WHEN 3 THEN '3NC' WHEN 4 THEN '4PC' ELSE NULL END
    WHERE LevelCode IS NULL;
    PRINT '✅ เพิ่ม LevelCode column แล้ว';
END
GO

-- Fix sp_InsertFolderAuthorityConfig
CREATE OR ALTER PROCEDURE core.sp_InsertFolderAuthorityConfig
    @ConfigLabel    NVARCHAR(200),
    @PositionName   NVARCHAR(200) = NULL,
    @DepartmentCode NVARCHAR(50)  = NULL,
    @DivisionCode   NVARCHAR(50)  = NULL,
    @LevelCode      NVARCHAR(10)  = NULL,
    @ConfidentialityLevel TINYINT = 0,
    @CanView    BIT = 0,
    @CanAdd     BIT = 0,
    @CanEdit    BIT = 0,
    @CanApprove BIT = 0,
    @CreatedBy  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- resolve level จาก LevelCode ถ้า ConfidentialityLevel ไม่ได้ส่งมา
    DECLARE @Level TINYINT = CASE
        WHEN @ConfidentialityLevel > 0 THEN @ConfidentialityLevel
        ELSE CASE @LevelCode
            WHEN '1HC' THEN 1 WHEN '2SC' THEN 2
            WHEN '3NC' THEN 3 WHEN '4PC' THEN 4 ELSE 0 END
    END;
    DECLARE @Code NVARCHAR(10) = CASE
        WHEN @LevelCode IS NOT NULL THEN @LevelCode
        ELSE CASE @Level WHEN 1 THEN '1HC' WHEN 2 THEN '2SC'
                         WHEN 3 THEN '3NC' WHEN 4 THEN '4PC' ELSE NULL END
    END;

    INSERT INTO core.FolderAuthorityConfig
        (ConfigLabel, PositionName, DepartmentCode, DivisionCode,
         LevelCode, ConfidentialityLevel,
         CanView, CanAdd, CanEdit, CanApprove, CreatedBy)
    VALUES
        (@ConfigLabel, @PositionName, @DepartmentCode, @DivisionCode,
         @Code, @Level,
         @CanView, @CanAdd, @CanEdit, @CanApprove, @CreatedBy);
    SELECT SCOPE_IDENTITY();
END
GO

-- Fix sp_UpdateFolderAuthorityConfig
CREATE OR ALTER PROCEDURE core.sp_UpdateFolderAuthorityConfig
    @ConfigID       INT,
    @ConfigLabel    NVARCHAR(200),
    @PositionName   NVARCHAR(200) = NULL,
    @DepartmentCode NVARCHAR(50)  = NULL,
    @DivisionCode   NVARCHAR(50)  = NULL,
    @LevelCode      NVARCHAR(10)  = NULL,
    @ConfidentialityLevel TINYINT = 0,
    @CanView    BIT = 0,
    @CanAdd     BIT = 0,
    @CanEdit    BIT = 0,
    @CanApprove BIT = 0,
    @UpdatedBy  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Level TINYINT = CASE
        WHEN @ConfidentialityLevel > 0 THEN @ConfidentialityLevel
        ELSE CASE @LevelCode
            WHEN '1HC' THEN 1 WHEN '2SC' THEN 2
            WHEN '3NC' THEN 3 WHEN '4PC' THEN 4 ELSE 0 END
    END;
    DECLARE @Code NVARCHAR(10) = CASE
        WHEN @LevelCode IS NOT NULL THEN @LevelCode
        ELSE CASE @Level WHEN 1 THEN '1HC' WHEN 2 THEN '2SC'
                         WHEN 3 THEN '3NC' WHEN 4 THEN '4PC' ELSE NULL END
    END;

    UPDATE core.FolderAuthorityConfig SET
        ConfigLabel          = @ConfigLabel,
        PositionName         = @PositionName,
        DepartmentCode       = @DepartmentCode,
        DivisionCode         = @DivisionCode,
        LevelCode            = @Code,
        ConfidentialityLevel = @Level,
        CanView    = @CanView,  CanAdd     = @CanAdd,
        CanEdit    = @CanEdit,  CanApprove = @CanApprove,
        UpdatedBy  = @UpdatedBy, UpdatedDate = GETUTCDATE()
    WHERE ConfigID = @ConfigID;
END
GO

PRINT '✅ Script 057 เสร็จสมบูรณ์ — SPs updated';
GO
