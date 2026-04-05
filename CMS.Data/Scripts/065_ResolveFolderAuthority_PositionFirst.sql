-- ============================================================
-- Script 065: แก้ sp_ResolveFolderAuthority
--             ให้ PositionName มี Priority สูงกว่า LevelCode
--             เพื่อรองรับ 1 Department มีหลายตำแหน่ง
--
-- Priority:
--   0. PositionName + DepartmentCode  (specific ที่สุด)
--   1. PositionName                   (ตำแหน่งอย่างเดียว)
--   2. LevelCode + DepartmentCode     (ระดับใน department)
--   3. LevelCode                      (ระดับอย่างเดียว)
--   4. DepartmentCode                 (department fallback)
--   5. DivisionCode                   (division fallback)
--
-- ตัวอย่าง:
--   User: PositionName=Programmer, DepartmentCode=DEP-IT, LevelCode=2A
--   → ค้นหา config ที่ PositionName=Programmer AND DepartmentCode=DEP-IT ก่อน
--   → ถ้าไม่มี → ค้นหา PositionName=Programmer อย่างเดียว
--   → ถ้าไม่มี → ค้นหา LevelCode=2A AND DepartmentCode=DEP-IT
--   → ถ้าไม่มี → ค้นหา LevelCode=2A อย่างเดียว
--   → ถ้าไม่มี → ค้นหา DepartmentCode=DEP-IT
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

    -- ── Priority 0: PositionName + DepartmentCode (specific ที่สุด) ──────────
    IF @PositionName IS NOT NULL AND @DepartmentCode IS NOT NULL
       AND EXISTS (
           SELECT 1 FROM core.FolderAuthorityConfig
           WHERE  IsActive       = 1
             AND  PositionName   = @PositionName
             AND  DepartmentCode = @DepartmentCode
       )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive       = 1
          AND  PositionName   = @PositionName
          AND  DepartmentCode = @DepartmentCode
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 1: PositionName อย่างเดียว ──────────────────────────────────
    IF @PositionName IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE  IsActive     = 1
          AND  PositionName = @PositionName
          AND  (DepartmentCode IS NULL OR DepartmentCode = '')
    )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive     = 1
          AND  PositionName = @PositionName
          AND  (DepartmentCode IS NULL OR DepartmentCode = '')
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 2: LevelCode + DepartmentCode ────────────────────────────────
    IF @LevelCode IS NOT NULL AND @DepartmentCode IS NOT NULL
       AND EXISTS (
           SELECT 1 FROM core.FolderAuthorityConfig
           WHERE  IsActive       = 1
             AND  LevelCode      = @LevelCode
             AND  DepartmentCode = @DepartmentCode
             AND  (PositionName  IS NULL OR PositionName = '')
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
          AND  (PositionName  IS NULL OR PositionName = '')
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 3: LevelCode อย่างเดียว ─────────────────────────────────────
    IF @LevelCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE  IsActive  = 1
          AND  LevelCode = @LevelCode
          AND  (DepartmentCode IS NULL OR DepartmentCode = '')
          AND  (PositionName   IS NULL OR PositionName   = '')
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
          AND  (PositionName   IS NULL OR PositionName   = '')
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 4: DepartmentCode ───────────────────────────────────────────
    IF @DepartmentCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE  IsActive       = 1
          AND  DepartmentCode = @DepartmentCode
          AND  (PositionName  IS NULL OR PositionName = '')
          AND  (LevelCode     IS NULL OR LevelCode    = '')
    )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive       = 1
          AND  DepartmentCode = @DepartmentCode
          AND  (PositionName  IS NULL OR PositionName = '')
          AND  (LevelCode     IS NULL OR LevelCode    = '')
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 5: DivisionCode ─────────────────────────────────────────────
    SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
           LevelCode, ConfidentialityLevel,
           CanView, CanAdd, CanEdit, CanApprove,
           IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
    FROM   core.FolderAuthorityConfig
    WHERE  IsActive       = 1
      AND  DivisionCode   = @DivisionCode
      AND  (PositionName  IS NULL OR PositionName  = '')
      AND  DepartmentCode IS NULL
      AND  (LevelCode     IS NULL OR LevelCode     = '')
    ORDER  BY ConfidentialityLevel;
END
GO

PRINT '✅ sp_ResolveFolderAuthority updated — Priority 0 = PositionName+DepartmentCode';
GO

-- ============================================================
-- ตรวจสอบ: แสดง config ทั้งหมดพร้อม priority ที่จะ match
-- ============================================================
SELECT
    ConfigLabel,
    PositionName,
    LevelCode,
    DepartmentCode,
    DivisionCode,
    CASE
        WHEN PositionName IS NOT NULL AND DepartmentCode IS NOT NULL THEN '0: Position+Dept'
        WHEN PositionName IS NOT NULL AND (DepartmentCode IS NULL OR DepartmentCode='') THEN '1: Position only'
        WHEN LevelCode IS NOT NULL AND DepartmentCode IS NOT NULL THEN '2: Level+Dept'
        WHEN LevelCode IS NOT NULL AND (DepartmentCode IS NULL OR DepartmentCode='') THEN '3: Level only'
        WHEN DepartmentCode IS NOT NULL THEN '4: Dept only'
        ELSE '5: Division'
    END AS MatchPriority,
    ConfidentialityLevel,
    CanView, CanAdd, CanEdit, CanApprove
FROM   core.FolderAuthorityConfig
WHERE  IsActive = 1
ORDER  BY MatchPriority, ConfigLabel, ConfidentialityLevel;
GO
