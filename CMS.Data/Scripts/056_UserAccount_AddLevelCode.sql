-- ============================================================
-- Script 056: Add LevelCode to UserAccount
-- ใช้ LevelCode เป็น key หลักในการ resolve FolderAuthorityConfig
-- ============================================================

-- 1. เพิ่ม column LevelCode ใน UserAccount
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('core.UserAccount') AND name = 'LevelCode'
)
BEGIN
    ALTER TABLE core.UserAccount ADD LevelCode NVARCHAR(20) NULL;
    PRINT '✅ เพิ่ม LevelCode ใน core.UserAccount แล้ว';
END
GO

-- 2. Update sp_ResolveFolderAuthority ให้รับ LevelCode เป็น Priority สูงสุด
CREATE OR ALTER PROCEDURE core.sp_ResolveFolderAuthority
    @LevelCode      NVARCHAR(20)  = NULL,   -- Priority 0: ตรง LevelCode ของ user
    @PositionName   NVARCHAR(200) = NULL,   -- Priority 1
    @DepartmentCode NVARCHAR(50)  = NULL,   -- Priority 2
    @DivisionCode   NVARCHAR(50)  = NULL    -- Priority 3
AS
BEGIN
    SET NOCOUNT ON;

    -- Priority 0: match LevelCode ของ user โดยตรง (ใหม่)
    IF @LevelCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND ConfigLabel = @LevelCode
    )
    BEGIN
        SELECT * FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND ConfigLabel = @LevelCode
        ORDER BY ConfidentialityLevel;
        RETURN;
    END

    -- Priority 1: PositionName
    IF @PositionName IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND PositionName = @PositionName
    )
    BEGIN
        SELECT * FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND PositionName = @PositionName
        ORDER BY ConfidentialityLevel;
        RETURN;
    END

    -- Priority 2: DepartmentCode
    IF @DepartmentCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND DepartmentCode = @DepartmentCode
          AND PositionName IS NULL
    )
    BEGIN
        SELECT * FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND DepartmentCode = @DepartmentCode
          AND PositionName IS NULL
        ORDER BY ConfidentialityLevel;
        RETURN;
    END

    -- Priority 3: DivisionCode
    SELECT * FROM core.FolderAuthorityConfig
    WHERE IsActive = 1
      AND DivisionCode   = @DivisionCode
      AND PositionName   IS NULL
      AND DepartmentCode IS NULL
    ORDER BY ConfidentialityLevel;
END
GO

PRINT '✅ Script 056 เสร็จสมบูรณ์';
GO
