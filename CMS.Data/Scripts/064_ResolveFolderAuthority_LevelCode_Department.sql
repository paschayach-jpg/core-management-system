-- ============================================================
-- Script 064: แก้ sp_ResolveFolderAuthority
--             เพิ่ม Priority: LevelCode + DepartmentCode
--
-- Priority ใหม่:
--   0. LevelCode + DepartmentCode  (specific ที่สุด)
--   1. LevelCode                   (ระดับตำแหน่งอย่างเดียว)
--   2. PositionName                (ชื่อตำแหน่งตรงๆ)
--   3. DepartmentCode              (แผนก fallback)
--   4. DivisionCode                (ฝ่าย fallback)
--
-- ตัวอย่าง:
--   User: LevelCode=1A, DepartmentCode=DEP-AGT
--   → ค้นหา config ที่ LevelCode=1A AND DepartmentCode=DEP-AGT ก่อน
--   → ถ้าไม่มี → ค้นหา LevelCode=1A อย่างเดียว
--   → ถ้าไม่มี → ค้นหา PositionName
--   → ถ้าไม่มี → ค้นหา DepartmentCode
--   → ถ้าไม่มี → ค้นหา DivisionCode
-- ============================================================

CREATE OR ALTER PROCEDURE core.sp_ResolveFolderAuthority
    @LevelCode      NVARCHAR(20)  = NULL,
    @PositionName   NVARCHAR(200) = NULL,
    @DepartmentCode NVARCHAR(50)  = NULL,
    @DivisionCode   NVARCHAR(50)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- ── Priority 0: LevelCode + DepartmentCode (specific ที่สุด) ─────────────
    IF @LevelCode IS NOT NULL AND @DepartmentCode IS NOT NULL
       AND EXISTS (
           SELECT 1 FROM core.FolderAuthorityConfig
           WHERE  IsActive       = 1
             AND  LevelCode      = @LevelCode
             AND  DepartmentCode = @DepartmentCode
       )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive       = 1
          AND  LevelCode      = @LevelCode
          AND  DepartmentCode = @DepartmentCode
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 1: LevelCode อย่างเดียว ─────────────────────────────────────
    IF @LevelCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE  IsActive  = 1
          AND  LevelCode = @LevelCode
          AND  (DepartmentCode IS NULL OR DepartmentCode = '')
    )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive  = 1
          AND  LevelCode = @LevelCode
          AND  (DepartmentCode IS NULL OR DepartmentCode = '')
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 2: PositionName ─────────────────────────────────────────────
    IF @PositionName IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE  IsActive      = 1
          AND  PositionName  = @PositionName
    )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive     = 1
          AND  PositionName = @PositionName
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 3: DepartmentCode ───────────────────────────────────────────
    IF @DepartmentCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE  IsActive       = 1
          AND  DepartmentCode = @DepartmentCode
          AND  PositionName   IS NULL
          AND  (LevelCode     IS NULL OR LevelCode = '')
    )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive       = 1
          AND  DepartmentCode = @DepartmentCode
          AND  PositionName   IS NULL
          AND  (LevelCode     IS NULL OR LevelCode = '')
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 4: DivisionCode ─────────────────────────────────────────────
    SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
           LevelCode, ConfidentialityLevel,
           CanView, CanAdd, CanEdit, CanApprove,
           IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
    FROM   core.FolderAuthorityConfig
    WHERE  IsActive       = 1
      AND  DivisionCode   = @DivisionCode
      AND  PositionName   IS NULL
      AND  DepartmentCode IS NULL
      AND  (LevelCode     IS NULL OR LevelCode = '')
    ORDER  BY ConfidentialityLevel;
END
GO

PRINT '✅ sp_ResolveFolderAuthority updated — Priority 0 = LevelCode+DepartmentCode';
GO

-- ============================================================
-- ตรวจสอบ: แสดง config ที่มี LevelCode กำหนดไว้
-- ============================================================
SELECT
    ConfigLabel,
    LevelCode,
    DepartmentCode,
    DivisionCode,
    PositionName,
    ConfidentialityLevel,
    CanView, CanAdd, CanEdit, CanApprove
FROM   core.FolderAuthorityConfig
WHERE  IsActive = 1
  AND  LevelCode IS NOT NULL
  AND  LevelCode <> ''
ORDER  BY LevelCode, DepartmentCode, ConfidentialityLevel;
GO
